using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Contracts.Battle
{
    public static class C1FormalRealtimeBattleSessionContract
    {
        public const string RequestSchemaId =
            "C1FormalRealtimeBattleSessionRequest.v1";
        public const string StateSchemaId =
            "C1FormalRealtimeBattleSessionStateSnapshot.v1";
        public const string TerminalSchemaId =
            "C1FormalRealtimeBattleTerminalResult.v1";
        public const string ItemRefreshRequestSchemaId =
            "C1FormalRealtimeBattleItemRefreshRequest.v1";
        public const string ItemRefreshResultSchemaId =
            "C1FormalRealtimeBattleItemRefreshResult.v1";
        public const string Pool15ItemSnapshotSchemaId =
            "C1FormalRealtimeBattlePool15ItemSnapshot.v1";
        public const string Pool15EventRequestSchemaId =
            "C1FormalRealtimeBattlePool15EventRequest.v1";
        public const string Pool15EventResultSchemaId =
            "C1FormalRealtimeBattlePool15EventResult.v1";
        public const string ActionProgressRelationSchemaId =
            "C1_FORMAL_BATTLE_ACTION_PROGRESS_RELATION_R1";
        public const string NianCapacitySchemaId =
            "C1FormalRealtimeBattleNianCapacitySnapshot.v1";
        public const string NianStateSchemaId =
            "C1FormalRealtimeBattleNianStateSnapshot.v1";
        public const string NianTransactionSchemaId =
            "C1FormalRealtimeBattleNianTransactionSnapshot.v1";
        public const string ItemActionCostSchemaId =
            "C1FormalRealtimeBattleItemActionCostSnapshot.v1";
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string PlayerOwner = "PLAYER";
        public const string EnemyOwner = "ENEMY";
        public const string BattleOwner = "BATTLE_BRIDGE";
        public const string PlayerActorId = "campaign.normal.lv1.player";
        public const int StartingPlayerMaxHp = 100;
        public const string PhaseInactive = "INACTIVE";
        public const string PhaseRunning = "RUNNING";
        public const string PhasePaused = "PAUSED";
        public const string PhaseVictory = "VICTORY";
        public const string PhaseDefeat = "DEFEAT";
        public const string PhaseReset = "RESET";
        public const string PhaseFaulted = "FAULTED";
        public const int MaxCueLedgerEntries = 2048;
        public const int MaxScheduledActionEntries = 512;
        public const int MaxPool15ActiveSourceCount = 15;
        public const int MaxCumulativeItemInstanceCount =
            MaxPool15ActiveSourceCount + 2;
    }

    public static class C1FormalRealtimeBattleErrorCodes
    {
        public const string None = "NONE";
        public const string RequestNull = "REQUEST_NULL";
        public const string SchemaMismatch = "SCHEMA_MISMATCH";
        public const string ContextRejected = "CONTEXT_REJECTED";
        public const string SessionIdRequired = "SESSION_ID_REQUIRED";
        public const string SessionTokenRequired = "SESSION_TOKEN_REQUIRED";
        public const string SessionAlreadyActive = "SESSION_ALREADY_ACTIVE";
        public const string DuplicateSessionId = "DUPLICATE_SESSION_ID";
        public const string DuplicateSessionToken = "DUPLICATE_SESSION_TOKEN";
        public const string SessionTokenInvalidated = "SESSION_TOKEN_INVALIDATED";
        public const string LaunchGenerationInvalid = "LAUNCH_GENERATION_INVALID";
        public const string LaunchGenerationStale = "LAUNCH_GENERATION_STALE";
        public const string ResetGenerationMismatch = "RESET_GENERATION_MISMATCH";
        public const string SessionEnvelopeMismatch = "SESSION_ENVELOPE_MISMATCH";
        public const string SessionGenerationStale = "SESSION_GENERATION_STALE";
        public const string ItemInputRejected = "ITEM_INPUT_REJECTED";
        public const string ItemCatalogMismatch = "ITEM_CATALOG_MISMATCH";
        public const string EnemyCatalogMismatch = "ENEMY_CATALOG_MISMATCH";
        public const string EnemyProfileRejected = "ENEMY_PROFILE_REJECTED";
        public const string LifecycleRejected = "LIFECYCLE_REJECTED";
        public const string DeltaInvalid = "DELTA_INVALID";
        public const string StaleTick = "STALE_TICK";
        public const string TickAfterTerminal = "TICK_AFTER_TERMINAL";
        public const string DuplicateAcceptedApplication =
            "DUPLICATE_ACCEPTED_APPLICATION";
        public const string EventBufferExceeded = "EVENT_BUFFER_EXCEEDED";
        public const string SimulationGuardExceeded = "SIMULATION_GUARD_EXCEEDED";
        public const string RefreshCommandIdRequired =
            "REFRESH_COMMAND_ID_REQUIRED";
        public const string RefreshCommandConflict = "REFRESH_COMMAND_CONFLICT";
        public const string RefreshStateStale = "REFRESH_STATE_STALE";
        public const string RefreshInputRejected = "REFRESH_INPUT_REJECTED";
        public const string RefreshActionCapacityExceeded =
            "REFRESH_ACTION_CAPACITY_EXCEEDED";
        public const string Pool15SnapshotRejected =
            "POOL15_SNAPSHOT_REJECTED";
        public const string Pool15FoundationRejected =
            "POOL15_FOUNDATION_REJECTED";
        public const string Pool15EventRejected = "POOL15_EVENT_REJECTED";
        public const string Pool15EventDuplicate = "POOL15_EVENT_DUPLICATE";
        public const string Pool15EventTimeStale = "POOL15_EVENT_TIME_STALE";
        public const string Pool15EventTriggerRejected =
            "POOL15_EVENT_TRIGGER_REJECTED";
        public const string Pool15EventConditionRejected =
            "POOL15_EVENT_CONDITION_REJECTED";
        public const string UnsupportedFormalEffectMapping =
            "UNSUPPORTED_FORMAL_EFFECT_MAPPING";
        public const string Pool15AtomicityRejected =
            "POOL15_ATOMICITY_REJECTED";
        public const string ActionProgressRelationRejected =
            "ACTION_PROGRESS_RELATION_REJECTED";
        public const string ActionProgressTargetRejected =
            "ACTION_PROGRESS_TARGET_REJECTED";
        public const string ActionProgressStale =
            "ACTION_PROGRESS_STALE";
        public const string ActionProgressAtomicityRejected =
            "ACTION_PROGRESS_ATOMICITY_REJECTED";
        public const string ActionProgressUnsupported =
            "ACTION_PROGRESS_UNSUPPORTED";
        public const string EnemyActionBattleContractRequired =
            "ENEMY_ACTION_BATTLE_CONTRACT_REQUIRED";
        public const string BoneSwapOperatorRejected =
            "BONE_SWAP_OPERATOR_REJECTED";
        public const string BoneSwapOperatorStale =
            "BONE_SWAP_OPERATOR_STALE";
        public const string NianCapacityRejected =
            "NIAN_CAPACITY_REJECTED";
        public const string NianActionCostRejected =
            "NIAN_ACTION_COST_REJECTED";
        public const string NianInsufficient = "NIAN_INSUFFICIENT";
    }

    public static class C1FormalRealtimeBattleCueKinds
    {
        public const string SessionStarted = "SESSION_STARTED";
        public const string ActorSpawnVisible = "ACTOR_SPAWN_VISIBLE";
        public const string PlayerHpChanged = "PLAYER_HP_CHANGED";
        public const string PlayerGuardChanged = "PLAYER_GUARD_CHANGED";
        public const string ActorHpChanged = "ACTOR_HP_CHANGED";
        public const string ActorShellChanged = "ACTOR_SHELL_CHANGED";
        public const string ActorShellBroken = "ACTOR_SHELL_BROKEN";
        public const string TargetChanged = "TARGET_CHANGED";
        public const string ItemTriggerScheduled = "ITEM_TRIGGER_SCHEDULED";
        public const string ItemTriggerAccepted = "ITEM_TRIGGER_ACCEPTED";
        public const string EnemyAttackScheduled = "ENEMY_ATTACK_SCHEDULED";
        public const string EnemyAttackAccepted = "ENEMY_ATTACK_ACCEPTED";
        public const string EnemySkillScheduled = "ENEMY_SKILL_SCHEDULED";
        public const string EnemySkillAccepted = "ENEMY_SKILL_ACCEPTED";
        public const string EnemyPassiveTriggered =
            "ENEMY_PASSIVE_TRIGGERED";
        public const string HitAccepted = "HIT_ACCEPTED";
        public const string DamageFloatPayload = "DAMAGE_FLOAT_PAYLOAD";
        public const string ApplicationFeedbackPayload =
            "APPLICATION_FEEDBACK_PAYLOAD";
        public const string ActorDefeated = "ACTOR_DEFEATED";
        public const string BattleTerminal = "BATTLE_TERMINAL";
        public const string SessionPaused = "SESSION_PAUSED";
        public const string SessionResumed = "SESSION_RESUMED";
        public const string SessionReset = "SESSION_RESET";
        public const string Pool15EffectAccepted = "POOL15_EFFECT_ACCEPTED";
        public const string Pool15EffectNotExecuted =
            "POOL15_EFFECT_NOT_EXECUTED";
        public const string ActionProgressMutated =
            "ACTION_PROGRESS_MUTATED";
        public const string EnemyActionUnsupported =
            "ENEMY_ACTION_UNSUPPORTED";
        public const string BoneSwapTelegraphed =
            "BONE_SWAP_TELEGRAPHED";
        public const string BoneSwapPendingLatest =
            "BONE_SWAP_PENDING_LATEST";
        public const string BoneSwapReturnAccepted =
            "BONE_SWAP_RETURN_ACCEPTED";
        public const string BoneSwapRecoveryStarted =
            "BONE_SWAP_RECOVERY_STARTED";
        public const string NianResourceChanged =
            "NIAN_RESOURCE_CHANGED";
        public const string StatusApplied = "STATUS_APPLIED";
        public const string StatusRefreshed = "STATUS_REFRESHED";
        public const string StatusTickAccepted = "STATUS_TICK_ACCEPTED";
        public const string StatusRemoved = "STATUS_REMOVED";

        public static readonly IReadOnlyList<string> All = Array.AsReadOnly(
            new[]
            {
                SessionStarted,
                ActorSpawnVisible,
                PlayerHpChanged,
                PlayerGuardChanged,
                ActorHpChanged,
                ActorShellChanged,
                ActorShellBroken,
                TargetChanged,
                ItemTriggerScheduled,
                ItemTriggerAccepted,
                EnemyAttackScheduled,
                EnemyAttackAccepted,
                EnemySkillScheduled,
                EnemySkillAccepted,
                EnemyPassiveTriggered,
                HitAccepted,
                DamageFloatPayload,
                ApplicationFeedbackPayload,
                ActorDefeated,
                BattleTerminal,
                SessionPaused,
                SessionResumed,
                SessionReset,
                Pool15EffectAccepted,
                Pool15EffectNotExecuted,
                ActionProgressMutated,
                EnemyActionUnsupported,
                BoneSwapTelegraphed,
                BoneSwapPendingLatest,
                BoneSwapReturnAccepted,
                BoneSwapRecoveryStarted,
                NianResourceChanged,
                StatusApplied,
                StatusRefreshed,
                StatusTickAccepted,
                StatusRemoved
            });
    }

    public static class C1FormalRealtimeBattleNianTransactionKinds
    {
        public const string Spend = "SPEND";
        public const string Refund = "REFUND";
        public const string Generation = "GENERATION";
    }

    public static class C1FormalRealtimeBattleApplicationFeedbackKinds
    {
        public const string Heal = "HEAL";
        public const string Guard = "GUARD";
        public const string Cleanse = "CLEANSE";
        public const string Nian = "NIAN";
        public const string Control = "CONTROL";
        public const string Shell = "SHELL";
    }

    public static class C1FormalRealtimeBattleFeedbackTriggerKinds
    {
        public const string BasicAction = "BASIC_ACTION";
        public const string Skill = "SKILL";
        public const string Passive = "PASSIVE";
        public const string ItemTrigger = "ITEM_TRIGGER";
        public const string StatusTrigger = "STATUS_TRIGGER";
        public const string EnvironmentTrigger = "ENVIRONMENT_TRIGGER";
    }

    public static class C1FormalRealtimeBattleFeedbackDeliveryKinds
    {
        public const string Instant = "INSTANT";
        public const string Periodic = "PERIODIC";
        public const string Delayed = "DELAYED";
        public const string Continuous = "CONTINUOUS";
    }

    public static class C1FormalRealtimeBattleFeedbackResultKinds
    {
        public const string HpDamage = "HP_DAMAGE";
        public const string ShellDamage = "SHELL_DAMAGE";
        public const string GuardDamage = "GUARD_DAMAGE";
        public const string Heal = "HEAL";
        public const string GuardGain = "GUARD_GAIN";
        public const string ShellGain = "SHELL_GAIN";
        public const string BuffApply = "BUFF_APPLY";
        public const string BuffRefresh = "BUFF_REFRESH";
        public const string BuffRemove = "BUFF_REMOVE";
        public const string DebuffApply = "DEBUFF_APPLY";
        public const string DebuffRefresh = "DEBUFF_REFRESH";
        public const string DebuffRemove = "DEBUFF_REMOVE";
        public const string Control = "CONTROL";
        public const string Cleanse = "CLEANSE";
        public const string NianGain = "NIAN_GAIN";
        public const string NianSpend = "NIAN_SPEND";
    }

    public static class C1FormalRealtimeBattleActionProgressOperatorIds
    {
        public const string AdvanceItemTriggerProgressStep =
            "ADVANCE_ITEM_TRIGGER_PROGRESS_STEP";
        public const string DelayEnemyCastProgressStep =
            "DELAY_ENEMY_CAST_PROGRESS_STEP";

        public static readonly IReadOnlyList<string> All = Array.AsReadOnly(
            new[]
            {
                AdvanceItemTriggerProgressStep,
                DelayEnemyCastProgressStep
            });
    }

    public static class C1FormalRealtimeBattleScheduledActionKinds
    {
        public const string ItemTrigger = "ITEM_TRIGGER";
        public const string ItemUnlitDiagnostic = "ITEM_NOT_EXECUTED_UNLIT";
        public const string EnemyBasicWave = "ENEMY_BASIC_WAVE";
        public const string EnemySkill = "ENEMY_SKILL";
        public const string EnemySkillStatusTick =
            "ENEMY_SKILL_STATUS_TICK";
        public const string EnemySkillStatusExpire =
            "ENEMY_SKILL_STATUS_EXPIRE";
        public const string Pool15EffectResolve = "POOL15_EFFECT_RESOLVE";
        public const string BoneSwapTelegraphWake =
            "BONE_SWAP_TELEGRAPH_WAKE";
        public const string BoneSwapReturn = "BONE_SWAP_RETURN";
        public const string StatusTick = "STATUS_TICK";
        public const string StatusExpire = "STATUS_EXPIRE";
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleStatusContributionSnapshot
    {
        public C1FormalRealtimeBattleStatusContributionSnapshot(
            string sourceItemInstanceId,
            string sourceBaseItemId,
            int tickPotency,
            long appliedAtBattleTimeMs)
        {
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.sourceBaseItemId = Normalize(sourceBaseItemId);
            this.tickPotency = tickPotency;
            this.appliedAtBattleTimeMs = appliedAtBattleTimeMs;
        }

        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public int tickPotency { get; }
        public long appliedAtBattleTimeMs { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleStatusSnapshot
    {
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattleStatusContributionSnapshot> contributions;

        public C1FormalRealtimeBattleStatusSnapshot(
            string statusKey,
            string statusFamilyKey,
            string targetActorId,
            int targetStableOrder,
            int stackCount,
            int maxStack,
            long nextTickAtBattleTimeMs,
            long expireAtBattleTimeMs,
            IEnumerable<C1FormalRealtimeBattleStatusContributionSnapshot>
                contributions)
        {
            this.statusKey = Normalize(statusKey);
            this.statusFamilyKey = Normalize(statusFamilyKey);
            this.targetActorId = Normalize(targetActorId);
            this.targetStableOrder = targetStableOrder;
            this.stackCount = stackCount;
            this.maxStack = maxStack;
            this.nextTickAtBattleTimeMs = nextTickAtBattleTimeMs;
            this.expireAtBattleTimeMs = expireAtBattleTimeMs;
            this.contributions = Array.AsReadOnly((contributions
                ?? Enumerable.Empty<
                    C1FormalRealtimeBattleStatusContributionSnapshot>())
                .ToArray());
        }

        public string statusKey { get; }
        public string statusFamilyKey { get; }
        public string targetActorId { get; }
        public int targetStableOrder { get; }
        public int stackCount { get; }
        public int maxStack { get; }
        public long nextTickAtBattleTimeMs { get; }
        public long expireAtBattleTimeMs { get; }
        public IReadOnlyList<C1FormalRealtimeBattleStatusContributionSnapshot>
            Contributions => contributions;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleError
    {
        public C1FormalRealtimeBattleError(string errorCode, string message)
        {
            this.errorCode = Normalize(errorCode);
            this.message = Normalize(message);
        }

        public string errorCode { get; }
        public string message { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleNianCapacitySnapshot
    {
        public C1FormalRealtimeBattleNianCapacitySnapshot(
            string productContext,
            string resourceKey,
            int initialNian,
            int maxNian,
            int generationAmount,
            long generationIntervalMilliseconds,
            string sourceRevision,
            string sourceProfileId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string stablePlacementId,
            int anchorX,
            int anchorY,
            string itemSessionCanonicalSignature,
            string arrangementCanonicalSignature,
            string itemSystemCanonicalSignature)
        {
            schemaId = C1FormalRealtimeBattleSessionContract.NianCapacitySchemaId;
            this.productContext = Normalize(productContext);
            this.resourceKey = Normalize(resourceKey);
            this.initialNian = initialNian;
            this.maxNian = maxNian;
            this.generationAmount = generationAmount;
            this.generationIntervalMilliseconds =
                generationIntervalMilliseconds;
            this.sourceRevision = Normalize(sourceRevision);
            this.sourceProfileId = Normalize(sourceProfileId);
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.sourceBaseItemId = Normalize(sourceBaseItemId);
            this.stablePlacementId = Normalize(stablePlacementId);
            this.anchorX = anchorX;
            this.anchorY = anchorY;
            this.itemSessionCanonicalSignature = Normalize(
                itemSessionCanonicalSignature);
            this.arrangementCanonicalSignature = Normalize(
                arrangementCanonicalSignature);
            this.itemSystemCanonicalSignature = Normalize(
                itemSystemCanonicalSignature);
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    schemaId,
                    this.productContext,
                    this.resourceKey,
                    initialNian.ToString(CultureInfo.InvariantCulture),
                    maxNian.ToString(CultureInfo.InvariantCulture),
                    this.sourceRevision,
                    this.sourceProfileId,
                    this.sourceItemInstanceId,
                    this.sourceBaseItemId,
                    this.stablePlacementId,
                    anchorX.ToString(CultureInfo.InvariantCulture),
                    anchorY.ToString(CultureInfo.InvariantCulture),
                    this.itemSessionCanonicalSignature,
                    this.arrangementCanonicalSignature,
                    this.itemSystemCanonicalSignature
                }));
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string resourceKey { get; }
        public int initialNian { get; }
        public int maxNian { get; }
        public int generationAmount { get; }
        public long generationIntervalMilliseconds { get; }
        public string sourceRevision { get; }
        public string sourceProfileId { get; }
        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public string stablePlacementId { get; }
        public int anchorX { get; }
        public int anchorY { get; }
        public string itemSessionCanonicalSignature { get; }
        public string arrangementCanonicalSignature { get; }
        public string itemSystemCanonicalSignature { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleItemActionCostSnapshot
    {
        public C1FormalRealtimeBattleItemActionCostSnapshot(
            string sourceItemInstanceId,
            string baseItemId,
            string rarityKey,
            string rarityVersionKey,
            string actionDefinitionId,
            string resourceKey,
            int cost,
            string sourceRevision,
            string sourceProfileRevision,
            string sourceProfileCanonicalSignature,
            string sourceProjectionCanonicalSignature,
            long cadenceMilliseconds,
            long firstOffsetMilliseconds)
        {
            schemaId = C1FormalRealtimeBattleSessionContract
                .ItemActionCostSchemaId;
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.baseItemId = Normalize(baseItemId);
            this.rarityKey = Normalize(rarityKey);
            this.rarityVersionKey = Normalize(rarityVersionKey);
            this.actionDefinitionId = Normalize(actionDefinitionId);
            this.resourceKey = Normalize(resourceKey);
            this.cost = cost;
            this.sourceRevision = Normalize(sourceRevision);
            this.sourceProfileRevision = Normalize(sourceProfileRevision);
            this.sourceProfileCanonicalSignature = Normalize(
                sourceProfileCanonicalSignature);
            this.sourceProjectionCanonicalSignature = Normalize(
                sourceProjectionCanonicalSignature);
            this.cadenceMilliseconds = cadenceMilliseconds;
            this.firstOffsetMilliseconds = firstOffsetMilliseconds;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    schemaId,
                    this.sourceItemInstanceId,
                    this.baseItemId,
                    this.rarityKey,
                    this.rarityVersionKey,
                    this.actionDefinitionId,
                    this.resourceKey,
                    cost.ToString(CultureInfo.InvariantCulture),
                    this.sourceRevision,
                    this.sourceProfileRevision,
                    this.sourceProfileCanonicalSignature,
                    this.sourceProjectionCanonicalSignature,
                    cadenceMilliseconds.ToString(CultureInfo.InvariantCulture),
                    firstOffsetMilliseconds.ToString(CultureInfo.InvariantCulture)
                }));
        }

        public string schemaId { get; }
        public string sourceItemInstanceId { get; }
        public string baseItemId { get; }
        public string rarityKey { get; }
        public string rarityVersionKey { get; }
        public string actionDefinitionId { get; }
        public string resourceKey { get; }
        public int cost { get; }
        public string sourceRevision { get; }
        public string sourceProfileRevision { get; }
        public string sourceProfileCanonicalSignature { get; }
        public string sourceProjectionCanonicalSignature { get; }
        public long cadenceMilliseconds { get; }
        public long firstOffsetMilliseconds { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleItemFactSnapshot
    {
        public C1FormalRealtimeBattleItemFactSnapshot(
            string sourceId,
            string baseItemId,
            string rarityVersionKey,
            bool isLit,
            int directDamage,
            long cooldownMs,
            string projectionCanonicalSignature,
            string itemCatalogCanonicalSignature)
            : this(
                sourceId,
                sourceId,
                baseItemId,
                rarityVersionKey,
                isLit,
                directDamage,
                cooldownMs,
                projectionCanonicalSignature,
                itemCatalogCanonicalSignature)
        {
        }

        public C1FormalRealtimeBattleItemFactSnapshot(
            string sourceId,
            string itemInstanceId,
            string baseItemId,
            string rarityVersionKey,
            bool isLit,
            int directDamage,
            long cooldownMs,
            string projectionCanonicalSignature,
            string itemCatalogCanonicalSignature)
            : this(
                sourceId,
                itemInstanceId,
                baseItemId,
                rarityVersionKey,
                isLit,
                directDamage,
                cooldownMs,
                projectionCanonicalSignature,
                itemCatalogCanonicalSignature,
                null,
                null)
        {
        }

        public C1FormalRealtimeBattleItemFactSnapshot(
            string sourceId,
            string itemInstanceId,
            string baseItemId,
            string rarityVersionKey,
            bool isLit,
            int directDamage,
            long cooldownMs,
            string projectionCanonicalSignature,
            string itemCatalogCanonicalSignature,
            C1FormalRealtimeBattleItemActionCostSnapshot actionCostFact)
            : this(
                sourceId,
                itemInstanceId,
                baseItemId,
                rarityVersionKey,
                isLit,
                directDamage,
                cooldownMs,
                projectionCanonicalSignature,
                itemCatalogCanonicalSignature,
                actionCostFact,
                null)
        {
        }

        public C1FormalRealtimeBattleItemFactSnapshot(
            string sourceId,
            string itemInstanceId,
            string baseItemId,
            string rarityVersionKey,
            bool isLit,
            int directDamage,
            long cooldownMs,
            string projectionCanonicalSignature,
            string itemCatalogCanonicalSignature,
            C1FormalRealtimeBattleItemActionCostSnapshot actionCostFact,
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot combatFact)
            : this(
                sourceId,
                itemInstanceId,
                baseItemId,
                rarityVersionKey,
                isLit,
                directDamage,
                cooldownMs,
                projectionCanonicalSignature,
                itemCatalogCanonicalSignature,
                actionCostFact,
                combatFact,
                null,
                null)
        {
        }

        public C1FormalRealtimeBattleItemFactSnapshot(
            string sourceId,
            string itemInstanceId,
            string baseItemId,
            string rarityVersionKey,
            bool isLit,
            int directDamage,
            long cooldownMs,
            string projectionCanonicalSignature,
            string itemCatalogCanonicalSignature,
            C1FormalRealtimeBattleItemActionCostSnapshot actionCostFact,
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot combatFact,
            ItemGeneratedInstanceSnapshot generatedInstance,
            CanonicalItemDefinition canonicalDefinition)
            : this(
                sourceId,
                itemInstanceId,
                baseItemId,
                rarityVersionKey,
                isLit,
                directDamage,
                cooldownMs,
                projectionCanonicalSignature,
                itemCatalogCanonicalSignature,
                actionCostFact,
                combatFact,
                generatedInstance,
                canonicalDefinition,
                string.Empty,
                0,
                0,
                0,
                false)
        {
        }

        public C1FormalRealtimeBattleItemFactSnapshot(
            string sourceId,
            string itemInstanceId,
            string baseItemId,
            string rarityVersionKey,
            bool isLit,
            int directDamage,
            long cooldownMs,
            string projectionCanonicalSignature,
            string itemCatalogCanonicalSignature,
            C1FormalRealtimeBattleItemActionCostSnapshot actionCostFact,
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot combatFact,
            ItemGeneratedInstanceSnapshot generatedInstance,
            CanonicalItemDefinition canonicalDefinition,
            string placementId,
            int anchorX,
            int anchorY,
            int rotation,
            bool isDirectLit)
        {
            this.sourceId = Normalize(sourceId);
            this.itemInstanceId = Normalize(itemInstanceId);
            this.baseItemId = Normalize(baseItemId);
            this.rarityVersionKey = Normalize(rarityVersionKey);
            this.isLit = isLit;
            this.directDamage = directDamage;
            this.cooldownMs = cooldownMs;
            this.projectionCanonicalSignature =
                Normalize(projectionCanonicalSignature);
            this.itemCatalogCanonicalSignature =
                Normalize(itemCatalogCanonicalSignature);
            this.actionCostFact = actionCostFact;
            this.combatFact = combatFact;
            this.generatedInstance = generatedInstance;
            this.canonicalDefinition = canonicalDefinition;
            this.placementId = Normalize(placementId);
            this.anchorX = anchorX;
            this.anchorY = anchorY;
            this.rotation = rotation;
            this.isDirectLit = isDirectLit;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    this.sourceId,
                    this.itemInstanceId,
                    this.baseItemId,
                    this.rarityVersionKey,
                    isLit ? "lit" : "unlit",
                    directDamage.ToString(CultureInfo.InvariantCulture),
                    cooldownMs.ToString(CultureInfo.InvariantCulture),
                    this.projectionCanonicalSignature,
                    this.itemCatalogCanonicalSignature,
                    actionCostFact == null
                        ? string.Empty
                        : actionCostFact.canonicalSignature,
                    combatFact == null
                        ? string.Empty
                        : combatFact.canonicalSignature
                }));
        }

        public string sourceId { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string rarityVersionKey { get; }
        public bool isLit { get; }
        public int directDamage { get; }
        public long cooldownMs { get; }
        public string projectionCanonicalSignature { get; }
        public string itemCatalogCanonicalSignature { get; }
        public C1FormalRealtimeBattleItemActionCostSnapshot actionCostFact
        {
            get;
        }
        public C1FormalRealtimeBattlePool15ActiveItemFactSnapshot combatFact
        {
            get;
        }
        public ItemGeneratedInstanceSnapshot generatedInstance { get; }
        public CanonicalItemDefinition canonicalDefinition { get; }
        public string placementId { get; }
        public int anchorX { get; }
        public int anchorY { get; }
        public int rotation { get; }
        public bool isDirectLit { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattlePool15ItemRow
    {
        public C1FormalRealtimeBattlePool15ItemRow(
            string itemInstanceId,
            string baseItemId,
            bool isPlaced,
            bool? isLit,
            string descriptorCanonicalSignature,
            string candidateProfileCanonicalSignature)
        {
            this.itemInstanceId = Normalize(itemInstanceId);
            this.baseItemId = Normalize(baseItemId);
            this.isPlaced = isPlaced;
            this.isLit = isLit;
            this.descriptorCanonicalSignature = Normalize(
                descriptorCanonicalSignature);
            this.candidateProfileCanonicalSignature = Normalize(
                candidateProfileCanonicalSignature);
            canonicalSignature = ComputeCanonicalSignature(
                this.itemInstanceId,
                this.baseItemId,
                isPlaced,
                isLit,
                this.descriptorCanonicalSignature,
                this.candidateProfileCanonicalSignature);
        }

        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public bool isPlaced { get; }
        public bool? isLit { get; }
        public string descriptorCanonicalSignature { get; }
        public string candidateProfileCanonicalSignature { get; }
        public string canonicalSignature { get; }

        internal static string ComputeCanonicalSignature(
            string itemInstanceId,
            string baseItemId,
            bool isPlaced,
            bool? isLit,
            string descriptorCanonicalSignature,
            string candidateProfileCanonicalSignature)
        {
            return C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    Normalize(itemInstanceId),
                    Normalize(baseItemId),
                    isPlaced ? "placed" : "tray",
                    isLit.HasValue
                        ? isLit.Value ? "lit" : "unlit"
                        : "absent",
                    Normalize(descriptorCanonicalSignature),
                    Normalize(candidateProfileCanonicalSignature)
                }));
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattlePool15ItemSnapshot
    {
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattlePool15ItemRow> itemRowsValue;

        public C1FormalRealtimeBattlePool15ItemSnapshot(
            string schemaId,
            string productContext,
            string entitlementCanonicalSignature,
            string poolCanonicalSignature,
            string sourceItemCatalogCanonicalSignature,
            string candidateProfileId,
            string candidateDataMaturity,
            string candidateProfileCanonicalSignature,
            IEnumerable<C1FormalRealtimeBattlePool15ItemRow> itemRows)
        {
            this.schemaId = Normalize(schemaId);
            this.productContext = Normalize(productContext);
            this.entitlementCanonicalSignature = Normalize(
                entitlementCanonicalSignature);
            this.poolCanonicalSignature = Normalize(poolCanonicalSignature);
            this.sourceItemCatalogCanonicalSignature = Normalize(
                sourceItemCatalogCanonicalSignature);
            this.candidateProfileId = Normalize(candidateProfileId);
            this.candidateDataMaturity = Normalize(candidateDataMaturity);
            this.candidateProfileCanonicalSignature = Normalize(
                candidateProfileCanonicalSignature);
            itemRowsValue = Array.AsReadOnly((itemRows
                ?? Enumerable.Empty<C1FormalRealtimeBattlePool15ItemRow>())
                .ToArray());
            StringBuilder value = new StringBuilder(string.Join("|", new[]
            {
                this.schemaId,
                this.productContext,
                this.entitlementCanonicalSignature,
                this.poolCanonicalSignature,
                this.sourceItemCatalogCanonicalSignature,
                this.candidateProfileId,
                this.candidateDataMaturity,
                this.candidateProfileCanonicalSignature
            }));
            foreach (C1FormalRealtimeBattlePool15ItemRow row in itemRowsValue)
            {
                value.Append("\nROW|").Append(
                    row == null ? "null" : row.canonicalSignature);
            }

            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                value.ToString());
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string entitlementCanonicalSignature { get; }
        public string poolCanonicalSignature { get; }
        public string sourceItemCatalogCanonicalSignature { get; }
        public string candidateProfileId { get; }
        public string candidateDataMaturity { get; }
        public string candidateProfileCanonicalSignature { get; }
        public IReadOnlyList<C1FormalRealtimeBattlePool15ItemRow> itemRows =>
            itemRowsValue;
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleCombatComponentSnapshot
    {
        public C1FormalRealtimeBattleCombatComponentSnapshot(
            string componentId,
            string operationId,
            string targetStatId,
            long signedAmount,
            string nativeUnitId,
            long absoluteCapPerAcceptedTrigger,
            string supportStatus,
            string maturity,
            string dependencyId,
            string sourceLineage,
            string sourceCanonicalSignature)
        {
            this.componentId = Normalize(componentId);
            this.operationId = Normalize(operationId);
            this.targetStatId = Normalize(targetStatId);
            this.signedAmount = signedAmount;
            this.nativeUnitId = Normalize(nativeUnitId);
            this.absoluteCapPerAcceptedTrigger =
                absoluteCapPerAcceptedTrigger;
            this.supportStatus = Normalize(supportStatus);
            this.maturity = Normalize(maturity);
            this.dependencyId = Normalize(dependencyId);
            this.sourceLineage = Normalize(sourceLineage);
            this.sourceCanonicalSignature = Normalize(
                sourceCanonicalSignature);
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    this.componentId,
                    this.operationId,
                    this.targetStatId,
                    signedAmount.ToString(CultureInfo.InvariantCulture),
                    this.nativeUnitId,
                    absoluteCapPerAcceptedTrigger.ToString(
                        CultureInfo.InvariantCulture),
                    this.supportStatus,
                    this.maturity,
                    this.dependencyId,
                    this.sourceLineage,
                    this.sourceCanonicalSignature
                }));
        }

        public string componentId { get; }
        public string operationId { get; }
        public string targetStatId { get; }
        public long signedAmount { get; }
        public string nativeUnitId { get; }
        public long absoluteCapPerAcceptedTrigger { get; }
        public string supportStatus { get; }
        public string maturity { get; }
        public string dependencyId { get; }
        public string sourceLineage { get; }
        public string sourceCanonicalSignature { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattlePool15ActiveItemFactSnapshot
    {
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattleCombatComponentSnapshot> componentsValue;

        public static C1FormalRealtimeBattlePool15ActiveItemFactSnapshot
            FromImmutableSourceFact(
                string itemInstanceId,
                string baseItemId,
                string descriptorCanonicalSignature,
                string candidateProfileCanonicalSignature,
                string sourceFactCanonicalSignature)
        {
            return new C1FormalRealtimeBattlePool15ActiveItemFactSnapshot(
                itemInstanceId,
                baseItemId,
                descriptorCanonicalSignature,
                candidateProfileCanonicalSignature,
                sourceFactCanonicalSignature);
        }

        public static C1FormalRealtimeBattlePool15ActiveItemFactSnapshot
            FromImmutableCombatFact(
                string itemInstanceId,
                string baseItemId,
                string sourceFactCanonicalSignature,
                string combatFactCanonicalSignature,
                string familyId,
                string familyVariantId,
                string effectCategoryId,
                string triggerId,
                string activationConditionId,
                string targetScopeId,
                string cueIdentity,
                string cadenceKind,
                long? cadenceMilliseconds,
                long? firstOffsetMilliseconds,
                string minimumLayoutTier,
                string maturity,
                string sourceProfileCanonicalSignature,
                IEnumerable<C1FormalRealtimeBattleCombatComponentSnapshot>
                    components)
        {
            return new C1FormalRealtimeBattlePool15ActiveItemFactSnapshot(
                itemInstanceId,
                baseItemId,
                string.Empty,
                string.Empty,
                sourceFactCanonicalSignature,
                combatFactCanonicalSignature,
                familyId,
                familyVariantId,
                effectCategoryId,
                triggerId,
                activationConditionId,
                targetScopeId,
                cueIdentity,
                cadenceKind,
                cadenceMilliseconds,
                firstOffsetMilliseconds,
                minimumLayoutTier,
                maturity,
                sourceProfileCanonicalSignature,
                components);
        }

        internal C1FormalRealtimeBattlePool15ActiveItemFactSnapshot(
            string itemInstanceId,
            string baseItemId,
            string descriptorCanonicalSignature,
            string candidateProfileCanonicalSignature,
            string sourceFactCanonicalSignature)
            : this(
                itemInstanceId,
                baseItemId,
                descriptorCanonicalSignature,
                candidateProfileCanonicalSignature,
                sourceFactCanonicalSignature,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                null,
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<
                    C1FormalRealtimeBattleCombatComponentSnapshot>())
        {
        }

        private C1FormalRealtimeBattlePool15ActiveItemFactSnapshot(
            string itemInstanceId,
            string baseItemId,
            string descriptorCanonicalSignature,
            string candidateProfileCanonicalSignature,
            string sourceFactCanonicalSignature,
            string combatFactCanonicalSignature,
            string familyId,
            string familyVariantId,
            string effectCategoryId,
            string triggerId,
            string activationConditionId,
            string targetScopeId,
            string cueIdentity,
            string cadenceKind,
            long? cadenceMilliseconds,
            long? firstOffsetMilliseconds,
            string minimumLayoutTier,
            string maturity,
            string sourceProfileCanonicalSignature,
            IEnumerable<C1FormalRealtimeBattleCombatComponentSnapshot>
                components)
        {
            this.itemInstanceId = Normalize(itemInstanceId);
            this.baseItemId = Normalize(baseItemId);
            this.descriptorCanonicalSignature = Normalize(
                descriptorCanonicalSignature);
            this.candidateProfileCanonicalSignature = Normalize(
                candidateProfileCanonicalSignature);
            this.sourceFactCanonicalSignature = Normalize(
                sourceFactCanonicalSignature);
            this.combatFactCanonicalSignature = Normalize(
                combatFactCanonicalSignature);
            this.familyId = Normalize(familyId);
            this.familyVariantId = Normalize(familyVariantId);
            this.effectCategoryId = Normalize(effectCategoryId);
            this.triggerId = Normalize(triggerId);
            this.activationConditionId = Normalize(activationConditionId);
            this.targetScopeId = Normalize(targetScopeId);
            this.cueIdentity = Normalize(cueIdentity);
            this.cadenceKind = Normalize(cadenceKind);
            this.cadenceMilliseconds = cadenceMilliseconds;
            this.firstOffsetMilliseconds = firstOffsetMilliseconds;
            this.minimumLayoutTier = Normalize(minimumLayoutTier);
            this.maturity = Normalize(maturity);
            this.sourceProfileCanonicalSignature = Normalize(
                sourceProfileCanonicalSignature);
            componentsValue = Array.AsReadOnly((components
                ?? Enumerable.Empty<
                    C1FormalRealtimeBattleCombatComponentSnapshot>())
                .Where(row => row != null)
                .ToArray());
            StringBuilder canonical = new StringBuilder(string.Join("|", new[]
            {
                this.itemInstanceId,
                this.baseItemId,
                this.descriptorCanonicalSignature,
                this.candidateProfileCanonicalSignature,
                this.sourceFactCanonicalSignature,
                this.combatFactCanonicalSignature,
                this.familyId,
                this.familyVariantId,
                this.effectCategoryId,
                this.triggerId,
                this.activationConditionId,
                this.targetScopeId,
                this.cueIdentity,
                this.cadenceKind,
                cadenceMilliseconds.HasValue
                    ? cadenceMilliseconds.Value.ToString(
                        CultureInfo.InvariantCulture)
                    : "absent",
                firstOffsetMilliseconds.HasValue
                    ? firstOffsetMilliseconds.Value.ToString(
                        CultureInfo.InvariantCulture)
                    : "absent",
                this.minimumLayoutTier,
                this.maturity,
                this.sourceProfileCanonicalSignature
            }));
            foreach (C1FormalRealtimeBattleCombatComponentSnapshot component in
                     componentsValue)
            {
                canonical.Append("\nCOMPONENT|").Append(
                    component.canonicalSignature);
            }
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                canonical.ToString());
        }

        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string descriptorCanonicalSignature { get; }
        public string candidateProfileCanonicalSignature { get; }
        public string sourceFactCanonicalSignature { get; }
        public string combatFactCanonicalSignature { get; }
        public string familyId { get; }
        public string familyVariantId { get; }
        public string effectCategoryId { get; }
        public string triggerId { get; }
        public string activationConditionId { get; }
        public string targetScopeId { get; }
        public string cueIdentity { get; }
        public string cadenceKind { get; }
        public long? cadenceMilliseconds { get; }
        public long? firstOffsetMilliseconds { get; }
        public string minimumLayoutTier { get; }
        public string maturity { get; }
        public string sourceProfileCanonicalSignature { get; }
        public IReadOnlyList<C1FormalRealtimeBattleCombatComponentSnapshot>
            components => componentsValue;
        public string canonicalSignature { get; }

        internal bool HasImmutableCombatFact =>
            combatFactCanonicalSignature.Length > 0
            && triggerId.Length > 0
            && activationConditionId.Length > 0
            && componentsValue.Count > 0;

        internal bool HasValidPlacedLitSourceFactCanonical => string.Equals(
            sourceFactCanonicalSignature,
            HasImmutableCombatFact
                ? C1FormalRealtimeBattleCanonical.Hash(string.Join("|", new[]
                {
                    itemInstanceId,
                    baseItemId,
                    "placed",
                    "lit",
                    combatFactCanonicalSignature
                }))
                : C1FormalRealtimeBattlePool15ItemRow.ComputeCanonicalSignature(
                    itemInstanceId,
                    baseItemId,
                    true,
                    true,
                    descriptorCanonicalSignature,
                    candidateProfileCanonicalSignature),
            StringComparison.Ordinal);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleActionProgressRelationSnapshot
    {
        private readonly ReadOnlyCollection<string>
            eligibleTargetSourceIdsValue;

        public C1FormalRealtimeBattleActionProgressRelationSnapshot(
            string itemBattleInputCanonical,
            string arrangementCanonicalSignature,
            string sourceItemInstanceId,
            string relationId,
            IEnumerable<string> eligibleTargetSourceIds)
        {
            this.itemBattleInputCanonical = Normalize(
                itemBattleInputCanonical);
            this.arrangementCanonicalSignature = Normalize(
                arrangementCanonicalSignature);
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.relationId = Normalize(relationId);
            eligibleTargetSourceIdsValue = Array.AsReadOnly((
                    eligibleTargetSourceIds ?? Enumerable.Empty<string>())
                .Select(Normalize)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            StringBuilder value = new StringBuilder(string.Join("|", new[]
            {
                C1FormalRealtimeBattleSessionContract
                    .ActionProgressRelationSchemaId,
                this.itemBattleInputCanonical,
                this.arrangementCanonicalSignature,
                this.sourceItemInstanceId,
                this.relationId
            }));
            foreach (string targetSourceId in eligibleTargetSourceIdsValue)
            {
                value.Append("\nTARGET|").Append(targetSourceId);
            }

            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                value.ToString());
        }

        public string itemBattleInputCanonical { get; }
        public string arrangementCanonicalSignature { get; }
        public string sourceItemInstanceId { get; }
        public string relationId { get; }
        public IReadOnlyList<string> eligibleTargetSourceIds =>
            eligibleTargetSourceIdsValue;
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleActionProgressMutationSnapshot
    {
        internal C1FormalRealtimeBattleActionProgressMutationSnapshot(
            string operatorId,
            string eventId,
            string sourceItemInstanceId,
            string targetActionId,
            string targetActionKind,
            string targetSourceId,
            string nativeUnitId,
            int requestedStepCount,
            int appliedStepCount,
            long beforeDueBattleTimeMs,
            long afterDueBattleTimeMs,
            long sourceNativeIntervalMs,
            string relationCanonicalSignature)
        {
            this.operatorId = Normalize(operatorId);
            this.eventId = Normalize(eventId);
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.targetActionId = Normalize(targetActionId);
            this.targetActionKind = Normalize(targetActionKind);
            this.targetSourceId = Normalize(targetSourceId);
            this.nativeUnitId = Normalize(nativeUnitId);
            this.requestedStepCount = requestedStepCount;
            this.appliedStepCount = appliedStepCount;
            this.beforeDueBattleTimeMs = beforeDueBattleTimeMs;
            this.afterDueBattleTimeMs = afterDueBattleTimeMs;
            this.sourceNativeIntervalMs = sourceNativeIntervalMs;
            this.relationCanonicalSignature = Normalize(
                relationCanonicalSignature);
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    this.operatorId,
                    this.eventId,
                    this.sourceItemInstanceId,
                    this.targetActionId,
                    this.targetActionKind,
                    this.targetSourceId,
                    this.nativeUnitId,
                    requestedStepCount.ToString(CultureInfo.InvariantCulture),
                    appliedStepCount.ToString(CultureInfo.InvariantCulture),
                    beforeDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture),
                    afterDueBattleTimeMs.ToString(
                        CultureInfo.InvariantCulture),
                    sourceNativeIntervalMs.ToString(
                        CultureInfo.InvariantCulture),
                    this.relationCanonicalSignature
                }));
        }

        public string operatorId { get; }
        public string eventId { get; }
        public string sourceItemInstanceId { get; }
        public string targetActionId { get; }
        public string targetActionKind { get; }
        public string targetSourceId { get; }
        public string nativeUnitId { get; }
        public int requestedStepCount { get; }
        public int appliedStepCount { get; }
        public long beforeDueBattleTimeMs { get; }
        public long afterDueBattleTimeMs { get; }
        public long sourceNativeIntervalMs { get; }
        public string relationCanonicalSignature { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattlePool15EventRequest
    {
        private readonly ReadOnlyCollection<string> conditionIdsValue;
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattleActionProgressRelationSnapshot>
            progressRelationsValue;

        public C1FormalRealtimeBattlePool15EventRequest(
            string schemaId,
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            string eventId,
            long expectedBattleTimeMs,
            string triggerId,
            IEnumerable<string> satisfiedConditionIds)
            : this(
                schemaId,
                sessionId,
                sessionToken,
                expectedSessionGeneration,
                expectedResetGeneration,
                eventId,
                expectedBattleTimeMs,
                triggerId,
                satisfiedConditionIds,
                Array.Empty<
                    C1FormalRealtimeBattleActionProgressRelationSnapshot>())
        {
        }

        public C1FormalRealtimeBattlePool15EventRequest(
            string schemaId,
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            string eventId,
            long expectedBattleTimeMs,
            string triggerId,
            IEnumerable<string> satisfiedConditionIds,
            IEnumerable<C1FormalRealtimeBattleActionProgressRelationSnapshot>
                progressRelations)
        {
            this.schemaId = Normalize(schemaId);
            this.sessionId = Normalize(sessionId);
            this.sessionToken = Normalize(sessionToken);
            this.expectedSessionGeneration = expectedSessionGeneration;
            this.expectedResetGeneration = expectedResetGeneration;
            this.eventId = Normalize(eventId);
            this.expectedBattleTimeMs = expectedBattleTimeMs;
            this.triggerId = Normalize(triggerId);
            conditionIdsValue = Array.AsReadOnly((satisfiedConditionIds
                ?? Enumerable.Empty<string>())
                .Select(Normalize)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            progressRelationsValue = Array.AsReadOnly((progressRelations
                    ?? Enumerable.Empty<
                        C1FormalRealtimeBattleActionProgressRelationSnapshot>())
                .OrderBy(
                    row => row == null
                        ? string.Empty
                        : row.sourceItemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(
                    row => row == null ? string.Empty : row.canonicalSignature,
                    StringComparer.Ordinal)
                .ToArray());
            StringBuilder value = new StringBuilder(string.Join("|", new[]
            {
                this.schemaId,
                this.sessionId,
                this.sessionToken,
                expectedSessionGeneration.ToString(
                    CultureInfo.InvariantCulture),
                expectedResetGeneration.ToString(
                    CultureInfo.InvariantCulture),
                this.eventId,
                expectedBattleTimeMs.ToString(CultureInfo.InvariantCulture),
                this.triggerId,
                string.Join(",", conditionIdsValue)
            }));
            foreach (C1FormalRealtimeBattleActionProgressRelationSnapshot row in
                     progressRelationsValue)
            {
                value.Append("\nRELATION|").Append(
                    row == null ? "null" : row.canonicalSignature);
            }

            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                value.ToString());
        }

        public string schemaId { get; }
        public string sessionId { get; }
        public string sessionToken { get; }
        public long expectedSessionGeneration { get; }
        public long expectedResetGeneration { get; }
        public string eventId { get; }
        public long expectedBattleTimeMs { get; }
        public string triggerId { get; }
        public IReadOnlyList<string> satisfiedConditionIds => conditionIdsValue;
        public IReadOnlyList<
            C1FormalRealtimeBattleActionProgressRelationSnapshot>
            progressRelations => progressRelationsValue;
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleEffectStateValueSnapshot
    {
        internal C1FormalRealtimeBattleEffectStateValueSnapshot(
            string stateKey,
            string targetIdentity,
            string nativeUnitId,
            long value)
        {
            this.stateKey = Normalize(stateKey);
            this.targetIdentity = Normalize(targetIdentity);
            this.nativeUnitId = Normalize(nativeUnitId);
            this.value = value;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    this.stateKey,
                    this.targetIdentity,
                    this.nativeUnitId,
                    value.ToString(CultureInfo.InvariantCulture)
                }));
        }

        public string stateKey { get; }
        public string targetIdentity { get; }
        public string nativeUnitId { get; }
        public long value { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleEffectStateSnapshot
    {
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattleEffectStateValueSnapshot> valuesValue;

        internal C1FormalRealtimeBattleEffectStateSnapshot(
            long playerGuard,
            IEnumerable<C1FormalRealtimeBattleEffectStateValueSnapshot> values)
        {
            this.playerGuard = playerGuard;
            valuesValue = Array.AsReadOnly((values
                ?? Enumerable.Empty<
                    C1FormalRealtimeBattleEffectStateValueSnapshot>())
                .Where(row => row != null)
                .OrderBy(row => row.stateKey, StringComparer.Ordinal)
                .ThenBy(row => row.targetIdentity, StringComparer.Ordinal)
                .ThenBy(row => row.nativeUnitId, StringComparer.Ordinal)
                .ToArray());
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                playerGuard.ToString(CultureInfo.InvariantCulture) + "|"
                + string.Join(",", valuesValue.Select(
                    row => row.canonicalSignature)));
        }

        public long playerGuard { get; }
        public IReadOnlyList<C1FormalRealtimeBattleEffectStateValueSnapshot>
            values => valuesValue;
        public string canonicalSignature { get; }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleEffectApplicationSnapshot
    {
        internal C1FormalRealtimeBattleEffectApplicationSnapshot(
            long sequence,
            string eventId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string sourceFactCanonicalSignature,
            string familyId,
            string familyVariantId,
            string triggerId,
            string conditionId,
            string operationId,
            string targetStatId,
            string targetScopeId,
            string targetIdentity,
            int targetStableOrder,
            long requestedAmount,
            long appliedAmount,
            string nativeUnitId,
            long? percentageBaseValueUnits,
            long targetBefore,
            long targetAfter,
            string candidateProfileId,
            string candidateDataMaturity,
            string candidateProfileCanonicalSignature,
            long triggerBattleTimeMs,
            long resolveBattleTimeMs,
            string cueIdentity,
            string foundationApplicationCanonicalSignature)
        {
            this.sequence = sequence;
            this.eventId = Normalize(eventId);
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.sourceBaseItemId = Normalize(sourceBaseItemId);
            this.sourceFactCanonicalSignature = Normalize(
                sourceFactCanonicalSignature);
            this.familyId = Normalize(familyId);
            this.familyVariantId = Normalize(familyVariantId);
            this.triggerId = Normalize(triggerId);
            this.conditionId = Normalize(conditionId);
            this.operationId = Normalize(operationId);
            this.targetStatId = Normalize(targetStatId);
            this.targetScopeId = Normalize(targetScopeId);
            this.targetIdentity = Normalize(targetIdentity);
            this.targetStableOrder = targetStableOrder;
            this.requestedAmount = requestedAmount;
            this.appliedAmount = appliedAmount;
            this.nativeUnitId = Normalize(nativeUnitId);
            this.percentageBaseValueUnits = percentageBaseValueUnits;
            this.targetBefore = targetBefore;
            this.targetAfter = targetAfter;
            this.candidateProfileId = Normalize(candidateProfileId);
            this.candidateDataMaturity = Normalize(candidateDataMaturity);
            this.candidateProfileCanonicalSignature = Normalize(
                candidateProfileCanonicalSignature);
            this.triggerBattleTimeMs = triggerBattleTimeMs;
            this.resolveBattleTimeMs = resolveBattleTimeMs;
            this.cueIdentity = Normalize(cueIdentity);
            this.foundationApplicationCanonicalSignature = Normalize(
                foundationApplicationCanonicalSignature);
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    sequence.ToString(CultureInfo.InvariantCulture),
                    this.eventId,
                    this.sourceItemInstanceId,
                    this.sourceBaseItemId,
                    this.sourceFactCanonicalSignature,
                    this.familyId,
                    this.familyVariantId,
                    this.triggerId,
                    this.conditionId,
                    this.operationId,
                    this.targetStatId,
                    this.targetScopeId,
                    this.targetIdentity,
                    targetStableOrder.ToString(CultureInfo.InvariantCulture),
                    requestedAmount.ToString(CultureInfo.InvariantCulture),
                    appliedAmount.ToString(CultureInfo.InvariantCulture),
                    this.nativeUnitId,
                    percentageBaseValueUnits.HasValue
                        ? percentageBaseValueUnits.Value.ToString(
                            CultureInfo.InvariantCulture)
                        : "absent",
                    targetBefore.ToString(CultureInfo.InvariantCulture),
                    targetAfter.ToString(CultureInfo.InvariantCulture),
                    this.candidateProfileId,
                    this.candidateDataMaturity,
                    this.candidateProfileCanonicalSignature,
                    triggerBattleTimeMs.ToString(CultureInfo.InvariantCulture),
                    resolveBattleTimeMs.ToString(CultureInfo.InvariantCulture),
                    this.cueIdentity,
                    this.foundationApplicationCanonicalSignature
                }));
        }

        public long sequence { get; }
        public string eventId { get; }
        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public string sourceFactCanonicalSignature { get; }
        public string familyId { get; }
        public string familyVariantId { get; }
        public string triggerId { get; }
        public string conditionId { get; }
        public string operationId { get; }
        public string targetStatId { get; }
        public string targetScopeId { get; }
        public string targetIdentity { get; }
        public int targetStableOrder { get; }
        public long requestedAmount { get; }
        public long appliedAmount { get; }
        public string nativeUnitId { get; }
        public long? percentageBaseValueUnits { get; }
        public long targetBefore { get; }
        public long targetAfter { get; }
        public string candidateProfileId { get; }
        public string candidateDataMaturity { get; }
        public string candidateProfileCanonicalSignature { get; }
        public long triggerBattleTimeMs { get; }
        public long resolveBattleTimeMs { get; }
        public string cueIdentity { get; }
        public string foundationApplicationCanonicalSignature { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleEffectCue
    {
        internal C1FormalRealtimeBattleEffectCue(
            long cueSequence,
            C1FormalRealtimeBattleEffectApplicationSnapshot application)
        {
            this.cueSequence = cueSequence;
            this.application = application;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                cueSequence.ToString(CultureInfo.InvariantCulture) + "|"
                + (application == null
                    ? string.Empty
                    : application.canonicalSignature));
        }

        public long cueSequence { get; }
        public C1FormalRealtimeBattleEffectApplicationSnapshot application { get; }
        public string cueIdentity => application == null
            ? string.Empty
            : application.cueIdentity;
        public string sourceBaseItemId => application == null
            ? string.Empty
            : application.sourceBaseItemId;
        public string sourceItemInstanceId => application == null
            ? string.Empty
            : application.sourceItemInstanceId;
        public string sourceFactCanonicalSignature => application == null
            ? string.Empty
            : application.sourceFactCanonicalSignature;
        public string familyId => application == null
            ? string.Empty
            : application.familyId;
        public string familyVariantId => application == null
            ? string.Empty
            : application.familyVariantId;
        public string triggerId => application == null
            ? string.Empty
            : application.triggerId;
        public string conditionId => application == null
            ? string.Empty
            : application.conditionId;
        public string operationId => application == null
            ? string.Empty
            : application.operationId;
        public string targetStatId => application == null
            ? string.Empty
            : application.targetStatId;
        public string targetScopeId => application == null
            ? string.Empty
            : application.targetScopeId;
        public string targetIdentity => application == null
            ? string.Empty
            : application.targetIdentity;
        public long requestedAmount => application == null
            ? 0L
            : application.requestedAmount;
        public long appliedAmount => application == null
            ? 0L
            : application.appliedAmount;
        public string nativeUnitId => application == null
            ? string.Empty
            : application.nativeUnitId;
        public long targetBefore => application == null
            ? 0L
            : application.targetBefore;
        public long targetAfter => application == null
            ? 0L
            : application.targetAfter;
        public string candidateProfileId => application == null
            ? string.Empty
            : application.candidateProfileId;
        public long triggerBattleTimeMs => application == null
            ? 0L
            : application.triggerBattleTimeMs;
        public long resolveBattleTimeMs => application == null
            ? 0L
            : application.resolveBattleTimeMs;
        public string candidateDataMaturity => application == null
            ? string.Empty
            : application.candidateDataMaturity;
        public string candidateProfileCanonicalSignature => application == null
            ? string.Empty
            : application.candidateProfileCanonicalSignature;
        public string applicationCanonicalSignature => application == null
            ? string.Empty
            : application.canonicalSignature;
        public string canonicalSignature { get; }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattlePool15EventResult
    {
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattleEffectApplicationSnapshot> applicationsValue;
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattleEffectCue> emittedCuesValue;

        internal C1FormalRealtimeBattlePool15EventResult(
            bool accepted,
            C1FormalRealtimeBattleError error,
            string eventId,
            int acceptedSourceCount,
            int scheduledSourceCount,
            C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot,
            IEnumerable<C1FormalRealtimeBattleEffectApplicationSnapshot>
                applications,
            IEnumerable<C1FormalRealtimeBattleEffectCue> emittedCues)
        {
            schemaId = C1FormalRealtimeBattleSessionContract
                .Pool15EventResultSchemaId;
            this.accepted = accepted;
            this.error = error ?? new C1FormalRealtimeBattleError(
                C1FormalRealtimeBattleErrorCodes.None,
                string.Empty);
            errorCode = this.error.errorCode;
            this.eventId = Normalize(eventId);
            this.acceptedSourceCount = acceptedSourceCount;
            this.scheduledSourceCount = scheduledSourceCount;
            this.stateSnapshot = stateSnapshot;
            applicationsValue = Array.AsReadOnly((applications
                ?? Enumerable.Empty<
                    C1FormalRealtimeBattleEffectApplicationSnapshot>())
                .Where(row => row != null)
                .ToArray());
            emittedCuesValue = Array.AsReadOnly((emittedCues
                ?? Enumerable.Empty<C1FormalRealtimeBattleEffectCue>())
                .Where(row => row != null)
                .ToArray());
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    schemaId,
                    accepted ? "true" : "false",
                    errorCode,
                    this.eventId,
                    acceptedSourceCount.ToString(CultureInfo.InvariantCulture),
                    scheduledSourceCount.ToString(CultureInfo.InvariantCulture),
                    stateSnapshot == null
                        ? string.Empty
                        : stateSnapshot.canonicalSignature,
                    string.Join(",", applicationsValue.Select(
                        row => row.canonicalSignature)),
                    string.Join(",", emittedCuesValue.Select(
                        row => row.canonicalSignature))
                }));
        }

        public string schemaId { get; }
        public bool accepted { get; }
        public C1FormalRealtimeBattleError error { get; }
        public string errorCode { get; }
        public string eventId { get; }
        public int acceptedSourceCount { get; }
        public int scheduledSourceCount { get; }
        public C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot { get; }
        public IReadOnlyList<C1FormalRealtimeBattleEffectApplicationSnapshot>
            applications => applicationsValue;
        public IReadOnlyList<C1FormalRealtimeBattleEffectCue> emittedCues =>
            emittedCuesValue;
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleSessionRequest
    {
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleItemFactSnapshot>
            itemFactsValue;
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
            pool15ActiveItemFactsValue;

        public C1FormalRealtimeBattleSessionRequest(
            string schemaId,
            string productContext,
            string chapterId,
            string stageId,
            string balanceProfileId,
            string encounterVariantId,
            string launchId,
            long launchGeneration,
            string sessionId,
            string sessionToken,
            long resetGeneration,
            string itemBattleInputCanonical,
            string expectedItemCatalogCanonical,
            string expectedEnemyCatalogCanonical,
            IEnumerable<C1FormalRealtimeBattleItemFactSnapshot> itemFacts)
            : this(
                schemaId,
                productContext,
                chapterId,
                stageId,
                balanceProfileId,
                encounterVariantId,
                launchId,
                launchGeneration,
                sessionId,
                sessionToken,
                resetGeneration,
                itemBattleInputCanonical,
                string.Empty,
                expectedItemCatalogCanonical,
                expectedEnemyCatalogCanonical,
                itemFacts,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>())
        {
        }

        public C1FormalRealtimeBattleSessionRequest(
            string schemaId,
            string productContext,
            string chapterId,
            string stageId,
            string balanceProfileId,
            string encounterVariantId,
            string launchId,
            long launchGeneration,
            string sessionId,
            string sessionToken,
            long resetGeneration,
            string itemBattleInputCanonical,
            string arrangementCanonicalSignature,
            string expectedItemCatalogCanonical,
            string expectedEnemyCatalogCanonical,
            IEnumerable<C1FormalRealtimeBattleItemFactSnapshot> itemFacts,
            string pool15ItemInputCanonical,
            string pool15PoolCanonicalSignature,
            string pool15SourceItemCatalogCanonicalSignature,
            string pool15CandidateProfileId,
            string pool15CandidateDataMaturity,
            string pool15CandidateProfileCanonicalSignature,
            IEnumerable<C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
                pool15ActiveItemFacts,
            C1FormalRealtimeBattleNianCapacitySnapshot nianCapacityFact = null)
        {
            this.schemaId = Normalize(schemaId);
            this.productContext = Normalize(productContext);
            this.chapterId = Normalize(chapterId);
            this.stageId = Normalize(stageId);
            this.balanceProfileId = Normalize(balanceProfileId);
            this.encounterVariantId = Normalize(encounterVariantId);
            this.launchId = Normalize(launchId);
            this.launchGeneration = launchGeneration;
            this.sessionId = Normalize(sessionId);
            this.sessionToken = Normalize(sessionToken);
            this.resetGeneration = resetGeneration;
            this.itemBattleInputCanonical = Normalize(itemBattleInputCanonical);
            this.arrangementCanonicalSignature = Normalize(
                arrangementCanonicalSignature);
            this.expectedItemCatalogCanonical =
                Normalize(expectedItemCatalogCanonical);
            this.expectedEnemyCatalogCanonical =
                Normalize(expectedEnemyCatalogCanonical);
            itemFactsValue = Array.AsReadOnly((itemFacts
                ?? Enumerable.Empty<C1FormalRealtimeBattleItemFactSnapshot>())
                .ToArray());
            this.pool15ItemInputCanonical = Normalize(pool15ItemInputCanonical);
            this.pool15PoolCanonicalSignature = Normalize(
                pool15PoolCanonicalSignature);
            this.pool15SourceItemCatalogCanonicalSignature = Normalize(
                pool15SourceItemCatalogCanonicalSignature);
            this.pool15CandidateProfileId = Normalize(pool15CandidateProfileId);
            this.pool15CandidateDataMaturity = Normalize(
                pool15CandidateDataMaturity);
            this.pool15CandidateProfileCanonicalSignature = Normalize(
                pool15CandidateProfileCanonicalSignature);
            pool15ActiveItemFactsValue = Array.AsReadOnly((pool15ActiveItemFacts
                ?? Enumerable.Empty<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>())
                .ToArray());
            this.nianCapacityFact = nianCapacityFact;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Request(this);
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string chapterId { get; }
        public string stageId { get; }
        public string balanceProfileId { get; }
        public string encounterVariantId { get; }
        public string launchId { get; }
        public long launchGeneration { get; }
        public string sessionId { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string itemBattleInputCanonical { get; }
        public string arrangementCanonicalSignature { get; }
        public string expectedItemCatalogCanonical { get; }
        public string expectedEnemyCatalogCanonical { get; }
        public IReadOnlyList<C1FormalRealtimeBattleItemFactSnapshot> itemFacts =>
            itemFactsValue;
        public string pool15ItemInputCanonical { get; }
        public string pool15PoolCanonicalSignature { get; }
        public string pool15SourceItemCatalogCanonicalSignature { get; }
        public string pool15CandidateProfileId { get; }
        public string pool15CandidateDataMaturity { get; }
        public string pool15CandidateProfileCanonicalSignature { get; }
        public IReadOnlyList<C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
            pool15ActiveItemFacts => pool15ActiveItemFactsValue;
        public C1FormalRealtimeBattleNianCapacitySnapshot nianCapacityFact
        {
            get;
        }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleItemRefreshRequest
    {
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleItemFactSnapshot>
            activeItemFactsValue;
        private readonly ReadOnlyCollection<
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
            activePool15ItemFactsValue;

        public C1FormalRealtimeBattleItemRefreshRequest(
            string schemaId,
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            string refreshCommandId,
            long expectedPausedBattleTimeMs,
            string expectedPriorStateCanonicalSignature,
            string itemBattleInputCanonical,
            string expectedItemSessionCanonicalSignature,
            string expectedItemCatalogCanonicalSignature,
            IEnumerable<C1FormalRealtimeBattleItemFactSnapshot> activeItemFacts)
            : this(
                schemaId,
                sessionId,
                sessionToken,
                expectedSessionGeneration,
                expectedResetGeneration,
                refreshCommandId,
                expectedPausedBattleTimeMs,
                expectedPriorStateCanonicalSignature,
                itemBattleInputCanonical,
                expectedItemSessionCanonicalSignature,
                expectedItemCatalogCanonicalSignature,
                activeItemFacts,
                false,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>())
        {
        }

        public C1FormalRealtimeBattleItemRefreshRequest(
            string schemaId,
            string sessionId,
            string sessionToken,
            long expectedSessionGeneration,
            long expectedResetGeneration,
            string refreshCommandId,
            long expectedPausedBattleTimeMs,
            string expectedPriorStateCanonicalSignature,
            string itemBattleInputCanonical,
            string expectedItemSessionCanonicalSignature,
            string expectedItemCatalogCanonicalSignature,
            IEnumerable<C1FormalRealtimeBattleItemFactSnapshot> activeItemFacts,
            bool replacePool15ActiveFacts,
            string pool15ItemInputCanonical,
            string pool15PoolCanonicalSignature,
            string pool15SourceItemCatalogCanonicalSignature,
            string pool15CandidateProfileId,
            string pool15CandidateDataMaturity,
            string pool15CandidateProfileCanonicalSignature,
            IEnumerable<C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
                activePool15ItemFacts,
            C1FormalRealtimeBattleNianCapacitySnapshot nianCapacityFact = null)
        {
            this.schemaId = Normalize(schemaId);
            this.sessionId = Normalize(sessionId);
            this.sessionToken = Normalize(sessionToken);
            this.expectedSessionGeneration = expectedSessionGeneration;
            this.expectedResetGeneration = expectedResetGeneration;
            this.refreshCommandId = Normalize(refreshCommandId);
            this.expectedPausedBattleTimeMs = expectedPausedBattleTimeMs;
            this.expectedPriorStateCanonicalSignature =
                Normalize(expectedPriorStateCanonicalSignature);
            this.itemBattleInputCanonical = Normalize(itemBattleInputCanonical);
            this.expectedItemSessionCanonicalSignature =
                Normalize(expectedItemSessionCanonicalSignature);
            this.expectedItemCatalogCanonicalSignature =
                Normalize(expectedItemCatalogCanonicalSignature);
            activeItemFactsValue = Array.AsReadOnly((activeItemFacts
                ?? Enumerable.Empty<C1FormalRealtimeBattleItemFactSnapshot>())
                .ToArray());
            this.replacePool15ActiveFacts = replacePool15ActiveFacts;
            this.pool15ItemInputCanonical = Normalize(
                pool15ItemInputCanonical);
            this.pool15PoolCanonicalSignature = Normalize(
                pool15PoolCanonicalSignature);
            this.pool15SourceItemCatalogCanonicalSignature = Normalize(
                pool15SourceItemCatalogCanonicalSignature);
            this.pool15CandidateProfileId = Normalize(pool15CandidateProfileId);
            this.pool15CandidateDataMaturity = Normalize(
                pool15CandidateDataMaturity);
            this.pool15CandidateProfileCanonicalSignature = Normalize(
                pool15CandidateProfileCanonicalSignature);
            activePool15ItemFactsValue = Array.AsReadOnly((
                    activePool15ItemFacts
                    ?? Enumerable.Empty<
                        C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>())
                .ToArray());
            this.nianCapacityFact = nianCapacityFact;
            canonicalSignature = C1FormalRealtimeBattleCanonical.ItemRefreshRequest(
                this);
        }

        public string schemaId { get; }
        public string sessionId { get; }
        public string sessionToken { get; }
        public long expectedSessionGeneration { get; }
        public long expectedResetGeneration { get; }
        public string refreshCommandId { get; }
        public long expectedPausedBattleTimeMs { get; }
        public string expectedPriorStateCanonicalSignature { get; }
        public string itemBattleInputCanonical { get; }
        public string expectedItemSessionCanonicalSignature { get; }
        public string expectedItemCatalogCanonicalSignature { get; }
        public IReadOnlyList<C1FormalRealtimeBattleItemFactSnapshot>
            activeItemFacts => activeItemFactsValue;
        public bool replacePool15ActiveFacts { get; }
        public string pool15ItemInputCanonical { get; }
        public string pool15PoolCanonicalSignature { get; }
        public string pool15SourceItemCatalogCanonicalSignature { get; }
        public string pool15CandidateProfileId { get; }
        public string pool15CandidateDataMaturity { get; }
        public string pool15CandidateProfileCanonicalSignature { get; }
        public IReadOnlyList<
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>
            activePool15ItemFacts => activePool15ItemFactsValue;
        public C1FormalRealtimeBattleNianCapacitySnapshot nianCapacityFact
        {
            get;
        }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleActorSnapshot
    {
        private readonly ReadOnlyCollection<string> actionIdsValue;
        private readonly ReadOnlyCollection<string> effectRequestKeysValue;
        private readonly ReadOnlyCollection<string> downstreamConditionsValue;

        public C1FormalRealtimeBattleActorSnapshot(
            string actorBalanceId,
            string contentId,
            string runtimeProfileId,
            int stableActorOrder,
            int maxHp,
            int currentHp,
            string sourceCanonicalSignature)
            : this(
                actorBalanceId,
                contentId,
                runtimeProfileId,
                string.Empty,
                stableActorOrder,
                stableActorOrder + 1,
                maxHp,
                currentHp,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                sourceCanonicalSignature)
        {
        }

        public C1FormalRealtimeBattleActorSnapshot(
            string actorBalanceId,
            string contentId,
            string runtimeProfileId,
            string operatorProfileId,
            int stableActorOrder,
            int occurrenceOrdinal,
            int maxHp,
            int currentHp,
            IEnumerable<string> actionIds,
            IEnumerable<string> effectRequestKeys,
            IEnumerable<string> downstreamConditions,
            string sourceCanonicalSignature)
            : this(
                actorBalanceId,
                contentId,
                runtimeProfileId,
                operatorProfileId,
                stableActorOrder,
                occurrenceOrdinal,
                maxHp,
                currentHp,
                actionIds,
                effectRequestKeys,
                downstreamConditions,
                sourceCanonicalSignature,
                false,
                0,
                0,
                false,
                string.Empty,
                string.Empty,
                string.Empty)
        {
        }

        public C1FormalRealtimeBattleActorSnapshot(
            string actorBalanceId,
            string contentId,
            string runtimeProfileId,
            string operatorProfileId,
            int stableActorOrder,
            int occurrenceOrdinal,
            int maxHp,
            int currentHp,
            IEnumerable<string> actionIds,
            IEnumerable<string> effectRequestKeys,
            IEnumerable<string> downstreamConditions,
            string sourceCanonicalSignature,
            bool hasShell,
            int maxShell,
            int currentShell,
            bool shellBroken,
            string shellBreakTargetId,
            string shellBrokenStateId,
            string shellBreakCounterWindowId)
        {
            this.actorBalanceId = Normalize(actorBalanceId);
            this.contentId = Normalize(contentId);
            this.runtimeProfileId = Normalize(runtimeProfileId);
            this.operatorProfileId = Normalize(operatorProfileId);
            this.stableActorOrder = stableActorOrder;
            this.occurrenceOrdinal = occurrenceOrdinal;
            this.maxHp = maxHp;
            this.currentHp = currentHp;
            defeated = currentHp <= 0;
            this.hasShell = hasShell;
            this.maxShell = maxShell;
            this.currentShell = currentShell;
            this.shellBroken = shellBroken;
            this.shellBreakTargetId = Normalize(shellBreakTargetId);
            this.shellBrokenStateId = Normalize(shellBrokenStateId);
            this.shellBreakCounterWindowId = Normalize(
                shellBreakCounterWindowId);
            actionIdsValue = Array.AsReadOnly((actionIds
                ?? Enumerable.Empty<string>()).Select(Normalize).ToArray());
            effectRequestKeysValue = Array.AsReadOnly((effectRequestKeys
                ?? Enumerable.Empty<string>()).Select(Normalize).ToArray());
            downstreamConditionsValue = Array.AsReadOnly((downstreamConditions
                ?? Enumerable.Empty<string>()).Select(Normalize).ToArray());
            this.sourceCanonicalSignature = Normalize(sourceCanonicalSignature);
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    this.actorBalanceId,
                    this.contentId,
                    this.runtimeProfileId,
                    this.operatorProfileId,
                    stableActorOrder.ToString(CultureInfo.InvariantCulture),
                    occurrenceOrdinal.ToString(CultureInfo.InvariantCulture),
                    maxHp.ToString(CultureInfo.InvariantCulture),
                    currentHp.ToString(CultureInfo.InvariantCulture),
                    defeated ? "true" : "false",
                    string.Join(",", actionIdsValue),
                    string.Join(",", effectRequestKeysValue),
                    string.Join(",", downstreamConditionsValue),
                    hasShell ? "true" : "false",
                    maxShell.ToString(CultureInfo.InvariantCulture),
                    currentShell.ToString(CultureInfo.InvariantCulture),
                    shellBroken ? "true" : "false",
                    this.shellBreakTargetId,
                    this.shellBrokenStateId,
                    this.shellBreakCounterWindowId,
                    this.sourceCanonicalSignature
                }));
        }

        public string actorBalanceId { get; }
        public string contentId { get; }
        public string runtimeProfileId { get; }
        public string operatorProfileId { get; }
        public int stableActorOrder { get; }
        public int occurrenceOrdinal { get; }
        public int maxHp { get; }
        public int currentHp { get; }
        public bool defeated { get; }
        public bool hasShell { get; }
        public int maxShell { get; }
        public int currentShell { get; }
        public bool shellBroken { get; }
        public string shellBreakTargetId { get; }
        public string shellBrokenStateId { get; }
        public string shellBreakCounterWindowId { get; }
        public IReadOnlyList<string> actionIds => actionIdsValue;
        public IReadOnlyList<string> effectRequestKeys => effectRequestKeysValue;
        public IReadOnlyList<string> downstreamConditions =>
            downstreamConditionsValue;
        public string sourceCanonicalSignature { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattlePlayerSnapshot
    {
        public C1FormalRealtimeBattlePlayerSnapshot(int maxHp, int currentHp)
            : this(maxHp, currentHp, 0L)
        {
        }

        public C1FormalRealtimeBattlePlayerSnapshot(
            int maxHp,
            int currentHp,
            long guard)
        {
            this.maxHp = maxHp;
            this.currentHp = currentHp;
            this.guard = guard;
            defeated = currentHp <= 0;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                maxHp.ToString(CultureInfo.InvariantCulture) + "|"
                + currentHp.ToString(CultureInfo.InvariantCulture) + "|"
                + guard.ToString(CultureInfo.InvariantCulture) + "|"
                + (defeated ? "true" : "false"));
        }

        public int maxHp { get; }
        public int currentHp { get; }
        public long guard { get; }
        public bool defeated { get; }
        public string canonicalSignature { get; }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleNianStateSnapshot
    {
        public C1FormalRealtimeBattleNianStateSnapshot(
            string resourceKey,
            int initialNian,
            int maxNian,
            int currentNian,
            int generationAmount,
            long generationIntervalMilliseconds,
            long nextGenerationAtBattleTimeMs,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string capacityCanonicalSignature,
            int acceptedTransactionCount)
        {
            schemaId = C1FormalRealtimeBattleSessionContract.NianStateSchemaId;
            this.resourceKey = Normalize(resourceKey);
            this.initialNian = initialNian;
            this.maxNian = maxNian;
            this.currentNian = currentNian;
            this.generationAmount = generationAmount;
            this.generationIntervalMilliseconds =
                generationIntervalMilliseconds;
            this.nextGenerationAtBattleTimeMs =
                nextGenerationAtBattleTimeMs;
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.sourceBaseItemId = Normalize(sourceBaseItemId);
            this.capacityCanonicalSignature = Normalize(
                capacityCanonicalSignature);
            this.acceptedTransactionCount = acceptedTransactionCount;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    schemaId,
                    this.resourceKey,
                    initialNian.ToString(CultureInfo.InvariantCulture),
                    maxNian.ToString(CultureInfo.InvariantCulture),
                    currentNian.ToString(CultureInfo.InvariantCulture),
                    this.sourceItemInstanceId,
                    this.sourceBaseItemId,
                    this.capacityCanonicalSignature,
                    acceptedTransactionCount.ToString(
                        CultureInfo.InvariantCulture)
                }));
        }

        public string schemaId { get; }
        public string resourceKey { get; }
        public int initialNian { get; }
        public int maxNian { get; }
        public int currentNian { get; }
        public int generationAmount { get; }
        public long generationIntervalMilliseconds { get; }
        public long nextGenerationAtBattleTimeMs { get; }
        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public string capacityCanonicalSignature { get; }
        public int acceptedTransactionCount { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleNianTransactionSnapshot
    {
        public C1FormalRealtimeBattleNianTransactionSnapshot(
            long sequence,
            string sessionId,
            long sessionGeneration,
            long resetGeneration,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string acceptedApplicationEventId,
            long battleTimeMs,
            string transactionKind,
            int requestedAmount,
            int appliedAmount,
            int before,
            int after)
        {
            schemaId = C1FormalRealtimeBattleSessionContract
                .NianTransactionSchemaId;
            this.sequence = sequence;
            this.sessionId = Normalize(sessionId);
            this.sessionGeneration = sessionGeneration;
            this.resetGeneration = resetGeneration;
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.sourceBaseItemId = Normalize(sourceBaseItemId);
            this.acceptedApplicationEventId = Normalize(
                acceptedApplicationEventId);
            this.battleTimeMs = battleTimeMs;
            this.transactionKind = Normalize(transactionKind);
            this.requestedAmount = requestedAmount;
            this.appliedAmount = appliedAmount;
            this.before = before;
            this.after = after;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    schemaId,
                    sequence.ToString(CultureInfo.InvariantCulture),
                    this.sessionId,
                    sessionGeneration.ToString(CultureInfo.InvariantCulture),
                    resetGeneration.ToString(CultureInfo.InvariantCulture),
                    this.sourceItemInstanceId,
                    this.sourceBaseItemId,
                    this.acceptedApplicationEventId,
                    battleTimeMs.ToString(CultureInfo.InvariantCulture),
                    this.transactionKind,
                    requestedAmount.ToString(CultureInfo.InvariantCulture),
                    appliedAmount.ToString(CultureInfo.InvariantCulture),
                    before.ToString(CultureInfo.InvariantCulture),
                    after.ToString(CultureInfo.InvariantCulture)
                }));
        }

        public string schemaId { get; }
        public long sequence { get; }
        public string sessionId { get; }
        public long sessionGeneration { get; }
        public long resetGeneration { get; }
        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public string acceptedApplicationEventId { get; }
        public long battleTimeMs { get; }
        public string transactionKind { get; }
        public int requestedAmount { get; }
        public int appliedAmount { get; }
        public int before { get; }
        public int after { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleScheduledActionSnapshot
    {
        public C1FormalRealtimeBattleScheduledActionSnapshot(
            string actionId,
            int stableOrder,
            long dueBattleTimeMs,
            string actionKind,
            string sourceOwner,
            string sourceId,
            int requestedDamage,
            bool executed,
            bool accepted,
            string diagnosticCode,
            string acceptedApplicationEventId)
        {
            this.actionId = Normalize(actionId);
            this.stableOrder = stableOrder;
            this.dueBattleTimeMs = dueBattleTimeMs;
            this.actionKind = Normalize(actionKind);
            this.sourceOwner = Normalize(sourceOwner);
            this.sourceId = Normalize(sourceId);
            this.requestedDamage = requestedDamage;
            this.executed = executed;
            this.accepted = accepted;
            this.diagnosticCode = Normalize(diagnosticCode);
            this.acceptedApplicationEventId =
                Normalize(acceptedApplicationEventId);
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    this.actionId,
                    stableOrder.ToString(CultureInfo.InvariantCulture),
                    dueBattleTimeMs.ToString(CultureInfo.InvariantCulture),
                    this.actionKind,
                    this.sourceOwner,
                    this.sourceId,
                    requestedDamage.ToString(CultureInfo.InvariantCulture),
                    executed ? "true" : "false",
                    accepted ? "true" : "false",
                    this.diagnosticCode,
                    this.acceptedApplicationEventId
                }));
        }

        public string actionId { get; }
        public int stableOrder { get; }
        public long dueBattleTimeMs { get; }
        public string actionKind { get; }
        public string sourceOwner { get; }
        public string sourceId { get; }
        public int requestedDamage { get; }
        public bool executed { get; }
        public bool accepted { get; }
        public string diagnosticCode { get; }
        public string acceptedApplicationEventId { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleCue
    {
        public C1FormalRealtimeBattleCue(
            string cueId,
            long sequence,
            long battleTimeMs,
            string cueKind,
            string sourceOwner,
            string sourceId,
            string targetActorId,
            int targetStableOrder,
            int playerHpBefore,
            int playerHpAfter,
            int actorHpBefore,
            int actorHpAfter,
            int requestedDamage,
            int appliedDamage,
            string floatPayload,
            int presentationPriority,
            string acceptedApplicationEventId)
            : this(
                cueId,
                sequence,
                battleTimeMs,
                cueKind,
                sourceOwner,
                sourceId,
                targetActorId,
                targetStableOrder,
                playerHpBefore,
                playerHpAfter,
                actorHpBefore,
                actorHpAfter,
                requestedDamage,
                appliedDamage,
                floatPayload,
                presentationPriority,
                acceptedApplicationEventId,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty)
        {
        }

        public C1FormalRealtimeBattleCue(
            string cueId,
            long sequence,
            long battleTimeMs,
            string cueKind,
            string sourceOwner,
            string sourceId,
            string targetActorId,
            int targetStableOrder,
            int playerHpBefore,
            int playerHpAfter,
            int actorHpBefore,
            int actorHpAfter,
            int requestedDamage,
            int appliedDamage,
            string floatPayload,
            int presentationPriority,
            string acceptedApplicationEventId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string effectFamilyId,
            string effectVariantId)
            : this(
                cueId,
                sequence,
                battleTimeMs,
                cueKind,
                sourceOwner,
                sourceId,
                targetActorId,
                targetStableOrder,
                playerHpBefore,
                playerHpAfter,
                actorHpBefore,
                actorHpAfter,
                requestedDamage,
                appliedDamage,
                floatPayload,
                presentationPriority,
                acceptedApplicationEventId,
                sourceItemInstanceId,
                sourceBaseItemId,
                effectFamilyId,
                effectVariantId,
                string.Empty,
                string.Empty,
                0,
                0,
                0,
                0,
                string.Empty)
        {
        }

        public C1FormalRealtimeBattleCue(
            string cueId,
            long sequence,
            long battleTimeMs,
            string cueKind,
            string sourceOwner,
            string sourceId,
            string targetActorId,
            int targetStableOrder,
            int playerHpBefore,
            int playerHpAfter,
            int actorHpBefore,
            int actorHpAfter,
            int requestedDamage,
            int appliedDamage,
            string floatPayload,
            int presentationPriority,
            string acceptedApplicationEventId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string effectFamilyId,
            string effectVariantId,
            string resourceKey,
            string resourceTransactionKind,
            int resourceRequestedAmount,
            int resourceAppliedAmount,
            int resourceBefore,
            int resourceAfter,
            string resourceTransactionCanonicalSignature,
            int sourceStableOrder = -1,
            string triggerKind = "",
            string deliveryKind = "",
            string resultKind = "")
            : this(
                cueId,
                sequence,
                battleTimeMs,
                cueKind,
                sourceOwner,
                sourceId,
                targetActorId,
                targetStableOrder,
                playerHpBefore,
                playerHpAfter,
                actorHpBefore,
                actorHpAfter,
                requestedDamage,
                appliedDamage,
                floatPayload,
                presentationPriority,
                acceptedApplicationEventId,
                sourceItemInstanceId,
                sourceBaseItemId,
                effectFamilyId,
                effectVariantId,
                resourceKey,
                resourceTransactionKind,
                resourceRequestedAmount,
                resourceAppliedAmount,
                resourceBefore,
                resourceAfter,
                resourceTransactionCanonicalSignature,
                0,
                0,
                0,
                false,
                string.Empty,
                string.Empty,
                sourceStableOrder,
                triggerKind,
                deliveryKind,
                resultKind)
        {
        }

        public C1FormalRealtimeBattleCue(
            string cueId,
            long sequence,
            long battleTimeMs,
            string cueKind,
            string sourceOwner,
            string sourceId,
            string targetActorId,
            int targetStableOrder,
            int playerHpBefore,
            int playerHpAfter,
            int actorHpBefore,
            int actorHpAfter,
            int requestedDamage,
            int appliedDamage,
            string floatPayload,
            int presentationPriority,
            string acceptedApplicationEventId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string effectFamilyId,
            string effectVariantId,
            string resourceKey,
            string resourceTransactionKind,
            int resourceRequestedAmount,
            int resourceAppliedAmount,
            int resourceBefore,
            int resourceAfter,
            string resourceTransactionCanonicalSignature,
            int shellBefore,
            int shellAfter,
            int shellAppliedAmount,
            bool shellBroken,
            string shellBrokenStateId,
            string shellBreakCounterWindowId,
            int sourceStableOrder = -1,
            string triggerKind = "",
            string deliveryKind = "",
            string resultKind = "")
        {
            this.cueId = Normalize(cueId);
            this.sequence = sequence;
            this.battleTimeMs = battleTimeMs;
            this.cueKind = Normalize(cueKind);
            this.sourceOwner = Normalize(sourceOwner);
            this.sourceId = Normalize(sourceId);
            this.targetActorId = Normalize(targetActorId);
            this.targetStableOrder = targetStableOrder;
            this.playerHpBefore = playerHpBefore;
            this.playerHpAfter = playerHpAfter;
            this.actorHpBefore = actorHpBefore;
            this.actorHpAfter = actorHpAfter;
            this.requestedDamage = requestedDamage;
            this.appliedDamage = appliedDamage;
            this.floatPayload = Normalize(floatPayload);
            this.presentationPriority = presentationPriority;
            this.acceptedApplicationEventId =
                Normalize(acceptedApplicationEventId);
            this.sourceItemInstanceId = Normalize(sourceItemInstanceId);
            this.sourceBaseItemId = Normalize(sourceBaseItemId);
            this.effectFamilyId = Normalize(effectFamilyId);
            this.effectVariantId = Normalize(effectVariantId);
            this.resourceKey = Normalize(resourceKey);
            this.resourceTransactionKind = Normalize(resourceTransactionKind);
            this.resourceRequestedAmount = resourceRequestedAmount;
            this.resourceAppliedAmount = resourceAppliedAmount;
            this.resourceBefore = resourceBefore;
            this.resourceAfter = resourceAfter;
            this.resourceTransactionCanonicalSignature = Normalize(
                resourceTransactionCanonicalSignature);
            this.shellBefore = shellBefore;
            this.shellAfter = shellAfter;
            this.shellAppliedAmount = shellAppliedAmount;
            this.shellBroken = shellBroken;
            this.shellBrokenStateId = Normalize(shellBrokenStateId);
            this.shellBreakCounterWindowId = Normalize(
                shellBreakCounterWindowId);
            this.sourceStableOrder = sourceStableOrder;
            this.triggerKind = Normalize(triggerKind);
            this.deliveryKind = Normalize(deliveryKind);
            this.resultKind = Normalize(resultKind);
            canonicalSignature = C1FormalRealtimeBattleCanonical.Cue(this);
        }

        public string cueId { get; }
        public long sequence { get; }
        public long battleTimeMs { get; }
        public string cueKind { get; }
        public string sourceOwner { get; }
        public string sourceId { get; }
        public string targetActorId { get; }
        public int targetStableOrder { get; }
        public int playerHpBefore { get; }
        public int playerHpAfter { get; }
        public int actorHpBefore { get; }
        public int actorHpAfter { get; }
        public int requestedDamage { get; }
        public int appliedDamage { get; }
        public string floatPayload { get; }
        public int presentationPriority { get; }
        public string acceptedApplicationEventId { get; }
        public string sourceItemInstanceId { get; }
        public string sourceBaseItemId { get; }
        public string effectFamilyId { get; }
        public string effectVariantId { get; }
        public string resourceKey { get; }
        public string resourceTransactionKind { get; }
        public int resourceRequestedAmount { get; }
        public int resourceAppliedAmount { get; }
        public int resourceBefore { get; }
        public int resourceAfter { get; }
        public string resourceTransactionCanonicalSignature { get; }
        public int shellBefore { get; }
        public int shellAfter { get; }
        public int shellAppliedAmount { get; }
        public bool shellBroken { get; }
        public string shellBrokenStateId { get; }
        public string shellBreakCounterWindowId { get; }
        public int sourceStableOrder { get; }
        public string triggerKind { get; }
        public string deliveryKind { get; }
        public string resultKind { get; }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleSessionStateSnapshot
    {
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleActorSnapshot>
            actorSnapshotsValue;
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleScheduledActionSnapshot>
            scheduledActionsValue;
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleStatusSnapshot>
            statusSnapshotsValue;

        internal C1FormalRealtimeBattleSessionStateSnapshot(
            bool accepted,
            string errorCode,
            string sessionId,
            long sessionGeneration,
            long resetGeneration,
            string phase,
            bool paused,
            bool terminal,
            long battleTimeMs,
            C1FormalRealtimeBattlePlayerSnapshot playerSnapshot,
            IEnumerable<C1FormalRealtimeBattleActorSnapshot> actorSnapshots,
            IEnumerable<C1FormalRealtimeBattleScheduledActionSnapshot>
                scheduledActions,
            int acceptedItemApplicationCount,
            int enemyApplicationCount,
            long cueSequence)
            : this(
                accepted,
                errorCode,
                sessionId,
                sessionGeneration,
                resetGeneration,
                phase,
                paused,
                terminal,
                battleTimeMs,
                playerSnapshot,
                actorSnapshots,
                scheduledActions,
                acceptedItemApplicationCount,
                enemyApplicationCount,
                cueSequence,
                0,
                string.Empty,
                new C1FormalRealtimeBattleEffectStateSnapshot(
                    0L,
                    Array.Empty<
                        C1FormalRealtimeBattleEffectStateValueSnapshot>()),
                0,
                0)
        {
        }

        internal C1FormalRealtimeBattleSessionStateSnapshot(
            bool accepted,
            string errorCode,
            string sessionId,
            long sessionGeneration,
            long resetGeneration,
            string phase,
            bool paused,
            bool terminal,
            long battleTimeMs,
            C1FormalRealtimeBattlePlayerSnapshot playerSnapshot,
            IEnumerable<C1FormalRealtimeBattleActorSnapshot> actorSnapshots,
            IEnumerable<C1FormalRealtimeBattleScheduledActionSnapshot>
                scheduledActions,
            int acceptedItemApplicationCount,
            int enemyApplicationCount,
            long cueSequence,
            int activePool15SourceCount,
            string activePool15SourceCanonicalSignature,
            C1FormalRealtimeBattleEffectStateSnapshot effectStateSnapshot,
            int pool15ApplicationCount,
            int pool15CueCount,
            C1FormalRealtimeBattleNianStateSnapshot nianStateSnapshot = null,
            IEnumerable<C1FormalRealtimeBattleStatusSnapshot>
                statusSnapshots = null)
        {
            schemaId = C1FormalRealtimeBattleSessionContract.StateSchemaId;
            this.accepted = accepted;
            this.errorCode = Normalize(errorCode);
            this.sessionId = Normalize(sessionId);
            this.sessionGeneration = sessionGeneration;
            this.resetGeneration = resetGeneration;
            this.phase = Normalize(phase);
            this.paused = paused;
            this.terminal = terminal;
            this.battleTimeMs = battleTimeMs;
            this.playerSnapshot = playerSnapshot;
            actorSnapshotsValue = Array.AsReadOnly((actorSnapshots
                ?? Enumerable.Empty<C1FormalRealtimeBattleActorSnapshot>())
                .ToArray());
            scheduledActionsValue = Array.AsReadOnly((scheduledActions
                ?? Enumerable.Empty<C1FormalRealtimeBattleScheduledActionSnapshot>())
                .ToArray());
            statusSnapshotsValue = Array.AsReadOnly((statusSnapshots
                ?? Enumerable.Empty<C1FormalRealtimeBattleStatusSnapshot>())
                .ToArray());
            this.acceptedItemApplicationCount = acceptedItemApplicationCount;
            this.enemyApplicationCount = enemyApplicationCount;
            this.cueSequence = cueSequence;
            this.activePool15SourceCount = activePool15SourceCount;
            this.activePool15SourceCanonicalSignature = Normalize(
                activePool15SourceCanonicalSignature);
            this.effectStateSnapshot = effectStateSnapshot
                ?? new C1FormalRealtimeBattleEffectStateSnapshot(
                    0L,
                    Array.Empty<
                        C1FormalRealtimeBattleEffectStateValueSnapshot>());
            this.pool15ApplicationCount = pool15ApplicationCount;
            this.pool15CueCount = pool15CueCount;
            this.nianStateSnapshot = nianStateSnapshot;
            canonicalSignature = C1FormalRealtimeBattleCanonical.State(this);
        }

        public string schemaId { get; }
        public bool accepted { get; }
        public string errorCode { get; }
        public string sessionId { get; }
        public long sessionGeneration { get; }
        public long resetGeneration { get; }
        public string phase { get; }
        public bool paused { get; }
        public bool terminal { get; }
        public long battleTimeMs { get; }
        public C1FormalRealtimeBattlePlayerSnapshot playerSnapshot { get; }
        public IReadOnlyList<C1FormalRealtimeBattleActorSnapshot> actorSnapshots =>
            actorSnapshotsValue;
        public IReadOnlyList<C1FormalRealtimeBattleScheduledActionSnapshot>
            scheduledActions => scheduledActionsValue;
        public IReadOnlyList<C1FormalRealtimeBattleStatusSnapshot>
            statusSnapshots => statusSnapshotsValue;
        public int acceptedItemApplicationCount { get; }
        public int enemyApplicationCount { get; }
        public long cueSequence { get; }
        public int activePool15SourceCount { get; }
        public string activePool15SourceCanonicalSignature { get; }
        public C1FormalRealtimeBattleEffectStateSnapshot effectStateSnapshot { get; }
        public int pool15ApplicationCount { get; }
        public int pool15CueCount { get; }
        public C1FormalRealtimeBattleNianStateSnapshot nianStateSnapshot
        {
            get;
        }
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleTerminalResult
    {
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleActorSnapshot>
            actorSnapshotsValue;
        private readonly BattleResultSnapshot battleResultSnapshotValue;

        internal C1FormalRealtimeBattleTerminalResult(
            bool accepted,
            C1FormalRealtimeBattleError error,
            string sessionId,
            string stageId,
            long durationMs,
            bool win,
            bool lose,
            C1FormalRealtimeBattlePlayerSnapshot playerSnapshot,
            IEnumerable<C1FormalRealtimeBattleActorSnapshot> actorSnapshots,
            int acceptedItemApplicationCount,
            int enemyApplicationCount,
            BattleResultSnapshot battleResultSnapshot)
        {
            schemaId = C1FormalRealtimeBattleSessionContract.TerminalSchemaId;
            this.accepted = accepted;
            this.error = error ?? new C1FormalRealtimeBattleError(
                C1FormalRealtimeBattleErrorCodes.None,
                string.Empty);
            errorCode = this.error.errorCode;
            this.sessionId = Normalize(sessionId);
            this.stageId = Normalize(stageId);
            this.durationMs = durationMs;
            this.win = win;
            this.lose = lose;
            this.playerSnapshot = playerSnapshot;
            actorSnapshotsValue = Array.AsReadOnly((actorSnapshots
                ?? Enumerable.Empty<C1FormalRealtimeBattleActorSnapshot>())
                .ToArray());
            this.acceptedItemApplicationCount = acceptedItemApplicationCount;
            this.enemyApplicationCount = enemyApplicationCount;
            battleResultSnapshotValue = CloneBattleResult(battleResultSnapshot);
            shouldWriteSave = false;
            shouldGrantReward = false;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Terminal(this);
        }

        public string schemaId { get; }
        public bool accepted { get; }
        public C1FormalRealtimeBattleError error { get; }
        public string errorCode { get; }
        public string sessionId { get; }
        public string stageId { get; }
        public long durationMs { get; }
        public bool win { get; }
        public bool lose { get; }
        public C1FormalRealtimeBattlePlayerSnapshot playerSnapshot { get; }
        public IReadOnlyList<C1FormalRealtimeBattleActorSnapshot> actorSnapshots =>
            actorSnapshotsValue;
        public int acceptedItemApplicationCount { get; }
        public int enemyApplicationCount { get; }
        public BattleResultSnapshot battleResultSnapshot =>
            CloneBattleResult(battleResultSnapshotValue);
        public bool shouldWriteSave { get; }
        public bool shouldGrantReward { get; }
        public string canonicalSignature { get; }

        private static BattleResultSnapshot CloneBattleResult(
            BattleResultSnapshot source)
        {
            if (source == null)
            {
                return null;
            }

            return new BattleResultSnapshot
            {
                resultId = source.resultId ?? string.Empty,
                requestId = source.requestId ?? string.Empty,
                resultType = source.resultType,
                win = source.win,
                lose = source.lose,
                abandon = source.abandon,
                roundId = source.roundId ?? string.Empty,
                bossDefeated = source.bossDefeated,
                durationSeconds = source.durationSeconds,
                chapterProgressDelta = source.chapterProgressDelta ?? string.Empty,
                rewardPreview = new List<string>(source.rewardPreview
                    ?? new List<string>()),
                itemDrops = new List<string>(source.itemDrops
                    ?? new List<string>()),
                buildPerformanceSummary = source.buildPerformanceSummary
                    ?? string.Empty,
                eventSummary = new List<string>(source.eventSummary
                    ?? new List<string>()),
                rewardClaimToken = source.rewardClaimToken ?? string.Empty,
                nextRouteHint = source.nextRouteHint ?? string.Empty,
                devOnly = false,
                shouldWriteSave = false,
                shouldGrantReward = false
            };
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleSessionStartSnapshot
    {
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleCue>
            initialCuesValue;

        internal C1FormalRealtimeBattleSessionStartSnapshot(
            bool accepted,
            C1FormalRealtimeBattleError error,
            C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot,
            IEnumerable<C1FormalRealtimeBattleCue> initialCues)
        {
            this.accepted = accepted;
            this.error = error ?? new C1FormalRealtimeBattleError(
                C1FormalRealtimeBattleErrorCodes.None,
                string.Empty);
            errorCode = this.error.errorCode;
            this.stateSnapshot = stateSnapshot;
            initialCuesValue = Array.AsReadOnly((initialCues
                ?? Enumerable.Empty<C1FormalRealtimeBattleCue>()).ToArray());
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    accepted ? "true" : "false",
                    errorCode,
                    stateSnapshot == null
                        ? string.Empty
                        : stateSnapshot.canonicalSignature,
                    string.Join(",", initialCuesValue.Select(
                        cue => cue == null ? string.Empty : cue.canonicalSignature))
                }));
        }

        public bool accepted { get; }
        public C1FormalRealtimeBattleError error { get; }
        public string errorCode { get; }
        public C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot { get; }
        public IReadOnlyList<C1FormalRealtimeBattleCue> initialCues =>
            initialCuesValue;
        public string canonicalSignature { get; }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleTickResult
    {
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleCue>
            emittedCuesValue;

        internal C1FormalRealtimeBattleTickResult(
            bool accepted,
            C1FormalRealtimeBattleError error,
            C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot,
            IEnumerable<C1FormalRealtimeBattleCue> emittedCues,
            C1FormalRealtimeBattleTerminalResult terminalResult)
        {
            this.accepted = accepted;
            this.error = error ?? new C1FormalRealtimeBattleError(
                C1FormalRealtimeBattleErrorCodes.None,
                string.Empty);
            errorCode = this.error.errorCode;
            this.stateSnapshot = stateSnapshot;
            emittedCuesValue = Array.AsReadOnly((emittedCues
                ?? Enumerable.Empty<C1FormalRealtimeBattleCue>()).ToArray());
            this.terminalResult = terminalResult;
            canonicalSignature = C1FormalRealtimeBattleCanonical.Hash(
                string.Join("|", new[]
                {
                    accepted ? "true" : "false",
                    errorCode,
                    stateSnapshot == null
                        ? string.Empty
                        : stateSnapshot.canonicalSignature,
                    terminalResult == null
                        ? string.Empty
                        : terminalResult.canonicalSignature,
                    string.Join(",", emittedCuesValue.Select(
                        cue => cue == null ? string.Empty : cue.canonicalSignature))
                }));
        }

        public bool accepted { get; }
        public C1FormalRealtimeBattleError error { get; }
        public string errorCode { get; }
        public C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot { get; }
        public IReadOnlyList<C1FormalRealtimeBattleCue> emittedCues =>
            emittedCuesValue;
        public C1FormalRealtimeBattleTerminalResult terminalResult { get; }
        public string canonicalSignature { get; }
    }

    [Serializable]
    public sealed class C1FormalRealtimeBattleItemRefreshResult
    {
        private readonly ReadOnlyCollection<C1FormalRealtimeBattleCue>
            emittedCuesValue;

        internal C1FormalRealtimeBattleItemRefreshResult(
            bool accepted,
            C1FormalRealtimeBattleError error,
            string sessionId,
            long sessionGeneration,
            long resetGeneration,
            long battleTimeMs,
            string itemBattleInputCanonical,
            string acceptedCommandId,
            int canceledFutureItemActionCount,
            int retainedFutureItemActionCount,
            int scheduledFutureItemActionCount,
            int activeItemFactCount,
            C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot,
            IEnumerable<C1FormalRealtimeBattleCue> emittedCues)
        {
            schemaId = C1FormalRealtimeBattleSessionContract
                .ItemRefreshResultSchemaId;
            this.accepted = accepted;
            this.error = error ?? new C1FormalRealtimeBattleError(
                C1FormalRealtimeBattleErrorCodes.None,
                string.Empty);
            errorCode = this.error.errorCode;
            this.sessionId = Normalize(sessionId);
            this.sessionGeneration = sessionGeneration;
            this.resetGeneration = resetGeneration;
            this.battleTimeMs = battleTimeMs;
            this.itemBattleInputCanonical = Normalize(itemBattleInputCanonical);
            this.acceptedCommandId = Normalize(acceptedCommandId);
            this.canceledFutureItemActionCount = canceledFutureItemActionCount;
            this.retainedFutureItemActionCount = retainedFutureItemActionCount;
            this.scheduledFutureItemActionCount = scheduledFutureItemActionCount;
            this.activeItemFactCount = activeItemFactCount;
            this.stateSnapshot = stateSnapshot;
            emittedCuesValue = Array.AsReadOnly((emittedCues
                ?? Enumerable.Empty<C1FormalRealtimeBattleCue>()).ToArray());
            canonicalSignature = C1FormalRealtimeBattleCanonical.ItemRefreshResult(
                this);
        }

        public string schemaId { get; }
        public bool accepted { get; }
        public C1FormalRealtimeBattleError error { get; }
        public string errorCode { get; }
        public string sessionId { get; }
        public long sessionGeneration { get; }
        public long resetGeneration { get; }
        public long battleTimeMs { get; }
        public string itemBattleInputCanonical { get; }
        public string acceptedCommandId { get; }
        public int canceledFutureItemActionCount { get; }
        public int retainedFutureItemActionCount { get; }
        public int scheduledFutureItemActionCount { get; }
        public int activeItemFactCount { get; }
        public C1FormalRealtimeBattleSessionStateSnapshot stateSnapshot { get; }
        public IReadOnlyList<C1FormalRealtimeBattleCue> emittedCues =>
            emittedCuesValue;
        public string canonicalSignature { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    internal static class C1FormalRealtimeBattleCanonical
    {
        internal static string Request(
            C1FormalRealtimeBattleSessionRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            StringBuilder value = new StringBuilder();
            value.Append(string.Join("|", new[]
            {
                request.schemaId,
                request.productContext,
                request.chapterId,
                request.stageId,
                request.balanceProfileId,
                request.encounterVariantId,
                request.launchId,
                request.launchGeneration.ToString(CultureInfo.InvariantCulture),
                request.sessionId,
                request.sessionToken,
                request.resetGeneration.ToString(CultureInfo.InvariantCulture),
                request.itemBattleInputCanonical,
                request.expectedItemCatalogCanonical,
                request.expectedEnemyCatalogCanonical,
                request.pool15ItemInputCanonical,
                request.pool15PoolCanonicalSignature,
                request.pool15SourceItemCatalogCanonicalSignature,
                request.pool15CandidateProfileId,
                request.pool15CandidateDataMaturity,
                request.pool15CandidateProfileCanonicalSignature
            }));
            if (!string.IsNullOrEmpty(request.arrangementCanonicalSignature))
            {
                value.Append("\nARRANGEMENT|").Append(
                    request.arrangementCanonicalSignature);
            }
            foreach (C1FormalRealtimeBattleItemFactSnapshot row in request.itemFacts)
            {
                value.Append("\nITEM|").Append(
                    row == null ? "null" : row.canonicalSignature);
            }

            foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot row in
                     request.pool15ActiveItemFacts)
            {
                value.Append("\nPOOL15|").Append(
                    row == null ? "null" : row.canonicalSignature);
            }
            if (request.nianCapacityFact != null)
            {
                value.Append("\nNIAN_CAPACITY|").Append(
                    request.nianCapacityFact.canonicalSignature);
            }

            return Hash(value.ToString());
        }

        internal static string Cue(C1FormalRealtimeBattleCue cue)
        {
            return cue == null
                ? string.Empty
                : Hash(string.Join("|", new[]
                {
                    cue.cueId,
                    cue.sequence.ToString(CultureInfo.InvariantCulture),
                    cue.battleTimeMs.ToString(CultureInfo.InvariantCulture),
                    cue.cueKind,
                    cue.sourceOwner,
                    cue.sourceId,
                    cue.targetActorId,
                    cue.targetStableOrder.ToString(CultureInfo.InvariantCulture),
                    cue.playerHpBefore.ToString(CultureInfo.InvariantCulture),
                    cue.playerHpAfter.ToString(CultureInfo.InvariantCulture),
                    cue.actorHpBefore.ToString(CultureInfo.InvariantCulture),
                    cue.actorHpAfter.ToString(CultureInfo.InvariantCulture),
                    cue.requestedDamage.ToString(CultureInfo.InvariantCulture),
                    cue.appliedDamage.ToString(CultureInfo.InvariantCulture),
                    cue.floatPayload,
                    cue.presentationPriority.ToString(CultureInfo.InvariantCulture),
                    cue.acceptedApplicationEventId,
                    cue.sourceItemInstanceId,
                    cue.sourceBaseItemId,
                    cue.effectFamilyId,
                    cue.effectVariantId,
                    cue.resourceKey,
                    cue.resourceTransactionKind,
                    cue.resourceRequestedAmount.ToString(
                        CultureInfo.InvariantCulture),
                    cue.resourceAppliedAmount.ToString(
                        CultureInfo.InvariantCulture),
                    cue.resourceBefore.ToString(CultureInfo.InvariantCulture),
                    cue.resourceAfter.ToString(CultureInfo.InvariantCulture),
                    cue.resourceTransactionCanonicalSignature,
                    cue.shellBefore.ToString(CultureInfo.InvariantCulture),
                    cue.shellAfter.ToString(CultureInfo.InvariantCulture),
                    cue.shellAppliedAmount.ToString(
                        CultureInfo.InvariantCulture),
                    cue.shellBroken ? "true" : "false",
                    cue.shellBrokenStateId,
                    cue.shellBreakCounterWindowId
                }));
        }

        internal static string ItemRefreshRequest(
            C1FormalRealtimeBattleItemRefreshRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            StringBuilder value = new StringBuilder(string.Join("|", new[]
            {
                request.schemaId,
                request.sessionId,
                request.sessionToken,
                request.expectedSessionGeneration.ToString(
                    CultureInfo.InvariantCulture),
                request.expectedResetGeneration.ToString(
                    CultureInfo.InvariantCulture),
                request.refreshCommandId,
                request.expectedPausedBattleTimeMs.ToString(
                    CultureInfo.InvariantCulture),
                request.expectedPriorStateCanonicalSignature,
                request.itemBattleInputCanonical,
                request.expectedItemSessionCanonicalSignature,
                request.expectedItemCatalogCanonicalSignature
            }));
            foreach (C1FormalRealtimeBattleItemFactSnapshot row in
                     request.activeItemFacts)
            {
                value.Append("\nITEM|").Append(
                    row == null ? "null" : row.canonicalSignature);
            }
            if (request.nianCapacityFact != null)
            {
                value.Append("\nNIAN_CAPACITY|").Append(
                    request.nianCapacityFact.canonicalSignature);
            }

            if (request.replacePool15ActiveFacts)
            {
                value.Append("\nPOOL15_REPLACE|true|")
                    .Append(request.pool15ItemInputCanonical).Append('|')
                    .Append(request.pool15PoolCanonicalSignature).Append('|')
                    .Append(request.pool15SourceItemCatalogCanonicalSignature)
                    .Append('|').Append(request.pool15CandidateProfileId)
                    .Append('|').Append(request.pool15CandidateDataMaturity)
                    .Append('|').Append(
                        request.pool15CandidateProfileCanonicalSignature);
                foreach (C1FormalRealtimeBattlePool15ActiveItemFactSnapshot row in
                         request.activePool15ItemFacts)
                {
                    value.Append("\nPOOL15|").Append(
                        row == null ? "null" : row.canonicalSignature);
                }
            }

            return Hash(value.ToString());
        }

        internal static string ItemRefreshResult(
            C1FormalRealtimeBattleItemRefreshResult result)
        {
            if (result == null)
            {
                return string.Empty;
            }

            return Hash(string.Join("|", new[]
            {
                result.schemaId,
                result.accepted ? "true" : "false",
                result.errorCode,
                result.sessionId,
                result.sessionGeneration.ToString(CultureInfo.InvariantCulture),
                result.resetGeneration.ToString(CultureInfo.InvariantCulture),
                result.battleTimeMs.ToString(CultureInfo.InvariantCulture),
                result.itemBattleInputCanonical,
                result.acceptedCommandId,
                result.canceledFutureItemActionCount.ToString(
                    CultureInfo.InvariantCulture),
                result.retainedFutureItemActionCount.ToString(
                    CultureInfo.InvariantCulture),
                result.scheduledFutureItemActionCount.ToString(
                    CultureInfo.InvariantCulture),
                result.activeItemFactCount.ToString(
                    CultureInfo.InvariantCulture),
                result.stateSnapshot == null
                    ? string.Empty
                    : result.stateSnapshot.canonicalSignature,
                string.Join(",", result.emittedCues.Select(
                    cue => cue == null ? string.Empty : cue.canonicalSignature))
            }));
        }

        internal static string State(
            C1FormalRealtimeBattleSessionStateSnapshot state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            StringBuilder value = new StringBuilder(string.Join("|", new[]
            {
                state.schemaId,
                state.accepted ? "true" : "false",
                state.errorCode,
                state.sessionId,
                state.sessionGeneration.ToString(CultureInfo.InvariantCulture),
                state.resetGeneration.ToString(CultureInfo.InvariantCulture),
                state.phase,
                state.paused ? "true" : "false",
                state.terminal ? "true" : "false",
                state.battleTimeMs.ToString(CultureInfo.InvariantCulture),
                state.playerSnapshot == null
                    ? string.Empty
                    : state.playerSnapshot.canonicalSignature,
                state.acceptedItemApplicationCount.ToString(
                    CultureInfo.InvariantCulture),
                state.enemyApplicationCount.ToString(CultureInfo.InvariantCulture),
                state.cueSequence.ToString(CultureInfo.InvariantCulture),
                state.activePool15SourceCount.ToString(
                    CultureInfo.InvariantCulture),
                state.activePool15SourceCanonicalSignature,
                state.effectStateSnapshot == null
                    ? string.Empty
                    : state.effectStateSnapshot.canonicalSignature,
                state.pool15ApplicationCount.ToString(
                    CultureInfo.InvariantCulture),
                state.pool15CueCount.ToString(CultureInfo.InvariantCulture),
                state.nianStateSnapshot == null
                    ? string.Empty
                    : state.nianStateSnapshot.canonicalSignature
            }));
            foreach (C1FormalRealtimeBattleActorSnapshot actor in
                     state.actorSnapshots)
            {
                value.Append("\nACTOR|").Append(
                    actor == null ? "null" : actor.canonicalSignature);
            }

            foreach (C1FormalRealtimeBattleScheduledActionSnapshot action in
                     state.scheduledActions)
            {
                value.Append("\nACTION|").Append(
                    action == null ? "null" : action.canonicalSignature);
            }

            return Hash(value.ToString());
        }

        internal static string Terminal(
            C1FormalRealtimeBattleTerminalResult terminal)
        {
            if (terminal == null)
            {
                return string.Empty;
            }

            StringBuilder value = new StringBuilder(string.Join("|", new[]
            {
                terminal.schemaId,
                terminal.accepted ? "true" : "false",
                terminal.errorCode,
                terminal.sessionId,
                terminal.stageId,
                terminal.durationMs.ToString(CultureInfo.InvariantCulture),
                terminal.win ? "true" : "false",
                terminal.lose ? "true" : "false",
                terminal.playerSnapshot == null
                    ? string.Empty
                    : terminal.playerSnapshot.canonicalSignature,
                terminal.acceptedItemApplicationCount.ToString(
                    CultureInfo.InvariantCulture),
                terminal.enemyApplicationCount.ToString(
                    CultureInfo.InvariantCulture),
                terminal.shouldWriteSave ? "true" : "false",
                terminal.shouldGrantReward ? "true" : "false"
            }));
            foreach (C1FormalRealtimeBattleActorSnapshot actor in
                     terminal.actorSnapshots)
            {
                value.Append("\nACTOR|").Append(
                    actor == null ? "null" : actor.canonicalSignature);
            }

            return Hash(value.ToString());
        }

        internal static string Hash(string value)
        {
            byte[] hash;
            using (SHA256 algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(
                    new UTF8Encoding(false).GetBytes(value ?? string.Empty));
            }

            return string.Concat(hash.Select(part => part.ToString(
                "x2",
                CultureInfo.InvariantCulture)));
        }
    }
}
