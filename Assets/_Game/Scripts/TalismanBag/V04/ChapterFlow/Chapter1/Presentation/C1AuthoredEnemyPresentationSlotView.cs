using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class C1AuthoredEnemyPresentationSlotView :
        MonoBehaviour,
        IPointerClickHandler,
        IC1AuthoredEnemyPresentationSlot
    {
        [SerializeField] private string displaySlotId = string.Empty;
        [SerializeField] private Image visualRenderer;
        [SerializeField] private Behaviour selectionVisual;
        [SerializeField] private C1AuthoredEnemyCalibrationMeshEffect
            calibrationEffect;

        private bool baselineCaptured;
        private bool baselineActiveSelf;
        private bool baselineImageEnabled;
        private bool baselineRaycastTarget;
        private bool baselineSelectionEnabled;
        private Sprite baselineSprite;

        public event Action<C1AuthoredEnemyPresentationSlotView>
            TargetSelectionRequested;

        public string DisplaySlotId => displaySlotId ?? string.Empty;
        public string BoundEnemyInstanceId { get; private set; } =
            string.Empty;
        public bool IsRuntimeVisible { get; private set; }
        public Image VisualRenderer => visualRenderer;
        public Behaviour SelectionVisual => selectionVisual;
        public RectTransform RuntimeDamageAnchor =>
            visualRenderer == null
                ? transform as RectTransform
                : visualRenderer.rectTransform;

        public bool TryCaptureAuthoredBaseline(out string diagnostic)
        {
            diagnostic = string.Empty;
            if (baselineCaptured)
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(displaySlotId))
            {
                diagnostic = "DISPLAY_SLOT_ID_EMPTY";
                return false;
            }
            visualRenderer = visualRenderer != null
                ? visualRenderer
                : GetComponent<Image>();
            calibrationEffect = calibrationEffect != null
                ? calibrationEffect
                : GetComponent<C1AuthoredEnemyCalibrationMeshEffect>();
            if (visualRenderer == null || calibrationEffect == null)
            {
                diagnostic =
                    "AUTHORED_IMAGE_OR_CALIBRATION_EFFECT_MISSING";
                return false;
            }

            baselineActiveSelf = gameObject.activeSelf;
            baselineImageEnabled = visualRenderer.enabled;
            baselineRaycastTarget = visualRenderer.raycastTarget;
            baselineSprite = visualRenderer.sprite;
            baselineSelectionEnabled =
                selectionVisual != null && selectionVisual.enabled;
            baselineCaptured = true;
            return true;
        }

        public bool TryApply(
            C1EnemyPresentationActorFrame frame,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (frame == null)
            {
                diagnostic = "ACTOR_FRAME_NULL";
                return false;
            }
            if (!TryCaptureAuthoredBaseline(out diagnostic))
            {
                return false;
            }
            if (!string.Equals(
                    frame.DisplaySlotId,
                    DisplaySlotId,
                    StringComparison.Ordinal))
            {
                diagnostic = "DISPLAY_SLOT_ID_MISMATCH";
                return false;
            }

            BoundEnemyInstanceId = frame.EnemyInstanceId;
            IsRuntimeVisible = frame.Visible && frame.Sprite != null;
            if (gameObject.activeSelf != IsRuntimeVisible)
            {
                gameObject.SetActive(IsRuntimeVisible);
            }
            visualRenderer.sprite =
                IsRuntimeVisible ? frame.Sprite : null;
            visualRenderer.enabled = IsRuntimeVisible;
            visualRenderer.raycastTarget =
                IsRuntimeVisible && frame.Targetable;
            if (selectionVisual != null)
            {
                selectionVisual.enabled =
                    IsRuntimeVisible && frame.Selected;
            }
            calibrationEffect.Apply(frame.Calibration);
            diagnostic = IsRuntimeVisible
                ? "BOUND_" + frame.BindingKey
                : "HIDDEN_" + frame.BindingKey;
            return true;
        }

        public void ClearRuntimePresentation()
        {
            if (!TryCaptureAuthoredBaseline(out _))
            {
                return;
            }
            BoundEnemyInstanceId = string.Empty;
            IsRuntimeVisible = false;
            visualRenderer.sprite = null;
            visualRenderer.enabled = false;
            visualRenderer.raycastTarget = false;
            if (selectionVisual != null)
            {
                selectionVisual.enabled = false;
            }
            calibrationEffect.RestoreIdentity();
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public void RestoreAuthoredBaseline()
        {
            if (!baselineCaptured || visualRenderer == null)
            {
                return;
            }
            BoundEnemyInstanceId = string.Empty;
            IsRuntimeVisible = false;
            visualRenderer.sprite = baselineSprite;
            bool safeEnabled =
                baselineSprite != null && baselineImageEnabled;
            visualRenderer.enabled = safeEnabled;
            visualRenderer.raycastTarget =
                safeEnabled && baselineRaycastTarget;
            if (selectionVisual != null)
            {
                selectionVisual.enabled =
                    safeEnabled && baselineSelectionEnabled;
            }
            calibrationEffect?.RestoreIdentity();
            bool safeActive =
                baselineSprite != null && baselineActiveSelf;
            if (gameObject.activeSelf != safeActive)
            {
                gameObject.SetActive(safeActive);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsRuntimeVisible
                && !string.IsNullOrWhiteSpace(BoundEnemyInstanceId))
            {
                TargetSelectionRequested?.Invoke(this);
            }
        }
    }
}
