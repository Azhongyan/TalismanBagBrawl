using TalismanBag.Debugging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class VersionInfoUI : MonoBehaviour, IPointerClickHandler
    {
        public const string VersionText = "TalismanBag v0.1.0-Run15-Mobile";

        [SerializeField] private Text label;
        [SerializeField] private PlaytestDebugPanel debugPanel;
        [SerializeField] private int tapsRequired = 5;

        private int tapCount;

        private void Awake()
        {
            if (label != null)
            {
                label.text = VersionText;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            tapCount++;
            if (tapCount < tapsRequired)
            {
                return;
            }

            tapCount = 0;
            debugPanel?.Toggle();
            PlaytestSessionLogger.Log("Debug Panel Toggled");
        }
    }
}
