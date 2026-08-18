using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleStatusSlotView : MonoBehaviour
    {
        [SerializeField] private Image statusTint;
        [SerializeField] private Image statusIcon;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private TMP_Text statusStack;
        [SerializeField] private TMP_Text statusDuration;

        public RectTransform RectTransform => transform as RectTransform;

        public bool ValidateAuthoredReferences()
        {
            return RectTransform != null
                   && statusTint != null
                   && statusIcon != null
                   && statusLabel != null
                   && statusStack != null
                   && statusDuration != null
                   && !statusTint.raycastTarget
                   && !statusIcon.raycastTarget;
        }

        public bool Show(
            Sprite icon,
            Color foregroundColor,
            string stackText,
            string durationText)
        {
            if (!ValidateAuthoredReferences() || icon == null)
            {
                Clear();
                return false;
            }

            gameObject.SetActive(true);
            statusTint.gameObject.SetActive(false);
            statusTint.enabled = false;
            statusIcon.sprite = icon;
            statusIcon.color = Color.white;
            statusIcon.enabled = true;
            statusLabel.text = string.Empty;
            statusStack.text = stackText ?? string.Empty;
            statusStack.color = foregroundColor;
            statusDuration.text = durationText ?? string.Empty;
            statusDuration.color = foregroundColor;
            return true;
        }

        public void Clear()
        {
            if (statusIcon != null)
            {
                statusIcon.sprite = null;
                statusIcon.enabled = false;
            }
            if (statusLabel != null)
            {
                statusLabel.text = string.Empty;
            }
            if (statusStack != null)
            {
                statusStack.text = string.Empty;
            }
            if (statusDuration != null)
            {
                statusDuration.text = string.Empty;
            }
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            Image configuredTint,
            Image configuredIcon,
            TMP_Text configuredLabel,
            TMP_Text configuredStack,
            TMP_Text configuredDuration)
        {
            statusTint = configuredTint;
            statusIcon = configuredIcon;
            statusLabel = configuredLabel;
            statusStack = configuredStack;
            statusDuration = configuredDuration;
        }

        public void ShowPreviewForEditor(
            Sprite icon,
            Color iconColor,
            Color textColor,
            string stackText,
            string durationText)
        {
            if (!ValidateAuthoredReferences() || icon == null)
            {
                return;
            }

            gameObject.SetActive(true);
            statusTint.gameObject.SetActive(false);
            statusTint.enabled = false;
            statusIcon.sprite = icon;
            statusIcon.color = iconColor;
            statusIcon.enabled = true;
            statusLabel.text = string.Empty;
            statusStack.text = stackText ?? string.Empty;
            statusStack.color = textColor;
            statusDuration.text = durationText ?? string.Empty;
            statusDuration.color = textColor;
        }
#endif
    }
}
