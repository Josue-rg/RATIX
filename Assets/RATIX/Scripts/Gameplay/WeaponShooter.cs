using UnityEngine;
using UnityEngine.InputSystem;

namespace RATIX
{
    /// <summary>
    /// Arma de prueba del mapa: dispara un rayo desde la cámara, aplica daño a los
    /// <see cref="TargetDummy"/> y dibuja un trazador. Cadencia, daño, alcance,
    /// retroceso y efectos se ajustan en el Inspector. Usa "Attack" del mapa
    /// "Player" de <c>InputSystem_Actions</c>.
    /// </summary>
    public class WeaponShooter : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Camera viewCamera;
        [SerializeField] private InputActionAsset inputActions;
        [Tooltip("Punto de salida del trazador (punta del arma).")]
        [SerializeField] private Transform muzzle;
        [SerializeField] private Light muzzleLight;
        [SerializeField] private LineRenderer tracer;

        [Header("Disparo")]
        [SerializeField, Min(1f)] private float damage = 34f;
        [SerializeField, Min(1f)] private float range = 150f;
        [SerializeField, Min(0.5f)] private float shotsPerSecond = 9f;
        [SerializeField] private bool automatic = true;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField, Min(0f)] private float recoil = 0.45f;

        [Header("Efectos")]
        [SerializeField, Min(0.01f)] private float flashSeconds = 0.05f;
        [SerializeField, Min(0.01f)] private float tracerSeconds = 0.045f;

        private static readonly RaycastHit[] Hits = new RaycastHit[16];

        private InputAction attackAction;
        private FirstPersonController controller;
        private float nextShotTime;
        private float flashTimer;
        private float tracerTimer;

        private void Awake()
        {
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>(true);
            controller = GetComponent<FirstPersonController>();

            if (inputActions == null) return;
            attackAction = inputActions.FindActionMap("Player", false)?.FindAction("Attack", false);
        }

        private void OnEnable()
        {
            attackAction?.Enable();
        }

        private void OnDisable()
        {
            attackAction?.Disable();
            if (muzzleLight != null) muzzleLight.enabled = false;
            if (tracer != null) tracer.enabled = false;
            flashTimer = 0f;
            tracerTimer = 0f;
        }

        private void Update()
        {
            TickEffects();

            if (attackAction == null || Cursor.lockState != CursorLockMode.Locked) return;
            if (Time.unscaledTime - FirstPersonController.LastCursorLockTime < 0.15f) return;

            bool wantsShot = automatic ? attackAction.IsPressed() : attackAction.WasPressedThisFrame();
            if (wantsShot && Time.time >= nextShotTime) Fire();
        }

        private void Fire()
        {
            nextShotTime = Time.time + 1f / shotsPerSecond;

            var cam = viewCamera != null ? viewCamera : Camera.main;
            if (cam == null) return;

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            Vector3 end = ray.origin + ray.direction * range;

            int count = Physics.RaycastNonAlloc(ray, Hits, range, hitMask, QueryTriggerInteraction.Ignore);
            float closest = float.MaxValue;
            bool found = false;
            RaycastHit best = default;

            for (int i = 0; i < count; i++)
            {
                var hit = Hits[i];
                if (hit.collider == null) continue;

                // Nunca acertarse a uno mismo (cápsula del CharacterController).
                var hitTransform = hit.collider.transform;
                if (hitTransform == transform.root || hitTransform.IsChildOf(transform.root)) continue;

                if (hit.distance < closest)
                {
                    closest = hit.distance;
                    best = hit;
                    found = true;
                }
            }

            if (found)
            {
                end = best.point;

                var target = best.collider.GetComponentInParent<TargetDummy>();
                if (target != null) target.TakeHit(damage);

                var body = best.collider.attachedRigidbody;
                if (body != null) body.AddForceAtPosition(ray.direction * 4f, best.point, ForceMode.Impulse);
            }

            PlayEffects(ray, end);
            if (controller != null) controller.AddRecoil(recoil);
        }

        private void PlayEffects(Ray ray, Vector3 end)
        {
            if (tracer != null)
            {
                Vector3 start = muzzle != null
                    ? muzzle.position
                    : ray.origin + ray.direction * 0.5f;

                tracer.SetPosition(0, start);
                tracer.SetPosition(1, end);
                tracer.enabled = true;
                tracerTimer = tracerSeconds;
            }

            if (muzzleLight != null)
            {
                muzzleLight.enabled = true;
                flashTimer = flashSeconds;
            }
        }

        private void TickEffects()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f && muzzleLight != null) muzzleLight.enabled = false;
            }

            if (tracerTimer > 0f)
            {
                tracerTimer -= Time.deltaTime;
                if (tracerTimer <= 0f && tracer != null) tracer.enabled = false;
            }
        }
    }
}
