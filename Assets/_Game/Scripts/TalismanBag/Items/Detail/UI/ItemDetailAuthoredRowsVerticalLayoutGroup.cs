using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.Items.Detail.UI
{
    /// <summary>
    /// Drives only child Y/height. Child X, width, scale, and all descendant geometry remain untouched.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ItemDetailAuthoredRowsVerticalLayoutGroup : UIBehaviour, ILayoutElement, ILayoutGroup
    {
        [SerializeField] private RectOffset m_Padding = new();
        [SerializeField] private TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;
        [SerializeField, Min(0f)] private float spacing = 6f;
        [SerializeField] private bool useChildScale = true;

        public float Spacing => spacing;
        public bool UseChildScale => useChildScale;
        public float minWidth => -1f;
        public float preferredWidth => -1f;
        public float flexibleWidth => -1f;
        public float minHeight => MeasureTotalHeight();
        public float preferredHeight => MeasureTotalHeight();
        public float flexibleHeight => -1f;
        public int layoutPriority => 1;

        public void CalculateLayoutInputHorizontal()
        {
        }

        public void CalculateLayoutInputVertical()
        {
        }

        public void SetLayoutHorizontal()
        {
            // Deliberately empty: authored X, width, anchors, and scale are never driven here.
        }

        public void SetLayoutVertical()
        {
            float position = Padding.top;
            for (int index = 0; index < transform.childCount; index++)
            {
                RectTransform child = transform.GetChild(index) as RectTransform;
                if (!IsLayoutChild(child))
                {
                    continue;
                }

                float height = Mathf.Max(0f, LayoutUtility.GetPreferredHeight(child));
                float scale = useChildScale ? Mathf.Abs(child.localScale.y) : 1f;
                SetChildVerticalOnly(child, position, height, scale);
                position += height * scale + spacing;
            }
        }

#if UNITY_EDITOR
        public void ConfigureEditor(float configuredSpacing, bool configuredUseChildScale)
        {
            m_Padding = new RectOffset();
            spacing = Mathf.Max(0f, configuredSpacing);
            useChildScale = configuredUseChildScale;
            SetDirty();
        }

        protected override void OnValidate()
        {
            spacing = Mathf.Max(0f, spacing);
            SetDirty();
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            SetDirty();
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetDirty();
        }

        private void OnTransformChildrenChanged()
        {
            SetDirty();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            SetDirty();
        }

        private RectOffset Padding => m_Padding ??= new RectOffset();

        private float MeasureTotalHeight()
        {
            float totalHeight = Padding.vertical;
            int activeCount = 0;
            for (int index = 0; index < transform.childCount; index++)
            {
                RectTransform child = transform.GetChild(index) as RectTransform;
                if (!IsLayoutChild(child))
                {
                    continue;
                }

                float scale = useChildScale ? Mathf.Abs(child.localScale.y) : 1f;
                totalHeight += Mathf.Max(0f, LayoutUtility.GetPreferredHeight(child)) * scale;
                activeCount++;
            }

            if (activeCount > 1)
            {
                totalHeight += spacing * (activeCount - 1);
            }

            return totalHeight;
        }

        private static bool IsLayoutChild(RectTransform child)
        {
            if (child == null || !child.gameObject.activeSelf)
            {
                return false;
            }

            Component[] components = child.GetComponents<Component>();
            for (int index = 0; index < components.Length; index++)
            {
                if (components[index] is ILayoutIgnorer ignorer && ignorer.ignoreLayout)
                {
                    return false;
                }
            }

            return true;
        }

        private static void SetChildVerticalOnly(
            RectTransform child,
            float position,
            float height,
            float scale)
        {
            Vector2 anchorMin = child.anchorMin;
            Vector2 anchorMax = child.anchorMax;
            anchorMin.y = 1f;
            anchorMax.y = 1f;
            child.anchorMin = anchorMin;
            child.anchorMax = anchorMax;

            Vector2 sizeDelta = child.sizeDelta;
            sizeDelta.y = height;
            child.sizeDelta = sizeDelta;

            Vector2 anchoredPosition = child.anchoredPosition;
            anchoredPosition.y = -position - height * (1f - child.pivot.y) * scale;
            child.anchoredPosition = anchoredPosition;
        }

        private void SetDirty()
        {
            if (!IsActive())
            {
                return;
            }

            if (transform is RectTransform rectTransform)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }
    }
}
