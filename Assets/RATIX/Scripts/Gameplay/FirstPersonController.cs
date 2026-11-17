using UnityEngine;
using UnityEngine.InputSystem;

namespace RATIX
{
    /// <summary>
    /// Controlador en primera persona para probar los mapas (mover, mirar, saltar,
    /// agacharse y esprintar). La lógica vive aquí; velocidades, salto, cámara y
    /// cursor se ajustan en el Inspector. Usa el mapa "Player" de
    /// <c>InputSystem_Actions</c>.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Camera viewCamera;
        [Tooltip("InputSystem_Actions: se usan Move, Look, Jump, Sprint y Crouch del mapa 'Player'.")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Movimiento")]
        [SerializeField, Min(0f)] private float walkSpeed = 5.5f;
        [SerializeField, Min(0f)] private float sprintSpeed = 8.5f;
        [SerializeField, Min(0f)] private float crouchSpeed = 2.4f;
        [Tooltip("Aceleración en m/s² hacia la velocidad objetivo.")]
        [SerializeField, Min(0.1f)] private float acceleration = 18f;
        [Tooltip("Fracción de aceleración disponible en el aire.")]
        [SerializeField, Range(0f, 1f)] private float airControl = 0.3f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -22f;

        [Header("Alturas")]
        [SerializeField, Min(1f)] private float standHeight = 1.8f;
        [SerializeField, Min(0.6f)] private float crouchHeight = 1.15f;
        [SerializeField, Min(0f)] private float standEye = 1.62f;
        [SerializeField, Min(0f)] private float crouchEye = 1.02f;
        [Tooltip("Velocidad a la que el cuerpo pasa de pie a agachado.")]
        [SerializeField, Min(0.1f)] private float heightBlendSpeed = 9f;

        [Header("Cámara")]
        [Tooltip("Grados de giro por píxel de ratón.")]
        [SerializeField, Min(0.01f)] private float lookSensitivity = 0.16f;
        [SerializeField, Range(10f, 89f)] private float maxPitch = 85f;

        [Header("Cursor")]
        [SerializeField] private bool lockCursorOnStart = true;
        [SerializeField] private Key releaseCursorKey = Key.Escape;

        /// <summary>Momento del último bloqueo de cursor (el arma lo usa para no disparar al reenganchar).</summary>
        public static float LastCursorLockTime { get; private set; } = -999f;

        private CharacterController controller;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction crouchAction;

        private Vector2 moveInput;
        private Vector2 lookInput;
        private float yaw;
        private float pitch;
        private float verticalSpeed;
        private float currentHeight;
        private bool crouching;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (viewCamera == null) viewCamera = GetComponentInChildren<Camera>(true);

            currentHeight = controller.height;
            yaw = transform.eulerAngles.y;

            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Player", false);
                if (map != null)
                {
                    moveAction = map.FindAction("Move", false);
                    lookAction = map.FindAction("Look", false);
                    jumpAction = map.FindAction("Jump", false);
                    sprintAction = map.FindAction("Sprint", false);
                    crouchAction = map.FindAction("Crouch", false);
                }
            }
        }

        private void Start()
        {
            if (lockCursorOnStart) SetCursorLocked(true);
        }

        private void OnEnable()
        {
            if (inputActions != null) inputActions.FindActionMap("Player", false)?.Enable();
        }

        private void OnDisable()
        {
            if (inputActions != null) inputActions.FindActionMap("Player", false)?.Disable();
            SetCursorLocked(false);
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SetCursorLocked(false);
        }

        private void Update()
        {
            ReadInput();
            HandleCursorKey();
            ApplyLook();
            ApplyMove();
        }

        /// <summary>Patada vertical de cámara al disparar.</summary>
        public void AddRecoil(float degrees)
        {
            pitch = Mathf.Clamp(pitch - degrees, -maxPitch, maxPitch);
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
            if (locked) LastCursorLockTime = Time.unscaledTime;
        }

        private void ReadInput()
        {
            if (moveAction != null) moveInput = moveAction.ReadValue<Vector2>();
            if (lookAction != null) lookInput = lookAction.ReadValue<Vector2>();
        }

        private void HandleCursorKey()
        {
            if (Keyboard.current != null && Keyboard.current[releaseCursorKey].wasPressedThisFrame)
                SetCursorLocked(false);

            // Clic para volver a capturar el ratón tras pulsar Escape.
            if (Cursor.lockState != CursorLockMode.Locked &&
                Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                SetCursorLocked(true);
            }
        }

        private void ApplyLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInput = Vector2.zero;
                return;
            }

            yaw += lookInput.x * lookSensitivity;
            pitch = Mathf.Clamp(pitch - lookInput.y * lookSensitivity, -maxPitch, maxPitch);

            // Input System ya entrega delta de ratón: no se multiplica por deltaTime.
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (viewCamera != null) viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void ApplyMove()
        {
            bool grounded = controller.isGrounded;

            if (crouchAction != null && crouchAction.WasPressedThisFrame()) crouching = !crouching;

            bool sprinting = !crouching && sprintAction != null && sprintAction.IsPressed() && moveInput.y > 0.1f;
            float speed = crouching ? crouchSpeed : sprinting ? sprintSpeed : walkSpeed;

            Vector3 wish = transform.right * moveInput.x + transform.forward * moveInput.y;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            Vector3 horizontal = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
            float blend = grounded ? acceleration : acceleration * airControl;
            horizontal = Vector3.MoveTowards(horizontal, wish * speed, blend * Time.deltaTime);

            if (grounded && verticalSpeed < 0f) verticalSpeed = -2f;
            if (grounded && jumpAction != null && jumpAction.WasPressedThisFrame())
                verticalSpeed = Mathf.Sqrt(-2f * gravity * jumpHeight);

            verticalSpeed += gravity * Time.deltaTime;

            ApplyHeight();

            controller.Move((horizontal + Vector3.up * verticalSpeed) * Time.deltaTime);
        }

        private void ApplyHeight()
        {
            float target = crouching ? crouchHeight : standHeight;
            currentHeight = Mathf.MoveTowards(currentHeight, target, heightBlendSpeed * Time.deltaTime);

            if (Mathf.Abs(controller.height - currentHeight) > 0.001f)
            {
                controller.height = currentHeight;
                controller.center = new Vector3(0f, currentHeight * 0.5f, 0f);
            }

            if (viewCamera == null) return;

            float blend = Mathf.InverseLerp(crouchHeight, standHeight, currentHeight);
            var local = viewCamera.transform.localPosition;
            viewCamera.transform.localPosition = new Vector3(local.x, Mathf.Lerp(crouchEye, standEye, blend), local.z);
        }
    }
}
