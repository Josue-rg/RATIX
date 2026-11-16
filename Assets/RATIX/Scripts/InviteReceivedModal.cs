using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Popup de invitación recibida (aparece en la misma posición que el correo).
    /// Lo abre el sistema de red cuando otro jugador te invita y muestra quién
    /// invita, el modo, la sala y el mapa. Permite aceptar o rechazar.
    /// </summary>
    public class InviteReceivedModal : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TMP_Text inviterValue;
        [SerializeField] private TMP_Text modeValue;
        [SerializeField] private TMP_Text roomValue;
        [SerializeField] private TMP_Text mapValue;

        [Header("Botones")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;

        [Header("Posición")]
        [Tooltip("Posición en la vista al abrirse (la misma del correo).")]
        [SerializeField] private Vector2 homePosition = new Vector2(0f, 77f);

        /// <summary>true = aceptar, false = rechazar.</summary>
        public event Action<bool> Answered;
        public event Action Accepted;
        public event Action Declined;

        /// <summary>Última invitación mostrada, por si la red la necesita al responder.</summary>
        public string Inviter { get; private set; }
        public string Mode { get; private set; }
        public string Room { get; private set; }
        public string Map { get; private set; }

        private bool _wired;

        private void Awake()
        {
            Resolve();
            Wire();
        }

        /// <summary>Recupera referencias por ruta para no depender del Inspector.</summary>
        private void Resolve()
        {
            if (inviterValue == null) inviterValue = FindText("Window/FromValue");
            if (modeValue == null) modeValue = FindText("Window/ModeValue");
            if (roomValue == null) roomValue = FindText("Window/RoomValue");
            if (mapValue == null) mapValue = FindText("Window/MapValue");

            if (acceptButton == null) acceptButton = FindButton("Window/Accept");
            if (declineButton == null) declineButton = FindButton("Window/Decline");
        }

        private TMP_Text FindText(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<TMP_Text>() : null;
        }

        private Button FindButton(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private void Wire()
        {
            if (_wired) return;
            _wired = true;

            Resolve();

            if (acceptButton != null) acceptButton.onClick.AddListener(Accept);
            if (declineButton != null) declineButton.onClick.AddListener(Decline);

            var x = transform.Find("Window/XButton");
            if (x != null)
            {
                var b = x.GetComponent<Button>();
                if (b != null) b.onClick.AddListener(Decline);
            }
        }

        /// <summary>Abre el popup con los datos de la invitación.</summary>
        public void Open(string inviter, string mode, string room, string map)
        {
            Resolve();
            Wire();

            Inviter = inviter;
            Mode = mode;
            Room = room;
            Map = map;

            if (inviterValue != null) inviterValue.text = string.IsNullOrEmpty(inviter) ? "Unknown player" : inviter;
            if (modeValue != null) modeValue.text = string.IsNullOrEmpty(mode) ? "-" : mode;
            if (roomValue != null) roomValue.text = string.IsNullOrEmpty(room) ? "-" : room;
            if (mapValue != null) mapValue.text = string.IsNullOrEmpty(map) ? "-" : map;

            if (transform is RectTransform rt)
            {
                rt.anchoredPosition = homePosition;
                rt.SetAsLastSibling();
            }

            gameObject.SetActive(true);
        }

        /// <summary>Abre el popup conservando los textos de ejemplo del editor.</summary>
        public void Open() => Open(Inviter, Mode, Room, Map);

        public void Close() => gameObject.SetActive(false);

        private void Accept()
        {
            Debug.Log($"[RATIX] Invite accepted: {Inviter} · {Mode} · {Room} · {Map}");
            Answered?.Invoke(true);
            Accepted?.Invoke();
            Close();
        }

        private void Decline()
        {
            Debug.Log($"[RATIX] Invite declined: {Inviter}");
            Answered?.Invoke(false);
            Declined?.Invoke();
            Close();
        }
    }
}
