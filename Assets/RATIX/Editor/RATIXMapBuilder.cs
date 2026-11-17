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
    /// Construye el mapa Shooter de prueba (Foundry) con geometría de bloque:
    /// arena cerrada, edificio central de dos alturas, pasarelas elevadas, dos
    /// bases, coberturas y blancos de práctica. El resultado queda como escena +
    /// prefabs + materiales editables (Reglas 1 y 2 del proyecto); este menú solo
    /// automatiza el montaje inicial, igual que el resto de herramientas RATIX.
    /// </summary>
    public static class RATIXMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/Foundry";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ShooterMap_01.unity";
        private const string ProfilePath = ArtFolder + "/ShooterMap_01_Profile.asset";

        // Medidas de la arena (metros).
        private const float HalfX = 46f;
        private const float HalfZ = 34f;
        private const float WallH = 7f;
        private const float DeckTop = 4.8f;   // altura de pasarelas, torres y techo
        private const float DeckThickness = 0.3f;

        private static Material MFloor;
        private static Material MWall;
        private static Material MConcrete;
        private static Material MMetal;
        private static Material MMetalDark;
        private static Material MRail;
        private static Material MAccent;
        private static Material MWood;
        private static Material MTeamA;
        private static Material MTeamB;
        private static Material MTeamAPad;
        private static Material MTeamBPad;
        private static Material MTargetFace;
        private static Material MTargetCenter;
        private static Material MTargetHit;

        private static GameObject PCrateSmall;
        private static GameObject PCrateLarge;
        private static GameObject PBarrier;
        private static GameObject PContainer;
        private static GameObject PPillar;
        private static GameObject PSpawnMarker;
        private static GameObject PTarget;

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Shooter (Foundry)", false, 20)]
        public static void BuildShooterMap()
        {
            // Se guarda lo que esté abierto para no perder ningún cambio pendiente.
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Los prefabs se montan en la escena vacía y se borran al guardarlos.
            BuildPrefabs();

            var mapRoot = new GameObject("ShooterMap_01").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildArena(environment);
            BuildMidBuilding(environment);
            BuildHighLanes(environment);
            BuildBases(environment);
            BuildProps(environment);
            BuildTargets(environment);
            BuildLighting(lighting);
            RATIXMapKit.Polish(environment);
            RATIXMapKit.Polish(lighting);

            MapDefinition definition = BuildGameplay(mapRoot);
            BuildPlayer(definition);
            BuildHud();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = definition.gameObject;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Mapa Foundry construido"));
            }

            Debug.Log("[RATIX] Mapa Shooter construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Shooter (Foundry)", false, 21)]
        public static void OpenShooterMap()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        // ------------------------------------------------------------------
        // Carpetas, texturas y materiales
        // ------------------------------------------------------------------

        private static void CreateFolders()
        {
            EnsureFolder("Assets/RATIX", "Maps");
            EnsureFolder("Assets/RATIX/Maps", "Shooter");
            EnsureFolder(MapFolder, "Art");
            EnsureFolder(ArtFolder, "Textures");
            EnsureFolder(ArtFolder, "Materials");
            EnsureFolder(MapFolder, "Prefabs");
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void BuildTextures()
        {
            // Rejilla de hormigón para el suelo (celdas de 1 m) y paneles metálicos.
            EnsureTexture("T_FloorGrid", new Color32(98, 106, 117, 255), new Color32(72, 80, 90, 255), 64, 512);
            EnsureTexture("T_WallPanel", new Color32(92, 102, 114, 255), new Color32(66, 76, 87, 255), 128, 512);
        }

        private static void EnsureTexture(string fileName, Color32 baseColor, Color32 seamColor, int cell, int size)
        {
            string path = TextureFolder + "/" + fileName + ".png";
            if (File.Exists(Path.GetFullPath(path))) return;

            var pixels = new Color32[size * size];
            var random = new System.Random(20260101);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color32 c = (x % cell <= 1 || y % cell <= 1) ? seamColor : baseColor;
                    int noise = random.Next(-10, 11);
                    pixels[y * size + x] = new Color32(
                        (byte)Mathf.Clamp(c.r + noise, 0, 255),
                        (byte)Mathf.Clamp(c.g + noise, 0, 255),
                        (byte)Mathf.Clamp(c.b + noise, 0, 255),
                        255);
                }
            }

            // Remaches en las intersecciones de los paneles.
            for (int y = 6; y < size; y += cell)
            {
                for (int x = 6; x < size; x += cell)
                {
                    for (int dy = 0; dy < 3; dy++)
                    {
                        for (int dx = 0; dx < 3; dx++)
                        {
                            int px = Mathf.Clamp(x + dx, 0, size - 1);
                            int py = Mathf.Clamp(y + dy, 0, size - 1);
                            pixels[py * size + px] = new Color32(120, 132, 145, 255);
                        }
                    }
                }
            }

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

        private static void BuildMaterials()
        {
            // Base neutra + acento del modo Shooter (docs/README.md · Paletas).
            MFloor = Lit("M_Floor", Color.white, 0f, 0.22f, "T_FloorGrid", new Vector2(11.5f, 8.5f));
            MWall = Lit("M_Wall", Color.white, 0f, 0.18f, "T_WallPanel", new Vector2(3f, 2f));
            MConcrete = Lit("M_Concrete", Hex(0x7C8794), 0f, 0.15f);
            MMetal = Lit("M_Metal", Hex(0x66727E), 0.55f, 0.42f);
            MMetalDark = Lit("M_MetalDark", Hex(0x414C58), 0.65f, 0.35f);
            MRail = Lit("M_Rail", Hex(0x58677A), 0.6f, 0.5f);
            MAccent = Lit("M_Accent", Hex(0xE0B45A), 0.2f, 0.45f);
            MWood = Lit("M_Wood", Hex(0x9A7350), 0f, 0.12f);
            MTeamA = Lit("M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit("M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit("M_TeamA_Pad", Color.Lerp(Hex(0x2A3038), Hex(0xCE5A46), 0.45f), 0f, 0.2f);
            MTeamBPad = Lit("M_TeamB_Pad", Color.Lerp(Hex(0x2A3038), Hex(0x6B9BC9), 0.45f), 0f, 0.2f);
            MTargetFace = Lit("M_TargetFace", Hex(0xDCE3EA), 0f, 0.3f);
            MTargetCenter = Lit("M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit("M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            SkyboxMaterial();
        }

        private static Color Hex(int rgb)
        {
            return new Color(
                ((rgb >> 16) & 0xFF) / 255f,
                ((rgb >> 8) & 0xFF) / 255f,
                (rgb & 0xFF) / 255f,
                1f);
        }

        private static Material Lit(string fileName, Color color, float metallic, float smoothness,
            string texture = null, Vector2 tiling = default)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
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

            if (!string.IsNullOrEmpty(texture))
            {
                var map = AssetDatabase.LoadAssetAtPath<Texture2D>(TextureFolder + "/" + texture + ".png");
                material.SetTexture("_BaseMap", map);
                material.SetTextureScale("_BaseMap", tiling);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Unlit(string fileName, Color color)
        {
            string path = MaterialFolder + "/" + fileName + ".mat";
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
            return material;
        }

        private static void SkyboxMaterial()
        {
            string path = MaterialFolder + "/M_Skybox.mat";
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

            material.SetFloat("_AtmosphereThickness", 1.05f);
            material.SetColor("_SkyTint", Hex(0x8398AD));
            material.SetColor("_GroundColor", Hex(0x39414A));
            material.SetFloat("_Exposure", 1.3f);
            material.SetFloat("_SunSize", 0.03f);
            EditorUtility.SetDirty(material);
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildPrefabs()
        {
            PCrateSmall = BuildCrate("Prop_Crate_Small", 1.2f);
            PCrateLarge = BuildCrate("Prop_Crate_Large", 2.4f);
            PBarrier = BuildBarrier();
            PContainer = BuildContainer();
            PPillar = BuildPillar();
            PSpawnMarker = BuildSpawnMarker();
            PTarget = BuildTarget();
            AssetDatabase.SaveAssets();
        }

        private static GameObject SavePrefab(string fileName, GameObject root)
        {
            string path = PrefabFolder + "/" + fileName + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildCrate(string fileName, float size)
        {
            var root = new GameObject(fileName);
            var t = root.transform;

            Box(t, "Body", new Vector3(0f, size * 0.5f, 0f), new Vector3(size, size, size), MWood);
            Box(t, "Lid", new Vector3(0f, size + 0.03f, 0f), new Vector3(size * 0.92f, 0.06f, size * 0.92f), MMetalDark, false);
            Box(t, "BandL", new Vector3(-size * 0.5f - 0.015f, size * 0.5f, 0f), new Vector3(0.05f, size * 0.86f, size * 0.86f), MMetalDark, false);
            Box(t, "BandR", new Vector3(size * 0.5f + 0.015f, size * 0.5f, 0f), new Vector3(0.05f, size * 0.86f, size * 0.86f), MMetalDark, false);

            return SavePrefab(fileName, root);
        }

        private static GameObject BuildBarrier()
        {
            var root = new GameObject("Prop_Barrier");
            var t = root.transform;

            Box(t, "Base", new Vector3(0f, 0.18f, 0f), new Vector3(2.4f, 0.36f, 0.9f), MConcrete);
            Box(t, "Body", new Vector3(0f, 0.63f, 0f), new Vector3(2.4f, 0.55f, 0.55f), MConcrete);
            Box(t, "Top", new Vector3(0f, 0.98f, 0f), new Vector3(2.4f, 0.18f, 0.34f), MConcrete);
            Box(t, "Stripe", new Vector3(0f, 0.63f, 0.282f), new Vector3(1.3f, 0.16f, 0.02f), MAccent, false);

            return SavePrefab("Prop_Barrier", root);
        }

        private static GameObject BuildContainer()
        {
            var root = new GameObject("Prop_Container");
            var t = root.transform;

            Box(t, "Body", new Vector3(0f, 1.3f, 0f), new Vector3(6f, 2.6f, 2.6f), MMetal);

            for (int i = -1; i <= 1; i++)
            {
                float x = i * 1.85f;
                Box(t, "RibN" + i, new Vector3(x, 1.3f, 1.32f), new Vector3(0.14f, 2.3f, 0.06f), MMetalDark, false);
                Box(t, "RibS" + i, new Vector3(x, 1.3f, -1.32f), new Vector3(0.14f, 2.3f, 0.06f), MMetalDark, false);
            }

            Box(t, "TopRail", new Vector3(0f, 2.66f, 0f), new Vector3(6.1f, 0.14f, 2.7f), MMetalDark, false);
            Box(t, "PostW", new Vector3(-2.95f, 1.3f, 0f), new Vector3(0.12f, 2.6f, 2.6f), MMetalDark, false);
            Box(t, "PostE", new Vector3(2.95f, 1.3f, 0f), new Vector3(0.12f, 2.6f, 2.6f), MMetalDark, false);

            return SavePrefab("Prop_Container", root);
        }

        private static GameObject BuildPillar()
        {
            var root = new GameObject("Prop_Pillar");
            var t = root.transform;

            Box(t, "Base", new Vector3(0f, 0.15f, 0f), new Vector3(0.95f, 0.3f, 0.95f), MConcrete);
            Cylinder(t, "Shaft", new Vector3(0f, 1.85f, 0f), 0.35f, 3.4f, MWall);
            Box(t, "Cap", new Vector3(0f, 3.65f, 0f), new Vector3(0.9f, 0.2f, 0.9f), MConcrete);

            return SavePrefab("Prop_Pillar", root);
        }

        private static GameObject BuildSpawnMarker()
        {
            var root = new GameObject("Prop_SpawnMarker");
            var t = root.transform;

            Cylinder(t, "Ring", new Vector3(0f, 0.015f, 0f), 0.88f, 0.03f, MMetalDark, false);
            Cylinder(t, "Disc", new Vector3(0f, 0.035f, 0f), 0.72f, 0.07f, MTeamA, false);

            return SavePrefab("Prop_SpawnMarker", root);
        }

        private static GameObject BuildTarget()
        {
            var root = new GameObject("Prop_Target");
            var t = root.transform;

            Cylinder(t, "Base", new Vector3(0f, 0.06f, 0f), 0.45f, 0.12f, MMetalDark);

            var pivot = Group(t, "Pivot", new Vector3(0f, 0.12f, 0f));
            Box(pivot, "Post", new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 1.6f, 0.12f), MMetalDark);
            var plate = Cylinder(pivot, "Plate", new Vector3(0f, 1.45f, 0f), 0.55f, 0.09f, MTargetFace, true, Quaternion.Euler(90f, 0f, 0f));
            Cylinder(pivot, "Bullseye", new Vector3(0f, 1.45f, 0.055f), 0.2f, 0.05f, MTargetCenter, false, Quaternion.Euler(90f, 0f, 0f));

            var dummy = root.AddComponent<TargetDummy>();
            SetObjectField(dummy, "plate", plate.GetComponent<Renderer>());
            SetObjectField(dummy, "idleMaterial", MTargetFace);
            SetObjectField(dummy, "hitMaterial", MTargetHit);
            SetObjectField(dummy, "fallPivot", pivot);

            return SavePrefab("Prop_Target", root);
        }


        // ------------------------------------------------------------------
        // Geometría del mapa
        // ------------------------------------------------------------------

        private static void BuildArena(Transform environment)
        {
            var arena = Group(environment, "Arena");

            Box(arena, "Floor", new Vector3(0f, -0.5f, 0f), new Vector3(HalfX * 2f, 1f, HalfZ * 2f), MFloor);

            Box(arena, "WallNorth", new Vector3(0f, WallH * 0.5f, HalfZ), new Vector3(HalfX * 2f + 2f, WallH, 1f), MWall);
            Box(arena, "WallSouth", new Vector3(0f, WallH * 0.5f, -HalfZ), new Vector3(HalfX * 2f + 2f, WallH, 1f), MWall);
            Box(arena, "WallEast", new Vector3(HalfX, WallH * 0.5f, 0f), new Vector3(1f, WallH, HalfZ * 2f), MWall);
            Box(arena, "WallWest", new Vector3(-HalfX, WallH * 0.5f, 0f), new Vector3(1f, WallH, HalfZ * 2f), MWall);

            Box(arena, "TrimNorth", new Vector3(0f, WallH + 0.1f, HalfZ), new Vector3(HalfX * 2f + 2.4f, 0.2f, 1.5f), MMetal, false);
            Box(arena, "TrimSouth", new Vector3(0f, WallH + 0.1f, -HalfZ), new Vector3(HalfX * 2f + 2.4f, 0.2f, 1.5f), MMetal, false);
            Box(arena, "TrimEast", new Vector3(HalfX, WallH + 0.1f, 0f), new Vector3(1.5f, 0.2f, HalfZ * 2f - 3f), MMetal, false);
            Box(arena, "TrimWest", new Vector3(-HalfX, WallH + 0.1f, 0f), new Vector3(1.5f, 0.2f, HalfZ * 2f - 3f), MMetal, false);

            for (int x = -40; x <= 40; x += 10)
            {
                Box(arena, "ButtressN" + x, new Vector3(x, 1.6f, HalfZ - 0.8f), new Vector3(0.8f, 3.2f, 0.5f), MMetalDark);
                Box(arena, "ButtressS" + x, new Vector3(x, 1.6f, -HalfZ + 0.8f), new Vector3(0.8f, 3.2f, 0.5f), MMetalDark);
            }

            for (int z = -30; z <= 30; z += 10)
            {
                Box(arena, "ButtressE" + z, new Vector3(HalfX - 0.8f, 1.6f, z), new Vector3(0.5f, 3.2f, 0.8f), MMetalDark);
                Box(arena, "ButtressW" + z, new Vector3(-HalfX + 0.8f, 1.6f, z), new Vector3(0.5f, 3.2f, 0.8f), MMetalDark);
            }
        }

        private static void BuildMidBuilding(Transform environment)
        {
            var mid = Group(environment, "MidBuilding");
            const float t = 0.8f;
            const float h = 4.4f;

            // Muros con puerta central de 6 m (una por lado).
            foreach (float z in new[] { 10f, -10f })
            {
                string tag = z > 0f ? "N" : "S";
                Box(mid, "Wall" + tag + "_W", new Vector3(-7.5f, h * 0.5f, z), new Vector3(9f, h, t), MWall);
                Box(mid, "Wall" + tag + "_E", new Vector3(7.5f, h * 0.5f, z), new Vector3(9f, h, t), MWall);
                Box(mid, "Wall" + tag + "_Lintel", new Vector3(0f, 3.8f, z), new Vector3(6f, 1.2f, t), MWall);
            }

            foreach (float x in new[] { 12f, -12f })
            {
                string tag = x > 0f ? "E" : "W";
                Box(mid, "Wall" + tag + "_S", new Vector3(x, h * 0.5f, -6.5f), new Vector3(t, h, 7f), MWall);
                Box(mid, "Wall" + tag + "_N", new Vector3(x, h * 0.5f, 6.5f), new Vector3(t, h, 7f), MWall);
                Box(mid, "Wall" + tag + "_Lintel", new Vector3(x, 3.8f, 0f), new Vector3(t, 1.2f, 6f), MWall);
            }

            Box(mid, "Roof", new Vector3(0f, 4.6f, 0f), new Vector3(24f, 0.4f, 20f), MMetal);

            // Núcleo central: rompe las líneas de tiro de la planta baja.
            Box(mid, "Core", new Vector3(0f, h * 0.5f, 0f), new Vector3(5f, h, 5f), MWall);
            Box(mid, "CoreStripeN", new Vector3(0f, 2.5f, 2.53f), new Vector3(5.1f, 0.18f, 0.06f), MAccent, false);
            Box(mid, "CoreStripeS", new Vector3(0f, 2.5f, -2.53f), new Vector3(5.1f, 0.18f, 0.06f), MAccent, false);

            // Peto del techo con huecos en las cuatro rampas y en los dos puentes.
            float py = DeckTop + 0.5f;
            WallAlongX(mid, "ParapetN", 9.82f, -12f, 12f, py, 1f, 0.35f, MWall);
            WallAlongX(mid, "ParapetS", -9.82f, -12f, 12f, py, 1f, 0.35f, MWall);
            WallAlongZ(mid, "ParapetE", 11.82f, -10f, 10f, py, 1f, 0.35f, MWall,
                new Vector2(-1.7f, 1.7f), new Vector2(3f, 7f));
            WallAlongZ(mid, "ParapetW", -11.82f, -10f, 10f, py, 1f, 0.35f, MWall,
                new Vector2(-7f, -3f), new Vector2(-1.7f, 1.7f));

            // Detalle y cobertura del techo (equipos y pretiles bajos).
            Box(mid, "RoofUnitN", new Vector3(-5f, 5.1f, 5.5f), new Vector3(3f, 0.6f, 2f), MMetalDark);
            Box(mid, "RoofUnitS", new Vector3(5f, 5.1f, -5.5f), new Vector3(3f, 0.6f, 2f), MMetalDark);
            Box(mid, "RoofCoverN", new Vector3(6f, 5.15f, 6.4f), new Vector3(4f, 0.7f, 0.4f), MConcrete);
            Box(mid, "RoofCoverS", new Vector3(-6f, 5.15f, -6.4f), new Vector3(4f, 0.7f, 0.4f), MConcrete);

            // Caseta de servicios: cobertura central del techo.
            Box(mid, "RoofHut", new Vector3(-1f, 5.9f, -4.5f), new Vector3(5f, 2.2f, 3.5f), MWall);
            Box(mid, "RoofHutCap", new Vector3(-1f, 7.05f, -4.5f), new Vector3(5.3f, 0.12f, 3.8f), MMetalDark, false);
            Box(mid, "RoofHutStripe", new Vector3(-1f, 6.6f, -2.73f), new Vector3(5.1f, 0.14f, 0.05f), MAccent, false);

            // Cuatro rampas de acceso al techo.
            Ramp(mid, "RampN", new Vector3(7f, 0f, 21f), new Vector3(7f, DeckTop, 10.2f), 4f);
            Ramp(mid, "RampS", new Vector3(-7f, 0f, -21f), new Vector3(-7f, DeckTop, -10.2f), 4f);
            Ramp(mid, "RampE", new Vector3(21f, 0f, 5f), new Vector3(12.2f, DeckTop, 5f), 4f);
            Ramp(mid, "RampW", new Vector3(-21f, 0f, -5f), new Vector3(-12.2f, DeckTop, -5f), 4f);
        }

        private static void BuildHighLanes(Transform environment)
        {
            var lanes = Group(environment, "HighLanes");
            float cy = DeckTop - DeckThickness * 0.5f;
            const float laneX = 26f;          // eje de la pasarela alta
            const float bridgeStart = 12f;    // borde del techo central
            const float bridgeCenter = 18.2f;
            const float bridgeLength = 12.4f;

            for (int sx = -1; sx <= 1; sx += 2)
            {
                float cx = sx * laneX;
                var catwalk = Group(lanes, sx > 0 ? "CatwalkEast" : "CatwalkWest");

                Box(catwalk, "Deck", new Vector3(cx, cy, 0f), new Vector3(3.2f, DeckThickness, 41f), MMetal);

                foreach (float offset in new[] { -1.6f, 1.6f })
                {
                    WallAlongZ(catwalk, "Rail" + (offset < 0f ? "W" : "E"), cx + offset, -21f, 21f,
                        DeckTop + 0.5f, 1f, 0.12f, MRail, new Vector2(-1.7f, 1.7f));
                }

                for (float z = -15.75f; z <= 16f; z += 10.5f)
                {
                    Box(catwalk, "Support" + z.ToString("0.#"), new Vector3(cx, DeckTop * 0.5f, z),
                        new Vector3(0.4f, DeckTop, 0.4f), MMetalDark);
                }

                // Puente de la pasarela al techo del edificio central.
                var bridge = Group(lanes, sx > 0 ? "BridgeEast" : "BridgeWest");
                Box(bridge, "Deck", new Vector3(sx * bridgeCenter, cy, 0f), new Vector3(bridgeLength, DeckThickness, 3.2f), MMetal);
                Rail(bridge, "RailN", new Vector3(sx * bridgeCenter, DeckTop + 0.5f, 1.7f), new Vector3(bridgeLength, 1f, 0.12f));
                Rail(bridge, "RailS", new Vector3(sx * bridgeCenter, DeckTop + 0.5f, -1.7f), new Vector3(bridgeLength, 1f, 0.12f));

                foreach (float px in new[] { 15f, 21f })
                {
                    Box(bridge, "Support" + px.ToString("0.#") + "N", new Vector3(sx * px, DeckTop * 0.5f, 1.2f),
                        new Vector3(0.35f, DeckTop, 0.35f), MMetalDark);
                    Box(bridge, "Support" + px.ToString("0.#") + "S", new Vector3(sx * px, DeckTop * 0.5f, -1.2f),
                        new Vector3(0.35f, DeckTop, 0.35f), MMetalDark);
                }

                for (int sz = -1; sz <= 1; sz += 2) BuildTower(lanes, sx, sz, laneX);
            }
        }

        private static void BuildTower(Transform parent, float sx, float sz, float laneX)
        {
            float cx = sx * laneX;
            float cz = sz * 25f;
            var tower = Group(parent, "Tower_" + (sx > 0 ? "E" : "W") + (sz > 0 ? "N" : "S"));

            Box(tower, "Deck", new Vector3(cx, DeckTop - 0.2f, cz), new Vector3(9f, 0.4f, 9f), MMetal);

            for (int i = 0; i < 4; i++)
            {
                float ox = i % 2 == 0 ? -3.6f : 3.6f;
                float oz = i < 2 ? -3.6f : 3.6f;
                Box(tower, "Leg" + i, new Vector3(cx + ox, DeckTop * 0.5f - 0.2f, cz + oz),
                    new Vector3(0.4f, DeckTop - 0.4f, 0.4f), MMetalDark);
            }

            float railY = DeckTop + 0.5f;

            // Lado hacia el centro y lado exterior: cerrados.
            Rail(tower, "RailInner", new Vector3(cx - sx * 4.4f, railY, cz), new Vector3(0.12f, 1f, 9f));
            Rail(tower, "RailOuter", new Vector3(cx, railY, cz + sz * 4.4f), new Vector3(9f, 1f, 0.12f));
            Rail(tower, "RailBack", new Vector3(cx + sx * 4.4f, railY, cz), new Vector3(0.12f, 1f, 9f));

            // Lado de la pasarela: abierto en el centro para poder pasar.
            WallAlongX(tower, "RailCatwalk", cz - sz * 4.4f, cx - 4.4f, cx + 4.4f, railY, 1f, 0.12f, MRail,
                new Vector2(cx - 1.7f, cx + 1.7f));
        }

        private static void BuildBases(Transform environment)
        {
            var bases = Group(environment, "Bases");
            BuildBase(bases, "BaseA", -1f);
            BuildBase(bases, "BaseB", 1f);
        }

        private static void BuildBase(Transform parent, string name, float side)
        {
            var b = Group(parent, name);
            float cx = side * 38f;
            float frontX = side * 30f;

            Box(b, "Pad", new Vector3(cx, 0.05f, 0f), new Vector3(15f, 0.1f, 23f),
                side < 0f ? MTeamAPad : MTeamBPad, false);

            Box(b, "WallNorth", new Vector3(cx, 2f, 12f), new Vector3(16f, 4f, 0.8f), MWall);
            Box(b, "WallSouth", new Vector3(cx, 2f, -12f), new Vector3(16f, 4f, 0.8f), MWall);
            Box(b, "WallFrontN", new Vector3(frontX, 2f, 8.5f), new Vector3(0.8f, 4f, 7f), MWall);
            Box(b, "WallFrontS", new Vector3(frontX, 2f, -8.5f), new Vector3(0.8f, 4f, 7f), MWall);
            Box(b, "WallFrontLintel", new Vector3(frontX, 3.6f, 0f), new Vector3(0.8f, 0.8f, 10f), MWall);

            // Franja de equipo en las paredes largas.
            var stripe = side < 0f ? MTeamA : MTeamB;
            Box(b, "StripeN", new Vector3(cx, 2.4f, 11.55f), new Vector3(14f, 0.16f, 0.06f), stripe, false);
            Box(b, "StripeS", new Vector3(cx, 2.4f, -11.55f), new Vector3(14f, 0.16f, 0.06f), stripe, false);

            Box(b, "TrimFront", new Vector3(frontX, 4.05f, 0f), new Vector3(0.9f, 0.14f, 10.2f), stripe, false);
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            // Contenedores (cobertura grande).
            PropPair(props, PContainer, 20f, -16f, 0f);
            PropPair(props, PContainer, 20f, 16f, 0f);
            PropPair(props, PContainer, 24f, 8f, 90f);

            // Barreras.
            PropPair(props, PBarrier, 15f, -6f, 0f);
            PropPair(props, PBarrier, 15f, 12f, 0f);
            PropPair(props, PBarrier, 22f, -5f, 0f);
            Instance(props, PBarrier, new Vector3(0f, 0f, -15f), 90f, "BarrierMidS");
            Instance(props, PBarrier, new Vector3(0f, 0f, 15f), 90f, "BarrierMidN");

            // Cajas: torres de cobertura y escalones.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                Instance(props, PCrateLarge, new Vector3(sx * 24f, 0f, -10f), 0f, "CrateLarge");
                Instance(props, PCrateSmall, new Vector3(sx * 24f, 2.4f, -10f), 12f, "CrateSmall");
                Instance(props, PCrateLarge, new Vector3(sx * 25f, 0f, -22f), 0f, "CrateLarge");
                Instance(props, PCrateSmall, new Vector3(sx * 25f, 2.4f, -22f), -8f, "CrateSmall");

                Instance(props, PCrateSmall, new Vector3(sx * 19f, 0f, -23f), 0f, "CrateSmall");
                Instance(props, PCrateSmall, new Vector3(sx * 20.3f, 0f, -23.4f), 24f, "CrateSmall");
                Instance(props, PCrateSmall, new Vector3(sx * 19.6f, 0f, -24.6f), 45f, "CrateSmall");

                Instance(props, PCrateSmall, new Vector3(sx * 19f, 0f, 23f), 0f, "CrateSmall");
                Instance(props, PCrateSmall, new Vector3(sx * 20.3f, 0f, 23.4f), -24f, "CrateSmall");
                Instance(props, PCrateSmall, new Vector3(sx * 19.6f, 0f, 24.6f), -45f, "CrateSmall");

                Instance(props, PPillar, new Vector3(sx * 16f, 0f, -13f), 0f, "Pillar");
                Instance(props, PPillar, new Vector3(sx * 16f, 0f, 13f), 0f, "Pillar");
            }

            // Cajas del centro de la arena (calles norte y sur).
            Instance(props, PCrateLarge, new Vector3(0f, 0f, -25f), 0f, "CrateCenterS");
            Instance(props, PCrateSmall, new Vector3(0f, 2.4f, -25f), 30f, "CrateCenterS");
            Instance(props, PCrateLarge, new Vector3(0f, 0f, 25f), 0f, "CrateCenterN");
            Instance(props, PCrateSmall, new Vector3(0f, 2.4f, 25f), -30f, "CrateCenterN");

            // Cajas dentro de las bases.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                Instance(props, PCrateSmall, new Vector3(sx * 36f, 0f, -9f), 0f, "CrateBase");
                Instance(props, PCrateSmall, new Vector3(sx * 37.4f, 0f, -9.4f), 20f, "CrateBase");
            }

            // Cobertura de los flancos y de las calles norte/sur.
            for (int sz = -1; sz <= 1; sz += 2)
            {
                PropPair(props, PBarrier, 33f, sz * 15f, 90f);
                PropPair(props, PCrateLarge, 9f, sz * 27f, 0f);
                PropPair(props, PCrateSmall, 7f, sz * 28.4f, 18f);
            }
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(0f, 0f, -30f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(0f, 0f, 30f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(-40f, 0f, -20f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(40f, 0f, -20f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(-40f, 0f, 20f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(40f, 0f, 20f), 270f, "Target");
        }

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.88f);
            sun.intensity = 2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;

            // Luz de relleno (sin sombras): levanta las caras en sombra sin aplanar.
            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(28f, 148f, 0f);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.62f, 0.72f, 0.85f);
            fill.intensity = 0.75f;
            fill.shadows = LightShadows.None;

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox.mat");
            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 0.8f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0035f;
            RenderSettings.fogColor = Hex(0x1A222C);

            // Luces interiores del edificio central.
            PointLight(parent, "LightMidN", new Vector3(0f, 3.7f, 7.5f), new Color(1f, 0.86f, 0.68f), 9f, 12f);
            PointLight(parent, "LightMidS", new Vector3(0f, 3.7f, -7.5f), new Color(1f, 0.86f, 0.68f), 9f, 12f);

            // Focos de las bases (para que el spawn no quede a oscuras).
            PointLight(parent, "LightBaseA", new Vector3(-40f, 5f, 0f), new Color(1f, 0.85f, 0.8f), 14f, 20f);
            PointLight(parent, "LightBaseB", new Vector3(40f, 5f, 0f), new Color(0.82f, 0.9f, 1f), 14f, 20f);

            var profile = LoadOrCreateProfile();
            var volumeGo = new GameObject("PostProcessing");
            volumeGo.transform.SetParent(parent, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.profile = profile;
        }

        private static void PointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static VolumeProfile LoadOrCreateProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            var bloom = ProfileComponent<Bloom>(profile);
            bloom.active = true;
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.55f);

            var tonemapping = ProfileComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = ProfileComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(0.35f);
            color.contrast.Override(10f);
            color.saturation.Override(-4f);

            var vignette = ProfileComponent<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(0.22f);
            vignette.smoothness.Override(0.45f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        /// <summary>Devuelve el componente del perfil o lo crea si aún no existe.</summary>
        private static T ProfileComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            return profile.TryGet<T>(out var existing) ? existing : profile.Add<T>(true);
        }

        // ------------------------------------------------------------------
        // Gameplay y HUD
        // ------------------------------------------------------------------

        private static MapDefinition BuildGameplay(Transform mapRoot)
        {
            var gameplay = Group(mapRoot, "Gameplay");
            var definitionGo = new GameObject("MapDefinition");
            definitionGo.transform.SetParent(gameplay, false);
            var definition = definitionGo.AddComponent<MapDefinition>();

            var spawns = Group(gameplay, "Spawns");
            var teamA = Group(spawns, "TeamA");
            var teamB = Group(spawns, "TeamB");
            var list = new List<SpawnPoint>();

            for (int i = 0; i < 5; i++)
            {
                float z = -8f + i * 4f;
                bool primary = i == 2;
                list.Add(MakeSpawn(teamA, "SpawnA" + (i + 1), new Vector3(-41f, 0.1f, z), 90f, SpawnPoint.Team.A, i, primary));
                list.Add(MakeSpawn(teamB, "SpawnB" + (i + 1), new Vector3(41f, 0.1f, z), -90f, SpawnPoint.Team.B, i, primary));
            }

            var serialized = new SerializedObject(definition);
            var array = serialized.FindProperty("spawnPoints");
            array.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return definition;
        }

        private static SpawnPoint MakeSpawn(Transform parent, string name, Vector3 position, float yaw,
            SpawnPoint.Team team, int slot, bool primary)
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

            if (PSpawnMarker != null)
            {
                var marker = Instance(go.transform, PSpawnMarker, Vector3.zero, 0f, "Marker");
                var tint = team == SpawnPoint.Team.A ? MTeamA : MTeamB;

                foreach (var renderer in marker.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.name == "Disc") renderer.sharedMaterial = tint;
                }
            }

            return point;
        }

        private static void BuildPlayer(MapDefinition definition)
        {
            RATIXMapKit.SpawnPlayer(definition);
        }

        private static void BuildHud()
        {
            RATIXMapKit.BuildHud();
        }

        // ------------------------------------------------------------------
        // Utilidades de escena
        // ------------------------------------------------------------------

        private static Transform Group(Transform parent, string name, Vector3 localPosition = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 size,
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

        private static GameObject Cylinder(Transform parent, string name, Vector3 position, float radius,
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

        private static GameObject Instance(Transform parent, GameObject prefab, Vector3 position, float yaw, string name = null)
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

        private static void PropPair(Transform parent, GameObject prefab, float x, float z, float yaw)
        {
            Instance(parent, prefab, new Vector3(x, 0f, z), yaw, prefab.name);
            Instance(parent, prefab, new Vector3(-x, 0f, z), -yaw, prefab.name);
        }

        private static void Ramp(Transform parent, string name, Vector3 from, Vector3 to, float width)
        {
            const float thickness = 0.3f;
            Vector3 delta = to - from;
            if (delta.sqrMagnitude < 0.001f) return;

            float length = delta.magnitude;
            Quaternion rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            Vector3 center = (from + to) * 0.5f - rotation * Vector3.up * (thickness * 0.5f);
            float half = width * 0.5f;

            Box(parent, name, center, new Vector3(width, thickness, length), MMetal, true, rotation);

            Vector3 trim = rotation * new Vector3(0f, thickness * 0.5f + 0.03f, 0f);
            Box(parent, name + "_TrimW", center + trim + rotation * new Vector3(-half + 0.08f, 0f, 0f),
                new Vector3(0.16f, 0.07f, length), MAccent, false, rotation);
            Box(parent, name + "_TrimE", center + trim + rotation * new Vector3(half - 0.08f, 0f, 0f),
                new Vector3(0.16f, 0.07f, length), MAccent, false, rotation);

            // Dos patas de apoyo a media rampa.
            Vector3 middle = (from + to) * 0.5f;
            float postHeight = Mathf.Max(0.3f, middle.y);
            Vector3 postCenter = new Vector3(middle.x, postHeight * 0.5f, middle.z);
            Box(parent, name + "_PostW", postCenter + rotation * new Vector3(-half + 0.35f, 0f, 0f),
                new Vector3(0.3f, postHeight, 0.3f), MMetalDark);
            Box(parent, name + "_PostE", postCenter + rotation * new Vector3(half - 0.35f, 0f, 0f),
                new Vector3(0.3f, postHeight, 0.3f), MMetalDark);
        }

        private static void Rail(Transform parent, string name, Vector3 center, Vector3 size)
        {
            Box(parent, name, center, size, MRail);
            Box(parent, name + "_Cap", center + Vector3.up * (size.y * 0.5f + 0.045f),
                new Vector3(size.x + 0.04f, 0.09f, size.z + 0.04f), MAccent, false);
        }

        private static void WallAlongX(Transform parent, string name, float z, float from, float to,
            float y, float height, float thickness, Material material, params Vector2[] gaps)
        {
            foreach (var run in Runs(from, to, gaps))
            {
                if (run.y - run.x < 0.05f) continue;
                Box(parent, name + "_" + run.x.ToString("0.#") + "_" + run.y.ToString("0.#"),
                    new Vector3((run.x + run.y) * 0.5f, y, z), new Vector3(run.y - run.x, height, thickness), material);
            }
        }

        private static void WallAlongZ(Transform parent, string name, float x, float from, float to,
            float y, float height, float thickness, Material material, params Vector2[] gaps)
        {
            foreach (var run in Runs(from, to, gaps))
            {
                if (run.y - run.x < 0.05f) continue;
                Box(parent, name + "_" + run.x.ToString("0.#") + "_" + run.y.ToString("0.#"),
                    new Vector3(x, y, (run.x + run.y) * 0.5f), new Vector3(thickness, height, run.y - run.x), material);
            }
        }

        private static List<Vector2> Runs(float from, float to, Vector2[] gaps)
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

        private static void SetObjectField(Object target, string field, Object value)
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

        private static void AddSceneToBuildSettings()
        {
            RATIXMapKit.AddSceneToBuildSettings(ScenePath);
        }
    }
}
