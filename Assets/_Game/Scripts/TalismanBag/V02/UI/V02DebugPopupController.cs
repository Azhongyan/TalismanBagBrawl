using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02DebugPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            openButton?.onClick.AddListener(Show);
            closeButton?.onClick.AddListener(Hide);
            Hide();
        }

        public void Show()
        {
            panel?.SetActive(true);
        }

        public void Hide()
        {
            panel?.SetActive(false);
        }
    }
}
