using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Piezas comunes de los mapas: primitivas, muros con huecos, rampas,
    /// escaleras, prefabs, materiales y el jugador de pruebas. Los builders de cada
    /// mapa (<c>RATIXMapBuilder</c>, <c>RATIXDesertMapBuilder</c>…) lo usan para no
    /// repetir geometría. Solo se ejecuta en el editor.
    /// </summary>
    public static class RATIXMapKit
    {
        public const string MapsFolder = "Assets/RATIX/Maps";
        public const string SharedFolder = MapsFolder + "/Shared";
        public const string SharedMaterials = SharedFolder + "/Materials";
        public const string SharedPrefabs = SharedFolder + "/Prefabs";
        public const string PlayerPrefabPath = SharedPrefabs + "/Player_FPS.prefab";
        public const string TracerMaterialPath = SharedMaterials + "/M_Tracer.mat";

        public const string WeaponModelPath = "Assets/RATIX/Models/Weapons/Weapon_Rifle.fbx";
        public const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        // Ajuste del arma en primera persona (se toca aquí y vale para todos los mapas).
        private static readonly Vector3 WeaponPosition = new Vector3(0.2f, -0.22f, 0.38f);
        private static readonly Vector3 WeaponEuler = new Vector3(0f, 180f, 0f);
        private const float WeaponScale = 0.5f;
        private static readonly Vector3 MuzzleLocal = new Vector3(0f, 0.055f, 0.63f);

        // ------------------------------------------------------------------
        // Carpetas y assets
        // ------------------------------------------------------------------

        public static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        public static void EnsurePath(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        /// <summary>Textura de rejilla (hormigón, losas) con remaches en los cruces.</summary>
        public static void EnsureGridTexture(string folder, string fileName, Color32 baseColor,
            Color32 seamColor, int cell, int size = 512)
        {
            string path = folder + "/" + fileName + ".png";
            if (File.Exists(Path.GetFullPath(path))) return;

            var pixels = new Color32[size * size];
            var random = new System.Random(20260101);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color32 c = (x % cell <= 1 || y % cell <= 1) ? seamColor : baseColor;
                    int noise = random.Next(-10, 11);
                    pixels[y * size + x] = Shift(c, noise);
                }
            }

            ImportPixels(path, pixels, size);
        }

        /// <summary>Textura de arena: grano fino y ondas suaves.</summary>
        public static void EnsureSandTexture(string folder, string fileName, Color32 baseColor,
            Color32 rippleColor, int size = 512)
        {
            string path = folder + "/" + fileName + ".png";
            if (File.Exists(Path.GetFullPath(path))) return;

            var pixels = new Color32[size * size];
            var random = new System.Random(778899);
            float twoPi = Mathf.PI * 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float ripple = Mathf.Sin((y + Mathf.Sin(x * 0.028f) * 18f) * twoPi / 96f);
                    float blend = Mathf.InverseLerp(0.62f, 1f, ripple) * 0.3f;
                    Color32 basePixel = Color32.Lerp(baseColor, rippleColor, blend);
                    int noise = random.Next(-8, 9);
                    pixels[y * size + x] = Shift(basePixel, noise);
                }
            }

            ImportPixels(path, pixels, size);
        }

        /// <summary>Textura de ladrillo / sillería con juntas.</summary>
        public static void EnsureBrickTexture(string folder, string fileName, Color32 brickColor,
            Color32 mortarColor, int brickWidth, int brickHeight, int size = 512)
        {
            string path = folder + "/" + fileName + ".png";
            if (File.Exists(Path.GetFullPath(path))) return;

            var pixels = new Color32[size * size];
            var random = new System.Random(424242);

            for (int y = 0; y < size; y++)
            {
                int row = y / brickHeight;
                int offset = (row % 2) * (brickWidth / 2);

                for (int x = 0; x < size; x++)
                {
                    bool mortar = (y % brickHeight <= 2) || ((x + offset) % brickWidth <= 2);
                    Color32 c = mortar ? mortarColor : brickColor;
                    int noise = random.Next(-9, 10);
                    pixels[y * size + x] = Shift(c, noise);
                }
            }

            ImportPixels(path, pixels, size);
        }

        private static Color32 Shift(Color32 c, int amount)
        {
            return new Color32(
                (byte)Mathf.Clamp(c.r + amount, 0, 255),
                (byte)Mathf.Clamp(c.g + amount, 0, 255),
                (byte)Mathf.Clamp(c.b + amount, 0, 255),
                c.a);
        }

        private static void ImportPixels(string path, Color32[] pixels, int size)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            texture.SetPixels32(pixels);
            texture.Apply();

            File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = true;
                importer.maxTextureSize = 512;
                importer.sRGBTexture = true;
                importer.SaveAndReimport();
            }
        }

        public static Color Hex(int rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }

        /// <summary>Crea o actualiza un material URP/Lit.</summary>
        public static Material Lit(string folder, string fileName, Color color, float metallic = 0f,
            float smoothness = 0.2f, string texturePath = null, Vector2 tiling = default)
        {
            string path = folder + "/" + fileName + ".mat";
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.enableInstancing = true;

            if (!string.IsNullOrEmpty(texturePath))
            {
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
                material.SetTextureScale("_BaseMap", tiling);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Crea o actualiza un material URP/Unlit (trazadores, cristales).</summary>
        public static Material Unlit(string folder, string fileName, Color color)
        {
            string path = folder + "/" + fileName + ".mat";
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            material.enableInstancing = true;
            return material;
        }

        /// <summary>Crea o actualiza el material de cielo procedural.</summary>
        public static Material Skybox(string folder, string fileName, Color skyTint, Color groundColor,
            float atmosphere, float exposure, float sunSize)
        {
            string path = folder + "/" + fileName + ".mat";
            var shader = Shader.Find("Skybox/Procedural");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.SetFloat("_AtmosphereThickness", atmosphere);
            material.SetColor("_SkyTint", skyTint);
            material.SetColor("_GroundColor", groundColor);
            material.SetFloat("_Exposure", exposure);
            material.SetFloat("_SunSize", sunSize);
            EditorUtility.SetDirty(material);
            return material;
        }

        // ------------------------------------------------------------------
        // Primitivas
        // ------------------------------------------------------------------

        public static Transform Group(Transform parent, string name, Vector3 localPosition = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        public static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size,
            Material material, bool collider = true, Quaternion? rotation = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation ?? Quaternion.identity;
            go.transform.localScale = size;

            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());

            return go;
        }

        public static GameObject Cylinder(Transform parent, string name, Vector3 position, float radius,
            float height, Material material, bool collider = true, Quaternion? rotation = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation ?? Quaternion.identity;
            go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);

            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());

            return go;
        }

        public static GameObject Sphere(Transform parent, string name, Vector3 position, Vector3 size,
            Material material, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;

            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());

            return go;
        }

        public static GameObject Instance(Transform parent, GameObject prefab, Vector3 position, float yaw,
            string name = null)
        {
            if (prefab == null) return null;

            var go = parent != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent)
                : (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (name != null) go.name = name;
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            return go;
        }

        /// <summary>Coloca el prefab en ±X con el giro espejado.</summary>
        public static void PropPair(Transform parent, GameObject prefab, float x, float z, float yaw)
        {
            Instance(parent, prefab, new Vector3(x, 0f, z), yaw, prefab.name);
            Instance(parent, prefab, new Vector3(-x, 0f, z), -yaw, prefab.name);
        }

        public static GameObject SavePrefab(string folder, string fileName, GameObject root)
        {
            string path = folder + "/" + fileName + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // ------------------------------------------------------------------
        // Construcción
        // ------------------------------------------------------------------

        /// <summary>Rampa maciza: de <paramref name="from"/> a <paramref name="to"/> (superficie).</summary>
        public static void Ramp(Transform parent, string name, Vector3 from, Vector3 to, float width,
            Material deck, Material trim, bool posts = true)
        {
            const float thickness = 0.3f;
            Vector3 delta = to - from;
            if (delta.sqrMagnitude < 0.001f) return;

            float length = delta.magnitude;
            Quaternion rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            Vector3 center = (from + to) * 0.5f - rotation * Vector3.up * (thickness * 0.5f);
            float half = width * 0.5f;

            Box(parent, name, center, new Vector3(width, thickness, length), deck, true, rotation);

            Vector3 trimOffset = rotation * new Vector3(0f, thickness * 0.5f + 0.03f, 0f);
            Box(parent, name + "_TrimW", center + trimOffset + rotation * new Vector3(-half + 0.08f, 0f, 0f),
                new Vector3(0.16f, 0.07f, length), trim, false, rotation);
            Box(parent, name + "_TrimE", center + trimOffset + rotation * new Vector3(half - 0.08f, 0f, 0f),
                new Vector3(0.16f, 0.07f, length), trim, false, rotation);

            if (!posts) return;

            Vector3 middle = (from + to) * 0.5f;
            float postHeight = Mathf.Max(0.3f, middle.y);
            Vector3 postCenter = new Vector3(middle.x, postHeight * 0.5f, middle.z);
            Box(parent, name + "_PostW", postCenter + rotation * new Vector3(-half + 0.35f, 0f, 0f),
                new Vector3(0.3f, postHeight, 0.3f), deck);
            Box(parent, name + "_PostE", postCenter + rotation * new Vector3(half - 0.35f, 0f, 0f),
                new Vector3(0.3f, postHeight, 0.3f), deck);
        }

        /// <summary>Escalera maciza con el número de escalones indicado.</summary>
        public static void Stairs(Transform parent, string name, Vector3 from, Vector3 to, float width,
            int steps, Material material)
        {
            Vector3 delta = to - from;
            float run = new Vector2(delta.x, delta.z).magnitude;
            if (steps < 1 || run < 0.1f) return;

            Vector3 flat = new Vector3(delta.x, 0f, delta.z).normalized;
            Quaternion rotation = Quaternion.LookRotation(flat, Vector3.up);
            float stepRun = run / steps;
            float stepRise = delta.y / steps;

            for (int i = 0; i < steps; i++)
            {
                float height = (i + 1) * stepRise;
                Vector3 position = from + flat * (stepRun * (i + 0.5f)) + Vector3.up * (height * 0.5f);
                Box(parent, name + "_" + i, position, new Vector3(width, height, stepRun), material, true, rotation);
            }
        }

        /// <summary>Barandilla de panel macizo con remate de color.</summary>
        public static void Rail(Transform parent, string name, Vector3 center, Vector3 size,
            Material panel, Material cap)
        {
            Box(parent, name, center, size, panel);
            Box(parent, name + "_Cap", center + Vector3.up * (size.y * 0.5f + 0.045f),
                new Vector3(size.x + 0.04f, 0.09f, size.z + 0.04f), cap, false);
        }

        public static void WallAlongX(Transform parent, string name, float z, float from, float to,
            float y, float height, float thickness, Material material, params Vector2[] gaps)
        {
            foreach (var run in Runs(from, to, gaps))
            {
                if (run.y - run.x < 0.05f) continue;
                Box(parent, name + "_" + run.x.ToString("0.#") + "_" + run.y.ToString("0.#"),
                    new Vector3((run.x + run.y) * 0.5f, y, z), new Vector3(run.y - run.x, height, thickness), material);
            }
        }

        public static void WallAlongZ(Transform parent, string name, float x, float from, float to,
            float y, float height, float thickness, Material material, params Vector2[] gaps)
        {
            foreach (var run in Runs(from, to, gaps))
            {
                if (run.y - run.x < 0.05f) continue;
                Box(parent, name + "_" + run.x.ToString("0.#") + "_" + run.y.ToString("0.#"),
                    new Vector3(x, y, (run.x + run.y) * 0.5f), new Vector3(thickness, height, run.y - run.x), material);
            }
        }

        /// <summary>Hueco en un muro: centro, ancho y alturas relativas al suelo del muro.</summary>
        public struct Opening
        {
            public float Center;
            public float Width;
            public float Bottom;
            public float Top;

            public Opening(float center, float width, float bottom, float top)
            {
                Center = center;
                Width = width;
                Bottom = bottom;
                Top = top;
            }
        }

        /// <summary>
        /// Muro con puertas y ventanas reales: reparte los paños entre huecos y añade
        /// el dintel y el antepecho de cada uno.
        /// </summary>
        public static void WallWithOpenings(Transform parent, string name, bool alongX, float fixedCoord,
            float from, float to, float y0, float height, float thickness, Material material,
            params Opening[] openings)
        {
            var sorted = new List<Opening>(openings ?? new Opening[0]);
            sorted.Sort((a, b) => a.Center.CompareTo(b.Center));

            float cursor = from;
            int index = 0;

            foreach (var opening in sorted)
            {
                float start = Mathf.Max(from, opening.Center - opening.Width * 0.5f);
                float end = Mathf.Min(to, opening.Center + opening.Width * 0.5f);

                if (start - cursor > 0.05f)
                    Segment(name + "_" + index++, cursor, start, 0f, height);

                if (opening.Bottom > 0.05f)
                    Segment(name + "_Sill" + index, start, end, 0f, opening.Bottom);

                if (height - opening.Top > 0.05f)
                    Segment(name + "_Head" + index, start, end, opening.Top, height);

                cursor = Mathf.Max(cursor, end);
            }

            if (to - cursor > 0.05f)
                Segment(name + "_" + index, cursor, to, 0f, height);

            void Segment(string segName, float a, float b, float bottom, float top)
            {
                float h = top - bottom;
                if (h < 0.05f || b - a < 0.05f) return;

                Vector3 position = alongX
                    ? new Vector3((a + b) * 0.5f, y0 + bottom + h * 0.5f, fixedCoord)
                    : new Vector3(fixedCoord, y0 + bottom + h * 0.5f, (a + b) * 0.5f);

                Vector3 size = alongX
                    ? new Vector3(b - a, h, thickness)
                    : new Vector3(thickness, h, b - a);

                Box(parent, segName, position, size, material);
            }
        }

        /// <summary>Almenas decorativas sobre un muro.</summary>
        public static void Crenellations(Transform parent, string name, bool alongX, float fixedCoord,
            float from, float to, float y, Material material, float spacing = 2f)
        {
            int count = Mathf.FloorToInt((to - from) / spacing);
            for (int i = 0; i <= count; i++)
            {
                float coordinate = from + i * spacing;
                Vector3 position = alongX
                    ? new Vector3(coordinate, y + 0.3f, fixedCoord)
                    : new Vector3(fixedCoord, y + 0.3f, coordinate);
                Vector3 size = alongX ? new Vector3(0.8f, 0.6f, 0.9f) : new Vector3(0.9f, 0.6f, 0.8f);
                Box(parent, name + "_" + i, position, size, material, false);
            }
        }

        public static List<Vector2> Runs(float from, float to, Vector2[] gaps)
        {
            var runs = new List<Vector2>();
            float cursor = from;

            if (gaps != null)
            {
                var sorted = new List<Vector2>(gaps);
                sorted.Sort((a, b) => a.x.CompareTo(b.x));

                foreach (var gap in sorted)
                {
                    float start = Mathf.Clamp(gap.x, from, to);
                    float end = Mathf.Clamp(gap.y, from, to);
                    if (start > cursor) runs.Add(new Vector2(cursor, start));
                    cursor = Mathf.Max(cursor, end);
                }
            }

            if (cursor < to) runs.Add(new Vector2(cursor, to));
            return runs;
        }

        public static void SetObjectField(Object target, string field, Object value)
        {
            if (target == null) return;

            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null)
            {
                Debug.LogWarning("[RATIX] El campo '" + field + "' no existe en " + target.GetType().Name);
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetArray(SerializedObject serialized, string field, Object[] values)
        {
            var array = serialized.FindProperty(field);
            if (array == null) return;

            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                // Quita entradas huérfanas (escena movida o borrada).
                if (!File.Exists(Path.GetFullPath(scene.path))) continue;
                if (scene.path == scenePath) return;
                scenes.Add(scene);
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ------------------------------------------------------------------
        // Pulido (remaster)
        // ------------------------------------------------------------------

        /// <summary>
        /// Pase de pulido para una escena ya montada: marca la geometría como
        /// estática (batching), apaga la sombra de las piezas decorativas y deja
        /// los materiales listos para instancing. No mueve nada.
        /// </summary>
        public static void Polish(Transform root)
        {
            if (root == null) return;

            int statics = 0;
            int noShadow = 0;

            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var go = renderer.gameObject;

                GameObjectUtility.SetStaticEditorFlags(go,
                    StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
                statics++;

                // Sin collider = pieza decorativa: no necesita proyectar sombra.
                if (go.GetComponent<Collider>() == null)
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    noShadow++;
                }
            }

            Debug.Log("[RATIX] Pulido de '" + root.name + "': " + statics + " estáticos, " + noShadow + " sin sombra.");
        }

        /// <summary>
        /// Busca pares de mallas cuyas caras horizontales coinciden (z-fighting).
        /// Devuelve un informe de texto ordenado por superficie solapada.
        /// </summary>
        public static string AuditCoplanar(Transform root, int maxPairs = 20)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            var bounds = new Bounds[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) bounds[i] = renderers[i].bounds;

            var hits = new List<(float area, string text)>();

            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = i + 1; j < renderers.Length; j++)
                {
                    var a = bounds[i];
                    var b = bounds[j];

                    float overlapX = Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x);
                    float overlapZ = Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z);
                    if (overlapX < 0.05f || overlapZ < 0.05f) continue;

                    bool sameTop = Mathf.Abs(a.max.y - b.max.y) < 0.015f;
                    bool sameBottom = Mathf.Abs(a.min.y - b.min.y) < 0.015f;
                    if (!sameTop && !sameBottom) continue;

                    float area = overlapX * overlapZ;
                    if (area < 0.05f) continue;

                    string text = renderers[i].name + "  <->  " + renderers[j].name +
                                  "   (" + area.ToString("0.0") + " m², y=" + a.max.y.ToString("0.000") + ")";
                    hits.Add((area, text));
                }
            }

            hits.Sort((x, y) => y.area.CompareTo(x.area));

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("[RATIX] Solapes coplanares en '" + root.name + "': " + hits.Count);
            for (int i = 0; i < Mathf.Min(maxPairs, hits.Count); i++) sb.AppendLine("  " + hits[i].text);
            return sb.ToString();
        }

        [MenuItem("Tools/RATIX/Mapas/Remasterizar escena abierta", false, 900)]
        public static void RemasterOpenScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var report = new System.Text.StringBuilder();

            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "Player" || go.name == "HUD") continue;
                Polish(go.transform);
                report.Append(AuditCoplanar(go.transform));
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log(report.ToString());
        }

        // ------------------------------------------------------------------
        // Jugador de pruebas (compartido por todos los mapas)
        // ------------------------------------------------------------------

        /// <summary>Paleta del arma en primera persona (el FBX viene en gris).</summary>
        private static void ApplyViewmodelMaterials(GameObject rifle)
        {
            var palette = new Dictionary<string, Material>
            {
                { "MAT_Gunmetal_Dark", Lit(SharedMaterials, "VM_Gunmetal", new Color(0.16f, 0.17f, 0.18f), 0.75f, 0.45f) },
                { "MAT_Steel_Light", Lit(SharedMaterials, "VM_Steel", new Color(0.6f, 0.62f, 0.64f), 0.85f, 0.6f) },
                { "MAT_Wood_Classic", Lit(SharedMaterials, "VM_Wood", new Color(0.38f, 0.25f, 0.15f), 0f, 0.3f) },
                { "MAT_Polymer_Black", Lit(SharedMaterials, "VM_Polymer", new Color(0.07f, 0.08f, 0.09f), 0f, 0.35f) },
                { "MAT_Red_Accent", Lit(SharedMaterials, "VM_Accent", new Color(0.55f, 0.12f, 0.1f), 0f, 0.4f) },
                { "MAT_Brass_Gold", Lit(SharedMaterials, "VM_Brass", new Color(0.72f, 0.55f, 0.24f), 0.9f, 0.6f) },
                { "MAT_Military_Olive", Lit(SharedMaterials, "VM_Olive", new Color(0.25f, 0.28f, 0.16f), 0f, 0.4f) }
            };

            foreach (var renderer in rifle.GetComponentsInChildren<MeshRenderer>(true))
            {
                var materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null && palette.TryGetValue(materials[i].name, out var replacement))
                        materials[i] = replacement;
                }

                renderer.sharedMaterials = materials;
            }
        }

        /// <summary>Monta (o rehace) el prefab del jugador FPS con el arma real.</summary>
        public static GameObject BuildPlayerPrefab()
        {
            EnsurePath(SharedMaterials);
            EnsurePath(SharedPrefabs);

            var tracerMaterial = Unlit(SharedMaterials, "M_Tracer", new Color(1f, 0.88f, 0.55f, 0.9f));

            var root = new GameObject("Player_FPS");

            var body = root.AddComponent<CharacterController>();
            body.height = 1.8f;
            body.radius = 0.35f;
            body.center = new Vector3(0f, 0.9f, 0f);
            body.slopeLimit = 50f;
            body.stepOffset = 0.42f;
            body.skinWidth = 0.02f;

            var cameraGo = new GameObject("Camera");
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.62f, 0f);

            var camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 75f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 450f;
            camera.allowHDR = true;
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();

            var cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;

            // Arma: modelo real del proyecto, sin collider y sin proyectar sombra.
            var weapon = new GameObject("Weapon");
            weapon.transform.SetParent(cameraGo.transform, false);
            weapon.transform.localPosition = WeaponPosition;
            weapon.transform.localRotation = Quaternion.Euler(WeaponEuler);
            weapon.transform.localScale = Vector3.one * WeaponScale;

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(WeaponModelPath);
            if (model != null)
            {
                var rifle = (GameObject)PrefabUtility.InstantiatePrefab(model, weapon.transform);
                rifle.name = "Rifle";
                rifle.transform.localPosition = Vector3.zero;
                rifle.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

                // El FBX trae los materiales en gris; el viewmodel usa su propia paleta.
                ApplyViewmodelMaterials(rifle);

                foreach (var animator in rifle.GetComponentsInChildren<Animator>(true))
                    Object.DestroyImmediate(animator);

                foreach (var collider in rifle.GetComponentsInChildren<Collider>(true))
                    Object.DestroyImmediate(collider);

                foreach (var renderer in rifle.GetComponentsInChildren<MeshRenderer>(true))
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                }
            }
            else
            {
                Debug.LogWarning("[RATIX] No encuentro " + WeaponModelPath + "; el jugador va sin arma visible.");
            }

            var muzzle = Group(weapon.transform, "Muzzle", MuzzleLocal);

            var flashGo = new GameObject("MuzzleFlash");
            flashGo.transform.SetParent(muzzle, false);
            var flash = flashGo.AddComponent<Light>();
            flash.type = LightType.Point;
            flash.color = new Color(1f, 0.84f, 0.55f);
            flash.range = 6f;
            flash.intensity = 6f;
            flash.shadows = LightShadows.None;
            flash.enabled = false;

            var tracerGo = new GameObject("Tracer");
            tracerGo.transform.SetParent(root.transform, false);
            var tracer = tracerGo.AddComponent<LineRenderer>();
            tracer.useWorldSpace = true;
            tracer.positionCount = 2;
            tracer.startWidth = 0.045f;
            tracer.endWidth = 0.012f;
            tracer.numCapVertices = 0;
            tracer.textureMode = LineTextureMode.Stretch;
            tracer.alignment = LineAlignment.View;
            tracer.shadowCastingMode = ShadowCastingMode.Off;
            tracer.receiveShadows = false;
            tracer.sharedMaterial = tracerMaterial;
            tracer.startColor = new Color(1f, 0.95f, 0.72f, 0.95f);
            tracer.endColor = new Color(1f, 0.85f, 0.45f, 0.05f);
            tracer.enabled = false;

            var actions = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(InputActionsPath);

            var controller = root.AddComponent<FirstPersonController>();
            SetObjectField(controller, "viewCamera", camera);
            SetObjectField(controller, "inputActions", actions);

            var shooter = root.AddComponent<WeaponShooter>();
            SetObjectField(shooter, "viewCamera", camera);
            SetObjectField(shooter, "inputActions", actions);
            SetObjectField(shooter, "muzzle", muzzle);
            SetObjectField(shooter, "muzzleLight", flash);
            SetObjectField(shooter, "tracer", tracer);

            return SavePrefab(SharedPrefabs, "Player_FPS", root);
        }

        /// <summary>Instancia el jugador compartido en el spawn principal del equipo A.</summary>
        public static GameObject SpawnPlayer(MapDefinition definition)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null) prefab = BuildPlayerPrefab();
            if (prefab == null) return null;

            var spawn = definition != null ? definition.GetPrimarySpawn((int)SpawnPoint.Team.A) : null;
            Vector3 position = spawn != null ? spawn.Position : new Vector3(-41f, 0.1f, 0f);
            float yaw = spawn != null ? spawn.Rotation.eulerAngles.y : 90f;

            return Instance(null, prefab, position + Vector3.up * 0.1f, yaw, "Player");
        }

        public static void BuildHud()
        {
            var hud = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            var canvas = hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var holder = new GameObject("Crosshair", typeof(RectTransform)).transform;
            holder.SetParent(hud.transform, false);
            holder.localPosition = Vector3.zero;

            Color color = new Color(0.95f, 0.97f, 1f, 0.85f);
            ImagePiece(holder, "Dot", new Vector2(4f, 4f), Vector2.zero, color);
            ImagePiece(holder, "Top", new Vector2(2f, 9f), new Vector2(0f, 14f), color);
            ImagePiece(holder, "Bottom", new Vector2(2f, 9f), new Vector2(0f, -14f), color);
            ImagePiece(holder, "Left", new Vector2(9f, 2f), new Vector2(-14f, 0f), color);
            ImagePiece(holder, "Right", new Vector2(9f, 2f), new Vector2(14f, 0f), color);
        }

        private static void ImagePiece(Transform parent, string name, Vector2 size, Vector2 position, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        /// <summary>Punto de aparición con su marcador visual.</summary>
        public static SpawnPoint CreateSpawn(Transform parent, string name, Vector3 position, float yaw,
            SpawnPoint.Team team, int slot, bool primary, GameObject markerPrefab, Material teamMaterial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var point = go.AddComponent<SpawnPoint>();
            var serialized = new SerializedObject(point);
            serialized.FindProperty("team").enumValueIndex = (int)team;
            serialized.FindProperty("slot").intValue = slot;
            serialized.FindProperty("primary").boolValue = primary;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (markerPrefab != null)
            {
                var marker = Instance(go.transform, markerPrefab, Vector3.zero, 0f, "Marker");
                foreach (var renderer in marker.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.name == "Disc") renderer.sharedMaterial = teamMaterial;
                }
            }

            return point;
        }
    }
}
