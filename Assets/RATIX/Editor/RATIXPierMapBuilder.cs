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
    /// Mapa Shooter **Muelle 7**: terminal portuario de contenedores a orillas del
    /// mar. Pasillos entre pilas de contenedores, dos almacenes como bases, dos
    /// grúas pórtico y una pasarela sobre el muelle. Escena + prefabs editables.
    /// </summary>
    public static class RATIXPierMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/Pier7";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ShooterMap_03_Muelle7.unity";
        private const string ProfilePath = ArtFolder + "/Pier_Profile.asset";

        private const float HX = 52f;
        private const float HZ = 36f;
        private const float QuayH = 1.4f;
        private const float WarehouseH = 7f;

        private static Material MAsphalt;
        private static Material MConcrete;
        private static Material MConcreteDark;
        private static Material MMetal;
        private static Material MMetalDark;
        private static Material MRust;
        private static Material MHazard;
        private static Material MSea;
        private static Material MWarehouse;
        private static Material MContainerRed;
        private static Material MContainerBlue;
        private static Material MContainerGreen;
        private static Material MContainerOrange;
        private static Material MTeamA;
        private static Material MTeamB;
        private static Material MTeamAPad;
        private static Material MTeamBPad;
        private static Material MTargetFace;
        private static Material MTargetCenter;
        private static Material MTargetHit;

        private static GameObject PContainer;
        private static GameObject PCrate;
        private static GameObject PBarrel;
        private static GameObject PSpawnMarker;
        private static GameObject PTarget;

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Shooter (Muelle 7)", false, 60)]
        public static void BuildPierMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildPrefabs();
            RATIXMapKit.BuildPlayerPrefab();

            var mapRoot = new GameObject("ShooterMap_03_Muelle7").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildHarbour(environment);
            BuildWarehouses(environment);
            BuildCranes(environment);
            BuildCatwalk(environment);
            BuildContainers(environment);
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
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Muelle 7 construido"));
            }

            Debug.Log("[RATIX] Mapa Shooter Muelle 7 construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Shooter (Muelle 7)", false, 61)]
        public static void OpenPierMap()
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
            EnsureGridTexture(TextureFolder, "T_Asphalt", new Color32(86, 90, 96, 255), new Color32(60, 64, 70, 255), 64);
            EnsureGridTexture(TextureFolder, "T_Warehouse", new Color32(150, 154, 158, 255), new Color32(110, 114, 120, 255), 128);
        }

        private static void BuildMaterials()
        {
            MAsphalt = Lit(MaterialFolder, "M_Asphalt", Color.white, 0f, 0.2f,
                TextureFolder + "/T_Asphalt.png", new Vector2(20f, 14f));
            MConcrete = Lit(MaterialFolder, "M_Concrete", Hex(0x8E9499), 0f, 0.2f);
            MConcreteDark = Lit(MaterialFolder, "M_ConcreteDark", Hex(0x5C6268), 0f, 0.16f);
            MMetal = Lit(MaterialFolder, "M_Metal", Hex(0x6E767E), 0.6f, 0.4f);
            MMetalDark = Lit(MaterialFolder, "M_MetalDark", Hex(0x33393F), 0.7f, 0.3f);
            MRust = Lit(MaterialFolder, "M_Rust", Hex(0x9A5E3C), 0.2f, 0.28f);
            MHazard = Lit(MaterialFolder, "M_Hazard", Hex(0xD8A74E), 0.15f, 0.42f);
            MSea = Lit(MaterialFolder, "M_Sea", Hex(0x2B5A66), 0.25f, 0.9f);
            MWarehouse = Lit(MaterialFolder, "M_Warehouse", Color.white, 0f, 0.18f,
                TextureFolder + "/T_Warehouse.png", new Vector2(4f, 3f));
            MContainerRed = Lit(MaterialFolder, "M_ContainerRed", Hex(0x9E3F31), 0.3f, 0.35f);
            MContainerBlue = Lit(MaterialFolder, "M_ContainerBlue", Hex(0x2F5C7A), 0.3f, 0.35f);
            MContainerGreen = Lit(MaterialFolder, "M_ContainerGreen", Hex(0x3C6B4A), 0.3f, 0.35f);
            MContainerOrange = Lit(MaterialFolder, "M_ContainerOrange", Hex(0xB36A2E), 0.3f, 0.35f);
            MTeamA = Lit(MaterialFolder, "M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit(MaterialFolder, "M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit(MaterialFolder, "M_TeamA_Pad", Color.Lerp(Hex(0x2A3138), Hex(0xCE5A46), 0.4f), 0f, 0.2f);
            MTeamBPad = Lit(MaterialFolder, "M_TeamB_Pad", Color.Lerp(Hex(0x2A3138), Hex(0x6B9BC9), 0.4f), 0f, 0.2f);
            MTargetFace = Lit(MaterialFolder, "M_TargetFace", Hex(0xDCE3EA), 0f, 0.3f);
            MTargetCenter = Lit(MaterialFolder, "M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit(MaterialFolder, "M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            Skybox(MaterialFolder, "M_Skybox_Pier", Hex(0x8FA6BB), Hex(0x3A4A55), 1.35f, 1.15f, 0.03f);
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildPrefabs()
        {
            PContainer = BuildContainer();
            PCrate = BuildCrate();
            PBarrel = BuildBarrel();
            PSpawnMarker = BuildSpawnMarker();
            PTarget = BuildTarget();
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildContainer()
        {
            var root = new GameObject("Prop_Container");

            Box(root.transform, "Body", new Vector3(0f, 1.3f, 0f), new Vector3(6f, 2.6f, 2.6f), MContainerBlue);

            for (int i = -1; i <= 1; i++)
            {
                float x = i * 1.85f;
                Box(root.transform, "RibN" + i, new Vector3(x, 1.3f, 1.32f), new Vector3(0.14f, 2.3f, 0.06f), MMetalDark, false);
                Box(root.transform, "RibS" + i, new Vector3(x, 1.3f, -1.32f), new Vector3(0.14f, 2.3f, 0.06f), MMetalDark, false);
            }

            Box(root.transform, "TopRail", new Vector3(0f, 2.66f, 0f), new Vector3(6.1f, 0.14f, 2.7f), MMetalDark, false);
            Box(root.transform, "PostW", new Vector3(-2.95f, 1.3f, 0f), new Vector3(0.12f, 2.6f, 2.6f), MMetalDark, false);
            Box(root.transform, "PostE", new Vector3(2.95f, 1.3f, 0f), new Vector3(0.12f, 2.6f, 2.6f), MMetalDark, false);

            return SavePrefab(PrefabFolder, "Prop_Container", root);
        }

        private static GameObject BuildCrate()
        {
            var root = new GameObject("Prop_Crate");
            var t = root.transform;
            const float size = 1.4f;

            Box(t, "Body", new Vector3(0f, size * 0.5f, 0f), new Vector3(size, size, size), MRust);
            Box(t, "Lid", new Vector3(0f, size + 0.02f, 0f), new Vector3(size * 0.92f, 0.06f, size * 0.92f), MMetalDark, false);

            return SavePrefab(PrefabFolder, "Prop_Crate", root);
        }

        private static GameObject BuildBarrel()
        {
            var root = new GameObject("Prop_Barrel");
            var t = root.transform;

            Cylinder(t, "Body", new Vector3(0f, 0.58f, 0f), 0.42f, 1.16f, MMetal);
            Cylinder(t, "BandTop", new Vector3(0f, 0.95f, 0f), 0.44f, 0.08f, MMetalDark, false);
            Cylinder(t, "BandBottom", new Vector3(0f, 0.2f, 0f), 0.44f, 0.08f, MMetalDark, false);

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
        // Puerto
        // ------------------------------------------------------------------

        private static void BuildHarbour(Transform environment)
        {
            var harbour = Group(environment, "Harbour");

            Box(harbour, "Sea", new Vector3(0f, -2.6f, 0f), new Vector3(420f, 1f, 420f), MSea, false);
            Box(harbour, "Yard", new Vector3(0f, -0.5f, 0f), new Vector3(HX * 2f, 1f, HZ * 2f), MAsphalt);

            // Muelle bajo: se ve el mar por encima.
            Box(harbour, "QuayNorth", new Vector3(0f, QuayH * 0.5f, HZ), new Vector3(HX * 2f + 2f, QuayH, 1.2f), MConcrete);
            Box(harbour, "QuaySouth", new Vector3(0f, QuayH * 0.5f, -HZ), new Vector3(HX * 2f + 2f, QuayH, 1.2f), MConcrete);
            Box(harbour, "QuayEast", new Vector3(HX, QuayH * 0.5f, 0f), new Vector3(1.2f, QuayH, HZ * 2f), MConcrete);
            Box(harbour, "QuayWest", new Vector3(-HX, QuayH * 0.5f, 0f), new Vector3(1.2f, QuayH, HZ * 2f), MConcrete);

            // Cierre invisible: la barandilla del muelle no se puede saltar.
            RATIXMapKit.Barrier(harbour, "BoundaryN", new Vector3(0f, 2.5f, HZ - 0.9f), new Vector3(HX * 2f, 5f, 0.4f));
            RATIXMapKit.Barrier(harbour, "BoundaryS", new Vector3(0f, 2.5f, -HZ + 0.9f), new Vector3(HX * 2f, 5f, 0.4f));
            RATIXMapKit.Barrier(harbour, "BoundaryE", new Vector3(HX - 0.9f, 2.5f, 0f), new Vector3(0.4f, 5f, HZ * 2f));
            RATIXMapKit.Barrier(harbour, "BoundaryW", new Vector3(-HX + 0.9f, 2.5f, 0f), new Vector3(0.4f, 5f, HZ * 2f));

            // Defensas de goma y noráis.
            for (float x = -44f; x <= 44f; x += 11f)
            {
                Box(harbour, "FenderN" + x, new Vector3(x, 0.55f, HZ - 0.9f), new Vector3(1.6f, 1.1f, 0.4f), MMetalDark, false);
                Box(harbour, "FenderS" + x, new Vector3(x, 0.55f, -HZ + 0.9f), new Vector3(1.6f, 1.1f, 0.4f), MMetalDark, false);
                Cylinder(harbour, "BollardN" + x, new Vector3(x + 5f, 0.35f, HZ - 2f), 0.22f, 0.7f, MRust, false);
                Cylinder(harbour, "BollardS" + x, new Vector3(x + 5f, 0.35f, -HZ + 2f), 0.22f, 0.7f, MRust, false);
            }

            // Buque atracado al norte (silueta y referencia).
            Box(harbour, "ShipHull", new Vector3(6f, 0.6f, HZ + 10f), new Vector3(62f, 6f, 12f), MContainerRed);
            Box(harbour, "ShipDeck", new Vector3(6f, 3.7f, HZ + 10f), new Vector3(62f, 0.4f, 12f), MConcreteDark, false);
            Box(harbour, "ShipBridge", new Vector3(30f, 6.6f, HZ + 10f), new Vector3(12f, 5.4f, 9f), MWarehouse);
            Box(harbour, "ShipFunnel", new Vector3(34f, 10.4f, HZ + 10f), new Vector3(3f, 3f, 3f), MMetalDark, false);
        }

        private static void BuildWarehouses(Transform environment)
        {
            var warehouses = Group(environment, "Warehouses");
            BuildWarehouse(warehouses, "WarehouseW", -44f);
            BuildWarehouse(warehouses, "WarehouseE", 44f);
        }

        private static void BuildWarehouse(Transform parent, string name, float cx)
        {
            var house = Group(parent, name);
            float side = Mathf.Sign(cx);
            float inner = cx - side * 9f;   // boca abierta hacia el patio

            Box(house, "Back", new Vector3(cx + side * 9f, WarehouseH * 0.5f, 0f), new Vector3(1.2f, WarehouseH, 22f), MWarehouse);
            Box(house, "SideN", new Vector3(cx, WarehouseH * 0.5f, 11f), new Vector3(18f, WarehouseH, 1.2f), MWarehouse);
            Box(house, "SideS", new Vector3(cx, WarehouseH * 0.5f, -11f), new Vector3(18f, WarehouseH, 1.2f), MWarehouse);
            Box(house, "Roof", new Vector3(cx, WarehouseH + 0.3f, 0f), new Vector3(19f, 0.6f, 23f), MConcreteDark);

            // Pórtico de la boca.
            Box(house, "Lintel", new Vector3(inner, WarehouseH - 0.5f, 0f), new Vector3(1.2f, 1f, 22f), MMetalDark);
            Box(house, "DoorPostN", new Vector3(inner, WarehouseH * 0.5f, 10.4f), new Vector3(1.2f, WarehouseH, 1.2f), MMetal);
            Box(house, "DoorPostS", new Vector3(inner, WarehouseH * 0.5f, -10.4f), new Vector3(1.2f, WarehouseH, 1.2f), MMetal);
            Box(house, "Hazard", new Vector3(inner, WarehouseH - 1.1f, 0f), new Vector3(1.3f, 0.25f, 22f), MHazard, false);

            // Interior.
            Box(house, "Pad", new Vector3(cx, 0.05f, 0f), new Vector3(16f, 0.1f, 20f),
                side < 0f ? MTeamAPad : MTeamBPad, false);

            for (int i = 0; i < 3; i++)
            {
                Box(house, "Rack" + i, new Vector3(cx + side * 5f, 1.6f, -7f + i * 7f), new Vector3(1.2f, 3.2f, 4.4f), MMetalDark);
            }

            for (int i = 0; i < 3; i++)
            {
                Instance(house, PCrate, new Vector3(cx - side * 4f, 0f, -8f + i * 6f), 14f + i * 10f, "Crate");
            }

            Instance(house, PBarrel, new Vector3(cx - side * 6.5f, 0f, 8.5f), 0f, "Barrel");
        }

        private static void BuildCranes(Transform environment)
        {
            var cranes = Group(environment, "Cranes");

            foreach (float cx in new[] { -18f, 18f })
            {
                var crane = Group(cranes, "Crane" + cx);

                foreach (float x in new[] { -2.6f, 2.6f })
                {
                    foreach (float z in new[] { -24f, 24f })
                    {
                        Box(crane, "Leg" + x + z, new Vector3(cx + x, 6.5f, z), new Vector3(1.1f, 13f, 1.1f), MMetal);
                        Box(crane, "Foot" + x + z, new Vector3(cx + x, 0.3f, z), new Vector3(2f, 0.6f, 2f), MConcreteDark);
                    }
                }

                Box(crane, "BeamN", new Vector3(cx, 13.2f, 24f), new Vector3(7f, 1.2f, 1.2f), MMetalDark);
                Box(crane, "BeamS", new Vector3(cx, 13.2f, -24f), new Vector3(7f, 1.2f, 1.2f), MMetalDark);
                Box(crane, "GirderA", new Vector3(cx - 2.6f, 13.2f, 0f), new Vector3(1f, 1f, 48f), MMetalDark);
                Box(crane, "GirderB", new Vector3(cx + 2.6f, 13.2f, 0f), new Vector3(1f, 1f, 48f), MMetalDark);
                Box(crane, "Trolley", new Vector3(cx, 12.2f, 4f), new Vector3(3f, 1.6f, 4f), MRust);
                Box(crane, "HazardA", new Vector3(cx - 2.6f, 13.75f, 0f), new Vector3(0.5f, 0.1f, 48f), MHazard, false);
                Box(crane, "HazardB", new Vector3(cx + 2.6f, 13.75f, 0f), new Vector3(0.5f, 0.1f, 48f), MHazard, false);
            }
        }

        private static void BuildCatwalk(Transform environment)
        {
            var catwalk = Group(environment, "Catwalk");
            float top = 4.2f;

            Box(catwalk, "Deck", new Vector3(0f, top - 0.15f, 30f), new Vector3(78f, 0.3f, 3.2f), MMetal);
            WallAlongX(catwalk, "RailOuter", 31.6f, -39f, 39f, top + 0.5f, 1f, 0.12f, MMetalDark, new Vector2(-1.6f, 1.6f));
            WallAlongX(catwalk, "RailInner", 28.4f, -39f, 39f, top + 0.5f, 1f, 0.12f, MMetalDark, new Vector2(-1.6f, 1.6f));

            for (float x = -36f; x <= 36f; x += 12f)
            {
                Box(catwalk, "Support" + x, new Vector3(x, top * 0.5f, 30f), new Vector3(0.4f, top, 0.4f), MMetalDark);
            }

            Stairs(catwalk, "StairsW", new Vector3(-40f, 0f, 26f), new Vector3(-40f, top, 28.4f), 2.6f, 14, MMetalDark);
            Stairs(catwalk, "StairsE", new Vector3(40f, 0f, 26f), new Vector3(40f, top, 28.4f), 2.6f, 14, MMetalDark);

            // Pasarela hacia el buque.
            Box(catwalk, "Gangway", new Vector3(6f, top - 0.15f, 34f), new Vector3(9f, 0.25f, 3f), MMetal);
        }

        private static void BuildContainers(Transform environment)
        {
            var containers = Group(environment, "Containers");

            // Pilas simétricas (cada una se replica en −X).
            Container(containers, 18f, -14f, 0f, 2, MContainerRed);
            Container(containers, 30f, -22f, 90f, 2, MContainerBlue);
            Container(containers, 14f, 12f, 0f, 1, MContainerGreen);
            Container(containers, 30f, 16f, 90f, 2, MContainerOrange);
            Container(containers, 40f, -4f, 0f, 1, MContainerBlue);

            // Pila central de cobertura.
            Instance(containers, PContainer, new Vector3(0f, 0f, -24f), 0f, "ContainerCenter");
            Instance(containers, PContainer, new Vector3(0f, 2.6f, -24f), 0f, "ContainerCenter");
            Instance(containers, PContainer, new Vector3(0f, 0f, 24f), 0f, "ContainerCenter");
            Instance(containers, PContainer, new Vector3(0f, 2.6f, 24f), 0f, "ContainerCenter");

            // Cajas sueltas.
            foreach (var spot in new[]
            {
                new Vector3(8f, 0f, -6f), new Vector3(-8f, 0f, 6f),
                new Vector3(22f, 0f, 2f), new Vector3(-22f, 0f, -2f)
            })
            {
                Instance(containers, PCrate, spot, 20f, "Crate");
            }
        }

        private static void Container(Transform parent, float x, float z, float yaw, int stack, Material material)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int level = 0; level < stack; level++)
                {
                    var instance = Instance(parent, PContainer, new Vector3(side * x, level * 2.62f, z), yaw,
                        "Container" + side + level);
                    if (instance == null) continue;

                    foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (renderer.name == "Body") renderer.sharedMaterial = material;
                    }
                }
            }
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            foreach (var spot in new[]
            {
                new Vector3(12f, 0f, 26f), new Vector3(-12f, 0f, 26f),
                new Vector3(12f, 0f, -26f), new Vector3(-12f, 0f, -26f),
                new Vector3(34f, 0f, 6f), new Vector3(-34f, 0f, -6f)
            })
            {
                Instance(props, PBarrel, spot, 0f, "Barrel");
            }

            // Noráis y cuerdas junto al muelle.
            for (float x = -30f; x <= 30f; x += 15f)
            {
                Cylinder(props, "Post" + x, new Vector3(x, 0.55f, 33f), 0.3f, 1.1f, MRust);
            }
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(0f, 0f, -30f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(0f, 0f, 15f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(24f, 0f, 28f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(-24f, 0f, 28f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(36f, 0f, -12f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(-36f, 0f, -12f), 90f, "Target");
        }

        // ------------------------------------------------------------------
        // Luz, juego y cierre
        // ------------------------------------------------------------------

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(34f, -62f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.94f, 0.86f);
            sun.intensity = 2.1f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.82f;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(22f, 120f, 0f);

            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.62f, 0.72f, 0.86f);
            fill.intensity = 0.7f;
            fill.shadows = LightShadows.None;

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox_Pier.mat");
            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.05f;
            RenderSettings.reflectionIntensity = 0.8f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0028f;
            RenderSettings.fogColor = Hex(0x9FB0BE);

            PointLight(parent, "LightWarehouseW", new Vector3(-42f, 5.4f, 0f), new Color(1f, 0.9f, 0.75f), 26f, 20f);
            PointLight(parent, "LightWarehouseE", new Vector3(42f, 5.4f, 0f), new Color(1f, 0.9f, 0.75f), 26f, 20f);

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
            bloom.intensity.Override(0.45f);
            bloom.scatter.Override(0.6f);

            var tonemapping = ProfileComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = ProfileComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(0.2f);
            color.contrast.Override(10f);
            color.saturation.Override(-2f);

            var vignette = ProfileComponent<Vignette>(profile);
            vignette.active = true;
            vignette.intensity.Override(0.24f);
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
            serialized.FindProperty("mapId").stringValue = "shooter_pier7";
            serialized.FindProperty("nameKey").stringValue = "value.pier7";
            serialized.FindProperty("displayName").stringValue = "Pier 7";
            serialized.FindProperty("mode").enumValueIndex = (int)MapDefinition.GameMode.Shooter;
            serialized.FindProperty("teams").intValue = 2;
            serialized.FindProperty("recommendedPlayers").intValue = 10;
            serialized.FindProperty("roundSeconds").floatValue = 600f;
            serialized.FindProperty("sceneName").stringValue = "ShooterMap_03_Muelle7";
            serialized.FindProperty("arenaSize").vector3Value = new Vector3(HX * 2f, WarehouseH, HZ * 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var spawns = Group(gameplay, "Spawns");
            var teamA = Group(spawns, "TeamA");
            var teamB = Group(spawns, "TeamB");
            var list = new List<SpawnPoint>();

            for (int i = 0; i < 5; i++)
            {
                float z = -8f + i * 4f;
                bool primary = i == 2;
                list.Add(CreateSpawn(teamA, "SpawnA" + (i + 1), new Vector3(-47f, 0.1f, z), 90f,
                    SpawnPoint.Team.A, i, primary, PSpawnMarker, MTeamA));
                list.Add(CreateSpawn(teamB, "SpawnB" + (i + 1), new Vector3(47f, 0.1f, z), -90f,
                    SpawnPoint.Team.B, i, primary, PSpawnMarker, MTeamB));
            }

            var definitionSerialized = new SerializedObject(definition);
            SetArray(definitionSerialized, "spawnPoints", list.ToArray());

            return definition;
        }
    }
}
