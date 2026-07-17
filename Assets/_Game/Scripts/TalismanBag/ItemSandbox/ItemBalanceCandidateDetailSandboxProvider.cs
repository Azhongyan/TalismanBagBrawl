using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    [DisallowMultipleComponent]
    public sealed class ItemBalanceCandidateDetailSandboxProvider : MonoBehaviour, IItemDetailViewModelProvider
    {
        public const long DefaultRootSeed = 40412001L;

        [SerializeField] private ItemInnerDataCatalogProvider catalogProvider;
        [SerializeField] private ItemBalanceWorkbenchCatalog workbenchCatalog;
        [SerializeField] private string selectedRarityKey = "white";
        [SerializeField] private long rootSeed = DefaultRootSeed;
        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool readsFormalBattleObject;
        [SerializeField] private bool readsFormalSaveData;
        [SerializeField] private bool writesFormalSystem;

        private ItemBalanceCandidateDetailSandboxAdapter adapter;
        private ReadOnlyCollection<ItemDetailListEntry> listEntries;
        private ReadOnlyCollection<string> lastValidationErrors = Array.AsReadOnly(Array.Empty<string>());

        public bool DevOnly => devOnly;
        public bool ReadsFormalBattleObject => readsFormalBattleObject;
        public bool ReadsFormalSaveData => readsFormalSaveData;
        public bool WritesFormalSystem => writesFormalSystem;
        public string SelectedRarityKey => selectedRarityKey;
        public long RootSeed => rootSeed;
        public ItemBalanceWorkbenchCatalog WorkbenchCatalog => workbenchCatalog;
        public IReadOnlyList<string> LastValidationErrors => lastValidationErrors;

#if UNITY_EDITOR
        public void ConfigureEditor(
            ItemInnerDataCatalogProvider configuredCatalogProvider,
            ItemBalanceWorkbenchCatalog configuredWorkbenchCatalog)
        {
            catalogProvider = configuredCatalogProvider;
            workbenchCatalog = configuredWorkbenchCatalog;
            ResetProvider();
        }
#endif

        public IReadOnlyList<ItemDetailListEntry> GetItemList()
        {
            EnsureInitialized();
            return listEntries;
        }

        public ItemDetailViewModel GetDetailViewModel(string baseItemId)
        {
            return RequestExact(baseItemId, selectedRarityKey,
                rootSeed.ToString(CultureInfo.InvariantCulture)).viewModel;
        }

        public ItemBalanceCandidateDetailResult RequestExact(
            string baseItemId,
            string rarityKey,
            string rootSeedText)
        {
            EnsureInitialized();
            ItemBalanceCandidateDetailResult result = adapter.Request(
                new ItemBalanceCandidateDetailRequest
                {
                    baseItemId = baseItemId,
                    rarityKey = rarityKey,
                    rootSeedText = rootSeedText
                });
            lastValidationErrors = Array.AsReadOnly(result.ValidationErrors.ToArray());
            return result;
        }

        public bool TrySetRarity(string rarityKey)
        {
            string normalized = string.IsNullOrWhiteSpace(rarityKey)
                ? string.Empty
                : rarityKey.Trim().ToLowerInvariant();
            if (!ItemInstanceRarityCatalog.TryParseStableKey(normalized, out _))
            {
                SetValidation("RARITY_INVALID: Use white/green/blue/purple/orange.");
                return false;
            }

            selectedRarityKey = normalized;
            SetValidation();
            return true;
        }

        public bool TrySetRootSeedText(string rootSeedText)
        {
            if (!long.TryParse(rootSeedText?.Trim(), NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out long parsed))
            {
                SetValidation("ROOT_SEED_INVALID: Seed must be a signed 64-bit integer.");
                return false;
            }

            rootSeed = parsed;
            SetValidation();
            return true;
        }

        public long Regenerate()
        {
            rootSeed = rootSeed == long.MaxValue ? long.MinValue : rootSeed + 1L;
            SetValidation();
            return rootSeed;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (adapter != null && listEntries != null)
            {
                return;
            }

            if (catalogProvider == null)
            {
                catalogProvider = GetComponent<ItemInnerDataCatalogProvider>();
            }

            adapter = new ItemBalanceCandidateDetailSandboxAdapter(workbenchCatalog, catalogProvider);
            listEntries = Array.AsReadOnly(adapter.CandidateBaseItemIds
                .Select(baseItemId =>
                {
                    ItemDetailViewModel catalog = catalogProvider?.GetDetailViewModel(baseItemId);
                    string displayName = catalog != null
                        && string.Equals(catalog.itemId, baseItemId, StringComparison.Ordinal)
                        ? catalog.displayItemName
                        : baseItemId;
                    return new ItemDetailListEntry(baseItemId, baseItemId + " · " + displayName);
                })
                .ToArray());
        }

        private void ResetProvider()
        {
            adapter = null;
            listEntries = null;
            SetValidation();
        }

        private void SetValidation(params string[] errors)
        {
            lastValidationErrors = Array.AsReadOnly((errors ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray());
        }
    }
}
