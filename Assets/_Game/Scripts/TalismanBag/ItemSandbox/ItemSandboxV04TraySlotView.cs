using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    [DisallowMultipleComponent]
    public sealed class ItemSandboxV04TraySlotView : MonoBehaviour,
        IInitializePotentialDragHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private enum GestureMode
        {
            None = 0,
            Pending = 1,
            Scrolling = 2,
            ItemDragging = 3
        }

        [SerializeField] private int slotIndex;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;
        [SerializeField] private float itemDragHoldSeconds = 0.18f;
        [SerializeField] private float itemDragMoveThresholdPixels = 18f;
        [SerializeField] private float scrollAxisBias = 1.15f;
        [SerializeField] private float viewportExitMarginPixels = 8f;

        private ItemSandboxV04BoardFullDetailAdapter adapter;
        private string itemId = string.Empty;
        private float pointerDownTime;
        private bool suppressClick;
        private bool forwardedScrollBegin;
        private GestureMode gestureMode;

        public int SlotIndex => slotIndex;
        public string ItemId => itemId;
        public RectTransform RectTransform => transform as RectTransform;
        public Image BackgroundImage => backgroundImage;
        public Text LabelText => labelText;

#if UNITY_EDITOR
        public void ConfigureEditor(int configuredSlotIndex, ScrollRect configuredScrollRect,
            Image configuredBackground, Text configuredLabel)
        {
            slotIndex = configuredSlotIndex;
            scrollRect = configuredScrollRect;
            backgroundImage = configuredBackground;
            labelText = configuredLabel;
        }
#endif

        public void Bind(ItemSandboxV04BoardFullDetailAdapter owner, string configuredItemId)
        {
            adapter = owner;
            itemId = Normalize(configuredItemId);
        }

        public void Show(string displayText, Color background, Color foreground, bool interactable)
        {
            gameObject.SetActive(true);
            if (backgroundImage != null)
            {
                backgroundImage.color = background;
                backgroundImage.raycastTarget = interactable;
            }

            if (labelText != null)
            {
                labelText.text = displayText ?? string.Empty;
                labelText.color = foreground;
                labelText.raycastTarget = false;
            }
        }

        public void Hide()
        {
            itemId = string.Empty;
            adapter = null;
            gameObject.SetActive(false);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            ResetGesture();
            if (scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDownTime = Time.unscaledTime;
            suppressClick = false;
            gestureMode = GestureMode.None;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (gestureMode == GestureMode.None || gestureMode == GestureMode.Pending)
            {
                ResetGesture(clearClickSuppression: false);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            suppressClick = true;
            gestureMode = GestureMode.Pending;
            ResolvePendingRoute(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (gestureMode == GestureMode.None)
            {
                gestureMode = GestureMode.Pending;
            }

            if (gestureMode == GestureMode.Pending)
            {
                ResolvePendingRoute(eventData);
            }

            if (gestureMode == GestureMode.Scrolling)
            {
                if (IsOutsideViewport(eventData, viewportExitMarginPixels))
                {
                    EndScroll(eventData);
                    if (scrollRect != null)
                    {
                        scrollRect.velocity = Vector2.zero;
                    }
                    BeginItemDrag(eventData);
                }
                else
                {
                    ForwardScroll(eventData);
                    return;
                }
            }

            if (gestureMode == GestureMode.ItemDragging)
            {
                adapter?.UpdateTrayDrag(this, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (gestureMode == GestureMode.Scrolling)
            {
                EndScroll(eventData);
            }
            else if (gestureMode == GestureMode.ItemDragging)
            {
                adapter?.EndTrayDrag(this, eventData);
            }

            ResetGesture(clearClickSuppression: false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (suppressClick)
            {
                suppressClick = false;
                return;
            }

            if (!string.IsNullOrWhiteSpace(itemId))
            {
                adapter?.SelectTrayItem(itemId);
            }
        }

        private void ResolvePendingRoute(PointerEventData eventData)
        {
            if (ShouldStartItemDrag(eventData) || !ShouldStartScroll(eventData))
            {
                BeginItemDrag(eventData);
            }
            else
            {
                BeginScroll(eventData);
            }
        }

        private void BeginItemDrag(PointerEventData eventData)
        {
            gestureMode = GestureMode.ItemDragging;
            adapter?.BeginTrayDrag(this, eventData);
        }

        private void BeginScroll(PointerEventData eventData)
        {
            if (scrollRect == null)
            {
                BeginItemDrag(eventData);
                return;
            }

            gestureMode = GestureMode.Scrolling;
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
            forwardedScrollBegin = true;
        }

        private void ForwardScroll(PointerEventData eventData)
        {
            if (scrollRect == null)
            {
                return;
            }

            if (!forwardedScrollBegin)
            {
                BeginScroll(eventData);
            }
            ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        }

        private void EndScroll(PointerEventData eventData)
        {
            if (forwardedScrollBegin && scrollRect != null)
            {
                ExecuteEvents.Execute(scrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
            }
            forwardedScrollBegin = false;
        }

        private bool ShouldStartItemDrag(PointerEventData eventData)
        {
            if (scrollRect == null
                || Time.unscaledTime - pointerDownTime >= Mathf.Max(0f, itemDragHoldSeconds)
                || IsOutsideViewport(eventData, viewportExitMarginPixels))
            {
                return true;
            }

            Vector2 delta = ResolveDelta(eventData);
            float horizontal = Mathf.Abs(delta.x);
            return horizontal >= Mathf.Max(1f, itemDragMoveThresholdPixels)
                && horizontal > Mathf.Abs(delta.y);
        }

        private bool ShouldStartScroll(PointerEventData eventData)
        {
            if (scrollRect == null || IsOutsideViewport(eventData, 0f))
            {
                return false;
            }

            Vector2 delta = ResolveDelta(eventData);
            return Mathf.Abs(delta.y) >= Mathf.Abs(delta.x) * Mathf.Max(1f, scrollAxisBias);
        }

        private bool IsOutsideViewport(PointerEventData eventData, float marginPixels)
        {
            RectTransform viewport = scrollRect == null
                ? null
                : scrollRect.viewport != null ? scrollRect.viewport : scrollRect.transform as RectTransform;
            if (viewport == null || eventData == null)
            {
                return false;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                return true;
            }

            Rect rect = viewport.rect;
            rect.xMin -= marginPixels;
            rect.xMax += marginPixels;
            rect.yMin -= marginPixels;
            rect.yMax += marginPixels;
            return !rect.Contains(local);
        }

        private static Vector2 ResolveDelta(PointerEventData eventData)
        {
            return eventData == null ? Vector2.zero : eventData.position - eventData.pressPosition;
        }

        private void ResetGesture(bool clearClickSuppression = true)
        {
            gestureMode = GestureMode.None;
            forwardedScrollBegin = false;
            if (clearClickSuppression)
            {
                suppressClick = false;
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
