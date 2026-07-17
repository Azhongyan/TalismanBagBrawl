using System;
using System.Collections.Generic;
using System.Globalization;
using TalismanBag.Items.Detail;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    public enum ItemSandboxDetailMode
    {
        Catalog = 0,
        GeneratedInstances = 1
    }

    public sealed class ItemSandboxDetailUiController : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour providerBehaviour;
        [SerializeField] private MonoBehaviour generatedInstanceProviderBehaviour;
        [SerializeField] private ItemSandboxItemButtonView[] itemButtons;
        [SerializeField] private ItemSandboxDetailPanelView detailPanel;
        [SerializeField] private ItemSandboxGridPlacementPreviewView placementPreview;
        [SerializeField] private Button catalogModeButton;
        [SerializeField] private Button generatedInstancesModeButton;
        [SerializeField] private GameObject candidateControlsRoot;
        [SerializeField] private Button[] candidateRarityButtons;
        [SerializeField] private InputField candidateSeedInput;
        [SerializeField] private Button candidateRegenerateButton;
        [SerializeField] private Text candidateValidationText;

        private IItemDetailViewModelProvider provider;
        private IItemDetailViewModelProvider catalogProvider;
        private IItemDetailViewModelProvider generatedInstanceProvider;
        private ItemBalanceCandidateDetailSandboxProvider candidateProvider;
        private string selectedItemId;
        private ItemSandboxDetailMode activeMode;
        private bool detailVisible;

        public void ConfigureEditor(
            MonoBehaviour configuredProviderBehaviour,
            ItemSandboxItemButtonView[] configuredItemButtons,
            ItemSandboxDetailPanelView configuredDetailPanel,
            ItemSandboxGridPlacementPreviewView configuredPlacementPreview = null)
        {
            providerBehaviour = configuredProviderBehaviour;
            itemButtons = configuredItemButtons;
            detailPanel = configuredDetailPanel;
            placementPreview = configuredPlacementPreview;
        }

        public void ConfigureInstanceModesEditor(
            MonoBehaviour configuredGeneratedInstanceProviderBehaviour,
            Button configuredCatalogModeButton,
            Button configuredGeneratedInstancesModeButton)
        {
            generatedInstanceProviderBehaviour = configuredGeneratedInstanceProviderBehaviour;
            catalogModeButton = configuredCatalogModeButton;
            generatedInstancesModeButton = configuredGeneratedInstancesModeButton;
        }

        public void ConfigureCandidatePreviewEditor(
            ItemBalanceCandidateDetailSandboxProvider configuredCandidateProvider,
            GameObject configuredControlsRoot,
            Button[] configuredRarityButtons,
            InputField configuredSeedInput,
            Button configuredRegenerateButton,
            Text configuredValidationText)
        {
            candidateProvider = configuredCandidateProvider;
            candidateControlsRoot = configuredControlsRoot;
            candidateRarityButtons = configuredRarityButtons;
            candidateSeedInput = configuredSeedInput;
            candidateRegenerateButton = configuredRegenerateButton;
            candidateValidationText = configuredValidationText;
        }

        public bool HasPlacementPreview => placementPreview != null;
        public bool HasGeneratedInstanceMode => generatedInstanceProviderBehaviour != null
            && catalogModeButton != null
            && generatedInstancesModeButton != null;
        public bool HasCandidateInstanceMode => (candidateProvider != null
                || generatedInstanceProviderBehaviour is ItemBalanceCandidateDetailSandboxProvider)
            && candidateControlsRoot != null
            && candidateRarityButtons != null
            && candidateRarityButtons.Length == 5
            && candidateSeedInput != null
            && candidateRegenerateButton != null;
        public ItemSandboxDetailMode ActiveMode => activeMode;

        private void Awake()
        {
            catalogProvider = providerBehaviour as IItemDetailViewModelProvider;
            generatedInstanceProvider = generatedInstanceProviderBehaviour as IItemDetailViewModelProvider;
            candidateProvider ??= generatedInstanceProviderBehaviour
                as ItemBalanceCandidateDetailSandboxProvider;
            provider = catalogProvider;
            detailVisible = false;
            detailPanel?.SetVisible(false);
            BindModeButtons();
            BindCandidateControls();
        }

        private void Start()
        {
            if (placementPreview != null)
            {
                placementPreview.BindSelectionRequest(SelectItem);
            }

            SelectMode(ItemSandboxDetailMode.Catalog);
        }

        public void ShowCatalogMode()
        {
            SelectMode(ItemSandboxDetailMode.Catalog);
        }

        public void ShowGeneratedInstancesMode()
        {
            SelectMode(ItemSandboxDetailMode.GeneratedInstances);
        }

        public void SelectMode(ItemSandboxDetailMode mode)
        {
            activeMode = mode;
            provider = mode == ItemSandboxDetailMode.GeneratedInstances
                ? generatedInstanceProvider
                : catalogProvider;
            selectedItemId = string.Empty;
            detailVisible = false;
            if (placementPreview != null)
            {
                placementPreview.gameObject.SetActive(mode == ItemSandboxDetailMode.Catalog);
            }

            detailPanel?.SetVisible(false);
            BindList();
            ApplyModeVisuals();
            UpdateCandidateControls();
        }

        public void SelectItem(string itemId)
        {
            SelectItem(itemId, showDetail: true);
        }

        private void SelectItem(string itemId, bool showDetail)
        {
            if (provider == null || string.IsNullOrEmpty(itemId))
            {
                return;
            }

            selectedItemId = itemId;
            if (placementPreview != null && activeMode == ItemSandboxDetailMode.Catalog)
            {
                placementPreview.SelectItem(itemId);
            }

            ItemDetailViewModel model = provider.GetDetailViewModel(itemId);
            if (placementPreview != null && activeMode == ItemSandboxDetailMode.Catalog)
            {
                model = placementPreview.ProjectDetailViewModel(model);
            }

            if (detailPanel != null)
            {
                detailPanel.Show(model);
                detailPanel.SetVisible(showDetail);
            }

            detailVisible = showDetail;

            UpdateSelectionState();
        }

        private void BindList()
        {
            if (provider == null || itemButtons == null)
            {
                return;
            }

            IReadOnlyList<ItemDetailListEntry> entries = provider.GetItemList();
            for (int i = 0; i < itemButtons.Length; i++)
            {
                ItemSandboxItemButtonView buttonView = itemButtons[i];
                if (buttonView == null)
                {
                    continue;
                }

                if (i >= entries.Count)
                {
                    buttonView.gameObject.SetActive(false);
                    continue;
                }

                buttonView.Bind(entries[i], SelectItem);
            }

            if (entries.Count > 0)
            {
                SelectItem(entries[0].itemId, showDetail: false);
            }
        }

        private void BindModeButtons()
        {
            if (catalogModeButton != null)
            {
                catalogModeButton.onClick.RemoveListener(ShowCatalogMode);
                catalogModeButton.onClick.AddListener(ShowCatalogMode);
            }

            if (generatedInstancesModeButton != null)
            {
                generatedInstancesModeButton.onClick.RemoveListener(ShowGeneratedInstancesMode);
                generatedInstancesModeButton.onClick.AddListener(ShowGeneratedInstancesMode);
            }
        }

        private void BindCandidateControls()
        {
            if (candidateRarityButtons != null)
            {
                for (int index = 0; index < candidateRarityButtons.Length; index++)
                {
                    Button button = candidateRarityButtons[index];
                    if (button == null)
                    {
                        continue;
                    }

                    int capturedIndex = index;
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => SelectCandidateRarity(capturedIndex));
                }
            }

            if (candidateSeedInput != null)
            {
                candidateSeedInput.onEndEdit.RemoveListener(ApplyCandidateSeed);
                candidateSeedInput.onEndEdit.AddListener(ApplyCandidateSeed);
            }

            if (candidateRegenerateButton != null)
            {
                candidateRegenerateButton.onClick.RemoveListener(RegenerateCandidate);
                candidateRegenerateButton.onClick.AddListener(RegenerateCandidate);
            }
        }

        private void SelectCandidateRarity(int index)
        {
            if (candidateProvider == null || index < 0 || index >= CandidateRarityKeys.Length)
            {
                return;
            }

            candidateProvider.TrySetRarity(CandidateRarityKeys[index]);
            RefreshCandidateSelection();
        }

        private void ApplyCandidateSeed(string seedText)
        {
            if (candidateProvider == null)
            {
                return;
            }

            bool changed = candidateProvider.TrySetRootSeedText(seedText);
            UpdateCandidateControls();
            if (changed)
            {
                RefreshCandidateSelection();
            }
        }

        private void RegenerateCandidate()
        {
            if (candidateProvider == null)
            {
                return;
            }

            candidateProvider.Regenerate();
            RefreshCandidateSelection();
        }

        private void RefreshCandidateSelection()
        {
            UpdateCandidateControls();
            if (activeMode == ItemSandboxDetailMode.GeneratedInstances
                && !string.IsNullOrWhiteSpace(selectedItemId))
            {
                SelectItem(selectedItemId, detailVisible);
            }
        }

        private void UpdateCandidateControls()
        {
            bool candidateMode = activeMode == ItemSandboxDetailMode.GeneratedInstances;
            if (candidateControlsRoot != null)
            {
                candidateControlsRoot.SetActive(candidateMode);
            }

            if (candidateProvider == null)
            {
                return;
            }

            if (candidateSeedInput != null)
            {
                string seed = candidateProvider.RootSeed.ToString(CultureInfo.InvariantCulture);
                if (!string.Equals(candidateSeedInput.text, seed, StringComparison.Ordinal))
                {
                    candidateSeedInput.SetTextWithoutNotify(seed);
                }
            }

            if (candidateValidationText != null)
            {
                candidateValidationText.text = candidateProvider.LastValidationErrors.Count == 0
                    ? "平衡候选预览 · 只读 · 未接战斗/存档"
                    : string.Join("\n", candidateProvider.LastValidationErrors);
            }

            if (candidateRarityButtons != null)
            {
                for (int index = 0; index < candidateRarityButtons.Length; index++)
                {
                    Button button = candidateRarityButtons[index];
                    Image image = button != null ? button.GetComponent<Image>() : null;
                    if (image != null)
                    {
                        bool selected = index < CandidateRarityKeys.Length
                            && string.Equals(candidateProvider.SelectedRarityKey,
                                CandidateRarityKeys[index], StringComparison.Ordinal);
                        image.color = selected
                            ? CandidateRaritySelectedColors[Math.Min(index, CandidateRaritySelectedColors.Length - 1)]
                            : new Color(0.25f, 0.18f, 0.12f, 0.88f);
                    }
                }
            }
        }

        private void ApplyModeVisuals()
        {
            SetModeButtonColor(catalogModeButton, activeMode == ItemSandboxDetailMode.Catalog);
            SetModeButtonColor(generatedInstancesModeButton,
                activeMode == ItemSandboxDetailMode.GeneratedInstances);
        }

        private static void SetModeButtonColor(Button button, bool active)
        {
            Image image = button != null ? button.GetComponent<Image>() : null;
            if (image != null)
            {
                image.color = active
                    ? new Color(0.72f, 0.50f, 0.18f, 0.92f)
                    : new Color(0.25f, 0.18f, 0.12f, 0.88f);
            }
        }

        private static readonly string[] CandidateRarityKeys =
        {
            "white", "green", "blue", "purple", "orange"
        };

        private static readonly Color[] CandidateRaritySelectedColors =
        {
            new(0.70f, 0.66f, 0.58f, 0.96f),
            new(0.38f, 0.61f, 0.31f, 0.96f),
            new(0.30f, 0.55f, 0.78f, 0.96f),
            new(0.57f, 0.36f, 0.74f, 0.96f),
            new(0.78f, 0.47f, 0.17f, 0.96f)
        };

        private void UpdateSelectionState()
        {
            if (itemButtons == null)
            {
                return;
            }

            for (int i = 0; i < itemButtons.Length; i++)
            {
                ItemSandboxItemButtonView buttonView = itemButtons[i];
                if (buttonView != null)
                {
                    buttonView.SetSelected(buttonView.ItemId == selectedItemId);
                }
            }
        }
    }
}
