using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.WorldMap
{
    public sealed class V04WorldMapStageNodeView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text stageLabelText;
        [SerializeField] private Text stateLabelText;
        [SerializeField] private Image stateBadgeImage;
        [SerializeField] private GameObject bossMarkerRoot;

        private Action<string> onSelected;
        private string stageId = string.Empty;
        private bool runtimeBound;

        public string StageId => stageId;

        public bool HasAuthoredBindings =>
            button != null
            && stageLabelText != null
            && stateLabelText != null
            && stateBadgeImage != null
            && bossMarkerRoot != null;

        public void Bind(
            V04WorldMapStageDefinition stage,
            V04WorldMapStageVisualStateDefinition state,
            Action<string> selected)
        {
            if (stage == null || state == null)
            {
                return;
            }

            stageId = stage.stageId;
            onSelected = selected;
            stageLabelText.text = stage.stageId;
            stateLabelText.text = state.displayName;
            if (ColorUtility.TryParseHtmlString(
                    state.badgeColorHtml,
                    out Color badgeColor))
            {
                stateBadgeImage.color = badgeColor;
            }

            bossMarkerRoot.SetActive(stage.isBossStage);
            if (!runtimeBound)
            {
                button.onClick.AddListener(HandleSelected);
                runtimeBound = true;
            }
        }

        public void UnbindRuntime()
        {
            if (runtimeBound && button != null)
            {
                button.onClick.RemoveListener(HandleSelected);
            }

            runtimeBound = false;
            onSelected = null;
        }

        public void Configure(
            Button configuredButton,
            Text configuredStageLabel,
            Text configuredStateLabel,
            Image configuredStateBadge,
            GameObject configuredBossMarker)
        {
            button = configuredButton;
            stageLabelText = configuredStageLabel;
            stateLabelText = configuredStateLabel;
            stateBadgeImage = configuredStateBadge;
            bossMarkerRoot = configuredBossMarker;
        }

        private void HandleSelected()
        {
            onSelected?.Invoke(stageId);
        }
    }
}
