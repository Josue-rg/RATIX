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
    /// Mapa Shooter **Metro Línea 9**: estación abandonada bajo tierra. Andenes
    /// elevados, foso de vías con un tren parado, entreplanta superior, vestíbulos
    /// en los extremos y galerías. Escena + prefabs editables.
    /// </summary>
    public static class RATIXMetroMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/Metro9";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ShooterMap_04_Metro9.unity";
        private const string ProfilePath = ArtFolder + "/Metro_Profile.asset";

        private const float HX = 42f;
        private const float HZ = 30f;
        private const float CeilY = 8.5f;
        private const float PlatformY = 1.1f;
        private const float MezzY = 5.2f;
        private const float TrackHalf = 3.5f;
        private const float PlatformEdge = 12f;

        private static Material MFloor;
        private static Material MTile;
        private static Material MTileGreen;
        private static Material MConcrete;
        private static Material MConcreteDark;
        private static Material MMetal;
        private static Material MMetalDark;
        private static Material MTrain;
        private static Material MTrainWindow;
        private static Material MHazard;
        private static Material MLamp;
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

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Shooter (Metro Línea 9)", false, 70)]
        public static void BuildMetroMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildPrefabs();
            RATIXMapKit.BuildPlayerPrefab();

            var mapRoot = new GameObject("ShooterMap_04_Metro9").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildStation(environment);
            BuildPlatforms(environment);
            BuildTrain(environment);
            BuildMezzanine(environment);
            BuildConcourse(environment);
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
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Metro Línea 9 construido"));
            }

            Debug.Log("[RATIX] Mapa Shooter Metro Línea 9 construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Shooter (Metro Línea 9)", false, 71)]
        public static void OpenMetroMap()
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
            EnsureGridTexture(TextureFolder, "T_Platform", new Color32(150, 152, 148, 255), new Color32(112, 114, 110, 255), 128);
            EnsureBrickTexture(TextureFolder, "T_Tiles", new Color32(206, 208, 196, 255), new Color32(150, 152, 142, 255), 64, 64);
        }

        private static void BuildMaterials()
        {
            MFloor = Lit(MaterialFolder, "M_Floor", Color.white, 0f, 0.25f,
                TextureFolder + "/T_Platform.png", new Vector2(18f, 14f));
            MTile = Lit(MaterialFolder, "M_Tile", Color.white, 0f, 0.35f,
                TextureFolder + "/T_Tiles.png", new Vector2(6f, 3f));
            MTileGreen = Lit(MaterialFolder, "M_TileGreen", Hex(0x3C5A50), 0f, 0.3f);
            MConcrete = Lit(MaterialFolder, "M_Concrete", Hex(0x8A8D8C), 0f, 0.2f);
            MConcreteDark = Lit(MaterialFolder, "M_ConcreteDark", Hex(0x55595A), 0f, 0.16f);
            MMetal = Lit(MaterialFolder, "M_Metal", Hex(0x7C8288), 0.65f, 0.45f);
            MMetalDark = Lit(MaterialFolder, "M_MetalDark", Hex(0x33383D), 0.7f, 0.3f);
            MTrain = Lit(MaterialFolder, "M_Train", Hex(0x9AA3AC), 0.6f, 0.55f);
            MTrainWindow = Lit(MaterialFolder, "M_TrainWindow", Hex(0x1B2329), 0.4f, 0.8f);
            MHazard = Lit(MaterialFolder, "M_Hazard", Hex(0xD8A74E), 0.15f, 0.42f);
            MLamp = Unlit(MaterialFolder, "M_Lamp", new Color(0.9f, 0.97f, 1f, 1f));
            MTeamA = Lit(MaterialFolder, "M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit(MaterialFolder, "M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit(MaterialFolder, "M_TeamA_Pad", Color.Lerp(Hex(0x2A3138), Hex(0xCE5A46), 0.4f), 0f, 0.2f);
            MTeamBPad = Lit(MaterialFolder, "M_TeamB_Pad", Color.Lerp(Hex(0x2A3138), Hex(0x6B9BC9), 0.4f), 0f, 0.2f);
            MTargetFace = Lit(MaterialFolder, "M_TargetFace", Hex(0xDCE3EA), 0f, 0.3f);
            MTargetCenter = Lit(MaterialFolder, "M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit(MaterialFolder, "M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            Skybox(MaterialFolder, "M_Skybox_Metro", Hex(0x5D6B72), Hex(0x22282C), 1f, 0.7f, 0.02f);
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

            Box(root.transform, "Body", new Vector3(0f, 0.7f, 0f), new Vector3(1.4f, 1.4f, 1.4f), MConcrete);
            Box(root.transform, "Band", new Vector3(0f, 0.7f, 0f), new Vector3(1.42f, 0.14f, 1.42f), MHazard, false);

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
        // Estación
        // ------------------------------------------------------------------

        private static void BuildStation(Transform environment)
        {
            var station = Group(environment, "Station");

            Box(station, "Bed", new Vector3(0f, -0.5f, 0f), new Vector3(HX * 2f, 1f, HZ * 2f), MFloor);
            Box(station, "Ceiling", new Vector3(0f, CeilY + 0.4f, 0f), new Vector3(HX * 2f, 0.8f, HZ * 2f), MConcreteDark);

            // Muros alicatados.
            Box(station, "WallNorth", new Vector3(0f, (CeilY + 1.1f) * 0.5f, HZ), new Vector3(HX * 2f, CeilY + 1.1f, 0.8f), MTile);
            Box(station, "WallSouth", new Vector3(0f, (CeilY + 1.1f) * 0.5f, -HZ), new Vector3(HX * 2f, CeilY + 1.1f, 0.8f), MTile);
            Box(station, "WallEast", new Vector3(HX, (CeilY + 1.1f) * 0.5f, 0f), new Vector3(0.8f, CeilY + 1.1f, HZ * 2f), MTile);
            Box(station, "WallWest", new Vector3(-HX, (CeilY + 1.1f) * 0.5f, 0f), new Vector3(0.8f, CeilY + 1.1f, HZ * 2f), MTile);

            Box(station, "SkirtNorth", new Vector3(0f, 0.9f, HZ - 0.45f), new Vector3(HX * 2f, 1.8f, 0.1f), MTileGreen, false);
            Box(station, "SkirtSouth", new Vector3(0f, 0.9f, -HZ + 0.45f), new Vector3(HX * 2f, 1.8f, 0.1f), MTileGreen, false);

            // Vigas del techo.
            for (float x = -36f; x <= 36f; x += 8f)
            {
                Box(station, "Beam" + x, new Vector3(x, CeilY - 0.2f, 0f), new Vector3(0.5f, 0.4f, HZ * 2f), MMetalDark, false);
            }

            // Foso de vías.
            Box(station, "TrackBed", new Vector3(0f, 0.02f, 0f), new Vector3(72f, 0.06f, TrackHalf * 2f), MConcreteDark, false);

            foreach (float z in new[] { -0.75f, 0.75f })
            {
                Box(station, "Rail" + z, new Vector3(0f, 0.16f, z), new Vector3(72f, 0.16f, 0.12f), MMetal, false);
            }

            for (float x = -34f; x <= 34f; x += 4f)
            {
                Box(station, "Sleeper" + x, new Vector3(x, 0.07f, 0f), new Vector3(0.3f, 0.1f, 3.6f), MConcreteDark, false);
            }

            // Galerías ciegas en los extremos del foso.
            foreach (float side in new[] { -1f, 1f })
            {
                Box(station, "Tunnel" + side, new Vector3(side * (HX - 0.4f), 1.6f, 0f), new Vector3(0.8f, 3.2f, 7f), MMetalDark);
                Box(station, "TunnelArch" + side, new Vector3(side * (HX - 0.8f), 3.2f, 0f), new Vector3(0.4f, 0.4f, 7.4f), MHazard, false);
            }
        }

        private static void BuildPlatforms(Transform environment)
        {
            var platforms = Group(environment, "Platforms");

            foreach (float sz in new[] { -1f, 1f })
            {
                float z = sz * (TrackHalf + 4.25f);

                Box(platforms, "Platform" + sz, new Vector3(0f, PlatformY * 0.5f, z),
                    new Vector3(72f, PlatformY, PlatformEdge - TrackHalf), MConcrete);

                Box(platforms, "Edge" + sz, new Vector3(0f, PlatformY + 0.01f, sz * (TrackHalf + 0.35f)),
                    new Vector3(72f, 0.04f, 0.7f), MHazard, false);

                // Barandilla de seguridad en el borde del andén (con huecos de acceso).
                WallAlongX(platforms, "EdgeRail" + sz, sz * (TrackHalf + 0.15f), -36f, 36f, PlatformY + 0.55f, 1.1f, 0.08f, MMetalDark,
                    new Vector2(-26.5f, -23.5f), new Vector2(-1.5f, 1.5f), new Vector2(23.5f, 26.5f));
            }
        }

        private static void BuildTrain(Transform environment)
        {
            var train = Group(environment, "Train");

            foreach (float carX in new[] { -18f, 0f, 18f })
            {
                var car = Group(train, "Car" + carX);

                Box(car, "Body", new Vector3(carX, 1.9f, 0f), new Vector3(14f, 3.4f, 3f), MTrain);
                Box(car, "Roof", new Vector3(carX, 3.68f, 0f), new Vector3(14.2f, 0.3f, 3.2f), MMetalDark, false);
                Box(car, "Skirt", new Vector3(carX, 0.35f, 0f), new Vector3(13.4f, 0.5f, 2.6f), MMetalDark, false);

                foreach (float sz in new[] { -1f, 1f })
                {
                    Box(car, "WindowA" + sz, new Vector3(carX - 4.2f, 2.35f, sz * 1.52f), new Vector3(3.6f, 0.9f, 0.06f), MTrainWindow, false);
                    Box(car, "WindowB" + sz, new Vector3(carX + 4.2f, 2.35f, sz * 1.52f), new Vector3(3.6f, 0.9f, 0.06f), MTrainWindow, false);
                    Box(car, "Door" + sz, new Vector3(carX, 1.8f, sz * 1.52f), new Vector3(1.6f, 2.4f, 0.07f), MMetalDark, false);
                }

                Box(car, "Front", new Vector3(carX + 7.05f, 1.9f, 0f), new Vector3(0.1f, 3.2f, 2.8f), MMetalDark, false);
            }

            // Acceso al techo del tren desde el andén (escalera metálica).
            Stairs(train, "TrainStairs", new Vector3(28f, PlatformY, 5.4f), new Vector3(28f, 3.8f, 1.2f), 1.6f, 10, MMetalDark);
        }

        private static void BuildMezzanine(Transform environment)
        {
            var mezz = Group(environment, "Mezzanine");

            foreach (float sz in new[] { -1f, 1f })
            {
                float z = sz * (TrackHalf + 4.25f);

                Box(mezz, "Deck" + sz, new Vector3(0f, MezzY - 0.15f, z),
                    new Vector3(64f, 0.3f, PlatformEdge - TrackHalf), MMetal);

                WallAlongX(mezz, "Rail" + sz, sz * (TrackHalf + 0.25f), -32f, 32f, MezzY + 0.55f, 1.1f, 0.1f, MMetalDark);
                WallAlongX(mezz, "RailWall" + sz, sz * 11.7f, -32f, 32f, MezzY + 0.55f, 1.1f, 0.1f, MMetalDark);

                for (float x = -28f; x <= 28f; x += 14f)
                {
                    Box(mezz, "Support" + sz + x, new Vector3(x, (PlatformY + MezzY) * 0.5f, z),
                        new Vector3(0.4f, MezzY - PlatformY, 0.4f), MMetalDark);
                }
            }
        }

        private static void BuildConcourse(Transform environment)
        {
            var concourses = Group(environment, "Concourse");

            foreach (float side in new[] { -1f, 1f })
            {
                var hall = Group(concourses, side < 0f ? "West" : "East");
                float cx = side * 37f;

                // Solera a la altura del andén.
                Box(hall, "Floor", new Vector3(cx, PlatformY * 0.5f, 0f), new Vector3(10f, PlatformY, HZ * 2f), MConcrete);
                Box(hall, "Pad", new Vector3(cx + side * 2f, PlatformY + 0.02f, 0f), new Vector3(6f, 0.06f, 18f),
                    side < 0f ? MTeamAPad : MTeamBPad, false);

                // Escaleras del vestíbulo al foso (atajo peligroso).
                Stairs(hall, "TrackSteps", new Vector3(cx - side * 4.4f, PlatformY, 0f), new Vector3(cx - side * 6.2f, 0.02f, 0f),
                    3f, 6, MConcreteDark);

                // Tornos.
                for (int i = 0; i < 4; i++)
                {
                    Box(hall, "Gate" + i, new Vector3(cx + side * 1.5f, PlatformY + 0.55f, -6f + i * 4f),
                        new Vector3(0.4f, 1.1f, 1.6f), MMetal);
                    Box(hall, "GateTop" + i, new Vector3(cx + side * 1.5f, PlatformY + 1.15f, -6f + i * 4f),
                        new Vector3(0.5f, 0.12f, 1.7f), MHazard, false);
                }

                // Escaleras a la entreplanta.
                Stairs(hall, "MezzStairs", new Vector3(cx + side * 3f, PlatformY, 6f),
                    new Vector3(cx + side * 3f, MezzY, 10.6f), 2.4f, 12, MMetalDark);

                // Taquillas / kiosco.
                Box(hall, "Kiosk", new Vector3(cx + side * 2f, PlatformY + 1.1f, -10f), new Vector3(3f, 2.2f, 3f), MTileGreen);
                Box(hall, "KioskTop", new Vector3(cx + side * 2f, PlatformY + 2.3f, -10f), new Vector3(3.4f, 0.2f, 3.4f), MMetalDark, false);
            }
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            foreach (var spot in new[]
            {
                new Vector3(-12f, 0f, -8f), new Vector3(12f, 0f, 8f),
                new Vector3(-26f, 0f, 8f), new Vector3(26f, 0f, -8f)
            })
            {
                Instance(props, PCrate, new Vector3(spot.x, PlatformY, spot.z), 12f, "Crate");
            }

            foreach (var spot in new[]
            {
                new Vector3(-8f, 0f, 7.5f), new Vector3(8f, 0f, -7.5f),
                new Vector3(-30f, 0f, -7.5f), new Vector3(30f, 0f, 7.5f)
            })
            {
                Instance(props, PBarrel, new Vector3(spot.x, PlatformY, spot.z), 0f, "Barrel");
            }

            // Bancos.
            foreach (float x in new[] { -20f, -6f, 6f, 20f })
            {
                Box(props, "Bench" + x, new Vector3(x, PlatformY + 0.45f, 10.4f), new Vector3(2.4f, 0.12f, 0.6f), MMetalDark);
                Box(props, "BenchBack" + x, new Vector3(x, PlatformY + 0.8f, 10.7f), new Vector3(2.4f, 0.6f, 0.08f), MMetalDark, false);
            }
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(0f, 0f, -22f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(0f, 0f, 22f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(-30f, PlatformY, 0f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(30f, PlatformY, 0f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(-14f, PlatformY, -9f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(14f, PlatformY, 9f), 180f, "Target");
        }

        // ------------------------------------------------------------------
        // Luz, juego y cierre
        // ------------------------------------------------------------------

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(64f, -40f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.84f, 0.9f, 1f);
            sun.intensity = 0.9f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.6f;

            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox_Metro.mat");
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1.15f;
            RenderSettings.reflectionIntensity = 0.7f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0055f;
            RenderSettings.fogColor = Hex(0x4A565C);

            // Lámparas de techo y bajo la entreplanta.
            for (float x = -30f; x <= 30f; x += 15f)
            {
                Strip(parent, "Lamp" + x, new Vector3(x, CeilY - 0.3f, 0f), 55f, 26f);
                Strip(parent, "LampN" + x, new Vector3(x, MezzY - 0.4f, 8f), 30f, 14f);
                Strip(parent, "LampS" + x, new Vector3(x, MezzY - 0.4f, -8f), 30f, 14f);
            }

            PointLight(parent, "LampW", new Vector3(-38f, 6.2f, 0f), new Color(0.85f, 0.94f, 1f), 40f, 20f);
            PointLight(parent, "LampE", new Vector3(38f, 6.2f, 0f), new Color(0.85f, 0.94f, 1f), 40f, 20f);
            PointLight(parent, "LampTrack", new Vector3(0f, 4.2f, 0f), new Color(0.7f, 0.85f, 1f), 26f, 22f);

            var profile = LoadOrCreateProfile();
            var volumeGo = new GameObject("PostProcessing");
            volumeGo.transform.SetParent(parent, false);
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.profile = profile;
        }

        private static void Strip(Transform parent, string name, Vector3 position, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.86f, 0.94f, 1f);
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;

            Box(parent, name + "_Panel", position + Vector3.up * 0.12f, new Vector3(2.4f, 0.06f, 0.35f), MLamp, false);
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
            bloom.intensity.Override(0.5f);
            bloom.scatter.Override(0.6f);

            var tonemapping = ProfileComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = ProfileComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(0.45f);
            color.contrast.Override(12f);
            color.saturation.Override(-6f);

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
            serialized.FindProperty("mapId").stringValue = "shooter_metro9";
            serialized.FindProperty("nameKey").stringValue = "value.metro9";
            serialized.FindProperty("displayName").stringValue = "Metro Line 9";
            serialized.FindProperty("mode").enumValueIndex = (int)MapDefinition.GameMode.Shooter;
            serialized.FindProperty("teams").intValue = 2;
            serialized.FindProperty("recommendedPlayers").intValue = 10;
            serialized.FindProperty("roundSeconds").floatValue = 600f;
            serialized.FindProperty("sceneName").stringValue = "ShooterMap_04_Metro9";
            serialized.FindProperty("arenaSize").vector3Value = new Vector3(HX * 2f, CeilY, HZ * 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var spawns = Group(gameplay, "Spawns");
            var teamA = Group(spawns, "TeamA");
            var teamB = Group(spawns, "TeamB");
            var list = new List<SpawnPoint>();

            for (int i = 0; i < 5; i++)
            {
                float z = -8f + i * 4f;
                bool primary = i == 2;
                list.Add(CreateSpawn(teamA, "SpawnA" + (i + 1), new Vector3(-38f, PlatformY + 0.1f, z), 90f,
                    SpawnPoint.Team.A, i, primary, PSpawnMarker, MTeamA));
                list.Add(CreateSpawn(teamB, "SpawnB" + (i + 1), new Vector3(38f, PlatformY + 0.1f, z), -90f,
                    SpawnPoint.Team.B, i, primary, PSpawnMarker, MTeamB));
            }

            var definitionSerialized = new SerializedObject(definition);
            SetArray(definitionSerialized, "spawnPoints", list.ToArray());

            return definition;
        }
    }
}
