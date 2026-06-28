using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02DebugPopupController : MonoBehaviour
    {
        private const int DebugSortingOrder = 32760;

        [SerializeField] private GameObject panel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            ConfigureTopLayer(openButton != null ? openButton.gameObject : null);
            ConfigureTopLayer(panel);
            openButton?.onClick.AddListener(Show);
            closeButton?.onClick.AddListener(Hide);
            Hide();
            BringToFront();
        }

        private void LateUpdate()
        {
            BringToFront();
        }

        public void Show()
        {
            ConfigureTopLayer(panel);
            panel?.SetActive(true);
            BringToFront();
        }

        public void Hide()
        {
            panel?.SetActive(false);
        }

        public void BringToFront()
        {
            ConfigureTopLayer(openButton != null ? openButton.gameObject : null);
            ConfigureTopLayer(panel);
            openButton?.transform.SetAsLastSibling();
            panel?.transform.SetAsLastSibling();
        }

        private static void ConfigureTopLayer(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            Canvas canvas = target.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = target.AddComponent<Canvas>();
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = DebugSortingOrder;

            if (target.GetComponent<GraphicRaycaster>() == null)
            {
                target.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
