using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Punto de aparición de un mapa. Es un dato de diseño: se coloca en la escena,
    /// se orienta con la flecha azul (Z) y se ajusta en el Inspector. La capa de red
    /// o el flujo de partida lo leen con <see cref="MapDefinition"/>.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        public enum Team
        {
            A = 0,
            B = 1,
            Neutral = 2
        }

        [Header("Aparición")]
        [SerializeField] private Team team = Team.A;
        [SerializeField, Min(0)] private int slot;
        [Tooltip("Si está marcado, es el punto que se usa por defecto al entrar al mapa.")]
        [SerializeField] private bool primary;

        public Team TeamId => team;
        public int Slot => slot;
        public bool IsPrimary => primary;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        private void OnDrawGizmos()
        {
            Color color = team switch
            {
                Team.A => new Color(0.81f, 0.35f, 0.27f, 0.9f),
                Team.B => new Color(0.42f, 0.61f, 0.79f, 0.9f),
                _ => new Color(0.78f, 0.78f, 0.78f, 0.9f)
            };

            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0f, 0.05f, 0f), new Vector3(1.5f, 0.1f, 1.5f));
            Gizmos.DrawRay(new Vector3(0f, 0.1f, 0f), Vector3.forward * 1.8f);

#if UNITY_EDITOR
            if (primary)
            {
                UnityEditor.Handles.color = color;
                UnityEditor.Handles.Label(transform.position + Vector3.up * 2.2f,
                    $"{(team == Team.A ? "A" : team == Team.B ? "B" : "-")}{slot}{(primary ? " ★" : string.Empty)}");
            }
#endif
        }
    }
}
