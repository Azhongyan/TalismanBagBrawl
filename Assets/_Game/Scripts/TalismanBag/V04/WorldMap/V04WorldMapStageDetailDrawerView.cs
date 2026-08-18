using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TalismanBag.V04.WorldMap
{
    public sealed class V04WorldMapStageDetailDrawerView : MonoBehaviour
    {
        [SerializeField] private Text stageTitleText;
        [SerializeField] private Text stageStateText;
        [SerializeField] private Text enemySummaryText;
        [SerializeField] private Text rankRangeText;
        [SerializeField] private Text firstClearRewardText;
        [SerializeField] private Text diagnosticText;
        [SerializeField] private V04WorldMapDropPreviewRowView[] dropPreviewRows;
        [SerializeField] private Button challengeButton;
        [SerializeField] private Button setPatrolButton;
        [SerializeField] private Button closeButton;

        private UnityAction closeAction;
        private Action<string> challengeAction;
        private string selectedStageId = string.Empty;
        private string baseDiagnostic = string.Empty;
        private bool challengeBound;

        public string SelectedStageId => selectedStageId;

        public bool HasAuthoredBindings =>
            stageTitleText != null
            && stageStateText != null
            && enemySummaryText != null
            && rankRangeText != null
            && firstClearRewardText != null
            && diagnosticText != null
            && challengeButton != null
            && setPatrolButton != null
            && closeButton != null
            && dropPreviewRows != null
            && dropPreviewRows.Length == 3
            && Array.TrueForAll(
                dropPreviewRows,
                row => row != null && row.HasAuthoredBindings);

        public void BindClose(UnityAction action)
        {
            if (closeButton == null || action == null || closeAction != null)
            {
                return;
            }

            closeAction = action;
            closeButton.onClick.AddListener(closeAction);
        }

        public void UnbindClose()
        {
            if (closeButton != null && closeAction != null)
            {
                closeButton.onClick.RemoveListener(closeAction);
            }

            closeAction = null;
        }

        public void BindChallenge(
            string stageId,
            bool routeReady,
            Action<string> action,
            string routeDiagnostic)
        {
            UnbindChallenge();
            selectedStageId = string.IsNullOrWhiteSpace(stageId)
                ? string.Empty
                : stageId.Trim();
            challengeAction = action;
            challengeButton.interactable = routeReady
                && challengeAction != null
                && !string.IsNullOrWhiteSpace(selectedStageId);
            challengeButton.onClick.AddListener(HandleChallenge);
            challengeBound = true;
            SetChallengeDiagnostic(routeDiagnostic);
        }

        public void UnbindChallenge()
        {
            if (challengeBound && challengeButton != null)
            {
                challengeButton.onClick.RemoveListener(HandleChallenge);
            }

            challengeBound = false;
            challengeAction = null;
            selectedStageId = string.Empty;
            if (challengeButton != null)
            {
                challengeButton.interactable = false;
            }
        }

        public void SetChallengeDiagnostic(string routeDiagnostic)
        {
            if (diagnosticText == null)
            {
                return;
            }

            diagnosticText.text = baseDiagnostic + "\n挑战：" +
                                  (string.IsNullOrWhiteSpace(routeDiagnostic)
                                      ? "FORMAL_ROUTE_NOT_READY"
                                      : routeDiagnostic.Trim());
        }

        public void Show(
            V04WorldMapStageDefinition stage,
            V04WorldMapStageVisualStateDefinition state,
            string progressAuthority)
        {
            if (stage == null || state == null)
            {
                return;
            }

            UnbindChallenge();
            gameObject.SetActive(true);
            stageTitleText.text = stage.displayName;
            stageStateText.text = "节点状态：" + state.displayName;
            enemySummaryText.text = stage.enemySummary;
            rankRangeText.text = stage.rankRangeLabel;
            firstClearRewardText.text = stage.firstClearRewardLabel;

            for (int index = 0; index < dropPreviewRows.Length; index++)
            {
                V04WorldMapDropPreviewDefinition definition =
                    index < (stage.dropPreviewRows?.Count ?? 0)
                        ? stage.dropPreviewRows[index]
                        : null;
                dropPreviewRows[index].Bind(definition);
            }

            setPatrolButton.interactable = false;
            baseDiagnostic =
                "巡行：PATROL_OWNER_UNBOUND\n" +
                "进度：" + (string.IsNullOrWhiteSpace(progressAuthority)
                    ? V04WorldMapProgressSnapshot.UnboundAuthority
                    : progressAuthority);
            SetChallengeDiagnostic("FORMAL_ROUTE_NOT_READY");
        }

        public void Hide()
        {
            UnbindChallenge();
            gameObject.SetActive(false);
        }

        public void Configure(
            Text configuredStageTitle,
            Text configuredStageState,
            Text configuredEnemySummary,
            Text configuredRankRange,
            Text configuredFirstClearReward,
            Text configuredDiagnostic,
            V04WorldMapDropPreviewRowView[] configuredDropRows,
            Button configuredChallenge,
            Button configuredSetPatrol,
            Button configuredClose)
        {
            stageTitleText = configuredStageTitle;
            stageStateText = configuredStageState;
            enemySummaryText = configuredEnemySummary;
            rankRangeText = configuredRankRange;
            firstClearRewardText = configuredFirstClearReward;
            diagnosticText = configuredDiagnostic;
            dropPreviewRows = configuredDropRows;
            challengeButton = configuredChallenge;
            setPatrolButton = configuredSetPatrol;
            closeButton = configuredClose;
        }

        private void HandleChallenge()
        {
            if (challengeButton == null || !challengeButton.interactable)
            {
                return;
            }

            challengeAction?.Invoke(selectedStageId);
        }
    }
}
