using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Blanco de práctica: se ilumina al recibir un impacto y, si se queda sin vida,
    /// cae y vuelve a levantarse a los pocos segundos. Sin UI ni texto: todo el
    /// ajuste (vida, color de golpe, tiempo de reaparición) está en el Inspector.
    /// </summary>
    public class TargetDummy : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField, Min(1f)] private float health = 100f;

        [Header("Visual")]
        [Tooltip("Renderer de la chapa que cambia de color al recibir un impacto.")]
        [SerializeField] private Renderer plate;
        [SerializeField] private Material idleMaterial;
        [SerializeField] private Material hitMaterial;
        [SerializeField, Min(0.01f)] private float flashSeconds = 0.12f;

        [Header("Caída")]
        [Tooltip("Pivote que gira cuando el blanco se queda sin vida.")]
        [SerializeField] private Transform fallPivot;
        [SerializeField] private bool fallWhenDown = true;
        [SerializeField, Min(0.5f)] private float respawnSeconds = 4f;

        private float healthLeft;
        private float flashTimer;
        private float respawnTimer;
        private bool down;

        private void Awake()
        {
            healthLeft = health;
        }

        private void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f && plate != null) plate.sharedMaterial = idleMaterial;
            }

            if (!down) return;

            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f) StandUp();
        }

        /// <summary>Recibe daño; lo llama el arma al acertar.</summary>
        public void TakeHit(float amount)
        {
            if (down) return;

            healthLeft -= Mathf.Max(0f, amount);

            if (plate != null && hitMaterial != null)
            {
                plate.sharedMaterial = hitMaterial;
                flashTimer = flashSeconds;
            }

            if (healthLeft > 0f) return;

            down = true;
            respawnTimer = respawnSeconds;
            if (fallWhenDown && fallPivot != null)
                fallPivot.localRotation = Quaternion.Euler(-82f, 0f, 0f);
        }

        private void StandUp()
        {
            down = false;
            healthLeft = health;
            if (fallPivot != null) fallPivot.localRotation = Quaternion.identity;
            if (plate != null) plate.sharedMaterial = idleMaterial;
        }
    }
}
