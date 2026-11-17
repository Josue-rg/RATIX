using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace RATIX
{
    /// <summary>
    /// Prepara una escena de mapa para iluminación horneada: mallas de primitiva con
    /// UV2 propia, geometría marcada para GI, luces en Mixed/Baked, rejilla de light
    /// probes y un LightingSettings por mapa. Después se hornea con el menú.
    /// </summary>
    public static class RATIXLightingTools
    {
        private const string MeshFolder = "Assets/RATIX/Maps/Shared/Meshes";
        private const string SettingsFolder = "Assets/RATIX/Maps/Shared/Lighting";

        private static Mesh cubeLM;
        private static Mesh cylinderLM;
        private static Mesh sphereLM;

        [MenuItem("Tools/RATIX/Mapas/Preparar luz horneada (escena abierta)", false, 901)]
        public static void PrepareOpenScene()
        {
            var scene = SceneManager.GetActiveScene();
            Transform root = null;

            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == "Player" || go.name == "HUD") continue;
                root = go.transform;
                break;
            }

            if (root == null)
            {
                Debug.LogWarning("[RATIX] No encuentro el objeto raíz del mapa.");
                return;
            }

            EnsureFolders();
            EnsureMeshes();

            int swapped = 0;
            int gi = 0;

            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;

                Mesh target = null;
                switch (filter.sharedMesh.name)
                {
                    case "Cube":
                    case "Cube_LM":
                        target = cubeLM;
                        break;
                    case "Cylinder":
                    case "Cylinder_LM":
                        target = cylinderLM;
                        break;
                    case "Sphere":
                    case "Sphere_LM":
                        target = sphereLM;
                        break;
                }

                if (target != null && filter.sharedMesh != target)
                {
                    filter.sharedMesh = target;
                    swapped++;
                }

                var flags = GameObjectUtility.GetStaticEditorFlags(renderer.gameObject);
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, flags | StaticEditorFlags.ContributeGI);
                gi++;
            }

            // Luces: el sol aporta sombra en tiempo real y GI; las de interior se hornean.
            foreach (var light in root.GetComponentsInChildren<Light>(true))
            {
                if (light.type == LightType.Directional)
                {
                    // El sol queda en tiempo real (sombras nítidas); solo se hornea el interior.
                    light.lightmapBakeType = LightmapBakeType.Realtime;
                }
                else if (light.type == LightType.Point)
                {
                    light.lightmapBakeType = LightmapBakeType.Baked;
                }
            }

            AddProbes(root);
            Lightmapping.lightingSettings = EnsureSettings(scene.name);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[RATIX] Escena preparada para hornear: " + swapped + " mallas con UV2, " + gi + " objetos GI.");
        }

        [MenuItem("Tools/RATIX/Mapas/Hornear luz (escena abierta)", false, 902)]
        public static void BakeOpenScene()
        {
            if (Lightmapping.isRunning)
            {
                Debug.LogWarning("[RATIX] Ya hay un horneado en marcha.");
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);

            Lightmapping.BakeAsync();
            Debug.Log("[RATIX] Horneando luz de " + scene.name + "…");
        }

        [MenuItem("Tools/RATIX/Mapas/Cancelar horneado", false, 903)]
        public static void CancelBake()
        {
            Lightmapping.Cancel();
            Debug.Log("[RATIX] Horneado cancelado.");
        }

        // ------------------------------------------------------------------

        private static void EnsureFolders()
        {
            RATIXMapKit.EnsurePath(MeshFolder);
            RATIXMapKit.EnsurePath(SettingsFolder);
        }

        private static void EnsureMeshes()
        {
            cubeLM = PrimitiveMesh("Cube", PrimitiveType.Cube);
            cylinderLM = PrimitiveMesh("Cylinder", PrimitiveType.Cylinder);
            sphereLM = PrimitiveMesh("Sphere", PrimitiveType.Sphere);
        }

        private static Mesh PrimitiveMesh(string name, PrimitiveType type)
        {
            string path = MeshFolder + "/" + name + "_LM.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            var go = GameObject.CreatePrimitive(type);
            var mesh = Object.Instantiate(go.GetComponent<MeshFilter>().sharedMesh);
            Object.DestroyImmediate(go);

            mesh.name = name + "_LM";
            Unwrapping.GenerateSecondaryUVSet(mesh);

            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static LightingSettings EnsureSettings(string sceneName)
        {
            string path = SettingsFolder + "/" + sceneName + "_Lighting.lighting";
            var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(path);

            if (settings == null)
            {
                settings = new LightingSettings();
                AssetDatabase.CreateAsset(settings, path);
            }

            settings.bakedGI = true;
            settings.realtimeGI = false;
            settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            settings.lightmapResolution = 4f;
            settings.lightmapMaxSize = 512;
            settings.lightmapPadding = 2;
            settings.lightmapCompression = LightmapCompression.NormalQuality;
            settings.ao = true;
            settings.aoMaxDistance = 0.7f;
            settings.mixedBakeMode = MixedLightingMode.Shadowmask;
            settings.directSampleCount = 16;
            settings.indirectSampleCount = 256;
            settings.environmentSampleCount = 128;
            settings.minBounces = 1;
            settings.maxBounces = 2;
            settings.indirectScale = 1f;

            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static void AddProbes(Transform root)
        {
            var lighting = root.Find("Lighting");
            Transform parent = lighting != null ? lighting : root;

            var old = parent.Find("LightProbes");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var definition = Object.FindAnyObjectByType<MapDefinition>();
            Vector3 size = definition != null ? definition.ArenaSize : new Vector3(90f, 8f, 60f);

            var positions = new List<Vector3>();
            const int nx = 9;
            const int nz = 7;
            const int ny = 3;

            for (int ix = 0; ix < nx; ix++)
            {
                float x = Mathf.Lerp(-size.x * 0.42f, size.x * 0.42f, ix / (float)(nx - 1));
                for (int iz = 0; iz < nz; iz++)
                {
                    float z = Mathf.Lerp(-size.z * 0.42f, size.z * 0.42f, iz / (float)(nz - 1));
                    for (int iy = 0; iy < ny; iy++)
                    {
                        float y = Mathf.Lerp(1.4f, Mathf.Max(2f, size.y - 1.5f), iy / (float)(ny - 1));
                        positions.Add(new Vector3(x, y, z));
                    }
                }
            }

            var go = new GameObject("LightProbes");
            go.transform.SetParent(parent, false);
            var group = go.AddComponent<LightProbeGroup>();
            group.probePositions = positions.ToArray();
        }
    }
}
