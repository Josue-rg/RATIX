using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Visual polish pass over the lobby scene.
    /// Only changes STYLE (color, sprite, corners, typography, hover states).
    /// Never touches positions, sizes, names or hierarchy.
    /// </summary>
    public static class RATIXUITheme
    {
        // ==== Paleta =========================================================
        private static readonly Color C_TEXT     = Hex("F2F6FC");
        private static readonly Color C_MUTED    = Hex("93A4BC");
        private static readonly Color C_DIM      = Hex("6B7C96");

        private static readonly Color C_SURFACE  = Hex("0B1320");
        private static readonly Color C_SURFACE2 = Hex("0E1826");
        private static readonly Color C_CARD     = Hex("111B2B");
        private static readonly Color C_CARD_ALT = Hex("162236");
        private static readonly Color C_CHIP     = Hex("1A2840");
        private static readonly Color C_INPUT    = Hex("101B2C");
        private static readonly Color C_TOP      = Hex("0A111D");
        private static readonly Color C_LEFT     = Hex("08101C");
        private static readonly Color C_GHOST    = Hex("152136");
        private static readonly Color C_HEADER   = Hex("101C2E");

        private static readonly Color C_GREEN    = Hex("22C55E");
        private static readonly Color C_BLUE     = Hex("3B82F6");
        private static readonly Color C_AMBER    = Hex("F59E0B");
        private static readonly Color C_GOLD     = Hex("FBBF24");
        private static readonly Color C_RED      = Hex("F87171");
        private static readonly Color C_BLUE_TXT = Hex("60A5FA");

        private const string UIDir = "Assets/RATIX/UI";
        private static Sprite _roundSm;
        private static Sprite _roundLg;
        private static Sprite _circle;
        private static Sprite _bgGradient;

        // ==== Entradas de menÃº ==============================================

        [MenuItem("Tools/RATIX/UI/1. Crear sprites del tema")]
        public static void CreateSpritesMenu()
        {
            EnsureSprites(force: true);
            Debug.Log("[RATIX] Sprites del tema creados en " + UIDir);
        }

        [MenuItem("Tools/RATIX/UI/2. Aplicar pulido al lobby")]
        public static void ApplyMenu() => Apply();

        // ==== AplicaciÃ³n ====================================================

        public static void Apply()
        {
            var canvas = GameObject.Find("LobbyCanvas");
            if (canvas == null)
            {
                Debug.LogError("[RATIX] No se encontrÃ³ LobbyCanvas.");
                return;
            }

            EnsureSprites(false);

            var unhandled = new List<string>();

            // 1) Backgrounds and surfaces.
            foreach (var img in canvas.GetComponentsInChildren<Image>(true))
                Paint(img, unhandled);

            // 2) TipografÃ­a.
            foreach (var t in canvas.GetComponentsInChildren<TMP_Text>(true))
                PaintText(t);

            // 3) Estados de interacciÃ³n.
            foreach (var b in canvas.GetComponentsInChildren<Button>(true))
                NormalizeButton(b);

            if (unhandled.Count > 0)
                Debug.Log("[RATIX] ImÃ¡genes sin regla (se dejaron con su color): " + string.Join(", ", unhandled));

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("[RATIX] Pulido visual aplicado y escena guardada.");
        }

        // ==== Image rules ===================================================

        private static void Paint(Image img, List<string> unhandled)
        {
            if (img == null) return;
            string n = img.name;
            string path = PathOf(img.transform);

            // Full-screen gradient background.
            if (path == "Background")
            {
                img.sprite = _bgGradient;
                img.type = Image.Type.Simple;
                img.color = Color.white;
                return;
            }

            // Shell.
            if (path == "TopBar") { Flat(img, C_TOP); return; }
            if (path == "TopBar/Logo") { Flat(img, new Color(1, 1, 1, 0)); return; }
            if (path.StartsWith("TopBar/") && img.GetComponent<Button>() != null) { Round(img, C_GHOST, false); return; }
            if (path == "LeftColumn") { Flat(img, C_LEFT); return; }

            // Player card.
            if (n == "PlayerCard") { Round(img, C_CARD, true); return; }
            if (n == "CharacterPanel") { Round(img, C_SURFACE2, true); return; }
            if (n == "Figure") { Round(img, Hex("0D1828"), false); return; }
            if (n == "Head")
            {
                if (_circle != null)
                {
                    img.sprite = _circle;
                    img.type = Image.Type.Simple;
                    img.preserveAspect = true;
                }
                img.color = Hex("22344C");
                img.SetAllDirty();
                return;
            }
            if (n == "Body")
            {
                img.sprite = _roundLg;
                img.type = Image.Type.Sliced;
                img.color = Hex("22344C");
                img.pixelsPerUnitMultiplier = 1f;
                img.SetAllDirty();
                return;
            }
            if (path.Contains("/BottomNav/") && img.GetComponent<Button>() != null) { Round(img, C_GHOST, false); return; }
            if (n == "CharacterButton") { Round(img, C_CARD_ALT, false); return; }

            // Mode / hall panels.
            if (n == "ModeSelectPanel" || n == "ZombieModePanel" || n == "ShooterModePanel" ||
                n == "AdventureModePanel" || n == "ZombieHallPanel" || n == "ShooterHallPanel")
            { Flat(img, C_SURFACE); return; }

            // Large mode cards.
            if (n == "ZombieButton" || n == "ShooterButton" || n == "AdventureButton")
            {
                Color accent = n == "ZombieButton" ? C_GREEN : n == "ShooterButton" ? C_BLUE : C_AMBER;
                Round(img, Color.Lerp(C_SURFACE, accent, 0.10f), true);
                return;
            }

            if (n == "Bar")
            {
                Flat(img, AccentOf(img.transform));
                return;
            }

            if (n == "Header") { Flat(img, C_HEADER); return; }
            if (n == "Back") { Round(img, C_CARD_ALT, false); return; }
            if (n.StartsWith("Tab")) { Round(img, IsActiveTab(img.transform) ? AccentOf(img.transform) : C_CARD_ALT, false); return; }

            // Lists and columns.
            if (n == "RoomList") { Round(img, C_SURFACE2, true); return; }
            if (n == "Matchmaking" || n == "CreateRoomPanel" || n == "SettingsPanel") { Round(img, C_SURFACE2, true); return; }
            if (n == "WeaponsBar") { Round(img, C_CARD, true); return; }
            if (n == "Search") { Round(img, C_CARD_ALT, false); return; }

            if (n.StartsWith("Room") || n.StartsWith("Rank") || n.StartsWith("Clan") && char.IsDigit(n[n.Length - 1]) ||
                n.StartsWith("Member"))
            {
                Round(img, IndexParity(n) ? C_CARD : C_CARD_ALT, false);
                return;
            }

            if (n == "SearchInput" || n.EndsWith("_in")) { Round(img, C_INPUT, false); return; }

            // Matchmaking inner blocks.
            if (n == "Carousel" || n == "Description" || n == "AccessoriesPanel") { Round(img, C_CARD, false); return; }
            if (n.StartsWith("Item")) { Round(img, C_CHIP, false); return; }
            if (n == "photo") { Flat(img, Hex("1E2E46")); return; }
            if (n == "Join") { Round(img, AccentOf(img.transform), false); return; }
            if (n == "Content") { Round(img, C_CARD, true); return; }

            // Hall loadout slots.
            if (n.Contains("Weapon") || n == "Pet") { Round(img, C_CARD_ALT, false); return; }

            // Modals.
            if (n == "ClanModal" || n == "LeaderboardModal" || n == "EventsModal")
            { Flat(img, new Color(0f, 0f, 0f, 0.72f)); return; }
            if (n == "Window") { Round(img, Hex("0F1929"), true); return; }
            if (n == "XButton") { Round(img, C_CARD_ALT, true); return; }
            if (n == "Sections" || n == "Details") { Round(img, C_CARD, true); return; }
            if (n == "CreateClanButton" || n == "CreateButton") { Round(img, AccentOf(img.transform), false); return; }
            if (n == "BackButton") { Round(img, C_CARD_ALT, false); return; }
            if (n == "WelcomeSection") { Round(img, C_GREEN, false); return; }
            if (n == "GiftSection") { Round(img, C_AMBER, false); return; }

            // Ãšltimo recurso: neutralizar colores muy saturados (placeholders).
            if (img.sprite == null && Saturation(img.color) > 0.25f)
            {
                Round(img, C_CARD_ALT, false);
                unhandled.Add(path);
            }
        }

        // ==== Text rules ====================================================

        private static void PaintText(TMP_Text t)
        {
            if (t == null) return;
            string n = t.name;
            string path = PathOf(t.transform);

            Color c = t.color;

            // Normalize greys and whites inherited from different clones.
            if (IsNearWhite(c)) c = C_TEXT;
            else if (IsGrey(c)) c = C_MUTED;

            // Mode headers: label uses the mode accent.
            if (path.EndsWith("/Header/Label")) c = AccentOf(t.transform);

            if (n == "VipLabel" || n == "Vip") c = C_GOLD;
            if (n == "TeamRed") c = C_RED;
            if (n == "Team Blue") c = C_BLUE_TXT;
            if (n == "Pos") c = path.Contains("/Rank0/") || path.Contains("/Rank1/") || path.Contains("/Rank2/") ? C_GOLD : C_MUTED;
            if (n == "QuickStartCheck" || n == "ObserverCheck") c = C_GREEN;
            if (n == "Money" || n == "Gold") c = Hex("C9D6E8");
            if (n == "Placeholder" || n == "SearchPlaceholder" || n == "Password...") c = C_DIM;
            if (n == "Clan" && path.Contains("PlayerCard")) c = C_MUTED;

            // TÃ­tulos: blanco pleno y un poco de tracking.
            bool bigTitle = path.EndsWith("ModeSelectPanel/Title") || path.EndsWith("/Window/Title") ||
                            (n == "Title" && path.Contains("Button/"));
            if (bigTitle) c = C_TEXT;

            t.color = c;

            // Tracking (solo en tÃ­tulos, no cambia posiciÃ³n ni tamaÃ±o).
            if (path.EndsWith("ModeSelectPanel/Title")) t.characterSpacing = 6f;
            else if (path.EndsWith("/Window/Title")) t.characterSpacing = 3f;
            else if (path.EndsWith("/Header/Label")) t.characterSpacing = 3f;
            else if (n == "Title" && path.Contains("Button/")) t.characterSpacing = 2f;
            else if (n == "Label" && path.Contains("/TopBar/")) t.characterSpacing = 0.5f;

            // Etiquetas de pestaÃ±as: activa blanca, inactivas atenuadas.
            if (path.Contains("/Tabs/") && t.transform.parent != null)
                t.color = IsActiveTab(t.transform.parent) ? Color.white : C_MUTED;
        }

        // ==== Buttons =======================================================

        private static void NormalizeButton(Button b)
        {
            if (b == null) return;
            var c = b.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1.14f, 1.14f, 1.14f, 1f);
            c.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            c.selectedColor = Color.white;
            c.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            c.fadeDuration = 0.08f;
            c.colorMultiplier = 1f;
            b.colors = c;
            b.transition = Selectable.Transition.ColorTint;
        }

        // ==== Helpers =======================================================

        private static void Flat(Image img, Color c)
        {
            img.sprite = null;
            img.type = Image.Type.Simple;
            img.color = c;
            img.SetAllDirty();
        }

        private static void Round(Image img, Color c, bool big)
        {
            var sprite = big ? _roundLg : _roundSm;
            if (sprite == null) { Flat(img, c); return; }
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = c;
            img.pixelsPerUnitMultiplier = 1f;
            img.SetAllDirty();

            if (big && img.GetComponent<Outline>() == null)
            {
                var rim = img.gameObject.AddComponent<Outline>();
                rim.effectColor = new Color(1f, 1f, 1f, 0.055f);
                rim.effectDistance = new Vector2(1f, 1f);
                rim.useGraphicAlpha = true;
            }
        }

        private static Color AccentOf(Transform t)
        {
            Transform p = t;
            while (p != null)
            {
                string n = p.name;
                if (n.Contains("Zombie")) return C_GREEN;
                if (n.Contains("Shooter")) return C_BLUE;
                if (n.Contains("Adventure")) return C_AMBER;
                p = p.parent;
            }
            return C_GREEN;
        }

        private static bool IsActiveTab(Transform t)
        {
            // Zombie/Shooter: first tab. Adventure: third tab.
            bool adventure = PathOf(t).Contains("Adventure");
            if (adventure) return t.name == "Tab2";
            return t.name == "Tab0";
        }

        private static bool IndexParity(string n)
        {
            int idx = 0;
            for (int i = n.Length - 1; i >= 0; i--)
            {
                if (char.IsDigit(n[i])) { idx = n[i] - '0'; break; }
            }
            return idx % 2 == 0;
        }

        private static string PathOf(Transform t)
        {
            string s = t.name;
            Transform p = t.parent;
            while (p != null && p.name != "LobbyCanvas")
            {
                s = p.name + "/" + s;
                p = p.parent;
            }
            return s;
        }

        private static bool IsNearWhite(Color c) => c.r > 0.92f && c.g > 0.92f && c.b > 0.92f;
        private static bool IsGrey(Color c)
        {
            float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            float min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            return max - min < 0.10f && c.r > 0.40f && c.r < 0.85f;
        }

        private static float Saturation(Color c)
        {
            float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b));
            float min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            return max <= 0f ? 0f : (max - min) / max;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        // ==== GeneraciÃ³n de sprites ========================================

        private static void EnsureSprites(bool force)
        {
            if (!AssetDatabase.IsValidFolder(UIDir))
                AssetDatabase.CreateFolder("Assets/RATIX", "UI");

            _roundSm = LoadOrCreate(UIDir + "/ui_round_sm.png", 48, 8, 10, force);
            _roundLg = LoadOrCreate(UIDir + "/ui_round_lg.png", 96, 18, 22, force);
            _circle = LoadOrCreateCircle(UIDir + "/ui_circle.png", force);
            _bgGradient = LoadOrCreateGradient(UIDir + "/ui_bg_gradient.png", force);
        }

        private static Sprite LoadOrCreate(string path, int size, int radius, int border, bool force)
        {
            if (!force && File.Exists(path))
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);

            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            float half = size * 0.5f;
            float pad = 1.5f;
            float b = half - pad - radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px0 = x + 0.5f - half;
                    float py0 = y + 0.5f - half;
                    float qx = Mathf.Abs(px0) - b;
                    float qy = Mathf.Abs(py0) - b;
                    float d = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f))
                              + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    float a = Mathf.Clamp01(0.5f - d);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spriteBorder = new Vector4(border, border, border, border);
            ti.spritePixelsPerUnit = 100f;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize = 256;
            ti.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadOrCreateCircle(string path, bool force)
        {
            if (!force && File.Exists(path))
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);

            const int size = 128;
            const float radius = 60f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - half;
                    float dy = y + 0.5f - half;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                    float a = Mathf.Clamp01(0.5f - d);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spriteBorder = Vector4.zero;
            ti.spritePixelsPerUnit = 100f;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize = 256;
            ti.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadOrCreateGradient(string path, bool force)
        {
            if (!force && File.Exists(path))
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);

            const int w = 8, h = 256;
            Color top = Hex("0E1728");
            Color bottom = Hex("060A11");
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float t = y / (h - 1f); // 0 abajo, 1 arriba
                Color c = Color.Lerp(bottom, top, Mathf.SmoothStep(0f, 1f, t));
                for (int x = 0; x < w; x++)
                    px[y * w + x] = c;
            }
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spriteBorder = Vector4.zero;
            ti.spritePixelsPerUnit = 100f;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize = 512;
            ti.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
