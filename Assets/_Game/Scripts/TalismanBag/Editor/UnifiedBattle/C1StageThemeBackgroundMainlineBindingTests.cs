using System;
using System.IO;
using System.Reflection;
using TalismanBag.Presentation.StageThemes;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.StageThemes;
using TalismanBag.V04.Campaign.Chapter1;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.UnifiedBattle
{
    public static class C1StageThemeBackgroundMainlineBindingTests
    {
        private const string HostSourcePath =
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs";
        private const string RegistrySourcePath =
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/StageThemes/C1StageThemeBackgroundRegistry.cs";
        private const string SuccessDiagnostic =
            "C1_STAGE_THEME_BACKGROUND_MAINLINE_BINDING_FOCUSED_TESTS_PASS";

        [MenuItem(
            "TalismanBag/V0.4/Unified Battle/Run C1 Stage Theme Mainline Binding Tests")]
        public static void RunFocusedTests()
        {
            RunFocusedTestsForAuthoring();
            Debug.Log(SuccessDiagnostic);
        }

        internal static void RunFocusedTestsForAuthoring()
        {
            C1StageThemeBackgroundRegistry registry =
                AssetDatabase.LoadAssetAtPath<C1StageThemeBackgroundRegistry>(
                    C1StageThemeBackgroundMainlineBindingAuthoring.RegistryAssetPath);
            Require(registry != null,
                "STAGE_THEME_TEST_REGISTRY_ASSET_MISSING");
            Require(registry.TryValidate(out string registryDiagnostic),
                registryDiagnostic);

            VerifyExactResolutionMatrix(registry);
            VerifyFailClosedMatrix(registry);
            VerifyCurrentStageMappings();
            VerifyHostAndPresenterLifecycle(registry);
            VerifyRuntimeStaticBoundaries();
        }

        private static void VerifyExactResolutionMatrix(
            C1StageThemeBackgroundRegistry registry)
        {
            RequireResolved(
                registry,
                C1StageThemeBackgroundRegistry.InteriorProfileId,
                registry.InteriorProfile);
            RequireResolved(
                registry,
                C1StageThemeBackgroundRegistry.ExteriorProfileId,
                registry.ExteriorProfile);
            RequireResolved(
                registry,
                C1StageThemeBackgroundRegistry.NightAnimatedProfileId,
                registry.NightAnimatedProfile);
        }

        private static void VerifyFailClosedMatrix(
            C1StageThemeBackgroundRegistry registry)
        {
            Require(!registry.TryResolve(
                    string.Empty,
                    out C1StageThemeBackgroundProfile emptyResult,
                    out string emptyDiagnostic),
                "STAGE_THEME_TEST_EMPTY_ID_ACCEPTED");
            Require(emptyResult == null,
                "STAGE_THEME_TEST_EMPTY_ID_RETURNED_PROFILE");
            Require(string.Equals(
                    emptyDiagnostic,
                    "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_ID_MISSING",
                    StringComparison.Ordinal),
                "STAGE_THEME_TEST_EMPTY_ID_DIAGNOSTIC_MISMATCH actual=" +
                emptyDiagnostic);

            const string unknownId =
                "campaign.normal.lv1.theme.bone_aspect.c1.unknown";
            Require(!registry.TryResolve(
                    unknownId,
                    out C1StageThemeBackgroundProfile unknownResult,
                    out string unknownDiagnostic),
                "STAGE_THEME_TEST_UNKNOWN_ID_ACCEPTED");
            Require(unknownResult == null,
                "STAGE_THEME_TEST_UNKNOWN_ID_RETURNED_PROFILE");
            Require(string.Equals(
                    unknownDiagnostic,
                    "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_UNKNOWN profile=" +
                    unknownId,
                    StringComparison.Ordinal),
                "STAGE_THEME_TEST_UNKNOWN_ID_DIAGNOSTIC_MISMATCH actual=" +
                unknownDiagnostic);

            C1StageThemeBackgroundRegistry duplicateRegistry =
                ScriptableObject.CreateInstance<
                    C1StageThemeBackgroundRegistry>();
            C1StageThemeBackgroundRegistry mismatchedRegistry =
                ScriptableObject.CreateInstance<
                    C1StageThemeBackgroundRegistry>();
            try
            {
                duplicateRegistry.ConfigureForEditor(
                    registry.InteriorProfile,
                    registry.InteriorProfile,
                    registry.NightAnimatedProfile);
                Require(!duplicateRegistry.TryResolve(
                        C1StageThemeBackgroundRegistry.InteriorProfileId,
                        out C1StageThemeBackgroundProfile duplicateResult,
                        out string duplicateDiagnostic),
                    "STAGE_THEME_TEST_DUPLICATE_REGISTRY_ACCEPTED");
                Require(duplicateResult == null,
                    "STAGE_THEME_TEST_DUPLICATE_REGISTRY_RETURNED_PROFILE");
                Require(string.Equals(
                        duplicateDiagnostic,
                        "STAGE_THEME_BACKGROUND_REGISTRY_DUPLICATE_REFERENCE slots=interior,exterior",
                        StringComparison.Ordinal),
                    "STAGE_THEME_TEST_DUPLICATE_DIAGNOSTIC_MISMATCH actual=" +
                    duplicateDiagnostic);

                mismatchedRegistry.ConfigureForEditor(
                    registry.ExteriorProfile,
                    registry.InteriorProfile,
                    registry.NightAnimatedProfile);
                Require(!mismatchedRegistry.TryResolve(
                        C1StageThemeBackgroundRegistry.InteriorProfileId,
                        out C1StageThemeBackgroundProfile mismatchedResult,
                        out string mismatchedDiagnostic),
                    "STAGE_THEME_TEST_MISMATCHED_REGISTRY_ACCEPTED");
                Require(mismatchedResult == null,
                    "STAGE_THEME_TEST_MISMATCHED_REGISTRY_RETURNED_PROFILE");
                Require(string.Equals(
                        mismatchedDiagnostic,
                        "STAGE_THEME_BACKGROUND_REGISTRY_PROFILE_ID_MISMATCH slot=interior expected=" +
                        C1StageThemeBackgroundRegistry.InteriorProfileId +
                        " actual=" +
                        C1StageThemeBackgroundRegistry.ExteriorProfileId,
                        StringComparison.Ordinal),
                    "STAGE_THEME_TEST_MISMATCHED_DIAGNOSTIC_MISMATCH actual=" +
                    mismatchedDiagnostic);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(duplicateRegistry);
                UnityEngine.Object.DestroyImmediate(mismatchedRegistry);
            }
        }

        private static void VerifyCurrentStageMappings()
        {
            Chapter1CampaignStageConfig stageOne =
                AssetDatabase.LoadAssetAtPath<Chapter1CampaignStageConfig>(
                    C1StageThemeBackgroundMainlineBindingAuthoring
                        .StageOneConfigPath);
            Chapter1CampaignStageConfig stageTwo =
                AssetDatabase.LoadAssetAtPath<Chapter1CampaignStageConfig>(
                    C1StageThemeBackgroundMainlineBindingAuthoring
                        .StageTwoConfigPath);
            Require(stageOne != null && stageTwo != null,
                "STAGE_THEME_TEST_CAMPAIGN_STAGE_CONFIG_MISSING");
            Require(string.Equals(
                    stageOne.StageThemeProfileId,
                    C1StageThemeBackgroundRegistry.InteriorProfileId,
                    StringComparison.Ordinal),
                "STAGE_THEME_TEST_STAGE_1_1_NOT_INTERIOR actual=" +
                stageOne.StageThemeProfileId);
            Require(string.Equals(
                    stageTwo.StageThemeProfileId,
                    C1StageThemeBackgroundRegistry.InteriorProfileId,
                    StringComparison.Ordinal),
                "STAGE_THEME_TEST_STAGE_1_2_NOT_INTERIOR actual=" +
                stageTwo.StageThemeProfileId);
        }

        private static void VerifyHostAndPresenterLifecycle(
            C1StageThemeBackgroundRegistry registry)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                C1StageThemeBackgroundMainlineBindingAuthoring.ShellPrefabPath);
            try
            {
                UnifiedBattleFormalSceneHost[] hosts =
                    root.GetComponentsInChildren<
                        UnifiedBattleFormalSceneHost>(true);
                C1StageThemeBackgroundPresenter[] presenters =
                    root.GetComponentsInChildren<
                        C1StageThemeBackgroundPresenter>(true);
                Require(hosts.Length == 1,
                    "STAGE_THEME_TEST_HOST_COUNT_INVALID count=" +
                    hosts.Length);
                Require(presenters.Length == 1,
                    "STAGE_THEME_TEST_BACKGROUND_OWNER_COUNT_INVALID count=" +
                    presenters.Length);

                UnifiedBattleFormalSceneHost host = hosts[0];
                C1StageThemeBackgroundPresenter presenter = presenters[0];
                Require(host.StageThemeBackgroundRegistry == registry,
                    "STAGE_THEME_TEST_HOST_REGISTRY_REFERENCE_MISMATCH");
                Require(host.StageThemeBackgroundPresenter == presenter,
                    "STAGE_THEME_TEST_HOST_PRESENTER_REFERENCE_MISMATCH");
                Require(host.GameBackgroundImage != null
                        && host.GameBackgroundImage.gameObject ==
                        presenter.gameObject
                        && presenter.TargetImage == host.GameBackgroundImage
                        && !host.GameBackgroundImage.raycastTarget,
                    "STAGE_THEME_TEST_BACKGROUND_IMAGE_BINDING_INVALID");
                Require(string.Equals(
                        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                            presenter.gameObject),
                        C1StageThemeBackgroundMainlineBindingAuthoring
                            .BackgroundPrefabPath,
                        StringComparison.Ordinal),
                    "STAGE_THEME_TEST_BACKGROUND_SOURCE_PREFAB_MISMATCH");

                Sprite authoredSafeSprite = presenter.AuthoredSafeSprite;
                Require(presenter.Apply(
                        registry.InteriorProfile,
                        out string applyDiagnostic),
                    applyDiagnostic);
                Require(presenter.ActiveProfile == registry.InteriorProfile,
                    "STAGE_THEME_TEST_VALID_APPLY_PROFILE_MISMATCH");

                MethodInfo resetMethod = typeof(UnifiedBattleFormalSceneHost)
                    .GetMethod(
                        "ResetStageThemePresentation",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Require(resetMethod != null,
                    "STAGE_THEME_TEST_HOST_RESET_METHOD_MISSING");
                resetMethod.Invoke(host, null);
                resetMethod.Invoke(host, null);
                Require(presenter.ActiveProfile == null
                        && !presenter.IsPlaying
                        && presenter.TargetImage.sprite == authoredSafeSprite,
                    "STAGE_THEME_TEST_HOST_RESET_NOT_IDEMPOTENT");

                Require(presenter.Apply(
                        registry.NightAnimatedProfile,
                        out string reentryApplyDiagnostic),
                    reentryApplyDiagnostic);
                resetMethod.Invoke(host, null);
                Require(presenter.ActiveProfile == null
                        && !presenter.IsPlaying
                        && presenter.TargetImage.sprite == authoredSafeSprite,
                    "STAGE_THEME_TEST_REENTRY_RESET_FAILED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void VerifyRuntimeStaticBoundaries()
        {
            string hostSource = File.ReadAllText(Path.GetFullPath(HostSourcePath))
                .Replace("\r\n", "\n");
            string registrySource = File.ReadAllText(
                    Path.GetFullPath(RegistrySourcePath))
                .Replace("\r\n", "\n");

            RequireNotContains(hostSource, "Resources.Load");
            RequireNotContains(hostSource, "GameObject.Find");
            RequireNotContains(hostSource, "new GameObject");
            RequireNotContains(hostSource, "Instantiate(");
            RequireNotContains(registrySource, "Resources.Load");
            RequireNotContains(registrySource, "AssetDatabase");
            RequireNotContains(registrySource, "Dictionary<");

            RequireContains(
                hostSource,
                "stageThemeBackgroundRegistry.TryResolve(\n                    consumed.StageThemeProfileId");
            RequireContains(
                hostSource,
                "stageThemeBackgroundPresenter.Apply(\n                    resolvedThemeProfile");
            RequireContains(
                hostSource,
                "private void ApplyFormalDormantState()\n        {\n            ResetStageThemePresentation();");
            RequireContains(
                hostSource,
                "private void ClearFormalLoop()\n        {\n            ResetStageThemePresentation();");
            RequireContains(
                hostSource,
                "private void OnDisable()\n        {\n            ClearFormalLoop();\n            contextAdmissionAttempted = false;");
            RequireContains(
                hostSource,
                "contextAdmissionAttempted = true;\n            ResetStageThemePresentation();");
            RequireContains(
                hostSource,
                "formalLoop = null;\n                ResetStageThemePresentation();\n                SetSummary(\n                    themeApplyDiagnostic");

            int resetCallCount = CountOccurrences(
                hostSource,
                "ResetStageThemePresentation();");
            Require(resetCallCount >= 8,
                "STAGE_THEME_TEST_LIFECYCLE_RESET_COVERAGE_INCOMPLETE count=" +
                resetCallCount);
        }

        private static void RequireResolved(
            C1StageThemeBackgroundRegistry registry,
            string profileId,
            C1StageThemeBackgroundProfile expected)
        {
            Require(registry.TryResolve(
                    profileId,
                    out C1StageThemeBackgroundProfile actual,
                    out string diagnostic),
                diagnostic);
            Require(actual == expected,
                "STAGE_THEME_TEST_RESOLVED_REFERENCE_MISMATCH profile=" +
                profileId);
            Require(string.Equals(
                    actual.ProfileId,
                    profileId,
                    StringComparison.Ordinal),
                "STAGE_THEME_TEST_RESOLVED_ID_MISMATCH profile=" + profileId);
        }

        private static int CountOccurrences(string source, string value)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(
                       value,
                       index,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static void RequireContains(string source, string expected)
        {
            Require(source.IndexOf(expected, StringComparison.Ordinal) >= 0,
                "STAGE_THEME_TEST_REQUIRED_SOURCE_PATTERN_MISSING pattern=" +
                expected);
        }

        private static void RequireNotContains(string source, string forbidden)
        {
            Require(source.IndexOf(forbidden, StringComparison.Ordinal) < 0,
                "STAGE_THEME_TEST_FORBIDDEN_SOURCE_PATTERN_PRESENT pattern=" +
                forbidden);
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }
    }
}
