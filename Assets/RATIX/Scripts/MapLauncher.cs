using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Lanza la partida con el mapa elegido en un carrusel. Va en el panel que
    /// tiene el carrusel de mapas (crear sala o ajustes de la sala); el botón de
    /// crear/entrar solo llama a <see cref="Launch"/>.
    /// </summary>
    public class MapLauncher : MonoBehaviour
    {
        [Header("Selección")]
        [SerializeField] private Carousel3D carousel;
        [Tooltip("Escenas en el mismo orden que las cartas del carrusel.")]
        [SerializeField] private string[] sceneNames;
        [Tooltip("Clave de idioma de cada mapa (value.*).")]
        [SerializeField] private string[] nameKeys;

        [Header("Pantalla")]
        [SerializeField] private LoadingScreen loadingScreen;

        public void Launch()
        {
            if (sceneNames == null || sceneNames.Length == 0)
            {
                Debug.LogWarning("[RATIX] MapLauncher sin escenas.");
                return;
            }

            int index = carousel != null ? Mathf.Clamp(carousel.SelectedIndex, 0, sceneNames.Length - 1) : 0;
            string scene = sceneNames[index];

            string name = carousel != null ? carousel.SelectedName : string.Empty;
            string key = nameKeys != null && index < nameKeys.Length && !string.IsNullOrEmpty(nameKeys[index])
                ? nameKeys[index]
                : Carousel3D.ValueKey(name);

            var screen = loadingScreen != null
                ? loadingScreen
                : Object.FindAnyObjectByType<LoadingScreen>(FindObjectsInactive.Include);

            if (screen == null)
            {
                Debug.LogWarning("[RATIX] No encuentro la pantalla de carga.");
                return;
            }

            screen.Show(L.Get(key, name), scene);
        }
    }
}
