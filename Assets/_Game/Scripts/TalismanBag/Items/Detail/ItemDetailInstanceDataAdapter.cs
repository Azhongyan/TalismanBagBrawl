using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Detail
{
    public enum ItemDetailInstanceProjectionStatus
    {
        Success = 0,
        InvalidInput = 1,
        InstanceUnavailable = 2,
        CatalogUnavailable = 3,
        PlacedInstanceBindingUnavailable = 4
    }

    public sealed class ItemDetailInstanceProjectionInput
    {
        public ItemDetailProjectionContextKind contextKind;
        public string baseItemId;
        public string itemInstanceId;
        public string placementId;
    }

    public sealed class ItemDetailInstanceProjectionResult
    {
        internal ItemDetailInstanceProjectionResult(
            ItemDetailInstanceProjectionStatus status,
            ItemDetailProjectionContextKind contextKind,
            ItemDetailViewModel viewModel,
            ItemInstanceProjectionContractSnapshot projection,
            string diagnostic)
        {
            this.status = status;
            this.contextKind = contextKind;
            this.viewModel = viewModel;
            this.projection = projection;
            this.diagnostic = diagnostic ?? string.Empty;
        }

        public bool isSuccess => status == ItemDetailInstanceProjectionStatus.Success && viewModel != null;
        public ItemDetailInstanceProjectionStatus status { get; }
        public ItemDetailProjectionContextKind contextKind { get; }
        public ItemDetailViewModel viewModel { get; }
        public ItemInstanceProjectionContractSnapshot projection { get; }
        public string diagnostic { get; }
    }

    public interface IItemDetailInstanceDataAdapter
    {
        ItemDetailInstanceProjectionResult Project(ItemDetailInstanceProjectionInput input);
    }

    /// <summary>
    /// Read-only bridge from Item Catalog + generated-instance projection facts to ItemDetailViewModel.
    /// It does not roll, place, awaken, activate a Build, or execute battle rules.
    /// </summary>
    public sealed class ItemDetailInstanceDataAdapter : IItemDetailInstanceDataAdapter
    {
        private const string Unavailable = "状态不可用/尚未接入";

        private readonly IItemDetailViewModelProvider catalogProvider;
        private readonly ItemInstanceProjectionSetSnapshot projectionSet;
        private readonly ItemStatRangeSchemaSnapshot statSchema;
        private readonly ItemAffixPoolAndRangeSchemaSnapshot affixSchema;

        public ItemDetailInstanceDataAdapter(
            IItemDetailViewModelProvider catalogProvider,
            ItemInstanceProjectionSetSnapshot projectionSet,
            ItemStatRangeSchemaSnapshot statSchema,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema)
        {
            this.catalogProvider = catalogProvider;
            this.projectionSet = projectionSet;
            this.statSchema = statSchema;
            this.affixSchema = affixSchema;
        }

        public ItemDetailInstanceProjectionResult Project(ItemDetailInstanceProjectionInput input)
        {
            if (input == null)
            {
                return Failure(ItemDetailInstanceProjectionStatus.InvalidInput,
                    ItemDetailProjectionContextKind.CatalogPreview, string.Empty, "Projection input is null.");
            }

            switch (input.contextKind)
            {
                case ItemDetailProjectionContextKind.CatalogPreview:
                    return ProjectCatalog(input.baseItemId);
                case ItemDetailProjectionContextKind.GeneratedInstancePreview:
                    return ProjectGenerated(input.itemInstanceId);
                case ItemDetailProjectionContextKind.PlacedInstance:
                    return Failure(
                        ItemDetailInstanceProjectionStatus.PlacedInstanceBindingUnavailable,
                        input.contextKind,
                        input.itemInstanceId,
                        "This package does not create placementId -> itemInstanceId binding; use the existing strict placement composer.");
                default:
                    return Failure(ItemDetailInstanceProjectionStatus.InvalidInput, input.contextKind,
                        input.itemInstanceId, "Unsupported projection context.");
            }
        }

        private ItemDetailInstanceProjectionResult ProjectCatalog(string baseItemId)
        {
            ItemDetailViewModel catalog = FindCatalog(baseItemId);
            if (catalog == null)
            {
                return Failure(ItemDetailInstanceProjectionStatus.CatalogUnavailable,
                    ItemDetailProjectionContextKind.CatalogPreview, string.Empty,
                    $"Catalog baseItemId '{Normalize(baseItemId)}' was not found.");
            }

            ItemDetailViewModel model = ItemDetailProjectionComposer.Compose(
                catalog,
                ItemDetailProjectionContextKind.CatalogPreview,
                string.Empty,
                null,
                null,
                null,
                null,
                null);
            model.itemInstanceId = string.Empty;
            model.baseItemId = catalog.itemId;
            model.rarityKey = catalog.rarityColorKey;
            model.rarityDisplayName = catalog.displayRarityName;
            return Success(ItemDetailProjectionContextKind.CatalogPreview, model, null);
        }

        private ItemDetailInstanceProjectionResult ProjectGenerated(string itemInstanceId)
        {
            string normalized = Normalize(itemInstanceId);
            if (normalized.Length == 0 || projectionSet == null)
            {
                return Failure(ItemDetailInstanceProjectionStatus.InstanceUnavailable,
                    ItemDetailProjectionContextKind.GeneratedInstancePreview, normalized,
                    normalized.Length == 0 ? "itemInstanceId is required." : "Projection set is unavailable.");
            }

            ItemInstanceProjectionQueryResult query = projectionSet.QueryByItemInstanceId(normalized);
            if (!query.isSuccess || query.snapshot == null)
            {
                return Failure(ItemDetailInstanceProjectionStatus.InstanceUnavailable,
                    ItemDetailProjectionContextKind.GeneratedInstancePreview, normalized,
                    $"Exact itemInstanceId query failed with '{query.primaryValidationCode}'; no baseItemId fallback was attempted.");
            }

            ItemInstanceProjectionContractSnapshot projection = query.snapshot;
            ItemDetailViewModel catalog = FindCatalog(projection.baseItemId);
            if (catalog == null)
            {
                return Failure(ItemDetailInstanceProjectionStatus.CatalogUnavailable,
                    ItemDetailProjectionContextKind.GeneratedInstancePreview, normalized,
                    $"Catalog baseItemId '{projection.baseItemId}' was not found.");
            }

            ItemDetailViewModel model = BuildGeneratedModel(catalog, projection);
            return Success(ItemDetailProjectionContextKind.GeneratedInstancePreview, model, projection);
        }

        private ItemDetailViewModel BuildGeneratedModel(
            ItemDetailViewModel catalog,
            ItemInstanceProjectionContractSnapshot projection)
        {
            ItemDetailViewModel model = catalog.Clone();
            model.itemId = projection.baseItemId;
            model.baseItemId = projection.baseItemId;
            model.itemInstanceId = projection.itemInstanceId;
            model.placementId = string.Empty;
            model.rarityKey = projection.rarityKey;
            model.rarityDisplayName = projection.rarity.ToDisplayName();
            model.displayRarityName = model.rarityDisplayName;
            model.rarityColorKey = projection.rarityKey;
            model.displayItemPower = ItemDetailPresentationFormatter.ItemPowerUnavailable;
            model.displayPrimaryStats = BuildStats(projection);
            model.displayFixedAffixes = BuildAffixes(projection, ItemAffixSlotKind.Fixed);
            model.displayRandomAffixes = BuildAffixes(projection, ItemAffixSlotKind.Random);
            model.displayOrangeAffix = "实例词条以本次生成结果为准；目录橙色词条预览不作为实例事实。";
            model.displayCoreEffects = new List<ItemDetailTextLine>
            {
                new("当前品阶可培养", $"{projection.EligibleCoreEffectIds.Count} 项", "eligible"),
                new("当前品阶可见", $"{projection.VisibleCoreEffectIds.Count} 项", "visible"),
                new("养成状态", Unavailable, "cultivation-unavailable"),
                new("战斗发动状态", Unavailable, "battle-unavailable")
            };
            model.statusFlags = new ItemDetailStatusFlags();
            model.displayLightingStatusText = Unavailable;
            model.displayArrayBonusStatusText = Unavailable;
            model.displayAwakeningStatusText = Unavailable;
            model.lightingPreview = new ItemLightingPreview { previewText = Unavailable };
            model.buildPreview = new ItemBuildPreview
            {
                countedInBuild = false,
                faMenPreviewText = QualificationText(projection.buildQualification, true),
                qiLeiPreviewText = QualificationText(projection.buildQualification, false)
            };
            model.awakeningPreview = new ItemAwakeningPreview
            {
                validationErrorsText = Unavailable,
                readOnlyInputText = "Generated projection exposes potential only; cultivation input is unavailable."
            };
            model.skillMonitorPreview = new ItemSkillMonitorPreview
            {
                selectedMainBuildId = "None",
                selectedMainBuildFound = false,
                previewText = Unavailable,
                validationErrorsText = Unavailable
            };
            model.battleEffectPreview = new ItemBattleEffectPreview
            {
                basicEffectActive = false,
                coreEffectActive = false,
                battleStateText = Unavailable
            };
            model.displayFaMenBuilds = new List<ItemDetailBuildPreview>
            {
                new(model.displayFaMenName, "实例资格", QualificationText(projection.buildQualification, true), false)
            };
            model.displayQiLeiBuilds = new List<ItemDetailBuildPreview>
            {
                new(model.displayQiLeiName, "实例资格", QualificationText(projection.buildQualification, false), false)
            };
            model.displayPlayerSections = BuildPlayerSections(model, projection);
            model.displayDebugSections = BuildDebugSections(model, projection);
            return model;
        }

        private List<ItemDetailStatLine> BuildStats(ItemInstanceProjectionContractSnapshot projection)
        {
            List<ItemDetailStatLine> output = new();
            foreach (ItemInstanceProjectionStatSnapshot stat in projection.Stats)
            {
                ItemStatQueryResult<ItemStatDefinitionSnapshot> query = statSchema?.QueryStatDefinition(stat.statId);
                ItemStatDefinitionSnapshot definition = query != null && query.isSuccess ? query.value : null;
                string label = definition?.displayName;
                if (string.IsNullOrWhiteSpace(label))
                {
                    label = "未命名属性";
                }

                output.Add(new ItemDetailStatLine(
                    label,
                    ItemDetailPresentationFormatter.FormatStat(stat.rawUnits, definition),
                    ItemDetailPresentationFormatter.UnitHint(definition?.unitKey)));
            }

            return output;
        }

        private List<ItemDetailTextLine> BuildAffixes(
            ItemInstanceProjectionContractSnapshot projection,
            ItemAffixSlotKind slotKind)
        {
            List<ItemDetailTextLine> output = new();
            foreach (ItemInstanceProjectionAffixSnapshot affix in projection.Affixes
                .Where(value => value.slotKind == slotKind))
            {
                ItemAffixQueryResult<ItemAffixDefinitionSnapshot> query = affixSchema?.QueryAffixDefinition(affix.affixId);
                ItemAffixDefinitionSnapshot definition = query != null && query.isSuccess ? query.value : null;
                string displayName = string.IsNullOrWhiteSpace(definition?.displayName)
                    ? "未命名词条"
                    : definition.displayName;
                output.Add(new ItemDetailTextLine(
                    displayName,
                    ItemDetailPresentationFormatter.FormatAffix(affix.rawUnits, definition),
                    slotKind == ItemAffixSlotKind.Fixed ? "fixed-instance" : "random-instance"));
            }

            return output;
        }

        private static List<ItemDetailSectionViewModel> BuildPlayerSections(
            ItemDetailViewModel model,
            ItemInstanceProjectionContractSnapshot projection)
        {
            string stats = Lines(model.displayPrimaryStats.Select(value =>
                string.IsNullOrWhiteSpace(value.hint)
                    ? $"{value.label}：{value.value}"
                    : $"{value.label}：{value.value}（{value.hint}）"));
            string fixedAffixes = TextLines(model.displayFixedAffixes);
            string randomAffixes = TextLines(model.displayRandomAffixes);
            string qualification = QualificationSummary(projection.buildQualification);
            return new List<ItemDetailSectionViewModel>
            {
                Section("道具头部", $"名称：{model.displayItemName}\n品阶：{model.rarityDisplayName}", "header"),
                Section("核心身份", $"法门：{model.displayFaMenName}\n器类：{model.displayQiLeiName}\n形状：{model.displayShapeName}", "identity"),
                Section("当前状态", "生成实例只读预览；摆放、点亮、养成与战斗状态尚未接入。", "currentState"),
                Section("基础属性", stats, "stats"),
                Section("触发条件", NonEmpty(model.displayTriggerText, "当前目录未配置"), "trigger"),
                Section("基础效果", TextLines(model.displayBasicEffects), "basic"),
                Section("核心效果潜力", $"当前品阶可培养：{projection.EligibleCoreEffectIds.Count} 项\n当前品阶可见：{projection.VisibleCoreEffectIds.Count} 项\n养成状态：{Unavailable}\n战斗发动状态：{Unavailable}", "coreEffect"),
                Section("法门Build资格", QualificationText(projection.buildQualification, true), "famenBuild"),
                Section("器类Build资格", QualificationText(projection.buildQualification, false), "qileiBuild"),
                Section("Build边界", qualification + "\n这里只表示实例资格，不表示已计入、已激活、主Build已选择或技能已触发。", "skillMonitor"),
                Section("固定词条", fixedAffixes, "fixedAffix"),
                Section("随机词条", randomAffixes, "randomAffix"),
                Section("养成与战斗", $"养成状态：{Unavailable}\n战斗发动状态：{Unavailable}", "orange"),
                Section("摆放提示", "摆放实例身份仍由现有摆放详情链路负责；本包不建立生成实例绑定。", "placement"),
                Section("文化描述", NonEmpty(model.displayFlavorText, "当前目录未配置"), "flavor")
            };
        }

        private static List<ItemDetailSectionViewModel> BuildDebugSections(
            ItemDetailViewModel model,
            ItemInstanceProjectionContractSnapshot projection)
        {
            string identity = Lines(new[]
            {
                "contextKind: GeneratedInstancePreview",
                "itemInstanceId: " + projection.itemInstanceId,
                "baseItemId: " + projection.baseItemId,
                "rarity: " + projection.rarity,
                "rarityKey: " + projection.rarityKey,
                "rarityDisplayName: " + model.rarityDisplayName,
                "placementId: None"
            });
            string trace = Lines(new[]
            {
                "schemaId: " + projection.schemaId,
                "sourceSchemaId: " + projection.sourceSchemaId,
                "sourceGenerationAlgorithmId: " + projection.sourceGenerationAlgorithmId,
                "generationVersion: " + projection.generationVersion.ToString(CultureInfo.InvariantCulture),
                "rootSeed: " + projection.rootSeed.ToString(CultureInfo.InvariantCulture),
                "generationDataStatus: " + projection.generationDataStatus,
                "sourceCanonicalSignature: " + projection.sourceCanonicalSignature,
                "stats: " + string.Join(" | ", projection.Stats.Select(value => $"{value.statId}={value.rawUnits}")),
                "affixes: " + string.Join(" | ", projection.Affixes.Select(value => $"{value.slotKind}:{value.affixId}={value.rawUnits}"))
            });
            string boundary = Lines(new[]
            {
                "eligibleCoreEffectIds: " + Join(projection.EligibleCoreEffectIds),
                "visibleCoreEffectIds: " + Join(projection.VisibleCoreEffectIds),
                "buildQualification: " + projection.buildQualification,
                "cultivationUnlocked: UNAVAILABLE",
                "battleActive: UNAVAILABLE",
                "fixtureStatus: QA_FIXTURE_ONLY",
                "balanceStatus: NOT_BALANCE_APPROVED",
                "formalGenerationStatus: NOT_FORMAL_GENERATION_DATA"
            });
            return new List<ItemDetailSectionViewModel>
            {
                Section("调试 / 实例身份", identity),
                Section("调试 / 生成溯源", trace),
                Section("调试 / 状态边界", boundary)
            };
        }

        private ItemDetailViewModel FindCatalog(string baseItemId)
        {
            string normalized = Normalize(baseItemId);
            if (normalized.Length == 0 || catalogProvider == null)
            {
                return null;
            }

            ItemDetailViewModel model = catalogProvider.GetDetailViewModel(normalized);
            return model != null && string.Equals(model.itemId, normalized, StringComparison.Ordinal)
                ? model
                : null;
        }

        private static ItemDetailInstanceProjectionResult Success(
            ItemDetailProjectionContextKind contextKind,
            ItemDetailViewModel model,
            ItemInstanceProjectionContractSnapshot projection)
        {
            return new ItemDetailInstanceProjectionResult(
                ItemDetailInstanceProjectionStatus.Success,
                contextKind,
                model,
                projection,
                string.Empty);
        }

        private static ItemDetailInstanceProjectionResult Failure(
            ItemDetailInstanceProjectionStatus status,
            ItemDetailProjectionContextKind contextKind,
            string itemInstanceId,
            string diagnostic)
        {
            ItemDetailViewModel unavailable = new()
            {
                itemId = string.Empty,
                baseItemId = string.Empty,
                itemInstanceId = Normalize(itemInstanceId),
                displayItemName = "实例不可用",
                displayRarityName = "未定阶",
                rarityDisplayName = "未定阶",
                displayLightingStatusText = Unavailable,
                displayAwakeningStatusText = Unavailable,
                displayArrayBonusStatusText = Unavailable,
                displayPlayerSections = new List<ItemDetailSectionViewModel>
                {
                    Section("实例状态", "严格实例查询失败；未退回目录原型。")
                },
                displayDebugSections = new List<ItemDetailSectionViewModel>
                {
                    Section("调试 / 实例身份", $"contextKind: {contextKind}\nitemInstanceId: {Normalize(itemInstanceId)}"),
                    Section("调试 / 查询状态", $"status: {status}\ndiagnostic: {diagnostic}"),
                    Section("调试 / 回退边界", "baseItemIdFallback: false")
                }
            };
            return new ItemDetailInstanceProjectionResult(status, contextKind, unavailable, null, diagnostic);
        }

        private static string QualificationSummary(ItemBuildQualification qualification)
        {
            return qualification switch
            {
                ItemBuildQualification.None => "Build资格：None",
                ItemBuildQualification.FaMenOnly => "Build资格：仅法门",
                ItemBuildQualification.QiLeiOnly => "Build资格：仅器类",
                ItemBuildQualification.Dual => "Build资格：法门与器类",
                _ => "Build资格：未解析"
            };
        }

        private static string QualificationText(ItemBuildQualification qualification, bool faMen)
        {
            bool eligible = faMen
                ? qualification == ItemBuildQualification.FaMenOnly || qualification == ItemBuildQualification.Dual
                : qualification == ItemBuildQualification.QiLeiOnly || qualification == ItemBuildQualification.Dual;
            return eligible
                ? "本实例具有该类Build资格；尚未计入或激活。"
                : qualification == ItemBuildQualification.None
                    ? "BuildQualification.None；本实例没有Build资格。"
                    : "本实例不具有该类Build资格。";
        }

        private static ItemDetailSectionViewModel Section(string title, string body, string stateKey = "")
        {
            return new ItemDetailSectionViewModel(title, Lines((body ?? string.Empty).Split('\n')), stateKey, true);
        }

        private static string TextLines(IEnumerable<ItemDetailTextLine> values)
        {
            return Lines((values ?? Array.Empty<ItemDetailTextLine>())
                .Where(value => value != null)
                .Select(value => string.IsNullOrWhiteSpace(value.title)
                    ? value.body
                    : value.title + "：" + value.body));
        }

        private static string Lines(IEnumerable<string> values)
        {
            string[] lines = (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray();
            return lines.Length == 0 ? "  当前无数据" : "  " + string.Join("\n  ", lines);
        }

        private static string Join(IReadOnlyList<string> values)
        {
            return values == null || values.Count == 0 ? "None" : string.Join("|", values);
        }

        private static string NonEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
