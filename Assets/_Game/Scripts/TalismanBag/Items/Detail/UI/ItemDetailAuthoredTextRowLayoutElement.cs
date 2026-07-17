using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.Items.Detail.UI
{
    [DisallowMultipleComponent]
    public sealed class ItemDetailAuthoredTextRowLayoutElement : UIBehaviour, ILayoutElement
    {
        [SerializeField] private Text rowText;
        [SerializeField, Min(0f)] private float minimumHeight;
        [SerializeField, Min(0f)] private float verticalPadding;

        public float minWidth => -1f;
        public float preferredWidth => -1f;
        public float flexibleWidth => -1f;
        public float minHeight => minimumHeight;
        public float preferredHeight => MeasurePreferredHeight();
        public float flexibleHeight => -1f;
        public int layoutPriority => 100;
        public Text RowText => rowText;
        public float MinimumHeight => minimumHeight;

        public void CalculateLayoutInputHorizontal()
        {
        }

        public void CalculateLayoutInputVertical()
        {
        }

        public void InvalidateLayout()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            RectTransform rowRect = transform as RectTransform;
            if (rowRect != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rowRect);
                if (rowRect.parent is RectTransform rowsRoot)
                {
                    LayoutRebuilder.MarkLayoutForRebuild(rowsRoot);
                }
            }
        }

#if UNITY_EDITOR
        public void ConfigureEditor(Text configuredRowText, float configuredMinimumHeight, float configuredVerticalPadding)
        {
            rowText = configuredRowText;
            minimumHeight = Mathf.Max(0f, configuredMinimumHeight);
            verticalPadding = Mathf.Max(0f, configuredVerticalPadding);
            InvalidateLayout();
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            InvalidateLayout();
        }

        protected override void OnDisable()
        {
            InvalidateLayout();
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            minimumHeight = Mathf.Max(0f, minimumHeight);
            verticalPadding = Mathf.Max(0f, verticalPadding);
            InvalidateLayout();
        }
#endif

        private float MeasurePreferredHeight()
        {
            float textHeight = rowText != null ? rowText.preferredHeight : 0f;
            return Mathf.Max(minimumHeight, textHeight + verticalPadding);
        }
    }
}
