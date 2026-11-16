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

#### 5.1 Interfaces y sub-interfaces

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

#### 5.2 Runtime: así se junta el flujo

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

#### 5.3 Checklist antes de dar Play

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
