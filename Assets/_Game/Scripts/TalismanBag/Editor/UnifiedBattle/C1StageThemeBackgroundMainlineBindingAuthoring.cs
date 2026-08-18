using System;
using System.Globalization;
using System.IO;
using TalismanBag.Presentation.StageThemes;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.StageThemes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.UnifiedBattle
{
    public static class C1StageThemeBackgroundMainlineBindingAuthoring
    {
        public const string ShellPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";
        public const string BackgroundPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxGameBackground.prefab";
        public const string RegistryAssetPath =
            "Assets/_Game/Resources/V04/StageThemeBackgrounds/C1StageThemeBackgroundRegistry.asset";
        public const string InteriorProfilePath =
            "Assets/_Game/Resources/V04/StageThemeBackgrounds/C1StageThemeBackground_Interior.asset";
        public const string ExteriorProfilePath =
            "Assets/_Game/Resources/V04/StageThemeBackgrounds/C1StageThemeBackground_Exterior.asset";
        public const string NightProfilePath =
            "Assets/_Game/Resources/V04/StageThemeBackgrounds/C1StageThemeBackground_NightAnimated.asset";
        public const string UnifiedScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        public const string StageOneConfigPath =
            "Assets/_Game/Resources/TalismanBag/Campaign/Chapter1/Stages/CampaignStage_1-1.asset";
        public const string StageTwoConfigPath =
            "Assets/_Game/Resources/TalismanBag/Campaign/Chapter1/Stages/CampaignStage_1-2.asset";

        private const string PresenterSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Presentation/StageThemes/C1StageThemeBackgroundPresenter.cs";
        private const string ProfileSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Presentation/StageThemes/C1StageThemeBackgroundProfile.cs";
        private const string MenuPath =
            "TalismanBag/V0.4/Unified Battle/Apply C1 Stage Theme Mainline Binding Once";
        private const string SuccessMarker =
            "C1_STAGE_THEME_BACKGROUND_MAINLINE_BINDING_AUTHORING_PASS";
        private const string FailureMarker =
            "C1_STAGE_THEME_BACKGROUND_MAINLINE_BINDING_AUTHORING_FAIL";

        public static string MarkerPath => Path.Combine(
            Path.GetTempPath(),
            "codex_v04_c1_stage_theme_mainline_binding_authoring.marker.txt");

        [MenuItem(MenuPath)]
        public static void ApplyFromMenu()
        {
            string receipt = ApplyAndValidateOnce();
            Debug.Log(receipt);
        }

        public static void RunOwnedAuthoring()
        {
            try
            {
                string receipt = ApplyAndValidateOnce();
                File.WriteAllText(
                    MarkerPath,
                    SuccessMarker + Environment.NewLine + receipt);
                Debug.Log("[C1StageThemeMainlineBinding] " + receipt);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    MarkerPath,
                    FailureMarker + Environment.NewLine + exception);
                Debug.LogException(exception);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                    return;
                }

                throw;
            }
        }

        public static string ApplyAndValidateOnce()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "STAGE_THEME_AUTHORING_REJECTED_EDITOR_IS_PLAYING");
            Require(!EditorApplication.isCompiling,
                "STAGE_THEME_AUTHORING_REJECTED_EDITOR_IS_COMPILING");

            ValidateRequiredInputs();
            string shellBeforeText = ReadAssetText(ShellPrefabPath);
            GeometrySnapshot initialGeometry = default;
            bool authored = false;

            GameObject root = PrefabUtility.LoadPrefabContents(ShellPrefabPath);
            try
            {
                BindingSnapshot snapshot = InspectBinding(root);
                initialGeometry = GeometrySnapshot.Capture(
                    snapshot.Host.GameBackgroundImage.rectTransform);

                bool unbound = snapshot.Host.StageThemeBackgroundRegistry == null
                               && snapshot.Host.StageThemeBackgroundPresenter == null;
                bool bound = snapshot.Host.StageThemeBackgroundRegistry != null
                             && snapshot.Host.StageThemeBackgroundPresenter != null;
                Require(unbound || bound,
                    "STAGE_THEME_AUTHORING_PARTIAL_HOST_BINDING_REJECTED");

                C1StageThemeBackgroundRegistry existingRegistry =
                    AssetDatabase.LoadAssetAtPath<
                        C1StageThemeBackgroundRegistry>(RegistryAssetPath);

                if (unbound)
                {
                    Require(existingRegistry == null,
                        "STAGE_THEME_AUTHORING_PARTIAL_REGISTRY_ASSET_REJECTED");

                    C1StageThemeBackgroundRegistry registry =
                        ScriptableObject.CreateInstance<
                            C1StageThemeBackgroundRegistry>();
                    registry.ConfigureForEditor(
                        LoadRequiredProfile(InteriorProfilePath),
                        LoadRequiredProfile(ExteriorProfilePath),
                        LoadRequiredProfile(NightProfilePath));
                    Require(registry.TryValidate(out string registryDiagnostic),
                        registryDiagnostic);
                    AssetDatabase.CreateAsset(registry, RegistryAssetPath);
                    EditorUtility.SetDirty(registry);
                    AssetDatabase.SaveAssets();

                    snapshot.Host.AssignStageThemeBackgroundForEditor(
                        registry,
                        snapshot.Presenter);
                    EditorUtility.SetDirty(snapshot.Host);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(
                        snapshot.Host);
                    Require(
                        PrefabUtility.SaveAsPrefabAsset(root, ShellPrefabPath) !=
                        null,
                        "STAGE_THEME_AUTHORING_SHELL_SAVE_FAILED");
                    authored = true;
                }
                else
                {
                    Require(existingRegistry != null,
                        "STAGE_THEME_AUTHORING_BOUND_REGISTRY_ASSET_MISSING");
                    Require(snapshot.Host.StageThemeBackgroundRegistry ==
                            existingRegistry,
                        "STAGE_THEME_AUTHORING_BOUND_REGISTRY_CONFLICT");
                    Require(snapshot.Host.StageThemeBackgroundPresenter ==
                            snapshot.Presenter,
                        "STAGE_THEME_AUTHORING_BOUND_PRESENTER_CONFLICT");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateRequiredInputs();
            ValidateSavedState(initialGeometry);
            C1StageThemeBackgroundMainlineBindingTests.RunFocusedTestsForAuthoring();

            string shellAfterText = ReadAssetText(ShellPrefabPath);
            if (authored)
            {
                // The exact final state must take the validation-only branch.
                ValidateValidationOnlyReentry(shellAfterText, initialGeometry);
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} mode={1} shellChanged={2} requiredInputs=valid",
                SuccessMarker,
                authored ? "AUTHORED" : "VALIDATION_ONLY",
                string.Equals(
                    shellBeforeText,
                    shellAfterText,
                    StringComparison.Ordinal)
                    ? "0"
                    : "1");
        }

        private static void ValidateValidationOnlyReentry(
            string expectedShellText,
            GeometrySnapshot expectedGeometry)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(ShellPrefabPath);
            try
            {
                BindingSnapshot snapshot = InspectBinding(root);
                Require(snapshot.Host.StageThemeBackgroundRegistry != null,
                    "STAGE_THEME_AUTHORING_REENTRY_REGISTRY_MISSING");
                Require(snapshot.Host.StageThemeBackgroundPresenter ==
                        snapshot.Presenter,
                    "STAGE_THEME_AUTHORING_REENTRY_PRESENTER_MISMATCH");
                Require(expectedGeometry.Equals(GeometrySnapshot.Capture(
                        snapshot.Host.GameBackgroundImage.rectTransform)),
                    "STAGE_THEME_AUTHORING_REENTRY_GEOMETRY_CHANGED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            Require(string.Equals(
                    expectedShellText,
                    ReadAssetText(ShellPrefabPath),
                    StringComparison.Ordinal),
                "STAGE_THEME_AUTHORING_REENTRY_WROTE_SHELL");
        }

        private static void ValidateSavedState(GeometrySnapshot initialGeometry)
        {
            C1StageThemeBackgroundRegistry registry =
                AssetDatabase.LoadAssetAtPath<C1StageThemeBackgroundRegistry>(
                    RegistryAssetPath);
            Require(registry != null,
                "STAGE_THEME_AUTHORING_REGISTRY_ASSET_MISSING_AFTER_SAVE");
            Require(registry.TryValidate(out string registryDiagnostic),
                registryDiagnostic);

            GameObject root = PrefabUtility.LoadPrefabContents(ShellPrefabPath);
            try
            {
                BindingSnapshot snapshot = InspectBinding(root);
                Require(snapshot.Host.StageThemeBackgroundRegistry == registry,
                    "STAGE_THEME_AUTHORING_SAVED_REGISTRY_REFERENCE_MISMATCH");
                Require(snapshot.Host.StageThemeBackgroundPresenter ==
                        snapshot.Presenter,
                    "STAGE_THEME_AUTHORING_SAVED_PRESENTER_REFERENCE_MISMATCH");
                Require(initialGeometry.Equals(GeometrySnapshot.Capture(
                        snapshot.Host.GameBackgroundImage.rectTransform)),
                    "STAGE_THEME_AUTHORING_BACKGROUND_GEOMETRY_CHANGED");
                Require(snapshot.Host.ValidateAuthoredBindings(
                        out string hostDiagnostic),
                    hostDiagnostic);
                RequireZeroMissingScripts(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static BindingSnapshot InspectBinding(GameObject root)
        {
            UnifiedBattleFormalSceneHost[] hosts =
                root.GetComponentsInChildren<UnifiedBattleFormalSceneHost>(true);
            Require(hosts.Length == 1,
                "STAGE_THEME_AUTHORING_HOST_COUNT_INVALID count=" +
                hosts.Length);
            UnifiedBattleFormalSceneHost host = hosts[0];

            C1StageThemeBackgroundPresenter[] presenters =
                root.GetComponentsInChildren<
                    C1StageThemeBackgroundPresenter>(true);
            Require(presenters.Length == 1,
                "STAGE_THEME_AUTHORING_PRESENTER_COUNT_INVALID count=" +
                presenters.Length);
            C1StageThemeBackgroundPresenter presenter = presenters[0];
            Image image = host.GameBackgroundImage;
            Require(image != null,
                "STAGE_THEME_AUTHORING_BACKGROUND_IMAGE_MISSING");
            Require(!image.raycastTarget,
                "STAGE_THEME_AUTHORING_BACKGROUND_IMAGE_INTERCEPTS_INPUT");
            Require(presenter.gameObject == image.gameObject,
                "STAGE_THEME_AUTHORING_PRESENTER_NOT_ON_BACKGROUND_IMAGE");
            Require(presenter.TargetImage == image,
                "STAGE_THEME_AUTHORING_PRESENTER_IMAGE_REFERENCE_MISMATCH");
            Require(presenter.ValidateAuthoredReferences(
                    out string presenterDiagnostic),
                presenterDiagnostic);
            Require(string.Equals(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        presenter.gameObject),
                    BackgroundPrefabPath,
                    StringComparison.Ordinal),
                "STAGE_THEME_AUTHORING_BACKGROUND_SOURCE_PREFAB_MISMATCH");

            return new BindingSnapshot(host, presenter);
        }

        private static C1StageThemeBackgroundProfile LoadRequiredProfile(
            string path)
        {
            C1StageThemeBackgroundProfile profile =
                AssetDatabase.LoadAssetAtPath<C1StageThemeBackgroundProfile>(path);
            Require(profile != null,
                "STAGE_THEME_AUTHORING_PROFILE_ASSET_MISSING path=" + path);
            return profile;
        }

        private static void ValidateRequiredInputs()
        {
            string[] paths =
            {
                ShellPrefabPath,
                BackgroundPrefabPath,
                PresenterSourcePath,
                ProfileSourcePath,
                InteriorProfilePath,
                ExteriorProfilePath,
                NightProfilePath,
                UnifiedScenePath,
                StageOneConfigPath,
                StageTwoConfigPath
            };
            foreach (string path in paths)
            {
                Require(File.Exists(Path.GetFullPath(path)),
                    "STAGE_THEME_AUTHORING_REQUIRED_INPUT_MISSING path=" + path);
            }
        }

        private static void RequireZeroMissingScripts(GameObject root)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                Require(components[i] != null,
                    "STAGE_THEME_AUTHORING_MISSING_SCRIPT index=" + i);
            }
        }

        private static string ReadAssetText(string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            Require(File.Exists(fullPath),
                "STAGE_THEME_AUTHORING_FILE_MISSING path=" + assetPath);
            return File.ReadAllText(fullPath);
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }

        private readonly struct BindingSnapshot
        {
            public BindingSnapshot(
                UnifiedBattleFormalSceneHost host,
                C1StageThemeBackgroundPresenter presenter)
            {
                Host = host;
                Presenter = presenter;
            }

            public UnifiedBattleFormalSceneHost Host { get; }
            public C1StageThemeBackgroundPresenter Presenter { get; }
        }

        private readonly struct GeometrySnapshot : IEquatable<GeometrySnapshot>
        {
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 pivot;
            private readonly Vector3 localPosition;
            private readonly Quaternion localRotation;
            private readonly Vector3 localScale;

            private GeometrySnapshot(RectTransform rect)
            {
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                pivot = rect.pivot;
                localPosition = rect.localPosition;
                localRotation = rect.localRotation;
                localScale = rect.localScale;
            }

            public static GeometrySnapshot Capture(RectTransform rect)
            {
                Require(rect != null,
                    "STAGE_THEME_AUTHORING_BACKGROUND_RECT_MISSING");
                return new GeometrySnapshot(rect);
            }

            public bool Equals(GeometrySnapshot other)
            {
                return anchorMin == other.anchorMin
                       && anchorMax == other.anchorMax
                       && anchoredPosition == other.anchoredPosition
                       && sizeDelta == other.sizeDelta
                       && pivot == other.pivot
                       && localPosition == other.localPosition
                       && localRotation == other.localRotation
                       && localScale == other.localScale;
            }

            public override bool Equals(object obj)
            {
                return obj is GeometrySnapshot other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = anchorMin.GetHashCode();
                    hash = (hash * 397) ^ anchorMax.GetHashCode();
                    hash = (hash * 397) ^ anchoredPosition.GetHashCode();
                    hash = (hash * 397) ^ sizeDelta.GetHashCode();
                    hash = (hash * 397) ^ pivot.GetHashCode();
                    hash = (hash * 397) ^ localPosition.GetHashCode();
                    hash = (hash * 397) ^ localRotation.GetHashCode();
                    return (hash * 397) ^ localScale.GetHashCode();
                }
            }
        }
    }
}
