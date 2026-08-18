using System;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.WorldMap
{
    public sealed class V04WorldMapChapterEntryView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text availabilityText;

        private Action<string> onSelected;
        private string chapterId = string.Empty;
        private bool runtimeBound;

        public string ChapterId => chapterId;

        public bool HasAuthoredBindings =>
            button != null
            && titleText != null
            && subtitleText != null
            && availabilityText != null;

        public void Bind(
            V04WorldMapChapterDefinition definition,
            Action<string> selected)
        {
            if (definition == null)
            {
                return;
            }

            chapterId = definition.chapterId;
            onSelected = selected;
            titleText.text = definition.displayName;
            subtitleText.text = definition.subtitle;
            availabilityText.text = definition.availableInVerticalSlice
                ? "进入章节"
                : "后续章节入口（本包未开放）";

            if (!runtimeBound)
            {
                button.onClick.AddListener(HandleSelected);
                runtimeBound = true;
            }
        }

        public void UnbindRuntime()
        {
            if (runtimeBound && button != null)
            {
                button.onClick.RemoveListener(HandleSelected);
            }

            runtimeBound = false;
            onSelected = null;
        }

        public void Configure(
            Button configuredButton,
            Text configuredTitle,
            Text configuredSubtitle,
            Text configuredAvailability)
        {
            button = configuredButton;
            titleText = configuredTitle;
            subtitleText = configuredSubtitle;
            availabilityText = configuredAvailability;
        }

        private void HandleSelected()
        {
            onSelected?.Invoke(chapterId);
        }
    }
}
