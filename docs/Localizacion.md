# Localización (idiomas)

> **Estado:** Fase 1 implementada (**EN/ES**, cambio instantáneo desde Ajustes).
> Fases 2 y 3 pendientes.

Objetivo: cambiar de idioma **al instante**, sin recargar la escena y sin depender
del texto literal. El texto visible se identifica con un **ID estable** y cada
texto escucha el cambio de idioma.

---

## 1. Arquitectura

```
en.json / es.json  →  LocalizationService  →  evento LanguageChanged
                                                     │
                                      TextLocalizer (uno por texto) → TMP_Text
```

- **Sin recargar escena.** Nada de `SceneManager.LoadScene` al cambiar idioma.
- **IDs, no textos.** La clave es independiente del texto; si cambia el inglés,
  la traducción sigue funcionando.
- **Un solo evento.** Al cambiar idioma se actualizan únicamente los textos
  **activos** de la escena.

---

## 2. Archivos

| Ruta | Qué es |
|---|---|
| `Assets/RATIX/Resources/Localization/en.json` | Textos base (inglés) |
| `Assets/RATIX/Resources/Localization/es.json` | Traducción (español) |
| `Assets/RATIX/Scripts/Localization/LocalizationService.cs` | Servicio + evento + `Get` / `Format` |
| `Assets/RATIX/Scripts/Localization/TextLocalizer.cs` | Componente que se pone en cada texto |
| `Assets/RATIX/Editor/LocalizationTools.cs` | Herramienta `RATIX ▸ Localización` |
| `Assets/RATIX/Localization/localization-report.json` | Salida del análisis (colisiones, omitidos, faltantes) |

`PlayerPrefs`: clave `ratix.language` (valor `en` | `es`). Por defecto `en`.

---

## 3. Formato del JSON

Objeto plano: `ID → texto`. UTF-8. Sin anidar.

```json
{
  "common.back": "Back",
  "common.close": "Close",
  "login.title": "RATIX",
  "login.subtitle": "SIGN IN TO CONTINUE",
  "login.username": "USERNAME",
  "login.password": "PASSWORD",
  "login.login_button": "LOG IN",
  "login.guest_button": "PLAY AS GUEST",
  "hall.selected_none": "Selected: none",
  "hall.selected_one": "Selected: {0}",
  "hall.selected_many": "Selected: {0} players"
}
```

- `{0}`, `{1}` → placeholders para textos compuestos (`string.Format`).
- Los dos idiomas deben tener **exactamente las mismas claves**. Una clave que
  falte en `es.json` cae al texto base, nunca deja el hueco vacío.

---

## 4. Convención de IDs

Formato: `pantalla.seccion.elemento`, en minúsculas, solo `a-z 0-9 . _`.

| Regla | Correcto | Incorrecto |
|---|---|---|
| Semántico, no por nombre de objeto | `login.login_button` | `Window/Button (1)` |
| Estable ante cambios de texto | `common.back` | `Back` |
| Contexto cuando hay ambigüedad | `weapon.bat` / `action.bat` | `bat` |
| Compartido entre pantallas | `common.close` | `close_1`, `close_2` |

- Los duplicados exactos (“Back” en 5 sitios) **comparten ID**.
- Los textos parecidos con distinto significado **no** comparten ID.
- Nunca se usa el texto como ID.

---

## 5. Reglas de qué se localiza

| Caso | ¿Se localiza? | Cómo |
|---|---|---|
| Etiqueta fija (títulos, botones, labels) | Sí | `TextLocalizer` + ID |
| Placeholder de un input | Sí | ID (`login.username_placeholder`) |
| Texto escrito por el jugador | **No** | Se deja tal cual |
| Nombre de jugador, clan, sala, mapa | **No** | Dato dinámico |
| Contadores y cifras (`5 / 15`, `Lv.999`) | **No** | Dato dinámico |
| Mensajes de chat | **No** | Dato dinámico |
| Texto compuesto por código | Sí | `L.Format("hall.selected_many", n)` |

Regla dura: **ningún texto visible se escribe suelto en la escena ni en el
código**. Si no tiene ID, no está terminado.

---

## 6. API

