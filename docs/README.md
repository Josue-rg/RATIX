# RATIX

RATIX es una plataforma multijugador centrada en **tiroteos, supervivencia y
progresión**, dividida en:

- **PvP asimétrico** → *Zombi*
- **PvP competitivo** → *Shooter*
- **PvE cooperativo/solo** → *Aventura*

---

## Modos de juego

### 1. Zombi
Trata de una sala donde los jugadores son aleatoriamente zombis y tienen que
infectar a los demás, que son humanos, mientras estos se defienden con armas.

### 2. Shooter
Estilo básico: sala donde hay 2 equipos y se pelean con armas.

### 3. Aventura
Modo aventura, lugar donde los jugadores pueden jugar solos o en equipo y jugar
mapas, para conocer el porqué de los zombis pasando múltiples mapas, y mejorando
armas y diferentes cosas.

> **Nota:** de cada uno de los modos existirán más variantes, que serán
> implementadas después.

---

## Estado actual del proyecto

- Motor: **Unity 6 (6000.6.0f1)** con **URP** y el nuevo **Input System**.
- **Lobby** (`Assets/RATIX/Scenes/Lobby.unity`) con:
  - **Selector de modo** (Zombi / Shooter / Aventura).
  - **Paneles de modo**: lista de salas + **Juego rápido** (carrusel de modo,
    descripción, accesorios, UNIRSE) + **Crear sala** (selección de modo y mapa,
    ajustes: nombre, rondas, jugadores, contraseña, etc.).
  - **Hall** (dentro de una sala): equipos, jugadores y **armas siempre visibles**
    + panel **Ajustes**.
  - **Modales flotantes** (se cierran con X): **Clan** (buscar / crear / dentro
    del clan), **Clasificación** (ranking por modo) y **Eventos** (secciones).
- TextMeshPro importado (fuente por defecto LiberationSans).
- **Entrada del juego**: `SplashScreen` (logo RATIX + barra, 3.5 s) → `LoginScreen`
  → lobby. Lo controla `AppFlowController` en `LobbyCanvas`; el login es visual
  (sin servidor) y avisa con el evento `LoggedIn`.

---

## Convenciones de trabajo (Unity)

1. **Paneles dispersos (como frames de Figma):** cada pantalla/panel está en una
   posición distinta de la escena para editarlo por separado; `LobbyController`
   los **reposiciona a la vista** y alterna su visibilidad en runtime.
   (Procedimiento obligatorio en la **Regla 5**.)
2. **Modales:** ventana flotante con fondo oscurecido + botón **X ó Cerrar** para cerrar.
3. **Copiar estilo:** para replicar una interfaz se **clona** el box de referencia
   (no se rehace); los textos se ajustan después.
4. **Edición manual en el editor + MCP:** el diseño se edita a mano en Unity; el
   MCP (`unity`) se usa para conectar, generar y verificar.

---

## Reglas importantes

### 1. Uso prioritario del Editor Visual (Scene-based Builder)
Todos los elementos visuales, escenarios, menús y personajes se crean como
**assets / prefabs / escenas** dentro del editor visual del motor, **NO**
generados dinámicamente al 100 % por código.

### 2. Separación entre Lógica y Diseño (Patrón MVC o Componentes)
El código solo debe controlar el **comportamiento** (la lógica), mientras que la
apariencia, posiciones y componentes visuales deben ser editables desde la
interfaz gráfica.

### 3. Parámetros editables (Inspectables)
Todas las variables de ajuste (daño de armas, velocidad del jugador, salud de
los zombis) deben estar expuestas en el **Inspector** del motor para poder
equilibrar el juego sin modificar código.

### 4. Estructura limpia para equipo/venta (Clean Code & Modular)
El proyecto debe seguir una **estructura de carpetas estándar y documentada**,
apta para que cualquier desarrollador externo pueda abrirlo, entenderlo y
modificarlo.

### 5. Interfaces separadas en el editor, flujo correcto en runtime

**Regla obligatoria al crear cualquier interfaz nueva.** Toda pantalla, modal o
sub-interfaz se crea **dentro de `LobbyCanvas`** y se **estaciona en una zona
libre de la escena**, nunca encima del lobby ni encima de otra interfaz.

La escena funciona como un tablero de Figma: la posición que ves en el editor es
solo de autoría. Al dar **Play**, el controlador trae la interfaz a su sitio, la
pone encima y la muestra. Así cada interfaz se puede editar por separado y,
aun así, el flujo en juego es el correcto.

