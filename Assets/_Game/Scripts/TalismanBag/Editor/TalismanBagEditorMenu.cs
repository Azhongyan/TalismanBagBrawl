#if UNITY_EDITOR
using TalismanBag.Editor.V04.WorldMap;
using TalismanBag.EditorTools.BuildSandbox;
using TalismanBag.EditorTools.ItemBalance;
using TalismanBag.V03.Editor;
using TalismanBag.V03.EditorTools;
using UnityEditor;

namespace TalismanBag.EditorTools
{
    /// <summary>
    /// The single productive menu surface for Talisman Bag editor tooling.
    /// Historical package-specific entry points remain callable from code or
    /// batch methods, but are intentionally not exposed as Unity menu items.
    /// </summary>
    public static class TalismanBagEditorMenu
    {
        private const string Root = "Tools/Talisman Bag/";

        [MenuItem(
            Root
            + "Build/Build V0.4 BattleSandbox Preview APK "
            + "[Writes PlayerSettings]")]
        public static void BuildBattleSandboxPreviewApk()
        {
            AndroidApkBuilder.BuildPlaytestApk();
        }

        [MenuItem(Root + "Authoring/Data/Open Item Balance Workbench")]
        public static void OpenItemBalanceWorkbench()
        {
            ItemBalanceWorkbenchWindow.Open();
        }

        [MenuItem(
            Root
            + "Authoring/Scene Repair/MainHome/"
            + "Refresh Edit Preview [Writes Scene]")]
        public static void RefreshMainHomeEditPreview()
        {
            V03MainHomeEditPreviewTools.RefreshEditPreview();
        }

        [MenuItem(
            Root
            + "Authoring/Scene Repair/MainHome/"
            + "Repair Full Background Underlay [Writes Scene]")]
        public static void RepairMainHomeFullBackground()
        {
            V03MainHomeSceneFix02.RepairFullBackgroundUnderlayBatch();
        }

        [MenuItem(
            Root
            + "Authoring/Scene Repair/Upgrade/"
            + "Bind Runtime Lock Services [Writes Scene]")]
        public static void BindUpgradeRuntimeLockServices()
        {
            V03TalismanUpgradeSceneBuilder.BindRuntimeLockServicesBatch();
        }

        [MenuItem(
            Root
            + "Authoring/Scene Repair/BootEntry/"
            + "Bind Runtime Lock Scene Nodes [Writes Scene]")]
        public static void BindBootEntryRuntimeLockNodes()
        {
            V03BootEntryFlow01SceneBuilder.BindRuntimeLockSceneNodes();
        }

        [MenuItem(Root + "QA/Upgrade/Verify Upgrade Slot Contract")]
        public static void VerifyUpgradeSlotContract()
        {
            V03TalismanUpgradeSceneBuilder.VerifyUpgradeSlotContractBatch();
        }

        [MenuItem(Root + "QA/World Map/Verify World Map Vertical Slice")]
        public static void VerifyWorldMapVerticalSlice()
        {
            V04WorldMapVerticalSliceVerifier.VerifyFromMenu();
        }

        [MenuItem(Root + "QA/BattleSandbox/Verify Preview Scene")]
        public static void VerifyBattleSandboxPreviewScene()
        {
            BuildSandboxGuardRunner.RunBattleSandboxPreviewSceneMenu();
        }

        [MenuItem(Root + "QA/BattleSandbox/Run Full Roster Regression")]
        public static void RunBattleSandboxFullRosterRegression()
        {
            BuildSandboxGuardRunner
                .RunBattleSandboxPlayableFullRosterRegressionMenu();
        }

        [MenuItem(Root + "QA/BattleSandbox/Run Dev Chapter Playable")]
        public static void RunBattleSandboxDevChapterPlayable()
        {
            BuildSandboxGuardRunner.RunBattleSandboxDevChapterPlayableMenu();
        }
    }
}
#endif
