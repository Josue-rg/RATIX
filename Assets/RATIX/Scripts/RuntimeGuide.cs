using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// Marcador de posición visible SOLO en el editor. Sirve para ver dónde se
    /// coloca una interfaz al entrar en Play (los paneles se editan estacionados
    /// fuera de la vista). Al ejecutar el juego se desactiva solo.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class RuntimeGuide : MonoBehaviour
    {
        private void OnEnable()
        {
            if (Application.isPlaying) gameObject.SetActive(false);
        }
    }
}