> ❌ Prohibido: apilar interfaces una sobre otra en el área visible del lobby.
>
> ❌ Prohibido: crear la interfaz instanciándola por código en runtime.
>
> ✅ Correcto: se crea y se edita en la escena, estacionada aparte; el script
> solo la **abre/cierra y la reposiciona**.

#### Atajos de creación (RATIX ▸ UI)

Para no repetir el trabajo manual, el editor trae un kit en el menú **RATIX ▸ UI**.
Solo **crea** objetos nuevos: no modifica ni repinta interfaces existentes.

| Opción | Qué hace |
|---|---|
| **Crear modal vacío** | Modal 1920 × 1080 con fondo oscuro, `Window` de 900 × 620, título, X y área de contenido. Ya sale con los sprites en `Sliced` y el estacionamiento asignado |
| **Crear panel de contenido vacío** | Panel 1460 × 1008 dentro de `Content`, con header (Back + título) y cuerpo |
| **Duplicar fila seleccionada** | Clona la fila elegida, la renombra (`Room3` → `Room4`) y la baja el alto + 8 px |
| **Ver siguiente hueco (modal / panel)** | Muestra la coordenada libre para no montar interfaces encima de otras |

Elegir el hueco es automático: el kit mide las interfaces grandes que ya existen y
toma la primera coordenada libre de la rejilla.

#### 5.1 Dónde se crea cada tipo

| Tipo | Padre en la jerarquía | Tamaño | Zona de parking |
|---|---|---|---|
| Pantalla de contenido (modo, hall, tienda, almacén, personajes) | `LobbyCanvas/Content` | 1460 × 1008 | Rejilla a la derecha y abajo del lobby |
| Modal flotante (Clan, Leaderboard, Events, Settings, Profile, Agenda, Kick, Invite) | `LobbyCanvas` | 1920 × 1080 | Alrededor del canvas (izquierda, arriba, derecha) |
| Popup lateral (Mails) | `LobbyCanvas` | 460 × 421 | Debajo del canvas |

#### 5.2 RectTransform estándar

- **Raíz de pantalla / modal:** `Anchor Min/Max = 0,0` · `Pivot = 0,0` ·
  `Size = 1920 × 1080` (o el tamaño del popup) · `Pos = coordenada de parking`.
- **Contenedor `Window`** (hijo de la raíz): `Anchor = 0.5 / 0.5` ·
  `Pivot = 0.5 / 0.5` · `Pos = 0,0` · tamaño de la ventana (ej. 900 × 620).
  Como está centrado por ancla, la ventana se ve bien sin importar dónde esté
  estacionada la raíz.
- **Contenido:** el fondo oscuro a pantalla completa vive en la **raíz**; dentro
  del `Window` van `Title`, `XButton`, el contenido y los botones.
- **Imágenes con `ui_round_*`:** el sprite tiene borde 9-slice, así que la
  `Image` debe estar en **Type = Sliced** (`Pixels Per Unit Multiplier = 1`).
  En `Simple` las esquinas se estiran y el modal se ve deforme.

#### 5.3 Interfaces y sub-interfaces

Hay dos formas válidas; se elige según si comparten marco:

1. **Sub-pantalla dentro del mismo modal** (comparte fondo, título y X):
   todos los paneles son hijos de `Window` y el controlador activa uno solo.
   Ejemplo: `ClanModal/Window > SearchPanel, CreatePanel, InsidePanel`.
2. **Ventana flotante encima de otra** (es un modal por sí misma):
   se crea su **propia raíz estacionada en otra zona** y se abre encima con
   `SetAsLastSibling()`.
   Ejemplo: `MailModal/Details`, `KickModal`, `InviteModal`.

Regla práctica: **si reutiliza el marco del modal → opción 1; si es otra ventana
flotante → opción 2.** En ningún caso se pone una interfaz encima del lobby en el
editor para "ver cómo queda".

#### 5.4 Mapa de parking actual y accesos

Canvas de referencia 1920 × 1080, ancla y pivot `0,0`:

