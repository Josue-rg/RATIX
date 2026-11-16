using UnityEditor;
using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Helper for the modals. They are authored in a row above the mode panels and
    /// stay enabled so they can be edited directly in the Scene view. These commands
    /// just bring one of them to the center for a closer look, or put them back.
    /// </summary>
    public static class RATIXModalEditor
    {
        private const string ClanModal = "ClanModal";
        private const string LeaderboardModal = "LeaderboardModal";
        private const string EventsModal = "EventsModal";

        private static readonly Vector2 ClanHome = new Vector2(0f, 2200f);
        private static readonly Vector2 LeaderboardHome = new Vector2(0f, 1120f);
        private static readonly Vector2 EventsHome = new Vector2(-4028f, 0f);

        [MenuItem("Tools/RATIX/UI/Edit Modal/Clan")]
        public static void EditClan() => Show(ClanModal);

        [MenuItem("Tools/RATIX/UI/Edit Modal/Leaderboard")]
        public static void EditLeaderboard() => Show(LeaderboardModal);

        [MenuItem("Tools/RATIX/UI/Edit Modal/Events")]
        public static void EditEvents() => Show(EventsModal);

        [MenuItem("Tools/RATIX/UI/Edit Modal/Reset positions")]
        public static void RestoreAll()
        {
            var canvas = GameObject.Find("LobbyCanvas");
            if (canvas == null) return;
            Place(canvas.transform, ClanModal, ClanHome);
            Place(canvas.transform, LeaderboardModal, LeaderboardHome);
            Place(canvas.transform, EventsModal, EventsHome);
            Debug.Log("[RATIX] Modals placed back in their row.");
        }

        private static void Show(string modalName)
        {
            var canvas = GameObject.Find("LobbyCanvas");
            if (canvas == null) return;

            RestoreAll();

            var modal = canvas.transform.Find(modalName) as RectTransform;
            if (modal == null) return;
            modal.anchoredPosition = Vector2.zero;
            modal.gameObject.SetActive(true);
            modal.SetAsLastSibling();

            Selection.activeGameObject = modal.gameObject;
            EditorGUIUtility.PingObject(modal.gameObject);
            Debug.Log($"[RATIX] Editing '{modalName}' in the center. Use Reset positions when done.");
        }

        private static void Place(Transform canvas, string name, Vector2 home)
        {
            var modal = canvas.Find(name) as RectTransform;
            if (modal == null) return;
            modal.anchoredPosition = home;
            modal.gameObject.SetActive(true);
        }
    }
}
