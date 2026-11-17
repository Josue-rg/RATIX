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
    /// Mapa de zombis **Hacienda El Alba**: finca abierta al atardecer. Zonas muy
    /// distintas para la ronda: cañaveral (laberinto bajo), canal seco con tres
    /// puentes, hacienda de dos plantas con porche y tejado, granero con pajar,
    /// silo, torre de agua y campos abiertos. Todo escena + prefabs editables.
    /// </summary>
    public static class RATIXHaciendaMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/HaciendaAlba";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ZombieMap_02_Hacienda.unity";
        private const string ProfilePath = ArtFolder + "/Hacienda_Profile.asset";

        private const float HX = 50f;
        private const float HZ = 38f;
        private const float FenceH = 3.6f;

        // Hacienda.
        private const float HouseX = -26f;
        private const float HouseZ = -20f;
        private const float HouseW = 26f;
        private const float HouseD = 18f;
        private const float GroundH = 3.2f;
        private const float SlabTop = 3.5f;
        private const float HouseRoof = 6f;

        // Granero.
        private const float BarnX = 24f;
        private const float BarnZ = -22f;
        private const float BarnW = 22f;
        private const float BarnD = 16f;
        private const float BarnWall = 4.2f;
        private const float BarnRidge = 7.5f;

        private static Material MGround;
        private static Material MGroundDark;
        private static Material MDirt;
        private static Material MStone;
        private static Material MStoneDark;
        private static Material MPlaster;
        private static Material MTerracotta;
        private static Material MWood;
        private static Material MWoodDark;
        private static Material MCane;
        private static Material MCaneLeaf;
        private static Material MStraw;
        private static Material MMetal;
        private static Material MMetalDark;
        private static Material MTeamA;
        private static Material MTeamB;
        private static Material MTeamAPad;
        private static Material MTeamBPad;
        private static Material MTargetFace;
        private static Material MTargetCenter;
        private static Material MTargetHit;

        private static GameObject PCane;
        private static GameObject PFence;
        private static GameObject PHayBale;
        private static GameObject PBarrel;
        private static GameObject PCrate;
        private static GameObject PSpawnMarker;
        private static GameObject PTarget;

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Zombi (Hacienda El Alba)", false, 50)]
        public static void BuildHaciendaMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildPrefabs();
            RATIXMapKit.BuildPlayerPrefab();

            var mapRoot = new GameObject("ZombieMap_02_Hacienda").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildGround(environment);
            BuildHouse(environment);
            BuildBarn(environment);
            BuildSilo(environment);
            BuildWaterTower(environment);
            BuildCanal(environment);
            BuildFields(environment);
            BuildProps(environment);
            BuildTargets(environment);
            BuildLighting(lighting);
            RATIXMapKit.Polish(environment);
            RATIXMapKit.Polish(lighting);

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
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Hacienda El Alba construida"));
            }

            Debug.Log("[RATIX] Mapa Zombi Hacienda El Alba construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Zombi (Hacienda El Alba)", false, 51)]
        public static void OpenHaciendaMap()
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
            EnsureSandTexture(TextureFolder, "T_Grass", new Color32(96, 106, 68, 255), new Color32(80, 90, 58, 255));
            EnsureSandTexture(TextureFolder, "T_Dirt", new Color32(136, 112, 80, 255), new Color32(116, 94, 66, 255));
            EnsureBrickTexture(TextureFolder, "T_Plaster", new Color32(206, 182, 148, 255), new Color32(168, 144, 112, 255), 160, 80);
            EnsureBrickTexture(TextureFolder, "T_WoodPlanks", new Color32(112, 76, 44, 255), new Color32(66, 42, 22, 255), 512, 40);
        }

        private static void BuildMaterials()
        {
            MGround = Lit(MaterialFolder, "M_Grass", Color.white, 0f, 0.1f,
                TextureFolder + "/T_Grass.png", new Vector2(20f, 15f));
            MGroundDark = Lit(MaterialFolder, "M_GrassDark", Hex(0x4E5535), 0f, 0.1f);
            MDirt = Lit(MaterialFolder, "M_Dirt", Color.white, 0f, 0.15f,
                TextureFolder + "/T_Dirt.png", new Vector2(8f, 6f));
            MStone = Lit(MaterialFolder, "M_Stone", Hex(0x9C8763), 0f, 0.16f);
            MStoneDark = Lit(MaterialFolder, "M_StoneDark", Hex(0x6E5C42), 0f, 0.14f);
            MPlaster = Lit(MaterialFolder, "M_Plaster", Color.white, 0f, 0.2f,
                TextureFolder + "/T_Plaster.png", new Vector2(3f, 2f));
            MTerracotta = Lit(MaterialFolder, "M_Terracotta", Hex(0x9C4A2E), 0f, 0.25f);
            MWood = Lit(MaterialFolder, "M_Wood", Hex(0x8A6438), 0f, 0.18f);
            MWoodDark = Lit(MaterialFolder, "M_WoodDark", Color.white, 0f, 0.15f,
                TextureFolder + "/T_WoodPlanks.png", new Vector2(2f, 2f));
            MCane = Lit(MaterialFolder, "M_Cane", Hex(0x9CA352), 0f, 0.12f);
            MCaneLeaf = Lit(MaterialFolder, "M_CaneLeaf", Hex(0x6B7A34), 0f, 0.1f);
            MStraw = Lit(MaterialFolder, "M_Straw", Hex(0xC8A75E), 0f, 0.12f);
            MMetal = Lit(MaterialFolder, "M_Metal", Hex(0x7A7468), 0.5f, 0.35f);
            MMetalDark = Lit(MaterialFolder, "M_MetalDark", Hex(0x3B3833), 0.6f, 0.3f);
            MTeamA = Lit(MaterialFolder, "M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit(MaterialFolder, "M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit(MaterialFolder, "M_TeamA_Pad", Color.Lerp(Hex(0x6E5C42), Hex(0xCE5A46), 0.35f), 0f, 0.2f);
            MTeamBPad = Lit(MaterialFolder, "M_TeamB_Pad", Color.Lerp(Hex(0x6E5C42), Hex(0x6B9BC9), 0.35f), 0f, 0.2f);
            MTargetFace = Lit(MaterialFolder, "M_TargetFace", Hex(0xE3D6BE), 0f, 0.3f);
            MTargetCenter = Lit(MaterialFolder, "M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit(MaterialFolder, "M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            Skybox(MaterialFolder, "M_Skybox_Alba", Hex(0xE3A971), Hex(0x8A7050), 2f, 1.5f, 0.05f);
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildPrefabs()
        {
            PCane = BuildCane();
            PFence = BuildFence();
            PHayBale = BuildHayBale();
            PBarrel = BuildBarrel();
            PCrate = BuildCrate();
            PSpawnMarker = BuildSpawnMarker();
            PTarget = BuildTarget();
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildCane()
        {
            var root = new GameObject("Prop_Cane");
            var t = root.transform;

            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -0.28f : 0.28f);
                float z = (i < 2 ? -0.22f : 0.22f);
                Box(t, "Stalk" + i, new Vector3(x, 1.55f, z), new Vector3(0.07f, 3.1f, 0.07f), MCane, false,
                    Quaternion.Euler(i * 2f - 3f, i * 17f, i * 2.5f));
            }

            Box(t, "LeafA", new Vector3(0f, 2.35f, 0f), new Vector3(1.15f, 1.5f, 1.15f), MCaneLeaf, false,
                Quaternion.Euler(0f, 20f, 6f));
            Box(t, "LeafB", new Vector3(0f, 1.6f, 0f), new Vector3(1f, 1.1f, 1.3f), MCaneLeaf, false,
                Quaternion.Euler(0f, -26f, -5f));

            // Sin colisión individual: la parcela se cubre con un volumen aparte.
            return SavePrefab(PrefabFolder, "Prop_Cane", root);
        }

        private static GameObject BuildFence()
        {
            var root = new GameObject("Prop_Fence");
            var t = root.transform;

            for (int i = 0; i < 3; i++)
            {
                Box(t, "Post" + i, new Vector3(-2f + i * 2f, 0.85f, 0f), new Vector3(0.18f, 1.7f, 0.18f), MWoodDark);
            }

            Box(t, "RailA", new Vector3(0f, 1.3f, 0f), new Vector3(6f, 0.14f, 0.08f), MWood, false);
            Box(t, "RailB", new Vector3(0f, 0.7f, 0f), new Vector3(6f, 0.14f, 0.08f), MWood, false);

            return SavePrefab(PrefabFolder, "Prop_Fence", root);
        }

        private static GameObject BuildHayBale()
        {
            var root = new GameObject("Prop_HayBale");
            var t = root.transform;

            Cylinder(t, "Bale", new Vector3(0f, 0.8f, 0f), 0.85f, 1.6f, MStraw,
                true, Quaternion.Euler(90f, 0f, 0f));

            return SavePrefab(PrefabFolder, "Prop_HayBale", root);
        }

        private static GameObject BuildBarrel()
        {
            var root = new GameObject("Prop_Barrel");
            var t = root.transform;

            Cylinder(t, "Body", new Vector3(0f, 0.58f, 0f), 0.42f, 1.16f, MWoodDark);
            Cylinder(t, "BandTop", new Vector3(0f, 0.98f, 0f), 0.44f, 0.08f, MMetalDark, false);
            Cylinder(t, "BandBottom", new Vector3(0f, 0.2f, 0f), 0.44f, 0.08f, MMetalDark, false);

            return SavePrefab(PrefabFolder, "Prop_Barrel", root);
        }

        private static GameObject BuildCrate()
        {
            var root = new GameObject("Prop_Crate");
            const float size = 1.4f;

            Box(root.transform, "Body", new Vector3(0f, size * 0.5f, 0f), new Vector3(size, size, size), MWood);
            Box(root.transform, "Lid", new Vector3(0f, size + 0.02f, 0f), new Vector3(size * 0.92f, 0.06f, size * 0.92f), MWoodDark, false);

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

            Cylinder(t, "Base", new Vector3(0f, 0.06f, 0f), 0.45f, 0.12f, MMetalDark);

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
        // Terreno y canal
        // ------------------------------------------------------------------

        private static void BuildGround(Transform environment)
        {
            var ground = Group(environment, "Ground");

            Box(ground, "GrassNorth", new Vector3(0f, -0.5f, 20.5f), new Vector3(HX * 2f, 1f, 35f), MGround);
            Box(ground, "GrassSouth", new Vector3(0f, -0.5f, -20.5f), new Vector3(HX * 2f, 1f, 35f), MGround);

            // Muro perimetral de piedra + valla de madera por dentro.
            Box(ground, "WallNorth", new Vector3(0f, FenceH * 0.5f, HZ), new Vector3(HX * 2f + 2f, FenceH, 1f), MStone);
            Box(ground, "WallSouth", new Vector3(0f, FenceH * 0.5f, -HZ), new Vector3(HX * 2f + 2f, FenceH, 1f), MStone);
            Box(ground, "WallEast", new Vector3(HX, FenceH * 0.5f, 0f), new Vector3(1f, FenceH, HZ * 2f), MStone);
            Box(ground, "WallWest", new Vector3(-HX, FenceH * 0.5f, 0f), new Vector3(1f, FenceH, HZ * 2f), MStone);

            Box(ground, "CapNorth", new Vector3(0f, FenceH + 0.1f, HZ), new Vector3(HX * 2f + 2.4f, 0.2f, 1.4f), MStoneDark, false);
            Box(ground, "CapSouth", new Vector3(0f, FenceH + 0.1f, -HZ), new Vector3(HX * 2f + 2.4f, 0.2f, 1.4f), MStoneDark, false);
            Box(ground, "CapEast", new Vector3(HX, FenceH + 0.1f, 0f), new Vector3(1.4f, 0.2f, HZ * 2f + 2.4f), MStoneDark, false);
            Box(ground, "CapWest", new Vector3(-HX, FenceH + 0.1f, 0f), new Vector3(1.4f, 0.2f, HZ * 2f + 2.4f), MStoneDark, false);

            // Postes de piedra cada 12 m.
            for (float x = -48f; x <= 48f; x += 12f)
            {
                Box(ground, "PostN" + x, new Vector3(x, 2.1f, HZ - 0.9f), new Vector3(1f, 4.2f, 1f), MStoneDark);
                Box(ground, "PostS" + x, new Vector3(x, 2.1f, -HZ + 0.9f), new Vector3(1f, 4.2f, 1f), MStoneDark);
            }
        }

        private static void BuildCanal(Transform environment)
        {
            var canal = Group(environment, "Canal");

            Box(canal, "Bed", new Vector3(0f, -2.1f, 0f), new Vector3(HX * 2f, 1f, 6f), MDirt);
            Box(canal, "WallN", new Vector3(0f, -0.85f, 3f), new Vector3(HX * 2f, 1.6f, 0.5f), MStoneDark);
            Box(canal, "WallS", new Vector3(0f, -0.85f, -3f), new Vector3(HX * 2f, 1.6f, 0.5f), MStoneDark);
            Box(canal, "CopingN", new Vector3(0f, 0.04f, 3.3f), new Vector3(HX * 2f, 0.12f, 0.7f), MStone, false);
            Box(canal, "CopingS", new Vector3(0f, 0.04f, -3.3f), new Vector3(HX * 2f, 0.12f, 0.7f), MStone, false);

            // Tres puentes de madera.
            foreach (float x in new[] { -20f, 0f, 20f })
            {
                var bridge = Group(canal, "Bridge" + x);
                Box(bridge, "Deck", new Vector3(x, 0.15f, 0f), new Vector3(5f, 0.3f, 8.4f), MWood);
                Box(bridge, "BeamA", new Vector3(x - 2.3f, 0.05f, 0f), new Vector3(0.3f, 0.2f, 8.4f), MWoodDark, false);
                Box(bridge, "BeamB", new Vector3(x + 2.3f, 0.05f, 0f), new Vector3(0.3f, 0.2f, 8.4f), MWoodDark, false);

                WoodRail(bridge, "RailW", new Vector3(x - 2.4f, 0.3f, 0f), 8.4f, true);
                WoodRail(bridge, "RailE", new Vector3(x + 2.4f, 0.3f, 0f), 8.4f, true);

                // Postes de apoyo en el cauce.
                Box(bridge, "PierN", new Vector3(x, -0.8f, 2.2f), new Vector3(0.4f, 1.6f, 0.4f), MWoodDark);
                Box(bridge, "PierS", new Vector3(x, -0.8f, -2.2f), new Vector3(0.4f, 1.6f, 0.4f), MWoodDark);
            }
        }

        // ------------------------------------------------------------------
        // Hacienda
        // ------------------------------------------------------------------

        /// <summary>Barandilla de madera con postes y dos travesaños.</summary>
        private static void WoodRail(Transform parent, string name, Vector3 baseCenter, float length, bool alongZ)
        {
            float half = length * 0.5f;
            int posts = Mathf.Max(2, Mathf.RoundToInt(length / 2f) + 1);

            for (int i = 0; i < posts; i++)
            {
                float offset = -half + i * (length / (posts - 1));
                Vector3 position = baseCenter + (alongZ ? new Vector3(0f, 0f, offset) : new Vector3(offset, 0f, 0f));
                Box(parent, name + "_P" + i, position + Vector3.up * 0.55f, new Vector3(0.13f, 1.1f, 0.13f), MWoodDark, false);
            }

            Vector3 railSize = alongZ ? new Vector3(0.1f, 0.12f, length) : new Vector3(length, 0.12f, 0.1f);
            Box(parent, name + "_R1", baseCenter + Vector3.up * 0.98f, railSize, MWood, false);
            Box(parent, name + "_R2", baseCenter + Vector3.up * 0.58f, railSize, MWood, false);
        }

        private static void BuildHouse(Transform environment)
        {
            var house = Group(environment, "Hacienda");
            house.localPosition = new Vector3(HouseX, 0f, HouseZ);

            float halfW = HouseW * 0.5f;
            float halfD = HouseD * 0.5f;
            const float t = 0.6f;

            // Planta baja: puerta principal al porche (norte) y puerta lateral.
            WallWithOpenings(house, "GroundN", true, halfD, -halfW, halfW, 0f, GroundH, t, MPlaster,
                new RATIXMapKit.Opening(0f, 2.8f, 0f, 2.9f),
                new RATIXMapKit.Opening(8f, 1.8f, 1.1f, 2.4f),
                new RATIXMapKit.Opening(-8f, 1.8f, 1.1f, 2.4f));
            WallWithOpenings(house, "GroundS", true, -halfD, -halfW, halfW, 0f, GroundH, t, MPlaster,
                new RATIXMapKit.Opening(0f, 2.2f, 0f, 2.9f),
                new RATIXMapKit.Opening(7f, 1.8f, 1.1f, 2.4f),
                new RATIXMapKit.Opening(-7f, 1.8f, 1.1f, 2.4f));
            WallWithOpenings(house, "GroundE", false, halfW, -halfD, halfD, 0f, GroundH, t, MPlaster,
                new RATIXMapKit.Opening(0f, 2.4f, 0f, 2.9f),
                new RATIXMapKit.Opening(5.5f, 1.8f, 1.1f, 2.4f),
                new RATIXMapKit.Opening(-5.5f, 1.8f, 1.1f, 2.4f));
            WallWithOpenings(house, "GroundW", false, -halfW, -halfD, halfD, 0f, GroundH, t, MPlaster,
                new RATIXMapKit.Opening(0f, 4f, 0f, 2.9f));

            // Tabique interior.
            WallWithOpenings(house, "Divider", true, -2f, -halfW, halfW, 0f, GroundH, 0.4f, MPlaster,
                new RATIXMapKit.Opening(-7f, 2f, 0f, 2.6f),
                new RATIXMapKit.Opening(7f, 2f, 0f, 2.6f));

            Box(house, "Slab", new Vector3(0f, GroundH + 0.15f, 0f), new Vector3(HouseW, 0.3f, HouseD), MStoneDark);

            // Planta alta.
            WallWithOpenings(house, "UpperN", true, halfD, -halfW, halfW, SlabTop, HouseRoof - SlabTop, t, MPlaster,
                new RATIXMapKit.Opening(6f, 1.8f, 0.7f, 1.9f),
                new RATIXMapKit.Opening(-6f, 1.8f, 0.7f, 1.9f));
            WallWithOpenings(house, "UpperS", true, -halfD, -halfW, halfW, SlabTop, HouseRoof - SlabTop, t, MPlaster,
                new RATIXMapKit.Opening(0f, 1.8f, 0.7f, 1.9f));
            WallWithOpenings(house, "UpperE", false, halfW, -halfD, halfD, SlabTop, HouseRoof - SlabTop, t, MPlaster,
                new RATIXMapKit.Opening(0f, 1.8f, 0.7f, 1.9f));
            WallWithOpenings(house, "UpperW", false, -halfW, -halfD, halfD, SlabTop, HouseRoof - SlabTop, t, MPlaster,
                new RATIXMapKit.Opening(0f, 3f, 0.7f, 1.9f));

            // Tejado plano transitable.
            Box(house, "Roof", new Vector3(0f, HouseRoof - 0.1f, 0f), new Vector3(HouseW + 0.6f, 0.3f, HouseD + 0.6f), MTerracotta);
            float py = HouseRoof + 0.45f;
            WallAlongX(house, "ParapetN", halfD - 0.2f, -halfW + 0.25f, halfW - 0.25f, py, 0.9f, 0.35f, MPlaster);
            WallAlongX(house, "ParapetS", -halfD + 0.2f, -halfW + 0.25f, halfW - 0.25f, py, 0.9f, 0.35f, MPlaster,
                new Vector2(-2.2f, 2.2f));
            WallAlongZ(house, "ParapetW", -halfW + 0.2f, -halfD + 0.2f, halfD - 0.2f, py, 0.9f, 0.35f, MPlaster);
            WallAlongZ(house, "ParapetE", halfW - 0.2f, -halfD + 0.2f, halfD - 0.2f, py, 0.9f, 0.35f, MPlaster);

            // Escalera exterior al tejado (detrás, hacia el sur).
            Stairs(house, "RoofStairs", new Vector3(0f, 0f, -halfD - 8.2f), new Vector3(0f, HouseRoof, -halfD - 0.4f),
                2.6f, 20, MStone);

            // Porche (arcada) en la fachada norte.
            for (int i = 0; i < 6; i++)
            {
                float x = -12.5f + i * 5f;
                Cylinder(house, "Column" + i, new Vector3(x, GroundH * 0.5f, halfD + 1.6f), 0.28f, GroundH, MPlaster);
            }

            Box(house, "PorchRoof", new Vector3(0f, GroundH + 0.02f, halfD + 1.1f), new Vector3(HouseW, 0.3f, 4.4f), MTerracotta);
            Box(house, "PorchBeam", new Vector3(0f, GroundH - 0.1f, halfD + 3.3f), new Vector3(HouseW, 0.3f, 0.3f), MWoodDark, false);

            // Muebles y cobertura interior.
            Box(house, "Table", new Vector3(6f, 0.75f, 0f), new Vector3(2.4f, 0.12f, 1.2f), MWood);
            Box(house, "TableLegA", new Vector3(5f, 0.37f, 0f), new Vector3(0.16f, 0.75f, 1f), MWoodDark, false);
            Box(house, "TableLegB", new Vector3(7f, 0.37f, 0f), new Vector3(0.16f, 0.75f, 1f), MWoodDark, false);
            Instance(house, PCrate, new Vector3(-10f, 0f, 5f), 18f, "Crate");
            Instance(house, PBarrel, new Vector3(-11f, 0f, -5.5f), 0f, "Barrel");
            Instance(house, PHayBale, new Vector3(8f, 0f, -5.5f), 90f, "HayBale");
        }

        // ------------------------------------------------------------------
        // Granero, silo y torre de agua
        // ------------------------------------------------------------------

        private static void BuildBarn(Transform environment)
        {
            var barn = Group(environment, "Barn");
            barn.localPosition = new Vector3(BarnX, 0f, BarnZ);

            float halfW = BarnW * 0.5f;
            float halfD = BarnD * 0.5f;

            WallWithOpenings(barn, "WallN", true, halfD, -halfW, halfW, 0f, BarnWall, 0.4f, MWoodDark,
                new RATIXMapKit.Opening(0f, 6f, 0f, 3.7f));
            WallWithOpenings(barn, "WallS", true, -halfD, -halfW, halfW, 0f, BarnWall, 0.4f, MWoodDark,
                new RATIXMapKit.Opening(6f, 2f, 1.2f, 2.6f));
            WallWithOpenings(barn, "WallW", false, -halfW, -halfD, halfD, 0f, BarnWall, 0.4f, MWoodDark,
                new RATIXMapKit.Opening(3f, 2.4f, 0f, 3f));
            WallWithOpenings(barn, "WallE", false, halfW, -halfD, halfD, 0f, BarnWall, 0.4f, MWoodDark,
                new RATIXMapKit.Opening(-3f, 2f, 1.2f, 2.6f));

            // Cubierta a dos aguas.
            Box(barn, "RoofW", new Vector3(0f, (BarnWall + BarnRidge) * 0.5f, -halfD * 0.5f),
                new Vector3(BarnW + 1f, 0.25f, 8.9f), MTerracotta, true,
                Quaternion.Euler(-22.4f, 0f, 0f));
            Box(barn, "RoofE", new Vector3(0f, (BarnWall + BarnRidge) * 0.5f, halfD * 0.5f),
                new Vector3(BarnW + 1f, 0.25f, 8.9f), MTerracotta, true,
                Quaternion.Euler(22.4f, 0f, 0f));
            Box(barn, "Ridge", new Vector3(0f, BarnRidge + 0.1f, 0f), new Vector3(BarnW + 1.2f, 0.2f, 0.4f), MWoodDark, false);

            // Hastiales en los extremos del eje X (donde bajan las aguas).
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * halfW;
                Box(barn, "GableA" + side, new Vector3(x, BarnWall + 0.45f, 0f), new Vector3(0.3f, 0.9f, BarnD), MWoodDark);
                Box(barn, "GableB" + side, new Vector3(x, BarnWall + 1.35f, 0f), new Vector3(0.3f, 0.9f, BarnD * 0.68f), MWoodDark);
                Box(barn, "GableC" + side, new Vector3(x, BarnWall + 2.25f, 0f), new Vector3(0.3f, 0.9f, BarnD * 0.36f), MWoodDark);
            }

            // Pajar elevado, con hueco en la barandilla para la escalera.
            Box(barn, "Loft", new Vector3(-5f, 3.65f, -4.5f), new Vector3(12f, 0.3f, 7f), MWood);
            Rail(barn, "LoftRailA", new Vector3(-7.1f, 4.25f, -1.1f), new Vector3(7.8f, 1.1f, 0.12f), MWood, MWoodDark);
            Rail(barn, "LoftRailB", new Vector3(0.1f, 4.25f, -1.1f), new Vector3(1.8f, 1.1f, 0.12f), MWood, MWoodDark);
            Stairs(barn, "LoftStairs", new Vector3(-2f, 0f, 5f), new Vector3(-2f, 3.8f, -1.1f), 2.2f, 14, MWood);

            Instance(barn, PHayBale, new Vector3(-7f, 3.8f, -5.4f), 0f, "HayBale");
            Instance(barn, PHayBale, new Vector3(-9f, 3.8f, -3.4f), 90f, "HayBale");
            Instance(barn, PHayBale, new Vector3(5f, 0f, 3f), 0f, "HayBale");
            Instance(barn, PHayBale, new Vector3(6.6f, 0f, 3.2f), 0f, "HayBale");
            Instance(barn, PCrate, new Vector3(8f, 0f, 5.4f), 24f, "Crate");
            Instance(barn, PBarrel, new Vector3(-8f, 0f, 4.4f), 0f, "Barrel");
        }

        private static void BuildSilo(Transform environment)
        {
            var silo = Group(environment, "Silo");

            Cylinder(silo, "Body", new Vector3(42f, 6.5f, -20f), 3.4f, 13f, MStone);
            Cylinder(silo, "BandTop", new Vector3(42f, 12.4f, -20f), 3.5f, 0.3f, MMetalDark, false);
            Cylinder(silo, "BandMid", new Vector3(42f, 7f, -20f), 3.5f, 0.25f, MMetalDark, false);
            Sphere(silo, "Dome", new Vector3(42f, 13.4f, -20f), new Vector3(7.2f, 2.6f, 7.2f), MMetalDark);

            // Escala exterior (decorativa).
            for (int i = 0; i < 10; i++)
            {
                Box(silo, "Rung" + i, new Vector3(38.6f, 1.2f + i * 1.1f, -20f), new Vector3(0.5f, 0.06f, 0.3f), MMetalDark, false);
            }
        }

        private static void BuildWaterTower(Transform environment)
        {
            var tower = Group(environment, "WaterTower");
            var center = new Vector3(42f, 0f, 18f);

            foreach (float x in new[] { -2.2f, 2.2f })
            {
                foreach (float z in new[] { -2.2f, 2.2f })
                {
                    Box(tower, "Leg" + x + z, center + new Vector3(x, 4.5f, z), new Vector3(0.35f, 9f, 0.35f), MWoodDark);
                }
            }

            Box(tower, "BraceA", center + new Vector3(0f, 5.5f, 0f), new Vector3(4.6f, 0.2f, 0.2f), MWoodDark, false);
            Box(tower, "BraceB", center + new Vector3(0f, 5.5f, 0f), new Vector3(0.2f, 0.2f, 4.6f), MWoodDark, false);
            Box(tower, "Deck", center + new Vector3(0f, 9.2f, 0f), new Vector3(5.4f, 0.3f, 5.4f), MWood);
            Cylinder(tower, "Tank", center + new Vector3(0f, 11.4f, 0f), 2.6f, 4f, MMetal);
            Cylinder(tower, "TankTop", center + new Vector3(0f, 13.5f, 0f), 2.7f, 0.3f, MMetalDark, false);
        }

        // ------------------------------------------------------------------
        // Campos, vallas y props
        // ------------------------------------------------------------------

        private static void BuildFields(Transform environment)
        {
            var fields = Group(environment, "Fields");

            // Cañaveral: filas tupidas con pasillos.
            for (int row = 0; row < 6; row++)
            {
                float z = 11f + row * 4f;
                for (float x = -45f; x <= -9f; x += 4.5f)
                {
                    Instance(fields, PCane, new Vector3(x, 0f, z), row * 13f + x, "Cane");
                }
            }

            // Vallas.
            for (float x = -44f; x <= -8f; x += 6f)
            {
                Instance(fields, PFence, new Vector3(x, 0f, 8.6f), 0f, "Fence");
            }

            for (float z = 10f; z <= 30f; z += 6f)
            {
                Instance(fields, PFence, new Vector3(-6.2f, 0f, z), 90f, "Fence");
            }

            Instance(fields, PFence, new Vector3(6f, 0f, 32f), 0f, "Fence");
            Instance(fields, PFence, new Vector3(12f, 0f, 32f), 0f, "Fence");
            Instance(fields, PFence, new Vector3(18f, 0f, 32f), 0f, "Fence");

            // Pacas y aperos por los campos.
            Instance(fields, PHayBale, new Vector3(20f, 0f, 24f), 0f, "HayBale");
            Instance(fields, PHayBale, new Vector3(21.6f, 0f, 24.2f), 0f, "HayBale");
            Instance(fields, PHayBale, new Vector3(26f, 0f, 30f), 90f, "HayBale");
            Instance(fields, PHayBale, new Vector3(-2f, 0f, 30f), 0f, "HayBale");
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            // Cerca del canal y los puentes.
            PropPair(props, PBarrel, 6f, 6f, 0f);
            PropPair(props, PCrate, 7.6f, 4.5f, 12f);
            PropPair(props, PBarrel, 24f, -6.5f, 0f);

            // Patio de la hacienda.
            Instance(props, PBarrel, new Vector3(-18f, 0f, -8f), 0f, "Barrel");
            Instance(props, PBarrel, new Vector3(-17f, 0f, -7.2f), 0f, "Barrel");
            Instance(props, PCrate, new Vector3(-33f, 0f, -8.5f), 20f, "Crate");
            Instance(props, PCrate, new Vector3(-31.6f, 0f, -8.9f), -14f, "Crate");

            // Camino del granero.
            Instance(props, PCrate, new Vector3(14f, 0f, -10f), 8f, "Crate");
            Instance(props, PBarrel, new Vector3(33f, 0f, -10f), 0f, "Barrel");
            Instance(props, PBarrel, new Vector3(34f, 0f, -9.2f), 0f, "Barrel");

            // Árboles secos.
            DeadTree(props, new Vector3(-4f, 0f, -12f), 0f);
            DeadTree(props, new Vector3(10f, 0f, 10f), 40f);
            DeadTree(props, new Vector3(36f, 0f, 12f), 110f);
            DeadTree(props, new Vector3(-38f, 0f, 34f), 200f);
        }

        private static void DeadTree(Transform parent, Vector3 position, float yaw)
        {
            var tree = Group(parent, "DeadTree");
            tree.localPosition = position;
            tree.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Cylinder(tree, "Trunk", new Vector3(0f, 2.2f, 0f), 0.28f, 4.4f, MWoodDark);
            Box(tree, "BranchA", new Vector3(0.9f, 3.6f, 0f), new Vector3(2f, 0.16f, 0.16f), MWoodDark, false,
                Quaternion.Euler(0f, 0f, 32f));
            Box(tree, "BranchB", new Vector3(-0.8f, 4.1f, 0.3f), new Vector3(1.7f, 0.14f, 0.14f), MWoodDark, false,
                Quaternion.Euler(0f, 20f, -38f));
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(0f, 0f, -12f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(-14f, 0f, 8f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(14f, 0f, 12f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(36f, 0f, 4f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(-42f, 0f, 4f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(12f, 0f, -34f), 0f, "Target");
        }

        // ------------------------------------------------------------------
        // Luz, juego y cierre
        // ------------------------------------------------------------------

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(30f, -108f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.82f, 0.58f);
            sun.intensity = 3.1f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.82f;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(26f, 72f, 0f);

            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.62f, 0.7f, 0.92f);
            fill.intensity = 0.7f;
            fill.shadows = LightShadows.None;

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox_Alba.mat");
            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.2f;
            RenderSettings.reflectionIntensity = 0.6f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0038f;
            RenderSettings.fogColor = Hex(0xC08A5C);

            PointLight(parent, "LightHouse", new Vector3(HouseX, 2.8f, HouseZ), new Color(1f, 0.8f, 0.6f), 16f, 14f);
            PointLight(parent, "LightBarn", new Vector3(BarnX, 3.2f, BarnZ), new Color(1f, 0.78f, 0.55f), 18f, 16f);
            PointLight(parent, "LightPorch", new Vector3(HouseX, 2.9f, HouseZ + HouseD * 0.5f + 1.4f),
                new Color(1f, 0.75f, 0.5f), 14f, 12f);

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
            bloom.threshold.Override(1.15f);
            bloom.intensity.Override(0.4f);
            bloom.scatter.Override(0.6f);

            var tonemapping = ProfileComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = ProfileComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(0.3f);
            color.contrast.Override(10f);
            color.saturation.Override(8f);

            var vignette = ProfileComponent<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(0.26f);
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
            serialized.FindProperty("mapId").stringValue = "zombie_hacienda_alba";
            serialized.FindProperty("nameKey").stringValue = "value.hacienda_alba";
            serialized.FindProperty("displayName").stringValue = "Hacienda El Alba";
            serialized.FindProperty("mode").enumValueIndex = (int)MapDefinition.GameMode.Zombie;
            serialized.FindProperty("teams").intValue = 2;
            serialized.FindProperty("recommendedPlayers").intValue = 12;
            serialized.FindProperty("roundSeconds").floatValue = 420f;
            serialized.FindProperty("sceneName").stringValue = "ZombieMap_02_Hacienda";
            serialized.FindProperty("arenaSize").vector3Value = new Vector3(HX * 2f, FenceH, HZ * 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var spawns = Group(gameplay, "Spawns");
            var humans = Group(spawns, "Survivors");
            var infected = Group(spawns, "Infected");
            var list = new List<SpawnPoint>();

            // Supervivientes: delante del porche, mirando al canal.
            for (int i = 0; i < 6; i++)
            {
                float x = HouseX - 10f + i * 4f;
                list.Add(CreateSpawn(humans, "SpawnHuman" + (i + 1), new Vector3(x, 0.1f, HouseZ + HouseD * 0.5f + 3.4f), 0f,
                    SpawnPoint.Team.A, i, i == 2, PSpawnMarker, MTeamA));
            }

            // Infectados: borde norte, entre el cañaveral y el granero.
            for (int i = 0; i < 6; i++)
            {
                float x = 8f + i * 4f;
                list.Add(CreateSpawn(infected, "SpawnInfected" + (i + 1), new Vector3(x, 0.1f, 33f), 180f,
                    SpawnPoint.Team.B, i, i == 0, PSpawnMarker, MTeamB));
            }

            var definitionSerialized = new SerializedObject(definition);
            SetArray(definitionSerialized, "spawnPoints", list.ToArray());

            return definition;
        }
    }
}
