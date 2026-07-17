using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    [DisallowMultipleComponent]
    public sealed class ItemSandboxDetailSectionView : MonoBehaviour
    {
        [SerializeField] private ItemDetailSectionView runtimeSection;

        public string Title => runtimeSection != null ? runtimeSection.Title : string.Empty;
        public string Body => runtimeSection != null ? runtimeSection.Body : string.Empty;

#if UNITY_EDITOR
        public void ConfigureEditor(ItemDetailSectionView configuredRuntimeSection)
        {
            runtimeSection = configuredRuntimeSection;
        }
#endif

        public void SetContent(ItemDetailSectionViewModel model)
        {
            if (runtimeSection != null)
            {
                runtimeSection.SetContent(model);
            }
        }

        public void SetContent(string title, string body, bool keepWhenEmpty = false)
        {
            if (runtimeSection != null)
            {
                runtimeSection.SetContent(title, body, keepWhenEmpty);
            }
        }
    }
}
