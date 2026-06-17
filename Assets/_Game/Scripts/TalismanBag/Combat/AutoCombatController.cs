using System.Collections.Generic;
using TalismanBag.Combo;
using TalismanBag.Debugging;
using TalismanBag.Enemies;
using TalismanBag.Feedback;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.Progression;
using TalismanBag.Run;
using TalismanBag.Shop;
using TalismanBag.UI;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Boss;
using TalismanBag.V02.Balance;
using TalismanBag.V02.Counters;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Formation;
using TalismanBag.V02.Result;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.Run;
using TalismanBag.V02.Status;
using TalismanBag.V02.Tags;
using TalismanBag.V02.UI;
using UnityEngine;
using UnityEngine.UI;
using PlayerInventory = TalismanBag.Inventory.PlayerTalismanInventory;

namespace TalismanBag.Combat
{
    public enum TalismanCombatState
    {
        Preparing,
        Fighting,
        Victory,
        Defeat,
        RunComplete
    }

    public sealed class AutoCombatController : MonoBehaviour
    {
        private const string SpiritStoneId = "spirit_stone_basic";
        private const string FireTalismanId = "fire_talisman_basic";
        private const string ShieldTalismanId = "shield_talisman_basic";
        private const string QiPillId = "qi_pill_basic";
        private const string ThunderTalismanId = "thunder_talisman_basic";
        private const string SealId = "seal_basic";
        private const string SwordPillId = "sword_pill_basic";
        private const string PeachWoodId = "peach_wood_basic";
        private const string ExorcismBellId = "exorcism_bell_basic";
        private const string WaterTalismanId = "water_talisman_basic";
        private const string PurifyTalismanId = "purify_talisman_basic";
        private const string SoulSuppressTalismanId = "soul_suppress_talisman_basic";
        private const float DefaultWeakCooldownMultiplier = 1.35f;

        [Header("References")]
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private ComboResolver comboResolver;
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private BattleStatsTracker statsTracker;
        [SerializeField] private BattleResultPanel resultPanel;
        [SerializeField] private ComboHighlightController comboHighlightController;
        [SerializeField] private FloatingCombatText floatingCombatText;
        [SerializeField] private TalismanTriggerFeedback talismanTriggerFeedback;
        [SerializeField] private TalismanCombatUI combatUI;
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] private ComboStatusUI comboStatusUI;
        [SerializeField] private RectTransform feedbackRoot;
        [SerializeField] private RunFlowControllerV2 runFlowController;
        [SerializeField] private ShopControllerV2 shopController;
        [SerializeField] private V02RunFlowController v02RunFlowController;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private BagExpansionController bagExpansionController;
        [SerializeField] private CombatSpeedController combatSpeedController;
        [SerializeField] private FormationPowerResolver formationPowerResolver;
        [SerializeField] private EnemySkillController enemySkillController;
        [SerializeField] private CounterMatchResolver counterMatchResolver;
        [SerializeField] private CounterFeedbackController counterFeedbackController;
        [SerializeField] private V02FailureTracker v02FailureTracker;
        [SerializeField] private V02RunStatsTracker v02RunStatsTracker;
        [SerializeField] private V02BossPhaseController v02BossPhaseController;
        [SerializeField] private V02CounterMultiplierConfig v02CounterMultiplierConfig;
        [SerializeField] private BattleBalanceLogger battleBalanceLogger;
        [SerializeField] private V02RunModifierState v02RunModifierState;
        [SerializeField] private V02EnemyIntentUI v02EnemyIntentUI;
        [SerializeField] private V02EnemyPreviewPanel v02EnemyPreviewPanel;
        [SerializeField] private StatusEffectController playerStatusController;
        [SerializeField] private StatusEffectController enemyStatusController;
        [SerializeField] private StatusAnchorUI playerStatusAnchor;
        [SerializeField] private StatusAnchorUI enemyStatusAnchor;
        [SerializeField] private StatusAnchorUI playerBuffAnchor;
        [SerializeField] private StatusAnchorUI playerDebuffAnchor;
        [SerializeField] private StatusAnchorUI enemyBuffAnchor;
        [SerializeField] private StatusAnchorUI enemyDebuffAnchor;
        [SerializeField] private StatusTooltipPanel statusTooltipPanel;
        [SerializeField] private TalismanGridSlotView[] slotViews;
        [SerializeField] private List<DraggableTalismanItemView> itemViews = new();

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button autoPlaceButton;
        [SerializeField] private Button damagePlayerButton;
        [SerializeField] private Button addManaButton;
        [SerializeField] private Button skipSwordButton;
        [SerializeField] private Button skipEvilButton;
        [SerializeField] private Button addRandomShopItemButton;
        [SerializeField] private Button clearSealsButton;
        [SerializeField] private Button triggerAllFeedbackButton;
        [SerializeField] private Button toggleComboHighlightButton;
        [SerializeField] private Button testFloatingTextButton;
        [SerializeField] private Button forceSwordChargeButton;
        [SerializeField] private Button forceSealButton;

        [Header("Stats")]
        [SerializeField] private CombatStats playerStats = new();

        [Header("Tuning")]
        [SerializeField] private int shieldCap = 50;
        [SerializeField] private float fireBoostedCooldown = 1.6f;
        [SerializeField] private float evilManaDrainInterval = 6f;
        [SerializeField] private float evilSealInterval = 10f;
        [SerializeField] private float evilSealDuration = 4f;
        [SerializeField] private float ghostShadowInterval = 7f;
        [SerializeField] private int ghostShadowDamage = 8;
        [SerializeField] private float swordChargeInterval = 8f;
        [SerializeField] private float swordChargeDuration = 2f;
        [SerializeField] private int swordChargeDamage = 30;
        [SerializeField] private float bossDrainInterval = 6f;
        [SerializeField] private float bossSealInterval = 9f;
        [SerializeField] private float bossEnragedSealInterval = 6f;
        [SerializeField] private float bossChargeInterval = 14f;
        [SerializeField] private float bossChargeDuration = 3f;
        [SerializeField] private int bossChargeDamage = 36;

        private readonly List<TalismanItemRuntime> sealedItems = new();
        private readonly HashSet<string> powerSlotNoticeRuntimeIds = new();
        private readonly HashSet<string> unpoweredBlockedRuntimeIds = new();
        private EnemyRuntime currentEnemy;
        private TalismanCombatState state = TalismanCombatState.Preparing;
        private int currentRound = 1;
        private int totalRounds = 7;
        private Text autoMergeButtonText;
        private float v02StatusTickTimer = 1f;
        private float v02EnemyBurnTickTimer = 1f;

        public bool CanEditLayout => state != TalismanCombatState.Fighting;
        public EnemyRuntime CurrentEnemy => currentEnemy;
        public int CurrentRoundNumber => currentRound;
        public float PlayerHpRemainPercent => playerStats.maxHP > 0 ? playerStats.hp / (float)playerStats.maxHP : 0f;
        public CombatStats PlayerStats => playerStats;

        private void Awake()
        {
            EnsureFormationPowerResolver();
            EnsureStatusEffectSystem();

            startButton?.onClick.AddListener(StartBattle);
            resetButton?.onClick.AddListener(ResetBattle);
            autoPlaceButton?.onClick.AddListener(AutoMergeDuplicateLevelOneItems);
            damagePlayerButton?.onClick.AddListener(DamagePlayer20);
            addManaButton?.onClick.AddListener(AddMana30);
            skipSwordButton?.onClick.AddListener(() => runFlowController?.SkipToSwordCultivator());
            skipEvilButton?.onClick.AddListener(() => runFlowController?.SkipToEvilCultivator());
            addRandomShopItemButton?.onClick.AddListener(() => shopController?.AddRandomShopItem());
            clearSealsButton?.onClick.AddListener(ClearAllSeals);
            triggerAllFeedbackButton?.onClick.AddListener(() => talismanTriggerFeedback?.TriggerAllFeedback());
            toggleComboHighlightButton?.onClick.AddListener(() => comboHighlightController?.ToggleComboHighlight());
            testFloatingTextButton?.onClick.AddListener(() => floatingCombatText?.TestFloatingText());
            forceSwordChargeButton?.onClick.AddListener(ForceSwordCharge);
            forceSealButton?.onClick.AddListener(ForceSealRandomItem);

            if (grid != null)
            {
                grid.GridChanged += OnGridChanged;
            }

            if (comboResolver != null)
            {
                comboResolver.CombosChanged += OnCombosChanged;
            }
        }

        private void Start()
        {
            EnsureFormationPowerResolver();
            EnsureStatusEffectSystem();

            foreach (DraggableTalismanItemView view in itemViews)
            {
                view?.CaptureHome();
            }

            RefreshGridDependentUI();
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (grid != null)
            {
                grid.GridChanged -= OnGridChanged;
            }

            if (comboResolver != null)
            {
                comboResolver.CombosChanged -= OnCombosChanged;
            }
        }

        private void EnsureStatusEffectSystem()
        {
            playerStatusController = EnsureStatusController(playerStatusController, "PlayerStatusEffects");
            enemyStatusController = EnsureStatusController(enemyStatusController, "EnemyStatusEffects");
            statusTooltipPanel = EnsureStatusTooltipPanel(statusTooltipPanel);

            playerBuffAnchor = EnsureStatusAnchor(
                playerBuffAnchor,
                "PlayerBuffAnchor",
                "V02PlayerAvatar",
                new Vector2(128f, 66f),
                new Vector2(290f, 48f));
            playerDebuffAnchor = EnsureStatusAnchor(
                playerDebuffAnchor,
                "PlayerDebuffAnchor",
                "V02PlayerAvatar",
                new Vector2(128f, 18f),
                new Vector2(290f, 48f));
            enemyBuffAnchor = EnsureStatusAnchor(
                enemyBuffAnchor,
                "EnemyBuffAnchor",
                "EnemySilhouette",
                new Vector2(116f, 82f),
                new Vector2(320f, 48f));
            enemyDebuffAnchor = EnsureStatusAnchor(
                enemyDebuffAnchor,
                "EnemyDebuffAnchor",
                "EnemySilhouette",
                new Vector2(116f, 34f),
                new Vector2(320f, 48f));

            playerStatusAnchor?.Bind(playerStatusController);
            playerStatusAnchor?.BindTooltip(statusTooltipPanel);
            enemyStatusAnchor?.Bind(enemyStatusController);
            enemyStatusAnchor?.BindTooltip(statusTooltipPanel);
            ConfigureStatusAnchor(playerBuffAnchor, playerStatusController, StatusPolarity.Buff, TextAnchor.MiddleLeft);
            ConfigureStatusAnchor(playerDebuffAnchor, playerStatusController, StatusPolarity.Debuff, TextAnchor.MiddleLeft);
            ConfigureStatusAnchor(enemyBuffAnchor, enemyStatusController, StatusPolarity.Buff, TextAnchor.MiddleRight);
            ConfigureStatusAnchor(enemyDebuffAnchor, enemyStatusController, StatusPolarity.Debuff, TextAnchor.MiddleRight);
        }

        private void ConfigureStatusAnchor(StatusAnchorUI anchor, StatusEffectController controller, StatusPolarity polarity, TextAnchor alignment)
        {
            if (anchor == null)
            {
                return;
            }

            anchor.SetIconAlignment(alignment);
            anchor.Bind(controller);
            anchor.BindTooltip(statusTooltipPanel);
            anchor.SetPolarityFilter(true, polarity);
        }

        private StatusEffectController EnsureStatusController(StatusEffectController controller, string objectName)
        {
            if (controller != null)
            {
                return controller;
            }

            Transform existing = transform.Find(objectName);
            if (existing != null && existing.TryGetComponent(out StatusEffectController existingController))
            {
                return existingController;
            }

            GameObject controllerObject = new(objectName, typeof(StatusEffectController));
            controllerObject.transform.SetParent(transform, false);
            return controllerObject.GetComponent<StatusEffectController>();
        }

        private StatusTooltipPanel EnsureStatusTooltipPanel(StatusTooltipPanel panel)
        {
            if (panel != null)
            {
                return panel;
            }

            Transform parent = feedbackRoot != null ? feedbackRoot : transform;
            Transform existing = parent.Find("StatusTooltipRuntime");
            if (existing != null && existing.TryGetComponent(out StatusTooltipPanel existingPanel))
            {
                return existingPanel;
            }

            GameObject panelObject = new("StatusTooltipRuntime", typeof(RectTransform), typeof(StatusTooltipPanel));
            panelObject.transform.SetParent(parent, false);
            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panelObject.GetComponent<StatusTooltipPanel>();
        }

        private StatusAnchorUI EnsureStatusAnchor(StatusAnchorUI anchor, string objectName, string targetObjectName, Vector2 position, Vector2 size)
        {
            if (anchor != null)
            {
                return anchor;
            }

            GameObject existingObject = GameObject.Find(objectName);
            if (existingObject != null && existingObject.TryGetComponent(out StatusAnchorUI existingAnchor))
            {
                return existingAnchor;
            }

            GameObject target = GameObject.Find(targetObjectName);
            Transform parent = target != null ? target.transform : feedbackRoot != null ? feedbackRoot : transform;
            GameObject anchorObject = new(objectName, typeof(RectTransform), typeof(StatusAnchorUI));
            anchorObject.transform.SetParent(parent, false);
            RectTransform rect = anchorObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return anchorObject.GetComponent<StatusAnchorUI>();
        }

        private void Update()
        {
            if (state != TalismanCombatState.Fighting || currentEnemy == null)
            {
                return;
            }

            float deltaTime = Time.deltaTime * (combatSpeedController != null ? combatSpeedController.CurrentSpeedMultiplier : 1f);
            v02RunStatsTracker?.TickBattle(deltaTime);
            UpdateSeals(deltaTime);
            UpdateV02TemporaryDisables(deltaTime);
            UpdateItems(deltaTime);
            UpdateEnemy(deltaTime);
            UpdateV02PlayerStatusDamage(deltaTime);
            UpdateV02EnemyBurnDamage(deltaTime);
            RefreshUI();
        }

        public void RegisterItemView(DraggableTalismanItemView view)
        {
            if (view != null && !itemViews.Contains(view))
            {
                itemViews.Add(view);
            }
        }

