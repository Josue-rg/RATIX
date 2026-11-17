using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace RATIX.EditorTools
{
    /// <summary>
    /// Genera el sistema de localización en la escena: pone TextLocalizer con un
    /// ID estable a cada texto estático y escribe en.json / es.json. No toca
    /// datos dinámicos (nombres, chats, contadores). Idempotente.
    /// Menú: RATIX ▸ Localización.
    /// </summary>
    public static class LocalizationTools
    {
        private const string CanvasName = "LobbyCanvas";
        private const string Folder = "Assets/RATIX/Resources/Localization";
        private const string EnPath = Folder + "/en.json";
        private const string EsPath = Folder + "/es.json";

        // Textos estáticos: valor en inglés → español.
        // Solo los textos que estén aquí reciben TextLocalizer.
        private static readonly Dictionary<string, string> Spanish = new Dictionary<string, string>
        {
            // Shell / lobby
            { "Friends", "Amigos" },
            { "Clan", "Clan" },
            { "Leaderboard", "Clasificación" },
            { "Events", "Eventos" },
            { "Warehouse", "Almacén" },
            { "Store", "Tienda" },
            { "Mail", "Correo" },
            { "Schedule", "Agenda" },
            { "Settings", "Ajustes" },
            { "Change character", "Cambiar personaje" },
            { "SELECT A MODE", "SELECCIONA UN MODO" },
            { "ZOMBIE", "ZOMBI" },
            { "Zombie survival", "Supervivencia zombi" },
            { "SHOOTER", "SHOOTER" },
            { "Team competition", "Competencia en equipos" },
            { "ADVENTURE", "AVENTURA" },
            { "Extreme adventures", "Aventuras extremas" },

            // Mode panels
            { "Quick match", "Juego rápido" },
            { "Create room", "Crear sala" },
            { "Global", "Global" },
            { "Server", "Servidor" },
            { "Basic", "Básico" },
            { "ROOMS", "SALAS" },
            { "PLAYERS", "JUGADORES" },
            { "Refresh", "Actualizar" },
            { "Search", "Buscar" },
            { "Name/ID:", "Nombre/ID:" },
            { "Type here", "Escribe aquí" },
            { "Mode", "Modo" },
            { "MAP", "MAPA" },
            { "Select mode", "Seleccionar modo" },
            { "Select map", "Seleccionar mapa" },
            { "Normal Zombie", "Zombi Normal" },
            { "Last survivor: Players are randomly assigned as zombie or human. Humans defend themselves with their weapons to survive each round. Zombies infect humans, and if you infect a human you may become human again or stay a zombie. Each round ends when all humans are infected, all zombies are killed, or time runs out.",
              "Último superviviente: los jugadores se asignan aleatoriamente como zombi o humano. Los humanos se defienden con sus armas para sobrevivir cada ronda. Los zombis infectan a los humanos, y si infectas a un humano puedes volver a ser humano o seguir siendo zombi. Cada ronda termina cuando todos los humanos están infectados, todos los zombis han muerto o se acaba el tiempo." },
            { "Accessories", "Accesorios" },
            { "Vest", "Chaleco" },
            { "Pending", "Pendiente" },
            { "Type here...", "Escribe aquí..." },
            { "Medkit", "Botiquín" },
            { "Antidote", "Antídoto" },
            { "Attribute", "Atributo" },
            { "Zombie 1", "Zombi 1" },
            { "Zombie 2", "Zombi 2" },
            { "Zombie 3", "Zombi 3" },
            { "JOIN", "UNIRSE" },
            { "Create", "Crear" },
            { "Additional settings", "Ajustes adicionales" },
            { "Name:", "Nombre:" },
            { "Rounds:", "Rondas:" },
            { "Players:", "Jugadores:" },
            { "Quick start:", "Inicio rápido:" },
            { "Observer:", "Observador:" },
            { "Password:", "Contraseña:" },
            { "Enter name", "Escribe un nombre" },
            { "Password", "Contraseña" },
            { "Base", "Base" },
            { "Field", "Campo" },
            { "Adventure", "Aventura" },
            { "Maps and progression coming soon", "Mapas y progreso próximamente" },
            { "Back", "Volver" },
            { "BACK", "VOLVER" },
            { "Room name", "Nombre de la sala" },
            { "Room ID", "ID de la sala" },

            // Hall
            { "Red Team", "Equipo Rojo" },
            { "Blue Team", "Equipo Azul" },
            { "RED TEAM", "EQUIPO ROJO" },
            { "BLUE TEAM", "EQUIPO AZUL" },
            { "Reserved for observers", "Reservado para observadores" },
            { "Primary weapon", "Arma principal" },
            { "Secondary weapon", "Arma secundaria" },
            { "Melee weapon", "Arma cuerpo a cuerpo" },
            { "Explosive weapon", "Arma explosiva" },
            { "Tactical weapon", "Arma táctica" },
            { "Pet", "Mascota" },
            { "Enter", "Entrar" },
            { "Kick", "Expulsar" },
            { "Invite", "Invitar" },
            { "Update", "Actualizar" },

            // Modals
            { "AGENDA", "AGENDA" },
            { "CLAN", "CLAN" },
            { "CREATE CLAN", "CREAR CLAN" },
            { "CREATE", "CREAR" },
            { "Search clans by name, ID or invite code", "Buscar clanes por nombre, ID o código de invitación" },
            { "Clan name", "Nombre del clan" },
            { "Description", "Descripción" },
            { "Tag", "Siglas" },
            { "MEMBERS", "MIEMBROS" },
            { "LEADER", "LÍDER" },
            { "OFFICER", "OFICIAL" },
            { "MEMBER", "MIEMBRO" },
            { "LEAVE CLAN", "SALIR DEL CLAN" },
            { "INVITE", "INVITAR" },
            { "FRIENDS", "AMIGOS" },
            { "EVENTS", "EVENTOS" },
            { "Welcome", "Bienvenida" },
            { "New player gift", "Regalo por ser nuevo" },
            { "Welcome to RATIX! Complete your first missions to earn exclusive rewards and discover everything the game has to offer.",
              "¡Bienvenido a RATIX! Completa tus primeras misiones para ganar recompensas exclusivas y descubrir todo lo que el juego ofrece." },
            { "Close", "Cerrar" },
            { "LEADERBOARD", "CLASIFICACIÓN" },
            { "TOP PLAYERS THIS SEASON", "MEJORES JUGADORES DE LA TEMPORADA" },
            { "Zombie", "Zombi" },
            { "Shooter", "Shooter" },
            { "SETTINGS", "AJUSTES" },
            { "GAMEPLAY", "JUGABILIDAD" },
            { "AUDIO", "AUDIO" },
            { "VIDEO", "VÍDEO" },
            { "LANGUAGE", "IDIOMA" },
            { "Language", "Idioma" },
            { "English", "Inglés" },
            { "Spanish", "Español" },
            { "Show damage numbers", "Mostrar números de daño" },
            { "Show FPS", "Mostrar FPS" },
            { "V-Sync", "V-Sync" },
            { "VSync", "VSync" },
            { "Display mode", "Modo de pantalla" },
            { "Effects", "Efectos" },
            { "Music", "Música" },
            { "Master volume", "Volumen general" },
            { "Quality", "Calidad" },
            { "APPLY", "APLICAR" },
            { "RESET", "RESTABLECER" },
            { "Fullscreen", "Pantalla completa" },
            { "Windowed", "Ventana" },
            { "Low", "Bajo" },
            { "Medium", "Medio" },
            { "High", "Alto" },
            { "Ultra", "Ultra" },
            { "LOW", "BAJO" },
            { "MEDIUM", "MEDIO" },
            { "HIGH", "ALTO" },
            { "ULTRA", "ULTRA" },

            // Profile / characters
            { "PROFILE", "PERFIL" },
            { "CAREER", "TRAYECTORIA" },
            { "MATCHES", "PARTIDAS" },
            { "K/D RATIO", "RATIO B/J" },
            { "ACCURACY", "PRECISIÓN" },
            { "ARMOR", "ARMADURA" },
            { "DAMAGE", "DAÑO" },
            { "SPEED", "VELOCIDAD" },
            { "PLAYTIME", "TIEMPO DE JUEGO" },
            { "RANK", "RANGO" },
            { "XP TO NEXT LEVEL", "XP PARA EL SIGUIENTE NIVEL" },
            { "CHANGE CHARACTER", "CAMBIAR PERSONAJE" },
            { "CHARACTERS", "PERSONAJES" },
            { "SELECT CHARACTER", "SELECCIONAR PERSONAJE" },
            { "PERKS", "VENTAJAS" },
            { "SELECTED", "SELECCIONADO" },
            { "OWNED", "EN PROPIEDAD" },
            { "LOCKED", "BLOQUEADO" },
            { "+10% SPEED", "+10% VELOCIDAD" },
            { "+15% ARMOR", "+15% ARMADURA" },
            { "ASSAULT", "ASALTO" },
            { "ASSAULT / CLOSE QUARTERS", "ASALTO / CORTA DISTANCIA" },
            { "RECON", "RECONOCIMIENTO" },
            { "MEDIC", "MÉDICO" },
            { "MARKSMAN", "TIRADOR" },
            { "SUPPORT", "APOYO" },
            { "STEALTH", "SIGILO" },
            { "TECH", "TÉCNICO" },
            { "HEAVY", "PESADO" },
            { "TROPHIES", "TROFEOS" },
            { "WINS", "VICTORIAS" },
            { "WAR STREAK", "RACHA DE GUERRA" },

            // Store / warehouse
            { "ITEM DETAILS", "DETALLES DEL OBJETO" },
            { "BUY", "COMPRAR" },
            { "Packages", "Paquetes" },
            { "Weapons", "Armas" },
            { "Skins", "Aspectos" },
            { "Items", "Objetos" },
            { "Characters", "Personajes" },
            { "EQUIP", "EQUIPAR" },
            { "PIN", "FIJAR" },
            { "Permanent", "Permanente" },
            { "COINS", "MONEDAS" },
            { "GOLD", "ORO" },
            { "Search item", "Buscar objeto" },
            { "All", "Todos" },
            { "Knife", "Cuchillo" },
            { "Shotgun", "Escopeta" },
            { "Sniper", "Francotirador" },
            { "Machine Gun", "Metralladora" },
            { "Pistol", "Pistola" },
            { "Submachine Gun", "Submetralladora" },
            { "Tactical", "Táctica" },
            { "The signature RATIX rifle. Balanced damage, control and a golden finish.", "El rifle insignia de RATIX. Daño, control y un acabado dorado equilibrados." },
            { "Legendary", "Legendario" },
            { "Elite", "Élite" },
            { "Gold", "Oro" },
            { "Activities", "Actividades" },
            { "Pass", "Pase" },
            { "Total:", "Total:" },

            // Mail
            { "Mails", "Correos" },
            { "Clear all", "Borrar todo" },
            { "Open All", "Abrir todo" },
            { "VIEW", "VER" },
            { "Open", "Abrir" },
            { "Details", "Detalles" },

            // Kick / Invite / Invite received
            { "KICK PLAYER", "EXPULSAR JUGADOR" },
            { "INVITE PLAYER", "INVITAR JUGADOR" },
            { "CLAN FRIENDS", "AMIGOS DEL CLAN" },
            { "KICK", "EXPULSAR" },
            { "GAME INVITE", "INVITACIÓN DE PARTIDA" },
            { "FROM", "DE" },
            { "MODE", "MODO" },
            { "ROOM", "SALA" },
            { "ACCEPT", "ACEPTAR" },
            { "DECLINE", "RECHAZAR" },

            // Login / splash
            { "SIGN IN TO CONTINUE", "INICIA SESIÓN PARA CONTINUAR" },
            { "USERNAME", "USUARIO" },
            { "PASSWORD", "CONTRASEÑA" },
            { "Enter your username", "Escribe tu usuario" },
            { "Enter your password", "Escribe tu contraseña" },
            { "LOG IN", "INICIAR SESIÓN" },
            { "PLAY AS GUEST", "JUGAR COMO INVITADO" },
            { "CREATE ACCOUNT", "CREAR CUENTA" },
            { "SURVIVE  ·  FIGHT  ·  PROGRESS", "SOBREVIVE  ·  LUCHA  ·  PROGRESA" },

            // Chat / friends
            { "Select a friend to open the chat", "Selecciona un amigo para abrir el chat" },
            { "Write a message...", "Escribe un mensaje..." },
            { "FOLLOW", "SEGUIR" },
            { "Playing:", "Jugando:" },
            { "Zombie / Shooter / Adventure", "Zombi / Shooter / Aventura" },
            { "CHAT · runtime position (move this box)", "CHAT · posición en juego (mueve este recuadro)" },
            { "Weapon image", "Imagen del arma" },
            { "In lobby", "En el lobby" },
            { "Playing Zombie", "Jugando Zombi" },
            { "Playing Shooter", "Jugando Shooter" },
            { "Playing Adventure", "Jugando Aventura" },
            { "Offline · 1d ago", "Desconectado · hace 1d" },
            { "Offline · 2h ago", "Desconectado · hace 2h" },

            // Character description
            { "Close-quarters specialist. Faster reload and extra resistance to infection.",
              "Especialista en combate cercano. Recarga más rápida y resistencia extra a la infección." }
        };

        // Claves que se usan desde el código (no están atadas a un texto de la escena).
        private static readonly Dictionary<string, (string en, string es)> RuntimeKeys = new Dictionary<string, (string, string)>
        {
            { "splash.loading", ("LOADING {0}%", "CARGANDO {0}%") },
            { "splash.ready", ("READY", "LISTO") },
            { "hall.selected_none", ("Selected: none", "Seleccionado: ninguno") },
            { "hall.selected_one", ("Selected: {0}", "Seleccionado: {0}") },
            { "hall.selected_players", ("Selected: {0} players", "Seleccionados: {0} jugadores") },
            { "hall.selected_friends", ("Selected: {0} friends", "Seleccionados: {0} amigos") },
            { "login.error_user", ("Enter your username to continue.", "Escribe tu usuario para continuar.") },
            { "value.normal_zombie", ("Normal Zombie", "Zombi Normal") },
            { "value.last_survivor", ("Last Survivor", "Último Superviviente") },
            { "value.evolution", ("Evolution", "Evolución") },
            { "value.c4_groups", ("C4 - groups", "C4 - grupos") },
            { "value.war", ("War", "Guerra") },
            { "value.p_vs_p", ("P vs P", "P vs P") }
        };

        [MenuItem("RATIX/Localización/1. Generar claves", false, 40)]
        public static void Generate()
        {
            var canvas = GameObject.Find(CanvasName);
            if (canvas == null)
            {
                Debug.LogError("[RATIX] No encuentro LobbyCanvas.");
                return;
            }

            var en = new Dictionary<string, string>();
            var es = new Dictionary<string, string>();
            var used = new HashSet<string>();
            var carouselLabels = new HashSet<TMP_Text>();
            int added = 0;

            // Los nombres de los carruseles los pone Carousel3D con claves "value.*".
            foreach (var carousel in canvas.GetComponentsInChildren<Carousel3D>(true))
            {
                var carouselSo = new SerializedObject(carousel);
                var selectionLabel = carouselSo.FindProperty("selectionLabel").objectReferenceValue as TMP_Text;
                if (selectionLabel == null) continue;

                carouselLabels.Add(selectionLabel);

                var oldLocalizer = selectionLabel.GetComponent<TextLocalizer>();
                if (oldLocalizer != null) Object.DestroyImmediate(oldLocalizer);
            }

            foreach (var text in canvas.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text == null || string.IsNullOrWhiteSpace(text.text)) continue;
                if (carouselLabels.Contains(text)) continue;

                // El texto que escribe el jugador no se localiza.
                var input = text.GetComponentInParent<TMP_InputField>();
                if (input != null && input.textComponent == text) continue;

                var path = PathOf(text.transform, canvas.transform);

                // La etiqueta del selector de idioma la controla LanguageSwitch.
                if (path == "SettingsModal/Window/LanguageChip/Label") continue;

                var localizer = text.GetComponent<TextLocalizer>();
                string key;

                if (localizer != null)
                {
                    key = localizer.Key;
                    if (string.IsNullOrEmpty(key)) key = Unique(Normalize(path), used);
                }
                else
                {
                    if (!Spanish.TryGetValue(text.text, out var spanish)) continue;

                    key = Unique(Normalize(path), used);
                    localizer = text.gameObject.AddComponent<TextLocalizer>();

                    var so = new SerializedObject(localizer);
                    so.FindProperty("key").stringValue = key;
                    so.FindProperty("target").objectReferenceValue = text;
                    so.ApplyModifiedPropertiesWithoutUndo();

                    added++;
                }

                used.Add(key);
                en[key] = text.text;
                es[key] = Spanish.TryGetValue(text.text, out var translated) ? translated : text.text;
            }

            foreach (var pair in RuntimeKeys)
            {
                en[pair.Key] = pair.Value.en;
                es[pair.Key] = pair.Value.es;
            }

            Directory.CreateDirectory(Folder);
            File.WriteAllText(EnPath, ToJson(en), new UTF8Encoding(false));
            File.WriteAllText(EsPath, ToJson(es), new UTF8Encoding(false));
            AssetDatabase.Refresh();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"[RATIX] Localización generada: {added} componentes nuevos, {en.Count} claves.");
        }

        [MenuItem("RATIX/Localización/2. Ver faltantes", false, 41)]
        public static void CheckMissing()
        {
            var en = ReadJson(EnPath);
            var es = ReadJson(EsPath);
            var missing = new List<string>();

            foreach (var key in en.Keys)
                if (!es.ContainsKey(key) || string.IsNullOrWhiteSpace(es[key])) missing.Add(key);

            Debug.Log(missing.Count == 0
                ? "[RATIX] Localización completa: todas las claves tienen español."
                : "[RATIX] Faltan " + missing.Count + " traducciones: " + string.Join(", ", missing));
        }

        // ==== Utilidades ====

        private static string PathOf(Transform transform, Transform root)
        {
            var parts = new List<string>();
            var current = transform;

            while (current != null && current != root)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }

        private static string Normalize(string path)
        {
            path = path.ToLowerInvariant();
            var sb = new StringBuilder();
            bool lastWasDot = true;

            foreach (char c in path)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                    lastWasDot = false;
                }
                else if (!lastWasDot)
                {
                    sb.Append('.');
                    lastWasDot = true;
                }
            }

            return sb.ToString().TrimEnd('.');
        }

        private static string Unique(string id, HashSet<string> used)
        {
            if (string.IsNullOrEmpty(id)) id = "text";
            if (!used.Contains(id)) return id;

            int n = 2;
            while (used.Contains(id + "." + n)) n++;
            return id + "." + n;
        }

        private static string ToJson(Dictionary<string, string> table)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");

            int i = 0;
            foreach (var pair in table)
            {
                sb.Append("  \"").Append(Escape(pair.Key)).Append("\": \"")
                  .Append(Escape(pair.Value)).Append('"');
                sb.AppendLine(++i < table.Count ? "," : string.Empty);
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private static Dictionary<string, string> ReadJson(string path)
        {
            var result = new Dictionary<string, string>();
            if (!File.Exists(path)) return result;

            string json = File.ReadAllText(path);
            int i = 0;

            while (i < json.Length)
            {
                int keyStart = json.IndexOf('"', i);
                if (keyStart < 0) break;

                string key = ReadString(json, ref keyStart);
                int colon = json.IndexOf(':', keyStart);
                if (colon < 0) break;

                int valueStart = json.IndexOf('"', colon + 1);
                if (valueStart < 0) break;

                result[key] = ReadString(json, ref valueStart);
                i = valueStart + 1;
            }

            return result;
        }

        private static string ReadString(string source, ref int index)
        {
            index++;
            var sb = new StringBuilder();

            while (index < source.Length)
            {
                char c = source[index++];
                if (c == '"') break;

                if (c == '\\' && index < source.Length)
                {
                    char escape = source[index++];
                    switch (escape)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case 'u':
                            if (index + 4 <= source.Length)
                            {
                                string hex = source.Substring(index, 4);
                                index += 4;
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                                    sb.Append((char)code);
                            }
                            break;
                        default: sb.Append(escape); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
