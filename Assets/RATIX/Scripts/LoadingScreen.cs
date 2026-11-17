using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Pantalla de carga de partida: cubre el lobby, muestra el mapa elegido y
    /// espera a que la escena esté lista (con un mínimo de tiempo para que no
    /// parpadee). Después activa la escena del mapa.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private TMP_Text mapName;
        [SerializeField] private Image progressFill;

        [Header("Tiempos")]
        [Tooltip("Tiempo mínimo en pantalla, aunque la escena cargue antes.")]
        [SerializeField] private float minimumSeconds = 1.4f;
        [SerializeField] private float holdAtEnd = 0.12f;

        /// <summary>Muestra la pantalla y carga la escena indicada.</summary>
        public void Show(string displayName, string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("[RATIX] LoadingScreen sin escena.");
                return;
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            // Por si la raíz está estacionada en el editor: se trae a la vista.
            if (transform is RectTransform rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(1920f, 1080f);
            }

            if (mapName != null) mapName.text = displayName;
            if (progressFill != null) progressFill.fillAmount = 0f;

            StartCoroutine(Load(sceneName));
        }

        private IEnumerator Load(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                Debug.LogError("[RATIX] No puedo cargar la escena '" + sceneName + "'. ¿Está en Build Settings?");
                yield break;
            }

            operation.allowSceneActivation = false;

            float elapsed = 0f;
            while (operation.progress < 0.9f || elapsed < minimumSeconds)
            {
                elapsed += Time.unscaledDeltaTime;

                float byLoad = operation.progress / 0.9f;
                float byTime = elapsed / minimumSeconds;
                if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(Mathf.Min(byLoad, byTime));

                yield return null;
            }

            if (progressFill != null) progressFill.fillAmount = 1f;
            yield return new WaitForSecondsRealtime(holdAtEnd);

            operation.allowSceneActivation = true;
        }
    }
}
