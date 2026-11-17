using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace RATIX
{
    /// <summary>
    /// Instala la selección de mapas en el lobby: miniaturas, nombres, carruseles
    /// de 4 cartas y la pantalla de carga. No toca la lógica de partida.
    /// </summary>
    public static class RATIXMapFlowInstaller
    {
        private const string LobbyScene = "Assets/RATIX/Scenes/Lobby.unity";
        private const string ThumbFolder = "Assets/RATIX/UI/MapThumbs";
        private const int ThumbSize = 384;

        private class MapEntry
        {
            public string Name;
            public string Key;
            public string Scene;
            public Vector3 CameraPosition;
            public Vector3 LookAt;
            public float Fov;
            public Sprite Thumb;
            public string File;
        }

        private static MapEntry[] ZombieMaps() => new[]
        {
            new MapEntry { Name = "Bunker K-9", Key = "value.bunker_k_9", Scene = "ZombieMap_01_Bunker",
                CameraPosition = new Vector3(0f, 2f, -8.5f), LookAt = new Vector3(6f, 4.5f, 4f), Fov = 62f, File = "bunker" },
            new MapEntry { Name = "Hacienda El Alba", Key = "value.hacienda_el_alba", Scene = "ZombieMap_02_Hacienda",
                CameraPosition = new Vector3(-58f, 46f, -58f), LookAt = new Vector3(0f, 0f, 0f), Fov = 42f, File = "hacienda" },
            new MapEntry { Name = "Cargo Ship Aurora", Key = "value.cargo_ship_aurora", Scene = "ZombieMap_03_Carguero",
                CameraPosition = new Vector3(-56f, 30f, -34f), LookAt = new Vector3(0f, 0f, 0f), Fov = 45f, File = "ship" },
            new MapEntry { Name = "Temple of Mist", Key = "value.temple_of_mist", Scene = "ZombieMap_04_Templo",
                CameraPosition = new Vector3(-54f, 30f, -44f), LookAt = new Vector3(0f, 3f, 0f), Fov = 45f, File = "temple" }
        };

        private static MapEntry[] ShooterMaps() => new[]
        {
            new MapEntry { Name = "Foundry", Key = "value.foundry", Scene = "ShooterMap_01",
                CameraPosition = new Vector3(-46f, 40f, -58f), LookAt = new Vector3(0f, 0f, 2f), Fov = 42f, File = "foundry" },
            new MapEntry { Name = "Kasbah", Key = "value.kasbah", Scene = "ShooterMap_02_Kasbah",
                CameraPosition = new Vector3(-50f, 42f, -52f), LookAt = new Vector3(2f, 0f, 2f), Fov = 42f, File = "kasbah" },
            new MapEntry { Name = "Pier 7", Key = "value.pier_7", Scene = "ShooterMap_03_Muelle7",
                CameraPosition = new Vector3(-58f, 44f, -52f), LookAt = new Vector3(0f, 0f, 0f), Fov = 42f, File = "pier" },
            new MapEntry { Name = "Metro Line 9", Key = "value.metro_line_9", Scene = "ShooterMap_04_Metro9",
                CameraPosition = new Vector3(-30f, 3f, 8f), LookAt = new Vector3(30f, 2.6f, 8f), Fov = 62f, File = "metro" }
        };

        [MenuItem("Tools/RATIX/UI/Instalar selección de mapas", false, 60)]
        public static void Install()
        {
            if (EditorSceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveOpenScenes();
            EnsureFolder(ThumbFolder);

            var zombie = ZombieMaps();
            var shooter = ShooterMaps();

            RenderThumbs(zombie);
            RenderThumbs(shooter);

            EditorSceneManager.OpenScene(LobbyScene, OpenSceneMode.Single);

            var canvas = GameObject.Find("LobbyCanvas");
            var loading = EnsureLoadingScreen(canvas.transform);

            SetupPanel("Content/ZombieModePanel/CreateRoomPanel", zombie, loading);
            SetupPanel("Content/ShooterModePanel/CreateRoomPanel", shooter, loading);
            SetupPanel("Content/ZombieHallModelPanel/SettingsPanel", zombie, loading);
            SetupPanel("Content/ShooterHallModelPanel/SettingsPanel", shooter, loading);

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("[RATIX] Selección de mapas instalada (miniaturas + nombres + pantalla de carga).");
        }

        // ------------------------------------------------------------------
        // Miniaturas
        // ------------------------------------------------------------------

        private static void RenderThumbs(MapEntry[] maps)
        {
            foreach (var map in maps)
            {
                string scenePath = FindScenePath(map.Scene);
                if (string.IsNullOrEmpty(scenePath))
                {
                    Debug.LogWarning("[RATIX] No encuentro la escena " + map.Scene);
                    continue;
                }

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                string png = ThumbFolder + "/" + map.File + ".png";
                RenderTo(map.CameraPosition, map.LookAt, map.Fov, png);
                ImportSprite(png);
                map.Thumb = AssetDatabase.LoadAssetAtPath<Sprite>(png);
            }
        }

        private static string FindScenePath(string sceneName)
        {
            foreach (var guid in AssetDatabase.FindAssets(sceneName + " t:Scene"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == sceneName) return path;
            }

            return null;
        }

        private static void RenderTo(Vector3 position, Vector3 lookAt, float fov, string path)
        {
            var go = new GameObject("ThumbCamera");
            go.hideFlags = HideFlags.HideAndDontSave;

            var camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = fov;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 800f;
            camera.allowHDR = true;

            go.transform.position = position;
            go.transform.LookAt(lookAt);

            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

            var target = new RenderTexture(ThumbSize, ThumbSize, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;
            camera.Render();

            RenderTexture.active = target;
            var texture = new Texture2D(ThumbSize, ThumbSize, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, ThumbSize, ThumbSize), 0, 0);
            texture.Apply();
            RenderTexture.active = null;

            File.WriteAllBytes(Path.GetFullPath(path), texture.EncodeToPNG());

            Object.DestroyImmediate(texture);
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(go);
        }

        private static void ImportSprite(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 512;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        // ------------------------------------------------------------------
        // Carruseles
        // ------------------------------------------------------------------

        private static void SetupPanel(string panelPath, MapEntry[] maps, LoadingScreen loading)
        {
            var panel = GameObject.Find("LobbyCanvas/" + panelPath);
            if (panel == null)
            {
                Debug.LogWarning("[RATIX] No encuentro " + panelPath);
                return;
            }

            Carousel3D carousel = null;
            foreach (var candidate in panel.GetComponentsInChildren<Carousel3D>(true))
            {
                if (candidate.transform.Find("Map") != null) carousel = candidate;
            }

            if (carousel == null)
            {
                Debug.LogWarning("[RATIX] " + panelPath + " no tiene carrusel de mapas.");
                return;
            }

            ConfigureCarousel(carousel, maps);

            var launcher = panel.GetComponent<MapLauncher>();
            if (launcher == null) launcher = panel.AddComponent<MapLauncher>();

            var serialized = new SerializedObject(launcher);
            serialized.FindProperty("carousel").objectReferenceValue = carousel;
            serialized.FindProperty("loadingScreen").objectReferenceValue = loading;

            var scenes = serialized.FindProperty("sceneNames");
            scenes.arraySize = maps.Length;
            var keys = serialized.FindProperty("nameKeys");
            keys.arraySize = maps.Length;

            for (int i = 0; i < maps.Length; i++)
            {
                scenes.GetArrayElementAtIndex(i).stringValue = maps[i].Scene;
                keys.GetArrayElementAtIndex(i).stringValue = maps[i].Key;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureCarousel(Carousel3D carousel, MapEntry[] maps)
        {
            var root = carousel.transform as RectTransform;
            root.sizeDelta = new Vector2(380f, 190f);

            var cards = new List<RectTransform>();
            for (int i = 0; i < carousel.transform.childCount; i++)
            {
                var child = carousel.transform.GetChild(i) as RectTransform;
                if (child != null && child.name.StartsWith("Card")) cards.Add(child);
            }

            if (cards.Count == 0)
            {
                Debug.LogWarning("[RATIX] El carrusel no tiene cartas.");
                return;
            }

            while (cards.Count < maps.Length)
            {
                var clone = Object.Instantiate(cards[cards.Count - 1].gameObject, carousel.transform) as GameObject;
                clone.name = "Card" + cards.Count;
                cards.Add(clone.transform as RectTransform);
            }

            var groups = new List<CanvasGroup>();
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                card.sizeDelta = new Vector2(108f, 108f);
                card.anchoredPosition = new Vector2((i - (cards.Count - 1) * 0.5f) * 118f, 0f);

                var photo = card.Find("photo")?.GetComponent<Image>();
                if (photo != null && i < maps.Length)
                {
                    photo.sprite = maps[i].Thumb;
                    photo.color = Color.white;
                    photo.preserveAspect = true;
                }

                var group = card.GetComponent<CanvasGroup>();
                if (group == null) group = card.gameObject.AddComponent<CanvasGroup>();
                groups.Add(group);
            }

            var serialized = new SerializedObject(carousel);
            var cardsProperty = serialized.FindProperty("cards");
            var groupsProperty = serialized.FindProperty("groups");
            var namesProperty = serialized.FindProperty("selectionNames");

            cardsProperty.arraySize = cards.Count;
            groupsProperty.arraySize = groups.Count;
            namesProperty.arraySize = maps.Length;

            for (int i = 0; i < cards.Count; i++)
            {
                cardsProperty.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
                groupsProperty.GetArrayElementAtIndex(i).objectReferenceValue = groups[i];
            }

            for (int i = 0; i < maps.Length; i++)
                namesProperty.GetArrayElementAtIndex(i).stringValue = maps[i].Name;

            serialized.FindProperty("spacing").floatValue = 118f;

            var label = carousel.transform.Find("Name") as RectTransform;
            var labelText = label != null ? label.GetComponent<TMP_Text>() : null;
            if (labelText != null)
            {
                label.sizeDelta = new Vector2(400f, 30f);
                label.anchoredPosition = new Vector2(0f, -66f);
                labelText.fontSize = 18f;
                serialized.FindProperty("selectionLabel").objectReferenceValue = labelText;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        // ------------------------------------------------------------------
        // Pantalla de carga
        // ------------------------------------------------------------------

        private static LoadingScreen EnsureLoadingScreen(Transform canvas)
        {
            var existing = canvas.Find("LoadingScreen");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var rootGo = new GameObject("LoadingScreen", typeof(RectTransform));
            var root = (RectTransform)rootGo.transform;
            root.SetParent(canvas, false);

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.zero;
            root.pivot = Vector2.zero;
            root.sizeDelta = new Vector2(1920f, 1080f);
            root.anchoredPosition = new Vector2(0f, 6640f); // estacionada

            var background = rootGo.AddComponent<Image>();
            background.color = new Color(0.039f, 0.055f, 0.075f, 1f); // #0A0E13
            background.raycastTarget = true;

            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(root, false);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(900f, 360f);

            var logo = MakeText(content, "Logo", "RATIX", 72f, new Color(0.91f, 0.93f, 0.96f), new Vector2(0f, 90f));
            logo.fontStyle = FontStyles.Bold;
            logo.characterSpacing = 12f;

            var loadingLabel = MakeText(content, "LoadingLabel", "LOADING", 16f, new Color(0.58f, 0.64f, 0.71f), new Vector2(0f, 26f));
            loadingLabel.characterSpacing = 10f;
            var localizer = loadingLabel.gameObject.AddComponent<TextLocalizer>();
            var localizerSerialized = new SerializedObject(localizer);
            localizerSerialized.FindProperty("key").stringValue = "loading.title";
            localizerSerialized.FindProperty("target").objectReferenceValue = loadingLabel;
            localizerSerialized.ApplyModifiedPropertiesWithoutUndo();

            var mapName = MakeText(content, "MapName", "", 34f, new Color(0.91f, 0.93f, 0.96f), new Vector2(0f, -24f));

            var barBackground = MakeImage(content, "BarBackground", new Vector2(460f, 8f), new Vector2(0f, -96f),
                new Color(0.1f, 0.14f, 0.19f, 1f));

            var barFill = MakeImage(content, "BarFill", new Vector2(460f, 8f), new Vector2(0f, -96f),
                new Color(0.36f, 0.62f, 0.72f, 1f));
            barFill.type = Image.Type.Filled;
            barFill.fillMethod = Image.FillMethod.Horizontal;
            barFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            barFill.fillAmount = 0f;

            var screen = rootGo.AddComponent<LoadingScreen>();
            var screenSerialized = new SerializedObject(screen);
            screenSerialized.FindProperty("mapName").objectReferenceValue = mapName;
            screenSerialized.FindProperty("progressFill").objectReferenceValue = barFill;
            screenSerialized.ApplyModifiedPropertiesWithoutUndo();

            rootGo.SetActive(false);
            return screen;
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, string value, float size, Color color,
            Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(860f, 90f);
            rect.anchoredPosition = position;

            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private static Image MakeImage(Transform parent, string name, Vector2 size, Vector2 position, Color color)
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
            return image;
        }

        private static void EnsureFolder(string path)
        {
            RATIXMapKit.EnsurePath(path);
        }
    }
}
