using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RATIX
{
    /// <summary>
    /// One-time pass that renames every UI object and translates every visible
    /// label to English. Idempotent: running it again does nothing.
    /// It NEVER moves, resizes or re-parents anything.
    /// </summary>
    public static class RATIXEnglishPass
    {
        [MenuItem("Tools/RATIX/UI/3. English names and texts")]
        public static void Apply()
        {
            var canvas = GameObject.Find("LobbyCanvas");
            if (canvas == null)
            {
                Debug.LogError("[RATIX] LobbyCanvas not found.");
                return;
            }

            int renamed = 0;
            foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
            {
                string newName = NewName(t);
                if (newName != null && newName != t.name)
                {
                    t.name = newName;
                    renamed++;
                }
            }

            int translated = 0;
            foreach (var t in canvas.GetComponentsInChildren<TMP_Text>(true))
            {
                string src = t.text;
                if (string.IsNullOrWhiteSpace(src)) continue;

                if (Texts.TryGetValue(src, out string en))
                {
                    if (en != src) { t.text = en; translated++; }
                    continue;
                }

                if (src.StartsWith("Ultimo zombi:"))
                {
                    t.text = "Last survivor: Players are randomly assigned as zombie or human. Humans defend themselves with their weapons to survive each round. Zombies infect humans, and if you infect a human you may become human again or stay a zombie. Each round ends when all humans are infected, all zombies are killed, or time runs out.";
                    translated++;
                }
                else if (src.StartsWith("¡Bienvenido a RATIX!"))
                {
                    t.text = "Welcome to RATIX! Complete your first missions to earn exclusive rewards and discover everything the game has to offer.";
                    translated++;
                }
            }

            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[RATIX] English pass done: {renamed} objects renamed, {translated} texts translated.");
        }

        // ==== Object names ==================================================

        private static string NewName(Transform t)
        {
            switch (t.name)
            {
                case "Accesorios": return "AccessoriesLabel";
                case "Acesorios": return "AccessoriesPanel";
                case "Agenda": return "Schedule";
                case "Ajustes": return t.parent != null && t.parent.name == "BottomNav" ? "Settings" : "SettingsPanel";
                case "Almacén": return "Warehouse";
                case "Arma blanca": return "MeleeWeapon";
                case "Arma explosiva": return "ExplosiveWeapon";
                case "Arma Principal": return "PrimaryWeapon";
                case "Arma Principal (1)": return "WeaponSlotsBar";
                case "Arma Secundaria": return "SecondaryWeapon";
                case "Arma tactica": return "TacticalWeapon";
                case "AventuraButton": return "AdventureButton";
                case "AventuraModePanel": return "AdventureModePanel";
                case "Clasificación": return "Leaderboard";
                case "ClasificacionModal": return "LeaderboardModal";
                case "Correo": return "Mail";
                case "Create hall": return "CreateRoomPanel";
                case "CrearButton": return "CreateButton";
                case "CrearClanButton": return "CreateClanButton";
                case "Descripción": return "DescriptionLabel";
                case "Descripción_in": return "DescriptionInput";
                case "Eject": return "Kick";
                case "Enter": return "EnterRoom";
                case "Eventos": return "Events";
                case "EventosModal": return "EventsModal";
                case "HallList": return "RoomList";
                case "HallPanelShuter": return "ShooterHallPanel";
                case "HallPanelZombi": return "ZombieHallPanel";
                case "Halls": return "RoomsLabel";
                case "ID of hall": return "RoomID";
                case "Jugadores": return "PlayersLabel";
                case "Mascota": return "Pet";
                case "Mod": return "ModeLabel";
                case "Name of hall": return "RoomName";
                case "Name's clan": return "PlayerClan";
                case "Name's´player": return "PlayerName";
                case "Nombre del clan": return "ClanNameLabel";
                case "Nombre del clan_in": return "ClanNameInput";
                case "Personajes": return "CharacterButton";
                case "Ph": return "Placeholder";
                case "Salas": return "RoomsLabel";
                case "SalasList": return "RoomList";
                case "SecBienvenida": return "WelcomeSection";
                case "SecRegalo": return "GiftSection";
                case "SelectMap": return "SelectMapLabel";
                case "SelectMod": return "SelectModeLabel";
                case "Siglas": return "TagLabel";
                case "Siglas_in": return "TagInput";
                case "status's player()": return "PlayerStatus";
                case "Team red": return "TeamRed";
                case "Tienda": return "Store";
                case "vip": return "VipLabel";
                case "VolverButton": return "BackButton";
                case "ZombiButton": return "ZombieButton";
                case "ZombiModePanel": return "ZombieModePanel";
                case "check": return "QuickStartCheck";
                case "check (1)": return "ObserverCheck";
                case "Lupa": return "SearchIcon";
                case "search0": return "SearchLabel";
                case "search1": return "SearchPlaceholder";
                case "Cuchillo": return "Knife";
                case "Escopeta": return "Shotgun";
                case "Francotirador": return "Sniper";
                case "Metralladora": return "MachineGun";
                case "Pistola": return "Pistol";
                case "SubMetralladora": return "SubmachineGun";
                case "Tactica": return "Tactical";
                case "Todos": return "AllTypes";
            }
            return null;
        }

        // ==== Visible texts =================================================

        private static readonly Dictionary<string, string> Texts = new Dictionary<string, string>
        {
            { "50/60 miembros", "50/60 members" },
            { "Accesorios", "Accessories" },
            { "Actualizar", "Update" },
            { "Agenda", "Schedule" },
            { "Ajustes", "Settings" },
            { "Ajustes Adicionlales", "Additional settings" },
            { "Almacén", "Warehouse" },
            { "Antidoto", "Antidote" },
            { "Arma blanca", "Melee weapon" },
            { "Arma explosiva", "Explosive weapon" },
            { "Arma Principal", "Primary weapon" },
            { "Arma Secundaria", "Secondary weapon" },
            { "Arma Tactica", "Tactical weapon" },
            { "Atributo", "Attribute" },
            { "Aventura", "Adventure" },
            { "AVENTURA", "ADVENTURE" },
            { "Aventuras extremas", "Extreme adventures" },
            { "Básico", "Basic" },
            { "Bienvenida", "Welcome" },
            { "Buscar clan por nombre, ID o código de invitación", "Search clans by name, ID or invite code" },
            { "C4 - grupos", "C4 - groups" },
            { "Cambiar personaje", "Change character" },
            { "Campo", "Field" },
            { "Chaleco", "Vest" },
            { "Clasificación", "Leaderboard" },
            { "CLASIFICACIÓN", "LEADERBOARD" },
            { "Competencia en equipos", "Team competition" },
            { "Contraseña:", "Password:" },
            { "Correo", "Mail" },
            { "Crear", "Create" },
            { "CREAR", "CREATE" },
            { "CREAR CLAN", "CREATE CLAN" },
            { "Crear sala", "Create room" },
            { "Cura", "Medkit" },
            { "Descripción", "Description" },
            { "Entrar", "Enter" },
            { "Equipo Azul", "Blue Team" },
            { "Equipo Rojo", "Red Team" },
            { "Escribir aqui", "Type here" },
            { "Escribir aquí...", "Type here..." },
            { "Eventos", "Events" },
            { "EVENTOS", "EVENTS" },
            { "Explulsar", "Kick" },
            { "ID de la sala", "Room ID" },
            { "Inicio rápido:", "Quick start:" },
            { "Invitar", "Invite" },
            { "J/E", "P/S" },
            { "Juego rapido", "Quick match" },
            { "Jugador 1", "Player 1" },
            { "Jugador 2", "Player 2" },
            { "Jugador 3", "Player 3" },
            { "Jugador 4", "Player 4" },
            { "Jugador 5", "Player 5" },
            { "Jugador 6", "Player 6" },
            { "Jugador 7", "Player 7" },
            { "Jugador 8", "Player 8" },
            { "JUGADORES", "PLAYERS" },
            { "Jugadores:", "Players:" },
            { "Líder", "Leader" },
            { "Lupa", "Search" },
            { "lv.999", "Lv.999" },
            { "MAPA", "MAP" },
            { "Mapas y progreso próximamente", "Maps and progression coming soon" },
            { "Mascota", "Pet" },
            { "Miembro", "Member" },
            { "MIEMBROS (50/60)", "MEMBERS (50/60)" },
            { "Modo", "Mode" },
            { "Ms: 0", "Coins: 0" },
            { "Nombre de la sala", "Room name" },
            { "Nombre del clan", "Clan name" },
            { "Nombre/ID:", "Name/ID:" },
            { "Nombre:", "Name:" },
            { "Observador:", "Observer:" },
            { "Os: 0", "Gold: 0" },
            { "Pendiente", "Pending" },
            { "Refrescar", "Refresh" },
            { "Regalo por ser nuevo", "New player gift" },
            { "Reservado para observadores", "Reserved for observers" },
            { "Rondas:", "Rounds:" },
            { "Sala 1", "Room 1" },
            { "SALAS", "ROOMS" },
            { "SELECCIONA UN MODO", "SELECT A MODE" },
            { "Selecionar mapa", "Select map" },
            { "Selecionar modo", "Select mode" },
            { "Servidor", "Server" },
            { "Siglas", "Tag" },
            { "Siglas: [RATX]", "Tag: [RATX]" },
            { "Supervivencia zombi", "Zombie survival" },
            { "Tienda", "Store" },
            { "UNIRSE", "JOIN" },
            { "Volver", "Back" },
            { "VOLVER", "BACK" },
            { "Zombi", "Zombie" },
            { "ZOMBI", "ZOMBIE" },
            { "Zombi 1", "Zombie 1" },
            { "Zombi 2", "Zombie 2" },
            { "Zombi 3", "Zombie 3" },
            { "Zombi Normal", "Normal Zombie" },
            { "VIP10", "VIP10" },

            // Pasada 2: textos que quedaban en español.
            { "Cuchillo", "Knife" },
            { "Escopeta", "Shotgun" },
            { "Francotirador", "Sniper" },
            { "Metralladora", "Machine Gun" },
            { "Pistola", "Pistol" },
            { "SubMetralladora", "Submachine Gun" },
            { "Tactica", "Tactical" },
            { "Todos", "All" },
            { "SEGUIR", "FOLLOW" },
            { "gg, otra?", "gg, again?" },
            { "dale, una mas y corto", "fine, one more and I'm out" },
            { "entra a la room 1, hay 8", "join room 1, there are 8" },
            { "ya voy", "on my way" },
            { "Has recibido 0ro(200) X10", "You have received 200 gold x10" },
            { "Imagen del arma", "Weapon image" },
            { "ZombieShooter/Adventura", "Zombie / Shooter / Adventure" },
            { "CHAT · posición en juego (mueve este recuadro)", "CHAT · runtime position (move this box)" },
        };
    }
}