        public void ClearRegisteredItemViewsForInventoryReset()
        {
            ClearAllSeals();
            ClearSlotViewLinks();
            grid?.Clear();

            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                    Destroy(view.gameObject);
                }
            }

            itemViews.Clear();
            RefreshGridDependentUI();
            RefreshUI();
        }

        public void ResetRunStats()
        {
            statsTracker?.ResetStats();
            v02RunModifierState?.ResetState();
        }

        public void SetEnemy(EnemyDefinition enemyDefinition, int roundIndex, int roundCount)
        {
            currentRound = roundIndex;
            totalRounds = Mathf.Max(1, roundCount);
            currentEnemy = new EnemyRuntime(enemyDefinition);
            enemySkillController?.Initialize(currentEnemy);
            v02EnemyPreviewPanel?.Show(enemyDefinition);
            ResetCombatStatsOnly();
            state = TalismanCombatState.Preparing;
            battleLogUI?.Clear();
            AddLog($"第 {currentRound}/{totalRounds} 场：{enemyDefinition.GetReadableLabel()}");
            PlaytestSessionLogger.Log($"Round {currentRound} Started: {enemyDefinition.displayName}");
            PrepareItemCooldowns();
            RefreshGridDependentUI();
            RefreshUI();
        }

        public void SetRunComplete()
        {
            state = TalismanCombatState.RunComplete;
            AddLog("15分钟验证通关，阵型验证完成");
            RefreshUI();
        }

        public void StartBattle()
        {
            if (state == TalismanCombatState.Fighting)
            {
                return;
            }

            if (currentEnemy == null)
            {
                AddLog("缺少当前敌人配置");
                return;
            }

            if (grid == null || grid.GetAllPlacedItems().Count == 0)
            {
                AddLog("请先把道具拖到阵盘里");
                return;
            }

            ClearAllSeals();
            RefreshFormationPowerStates();
            v02RunStatsTracker?.CaptureBattleStart(formationPowerResolver, grid);
            battleBalanceLogger?.BeginBattle(currentRound, currentEnemy, v02RunFlowController != null ? v02RunFlowController.CurrentRound : null);
            state = TalismanCombatState.Fighting;
            resultPanel?.Hide();
            ConfigureEnemyTimers();
            v02BossPhaseController?.ResetPhase();
            enemySkillController?.Initialize(currentEnemy);
            powerSlotNoticeRuntimeIds.Clear();
            PrepareItemCooldowns();
            AddLog("自动斗法开始");
            v02RunFlowController?.OnCombatStarted();
            PlaytestSessionLogger.Log($"Combat Started: Round {currentRound}");
            RefreshUI();
        }

        public void ResetBattle()
        {
            if (currentEnemy != null)
            {
                currentEnemy.Reset(currentEnemy.definition);
            }

            ResetCombatStatsOnly();
            state = TalismanCombatState.Preparing;
            v02BossPhaseController?.ResetPhase();
            battleLogUI?.Clear();
            AddLog("战前整理：观察敌人后调整阵盘");
            PrepareItemCooldowns();
            RefreshGridDependentUI();
            RefreshUI();
        }

        public void AutoPlaceStarterBuild()
        {
            if (!CanEditLayout || grid == null || slotViews == null)
            {
                return;
            }

            foreach (DraggableTalismanItemView view in itemViews)
            {
                view?.ReturnToHome();
            }

            ClearSlotViewLinks();
            grid.Clear();
            PlaceById(SpiritStoneId, new Vector2Int(2, 2));
            PlaceById(FireTalismanId, new Vector2Int(3, 2));
            PlaceById(SwordPillId, new Vector2Int(4, 2));
            PlaceById(ShieldTalismanId, new Vector2Int(2, 1));
            PlaceById(QiPillId, new Vector2Int(3, 1));
            AddLog("已自动摆放推荐阵型");
            RefreshGridDependentUI();
            RefreshUI();
        }

        public void AutoPlaceRecommendedBuild(int roundNumber)
        {
            if (!CanEditLayout || grid == null || slotViews == null)
            {
                return;
            }

            foreach (DraggableTalismanItemView view in itemViews)
            {
                view?.ReturnToHome();
            }

            ClearSlotViewLinks();
            grid.Clear();

            switch (Mathf.Clamp(roundNumber, 1, 7))
            {
                case 1:
                    PlaceById(SpiritStoneId, new Vector2Int(2, 2));
                    PlaceById(FireTalismanId, new Vector2Int(3, 2));
                    PlaceById(ShieldTalismanId, new Vector2Int(2, 1));
                    PlaceById(QiPillId, new Vector2Int(3, 1));
                    break;
                case 2:
                    PlaceById(ThunderTalismanId, new Vector2Int(2, 2));
                    PlaceById(PeachWoodId, new Vector2Int(1, 2));
                    PlaceById(ExorcismBellId, new Vector2Int(3, 2));
                    PlaceById(SpiritStoneId, new Vector2Int(2, 1));
                    break;
                case 3:
                    PlaceById(ThunderTalismanId, new Vector2Int(2, 2));
                    PlaceById(SealId, new Vector2Int(2, 3));
                    PlaceById(ShieldTalismanId, new Vector2Int(1, 2));
                    PlaceById(QiPillId, new Vector2Int(1, 1));
                    PlaceById(SwordPillId, new Vector2Int(3, 2));
                    PlaceById(FireTalismanId, new Vector2Int(3, 3));
                    break;
                case 4:
                    PlaceById(SpiritStoneId, new Vector2Int(1, 2));
                    PlaceById(SpiritStoneId, new Vector2Int(3, 2));
                    PlaceById(FireTalismanId, new Vector2Int(2, 2));
                    PlaceById(ShieldTalismanId, new Vector2Int(1, 1));
                    PlaceById(QiPillId, new Vector2Int(3, 1));
                    PlaceById(WaterTalismanId, new Vector2Int(4, 1));
                    PlaceById(ThunderTalismanId, new Vector2Int(2, 3));
                    break;
                case 5:
                    PlaceById(ExorcismBellId, new Vector2Int(2, 2));
                    PlaceById(PeachWoodId, new Vector2Int(1, 2));
                    PlaceById(ThunderTalismanId, new Vector2Int(3, 2));
                    PlaceById(FireTalismanId, new Vector2Int(2, 3));
                    PlaceById(SpiritStoneId, new Vector2Int(2, 1));
                    break;
                case 6:
                    PlaceById(ThunderTalismanId, new Vector2Int(2, 2));
                    PlaceById(SealId, new Vector2Int(2, 3));
                    PlaceById(SwordPillId, new Vector2Int(3, 2));
                    PlaceById(FireTalismanId, new Vector2Int(3, 3));
                    PlaceById(ShieldTalismanId, new Vector2Int(1, 2));
                    PlaceById(QiPillId, new Vector2Int(1, 1));
                    break;
                case 7:
                    PlaceById(SpiritStoneId, new Vector2Int(1, 2));
                    PlaceById(SpiritStoneId, new Vector2Int(3, 2));
                    PlaceById(ThunderTalismanId, new Vector2Int(2, 2));
                    PlaceById(SealId, new Vector2Int(2, 3));
                    PlaceById(ShieldTalismanId, new Vector2Int(1, 1));
                    PlaceById(QiPillId, new Vector2Int(3, 1));
                    PlaceById(FireTalismanId, new Vector2Int(4, 2));
                    PlaceById(SwordPillId, new Vector2Int(4, 3));
                    break;
            }

            AddLog($"调试：已摆放 Round {Mathf.Clamp(roundNumber, 1, 7)} 推荐阵型");
            RefreshGridDependentUI();
            RefreshUI();
        }

        public void AutoMergeDuplicateLevelOneItems()
        {
            if (!CanEditLayout)
            {
                return;
            }

            bool mergedAny = false;
            itemViews.RemoveAll(view => view == null);
            Dictionary<string, List<DraggableTalismanItemView>> groups = new();
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view == null || view.Definition == null || view.Level != 1)
                {
                    continue;
                }

                string id = view.Definition.itemId;
                if (!groups.ContainsKey(id))
                {
                    groups[id] = new List<DraggableTalismanItemView>();
                }

                groups[id].Add(view);
            }

            foreach (List<DraggableTalismanItemView> group in groups.Values)
            {
                while (group.Count >= 2)
                {
                    DraggableTalismanItemView upgraded = group[0];
                    DraggableTalismanItemView consumed = group[1];
                    group.RemoveRange(0, 2);
                    TalismanItemDefinition mergedDefinition = upgraded.Definition;
                    string mergedName = mergedDefinition != null ? mergedDefinition.displayName : "道具";
                    upgraded.ReturnToHome();
                    consumed.ReturnToHome();
                    if (shopController != null && mergedDefinition != null)
                    {
                        inventory?.RemoveItem(upgraded.RuntimeItem);
                        inventory?.RemoveItem(consumed.RuntimeItem);
                        itemViews.Remove(upgraded);
                        itemViews.Remove(consumed);
                        upgraded.gameObject.SetActive(false);
                        consumed.gameObject.SetActive(false);
                        Destroy(upgraded.gameObject);
                        Destroy(consumed.gameObject);
                        shopController.AddItemToInventory(mergedDefinition, 2);
                    }
                    else
                    {
                        upgraded.SetRuntimeLevel(2);
                        inventory?.RemoveItem(consumed.RuntimeItem);
                        itemViews.Remove(consumed);
                        consumed.gameObject.SetActive(false);
                        Destroy(consumed.gameObject);
                    }

                    if (statsTracker != null)
                    {
                        statsTracker.totalMergedItems++;
                    }
                    mergedAny = true;
                    string mergeMessage = $"{mergedName} Lv1 ×2 合成为 {mergedName} Lv2";
                    AddLog(mergeMessage);
                    PlaytestSessionLogger.Log($"Item Merged: {mergeMessage}");
                }
            }

            if (!mergedAny)
            {
                AddLog("暂无可合成");
            }

            RefreshGridDependentUI();
            RefreshUI();
        }

        public bool CanMergeAny()
        {
            return FindMergeCandidate() != null;
        }

        public void ForceBossPhase2()
        {
            if (!IsBoss())
            {
                StartBossFight();
            }

            if (!IsBoss() || currentEnemy == null || currentEnemy.definition == null)
            {
                AddLog("调试：当前不是 Boss 战");
                return;
            }

            currentEnemy.currentHp = Mathf.Min(currentEnemy.currentHp, Mathf.Max(1, currentEnemy.definition.maxHp / 2));
            currentEnemy.sealTimer = Mathf.Min(currentEnemy.sealTimer, GetSealInterval());
            TriggerBossEnrage(force: true);
            Emit(BattleEventType.EnemyCountered, BattleLogCategory.Danger, "调试：Boss 压入半血狂暴", currentEnemy.definition.enemyId, value: 0, screenPosition: GetEnemyPosition());
            RefreshUI();
        }

        public void StartBossFight()
        {
            runFlowController?.SkipToRound(totalRounds);
            AddLog($"调试：进入 Round {totalRounds} Boss 战");
            RefreshUI();
        }

        public void ForceBossCharge()
        {
            if (!EnsureBossForDebug())
            {
                return;
            }

            currentEnemy.isCharging = false;
            currentEnemy.chargeTimer = 0f;
            StartBossCharge();
            AddLog("调试：强制 Boss 蓄力");
            RefreshUI();
        }

        public void ForceBossEnrage()
        {
            if (!EnsureBossForDebug())
            {
                return;
            }

            TriggerBossEnrage(force: true);
            RefreshUI();
        }

        public void ForceBossSeal()
        {
            if (!EnsureBossForDebug())
            {
                return;
            }

            ForceSealRandomItem();
        }

        public void ForceBossManaDrain()
        {
            if (!EnsureBossForDebug())
            {
                return;
            }

            DrainMana(currentEnemy.definition.GetReadableLabel(), GetManaDrainAmount());
            RefreshUI();
        }

        public void UpgradePlacedCoreBuildToLevel2()
        {
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view == null || view.CurrentSlot == null || view.Definition == null)
                {
                    continue;
                }

                string id = view.Definition.itemId;
                if (id == SpiritStoneId || id == FireTalismanId || id == ShieldTalismanId || id == QiPillId || id == ThunderTalismanId || id == SwordPillId)
                {
                    view.SetRuntimeLevel(2);
                }
            }

            AddLog("调试：核心阵型升级为 Lv2");
            RefreshGridDependentUI();
            RefreshUI();
        }

        public void DamagePlayer20()
        {
            playerStats.TakeDamage(20);
            Emit(BattleEventType.DamageTaken, BattleLogCategory.Damage, "调试：玩家受到 20 点伤害", value: 20, screenPosition: GetPlayerPosition());
            combatUI?.FlashHP();
            RefreshUI();
            CheckDefeat();
            RefreshUI();
        }

        public void AddMana30()
        {
            playerStats.AddMana(30);
            Emit(BattleEventType.ManaGenerated, BattleLogCategory.Mana, "调试：获得 30 点灵气", value: 30, screenPosition: GetManaPosition());
            RefreshUI();
        }

        public void ForceSwordCharge()
        {
            if (currentEnemy == null)
            {
                return;
            }

            if (IsBoss())
            {
                ForceBossCharge();
                return;
            }

            currentEnemy.isCharging = true;
            currentEnemy.chargeTimer = GetChargeDuration();
            Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, "剑修开始蓄力连斩", value: 1, screenPosition: GetEnemyPosition());
        }

        public void ForceSealRandomItem()
        {
            if (grid == null || grid.GetAllPlacedItems().Count == 0)
            {
                AddLog("没有可封印的道具");
                return;
            }

            SealRandomItem();
        }

        private bool EnsureBossForDebug()
        {
            if (!IsBoss())
            {
                StartBossFight();
            }

            if (IsBoss() && currentEnemy != null && currentEnemy.definition != null)
            {
                return true;
            }

            AddLog("调试：当前不是 Boss 战");
            return false;
        }

        public void ClearAllSeals()
        {
            foreach (TalismanItemRuntime item in sealedItems)
            {
                ClearSeal(item);
            }

            sealedItems.Clear();
            RefreshAfterSealStateChanged();
        }

        private void UpdateItems(float deltaTime)
        {
            if (grid == null)
            {
                return;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item == null || item.definition == null || item.isSealed || item.isTemporarilyDisabled || item.definition.itemType == TalismanItemType.PassiveTool)
                {
                    continue;
                }

                if (!IsFormationItemActive(item))
                {
                    TrackUnpoweredTriggerBlocked(item);
                    continue;
                }

                item.cooldownTimer -= deltaTime;
                if (item.cooldownTimer > 0f)
                {
                    continue;
                }

                bool triggered = TryTriggerItem(item);
                item.cooldownTimer = triggered ? GetEffectiveCooldown(item) : 0.1f;
                if (triggered)
                {
                    item.triggerCount++;
                    FlashItem(item);
                }
            }
        }

        private bool TryTriggerItem(TalismanItemRuntime item)
        {
            ResolveTalismanEffect(item);
            switch (item.definition.itemId)
            {
                case SpiritStoneId:
                    AddManaWithWaste(GetSpiritManaGain(item), item);
                    Emit(BattleEventType.ItemTriggered, BattleLogCategory.Normal, "", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
                    EmitManaFlows(item);
                    return true;
                case FireTalismanId:
                    return TryTriggerFireTalisman(item);
                case ShieldTalismanId:
                    return TryTriggerShieldTalisman(item);
                case QiPillId:
                    return TryTriggerQiPill(item);
                case ThunderTalismanId:
                    return TryTriggerThunderTalisman(item);
                case SwordPillId:
                    return TryTriggerSwordPill(item);
                case ExorcismBellId:
                    return TryTriggerExorcismBell(item);
                case WaterTalismanId:
                    return TryTriggerWaterTalisman(item);
                case "chain_thunder_talisman_basic":
                    return TryTriggerChainThunderTalisman(item);
                case "purify_talisman_basic":
                    return TryTriggerPurifyTalisman(item);
                case "soul_suppress_talisman_basic":
                    return TryTriggerSoulSuppressTalisman(item);
                default:
                    return false;
            }
        }

        private void ResolveTalismanEffect(TalismanItemRuntime item)
        {
            if (item?.definition == null)
            {
                return;
            }

            EffectType effectType = item.definition.effectType;
            switch (effectType)
            {
                case EffectType.DealDamage:
                case EffectType.ChainDamage:
                case EffectType.GainShield:
                case EffectType.Heal:
                case EffectType.CleanseStatus:
                case EffectType.SuppressGhost:
                case EffectType.GenerateEnergy:
                case EffectType.EnhanceAdjacent:
                    return;
            }
        }

        private bool TryTriggerTaggedPlaceholder(TalismanItemRuntime item, int baseDamage, string message)
        {
            if (item == null || item.definition == null)
            {
                return false;
            }

            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            if (baseDamage > 0 && currentEnemy != null)
            {
                int damage = item.level >= 2 ? Mathf.RoundToInt(baseDamage * 1.5f) : baseDamage;
                DealDamage(item, damage, $"{GetRuntimeItemName(item)}: {message}");
                return true;
            }

            Emit(BattleEventType.ItemTriggered, BattleLogCategory.Normal, message, item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
            return true;
        }

        private bool TryTriggerChainThunderTalisman(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int damage = item.level >= 2 ? 22 : 14;
            if (v02RunModifierState != null && v02RunModifierState.chainThunderDamageMultiplierBonus > 0f)
            {
                damage = Mathf.RoundToInt(damage * (1f + v02RunModifierState.chainThunderDamageMultiplierBonus));
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "[强化] 连锁雷符强化生效", item.definition.itemId, value: damage, screenPosition: GetItemPosition(item));
            }

            DealDamage(item, damage, $"{GetRuntimeItemName(item)}引发连锁雷击，造成 {damage} 点雷伤害");
            return true;
        }

        private bool TryTriggerPurifyTalisman(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            EnsureStatusEffectSystem();
            bool cleansedStatus = counterMatchResolver != null && counterMatchResolver.TryResolveCleanse(item, playerStatusController);
            bool cleansedSeal = counterMatchResolver != null && counterMatchResolver.TryResolveSealCleanse(item, sealedItems);

            Emit(BattleEventType.ItemTriggered, BattleLogCategory.Normal, "", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));

            if (cleansedStatus)
            {
                v02StatusTickTimer = 1f;
                v02RunStatsTracker?.RecordCleanse();
                counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.Cleanse, GetPlayerPosition());
            }

            if (cleansedSeal)
            {
                if (v02FailureTracker != null)
                {
                    v02FailureTracker.sealCleansedCount++;
                }

                v02RunStatsTracker?.RecordUnseal();
                counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.Unseal, GetItemPosition(item));
                RefreshAfterSealStateChanged();
            }

            if (!cleansedStatus && !cleansedSeal)
            {
                Emit(BattleEventType.LogMessage, BattleLogCategory.Normal, $"{GetRuntimeItemName(item)}净化流转，但当前没有负面状态或封印", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
            }

            playerStatusAnchor?.Refresh();
            playerDebuffAnchor?.Refresh();
            return true;
        }

        private bool TryTriggerSoulSuppressTalisman(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int damage = item.level >= 2 ? 16 : 10;
            if (HasEnemyWeakness(CounterTag.Ghost) || HasEnemyWeakness(CounterTag.StealEnergy))
            {
                damage = Mathf.RoundToInt(damage * 1.4f);
                counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.SoulSuppress, GetEnemyPosition());
            }

            DealDamage(item, damage, $"{GetRuntimeItemName(item)}镇魂压制，造成 {damage} 点魂系伤害");
            return true;
        }

        private bool TryTriggerFireTalisman(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int damage = item.level >= 2 ? 20 : 12;
            if (HasActiveAdjacentItemId(item.gridPosition, SpiritStoneId))
            {
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "火灵连发：火符触发加速", item.definition.itemId, value: 0, screenPosition: GetItemPosition(item));
            }

            TalismanItemRuntime adjacentSeal = FindAdjacentItem(item.gridPosition, SealId);
            if (adjacentSeal != null)
            {
                damage += adjacentSeal.level >= 2 ? 6 : 3;
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "法印强化相邻符箓", item.definition.itemId, value: 0, screenPosition: GetItemPosition(item));
            }

            DealDamage(item, damage, $"{GetRuntimeItemName(item)}释放火球，造成 {damage} 点伤害");
            ApplyFireBurnReward(item);
            return true;
        }

        private bool TryTriggerShieldTalisman(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int shieldGain = item.level >= 2 ? 28 : 18;
            float shieldMultiplier = 1f + GetShieldRewardMultiplier(item);
            if (shieldMultiplier > 1f)
            {
                shieldGain = Mathf.RoundToInt(shieldGain * shieldMultiplier);
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "[强化] 护身符护盾提高", item.definition.itemId, value: shieldGain, screenPosition: GetItemPosition(item));
            }

            playerStats.AddShield(shieldGain, shieldCap);
            Emit(BattleEventType.ItemTriggered, BattleLogCategory.Normal, "", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
            Emit(BattleEventType.ShieldGained, BattleLogCategory.Defense, $"{GetRuntimeItemName(item)}触发，获得 {shieldGain} 点护盾", item.definition.itemId, value: shieldGain, screenPosition: GetPlayerPosition());
            combatUI?.ShowShieldFeedback();
            RefreshUI();
            return true;
        }

        private bool TryTriggerQiPill(TalismanItemRuntime item)
        {
            if (playerStats.hp >= 50 || !SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            bool boosted = HasActiveAdjacentItemId(item.gridPosition, ShieldTalismanId);
            int healAmount = item.level >= 2 ? boosted ? 40 : 32 : boosted ? 28 : 20;
            if (boosted)
            {
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "护丹：回气丹治疗提高", item.definition.itemId, value: 0, screenPosition: GetItemPosition(item));
            }

            if (HasActiveAdjacentItemId(item.gridPosition, WaterTalismanId))
            {
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "水丹回气：回气丹冷却缩短", item.definition.itemId, value: 0, screenPosition: GetItemPosition(item));
            }

            playerStats.Heal(healAmount);
            Emit(BattleEventType.ItemTriggered, BattleLogCategory.Normal, "", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
            Emit(BattleEventType.HealReceived, BattleLogCategory.Heal, $"{GetRuntimeItemName(item)}触发，恢复 {healAmount} 点气血", item.definition.itemId, value: healAmount, screenPosition: GetPlayerPosition());
            combatUI?.FlashHP();
            RefreshUI();
            return true;
        }

        private bool TryTriggerThunderTalisman(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int damage = item.level >= 2 ? 28 : 18;
            if (IsGhostFamily())
            {
                damage = Mathf.RoundToInt(damage * 1.5f);
            }

            TalismanItemRuntime adjacentSeal = FindAdjacentItem(item.gridPosition, SealId);
            float critChance = adjacentSeal != null && adjacentSeal.level >= 2 ? 0.45f : 0.3f;
            if (adjacentSeal != null && Random.value < critChance)
            {
                damage *= 2;
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "雷印共鸣，雷符暴击", item.definition.itemId, value: damage, screenPosition: GetItemPosition(item));
            }

            if ((currentEnemy.definition.enemyType == EnemyType.SwordCultivator || IsBoss()) && currentEnemy.isCharging)
            {
                InterruptCurrentCharge(item);
            }

            DealDamage(item, damage, $"{GetRuntimeItemName(item)}轰击，造成 {damage} 点雷伤害");
            return true;
        }

        private bool TryTriggerSwordPill(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int damage = item.level >= 2 ? 14 : 8;
            if (HasActiveAdjacentItemId(item.gridPosition, FireTalismanId))
            {
                damage += item.level >= 2 ? 8 : 5;
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "火剑流：小剑丸附带火焰伤害", item.definition.itemId, value: damage, screenPosition: GetItemPosition(item));
            }

            if (currentEnemy.definition.enemyType == EnemyType.SwordCultivator && HasActiveAdjacentItemId(item.gridPosition, FireTalismanId))
            {
                damage = Mathf.RoundToInt(damage * 1.3f);
            }

            if (v02RunModifierState != null && v02RunModifierState.swordCritChanceBonus > 0f && Random.value < v02RunModifierState.swordCritChanceBonus)
            {
                damage = Mathf.RoundToInt(damage * 1.8f);
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "[强化] 剑丸暴击！", item.definition.itemId, value: damage, screenPosition: GetItemPosition(item));
            }

            DealDamage(item, damage, $"{GetRuntimeItemName(item)}飞出攻击，造成 {damage} 点伤害");
            return true;
        }

        private bool TryTriggerExorcismBell(TalismanItemRuntime item)
        {
            if (!SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int damage = item.level >= 2 ? 20 : 12;
            if (currentEnemy.definition.enemyType == EnemyType.SwordCultivator)
            {
                damage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.5f));
            }

            DealDamage(item, damage, $"{GetRuntimeItemName(item)}震响，造成 {damage} 点驱邪伤害");
            return true;
        }

        private bool TryTriggerWaterTalisman(TalismanItemRuntime item)
        {
            if (playerStats.hp >= playerStats.maxHP || !SpendMana(item, GetManaCost(item)))
            {
                return false;
            }

            int heal = item.level >= 2 ? 18 : 10;
            playerStats.Heal(heal);
            if (HasActiveAdjacentItemId(item.gridPosition, QiPillId))
            {
                Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "水丹回气：回气丹冷却缩短", item.definition.itemId, value: 0, screenPosition: GetItemPosition(item));
            }

            Emit(BattleEventType.ItemTriggered, BattleLogCategory.Normal, "", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
            Emit(BattleEventType.HealReceived, BattleLogCategory.Heal, $"{GetRuntimeItemName(item)}流转，恢复 {heal} 点气血", item.definition.itemId, value: heal, screenPosition: GetPlayerPosition());
            combatUI?.FlashHP();
            RefreshUI();
            return true;
        }

        private void DealDamage(TalismanItemRuntime source, int amount, string logMessage)
        {
            int finalDamage = ApplyDamageModifiers(source, amount);
            bool shieldCountered = false;
            bool groupCountered = false;

            if (counterMatchResolver != null && counterMatchResolver.TryResolveShieldBreak(source, currentEnemy, finalDamage, out int shieldBreakDamage))
            {
                finalDamage = shieldBreakDamage;
                if (v02RunModifierState != null &&
                    v02RunModifierState.thunderShieldBreakMultiplierBonus > 0f &&
                    IsThunderRewardSource(source) &&
                    currentEnemy.currentShield > 0)
                {
                    float baseMultiplier = v02CounterMultiplierConfig != null
                        ? v02CounterMultiplierConfig.rewardShieldBreakMultiplier
                        : 2f;
                    float boostedMultiplier = baseMultiplier + v02RunModifierState.thunderShieldBreakMultiplierBonus;
                    finalDamage = Mathf.Max(finalDamage, Mathf.RoundToInt(amount * boostedMultiplier));
                    Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, "[强化] 雷符破盾强化生效", source.definition.itemId, value: finalDamage, screenPosition: GetEnemyPosition());
                }

                shieldCountered = true;
                counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.ShieldBreak, GetEnemyPosition());
            }

            if (counterMatchResolver != null && counterMatchResolver.TryResolveGroupCounter(source, currentEnemy))
            {
                float groupMultiplier = v02CounterMultiplierConfig != null ? v02CounterMultiplierConfig.groupClearMultiplier : 1.6f;
                finalDamage = Mathf.RoundToInt(finalDamage * groupMultiplier);
                groupCountered = true;
                counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.ChainClear, GetEnemyPosition());
            }

            if (!shieldCountered && !groupCountered)
            {
                finalDamage = ApplyV02CounterRelationMultiplier(source, finalDamage);
            }

            int shieldBlocked = 0;
            int shieldBefore = currentEnemy.currentShield;
            if (currentEnemy.currentShield > 0)
            {
                shieldBlocked = Mathf.Min(currentEnemy.currentShield, finalDamage);
                currentEnemy.currentShield -= shieldBlocked;
                finalDamage -= shieldBlocked;
            }

            if (finalDamage > 0)
            {
                currentEnemy.currentHp = Mathf.Max(0, currentEnemy.currentHp - finalDamage);
            }

            Emit(BattleEventType.ItemTriggered, BattleLogCategory.Normal, "", source.definition.itemId, value: 1, screenPosition: GetItemPosition(source));
            if (shieldCountered)
            {
                Emit(BattleEventType.EnemyCountered, BattleLogCategory.Counter, $"[克制] {GetRuntimeItemName(source)}触发破盾，护盾承受额外伤害", source.definition.itemId, currentEnemy.definition.enemyId, shieldBlocked, GetEnemyPosition());
            }

            if (groupCountered)
            {
                Emit(BattleEventType.EnemyCountered, BattleLogCategory.Counter, $"[克制] {GetRuntimeItemName(source)}对群体敌人触发连锁清场", source.definition.itemId, currentEnemy.definition.enemyId, finalDamage, GetEnemyPosition());
            }

            if (shieldBlocked > 0)
            {
                Emit(BattleEventType.EnemyCountered, BattleLogCategory.Defense, $"[战斗] {GetRuntimeItemName(source)} 命中护盾，抵消 {shieldBlocked} 点伤害", source.definition.itemId, currentEnemy.definition.enemyId, shieldBlocked, GetEnemyPosition());
                if (!shieldCountered && shieldBefore > 0 && v02FailureTracker != null)
                {
                    v02FailureTracker.shieldNotBrokenCount++;
                }
            }

            string damageMessage = shieldBlocked > 0
                ? $"{logMessage}（护盾后 {finalDamage}）"
                : shieldCountered || groupCountered ? $"{logMessage}（克制后 {finalDamage}）" : logMessage;
            Emit(BattleEventType.DamageDealt, BattleLogCategory.Damage, damageMessage, source.definition.itemId, currentEnemy.definition.enemyId, finalDamage, GetEnemyPosition());
            RefreshUI();
            if (v02BossPhaseController == null)
            {
                TriggerBossEnrage();
            }
            CheckVictory();
        }

        private int ApplyV02CounterRelationMultiplier(TalismanItemRuntime source, int damage)
        {
            if (v02CounterMultiplierConfig == null || currentEnemy?.definition == null || source?.definition == null || damage <= 0)
            {
                return damage;
            }

            CounterRelation relation = V02CounterBalanceUtility.ResolveItemRelation(source, currentEnemy.definition, IsFormationItemActive(source));
            float multiplier = v02CounterMultiplierConfig.GetMultiplier(relation);
            if (Mathf.Approximately(multiplier, 1f))
            {
                return damage;
            }

            int adjusted = Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
            if (relation == CounterRelation.StrongCounter || relation == CounterRelation.LightCounter)
            {
                Emit(BattleEventType.EnemyCountered, BattleLogCategory.Counter, $"[\u514b\u5236] {GetRuntimeItemName(source)}\u547d\u4e2d\u5f31\u70b9\uff0c\u4f24\u5bb3 x{multiplier:0.##}", source.definition.itemId, currentEnemy.definition.enemyId, adjusted, GetEnemyPosition());
            }
            else if (relation == CounterRelation.Resisted || relation == CounterRelation.HardResisted)
            {
                Emit(BattleEventType.LogMessage, BattleLogCategory.Danger, $"[抗性] {currentEnemy.definition.GetReadableLabel()}抵抗{GetRuntimeItemName(source)}，伤害 x{multiplier:0.##}", source.definition.itemId, currentEnemy.definition.enemyId, adjusted, GetEnemyPosition());
            }

            return adjusted;
        }

        private int ApplyDamageModifiers(TalismanItemRuntime source, int damage)
        {
            if (IsGhostFamily() && comboResolver != null && comboResolver.IsComboActive(ComboResolver.ExorcismArray))
            {
                bool hasLv2Peach = HasPlacedItemLevel(PeachWoodId, 2);
                damage = Mathf.RoundToInt(damage * (hasLv2Peach ? 1.45f : 1.3f));
                Emit(BattleEventType.EnemyCountered, BattleLogCategory.Counter, "驱邪阵压制鬼怪", value: damage, screenPosition: GetEnemyPosition());
            }
            else if (IsGhostFamily() && HasPlacedItem(PeachWoodId))
            {
                damage = Mathf.RoundToInt(damage * 1.1f);
                Emit(BattleEventType.EnemyCountered, BattleLogCategory.Counter, "桃木牌压制鬼怪", value: damage, screenPosition: GetEnemyPosition());
            }

            return Mathf.Max(1, damage);
        }

        private bool SpendMana(TalismanItemRuntime item, int manaCost)
        {
            if (playerStats.mana < manaCost)
            {
                return false;
            }

            playerStats.mana -= manaCost;
            Emit(BattleEventType.ManaSpent, BattleLogCategory.Mana, $"{GetRuntimeItemName(item)}消耗 {manaCost} 点灵气", item.definition.itemId, value: manaCost, screenPosition: GetManaPosition());
            return true;
        }

        private void UpdateEnemy(float deltaTime)
        {
            if (HasV02EnemyData())
            {
                UpdateBasicAttack(deltaTime);
                if (IsBoss() && v02BossPhaseController != null)
                {
                    v02BossPhaseController.Tick(currentEnemy, deltaTime);
                    return;
                }

                enemySkillController?.Tick(deltaTime);
                return;
            }

            UpdateEnemyBehavior(deltaTime);
        }

        private void UpdateEnemyBehavior(float deltaTime)
        {
            if (currentEnemy?.definition == null)
            {
                return;
            }

            switch (currentEnemy.definition.enemyType)
            {
                case EnemyType.Ghost:
                    UpdateBasicAttack(deltaTime);
                    break;
                case EnemyType.GhostSwarm:
                    UpdateBasicAttack(deltaTime);
                    UpdateGhostSwarmSpecial(deltaTime);
                    break;
                case EnemyType.SwordCultivator:
                    UpdateSwordCultivatorBehavior(deltaTime);
                    break;
                case EnemyType.EvilCultivator:
                    UpdateBasicAttack(deltaTime);
                    UpdateEvilCultivatorSpecial(deltaTime);
                    break;
                case EnemyType.Boss:
                    UpdateBossBehavior(deltaTime);
                    break;
                default:
                    UpdateBasicAttack(deltaTime);
                    break;
            }
        }

        private void UpdateBasicAttack(float deltaTime)
        {
            if (currentEnemy.isCharging && (currentEnemy.definition.enemyType == EnemyType.SwordCultivator || IsBoss()))
            {
                return;
            }

            currentEnemy.attackTimer -= deltaTime;
            if (currentEnemy.attackTimer > 0f)
            {
                return;
            }

            currentEnemy.attackTimer += GetEnemyAttackInterval();
            DealDamageToPlayer(currentEnemy.definition.attackDamage, $"{currentEnemy.definition.GetReadableLabel()}攻击，造成 {currentEnemy.definition.attackDamage} 点伤害");
        }

        private void UpdateSwordCultivatorBehavior(float deltaTime)
        {
            if (currentEnemy.isCharging)
            {
                currentEnemy.chargeTimer -= deltaTime;
                if (currentEnemy.chargeTimer <= 0f)
                {
                    currentEnemy.isCharging = false;
                    currentEnemy.specialTimer = GetChargeInterval();
                    int damage = GetChargeDamage();
                    DealDamageToPlayer(damage, $"{currentEnemy.definition.GetReadableLabel()}释放连斩，造成 {damage} 点伤害", BattleLogCategory.Danger);
                }
                else
                {
                    EmitChargeProgress();
                }

                return;
            }

            currentEnemy.specialTimer -= deltaTime;
            if (currentEnemy.specialTimer <= 0f)
            {
                currentEnemy.isCharging = true;
                currentEnemy.chargeTimer = GetChargeDuration();
                Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, $"{currentEnemy.definition.GetReadableLabel()}开始蓄力连斩", value: 1, screenPosition: GetEnemyPosition());
            }
        }

        private void UpdateEvilCultivatorSpecial(float deltaTime)
        {
            UpdateEvilCultivatorSpecial(deltaTime, currentEnemy.definition.GetReadableLabel());
        }

        private void UpdateEvilCultivatorSpecial(float deltaTime, string sourceName)
        {
            currentEnemy.specialTimer -= deltaTime;
            if (currentEnemy.specialTimer <= 0f)
            {
                currentEnemy.specialTimer += GetManaDrainInterval();
                DrainMana(sourceName, GetManaDrainAmount());
            }

            currentEnemy.sealTimer -= deltaTime;
            if (currentEnemy.sealTimer <= 0f)
            {
                currentEnemy.sealTimer += GetSealInterval();
                SealRandomItem();
            }
        }

        private void UpdateGhostSwarmSpecial(float deltaTime)
        {
            currentEnemy.specialTimer -= deltaTime;
            if (currentEnemy.specialTimer <= 0f)
            {
                currentEnemy.specialTimer += GetGhostShadowInterval();
                int damage = GetGhostShadowDamage();
                DealDamageToPlayer(damage, $"{currentEnemy.definition.GetReadableLabel()}发动鬼影攻击，造成 {damage} 点伤害", BattleLogCategory.Danger);
            }
        }

        private void UpdateBossBehavior(float deltaTime)
        {
            TriggerBossEnrage();
            UpdateBasicAttack(deltaTime);
            UpdateBossManaDrain(deltaTime);
            UpdateBossSeal(deltaTime);
            UpdateBossCharge(deltaTime);
        }

        private void UpdateBossManaDrain(float deltaTime)
        {
            currentEnemy.manaDrainTimer -= deltaTime;
            if (currentEnemy.manaDrainTimer > 0f)
            {
                return;
            }

            currentEnemy.manaDrainTimer += GetManaDrainInterval();
            DrainMana(currentEnemy.definition.GetReadableLabel(), GetManaDrainAmount());
        }

        private void UpdateBossSeal(float deltaTime)
        {
            currentEnemy.sealTimer -= deltaTime;
            if (currentEnemy.sealTimer > 0f)
            {
                return;
            }

            currentEnemy.sealTimer += GetSealInterval();
            SealRandomItem();
        }

        private void UpdateBossCharge(float deltaTime)
        {
            if (currentEnemy.isCharging)
            {
                currentEnemy.chargeTimer -= deltaTime;
                if (currentEnemy.chargeTimer <= 0f)
                {
                    ResolveBossCharge();
                }
                else
                {
                    EmitChargeProgress();
                }

                return;
            }

            currentEnemy.chargeIntervalTimer -= deltaTime;
            currentEnemy.bossChargeCooldown = currentEnemy.chargeIntervalTimer;
            if (currentEnemy.chargeIntervalTimer <= 0f)
            {
                StartBossCharge();
            }
        }

        private void StartBossCharge()
        {
            if (!IsBoss() || currentEnemy == null)
            {
                return;
            }

            currentEnemy.isCharging = true;
            currentEnemy.chargeTimer = GetChargeDuration();
            currentEnemy.chargeIntervalTimer = GetChargeInterval();
            currentEnemy.bossChargeCooldown = currentEnemy.chargeIntervalTimer;
            statsTracker?.RecordBossChargeStarted();
            Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, $"{currentEnemy.definition.GetReadableLabel()}开始蓄力心魔冲击", currentEnemy.definition.enemyId, value: 1, screenPosition: GetEnemyPosition());
        }

        private void ResolveBossCharge()
        {
            if (!IsBoss() || currentEnemy == null)
            {
                return;
            }

            currentEnemy.isCharging = false;
            currentEnemy.chargeTimer = 0f;
            currentEnemy.chargeIntervalTimer = GetChargeInterval();
            currentEnemy.bossChargeCooldown = currentEnemy.chargeIntervalTimer;
            int damage = GetChargeDamage();
            playerStats.shield = 0;
            statsTracker?.RecordBossChargeHit();
            DealDamageToPlayer(damage, $"心魔冲击命中，清空护盾并造成 {damage} 点伤害", BattleLogCategory.Danger);
        }

        private void TriggerBossEnrage(bool force = false)
        {
            if (!IsBoss() || currentEnemy == null || currentEnemy.definition == null)
            {
                return;
            }

            if (currentEnemy.hasTriggeredEnrage)
            {
                return;
            }

            bool shouldEnrage = currentEnemy.currentHp > 0 && (force || currentEnemy.currentHp <= Mathf.CeilToInt(currentEnemy.definition.maxHp * 0.5f));
            if (!shouldEnrage)
            {
                return;
            }

            currentEnemy.isEnraged = true;
            currentEnemy.hasTriggeredEnrage = true;
            currentEnemy.attackTimer = Mathf.Min(currentEnemy.attackTimer, GetEnemyAttackInterval());
            currentEnemy.sealTimer = Mathf.Min(currentEnemy.sealTimer, GetSealInterval());
            statsTracker?.RecordBossEnrage();
            Emit(BattleEventType.EnemyEnraged, BattleLogCategory.Danger, $"{currentEnemy.definition.GetReadableLabel()}进入狂暴状态，攻势加快", currentEnemy.definition.enemyId, value: 1, screenPosition: GetEnemyPosition());
        }

        private void DrainMana(string sourceName, int amount)
        {
            int drained = Mathf.Min(playerStats.mana, amount);
            playerStats.mana -= drained;
            if (IsBoss())
            {
                statsTracker?.RecordBossManaDrain();
            }

            Emit(BattleEventType.ManaSpent, BattleLogCategory.Danger, $"{sourceName}吸走了 {drained} 点灵气", currentEnemy.definition.enemyId, value: drained, screenPosition: GetManaPosition());
        }

        private void InterruptCurrentCharge(TalismanItemRuntime item)
        {
            currentEnemy.isCharging = false;
            currentEnemy.chargeTimer = 0f;
            if (IsBoss())
            {
                currentEnemy.chargeIntervalTimer = GetChargeInterval();
                currentEnemy.bossChargeCooldown = currentEnemy.chargeIntervalTimer;
                statsTracker?.RecordBossChargeInterrupted();
                Emit(BattleEventType.EnemyInterrupted, BattleLogCategory.Counter, "雷符打断了心魔冲击", item.definition.itemId, value: 0, screenPosition: GetEnemyPosition());
            }
            else
            {
                currentEnemy.specialTimer = GetChargeInterval();
                Emit(BattleEventType.EnemyInterrupted, BattleLogCategory.Counter, "雷符打断了剑修连斩", item.definition.itemId, value: 0, screenPosition: GetEnemyPosition());
            }
        }

        private void DealDamageToPlayer(int amount, string logMessage, BattleLogCategory category = BattleLogCategory.Damage, string sourceOverride = null, Vector2 screenPositionOverride = default)
        {
            int hpDamage = playerStats.TakeDamage(amount);
            int blocked = Mathf.Max(0, amount - hpDamage);
            if (blocked > 0 && IsEnemyChargePressure())
            {
                counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.GuardReduce, GetPlayerPosition());
            }

            string sourceId = string.IsNullOrWhiteSpace(sourceOverride) ? currentEnemy.definition.enemyId : sourceOverride;
            Vector2 position = screenPositionOverride == Vector2.zero ? GetPlayerPosition() : screenPositionOverride;
            Emit(BattleEventType.DamageTaken, category, logMessage, sourceId, "player", amount, position);
            combatUI?.FlashHP();
            RefreshUI();
            CheckDefeat();
        }

        public void V02DealDamageToPlayer(int amount, string logMessage)
        {
            if (currentEnemy?.definition == null || amount <= 0)
            {
                return;
            }

            DealDamageToPlayer(amount, logMessage, BattleLogCategory.Danger);
        }

        public void V02AddEnemyShield(int amount, string logMessage)
        {
            if (currentEnemy == null || amount <= 0)
            {
                return;
            }

            currentEnemy.currentShield += amount;
            Emit(BattleEventType.ShieldGained, BattleLogCategory.Defense, logMessage, currentEnemy.definition.enemyId, value: amount, screenPosition: GetEnemyPosition());
            combatUI?.ShowShieldFeedback();
            RefreshUI();
        }

        public void V02AddPlayerStatus(int poison, int burn, string logMessage)
        {
            if (poison <= 0 && burn <= 0)
            {
                return;
            }

            EnsureStatusEffectSystem();
            string sourceId = currentEnemy?.definition != null ? currentEnemy.definition.GetReadableLabel() : "敌人";
            if (poison > 0)
            {
                playerStatusController?.AddStatus(StatusEffectLibrary.Poison, sourceId, "Enemy", poison);
            }

            if (burn > 0)
            {
                playerStatusController?.AddStatus(StatusEffectLibrary.Burn, sourceId, "Enemy", burn);
            }

            Emit(BattleEventType.LogMessage, BattleLogCategory.Danger, logMessage, currentEnemy?.definition != null ? currentEnemy.definition.enemyId : string.Empty, value: poison + burn, screenPosition: GetPlayerPosition());
            playerStatusAnchor?.Refresh();
            playerDebuffAnchor?.Refresh();
        }

        public bool V02TryCounterStealEnergy(EnemySkillDefinition skill)
        {
            if (counterMatchResolver == null || skill == null)
            {
                return false;
            }

            List<TalismanItemRuntime> candidates = GetPlacedPoweredCounterCandidates();
            if (!counterMatchResolver.TryResolveStealEnergyCounter(skill, candidates))
            {
                return false;
            }

            if (v02FailureTracker != null)
            {
                v02FailureTracker.stealEnergyCounteredCount++;
            }

            v02RunStatsTracker?.RecordSoulSuppressCounter();
            playerStatusController?.AddStatus(StatusEffectLibrary.SoulSuppressTriggered, "镇魂符", "Item", 1, 1.5f);
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.SoulSuppress, GetEnemyPosition());
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.FormationProtected, GetManaPosition());
            Emit(BattleEventType.EnemyInterrupted, BattleLogCategory.Counter, "[克制] 镇魂符反制偷灵，阵眼保护成功", currentEnemy?.definition != null ? currentEnemy.definition.enemyId : string.Empty, value: 1, screenPosition: GetEnemyPosition());
            return true;
        }

        private List<TalismanItemRuntime> GetLegalSealTargets()
        {
            List<TalismanItemRuntime> targets = new();
            if (grid == null)
            {
                return targets;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (IsLegalSealTarget(item))
                {
                    targets.Add(item);
                }
            }

            return targets;
        }

        private bool IsLegalSealTarget(TalismanItemRuntime item)
        {
            if (item?.definition == null || item.isSealed || !item.isPlaced || grid == null)
            {
                return false;
            }

            if (grid.GetItemAt(item.gridPosition) != item)
            {
                return false;
            }

            Vector2Int eyePosition = formationPowerResolver != null
                ? formationPowerResolver.FormationEyePosition
                : FormationPowerResolver.GetDefaultEyeCorePosition(grid.Width, grid.Height);
            return item.gridPosition != eyePosition;
        }

        private bool ApplySealToItem(TalismanItemRuntime item, float duration)
        {
            if (!IsLegalSealTarget(item))
            {
                return false;
            }

            item.isSealed = true;
            item.sealRemaining = duration > 0f ? duration : GetSealDuration();
            if (!sealedItems.Contains(item))
            {
                sealedItems.Add(item);
            }

            return true;
        }

        private static void ClearSeal(TalismanItemRuntime item)
        {
            if (item == null)
            {
                return;
            }

            item.isSealed = false;
            item.sealRemaining = 0f;
        }

        private void RefreshAfterSealStateChanged()
        {
            RefreshFormationPowerStates();
            RefreshSealHighlights();
            SyncPlayerFormationStatusEffects();
            RefreshUI();
        }

        public void V02ApplyEnergyDisruption(float duration, string logMessage)
        {
            if (grid == null)
            {
                return;
            }

            if (duration <= 0f)
            {
                duration = formationPowerResolver != null ? formationPowerResolver.DefaultStealEnergyDisableDuration : 3f;
            }

            if (duration <= 0f)
            {
                return;
            }

            Vector2Int source = formationPowerResolver != null ? formationPowerResolver.FormationEyePosition : FormationPowerResolver.GetDefaultEyeCorePosition(grid.Width, grid.Height);
            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition != null && item.definition.itemId == SpiritStoneId)
                {
                    source = item.gridPosition;
                    break;
                }
            }

            int affected = 0;
            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition == null || item.definition.itemId == SpiritStoneId)
                {
                    continue;
                }

                if (Mathf.Abs(item.gridPosition.x - source.x) <= 1 && Mathf.Abs(item.gridPosition.y - source.y) <= 1)
                {
                    item.isTemporarilyDisabled = true;
                    item.temporaryDisabledRemaining = duration;
                    affected++;
                }
            }

            if (affected > 0 && v02FailureTracker != null)
            {
                v02FailureTracker.stealEnergyHitCount++;
            }

            Emit(BattleEventType.LogMessage, BattleLogCategory.Danger, $"{logMessage}（影响 {affected} 个符箓）", currentEnemy?.definition != null ? currentEnemy.definition.enemyId : string.Empty, value: affected, screenPosition: GetEnemyPosition());
            SyncPlayerFormationStatusEffects();
            RefreshSealHighlights();
            RefreshUI();
        }

        public void V02SealRandomRowOrColumn(float duration, string logMessage)
        {
            if (grid == null)
            {
                return;
            }

            List<TalismanItemRuntime> candidates = GetLegalSealTargets();
            if (candidates.Count == 0)
            {
                Emit(BattleEventType.ItemSealed, BattleLogCategory.Seal, $"{logMessage}\uff08\u6ca1\u6709\u53ef\u5c01\u5370\u76ee\u6807\uff09", currentEnemy?.definition != null ? currentEnemy.definition.enemyId : string.Empty, value: 0, screenPosition: GetEnemyPosition());
                RefreshAfterSealStateChanged();
                return;
            }

            bool sealColumn = Random.value > 0.5f;
            TalismanItemRuntime anchor = candidates[Random.Range(0, candidates.Count)];
            int index = sealColumn ? anchor.gridPosition.x : anchor.gridPosition.y;
            int sealedCount = 0;
            foreach (TalismanItemRuntime item in candidates)
            {
                bool match = sealColumn ? item.gridPosition.x == index : item.gridPosition.y == index;
                if (!match)
                {
                    continue;
                }

                if (ApplySealToItem(item, duration))
                {
                    sealedCount++;
                }
            }

            if (sealedCount > 0 && v02FailureTracker != null)
            {
                v02FailureTracker.sealHitCount += sealedCount;
            }

            Emit(BattleEventType.ItemSealed, BattleLogCategory.Seal, $"{logMessage}（{(sealColumn ? "列" : "行")} {index + 1}，封印 {sealedCount} 个）", currentEnemy?.definition != null ? currentEnemy.definition.enemyId : string.Empty, value: sealedCount, screenPosition: GetEnemyPosition());
            RefreshAfterSealStateChanged();
        }

        public int V02SealFormationEyeArea(float duration, string logMessage)
        {
            if (grid == null)
            {
                return 0;
            }

            TalismanItemRuntime target = FindFormationEyeSealTarget();
            if (!IsLegalSealTarget(target))
            {
                Emit(BattleEventType.ItemSealed, BattleLogCategory.Seal, $"{logMessage}\uff08\u6ca1\u6709\u53ef\u5c01\u5370\u76ee\u6807\uff09", currentEnemy?.definition != null ? currentEnemy.definition.enemyId : string.Empty, value: 0, screenPosition: GetEnemyPosition());
                RefreshAfterSealStateChanged();
                return 0;
            }

            ApplySealToItem(target, duration);

            if (v02FailureTracker != null)
            {
                v02FailureTracker.sealHitCount++;
            }

            Emit(BattleEventType.ItemSealed, BattleLogCategory.Seal, $"{logMessage}\uff1a{target.definition.displayName}", currentEnemy?.definition != null ? currentEnemy.definition.enemyId : string.Empty, target.definition.itemId, 1, GetItemPosition(target));
            RefreshAfterSealStateChanged();
            return 1;
        }

        public void V02ClearPlayerStatuses()
        {
            playerStatusController?.RemoveStatus(StatusEffectIds.Poison);
            playerStatusController?.RemoveStatus(StatusEffectIds.Burn);
            v02StatusTickTimer = 1f;
            playerStatusAnchor?.Refresh();
            playerBuffAnchor?.Refresh();
            playerDebuffAnchor?.Refresh();
            AddLog("已清除玩家中毒和燃烧");
        }

        public void V02ClearTemporaryDisables()
        {
            V02ClearTemporaryDisablesWithoutLog();
            AddLog("已清除临时失效");
        }

        private void V02ClearTemporaryDisablesWithoutLog()
        {
            if (grid == null)
            {
                return;
            }

            bool changed = false;
            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item != null)
                {
                    changed |= item.isTemporarilyDisabled || item.temporaryDisabledRemaining > 0f;
                    item.isTemporarilyDisabled = false;
                    item.temporaryDisabledRemaining = 0f;
                }
            }

            if (changed)
            {
                RefreshAfterSealStateChanged();
            }
            else
            {
                RefreshSealHighlights();
                SyncPlayerFormationStatusEffects();
            }
        }

        public void ForceV02EnemySkill()
        {
            enemySkillController?.ForceFirstSkill();
        }

        public void V02DebugSetEnemyShield(int amount)
        {
            if (currentEnemy == null)
            {
                return;
            }

            currentEnemy.currentShield = Mathf.Max(0, amount);
            Emit(BattleEventType.ShieldGained, BattleLogCategory.Defense, $"[调试] 敌人护盾设为 {currentEnemy.currentShield}", currentEnemy.definition.enemyId, value: currentEnemy.currentShield, screenPosition: GetEnemyPosition());
            RefreshUI();
        }

        public void V02DebugForceWin()
        {
            if (currentEnemy == null)
            {
                return;
            }

            if (state != TalismanCombatState.Fighting)
            {
                state = TalismanCombatState.Fighting;
            }

            currentEnemy.currentHp = 0;
            CheckVictory();
        }

        public void V02DebugForceLose()
        {
            if (state != TalismanCombatState.Fighting)
            {
                state = TalismanCombatState.Fighting;
            }

            playerStats.hp = 0;
            CheckDefeat();
        }

        public void V02DebugApplyPlayerStatus(int poison, int burn)
        {
            V02AddPlayerStatus(poison, burn, $"[调试] 施加中毒 {poison} / 燃烧 {burn}");
        }

        public bool V02DebugSealFirstPlacedItem(float duration)
        {
            if (grid == null)
            {
                return false;
            }

            foreach (TalismanItemRuntime item in GetLegalSealTargets())
            {
                if (item.definition.itemId == "purify_talisman_basic")
                {
                    continue;
                }

                if (!ApplySealToItem(item, duration))
                {
                    continue;
                }

                if (v02FailureTracker != null)
                {
                    v02FailureTracker.sealHitCount++;
                }

                Emit(BattleEventType.ItemSealed, BattleLogCategory.Seal, $"[调试] 封印 {item.definition.displayName}", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
                RefreshAfterSealStateChanged();
                return true;
            }

            AddLog("[调试] 没有可封印的已摆放符箓");
            return false;
        }

        public bool V02DebugTriggerItem(string itemId)
        {
            TalismanItemRuntime item = FindPlacedItemById(itemId);
            if (item == null)
            {
                AddLog($"[调试] 阵盘中没有可触发道具：{itemId}");
                return false;
            }

            playerStats.mana = playerStats.maxMana;
            item.cooldownTimer = 0f;
            bool triggered = TryTriggerItem(item);
            if (triggered)
            {
                item.triggerCount++;
                FlashItem(item);
            }

            RefreshUI();
            return triggered;
        }

        public void V02DebugEmitCounterLogPriority()
        {
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.ShieldBreak, GetEnemyPosition());
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.Cleanse, GetPlayerPosition());
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.Unseal, GetItemPosition(FindPlacedItemById("purify_talisman_basic")));
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.SoulSuppress, GetEnemyPosition());
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.FormationProtected, GetManaPosition());
            counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.ChainClear, GetEnemyPosition());
        }

        public void V02DebugResolveStealEnergyCounter()
        {
            EnemySkillDefinition stealSkill = null;
            if (currentEnemy?.definition?.skills != null)
            {
                foreach (EnemySkillDefinition skill in currentEnemy.definition.skills)
                {
                    if (skill != null && skill.skillType == EnemySkillType.StealEnergy)
                    {
                        stealSkill = skill;
                        break;
                    }
                }
            }

            if (stealSkill == null)
            {
                AddLog("[调试] 当前敌人没有偷灵技能");
                return;
            }

            if (!V02TryCounterStealEnergy(stealSkill))
            {
                counterFeedbackController?.ShowCounterFeedbackAtScreen(CounterFeedbackType.CounterFailed, GetEnemyPosition());
            }
        }

        private void SealRandomItem()
        {
            if (grid == null)
            {
                return;
            }

            List<TalismanItemRuntime> candidates = GetLegalSealTargets();
            if (candidates.Count == 0)
            {
                return;
            }

            TalismanItemRuntime target = candidates[Random.Range(0, candidates.Count)];
            if (!ApplySealToItem(target, GetSealDuration()))
            {
                return;
            }

            if (IsBoss())
            {
                statsTracker?.RecordBossSeal();
            }

            Emit(BattleEventType.ItemSealed, BattleLogCategory.Seal, $"{currentEnemy.definition.GetReadableLabel()}封印了{target.definition.displayName}", currentEnemy.definition.enemyId, target.definition.itemId, 0, GetItemPosition(target));
            RefreshAfterSealStateChanged();
        }

        private void UpdateSeals(float deltaTime)
        {
            bool changed = false;
            for (int i = sealedItems.Count - 1; i >= 0; i--)
            {
                TalismanItemRuntime item = sealedItems[i];
                if (item == null)
                {
                    sealedItems.RemoveAt(i);
                    changed = true;
                    continue;
                }

                item.sealRemaining -= deltaTime;
                if (item.sealRemaining <= 0f)
                {
                    ClearSeal(item);
                    sealedItems.RemoveAt(i);
                    changed = true;
                    Emit(BattleEventType.ItemUnsealed, BattleLogCategory.Seal, $"{item.definition.displayName}的封印解除", item.definition.itemId, value: 0, screenPosition: GetItemPosition(item));
                }
            }

            if (changed)
            {
                RefreshAfterSealStateChanged();
            }
            else
            {
                RefreshSealHighlights();
                SyncPlayerFormationStatusEffects();
            }
        }

        private void UpdateV02TemporaryDisables(float deltaTime)
        {
            if (grid == null)
            {
                return;
            }

            bool changed = false;
            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item == null || !item.isTemporarilyDisabled)
                {
                    continue;
                }

                item.temporaryDisabledRemaining -= deltaTime;
                if (item.temporaryDisabledRemaining <= 0f)
                {
                    item.isTemporarilyDisabled = false;
                    item.temporaryDisabledRemaining = 0f;
                    changed = true;
                }
            }

            if (changed)
            {
                RefreshGridDependentUI();
                SyncPlayerFormationStatusEffects();
                RefreshUI();
            }
        }

        private void UpdateV02PlayerStatusDamage(float deltaTime)
        {
            int poisonStackCount = playerStatusController != null ? playerStatusController.GetStackCount(StatusEffectIds.Poison) : 0;
            int burnStackCount = playerStatusController != null ? playerStatusController.GetStackCount(StatusEffectIds.Burn) : 0;
            int statusDamage = poisonStackCount + burnStackCount;
            if (statusDamage <= 0 || currentEnemy?.definition == null)
            {
                v02StatusTickTimer = 1f;
                return;
            }

            v02StatusTickTimer -= deltaTime;
            if (v02StatusTickTimer > 0f)
            {
                return;
            }

            v02StatusTickTimer += 1f;
            if (v02FailureTracker != null)
            {
                v02FailureTracker.poisonDamageTaken += Mathf.Max(0, poisonStackCount);
                v02FailureTracker.burnDamageTaken += Mathf.Max(0, burnStackCount);
            }

            DealDamageToPlayer(
                statusDamage,
                $"[状态] 中毒 {poisonStackCount} / 燃烧 {burnStackCount}，受到 {statusDamage} 点持续伤害",
                BattleLogCategory.Danger,
                StatusEffectIds.StatusDamage,
                GetPlayerStatusPosition());
        }

        private void UpdateV02EnemyBurnDamage(float deltaTime)
        {
            int burnStackCount = enemyStatusController != null ? enemyStatusController.GetStackCount(StatusEffectIds.Burn) : 0;
            if (currentEnemy?.definition == null || burnStackCount <= 0 || state != TalismanCombatState.Fighting)
            {
                v02EnemyBurnTickTimer = 1f;
                return;
            }

            v02EnemyBurnTickTimer -= deltaTime;
            if (v02EnemyBurnTickTimer > 0f)
            {
                return;
            }

            v02EnemyBurnTickTimer += 1f;
            int damage = Mathf.Max(1, burnStackCount);
            currentEnemy.currentHp = Mathf.Max(0, currentEnemy.currentHp - damage);
            Emit(BattleEventType.DamageDealt, BattleLogCategory.Damage, $"[燃烧] {currentEnemy.definition.GetReadableLabel()}受到 {damage} 点燃烧伤害", value: damage, screenPosition: GetEnemyPosition());
            RefreshUI();
            CheckVictory();
        }

        private void CheckVictory()
        {
            if (currentEnemy.currentHp > 0 || state != TalismanCombatState.Fighting)
            {
                return;
            }

            state = TalismanCombatState.Victory;
            Emit(BattleEventType.BattleWin, BattleLogCategory.Result, $"{currentEnemy.definition.GetReadableLabel()}被击败，斗法胜利", value: 0, screenPosition: GetEnemyPosition());
            PlaytestSessionLogger.Log($"Round {currentRound} Won: {currentEnemy.definition.displayName}");
            ClearAllSeals();
            RefreshUI();
            if (v02RunFlowController != null)
            {
                v02RunFlowController.OnBattleWin();
                return;
            }

            if (currentRound >= totalRounds)
            {
                resultPanel?.Show(true, statsTracker != null ? statsTracker.CreateSnapshot() : default);
            }

            runFlowController?.OnBattleWin();
        }

        private void CheckDefeat()
        {
            if (playerStats.hp > 0 || state == TalismanCombatState.Victory || state == TalismanCombatState.RunComplete)
            {
                return;
            }

            state = TalismanCombatState.Defeat;
            Emit(BattleEventType.BattleLose, BattleLogCategory.Result, "玩家气血归零，斗法失败", value: 0, screenPosition: GetPlayerPosition());
            PlaytestSessionLogger.Log($"Round {currentRound} Lost: HP reached 0");
            ClearAllSeals();
            RefreshUI();
            if (v02RunFlowController != null)
            {
                v02RunFlowController.OnBattleLose();
                return;
            }

            resultPanel?.Show(false, statsTracker != null ? statsTracker.CreateSnapshot() : default);
            runFlowController?.OnBattleLose();
        }

        private void ResetCombatStatsOnly()
        {
            EnsureStatusEffectSystem();
            playerStats.ResetPlayer();
            playerStatusController?.ClearAll();
            enemyStatusController?.ClearAll();
            ClearAllSeals();
            V02ClearTemporaryDisablesWithoutLog();
            powerSlotNoticeRuntimeIds.Clear();
            unpoweredBlockedRuntimeIds.Clear();
            v02StatusTickTimer = 1f;
            v02EnemyBurnTickTimer = 1f;
            ConfigureEnemyTimers();
        }

        private void ConfigureEnemyTimers()
        {
            if (currentEnemy == null || currentEnemy.definition == null)
            {
                return;
            }

            currentEnemy.attackTimer = GetEnemyAttackInterval();
            currentEnemy.isBoss = currentEnemy.definition.enemyType == EnemyType.Boss;
            currentEnemy.isCharging = false;
            currentEnemy.isEnraged = false;
            currentEnemy.hasTriggeredEnrage = false;
            currentEnemy.chargeTimer = 0f;
            currentEnemy.chargeIntervalTimer = currentEnemy.definition.enemyType == EnemyType.Boss ? GetChargeInterval() : 0f;
            currentEnemy.manaDrainTimer = currentEnemy.definition.enemyType == EnemyType.Boss ? GetManaDrainInterval() : 0f;
            currentEnemy.specialTimer = currentEnemy.definition.enemyType switch
            {
                EnemyType.SwordCultivator => GetChargeInterval(),
                EnemyType.EvilCultivator => GetManaDrainInterval(),
                EnemyType.GhostSwarm => GetGhostShadowInterval(),
                _ => 0f
            };
            currentEnemy.sealTimer = currentEnemy.definition.enemyType switch
            {
                EnemyType.EvilCultivator => GetSealInterval(),
                EnemyType.Boss => GetSealInterval(),
                _ => 0f
            };
            currentEnemy.bossChargeCooldown = currentEnemy.chargeIntervalTimer;
            v02BossPhaseController?.ResetPhase();
        }

        private void PrepareItemCooldowns()
        {
            if (grid == null)
            {
                return;
            }

            RefreshFormationPowerStates();
            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                item.ResetForBattle(GetEffectiveCooldown(item));
                EmitPowerSlotNoticeIfNeeded(item);
            }
        }

        private void EmitPowerSlotNoticeIfNeeded(TalismanItemRuntime item)
        {
            if (state != TalismanCombatState.Fighting ||
                bagExpansionController == null ||
                item == null ||
                item.definition == null ||
                item.definition.itemType == TalismanItemType.PassiveTool ||
                bagExpansionController.GetCooldownMultiplier(item) >= 0.999f)
            {
                return;
            }

            string noticeId = string.IsNullOrEmpty(item.runtimeId) ? $"{item.definition.itemId}:{item.gridPosition.x}:{item.gridPosition.y}" : item.runtimeId;
            if (!powerSlotNoticeRuntimeIds.Add(noticeId))
            {
                return;
            }

            Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, $"{GetRuntimeItemName(item)}受到阵眼强化，冷却缩短", item.definition.itemId, value: 0, screenPosition: GetItemPosition(item));
        }

        private void TrackUnpoweredTriggerBlocked(TalismanItemRuntime item)
        {
            if (item?.definition == null || !item.definition.requiresFormationPower)
            {
                return;
            }

            string noticeId = GetRuntimeNoticeId(item);
            if (!unpoweredBlockedRuntimeIds.Add(noticeId))
            {
                return;
            }

            if (v02FailureTracker != null)
            {
                v02FailureTracker.unpoweredTriggerBlockedCount++;
            }

            Emit(BattleEventType.LogMessage, BattleLogCategory.Danger, $"{GetRuntimeItemName(item)}未供能，无法触发", item.definition.itemId, value: 1, screenPosition: GetItemPosition(item));
        }

        private static string GetRuntimeNoticeId(TalismanItemRuntime item)
        {
            return string.IsNullOrEmpty(item.runtimeId) ? $"{item.definition.itemId}:{item.gridPosition.x}:{item.gridPosition.y}" : item.runtimeId;
        }

        private void ApplyFireBurnReward(TalismanItemRuntime item)
        {
            if (v02RunModifierState == null || v02RunModifierState.fireBurnBonusStacks <= 0 || currentEnemy?.definition == null)
            {
                return;
            }

            EnsureStatusEffectSystem();
            enemyStatusController?.AddStatus(StatusEffectLibrary.Burn, GetRuntimeItemName(item), "Item", v02RunModifierState.fireBurnBonusStacks);
            Emit(BattleEventType.ComboActivated, BattleLogCategory.Combo, $"[强化] 火符燃烧强化生效，额外叠加 {v02RunModifierState.fireBurnBonusStacks} 层燃烧", item.definition.itemId, value: v02RunModifierState.fireBurnBonusStacks, screenPosition: GetEnemyPosition());
        }

        private float GetShieldRewardMultiplier(TalismanItemRuntime item)
        {
            if (v02RunModifierState == null || item?.definition == null)
            {
                return 0f;
            }

            float multiplier = Mathf.Max(0f, v02RunModifierState.shieldAmountMultiplierBonus);
            if (v02RunModifierState.outerRingDefenseBoostUnlocked && IsOuterRingItem(item))
            {
                multiplier += 0.3f;
            }

            return multiplier;
        }

        private float GetCleanseCooldownReduction(TalismanItemRuntime item)
        {
            if (v02RunModifierState == null || item?.definition == null)
            {
                return 0f;
            }

            float reduction = Mathf.Max(0f, v02RunModifierState.cleanseCooldownReduction);
            if (v02RunModifierState.outerRingDefenseBoostUnlocked && IsOuterRingItem(item))
            {
                reduction += 0.15f;
            }

            return Mathf.Clamp(reduction, 0f, 0.8f);
        }

        private bool IsOuterRingItem(TalismanItemRuntime item)
        {
            if (grid == null || item == null)
            {
                return false;
            }

            Vector2Int position = item.gridPosition;
            return position.x == 0 || position.x == grid.Width - 1 || position.y == 0 || position.y == grid.Height - 1;
        }

        private bool IsThunderRewardSource(TalismanItemRuntime item)
        {
            return item?.definition != null &&
                   (item.definition.itemId == ThunderTalismanId ||
                    item.definition.itemId == "chain_thunder_talisman_basic" ||
                    item.definition.elementTag == ElementTag.Thunder);
        }

        private float GetEffectiveCooldown(TalismanItemRuntime item)
        {
            if (item == null || item.definition == null)
            {
                return 1f;
            }

            float cooldown = Mathf.Max(0.1f, item.definition.baseCooldown);
            if (item.definition.itemId == FireTalismanId && HasActiveAdjacentItemId(item.gridPosition, SpiritStoneId))
            {
                cooldown = fireBoostedCooldown;
            }

            if (item.definition.itemId == QiPillId && HasActiveAdjacentItemId(item.gridPosition, WaterTalismanId))
            {
                cooldown *= 0.8f;
            }

            if (bagExpansionController != null)
            {
                cooldown *= bagExpansionController.GetCooldownMultiplier(item);
            }

            FormationPowerState formationPowerState = GetCurrentFormationPowerState(item);
            if (formationPowerState == FormationPowerState.WeakPowered)
            {
                cooldown *= formationPowerResolver != null ? formationPowerResolver.WeakCooldownMultiplier : DefaultWeakCooldownMultiplier;
            }

            if (item.definition.itemId == "purify_talisman_basic")
            {
                float reduction = GetCleanseCooldownReduction(item);
                if (reduction > 0f)
                {
                    cooldown *= Mathf.Clamp01(1f - reduction);
                }
            }

            return Mathf.Max(0.1f, cooldown);
        }

        private int GetManaCost(TalismanItemRuntime item)
        {
            return item.definition.itemId switch
            {
                FireTalismanId => item.level >= 2 ? 12 : 10,
                ThunderTalismanId => item.level >= 2 ? 18 : 16,
                ShieldTalismanId => item.level >= 2 ? 10 : 8,
                QiPillId => item.level >= 2 ? 14 : 12,
                _ => item.definition.manaCost
            };
        }

        private int GetSpiritManaGain(TalismanItemRuntime item)
        {
            return item.level >= 2 ? 18 : 12;
        }

        private float GetEnemyAttackInterval()
        {
            if (IsBoss() && currentEnemy != null && currentEnemy.isEnraged)
            {
                return 2.5f;
            }

            return currentEnemy.definition.attackInterval;
        }

        private float GetChargeInterval()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.chargeInterval > 0f)
            {
                return currentEnemy.definition.chargeInterval;
            }

            return IsBoss() ? bossChargeInterval : swordChargeInterval;
        }

        private float GetChargeDuration()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.chargeDuration > 0f)
            {
                return currentEnemy.definition.chargeDuration;
            }

            return IsBoss() ? bossChargeDuration : swordChargeDuration;
        }

        private int GetChargeDamage()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.chargeAttackDamage > 0)
            {
                return currentEnemy.definition.chargeAttackDamage;
            }

            return IsBoss() ? bossChargeDamage : swordChargeDamage;
        }

        private float GetManaDrainInterval()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.manaDrainInterval > 0f)
            {
                return currentEnemy.definition.manaDrainInterval;
            }

            return IsBoss() ? bossDrainInterval : evilManaDrainInterval;
        }

        private int GetManaDrainAmount()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.manaDrainAmount > 0)
            {
                return currentEnemy.definition.manaDrainAmount;
            }

            return IsBoss() ? 12 : 10;
        }

        private float GetSealInterval()
        {
            if (IsBoss() && currentEnemy != null && currentEnemy.isEnraged)
            {
                return bossEnragedSealInterval;
            }

            if (currentEnemy?.definition != null && currentEnemy.definition.sealInterval > 0f)
            {
                return currentEnemy.definition.sealInterval;
            }

            return IsBoss() ? bossSealInterval : evilSealInterval;
        }

        private float GetSealDuration()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.sealDuration > 0f)
            {
                return currentEnemy.definition.sealDuration;
            }

            if (formationPowerResolver != null && formationPowerResolver.DefaultSealDuration > 0f)
            {
                return formationPowerResolver.DefaultSealDuration;
            }

            return evilSealDuration;
        }

        private float GetGhostShadowInterval()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.ghostShadowInterval > 0f)
            {
                return currentEnemy.definition.ghostShadowInterval;
            }

            return ghostShadowInterval;
        }

        private int GetGhostShadowDamage()
        {
            if (currentEnemy?.definition != null && currentEnemy.definition.ghostShadowDamage > 0)
            {
                return currentEnemy.definition.ghostShadowDamage;
            }

            return ghostShadowDamage;
        }

        private void EmitChargeProgress()
        {
            float duration = GetChargeDuration();
            int progress = Mathf.RoundToInt((1f - currentEnemy.chargeTimer / duration) * 100f);
            Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, "", value: progress, screenPosition: GetEnemyPosition());
        }

        private void OnGridChanged()
        {
            EnsureFormationPowerResolver();
            formationPowerResolver?.RefreshPowerStates();
            SyncLiveFormationPowerStates();
            comboResolver?.RefreshCombos();
            RefreshSealHighlights();
            RefreshComboHighlights();
        }

        private void EnsureFormationPowerResolver()
        {
            if (grid == null || slotViews == null || slotViews.Length == 0)
            {
                return;
            }

            if (formationPowerResolver == null)
            {
                formationPowerResolver = GetComponent<FormationPowerResolver>();
                if (formationPowerResolver == null)
                {
                    formationPowerResolver = gameObject.AddComponent<FormationPowerResolver>();
                }
            }

            formationPowerResolver.Bind(grid, slotViews);
            formationPowerResolver.BindRunModifierState(v02RunModifierState);
        }

        private void RefreshFormationPowerStates()
        {
            if (grid == null)
            {
                return;
            }

            EnsureFormationPowerResolver();
            formationPowerResolver?.RefreshPowerStates();
            SyncLiveFormationPowerStates();
        }

        private void SyncLiveFormationPowerStates()
        {
            if (grid == null)
            {
                return;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition == null)
                {
                    continue;
                }

                item.powerState = GetCurrentFormationPowerState(item);
            }
        }

        private FormationPowerState GetCurrentFormationPowerState(TalismanItemRuntime item)
        {
            if (item?.definition == null)
            {
                return FormationPowerState.Unpowered;
            }

            if (item.definition.itemId == SpiritStoneId)
            {
                return FormationPowerState.Powered;
            }

            return ResolveLiveFormationPowerState(item.gridPosition);
        }

        private FormationPowerState ResolveLiveFormationPowerState(Vector2Int position)
        {
            if (grid == null)
            {
                return FormationPowerState.Unpowered;
            }

            List<Vector2Int> spiritStonePositions = new();
            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition != null && item.definition.itemId == SpiritStoneId)
                {
                    spiritStonePositions.Add(item.gridPosition);
                }
            }

            V02FormationBalanceConfig formationConfig = formationPowerResolver != null
                ? formationPowerResolver.FormationBalanceConfig
                : null;

            if (formationConfig == null || formationConfig.spiritStoneNineGridPowerEnabled)
            {
                foreach (Vector2Int source in spiritStonePositions)
                {
                    if (Mathf.Abs(position.x - source.x) <= 1 && Mathf.Abs(position.y - source.y) <= 1)
                    {
                        return FormationPowerState.Powered;
                    }
                }
            }

            V02RunModifierState modifierState = v02RunModifierState != null
                ? v02RunModifierState
                : formationPowerResolver != null ? formationPowerResolver.RunModifierState : null;
            if (modifierState != null &&
                modifierState.spiritLinkBetweenStonesUnlocked &&
                (formationConfig == null || formationConfig.spiritLinkBetweenStonesEnabled) &&
                IsOnLiveSpiritLink(position, spiritStonePositions))
            {
                return FormationPowerState.Powered;
            }

            Vector2Int eyePosition = formationPowerResolver != null ? formationPowerResolver.FormationEyePosition : FormationPowerResolver.GetDefaultEyeCorePosition(grid.Width, grid.Height);
            int eyeDx = Mathf.Abs(position.x - eyePosition.x);
            int eyeDy = Mathf.Abs(position.y - eyePosition.y);
            if (modifierState != null &&
                modifierState.eyeCoreNineGridUnlocked &&
                (formationConfig == null || formationConfig.upgradedEyeNineGridEnabled) &&
                eyeDx <= 1 && eyeDy <= 1)
            {
                return FormationPowerState.Powered;
            }

            if ((formationConfig == null || formationConfig.formationEyeCrossPowerEnabled) && eyeDx + eyeDy == 1)
            {
                return FormationPowerState.Powered;
            }

            if ((formationConfig == null || formationConfig.formationEyeDiagonalWeakPowerEnabled) && eyeDx == 1 && eyeDy == 1)
            {
                return FormationPowerState.WeakPowered;
            }

            return FormationPowerState.Unpowered;
        }

        private static bool IsOnLiveSpiritLink(Vector2Int position, List<Vector2Int> spiritStonePositions)
        {
            if (spiritStonePositions == null || spiritStonePositions.Count < 2)
            {
                return false;
            }

            for (int i = 0; i < spiritStonePositions.Count; i++)
            {
                for (int j = i + 1; j < spiritStonePositions.Count; j++)
                {
                    Vector2Int a = spiritStonePositions[i];
                    Vector2Int b = spiritStonePositions[j];
                    if (a.x == b.x && position.x == a.x && IsBetweenLiveSpiritLink(position.y, a.y, b.y))
                    {
                        return true;
                    }

                    if (a.y == b.y && position.y == a.y && IsBetweenLiveSpiritLink(position.x, a.x, b.x))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsBetweenLiveSpiritLink(int value, int a, int b)
        {
            return value >= Mathf.Min(a, b) && value <= Mathf.Max(a, b);
        }

        private void OnCombosChanged()
        {
            comboStatusUI?.Refresh(comboResolver);
            RefreshComboHighlights();
        }

        private void RefreshGridDependentUI()
        {
            OnGridChanged();
            comboStatusUI?.Refresh(comboResolver);
            RefreshSealHighlights();
        }

        private void RefreshComboHighlights()
        {
            if (slotViews == null || grid == null)
            {
                return;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView == null)
                {
                    continue;
                }

                TalismanItemRuntime item = grid.GetItemAt(slotView.GridPosition);
                bool highlighted = item != null && IsItemInVisibleCombo(item);
                slotView.SetComboHighlight(highlighted);
            }
        }

        private bool IsItemInVisibleCombo(TalismanItemRuntime item)
        {
            if (item == null || item.isSealed || !IsFormationItemActive(item))
            {
                return false;
            }

            string itemId = item.definition.itemId;
            Vector2Int position = item.gridPosition;
            return (itemId == FireTalismanId && (HasActiveAdjacentItemId(position, SpiritStoneId) || HasActiveAdjacentItemId(position, SwordPillId))) ||
                   (itemId == SpiritStoneId && HasActiveAdjacentItemId(position, FireTalismanId)) ||
                   (itemId == QiPillId && (HasActiveAdjacentItemId(position, ShieldTalismanId) || HasActiveAdjacentItemId(position, WaterTalismanId))) ||
                   (itemId == ShieldTalismanId && HasActiveAdjacentItemId(position, QiPillId)) ||
                   (itemId == ThunderTalismanId && HasActiveAdjacentItemId(position, SealId)) ||
                   (itemId == SealId && HasActiveAdjacentItemId(position, ThunderTalismanId)) ||
                   (itemId == SwordPillId && HasActiveAdjacentItemId(position, FireTalismanId)) ||
                   (itemId == PeachWoodId && HasActiveAdjacentItemId(position, ExorcismBellId)) ||
                   (itemId == ExorcismBellId && HasActiveAdjacentItemId(position, PeachWoodId)) ||
                   (itemId == WaterTalismanId && HasActiveAdjacentItemId(position, QiPillId));
        }

        private void RefreshSealHighlights()
        {
            if (slotViews == null || grid == null)
            {
                return;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                TalismanItemRuntime item = slotView != null ? grid.GetItemAt(slotView.GridPosition) : null;
                slotView?.SetSealed(item != null && (item.isSealed || item.isTemporarilyDisabled));
            }
        }

        private void RefreshUI()
        {
            combatUI?.Refresh(playerStats, currentEnemy, GetStateLabel());
            v02EnemyIntentUI?.Refresh(currentEnemy);
            RefreshStatusEffects();
            if (state != TalismanCombatState.Fighting)
            {
                v02EnemyPreviewPanel?.Show(currentEnemy?.definition);
            }

            if (startButton != null)
            {
                startButton.interactable = state == TalismanCombatState.Preparing || state == TalismanCombatState.Victory || state == TalismanCombatState.Defeat;
            }

            if (autoPlaceButton != null)
            {
                autoPlaceButton.interactable = CanEditLayout;
            }

            RefreshAutoMergeButton();
        }

        private void RefreshStatusEffects()
        {
            EnsureStatusEffectSystem();
            SyncPlayerStatusEffects();
            SyncEnemyStatusEffects();
            playerStatusAnchor?.Refresh();
            enemyStatusAnchor?.Refresh();
            playerBuffAnchor?.Refresh();
            playerDebuffAnchor?.Refresh();
            enemyBuffAnchor?.Refresh();
            enemyDebuffAnchor?.Refresh();
        }

        private void SyncPlayerStatusEffects()
        {
            if (playerStatusController == null)
            {
                return;
            }

            playerStatusController.SetStatus(StatusEffectLibrary.Shield, "修士", "Player", playerStats.shield);
            SyncPlayerFormationStatusEffects();
            SyncPlayerPassiveReadyStatuses();
        }

        private void SyncPlayerFormationStatusEffects()
        {
            if (playerStatusController == null)
            {
                return;
            }

            int sealedCount = 0;
            float sealRemaining = 0f;
            foreach (TalismanItemRuntime item in sealedItems)
            {
                if (item == null || !item.isSealed)
                {
                    continue;
                }

                sealedCount++;
                sealRemaining = Mathf.Max(sealRemaining, item.sealRemaining);
            }

            playerStatusController.SetStatus(StatusEffectLibrary.Seal, currentEnemy?.definition != null ? currentEnemy.definition.GetReadableLabel() : "封印", "Enemy", sealedCount, sealRemaining);

            int disabledCount = 0;
            float disabledRemaining = 0f;
            if (grid != null)
            {
                foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
                {
                    if (item == null || !item.isTemporarilyDisabled)
                    {
                        continue;
                    }

                    disabledCount++;
                    disabledRemaining = Mathf.Max(disabledRemaining, item.temporaryDisabledRemaining);
                }
            }

            playerStatusController.SetStatus(StatusEffectLibrary.EnergyDisrupt, currentEnemy?.definition != null ? currentEnemy.definition.GetReadableLabel() : "供能中断", "Enemy", disabledCount, disabledRemaining);
        }

        private void SyncPlayerPassiveReadyStatuses()
        {
            TalismanItemRuntime purify = FindActivePlacedItemById(PurifyTalismanId);
            playerStatusController.SetStatus(StatusEffectLibrary.CleanseReady, purify != null ? GetRuntimeItemName(purify) : string.Empty, "Item", purify != null ? 1 : 0);

            TalismanItemRuntime soulSuppress = FindActivePlacedItemById(SoulSuppressTalismanId);
            playerStatusController.SetStatus(StatusEffectLibrary.SoulSuppressReady, soulSuppress != null ? GetRuntimeItemName(soulSuppress) : string.Empty, "Item", soulSuppress != null ? 1 : 0);
        }

        private void SyncEnemyStatusEffects()
        {
            if (enemyStatusController == null)
            {
                return;
            }

            if (currentEnemy?.definition == null)
            {
                enemyStatusController.ClearAll();
                return;
            }

            enemyStatusController.SetStatus(StatusEffectLibrary.Shield, currentEnemy.definition.GetReadableLabel(), "Enemy", currentEnemy.currentShield);
            SyncEnemyIntentStatus();
            SyncEnemyBossPhaseStatus();
        }

        private void SyncEnemyIntentStatus()
        {
            if (currentEnemy == null)
            {
                enemyStatusController?.RemoveStatus(StatusEffectIds.EnemyIntent);
                return;
            }

            bool hasIntent = currentEnemy.isCharging || currentEnemy.isCastingSkill;
            if (!hasIntent)
            {
                enemyStatusController?.RemoveStatus(StatusEffectIds.EnemyIntent);
                return;
            }

            float remaining = currentEnemy.isCastingSkill && currentEnemy.currentCastingSkill != null
                ? currentEnemy.currentCastingSkill.castTimer
                : Mathf.Max(0.1f, currentEnemy.chargeTimer);
            string source = !string.IsNullOrWhiteSpace(currentEnemy.currentIntentText)
                ? currentEnemy.currentIntentText
                : currentEnemy.definition.GetReadableLabel();
            enemyStatusController?.SetStatus(StatusEffectLibrary.EnemyIntent, source, "Enemy", 1, remaining);
        }

        private void SyncEnemyBossPhaseStatus()
        {
            if (!IsBoss() || v02BossPhaseController == null || v02BossPhaseController.CurrentPhase == V02BossPhase.None)
            {
                enemyStatusController?.RemoveStatus(StatusEffectIds.BossPhase);
                return;
            }

            enemyStatusController?.SetStatus(StatusEffectLibrary.BossPhase, v02BossPhaseController.CurrentPhase.ToString(), "Boss", 1);
        }

        private void RefreshAutoMergeButton()
        {
            if (autoPlaceButton == null)
            {
                return;
            }

            if (autoMergeButtonText == null)
            {
                autoMergeButtonText = autoPlaceButton.GetComponentInChildren<Text>();
            }

            if (autoMergeButtonText != null)
            {
                autoMergeButtonText.text = GetAutoMergeButtonLabel();
            }
        }

        private string GetAutoMergeButtonLabel()
        {
            if (!CanEditLayout)
            {
                return "自动合成";
            }

            DraggableTalismanItemView candidate = FindMergeCandidate();
            return candidate != null && candidate.Definition != null ? $"可合成：{candidate.Definition.displayName} Lv2" : "暂无可合成";
        }

        private DraggableTalismanItemView FindMergeCandidate()
        {
            Dictionary<string, int> counts = new();
            Dictionary<string, DraggableTalismanItemView> firstViews = new();
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view == null || view.Definition == null || view.Level != 1)
                {
                    continue;
                }

                string id = view.Definition.itemId;
                counts.TryGetValue(id, out int count);
                counts[id] = count + 1;
                if (!firstViews.ContainsKey(id))
                {
                    firstViews[id] = view;
                }

                if (counts[id] >= 2)
                {
                    return firstViews[id];
                }
            }

            return null;
        }

        private string GetStateLabel()
        {
            return state switch
            {
                TalismanCombatState.Fighting => currentEnemy != null && currentEnemy.isCharging ? "蓄力中" : "自动斗法中",
                TalismanCombatState.Victory => "胜利",
                TalismanCombatState.Defeat => "失败",
                TalismanCombatState.RunComplete => "通关",
                _ => "战前整理",
            };
        }

        private void FlashItem(TalismanItemRuntime item)
        {
            DraggableTalismanItemView view = FindItemView(item);
            view?.Flash();
            view?.CurrentSlot?.Flash();
        }

        private DraggableTalismanItemView FindItemView(TalismanItemRuntime item)
        {
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view != null && view.RuntimeItem == item)
                {
                    return view;
                }
            }

            return null;
        }

        private void ClearSlotViewLinks()
        {
            if (slotViews == null)
            {
                return;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                slotView?.SetItemView(null);
            }
        }

        private void PlaceById(string itemId, Vector2Int position)
        {
            DraggableTalismanItemView view = FindItemViewById(itemId);
            TalismanGridSlotView slot = FindSlotForPlacement(position);
            if (view != null && slot != null)
            {
                view.ForcePlaceOnSlot(slot);
            }
        }

        private TalismanGridSlotView FindSlotForPlacement(Vector2Int preferredPosition)
        {
            TalismanGridSlotView preferred = FindSlot(preferredPosition);
            if (CanUsePlacementSlot(preferred))
            {
                return preferred;
            }

            TalismanGridSlotView best = null;
            int bestDistance = int.MaxValue;
            if (slotViews == null)
            {
                return null;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (!CanUsePlacementSlot(slotView))
                {
                    continue;
                }

                Vector2Int position = slotView.GridPosition;
                int distance = Mathf.Abs(position.x - preferredPosition.x) + Mathf.Abs(position.y - preferredPosition.y);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = slotView;
                }
            }

            return best;
        }

        private bool CanUsePlacementSlot(TalismanGridSlotView slot)
        {
            return slot != null &&
                   slot.CanAcceptItem &&
                   slot.CurrentItemView == null &&
                   (grid == null || grid.GetItemAt(slot.GridPosition) == null);
        }

        private DraggableTalismanItemView FindItemViewById(string itemId)
        {
            foreach (DraggableTalismanItemView view in itemViews)
            {
                if (view != null && view.CurrentSlot == null && view.Definition != null && view.Definition.itemId == itemId)
                {
                    return view;
                }
            }

            return null;
        }

        private TalismanGridSlotView FindSlot(Vector2Int position)
        {
            if (slotViews == null)
            {
                return null;
            }

            foreach (TalismanGridSlotView slotView in slotViews)
            {
                if (slotView != null && slotView.GridPosition == position)
                {
                    return slotView;
                }
            }

            return null;
        }

        private TalismanItemRuntime FindPlacedItemById(string itemId)
        {
            if (grid == null)
            {
                return null;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition != null && item.definition.itemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }

        private TalismanItemRuntime FindActivePlacedItemById(string itemId)
        {
            if (grid == null)
            {
                return null;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition == null || item.definition.itemId != itemId)
                {
                    continue;
                }

                if (!item.isSealed && !item.isTemporarilyDisabled && IsFormationItemActive(item))
                {
                    return item;
                }
            }

            return null;
        }

        private TalismanItemRuntime FindFormationEyeSealTarget()
        {
            if (grid == null)
            {
                return null;
            }

            Vector2Int eye = formationPowerResolver != null ? formationPowerResolver.FormationEyePosition : FormationPowerResolver.GetDefaultEyeCorePosition(grid.Width, grid.Height);
            Vector2Int[] primary =
            {
                eye + Vector2Int.up,
                eye + Vector2Int.down,
                eye + Vector2Int.left,
                eye + Vector2Int.right
            };

            TalismanItemRuntime target = FindUnsealedItemAt(primary);
            if (target != null)
            {
                return target;
            }

            Vector2Int[] secondary =
            {
                eye + new Vector2Int(1, 1),
                eye + new Vector2Int(1, -1),
                eye + new Vector2Int(-1, 1),
                eye + new Vector2Int(-1, -1)
            };

            target = FindUnsealedItemAt(secondary);
            if (target != null)
            {
                return target;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (IsLegalSealTarget(item))
                {
                    return item;
                }
            }

            return null;
        }

        private TalismanItemRuntime FindUnsealedItemAt(IEnumerable<Vector2Int> positions)
        {
            foreach (Vector2Int position in positions)
            {
                TalismanItemRuntime item = grid.GetItemAt(position);
                if (IsLegalSealTarget(item))
                {
                    return item;
                }
            }

            return null;
        }

        private TalismanItemRuntime FindAdjacentItem(Vector2Int position, string itemId)
        {
            foreach (TalismanItemRuntime item in grid.GetAdjacentItems(position))
            {
                if (item?.definition != null && !item.isSealed && item.definition.itemId == itemId && IsFormationItemActive(item))
                {
                    return item;
                }
            }

            return null;
        }

        private bool IsFormationItemActive(TalismanItemRuntime item)
        {
            if (item?.definition == null)
            {
                return true;
            }

            return !item.definition.requiresFormationPower ||
                   formationPowerResolver != null && formationPowerResolver.UnpoweredTalismansCanTrigger ||
                   GetCurrentFormationPowerState(item) != FormationPowerState.Unpowered;
        }

        private bool HasActiveAdjacentItemId(Vector2Int position, string itemId)
        {
            return FindAdjacentItem(position, itemId) != null;
        }

        private bool HasPlacedItem(string itemId)
        {
            return HasPlacedItemLevel(itemId, 1);
        }

        private bool HasPlacedItemLevel(string itemId, int minLevel)
        {
            if (grid == null)
            {
                return false;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition != null && !item.isSealed && item.definition.itemId == itemId && item.level >= minLevel)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsBoss()
        {
            return currentEnemy?.definition != null && currentEnemy.definition.enemyType == EnemyType.Boss;
        }

        private bool HasV02EnemyData()
        {
            EnemyDefinition definition = currentEnemy?.definition;
            return definition != null &&
                   (!string.IsNullOrWhiteSpace(definition.enemyClass) ||
                    !string.IsNullOrWhiteSpace(definition.enemyArchetype) ||
                    definition.skills != null && definition.skills.Count > 0 ||
                    definition.weaknessTags != null && definition.weaknessTags.Count > 0);
        }

        private bool HasEnemyWeakness(CounterTag tag)
        {
            return currentEnemy?.definition?.weaknessTags != null && currentEnemy.definition.weaknessTags.Contains(tag);
        }

        private bool IsEnemyChargePressure()
        {
            if (currentEnemy == null)
            {
                return false;
            }

            if (currentEnemy.isCharging)
            {
                return true;
            }

            EnemySkillDefinition skill = currentEnemy.currentCastingSkill?.definition;
            return skill?.skillTags != null && skill.skillTags.Contains(CounterTag.Charge);
        }

        private List<TalismanItemRuntime> GetPlacedPoweredCounterCandidates()
        {
            List<TalismanItemRuntime> candidates = new();
            if (grid == null)
            {
                return candidates;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition == null || item.isSealed || item.isTemporarilyDisabled || !IsFormationItemActive(item))
                {
                    continue;
                }

                candidates.Add(item);
            }

            return candidates;
        }

        private static string GetRuntimeItemName(TalismanItemRuntime item)
        {
            if (item == null || item.definition == null)
            {
                return "道具";
            }

            return item.level > 1 ? $"{item.definition.displayName} Lv{item.level}" : item.definition.displayName;
        }

        private bool IsGhostFamily()
        {
            return currentEnemy?.definition != null &&
                   (currentEnemy.definition.enemyType == EnemyType.Ghost ||
                    currentEnemy.definition.enemyType == EnemyType.GhostSwarm);
        }

        private void AddLog(string message)
        {
            Emit(BattleEventType.LogMessage, BattleLogCategory.Normal, message);
        }

        private void Emit(BattleEventType eventType, BattleLogCategory category, string message, string sourceId = "", string targetId = "", int value = 0, Vector2 screenPosition = default, Vector2 targetScreenPosition = default)
        {
            eventRouter?.Emit(eventType, category, message, sourceId, targetId, value, screenPosition, targetScreenPosition);
            if (eventRouter == null && !string.IsNullOrEmpty(message))
            {
                battleLogUI?.AddLog(message);
            }
        }

        private void AddManaWithWaste(int amount, TalismanItemRuntime source)
        {
            int before = playerStats.mana;
            playerStats.AddMana(amount);
            int gained = playerStats.mana - before;
            int wasted = Mathf.Max(0, amount - gained);
            Emit(BattleEventType.ManaGenerated, BattleLogCategory.Mana, $"{GetRuntimeItemName(source)}产生 {gained} 点灵气", source.definition.itemId, value: gained, screenPosition: GetItemPosition(source));
            if (wasted > 0 && statsTracker != null)
            {
                statsTracker.totalManaWasted += wasted;
            }
        }

        private void EmitManaFlows(TalismanItemRuntime source)
        {
            if (grid == null)
            {
                return;
            }

            Vector2 from = GetItemPosition(source);
            foreach (TalismanItemRuntime adjacent in grid.GetAdjacentItems(source.gridPosition))
            {
                if (adjacent?.definition == null || adjacent.definition.manaCost <= 0)
                {
                    continue;
                }

                Emit(BattleEventType.ManaGenerated, BattleLogCategory.Normal, "", source.definition.itemId, adjacent.definition.itemId, 0, from, GetItemPosition(adjacent));
            }
        }

        private Vector2 GetItemPosition(TalismanItemRuntime item)
        {
            DraggableTalismanItemView view = FindItemView(item);
            if (view != null && view.CurrentSlot != null)
            {
                return RectToFeedbackPosition(view.CurrentSlot.GetComponent<RectTransform>());
            }

            return Vector2.zero;
        }

        private Vector2 GetPlayerPosition()
        {
            return new Vector2(-330f, 865f);
        }

        private Vector2 GetPlayerStatusPosition()
        {
            StatusAnchorUI anchor = playerDebuffAnchor != null ? playerDebuffAnchor : playerStatusAnchor;
            if (anchor != null)
            {
                Vector2 position = anchor.GetFloatingTextPosition(feedbackRoot);
                if (position != Vector2.zero)
                {
                    return position;
                }
            }

            return GetPlayerPosition();
        }

        private Vector2 GetEnemyPosition()
        {
            return new Vector2(-330f, 660f);
        }

        private Vector2 GetManaPosition()
        {
            return new Vector2(0f, 865f);
        }

        private Vector2 RectToFeedbackPosition(RectTransform rect)
        {
            if (rect == null || feedbackRoot == null)
            {
                return Vector2.zero;
            }

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rect.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(feedbackRoot, screenPoint, null, out Vector2 localPoint);
            return localPoint;
        }
    }
}
