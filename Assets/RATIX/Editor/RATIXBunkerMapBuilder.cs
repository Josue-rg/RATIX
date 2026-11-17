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
    /// Mapa de zombis **Búnker K-9**: instalación subterránea de tres alturas.
    /// Zonas muy distintas para pasar la ronda (entrada, sala del reactor con
    /// anillo elevado, dormitorios, enfermería, almacén, sala de control con
    /// grada, armería, sala de servidores y garaje con muelle). Todo escena +
    /// prefabs + materiales editables.
    /// </summary>
    public static class RATIXBunkerMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/BunkerK9";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ZombieMap_01_Bunker.unity";
        private const string ProfilePath = ArtFolder + "/Bunker_Profile.asset";

        private const float HX = 36f;
        private const float HZ = 28f;
        private const float WallH = 10f;
        private const float CeilY = 9f;
        private const float CatwalkTop = 4.6f;

        private static Material MFloor;
        private static Material MConcrete;
        private static Material MConcreteDark;
        private static Material MMetal;
        private static Material MMetalDark;
        private static Material MRust;
        private static Material MHazard;
        private static Material MEmergency;
        private static Material MLampWhite;
        private static Material MLampRed;
        private static Material MTeamA;
        private static Material MTeamB;
        private static Material MTeamAPad;
        private static Material MTeamBPad;
        private static Material MTargetFace;
        private static Material MTargetCenter;
        private static Material MTargetHit;

        private static GameObject PCrate;
        private static GameObject PBarrel;
        private static GameObject PSpawnMarker;
        private static GameObject PTarget;

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Zombi (Búnker K-9)", false, 40)]
        public static void BuildBunkerMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildPrefabs();
            RATIXMapKit.BuildPlayerPrefab();

            var mapRoot = new GameObject("ZombieMap_01_Bunker").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildShell(environment);
            BuildHall(environment);
            BuildWestWing(environment);
            BuildEastWing(environment);
            BuildGarage(environment);
            BuildEntrance(environment);
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
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Búnker K-9 construido"));
            }

            Debug.Log("[RATIX] Mapa Zombi Búnker K-9 construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Zombi (Búnker K-9)", false, 41)]
        public static void OpenBunkerMap()
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
            EnsureGridTexture(TextureFolder, "T_BunkerFloor", new Color32(104, 110, 118, 255), new Color32(66, 72, 80, 255), 128);
            EnsureGridTexture(TextureFolder, "T_BunkerWall", new Color32(124, 130, 138, 255), new Color32(88, 94, 102, 255), 256);
        }

        private static void BuildMaterials()
        {
            MFloor = Lit(MaterialFolder, "M_Floor", Color.white, 0f, 0.28f,
                TextureFolder + "/T_BunkerFloor.png", new Vector2(18f, 14f));
            MConcrete = Lit(MaterialFolder, "M_Concrete", Color.white, 0f, 0.2f,
                TextureFolder + "/T_BunkerWall.png", new Vector2(4f, 3f));
            MConcreteDark = Lit(MaterialFolder, "M_ConcreteDark", Hex(0x5F6A74), 0f, 0.15f);
            MMetal = Lit(MaterialFolder, "M_Metal", Hex(0x6A737C), 0.6f, 0.4f);
            MMetalDark = Lit(MaterialFolder, "M_MetalDark", Hex(0x2F363D), 0.7f, 0.3f);
            MRust = Lit(MaterialFolder, "M_Rust", Hex(0x8A5A3C), 0.2f, 0.25f);
            MHazard = Lit(MaterialFolder, "M_Hazard", Hex(0xD8A74E), 0.2f, 0.4f);
            MEmergency = Lit(MaterialFolder, "M_Emergency", Hex(0xB03A2E), 0.1f, 0.4f);
            MLampWhite = Unlit(MaterialFolder, "M_LampWhite", new Color(0.92f, 0.96f, 1f, 1f));
            MLampRed = Unlit(MaterialFolder, "M_LampRed", new Color(1f, 0.25f, 0.18f, 1f));
            MTeamA = Lit(MaterialFolder, "M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit(MaterialFolder, "M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit(MaterialFolder, "M_TeamA_Pad", Color.Lerp(Hex(0x2A3138), Hex(0xCE5A46), 0.4f), 0f, 0.2f);
            MTeamBPad = Lit(MaterialFolder, "M_TeamB_Pad", Color.Lerp(Hex(0x2A3138), Hex(0x6B9BC9), 0.4f), 0f, 0.2f);
            MTargetFace = Lit(MaterialFolder, "M_TargetFace", Hex(0xDCE3EA), 0f, 0.3f);
            MTargetCenter = Lit(MaterialFolder, "M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit(MaterialFolder, "M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            Skybox(MaterialFolder, "M_Skybox_Bunker", Hex(0x6E7C8C), Hex(0x2A2F35), 1.1f, 0.8f, 0.02f);
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildPrefabs()
        {
            PCrate = BuildCrate();
            PBarrel = BuildBarrel();
            PSpawnMarker = BuildSpawnMarker();
            PTarget = BuildTarget();
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildCrate()
        {
            var root = new GameObject("Prop_Crate");
            var t = root.transform;
            const float size = 1.5f;

            Box(t, "Body", new Vector3(0f, size * 0.5f, 0f), new Vector3(size, size, size), MRust);
            Box(t, "Band", new Vector3(0f, size * 0.5f, 0f), new Vector3(size * 1.01f, 0.14f, size * 1.01f), MMetalDark, false);
            Box(t, "Top", new Vector3(0f, size + 0.03f, 0f), new Vector3(size * 0.92f, 0.06f, size * 0.92f), MMetalDark, false);

            return SavePrefab(PrefabFolder, "Prop_Crate", root);
        }

        private static GameObject BuildBarrel()
        {
            var root = new GameObject("Prop_Barrel");
            var t = root.transform;

            Cylinder(t, "Body", new Vector3(0f, 0.6f, 0f), 0.42f, 1.2f, MMetal);
            Cylinder(t, "BandTop", new Vector3(0f, 1.0f, 0f), 0.44f, 0.08f, MMetalDark, false);
            Cylinder(t, "BandBottom", new Vector3(0f, 0.2f, 0f), 0.44f, 0.08f, MMetalDark, false);
            Cylinder(t, "Lid", new Vector3(0f, 1.21f, 0f), 0.4f, 0.04f, MHazard, false);

            return SavePrefab(PrefabFolder, "Prop_Barrel", root);
        }

        private static GameObject BuildSpawnMarker()
        {
            var root = new GameObject("Prop_SpawnMarker");
            var t = root.transform;

            Cylinder(t, "Ring", new Vector3(0f, 0.015f, 0f), 0.88f, 0.03f, MMetalDark, false);
            Cylinder(t, "Disc", new Vector3(0f, 0.035f, 0f), 0.72f, 0.07f, MTeamA, false);

            return SavePrefab(PrefabFolder, "Prop_SpawnMarker", root);
        }

        private static GameObject BuildTarget()
        {
            var root = new GameObject("Prop_Target");
            var t = root.transform;

            Cylinder(t, "Base", new Vector3(0f, 0.06f, 0f), 0.45f, 0.12f, MMetalDark);

            var pivot = Group(t, "Pivot", new Vector3(0f, 0.12f, 0f));
            Box(pivot, "Post", new Vector3(0f, 0.8f, 0f), new Vector3(0.12f, 1.6f, 0.12f), MMetal);
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
        // Estructura
        // ------------------------------------------------------------------

        private static void BuildShell(Transform environment)
        {
            var shell = Group(environment, "Shell");

            Box(shell, "Floor", new Vector3(0f, -0.5f, 0f), new Vector3(HX * 2f, 1f, HZ * 2f), MFloor);

            Box(shell, "WallNorth", new Vector3(0f, WallH * 0.5f, HZ), new Vector3(HX * 2f + 2f, WallH, 1.2f), MConcrete);
            Box(shell, "WallSouth", new Vector3(0f, WallH * 0.5f, -HZ), new Vector3(HX * 2f + 2f, WallH, 1.2f), MConcrete);
            Box(shell, "WallEast", new Vector3(HX, WallH * 0.5f, 0f), new Vector3(1.2f, WallH, HZ * 2f), MConcrete);
            Box(shell, "WallWest", new Vector3(-HX, WallH * 0.5f, 0f), new Vector3(1.2f, WallH, HZ * 2f), MConcrete);

            // Techos: el vestíbulo del reactor queda a cielo abierto.
            Box(shell, "CeilWest", new Vector3(-24.5f, CeilY + 0.3f, 0f), new Vector3(23f, 0.6f, HZ * 2f), MConcreteDark);
            Box(shell, "CeilEast", new Vector3(24.5f, CeilY + 0.3f, 0f), new Vector3(23f, 0.6f, HZ * 2f), MConcreteDark);
            Box(shell, "CeilNorth", new Vector3(0f, CeilY + 0.3f, 20f), new Vector3(26f, 0.6f, 16f), MConcreteDark);
            Box(shell, "CeilSouth", new Vector3(0f, CeilY + 0.3f, -20f), new Vector3(26f, 0.6f, 16f), MConcreteDark);

            // Vigas del techo.
            for (float z = -24f; z <= 24f; z += 8f)
            {
                Box(shell, "BeamW" + z, new Vector3(-24.5f, CeilY - 0.15f, z), new Vector3(23f, 0.3f, 0.4f), MMetalDark, false);
                Box(shell, "BeamE" + z, new Vector3(24.5f, CeilY - 0.15f, z), new Vector3(23f, 0.3f, 0.4f), MMetalDark, false);
            }

            for (float x = -20f; x <= 20f; x += 8f)
            {
                Box(shell, "BeamN" + x, new Vector3(x, CeilY - 0.15f, 20f), new Vector3(0.4f, 0.3f, 16f), MMetalDark, false);
                Box(shell, "BeamS" + x, new Vector3(x, CeilY - 0.15f, -20f), new Vector3(0.4f, 0.3f, 16f), MMetalDark, false);
            }

            // Pilastras del perímetro.
            for (float x = -30f; x <= 30f; x += 10f)
            {
                Box(shell, "PillarN" + x, new Vector3(x, 4.5f, HZ - 0.9f), new Vector3(0.9f, 9f, 0.6f), MConcreteDark);
                Box(shell, "PillarS" + x, new Vector3(x, 4.5f, -HZ + 0.9f), new Vector3(0.9f, 9f, 0.6f), MConcreteDark);
            }
        }

        private static void BuildHall(Transform environment)
        {
            var hall = Group(environment, "ReactorHall");

            // Núcleo del reactor.
            Cylinder(hall, "Core", new Vector3(0f, 4.5f, 0f), 4f, 9f, MMetalDark);
            Cylinder(hall, "RingLow", new Vector3(0f, 2.2f, 0f), 4.3f, 0.3f, MEmergency, false);
            Cylinder(hall, "RingHigh", new Vector3(0f, 6.4f, 0f), 4.3f, 0.3f, MEmergency, false);
            Cylinder(hall, "GlowLow", new Vector3(0f, 2.5f, 0f), 4.34f, 0.14f, MLampRed, false);
            Cylinder(hall, "GlowHigh", new Vector3(0f, 6.7f, 0f), 4.34f, 0.14f, MLampRed, false);
            Cylinder(hall, "Cap", new Vector3(0f, 8.9f, 0f), 4.4f, 0.3f, MMetal, false);

            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f + 45f;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                Box(hall, "Strut" + i, direction * 6f + Vector3.up * 7.4f,
                    new Vector3(0.3f, 0.3f, 4.2f), MMetalDark, false, Quaternion.Euler(0f, angle, 0f));
            }

            // Anillo de paso elevado.
            Box(hall, "CatwalkW", new Vector3(-11.4f, CatwalkTop - 0.15f, 0f), new Vector3(3.2f, 0.3f, 24f), MMetal);
            Box(hall, "CatwalkE", new Vector3(11.4f, CatwalkTop - 0.15f, 0f), new Vector3(3.2f, 0.3f, 24f), MMetal);
            Box(hall, "CatwalkN", new Vector3(0f, CatwalkTop - 0.15f, 10.4f), new Vector3(26f, 0.3f, 3.2f), MMetal);
            Box(hall, "CatwalkS", new Vector3(0f, CatwalkTop - 0.15f, -10.4f), new Vector3(26f, 0.3f, 3.2f), MMetal);

            float railY = CatwalkTop + 0.55f;
            Rail(hall, "RailW", new Vector3(-9.8f, railY, 0f), new Vector3(0.12f, 1.1f, 24f), MMetalDark, MHazard);
            Rail(hall, "RailE", new Vector3(9.8f, railY, 0f), new Vector3(0.12f, 1.1f, 24f), MMetalDark, MHazard);
            Rail(hall, "RailN", new Vector3(0f, railY, 9.8f), new Vector3(26f, 1.1f, 0.12f), MMetalDark, MHazard);
            WallAlongX(hall, "RailS", -9.8f, -13f, 13f, railY, 1.1f, 0.12f, MMetalDark, new Vector2(-1.7f, 1.7f));

            // Soportes del anillo.
            foreach (var support in new[]
            {
                new Vector3(-11.4f, 2.2f, -10f), new Vector3(-11.4f, 2.2f, 10f),
                new Vector3(11.4f, 2.2f, -10f), new Vector3(11.4f, 2.2f, 10f)
            })
            {
                Box(hall, "Support", support, new Vector3(0.3f, 4.4f, 0.3f), MMetalDark);
            }

            // Escalera del vestíbulo al anillo.
            Stairs(hall, "StairsSouth", new Vector3(0f, 0f, -16.2f), new Vector3(0f, CatwalkTop, -10.8f), 3.2f, 19, MMetalDark);

            // Muros del vestíbulo (con huecos a cada zona).
            WallWithOpenings(hall, "WallWest", false, -13f, -HZ, HZ, 0f, CeilY, 0.8f, MConcrete,
                new RATIXMapKit.Opening(-18f, 2.4f, 0f, 3f),
                new RATIXMapKit.Opening(-1f, 2.4f, 0f, 3f),
                new RATIXMapKit.Opening(16f, 2.4f, 0f, 3f));
            WallWithOpenings(hall, "WallEast", false, 13f, -HZ, HZ, 0f, CeilY, 0.8f, MConcrete,
                new RATIXMapKit.Opening(-18f, 2.4f, 0f, 3f),
                new RATIXMapKit.Opening(-1f, 2.4f, 0f, 3f),
                new RATIXMapKit.Opening(16f, 2.4f, 0f, 3f));
            WallWithOpenings(hall, "WallNorth", true, 12f, -13f, 13f, 0f, CeilY, 0.8f, MConcrete,
                new RATIXMapKit.Opening(0f, 4.5f, 0f, 3.4f));
            WallWithOpenings(hall, "WallSouth", true, -12f, -13f, 13f, 0f, CeilY, 0.8f, MConcrete,
                new RATIXMapKit.Opening(0f, 4f, 0f, 5f));

            // Franjas de peligro en el borde del anillo.
            Box(hall, "HazardW", new Vector3(-9.65f, CatwalkTop + 0.02f, 0f), new Vector3(0.3f, 0.04f, 24f), MHazard, false);
            Box(hall, "HazardE", new Vector3(9.65f, CatwalkTop + 0.02f, 0f), new Vector3(0.3f, 0.04f, 24f), MHazard, false);
        }

        private static void BuildWestWing(Transform environment)
        {
            var west = Group(environment, "WestWing");

            WallWithOpenings(west, "Divider1", true, -8f, -HX, -13f, 0f, CeilY, 0.6f, MConcrete,
                new RATIXMapKit.Opening(-24f, 2.2f, 0f, 3f));
            WallWithOpenings(west, "Divider2", true, 6f, -HX, -13f, 0f, CeilY, 0.6f, MConcrete,
                new RATIXMapKit.Opening(-24f, 2.2f, 0f, 3f));

            // Dormitorios (sur).
            for (int i = 0; i < 6; i++)
            {
                float z = -26f + i * 3.2f;
                Bed(west, "Bed" + i, new Vector3(-33.4f, 0f, z), 90f);
            }

            for (int i = 0; i < 4; i++)
            {
                Box(west, "Locker" + i, new Vector3(-29.5f, 1.1f, -26f + i * 1.4f), new Vector3(0.6f, 2.2f, 1.2f), MMetal);
            }

            // Enfermería (centro).
            for (int i = 0; i < 3; i++)
            {
                Bed(west, "Stretcher" + i, new Vector3(-31f + i * 6f, 0f, -2f), 0f);
            }

            Box(west, "Cabinet1", new Vector3(-34.8f, 1f, 2.2f), new Vector3(0.5f, 2f, 1.6f), MMetal);
            Box(west, "Cabinet2", new Vector3(-34.8f, 1f, -6.2f), new Vector3(0.5f, 2f, 1.6f), MMetal);
            Cylinder(west, "Sink", new Vector3(-33.2f, 0.5f, -6.6f), 0.3f, 0.9f, MMetal, false);

            // Almacén (norte).
            for (int i = 0; i < 3; i++)
            {
                Shelf(west, "Shelf" + i, new Vector3(-34.4f, 0f, 10f + i * 5.4f), 0f);
            }

            Instance(west, PCrate, new Vector3(-20f, 0f, 22f), 14f, "Crate");
            Instance(west, PCrate, new Vector3(-18.4f, 0f, 20.4f), -22f, "Crate");
            Instance(west, PBarrel, new Vector3(-16f, 0f, 25f), 0f, "Barrel");
            Instance(west, PBarrel, new Vector3(-15f, 0f, 24f), 0f, "Barrel");
        }

        private static void BuildEastWing(Transform environment)
        {
            var east = Group(environment, "EastWing");

            WallWithOpenings(east, "Divider1", true, -8f, 13f, HX, 0f, CeilY, 0.6f, MConcrete,
                new RATIXMapKit.Opening(24f, 2.2f, 0f, 3f));
            WallWithOpenings(east, "Divider2", true, 6f, 13f, HX, 0f, CeilY, 0.6f, MConcrete,
                new RATIXMapKit.Opening(24f, 2.2f, 0f, 3f));

            // Sala de control (sur): grada elevada mirando al reactor.
            Box(east, "Gantry", new Vector3(31f, 3.05f, -20f), new Vector3(10f, 0.3f, 11f), MMetal);
            Stairs(east, "GantryStairs", new Vector3(21f, 0f, -20f), new Vector3(26.2f, 3.2f, -20f), 2.6f, 12, MMetalDark);
            Rail(east, "GantryRail", new Vector3(26.1f, 3.75f, -20f), new Vector3(0.12f, 1.1f, 11f), MMetalDark, MHazard);

            for (int i = 0; i < 3; i++)
            {
                Box(east, "Console" + i, new Vector3(31f, 3.6f, -24f + i * 4f), new Vector3(4f, 0.8f, 1.4f), MMetalDark);
                Box(east, "ConsoleScreen" + i, new Vector3(31f, 3.95f, -24.8f + i * 4f), new Vector3(3.4f, 0.5f, 0.06f), MLampWhite, false);
            }

            // Armería (centro).
            for (int i = 0; i < 2; i++)
            {
                Box(east, "WeaponRack" + i, new Vector3(34.6f, 1.1f, -4.5f + i * 5f), new Vector3(0.4f, 2.2f, 3.4f), MMetalDark);
                Box(east, "RackGlow" + i, new Vector3(34.36f, 1.4f, -4.5f + i * 5f), new Vector3(0.04f, 1.4f, 3f), MHazard, false);
            }

            Instance(east, PCrate, new Vector3(20f, 0f, 4.6f), 0f, "Crate");
            Instance(east, PCrate, new Vector3(21.6f, 0f, 4.6f), 8f, "Crate");
            Instance(east, PCrate, new Vector3(20.8f, 1.5f, 4.6f), -12f, "Crate");

            // Sala de servidores (norte): pasillo entre racks.
            for (int row = 0; row < 4; row++)
            {
                float x = 17f + row * 4.2f;
                Box(east, "RackA" + row, new Vector3(x, 1.1f, 12f), new Vector3(1.1f, 2.2f, 5f), MMetalDark);
                Box(east, "RackB" + row, new Vector3(x, 1.1f, 23f), new Vector3(1.1f, 2.2f, 5f), MMetalDark);
                Box(east, "RackLed" + row, new Vector3(x - 0.58f, 1.6f, 12f), new Vector3(0.04f, 0.12f, 4.4f), MLampRed, false);
            }

            Instance(east, PBarrel, new Vector3(30f, 0f, 20f), 0f, "Barrel");
            Instance(east, PBarrel, new Vector3(31.2f, 0f, 20.8f), 0f, "Barrel");
        }

        private static void BuildGarage(Transform environment)
        {
            var garage = Group(environment, "Garage");

            // Muelle de carga.
            Box(garage, "Dock", new Vector3(0f, 0.6f, 24.5f), new Vector3(13f, 1.2f, 6f), MConcreteDark);
            Box(garage, "DockEdge", new Vector3(0f, 1.2f, 21.5f), new Vector3(13.2f, 0.08f, 0.5f), MHazard, false);
            Ramp(garage, "DockRamp", new Vector3(0f, 0f, 18f), new Vector3(0f, 1.2f, 21.4f), 6f, MMetal, MHazard, false);

            // Camión (bloqueado, como cobertura).
            Box(garage, "TruckBody", new Vector3(-4f, 1.6f, 24.6f), new Vector3(3.2f, 2.6f, 6.6f), MMetal);
            Box(garage, "TruckCab", new Vector3(-4f, 1.5f, 20.2f), new Vector3(3f, 2f, 2.4f), MRust);
            Box(garage, "TruckWindow", new Vector3(-4f, 1.8f, 19f), new Vector3(2.6f, 0.7f, 0.06f), MMetalDark, false);

            // Puerta de vehículos (cerrada).
            Box(garage, "Shutter", new Vector3(7f, 3.4f, 27.2f), new Vector3(8f, 6.8f, 0.3f), MMetalDark);
            Box(garage, "ShutterFrame", new Vector3(7f, 3.4f, 27.4f), new Vector3(8.4f, 0.2f, 0.1f), MHazard, false);

            Instance(garage, PCrate, new Vector3(6f, 0f, 16f), 12f, "Crate");
            Instance(garage, PCrate, new Vector3(6f, 1.5f, 16f), -18f, "Crate");
            Instance(garage, PCrate, new Vector3(7.8f, 0f, 17.4f), 30f, "Crate");
            Instance(garage, PBarrel, new Vector3(-8f, 0f, 16f), 0f, "Barrel");
            Instance(garage, PBarrel, new Vector3(-8.8f, 0f, 17.2f), 0f, "Barrel");

            // Luz roja de alarma.
            Box(garage, "AlarmBar", new Vector3(0f, 8.4f, 12.4f), new Vector3(8f, 0.25f, 0.25f), MEmergency, false);
        }

        private static void BuildEntrance(Transform environment)
        {
            var entrance = Group(environment, "Entrance");

            // Esclusa.
            WallWithOpenings(entrance, "Airlock", true, -20f, -13f, 13f, 0f, CeilY, 0.6f, MConcrete,
                new RATIXMapKit.Opening(0f, 2.6f, 0f, 3.2f));

            Box(entrance, "DeconLeft", new Vector3(-4.4f, 2.4f, -23f), new Vector3(0.2f, 4.8f, 1.6f), MMetal);
            Box(entrance, "DeconRight", new Vector3(4.4f, 2.4f, -23f), new Vector3(0.2f, 4.8f, 1.6f), MMetal);
            Box(entrance, "DeconTop", new Vector3(0f, 4.85f, -23f), new Vector3(9f, 0.3f, 1.6f), MMetal);

            for (int i = 0; i < 4; i++)
            {
                Box(entrance, "EntranceLocker" + i, new Vector3(-12.2f, 1.1f, -26f + i * 1.4f), new Vector3(0.6f, 2.2f, 1.2f), MMetal);
            }

            Box(entrance, "HazardLineA", new Vector3(0f, 0.02f, -25.6f), new Vector3(10f, 0.04f, 0.4f), MHazard, false);
            Box(entrance, "HazardLineB", new Vector3(0f, 0.02f, -21.4f), new Vector3(10f, 0.04f, 0.4f), MHazard, false);

            Instance(entrance, PBarrel, new Vector3(9f, 0f, -25f), 0f, "Barrel");
            Instance(entrance, PCrate, new Vector3(-9f, 0f, -15f), 20f, "Crate");
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            Instance(props, PCrate, new Vector3(-4f, 0f, 6f), 0f, "Crate");
            Instance(props, PCrate, new Vector3(4.6f, 0f, 6.4f), 22f, "Crate");
            Instance(props, PBarrel, new Vector3(5.4f, 0f, -6.6f), 0f, "Barrel");
            Instance(props, PBarrel, new Vector3(-6.2f, 0f, -6.4f), 0f, "Barrel");
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(-5f, 0f, -25f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(5f, 0f, -25f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(-30f, 0f, 24f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(30f, 0f, 24f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(-24f, 0f, 1f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(24f, 0f, 1f), 270f, "Target");
        }

        // ------------------------------------------------------------------
        // Detalle de mobiliario
        // ------------------------------------------------------------------

        private static void Bed(Transform parent, string name, Vector3 position, float yaw)
        {
            var group = Group(parent, name);
            group.localPosition = position;
            group.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Box(group, "Frame", new Vector3(0f, 0.28f, 0f), new Vector3(2f, 0.32f, 0.9f), MMetalDark);
            Box(group, "Mattress", new Vector3(0f, 0.52f, 0f), new Vector3(1.9f, 0.18f, 0.82f), MConcreteDark, false);
            Box(group, "Pillow", new Vector3(-0.72f, 0.64f, 0f), new Vector3(0.42f, 0.12f, 0.6f), MLampWhite, false);
        }

        private static void Shelf(Transform parent, string name, Vector3 position, float yaw)
        {
            var group = Group(parent, name);
            group.localPosition = position;
            group.localRotation = Quaternion.Euler(0f, yaw, 0f);

            for (int i = 0; i < 3; i++)
            {
                Box(group, "Level" + i, new Vector3(0f, 0.6f + i * 0.75f, 0f), new Vector3(0.8f, 0.08f, 3f), MMetal);
            }

            Box(group, "PostA", new Vector3(0f, 1.1f, -1.4f), new Vector3(0.1f, 2.2f, 0.1f), MMetalDark, false);
            Box(group, "PostB", new Vector3(0f, 1.1f, 1.4f), new Vector3(0.1f, 2.2f, 0.1f), MMetalDark, false);
        }

        // ------------------------------------------------------------------
        // Luz, juego y cierre
        // ------------------------------------------------------------------

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(58f, -30f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.86f, 0.9f, 1f);
            sun.intensity = 1.7f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.8f;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(30f, 150f, 0f);

            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.55f, 0.62f, 0.72f);
            fill.intensity = 0.45f;
            fill.shadows = LightShadows.None;

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox_Bunker.mat");
            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.3f;
            RenderSettings.reflectionIntensity = 0.7f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.006f;
            RenderSettings.fogColor = Hex(0x2D343C);

            // Lámparas de techo + foco en cada zona.
            CeilingLamp(parent, "LampDorm", new Vector3(-24f, 8.75f, -18f), 45f, 22f, false);
            CeilingLamp(parent, "LampInfirmary", new Vector3(-24f, 8.75f, -1f), 48f, 22f, false);
            CeilingLamp(parent, "LampStorage", new Vector3(-24f, 8.75f, 17f), 42f, 20f, false);
            CeilingLamp(parent, "LampControl", new Vector3(24f, 8.75f, -20f), 45f, 20f, false);
            CeilingLamp(parent, "LampArmory", new Vector3(24f, 8.75f, -1f), 42f, 20f, false);
            CeilingLamp(parent, "LampServers", new Vector3(24f, 8.75f, 17f), 38f, 20f, false);
            CeilingLamp(parent, "LampEntrance", new Vector3(0f, 8.75f, -24f), 45f, 20f, false);
            CeilingLamp(parent, "LampEntrance2", new Vector3(0f, 8.75f, -15f), 40f, 18f, false);
            CeilingLamp(parent, "LampGarageA", new Vector3(-7f, 8.75f, 18f), 45f, 20f, false);
            CeilingLamp(parent, "LampGarageB", new Vector3(7f, 8.75f, 24f), 45f, 20f, false);

            // Luces de emergencia (rojas) en el vestíbulo.
            PointLight(parent, "EmergencyHallA", new Vector3(-12f, 7.6f, -12f), new Color(1f, 0.2f, 0.15f), 22f, 16f);
            PointLight(parent, "EmergencyHallB", new Vector3(12f, 7.6f, 12f), new Color(1f, 0.2f, 0.15f), 22f, 16f);
            PointLight(parent, "HallFill", new Vector3(0f, 8f, 0f), new Color(0.8f, 0.88f, 1f), 20f, 30f);

            var profile = LoadOrCreateProfile();
            var volumeGo = new GameObject("PostProcessing");
            volumeGo.transform.SetParent(parent, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.profile = profile;
        }

        private static void CeilingLamp(Transform parent, string name, Vector3 position, float intensity,
            float range, bool red)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = red ? new Color(1f, 0.35f, 0.3f) : new Color(0.85f, 0.92f, 1f);
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;

            Box(parent, name + "_Panel", position + Vector3.up * 0.12f, new Vector3(1.6f, 0.06f, 0.5f),
                red ? MLampRed : MLampWhite, false);
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
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(0.55f);
            bloom.scatter.Override(0.65f);

            var tonemapping = ProfileComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = ProfileComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(0.6f);
            color.contrast.Override(14f);
            color.saturation.Override(-8f);

            var vignette = ProfileComponent<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.5f);

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
            serialized.FindProperty("mapId").stringValue = "zombie_bunker_k9";
            serialized.FindProperty("nameKey").stringValue = "value.bunker_k9";
            serialized.FindProperty("displayName").stringValue = "Bunker K-9";
            serialized.FindProperty("mode").enumValueIndex = (int)MapDefinition.GameMode.Zombie;
            serialized.FindProperty("teams").intValue = 2;
            serialized.FindProperty("recommendedPlayers").intValue = 12;
            serialized.FindProperty("roundSeconds").floatValue = 420f;
            serialized.FindProperty("sceneName").stringValue = "ZombieMap_01_Bunker";
            serialized.FindProperty("arenaSize").vector3Value = new Vector3(HX * 2f, WallH, HZ * 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var spawns = Group(gameplay, "Spawns");
            var humans = Group(spawns, "Survivors");
            var infected = Group(spawns, "Infected");
            var list = new List<SpawnPoint>();

            // Supervivientes: esclusa de entrada (sur).
            for (int i = 0; i < 6; i++)
            {
                float x = -8f + i * 3.2f;
                list.Add(CreateSpawn(humans, "SpawnHuman" + (i + 1), new Vector3(x, 0.1f, -24.5f), 0f,
                    SpawnPoint.Team.A, i, i == 2, PSpawnMarker, MTeamA));
            }

            // Infectados: laterales del garaje (norte), obliga a cruzar el mapa.
            for (int i = 0; i < 6; i++)
            {
                float side = i < 3 ? -1f : 1f;
                float z = 14f + (i % 3) * 4f;
                list.Add(CreateSpawn(infected, "SpawnInfected" + (i + 1), new Vector3(side * 10f, 0.1f, z), 180f,
                    SpawnPoint.Team.B, i, i == 0, PSpawnMarker, MTeamB));
            }

            var definitionSerialized = new SerializedObject(definition);
            SetArray(definitionSerialized, "spawnPoints", list.ToArray());

            return definition;
        }
    }
}
