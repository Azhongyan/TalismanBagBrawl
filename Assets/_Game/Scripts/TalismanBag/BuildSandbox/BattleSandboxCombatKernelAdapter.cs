using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    public enum BattleSandboxKernelPowerState
    {
        Unpowered = 0,
        FullPowered = 1,
        WeakPowered = 2
    }

    [Serializable]
    public sealed class BattleSandboxCombatKernelAdapterPreview
    {
        public const string PackageName = "V0.4-BattleSandboxCombatKernelAdapter01";
        public const string PreviewBuildId = "battle_sandbox_combat_kernel_adapter";

        public string packageName = PackageName;
        public string sourcePreviewBuildId = PreviewBuildId;
        public bool devOnly = true;
        public bool isEnabled;
        public bool adapterOnly = true;
        public bool readsV02V03RuleMouthfeel = true;
        public bool reusesSpiritPowerRules = true;
        public bool reusesCooldownRules = true;
        public bool reusesDamageRules = true;
        public bool reusesShieldRules = true;
        public bool reusesHealingRules = true;
        public bool reusesEnemyHpRules = true;
        public bool reusesEnemySkillCastBarRules = true;
        public bool callsFormalRunFlow;
        public bool readsFormalSaveData;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool rewritesV04Ui;
        public bool modifiesV02V03Runtime;
        public bool callsFormalDamageSettlement;
        public bool opensFeatureFlag;
        public BattleSandboxCombatKernelSampleSnapshot sample = new();
        public List<BattleSandboxCombatKernelAdapterRow> rows = new();

        public int RuleRowCount => rows?.Count ?? 0;
        public int FeatureFlagDefaultTrueCount => BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
        public bool FeatureFlagsAllDisabled => BuildSandboxFeatureFlags.AreAllDefaultsDisabled();
        public int RuleScopeLeakCount => CountRuleScopeLeaks();
        public int FormalFlowLeakCount => CountFormalFlowLeaks();
        public int UiLayoutLeakCount => rows?.Count(row => row == null || row.uiLayoutLeak) ?? 1;
        public bool DevOnlyIsolationPass => devOnly
            && !isEnabled
            && adapterOnly
            && readsV02V03RuleMouthfeel
            && !callsFormalRunFlow
            && !readsFormalSaveData
            && !writesFormalSaveData
            && !grantsFormalReward
            && !advancesChapter
            && !rewritesV04Ui
            && !modifiesV02V03Runtime
            && !callsFormalDamageSettlement
            && !opensFeatureFlag
            && FeatureFlagsAllDisabled
            && RuleScopeLeakCount == 0;

        private int CountRuleScopeLeaks()
        {
            if (rows == null || rows.Count == 0)
            {
                return 1;
            }

            return rows.Count(row =>
                row == null
                || !row.devOnly
                || row.isEnabled
                || row.formalFlowLeak
                || row.uiLayoutLeak
                || row.modifiesStableRuntime);
        }

        private int CountFormalFlowLeaks()
        {
            int leaks = 0;
            if (!devOnly) leaks++;
            if (isEnabled) leaks++;
            if (!adapterOnly) leaks++;
            if (!readsV02V03RuleMouthfeel) leaks++;
            if (!reusesSpiritPowerRules) leaks++;
            if (!reusesCooldownRules) leaks++;
            if (!reusesDamageRules) leaks++;
            if (!reusesShieldRules) leaks++;
            if (!reusesHealingRules) leaks++;
            if (!reusesEnemyHpRules) leaks++;
            if (!reusesEnemySkillCastBarRules) leaks++;
            if (callsFormalRunFlow) leaks++;
            if (readsFormalSaveData) leaks++;
            if (writesFormalSaveData) leaks++;
            if (grantsFormalReward) leaks++;
            if (advancesChapter) leaks++;
            if (rewritesV04Ui) leaks++;
            if (modifiesV02V03Runtime) leaks++;
            if (callsFormalDamageSettlement) leaks++;
            if (opensFeatureFlag) leaks++;
            leaks += RuleScopeLeakCount;
            leaks += FeatureFlagDefaultTrueCount;
            return leaks;
        }
    }

    [Serializable]
    public sealed class BattleSandboxCombatKernelAdapterRow
    {
        public string ruleKey = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string sourceReference = string.Empty;
        public string adapterMouthfeel = string.Empty;
        public string sampleInput = string.Empty;
        public string sampleOutput = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool formalFlowLeak;
        public bool uiLayoutLeak;
        public bool modifiesStableRuntime;

        public bool ScopePass => devOnly
            && !isEnabled
            && !formalFlowLeak
            && !uiLayoutLeak
            && !modifiesStableRuntime;
    }

    [Serializable]
    public sealed class BattleSandboxCombatKernelSampleSnapshot
    {
        public BattleSandboxKernelPowerSample power = new();
        public BattleSandboxKernelCooldownSample cooldown = new();
        public BattleSandboxKernelDamageSample damage = new();
        public BattleSandboxKernelShieldSample playerShield = new();
        public BattleSandboxKernelHealingSample healing = new();
        public BattleSandboxKernelEnemyHpSample enemyHp = new();
        public BattleSandboxKernelCastBarSample castBar = new();
        public BattleSandboxKernelStatusTickSample statusTick = new();
    }

    [Serializable]
    public sealed class BattleSandboxKernelPowerSample
    {
        public Vector2Int boardSize = new(5, 5);
        public Vector2Int eyeCell = new(2, 2);
        public Vector2Int spiritStoneCell = new(2, 3);
        public Vector2Int eyeCrossCell = new(3, 2);
        public Vector2Int eyeDiagonalCell = new(3, 3);
        public Vector2Int spiritNineGridCell = new(3, 4);
        public BattleSandboxKernelPowerState eyeCrossState = BattleSandboxKernelPowerState.FullPowered;
        public BattleSandboxKernelPowerState eyeDiagonalState = BattleSandboxKernelPowerState.WeakPowered;
        public BattleSandboxKernelPowerState spiritNineGridState = BattleSandboxKernelPowerState.FullPowered;
        public float weakPoweredCooldownMultiplier = 1.35f;
    }

    [Serializable]
    public sealed class BattleSandboxKernelCooldownSample
    {
        public float minCooldown = BattleSandboxCombatKernelAdapterBuilder.MinCooldown;
        public float baseCooldown = 2f;
        public float computedCooldown = 1.8f;
        public float fireAdjacentSpiritCooldown = 1.6f;
        public float weakPowerMultiplier = 1.35f;
        public float fullPoweredFireCooldown = 1.6f;
        public float weakPoweredFireCooldown = 2.16f;
        public float trainedWeakPoweredFireCooldown = 1.944f;
        public float minClampedCooldown = BattleSandboxCombatKernelAdapterBuilder.MinCooldown;
    }

    [Serializable]
    public sealed class BattleSandboxKernelDamageSample
    {
        public int enemyHpBefore = 120;
        public int enemyShieldBefore = 8;
        public int incomingDamage = 20;
        public int blockedByShield = 8;
        public int hpDamage = 12;
        public int enemyShieldAfter;
        public int enemyHpAfter = 108;
    }

    [Serializable]
    public sealed class BattleSandboxKernelShieldSample
    {
        public int playerShieldBefore = 45;
        public int shieldGain = 18;
        public int shieldCap = 50;
        public int playerShieldAfter = 50;
        public int wastedShield = 13;
        public int enemyShieldBefore;
        public int enemyShieldGain = 12;
        public int enemyShieldAfter = 12;
    }

    [Serializable]
    public sealed class BattleSandboxKernelHealingSample
    {
        public int playerHpBefore = 90;
        public int healAmount = 20;
        public int playerMaxHp = 100;
        public int playerHpAfter = 100;
        public int overheal = 10;
    }

    [Serializable]
    public sealed class BattleSandboxKernelEnemyHpSample
    {
        public int maxHp = 120;
        public int currentHpAtSpawn = 120;
        public int shieldAtSpawn;
        public int attackDamage = 8;
        public float attackInterval = 2f;
        public int bossEnrageHpThreshold = 60;
    }

    [Serializable]
    public sealed class BattleSandboxKernelCastBarSample
    {
        public float initialDelay = 0.5f;
        public float castTime = 1.25f;
        public float tickDelta = 0.5f;
        public float castTimeRemainingAfterTick = 0.75f;
        public float castBarRemainingRatioAfterTick = 0.6f;
        public float cooldownAfterComplete = 4f;
        public float zeroCooldownAfterComplete = BattleSandboxCombatKernelAdapterBuilder.MinCooldown;
    }

    [Serializable]
    public sealed class BattleSandboxKernelStatusTickSample
    {
        public int playerPoisonStacks = 2;
        public int playerBurnStacks = 3;
        public int playerDotDamagePerSecond = 5;
        public int enemyBurnStacks = 3;
        public int enemyBurnDamagePerSecond = 3;
    }

    public static class BattleSandboxCombatKernelAdapterBuilder
    {
        public const string PackageName = BattleSandboxCombatKernelAdapterPreview.PackageName;
        public const float MinCooldown = 0.1f;
        public const float WeakPowerCooldownMultiplier = 1.35f;
        public const float FireAdjacentSpiritCooldown = 1.6f;
        public const float QiPillAdjacentWaterCooldownMultiplier = 0.8f;
        public const int PlayerShieldCap = 50;

        private const string SourceSpiritPower =
            "V02/Formation/FormationPowerResolver + V02/Balance/V02FormationBalanceConfig";
        private const string SourceCooldown =
            "V02/CoreLoop/Battle/BattleLoadoutSnapshotBuilder + Combat/AutoCombatController.GetEffectiveCooldown";
        private const string SourceDamage =
            "Combat/AutoCombatController.DealDamage + Combat/CombatStats.TakeDamage";
        private const string SourceShield =
            "Combat/CombatStats.AddShield + Combat/AutoCombatController.V02AddEnemyShield";
        private const string SourceHealing =
            "Combat/CombatStats.Heal + Combat/AutoCombatController.TryTriggerItem";
        private const string SourceEnemyHp =
            "Enemies/EnemyRuntime + Combat/AutoCombatController.UpdateEnemyBasicAttack";
        private const string SourceCastBar =
            "V02/EnemySkills/EnemySkillRuntime + V02/EnemySkills/EnemySkillController";
        private const string SourceStatus =
            "V02/Status/StatusEffectController + Combat/AutoCombatController status ticks";
        private const string SourceIsolation =
            "Docs/LOCKED + Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP";

        public static BattleSandboxCombatKernelAdapterPreview BuildDefaultPreview()
        {
            BattleSandboxCombatKernelSampleSnapshot sample = BuildSampleSnapshot();
            return new BattleSandboxCombatKernelAdapterPreview
            {
                packageName = PackageName,
                sourcePreviewBuildId = BattleSandboxCombatKernelAdapterPreview.PreviewBuildId,
                devOnly = true,
                isEnabled = false,
                adapterOnly = true,
                readsV02V03RuleMouthfeel = true,
                reusesSpiritPowerRules = true,
                reusesCooldownRules = true,
                reusesDamageRules = true,
                reusesShieldRules = true,
                reusesHealingRules = true,
                reusesEnemyHpRules = true,
                reusesEnemySkillCastBarRules = true,
                callsFormalRunFlow = false,
                readsFormalSaveData = false,
                writesFormalSaveData = false,
                grantsFormalReward = false,
                advancesChapter = false,
                rewritesV04Ui = false,
                modifiesV02V03Runtime = false,
                callsFormalDamageSettlement = false,
                opensFeatureFlag = false,
                sample = sample,
                rows = BuildRows(sample)
            };
        }

        public static BattleSandboxCombatKernelSampleSnapshot BuildSampleSnapshot()
        {
            return new BattleSandboxCombatKernelSampleSnapshot
            {
                power = BuildPowerSample(),
                cooldown = BuildCooldownSample(),
                damage = ApplyEnemyDamage(120, 8, 20),
                playerShield = ApplyPlayerShield(45, 18, PlayerShieldCap),
                healing = ApplyHealing(90, 20, 100),
                enemyHp = BuildEnemyHpSample(),
                castBar = BuildCastBarSample(),
                statusTick = BuildStatusTickSample()
            };
        }

        public static BattleSandboxKernelPowerState ResolvePowerState(
            Vector2Int cell,
            Vector2Int eyeCell,
            IReadOnlyList<Vector2Int> spiritStones,
            bool spiritStoneNineGridPower = true,
            bool spiritLinkPower = true,
            bool upgradedEyeNineGridUnlocked = false,
            bool eyeCrossPower = true,
            bool eyeDiagonalWeakPower = true)
        {
            IReadOnlyList<Vector2Int> stones = spiritStones ?? Array.Empty<Vector2Int>();
            if (stones.Any(stone => stone == cell))
            {
                return BattleSandboxKernelPowerState.FullPowered;
            }

            if (spiritStoneNineGridPower
                && stones.Any(stone => Mathf.Abs(stone.x - cell.x) <= 1 && Mathf.Abs(stone.y - cell.y) <= 1))
            {
                return BattleSandboxKernelPowerState.FullPowered;
            }

            if (spiritLinkPower && IsBetweenTwoSpiritStones(cell, stones))
            {
                return BattleSandboxKernelPowerState.FullPowered;
            }

            int eyeDx = Mathf.Abs(cell.x - eyeCell.x);
            int eyeDy = Mathf.Abs(cell.y - eyeCell.y);
            if (upgradedEyeNineGridUnlocked && eyeDx <= 1 && eyeDy <= 1)
            {
                return BattleSandboxKernelPowerState.FullPowered;
            }

            if (eyeCrossPower && eyeDx + eyeDy == 1)
            {
                return BattleSandboxKernelPowerState.FullPowered;
            }

            if (eyeDiagonalWeakPower && eyeDx == 1 && eyeDy == 1)
            {
                return BattleSandboxKernelPowerState.WeakPowered;
            }

            return BattleSandboxKernelPowerState.Unpowered;
        }

        public static float ResolveEffectiveCooldown(
            float baseCooldown,
            float computedCooldown,
            bool isFireAdjacentSpirit,
            bool isQiPillAdjacentWater,
            bool isWeakPowered)
        {
            float cooldown = Mathf.Max(MinCooldown, baseCooldown);
            if (isFireAdjacentSpirit)
            {
                cooldown = FireAdjacentSpiritCooldown;
            }

            if (isQiPillAdjacentWater)
            {
                cooldown *= QiPillAdjacentWaterCooldownMultiplier;
            }

            if (isWeakPowered)
            {
                cooldown *= WeakPowerCooldownMultiplier;
            }

            if (computedCooldown > 0f && baseCooldown > 0f)
            {
                cooldown *= computedCooldown / baseCooldown;
            }

            return Mathf.Max(MinCooldown, cooldown);
        }

        public static BattleSandboxKernelDamageSample ApplyEnemyDamage(
            int enemyHp,
            int enemyShield,
            int incomingDamage)
        {
            int safeHp = Mathf.Max(0, enemyHp);
            int safeShield = Mathf.Max(0, enemyShield);
            int safeDamage = Mathf.Max(0, incomingDamage);
            int blockedByShield = Mathf.Min(safeShield, safeDamage);
            int hpDamage = safeDamage - blockedByShield;
            return new BattleSandboxKernelDamageSample
            {
                enemyHpBefore = safeHp,
                enemyShieldBefore = safeShield,
                incomingDamage = safeDamage,
                blockedByShield = blockedByShield,
                hpDamage = hpDamage,
                enemyShieldAfter = safeShield - blockedByShield,
                enemyHpAfter = Mathf.Max(0, safeHp - hpDamage)
            };
        }

        public static BattleSandboxKernelShieldSample ApplyPlayerShield(
            int currentShield,
            int gain,
            int cap)
        {
            int safeCurrent = Mathf.Max(0, currentShield);
            int safeGain = Mathf.Max(0, gain);
            int safeCap = Mathf.Max(0, cap);
            int after = Mathf.Clamp(safeCurrent + safeGain, 0, safeCap);
            return new BattleSandboxKernelShieldSample
            {
                playerShieldBefore = safeCurrent,
                shieldGain = safeGain,
                shieldCap = safeCap,
                playerShieldAfter = after,
                wastedShield = Mathf.Max(0, safeCurrent + safeGain - safeCap),
                enemyShieldBefore = 0,
                enemyShieldGain = 12,
                enemyShieldAfter = 12
            };
        }

        public static BattleSandboxKernelHealingSample ApplyHealing(
            int currentHp,
            int healAmount,
            int maxHp)
        {
            int safeMax = Mathf.Max(1, maxHp);
            int safeHp = Mathf.Clamp(currentHp, 0, safeMax);
            int safeHeal = Mathf.Max(0, healAmount);
            int after = Mathf.Clamp(safeHp + safeHeal, 0, safeMax);
            return new BattleSandboxKernelHealingSample
            {
                playerHpBefore = safeHp,
                healAmount = safeHeal,
                playerMaxHp = safeMax,
                playerHpAfter = after,
                overheal = Mathf.Max(0, safeHp + safeHeal - safeMax)
            };
        }

        public static BattleSandboxKernelEnemyHpSample BuildEnemyHpSample()
        {
            const int maxHp = 120;
            return new BattleSandboxKernelEnemyHpSample
            {
                maxHp = maxHp,
                currentHpAtSpawn = maxHp,
                shieldAtSpawn = 0,
                attackDamage = 8,
                attackInterval = 2f,
                bossEnrageHpThreshold = Mathf.CeilToInt(maxHp * 0.5f)
            };
        }

        public static BattleSandboxKernelCastBarSample BuildCastBarSample()
        {
            const float initialDelay = 0.5f;
            const float castTime = 1.25f;
            const float tickDelta = 0.5f;
            float remaining = Mathf.Max(0f, castTime - tickDelta);
            return new BattleSandboxKernelCastBarSample
            {
                initialDelay = initialDelay,
                castTime = castTime,
                tickDelta = tickDelta,
                castTimeRemainingAfterTick = remaining,
                castBarRemainingRatioAfterTick = castTime <= 0f ? 0f : remaining / castTime,
                cooldownAfterComplete = Mathf.Max(MinCooldown, 4f),
                zeroCooldownAfterComplete = Mathf.Max(MinCooldown, 0f)
            };
        }

        public static BattleSandboxKernelStatusTickSample BuildStatusTickSample()
        {
            const int poison = 2;
            const int burn = 3;
            const int enemyBurn = 3;
            return new BattleSandboxKernelStatusTickSample
            {
                playerPoisonStacks = poison,
                playerBurnStacks = burn,
                playerDotDamagePerSecond = Mathf.Max(0, poison) + Mathf.Max(0, burn),
                enemyBurnStacks = enemyBurn,
                enemyBurnDamagePerSecond = Mathf.Max(1, enemyBurn)
            };
        }

        private static BattleSandboxKernelPowerSample BuildPowerSample()
        {
            BattleSandboxKernelPowerSample sample = new();
            IReadOnlyList<Vector2Int> stones = new[] { sample.spiritStoneCell };
            sample.eyeCrossState = ResolvePowerState(sample.eyeCrossCell, sample.eyeCell, stones);
            sample.eyeDiagonalState = ResolvePowerState(sample.eyeDiagonalCell, sample.eyeCell, Array.Empty<Vector2Int>());
            sample.spiritNineGridState = ResolvePowerState(sample.spiritNineGridCell, sample.eyeCell, stones);
            sample.weakPoweredCooldownMultiplier = WeakPowerCooldownMultiplier;
            return sample;
        }

        private static BattleSandboxKernelCooldownSample BuildCooldownSample()
        {
            const float baseCooldown = 2f;
            const float computedCooldown = 1.8f;
            return new BattleSandboxKernelCooldownSample
            {
                minCooldown = MinCooldown,
                baseCooldown = baseCooldown,
                computedCooldown = computedCooldown,
                fireAdjacentSpiritCooldown = FireAdjacentSpiritCooldown,
                weakPowerMultiplier = WeakPowerCooldownMultiplier,
                fullPoweredFireCooldown = ResolveEffectiveCooldown(baseCooldown, 0f, true, false, false),
                weakPoweredFireCooldown = ResolveEffectiveCooldown(baseCooldown, 0f, true, false, true),
                trainedWeakPoweredFireCooldown = ResolveEffectiveCooldown(baseCooldown, computedCooldown, true, false, true),
                minClampedCooldown = ResolveEffectiveCooldown(0f, 0f, false, false, false)
            };
        }

        private static List<BattleSandboxCombatKernelAdapterRow> BuildRows(
            BattleSandboxCombatKernelSampleSnapshot sample)
        {
            return new List<BattleSandboxCombatKernelAdapterRow>
            {
                Row(
                    "spiritPower",
                    "灵力供能",
                    SourceSpiritPower,
                    "Spirit stone nine-grid and formation-eye cross grant full power; diagonal eye grants weak power with 1.35 cooldown multiplier.",
                    $"eyeCross={FormatCell(sample.power.eyeCrossCell)}, eyeDiagonal={FormatCell(sample.power.eyeDiagonalCell)}, spiritCell={FormatCell(sample.power.spiritStoneCell)}",
                    $"cross={sample.power.eyeCrossState}, diagonal={sample.power.eyeDiagonalState}, spiritNineGrid={sample.power.spiritNineGridState}"),
                Row(
                    "cooldown",
                    "冷却口径",
                    SourceCooldown,
                    "Cooldown is clamped at 0.1, fire+spirit uses 1.6, weak power multiplies 1.35, computedCooldown/baseCooldown applies as the upgrade multiplier.",
                    $"base={sample.cooldown.baseCooldown:0.###}, computed={sample.cooldown.computedCooldown:0.###}, weakMultiplier={sample.cooldown.weakPowerMultiplier:0.###}",
                    $"fullFire={sample.cooldown.fullPoweredFireCooldown:0.###}, weakFire={sample.cooldown.weakPoweredFireCooldown:0.###}, trainedWeakFire={sample.cooldown.trainedWeakPoweredFireCooldown:0.###}"),
                Row(
                    "damage",
                    "伤害与护盾抵扣",
                    SourceDamage,
                    "Damage first consumes shield, then applies remaining HP damage, clamped at zero.",
                    $"enemyHp={sample.damage.enemyHpBefore}, shield={sample.damage.enemyShieldBefore}, damage={sample.damage.incomingDamage}",
                    $"blocked={sample.damage.blockedByShield}, hpDamage={sample.damage.hpDamage}, hpAfter={sample.damage.enemyHpAfter}, shieldAfter={sample.damage.enemyShieldAfter}"),
                Row(
                    "playerShield",
                    "玩家护盾",
                    SourceShield,
                    "Player shield is capped at 50; enemy shield is additive in the current V0.2/V0.3 mouthfeel.",
                    $"playerShield={sample.playerShield.playerShieldBefore}, gain={sample.playerShield.shieldGain}, cap={sample.playerShield.shieldCap}",
                    $"playerShieldAfter={sample.playerShield.playerShieldAfter}, wasted={sample.playerShield.wastedShield}, enemyShieldAfter={sample.playerShield.enemyShieldAfter}"),
                Row(
                    "healing",
                    "治疗口径",
                    SourceHealing,
                    "Healing clamps player HP into 0..maxHP and records overheal only as sandbox preview data.",
                    $"hp={sample.healing.playerHpBefore}, heal={sample.healing.healAmount}, maxHp={sample.healing.playerMaxHp}",
                    $"hpAfter={sample.healing.playerHpAfter}, overheal={sample.healing.overheal}"),
                Row(
                    "enemyHp",
                    "敌人血量与普攻",
                    SourceEnemyHp,
                    "Enemy runtime starts at definition max HP, shield starts at 0, basic attack timer follows attackInterval, Boss half-HP threshold uses ceil(maxHp*0.5).",
                    $"maxHp={sample.enemyHp.maxHp}, attack={sample.enemyHp.attackDamage}, interval={sample.enemyHp.attackInterval:0.###}",
                    $"spawnHp={sample.enemyHp.currentHpAtSpawn}, spawnShield={sample.enemyHp.shieldAtSpawn}, enrageHp={sample.enemyHp.bossEnrageHpThreshold}"),
                Row(
                    "enemySkillCastBar",
                    "敌人施法条",
                    SourceCastBar,
                    "Enemy skills wait initialDelay, enter casting for castTime, tick the cast bar down, then reset cooldown with 0.1 minimum.",
                    $"initialDelay={sample.castBar.initialDelay:0.###}, castTime={sample.castBar.castTime:0.###}, tick={sample.castBar.tickDelta:0.###}",
                    $"remaining={sample.castBar.castTimeRemainingAfterTick:0.###}, bar={sample.castBar.castBarRemainingRatioAfterTick:0.###}, cooldownAfterComplete={sample.castBar.cooldownAfterComplete:0.###}"),
                Row(
                    "statusTick",
                    "状态跳字",
                    SourceStatus,
                    "Player poison and burn stacks deal stack-sum damage per second; enemy burn tick uses max(1, stack).",
                    $"playerPoison={sample.statusTick.playerPoisonStacks}, playerBurn={sample.statusTick.playerBurnStacks}, enemyBurn={sample.statusTick.enemyBurnStacks}",
                    $"playerDot={sample.statusTick.playerDotDamagePerSecond}, enemyBurnDot={sample.statusTick.enemyBurnDamagePerSecond}"),
                Row(
                    "adapterIsolation",
                    "沙盒隔离",
                    SourceIsolation,
                    "Adapter is report-only/devOnly data and does not call formal flow, save, reward, chapter, V04 UI layout, or stable V0.2/V0.3 runtime writes.",
                    "devOnly=true, isEnabled=false, featureFlagsDefaultFalse=true",
                    "formalRunFlow=0, saveWrites=0, rewardWrites=0, chapterAdvances=0, uiRewrites=0, stableRuntimeWrites=0")
            };
        }

        private static BattleSandboxCombatKernelAdapterRow Row(
            string ruleKey,
            string chineseDisplayName,
            string sourceReference,
            string adapterMouthfeel,
            string sampleInput,
            string sampleOutput)
        {
            return new BattleSandboxCombatKernelAdapterRow
            {
                ruleKey = ruleKey,
                chineseDisplayName = chineseDisplayName,
                sourceReference = sourceReference,
                adapterMouthfeel = adapterMouthfeel,
                sampleInput = sampleInput,
                sampleOutput = sampleOutput,
                devOnly = true,
                isEnabled = false,
                formalFlowLeak = false,
                uiLayoutLeak = false,
                modifiesStableRuntime = false
            };
        }

        private static bool IsBetweenTwoSpiritStones(
            Vector2Int cell,
            IReadOnlyList<Vector2Int> spiritStones)
        {
            for (int i = 0; i < spiritStones.Count; i++)
            {
                for (int j = i + 1; j < spiritStones.Count; j++)
                {
                    Vector2Int left = spiritStones[i];
                    Vector2Int right = spiritStones[j];
                    if (left.y == right.y
                        && cell.y == left.y
                        && cell.x >= Mathf.Min(left.x, right.x)
                        && cell.x <= Mathf.Max(left.x, right.x))
                    {
                        return true;
                    }

                    if (left.x == right.x
                        && cell.x == left.x
                        && cell.y >= Mathf.Min(left.y, right.y)
                        && cell.y <= Mathf.Max(left.y, right.y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string FormatCell(Vector2Int cell)
        {
            return cell.x + ":" + cell.y;
        }
    }
}
