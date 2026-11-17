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
    /// Mapa de zombis **Carguero Aurora**: buque de carga en alta mar. Bodega
    /// abierta, cubierta estrecha con contenedores, castillo de proa, grúa y
    /// superestructura de tres puentes con el puente de mando. Escena + prefabs.
    /// </summary>
    public static class RATIXShipMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/ShipAurora";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ZombieMap_03_Carguero.unity";
        private const string ProfilePath = ArtFolder + "/Ship_Profile.asset";

        private const float HX = 44f;       // eslora/2
        private const float HZ = 20f;       // manga/2
        private const float SeaY = -4.2f;
        private const float HoldDepth = -2.5f;
        private const float SuperX = 34f;   // centro de la superestructura

        private static Material MDeck;
        private static Material MDeckDark;
        private static Material MHull;
        private static Material MHullDark;
        private static Material MMetal;
        private static Material MMetalDark;
        private static Material MRust;
        private static Material MHazard;
        private static Material MSea;
        private static Material MWindow;
        private static Material MContainerRed;
        private static Material MContainerBlue;
        private static Material MContainerGreen;
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

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Zombi (Carguero Aurora)", false, 80)]
        public static void BuildShipMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildPrefabs();
            RATIXMapKit.BuildPlayerPrefab();

            var mapRoot = new GameObject("ZombieMap_03_Carguero").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildSea(environment);
            BuildHull(environment);
            BuildHold(environment);
            BuildForecastle(environment);
            BuildSuperstructure(environment);
            BuildDeckGear(environment);
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
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Carguero Aurora construido"));
            }

            Debug.Log("[RATIX] Mapa Zombi Carguero Aurora construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Zombi (Carguero Aurora)", false, 81)]
        public static void OpenShipMap()
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
            EnsureGridTexture(TextureFolder, "T_Deck", new Color32(96, 100, 104, 255), new Color32(70, 74, 78, 255), 64);
            EnsureGridTexture(TextureFolder, "T_Hull", new Color32(120, 124, 128, 255), new Color32(92, 96, 100, 255), 256);
        }

        private static void BuildMaterials()
        {
            MDeck = Lit(MaterialFolder, "M_Deck", Color.white, 0f, 0.22f,
                TextureFolder + "/T_Deck.png", new Vector2(18f, 9f));
            MDeckDark = Lit(MaterialFolder, "M_DeckDark", Hex(0x4E565C), 0f, 0.2f);
            MHull = Lit(MaterialFolder, "M_Hull", Color.white, 0.2f, 0.35f,
                TextureFolder + "/T_Hull.png", new Vector2(10f, 2f));
            MHullDark = Lit(MaterialFolder, "M_HullDark", Hex(0x5A646B), 0.3f, 0.3f);
            MMetal = Lit(MaterialFolder, "M_Metal", Hex(0x6E767E), 0.6f, 0.4f);
            MMetalDark = Lit(MaterialFolder, "M_MetalDark", Hex(0x33393F), 0.7f, 0.3f);
            MRust = Lit(MaterialFolder, "M_Rust", Hex(0x9A5E3C), 0.2f, 0.28f);
            MHazard = Lit(MaterialFolder, "M_Hazard", Hex(0xD8A74E), 0.15f, 0.42f);
            MSea = Lit(MaterialFolder, "M_Sea", Hex(0x1F4A55), 0.25f, 0.9f);
            MWindow = Lit(MaterialFolder, "M_Window", Hex(0x1B252B), 0.4f, 0.85f);
            MContainerRed = Lit(MaterialFolder, "M_ContainerRed", Hex(0x9E3F31), 0.3f, 0.35f);
            MContainerBlue = Lit(MaterialFolder, "M_ContainerBlue", Hex(0x2F5C7A), 0.3f, 0.35f);
            MContainerGreen = Lit(MaterialFolder, "M_ContainerGreen", Hex(0x3C6B4A), 0.3f, 0.35f);
            MTeamA = Lit(MaterialFolder, "M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit(MaterialFolder, "M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit(MaterialFolder, "M_TeamA_Pad", Color.Lerp(Hex(0x2A3138), Hex(0xCE5A46), 0.4f), 0f, 0.2f);
            MTeamBPad = Lit(MaterialFolder, "M_TeamB_Pad", Color.Lerp(Hex(0x2A3138), Hex(0x6B9BC9), 0.4f), 0f, 0.2f);
            MTargetFace = Lit(MaterialFolder, "M_TargetFace", Hex(0xDCE3EA), 0f, 0.3f);
            MTargetCenter = Lit(MaterialFolder, "M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit(MaterialFolder, "M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            Skybox(MaterialFolder, "M_Skybox_Ship", Hex(0x7E93A6), Hex(0x2C3C44), 1.4f, 1.1f, 0.03f);
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
            Box(root.transform, "TopRail", new Vector3(0f, 2.66f, 0f), new Vector3(6.1f, 0.14f, 2.7f), MMetalDark, false);

            for (int i = -1; i <= 1; i++)
            {
                Box(root.transform, "RibN" + i, new Vector3(i * 1.85f, 1.3f, 1.32f), new Vector3(0.14f, 2.3f, 0.06f), MMetalDark, false);
                Box(root.transform, "RibS" + i, new Vector3(i * 1.85f, 1.3f, -1.32f), new Vector3(0.14f, 2.3f, 0.06f), MMetalDark, false);
            }

            return SavePrefab(PrefabFolder, "Prop_Container", root);
        }

        private static GameObject BuildCrate()
        {
            var root = new GameObject("Prop_Crate");

            Box(root.transform, "Body", new Vector3(0f, 0.7f, 0f), new Vector3(1.4f, 1.4f, 1.4f), MRust);
            Box(root.transform, "Lid", new Vector3(0f, 1.42f, 0f), new Vector3(1.3f, 0.06f, 1.3f), MMetalDark, false);

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
        // Casco y cubierta
        // ------------------------------------------------------------------

        private static void BuildSea(Transform environment)
        {
            var sea = Group(environment, "Sea");
            Box(sea, "Water", new Vector3(0f, SeaY, 0f), new Vector3(520f, 1f, 520f), MSea, false);
        }

        private static void BuildHull(Transform environment)
        {
            var hull = Group(environment, "Hull");

            // Fondo y costados.
            Box(hull, "Bottom", new Vector3(0f, SeaY + 0.3f, 0f), new Vector3(HX * 2f, 0.6f, HZ * 2f), MHullDark);
            Box(hull, "SideN", new Vector3(0f, -2.1f, HZ - 0.6f), new Vector3(HX * 2f, 4.2f, 1.2f), MHull);
            Box(hull, "SideS", new Vector3(0f, -2.1f, -HZ + 0.6f), new Vector3(HX * 2f, 4.2f, 1.2f), MHull);
            Box(hull, "Stern", new Vector3(-HX + 0.6f, -2.1f, 0f), new Vector3(1.2f, 4.2f, HZ * 2f), MHull);
            Box(hull, "Bow", new Vector3(HX - 0.6f, -2.1f, 0f), new Vector3(1.2f, 4.2f, HZ * 2f), MHull);

            // Proa en punta.
            Box(hull, "BowWedge", new Vector3(HX + 1.6f, -2.1f, 0f), new Vector3(4f, 4.2f, 18f), MHull, true,
                Quaternion.Euler(0f, 45f, 0f));

            // Cubierta en piezas (deja el hueco de la bodega).
            Box(hull, "DeckBow", new Vector3(-26f, -0.15f, 0f), new Vector3(32f, 0.3f, HZ * 2f), MDeck);
            Box(hull, "DeckStern", new Vector3(26f, -0.15f, 0f), new Vector3(32f, 0.3f, HZ * 2f), MDeck);
            Box(hull, "DeckSideN", new Vector3(0f, -0.15f, 13.5f), new Vector3(20f, 0.3f, 13f), MDeck);
            Box(hull, "DeckSideS", new Vector3(0f, -0.15f, -13.5f), new Vector3(20f, 0.3f, 13f), MDeck);

            // Amurada.
            Box(hull, "BulwarkN", new Vector3(0f, 0.6f, HZ - 0.15f), new Vector3(HX * 2f, 1.2f, 0.3f), MHullDark);
            Box(hull, "BulwarkS", new Vector3(0f, 0.6f, -HZ + 0.15f), new Vector3(HX * 2f, 1.2f, 0.3f), MHullDark);
            Box(hull, "BulwarkStern", new Vector3(-HX + 1.2f, 0.6f, 0f), new Vector3(0.3f, 1.2f, HZ * 2f), MHullDark);
            Box(hull, "BulwarkCapN", new Vector3(0f, 1.24f, HZ - 0.15f), new Vector3(HX * 2f, 0.08f, 0.45f), MHazard, false);
            Box(hull, "BulwarkCapS", new Vector3(0f, 1.24f, -HZ + 0.15f), new Vector3(HX * 2f, 0.08f, 0.45f), MHazard, false);

            // Nombre del buque (franja).
            Box(hull, "HullStripe", new Vector3(0f, -1.2f, -HZ + 0.05f), new Vector3(40f, 0.35f, 0.06f), MHazard, false);
        }

        private static void BuildHold(Transform environment)
        {
            var hold = Group(environment, "Hold");
            const float halfX = 10f;
            const float halfZ = 7f;

            Box(hold, "Floor", new Vector3(0f, HoldDepth - 0.15f, 0f), new Vector3(halfX * 2f, 0.3f, halfZ * 2f), MDeckDark);
            Box(hold, "WallN", new Vector3(0f, (HoldDepth) * 0.5f, halfZ), new Vector3(halfX * 2f, -HoldDepth, 0.4f), MHullDark);
            Box(hold, "WallS", new Vector3(0f, (HoldDepth) * 0.5f, -halfZ), new Vector3(halfX * 2f, -HoldDepth, 0.4f), MHullDark);
            Box(hold, "WallW", new Vector3(-halfX, (HoldDepth) * 0.5f, 0f), new Vector3(0.4f, -HoldDepth, halfZ * 2f), MHullDark);
            Box(hold, "WallE", new Vector3(halfX, (HoldDepth) * 0.5f, 0f), new Vector3(0.4f, -HoldDepth, halfZ * 2f), MHullDark);

            // Borde de la escotilla.
            Box(hold, "CoamingN", new Vector3(0f, 0.25f, halfZ), new Vector3(halfX * 2f + 1f, 0.5f, 0.6f), MHazard);
            Box(hold, "CoamingS", new Vector3(0f, 0.25f, -halfZ), new Vector3(halfX * 2f + 1f, 0.5f, 0.6f), MHazard);
            Box(hold, "CoamingW", new Vector3(-halfX, 0.25f, 0f), new Vector3(0.6f, 0.5f, halfZ * 2f), MHazard);
            Box(hold, "CoamingE", new Vector3(halfX, 0.25f, 0f), new Vector3(0.6f, 0.5f, halfZ * 2f), MHazard);

            // Escalera de salida de la bodega.
            Stairs(hold, "HoldLadder", new Vector3(-8f, HoldDepth, 5f), new Vector3(-8f, 0f, 7.6f), 1.6f, 12, MMetalDark);

            Instance(hold, PCrate, new Vector3(4f, HoldDepth, -2f), 18f, "Crate");
            Instance(hold, PCrate, new Vector3(5.4f, HoldDepth, -2.4f), -12f, "Crate");
            Instance(hold, PContainer, new Vector3(0f, HoldDepth, 0f), 90f, "ContainerHold");
        }

        private static void BuildForecastle(Transform environment)
        {
            var fore = Group(environment, "Forecastle");

            Box(fore, "Platform", new Vector3(-34f, 1.1f, 0f), new Vector3(12f, 2.2f, HZ * 2f - 2f), MHullDark);
            Box(fore, "Top", new Vector3(-34f, 2.22f, 0f), new Vector3(12f, 0.06f, HZ * 2f - 1.8f), MDeck, false);
            Stairs(fore, "ForeStairs", new Vector3(-27.4f, 0f, 0f), new Vector3(-28.6f, 2.2f, 0f), 4f, 10, MMetalDark);

            Box(fore, "Hatch", new Vector3(-38f, 2.5f, 0f), new Vector3(4f, 0.6f, 4f), MMetalDark);
            Cylinder(fore, "Windlass", new Vector3(-30f, 2.6f, 6f), 0.5f, 1.2f, MRust, false,
                Quaternion.Euler(90f, 0f, 0f));

            foreach (float z in new[] { -7f, 7f })
            {
                Box(fore, "Bollard" + z, new Vector3(-40f, 2.45f, z), new Vector3(0.4f, 0.5f, 0.4f), MMetalDark);
            }
        }

        private static void BuildSuperstructure(Transform environment)
        {
            var super = Group(environment, "Superstructure");

            // Puente 1: 0 → 3.2.
            WallWithOpenings(super, "Level1N", true, 12f, SuperX - 8f, SuperX + 8f, 0f, 3.2f, 0.4f, MHull,
                new RATIXMapKit.Opening(-2f, 1.6f, 0f, 2.4f),
                new RATIXMapKit.Opening(4f, 1.6f, 0f, 2.4f));
            WallWithOpenings(super, "Level1S", true, -12f, SuperX - 8f, SuperX + 8f, 0f, 3.2f, 0.4f, MHull,
                new RATIXMapKit.Opening(0f, 1.6f, 0f, 2.4f));
            WallWithOpenings(super, "Level1W", false, SuperX - 8f, -12f, 12f, 0f, 3.2f, 0.4f, MHull,
                new RATIXMapKit.Opening(0f, 1.8f, 0f, 2.6f));
            WallWithOpenings(super, "Level1E", false, SuperX + 8f, -12f, 12f, 0f, 3.2f, 0.4f, MHull,
                new RATIXMapKit.Opening(0f, 1.6f, 0f, 2.4f));
            Box(super, "Level1Roof", new Vector3(SuperX, 3.35f, 0f), new Vector3(16.6f, 0.3f, 24.6f), MDeck);

            // Puente 2: 3.2 → 6.2.
            Box(super, "Level2Block", new Vector3(SuperX, 4.7f, 0f), new Vector3(13f, 3f, 20f), MHull);
            Box(super, "Level2WindowsN", new Vector3(SuperX, 5.2f, 10.1f), new Vector3(11f, 0.8f, 0.06f), MWindow, false);
            Box(super, "Level2WindowsS", new Vector3(SuperX, 5.2f, -10.1f), new Vector3(11f, 0.8f, 0.06f), MWindow, false);
            Box(super, "Level2Door", new Vector3(SuperX - 6.5f, 4.4f, 3f), new Vector3(0.06f, 2.2f, 1.4f), MWindow, false);

            Stairs(super, "Level2Stairs", new Vector3(SuperX - 4f, 3.2f, -8f), new Vector3(SuperX - 4f, 6.2f, -3.4f), 2.2f, 12, MMetalDark);

            // Puente de mando: 6.2 → 9.
            Box(super, "Bridge", new Vector3(SuperX, 7.7f, 4f), new Vector3(12f, 3f, 10f), MHullDark);
            Box(super, "BridgeGlassN", new Vector3(SuperX, 8.1f, 9.05f), new Vector3(10.4f, 1.1f, 0.08f), MWindow, false);
            Box(super, "BridgeGlassS", new Vector3(SuperX, 8.1f, -1.05f), new Vector3(10.4f, 1.1f, 0.08f), MWindow, false);
            Box(super, "BridgeGlassW", new Vector3(SuperX - 6.05f, 8.1f, 4f), new Vector3(0.08f, 1.1f, 8.6f), MWindow, false);
            Box(super, "BridgeRoof", new Vector3(SuperX, 9.3f, 4f), new Vector3(12.6f, 0.3f, 10.6f), MDeck);

            // Escalera de la cubierta al puente de mando.
            Stairs(super, "BridgeStairs", new Vector3(SuperX + 6f, 6.2f, -6f), new Vector3(SuperX + 6f, 9.2f, -1.6f), 2f, 10, MMetalDark);

            // Chimenea y mástil.
            Box(super, "Funnel", new Vector3(SuperX, 10.6f, -6f), new Vector3(4f, 3f, 3f), MMetalDark);
            Box(super, "FunnelBand", new Vector3(SuperX, 11.6f, -6f), new Vector3(4.2f, 0.4f, 3.2f), MHazard, false);
            Cylinder(super, "Mast", new Vector3(SuperX - 6f, 11.4f, 4f), 0.12f, 4.4f, MMetal);
        }

        private static void BuildDeckGear(Transform environment)
        {
            var gear = Group(environment, "DeckGear");

            // Grúa de cubierta.
            var crane = Group(gear, "Crane");
            Cylinder(crane, "Post", new Vector3(18f, 3f, -14f), 0.5f, 6f, MMetal);
            Box(crane, "Jib", new Vector3(24f, 5.6f, -14f), new Vector3(14f, 0.5f, 0.6f), MHazard, false,
                Quaternion.Euler(0f, 0f, 8f));
            Box(crane, "Cab", new Vector3(18f, 5.2f, -14f), new Vector3(2f, 1.6f, 2f), MMetalDark);
            Box(crane, "Cable", new Vector3(30f, 3.4f, -14f), new Vector3(0.06f, 4f, 0.06f), MMetalDark, false);

            // Contenedores en cubierta (cobertura).
            InstanceDeck(gear, 22f, 8f, 90f, 1, MContainerRed);
            InstanceDeck(gear, 22f, 8f, 90f, 2, MContainerRed);
            InstanceDeck(gear, 24f, -8f, 90f, 1, MContainerGreen);
            InstanceDeck(gear, -16f, 12f, 0f, 1, MContainerBlue);
            InstanceDeck(gear, -16f, -12f, 0f, 1, MContainerRed);

            // Botes salvavidas.
            foreach (float z in new[] { 14f, -14f })
            {
                Box(gear, "Lifeboat" + z, new Vector3(28f, 4.2f, z), new Vector3(5f, 1.4f, 1.8f), MHazard);
                Box(gear, "LifeboatFrame" + z, new Vector3(28f, 3.4f, z), new Vector3(5.4f, 0.4f, 0.4f), MMetalDark, false);
            }
        }

        private static void InstanceDeck(Transform parent, float x, float z, float yaw, int level, Material material)
        {
            var instance = Instance(parent, PContainer, new Vector3(x, (level - 1) * 2.62f, z), yaw, "Container");
            if (instance == null) return;

            foreach (var renderer in instance.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer.name == "Body") renderer.sharedMaterial = material;
            }
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            foreach (var spot in new[]
            {
                new Vector3(-8f, 0f, 12f), new Vector3(8f, 0f, -12f),
                new Vector3(14f, 0f, 15f), new Vector3(-14f, 0f, -15f)
            })
            {
                Instance(props, PCrate, spot, 16f, "Crate");
            }

            foreach (var spot in new[]
            {
                new Vector3(-4f, 0f, 16f), new Vector3(4f, 0f, -16f),
                new Vector3(-20f, 0f, 10f), new Vector3(12f, 0f, -10f)
            })
            {
                Instance(props, PBarrel, spot, 0f, "Barrel");
            }
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(-30f, 2.2f, 6f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(-30f, 2.2f, -6f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(0f, 0f, -16f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(0f, 0f, 16f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(20f, 0f, 0f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(-12f, 0f, 0f), 90f, "Target");
        }

        // ------------------------------------------------------------------
        // Luz, juego y cierre
        // ------------------------------------------------------------------

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(28f, -48f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.9f, 0.78f);
            sun.intensity = 2.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(20f, 135f, 0f);

            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.6f, 0.72f, 0.88f);
            fill.intensity = 0.7f;
            fill.shadows = LightShadows.None;

            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox_Ship.mat");
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.1f;
            RenderSettings.reflectionIntensity = 0.85f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0035f;
            RenderSettings.fogColor = Hex(0x7C8E9C);

            PointLight(parent, "LightSuper", new Vector3(SuperX, 5f, 0f), new Color(1f, 0.84f, 0.62f), 24f, 18f);
            PointLight(parent, "LightHold", new Vector3(0f, -0.8f, 0f), new Color(1f, 0.8f, 0.55f), 18f, 14f);
            PointLight(parent, "LightFore", new Vector3(-34f, 4f, 0f), new Color(1f, 0.9f, 0.7f), 20f, 16f);

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
            color.postExposure.Override(0.25f);
            color.contrast.Override(10f);
            color.saturation.Override(-4f);

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
            serialized.FindProperty("mapId").stringValue = "zombie_ship_aurora";
            serialized.FindProperty("nameKey").stringValue = "value.ship_aurora";
            serialized.FindProperty("displayName").stringValue = "Cargo Ship Aurora";
            serialized.FindProperty("mode").enumValueIndex = (int)MapDefinition.GameMode.Zombie;
            serialized.FindProperty("teams").intValue = 2;
            serialized.FindProperty("recommendedPlayers").intValue = 10;
            serialized.FindProperty("roundSeconds").floatValue = 420f;
            serialized.FindProperty("sceneName").stringValue = "ZombieMap_03_Carguero";
            serialized.FindProperty("arenaSize").vector3Value = new Vector3(HX * 2f, 12f, HZ * 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var spawns = Group(gameplay, "Spawns");
            var humans = Group(spawns, "Survivors");
            var infected = Group(spawns, "Infected");
            var list = new List<SpawnPoint>();

            for (int i = 0; i < 5; i++)
            {
                float z = -6f + i * 3f;
                list.Add(CreateSpawn(humans, "SpawnHuman" + (i + 1), new Vector3(-32f, 2.3f, z), 90f,
                    SpawnPoint.Team.A, i, i == 2, PSpawnMarker, MTeamA));
            }

            for (int i = 0; i < 5; i++)
            {
                float z = -6f + i * 3f;
                list.Add(CreateSpawn(infected, "SpawnInfected" + (i + 1), new Vector3(38f, 0.1f, z), -90f,
                    SpawnPoint.Team.B, i, i == 2, PSpawnMarker, MTeamB));
            }

            var definitionSerialized = new SerializedObject(definition);
            SetArray(definitionSerialized, "spawnPoints", list.ToArray());

            return definition;
        }
    }
}
