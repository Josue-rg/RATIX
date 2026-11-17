using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Lobby navigation: mode panels are authored at different positions in the
    /// canvas (like Figma frames) and are repositioned at runtime. Floating modals
    /// (Clan, Leaderboard, Events) open on top and close with the X button.
    /// </summary>
    public class LobbyController : MonoBehaviour
    {
        [Header("Panels (dispersed; repositioned at runtime)")]
        [SerializeField] private RectTransform modeSelectRect;
        [FormerlySerializedAs("zombiModeRect")]
        [SerializeField] private RectTransform zombieModeRect;
        [SerializeField] private RectTransform shooterModeRect;
        [FormerlySerializedAs("aventuraModeRect")]
        [SerializeField] private RectTransform adventureModeRect;
        [FormerlySerializedAs("hallZombiRect")]
        [SerializeField] private RectTransform zombieHallRect;
        [FormerlySerializedAs("hallShuterRect")]
        [SerializeField] private RectTransform shooterHallRect;
        [SerializeField] private Vector2 viewPosition = Vector2.zero;

        [Header("Modals")]
        [SerializeField] private RectTransform clanModal;
        [FormerlySerializedAs("clasificacionModal")]
        [SerializeField] private RectTransform leaderboardModal;
        [SerializeField] private RectTransform eventsModal;
        [SerializeField] private HallActionModal kickModal;
        [SerializeField] private HallActionModal inviteModal;

        [Header("More windows")]
        [SerializeField] private RectTransform settingsModal;
        [SerializeField] private RectTransform mailModal;
        [SerializeField] private RectTransform profileModal;
        [SerializeField] private RectTransform agendaModal;
        [SerializeField] private RectTransform charactersPanel;
        [SerializeField] private RectTransform friendsModal;
        [SerializeField] private RectTransform storePanel;
        [SerializeField] private RectTransform warehousePanel;
        [SerializeField] private InviteReceivedModal inviteReceivedModal;

        [Header("Clan sub-panels")]
        [SerializeField] private GameObject clanSearchPanel;
        [SerializeField] private GameObject clanCreatePanel;
        [SerializeField] private GameObject clanInsidePanel;

        [Header("Mode buttons (selector)")]
        [FormerlySerializedAs("zombiButton")]
        [SerializeField] private Button zombieButton;
        [SerializeField] private Button shooterButton;
        [FormerlySerializedAs("aventuraButton")]
        [SerializeField] private Button adventureButton;

        [Header("Shell")]
        [SerializeField] private Button clanButton;
        [FormerlySerializedAs("clasificacionButton")]
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button eventsButton;
        [FormerlySerializedAs("almacenButton")]
        [SerializeField] private Button warehouseButton;
        [FormerlySerializedAs("tiendaButton")]
        [SerializeField] private Button storeButton;
        [FormerlySerializedAs("ajustesButton")]
        [SerializeField] private Button settingsButton;

        [Header("Mapas")]
        [SerializeField] private MapLauncher zombieCreateLauncher;
        [SerializeField] private MapLauncher shooterCreateLauncher;
        [SerializeField] private MapLauncher zombieHallLauncher;
        [SerializeField] private MapLauncher shooterHallLauncher;

        /// <summary>Pantalla de contenido activa y pila para que "Back" vuelva al origen.</summary>
        private readonly System.Collections.Generic.List<RectTransform> contentHistory = new System.Collections.Generic.List<RectTransform>();
        private RectTransform currentContent;

        private void Start()
        {
            ResolveMissingReferences();

            // Mode selection.
            Wire(zombieButton, EnterZombie);
            Wire(shooterButton, EnterShooter);
            Wire(adventureButton, EnterAdventure);

            // Modals.
            Wire(clanButton, OpenClan);
            Wire(leaderboardButton, OpenLeaderboard);
            Wire(eventsButton, OpenEvents);
            Wire(warehouseButton, OpenWarehouse);
            Wire(storeButton, OpenStore);
            Wire(settingsButton, OpenSettings);
            Wire(FindButton("TopBar/Friends"), OpenFriends);
            Wire(FindButton("LeftColumn/BottomNav/Mail"), OpenMail);
            Wire(FindButton("LeftColumn/BottomNav/Schedule"), OpenAgenda);
            Wire(FindButton("LeftColumn/CharacterPanel/CharacterButton"), OpenCharacters);

            // Profile: clicking the player card in the left column.
            WireByPath(transform.Find("LeftColumn") as RectTransform, "PlayerCard", OpenProfile);

            // Close modals (X).
            WireByPath(clanModal, "Window/XButton", CloseClan);
            WireByPath(leaderboardModal, "Window/XButton", CloseLeaderboard);
            WireByPath(eventsModal, "Window/XButton", CloseEvents);
            WireByPath(eventsModal, "Window/Close", CloseEvents);

            // Clan states.
            WireByPath(clanModal, "Window/SearchPanel/CreateClanButton", ShowClanCreate);
            WireByPath(clanModal, "Window/CreatePanel/CreateButton", ShowClanInside);
            WireByPath(clanModal, "Window/CreatePanel/BackButton", ShowClanSearch);
            WireByPath(clanModal, "Window/InsidePanel/BackButton", ShowClanSearch);

            // Back buttons: vuelven a la pantalla donde estabas.
            WireByPath(zombieModeRect, "Header/Back", GoBack);
            WireByPath(shooterModeRect, "Header/Back", GoBack);
            WireByPath(adventureModeRect, "Header/Back", GoBack);
            WireByPath(zombieHallRect, "Header/Back", GoBack);
            WireByPath(shooterHallRect, "Header/Back", GoBack);
            WireByPath(storePanel, "Header/Back", GoBack);
            WireByPath(warehousePanel, "Header/Back", GoBack);
            WireByPath(settingsModal, "Window/XButton", CloseSettings);
            WireByPath(profileModal, "Window/XButton", CloseProfile);
            WireByPath(profileModal, "Window/CloseButton", CloseProfile);
            WireByPath(profileModal, "Window/ChangeCharacterButton", OpenCharacters);
            WireByPath(agendaModal, "Window/XButton", CloseAgenda);
            WireByPath(agendaModal, "Window/CloseButton", CloseAgenda);
            WireByPath(charactersPanel, "Header/Back", GoBack);
            WireByPath(friendsModal, "Window/XButton", CloseFriends);
            WireByPath(friendsModal, "Window/Chat/Header/RoomCard/Follow", OpenFriendsRoom);

            // Room click -> Hall.
            for (int i = 0; i < 7; i++)
            {
                WireByPath(zombieModeRect, "RoomList/Room" + i, EnterZombieHall);
                WireByPath(shooterModeRect, "RoomList/Room" + i, EnterShooterHall);
            }

            // Room list refresh.
            WireByPath(zombieModeRect, "RoomList/Search/Refresh", RefreshRooms);
            WireByPath(shooterModeRect, "RoomList/Search/Refresh", RefreshRooms);

            // Hall actions.
            WireByPath(zombieHallRect, "RoomList/ActionBar/Kick", OpenKick);
            WireByPath(zombieHallRect, "RoomList/ActionBar/Invite", OpenInvite);
            WireByPath(zombieHallRect, "RoomList/ActionBar/EnterRoom", () => Launch(zombieHallLauncher));
            WireByPath(shooterHallRect, "RoomList/ActionBar/Kick", OpenKick);
            WireByPath(shooterHallRect, "RoomList/ActionBar/Invite", OpenInvite);
            WireByPath(shooterHallRect, "RoomList/ActionBar/EnterRoom", () => Launch(shooterHallLauncher));

            // Crear sala -> carga el mapa elegido (con pantalla de carga).
            WireByPath(zombieModeRect, "CreateRoomPanel/Join", () => Launch(zombieCreateLauncher));
            WireByPath(shooterModeRect, "CreateRoomPanel/Join", () => Launch(shooterCreateLauncher));

            // Reposition dispersed panels.
            var panels = new[] { modeSelectRect, zombieModeRect, shooterModeRect, adventureModeRect, zombieHallRect, shooterHallRect, storePanel, warehousePanel, charactersPanel };
            foreach (var p in panels)
                if (p != null) p.anchoredPosition = viewPosition;

            // Modals always start closed, even if left open in the editor.
            CloseModal(clanModal);
            CloseModal(leaderboardModal);
            CloseModal(eventsModal);
            if (kickModal != null) kickModal.Close();
            if (inviteModal != null) inviteModal.Close();
            if (settingsModal != null) settingsModal.gameObject.SetActive(false);
            if (mailModal != null) mailModal.gameObject.SetActive(false);
            if (profileModal != null) profileModal.gameObject.SetActive(false);
            if (agendaModal != null) agendaModal.gameObject.SetActive(false);
            if (charactersPanel != null) charactersPanel.gameObject.SetActive(false);
            if (friendsModal != null) friendsModal.gameObject.SetActive(false);
            if (inviteReceivedModal != null) inviteReceivedModal.Close();

            ShowModeSelect();
        }

        /// <summary>
        /// Recovers inspector references that were lost while the UI was cloned.
        /// </summary>
        private void ResolveMissingReferences()
        {
            zombieButton ??= FindButton("Content/ModeSelectPanel/ZombieButton");
            shooterButton ??= FindButton("Content/ModeSelectPanel/ShooterButton");
            adventureButton ??= FindButton("Content/ModeSelectPanel/AdventureButton");

            clanButton ??= FindButton("TopBar/Clan");
            leaderboardButton ??= FindButton("TopBar/Leaderboard");
            eventsButton ??= FindButton("TopBar/Events");
            warehouseButton ??= FindButton("TopBar/Warehouse");
            storeButton ??= FindButton("TopBar/Store");
            settingsButton ??= FindButton("LeftColumn/BottomNav/Settings");

            if (clanModal == null) clanModal = FindRect("ClanModal");
            if (leaderboardModal == null) leaderboardModal = FindRect("LeaderboardModal");
            if (eventsModal == null) eventsModal = FindRect("EventsModal");
            if (kickModal == null) kickModal = FindComponent<HallActionModal>("KickModal");
            if (inviteModal == null) inviteModal = FindComponent<HallActionModal>("InviteModal");
            if (settingsModal == null) settingsModal = FindRect("SettingsModal");
            if (mailModal == null) mailModal = FindRect("MailModal");
            if (profileModal == null) profileModal = FindRect("ProfileModal");
            if (agendaModal == null) agendaModal = FindRect("AgendaModal");
            if (charactersPanel == null) charactersPanel = FindRect("Content/CharactersPanel");
            if (friendsModal == null) friendsModal = FindRect("FriendsModal");
            if (storePanel == null) storePanel = FindRect("Content/StoreModelPanel") ?? FindRect("Content/StoreModalPanel");
            if (warehousePanel == null) warehousePanel = FindRect("Content/WarehouseModelPanel") ?? FindRect("Content/WarehouseModalPanel");
            if (inviteReceivedModal == null) inviteReceivedModal = FindComponent<InviteReceivedModal>("InviteReceivedModal");

            if (zombieCreateLauncher == null) zombieCreateLauncher = FindComponent<MapLauncher>("Content/ZombieModePanel/CreateRoomPanel");
            if (shooterCreateLauncher == null) shooterCreateLauncher = FindComponent<MapLauncher>("Content/ShooterModePanel/CreateRoomPanel");
            if (zombieHallLauncher == null) zombieHallLauncher = FindComponent<MapLauncher>("Content/ZombieHallModelPanel/SettingsPanel");
            if (shooterHallLauncher == null) shooterHallLauncher = FindComponent<MapLauncher>("Content/ShooterHallModelPanel/SettingsPanel");
        }

        private T FindComponent<T>(string path) where T : Component
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<T>() : null;
        }

        private Button FindButton(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        private RectTransform FindRect(string path)
        {
            var t = transform.Find(path);
            return t as RectTransform;
        }

        private void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null) button.onClick.AddListener(action);
        }

        private void WireByPath(RectTransform panel, string path, UnityEngine.Events.UnityAction action)
        {
            if (panel == null) return;
            var t = panel.Find(path);
            if (t == null) return;
            var btn = t.GetComponent<Button>();
            if (btn == null)
            {
                btn = t.gameObject.AddComponent<Button>();
                var img = t.GetComponent<Image>();
                if (img != null) btn.targetGraphic = img;
                foreach (var g in t.GetComponentsInChildren<Graphic>(true))
                    if (g.transform != t) g.raycastTarget = false;
            }
            btn.onClick.AddListener(action);
        }

        private void Log(string message) => Debug.Log("[RATIX] " + message);

        // ==== Panel navigation ====

        public void EnterZombie() => ShowContent(zombieModeRect);
        public void EnterShooter() => ShowContent(shooterModeRect);
        public void EnterAdventure() => ShowContent(adventureModeRect);
        public void EnterZombieHall() => ShowContent(zombieHallRect);
        public void EnterShooterHall() => ShowContent(shooterHallRect);
        public void BackToZombie() => ShowContent(zombieModeRect);
        public void BackToShooter() => ShowContent(shooterModeRect);
        public void ShowModeSelect() => ShowContent(modeSelectRect);

        private void ShowContent(RectTransform active) => ShowContent(active, true);

        private void ShowContent(RectTransform active, bool remember)
        {
            if (active == null) return;

            // Guarda de dónde vienes para que "Back" te devuelva ahí y no al selector.
            if (remember && currentContent != null && currentContent != active)
                contentHistory.Add(currentContent);
            currentContent = active;

            if (modeSelectRect != null) modeSelectRect.gameObject.SetActive(active == modeSelectRect);
            if (zombieModeRect != null) zombieModeRect.gameObject.SetActive(active == zombieModeRect);
            if (shooterModeRect != null) shooterModeRect.gameObject.SetActive(active == shooterModeRect);
            if (adventureModeRect != null) adventureModeRect.gameObject.SetActive(active == adventureModeRect);
            if (zombieHallRect != null) zombieHallRect.gameObject.SetActive(active == zombieHallRect);
            if (shooterHallRect != null) shooterHallRect.gameObject.SetActive(active == shooterHallRect);
            if (storePanel != null) storePanel.gameObject.SetActive(active == storePanel);
            if (warehousePanel != null) warehousePanel.gameObject.SetActive(active == warehousePanel);
            if (charactersPanel != null) charactersPanel.gameObject.SetActive(active == charactersPanel);
        }

        /// <summary>Vuelve a la pantalla anterior (modo o sala); si no hay historial, al selector.</summary>
        public void GoBack()
        {
            while (contentHistory.Count > 0)
            {
                var previous = contentHistory[contentHistory.Count - 1];
                contentHistory.RemoveAt(contentHistory.Count - 1);
                if (previous == null) continue;

                ShowContent(previous, false);
                return;
            }

            ShowContent(modeSelectRect, false);
        }

        // ==== Modals ====

        public void OpenClan()
        {
            ShowClanSearch();
            OpenModal(clanModal);
        }

        public void OpenLeaderboard() => OpenModal(leaderboardModal);
        public void OpenEvents() => OpenModal(eventsModal);

        public void OpenKick()
        {
            if (kickModal != null) kickModal.Open();
        }

        public void OpenInvite()
        {
            if (inviteModal != null) inviteModal.Open();
        }

        public void EnterMatch() => Log("Starting match... (gameplay scene not implemented yet)");

        /// <summary>Lanza el mapa elegido con la pantalla de carga.</summary>
        private void Launch(MapLauncher launcher)
        {
            if (launcher != null) launcher.Launch();
        }

        /// <summary>
        /// Muestra el popup de invitación recibida (misma posición que el correo).
        /// Lo llamará el sistema de red cuando otro jugador invite.
        /// </summary>
        public void OpenInviteReceived(string inviter, string mode, string room, string map)
        {
            if (inviteReceivedModal == null) inviteReceivedModal = FindComponent<InviteReceivedModal>("InviteReceivedModal");
            if (inviteReceivedModal != null) inviteReceivedModal.Open(inviter, mode, room, map);
        }

        public void RefreshRooms() => Log("Refreshing room list...");

        // ==== Extra windows ====

        public void OpenSettings()
        {
            if (settingsModal == null) return;
            settingsModal.anchoredPosition = Vector2.zero;
            settingsModal.gameObject.SetActive(true);
            settingsModal.SetAsLastSibling();
        }

        public void CloseSettings()
        {
            if (settingsModal != null) settingsModal.gameObject.SetActive(false);
        }

        public void OpenMail()
        {
            if (mailModal == null) return;
            mailModal.anchoredPosition = new Vector2(0f, 77f);
            mailModal.gameObject.SetActive(true);
            mailModal.SetAsLastSibling();
        }

        public void CloseMail()
        {
            if (mailModal != null) mailModal.gameObject.SetActive(false);
        }

        // ==== Profile / Agenda / Characters ====

        public void OpenProfile()
        {
            CloseAgenda();
            OpenModal(profileModal);
        }

        public void CloseProfile() => CloseModal(profileModal);

        public void OpenAgenda()
        {
            CloseProfile();
            OpenModal(agendaModal);
        }

        public void CloseAgenda() => CloseModal(agendaModal);

        public void OpenCharacters()
        {
            CloseProfile();
            CloseAgenda();
            if (charactersPanel != null) charactersPanel.anchoredPosition = viewPosition;
            ShowContent(charactersPanel);
        }

        // ==== Friends ====

        public void OpenFriends()
        {
            CloseProfile();
            CloseAgenda();
            OpenModal(friendsModal);
        }

        public void CloseFriends() => CloseModal(friendsModal);

        /// <summary>Temporal: "seguir" a un amigo lleva a la sala de ejemplo (hall).</summary>
        public void OpenFriendsRoom()
        {
            CloseFriends();
            ShowContent(zombieHallRect);
        }

        public void OpenStore() => ShowContent(storePanel);
        public void OpenWarehouse() => ShowContent(warehousePanel);

        public void CloseClan() => CloseModal(clanModal);
        public void CloseLeaderboard() => CloseModal(leaderboardModal);
        public void CloseEvents() => CloseModal(eventsModal);

        private void OpenModal(RectTransform modal)
        {
            if (modal == null) return;
            modal.anchoredPosition = Vector2.zero;
            modal.gameObject.SetActive(true);
            modal.SetAsLastSibling(); // keep it above everything else
        }

        private void CloseModal(RectTransform modal)
        {
            if (modal != null) modal.gameObject.SetActive(false);
        }

        // ==== Clan states ====

        public void ShowClanSearch() => SetClanPanel(clanSearchPanel);
        public void ShowClanCreate() => SetClanPanel(clanCreatePanel);
        public void ShowClanInside() => SetClanPanel(clanInsidePanel);

        private void SetClanPanel(GameObject active)
        {
            if (clanSearchPanel != null) clanSearchPanel.SetActive(active == clanSearchPanel);
            if (clanCreatePanel != null) clanCreatePanel.SetActive(active == clanCreatePanel);
            if (clanInsidePanel != null) clanInsidePanel.SetActive(active == clanInsidePanel);
        }
    }
}
