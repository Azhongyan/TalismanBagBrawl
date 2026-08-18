using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TalismanBag.BattleBridge.Formal;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Presentation.FormalBattle;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.Presentation.ExactItemDetail;
using TalismanBag.UnifiedBattle.Presentation.ExactNavigation;
using TalismanBag.V04.Campaign.Chapter1;
using TalismanBag.V04.ChapterFlow.Chapter1;
using TalismanBag.V04.RewardDrop.Contracts;
using TalismanBag.V04.RewardDrop.Runtime.CampaignLoot;
using TalismanBag.V04.WorldMap;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class C1FormalObtainRebuildBattleLoopTests
    {
        private static int assertionCount;
        private static string cumulativeOwnerChainEvidence = string.Empty;

        public static string CumulativeOwnerChainEvidence =>
            cumulativeOwnerChainEvidence;

        [MenuItem("TalismanBag/V0.4/Unified Battle/Run C1 Formal Loop Tests")]
        public static void RunFromMenu()
        {
            RunAllOrThrow();
            Debug.Log("[C1FormalLoopTests] PASS assertions=" + assertionCount);
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                RunAllOrThrow();
                Debug.Log("[C1FormalLoopTests] PASS assertions=" + assertionCount);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("[C1FormalLoopTests] FAIL " + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void RunAllOrThrow()
        {
            assertionCount = 0;
            VerifyFormalPrepareParityCorrection();
            VerifyItemDetailAdmissionGate();
            VerifyPrepareOrchestrationCorrection();
            VerifyPausedLiveItemRefreshCorrection();
            VerifyStageOneAndIdempotentEntitlement();
            VerifyTrayStageTwoTrace();
            VerifyWeakStageTwoTrace();
            VerifyTargetStageTwoTrace();
            VerifyStageClearMismatchFailsClosed();
            VerifyResetAndNewLaunchLifecycle();
            VerifyDeterministicReceiptAndNoPersistenceCommands();
        }

        public static void RunBaseAdmissionDetailGateCorrectionOrThrow()
        {
            assertionCount = 0;
            VerifyItemDetailAdmissionGate();
            VerifyPrepareOrchestrationCorrection();
        }

        public static void RunPrepareOrchestrationCorrectionOrThrow()
        {
            assertionCount = 0;
            VerifyPrepareOrchestrationCorrection();
        }

        public static void RunFormalPrepareParityCorrectionOrThrow()
        {
            assertionCount = 0;
            VerifyFormalPrepareParityCorrection();
            VerifyPrepareOrchestrationCorrection();
        }

        public static int RunRev10PausedLiveItemRefreshCorrectionOrThrow()
        {
            assertionCount = 0;
            VerifyPausedLiveItemRefreshCorrection();
            return assertionCount;
        }

        public static int RunOptionalI002StageTwoCorrectionOrThrow()
        {
            assertionCount = 0;
            VerifyTrayStageTwoTrace(true);
            VerifyWeakStageTwoTrace(true);
            VerifyTargetStageTwoTrace(true);
            return assertionCount;
        }

        public static int RunStage1To5CorePlayabilitySliceOrThrow()
        {
            assertionCount = 0;
            VerifyStage1To5ConfigsAndWorldMapProgress();
            VerifyCampaignRunSessionLineageRejectsMismatch();
            VerifyStage1To5RewardItemCompletionChain();
            VerifyStage1To5StaticOwnershipBoundaries();
            VerifyDynamicItemInstanceConsumerContract();
            return assertionCount;
        }

        public static int RunCumulativeItemInstanceRealOwnerChainOrThrow()
        {
            assertionCount = 0;
            cumulativeOwnerChainEvidence = string.Empty;
            VerifyCumulativeItemInstanceRewardToNextBattleChain();
            return assertionCount;
        }

        public static void
            ExecuteCumulativeItemInstanceRealOwnerChainFromCommandLine()
        {
            try
            {
                int assertions =
                    RunCumulativeItemInstanceRealOwnerChainOrThrow();
                Debug.Log(
                    "[CanonicalFormalFeederProof] PASS assertions="
                    + assertions + " " + cumulativeOwnerChainEvidence);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[CanonicalFormalFeederProof] FAIL " + exception);
                EditorApplication.Exit(1);
            }
        }

        public static int RunClearedReplayContinuationCorrectionOrThrow()
        {
            assertionCount = 0;
            VerifyClearedReplayContinuesToFormalSuccessor();
            return assertionCount;
        }

        public static int RunStage3To4BoneSwapPresentationBindingOrThrow()
        {
            assertionCount = 0;
            VerifyStage3To4BoneSwapPresentationBinding();
            return assertionCount;
        }

        public static int RunRev11SingleFormalPathConsolidationOrThrow()
        {
            assertionCount = 0;
            VerifyRev11fDetachedPrefabViewportAuthoringContract();
            VerifyRev11SingleFormalPathComposition();
            VerifyFormalPrepareParityCorrection();
            VerifyItemDetailAdmissionGate();
            VerifyPrepareOrchestrationCorrection();
            VerifyPausedLiveItemRefreshCorrection();
            return assertionCount;
        }

        private static void VerifyRev11SingleFormalPathComposition()
        {
            const string shellPath =
                "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/"
                + "UnifiedBattlePageShell.prefab";
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(
                shellPath);
            Require(asset != null, "REV11 Shell Prefab missing");
            UnifiedBattlePageShell shell = asset.GetComponent<
                UnifiedBattlePageShell>();
            UnifiedBattleFormalSceneHost host = asset.GetComponent<
                UnifiedBattleFormalSceneHost>();
            Require(shell != null && host != null,
                "REV11 Shell/Host root identity missing");
            Require(shell.CollectMissingRequiredSlots().Count == 0,
                "REV11 formal composition anchor missing");
            Require(host.ValidateAuthoredBindings(out string diagnostic),
                diagnostic);

            string[] obsoleteNames =
            {
                "BoardArea",
                "ItemTrayArea",
                "BossCastBarSlot",
                "StoryGuidePopupLayer",
                "ResultRewardPlaceholder",
                "V03FlowAdapterSlot",
                "V04SandboxAdapterSlot",
                "DevOnlyDiagnosticsSlot"
            };
            Transform[] hierarchy = asset.GetComponentsInChildren<Transform>(
                true);
            foreach (string obsoleteName in obsoleteNames)
            {
                Require(hierarchy.All(value => !string.Equals(
                        value.name,
                        obsoleteName,
                        StringComparison.Ordinal)),
                    "REV11 obsolete carrier remains " + obsoleteName);
            }

            C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                asset.GetComponentsInChildren<
                    C1ExactBattleSandboxItemArrangementPresenter>(true)
                .Single();
            C1ExactBattleSandboxItemBoardView board = itemPresenter
                .GetComponentsInChildren<
                    C1ExactBattleSandboxItemBoardView>(true).Single();
            C1ExactBattleSandboxItemTrayView tray = itemPresenter
                .GetComponentsInChildren<
                    C1ExactBattleSandboxItemTrayView>(true).Single();
            Require(itemPresenter.ValidateAuthoredReferences()
                    && board.PresentationCatalogSnapshot != null
                    && board.AuthoredArtworkCapacity > 0
                    && board.AuthoredArtworkCapacity <=
                    C1ExactBattleSandboxItemBoardView
                        .PhysicalPresentationCapacity
                    && tray.AuthoredCardCapacity > 0
                    && tray.AuthoredCardCapacity <=
                    C1ExactBattleSandboxItemTrayView
                        .PhysicalPresentationCapacity,
                "REV11 dynamic Item provider carrier invalid");
            FormalBattlePresentationRoot presentation = asset
                .GetComponentsInChildren<FormalBattlePresentationRoot>(true)
                .Single();
            Require(presentation.ValidateDownstreamCompositionReferences(
                    out diagnostic),
                diagnostic);

            C1ExactBattleSandboxCombatLogView combatLog = asset
                .GetComponentsInChildren<
                    C1ExactBattleSandboxCombatLogView>(true).Single();
            Require(combatLog.ValidateAuthoredReferences()
                    && !combatLog.IsOpen,
                "REV11 formal combat-log carrier invalid");
            C1ExactBattleSandboxItemDetailPresenter detail = asset
                .GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailPresenter>(true).Single();
            Require(detail.ValidateAuthoredReferences()
                    && detail.PresentationFrame != null
                    && string.Equals(
                        detail.PresentationFrame.name,
                        "ItemDetailPresentationFrame",
                        StringComparison.Ordinal)
                    && detail.PanelView.transform.parent
                    == detail.PresentationFrame
                    && detail.PresentationFrame.anchorMin
                    == new Vector2(0.5f, 0.5f)
                    && detail.PresentationFrame.anchorMax
                    == new Vector2(0.5f, 0.5f)
                    && detail.PresentationFrame.pivot
                    == new Vector2(0.5f, 0.5f)
                    && detail.PresentationFrame.anchoredPosition
                    == Vector2.zero
                    && detail.PresentationFrame.sizeDelta == new Vector2(
                        C1ExactBattleSandboxItemDetailPresenter
                            .PresentationReferenceWidth,
                        C1ExactBattleSandboxItemDetailPresenter
                            .PresentationReferenceHeight)
                    && detail.PresentationFrame.localRotation
                    == Quaternion.identity
                    && detail.PresentationFrame.localScale == Vector3.one
                    && detail.PresentationFrame.GetComponents<
                        UnityEngine.UI.Graphic>().Length == 0
                    && detail.PopupCanvasGroup.alpha == 0f
                    && !detail.PopupCanvasGroup.interactable
                    && !detail.PopupCanvasGroup.blocksRaycasts,
                "REV11 exact Item-detail closed state invalid");
            VerifyItemDetailReferenceFit(new Vector2(1920f, 1080f));
            VerifyItemDetailReferenceFit(new Vector2(1080f, 1920f));
            VerifyItemDetailReferenceFit(new Vector2(720f, 1280f));
            Require(!C1ExactBattleSandboxItemDetailPresenter
                    .TryCalculateUniformReferenceFit(
                        Vector2.zero,
                        out _)
                    && !C1ExactBattleSandboxItemDetailPresenter
                    .TryCalculateUniformReferenceFit(
                        new Vector2(float.NaN, 1080f),
                        out _),
                "REV11E invalid Item-detail viewport was accepted");

            string slotsSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattlePageShellSlotNames.cs");
            string requiredBlock = MethodSource(
                slotsSource,
                "public static readonly string[] RequiredSlots",
                "};");
            Require(requiredBlock.IndexOf("BoardArea",
                        StringComparison.Ordinal) < 0
                    && requiredBlock.IndexOf("ItemTrayArea",
                        StringComparison.Ordinal) < 0,
                "REV11 ghost Item slots remain required");

            string hostSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattleFormalSceneHost.cs");
            Require(hostSource.IndexOf("v03FlowAdapterSummaryText",
                        StringComparison.Ordinal) < 0
                    && hostSource.IndexOf("combatLogView",
                        StringComparison.Ordinal) >= 0,
                "REV11 V03 sample log responsibility remains");
            string detailSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/"
                + "ExactItemDetail/C1ExactBattleSandboxItemDetailPresenter.cs");
            Require(detailSource.IndexOf("CreateViewModelClone()",
                         StringComparison.Ordinal) >= 0
                    && detailSource.IndexOf("CachedLegacyTextFont == null",
                        StringComparison.Ordinal) >= 0
                    && detailSource.IndexOf(
                        "renderedResultSignature = string.Empty;",
                        detailSource.IndexOf(
                            "CachedLegacyTextFont == null",
                            StringComparison.Ordinal),
                        StringComparison.Ordinal) >= 0
                    && detailSource.IndexOf("82",
                         StringComparison.Ordinal) < 0
                    && detailSource.IndexOf("28",
                        StringComparison.Ordinal) < 0,
                "REV11 Item detail is not released-projection-only");
            string authoringSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/"
                + "C1UnifiedSingleFormalPathAuthoring.cs");
            Require(authoringSource.IndexOf("EditorSceneManager",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf("OpenScene(",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf("SaveScene(",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf("UnpackPrefab",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf("RevertAll",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf("ApplyAll",
                        StringComparison.Ordinal) < 0,
                "REV11 authoring is not Shell-Prefab-only");
        }

        private static void VerifyRev11fDetachedPrefabViewportAuthoringContract()
        {
            const string authoringPath =
                "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/"
                + "C1UnifiedSingleFormalPathAuthoring.cs";
            string source = ReadProjectSource(authoringPath);
            string create = MethodSource(
                source,
                "private static RectTransform CreateItemDetailPresentationFrame",
                "private static void ConfigureSourceItemDetailPanelTransform");
            string validate = MethodSource(
                source,
                "private static bool IsAuthoredReferenceFrameGeometryValid",
                "private static bool IsSourcePanelGeometryValid");
            Require(create.IndexOf("frame.localScale = Vector3.one;",
                        StringComparison.Ordinal) >= 0
                    && create.IndexOf("viewport.rect",
                        StringComparison.Ordinal) < 0
                    && create.IndexOf("TryCalculateUniformReferenceFit",
                        StringComparison.Ordinal) < 0
                    && validate.IndexOf("viewport.rect",
                        StringComparison.Ordinal) < 0
                    && validate.IndexOf("TryCalculateUniformReferenceFit",
                        StringComparison.Ordinal) < 0
                    && source.IndexOf("presenter.RefreshPresentationLayout()",
                        StringComparison.Ordinal) < 0,
                "REV11F detached Prefab authoring still depends on a live viewport");

            GameObject viewportObject = new GameObject(
                "REV11F_DetachedZeroViewport",
                typeof(RectTransform));
            try
            {
                RectTransform viewport = (RectTransform)viewportObject.transform;
                viewport.sizeDelta = Vector2.zero;
                Require(viewport.rect.size == Vector2.zero,
                    "REV11F detached viewport fixture is not zero-sized");
                MethodInfo createMethod = typeof(
                        C1UnifiedSingleFormalPathAuthoring)
                    .GetMethod(
                        "CreateItemDetailPresentationFrame",
                        BindingFlags.Static | BindingFlags.NonPublic);
                Require(createMethod != null,
                    "REV11F frame authoring method missing");
                RectTransform frame = (RectTransform)createMethod.Invoke(
                    null,
                    new object[] { viewport });
                Require(frame != null
                        && frame.parent == viewport
                        && frame.anchorMin == new Vector2(0.5f, 0.5f)
                        && frame.anchorMax == new Vector2(0.5f, 0.5f)
                        && frame.pivot == new Vector2(0.5f, 0.5f)
                        && frame.anchoredPosition == Vector2.zero
                        && frame.sizeDelta == new Vector2(
                            C1ExactBattleSandboxItemDetailPresenter
                                .PresentationReferenceWidth,
                            C1ExactBattleSandboxItemDetailPresenter
                                .PresentationReferenceHeight)
                        && frame.localRotation == Quaternion.identity
                        && frame.localScale == Vector3.one,
                    "REV11F detached zero viewport authored geometry invalid");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(viewportObject);
            }

            Require(!C1ExactBattleSandboxItemDetailPresenter
                    .TryCalculateUniformReferenceFit(Vector2.zero, out _)
                    && !C1ExactBattleSandboxItemDetailPresenter
                    .TryCalculateUniformReferenceFit(
                        new Vector2(float.NaN, 1080f),
                        out _),
                "REV11F runtime fit accepted an invalid live viewport");
        }

        private static void VerifyItemDetailReferenceFit(Vector2 viewport)
        {
            Require(C1ExactBattleSandboxItemDetailPresenter
                    .TryCalculateUniformReferenceFit(
                        viewport,
                        out float scale)
                    && scale > 0f
                    && !float.IsNaN(scale)
                    && !float.IsInfinity(scale),
                "REV11E Item-detail reference fit invalid " + viewport);
            float fittedWidth =
                C1ExactBattleSandboxItemDetailPresenter
                    .PresentationReferenceWidth * scale;
            float fittedHeight =
                C1ExactBattleSandboxItemDetailPresenter
                    .PresentationReferenceHeight * scale;
            Require(fittedWidth <= viewport.x + 0.01f
                    && fittedHeight <= viewport.y + 0.01f,
                "REV11E Item-detail reference crops viewport " + viewport);
        }

        private static void VerifyFormalPrepareParityCorrection()
        {
            Type motionType = typeof(UnifiedBattleFormalSceneHost).Assembly
                .GetType(
                    "TalismanBag.UnifiedBattle.Presentation.ExactPrepare."
                    + "C1ExactBattleSandboxPrepareMotion",
                    true);
            object motion = Activator.CreateInstance(
                motionType,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { 9f },
                null);
            Require(motion != null, "exact prepare motion model missing");
            PropertyInfo state = motionType.GetProperty(
                "State",
                BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo progress = motionType.GetProperty(
                "Progress",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo beginOpen = motionType.GetMethod(
                "TryBeginOpen",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo beginClose = motionType.GetMethod(
                "TryBeginClose",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo advance = motionType.GetMethod(
                "Advance",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo snapClosed = motionType.GetMethod(
                "SnapClosed",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(state != null && progress != null && beginOpen != null
                    && beginClose != null && advance != null
                    && snapClosed != null,
                "exact prepare motion API incomplete");
            Equal("Closed", state.GetValue(motion).ToString(),
                "prepare begins authored closed");
            Equal(0f, (float)progress.GetValue(motion),
                "prepare begins at closed offset");
            Require((bool)beginOpen.Invoke(motion, null),
                "prepare pull-up starts once");
            Require(!(bool)beginOpen.Invoke(motion, null),
                "duplicate pull-up is idempotently rejected");
            AdvancePrepareMotionToCompletion(
                motion,
                state,
                advance,
                "Open");
            Equal(1f, (float)progress.GetValue(motion),
                "open completion reaches exact source geometry");
            Require((bool)beginClose.Invoke(motion, null),
                "prepare lower starts once");
            AdvancePrepareMotionToCompletion(
                motion,
                state,
                advance,
                "Closed");
            Equal(0f, (float)progress.GetValue(motion),
                "closed completion reaches exact -750 offset");
            snapClosed.Invoke(motion, null);
            Equal("Closed", state.GetValue(motion).ToString(),
                "reset clears stale prepare transition");

            string hostSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattleFormalSceneHost.cs");
            string open = MethodSource(
                hostSource,
                "private bool OpenLivePrepare",
                "private bool CloseLivePrepare");
            string close = MethodSource(
                hostSource,
                "private bool CloseLivePrepare",
                "private bool TryOpenRebuildArrangement");
            string completeOpen = MethodSource(
                hostSource,
                "private void CompletePrepareSurfaceOpen",
                "private void CompletePrepareSurfaceClosed");
            string completeClosed = MethodSource(
                hostSource,
                "private void CompletePrepareSurfaceClosed",
                "private bool BeginRebuildCloseForStageTwo");
            Require(open.IndexOf("PauseRealtimeBattle()",
                        StringComparison.Ordinal) >= 0
                    && open.IndexOf("TryBeginOpen",
                        StringComparison.Ordinal) > open.IndexOf(
                        "PauseRealtimeBattle()",
                        StringComparison.Ordinal),
                "live open pauses before authored pull-up");
            Require(open.IndexOf("PREPARE_ENABLED",
                        StringComparison.Ordinal) < 0
                    && completeOpen.IndexOf("PREPARE_ENABLED",
                        StringComparison.Ordinal) >= 0
                    && completeOpen.IndexOf("SetOpenInputEnabled",
                        StringComparison.Ordinal) >= 0,
                "Item input enables only at exact open completion");
            Require(close.IndexOf("BATTLE_LOCKED",
                        StringComparison.Ordinal) >= 0
                    && close.IndexOf("SetOpenInputEnabled",
                        StringComparison.Ordinal) >= 0
                    && close.IndexOf("TryBeginClose",
                        StringComparison.Ordinal) > close.IndexOf(
                        "BATTLE_LOCKED",
                        StringComparison.Ordinal)
                    && close.IndexOf("ResumeRealtimeBattle",
                        StringComparison.Ordinal) < 0
                    && completeClosed.IndexOf("ResumeRealtimeBattle",
                        StringComparison.Ordinal) >= 0,
                "close locks and disables input before lowering, then resumes at closed completion");
            Require(hostSource.IndexOf("SetArrangementSurfaceVisible",
                        StringComparison.Ordinal) < 0
                    && hostSource.IndexOf(
                        "SetSlotActive(slots, UnifiedBattlePageShellSlotNames.BoardArea",
                        StringComparison.Ordinal) < 0
                    && hostSource.IndexOf(
                        "SetSlotActive(slots, UnifiedBattlePageShellSlotNames.ItemTrayArea",
                        StringComparison.Ordinal) < 0,
                "prepare behavior never bare-toggles BoardArea or ItemTrayArea");
            Require(hostSource.IndexOf(
                        "prepareSurfaceTransition !=\n"
                        + "                    C1FormalPrepareSurfaceTransition.None",
                        StringComparison.Ordinal) >= 0,
                "battle advancement is blocked throughout prepare motion");

            string navSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/"
                + "ExactNavigation/C1ExactBattleSandboxNavigationBarView.cs");
            Require(navSource.IndexOf("\"鎴樻枟鏃ュ織\"",
                        StringComparison.Ordinal) >= 0
                    && navSource.IndexOf("\"鍔犻€?2x\"",
                        StringComparison.Ordinal) >= 0
                    && navSource.IndexOf("battleLogLabel.text",
                        StringComparison.Ordinal) >= 0,
                "speed and battle-log labels have distinct semantic validation");
        }

        private static void AdvancePrepareMotionToCompletion(
            object motion,
            PropertyInfo state,
            MethodInfo advance,
            string expectedState)
        {
            int completions = 0;
            for (int index = 0; index < 240; index++)
            {
                if ((bool)advance.Invoke(motion, new object[] { 1f / 60f }))
                {
                    completions++;
                    break;
                }
            }

            Equal(1, completions,
                "prepare motion completes exactly once for " + expectedState);
            Equal(expectedState, state.GetValue(motion).ToString(),
                "prepare motion terminal state " + expectedState);
        }

        private static string ReadProjectSource(string relativePath)
        {
            return File.ReadAllText(Path.Combine(
                Directory.GetCurrentDirectory(),
                relativePath));
        }

        private static string MethodSource(
            string source,
            string startToken,
            string endToken)
        {
            int start = source.IndexOf(startToken, StringComparison.Ordinal);
            int end = source.IndexOf(endToken, start, StringComparison.Ordinal);
            Require(start >= 0 && end > start,
                "source method boundary missing " + startToken);
            return source.Substring(start, end - start);
        }

        private static void VerifyItemDetailAdmissionGate()
        {
            bool[] values = { false, true };
            int absentCount = 0;
            int readyCount = 0;
            int partialCount = 0;
            foreach (bool presenterPresent in values)
            foreach (bool popupSlotPresent in values)
            foreach (bool exactSlotIdentity in values)
            foreach (bool authoredReferencesValid in values)
            {
                string expectedState;
                string expectedDiagnostic;
                if (!presenterPresent && !popupSlotPresent)
                {
                    expectedState = "Absent";
                    expectedDiagnostic = "UNIFIED_FORMAL_ITEM_DETAIL_ABSENT";
                    absentCount++;
                }
                else if (presenterPresent && popupSlotPresent
                         && exactSlotIdentity && authoredReferencesValid)
                {
                    expectedState = "Ready";
                    expectedDiagnostic = "UNIFIED_FORMAL_ITEM_DETAIL_READY";
                    readyCount++;
                }
                else
                {
                    expectedState = "PartialOrInvalid";
                    expectedDiagnostic =
                        "UNIFIED_FORMAL_ITEM_DETAIL_PARTIAL_OR_INVALID";
                    partialCount++;
                }

                string actualState = EvaluateItemDetailAdmission(
                    presenterPresent,
                    popupSlotPresent,
                    exactSlotIdentity,
                    authoredReferencesValid,
                    out string actualDiagnostic);
                Equal(expectedState, actualState,
                    "Item-detail admission state for "
                    + presenterPresent + "/" + popupSlotPresent + "/"
                    + exactSlotIdentity + "/" + authoredReferencesValid);
                Equal(expectedDiagnostic, actualDiagnostic,
                    "Item-detail admission diagnostic");
            }

            Equal(4, absentCount,
                "all wholly absent flag combinations are core-route compatible");
            Equal(1, readyCount,
                "only complete exact authored detail is ready");
            Equal(11, partialCount,
                "every partial, wrong-mount and invalid-reference combination rejects");

            Equal("Absent", EvaluateItemDetailAdmission(
                    false, false, false, false, out string firstAbsent),
                "first absent admission");
            Equal("Ready", EvaluateItemDetailAdmission(
                    true, true, true, true, out string ready),
                "ready admission after absent");
            Equal("Absent", EvaluateItemDetailAdmission(
                    false, false, false, false, out string reenteredAbsent),
                "absent re-entry has no stale ready state");
            Equal(firstAbsent, reenteredAbsent,
                "absent reset/re-entry diagnostic is stable");
            Equal("UNIFIED_FORMAL_ITEM_DETAIL_READY", ready,
                "ready diagnostic is explicit");

            string hostSource = File.ReadAllText(Path.Combine(
                Directory.GetCurrentDirectory(),
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattleFormalSceneHost.cs"));
            int validationStart = hostSource.IndexOf(
                "public bool ValidateAuthoredBindings",
                StringComparison.Ordinal);
            int assignmentStart = hostSource.IndexOf(
                "public void AssignForEditor",
                validationStart,
                StringComparison.Ordinal);
            Require(validationStart >= 0 && assignmentStart > validationStart,
                "Host validation source boundary missing");
            string validationSource = hostSource.Substring(
                validationStart,
                assignmentStart - validationStart);
            Require(validationSource.IndexOf(
                    "itemDetailPresenter == null",
                    StringComparison.Ordinal) < 0,
                "wholly absent Item detail must not be a core reference gate");
            Require(hostSource.IndexOf(
                    "itemPresenter.Bind(\n"
                    + "                    formalLoop.ItemAuthority,\n"
                    + "                    out string presenterDiagnostic)",
                    StringComparison.Ordinal) >= 0
                    || hostSource.IndexOf(
                    "itemPresenter.Bind(\r\n"
                    + "                    formalLoop.ItemAuthority,\r\n"
                    + "                    out string presenterDiagnostic)",
                    StringComparison.Ordinal) >= 0,
                "formal Item binding must not depend on a detail catalog provider");
            Require(hostSource.IndexOf(
                    "itemDetailPresenter?.Unbind();",
                    StringComparison.Ordinal) >= 0
                    && hostSource.IndexOf(
                    "itemDetailPresenter?.ResetPresentation();",
                    StringComparison.Ordinal) >= 0,
                "reset/disable remains null-safe for absent Item detail");
        }

        private static string EvaluateItemDetailAdmission(
            bool presenterPresent,
            bool popupSlotPresent,
            bool exactSlotIdentity,
            bool authoredReferencesValid,
            out string diagnostic)
        {
            Type gate = typeof(UnifiedBattleFormalSceneHost).Assembly.GetType(
                "TalismanBag.UnifiedBattle.C1FormalItemDetailAdmissionGate",
                true);
            MethodInfo evaluate = gate.GetMethod(
                "Evaluate",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(evaluate != null, "Item-detail admission gate missing");
            object[] arguments =
            {
                presenterPresent,
                popupSlotPresent,
                exactSlotIdentity,
                authoredReferencesValid,
                null
            };
            object state = evaluate.Invoke(null, arguments);
            diagnostic = (string)arguments[4];
            return state.ToString();
        }

        private static void VerifyPrepareOrchestrationCorrection()
        {
            Require(!InvokePrepareOrchestration(
                    "ShouldSubmitBattleAdvance",
                    new object[] { true, false }),
                "visible prepare surface blocks Host Battle advancement");
            Require(!InvokePrepareOrchestration(
                    "ShouldSubmitBattleAdvance",
                    new object[] { false, true }),
                "authoritative Battle pause blocks Host Battle advancement");
            Require(InvokePrepareOrchestration(
                    "ShouldSubmitBattleAdvance",
                    new object[] { false, false }),
                "running hidden arrangement state permits Host Battle advancement");

            C1FormalObtainRebuildBattleLoop stageOne = CreateOfflineLoop(
                "prepare-stage-one",
                1L);
            VerifyAcceptedPrepareCycle(stageOne, "stage 1");
            stageOne.Reset();

            C1FormalObtainRebuildBattleLoop stageTwo = CreateOfflineStageTwoLoop(
                "prepare-stage-two",
                2L);
            VerifyAcceptedPrepareCycle(stageTwo, "stage 2");
            stageTwo.Reset();

            VerifyPrepareEnableFailureCompensation();
            VerifyPrepareCompensationFailureFailsClosed();
            VerifyPausedResetDoesNotCrossGeneration();
        }

        private static void VerifyPausedLiveItemRefreshCorrection()
        {
            C1FormalObtainRebuildBattleLoop loop = CreateOfflineLoop(
                "rev10-remove-replace",
                301L);
            Require(loop.TryAdvanceBy(1000L, out string diagnostic), diagnostic);
            Require(loop.TryPauseRealtimeBattle(out _, out diagnostic), diagnostic);
            C1FormalRealtimeBattleSessionStateSnapshot beforeRemove =
                loop.RealtimeState;
            int enemyHpBefore = beforeRemove.actorSnapshots.Sum(row => row.currentHp);
            long enemyDueBefore = NextPendingActionDue(
                beforeRemove,
                C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave,
                null);
            ReturnItemToTray(
                loop.ItemAuthority,
                "rev10-return-i001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
            Require(loop.TryRefreshRealtimeItemsWhilePaused(
                    "rev10-refresh-remove",
                    out C1FormalRealtimeBattleItemRefreshResult removed,
                    out diagnostic),
                "stage-one remove refresh: " + diagnostic + " "
                + (removed?.error?.message ?? string.Empty));
            Require(removed != null
                    && removed.activeItemFactCount == 0
                    && removed.canceledFutureItemActionCount == 1
                    && removed.retainedFutureItemActionCount == 0
                    && removed.scheduledFutureItemActionCount == 0,
                "stage 1 removal refresh does not cancel the old Item trigger");
            Equal(beforeRemove.battleTimeMs, removed.stateSnapshot.battleTimeMs,
                "refresh preserves paused Battle time");
            Equal(beforeRemove.playerSnapshot.currentHp,
                removed.stateSnapshot.playerSnapshot.currentHp,
                "refresh preserves player HP");
            Equal(enemyHpBefore,
                removed.stateSnapshot.actorSnapshots.Sum(row => row.currentHp),
                "refresh preserves enemy HP");
            Equal(enemyDueBefore,
                NextPendingActionDue(
                    removed.stateSnapshot,
                    C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave,
                    null),
                "refresh preserves enemy cadence");
            Require(loop.TryRefreshRealtimeItemsWhilePaused(
                    "rev10-refresh-remove",
                    out C1FormalRealtimeBattleItemRefreshResult duplicate,
                    out diagnostic)
                    && ReferenceEquals(removed, duplicate),
                "identical Mainline refresh retry is not an exact no-op");
            Require(loop.TryResumeRealtimeBattle(out _, out diagnostic), diagnostic);
            Require(loop.TryAdvanceBy(5000L, out diagnostic), diagnostic);
            Equal(enemyHpBefore,
                loop.RealtimeState.actorSnapshots.Sum(row => row.currentHp),
                "returned Item still damages enemy after resume");
            Require(loop.RealtimeState.enemyApplicationCount >
                    beforeRemove.enemyApplicationCount,
                "enemy cadence did not continue after Item removal");

            Require(loop.TryPauseRealtimeBattle(out _, out diagnostic), diagnostic);
            long replaceTime = loop.RealtimeState.battleTimeMs;
            PlaceI001(
                loop.ItemAuthority,
                "rev10-place-i001",
                new Vector2Int(0, 1));
            Require(loop.TryRefreshRealtimeItemsWhilePaused(
                    "rev10-refresh-replace",
                    out C1FormalRealtimeBattleItemRefreshResult replaced,
                    out diagnostic),
                "stage-one replace refresh: " + diagnostic + " "
                + (replaced?.error?.message ?? string.Empty));
            C1FormalItemBattleItemRow i001Row = loop.ItemAuthority
                .CreateBattleInputSnapshot().itemRows.Single(row =>
                    row.itemInstanceId ==
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
            C1Lv1StarterItemProjectionResult i001Projection =
                C1Lv1StarterItemBaselineCatalog.TryGetProjection(
                    C1FormalRealtimeBattleSessionContract.ProductContext,
                    i001Row.baseItemId,
                    "I001@white",
                    C1Lv1StarterItemBaselineCatalog.ProfileRevision);
            long cooldownMs = decimal.ToInt64(
                i001Projection.projection.cooldownSeconds * 1000m);
            long firstDue = NextPendingActionDue(
                replaced.stateSnapshot,
                C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                "I001@white");
            Equal(replaceTime + cooldownMs, firstDue,
                "re-placed Item first trigger is not paused tick + cooldown");
            Require(loop.TryResumeRealtimeBattle(out _, out diagnostic), diagnostic);
            Require(loop.TryAdvanceBy(cooldownMs - 1L, out diagnostic), diagnostic);
            int enemyHpBeforeDue = loop.RealtimeState.actorSnapshots.Sum(
                row => row.currentHp);
            Equal(enemyHpBefore, enemyHpBeforeDue,
                "re-placed Item produced a catch-up burst");
            Require(loop.TryAdvanceBy(1L, out diagnostic), diagnostic);
            Require(loop.RealtimeState.actorSnapshots.Sum(row => row.currentHp)
                    < enemyHpBeforeDue,
                "re-placed Item did not fire at its exact new due tick");
            loop.Reset();

            C1FormalObtainRebuildBattleLoop stageTwo =
                CreateOfflineStageTwoLoop("rev10-stage-two", 302L);
            Require(stageTwo.TryAdvanceBy(1000L, out diagnostic), diagnostic);
            Require(stageTwo.TryPauseRealtimeBattle(out _, out diagnostic),
                diagnostic);
            ReturnItemToTray(
                stageTwo.ItemAuthority,
                "rev10-stage-two-return-i002",
                RewardedItemInstanceId(stageTwo.ItemAuthority));
            Require(stageTwo.TryRefreshRealtimeItemsWhilePaused(
                    "rev10-stage-two-i001-only",
                    out C1FormalRealtimeBattleItemRefreshResult i001Only,
                    out diagnostic),
                "stage-two I001-only refresh: " + diagnostic + " "
                + (i001Only?.error?.message ?? string.Empty));
            Require(i001Only.activeItemFactCount == 1
                    && i001Only.canceledFutureItemActionCount == 0
                    && i001Only.retainedFutureItemActionCount == 1
                    && i001Only.scheduledFutureItemActionCount == 0,
                "stage 1-2 did not retain only the latest active I001 fact "
                + "active=" + i001Only.activeItemFactCount
                + " canceled=" + i001Only.canceledFutureItemActionCount
                + " retained=" + i001Only.retainedFutureItemActionCount
                + " scheduled=" + i001Only.scheduledFutureItemActionCount);
            ReturnItemToTray(
                stageTwo.ItemAuthority,
                "rev10-stage-two-return-i001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
            Require(stageTwo.TryRefreshRealtimeItemsWhilePaused(
                    "rev10-stage-two-none",
                    out C1FormalRealtimeBattleItemRefreshResult stageTwoNone,
                    out diagnostic)
                    && stageTwoNone.activeItemFactCount == 0
                    && stageTwoNone.canceledFutureItemActionCount == 1,
                "stage 1-2 zero-active refresh did not cancel I002");
            stageTwo.Reset();

            C1FormalObtainRebuildBattleLoop defeat = CreateOfflineLoop(
                "rev10-live-defeat",
                303L);
            Require(defeat.TryAdvanceBy(1000L, out diagnostic), diagnostic);
            Require(defeat.TryPauseRealtimeBattle(out _, out diagnostic), diagnostic);
            ReturnItemToTray(
                defeat.ItemAuthority,
                "rev10-defeat-return-i001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
            Require(defeat.TryRefreshRealtimeItemsWhilePaused(
                    "rev10-defeat-refresh-none",
                    out _,
                    out diagnostic),
                "live-defeat zero refresh: " + diagnostic);
            Require(defeat.TryResumeRealtimeBattle(out _, out diagnostic), diagnostic);
            int guard = 0;
            while (defeat.Phase ==
                       C1FormalObtainRebuildBattlePhase.StageOneBattle
                   && guard++ < 100)
            {
                Require(defeat.TryAdvanceBy(1000L, out diagnostic), diagnostic);
            }

            Require(defeat.Phase ==
                    C1FormalObtainRebuildBattlePhase.StageOneResult
                    && defeat.StageOneRealtimeTerminal != null
                    && defeat.StageOneRealtimeTerminal.accepted
                    && defeat.StageOneRealtimeTerminal.lose
                    && !defeat.StageOneRealtimeTerminal.win,
                "refresh-divergent live defeat was not accepted as Battle truth");
            Require(!defeat.TryClaimFirstClearReward(out _),
                "live defeat incorrectly granted first-clear entitlement");
            defeat.Reset();

            string hostSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattleFormalSceneHost.cs");
            string close = MethodSource(
                hostSource,
                "private bool CloseLivePrepare",
                "private bool TryOpenRebuildArrangement");
            int lockIndex = close.IndexOf("BATTLE_LOCKED", StringComparison.Ordinal);
            int refreshIndex = close.IndexOf(
                "RefreshRealtimeItemsWhilePaused",
                StringComparison.Ordinal);
            int lowerIndex = close.IndexOf("TryBeginClose", StringComparison.Ordinal);
            Require(lockIndex >= 0 && refreshIndex > lockIndex
                    && lowerIndex > refreshIndex,
                "Host close order is not lock -> refresh -> lower");
            Require(close.IndexOf("ResumeRealtimeBattle", StringComparison.Ordinal)
                    < 0,
                "Host resumes before authored lower completion");
            Require(close.IndexOf(
                        "BattleItemRefreshRejectedDiagnostic",
                        StringComparison.Ordinal) >= 0,
                "Host lacks a stable refresh rejection diagnostic");
        }

        private static void VerifyAcceptedPrepareCycle(
            C1FormalObtainRebuildBattleLoop loop,
            string label)
        {
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                CreateAuthorizationPresenter(loop.ItemAuthority);
            List<string> order = new List<string>();
            bool surfaceVisible = false;
            long beforeTime = loop.RealtimeState.battleTimeMs;
            Func<C1FormalRealtimeBattleTickResult> pause = () =>
            {
                order.Add("pause");
                loop.TryPauseRealtimeBattle(
                    out C1FormalRealtimeBattleTickResult tick,
                    out _);
                return tick;
            };
            Func<C1FormalItemInteractionAuthorizationResult> enable = () =>
            {
                order.Add("enable");
                return presenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        presenter.Current,
                        C1FormalItemInteractionMode.PREPARE_ENABLED));
            };
            Action show = () =>
            {
                order.Add("show");
                surfaceVisible = true;
            };
            Action hide = () =>
            {
                order.Add("hide");
                surfaceVisible = false;
            };
            Func<C1FormalRealtimeBattleTickResult> resume = () =>
            {
                order.Add("resume");
                loop.TryResumeRealtimeBattle(
                    out C1FormalRealtimeBattleTickResult tick,
                    out _);
                return tick;
            };
            Action publish = () => order.Add("publish");
            object[] openArguments =
            {
                pause,
                enable,
                show,
                hide,
                resume,
                publish,
                null,
                null,
                null,
                null
            };
            Require(InvokePrepareOrchestration("TryOpen", openArguments),
                label + " prepare open accepted");
            Equal("pause,enable,show,publish", string.Join(",", order),
                label + " pause-before-enable and show-before-publish order");
            C1FormalRealtimeBattleTickResult pauseTick =
                (C1FormalRealtimeBattleTickResult)openArguments[6];
            Require(ReferenceEquals(pauseTick, loop.LastRealtimeTick)
                    && pauseTick.emittedCues.Any(cue => string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.SessionPaused,
                        StringComparison.Ordinal)),
                label + " transports the exact accepted pause tick and cue");
            Require(loop.IsRealtimeBattlePaused && surfaceVisible
                    && presenter.CurrentInteractionAuthorization.accepted
                    && presenter.CurrentInteractionAuthorization
                        .interactionEnabled,
                label + " prepare state is authoritatively paused and enabled");

            Require(!loop.TryAdvanceBy(5000L, out _),
                label + " paused Battle rejects hidden advancement");
            Equal(beforeTime, loop.RealtimeState.battleTimeMs,
                label + " paused Battle time remains exact");
            Require(!loop.TryPauseRealtimeBattle(out _, out _)
                    && loop.IsRealtimeBattlePaused,
                label + " duplicate open fails closed without changing pause truth");
            Equal(beforeTime, loop.RealtimeState.battleTimeMs,
                label + " duplicate open preserves Battle time");

            order.Clear();
            Func<C1FormalItemInteractionAuthorizationResult> lockItem = () =>
            {
                order.Add("lock");
                return presenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        presenter.Current,
                        C1FormalItemInteractionMode.BATTLE_LOCKED));
            };
            object[] closeArguments =
            {
                lockItem,
                hide,
                resume,
                publish,
                null,
                null,
                null
            };
            Require(InvokePrepareOrchestration("TryClose", closeArguments),
                label + " prepare close accepted");
            Equal("lock,hide,resume,publish", string.Join(",", order),
                label + " lock-before-hide-before-resume order");
            C1FormalRealtimeBattleTickResult resumeTick =
                (C1FormalRealtimeBattleTickResult)closeArguments[5];
            Require(ReferenceEquals(resumeTick, loop.LastRealtimeTick)
                    && resumeTick.emittedCues.Any(cue => string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.SessionResumed,
                        StringComparison.Ordinal)),
                label + " transports the exact accepted resume tick and cue");
            Require(!loop.IsRealtimeBattlePaused && !surfaceVisible
                    && presenter.CurrentInteractionAuthorization.accepted
                    && !presenter.CurrentInteractionAuthorization
                        .interactionEnabled,
                label + " close locks Item and resumes the same Battle");
            Equal(beforeTime, loop.RealtimeState.battleTimeMs,
                label + " pause/resume transaction has zero hidden time advance");
            Require(!loop.TryResumeRealtimeBattle(out _, out _)
                    && !loop.IsRealtimeBattlePaused,
                label + " duplicate close fails closed without changing resume truth");
        }

        private static void VerifyPrepareEnableFailureCompensation()
        {
            C1FormalObtainRebuildBattleLoop loop = CreateOfflineLoop(
                "prepare-compensation",
                1L);
            List<string> order = new List<string>();
            bool surfaceVisible = false;
            long beforeTime = loop.RealtimeState.battleTimeMs;
            object[] arguments =
            {
                (Func<C1FormalRealtimeBattleTickResult>)(() =>
                {
                    order.Add("pause");
                    loop.TryPauseRealtimeBattle(
                        out C1FormalRealtimeBattleTickResult tick,
                        out _);
                    return tick;
                }),
                (Func<C1FormalItemInteractionAuthorizationResult>)(() =>
                {
                    order.Add("enable");
                    return C1FormalItemInteractionAuthorizationResult
                        .InitialLocked();
                }),
                (Action)(() =>
                {
                    order.Add("show");
                    surfaceVisible = true;
                }),
                (Action)(() =>
                {
                    order.Add("hide");
                    surfaceVisible = false;
                }),
                (Func<C1FormalRealtimeBattleTickResult>)(() =>
                {
                    order.Add("resume");
                    loop.TryResumeRealtimeBattle(
                        out C1FormalRealtimeBattleTickResult tick,
                        out _);
                    return tick;
                }),
                (Action)(() => order.Add("publish")),
                null,
                null,
                null,
                null
            };
            Require(!InvokePrepareOrchestration("TryOpen", arguments),
                "rejected Item enable cannot open prepare");
            Equal("pause,enable,hide,resume,publish", string.Join(",", order),
                "Item enable failure performs one bounded compensated resume");
            Require(!loop.IsRealtimeBattlePaused && !surfaceVisible,
                "successful compensation returns to running and hidden");
            Equal(beforeTime, loop.RealtimeState.battleTimeMs,
                "compensation has zero hidden time advance");
            loop.Reset();
        }

        private static void VerifyPrepareCompensationFailureFailsClosed()
        {
            C1FormalObtainRebuildBattleLoop loop = CreateOfflineLoop(
                "prepare-compensation-failure",
                1L);
            int compensationAttempts = 0;
            bool surfaceVisible = false;
            object[] arguments =
            {
                (Func<C1FormalRealtimeBattleTickResult>)(() =>
                {
                    loop.TryPauseRealtimeBattle(
                        out C1FormalRealtimeBattleTickResult tick,
                        out _);
                    return tick;
                }),
                (Func<C1FormalItemInteractionAuthorizationResult>)(() =>
                    C1FormalItemInteractionAuthorizationResult.InitialLocked()),
                (Action)(() => surfaceVisible = true),
                (Action)(() => surfaceVisible = false),
                (Func<C1FormalRealtimeBattleTickResult>)(() =>
                {
                    compensationAttempts++;
                    return null;
                }),
                (Action)(() => { }),
                null,
                null,
                null,
                null
            };
            Require(!InvokePrepareOrchestration("TryOpen", arguments),
                "failed compensation cannot open prepare");
            Equal(1, compensationAttempts,
                "failed compensation is attempted exactly once");
            Require(loop.IsRealtimeBattlePaused && !surfaceVisible,
                "failed compensation remains authoritatively paused and hidden");
            loop.Reset();
        }

        private static void VerifyPausedResetDoesNotCrossGeneration()
        {
            C1FormalObtainRebuildBattleLoop first = CreateOfflineLoop(
                "prepare-reset-first",
                1L);
            Require(first.TryPauseRealtimeBattle(out _, out _)
                    && first.IsRealtimeBattlePaused,
                "reset fixture is paused");
            first.Reset();
            Require(!first.IsRealtimeBattlePaused
                    && first.CurrentRealtimeStart == null
                    && first.RealtimeState == null,
                "reset clears paused lifecycle state");

            C1FormalObtainRebuildBattleLoop second = CreateOfflineLoop(
                "prepare-reset-second",
                2L);
            Require(!second.IsRealtimeBattlePaused
                    && second.RealtimeState != null
                    && !second.RealtimeState.paused
                    && second.RealtimeState.battleTimeMs == 0L,
                "new generation does not inherit stale prepare pause state");
            second.Reset();
        }

        private static void VerifyStageOneAndIdempotentEntitlement()
        {
            C1FormalObtainRebuildBattleLoop loop = CreateLoop("stage-one", 1L);
            C1FormalRealtimeBattleSessionStartSnapshot stageOneStart =
                loop.CurrentRealtimeStart;
            Require(stageOneStart != null && stageOneStart.accepted,
                "stage 1 retains the accepted realtime start envelope");
            C1FormalRealtimeBattleCue stageOneTarget = stageOneStart.initialCues
                .Single(cue => string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.TargetChanged,
                    StringComparison.Ordinal));
            Require(stageOneStart.stateSnapshot.actorSnapshots.Any(actor =>
                    string.Equals(
                        actor.actorBalanceId,
                        stageOneTarget.targetActorId,
                        StringComparison.Ordinal)
                    && actor.stableActorOrder == stageOneTarget.targetStableOrder),
                "stage 1 TARGET_CHANGED identifies an authoritative actor snapshot");
            AdvanceActiveBattleToTerminal(loop);
            Require(ReferenceEquals(stageOneStart, loop.CurrentRealtimeStart),
                "stage 1 keeps the exact accepted immutable start envelope");
            C1FormalRealtimeBattleTerminalResult result =
                loop.StageOneRealtimeTerminal;
            Require(result.accepted && result.win && !result.lose,
                "1-1 accepted victory");
            Equal(16000L, result.durationMs, "1-1 duration");
            Equal(3, result.enemyApplicationCount, "1-1 enemy applications");
            Equal(76, result.playerSnapshot.currentHp, "1-1 player HP");
            Require(!result.shouldWriteSave && !result.shouldGrantReward,
                "1-1 result has no persistence or direct grant command");

            C1FormalItemBattleInputSnapshot initial =
                loop.ItemAuthority.CreateBattleInputSnapshot();
            Equal(1, initial.itemRows.Count, "1-1 exact Item row count");
            Require(initial.itemRows[0].baseItemId ==
                    "I001"
                    && initial.itemRows[0].isLit == true,
                "1-1 uses authoritative lit I001");

            Equal(C1FormalObtainRebuildBattlePhase.RewardClaimed,
                loop.Phase, "victory automatically enters RewardClaimed");
            Require(!loop.TryStartStageTwo(out _),
                "Continue is rejected before the player enters Arrange");
            string rewardedItemInstanceId = loop.FirstClearReceipt.rewardResult
                .rewardEntries.Single().itemInstanceId;
            Require(loop.ItemAuthority.Current.FindRosterEntry(
                    rewardedItemInstanceId) != null,
                "claim exposes the canonical reward Item instance");
            Equal(RewardedItemInstanceId(loop.ItemAuthority),
                rewardedItemInstanceId,
                "reward and Item Authority keep the same Item instance identity");

            int rosterCountAfterClaim = loop.ItemAuthority.Current.roster.Count;
            string entitledSignature = loop.ItemAuthority.Current.canonicalSignature;
            string receiptIdentity = loop.FirstClearReceipt.rewardResult
                .canonicalSignature;
            Require(loop.TryClaimFirstClearReward(out string diagnostic), diagnostic);
            Equal(C1FormalObtainRebuildBattleLoop.DuplicateAcceptedDiagnostic,
                diagnostic, "duplicate automatic reward callback is unchanged");
            Equal(rosterCountAfterClaim,
                loop.ItemAuthority.Current.roster.Count,
                "duplicate claim adds no second reward Item");
            Equal(entitledSignature,
                loop.ItemAuthority.Current.canonicalSignature,
                "duplicate claim preserves the Item session");
            Equal(receiptIdentity,
                loop.FirstClearReceipt.rewardResult.canonicalSignature,
                "duplicate claim preserves the accepted reward receipt");
            Require(loop.TryBeginRewardArrangement(out diagnostic), diagnostic);
            Equal(C1FormalObtainRebuildBattlePhase.RebuildForStageTwo,
                loop.Phase, "Arrange explicitly enters rebuild phase");
            Require(loop.TryBeginRewardArrangement(out diagnostic), diagnostic);
            Equal(C1FormalObtainRebuildBattleLoop.DuplicateAcceptedDiagnostic,
                diagnostic, "duplicate Arrange entry is idempotent");
            loop.Reset();
        }

        private static void VerifyWeakStageTwoTrace(bool offline = false)
        {
            C1FormalObtainRebuildBattleLoop loop = CreateEntitledLoop(
                "weak",
                1L,
                offline);
            C1FormalRealtimeBattleSessionStartSnapshot stageOneStart =
                loop.CurrentRealtimeStart;
            PlaceRewardItem(loop.ItemAuthority, "weak-place", new Vector2Int(3, 3));
            Require(loop.TryStartStageTwo(out string diagnostic), diagnostic);
            C1FormalRealtimeBattleSessionStartSnapshot stageTwoStart =
                loop.CurrentRealtimeStart;
            Require(stageTwoStart != null && stageTwoStart.accepted
                    && !ReferenceEquals(stageOneStart, stageTwoStart),
                "stage 2 atomically refreshes the accepted realtime start envelope");
            Require(!string.Equals(
                    stageOneStart.stateSnapshot.sessionId,
                    stageTwoStart.stateSnapshot.sessionId,
                    StringComparison.Ordinal),
                "stage 2 start carries the refreshed Battle session identity");
            C1FormalRealtimeBattleCue stageTwoTarget = stageTwoStart.initialCues
                .Single(cue => string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.TargetChanged,
                    StringComparison.Ordinal));
            Require(stageTwoStart.stateSnapshot.actorSnapshots.Any(actor =>
                    string.Equals(
                        actor.actorBalanceId,
                        stageTwoTarget.targetActorId,
                        StringComparison.Ordinal)
                    && actor.stableActorOrder == stageTwoTarget.targetStableOrder),
                "stage 2 exposes its own authoritative initial target cue");
            AdvanceActiveBattleToTerminal(loop);
            C1FormalRealtimeBattleTerminalResult result =
                loop.StageTwoRealtimeTerminal;
            Equal(48000L, result.durationMs, "weak 1-2 duration");
            Equal(10, result.enemyApplicationCount, "weak 1-2 enemy applications");
            Equal(20, result.playerSnapshot.currentHp, "weak 1-2 player HP");
            string resultSignature = result.canonicalSignature;
            Require(loop.TryStartStageTwo(out diagnostic), diagnostic);
            Equal(C1FormalObtainRebuildBattleLoop.DuplicateAcceptedDiagnostic,
                diagnostic, "duplicate 1-2 callback is unchanged");
            Equal(resultSignature,
                loop.StageTwoRealtimeTerminal.canonicalSignature,
                "duplicate 1-2 callback preserves result");
            loop.Reset();
        }

        private static void VerifyTrayStageTwoTrace(bool offline = false)
        {
            C1FormalObtainRebuildBattleLoop loop = CreateEntitledLoop(
                "tray-optional",
                1L,
                offline);
            C1FormalItemBattleItemRow i002 = loop.ItemAuthority
                .CreateBattleInputSnapshot().itemRows.Single(row =>
                    row.itemInstanceId ==
                    RewardedItemInstanceId(loop.ItemAuthority));
            Require(!i002.isPlaced && !i002.isLit.HasValue,
                "entitled I002 remains authoritative inactive Tray inventory");
            Require(loop.TryStartStageTwo(out string diagnostic), diagnostic);
            C1FormalRealtimeBattleSessionStartSnapshot start =
                loop.CurrentRealtimeStart;
            Require(start != null && start.accepted
                    && start.stateSnapshot != null,
                "stage 1-2 accepts unplaced I002 entitlement");
            Require(start.stateSnapshot.scheduledActions.Any(action =>
                    string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                        StringComparison.Ordinal)
                    && string.Equals(
                        action.sourceId,
                        "I001@white",
                        StringComparison.Ordinal)),
                "Tray branch retains the required active I001 runtime fact");
            Require(start.stateSnapshot.scheduledActions.All(action =>
                    !string.Equals(
                        action.sourceId,
                        "I002@white",
                        StringComparison.Ordinal)),
                "Tray I002 creates no realtime trigger or unlit diagnostic action");

            AdvanceActiveBattleToTerminal(loop);
            C1FormalRealtimeBattleTerminalResult result =
                loop.StageTwoRealtimeTerminal;
            Equal(48000L, result.durationMs, "Tray 1-2 weak duration");
            Equal(10, result.enemyApplicationCount,
                "Tray 1-2 enemy applications");
            Equal(20, result.playerSnapshot.currentHp, "Tray 1-2 player HP");
            Equal(24, result.acceptedItemApplicationCount,
                "Tray 1-2 accepted I001 applications");
            loop.Reset();
        }

        private static void VerifyTargetStageTwoTrace(bool offline = false)
        {
            C1FormalObtainRebuildBattleLoop loop = CreateEntitledLoop(
                "target",
                1L,
                offline);
            PlaceRewardItem(loop.ItemAuthority, "target-place", new Vector2Int(0, 2));
            C1FormalItemBattleItemRow i002 = loop.ItemAuthority
                .CreateBattleInputSnapshot().itemRows.Single(row =>
                    row.itemInstanceId ==
                    RewardedItemInstanceId(loop.ItemAuthority));
            Equal(true, i002.isLit, "target I002 lighting comes from Item authority");
            Require(loop.TryStartStageTwo(out string diagnostic), diagnostic);
            AdvanceActiveBattleToTerminal(loop);
            C1FormalRealtimeBattleTerminalResult result =
                loop.StageTwoRealtimeTerminal;
            Equal(36000L, result.durationMs, "target 1-2 duration");
            Equal(7, result.enemyApplicationCount, "target 1-2 enemy applications");
            Equal(44, result.playerSnapshot.currentHp, "target 1-2 player HP");
            Require(!result.shouldWriteSave && !result.shouldGrantReward,
                "1-2 result has no persistence or reward command");
            loop.Reset();
        }

        private static void VerifyStageClearMismatchFailsClosed()
        {
            VerifyCampaignRunSessionLineageRejectsMismatch();
        }

        private static void VerifyResetAndNewLaunchLifecycle()
        {
            C1FormalObtainRebuildBattleLoop first = CreateEntitledLoop(
                "lifecycle-first", 1L);
            C1FormalItemSessionAuthority firstAuthority = first.ItemAuthority;
            C1FormalRealtimeBattleSessionStartSnapshot firstStart =
                first.CurrentRealtimeStart;
            first.Reset();
            Equal(C1FormalObtainRebuildBattlePhase.Inactive,
                first.Phase, "reset phase");
            Require(first.ItemAuthority == null
                    && first.FirstClearReceipt == null
                    && first.CurrentRealtimeStart == null,
                "reset clears only memory loop state");

            C1FormalObtainRebuildBattleLoop second = CreateLoop(
                "lifecycle-second", 2L);
            Require(second.ItemAuthority != null
                    && !ReferenceEquals(firstAuthority, second.ItemAuthority),
                "new launch owns a fresh Item authority");
            Equal(2L, second.ItemAuthority.Current.resetGeneration,
                "new launch carries its accepted generation");
            Require(second.CurrentRealtimeStart != null
                    && !ReferenceEquals(firstStart, second.CurrentRealtimeStart)
                    && !string.Equals(
                        firstStart.canonicalSignature,
                        second.CurrentRealtimeStart.canonicalSignature,
                        StringComparison.Ordinal),
                "new launch does not expose stale realtime initial cues");
            second.Reset();
        }

        private static void VerifyDeterministicReceiptAndNoPersistenceCommands()
        {
            C1FormalObtainRebuildBattleLoop first = CreateEntitledLoop(
                "deterministic", 1L);
            C1FormalObtainRebuildBattleLoop second = CreateEntitledLoop(
                "deterministic", 1L);
            Equal(first.FirstClearReceipt.stageClearFact.canonicalSignature,
                second.FirstClearReceipt.stageClearFact.canonicalSignature,
                "StageClear canonical determinism");
            Equal(first.FirstClearReceipt.rewardResult.canonicalSignature,
                second.FirstClearReceipt.rewardResult.canonicalSignature,
                "RewardResult canonical determinism");
            Equal(first.ItemAuthority.Current.entitlementCanonicalSignature,
                second.ItemAuthority.Current.entitlementCanonicalSignature,
                "entitlement canonical determinism");
            Require(!FormalBattleLaunchTransit.HasPendingContext,
                "in-memory 1-2 does not publish a WorldMap transit");
            Require(!first.StageOneRealtimeTerminal.shouldWriteSave
                    && !first.StageOneRealtimeTerminal.shouldGrantReward,
                "Battle exposes no Save/Progress/Reward write command");
            first.Reset();
            second.Reset();
        }

        private static C1FormalObtainRebuildBattleLoop CreateEntitledLoop(
            string suffix,
            long generation,
            bool offline = false)
        {
            C1FormalObtainRebuildBattleLoop loop = offline
                ? CreateOfflineLoop(suffix, generation)
                : CreateLoop(suffix, generation);
            AdvanceActiveBattleToTerminal(loop);
            Equal(C1FormalObtainRebuildBattlePhase.RewardClaimed,
                loop.Phase, "victory automatically accepts the reward");
            string diagnostic;
            Require(loop.TryBeginRewardArrangement(out diagnostic), diagnostic);
            return loop;
        }

        private static void AdvanceActiveBattleToTerminal(
            C1FormalObtainRebuildBattleLoop loop)
        {
            Require(loop != null
                    && (loop.Phase == C1FormalObtainRebuildBattlePhase.StageOneBattle
                        || loop.Phase ==
                            C1FormalObtainRebuildBattlePhase.StageTwoBattle),
                "a realtime Battle phase is active");
            Require(loop.TryAdvanceBy(60000L, out string diagnostic), diagnostic);
            Require(loop.Phase == C1FormalObtainRebuildBattlePhase.RewardClaimed
                    || loop.Phase ==
                        C1FormalObtainRebuildBattlePhase.StageOneResult
                    || loop.Phase ==
                        C1FormalObtainRebuildBattlePhase.StageTwoResult,
                "realtime Battle reaches its accepted terminal result");
        }

        private static C1FormalObtainRebuildBattleLoop CreateLoop(
            string suffix,
            long generation)
        {
            Require(Chapter1CampaignStageCatalog.TryGet(
                    Chapter1CampaignStageConfig.StageOneId,
                    out Chapter1CampaignStageConfig config,
                    out string diagnostic),
                diagnostic);
            BattleLaunchContext context = config.CreateLaunchContext(
                "c1-formal-loop-test-launch-" + suffix,
                "c1-formal-loop-test-token-" + suffix,
                generation);
            CanonicalItemDefinitionResolver canonicalItemResolver =
                LoadCanonicalItemResolver();
            Require(C1FormalObtainRebuildBattleLoop.TryCreate(
                    context,
                    config,
                    canonicalItemResolver,
                    out C1FormalObtainRebuildBattleLoop loop,
                    out diagnostic),
                diagnostic);
            return loop;
        }

        private static C1FormalObtainRebuildBattleLoop CreateOfflineLoop(
            string suffix,
            long generation)
        {
            Chapter1CampaignStageConfig config = CreateOfflineStageConfig(
                Chapter1CampaignStageConfig.StageOneId);
            BattleLaunchContext context = config.CreateLaunchContext(
                "c1-formal-loop-offline-launch-" + suffix,
                "c1-formal-loop-offline-token-" + suffix,
                generation);
            Require(config.TryValidate(out string configDiagnostic),
                configDiagnostic);
            Require(config.MatchesContext(context),
                "offline stage config does not match its launch context");
            Require(config.RouteEnabled && !config.BattleContentReady,
                "offline stage-one route flags invalid");
            C1FormalItemSessionCreationResult itemCreation =
                C1FormalItemSessionAuthority.Create(
                    context.Token,
                    context.Generation,
                    LoadCanonicalItemResolver());
            Require(itemCreation != null && itemCreation.isSuccess
                    && itemCreation.authority != null,
                itemCreation?.diagnosticCode);
            C1FormalRealtimeBattleSessionAdapter realtimeAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            Require(realtimeAdapter.TryStart(
                    context,
                    itemCreation.authority.CreateBattleInputSnapshot(),
                    itemCreation.authority.Current.canonicalSignature,
                    "c1-formal-loop-offline-session-" + suffix,
                    "c1-formal-loop-offline-live-token-" + suffix,
                    out C1FormalRealtimeBattleSessionStartSnapshot start)
                    && start != null && start.accepted,
                start?.errorCode);
            ConstructorInfo constructor = typeof(
                C1FormalObtainRebuildBattleLoop).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleLaunchContext),
                    typeof(C1FormalRealtimeBattleSessionAdapter),
                    typeof(C1FormalItemSessionAuthority),
                    typeof(C1CampaignStageClearRewardProgressBridge),
                    typeof(C1FormalRealtimeBattleSessionStartSnapshot),
                    typeof(bool)
                },
                null);
            Require(constructor != null,
                "formal loop private creation seam missing");
            return (C1FormalObtainRebuildBattleLoop)constructor.Invoke(
                new object[]
                {
                    context,
                    realtimeAdapter,
                    itemCreation.authority,
                    null,
                    start,
                    false
                });
        }

        private static C1FormalObtainRebuildBattleLoop
            CreateOfflineStageTwoLoop(string suffix, long generation)
        {
            C1FormalObtainRebuildBattleLoop stageOne = CreateOfflineLoop(
                suffix + "-stage-one-source",
                generation);
            AdvanceActiveBattleToTerminal(stageOne);
            Equal(C1FormalObtainRebuildBattlePhase.RewardClaimed,
                stageOne.Phase, "stage-one victory automatically accepts reward");
            PlaceRewardItem(
                stageOne.ItemAuthority,
                suffix + "-place-i002",
                new Vector2Int(3, 3));
            C1FormalItemSessionAuthority authority = stageOne.ItemAuthority;
            BattleLaunchContext stageOneContext = stageOne.StageOneContext;
            stageOne.Reset();

            Chapter1CampaignStageConfig config = CreateOfflineStageConfig(
                Chapter1CampaignStageConfig.StageTwoId);
            BattleLaunchContext context = config.CreateLaunchContext(
                "c1-formal-loop-offline-stage-two-launch-" + suffix,
                "c1-formal-loop-offline-stage-two-token-" + suffix,
                generation + 1L);
            Require(config.TryValidate(out string configDiagnostic),
                configDiagnostic);
            Require(config.MatchesContext(context)
                    && !config.RouteEnabled && !config.BattleContentReady,
                "offline stage-two config/context invalid");
            C1FormalRealtimeBattleSessionAdapter realtimeAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            Require(realtimeAdapter.TryStart(
                    context,
                    authority.CreateBattleInputSnapshot(),
                    authority.Current.canonicalSignature,
                    "c1-formal-loop-offline-stage-two-session-" + suffix,
                    "c1-formal-loop-offline-stage-two-live-token-" + suffix,
                    out C1FormalRealtimeBattleSessionStartSnapshot start)
                    && start != null && start.accepted,
                start?.errorCode);
            ConstructorInfo constructor = typeof(
                C1FormalObtainRebuildBattleLoop).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleLaunchContext),
                    typeof(C1FormalRealtimeBattleSessionAdapter),
                    typeof(C1FormalItemSessionAuthority),
                    typeof(C1CampaignStageClearRewardProgressBridge),
                    typeof(C1FormalRealtimeBattleSessionStartSnapshot),
                    typeof(bool)
                },
                null);
            Require(constructor != null,
                "formal loop private creation seam missing");
            C1FormalObtainRebuildBattleLoop loop =
                (C1FormalObtainRebuildBattleLoop)constructor.Invoke(
                    new object[]
                    {
                        stageOneContext,
                        realtimeAdapter,
                        authority,
                        null,
                        start,
                        false
                    });
            SetPrivateField(loop, "stageTwoContext", context);
            SetPrivateField(
                loop,
                "phase",
                C1FormalObtainRebuildBattlePhase.StageTwoBattle);
            return loop;
        }

        private static CanonicalItemDefinitionResolver
            LoadCanonicalItemResolver()
        {
            ItemBalanceWorkbenchCatalog source =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    "Assets/_Game/Configs/ItemBalanceWorkbench/" +
                    "ItemBalanceWorkbenchCatalog.asset");
            Require(source != null,
                "canonical Item catalog source is present");
            Require(CanonicalItemDefinitionResolver.TryCreate(
                    source,
                    out CanonicalItemDefinitionResolver resolver,
                    out IReadOnlyList<string> errors),
                "canonical Item catalog resolves: " +
                string.Join(",", errors));
            return resolver;
        }

        private static Chapter1CampaignStageConfig CreateOfflineStageConfig(
            string stageId)
        {
#pragma warning disable SYSLIB0050
            Chapter1CampaignStageConfig config =
                (Chapter1CampaignStageConfig)
                FormatterServices.GetUninitializedObject(typeof(
                    Chapter1CampaignStageConfig));
#pragma warning restore SYSLIB0050
            bool stageOne = string.Equals(
                stageId,
                Chapter1CampaignStageConfig.StageOneId,
                StringComparison.Ordinal);
            config.ConfigureForEditor(
                stageId,
                "campaign.normal.lv1.balance.identity.c1",
                stageOne
                    ? "campaign.normal.lv1.encounter.c1.1-1"
                    : "campaign.normal.lv1.encounter.c1.1-2",
                "campaign.normal.lv1.theme.bone_aspect.c1",
                stageOne
                    ? "campaign.normal.lv1.enemy_presentation.c1.1-1"
                    : "campaign.normal.lv1.enemy_presentation.c1.1-2",
                stageOne,
                false);
            return config;
        }

        private static BattleLaunchContext CreateStageTwoContext(
            BattleLaunchContext stageOne,
            string suffix)
        {
            Require(Chapter1CampaignStageCatalog.TryGet(
                    Chapter1CampaignStageConfig.StageTwoId,
                    out Chapter1CampaignStageConfig config,
                    out string diagnostic),
                diagnostic);
            return config.CreateLaunchContext(
                "c1-formal-loop-test-stage-two-launch-" + suffix,
                "c1-formal-loop-test-stage-two-token-" + suffix,
                stageOne.Generation + 1L);
        }

        private static C1FormalItemSessionAuthority NewAuthority(
            string suffix,
            long generation)
        {
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    "c1-formal-loop-test-item-" + suffix,
                    generation);
            Require(creation != null && creation.isSuccess, creation?.diagnosticCode);
            return creation.authority;
        }

        private static C1ExactBattleSandboxItemArrangementPresenter
            CreateAuthorizationPresenter(C1FormalItemSessionAuthority authority)
        {
#pragma warning disable SYSLIB0050
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                (C1ExactBattleSandboxItemArrangementPresenter)
                FormatterServices.GetUninitializedObject(typeof(
                    C1ExactBattleSandboxItemArrangementPresenter));
#pragma warning restore SYSLIB0050
            SetPrivateField(presenter, "authority", authority);
            return presenter;
        }

        private static bool InvokePrepareOrchestration(
            string methodName,
            object[] arguments)
        {
            Type orchestration = typeof(UnifiedBattleFormalSceneHost).Assembly
                .GetType(
                    "TalismanBag.UnifiedBattle.C1FormalPrepareOrchestration",
                    true);
            MethodInfo method = orchestration.GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(method != null,
                "prepare orchestration method missing: " + methodName);
            return (bool)method.Invoke(null, arguments);
        }

        private static void SetPrivateField(
            object target,
            string fieldName,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(field != null, "private field missing: " + fieldName);
            field.SetValue(target, value);
        }

        private static void VerifyStage1To5ConfigsAndWorldMapProgress()
        {
            Chapter1CampaignStageConfig[] configs = Enumerable.Range(1, 5)
                .Select(index => CreateStageConfig("1-" + index))
                .ToArray();
            Require(configs.All(config => config.TryValidate(out _)),
                "all five in-memory StageConfigs must validate");
            Require(Chapter1CampaignStageCatalog.TryBuildValidatedCatalog(
                    configs,
                    out IReadOnlyDictionary<string,
                        Chapter1CampaignStageConfig> catalog,
                    out string diagnostic),
                diagnostic);
            Equal(5, catalog.Count, "exact five-stage catalog");
            Equal("campaign.normal.lv1.theme.bone_aspect.c1",
                catalog["1-1"].StageThemeProfileId, "1-1 interior");
            Equal("campaign.normal.lv1.theme.bone_aspect.c1",
                catalog["1-2"].StageThemeProfileId, "1-2 interior");
            Equal("campaign.normal.lv1.theme.bone_aspect.c1.exterior",
                catalog["1-3"].StageThemeProfileId, "1-3 exterior");
            Equal("campaign.normal.lv1.theme.bone_aspect.c1.exterior",
                catalog["1-4"].StageThemeProfileId, "1-4 exterior");
            Equal("campaign.normal.lv1.theme.bone_aspect.c1.night_animated",
                catalog["1-5"].StageThemeProfileId, "1-5 night");

            MemoryCompletionStorage storage = new MemoryCompletionStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);
            V04WorldMapProgressSnapshot fresh =
                V04WorldMapProgressSnapshot.CreateFromCompletion(
                    repository.Load());
            Require(fresh.formalProgress && fresh.IsAvailable("1-1"),
                "fresh install exposes only 1-1");
            for (int index = 2; index <= 5; index++)
            {
                string stageId = "1-" + index;
                Require(!fresh.IsAvailable(stageId)
                        && !fresh.IsCleared(stageId),
                    "fresh incomplete stage locked " + stageId);
            }

            for (int index = 1; index <= 4; index++)
                Require(repository.MarkCleared("1-" + index),
                    "contiguous completion " + index);
            V04WorldMapProgressSnapshot prefix =
                V04WorldMapProgressSnapshot.CreateFromCompletion(
                    repository.Snapshot);
            for (int index = 1; index <= 4; index++)
                Require(prefix.IsCleared("1-" + index),
                    "cleared replay stage " + index);
            Require(!prefix.IsAvailable("1-5") && !prefix.IsCleared("1-5"),
                "restart leaves first incomplete stage locked");

            foreach (Chapter1CampaignStageConfig config in configs)
                UnityEngine.Object.DestroyImmediate(config);
        }

        private static void VerifyStage1To5RewardItemCompletionChain()
        {
            MemoryCompletionStorage storage = new MemoryCompletionStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);
            const string campaignRunSessionId =
                "stage1to5.campaign.run.session";
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    campaignRunSessionId,
                    1L);
            Require(creation != null && creation.isSuccess
                    && creation.authority != null,
                creation?.diagnosticCode);
            Require(C1CampaignStageClearRewardProgressBridge.TryCreate(
                    creation.authority,
                    repository,
                    campaignRunSessionId,
                    4515L,
                    out C1CampaignStageClearRewardProgressBridge bridge,
                    out string diagnostic),
                diagnostic);

            long generation = 10L;
            int expectedRoster = creation.authority.Current.roster.Count;
            string firstRewardItemInstanceId = string.Empty;
            string firstRewardBaseItemId = string.Empty;
            for (int index = 1; index <= 5; index++)
            {
                string stageId = "1-" + index;
                Chapter1CampaignStageConfig config = CreateStageConfig(stageId);
                BattleLaunchContext context = config.CreateLaunchContext(
                    "focused.launch." + stageId,
                    "focused.token." + stageId,
                    generation++);
                List<C1FormalRealtimeBattleCue> observedCues = index == 2
                    ? new List<C1FormalRealtimeBattleCue>()
                    : null;
                if (index == 2)
                {
                    Require(bridge.TryCreateCumulativeBattleInput(
                            out C1FormalItemBattleInputSnapshot itemInput,
                            out diagnostic),
                        diagnostic);
                    C1FormalItemBattleItemRow transported =
                        itemInput?.itemRows.SingleOrDefault(row => string.Equals(
                            row.itemInstanceId,
                            firstRewardItemInstanceId,
                            StringComparison.Ordinal));
                    Require(transported != null
                            && transported.isPlaced
                            && transported.isLit == true
                            && string.Equals(
                                transported.baseItemId,
                                firstRewardBaseItemId,
                                StringComparison.Ordinal),
                        "placed reward ItemInstance must enter next Battle "
                        + "unchanged");
                }
                C1FormalRealtimeBattleTerminalResult terminal =
                    RunAcceptedRealtimeVictory(
                        context,
                        creation.authority,
                        bridge,
                        observedCues);
                if (index == 2)
                {
                    Require(observedCues.Any(cue =>
                            IsAcceptedItemOriginCueKind(cue.cueKind)
                            && string.Equals(
                                cue.sourceItemInstanceId,
                                firstRewardItemInstanceId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                cue.sourceBaseItemId,
                                firstRewardBaseItemId,
                                StringComparison.Ordinal)
                            && !string.IsNullOrEmpty(cue.effectFamilyId)
                            && !string.IsNullOrEmpty(
                                cue.acceptedApplicationEventId)),
                        "placed reward must produce an authoritative "
                        + "ItemInstance-origin Battle effect cue");
                }
                Require(bridge.TryAcceptVictory(
                        context,
                        terminal,
                        out C1CampaignStageRewardProgressResult accepted,
                        out diagnostic),
                    diagnostic);
                Require(accepted != null && accepted.itemChanged
                        && accepted.completionChanged && !accepted.replay,
                    "first-clear reward/item/completion chain " + stageId);
                Equal(campaignRunSessionId,
                    accepted.stageClearFact.runSessionId,
                    "stage clear preserves campaign run identity " + stageId);
                Equal(campaignRunSessionId,
                    accepted.grant.stageClearFact.runSessionId,
                    "grant preserves campaign run identity " + stageId);
                Equal(campaignRunSessionId,
                    creation.authority.Current.sessionToken,
                    "Item Authority preserves campaign run identity "
                    + stageId);
                expectedRoster++;
                Equal(expectedRoster, creation.authority.Current.roster.Count,
                    "one roster insertion " + stageId);
                Require(creation.authority.Current.FindRosterEntry(
                            accepted.itemInstanceId) != null
                        && creation.authority.Current.trayLayout.placements.Any(
                            row => string.Equals(
                                row.itemInstanceId,
                                accepted.itemInstanceId,
                                StringComparison.Ordinal)
                                && row.isActiveInTray),
                    "accepted drop appears once in Tray " + stageId);
                if (index == 1)
                {
                    firstRewardItemInstanceId = accepted.itemInstanceId;
                    firstRewardBaseItemId = accepted.selectedBaseItemId;
                    PlaceCanonicalRewardLit(
                        creation.authority,
                        firstRewardItemInstanceId);
                }
                else
                {
                    Require(creation.authority.Current.FindRosterEntry(
                                firstRewardItemInstanceId) != null
                            && creation.authority.Current
                                .FindPlacementByInstanceId(
                                    firstRewardItemInstanceId) != null,
                        "later rewards must preserve the prior placed "
                        + "ItemInstance");
                }
                int writesBeforeDuplicate = storage.SetCount;
                int rosterBeforeDuplicate = creation.authority.Current.roster.Count;
                Require(bridge.TryAcceptVictory(
                        context,
                        terminal,
                        out C1CampaignStageRewardProgressResult duplicate,
                        out diagnostic),
                    diagnostic);
                Equal(accepted.itemInstanceId, duplicate.itemInstanceId,
                    "duplicate returns identical receipt");
                Equal(writesBeforeDuplicate, storage.SetCount,
                    "duplicate completion has zero write");
                Equal(rosterBeforeDuplicate,
                    creation.authority.Current.roster.Count,
                    "duplicate grant has zero roster insertion");
                UnityEngine.Object.DestroyImmediate(config);
            }

            Equal(5, repository.Snapshot.completedStageIds.Count,
                "completion prefix reaches 1-5");
            Equal(5, storage.SetCount,
                "only first clear completion writes persist");

            V04CampaignStageCompletionRepository reloaded =
                new V04CampaignStageCompletionRepository(storage);
            Equal(5, reloaded.Load().completedStageIds.Count,
                "restart loads completion-only prefix");
            const string replayCampaignRunSessionId =
                "stage1to5.replay.campaign.run.session";
            C1FormalItemSessionCreationResult replayItems =
                C1FormalItemSessionAuthority.Create(
                    replayCampaignRunSessionId,
                    2L);
            Require(C1CampaignStageClearRewardProgressBridge.TryCreate(
                    replayItems.authority,
                    reloaded,
                    replayCampaignRunSessionId,
                    9154L,
                    out C1CampaignStageClearRewardProgressBridge replayBridge,
                    out diagnostic),
                diagnostic);
            Chapter1CampaignStageConfig replayConfig = CreateStageConfig("1-4");
            BattleLaunchContext replayContext = replayConfig.CreateLaunchContext(
                "focused.replay.launch.1-4",
                "focused.replay.token.1-4",
                50L);
            C1FormalRealtimeBattleTerminalResult replayTerminal =
                RunAcceptedRealtimeVictory(
                    replayContext,
                    replayItems.authority,
                    replayBridge);
            int replayWritesBefore = storage.SetCount;
            Require(replayBridge.TryAcceptVictory(
                    replayContext,
                    replayTerminal,
                    out C1CampaignStageRewardProgressResult replay,
                    out diagnostic),
                diagnostic);
            Require(replay.replay && replay.itemChanged
                    && !replay.completionChanged,
                "cleared replay grants new Item without completion rewrite");
            Equal(replayCampaignRunSessionId,
                replay.stageClearFact.runSessionId,
                "replay preserves campaign run identity");
            Equal(replayWritesBefore, storage.SetCount,
                "replay completion proof has zero persistence write");
            UnityEngine.Object.DestroyImmediate(replayConfig);
        }

        private static void VerifyCumulativeItemInstanceRewardToNextBattleChain()
        {
            MemoryCompletionStorage storage = new MemoryCompletionStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);
            const string campaignRunSessionId =
                "cumulative.iteminstance.focused.session";
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    campaignRunSessionId,
                    1L,
                    LoadCanonicalItemResolver());
            Require(creation != null && creation.isSuccess
                    && creation.authority != null,
                creation?.diagnosticCode);
            Require(C1CampaignStageClearRewardProgressBridge.TryCreate(
                    creation.authority,
                    repository,
                    campaignRunSessionId,
                    5L,
                    out C1CampaignStageClearRewardProgressBridge bridge,
                    out string diagnostic),
                diagnostic);

            BattleLaunchContext stageOne = CreateFocusedLaunchContext(
                "1-1",
                1L);
            C1FormalRealtimeBattleTerminalResult stageOneTerminal =
                RunAcceptedRealtimeVictory(
                    stageOne,
                    creation.authority,
                    bridge);
            int rosterCountBeforeReward = creation.authority.Current.roster.Count;
            Require(bridge.TryAcceptVictory(
                    stageOne,
                    stageOneTerminal,
                    out C1CampaignStageRewardProgressResult reward,
                    out diagnostic),
                diagnostic);
            Require(reward != null && reward.itemChanged
                    && reward.completionChanged && !reward.replay,
                "1-1 reward must be accepted by the same formal chain");
            string rewardItemInstanceId = reward.itemInstanceId;
            string rewardBaseItemId = reward.selectedBaseItemId;
            Require(!string.IsNullOrEmpty(rewardItemInstanceId)
                    && !string.IsNullOrEmpty(rewardBaseItemId)
                    && reward != null,
                "the formal Reward must select one ordinary Canonical ItemInstance");
            Equal(rosterCountBeforeReward + 1,
                creation.authority.Current.roster.Count,
                "the accepted victory adds exactly one reward ItemInstance");
            C1FormalItemRosterEntrySnapshot rewardedRoster =
                creation.authority.Current.FindRosterEntry(
                    rewardItemInstanceId);
            ItemGeneratedInstanceSnapshot rewardedGenerated =
                rewardedRoster?.ordinaryInstance?.generatedInstance;
            CanonicalItemDefinition rewardedDefinition =
                rewardedRoster?.ordinaryInstance?.canonicalDefinition;
            Require(rewardedGenerated != null
                    && rewardedDefinition != null
                    && string.Equals(
                        rewardedGenerated.itemInstanceId,
                        rewardItemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        rewardedGenerated.baseItemId,
                        rewardBaseItemId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        rewardedDefinition.baseItemId,
                        rewardBaseItemId,
                        StringComparison.Ordinal),
                "Reward and Authority must retain the same generated Item identity and definition");
            Require(C1FormalRealtimeBattleCanonicalEffectOperator.TryResolve(
                    rewardedDefinition.combatEffect,
                    out C1FormalRealtimeBattleCanonicalEffectPlan effectPlan),
                "The rewarded Canonical Item must resolve its own combatEffect: "
                + rewardBaseItemId);
            Require(rewardedDefinition.isOrdinaryDropEligible,
                "The formal Reward must resolve to an ordinary Canonical Item: "
                + rewardBaseItemId);
            long rewardedDamage = ReadGeneratedStat(
                rewardedGenerated,
                "damage");
            long rewardedNianCost = ReadGeneratedStat(
                rewardedGenerated,
                "nianCost");
            long rewardedCooldown = ReadGeneratedStat(
                rewardedGenerated,
                "cooldown");
            Require(rewardedNianCost > 0L
                    && rewardedCooldown > 0L,
                "The real rewarded Item must carry its rolled Battle timing and cost");
            C1FormalItemRosterEntrySnapshot retainedStarter = creation.authority.Current
                .FindRosterEntry(CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
            Require(retainedStarter != null,
                "starter ItemInstance must exist before cumulative reward");
            C1ExactBattleSandboxItemArrangementPresenter rebuildPresenter =
                CreateAuthorizationPresenter(creation.authority);
            C1FormalItemInteractionAuthorizationResult enabled =
                rebuildPresenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        rebuildPresenter.Current,
                        C1FormalItemInteractionMode.PREPARE_ENABLED));
            Require(enabled.accepted && enabled.interactionEnabled,
                "rebuild must enable the same Item Authority binding");
            Require(bridge.TryCreateCumulativeBattleInput(
                    out C1FormalItemBattleInputSnapshot trayInput,
                    out diagnostic),
                diagnostic);
            Equal(creation.authority.Current.canonicalSignature,
                trayInput.sessionCanonicalSignature,
                "Prepare must expose the current cumulative Tray snapshot");
            C1FormalItemBattleItemRow trayRewardRow = trayInput.itemRows
                .SingleOrDefault(row => string.Equals(
                    row.itemInstanceId,
                    rewardItemInstanceId,
                    StringComparison.Ordinal));
            Require(trayRewardRow != null && !trayRewardRow.isPlaced
                    && !trayRewardRow.isLit.HasValue
                    && string.Equals(
                        trayRewardRow.baseItemId,
                        rewardBaseItemId,
                        StringComparison.Ordinal)
                    && ReferenceEquals(
                        rewardedGenerated,
                        trayRewardRow.generatedInstance)
                    && ReferenceEquals(
                        rewardedDefinition,
                        trayRewardRow.canonicalDefinition),
                "the real reward must remain the same inactive Tray instance "
                + "until the player places it during Prepare");
            string beforePlacementSignature =
                creation.authority.Current.canonicalSignature;
            PlaceCanonicalRewardLit(
                creation.authority,
                rewardItemInstanceId,
                effectPlan);
            string postPlacementSignature =
                creation.authority.Current.canonicalSignature;
            Require(!string.Equals(
                        beforePlacementSignature,
                        postPlacementSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        rebuildPresenter.Current.canonicalSignature,
                        postPlacementSignature,
                        StringComparison.Ordinal),
                "Prepare must publish the placed+lit Authority snapshot before 1-2");
            C1FormalItemInteractionAuthorizationResult locked =
                rebuildPresenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        rebuildPresenter.Current,
                        C1FormalItemInteractionMode.BATTLE_LOCKED));
            Require(locked.accepted && !locked.interactionEnabled,
                "closing Prepare must lock the updated snapshot before 1-2");

            Require(bridge.TryCreateCumulativeBattleInput(
                    out C1FormalItemBattleInputSnapshot stageTwoInput,
                    out diagnostic),
                diagnostic);
            Equal(postPlacementSignature,
                stageTwoInput.sessionCanonicalSignature,
                "1-2 feeder must use the post-placement cumulative snapshot");
            C1FormalItemBattleItemRow transported = stageTwoInput.itemRows
                .SingleOrDefault(row => string.Equals(
                    row.itemInstanceId,
                    rewardItemInstanceId,
                    StringComparison.Ordinal));
            Require(transported != null && transported.isPlaced
                    && transported.isLit == true
                    && string.Equals(
                        transported.baseItemId,
                        rewardBaseItemId,
                        StringComparison.Ordinal)
                    && ReferenceEquals(
                        rewardedGenerated,
                        transported.generatedInstance)
                    && ReferenceEquals(
                        rewardedDefinition,
                        transported.canonicalDefinition)
                    && ReadGeneratedStat(
                        transported.generatedInstance,
                        "damage") == rewardedDamage
                    && ReadGeneratedStat(
                        transported.generatedInstance,
                        "nianCost") == rewardedNianCost
                    && ReadGeneratedStat(
                        transported.generatedInstance,
                        "cooldown") == rewardedCooldown,
                "the same placed+lit rewarded Item must enter 1-2 unchanged");

            BattleLaunchContext stageTwo = CreateFocusedLaunchContext(
                "1-2",
                2L);
            List<C1FormalRealtimeBattleCue> observedCues =
                new List<C1FormalRealtimeBattleCue>();
            C1FormalRealtimeBattleSessionAdapter stageTwoAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            CanonicalMutationObservation mutationObservation =
                new CanonicalMutationObservation();
            mutationObservation.CaptureInitialPlayerGuard(0L);
            Require(InvokeMainlineCumulativeStart(
                    stageTwo,
                    creation.authority,
                    bridge,
                    stageTwoAdapter,
                    out C1FormalRealtimeBattleSessionStartSnapshot stageTwoStart,
                    out diagnostic),
                diagnostic);
            Require(stageTwoStart != null && stageTwoStart.accepted,
                stageTwoStart?.errorCode);
            observedCues.AddRange(stageTwoStart.initialCues);
            mutationObservation.Capture(
                stageTwoAdapter.Session,
                stageTwoStart.stateSnapshot);
            AdvanceUntilRewardActionCommitted(
                stageTwo,
                stageTwoAdapter,
                rewardItemInstanceId,
                rewardBaseItemId,
                effectPlan.EffectId,
                observedCues,
                mutationObservation);
            string stateSinkEvidence = RequireCanonicalEffectStateSinkMutation(
                effectPlan,
                rewardItemInstanceId,
                rewardBaseItemId,
                observedCues,
                mutationObservation);
            C1FormalRealtimeBattleCue exactInstanceCue = observedCues
                .FirstOrDefault(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceItemInstanceId,
                        rewardItemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        rewardBaseItemId,
                        StringComparison.Ordinal)
                    && string.IsNullOrEmpty(cue.effectVariantId)
                    && !string.IsNullOrEmpty(
                        cue.acceptedApplicationEventId));
            Require(exactInstanceCue != null,
                "1-2 must emit the exact reward ItemInstance action commit; "
                + "reward=" + rewardItemInstanceId + "/"
                + rewardBaseItemId + " observed=" + string.Join(
                    ";",
                    observedCues
                        .Where(cue => !string.IsNullOrEmpty(
                            cue.sourceItemInstanceId))
                        .Take(12)
                        .Select(cue => string.Join(
                            "/",
                            cue.cueKind,
                            cue.sourceItemInstanceId,
                            cue.sourceBaseItemId,
                            cue.effectFamilyId,
                            cue.acceptedApplicationEventId))));
            MethodInfo transport = typeof(UnifiedBattleFormalSceneHost)
                .GetMethod(
                    "TransportOrderedCuesUnchanged",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Require(transport != null,
                "Mainline ordered cue transport seam is missing");
            IReadOnlyList<C1FormalRealtimeBattleCue> forwardedCues =
                (IReadOnlyList<C1FormalRealtimeBattleCue>)transport.Invoke(
                    null,
                    new object[] { observedCues });
            Require(ReferenceEquals(observedCues, forwardedCues)
                    && forwardedCues.Any(cue => ReferenceEquals(
                        cue,
                        exactInstanceCue)),
                "Mainline must transport the accepted ordered cue list "
                + "and exact-instance cue unchanged");
            string hostSource = ReadProjectSource(
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattleFormalSceneHost.cs");
            string nextStageSource = MethodSource(
                hostSource,
                "private void TryStartStageTwoFromRebuild()",
                "private bool TryBindFormalItemSourceProvider");
            int nextStartIndex = nextStageSource.IndexOf(
                "formalLoop.TryStartNextStage",
                StringComparison.Ordinal);
            int publishIndex = nextStageSource.IndexOf(
                "itemPresenter.PublishCurrent",
                StringComparison.Ordinal);
            int bindProviderIndex = nextStageSource.IndexOf(
                "TryBindFormalItemSourceProvider",
                StringComparison.Ordinal);
            int beginPresentationIndex = nextStageSource.IndexOf(
                "formalPresentationRoot.BeginRealtime",
                StringComparison.Ordinal);
            Require(nextStartIndex >= 0
                    && publishIndex > nextStartIndex
                    && bindProviderIndex > publishIndex
                    && beginPresentationIndex > bindProviderIndex,
                "next-Battle Host lifecycle must start from the latest Authority, "
                + "publish it, bind its source provider, then begin presentation");
            string updateSource = MethodSource(
                hostSource,
                "private void Update()",
                "private void OnDisable()");
            Require(updateSource.Contains("TransportOrderedCuesUnchanged")
                    && updateSource.Contains(
                        "formalLoop.LastRealtimeTick.emittedCues")
                    && updateSource.Contains(
                        "formalPresentationRoot.ConsumeRealtime")
                    && updateSource.Contains("itemPresenter.Current"),
                "accepted next-Battle ordered cues must enter FormalBattlePresentationRoot "
                + "with the same current Item provider snapshot");
            cumulativeOwnerChainEvidence = "baseItemId=" + rewardBaseItemId
                + " itemInstanceId=" + rewardItemInstanceId
                + " stage=1-2"
                + " rolledDamage=" + rewardedDamage
                + " nianCost=" + rewardedNianCost
                + " cooldownTurns=" + rewardedCooldown
                + " placement=placed+lit"
                + " effect=" + effectPlan.EffectId
                + " mutationKind=" + effectPlan.MutationKind
                + " stateSink=" + stateSinkEvidence
                + " cueSource=" + exactInstanceCue.sourceItemInstanceId
                + " presentation=FormalBattlePresentationRoot.ConsumeRealtime";
            Require(creation.authority.Current.FindRosterEntry(
                        retainedStarter.itemInstanceId) != null
                    && creation.authority.Current.FindRosterEntry(
                        rewardItemInstanceId) != null
                    && creation.authority.Current.FindPlacementByInstanceId(
                        rewardItemInstanceId) != null,
                "cumulative Battle must retain prior and rewarded instances");
            Equal(1, storage.SetCount,
                "focused proof persists only the accepted 1-1 clear");
            Equal(1, repository.Snapshot.completedStageIds.Count,
                "focused proof does not require a fabricated 1-2 victory");
        }

        private static string RequireCanonicalEffectStateSinkMutation(
            C1FormalRealtimeBattleCanonicalEffectPlan plan,
            string itemInstanceId,
            string baseItemId,
            IReadOnlyCollection<C1FormalRealtimeBattleCue> cues,
            CanonicalMutationObservation observation)
        {
            Require(plan != null && observation != null,
                "Canonical effect mutation proof input is missing");
            C1FormalRealtimeBattleCue[] actionCommitted = (cues
                    ?? Array.Empty<C1FormalRealtimeBattleCue>())
                .Where(cue => cue != null
                    && string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceItemInstanceId,
                        itemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        baseItemId,
                        StringComparison.Ordinal)
                    && string.IsNullOrEmpty(cue.effectVariantId)
                    && !string.IsNullOrEmpty(cue.acceptedApplicationEventId))
                .ToArray();
            if (plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind.PlayerGuardFlat
                || plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind
                        .PlayerGuardPercent
                || plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind.DamageToGuard
                || plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind.HealToGuard)
            {
                C1FormalRealtimeBattleCue guardAccepted = (cues
                        ?? Array.Empty<C1FormalRealtimeBattleCue>())
                    .FirstOrDefault(cue => cue != null
                        && string.Equals(
                            cue.cueKind,
                            C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                            StringComparison.Ordinal)
                        && string.Equals(
                            cue.sourceItemInstanceId,
                            itemInstanceId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            cue.sourceBaseItemId,
                            baseItemId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            cue.effectVariantId,
                            plan.EffectId,
                            StringComparison.Ordinal)
                        && !string.IsNullOrEmpty(
                            cue.acceptedApplicationEventId)
                        && cue.appliedDamage > 0);
                Require(guardAccepted != null,
                    "Canonical guard mutation has no same-source accepted cue: "
                    + plan.EffectId);
                Require(HasPlayerGuardIncrease(observation.PlayerGuards),
                    "Canonical guard effect emitted acceptance without a real "
                    + "Guard increase: " + plan.EffectId);
                return "PLAYER_GUARD:"
                       + guardAccepted.acceptedApplicationEventId;
            }
            Require(actionCommitted.Length > 0,
                "The rewarded Item never reached a committed action: "
                + baseItemId + "/" + plan.EffectId);
            string applicationId = actionCommitted[0].acceptedApplicationEventId;
            switch (plan.MutationKind)
            {
                case C1FormalRealtimeBattleCanonicalMutationKind.EnemyShellPercent:
                case C1FormalRealtimeBattleCanonicalMutationKind.DirectDamagePercent:
                case C1FormalRealtimeBattleCanonicalMutationKind.ExtraTarget:
                case C1FormalRealtimeBattleCanonicalMutationKind.DamageToBreak:
                case C1FormalRealtimeBattleCanonicalMutationKind.ExtraBreakPercent:
                case C1FormalRealtimeBattleCanonicalMutationKind.ExtraBurnTrigger:
                case C1FormalRealtimeBattleCanonicalMutationKind.BurnConversion:
                case C1FormalRealtimeBattleCanonicalMutationKind.EmberTrigger:
                case C1FormalRealtimeBattleCanonicalMutationKind.ExtraDamagePercent:
                {
                    C1FormalRealtimeBattleCue changed = (cues
                            ?? Array.Empty<C1FormalRealtimeBattleCue>())
                        .FirstOrDefault(cue => cue != null
                            && string.Equals(
                                cue.sourceItemInstanceId,
                                itemInstanceId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                cue.sourceBaseItemId,
                                baseItemId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                cue.effectVariantId,
                                plan.EffectId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                cue.acceptedApplicationEventId,
                                applicationId,
                                StringComparison.Ordinal)
                            && (cue.actorHpBefore != cue.actorHpAfter
                                || cue.shellBefore != cue.shellAfter));
                    Require(changed != null,
                        "Canonical damage/break effect emitted acceptance without "
                        + "an HP or Shell mutation: " + plan.EffectId);
                    return "HP_OR_SHELL:" + changed.targetActorId;
                }
                case C1FormalRealtimeBattleCanonicalMutationKind.DebuffToHeal:
                case C1FormalRealtimeBattleCanonicalMutationKind.PlayerHealFlat:
                    Require(HasPlayerHpIncrease(observation.States),
                        "Canonical heal effect emitted acceptance without a real "
                        + "Player HP increase: " + plan.EffectId);
                    return "PLAYER_HP";
                case C1FormalRealtimeBattleCanonicalMutationKind.PlayerNianRefund:
                    Require(observation.NianTransactions.Any(value =>
                            string.Equals(
                                value.sourceItemInstanceId,
                                itemInstanceId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                value.acceptedApplicationEventId,
                                applicationId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                value.transactionKind,
                                C1FormalRealtimeBattleNianTransactionKinds.Refund,
                                StringComparison.Ordinal)
                            && value.appliedAmount > 0
                            && value.after > value.before),
                        "Canonical refund emitted acceptance without a real Nian "
                        + "increase: " + plan.EffectId);
                    return "NIAN";
                case C1FormalRealtimeBattleCanonicalMutationKind.EnemyCastDelay:
                    Require(HasActionDueTimeChange(
                            observation.States,
                            true),
                        "Canonical control effect emitted acceptance without an "
                        + "Enemy Action delay: " + plan.EffectId);
                    return "ENEMY_ACTION_PROGRESS";
                case C1FormalRealtimeBattleCanonicalMutationKind.CooldownReduction:
                    Require(observation.ActionProgressMutations.Any(value =>
                            string.Equals(
                                value.sourceItemInstanceId,
                                itemInstanceId,
                                StringComparison.Ordinal)
                            && value.appliedStepCount > 0
                            && value.afterDueBattleTimeMs
                            < value.beforeDueBattleTimeMs),
                        "Canonical cooldown effect emitted acceptance without a "
                        + "real Item Action advance: " + plan.EffectId);
                    return "ITEM_ACTION_PROGRESS";
                case C1FormalRealtimeBattleCanonicalMutationKind.BurnStack:
                    Require((cues ?? Array.Empty<C1FormalRealtimeBattleCue>())
                            .Any(cue => cue != null
                                && (string.Equals(
                                        cue.cueKind,
                                        C1FormalRealtimeBattleCueKinds
                                            .StatusApplied,
                                        StringComparison.Ordinal)
                                    || string.Equals(
                                        cue.cueKind,
                                        C1FormalRealtimeBattleCueKinds
                                            .StatusRefreshed,
                                        StringComparison.Ordinal))
                                && string.Equals(
                                    cue.sourceItemInstanceId,
                                    itemInstanceId,
                                    StringComparison.Ordinal)
                                && string.Equals(
                                    cue.sourceBaseItemId,
                                    baseItemId,
                                    StringComparison.Ordinal)
                                && string.Equals(
                                    cue.acceptedApplicationEventId,
                                    applicationId,
                                    StringComparison.Ordinal)),
                        "Canonical status effect has no same-application Status "
                        + "cue: " + plan.EffectId);
                    Require(observation.Statuses.Any(status =>
                            status != null && status.stackCount > 0
                            && status.Contributions.Any(contribution =>
                                string.Equals(
                                    contribution.sourceItemInstanceId,
                                    itemInstanceId,
                                    StringComparison.Ordinal))),
                        "Canonical status effect emitted acceptance without a real "
                        + "BattleStatusState contribution: " + plan.EffectId);
                    return "BATTLE_STATUS_STATE";
                case C1FormalRealtimeBattleCanonicalMutationKind.NianCostReduction:
                case C1FormalRealtimeBattleCanonicalMutationKind.Cleanse:
                case C1FormalRealtimeBattleCanonicalMutationKind.TargetingOverride:
                    throw new InvalidOperationException(
                        "CANONICAL_STATE_SINK_PROOF_GAP baseItemId=" + baseItemId
                        + " effect=" + plan.EffectId
                        + " mutationKind=" + plan.MutationKind);
                default:
                    throw new InvalidOperationException(
                        "CANONICAL_STATE_SINK_UNSUPPORTED baseItemId=" + baseItemId
                        + " effect=" + plan.EffectId
                        + " mutationKind=" + plan.MutationKind);
            }
        }

        private static bool HasPlayerGuardIncrease(
            IReadOnlyList<long> playerGuards)
        {
            for (int index = 1; index < playerGuards.Count; index++)
            {
                if (playerGuards[index] > playerGuards[index - 1])
                    return true;
            }
            return false;
        }

        private static bool HasPlayerHpIncrease(
            IReadOnlyList<C1FormalRealtimeBattleSessionStateSnapshot> states)
        {
            for (int index = 1; index < states.Count; index++)
            {
                if (states[index - 1]?.playerSnapshot != null
                    && states[index]?.playerSnapshot != null
                    && states[index].playerSnapshot.currentHp
                    > states[index - 1].playerSnapshot.currentHp)
                    return true;
            }
            return false;
        }

        private static bool HasActionDueTimeChange(
            IReadOnlyList<C1FormalRealtimeBattleSessionStateSnapshot> states,
            bool movedLater)
        {
            for (int index = 1; index < states.Count; index++)
            {
                Dictionary<string, long> before = states[index - 1]
                    .scheduledActions.ToDictionary(
                        value => value.actionId,
                        value => value.dueBattleTimeMs,
                        StringComparer.Ordinal);
                foreach (C1FormalRealtimeBattleScheduledActionSnapshot action in
                         states[index].scheduledActions)
                {
                    if (!before.TryGetValue(action.actionId, out long dueBefore))
                        continue;
                    if (movedLater
                        ? action.dueBattleTimeMs > dueBefore
                        : action.dueBattleTimeMs < dueBefore)
                        return true;
                }
            }
            return false;
        }

        private sealed class CanonicalMutationObservation
        {
            private readonly HashSet<long> nianSequences = new HashSet<long>();
            private readonly HashSet<string> actionMutationEvents =
                new HashSet<string>(StringComparer.Ordinal);

            internal List<C1FormalRealtimeBattleSessionStateSnapshot> States
            {
                get;
            } = new List<C1FormalRealtimeBattleSessionStateSnapshot>();

            internal List<long> PlayerGuards { get; } = new List<long>();

            internal List<C1FormalRealtimeBattleStatusSnapshot> Statuses
            {
                get;
            } = new List<C1FormalRealtimeBattleStatusSnapshot>();

            internal List<C1FormalRealtimeBattleNianTransactionSnapshot>
                NianTransactions { get; } =
                    new List<C1FormalRealtimeBattleNianTransactionSnapshot>();

            internal List<C1FormalRealtimeBattleActionProgressMutationSnapshot>
                ActionProgressMutations { get; } =
                    new List<
                        C1FormalRealtimeBattleActionProgressMutationSnapshot>();

            internal void CaptureInitialPlayerGuard(long playerGuard)
            {
                PlayerGuards.Add(playerGuard);
            }

            internal void Capture(
                C1FormalRealtimeBattleSession session,
                C1FormalRealtimeBattleSessionStateSnapshot state)
            {
                if (state != null)
                {
                    States.Add(state);
                    if (state.playerSnapshot != null)
                        PlayerGuards.Add(state.playerSnapshot.guard);
                }
                if (session == null)
                    return;

                Statuses.AddRange(session.StatusSnapshots);
                foreach (C1FormalRealtimeBattleNianTransactionSnapshot value in
                         session.NianTransactionLedger)
                {
                    if (nianSequences.Add(value.sequence))
                        NianTransactions.Add(value);
                }
                foreach (
                    C1FormalRealtimeBattleActionProgressMutationSnapshot value in
                    session.ActionProgressMutationLedger)
                {
                    if (actionMutationEvents.Add(value.eventId))
                        ActionProgressMutations.Add(value);
                }
            }
        }

        private static long ReadGeneratedStat(
            ItemGeneratedInstanceSnapshot generated,
            string statId)
        {
            ItemGeneratedStatSnapshot row = generated?.GeneratedStats?
                .SingleOrDefault(value => string.Equals(
                    value.statId,
                    statId,
                    StringComparison.Ordinal));
            return row == null ? 0L : row.rawUnits;
        }

        private static void VerifyClearedReplayContinuesToFormalSuccessor()
        {
            VerifyClearedReplayContinuation("1-2", "1-3", 2);
            VerifyClearedReplayContinuation("1-3", "1-4", 3);
            VerifyClearedReplayContinuation("1-4", "1-5", 4);
            VerifyFinalStageResultAction(true);
            VerifyFinalStageResultAction(false);
        }

        private static void VerifyStage3To4BoneSwapPresentationBinding()
        {
            V04Chapter1StageWavePlanDefinition stageThree =
                V04Chapter1StageWaveEncounterTruth.FindPlan("1-3");
            V04Chapter1StageWavePlanDefinition stageFour =
                V04Chapter1StageWaveEncounterTruth.FindPlan("1-4");
            Require(stageThree != null && stageFour != null,
                "stage 1-3 to 1-4 encounter transition missing");
            Require(stageFour.waves.Count == 1
                    && stageFour.waves[0].actors.Count == 2,
                "stage 1-4 presentation actor count invalid");

            V04Chapter1WaveEntryDefinition houndPlan =
                stageFour.waves[0].actors[0];
            V04Chapter1WaveEntryDefinition remnantPlan =
                stageFour.waves[0].actors[1];
            C1FormalEnemyDefinition hound =
                C1FormalEnemyDefinitionCatalog.FindByContentId(
                    houndPlan.enemyContentId);
            C1FormalEnemyDefinition remnant =
                C1FormalEnemyDefinitionCatalog.FindByContentId(
                    remnantPlan.enemyContentId);
            Require(hound != null && remnant != null,
                "stage 1-4 formal Enemy definitions missing");
            Equal(
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                houndPlan.enemyContentId,
                "stage 1-4 order 0 content identity");
            Equal(
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                hound.RuntimeProfileId,
                "stage 1-4 order 0 runtime presentation identity");
            Equal(
                C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                remnantPlan.enemyContentId,
                "stage 1-4 order 1 content identity");
            Equal(
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                remnant.RuntimeProfileId,
                "stage 1-4 order 1 runtime presentation identity");

            HashSet<string> authoredPresentationKeys =
                ReadFormalEnemyPresentationIdentityKeys();
            Require(authoredPresentationKeys.Contains(
                    FormalEnemyPresentationKey(
                        hound.ContentId,
                        hound.RuntimeProfileId)),
                "stage 1-4 order 0 exact presentation binding rejected");
            Require(authoredPresentationKeys.Contains(
                    FormalEnemyPresentationKey(
                        remnant.ContentId,
                        remnant.RuntimeProfileId)),
                "stage 1-4 order 1 exact presentation binding rejected");
        }

        private static HashSet<string>
            ReadFormalEnemyPresentationIdentityKeys()
        {
            const string profilePath =
                "Assets/_Game/Resources/V04/FormalBattlePresentation/"
                + "FormalBattlePresentationProfile.asset";
            Require(File.Exists(profilePath),
                "formal presentation profile asset missing");
            var keys = new HashSet<string>(StringComparer.Ordinal);
            string pendingContentId = string.Empty;
            foreach (string rawLine in File.ReadLines(profilePath))
            {
                string line = rawLine.Trim();
                const string contentPrefix = "- contentId: ";
                const string runtimePrefix = "runtimeProfileId: ";
                if (line.StartsWith(contentPrefix, StringComparison.Ordinal))
                {
                    pendingContentId = line.Substring(contentPrefix.Length);
                    continue;
                }

                if (pendingContentId.Length == 0
                    || !line.StartsWith(
                        runtimePrefix,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                string runtimeProfileId = line.Substring(
                    runtimePrefix.Length);
                Require(keys.Add(FormalEnemyPresentationKey(
                        pendingContentId,
                        runtimeProfileId)),
                    "duplicate formal enemy presentation identity");
                pendingContentId = string.Empty;
            }

            return keys;
        }

        private static string FormalEnemyPresentationKey(
            string contentId,
            string runtimeProfileId)
        {
            return (contentId ?? string.Empty) + "|"
                + (runtimeProfileId ?? string.Empty);
        }

        private static void VerifyClearedReplayContinuation(
            string stageId,
            string expectedNextStageId,
            int completedPrefixLength)
        {
            MemoryCompletionStorage storage = new MemoryCompletionStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);
            for (int index = 1; index <= completedPrefixLength; index++)
            {
                Require(repository.MarkCleared("1-" + index),
                    "replay completion prefix " + index);
            }

            C1FormalObtainRebuildBattleLoop loop =
                CreateCompletionAwareLoop(
                    stageId,
                    repository,
                    out C1CampaignStageClearRewardProgressBridge bridge);
            Require(loop.IsReplayRun,
                "cleared stage must retain replay reward lineage " + stageId);
            AdvanceActiveBattleToTerminal(loop);
            string diagnostic;
            Require(loop.LastRewardProgress != null
                    && loop.LastRewardProgress.replay,
                "cleared stage victory remains a replay reward " + stageId);
            Require(!loop.ShouldReturnWorldMapAfterRebuild,
                "cleared replay with successor must offer Continue " + stageId);
            Equal(expectedNextStageId,
                loop.PendingNextStageId,
                "cleared replay formal successor " + stageId);

            BattleLaunchContext nextContext = CreateFocusedLaunchContext(
                expectedNextStageId,
                loop.CurrentContext.Generation + 1L);
            C1FormalRealtimeBattleSessionAdapter nextAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            Require(InvokeMainlineCumulativeStart(
                    nextContext,
                    loop.ItemAuthority,
                    bridge,
                    nextAdapter,
                    out C1FormalRealtimeBattleSessionStartSnapshot start,
                    out diagnostic),
                diagnostic);
            Require(start != null && start.accepted,
                start?.errorCode);
            nextAdapter.Unbind();
            loop.Reset();
        }

        private static void VerifyFinalStageResultAction(bool alreadyCleared)
        {
            MemoryCompletionStorage storage = new MemoryCompletionStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);
            int initialPrefixLength = alreadyCleared ? 5 : 4;
            for (int index = 1; index <= initialPrefixLength; index++)
            {
                Require(repository.MarkCleared("1-" + index),
                    "final-stage completion prefix " + index);
            }
            V04WorldMapProgressSnapshot before =
                V04WorldMapProgressSnapshot.CreateFromCompletion(
                    repository.Snapshot);
            if (!alreadyCleared)
            {
                Require(!before.IsCleared("1-5")
                        && !before.IsAvailable("1-5"),
                    "uncleared 1-5 remains locked before accepted victory");
            }

            C1FormalObtainRebuildBattleLoop loop =
                CreateCompletionAwareLoop(
                    "1-5",
                    repository,
                    out _);
            Equal(alreadyCleared,
                loop.IsReplayRun,
                "1-5 replay lineage before victory");
            AdvanceActiveBattleToTerminal(loop);
            Require(loop.ShouldReturnWorldMapAfterRebuild
                    && string.IsNullOrEmpty(loop.PendingNextStageId),
                "1-5 has no formal successor and returns to WorldMap");
            Require(repository.Snapshot.IsCleared("1-5"),
                "accepted 1-5 victory records completion");
            Equal(!alreadyCleared,
                loop.LastRewardProgress.completionChanged,
                "only first accepted 1-5 victory changes completion");
            loop.Reset();
        }

        private static C1FormalObtainRebuildBattleLoop
            CreateCompletionAwareLoop(
                string stageId,
                V04CampaignStageCompletionRepository repository,
                out C1CampaignStageClearRewardProgressBridge bridge)
        {
            string sessionId = "focused.replay.action.session." + stageId;
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(sessionId, 70L);
            Require(creation != null && creation.isSuccess
                    && creation.authority != null,
                creation?.diagnosticCode);
            Require(C1CampaignStageClearRewardProgressBridge.TryCreate(
                    creation.authority,
                    repository,
                    sessionId,
                    7515L,
                    out bridge,
                    out string diagnostic),
                diagnostic);

            BattleLaunchContext context = CreateFocusedLaunchContext(
                stageId,
                70L);
            C1FormalRealtimeBattleSessionAdapter realtimeAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            Require(InvokeMainlineCumulativeStart(
                    context,
                    creation.authority,
                    bridge,
                    realtimeAdapter,
                    out C1FormalRealtimeBattleSessionStartSnapshot start,
                    out diagnostic),
                diagnostic);

            ConstructorInfo constructor = typeof(
                C1FormalObtainRebuildBattleLoop).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleLaunchContext),
                    typeof(C1FormalRealtimeBattleSessionAdapter),
                    typeof(C1FormalItemSessionAuthority),
                    typeof(C1CampaignStageClearRewardProgressBridge),
                    typeof(C1FormalRealtimeBattleSessionStartSnapshot),
                    typeof(bool)
                },
                null);
            Require(constructor != null,
                "completion-aware formal loop creation seam missing");
            return (C1FormalObtainRebuildBattleLoop)constructor.Invoke(
                new object[]
                {
                    context,
                    realtimeAdapter,
                    creation.authority,
                    bridge,
                    start,
                    repository.IsCleared(stageId)
                });
        }

        private static bool IsAcceptedItemOriginCueKind(string cueKind)
        {
            return string.Equals(
                       cueKind,
                       C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                       StringComparison.Ordinal)
                   || string.Equals(
                       cueKind,
                       C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                       StringComparison.Ordinal);
        }

        private static bool InvokeMainlineCumulativeStart(
            BattleLaunchContext context,
            C1FormalItemSessionAuthority authority,
            C1CampaignStageClearRewardProgressBridge bridge,
            C1FormalRealtimeBattleSessionAdapter adapter,
            out C1FormalRealtimeBattleSessionStartSnapshot start,
            out string diagnostic)
        {
            MethodInfo method = typeof(C1FormalObtainRebuildBattleLoop)
                .GetMethod(
                    "TryStartRealtime",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Require(method != null,
                "Mainline cumulative start seam is missing");
            object[] arguments =
            {
                context,
                authority,
                bridge,
                adapter,
                null,
                null
            };
            bool accepted = (bool)method.Invoke(null, arguments);
            start = arguments[4] as C1FormalRealtimeBattleSessionStartSnapshot;
            diagnostic = arguments[5] as string ?? string.Empty;
            return accepted;
        }

        private static void VerifyCampaignRunSessionLineageRejectsMismatch()
        {
            MemoryCompletionStorage storage = new MemoryCompletionStorage();
            V04CampaignStageCompletionRepository repository =
                new V04CampaignStageCompletionRepository(storage);
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    "stage1to5.lineage.item.session",
                    1L);
            Require(creation != null && creation.isSuccess
                    && creation.authority != null,
                creation?.diagnosticCode);
            Require(C1CampaignStageClearRewardProgressBridge.TryCreate(
                    creation.authority,
                    repository,
                    "stage1to5.lineage.reward.session",
                    7715L,
                    out C1CampaignStageClearRewardProgressBridge bridge,
                    out string diagnostic),
                diagnostic);

            Chapter1CampaignStageConfig config = CreateStageConfig("1-1");
            BattleLaunchContext context = config.CreateLaunchContext(
                "focused.lineage.launch.1-1",
                "focused.lineage.token.1-1",
                71L);
            C1FormalRealtimeBattleTerminalResult terminal =
                RunAcceptedRealtimeVictory(
                    context,
                    creation.authority,
                    bridge);
            int rosterBefore = creation.authority.Current.roster.Count;
            int completionWritesBefore = storage.SetCount;
            string rewardSessionBefore = bridge.RewardSession.canonicalSignature;

            Require(!bridge.TryAcceptVictory(
                    context,
                    terminal,
                    out C1CampaignStageRewardProgressResult rejected,
                    out diagnostic),
                "mismatched campaign run identities must reject");
            Equal("CAMPAIGN_LOOT_STAGE_LINEAGE_MISMATCH",
                diagnostic,
                "mismatched campaign run identity diagnostic");
            Require(rejected == null,
                "mismatched campaign run identity returns no result");
            Equal(rosterBefore,
                creation.authority.Current.roster.Count,
                "mismatched campaign run identity has zero Item write");
            Equal(completionWritesBefore,
                storage.SetCount,
                "mismatched campaign run identity has zero completion write");
            Equal(rewardSessionBefore,
                bridge.RewardSession.canonicalSignature,
                "mismatched campaign run identity has zero Reward mutation");
            UnityEngine.Object.DestroyImmediate(config);
        }

        private static void VerifyStage1To5StaticOwnershipBoundaries()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string bridge = File.ReadAllText(Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "C1CampaignStageClearRewardProgressBridge.cs"));
            string loop = File.ReadAllText(Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "C1FormalObtainRebuildBattleLoop.cs"));
            string host = File.ReadAllText(Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattleFormalSceneHost.cs"));
            Require(bridge.Contains(
                        "C1CampaignDirectLootSessionResolver.Resolve")
                    && bridge.Contains(
                        "AcceptPool15CampaignLootEntitlement")
                    && bridge.Contains("completionRepository.MarkCleared"),
                "Mainline bridge must transport Reward鈫扞tem鈫扖ompletion");
            Require(loop.Contains("TryStartNextStage")
                    && loop.Contains("CurrentStageId")
                    && !loop.Contains("entitle-i002"),
                "loop must be generic and not fixed-I002 driven");
            Require(host.Contains("formalLoop.PendingNextStageId")
                    && host.Contains("formalLoop.CurrentStageId")
                    && !host.Contains("鍥哄畾棣栭€氬鍔憋細I002@white"),
                "Host must render current/next stage and direct loot");
            Require(!bridge.Contains("PlayerPrefs")
                    && !loop.Contains("PlayerPrefs")
                    && !host.Contains("PlayerPrefs"),
                "Mainline persists no Item/Reward/formation state");
        }

        private static void VerifyDynamicItemInstanceConsumerContract()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string host = File.ReadAllText(Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/"
                + "UnifiedBattleFormalSceneHost.cs"));
            string authoring = File.ReadAllText(Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/"
                + "C1FormalObtainRebuildBattleLoopSceneAuthoring.cs"));
            string singlePathAuthoring = File.ReadAllText(Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/"
                + "C1UnifiedSingleFormalPathAuthoring.cs"));
            string normalizedHost = new string(host.Where(
                value => !char.IsWhiteSpace(value)).ToArray());
            string normalizedAuthoring = new string(authoring.Where(
                value => !char.IsWhiteSpace(value)).ToArray());
            string normalizedSinglePath = new string(singlePathAuthoring.Where(
                value => !char.IsWhiteSpace(value)).ToArray());

            Require(!host.Contains("AuthoredPresentationCapacity")
                    && !host.Contains("exactCardViews")
                    && normalizedHost.Contains(
                        "itemBoardView.PresentationCatalogSnapshot==null")
                    && normalizedHost.Contains(
                        "formalPresentationRoot.BindItemSourceProvider("
                        + "itemPresenter,catalog,outstringproviderDiagnostic)")
                    && normalizedHost.Contains(
                        "formalPresentationRoot.UnbindItemSourceProvider()"),
                "Host must bind the exact dynamic provider/catalog lifecycle "
                + "without Card count admission");
            Require(!authoring.Contains("AuthoredPresentationCapacity")
                    && !authoring.Contains("exactCards=")
                    && normalizedAuthoring.Contains(
                        "ValidateDynamicItemPresentationMount(board,tray)"),
                "Mainline Scene authoring still freezes Item Card capacity");
            Require(!singlePathAuthoring.Contains(
                        "AuthoredPresentationCapacity")
                    && !normalizedSinglePath.Contains(
                        "AssignExternalCompositionForEditor("
                        + "damageFloatPool,cueFxAudioRoot,cardViews)")
                    && normalizedSinglePath.Contains(
                        "ExecuteDynamicItemProviderShellCleanupFromCommandLine")
                    && normalizedSinglePath.Contains(
                        "C1_DYNAMIC_ITEM_PROVIDER_SHELL_CLEANUP_AND_VALIDATION_PASS")
                    && normalizedSinglePath.Contains(
                        "DYNAMIC_ITEM_PROVIDER_UNRELATED_OVERRIDE_DRIFT"),
                "Shell cleanup/provider authoring boundary is incomplete");

            MethodInfo transport = typeof(UnifiedBattleFormalSceneHost)
                .GetMethod(
                    "TransportOrderedCuesUnchanged",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Require(transport != null,
                "ordered Battle cue transport seam missing");
            foreach (int count in new[] { 0, 1, 7, 8, 15 })
            {
                List<C1FormalRealtimeBattleCue> cues = Enumerable
                    .Range(0, count)
                    .Select(index => CreateItemOriginCue(
                        index,
                        "dynamic.instance." + index))
                    .ToList();
                IReadOnlyList<C1FormalRealtimeBattleCue> transported =
                    (IReadOnlyList<C1FormalRealtimeBattleCue>)transport.Invoke(
                        null,
                        new object[] { cues });
                Require(ReferenceEquals(cues, transported)
                        && transported.Count == count
                        && transported.Select(value => value.sequence)
                            .SequenceEqual(cues.Select(value => value.sequence))
                        && transported.Select(value =>
                                value.sourceItemInstanceId)
                            .SequenceEqual(cues.Select(value =>
                                value.sourceItemInstanceId)),
                    "ordered Item-origin cue transport changed count=" + count);
            }

            C1FormalRealtimeBattleCue duplicate = CreateItemOriginCue(
                1,
                "duplicate.instance");
            List<C1FormalRealtimeBattleCue> duplicateAndStale = new()
            {
                CreateItemOriginCue(2, "later.instance"),
                duplicate,
                duplicate,
                CreateItemOriginCue(0, "stale.instance")
            };
            IReadOnlyList<C1FormalRealtimeBattleCue> unchanged =
                (IReadOnlyList<C1FormalRealtimeBattleCue>)transport.Invoke(
                    null,
                    new object[] { duplicateAndStale });
            Require(ReferenceEquals(duplicateAndStale, unchanged)
                    && ReferenceEquals(unchanged[1], unchanged[2])
                    && unchanged[0].sequence == 3L
                    && unchanged[3].sequence == 1L,
                "Mainline filtered, deduplicated or reordered Battle cues");

            string accept = MethodSource(
                host,
                "private void TryAcceptContextOnce()",
                "private C1FormalItemDetailAdmissionState");
            string clear = MethodSource(
                host,
                "private void ClearFormalLoop()",
                "private void ResetStageThemePresentation()");
            string next = MethodSource(
                host,
                "private void TryStartStageTwoFromRebuild()",
                "private void ResetPrepareSurfaceForLifecycle()");
            Require(accept.Contains("TryBindFormalItemSourceProvider")
                    && accept.Contains("UnbindItemSourceProvider")
                    && clear.Contains("UnbindItemSourceProvider")
                    && next.Contains("UnbindItemSourceProvider")
                    && next.Contains("TryBindFormalItemSourceProvider"),
                "provider bind/unbind reset-generation lifecycle incomplete");
        }

        private static C1FormalRealtimeBattleCue CreateItemOriginCue(
            int index,
            string itemInstanceId)
        {
            return new C1FormalRealtimeBattleCue(
                "dynamic.cue." + index,
                index + 1L,
                index * 100L,
                C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                C1FormalRealtimeBattleSessionContract.PlayerOwner,
                itemInstanceId,
                "enemy.0",
                0,
                100,
                100,
                100,
                90,
                10,
                10,
                "-10",
                100,
                "dynamic.application." + index,
                itemInstanceId,
                "I003",
                "direct_damage",
                "default");
        }

        private static C1FormalRealtimeBattleTerminalResult
            RunAcceptedRealtimeVictory(
                BattleLaunchContext context,
                C1FormalItemSessionAuthority authority,
                C1CampaignStageClearRewardProgressBridge bridge,
                ICollection<C1FormalRealtimeBattleCue> observedCues = null)
        {
            Require(bridge.TryCreateCumulativeBattleInput(
                    out C1FormalItemBattleInputSnapshot itemInput,
                    out string diagnostic),
                diagnostic);
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            Require(adapter.TryStartFromCumulativeItemSnapshot(
                    context,
                    itemInput,
                    authority.Current.canonicalSignature,
                    "focused.live.session." + context.StageId,
                    "focused.live.token." + context.StageId,
                    out C1FormalRealtimeBattleSessionStartSnapshot start),
                start?.errorCode);
            return AdvanceAcceptedRealtimeVictory(
                context,
                adapter,
                observedCues);
        }

        private static C1FormalRealtimeBattleTerminalResult
            AdvanceAcceptedRealtimeVictory(
                BattleLaunchContext context,
                C1FormalRealtimeBattleSessionAdapter adapter,
                ICollection<C1FormalRealtimeBattleCue> observedCues = null,
                CanonicalMutationObservation mutationObservation = null)
        {
            C1FormalRealtimeBattleTerminalResult terminal = null;
            for (int guard = 0; guard < 1000 && !adapter.IsTerminal; guard++)
            {
                Require(adapter.TryAdvanceBy(
                        500L,
                        out C1FormalRealtimeBattleTickResult tick),
                    tick?.errorCode);
                if (observedCues != null && tick?.emittedCues != null)
                {
                    foreach (C1FormalRealtimeBattleCue cue in tick.emittedCues)
                        observedCues.Add(cue);
                }
                mutationObservation?.Capture(
                    adapter.Session,
                    tick?.stateSnapshot);
                if (tick.terminalResult != null)
                    terminal = tick.terminalResult;
            }

            adapter.Unbind();
            Require(terminal != null && terminal.accepted && terminal.win
                    && !terminal.lose,
                "accepted real-time victory " + context.StageId);
            return terminal;
        }

        private static void AdvanceUntilRewardActionCommitted(
            BattleLaunchContext context,
            C1FormalRealtimeBattleSessionAdapter adapter,
            string itemInstanceId,
            string baseItemId,
            string effectId,
            ICollection<C1FormalRealtimeBattleCue> observedCues,
            CanonicalMutationObservation mutationObservation)
        {
            Require(context != null && adapter != null
                    && !string.IsNullOrEmpty(itemInstanceId)
                    && !string.IsNullOrEmpty(baseItemId)
                    && !string.IsNullOrEmpty(effectId)
                    && observedCues != null
                    && mutationObservation != null,
                "Canonical effect focused advancement input is missing");

            bool actionCommitted = false;
            for (int guard = 0;
                 guard < 1000 && !adapter.IsTerminal && !actionCommitted;
                 guard++)
            {
                Require(adapter.TryAdvanceBy(
                        500L,
                        out C1FormalRealtimeBattleTickResult tick),
                    tick?.errorCode);
                if (tick?.emittedCues != null)
                {
                    foreach (C1FormalRealtimeBattleCue cue in tick.emittedCues)
                    {
                        observedCues.Add(cue);
                        if (cue != null
                            && string.Equals(
                                cue.cueKind,
                                C1FormalRealtimeBattleCueKinds
                                    .ItemTriggerAccepted,
                                StringComparison.Ordinal)
                            && string.Equals(
                                cue.sourceItemInstanceId,
                                itemInstanceId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                cue.sourceBaseItemId,
                                baseItemId,
                                StringComparison.Ordinal)
                            && string.IsNullOrEmpty(cue.effectVariantId)
                            && !string.IsNullOrEmpty(
                                cue.acceptedApplicationEventId))
                            actionCommitted = true;
                    }
                }
                mutationObservation.Capture(
                    adapter.Session,
                    tick?.stateSnapshot);
            }

            adapter.Unbind();
            Require(actionCommitted,
                "The placed+lit reward never committed an action before Battle "
                + "terminal: stage=" + context.StageId
                + " reward=" + itemInstanceId + "/" + baseItemId
                + " effect=" + effectId);
        }

        private static void PlaceCanonicalRewardLit(
            C1FormalItemSessionAuthority authority,
            string itemInstanceId,
            C1FormalRealtimeBattleCanonicalEffectPlan effectPlan = null)
        {
            Vector2Int[] preferredAnchors =
            {
                new Vector2Int(2, 1),
                new Vector2Int(1, 2),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(0, 0),
                new Vector2Int(2, 2),
                new Vector2Int(0, 2)
            };
            Vector2Int[] anchors = preferredAnchors.Concat(
                    Enumerable.Range(0, C1FormalItemSessionContract.BoardSize)
                        .SelectMany(y => Enumerable.Range(
                                0,
                                C1FormalItemSessionContract.BoardSize)
                            .Select(x => new Vector2Int(x, y))))
                .Distinct()
                .ToArray();
            int attempt = 0;
            foreach (int rotation in new[] { 0, 90, 180, 270 })
            {
                foreach (Vector2Int anchor in anchors)
                {
                    attempt++;
                    C1FormalItemSessionOperationResult placed = authority.Submit(
                        new C1FormalItemArrangementCommand(
                            C1FormalItemArrangementCommandKind.PlaceFromTray,
                            authority.Current.sessionToken,
                            authority.Current.resetGeneration,
                            "focused.canonical.reward.place." + attempt,
                            itemInstanceId,
                            authority.Current.canonicalSignature,
                            new C1FormalItemPlacementCandidate(
                                anchor,
                                rotation)));
                    if (placed == null || !placed.accepted || !placed.changed)
                        continue;

                    C1FormalItemPlacementSnapshot placement = authority.Current
                        .FindPlacementByInstanceId(itemInstanceId);
                    if (placement != null
                        && authority.Current.itemSystemSnapshot.FindPlacement(
                            placement.placementId)?.isLit == true)
                    {
                        bool requiresHorizontalNeighbor = effectPlan != null
                            && effectPlan.MutationKind ==
                            C1FormalRealtimeBattleCanonicalMutationKind
                                .PlayerGuardPercent
                            && string.Equals(
                                effectPlan.TriggerEventId,
                                "on_layout_evaluate",
                                StringComparison.Ordinal)
                            && string.Equals(
                                effectPlan.ConditionId,
                                "horizontal_adjacent",
                                StringComparison.Ordinal);
                        if (!requiresHorizontalNeighbor
                            || TryEnsureHorizontalOrdinaryLitNeighbor(
                                authority,
                                itemInstanceId,
                                attempt))
                            return;
                    }

                    ReturnItemToTray(
                        authority,
                        "focused.canonical.reward.return." + attempt,
                        itemInstanceId);
                }
            }

            throw new InvalidOperationException(
                "No legal lit Board placement exists for reward ItemInstance "
                + itemInstanceId);
        }

        private static bool TryEnsureHorizontalOrdinaryLitNeighbor(
            C1FormalItemSessionAuthority authority,
            string sourceItemInstanceId,
            int placementAttempt)
        {
            if (HasHorizontalOrdinaryLitNeighbor(
                    authority,
                    sourceItemInstanceId))
                return true;

            C1FormalItemSessionSnapshot snapshot = authority.Current;
            C1FormalItemPlacementSnapshot sourcePlacement = snapshot
                .FindPlacementByInstanceId(sourceItemInstanceId);
            var sourceSystemPlacement = sourcePlacement == null
                ? null
                : snapshot.itemSystemSnapshot.FindPlacement(
                    sourcePlacement.placementId);
            C1FormalItemRosterEntrySnapshot neighborRoster = snapshot.roster
                .Where(row => row != null
                    && !row.isSpecialLightingSource
                    && !string.Equals(
                        row.itemInstanceId,
                        sourceItemInstanceId,
                        StringComparison.Ordinal)
                    && snapshot.FindPlacementByInstanceId(
                        row.itemInstanceId) != null)
                .OrderBy(row => row.itemInstanceId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (sourceSystemPlacement == null || neighborRoster == null)
                return false;

            Vector2Int[] neighborAnchors = sourceSystemPlacement.OccupiedCells
                .SelectMany(cell => new[]
                {
                    new Vector2Int(cell.x - 1, cell.y),
                    new Vector2Int(cell.x + 1, cell.y)
                })
                .Where(cell => cell.x >= 0
                    && cell.x < C1FormalItemSessionContract.BoardSize
                    && cell.y >= 0
                    && cell.y < C1FormalItemSessionContract.BoardSize)
                .Distinct()
                .ToArray();
            int attempt = 0;
            foreach (Vector2Int anchor in neighborAnchors)
            {
                attempt++;
                C1FormalItemPlacementSnapshot currentNeighbor = authority.Current
                    .FindPlacementByInstanceId(neighborRoster.itemInstanceId);
                C1FormalItemSessionOperationResult moved = authority.Submit(
                    new C1FormalItemArrangementCommand(
                        C1FormalItemArrangementCommandKind.MoveOnBoard,
                        authority.Current.sessionToken,
                        authority.Current.resetGeneration,
                        "focused.canonical.horizontal-neighbor." + placementAttempt + "." + attempt,
                        neighborRoster.itemInstanceId,
                        authority.Current.canonicalSignature,
                        new C1FormalItemPlacementCandidate(
                            anchor,
                            currentNeighbor?.rotation ?? 0)));
                if (moved?.accepted == true
                    && HasHorizontalOrdinaryLitNeighbor(
                        authority,
                        sourceItemInstanceId))
                    return true;
            }
            return false;
        }

        private static bool HasHorizontalOrdinaryLitNeighbor(
            C1FormalItemSessionAuthority authority,
            string sourceItemInstanceId)
        {
            C1FormalItemSessionSnapshot snapshot = authority?.Current;
            C1FormalItemPlacementSnapshot sourcePlacement = snapshot?
                .FindPlacementByInstanceId(sourceItemInstanceId);
            var sourceSystemPlacement = sourcePlacement == null
                ? null
                : snapshot.itemSystemSnapshot.FindPlacement(
                    sourcePlacement.placementId);
            if (sourceSystemPlacement?.isLit != true)
                return false;

            foreach (C1FormalItemRosterEntrySnapshot row in snapshot.roster
                         .Where(row => row != null
                             && !row.isSpecialLightingSource
                             && !string.Equals(
                                 row.itemInstanceId,
                                 sourceItemInstanceId,
                                 StringComparison.Ordinal)))
            {
                C1FormalItemPlacementSnapshot placement = snapshot
                    .FindPlacementByInstanceId(row.itemInstanceId);
                var systemPlacement = placement == null
                    ? null
                    : snapshot.itemSystemSnapshot.FindPlacement(
                        placement.placementId);
                if (systemPlacement?.isLit != true)
                    continue;
                if (sourceSystemPlacement.OccupiedCells.Any(left =>
                        systemPlacement.OccupiedCells.Any(right =>
                            Math.Abs(left.x - right.x) == 1
                            && left.y == right.y)))
                    return true;
            }
            return false;
        }

        private static Chapter1CampaignStageConfig CreateStageConfig(
            string stageId)
        {
            int index = int.Parse(stageId.Substring(2));
            Chapter1CampaignStageConfig config = ScriptableObject.CreateInstance<
                Chapter1CampaignStageConfig>();
            string theme = index <= 2
                ? "campaign.normal.lv1.theme.bone_aspect.c1"
                : index <= 4
                    ? "campaign.normal.lv1.theme.bone_aspect.c1.exterior"
                    : "campaign.normal.lv1.theme.bone_aspect.c1.night_animated";
            config.ConfigureForEditor(
                stageId,
                Chapter1CampaignStageConfig.BalanceProfileIdValue,
                Chapter1CampaignStageConfig.EncounterVariantIdFor(stageId),
                theme,
                Chapter1CampaignStageConfig.EnemyPresentationProfileIdFor(
                    stageId),
                true,
                true);
            return config;
        }

        private static BattleLaunchContext CreateFocusedLaunchContext(
            string stageId,
            long generation)
        {
            int stageIndex = int.Parse(stageId.Substring(2));
            string themeProfileId = stageIndex <= 2
                ? "campaign.normal.lv1.theme.bone_aspect.c1"
                : stageIndex <= 4
                    ? "campaign.normal.lv1.theme.bone_aspect.c1.exterior"
                    : "campaign.normal.lv1.theme.bone_aspect.c1.night_animated";
            return new BattleLaunchContext(
                BattleLaunchContext.SchemaId,
                BattleLaunchContext.CurrentSchemaVersion,
                "focused.cumulative.launch." + stageId,
                "focused.cumulative.token." + stageId,
                generation,
                Chapter1CampaignStageConfig.ProductContextId,
                Chapter1CampaignStageConfig.ChapterIdValue,
                stageId,
                Chapter1CampaignStageConfig.BalanceProfileIdValue,
                Chapter1CampaignStageConfig.EncounterVariantIdFor(stageId),
                themeProfileId,
                Chapter1CampaignStageConfig.EnemyPresentationProfileIdFor(
                    stageId),
                Chapter1CampaignStageConfig.SourceRouteId,
                Chapter1CampaignStageConfig.ReturnRouteId);
        }

        private sealed class MemoryCompletionStorage :
            IV04CampaignStageCompletionStorage
        {
            private readonly Dictionary<string, string> values =
                new Dictionary<string, string>(StringComparer.Ordinal);

            internal int SetCount { get; private set; }

            public bool HasKey(string key) => values.ContainsKey(key);

            public string GetString(string key) => values.TryGetValue(
                key,
                out string value) ? value : string.Empty;

            public void SetString(string key, string value)
            {
                values[key] = value;
                SetCount++;
            }

            public void DeleteKey(string key)
            {
                values.Remove(key);
            }
        }

        private static void PlaceRewardItem(
            C1FormalItemSessionAuthority authority,
            string commandId,
            Vector2Int cell)
        {
            C1FormalItemSessionOperationResult result = authority.Submit(
                new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    commandId,
                    RewardedItemInstanceId(authority),
                    authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(cell, 0)));
            Require(result.accepted && result.changed, result.diagnosticCode);
        }

        private static string RewardedItemInstanceId(
            C1FormalItemSessionAuthority authority)
        {
            return authority.Current.roster
                .Where(row => !row.isSpecialLightingSource
                    && !string.Equals(
                        row.itemInstanceId,
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                        StringComparison.Ordinal))
                .Select(row => row.itemInstanceId)
                .Single();
        }

        private static void PlaceI001(
            C1FormalItemSessionAuthority authority,
            string commandId,
            Vector2Int cell)
        {
            C1FormalItemSessionOperationResult result = authority.Submit(
                new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    commandId,
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                    authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(cell, 0)));
            Require(result.accepted && result.changed, result.diagnosticCode);
        }

        private static void ReturnItemToTray(
            C1FormalItemSessionAuthority authority,
            string commandId,
            string itemInstanceId)
        {
            C1FormalItemSessionOperationResult result = authority.Submit(
                new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.ReturnToTray,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    commandId,
                    itemInstanceId,
                    authority.Current.canonicalSignature,
                    null));
            Require(result.accepted && result.changed, result.diagnosticCode);
        }

        private static long NextPendingActionDue(
            C1FormalRealtimeBattleSessionStateSnapshot state,
            string actionKind,
            string sourceId)
        {
            C1FormalRealtimeBattleScheduledActionSnapshot action =
                state?.scheduledActions
                    .Where(row => !row.executed
                        && string.Equals(
                            row.actionKind,
                            actionKind,
                            StringComparison.Ordinal)
                        && (string.IsNullOrEmpty(sourceId)
                            || string.Equals(
                                row.sourceId,
                                sourceId,
                                StringComparison.Ordinal)))
                    .OrderBy(row => row.dueBattleTimeMs)
                    .FirstOrDefault();
            return action == null ? -1L : action.dueBattleTimeMs;
        }

        private static void MoveRewardItem(
            C1FormalItemSessionAuthority authority,
            string commandId,
            Vector2Int cell)
        {
            C1FormalItemSessionOperationResult result = authority.Submit(
                new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.MoveOnBoard,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    commandId,
                    RewardedItemInstanceId(authority),
                    authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(cell, 0)));
            Require(result.accepted && result.changed, result.diagnosticCode);
        }

        private static string StableLoopId(
            BattleLaunchContext context,
            string suffix)
        {
            return "c1.formal.loop." + context.LaunchId + "." +
                   context.Generation + "." + suffix;
        }

        private static void Require(bool condition, string message)
        {
            assertionCount++;
            if (!condition)
            {
                throw new InvalidOperationException(message ?? "assertion failed");
            }
        }

        private static void Equal<T>(T expected, T actual, string message)
        {
            assertionCount++;
            if (!Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    message + " expected=" + expected + " actual=" + actual);
            }
        }
    }
}

