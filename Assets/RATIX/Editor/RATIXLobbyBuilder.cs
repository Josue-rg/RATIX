using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace RATIX
{
    /// <summary>
    /// LEGACY builder: recreates the first lobby prototype. It does NOT match the
    /// current scene and overwrites Lobby.unity, so it is kept only for reference.
    /// The lobby is now edited by hand in the editor.
    /// </summary>
    public static class RATIXLobbyBuilder
    {
        private const string ScenePath = "Assets/RATIX/Scenes/Lobby.unity";
        private static readonly Vector2 ViewPosition = new Vector2(470f, 64f);
        private const float PanelW = 1440f;
        private const float PanelH = 944f;
        private const float Spread = 1600f;

        private static readonly Color C_BG       = new Color(0.059f, 0.090f, 0.165f);
        private static readonly Color C_CARD     = new Color(0.098f, 0.129f, 0.204f);
        private static readonly Color C_CARD_ALT = new Color(0.118f, 0.161f, 0.231f);
        private static readonly Color C_CARD_HI  = new Color(0.145f, 0.192f, 0.275f);
        private static readonly Color C_TEXT     = new Color(0.973f, 0.980f, 0.988f);
        private static readonly Color C_MUTED    = new Color(0.580f, 0.639f, 0.722f);
        private static readonly Color C_GREEN    = new Color(0.082f, 0.502f, 0.239f);
        private static readonly Color C_BLUE     = new Color(0.145f, 0.388f, 0.922f);
        private static readonly Color C_AMBER    = new Color(0.851f, 0.467f, 0.024f);
        private static readonly Color C_NEUTRAL  = new Color(0.200f, 0.255f, 0.333f);

        private static TMP_FontAsset _font;
        private static Sprite _round;
        private static RectTransform _canvas;

        [MenuItem("Tools/RATIX/Legacy/Rebuild lobby (overwrites scene)")]
        public static void BuildLobbyMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "RATIX - Legacy builder",
                    "This rebuilds Assets/RATIX/Scenes/Lobby.unity with the old prototype layout and OVERWRITES the current scene.\n\nContinue?",
                    "Rebuild", "Cancel"))
                return;

            Build();
        }

        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = C_BG;

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            var canvasGO = new GameObject("LobbyCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            _canvas = canvasGO.GetComponent<RectTransform>();

            _font = TMP_Settings.defaultFontAsset;
            if (_font == null)
                _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            _round = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // Fondo.
            var bg = UI("Background", _canvas);
            Stretch(bg.GetComponent<RectTransform>());
            bg.AddComponent<Image>().color = C_BG;

            // Shell.
            var topBarRefs = BuildTopBar();
            var leftRefs = BuildLeftColumn();
            BuildNews();

            // Paneles de modo (dispersos).
            var select = BuildModeSelectPanel(new Vector2(ViewPosition.x + 0f * Spread, ViewPosition.y));
            var zombi = BuildRoomListPanel(new Vector2(ViewPosition.x + 1f * Spread, ViewPosition.y), "ZOMBI", C_GREEN, "Último zombi", "Zombi normal", "Evolución");
            var shooter = BuildRoomListPanel(new Vector2(ViewPosition.x + 2f * Spread, ViewPosition.y), "SHOOTER", C_BLUE, "Francotirador", "Asalto", "Apoyo");
            var aventura = BuildAventuraPanel(new Vector2(ViewPosition.x + 3f * Spread, ViewPosition.y));

            // Overlays desplegables (armas + chat).
            var weapons = BuildWeaponsOverlay();
            var chat = BuildChatOverlay();

            // Controlador.
            var controller = canvasGO.AddComponent<LobbyController>();
            var so = new SerializedObject(controller);

            var panelsProp = so.FindProperty("modePanels");
            panelsProp.arraySize = 4;
            panelsProp.GetArrayElementAtIndex(0).objectReferenceValue = select.panel;
            panelsProp.GetArrayElementAtIndex(1).objectReferenceValue = zombi.panel;
            panelsProp.GetArrayElementAtIndex(2).objectReferenceValue = shooter.panel;
            panelsProp.GetArrayElementAtIndex(3).objectReferenceValue = aventura.panel;
            so.FindProperty("viewPosition").vector2Value = ViewPosition;

            so.FindProperty("zombiButton").objectReferenceValue = select.zombiButton;
            so.FindProperty("shooterButton").objectReferenceValue = select.shooterButton;
            so.FindProperty("aventuraButton").objectReferenceValue = select.aventuraButton;
            so.FindProperty("zombiBackButton").objectReferenceValue = zombi.backButton;
            so.FindProperty("shooterBackButton").objectReferenceValue = shooter.backButton;
            so.FindProperty("aventuraBackButton").objectReferenceValue = aventura.backButton;

            so.FindProperty("weaponsButton").objectReferenceValue = weapons.toggle;
            so.FindProperty("chatButton").objectReferenceValue = chat.toggle;
            so.FindProperty("weaponsOverlay").objectReferenceValue = weapons.overlay;
            so.FindProperty("chatOverlay").objectReferenceValue = chat.overlay;

            so.FindProperty("clanButton").objectReferenceValue = topBarRefs.buttons[0];
            so.FindProperty("clasificacionButton").objectReferenceValue = topBarRefs.buttons[1];
            so.FindProperty("eventosButton").objectReferenceValue = topBarRefs.buttons[2];
            so.FindProperty("almacenButton").objectReferenceValue = topBarRefs.buttons[3];
            so.FindProperty("tiendaButton").objectReferenceValue = topBarRefs.buttons[4];
            so.FindProperty("ajustesButton").objectReferenceValue = leftRefs.buttons[0];
            so.ApplyModifiedPropertiesWithoutUndo();

            var sceneDir = System.IO.Path.GetDirectoryName(ScenePath);
            if (!System.IO.Directory.Exists(sceneDir))
                System.IO.Directory.CreateDirectory(sceneDir);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                Debug.LogError("[RATIX] No se pudo guardar la escena en " + ScenePath);

            var buildScenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path != ScenePath) buildScenes.Add(s);
            EditorBuildSettings.scenes = buildScenes.ToArray();

            Debug.Log("[RATIX] Lobby construido en " + ScenePath);
        }

        // ==== Shell ====

        private static LobbyRefs BuildTopBar()
        {
            var bar = UI("TopBar", _canvas);
            var rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 72f);
            rt.anchoredPosition = Vector2.zero;
            bar.AddComponent<Image>().color = C_CARD;
            var prt = bar.GetComponent<RectTransform>();

            var logo = UI("Logo", prt);
            Place(logo.GetComponent<RectTransform>(), 28f, 0f, 240f, 72f);
            Label("Label", logo.GetComponent<RectTransform>(), "RATIX", 34f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
            var accent = UI("Accent", prt);
            Place(accent.GetComponent<RectTransform>(), 22f, 20f, 6f, 32f);
            accent.AddComponent<Image>().color = C_GREEN;

            var refs = new LobbyRefs();
            string[] items = { "Clan", "Clasificación", "Eventos", "Almacén", "Tienda" };
            for (int i = 0; i < items.Length; i++)
            {
                var b = UI(items[i], prt);
                float w = 172f, gap = 8f;
                float x = 1440f - 28f - (items.Length - i) * (w + gap);
                Place(b.GetComponent<RectTransform>(), x, 12f, w, 48f);
                MakeButton(b, items[i], C_CARD_ALT, 22f);
                SetTextColor(b, C_TEXT);
                refs.buttons.Add(b.GetComponent<Button>());
            }
            return refs;
        }

        private static LobbyRefs BuildLeftColumn()
        {
            var col = UI("LeftColumn", _canvas);
            var rt = col.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(0f, 64f);
            rt.offsetMax = new Vector2(460f, -72f);
            col.AddComponent<Image>().color = C_CARD;
            var prt = col.GetComponent<RectTransform>();
            var refs = new LobbyRefs();

            var card = UI("PlayerCard", prt);
            Place(card.GetComponent<RectTransform>(), 20f, 816f, 420f, 116f);
            card.AddComponent<Image>().color = C_CARD_ALT;
            var crt = card.GetComponent<RectTransform>();
            var avatar = UI("Avatar", crt);
            Place(avatar.GetComponent<RectTransform>(), 16f, 26f, 64f, 64f);
            var av = avatar.AddComponent<Image>();
            av.sprite = _round;
            av.color = C_GREEN;
            Label("Initial", avatar.GetComponent<RectTransform>(), "J", 26f, Color.white, FontStyles.Bold);
            Label("Name", crt, "Nombre", 30f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(crt.Find("Name").GetComponent<RectTransform>(), 96f, 60f, 220f, 36f);
            Label("Level", crt, "Nivel 0", 20f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(crt.Find("Level").GetComponent<RectTransform>(), 96f, 28f, 160f, 26f);
            var vip = UI("VIP", crt);
            Place(vip.GetComponent<RectTransform>(), 308f, 34f, 88f, 48f);
            MakeButton(vip, "VIP", C_AMBER, 20f);

            var charPanel = UI("CharacterPanel", prt);
            Place(charPanel.GetComponent<RectTransform>(), 20f, 300f, 420f, 480f);
            charPanel.AddComponent<Image>().color = C_CARD_ALT;
            var cprt = charPanel.GetComponent<RectTransform>();
            Label("CharacterTitle", cprt, "PERSONAJE", 22f, C_MUTED, FontStyles.Bold);
            Place(cprt.Find("CharacterTitle").GetComponent<RectTransform>(), 0f, 428f, 420f, 30f);
            var head = UI("Head", cprt);
            Place(head.GetComponent<RectTransform>(), 150f, 330f, 120f, 120f);
            var h = head.AddComponent<Image>();
            h.sprite = _round;
            h.color = C_MUTED;
            var body = UI("Body", cprt);
            Place(body.GetComponent<RectTransform>(), 170f, 140f, 80f, 200f);
            var bImg = body.AddComponent<Image>();
            bImg.sprite = _round;
            bImg.color = C_MUTED;
            var persBtn = UI("Personajes", cprt);
            Place(persBtn.GetComponent<RectTransform>(), 90f, 24f, 240f, 56f);
            MakeButton(persBtn, "Cambiar personaje", C_NEUTRAL, 22f);

            var nav = UI("BottomNav", prt);
            Place(nav.GetComponent<RectTransform>(), 20f, 20f, 420f, 56f);
            var nprt = nav.GetComponent<RectTransform>();
            string[] navItems = { "Agenda", "Correo", "Ajustes" };
            for (int i = 0; i < navItems.Length; i++)
            {
                var n = UI(navItems[i], nprt);
                float w = 132f;
                Place(n.GetComponent<RectTransform>(), i * (w + 12f), 0f, w, 56f);
                MakeButton(n, navItems[i], C_CARD_HI, 22f);
                SetTextColor(n, C_TEXT);
                if (navItems[i] == "Ajustes") refs.buttons.Add(n.GetComponent<Button>());
            }

            return refs;
        }

        private static void BuildNews()
        {
            var news = UI("NewsTicker", _canvas);
            var rt = news.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(0f, 64f);
            news.AddComponent<Image>().color = C_CARD;
            var l = Label("Label", news.GetComponent<RectTransform>(),
                "Noticias  ·  Jugador tal obtuvo tal cosa", 24f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            l.rectTransform.anchorMin = new Vector2(0f, 0f);
            l.rectTransform.anchorMax = new Vector2(1f, 1f);
            l.rectTransform.offsetMin = new Vector2(28f, 0f);
            l.rectTransform.offsetMax = new Vector2(-28f, 0f);
        }

        // ==== Paneles de modo (dispersos) ====

        private static LobbyRefs BuildModeSelectPanel(Vector2 pos)
        {
            var p = Panel("ModeSelectPanel", pos);
            var prt = p.GetComponent<RectTransform>();
            var refs = new LobbyRefs { panel = prt };

            var title = Label("Title", prt, "SELECCIONA UN MODO", 40f, C_TEXT, FontStyles.Bold);
            Place(title.rectTransform, 0f, 700f, 1440f, 54f);
            var subtitle = Label("Subtitle", prt, "Elige cómo quieres jugar", 24f, C_MUTED);
            Place(subtitle.rectTransform, 0f, 660f, 1440f, 34f);

            var zombi = UI("ZombiButton", prt);
            Place(zombi.GetComponent<RectTransform>(), 360f, 430f, 720f, 96f);
            refs.zombiButton = ModeCard(zombi, "ZOMBI", "PvP asimétrico · Infecta o sobrevive", C_GREEN);

            var shooter = UI("ShooterButton", prt);
            Place(shooter.GetComponent<RectTransform>(), 360f, 314f, 720f, 96f);
            refs.shooterButton = ModeCard(shooter, "SHOOTER", "PvP competitivo · Combate por equipos", C_BLUE);

            var aventura = UI("AventuraButton", prt);
            Place(aventura.GetComponent<RectTransform>(), 360f, 198f, 720f, 96f);
            refs.aventuraButton = ModeCard(aventura, "AVENTURA", "PvE cooperativo · Descubre el origen", C_AMBER);

            return refs;
        }

        private static Button ModeCard(GameObject go, string title, string desc, Color accent)
        {
            var img = go.AddComponent<Image>();
            img.sprite = _round;
            img.color = C_CARD_ALT;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(C_CARD_HI, Color.white, 0.05f);
            colors.pressedColor = C_CARD_HI;
            colors.selectedColor = colors.highlightedColor;
            colors.colorMultiplier = 1f;
            btn.colors = colors;

            var prt = go.GetComponent<RectTransform>();
            var bar = UI("Bar", prt);
            Place(bar.GetComponent<RectTransform>(), 0f, 0f, 10f, 96f);
            bar.AddComponent<Image>().color = accent;

            var t = Label("Title", prt, title, 32f, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(t.rectTransform, 40f, 50f, 400f, 38f);
            var d = Label("Desc", prt, desc, 20f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(d.rectTransform, 40f, 18f, 620f, 26f);

            return btn;
        }

        private static LobbyRefs BuildRoomListPanel(Vector2 pos, string modeTitle, Color accent,
            string roleA, string roleB, string roleC)
        {
            var p = Panel(modeTitle + "Panel", pos);
            var prt = p.GetComponent<RectTransform>();
            var refs = new LobbyRefs { panel = prt };

            var header = UI("Header", prt);
            Place(header.GetComponent<RectTransform>(), 0f, 880f, 1440f, 64f);
            header.AddComponent<Image>().color = accent;
            Label("Label", header.GetComponent<RectTransform>(), modeTitle, 32f, Color.white, FontStyles.Bold);

            var back = UI("Back", prt);
            Place(back.GetComponent<RectTransform>(), 16f, 884f, 150f, 56f);
            refs.backButton = MakeButton(back, "Volver", C_NEUTRAL, 22f);

            var money = Label("Money", prt, "Dinero  0      Oro  0", 24f, C_TEXT, FontStyles.Normal, TextAlignmentOptions.Right);
            Place(money.rectTransform, 1040f, 890f, 370f, 44f);

            string[] tabs = { "Global", "Servidor", "Básico" };
            var tabsRow = UI("Tabs", prt);
            Place(tabsRow.GetComponent<RectTransform>(), 20f, 816f, 1400f, 52f);
            var trt = tabsRow.GetComponent<RectTransform>();
            for (int i = 0; i < tabs.Length; i++)
            {
                var t = UI("Tab" + i, trt);
                Place(t.GetComponent<RectTransform>(), i * 220f, 0f, 210f, 52f);
                MakeButton(t, tabs[i], (i == 0 ? accent : C_CARD_ALT), 24f);
                SetTextColor(t, C_TEXT);
            }

            var list = UI("RoomList", prt);
            Place(list.GetComponent<RectTransform>(), 20f, 20f, 900f, 780f);
            list.AddComponent<Image>().color = C_CARD;
            var lrt = list.GetComponent<RectTransform>();

            var head = UI("ListHeader", lrt);
            Place(head.GetComponent<RectTransform>(), 24f, 728f, 852f, 40f);
            Label("Salas", head.GetComponent<RectTransform>(), "SALAS", 22f, C_MUTED, FontStyles.Bold, TextAlignmentOptions.Left);
            Label("Jugadores", head.GetComponent<RectTransform>(), "JUGADORES", 22f, C_MUTED, FontStyles.Bold, TextAlignmentOptions.Right);

            for (int i = 0; i < 7; i++)
            {
                var row = UI("Room" + i, lrt);
                Place(row.GetComponent<RectTransform>(), 16f, 660f - i * 72f, 868f, 64f);
                row.AddComponent<Image>().color = (i % 2 == 0 ? C_CARD_ALT : C_CARD_HI);
                Label("Name", row.GetComponent<RectTransform>(), "Sala " + (i + 1), 24f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
                var n = row.GetComponent<RectTransform>().Find("Name").GetComponent<RectTransform>();
                n.offsetMin = new Vector2(20f, 0f);
                n.offsetMax = new Vector2(-140f, 0f);
                Label("Count", row.GetComponent<RectTransform>(), "4 / 15", 24f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Right);
                var c = row.GetComponent<RectTransform>().Find("Count").GetComponent<RectTransform>();
                c.offsetMin = new Vector2(0f, 0f);
                c.offsetMax = new Vector2(-20f, 0f);
            }

            var match = UI("Matchmaking", prt);
            Place(match.GetComponent<RectTransform>(), 940f, 20f, 480f, 780f);
            match.AddComponent<Image>().color = C_CARD;
            var mrt = match.GetComponent<RectTransform>();

            Label("Label", mrt, "EMPAREJAMIENTO", 24f, C_TEXT, FontStyles.Bold);
            Place(mrt.Find("Label").GetComponent<RectTransform>(), 0f, 728f, 480f, 40f);

            var slots = UI("Slots", mrt);
            Place(slots.GetComponent<RectTransform>(), 24f, 320f, 432f, 330f);
            slots.AddComponent<Image>().color = C_CARD_ALT;
            var srt = slots.GetComponent<RectTransform>();
            string[] roles = { roleA, roleB, roleC };
            for (int i = 0; i < 3; i++)
            {
                var slot = UI("Slot" + i, srt);
                Place(slot.GetComponent<RectTransform>(), 26f + i * 132f, 190f, 104f, 104f);
                var img = slot.AddComponent<Image>();
                img.sprite = _round;
                img.color = C_CARD_HI;
                Label("Role", slot.GetComponent<RectTransform>(), roles[i], 16f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Center);
                Place(slot.GetComponent<RectTransform>().Find("Role").GetComponent<RectTransform>(), -30f, -36f, 164f, 26f);
            }

            var join = UI("Unirse", mrt);
            Place(join.GetComponent<RectTransform>(), 24f, 180f, 432f, 72f);
            MakeButton(join, "UNIRSE", accent, 28f);

            return refs;
        }

        private static LobbyRefs BuildAventuraPanel(Vector2 pos)
        {
            var p = Panel("AventuraPanel", pos);
            var prt = p.GetComponent<RectTransform>();
            var refs = new LobbyRefs { panel = prt };

            var header = UI("Header", prt);
            Place(header.GetComponent<RectTransform>(), 0f, 880f, 1440f, 64f);
            header.AddComponent<Image>().color = C_AMBER;
            Label("Label", header.GetComponent<RectTransform>(), "AVENTURA", 32f, Color.white, FontStyles.Bold);

            var back = UI("Back", prt);
            Place(back.GetComponent<RectTransform>(), 16f, 884f, 150f, 56f);
            refs.backButton = MakeButton(back, "Volver", C_NEUTRAL, 22f);

            var money = Label("Money", prt, "Monedas  0      Oro  0", 24f, C_TEXT, FontStyles.Normal, TextAlignmentOptions.Right);
            Place(money.rectTransform, 1040f, 890f, 370f, 44f);

            string[] tabs = { "Base", "Campo", "Aventura" };
            var tabsRow = UI("Tabs", prt);
            Place(tabsRow.GetComponent<RectTransform>(), 20f, 816f, 1400f, 52f);
            var trt = tabsRow.GetComponent<RectTransform>();
            for (int i = 0; i < tabs.Length; i++)
            {
                var t = UI("Tab" + i, trt);
                Place(t.GetComponent<RectTransform>(), i * 220f, 0f, 210f, 52f);
                MakeButton(t, tabs[i], (i == 2 ? C_AMBER : C_CARD_ALT), 24f);
                SetTextColor(t, C_TEXT);
            }

            var area = UI("Content", prt);
            Place(area.GetComponent<RectTransform>(), 20f, 20f, 1400f, 780f);
            area.AddComponent<Image>().color = C_CARD;
            Label("Hint", area.GetComponent<RectTransform>(),
                "Mapas y progreso próximamente", 30f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Center);

            return refs;
        }

        // ==== Overlays desplegables ====

        private static OverlayRefs BuildWeaponsOverlay()
        {
            var toggle = UI("ArmasButton", _canvas);
            Place(toggle.GetComponent<RectTransform>(), 470f, 64f, 160f, 48f);
            var toggleBtn = MakeButton(toggle, "ARMAS  ▾", C_NEUTRAL, 20f);

            var overlay = UI("WeaponsOverlay", _canvas);
            Place(overlay.GetComponent<RectTransform>(), 470f, 112f, 1440f, 216f);
            overlay.AddComponent<Image>().color = C_CARD;
            var prt = overlay.GetComponent<RectTransform>();
            string[] row1 = { "Arma Principal", "Arma Secundaria" };
            string[] row2 = { "Arma blanca", "Arma explosiva" };
            for (int i = 0; i < 2; i++)
            {
                var s1 = UI(row1[i], prt);
                Place(s1.GetComponent<RectTransform>(), 16f + i * 712f, 116f, 696f, 84f);
                s1.AddComponent<Image>().color = C_CARD_ALT;
                Label("Label", s1.GetComponent<RectTransform>(), row1[i], 26f, C_TEXT);

                var s2 = UI(row2[i], prt);
                Place(s2.GetComponent<RectTransform>(), 16f + i * 712f, 16f, 696f, 84f);
                s2.AddComponent<Image>().color = C_CARD_ALT;
                Label("Label", s2.GetComponent<RectTransform>(), row2[i], 26f, C_MUTED);
            }
            overlay.SetActive(false);

            return new OverlayRefs { toggle = toggleBtn, overlay = overlay };
        }

        private static OverlayRefs BuildChatOverlay()
        {
            var toggle = UI("ChatButton", _canvas);
            Place(toggle.GetComponent<RectTransform>(), 1750f, 64f, 160f, 48f);
            var toggleBtn = MakeButton(toggle, "CHAT  ▾", C_NEUTRAL, 20f);

            var overlay = UI("ChatOverlay", _canvas);
            Place(overlay.GetComponent<RectTransform>(), 1420f, 112f, 490f, 520f);
            overlay.AddComponent<Image>().color = C_CARD;
            var prt = overlay.GetComponent<RectTransform>();
            var head = UI("Header", prt);
            Place(head.GetComponent<RectTransform>(), 0f, 468f, 490f, 52f);
            head.AddComponent<Image>().color = C_CARD_ALT;
            Label("Label", head.GetComponent<RectTransform>(), "CHAT GLOBAL", 22f, C_TEXT, FontStyles.Bold);
            var body = UI("Body", prt);
            Place(body.GetComponent<RectTransform>(), 0f, 56f, 490f, 412f);
            body.AddComponent<Image>().color = C_CARD_ALT;
            Label("Hint", body.GetComponent<RectTransform>(),
                "Mensajes del chat próximamente", 20f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Center);
            overlay.SetActive(false);

            return new OverlayRefs { toggle = toggleBtn, overlay = overlay };
        }

        // ==== Helpers ====

        private static GameObject Panel(string name, Vector2 pos)
        {
            var go = UI(name, _canvas);
            Place(go.GetComponent<RectTransform>(), pos.x, pos.y, PanelW, PanelH);
            go.AddComponent<Image>().color = C_BG;
            return go;
        }

        private static GameObject UI(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            return go;
        }

        private static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI Label(string name, RectTransform parent, string text, float size, Color color,
            FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var go = UI(name, parent);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.font = _font;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = align;
            t.raycastTarget = false;
            Stretch(t.rectTransform);
            return t;
        }

        private static Button MakeButton(GameObject go, string label, Color color, float fontSize)
        {
            var img = go.AddComponent<Image>();
            img.sprite = _round;
            img.color = color;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.14f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.4f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            Label("Label", go.GetComponent<RectTransform>(), label, fontSize, Color.white, FontStyles.Bold);
            return btn;
        }

        private static void SetTextColor(GameObject button, Color color)
        {
            var t = button.transform.Find("Label");
            if (t != null)
            {
                var tmp = t.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.color = color;
            }
        }
    }

    internal sealed class LobbyRefs
    {
        public RectTransform panel;
        public Button zombiButton;
        public Button shooterButton;
        public Button aventuraButton;
        public Button backButton;
        public List<Button> buttons = new List<Button>();
    }

    internal sealed class OverlayRefs
    {
        public Button toggle;
        public GameObject overlay;
    }
}
