using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Ficha de un mapa: identidad, modo, tamaño del área jugable y puntos de
    /// aparición. Es solo DATA (no ejecuta lógica de partida); todo se ajusta en el
    /// Inspector y la capa de red / el flujo de juego lo leen desde aquí.
    /// </summary>
    public class MapDefinition : MonoBehaviour
    {
        public enum GameMode
        {
            Shooter = 0,
            Zombie = 1,
            Adventure = 2
        }

        [Header("Identidad")]
        [Tooltip("ID estable del mapa (minúsculas, sin espacios).")]
        [SerializeField] private string mapId = "shooter_foundry";
        [Tooltip("Clave de localización del nombre (value.*), como en el carrusel de salas.")]
        [SerializeField] private string nameKey = "value.foundry";
        [Tooltip("Texto de respaldo si la clave no existe en la tabla de idioma.")]
        [SerializeField] private string displayName = "Foundry";

        [Header("Partida")]
        [SerializeField] private GameMode mode = GameMode.Shooter;
        [SerializeField, Min(1)] private int teams = 2;
        [SerializeField, Min(1)] private int recommendedPlayers = 8;
        [SerializeField, Min(10f)] private float roundSeconds = 600f;
        [Tooltip("Escena que se carga para jugar este mapa.")]
        [SerializeField] private string sceneName = "ShooterMap_01";

        [Header("Arena")]
        [Tooltip("Ancho (X), alto (Y) y largo (Z) del área jugable, en metros.")]
        [SerializeField] private Vector3 arenaSize = new Vector3(92f, 7f, 68f);

        [Header("Aparición")]
        [SerializeField] private SpawnPoint[] spawnPoints;

        public string MapId => mapId;
        public string NameKey => nameKey;
        public GameMode Mode => mode;
        public int Teams => teams;
        public int RecommendedPlayers => recommendedPlayers;
        public float RoundSeconds => roundSeconds;
        public string SceneName => sceneName;
        public Vector3 ArenaSize => arenaSize;
        public SpawnPoint[] SpawnPoints => spawnPoints;

        /// <summary>Nombre del mapa en el idioma activo (EN/ES).</summary>
        public string DisplayName => L.Get(nameKey, displayName);

        /// <summary>Punto de aparición concreto; si no existe, el primero libre del equipo.</summary>
        public SpawnPoint GetSpawn(int team, int slot)
        {
            if (spawnPoints == null) return null;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                var point = spawnPoints[i];
                if (point != null && (int)point.TeamId == team && point.Slot == slot) return point;
            }

            return GetPrimarySpawn(team);
        }

        /// <summary>Punto marcado como principal del equipo; si no hay, el primero de ese equipo.</summary>
        public SpawnPoint GetPrimarySpawn(int team)
        {
            if (spawnPoints == null) return null;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                var point = spawnPoints[i];
                if (point != null && (int)point.TeamId == team && point.IsPrimary) return point;
            }

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                var point = spawnPoints[i];
                if (point != null && (int)point.TeamId == team) return point;
            }

            return null;
        }
    }
}
