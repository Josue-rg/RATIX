# Mapas

> **Estado:** ocho mapas jugables de prueba.
>
> | Modo | Mapa | Escena |
> |---|---|---|
> | Shooter | **Foundry** · arena industrial | `Maps/Foundry/ShooterMap_01.unity` |
> | Shooter | **Kasbah** · poblado del desierto | `Maps/Kasbah/ShooterMap_02_Kasbah.unity` |
> | Shooter | **Muelle 7** · puerto de contenedores | `Maps/Pier7/ShooterMap_03_Muelle7.unity` |
> | Shooter | **Metro Línea 9** · estación subterránea | `Maps/Metro9/ShooterMap_04_Metro9.unity` |
> | Zombi | **Búnker K-9** · instalación cerrada | `Maps/BunkerK9/ZombieMap_01_Bunker.unity` |
> | Zombi | **Hacienda El Alba** · finca al atardecer | `Maps/HaciendaAlba/ZombieMap_02_Hacienda.unity` |
> | Zombi | **Carguero Aurora** · buque en alta mar | `Maps/ShipAurora/ZombieMap_03_Carguero.unity` |
> | Zombi | **Templo de la Niebla** · ruinas en la selva | `Maps/TempleMist/ZombieMap_04_Templo.unity` |

---

## 1. Dónde vive un mapa

Las escenas de **juego** no van en `Assets/RATIX/Scenes/`, que es la carpeta de
las escenas de **interfaz** (`Lobby.unity`). Cada mapa tiene su propia carpeta
con su arte y sus prefabs:

```
Assets/RATIX/Maps/
├── Foundry/  Kasbah/  Pier7/  Metro9/          ← Shooter
├── BunkerK9/  HaciendaAlba/  ShipAurora/  TempleMist/   ← Zombi
└── Shared/                                      ← común a todos
    ├── Prefabs/Player_FPS.prefab
    └── Materials/ (M_Tracer, VM_* del arma)
```

Regla: **un mapa = una carpeta** (`Art/` + `Prefabs/` + escena). Lo compartido
vive en `Shared/`.

### Jugador de pruebas (compartido)

`Maps/Shared/Prefabs/Player_FPS.prefab` lo usan los ocho mapas. El arma visible
es el **modelo real** `Weapons/Weapon_Rifle.fbx`, sin collider, sin sombra y con
paleta propia (`VM_*`) porque el FBX trae los materiales en gris.

Para cambiar de arma: en `RATIXMapKit.cs`, constante `WeaponModelPath`
(`Weapon_SMG`, `Weapon_Shotgun`, `Weapon_MachineGun`, `Weapon_Sniper`…) y
reconstruir un mapa.

Scripts en `Assets/RATIX/Scripts/Gameplay/`: `MapDefinition`, `SpawnPoint`,
`FirstPersonController`, `WeaponShooter` y `TargetDummy`.

---

## 2. Mapas Shooter (PvP, 2 equipos)

### Foundry — `shooter_foundry` · `value.foundry`
Arena industrial **92 × 68 m**, 8 jugadores. Edificio central de dos alturas con
techo transitable y cuatro rampas, dos pasarelas (`x = ±26`) con cuatro torres y
dos puentes, bases simétricas y contenedores.

### Kasbah — `shooter_kasbah` · `value.kasbah`
Poblado del desierto **96 × 72 m**, 10 jugadores. Plaza con pozo y mercadillo,
cuatro casas de dos alturas con tejado transitable, callejones, dunas, palmeras
y bastiones.

### Muelle 7 — `shooter_pier7` · `value.pier7`
Terminal portuario **104 × 72 m**, 10 jugadores, rodeado de mar.

| Zona | Rol |
|---|---|
| **Patio de contenedores** | Pilas de 1-2 alturas que forman pasillos y cobertura |
| **Dos almacenes** (este/oeste) | Bases con racks, cajas y boca abierta al patio |
| **Dos grúas pórtico** (`x = ±18`) | Silueta y referencia; sus patas son cobertura |
| **Pasarela del muelle** | Catwalk a 4,2 m sobre el borde norte, con dos escaleras |
| **Buque atracado** | Casco y puente al norte, fuera del área jugable (paisaje) |

### Metro Línea 9 — `shooter_metro9` · `value.metro9`
Estación bajo tierra **84 × 60 m**, 10 jugadores, con techo a 8,5 m.

| Zona | Rol |
|---|---|
| **Andenes** (norte y sur) | Elevados 1,1 m sobre el foso, con línea amarilla y bancos |
| **Foso de vías** | Carril bajo con raíles y traviesas; cruza de punta a punta |
| **Tren parado** (3 coches) | Bloquea el centro: dos carriles paralelos y huecos entre coches |
| **Entreplanta** (5,2 m) | Pasarela sobre cada andén, con barandilla y luces bajo el forjado |
| **Vestíbulos** (este/oeste) | Bases con tornos, kiosco, escaleras al foso y a la entreplanta |
| **Galerías ciegas** | Los túneles del foso, sin salida: riesgo/recompensa |

---

## 3. Mapas Zombi (infección)

El modo Zombi es PvP asimétrico: unos son zombis y tienen que infectar a los
humanos. Por eso **no son mapas de escondites**: cada uno está partido en zonas
muy distintas, con bucles y chokepoints, para jugar la ronda **rotando de zona**.

