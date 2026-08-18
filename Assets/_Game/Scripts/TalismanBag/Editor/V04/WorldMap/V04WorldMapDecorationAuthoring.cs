#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.V04.WorldMap
{
    public static class V04WorldMapDecorationAuthoring
    {
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_WorldMap.unity";
        private const string LightAtlasPath =
            "Assets/_Game/Resources/V04/WorldMapDecoration/WorldMap_UI_LightAtlas_v1.png";
        private const string DarkAtlasPath =
            "Assets/_Game/Resources/V04/WorldMapDecoration/WorldMap_UI_DarkAtlas_v1.png";
        private const string PassMarker =
            "WORLD_MAP_DECORATION_VISUAL_ASSETS_V1_AUTHORING_PASS";

        [MenuItem("TalismanBag/V0.4/World Map/Apply Decoration Visual Assets V1")]
        public static void ApplyFromMenu()
        {
            Apply();
            Debug.Log(PassMarker);
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                Apply();
                Debug.Log(PassMarker);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.Log("WORLD_MAP_DECORATION_VISUAL_ASSETS_V1_AUTHORING_FAIL");
                EditorApplication.Exit(1);
            }
        }

        private static void Apply()
        {
            if (Application.isPlaying)
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_DECORATION_AUTHORING_REQUIRES_EDIT_MODE");
            }

            ConfigureLightAtlas();
            ConfigureDarkAtlas();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Dictionary<string, Sprite> light = LoadSprites(LightAtlasPath);
            Dictionary<string, Sprite> dark = LoadSprites(DarkAtlasPath);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplySceneDecoration(scene, light, dark);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_DECORATION_SCENE_SAVE_FAILED");
            }

            AssetDatabase.SaveAssets();
        }

        private static void ConfigureLightAtlas()
        {
            ConfigureAtlas(
                LightAtlasPath,
                new[]
                {
                    Meta("light_top_bar", 30, 35, 1475, 167, new Vector4(105, 45, 105, 45)),
                    Meta("light_chapter_title", 400, 240, 718, 159, new Vector4(42, 38, 42, 38)),
                    Meta("light_stage_neutral", 85, 437, 328, 330),
                    Meta("light_stage_current", 471, 433, 336, 352),
                    Meta("light_stage_locked", 854, 435, 336, 337),
                    Meta("light_compass", 1218, 313, 304, 568),
                    Meta("light_cloud_left", 65, 830, 425, 135),
                    Meta("light_cloud_right", 521, 828, 426, 135),
                    Meta("light_region_plaque", 984, 839, 341, 146, new Vector4(58, 38, 58, 38))
                });
        }

        private static void ConfigureDarkAtlas()
        {
            ConfigureAtlas(
                DarkAtlasPath,
                new[]
                {
                    Meta("dark_detail_drawer", 48, 53, 1114, 542, new Vector4(52, 52, 52, 52)),
                    Meta("dark_art_well", 1219, 54, 273, 554, new Vector4(30, 30, 30, 30)),
                    Meta("dark_info_row_1", 50, 626, 513, 70, new Vector4(34, 20, 34, 20)),
                    Meta("dark_info_row_2", 50, 714, 513, 70, new Vector4(34, 20, 34, 20)),
                    Meta("dark_info_row_3", 50, 800, 513, 70, new Vector4(34, 20, 34, 20)),
                    Meta("dark_info_row_4", 50, 887, 513, 70, new Vector4(34, 20, 34, 20)),
                    Meta("dark_primary_button", 642, 629, 565, 154, new Vector4(58, 42, 58, 42)),
                    Meta("dark_secondary_button", 642, 804, 565, 152, new Vector4(58, 42, 58, 42)),
                    Meta("dark_icon_well", 1262, 727, 228, 211, new Vector4(28, 28, 28, 28))
                });
        }

        private static SpriteMetaData Meta(
            string name,
            float x,
            float top,
            float width,
            float height,
            Vector4 border = default)
        {
            const float atlasHeight = 1024f;
            return new SpriteMetaData
            {
                name = name,
                rect = new Rect(x, atlasHeight - top - height, width, height),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f),
                border = border
            };
        }

        private static void ConfigureAtlas(string assetPath, SpriteMetaData[] sprites)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_DECORATION_TEXTURE_IMPORTER_MISSING path=" + assetPath);
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 100f;
            importer.spritesheet = sprites;
            importer.SaveAndReimport();
        }

        private static Dictionary<string, Sprite> LoadSprites(string assetPath)
        {
            Dictionary<string, Sprite> result = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Sprite>()
                .ToDictionary(sprite => sprite.name, StringComparer.Ordinal);
            if (result.Count == 0)
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_DECORATION_SPRITES_MISSING path=" + assetPath);
            }

            return result;
        }

        private static void ApplySceneDecoration(
            Scene scene,
            IReadOnlyDictionary<string, Sprite> light,
            IReadOnlyDictionary<string, Sprite> dark)
        {
            Image topBar = FindRequired<Image>(scene, "TopBar");
            AssignSliced(topBar, light["light_top_bar"]);

            AssignSliced(
                FindRequired<Image>(scene, "QingshifangRegionHotspotButton"),
                light["light_region_plaque"]);
            AssignSimple(
                FindRequired<Image>(scene, "QingshifangRegionImageSlot"),
                light["light_compass"],
                true);

            for (int index = 1; index <= 4; index++)
            {
                AssignSliced(
                    FindRequired<Image>(scene, "ChapterEntry_" + index),
                    light["light_region_plaque"]);
            }

            for (int index = 1; index <= 10; index++)
            {
                AssignSimple(
                    FindRequired<Image>(scene, "StageNode_" + index),
                    light["light_stage_neutral"],
                    true);
            }

            AssignSliced(
                FindRequired<Image>(scene, "StageDetailDrawer"),
                dark["dark_detail_drawer"]);
            for (int index = 1; index <= 3; index++)
            {
                AssignSliced(
                    FindRequired<Image>(scene, "DropPreviewRow_" + index),
                    dark["dark_info_row_" + index]);
            }

            AssignSliced(
                FindRequired<Image>(scene, "ChallengeButton"),
                dark["dark_primary_button"]);
            AssignSliced(
                FindRequired<Image>(scene, "SetPatrolButton"),
                dark["dark_secondary_button"]);
            AssignSliced(
                FindRequired<Image>(scene, "CloseDrawerButton"),
                dark["dark_secondary_button"]);
            AssignSliced(
                FindRequired<Image>(scene, "BackButton"),
                dark["dark_secondary_button"]);
            AssignSliced(
                FindRequired<Image>(scene, "WorldMapUpgradeButton"),
                dark["dark_secondary_button"]);

            SetButtonLabelColor(scene, "ChallengeButton", new Color(1f, 0.94f, 0.78f, 1f));
            SetButtonLabelColor(scene, "SetPatrolButton", new Color(0.16f, 0.12f, 0.08f, 1f));
            SetButtonLabelColor(scene, "CloseDrawerButton", new Color(0.16f, 0.12f, 0.08f, 1f));
            SetButtonLabelColor(scene, "BackButton", new Color(0.16f, 0.12f, 0.08f, 1f));
            SetButtonLabelColor(scene, "WorldMapUpgradeButton", new Color(0.16f, 0.12f, 0.08f, 1f));

            Transform chapterMap = FindRequiredTransform(scene, "ChapterStageMapView");
            Image chapterTitle = UpsertDecorationImage(
                chapterMap,
                "Decoration_ChapterTitlePlaque",
                light["light_chapter_title"],
                new Vector2(0f, 590f),
                new Vector2(760f, 112f),
                true);
            chapterTitle.transform.SetSiblingIndex(1);
            FindRequired<Text>(scene, "ChapterTitleText").color =
                new Color(1f, 0.92f, 0.74f, 1f);

            Image cloudLeft = UpsertDecorationImage(
                chapterMap,
                "Decoration_CloudRouteLeft",
                light["light_cloud_left"],
                new Vector2(-270f, 45f),
                new Vector2(390f, 124f),
                true);
            cloudLeft.color = new Color(1f, 1f, 1f, 0.72f);
            cloudLeft.transform.SetSiblingIndex(2);

            Image cloudRight = UpsertDecorationImage(
                chapterMap,
                "Decoration_CloudRouteRight",
                light["light_cloud_right"],
                new Vector2(260f, -295f),
                new Vector2(390f, 124f),
                true);
            cloudRight.color = new Color(1f, 1f, 1f, 0.66f);
            cloudRight.transform.SetSiblingIndex(3);
        }

        private static Image UpsertDecorationImage(
            Transform parent,
            string objectName,
            Sprite sprite,
            Vector2 anchoredPosition,
            Vector2 size,
            bool preserveAspect)
        {
            Transform existing = parent.Find(objectName);
            GameObject target = existing != null
                ? existing.gameObject
                : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = target.GetComponent<Image>();
            AssignSimple(image, sprite, preserveAspect);
            image.raycastTarget = false;
            return image;
        }

        private static void AssignSimple(Image image, Sprite sprite, bool preserveAspect)
        {
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.color = Color.white;
        }

        private static void AssignSliced(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.fillCenter = true;
            image.color = Color.white;
        }

        private static void SetButtonLabelColor(Scene scene, string buttonName, Color color)
        {
            Transform button = FindRequiredTransform(scene, buttonName);
            Text label = button.GetComponentsInChildren<Text>(true)
                .FirstOrDefault(text => string.Equals(text.name, "Label", StringComparison.Ordinal));
            if (label != null)
            {
                label.color = color;
            }
        }

        private static T FindRequired<T>(Scene scene, string objectName)
            where T : Component
        {
            Transform transform = FindRequiredTransform(scene, objectName);
            T component = transform.GetComponent<T>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    "WORLD_MAP_DECORATION_COMPONENT_MISSING object=" + objectName
                    + " component=" + typeof(T).Name);
            }

            return component;
        }

        private static Transform FindRequiredTransform(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Transform found = FindDeepChild(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            throw new InvalidOperationException(
                "WORLD_MAP_DECORATION_OBJECT_MISSING name=" + objectName);
        }

        private static Transform FindDeepChild(Transform root, string objectName)
        {
            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            foreach (Transform child in root)
            {
                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
