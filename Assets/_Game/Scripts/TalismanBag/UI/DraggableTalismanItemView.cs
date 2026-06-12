using System;
using System.Collections;
using TalismanBag.Combat;
using TalismanBag.Grid;
using TalismanBag.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class DraggableTalismanItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public static event Action<DraggableTalismanItemView> ItemClicked;

        [SerializeField] private TalismanItemDefinition definition;
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Text label;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private Vector2 dragScreenOffset = new(0f, 110f);
        [SerializeField] private float dragScale = 1.14f;
        [SerializeField] private float inventoryDropScreenRatio = 0.32f;

        private RectTransform rectTransform;
        private TalismanItemRuntime runtimeItem;
        private Transform originalParent;
        private Vector2 originalAnchoredPosition;
        private Vector3 homeScale = Vector3.one;
        private TalismanGridSlotView originalSlot;
        private TalismanGridSlotView currentSlot;
        private Transform homeParent;
        private Vector2 homeAnchoredPosition;
        private bool placedDuringDrag;
        private bool dragBlocked;
        private bool draggingFromGrid;
        private Coroutine flashRoutine;

        public TalismanItemRuntime RuntimeItem => runtimeItem;
        public TalismanItemDefinition Definition => definition;
        public TalismanGridSlotView CurrentSlot => currentSlot;
        public int Level => runtimeItem != null ? runtimeItem.level : 1;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (definition != null)
            {
                Bind(definition, grid, rootCanvas, combatController);
            }

            CaptureHome();
            homeScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        }

        public void Bind(TalismanItemDefinition itemDefinition, TalismanBagGrid bagGrid, Canvas canvas, AutoCombatController controller)
        {
            BindRuntime(new TalismanItemRuntime(itemDefinition), bagGrid, canvas, controller);
        }

        public void BindRuntime(TalismanItemRuntime itemRuntime, TalismanBagGrid bagGrid, Canvas canvas, AutoCombatController controller)
        {
            runtimeItem = itemRuntime;
            definition = runtimeItem != null ? runtimeItem.definition : null;
            grid = bagGrid;
            rootCanvas = canvas;
            combatController = controller;

            if (background != null)
            {
                background.color = definition != null ? definition.uiColor : Color.white;
            }

            if (icon != null)
            {
                icon.sprite = definition != null ? definition.icon : null;
                icon.enabled = icon.sprite != null;
            }

            if (label != null)
            {
                RefreshLabel();
            }
        }

        public void SetRuntimeLevel(int level)
        {
            if (runtimeItem == null)
            {
                runtimeItem = new TalismanItemRuntime(definition);
            }

            runtimeItem.level = Mathf.Max(1, level);
            RefreshLabel();
        }

        public void CaptureHome()
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            homeParent = transform.parent;
            homeAnchoredPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragBlocked = combatController != null && !combatController.CanEditLayout;
            if (dragBlocked)
            {
                return;
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            placedDuringDrag = false;
            originalParent = transform.parent;
            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalSlot = currentSlot;
            draggingFromGrid = originalSlot != null;

            if (currentSlot != null)
            {
                currentSlot.SetItemView(null);
                currentSlot = null;
            }

            if (runtimeItem != null && runtimeItem.isPlaced)
            {
                grid.RemoveItem(runtimeItem);
            }

            transform.SetParent(rootCanvas.transform, true);
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.85f;
            }

            rectTransform.localScale = homeScale * dragScale;
            MoveToPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragBlocked || combatController != null && !combatController.CanEditLayout)
            {
                return;
            }

            MoveToPointer(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragBlocked)
            {
                dragBlocked = false;
                return;
            }

            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            if (!placedDuringDrag)
            {
                if (draggingFromGrid && IsPointerInInventoryZone(eventData))
                {
                    ReturnToHome();
                }
                else
                {
                    ReturnToOriginalPosition();
                }
            }

            rectTransform.localScale = homeScale;
            draggingFromGrid = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ItemClicked?.Invoke(this);
        }

        public bool TryPlaceOnSlot(TalismanGridSlotView slot)
        {
            if (slot == null || grid == null || runtimeItem == null)
            {
                return false;
            }

            if (!slot.CanAcceptItem)
            {
                return false;
            }

            if (slot.CurrentItemView != null && slot.CurrentItemView != this)
            {
                return false;
            }

            if (!grid.PlaceItem(runtimeItem, slot.GridPosition))
            {
                return false;
            }

            AttachToSlot(slot);
            placedDuringDrag = true;
            return true;
        }

        public bool ForcePlaceOnSlot(TalismanGridSlotView slot)
        {
            originalSlot = currentSlot;
            if (currentSlot != null)
            {
                currentSlot.SetItemView(null);
                currentSlot = null;
            }

            if (runtimeItem != null && runtimeItem.isPlaced)
            {
                grid.RemoveItem(runtimeItem);
            }

            bool placed = TryPlaceOnSlot(slot);
            placedDuringDrag = false;
            return placed;
        }

        public void ReturnToHome()
        {
            if (currentSlot != null)
            {
                currentSlot.SetItemView(null);
                currentSlot = null;
            }

            if (runtimeItem != null && runtimeItem.isPlaced)
            {
                grid.RemoveItem(runtimeItem);
            }

            transform.SetParent(homeParent != null ? homeParent : transform.parent, false);
            rectTransform.anchoredPosition = homeAnchoredPosition;
            rectTransform.localScale = homeScale;
        }

        public void Flash()
        {
            if (background == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private void AttachToSlot(TalismanGridSlotView slot)
        {
            currentSlot = slot;
            currentSlot.SetItemView(this);
            transform.SetParent(slot.ItemAnchor, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = homeScale;
        }

        private void ReturnToOriginalPosition()
        {
            if (originalSlot != null && grid.PlaceItem(runtimeItem, originalSlot.GridPosition))
            {
                AttachToSlot(originalSlot);
                return;
            }

            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalAnchoredPosition;
            rectTransform.localScale = homeScale;
        }

        private void MoveToPointer(PointerEventData eventData)
        {
            if (rootCanvas == null || rectTransform == null)
            {
                return;
            }

            RectTransform canvasRect = rootCanvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return;
            }

            Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position + dragScreenOffset, eventCamera, out Vector2 localPosition))
            {
                rectTransform.anchoredPosition = localPosition;
            }
        }

        private bool IsPointerInInventoryZone(PointerEventData eventData)
        {
            return eventData.position.y <= Screen.height * inventoryDropScreenRatio;
        }

        private IEnumerator FlashRoutine()
        {
            Color original = background.color;
            background.color = Color.white;
            yield return new WaitForSeconds(0.16f);
            background.color = original;
        }

        private void RefreshLabel()
        {
            if (label == null)
            {
                return;
            }

            if (definition == null)
            {
                label.text = string.Empty;
                return;
            }

            int level = runtimeItem != null ? runtimeItem.level : 1;
            label.text = level > 1 ? $"{definition.displayName}\nLv{level}" : definition.displayName;
        }
    }
}
