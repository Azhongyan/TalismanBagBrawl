using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    [DisallowMultipleComponent]
    public sealed class ItemSandboxDetailPanelView : MonoBehaviour
    {
        [SerializeField] private ItemDetailPanelView runtimeView;

#if UNITY_EDITOR
        public void ConfigureEditor(ItemDetailPanelView configuredRuntimeView)
        {
            runtimeView = configuredRuntimeView;
        }
#endif

        public void Show(ItemDetailViewModel model)
        {
            if (runtimeView != null)
            {
                runtimeView.Bind(model);
            }
        }

        public void ShowDetailTab()
        {
            if (runtimeView != null)
            {
                runtimeView.ShowPlayerDetailTab();
            }
        }

        public void ShowDebugTab()
        {
            if (runtimeView != null)
            {
                runtimeView.ShowDebugTab();
            }
        }

        public void ResetScrollToTop()
        {
            if (runtimeView != null)
            {
                runtimeView.ResetScrollToTop();
            }
        }

        public void SetVisible(bool visible)
        {
            if (runtimeView != null)
            {
                runtimeView.SetVisible(visible);
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }
    }
}
