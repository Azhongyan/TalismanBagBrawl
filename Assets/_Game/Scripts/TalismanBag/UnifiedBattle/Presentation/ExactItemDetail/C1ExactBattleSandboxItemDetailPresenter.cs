using System;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.UnifiedBattle.Presentation.ExactItemDetail
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxItemDetailPresenter :
        MonoBehaviour
    {
        public const string MissingReferenceDiagnostic =
            "UNIFIED_EXACT_ITEM_DETAIL_AUTHORED_REFERENCE_MISSING";
        public const string BoundDiagnostic =
            "UNIFIED_EXACT_ITEM_DETAIL_BOUND";
        public const string LayoutInvalidDiagnostic =
            "UNIFIED_EXACT_ITEM_DETAIL_PRESENTATION_LAYOUT_INVALID";
        public const string RuntimeBindingIdentityMarker =
            "UNIFIED_EXACT_ITEM_DETAIL_RUNTIME_BINDING_IDENTITY";
        public const float PresentationReferenceWidth = 1080f;
        public const float PresentationReferenceHeight = 1920f;

        [SerializeField] private ItemDetailPanelView panelView;
        [SerializeField] private Button closeButton;
        [SerializeField] private CanvasGroup popupCanvasGroup;
        [SerializeField] private RectTransform presentationFrame;
        [SerializeField] private RectTransform carrierRoot;
        [SerializeField] private Image modalBackdropImage;
        [SerializeField] private Button modalBackdropButton;

        private C1ExactBattleSandboxItemArrangementPresenter itemPresenter;
        private string renderedResultSignature = string.Empty;
        private bool listenersBound;

        public ItemDetailPanelView PanelView => panelView;
        public Button CloseButton => closeButton;
        public CanvasGroup PopupCanvasGroup => popupCanvasGroup;
        public RectTransform PresentationFrame => presentationFrame;
        public RectTransform CarrierRoot => carrierRoot;
        public Image ModalBackdropImage => modalBackdropImage;
        public Button ModalBackdropButton => modalBackdropButton;
        public bool IsBound => listenersBound && itemPresenter != null;

        public bool ValidateAuthoredReferences()
        {
            return panelView != null
                   && closeButton != null
                   && popupCanvasGroup != null
                   && presentationFrame != null
                   && carrierRoot != null
                   && modalBackdropImage != null
                   && modalBackdropButton != null
                   && popupCanvasGroup.transform == transform
                   && modalBackdropImage.transform.parent == transform
                   && modalBackdropButton.transform
                   == modalBackdropImage.transform
                   && modalBackdropButton.targetGraphic
                   == modalBackdropImage
                   && modalBackdropImage.rectTransform.anchorMin
                   == Vector2.zero
                   && modalBackdropImage.rectTransform.anchorMax
                   == Vector2.one
                   && modalBackdropImage.rectTransform.anchoredPosition
                   == Vector2.zero
                   && modalBackdropImage.rectTransform.sizeDelta
                   == Vector2.zero
                   && modalBackdropImage.raycastTarget
                   && modalBackdropImage.transform.GetSiblingIndex()
                   < presentationFrame.GetSiblingIndex()
                   && presentationFrame.parent == transform
                   && presentationFrame.childCount == 1
                   && presentationFrame.GetChild(0) == carrierRoot
                   && carrierRoot.parent == presentationFrame
                   && HasValidEmbeddedCarrierMount()
                   && panelView.transform != carrierRoot
                   && panelView.transform.IsChildOf(carrierRoot)
                   && carrierRoot.GetComponentsInChildren<
                           ItemDetailPanelView>(true).Length == 1
                   && closeButton.transform.IsChildOf(panelView.transform);
        }

        public static bool TryCalculateUniformReferenceFit(
            Vector2 viewportSize,
            out float uniformScale)
        {
            uniformScale = 0f;
            if (!IsFinitePositive(viewportSize.x)
                || !IsFinitePositive(viewportSize.y))
            {
                return false;
            }

            float widthScale = viewportSize.x / PresentationReferenceWidth;
            float heightScale = viewportSize.y / PresentationReferenceHeight;
            float resolved = Mathf.Min(widthScale, heightScale);
            if (!IsFinitePositive(resolved))
            {
                return false;
            }

            uniformScale = resolved;
            return true;
        }

        public bool RefreshPresentationLayout()
        {
            RectTransform viewport = transform as RectTransform;
            if (viewport == null
                || presentationFrame == null
                || !HasValidEmbeddedCarrierMount()
                || !TryCalculateUniformReferenceFit(
                    viewport.rect.size,
                    out float scale))
            {
                renderedResultSignature = string.Empty;
                ApplyPopupInputState(false);
                panelView?.Close();
                return false;
            }

            presentationFrame.localScale = new Vector3(scale, scale, scale);
            return true;
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            ItemDetailPanelView configuredPanelView,
            Button configuredCloseButton,
            CanvasGroup configuredPopupCanvasGroup)
        {
            AssignForEditor(
                configuredPanelView,
                configuredCloseButton,
                configuredPopupCanvasGroup,
                null,
                null,
                null,
                null);
        }

        public void AssignForEditor(
            ItemDetailPanelView configuredPanelView,
            Button configuredCloseButton,
            CanvasGroup configuredPopupCanvasGroup,
            RectTransform configuredPresentationFrame)
        {
            AssignForEditor(
                configuredPanelView,
                configuredCloseButton,
                configuredPopupCanvasGroup,
                configuredPresentationFrame,
                configuredPanelView != null
                    ? configuredPanelView.transform as RectTransform
                    : null,
                null,
                null);
        }

        public void AssignForEditor(
            ItemDetailPanelView configuredPanelView,
            Button configuredCloseButton,
            CanvasGroup configuredPopupCanvasGroup,
            RectTransform configuredPresentationFrame,
            RectTransform configuredCarrierRoot)
        {
            AssignForEditor(
                configuredPanelView,
                configuredCloseButton,
                configuredPopupCanvasGroup,
                configuredPresentationFrame,
                configuredCarrierRoot,
                null,
                null);
        }

        public void AssignForEditor(
            ItemDetailPanelView configuredPanelView,
            Button configuredCloseButton,
            CanvasGroup configuredPopupCanvasGroup,
            RectTransform configuredPresentationFrame,
            RectTransform configuredCarrierRoot,
            Image configuredModalBackdropImage,
            Button configuredModalBackdropButton)
        {
            panelView = configuredPanelView;
            closeButton = configuredCloseButton;
            popupCanvasGroup = configuredPopupCanvasGroup;
            presentationFrame = configuredPresentationFrame;
            carrierRoot = configuredCarrierRoot;
            modalBackdropImage = configuredModalBackdropImage;
            modalBackdropButton = configuredModalBackdropButton;
        }
#endif

        public bool Bind(
            C1ExactBattleSandboxItemArrangementPresenter configuredPresenter,
            out string diagnostic)
        {
            if (configuredPresenter == null || !ValidateAuthoredReferences())
            {
                diagnostic = MissingReferenceDiagnostic;
                ResetPresentation();
                return false;
            }

            if (!RefreshPresentationLayout())
            {
                diagnostic = LayoutInvalidDiagnostic;
                ResetPresentation();
                return false;
            }

            UnbindInternal(false);
            itemPresenter = configuredPresenter;
            itemPresenter.FormalItemDetailChanged += HandleItemDetailChanged;
            panelView.SetExternalCloseRequest(RequestClose);
            modalBackdropButton.onClick.RemoveListener(HandleCloseRequested);
            modalBackdropButton.onClick.AddListener(HandleCloseRequested);
            listenersBound = true;
            Render(itemPresenter.CurrentFormalItemDetail, true);
            LogRuntimeBindingIdentity();
            diagnostic = BoundDiagnostic;
            return true;
        }

        public void Unbind()
        {
            UnbindInternal(true);
        }

        public void ResetPresentation()
        {
            renderedResultSignature = string.Empty;
            ApplyPopupInputState(false);
            if (panelView == null)
            {
                return;
            }

            panelView.Bind(null);
            panelView.Close();
        }

        public void RequestClose()
        {
            C1FormalItemSessionSnapshot snapshot = itemPresenter?.Current;
            if (snapshot == null)
            {
                ResetPresentation();
                return;
            }

            C1FormalItemDetailSelectionResult result =
                itemPresenter.CloseFormalItemDetail(
                    C1FormalItemDetailSelectionRequest.Close(snapshot));
            if (result == null || !result.accepted || !result.isOpen)
            {
                ResetPresentation();
            }
        }

        private void HandleCloseRequested()
        {
            RequestClose();
        }

        private void HandleItemDetailChanged(
            C1FormalItemDetailSelectionResult result)
        {
            Render(result, false);
        }

        private void Render(
            C1FormalItemDetailSelectionResult result,
            bool force)
        {
            if (panelView == null)
            {
                return;
            }

            string signature = result?.canonicalSignature ?? string.Empty;
            bool shouldOpen = result != null
                              && result.accepted
                              && result.isOpen
                              && result.projection != null;
            if (!force
                && string.Equals(
                    renderedResultSignature,
                    signature,
                    StringComparison.Ordinal)
                && panelView.gameObject.activeSelf == shouldOpen)
            {
                return;
            }

            if (!shouldOpen)
            {
                renderedResultSignature = signature;
                panelView.Bind(null);
                panelView.Close();
                ApplyPopupInputState(false);
                return;
            }

            ApplyPopupInputState(false);
            if (!RefreshPresentationLayout())
            {
                renderedResultSignature = string.Empty;
                return;
            }

            ItemDetailViewModel model =
                result.projection.CreateViewModelClone();
            if (!ValidateFormalProjection(result.projection, model))
            {
                renderedResultSignature = string.Empty;
                ResetPresentation();
                return;
            }

            panelView.SetExternalCloseRequest(RequestClose);
            panelView.SetPreserveAuthoredVisualStyle(true);
            panelView.Bind(model, result.projection.artwork);
            if (panelView.CachedLegacyTextFont == null)
            {
                renderedResultSignature = string.Empty;
                panelView.Close();
                ApplyPopupInputState(false);
                return;
            }

            panelView.SetVisible(true);
            bool singleCellArtwork = IsSingleCellArtwork(
                model.displayShapeName);
            if (!panelView.ValidateVisibleFormalBinding(
                    result.projection.itemInstanceId,
                    result.projection.baseItemId,
                    result.projection.artwork,
                    singleCellArtwork))
            {
                renderedResultSignature = string.Empty;
                ResetPresentation();
                return;
            }
            ApplyPopupInputState(true);
            renderedResultSignature = signature;
        }

        private void ApplyPopupInputState(bool visible)
        {
            if (popupCanvasGroup == null)
            {
                return;
            }

            popupCanvasGroup.alpha = visible ? 1f : 0f;
            popupCanvasGroup.interactable = visible;
            popupCanvasGroup.blocksRaycasts = visible;
            if (modalBackdropImage != null)
            {
                modalBackdropImage.enabled = visible;
            }

            if (modalBackdropButton != null)
            {
                modalBackdropButton.interactable = visible;
            }
        }

        private void UnbindInternal(bool resetPresentation)
        {
            if (itemPresenter != null)
            {
                itemPresenter.FormalItemDetailChanged -=
                    HandleItemDetailChanged;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseRequested);
            }

            if (modalBackdropButton != null)
            {
                modalBackdropButton.onClick.RemoveListener(
                    HandleCloseRequested);
            }

            panelView?.SetExternalCloseRequest(null);

            itemPresenter = null;
            listenersBound = false;
            renderedResultSignature = string.Empty;
            if (resetPresentation)
            {
                ResetPresentation();
            }
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (presentationFrame != null)
            {
                RefreshPresentationLayout();
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f
                   && !float.IsNaN(value)
                   && !float.IsInfinity(value);
        }

        private static bool ValidateFormalProjection(
            C1FormalItemDetailProjection projection,
            ItemDetailViewModel model)
        {
            if (projection == null
                || model == null
                || string.IsNullOrWhiteSpace(projection.itemInstanceId)
                || string.IsNullOrWhiteSpace(projection.baseItemId)
                || !string.Equals(
                    model.itemInstanceId,
                    projection.itemInstanceId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    model.baseItemId,
                    projection.baseItemId,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(model.displayItemName)
                || projection.artwork == null)
            {
                return false;
            }

            C1FormalItemArtworkLightingState artworkState =
                projection.placementFacts == null
                || !projection.placementFacts.isPresent
                    ? C1FormalItemArtworkLightingState.PlacementAbsent
                    : projection.lightingFacts != null
                      && projection.lightingFacts.isLit == true
                        ? C1FormalItemArtworkLightingState.Lit
                        : C1FormalItemArtworkLightingState.Unlit;
            if (!string.Equals(
                    projection.artworkIdentity,
                    C1FormalItemArtworkIdentity.Create(
                        projection.baseItemId,
                        artworkState),
                    StringComparison.Ordinal))
            {
                return false;
            }

            C1FormalItemCombatDetailFacts combat = projection.combatFacts;
            if (combat == null || !combat.isApplicable)
            {
                return model.displayPlayerSections != null
                       && model.displayPlayerSections.Any(section =>
                           section != null
                           && !string.IsNullOrWhiteSpace(section.body));
            }

            return combat.cooldownSeconds.HasValue
                   && combat.cooldownSeconds.Value > 0m
                   && model.displayPlayerSections != null
                   && new[] { "stats", "trigger", "basic" }.All(
                       stateKey => model.displayPlayerSections.Any(
                           section => section != null
                                      && string.Equals(
                                          section.stateKey,
                                          stateKey,
                                          StringComparison.Ordinal)
                                      && !string.IsNullOrWhiteSpace(
                                          section.body)));
        }

        private static bool IsSingleCellArtwork(string shapeName)
        {
            return !string.IsNullOrWhiteSpace(shapeName)
                   && (shapeName.Contains(
                           "单格",
                           StringComparison.Ordinal)
                       || shapeName.Contains(
                           "single_1",
                           StringComparison.OrdinalIgnoreCase));
        }

        private bool HasValidEmbeddedCarrierMount()
        {
            if (presentationFrame == null
                || carrierRoot == null
                || carrierRoot.parent != presentationFrame
                || presentationFrame.childCount != 1
                || presentationFrame.GetChild(0) != carrierRoot
                || !Approximately(carrierRoot.anchorMin, Vector2.zero)
                || !Approximately(carrierRoot.anchorMax, Vector2.one)
                || !Approximately(carrierRoot.anchoredPosition, Vector2.zero)
                || !Approximately(carrierRoot.sizeDelta, Vector2.zero)
                || !Approximately(
                    carrierRoot.pivot,
                    new Vector2(0.5f, 0.5f))
                || !Approximately(carrierRoot.localPosition, Vector3.zero)
                || !Approximately(carrierRoot.localScale, Vector3.one)
                || Quaternion.Angle(
                    carrierRoot.localRotation,
                    Quaternion.identity) > 0.001f
                || !Approximately(carrierRoot.localEulerAngles, Vector3.zero))
            {
                return false;
            }

            Component[] rootComponents = carrierRoot.GetComponents<Component>();
            return rootComponents.Length == 4
                   && rootComponents[0] is RectTransform
                   && rootComponents[1] is Canvas canvas
                   && canvas.renderMode == RenderMode.ScreenSpaceOverlay
                   && rootComponents[2] is CanvasScaler
                   && rootComponents[3] is GraphicRaycaster;
        }

        private void LogRuntimeBindingIdentity()
        {
            Canvas rootCanvas = carrierRoot == null
                ? null
                : carrierRoot.GetComponent<Canvas>();
            Rect rect = carrierRoot == null ? default : carrierRoot.rect;
            Vector3 scale = carrierRoot == null
                ? Vector3.zero
                : carrierRoot.localScale;
            Debug.Log(
                "[UnifiedExactItemDetail] " + RuntimeBindingIdentityMarker
                + " scenePath=" + gameObject.scene.path
                + " sceneName=" + gameObject.scene.name
                + " presenter=" + HierarchyPath(transform)
                + " carrier="
                + (carrierRoot == null ? "<null>" : carrierRoot.name)
                + " panel="
                + (panelView == null
                    ? "<null>"
                    : HierarchyPath(panelView.transform))
                + " close="
                + (closeButton == null
                    ? "<null>"
                    : HierarchyPath(closeButton.transform))
                + " rootCanvas=" + (rootCanvas == null ? "0" : "1")
                + " renderMode="
                + (rootCanvas == null
                    ? "<none>"
                    : rootCanvas.renderMode.ToString())
                + " rect=" + Format(rect.width) + "x" + Format(rect.height)
                + " scale=" + Format(scale.x) + "," + Format(scale.y)
                + "," + Format(scale.z),
                this);
        }

        private static string HierarchyPath(Transform value)
        {
            if (value == null)
            {
                return "<null>";
            }

            string path = value.name;
            for (Transform parent = value.parent;
                 parent != null;
                 parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return path;
        }

        private static string Format(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static bool Approximately(Vector2 left, Vector2 right)
        {
            return IsFinite(left.x)
                   && IsFinite(left.y)
                   && Mathf.Abs(left.x - right.x) <= 0.0001f
                   && Mathf.Abs(left.y - right.y) <= 0.0001f;
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return IsFinite(left.x)
                   && IsFinite(left.y)
                   && IsFinite(left.z)
                   && Mathf.Abs(left.x - right.x) <= 0.0001f
                   && Mathf.Abs(left.y - right.y) <= 0.0001f
                   && Mathf.Abs(left.z - right.z) <= 0.0001f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxItemDetailCloseRelay :
        MonoBehaviour,
        IPointerDownHandler
    {
        [SerializeField] private C1ExactBattleSandboxItemDetailPresenter
            presenter;
        [SerializeField] private ItemDetailPanelView panelView;

        public bool ValidateAuthoredReferences()
        {
            return presenter != null
                   && panelView != null
                   && panelView.transform == transform;
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            C1ExactBattleSandboxItemDetailPresenter configuredPresenter,
            ItemDetailPanelView configuredPanelView)
        {
            presenter = configuredPresenter;
            panelView = configuredPanelView;
        }
#endif

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null
                || eventData.button != PointerEventData.InputButton.Left
                || presenter == null
                || panelView == null
                || !panelView.gameObject.activeInHierarchy
                || panelView.ContainsPopupContentScreenPoint(
                    eventData.position,
                    eventData.pressEventCamera))
            {
                return;
            }

            presenter.RequestClose();
            eventData.Use();
        }
    }
}
