using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items;

namespace TalismanBag.BattleBridge.DevSession
{
    public sealed class BattleSandboxDevBattleSessionEngine
    {
        private readonly List<BattleSandboxDevBattleLedgerEntry> ledger = new();
        private readonly List<BattleSandboxDevBattlePresentationCue> cues = new();

        private BattleSandboxDevBattleSessionRequest activeRequest;
        private BattleSandboxDevBattleSessionAcceptedStart acceptedStart;
        private BattleSandboxNianResourceEngine nianEngine;
        private BattleSandboxDevBattleSessionSnapshot current;
        private long runtimeGeneration = 1L;
        private long runtimeRevision;
        private long elapsedMilliseconds;
        private int playerHp;
        private int playerShield;
        private int enemyHp;
        private int itemFactOrdinal;
        private int enemyCueOrdinal;
        private int acceptedItemCount;
        private int rejectedItemCount;
        private int enemyBasicCount;
        private int enemySkillCount;
        private int enemyPlayerDamage;
        private BattleSandboxDevBattleSessionOutcome outcome;
        private int highestResetGeneration;
        private int lastAcceptedBattleIndex;

        public BattleSandboxDevBattleSessionSnapshot Current => current;
        public bool IsRunning => activeRequest != null
            && outcome == BattleSandboxDevBattleSessionOutcome.Running;
        public string LastDiagnosticCode { get; private set; } = "NONE";

        public bool TryStart(
            BattleSandboxDevBattleSessionRequest request,
            bool isEditor,
            out BattleSandboxDevBattleSessionAcceptedStart result)
        {
            result = null;
            if (activeRequest != null)
            {
                if (string.Equals(activeRequest.StartKey,
                        request?.StartKey,
                        StringComparison.Ordinal)
                    && acceptedStart != null)
                {
                    LastDiagnosticCode = "NONE";
                    result = acceptedStart;
                    return true;
                }

                LastDiagnosticCode = "DEV_SESSION_DUPLICATE_START_MISMATCH";
                return false;
            }
            if (!BattleSandboxDevBattleSessionContract.Validate(
                    request,
                    isEditor,
                    out string diagnostic))
            {
                LastDiagnosticCode = diagnostic;
                return false;
            }
            if (request.ResetGeneration < highestResetGeneration
                || (request.ResetGeneration == highestResetGeneration
                    && request.BattleIndex <= lastAcceptedBattleIndex))
            {
                LastDiagnosticCode = "DEV_SESSION_STALE_GENERATION_OR_BATTLE";
                return false;
            }

            activeRequest = request;
            highestResetGeneration = Math.Max(
                highestResetGeneration,
                request.ResetGeneration);
            lastAcceptedBattleIndex = request.BattleIndex;
            nianEngine = BattleSandboxNianResourceEngine.Create(
                request.I031Source);
            elapsedMilliseconds = 0L;
            playerHp = request.Encounter.PlayerMaxHp;
            playerShield = request.Encounter.PlayerInitialShield;
            enemyHp = request.Encounter.EnemyMaxHp;
            itemFactOrdinal = 0;
            enemyCueOrdinal = 0;
            acceptedItemCount = 0;
            rejectedItemCount = 0;
            enemyBasicCount = 0;
            enemySkillCount = 0;
            enemyPlayerDamage = 0;
            outcome = BattleSandboxDevBattleSessionOutcome.Running;
            ledger.Clear();
            cues.Clear();
            runtimeRevision++;
            AddLedger(
                0L,
                BattleSandboxDevBattleLedgerKind.Started,
                "START|B" + request.BattleIndex.ToString(
                    CultureInfo.InvariantCulture),
                string.Empty,
                string.Empty,
                nianEngine.Current.currentNian,
                nianEngine.Current.currentNian,
                enemyHp,
                enemyHp,
                playerShield,
                playerShield,
                playerHp,
                playerHp,
                request.MechanicsProfileId + "|"
                    + request.Encounter.EncounterProfileId + "|"
                    + request.SelectedRewardBaseItemId);
            AddCue(
                0L,
                BattleSandboxDevBattleCueKind.BattleStarted,
                string.Empty,
                string.Empty,
                0,
                0,
                0,
                0,
                "BATTLE_STARTED");
            if (request.LiHuoBuild2Active)
            {
                AddCue(
                    0L,
                    BattleSandboxDevBattleCueKind.Build2Activated,
                    request.SelectedRewardIdentityId,
                    request.SelectedRewardBaseItemId,
                    0,
                    0,
                    0,
                    0,
                    "LIHUO_BUILD2_ACTIVATED");
            }

            current = BuildSnapshot();
            acceptedStart = new BattleSandboxDevBattleSessionAcceptedStart(
                request.StartKey,
                request.ResetGeneration,
                request.HostSessionToken,
                request.BattleIndex,
                request.Encounter.EncounterProfileId,
                request.Encounter.ProfileFingerprint,
                current);
            result = acceptedStart;
            LastDiagnosticCode = "NONE";
            return true;
        }