`LocalizationService` (`static`):

```csharp
L.Current                       // "en" | "es"
L.Initialize()                  // carga tablas y PlayerPrefs
L.SetLanguage("es")             // guarda pref + dispara LanguageChanged
L.Get("login.title")            // string (fallback al valor actual)
L.Format("hall.selected_many", 3)
event Action LanguageChanged    // suscripción de TextLocalizer
```

`TextLocalizer`:

```csharp
[SerializeField] private string key;
[SerializeField] private TMP_Text target;

OnEnable  → L.LanguageChanged += Apply; Apply();
OnDisable → L.LanguageChanged -= Apply;
Apply()   → target.text = L.Get(key, target.text);
```

- Inicialización con `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`.
- Si falta un ID: se mantiene el texto actual y se registra un warning **una
  sola vez** por clave (no spamear la consola).

---

## 7. Selector de idioma

- Va dentro del `SettingsModal` existente (no se crea pantalla nueva).
- Fila `LANGUAGE` con el chip `LanguageChip`: muestra `ENGLISH` / `ESPAÑOL` y
  alterna con un clic (`LanguageSwitch` → `L.Toggle()`).
- No reinicia el juego, no recarga escena, no pierde estado.

---

## 8. Herramienta de editor (`RATIX ▸ Localización`)

Menú:

1. **Analizar escena**: recorre la escena activa y escribe
   `localization-report.json` con: textos únicos, propuesta de ID, duplicados,
   colisiones y textos omitidos (dinámicos).
2. **Generar claves**: añade `TextLocalizer` a los textos aprobados, asigna IDs
   únicos, conserva los IDs ya existentes (idempotente) y fusiona `en.json`.
3. **Exportar CSV**: `id,en,es` para traducir fuera de Unity.
4. **Ver faltantes**: lista claves sin traducir o usadas en escena que no están
   en el JSON.

Reglas de la herramienta:

- Recorre `TMP_Text` **incluyendo inactivos**.
- **Excluye**:
  - el texto tipado de `TMP_InputField` (sí incluye su placeholder);
  - textos dinámicos por ruta: filas de jugadores/salas, contadores, chat,
    `Name`, `Status`, `Count`, `Server`, `Vip`, `Level`;
  - cadenas formadas solo por números/símbolos.
- **Deduplica** textos idénticos en un solo ID.
- **Detecta colisiones** y no genera claves nuevas hasta resolverlas.
- IDs propuestos desde la ruta jerárquica (`lobby.content.zombie...`), revisables
  en el reporte antes de aplicarlos.
- Nunca sobrescribe una traducción existente en `es.json`.

---

## 9. Rendimiento

- Cambio de idioma: O(1) por texto, solo textos activos, en un frame.
- 925 `TextLocalizer` ≈ decenas de KB; sin coste por frame.
- El único pico es la reconstrucción de mallas de TMP al cambiar muchos textos a
  la vez: se hace **una sola vez** por cambio; si hay `ContentSizeFitter`, un
  único `LayoutRebuilder.ForceRebuildLayoutImmediate` al final.
- Prohibido llamar `L.Get` en `Update`.

---

## 10. Checklist para texto o interfaz nueva

Antes de dar algo por terminado:

1. ¿El texto tiene ID y `TextLocalizer`? (o `L.Format` si es dinámico)
2. ¿La clave está en `en.json` **y** `es.json`?
3. ¿Es un dato dinámico? Entonces **no** lleva ID.
4. ¿Se reutilizó una clave común (`common.*`) en vez de duplicar?
5. ¿`RATIX ▸ Localización ▸ Ver faltantes` sale limpio?
6. ¿Probado en `en` y `es` sin recargar escena?
7. ¿La escena guardada?

---

## 11. Fases

1. ✅ Servicio + JSON + `TextLocalizer` + selector en Ajustes (sin recarga).
2. ⏳ Herramienta de editor (analizar / generar / exportar / faltantes).
   - Ya existen `RATIX ▸ Localización ▸ 1. Generar claves` y `2. Ver faltantes`.
3. ✅ Traducción EN/ES de los textos estáticos; pendiente QA de pantallas nuevas.
