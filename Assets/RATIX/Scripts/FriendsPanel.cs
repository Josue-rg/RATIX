using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Friends popup: player list on the left, chat on the right.
    /// Clicking a row fills the chat header with that player's name and status.
    /// The data is authored in the scene; the panel only swaps it.
    /// </summary>
    public class FriendsPanel : MonoBehaviour
    {
        [SerializeField] private GameObject emptyState;
        [SerializeField] private GameObject chatState;
        [SerializeField] private TMP_Text chatName;
        [SerializeField] private TMP_Text chatStatus;
        [SerializeField] private TMP_Text chatRoom;

        [Tooltip("Posición donde el chat se coloca al abrirse (dentro del panel derecho).")]
        [SerializeField] private Vector2 chatHome = new Vector2(484f, 24f);

        private bool _wired;
        private RectTransform _chatRect;

        private void Awake()
        {
            Resolve();
            Wire();
        }

        /// <summary>Recupera las referencias por ruta para no depender de valores del Inspector.</summary>
        private void Resolve()
        {
            if (emptyState == null)
            {
                var t = transform.Find("Window/Empty");
                if (t != null) emptyState = t.gameObject;
            }

            if (chatState == null)
            {
                var t = transform.Find("Window/Chat");
                if (t != null) chatState = t.gameObject;
            }

            if (chatName == null) chatName = FindText("Window/Chat/Header/Name");
            if (chatStatus == null) chatStatus = FindText("Window/Chat/Header/Status");
            if (chatRoom == null) chatRoom = FindText("Window/Chat/Header/RoomCard/Title");
            if (_chatRect == null && chatState != null) _chatRect = chatState.transform as RectTransform;
        }

        private TMP_Text FindText(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<TMP_Text>() : null;
        }

        private void OnEnable()
        {
            if (emptyState != null) emptyState.SetActive(true);
            if (chatState != null) chatState.SetActive(false);
        }

        private void Wire()
        {
            if (_wired) return;
            _wired = true;

            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                if (!button.name.StartsWith("FriendRow")) continue;
                var captured = button;
                button.onClick.AddListener(() => Select(captured));
            }
        }

        public void Select(Button row)
        {
            if (emptyState != null) emptyState.SetActive(false);
            if (chatState != null) chatState.SetActive(true);

            // El chat se edita estacionado fuera del modal; al abrirse vuelve a su sitio.
            if (_chatRect != null) _chatRect.anchoredPosition = chatHome;

            if (chatName != null) chatName.text = Read(row, "Name", "Player");
            if (chatStatus != null) chatStatus.text = Read(row, "Status", "Offline");
            if (chatRoom != null) chatRoom.text = Read(row, "Room", "Room 1");
        }

        private static string Read(Button row, string child, string fallback)
        {
            var target = row.transform.Find(child);
            var text = target != null ? target.GetComponent<TMP_Text>() : null;
            return text != null && !string.IsNullOrEmpty(text.text) ? text.text : fallback;
        }
    }
}
