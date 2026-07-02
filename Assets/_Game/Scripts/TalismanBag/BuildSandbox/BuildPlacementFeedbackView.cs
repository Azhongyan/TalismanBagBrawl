using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BuildPlacementFeedbackView : MonoBehaviour
    {
        [SerializeField] private Text feedbackText;
        [SerializeField] private Image backgroundImage;

        private static readonly Color NeutralColor = new(0.18f, 0.14f, 0.09f, 1f);
        private static readonly Color ValidColor = new(0.13f, 0.30f, 0.18f, 1f);
        private static readonly Color InvalidColor = new(0.42f, 0.13f, 0.10f, 1f);
        private static readonly Color InfoColor = new(0.18f, 0.22f, 0.32f, 1f);

        public string CurrentMessage => feedbackText == null ? string.Empty : feedbackText.text;

        public void Bind(Text text, Image background)
        {
            feedbackText = text;
            backgroundImage = background;
        }

        public void ShowNeutral(string message)
        {
            Set(message, NeutralColor);
        }

        public void ShowValid(string message)
        {
            Set(message, ValidColor);
        }

        public void ShowInvalid(string message)
        {
            Set(message, InvalidColor);
        }

        public void ShowInfo(string message)
        {
            Set(message, InfoColor);
        }

        private void Set(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message ?? string.Empty;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = color;
            }
        }
    }
}