| Objeto | Posición en el editor |
|---|---|
| Lobby (vista real) | `0, 0` |
| `EventsModal` | `−2037, 0` |
| `SettingsModal` | `−2037, 1120` |
| `ProfileModal` | `−2037, 2200` |
| `AgendaModal` | `−2037, 3320` |
| `ClanModal` | `0, 2200` |
| `LeaderboardModal` | `0, 1120` |
| `KickModal` | `2006, 2109` |
| `InviteModal` | `2006, 3280` |
| `FriendsModal` | `2006, 0` |
| `MailModal` | `0, −421.88` (se abre en `0, 77`) |
| `InviteReceivedModal` | `0, −880` (se abre en `0, 77`) |
| `SplashScreen` | `0, 3320` (se muestra a pantalla completa al iniciar) |
| `LoginScreen` | `0, 5560` (se muestra después del splash) |
| `ZombieModePanel` | `1546, 0` |
| `ShooterModePanel` | `3557, 0` |
| `AdventureModePanel` | `5565, 0` |
| `ZombieHallModelPanel` | `1546, 1076` |
| `ShooterHallModelPanel` | `3557, 1076` |
| `StoreModelPanel` | `1546, −1074` |
| `WarehouseModelPanel` | `1546, −2135` |
| `CharactersPanel` | `3557, −1074` |

Qué abre cada cosa:

| Acción | Interfaz |
|---|---|
| Clic en `LeftColumn/PlayerCard` | `ProfileModal` |
| `LeftColumn/BottomNav/Schedule` | `AgendaModal` |
| `LeftColumn/BottomNav/Settings` | `SettingsModal` |
| `TopBar/Friends` | `FriendsModal` (lista + chat; `SEGUIR` lleva al hall) |
| `LeftColumn/CharacterPanel/CharacterButton` | `CharactersPanel` |
| `ProfileModal > CHANGE CHARACTER` | `CharactersPanel` |
| `CharactersPanel > Header/Back` | pantalla anterior |

Los paneles de contenido se reposicionan a la vista en `Start()` (array `panels`)
y `ShowContent()` alterna su visibilidad; los modales se abren con `OpenModal()`
(posición cero + `SetAsLastSibling`).

**Guías de posición (solo editor):** para las interfaces que se editan
estacionadas pero se colocan por código (como el chat de `FriendsModal`) hay un
recuadro `RuntimeGuide` dentro del modal, en la coordenada exacta de llegada.
Se ve solo en el editor (al dar Play se apaga solo): se mueve el recuadro y la
interfaz aparece ahí. Ejemplo: `FriendsModal/Window/ChatGuide` marca dónde cae
`FriendsModal/Window/Chat`; `FriendsPanel` lee su `anchoredPosition` al abrir el
chat. Lo mismo aplica a cualquier otra interfaz que necesite una guía.

**Invitación recibida:** `InviteReceivedModal` es el popup que aparece cuando
otro jugador invita (misma posición que el correo, `0, 77`). Se abre con
`LobbyController.OpenInviteReceived(inviter, mode, room, map)` y expone
`Answered` / `Accepted` / `Declined` para que la capa de red responda.
En `KickModal` e `InviteModal` se pueden seleccionar **varias filas** (clic para
marcar o desmarcar); la acción se aplica a todas las elegidas.

**"Back" recuerda de dónde vienes:** `ShowContent()` guarda la pantalla actual en
una pila (`contentHistory`) y `GoBack()` la restaura. Por eso, si estás dentro de
una sala o de un modo y te vas a Tienda o Almacén, el **Back** te devuelve a esa
sala / modo y no al selector. Todos los botones `Header/Back` llaman a `GoBack()`.

#### 5.5 Runtime: así se junta el flujo

```csharp
// Abrir: del parking a la vista, encima de todo y visible.
modal.anchoredPosition = Vector2.zero;
modal.SetAsLastSibling();
modal.gameObject.SetActive(true);

// Cerrar
modal.gameObject.SetActive(false);
```

- En `Start()` **todos los modales se cierran**, aunque en el editor estén
  abiertos (`LobbyController` ya lo hace): el estado del editor no debe afectar
  al juego.
- El campo del modal se expone con `[SerializeField]` en el controlador y, si
  aplica, se recupera con `transform.Find` para no romper la referencia al
  clonar. Ejemplo: `[SerializeField] private RectTransform friendsModal;`
- Si la interfaz tiene un punto de reposo propio (como el detalle del correo),
  ese valor va como campo serializado editable en el Inspector
  (`MailPanel.detailsHomePosition`), no escrito a mano en el layout del editor.
- Los sub-paneles también arrancan en su estado inicial (cerrados o en la
  primera pestaña) desde el controlador.

#### 5.6 Checklist antes de dar Play

1. ¿Está creada como hijo de `LobbyCanvas` (o de `Content`) y **fuera** del área
   del lobby?
2. ¿Raíz con `Anchor 0,0 / 0,0` y `Pivot 0,0`, y el `Window` centrado en
   `0.5 / 0.5`?
