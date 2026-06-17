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
    public sealed class DraggableTalismanItemView : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public static event Action<DraggableTalismanItemView> ItemClicked;
        public static event Action<DraggableTalismanItemView> ItemDragStarted;

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
        private bool hasDragWorldPointerOffset;
        private Vector3 dragWorldPointerOffset;
        private float suppressClickUntilTime;
        private Coroutine flashRoutine;
        private Outline selectionOutline;

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

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
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
            ItemDragStarted?.Invoke(this);
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
            CaptureDragPointerOffset(eventData);
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

            bool lockedDuringDrag = combatController != null && !combatController.CanEditLayout;
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
            }

            if (!placedDuringDrag)
            {
                if (!lockedDuringDrag && draggingFromGrid && IsPointerInInventoryZone(eventData))
                {
                    ReturnToHome();
                }
                else
                {
                    ReturnToOriginalPosition();
                }
            }

            rectTransform.localScale = homeScale;
            hasDragWorldPointerOffset = false;
            suppressClickUntilTime = Time.unscaledTime + 0.15f;
            draggingFromGrid = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Time.unscaledTime < suppressClickUntilTime)
            {
                return;
            }

            ItemClicked?.Invoke(this);
        }

        public void SetSelectedVisual(bool selected)
        {
            SetSelectedVisual(selected, new Color(1f, 0.82f, 0.28f, 1f), new Vector2(6f, -6f));
        }

        public void SetSelectedVisual(bool selected, Color outlineColor, Vector2 effectDistance)
        {
            if (!selected)
            {
                if (selectionOutline != null)
                {
                    selectionOutline.enabled = false;
                }

                return;
            }

            Outline outline = EnsureSelectionOutline();
            if (outline == null)
            {
                return;
            }

            outline.effectColor = outlineColor;
            outline.effectDistance = effectDistance;
            outline.useGraphicAlpha = false;
            outline.enabled = true;
        }

        private Outline EnsureSelectionOutline()
        {
            if (selectionOutline != null)
            {
                return selectionOutline;
            }

            GameObject outlineTarget = background != null ? background.gameObject : gameObject;
            selectionOutline = outlineTarget.GetComponent<Outline>();
            if (selectionOutline == null)
            {
                selectionOutline = outlineTarget.AddComponent<Outline>();
            }

            return selectionOutline;
        }

        public bool TryPlaceOnSlot(TalismanGridSlotView slot)
        {
            if (combatController != null && !combatController.CanEditLayout)
            {
                return false;
            }

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
            grid.NotifyChanged();
            return true;
        }

        public bool ForcePlaceOnSlot(TalismanGridSlotView slot)
        {
            if (combatController != null && !combatController.CanEditLayout)
            {
                return false;
            }

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


        private void CaptureDragPointerOffset(PointerEventData eventData)
        {
            hasDragWorldPointerOffset = false;
            if (rootCanvas == null || rectTransform == null)
            {
                return;
            }

            RectTransform canvasRect = rootCanvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return;
            }

            if (TryGetPointerWorldPosition(canvasRect, eventData.position, out Vector3 pointerWorldPosition))
            {
                dragWorldPointerOffset = rectTransform.position - pointerWorldPosition;
                hasDragWorldPointerOffset = true;
            }
        }

        private bool TryGetPointerWorldPosition(RectTransform canvasRect, Vector2 screenPosition, out Vector3 worldPosition)
        {
            Camera eventCamera = rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas != null ? rootCanvas.worldCamera : null;
            return RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screenPosition, eventCamera, out worldPosition);
        }

        private Vector3 GetDragScreenOffsetWorldDelta(RectTransform canvasRect, PointerEventData eventData)
        {
            Vector2 effectiveOffset = eventData.pointerId >= 0 ? dragScreenOffset : Vector2.zero;
            if (effectiveOffset == Vector2.zero)
            {
                return Vector3.zero;
            }

            if (!TryGetPointerWorldPosition(canvasRect, eventData.position, out Vector3 baseWorld) ||
                !TryGetPointerWorldPosition(canvasRect, eventData.position + effectiveOffset, out Vector3 offsetWorld))
            {
                return Vector3.zero;
            }

            return offsetWorld - baseWorld;
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

            if (TryGetPointerWorldPosition(canvasRect, eventData.position, out Vector3 pointerWorldPosition))
            {
                Vector3 preservedPointerOffset = hasDragWorldPointerOffset ? dragWorldPointerOffset : Vector3.zero;
                rectTransform.position = pointerWorldPosition + preservedPointerOffset + GetDragScreenOffsetWorldDelta(canvasRect, eventData);
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
