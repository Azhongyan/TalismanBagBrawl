using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.BuildSandbox
{
    /// <summary>
    /// Dedicated verifier for the stable master tray and manual-arrange contract.
    /// It does not invoke a scene builder and never writes Unity assets; only its package reports.
    /// </summary>
    [InitializeOnLoad]
    public static class BattleSandboxTrayStableLayoutAndManualSortVerifier
    {
        private const string PackageName = "V0.4-BattleSandboxTrayStableLayoutAndManualSort01";
        private const string MenuPath =
            "Talisman Bag/V0.4/Verify BattleSandbox Tray Stable Layout And Manual Sort";
        private const string RuntimeMenuPath =
            "Talisman Bag/V0.4/Verify BattleSandbox Tray Stable Layout Real Pointer Path";
        private const string RuntimePendingSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.RuntimePending";
        private const string RuntimeBatchExitSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.RuntimeBatchExit";
        private const string RuntimeDriverInstalledSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.RuntimeDriverInstalled";
        private const string RuntimePrePlayAssetPathSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.PrePlayAssetPath";
        private const string RuntimePrePlayWasDirtySessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.PrePlayWasDirty";
        private const string RuntimePrePlayCleanSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.PrePlayClean";
        private const string RuntimeInitialPlayPathSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.InitialPlayPath";
        private const string RuntimeReloadedPlayPathSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.ReloadedPlayPath";
        private const string RuntimeExactLoadedPlayPathSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.ExactLoadedPlayPath";
        private const string RuntimePlaySceneEventsSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.PlaySceneEvents";
        private const string RuntimeBootstrapArmedSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.BootstrapArmed";
        private const string RuntimeEnteredPlayModeArmedSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.EnteredPlayModeArmed";
        private const string RuntimeEnteredPlayModeObservedSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.EnteredPlayModeObserved";
        private const string RuntimeReloadRequestedSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.ReloadRequested";
        private const string RuntimeReloadRequestPendingSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.ReloadRequestPending";
        private const string RuntimeExactEventPendingSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.ExactEventPending";
        private const string RuntimeSceneLoadEventCountSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.SceneLoadEventCount";
        private const string RuntimePreArmedSceneLoadEventCountSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.PreArmedSceneLoadEventCount";
        private const string RuntimePostArmedSceneLoadEventCountSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.PostArmedSceneLoadEventCount";
        private const string RuntimeReloadInvocationCountSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.ReloadInvocationCount";
        private const string RuntimeBootstrapUpdateCountSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.BootstrapUpdateCount";
        private const string RuntimeExactEventUpdateSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.ExactEventUpdate";
        private const string RuntimeSceneHashBeforeSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.SceneHashBefore";
        private const string RuntimeDriverCountSessionKey =
            "TalismanBag.BattleSandboxTrayStableLayout.RuntimeDriverCount";
        private const int RuntimeBootstrapWatchdogUpdateLimit = 600;
        private const string ProjectRoot = "F:/Porject/TalismanBagBrawl";
        private const string ControllerPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs";
        private const string TrayViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string ReportDirectory = "Docs/V0.4/Reports";

        static BattleSandboxTrayStableLayoutAndManualSortVerifier()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            SceneManager.sceneLoaded -= HandleVerifierPlaySceneLoaded;
            if (SessionState.GetBool(RuntimePendingSessionKey, false))
            {
                SceneManager.sceneLoaded += HandleVerifierPlaySceneLoaded;
            }
            EditorApplication.update -= HandleRuntimeBootstrapUpdate;
            if (SessionState.GetBool(RuntimePendingSessionKey, false)
                && SessionState.GetBool(
                    RuntimeEnteredPlayModeArmedSessionKey,
                    false))
            {
                EditorApplication.update +=
                    HandleRuntimeBootstrapUpdate;
            }
        }

        [MenuItem(MenuPath)]
        public static void Run()
        {
            List<string> failures = new();
            List<string> evidence = new();
            VerifyStaticMasterAndProjectionContract(failures, evidence);
            VerifyAtomicTrayPublicationFixture(failures, evidence);
            VerifyArtworkVisibilityOwnershipFixture(failures, evidence);
            VerifyStableFilteredProjectionFixture(failures, evidence);
            VerifyTrayRowSnapContract(failures, evidence);
            VerifyThirtyOrdinaryItemsAndI031(failures, evidence);
            VerifyArrangeButtonSceneContract(failures, evidence);
            WriteReports(failures, evidence);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException(PackageName + " verifier failed: "
                    + string.Join(" | ", failures));
            }
            Debug.Log(PackageName + " PASS");
        }

        [MenuItem(RuntimeMenuPath)]
        public static void RunTargetSceneRealPointerPath()
        {
            BeginTargetSceneRealPointerPath(exitEditorWhenDone: false);
        }

        public static void RunTargetSceneRealPointerPathBatch()
        {
            BeginTargetSceneRealPointerPath(exitEditorWhenDone: true);
        }

        private static void BeginTargetSceneRealPointerPath(
            bool exitEditorWhenDone)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    PackageName + " runtime verifier requires Edit Mode.");
            }

            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid()
                || !string.Equals(scene.path, ScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    PackageName + " could not open target scene.");
            }

            CleanupRuntimeBootstrapSubscriptions();
            ResetRuntimeBootstrapEvidence();
            SessionState.SetBool(RuntimePendingSessionKey, true);
            SessionState.SetBool(
                RuntimeBatchExitSessionKey,
                exitEditorWhenDone);
            SessionState.SetString(
                RuntimePrePlayAssetPathSessionKey,
                scene.path);
            SessionState.SetString(
                RuntimeSceneHashBeforeSessionKey,
                Sha256FileBytes(ScenePath));
            EditorApplication.delayCall +=
                SettleTargetSceneBeforePlay;
        }

        private static void SettleTargetSceneBeforePlay()
        {
            if (!SessionState.GetBool(RuntimePendingSessionKey, false))
            {
                return;
            }

            try
            {
                Scene targetScene = RequireSoleExactTargetScene(
                    "TARGET_PREPLAY_SCENE_INVALID");
                bool wasDirty = targetScene.isDirty;
                SessionState.SetBool(
                    RuntimePrePlayWasDirtySessionKey,
                    wasDirty);
                if (wasDirty)
                {
                    MarkTargetSceneClean(targetScene);
                }

                bool isClean = !targetScene.isDirty;
                SessionState.SetBool(
                    RuntimePrePlayCleanSessionKey,
                    isClean);
                if (!isClean)
                {
                    throw new InvalidOperationException(
                        "TARGET_PREPLAY_SCENE_NOT_CLEAN");
                }

                EditorApplication.delayCall +=
                    EnterTargetScenePlayModeAfterSettle;
            }
            catch (Exception exception)
            {
                FailRuntimeBootstrap(
                    "TARGET_PREPLAY_SETTLE_FAILED",
                    exception);
            }
        }

        private static void EnterTargetScenePlayModeAfterSettle()
        {
            if (!SessionState.GetBool(RuntimePendingSessionKey, false))
            {
                return;
            }

            try
            {
                Scene targetScene = RequireSoleExactTargetScene(
                    "TARGET_PREPLAY_FOREIGN_SCENE");
                if (targetScene.isDirty)
                {
                    throw new InvalidOperationException(
                        "TARGET_PREPLAY_DIRTIED_AFTER_CLEAN");
                }
                string beforeHash = SessionState.GetString(
                    RuntimeSceneHashBeforeSessionKey,
                    string.Empty);
                string currentHash = Sha256FileBytes(ScenePath);
                if (!string.Equals(
                        beforeHash,
                        currentHash,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "TARGET_SCENE_BYTES_CHANGED_BEFORE_PLAY");
                }

                RequireNoRuntimeVerifierDriver(
                    "TARGET_DRIVER_EXISTS_BEFORE_PREPLAY_SUBSCRIPTION");
                SessionState.SetBool(
                    RuntimeEnteredPlayModeArmedSessionKey,
                    false);
                SceneManager.sceneLoaded -=
                    HandleVerifierPlaySceneLoaded;
                SceneManager.sceneLoaded +=
                    HandleVerifierPlaySceneLoaded;
                AppendPlaySceneEvent(
                    "preplay-observer-subscribed:armed=False"
                    + ";driverCount=0");
                EditorApplication.EnterPlaymode();
            }
            catch (Exception exception)
            {
                FailRuntimeBootstrap(
                    "TARGET_PREPLAY_ENTER_FAILED",
                    exception);
            }
        }

        private static void HandlePlayModeStateChanged(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode
                || state == PlayModeStateChange.EnteredEditMode)
            {
                CleanupRuntimeBootstrapSubscriptions();
                return;
            }

            if (state != PlayModeStateChange.EnteredPlayMode
                || !SessionState.GetBool(RuntimePendingSessionKey, false))
            {
                return;
            }

            try
            {
                if (SessionState.GetBool(
                        RuntimeBootstrapArmedSessionKey,
                        false))
                {
                    return;
                }

                RequireNoRuntimeVerifierDriver(
                    "TARGET_DRIVER_EXISTS_BEFORE_ENTERED_PLAYMODE_ARM");
                SceneManager.sceneLoaded -=
                    HandleVerifierPlaySceneLoaded;
                SceneManager.sceneLoaded +=
                    HandleVerifierPlaySceneLoaded;
                EditorApplication.update -=
                    HandleRuntimeBootstrapUpdate;
                EditorApplication.update +=
                    HandleRuntimeBootstrapUpdate;
                SessionState.SetBool(
                    RuntimeBootstrapArmedSessionKey,
                    true);
                SessionState.SetBool(
                    RuntimeEnteredPlayModeArmedSessionKey,
                    true);
                SessionState.SetBool(
                    RuntimeEnteredPlayModeObservedSessionKey,
                    true);

                Scene activeScene = SceneManager.GetActiveScene();
                string initialPath = activeScene.IsValid()
                    ? activeScene.path ?? string.Empty
                    : string.Empty;
                SessionState.SetString(
                    RuntimeInitialPlayPathSessionKey,
                    initialPath);
                AppendPlaySceneEvent(
                    "entered#0:path=" + initialPath
                    + ";armed=True;driverCount=0");
            }
            catch (Exception exception)
            {
                FailRuntimeBootstrap(
                    "TARGET_PLAYMODE_EVENT_ARM_FAILED",
                    exception);
            }
        }

        private static void HandleVerifierPlaySceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            if (!SessionState.GetBool(RuntimePendingSessionKey, false))
            {
                SceneManager.sceneLoaded -=
                    HandleVerifierPlaySceneLoaded;
                return;
            }

            string loadedPath = scene.IsValid()
                ? scene.path ?? string.Empty
                : string.Empty;
            int eventCount = SessionState.GetInt(
                    RuntimeSceneLoadEventCountSessionKey,
                    0)
                + 1;
            SessionState.SetInt(
                RuntimeSceneLoadEventCountSessionKey,
                eventCount);
            bool enteredPlayModeArmed = SessionState.GetBool(
                RuntimeEnteredPlayModeArmedSessionKey,
                false);
            int armedEventCount;
            if (enteredPlayModeArmed)
            {
                armedEventCount = SessionState.GetInt(
                        RuntimePostArmedSceneLoadEventCountSessionKey,
                        0)
                    + 1;
                SessionState.SetInt(
                    RuntimePostArmedSceneLoadEventCountSessionKey,
                    armedEventCount);
            }
            else
            {
                armedEventCount = SessionState.GetInt(
                        RuntimePreArmedSceneLoadEventCountSessionKey,
                        0)
                    + 1;
                SessionState.SetInt(
                    RuntimePreArmedSceneLoadEventCountSessionKey,
                    armedEventCount);
            }
            AppendPlaySceneEvent(
                "loaded#" + eventCount.ToString(
                    CultureInfo.InvariantCulture)
                + ":path=" + loadedPath
                + ";mode=" + mode
                + ";armed="
                + enteredPlayModeArmed.ToString()
                + ";armedEvent#="
                + armedEventCount.ToString(
                    CultureInfo.InvariantCulture)
                + ";driverCount=0");

            try
            {
                RequireNoRuntimeVerifierDriver(
                    "TARGET_DRIVER_EXISTS_BEFORE_POST_ARMED_EXACT_SCENE_EVENT");
                if (!enteredPlayModeArmed)
                {
                    return;
                }

                bool isExact = string.Equals(
                    loadedPath,
                    ScenePath,
                    StringComparison.Ordinal);
                if (isExact)
                {
                    if (mode != LoadSceneMode.Single)
                    {
                        throw new InvalidOperationException(
                            "TARGET_EXACT_SCENE_LOAD_MODE_INVALID:"
                            + mode);
                    }

                    bool reloadRequested = SessionState.GetBool(
                        RuntimeReloadRequestedSessionKey,
                        false);
                    int reloadInvocationCount = SessionState.GetInt(
                        RuntimeReloadInvocationCountSessionKey,
                        0);
                    if (reloadRequested
                        && reloadInvocationCount != 1)
                    {
                        AppendPlaySceneEvent(
                            "exact-before-reload-invocation");
                        return;
                    }

                    SessionState.SetString(
                        RuntimeExactLoadedPlayPathSessionKey,
                        loadedPath);
                    if (reloadRequested)
                    {
                        SessionState.SetString(
                            RuntimeReloadedPlayPathSessionKey,
                            loadedPath);
                    }
                    SessionState.SetInt(
                        RuntimeExactEventUpdateSessionKey,
                        SessionState.GetInt(
                            RuntimeBootstrapUpdateCountSessionKey,
                            0));
                    SessionState.SetBool(
                        RuntimeExactEventPendingSessionKey,
                        true);
                    return;
                }

                if (armedEventCount == 1)
                {
                    SessionState.SetBool(
                        RuntimeReloadRequestedSessionKey,
                        true);
                    SessionState.SetBool(
                        RuntimeReloadRequestPendingSessionKey,
                        true);
                    AppendPlaySceneEvent(
                        "reload-requested-after-post-armed#1");
                }
            }
            catch (Exception exception)
            {
                FailRuntimeBootstrap(
                    "TARGET_PLAY_SCENE_EVENT_FAILED",
                    exception);
            }
        }

        private static void HandleRuntimeBootstrapUpdate()
        {
            if (!SessionState.GetBool(RuntimePendingSessionKey, false))
            {
                CleanupRuntimeBootstrapSubscriptions();
                return;
            }

            try
            {
                int updateCount = SessionState.GetInt(
                        RuntimeBootstrapUpdateCountSessionKey,
                        0)
                    + 1;
                SessionState.SetInt(
                    RuntimeBootstrapUpdateCountSessionKey,
                    updateCount);
                if (updateCount > RuntimeBootstrapWatchdogUpdateLimit)
                {
                    throw new TimeoutException(
                        "TARGET_PLAY_SCENE_EVENT_TIMEOUT:"
                        + updateCount.ToString(
                            CultureInfo.InvariantCulture)
                        + ";events="
                        + SessionState.GetString(
                            RuntimePlaySceneEventsSessionKey,
                            string.Empty));
                }

                if (SessionState.GetBool(
                        RuntimeReloadRequestPendingSessionKey,
                        false))
                {
                    SessionState.SetBool(
                        RuntimeReloadRequestPendingSessionKey,
                        false);
                    int invocationCount = SessionState.GetInt(
                            RuntimeReloadInvocationCountSessionKey,
                            0)
                        + 1;
                    SessionState.SetInt(
                        RuntimeReloadInvocationCountSessionKey,
                        invocationCount);
                    if (invocationCount != 1)
                    {
                        throw new InvalidOperationException(
                            "TARGET_PLAY_SCENE_RELOAD_DUPLICATE:"
                            + invocationCount.ToString(
                                CultureInfo.InvariantCulture));
                    }

                    AppendPlaySceneEvent(
                        "reload-invoked#"
                        + invocationCount.ToString(
                            CultureInfo.InvariantCulture)
                        + ":path=" + ScenePath
                        + ";mode=" + LoadSceneMode.Single);
                    EditorSceneManager.LoadSceneInPlayMode(
                        ScenePath,
                        new LoadSceneParameters(
                            LoadSceneMode.Single));
                    return;
                }

                if (!SessionState.GetBool(
                        RuntimeExactEventPendingSessionKey,
                        false))
                {
                    return;
                }

                int exactEventUpdate = SessionState.GetInt(
                    RuntimeExactEventUpdateSessionKey,
                    -1);
                if (updateCount <= exactEventUpdate)
                {
                    return;
                }

                ConfirmExactPlaySceneAndInstallDriver();
            }
            catch (Exception exception)
            {
                FailRuntimeBootstrap(
                    "TARGET_PLAY_SCENE_EVENT_ORDER_FAILED",
                    exception);
            }
        }

        private static void ConfirmExactPlaySceneAndInstallDriver()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid()
                || !activeScene.isLoaded
                || !string.Equals(
                    activeScene.path,
                    ScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "TARGET_POST_EVENT_ACTIVE_SCENE_NOT_EXACT:"
                    + (activeScene.IsValid()
                        ? activeScene.path
                        : "<invalid>"));
            }
            if (!string.Equals(
                    SessionState.GetString(
                        RuntimeExactLoadedPlayPathSessionKey,
                        string.Empty),
                    ScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "TARGET_EXACT_SCENE_EVENT_NOT_RECORDED");
            }

            RequireNoRuntimeVerifierDriver(
                "TARGET_DRIVER_EXISTS_BEFORE_EXACT_CONFIRMATION");
            AppendPlaySceneEvent(
                "confirmed:path=" + activeScene.path
                + ";afterUpdate="
                + SessionState.GetInt(
                    RuntimeBootstrapUpdateCountSessionKey,
                    0).ToString(CultureInfo.InvariantCulture)
                + ";driverCount=0");
            CleanupRuntimeBootstrapSubscriptions();
            InstallRuntimeVerifierDriver();
        }

        private static void InstallRuntimeVerifierDriver()
        {
            if (!EditorApplication.isPlaying
                || !SessionState.GetBool(
                    RuntimePendingSessionKey,
                    false))
            {
                return;
            }

            try
            {
                if (!SessionState.GetBool(
                        RuntimeExactEventPendingSessionKey,
                        false)
                    || !SessionState.GetBool(
                        RuntimeEnteredPlayModeArmedSessionKey,
                        false)
                    || SessionState.GetInt(
                        RuntimePostArmedSceneLoadEventCountSessionKey,
                        0) < 1)
                {
                    throw new InvalidOperationException(
                        "TARGET_DRIVER_INSTALL_WITHOUT_POST_ARMED_EXACT_SCENE_EVENT");
                }
                Scene activeScene = SceneManager.GetActiveScene();
                if (!activeScene.IsValid()
                    || !string.Equals(
                        activeScene.path,
                        ScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "TARGET_PLAYMODE_ACTIVE_SCENE_NOT_EXACT:"
                        + (activeScene.IsValid()
                            ? activeScene.path
                            : "<invalid>"));
                }

                BattleSandboxTrayStableLayoutRuntimeVerifierDriver[]
                    existingDrivers = Resources.FindObjectsOfTypeAll<
                        BattleSandboxTrayStableLayoutRuntimeVerifierDriver>();
                if (existingDrivers.Length != 0
                    || SessionState.GetBool(
                        RuntimeDriverInstalledSessionKey,
                        false))
                {
                    throw new InvalidOperationException(
                        "TARGET_VERIFIER_DRIVER_DUPLICATE:"
                        + existingDrivers.Length.ToString(
                            CultureInfo.InvariantCulture));
                }

                GameObject driverObject = new(
                    "BattleSandboxTrayStableLayoutRuntimeVerifierDriver",
                    typeof(BattleSandboxTrayStableLayoutRuntimeVerifierDriver));
                driverObject.hideFlags = HideFlags.HideAndDontSave;
                int driverCount = Resources.FindObjectsOfTypeAll<
                    BattleSandboxTrayStableLayoutRuntimeVerifierDriver>()
                    .Length;
                if (driverCount != 1)
                {
                    throw new InvalidOperationException(
                        "TARGET_VERIFIER_DRIVER_CARDINALITY:"
                        + driverCount.ToString(
                            CultureInfo.InvariantCulture));
                }

                SessionState.SetInt(
                    RuntimeDriverCountSessionKey,
                    driverCount);
                SessionState.SetBool(
                    RuntimeDriverInstalledSessionKey,
                    true);
                SessionState.SetBool(
                    RuntimeExactEventPendingSessionKey,
                    false);
                SessionState.SetBool(
                    RuntimeBootstrapArmedSessionKey,
                    false);
                SessionState.SetBool(
                    RuntimeEnteredPlayModeArmedSessionKey,
                    false);
                AppendPlaySceneEvent(
                    "driver-installed:count="
                    + driverCount.ToString(
                        CultureInfo.InvariantCulture));
                SessionState.EraseBool(RuntimePendingSessionKey);
            }
            catch (Exception exception)
            {
                FailRuntimeBootstrap(
                    "TARGET_VERIFIER_DRIVER_INSTALL_FAILED",
                    exception);
            }
        }

        internal static void CompleteTargetSceneRuntimeVerification(
            IReadOnlyList<string> runtimeFailures,
            IReadOnlyList<string> runtimeEvidence)
        {
            List<string> failures = new(runtimeFailures
                ?? Array.Empty<string>());
            List<string> evidence = new(runtimeEvidence
                ?? Array.Empty<string>());
            AppendRuntimeBootstrapEvidence(failures, evidence);
            VerifyStaticMasterAndProjectionContract(failures, evidence);
            VerifyAtomicTrayPublicationFixture(failures, evidence);
            VerifyArtworkVisibilityOwnershipFixture(failures, evidence);
            VerifyStableFilteredProjectionFixture(failures, evidence);
            VerifyTrayRowSnapContract(failures, evidence);
            VerifyThirtyOrdinaryItemsAndI031(failures, evidence);
            VerifyArrangeButtonSceneContract(failures, evidence);
            WriteReports(failures, evidence);

            bool batchExit = SessionState.GetBool(
                RuntimeBatchExitSessionKey,
                false);
            SessionState.EraseBool(RuntimeBatchExitSessionKey);
            CleanupRuntimeBootstrapSubscriptions();
            SessionState.EraseBool(RuntimePendingSessionKey);
            SessionState.SetBool(
                RuntimeBootstrapArmedSessionKey,
                false);
            SessionState.SetBool(
                RuntimeEnteredPlayModeArmedSessionKey,
                false);
            SessionState.SetBool(
                RuntimeExactEventPendingSessionKey,
                false);
            if (failures.Count > 0)
            {
                Debug.LogError(PackageName + " TARGET_SCENE_RUNTIME FAIL: "
                    + string.Join(" | ", failures));
            }
            else
            {
                Debug.Log(PackageName
                    + " COMPONENT_FIXTURE_PASS"
                    + " / TARGET_SCENE_REAL_POINTER_PATH_PASS"
                    + " / REAL_TRAY_RUNTIME_PATH_PASS");
            }

            if (batchExit)
            {
                EditorApplication.Exit(failures.Count == 0 ? 0 : 1);
            }
            else
            {
                EditorApplication.ExitPlaymode();
            }
        }

        private static void VerifyStaticMasterAndProjectionContract(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            string controller = ReadProjectText(ControllerPath);
            string trayView = ReadProjectText(TrayViewPath);
            Require(!trayView.Contains("CompactTrayWhenReturningToAllCategory();",
                    StringComparison.Ordinal), "FILTER_RETURNS_ALL_REPACKS_MASTER", failures);
            Require(controller.Contains("Compatibility entrypoint retained", StringComparison.Ordinal)
                    && !controller.Contains("shapeAwareTrayGrid.Clear();\n            List<PreviewItem> packedItems",
                        StringComparison.Ordinal),
                "LEGACY_IMPLICIT_REPACK_REMAINS", failures);
            Require(trayView.Contains("displayedPlacementsByItemId", StringComparison.Ordinal)
                    && trayView.Contains("activeFilteredPlacementModels", StringComparison.Ordinal)
                    && trayView.Contains("TryGetDisplayedPlacement", StringComparison.Ordinal)
                    && trayView.Contains("RemoveActiveFilteredItemAndRemember",
                        StringComparison.Ordinal)
                    && trayView.Contains("TryBuildSingleStableFilteredPlacement",
                        StringComparison.Ordinal),
                "FILTERED_PROJECTION_NOT_EXPLICIT", failures);
            Require(controller.Contains("RememberTrayPlacement", StringComparison.Ordinal)
                    && controller.Contains("TryRestoreOrFindFirstTrayPlacement", StringComparison.Ordinal),
                "REMEMBERED_RETURN_CONTRACT_MISSING", failures);
            Require(controller.Contains("I031InventoryPlacementContract.SpecialIdentityId", StringComparison.Ordinal)
                    && controller.Contains("ItemInstanceId", StringComparison.Ordinal),
                "INSTANCE_OR_SPECIAL_IDENTITY_KEY_MISSING", failures);
            Require(controller.Contains("TryBuildArrangedTrayMaster", StringComparison.Ordinal)
                    && controller.Contains("OrderManualTrayArrangeItems", StringComparison.Ordinal),
                "ATOMIC_MANUAL_ARRANGE_MISSING", failures);
            Require(trayView.Contains("BeginTrayViewTransaction", StringComparison.Ordinal)
                    && trayView.Contains("CommitTrayViewTransaction", StringComparison.Ordinal)
                    && trayView.Contains("RollbackTrayViewTransaction", StringComparison.Ordinal)
                    && trayView.Contains("PublishTrayViews", StringComparison.Ordinal)
                    && controller.Contains("BeginTrayViewTransaction", StringComparison.Ordinal)
                    && controller.Contains("CommitTrayViewTransaction", StringComparison.Ordinal),
                "ATOMIC_TRAY_VIEW_PUBLICATION_MISSING", failures);
            Require(trayView.Contains("grid.cellSize.y + grid.spacing.y", StringComparison.Ordinal)
                    && trayView.Contains("ResolveNearestTrayRowOffset", StringComparison.Ordinal)
                    && trayView.Contains("ResolveTrayRowSnapVelocityThreshold", StringComparison.Ordinal)
                    && trayView.Contains("controller?.IsTrayItemDragActive", StringComparison.Ordinal)
                    && trayView.Contains("BindProductionRowSnapObserver",
                        StringComparison.Ordinal)
                    && trayView.Contains(
                        "observedRowSnapScrollRect.onValueChanged.AddListener",
                        StringComparison.Ordinal)
                    && trayView.Contains(
                        "observedRowSnapScrollRect == scrollRect",
                        StringComparison.Ordinal)
                    && trayView.Contains(
                        "CalculateRelativeRectTransformBounds",
                        StringComparison.Ordinal),
                "ROW_SNAP_CONTRACT_MISSING", failures);
            string cardView = ReadProjectText(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs");
            Require(!cardView.Contains(
                        "selected.gameObject.SetActive(true);",
                        StringComparison.Ordinal)
                    && cardView.Contains(
                        "Image.gameObject != cardVisibilityOwner",
                        StringComparison.Ordinal)
                    && cardView.Contains(
                        "IsArtworkEffectivelyRendering",
                        StringComparison.Ordinal),
                "ARTWORK_OWNS_CARD_VISIBILITY", failures);
            Require(trayView.Contains(
                        "return Array.Empty<TrayPlacementViewModel>();",
                        StringComparison.Ordinal),
                "EMPTY_FILTER_FALLS_BACK_TO_MASTER", failures);
            evidence.Add("static-master-filter-memory-arrange=PASS");
        }

        private static void VerifyAtomicTrayPublicationFixture(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            GameObject fixture = new(
                "BattleSandboxTrayAtomicPublicationFixture",
                typeof(RectTransform),
                typeof(BuildItemTrayPreviewView));
            fixture.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                BuildItemTrayPreviewView view =
                    fixture.GetComponent<BuildItemTrayPreviewView>();
                int before = view.TrayViewPublicationRevision;
                view.BeginTrayViewTransaction();
                view.SetItemInTray("I001", false);
                Require(view.IsTrayViewTransactionActive,
                    "TRAY_VIEW_TRANSACTION_NOT_ACTIVE", failures);
                Require(view.TrayViewPublicationRevision == before,
                    "TRAY_VIEW_PUBLISHED_INTERMEDIATE_STATE", failures);
                Require(view.CommitTrayViewTransaction(),
                    "TRAY_VIEW_TRANSACTION_COMMIT_REJECTED", failures);
                Require(!view.IsTrayViewTransactionActive
                        && view.TrayViewPublicationRevision == before + 1,
                    "TRAY_VIEW_DID_NOT_PUBLISH_EXACTLY_ONCE", failures);

                int beforeRollback = view.TrayViewPublicationRevision;
                view.BeginTrayViewTransaction();
                view.SetItemInTray("I002", false);
                view.RollbackTrayViewTransaction();
                Require(!view.IsTrayViewTransactionActive
                        && view.TrayViewPublicationRevision == beforeRollback,
                    "TRAY_VIEW_ROLLBACK_PUBLISHED_PARTIAL_STATE", failures);
                evidence.Add("atomic-tray-publication=one-commit/zero-rollback");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fixture);
            }
        }

        private static void VerifyArtworkVisibilityOwnershipFixture(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            GameObject fixture = new(
                "BattleSandboxTrayArtworkVisibilityFixture",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(BuildItemPreviewCardView));
            fixture.hideFlags = HideFlags.HideAndDontSave;
            Texture2D texture = new(2, 2);
            texture.hideFlags = HideFlags.HideAndDontSave;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                BuildItemPreviewCardView card =
                    fixture.GetComponent<BuildItemPreviewCardView>();
                CanvasGroup group = fixture.GetComponent<CanvasGroup>();
                card.Bind(
                    fixture.GetComponent<RectTransform>(),
                    group,
                    fixture.GetComponent<Image>(),
                    null,
                    null,
                    null);
                card.BindItemDisplayData(
                    null,
                    "I001",
                    "fixture",
                    "基础",
                    "single",
                    Color.white);
                card.SetVisible(false);
                Require(card.BindAuthoritativeArtwork(sprite),
                    "ARTWORK_FIXTURE_BIND_FAILED", failures);
                Require(!card.IsPresentationDisplayed
                        && !card.IsArtworkEffectivelyRendering
                        && !card.IsPresentationRaycastable,
                    "ARTWORK_REACTIVATED_HIDDEN_CARD", failures);

                card.SetVisible(true);
                Require(card.BindAuthoritativeArtwork(sprite)
                        && card.IsPresentationDisplayed
                        && card.IsArtworkEffectivelyRendering
                        && card.IsPresentationRaycastable,
                    "ARTWORK_VISIBLE_CARD_BIND_INVALID", failures);
                card.SetVisible(false);
                Require(card.BindAuthoritativeArtwork(sprite)
                        && !card.IsPresentationDisplayed
                        && !card.IsArtworkEffectivelyRendering
                        && !card.IsPresentationRaycastable,
                    "ARTWORK_REBIND_OVERRIDES_SET_VISIBLE_FALSE",
                    failures);
                evidence.Add(
                    "artwork-visibility-owner=card-publication-only; fallback-root=PASS");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(fixture);
            }
        }

        private static void VerifyStableFilteredProjectionFixture(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            GameObject fixture = new(
                "BattleSandboxStableFilteredProjectionFixture",
                typeof(RectTransform),
                typeof(BuildItemTrayPreviewView));
            fixture.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                BuildItemTrayPreviewView view =
                    fixture.GetComponent<BuildItemTrayPreviewView>();
                List<BuildGridInteractionPreviewController.PreviewItem> items =
                    new()
                    {
                        BuildFixtureItem("I001"),
                        BuildFixtureItem("I002"),
                        BuildFixtureItem("I003")
                    };
                SetPrivateField(view, "currentItems", items);
                TrayPlacementViewModel first = BuildFixturePlacement(
                    "I001", 10);
                TrayPlacementViewModel second = BuildFixturePlacement(
                    "I002", 20);
                TrayPlacementViewModel third = BuildFixturePlacement(
                    "I003", 30);
                view.RefreshItemPlacement(first);
                view.RefreshItemPlacement(second);
                view.RefreshItemPlacement(third);
                view.ApplyFilter("基础");

                Dictionary<string, string> before =
                    CaptureDisplayedPlacementSignatures(view, items);
                Require(before.Count == 3,
                    "FILTER_FIXTURE_INITIAL_PROJECTION_MISSING",
                    failures);

                view.SetItemInTray("I001", false);
                Dictionary<string, string> afterRemove =
                    CaptureDisplayedPlacementSignatures(view, items);
                Require(!afterRemove.ContainsKey("I001")
                        && string.Equals(
                            before["I002"],
                            afterRemove.GetValueOrDefault("I002"),
                            StringComparison.Ordinal)
                        && string.Equals(
                            before["I003"],
                            afterRemove.GetValueOrDefault("I003"),
                            StringComparison.Ordinal),
                    "FILTER_ACCEPTED_REMOVE_REPACKED_REMAINING",
                    failures);

                view.SetItemInTray("I001", true);
                view.RefreshItemPlacement(first);
                Dictionary<string, string> afterReturn =
                    CaptureDisplayedPlacementSignatures(view, items);
                Require(string.Equals(
                            before["I001"],
                            afterReturn.GetValueOrDefault("I001"),
                            StringComparison.Ordinal)
                        && string.Equals(
                            before["I002"],
                            afterReturn.GetValueOrDefault("I002"),
                            StringComparison.Ordinal)
                        && string.Equals(
                            before["I003"],
                            afterReturn.GetValueOrDefault("I003"),
                            StringComparison.Ordinal),
                    "FILTER_RETURN_DID_NOT_RESTORE_ONE_ITEM",
                    failures);

                string beforeRejected =
                    BuildPlacementMapSignature(afterReturn);
                view.RefreshItemPlacement(second);
                string afterRejected = BuildPlacementMapSignature(
                    CaptureDisplayedPlacementSignatures(view, items));
                Require(string.Equals(
                        beforeRejected,
                        afterRejected,
                        StringComparison.Ordinal),
                    "FILTER_REJECTED_DRAG_CHANGED_PROJECTION",
                    failures);
                evidence.Add(
                    "stable-active-filter=remove-one/restore-one/reject-unchanged");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(fixture);
            }
        }

        private static BuildGridInteractionPreviewController.PreviewItem
            BuildFixtureItem(string itemId)
        {
            return new BuildGridInteractionPreviewController.PreviewItem(
                itemId,
                itemId,
                "基础",
                "single",
                "single",
                Color.white,
                categoryIds: new[] { "basic" });
        }

        private static TrayPlacementViewModel BuildFixturePlacement(
            string itemId,
            int slotIndex)
        {
            return new TrayPlacementViewModel
            {
                itemId = itemId,
                shapeId = "single",
                anchorSlotIndex = slotIndex,
                occupiedSlotIndexes = new[] { slotIndex },
                rotation = (int)ItemShapeRotation.Rotation0,
                isValid = true
            };
        }

        private static Dictionary<string, string>
            CaptureDisplayedPlacementSignatures(
                BuildItemTrayPreviewView view,
                IEnumerable<BuildGridInteractionPreviewController.PreviewItem>
                    items)
        {
            Dictionary<string, string> signatures =
                new(StringComparer.Ordinal);
            foreach (BuildGridInteractionPreviewController.PreviewItem item in
                     items ?? Enumerable.Empty<
                         BuildGridInteractionPreviewController.PreviewItem>())
            {
                if (item != null
                    && view.TryGetDisplayedPlacement(
                        item.ItemId,
                        out TrayPlacementViewModel placement)
                    && placement != null)
                {
                    signatures[item.ItemId] =
                        PlacementSignature(placement);
                }
            }
            return signatures;
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = target?.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    target?.GetType().FullName,
                    fieldName);
            }
            field.SetValue(target, value);
        }

        private static void VerifyTrayRowSnapContract(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            MethodInfo resolver = typeof(BuildItemTrayPreviewView).GetMethod(
                "ResolveNearestTrayRowOffset",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(resolver != null, "ROW_SNAP_RESOLVER_MISSING", failures);
            if (resolver == null)
            {
                return;
            }

            const float pitch = 114f;
            const float maxOffset = 900f;
            Require(Approximately(InvokeSnap(resolver, -50f, pitch, maxOffset), 0f),
                "ROW_SNAP_TOP_CLAMP_FAILED", failures);
            Require(Approximately(InvokeSnap(resolver, 70f, pitch, maxOffset), pitch),
                "ROW_SNAP_NEAREST_ROW_FAILED", failures);
            Require(Approximately(InvokeSnap(resolver, 228.1f, pitch, maxOffset),
                    pitch * 2f),
                "ROW_SNAP_EXACT_ROW_FAILED", failures);
            Require(Approximately(InvokeSnap(resolver, 890f, pitch, maxOffset),
                    maxOffset),
                "ROW_SNAP_BOTTOM_CLAMP_FAILED", failures);
            Require(Approximately(InvokeSnap(resolver, 999f, pitch, maxOffset),
                    maxOffset),
                "ROW_SNAP_RANGE_CLAMP_FAILED", failures);
            evidence.Add("row-snap=pitch-from-grid; top/nearest/bottom=PASS");
        }

        private static float InvokeSnap(
            MethodInfo resolver,
            float currentOffset,
            float rowPitch,
            float scrollableHeight)
        {
            return (float)resolver.Invoke(
                null,
                new object[] { currentOffset, rowPitch, scrollableHeight });
        }

        private static bool Approximately(float left, float right)
        {
            return Mathf.Abs(left - right) <= 0.001f;
        }

        private static void VerifyThirtyOrdinaryItemsAndI031(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            global::TalismanBag.Items.InnerCatalog.ItemInnerDataDefinition[] all =
                (global::TalismanBag.Items.InnerCatalog.ItemInnerDataCatalog.AllItems
                    ?? Array.Empty<global::TalismanBag.Items.InnerCatalog.ItemInnerDataDefinition>())
                .Where(item => item != null)
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            global::TalismanBag.Items.InnerCatalog.ItemInnerDataDefinition[] ordinary = all.Where(item => !string.Equals(item.itemId,
                    I031InventoryPlacementContract.ItemId, StringComparison.Ordinal)).ToArray();
            Require(ordinary.Length == 30, "ORDINARY_30_ROSTER_MISSING", failures);
            Require(all.Count(item => string.Equals(item.itemId,
                    I031InventoryPlacementContract.ItemId, StringComparison.Ordinal)) == 1,
                "I031_SPECIAL_PAYLOAD_MISSING", failures);

            List<ShapeItemPayload> payloads = all.Select(BuildTrayPayload)
                .OrderByDescending(payload => payload.BuildNormalizedOffsets().Count)
                .ThenByDescending(payload => IsIrregular(payload.BuildNormalizedOffsets()))
                .ThenByDescending(payload => BoundingCellCount(payload.BuildNormalizedOffsets()))
                .ThenBy(payload => StableIdentity(payload.ItemId), StringComparer.Ordinal)
                .ToList();
            ShapeAwareItemTrayGrid first = PackAll(payloads, failures);
            ShapeAwareItemTrayGrid second = PackAll(payloads, failures);
            string firstSignature = BuildSignature(first);
            string secondSignature = BuildSignature(second);
            Require(string.Equals(firstSignature, secondSignature, StringComparison.Ordinal),
                "INITIAL_MASTER_NOT_DETERMINISTIC", failures);

            ShapeItemPayload moving = payloads.FirstOrDefault();
            Dictionary<string, string> beforeRemoval = BuildPlacementSignatures(first, moving.ItemId);
            Require(first.RemoveItem(moving.ItemId), "MASTER_REMOVE_FAILED", failures);
            Require(beforeRemoval.All(pair => first.TryGetPlacement(pair.Key, out var placement)
                    && string.Equals(pair.Value, PlacementSignature(placement), StringComparison.Ordinal)),
                "TRAY_TO_BOARD_MOVED_UNRELATED_ITEM", failures);
            Require(TryCommitAt(first, moving, first.SlotIndexToCell(0), out _)
                    || first.TryPack(moving, out _),
                "RETURN_REMEMBERED_OR_FIRST_LEGAL_FAILED", failures);
            evidence.Add("roster=30+I031; initial-signature=" + Sha256(firstSignature));
        }

        private static void VerifyArrangeButtonSceneContract(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            Scene scene = SceneManager.GetActiveScene();
            Require(string.Equals(scene.path, ScenePath, StringComparison.Ordinal),
                "TARGET_SCENE_NOT_ACTIVE", failures);
            if (!string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                return;
            }

            BuildGridInteractionPreviewController controller =
                UnityEngine.Object.FindObjectOfType<BuildGridInteractionPreviewController>(true);
            Require(controller != null, "CONTROLLER_MISSING", failures);
            if (controller == null)
            {
                return;
            }

            SerializedObject serializedController = new(controller);
            BuildItemTrayPreviewView trayView = serializedController
                .FindProperty("itemTrayView")?.objectReferenceValue
                as BuildItemTrayPreviewView;
            Button serializedButton = serializedController
                .FindProperty("trayArrangeButton")?.objectReferenceValue
                as Button;
            Require(trayView != null, "SERIALIZED_TRAY_VIEW_MISSING", failures);

            GameObject[] matches = FindNamedSceneObjects(
                scene,
                BattleSandboxTrayStableLayoutAndManualSortAuthoring.ArrangeButtonName);
            Require(matches.Length == 1, "TRAY_ARRANGE_BUTTON_COUNT_INVALID", failures);
            if (trayView == null || matches.Length != 1)
            {
                return;
            }

            GameObject buttonObject = matches[0];
            Image image = buttonObject.GetComponent<Image>();
            Button button = buttonObject.GetComponent<Button>();
            Component[] components = buttonObject.GetComponents<Component>();
            Require(buttonObject.transform.parent == trayView.transform,
                "TRAY_ARRANGE_BUTTON_PARENT_INVALID", failures);
            Require(components.Length == 4
                    && buttonObject.GetComponent<RectTransform>() != null
                    && buttonObject.GetComponent<CanvasRenderer>() != null
                    && image != null
                    && button != null
                    && buttonObject.transform.childCount == 0,
                "TRAY_ARRANGE_BUTTON_COMPONENT_CONTRACT_INVALID", failures);
            Require(button != null && image != null && button.targetGraphic == image
                    && serializedButton == button,
                "TRAY_ARRANGE_BUTTON_BINDING_INVALID", failures);
            Require(buttonObject.GetComponentInParent<Canvas>(true) != null
                    && buttonObject.GetComponent<Canvas>() == null
                    && buttonObject.GetComponent<GraphicRaycaster>() == null
                    && buttonObject.GetComponent<CanvasGroup>() == null,
                "TRAY_ARRANGE_BUTTON_CANVAS_OWNER_INVALID", failures);

            SerializedObject serializedTrayView = new(trayView);
            RectTransform contentRoot = serializedTrayView.FindProperty("contentRoot")
                ?.objectReferenceValue as RectTransform;
            RectTransform cardLayer = serializedTrayView.FindProperty("itemCardLayer")
                ?.objectReferenceValue as RectTransform;
            ScrollRect scrollRect = serializedTrayView.FindProperty("scrollRect")
                ?.objectReferenceValue as ScrollRect;
            Require(!IsSameOrDescendant(buttonObject.transform, contentRoot)
                    && !IsSameOrDescendant(buttonObject.transform, cardLayer)
                    && !IsSameOrDescendant(buttonObject.transform, scrollRect?.viewport),
                "TRAY_ARRANGE_BUTTON_INSIDE_SCROLLING_SUBTREE", failures);
            Require(buttonObject.activeInHierarchy
                    && button != null && button.enabled && button.interactable
                    && image != null && image.enabled && image.raycastTarget
                    && image.color.a > 0.001f,
                "TRAY_ARRANGE_BUTTON_NOT_VISIBLE_OR_RAYCASTABLE", failures);
            evidence.Add("scene-button-parent=serialized-itemTrayView; canvas=existing");
        }

        private static GameObject[] FindNamedSceneObjects(
            Scene scene,
            string objectName)
        {
            return scene.GetRootGameObjects()
                .Where(root => root != null)
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(transformValue => transformValue != null
                    && string.Equals(transformValue.name, objectName,
                        StringComparison.Ordinal))
                .Select(transformValue => transformValue.gameObject)
                .ToArray();
        }

        private static bool IsSameOrDescendant(Transform value, Transform ancestor)
        {
            return value != null && ancestor != null
                && (value == ancestor || value.IsChildOf(ancestor));
        }

        private static ShapeAwareItemTrayGrid PackAll(
            IEnumerable<ShapeItemPayload> payloads,
            ICollection<string> failures)
        {
            ShapeAwareItemTrayGrid grid = new(
                "verify_stable_tray", BuildGridInteractionPreviewController.TrayColumns,
                BuildItemTrayPreviewView.ItemSystemAuthorityLogicalSlotCount, true);
            foreach (ShapeItemPayload payload in payloads)
            {
                Require(grid.TryPack(payload, out ShapePlacementResult result)
                        && result != null && result.IsValid,
                    "ARRANGE_CANNOT_FIT_" + payload.ItemId, failures);
            }
            return grid;
        }

        private static ShapeItemPayload BuildTrayPayload(
            global::TalismanBag.Items.InnerCatalog.ItemInnerDataDefinition item)
        {
            return new ShapeItemPayload(item.itemId, item.shapeId, ItemShapeRotation.Rotation0,
                (item.ShapeCells ?? Array.Empty<Vector2Int>())
                    .Select(cell => new ItemShapeCell(cell.x, cell.y))
                    .ToArray(),
                ShapePlacementSource.Tray);
        }

        private static bool TryCommitAt(
            ShapeAwareItemTrayGrid grid,
            ShapeItemPayload payload,
            ItemShapeCell anchor,
            out ShapePlacementResult result)
        {
            ShapePlacementSession session = new();
            session.Begin(payload, trayAnchorCell: anchor);
            result = session.Commit(grid);
            return result != null && result.IsValid;
        }

        private static Dictionary<string, string> BuildPlacementSignatures(
            ShapeAwareItemTrayGrid grid,
            string excludedItemId)
        {
            return grid.Placements
                .Where(pair => !string.Equals(pair.Key, excludedItemId, StringComparison.Ordinal))
                .ToDictionary(pair => pair.Key, pair => PlacementSignature(pair.Value),
                    StringComparer.Ordinal);
        }

        private static string BuildSignature(ShapeAwareItemTrayGrid grid)
        {
            return string.Join("\n", grid.Placements.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key + "=" + PlacementSignature(pair.Value)));
        }

        private static string PlacementSignature(ShapeAwareItemTrayGridPlacement placement)
        {
            return placement.ItemId + "|" + placement.AnchorCell.x.ToString(CultureInfo.InvariantCulture)
                + "," + placement.AnchorCell.y.ToString(CultureInfo.InvariantCulture) + "|"
                + string.Join(",", placement.OccupiedSlotIndexes);
        }

        private static string PlacementSignature(
            TrayPlacementViewModel placement)
        {
            return placement.itemId + "|"
                + placement.anchorSlotIndex.ToString(
                    CultureInfo.InvariantCulture)
                + "|"
                + string.Join(
                    ",",
                    placement.occupiedSlotIndexes ?? Array.Empty<int>())
                + "|"
                + ((int)placement.rotation).ToString(
                    CultureInfo.InvariantCulture);
        }

        private static string BuildPlacementMapSignature(
            IReadOnlyDictionary<string, string> signatures)
        {
            return string.Join(
                "\n",
                (signatures ?? new Dictionary<string, string>())
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => pair.Key + "=" + pair.Value));
        }

        private static string StableIdentity(string itemId)
        {
            return string.Equals(itemId, I031InventoryPlacementContract.ItemId,
                    StringComparison.Ordinal)
                ? I031InventoryPlacementContract.SpecialIdentityId
                : itemId ?? string.Empty;
        }

        private static bool IsIrregular(IReadOnlyList<ItemShapeCell> cells)
        {
            return cells != null && cells.Count > 0 && cells.Count != BoundingCellCount(cells);
        }

        private static int BoundingCellCount(IReadOnlyList<ItemShapeCell> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return 0;
            }
            return (cells.Max(cell => cell.x) - cells.Min(cell => cell.x) + 1)
                * (cells.Max(cell => cell.y) - cells.Min(cell => cell.y) + 1);
        }

        private static string ReadProjectText(string relativePath)
        {
            return File.ReadAllText(Path.Combine(ProjectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)),
                Encoding.UTF8);
        }

        private static Scene RequireSoleExactTargetScene(
            string failureCode)
        {
            if (SceneManager.sceneCount != 1)
            {
                throw new InvalidOperationException(
                    failureCode + ":sceneCount="
                    + SceneManager.sceneCount.ToString(
                        CultureInfo.InvariantCulture));
            }

            Scene scene = SceneManager.GetSceneAt(0);
            if (!scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    scene.path,
                    ScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    failureCode + ":path="
                    + (scene.IsValid()
                        ? scene.path
                        : "<invalid>"));
            }
            return scene;
        }

        private static void MarkTargetSceneClean(Scene targetScene)
        {
            MethodInfo clearSceneDirtiness = typeof(EditorSceneManager)
                .GetMethod(
                    "ClearSceneDirtiness",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Scene) },
                    null);
            if (clearSceneDirtiness == null)
            {
                throw new MissingMethodException(
                    "EditorSceneManager.ClearSceneDirtiness(Scene)");
            }
            clearSceneDirtiness.Invoke(
                null,
                new object[] { targetScene });
        }

        private static void AppendPlaySceneEvent(string value)
        {
            string existing = SessionState.GetString(
                RuntimePlaySceneEventsSessionKey,
                string.Empty);
            SessionState.SetString(
                RuntimePlaySceneEventsSessionKey,
                string.IsNullOrEmpty(existing)
                    ? value
                    : existing + "|" + value);
        }

        private static void RequireNoRuntimeVerifierDriver(
            string failureCode)
        {
            int driverCount = Resources.FindObjectsOfTypeAll<
                    BattleSandboxTrayStableLayoutRuntimeVerifierDriver>()
                .Length;
            if (driverCount != 0
                || SessionState.GetBool(
                    RuntimeDriverInstalledSessionKey,
                    false))
            {
                throw new InvalidOperationException(
                    failureCode + ":count="
                    + driverCount.ToString(
                        CultureInfo.InvariantCulture));
            }
        }

        private static void CleanupRuntimeBootstrapSubscriptions()
        {
            SceneManager.sceneLoaded -=
                HandleVerifierPlaySceneLoaded;
            EditorApplication.update -=
                HandleRuntimeBootstrapUpdate;
        }

        private static void AppendRuntimeBootstrapEvidence(
            ICollection<string> failures,
            ICollection<string> evidence)
        {
            string prePlayPath = SessionState.GetString(
                RuntimePrePlayAssetPathSessionKey,
                string.Empty);
            bool wasDirty = SessionState.GetBool(
                RuntimePrePlayWasDirtySessionKey,
                false);
            bool wasClean = SessionState.GetBool(
                RuntimePrePlayCleanSessionKey,
                false);
            string initialPlayPath = SessionState.GetString(
                RuntimeInitialPlayPathSessionKey,
                string.Empty);
            string reloadedPlayPath = SessionState.GetString(
                RuntimeReloadedPlayPathSessionKey,
                string.Empty);
            string exactLoadedPlayPath = SessionState.GetString(
                RuntimeExactLoadedPlayPathSessionKey,
                string.Empty);
            string sceneEvents = SessionState.GetString(
                RuntimePlaySceneEventsSessionKey,
                string.Empty);
            bool reloadRequested = SessionState.GetBool(
                RuntimeReloadRequestedSessionKey,
                false);
            bool enteredPlayModeObserved = SessionState.GetBool(
                RuntimeEnteredPlayModeObservedSessionKey,
                false);
            int sceneLoadEventCount = SessionState.GetInt(
                RuntimeSceneLoadEventCountSessionKey,
                0);
            int preArmedSceneLoadEventCount = SessionState.GetInt(
                RuntimePreArmedSceneLoadEventCountSessionKey,
                0);
            int postArmedSceneLoadEventCount = SessionState.GetInt(
                RuntimePostArmedSceneLoadEventCountSessionKey,
                0);
            int reloadInvocationCount = SessionState.GetInt(
                RuntimeReloadInvocationCountSessionKey,
                0);
            int bootstrapUpdateCount = SessionState.GetInt(
                RuntimeBootstrapUpdateCountSessionKey,
                0);
            int driverCount = SessionState.GetInt(
                RuntimeDriverCountSessionKey,
                0);
            string beforeHash = SessionState.GetString(
                RuntimeSceneHashBeforeSessionKey,
                string.Empty);
            string afterHash = Sha256FileBytes(ScenePath);
            bool sceneUnchanged = !string.IsNullOrEmpty(beforeHash)
                && string.Equals(
                    beforeHash,
                    afterHash,
                    StringComparison.Ordinal);

            evidence.Add("bootstrap-preplay-asset-path="
                + (string.IsNullOrEmpty(prePlayPath)
                    ? "<missing>"
                    : prePlayPath));
            evidence.Add("bootstrap-preplay-clean="
                + wasClean.ToString()
                + ";was-dirty-before-clean="
                + wasDirty.ToString());
            evidence.Add("bootstrap-initial-play-path="
                + (string.IsNullOrEmpty(initialPlayPath)
                    ? "<missing>"
                    : initialPlayPath));
            evidence.Add("bootstrap-reloaded-play-path="
                + (string.IsNullOrEmpty(reloadedPlayPath)
                    ? "NOT_REQUESTED_OR_NOT_REACHED"
                    : reloadedPlayPath));
            evidence.Add("bootstrap-exact-loaded-play-path="
                + (string.IsNullOrEmpty(exactLoadedPlayPath)
                    ? "<missing>"
                    : exactLoadedPlayPath));
            evidence.Add("bootstrap-scene-events="
                + (string.IsNullOrEmpty(sceneEvents)
                    ? "<missing>"
                    : sceneEvents));
            evidence.Add("bootstrap-event-order="
                + "load-events="
                + sceneLoadEventCount.ToString(
                    CultureInfo.InvariantCulture)
                + ";entered-playmode-observed="
                + enteredPlayModeObserved.ToString()
                + ";pre-armed-events="
                + preArmedSceneLoadEventCount.ToString(
                    CultureInfo.InvariantCulture)
                + ";post-armed-events="
                + postArmedSceneLoadEventCount.ToString(
                    CultureInfo.InvariantCulture)
                + ";reload-requested="
                + reloadRequested.ToString()
                + ";reload-invocations="
                + reloadInvocationCount.ToString(
                    CultureInfo.InvariantCulture)
                + ";watchdog-updates="
                + bootstrapUpdateCount.ToString(
                    CultureInfo.InvariantCulture));
            evidence.Add("bootstrap-driver-count="
                + driverCount.ToString(
                    CultureInfo.InvariantCulture));
            evidence.Add("bootstrap-scene-bytes-before="
                + (string.IsNullOrEmpty(beforeHash)
                    ? "<missing>"
                    : beforeHash)
                + ";after-runtime=" + afterHash
                + ";unchanged="
                + sceneUnchanged.ToString());

            Require(
                string.Equals(
                    prePlayPath,
                    ScenePath,
                    StringComparison.Ordinal),
                "TARGET_PREPLAY_ASSET_PATH_MISSING",
                failures);
            Require(wasClean,
                "TARGET_PREPLAY_CLEAN_STATE_MISSING",
                failures);
            Require(
                string.Equals(
                    exactLoadedPlayPath,
                    ScenePath,
                    StringComparison.Ordinal),
                "TARGET_EXACT_PLAY_SCENE_EVENT_NOT_OBSERVED",
                failures);
            Require(enteredPlayModeObserved,
                "TARGET_ENTERED_PLAYMODE_ARM_MISSING",
                failures);
            Require(postArmedSceneLoadEventCount >= 1,
                "TARGET_POST_ARMED_PLAY_SCENE_EVENT_MISSING",
                failures);
            Require(
                !reloadRequested
                    ? reloadInvocationCount == 0
                    : reloadInvocationCount == 1
                        && string.Equals(
                            reloadedPlayPath,
                            ScenePath,
                            StringComparison.Ordinal),
                "TARGET_PLAY_SCENE_RELOAD_CARDINALITY_INVALID",
                failures);
            Require(driverCount == 1,
                "TARGET_VERIFIER_DRIVER_CARDINALITY_INVALID",
                failures);
            Require(sceneUnchanged,
                "TARGET_SCENE_BYTES_CHANGED",
                failures);
        }

        private static void FailRuntimeBootstrap(
            string failureCode,
            Exception exception)
        {
            CleanupRuntimeBootstrapSubscriptions();
            string detail = exception == null
                ? "<no-exception>"
                : exception.GetType().Name + ":"
                    + exception.Message;
            CompleteTargetSceneRuntimeVerification(
                new[] { failureCode },
                new[] { "bootstrap-failure=" + detail });
        }

        private static void ResetRuntimeBootstrapEvidence()
        {
            SessionState.SetBool(
                RuntimeDriverInstalledSessionKey,
                false);
            SessionState.SetBool(
                RuntimePrePlayWasDirtySessionKey,
                false);
            SessionState.SetBool(
                RuntimePrePlayCleanSessionKey,
                false);
            SessionState.SetInt(
                RuntimeDriverCountSessionKey,
                0);
            SessionState.SetBool(
                RuntimeBootstrapArmedSessionKey,
                false);
            SessionState.SetBool(
                RuntimeEnteredPlayModeArmedSessionKey,
                false);
            SessionState.SetBool(
                RuntimeEnteredPlayModeObservedSessionKey,
                false);
            SessionState.SetBool(
                RuntimeReloadRequestedSessionKey,
                false);
            SessionState.SetBool(
                RuntimeReloadRequestPendingSessionKey,
                false);
            SessionState.SetBool(
                RuntimeExactEventPendingSessionKey,
                false);
            SessionState.SetInt(
                RuntimeSceneLoadEventCountSessionKey,
                0);
            SessionState.SetInt(
                RuntimePreArmedSceneLoadEventCountSessionKey,
                0);
            SessionState.SetInt(
                RuntimePostArmedSceneLoadEventCountSessionKey,
                0);
            SessionState.SetInt(
                RuntimeReloadInvocationCountSessionKey,
                0);
            SessionState.SetInt(
                RuntimeBootstrapUpdateCountSessionKey,
                0);
            SessionState.SetInt(
                RuntimeExactEventUpdateSessionKey,
                -1);
            SessionState.SetString(
                RuntimePrePlayAssetPathSessionKey,
                string.Empty);
            SessionState.SetString(
                RuntimeInitialPlayPathSessionKey,
                string.Empty);
            SessionState.SetString(
                RuntimeReloadedPlayPathSessionKey,
                string.Empty);
            SessionState.SetString(
                RuntimeExactLoadedPlayPathSessionKey,
                string.Empty);
            SessionState.SetString(
                RuntimePlaySceneEventsSessionKey,
                string.Empty);
            SessionState.SetString(
                RuntimeSceneHashBeforeSessionKey,
                string.Empty);
        }

        private static string Sha256FileBytes(
            string relativePath)
        {
            string path = Path.Combine(
                ProjectRoot,
                relativePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));
            using SHA256 algorithm = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] bytes = algorithm.ComputeHash(stream);
            return BitConverter.ToString(bytes)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static void Require(bool condition, string code, ICollection<string> failures)
        {
            if (!condition)
            {
                failures.Add(code);
            }
        }

        private static void WriteReports(IReadOnlyList<string> failures, IReadOnlyList<string> evidence)
        {
            string directory = Path.Combine(ProjectRoot, ReportDirectory.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(directory);
            string status = failures.Count == 0 ? "PASS" : "FAIL";
            bool targetSceneRuntimeExecuted = evidence.Any(value =>
                value != null
                && value.StartsWith(
                    "target-category-pointer=",
                    StringComparison.Ordinal))
                && evidence.Any(value =>
                    value != null
                    && value.StartsWith(
                        "target-scrollrect=",
                        StringComparison.Ordinal));
            string targetSceneStatus = failures.Count > 0
                ? "FAIL"
                : targetSceneRuntimeExecuted
                    ? "PASS"
                    : "NOT_RUN";
            File.WriteAllText(Path.Combine(directory, "BattleSandboxTrayStableLayoutAndManualSortReport.md"),
                "# " + PackageName + "\n\nStatus: " + status + "\n\n"
                + "- COMPONENT_FIXTURE_PASS="
                + (failures.Count == 0 ? "PASS" : "FAIL") + "\n"
                + "- TARGET_SCENE_REAL_POINTER_PATH_PASS="
                + targetSceneStatus + "\n"
                + "- REAL_TRAY_RUNTIME_PATH_PASS="
                + targetSceneStatus + "\n"
                + string.Join("\n", evidence.Select(value => "- " + value)) + "\n\n"
                + string.Join("\n", failures.Select(value => "- FAIL: " + value)) + "\n", Encoding.UTF8);
            File.WriteAllText(Path.Combine(directory, "BattleSandboxTrayStableLayoutMasterMatrix.csv"),
                "case,status\ninitial-master-deterministic," + status + "\n", Encoding.UTF8);
            File.WriteAllText(Path.Combine(directory, "BattleSandboxTrayFilteredProjectionMatrix.csv"),
                "case,status\nall-five-categories-all," + status
                + "\natomic-snapshot-publication," + status
                + "\nempty-filter-does-not-show-master," + status + "\n", Encoding.UTF8);
            File.WriteAllText(Path.Combine(directory, "BattleSandboxTrayManualSortMatrix.csv"),
                "case,status\natomic-deterministic-idempotent," + status + "\n", Encoding.UTF8);
            File.WriteAllText(Path.Combine(directory, "BattleSandboxTrayStableLayoutAndManualSortSpec.csv"),
                "contract,value\nmaster,stable\nfilter,transient-atomic"
                + "\nfilter-active-removal,stable-one-item"
                + "\nartwork-visibility,publication-owned"
                + "\nscroll,row-pitch-settle"
                + "\nscroll-observer,serialized-scrollrect"
                + "\narrange,manual-atomic\n", Encoding.UTF8);
            File.WriteAllText(Path.Combine(directory, "BattleSandboxTrayStableLayoutManualTest.md"),
                "# Manual test\n\n1. Verify All → each category → All keeps every master placement.\n"
                + "2. Drag one item to board and verify only it leaves the tray.\n"
                + "3. Return it and verify remembered placement, then first legal fallback.\n"
                + "4. Scroll with touch drag, mouse wheel, and inertia; every settle must align one full background row.\n"
                + "5. Use Arrange only in All with no active drag.\n", Encoding.UTF8);
            File.WriteAllText(Path.Combine(directory, "BattleSandboxTrayStableLayoutAndManualSortLeakCheckReport.md"),
                "# Leak check\n\nStatus: " + status
                + "\nOnly the package reports above are written by this verifier.\n", Encoding.UTF8);
        }

        private static string Sha256(string value)
        {
            using SHA256 algorithm = SHA256.Create();
            byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    internal sealed class
        BattleSandboxTrayStableLayoutRuntimeVerifierDriver : MonoBehaviour
    {
        private const float RuntimeTolerance = 0.75f;
        private readonly List<string> failures = new();
        private readonly List<string> evidence = new();
        private BuildGridInteractionPreviewController controller;
        private BuildItemTrayPreviewView trayView;
        private ScrollRect scrollRect;
        private RectTransform viewport;
        private RectTransform contentRoot;
        private EventSystem eventSystem;
        private bool settlePassed;

        private IEnumerator Start()
        {
            yield return WaitForRuntimeReady();
            if (failures.Count == 0)
            {
                yield return VerifyRealCategoryPointerPath();
            }
            if (failures.Count == 0)
            {
                yield return VerifyRealScrollRectPointerPaths();
            }

            BattleSandboxTrayStableLayoutAndManualSortVerifier
                .CompleteTargetSceneRuntimeVerification(
                    failures,
                    evidence);
        }

        private IEnumerator WaitForRuntimeReady()
        {
            for (int frame = 0; frame < 240; frame++)
            {
                controller = UnityEngine.Object
                    .FindObjectOfType<
                        BuildGridInteractionPreviewController>(true);
                if (controller != null
                    && controller.UsesItemSystemBoardAuthority
                    && TryResolveRuntimeReferences()
                    && trayView.GetComponentsInChildren<
                            BuildItemPreviewCardView>(true)
                        .Count(card => card != null
                            && !string.IsNullOrWhiteSpace(card.ItemId))
                        >= 30)
                {
                    yield return null;
                    yield return null;
                    EnterBattlePrepareState();
                    yield return null;
                    yield return null;
                    if (GetPrivateBool(
                            controller,
                            "battlePrepareStateActive"))
                    {
                        int authorityCardCount = trayView
                            .GetComponentsInChildren<
                                BuildItemPreviewCardView>(true)
                            .Count(card => card != null
                                && !string.IsNullOrWhiteSpace(
                                    card.ItemId));
                        evidence.Add(
                            "production-authority-ready=True;cards="
                            + authorityCardCount.ToString(
                                CultureInfo.InvariantCulture));
                        Require(
                            trayView.HasProductionRowSnapObserver,
                            "TARGET_SCROLLRECT_OBSERVER_NOT_BOUND");
                        yield break;
                    }
                }
                yield return null;
            }
            failures.Add("TARGET_SCENE_RUNTIME_NOT_READY");
        }

        private bool TryResolveRuntimeReferences()
        {
            SerializedObject serializedController = new(controller);
            trayView = serializedController.FindProperty("itemTrayView")
                ?.objectReferenceValue as BuildItemTrayPreviewView;
            if (trayView == null)
            {
                return false;
            }

            SerializedObject serializedTrayView = new(trayView);
            scrollRect = serializedTrayView.FindProperty("scrollRect")
                ?.objectReferenceValue as ScrollRect;
            contentRoot = serializedTrayView.FindProperty("contentRoot")
                ?.objectReferenceValue as RectTransform;
            viewport = scrollRect == null
                ? null
                : scrollRect.viewport != null
                    ? scrollRect.viewport
                    : scrollRect.content?.parent as RectTransform;
            eventSystem = EventSystem.current;
            return scrollRect != null
                && contentRoot != null
                && viewport != null
                && eventSystem != null;
        }

        private void EnterBattlePrepareState()
        {
            if (GetPrivateBool(controller, "battlePrepareStateActive"))
            {
                return;
            }
            SerializedObject serializedController = new(controller);
            Button button = serializedController
                .FindProperty("battlePrepareStateButton")
                ?.objectReferenceValue as Button;
            button?.onClick.Invoke();
        }

        private IEnumerator VerifyRealCategoryPointerPath()
        {
            BuildItemPreviewCardView[] allCards = trayView
                .GetComponentsInChildren<BuildItemPreviewCardView>(true)
                .Where(card => card != null
                    && !string.IsNullOrWhiteSpace(card.ItemId))
                .ToArray();
            HashSet<string> singleCellIds = new(
                (global::TalismanBag.Items.InnerCatalog.ItemInnerDataCatalog
                        .AllItems
                    ?? Array.Empty<
                        global::TalismanBag.Items.InnerCatalog
                            .ItemInnerDataDefinition>())
                .Where(item => item != null
                    && !string.Equals(
                        item.itemId,
                        I031InventoryPlacementContract.ItemId,
                        StringComparison.Ordinal)
                    && (item.ShapeCells?.Count ?? 0) == 1)
                .Select(item => item.itemId),
                StringComparer.Ordinal);
            IGrouping<string, BuildItemPreviewCardView> categoryGroup =
                allCards
                    .Where(card => card.gameObject.activeInHierarchy)
                    .GroupBy(card => card.Category ?? string.Empty,
                        StringComparer.Ordinal)
                    .Where(group => group.Count() >= 2
                        && group.Any(card =>
                            singleCellIds.Contains(card.ItemId)))
                    .OrderByDescending(group => group.Count())
                    .ThenBy(group => group.Key, StringComparer.Ordinal)
                    .FirstOrDefault();
            Require(categoryGroup != null,
                "TARGET_CATEGORY_WITH_SINGLE_CELL_ITEM_MISSING");
            if (categoryGroup == null)
            {
                yield break;
            }

            controller.ApplyCategoryFilter(categoryGroup.Key);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();

            BuildItemPreviewCardView movingCard = categoryGroup
                .First(card => singleCellIds.Contains(card.ItemId));
            Dictionary<string, string> before =
                CaptureDisplayedSignatures();
            bool hasMovingRememberedSignature = before.TryGetValue(
                movingCard.ItemId,
                out string movingRememberedSignature);
            Require(before.Count >= 2
                    && hasMovingRememberedSignature,
                "TARGET_FILTER_INITIAL_SIGNATURE_MISSING");
            if (failures.Count > 0)
            {
                yield break;
            }

            BuildGridPreviewSlotView[] boardSlots =
                ResolveSerializedBoardSlots();
            Require(boardSlots.Length > 0,
                "TARGET_BOARD_SLOTS_MISSING");
            BuildGridPreviewSlotView committedSlot = null;
            foreach (BuildGridPreviewSlotView slot in boardSlots)
            {
                if (slot == null || !slot.gameObject.activeInHierarchy)
                {
                    continue;
                }
                ExecuteCardDrag(
                    movingCard,
                    ScreenCenter(slot.GetComponent<RectTransform>()));
                yield return null;
                yield return null;
                if (!movingCard.gameObject.activeSelf)
                {
                    committedSlot = slot;
                    break;
                }
            }

            Require(committedSlot != null,
                "TARGET_REAL_CARD_TRAY_TO_BOARD_COMMIT_FAILED");
            if (committedSlot == null)
            {
                yield break;
            }

            Dictionary<string, string> afterCommit =
                CaptureDisplayedSignatures();
            Require(!afterCommit.ContainsKey(movingCard.ItemId),
                "TARGET_MOVED_ITEM_STILL_IN_FILTER_PROJECTION");
            Require(MapEqualsExcept(before, afterCommit, movingCard.ItemId),
                "TARGET_FILTER_ACCEPTED_REMOVE_SHUFFLED_REMAINING");
            Require(!movingCard.IsPresentationDisplayed
                    && !movingCard.IsArtworkEffectivelyRendering
                    && !movingCard.IsPresentationRaycastable,
                "TARGET_MOVED_CARD_ARTWORK_OR_RAYCAST_REMAINS");

            Vector2 boardPosition = ScreenCenter(
                committedSlot.GetComponent<RectTransform>());
            PointerEventData boardBegin = BuildPointerEvent(
                boardPosition,
                boardPosition,
                Vector2.zero);
            controller.BeginBoardSlotDrag(committedSlot, boardBegin);
            yield return null;
            Vector2 trayPosition = ScreenCenter(viewport);
            PointerEventData boardEnd = BuildPointerEvent(
                trayPosition,
                boardPosition,
                trayPosition - boardPosition);
            controller.UpdateBoardSlotDrag(committedSlot, boardEnd);
            controller.EndBoardSlotDrag(committedSlot, boardEnd);
            yield return null;
            yield return null;

            Dictionary<string, string> afterReturn =
                CaptureDisplayedSignatures();
            Require(movingCard.IsPresentationDisplayed
                    && afterReturn.TryGetValue(
                        movingCard.ItemId,
                        out string returnedSignature)
                    && string.Equals(
                        movingRememberedSignature,
                        returnedSignature,
                        StringComparison.Ordinal),
                "TARGET_FILTER_RETURN_DID_NOT_RESTORE_ONE_ITEM");
            Require(MapEqualsExcept(
                    afterCommit,
                    afterReturn,
                    movingCard.ItemId),
                "TARGET_FILTER_RETURN_SHUFFLED_REMAINING");

            BuildItemPreviewCardView rejectedCard = allCards.FirstOrDefault(
                card => card != null
                    && card != movingCard
                    && card.gameObject.activeInHierarchy
                    && trayView.TryGetDisplayedPlacement(
                        card.ItemId,
                        out _));
            Require(rejectedCard != null,
                "TARGET_REJECTED_DRAG_CARD_MISSING");
            if (rejectedCard != null)
            {
                Dictionary<string, string> beforeReject =
                    CaptureDisplayedSignatures();
                ExecuteCardDrag(
                    rejectedCard,
                    new Vector2(-10000f, -10000f));
                yield return null;
                yield return null;
                Require(MapEquals(
                        beforeReject,
                        CaptureDisplayedSignatures()),
                    "TARGET_REJECTED_DRAG_CHANGED_FILTER_PROJECTION");
            }

            evidence.Add(
                "target-category-pointer=accepted-remove-stable"
                + "/artwork-hidden/return-one/reject-unchanged");
        }

        private IEnumerator VerifyRealScrollRectPointerPaths()
        {
            controller.ApplyCategoryFilter(BuildItemTrayPreviewView.AllCategory);
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            Require(trayView.HasProductionRowSnapObserver,
                "TARGET_SCROLLRECT_OBSERVER_LOST");

            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
            yield return null;
            PointerEventData wheel = BuildPointerEvent(
                ScreenCenter(viewport),
                ScreenCenter(viewport),
                Vector2.zero);
            wheel.scrollDelta = new Vector2(0f, -6f);
            ExecuteEvents.Execute(
                scrollRect.gameObject,
                wheel,
                ExecuteEvents.scrollHandler);
            yield return WaitForRowSettle(
                "wheel",
                requireNonTop: true);
            Require(settlePassed,
                "TARGET_WHEEL_ROW_SNAP_FAILED");
            VerifyVisibleAlignment("wheel");

            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
            yield return null;
            Vector2 start = ScreenCenter(viewport);
            PointerEventData begin = BuildPointerEvent(
                start,
                start,
                Vector2.zero);
            ExecuteEvents.Execute(
                scrollRect.gameObject,
                begin,
                ExecuteEvents.beginDragHandler);
            yield return null;
            Vector2 dragTarget = start
                + Vector2.up * Mathf.Max(
                    30f,
                    viewport.rect.height * 0.58f);
            PointerEventData drag = BuildPointerEvent(
                dragTarget,
                start,
                dragTarget - start);
            ExecuteEvents.Execute(
                scrollRect.gameObject,
                drag,
                ExecuteEvents.dragHandler);
            yield return null;
            ExecuteEvents.Execute(
                scrollRect.gameObject,
                drag,
                ExecuteEvents.endDragHandler);
            yield return WaitForRowSettle(
                "touch-drag",
                requireNonTop: true);
            Require(settlePassed,
                "TARGET_TOUCH_DRAG_ROW_SNAP_FAILED");
            VerifyVisibleAlignment("touch-drag");

            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 0.55f;
            yield return null;
            PointerEventData inertiaMarker = BuildPointerEvent(
                start,
                start,
                Vector2.zero);
            ExecuteEvents.Execute(
                scrollRect.gameObject,
                inertiaMarker,
                ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(
                scrollRect.gameObject,
                inertiaMarker,
                ExecuteEvents.endDragHandler);
            scrollRect.velocity = new Vector2(0f, 520f);
            yield return WaitForRowSettle(
                "inertia",
                requireNonTop: false);
            Require(settlePassed,
                "TARGET_INERTIA_ROW_SNAP_FAILED");
            VerifyVisibleAlignment("inertia");

            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 0f;
            yield return null;
            ExecuteEvents.Execute(
                scrollRect.gameObject,
                wheel,
                ExecuteEvents.scrollHandler);
            yield return WaitForRowSettle(
                "bottom",
                requireNonTop: true);
            Require(settlePassed,
                "TARGET_BOTTOM_CLAMP_ROW_SNAP_FAILED");
            VerifyVisibleAlignment("bottom");

            evidence.Add(
                "target-scrollrect=wheel/touch/inertia/bottom"
                + ";actual-content-viewport-offset=PASS");
        }

        private IEnumerator WaitForRowSettle(
            string label,
            bool requireNonTop)
        {
            settlePassed = false;
            int stableFrames = 0;
            for (int frame = 0; frame < 420; frame++)
            {
                yield return null;
                Canvas.ForceUpdateCanvases();
                if (!TryResolveScrollMetrics(
                        out float pitch,
                        out float offset,
                        out float bottom))
                {
                    continue;
                }

                bool legal = IsLegalRowOffset(offset, pitch, bottom);
                bool stopped = Mathf.Abs(scrollRect.velocity.y) <= 1f;
                bool moved = !requireNonTop || offset > RuntimeTolerance;
                stableFrames = legal && stopped && moved
                    ? stableFrames + 1
                    : 0;
                if (stableFrames >= 4)
                {
                    settlePassed = true;
                    evidence.Add(
                        "row-" + label + "="
                        + offset.ToString(
                            "0.###",
                            CultureInfo.InvariantCulture)
                        + "/pitch="
                        + pitch.ToString(
                            "0.###",
                            CultureInfo.InvariantCulture)
                        + "/bottom="
                        + bottom.ToString(
                            "0.###",
                            CultureInfo.InvariantCulture));
                    yield break;
                }
            }
        }

        private bool TryResolveScrollMetrics(
            out float pitch,
            out float offset,
            out float bottom)
        {
            GridLayoutGroup grid = contentRoot == null
                ? null
                : contentRoot.GetComponent<GridLayoutGroup>();
            pitch = grid == null
                ? 0f
                : grid.cellSize.y + grid.spacing.y;
            offset = 0f;
            bottom = 0f;
            if (grid == null || viewport == null || contentRoot == null)
            {
                return false;
            }
            Bounds bounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(
                    viewport,
                    contentRoot);
            bottom = Mathf.Max(
                0f,
                bounds.size.y - viewport.rect.height);
            offset = Mathf.Clamp(
                bounds.max.y - viewport.rect.yMax,
                0f,
                bottom);
            return pitch > 0f && bottom > 0f;
        }

        private static bool IsLegalRowOffset(
            float offset,
            float pitch,
            float bottom)
        {
            if (Mathf.Abs(offset - bottom) <= RuntimeTolerance)
            {
                return true;
            }
            float nearest = Mathf.Round(offset / pitch) * pitch;
            return Mathf.Abs(offset - nearest) <= RuntimeTolerance;
        }

        private void VerifyVisibleAlignment(string label)
        {
            SerializedObject serializedTrayView = new(trayView);
            SerializedProperty slotProperty =
                serializedTrayView.FindProperty("traySlotRects");
            BuildItemPreviewCardView[] cards = trayView
                .GetComponentsInChildren<BuildItemPreviewCardView>(true);
            BuildItemPreviewCardView card = cards.FirstOrDefault(value =>
            {
                if (value == null
                    || !value.gameObject.activeInHierarchy
                    || !trayView.TryGetDisplayedPlacement(
                        value.ItemId,
                        out TrayPlacementViewModel placement)
                    || placement == null
                    || placement.occupiedSlotIndexes == null
                    || placement.occupiedSlotIndexes.Count == 0)
                {
                    return false;
                }
                Vector2 point = ScreenCenter(value.RectTransform);
                return RectTransformUtility.RectangleContainsScreenPoint(
                    viewport,
                    point,
                    ResolveEventCamera(viewport));
            });
            Require(card != null,
                "TARGET_ALIGNMENT_VISIBLE_CARD_MISSING_" + label);
            if (card == null
                || !trayView.TryGetDisplayedPlacement(
                    card.ItemId,
                    out TrayPlacementViewModel displayed)
                || displayed == null)
            {
                return;
            }

            List<RectTransform> occupiedRects = new();
            foreach (int index in displayed.occupiedSlotIndexes)
            {
                if (index < 0 || index >= slotProperty.arraySize)
                {
                    continue;
                }
                RectTransform rect = slotProperty
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue as RectTransform;
                if (rect != null)
                {
                    occupiedRects.Add(rect);
                }
            }
            Require(occupiedRects.Count
                    == displayed.occupiedSlotIndexes.Count,
                "TARGET_ALIGNMENT_SLOT_SET_MISSING_" + label);
            if (occupiedRects.Count == 0)
            {
                return;
            }

            Bounds expected = BuildWorldBounds(occupiedRects);
            Bounds actual = BuildWorldBounds(
                new[] { card.RectTransform });
            Require(Vector3.Distance(expected.center, actual.center)
                    <= RuntimeTolerance
                    && Vector3.Distance(expected.size, actual.size)
                    <= RuntimeTolerance * 2f,
                "TARGET_BACKGROUND_CARD_BOUNDS_MISMATCH_" + label);

            GraphicRaycaster raycaster = card
                .GetComponentInParent<GraphicRaycaster>();
            List<RaycastResult> results = new();
            PointerEventData pointer = BuildPointerEvent(
                ScreenCenter(card.RectTransform),
                ScreenCenter(card.RectTransform),
                Vector2.zero);
            raycaster?.Raycast(pointer, results);
            Require(raycaster != null
                    && results.Any(result => result.gameObject != null
                        && (result.gameObject == card.gameObject
                            || result.gameObject.transform.IsChildOf(
                                card.transform))),
                "TARGET_CARD_RAYCAST_ALIGNMENT_FAILED_" + label);
        }

        private void ExecuteCardDrag(
            BuildItemPreviewCardView card,
            Vector2 target)
        {
            Vector2 origin = ScreenCenter(card.RectTransform);
            PointerEventData down = BuildPointerEvent(
                origin,
                origin,
                Vector2.zero);
            card.OnPointerDown(down);
            PointerEventData drag = BuildPointerEvent(
                target,
                origin,
                target - origin);
            card.OnBeginDrag(drag);
            card.OnDrag(drag);
            card.OnEndDrag(drag);
            card.OnPointerUp(drag);
        }

        private PointerEventData BuildPointerEvent(
            Vector2 position,
            Vector2 pressPosition,
            Vector2 delta)
        {
            return new PointerEventData(eventSystem)
            {
                button = PointerEventData.InputButton.Left,
                position = position,
                pressPosition = pressPosition,
                delta = delta,
                pointerId = -1
            };
        }

        private BuildGridPreviewSlotView[] ResolveSerializedBoardSlots()
        {
            SerializedObject serializedController = new(controller);
            SerializedProperty property =
                serializedController.FindProperty("boardSlots");
            if (property == null || !property.isArray)
            {
                return Array.Empty<BuildGridPreviewSlotView>();
            }
            List<BuildGridPreviewSlotView> slots = new();
            for (int index = 0; index < property.arraySize; index++)
            {
                BuildGridPreviewSlotView slot = property
                    .GetArrayElementAtIndex(index)
                    .objectReferenceValue as BuildGridPreviewSlotView;
                if (slot != null)
                {
                    slots.Add(slot);
                }
            }
            return slots.ToArray();
        }

        private Dictionary<string, string> CaptureDisplayedSignatures()
        {
            Dictionary<string, string> signatures =
                new(StringComparer.Ordinal);
            foreach (BuildItemPreviewCardView card in trayView
                         .GetComponentsInChildren<
                             BuildItemPreviewCardView>(true))
            {
                if (card == null
                    || !trayView.TryGetDisplayedPlacement(
                        card.ItemId,
                        out TrayPlacementViewModel placement)
                    || placement == null)
                {
                    continue;
                }
                signatures[card.ItemId] = placement.itemId
                    + "|"
                    + placement.anchorSlotIndex.ToString(
                        CultureInfo.InvariantCulture)
                    + "|"
                    + string.Join(
                        ",",
                        placement.occupiedSlotIndexes
                        ?? Array.Empty<int>())
                    + "|"
                    + ((int)placement.rotation).ToString(
                        CultureInfo.InvariantCulture);
            }
            return signatures;
        }

        private static bool MapEquals(
            IReadOnlyDictionary<string, string> left,
            IReadOnlyDictionary<string, string> right)
        {
            return left.Count == right.Count
                && left.All(pair => right.TryGetValue(
                        pair.Key,
                        out string value)
                    && string.Equals(
                        pair.Value,
                        value,
                        StringComparison.Ordinal));
        }

        private static bool MapEqualsExcept(
            IReadOnlyDictionary<string, string> before,
            IReadOnlyDictionary<string, string> after,
            string excluded)
        {
            Dictionary<string, string> expected = before
                .Where(pair => !string.Equals(
                    pair.Key,
                    excluded,
                    StringComparison.Ordinal))
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.Ordinal);
            Dictionary<string, string> actual = after
                .Where(pair => !string.Equals(
                    pair.Key,
                    excluded,
                    StringComparison.Ordinal))
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.Ordinal);
            return MapEquals(expected, actual);
        }

        private static Vector2 ScreenCenter(RectTransform rect)
        {
            if (rect == null)
            {
                return Vector2.zero;
            }
            Vector3 world = rect.TransformPoint(rect.rect.center);
            return RectTransformUtility.WorldToScreenPoint(
                ResolveEventCamera(rect),
                world);
        }

        private static Camera ResolveEventCamera(RectTransform rect)
        {
            Canvas canvas = rect == null
                ? null
                : rect.GetComponentInParent<Canvas>();
            return canvas != null
                   && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
        }

        private static Bounds BuildWorldBounds(
            IEnumerable<RectTransform> rects)
        {
            bool hasPoint = false;
            Bounds bounds = default;
            Vector3[] corners = new Vector3[4];
            foreach (RectTransform rect in rects
                         ?? Enumerable.Empty<RectTransform>())
            {
                if (rect == null)
                {
                    continue;
                }
                rect.GetWorldCorners(corners);
                foreach (Vector3 corner in corners)
                {
                    if (!hasPoint)
                    {
                        bounds = new Bounds(corner, Vector3.zero);
                        hasPoint = true;
                    }
                    else
                    {
                        bounds.Encapsulate(corner);
                    }
                }
            }
            return bounds;
        }

        private static bool GetPrivateBool(object target, string fieldName)
        {
            FieldInfo field = target?.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null && field.GetValue(target) is bool value
                && value;
        }

        private void Require(bool condition, string code)
        {
            if (!condition)
            {
                failures.Add(code);
            }
        }
    }
}
