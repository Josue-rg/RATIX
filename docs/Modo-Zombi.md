# Modo Zombi

## Tipo
**PvP asimétrico**

## Descripción
Sala donde los jugadores son **aleatoriamente zombis** y tienen que **infectar**
a los demás (humanos), que se defienden con armas.

---

## Interfaz (ya construida)

**Panel de modo** (al entrar al modo):
- **Pestañas**: Global · Servidor · Básico.
- **Lista de salas**: búsqueda (nombre/ID) + salas (nombre + jugadores).
- **Juego rápido**: carrusel de modo (clic), descripción del modo, accesorios
  (Cura, Antídoto, Atributo + 3 slots de zombis) y botón **UNIRSE**.
- **Crear sala**: carrusel de modo, carrusel de mapa, ajustes (nombre, rondas,
  jugadores, contraseña, inicio rápido, observador) y botón **CREAR**.

**Hall** (al entrar a una sala):
- Equipos (Rojo / Azul) con jugadores (nombre, clan, servidor, VIP).
- **Armas siempre visibles** (6 slots) + panel **Ajustes** (copia de "Crear sala").

---

## Parámetros editables (por definir)
- Salud del humano / zombi, daño de armas, velocidad, tiempo de infección,
  duración de ronda, número de jugadores.

## Pendiente
- Reglas exactas de infección, arsenal, condiciones de victoria.

---

## Mapas

Mapas pensados para **rotar de zona durante la ronda** (no escondites), muy
distintos entre sí:

- **Búnker K-9** (`zombie_bunker_k9`): instalación cerrada de 72 × 56 m y tres
  alturas. Esclusa, sala del reactor con anillo elevado, dormitorios,
  enfermería, almacén, sala de control con grada, armería, sala de servidores y
  garaje con muelle de carga. Supervivientes al sur, infectados al norte.
- **Hacienda El Alba** (`zombie_hacienda_alba`): finca abierta de 100 × 76 m al
  atardecer. Cañaveral, canal seco con tres puentes, hacienda de dos plantas con
  porche y tejado, granero con pajar, silo, torre de agua y campos.
- **Carguero Aurora** (`zombie_ship_aurora`): buque de 88 × 40 m en alta mar.
  Castillo de proa, cubierta estrecha, bodega a −2,5 m, grúa y superestructura de
  tres puentes con el puente de mando.
- **Templo de la Niebla** (`zombie_temple_mist`): ruinas de 96 × 80 m en la
  selva. Pirámide de cinco terrazas con escalera, templo superior, estanques,
  ruinas, pasarela de madera y maleza con niebla.

Ver [`Mapas.md`](Mapas.md).
