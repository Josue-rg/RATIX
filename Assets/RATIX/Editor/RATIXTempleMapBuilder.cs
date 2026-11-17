using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static RATIX.RATIXMapKit;

namespace RATIX
{
    /// <summary>
    /// Mapa de zombis **Templo de la Niebla**: ruinas en la selva. Pirámide
    /// escalonada transitable, estanques, muros derruidos, pasarelas de madera y
    /// maleza cerrada con niebla. Escena + prefabs editables.
    /// </summary>
    public static class RATIXTempleMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/TempleMist";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ZombieMap_04_Templo.unity";
        private const string ProfilePath = ArtFolder + "/Temple_Profile.asset";

        private const float HX = 48f;
        private const float HZ = 40f;
        private const float FenceH = 4f;

        private static Material MGround;
        private static Material MGroundDark;
        private static Material MStone;
        private static Material MStoneDark;
        private static Material MMoss;
        private static Material MWater;
        private static Material MWood;
        private static Material MWoodDark;
        private static Material MLeaf;
        private static Material MLeafDark;
        private static Material MTrunk;
        private static Material MTeamA;
        private static Material MTeamB;
        private static Material MTeamAPad;
        private static Material MTeamBPad;
        private static Material MTargetFace;
        private static Material MTargetCenter;
        private static Material MTargetHit;

        private static GameObject PJungle;
        private static GameObject PBigTree;
        private static GameObject PCrate;
        private static GameObject PSpawnMarker;
        private static GameObject PTarget;

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Zombi (Templo de la Niebla)", false, 90)]
        public static void BuildTempleMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildPrefabs();
            RATIXMapKit.BuildPlayerPrefab();

            var mapRoot = new GameObject("ZombieMap_04_Templo").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildJungle(environment);
            BuildPyramid(environment);
            BuildRuins(environment);
            BuildPools(environment);
            BuildProps(environment);
            BuildTargets(environment);
            BuildLighting(lighting);

