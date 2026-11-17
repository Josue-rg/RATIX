using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Mini modal de confirmación reutilizable: fondo oscuro y ventana centrada.
    /// Se usa antes de aplicar cambios en Ajustes y en cualquier otra acción que
    /// necesite "¿estás seguro?".
    /// </summary>
    public class ConfirmModal : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Vector2 homePosition = Vector2.zero;

        private Action onConfirm;
        private bool wired;
        private bool shown;

        private void Awake()
        {
            Resolve();
            Wire();
        }

        private void Start()
        {
            // Se queda visible en el editor (estacionado) y se oculta al dar Play.
            if (!shown) gameObject.SetActive(false);
        }

        private void Resolve()
        {
            if (titleLabel == null) titleLabel = FindText("Window/Title");
            if (messageLabel == null) messageLabel = FindText("Window/Message");
            if (cancelButton == null) cancelButton = FindButton("Window/CancelButton");
            if (confirmButton == null) confirmButton = FindButton("Window/ConfirmButton");
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
            if (wired) return;
            wired = true;

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(Cancel);
                cancelButton.onClick.AddListener(Cancel);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(Confirm);
                confirmButton.onClick.AddListener(Confirm);
            }
        }

        /// <summary>Muestra el modal y ejecuta la acción al confirmar.</summary>
        public void Show(string title, string message, Action confirmed)
        {
            Resolve();
            Wire();

            onConfirm = confirmed;
            shown = true;

            if (titleLabel != null) titleLabel.text = title;
            if (messageLabel != null) messageLabel.text = message;

            if (transform is RectTransform rect)
            {
                rect.anchoredPosition = homePosition;
                rect.SetAsLastSibling();
            }

            gameObject.SetActive(true);
        }

        public void Confirm()
        {
            var callback = onConfirm;
            onConfirm = null;
            Close();
            callback?.Invoke();
        }

        public void Cancel()
        {
            onConfirm = null;
            Close();
        }

        public void Close() => gameObject.SetActive(false);
    }
}
