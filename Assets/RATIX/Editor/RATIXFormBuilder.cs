using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Builds the real form controls (inputs, dropdowns, toggles), the room search
    /// field, the hall action buttons and the Kick/Invite modals.
    /// It only edits the open Lobby scene and saves it.
    /// </summary>
    public static class RATIXFormBuilder
    {
        private const string UIDir = "Assets/RATIX/UI";

        private static readonly Color C_TEXT = Hex("F2F6FC");
        private static readonly Color C_MUTED = Hex("93A4BC");
        private static readonly Color C_DIM = Hex("6B7C96");
        private static readonly Color C_SURFACE = Hex("0B1320");
        private static readonly Color C_SURFACE2 = Hex("0E1826");
        private static readonly Color C_CARD = Hex("111B2B");
        private static readonly Color C_CARD_ALT = Hex("162236");
        private static readonly Color C_INPUT = Hex("101B2C");
        private static readonly Color C_GHOST = Hex("152136");
        private static readonly Color C_BUTTON = Hex("243349");
        private static readonly Color C_GREEN = Hex("22C55E");
        private static readonly Color C_BLUE = Hex("3B82F6");
        private static readonly Color C_AMBER = Hex("F59E0B");
        private static readonly Color C_GOLD = Hex("FBBF24");

        private static Sprite _roundSm;
        private static Sprite _roundLg;
        private static Sprite _circle;
        private static Sprite _ring;
        private static TMP_FontAsset _font;

        // ==== Menu ==========================================================

        [MenuItem("Tools/RATIX/UI/4. Settings forms")]
        public static void BuildForms()
        {
            LoadAssets();
            var canvas = Canvas();
            var forms = new[]
            {
                ("Content/ZombieModePanel/CreateRoomPanel/Settings", C_GREEN),
                ("Content/ShooterModePanel/CreateRoomPanel/Settings", C_BLUE),
                ("Content/ZombieHallPanel/SettingsPanel/Settings", C_GREEN),
                ("Content/ShooterHallPanel/SettingsPanel/Settings", C_BLUE),
            };
            foreach (var (path, accent) in forms)
            {
                var panel = canvas.Find(path) as RectTransform;
                if (panel == null) { Debug.LogError("[RATIX] Missing " + path); continue; }
                BuildForm(panel, accent);
            }
            Save();
            Debug.Log("[RATIX] Settings forms built.");
        }

        [MenuItem("Tools/RATIX/UI/5. Room search")]
        public static void BuildSearch()
        {
            LoadAssets();
            var canvas = Canvas();
            foreach (var path in new[]
            {
                "Content/ZombieModePanel/RoomList",
                "Content/ShooterModePanel/RoomList",
            })
            {
                var list = canvas.Find(path) as RectTransform;
                if (list == null) { Debug.LogError("[RATIX] Missing " + path); continue; }
                BuildRoomSearch(list);
            }
            Save();
            Debug.Log("[RATIX] Room search built.");
        }

        [MenuItem("Tools/RATIX/UI/6. Hall actions and modals")]
        public static void BuildHallActions()
        {
            LoadAssets();
            var canvas = Canvas();

            BuildHallBar(canvas.Find("Content/ZombieHallPanel") as RectTransform, C_GREEN);
            BuildHallBar(canvas.Find("Content/ShooterHallPanel") as RectTransform, C_BLUE);

            var kick = BuildActionModal(canvas, "KickModal", new Vector2(0f, 3400f),
                "KICK PLAYER", "RED TEAM", "BLUE TEAM",
                new[] { "Player 1", "Player 2", "Player 3", "Player 4", "Player 5", "Player 6", "Player 7" },
                new[] { "Player 8", "Player 9", "Player 10", "Player 11", "Player 12", "Player 13", "Player 14" },
                "KICK", C_GREEN);

            var invite = BuildActionModal(canvas, "InviteModal", new Vector2(1940f, 3400f),
                "INVITE PLAYER", "FRIENDS", "CLAN FRIENDS",
                new[] { "Friend 1", "Friend 2", "Friend 3", "Friend 4", "Friend 5", "Friend 6", "Friend 7" },
                new[] { "Clanmate 1", "Clanmate 2", "Clanmate 3", "Clanmate 4", "Clanmate 5", "Clanmate 6", "Clanmate 7" },
                "INVITE", C_BLUE);

            // Wire the lobby controller to the new modals (fallback by name also exists).
            var lc = canvas.GetComponent<LobbyController>();
            var so = new SerializedObject(lc);
            var kp = so.FindProperty("kickModal");
            var ip = so.FindProperty("inviteModal");
            if (kp != null) kp.objectReferenceValue = kick;
            if (ip != null) ip.objectReferenceValue = invite;
            so.ApplyModifiedPropertiesWithoutUndo();

            Save();
            Debug.Log("[RATIX] Hall actions and Kick/Invite modals built.");
        }

        [MenuItem("Tools/RATIX/UI/7. Improve carousels")]
        public static void ImproveCarousels()
        {
            LoadAssets();
            var canvas = Canvas();
            int cards = 0;

            foreach (var car in canvas.GetComponentsInChildren<Carousel3D>(true))
            {
                Color accent = AccentOf(car.transform);
                foreach (Transform card in car.transform)
                {
                    if (!card.name.StartsWith("Card")) continue;

                    var img = card.GetComponent<Image>();
                    if (img == null) img = card.gameObject.AddComponent<Image>();
                    img.sprite = _roundLg;
                    img.type = Image.Type.Sliced;
                    img.color = C_CARD;
                    img.raycastTarget = true;

                    // The card owns its click button; otherwise Carousel3D would add a
                    // transparent image on top and hide this frame.
                    var cardButton = card.GetComponent<Button>();
                    if (cardButton == null)
                    {
                        cardButton = card.gameObject.AddComponent<Button>();
                        cardButton.targetGraphic = img;
                        cardButton.transition = Selectable.Transition.None;
                    }

                    var mask = card.GetComponent<Mask>();
                    if (mask == null) mask = card.gameObject.AddComponent<Mask>();
                    mask.showMaskGraphic = true;

                    var photo = card.Find("photo") as RectTransform;
                    if (photo != null)
                    {
                        photo.anchorMin = Vector2.zero;
                        photo.anchorMax = Vector2.one;
                        photo.offsetMin = Vector2.zero;
                        photo.offsetMax = Vector2.zero;
                        var pi = photo.GetComponent<Image>();
                        if (pi != null)
                        {
                            pi.raycastTarget = false;
                            pi.color = pi.sprite == null ? Hex("1E2E46") : Color.white;
                        }
                    }

                    var ring = card.Find("Ring");
                    if (ring == null)
                    {
                        var go = NewUI("Ring", card, 0, 0, 0, 0);
                        Stretch(go.GetComponent<RectTransform>());
                        var ri = go.AddComponent<Image>();
                        ri.sprite = _ring;
                        ri.type = Image.Type.Simple;
                        ri.color = accent;
                        ri.raycastTarget = false;
                    }
                    cards++;
                }
            }

            Save();
            Debug.Log($"[RATIX] Carousels improved ({cards} cards).");
        }

        // ==== Settings form =================================================

        private static void BuildForm(RectTransform panel, Color accent)
        {
            // Remove old placeholders (only text; the labels stay).
            foreach (var n in new[] { "name_hall", "#rounds", "#players", "QuickStartCheck", "ObserverCheck", "Password..." })
            {
                var old = panel.Find(n);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }
            // Idempotency.
            foreach (var n in new[] { "NameInput", "RoundsDropdown", "PlayersDropdown", "QuickStartToggle", "ObserverToggle", "PasswordInput", "Form" })
            {
                var old = panel.Find(n);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            var form = NewUI("Form", panel, 0, 0, panel.sizeDelta.x, panel.sizeDelta.y);
            form.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            form.GetComponent<RectTransform>().anchorMax = Vector2.one;
            form.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            form.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            var root = form.GetComponent<RectTransform>();

            BuildInput(root, "NameInput", 149.8f, 115f, 210.2f, 22f, "Enter name", false);
            BuildDropdown(root, "RoundsDropdown", 149.8f, 80f, 30f, 22f, new[] { "5", "10", "15" }, 0, accent);
            BuildDropdown(root, "PlayersDropdown", 314.8f, 80f, 30f, 22f, new[] { "8", "10", "16" }, 0, accent);
            BuildToggle(root, "QuickStartToggle", 149.8f, 45f, 22f, 22f, true, accent);
            BuildToggle(root, "ObserverToggle", 314.8f, 45f, 22f, 22f, true, accent);
            BuildInput(root, "PasswordInput", 149.8f, 10f, 210.2f, 22f, "Password", true);
        }

        private static TMP_InputField BuildInput(RectTransform parent, string name, float x, float y, float w, float h,
            string placeholder, bool password)
        {
            var go = NewUI(name, parent, x, y, w, h);
            var rt = go.GetComponent<RectTransform>();
            var bg = go.AddComponent<Image>();
            bg.sprite = _roundSm;
            bg.type = Image.Type.Sliced;
            bg.color = C_INPUT;

            var input = go.AddComponent<TMP_InputField>();
            input.targetGraphic = bg;

            var area = NewUI("Text Area", rt, 0, 0, w, h);
            var art = area.GetComponent<RectTransform>();
            Stretch(art, 7f, 7f, 2f, 2f);
            area.AddComponent<RectMask2D>();

            var textGo = NewUI("Text", area.transform, 0, 0, 0, 0);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.font = _font;
            text.fontSize = 12;
            text.color = C_TEXT;
            text.alignment = TextAlignmentOptions.Left;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            Stretch(text.rectTransform);

            var phGo = NewUI("Placeholder", area.transform, 0, 0, 0, 0);
            var ph = phGo.AddComponent<TextMeshProUGUI>();
            ph.font = _font;
            ph.fontSize = 12;
            ph.color = C_DIM;
            ph.alignment = TextAlignmentOptions.Left;
            ph.raycastTarget = false;
            ph.text = placeholder;
            ph.textWrappingMode = TextWrappingModes.NoWrap;
            Stretch(ph.rectTransform);

            input.textViewport = art;
            input.textComponent = text;
            input.placeholder = ph;
            input.pointSize = 12;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.contentType = password ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            input.characterLimit = password ? 16 : 20;
            input.caretColor = Color.white;
            input.customCaretColor = true;
            input.selectionColor = new Color(0.13f, 0.77f, 0.37f, 0.35f);
            return input;
        }

        private static MiniDropdown BuildDropdown(RectTransform parent, string name, float x, float y, float w, float h,
            string[] options, int value, Color accent)
        {
            var go = NewUI(name, parent, x, y, w, h);
            var rt = go.GetComponent<RectTransform>();
            var bg = go.AddComponent<Image>();
            bg.sprite = _roundSm;
            bg.type = Image.Type.Sliced;
            bg.color = C_INPUT;

            var rim = go.AddComponent<Outline>();
            rim.effectColor = new Color(1f, 1f, 1f, 0.06f);
            rim.effectDistance = new Vector2(1f, -1f);

            var rootButton = go.AddComponent<Button>();
            rootButton.targetGraphic = bg;
            Normalize(rootButton);

            var caption = Label(rt, "Label", options[value], 12f, C_TEXT, FontStyles.Normal, TextAlignmentOptions.Center);
            Stretch(caption.rectTransform, 2f, 2f, 0f, 0f);

            float listW = Mathf.Max(90f, w);
            var listGo = NewUI("List", rt, 0, 0, listW, options.Length * 26f + 4f);
            var lrt = listGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0f, 0f);
            lrt.anchorMax = new Vector2(0f, 0f);
            lrt.pivot = new Vector2(0f, 1f);
            lrt.anchoredPosition = new Vector2(0f, -2f);
            lrt.sizeDelta = new Vector2(listW, options.Length * 26f + 4f);
            var listImg = listGo.AddComponent<Image>();
            listImg.sprite = _roundSm;
            listImg.type = Image.Type.Sliced;
            listImg.color = C_SURFACE2;
            var listRim = listGo.AddComponent<Outline>();
            listRim.effectColor = new Color(1f, 1f, 1f, 0.08f);
            listRim.effectDistance = new Vector2(1f, -1f);

            var buttons = new Button[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                var row = NewUI("Option" + i, lrt, 2f, 2f + (options.Length - 1 - i) * 26f, listW - 4f, 24f);
                var rowImg = row.AddComponent<Image>();
                rowImg.sprite = _roundSm;
                rowImg.type = Image.Type.Sliced;
                rowImg.color = i % 2 == 0 ? C_CARD : C_CARD_ALT;
                var rowBtn = row.AddComponent<Button>();
                rowBtn.targetGraphic = rowImg;
                Normalize(rowBtn);

                var rowLabel = Label(row.GetComponent<RectTransform>(), "Label", options[i], 12f, C_TEXT,
                    FontStyles.Normal, TextAlignmentOptions.Left);
                Stretch(rowLabel.rectTransform, 8f, 6f, 0f, 0f);
                buttons[i] = rowBtn;
            }

            var md = go.AddComponent<MiniDropdown>();
            md.Init(caption, listGo, buttons, options);
            md.Select(value);
            listGo.SetActive(false);
            return md;
        }

        private static Toggle BuildToggle(RectTransform parent, string name, float x, float y, float w, float h,
            bool isOn, Color accent)
        {
            var go = NewUI(name, parent, x, y, w, h);
            var bg = go.AddComponent<Image>();
            bg.sprite = _roundSm;
            bg.type = Image.Type.Sliced;
            bg.color = C_INPUT;

            var toggle = go.AddComponent<Toggle>();
            toggle.targetGraphic = bg;

            var checkGo = NewUI("Checkmark", go.GetComponent<RectTransform>(), (w - 10f) * 0.5f, (h - 10f) * 0.5f, 10f, 10f);
            var check = checkGo.AddComponent<Image>();
            check.sprite = _circle;
            check.color = accent;

            toggle.graphic = check;
            toggle.isOn = isOn;
            return toggle;
        }

        // ==== Room search ===================================================

        private static void BuildRoomSearch(RectTransform roomList)
        {
            var search = roomList.Find("Search") as RectTransform;
            if (search == null) { Debug.LogError("[RATIX] Missing Search in " + roomList.name); return; }

            var icon = search.Find("Search");
            if (icon != null) icon.gameObject.SetActive(false);
            var oldPh = search.Find("SearchPlaceholder");
            if (oldPh != null) oldPh.gameObject.SetActive(false);
            var old = search.Find("SearchInput");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            BuildInput(search, "SearchInput", 120f, 4f, 330f, 39f, "Type here", false);

            var refresh = search.Find("Refresh");
            if (refresh != null && refresh.GetComponent<Button>() == null)
                ConvertToButton(refresh.gameObject, C_BUTTON, C_TEXT);
        }

        // ==== Hall actions ==================================================

        private static void BuildHallBar(RectTransform hall, Color accent)
        {
            if (hall == null) return;
            var bar = hall.Find("RoomList/Search") as RectTransform;
            if (bar == null) { Debug.LogError("[RATIX] Missing hall action bar in " + hall.name); return; }
            if (bar.name == "Search") bar.name = "ActionBar";

            var kick = bar.Find("Kick");
            if (kick != null && kick.GetComponent<Button>() == null)
                ConvertToButton(kick.gameObject, C_BUTTON, C_TEXT);

            var invite = bar.Find("Invite");
            if (invite != null && invite.GetComponent<Button>() == null)
                ConvertToButton(invite.gameObject, C_BUTTON, C_TEXT);

            var enter = bar.Find("EnterRoom");
            if (enter != null && enter.GetComponent<Button>() == null)
                ConvertToButton(enter.gameObject, accent, Color.white);
        }

        private static HallActionModal BuildActionModal(RectTransform canvas, string name, Vector2 pos,
            string title, string leftHeader, string rightHeader, string[] leftNames, string[] rightNames,
            string actionLabel, Color accent)
        {
            var existing = canvas.Find(name);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var root = NewUI(name, canvas, pos.x, pos.y, 1920f, 1080f);
            var rootRt = root.GetComponent<RectTransform>();
            var overlay = root.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.72f);

            var win = NewUI("Window", rootRt, 0, 0, 640f, 480f);
            var wrt = win.GetComponent<RectTransform>();
            wrt.anchorMin = new Vector2(0.5f, 0.5f);
            wrt.anchorMax = new Vector2(0.5f, 0.5f);
            wrt.pivot = new Vector2(0.5f, 0.5f);
            wrt.anchoredPosition = Vector2.zero;
            wrt.sizeDelta = new Vector2(640f, 480f);
            var wimg = win.AddComponent<Image>();
            wimg.sprite = _roundLg;
            wimg.type = Image.Type.Sliced;
            wimg.color = Hex("0F1929");
            var rim = win.AddComponent<Outline>();
            rim.effectColor = new Color(1f, 1f, 1f, 0.055f);
            rim.effectDistance = new Vector2(1f, -1f);

            var t = Label(wrt, "Title", title, 24f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(t.rectTransform, 24f, 414f, 420f, 40f);

            var xBtn = NewUI("XButton", wrt, 584f, 424f, 36f, 36f);
            var xImg = xBtn.AddComponent<Image>();
            xImg.sprite = _roundLg;
            xImg.type = Image.Type.Sliced;
            xImg.color = C_CARD_ALT;
            var xButton = xBtn.AddComponent<Button>();
            xButton.targetGraphic = xImg;
            Normalize(xButton);
            var xLabel = Label(xBtn.GetComponent<RectTransform>(), "Label", "X", 18f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(xLabel.rectTransform);

            var left = Panel(wrt, "LeftPanel", 24f, 84f, 284f, 300f);
            var right = Panel(wrt, "RightPanel", 332f, 84f, 284f, 300f);

            var lh = Label(left, "Header", leftHeader, 12f, C_MUTED, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(lh.rectTransform, 12f, 266f, 260f, 24f);
            var rh = Label(right, "Header", rightHeader, 12f, C_MUTED, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(rh.rectTransform, 12f, 266f, 260f, 24f);

            for (int i = 0; i < leftNames.Length; i++) BuildRow(left, i, leftNames[i], accent);
            for (int i = 0; i < rightNames.Length; i++) BuildRow(right, i, rightNames[i], accent);

            var selected = Label(wrt, "SelectedLabel", "Selected: none", 12f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(selected.rectTransform, 24f, 36f, 300f, 24f);

            var actionGo = NewUI("ActionButton", wrt, 416f, 24f, 200f, 52f);
            var aImg = actionGo.AddComponent<Image>();
            aImg.sprite = _roundSm;
            aImg.type = Image.Type.Sliced;
            aImg.color = accent;
            var aBtn = actionGo.AddComponent<Button>();
            aBtn.targetGraphic = aImg;
            Normalize(aBtn);
            var al = Label(actionGo.GetComponent<RectTransform>(), "Label", actionLabel, 18f, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(al.rectTransform);

            var modal = root.AddComponent<HallActionModal>();
            modal.Init(actionLabel, aBtn, selected);
            return modal;
        }

        private static void BuildRow(RectTransform panel, int index, string name, Color accent)
        {
            var go = NewUI("Row" + index, panel, 8f, 228f - index * 30f, 268f, 26f);
            var img = go.AddComponent<Image>();
            img.sprite = _roundSm;
            img.type = Image.Type.Sliced;
            img.color = index % 2 == 0 ? C_CARD : C_CARD_ALT;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            Normalize(btn);
            var label = Label(go.GetComponent<RectTransform>(), "Label", name, 13f, C_TEXT, FontStyles.Normal, TextAlignmentOptions.Left);
            Stretch(label.rectTransform, 10f, 8f, 0f, 0f);
        }

        // ==== Settings modal ================================================

        [MenuItem("Tools/RATIX/UI/8. Settings content")]
        public static void BuildSettingsContent()
        {
            LoadAssets();
            var canvas = Canvas();
            var modal = canvas.Find("SettingsModal") as RectTransform;
            if (modal == null) { Debug.LogError("[RATIX] SettingsModal not found"); return; }
            var win = modal.Find("Window") as RectTransform;
            if (win == null) { Debug.LogError("[RATIX] SettingsModal/Window not found"); return; }

            foreach (var n in new[]
            {
                "AudioHeader", "VideoHeader", "MasterLabel", "MasterSlider", "MasterValue",
                "MusicLabel", "MusicSlider", "MusicValue", "EffectsLabel", "EffectsSlider", "EffectsValue",
                "DisplayLabel", "DisplayDropdown", "QualityLabel", "QualityDropdown",
                "VsyncLabel", "VsyncToggle", "FpsLabel", "FpsToggle",
                "LanguageLabel", "LanguageDropdown", "ResetButton", "ApplyButton"
            })
            {
                var old = win.Find(n);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            Section(win, "AudioHeader", "AUDIO", 24f, 356f);
            var master = BuildSlider(win, "MasterSlider", 24f, 302f, 230f, 0.8f, C_GREEN);
            var mv = ValueLabel(win, "MasterValue", "80%", 260f, 302f);
            MasterLabel(win, "MasterLabel", "Master volume", 24f, 326f);

            var music = BuildSlider(win, "MusicSlider", 24f, 246f, 230f, 0.6f, C_GREEN);
            var muv = ValueLabel(win, "MusicValue", "60%", 260f, 246f);
            MasterLabel(win, "MusicLabel", "Music", 24f, 270f);

            var effects = BuildSlider(win, "EffectsSlider", 24f, 190f, 230f, 0.7f, C_GREEN);
            var ev = ValueLabel(win, "EffectsValue", "70%", 260f, 190f);
            MasterLabel(win, "EffectsLabel", "Effects", 24f, 214f);

            Section(win, "VideoHeader", "VIDEO", 324f, 356f);
            MasterLabel(win, "DisplayLabel", "Display mode", 324f, 322f);
            BuildDropdown(win, "DisplayDropdown", 462f, 318f, 150f, 28f, new[] { "Fullscreen", "Windowed" }, 0, C_GREEN);
            MasterLabel(win, "QualityLabel", "Quality", 324f, 282f);
            BuildDropdown(win, "QualityDropdown", 462f, 278f, 150f, 28f, new[] { "Low", "Medium", "High", "Ultra" }, 2, C_GREEN);
            MasterLabel(win, "VsyncLabel", "VSync", 324f, 242f);
            BuildToggle(win, "VsyncToggle", 590f, 240f, 22f, 22f, true, C_GREEN);
            MasterLabel(win, "FpsLabel", "Show FPS", 324f, 206f);
            BuildToggle(win, "FpsToggle", 590f, 204f, 22f, 22f, false, C_GREEN);

            MasterLabel(win, "LanguageLabel", "Language", 24f, 140f);
            BuildDropdown(win, "LanguageDropdown", 120f, 136f, 130f, 28f, new[] { "English", "Spanish" }, 0, C_GREEN);

            var reset = TextButton(win, "ResetButton", "RESET", 324f, 40f, 140f, 44f, C_BUTTON, 15f, C_TEXT);
            var apply = TextButton(win, "ApplyButton", "APPLY", 476f, 40f, 140f, 44f, C_GREEN, 15f, Color.white);

            var settings = modal.GetComponent<SettingsPanel>();
            if (settings == null) settings = modal.gameObject.AddComponent<SettingsPanel>();
            settings.Init(master, music, effects, mv, muv, ev, apply, reset);

            Save();
            Debug.Log("[RATIX] Settings content built.");
        }

        private static void Section(RectTransform parent, string name, string text, float x, float y)
        {
            var t = Label(parent, name, text, 12f, C_MUTED, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(t.rectTransform, x, y, 200f, 24f);
            t.characterSpacing = 2f;
        }

        private static void MasterLabel(RectTransform parent, string name, string text, float x, float y)
        {
            var t = Label(parent, name, text, 14f, C_TEXT, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(t.rectTransform, x, y, 200f, 22f);
        }

        private static TMP_Text ValueLabel(RectTransform parent, string name, string text, float x, float y)
        {
            var t = Label(parent, name, text, 14f, C_GREEN, FontStyles.Bold, TextAlignmentOptions.Right);
            Place(t.rectTransform, x, y, 44f, 20f);
            return t;
        }

        private static Slider BuildSlider(RectTransform parent, string name, float x, float y, float w, float value, Color accent)
        {
            var go = NewUI(name, parent, x, y, w, 20f);
            var rt = go.GetComponent<RectTransform>();
            var bg = go.AddComponent<Image>();
            bg.sprite = _roundSm;
            bg.type = Image.Type.Sliced;
            bg.color = C_INPUT;

            var slider = go.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;

            var fillArea = NewUI("Fill Area", rt, 0f, 0f, w, 20f);
            var fart = fillArea.GetComponent<RectTransform>();
            Stretch(fart);

            var fill = NewUI("Fill", fart, 0f, 0f, w, 20f);
            var frt = fill.GetComponent<RectTransform>();
            Stretch(frt);
            var fimg = fill.AddComponent<Image>();
            fimg.sprite = _roundSm;
            fimg.type = Image.Type.Sliced;
            fimg.color = accent;

            var handleArea = NewUI("Handle Slide Area", rt, 0f, 0f, w, 20f);
            Stretch(handleArea.GetComponent<RectTransform>());
            var handle = NewUI("Handle", handleArea.transform, 0f, 0f, 20f, 0f);
            var himg = handle.AddComponent<Image>();
            himg.sprite = _circle;
            himg.color = Color.white;

            slider.fillRect = frt;
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = himg;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            return slider;
        }

        private static Button TextButton(RectTransform parent, string name, string text, float x, float y, float w, float h,
            Color bgColor, float fontSize, Color textColor)
        {
            var go = NewUI(name, parent, x, y, w, h);
            var img = go.AddComponent<Image>();
            img.sprite = _roundSm;
            img.type = Image.Type.Sliced;
            img.color = bgColor;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            Normalize(btn);
            var label = Label(go.GetComponent<RectTransform>(), "Label", text, fontSize, textColor, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            return btn;
        }

        // ==== Mail modal ====================================================

        [MenuItem("Tools/RATIX/UI/9. Mail content")]
        public static void BuildMailContent()
        {
            LoadAssets();
            var canvas = Canvas();
            var modal = canvas.Find("MailModal") as RectTransform;
            if (modal == null) { Debug.LogError("[RATIX] MailModal not found"); return; }
            var win = modal.Find("Window") as RectTransform;
            var details = modal.Find("Details") as RectTransform;
            if (win == null || details == null) { Debug.LogError("[RATIX] MailModal structure incomplete"); return; }

            // Center both windows inside the popup.
            win.anchorMin = win.anchorMax = new Vector2(0.5f, 0.5f);
            win.pivot = new Vector2(0.5f, 0.5f);
            win.anchoredPosition = Vector2.zero;

            details.anchorMin = details.anchorMax = new Vector2(0.5f, 0.5f);
            details.pivot = new Vector2(0.5f, 0.5f);
            details.anchoredPosition = Vector2.zero;
            details.sizeDelta = new Vector2(400f, 310f);

            var detailsWindow = details.Find("Window") as RectTransform;
            if (detailsWindow != null)
            {
                detailsWindow.anchorMin = detailsWindow.anchorMax = new Vector2(0.5f, 0.5f);
                detailsWindow.pivot = new Vector2(0.5f, 0.5f);
                detailsWindow.anchoredPosition = Vector2.zero;
                detailsWindow.sizeDelta = new Vector2(400f, 310f);
            }

            for (int i = 0; i < 8; i++)
            {
                var old = win.Find("MailRow" + i);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            var subjects = new[]
            {
                "Welcome to RATIX!",
                "Daily reward",
                "Clan invitation",
                "Server maintenance",
                "New season is live"
            };
            var senders = new[] { "RATIX Team", "System", "InsanosPROs67", "Support", "Store" };
            var dates = new[] { "Today", "Today", "Yesterday", "2 days ago", "3 days ago" };
            var bodies = new[]
            {
                "Welcome, soldier. Complete your first missions to earn exclusive rewards and discover everything the game has to offer. Good luck out there!",
                "Your daily reward is ready: 250 coins. Come back tomorrow to collect more.",
                "Your clan has invited you to join the next clan war. Accept before the timer ends.",
                "Servers will be under maintenance tonight from 02:00 to 04:00. Thanks for your patience.",
                "The new season is live. Unlock exclusive skins and weapons before the season ends."
            };

            var panel = modal.GetComponent<MailPanel>();
            if (panel == null) panel = modal.gameObject.AddComponent<MailPanel>();

            for (int i = 0; i < subjects.Length; i++)
                BuildMailRow(win, i, subjects.Length, senders[i], subjects[i], dates[i], i < 3, panel);

            var title = details.Find("Window/Details")?.GetComponent<TMP_Text>();
            var body = details.Find("Window/Details (1)")?.GetComponent<TMP_Text>();
            if (title != null) { title.fontSize = 20f; title.color = C_TEXT; }
            if (body != null)
            {
                body.fontSize = 14f;
                body.color = C_MUTED;
                body.alignment = TextAlignmentOptions.TopLeft;
                body.textWrappingMode = TextWrappingModes.Normal;
                body.margin = new Vector4(10f, 10f, 10f, 10f);
            }

            // Shrink the action labels that were authored too large for their buttons.
            foreach (var path in new[] { "Window/ActionBar/Clear all", "Window/ActionBar/Open All",
                         "Details/Window/ActionBar/Close", "Details/Window/ActionBar/Open" })
            {
                var t = modal.Find(path + "/Label") ?? modal.Find(path + "/" + path.Substring(path.LastIndexOf('/') + 1));
                var tmp = t != null ? t.GetComponent<TMP_Text>() : null;
                if (tmp != null) tmp.fontSize = 13f;
            }

            panel.Init(details.gameObject, title, body, subjects, senders, bodies);
            details.gameObject.SetActive(false);

            Save();
            Debug.Log("[RATIX] Mail content built.");
        }

        private static void BuildMailRow(RectTransform win, int index, int total, string sender, string subject, string date,
            bool unread, MailPanel panel)
        {
            var row = NewUI("MailRow" + index, win, 13f, 64f + (total - 1 - index) * 56f, 420f, 52f);
            var img = row.AddComponent<Image>();
            img.sprite = _roundSm;
            img.type = Image.Type.Sliced;
            img.color = index % 2 == 0 ? C_CARD : C_CARD_ALT;

            var dot = NewUI("Unread", row.GetComponent<RectTransform>(), 8f, 22f, 8f, 8f);
            var di = dot.AddComponent<Image>();
            di.sprite = _circle;
            di.color = unread ? C_GREEN : new Color(1f, 1f, 1f, 0.12f);

            var s = Label(row.GetComponent<RectTransform>(), "Sender", sender, 14f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(s.rectTransform, 24f, 26f, 170f, 18f);
            var sub = Label(row.GetComponent<RectTransform>(), "Subject", subject, 12f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(sub.rectTransform, 24f, 7f, 245f, 18f);
            var d = Label(row.GetComponent<RectTransform>(), "Date", date, 10f, C_DIM, FontStyles.Normal, TextAlignmentOptions.Right);
            Place(d.rectTransform, 200f, 27f, 130f, 16f);

            var view = TextButton(row.GetComponent<RectTransform>(), "View", "VIEW", 340f, 8f, 70f, 36f, C_BUTTON, 12f, C_TEXT);
            UnityEventTools.AddIntPersistentListener(view.onClick, panel.Select, index);
        }

        // ==== Store =========================================================

        [MenuItem("Tools/RATIX/UI/10. Store content")]
        public static void BuildStoreContent()
        {
            LoadAssets();
            var canvas = Canvas();
            var panel = (canvas.Find("Content/StoreModelPanel") ?? canvas.Find("Content/StoreModalPanel")) as RectTransform;
            if (panel == null) { Debug.LogError("[RATIX] Store panel not found"); return; }

            foreach (var n in new[]
            {
                "Tabs", "SearchInput", "CurrencyCoins", "CurrencyGold", "CoinsCap", "GoldCap",
                "EquippedPanel", "ItemCard0", "ItemCard1", "ItemCard2", "ItemCard3", "ItemCard4", "ItemCard5",
                "DetailsPanel", "BottomBar"
            })
            {
                var old = panel.Find(n);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            var store = panel.GetComponent<StorePanel>();
            if (store == null) store = panel.gameObject.AddComponent<StorePanel>();

            var storeTitle = panel.Find("Header/Label")?.GetComponent<TMP_Text>();
            if (storeTitle != null)
            {
                storeTitle.color = C_GOLD;
                storeTitle.characterSpacing = 3f;
            }

            // Category tabs.
            var tabs = NewUI("Tabs", panel, 20f, 868f, 850f, 44f);
            string[] tabNames = { "Packages", "Weapons", "Characters", "Skins", "Items" };
            for (int i = 0; i < tabNames.Length; i++)
            {
                var b = TextButton(tabs.GetComponent<RectTransform>(), "Tab" + i, tabNames[i], i * 170f, 0f, 158f, 44f,
                    i == 1 ? C_GOLD : C_CARD_ALT, 14f, i == 1 ? Hex("1A1206") : C_MUTED);
            }

            BuildInput(panel, "SearchInput", 900f, 870f, 300f, 40f, "Search item", false);

            var coinsCap = Label(panel, "CoinsCap", "COINS", 10f, C_DIM, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(coinsCap.rectTransform, 1080f, 878f, 50f, 24f);
            var coins = Label(panel, "CurrencyCoins", "12,450", 16f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(coins.rectTransform, 1120f, 872f, 120f, 36f);
            var goldCap = Label(panel, "GoldCap", "GOLD", 10f, C_DIM, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(goldCap.rectTransform, 1260f, 878f, 40f, 24f);
            var gold = Label(panel, "CurrencyGold", "320", 16f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(gold.rectTransform, 1300f, 872f, 90f, 36f);

            // Item grid (3 columns).
            var names = new[] { "RATIX Prime", "Nightfall", "Vanguard", "Ember", "Frostbite", "Wasteland", "Sunset", "Obsidian", "Tundra" };
            var rarities = new[] { "Legendary", "Epic", "Rare", "Legendary", "Epic", "Rare", "Epic", "Legendary", "Rare" };
            var prices = new[] { "2,400", "1,800", "1,200", "2,600", "1,900", "1,100", "1,700", "2,800", "1,300" };
            var descriptions = new[]
            {
                "The signature RATIX rifle. Balanced damage, control and a golden finish.",
                "A stealth-focused rifle with a dark violet finish and improved accuracy.",
                "Standard-issue assault rifle. Reliable in any situation.",
                "A blazing skin with animated embers along the barrel.",
                "A cold, precise rifle with a frost-blue finish.",
                "A rugged wasteland skin built for survival.",
                "A warm sunset finish with a polished amber barrel.",
                "A blacked-out rifle with obsidian accents and gold trim.",
                "A frozen skin with a pale blue finish and frost details."
            };
            var statsAr = new[]
            {
                "Damage 85\nFire rate 62\nAccuracy 74",
                "Damage 78\nFire rate 70\nAccuracy 80",
                "Damage 70\nFire rate 66\nAccuracy 72",
                "Damage 82\nFire rate 64\nAccuracy 76",
                "Damage 76\nFire rate 72\nAccuracy 82",
                "Damage 72\nFire rate 68\nAccuracy 70",
                "Damage 80\nFire rate 66\nAccuracy 76",
                "Damage 88\nFire rate 60\nAccuracy 78",
                "Damage 74\nFire rate 70\nAccuracy 74"
            };
            var rarityColors = new[] { C_GOLD, Hex("A855F7"), C_BLUE, C_GOLD, Hex("A855F7"), C_BLUE, Hex("A855F7"), C_GOLD, C_BLUE };

            for (int i = 0; i < names.Length; i++)
            {
                int col = i % 3;
                int row = i / 3;
                float x = 20f + col * 365f;
                float y = 590f - row * 240f;
                BuildStoreCard(panel, store, i, x, y, names[i], rarities[i], prices[i], rarityColors[i]);
            }

            // Details.
            var det = NewUI("DetailsPanel", panel, 1120f, 110f, 320f, 720f);
            var detImg = det.AddComponent<Image>();
            detImg.sprite = _roundLg;
            detImg.type = Image.Type.Sliced;
            detImg.color = C_CARD;
            var rim = det.AddComponent<Outline>();
            rim.effectColor = new Color(1f, 1f, 1f, 0.055f);
            rim.effectDistance = new Vector2(1f, -1f);

            var dTitle = Label(det.GetComponent<RectTransform>(), "Title", "ITEM DETAILS", 12f, C_MUTED, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(dTitle.rectTransform, 16f, 682f, 288f, 24f);
            dTitle.characterSpacing = 2f;

            var dIcon = NewUI("Icon", det.GetComponent<RectTransform>(), 60f, 450f, 200f, 200f);
            var dIconImg = dIcon.AddComponent<Image>();
            dIconImg.sprite = _roundLg;
            dIconImg.type = Image.Type.Sliced;
            dIconImg.color = Color.Lerp(C_SURFACE, C_GOLD, 0.4f);
            var dLetter = Label(dIcon.GetComponent<RectTransform>(), "Letter", "R", 64f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(dLetter.rectTransform);

            var dName = Label(det.GetComponent<RectTransform>(), "Name", "RATIX Prime", 20f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(dName.rectTransform, 16f, 410f, 288f, 30f);
            var dRarity = Label(det.GetComponent<RectTransform>(), "Rarity", "Legendary", 13f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(dRarity.rectTransform, 16f, 384f, 288f, 22f);
            var dDesc = Label(det.GetComponent<RectTransform>(), "Description", descriptions[0], 11f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            Place(dDesc.rectTransform, 16f, 300f, 288f, 76f);
            dDesc.textWrappingMode = TextWrappingModes.Normal;
            var dStats = Label(det.GetComponent<RectTransform>(), "Stats", statsAr[0], 12f, C_TEXT, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            Place(dStats.rectTransform, 16f, 220f, 288f, 70f);
            dStats.textWrappingMode = TextWrappingModes.Normal;
            var dPrice = Label(det.GetComponent<RectTransform>(), "Price", prices[0] + " coins", 18f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(dPrice.rectTransform, 16f, 150f, 288f, 30f);
            var dBtn = TextButton(det.GetComponent<RectTransform>(), "BuySelected", "BUY", 16f, 90f, 288f, 50f, C_GOLD, 18f, Hex("1A1206"));
            var dStatus = Label(det.GetComponent<RectTransform>(), "Status", "", 12f, C_GREEN, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(dStatus.rectTransform, 16f, 50f, 288f, 24f);

            // Bottom bar.
            var bottom = NewUI("BottomBar", panel, 20f, 20f, 1420f, 72f);
            var bImg = bottom.AddComponent<Image>();
            bImg.sprite = _roundLg;
            bImg.type = Image.Type.Sliced;
            bImg.color = C_CARD;

            TextButton(bottom.GetComponent<RectTransform>(), "Prev", "<", 12f, 16f, 60f, 40f, C_BUTTON, 16f, C_TEXT);
            var page = Label(bottom.GetComponent<RectTransform>(), "Page", "1 / 5", 14f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Center);
            Place(page.rectTransform, 80f, 16f, 80f, 40f);
            TextButton(bottom.GetComponent<RectTransform>(), "Next", ">", 170f, 16f, 60f, 40f, C_BUTTON, 16f, C_TEXT);
            BuildDropdown(bottom.GetComponent<RectTransform>(), "Filter", 250f, 22f, 140f, 28f, new[] { "All", "Weapons", "Skins", "Items" }, 0, C_GOLD);
            var qty = Label(bottom.GetComponent<RectTransform>(), "Qty", "Qty 33 / 99", 12f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(qty.rectTransform, 410f, 16f, 180f, 40f);
            TextButton(bottom.GetComponent<RectTransform>(), "Clear", "CLEAR", 1240f, 16f, 160f, 40f, C_BUTTON, 14f, C_TEXT);

            store.Init(dName, dRarity, dDesc, dStats, dPrice, dStatus, dIconImg, dBtn,
                names, rarities, descriptions, prices, statsAr, rarityColors);
            store.Select(0);

            Save();
            Debug.Log("[RATIX] Store content built.");
        }

        private static void BuildStoreCard(RectTransform panel, StorePanel store, int index, float x, float y,
            string name, string rarity, string price, Color rarityColor)
        {
            var card = NewUI("ItemCard" + index, panel, x, y, 350f, 220f);
            var img = card.AddComponent<Image>();
            img.sprite = _roundLg;
            img.type = Image.Type.Sliced;
            img.color = C_CARD;
            var rim = card.AddComponent<Outline>();
            rim.effectColor = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.55f);
            rim.effectDistance = new Vector2(1.5f, -1.5f);

            var icon = NewUI("Icon", card.GetComponent<RectTransform>(), 14f, 84f, 110f, 110f);
            var ii = icon.AddComponent<Image>();
            ii.sprite = _roundLg;
            ii.type = Image.Type.Sliced;
            ii.color = Color.Lerp(C_SURFACE, rarityColor, 0.4f);
            var letter = Label(icon.GetComponent<RectTransform>(), "Letter", name.Substring(0, 1), 40f, rarityColor, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(letter.rectTransform);

            var nm = Label(card.GetComponent<RectTransform>(), "Name", name, 20f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(nm.rectTransform, 136f, 160f, 200f, 28f);
            var rar = Label(card.GetComponent<RectTransform>(), "Rarity", rarity, 12f, rarityColor, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(rar.rectTransform, 136f, 136f, 200f, 20f);
            var perm = Label(card.GetComponent<RectTransform>(), "Perm", "Permanent", 11f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(perm.rectTransform, 136f, 114f, 200f, 18f);
            var priceLabel = Label(card.GetComponent<RectTransform>(), "Price", price + " coins", 14f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(priceLabel.rectTransform, 136f, 88f, 200f, 20f);

            var view = TextButton(card.GetComponent<RectTransform>(), "View", "VIEW", 136f, 16f, 96f, 34f, C_BUTTON, 12f, C_TEXT);
            var buy = TextButton(card.GetComponent<RectTransform>(), "Buy", "BUY", 238f, 16f, 98f, 34f, rarityColor, 12f, Hex("120D06"));

            UnityEventTools.AddIntPersistentListener(view.onClick, store.Select, index);
            UnityEventTools.AddIntPersistentListener(buy.onClick, store.Buy, index);
        }

        // ==== Warehouse =====================================================

        [MenuItem("Tools/RATIX/UI/11. Warehouse content")]
        public static void BuildWarehouseContent()
        {
            LoadAssets();
            var canvas = Canvas();
            var panel = (canvas.Find("Content/WarehouseModelPanel") ?? canvas.Find("Content/WarehouseModalPanel")) as RectTransform;
            if (panel == null) { Debug.LogError("[RATIX] Warehouse panel not found"); return; }

            foreach (var n in new[]
            {
                "Tabs", "SearchInput", "CurrencyCoins", "CurrencyGold", "CoinsCap", "GoldCap",
                "WeaponsBar", "ItemCard0", "ItemCard1", "ItemCard2", "ItemCard3", "ItemCard4",
                "ItemCard5", "ItemCard6", "ItemCard7", "ItemCard8", "BottomBar"
            })
            {
                var old = panel.Find(n);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            var wh = panel.GetComponent<WarehousePanel>();
            if (wh == null) wh = panel.gameObject.AddComponent<WarehousePanel>();

            var title = panel.Find("Header/Label")?.GetComponent<TMP_Text>();
            if (title != null)
            {
                title.color = C_GOLD;
                title.characterSpacing = 3f;
            }

            // Category tabs.
            var tabs = NewUI("Tabs", panel, 20f, 868f, 520f, 44f);
            string[] tabNames = { "Weapons", "Skins", "Items" };
            for (int i = 0; i < tabNames.Length; i++)
                TextButton(tabs.GetComponent<RectTransform>(), "Tab" + i, tabNames[i], i * 170f, 0f, 158f, 44f,
                    i == 0 ? C_GOLD : C_CARD_ALT, 14f, i == 0 ? Hex("1A1206") : C_MUTED);

            BuildInput(panel, "SearchInput", 900f, 870f, 300f, 40f, "Search item", false);

            var coinsCap = Label(panel, "CoinsCap", "COINS", 10f, C_DIM, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(coinsCap.rectTransform, 1080f, 878f, 50f, 24f);
            var coins = Label(panel, "CurrencyCoins", "12,450", 16f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(coins.rectTransform, 1120f, 872f, 120f, 36f);
            var goldCap = Label(panel, "GoldCap", "GOLD", 10f, C_DIM, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(goldCap.rectTransform, 1260f, 878f, 40f, 24f);
            var gold = Label(panel, "CurrencyGold", "320", 16f, C_GOLD, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(gold.rectTransform, 1300f, 872f, 90f, 36f);

            // Weapons bar (equipped).
            var bar = NewUI("WeaponsBar", panel, 20f, 110f, 300f, 720f);
            var barImg = bar.AddComponent<Image>();
            barImg.sprite = _roundLg;
            barImg.type = Image.Type.Sliced;
            barImg.color = C_CARD;
            var barTitle = Label(bar.GetComponent<RectTransform>(), "Title", "WEAPONS", 12f, C_MUTED, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(barTitle.rectTransform, 14f, 684f, 270f, 24f);
            barTitle.characterSpacing = 2f;

            string[] slotNames = { "RATIX Prime", "Sidearm", "Combat Knife", "Frag Grenade", "Smoke" };
            string[] slotTypes = { "Primary weapon", "Secondary weapon", "Melee", "Explosive", "Tactical" };
            Color[] slotRarity = { C_GOLD, Hex("A855F7"), C_BLUE, C_AMBER, C_MUTED };
            for (int i = 0; i < slotNames.Length; i++)
            {
                var slot = NewUI("Slot" + i, bar.GetComponent<RectTransform>(), 12f, 550f - i * 130f, 276f, 118f);
                var si = slot.AddComponent<Image>();
                si.sprite = _roundSm;
                si.type = Image.Type.Sliced;
                si.color = C_CARD_ALT;

                var rarity = NewUI("Rarity", slot.GetComponent<RectTransform>(), 0f, 0f, 5f, 118f);
                rarity.AddComponent<Image>().color = slotRarity[i];

                var icon = NewUI("Icon", slot.GetComponent<RectTransform>(), 14f, 40f, 60f, 60f);
                var ii = icon.AddComponent<Image>();
                ii.sprite = _roundSm;
                ii.type = Image.Type.Sliced;
                ii.color = Color.Lerp(C_SURFACE, slotRarity[i], 0.35f);
                var letter = Label(icon.GetComponent<RectTransform>(), "Letter", slotNames[i].Substring(0, 1), 24f, slotRarity[i], FontStyles.Bold, TextAlignmentOptions.Center);
                Stretch(letter.rectTransform);

                var nm = Label(slot.GetComponent<RectTransform>(), "Name", slotNames[i], 14f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
                Place(nm.rectTransform, 86f, 76f, 180f, 20f);
                var tp = Label(slot.GetComponent<RectTransform>(), "Type", slotTypes[i], 11f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
                Place(tp.rectTransform, 86f, 56f, 180f, 18f);

                var remove = TextButton(slot.GetComponent<RectTransform>(), "Remove", "REMOVE", 190f, 12f, 74f, 26f, C_BUTTON, 9f, C_TEXT);
                UnityEventTools.AddIntPersistentListener(remove.onClick, wh.RemoveEquipped, i);
            }

            // Owned items (3 columns).
            var names = new[] { "RATIX Prime", "Nightfall", "Vanguard", "Ember", "Frostbite", "Wasteland", "Sunset", "Obsidian", "Tundra" };
            var rarities = new[] { "Legendary", "Epic", "Rare", "Legendary", "Epic", "Rare", "Epic", "Legendary", "Rare" };
            var rarityColors = new[] { C_GOLD, Hex("A855F7"), C_BLUE, C_GOLD, Hex("A855F7"), C_BLUE, Hex("A855F7"), C_GOLD, C_BLUE };

            for (int i = 0; i < names.Length; i++)
            {
                int col = i % 3;
                int row = i / 3;
                float x = 340f + col * 375f;
                float y = 590f - row * 240f;
                BuildWarehouseCard(panel, wh, i, x, y, names[i], rarities[i], rarityColors[i]);
            }

            // Bottom bar.
            var bottom = NewUI("BottomBar", panel, 20f, 20f, 1420f, 72f);
            var bImg = bottom.AddComponent<Image>();
            bImg.sprite = _roundLg;
            bImg.type = Image.Type.Sliced;
            bImg.color = C_CARD;

            TextButton(bottom.GetComponent<RectTransform>(), "Prev", "<", 12f, 16f, 60f, 40f, C_BUTTON, 16f, C_TEXT);
            var page = Label(bottom.GetComponent<RectTransform>(), "Page", "1 / 12", 14f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Center);
            Place(page.rectTransform, 80f, 16f, 80f, 40f);
            TextButton(bottom.GetComponent<RectTransform>(), "Next", ">", 170f, 16f, 60f, 40f, C_BUTTON, 16f, C_TEXT);
            BuildDropdown(bottom.GetComponent<RectTransform>(), "Filter", 250f, 22f, 140f, 28f, new[] { "All", "Weapons", "Skins", "Items" }, 0, C_GOLD);
            var qty = Label(bottom.GetComponent<RectTransform>(), "Qty", "Qty 33 / 99", 12f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(qty.rectTransform, 410f, 16f, 180f, 40f);

            var clear = TextButton(bottom.GetComponent<RectTransform>(), "Clear", "CLEAR", 1080f, 16f, 140f, 40f, C_BUTTON, 14f, C_TEXT);
            UnityEventTools.AddPersistentListener(clear.onClick, wh.Clear);
            var repair = TextButton(bottom.GetComponent<RectTransform>(), "RepairAll", "REPAIR ALL", 1240f, 16f, 160f, 40f, C_GOLD, 13f, Hex("1A1206"));
            UnityEventTools.AddPersistentListener(repair.onClick, wh.RepairAll);

            Save();
            Debug.Log("[RATIX] Warehouse content built.");
        }

        private static void BuildWarehouseCard(RectTransform panel, WarehousePanel wh, int index, float x, float y,
            string name, string rarity, Color rarityColor)
        {
            var card = NewUI("ItemCard" + index, panel, x, y, 350f, 220f);
            var img = card.AddComponent<Image>();
            img.sprite = _roundLg;
            img.type = Image.Type.Sliced;
            img.color = C_CARD;
            var rim = card.AddComponent<Outline>();
            rim.effectColor = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.55f);
            rim.effectDistance = new Vector2(1.5f, -1.5f);

            var icon = NewUI("Icon", card.GetComponent<RectTransform>(), 14f, 84f, 110f, 110f);
            var ii = icon.AddComponent<Image>();
            ii.sprite = _roundLg;
            ii.type = Image.Type.Sliced;
            ii.color = Color.Lerp(C_SURFACE, rarityColor, 0.4f);
            var letter = Label(icon.GetComponent<RectTransform>(), "Letter", name.Substring(0, 1), 40f, rarityColor, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(letter.rectTransform);

            var nm = Label(card.GetComponent<RectTransform>(), "Name", name, 20f, C_TEXT, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(nm.rectTransform, 136f, 164f, 200f, 28f);
            var rar = Label(card.GetComponent<RectTransform>(), "Rarity", rarity, 12f, rarityColor, FontStyles.Bold, TextAlignmentOptions.Left);
            Place(rar.rectTransform, 136f, 140f, 200f, 20f);
            var perm = Label(card.GetComponent<RectTransform>(), "Perm", "Permanent", 11f, C_MUTED, FontStyles.Normal, TextAlignmentOptions.Left);
            Place(perm.rectTransform, 136f, 118f, 200f, 18f);

            var equip = TextButton(card.GetComponent<RectTransform>(), "Equip", "EQUIP", 136f, 70f, 96f, 34f, C_GOLD, 12f, Hex("1A1206"));
            var remove = TextButton(card.GetComponent<RectTransform>(), "Remove", "REMOVE", 238f, 70f, 98f, 34f, C_BUTTON, 12f, C_TEXT);

            UnityEventTools.AddIntPersistentListener(equip.onClick, wh.Equip, index);
            UnityEventTools.AddIntPersistentListener(remove.onClick, wh.RemoveItem, index);
        }

        // ==== Helpers =======================================================

        private static RectTransform Canvas()
        {
            var go = GameObject.Find("LobbyCanvas");
            if (go == null) throw new System.Exception("LobbyCanvas not found");
            return go.GetComponent<RectTransform>();
        }

        private static GameObject NewUI(string name, Transform parent, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return go;
        }

        private static RectTransform Panel(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = NewUI(name, parent, x, y, w, h);
            var img = go.AddComponent<Image>();
            img.sprite = _roundSm;
            img.type = Image.Type.Sliced;
            img.color = C_CARD;
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI Label(Transform parent, string name, string text, float size, Color color,
            FontStyles style, TextAlignmentOptions align)
        {
            var go = NewUI(name, parent, 0, 0, 0, 0);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.font = _font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = align;
            t.raycastTarget = false;
            Stretch(t.rectTransform);
            return t;
        }

        private static void Place(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void Stretch(RectTransform rt, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static void ConvertToButton(GameObject go, Color bgColor, Color textColor)
        {
            var tmp = go.GetComponent<TMP_Text>();
            string text = tmp != null ? tmp.text : go.name;
            float size = tmp != null ? tmp.fontSize : 14f;
            var style = tmp != null ? tmp.fontStyle : FontStyles.Bold;
            var align = tmp != null ? tmp.alignment : TextAlignmentOptions.Center;
            if (tmp != null) Object.DestroyImmediate(tmp);

            var bg = go.GetComponent<Image>();
            if (bg == null) bg = go.AddComponent<Image>();
            bg.sprite = _roundSm;
            bg.type = Image.Type.Sliced;
            bg.color = bgColor;
            bg.raycastTarget = true;

            var btn = go.GetComponent<Button>();
            if (btn == null) btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            Normalize(btn);

            var label = Label(go.transform, "Label", text, size, textColor, style, align);
            Stretch(label.rectTransform);
        }

        private static void Normalize(Button b)
        {
            var c = b.colors;
            c.normalColor = Color.white;
            c.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
            c.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            c.selectedColor = Color.white;
            c.fadeDuration = 0.08f;
            c.colorMultiplier = 1f;
            b.colors = c;
        }

        private static Color AccentOf(Transform t)
        {
            Transform p = t;
            while (p != null)
            {
                if (p.name.Contains("Zombie")) return C_GREEN;
                if (p.name.Contains("Shooter")) return C_BLUE;
                if (p.name.Contains("Adventure")) return C_AMBER;
                p = p.parent;
            }
            return C_GREEN;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        private static void Save()
        {
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
        }

        private static void LoadAssets()
        {
            _font = TMP_Settings.defaultFontAsset;
            _roundSm = AssetDatabase.LoadAssetAtPath<Sprite>(UIDir + "/ui_round_sm.png");
            _roundLg = AssetDatabase.LoadAssetAtPath<Sprite>(UIDir + "/ui_round_lg.png");
            _circle = AssetDatabase.LoadAssetAtPath<Sprite>(UIDir + "/ui_circle.png");
            _ring = LoadOrCreateRing(UIDir + "/ui_ring.png");
        }

        private static Sprite LoadOrCreateRing(string path)
        {
            if (File.Exists(path)) return AssetDatabase.LoadAssetAtPath<Sprite>(path);

            const int size = 128;
            const float radius = 24f;
            const float thickness = 4f;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            float half = size * 0.5f;
            float pad = 1.5f;
            const float inset = 4f; // keep the ring inside the card mask
            float b0 = half - pad - inset - radius, r0 = radius;
            float b1 = b0 - thickness, r1 = Mathf.Max(1f, radius - thickness);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - half;
                    float dy = y + 0.5f - half;
                    float a0 = 0.5f - RoundedSdf(dx, dy, b0, r0);
                    float a1 = 0.5f - RoundedSdf(dx, dy, b1, r1);
                    float a = Mathf.Clamp01(a0) - Mathf.Clamp01(a1);
                    px[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
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
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.maxTextureSize = 256;
            ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static float RoundedSdf(float px, float py, float b, float r)
        {
            float qx = Mathf.Abs(px) - b;
            float qy = Mathf.Abs(py) - b;
            return Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f))
                   + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        }
    }
}
