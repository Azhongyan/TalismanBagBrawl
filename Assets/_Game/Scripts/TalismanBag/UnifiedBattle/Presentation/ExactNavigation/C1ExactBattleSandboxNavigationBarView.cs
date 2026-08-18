using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UnifiedBattle.Presentation.ExactNavigation
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxNavigationBarView : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Text backLabel;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Text primaryLabel;
        [SerializeField] private Button arrangementButton;
        [SerializeField] private Text arrangementLabel;
        [SerializeField] private Button speedButton;
        [SerializeField] private Text speedLabel;
        [SerializeField] private Button battleLogButton;
        [SerializeField] private Text battleLogLabel;

        public Button BackButton => backButton;
        public Button PrimaryButton => primaryButton;
        public Button ArrangementButton => arrangementButton;
        public Button SpeedButton => speedButton;
        public Button BattleLogButton => battleLogButton;
        public string PrimaryLabelText => primaryLabel == null
            ? string.Empty
            : primaryLabel.text;
        public string SpeedLabelText => speedLabel == null
            ? string.Empty
            : speedLabel.text;
        public string BattleLogLabelText => battleLogLabel == null
            ? string.Empty
            : battleLogLabel.text;

        public bool ValidateAuthoredReferences()
        {
            Button[] buttons =
            {
                backButton,
                primaryButton,
                arrangementButton,
                speedButton,
                battleLogButton
            };
            Text[] labels =
            {
                backLabel,
                primaryLabel,
                arrangementLabel,
                speedLabel,
                battleLogLabel
            };

            if (Array.Exists(buttons, value => value == null)
                || Array.Exists(labels, value => value == null)
                || new HashSet<Button>(buttons).Count != buttons.Length)
            {
                return false;
            }

            for (int index = 0; index < buttons.Length; index++)
            {
                if (buttons[index].transform.parent != transform
                    || labels[index].transform.parent !=
                    buttons[index].transform
                    || labels[index].raycastTarget)
                {
                    return false;
                }
            }

            return string.Equals(
                       battleLogLabel.text,
                       "战斗日志",
                       StringComparison.Ordinal)
                   && battleLogButton != speedButton
                   && !string.Equals(
                       speedLabel.text,
                       battleLogLabel.text,
                       StringComparison.Ordinal);
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            Button configuredBackButton,
            Text configuredBackLabel,
            Button configuredPrimaryButton,
            Text configuredPrimaryLabel,
            Button configuredArrangementButton,
            Text configuredArrangementLabel,
            Button configuredSpeedButton,
            Text configuredSpeedLabel,
            Button configuredBattleLogButton,
            Text configuredBattleLogLabel)
        {
            backButton = configuredBackButton;
            backLabel = configuredBackLabel;
            primaryButton = configuredPrimaryButton;
            primaryLabel = configuredPrimaryLabel;
            arrangementButton = configuredArrangementButton;
            arrangementLabel = configuredArrangementLabel;
            speedButton = configuredSpeedButton;
            speedLabel = configuredSpeedLabel;
            battleLogButton = configuredBattleLogButton;
            battleLogLabel = configuredBattleLogLabel;
        }
#endif

        public void RenderPrimary(string label, bool interactable)
        {
            if (primaryLabel != null)
            {
                primaryLabel.text = label ?? string.Empty;
            }

            if (primaryButton != null)
            {
                primaryButton.interactable = interactable;
            }
        }

        public void RenderAuxiliary(
            int playbackRate,
            bool arrangementInteractable,
            bool speedInteractable,
            bool battleLogInteractable)
        {
            if (backButton != null)
            {
                backButton.interactable = true;
            }

            if (arrangementButton != null)
            {
                arrangementButton.interactable = arrangementInteractable;
            }

            if (speedButton != null)
            {
                speedButton.interactable = speedInteractable;
            }

            if (battleLogButton != null)
            {
                battleLogButton.interactable = battleLogInteractable;
            }

            if (speedLabel != null)
            {
                speedLabel.text = playbackRate > 1
                    ? "加速 2x"
                    : "加速 1x";
            }
        }

        public void ResetRuntimeState()
        {
            RenderPrimary("继续战斗", false);
            RenderAuxiliary(1, false, false, false);
        }
    }
}
