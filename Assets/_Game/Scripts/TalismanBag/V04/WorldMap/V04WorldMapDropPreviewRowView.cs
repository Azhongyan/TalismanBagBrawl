using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.WorldMap
{
    public sealed class V04WorldMapDropPreviewRowView : MonoBehaviour
    {
        [SerializeField] private Text displayNameText;
        [SerializeField] private Text bindingStatusText;

        public bool HasAuthoredBindings =>
            displayNameText != null && bindingStatusText != null;

        public void Bind(V04WorldMapDropPreviewDefinition definition)
        {
            displayNameText.text = definition?.displayName ?? "未绑定掉落槽位";
            bindingStatusText.text = definition?.bindingStatus
                ?? V04WorldMapCatalog.UnboundDropStatus;
        }

        public void Configure(Text configuredDisplayName, Text configuredBindingStatus)
        {
            displayNameText = configuredDisplayName;
            bindingStatusText = configuredBindingStatus;
        }
    }
}