| | Búnker K-9 | Hacienda El Alba | Carguero Aurora | Templo de la Niebla |
|---|---|---|---|---|
| Sensación | Cerrado y oscuro | Abierto al atardecer | Marino y estrecho | Selvático y brumoso |
| Materiales | Hormigón, metal | Cal, madera, teja | Acero, óxido, mar | Piedra, musgo, agua |
| Alturas | Suelo, anillo 4,6 m, grada | Canal −1,6 m, pajar, tejado | Bodega −2,5 m, 3 puentes | 5 terrazas + templo |
| Luz | Lámparas y alarma roja | Sol bajo cálido | Sol marino | Luz verde y niebla |

### 3.1 Búnker K-9 — `zombie_bunker_k9` · `value.bunker_k9`
**72 × 56 m**, 12 jugadores. Esclusa de entrada, sala del reactor con anillo
elevado, dormitorios, enfermería, almacén, sala de control con grada, armería,
sala de servidores y garaje con muelle de carga. Supervivientes al sur,
infectados en los laterales del garaje.

### 3.2 Hacienda El Alba — `zombie_hacienda_alba` · `value.hacienda_alba`
**100 × 76 m**, 12 jugadores. Cañaveral, canal seco con tres puentes, hacienda de
dos plantas con porche y tejado, granero con pajar, silo, torre de agua y campos.
Supervivientes en el porche, infectados en el borde norte.

### 3.3 Carguero Aurora — `zombie_ship_aurora` · `value.ship_aurora`
Buque **88 × 40 m** en alta mar, 10 jugadores.

| Zona | Rol |
|---|---|
| **Castillo de proa** | Plataforma elevada 2,2 m con escalera; spawn de supervivientes |
| **Cubierta principal** | Pasillo estrecho entre la amurada y la bodega |
| **Bodega** | Hueco de 20 × 14 m y −2,5 m, con contenedor y escalera de salida |
| **Grúa de cubierta** | Mástil y pluma sobre la banda de estribor |
| **Superestructura** | Tres niveles: puente 1 (cubierta), puente 2 y **puente de mando** con cristales |
| **Popa** | Contenedores y botes salvavidas; spawn de infectados |

### 3.4 Templo de la Niebla — `zombie_temple_mist` · `value.temple_mist`
Ruinas **96 × 80 m**, 12 jugadores, con niebla densa.

| Zona | Rol |
|---|---|
| **Pirámide escalonada** | 5 terrazas de 1,2 m y **escalera norte** que sube terraza a terraza hasta el templo (6 m) |
| **Templo superior** | Plataforma 6 × 6 con cuatro pilares, techo y dos braseros |
| **Estanques** (2) | Agua somera con borde de piedra y juncos |
| **Ruinas** (4 esquinas) | Muros derruidos, dinteles y columnas caídas |
| **Pasarela de madera** | Camino elevado 1,3 m sobre el flanco oeste |
| **Selva** | Árboles grandes y maleza que cierran la visión |

---

## 4. Cómo se prueban

1. Menú **`RATIX ▸ Mapas`** → abrir el mapa que quieras.
2. **Play**. Apareces en el spawn principal del equipo A.

| Tecla | Acción |
|---|---|
| `WASD` | Mover |
| `Ratón` | Mirar |
| `Shift` | Esprintar |
| `Espacio` | Saltar |
| `C` | Agacharse |
| `Clic` | Disparar |
| `Esc` | Liberar el ratón (clic para volver a capturarlo) |

Arma, movimiento y blancos se ajustan **en el Inspector** (Regla 3).

---

## 5. Reconstruir o ampliar

Menú **`RATIX ▸ Mapas`** (`Tools/RATIX/Mapas`), con una entrada de **construir**
y otra de **abrir** por cada mapa.

- Todos usan **`RATIXMapKit`** (`Assets/RATIX/Editor/`): primitivas, muros con
  puertas y ventanas, rampas, escaleras, barandillas, texturas, materiales y el
  jugador compartido.
- Las **texturas** solo se generan si no existen (borrar el PNG para otro color).
- Los **prefabs** se sobrescriben al reconstruir.
- Los mapas se añaden solos a **Build Settings** y se limpian entradas huérfanas.

### Añadir otro mapa

1. Crear `Maps/<Nombre>/` con `Art/` y `Prefabs/`.
2. Copiar el builder más parecido como punto de partida.
3. Ajustar `MapDefinition` (ID, `nameKey`, modo, tamaño, spawns).
4. Añadir `value.<mapa>` en `en.json` y `es.json`.

---

## 6. Reglas que cumplen los mapas

| Regla | Cómo se cumple |
|---|---|
| **1 · Editor visual** | Todo es escena, prefab y material en disco. Nada generado en runtime. |
| **2 · Lógica / diseño** | Los scripts solo mueven, disparan o reaccionan; todo lo visual está en la escena. |
| **3 · Parámetros** | Velocidad, salto, daño, cadencia, vida y reaparición son `[SerializeField]`. |
| **4 · Estructura** | `Maps/<Mapa>/` con `Art/`, `Prefabs/` y escena; común en `Maps/Shared/`. |
| **5 · UI** | Los mapas no tienen UI de lobby; el HUD es un canvas creado en la escena, sin texto. |
| **6 · Assets pesados** | Primitivas y texturas de 512 px; los modelos de arma existentes se reutilizan. |
| **Localización** | Sin texto visible en el mapa; los nombres usan claves `value.*`. |

---

## 7. Pendiente

- **Lógica del modo Zombi** (infección, rondas, contagio) y enlazar los mapas al
  carrusel de *Crear sala*.
- NavMesh para bots e infectados controlados por la IA.
- Recogidas de munición / vida (necesitan UI con ID de localización).
- Sonido de disparo y ambiente propio de cada mapa (mar, metro, selva).
- Iluminación horneada (lightmaps y light probes) cuando el arte sea definitivo.
- Vestir los mapas con arte definitivo (props con curvas, telas, vegetación real).
