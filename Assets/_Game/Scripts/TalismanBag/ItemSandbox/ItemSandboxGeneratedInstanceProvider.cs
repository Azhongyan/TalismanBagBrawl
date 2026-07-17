using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    [DisallowMultipleComponent]
    public sealed class ItemSandboxGeneratedInstanceProvider : MonoBehaviour, IItemDetailViewModelProvider
    {
        [SerializeField] private ItemInnerDataCatalogProvider catalogProvider;
        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool readsFormalBattleObject;
        [SerializeField] private bool readsFormalSaveData;
        [SerializeField] private bool writesFormalSystem;

        private ReadOnlyCollection<ItemDetailListEntry> listEntries;
        private IItemDetailInstanceDataAdapter adapter;
        private ItemInstanceProjectionQaFixture fixture;

        public bool DevOnly => devOnly;
        public bool ReadsFormalBattleObject => readsFormalBattleObject;
        public bool ReadsFormalSaveData => readsFormalSaveData;
        public bool WritesFormalSystem => writesFormalSystem;
        public int InstanceCount
        {
            get
            {
                EnsureInitialized();
                return fixture?.GeneratedInstances.Count ?? 0;
            }
        }

#if UNITY_EDITOR
        public void ConfigureEditor(ItemInnerDataCatalogProvider configuredCatalogProvider)
        {
            catalogProvider = configuredCatalogProvider;
            ResetProvider();
        }
#endif

        public IReadOnlyList<ItemDetailListEntry> GetItemList()
        {
            EnsureInitialized();
            return listEntries;
        }

        public ItemDetailViewModel GetDetailViewModel(string itemInstanceId)
        {
            EnsureInitialized();
            return adapter.Project(new ItemDetailInstanceProjectionInput
            {
                contextKind = ItemDetailProjectionContextKind.GeneratedInstancePreview,
                itemInstanceId = itemInstanceId
            }).viewModel;
        }

        public ItemDetailInstanceProjectionResult ProjectExact(string itemInstanceId)
        {
            EnsureInitialized();
            return adapter.Project(new ItemDetailInstanceProjectionInput
            {
                contextKind = ItemDetailProjectionContextKind.GeneratedInstancePreview,
                itemInstanceId = itemInstanceId
            });
        }

        public ItemDetailInstanceProjectionResult ProjectCatalog(string baseItemId)
        {
            EnsureInitialized();
            return adapter.Project(new ItemDetailInstanceProjectionInput
            {
                contextKind = ItemDetailProjectionContextKind.CatalogPreview,
                baseItemId = baseItemId
            });
        }

        public ItemInstanceProjectionQaFixture GetFixture()
        {
            EnsureInitialized();
            return fixture;
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

            fixture = ItemInstanceProjectionQaFixture.Create();
            adapter = new ItemDetailInstanceDataAdapter(
                catalogProvider,
                fixture.ProjectionSet,
                fixture.StatSchema,
                fixture.AffixSchema);
            listEntries = Array.AsReadOnly(fixture.GeneratedInstances
                .Select(source =>
                {
                    ItemDetailViewModel catalog = catalogProvider?.GetDetailViewModel(source.baseItemId);
                    string displayName = catalog != null
                        && string.Equals(catalog.itemId, source.baseItemId, StringComparison.Ordinal)
                        ? catalog.displayItemName
                        : source.baseItemId;
                    string shortId = source.itemInstanceId.Length <= 8
                        ? source.itemInstanceId
                        : source.itemInstanceId.Substring(source.itemInstanceId.Length - 8);
                    return new ItemDetailListEntry(
                        source.itemInstanceId,
                        $"{displayName} · {source.rarity.ToDisplayName()} · {shortId}");
                })
                .ToArray());
        }

        private void ResetProvider()
        {
            fixture = null;
            adapter = null;
            listEntries = null;
        }
    }
}
