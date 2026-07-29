using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation.Projection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class LayoutResilienceBattleSandboxPlaytestInstallDecision
    {
        internal LayoutResilienceBattleSandboxPlaytestInstallDecision(
            bool shouldInstall,
            LayoutResilienceBattleSandboxPlaytestStatus status,
            LayoutResilienceBattleSandboxPlaytestIssue issue)
        {
            this.shouldInstall = shouldInstall;
            this.status = status;
            this.issue = issue;
        }

        public bool shouldInstall { get; }
        public LayoutResilienceBattleSandboxPlaytestStatus status { get; }
        public LayoutResilienceBattleSandboxPlaytestIssue issue { get; }
    }

    public sealed class LayoutResilienceBattleSandboxPlaytestRefreshGate
    {
        private object lastSnapshotReference;

        public bool ShouldEvaluate(object snapshotReference)
        {
            if (snapshotReference == null || ReferenceEquals(
                lastSnapshotReference, snapshotReference))
            {
                return false;
            }
            lastSnapshotReference = snapshotReference;
            return true;
        }
    }

    [DisallowMultipleComponent]
    public sealed class LayoutResilienceBattleSandboxPlaytestAdapter : MonoBehaviour
    {
        public const string PackageKey =
            "V0.4-LayoutResilienceBattleSandboxPlaytestAdapter01";
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        public const string FeedbackObjectName = "ItemSandboxFeedbackText";
        public const string FeedbackParentName = "FeedbackRoot";

        private readonly LayoutResilienceBattleSandboxPlaytestRefreshGate
            refreshGate = new LayoutResilienceBattleSandboxPlaytestRefreshGate();
        private ItemSandboxV04BoardFullDetailAdapter sourceAdapter;
        private Text feedbackText;
        private LayoutResilienceBattleSandboxPlaytestIssue feedbackBindingIssue;
        private bool feedbackBindingResolved;
        private int if01ValidateCallCount;
        private int p6EvaluateCallCount;

        public LayoutResilienceBattleSandboxPlaytestSnapshot CurrentSnapshot
        {
            get;
            private set;
        }
        public int If01ValidateCallCount => if01ValidateCallCount;
        public int P6EvaluateCallCount => p6EvaluateCallCount;

        private void Awake()
        {
            sourceAdapter = GetComponent<ItemSandboxV04BoardFullDetailAdapter>();
        }

        private void LateUpdate()
        {
            if (sourceAdapter == null)
            {
                sourceAdapter = GetComponent<
                    ItemSandboxV04BoardFullDetailAdapter>();
            }
            ItemFullDetailBuildSandboxWorkbenchSession session =
                sourceAdapter?.Session;
            ItemFullDetailWorkbenchSnapshot snapshot = session?.Snapshot;
            if (refreshGate.ShouldEvaluate(snapshot))
            {
                if01ValidateCallCount++;
                p6EvaluateCallCount++;
                CurrentSnapshot = EvaluateSession(session);
                ResolveFeedbackBinding();
                if (feedbackBindingIssue != null)
                {
                    CurrentSnapshot = CurrentSnapshot.WithIssue(
                        feedbackBindingIssue);
                }
            }
            ApplyFeedback();
        }

        private void ResolveFeedbackBinding()
        {
            if (feedbackBindingResolved)
            {
                return;
            }
            feedbackBindingResolved = true;
            TryResolveFeedbackText(transform, out feedbackText,
                out feedbackBindingIssue);
            if (feedbackBindingIssue != null)
            {
                Debug.LogError("[LayoutResiliencePlaytest] " +
                    feedbackBindingIssue.message, this);
            }
        }

        private void ApplyFeedback()
        {
            if (feedbackText == null || CurrentSnapshot == null)
            {
                return;
            }
            string next = LayoutResilienceBattleSandboxPlaytestFeedback
                .ApplyToExistingFeedback(
                    feedbackText.text,
                    CurrentSnapshot.aggregateFeedbackText);
            if (!string.Equals(feedbackText.text, next,
                StringComparison.Ordinal))
            {
                feedbackText.text = next;
            }
        }

        public static LayoutResilienceBattleSandboxPlaytestInstallDecision
            DecideInstallation(string activeScenePath, int sourceAdapterCount)
        {
            if (!string.Equals(activeScenePath, TargetScenePath,
                StringComparison.Ordinal))
            {
                return new LayoutResilienceBattleSandboxPlaytestInstallDecision(
                    false,
                    LayoutResilienceBattleSandboxPlaytestStatus.Complete,
                    null);
            }
            if (sourceAdapterCount != 1)
            {
                return new LayoutResilienceBattleSandboxPlaytestInstallDecision(
                    false,
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                    new LayoutResilienceBattleSandboxPlaytestIssue(
                        sourceAdapterCount == 0
                            ? "ITEM_SANDBOX_ADAPTER_MISSING"
                            : "ITEM_SANDBOX_ADAPTER_DUPLICATE",
                        LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                        "Scene/ItemSandboxV04BoardFullDetailAdapter",
                        sourceAdapterCount == 0
                            ? "目标场景缺少唯一道具沙盒适配器。"
                            : "目标场景存在重复道具沙盒适配器。"));
            }
            return new LayoutResilienceBattleSandboxPlaytestInstallDecision(
                true,
                LayoutResilienceBattleSandboxPlaytestStatus.Complete,
                null);
        }

        public static bool TryResolveFeedbackText(
            Transform adapterRoot,
            out Text resolved,
            out LayoutResilienceBattleSandboxPlaytestIssue issue)
        {
            resolved = null;
            issue = null;
            if (adapterRoot == null)
            {
                issue = new LayoutResilienceBattleSandboxPlaytestIssue(
                    "FEEDBACK_ROOT_MISSING",
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                    "ItemSandboxRoot",
                    "找不到道具沙盒根节点。");
                return false;
            }

            Text[] matches = adapterRoot.GetComponentsInChildren<Text>(true)
                .Where(value => value != null && string.Equals(
                    value.gameObject.name,
                    FeedbackObjectName,
                    StringComparison.Ordinal))
                .ToArray();
            bool exactPath = matches.Length == 1 &&
                matches[0].transform.parent != null &&
                matches[0].transform.parent.parent == adapterRoot &&
                string.Equals(matches[0].transform.parent.name,
                    FeedbackParentName, StringComparison.Ordinal);
            if (!exactPath)
            {
                issue = new LayoutResilienceBattleSandboxPlaytestIssue(
                    matches.Length == 0
                        ? "FEEDBACK_TEXT_MISSING"
                        : matches.Length > 1
                            ? "FEEDBACK_TEXT_DUPLICATE"
                            : "FEEDBACK_TEXT_PATH_INVALID",
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                    "ItemSandboxRoot/FeedbackRoot/ItemSandboxFeedbackText",
                    matches.Length == 0
                        ? "找不到唯一阵势韧性反馈文字。"
                        : matches.Length > 1
                            ? "发现重复阵势韧性反馈文字。"
                            : "阵势韧性反馈文字不在指定层级。");
                return false;
            }
            resolved = matches[0];
            return true;
        }

        public static LayoutResilienceBattleSandboxPlaytestSnapshot EvaluateSession(
            ItemFullDetailBuildSandboxWorkbenchSession session)
        {
            ItemSystemSnapshot itemSnapshot = session?.Snapshot?.placementSnapshot;
            ItemFullDetailWorkbenchPlacement[] ordinaryPlacements =
                (session?.Placements ??
                    Array.Empty<ItemFullDetailWorkbenchPlacement>())
                .Where(value => value != null && !value.isJuNian &&
                    !string.Equals(value.baseItemId, "I031",
                        StringComparison.Ordinal))
                .ToArray();
            HashSet<string> placedInstanceIds = new HashSet<string>(
                ordinaryPlacements
                    .Where(value => !string.IsNullOrWhiteSpace(
                        value.itemInstanceId))
                    .Select(value => value.itemInstanceId),
                StringComparer.Ordinal);
            ItemInstanceProjectionContractSnapshot[] projections =
                (session?.Instances ??
                    Array.Empty<ItemFullDetailWorkbenchInstance>())
                .Where(value => value?.Projection != null &&
                    placedInstanceIds.Contains(value.itemInstanceId))
                .Select(value => value.Projection)
                .ToArray();
            ItemInstancePlacementBindingInput[] bindings = ordinaryPlacements
                .Select(value => new ItemInstancePlacementBindingInput(
                    value.itemInstanceId,
                    value.placementId,
                    value.baseItemId))
                .ToArray();
            return EvaluateAuthoritySnapshot(
                itemSnapshot,
                projections,
                bindings,
                ItemInstancePlacementBindingValidator.Instance,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        public static LayoutResilienceBattleSandboxPlaytestSnapshot
            EvaluateAuthoritySnapshot(
                ItemSystemSnapshot itemSnapshot,
                IReadOnlyList<ItemInstanceProjectionContractSnapshot> projections,
                IReadOnlyList<ItemInstancePlacementBindingInput> bindings,
                IItemInstancePlacementBindingValidator bindingValidator,
                IRealLayoutResilienceEvaluationPipeline pipeline)
        {
            string itemSignature = BuildItemSnapshotSignature(itemSnapshot);
            List<LayoutResilienceBattleSandboxPlaytestIssue> localIssues =
                new List<LayoutResilienceBattleSandboxPlaytestIssue>();
            ItemInstancePlacementBindingValidationResult bindingResult = null;
            try
            {
                ItemInstanceProjectionSetSnapshot projectionSet = new
                    ItemInstanceProjectionSetSnapshot(
                        projections ??
                            Array.Empty<ItemInstanceProjectionContractSnapshot>(),
                        Array.Empty<ItemInstanceProjectionValidationError>());
                bindingResult = (bindingValidator ??
                        ItemInstancePlacementBindingValidator.Instance)
                    .Validate(projectionSet, itemSnapshot, bindings);
            }
            catch (Exception exception)
            {
                localIssues.Add(new LayoutResilienceBattleSandboxPlaytestIssue(
                    "IF01_AUTHORITY_EXCEPTION",
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                    "IF01",
                    "IF01 validation did not complete: " +
                    exception.GetType().Name + "."));
            }

            RealLayoutResilienceEvaluationPipelineResult pipelineResult = null;
            try
            {
                string batchId = PackageKey + "|" + itemSignature;
                pipelineResult = (pipeline ??
                        DefaultRealLayoutResilienceEvaluationPipeline.Instance)
                    .Evaluate(new RealLayoutResilienceEvaluationPipelineInput(
                        batchId,
                        itemSnapshot,
                        bindingResult?.snapshot));
            }
            catch (Exception exception)
            {
                localIssues.Add(new LayoutResilienceBattleSandboxPlaytestIssue(
                    "P6_AUTHORITY_EXCEPTION",
                    LayoutResilienceBattleSandboxPlaytestStatus.Invalid,
                    "P6",
                    "P6 evaluation did not complete: " +
                    exception.GetType().Name + "."));
            }

            return LayoutResilienceBattleSandboxPlaytestFeedback.FromAuthorities(
                itemSignature,
                bindingResult,
                pipelineResult,
                localIssues);
        }

        public static string BuildItemSnapshotSignature(
            ItemSystemSnapshot itemSnapshot)
        {
            return LayoutResilienceBattleSandboxPlaytestCanonical.HashText(
                itemSnapshot == null
                    ? string.Empty
                    : itemSnapshot.BuildDebugSignature());
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterEditorPlayBootstrap()
        {
            SceneManager.sceneLoaded -= OnEditorPlaySceneLoaded;
            SceneManager.sceneLoaded += OnEditorPlaySceneLoaded;
        }

        private static void OnEditorPlaySceneLoaded(
            Scene scene,
            LoadSceneMode mode)
        {
            if (!string.Equals(scene.path, TargetScenePath,
                StringComparison.Ordinal))
            {
                return;
            }
            ItemSandboxV04BoardFullDetailAdapter[] adapters =
                UnityEngine.Object
                    .FindObjectsOfType<ItemSandboxV04BoardFullDetailAdapter>(true)
                    .Where(value => value != null &&
                        value.gameObject.scene == scene)
                    .ToArray();
            LayoutResilienceBattleSandboxPlaytestInstallDecision decision =
                DecideInstallation(scene.path, adapters.Length);
            if (!decision.shouldInstall)
            {
                if (decision.issue != null)
                {
                    Debug.LogError("[LayoutResiliencePlaytest] " +
                        decision.issue.message);
                }
                return;
            }
            if (adapters[0].GetComponent<
                    LayoutResilienceBattleSandboxPlaytestAdapter>() == null)
            {
                adapters[0].gameObject.AddComponent<
                    LayoutResilienceBattleSandboxPlaytestAdapter>();
            }
        }
#endif
    }
}