3. ¿El lobby quedó limpio, sin la interfaz tapando otras?
4. ¿El controlador tiene su `[SerializeField]` y sus métodos `Open` / `Close`?
5. ¿Está incluida en la lista de "cerrados al inicio" de `Start()`?
6. ¿El botón que la abre está cableado (TopBar, BottomNav o el panel dueño)?
7. ¿Abrirla dos veces o con otro modal abierto sigue viéndose bien
   (`SetAsLastSibling`)?
8. ¿Escena guardada?

### 6. Assets pesados, Git LFS y archivos locales

**Estado actual:** el repositorio guarda **todo como archivo normal de Git**.
Git LFS está **desactivado** en `.gitattributes` porque la cuenta superó su
presupuesto de LFS y GitHub rechazaba cada `push` con *"This repository exceeded
its LFS budget"*. El proyecto pesa ~8 MB, así que todavía no lo necesita.

**Límites de GitHub:**
- Archivos de **más de 50 MB**: GitHub avisa (el repo se hincha para siempre).
- Archivos de **más de 100 MB**: GitHub los **bloquea** y el push falla.

**Antes de importar mapas, personajes o packs de texturas pesados:**
1. Si el archivo supera ~50 MB, **no** lo subas al repo: guárdalo fuera
   (Drive / almacenamiento externo) y anótalo en el README del modo.
2. Si se quiere versionar: arreglar el presupuesto en
   **GitHub → Settings → Billing → Git LFS**, descomentar en `.gitattributes`
   solo las extensiones necesarias y traer los archivos con
   `git lfs migrate import --include="*.fbx,*.blend"`.
3. Flujo recomendado de modelado: trabajar el `.blend` fuera del repo y subir
   solo el `.fbx` exportado y liviano dentro de `Assets/`.

**Capturas y temporales:** `/Screenshots/` está en `.gitignore`; las capturas de
trabajo nunca se suben. `Library/`, `Temp/`, `Logs/` y `obj/` también quedan
fuera del control de versiones.

---

## Despliegue web (estado actual)

**Activo:** <https://josue-rg.github.io/RATIX/>
GitHub Pages · rama `main` · carpeta `/ (root)` · sin Jekyll (`.nojekyll`).

El lobby corre como **WebGL**. El build vive en la **raíz** del repo y es lo único
que Pages publica:

| Ruta | Qué es |
|---|---|
| `index.html` | arranque del player |
| `Build/` | `WebGLBuild.loader.js`, `.data.unityweb`, `.wasm.unityweb`, `.framework.js.unityweb` |
| `TemplateData/` | estilo, favicon e imágenes del loader |
| `Data/` | `lib_burst_generated` (lo genera el build) |
| `.nojekyll` | evita el procesado Jekyll |

- **Peso del build actual: 13.3 MB** (wasm 9.5 MB · data 4.1 MB · resto ~1 MB).
- **Compresión: Gzip + Decompression Fallback.** Es obligatorio en este host:
  con Brotli sin fallback la página se queda en la barra de carga.

### Pantalla y escalado (letterbox 16:9)

El lobby está diseñado a **1920 × 1080**. En la web el canvas **nunca** se muestra a
tamaño fijo: se escala para **caber completo** en la ventana manteniendo la
proporción 16:9 y, si la pantalla no es 16:9 (portátil 16:10, ventana estrecha),
quedan **franjas negras** alrededor. Así se ve igual en el monitor que en la
laptop, sin recortes ni scroll.

- El tamaño se calcula en `fitCanvasToWindow()` dentro de `index.html`
  (`Math.min(anchoVentana/1920, alturaVentana/1080)`) y se recalcula en
  `resize`, `orientationchange` y `fullscreenchange`.
- La fuente de verdad es la **plantilla propia**
  `Assets/WebGLTemplates/RATIX/` (`index.html` + `TemplateData/`), y
  `RATIXWebGLBuilder` la aplica con
  `PlayerSettings.WebGL.template = "PROJECT:RATIX"`.
- ⚠️ **No editar a mano el `index.html` de la raíz para cambiar el tamaño:** se
  regenera en cada build. Cualquier cambio va en la plantilla.
- El botón de pantalla completa usa `fullscreenElementID: "unity-container"`
  para que el letterbox también se respete en fullscreen.
- Los `CanvasScaler` del lobby siguen en **Scale With Screen Size · 1920×1080 ·
  Match 0.5**; con el canvas siempre en 16:9 no hace falta tocarlos.

| Ventana del navegador | Canvas resultante |
|---|---|
| 1366 × 768 | 1297 × 730 |
| 1536 × 730 | 1230 × 692 |
| 1920 × 1080 | 1852 × 1042 |
| 1280 × 800 (16:10) | 1280 × 720 + barras |

