using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Combat
{
    public sealed class CombatSpeedController : MonoBehaviour
    {
        [SerializeField] private Button toggleButton;
        [SerializeField] private Text label;
        [SerializeField] private float normalSpeed = 1f;
        [SerializeField] private float fastSpeed = 2f;

        public float CurrentSpeedMultiplier { get; private set; } = 1f;

        private void Awake()
        {
            CurrentSpeedMultiplier = normalSpeed;
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleSpeed);
            }

            RefreshLabel();
        }

        public void ToggleSpeed()
        {
            CurrentSpeedMultiplier = Mathf.Approximately(CurrentSpeedMultiplier, normalSpeed) ? fastSpeed : normalSpeed;
            RefreshLabel();
        }

        public void SetSpeed(float multiplier)
        {
            CurrentSpeedMultiplier = Mathf.Approximately(multiplier, fastSpeed) ? fastSpeed : normalSpeed;
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (label != null)
            {
                label.text = Mathf.Approximately(CurrentSpeedMultiplier, fastSpeed) ? "2x" : "1x";
            }
        }
    }
}
