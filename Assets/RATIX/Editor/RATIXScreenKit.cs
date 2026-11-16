using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX.EditorTools
{
    /// <summary>
    /// Atajos para crear interfaces nuevas del lobby sin repetir el trabajo manual.
    ///
    /// Solo CREA objetos nuevos: nunca modifica ni repinta las interfaces que ya
    /// existen. Respeta la Regla 5 del README (interfaces estacionadas fuera del
    /// lobby), la paleta neutra con acento y los sprites redondeados en modo
    /// Sliced (que es lo que evita las esquinas deformes).
    ///
    /// Menú: RATIX ▸ UI.
    /// </summary>
    public static class RATIXScreenKit
    {
        private const string CanvasName = "LobbyCanvas";
        private const string UIFolder = "Assets/RATIX/UI/";

        // ==== Tokens (mismos valores que usa el lobby) ====
        private static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.72f);
        private static readonly Color WindowBg = Hex("0F1929");
        private static readonly Color Card = Hex("111B2B");
        private static readonly Color CardAlt = Hex("162236");
        private static readonly Color Text = Hex("F2F6FC");
        private static readonly Color Muted = Hex("94A3B8");
        private static readonly Color Accent = Hex("5C8FA8");
        private static readonly Color AccentDeep = Hex("2F5A73");
        private static readonly Color OnAccent = Hex("EAF4F8");
        private static readonly Color HeaderBg = Hex("19242B");
        private static readonly Color BackBg = Hex("1F2C33");

        // Paso de la rejilla de parking (deja aire entre pantallas).
        private static readonly float[] ModalColumns = { -2037f, 0f, 2006f };
        private static readonly float[] ModalRows = { 0f, 1120f, 2200f, 3320f, 4440f, 5560f };
        private static readonly float[] PanelColumns = { 1546f, 3557f, 5565f };
        private static readonly float[] PanelRows = { 0f, 1076f, -1074f, -2135f, -3208f, -4284f };

        // ==== Menú ====

        [MenuItem("RATIX/UI/Crear modal vacío", false, 10)]
        public static void CreateEmptyModal()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;

            var slot = NextFreeSlot(canvas, new Vector2(1920f, 1080f), ModalColumns, ModalRows);
            var root = new GameObject("NewModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rootRect = (RectTransform)root.transform;
            rootRect.SetParent(canvas, false);
            ApplyParking(rootRect, slot, new Vector2(1920f, 1080f));

            var backdrop = root.GetComponent<Image>();
            backdrop.color = Backdrop;
            backdrop.raycastTarget = true;

            var window = NewRect(root.transform, "Window", 900f, 620f);
            window.anchorMin = window.anchorMax = new Vector2(0.5f, 0.5f);
            window.pivot = new Vector2(0.5f, 0.5f);
            window.anchoredPosition = Vector2.zero;
            var windowImage = window.gameObject.AddComponent<Image>();
            windowImage.color = WindowBg;
            windowImage.sprite = LoadSprite("ui_round_lg");
            windowImage.type = Image.Type.Sliced;
            windowImage.pixelsPerUnitMultiplier = 1f;

            AddText(window, "Title", "NEW MODAL", 24f, 556f, 500f, 40f, 30, Text, TextAlignmentOptions.Left, true);
            AddButton(window, "XButton", "X", 836f, 556f, 40f, 40f, CardAlt, Text, 18);

            var body = NewRect(window, "Body", 852f, 480f);
            body.anchoredPosition = new Vector2(24f, 56f);
            var bodyImage = body.gameObject.AddComponent<Image>();
            bodyImage.color = Card;
            bodyImage.sprite = LoadSprite("ui_round_lg");
            bodyImage.type = Image.Type.Sliced;
            bodyImage.pixelsPerUnitMultiplier = 1f;
            AddText(body, "Hint", "Contenido aquí", 0f, 0f, 852f, 480f, 14, Muted, TextAlignmentOptions.Center, false);

            Finish(root, $"modal en {slot.x}, {slot.y}");
        }

        [MenuItem("RATIX/UI/Crear panel de contenido vacío", false, 11)]
        public static void CreateEmptyContentPanel()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;

            var content = canvas.Find("Content") as RectTransform;
            if (content == null)
            {
                EditorUtility.DisplayDialog("RATIX", "No encontré LobbyCanvas/Content.", "Ok");
                return;
            }

            var slot = NextFreeSlot(content, new Vector2(1460f, 1008f), PanelColumns, PanelRows);
            var root = new GameObject("NewPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rootRect = (RectTransform)root.transform;
            rootRect.SetParent(content, false);
            ApplyParking(rootRect, slot, new Vector2(1460f, 1008f));
            root.GetComponent<Image>().color = Hex("0A0E13");

            var header = NewRect(root.transform, "Header", 1460f, 64f);
            header.anchoredPosition = new Vector2(0f, 944f);
            var headerImage = header.gameObject.AddComponent<Image>();
            headerImage.color = HeaderBg;

            AddButton(header, "Back", "Back", 0f, 0f, 150f, 64f, BackBg, Text, 22);
            AddText(header, "Label", "NEW PANEL", 0f, 0f, 1460f, 64f, 32, Accent, TextAlignmentOptions.Center, true);

            var body = NewRect(root.transform, "Body", 1428f, 912f);
            body.anchoredPosition = new Vector2(16f, 16f);
            var bodyImage = body.gameObject.AddComponent<Image>();
            bodyImage.color = Card;
            bodyImage.sprite = LoadSprite("ui_round_lg");
            bodyImage.type = Image.Type.Sliced;
            bodyImage.pixelsPerUnitMultiplier = 1f;
            AddText(body, "Hint", "Contenido aquí", 0f, 0f, 1428f, 912f, 14, Muted, TextAlignmentOptions.Center, false);

            Finish(root, $"panel en {slot.x}, {slot.y}");
        }

        [MenuItem("RATIX/UI/Duplicar fila seleccionada", false, 12)]
        public static void DuplicateRow()
        {
            var selected = Selection.activeTransform;
            if (selected == null || selected.parent == null)
            {
                EditorUtility.DisplayDialog("RATIX", "Selecciona primero la fila que quieres duplicar.", "Ok");
                return;
            }

            var copy = Object.Instantiate(selected.gameObject, selected.parent);
            copy.name = NextName(selected.name);
            var rect = (RectTransform)copy.transform;
            float spacing = rect.sizeDelta.y + 8f;
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - spacing);

            Undo.RegisterCreatedObjectUndo(copy, "Duplicar fila");
            EditorSceneDirty();
            Selection.activeGameObject = copy;
        }

        [MenuItem("RATIX/UI/Ver siguiente hueco (modal)", false, 20)]
        public static void ShowNextModalSlot()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;
            var slot = NextFreeSlot(canvas, new Vector2(1920f, 1080f), ModalColumns, ModalRows);
            EditorUtility.DisplayDialog("RATIX", $"Siguiente hueco libre para un modal de 1920 x 1080:\n\nX = {slot.x}\nY = {slot.y}", "Ok");
        }

        [MenuItem("RATIX/UI/Ver siguiente hueco (panel)", false, 21)]
        public static void ShowNextPanelSlot()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;
            var content = canvas.Find("Content") as RectTransform;
            if (content == null) return;
            var slot = NextFreeSlot(content, new Vector2(1460f, 1008f), PanelColumns, PanelRows);
            EditorUtility.DisplayDialog("RATIX", $"Siguiente hueco libre para un panel de 1460 x 1008:\n\nX = {slot.x}\nY = {slot.y}", "Ok");
        }

        [MenuItem("RATIX/UI/Ver invitación recibida", false, 22)]
        public static void FrameInviteReceived()
        {
            var canvas = FindCanvas();
            if (canvas == null) return;

            var target = canvas.Find("InviteReceivedModal");
            if (target == null)
            {
                EditorUtility.DisplayDialog("RATIX", "No encuentro LobbyCanvas/InviteReceivedModal en la escena.", "Ok");
                return;
            }

            Selection.activeGameObject = target.gameObject;
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.in2DMode = true;
                sceneView.FrameSelected();
                sceneView.Repaint();
            }
        }

        // ==== Construcción ====

        private static RectTransform NewRect(Transform parent, string name, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static TMP_Text AddText(Transform parent, string name, string content, float x, float y,
            float width, float height, float size, Color color, TextAlignmentOptions alignment, bool bold)
        {
            var rect = NewRect(parent, name, width, height);
            rect.anchoredPosition = new Vector2(x, y);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = LoadFont();
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject AddButton(Transform parent, string name, string label, float x, float y,
            float width, float height, Color background, Color labelColor, float size)
        {
            var rect = NewRect(parent, name, width, height);
            rect.anchoredPosition = new Vector2(x, y);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = background;
            image.sprite = LoadSprite("ui_round_sm");
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
            image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            AddText(rect, "Label", label, 0f, 0f, width, height, size, labelColor, TextAlignmentOptions.Center, true);
            return rect.gameObject;
        }

        // ==== Parking ====

        private static void ApplyParking(RectTransform rect, Vector2 slot, Vector2 size)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = size;
            rect.anchoredPosition = slot;
        }

        /// <summary>Busca la primera coordenada libre que no pise ninguna interfaz existente.</summary>
        private static Vector2 NextFreeSlot(RectTransform parent, Vector2 size, float[] columns, float[] rows)
        {
            var taken = new System.Collections.Generic.List<Rect>();
            var origin = new Vector2(parent.rect.width * parent.pivot.x, parent.rect.height * parent.pivot.y);

            foreach (Transform child in parent)
            {
                if (!(child is RectTransform childRect)) continue;

                // Rect del hijo expresado en el mismo espacio que el parking:
                // (0,0) = esquina inferior izquierda del padre.
                var corners = new Vector3[4];
                childRect.GetWorldCorners(corners);
                Vector3 a = parent.InverseTransformPoint(corners[0]) + (Vector3)origin;
                Vector3 b = parent.InverseTransformPoint(corners[2]) + (Vector3)origin;
                var rect = Rect.MinMaxRect(a.x, a.y, b.x, b.y);

                // Solo cuentan las interfaces grandes: los adornos pequeños no bloquean huecos.
                if (rect.width < size.x * 0.5f || rect.height < size.y * 0.5f) continue;
                taken.Add(rect);
            }

            foreach (var row in rows)
            {
                foreach (var column in columns)
                {
                    var candidate = new Rect(column, row, size.x, size.y);
                    bool overlaps = taken.Any(r => candidate.Overlaps(r));
                    if (!overlaps) return new Vector2(column, row);
                }
            }

            Debug.LogWarning("[RATIX] No quedan huecos en la rejilla; se usará la esquina por defecto.");
            return new Vector2(0f, 0f);
        }

        // ==== Utilidades ====

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString("#" + hex, out Color color) ? color : Color.magenta;
        }

        private static RectTransform FindCanvas()
        {
            var canvas = GameObject.Find(CanvasName);
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("RATIX", $"Abre la escena del lobby (no encontré {CanvasName}).", "Ok");
                return null;
            }

            return canvas.transform as RectTransform;
        }

        private static Sprite LoadSprite(string file)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UIFolder + file + ".png");
            if (sprite == null) Debug.LogWarning($"[RATIX] Falta el sprite {UIFolder}{file}.png");
            return sprite;
        }

        private static TMP_FontAsset LoadFont()
        {
            var reference = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)
                .FirstOrDefault(t => t.font != null);
            return reference != null ? reference.font : TMP_Settings.defaultFontAsset;
        }

        /// <summary>Renombra "Room3" a "Room4" y "ItemCard0" a "ItemCard1".</summary>
        private static string NextName(string name)
        {
            int index = name.Length;
            while (index > 0 && char.IsDigit(name[index - 1])) index--;
            if (index == name.Length) return name + " Copy";

            string prefix = name.Substring(0, index);
            string digits = name.Substring(index);
            return int.TryParse(digits, out int number)
                ? prefix + (number + 1).ToString(digits)
                : name + " Copy";
        }

        private static void Finish(GameObject root, string what)
        {
            Undo.RegisterCreatedObjectUndo(root, "Crear interfaz");
            EditorSceneDirty();
            Selection.activeGameObject = root;
            Debug.Log($"[RATIX] Interfaz creada ({what}).");
        }

        private static void EditorSceneDirty()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}