### Al publicar cambios

```
RATIX ▸ WebGL ▸ Construir para GitHub Pages
git add index.html Build TemplateData Data
git commit -m "build web"
git push
```

Pages reconstruye solo en 1-3 min (pestaña **Actions** → *pages build and
deployment*).

### Reglas y advertencias

1. **Todo cambio que quieras ver en la web necesita build antes del push.** Si
   solo empujas código, la web sigue mostrando el build anterior.
2. **No cambiar la compresión del build** a Brotli sin fallback (rompe la carga).
3. **No borrar `.nojekyll`** ni las excepciones del final del `.gitignore`
   (`!/Build/`, `!/Build/**`, `!/TemplateData/**`): sin ellas el build no se sube.
4. **Tamaño:** el sitio publicado puede llegar a ~1 GB; el problema real es el
   **historial de Git**, que crece ~13 MB por build. Regla práctica: **un build
   por commit** y construir solo cuando haya algo que probar. Si compilas dos
   veces seguidas sin publicar: `git commit --amend` en vez de un commit nuevo.
5. **Nunca** subir `Library/`, `Temp/`, `Logs/` ni `Screenshots/`.
6. GitHub **bloquea archivos de más de 100 MB** y avisa desde 50 MB. Si un build
   pasa de ~50 MB, revisar texturas y audio antes de subirlo.
7. El proyecto queda en **plataforma WebGL** tras construir. Para volver a
   Windows: *File ▸ Build Profiles ▸ Windows ▸ Switch Platform*. Con la escena
   abierta el Play normal funciona igual.
8. Si la web no muestra el cambio, primero **Ctrl + F5** (el loader cachea con
   IndexedDB) y después mirar *Actions*.
9. La URL lleva el nombre del repo en mayúsculas: `/RATIX/`.

---

## Paletas por modo

Dirección de arte: **base neutra casi negra + un acento contenido por modo**. Los
paneles no se rellenan con el color del modo; solo se tiñen un ~6 % y el color
aparece en la barra, el título, la pestaña activa, los anillos del carrusel y el
botón principal. Esa disciplina es lo que hace que el lobby se vea serio y no
"infantil".

### Base compartida (todos los modos)

| Rol | Color |
|---|---|
| Fondo de panel | `#0A0E13` |
| Superficie / tarjeta | `#141C26` · `#1A242F` |
| Filas y chips | `#1A242F` · `#202C38` · `#273442` |
| Texto principal | `#E8EDF4` |
| Texto secundario | `#93A2B5` |
| VIP / oro | `#D8A74E` |
| Equipo rojo / azul | `#CE5A46` / `#6B9BC9` |

### Acentos

| Modo | Acento (detalles y texto) | Botón / pestaña activa | Texto sobre botón |
|---|---|---|---|
| Zombi | `#6CA578` | `#4E7C57` | `#F2F7F2` |
| Shooter | `#5B9FB8` | `#1F6180` | `#EAF5FA` |
| Aventura | `#C9A15B` | `#6B5027` | `#F7F1E4` |

Regla: el acento claro va solo en elementos **pequeños**; las superficies grandes
(paneles, tarjetas, botones) usan el tono profundo. Así el color guía la vista sin
gritar.

**Dónde se aplican:** `Content/ModeSelectPanel/ZombieButton`, `ShooterButton`,
`AdventureButton`, `Content/ZombieModePanel`, `ShooterModePanel`,
`AdventureModePanel`, `ZombieHallModelPanel` y `ShooterHallModelPanel`. Si se
re-ejecuta `RATIXUITheme` (tema del editor), vuelve a pintar con la paleta azul
por defecto: hay que reaplicar estos colores después.

---

## Instrucción de arquitectura para el motor

- **Prohibido el diseño 100 % dinámico:** no crear menús, niveles o entidades
  instanciándolos únicamente por código durante el runtime, salvo para *spawns*
  dinámicos de partida.
- **Archivos de escena y prefabs:** toda entidad visual, interfaz de usuario
  (UI) y mapa debe guardarse como un recurso editable / escena en el disco.
- **Inspección de variables:** exponer las propiedades de ajuste
  (`[SerializeField]` en Unity) en los scripts para que sean editables desde la
  interfaz gráfica sin modificar el código.

---

## Documentación de modos

- [Modo Zombi](Modo-Zombi.md)
- [Modo Shooter](Modo-Shooter.md)
- [Modo Aventura](Modo-Aventura.md)