        public bool TryAdvanceBy(
            long deltaMilliseconds,
            BattleSandboxDevBattleLiveSignatures liveSignatures,
            out BattleSandboxDevBattleSessionSnapshot snapshot)
        {
            snapshot = current;
            if (activeRequest == null || current == null)
            {
                LastDiagnosticCode = "DEV_SESSION_NOT_STARTED";
                return false;
            }
            if (deltaMilliseconds < 0L)
            {
                LastDiagnosticCode = "DEV_SESSION_NEGATIVE_DELTA_REJECTED";
                return false;
            }
            if (!activeRequest.SourceSignatures.Matches(liveSignatures))
            {
                LastDiagnosticCode = "DEV_SESSION_AUTHORITATIVE_SOURCE_DRIFT";
                return false;
            }
            if (current.IsTerminal || deltaMilliseconds == 0L)
            {
                LastDiagnosticCode = "NONE";
                return true;
            }

            long target = Math.Min(
                activeRequest.Encounter.TargetDurationMilliseconds,
                elapsedMilliseconds + deltaMilliseconds);
            while (outcome == BattleSandboxDevBattleSessionOutcome.Running)
            {
                long itemTick = (nianEngine.Current.pulseCount + 1L)
                    * BattleSandboxNianResourceEngine.PulseIntervalTicks;
                long enemyTick = enemyCueOrdinal
                    < activeRequest.Encounter.AcceptedEnemyActionCadence.Count
                    ? activeRequest.Encounter
                        .AcceptedEnemyActionCadence[enemyCueOrdinal]
                        .AtMilliseconds
                    : long.MaxValue;
                long terminalTick =
                    activeRequest.Encounter.TargetDurationMilliseconds;
                long next = Math.Min(itemTick, Math.Min(enemyTick, terminalTick));
                if (next > target)
                {
                    elapsedMilliseconds = target;
                    break;
                }

                elapsedMilliseconds = next;
                if (itemTick == next)
                {
                    ApplyScheduledItemPulse(next);
                }
                if (outcome == BattleSandboxDevBattleSessionOutcome.Running
                    && enemyTick == next)
                {
                    ApplyAcceptedEnemyAction(next);
                }
                if (outcome == BattleSandboxDevBattleSessionOutcome.Running
                    && terminalTick == next)
                {
                    Finish(
                        BattleSandboxDevBattleSessionOutcome.Timeout,
                        next,
                        "TARGET_DURATION_REACHED");
                }
                if (next == target)
                {
                    break;
                }
            }

            runtimeRevision++;
            current = BuildSnapshot();
            snapshot = current;
            LastDiagnosticCode = "NONE";
            return true;
        }

        public void Reset()
        {
            activeRequest = null;
            acceptedStart = null;
            nianEngine = null;
            current = null;
            elapsedMilliseconds = 0L;
            playerHp = 0;
            playerShield = 0;
            enemyHp = 0;
            itemFactOrdinal = 0;
            enemyCueOrdinal = 0;
            acceptedItemCount = 0;
            rejectedItemCount = 0;
            enemyBasicCount = 0;
            enemySkillCount = 0;
            enemyPlayerDamage = 0;
            outcome = BattleSandboxDevBattleSessionOutcome.Running;
            ledger.Clear();
            cues.Clear();
            runtimeGeneration++;
            runtimeRevision++;
            LastDiagnosticCode = "NONE";
        }

