using System;
using System.Collections.Generic;
using TalismanBag.Presentation.StageThemes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.StageThemes
{
    public static class C1StageThemeBackgroundPrefabAuthoring
    {
        internal const string PackageId =
            "V0.4-C1StageThemeBackgroundPrefabAndProfiles01";
        internal const string PrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxGameBackground.prefab";
        internal const string ProfilesParentPath =
            "Assets/_Game/Resources/V04";
        internal const string ProfilesFolderPath =
            ProfilesParentPath + "/StageThemeBackgrounds";
        internal const string InteriorProfilePath =
            ProfilesFolderPath + "/C1StageThemeBackground_Interior.asset";
        internal const string ExteriorProfilePath =
            ProfilesFolderPath + "/C1StageThemeBackground_Exterior.asset";
        internal const string NightProfilePath =
            ProfilesFolderPath + "/C1StageThemeBackground_NightAnimated.asset";
        internal const string InteriorSpritePath =
            "Assets/_Game/Resources/场景地图/ChatGPT Image 2026年7月24日 17_40_43.png";
        internal const string ExteriorSpritePath =
            "Assets/_Game/Resources/场景地图/ChatGPT Image 2026年7月24日 17_40_50.png";
        internal const string SequenceFolderPath =
            "Assets/_Game/Resources/场景地图/场景动画序列帧";

        internal const string InteriorProfileId =
            "campaign.normal.lv1.theme.bone_aspect.c1";
        internal const string ExteriorProfileId =
            "campaign.normal.lv1.theme.bone_aspect.c1.exterior";
        internal const string NightProfileId =
            "campaign.normal.lv1.theme.bone_aspect.c1.night_animated";

        internal const int SequenceFrameCount = 11;
        internal const float FramesPerSecond = 10f;
        internal const float LoopFrameOneHoldSeconds = 5f;

        [MenuItem(
            "Tools/TalismanBag/V0.4/Author C1 Stage Theme Background Carrier")]
        public static void AuthorFromMenu()
        {
            AuthorAndValidate();
        }

        public static void AuthorBatch()
        {
            try
            {
                AuthorAndValidate();
                Debug.Log("[" + PackageId + "] AUTHORING_PASS");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[" + PackageId + "] AUTHORING_FAIL " +
                    exception.Message);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                throw;
            }
        }

        internal static string GetSequenceFramePath(int oneBasedPosition)
        {
            return SequenceFolderPath + "/frame_" +
                   oneBasedPosition.ToString("00000") + ".png";
        }

        private static void AuthorAndValidate()
        {
            Dictionary<string, byte[]> protectedPngBytes =
                CaptureProtectedPngBytes();
            AuthoringSnapshot prefabSnapshot = CapturePrefabSnapshot();

            ConfigureSequenceImports();
            EnsureProfileFolder();

            Sprite interiorSprite = LoadRequiredSprite(InteriorSpritePath);
            Sprite exteriorSprite = LoadRequiredSprite(ExteriorSpritePath);
            Sprite[] sequenceFrames = LoadSequenceFrames();

            ConfigureProfile(
                InteriorProfilePath,
                InteriorProfileId,
                C1StageThemeBackgroundMode.StaticSprite,
                interiorSprite,
                Array.Empty<Sprite>());
            ConfigureProfile(
                ExteriorProfilePath,
                ExteriorProfileId,
                C1StageThemeBackgroundMode.StaticSprite,
                exteriorSprite,
                Array.Empty<Sprite>());
            ConfigureProfile(
                NightProfilePath,
                NightProfileId,
                C1StageThemeBackgroundMode.SpriteSequence,
                null,
                sequenceFrames);

            UpgradePrefab(interiorSprite, prefabSnapshot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            C1StageThemeBackgroundCarrierTests.ValidateAllOrThrow(
                protectedPngBytes,
                prefabSnapshot);
        }

        private static void ConfigureSequenceImports()
        {
            for (int i = 1; i <= SequenceFrameCount; i++)
            {
                string path = GetSequenceFramePath(i);
                TextureImporter importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_FRAME_IMPORTER_MISSING path=" +
                        path);
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.alphaIsTransparency = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.textureCompression =
                    TextureImporterCompression.Compressed;
                importer.compressionQuality = 50;

                TextureImporterPlatformSettings android =
                    importer.GetPlatformTextureSettings("Android");
                android.name = "Android";
                android.overridden = true;
                android.maxTextureSize = 2048;
                android.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                android.format = TextureImporterFormat.Automatic;
                android.textureCompression =
                    TextureImporterCompression.Compressed;
                android.compressionQuality = 50;
                android.crunchedCompression = false;
                importer.SetPlatformTextureSettings(android);

                importer.SaveAndReimport();
            }
        }

        private static void EnsureProfileFolder()
        {
            if (!AssetDatabase.IsValidFolder(ProfilesParentPath))
            {
                throw new InvalidOperationException(
                    "STAGE_THEME_BACKGROUND_PROFILE_PARENT_MISSING path=" +
                    ProfilesParentPath);
            }

            if (!AssetDatabase.IsValidFolder(ProfilesFolderPath))
            {
                AssetDatabase.CreateFolder(
                    ProfilesParentPath,
                    "StageThemeBackgrounds");
            }
        }

        private static void ConfigureProfile(
            string assetPath,
            string profileId,
            C1StageThemeBackgroundMode mode,
            Sprite staticSprite,
            Sprite[] sequenceFrames)
        {
            C1StageThemeBackgroundProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    C1StageThemeBackgroundProfile>(assetPath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<
                    C1StageThemeBackgroundProfile>();
                AssetDatabase.CreateAsset(profile, assetPath);
            }

            profile.ConfigureForEditor(
                profileId,
                mode,
                staticSprite,
                sequenceFrames,
                FramesPerSecond,
                LoopFrameOneHoldSeconds);
            if (!profile.TryValidate(out string diagnostic))
            {
                throw new InvalidOperationException(diagnostic);
            }

            EditorUtility.SetDirty(profile);
        }

        private static Sprite LoadRequiredSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException(
                    "STAGE_THEME_BACKGROUND_SPRITE_MISSING path=" + path);
            }

            return sprite;
        }

        private static Sprite[] LoadSequenceFrames()
        {
            Sprite[] frames = new Sprite[SequenceFrameCount];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = LoadRequiredSprite(GetSequenceFramePath(i + 1));
            }

            return frames;
        }

        private static void UpgradePrefab(
            Sprite interiorSprite,
            AuthoringSnapshot expectedSnapshot)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Image[] images = root.GetComponentsInChildren<Image>(true);
                if (images.Length != 1 || images[0].gameObject != root)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PREFAB_REQUIRES_ONE_ROOT_IMAGE");
                }

                C1StageThemeBackgroundPresenter[] presenters =
                    root.GetComponentsInChildren<
                        C1StageThemeBackgroundPresenter>(true);
                if (presenters.Length > 1)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_DUPLICATE_PRESENTER");
                }

                C1StageThemeBackgroundPresenter presenter =
                    presenters.Length == 1
                        ? presenters[0]
                        : root.AddComponent<
                            C1StageThemeBackgroundPresenter>();
                if (presenter.gameObject != root)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PRESENTER_NOT_LOCAL");
                }

                images[0].raycastTarget = false;
                presenter.AssignForEditor(images[0], interiorSprite);
                EditorUtility.SetDirty(images[0]);
                EditorUtility.SetDirty(presenter);

                AuthoringSnapshot currentSnapshot =
                    AuthoringSnapshot.Capture(root, images[0]);
                expectedSnapshot.RequireLayoutAndImagePresentationEqual(
                    currentSnapshot);

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static AuthoringSnapshot CapturePrefabSnapshot()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Image[] images = root.GetComponentsInChildren<Image>(true);
                if (images.Length != 1 || images[0].gameObject != root)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PREFAB_BASELINE_INVALID");
                }

                return AuthoringSnapshot.Capture(root, images[0]);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Dictionary<string, byte[]>
            CaptureProtectedPngBytes()
        {
            Dictionary<string, byte[]> bytesByPath =
                new Dictionary<string, byte[]>(StringComparer.Ordinal)
                {
                    [InteriorSpritePath] = ReadAssetBytes(InteriorSpritePath),
                    [ExteriorSpritePath] = ReadAssetBytes(ExteriorSpritePath)
                };
            for (int i = 1; i <= SequenceFrameCount; i++)
            {
                string path = GetSequenceFramePath(i);
                bytesByPath.Add(path, ReadAssetBytes(path));
            }

            return bytesByPath;
        }

        internal static byte[] ReadAssetBytes(string projectRelativePath)
        {
            return System.IO.File.ReadAllBytes(projectRelativePath);
        }

        internal readonly struct AuthoringSnapshot
        {
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 pivot;
            private readonly Vector3 localPosition;
            private readonly Vector3 localScale;
            private readonly Quaternion localRotation;
            private readonly Color imageColor;
            private readonly Material imageMaterial;
            private readonly Sprite imageSprite;
            private readonly bool imageEnabled;
            private readonly bool preserveAspect;
            private readonly int childCount;

            private AuthoringSnapshot(GameObject root, Image image)
            {
                RectTransform rect = (RectTransform)root.transform;
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                pivot = rect.pivot;
                localPosition = rect.localPosition;
                localScale = rect.localScale;
                localRotation = rect.localRotation;
                imageColor = image.color;
                imageMaterial = image.material == image.defaultMaterial
                    ? null
                    : image.material;
                imageSprite = image.sprite;
                imageEnabled = image.enabled;
                preserveAspect = image.preserveAspect;
                childCount = root.transform.childCount;
            }

            internal static AuthoringSnapshot Capture(
                GameObject root,
                Image image)
            {
                return new AuthoringSnapshot(root, image);
            }

            internal void RequireLayoutAndImagePresentationEqual(
                AuthoringSnapshot actual)
            {
                if (anchorMin != actual.anchorMin
                    || anchorMax != actual.anchorMax
                    || anchoredPosition != actual.anchoredPosition
                    || sizeDelta != actual.sizeDelta
                    || pivot != actual.pivot
                    || localPosition != actual.localPosition
                    || localScale != actual.localScale
                    || localRotation != actual.localRotation
                    || imageColor != actual.imageColor
                    || imageMaterial != actual.imageMaterial
                    || imageSprite != actual.imageSprite
                    || imageEnabled != actual.imageEnabled
                    || preserveAspect != actual.preserveAspect
                    || childCount != actual.childCount)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_AUTHORED_PRESENTATION_CHANGED");
                }
            }
        }
    }
}
