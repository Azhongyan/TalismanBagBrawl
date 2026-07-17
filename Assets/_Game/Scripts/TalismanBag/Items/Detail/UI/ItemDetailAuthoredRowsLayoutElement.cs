using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.Items.Detail.UI
{
    [DisallowMultipleComponent]
    public sealed class ItemDetailAuthoredRowsLayoutElement : UIBehaviour, ILayoutElement
    {
        [SerializeField] private RectTransform rowsRoot;
        [SerializeField] private string rowNamePrefix = string.Empty;
        [SerializeField, Min(0f)] private float minimumHeight;
        [SerializeField, Min(0f)] private float topPadding;
        [SerializeField, Min(0f)] private float bottomPadding;
#if UNITY_EDITOR
        [SerializeField, HideInInspector] private bool editorAffixDynamicRowsInitialized;
#endif

        private readonly Vector3[] worldCorners = new Vector3[4];

        public float minWidth => -1f;
        public float preferredWidth => -1f;
        public float flexibleWidth => -1f;
        public float minHeight => MeasurePreferredHeight();
        public float preferredHeight => MeasurePreferredHeight();
        public float flexibleHeight => -1f;
        public int layoutPriority => 100;
        public RectTransform RowsRoot => rowsRoot;
        public string RowNamePrefix => rowNamePrefix;

        public void CalculateLayoutInputHorizontal()
        {
        }

        public void CalculateLayoutInputVertical()
        {
        }

        public void InvalidateLayout()
        {
            if (rowsRoot != null && rowsRoot.gameObject.activeInHierarchy)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rowsRoot);
            }

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform != null && isActiveAndEnabled)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }

#if UNITY_EDITOR
        public bool EditorAffixDynamicRowsInitialized => editorAffixDynamicRowsInitialized;

        public void ConfigureEditor(
            RectTransform configuredRowsRoot,
            string configuredRowNamePrefix,
            float configuredMinimumHeight = 0f,
            float configuredTopPadding = 0f,
            float configuredBottomPadding = 0f)
        {
            rowsRoot = configuredRowsRoot;
            rowNamePrefix = configuredRowNamePrefix ?? string.Empty;
            minimumHeight = Mathf.Max(0f, configuredMinimumHeight);
            topPadding = Mathf.Max(0f, configuredTopPadding);
            bottomPadding = Mathf.Max(0f, configuredBottomPadding);
            InvalidateLayout();
        }

        public void MarkAffixDynamicRowsEditor()
        {
            editorAffixDynamicRowsInitialized = true;
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

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            InvalidateLayout();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            InvalidateLayout();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            minimumHeight = Mathf.Max(0f, minimumHeight);
            topPadding = Mathf.Max(0f, topPadding);
            bottomPadding = Mathf.Max(0f, bottomPadding);
            InvalidateLayout();
        }
#endif

        private float MeasurePreferredHeight()
        {
            const string staticAuthoredCoreRowsRootName = "CoreEffectRowsRoot";
            RectTransform host = transform as RectTransform;
            if (host == null || rowsRoot == null)
            {
                return minimumHeight;
            }

            ItemDetailAuthoredRowsVerticalLayoutGroup dynamicRowsLayout =
                rowsRoot.GetComponent<ItemDetailAuthoredRowsVerticalLayoutGroup>();
            if (dynamicRowsLayout != null
                && dynamicRowsLayout.isActiveAndEnabled
                && !string.Equals(rowsRoot.name, staticAuthoredCoreRowsRootName, StringComparison.Ordinal))
            {
                float rowsPreferredHeight = LayoutUtility.GetPreferredHeight(rowsRoot);
                return Mathf.Max(
                    minimumHeight,
                    topPadding + Mathf.Max(0f, rowsPreferredHeight) + bottomPadding);
            }

            float hostTop = host.rect.yMax;
            float lowestPoint = hostTop;
            float highestPoint = hostTop;
            bool foundActiveRow = false;
            for (int index = 0; index < rowsRoot.childCount; index++)
            {
                RectTransform row = rowsRoot.GetChild(index) as RectTransform;
                if (row == null
                    || !row.gameObject.activeSelf
                    || (!string.IsNullOrEmpty(rowNamePrefix)
                        && !row.name.StartsWith(rowNamePrefix, StringComparison.Ordinal)))
                {
                    continue;
                }

                row.GetWorldCorners(worldCorners);
                for (int cornerIndex = 0; cornerIndex < worldCorners.Length; cornerIndex++)
                {
                    float localY = host.InverseTransformPoint(worldCorners[cornerIndex]).y;
                    lowestPoint = Mathf.Min(lowestPoint, localY);
                    highestPoint = Mathf.Max(highestPoint, localY);
                }

                foundActiveRow = true;
            }

            if (!foundActiveRow)
            {
                return minimumHeight;
            }

            float topOverflow = Mathf.Max(0f, highestPoint - hostTop);
            float measuredHeight = topPadding
                + topOverflow
                + Mathf.Max(0f, hostTop - lowestPoint)
                + bottomPadding;
            return Mathf.Max(minimumHeight, measuredHeight);
        }
    }
}