            MapDefinition definition = BuildGameplay(mapRoot);
            RATIXMapKit.SpawnPlayer(definition);
            RATIXMapKit.BuildHud();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = definition.gameObject;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Templo de la Niebla construido"));
            }

            Debug.Log("[RATIX] Mapa Zombi Templo de la Niebla construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Zombi (Templo de la Niebla)", false, 91)]
        public static void OpenTempleMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        // ------------------------------------------------------------------
        // Assets
        // ------------------------------------------------------------------

        private static void CreateFolders()
        {
            EnsurePath(TextureFolder);
            EnsurePath(MaterialFolder);
            EnsurePath(PrefabFolder);
        }

        private static void BuildTextures()
        {
            EnsureSandTexture(TextureFolder, "T_Moss", new Color32(82, 96, 62, 255), new Color32(66, 78, 50, 255));
            EnsureBrickTexture(TextureFolder, "T_TempleStone", new Color32(126, 132, 112, 255), new Color32(88, 94, 78, 255), 128, 64);
        }

        private static void BuildMaterials()
        {
            MGround = Lit(MaterialFolder, "M_Ground", Color.white, 0f, 0.1f,
                TextureFolder + "/T_Moss.png", new Vector2(18f, 15f));
            MGroundDark = Lit(MaterialFolder, "M_GroundDark", Hex(0x44502F), 0f, 0.1f);
            MStone = Lit(MaterialFolder, "M_Stone", Color.white, 0f, 0.18f,
                TextureFolder + "/T_TempleStone.png", new Vector2(2f, 1f));
            MStoneDark = Lit(MaterialFolder, "M_StoneDark", Hex(0x5E6B52), 0f, 0.15f);
            MMoss = Lit(MaterialFolder, "M_Moss", Hex(0x4F6B3A), 0f, 0.1f);
            MWater = Lit(MaterialFolder, "M_Water", Hex(0x25413D), 0.35f, 0.92f);
            MWood = Lit(MaterialFolder, "M_Wood", Hex(0x6B4E2E), 0f, 0.16f);
            MWoodDark = Lit(MaterialFolder, "M_WoodDark", Hex(0x3E2C1A), 0f, 0.14f);
            MLeaf = Lit(MaterialFolder, "M_Leaf", Hex(0x4E6B38), 0f, 0.1f);
            MLeafDark = Lit(MaterialFolder, "M_LeafDark", Hex(0x3A5228), 0f, 0.1f);
            MTrunk = Lit(MaterialFolder, "M_Trunk", Hex(0x4A3A28), 0f, 0.12f);
            MTeamA = Lit(MaterialFolder, "M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit(MaterialFolder, "M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit(MaterialFolder, "M_TeamA_Pad", Color.Lerp(Hex(0x2F3A28), Hex(0xCE5A46), 0.4f), 0f, 0.2f);
            MTeamBPad = Lit(MaterialFolder, "M_TeamB_Pad", Color.Lerp(Hex(0x2F3A28), Hex(0x6B9BC9), 0.4f), 0f, 0.2f);
            MTargetFace = Lit(MaterialFolder, "M_TargetFace", Hex(0xDCD6C2), 0f, 0.3f);
            MTargetCenter = Lit(MaterialFolder, "M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit(MaterialFolder, "M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            Skybox(MaterialFolder, "M_Skybox_Temple", Hex(0xA9B89B), Hex(0x4E5A42), 0.9f, 1.35f, 0.02f);
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildPrefabs()
        {
            PJungle = BuildJungleClump();
            PBigTree = BuildBigTree();
            PCrate = BuildCrate();
            PSpawnMarker = BuildSpawnMarker();
            PTarget = BuildTarget();
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildJungleClump()
        {
            var root = new GameObject("Prop_Jungle");

            Cylinder(root.transform, "Stem", new Vector3(0f, 1.1f, 0f), 0.16f, 2.2f, MTrunk, false);
            Box(root.transform, "LeafA", new Vector3(0f, 2.2f, 0f), new Vector3(2.4f, 0.7f, 2.2f), MLeaf, false,
                Quaternion.Euler(0f, 18f, 0f));
            Box(root.transform, "LeafB", new Vector3(0.4f, 1.5f, -0.3f), new Vector3(2f, 0.6f, 1.8f), MLeafDark, false,
                Quaternion.Euler(0f, -30f, 4f));
            Box(root.transform, "Fern", new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(1.6f, 0.7f, 1.4f), MLeafDark, false,
                Quaternion.Euler(0f, 40f, -6f));

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 1f, 0f);
            collider.height = 2f;
            collider.radius = 0.35f;

            return SavePrefab(PrefabFolder, "Prop_Jungle", root);
        }

        private static GameObject BuildBigTree()
        {
            var root = new GameObject("Prop_BigTree");

            Cylinder(root.transform, "Trunk", new Vector3(0f, 3.4f, 0f), 0.45f, 6.8f, MTrunk);
            Box(root.transform, "CanopyA", new Vector3(0f, 7.4f, 0f), new Vector3(7f, 1.8f, 7f), MLeaf, false,
                Quaternion.Euler(0f, 12f, 0f));
            Box(root.transform, "CanopyB", new Vector3(1.2f, 6.5f, -1f), new Vector3(5f, 1.4f, 5f), MLeafDark, false);

            return SavePrefab(PrefabFolder, "Prop_BigTree", root);
        }

        private static GameObject BuildCrate()
        {
            var root = new GameObject("Prop_Crate");

            Box(root.transform, "Body", new Vector3(0f, 0.7f, 0f), new Vector3(1.4f, 1.4f, 1.4f), MWood);
            Box(root.transform, "Lid", new Vector3(0f, 1.42f, 0f), new Vector3(1.3f, 0.06f, 1.3f), MWoodDark, false);

            return SavePrefab(PrefabFolder, "Prop_Crate", root);
        }

        private static GameObject BuildSpawnMarker()
        {
            var root = new GameObject("Prop_SpawnMarker");
            var t = root.transform;

            Cylinder(t, "Ring", new Vector3(0f, 0.015f, 0f), 0.88f, 0.03f, MStoneDark, false);
            Cylinder(t, "Disc", new Vector3(0f, 0.035f, 0f), 0.72f, 0.07f, MTeamA, false);

            return SavePrefab(PrefabFolder, "Prop_SpawnMarker", root);
        }

        private static GameObject BuildTarget()
        {
            var root = new GameObject("Prop_Target");
            var t = root.transform;

            Cylinder(t, "Base", new Vector3(0f, 0.06f, 0f), 0.45f, 0.12f, MStoneDark);

            var pivot = Group(t, "Pivot", new Vector3(0f, 0.12f, 0f));
            Box(pivot, "Post", new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 1.6f, 0.12f), MWood);
            var plate = Cylinder(pivot, "Plate", new Vector3(0f, 1.45f, 0f), 0.55f, 0.09f, MTargetFace, true,
                Quaternion.Euler(90f, 0f, 0f));
            Cylinder(pivot, "Bullseye", new Vector3(0f, 1.45f, 0.055f), 0.2f, 0.05f, MTargetCenter, false,
                Quaternion.Euler(90f, 0f, 0f));

            var dummy = root.AddComponent<TargetDummy>();
            SetObjectField(dummy, "plate", plate.GetComponent<Renderer>());
            SetObjectField(dummy, "idleMaterial", MTargetFace);
            SetObjectField(dummy, "hitMaterial", MTargetHit);
            SetObjectField(dummy, "fallPivot", pivot);

            return SavePrefab(PrefabFolder, "Prop_Target", root);
        }

        // ------------------------------------------------------------------
        // Selva
        // ------------------------------------------------------------------

        private static void BuildJungle(Transform environment)
        {
            var jungle = Group(environment, "Jungle");

            Box(jungle, "Ground", new Vector3(0f, -0.5f, 0f), new Vector3(HX * 2f, 1f, HZ * 2f), MGround);

            // Muro perimetral de piedra cubierto de musgo.
            Box(jungle, "WallNorth", new Vector3(0f, FenceH * 0.5f, HZ), new Vector3(HX * 2f + 2f, FenceH, 1.2f), MStone);
            Box(jungle, "WallSouth", new Vector3(0f, FenceH * 0.5f, -HZ), new Vector3(HX * 2f + 2f, FenceH, 1.2f), MStone);
            Box(jungle, "WallEast", new Vector3(HX, FenceH * 0.5f, 0f), new Vector3(1.2f, FenceH, HZ * 2f), MStone);
            Box(jungle, "WallWest", new Vector3(-HX, FenceH * 0.5f, 0f), new Vector3(1.2f, FenceH, HZ * 2f), MStone);
            Box(jungle, "MossNorth", new Vector3(0f, FenceH - 0.45f, HZ - 0.1f), new Vector3(HX * 2f, 0.9f, 0.25f), MMoss, false);

            for (int i = 0; i < 14; i++)
            {
                float angle = i * (360f / 14f);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Vector3 position = direction * (28f + (i % 3) * 4f);
                position.y = 0f;
                Instance(jungle, PBigTree, position, angle, "BigTree");
            }

            // Maleza por los bordes y entre las ruinas.
            var random = new System.Random(5150);
            for (int i = 0; i < 34; i++)
            {
                float angle = (float)(random.NextDouble() * 360.0);
                float radius = 20f + (float)random.NextDouble() * 16f;
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, radius);
                if (Mathf.Abs(position.x) < 18f && Mathf.Abs(position.z) < 18f) continue;
                Instance(jungle, PJungle, new Vector3(position.x, 0f, position.z), angle * 1.7f, "Jungle");
            }
        }

        // ------------------------------------------------------------------
        // Pirámide
        // ------------------------------------------------------------------

        private static void BuildPyramid(Transform environment)
        {
            var pyramid = Group(environment, "Pyramid");

            float[] half = { 15f, 12f, 9f, 6f, 3f };
            for (int i = 0; i < half.Length; i++)
            {
                float y = 0.6f + i * 1.2f;
                Box(pyramid, "Tier" + i, new Vector3(0f, y, 0f), new Vector3(half[i] * 2f, 1.2f, half[i] * 2f), MStone);
                Box(pyramid, "TierMoss" + i, new Vector3(0f, y + 0.62f, 0f), new Vector3(half[i] * 2f + 0.1f, 0.06f, half[i] * 2f + 0.1f), MMoss, false);
            }

            // Escalera norte: un tramo sobre cada terraza.
            for (int i = 0; i < half.Length; i++)
            {
                float zFrom = 18f - i * 3f;
                float yFrom = i * 1.2f;
                Stairs(pyramid, "Flight" + i, new Vector3(0f, yFrom, zFrom), new Vector3(0f, yFrom + 1.2f, zFrom - 3f),
                    4f, 6, MStoneDark);
            }

            // Templo superior.
            Box(pyramid, "TopFloor", new Vector3(0f, 6.06f, 0f), new Vector3(6.4f, 0.12f, 6.4f), MStoneDark, false);

            foreach (float x in new[] { -2.3f, 2.3f })
            {
                foreach (float z in new[] { -2.3f, 2.3f })
                {
                    Box(pyramid, "ShrinePillar" + x + z, new Vector3(x, 7.05f, z), new Vector3(0.6f, 2f, 0.6f), MStone);
                }
            }

            Box(pyramid, "ShrineRoof", new Vector3(0f, 8.15f, 0f), new Vector3(6.2f, 0.35f, 6.2f), MStoneDark);
            Box(pyramid, "Altar", new Vector3(0f, 6.45f, 0f), new Vector3(2.2f, 0.8f, 1.4f), MStoneDark);

            // Antorchas del templo.
            foreach (float x in new[] { -2.9f, 2.9f })
            {
                Box(pyramid, "Brazier" + x, new Vector3(x, 6.5f, 2.9f), new Vector3(0.5f, 0.9f, 0.5f), MWoodDark);
            }
        }

        // ------------------------------------------------------------------
        // Ruinas y estanques
        // ------------------------------------------------------------------

        private static void BuildRuins(Transform environment)
        {
            var ruins = Group(environment, "Ruins");

            foreach (float sx in new[] { -1f, 1f })
            {
                foreach (float sz in new[] { -1f, 1f })
                {
                    var ruin = Group(ruins, "Ruin" + sx + sz);
                    float cx = sx * 31f;
                    float cz = sz * 26f;

                    Box(ruin, "WallA", new Vector3(cx, 1.8f, cz), new Vector3(10f, 3.6f, 0.9f), MStone);
                    Box(ruin, "WallB", new Vector3(cx + 4.5f, 1.2f, cz + sz * 4f), new Vector3(0.9f, 2.4f, 8f), MStone);
                    Box(ruin, "Lintel", new Vector3(cx - 4.6f, 3.2f, cz), new Vector3(2.2f, 0.6f, 1.1f), MStoneDark);
                    Box(ruin, "Moss", new Vector3(cx, 3.65f, cz), new Vector3(10f, 0.1f, 1f), MMoss, false);

                    Cylinder(ruin, "Fallen1", new Vector3(cx + 4f, 0.45f, cz - sz * 2.6f), 0.45f, 4f, MStone,
                        true, Quaternion.Euler(90f, 0f, 0f));
                    Cylinder(ruin, "Fallen2", new Vector3(cx + 3f, 0.45f, cz - sz * 3.4f), 0.45f, 3f, MStone,
                        true, Quaternion.Euler(90f, 0f, 20f));
                }
            }

            // Pasarela de madera al oeste.
            var walkway = Group(ruins, "Walkway");
            Box(walkway, "Deck", new Vector3(-28f, 1.15f, 0f), new Vector3(14f, 0.3f, 3.4f), MWood);
            Box(walkway, "RailN", new Vector3(-28f, 1.75f, 1.6f), new Vector3(14f, 0.12f, 0.12f), MWoodDark, false);
            Box(walkway, "RailS", new Vector3(-28f, 1.75f, -1.6f), new Vector3(14f, 0.12f, 0.12f), MWoodDark, false);

            foreach (float x in new[] { -34f, -28f, -22f })
            {
                Box(walkway, "PostN" + x, new Vector3(x, 0.85f, 1.6f), new Vector3(0.2f, 1.7f, 0.2f), MWoodDark);
                Box(walkway, "PostS" + x, new Vector3(x, 0.85f, -1.6f), new Vector3(0.2f, 1.7f, 0.2f), MWoodDark);
            }

            Stairs(walkway, "WalkwayStairs", new Vector3(-37f, 0f, 0f), new Vector3(-35.2f, 1.3f, 0f), 3f, 8, MWoodDark);
        }

        private static void BuildPools(Transform environment)
        {
            var pools = Group(environment, "Pools");

            foreach (float sx in new[] { -1f, 1f })
            {
                float cx = sx * 26f;

                Box(pools, "Water" + sx, new Vector3(cx, 0.04f, 14f), new Vector3(11f, 0.08f, 13f), MWater, false);

                Box(pools, "EdgeN" + sx, new Vector3(cx, 0.16f, 20.8f), new Vector3(12f, 0.35f, 0.7f), MStoneDark);
                Box(pools, "EdgeS" + sx, new Vector3(cx, 0.16f, 7.2f), new Vector3(12f, 0.35f, 0.7f), MStoneDark);
                Box(pools, "EdgeW" + sx, new Vector3(cx - 5.8f, 0.16f, 14f), new Vector3(0.7f, 0.35f, 13f), MStoneDark);
                Box(pools, "EdgeE" + sx, new Vector3(cx + 5.8f, 0.16f, 14f), new Vector3(0.7f, 0.35f, 13f), MStoneDark);

                // Juncos.
                for (int i = 0; i < 4; i++)
                {
                    Cylinder(pools, "Reed" + sx + i, new Vector3(cx - 4f + i * 2.6f, 0.9f, 18.6f), 0.06f, 1.8f, MLeafDark, false);
                }
            }
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            foreach (var spot in new[]
            {
                new Vector3(-12f, 0f, 22f), new Vector3(12f, 0f, 22f),
                new Vector3(-12f, 0f, -22f), new Vector3(12f, 0f, -22f),
                new Vector3(20f, 0f, 0f), new Vector3(-20f, 0f, 0f)
            })
            {
                Instance(props, PCrate, spot, 18f, "Crate");
            }

            foreach (var spot in new[]
            {
                new Vector3(0f, 0f, 26f), new Vector3(0f, 0f, -26f),
                new Vector3(36f, 0f, 8f), new Vector3(-36f, 0f, -8f)
            })
            {
                Instance(props, PJungle, spot, 20f, "Jungle");
            }
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(0f, 0f, 30f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(-16f, 0f, 0f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(16f, 0f, 0f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(30f, 0f, -18f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(-30f, 0f, -18f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(0f, 6.1f, 0f), 0f, "Target");
        }

        // ------------------------------------------------------------------
        // Luz, juego y cierre
        // ------------------------------------------------------------------

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(46f, -64f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.98f, 0.88f);
            sun.intensity = 2.4f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.78f;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(24f, 120f, 0f);

            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.66f, 0.8f, 0.66f);
            fill.intensity = 0.7f;
            fill.shadows = LightShadows.None;

            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox_Temple.mat");
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.35f;
            RenderSettings.reflectionIntensity = 0.6f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0055f;
            RenderSettings.fogColor = Hex(0x8E9A82);

            PointLight(parent, "BrazierA", new Vector3(-2.9f, 7f, 2.9f), new Color(1f, 0.6f, 0.25f), 14f, 12f);
            PointLight(parent, "BrazierB", new Vector3(2.9f, 7f, 2.9f), new Color(1f, 0.6f, 0.25f), 14f, 12f);
            PointLight(parent, "LightWalkway", new Vector3(-28f, 3f, 0f), new Color(0.85f, 0.9f, 0.7f), 10f, 12f);

            var profile = LoadOrCreateProfile();
            var volumeGo = new GameObject("PostProcessing");
            volumeGo.transform.SetParent(parent, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.profile = profile;
        }

        private static void PointLight(Transform parent, string name, Vector3 position, Color color,
            float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

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
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.4f);
            bloom.scatter.Override(0.6f);

            var tonemapping = ProfileComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = ProfileComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(0.35f);
            color.contrast.Override(11f);
            color.saturation.Override(4f);

            var vignette = ProfileComponent<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.45f);

            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T ProfileComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            return profile.TryGet<T>(out var existing) ? existing : profile.Add<T>(true);
        }

        private static MapDefinition BuildGameplay(Transform mapRoot)
        {
            var gameplay = Group(mapRoot, "Gameplay");
            var definitionGo = new GameObject("MapDefinition");
            definitionGo.transform.SetParent(gameplay, false);

            var definition = definitionGo.AddComponent<MapDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("mapId").stringValue = "zombie_temple_mist";
            serialized.FindProperty("nameKey").stringValue = "value.temple_mist";
            serialized.FindProperty("displayName").stringValue = "Temple of Mist";
            serialized.FindProperty("mode").enumValueIndex = (int)MapDefinition.GameMode.Zombie;
            serialized.FindProperty("teams").intValue = 2;
            serialized.FindProperty("recommendedPlayers").intValue = 12;
            serialized.FindProperty("roundSeconds").floatValue = 420f;
            serialized.FindProperty("sceneName").stringValue = "ZombieMap_04_Templo";
            serialized.FindProperty("arenaSize").vector3Value = new Vector3(HX * 2f, 8.5f, HZ * 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var spawns = Group(gameplay, "Spawns");
            var humans = Group(spawns, "Survivors");
            var infected = Group(spawns, "Infected");
            var list = new List<SpawnPoint>();

            for (int i = 0; i < 6; i++)
            {
                float x = -7.5f + i * 3f;
                list.Add(CreateSpawn(humans, "SpawnHuman" + (i + 1), new Vector3(x, 0.1f, 22f), 180f,
                    SpawnPoint.Team.A, i, i == 2, PSpawnMarker, MTeamA));
            }

            for (int i = 0; i < 6; i++)
            {
                float x = -7.5f + i * 3f;
                list.Add(CreateSpawn(infected, "SpawnInfected" + (i + 1), new Vector3(x, 0.1f, -26f), 0f,
                    SpawnPoint.Team.B, i, i == 2, PSpawnMarker, MTeamB));
            }

            var definitionSerialized = new SerializedObject(definition);
            SetArray(definitionSerialized, "spawnPoints", list.ToArray());

            return definition;
        }
    }
}
