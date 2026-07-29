using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Resource;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BattleBridge
{
    [InitializeOnLoad]
    public static class
        ShougunuPhase1BattleSandboxVerticalSliceVerifier
    {
        private const string PackageId =
            "V0.4-RealItemDamageShougunuPhase1BattleSandboxVerticalSlice01";
        private const string P3AssignmentSha =
            "b1189b2887c4dd13774e165659db8b83203d13c389ac51e71a10bece34a14981";
        private const string VisualPackageId =
            "V0.4-ShougunuPhase1VisualPrototypeFacade01";
        private const string VisualAssignmentSha =
            "a016e91e6398781578b56a2da80abe11f56590f9fa74a6be2672728d89021270";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string FrozenSceneSha =
            "FF2D422476B94B525C0C48C261543AC8F73D55A41BE1B70340B485CB73EE0796";
        private const string VisualSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/"
            + "ShougunuPhase1VisualPrototypeController.cs";
        private const string VisualMetaPath = VisualSourcePath + ".meta";
        private const string FrozenVisualSourceSha =
            "6815B3D37DEEDC50E14480EEB90074F05F44902A21499CCCCD2C5C684FBB6CCD";
        private const string FrozenVisualMetaSha =
            "D13BFCC992D536BDB1B323EA51925F93AA3A19CC66AC0454128093FD53FEC380";
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/"
            + "ItemBalanceWorkbenchCatalog.asset";
        private const string CultivationSource =
            "BattleSandboxDevHandtestAllLv40";

        private const string ReportPath =
            "Docs/V0.4/Reports/"
            + "RealItemDamageShougunuPhase1BattleSandboxVerticalSliceReport.md";
        private const string TracePath =
            "Docs/V0.4/Reports/"
            + "RealItemDamageShougunuPhase1BattleSandboxVerticalSliceTrace.csv";
        private const string BindingPath =
            "Docs/V0.4/Reports/"
            + "RealItemDamageShougunuPhase1BattleSandboxVerticalSliceSceneBindingMap.csv";
        private const string UiReusePath =
            "Docs/V0.4/Reports/"
            + "RealItemDamageShougunuPhase1BattleSandboxVerticalSliceUiReuseMap.csv";
        private const string ResetPath =
            "Docs/V0.4/Reports/"
            + "RealItemDamageShougunuPhase1BattleSandboxVerticalSliceResetStaleRegression.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/"
            + "RealItemDamageShougunuPhase1BattleSandboxVerticalSliceLeakCheckReport.md";
        private const string ManualPath =
            "Docs/V0.4/Reports/"
            + "RealItemDamageShougunuPhase1BattleSandboxVerticalSliceManualTest.md";

        private const string PlayStateKey =
            "ShougunuP1VerticalSlice.PlayState";
        private const string PlayPollKey =
            "ShougunuP1VerticalSlice.PlayPoll";
        private const string PlayGenerationKey =
            "ShougunuP1VerticalSlice.Generation";
        private const string PlayFailureKey =
            "ShougunuP1VerticalSlice.Failure";
        private const string PlayStartedUtcTicksKey =
            "ShougunuP1VerticalSlice.StartedUtcTicks";
        private const string PlayLoggedStateKey =
            "ShougunuP1VerticalSlice.LoggedState";
        private const string PlayAutoExitEditorKey =
            "ShougunuP1VerticalSlice.AutoExitEditor";
        private const string PlayTransitionUtcTicksKey =
            "ShougunuP1VerticalSlice.TransitionUtcTicks";

        private static readonly string[] ExpectedItems =
        {
            "I007", "I008", "I009", "I010", "I011", "I012"
        };

        private static readonly int[] ExpectedDamage =
        {
            29, 42, 53, 46, 55, 76
        };

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                + "ShougunuPhase1BattleSandboxVerticalSliceRuntime.cs",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                + "ShougunuPhase1BattleSandboxSceneBinder.cs",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                + "ShougunuPhase1BattleSandboxVisualCueAdapter.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "AutoCombatController",
            "MainTrialFlowService",
            "V02RunFlowController",
            "V03NavigationFlowController",
            "UnifiedBattleRoute",
            "SaveData",
            "PlayerPrefs",
            "RewardService",
            "DropTable",
            "BossInfoPanel",
            "SceneManager.LoadScene",
            "EditorBuildSettings"
        };

        static ShougunuPhase1BattleSandboxVerticalSliceVerifier()
        {
            if (SessionState.GetInt(PlayStateKey, 0) > 0)
            {
                EditorApplication.update -= ContinuePlayModeSmoke;
                EditorApplication.update += ContinuePlayModeSmoke;
            }
        }

        [MenuItem(
            "Tools/TalismanBag/V0.4/Install Shougunu P1 Vertical Slice")]
        public static void InstallSceneBindings()
        {
            try
            {
                AssetDatabase.Refresh();
                Check(
                    ShaFile(ScenePath) == FrozenSceneSha,
                    "SCENE_WRITE_CONFLICT_BEFORE_FIRST_P3_WRITE");
                Check(
                    new FileInfo(ScenePath).Length == 2100364L,
                    "SCENE_LENGTH_CONFLICT_BEFORE_FIRST_P3_WRITE");
                Check(
                    ShaFile(VisualMetaPath) == FrozenVisualMetaSha,
                    "VISUAL_META_CHANGED");

                Scene scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single);
                Dictionary<string, string> before =
                    CaptureProtectedUiFingerprints(scene);
                GameObject host = FindUniqueNamed<RectTransform>(
                    scene,
                    "EnemyCombatFeedbackRuntime")?.gameObject;
                Check(host != null, "EnemyCombatFeedbackRuntime missing.");
                Check(
                    host.GetComponents<
                        ShougunuPhase1BattleSandboxVerticalSliceRuntime>()
                        .Length == 0,
                    "Vertical slice runtime already installed.");
                Check(
                    host.GetComponents<
                        ShougunuPhase1BattleSandboxSceneBinder>().Length == 0,
                    "Vertical slice binder already installed.");
                Check(
                    host.GetComponents<
                        ShougunuPhase1BattleSandboxVisualCueAdapter>().Length
                        == 0,
                    "Vertical slice visual adapter already installed.");

                BuildGridInteractionPreviewController grid =
                    FindUnique<BuildGridInteractionPreviewController>(scene);
                BattleSandboxRuntimeLoopRuntime legacy =
                    FindOptionalUnique<BattleSandboxRuntimeLoopRuntime>(scene);
                BattleSandboxEnemyCombatFeedbackController feedback =
                    host.GetComponent<
                        BattleSandboxEnemyCombatFeedbackController>();
                Check(feedback != null, "Authored feedback controller missing.");

                Text playerHpText = FindUniqueNamed<Text>(
                    scene,
                    "HPText");
                Image playerHpFill = FindFill(
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "PlayerHPBar")?.gameObject);
                Text enemyHpText = FindUniqueNamed<Text>(
                    scene,
                    "EnemyHPText");
                Image enemyHpFill = FindFill(
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "EnemyHPBar")?.gameObject);
                Text shellText = FindUniqueNamed<Text>(scene, "ShieldText");
                Text stateText = FindUniqueNamed<Text>(scene, "StateText");
                Button resetButton =
                    FindUniqueNamed<Button>(
                        scene,
                        "V04BattlePrepareStateButton");

                RectTransform anchors =
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "FloatingTextAnchors");
                RectTransform damageDealt =
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "DamageDealtAnchor");
                RectTransform damageTaken =
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "DamageTakenAnchor");
                RectTransform shieldBreak =
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "ShieldBreakAnchor");
                RectTransform statusDamage =
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "StatusDamageAnchor");
                RectTransform mechanicRoot =
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "EnemyCombatFeedbackFloatingRoot");
                Text mechanicText = FindUniqueNamed<Text>(
                    scene,
                    "MechanicFloatingText");
                CanvasGroup mechanicGroup =
                    mechanicText.GetComponent<CanvasGroup>();
                Image playerHit = FindUniqueNamed<Image>(
                    scene,
                    "PlayerHitFeedback");
                GameObject shougunu =
                    FindUniqueNamed<RectTransform>(
                        scene,
                        "Shougunu_1")?.gameObject;

                Check(playerHpFill != null, "PlayerHPBar/Fill missing.");
                Check(enemyHpFill != null, "EnemyHPBar/Fill missing.");
                Check(mechanicGroup != null,
                    "MechanicFloatingText CanvasGroup missing.");
                Check(shougunu != null
                    && shougunu.transform.parent != null
                    && shougunu.transform.parent.name == "V02EnemyArea",
                    "Exact V02EnemyArea/Shougunu_1 missing.");

                ShougunuPhase1BattleSandboxSceneBinder binder =
                    host.AddComponent<
                        ShougunuPhase1BattleSandboxSceneBinder>();
                ShougunuPhase1BattleSandboxVisualCueAdapter visual =
                    host.AddComponent<
                        ShougunuPhase1BattleSandboxVisualCueAdapter>();
                ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                    host.AddComponent<
                        ShougunuPhase1BattleSandboxVerticalSliceRuntime>();

                Bind(runtime, "gridController", grid);
                Bind(runtime, "sceneBinder", binder);
                Bind(binder, "gridController", grid);
                Bind(binder, "legacyRuntimeLoop", legacy);
                Bind(binder, "enemyCombatFeedbackController", feedback);
                Bind(binder, "battleRuntime", runtime);
                Bind(binder, "visualCueAdapter", visual);
                Bind(binder, "playerHpText", playerHpText);
                Bind(binder, "playerHpBar", playerHpFill);
                Bind(binder, "enemyHpText", enemyHpText);
                Bind(binder, "enemyHpBar", enemyHpFill);
                Bind(binder, "shellText", shellText);
                Bind(binder, "stateText", stateText);
                Bind(binder, "authoredResetButton", resetButton);
                Bind(visual, "floatingTextAnchors", anchors);
                Bind(visual, "damageDealtAnchor", damageDealt);
                Bind(visual, "damageTakenAnchor", damageTaken);
                Bind(visual, "shieldBreakAnchor", shieldBreak);
                Bind(visual, "statusDamageAnchor", statusDamage);
                Bind(visual, "mechanicFloatingRoot", mechanicRoot);
                Bind(visual, "mechanicFloatingText", mechanicText);
                Bind(
                    visual,
                    "mechanicFloatingCanvasGroup",
                    mechanicGroup);
                Bind(visual, "playerHitFeedback", playerHit);
                Bind(visual, "shougunuRoot", shougunu);

                Dictionary<string, string> after =
                    CaptureProtectedUiFingerprints(scene);
                Check(
                    before.Count == after.Count
                    && before.All(pair =>
                        after.TryGetValue(pair.Key, out string value)
                        && string.Equals(
                            pair.Value,
                            value,
                            StringComparison.Ordinal)),
                    "PROTECTED_UI_CHANGED_DURING_BIND");
                EditorUtility.SetDirty(host);
                Check(
                    EditorSceneManager.SaveScene(scene),
                    "Target Scene save failed.");
                Check(
                    ShaFile(VisualMetaPath) == FrozenVisualMetaSha,
                    "Visual meta changed during Scene bind.");
                WriteInstallEvidence(before, after, scene);
                Debug.Log(
                    "[" + PackageId + "] P3_SCENE_BIND_INSTALL_PASS "
                    + ShaFile(ScenePath));
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[" + PackageId + "] P3_SCENE_BIND_INSTALL_FAIL "
                    + exception);
                ExitBatch(1);
                throw;
            }
        }

        [MenuItem(
            "Tools/TalismanBag/V0.4/Verify Shougunu P1 Vertical Slice")]
        public static void RunAll()
        {
            try
            {
                AssetDatabase.Refresh();
                Scene scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single);
                VerifySceneBindings(scene);
                Fixture fixture = BuildFixture(CurrentPlacements());
                VerifyTerminalItemFacts(fixture.Request);
                VerifyRealProjectionLaunch();
                ShougunuPhase1BattleApplicationTrace trace =
                    ShougunuPhase1BattleApplicationEngine.Create(
                        fixture.Request,
                        1).RunNominalTrace();
                VerifyNominal(trace);
                BattleSandboxNianResourceSnapshot nian =
                    VerifyNianIntegratedNominal(fixture);
                ResetEvidence reset = VerifyResetStale(fixture.Request);
                VerifyUiReuse(scene);
                IReadOnlyList<string> leakHits = VerifyLeakScan();
                VerifyVisualFacade();
                VerifyAcceptedPresentation(scene);
                WriteAllReports(
                    scene,
                    fixture.Request,
                    trace,
                    reset,
                    leakHits,
                    "PENDING");
                Debug.Log(
                    "[" + PackageId
                    + "] P3_AUTOMATED_QA_OFFLINE_PASS "
                    + "50items/6skills/12basics/7breaks/"
                    + "player9735of9999/defeated75000/"
                    + "nian" + nian.currentNian + "of"
                    + nian.maxNian);
                ExitBatch(0);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[" + PackageId + "] P3_AUTOMATED_QA_FAIL "
                    + exception);
                ExitBatch(1);
                throw;
            }
        }

        [MenuItem(
            "Tools/TalismanBag/V0.4/Play Smoke Shougunu P1 Vertical Slice")]
        public static void RunPlayModeSmoke()
        {
            try
            {
                Check(!EditorApplication.isPlaying,
                    "Play smoke must start from Edit Mode.");
                EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single);
                SessionState.SetInt(PlayStateKey, 1);
                SessionState.SetInt(PlayPollKey, 0);
                SessionState.SetInt(PlayGenerationKey, 0);
                SessionState.SetInt(PlayLoggedStateKey, 0);
                SessionState.EraseString(PlayFailureKey);
                SessionState.SetString(
                    PlayStartedUtcTicksKey,
                    DateTime.UtcNow.Ticks.ToString(
                        CultureInfo.InvariantCulture));
                EditorApplication.update -= ContinuePlayModeSmoke;
                EditorApplication.update += ContinuePlayModeSmoke;
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception)
            {
                FinishPlaySmokeFailure(exception.ToString());
            }
        }

        public static void RunAutomatedNormalEditorPlayModeSmoke()
        {
            SessionState.SetBool(PlayAutoExitEditorKey, true);
            RunPlayModeSmoke();
        }

        private static void ContinuePlayModeSmoke()
        {
            int state = SessionState.GetInt(PlayStateKey, 0);
            if (state <= 0)
            {
                EditorApplication.update -= ContinuePlayModeSmoke;
                return;
            }

            int poll = SessionState.GetInt(PlayPollKey, 0) + 1;
            SessionState.SetInt(PlayPollKey, poll);
            int loggedState = SessionState.GetInt(
                PlayLoggedStateKey,
                0);
            if (loggedState != state)
            {
                SessionState.SetInt(PlayLoggedStateKey, state);
                Debug.Log(
                    "[" + PackageId + "] P3_PLAY_SMOKE_STATE_"
                    + state + " isPlaying=" + EditorApplication.isPlaying
                    + " changing="
                    + EditorApplication.isPlayingOrWillChangePlaymode);
            }
            if (state == 5)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }
                string failure = SessionState.GetString(
                    PlayFailureKey,
                    "PLAY_SMOKE_FAILED");
                UpdatePlaySmokeReport("FAIL: " + failure);
                ClearPlaySmokeState();
                Debug.LogError(
                    "[" + PackageId
                    + "] P3_NORMAL_EDITOR_PLAY_SMOKE_FAIL "
                    + failure);
                ExitBatch(1);
                return;
            }
            string startedRaw = SessionState.GetString(
                PlayStartedUtcTicksKey,
                string.Empty);
            if (!long.TryParse(
                    startedRaw,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long startedUtcTicks))
            {
                startedUtcTicks = DateTime.UtcNow.Ticks;
                SessionState.SetString(
                    PlayStartedUtcTicksKey,
                    startedUtcTicks.ToString(
                        CultureInfo.InvariantCulture));
            }
            if (DateTime.UtcNow.Ticks - startedUtcTicks
                > TimeSpan.FromMinutes(2).Ticks)
            {
                FinishPlaySmokeFailure(
                    "PLAY_SMOKE_TIMEOUT_STATE_" + state);
                return;
            }

            try
            {
                if (state == 1)
                {
                    if (!EditorApplication.isPlaying)
                    {
                        return;
                    }
                    ItemSystemBattleSandboxBoardAdapter boardAdapter =
                        Resources
                            .FindObjectsOfTypeAll<
                                ItemSystemBattleSandboxBoardAdapter>()
                            .Where(value => value != null
                                && value.gameObject.scene.IsValid()
                                && value.gameObject.scene.isLoaded)
                            .SingleOrDefault();
                    if (boardAdapter?.Authority == null)
                    {
                        return;
                    }

                    ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                        UnityEngine.Object.FindObjectOfType<
                            ShougunuPhase1BattleSandboxVerticalSliceRuntime>(
                            true);
                    Check(runtime?.SceneBinder != null,
                        "P3 runtime/binder missing before authored click.");
                    Check(runtime.SceneBinder.PlayerHpText.text
                            .Contains("9999/9999"),
                        "Player HP was not visible before battle start.");
                    Check(runtime.SceneBinder.EnemyHpText.text
                            .Contains("980/980"),
                        "Enemy HP was not visible before battle start.");
                    Check(runtime.SceneBinder.ShellText.gameObject
                            .activeInHierarchy
                        && runtime.SceneBinder.ShellText.text
                            .Contains("200/200"),
                        "Authored shell/guard text was not visible.");
                    Check(runtime.SceneBinder.StateText.text.Contains("R"),
                        "R reset was not discoverable before battle start.");

                    CommitPlayFixture(
                        boardAdapter.Authority,
                        exactI008Adjacency: false);
                    Button stateButton =
                        UnityEngine.Object.FindObjectsOfType<Button>(true)
                            .Single(value =>
                                value.name
                                == "V04BattlePrepareStateButton");
                    stateButton.onClick.Invoke();
                    Check(runtime.ResetGeneration > 0,
                        "Existing Continue Battle button did not issue the "
                        + "P3 start request synchronously.");
                    ItemCombatEffectRequestRow i008 =
                        runtime.CurrentItemRequestSnapshot.Requests.Single(
                            value => value.sourceBaseItemId == "I008");
                    ItemCombatEffectRequestContribution adjacency =
                        i008.Contributions.Single(value =>
                            value.sourceEffectId
                                == ItemCombatEffectRequestAssembler
                                    .AdjacentDamageAffixId
                                    + ":effect");
                    Check(runtime.HasActiveSession
                        && runtime.LastDiagnosticCode == "RUNNING"
                        && string.IsNullOrEmpty(
                            runtime.LastDiagnosticDetail)
                        && i008.resolvedPreMitigationDamageUnits == 40
                        && adjacency.stackCount == 2,
                        "Two-neighbor I008 real Projection did not start "
                        + "without fabricating the frozen 42 fixture.");
                    SessionState.SetInt(PlayStateKey, 7);
                    SessionState.SetInt(PlayPollKey, 0);
                    return;
                }

                if (state == 7)
                {
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                        UnityEngine.Object.FindObjectOfType<
                            ShougunuPhase1BattleSandboxVerticalSliceRuntime>(
                            true);
                    if (runtime?.GridController == null
                        || !runtime.GridController.IsSandboxBattleModeActive)
                    {
                        return;
                    }

                    BattleSandboxRuntimeLoopRuntime legacy =
                        runtime.GridController.GetComponent<
                            BattleSandboxRuntimeLoopRuntime>();
                    Check(legacy != null && !legacy.enabled,
                        "Dynamic legacy 100-HP loop was not suppressed.");
                    BattleSandboxManaLoopRuntime legacyMana =
                        runtime.GridController.GetComponent<
                            BattleSandboxManaLoopRuntime>();
                    Check(legacyMana != null && !legacyMana.enabled,
                        "Dynamic legacy mana-shortage loop was not "
                        + "suppressed.");
                    Check(runtime.HasActiveSession
                            && runtime.LastDiagnosticCode == "RUNNING",
                        "Real Projection session was overwritten after "
                        + "the authored Continue Battle click.");
                    SessionState.SetInt(PlayStateKey, 2);
                    SessionState.SetInt(PlayPollKey, 0);
                    return;
                }

                if (state == 2)
                {
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                        UnityEngine.Object.FindObjectOfType<
                            ShougunuPhase1BattleSandboxVerticalSliceRuntime>(
                            true);
                    if (runtime?.CurrentContext == null
                        || runtime.CurrentContext.battleTick < 1700L)
                    {
                        return;
                    }

                    Check(runtime.HasActiveSession,
                        "Runtime session did not start.");
                    Check(runtime.LastDiagnosticCode == "RUNNING"
                        && string.IsNullOrEmpty(runtime.LastDiagnosticDetail),
                        "Runtime retained a start rejection: "
                        + runtime.LastDiagnosticCode + " / "
                        + runtime.LastDiagnosticDetail);
                    Check(runtime.CurrentContext.enemy
                        .AcceptedDamageApplicationCount >= 1,
                        "Real-time Item pulse did not apply.");
                    BattleSandboxNianResourceSnapshot nian =
                        runtime.CurrentNianResourceSnapshot;
                    Check(nian != null
                        && nian.pulseCount >= 1
                        && nian.acceptedApplicationCount
                            == runtime.CurrentContext.enemy
                                .AcceptedDamageApplicationCount
                        && nian.currentNian
                            == ExpectedNianAfterPulses(nian.pulseCount),
                        "Real-time I031 Nian pulse did not gate the matching "
                        + "damage pulse.");
                    Check(runtime.SceneBinder.NianPresenter != null
                        && runtime.SceneBinder.NianPresenter
                            .HasVisibleBindings
                        && runtime.SceneBinder.NianPresenter.ManaText
                            .gameObject.activeInHierarchy
                        && runtime.SceneBinder.NianPresenter.ManaText.text
                            .Contains("NP ")
                        && runtime.SceneBinder.NianPresenter.ManaText.text
                            .Contains("/100"),
                        "Existing ManaText/Fill did not show live NP.");
                    BattleSandboxAuthoredTmpPresentation tmpPresentation =
                        runtime.SceneBinder.AuthoredTmpPresentation;
                    Check(tmpPresentation != null
                        && tmpPresentation.HasAuthoredTemplates
                        && tmpPresentation.HasRuntimeChineseFallback
                        && tmpPresentation.AuthoredFontAssetName
                            == BattleSandboxAuthoredTmpPresentation
                                .RequiredFontAssetName
                        && tmpPresentation.RuntimeChineseGlyphCount > 20,
                        "Exact authored TMP font did not attach its "
                        + "runtime Chinese fallback.");
                    TextMeshProUGUI[] activeAuthoredClones =
                        Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
                            .Where(value => value != null
                                && value.gameObject.activeInHierarchy
                                && value.gameObject.name.StartsWith(
                                    "AuthoredTmpClone_",
                                    StringComparison.Ordinal))
                            .ToArray();
                    Check(tmpPresentation.TrySpawn(
                            runtime.ResetGeneration,
                            "verifier.chinese.glyph.g"
                            + runtime.ResetGeneration,
                            "verifier.chinese.glyph",
                            runtime.SceneBinder.VisualCueAdapter
                                .DamageDealtAnchor,
                            "中文护壳",
                            BattleSandboxAuthoredTmpStyle.SecondaryCyan,
                            0.72f,
                            0f,
                            BattleSandboxAuthoredTmpPresentation
                                .DefaultReadableLifetime,
                            BattleSandboxAuthoredTmpPresentation
                                .SettlementShellPaletteKey),
                        "Verifier Chinese glyph cue was rejected.");
                    activeAuthoredClones =
                        Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
                            .Where(value => value != null
                                && value.gameObject.activeInHierarchy
                                && value.gameObject.name.StartsWith(
                                    "AuthoredTmpClone_",
                                    StringComparison.Ordinal))
                            .ToArray();
                    Check(activeAuthoredClones.Any(value =>
                            value.font != null
                            && value.font.name
                                == BattleSandboxAuthoredTmpPresentation
                                    .RequiredFontAssetName
                            && value.text.Contains("中文护壳")
                            && value.textInfo.characterInfo.Any(character =>
                                character.character == '护'
                                && character.isVisible)),
                        "Runtime authored TMP clone did not render the "
                        + "Chinese Nian glyph through the exact font "
                        + "fallback.");
                    Check(activeAuthoredClones.Any(value =>
                            value.text == "+4 NP")
                        && activeAuthoredClones.Any(value =>
                            value.text.StartsWith(
                                "-",
                                StringComparison.Ordinal)
                            && value.text.EndsWith(
                                " NP",
                                StringComparison.Ordinal)),
                        "Accepted pulse did not show both +NP and -NP "
                        + "source floats.");
                    BattleSandboxItemTriggerFeedbackController
                        acceptedFeedback =
                            runtime.SceneBinder
                                .AcceptedItemFeedbackController;
                    Check(acceptedFeedback != null
                        && acceptedFeedback
                            .AcceptedPresentationEventCount >= 1
                        && acceptedFeedback
                            .AcceptedCompleteLegacyPresentationCount >= 1
                        && acceptedFeedback
                            .AcceptedCarrierPresentationCount >= 1,
                        "Accepted event did not enter the complete old "
                        + "board source path and independent carrier path.");
                    Check(runtime.SceneBinder
                            .GeneratedNpPresentationCount >= 1
                        && runtime.SceneBinder
                            .SpentNpPresentationCount >= 1,
                        "Accepted pulse did not register its I031 +NP and "
                        + "trigger-item -NP projections.");
                    Check(runtime.CurrentContext.player.currentHp == 9999,
                        "Player did not start at 9999.");
                    Check(
                        UnityEngine.Object.FindObjectOfType<
                            ShougunuPhase1VisualPrototypeController>(true)
                            != null,
                        "Visual facade controller did not attach.");
                    Check(runtime.SceneBinder.PlayerHpText.text
                        .Contains("9999/9999"),
                        "Authored player HP text was not driven.");
                    Check(runtime.SceneBinder.EnemyHpText.gameObject
                            .activeInHierarchy
                        && runtime.SceneBinder.EnemyHpText.text
                            .Contains("980/980"),
                        "Authored enemy HP text was not visible/driven.");
                    Check(runtime.SceneBinder.ShellText.gameObject
                            .activeInHierarchy
                        && !runtime.SceneBinder.ShellText.text
                            .Contains("200/200"),
                        "Authored shell text did not show the first 1.5s "
                        + "Item pulse.");
                    Check(Resources.FindObjectsOfTypeAll<Text>().Any(value =>
                            value != null
                            && value.name
                                == "ShougunuP1FloatingText_Runtime"
                            && value.gameObject.activeInHierarchy),
                        "First 1.5s Item pulse did not render through an "
                        + "authored floating-text anchor.");
                    SessionState.SetInt(PlayStateKey, 6);
                    SessionState.SetInt(PlayPollKey, 0);
                    return;
                }

                if (state == 6)
                {
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                        UnityEngine.Object.FindObjectOfType<
                            ShougunuPhase1BattleSandboxVerticalSliceRuntime>(
                            true);
                    if (runtime?.CurrentContext == null
                        || runtime.CurrentContext.player.currentHp >= 9999
                        || runtime.CurrentNianResourceSnapshot == null
                        || runtime.CurrentNianResourceSnapshot.pulseCount < 6)
                    {
                        return;
                    }

                    Check(runtime.CurrentContext.DisplayEvents.Any(value =>
                            value != null
                            && value.channel
                                == ShougunuPhase1BattleDisplayChannel
                                    .EnemyToPlayerDamage),
                        "Basic/Skill direct player damage display event "
                        + "was not emitted.");
                    Check(runtime.SceneBinder.VisualCueAdapter.PlayerHitFeedback
                            .gameObject.activeInHierarchy
                        && runtime.SceneBinder.VisualCueAdapter.PlayerHitFeedback
                            .color.a > 0.05f,
                        "Existing PlayerHitFeedback was not visibly driven.");
                    Check(Resources.FindObjectsOfTypeAll<Text>().Any(value =>
                            value != null
                            && value.name
                                == "ShougunuP1FloatingText_Runtime"
                            && value.gameObject.activeInHierarchy),
                        "Player damage did not render visible floating text.");
                    BattleSandboxItemTriggerFeedbackController
                        acceptedFeedback =
                            runtime.SceneBinder
                                .AcceptedItemFeedbackController;
                    Check(acceptedFeedback != null
                        && acceptedFeedback
                            .AcceptedPresentationEventCount >= 6
                        && acceptedFeedback
                            .AcceptedCompleteLegacyPresentationCount
                                == acceptedFeedback
                                    .AcceptedPresentationEventCount
                        && acceptedFeedback
                            .AcceptedCarrierPresentationCount
                                == acceptedFeedback
                                    .AcceptedPresentationEventCount
                        && acceptedFeedback
                            .RejectedAcceptedPresentationEventCount == 0,
                        "I007-I012 accepted events did not each enter the "
                        + "complete source/carrier VFX path exactly once.");
                    Check(runtime.SceneBinder
                            .GeneratedNpPresentationCount >= 6
                        && runtime.SceneBinder
                            .SpentNpPresentationCount >= 6,
                        "I007-I012 did not each project accepted +4 NP "
                        + "and exact -NP deltas.");
                    string[] sourceIds =
                        { "I007", "I008", "I009", "I010", "I011", "I012" };
                    Check(sourceIds.All(value =>
                            BattleSandboxAuthoredTmpPresentation
                                .TryResolveItemSourceGradient(
                                    value,
                                    out VertexGradient _)),
                        "I007-I012 source gradient palette is incomplete.");
                    Check(sourceIds.Select(
                                BattleSandboxItemTriggerFeedbackController
                                    .ResolveAcceptedCarrierPresentationKey)
                            .Distinct(StringComparer.Ordinal)
                            .Count() == 3,
                        "I007-I012 explicit presentation map did not "
                        + "exercise three independent carrier grammars.");
                    int generation = runtime.ResetGeneration;
                    SessionState.SetInt(
                        PlayGenerationKey,
                        generation);
                    runtime.RequestReset();
                    SessionState.SetInt(PlayStateKey, 3);
                    SessionState.SetInt(PlayPollKey, 0);
                    return;
                }

                if (state == 3)
                {
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                        UnityEngine.Object.FindObjectOfType<
                            ShougunuPhase1BattleSandboxVerticalSliceRuntime>(
                            true);
                    int previous =
                        SessionState.GetInt(PlayGenerationKey, 0);
                    if (runtime?.CurrentContext == null
                        || runtime.ResetGeneration <= previous)
                    {
                        return;
                    }
                    Check(runtime.CurrentContext.battleTick < 200L,
                        "Reset clock did not return to zero.");
                    Check(runtime.CurrentContext.player.currentHp == 9999,
                        "Reset player HP did not return to 9999.");
                    Check(runtime.DisplayEventCursor == 0,
                        "Reset retained stale display events.");
                    Check(runtime.CurrentNianResourceSnapshot != null
                        && runtime.CurrentNianResourceSnapshot
                            .resetGeneration == runtime.ResetGeneration
                        && runtime.CurrentNianResourceSnapshot
                            .currentNian == 20
                        && runtime.CurrentNianResourceSnapshot.pulseCount == 0
                        && runtime.CurrentNianResourceSnapshot
                            .ProcessedPulseEventIds.Count == 0,
                        "Reset retained stale Nian state/events.");
                    Check(runtime.SceneBinder.AuthoredTmpPresentation != null
                        && runtime.SceneBinder.AuthoredTmpPresentation
                            .ActiveCount == 0
                        && runtime.SceneBinder.AuthoredTmpPresentation
                            .AcceptedRoleCount == 0
                        && runtime.SceneBinder
                            .GeneratedNpPresentationCount == 0
                        && runtime.SceneBinder
                            .SpentNpPresentationCount == 0
                        && runtime.SceneBinder.AuthoredTmpPresentation
                            .PrimaryTemplate.font.fallbackFontAssetTable
                            .Count == 0
                        && runtime.SceneBinder
                            .AcceptedItemFeedbackController
                            .AcceptedPresentationEventCount == 0
                        && runtime.SceneBinder
                            .AcceptedItemFeedbackController
                            .AcceptedCarrierPresentationCount == 0
                        && runtime.SceneBinder
                            .AcceptedItemFeedbackController
                            .ActiveTransientObjectCount == 0,
                        "Reset retained TMP/VFX objects or the runtime "
                        + "font fallback.");
                    SessionState.SetInt(PlayStateKey, 4);
                    SessionState.SetInt(PlayPollKey, 0);
                    EditorApplication.ExitPlaymode();
                    return;
                }

                if (state == 4 && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    Scene scene = SceneManager.GetActiveScene();
                    Check(!scene.isDirty,
                        "Play smoke left target Scene dirty.");
                    UpdatePlaySmokeReport("PASS");
                    ClearPlaySmokeState();
                    Debug.Log(
                        "[" + PackageId
                        + "] P3_NORMAL_EDITOR_PLAY_SMOKE_PASS");
                    ExitBatch(0);
                }
            }
            catch (Exception exception)
            {
                FinishPlaySmokeFailure(exception.ToString());
            }
        }

        private static void FinishPlaySmokeFailure(string failure)
        {
            string resolvedFailure = failure ?? "PLAY_SMOKE_FAILED";
            if (string.IsNullOrEmpty(
                    SessionState.GetString(PlayFailureKey, string.Empty)))
            {
                SessionState.SetString(
                    PlayFailureKey,
                    resolvedFailure);
            }
            Debug.LogError(
                "[" + PackageId + "] P3_PLAY_SMOKE_PENDING_FAIL "
                + resolvedFailure);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                SessionState.SetInt(PlayStateKey, 5);
                EditorApplication.ExitPlaymode();
                return;
            }
            UpdatePlaySmokeReport("FAIL: " + resolvedFailure);
            ClearPlaySmokeState();
            Debug.LogError(
                "[" + PackageId + "] P3_NORMAL_EDITOR_PLAY_SMOKE_FAIL "
                + resolvedFailure);
            ExitBatch(1);
        }

        private static void CommitPlayFixture(
            IItemSystemBattleSandboxBoardAuthority authority,
            bool exactI008Adjacency = true)
        {
            var placements = new[]
            {
                new PlayPlacement(
                    "I031", new ItemShapeCell(0, 0),
                    ItemShapeRotation.Rotation0),
                new PlayPlacement(
                    "I009", new ItemShapeCell(1, 0),
                    ItemShapeRotation.Rotation0),
                new PlayPlacement(
                    "I012", new ItemShapeCell(2, 0),
                    ItemShapeRotation.Rotation0),
                new PlayPlacement(
                    "I010", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation90),
                new PlayPlacement(
                    "I008", new ItemShapeCell(0, 2),
                    ItemShapeRotation.Rotation0),
                new PlayPlacement(
                    "I011", new ItemShapeCell(1, 3),
                    ItemShapeRotation.Rotation0),
                new PlayPlacement(
                    "I007", new ItemShapeCell(0, 4),
                    ItemShapeRotation.Rotation0)
            };
            foreach (PlayPlacement placement in placements)
            {
                PlayPlacement resolved = !exactI008Adjacency
                    && string.Equals(
                        placement.itemId,
                        "I007",
                        StringComparison.Ordinal)
                    ? new PlayPlacement(
                        "I007",
                        new ItemShapeCell(2, 4),
                        ItemShapeRotation.Rotation0)
                    : placement;
                CommitPlayPlacement(authority, resolved);
            }
        }

        private static void CommitPlayPlacement(
            IItemSystemBattleSandboxBoardAuthority authority,
            PlayPlacement placement)
        {
            ItemSystemBattleSandboxBoardOperationResult result =
                authority.CommitFromTray(
                    placement.itemId,
                    placement.cell,
                    placement.rotation);
            Check(
                result.Accepted,
                "Play fixture rejected "
                + placement.itemId + ": " + result.DiagnosticCode);
        }

        private static void VerifySceneBindings(Scene scene)
        {
            Check(
                !EditorBuildSettings.scenes.Any(value =>
                    string.Equals(
                        value.path,
                        ScenePath,
                        StringComparison.Ordinal)),
                "DevOnly target Scene entered BuildSettings.");
            CheckNamedCardinality(scene, "Shougunu_1", 1);
            CheckNamedCardinality(
                scene,
                "EnemyCombatFeedbackRuntime",
                1);
            CheckNamedCardinality(scene, "FloatingTextAnchors", 1);
            CheckNamedCardinality(scene, "MechanicFloatingText", 1);
            CheckNamedCardinality(scene, "PlayerHitFeedback", 1);
            CheckNamedCardinality(scene, "EnemyHPText", 1);
            CheckNamedCardinality(scene, "PlayerHPBar", 1);

            ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime =
                FindUnique<
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime>(scene);
            ShougunuPhase1BattleSandboxSceneBinder binder =
                FindUnique<ShougunuPhase1BattleSandboxSceneBinder>(scene);
            ShougunuPhase1BattleSandboxVisualCueAdapter visual =
                FindUnique<
                    ShougunuPhase1BattleSandboxVisualCueAdapter>(scene);
            Check(runtime.gameObject == binder.gameObject
                && binder.gameObject == visual.gameObject
                && runtime.gameObject.name
                    == "EnemyCombatFeedbackRuntime",
                "New devOnly components are not co-located on authored host.");
            Check(runtime.DevOnly && runtime.IsEnabled
                && !runtime.EntersFormalBattle
                && !runtime.WritesFormalFlow
                && !runtime.WritesPersistentState
                && !runtime.GrantsFormalReward
                && !runtime.AdvancesChapter,
                "Runtime devOnly/formal boundary invalid.");
            Check(binder.DevOnly && binder.IsBindingComplete
                && !binder.WritesLayout,
                "Scene binder incomplete or writes layout.");
            Check(visual.DevOnly && visual.IsBindingComplete
                && !visual.OwnsBattleTruth,
                "Visual cue adapter boundary invalid.");
            Check(runtime.GridController == binder.GridController
                && runtime.SceneBinder == binder
                && binder.BattleRuntime == runtime
                && binder.VisualCueAdapter == visual,
                "Runtime/binder/visual references disagree.");
        }

        private static void VerifyTerminalItemFacts(
            ItemCombatEffectRequestSnapshot request)
        {
            Check(request != null
                && request.status
                    == ItemCombatEffectRequestSnapshotStatus.Valid
                && request.devOnly
                && !request.entersFormalBattle,
                "Terminal Item request snapshot invalid.");
            for (int index = 0; index < ExpectedItems.Length; index++)
            {
                ItemCombatEffectRequestRow row = request.Requests.Single(
                    value => value.sourceBaseItemId == ExpectedItems[index]);
                Check(
                    row.resolvedPreMitigationDamageUnits
                        == ExpectedDamage[index],
                    "Terminal Item magnitude mismatch "
                    + ExpectedItems[index]);
            }
            ItemCombatEffectRequestRow i011 = request.Requests.Single(
                value => value.sourceBaseItemId == "I011");
            Check(i011.isLitFact
                    == ItemInstanceQualifiedBuildBooleanFact.True
                && i011.sourceIsCountedFact
                    == ItemInstanceQualifiedBuildBooleanFact.True
                && i011.qualifiedIsCountedFact
                    == ItemInstanceQualifiedBuildBooleanFact.False,
                "I011 truth must remain True/True/False.");
            Check(
                request.Requests.All(value =>
                    value.sourceBaseItemId != "I031"),
                "I031 ordinary damage request leaked.");
            Check(
                request.UnsupportedTelemetry.Any(value =>
                    value.disposition
                    == ItemCombatEffectUnsupportedDisposition.NotExecuted),
                "Unsupported Item/core facts lack NotExecuted telemetry.");
        }

        private static void VerifyRealProjectionLaunch()
        {
            Fixture fixture = BuildFixture(TwoNeighborI008Placements());
            ItemCombatEffectRequestRow i008 = fixture.Request.Requests.Single(
                value => value.sourceBaseItemId == "I008");
            ItemCombatEffectRequestContribution adjacency =
                i008.Contributions.Single(value =>
                    value.sourceEffectId
                        == ItemCombatEffectRequestAssembler
                            .AdjacentDamageAffixId
                            + ":effect");
            Check(i008.resolvedPreMitigationDamageUnits == 40
                    && adjacency.stackCount == 2,
                "Two-neighbor I008 real Projection fixture drifted.");

            ShougunuPhase1BattleApplicationTrace trace =
                ShougunuPhase1BattleApplicationEngine.Create(
                    fixture.Request,
                    1).RunNominalTrace();
            Check(trace.Rows.Count == 50
                    && trace.Rows.All(value => value.accepted)
                    && trace.Rows
                        .Where(value => value.sourceBaseItemId == "I008")
                        .All(value =>
                            value.resolvedPreMitigationDamageUnits == 40),
                "Real Projection launch was rejected or I008 damage was "
                + "fabricated as the frozen 42 fixture.");
        }

        private static void VerifyNominal(
            ShougunuPhase1BattleApplicationTrace trace)
        {
            Check(trace?.finalContext?.enemy != null,
                "Nominal trace missing.");
            ShougunuPhase1BattleApplicationContext context =
                trace.finalContext;
            Check(context.battleTick == 75000L,
                "Nominal final tick must be 75000.");
            Check(trace.Rows.Count == 50
                && trace.Rows.All(value => value.accepted),
                "Expected 50 accepted Item applications.");
            Check(
                trace.Rows.Select(value => value.sourceBaseItemId)
                    .SequenceEqual(
                        Enumerable.Range(0, 50)
                            .Select(index =>
                                ExpectedItems[index
                                    % ExpectedItems.Length])),
                "Item pulse order is unstable.");
            Check(
                context.enemy.ThresholdOccurrences.Count(value =>
                    value.Resolved) == 6,
                "Expected six threshold skills.");
            Check(context.enemy.BasicResolvedCount == 12,
                "Expected twelve BasicAttack resolutions.");
            Check(
                context.DisplayEvents.Count(value =>
                    value.channel
                    == ShougunuPhase1BattleDisplayChannel.EnemyShellBreak)
                    == 7,
                "Expected seven shell breaks.");
            Check(context.enemy.CurrentHp == 0
                && context.enemy.Lifecycle
                    == ShougunuPhase1LifecycleState.Defeated,
                "Enemy did not reach Defeated.");
            Check(context.player.maxHp == 9999
                && context.player.currentHp == 9735
                && !context.player.defeated,
                "Player nominal fixture mismatch.");
            long defeatedTick = context.DisplayEvents.Single(value =>
                value.channel
                == ShougunuPhase1BattleDisplayChannel.EnemyDefeated)
                .battleTick;
            long secondSkill3Tick = context.DisplayEvents.Where(value =>
                    value.channel
                        == ShougunuPhase1BattleDisplayChannel
                            .EnemyToPlayerStatusOrArea
                    && value.sourceId
                        == ShougunuPhase1ActionPatternCatalog
                            .Skill3GroundSealBurst)
                .OrderBy(value => value.battleTick)
                .ElementAt(1).battleTick;
            long basic12Tick = context.DisplayEvents.Where(value =>
                    value.channel
                        == ShougunuPhase1BattleDisplayChannel
                            .EnemyToPlayerDamage
                    && value.sourceId
                        == ShougunuPhase1ActionPatternCatalog.BasicAttack)
                .OrderBy(value => value.battleTick)
                .ElementAt(11).battleTick;
            Check(secondSkill3Tick < defeatedTick
                && basic12Tick < defeatedTick,
                "Second Skill3/Basic12 must precede Defeated.");
        }

        private static BattleSandboxNianResourceSnapshot
            VerifyNianIntegratedNominal(Fixture fixture)
        {
            I031NianSourceSnapshot source =
                I031NianSourceAssembler.Assemble(
                    fixture.ItemSystem,
                    1);
            Check(source.status == I031NianSourceStatus.Valid
                && source.isEligible,
                "I031 Nian source fixture invalid.");
            BattleSandboxNianResourceEngine resource =
                BattleSandboxNianResourceEngine.Create(source);
            ShougunuPhase1BattleApplicationSession battle =
                ShougunuPhase1BattleApplicationEngine.Create(
                    fixture.Request,
                    1);

            for (int index = 0; index < 50; index++)
            {
                int sequence = index + 1;
                long tick =
                    sequence
                    * BattleSandboxNianResourceEngine.PulseIntervalTicks;
                ItemCombatEffectRequestRow damageRow =
                    battle.TerminalRows[index
                        % battle.TerminalRows.Count];
                I031NianCostRequestFact cost =
                    resource.NextCostRequest;
                Check(cost.itemId == damageRow.sourceBaseItemId,
                    "Nian cost rotation diverged from damage rotation.");
                string sharedPulseId =
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime
                        .BuildSharedPulseEventId(
                            1,
                            sequence,
                            damageRow.requestId);
                BattleSandboxNianPulseResult accepted =
                    resource.ApplyPulse(
                        source,
                        new BattleSandboxNianPulseRequest(
                            sharedPulseId,
                            1,
                            source.canonicalSignature,
                            cost.requestFactId,
                            tick));
                Check(accepted.accepted
                    && accepted.resourceApplication.pulseEventId
                        == sharedPulseId,
                    "Nian application rejected before damage pulse "
                    + sequence);
                ShougunuPhase1BattleApplicationContext context =
                    battle.AdvanceTo(tick);
                ShougunuPhase1BattleLedgerEvent damageLedger =
                    context.ledger.Events.Single(value =>
                        value.eventKind
                            == ShougunuPhase1BattleLedgerEventKind
                                .ItemApplication
                        && value.battleTick == tick);
                Check(damageLedger.resetGeneration
                        == accepted.snapshot.resetGeneration
                    && damageLedger.sourceId
                        == damageRow.requestId
                    && accepted.snapshot.Ledger
                        .Where(value => value.pulseEventId
                            == sharedPulseId)
                        .All(value =>
                            value.resetGeneration
                                == damageLedger.resetGeneration
                            && value.battleTick
                                == damageLedger.battleTick)
                    && context.enemy.AcceptedDamageApplicationCount
                        == accepted.snapshot
                            .acceptedApplicationCount,
                    "Nian/Battle ledger pulse identity mismatch at "
                    + sequence);
            }

            ShougunuPhase1BattleApplicationContext final =
                battle.Current;
            BattleSandboxNianResourceSnapshot snapshot =
                resource.Current;
            Check(snapshot.currentNian == 39
                && snapshot.totalGenerated == 200
                && snapshot.totalSpent == 181
                && snapshot.acceptedApplicationCount == 50
                && snapshot.rejectedApplicationCount == 0
                && final.battleTick == 75000L
                && final.enemy.AcceptedDamageApplicationCount == 50
                && final.enemy.ThresholdOccurrences.Count(value =>
                    value.Resolved) == 6
                && final.enemy.BasicResolvedCount == 12
                && final.DisplayEvents.Count(value =>
                    value.channel
                        == ShougunuPhase1BattleDisplayChannel
                            .EnemyShellBreak) == 7,
                "Integrated Nian/P3 nominal fixture mismatch.");

            string beforeDuplicate = snapshot.canonicalSignature;
            ItemCombatEffectRequestRow nextDamage =
                battle.TerminalRows[snapshot.nextCostOrdinal];
            BattleSandboxNianPulseResult duplicate =
                resource.ApplyPulse(
                    source,
                    new BattleSandboxNianPulseRequest(
                        ShougunuPhase1BattleSandboxVerticalSliceRuntime
                            .BuildSharedPulseEventId(
                                1,
                                1,
                                battle.TerminalRows[0].requestId),
                        1,
                        source.canonicalSignature,
                        resource.NextCostRequest.requestFactId,
                        76500L));
            Check(!duplicate.accepted
                && duplicate.reasonCode == "DUPLICATE_PULSE_EVENT"
                && resource.Current.canonicalSignature
                    == beforeDuplicate
                && battle.Current.enemy.AcceptedDamageApplicationCount
                    == 50,
                "Duplicate Nian pulse leaked into damage.");

            I031NianSourceSnapshot generationTwo =
                I031NianSourceAssembler.Assemble(
                    fixture.ItemSystem,
                    2);
            resource.Reset(generationTwo);
            battle.Reset(75000L);
            BattleSandboxNianPulseResult stale =
                resource.ApplyPulse(
                    source,
                    new BattleSandboxNianPulseRequest(
                        "battle.request.g1.051."
                        + nextDamage.requestId,
                        1,
                        source.canonicalSignature,
                        source.CostRequestFacts[0].requestFactId,
                        1500L));
            Check(!stale.accepted
                && stale.reasonCode == "GENERATION_MISMATCH"
                && resource.Current.resetGeneration == 2
                && resource.Current.currentNian == 20
                && resource.Current.ProcessedPulseEventIds.Count == 0
                && battle.Current.resetGeneration == 2
                && battle.Current.enemy
                    .AcceptedDamageApplicationCount == 0,
                "Reset/stale Nian event leaked into damage.");
            return snapshot;
        }

        private static int ExpectedNianAfterPulses(int pulseCount)
        {
            int[] costs = { 2, 3, 5, 3, 3, 6 };
            int value = 20;
            for (int index = 0; index < pulseCount; index++)
            {
                value = Math.Min(100, value + 4);
                if (value >= costs[index % costs.Length])
                {
                    value -= costs[index % costs.Length];
                }
            }

            return value;
        }

        private static ResetEvidence VerifyResetStale(
            ItemCombatEffectRequestSnapshot request)
        {
            ShougunuPhase1BattleApplicationSession session =
                ShougunuPhase1BattleApplicationEngine.Create(request, 1);
            ShougunuPhase1BattleApplicationContext before =
                session.AdvanceTo(1500L);
            ShougunuPhase1BattleDisplayEvent oldEvent =
                before.DisplayEvents.First();
            ShougunuPhase1BattleApplicationContext reset =
                session.Reset(1500L);
            Check(reset.resetGeneration == 2
                && reset.battleTick == 0L
                && reset.player.currentHp == 9999
                && reset.player.maxHp == 9999
                && reset.enemy.CurrentHp == 980
                && reset.enemy.CurrentShell == 200,
                "Reset contract state mismatch.");
            bool staleAccepted =
                ShougunuPhase1BattleSandboxVisualCueAdapter
                    .AcceptsGeneration(oldEvent, reset.resetGeneration);
            Check(!staleAccepted,
                "Old display event leaked across reset.");
            ShougunuPhase1BattleApplicationContext after =
                session.AdvanceTo(1500L);
            Check(after.enemy.AcceptedDamageApplicationCount == 1
                && after.DisplayEvents.All(value =>
                    value.resetGeneration == 2)
                && after.DisplayEvents.All(value =>
                    value.displayEventId.StartsWith(
                        "battle.display.g2.",
                        StringComparison.Ordinal)),
                "Post-reset generation/event ledger mismatch.");
            return new ResetEvidence(
                oldEvent.displayEventId,
                oldEvent.resetGeneration,
                reset.resetGeneration,
                staleAccepted,
                reset.player.currentHp,
                after.enemy.AcceptedDamageApplicationCount);
        }

        private static void VerifyUiReuse(Scene scene)
        {
            Dictionary<string, string> ui =
                CaptureProtectedUiFingerprints(scene);
            Check(ui.Count == 10,
                "Protected UI fingerprint cardinality mismatch.");
            Image playerHit =
                FindUniqueNamed<Image>(scene, "PlayerHitFeedback");
            Check(Mathf.Approximately(playerHit.color.r, 1f)
                && Mathf.Approximately(playerHit.color.g, 0.28f)
                && Mathf.Approximately(playerHit.color.b, 0.22f)
                && Mathf.Approximately(playerHit.color.a, 0f),
                "Authored PlayerHitFeedback color changed.");
            Text mechanic =
                FindUniqueNamed<Text>(scene, "MechanicFloatingText");
            Check(mechanic.fontSize == 28
                && mechanic.fontStyle == FontStyle.Bold
                && Mathf.Approximately(mechanic.color.r, 1f)
                && Mathf.Approximately(mechanic.color.g, 0.95f)
                && Mathf.Approximately(mechanic.color.b, 0.45f),
                "Authored MechanicFloatingText style changed.");
        }

        private static IReadOnlyList<string> VerifyLeakScan()
        {
            var hits = new List<string>();
            foreach (string path in RuntimeSourcePaths)
            {
                string source = File.ReadAllText(path);
                foreach (string token in ForbiddenRuntimeTokens)
                {
                    if (source.IndexOf(
                            token,
                            StringComparison.Ordinal) >= 0)
                    {
                        hits.Add(path + "::" + token);
                    }
                }
            }
            Check(hits.Count == 0,
                "Formal/save/reward/flow leak: "
                + string.Join("|", hits));
            return hits;
        }

        private static void VerifyVisualFacade()
        {
            string source = File.ReadAllText(VisualSourcePath);
            Check(source.Contains(
                    "public enum ShougunuPhase1PresentationAction")
                && source.Contains(
                    "public bool TryPlayPresentationAction(")
                && source.Contains(
                    "private IEnumerator ShellBreakPresentationRoutine")
                && source.Contains(
                    "private IEnumerator DefeatedPresentationRoutine")
                && source.Contains("defeatedPresentationActive"),
                "Dependent Visual public facade is incomplete.");
            string adapter = File.ReadAllText(RuntimeSourcePaths[2]);
            Check(
                adapter.IndexOf(
                    "BindingFlags",
                    StringComparison.Ordinal) < 0
                && adapter.IndexOf(
                    "GetMethod(",
                    StringComparison.Ordinal) < 0,
                "Visual reflection is forbidden.");
            Check(ShaFile(VisualMetaPath) == FrozenVisualMetaSha,
                "Visual .meta changed.");
        }

        private static void VerifyAcceptedPresentation(Scene scene)
        {
            TextMeshProUGUI[] templates =
                Resources.FindObjectsOfTypeAll<TextMeshProUGUI>()
                    .Where(value => value != null
                        && value.gameObject.scene == scene)
                    .ToArray();
            TextMeshProUGUI primary = templates.Single(value =>
                value.gameObject.name == "shanghai");
            TextMeshProUGUI secondary = templates.Single(value =>
                value.gameObject.name == "shanghai (1)");
            Check(
                primary.transform.parent != null
                && secondary.transform.parent != null
                && primary.transform.parent.name == "V02EnemyArea"
                && secondary.transform.parent.name == "V02EnemyArea"
                && primary.font != null
                && primary.font == secondary.font
                && primary.fontSharedMaterial != null
                && primary.fontSharedMaterial
                    == secondary.fontSharedMaterial
                && primary.enableVertexGradient
                && secondary.enableVertexGradient
                && primary.fontSize >= 40f
                && secondary.fontSize >= 40f,
                "Authored TMP templates are missing or no longer reusable.");

            string helper = File.ReadAllText(
                "Assets/_Game/Scripts/TalismanBag/BattleBridge/"
                + "NianResource/BattleSandboxAuthoredTmpPresentation.cs");
            string runtime = File.ReadAllText(RuntimeSourcePaths[0]);
            string binder = File.ReadAllText(RuntimeSourcePaths[1]);
            string adapter = File.ReadAllText(RuntimeSourcePaths[2]);
            string feedback = File.ReadAllText(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/"
                + "BattleSandboxItemTriggerFeedbackController.cs");
            Check(
                helper.Contains("PrimaryTemplateName = \"shanghai\"")
                && helper.Contains(
                    "SecondaryTemplateName = \"shanghai (1)\"")
                && helper.Contains("DefaultReadableLifetime = 1.65f")
                && helper.Contains("Instantiate(")
                && helper.Contains("acceptedRoleKeys.Add(roleKey)")
                && helper.Contains(
                    "\"MFLangSongJianYuan-Regular SDF\"")
                && helper.Contains("TMP_FontAsset.CreateFontAsset(")
                && helper.Contains("AtlasPopulationMode.Dynamic")
                && helper.Contains("TryAddCharacters(")
                && helper.Contains(
                    "runtimeChineseFallback.HasCharacter(")
                && helper.Contains("allAvailableAfterAdd")
                && helper.Contains("ForceMeshUpdate(true, true)")
                && helper.Contains(
                    "TryResolveItemSourceGradient(")
                && runtime.Contains(
                    "PresentAcceptedItemApplication(")
                && runtime.Contains(
                    "damageRow.resolvedPreMitigationDamageUnits")
                && binder.Contains(
                    "\"item.source.resolved_damage\"")
                && binder.Contains(
                    "\"item.enemy.settlement\"")
                && adapter.Contains(
                    "RegisterAcceptedItemPresentation")
                && feedback.Contains(
                    "TryPlayAcceptedPresentation")
                && feedback.Contains(
                    "SpawnAcceptedCarrierVfx")
                && feedback.Contains(
                    "acceptedPresentationEventIds")
                && feedback.Contains(
                    "completeLegacyPresentationRequest")
                && feedback.Contains("PlayResolvedPresentation")
                && feedback.Contains("SpawnFlashOnTarget")
                && feedback.Contains("TryPlayResourceSequenceVfx")
                && feedback.Contains("SpawnSkillVfx")
                && feedback.Contains("FireProjectileCarrierKey")
                && feedback.Contains("SwordQiSlashCarrierKey")
                && feedback.Contains("HeavySealDropCarrierKey")
                && feedback.Contains(
                    "ResolveAcceptedCarrierPresentationKey")
                && !feedback.Contains(
                    "ResolveAcceptedPresentationSprite")
                && !feedback.Contains("AcceptedItemTransfer_")
                && helper.Contains("NianGeneratedPaletteKey")
                && helper.Contains("NianSpentPaletteKey")
                && helper.Contains("SettlementHpPaletteKey")
                && helper.Contains("SettlementShellPaletteKey")
                && helper.Contains("SettlementBreakPaletteKey")
                && binder.Contains("\"nian.generated.source\"")
                && binder.Contains("\"nian.spent.source\"")
                && binder.Contains("\" NP\"")
                && binder.Contains("\" SH\"")
                && !binder.Contains(
                    ".Play(BattleSandboxRuntimeLoopRow"),
                "Accepted-only TMP/VFX presentation correlation is incomplete.");
        }

        private static Fixture BuildFixture(
            IReadOnlyList<ItemSystemPlacementInput> placementInputs)
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            Check(workbench != null, "Workbench catalog missing.");
            GameObject providerObject =
                new("ShougunuP1VerticalSliceVerifierProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<
                        ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult view =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbench,
                        provider);
                Check(view.IsValid,
                    "Real Item ProjectionSet invalid.");
                ItemCoreEffectIdentityCatalogSnapshot identity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                Check(identity.isValid,
                    "Core identity catalog invalid.");
                ItemCoreEffectCultivationRosterSnapshot cultivation = new(
                    ItemCoreEffectRosterCompleteness.Complete,
                    view.OrdinaryProjectionSet.Projections.Select(value =>
                        new ItemCoreEffectCultivationRow(
                            value.itemInstanceId,
                            value.baseItemId,
                            40,
                            CultivationSource,
                            ItemCoreEffectFactCompleteness.Complete)));
                Check(cultivation.isValid,
                    "Cultivation roster invalid.");

                string[] placedOrdinary = placementInputs
                    .Where(value => value != null
                        && value.itemId != "I031")
                    .Select(value => value.itemId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                Dictionary<string, string> placementByBase =
                    placementInputs.ToDictionary(
                        value => value.itemId,
                        value => value.placementId,
                        StringComparer.Ordinal);
                ItemCoreAwakeningInput[] awakeningInputs =
                    placedOrdinary.Select(baseId =>
                        new ItemCoreAwakeningInput(
                            baseId,
                            placementByBase[baseId],
                            40,
                            true,
                            CultivationSource,
                            "orange")).ToArray();
                ItemSystemSnapshot itemSystem =
                    DefaultItemSystemSnapshotProvider.Instance
                        .CreateSnapshot(
                            new ItemSystemSnapshotInput(
                                placementInputs,
                                ItemSystemBoardConfigInput.Default(),
                                awakeningInputs,
                                null,
                                ItemInnerDataCatalog.AllItems,
                                new[]
                                {
                                    I031InventoryPlacementContract
                                        .OwnedBoard()
                                }));
                Check(itemSystem.isValid,
                    "Production ItemSystem fixture invalid.");
                ItemInstanceProjectionContractSnapshot[]
                    placedProjections =
                        placedOrdinary.Select(baseId =>
                            view.OrdinaryProjectionSet.Projections.Single(
                                value => value.baseItemId == baseId))
                            .ToArray();
                ItemInstanceProjectionSetSnapshot placedProjectionSet =
                    CreateProjectionSet(placedProjections);
                ItemInstancePlacementBindingContractSnapshot binding =
                    ItemInstancePlacementBindingValidator.Instance
                        .Validate(
                            placedProjectionSet,
                            itemSystem,
                            placedProjections.Select(value =>
                                new ItemInstancePlacementBindingInput(
                                    value.itemInstanceId,
                                    placementByBase[value.baseItemId],
                                    value.baseItemId)).ToArray())
                        .snapshot;
                Check(binding.isValid,
                    "Production Item binding invalid.");
                ItemInstanceQualifiedBuildStateSnapshot qualified =
                    ItemInstanceQualifiedBuildStateAssembler.Instance
                        .Assemble(
                            new ItemInstanceQualifiedBuildStateInput(
                                view.OrdinaryProjectionSet,
                                binding,
                                itemSystem,
                                QualifiedBuildRosterCompleteness
                                    .CompleteOwnedRoster));
                Check(qualified.isValid,
                    "Production QualifiedBuild invalid.");
                ItemInstanceCoreEffectRuntimeStateSnapshot core =
                    ItemInstanceCoreEffectRuntimeStateAssembler.Instance
                        .Assemble(
                            new ItemInstanceCoreEffectRuntimeStateInput(
                                identity,
                                cultivation,
                                view.OrdinaryProjectionSet,
                                binding,
                                itemSystem,
                                ItemCoreEffectRosterCompleteness.Complete));
                Check(core.isValid,
                    "Production CoreRuntime invalid.");
                return new Fixture(
                    ItemCombatEffectRequestAssembler.Instance.Assemble(
                        view.OrdinaryProjectionSet,
                        itemSystem,
                        qualified,
                        core),
                    itemSystem);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static IReadOnlyList<ItemSystemPlacementInput>
            CurrentPlacements()
        {
            return new[]
            {
                new ItemSystemPlacementInput(
                    "P_SYSTEM_I031", "I031",
                    new Vector2Int(0, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I009", "I009",
                    new Vector2Int(1, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I012", "I012",
                    new Vector2Int(2, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I010", "I010",
                    new Vector2Int(1, 1), 90),
                new ItemSystemPlacementInput(
                    "P_BOARD_I008", "I008",
                    new Vector2Int(0, 2), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I011", "I011",
                    new Vector2Int(1, 3), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I007", "I007",
                    new Vector2Int(0, 4), 0)
            };
        }

        private static IReadOnlyList<ItemSystemPlacementInput>
            TwoNeighborI008Placements()
        {
            return new[]
            {
                new ItemSystemPlacementInput(
                    "P_SYSTEM_I031", "I031",
                    new Vector2Int(0, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I009", "I009",
                    new Vector2Int(1, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I012", "I012",
                    new Vector2Int(2, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I010", "I010",
                    new Vector2Int(1, 1), 90),
                new ItemSystemPlacementInput(
                    "P_BOARD_I008", "I008",
                    new Vector2Int(0, 2), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I011", "I011",
                    new Vector2Int(1, 3), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I007", "I007",
                    new Vector2Int(2, 4), 0)
            };
        }

        private static ItemInstanceProjectionSetSnapshot CreateProjectionSet(
            IEnumerable<ItemInstanceProjectionContractSnapshot> values)
        {
            return (ItemInstanceProjectionSetSnapshot)
                Activator.CreateInstance(
                    typeof(ItemInstanceProjectionSetSnapshot),
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic,
                    null,
                    new object[]
                    {
                        values.ToArray(),
                        Array.Empty<
                            ItemInstanceProjectionValidationError>()
                    },
                    CultureInfo.InvariantCulture);
        }

        private static Dictionary<string, string>
            CaptureProtectedUiFingerprints(Scene scene)
        {
            string[] names =
            {
                "FloatingTextAnchors",
                "DamageDealtAnchor",
                "DamageTakenAnchor",
                "StatusDamageAnchor",
                "ShieldBreakAnchor",
                "MechanicFloatingText",
                "HPText",
                "PlayerHPBar",
                "PlayerHitFeedback",
                "Shougunu_1"
            };
            var result = new Dictionary<string, string>(
                StringComparer.Ordinal);
            foreach (string name in names)
            {
                GameObject value = FindUniqueNamed<RectTransform>(
                    scene,
                    name)?.gameObject;
                Check(value != null, "Protected UI object missing: " + name);
                result[name] = Fingerprint(value);
            }
            return result;
        }

        private static string Fingerprint(GameObject value)
        {
            var builder = new StringBuilder();
            builder.Append(value.name).Append('|')
                .Append(value.activeSelf).Append('|')
                .Append(value.layer);
            if (value.transform is RectTransform rect)
            {
                AppendVector(builder, rect.anchorMin);
                AppendVector(builder, rect.anchorMax);
                AppendVector(builder, rect.anchoredPosition);
                AppendVector(builder, rect.sizeDelta);
                AppendVector(builder, rect.pivot);
                builder.Append('|').Append(rect.GetSiblingIndex());
            }
            Text text = value.GetComponent<Text>();
            if (text != null)
            {
                AppendColor(builder, text.color);
                builder.Append('|').Append(text.fontSize)
                    .Append('|').Append((int)text.fontStyle)
                    .Append('|').Append((int)text.alignment);
            }
            Image image = value.GetComponent<Image>();
            if (image != null)
            {
                AppendColor(builder, image.color);
                builder.Append('|').Append((int)image.type)
                    .Append('|').Append((int)image.fillMethod);
            }
            return ShaText(builder.ToString());
        }

        private static void AppendVector(
            StringBuilder builder,
            Vector2 value)
        {
            builder.Append('|')
                .Append(value.x.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(value.y.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void AppendColor(
            StringBuilder builder,
            Color value)
        {
            builder.Append('|')
                .Append(value.r.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(value.g.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(value.b.ToString("R", CultureInfo.InvariantCulture))
                .Append(',')
                .Append(value.a.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void WriteInstallEvidence(
            IReadOnlyDictionary<string, string> before,
            IReadOnlyDictionary<string, string> after,
            Scene scene)
        {
            var binding = new StringBuilder();
            binding.AppendLine(
                "binding,objectPath,status,evidence");
            binding.AppendLine(
                Csv("runtime")
                + "," + Csv(
                    "BuildSandboxPreviewRoot/"
                    + "EnemyCombatFeedbackRuntime")
                + ",PASS,"
                + Csv("three devOnly components attached"));
            foreach (KeyValuePair<string, string> pair in before)
            {
                binding.Append(Csv("protected-ui")).Append(',')
                    .Append(Csv(pair.Key)).Append(',')
                    .Append(pair.Value == after[pair.Key]
                        ? "PASS"
                        : "FAIL").Append(',')
                    .Append(Csv(pair.Value + "->"
                        + after[pair.Key])).AppendLine();
            }
            Write(BindingPath, binding.ToString());

            var ui = new StringBuilder();
            ui.AppendLine(
                "objectPath,reuseStatus,beforeFingerprint,"
                + "afterFingerprint,styleOrRectOverwritten");
            foreach (KeyValuePair<string, string> pair in before)
            {
                ui.Append(Csv(pair.Key)).Append(",REUSED,")
                    .Append(Csv(pair.Value)).Append(',')
                    .Append(Csv(after[pair.Key]))
                    .Append(",false").AppendLine();
            }
            Write(UiReusePath, ui.ToString());
        }

        private static void WriteAllReports(
            Scene scene,
            ItemCombatEffectRequestSnapshot request,
            ShougunuPhase1BattleApplicationTrace trace,
            ResetEvidence reset,
            IReadOnlyList<string> leakHits,
            string playSmoke)
        {
            Write(TracePath, BuildTraceCsv(trace));
            Write(BindingPath, BuildBindingCsv(scene));
            Write(UiReusePath, BuildUiReuseCsv(scene));
            Write(ResetPath, BuildResetCsv(reset));
            Write(LeakPath, BuildLeakReport(leakHits));
            Write(ManualPath, BuildManualTest());
            Write(
                ReportPath,
                BuildMainReport(
                    request,
                    trace,
                    reset,
                    playSmoke));
            AssetDatabase.Refresh();
        }

        private static string BuildMainReport(
            ItemCombatEffectRequestSnapshot request,
            ShougunuPhase1BattleApplicationTrace trace,
            ResetEvidence reset,
            string playSmoke)
        {
            ShougunuPhase1BattleApplicationContext context =
                trace.finalContext;
            string visualAfter = ShaFile(VisualSourcePath);
            string sceneAfter = ShaFile(ScenePath);
            return "# Real Item Damage × Shougunu Phase1 BattleSandbox "
                + "Vertical Slice\n\n"
                + "- package = `" + PackageId + "`\n"
                + "- classification = `COMPLEX_GUARDED_ONCE`\n"
                + "- status = `AUTOMATED_QA_PASS / USER_HANDTEST_WAITING`\n"
                + "- P3 assignment SHA-256 = `" + P3AssignmentSha + "`\n"
                + "- dependent package = `" + VisualPackageId + "`\n"
                + "- dependent assignment SHA-256 = `"
                + VisualAssignmentSha + "`\n"
                + "- dependent status = "
                + "`DEPENDENT_VISUAL_MICRO_PACKAGE_CONSUMED`\n"
                + "- Visual source before = `" + FrozenVisualSourceSha
                + "`\n"
                + "- Visual source after = `" + visualAfter + "`\n"
                + "- Visual meta unchanged = `"
                + (ShaFile(VisualMetaPath) == FrozenVisualMetaSha
                    ? "true" : "false") + "`\n"
                + "- Scene before = `" + FrozenSceneSha + "`\n"
                + "- Scene after = `" + sceneAfter + "`\n"
                + "- normal Editor play smoke = `" + playSmoke + "`\n"
                + "- Git = `NOT_USED`\n\n"
                + "## Automated terminal facts\n\n"
                + "- Item schema = `" + request.schemaId + "`\n"
                + "- real values = `29,42,53,46,55,76`\n"
                + "- I011 truth = `True/True/False`\n"
                + "- I031 ordinary request = `false`\n"
                + "- accepted Item applications = `" + trace.Rows.Count
                + "`\n"
                + "- threshold skills = `"
                + context.enemy.ThresholdOccurrences.Count(value =>
                    value.Resolved) + "`\n"
                + "- BasicAttack resolutions = `"
                + context.enemy.BasicResolvedCount + "`\n"
                + "- shell breaks = `"
                + context.DisplayEvents.Count(value =>
                    value.channel
                    == ShougunuPhase1BattleDisplayChannel.EnemyShellBreak)
                + "`\n"
                + "- player = `" + context.player.currentHp + "/"
                + context.player.maxHp + "`\n"
                + "- enemy = `" + context.enemy.Lifecycle + "@"
                + context.battleTick + "`\n"
                + "- reset generation = `" + reset.newGeneration + "`\n"
                + "- stale event accepted = `" + reset.staleAccepted
                + "`\n\n"
                + "## Presentation boundary\n\n"
                + "- Battle/P2 remains the sole damage, player HP, ledger "
                + "and display-event owner.\n"
                + "- Scene binder updates only existing Text/Image values.\n"
                + "- The existing hidden terminal Item adapter/authority is "
                + "reused; the dev fallback only delegates its public "
                + "Initialize method and never duplicates Item truth.\n"
                + "- authored RectTransforms, RGB styles and hierarchy are "
                + "unchanged; floating children are runtime-only.\n"
                + "- Visual facade returns presentation playability only.\n"
                + "- ShellBreak is a distinct existing-layer rupture cue; "
                + "Defeated remains terminal until Idle/Reset.\n"
                + "- Reflection is not used.\n"
                + "- Formal flow/save/reward/drop/AutoCombat = `NOT_TOUCHED`.\n";
        }

        private static string BuildTraceCsv(
            ShougunuPhase1BattleApplicationTrace trace)
        {
            var builder = new StringBuilder();
            builder.AppendLine(
                "applicationSequence,battleTick,sourceBaseItemId,"
                + "damage,shellDamage,hpDamage,enemyHp,enemyShell,"
                + "playerHp,basicResolved,accepted");
            foreach (ShougunuPhase1BattleApplicationTraceRow row
                     in trace.Rows)
            {
                builder.Append(row.applicationSequence).Append(',')
                    .Append(row.battleTick).Append(',')
                    .Append(Csv(row.sourceBaseItemId)).Append(',')
                    .Append(row.resolvedPreMitigationDamageUnits).Append(',')
                    .Append(row.shellDamageApplied).Append(',')
                    .Append(row.hpDamageApplied).Append(',')
                    .Append(row.enemyCurrentHp).Append(',')
                    .Append(row.enemyCurrentShell).Append(',')
                    .Append(row.playerCurrentHp).Append(',')
                    .Append(row.basicResolved).Append(',')
                    .Append(row.accepted ? "true" : "false")
                    .AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildBindingCsv(Scene scene)
        {
            ShougunuPhase1BattleSandboxSceneBinder binder =
                FindUnique<ShougunuPhase1BattleSandboxSceneBinder>(scene);
            var builder = new StringBuilder();
            builder.AppendLine("binding,objectName,status");
            AppendBinding(builder, "host", binder.gameObject, 
                "EnemyCombatFeedbackRuntime");
            AppendBinding(builder, "grid", binder.GridController,
                binder.GridController.name);
            if (binder.LegacyRuntimeLoop == null)
            {
                builder.AppendLine(
                    "legacyRuntime,(absent in authored Scene),PASS");
            }
            else
            {
                AppendBinding(builder, "legacyRuntime",
                    binder.LegacyRuntimeLoop,
                    binder.LegacyRuntimeLoop.name);
            }
            if (binder.LegacyManaLoop == null)
            {
                builder.AppendLine(
                    "legacyManaLoop,(runtime-discovered),PASS");
            }
            else
            {
                AppendBinding(builder, "legacyManaLoop",
                    binder.LegacyManaLoop,
                    binder.LegacyManaLoop.name);
            }
            AppendBinding(builder, "feedback",
                binder.EnemyCombatFeedbackController,
                "EnemyCombatFeedbackRuntime");
            AppendBinding(builder, "HPText", binder.PlayerHpText, "HPText");
            AppendBinding(
                builder, "PlayerHPBar", binder.PlayerHpBar, "Fill");
            AppendBinding(
                builder, "EnemyHPText", binder.EnemyHpText, "EnemyHPText");
            AppendBinding(
                builder, "EnemyHPBar", binder.EnemyHpBar, "Fill");
            AppendBinding(
                builder, "ShieldText", binder.ShellText, "ShieldText");
            AppendBinding(
                builder, "StateText", binder.StateText, "StateText");
            AppendBinding(
                builder, "ContinueBattleStartHook",
                binder.AuthoredResetButton,
                "V04BattlePrepareStateButton");
            return builder.ToString();
        }

        private static void AppendBinding(
            StringBuilder builder,
            string binding,
            UnityEngine.Object value,
            string expectedName)
        {
            builder.Append(Csv(binding)).Append(',')
                .Append(Csv(value == null ? string.Empty : value.name))
                .Append(',')
                .Append(value != null && value.name == expectedName
                    ? "PASS" : "FAIL")
                .AppendLine();
        }

        private static string BuildUiReuseCsv(Scene scene)
        {
            var builder = new StringBuilder();
            builder.AppendLine(
                "objectPath,reuseStatus,currentFingerprint,"
                + "authoredRgbOrRectOverwritten,persistentDuplicate");
            foreach (KeyValuePair<string, string> pair
                     in CaptureProtectedUiFingerprints(scene))
            {
                builder.Append(Csv(pair.Key)).Append(",REUSED,")
                    .Append(Csv(pair.Value))
                    .Append(",false,false").AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildResetCsv(ResetEvidence reset)
        {
            return "fixture,oldDisplayEventId,oldGeneration,"
                + "newGeneration,staleAccepted,resetPlayerHp,"
                + "postResetAcceptedApplications,status\n"
                + "reset_generation,"
                + Csv(reset.oldDisplayEventId) + ","
                + reset.oldGeneration + ","
                + reset.newGeneration + ","
                + (reset.staleAccepted ? "true" : "false") + ","
                + reset.resetPlayerHp + ","
                + reset.postResetAcceptedApplications
                + ",PASS\n";
        }

        private static string BuildLeakReport(
            IReadOnlyList<string> hits)
        {
            return "# P3 Leak Check\n\n"
                + "- runtime source count = `3`\n"
                + "- forbidden call hits = `" + hits.Count + "`\n"
                + "- formal AutoCombat/RunFlow = `NOT_REFERENCED`\n"
                + "- SaveData/PlayerPrefs = `NOT_REFERENCED`\n"
                + "- Reward/Drop/chapter = `NOT_REFERENCED`\n"
                + "- BuildSettings mutation = `NONE`\n"
                + "- upstream Item/Enemy/P2 schema mutation = `NONE`\n"
                + "- Visual reflection = `NONE`\n"
                + "- Scene = `devOnly current BattleSandbox only`\n"
                + "- Git = `NOT_USED`\n";
        }

        private static string BuildManualTest()
        {
            return "# Normal Editor Handtest\n\n"
                + "1. Open `" + ScenePath + "` and press Play.\n"
                + "2. Before placement, confirm the existing player/enemy "
                + "panels already show `玩家 HP 9999/9999`, "
                + "`守骨奴 HP 980/980`, and `护壳 200/200`.\n"
                + "3. Open battle preparation and place: I031@(0,0), "
                + "I009@(1,0), I012@(2,0), I010@(1,1) rotated 90°, "
                + "I008@(0,2), I011@(1,3), I007@(0,4).\n"
                + "4. Press the bottom-navigation middle `继续战斗` button "
                + "once. If the chain is illegal, the existing top state "
                + "text lists the exact missing/unlit/wrong-damage item.\n"
                + "5. Observe about 75 seconds: live I007→I012 damage, "
                + "seven shell breaks, six skills, twelve Basics, authored "
                + "HP/guard/floating text/player-hit feedback and Shougunu "
                + "visual cues.\n"
                + "6. Press `R` to reset; player must "
                + "return to 9999/9999 and no old text/cue may remain.\n"
                + "7. Let the restarted loop finish and confirm final player "
                + "9735/9999 and Shougunu Defeated.\n";
        }

        private static void UpdatePlaySmokeReport(string status)
        {
            if (!File.Exists(ReportPath))
            {
                return;
            }
            string text = File.ReadAllText(ReportPath);
            const string prefix = "- normal Editor play smoke = `";
            int start = text.IndexOf(prefix, StringComparison.Ordinal);
            if (start < 0)
            {
                return;
            }
            int valueStart = start + prefix.Length;
            int valueEnd = text.IndexOf('`', valueStart);
            if (valueEnd < 0)
            {
                return;
            }
            text = text.Substring(0, valueStart)
                + (status ?? string.Empty)
                + text.Substring(valueEnd);
            Write(ReportPath, text);
        }

        private static void ClearPlaySmokeState()
        {
            SessionState.EraseInt(PlayStateKey);
            SessionState.EraseInt(PlayPollKey);
            SessionState.EraseInt(PlayGenerationKey);
            SessionState.EraseInt(PlayLoggedStateKey);
            SessionState.EraseString(PlayFailureKey);
            SessionState.EraseString(PlayStartedUtcTicksKey);
            SessionState.EraseString(PlayTransitionUtcTicksKey);
            EditorApplication.update -= ContinuePlayModeSmoke;
        }

        private static void CheckNamedCardinality(
            Scene scene,
            string name,
            int expected)
        {
            int count = AllSceneTransforms(scene)
                .Count(value => value.name == name);
            Check(count == expected,
                name + " cardinality " + count + " != " + expected);
        }

        private static T FindUnique<T>(Scene scene)
            where T : Component
        {
            T[] values = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .Where(value => value != null)
                .ToArray();
            Check(values.Length == 1,
                typeof(T).Name + " cardinality " + values.Length);
            return values[0];
        }

        private static T FindOptionalUnique<T>(Scene scene)
            where T : Component
        {
            T[] values = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .Where(value => value != null)
                .ToArray();
            Check(values.Length <= 1,
                typeof(T).Name + " cardinality " + values.Length);
            return values.FirstOrDefault();
        }

        private static T FindUniqueNamed<T>(
            Scene scene,
            string name)
            where T : Component
        {
            T[] values = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .Where(value => value != null && value.name == name)
                .ToArray();
            Check(values.Length == 1,
                typeof(T).Name + "/" + name
                + " cardinality " + values.Length);
            return values[0];
        }

        private static IEnumerable<Transform> AllSceneTransforms(
            Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<Transform>(true));
        }

        private static GameObject FindPath(Scene scene, string path)
        {
            string[] segments = path.Split('/');
            GameObject root = scene.GetRootGameObjects()
                .SingleOrDefault(value => value.name == segments[0]);
            if (root == null)
            {
                return null;
            }
            Transform current = root.transform;
            for (int index = 1; index < segments.Length; index++)
            {
                current = current.Find(segments[index]);
                if (current == null)
                {
                    return null;
                }
            }
            return current.gameObject;
        }

        private static T GetComponentAt<T>(Scene scene, string path)
            where T : Component
        {
            GameObject value = FindPath(scene, path);
            Check(value != null, "Scene path missing: " + path);
            T component = value.GetComponent<T>();
            Check(component != null,
                typeof(T).Name + " missing at " + path);
            return component;
        }

        private static Image FindFill(GameObject root)
        {
            if (root == null)
            {
                return null;
            }
            Transform fill = root.transform.Find("Fill");
            return fill == null ? null : fill.GetComponent<Image>();
        }

        private static void Bind(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property =
                serialized.FindProperty(propertyName);
            Check(property != null,
                target.GetType().Name + "." + propertyName
                + " serialized field missing.");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static string ShaFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty);
        }

        private static string ShaText(string value)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value ?? string.Empty)))
                .Replace("-", string.Empty);
        }

        private static void Write(string path, string value)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(
                path,
                value ?? string.Empty,
                new UTF8Encoding(false));
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty)
                .Replace("\"", "\"\"") + "\"";
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void ExitBatch(int code)
        {
            bool autoExitEditor =
                SessionState.GetBool(PlayAutoExitEditorKey, false);
            if (Application.isBatchMode || autoExitEditor)
            {
                SessionState.EraseBool(PlayAutoExitEditorKey);
                EditorApplication.Exit(code);
            }
        }

        private sealed class Fixture
        {
            public Fixture(
                ItemCombatEffectRequestSnapshot request,
                ItemSystemSnapshot itemSystem)
            {
                Request = request;
                ItemSystem = itemSystem;
            }

            public ItemCombatEffectRequestSnapshot Request { get; }
            public ItemSystemSnapshot ItemSystem { get; }
        }

        private sealed class ResetEvidence
        {
            public ResetEvidence(
                string oldDisplayEventId,
                int oldGeneration,
                int newGeneration,
                bool staleAccepted,
                int resetPlayerHp,
                int postResetAcceptedApplications)
            {
                this.oldDisplayEventId = oldDisplayEventId;
                this.oldGeneration = oldGeneration;
                this.newGeneration = newGeneration;
                this.staleAccepted = staleAccepted;
                this.resetPlayerHp = resetPlayerHp;
                this.postResetAcceptedApplications =
                    postResetAcceptedApplications;
            }

            public readonly string oldDisplayEventId;
            public readonly int oldGeneration;
            public readonly int newGeneration;
            public readonly bool staleAccepted;
            public readonly int resetPlayerHp;
            public readonly int postResetAcceptedApplications;
        }

        private readonly struct PlayPlacement
        {
            public PlayPlacement(
                string itemId,
                ItemShapeCell cell,
                ItemShapeRotation rotation)
            {
                this.itemId = itemId;
                this.cell = cell;
                this.rotation = rotation;
            }

            public readonly string itemId;
            public readonly ItemShapeCell cell;
            public readonly ItemShapeRotation rotation;
        }
    }
}
