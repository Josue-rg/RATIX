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
    /// Mapa Shooter inspirado en un poblado del desierto: arena cerrada de arena,
    /// kasbah con callejones, cuatro casas de dos alturas transitables, plaza con
    /// pozo y mercadillo, palmeras y dunas. Todo es escena + prefabs + materiales
    /// editables (Reglas 1 y 2); el menú solo automatiza el montaje.
    /// </summary>
    public static class RATIXDesertMapBuilder
    {
        private const string MapFolder = "Assets/RATIX/Maps/Kasbah";
        private const string ArtFolder = MapFolder + "/Art";
        private const string TextureFolder = ArtFolder + "/Textures";
        private const string MaterialFolder = ArtFolder + "/Materials";
        private const string PrefabFolder = MapFolder + "/Prefabs";
        private const string ScenePath = MapFolder + "/ShooterMap_02_Kasbah.unity";
        private const string ProfilePath = ArtFolder + "/Kasbah_Profile.asset";

        // Arena (metros).
        private const float HX = 48f;
        private const float HZ = 36f;
        private const float WallH = 8f;

        // Casas: planta 3,4 m + planta alta 2,4 m + losa = techo a 6,1 m.
        private const float GroundHeight = 3.4f;
        private const float SlabTop = 3.7f;
        private const float UpperHeight = 2.4f;
        private const float RoofTop = SlabTop + UpperHeight;   // 6.1
        private const float HouseWidth = 14f;
        private const float HouseDepth = 12f;

        private static Material MSand;
        private static Material MSandDark;
        private static Material MPaving;
        private static Material MStone;
        private static Material MStoneLight;
        private static Material MStoneDark;
        private static Material MWood;
        private static Material MMetal;
        private static Material MMetalDark;
        private static Material MClothCream;
        private static Material MClothRed;
        private static Material MClothTeal;
        private static Material MTeamA;
        private static Material MTeamB;
        private static Material MTeamAPad;
        private static Material MTeamBPad;
        private static Material MTargetFace;
        private static Material MTargetCenter;
        private static Material MTargetHit;

        private static GameObject PCrate;
        private static GameObject PBarrel;
        private static GameObject PSandbags;
        private static GameObject PPalm;
        private static GameObject PAwningCream;
        private static GameObject PAwningRed;
        private static GameObject PWell;
        private static GameObject PSpawnMarker;
        private static GameObject PTarget;

        [MenuItem("Tools/RATIX/Mapas/Construir mapa Shooter (Kasbah)", false, 30)]
        public static void BuildKasbahMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();

            CreateFolders();
            BuildTextures();
            BuildMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildPrefabs();
            RATIXMapKit.BuildPlayerPrefab();

            var mapRoot = new GameObject("ShooterMap_02_Kasbah").transform;
            var environment = Group(mapRoot, "Environment");
            var lighting = Group(mapRoot, "Lighting");

            BuildArena(environment);
            BuildBases(environment);
            BuildVillage(environment);
            BuildPlaza(environment);
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
                SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Mapa Kasbah construido"));
            }

            Debug.Log("[RATIX] Mapa Shooter Kasbah construido en " + ScenePath);
        }

        [MenuItem("Tools/RATIX/Mapas/Abrir mapa Shooter (Kasbah)", false, 31)]
        public static void OpenKasbahMap()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        // ------------------------------------------------------------------
        // Carpetas, texturas y materiales
        // ------------------------------------------------------------------

        private static void CreateFolders()
        {
            EnsurePath(TextureFolder);
            EnsurePath(MaterialFolder);
            EnsurePath(PrefabFolder);
        }

        private static void BuildTextures()
        {
            EnsureSandTexture(TextureFolder, "T_Sand", new Color32(196, 170, 126, 255), new Color32(176, 150, 108, 255));
            EnsureBrickTexture(TextureFolder, "T_Sandstone", new Color32(184, 156, 112, 255), new Color32(146, 120, 84, 255), 128, 64);
            EnsureGridTexture(TextureFolder, "T_Paving", new Color32(168, 146, 112, 255), new Color32(128, 109, 82, 255), 128, 512);
        }

        private static void BuildMaterials()
        {
            MSand = Lit(MaterialFolder, "M_Sand", Color.white, 0f, 0.12f,
                TextureFolder + "/T_Sand.png", new Vector2(12f, 9f));
            MSandDark = Lit(MaterialFolder, "M_SandDark", Hex(0xB08F62), 0f, 0.15f);
            MPaving = Lit(MaterialFolder, "M_Paving", Color.white, 0f, 0.2f,
                TextureFolder + "/T_Paving.png", new Vector2(9f, 12f));
            MStone = Lit(MaterialFolder, "M_Stone", Color.white, 0f, 0.16f,
                TextureFolder + "/T_Sandstone.png", new Vector2(2f, 1f));
            MStoneLight = Lit(MaterialFolder, "M_StoneLight", Hex(0xD3BC93), 0f, 0.18f);
            MStoneDark = Lit(MaterialFolder, "M_StoneDark", Hex(0x9A7F58), 0f, 0.14f);
            MWood = Lit(MaterialFolder, "M_Wood", Hex(0x7A5535), 0f, 0.18f);
            MMetal = Lit(MaterialFolder, "M_Metal", Hex(0x6F675C), 0.5f, 0.35f);
            MMetalDark = Lit(MaterialFolder, "M_MetalDark", Hex(0x3D3933), 0.6f, 0.3f);
            MClothCream = Lit(MaterialFolder, "M_ClothCream", Hex(0xD9C8A4), 0f, 0.1f);
            MClothRed = Lit(MaterialFolder, "M_ClothRed", Hex(0x9E3F31), 0f, 0.12f);
            MClothTeal = Lit(MaterialFolder, "M_ClothTeal", Hex(0x2F6B6C), 0f, 0.12f);
            MTeamA = Lit(MaterialFolder, "M_TeamA", Hex(0xCE5A46), 0.1f, 0.35f);
            MTeamB = Lit(MaterialFolder, "M_TeamB", Hex(0x6B9BC9), 0.1f, 0.35f);
            MTeamAPad = Lit(MaterialFolder, "M_TeamA_Pad", Color.Lerp(Hex(0x7A6242), Hex(0xCE5A46), 0.32f), 0f, 0.2f);
            MTeamBPad = Lit(MaterialFolder, "M_TeamB_Pad", Color.Lerp(Hex(0x7A6242), Hex(0x6B9BC9), 0.32f), 0f, 0.2f);
            MTargetFace = Lit(MaterialFolder, "M_TargetFace", Hex(0xE3D6BE), 0f, 0.3f);
            MTargetCenter = Lit(MaterialFolder, "M_TargetCenter", Hex(0xCE5A46), 0f, 0.3f);
            MTargetHit = Lit(MaterialFolder, "M_TargetHit", Hex(0xF2C94C), 0.1f, 0.6f);

            Skybox(MaterialFolder, "M_Skybox_Kasbah", Hex(0xB89468), Hex(0x7A6142), 1.7f, 1.25f, 0.045f);
        }

        // ------------------------------------------------------------------
        // Prefabs
        // ------------------------------------------------------------------

        private static void BuildPrefabs()
        {
            PCrate = BuildCrate();
            PBarrel = BuildBarrel();
            PSandbags = BuildSandbags();
            PPalm = BuildPalm();
            PAwningCream = BuildAwning("Prop_Awning_Cream", MClothCream);
            PAwningRed = BuildAwning("Prop_Awning_Red", MClothRed);
            PWell = BuildWell();
            PSpawnMarker = BuildSpawnMarker();
            PTarget = BuildTarget();
            AssetDatabase.SaveAssets();
        }

        private static GameObject BuildCrate()
        {
            var root = new GameObject("Prop_Crate");
            var t = root.transform;
            const float size = 1.4f;

            Box(t, "Body", new Vector3(0f, size * 0.5f, 0f), new Vector3(size, size, size), MWood);
            Box(t, "Lid", new Vector3(0f, size + 0.02f, 0f), new Vector3(size * 0.9f, 0.06f, size * 0.9f), MMetalDark, false);

            foreach (float x in new[] { -1f, 1f })
            {
                foreach (float z in new[] { -1f, 1f })
                {
                    Box(t, "Post" + x + z, new Vector3(x * (size * 0.5f - 0.06f), size * 0.5f, z * (size * 0.5f - 0.06f)),
                        new Vector3(0.12f, size * 0.96f, 0.12f), MMetalDark, false);
                }
            }

            return SavePrefab(PrefabFolder, "Prop_Crate", root);
        }

        private static GameObject BuildBarrel()
        {
            var root = new GameObject("Prop_Barrel");
            var t = root.transform;

            Cylinder(t, "Body", new Vector3(0f, 0.58f, 0f), 0.42f, 1.16f, MMetal);
            Cylinder(t, "BandTop", new Vector3(0f, 0.98f, 0f), 0.44f, 0.08f, MMetalDark, false);
            Cylinder(t, "BandBottom", new Vector3(0f, 0.2f, 0f), 0.44f, 0.08f, MMetalDark, false);
            Cylinder(t, "Lid", new Vector3(0f, 1.17f, 0f), 0.4f, 0.04f, MMetalDark, false);

            return SavePrefab(PrefabFolder, "Prop_Barrel", root);
        }

        private static GameObject BuildSandbags()
        {
            var root = new GameObject("Prop_Sandbags");
            var t = root.transform;

            // Dos hiladas de sacos (cápsulas tumbadas); se solapan y giran un poco.
            for (int row = 0; row < 2; row++)
            {
                int count = row == 0 ? 3 : 2;
                for (int i = 0; i < count; i++)
                {
                    float x = (i - (count - 1) * 0.5f) * 0.74f + (row == 1 ? 0.2f : 0f);

                    var bag = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    bag.name = "Bag" + row + i;
                    bag.transform.SetParent(t, false);
                    bag.transform.localPosition = new Vector3(x, 0.17f + row * 0.29f, 0f);
                    bag.transform.localRotation = Quaternion.Euler(row == 0 ? 2f : -3f, i % 2 == 0 ? 3f : -4f, 90f);
                    bag.transform.localScale = new Vector3(0.3f, 0.36f, 0.52f);

                    Object.DestroyImmediate(bag.GetComponent<Collider>());
                    bag.GetComponent<MeshRenderer>().sharedMaterial = MSandDark;
                }
            }

            // Un solo colisionador para toda la barrera.
            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.33f, 0f);
            collider.size = new Vector3(2.4f, 0.66f, 0.58f);

            return SavePrefab(PrefabFolder, "Prop_Sandbags", root);
        }

        private static GameObject BuildPalm()
        {
            var root = new GameObject("Prop_Palm");
            var t = root.transform;

            var trunk = Cylinder(t, "Trunk", new Vector3(0f, 2.3f, 0f), 0.19f, 4.6f, MWood, false);
            trunk.transform.localRotation = Quaternion.Euler(0f, 0f, 3.5f);

            Cylinder(t, "Crown", new Vector3(0.28f, 4.6f, 0f), 0.9f, 0.7f, MClothTeal, false);

            for (int i = 0; i < 7; i++)
            {
                float yaw = i * (360f / 7f);
                var frond = Box(t, "Frond" + i, new Vector3(0.28f, 4.55f, 0f),
                    new Vector3(0.55f, 0.06f, 2.6f), MClothTeal, false,
                    Quaternion.Euler(-18f, yaw, 0f));
                frond.transform.localPosition += Quaternion.Euler(0f, yaw, 0f) * new Vector3(0f, 0f, 1.25f);
            }

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 2.3f, 0f);
            collider.height = 4.6f;
            collider.radius = 0.24f;

            return SavePrefab(PrefabFolder, "Prop_Palm", root);
        }

        private static GameObject BuildAwning(string fileName, Material cloth)
        {
            var root = new GameObject(fileName);
            var t = root.transform;

            foreach (float x in new[] { -1.8f, 1.8f })
            {
                foreach (float z in new[] { -1.3f, 1.3f })
                {
                    Box(t, "Post" + x + z, new Vector3(x, 1.2f, z), new Vector3(0.1f, 2.4f, 0.1f), MWood);
                }
            }

            var left = Box(t, "ClothL", new Vector3(-0.98f, 2.5f, 0f), new Vector3(2f, 0.06f, 3f), cloth, false);
            left.transform.localRotation = Quaternion.Euler(0f, 0f, 10f);
            var right = Box(t, "ClothR", new Vector3(0.98f, 2.5f, 0f), new Vector3(2f, 0.06f, 3f), cloth, false);
            right.transform.localRotation = Quaternion.Euler(0f, 0f, -10f);

            Box(t, "Valance", new Vector3(0f, 2.22f, 1.52f), new Vector3(4f, 0.34f, 0.04f), cloth, false);

            return SavePrefab(PrefabFolder, fileName, root);
        }

        private static GameObject BuildWell()
        {
            var root = new GameObject("Prop_Well");
            var t = root.transform;

            Cylinder(t, "Ring", new Vector3(0f, 0.48f, 0f), 1.15f, 0.96f, MStone);
            Cylinder(t, "Water", new Vector3(0f, 0.9f, 0f), 0.82f, 0.06f, MMetalDark, false);

            foreach (float x in new[] { -1.05f, 1.05f })
            {
                Box(t, "Post" + x, new Vector3(x, 1.75f, 0f), new Vector3(0.16f, 1.6f, 0.16f), MWood);
            }

            Box(t, "Beam", new Vector3(0f, 2.5f, 0f), new Vector3(2.4f, 0.14f, 0.18f), MWood);
            Box(t, "Roof", new Vector3(0f, 2.72f, 0f), new Vector3(2.9f, 0.12f, 1.5f), MClothCream, false);
            Box(t, "Rope", new Vector3(0f, 2.1f, 0f), new Vector3(0.03f, 0.7f, 0.03f), MClothCream, false);
            Cylinder(t, "Bucket", new Vector3(0f, 1.7f, 0f), 0.18f, 0.24f, MWood, false);

            return SavePrefab(PrefabFolder, "Prop_Well", root);
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
        // Geometría: arena
        // ------------------------------------------------------------------

        private static void BuildArena(Transform environment)
        {
            var arena = Group(environment, "Arena");

            Box(arena, "SandFloor", new Vector3(0f, -0.5f, 0f), new Vector3(HX * 2f, 1f, HZ * 2f), MSand);

            Box(arena, "WallNorth", new Vector3(0f, WallH * 0.5f, HZ), new Vector3(HX * 2f + 2f, WallH, 1.2f), MStone);
            Box(arena, "WallSouth", new Vector3(0f, WallH * 0.5f, -HZ), new Vector3(HX * 2f + 2f, WallH, 1.2f), MStone);
            Box(arena, "WallEast", new Vector3(HX, WallH * 0.5f, 0f), new Vector3(1.2f, WallH, HZ * 2f), MStone);
            Box(arena, "WallWest", new Vector3(-HX, WallH * 0.5f, 0f), new Vector3(1.2f, WallH, HZ * 2f), MStone);

            Box(arena, "CapNorth", new Vector3(0f, WallH + 0.12f, HZ), new Vector3(HX * 2f + 2.6f, 0.24f, 1.7f), MStoneLight, false);
            Box(arena, "CapSouth", new Vector3(0f, WallH + 0.12f, -HZ), new Vector3(HX * 2f + 2.6f, 0.24f, 1.7f), MStoneLight, false);
            Box(arena, "CapEast", new Vector3(HX, WallH + 0.12f, 0f), new Vector3(1.7f, 0.24f, HZ * 2f - 3.4f), MStoneLight, false);
            Box(arena, "CapWest", new Vector3(-HX, WallH + 0.12f, 0f), new Vector3(1.7f, 0.24f, HZ * 2f - 3.4f), MStoneLight, false);

            // Bastiones de esquina.
            for (int sx = -1; sx <= 1; sx += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    BuildBastion(arena, sx, sz);
                }
            }

            // Dunas de arena en los bordes (cobertura blanda).
            foreach (var dune in new[]
            {
                new Vector3(0f, 0f, 32f), new Vector3(0f, 0f, -32f),
                new Vector3(22f, 0f, 30.5f), new Vector3(22f, 0f, -30.5f),
                new Vector3(-22f, 0f, 30.5f), new Vector3(-22f, 0f, -30.5f)
            })
            {
                Dune(arena, "Dune", dune, new Vector3(15f, 2.1f, 7f));
            }
        }

        private static void BuildBastion(Transform parent, float sx, float sz)
        {
            var bastion = Group(parent, "Bastion_" + (sx > 0 ? "E" : "W") + (sz > 0 ? "N" : "S"));
            float x = sx * (HX - 4f);
            float z = sz * (HZ - 4f);

            Box(bastion, "Tower", new Vector3(x, 5f, z), new Vector3(5.5f, 10f, 5.5f), MStone);
            Box(bastion, "Cap", new Vector3(x, 10.15f, z), new Vector3(6.2f, 0.3f, 6.2f), MStoneLight, false);
            Crenellations(bastion, "CrenelX", true, z + sz * 2.9f, x - 2.6f, x + 2.6f, 10.3f, MStone, 1.8f);
            Crenellations(bastion, "CrenelZ", false, x + sx * 2.9f, z - 2.6f, z + 2.6f, 10.3f, MStone, 1.8f);

            Box(bastion, "ArrowSlit", new Vector3(x - sx * 2.76f, 6.4f, z), new Vector3(0.06f, 1.1f, 0.28f), MStoneDark, false);
        }

        private static void Dune(Transform parent, string name, Vector3 position, Vector3 size)
        {
            var dune = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dune.name = name;
            dune.transform.SetParent(parent, false);
            dune.transform.localPosition = position + Vector3.up * (size.y * 0.35f);
            dune.transform.localScale = size;

            var renderer = dune.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = MSand;

            // La esfera achatada necesita MeshCollider: el SphereCollider no admite escala no uniforme.
            Object.DestroyImmediate(dune.GetComponent<Collider>());
            var collider = dune.AddComponent<MeshCollider>();
            collider.sharedMesh = dune.GetComponent<MeshFilter>().sharedMesh;
            collider.convex = true;
        }

        // ------------------------------------------------------------------
        // Geometría: bases
        // ------------------------------------------------------------------

        private static void BuildBases(Transform environment)
        {
            var bases = Group(environment, "Bases");
            BuildBase(bases, "BaseA", -1f);
            BuildBase(bases, "BaseB", 1f);
        }

        private static void BuildBase(Transform parent, string name, float side)
        {
            var b = Group(parent, name);
            float cx = side * 41f;
            float frontX = side * 34f;
            var team = side < 0f ? MTeamA : MTeamB;

            Box(b, "Pad", new Vector3(cx, 0.05f, 0f), new Vector3(13f, 0.1f, 21f),
                side < 0f ? MTeamAPad : MTeamBPad, false);

            Box(b, "WallNorth", new Vector3(cx, 2.25f, 11f), new Vector3(15f, 4.5f, 0.8f), MStone);
            Box(b, "WallSouth", new Vector3(cx, 2.25f, -11f), new Vector3(15f, 4.5f, 0.8f), MStone);

            // Fachada con puerta de 6 m.
            WallWithOpenings(b, "Facade", false, frontX, -11f, 11f, 0f, 4.5f, 0.8f, MStone,
                new RATIXMapKit.Opening(0f, 6f, 0f, 3.5f));

            Box(b, "GatePostN", new Vector3(frontX, 2.6f, 10.4f), new Vector3(1.2f, 5.2f, 1.2f), MStoneLight);
            Box(b, "GatePostS", new Vector3(frontX, 2.6f, -10.4f), new Vector3(1.2f, 5.2f, 1.2f), MStoneLight);

            // Bandera de equipo sobre la puerta.
            Box(b, "Banner", new Vector3(frontX - side * 0.48f, 3.9f, 0f), new Vector3(0.06f, 1.3f, 4.2f), team, false);
            Box(b, "BannerBar", new Vector3(frontX - side * 0.5f, 4.62f, 0f), new Vector3(0.1f, 0.12f, 4.5f), MWood, false);

            // Cobertura dentro de la base.
            Instance(b, PSandbags, new Vector3(cx - side * 1f, 0f, 6.5f), 0f, "Sandbags");
            Instance(b, PSandbags, new Vector3(cx - side * 1f, 0f, -6.5f), 0f, "Sandbags");
            Instance(b, PCrate, new Vector3(cx - side * 3f, 0f, 4.6f), 18f, "Crate");
            Instance(b, PBarrel, new Vector3(cx + side * 2f, 0f, 8.5f), 0f, "Barrel");
            Instance(b, PBarrel, new Vector3(cx + side * 2.6f, 0f, 8f), 0f, "Barrel");
        }

        // ------------------------------------------------------------------
        // Geometría: poblado
        // ------------------------------------------------------------------

        private static void BuildVillage(Transform environment)
        {
            var village = Group(environment, "Village");

            House(village, "HouseNW", new Vector3(-15f, 0f, 17f), 180f);
            House(village, "HouseNE", new Vector3(15f, 0f, 17f), 180f);
            House(village, "HouseSW", new Vector3(-15f, 0f, -17f), 0f);
            House(village, "HouseSE", new Vector3(15f, 0f, -17f), 0f);
        }

        /// <summary>Casa de dos alturas: puerta a la plaza (+Z local) y escalera al fondo (−Z local).</summary>
        private static void House(Transform parent, string name, Vector3 position, float yaw)
        {
            var house = Group(parent, name);
            house.localPosition = position;
            house.localRotation = Quaternion.Euler(0f, yaw, 0f);

            float halfW = HouseWidth * 0.5f;
            float halfD = HouseDepth * 0.5f;
            const float t = 0.6f;

            // Planta baja.
            WallWithOpenings(house, "GroundFront", true, halfD, -halfW, halfW, 0f, GroundHeight, t, MStone,
                new RATIXMapKit.Opening(0f, 2.6f, 0f, 2.8f),
                new RATIXMapKit.Opening(4.6f, 1.5f, 1.2f, 2.4f),
                new RATIXMapKit.Opening(-4.6f, 1.5f, 1.2f, 2.4f));
            WallWithOpenings(house, "GroundBack", true, -halfD, -halfW, halfW, 0f, GroundHeight, t, MStone,
                new RATIXMapKit.Opening(3.4f, 1.5f, 1.2f, 2.4f),
                new RATIXMapKit.Opening(-3.4f, 1.5f, 1.2f, 2.4f));
            WallWithOpenings(house, "GroundLeft", false, -halfW, -halfD, halfD, 0f, GroundHeight, t, MStone,
                new RATIXMapKit.Opening(0f, 1.5f, 1.2f, 2.4f));
            WallWithOpenings(house, "GroundRight", false, halfW, -halfD, halfD, 0f, GroundHeight, t, MStone,
                new RATIXMapKit.Opening(0f, 1.5f, 1.2f, 2.4f));

            // Forjado y planta alta.
            Box(house, "Slab", new Vector3(0f, GroundHeight + 0.15f, 0f), new Vector3(HouseWidth, 0.3f, HouseDepth), MStoneDark);

            float upperY = SlabTop;
            WallWithOpenings(house, "UpperFront", true, halfD, -halfW, halfW, upperY, UpperHeight, t, MStone,
                new RATIXMapKit.Opening(3.2f, 1.6f, 0.6f, 1.6f),
                new RATIXMapKit.Opening(-3.2f, 1.6f, 0.6f, 1.6f));
            WallWithOpenings(house, "UpperBack", true, -halfD, -halfW, halfW, upperY, UpperHeight, t, MStone,
                new RATIXMapKit.Opening(4.2f, 1.4f, 0.6f, 1.6f));
            WallWithOpenings(house, "UpperLeft", false, -halfW, -halfD, halfD, upperY, UpperHeight, t, MStone,
                new RATIXMapKit.Opening(0f, 1.4f, 0.6f, 1.6f));
            WallWithOpenings(house, "UpperRight", false, halfW, -halfD, halfD, upperY, UpperHeight, t, MStone,
                new RATIXMapKit.Opening(0f, 1.4f, 0.6f, 1.6f));

            // Techo transitable + peto (hueco en la esquina de la escalera).
            Box(house, "Roof", new Vector3(0f, RoofTop - 0.1f, 0f), new Vector3(HouseWidth + 0.4f, 0.3f, HouseDepth + 0.4f), MStoneDark);
            float py = RoofTop + 0.45f;
            WallAlongX(house, "ParapetN", halfD - 0.15f, -halfW + 0.2f, halfW - 0.2f, py, 0.9f, 0.3f, MStone);
            WallAlongX(house, "ParapetS", -halfD + 0.15f, -halfW + 0.2f, halfW - 0.2f, py, 0.9f, 0.3f, MStone,
                new Vector2(-6.1f, -3.3f));
            WallAlongZ(house, "ParapetW", -halfW + 0.15f, -halfD + 0.2f, halfD - 0.2f, py, 0.9f, 0.3f, MStone);
            WallAlongZ(house, "ParapetE", halfW - 0.15f, -halfD + 0.2f, halfD - 0.2f, py, 0.9f, 0.3f, MStone);

            // Escalera exterior al techo (al fondo, en la calle).
            Stairs(house, "Stairs", new Vector3(-4.7f, 0f, -halfD - 8.4f), new Vector3(-4.7f, RoofTop, -halfD - 0.6f),
                2.4f, 20, MStone);

            // Interior: una caja y un barril.
            Instance(house, PCrate, new Vector3(-3.6f, 0f, 2.2f), 24f, "CrateInside");
            Instance(house, PBarrel, new Vector3(3.8f, 0f, -2.4f), 0f, "BarrelInside");
        }

        private static void BuildPlaza(Transform environment)
        {
            var plaza = Group(environment, "Plaza");

            // Pavimento elevado 4 cm.
            Box(plaza, "Paving", new Vector3(0f, 0.02f, 0f), new Vector3(18f, 0.08f, 24f), MPaving, false);
            Box(plaza, "PavingStripeN", new Vector3(0f, 0.09f, 9f), new Vector3(18f, 0.02f, 0.35f), MStoneDark, false);
            Box(plaza, "PavingStripeS", new Vector3(0f, 0.09f, -9f), new Vector3(18f, 0.02f, 0.35f), MStoneDark, false);

            Instance(plaza, PWell, Vector3.zero, 0f, "Well");

            foreach (var spot in new[]
            {
                new Vector3(5.6f, 0f, 7.6f), new Vector3(-5.6f, 0f, 7.6f),
                new Vector3(5.6f, 0f, -7.6f), new Vector3(-5.6f, 0f, -7.6f)
            })
            {
                Instance(plaza, PAwningCream, spot, 0f, "Awning");
            }

            Instance(plaza, PAwningRed, new Vector3(7.2f, 0f, 0f), 90f, "AwningRed");
            Instance(plaza, PAwningRed, new Vector3(-7.2f, 0f, 0f), 90f, "AwningRed");

            Instance(plaza, PCrate, new Vector3(6.4f, 0f, 8.6f), 12f, "Crate");
            Instance(plaza, PCrate, new Vector3(-6.4f, 0f, 8.6f), -12f, "Crate");
            Instance(plaza, PCrate, new Vector3(6.4f, 0f, -8.6f), 30f, "Crate");
            Instance(plaza, PCrate, new Vector3(-6.4f, 0f, -8.6f), -30f, "Crate");

            Instance(plaza, PBarrel, new Vector3(2.6f, 0f, 9.8f), 0f, "Barrel");
            Instance(plaza, PBarrel, new Vector3(-2.6f, 0f, 9.8f), 0f, "Barrel");
            Instance(plaza, PBarrel, new Vector3(2.6f, 0f, -9.8f), 0f, "Barrel");
            Instance(plaza, PBarrel, new Vector3(-2.6f, 0f, -9.8f), 0f, "Barrel");
        }

        private static void BuildProps(Transform environment)
        {
            var props = Group(environment, "Props");

            // Palmeras: plazas, calles y bordes.
            foreach (var spot in new[]
            {
                new Vector3(9.6f, 0f, 10.4f), new Vector3(-9.6f, 0f, 10.4f),
                new Vector3(9.6f, 0f, -10.4f), new Vector3(-9.6f, 0f, -10.4f),
                new Vector3(26.5f, 0f, 14f), new Vector3(-26.5f, 0f, 14f),
                new Vector3(26.5f, 0f, -14f), new Vector3(-26.5f, 0f, -14f),
                new Vector3(37f, 0f, 25f), new Vector3(-37f, 0f, 25f),
                new Vector3(37f, 0f, -25f), new Vector3(-37f, 0f, -25f)
            })
            {
                Instance(props, PPalm, spot, spot.x * 7f, "Palm");
            }

            // Barreras de sacos en las bocas de la plaza y en las calles.
            PropPair(props, PSandbags, 8.4f, 10.6f, 0f);
            PropPair(props, PSandbags, 8.4f, -10.6f, 0f);
            PropPair(props, PSandbags, 27f, 5.5f, 90f);
            PropPair(props, PSandbags, 27f, -5.5f, 90f);

            // Cajas y barriles repartidos.
            foreach (var spot in new[]
            {
                new Vector3(11.2f, 0f, 4.6f), new Vector3(11.2f, 0f, -4.6f),
                new Vector3(20.6f, 0f, 8.4f), new Vector3(20.6f, 0f, -8.4f),
                new Vector3(30.6f, 0f, 20.4f), new Vector3(30.6f, 0f, -20.4f),
                new Vector3(6.6f, 0f, 26.2f), new Vector3(6.6f, 0f, -26.2f)
            })
            {
                Instance(props, PCrate, spot, spot.x * 3f, "Crate");
            }

            foreach (var spot in new[]
            {
                new Vector3(12.4f, 0f, 18.6f), new Vector3(-12.4f, 0f, 18.6f),
                new Vector3(12.4f, 0f, -18.6f), new Vector3(-12.4f, 0f, -18.6f),
                new Vector3(24.4f, 0f, 27.4f), new Vector3(-24.4f, 0f, 27.4f),
                new Vector3(24.4f, 0f, -27.4f), new Vector3(-24.4f, 0f, -27.4f),
                new Vector3(32.4f, 0f, 0f), new Vector3(-32.4f, 0f, 0f)
            })
            {
                Instance(props, PBarrel, spot, 0f, "Barrel");
            }

            // Cajas sobre los tejados (cobertura alta).
            foreach (var spot in new[]
            {
                new Vector3(-14f, 0f, 21.6f), new Vector3(14f, 0f, 21.6f),
                new Vector3(-14f, 0f, -21.6f), new Vector3(14f, 0f, -21.6f)
            })
            {
                var crate = Instance(props, PCrate, spot, 20f, "RoofCrate");
                crate.transform.localPosition = new Vector3(spot.x, RoofTop, spot.z);
            }
        }

        private static void BuildTargets(Transform environment)
        {
            var targets = Group(environment, "Targets");

            Instance(targets, PTarget, new Vector3(0f, 0f, -25f), 0f, "Target");
            Instance(targets, PTarget, new Vector3(0f, 0f, 25f), 180f, "Target");
            Instance(targets, PTarget, new Vector3(-40f, 0f, 22f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(40f, 0f, 22f), 270f, "Target");
            Instance(targets, PTarget, new Vector3(-40f, 0f, -22f), 90f, "Target");
            Instance(targets, PTarget, new Vector3(40f, 0f, -22f), 270f, "Target");
        }

        // ------------------------------------------------------------------
        // Luz, juego y cierre
        // ------------------------------------------------------------------

        private static void BuildLighting(Transform parent)
        {
            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(parent, false);
            sunGo.transform.rotation = Quaternion.Euler(42f, -52f, 0f);

            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.92f, 0.78f);
            sun.intensity = 2.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(parent, false);
            fillGo.transform.rotation = Quaternion.Euler(24f, 132f, 0f);

            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.72f, 0.76f, 0.85f);
            fill.intensity = 0.7f;
            fill.shadows = LightShadows.None;

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_Skybox_Kasbah.mat");
            RenderSettings.skybox = skybox;
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 0.95f;
            RenderSettings.reflectionIntensity = 0.6f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0042f;
            RenderSettings.fogColor = Hex(0xC4A87E);

            // Luz interior de cada casa (planta baja).
            foreach (var house in new[] { "HouseNW", "HouseNE", "HouseSW", "HouseSE" })
            {
                var houseTransform = parent.parent.Find("Environment/Village/" + house);
                Vector3 position = houseTransform != null ? houseTransform.position : Vector3.zero;
                PointLight(parent, "Light" + house, position + new Vector3(0f, 2.9f, 0f),
                    new Color(1f, 0.82f, 0.6f), 7f, 9f);
            }

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
            bloom.intensity.Override(0.35f);
            bloom.scatter.Override(0.6f);

            var tonemapping = ProfileComponent<Tonemapping>(profile);
            tonemapping.active = true;
            tonemapping.mode.Override(TonemappingMode.ACES);

            var color = ProfileComponent<ColorAdjustments>(profile);
            color.active = true;
            color.postExposure.Override(0.2f);
            color.contrast.Override(12f);
            color.saturation.Override(4f);

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
            serialized.FindProperty("mapId").stringValue = "shooter_kasbah";
            serialized.FindProperty("nameKey").stringValue = "value.kasbah";
            serialized.FindProperty("displayName").stringValue = "Kasbah";
            serialized.FindProperty("mode").enumValueIndex = (int)MapDefinition.GameMode.Shooter;
            serialized.FindProperty("teams").intValue = 2;
            serialized.FindProperty("recommendedPlayers").intValue = 10;
            serialized.FindProperty("roundSeconds").floatValue = 600f;
            serialized.FindProperty("sceneName").stringValue = "ShooterMap_02_Kasbah";
            serialized.FindProperty("arenaSize").vector3Value = new Vector3(HX * 2f, WallH, HZ * 2f);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var spawns = Group(gameplay, "Spawns");
            var teamA = Group(spawns, "TeamA");
            var teamB = Group(spawns, "TeamB");
            var list = new List<SpawnPoint>();

            for (int i = 0; i < 5; i++)
            {
                float z = -8f + i * 4f;
                bool primary = i == 2;
                list.Add(CreateSpawn(teamA, "SpawnA" + (i + 1), new Vector3(-45f, 0.1f, z), 90f,
                    SpawnPoint.Team.A, i, primary, PSpawnMarker, MTeamA));
                list.Add(CreateSpawn(teamB, "SpawnB" + (i + 1), new Vector3(45f, 0.1f, z), -90f,
                    SpawnPoint.Team.B, i, primary, PSpawnMarker, MTeamB));
            }

            var definitionSerialized = new SerializedObject(definition);
            SetArray(definitionSerialized, "spawnPoints", list.ToArray());

            return definition;
        }
    }
}