        private void ApplyScheduledItemPulse(long tick)
        {
            BattleSandboxDevBattleItemRequestFact fact =
                activeRequest.ItemRequestFacts[
                    itemFactOrdinal % activeRequest.ItemRequestFacts.Count];
            itemFactOrdinal++;
            string eventId = "B"
                + activeRequest.BattleIndex.ToString(CultureInfo.InvariantCulture)
                + "|P" + (nianEngine.Current.pulseCount + 1).ToString(
                    CultureInfo.InvariantCulture)
                + "|" + fact.RequestId;
            int nianBefore = nianEngine.Current.currentNian;
            BattleSandboxNianPulseResult pulse =
                nianEngine.ApplyActualItemCostPulse(
                    activeRequest.I031Source,
                    new BattleSandboxNianActualItemCostPulseRequest(
                        BattleSandboxNianActualItemCostPulseRequest
                            .CurrentSchemaId,
                        eventId,
                        activeRequest.ResetGeneration,
                        activeRequest.I031Source.canonicalSignature,
                        fact.SourceBaseItemId,
                        fact.NianCostRequestFactId,
                        tick));
            int spent = pulse.accepted
                ? pulse.resourceApplication.nianCost
                : 0;
            int afterGeneration = pulse.snapshot.currentNian + spent;
            int generated = afterGeneration - nianBefore;
            AddLedger(
                tick,
                BattleSandboxDevBattleLedgerKind.NianPulse,
                eventId + "|NP",
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.ItemId,
                nianBefore,
                afterGeneration,
                enemyHp,
                enemyHp,
                playerShield,
                playerShield,
                playerHp,
                playerHp,
                generated == activeRequest.I031Source.generationPerPulse
                    ? "GENERATED"
                    : "GENERATED_CAPPED");
            AddCue(
                tick,
                BattleSandboxDevBattleCueKind.NianGenerated,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.ItemId,
                generated,
                0,
                0,
                0,
                "I031_NIAN_GENERATED");

            if (!pulse.accepted)
            {
                rejectedItemCount++;
                AddLedger(
                    tick,
                    BattleSandboxDevBattleLedgerKind
                        .ItemApplicationRejected,
                    eventId,
                    fact.SourceItemInstanceId,
                    fact.SourceBaseItemId,
                    afterGeneration,
                    pulse.snapshot.currentNian,
                    enemyHp,
                    enemyHp,
                    playerShield,
                    playerShield,
                    playerHp,
                    playerHp,
                    pulse.reasonCode);
                AddCue(
                    tick,
                    BattleSandboxDevBattleCueKind
                        .ItemApplicationRejected,
                    fact.SourceItemInstanceId,
                    fact.SourceBaseItemId,
                    0,
                    0,
                    0,
                    0,
                    pulse.reasonCode);
                return;
            }

            int damage = (int)Math.Min(
                int.MaxValue,
                Math.Max(0L, fact.ResolvedDamageUnits));
            int enemyBefore = enemyHp;
            enemyHp = Math.Max(0, enemyHp - damage);
            acceptedItemCount++;
            AddLedger(
                tick,
                BattleSandboxDevBattleLedgerKind.ItemApplicationAccepted,
                eventId,
                fact.SourceItemInstanceId,
                fact.SourceBaseItemId,
                afterGeneration,
                pulse.snapshot.currentNian,
                enemyBefore,
                enemyHp,
                playerShield,
                playerShield,
                playerHp,
                playerHp,
                "ITEM_DAMAGE_ACCEPTED");
            AddCue(
                tick,
                BattleSandboxDevBattleCueKind.ItemApplicationAccepted,
                fact.SourceItemInstanceId,
                fact.SourceBaseItemId,
                -spent,
                enemyHp - enemyBefore,
                0,
                0,
                "ITEM_DAMAGE_ACCEPTED");
            if (enemyHp <= 0)
            {
                Finish(
                    BattleSandboxDevBattleSessionOutcome.Victory,
                    tick,
                    "ENEMY_HP_ZERO");
            }
        }

        private void ApplyAcceptedEnemyAction(long tick)
        {
            BattleSandboxExplicitDevEnemyActionCue action =
                activeRequest.Encounter
                    .AcceptedEnemyActionCadence[enemyCueOrdinal++];
            int damage = action.ActionKind ==
                BattleSandboxExplicitDevEnemyActionKind.Skill
                ? activeRequest.Encounter.SkillDamage
                : activeRequest.Encounter.BasicAttackDamage;
            int shieldBefore = playerShield;
            int hpBefore = playerHp;
            int shieldDamage = Math.Min(playerShield, damage);
            playerShield -= shieldDamage;
            int hpDamage = Math.Min(
                playerHp,
                Math.Max(0, damage - shieldDamage));
            playerHp -= hpDamage;
            enemyPlayerDamage += shieldDamage + hpDamage;
            bool skill = action.ActionKind ==
                BattleSandboxExplicitDevEnemyActionKind.Skill;
            if (skill)
            {
                enemySkillCount++;
            }
            else
            {
                enemyBasicCount++;
            }

            AddLedger(
                tick,
                skill
                    ? BattleSandboxDevBattleLedgerKind.EnemySkill
                    : BattleSandboxDevBattleLedgerKind.EnemyBasicAttack,
                "B" + activeRequest.BattleIndex.ToString(
                    CultureInfo.InvariantCulture)
                    + "|E" + action.Sequence.ToString(
                        CultureInfo.InvariantCulture),
                string.Empty,
                string.Empty,
                nianEngine.Current.currentNian,
                nianEngine.Current.currentNian,
                enemyHp,
                enemyHp,
                shieldBefore,
                playerShield,
                hpBefore,
                playerHp,
                skill ? "ENEMY_SKILL_ACCEPTED" : "ENEMY_BASIC_ACCEPTED");
            AddCue(
                tick,
                skill
                    ? BattleSandboxDevBattleCueKind.EnemySkill
                    : BattleSandboxDevBattleCueKind.EnemyBasicAttack,
                string.Empty,
                string.Empty,
                0,
                0,
                playerShield - shieldBefore,
                playerHp - hpBefore,
                skill ? "ENEMY_SKILL_ACCEPTED" : "ENEMY_BASIC_ACCEPTED");
            if (playerHp <= 0)
            {
                Finish(
                    BattleSandboxDevBattleSessionOutcome.Defeat,
                    tick,
                    "PLAYER_HP_ZERO");
            }
        }

