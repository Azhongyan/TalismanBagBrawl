using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattlePageViewAdapter
    {
        public const string PackageName = "V0.4-BattlePageViewAdapter01";
        public const string ReferenceMode = "ReadOnlySpecNoFormalGameObjectReuse";

        public string packageName = PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsFormalSaveData;
        public bool writesFormalScene;
        public bool writesFormalUi;
        public bool setParentsFormalUi;
        public bool overridesFormalLayoutFrame;
        public bool touchesV02FormationGridFrame;
        public bool touchesFormalPrepareController;
        public bool connectsFormalBattle;
        public BattlePageViewSpec spec = BattlePageViewSpec.CreateDefault();

        public static BattlePageViewAdapter CreateDefault()
        {
            return new BattlePageViewAdapter
            {
                packageName = PackageName,
                devOnly = true,
                isEnabled = false,
                readsFormalSaveData = false,
                writesFormalScene = false,
                writesFormalUi = false,
                setParentsFormalUi = false,
                overridesFormalLayoutFrame = false,
                touchesV02FormationGridFrame = false,
                touchesFormalPrepareController = false,
                connectsFormalBattle = false,
                spec = BattlePageViewSpec.CreateDefault()
            };
        }

        public IReadOnlyList<BattlePageViewSpecSection> BuildSections()
        {
            return spec?.BuildSections() ?? new List<BattlePageViewSpecSection>();
        }
    }

    [Serializable]
    public sealed class BattlePageViewSpec
    {
        public string specId = "battle_page_view_spec_v04_readonly";
        public string packageName = BattlePageViewAdapter.PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool referenceOnly = true;
        public bool reusesFormalGameObjects;
        public bool writesFormalUi;
        public string referenceMode = BattlePageViewAdapter.ReferenceMode;
        public BattlePrepareVisualSpec battlePrepare = BattlePrepareVisualSpec.CreateDefault();
        public BattleGridVisualSpec boardGrid = BattleGridVisualSpec.CreateDefault();
        public ItemTrayVisualSpec itemTray = ItemTrayVisualSpec.CreateDefault();
        public CategoryTabsVisualSpec categoryTabs = CategoryTabsVisualSpec.CreateDefault();
        public SelectedItemInfoVisualSpec selectedItemInfo = SelectedItemInfoVisualSpec.CreateDefault();
        public PlacementFeedbackVisualSpec placementFeedback = PlacementFeedbackVisualSpec.CreateDefault();
        public ActionButtonsVisualSpec actionButtons = ActionButtonsVisualSpec.CreateDefault();
        public DataPanelDockBindingSpec dataPanelDock = DataPanelDockBindingSpec.CreateDefault();
        public BattleUiReuseSpec battleUiReuse = BattleUiReuseSpec.CreateDefault();
        public DeveloperTuningPanelFieldSpec developerTuningPanel = DeveloperTuningPanelFieldSpec.CreateDefault();
        public PlayerHintMaskingSpec playerHintMasking = PlayerHintMaskingSpec.CreateDefault();
        public List<string> readOnlyFormalReferences = new()
        {
            BattlePageViewFormalReferenceKeys.CurrentFormationScene,
            BattlePageViewFormalReferenceKeys.PrepareController,
            "V02FormationGridFrame",
            BattlePageViewFormalReferenceKeys.BottomOperationArea,
            "V02PrimaryActionButtons"
        };

        public int OutputSpecCount => BuildSections().Count;

        public static BattlePageViewSpec CreateDefault()
        {
            return new BattlePageViewSpec();
        }

        public List<BattlePageViewSpecSection> BuildSections()
        {
            return new List<BattlePageViewSpecSection>
            {
                boardGrid.ToSection(),
                itemTray.ToSection(),
                categoryTabs.ToSection(),
                selectedItemInfo.ToSection(),
                placementFeedback.ToSection(),
                actionButtons.ToSection(),
                dataPanelDock.ToSection(),
                battleUiReuse.ToSection(),
                developerTuningPanel.ToSection(),
                playerHintMasking.ToSection()
            };
        }
    }

    [Serializable]
    public sealed class BattlePrepareVisualSpec
    {
        public string specId = "battle_prepare_visual_language";
        public string sourceSummary = "Read-only reference to V03 battle prepare visual language.";
        public float boardSize = 800f;
        public float itemTraySize = 800f;
        public float backgroundOverlayAlpha = 0.5f;
        public string motionRootName = BattlePageViewFormalReferenceKeys.PrepareMotionRoot;
        public string overlayName = BattlePageViewFormalReferenceKeys.PrepareDarkOverlay;
        public string boardRootName = "V02FormationGridFrame";
        public string itemTrayRootName = BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot;
        public bool boardAndTrayMoveTogether = true;
        public bool overlayBehindBoardAndTray = true;
        public bool referenceOnly = true;

        public static BattlePrepareVisualSpec CreateDefault()
        {
            return new BattlePrepareVisualSpec();
        }
    }

    [Serializable]
    public sealed class BattleGridVisualSpec
    {
        public string specId = "BoardGrid";
        public string previewSlotName = "BoardGridPreview";
        public string formalReferenceName = "V02FormationGridFrame";
        public float width = 800f;
        public float height = 800f;
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public string contract = "Use battle-board scale and placement affordance as a V04 preview slot only.";

        public static BattleGridVisualSpec CreateDefault()
        {
            return new BattleGridVisualSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                contract);
        }
    }

    [Serializable]
    public sealed class ItemTrayVisualSpec
    {
        public string specId = "ItemTray";
        public string previewSlotName = "ItemTrayPreview";
        public string formalReferenceName = BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot;
        public string scrollAreaName = "ItemTrayScroll";
        public string slotPrefix = "ItemTrayGridSlot_";
        public float width = 800f;
        public float height = 800f;
        public int columnCount = 5;
        public int rowCount = 8;
        public int visibleRowCount = 5;
        public int slotCount = 40;
        public float slotWidth = 104f;
        public float slotHeight = 104f;
        public float slotSpacingX = 14f;
        public float slotSpacingY = 14f;
        public bool verticalScroll = true;
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public string contract = "Use the battle item tray as a V04 grid inventory preview language.";

        public static ItemTrayVisualSpec CreateDefault()
        {
            return new ItemTrayVisualSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"{contract} {columnCount}x{rowCount}, visibleRows={visibleRowCount}, scroll={verticalScroll}.");
        }
    }

    [Serializable]
    public sealed class CategoryTabsVisualSpec
    {
        public string specId = "CategoryTabs";
        public string previewSlotName = "ItemTrayCategoryTabs";
        public string formalReferenceName = "ItemTrayCategoryTabs";
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public List<string> categoryIds = new()
        {
            "All",
            "Talisman",
            "Tool",
            "Material",
            "Consumable",
            "Special"
        };
        public List<string> displayLabels = new()
        {
            "全部",
            "符箓",
            "法器",
            "材料",
            "消耗",
            "特殊"
        };

        public static CategoryTabsVisualSpec CreateDefault()
        {
            return new CategoryTabsVisualSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"Category filter tabs: {string.Join("/", categoryIds)}.");
        }
    }

    [Serializable]
    public sealed class SelectedItemInfoVisualSpec
    {
        public string specId = "SelectedItemInfo";
        public string previewSlotName = "SelectedItemInfo";
        public string formalReferenceName = "Item tooltip / selected item info language";
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public string contract = "Show selected item name, tags, shape, affix summary, and upgrade/placement hints in V04 only.";

        public static SelectedItemInfoVisualSpec CreateDefault()
        {
            return new SelectedItemInfoVisualSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                contract);
        }
    }

    [Serializable]
    public sealed class PlacementFeedbackVisualSpec
    {
        public string specId = "PlacementFeedback";
        public string previewSlotName = "PlacementFeedback";
        public string formalReferenceName = "Battle prepare placement feedback language";
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public List<string> feedbackStates = new()
        {
            "CanPlace",
            "OutOfBounds",
            "Overlap",
            "MissingEnergy",
            "ShapeRotated",
            "SynergyChanged"
        };
        public string contract = "Report placement legality and Build readiness hints without moving formal board objects.";

        public static PlacementFeedbackVisualSpec CreateDefault()
        {
            return new PlacementFeedbackVisualSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"{contract} states={string.Join("/", feedbackStates)}.");
        }
    }

    [Serializable]
    public sealed class ActionButtonsVisualSpec
    {
        public string specId = "ActionButtons";
        public string previewSlotName = "DevOnlyControlBar";
        public string formalReferenceName = BattlePageViewFormalReferenceKeys.PrepareStateButtons;
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public List<string> actionIds = new()
        {
            "EnterPrepare",
            "ContinueBattle",
            "RunSimulation",
            "ResetPreview",
            "ExportReport"
        };
        public List<string> userFacingLabels = new()
        {
            "整备",
            "继续战斗",
            "Run Simulation",
            "Reset Preview",
            "Export Report"
        };

        public static ActionButtonsVisualSpec CreateDefault()
        {
            return new ActionButtonsVisualSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"Action affordances: {string.Join("/", actionIds)}.");
        }
    }

    [Serializable]
    public sealed class DataPanelDockBindingSpec
    {
        public string specId = "DataPanelDock";
        public string previewSlotName = "BuildSandboxDataPanelDock";
        public string formalReferenceName = "None";
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public List<string> bindingSlotNames = new()
        {
            "BuildSummaryPanelSlot",
            "SynergyPanelSlot",
            "ShapeOccupancyPanelSlot",
            "AffixModifierPanelSlot",
            "ProblemReadinessPanelSlot",
            "SimulationResultPanelSlot"
        };

        public static DataPanelDockBindingSpec CreateDefault()
        {
            return new DataPanelDockBindingSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"Dock binds V04 data panels only: {string.Join("/", bindingSlotNames)}.");
        }
    }

    [Serializable]
    public sealed class BattleUiReuseSpec
    {
        public string specId = "BattleUiReuse";
        public string previewSlotName = "UnifiedBattleFeedbackLanguage";
        public string formalReferenceName = "Existing battle feedback language";
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public bool createsDedicatedMechanicPanels;
        public List<BattleUiReuseRecommendation> recommendations = new()
        {
            new BattleUiReuseRecommendation(
                "BattleHint",
                "battleHint",
                "战斗提示",
                "PlacementFeedback / DevOnlyControlBar hint lane",
                "Use short Chinese clue copy for map rules, enemy pressure, and failed placement feedback.",
                true,
                true,
                "只显示线索、状态和失败反馈，不显示完整解法。"),
            new BattleUiReuseRecommendation(
                "DamageFeedback",
                "damageFeedback",
                "伤害反馈",
                "PlacementFeedback / combat feedback lane",
                "Use the same concise feedback rhythm for damage, mitigation, and trigger feedback.",
                true,
                true,
                "玩家侧只显示中文战斗反馈，不暴露数值调参答案。"),
            new BattleUiReuseRecommendation(
                "CombatLog",
                "combatLog",
                "战斗日志",
                "BuildSandboxDataPanelDock event preview",
                "Reuse one event-feed language for trigger order, mechanic feedback, and simulation summaries.",
                true,
                true,
                "玩家侧日志只保留可理解事件，不显示配置键或权重。"),
            new BattleUiReuseRecommendation(
                "Tooltip",
                "tooltip",
                "状态说明",
                "SelectedItemInfo / tooltip summary",
                "Use selected-item and status tooltip language for tags, shapes, affix summaries, and hint explanations.",
                true,
                true,
                "玩家侧说明只中文；English stable key stays in report/config/dev field layer."),
            new BattleUiReuseRecommendation(
                "BossInfo",
                "bossInfo",
                "Boss信息",
                "ProblemReadinessPanelSlot / boss summary hint",
                "Use a compact boss summary plus staged clues; keep full keys in developer tuning data.",
                true,
                true,
                "玩家侧只显示阶段线索，不显示 Boss 六钥匙完整答案。")
        };

        public static BattleUiReuseSpec CreateDefault()
        {
            return new BattleUiReuseSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"Reuse channels={recommendations.Count}, dedicatedPanels={createsDedicatedMechanicPanels}.");
        }
    }

    [Serializable]
    public sealed class BattleUiReuseRecommendation
    {
        public BattleUiReuseRecommendation(
            string channelId,
            string englishStableKey,
            string chineseDisplayName,
            string reuseSurface,
            string reuseRecommendation,
            bool playerFacingAllowed,
            bool developerPanelAllowed,
            string maskingNote)
        {
            this.channelId = channelId;
            this.englishStableKey = englishStableKey;
            this.chineseDisplayName = chineseDisplayName;
            this.reuseSurface = reuseSurface;
            this.reuseRecommendation = reuseRecommendation;
            this.playerFacingAllowed = playerFacingAllowed;
            this.developerPanelAllowed = developerPanelAllowed;
            this.maskingNote = maskingNote;
        }

        public string channelId;
        public string englishStableKey;
        public string chineseDisplayName;
        public string reuseSurface;
        public string reuseRecommendation;
        public bool playerFacingAllowed;
        public bool developerPanelAllowed;
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public bool createsDedicatedPanel;
        public string maskingNote;
    }

    [Serializable]
    public sealed class DeveloperTuningPanelFieldSpec
    {
        public string specId = "DeveloperTuningPanel";
        public string previewSlotName = "BuildSandboxDataPanelDock";
        public string formalReferenceName = "None";
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public bool playerUiChineseOnly = true;
        public bool playerUiShowsEnglishStableKey;
        public bool englishStableKeysConfigReportDevPanelOnly = true;
        public List<DeveloperTuningFieldBinding> fields = new()
        {
            new DeveloperTuningFieldBinding("buildSummary", "构筑概览", "BuildSummaryPanelSlot", true),
            new DeveloperTuningFieldBinding("synergySummary", "协同概览", "SynergyPanelSlot", true),
            new DeveloperTuningFieldBinding("shapeOccupancy", "格位占用", "ShapeOccupancyPanelSlot", true),
            new DeveloperTuningFieldBinding("affixModifier", "词缀修正", "AffixModifierPanelSlot", true),
            new DeveloperTuningFieldBinding("problemReadiness", "题目准备度", "ProblemReadinessPanelSlot", true),
            new DeveloperTuningFieldBinding("simulationResult", "模拟结果", "SimulationResultPanelSlot", true),
            new DeveloperTuningFieldBinding("dropBiasPreview", "定向倾向", "ProblemReadinessPanelSlot", true),
            new DeveloperTuningFieldBinding("bossSixKeyFullAnswer", "Boss六钥匙完整答案", "ProblemReadinessPanelSlot", true)
        };

        public static DeveloperTuningPanelFieldSpec CreateDefault()
        {
            return new DeveloperTuningPanelFieldSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"Developer fields={fields.Count}, playerChineseOnly={playerUiChineseOnly}, englishKeysPlayerVisible={playerUiShowsEnglishStableKey}.");
        }
    }

    [Serializable]
    public sealed class DeveloperTuningFieldBinding
    {
        public DeveloperTuningFieldBinding(
            string englishStableKey,
            string chineseDisplayName,
            string dataPanelSlot,
            bool developerOnly)
        {
            this.englishStableKey = englishStableKey;
            this.chineseDisplayName = chineseDisplayName;
            this.dataPanelSlot = dataPanelSlot;
            this.developerOnly = developerOnly;
        }

        public string englishStableKey;
        public string chineseDisplayName;
        public string dataPanelSlot;
        public bool developerOnly;
    }

    [Serializable]
    public sealed class PlayerHintMaskingSpec
    {
        public string specId = "PlayerHintMasking";
        public string previewSlotName = "PlayerHintPreview";
        public string formalReferenceName = "None";
        public bool referenceOnly = true;
        public bool canWriteFormalUi;
        public bool playerUiChineseOnly = true;
        public bool playerUiShowsEnglishStableKey;
        public bool developerPanelMayShowFullRules = true;
        public bool playerHintPreviewShowsFullAnswers;
        public List<PlayerHintMaskingRule> maskingRules = new()
        {
            new PlayerHintMaskingRule(
                "hardSolutionTags",
                "硬解标签",
                "显示模糊能力方向和失败反馈，不显示完整标签列表。"),
            new PlayerHintMaskingRule(
                "requiredSynergy",
                "必需协同",
                "显示协同倾向线索，不显示必须选择的完整协同答案。"),
            new PlayerHintMaskingRule(
                "requiredAffix",
                "必需词缀",
                "显示词缀方向提示，不显示必须刷到的完整词缀答案。"),
            new PlayerHintMaskingRule(
                "requiredStats",
                "必需属性",
                "显示压力类型和短板反馈，不显示达标阈值完整答案。"),
            new PlayerHintMaskingRule(
                "dropBiasWeights",
                "定向倾向权重",
                "玩家不显示权重；仅可显示掉落倾向的中文氛围线索。"),
            new PlayerHintMaskingRule(
                "bossSixKeyFullAnswer",
                "Boss六钥匙完整答案",
                "玩家只看阶段线索和失败反馈；完整钥匙组合仅在开发者数据面板。")
        };

        public static PlayerHintMaskingSpec CreateDefault()
        {
            return new PlayerHintMaskingSpec();
        }

        public BattlePageViewSpecSection ToSection()
        {
            return new BattlePageViewSpecSection(
                specId,
                previewSlotName,
                formalReferenceName,
                referenceOnly,
                canWriteFormalUi,
                $"Masking rules={maskingRules.Count}, playerChineseOnly={playerUiChineseOnly}, fullAnswersPlayerVisible={playerHintPreviewShowsFullAnswers}.");
        }
    }

    [Serializable]
    public sealed class PlayerHintMaskingRule
    {
        public PlayerHintMaskingRule(
            string englishStableKey,
            string chineseDisplayName,
            string playerChineseHintPolicy)
        {
            this.englishStableKey = englishStableKey;
            this.chineseDisplayName = chineseDisplayName;
            this.playerChineseHintPolicy = playerChineseHintPolicy;
        }

        public string englishStableKey;
        public string chineseDisplayName;
        public string playerChineseHintPolicy;
        public bool maskRequired = true;
        public bool playerVisible;
        public bool developerDataPanelVisible = true;
        public bool reportVisible = true;
        public bool configVisible = true;
    }

    public readonly struct BattlePageViewSpecSection
    {
        public BattlePageViewSpecSection(
            string sectionId,
            string previewSlotName,
            string formalReferenceName,
            bool referenceOnly,
            bool canWriteFormalUi,
            string contract)
        {
            SectionId = sectionId ?? string.Empty;
            PreviewSlotName = previewSlotName ?? string.Empty;
            FormalReferenceName = formalReferenceName ?? string.Empty;
            ReferenceOnly = referenceOnly;
            CanWriteFormalUi = canWriteFormalUi;
            Contract = contract ?? string.Empty;
        }

        public string SectionId { get; }
        public string PreviewSlotName { get; }
        public string FormalReferenceName { get; }
        public bool ReferenceOnly { get; }
        public bool CanWriteFormalUi { get; }
        public string Contract { get; }
    }

    public static class BattlePageViewFormalReferenceKeys
    {
        public static string CurrentFormationScene => "Scene_TalismanBag_" + "V02_FormationCounter";
        public static string CurrentMainHomeScene => "Scene_TalismanBag_" + "V03_MainHome";
        public static string CurrentUpgradeScene => "Scene_TalismanBag_" + "V03_TalismanUpgrade";
        public static string PrepareController => "V03" + "BattlePrepareInteractionController";
        public static string BottomOperationArea => "V02" + "BottomOperationArea";
        public static string PrepareMotionRoot => "V03" + "BattlePrepareMotionRoot";
        public static string PrepareDarkOverlay => "V03" + "BattlePrepareDarkOverlay";
        public static string PrepareItemTrayRoot => "V03" + "BattlePrepareItemTrayRoot";
        public static string PrepareStateButtons => "V03BattleStateButton / " + "V03PrepareToggleButton";
        public static string FormalLayoutFrameKind => "Rect" + "Transform";
    }
}
