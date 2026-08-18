using System;
using System.Collections.Generic;
using TalismanBag.Presentation.StageThemes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.StageThemes
{
    public static class C1StageThemeBackgroundCarrierTests
    {
        [MenuItem(
            "Tools/TalismanBag/V0.4/Validate C1 Stage Theme Background Carrier")]
        public static void ValidateFromMenu()
        {
            ValidateAllOrThrow(null, null);
            Debug.Log(
                "[" + C1StageThemeBackgroundPrefabAuthoring.PackageId +
                "] VALIDATION_PASS");
        }

        internal static void ValidateAllOrThrow(
            IReadOnlyDictionary<string, byte[]> protectedPngBytes,
            C1StageThemeBackgroundPrefabAuthoring.AuthoringSnapshot?
                expectedPrefabSnapshot)
        {
            ValidatePrefab(expectedPrefabSnapshot);
            ValidateProfiles();
            ValidateSequenceImports();
            ValidateProtectedPngBytes(protectedPngBytes);
        }

        private static void ValidatePrefab(
            C1StageThemeBackgroundPrefabAuthoring.AuthoringSnapshot?
                expectedSnapshot)
        {
            string prefabPath =
                C1StageThemeBackgroundPrefabAuthoring.PrefabPath;
            string[] dependencies =
                AssetDatabase.GetDependencies(prefabPath, true);
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (dependencies[i].EndsWith(
                        ".unity",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PREFAB_SCENE_DEPENDENCY path=" +
                        dependencies[i]);
                }
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (!string.Equals(
                        root.name,
                        "C1ExactBattleSandboxGameBackground",
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PREFAB_ROOT_NAME_INVALID");
                }

                Transform[] transforms =
                    root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            transforms[i].gameObject) > 0)
                    {
                        throw new InvalidOperationException(
                            "STAGE_THEME_BACKGROUND_PREFAB_MISSING_SCRIPT object=" +
                            transforms[i].name);
                    }
                }

                Image[] images = root.GetComponentsInChildren<Image>(true);
                if (images.Length != 1
                    || images[0].gameObject != root
                    || images[0].raycastTarget)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PREFAB_IMAGE_CONTRACT_INVALID");
                }

                C1StageThemeBackgroundPresenter[] presenters =
                    root.GetComponentsInChildren<
                        C1StageThemeBackgroundPresenter>(true);
                string presenterDiagnostic =
                    "STAGE_THEME_BACKGROUND_PRESENTER_COUNT_INVALID";
                bool presenterValid = presenters.Length == 1
                    && presenters[0].ValidateAuthoredReferences(
                        out presenterDiagnostic);
                if (presenters.Length != 1
                    || presenters[0].gameObject != root
                    || presenters[0].TargetImage != images[0]
                    || presenters[0].AuthoredSafeSprite != images[0].sprite
                    || !presenterValid)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PREFAB_PRESENTER_INVALID " +
                        presenterDiagnostic);
                }

                if (!(root.transform is RectTransform))
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PREFAB_RECT_MISSING");
                }

                if (expectedSnapshot.HasValue)
                {
                    C1StageThemeBackgroundPrefabAuthoring.AuthoringSnapshot
                        actual = C1StageThemeBackgroundPrefabAuthoring
                            .AuthoringSnapshot.Capture(root, images[0]);
                    expectedSnapshot.Value
                        .RequireLayoutAndImagePresentationEqual(actual);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateProfiles()
        {
            C1StageThemeBackgroundProfile interior = LoadRequiredProfile(
                C1StageThemeBackgroundPrefabAuthoring.InteriorProfilePath);
            C1StageThemeBackgroundProfile exterior = LoadRequiredProfile(
                C1StageThemeBackgroundPrefabAuthoring.ExteriorProfilePath);
            C1StageThemeBackgroundProfile night = LoadRequiredProfile(
                C1StageThemeBackgroundPrefabAuthoring.NightProfilePath);

            ValidateStaticProfile(
                interior,
                C1StageThemeBackgroundPrefabAuthoring.InteriorProfileId,
                C1StageThemeBackgroundPrefabAuthoring.InteriorSpritePath);
            ValidateStaticProfile(
                exterior,
                C1StageThemeBackgroundPrefabAuthoring.ExteriorProfileId,
                C1StageThemeBackgroundPrefabAuthoring.ExteriorSpritePath);

            if (!night.TryValidate(out string diagnostic)
                || !string.Equals(
                    night.ProfileId,
                    C1StageThemeBackgroundPrefabAuthoring.NightProfileId,
                    StringComparison.Ordinal)
                || night.Mode !=
                    C1StageThemeBackgroundMode.SpriteSequence
                || night.StaticSprite != null
                || night.SequenceFrames == null
                || night.SequenceFrames.Length !=
                    C1StageThemeBackgroundPrefabAuthoring.SequenceFrameCount
                || !Mathf.Approximately(
                    night.FramesPerSecond,
                    C1StageThemeBackgroundPrefabAuthoring.FramesPerSecond)
                || !Mathf.Approximately(
                    night.LoopFrameOneHoldSeconds,
                    C1StageThemeBackgroundPrefabAuthoring
                        .LoopFrameOneHoldSeconds))
            {
                throw new InvalidOperationException(
                    "STAGE_THEME_BACKGROUND_NIGHT_PROFILE_INVALID " +
                    diagnostic);
            }

            for (int i = 0; i < night.SequenceFrames.Length; i++)
            {
                string expectedPath =
                    C1StageThemeBackgroundPrefabAuthoring
                        .GetSequenceFramePath(i + 1);
                string actualPath =
                    AssetDatabase.GetAssetPath(night.SequenceFrames[i]);
                if (!string.Equals(
                        actualPath,
                        expectedPath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_SEQUENCE_ORDER_INVALID position=" +
                        (i + 1) + " actual=" + actualPath);
                }
            }
        }

        private static C1StageThemeBackgroundProfile LoadRequiredProfile(
            string path)
        {
            C1StageThemeBackgroundProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    C1StageThemeBackgroundProfile>(path);
            if (profile == null)
            {
                throw new InvalidOperationException(
                    "STAGE_THEME_BACKGROUND_PROFILE_ASSET_MISSING path=" +
                    path);
            }

            return profile;
        }

        private static void ValidateStaticProfile(
            C1StageThemeBackgroundProfile profile,
            string expectedId,
            string expectedSpritePath)
        {
            if (!profile.TryValidate(out string diagnostic)
                || !string.Equals(
                    profile.ProfileId,
                    expectedId,
                    StringComparison.Ordinal)
                || profile.Mode != C1StageThemeBackgroundMode.StaticSprite
                || profile.StaticSprite == null
                || !string.Equals(
                    AssetDatabase.GetAssetPath(profile.StaticSprite),
                    expectedSpritePath,
                    StringComparison.Ordinal)
                || (profile.SequenceFrames != null
                    && profile.SequenceFrames.Length != 0))
            {
                throw new InvalidOperationException(
                    "STAGE_THEME_BACKGROUND_STATIC_PROFILE_INVALID id=" +
                    expectedId + " " + diagnostic);
            }
        }

        private static void ValidateSequenceImports()
        {
            for (int i = 1;
                 i <= C1StageThemeBackgroundPrefabAuthoring
                     .SequenceFrameCount;
                 i++)
            {
                string path = C1StageThemeBackgroundPrefabAuthoring
                    .GetSequenceFramePath(i);
                TextureImporter importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (importer == null
                    || importer.textureType != TextureImporterType.Sprite
                    || importer.spriteImportMode != SpriteImportMode.Single
                    || importer.mipmapEnabled
                    || importer.isReadable
                    || importer.maxTextureSize != 2048
                    || importer.textureCompression ==
                        TextureImporterCompression.Uncompressed
                    || sprite == null
                    || sprite.texture == null
                    || sprite.texture.width != 1672
                    || sprite.texture.height != 941)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_FRAME_IMPORT_INVALID path=" +
                        path);
                }

                TextureImporterPlatformSettings android =
                    importer.GetPlatformTextureSettings("Android");
                if (!android.overridden
                    || android.maxTextureSize != 2048
                    || android.textureCompression ==
                        TextureImporterCompression.Uncompressed)
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_ANDROID_IMPORT_INVALID path=" +
                        path);
                }
            }
        }

        private static void ValidateProtectedPngBytes(
            IReadOnlyDictionary<string, byte[]> expectedBytesByPath)
        {
            if (expectedBytesByPath == null)
            {
                return;
            }

            foreach (KeyValuePair<string, byte[]> pair in expectedBytesByPath)
            {
                byte[] actual = C1StageThemeBackgroundPrefabAuthoring
                    .ReadAssetBytes(pair.Key);
                if (!BytesEqual(actual, pair.Value))
                {
                    throw new InvalidOperationException(
                        "STAGE_THEME_BACKGROUND_PROTECTED_PNG_CHANGED path=" +
                        pair.Key);
                }
            }
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
