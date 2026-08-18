using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.BuildSandbox
{
    public static class LiHuoCombatFeedbackOrchestrationVerifier
    {
        public const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        public const string ControllerPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/"
            + "LiHuoCombatFeedbackOrchestrationController.cs";
        public const string ProfilePath =
            "Assets/_Game/Resources/V04/LiHuoCombatFeedbackDevOnly/"
            + "lihuo_combat_feedback_profile.json";
        public const string AtlasPath =
            "Assets/_Game/Resources/V04/LiHuoCombatFeedbackDevOnly/"
            + "lihuo_vfx_atlas_alpha_v1.png";
        public const string RequiredTmpFontPath =
            "Assets/TextMesh Pro/Fonts/"
            + "MFLangSongJianYuan-Regular SDF.asset";
        public const string RequiredSourceFontPath =
            "Assets/TextMesh Pro/Fonts/"
            + "MFLangSongJianYuan-Regular.otf";

        private const string StaticPassMarker =
            "LIHUO_ORCHESTRATION_STATIC_QA_PASS";
        private const string RenderedPassMarker =
            "LIHUO_ORCHESTRATION_RENDERED_QA_PASS";
        private const string AndroidPassMarker =
            "LIHUO_ORCHESTRATION_ANDROID_BUILD_QA_PASS";

        private const string RenderedPendingKey =
            "LiHuo.Rendered.Pending";
        private const string RenderedCaptureDirectoryKey =
            "LiHuo.Rendered.CaptureDirectory";
        private const string RenderedResultReadyKey =
            "LiHuo.Rendered.ResultReady";
        private const string RenderedPassedKey =
            "LiHuo.Rendered.Passed";
        private const string RenderedEvidenceKey =
            "LiHuo.Rendered.Evidence";

        [InitializeOnLoadMethod]
        private static void InitializeRenderedQaLifecycle()
        {
            RegisterRenderedQaLifecycle();
            if (SessionState.GetBool(RenderedPendingKey, false))
            {
                EditorApplication.delayCall -= EnsureRenderedDriver;
                EditorApplication.delayCall += EnsureRenderedDriver;
            }
        }

        [MenuItem(
            "Tools/Talisman Bag/Dev Only/V0.4/Verify LiHuo Combat Feedback Orchestration")]
        public static void VerifyFromMenu()
        {
            VerifyStaticContract(true);
        }

        public static void RunStaticBatch()
        {
            bool passed = VerifyStaticContract(true);
            if (!passed)
            {
                throw new InvalidOperationException(
                    "LiHuo orchestration static QA failed.");
            }
        }

        public static bool VerifyStaticContract(bool log)
        {
            List<string> failures = new();
            VerifyRequiredAssets(failures);
            VerifyCueGate(failures);
            VerifyStyleRouting(failures);
            VerifyAtlasAlpha(failures);
            VerifySourceBoundary(failures);

            if (failures.Count == 0)
            {
                if (log)
                {
                    Debug.Log(
                        "[" + LiHuoCombatFeedbackOrchestrationController
                            .PackageId
                        + "] " + StaticPassMarker
                        + " events=24"
                        + " alphaAtlas=source-square-positive"
                        + " noItemIdStyleBranch=true"
                        + " noTruthWrite=true"
                        + " noSceneSave=true");
                }
                return true;
            }

            foreach (string failure in failures)
            {
                Debug.LogError(
                    "[" + LiHuoCombatFeedbackOrchestrationController
                        .PackageId
                    + "] STATIC_QA_FAIL " + failure);
            }
            return false;
        }

        private static void VerifyRequiredAssets(List<string> failures)
        {
            string[] required =
            {
                ScenePath,
                ControllerPath,
                ProfilePath,
                AtlasPath,
                RequiredTmpFontPath,
                RequiredSourceFontPath
            };
            foreach (string path in required)
            {
                if (!File.Exists(path))
                {
                    failures.Add("missing=" + path);
                }
            }

            TextAsset profile = AssetDatabase.LoadAssetAtPath<TextAsset>(
                ProfilePath);
            if (profile == null)
            {
                failures.Add("profile_import=null");
            }
            else
            {
                LiHuoCombatFeedbackReplaceableProfile parsed =
                    JsonUtility.FromJson<
                        LiHuoCombatFeedbackReplaceableProfile>(
                        profile.text);
                if (parsed == null
                    || !string.Equals(
                        parsed.buildFamilyStableKey,
                        "lihuo",
                        StringComparison.Ordinal)
                    || !string.Equals(
                        parsed.taiBaiReservedStableKey,
                        "taibai",
                        StringComparison.Ordinal)
                    || parsed.poolSize < 40
                    || parsed.impactEmphasisSeconds < 0.035f
                    || parsed.impactEmphasisSeconds > 0.055f)
                {
                    failures.Add("replaceable_profile=invalid");
                }
            }

            TextureImporter importer =
                AssetImporter.GetAtPath(AtlasPath) as TextureImporter;
            if (importer == null
                || importer.textureType != TextureImporterType.Default
                || importer.mipmapEnabled
                || !importer.alphaIsTransparency
                || importer.maxTextureSize < 1536)
            {
                failures.Add("atlas_import_contract=invalid");
            }
        }

        private static void VerifyCueGate(List<string> failures)
        {
            LiHuoCombatFeedbackCueGate gate = new();
            for (int index = 0; index < 24; index++)
            {
                bool accepted = gate.TryAccept(
                    3,
                    1500L + index * 1500L,
                    "accepted.lihuo."
                    + index.ToString("D2", CultureInfo.InvariantCulture));
                if (!accepted)
                {
                    failures.Add("cue_gate_rejected=" + index);
                    break;
                }
            }
            if (gate.AcceptedCount != 24
                || gate.TryAccept(
                    3,
                    37500L,
                    "accepted.lihuo.23")
                || gate.TryAccept(
                    3,
                    100L,
                    "accepted.lihuo.stale"))
            {
                failures.Add("cue_gate_dedup_or_monotonic=false");
            }
            if (!gate.TryAccept(
                    4,
                    1500L,
                    "accepted.lihuo.new_generation")
                || gate.Generation != 4
                || gate.AcceptedCount != 1)
            {
                failures.Add("cue_gate_generation_reset=false");
            }
        }

        private static void VerifyStyleRouting(List<string> failures)
        {
            if (LiHuoCombatFeedbackOrchestrationController
                    .ResolveBuildFamily("lihuo")
                != LiHuoCombatFeedbackBuildFamily.LiHuo
                || LiHuoCombatFeedbackOrchestrationController
                    .ResolveBuildFamily("taibai")
                != LiHuoCombatFeedbackBuildFamily.TaiBaiReserved
                || LiHuoCombatFeedbackOrchestrationController
                    .ResolveBuildFamily("zhenlei")
                != LiHuoCombatFeedbackBuildFamily.Unknown)
            {
                failures.Add("build_family_router=false");
            }

            BattleSandboxAcceptedItemPresentationEvent hp =
                NewPresentation("hp", 0, 37);
            BattleSandboxAcceptedItemPresentationEvent shell =
                NewPresentation("shell", 29, 0);
            if (LiHuoCombatFeedbackOrchestrationController
                    .ResolveOutcomeKind(hp, false)
                != LiHuoCombatFeedbackOutcomeKind.HpDirectDamage
                || LiHuoCombatFeedbackOrchestrationController
                    .ResolveOutcomeKind(shell, false)
                != LiHuoCombatFeedbackOutcomeKind.ShellDamage
                || LiHuoCombatFeedbackOrchestrationController
                    .ResolveOutcomeKind(shell, true)
                != LiHuoCombatFeedbackOutcomeKind.ShellBreak)
            {
                failures.Add("outcome_router=false");
            }
        }

        private static BattleSandboxAcceptedItemPresentationEvent
            NewPresentation(
                string suffix,
                int shell,
                int hp)
        {
            return new BattleSandboxAcceptedItemPresentationEvent(
                "pulse." + suffix,
                "nian." + suffix,
                "ledger." + suffix,
                1,
                1500L,
                "synthetic-source",
                "synthetic-instance",
                "synthetic-placement",
                new[] { new ItemShapeCell(0, 0) },
                new[] { new ItemShapeCell(1, 1) },
                4,
                2,
                shell + hp,
                shell,
                hp);
        }

        private static void VerifyAtlasAlpha(List<string> failures)
        {
            if (!File.Exists(AtlasPath))
            {
                return;
            }

            byte[] bytes = File.ReadAllBytes(AtlasPath);
            Texture2D decoded = new(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(decoded, bytes, false)
                    || decoded.width < 1024
                    || decoded.height < 1024
                    || decoded.width != decoded.height)
                {
                    failures.Add("atlas_source_dimensions=false");
                    return;
                }

                Color32[] pixels = decoded.GetPixels32();
                int transparent = pixels.Count(value => value.a == 0);
                int partial = pixels.Count(value =>
                    value.a > 0 && value.a < 255);
                if (transparent < 1200000 || partial < 80000)
                {
                    failures.Add(
                        "atlas_alpha_semantics=false transparent="
                        + transparent
                        + " partial=" + partial);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        private static void VerifySourceBoundary(List<string> failures)
        {
            if (!File.Exists(ControllerPath))
            {
                return;
            }

            string source = File.ReadAllText(ControllerPath);
            string[] forbidden =
            {
                "System.Reflection",
                "BindingFlags",
                "StartManual(",
                "Time.timeScale",
                "ApplyDamage(",
                "Acknowledge(",
                "SceneManager.LoadScene",
                "EditorSceneManager.SaveScene",
                "AssetDatabase.SaveAssets",
                "\"I007\"",
                "\"I008\"",
                "\"I009\"",
                "\"I010\"",
                "\"I011\"",
                "\"I012\""
            };
            foreach (string token in forbidden)
            {
                if (source.Contains(token, StringComparison.Ordinal))
                {
                    failures.Add("forbidden_token=" + token);
                }
            }

            if (!source.Contains(
                    "LastAcceptedItemPresentation",
                    StringComparison.Ordinal)
                || !source.Contains(
                    "sourceFaMenTag",
                    StringComparison.Ordinal)
                || !source.Contains(
                    "TryResolveBoardSourceWorldPosition",
                    StringComparison.Ordinal)
                || !source.Contains(
                    "Time.unscaledDeltaTime",
                    StringComparison.Ordinal)
                || !source.Contains(
                    "UsesItemArtworkAsProjectile => false",
                    StringComparison.Ordinal)
                || !source.Contains(
                    "HasPerLoopSpriteAllocation => false",
                    StringComparison.Ordinal))
            {
                failures.Add("required_presentation_contract=missing");
            }

            int spriteCreateCount = CountOccurrences(
                source,
                "Sprite.Create(");
            if (spriteCreateCount != 2)
            {
                failures.Add(
                    "sprite_create_sites=" + spriteCreateCount);
            }
        }

        private static int CountOccurrences(
            string source,
            string token)
        {
            int count = 0;
            int cursor = 0;
            while (cursor < source.Length)
            {
                int found = source.IndexOf(
                    token,
                    cursor,
                    StringComparison.Ordinal);
                if (found < 0)
                {
                    break;
                }
                count++;
                cursor = found + token.Length;
            }
            return count;
        }

        public static void RunRenderedQa()
        {
            if (!VerifyStaticContract(true))
            {
                throw new InvalidOperationException(
                    "Static QA must pass before rendered QA.");
            }
            if (Application.isBatchMode)
            {
                throw new InvalidOperationException(
                    "Rendered QA requires a normal graphics Editor.");
            }
            if (SessionState.GetBool(RenderedPendingKey, false))
            {
                return;
            }

            string captureDirectory =
                ResolveArgument(
                    "-lihuoCaptureDir",
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.MyDocuments),
                        "符箓",
                        "_lihuo_feedback_qa"));
            Directory.CreateDirectory(captureDirectory);
            SessionState.SetBool(RenderedPendingKey, true);
            SessionState.SetString(
                RenderedCaptureDirectoryKey,
                captureDirectory);
            SessionState.SetBool(RenderedResultReadyKey, false);
            SessionState.SetBool(RenderedPassedKey, false);
            SessionState.SetString(RenderedEvidenceKey, string.Empty);
            RegisterRenderedQaLifecycle();
            EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);
            Debug.Log(
                "[" + LiHuoCombatFeedbackOrchestrationController.PackageId
                + "] RENDERED_QA_START capture="
                + captureDirectory);
            EditorApplication.isPlaying = true;
        }

        private static void RegisterRenderedQaLifecycle()
        {
            EditorApplication.playModeStateChanged -=
                OnRenderedPlayModeChanged;
            EditorApplication.playModeStateChanged +=
                OnRenderedPlayModeChanged;
        }

        private static void OnRenderedPlayModeChanged(
            PlayModeStateChange state)
        {
            if (!SessionState.GetBool(RenderedPendingKey, false))
            {
                return;
            }
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall -= EnsureRenderedDriver;
                EditorApplication.delayCall += EnsureRenderedDriver;
                return;
            }
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            CompleteRenderedQa();
        }

        private static void EnsureRenderedDriver()
        {
            if (!SessionState.GetBool(RenderedPendingKey, false)
                || !EditorApplication.isPlaying)
            {
                return;
            }
            if (UnityEngine.Object.FindObjectOfType<
                    LiHuoCombatFeedbackRenderedQaDriver>() != null)
            {
                return;
            }

            GameObject driverObject = new(
                "LiHuoCombatFeedbackRenderedQaDriver");
            driverObject.hideFlags =
                HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild;
            LiHuoCombatFeedbackRenderedQaDriver driver =
                driverObject.AddComponent<
                    LiHuoCombatFeedbackRenderedQaDriver>();
            driver.Begin(
                SessionState.GetString(
                    RenderedCaptureDirectoryKey,
                    string.Empty));
        }

        private static void CompleteRenderedQa()
        {
            SessionState.SetBool(RenderedPendingKey, false);
            EditorApplication.delayCall -= EnsureRenderedDriver;
            bool sceneStable =
                !SceneManager.GetActiveScene().isDirty;
            bool resultReady =
                SessionState.GetBool(RenderedResultReadyKey, false);
            bool passed =
                resultReady
                && SessionState.GetBool(RenderedPassedKey, false)
                && sceneStable;
            string evidence =
                SessionState.GetString(
                    RenderedEvidenceKey,
                    "driver_result_missing");
            if (passed)
            {
                Debug.Log(
                    "[" + LiHuoCombatFeedbackOrchestrationController
                        .PackageId
                    + "] " + RenderedPassMarker
                    + " sceneStable=true " + evidence);
            }
            else
            {
                Debug.LogError(
                    "[" + LiHuoCombatFeedbackOrchestrationController
                        .PackageId
                    + "] RENDERED_QA_FAIL sceneStable="
                    + sceneStable
                    + " resultReady=" + resultReady
                    + " " + evidence);
            }

            SessionState.SetBool(RenderedResultReadyKey, false);
            SessionState.SetBool(RenderedPassedKey, false);
            SessionState.SetString(RenderedEvidenceKey, string.Empty);
            SessionState.SetString(
                RenderedCaptureDirectoryKey,
                string.Empty);
            EditorApplication.delayCall += () =>
                EditorApplication.Exit(passed ? 0 : 1);
        }

        internal static void StoreRenderedDriverResult(
            bool passed,
            string evidence)
        {
            SessionState.SetBool(RenderedResultReadyKey, true);
            SessionState.SetBool(RenderedPassedKey, passed);
            SessionState.SetString(
                RenderedEvidenceKey,
                evidence ?? string.Empty);
        }

        public static void RunAndroidBuildQa()
        {
            if (!VerifyStaticContract(true))
            {
                throw new InvalidOperationException(
                    "Static QA must pass before Android build QA.");
            }
            string output = ResolveArgument(
                "-lihuoAndroidOutput",
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments),
                    "符箓",
                    "_lihuo_feedback_qa",
                    "LiHuoFeedbackDev.apk"));
            Directory.CreateDirectory(
                Path.GetDirectoryName(output) ?? ".");
            Font requiredSourceFont =
                AssetDatabase.LoadAssetAtPath<Font>(
                    RequiredSourceFontPath);
            if (requiredSourceFont == null)
            {
                throw new InvalidOperationException(
                    "Required runtime Chinese source font is missing.");
            }
            UnityEngine.Object[] previousPreloadedAssets =
                PlayerSettings.GetPreloadedAssets()
                ?? Array.Empty<UnityEngine.Object>();
            UnityEngine.Object[] buildPreloadedAssets =
                previousPreloadedAssets
                    .Where(value => value != null)
                    .Concat(new UnityEngine.Object[]
                    {
                        requiredSourceFont
                    })
                    .Distinct()
                    .ToArray();
            BuildPlayerOptions options = new()
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options =
                    BuildOptions.Development
                    | BuildOptions.CompressWithLz4
            };
            BuildReport report;
            try
            {
                PlayerSettings.SetPreloadedAssets(
                    buildPreloadedAssets);
                report = BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                PlayerSettings.SetPreloadedAssets(
                    previousPreloadedAssets);
                AssetDatabase.SaveAssets();
            }
            bool preloadedAssetsRestored =
                PlayerSettings.GetPreloadedAssets()
                    .SequenceEqual(previousPreloadedAssets);
            bool succeeded =
                report.summary.result == BuildResult.Succeeded
                && File.Exists(output)
                && new FileInfo(output).Length > 0;
            HashSet<string> packedSources = new(
                report.packedAssets
                    .SelectMany(value =>
                        value.contents
                            ?? Array.Empty<PackedAssetInfo>())
                    .Select(value =>
                        value.sourceAssetPath ?? string.Empty),
                StringComparer.Ordinal);
            bool atlasIncluded = packedSources.Contains(AtlasPath);
            bool profileIncluded = packedSources.Contains(ProfilePath);
            bool tmpIncluded =
                packedSources.Contains(RequiredTmpFontPath);
            bool sourceFontIncluded =
                packedSources.Contains(RequiredSourceFontPath);
            bool included =
                atlasIncluded
                && profileIncluded
                && tmpIncluded
                && sourceFontIncluded
                && preloadedAssetsRestored;
            if (!succeeded || !included)
            {
                throw new InvalidOperationException(
                    "Android build/inclusion QA failed: result="
                    + report.summary.result
                    + " atlas=" + atlasIncluded
                    + " profile=" + profileIncluded
                    + " tmp=" + tmpIncluded
                    + " sourceFont=" + sourceFontIncluded
                    + " preloadedAssetsRestored="
                    + preloadedAssetsRestored);
            }
            Debug.Log(
                "[" + LiHuoCombatFeedbackOrchestrationController.PackageId
                + "] " + AndroidPassMarker
                + " output=" + output
                + " bytes=" + new FileInfo(output).Length
                + " atlas=true profile=true tmp=true sourceFont=true"
                + " preloadedAssetsRestored=true");
        }

        private static string ResolveArgument(
            string key,
            string fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(
                    args[index],
                    key,
                    StringComparison.Ordinal))
                {
                    return Path.GetFullPath(args[index + 1]);
                }
            }
            return Path.GetFullPath(fallback);
        }

    }

    public sealed class LiHuoCombatFeedbackRenderedQaDriver :
        MonoBehaviour
    {
        private readonly struct QaPlacement
        {
            public readonly string itemId;
            public readonly ItemShapeCell cell;
            public readonly ItemShapeRotation rotation;

            public QaPlacement(
                string itemId,
                ItemShapeCell cell,
                ItemShapeRotation rotation)
            {
                this.itemId = itemId;
                this.cell = cell;
                this.rotation = rotation;
            }
        }

        private readonly struct RectSnapshot
        {
            public readonly RectTransform rect;
            public readonly Vector2 anchoredPosition;
            public readonly Vector2 sizeDelta;
            public readonly Vector2 anchorMin;
            public readonly Vector2 anchorMax;
            public readonly Vector2 pivot;
            public readonly Vector3 localScale;
            public readonly Quaternion localRotation;
            public readonly Transform parent;

            public RectSnapshot(RectTransform value)
            {
                rect = value;
                anchoredPosition =
                    value == null ? default : value.anchoredPosition;
                sizeDelta =
                    value == null ? default : value.sizeDelta;
                anchorMin =
                    value == null ? default : value.anchorMin;
                anchorMax =
                    value == null ? default : value.anchorMax;
                pivot =
                    value == null ? default : value.pivot;
                localScale =
                    value == null ? default : value.localScale;
                localRotation =
                    value == null ? default : value.localRotation;
                parent = value == null ? null : value.parent;
            }

            public bool IsExact()
            {
                return rect != null
                    && rect.anchoredPosition == anchoredPosition
                    && rect.sizeDelta == sizeDelta
                    && rect.anchorMin == anchorMin
                    && rect.anchorMax == anchorMax
                    && rect.pivot == pivot
                    && rect.localScale == localScale
                    && rect.localRotation == localRotation
                    && rect.parent == parent;
            }
        }

        public static bool LastPassed { get; private set; }
        public static string LastEvidence { get; private set; } =
            string.Empty;

        private string captureDirectory;
        private string beforePath;
        private string shellPath;
        private string hpPath;
        private ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime;
        private LiHuoCombatFeedbackOrchestrationController controller;
        private RectSnapshot backgroundSnapshot;
        private RectSnapshot shougunuSnapshot;
        private float startedAt;
        private float lastEventAt;
        private int lastAcceptedCount;
        private int startSpriteAllocations;
        private int packageWarnings;
        private int packageErrors;
        private bool battleStarted;
        private bool beforeCaptured;
        private bool shellCaptured;
        private bool hpCaptured;
        private bool fixtureCommitted;
        private bool finishing;

        public void Begin(string targetDirectory)
        {
            LastPassed = false;
            LastEvidence = "driver_not_finished";
            captureDirectory = targetDirectory;
            beforePath = Path.Combine(
                captureDirectory,
                "lihuo_before.png");
            shellPath = Path.Combine(
                captureDirectory,
                "lihuo_shell_impact.png");
            hpPath = Path.Combine(
                captureDirectory,
                "lihuo_hp_impact.png");
            DeleteEvidence(beforePath);
            DeleteEvidence(shellPath);
            DeleteEvidence(hpPath);
            startedAt = Time.unscaledTime;
            Application.logMessageReceived += OnLog;
        }

        private void Update()
        {
            if (finishing)
            {
                return;
            }
            if (runtime == null)
            {
                runtime = FindObjectOfType<
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime>();
            }
            if (controller == null)
            {
                controller = FindObjectOfType<
                    LiHuoCombatFeedbackOrchestrationController>();
            }
            if (runtime == null || controller == null)
            {
                if (Time.unscaledTime - startedAt > 8f)
                {
                    Finish(false, "bindings_timeout");
                }
                return;
            }
            if (!controller.Initialized)
            {
                if (Time.unscaledTime - startedAt > 12f)
                {
                    Finish(
                        false,
                        "controller_init_timeout:"
                        + controller.LastDiagnostic);
                }
                return;
            }

            if (!battleStarted)
            {
                if (!fixtureCommitted)
                {
                    ItemSystemBattleSandboxBoardAdapter adapter =
                        Resources.FindObjectsOfTypeAll<
                                ItemSystemBattleSandboxBoardAdapter>()
                            .Where(value => value != null
                                && value.gameObject.scene
                                    == gameObject.scene)
                            .SingleOrDefault();
                    if (adapter?.Authority == null)
                    {
                        return;
                    }
                    if (!TryCommitOfficialP3PlayFixture(
                            adapter.Authority))
                    {
                        return;
                    }
                    fixtureCommitted = true;
                }

                CaptureAuthoredGeometry();
                controller.SetPresentationModes(true, true);
                startSpriteAllocations =
                    controller.LifetimeSpriteAllocationCount;
                runtime.HandleAuthoredBattleStateButton();
                battleStarted = true;
                startedAt = Time.unscaledTime;
                return;
            }

            float runTime = Time.unscaledTime - startedAt;
            if (!beforeCaptured && runTime >= 0.55f)
            {
                ScreenCapture.CaptureScreenshot(beforePath);
                beforeCaptured = true;
            }

            int accepted = controller.AcceptedPresentationCount;
            if (accepted != lastAcceptedCount)
            {
                lastAcceptedCount = accepted;
                lastEventAt = Time.unscaledTime;
            }
            float sinceEvent = Time.unscaledTime - lastEventAt;
            if (accepted > 0
                && sinceEvent >= 0.27f
                && controller.ActiveVisualCount > 3)
            {
                if (!shellCaptured
                    && (controller.LastOutcome
                        == LiHuoCombatFeedbackOutcomeKind.ShellDamage
                        || controller.LastOutcome
                            == LiHuoCombatFeedbackOutcomeKind.ShellBreak))
                {
                    ScreenCapture.CaptureScreenshot(shellPath);
                    shellCaptured = true;
                }
                else if (!hpCaptured
                    && controller.LastOutcome
                        == LiHuoCombatFeedbackOutcomeKind.HpDirectDamage)
                {
                    ScreenCapture.CaptureScreenshot(hpPath);
                    hpCaptured = true;
                }
            }

            bool enoughEvents = accepted >= 24;
            bool evidenceReady =
                beforeCaptured && shellCaptured && hpCaptured;
            bool settled =
                sinceEvent >= 0.94f
                && controller.ActiveVisualCount == 0;
            if (enoughEvents && evidenceReady && settled)
            {
                bool exact =
                    controller.CueGateAcceptedCount == accepted
                    && controller.LifetimeSpriteAllocationCount
                        == startSpriteAllocations
                    && controller.RuntimePoolSize >= 40
                    && controller.PeakActiveVisualCount
                        <= controller.RuntimePoolSize
                    && controller.HasTypedShougunuPresentationFacade
                    && controller.HasRequiredAuthoredTmpBaseline
                    && controller.GeneratedNpPresentationCount > 0
                    && controller.SpentNpPresentationCount >= accepted
                    && controller.AuthoredTmpAcceptedRoleCount
                        >= accepted * 3
                    && !controller.UsesItemArtworkAsProjectile
                    && !controller.HasPerLoopSpriteAllocation
                    && backgroundSnapshot.IsExact()
                    && shougunuSnapshot.IsExact()
                    && packageWarnings == 0
                    && packageErrors == 0;
                Finish(
                    exact,
                    "events=" + accepted
                    + " gate=" + controller.CueGateAcceptedCount
                    + " sprites=" + startSpriteAllocations
                    + " peak=" + controller.PeakActiveVisualCount
                    + "/" + controller.RuntimePoolSize
                    + " npGenerated="
                    + controller.GeneratedNpPresentationCount
                    + " npSpent="
                    + controller.SpentNpPresentationCount
                    + " tmpRoles="
                    + controller.AuthoredTmpAcceptedRoleCount
                    + " warnings=" + packageWarnings
                    + " errors=" + packageErrors
                    + " before=" + beforePath
                    + " shell=" + shellPath
                    + " hp=" + hpPath);
                return;
            }

            if (runTime > 82f)
            {
                Finish(
                    false,
                    "timeout events=" + accepted
                    + " before=" + beforeCaptured
                    + " shell=" + shellCaptured
                    + " hp=" + hpCaptured
                    + " diagnostic=" + controller.LastDiagnostic);
            }
        }

        private bool TryCommitOfficialP3PlayFixture(
            IItemSystemBattleSandboxBoardAuthority authority)
        {
            QaPlacement[] placements =
            {
                new(
                    "I031",
                    new ItemShapeCell(0, 0),
                    ItemShapeRotation.Rotation0),
                new(
                    "I009",
                    new ItemShapeCell(1, 0),
                    ItemShapeRotation.Rotation0),
                new(
                    "I012",
                    new ItemShapeCell(2, 0),
                    ItemShapeRotation.Rotation0),
                new(
                    "I010",
                    new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation90),
                new(
                    "I008",
                    new ItemShapeCell(0, 2),
                    ItemShapeRotation.Rotation0),
                new(
                    "I011",
                    new ItemShapeCell(1, 3),
                    ItemShapeRotation.Rotation0),
                new(
                    "I007",
                    new ItemShapeCell(2, 4),
                    ItemShapeRotation.Rotation0)
            };
            foreach (QaPlacement placement in placements)
            {
                ItemSystemBattleSandboxBoardOperationResult result =
                    authority.CommitFromTray(
                        placement.itemId,
                        placement.cell,
                        placement.rotation);
                if (!result.Accepted)
                {
                    Finish(
                        false,
                        "official_p3_fixture_rejected:"
                        + placement.itemId + ":"
                        + result.DiagnosticCode);
                    return false;
                }
            }
            return true;
        }

        private void CaptureAuthoredGeometry()
        {
            Image background = Resources
                .FindObjectsOfTypeAll<Image>()
                .FirstOrDefault(value => value != null
                    && value.gameObject.scene == gameObject.scene
                    && string.Equals(
                        value.name,
                        "gameBackground",
                        StringComparison.Ordinal));
            Image shougunu = Resources
                .FindObjectsOfTypeAll<Image>()
                .FirstOrDefault(value => value != null
                    && value.gameObject.scene == gameObject.scene
                    && string.Equals(
                        value.name,
                        "Shougunu_1",
                        StringComparison.Ordinal)
                    && value.transform.parent != null
                    && string.Equals(
                        value.transform.parent.name,
                        "V02EnemyArea",
                        StringComparison.Ordinal));
            backgroundSnapshot = new RectSnapshot(
                background?.rectTransform);
            shougunuSnapshot = new RectSnapshot(
                shougunu?.rectTransform);
        }

        private void OnLog(
            string condition,
            string stackTrace,
            LogType type)
        {
            if ((condition ?? string.Empty).IndexOf(
                    LiHuoCombatFeedbackOrchestrationController.PackageId,
                    StringComparison.Ordinal) < 0)
            {
                return;
            }
            if (type == LogType.Warning)
            {
                packageWarnings++;
            }
            else if (type == LogType.Error
                || type == LogType.Exception
                || type == LogType.Assert)
            {
                packageErrors++;
            }
        }

        private void Finish(bool passed, string evidence)
        {
            finishing = true;
            Application.logMessageReceived -= OnLog;
            controller?.SetPresentationModes(true, false);
            LastPassed = passed;
            LastEvidence = evidence ?? string.Empty;
            LiHuoCombatFeedbackOrchestrationVerifier
                .StoreRenderedDriverResult(passed, LastEvidence);
            Debug.Log(
                "[" + LiHuoCombatFeedbackOrchestrationController.PackageId
                + "] RENDERED_DRIVER_FINISH passed=" + passed
                + " " + LastEvidence);
            EditorApplication.isPlaying = false;
        }

        private static void DeleteEvidence(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
        }
    }
}
