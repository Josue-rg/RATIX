using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Pantalla de presentación que se muestra justo después del logo de Unity.
    /// Avanza una barra de carga y al terminar avisa a AppFlowController.
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private TMP_Text statusLabel;
        [Tooltip("Ancho máximo de la barra de carga en píxeles.")]
        [SerializeField] private float barWidth = 460f;
        [Tooltip("Duración del splash en segundos.")]
        [SerializeField] private float duration = 3.5f;
        [Tooltip("Posición en la vista al mostrarse (se edita estacionado).")]
        [SerializeField] private Vector2 homePosition = Vector2.zero;

        public event Action Finished;

        private Coroutine routine;

        /// <summary>Muestra la pantalla desde el principio.</summary>
        public void Play()
        {
            if (transform is RectTransform rt)
            {
                rt.anchoredPosition = homePosition;
                rt.SetAsLastSibling();
            }

            gameObject.SetActive(true);

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run());
        }

        public void Hide() => gameObject.SetActive(false);

        /// <summary>Ancho de la barra según el progreso (0..1).</summary>
        private void Fill(float progress)
        {
            var rect = progressFill.rectTransform;
            rect.sizeDelta = new Vector2(barWidth * Mathf.Clamp01(progress), rect.sizeDelta.y);
        }

        private void OnDisable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        private IEnumerator Run()
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                if (progressFill != null) Fill(progress);
                if (statusLabel != null)
                    statusLabel.text = L.Format("splash.loading", Mathf.RoundToInt(progress * 100f));

                yield return null;
            }

            if (progressFill != null) Fill(1f);
            if (statusLabel != null) statusLabel.text = L.Get("splash.ready", "READY");

            routine = null;
            Finished?.Invoke();
        }
    }
}
