using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattlePrepareComponentAdapter
    {
        public const string PackageName = "V0.4-BattlePrepareComponentAdapter01";
        public const string ReferenceMode = "ExtensionAdapterNoFormalUiWrite";

        public string packageName = PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsFormalSaveData;
        public bool writesFormalScene;
        public bool writesFormalUi;
        public bool setParentsFormalUi;
        public bool overridesFormalLayoutFrame;
        public bool touchesFormalPrepareController;
        public bool touchesRunFlow;
        public bool touchesPageState;
        public bool touchesFormationState;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public bool playerUiShowsEnglishStableKey;
        public bool playerUiShowsFullAnswers;
        public string sourcePreviewBuildId = string.Empty;
        public string baseComponentName = BattlePageViewFormalReferenceKeys.PrepareController;
        public string baseComponentMode = "BaseComponentSourceOnly";
        public string referenceMode = ReferenceMode;
        public BattlePrepareComponentAdapterViewModel viewModel = new();

        public static BattlePrepareComponentAdapter Build(
            BuildSandboxPreviewContext context,
            BattlePageViewAdapter battlePageAdapter = null)
        {
            BuildSandboxPreviewContext safeContext = context ?? new BuildSandboxPreviewContext();
            BattlePageViewSpec spec =
                battlePageAdapter?.spec ?? BattlePageViewAdapter.CreateDefault().spec;

            BattlePrepareComponentAdapter adapter = new()
            {
                packageName = PackageName,
                devOnly = true,
                isEnabled = false,
                readsFormalSaveData = false,
                writesFormalScene = false,
                writesFormalUi = false,
                setParentsFormalUi = false,
                overridesFormalLayoutFrame = false,
                touchesFormalPrepareController = false,
                touchesRunFlow = false,
                touchesPageState = false,
                touchesFormationState = false,
                touchesSaveData = false,
                touchesBossRewardDropNumeric = false,
                playerUiShowsEnglishStableKey = false,
                playerUiShowsFullAnswers = false,
                sourcePreviewBuildId = safeContext.previewBuildId ?? string.Empty,
                baseComponentName = BattlePageViewFormalReferenceKeys.PrepareController,
                baseComponentMode = "BaseComponentSourceOnly",
                referenceMode = ReferenceMode
            };

            adapter.viewModel = BuildViewModel(safeContext, spec);
            return adapter;
        }

        private static BattlePrepareComponentAdapterViewModel BuildViewModel(
            BuildSandboxPreviewContext context,
            BattlePageViewSpec spec)
        {
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems =
                context.layoutSnapshot?.placedItems ?? new List<BuildSandboxPlacedItemSnapshot>();
            BuildSandboxPreviewViewModel preview = context.viewModel ?? new BuildSandboxPreviewViewModel();

            BattlePrepareComponentAdapterViewModel viewModel = new()
            {
                sourcePreviewBuildId = context.previewBuildId ?? string.Empty,
                boardOccupancy = BuildBoardOccupancy(placedItems, preview, spec),
                itemTrayShape = BuildItemTrayShape(placedItems, spec),
                dragRotationPlacement = BuildDragRotationPlacement(placedItems, preview),
                itemInfoBuildFields = BuildItemInfoFields(placedItems, preview),
                battleFeedbackMechanicHint =
                    MechanicHintFeedbackPreviewBuilder.Build(context, spec).feedbackExtension
            };
            return viewModel;
        }

        private static BoardOccupancyExtensionViewModel BuildBoardOccupancy(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            BuildSandboxPreviewViewModel preview,
            BattlePageViewSpec spec)
        {
            BoardOccupancyExtensionViewModel model = new()
            {
                extensionId = "BoardOccupancyExtension",
                targetComponentName = "V02FormationGridFrame",
                targetNodeName = spec?.boardGrid?.formalReferenceName ?? "V02FormationGridFrame",
                adapterOutputKey = "boardOccupancy",
                canWriteFormalUi = false,
                requiresExplicitRuntimeBinding = true,
                placementLegal = preview?.shapeOccupancy?.placementLegal ?? true
            };

            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null)
                {
                    continue;
                }

                foreach (ItemShapeCell cell in item.occupiedCells ?? new List<ItemShapeCell>())
                {
                    model.cells.Add(new BoardOccupancyCellRow
                    {
                        itemId = item.itemId ?? string.Empty,
                        shapeId = item.shapeId ?? string.Empty,
                        x = cell.x,
                        y = cell.y,
                        rotation = item.rotation,
                        powered = item.isPowered,
                        playerHintChinese = FormatCellHint(item, cell)
                    });
                }
            }

            model.occupiedCellCount = model.cells.Count;
            model.placedItemCount = placedItems?.Count ?? 0;
            model.invalidReasonSummaries = Clean(preview?.shapeOccupancy?.invalidReasons);
            return model;
        }

        private static ItemTrayShapeExtensionViewModel BuildItemTrayShape(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            BattlePageViewSpec spec)
        {
            ItemTrayShapeExtensionViewModel model = new()
            {
                extensionId = "ItemTrayShapeExtension",
                targetComponentName = BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot,
                targetNodeName = spec?.itemTray?.formalReferenceName ?? BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot,
                categoryTabsNodeName = spec?.categoryTabs?.formalReferenceName ?? "ItemTrayCategoryTabs",
                scrollNodeName = spec?.itemTray?.scrollAreaName ?? "ItemTrayScroll",
                slotPrefix = spec?.itemTray?.slotPrefix ?? "ItemTrayGridSlot_",
                adapterOutputKey = "itemTrayShape",
                canWriteFormalUi = false,
                requiresExplicitRuntimeBinding = true
            };

            foreach (string category in spec?.categoryTabs?.displayLabels ?? new List<string>())
            {
                model.categoryLabels.Add(category);
            }

            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null)
                {
                    continue;
                }

                model.items.Add(new ItemTrayShapeRow
                {
                    itemId = item.itemId ?? string.Empty,
                    displayNameChinese = FormatItemName(item.itemId),
                    categoryChinese = ResolveCategory(item.tags, item.itemId),
                    shapeId = item.shapeId ?? string.Empty,
                    shapeChinese = FormatShapeName(item.shapeId, item.occupiedCells?.Count ?? 0),
                    occupiedCellCount = item.occupiedCells?.Count ?? 0,
                    rotation = item.rotation,
                    rarityChinese = FormatRarity(item.rarity),
                    tagSummaryChinese = FormatChineseList(item.tags, "暂无标签"),
                    affixSummaryChinese = FormatChineseList(item.affixList, "暂无词条")
                });
            }

            return model;
        }

        private static DragRotationPlacementExtensionViewModel BuildDragRotationPlacement(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            BuildSandboxPreviewViewModel preview)
        {
            DragRotationPlacementExtensionViewModel model = new()
            {
                extensionId = "DragRotationPlacementExtension",
                targetComponentName = "DraggableTalismanItemView",
                targetNodeName = "BattlePrepare drag gesture",
                adapterOutputKey = "dragRotationPlacement",
                canWriteFormalUi = false,
                requiresExplicitRuntimeBinding = true,
                placementLegal = preview?.shapeOccupancy?.placementLegal ?? true
            };

            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null)
                {
                    continue;
                }

                model.rows.Add(new DragRotationPlacementRow
                {
                    itemId = item.itemId ?? string.Empty,
                    shapeId = item.shapeId ?? string.Empty,
                    anchor = FormatCell(item.anchorCell),
                    occupiedCells = FormatCells(item.occupiedCells),
                    rotation = item.rotation,
                    playerFeedbackChinese = FormatPlacementFeedback(item)
                });
            }

            model.invalidReasonSummaries = Clean(preview?.shapeOccupancy?.invalidReasons);
            return model;
        }

        private static ItemInfoBuildFieldsExtensionViewModel BuildItemInfoFields(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            BuildSandboxPreviewViewModel preview)
        {
            ItemInfoBuildFieldsExtensionViewModel model = new()
            {
                extensionId = "ItemInfoBuildFieldsExtension",
                targetComponentName = "V02TalismanTooltipUI",
                targetNodeName = "Item tooltip / selected item info language",
                adapterOutputKey = "itemInfoBuildFields",
                canWriteFormalUi = false,
                requiresExplicitRuntimeBinding = true
            };

            List<string> activeSynergies = Clean(preview?.buildSummary?.activeSynergies);
            List<string> activeThresholds = Clean(preview?.buildSummary?.activeThresholds);
            List<string> globalAffixes = Clean(preview?.affixModifier?.affixIds);

            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                if (item == null)
                {
                    continue;
                }

                List<BattlePrepareItemInfoFieldRow> fields = new()
                {
                    Field("shape.preview", "形状预览", FormatShapeName(item.shapeId, item.occupiedCells?.Count ?? 0), false),
                    Field("shape.occupiedCells", "占用格子", FormatCells(item.occupiedCells), false),
                    Field("shape.rotation", "旋转状态", FormatRotation(item.rotation), false),
                    Field("affix.preview", "词条预览", FormatChineseList(Merge(item.affixList, globalAffixes), "暂无词条"), false),
                    Field("synergy.preview", "羁绊预览", FormatChineseList(activeSynergies, "暂无羁绊"), false),
                    Field("synergy.thresholdPreview", "阈值预览", FormatChineseList(activeThresholds, "暂无阈值"), true)
                };

                model.items.Add(new ItemInfoBuildFieldsRow
                {
                    itemId = item.itemId ?? string.Empty,
                    displayNameChinese = FormatItemName(item.itemId),
                    playerSummaryChinese = FormatItemPlayerSummary(item, activeSynergies),
                    developerFieldCount = fields.Count,
                    fields = fields
                });
            }

            return model;
        }

        private static BattleFeedbackMechanicHintExtensionViewModel BuildFeedbackHints(
            BuildSandboxPreviewViewModel preview)
        {
            BattleFeedbackMechanicHintExtensionViewModel model = new()
            {
                extensionId = "BattleFeedbackMechanicHintExtension",
                targetComponentName = "BattleLog / Tooltip / FloatingCombatText",
                targetNodeName = "Existing battle feedback language",
                adapterOutputKey = "battleFeedbackMechanicHint",
                canWriteFormalUi = false,
                requiresExplicitRuntimeBinding = true
            };

            foreach (string reason in Clean(preview?.shapeOccupancy?.invalidReasons))
            {
                model.rows.Add(Feedback("placement", "摆放反馈", ToPlacementHint(reason), false));
            }

            foreach (string missing in Clean(preview?.synergy?.missingRequirementSummaries))
            {
                model.rows.Add(Feedback("synergy", "羁绊线索", "这套摆法还有一段配合没有接上。", true));
            }

            foreach (BossReadinessViewModel boss in preview?.problemReadiness?.bossRows ?? new List<BossReadinessViewModel>())
            {
                if (boss == null || boss.ready)
                {
                    continue;
                }

                model.rows.Add(Feedback(
                    "readiness",
                    "准备度线索",
                    string.IsNullOrWhiteSpace(boss.displayName)
                        ? "当前构筑还差一点应对能力。"
                        : $"面对{boss.displayName}时，当前构筑还差一点应对能力。",
                    true));
            }

            if (model.rows.Count == 0)
            {
                model.rows.Add(Feedback("normal", "摆放反馈", "当前构筑线索稳定，可继续观察。", false));
            }

            return model;
        }

        private static BattlePrepareItemInfoFieldRow Field(
            string englishStableKey,
            string chineseDisplayName,
            string playerChineseText,
            bool developerOnly)
        {
            return new BattlePrepareItemInfoFieldRow
            {
                englishStableKey = englishStableKey,
                chineseDisplayName = chineseDisplayName,
                playerChineseText = playerChineseText,
                developerOnly = developerOnly,
                playerVisible = !developerOnly,
                maskedFromPlayer = developerOnly,
                canWriteFormalUi = false
            };
        }

        private static BattleFeedbackMechanicHintRow Feedback(
            string englishStableKey,
            string chineseDisplayName,
            string playerChineseText,
            bool maskedFromPlayer)
        {
            return new BattleFeedbackMechanicHintRow
            {
                englishStableKey = englishStableKey,
                chineseDisplayName = chineseDisplayName,
                playerChineseText = playerChineseText,
                playerVisible = !maskedFromPlayer,
                maskedFromPlayer = maskedFromPlayer,
                developerPanelVisible = true,
                canWriteFormalUi = false
            };
        }

        private static string FormatItemPlayerSummary(
            BuildSandboxPlacedItemSnapshot item,
            IReadOnlyList<string> activeSynergies)
        {
            string shape = FormatShapeName(item.shapeId, item.occupiedCells?.Count ?? 0);
            string synergy = activeSynergies != null && activeSynergies.Count > 0
                ? "已有可用配合线索"
                : "暂未形成明显配合";
            return $"{FormatItemName(item.itemId)}：{shape}，{synergy}。";
        }

        private static string FormatCellHint(BuildSandboxPlacedItemSnapshot item, ItemShapeCell cell)
        {
            return $"{FormatItemName(item.itemId)}占用第{cell.x + 1}列第{cell.y + 1}行。";
        }

        private static string FormatPlacementFeedback(BuildSandboxPlacedItemSnapshot item)
        {
            string powered = item.isPowered ? "供能已接上" : "供能待确认";
            return $"{FormatItemName(item.itemId)}已放在{FormatCell(item.anchorCell)}，{powered}。";
        }

        private static string ToPlacementHint(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return "当前位置不能放置。";
            }

            if (reason.IndexOf("OutOfGrid", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "越界：有格子超出棋盘。";
            }

            if (reason.IndexOf("CellOccupied", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "重叠：目标格子已被占用。";
            }

            return "当前位置需要重新确认。";
        }

        private static string FormatItemName(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return "未命名道具";
            }

            if (itemId.IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "离火符";
            }

            if (itemId.IndexOf("guard", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "护阵符";
            }

            if (itemId.IndexOf("cleanse", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "净厄符";
            }

            if (itemId.IndexOf("stone", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "青石炉芯";
            }

            if (itemId.IndexOf("energy", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "聚能香";
            }

            if (itemId.IndexOf("thunder", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "雷引符";
            }

            if (itemId.IndexOf("soul", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "镇魂印";
            }

            return "沙盒道具";
        }

        private static string FormatShapeName(string shapeId, int cellCount)
        {
            if (string.IsNullOrWhiteSpace(shapeId))
            {
                return cellCount > 0 ? $"{cellCount}格形状" : "未知形状";
            }

            if (shapeId.IndexOf("Single", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "单格";
            }

            if (shapeId.IndexOf("Vertical", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "竖二格";
            }

            if (shapeId.IndexOf("Corner", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "拐角三格";
            }

            if (shapeId.IndexOf("Square", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "方四格";
            }

            return cellCount > 0 ? $"{cellCount}格形状" : "多格形状";
        }

        private static string ResolveCategory(IEnumerable<string> tags, string itemId)
        {
            List<string> tokens = Clean(tags);
            if (tokens.Any(value => ContainsAny(value, "talisman", "符")))
            {
                return "符箓";
            }

            if (tokens.Any(value => ContainsAny(value, "tool", "法器")))
            {
                return "法器";
            }

            if (ContainsAny(itemId, "stone", "material", "core"))
            {
                return "材料";
            }

            if (ContainsAny(itemId, "incense", "pill", "energy"))
            {
                return "消耗";
            }

            if (ContainsAny(itemId, "seal", "soul", "bell"))
            {
                return "特殊";
            }

            return "符箓";
        }

        private static string FormatRarity(string rarity)
        {
            if (string.IsNullOrWhiteSpace(rarity))
            {
                return "沙盒";
            }

            return rarity.ToLowerInvariant() switch
            {
                "white" => "白",
                "green" => "绿",
                "blue" => "蓝",
                "purple" => "紫",
                "orange" => "橙",
                _ => "沙盒"
            };
        }

        private static string FormatRotation(ItemShapeRotation rotation)
        {
            return rotation switch
            {
                ItemShapeRotation.Rotation90 => "右转",
                ItemShapeRotation.Rotation180 => "倒转",
                ItemShapeRotation.Rotation270 => "左转",
                _ => "正向"
            };
        }

        private static string FormatCell(ItemShapeCell cell)
        {
            return $"第{cell.x + 1}列第{cell.y + 1}行";
        }

        private static string FormatCells(IEnumerable<ItemShapeCell> cells)
        {
            List<ItemShapeCell> safeCells = (cells ?? Enumerable.Empty<ItemShapeCell>()).ToList();
            if (safeCells.Count == 0)
            {
                return "暂无占格";
            }

            return string.Join("、", safeCells
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .Select(FormatCell));
        }

        private static List<string> Merge(IEnumerable<string> left, IEnumerable<string> right)
        {
            return Clean((left ?? Enumerable.Empty<string>()).Concat(right ?? Enumerable.Empty<string>()));
        }

        private static List<string> Clean(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static string FormatChineseList(IEnumerable<string> values, string empty)
        {
            List<string> list = Clean(values);
            if (list.Count == 0)
            {
                return empty;
            }

            return string.Join("、", list.Select(ToChineseToken));
        }

        private static string ToChineseToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string lower = value.ToLowerInvariant();
            if (ContainsAny(lower, "fire", "li_huo", "lihuo")) return "火势";
            if (ContainsAny(lower, "guard", "shield", "hu_zhen")) return "护阵";
            if (ContainsAny(lower, "cleanse", "jing_e")) return "净厄";
            if (ContainsAny(lower, "energy", "ju_neng")) return "聚能";
            if (ContainsAny(lower, "thunder", "jing_lei")) return "惊雷";
            if (ContainsAny(lower, "control", "zhen_hun")) return "镇魂";
            if (ContainsAny(lower, "orange", "core")) return "核心词条";
            if (ContainsAny(lower, "bond")) return "羁绊加成";
            return "构筑线索";
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return tokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    [Serializable]
    public sealed class BattlePrepareComponentAdapterViewModel
    {
        public string sourcePreviewBuildId = string.Empty;
        public BoardOccupancyExtensionViewModel boardOccupancy = new();
        public ItemTrayShapeExtensionViewModel itemTrayShape = new();
        public DragRotationPlacementExtensionViewModel dragRotationPlacement = new();
        public ItemInfoBuildFieldsExtensionViewModel itemInfoBuildFields = new();
        public BattleFeedbackMechanicHintExtensionViewModel battleFeedbackMechanicHint = new();

        public int OutputSectionCount => 5;
    }

    [Serializable]
    public sealed class BoardOccupancyExtensionViewModel
    {
        public string extensionId = "BoardOccupancyExtension";
        public string targetComponentName = string.Empty;
        public string targetNodeName = string.Empty;
        public string adapterOutputKey = string.Empty;
        public bool canWriteFormalUi;
        public bool requiresExplicitRuntimeBinding = true;
        public int placedItemCount;
        public int occupiedCellCount;
        public bool placementLegal = true;
        public List<BoardOccupancyCellRow> cells = new();
        public List<string> invalidReasonSummaries = new();
    }

    [Serializable]
    public sealed class BoardOccupancyCellRow
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public int x;
        public int y;
        public ItemShapeRotation rotation = ItemShapeRotation.Rotation0;
        public bool powered;
        public string playerHintChinese = string.Empty;
    }

    [Serializable]
    public sealed class ItemTrayShapeExtensionViewModel
    {
        public string extensionId = "ItemTrayShapeExtension";
        public string targetComponentName = string.Empty;
        public string targetNodeName = string.Empty;
        public string categoryTabsNodeName = string.Empty;
        public string scrollNodeName = string.Empty;
        public string slotPrefix = string.Empty;
        public string adapterOutputKey = string.Empty;
        public bool canWriteFormalUi;
        public bool requiresExplicitRuntimeBinding = true;
        public List<string> categoryLabels = new();
        public List<ItemTrayShapeRow> items = new();
    }

    [Serializable]
    public sealed class ItemTrayShapeRow
    {
        public string itemId = string.Empty;
        public string displayNameChinese = string.Empty;
        public string categoryChinese = string.Empty;
        public string shapeId = string.Empty;
        public string shapeChinese = string.Empty;
        public int occupiedCellCount;
        public ItemShapeRotation rotation = ItemShapeRotation.Rotation0;
        public string rarityChinese = string.Empty;
        public string tagSummaryChinese = string.Empty;
        public string affixSummaryChinese = string.Empty;
    }

    [Serializable]
    public sealed class DragRotationPlacementExtensionViewModel
    {
        public string extensionId = "DragRotationPlacementExtension";
        public string targetComponentName = string.Empty;
        public string targetNodeName = string.Empty;
        public string adapterOutputKey = string.Empty;
        public bool canWriteFormalUi;
        public bool requiresExplicitRuntimeBinding = true;
        public bool placementLegal = true;
        public List<DragRotationPlacementRow> rows = new();
        public List<string> invalidReasonSummaries = new();
    }

    [Serializable]
    public sealed class DragRotationPlacementRow
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public string anchor = string.Empty;
        public string occupiedCells = string.Empty;
        public ItemShapeRotation rotation = ItemShapeRotation.Rotation0;
        public string playerFeedbackChinese = string.Empty;
    }

    [Serializable]
    public sealed class ItemInfoBuildFieldsExtensionViewModel
    {
        public string extensionId = "ItemInfoBuildFieldsExtension";
        public string targetComponentName = string.Empty;
        public string targetNodeName = string.Empty;
        public string adapterOutputKey = string.Empty;
        public bool canWriteFormalUi;
        public bool requiresExplicitRuntimeBinding = true;
        public List<ItemInfoBuildFieldsRow> items = new();
    }

    [Serializable]
    public sealed class ItemInfoBuildFieldsRow
    {
        public string itemId = string.Empty;
        public string displayNameChinese = string.Empty;
        public string playerSummaryChinese = string.Empty;
        public int developerFieldCount;
        public List<BattlePrepareItemInfoFieldRow> fields = new();
    }

    [Serializable]
    public sealed class BattlePrepareItemInfoFieldRow
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string playerChineseText = string.Empty;
        public bool developerOnly;
        public bool playerVisible = true;
        public bool maskedFromPlayer;
        public bool canWriteFormalUi;
    }

    [Serializable]
    public sealed class BattleFeedbackMechanicHintExtensionViewModel
    {
        public string extensionId = "BattleFeedbackMechanicHintExtension";
        public string targetComponentName = string.Empty;
        public string targetNodeName = string.Empty;
        public string adapterOutputKey = string.Empty;
        public bool canWriteFormalUi;
        public bool requiresExplicitRuntimeBinding = true;
        public List<BattleFeedbackMechanicHintRow> rows = new();
    }

    [Serializable]
    public sealed class BattleFeedbackMechanicHintRow
    {
        public string englishStableKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string playerChineseText = string.Empty;
        public bool playerVisible = true;
        public bool maskedFromPlayer;
        public bool developerPanelVisible = true;
        public bool canWriteFormalUi;
    }
}