        private void Finish(
            BattleSandboxDevBattleSessionOutcome terminalOutcome,
            long tick,
            string reason)
        {
            if (outcome != BattleSandboxDevBattleSessionOutcome.Running)
            {
                return;
            }
            outcome = terminalOutcome;
            AddLedger(
                tick,
                BattleSandboxDevBattleLedgerKind.Result,
                "B" + activeRequest.BattleIndex.ToString(
                    CultureInfo.InvariantCulture) + "|RESULT",
                string.Empty,
                string.Empty,
                nianEngine.Current.currentNian,
                nianEngine.Current.currentNian,
                enemyHp,
                enemyHp,
                playerShield,
                playerShield,
                playerHp,
                playerHp,
                reason);
            AddCue(
                tick,
                terminalOutcome == BattleSandboxDevBattleSessionOutcome.Victory
                    ? BattleSandboxDevBattleCueKind.BattleVictory
                    : terminalOutcome ==
                        BattleSandboxDevBattleSessionOutcome.Defeat
                        ? BattleSandboxDevBattleCueKind.BattleDefeat
                        : BattleSandboxDevBattleCueKind.BattleTimeout,
                string.Empty,
                string.Empty,
                0,
                0,
                0,
                0,
                reason);
        }

        private BattleSandboxDevBattleSessionSnapshot BuildSnapshot()
        {
            return new BattleSandboxDevBattleSessionSnapshot(
                runtimeGeneration,
                runtimeRevision,
                activeRequest.LabProfileId,
                activeRequest.MechanicsProfileId,
                activeRequest.BattleIndex,
                activeRequest.SelectedRewardBaseItemId,
                activeRequest.Encounter.EncounterProfileId,
                activeRequest.Encounter.ProfileFingerprint,
                activeRequest.Encounter.EnemyIdentity,
                elapsedMilliseconds,
                activeRequest.Encounter.PlayerMaxHp,
                activeRequest.Encounter.PlayerInitialShield,
                playerHp,
                playerShield,
                activeRequest.Encounter.EnemyMaxHp,
                enemyHp,
                BattleSandboxNianResourceSnapshot.InitialNian,
                nianEngine.Current.currentNian,
                nianEngine.Current.totalGenerated,
                nianEngine.Current.totalSpent,
                nianEngine.Current.rejectedApplicationCount,
                acceptedItemCount,
                rejectedItemCount,
                enemyBasicCount,
                enemySkillCount,
                enemyPlayerDamage,
                outcome,
                ledger,
                cues);
        }

        private void AddLedger(
            long tick,
            BattleSandboxDevBattleLedgerKind kind,
            string eventId,
            string sourceIdentity,
            string sourceBaseItemId,
            int nianBefore,
            int nianAfter,
            int enemyBefore,
            int enemyAfter,
            int shieldBefore,
            int shieldAfter,
            int hpBefore,
            int hpAfter,
            string reason)
        {
            ledger.Add(new BattleSandboxDevBattleLedgerEntry(
                ledger.Count + 1,
                tick,
                kind,
                eventId,
                sourceIdentity,
                sourceBaseItemId,
                nianBefore,
                nianAfter,
                enemyBefore,
                enemyAfter,
                shieldBefore,
                shieldAfter,
                hpBefore,
                hpAfter,
                reason));
        }

        private void AddCue(
            long tick,
            BattleSandboxDevBattleCueKind kind,
            string sourceIdentity,
            string sourceBaseItemId,
            int nianDelta,
            int enemyHpDelta,
            int shieldDelta,
            int hpDelta,
            string semanticKey)
        {
            cues.Add(new BattleSandboxDevBattlePresentationCue(
                cues.Count + 1,
                tick,
                kind,
                sourceIdentity,
                sourceBaseItemId,
                nianDelta,
                enemyHpDelta,
                shieldDelta,
                hpDelta,
                semanticKey));
        }
    }
}
