using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TalismanBag.BattleBridge.CampaignLoot;
using TalismanBag.BattleBridge.Formal;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance.C1Stage3To5Playtest;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.Items;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using UnityEditor;
using UnityEngine;

#if C1_FORMAL_REALTIME_BATTLE_SESSION_SCOPED_STANDALONE && !C1_FORMAL_REALTIME_BATTLE_SESSION_UNITY_REFERENCES
namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuItemAttribute : Attribute
    {
        public MenuItemAttribute(string path)
        {
        }
    }
}
#endif

#if C1_FORMAL_REALTIME_BATTLE_SESSION_SCOPED_STANDALONE && !C1_FORMAL_REALTIME_BATTLE_SESSION_UNITY_REFERENCES
namespace UnityEngine
{
    public static class Application
    {
        public static string dataPath => Path.Combine(
            Environment.CurrentDirectory,
            "Assets");
    }

    public static class Debug
    {
        public static void Log(object message)
        {
            Console.WriteLine(message);
        }

        public static void LogError(object message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
#endif

#if C1_FORMAL_REALTIME_BATTLE_SESSION_SCOPED_STANDALONE
public static class C1FormalRealtimeBattleSessionScopedProgram
{
    public static int Main()
    {
        TalismanBag.Editor.BattleBridge.C1FormalRealtimeBattleSessionVerifier
            .RunCurrentFocusedOrThrow();
        return 0;
    }
}
#endif

namespace TalismanBag.Editor.BattleBridge
{
    public static class C1FormalRealtimeBattleSessionVerifier
    {
        private const string InitialVerifierInstanceSignature =
            "verifier.initial.instance.signature";
        private const string RewardVerifierItemInstanceId =
            "verifier.reward.item.i002";
        private const string RewardVerifierInstanceSignature =
            "verifier.reward.instance.signature";
        private const string PackageName =
            "V0.4-C1Stage1To5FormalRealtimeBattleSession01";
        private const string TerminalMarker =
            "ACCEPTED_BATTLE_1_3_TO_1_5_RELEASE";
        [MenuItem(
            "TalismanBag/Verification/Run C1 Formal Realtime Battle Session Verifier")]
        public static void RunMenu()
        {
            int checks = RunCurrentFocusedOrThrow();
            Debug.Log(
                "CURRENT_FORMAL_BATTLE_WATER_PASS checks="
                + checks.ToString(CultureInfo.InvariantCulture));
        }

        public static void RunBatch()
        {
            RunCurrentFocusedOrThrow();
        }

        public static int RunCurrentFocusedOrThrow()
        {
            return RunPerActorEnemyActionsFocusedOrThrow()
                + RunEnemySkillComponentsFocusedOrThrow()
                + RunBattleFeedbackFactsFocusedOrThrow()
                + RunI031TriggerFactsFocusedOrThrow();
        }

        [MenuItem(
            "TalismanBag/Verification/Run Per-Actor Enemy Action Water Verifier")]
        public static void RunPerActorEnemyActionsMenu()
        {
            int checks = RunPerActorEnemyActionsFocusedOrThrow();
            Debug.Log(
                "PER_ACTOR_ENEMY_ACTION_WATER_PASS checks="
                + checks.ToString(CultureInfo.InvariantCulture));
        }

        public static int RunPerActorEnemyActionsFocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyCanonicalChapterOneEnemyProjection(context);
            VerifyPerActorEnemyActions(context);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "PER_ACTOR_ENEMY_ACTION_WATER_FAILED " + failures);
            }

            return context.Checks.Count;
        }

        [MenuItem(
            "TalismanBag/Verification/Run Enemy Skill Component Runtime Verifier")]
        public static void RunEnemySkillComponentsMenu()
        {
            int checks = RunEnemySkillComponentsFocusedOrThrow();
            Debug.Log(
                "ENEMY_SKILL_COMPONENT_RUNTIME_PASS checks="
                + checks.ToString(CultureInfo.InvariantCulture));
        }

        public static int RunEnemySkillComponentsFocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyEnemySkillDefinitions(context);
            VerifyEnemySkillRuntime(context);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "ENEMY_SKILL_COMPONENT_RUNTIME_FAILED " + failures);
            }

            return context.Checks.Count;
        }

        [MenuItem(
            "TalismanBag/Verification/Run Battle Feedback Fact Contract Verifier")]
        public static void RunBattleFeedbackFactsMenu()
        {
            int checks = RunBattleFeedbackFactsFocusedOrThrow();
            Debug.Log(
                "BATTLE_FEEDBACK_FACT_CONTRACT_PASS checks="
                + checks.ToString(CultureInfo.InvariantCulture));
        }

        public static int RunBattleFeedbackFactsFocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyBattleFeedbackFacts(context);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "BATTLE_FEEDBACK_FACT_CONTRACT_FAILED " + failures);
            }

            return context.Checks.Count;
        }

        [MenuItem(
            "TalismanBag/Verification/Run I031 Trigger Fact Verifier")]
        public static void RunI031TriggerFactsMenu()
        {
            int checks = RunI031TriggerFactsFocusedOrThrow();
            Debug.Log(
                "I031_TRIGGER_FACT_WATER_PASS checks="
                + checks.ToString(CultureInfo.InvariantCulture));
        }

        public static int RunI031TriggerFactsFocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyI031TriggerFacts(context);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "I031_TRIGGER_FACT_WATER_FAILED " + failures);
            }

            return context.Checks.Count;
        }

        private static void VerifyI031TriggerFacts(
            VerificationContext context)
        {
            long interval = C1FormalI031NianCapacityProjection
                .GenerationIntervalMilliseconds;
            bool started = TryStartCumulativeItemSnapshot(
                "i031-trigger-fact",
                "1-1",
                12106L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter adapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            C1FormalRealtimeBattleNianStateSnapshot initial = start
                ?.stateSnapshot?.nianStateSnapshot;
            bool scheduled = started
                && initial != null
                && initial.sourceItemInstanceId ==
                    I031InventoryPlacementContract.SpecialIdentityId
                && initial.sourceBaseItemId ==
                    I031InventoryPlacementContract.ItemId
                && initial.generationAmount ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && initial.generationIntervalMilliseconds == interval
                && initial.nextGenerationAtBattleTimeMs == interval;
            context.Check(
                "i031-authoritative-next-trigger",
                "nian-trigger",
                scheduled,
                "I031 exposes exact source, amount, interval and first due Battle time",
                scheduled ? "PASS" : "missing I031 schedule fact",
                "I031_NEXT_TRIGGER_FACT_MISSING");

            bool emptied = started
                && SetSessionNianForBoundaryVerification(
                    adapter.Session,
                    0);
            C1FormalRealtimeBattleTickResult beforeTick = null;
            bool beforeDue = emptied
                && adapter.TryAdvanceTo(interval - 1L, out beforeTick)
                && beforeTick.stateSnapshot.nianStateSnapshot.currentNian == 0
                && beforeTick.stateSnapshot.nianStateSnapshot
                    .nextGenerationAtBattleTimeMs == interval;
            C1FormalRealtimeBattleTickResult triggerTick = null;
            bool triggered = beforeDue
                && adapter.TryAdvanceTo(interval, out triggerTick);
            C1FormalRealtimeBattleCue cue = triggerTick?.emittedCues
                ?.SingleOrDefault(value => value.cueKind ==
                    C1FormalRealtimeBattleCueKinds.NianResourceChanged);
            bool committed = triggered
                && cue != null
                && cue.battleTimeMs == interval
                && cue.sourceItemInstanceId ==
                    I031InventoryPlacementContract.SpecialIdentityId
                && cue.sourceBaseItemId ==
                    I031InventoryPlacementContract.ItemId
                && cue.targetActorId ==
                    C1FormalRealtimeBattleSessionContract.PlayerActorId
                && cue.triggerKind ==
                    C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger
                && cue.deliveryKind ==
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic
                && cue.resultKind ==
                    C1FormalRealtimeBattleFeedbackResultKinds.NianGain
                && cue.resourceRequestedAmount ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && cue.resourceAppliedAmount ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && cue.resourceBefore == 0
                && cue.resourceAfter ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && triggerTick.stateSnapshot.nianStateSnapshot.currentNian ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && triggerTick.stateSnapshot.nianStateSnapshot
                    .nextGenerationAtBattleTimeMs == interval * 2L;
            context.Check(
                "i031-periodic-nian-mutation-cue",
                "nian-trigger",
                committed,
                "I031 commits Nian before an exact ITEM_TRIGGER / PERIODIC / NIAN_GAIN cue and schedules the next interval",
                committed ? "PASS" : "missing committed I031 trigger fact",
                "I031_TRIGGER_FACT_INVALID");
            adapter?.Reset();
        }

        private static void VerifyBattleFeedbackFacts(
            VerificationContext context)
        {
            bool hostStarted = TryStartCumulativeItemSnapshot(
                "feedback-fact-host",
                "1-1",
                12101L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter hostAdapter,
                out _,
                out _);
            bool hostAdvanced = hostStarted
                && hostAdapter.TryAdvanceTo(6500L, out _);
            C1FormalRealtimeBattleCue basic = hostAdapter?.Session.CueLedger
                .FirstOrDefault(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.DamageFloatPayload
                    && cue.sourceOwner ==
                        C1FormalRealtimeBattleSessionContract.EnemyOwner
                    && cue.triggerKind ==
                        C1FormalRealtimeBattleFeedbackTriggerKinds.BasicAction);
            bool basicFacts = hostAdvanced
                && basic != null
                && basic.sourceStableOrder >= 0
                && basic.deliveryKind ==
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant
                && basic.resultKind ==
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage
                && basic.targetActorId ==
                    C1FormalRealtimeBattleSessionContract.PlayerActorId
                && basic.targetStableOrder == -1
                && basic.acceptedApplicationEventId.Length > 0;
            context.Check(
                "feedback-enemy-basic",
                "feedback-fact",
                basicFacts,
                "exact Enemy / BASIC_ACTION / INSTANT / HP_DAMAGE / Player",
                basicFacts ? "PASS" : "missing basic feedback fact",
                "FEEDBACK_ENEMY_BASIC_FACT_MISSING");

            C1FormalRealtimeBattleCue skill = hostAdapter?.Session.CueLedger
                .FirstOrDefault(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.DamageFloatPayload
                    && cue.triggerKind ==
                        C1FormalRealtimeBattleFeedbackTriggerKinds.Skill);
            C1FormalRealtimeBattleCue periodic = hostAdapter?.Session.CueLedger
                .FirstOrDefault(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.DamageFloatPayload
                    && cue.triggerKind ==
                        C1FormalRealtimeBattleFeedbackTriggerKinds.StatusTrigger);
            bool skillAndPeriodic = skill != null
                && skill.sourceStableOrder >= 0
                && skill.deliveryKind ==
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant
                && skill.resultKind ==
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage
                && periodic != null
                && periodic.sourceStableOrder >= 0
                && periodic.deliveryKind ==
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Periodic
                && periodic.resultKind ==
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage
                && periodic.effectVariantId == "pollution";
            context.Check(
                "feedback-enemy-skill-and-status",
                "feedback-fact",
                skillAndPeriodic,
                "active Skill is instant and Pollution retains Enemy source through periodic delivery",
                skillAndPeriodic ? "PASS" : "skill/status fact missing",
                "FEEDBACK_ENEMY_SKILL_STATUS_FACT_MISSING");
            hostAdapter?.Reset();

            bool houndStarted = TryStartCumulativeItemSnapshot(
                "feedback-fact-hound",
                "1-3",
                12102L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter houndAdapter,
                out _,
                out _);
            C1FormalRealtimeBattleCue shellGain = houndAdapter?.Session
                .CueLedger.FirstOrDefault(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.ActorShellChanged
                    && cue.resultKind ==
                        C1FormalRealtimeBattleFeedbackResultKinds.ShellGain);
            bool shellFacts = houndStarted
                && shellGain != null
                && shellGain.sourceId == shellGain.targetActorId
                && shellGain.sourceStableOrder == shellGain.targetStableOrder
                && shellGain.triggerKind ==
                    C1FormalRealtimeBattleFeedbackTriggerKinds.Passive
                && shellGain.deliveryKind ==
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant
                && shellGain.appliedDamage == 60;
            context.Check(
                "feedback-enemy-shell-passive",
                "feedback-fact",
                shellFacts,
                "exact Hound / PASSIVE / INSTANT / SHELL_GAIN / same Hound",
                shellFacts ? "PASS" : "shell feedback fact missing",
                "FEEDBACK_ENEMY_SHELL_FACT_MISSING");
            houndAdapter?.Reset();

            bool remnantStarted = TryStartCumulativeItemSnapshot(
                "feedback-fact-remnant",
                "1-4",
                12103L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter remnantAdapter,
                out _,
                out _);
            bool remnantReady = remnantStarted
                && RetireFirstActorForEnemySkillVerifier(
                    remnantAdapter.Session)
                && remnantAdapter.TryAdvanceTo(3000L, out _)
                && TriggerOnDamagedForEnemySkillVerifier(
                    remnantAdapter.Session,
                    10);
            C1FormalRealtimeBattleCue passive = remnantAdapter?.Session
                .CueLedger.FirstOrDefault(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.DamageFloatPayload
                    && cue.triggerKind ==
                        C1FormalRealtimeBattleFeedbackTriggerKinds.Passive);
            bool passiveFacts = remnantReady
                && passive != null
                && passive.sourceStableOrder >= 0
                && passive.deliveryKind ==
                    C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant
                && passive.resultKind ==
                    C1FormalRealtimeBattleFeedbackResultKinds.HpDamage
                && passive.targetActorId ==
                    C1FormalRealtimeBattleSessionContract.PlayerActorId;
            context.Check(
                "feedback-enemy-passive-reflect",
                "feedback-fact",
                passiveFacts,
                "exact Remnant / PASSIVE / INSTANT / HP_DAMAGE / Player",
                passiveFacts ? "PASS" : "passive feedback fact missing",
                "FEEDBACK_ENEMY_PASSIVE_FACT_MISSING");
            remnantAdapter?.Reset();

            bool itemFacts;
            string itemActual;
            try
            {
                C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier
                    .VerifyDirectDamageFeedbackFactOrThrow();
                itemFacts = true;
                itemActual = "PASS";
            }
            catch (Exception exception)
            {
                itemFacts = false;
                itemActual = exception.Message;
            }
            context.Check(
                "feedback-item-direct-damage",
                "feedback-fact",
                itemFacts,
                "exact ItemInstance / ITEM_TRIGGER / INSTANT / HP_DAMAGE / exact Enemy",
                itemActual,
                "FEEDBACK_ITEM_DAMAGE_FACT_MISSING");

            bool guardStarted = TryStartCumulativeItemSnapshot(
                "feedback-fact-guard",
                "1-1",
                12105L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter guardAdapter,
                out _,
                out _);
            bool guardReady = guardStarted
                && SetSessionGuardForBoundaryVerification(
                    guardAdapter.Session,
                    10L);
            bool guardAdvanced = guardReady
                && guardAdapter.TryAdvanceTo(3000L, out _);
            C1FormalRealtimeBattleCue guardDamage = guardAdapter?.Session
                .CueLedger.FirstOrDefault(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.PlayerGuardChanged);
            bool guardFacts = guardAdvanced
                && guardDamage != null
                && guardDamage.appliedDamage > 0
                && guardDamage.sourceStableOrder >= 0
                && guardDamage.resultKind ==
                    C1FormalRealtimeBattleFeedbackResultKinds.GuardDamage
                && guardDamage.targetActorId ==
                    C1FormalRealtimeBattleSessionContract.PlayerActorId;
            context.Check(
                "feedback-player-guard-damage",
                "feedback-fact",
                guardFacts,
                "incoming Enemy damage exposes the real Guard absorption as GUARD_DAMAGE",
                guardFacts ? "PASS" : "ready=" + guardReady
                    + ";guard damage fact missing",
                "FEEDBACK_GUARD_DAMAGE_FACT_MISSING");
            guardAdapter?.Reset();
        }

        private static void VerifyEnemySkillDefinitions(
            VerificationContext context)
        {
            C1FormalEnemyDefinition host =
                C1FormalEnemyDefinitionCatalog.FindByContentId(
                    C1EnemyRuntimeContract.ShatteredHostContentId);
            C1FormalEnemyDefinition hound =
                C1FormalEnemyDefinitionCatalog.FindByContentId(
                    C1EnemyRuntimeContract.PorcelainHoundContentId);
            C1FormalEnemyDefinition remnant =
                C1FormalEnemyDefinitionCatalog.FindByContentId(
                    C1EnemyRuntimeContract.BoneSwapRemnantContentId);
            bool reusableSchema = host?.Skills.Count == 2
                && hound?.Skills.Count == 2
                && remnant?.Skills.Count == 2
                && new[] { host, hound, remnant }.All(definition =>
                    definition.Skills.Any(skill => string.Equals(
                        skill.ActivationType,
                        C1FormalEnemySkillActivationTypes.Active,
                        StringComparison.Ordinal))
                    && definition.Skills.Any(skill => string.Equals(
                        skill.ActivationType,
                        C1FormalEnemySkillActivationTypes.Passive,
                        StringComparison.Ordinal)));
            context.Check(
                "enemy-skill-component-schema",
                "enemy-skill-definition",
                reusableSchema,
                "each Enemy composes one active and one passive from shared effect/status kinds",
                reusableSchema ? "PASS" : "definitions incomplete",
                "ENEMY_SKILL_COMPONENT_SCHEMA_INVALID");
        }

        private static void VerifyEnemySkillRuntime(
            VerificationContext context)
        {
            bool hostStarted = TryStartCumulativeItemSnapshot(
                "enemy-skill-host",
                "1-1",
                12001L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter hostAdapter,
                out _,
                out _);
            C1FormalRealtimeBattleTickResult hostTick = null;
            bool hostAdvanced = hostStarted
                && hostAdapter.TryAdvanceTo(6500L, out hostTick);
            bool pollution = hostAdvanced
                && hostTick.stateSnapshot.statusSnapshots.Any(status =>
                    string.Equals(
                        status.statusKey,
                        "pollution",
                        StringComparison.Ordinal)
                    && status.targetStableOrder == -1
                    && status.stackCount == 3
                    && status.maxStack == 3)
                && hostTick.emittedCues.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.StatusTickAccepted)
                && hostTick.emittedCues.Concat(
                        hostAdapter.Session.CueLedger)
                    .Any(cue => cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.EnemySkillAccepted
                        && cue.effectVariantId ==
                        C1EnemyRuntimeContract.ShatteredHostSkillActionId);
            bool pollutionExpired = pollution
                && RetireAllActorsForEnemySkillVerifier(
                    hostAdapter.Session)
                && hostAdapter.TryAdvanceTo(14000L, out _)
                && !hostAdapter.Session.StatusSnapshots.Any(status =>
                    status.statusKey == "pollution")
                && hostAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.StatusRemoved
                    && cue.effectVariantId == "pollution");
            context.Check(
                "enemy-skill-pollution-runtime",
                "enemy-status-runtime",
                pollution && pollutionExpired,
                "pollution stacks to three, owns timed damage ticks, active-skill damage and expiry",
                pollution && pollutionExpired ? "PASS" : "start="
                    + hostStarted + ";advance=" + hostAdvanced
                    + ";expired=" + pollutionExpired,
                "ENEMY_POLLUTION_RUNTIME_INVALID");
            hostAdapter?.Reset();

            bool houndStarted = TryStartCumulativeItemSnapshot(
                "enemy-skill-hound",
                "1-3",
                12002L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter houndAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot houndStart);
            C1FormalRealtimeBattleActorSnapshot houndActor = houndStart
                ?.stateSnapshot?.actorSnapshots.FirstOrDefault(actor =>
                    actor.contentId ==
                        C1EnemyRuntimeContract.PorcelainHoundContentId);
            bool houndAdvanced = houndStarted
                && houndAdapter.TryAdvanceTo(7000L, out _);
            bool houndSkill = houndAdvanced
                && houndActor != null
                && houndActor.currentShell == 60
                && houndAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.EnemySkillAccepted
                    && cue.sourceId == houndActor.actorBalanceId
                    && cue.effectVariantId ==
                        C1FormalEnemyDefinitionCatalog
                            .PorcelainHoundChargeSkillId
                    && cue.requestedDamage == 11);
            context.Check(
                "enemy-skill-hound-runtime",
                "enemy-skill-runtime",
                houndSkill,
                "OnSpawn grants Shell and the timed charge commits eleven damage",
                houndSkill ? "PASS" : "start=" + houndStarted
                    + ";advance=" + houndAdvanced,
                "ENEMY_HOUND_SKILL_RUNTIME_INVALID");
            houndAdapter?.Reset();

            bool remnantStarted = TryStartCumulativeItemSnapshot(
                "enemy-skill-remnant",
                "1-4",
                12003L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter remnantAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot remnantStart);
            bool isolatedRemnant = remnantStarted
                && RetireFirstActorForEnemySkillVerifier(
                    remnantAdapter.Session);
            bool passiveBeforeBuff = isolatedRemnant
                && TriggerOnDamagedForEnemySkillVerifier(
                    remnantAdapter.Session,
                    10);
            bool reflectWindowReady = isolatedRemnant
                && remnantAdapter.TryAdvanceTo(3000L, out _)
                && remnantAdapter.Session.StatusSnapshots.Any(status =>
                    status.statusKey == "bone_swap_reflect"
                    && status.targetStableOrder >= 0
                    && status.stackCount > 0);
            bool damageTriggerSubmitted = reflectWindowReady
                && TriggerOnDamagedForEnemySkillVerifier(
                    remnantAdapter.Session,
                    10);
            bool passiveTriggered = damageTriggerSubmitted
                && remnantAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.EnemyPassiveTriggered
                    && cue.effectVariantId ==
                        C1FormalEnemyDefinitionCatalog.BoneSwapReflectSkillId
                    && cue.appliedDamage > 0);
            bool reflectStatus = remnantAdapter?.Session.StatusSnapshots.Any(
                status => status.statusKey == "bone_swap_reflect"
                    && status.targetStableOrder >= 0
                    && status.stackCount > 0) == true;
            context.Check(
                "enemy-skill-remnant-runtime",
                "enemy-skill-runtime",
                !passiveBeforeBuff && passiveTriggered && reflectStatus,
                "active reflect window is live and player damage causes immediate passive return without an Enemy basic attack",
                "started=" + remnantStarted + ";error="
                + (remnantStart == null
                    ? "missing"
                    : remnantStart.errorCode)
                + ";isolated=" + isolatedRemnant + ";passive="
                + passiveTriggered + ";status=" + reflectStatus
                + ";beforeBuff=" + passiveBeforeBuff
                + ";damageTrigger=" + damageTriggerSubmitted,
                "ENEMY_REMNANT_SKILL_RUNTIME_INVALID");
            remnantAdapter?.Reset();
        }

        private static bool RetireFirstActorForEnemySkillVerifier(
            C1FormalRealtimeBattleSession session)
        {
            FieldInfo actorsField = typeof(C1FormalRealtimeBattleSession)
                .GetField(
                    "actors",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            IList actorRows = actorsField?.GetValue(session) as IList;
            if (actorRows == null || actorRows.Count < 2)
            {
                return false;
            }

            object first = actorRows[0];
            PropertyInfo currentHp = first.GetType().GetProperty(
                "CurrentHp",
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic);
            if (currentHp == null)
            {
                return false;
            }

            currentHp.SetValue(first, 0);
            return true;
        }

        private static bool RetireAllActorsForEnemySkillVerifier(
            C1FormalRealtimeBattleSession session)
        {
            FieldInfo actorsField = typeof(C1FormalRealtimeBattleSession)
                .GetField(
                    "actors",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            IList actorRows = actorsField?.GetValue(session) as IList;
            if (actorRows == null || actorRows.Count == 0)
            {
                return false;
            }

            foreach (object actor in actorRows)
            {
                PropertyInfo currentHp = actor.GetType().GetProperty(
                    "CurrentHp",
                    BindingFlags.Instance
                    | BindingFlags.Public
                    | BindingFlags.NonPublic);
                if (currentHp == null)
                {
                    return false;
                }
                currentHp.SetValue(actor, 0);
            }
            return true;
        }

        private static bool TriggerOnDamagedForEnemySkillVerifier(
            C1FormalRealtimeBattleSession session,
            int incomingAppliedDamage)
        {
            FieldInfo actorsField = typeof(C1FormalRealtimeBattleSession)
                .GetField(
                    "actors",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            IList actorRows = actorsField?.GetValue(session) as IList;
            MethodInfo trigger = typeof(C1FormalRealtimeBattleSession)
                .GetMethod(
                    "TriggerEnemySkills",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            if (actorRows == null || actorRows.Count < 2 || trigger == null)
            {
                return false;
            }

            var emitted = new List<C1FormalRealtimeBattleCue>();
            trigger.Invoke(
                session,
                new object[]
                {
                    actorRows[1],
                    C1FormalEnemySkillTriggerTypes.OnDamaged,
                    incomingAppliedDamage,
                    "verifier.enemy.skill.source-application",
                    emitted
                });
            return emitted.Any(cue => cue.cueKind ==
                C1FormalRealtimeBattleCueKinds.EnemyPassiveTriggered);
        }

        private static void VerifyCanonicalChapterOneEnemyProjection(
            VerificationContext context)
        {
            var expectedRosters = new Dictionary<string, string[]>(
                StringComparer.Ordinal)
            {
                {
                    "1-1",
                    new[]
                    {
                        C1EnemyRuntimeContract.ShatteredHostContentId,
                        C1EnemyRuntimeContract.ShatteredHostContentId
                    }
                },
                {
                    "1-2",
                    new[]
                    {
                        C1EnemyRuntimeContract.ShatteredHostContentId,
                        C1EnemyRuntimeContract.ShatteredHostContentId,
                        C1EnemyRuntimeContract.ShatteredHostContentId
                    }
                },
                {
                    "1-3",
                    new[]
                    {
                        C1EnemyRuntimeContract.ShatteredHostContentId,
                        C1EnemyRuntimeContract.PorcelainHoundContentId
                    }
                },
                {
                    "1-4",
                    new[]
                    {
                        C1EnemyRuntimeContract.PorcelainHoundContentId,
                        C1EnemyRuntimeContract.BoneSwapRemnantContentId
                    }
                },
                {
                    "1-5",
                    new[]
                    {
                        C1EnemyRuntimeContract.ShatteredHostContentId,
                        C1EnemyRuntimeContract.PorcelainHoundContentId,
                        C1EnemyRuntimeContract.BoneSwapRemnantContentId
                    }
                }
            };

            bool valid = true;
            var diagnostics = new List<string>();
            long generation = 11800L;
            foreach (KeyValuePair<string, string[]> expected in
                     expectedRosters)
            {
                bool started = TryStartCumulativeItemSnapshot(
                    "canonical-enemy-" + expected.Key,
                    expected.Key,
                    generation++,
                    Array.Empty<C1FormalItemBattleItemRow>(),
                    out C1FormalRealtimeBattleSessionAdapter adapter,
                    out _,
                    out C1FormalRealtimeBattleSessionStartSnapshot start);
                C1FormalRealtimeBattleActorSnapshot[] actors =
                    (start?.stateSnapshot?.actorSnapshots
                        ?? Array.Empty<C1FormalRealtimeBattleActorSnapshot>())
                    .OrderBy(actor => actor.stableActorOrder)
                    .ToArray();
                C1FormalRealtimeBattleScheduledActionSnapshot[] actions =
                    (start?.stateSnapshot?.scheduledActions
                        ?? Array.Empty<
                            C1FormalRealtimeBattleScheduledActionSnapshot>())
                    .Where(action => !action.executed
                        && string.Equals(
                            action.actionKind,
                            C1FormalRealtimeBattleScheduledActionKinds
                                .EnemyBasicWave,
                            StringComparison.Ordinal))
                    .ToArray();
                bool stageValid = started
                    && actors.Select(actor => actor.contentId)
                        .SequenceEqual(expected.Value)
                    && actions.Length == actors.Length;
                for (int actorIndex = 0; actorIndex < actors.Length;
                     actorIndex++)
                {
                    C1FormalRealtimeBattleActorSnapshot actor =
                        actors[actorIndex];
                    C1FormalEnemyDefinition definition =
                        C1FormalEnemyDefinitionCatalog.FindByContentId(
                            actor.contentId);
                    C1FormalRealtimeBattleScheduledActionSnapshot action =
                        actions.SingleOrDefault(candidate => string.Equals(
                            candidate.sourceId,
                            actor.actorBalanceId,
                            StringComparison.Ordinal));
                    stageValid = stageValid
                        && definition != null
                        && actor.maxHp == definition.MaxHp
                        && actor.currentHp == definition.MaxHp
                        && actor.maxShell == definition.MaxShell
                        && actor.currentShell == definition.MaxShell
                        && action != null
                        && action.requestedDamage ==
                            definition.PrimaryAttack.Damage
                        && action.dueBattleTimeMs ==
                            definition.PrimaryAttack.FirstDueMilliseconds
                            + actorIndex * 400L;
                }

                int expectedActiveSkills = expected.Value.Sum(contentId =>
                    C1FormalEnemyDefinitionCatalog.FindByContentId(contentId)
                        ?.Skills.Count(skill => string.Equals(
                            skill.ActivationType,
                            C1FormalEnemySkillActivationTypes.Active,
                            StringComparison.Ordinal)) ?? 0);
                int actualActiveSkills = (start?.stateSnapshot
                        ?.scheduledActions
                        ?? Array.Empty<
                            C1FormalRealtimeBattleScheduledActionSnapshot>())
                    .Count(action => !action.executed
                        && action.actionKind ==
                            C1FormalRealtimeBattleScheduledActionKinds
                                .EnemySkill);
                stageValid = stageValid
                    && actualActiveSkills == expectedActiveSkills;
                valid = valid && stageValid;
                diagnostics.Add(
                    expected.Key + "=" + string.Join(",", actors.Select(
                        actor => actor.contentId + ":hp" + actor.maxHp
                        + ":shell" + actor.maxShell))
                    + ":activeSkills" + actualActiveSkills);
                adapter?.Reset();
            }

            context.Check(
                "canonical-chapter-one-enemy-projection",
                "enemy-definition",
                valid,
                "stages 1-1 through 1-5 consume the shared Enemy definitions",
                string.Join(";", diagnostics),
                "CANONICAL_CHAPTER_ONE_ENEMY_PROJECTION_INVALID");
        }

#if false // Retired Pool15 / compatibility QA; remove after current proof compiles.
        public static int RunFocused()
        {
            try
            {
                VerificationContext context = RunVerification();
                foreach (CheckRow failure in context.Checks.Where(
                             row => !row.Passed))
                {
                    LogError(
                        failure.Name + "|" + failure.Code + "|expected="
                        + failure.Expected + "|actual=" + failure.Actual);
                }

                return context.FailureCount == 0 ? 0 : 1;
            }
            catch (Exception exception)
            {
                LogError(exception);
                return 1;
            }
        }

        public static int RunRev10FocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyContractShape(context);
            VerifyPausedLiveItemRefresh(context);
            ScenarioTrace baseline = RunScenario(
                "1-1",
                false,
                101L,
                context);
            VerifyTrace(context, baseline, 16000L, 3, 76, 8, 2);
            ScenarioTrace weakBaseline = RunScenario(
                "1-2-weak",
                false,
                102L,
                context);
            VerifyTrace(context, weakBaseline, 48000L, 10, 20, 24, 3);
            ScenarioTrace targetBaseline = RunScenario(
                "1-2-target",
                true,
                103L,
                context);
            VerifyTrace(context, targetBaseline, 36000L, 7, 44, 35, 3);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "REV10_FOCUSED_BATTLE_TEST_FAILED " + failures);
            }

            return context.Checks.Count;
        }

        public static int RunRev11NianFocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyFormalNianAndCombatFact(context);
            VerifyFormalShellAndI003(context);
            VerifyFormalI004(context);
            VerifyGroupedRemainingPool15Supported(context);
            VerifyCurrentAdapterLiveRegression(context);
            VerifyCurrentStageAndBoneSwapRegression(context);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "REV11_NIAN_FOCUSED_BATTLE_TEST_FAILED " + failures);
            }

            Console.WriteLine(
                "REV11_NIAN_FOCUSED_BATTLE_PASS checks="
                + context.Checks.Count.ToString(
                    CultureInfo.InvariantCulture));
            return context.Checks.Count;
        }

        public static int RunI031GenerationFocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyFormalI031GenerationOnly(context);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "I031_GENERATION_FOCUSED_BATTLE_TEST_FAILED " + failures);
            }

            Console.WriteLine(
                "I031_GENERATION_FOCUSED_BATTLE_PASS checks="
                + context.Checks.Count.ToString(
                    CultureInfo.InvariantCulture));
            return context.Checks.Count;
        }

        private static void VerifyFormalI031GenerationOnly(
            VerificationContext context)
        {
            bool generationStarted = TryStartCumulativeItemSnapshot(
                "i031-generation-only",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                113100L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter generationAdapter,
                out _,
                out _);
            bool emptied = generationStarted
                && SetSessionNianForBoundaryVerification(
                    generationAdapter.Session,
                    0);
            C1FormalRealtimeBattleTickResult beforeGenerationTick = null;
            bool beforeGeneration = emptied
                && generationAdapter.TryAdvanceTo(
                    C1FormalI031NianCapacityProjection
                        .GenerationIntervalMilliseconds - 1L,
                    out beforeGenerationTick)
                && beforeGenerationTick.stateSnapshot.nianStateSnapshot
                    .currentNian == 0
                && generationAdapter.Session.NianTransactionLedger.All(row =>
                    row.transactionKind !=
                        C1FormalRealtimeBattleNianTransactionKinds.Generation);
            C1FormalRealtimeBattleTickResult generationTick = null;
            bool generated = beforeGeneration
                && generationAdapter.TryAdvanceTo(
                    C1FormalI031NianCapacityProjection
                        .GenerationIntervalMilliseconds,
                    out generationTick);
            C1FormalRealtimeBattleNianTransactionSnapshot generation =
                generationAdapter.Session.NianTransactionLedger
                    .SingleOrDefault(row =>
                        row.transactionKind ==
                            C1FormalRealtimeBattleNianTransactionKinds
                                .Generation);
            bool generationAccepted = generated
                && generation != null
                && generation.requestedAmount ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && generation.appliedAmount ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && generation.before == 0
                && generation.after ==
                    C1FormalI031NianCapacityProjection.GenerationAmount
                && generationTick.stateSnapshot.nianStateSnapshot.currentNian
                    == C1FormalI031NianCapacityProjection.GenerationAmount
                && generation.sourceItemInstanceId ==
                    I031InventoryPlacementContract.SpecialIdentityId
                && generation.sourceBaseItemId ==
                    I031InventoryPlacementContract.ItemId
                && generationAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.NianResourceChanged
                    && cue.resourceTransactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Generation
                    && cue.sourceItemInstanceId ==
                        I031InventoryPlacementContract.SpecialIdentityId
                    && cue.resourceBefore == 0
                    && cue.resourceAfter ==
                        C1FormalI031NianCapacityProjection.GenerationAmount);
            context.Check(
                "formal-i031-generates-live-nian-on-session-time",
                "nian",
                generationAccepted,
                "0 through 499ms; I031 adds the formal generation amount at 500ms with exact source cue",
                "started=" + generationStarted + ";before="
                + beforeGeneration + ";generated=" + generated + ";nian="
                + (generationTick?.stateSnapshot?.nianStateSnapshot == null
                    ? -1
                    : generationTick.stateSnapshot.nianStateSnapshot
                        .currentNian),
                "FORMAL_I031_NIAN_GENERATION_FAILED");

            bool filled = generationAccepted
                && SetSessionNianForBoundaryVerification(
                    generationAdapter.Session,
                    C1FormalI031NianCapacityProjection.MaxNian);
            int generationCountAtCap = generationAdapter.Session
                .NianTransactionLedger.Count(row =>
                    row.transactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Generation);
            C1FormalRealtimeBattleTickResult cappedTick = null;
            bool capped = filled
                && generationAdapter.TryAdvanceTo(
                    C1FormalI031NianCapacityProjection
                        .GenerationIntervalMilliseconds * 2L,
                    out cappedTick)
                && cappedTick.stateSnapshot.nianStateSnapshot.currentNian
                    == C1FormalI031NianCapacityProjection.MaxNian
                && generationAdapter.Session.NianTransactionLedger.Count(row =>
                    row.transactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Generation)
                    == generationCountAtCap;
            context.Check(
                "formal-i031-generation-does-not-overflow",
                "nian",
                capped,
                "full Nian remains at the formal maximum without a no-op transaction",
                "filled=" + filled + ";nian="
                + (cappedTick?.stateSnapshot?.nianStateSnapshot == null
                    ? -1
                    : cappedTick.stateSnapshot.nianStateSnapshot.currentNian),
                "FORMAL_I031_NIAN_OVERFLOW_FAILED");
            generationAdapter.Reset();
        }

        private static void VerifyFormalNianAndCombatFact(
            VerificationContext context)
        {
            C1FormalItemBattleItemRow i001 = CreateBattleItemRow(
                "I001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                InitialVerifierInstanceSignature,
                true,
                true);
            bool spendStarted = TryStartCumulativeItemSnapshot(
                "rev11-nian-spend",
                C1Lv1EarlyEncounterBalanceContract.Stage1_1,
                11101L,
                new[] { i001 },
                out C1FormalRealtimeBattleSessionAdapter spendAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot spendStart);
            int enemyHpBeforeSpend = spendStart?.stateSnapshot == null
                ? -1
                : spendStart.stateSnapshot.actorSnapshots.Sum(actor =>
                    actor.currentHp);
            C1FormalRealtimeBattleTickResult spendTick = null;
            bool spendAdvanced = spendStarted
                && spendAdapter.TryAdvanceTo(
                    i001.actionCostFact.firstOffsetMilliseconds,
                    out spendTick);
            C1FormalRealtimeBattleNianTransactionSnapshot spend = spendAdapter
                .Session.NianTransactionLedger.FirstOrDefault();
            bool spendAccepted = spendAdvanced
                && spend != null
                && spend.transactionKind ==
                    C1FormalRealtimeBattleNianTransactionKinds.Spend
                && spend.sourceItemInstanceId == i001.itemInstanceId
                && spend.requestedAmount == i001.actionCostFact.cost
                && spend.appliedAmount == i001.actionCostFact.cost
                && spend.before == C1FormalI031NianCapacityProjection.InitialNian
                && spend.after == spend.before - i001.actionCostFact.cost
                && spendTick.stateSnapshot.nianStateSnapshot.currentNian
                    == spend.after
                && spendTick.stateSnapshot.actorSnapshots.Sum(actor =>
                    actor.currentHp) < enemyHpBeforeSpend
                && spendAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.NianResourceChanged
                    && cue.resourceTransactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Spend
                    && cue.resourceBefore == spend.before
                    && cue.resourceAfter == spend.after
                    && cue.sourceItemInstanceId == i001.itemInstanceId);
            context.Check(
                "formal-nian-spend-before-damage",
                "nian",
                spendAccepted,
                "formal Nian Spend precedes accepted live damage",
                spend == null ? "missing" : spend.before + "->" + spend.after,
                "FORMAL_NIAN_SPEND_FAILED");
            spendAdapter.Reset();
            bool resetCleared = spendAdapter.Session.NianTransactionLedger.Count
                == 0;
            context.Check(
                "formal-nian-reset-clears-transactions",
                "lifecycle",
                resetCleared,
                "0 retained transactions",
                spendAdapter.Session.NianTransactionLedger.Count.ToString(
                    CultureInfo.InvariantCulture),
                "FORMAL_NIAN_RESET_FAILED");

            const string rewardedI002Instance =
                "verifier.rev11.rewarded.i002";
            C1FormalItemBattleItemRow rewardedI002 = CreatePool15BattleItemRow(
                "I002",
                rewardedI002Instance,
                true,
                true);
            bool refundStarted = TryStartCumulativeItemSnapshot(
                "rev11-nian-refund",
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                11102L,
                new[] { i001, rewardedI002 },
                out C1FormalRealtimeBattleSessionAdapter refundAdapter,
                out _,
                out _);
            bool refundAdvanced = refundStarted
                && refundAdapter.TryAdvanceTo(
                    rewardedI002.actionCostFact.firstOffsetMilliseconds,
                    out _);
            C1FormalRealtimeBattleNianTransactionSnapshot refund = refundAdapter
                .Session.NianTransactionLedger.FirstOrDefault(row =>
                    row.transactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Refund
                    && row.sourceItemInstanceId == rewardedI002Instance);
            bool refundAccepted = refundAdvanced
                && refund != null
                && refund.appliedAmount > 0
                && refund.after > refund.before
                && refund.after <= C1FormalI031NianCapacityProjection.MaxNian
                && refundAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.NianResourceChanged
                    && cue.resourceTransactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Refund
                    && cue.resourceAppliedAmount == refund.appliedAmount
                    && cue.sourceItemInstanceId == rewardedI002Instance
                    && cue.acceptedApplicationEventId
                        == refund.acceptedApplicationEventId);
            context.Check(
                "rewarded-i002-refunds-same-live-nian",
                "nian",
                refundAccepted,
                "nonzero capped Refund with exact rewarded ItemInstance cue",
                refund == null
                    ? "start=" + refundStarted + ";advance=" + refundAdvanced
                      + ";tx=" + string.Join(",", refundAdapter.Session
                          .NianTransactionLedger.Select(row =>
                              row.transactionKind + ":"
                              + row.sourceItemInstanceId + ":" + row.before
                              + ">" + row.after))
                    : refund.before + "->" + refund.after
                      + "/applied=" + refund.appliedAmount,
                "FORMAL_NIAN_REFUND_FAILED");
            refundAdapter.Reset();

            VerifyFormalI031GenerationOnly(context);

            const string guardInstance = "verifier.rev11.i013.guard";
            bool guardStarted = TryStartCumulativeItemSnapshot(
                "rev11-combat-fact-guard",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                11104L,
                new[]
                {
                    CreatePool15BattleItemRow(
                        "I013", guardInstance, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter guardAdapter,
                out _,
                out _);
            C1FormalRealtimeBattleSessionStateSnapshot guardState = null;
            bool guardApplied = guardStarted
                && guardAdapter.TryAdvanceTo(4500L, out _)
                && guardAdapter.TryGetSnapshot(out guardState)
                && guardState.playerSnapshot.guard > 0
                && guardAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == guardInstance
                    && row.operationId == "AddFlat"
                    && row.targetStatId == "guard"
                    && row.appliedAmount > 0)
                && guardAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted
                    && cue.sourceItemInstanceId == guardInstance);
            long guardBeforeHit = guardApplied ? guardState.playerSnapshot.guard : 0;
            C1FormalRealtimeBattleTickResult guardHit = null;
            bool guardConsumed = guardApplied
                && guardAdapter.TryAdvanceTo(9000L, out guardHit)
                && guardHit.stateSnapshot.playerSnapshot.guard < guardBeforeHit;
            context.Check(
                "nonstarter-combat-fact-mutates-consumed-live-guard",
                "combat-fact",
                guardApplied && guardConsumed,
                "placed+lit I013 reacts to a real enemy wave and its guard is consumed by the next wave",
                "applied=" + guardApplied + ";guard=" + guardBeforeHit + "->"
                + (guardHit?.stateSnapshot == null
                    ? -1
                    : guardHit.stateSnapshot.playerSnapshot.guard)
                + ";cues=" + string.Join(",", guardAdapter.Session.CueLedger
                    .Where(cue => cue.sourceItemInstanceId == guardInstance)
                    .Select(cue => cue.cueKind + ":" + cue.floatPayload)),
                "COMBAT_FACT_LIVE_GUARD_FAILED");
            guardAdapter.Reset();

            bool trayStarted = TryStartCumulativeItemSnapshot(
                "rev11-combat-fact-tray",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                11105L,
                new[]
                {
                    CreatePool15BattleItemRow(
                        "I013", "verifier.rev11.i013.tray", false, null)
                },
                out C1FormalRealtimeBattleSessionAdapter trayAdapter,
                out _,
                out _);
            bool trayInactive = trayStarted
                && trayAdapter.TryAdvanceBy(1L, out var trayTick)
                && trayTick.stateSnapshot.playerSnapshot.guard == 0
                && trayAdapter.Session.Pool15ApplicationLedger.Count == 0
                && !trayAdapter.Session.CueLedger.Any(cue =>
                    cue.sourceItemInstanceId == "verifier.rev11.i013.tray");
            context.Check(
                "tray-combat-fact-produces-no-delta-or-cue",
                "combat-fact",
                trayInactive,
                "no live delta and no Item-origin cue",
                trayInactive ? "PASS" : "FAIL",
                "COMBAT_FACT_TRAY_LEAK");
            trayAdapter.Reset();

            const string oldGuard = "verifier.rev11.refresh.old.i013";
            const string newGuard = "verifier.rev11.refresh.new.i013";
            string refreshSession = "verifier.rev11.refresh.item-session";
            C1FormalItemBattleInputSnapshot beforeRefresh =
                CreateStageTwoItemSnapshot(
                    "rev11-refresh-before",
                    refreshSession,
                    new[]
                    {
                        CreatePool15BattleItemRow("I013", oldGuard, true, true)
                    });
            C1FormalRealtimeBattleSessionAdapter refreshAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool refreshStarted = refreshAdapter
                .TryStartFromCumulativeItemSnapshot(
                    CreateContext(
                        "rev11-refresh",
                        C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                        11106L),
                    beforeRefresh,
                    refreshSession,
                    "verifier.rev11.refresh.session",
                    "verifier.rev11.refresh.token",
                    out _)
                && refreshAdapter.TryAdvanceBy(1L, out _)
                && refreshAdapter.TryPause(out _);
            int oldApplicationCount = refreshAdapter.Session
                .Pool15ApplicationLedger.Count(row =>
                    row.sourceItemInstanceId == oldGuard);
            C1FormalItemBattleInputSnapshot afterRefresh =
                CreateStageTwoItemSnapshot(
                    "rev11-refresh-after",
                    refreshSession,
                    new[]
                    {
                        CreatePool15BattleItemRow("I013", oldGuard, false, null),
                        CreatePool15BattleItemRow("I013", newGuard, true, true)
                    });
            bool refreshAccepted = refreshStarted
                && refreshAdapter.TryRefreshFromCumulativeItemSnapshotWhilePaused(
                    afterRefresh,
                    refreshSession,
                    "verifier.rev11.refresh.command",
                    out var refreshResult)
                && refreshResult.accepted
                && refreshAdapter.TryResume(out _)
                && refreshAdapter.TryAdvanceTo(4500L, out _);
            bool refreshSafe = refreshAccepted
                && refreshAdapter.Session.Pool15ApplicationLedger.Count(row =>
                    row.sourceItemInstanceId == oldGuard) == oldApplicationCount
                && refreshAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == newGuard);
            context.Check(
                "paused-refresh-replaces-only-future-item-work",
                "lifecycle",
                refreshSafe,
                "old source history retained once; new source applies; no old burst",
                "accepted=" + refreshAccepted + ";old=" + oldApplicationCount
                + "->" + refreshAdapter.Session.Pool15ApplicationLedger.Count(
                    row => row.sourceItemInstanceId == oldGuard)
                + ";new=" + refreshAdapter.Session.Pool15ApplicationLedger.Count(
                    row => row.sourceItemInstanceId == newGuard)
                + ";cues=" + string.Join(",", refreshAdapter.Session.CueLedger
                    .Where(cue => cue.sourceItemInstanceId == newGuard)
                    .Select(cue => cue.cueKind + ":" + cue.floatPayload)),
                "FORMAL_NIAN_REFRESH_FAILED");
            refreshAdapter.Reset();

        }

        private static void VerifyFormalShellAndI003(
            VerificationContext context)
        {
            var shellStages = new[]
            {
                new
                {
                    Stage = C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                    Count = 2
                },
                new
                {
                    Stage = C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    Count = 1
                },
                new
                {
                    Stage = C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                    Count = 1
                }
            };
            long generation = 11120L;
            bool shellRoster = true;
            foreach (var row in shellStages)
            {
                bool started = TryStartCumulativeItemSnapshot(
                    "rev11-shell-roster-" + row.Stage,
                    row.Stage,
                    generation++,
                    Array.Empty<C1FormalItemBattleItemRow>(),
                    out C1FormalRealtimeBattleSessionAdapter adapter,
                    out _,
                    out C1FormalRealtimeBattleSessionStartSnapshot start);
                C1FormalRealtimeBattleActorSnapshot[] shellActors = start
                    ?.stateSnapshot?.actorSnapshots
                    .Where(actor => actor.hasShell)
                    .ToArray() ?? Array.Empty<
                    C1FormalRealtimeBattleActorSnapshot>();
                shellRoster = shellRoster
                    && started
                    && shellActors.Length == row.Count
                    && shellActors.All(actor => actor.maxShell == 100
                        && actor.currentShell == 100
                        && !actor.shellBroken
                        && string.IsNullOrEmpty(actor.shellBrokenStateId)
                        && string.IsNullOrEmpty(
                            actor.shellBreakCounterWindowId));
                adapter.Reset();
            }
            context.Check(
                "formal-shell-roster-from-enemy-defense-fact",
                "shell",
                shellRoster,
                "1-3=2, 1-4=1, 1-5=1 exact 100/100 shell actors",
                shellRoster ? "PASS" : "FAIL",
                "FORMAL_SHELL_ROSTER_FAILED");

            C1FormalItemBattleItemRow i001 = CreateBattleItemRow(
                "I001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                InitialVerifierInstanceSignature,
                true,
                true);
            bool baselineStarted = TryStartCumulativeItemSnapshot(
                "rev11-shell-baseline",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                new[] { i001 },
                out C1FormalRealtimeBattleSessionAdapter baselineAdapter,
                out _,
                out _);
            bool baselineShellFirst = baselineStarted
                && baselineAdapter.TryAdvanceTo(2000L, out var baselineTick)
                && baselineTick.stateSnapshot.actorSnapshots[0].hasShell
                && baselineTick.stateSnapshot.actorSnapshots[0].currentShell
                    == 18
                && baselineTick.stateSnapshot.actorSnapshots[0].currentHp
                    == 720
                && baselineAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.ActorShellChanged
                    && cue.sourceItemInstanceId == i001.itemInstanceId
                    && cue.shellBefore == 100
                    && cue.shellAfter == 18
                    && cue.shellAppliedAmount == 82);
            context.Check(
                "formal-direct-hit-is-shell-first",
                "shell",
                baselineShellFirst,
                "first I001 hit changes shell 100->18 while Hound HP remains 720",
                baselineShellFirst ? "PASS" : "FAIL",
                "FORMAL_SHELL_FIRST_FAILED");
            baselineAdapter.Reset();

            const string i003Instance = "verifier.rev11.i003.shell";
            bool modifierStarted = TryStartCumulativeItemSnapshot(
                "rev11-i003-shell",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I003", i003Instance, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter modifierAdapter,
                out _,
                out _);
            C1FormalRealtimeBattleEffectApplicationSnapshot i003Application =
                null;
            bool modifierApplied = modifierStarted
                && modifierAdapter.TryAdvanceTo(2000L, out var modifierTick)
                && modifierTick.stateSnapshot.actorSnapshots[0].currentShell
                    == 13
                && modifierTick.stateSnapshot.actorSnapshots[0].currentHp
                    == 720
                && (i003Application = modifierAdapter.Session
                    .Pool15ApplicationLedger.SingleOrDefault(application =>
                        application.sourceItemInstanceId == i003Instance)) != null
                && i003Application.operationId == "AddPercent"
                && i003Application.targetStatId == "break"
                && i003Application.nativeUnitId == "basisPoint"
                && i003Application.requestedAmount == 650L
                && i003Application.appliedAmount == 5L
                && i003Application.percentageBaseValueUnits == 82L
                && i003Application.targetBefore == 18L
                && i003Application.targetAfter == 13L
                && modifierAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted
                    && cue.sourceItemInstanceId == i003Instance
                    && cue.shellBefore == 18
                    && cue.shellAfter == 13
                    && cue.shellAppliedAmount == 5);
            context.Check(
                "formal-i003-adds-real-shell-break",
                "shell",
                modifierApplied,
                "exact +650bp I003 adds 5 shell damage from base shell portion 82",
                i003Application == null ? "missing" : i003Application
                    .targetBefore + ">" + i003Application.targetAfter
                    + "/applied=" + i003Application.appliedAmount,
                "FORMAL_I003_SHELL_DELTA_FAILED");

            bool broken = modifierApplied
                && modifierAdapter.TryAdvanceTo(4000L, out var brokenTick)
                && brokenTick.stateSnapshot.actorSnapshots[0].currentShell == 0
                && brokenTick.stateSnapshot.actorSnapshots[0].shellBroken
                && brokenTick.stateSnapshot.actorSnapshots[0]
                    .shellBrokenStateId ==
                    C1Stage3To5PlaytestEnemyEncounterContract
                        .ShellBrokenStateId
                && brokenTick.stateSnapshot.actorSnapshots[0]
                    .shellBreakCounterWindowId ==
                    C1Stage3To5PlaytestEnemyEncounterContract
                        .ShellBreakCounterWindowId
                && modifierAdapter.Session.CueLedger.Count(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.ActorShellBroken) == 1;
            int modifierCountAtBreak = modifierAdapter.Session
                .Pool15ApplicationLedger.Count(application =>
                    application.sourceItemInstanceId == i003Instance);
            bool noRegeneration = broken
                && modifierAdapter.TryAdvanceTo(6000L, out var afterBreakTick)
                && afterBreakTick.stateSnapshot.actorSnapshots[0].currentShell
                    == 0
                && modifierAdapter.Session.CueLedger.Count(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.ActorShellBroken) == 1
                && modifierAdapter.Session.Pool15ApplicationLedger.Count(
                    application => application.sourceItemInstanceId
                        == i003Instance) == modifierCountAtBreak;
            context.Check(
                "formal-shell-break-state-once-no-regeneration",
                "shell",
                broken && noRegeneration,
                "shell reaches 0, publishes broken/counter once, and remains 0",
                "broken=" + broken + ";noRegen=" + noRegeneration,
                "FORMAL_SHELL_BREAK_LIFECYCLE_FAILED");
            modifierAdapter.Reset();

            const string i003A = "verifier.rev11.i003.multi.a";
            const string i003B = "verifier.rev11.i003.multi.b";
            bool multipleStarted = TryStartCumulativeItemSnapshot(
                "rev11-i003-multiple",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow("I003", i003A, true, true),
                    CreatePool15BattleItemRow("I003", i003B, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter multipleAdapter,
                out _,
                out _);
            C1FormalRealtimeBattleEffectApplicationSnapshot[] multiple = null;
            bool multipleDistinct = multipleStarted
                && multipleAdapter.TryAdvanceTo(2000L, out var multipleTick)
                && multipleTick.stateSnapshot.actorSnapshots[0].currentShell
                    == 7
                && (multiple = multipleAdapter.Session
                    .Pool15ApplicationLedger.Where(application =>
                        application.sourceBaseItemId == "I003")
                    .OrderBy(application => application.sourceItemInstanceId,
                        StringComparer.Ordinal)
                    .ToArray()).Length == 2
                && multiple.Select(application => application
                        .sourceItemInstanceId)
                    .SequenceEqual(new[] { i003A, i003B })
                && multiple.Sum(application => application.appliedAmount) == 11L;
            context.Check(
                "formal-multiple-i003-additive-instance-lineage",
                "shell",
                multipleDistinct,
                "two exact instances contribute stable 5+6 shell damage",
                multipleDistinct ? "PASS" : "FAIL",
                "FORMAL_I003_MULTI_SOURCE_FAILED");
            multipleAdapter.Reset();

            const string unshieldedI003 = "verifier.rev11.i003.unshielded";
            bool unshieldedStarted = TryStartCumulativeItemSnapshot(
                "rev11-i003-unshielded",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                generation++,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I003", unshieldedI003, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter unshieldedAdapter,
                out _,
                out _);
            bool unshieldedInactive = unshieldedStarted
                && unshieldedAdapter.TryAdvanceTo(2000L, out var unshieldedTick)
                && !unshieldedTick.stateSnapshot.actorSnapshots[0].hasShell
                && unshieldedTick.stateSnapshot.actorSnapshots[0].currentHp
                    == 418
                && !unshieldedAdapter.Session.Pool15ApplicationLedger.Any(
                    application => application.sourceItemInstanceId
                        == unshieldedI003)
                && !unshieldedAdapter.Session.CueLedger.Any(cue =>
                    cue.sourceItemInstanceId == unshieldedI003);
            context.Check(
                "formal-i003-unshielded-target-inactive",
                "shell",
                unshieldedInactive,
                "unshielded Host receives no I003 application or cue",
                unshieldedInactive ? "PASS" : "FAIL",
                "FORMAL_I003_UNSHIELDED_LEAK");
            unshieldedAdapter.Reset();

            const string trayI003 = "verifier.rev11.i003.tray";
            bool trayStarted = TryStartCumulativeItemSnapshot(
                "rev11-i003-tray",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow("I003", trayI003, false, null)
                },
                out C1FormalRealtimeBattleSessionAdapter trayAdapter,
                out _,
                out _);
            bool trayInactive = trayStarted
                && trayAdapter.TryAdvanceTo(2000L, out var trayTick)
                && trayTick.stateSnapshot.actorSnapshots[0].currentShell == 18
                && !trayAdapter.Session.Pool15ApplicationLedger.Any(
                    application => application.sourceItemInstanceId == trayI003)
                && !trayAdapter.Session.CueLedger.Any(cue =>
                    cue.sourceItemInstanceId == trayI003);
            context.Check(
                "formal-i003-tray-inactive",
                "shell",
                trayInactive,
                "Tray I003 produces no shell delta, application, or cue",
                trayInactive ? "PASS" : "FAIL",
                "FORMAL_I003_TRAY_LEAK");
            trayAdapter.Reset();

        }

        private static void VerifyFormalI004(VerificationContext context)
        {
            C1FormalItemBattleItemRow i001 = CreateBattleItemRow(
                "I001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                InitialVerifierInstanceSignature,
                true,
                true);
            const string i004Instance = "verifier.rev11.i004.secondary";
            bool started = TryStartCumulativeItemSnapshot(
                "rev11-i004-secondary",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11130L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I004", i004Instance, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter adapter,
                out _,
                out _);
            C1FormalRealtimeBattleEffectApplicationSnapshot application = null;
            C1FormalRealtimeBattleCue triggerCue = null;
            C1FormalRealtimeBattleCue effectCue = null;
            bool secondaryApplied = started
                && adapter.TryAdvanceTo(2000L, out var firstTick)
                && firstTick.stateSnapshot.actorSnapshots[0].currentHp == 418
                && firstTick.stateSnapshot.actorSnapshots[1].currentHp == 600
                && firstTick.stateSnapshot.actorSnapshots[1].currentShell == 18
                && firstTick.stateSnapshot.actorSnapshots[2].currentShell == 100
                && adapter.Session.CurrentTargetActorId
                    == firstTick.stateSnapshot.actorSnapshots[0].actorBalanceId
                && adapter.Session.NianTransactionLedger.Count(row =>
                    row.transactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Spend) == 1
                && (application = adapter.Session.Pool15ApplicationLedger
                    .SingleOrDefault(row => row.sourceItemInstanceId
                        == i004Instance)) != null
                && application.operationId == "ExtraTarget"
                && application.targetStatId == "targetCount"
                && application.nativeUnitId == "count"
                && application.requestedAmount == 1L
                && application.appliedAmount == 1L
                && application.targetStableOrder == 1
                && (triggerCue = adapter.Session.CueLedger.SingleOrDefault(
                    cue => cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted
                        && cue.sourceItemInstanceId == i001.itemInstanceId)) != null
                && (effectCue = adapter.Session.CueLedger.SingleOrDefault(
                    cue => cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted
                        && cue.sourceItemInstanceId == i004Instance)) != null
                && effectCue.acceptedApplicationEventId
                    == triggerCue.acceptedApplicationEventId
                && effectCue.targetStableOrder == 1
                && effectCue.shellBefore == 100
                && effectCue.shellAfter == 18
                && effectCue.shellAppliedAmount == 82
                && adapter.Session.CueLedger.Any(cue =>
                    cue.cueKind == C1FormalRealtimeBattleCueKinds.HitAccepted
                    && cue.sourceItemInstanceId == i004Instance
                    && cue.acceptedApplicationEventId
                        == triggerCue.acceptedApplicationEventId)
                && !adapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.TargetChanged
                    && cue.sourceItemInstanceId == i004Instance);
            bool onceOnly = secondaryApplied
                && adapter.TryAdvanceTo(4000L, out var secondTick)
                && secondTick.stateSnapshot.actorSnapshots[0].currentHp == 336
                && secondTick.stateSnapshot.actorSnapshots[1].currentShell == 18
                && adapter.Session.CurrentTargetActorId
                    == secondTick.stateSnapshot.actorSnapshots[0].actorBalanceId
                && adapter.Session.Pool15ApplicationLedger.Count(row =>
                    row.sourceItemInstanceId == i004Instance) == 1
                && adapter.Session.NianTransactionLedger.Count(row =>
                    row.transactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Spend) == 2;
            context.Check(
                "formal-i004-next-zhenlei-secondary-once",
                "combat-fact",
                secondaryApplied && onceOnly,
                "primary 500->418; nearest secondary shell 100->18; one Nian spend per trigger; I004 consumed once",
                "applied=" + secondaryApplied + ";once=" + onceOnly,
                "FORMAL_I004_SECONDARY_FAILED");
            adapter.Reset();

            const string trayI004 = "verifier.rev11.i004.tray";
            bool trayStarted = TryStartCumulativeItemSnapshot(
                "rev11-i004-tray",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11131L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I004", trayI004, false, null)
                },
                out C1FormalRealtimeBattleSessionAdapter trayAdapter,
                out _,
                out _);
            bool trayInactive = trayStarted
                && trayAdapter.TryAdvanceTo(2000L, out var trayTick)
                && trayTick.stateSnapshot.actorSnapshots[0].currentHp == 418
                && trayTick.stateSnapshot.actorSnapshots[1].currentShell == 100
                && !trayAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == trayI004)
                && !trayAdapter.Session.CueLedger.Any(cue =>
                    cue.sourceItemInstanceId == trayI004);
            trayAdapter.Reset();
            const string unlitI004 = "verifier.rev11.i004.unlit";
            bool unlitStarted = TryStartCumulativeItemSnapshot(
                "rev11-i004-unlit",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11134L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I004", unlitI004, true, false)
                },
                out C1FormalRealtimeBattleSessionAdapter unlitAdapter,
                out _,
                out _);
            bool unlitInactive = unlitStarted
                && unlitAdapter.TryAdvanceTo(2000L, out var unlitTick)
                && unlitTick.stateSnapshot.actorSnapshots[0].currentHp == 418
                && unlitTick.stateSnapshot.actorSnapshots[1].currentShell
                    == 100
                && !unlitAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == unlitI004)
                && !unlitAdapter.Session.CueLedger.Any(cue =>
                    cue.sourceItemInstanceId == unlitI004);
            context.Check(
                "formal-i004-tray-and-unlit-inactive",
                "combat-fact",
                trayInactive && unlitInactive,
                "Tray and placed-unlit I004 create no secondary delta or cue",
                "tray=" + trayInactive + ";unlit=" + unlitInactive,
                "FORMAL_I004_INACTIVE_LEAK");
            unlitAdapter.Reset();

            const string i004A = "verifier.rev11.i004.multi.a";
            const string i004B = "verifier.rev11.i004.multi.b";
            bool multipleStarted = TryStartCumulativeItemSnapshot(
                "rev11-i004-multiple",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11132L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow("I004", i004A, true, true),
                    CreatePool15BattleItemRow("I004", i004B, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter multipleAdapter,
                out _,
                out _);
            C1FormalRealtimeBattleEffectApplicationSnapshot[] multiple = null;
            bool multipleDistinct = multipleStarted
                && multipleAdapter.TryAdvanceTo(2000L, out var multipleTick)
                && multipleTick.stateSnapshot.actorSnapshots[0].currentHp == 418
                && multipleTick.stateSnapshot.actorSnapshots[1].currentShell
                    == 18
                && multipleTick.stateSnapshot.actorSnapshots[2].currentShell
                    == 18
                && multipleAdapter.Session.CurrentTargetActorId
                    == multipleTick.stateSnapshot.actorSnapshots[0]
                        .actorBalanceId
                && (multiple = multipleAdapter.Session.Pool15ApplicationLedger
                    .Where(row => row.sourceBaseItemId == "I004")
                    .OrderBy(row => row.sourceItemInstanceId,
                        StringComparer.Ordinal)
                    .ToArray()).Length == 2
                && multiple.Select(row => row.sourceItemInstanceId)
                    .SequenceEqual(new[] { i004A, i004B })
                && multiple.Select(row => row.targetStableOrder)
                    .SequenceEqual(new[] { 1, 2 });
            context.Check(
                "formal-i004-multiple-distinct-secondaries",
                "combat-fact",
                multipleDistinct,
                "two stable ItemInstances select stable distinct secondary actors 1 and 2",
                multipleDistinct ? "PASS" : "FAIL",
                "FORMAL_I004_MULTI_TARGET_FAILED");
            multipleAdapter.Reset();

            const string i003Instance =
                "verifier.rev11.i003.on-i004-secondary";
            const string combinedI004 =
                "verifier.rev11.i004.with-i003";
            bool combinedStarted = TryStartCumulativeItemSnapshot(
                "rev11-i004-i003-combined",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11133L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I003", i003Instance, true, true),
                    CreatePool15BattleItemRow(
                        "I004", combinedI004, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter combinedAdapter,
                out _,
                out _);
            bool i003AppliedToSecondary = combinedStarted
                && combinedAdapter.TryAdvanceTo(2000L, out var combinedTick)
                && combinedTick.stateSnapshot.actorSnapshots[0].currentHp == 418
                && combinedTick.stateSnapshot.actorSnapshots[1].currentShell
                    == 13
                && combinedAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == combinedI004
                    && row.targetStableOrder == 1)
                && combinedAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == i003Instance
                    && row.targetStableOrder == 1
                    && row.appliedAmount == 5L);
            context.Check(
                "formal-i003-applies-independently-to-i004-hit",
                "combat-fact",
                i003AppliedToSecondary,
                "I004 base shell 100->18 then exact I003 modifier 18->13 on the same secondary hit",
                i003AppliedToSecondary ? "PASS" : "FAIL",
                "FORMAL_I004_I003_COMPOSITION_FAILED");
            combinedAdapter.Reset();
        }

        private static void VerifyGroupedRemainingPool15Supported(
            VerificationContext context)
        {
            C1FormalItemBattleItemRow i001 = CreateBattleItemRow(
                "I001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                InitialVerifierInstanceSignature,
                true,
                true);
            const string i015Instance = "verifier.rev11.i015.live-guard";
            bool guardStarted = TryStartCumulativeItemSnapshot(
                "rev11-grouped-i015",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11140L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I015", i015Instance, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter guardAdapter,
                out _,
                out _);
            bool guardApplied = guardStarted
                && guardAdapter.TryAdvanceTo(2000L, out var guardTick)
                && guardTick.stateSnapshot.playerSnapshot.guard == 8L
                && guardAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == i015Instance
                    && row.operationId == "AddFlat"
                    && row.targetStatId == "guard"
                    && row.nativeUnitId == "flat"
                    && row.appliedAmount == 8L
                    && row.targetBefore == 0L
                    && row.targetAfter == 8L)
                && guardAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted
                    && cue.sourceItemInstanceId == i015Instance
                    && !string.IsNullOrEmpty(
                        cue.acceptedApplicationEventId));
            bool guardConsumed = guardApplied
                && guardAdapter.TryAdvanceTo(4500L, out var guardWave)
                && guardWave.stateSnapshot.playerSnapshot.guard == 1L
                && guardWave.stateSnapshot.playerSnapshot.currentHp ==
                    C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp;
            context.Check(
                "grouped-guard-sustain-i015-live-and-consumed",
                "GUARD_SUSTAIN",
                guardApplied && guardConsumed,
                "exact I015 raises live guard 0->8 and the enemy wave consumes it 8->1",
                "applied=" + guardApplied + ";consumed=" + guardConsumed,
                "GROUPED_I015_LIVE_GUARD_FAILED");
            guardAdapter.Reset();

            const string i025Instance =
                "verifier.rev11.i025.enemy-cast-delay";
            bool delayStarted = TryStartCumulativeItemSnapshot(
                "rev11-grouped-i025",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11141L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I025", i025Instance, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter delayAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot delayStart);
            C1FormalRealtimeBattleScheduledActionSnapshot initialWave =
                delayStart?.stateSnapshot?.scheduledActions.SingleOrDefault(
                    action => action.actionKind ==
                        C1FormalRealtimeBattleScheduledActionKinds
                            .EnemyBasicWave
                        && !action.executed);
            C1FormalRealtimeBattleActionProgressMutationSnapshot mutation =
                null;
            C1FormalRealtimeBattleEffectApplicationSnapshot delayApplication =
                null;
            bool delayed = delayStarted
                && initialWave != null
                && initialWave.dueBattleTimeMs == 4500L
                && delayAdapter.TryAdvanceTo(2000L, out var delayTick)
                && delayTick.stateSnapshot.scheduledActions.Any(action =>
                    action.actionId == initialWave.actionId
                    && !action.executed
                    && action.dueBattleTimeMs == 9000L)
                && (mutation = delayAdapter.Session
                    .ActionProgressMutationLedger.SingleOrDefault(row =>
                        row.sourceItemInstanceId == i025Instance)) != null
                && mutation.operatorId ==
                    C1FormalRealtimeBattleActionProgressOperatorIds
                        .DelayEnemyCastProgressStep
                && mutation.beforeDueBattleTimeMs == 4500L
                && mutation.afterDueBattleTimeMs == 9000L
                && mutation.sourceNativeIntervalMs == 4500L
                && mutation.requestedStepCount == 1
                && mutation.appliedStepCount == 1
                && (delayApplication = delayAdapter.Session
                    .Pool15ApplicationLedger.SingleOrDefault(row =>
                        row.sourceItemInstanceId == i025Instance)) != null
                && delayApplication.operationId == "ReduceFlat"
                && delayApplication.targetStatId == "castProgress"
                && delayApplication.nativeUnitId == "turn"
                && delayApplication.targetBefore == 4500L
                && delayApplication.targetAfter == 9000L
                && delayAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.ActionProgressMutated
                    && cue.sourceItemInstanceId == i025Instance
                    && !string.IsNullOrEmpty(
                        cue.acceptedApplicationEventId))
                && delayAdapter.Session.NianTransactionLedger.Count(row =>
                    row.transactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Spend) == 1;
            context.Check(
                "grouped-enemy-control-i025-delays-real-wave",
                "ENEMY_CONTROL",
                delayed,
                "exact I025 delays the authoritative future enemy wave 4500->9000 by one native interval",
                mutation == null
                    ? "missing"
                    : mutation.beforeDueBattleTimeMs + "->"
                      + mutation.afterDueBattleTimeMs,
                "GROUPED_I025_LIVE_DELAY_FAILED");
            delayAdapter.Reset();

            const string trayI015 = "verifier.rev11.i015.tray";
            const string unlitI025 = "verifier.rev11.i025.unlit";
            bool inactiveStarted = TryStartCumulativeItemSnapshot(
                "rev11-grouped-inactive",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                11142L,
                new[]
                {
                    i001,
                    CreatePool15BattleItemRow(
                        "I015", trayI015, false, null),
                    CreatePool15BattleItemRow(
                        "I025", unlitI025, true, false)
                },
                out C1FormalRealtimeBattleSessionAdapter inactiveAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot inactiveStart);
            C1FormalRealtimeBattleScheduledActionSnapshot inactiveWave =
                inactiveStart?.stateSnapshot?.scheduledActions
                    .SingleOrDefault(action => action.actionKind ==
                        C1FormalRealtimeBattleScheduledActionKinds
                            .EnemyBasicWave
                        && !action.executed);
            bool inactiveExcluded = inactiveStarted
                && inactiveWave != null
                && inactiveAdapter.TryAdvanceTo(2000L, out var inactiveTick)
                && inactiveTick.stateSnapshot.playerSnapshot.guard == 0L
                && inactiveTick.stateSnapshot.scheduledActions.Any(action =>
                    action.actionId == inactiveWave.actionId
                    && action.dueBattleTimeMs == 4500L)
                && !inactiveAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    row.sourceItemInstanceId == trayI015
                    || row.sourceItemInstanceId == unlitI025)
                && !inactiveAdapter.Session.CueLedger.Any(cue =>
                    cue.sourceItemInstanceId == trayI015
                    || cue.sourceItemInstanceId == unlitI025);
            context.Check(
                "grouped-supported-tray-unlit-excluded",
                "active-filter",
                inactiveExcluded,
                "Tray I015 and placed-unlit I025 produce no state, schedule, or cue",
                inactiveExcluded ? "PASS" : "FAIL",
                "GROUPED_SUPPORTED_INACTIVE_LEAK");
            inactiveAdapter.Reset();
        }

        private static void VerifyCurrentAdapterLiveRegression(
            VerificationContext context)
        {
            var rows = new[]
            {
                new
                {
                    Name = "1-1",
                    Stage = C1Lv1EarlyEncounterBalanceContract.Stage1_1,
                    Items = new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true)
                    },
                    Duration = 16000L,
                    EnemyApplications = 3,
                    PlayerHp = 76,
                    ItemApplications = 8
                },
                new
                {
                    Name = "1-2-weak",
                    Stage = C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                    Items = new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true)
                    },
                    Duration = 48000L,
                    EnemyApplications = 10,
                    PlayerHp = 20,
                    ItemApplications = 24
                },
                new
                {
                    Name = "1-2-target",
                    Stage = C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                    Items = new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true),
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            true,
                            true)
                    },
                    Duration = 36000L,
                    EnemyApplications = 7,
                    PlayerHp = 44,
                    ItemApplications = 35
                }
            };
            long generation = 11200L;
            foreach (var row in rows)
            {
                bool started = TryStartCumulativeItemSnapshot(
                    "rev11-parity-" + row.Name,
                    row.Stage,
                    generation++,
                    row.Items,
                    out C1FormalRealtimeBattleSessionAdapter adapter,
                    out _,
                    out C1FormalRealtimeBattleSessionStartSnapshot start);
                int guard = 0;
                while (started && !adapter.IsTerminal && guard++ < 240)
                {
                    if (!adapter.TryAdvanceBy(500L, out _)) break;
                }

                C1FormalRealtimeBattleTerminalResult terminal =
                    adapter.Session.TerminalResult;
                bool parity = terminal != null
                    && terminal.accepted
                    && terminal.win
                    && terminal.durationMs == row.Duration
                    && terminal.enemyApplicationCount == row.EnemyApplications
                    && terminal.playerSnapshot.currentHp == row.PlayerHp
                    && terminal.acceptedItemApplicationCount
                        == row.ItemApplications;
                context.Check(
                    "rev11-current-live-regression-" + row.Name,
                    "live-session",
                    parity,
                    row.Duration + "/" + row.EnemyApplications + "/"
                    + row.PlayerHp + "/" + row.ItemApplications,
                    terminal == null ? "started=" + started + ";error="
                    + (start == null ? "missing" : start.errorCode)
                    : terminal.durationMs + "/"
                    + terminal.enemyApplicationCount + "/"
                    + terminal.playerSnapshot.currentHp + "/"
                    + terminal.acceptedItemApplicationCount,
                    "REV11_LIVE_REGRESSION_FAILED");
                adapter.Reset();
            }
        }

        private static void VerifyCurrentStageAndBoneSwapRegression(
            VerificationContext context)
        {
            long generation = 11300L;
            foreach (string stageId in new[]
                     {
                         C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                         C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                         C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5
                     })
            {
                bool started = TryStartCumulativeItemSnapshot(
                    "rev11-stage-" + stageId,
                    stageId,
                    generation++,
                    Array.Empty<C1FormalItemBattleItemRow>(),
                    out C1FormalRealtimeBattleSessionAdapter adapter,
                    out _,
                    out C1FormalRealtimeBattleSessionStartSnapshot start);
                C1Stage3To5PlaytestEncounterProfile source =
                    C1Stage3To5PlaytestEnemyEncounterCatalog.FindByStageId(
                        stageId);
                bool wave = started
                    && adapter.TryAdvanceTo(4500L, out var tick)
                    && source != null
                    && tick.stateSnapshot.enemyApplicationCount
                        == source.Actors.Count
                    && tick.stateSnapshot.playerSnapshot.currentHp == 93;
                bool exactActors = start?.stateSnapshot != null
                    && source != null
                    && start.stateSnapshot.actorSnapshots.Count
                        == source.Actors.Count
                    && start.stateSnapshot.actorSnapshots.Sum(actor =>
                        actor.maxHp) == source.TotalMaxHp;
                context.Check(
                    "rev11-stage-regression-" + stageId,
                    "stage-runtime",
                    wave && exactActors,
                    "exact actors each own the first 4500ms action; total damage=7",
                    "start=" + started + ";wave=" + wave + ";actors="
                    + exactActors,
                    "REV11_STAGE_REGRESSION_FAILED");
                adapter.Reset();
            }

            bool boneStarted = TryStartCumulativeItemSnapshot(
                "rev11-bone-swap",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation,
                new[]
                {
                    CreateBattleItemRow(
                        "I001",
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                        InitialVerifierInstanceSignature,
                        true,
                        true)
                },
                out C1FormalRealtimeBattleSessionAdapter boneAdapter,
                out _,
                out _);
            bool boneAdvanced = boneStarted;
            int boneGuard = 0;
            while (boneAdvanced
                   && !boneAdapter.IsTerminal
                   && boneGuard++ < 120
                   && !boneAdapter.Session.CueLedger.Any(cue =>
                       cue.cueKind == C1FormalRealtimeBattleCueKinds
                           .BoneSwapReturnAccepted))
            {
                boneAdvanced = boneAdapter.TryAdvanceBy(500L, out _);
            }
            bool boneSwap = boneAdvanced
                && boneAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.BoneSwapTelegraphed)
                && boneAdapter.Session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.BoneSwapReturnAccepted);
            context.Check(
                "rev11-bone-swap-regression",
                "bone-swap",
                boneSwap,
                "released telegraph and return remain live",
                boneSwap ? "PASS" : "started=" + boneStarted + ";cues="
                    + string.Join(",", boneAdapter.Session.CueLedger.Select(
                        cue => cue.cueKind + "@" + cue.battleTimeMs)),
                "REV11_BONE_SWAP_REGRESSION_FAILED");
            boneAdapter.Reset();
        }

#endif

        private static void VerifyPerActorEnemyActions(
            VerificationContext context)
        {
            const string StageId = "1-3";
            bool started = TryStartCumulativeItemSnapshot(
                "per-actor-enemy-actions",
                StageId,
                11901L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter adapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            IReadOnlyList<C1FormalRealtimeBattleActorSnapshot> actors =
                start?.stateSnapshot?.actorSnapshots
                ?? Array.Empty<C1FormalRealtimeBattleActorSnapshot>();
            string[] actorIds = actors
                .OrderBy(actor => actor.stableActorOrder)
                .Select(actor => actor.actorBalanceId)
                .ToArray();
            C1FormalRealtimeBattleScheduledActionSnapshot[] scheduled =
                (start?.stateSnapshot?.scheduledActions
                    ?? Array.Empty<
                        C1FormalRealtimeBattleScheduledActionSnapshot>())
                .Where(action => !action.executed
                    && string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds
                            .EnemyBasicWave,
                        StringComparison.Ordinal))
                .OrderBy(action => action.stableOrder)
                .ToArray();
            string hostActorId = actors.SingleOrDefault(actor =>
                string.Equals(
                    actor.contentId,
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                    StringComparison.Ordinal))?.actorBalanceId
                ?? string.Empty;
            string houndActorId = actors.SingleOrDefault(actor =>
                string.Equals(
                    actor.contentId,
                    C1EnemyRuntimeContract.PorcelainHoundContentId,
                    StringComparison.Ordinal))?.actorBalanceId
                ?? string.Empty;
            bool independentSchedule = started
                && actorIds.Length == 2
                && scheduled.Length == actorIds.Length
                && scheduled.Select(action => action.sourceId)
                    .SequenceEqual(actorIds)
                && scheduled.Any(action => string.Equals(
                        action.sourceId,
                        hostActorId,
                        StringComparison.Ordinal)
                    && action.dueBattleTimeMs == 2400L
                    && action.requestedDamage == 2)
                && scheduled.Any(action => string.Equals(
                        action.sourceId,
                        houndActorId,
                        StringComparison.Ordinal)
                    && action.dueBattleTimeMs == 3200L
                    && action.requestedDamage == 5);
            context.Check(
                "per-actor-enemy-independent-schedule",
                "enemy-action",
                independentSchedule,
                "one exact pending action per live actor from its Enemy definition",
                "actors=" + string.Join(",", actorIds)
                + ";actions=" + string.Join(",", scheduled.Select(action =>
                    action.sourceId + ":" + action.requestedDamage + "@"
                    + action.dueBattleTimeMs)),
                "PER_ACTOR_ENEMY_SCHEDULE_INVALID");

            C1FormalRealtimeBattleTickResult firstTick = null;
            bool firstResolved = independentSchedule
                && adapter.TryAdvanceTo(
                    3200L,
                    out firstTick);
            C1FormalRealtimeBattleCue[] accepted = firstResolved
                ? firstTick.emittedCues.Where(cue => string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                        StringComparison.Ordinal))
                    .OrderBy(cue => cue.sequence)
                    .ToArray()
                : Array.Empty<C1FormalRealtimeBattleCue>();
            bool exactCommittedSources = firstResolved
                && firstTick.stateSnapshot.playerSnapshot.currentHp == 93
                && firstTick.stateSnapshot.enemyApplicationCount == 2
                && accepted.Length == 2
                && accepted.Select(cue => cue.sourceId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(actorIds.OrderBy(
                        value => value,
                        StringComparer.Ordinal))
                && accepted.All(cue => string.Equals(
                    cue.targetActorId,
                    C1FormalRealtimeBattleSessionContract.PlayerActorId,
                    StringComparison.Ordinal))
                && accepted.Sum(cue => cue.requestedDamage) == 7
                && accepted.Sum(cue => cue.appliedDamage) == 7
                && accepted.Select(cue => cue.acceptedApplicationEventId)
                    .Distinct(StringComparer.Ordinal).Count() == 2;
            context.Check(
                "per-actor-enemy-exact-commit-cues",
                "enemy-action",
                exactCommittedSources,
                "two exact source actors commit their own 2 and 5 damage",
                "hp=" + (firstResolved
                    ? firstTick.stateSnapshot.playerSnapshot.currentHp
                        .ToString(CultureInfo.InvariantCulture)
                    : "missing") + ";sources=" + string.Join(",",
                    accepted.Select(cue => cue.sourceId + ":"
                        + cue.appliedDamage)),
                "PER_ACTOR_ENEMY_COMMIT_CUE_INVALID");
            adapter.Reset();

            bool cancelStarted = TryStartCumulativeItemSnapshot(
                "per-actor-enemy-death-cancel",
                StageId,
                11902L,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter cancelAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot cancelStart);
            C1FormalRealtimeBattleSession cancelSession =
                cancelAdapter.Session;
            string defeatedActorId = cancelStart?.stateSnapshot?.actorSnapshots
                .OrderBy(actor => actor.stableActorOrder)
                .Select(actor => actor.actorBalanceId)
                .FirstOrDefault() ?? string.Empty;
            FieldInfo actorsField = typeof(C1FormalRealtimeBattleSession)
                .GetField("actors", BindingFlags.Instance | BindingFlags.NonPublic);
            object defeatedActor = ((IEnumerable)actorsField?.GetValue(
                    cancelSession))?.Cast<object>().FirstOrDefault();
            Type defeatedActorType = defeatedActor?.GetType();
            PropertyInfo currentHpProperty = defeatedActorType?.GetProperty(
                "CurrentHp",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo cancelMethod = typeof(C1FormalRealtimeBattleSession)
                .GetMethod(
                    "CancelEnemyActionsForDefeatedActor",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            bool boundaryAvailable = cancelStarted
                && defeatedActor != null
                && currentHpProperty != null
                && cancelMethod != null;
            if (boundaryAvailable)
            {
                currentHpProperty.SetValue(defeatedActor, 0);
                cancelMethod.Invoke(
                    cancelSession,
                    new[]
                    {
                        defeatedActor,
                        "verifier.per-actor-enemy-defeat"
                    });
            }

            C1FormalRealtimeBattleSessionStateSnapshot cancelState = null;
            bool snapshotRead = boundaryAvailable
                && cancelAdapter.TryGetSnapshot(out cancelState);
            bool ownPendingCancelled = snapshotRead
                && cancelState.scheduledActions.Any(action =>
                    string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds
                            .EnemyBasicWave,
                        StringComparison.Ordinal)
                    && string.Equals(
                        action.sourceId,
                        defeatedActorId,
                        StringComparison.Ordinal)
                    && action.executed
                    && !action.accepted
                    && string.Equals(
                        action.diagnosticCode,
                        C1FormalRealtimeBattleCueKinds.ActorDefeated,
                        StringComparison.Ordinal));
            int followupGuard = 0;
            while (cancelStarted
                   && !cancelSession.IsTerminal
                   && cancelSession.BattleTimeMs < 9000L
                   && followupGuard++ < 24)
            {
                cancelStarted = cancelSession.TryAdvanceBy(500L, out _);
            }
            C1FormalRealtimeBattleCue[] attacksAfterDefeat =
                cancelSession.CueLedger.Where(cue =>
                        cue.battleTimeMs > 0L
                        && string.Equals(
                            cue.cueKind,
                            C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                            StringComparison.Ordinal))
                    .ToArray();
            bool deathCancellation = ownPendingCancelled
                && attacksAfterDefeat.All(cue => !string.Equals(
                    cue.sourceId,
                    defeatedActorId,
                    StringComparison.Ordinal))
                && attacksAfterDefeat.Any(cue => !string.Equals(
                    cue.sourceId,
                    defeatedActorId,
                    StringComparison.Ordinal));
            context.Check(
                "per-actor-enemy-death-cancels-only-own-action",
                "enemy-action",
                deathCancellation,
                "defeated source stops; another living source continues",
                "defeated=" + defeatedActorId + "@focused-boundary"
                + ";cancelled=" + ownPendingCancelled + ";after="
                + string.Join(",", attacksAfterDefeat.Select(cue =>
                    cue.sourceId + "@" + cue.battleTimeMs))
                + ";battleTime=" + cancelSession.BattleTimeMs
                + ";startError=" + (cancelStart == null
                    ? "missing"
                    : cancelStart.errorCode + ":" + cancelStart.error.message)
                + ";terminal=" + (cancelSession.TerminalResult == null
                    ? "none"
                    : (cancelSession.TerminalResult.win
                        ? "win"
                        : "lose") + "@"
                      + cancelSession.TerminalResult.durationMs)
                + ";actors=" + (cancelState == null
                    ? "missing"
                    : string.Join(",", cancelState.actorSnapshots.Select(
                        actor => actor.actorBalanceId + ":" + actor.currentHp)))
                + ";cues=" + string.Join(",",
                    cancelSession.CueLedger.Select(cue =>
                        cue.cueKind).Distinct(StringComparer.Ordinal)),
                "PER_ACTOR_ENEMY_DEATH_CANCEL_INVALID");
            cancelAdapter.Reset();
        }

#if false // Retired Pool15 / compatibility QA; remove after current proof compiles.
        public static int RunOptionalI002FocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyContractShape(context);
            VerifyPausedLiveItemRefresh(context);
            VerifyActiveItemAdmissionAndRealDefeat(context);
            VerifyPool15FormalIntegration(context);
            ScenarioTrace stageOne = RunScenario(
                "1-1",
                false,
                301L,
                context);
            VerifyTrace(context, stageOne, 16000L, 3, 76, 8, 2);
            ScenarioTrace weak = RunScenario(
                "1-2-weak",
                false,
                302L,
                context);
            VerifyTrace(context, weak, 48000L, 10, 20, 24, 3);
            ScenarioTrace target = RunScenario(
                "1-2-target",
                true,
                303L,
                context);
            VerifyTrace(context, target, 36000L, 7, 44, 35, 3);
            VerifyStage1To5FormalRealtimeExtension(context);
            VerifyLifecycleAndFailClosed(context);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "OPTIONAL_I002_REALTIME_TEST_FAILED " + failures);
            }

            return context.Checks.Count;
        }

        public static int RunCumulativeItemInputFocusedOrThrow()
        {
            VerificationContext context = new VerificationContext();
            VerifyCumulativeItemInput(context);
            ScenarioTrace stageOne = RunScenario(
                "1-1",
                false,
                8701L,
                context);
            VerifyTrace(context, stageOne, 16000L, 3, 76, 8, 2);
            ScenarioTrace weak = RunScenario(
                "1-2-weak",
                false,
                8702L,
                context);
            VerifyTrace(context, weak, 48000L, 10, 20, 24, 3);
            ScenarioTrace target = RunScenario(
                "1-2-target",
                true,
                8703L,
                context);
            VerifyTrace(context, target, 36000L, 7, 44, 35, 3);
            if (context.FailureCount != 0)
            {
                string failures = string.Join(
                    "; ",
                    context.Checks.Where(row => !row.Passed).Select(row =>
                        row.Name + "=" + row.Actual));
                throw new InvalidOperationException(
                    "CUMULATIVE_ITEM_INPUT_TEST_FAILED " + failures);
            }

            return context.Checks.Count;
        }

        private static void RunOrThrow()
        {
            VerificationContext context = RunVerification();
            if (context.FailureCount != 0)
            {
                throw new InvalidOperationException(
                    PackageName + " failed with " +
                    context.FailureCount.ToString(CultureInfo.InvariantCulture) +
                    " verification error(s).");
            }
        }

        private static VerificationContext RunVerification()
        {
            VerificationContext context = new VerificationContext();
            try
            {
                VerifyContractShape(context);
                VerifyProductionAdapterStageOne(context);
                VerifyOptionalStageTwoRuntimeFactBoundary(context);
                VerifyActiveItemAdmissionAndRealDefeat(context);
                VerifyLiveSessionRegressionScenarios(context);
                VerifyPool15FormalIntegration(context);
                VerifyItemInstancePresentationCueLineage(context);
                VerifyAutomaticGrowingPool15Runtime(context);
                VerifyStage1To5FormalRealtimeExtension(context);
                VerifyLifecycleAndFailClosed(context);
            }
            catch (Exception exception)
            {
                context.Check(
                    "verifier-unhandled-exception",
                    "verifier",
                    false,
                    "no exception",
                    exception.GetType().Name + ": " + exception.Message,
                    "VERIFIER_EXCEPTION");
            }

            if (context.FailureCount == 0)
            {
                Log(TerminalMarker);
            }
            else
            {
                LogError(
                    "C1_STAGE1_TO5_FORMAL_REALTIME_SESSION_FAIL errors=" +
                    context.FailureCount.ToString(CultureInfo.InvariantCulture));
            }

            return context;
        }

        private static void Log(object message)
        {
#if C1_FORMAL_REALTIME_BATTLE_SESSION_SCOPED_STANDALONE
            Console.WriteLine(message);
#else
            Debug.Log(message);
#endif
        }

        private static void LogError(object message)
        {
#if C1_FORMAL_REALTIME_BATTLE_SESSION_SCOPED_STANDALONE
            Console.Error.WriteLine(message);
#else
            Debug.LogError(message);
#endif
        }

        private static void VerifyContractShape(VerificationContext context)
        {
            Type[] requiredTypes =
            {
                typeof(C1FormalRealtimeBattleSessionRequest),
                typeof(C1FormalRealtimeBattleSessionStartSnapshot),
                typeof(C1FormalRealtimeBattleSessionStateSnapshot),
                typeof(C1FormalRealtimeBattleActorSnapshot),
                typeof(C1FormalRealtimeBattlePlayerSnapshot),
                typeof(C1FormalRealtimeBattleScheduledActionSnapshot),
                typeof(C1FormalRealtimeBattleCue),
                typeof(C1FormalRealtimeBattleTickResult),
                typeof(C1FormalRealtimeBattleTerminalResult),
                typeof(C1FormalRealtimeBattleItemRefreshRequest),
                typeof(C1FormalRealtimeBattleItemRefreshResult),
                typeof(C1FormalRealtimeBattlePool15ItemRow),
                typeof(C1FormalRealtimeBattlePool15ItemSnapshot),
                typeof(C1FormalRealtimeBattlePool15ActiveItemFactSnapshot),
                typeof(C1FormalRealtimeBattlePool15EventRequest),
                typeof(C1FormalRealtimeBattlePool15EventResult),
                typeof(C1FormalRealtimeBattleEffectStateValueSnapshot),
                typeof(C1FormalRealtimeBattleEffectStateSnapshot),
                typeof(C1FormalRealtimeBattleEffectApplicationSnapshot),
                typeof(C1FormalRealtimeBattleEffectCue),
                typeof(C1FormalRealtimeBattleError)
            };
            foreach (Type type in requiredTypes)
            {
                context.Check(
                    "immutable-contract-" + type.Name,
                    "contract",
                    type.IsSealed && type.GetFields(
                        BindingFlags.Instance | BindingFlags.Public).Length == 0,
                    "sealed with no public mutable fields",
                    type.IsSealed ? "sealed" : "not sealed",
                    "MUTABLE_CONTRACT");
            }

            foreach (string cueKind in C1FormalRealtimeBattleCueKinds.All)
            {
                context.RequiredCueKinds.Add(cueKind);
            }

            context.Check(
                "bounded-cue-ledger",
                "contract",
                C1FormalRealtimeBattleSessionContract.MaxCueLedgerEntries > 0
                && C1FormalRealtimeBattleSessionContract.MaxCueLedgerEntries <= 4096,
                "1..4096",
                C1FormalRealtimeBattleSessionContract.MaxCueLedgerEntries.ToString(
                    CultureInfo.InvariantCulture),
                "UNBOUNDED_CUE_LEDGER");
            string[] forbiddenEventInjectionProperties =
            {
                "sourceBaseItemId",
                "targetIdentity",
                "targetScopeId",
                "percentageBaseValueUnits",
                "requestedAmount",
                "appliedAmount",
                "cueIdentity"
            };
            PropertyInfo[] eventProperties = typeof(
                    C1FormalRealtimeBattlePool15EventRequest)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);
            bool noInjectedExecutionTruth = forbiddenEventInjectionProperties.All(
                name => eventProperties.All(property => !string.Equals(
                    property.Name,
                    name,
                    StringComparison.Ordinal)));
            context.Check(
                "pool15-event-envelope-session-owned-truth",
                "contract",
                noInjectedExecutionTruth,
                "no public source/target/base/delta/cue injection",
                noInjectedExecutionTruth ? "PASS" : "FAIL",
                "POOL15_EVENT_TRUTH_INJECTABLE");
        }

        private static void VerifyPausedLiveItemRefresh(
            VerificationContext context)
        {
            ScenarioFixture removedFixture = CreateFixture(
                "refresh-remove",
                "1-1",
                false,
                201L,
                0L);
            C1FormalRealtimeBattleSession removedSession =
                removedFixture.Session;
            bool started = removedSession.TryStart(
                removedFixture.Request,
                out _);
            bool advanced = started && removedSession.TryAdvanceBy(1000L, out _);
            bool paused = advanced && removedSession.TryPause(out _);
            removedSession.TryGetSnapshot(out C1FormalRealtimeBattleSessionStateSnapshot
                beforeRemove);
            long enemyDueBefore = NextDue(
                beforeRemove,
                C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave,
                null);
            int enemyHpBefore = beforeRemove.actorSnapshots.Sum(row => row.currentHp);
            C1FormalRealtimeBattleItemRefreshRequest removeRequest =
                CreateRefreshRequest(
                    removedFixture,
                    beforeRemove,
                    "refresh-remove-command",
                    "item-input-none",
                    Array.Empty<C1FormalRealtimeBattleItemFactSnapshot>());
            bool removed = removedSession.TryRefreshItemsWhilePaused(
                removeRequest,
                out C1FormalRealtimeBattleItemRefreshResult removeResult);
            bool preserved = removed
                && removeResult != null
                && removeResult.accepted
                && removeResult.activeItemFactCount == 0
                && removeResult.canceledFutureItemActionCount == 1
                && removeResult.retainedFutureItemActionCount == 0
                && removeResult.scheduledFutureItemActionCount == 0
                && removeResult.stateSnapshot.battleTimeMs ==
                    beforeRemove.battleTimeMs
                && removeResult.stateSnapshot.playerSnapshot.currentHp ==
                    beforeRemove.playerSnapshot.currentHp
                && removeResult.stateSnapshot.actorSnapshots.Sum(
                    row => row.currentHp) == enemyHpBefore
                && NextDue(
                    removeResult.stateSnapshot,
                    C1FormalRealtimeBattleScheduledActionKinds.EnemyBasicWave,
                    null) ==
                    enemyDueBefore
                && removeResult.stateSnapshot.enemyApplicationCount ==
                    beforeRemove.enemyApplicationCount;
            context.Check(
                "paused-remove-cancels-item-only",
                "rev10-refresh",
                paused && preserved,
                "zero facts; item canceled; clock/HP/enemy cadence preserved",
                removed ? "accepted" : removeResult?.errorCode ?? "missing",
                "REFRESH_REMOVE_FAILED");

            bool duplicate = removedSession.TryRefreshItemsWhilePaused(
                removeRequest,
                out C1FormalRealtimeBattleItemRefreshResult duplicateResult);
            C1FormalRealtimeBattleItemRefreshRequest conflictRequest =
                CreateRefreshRequest(
                    removedFixture,
                    beforeRemove,
                    "refresh-remove-command",
                    "item-input-conflict",
                    new[]
                    {
                        CreateItemFact(
                            "I001",
                            "I001@white",
                            true)
                    });
            bool conflict = !removedSession.TryRefreshItemsWhilePaused(
                conflictRequest,
                out C1FormalRealtimeBattleItemRefreshResult conflictResult);
            C1FormalRealtimeBattleItemRefreshRequest staleRequest =
                CreateRefreshRequest(
                    removedFixture,
                    beforeRemove,
                    "refresh-stale-command",
                    "item-input-stale",
                    Array.Empty<C1FormalRealtimeBattleItemFactSnapshot>());
            bool stale = !removedSession.TryRefreshItemsWhilePaused(
                staleRequest,
                out C1FormalRealtimeBattleItemRefreshResult staleResult);
            context.Check(
                "refresh-command-idempotence-and-stale-rejection",
                "rev10-refresh",
                duplicate
                && ReferenceEquals(removeResult, duplicateResult)
                && conflict
                && conflictResult.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.RefreshCommandConflict
                && stale
                && staleResult.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.RefreshStateStale,
                "same envelope no-op; conflict/stale reject",
                "duplicate=" + duplicate + " conflict=" + conflict
                + " stale=" + stale,
                "REFRESH_DEDUPE_FAILED");

            bool resumed = removedSession.TryResume(out _);
            bool runningRejected = !removedSession.TryRefreshItemsWhilePaused(
                CreateRefreshRequest(
                    removedFixture,
                    removeResult.stateSnapshot,
                    "refresh-running-command",
                    "item-input-running",
                    Array.Empty<C1FormalRealtimeBattleItemFactSnapshot>()),
                out C1FormalRealtimeBattleItemRefreshResult runningResult);
            bool advancedWithoutItem = removedSession.TryAdvanceBy(5000L, out _);
            removedSession.TryGetSnapshot(out C1FormalRealtimeBattleSessionStateSnapshot
                afterNoItem);
            context.Check(
                "removed-item-never-damages-and-enemy-continues",
                "rev10-refresh",
                resumed
                && runningRejected
                && runningResult.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected
                && advancedWithoutItem
                && afterNoItem.actorSnapshots.Sum(row => row.currentHp) ==
                    enemyHpBefore
                && afterNoItem.enemyApplicationCount >
                    beforeRemove.enemyApplicationCount,
                "enemy HP unchanged; enemy cadence advances",
                "enemyHp=" + afterNoItem.actorSnapshots.Sum(row => row.currentHp)
                + " enemyActions=" + afterNoItem.enemyApplicationCount,
                "REMOVED_ITEM_DAMAGE_LEAK");

            bool pausedAgain = removedSession.TryPause(out _);
            removedSession.TryGetSnapshot(out C1FormalRealtimeBattleSessionStateSnapshot
                beforeReplace);
            C1FormalRealtimeBattleItemFactSnapshot i001 = CreateItemFact(
                "I001",
                "I001@white",
                true);
            bool replaced = removedSession.TryRefreshItemsWhilePaused(
                CreateRefreshRequest(
                    removedFixture,
                    beforeReplace,
                    "refresh-replace-command",
                    "item-input-i001",
                    new[] { i001 }),
                out C1FormalRealtimeBattleItemRefreshResult replaceResult);
            long firstNewItemDue = NextDue(
                replaceResult?.stateSnapshot,
                C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                i001.sourceId);
            bool cadenceExact = pausedAgain
                && replaced
                && replaceResult.scheduledFutureItemActionCount == 1
                && firstNewItemDue == beforeReplace.battleTimeMs + i001.cooldownMs;
            bool resumedAgain = removedSession.TryResume(out _);
            int hpBeforeNewDue = replaceResult.stateSnapshot.actorSnapshots.Sum(
                row => row.currentHp);
            bool noCatchUp = removedSession.TryAdvanceTo(
                firstNewItemDue - 1L,
                out _);
            removedSession.TryGetSnapshot(out C1FormalRealtimeBattleSessionStateSnapshot
                justBeforeDue);
            bool firesAtDue = removedSession.TryAdvanceTo(firstNewItemDue, out _);
            removedSession.TryGetSnapshot(out C1FormalRealtimeBattleSessionStateSnapshot
                atDue);
            context.Check(
                "replace-schedules-from-paused-tick-without-catchup",
                "rev10-refresh",
                cadenceExact
                && resumedAgain
                && noCatchUp
                && justBeforeDue.actorSnapshots.Sum(row => row.currentHp) ==
                    hpBeforeNewDue
                && firesAtDue
                && atDue.actorSnapshots.Sum(row => row.currentHp) < hpBeforeNewDue,
                "first attack exactly paused tick + cooldown",
                "due=" + firstNewItemDue + " paused=" + beforeReplace.battleTimeMs,
                "REFRESH_REPLACE_CADENCE_FAILED");

            C1FormalRealtimeBattleTerminalResult liveWin = null;
            int winGuard = 0;
            while (!removedSession.IsTerminal && winGuard++ < 100)
            {
                if (!removedSession.TryAdvanceBy(
                        1000L,
                        out C1FormalRealtimeBattleTickResult tick))
                {
                    break;
                }

                liveWin = tick.terminalResult ?? liveWin;
            }

            removedSession.TryGetSnapshot(
                out C1FormalRealtimeBattleSessionStateSnapshot terminalState);
            string terminalCanonical = terminalState.canonicalSignature;
            bool terminalRefreshRejected = !removedSession
                .TryRefreshItemsWhilePaused(
                    CreateRefreshRequest(
                        removedFixture,
                        terminalState,
                        "refresh-after-terminal-command",
                        "item-input-after-terminal",
                        new[] { i001 }),
                    out C1FormalRealtimeBattleItemRefreshResult terminalRefresh);
            bool reset = removedSession.TryReset(
                terminalState.sessionId,
                removedFixture.Request.sessionToken,
                terminalState.sessionGeneration,
                terminalState.resetGeneration,
                out _);
            bool staleAfterReset = !removedSession.TryRefreshItemsWhilePaused(
                CreateRefreshRequest(
                    removedFixture,
                    terminalState,
                    "refresh-after-reset-command",
                    "item-input-after-reset",
                    new[] { i001 }),
                out C1FormalRealtimeBattleItemRefreshResult resetRefresh);
            context.Check(
                "refresh-divergent-live-win-and-lifecycle-rejection",
                "rev10-refresh",
                liveWin != null
                && liveWin.accepted
                && liveWin.win
                && !liveWin.lose
                && liveWin.durationMs != 16000L
                && !liveWin.shouldWriteSave
                && !liveWin.shouldGrantReward
                && terminalRefreshRejected
                && terminalRefresh.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.TickAfterTerminal
                && terminalRefresh.stateSnapshot.canonicalSignature ==
                    terminalCanonical
                && reset
                && staleAfterReset
                && !resetRefresh.accepted,
                "live win accepted; terminal/reset refreshes reject unchanged",
                liveWin == null
                    ? "live win missing"
                    : "duration=" + liveWin.durationMs
                      + " terminalReject=" + terminalRefreshRejected
                      + " resetReject=" + staleAfterReset,
                "REFRESH_WIN_OR_LIFECYCLE_FAILED");

            ScenarioFixture stageTwoFixture = CreateFixture(
                "refresh-stage-two",
                "1-2",
                true,
                202L,
                0L);
            bool stageTwoStarted = stageTwoFixture.Session.TryStart(
                stageTwoFixture.Request,
                out _)
                && stageTwoFixture.Session.TryAdvanceBy(1000L, out _)
                && stageTwoFixture.Session.TryPause(out _);
            stageTwoFixture.Session.TryGetSnapshot(
                out C1FormalRealtimeBattleSessionStateSnapshot stageTwoPrior);
            C1FormalRealtimeBattleItemFactSnapshot i002 = CreateItemFact(
                "I002",
                "I002@white",
                true);
            bool stageTwoRefreshed = stageTwoFixture.Session
                .TryRefreshItemsWhilePaused(
                    CreateRefreshRequest(
                        stageTwoFixture,
                        stageTwoPrior,
                        "refresh-stage-two-command",
                        "item-input-i002-only",
                        new[] { i002 }),
                    out C1FormalRealtimeBattleItemRefreshResult stageTwoResult);
            context.Check(
                "stage-two-latest-active-combination-only",
                "rev10-refresh",
                stageTwoStarted
                && stageTwoRefreshed
                && stageTwoResult.activeItemFactCount == 1
                && stageTwoResult.canceledFutureItemActionCount == 1
                && stageTwoResult.retainedFutureItemActionCount == 1
                && stageTwoResult.scheduledFutureItemActionCount == 0,
                "I001 removed; unchanged I002 cadence retained",
                stageTwoRefreshed ? "accepted" : stageTwoResult?.errorCode,
                "REFRESH_STAGE_TWO_COMBINATION_FAILED");

            ScenarioFixture loseFixture = CreateFixture(
                "refresh-live-lose",
                "1-1",
                false,
                203L,
                0L);
            C1FormalRealtimeBattleSession loseSession = loseFixture.Session;
            bool loseStarted = loseSession.TryStart(loseFixture.Request, out _)
                && loseSession.TryAdvanceBy(1000L, out _)
                && loseSession.TryPause(out _);
            loseSession.TryGetSnapshot(out C1FormalRealtimeBattleSessionStateSnapshot
                losePrior);
            bool zeroAccepted = loseSession.TryRefreshItemsWhilePaused(
                CreateRefreshRequest(
                    loseFixture,
                    losePrior,
                    "refresh-live-lose-command",
                    "item-input-none-live-lose",
                    Array.Empty<C1FormalRealtimeBattleItemFactSnapshot>()),
                out _)
                && loseSession.TryResume(out _);
            C1FormalRealtimeBattleTerminalResult liveLose = null;
            int guard = 0;
            while (zeroAccepted && !loseSession.IsTerminal && guard++ < 100)
            {
                if (!loseSession.TryAdvanceBy(
                        1000L,
                        out C1FormalRealtimeBattleTickResult tick))
                {
                    break;
                }

                liveLose = tick.terminalResult ?? liveLose;
            }

            context.Check(
                "refresh-divergent-live-terminal-is-authoritative",
                "rev10-refresh",
                loseStarted
                && liveLose != null
                && liveLose.accepted
                && !liveLose.win
                && liveLose.lose
                && !liveLose.shouldWriteSave
                && !liveLose.shouldGrantReward
                && liveLose.battleResultSnapshot != null
                && !liveLose.battleResultSnapshot.shouldWriteSave
                && !liveLose.battleResultSnapshot.shouldGrantReward,
                "accepted live defeat with no persistence side effect",
                liveLose == null ? "missing" : liveLose.errorCode,
                "REFRESH_TERMINAL_TRUTH_FAILED");
        }

        private static C1FormalRealtimeBattleItemRefreshRequest
            CreateRefreshRequest(
                ScenarioFixture fixture,
                C1FormalRealtimeBattleSessionStateSnapshot prior,
                string commandId,
                string inputCanonical,
                IEnumerable<C1FormalRealtimeBattleItemFactSnapshot> facts)
        {
            return new C1FormalRealtimeBattleItemRefreshRequest(
                C1FormalRealtimeBattleSessionContract.ItemRefreshRequestSchemaId,
                fixture.Request.sessionId,
                fixture.Request.sessionToken,
                prior.sessionGeneration,
                prior.resetGeneration,
                commandId,
                prior.battleTimeMs,
                prior.canonicalSignature,
                inputCanonical,
                "verifier.item-session.canonical",
                fixture.Request.expectedItemCatalogCanonical,
                facts);
        }

        private static long NextDue(
            C1FormalRealtimeBattleSessionStateSnapshot state,
            string actionKind,
            string sourceId)
        {
            if (state == null)
            {
                return -1L;
            }

            C1FormalRealtimeBattleScheduledActionSnapshot action =
                state.scheduledActions
                    .Where(row => !row.executed
                        && string.Equals(
                            row.actionKind,
                            actionKind,
                            StringComparison.Ordinal)
                        && (string.IsNullOrEmpty(sourceId)
                            || string.Equals(
                                row.sourceId,
                                sourceId,
                                StringComparison.Ordinal)))
                    .OrderBy(row => row.dueBattleTimeMs)
                    .FirstOrDefault();
            return action == null ? -1L : action.dueBattleTimeMs;
        }

        private static void VerifyProductionAdapterStageOne(
            VerificationContext context)
        {
            BattleLaunchContext launch = CreateContext("adapter", "1-1", 1L);
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    launch.Token,
                    launch.Generation);
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool started = creation != null
                && creation.isSuccess
                && creation.authority != null
                && adapter.TryStart(
                    launch,
                    creation.authority.CreateBattleInputSnapshot(),
                    creation.authority.Current.canonicalSignature,
                    "verifier.production.adapter.session",
                    "verifier.production.adapter.token",
                    out C1FormalRealtimeBattleSessionStartSnapshot start)
                && start != null
                && start.accepted
                && start.stateSnapshot != null
                && !start.stateSnapshot.terminal
                && start.stateSnapshot.battleTimeMs == 0L;
            context.Check(
                "production-item-input-adapter-starts-live-nonterminal",
                "runtime-integration",
                started,
                "accepted at 0ms and non-terminal",
                started ? "accepted/non-terminal" : "rejected",
                "ITEM_BATTLE_INPUT_SEAM_MISSING");
            adapter.Unbind();
        }

        private static void VerifyOptionalStageTwoRuntimeFactBoundary(
            VerificationContext context)
        {
            BattleLaunchContext stageTwo = CreateContext(
                "runtime-fact-boundary",
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                401L);
            C1FormalRealtimeBattleSessionRequest twoFacts = BuildRequest(
                stageTwo,
                "runtime-fact-boundary",
                true,
                0L);
            C1FormalRealtimeBattleItemFactSnapshot i001 = twoFacts.itemFacts[0];
            C1FormalRealtimeBattleItemFactSnapshot i002 = twoFacts.itemFacts[1];

            bool zeroAccepted = StartsNonTerminal(
                CopyRequestWithFacts(
                    twoFacts,
                    "zero-active",
                    Array.Empty<C1FormalRealtimeBattleItemFactSnapshot>()));
            bool i001OnlyAccepted = StartsNonTerminal(
                CopyRequestWithFacts(twoFacts, "one-i001", new[] { i001 }));
            bool i002OnlyAccepted = StartsNonTerminal(
                CopyRequestWithFacts(twoFacts, "one-i002", new[] { i002 }));
            bool bothAccepted = StartsNonTerminal(twoFacts);

            C1FormalRealtimeBattleSession duplicateSession =
                new C1FormalRealtimeBattleSession();
            bool duplicateRejected = !duplicateSession.TryStart(
                CopyRequestWithFacts(
                    twoFacts,
                    "duplicate-i001",
                    new[] { i001, i001 }),
                out C1FormalRealtimeBattleSessionStartSnapshot duplicateStart)
                && duplicateStart.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.ItemInputRejected;

            C1FormalRealtimeBattleItemFactSnapshot unknown =
                new C1FormalRealtimeBattleItemFactSnapshot(
                    "I999@white",
                    "verifier.unknown.item.instance",
                    "I999",
                    i002.rarityVersionKey,
                    true,
                    i002.directDamage,
                    i002.cooldownMs,
                    i002.projectionCanonicalSignature,
                    i002.itemCatalogCanonicalSignature);
            C1FormalRealtimeBattleSession unknownSession =
                new C1FormalRealtimeBattleSession();
            bool unknownRejected = !unknownSession.TryStart(
                CopyRequestWithFacts(
                    twoFacts,
                    "unknown",
                    new[] { i001, unknown }),
                out C1FormalRealtimeBattleSessionStartSnapshot unknownStart)
                && unknownStart.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.ItemInputRejected;

            ScenarioFixture stageOneFixture = CreateFixture(
                "stage-one-extra-i002",
                C1Lv1EarlyEncounterBalanceContract.Stage1_1,
                false,
                402L,
                0L);
            C1FormalRealtimeBattleSession stageOneSession =
                new C1FormalRealtimeBattleSession();
            bool stageOneI002Rejected = !stageOneSession.TryStart(
                CopyRequestWithFacts(
                    stageOneFixture.Request,
                    "stage-one-extra-i002",
                    new[]
                    {
                        stageOneFixture.Request.itemFacts[0],
                        i002
                    }),
                out C1FormalRealtimeBattleSessionStartSnapshot stageOneStart)
                && stageOneStart.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.ItemInputRejected;

            C1FormalRealtimeBattleSession unlitSession =
                new C1FormalRealtimeBattleSession();
            C1FormalRealtimeBattleItemFactSnapshot unlit =
                new C1FormalRealtimeBattleItemFactSnapshot(
                    i002.sourceId,
                    i002.itemInstanceId,
                    i002.baseItemId,
                    i002.rarityVersionKey,
                    false,
                    i002.directDamage,
                    i002.cooldownMs,
                    i002.projectionCanonicalSignature,
                    i002.itemCatalogCanonicalSignature);
            bool unlitRejected = !unlitSession.TryStart(
                CopyRequestWithFacts(
                    twoFacts,
                    "unlit-runtime-fact",
                    new[] { i001, unlit }),
                out C1FormalRealtimeBattleSessionStartSnapshot unlitStart)
                && unlitStart.errorCode ==
                    C1FormalRealtimeBattleErrorCodes.ItemInputRejected;

            context.Check(
                "stage-two-runtime-fact-boundary",
                "runtime-integration",
                zeroAccepted
                && i001OnlyAccepted
                && i002OnlyAccepted
                && bothAccepted
                && duplicateRejected
                && unknownRejected
                && stageOneI002Rejected
                && unlitRejected,
                "stage 1-2 accepts 0/1/2 lit active facts in any valid identity combination",
                "zero=" + zeroAccepted
                + ";i001Only=" + i001OnlyAccepted
                + ";i002Only=" + i002OnlyAccepted
                + ";both=" + bothAccepted
                + ";duplicate=" + duplicateRejected
                + ";unknown=" + unknownRejected
                + ";stageOneExtra=" + stageOneI002Rejected
                + ";unlit=" + unlitRejected,
                "OPTIONAL_I002_RUNTIME_FACT_BOUNDARY_FAILED");
        }

        private static bool StartsNonTerminal(
            C1FormalRealtimeBattleSessionRequest request)
        {
            C1FormalRealtimeBattleSession session =
                new C1FormalRealtimeBattleSession();
            return session.TryStart(
                    request,
                    out C1FormalRealtimeBattleSessionStartSnapshot start)
                && start != null
                && start.accepted
                && start.stateSnapshot != null
                && !start.stateSnapshot.terminal
                && start.stateSnapshot.battleTimeMs == 0L;
        }

        private static void VerifyActiveItemAdmissionAndRealDefeat(
            VerificationContext context)
        {
            AdmissionEvidence[] rows =
            {
                RunAdapterAdmission(
                    "stage-1-2-i001-only-no-i002-entitlement",
                    true,
                    true,
                    false,
                    null,
                    1,
                    501L,
                    false),
                RunAdapterAdmission(
                    "stage-1-2-i001-with-i002-tray",
                    true,
                    true,
                    false,
                    null,
                    1,
                    502L),
                RunAdapterAdmission(
                    "stage-1-2-both-active",
                    true,
                    true,
                    true,
                    true,
                    2,
                    503L)
            };
            context.Admissions.AddRange(rows);

            foreach (AdmissionEvidence row in rows)
            {
                bool terminalShape = row.Terminal != null
                    && row.Terminal.accepted
                    && row.Terminal.win
                    && !row.Terminal.lose
                    && !row.Terminal.shouldWriteSave
                    && !row.Terminal.shouldGrantReward
                    && row.Terminal.battleResultSnapshot != null
                    && !row.Terminal.battleResultSnapshot.shouldWriteSave
                    && !row.Terminal.battleResultSnapshot.shouldGrantReward;
                bool passed = row.StartedNonTerminal
                    && row.ScheduledItemCount == row.ExpectedActiveCount
                    && row.TickCount > 0
                    && terminalShape;
                context.Check(
                    "active-admission-" + Slug(row.Scenario),
                    "runtime-integration",
                    passed,
                    "accepted non-terminal; active=" + row.ExpectedActiveCount
                    + "; terminal=victory",
                    row.Describe(),
                    "ACTIVE_ITEM_ADMISSION_OR_TERMINAL_FAILED");
                foreach (C1FormalRealtimeBattleCue cue in row.Cues)
                {
                    context.SeenCueKinds.Add(cue.cueKind);
                }
            }

            AdmissionEvidence i001Only = rows[0];
            AdmissionEvidence i002Tray = rows[1];
            AdmissionEvidence both = rows[2];
            bool noI002Synthesized = i001Only.Start != null
                && i001Only.Start.stateSnapshot != null
                && i001Only.Start.stateSnapshot.scheduledActions.All(action =>
                    !string.Equals(
                        action.sourceId,
                        "I002@white",
                        StringComparison.Ordinal))
                && i001Only.Cues.All(cue => !string.Equals(
                    cue.sourceId,
                    "I002@white",
                    StringComparison.Ordinal))
                && i001Only.Terminal != null;
            context.Check(
                "i001-only-no-i002-synthesis",
                "runtime-integration",
                noI002Synthesized,
                "no I002 action or cue",
                i001Only.Describe(),
                "I002_SYNTHESIZED");
            context.Check(
                "i001-only-existing-live-trace",
                "runtime-integration",
                MatchesTerminal(i001Only.Terminal, true, 48000L, 10, 20, 24),
                "48000ms / enemy=10 / playerHp=20 / item=24",
                i001Only.Describe(),
                "I001_ONLY_TRACE_DRIFT");
            context.Check(
                "i002-present-weak-live-trace",
                "runtime-integration",
                MatchesTerminal(i002Tray.Terminal, true, 48000L, 10, 20, 24),
                "48000ms / enemy=10 / playerHp=20 / item=24",
                i002Tray.Describe(),
                "I002_PRESENT_WEAK_TRACE_DRIFT");
            context.Check(
                "both-active-existing-live-trace",
                "runtime-integration",
                MatchesTerminal(both.Terminal, true, 36000L, 7, 44, 35),
                "36000ms / enemy=7 / playerHp=44 / item=35",
                both.Describe(),
                "BOTH_ACTIVE_TRACE_DRIFT");

            VerifyAdmissionRejections(context);
        }

        private static void VerifyPool15FormalIntegration(
            VerificationContext context)
        {
            C1CampaignLootEligibleItemPool15Result poolResult =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            C1CampaignLootPool15BattleCandidateProfileSnapshot candidates =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            bool sourceAccepted = poolResult != null
                && poolResult.accepted
                && poolResult.snapshot != null
                && poolResult.snapshot.Profiles.Count == 15
                && poolResult.snapshot.Families.Count == 5
                && string.Equals(
                    candidates.dataMaturity,
                    "CANDIDATE",
                    StringComparison.Ordinal)
                && !candidates.entersFormalFlow;
            context.Check(
                "pool15-protected-source-and-candidate-maturity",
                "formal-pool15",
                sourceAccepted,
                "15 descriptors / 5 families / CANDIDATE / entersFormalFlow=false",
                sourceAccepted ? "PASS" : "FAIL",
                "POOL15_SOURCE_REJECTED");
            if (!sourceAccepted)
            {
                return;
            }

            int generation = 8000;
            foreach (C1CampaignLootItemProfileSnapshot descriptor in
                     poolResult.snapshot.Profiles.OrderBy(
                         row => row.baseItemId,
                         StringComparer.Ordinal))
            {
                Pool15ScenarioEvidence scenario = StartPool15Scenario(
                    "coverage-" + descriptor.baseItemId.ToLowerInvariant(),
                    new[] { descriptor.baseItemId },
                    Array.Empty<string>(),
                    generation++);
                C1CampaignLootPool15BattleCandidateProfile candidate =
                    candidates.Find(descriptor.baseItemId);
                string beforeState = scenario.Start == null
                    || scenario.Start.stateSnapshot == null
                    ? string.Empty
                    : scenario.Start.stateSnapshot.effectStateSnapshot
                        .canonicalSignature;
                int enemyHpBefore = scenario.Start == null
                    || scenario.Start.stateSnapshot == null
                        ? -1
                        : scenario.Start.stateSnapshot.actorSnapshots.Sum(
                            actor => actor.currentHp);
                C1FormalRealtimeBattlePool15EventResult result = null;
                IEnumerable<
                    C1FormalRealtimeBattleActionProgressRelationSnapshot>
                    progressRelations = string.Equals(
                        descriptor.baseItemId,
                        "I022",
                        StringComparison.Ordinal)
                    ? new[]
                    {
                        new C1FormalRealtimeBattleActionProgressRelationSnapshot(
                            scenario.FormalSnapshot.canonicalSignature,
                            scenario.FormalSnapshot.arrangementCanonicalSignature,
                            "verifier.pool15.instance.I022",
                            "adjacent_lit_item",
                            new[]
                            {
                                "I001@white"
                            })
                    }
                    : Array.Empty<
                        C1FormalRealtimeBattleActionProgressRelationSnapshot>();
                bool executed = scenario.Started
                    && scenario.Adapter.TryExecutePool15Event(
                        "verifier.pool15.event." + descriptor.baseItemId,
                        descriptor.triggerId,
                        new[] { descriptor.conditionId },
                        progressRelations,
                        out result);
                C1FormalRealtimeBattleEffectApplicationSnapshot application =
                    scenario.Adapter.Session.Pool15ApplicationLedger
                        .SingleOrDefault();
                C1FormalRealtimeBattleEffectCue cue = scenario.Adapter.Session
                    .Pool15CueLedger.SingleOrDefault();
                scenario.Adapter.TryGetSnapshot(
                    out C1FormalRealtimeBattleSessionStateSnapshot after);
                bool passed = executed
                    && result != null
                    && result.accepted
                    && result.acceptedSourceCount == 1
                    && application != null
                    && cue != null
                    && after != null
                    && after.activePool15SourceCount == 1
                    && !string.Equals(
                        beforeState,
                        after.effectStateSnapshot.canonicalSignature,
                        StringComparison.Ordinal)
                    && string.Equals(
                        application.sourceBaseItemId,
                        descriptor.baseItemId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        application.familyId,
                        descriptor.familyId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        application.familyVariantId,
                        descriptor.familyVariantId,
                        StringComparison.Ordinal)
                    && application.requestedAmount == candidate.amount
                    && application.appliedAmount > 0L
                    && string.Equals(
                        application.nativeUnitId,
                        candidate.nativeUnitId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        application.candidateProfileId,
                        C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                            .ProfileId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        application.candidateDataMaturity,
                        "CANDIDATE",
                        StringComparison.Ordinal)
                    && application.targetAfter > application.targetBefore
                    && application.triggerBattleTimeMs == 0L
                    && application.resolveBattleTimeMs == 0L
                    && string.Equals(
                        cue.cueIdentity,
                        application.cueIdentity,
                        StringComparison.Ordinal);
                int enemyHpAfter = after == null
                    ? -1
                    : after.actorSnapshots.Sum(actor => actor.currentHp);
                bool refundSeparated = !string.Equals(
                        descriptor.baseItemId,
                        "I002",
                        StringComparison.Ordinal)
                    || (enemyHpBefore == enemyHpAfter
                        && after.scheduledActions.All(action =>
                            !string.Equals(
                                action.sourceId,
                                "I002@white",
                                StringComparison.Ordinal)));
                passed &= refundSeparated;
                Pool15CoverageEvidence evidence =
                    new Pool15CoverageEvidence(
                        descriptor.baseItemId,
                        descriptor.familyId,
                        descriptor.familyVariantId,
                        descriptor.triggerId,
                        descriptor.conditionId,
                        candidate.nativeUnitId,
                        application,
                        passed);
                context.PoolCoverage.Add(evidence);
                context.Check(
                    "pool15-formal-coverage-" + descriptor.baseItemId,
                    "formal-pool15",
                    passed,
                    "source-attributable live state + ledger + cue",
                    evidence.Describe(),
                    "POOL15_FORMAL_COVERAGE_FAILED");
                if (cue != null)
                {
                    context.SeenCueKinds.Add(
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted);
                }

                if (string.Equals(
                        descriptor.baseItemId,
                        "I015",
                        StringComparison.Ordinal)
                    && after != null)
                {
                    bool guardBeforeAttack = after.playerSnapshot.guard == 8L;
                    bool advanced = scenario.Adapter.TryAdvanceTo(
                        4800L,
                        out C1FormalRealtimeBattleTickResult guardTick);
                    bool guardAbsorbed = advanced
                        && guardTick != null
                        && guardTick.stateSnapshot != null
                        && guardTick.stateSnapshot.playerSnapshot.currentHp == 100
                        && guardTick.stateSnapshot.enemyApplicationCount == 1
                        && (guardTick.stateSnapshot.playerSnapshot.guard == 0L
                            || (guardTick.stateSnapshot.playerSnapshot.guard == 8L
                                && scenario.Adapter.Session
                                    .Pool15ApplicationLedger.Count >= 2));
                    context.Check(
                        "pool15-guard-absorbs-enemy-damage-before-hp",
                        "formal-live-state",
                        guardBeforeAttack && guardAbsorbed,
                        "first enemy damage is absorbed before HP; any remaining guard is backed by the automatic second application",
                        guardBeforeAttack + "/" + guardAbsorbed
                        + ";advanced=" + advanced + ";hp="
                        + (guardTick?.stateSnapshot?.playerSnapshot == null
                            ? "missing"
                            : guardTick.stateSnapshot.playerSnapshot.currentHp
                                .ToString(CultureInfo.InvariantCulture))
                        + ";guard="
                        + (guardTick?.stateSnapshot?.playerSnapshot == null
                            ? "missing"
                            : guardTick.stateSnapshot.playerSnapshot.guard
                                .ToString(CultureInfo.InvariantCulture))
                        + ";enemy="
                        + (guardTick?.stateSnapshot == null
                            ? "missing"
                            : guardTick.stateSnapshot.enemyApplicationCount
                                .ToString(CultureInfo.InvariantCulture)),
                        "POOL15_GUARD_ABSORPTION_FAILED");
                }

                scenario.Adapter.Unbind();
            }

            int distinctFamilies = context.PoolCoverage
                .Where(row => row.Passed)
                .Select(row => row.FamilyId)
                .Distinct(StringComparer.Ordinal)
                .Count();
            context.Check(
                "pool15-exact-coverage",
                "formal-pool15",
                context.PoolCoverage.Count == 15
                && context.PoolCoverage.All(row => row.Passed)
                && distinctFamilies == 5,
                "15/15 descriptors and 5/5 families",
                context.PoolCoverage.Count(row => row.Passed) + "/15;"
                + distinctFamilies + "/5",
                "POOL15_COVERAGE_INCOMPLETE");

            VerifyPool15ActiveFiltering(context, generation);
            VerifyPool15SourceOrderAndSensitivity(context, generation + 20);
            VerifyPool15AtomicRejections(context, generation + 100);
            VerifyPool15FormalMappingShape(context);
        }

        private static void VerifyItemInstancePresentationCueLineage(
            VerificationContext context)
        {
            AdmissionEvidence fixedTrace = RunAdapterAdmission(
                "item-instance-cue-lineage",
                true,
                true,
                true,
                true,
                2,
                9800L);
            Dictionary<string, string> expectedBaseByInstance =
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    {
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                        "I001"
                    },
                    {
                        RewardVerifierItemInstanceId,
                        "I002"
                    }
                };
            C1FormalRealtimeBattleCue[] fixedItemCues = fixedTrace.Cues
                .Where(cue => !string.IsNullOrEmpty(
                    cue.sourceItemInstanceId))
                .ToArray();
            bool fixedInstancesExact = fixedTrace.Terminal != null
                && fixedTrace.Terminal.accepted
                && fixedItemCues.Length > 0
                && fixedItemCues.All(cue =>
                    expectedBaseByInstance.TryGetValue(
                        cue.sourceItemInstanceId,
                        out string expectedBase)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        expectedBase,
                        StringComparison.Ordinal)
                    && string.IsNullOrEmpty(cue.effectFamilyId)
                    && string.IsNullOrEmpty(cue.effectVariantId))
                && expectedBaseByInstance.Keys.All(instanceId =>
                    fixedItemCues.Any(cue => string.Equals(
                        cue.sourceItemInstanceId,
                        instanceId,
                        StringComparison.Ordinal)));
            string[] requiredItemCueKinds =
            {
                C1FormalRealtimeBattleCueKinds.ItemTriggerScheduled,
                C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                C1FormalRealtimeBattleCueKinds.HitAccepted,
                C1FormalRealtimeBattleCueKinds.ActorHpChanged,
                C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                C1FormalRealtimeBattleCueKinds.ActorDefeated,
                C1FormalRealtimeBattleCueKinds.TargetChanged
            };
            bool followUpLineage = requiredItemCueKinds.All(cueKind =>
                    fixedItemCues.Any(cue => string.Equals(
                        cue.cueKind,
                        cueKind,
                        StringComparison.Ordinal)))
                && fixedItemCues.Where(cue => !string.IsNullOrEmpty(
                        cue.acceptedApplicationEventId))
                    .GroupBy(
                        cue => cue.acceptedApplicationEventId,
                        StringComparer.Ordinal)
                    .All(group => group.Select(cue =>
                            cue.sourceItemInstanceId + "|"
                            + cue.sourceBaseItemId)
                        .Distinct(StringComparer.Ordinal).Count() == 1);
            context.Check(
                "fixed-item-instance-enters-live-fact-and-cue-lineage",
                "presentation-cue-lineage",
                fixedInstancesExact && followUpLineage,
                "validated I001/I002 ItemInstance ids persist through scheduled, hit, HP, float, death, and target cues",
                "itemCues=" + fixedItemCues.Length + ";instances="
                + string.Join(",", fixedItemCues.Select(cue =>
                        cue.sourceItemInstanceId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)),
                "FIXED_ITEM_CUE_LINEAGE_FAILED");

            bool noPartialOrigin = fixedTrace.Cues.All(cue =>
                !string.IsNullOrEmpty(cue.sourceItemInstanceId)
                    ? !string.IsNullOrEmpty(cue.sourceBaseItemId)
                    : string.IsNullOrEmpty(cue.sourceBaseItemId)
                      && string.IsNullOrEmpty(cue.effectFamilyId)
                      && string.IsNullOrEmpty(cue.effectVariantId));
            bool nonItemCuesEmpty = fixedTrace.Cues.Where(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.SessionStarted,
                        StringComparison.Ordinal)
                    || string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.ActorSpawnVisible,
                        StringComparison.Ordinal)
                    || string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.EnemyAttackScheduled,
                        StringComparison.Ordinal)
                    || string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                        StringComparison.Ordinal)
                    || string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.PlayerHpChanged,
                        StringComparison.Ordinal)
                    || string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.BattleTerminal,
                        StringComparison.Ordinal)
                    || (string.Equals(
                            cue.cueKind,
                            C1FormalRealtimeBattleCueKinds.TargetChanged,
                            StringComparison.Ordinal)
                        && string.IsNullOrEmpty(
                            cue.acceptedApplicationEventId))
                    || ((string.Equals(
                                cue.cueKind,
                                C1FormalRealtimeBattleCueKinds.HitAccepted,
                                StringComparison.Ordinal)
                            || string.Equals(
                                cue.cueKind,
                                C1FormalRealtimeBattleCueKinds.DamageFloatPayload,
                                StringComparison.Ordinal))
                        && string.Equals(
                            cue.sourceOwner,
                            C1FormalRealtimeBattleSessionContract.EnemyOwner,
                            StringComparison.Ordinal)))
                .All(cue => string.IsNullOrEmpty(cue.sourceItemInstanceId)
                    && string.IsNullOrEmpty(cue.sourceBaseItemId)
                    && string.IsNullOrEmpty(cue.effectFamilyId)
                    && string.IsNullOrEmpty(cue.effectVariantId));
            context.Check(
                "non-item-cues-have-empty-item-origin",
                "presentation-cue-lineage",
                noPartialOrigin && nonItemCuesEmpty,
                "enemy, spawn, target-only, HP, and terminal cues contain no Item identity",
                "partial=" + noPartialOrigin + ";nonItem="
                + nonItemCuesEmpty,
                "NON_ITEM_CUE_IDENTITY_LEAK");
        }

        private static void VerifyCumulativeItemInput(
            VerificationContext context)
        {
            long generation = 8800L;
            string[] fiveFamilyBaseIds =
            {
                "I007", "I004", "I015", "I025", "I010"
            };
            string[] fiveFamilyInstanceIds = fiveFamilyBaseIds.Select(
                    (baseItemId, index) =>
                        "verifier.cumulative.five." + index + "."
                        + baseItemId)
                .ToArray();
            List<C1FormalItemBattleItemRow> fiveFamilyRows = new List<
                C1FormalItemBattleItemRow>
            {
                CreateBattleItemRow(
                    "I001",
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                    InitialVerifierInstanceSignature,
                    true,
                    true)
            };
            fiveFamilyRows.AddRange(fiveFamilyBaseIds.Select(
                (baseItemId, index) => CreatePool15BattleItemRow(
                    baseItemId,
                    fiveFamilyInstanceIds[index],
                    true,
                    true)));
            bool fiveStarted = TryStartCumulativeItemSnapshot(
                "five-family",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                fiveFamilyRows,
                out C1FormalRealtimeBattleSessionAdapter fiveAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot fiveStart);
            bool fiveApplied = fiveStarted
                && AdvanceUntilPoolSourcesApplied(
                    fiveAdapter,
                    fiveFamilyInstanceIds,
                    30000L);
            C1FormalRealtimeBattleEffectApplicationSnapshot[] fiveApps =
                fiveAdapter.Session.Pool15ApplicationLedger
                    .Where(row => fiveFamilyInstanceIds.Contains(
                        row.sourceItemInstanceId,
                        StringComparer.Ordinal))
                    .GroupBy(
                        row => row.sourceItemInstanceId,
                        StringComparer.Ordinal)
                    .Select(group => group.First())
                    .ToArray();
            C1FormalRealtimeBattleCue[] fiveCues = fiveAdapter.Session.CueLedger
                .Where(cue => string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                        StringComparison.Ordinal)
                    && fiveFamilyInstanceIds.Contains(
                        cue.sourceItemInstanceId,
                        StringComparer.Ordinal))
                .ToArray();
            bool fiveFamilies = fiveStarted
                && fiveStart?.stateSnapshot != null
                && fiveStart.stateSnapshot.activePool15SourceCount == 5
                && fiveApplied
                && fiveApps.Select(row => row.familyId)
                    .Distinct(StringComparer.Ordinal).Count() == 5
                && fiveApps.All(row => fiveCues.Any(cue =>
                    string.Equals(
                        cue.sourceItemInstanceId,
                        row.sourceItemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        row.sourceBaseItemId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.effectFamilyId,
                        row.familyId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.effectVariantId,
                        row.familyVariantId,
                        StringComparison.Ordinal)));
            context.Check(
                "cumulative-five-family-auto-runtime",
                "cumulative-input",
                fiveFamilies,
                "one Item snapshot schedules and applies every placed+lit family source with exact cue lineage",
                "start=" + fiveStarted + ";apps=" + fiveApps.Length
                + ";families=" + fiveApps.Select(row => row.familyId)
                    .Distinct(StringComparer.Ordinal).Count(),
                "CUMULATIVE_FIVE_FAMILY_FAILED");
            fiveAdapter.Unbind();

            bool rewardOnlyStarted = TryStartCumulativeItemSnapshot(
                "reward-without-i001",
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                generation++,
                new[]
                {
                    CreatePool15BattleItemRow(
                        "I007",
                        "verifier.cumulative.reward-only.I007",
                        true,
                        true)
                },
                out C1FormalRealtimeBattleSessionAdapter rewardOnlyAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot rewardOnlyStart);
            bool rewardOnlyApplied = rewardOnlyStarted
                && AdvanceUntilPoolSourcesApplied(
                    rewardOnlyAdapter,
                    new[] { "verifier.cumulative.reward-only.I007" },
                    20000L);
            bool zeroStarted = TryStartCumulativeItemSnapshot(
                "zero-active",
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                generation++,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter zeroAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot zeroStart);
            bool zeroHasNoItemSchedule = zeroStarted
                && zeroStart?.stateSnapshot != null
                && zeroStart.stateSnapshot.activePool15SourceCount == 0
                && zeroStart.stateSnapshot.scheduledActions.All(action =>
                    !string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                        StringComparison.Ordinal)
                    && !string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds
                            .Pool15EffectResolve,
                        StringComparison.Ordinal));
            context.Check(
                "cumulative-zero-and-no-i001-admission",
                "admission",
                rewardOnlyStarted
                && rewardOnlyStart?.stateSnapshot != null
                && rewardOnlyStart.stateSnapshot.activePool15SourceCount == 1
                && rewardOnlyApplied
                && zeroHasNoItemSchedule,
                "reward-only executes; zero-active starts with no Item schedule",
                "reward=" + rewardOnlyStarted + "/" + rewardOnlyApplied
                + ";zero=" + zeroStarted + "/" + zeroHasNoItemSchedule,
                "CUMULATIVE_I001_TICKET_REMAINS");
            rewardOnlyAdapter.Unbind();
            zeroAdapter.Unbind();

            const string duplicateA =
                "verifier.cumulative.duplicate-base.a";
            const string duplicateB =
                "verifier.cumulative.duplicate-base.b";
            bool duplicateBaseStarted = TryStartCumulativeItemSnapshot(
                "duplicate-base",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                new[]
                {
                    CreatePool15BattleItemRow("I015", duplicateA, true, true),
                    CreatePool15BattleItemRow("I015", duplicateB, true, true)
                },
                out C1FormalRealtimeBattleSessionAdapter duplicateBaseAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot
                    duplicateBaseStart);
            bool duplicateBaseApplied = duplicateBaseStarted
                && AdvanceUntilPoolSourcesApplied(
                    duplicateBaseAdapter,
                    new[] { duplicateA, duplicateB },
                    20000L);
            C1FormalRealtimeBattleCue[] duplicateCues = duplicateBaseAdapter
                .Session.CueLedger.Where(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        "I015",
                        StringComparison.Ordinal))
                .GroupBy(
                    cue => cue.sourceItemInstanceId,
                    StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
            const string duplicateInstance =
                "verifier.cumulative.duplicate-instance";
            bool duplicateInstanceRejected =
                !TryStartCumulativeItemSnapshot(
                    "duplicate-instance",
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    generation++,
                    new[]
                    {
                        CreatePool15BattleItemRow(
                            "I015",
                            duplicateInstance,
                            true,
                            true),
                        CreatePool15BattleItemRow(
                            "I007",
                            duplicateInstance,
                            true,
                            true)
                    },
                    out C1FormalRealtimeBattleSessionAdapter
                        duplicateInstanceAdapter,
                    out _,
                    out C1FormalRealtimeBattleSessionStartSnapshot
                        duplicateInstanceStart)
                && string.Equals(
                    duplicateInstanceStart?.errorCode,
                    C1FormalRealtimeBattleErrorCodes.ItemInputRejected,
                    StringComparison.Ordinal);
            context.Check(
                "cumulative-instance-identity-boundary",
                "source-correlation",
                duplicateBaseStarted
                && duplicateBaseStart?.stateSnapshot != null
                && duplicateBaseStart.stateSnapshot.activePool15SourceCount == 2
                && duplicateBaseApplied
                && duplicateCues.Length == 2
                && duplicateCues.Select(cue => cue.sourceItemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() == 2
                && duplicateCues.Select(cue => cue.canonicalSignature)
                    .Distinct(StringComparer.Ordinal).Count() == 2
                && duplicateInstanceRejected,
                "duplicate base stays distinct; duplicate ItemInstance rejects once at Adapter",
                "base=" + duplicateBaseStarted + "/" + duplicateCues.Length
                + ";sameInstanceRejected=" + duplicateInstanceRejected,
                "CUMULATIVE_INSTANCE_IDENTITY_FAILED");
            duplicateBaseAdapter.Unbind();
            duplicateInstanceAdapter.Unbind();

            const string rewardedI002Instance =
                "verifier.cumulative.rewarded.I002";
            string coexistItemSession =
                "verifier.cumulative.item-session.i002-coexist";
            C1FormalItemBattleInputSnapshot coexistActive =
                CreateStageTwoItemSnapshot(
                    "cumulative-i002-coexist-active",
                    coexistItemSession,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            true,
                            true),
                        CreatePool15BattleItemRow(
                            "I002",
                            rewardedI002Instance,
                            true,
                            true)
                    });
            C1FormalRealtimeBattleSessionAdapter coexistAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool coexistStarted = coexistAdapter
                .TryStartFromCumulativeItemSnapshot(
                    CreateContext(
                        "cumulative-i002-coexist",
                        C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                        generation++),
                    coexistActive,
                    coexistItemSession,
                    "verifier.cumulative.session.i002-coexist",
                    "verifier.cumulative.token.i002-coexist",
                    out C1FormalRealtimeBattleSessionStartSnapshot
                        coexistStart);
            bool coexistApplied = coexistStarted
                && coexistAdapter.TryAdvanceBy(1L, out _)
                && coexistAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    string.Equals(
                        row.sourceItemInstanceId,
                        rewardedI002Instance,
                        StringComparison.Ordinal)
                    && string.Equals(
                        row.sourceBaseItemId,
                        "I002",
                        StringComparison.Ordinal)
                    && string.Equals(
                        row.familyId,
                        "DIRECT_TEMPO",
                        StringComparison.Ordinal))
                && coexistAdapter.Session.CueLedger.Any(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceItemInstanceId,
                        rewardedI002Instance,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        "I002",
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.effectFamilyId,
                        "DIRECT_TEMPO",
                        StringComparison.Ordinal));
            bool fixedI002Separate = coexistStarted
                && coexistStart?.stateSnapshot != null
                && coexistStart.stateSnapshot.activePool15SourceCount == 1
                && coexistAdapter.Session.CueLedger.Any(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.ItemTriggerScheduled,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceItemInstanceId,
                        RewardVerifierItemInstanceId,
                        StringComparison.Ordinal)
                    && !string.Equals(
                        cue.sourceItemInstanceId,
                        rewardedI002Instance,
                        StringComparison.Ordinal));
            bool coexistPaused = coexistApplied
                && coexistAdapter.TryPause(out _);
            C1FormalItemBattleInputSnapshot coexistTray =
                CreateStageTwoItemSnapshot(
                    "cumulative-i002-coexist-tray",
                    coexistItemSession,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            true,
                            true),
                        CreatePool15BattleItemRow(
                            "I002",
                            rewardedI002Instance,
                            false,
                            null)
                    });
            int rewardedApplicationCountBeforeTray = coexistAdapter.Session
                .Pool15ApplicationLedger.Count(row => string.Equals(
                    row.sourceItemInstanceId,
                    rewardedI002Instance,
                    StringComparison.Ordinal));
            bool rewardedTrayInactive = coexistPaused
                && coexistAdapter
                    .TryRefreshFromCumulativeItemSnapshotWhilePaused(
                        coexistTray,
                        coexistItemSession,
                        "verifier.cumulative.i002-coexist.tray",
                        out C1FormalRealtimeBattleItemRefreshResult
                            coexistRefresh)
                && coexistRefresh.accepted
                && coexistRefresh.stateSnapshot.activePool15SourceCount == 0
                && coexistAdapter.TryResume(out _)
                && coexistAdapter.TryAdvanceBy(1L, out _)
                && coexistAdapter.Session.Pool15ApplicationLedger.Count(row =>
                    string.Equals(
                        row.sourceItemInstanceId,
                        rewardedI002Instance,
                        StringComparison.Ordinal))
                    == rewardedApplicationCountBeforeTray;
            context.Check(
                "cumulative-fixed-and-rewarded-i002-coexist",
                "source-correlation",
                fixedI002Separate && coexistApplied && rewardedTrayInactive,
                "fixed I002 and rewarded I002 remain distinct; rewarded active applies exact-instance cue; Tray schedules nothing",
                "fixed=" + fixedI002Separate + ";rewarded="
                + coexistApplied + ";tray=" + rewardedTrayInactive,
                "CUMULATIVE_REWARDED_I002_IDENTITY_FAILED");
            coexistAdapter.Unbind();

            bool inactiveStarted = TryStartCumulativeItemSnapshot(
                "tray-unlit",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                new[]
                {
                    CreatePool15BattleItemRow(
                        "I007",
                        "verifier.cumulative.tray.I007",
                        false,
                        null),
                    CreatePool15BattleItemRow(
                        "I004",
                        "verifier.cumulative.unlit.I004",
                        true,
                        false),
                    CreatePool15BattleItemRow(
                        "I015",
                        "verifier.cumulative.active.I015",
                        true,
                        true)
                },
                out C1FormalRealtimeBattleSessionAdapter inactiveAdapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot inactiveStart);
            string[] inactiveIds =
            {
                "verifier.cumulative.tray.I007",
                "verifier.cumulative.unlit.I004"
            };
            bool inactiveExcluded = inactiveStarted
                && inactiveStart?.stateSnapshot != null
                && inactiveStart.stateSnapshot.activePool15SourceCount == 1
                && inactiveStart.stateSnapshot.scheduledActions.All(action =>
                    !inactiveIds.Contains(action.sourceId,
                        StringComparer.Ordinal));
            context.Check(
                "cumulative-tray-unlit-exclusion",
                "filtering",
                inactiveExcluded,
                "Tray and placed-unlit rows remain valid but schedule nothing",
                "start=" + inactiveStarted + ";active="
                + (inactiveStart?.stateSnapshot == null
                    ? -1
                    : inactiveStart.stateSnapshot.activePool15SourceCount),
                "CUMULATIVE_INACTIVE_ROW_SCHEDULED");
            inactiveAdapter.Unbind();

            string[] sevenBaseIds =
            {
                "I007", "I004", "I015", "I025", "I010", "I003", "I014"
            };
            bool growingStages = true;
            foreach (string stageId in new[]
                     {
                         C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                         C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5
                     })
            {
                string stageSuffix = stageId.Replace('-', '_');
                string[] sevenInstanceIds = sevenBaseIds.Select(
                        (baseItemId, index) =>
                            "verifier.cumulative.seven." + stageSuffix + "."
                            + index + "." + baseItemId)
                    .ToArray();
                bool stageStarted = TryStartCumulativeItemSnapshot(
                    "seven-" + stageSuffix,
                    stageId,
                    generation++,
                    sevenBaseIds.Select((baseItemId, index) =>
                        CreatePool15BattleItemRow(
                            baseItemId,
                            sevenInstanceIds[index],
                            true,
                            true)),
                    out C1FormalRealtimeBattleSessionAdapter stageAdapter,
                    out _,
                    out C1FormalRealtimeBattleSessionStartSnapshot stageStart);
                bool stageApplied = stageStarted
                    && AdvanceUntilPoolSourcesApplied(
                        stageAdapter,
                        sevenInstanceIds,
                        40000L);
                growingStages &= stageStarted
                    && stageStart?.stateSnapshot != null
                    && stageStart.stateSnapshot.activePool15SourceCount == 7
                    && stageApplied;
                stageAdapter.Unbind();
            }
            context.Check(
                "cumulative-seven-instance-stage14-stage15",
                "growing-build",
                growingStages,
                "1-4 and 1-5 admit seven current active instances and every source applies",
                growingStages ? "PASS" : "FAIL",
                "CUMULATIVE_GROWING_BUILD_FAILED");

            const string refreshKeep =
                "verifier.cumulative.refresh.keep.I015";
            const string refreshRemove =
                "verifier.cumulative.refresh.remove.I015";
            const string refreshAdd =
                "verifier.cumulative.refresh.add.I025";
            string refreshItemSession =
                "verifier.cumulative.item-session.refresh";
            C1FormalItemBattleInputSnapshot refreshBefore =
                CreateStageTwoItemSnapshot(
                    "cumulative-refresh-before",
                    refreshItemSession,
                    new[]
                    {
                        CreatePool15BattleItemRow(
                            "I015", refreshKeep, true, true),
                        CreatePool15BattleItemRow(
                            "I015", refreshRemove, true, true)
                    });
            C1FormalRealtimeBattleSessionAdapter refreshAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool refreshStarted = refreshAdapter
                .TryStartFromCumulativeItemSnapshot(
                    CreateContext(
                        "cumulative-refresh",
                        C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                        generation++),
                    refreshBefore,
                    refreshItemSession,
                    "verifier.cumulative.session.refresh",
                    "verifier.cumulative.token.refresh",
                    out C1FormalRealtimeBattleSessionStartSnapshot
                        refreshStart);
            C1FormalRealtimeBattleSessionStateSnapshot refreshPausedBefore =
                null;
            bool reachedDue = refreshStarted
                && refreshAdapter.TryPause(out _)
                && refreshAdapter.TryGetSnapshot(out refreshPausedBefore);
            C1FormalRealtimeBattleCue[] refreshHistoryBefore = reachedDue
                ? refreshAdapter.Session.CueLedger.ToArray()
                : Array.Empty<C1FormalRealtimeBattleCue>();
            int refreshApplicationsBefore = refreshAdapter.Session
                .Pool15ApplicationLedger.Count;
            C1FormalItemBattleInputSnapshot refreshAfter =
                CreateStageTwoItemSnapshot(
                    "cumulative-refresh-after",
                    refreshItemSession,
                    new[]
                    {
                        CreatePool15BattleItemRow(
                            "I015", refreshKeep, true, true),
                        CreatePool15BattleItemRow(
                            "I015", refreshRemove, false, null),
                        CreatePool15BattleItemRow(
                            "I025", refreshAdd, true, true)
                    });
            C1FormalRealtimeBattleItemRefreshResult refreshResult = null;
            C1FormalRealtimeBattleSessionStateSnapshot refreshPausedAfter =
                null;
            bool refreshed = reachedDue
                && refreshAdapter
                    .TryRefreshFromCumulativeItemSnapshotWhilePaused(
                        refreshAfter,
                        refreshItemSession,
                        "verifier.cumulative.refresh.command",
                        out refreshResult)
                && refreshResult.accepted
                && refreshAdapter.TryGetSnapshot(out refreshPausedAfter);
            bool refreshPreserved = refreshed
                && refreshPausedBefore.battleTimeMs
                    == refreshPausedAfter.battleTimeMs
                && refreshPausedBefore.playerSnapshot.currentHp
                    == refreshPausedAfter.playerSnapshot.currentHp
                && refreshPausedBefore.enemyApplicationCount
                    == refreshPausedAfter.enemyApplicationCount
                && refreshPausedBefore.actorSnapshots.Select(actor =>
                        actor.currentHp)
                    .SequenceEqual(refreshPausedAfter.actorSnapshots.Select(
                        actor => actor.currentHp))
                && refreshPausedAfter.activePool15SourceCount == 2
                && refreshAdapter.Session.CueLedger
                    .Take(refreshHistoryBefore.Length)
                    .Select(cue => cue.canonicalSignature)
                    .SequenceEqual(refreshHistoryBefore.Select(cue =>
                        cue.canonicalSignature));
            bool refreshDuplicate = refreshed
                && refreshAdapter
                    .TryRefreshFromCumulativeItemSnapshotWhilePaused(
                        refreshAfter,
                        refreshItemSession,
                        "verifier.cumulative.refresh.command",
                        out C1FormalRealtimeBattleItemRefreshResult
                            refreshDuplicateResult)
                && string.Equals(
                    refreshDuplicateResult.canonicalSignature,
                    refreshResult.canonicalSignature,
                    StringComparison.Ordinal);
            bool refreshConflict = refreshed
                && !refreshAdapter
                    .TryRefreshFromCumulativeItemSnapshotWhilePaused(
                        refreshBefore,
                        refreshItemSession,
                        "verifier.cumulative.refresh.command",
                        out C1FormalRealtimeBattleItemRefreshResult
                            refreshConflictResult)
                && string.Equals(
                    refreshConflictResult.errorCode,
                    C1FormalRealtimeBattleErrorCodes.RefreshCommandConflict,
                    StringComparison.Ordinal);
            bool noBurst = refreshPreserved
                && refreshAdapter.TryResume(out _)
                && refreshAdapter.TryAdvanceBy(1L, out _)
                && refreshAdapter.Session.Pool15ApplicationLedger.Count
                    >= refreshApplicationsBefore
                && refreshAdapter.Session.Pool15ApplicationLedger.All(row =>
                    row.resolveBattleTimeMs >=
                    refreshPausedBefore.battleTimeMs)
                && !refreshAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    string.Equals(
                        row.sourceItemInstanceId,
                        refreshRemove,
                        StringComparison.Ordinal));
            bool refreshFutureApplied = noBurst
                && AdvanceUntilPoolSourcesApplied(
                    refreshAdapter,
                    new[] { refreshKeep, refreshAdd },
                    20000L)
                && !refreshAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    string.Equals(
                        row.sourceItemInstanceId,
                        refreshRemove,
                        StringComparison.Ordinal));
            long resetGeneration = refreshAdapter.Reset();
            bool rebound = refreshAdapter.TryStartFromCumulativeItemSnapshot(
                CreateContext(
                    "cumulative-refresh-rebind",
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                    generation++),
                refreshAfter,
                refreshItemSession,
                "verifier.cumulative.session.refresh.rebind",
                "verifier.cumulative.token.refresh.rebind."
                + resetGeneration,
                out C1FormalRealtimeBattleSessionStartSnapshot reboundStart)
                && reboundStart.accepted;
            context.Check(
                "cumulative-paused-refresh-and-rebind",
                "live-refresh",
                refreshPreserved
                && refreshDuplicate
                && refreshConflict
                && noBurst
                && refreshFutureApplied
                && rebound,
                "future sources replace atomically; no restart/burst/history rewrite; reset rebind remains valid",
                "preserved=" + refreshPreserved + ";duplicate="
                + refreshDuplicate + ";conflict=" + refreshConflict
                + ";noBurst=" + noBurst + ";future="
                + refreshFutureApplied + ";rebind=" + rebound,
                "CUMULATIVE_REFRESH_FAILED");
            refreshAdapter.Unbind();

            bool stage13Started = TryStartCumulativeItemSnapshot(
                "stage13-zero",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                generation++,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter stage13Adapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot stage13Start);
            bool stage15Started = TryStartCumulativeItemSnapshot(
                "stage15-zero",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                generation++,
                Array.Empty<C1FormalItemBattleItemRow>(),
                out C1FormalRealtimeBattleSessionAdapter stage15Adapter,
                out _,
                out C1FormalRealtimeBattleSessionStartSnapshot stage15Start);
            bool encounterActors = stage13Started
                && stage15Started
                && stage13Start.stateSnapshot.actorSnapshots.Count
                    == C1Stage3To5PlaytestEnemyEncounterCatalog
                        .FindByStageId(
                            C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3)
                        .Actors.Count
                && stage15Start.stateSnapshot.actorSnapshots.Count
                    == C1Stage3To5PlaytestEnemyEncounterCatalog
                        .FindByStageId(
                            C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5)
                        .Actors.Count;
            stage13Adapter.Unbind();
            stage15Adapter.Unbind();

            bool boneStarted = TryStartCumulativeItemSnapshot(
                "bone-wave-fixed-i001",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                generation++,
                new[]
                {
                    CreateBattleItemRow(
                        "I001",
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                        InitialVerifierInstanceSignature,
                        true,
                        true)
                },
                out C1FormalRealtimeBattleSessionAdapter boneAdapter,
                out _,
                out _);
            bool beforeWave = boneStarted
                && boneAdapter.TryAdvanceTo(4499L, out var beforeWaveTick)
                && beforeWaveTick.stateSnapshot.playerSnapshot.currentHp == 100
                && beforeWaveTick.stateSnapshot.enemyApplicationCount == 0;
            bool firstWave = beforeWave
                && boneAdapter.TryAdvanceTo(4500L, out var firstWaveTick)
                && firstWaveTick.stateSnapshot.playerSnapshot.currentHp == 93
                && firstWaveTick.stateSnapshot.enemyApplicationCount == 1;
            int terminalGuard = 0;
            while (firstWave
                   && !boneAdapter.IsTerminal
                   && terminalGuard++ < 40)
            {
                if (!boneAdapter.TryAdvanceBy(4500L, out _)) break;
            }
            C1FormalRealtimeBattleCue firstTelegraph = boneAdapter.Session
                .CueLedger.FirstOrDefault(cue => string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.BoneSwapTelegraphed,
                    StringComparison.Ordinal));
            C1FormalRealtimeBattleCue firstReturn = firstTelegraph == null
                ? null
                : boneAdapter.Session.CueLedger.FirstOrDefault(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.BoneSwapReturnAccepted,
                        StringComparison.Ordinal));
            C1Stage3To5BoneSwapReleaseRow boneRelease =
                C1Stage3To5PlaytestEnemyEncounterCatalog.BoneSwapRelease;
            bool boneAndWave = encounterActors
                && firstWave
                && boneAdapter.Session.TerminalResult != null
                && boneAdapter.Session.TerminalResult.accepted
                && firstTelegraph != null
                && firstReturn != null
                && firstReturn.battleTimeMs - firstTelegraph.battleTimeMs
                    == boneRelease.TelegraphMilliseconds
                && firstReturn.requestedDamage > 0
                && firstReturn.requestedDamage <= boneRelease.CapDamage
                && string.IsNullOrEmpty(firstTelegraph.sourceItemInstanceId)
                && string.IsNullOrEmpty(firstReturn.sourceItemInstanceId);
            context.Check(
                "cumulative-stage13to15-enemy-boneswap-regression",
                "encounter-regression",
                boneAndWave,
                "catalog actors, 4500ms/7 wave, live defeat, and BoneSwap timing remain authoritative",
                "actors=" + encounterActors + ";wave=" + firstWave
                + ";terminal=" + (boneAdapter.Session.TerminalResult != null)
                + ";bone=" + (firstTelegraph != null && firstReturn != null),
                "CUMULATIVE_ENCOUNTER_REGRESSION");
            boneAdapter.Unbind();
        }

        private static void VerifyAutomaticGrowingPool15Runtime(
            VerificationContext context)
        {
            C1CampaignLootEligibleItemPool15Snapshot pool =
                C1CampaignLootEligibleItemPool15Carrier.Resolve().snapshot;
            string[] roster = pool.Profiles.Select(row => row.baseItemId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            long generation = 6200L;
            bool matrixPassed = roster.Length ==
                C1FormalRealtimeBattleSessionContract.MaxPool15ActiveSourceCount;
            for (int count = 0;
                 count <= C1FormalRealtimeBattleSessionContract
                     .MaxPool15ActiveSourceCount;
                 count++)
            {
                bool started = TryStartPool15RowSet(
                    "capacity-" + count,
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    CreateBoundedActivePool15Rows(
                        "capacity-" + count,
                        roster.Take(count)),
                    generation++,
                    out C1FormalRealtimeBattleSessionAdapter adapter,
                    out C1FormalRealtimeBattleSessionStartSnapshot start);
                bool ticked = started
                    && adapter.TryAdvanceBy(
                        1L,
                        out C1FormalRealtimeBattleTickResult tick)
                    && tick != null
                    && tick.accepted;
                matrixPassed &= ticked
                    && start?.stateSnapshot != null
                    && start.stateSnapshot.activePool15SourceCount == count;
                adapter?.Unbind();
            }
            context.Check(
                "pool15-growing-capacity-0-through-15",
                "runtime-integration",
                matrixPassed,
                "0..15 active ItemInstances start and advance",
                matrixPassed ? "PASS" : "FAIL",
                "POOL15_GROWING_CAPACITY_FAILED");

            string[] fiveFamilySources =
            {
                "I007", "I004", "I015", "I025", "I010"
            };
            C1FormalRealtimeBattleTerminalResult zeroTerminal =
                RunAutomaticPool15Terminal(
                    "auto-zero-stage14",
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    Array.Empty<C1FormalRealtimeBattlePool15ItemRow>(),
                    generation++,
                    out C1FormalRealtimeBattleSessionAdapter zeroAdapter,
                    out bool zeroStarted);
            C1FormalRealtimeBattleTerminalResult fiveTerminal =
                RunAutomaticPool15Terminal(
                    "auto-five-stage14",
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    CreateBoundedActivePool15Rows(
                        "auto-five-stage14",
                        fiveFamilySources),
                    generation++,
                    out C1FormalRealtimeBattleSessionAdapter fiveAdapter,
                    out bool fiveStarted);
            C1FormalRealtimeBattleEffectApplicationSnapshot[] applications =
                fiveAdapter.Session.Pool15ApplicationLedger.ToArray();
            HashSet<string> families = new HashSet<string>(
                applications.Select(row => row.familyId),
                StringComparer.Ordinal);
            string[] expectedFamilies =
            {
                "DIRECT_TEMPO", "TARGET_SPREAD", "GUARD_SUSTAIN",
                "ENEMY_CONTROL", "POSITIONAL_ADJACENCY"
            };
            bool fiveFamilies = fiveStarted
                && applications.Length >= 5
                && expectedFamilies.All(families.Contains)
                && applications.All(row =>
                    !string.IsNullOrEmpty(row.sourceItemInstanceId)
                    && !string.IsNullOrEmpty(row.sourceFactCanonicalSignature)
                    && !string.IsNullOrEmpty(row.triggerId)
                    && !string.IsNullOrEmpty(row.targetIdentity)
                    && !string.IsNullOrEmpty(row.cueIdentity))
                && fiveAdapter.Session.Pool15CueLedger.Count ==
                applications.Length
                && fiveAdapter.Session.CueLedger.Any(cue => string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                    StringComparison.Ordinal));
            context.Check(
                "pool15-normal-tick-five-family-no-manual-injection",
                "runtime-integration",
                fiveFamilies,
                "normal Tick/Advance produces 5/5 source/trigger/effect/target/cue truth",
                "apps=" + applications.Length + ";families="
                + string.Join(",", families.OrderBy(
                    value => value,
                    StringComparer.Ordinal)),
                "POOL15_AUTOMATIC_FIVE_FAMILY_FAILED");
            bool materialTerminal = zeroStarted
                && zeroTerminal != null
                && fiveTerminal != null
                && !string.Equals(
                    zeroTerminal.canonicalSignature,
                    fiveTerminal.canonicalSignature,
                    StringComparison.Ordinal)
                && (zeroTerminal.playerSnapshot.currentHp
                    != fiveTerminal.playerSnapshot.currentHp
                    || zeroTerminal.durationMs != fiveTerminal.durationMs
                    || zeroTerminal.win != fiveTerminal.win);
            context.Check(
                "pool15-active-effects-change-live-terminal",
                "runtime-integration",
                materialTerminal,
                "five-family terminal materially differs from zero-active",
                zeroTerminal == null || fiveTerminal == null
                    ? "missing"
                    : "zero=" + zeroTerminal.durationMs + "/"
                      + zeroTerminal.playerSnapshot.currentHp + ";active="
                      + fiveTerminal.durationMs + "/"
                      + fiveTerminal.playerSnapshot.currentHp,
                "POOL15_TERMINAL_NOT_MATERIAL");
            zeroAdapter?.Unbind();
            fiveAdapter?.Unbind();

            List<C1FormalRealtimeBattlePool15ItemRow> duplicateRows =
                CreateBoundedActivePool15Rows(
                    "duplicate-base-live",
                    new[] { "I015", "I015" });
            bool duplicateStarted = TryStartPool15RowSet(
                "duplicate-base-live",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                duplicateRows,
                generation++,
                out C1FormalRealtimeBattleSessionAdapter duplicateAdapter,
                out C1FormalRealtimeBattleSessionStartSnapshot duplicateStart);
            bool duplicateAdvanced = duplicateStarted
                && duplicateAdapter.TryAdvanceTo(
                    9000L,
                    out C1FormalRealtimeBattleTickResult duplicateTick)
                && duplicateTick != null
                && duplicateTick.accepted;
            C1FormalRealtimeBattleEffectApplicationSnapshot[] duplicateApps =
                duplicateAdapter.Session.Pool15ApplicationLedger
                    .Where(row => string.Equals(
                        row.sourceBaseItemId,
                        "I015",
                        StringComparison.Ordinal))
                    .ToArray();
            C1FormalRealtimeBattleCue[] duplicateOrderedCues =
                duplicateAdapter.Session.CueLedger.Where(cue =>
                        string.Equals(
                            cue.cueKind,
                            C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                            StringComparison.Ordinal)
                        && string.Equals(
                            cue.sourceBaseItemId,
                            "I015",
                            StringComparison.Ordinal))
                    .ToArray();
            bool canonicalUsesItemInstance = false;
            if (duplicateOrderedCues.Length == 2)
            {
                C1FormalRealtimeBattleCue sourceCue = duplicateOrderedCues[0];
                C1FormalRealtimeBattleCue alternateInstanceCue =
                    new C1FormalRealtimeBattleCue(
                        sourceCue.cueId,
                        sourceCue.sequence,
                        sourceCue.battleTimeMs,
                        sourceCue.cueKind,
                        sourceCue.sourceOwner,
                        sourceCue.sourceId,
                        sourceCue.targetActorId,
                        sourceCue.targetStableOrder,
                        sourceCue.playerHpBefore,
                        sourceCue.playerHpAfter,
                        sourceCue.actorHpBefore,
                        sourceCue.actorHpAfter,
                        sourceCue.requestedDamage,
                        sourceCue.appliedDamage,
                        sourceCue.floatPayload,
                        sourceCue.presentationPriority,
                        sourceCue.acceptedApplicationEventId,
                        duplicateOrderedCues[1].sourceItemInstanceId,
                        sourceCue.sourceBaseItemId,
                        sourceCue.effectFamilyId,
                        sourceCue.effectVariantId);
                canonicalUsesItemInstance = !string.Equals(
                    sourceCue.canonicalSignature,
                    alternateInstanceCue.canonicalSignature,
                    StringComparison.Ordinal);
            }
            bool duplicateSeparate = duplicateAdvanced
                && duplicateStart.stateSnapshot.activePool15SourceCount == 2
                && duplicateApps.Length == 2
                && duplicateApps.Select(row => row.sourceItemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() == 2
                && duplicateAdapter.Session.Pool15CueLedger
                    .Select(row => row.sourceItemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() == 2
                && duplicateOrderedCues.Length == 2
                && duplicateOrderedCues.All(cue =>
                    !string.IsNullOrEmpty(cue.sourceItemInstanceId)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        "I015",
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.effectFamilyId,
                        "GUARD_SUSTAIN",
                        StringComparison.Ordinal)
                    && !string.IsNullOrEmpty(cue.effectVariantId))
                && duplicateOrderedCues.Select(cue => cue.sourceItemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() == 2
                && duplicateOrderedCues.Select(cue => cue.canonicalSignature)
                    .Distinct(StringComparer.Ordinal).Count() == 2
                && canonicalUsesItemInstance;
            context.Check(
                "pool15-duplicate-base-distinct-instance-ledgers",
                "source-correlation",
                duplicateSeparate,
                "two I015 ItemInstances keep separate effects and cues",
                "apps=" + duplicateApps.Length,
                "POOL15_BASE_IDENTITY_COLLAPSE");
            duplicateAdapter?.Unbind();

            C1FormalRealtimeBattleSessionRequest staleFactBase =
                BuildStage3To5DirectRequest(
                    "stale-pool15-source-fact",
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    generation++,
                    0L);
            C1CampaignLootItemProfileSnapshot staleDescriptor =
                pool.FindProfile("I015");
            C1CampaignLootPool15BattleCandidateProfileSnapshot candidateSet =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
                    .Resolve();
            C1CampaignLootPool15BattleCandidateProfile staleCandidate =
                candidateSet.Find("I015");
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot staleFact =
                C1FormalRealtimeBattlePool15ActiveItemFactSnapshot
                    .FromImmutableSourceFact(
                    "verifier.pool15.stale.instance",
                    "I015",
                    staleDescriptor.profileCanonicalSignature,
                    staleCandidate.canonicalSignature,
                    "stale-source-fact-canonical");
            C1FormalRealtimeBattleSessionRequest staleFactRequest =
                CopyPool15ActiveFacts(staleFactBase, new[] { staleFact });
            C1FormalRealtimeBattleSession staleFactSession =
                new C1FormalRealtimeBattleSession();
            bool staleFactRejected = !staleFactSession.TryStart(
                    staleFactRequest,
                    out C1FormalRealtimeBattleSessionStartSnapshot staleStart)
                && string.Equals(
                    staleStart?.errorCode,
                    C1FormalRealtimeBattleErrorCodes.Pool15SnapshotRejected,
                    StringComparison.Ordinal);
            context.Check(
                "pool15-stale-source-fact-canonical-rejected",
                "source-correlation",
                staleFactRejected,
                "stale placed+lit source fact rejects atomically",
                staleStart?.errorCode ?? "missing",
                "POOL15_STALE_SOURCE_FACT_ACCEPTED");

            string refreshScenario = "paused-pool15-refresh";
            string refreshItemSessionCanonical =
                "verifier.pool15.item-session." + refreshScenario;
            C1FormalItemBattleInputSnapshot refreshFormalSnapshot =
                CreateStageTwoItemSnapshot(
                    refreshScenario,
                    refreshItemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true)
                    });
            List<C1FormalRealtimeBattlePool15ItemRow> refreshInitialRows =
                CreateBoundedActivePool15Rows(
                    refreshScenario,
                    new[] { "I015", "I015" });
            C1FormalRealtimeBattlePool15ItemSnapshot refreshInitialPool =
                CreatePool15SnapshotFromRows(
                    refreshScenario + ".initial",
                    refreshInitialRows);
            C1FormalRealtimeBattleSessionAdapter refreshAdapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool refreshStarted = refreshAdapter.TryStart(
                CreateContext(
                    refreshScenario,
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    generation++),
                refreshFormalSnapshot,
                refreshItemSessionCanonical,
                "verifier.pool15.session." + refreshScenario,
                "verifier.pool15.token." + refreshScenario,
                refreshInitialPool,
                out C1FormalRealtimeBattleSessionStartSnapshot refreshStart);
            long firstItemDue = refreshStarted
                ? refreshStart.stateSnapshot.scheduledActions
                    .Where(action => !action.executed
                        && string.Equals(
                            action.actionKind,
                            C1FormalRealtimeBattleScheduledActionKinds
                                .ItemTrigger,
                            StringComparison.Ordinal))
                    .Select(action => action.dueBattleTimeMs)
                    .DefaultIfEmpty(-1L)
                    .Min()
                : -1L;
            bool reachedTrigger = firstItemDue > 0L
                && refreshAdapter.TryAdvanceTo(
                    firstItemDue,
                    out C1FormalRealtimeBattleTickResult refreshTriggerTick)
                && refreshTriggerTick.accepted;
            int poolLedgerBeforeRefresh = refreshAdapter.Session
                .Pool15ApplicationLedger.Count;
            bool pendingStagger = reachedTrigger
                && refreshAdapter.TryGetSnapshot(
                    out C1FormalRealtimeBattleSessionStateSnapshot
                        beforePoolRefresh)
                && beforePoolRefresh.scheduledActions.Any(action =>
                    !action.executed
                    && string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds
                            .Pool15EffectResolve,
                        StringComparison.Ordinal)
                    && string.Equals(
                        action.sourceId,
                        refreshInitialRows[1].itemInstanceId,
                        StringComparison.Ordinal));
            bool refreshPaused = pendingStagger
                && refreshAdapter.TryPause(out _);
            refreshAdapter.TryGetSnapshot(
                out C1FormalRealtimeBattleSessionStateSnapshot pausedBefore);
            List<C1FormalRealtimeBattlePool15ItemRow> refreshReplacementRows =
                CreateBoundedActivePool15Rows(
                    refreshScenario,
                    new[] { "I015", "I025" });
            C1FormalRealtimeBattlePool15ItemSnapshot refreshReplacementPool =
                CreatePool15SnapshotFromRows(
                    refreshScenario + ".replacement",
                    refreshReplacementRows);
            C1FormalRealtimeBattleItemRefreshResult poolRefreshResult = null;
            bool poolRefreshed = refreshPaused
                && refreshAdapter.TryRefreshItemsWhilePaused(
                    refreshFormalSnapshot,
                    refreshItemSessionCanonical,
                    refreshReplacementPool,
                    "verifier.pool15.refresh.command",
                    out poolRefreshResult)
                && poolRefreshResult.accepted;
            bool refreshDuplicate = poolRefreshed
                && refreshAdapter.TryRefreshItemsWhilePaused(
                    refreshFormalSnapshot,
                    refreshItemSessionCanonical,
                    refreshReplacementPool,
                    "verifier.pool15.refresh.command",
                    out C1FormalRealtimeBattleItemRefreshResult
                        poolRefreshDuplicate)
                && string.Equals(
                    poolRefreshResult.canonicalSignature,
                    poolRefreshDuplicate.canonicalSignature,
                    StringComparison.Ordinal);
            bool refreshConflict = poolRefreshed
                && !refreshAdapter.TryRefreshItemsWhilePaused(
                    refreshFormalSnapshot,
                    refreshItemSessionCanonical,
                    refreshInitialPool,
                    "verifier.pool15.refresh.command",
                    out C1FormalRealtimeBattleItemRefreshResult
                        poolRefreshConflict)
                && string.Equals(
                    poolRefreshConflict.errorCode,
                    C1FormalRealtimeBattleErrorCodes.RefreshCommandConflict,
                    StringComparison.Ordinal);
            refreshAdapter.TryGetSnapshot(
                out C1FormalRealtimeBattleSessionStateSnapshot pausedAfter);
            bool canceledOldFuture = poolRefreshed
                && pausedBefore.battleTimeMs == pausedAfter.battleTimeMs
                && pausedBefore.activePool15SourceCount == 2
                && pausedAfter.activePool15SourceCount == 2
                && !string.Equals(
                    pausedBefore.activePool15SourceCanonicalSignature,
                    pausedAfter.activePool15SourceCanonicalSignature,
                    StringComparison.Ordinal)
                && pausedAfter.scheduledActions.Any(action =>
                    action.executed
                    && !action.accepted
                    && string.Equals(
                        action.diagnosticCode,
                        "POOL15_REFRESH_CANCELED_FUTURE_EFFECT",
                        StringComparison.Ordinal));
            bool refreshResumed = canceledOldFuture
                && refreshAdapter.TryResume(out _);
            C1FormalRealtimeBattleTickResult refreshNoBurstTick = null;
            bool noPastDueBurst = refreshResumed
                && refreshAdapter.TryAdvanceBy(
                    1L,
                    out refreshNoBurstTick)
                && refreshNoBurstTick.accepted
                && refreshAdapter.Session.Pool15ApplicationLedger.Count
                    == poolLedgerBeforeRefresh;
            bool futureRemapped = noPastDueBurst
                && refreshAdapter.TryAdvanceTo(
                    firstItemDue + 5000L,
                    out C1FormalRealtimeBattleTickResult refreshFutureTick)
                && refreshFutureTick.accepted
                && refreshAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    string.Equals(
                        row.sourceBaseItemId,
                        "I025",
                        StringComparison.Ordinal)
                    && string.Equals(
                        row.sourceItemInstanceId,
                        refreshReplacementRows[1].itemInstanceId,
                        StringComparison.Ordinal))
                && !refreshAdapter.Session.Pool15ApplicationLedger.Any(row =>
                    string.Equals(
                        row.sourceBaseItemId,
                        "I015",
                        StringComparison.Ordinal)
                    && string.Equals(
                        row.sourceItemInstanceId,
                        refreshInitialRows[1].itemInstanceId,
                        StringComparison.Ordinal))
                && refreshAdapter.Session.CueLedger.Any(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.Pool15EffectAccepted,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceItemInstanceId,
                        refreshReplacementRows[1].itemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        "I025",
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.effectFamilyId,
                        "ENEMY_CONTROL",
                        StringComparison.Ordinal)
                    && !string.IsNullOrEmpty(cue.effectVariantId))
                && refreshAdapter.Session.CueLedger.Any(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.ActionProgressMutated,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceItemInstanceId,
                        refreshReplacementRows[1].itemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        "I025",
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.effectFamilyId,
                        "ENEMY_CONTROL",
                        StringComparison.Ordinal))
                && !poolRefreshResult.emittedCues.Any(cue => string.Equals(
                    cue.sourceItemInstanceId,
                    refreshInitialRows[1].itemInstanceId,
                    StringComparison.Ordinal))
                && !refreshNoBurstTick.emittedCues.Any(cue => string.Equals(
                    cue.sourceItemInstanceId,
                    refreshInitialRows[1].itemInstanceId,
                    StringComparison.Ordinal));
            context.Check(
                "pool15-paused-refresh-cancels-old-future-and-remaps",
                "runtime-integration",
                poolRefreshed
                && refreshDuplicate
                && refreshConflict
                && canceledOldFuture
                && noPastDueBurst
                && futureRemapped,
                "paused tick/history retained; old future canceled; no burst; replacement joins a later trigger",
                "started=" + refreshStarted + ";pending=" + pendingStagger
                + ";refresh=" + poolRefreshed + ";duplicate="
                 + refreshDuplicate + ";conflict=" + refreshConflict
                 + ";canceled=" + canceledOldFuture + ";noBurst="
                 + noPastDueBurst + ";future=" + futureRemapped
                 + ";ledger=" + string.Join(",", refreshAdapter.Session
                     .Pool15ApplicationLedger.Select(row =>
                         row.sourceBaseItemId + "@" + row.sourceItemInstanceId))
                 + ";diagnostics=" + string.Join(",", pausedAfter
                     .scheduledActions.Where(action => action.executed
                         && !action.accepted).Select(action =>
                         action.diagnosticCode + "@" + action.sourceId)),
                "POOL15_PAUSED_REFRESH_FAILED");
            refreshAdapter.Unbind();

            bool stage15Started = TryStartPool15RowSet(
                "accumulated-five-stage15",
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                CreateBoundedActivePool15Rows(
                    "accumulated-five-stage15",
                    fiveFamilySources),
                generation++,
                out C1FormalRealtimeBattleSessionAdapter stage15Adapter,
                out C1FormalRealtimeBattleSessionStartSnapshot stage15Start);
            bool stage15Ticked = stage15Started
                && stage15Adapter.TryAdvanceBy(
                    1L,
                    out C1FormalRealtimeBattleTickResult stage15Tick)
                && stage15Tick.accepted;
            context.Check(
                "pool15-five-active-stage14-stage15-admission",
                "runtime-integration",
                fiveStarted && stage15Ticked
                && stage15Start.stateSnapshot.activePool15SourceCount == 5,
                "five simultaneous placed+lit sources start 1-4 and 1-5",
                "stage14=" + fiveStarted + ";stage15=" + stage15Ticked,
                "POOL15_ACCUMULATED_STAGE_ADMISSION_FAILED");
            stage15Adapter?.Unbind();

            List<C1FormalRealtimeBattlePool15ItemRow> i002Rows =
                CreateBoundedActivePool15Rows(
                    "i002-separate",
                    new[] { "I002" });
            bool i002Started = TryStartPool15RowSet(
                "i002-separate",
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                i002Rows,
                generation++,
                out C1FormalRealtimeBattleSessionAdapter i002Adapter,
                out C1FormalRealtimeBattleSessionStartSnapshot i002Start);
            bool i002Advanced = i002Started
                && i002Adapter.TryAdvanceTo(
                    17000L,
                    out C1FormalRealtimeBattleTickResult i002Tick)
                && i002Tick.accepted;
            C1FormalRealtimeBattleEffectApplicationSnapshot refund =
                i002Adapter.Session.Pool15ApplicationLedger.FirstOrDefault(row =>
                    string.Equals(
                        row.sourceBaseItemId,
                        "I002",
                        StringComparison.Ordinal));
            bool i002Separate = i002Advanced
                && refund != null
                && !string.Equals(
                    refund.operationId,
                    "Damage",
                    StringComparison.OrdinalIgnoreCase)
                && !i002Adapter.Session.CueLedger.Any(cue =>
                    string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.ActorHpChanged,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceId,
                        i002Rows[0].itemInstanceId,
                        StringComparison.Ordinal));
            context.Check(
                "pool15-i002-refund-separate-from-direct-damage",
                "source-correlation",
                i002Separate,
                "I002 Pool15 Refund has no actor-HP damage cue",
                refund == null ? "missing" : refund.operationId,
                "POOL15_I002_DAMAGE_DUPLICATED");
            i002Adapter?.Unbind();
        }

        private static List<C1FormalRealtimeBattlePool15ItemRow>
            CreateBoundedActivePool15Rows(
                string suffix,
                IEnumerable<string> baseItemIds)
        {
            C1CampaignLootEligibleItemPool15Snapshot pool =
                C1CampaignLootEligibleItemPool15Carrier.Resolve().snapshot;
            C1CampaignLootPool15BattleCandidateProfileSnapshot candidates =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            List<C1FormalRealtimeBattlePool15ItemRow> rows =
                new List<C1FormalRealtimeBattlePool15ItemRow>();
            int ordinal = 0;
            foreach (string baseItemId in baseItemIds ?? Array.Empty<string>())
            {
                C1CampaignLootItemProfileSnapshot descriptor =
                    pool.FindProfile(baseItemId);
                C1CampaignLootPool15BattleCandidateProfile candidate =
                    candidates.Find(baseItemId);
                rows.Add(new C1FormalRealtimeBattlePool15ItemRow(
                    "verifier.pool15." + suffix + ".instance."
                    + ordinal.ToString("D2", CultureInfo.InvariantCulture),
                    baseItemId,
                    true,
                    true,
                    descriptor?.profileCanonicalSignature ?? string.Empty,
                    candidate?.canonicalSignature ?? string.Empty));
                ordinal++;
            }
            return rows;
        }

        private static C1FormalRealtimeBattleTerminalResult
            RunAutomaticPool15Terminal(
                string scenario,
                string stageId,
                IEnumerable<C1FormalRealtimeBattlePool15ItemRow> rows,
                long generation,
                out C1FormalRealtimeBattleSessionAdapter adapter,
                out bool started)
        {
            started = TryStartPool15RowSet(
                scenario,
                stageId,
                rows,
                generation,
                out adapter,
                out C1FormalRealtimeBattleSessionStartSnapshot start)
                && start != null
                && start.accepted;
            int guard = 0;
            while (started && !adapter.IsTerminal && guard++ < 500)
            {
                if (!adapter.TryAdvanceBy(
                        500L,
                        out C1FormalRealtimeBattleTickResult tick)
                    || tick == null
                    || !tick.accepted)
                {
                    break;
                }
            }
            return adapter.Session.TerminalResult;
        }

        private static void VerifyPool15ActiveFiltering(
            VerificationContext context,
            int generation)
        {
            Pool15ScenarioEvidence weak = StartPool15Scenario(
                "filter-weak",
                Array.Empty<string>(),
                new[] { "I004" },
                generation++);
            Pool15ScenarioEvidence middle = StartPool15Scenario(
                "filter-middle",
                new[] { "I002" },
                new[] { "I004" },
                generation++);
            Pool15ScenarioEvidence target = StartPool15Scenario(
                "filter-target",
                new[] { "I004", "I015" },
                new[] { "I002" },
                generation++);
            Pool15ScenarioEvidence[] rows = { weak, middle, target };
            int[] expected = { 0, 1, 2 };
            for (int index = 0; index < rows.Length; index++)
            {
                Pool15ScenarioEvidence row = rows[index];
                int actual = row.Start == null
                    || row.Start.stateSnapshot == null
                        ? -1
                        : row.Start.stateSnapshot.activePool15SourceCount;
                bool passed = row.Started && actual == expected[index];
                context.Filtering.Add(new Pool15FilteringEvidence(
                    row.Scenario,
                    expected[index],
                    actual,
                    passed));
                context.Check(
                    "pool15-active-filter-" + row.Scenario,
                    "formal-pool15-filter",
                    passed,
                    expected[index].ToString(CultureInfo.InvariantCulture),
                    actual.ToString(CultureInfo.InvariantCulture),
                    "POOL15_ACTIVE_FILTER_FAILED");
                row.Adapter.Unbind();
            }

            Pool15ScenarioEvidence liveTerminal = StartPool15Scenario(
                "filter-live-terminal-no-i002",
                new[] { "I013" },
                new[] { "I004" },
                generation++);
            C1CampaignLootItemProfileSnapshot liveDescriptor =
                C1CampaignLootEligibleItemPool15Carrier.Resolve().snapshot
                    .FindProfile("I013");
            bool liveEffectApplied = liveTerminal.Started
                && liveDescriptor != null
                && liveTerminal.Adapter.TryExecutePool15Event(
                    "verifier.pool15.event.live-terminal-no-i002",
                    liveDescriptor.triggerId,
                    new[] { liveDescriptor.conditionId },
                    out C1FormalRealtimeBattlePool15EventResult liveEffectResult)
                && liveEffectResult != null
                && liveEffectResult.accepted
                && liveEffectResult.acceptedSourceCount == 1;
            C1FormalRealtimeBattleTerminalResult terminal = null;
            int liveGuard = 0;
            while (liveEffectApplied
                   && !liveTerminal.Adapter.IsTerminal
                   && liveGuard++ < 400)
            {
                if (!liveTerminal.Adapter.TryAdvanceBy(
                        500L,
                        out C1FormalRealtimeBattleTickResult tick))
                {
                    break;
                }

                terminal = tick.terminalResult ?? terminal;
            }

            bool noFixedI002Residue = liveTerminal.Start != null
                && liveTerminal.Start.stateSnapshot != null
                && liveTerminal.Start.stateSnapshot.scheduledActions.All(action =>
                    !string.Equals(
                        action.sourceId,
                        "I002@white",
                        StringComparison.Ordinal))
                && liveTerminal.Start.initialCues.All(cue => !string.Equals(
                    cue.sourceId,
                    "I002@white",
                    StringComparison.Ordinal));
            bool liveTerminalAccepted = liveEffectApplied
                && liveTerminal.Start.stateSnapshot.activePool15SourceCount == 1
                && terminal != null
                && terminal.accepted
                && terminal.win
                && !terminal.lose
                && !terminal.shouldWriteSave
                && !terminal.shouldGrantReward
                && terminal.battleResultSnapshot != null
                && !terminal.battleResultSnapshot.shouldWriteSave
                && !terminal.battleResultSnapshot.shouldGrantReward
                && noFixedI002Residue;
            context.Check(
                "pool15-no-i002-live-terminal-authoritative",
                "formal-pool15-filter",
                liveTerminalAccepted,
                "I001 + I013 active + I004 unlit -> accepted live victory; no I002/persistence",
                terminal == null
                    ? "missing terminal"
                    : "duration=" + terminal.durationMs
                      + ";active=" + liveTerminal.Start.stateSnapshot
                          .activePool15SourceCount,
                "POOL15_NO_I002_LIVE_TERMINAL_FAILED");
            liveTerminal.Adapter.Unbind();
        }

        private static void VerifyPool15SourceOrderAndSensitivity(
            VerificationContext context,
            int generation)
        {
            foreach (int outerTickMs in new[] { 96, 120, 144 })
            {
                foreach (KeyValuePair<string, string[]> layout in new[]
                         {
                             new KeyValuePair<string, string[]>(
                                 "weak",
                                 Array.Empty<string>()),
                             new KeyValuePair<string, string[]>(
                                 "middle",
                                 new[] { "I004" }),
                             new KeyValuePair<string, string[]>(
                                 "target",
                                 new[] { "I004", "I015" })
                         })
                {
                    Pool15ScenarioEvidence scenario = StartPool15Scenario(
                        "sensitivity-" + layout.Key + "-" + outerTickMs,
                        layout.Value,
                        Array.Empty<string>(),
                        generation++);
                    bool accepted = scenario.Started
                        && scenario.Adapter.TryExecutePool15Event(
                            "verifier.sensitivity." + layout.Key + "."
                            + outerTickMs,
                            "on_direct_lit",
                            new[] { "next_zhenlei_trigger", "is_direct_lit" },
                            out C1FormalRealtimeBattlePool15EventResult result)
                        && result != null
                        && result.accepted;
                    int guard = 0;
                    while (scenario.Adapter.Session.Pool15ApplicationLedger.Count
                           < layout.Value.Length
                           && guard++ < 4)
                    {
                        scenario.Adapter.TryAdvanceBy(outerTickMs, out _);
                    }

                    C1FormalRealtimeBattleEffectApplicationSnapshot[] ledger =
                        scenario.Adapter.Session.Pool15ApplicationLedger.ToArray();
                    string order = string.Join(">", ledger.Select(
                        row => row.sourceBaseItemId));
                    string times = string.Join(">", ledger.Select(row =>
                        row.resolveBattleTimeMs.ToString(
                            CultureInfo.InvariantCulture)));
                    bool passed = accepted
                        && ledger.Length == layout.Value.Length
                        && (layout.Value.Length < 2
                            || (order == "I004>I015"
                                && times == "0>120"))
                        && ledger.All(row => string.Equals(
                            row.candidateDataMaturity,
                            "CANDIDATE",
                            StringComparison.Ordinal));
                    context.Sensitivity.Add(new Pool15SensitivityEvidence(
                        layout.Key,
                        outerTickMs,
                        ledger.Length,
                        order,
                        times,
                        passed));
                    context.Check(
                        "pool15-sensitivity-" + layout.Key + "-" + outerTickMs,
                        "formal-pool15-timing",
                        passed,
                        layout.Value.Length + " source(s), stable order, 120ms source stagger",
                        ledger.Length + ";" + order + ";" + times,
                        "POOL15_TIMING_SENSITIVITY_FAILED");
                    scenario.Adapter.Unbind();
                }
            }
        }

        private static void VerifyPool15AtomicRejections(
            VerificationContext context,
            int generation)
        {
            Pool15ScenarioEvidence trigger = StartPool15Scenario(
                "reject-trigger",
                new[] { "I004" },
                Array.Empty<string>(),
                generation++);
            VerifyPool15EventRejection(
                context,
                trigger,
                "invalid-trigger",
                "unknown_trigger",
                new[] { "next_zhenlei_trigger" },
                C1FormalRealtimeBattleErrorCodes.Pool15EventTriggerRejected);

            Pool15ScenarioEvidence condition = StartPool15Scenario(
                "reject-condition",
                new[] { "I004" },
                Array.Empty<string>(),
                generation++);
            VerifyPool15EventRejection(
                context,
                condition,
                "missing-condition",
                "on_direct_lit",
                Array.Empty<string>(),
                C1FormalRealtimeBattleErrorCodes.Pool15EventConditionRejected);

            Pool15ScenarioEvidence duplicate = StartPool15Scenario(
                "reject-duplicate",
                new[] { "I010" },
                Array.Empty<string>(),
                generation++);
            bool first = duplicate.Started
                && duplicate.Adapter.TryExecutePool15Event(
                    "verifier.pool15.duplicate",
                    "on_lit",
                    new[] { "adjacent_lihuo" },
                    out _);
            int ledgerBefore = duplicate.Adapter.Session
                .Pool15ApplicationLedger.Count;
            duplicate.Adapter.TryGetSnapshot(out var duplicateBefore);
            bool second = !duplicate.Adapter.TryExecutePool15Event(
                "verifier.pool15.duplicate",
                "on_lit",
                new[] { "adjacent_lihuo" },
                out C1FormalRealtimeBattlePool15EventResult duplicateResult);
            duplicate.Adapter.TryGetSnapshot(out var duplicateAfter);
            bool duplicatePassed = first
                && second
                && duplicateResult != null
                && string.Equals(
                    duplicateResult.errorCode,
                    C1FormalRealtimeBattleErrorCodes.Pool15EventDuplicate,
                    StringComparison.Ordinal)
                && ledgerBefore == duplicate.Adapter.Session
                    .Pool15ApplicationLedger.Count
                && string.Equals(
                    duplicateBefore.canonicalSignature,
                    duplicateAfter.canonicalSignature,
                    StringComparison.Ordinal);
            AddPool15Rejection(
                context,
                "duplicate-event",
                duplicatePassed,
                duplicateResult == null
                    ? string.Empty
                    : duplicateResult.errorCode);
            duplicate.Adapter.Unbind();

            Pool15ScenarioEvidence overflow = StartPool15Scenario(
                "reject-overflow",
                new[] { "I015" },
                Array.Empty<string>(),
                generation++);
            FieldInfo playerGuardField = typeof(C1FormalRealtimeBattleSession)
                .GetField(
                    "playerGuard",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            playerGuardField?.SetValue(
                overflow.Adapter.Session,
                long.MaxValue);
            VerifyPool15EventRejection(
                context,
                overflow,
                "overflow-atomic",
                "on_direct_lit",
                new[] { "is_direct_lit" },
                C1FormalRealtimeBattleErrorCodes.Pool15AtomicityRejected);

            Pool15ScenarioEvidence target = StartPool15Scenario(
                "reject-target",
                new[] { "I004" },
                Array.Empty<string>(),
                generation++);
            ClearSessionActors(target.Adapter.Session);
            VerifyPool15EventRejection(
                context,
                target,
                "missing-target",
                "on_direct_lit",
                new[] { "next_zhenlei_trigger" },
                C1FormalRealtimeBattleErrorCodes.Pool15AtomicityRejected);

            Pool15ScenarioEvidence percentageBase = StartPool15Scenario(
                "reject-percentage-base",
                new[] { "I026" },
                Array.Empty<string>(),
                generation++);
            ClearSessionActors(percentageBase.Adapter.Session);
            VerifyPool15EventRejection(
                context,
                percentageBase,
                "missing-percentage-base",
                "on_slash",
                new[] { "target_has_flaw" },
                C1FormalRealtimeBattleErrorCodes.Pool15AtomicityRejected);

            bool staleUnitRejected = RejectTamperedPool15Snapshot(
                "stale-unit-profile",
                "I014",
                generation++,
                tamperDescriptor: false,
                tamperCandidate: true,
                out string staleUnitCode);
            AddPool15Rejection(
                context,
                "unit-conflict-or-stale-profile",
                staleUnitRejected,
                staleUnitCode);
            bool staleTargetRejected = RejectTamperedPool15Snapshot(
                "stale-target-descriptor",
                "I004",
                generation++,
                tamperDescriptor: true,
                tamperCandidate: false,
                out string staleTargetCode);
            AddPool15Rejection(
                context,
                "stale-target-descriptor",
                staleTargetRejected,
                staleTargetCode);

            List<C1FormalRealtimeBattlePool15ItemRow> missingRows =
                CreatePool15Rows(
                    Array.Empty<string>(),
                    Array.Empty<string>());
            missingRows.RemoveAt(missingRows.Count - 1);
            AddPool15RowSetAcceptance(
                context,
                "bounded-partial-entitlement-roster",
                missingRows,
                generation++,
                0);

            List<C1FormalRealtimeBattlePool15ItemRow> extraRows =
                CreatePool15Rows(
                    Array.Empty<string>(),
                    Array.Empty<string>());
            C1FormalRealtimeBattlePool15ItemRow firstRow = extraRows[0];
            extraRows.Add(new C1FormalRealtimeBattlePool15ItemRow(
                "verifier.pool15.instance.extra",
                firstRow.baseItemId,
                false,
                null,
                firstRow.descriptorCanonicalSignature,
                firstRow.candidateProfileCanonicalSignature));
            AddPool15RowSetRejection(
                context,
                "extra-entitlement-row",
                extraRows,
                generation++);

            List<C1FormalRealtimeBattlePool15ItemRow> duplicateRows =
                CreatePool15Rows(
                    Array.Empty<string>(),
                    Array.Empty<string>());
            C1FormalRealtimeBattlePool15ItemRow duplicateSource =
                duplicateRows[0];
            C1FormalRealtimeBattlePool15ItemRow replaced =
                duplicateRows[duplicateRows.Count - 1];
            duplicateRows[duplicateRows.Count - 1] =
                new C1FormalRealtimeBattlePool15ItemRow(
                    replaced.itemInstanceId,
                    duplicateSource.baseItemId,
                    false,
                    null,
                    duplicateSource.descriptorCanonicalSignature,
                    duplicateSource.candidateProfileCanonicalSignature);
            AddPool15RowSetAcceptance(
                context,
                "duplicate-base-distinct-instance-entitlement",
                duplicateRows,
                generation++,
                0);

            List<C1FormalRealtimeBattlePool15ItemRow> duplicateInstanceRows =
                CreatePool15Rows(
                    Array.Empty<string>(),
                    Array.Empty<string>());
            C1FormalRealtimeBattlePool15ItemRow firstInstance =
                duplicateInstanceRows[0];
            C1FormalRealtimeBattlePool15ItemRow lastInstance =
                duplicateInstanceRows[duplicateInstanceRows.Count - 1];
            duplicateInstanceRows[duplicateInstanceRows.Count - 1] =
                new C1FormalRealtimeBattlePool15ItemRow(
                    firstInstance.itemInstanceId,
                    lastInstance.baseItemId,
                    false,
                    null,
                    lastInstance.descriptorCanonicalSignature,
                    lastInstance.candidateProfileCanonicalSignature);
            AddPool15RowSetRejection(
                context,
                "duplicate-item-instance",
                duplicateInstanceRows,
                generation++);

            List<C1FormalRealtimeBattlePool15ItemRow> inconsistentRows =
                CreatePool15Rows(
                    Array.Empty<string>(),
                    Array.Empty<string>());
            C1FormalRealtimeBattlePool15ItemRow inconsistent =
                inconsistentRows[0];
            inconsistentRows[0] = new C1FormalRealtimeBattlePool15ItemRow(
                inconsistent.itemInstanceId,
                inconsistent.baseItemId,
                false,
                false,
                inconsistent.descriptorCanonicalSignature,
                inconsistent.candidateProfileCanonicalSignature);
            AddPool15RowSetRejection(
                context,
                "inconsistent-tray-lighting",
                inconsistentRows,
                generation++);
        }

        private static void AddPool15RowSetRejection(
            VerificationContext context,
            string scenario,
            IEnumerable<C1FormalRealtimeBattlePool15ItemRow> rows,
            long generation)
        {
            bool rejected = RejectPool15RowSet(
                scenario,
                rows,
                generation,
                out string errorCode);
            AddPool15Rejection(context, scenario, rejected, errorCode);
        }

        private static void AddPool15RowSetAcceptance(
            VerificationContext context,
            string scenario,
            IEnumerable<C1FormalRealtimeBattlePool15ItemRow> rows,
            long generation,
            int expectedActiveCount)
        {
            bool accepted = TryStartPool15RowSet(
                scenario,
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                rows,
                generation,
                out C1FormalRealtimeBattleSessionAdapter adapter,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            bool passed = accepted
                && start != null
                && start.accepted
                && start.stateSnapshot != null
                && start.stateSnapshot.activePool15SourceCount
                == expectedActiveCount;
            context.Check(
                "pool15-accept-" + scenario,
                "pool15-admission",
                passed,
                "accepted active=" + expectedActiveCount.ToString(
                    CultureInfo.InvariantCulture),
                start == null
                    ? "missing"
                    : start.errorCode + "/active="
                      + (start.stateSnapshot == null
                          ? "missing"
                          : start.stateSnapshot.activePool15SourceCount.ToString(
                              CultureInfo.InvariantCulture)),
                "POOL15_BOUNDED_ADMISSION_FAILED");
            adapter?.Unbind();
        }

        private static void VerifyPool15EventRejection(
            VerificationContext context,
            Pool15ScenarioEvidence scenario,
            string name,
            string triggerId,
            IEnumerable<string> conditionIds,
            string expectedCode)
        {
            scenario.Adapter.TryGetSnapshot(out var before);
            int ledgerBefore = scenario.Adapter.Session
                .Pool15ApplicationLedger.Count;
            int cuesBefore = scenario.Adapter.Session.Pool15CueLedger.Count;
            C1FormalRealtimeBattlePool15EventResult result = null;
            bool rejected = scenario.Started
                && !scenario.Adapter.TryExecutePool15Event(
                    "verifier.pool15.rejection." + name,
                    triggerId,
                    conditionIds,
                    out result)
                && result != null
                && string.Equals(
                    result.errorCode,
                    expectedCode,
                    StringComparison.Ordinal);
            scenario.Adapter.TryGetSnapshot(out var after);
            bool unchanged = before != null
                && after != null
                && string.Equals(
                    before.canonicalSignature,
                    after.canonicalSignature,
                    StringComparison.Ordinal)
                && ledgerBefore == scenario.Adapter.Session
                    .Pool15ApplicationLedger.Count
                && cuesBefore == scenario.Adapter.Session.Pool15CueLedger.Count;
            AddPool15Rejection(
                context,
                name,
                rejected && unchanged,
                result == null ? string.Empty : result.errorCode);
            scenario.Adapter.Unbind();
        }

        private static void AddPool15Rejection(
            VerificationContext context,
            string scenario,
            bool passed,
            string errorCode)
        {
            context.PoolRejections.Add(new RejectionEvidence(
                scenario,
                passed,
                errorCode));
            context.Check(
                "pool15-atomic-rejection-" + Slug(scenario),
                "formal-pool15-rejection",
                passed,
                "rejected with no state/ledger/cue mutation",
                errorCode,
                "POOL15_ATOMIC_REJECTION_FAILED");
        }

        private static void VerifyPool15FormalMappingShape(
            VerificationContext context)
        {
            string sessionSource = File.ReadAllText(Absolute(
                context,
                "Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs"));
            string[] familyMethods =
            {
                "TryMapDirectTempo(",
                "TryMapTargetSpread(",
                "TryMapGuardSustain(",
                "TryMapEnemyControl(",
                "TryMapPositionalAdjacency("
            };
            bool exactSharedPaths = familyMethods.All(method =>
                    CountOrdinal(sessionSource, method) == 2)
                && CountOrdinal(sessionSource, "case \"I") == 0
                && CountOrdinal(sessionSource, "turn * 1000") == 0
                && CountOrdinal(sessionSource, "1000 * turn") == 0
                && CountOrdinal(sessionSource, "turnToSeconds") == 0;
            context.Check(
                "pool15-five-shared-formal-family-paths",
                "formal-pool15-architecture",
                exactSharedPaths,
                "five family mapping methods; zero item-specific switches; no new turn conversion",
                exactSharedPaths ? "PASS" : "FAIL",
                "POOL15_FORMAL_PATH_PROLIFERATION");
        }

        private static AdmissionEvidence RunAdapterAdmission(
            string scenario,
            bool i001Placed,
            bool? i001Lit,
            bool i002Placed,
            bool? i002Lit,
            int expectedActiveCount,
            long generation,
            bool includeI002 = true)
        {
            string itemSessionCanonical = "verifier.item-session." + scenario;
            List<C1FormalItemBattleItemRow> itemRows =
                new List<C1FormalItemBattleItemRow>
                {
                    CreateBattleItemRow(
                        "I001",
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                        InitialVerifierInstanceSignature,
                        i001Placed,
                        i001Lit)
                };
            if (includeI002)
            {
                itemRows.Add(CreateBattleItemRow(
                    "I002",
                    RewardVerifierItemInstanceId,
                    RewardVerifierInstanceSignature,
                    i002Placed,
                    i002Lit));
            }

            C1FormalItemBattleInputSnapshot itemSnapshot =
                CreateStageTwoItemSnapshot(
                    scenario,
                    itemSessionCanonical,
                    itemRows);
            BattleLaunchContext launch = CreateContext(
                scenario,
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                generation);
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool started = adapter.TryStart(
                launch,
                itemSnapshot,
                itemSessionCanonical,
                "verifier.adapter.session." + scenario,
                "verifier.adapter.token." + scenario,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            AdmissionEvidence evidence = new AdmissionEvidence(
                scenario,
                expectedActiveCount,
                start);
            if (!started || start == null || !start.accepted
                || start.stateSnapshot == null)
            {
                adapter.Unbind();
                return evidence;
            }

            evidence.StartedNonTerminal = !start.stateSnapshot.terminal
                && start.stateSnapshot.battleTimeMs == 0L;
            evidence.ScheduledItemCount = start.stateSnapshot.scheduledActions.Count(
                action => !action.executed
                    && string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                        StringComparison.Ordinal));
            evidence.Cues.AddRange(start.initialCues);
            int guard = 0;
            while (!adapter.IsTerminal && guard++ < 300)
            {
                if (!adapter.TryAdvanceBy(
                        500L,
                        out C1FormalRealtimeBattleTickResult tick)
                    || tick == null)
                {
                    break;
                }

                evidence.TickCount++;
                evidence.Cues.AddRange(tick.emittedCues);
                if (tick.terminalResult != null)
                {
                    evidence.Terminal = tick.terminalResult;
                    break;
                }
            }

            adapter.Unbind();
            return evidence;
        }

        private static bool IsRealtimeDefeat(AdmissionEvidence row)
        {
            return row != null
                && row.StartedNonTerminal
                && row.TickCount > 0
                && row.Terminal != null
                && row.Terminal.accepted
                && !row.Terminal.win
                && row.Terminal.lose
                && row.Terminal.playerSnapshot.currentHp == 0
                && row.Cues.Any(cue => string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                    StringComparison.Ordinal))
                && row.Cues.Any(cue => string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.BattleTerminal,
                    StringComparison.Ordinal));
        }

        private static bool MatchesTerminal(
            C1FormalRealtimeBattleTerminalResult terminal,
            bool win,
            long durationMs,
            int enemyApplications,
            int playerHp,
            int itemApplications)
        {
            return terminal != null
                && terminal.accepted
                && terminal.win == win
                && terminal.lose != win
                && terminal.durationMs == durationMs
                && terminal.enemyApplicationCount == enemyApplications
                && terminal.playerSnapshot.currentHp == playerHp
                && terminal.acceptedItemApplicationCount == itemApplications;
        }

        private static void VerifyAdmissionRejections(VerificationContext context)
        {
            string canonical = "verifier.item-session.rejection";
            C1FormalItemBattleItemRow i001 = CreateBattleItemRow(
                "I001",
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                InitialVerifierInstanceSignature,
                true,
                true);
            C1FormalItemBattleItemRow i002 = CreateBattleItemRow(
                "I002",
                RewardVerifierItemInstanceId,
                RewardVerifierInstanceSignature,
                true,
                true);
            C1FormalItemBattleItemRow unknown = CreateBattleItemRow(
                "I999",
                "verifier.unknown.instance",
                "verifier.unknown.canonical",
                true,
                true);
            List<RejectionEvidence> rows = new List<RejectionEvidence>
            {
                RejectAdapterSnapshot(
                    "duplicate-instance",
                    canonical,
                    new[]
                    {
                        i001,
                        CreateBattleItemRow(
                            "I002",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            RewardVerifierInstanceSignature,
                            true,
                            true)
                    },
                    canonical,
                    601L),
                RejectAdapterSnapshot(
                    "duplicate-base-item",
                    canonical,
                    new[]
                    {
                        i001,
                        CreateBattleItemRow(
                            "I001",
                            RewardVerifierItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true)
                    },
                    canonical,
                    602L),
                RejectAdapterSnapshot(
                    "unknown-item",
                    canonical,
                    new[] { i001, unknown },
                    canonical,
                    603L),
                RejectAdapterSnapshot(
                    "missing-i001",
                    canonical,
                    new[] { i002 },
                    canonical,
                    604L),
                RejectAdapterSnapshot(
                    "extra-entitlement",
                    canonical,
                    new[] { i001, i002, unknown },
                    canonical,
                    605L),
                RejectAdapterSnapshot(
                    "stale-instance-canonical",
                    canonical,
                    new[]
                    {
                        i001,
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            "stale-instance-canonical",
                            true,
                            true)
                    },
                    canonical,
                    606L),
                RejectAdapterSnapshot(
                    "stale-catalog-canonical",
                    canonical,
                    new[]
                    {
                        i001,
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            true,
                            true,
                            itemCatalogCanonicalOverride: "stale-catalog")
                    },
                    canonical,
                    607L),
                RejectAdapterSnapshot(
                    "inconsistent-placement-lighting",
                    canonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            false,
                            false),
                        i002
                    },
                    canonical,
                    608L),
                RejectAdapterSnapshot(
                    "stale-item-session-envelope",
                    canonical,
                    new[] { i001, i002 },
                    "different-expected-session",
                    609L),
                RejectAdapterSnapshot(
                    "wrong-product-context",
                    canonical,
                    new[] { i001, i002 },
                    canonical,
                    610L,
                    "DEV_SHOWCASE_LV40")
            };

            ScenarioFixture staleReset = CreateFixture(
                "admission-stale-reset",
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                true,
                611L,
                1L);
            bool resetRejected = !staleReset.Session.TryStart(
                    staleReset.Request,
                    out C1FormalRealtimeBattleSessionStartSnapshot resetStart)
                && resetStart != null
                && string.Equals(
                    resetStart.errorCode,
                    C1FormalRealtimeBattleErrorCodes.ResetGenerationMismatch,
                    StringComparison.Ordinal);
            rows.Add(new RejectionEvidence(
                "stale-reset-envelope",
                resetRejected,
                resetStart == null ? string.Empty : resetStart.errorCode));

            context.Rejections.AddRange(rows);
            foreach (RejectionEvidence row in rows)
            {
                context.Check(
                    "admission-reject-" + Slug(row.Scenario),
                    "negative-fixture",
                    row.Rejected,
                    "rejected",
                    row.ErrorCode,
                    "ADMISSION_REJECTION_FAILED");
            }
        }

        private static RejectionEvidence RejectAdapterSnapshot(
            string scenario,
            string itemSessionCanonical,
            IEnumerable<C1FormalItemBattleItemRow> itemRows,
            string expectedItemSessionCanonical,
            long generation,
            string productContext = null)
        {
            C1FormalItemBattleInputSnapshot snapshot = CreateStageTwoItemSnapshot(
                scenario,
                itemSessionCanonical,
                itemRows);
            BattleLaunchContext launch = CreateContext(
                "rejection-" + scenario,
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                generation,
                productContext);
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool rejected = !adapter.TryStart(
                launch,
                snapshot,
                expectedItemSessionCanonical,
                "verifier.rejection.session." + scenario,
                "verifier.rejection.token." + scenario,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            string errorCode = start == null ? string.Empty : start.errorCode;
            adapter.Unbind();
            return new RejectionEvidence(scenario, rejected, errorCode);
        }

        private static C1FormalItemBattleInputSnapshot CreateStageTwoItemSnapshot(
            string suffix,
            string itemSessionCanonical,
            IEnumerable<C1FormalItemBattleItemRow> itemRows)
        {
            string arrangementCanonical = "verifier.arrangement." + suffix;
            string itemSystemCanonical = "verifier.item-system." + suffix;
            Vector2Int sourceAnchor = new Vector2Int(0, 0);
            ConstructorInfo capacityConstructor = typeof(
                    C1FormalI031NianCapacityFact)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single();
            C1FormalI031NianCapacityFact capacity =
                (C1FormalI031NianCapacityFact)capacityConstructor.Invoke(
                    new object[]
                    {
                        I031InventoryPlacementContract.SpecialIdentityId,
                        I031InventoryPlacementContract.ItemId,
                        I031InventoryPlacementContract.StablePlacementId,
                        sourceAnchor,
                        itemSessionCanonical,
                        arrangementCanonical,
                        itemSystemCanonical
                    });
            ConstructorInfo constructor = typeof(C1FormalItemBattleInputSnapshot)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single();
            return (C1FormalItemBattleInputSnapshot)constructor.Invoke(new object[]
            {
                itemSessionCanonical,
                arrangementCanonical,
                itemSystemCanonical,
                I031InventoryPlacementContract.StablePlacementId,
                (Vector2Int?)sourceAnchor,
                capacity,
                itemRows
            });
        }

        private static C1FormalItemBattleItemRow CreatePool15BattleItemRow(
            string baseItemId,
            string itemInstanceId,
            bool isPlaced,
            bool? isLit)
        {
            return CreateBattleItemRow(
                baseItemId,
                itemInstanceId,
                "verifier.pool15.instance-canonical." + itemInstanceId,
                isPlaced,
                isLit);
        }

        private static bool TryStartCumulativeItemSnapshot(
            string scenario,
            string stageId,
            long generation,
            IEnumerable<C1FormalItemBattleItemRow> rows,
            out C1FormalRealtimeBattleSessionAdapter adapter,
            out C1FormalItemBattleInputSnapshot itemSnapshot,
            out C1FormalRealtimeBattleSessionStartSnapshot start)
        {
            string itemSessionCanonical =
                "verifier.cumulative.item-session." + scenario;
            itemSnapshot = CreateStageTwoItemSnapshot(
                "cumulative-" + scenario,
                itemSessionCanonical,
                rows ?? Array.Empty<C1FormalItemBattleItemRow>());
            adapter = new C1FormalRealtimeBattleSessionAdapter();
            return adapter.TryStartFromCumulativeItemSnapshot(
                CreateContext(scenario, stageId, generation),
                itemSnapshot,
                itemSessionCanonical,
                "verifier.cumulative.session." + scenario,
                "verifier.cumulative.token." + scenario,
                out start);
        }

        private static bool AdvanceUntilPoolSourcesApplied(
            C1FormalRealtimeBattleSessionAdapter adapter,
            IEnumerable<string> expectedItemInstanceIds,
            long maxBattleTimeMs)
        {
            HashSet<string> expected = new HashSet<string>(
                expectedItemInstanceIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            int guard = 0;
            while (adapter != null
                   && !adapter.IsTerminal
                   && adapter.Session.BattleTimeMs < maxBattleTimeMs
                   && guard++ < 400)
            {
                HashSet<string> applied = new HashSet<string>(
                    adapter.Session.Pool15ApplicationLedger.Select(row =>
                        row.sourceItemInstanceId),
                    StringComparer.Ordinal);
                if (expected.All(applied.Contains)) return true;
                if (!adapter.TryAdvanceBy(500L, out _)) return false;
            }

            HashSet<string> finalApplied = new HashSet<string>(
                adapter == null
                    ? Array.Empty<string>()
                    : adapter.Session.Pool15ApplicationLedger.Select(row =>
                        row.sourceItemInstanceId),
                StringComparer.Ordinal);
            return expected.All(finalApplied.Contains);
        }

        private static C1FormalItemBattleItemRow CreateBattleItemRow(
            string baseItemId,
            string itemInstanceId,
            string instanceCanonical,
            bool isPlaced,
            bool? isLit,
            string itemCatalogCanonicalOverride = null,
            string baselineProjectionCanonicalOverride = null)
        {
            bool i001 = string.Equals(
                baseItemId,
                "I001",
                StringComparison.Ordinal);
            bool i002 = string.Equals(
                baseItemId,
                "I002",
                StringComparison.Ordinal);
            C1CampaignLootEligibleItemPool15Snapshot pool =
                C1CampaignLootEligibleItemPool15Carrier.Resolve().snapshot;
            C1CampaignPool15BaseCombatFact combatFact = i001
                ? null
                : C1CampaignPool15BaseCombatFacts.Resolve()
                    ?.snapshot?.Find(baseItemId);
            string rarityVersionKey = i001
                ? "I001@white"
                : i002
                    ? "I002@white"
                    : string.Empty;
            C1Lv1StarterItemProjectionResult projectionResult =
                i001 || i002
                    ? C1Lv1StarterItemBaselineCatalog.TryGetProjection(
                    C1FormalRealtimeBattleSessionContract.ProductContext,
                    i001
                        ? "I001"
                        : "I002",
                    rarityVersionKey,
                    C1Lv1StarterItemBaselineCatalog.ProfileRevision)
                    : null;
            if ((i001 || i002)
                && (projectionResult == null
                    || !projectionResult.isSuccess
                    || projectionResult.projection == null))
            {
                throw new InvalidOperationException(
                    "VERIFIER_ITEM_PROJECTION_MISSING " + baseItemId);
            }
            if (!i001 && combatFact == null)
            {
                throw new InvalidOperationException(
                    "VERIFIER_COMBAT_FACT_MISSING " + baseItemId);
            }

            string baselineCanonical =
                baselineProjectionCanonicalOverride
                ?? (i001 || i002
                    ? projectionResult.projection.canonicalSignature
                    : combatFact.sourceProfileCanonicalSignature);
            string itemCatalogCanonical = itemCatalogCanonicalOverride
                ?? (i001
                    ? C1Lv1StarterItemBaselineCatalog.canonicalSignature
                    : pool.sourceItemCatalogCanonicalSignature);
            C1FormalItemOrdinaryInstanceSnapshot ordinaryInstance = null;
            if (!i001)
            {
                ItemInstanceIdentityCreationResult identityResult =
                    ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                        ItemRarityInstanceFoundation.Create(),
                        itemInstanceId,
                        baseItemId,
                        C1Lv1StarterItemBaselineCatalog.WhiteRarityKey,
                        1,
                        1L);
                if (identityResult == null
                    || !identityResult.isValid
                    || identityResult.snapshot == null)
                {
                    throw new InvalidOperationException(
                        "VERIFIER_ITEM_IDENTITY_MISSING " + baseItemId);
                }

                ConstructorInfo ordinaryConstructor = typeof(
                        C1FormalItemOrdinaryInstanceSnapshot)
                    .GetConstructors(
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single(constructor =>
                        constructor.GetParameters().Length == 7);
                ordinaryInstance =
                    (C1FormalItemOrdinaryInstanceSnapshot)
                    ordinaryConstructor.Invoke(new object[]
                    {
                        identityResult.snapshot,
                        "verifier.identity-canonical." + itemInstanceId,
                        baselineCanonical,
                        itemCatalogCanonical,
                        "VERIFIER_CURRENT_ITEM_INSTANCE",
                        instanceCanonical,
                        combatFact
                    });
            }

            ConstructorInfo rosterConstructor = typeof(
                    C1FormalItemRosterEntrySnapshot)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single();
            C1FormalItemRosterEntrySnapshot roster =
                (C1FormalItemRosterEntrySnapshot)rosterConstructor.Invoke(
                    new object[]
                    {
                        itemInstanceId,
                        baseItemId,
                        false,
                        C1Lv1StarterItemBaselineCatalog.WhiteRarityKey,
                        baselineCanonical,
                        itemCatalogCanonical,
                        instanceCanonical,
                        ordinaryInstance
                    });
            C1FormalItemPlacementSnapshot placement = null;
            if (isPlaced)
            {
                ConstructorInfo placementConstructor = typeof(
                        C1FormalItemPlacementSnapshot)
                    .GetConstructors(
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single();
                placement = (C1FormalItemPlacementSnapshot)
                    placementConstructor.Invoke(new object[]
                    {
                        itemInstanceId,
                        "verifier.placement." + itemInstanceId,
                        baseItemId,
                        new Vector2Int(1, 1),
                        0
                    });
            }
            ConstructorInfo rowConstructor = typeof(C1FormalItemBattleItemRow)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single();
            return (C1FormalItemBattleItemRow)rowConstructor.Invoke(new object[]
            {
                roster,
                placement,
                isPlaced,
                isLit,
                isLit
            });
        }

        private static Pool15ScenarioEvidence StartPool15Scenario(
            string scenario,
            IEnumerable<string> activeBaseItemIds,
            IEnumerable<string> placedUnlitBaseItemIds,
            long generation,
            bool i001Active = true)
        {
            string itemSessionCanonical =
                "verifier.pool15.item-session." + scenario;
            C1FormalItemBattleInputSnapshot formalSnapshot =
                CreateStageTwoItemSnapshot(
                    "pool15-" + scenario,
                    itemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            i001Active,
                            i001Active ? (bool?)true : null)
                    });
            C1FormalRealtimeBattlePool15ItemSnapshot poolSnapshot =
                CreatePool15Snapshot(
                    scenario,
                    activeBaseItemIds,
                    placedUnlitBaseItemIds);
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool started = adapter.TryStart(
                CreateContext(
                    "pool15-" + scenario,
                    C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                    generation),
                formalSnapshot,
                itemSessionCanonical,
                "verifier.pool15.session." + scenario,
                "verifier.pool15.token." + scenario,
                poolSnapshot,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            return new Pool15ScenarioEvidence(
                scenario,
                formalSnapshot,
                adapter,
                start,
                started && start != null && start.accepted
                    && start.stateSnapshot != null
                    && !start.stateSnapshot.terminal);
        }

        private static C1FormalRealtimeBattlePool15ItemSnapshot
            CreatePool15Snapshot(
                string suffix,
                IEnumerable<string> activeBaseItemIds,
                IEnumerable<string> placedUnlitBaseItemIds)
        {
            return CreatePool15SnapshotFromRows(
                suffix,
                CreatePool15Rows(
                    activeBaseItemIds,
                    placedUnlitBaseItemIds));
        }

        private static C1FormalRealtimeBattlePool15ItemSnapshot
            CreatePool15SnapshotFromRows(
                string suffix,
                IEnumerable<C1FormalRealtimeBattlePool15ItemRow> rows)
        {
            C1CampaignLootEligibleItemPool15Snapshot pool =
                C1CampaignLootEligibleItemPool15Carrier.Resolve().snapshot;
            C1CampaignLootPool15BattleCandidateProfileSnapshot candidates =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            return new C1FormalRealtimeBattlePool15ItemSnapshot(
                C1FormalRealtimeBattleSessionContract
                    .Pool15ItemSnapshotSchemaId,
                C1FormalRealtimeBattleSessionContract.ProductContext,
                "verifier.pool15.entitlement." + suffix,
                pool.poolCanonicalSignature,
                pool.sourceItemCatalogCanonicalSignature,
                candidates.profileId,
                candidates.dataMaturity,
                candidates.canonicalSignature,
                rows);
        }

        private static List<C1FormalRealtimeBattlePool15ItemRow>
            CreatePool15Rows(
                IEnumerable<string> activeBaseItemIds,
                IEnumerable<string> placedUnlitBaseItemIds)
        {
            HashSet<string> active = new HashSet<string>(
                activeBaseItemIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            HashSet<string> unlit = new HashSet<string>(
                placedUnlitBaseItemIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            C1CampaignLootEligibleItemPool15Snapshot pool =
                C1CampaignLootEligibleItemPool15Carrier.Resolve().snapshot;
            C1CampaignLootPool15BattleCandidateProfileSnapshot candidates =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.Resolve();
            return pool.Profiles.OrderBy(
                    row => row.baseItemId,
                    StringComparer.Ordinal)
                .Select(descriptor =>
                {
                    bool isActive = active.Contains(descriptor.baseItemId);
                    bool isUnlit = unlit.Contains(descriptor.baseItemId);
                    return new C1FormalRealtimeBattlePool15ItemRow(
                        "verifier.pool15.instance." + descriptor.baseItemId,
                        descriptor.baseItemId,
                        isActive || isUnlit,
                        isActive ? (bool?)true : isUnlit ? false : null,
                        descriptor.profileCanonicalSignature,
                        candidates.Find(descriptor.baseItemId)
                            .canonicalSignature);
                })
                .ToList();
        }

        private static bool RejectTamperedPool15Snapshot(
            string scenario,
            string baseItemId,
            long generation,
            bool tamperDescriptor,
            bool tamperCandidate,
            out string errorCode)
        {
            List<C1FormalRealtimeBattlePool15ItemRow> rows = CreatePool15Rows(
                new[] { baseItemId },
                Array.Empty<string>());
            int index = rows.FindIndex(row => string.Equals(
                row.baseItemId,
                baseItemId,
                StringComparison.Ordinal));
            C1FormalRealtimeBattlePool15ItemRow source = rows[index];
            rows[index] = new C1FormalRealtimeBattlePool15ItemRow(
                source.itemInstanceId,
                source.baseItemId,
                source.isPlaced,
                source.isLit,
                tamperDescriptor
                    ? "stale-descriptor"
                    : source.descriptorCanonicalSignature,
                tamperCandidate
                    ? "stale-candidate-unit-profile"
                    : source.candidateProfileCanonicalSignature);

            string itemSessionCanonical =
                "verifier.pool15.item-session." + scenario;
            C1FormalItemBattleInputSnapshot formalSnapshot =
                CreateStageTwoItemSnapshot(
                    scenario,
                    itemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true)
                    });
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool rejected = !adapter.TryStart(
                CreateContext(
                    scenario,
                    C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                    generation),
                formalSnapshot,
                itemSessionCanonical,
                "verifier.pool15.session." + scenario,
                "verifier.pool15.token." + scenario,
                CreatePool15SnapshotFromRows(scenario, rows),
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            errorCode = start == null ? string.Empty : start.errorCode;
            adapter.Unbind();
            return rejected && string.Equals(
                errorCode,
                C1FormalRealtimeBattleErrorCodes.Pool15SnapshotRejected,
                StringComparison.Ordinal);
        }

        private static bool RejectPool15RowSet(
            string scenario,
            IEnumerable<C1FormalRealtimeBattlePool15ItemRow> rows,
            long generation,
            out string errorCode)
        {
            bool rejected = !TryStartPool15RowSet(
                scenario,
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                rows,
                generation,
                out C1FormalRealtimeBattleSessionAdapter adapter,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            errorCode = start == null ? string.Empty : start.errorCode;
            adapter.Unbind();
            return rejected && string.Equals(
                errorCode,
                C1FormalRealtimeBattleErrorCodes.Pool15SnapshotRejected,
                StringComparison.Ordinal);
        }

        private static bool TryStartPool15RowSet(
            string scenario,
            string stageId,
            IEnumerable<C1FormalRealtimeBattlePool15ItemRow> rows,
            long generation,
            out C1FormalRealtimeBattleSessionAdapter adapter,
            out C1FormalRealtimeBattleSessionStartSnapshot start)
        {
            string itemSessionCanonical =
                "verifier.pool15.item-session." + scenario;
            C1FormalItemBattleInputSnapshot formalSnapshot =
                CreateStageTwoItemSnapshot(
                    scenario,
                    itemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true)
                    });
            adapter = new C1FormalRealtimeBattleSessionAdapter();
            return adapter.TryStart(
                CreateContext(scenario, stageId, generation),
                formalSnapshot,
                itemSessionCanonical,
                "verifier.pool15.session." + scenario,
                "verifier.pool15.token." + scenario,
                CreatePool15SnapshotFromRows(scenario, rows),
                out start);
        }

        private static void ClearSessionActors(
            C1FormalRealtimeBattleSession session)
        {
            FieldInfo field = typeof(C1FormalRealtimeBattleSession).GetField(
                "actors",
                BindingFlags.Instance | BindingFlags.NonPublic);
            (field?.GetValue(session) as System.Collections.IList)?.Clear();
        }

        private static bool SetSessionNianForBoundaryVerification(
            C1FormalRealtimeBattleSession session,
            int currentNian)
        {
            FieldInfo field = typeof(C1FormalRealtimeBattleSession).GetField(
                "currentNian",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || session == null)
            {
                return false;
            }

            field.SetValue(session, currentNian);
            return true;
        }

        private static bool SetSessionGuardForBoundaryVerification(
            C1FormalRealtimeBattleSession session,
            long currentGuard)
        {
            FieldInfo field = typeof(C1FormalRealtimeBattleSession).GetField(
                "playerGuard",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || session == null)
            {
                return false;
            }

            field.SetValue(session, currentGuard);
            return true;
        }

        private static void VerifyLiveSessionRegressionScenarios(
            VerificationContext context)
        {
            ScenarioTrace stageOne = RunScenario(
                "1-1",
                false,
                1L,
                context);
            ScenarioTrace weak = RunScenario(
                "1-2-weak",
                false,
                1L,
                context);
            ScenarioTrace tray = RunScenario(
                "1-2-tray",
                false,
                2L,
                context,
                false);
            ScenarioTrace target = RunScenario(
                "1-2-target",
                true,
                1L,
                context);
            context.Traces.Add(stageOne);
            context.Traces.Add(weak);
            context.Traces.Add(tray);
            context.Traces.Add(target);

            VerifyTrace(context, stageOne, 16000L, 3, 76, 8, 2);
            VerifyTrace(context, weak, 48000L, 10, 20, 24, 3);
            VerifyTrace(context, tray, 48000L, 10, 20, 24, 3);
            VerifyTrace(context, target, 36000L, 7, 44, 35, 3);

            decimal speedGain = weak.Terminal == null
                || weak.Terminal.durationMs == 0L
                ? 0m
                : (weak.Terminal.durationMs - target.Terminal.durationMs)
                  / (decimal)weak.Terminal.durationMs;
            int avoided = weak.Terminal == null || target.Terminal == null
                ? 0
                : weak.Terminal.enemyApplicationCount
                  - target.Terminal.enemyApplicationCount;
            context.Check(
                "target-build-material-difference",
                "live-session",
                speedGain == 0.25m && avoided == 3,
                "25% faster and 3 enemy applications avoided",
                (speedGain * 100m).ToString(
                    "0.##",
                    CultureInfo.InvariantCulture) + "% / " +
                avoided.ToString(CultureInfo.InvariantCulture),
                "REALTIME_LIVE_REGRESSION");

            VerifyCueLedger(context, stageOne);
            VerifyCueLedger(context, weak);
            VerifyCueLedger(context, tray);
            VerifyCueLedger(context, target);
            bool weakI002Accepted = weak.Cues.Any(cue =>
                string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                    StringComparison.Ordinal)
                && string.Equals(
                    cue.sourceId,
                    "I002@white",
                    StringComparison.Ordinal));
            bool weakHasI002Residue = weak.Cues.Any(cue => string.Equals(
                    cue.sourceId,
                    "I002@white",
                    StringComparison.Ordinal))
                || (weak.FinalState != null
                    && weak.FinalState.scheduledActions.Any(action =>
                        string.Equals(
                            action.sourceId,
                            "I002@white",
                            StringComparison.Ordinal)));
            context.Check(
                "i002-inactive-entitlement-is-not-a-live-runtime-fact",
                "runtime-integration",
                !weakI002Accepted && !weakHasI002Residue,
                "no I002 action, diagnostic, cue, or damage",
                (!weakI002Accepted && !weakHasI002Residue) ? "PASS" : "FAIL",
                "ITEM_UNLIT_EXECUTION_LEAK");

            bool trayHasI002Residue = tray.Cues.Any(cue => string.Equals(
                    cue.sourceId,
                    "I002@white",
                    StringComparison.Ordinal))
                || (tray.FinalState != null
                    && tray.FinalState.scheduledActions.Any(action =>
                        string.Equals(
                            action.sourceId,
                            "I002@white",
                            StringComparison.Ordinal)));
            context.Check(
                "i002-tray-entitlement-is-not-live-runtime-fact",
                "runtime-integration",
                !trayHasI002Residue,
                "no I002 live action, diagnostic, cue, or damage",
                trayHasI002Residue ? "residue present" : "PASS",
                "OPTIONAL_I002_RUNTIME_RESIDUE");
        }

        private static ScenarioTrace RunScenario(
            string scenario,
            bool i002Lit,
            long generation,
            VerificationContext context,
            bool includeStageTwoI002Fact = true)
        {
            string stageId = scenario == "1-1" ? "1-1" : "1-2";
            BattleLaunchContext launch = CreateContext(
                scenario,
                stageId,
                generation);
            C1FormalRealtimeBattleSession session =
                new C1FormalRealtimeBattleSession();
            C1FormalRealtimeBattleSessionRequest request = BuildRequest(
                launch,
                scenario,
                i002Lit,
                0L,
                includeStageTwoI002Fact: includeStageTwoI002Fact);
            bool started = session.TryStart(
                request,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            ScenarioTrace trace = new ScenarioTrace(
                scenario,
                request,
                session,
                start);
            if (!started || start == null || !start.accepted
                || start.stateSnapshot == null)
            {
                return trace;
            }

            trace.Cues.AddRange(start.initialCues);
            bool firstTick = session.TryAdvanceBy(
                1000L,
                out C1FormalRealtimeBattleTickResult first);
            if (first != null)
            {
                trace.Cues.AddRange(first.emittedCues);
                trace.FinalState = first.stateSnapshot;
            }

            trace.StartedNonImmediate = firstTick
                && first != null
                && first.accepted
                && first.terminalResult == null
                && first.stateSnapshot != null
                && !first.stateSnapshot.terminal
                && first.stateSnapshot.battleTimeMs == 1000L
                && first.stateSnapshot.playerSnapshot.currentHp == 100
                && first.stateSnapshot.actorSnapshots.Sum(actor => actor.currentHp)
                == first.stateSnapshot.actorSnapshots.Sum(actor => actor.maxHp);
            int tickGuard = 0;
            while (!session.IsTerminal && tickGuard < 200)
            {
                tickGuard++;
                if (!session.TryAdvanceBy(
                        500L,
                        out C1FormalRealtimeBattleTickResult tick)
                    || tick == null)
                {
                    break;
                }

                trace.TickCount++;
                trace.Cues.AddRange(tick.emittedCues);
                trace.FinalState = tick.stateSnapshot;
                if (tick.terminalResult != null)
                {
                    trace.Terminal = tick.terminalResult;
                    break;
                }
            }

            bool rejectedAfterTerminal = !session.TryAdvanceBy(
                1L,
                out C1FormalRealtimeBattleTickResult terminalTick);
            trace.TickAfterTerminalRejected = rejectedAfterTerminal
                && terminalTick != null
                && string.Equals(
                    terminalTick.errorCode,
                    C1FormalRealtimeBattleErrorCodes.TickAfterTerminal,
                    StringComparison.Ordinal);
            foreach (C1FormalRealtimeBattleCue cue in trace.Cues)
            {
                context.SeenCueKinds.Add(cue.cueKind);
            }

            return trace;
        }

        private static void VerifyTrace(
            VerificationContext context,
            ScenarioTrace trace,
            long durationMs,
            int enemyApplications,
            int playerHp,
            int acceptedItemApplications,
            int actorCount)
        {
            C1FormalRealtimeBattleTerminalResult terminal = trace.Terminal;
            bool passed = trace.StartSnapshot != null
                && trace.StartSnapshot.accepted
                && trace.StartSnapshot.stateSnapshot != null
                && !trace.StartSnapshot.stateSnapshot.terminal
                && trace.StartedNonImmediate
                && trace.TickCount > 0
                && terminal != null
                && terminal.accepted
                && terminal.win
                && !terminal.lose
                && terminal.durationMs == durationMs
                && terminal.enemyApplicationCount == enemyApplications
                && terminal.playerSnapshot.currentHp == playerHp
                && terminal.acceptedItemApplicationCount ==
                    acceptedItemApplications
                && terminal.actorSnapshots.Count == actorCount
                && terminal.actorSnapshots.Sum(actor => actor.currentHp) == 0
                && !terminal.shouldWriteSave
                && !terminal.shouldGrantReward
                && terminal.battleResultSnapshot != null
                && !terminal.battleResultSnapshot.shouldWriteSave
                && !terminal.battleResultSnapshot.shouldGrantReward
                && trace.TickAfterTerminalRejected;
            context.Check(
                "trace-" + trace.Scenario,
                "live-session",
                passed,
                durationMs + "ms / enemy=" + enemyApplications +
                " / playerHp=" + playerHp,
                terminal == null
                    ? "terminal missing"
                    : terminal.durationMs + "ms / enemy=" +
                      terminal.enemyApplicationCount + " / playerHp=" +
                      terminal.playerSnapshot.currentHp,
                "REALTIME_LIVE_REGRESSION");
        }

        private static void VerifyCueLedger(
            VerificationContext context,
            ScenarioTrace trace)
        {
            bool sequenceMonotonic = trace.Cues.Select(cue => cue.sequence)
                .SequenceEqual(trace.Cues.Select(cue => cue.sequence)
                    .OrderBy(value => value));
            bool cueIdsUnique = trace.Cues.Select(cue => cue.cueId)
                .Distinct(StringComparer.Ordinal).Count() == trace.Cues.Count;
            List<string> acceptedIds = trace.Cues
                .Where(cue => string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                    StringComparison.Ordinal)
                    || string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.EnemyAttackAccepted,
                        StringComparison.Ordinal))
                .Select(cue => cue.acceptedApplicationEventId)
                .ToList();
            bool applicationIdsUnique = acceptedIds.All(id =>
                    !string.IsNullOrEmpty(id))
                && acceptedIds.Distinct(StringComparer.Ordinal).Count()
                == acceptedIds.Count;
            context.Check(
                "cue-order-and-dedupe-" + trace.Scenario,
                "cue-contract",
                sequenceMonotonic && cueIdsUnique && applicationIdsUnique,
                "monotonic cue sequence, unique cue/application IDs",
                sequenceMonotonic && cueIdsUnique && applicationIdsUnique
                    ? "PASS"
                    : "FAIL",
                "CUE_LEDGER_ORDER_OR_DEDUPE_FAIL");
        }

        private static void VerifyStage1To5FormalRealtimeExtension(
            VerificationContext context)
        {
            C1Stage3To5PlaytestEnemyEncounterValidationResult validation =
                C1Stage3To5PlaytestEnemyEncounterCatalog.Validation;
            bool supplierAccepted = validation != null
                && validation.IsValid
                && string.Equals(
                    C1Stage3To5PlaytestEnemyEncounterCatalog
                        .CanonicalSignature,
                    "79d5e5d0b3cb18f0e5a854dbba1327b1ad7948702ff3c581b0e67eebd948058c",
                    StringComparison.Ordinal);
            context.Check(
                "stage3to5-enemy-supplier-release",
                "supplier",
                supplierAccepted,
                "valid/79d5e5d0...",
                validation == null
                    ? "missing"
                    : validation.IsValid + "/"
                      + C1Stage3To5PlaytestEnemyEncounterCatalog
                          .CanonicalSignature,
                "STAGE3TO5_ENEMY_SUPPLIER_REJECTED");

            StageExtensionEvidence stage13 = RunStageExtensionScenario(
                context,
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                new[] { "I001" },
                Array.Empty<string>(),
                4101L);
            StageExtensionEvidence stage14 = RunStageExtensionScenario(
                context,
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                new[] { "I001" },
                new[] { "I013" },
                4102L);
            StageExtensionEvidence stage15 = RunStageExtensionScenario(
                context,
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                new[]
                {
                    "I001",
                    "I002"
                },
                new[] { "I003", "I013" },
                4103L);

            VerifyStageExtensionEvidence(context, stage13, 0);
            VerifyStageExtensionEvidence(context, stage14, 1);
            VerifyStageExtensionEvidence(context, stage15, 2);
            VerifyStage3To5WaveAndDefeat(context);
            VerifyStage3To5BoneSwap(context, stage14);
            VerifyStage3To5LiveRefresh(context);
            VerifyStage3To5Rejections(context);
        }

        private static StageExtensionEvidence RunStageExtensionScenario(
            VerificationContext context,
            string stageId,
            IEnumerable<string> activeDirectItems,
            IEnumerable<string> activePool15Items,
            long generation)
        {
            string suffix = "stage-extension-" + stageId;
            string itemSessionCanonical = "verifier." + suffix
                + ".item-session";
            HashSet<string> activeDirect = new HashSet<string>(
                activeDirectItems ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            C1FormalItemBattleInputSnapshot formalSnapshot =
                CreateStageTwoItemSnapshot(
                    suffix,
                    itemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            activeDirect.Contains(
                                "I001"),
                            activeDirect.Contains(
                                "I001")
                                ? (bool?)true
                                : null),
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            activeDirect.Contains(
                                "I002"),
                            activeDirect.Contains(
                                "I002")
                                ? (bool?)true
                                : null)
                    });
            string[] activePool = (activePool15Items
                ?? Array.Empty<string>()).ToArray();
            C1FormalRealtimeBattlePool15ItemSnapshot poolSnapshot =
                CreatePool15Snapshot(
                    suffix,
                    activePool,
                    Array.Empty<string>());
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool started = adapter.TryStart(
                CreateContext(suffix, stageId, generation),
                formalSnapshot,
                itemSessionCanonical,
                "verifier." + suffix + ".session",
                "verifier." + suffix + ".token",
                poolSnapshot,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            StageExtensionEvidence evidence = new StageExtensionEvidence(
                stageId,
                activePool.Length,
                adapter,
                start,
                started && start != null && start.accepted
                    && start.stateSnapshot != null
                    && !start.stateSnapshot.terminal);
            if (evidence.StartedNonTerminal)
            {
                int guard = 0;
                while (!adapter.IsTerminal && guard++ < 400)
                {
                    if (!adapter.TryAdvanceBy(
                            500L,
                            out C1FormalRealtimeBattleTickResult tick))
                    {
                        break;
                    }
                    evidence.TickCount++;
                    evidence.FinalState = tick.stateSnapshot;
                    if (tick.terminalResult != null)
                    {
                        evidence.Terminal = tick.terminalResult;
                    }
                }

                adapter.TryGetSnapshot(out evidence.FinalState);
                evidence.Terminal = adapter.Session.TerminalResult;
                evidence.Cues.AddRange(adapter.Session.CueLedger);
                foreach (C1FormalRealtimeBattleCue cue in evidence.Cues)
                {
                    context.SeenCueKinds.Add(cue.cueKind);
                }
            }
            context.StageExtensions.Add(evidence);
            adapter.Reset();
            return evidence;
        }

        private static void VerifyStageExtensionEvidence(
            VerificationContext context,
            StageExtensionEvidence evidence,
            int expectedPool15Active)
        {
            C1Stage3To5PlaytestEncounterProfile source =
                C1Stage3To5PlaytestEnemyEncounterCatalog.FindByStageId(
                    evidence.StageId);
            C1FormalRealtimeBattleSessionStateSnapshot start =
                evidence.Start == null ? null : evidence.Start.stateSnapshot;
            bool exactActors = source != null
                && start != null
                && start.actorSnapshots.Count == source.Actors.Count
                && start.actorSnapshots.Select(actor => actor.maxHp).Sum()
                    == source.TotalMaxHp;
            if (exactActors)
            {
                for (int index = 0; index < source.Actors.Count; index++)
                {
                    C1Stage3To5PlaytestActorRow expected = source.Actors[index];
                    C1FormalRealtimeBattleActorSnapshot actual =
                        start.actorSnapshots[index];
                    exactActors = actual.actorBalanceId == expected.ActorBalanceId
                        && actual.contentId == expected.ContentId
                        && actual.runtimeProfileId == expected.RuntimeProfileId
                        && actual.operatorProfileId == expected.OperatorProfileId
                        && actual.occurrenceOrdinal == expected.OccurrenceOrdinal
                        && actual.maxHp == expected.MaxHp
                        && actual.currentHp == expected.MaxHp
                        && actual.actionIds.SequenceEqual(expected.ActionIds)
                        && actual.effectRequestKeys.SequenceEqual(
                            expected.EffectRequestKeys)
                        && actual.downstreamConditions.SequenceEqual(
                            expected.DownstreamConditions)
                        && actual.sourceCanonicalSignature
                            == expected.CanonicalSignature;
                    if (!exactActors)
                    {
                        break;
                    }
                }
            }

            int expectedUnsupported = source == null
                ? -1
                : source.Actors.Sum(actor => actor.ActionIds.Count(actionId =>
                    !string.Equals(
                        actionId,
                        C1Stage3To5PlaytestEnemyEncounterCatalog
                            .BoneSwapRelease.ActionId,
                        StringComparison.Ordinal)));
            int actualUnsupported = evidence.Cues.Count(cue => string.Equals(
                cue.cueKind,
                C1FormalRealtimeBattleCueKinds.EnemyActionUnsupported,
                StringComparison.Ordinal));
            bool liveHpProgression = evidence.Cues.Any(cue =>
                cue.battleTimeMs > 0L
                && string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.ActorHpChanged,
                    StringComparison.Ordinal)
                && cue.actorHpAfter < cue.actorHpBefore);
            bool terminalLive = evidence.Terminal != null
                && evidence.Terminal.accepted
                && !evidence.Terminal.shouldWriteSave
                && !evidence.Terminal.shouldGrantReward;
            bool passed = evidence.StartedNonTerminal
                && start.battleTimeMs == 0L
                && start.activePool15SourceCount == expectedPool15Active
                && exactActors
                && actualUnsupported == expectedUnsupported
                && liveHpProgression
                && terminalLive;
            context.Check(
                "stage-extension-" + Slug(evidence.StageId),
                "stage-runtime",
                passed,
                "non-terminal start/exact catalog actors/pool="
                + expectedPool15Active + "/live HP/live terminal",
                evidence.Describe() + ";actors=" + exactActors
                + ";unsupported=" + actualUnsupported + "/"
                + expectedUnsupported + ";liveHp=" + liveHpProgression,
                "STAGE_EXTENSION_RUNTIME_FAILED");
        }

        private static void VerifyStage3To5WaveAndDefeat(
            VerificationContext context)
        {
            string stageId =
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3;
            string suffix = "stage-wave-defeat";
            string itemSessionCanonical = "verifier." + suffix
                + ".item-session";
            C1FormalItemBattleInputSnapshot formal =
                CreateStageTwoItemSnapshot(
                    suffix,
                    itemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            false,
                            null),
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            false,
                            null)
                    });
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool started = adapter.TryStart(
                CreateContext(suffix, stageId, 4201L),
                formal,
                itemSessionCanonical,
                "verifier." + suffix + ".session",
                "verifier." + suffix + ".token",
                CreatePool15Snapshot(
                    suffix,
                    Array.Empty<string>(),
                    Array.Empty<string>()),
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            bool beforeWave = started
                && adapter.TryAdvanceTo(
                    4499L,
                    out C1FormalRealtimeBattleTickResult before)
                && before.stateSnapshot.playerSnapshot.currentHp == 100
                && before.stateSnapshot.enemyApplicationCount == 0;
            bool firstWave = beforeWave
                && adapter.TryAdvanceTo(
                    4500L,
                    out C1FormalRealtimeBattleTickResult first)
                && first.stateSnapshot.playerSnapshot.currentHp == 93
                && first.stateSnapshot.enemyApplicationCount == 3;
            int guard = 0;
            while (firstWave && !adapter.IsTerminal && guard++ < 100)
            {
                adapter.TryAdvanceBy(4500L, out _);
            }
            C1FormalRealtimeBattleTerminalResult terminal =
                adapter.Session.TerminalResult;
            bool defeat = terminal != null
                && terminal.accepted
                && terminal.lose
                && !terminal.win
                && terminal.durationMs == 67500L
                && terminal.enemyApplicationCount == 43
                && terminal.playerSnapshot.currentHp == 0;
            context.Check(
                "stage3to5-per-actor-actions-and-real-defeat",
                "enemy-wave",
                started && beforeWave && firstWave && defeat,
                "4500ms/7 across 3 actors and live defeat 67500/43/0",
                "start=" + started + ";before=" + beforeWave
                + ";first=" + firstWave + ";terminal="
                + (terminal == null ? "missing" : terminal.durationMs + "/"
                    + terminal.enemyApplicationCount + "/"
                    + terminal.playerSnapshot.currentHp),
                "STAGE3TO5_WAVE_OR_DEFEAT_FAILED");
            adapter.Reset();
        }

        private static void VerifyStage3To5BoneSwap(
            VerificationContext context,
            StageExtensionEvidence evidence)
        {
            List<C1FormalRealtimeBattleCue> telegraphs = evidence.Cues
                .Where(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.BoneSwapTelegraphed)
                .OrderBy(cue => cue.sequence)
                .ToList();
            List<C1FormalRealtimeBattleCue> returns = evidence.Cues
                .Where(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.BoneSwapReturnAccepted)
                .OrderBy(cue => cue.sequence)
                .ToList();
            List<C1FormalRealtimeBattleCue> recovery = evidence.Cues
                .Where(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.BoneSwapRecoveryStarted)
                .OrderBy(cue => cue.sequence)
                .ToList();
            List<C1FormalRealtimeBattleCue> pending = evidence.Cues
                .Where(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.BoneSwapPendingLatest)
                .OrderBy(cue => cue.sequence)
                .ToList();
            C1Stage3To5BoneSwapReleaseRow release =
                C1Stage3To5PlaytestEnemyEncounterCatalog.BoneSwapRelease;
            bool timing = telegraphs.Count >= 2
                && returns.Count >= 2
                && returns[0].battleTimeMs - telegraphs[0].battleTimeMs
                    == release.TelegraphMilliseconds
                && telegraphs[1].battleTimeMs - returns[0].battleTimeMs
                    == release.RecoverMilliseconds;
            C1FormalRealtimeBattleCue latestBeforeSecond = pending
                .Where(cue => telegraphs.Count >= 2
                    && cue.battleTimeMs < telegraphs[1].battleTimeMs)
                .LastOrDefault();
            bool latestWins = latestBeforeSecond != null
                && telegraphs.Count >= 2
                && latestBeforeSecond.acceptedApplicationEventId
                    == telegraphs[1].acceptedApplicationEventId;
            bool bounded = returns.Count >= 2
                && returns.All(cue => cue.requestedDamage > 0
                    && cue.requestedDamage <= release.CapDamage)
                && returns.Any(cue => cue.requestedDamage
                    == release.CapDamage);
            bool explicitIdentity = telegraphs.Concat(returns).All(cue =>
                cue.sourceId == release.ActionId
                && cue.floatPayload.IndexOf(
                    release.CanonicalSignature,
                    StringComparison.Ordinal) >= 0);
            bool nonItemPresentationEmpty = evidence.Cues.Where(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.BoneSwapTelegraphed
                    || cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.BoneSwapPendingLatest
                    || cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.BoneSwapReturnAccepted
                    || cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.BoneSwapRecoveryStarted
                    || cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.EnemyActionUnsupported)
                .All(cue => string.IsNullOrEmpty(cue.sourceItemInstanceId)
                    && string.IsNullOrEmpty(cue.sourceBaseItemId)
                    && string.IsNullOrEmpty(cue.effectFamilyId)
                    && string.IsNullOrEmpty(cue.effectVariantId));
            StageExtensionEvidence terminalCancellationEvidence = context
                .StageExtensions.FirstOrDefault(row => row.FinalState != null
                && row.FinalState.scheduledActions.Any(action =>
                    (action.actionKind ==
                         C1FormalRealtimeBattleScheduledActionKinds
                             .BoneSwapTelegraphWake
                     || action.actionKind ==
                         C1FormalRealtimeBattleScheduledActionKinds
                             .BoneSwapReturn)
                    && action.executed
                    && !action.accepted
                    && action.diagnosticCode ==
                        C1FormalRealtimeBattleErrorCodes
                            .BoneSwapOperatorStale));
            bool terminalCancelled = terminalCancellationEvidence != null;
            bool accepted = timing
                && latestWins
                && bounded
                && explicitIdentity
                && nonItemPresentationEmpty
                && recovery.Count >= 2
                && terminalCancelled;
            context.Check(
                "bone-swap-release-operator-runtime",
                "bone-swap",
                accepted,
                "25% cap7/1500ms/18000ms/latest-wins/terminal cancellation",
                "telegraph=" + telegraphs.Count + ";return=" + returns.Count
                + ";recovery=" + recovery.Count + ";pending=" + pending.Count
                + ";timing=" + timing + ";latest=" + latestWins
                + ";bounded=" + bounded + ";terminalCancel="
                + terminalCancelled + ";terminalCancelStage="
                + (terminalCancellationEvidence == null
                    ? "missing"
                    : terminalCancellationEvidence.StageId)
                + ";finalBoneActions="
                + (terminalCancellationEvidence == null
                    ? "missing"
                    : string.Join(",", terminalCancellationEvidence.FinalState
                        .scheduledActions
                        .Where(action => action.actionKind.IndexOf(
                            "BONE_SWAP",
                            StringComparison.Ordinal) >= 0)
                        .Select(action => action.actionKind + "@"
                            + action.dueBattleTimeMs + ":executed="
                            + action.executed + ":accepted=" + action.accepted
                            + ":diag=" + action.diagnosticCode))),
                "BONE_SWAP_RUNTIME_FAILED");

            bool onlySingleTargetAccepted = telegraphs.All(telegraph =>
                evidence.Cues.Count(cue => cue.acceptedApplicationEventId
                    == telegraph.acceptedApplicationEventId
                    && cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.ActorHpChanged) == 1);
            context.Check(
                "bone-swap-direct-single-target-source-only",
                "bone-swap",
                onlySingleTargetAccepted,
                "each captured source has exactly one accepted actor HP target",
                onlySingleTargetAccepted ? "PASS" : "area/spill captured",
                "BONE_SWAP_SOURCE_FILTER_FAILED");
        }

        private static void VerifyStage3To5LiveRefresh(
            VerificationContext context)
        {
            string stageId =
                C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4;
            string itemSessionCanonical =
                "verifier.stage-refresh.item-session";
            C1FormalItemBattleInputSnapshot beforeItems =
                CreateStageTwoItemSnapshot(
                    "stage-refresh-before",
                    itemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            true,
                            true),
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            false,
                            null)
                    });
            C1FormalRealtimeBattleSessionAdapter adapter =
                new C1FormalRealtimeBattleSessionAdapter();
            bool started = adapter.TryStart(
                CreateContext("stage-refresh", stageId, 4301L),
                beforeItems,
                itemSessionCanonical,
                "verifier.stage-refresh.session",
                "verifier.stage-refresh.token",
                CreatePool15Snapshot(
                    "stage-refresh",
                    new[] { "I013" },
                    Array.Empty<string>()),
                out _);
            C1FormalRealtimeBattleSessionStateSnapshot before = null;
            bool advanced = started
                && adapter.TryAdvanceTo(10000L, out _)
                && adapter.TryPause(out _)
                && adapter.TryGetSnapshot(out before);
            C1FormalRealtimeBattleCue[] ledgerBefore = advanced
                ? adapter.Session.CueLedger.ToArray()
                : Array.Empty<C1FormalRealtimeBattleCue>();
            string targetBefore = ledgerBefore
                .Where(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.TargetChanged)
                .Select(cue => cue.targetActorId)
                .LastOrDefault() ?? string.Empty;
            C1FormalItemBattleInputSnapshot refreshedItems =
                CreateStageTwoItemSnapshot(
                    "stage-refresh-after",
                    itemSessionCanonical,
                    new[]
                    {
                        CreateBattleItemRow(
                            "I001",
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                            InitialVerifierInstanceSignature,
                            false,
                            null),
                        CreateBattleItemRow(
                            "I002",
                            RewardVerifierItemInstanceId,
                            RewardVerifierInstanceSignature,
                            true,
                            true)
                    });
            C1FormalRealtimeBattleItemRefreshResult refresh = null;
            C1FormalRealtimeBattleSessionStateSnapshot after = null;
            bool refreshed = advanced
                && adapter.TryRefreshItemsWhilePaused(
                    refreshedItems,
                    itemSessionCanonical,
                    "verifier.stage-refresh.command",
                    out refresh)
                && adapter.TryGetSnapshot(out after);
            bool duplicateAccepted = refreshed
                && adapter.TryRefreshItemsWhilePaused(
                    refreshedItems,
                    itemSessionCanonical,
                    "verifier.stage-refresh.command",
                    out C1FormalRealtimeBattleItemRefreshResult duplicate)
                && duplicate.canonicalSignature == refresh.canonicalSignature;
            C1FormalRealtimeBattleCue[] ledgerAfter = refreshed
                ? adapter.Session.CueLedger.ToArray()
                : Array.Empty<C1FormalRealtimeBattleCue>();
            string targetAfter = ledgerAfter
                .Where(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.TargetChanged)
                .Select(cue => cue.targetActorId)
                .LastOrDefault() ?? string.Empty;
            bool preserved = refreshed
                && before.battleTimeMs == after.battleTimeMs
                && before.playerSnapshot.currentHp
                    == after.playerSnapshot.currentHp
                && before.enemyApplicationCount == after.enemyApplicationCount
                && before.actorSnapshots.Select(actor => actor.currentHp)
                    .SequenceEqual(after.actorSnapshots.Select(
                        actor => actor.currentHp))
                && targetBefore == targetAfter
                && ledgerAfter.Take(ledgerBefore.Length).Select(
                        cue => cue.canonicalSignature)
                    .SequenceEqual(ledgerBefore.Select(
                        cue => cue.canonicalSignature))
                && after.scheduledActions.Any(action => !action.executed
                    && action.actionKind ==
                        C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger
                    && action.sourceId ==
                        "I002@white")
                && after.scheduledActions.Any(action => !action.executed
                    && action.actionKind ==
                        C1FormalRealtimeBattleScheduledActionKinds
                            .EnemyBasicWave
                    && action.dueBattleTimeMs == 13500L);
            context.Check(
                "stage3to5-rev10-live-item-refresh",
                "live-refresh",
                refreshed && duplicateAccepted && preserved,
                "future Item remap only; clock/HP/target/wave/history preserved; idempotent",
                "start=" + started + ";refresh=" + refreshed
                + ";duplicate=" + duplicateAccepted + ";preserved="
                + preserved,
                "STAGE3TO5_LIVE_REFRESH_FAILED");
            adapter.Reset();
        }

        private static void VerifyStage3To5Rejections(
            VerificationContext context)
        {
            C1FormalRealtimeBattleSessionRequest valid =
                BuildStage3To5DirectRequest(
                    "stage-reject-base",
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                    4401L,
                    0L);
            List<C1FormalRealtimeBattleSessionRequest> requests = new List<
                C1FormalRealtimeBattleSessionRequest>
            {
                CopyStage3To5Request(valid, "wrong-profile",
                    balanceProfileId: "wrong.profile"),
                CopyStage3To5Request(valid, "wrong-variant",
                    encounterVariantId: "wrong.variant"),
                CopyStage3To5Request(valid, "wrong-canonical",
                    expectedEnemyCatalogCanonical: "wrong.canonical"),
                CopyStage3To5Request(valid, "wrong-stage",
                    stageId: "1-6"),
                CopyStage3To5Request(valid, "wrong-context",
                    productContext: "DEV_SHOWCASE_LV40"),
                CopyStage3To5Request(valid, "wrong-reset",
                    resetGeneration: 1L)
            };
            string[] names =
            {
                "wrong-profile",
                "wrong-variant",
                "wrong-canonical",
                "wrong-stage",
                "wrong-context",
                "wrong-reset"
            };
            for (int index = 0; index < requests.Count; index++)
            {
                C1FormalRealtimeBattleSession session =
                    new C1FormalRealtimeBattleSession();
                bool rejected = !session.TryStart(
                    requests[index],
                    out C1FormalRealtimeBattleSessionStartSnapshot start);
                RejectionEvidence row = new RejectionEvidence(
                    names[index],
                    rejected,
                    start == null ? string.Empty : start.errorCode);
                context.StageRejections.Add(row);
                context.Check(
                    "stage3to5-reject-" + names[index],
                    "negative-fixture",
                    rejected,
                    "rejected",
                    row.ErrorCode,
                    "STAGE3TO5_REJECTION_FAILED");
            }

            C1FormalRealtimeBattleSession lifecycle =
                new C1FormalRealtimeBattleSession();
            bool startAccepted = lifecycle.TryStart(
                valid,
                out C1FormalRealtimeBattleSessionStartSnapshot accepted);
            bool reset = startAccepted && lifecycle.TryReset(
                accepted.stateSnapshot.sessionId,
                valid.sessionToken,
                accepted.stateSnapshot.sessionGeneration,
                accepted.stateSnapshot.resetGeneration,
                out _);
            C1FormalRealtimeBattleSessionRequest duplicate =
                CopyStage3To5Request(
                    valid,
                    "duplicate",
                    launchGeneration: valid.launchGeneration + 1L,
                    resetGeneration: lifecycle.CurrentResetGeneration,
                    preserveSessionIdentity: true);
            bool duplicateRejected = reset
                && !lifecycle.TryStart(duplicate, out _);
            C1FormalRealtimeBattleSessionRequest stale =
                CopyStage3To5Request(
                    valid,
                    "stale-generation",
                    launchGeneration: valid.launchGeneration,
                    resetGeneration: lifecycle.CurrentResetGeneration);
            bool staleRejected = reset
                && !lifecycle.TryStart(stale, out _);
            context.Check(
                "stage3to5-lifecycle-dedupe-stale-reset",
                "lifecycle",
                startAccepted && reset && duplicateRejected && staleRejected,
                "start/reset/duplicate reject/stale reject",
                "start=" + startAccepted + ";reset=" + reset
                + ";duplicate=" + duplicateRejected + ";stale="
                + staleRejected,
                "STAGE3TO5_LIFECYCLE_FAILED");
        }

        private static C1FormalRealtimeBattleSessionRequest
            BuildStage3To5DirectRequest(
                string suffix,
                string stageId,
                long generation,
                long resetGeneration)
        {
            BattleLaunchContext launch = CreateContext(
                suffix,
                stageId,
                generation);
            C1FormalRealtimeBattlePool15ItemSnapshot pool =
                CreatePool15Snapshot(
                    suffix,
                    Array.Empty<string>(),
                    Array.Empty<string>());
            C1FormalRealtimeBattlePool15ActiveItemFactSnapshot[] active =
                Array.Empty<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>();
            return new C1FormalRealtimeBattleSessionRequest(
                C1FormalRealtimeBattleSessionContract.RequestSchemaId,
                launch.ProductContext,
                launch.ChapterId,
                launch.StageId,
                launch.BalanceProfileId,
                launch.EncounterVariantId,
                launch.LaunchId,
                launch.Generation,
                "verifier." + suffix + ".session",
                "verifier." + suffix + ".token",
                resetGeneration,
                "verifier." + suffix + ".item-input",
                "verifier." + suffix + ".arrangement",
                CanonicalItemCatalogContract.CatalogId,
                C1Stage3To5PlaytestEnemyEncounterCatalog.CanonicalSignature,
                new[]
                {
                    CreateItemFact(
                        "I001",
                        "I001@white",
                        true)
                },
                pool.canonicalSignature,
                pool.poolCanonicalSignature,
                pool.sourceItemCatalogCanonicalSignature,
                pool.candidateProfileId,
                pool.candidateDataMaturity,
                pool.candidateProfileCanonicalSignature,
                active);
        }

        private static C1FormalRealtimeBattleSessionRequest
            CopyStage3To5Request(
                C1FormalRealtimeBattleSessionRequest source,
                string suffix,
                string productContext = null,
                string stageId = null,
                string balanceProfileId = null,
                string encounterVariantId = null,
                string expectedEnemyCatalogCanonical = null,
                long? launchGeneration = null,
                long? resetGeneration = null,
                bool preserveSessionIdentity = false)
        {
            return new C1FormalRealtimeBattleSessionRequest(
                source.schemaId,
                productContext ?? source.productContext,
                source.chapterId,
                stageId ?? source.stageId,
                balanceProfileId ?? source.balanceProfileId,
                encounterVariantId ?? source.encounterVariantId,
                source.launchId + "." + suffix,
                launchGeneration ?? source.launchGeneration,
                preserveSessionIdentity
                    ? source.sessionId
                    : source.sessionId + "." + suffix,
                preserveSessionIdentity
                    ? source.sessionToken
                    : source.sessionToken + "." + suffix,
                resetGeneration ?? source.resetGeneration,
                source.itemBattleInputCanonical,
                source.arrangementCanonicalSignature,
                source.expectedItemCatalogCanonical,
                expectedEnemyCatalogCanonical
                    ?? source.expectedEnemyCatalogCanonical,
                source.itemFacts,
                source.pool15ItemInputCanonical,
                source.pool15PoolCanonicalSignature,
                source.pool15SourceItemCatalogCanonicalSignature,
                source.pool15CandidateProfileId,
                source.pool15CandidateDataMaturity,
                source.pool15CandidateProfileCanonicalSignature,
                source.pool15ActiveItemFacts);
        }

        private static C1FormalRealtimeBattleSessionRequest
            CopyPool15ActiveFacts(
                C1FormalRealtimeBattleSessionRequest source,
                IEnumerable<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot> facts)
        {
            return new C1FormalRealtimeBattleSessionRequest(
                source.schemaId,
                source.productContext,
                source.chapterId,
                source.stageId,
                source.balanceProfileId,
                source.encounterVariantId,
                source.launchId,
                source.launchGeneration,
                source.sessionId,
                source.sessionToken,
                source.resetGeneration,
                source.itemBattleInputCanonical,
                source.arrangementCanonicalSignature,
                source.expectedItemCatalogCanonical,
                source.expectedEnemyCatalogCanonical,
                source.itemFacts,
                source.pool15ItemInputCanonical,
                source.pool15PoolCanonicalSignature,
                source.pool15SourceItemCatalogCanonicalSignature,
                source.pool15CandidateProfileId,
                source.pool15CandidateDataMaturity,
                source.pool15CandidateProfileCanonicalSignature,
                facts);
        }

        private static void VerifyLifecycleAndFailClosed(
            VerificationContext context)
        {
            ScenarioFixture fixture = CreateFixture("lifecycle", "1-2", true, 1L, 0L);
            C1FormalRealtimeBattleSession session = fixture.Session;
            bool started = session.TryStart(fixture.Request, out _);
            bool advanced = started && session.TryAdvanceBy(1000L, out _);
            long beforePause = session.BattleTimeMs;
            bool paused = session.TryPause(out C1FormalRealtimeBattleTickResult pauseTick);
            bool hiddenTickRejected = !session.TryAdvanceBy(1000L, out _)
                && session.BattleTimeMs == beforePause;
            bool resumed = session.TryResume(out C1FormalRealtimeBattleTickResult resumeTick);
            bool resumedAdvance = session.TryAdvanceBy(1000L, out _)
                && session.BattleTimeMs == beforePause + 1000L;
            if (pauseTick != null)
            {
                foreach (C1FormalRealtimeBattleCue cue in pauseTick.emittedCues)
                {
                    context.SeenCueKinds.Add(cue.cueKind);
                }
            }

            if (resumeTick != null)
            {
                foreach (C1FormalRealtimeBattleCue cue in resumeTick.emittedCues)
                {
                    context.SeenCueKinds.Add(cue.cueKind);
                }
            }

            context.Check(
                "pause-resume-no-hidden-tick",
                "lifecycle",
                advanced && paused && hiddenTickRejected && resumed && resumedAdvance,
                "paused time unchanged; resumed time explicit",
                session.BattleTimeMs.ToString(CultureInfo.InvariantCulture),
                "PAUSE_RESUME_LIFECYCLE_FAIL");

            C1FormalRealtimeBattleSessionStateSnapshot active = null;
            session.TryGetSnapshot(out active);
            bool wrongTokenRejected = !session.TryAdvanceBy(
                active.sessionId,
                "wrong-token",
                active.sessionGeneration,
                active.resetGeneration,
                1L,
                out _);
            bool staleSessionRejected = !session.TryAdvanceBy(
                active.sessionId,
                fixture.Request.sessionToken,
                active.sessionGeneration - 1L,
                active.resetGeneration,
                1L,
                out _);
            bool staleResetRejected = !session.TryAdvanceBy(
                active.sessionId,
                fixture.Request.sessionToken,
                active.sessionGeneration,
                active.resetGeneration + 1L,
                1L,
                out _);
            bool duplicateStartRejected = !session.TryStart(fixture.Request, out _);
            bool reset = session.TryReset(
                active.sessionId,
                fixture.Request.sessionToken,
                active.sessionGeneration,
                active.resetGeneration,
                out C1FormalRealtimeBattleTickResult resetTick);
            bool resetCue = resetTick != null && resetTick.emittedCues.Any(cue =>
                string.Equals(
                    cue.cueKind,
                    C1FormalRealtimeBattleCueKinds.SessionReset,
                    StringComparison.Ordinal));
            if (resetTick != null)
            {
                foreach (C1FormalRealtimeBattleCue cue in resetTick.emittedCues)
                {
                    context.SeenCueKinds.Add(cue.cueKind);
                }
            }

            bool staleAfterResetRejected = !session.TryAdvanceBy(
                active.sessionId,
                fixture.Request.sessionToken,
                active.sessionGeneration,
                active.resetGeneration,
                1L,
                out _);
            ScenarioFixture rebound = CreateFixture(
                "lifecycle-rebound",
                "1-2",
                true,
                2L,
                1L);
            bool reboundAccepted = session.TryStart(rebound.Request, out var reboundStart)
                && reboundStart != null
                && reboundStart.accepted
                && !reboundStart.stateSnapshot.terminal;
            context.Check(
                "session-envelope-dedupe-reset-rebind",
                "lifecycle",
                wrongTokenRejected
                && staleSessionRejected
                && staleResetRejected
                && duplicateStartRejected
                && reset
                && resetCue
                && staleAfterResetRejected
                && reboundAccepted,
                "all stale/duplicate envelopes fail; new generation rebinds",
                "wrongToken=" + wrongTokenRejected +
                " staleSession=" + staleSessionRejected +
                " staleReset=" + staleResetRejected +
                " reset=" + reset +
                " rebound=" + reboundAccepted,
                "SESSION_LIFECYCLE_DEDUPE_FAIL");

            ScenarioFixture wrongContext = CreateFixture(
                "wrong-context",
                "1-1",
                false,
                1L,
                0L,
                "DEV_SHOWCASE_LV40");
            bool wrongContextRejected = !new C1FormalRealtimeBattleSession()
                .TryStart(wrongContext.Request, out _);
            context.Check(
                "wrong-product-context-fails-closed",
                "negative-fixture",
                wrongContextRejected,
                "rejected",
                wrongContextRejected ? "rejected" : "accepted",
                "CONTEXT_REJECTED");
        }

        private static ScenarioFixture CreateFixture(
            string suffix,
            string stageId,
            bool i002Lit,
            long generation,
            long resetGeneration,
            string productContext = null)
        {
            BattleLaunchContext launch = CreateContext(suffix, stageId, generation);
            C1FormalRealtimeBattleSessionRequest request = BuildRequest(
                launch,
                suffix,
                i002Lit,
                resetGeneration,
                productContext);
            return new ScenarioFixture(
                new C1FormalRealtimeBattleSession(),
                request);
        }

        private static C1FormalRealtimeBattleSessionRequest BuildRequest(
            BattleLaunchContext launch,
            string suffix,
            bool i002Lit,
            long resetGeneration,
            string productContext = null,
            bool includeStageTwoI002Fact = true)
        {
            List<C1FormalRealtimeBattleItemFactSnapshot> facts =
                new List<C1FormalRealtimeBattleItemFactSnapshot>
                {
                    CreateItemFact(
                        "I001",
                        "I001@white",
                        true)
                };
            if (launch.StageId == C1Lv1EarlyEncounterBalanceContract.Stage1_2
                && includeStageTwoI002Fact
                && i002Lit)
            {
                facts.Add(CreateItemFact(
                    "I002",
                    "I002@white",
                    i002Lit));
            }

            return new C1FormalRealtimeBattleSessionRequest(
                C1FormalRealtimeBattleSessionContract.RequestSchemaId,
                productContext ?? launch.ProductContext,
                launch.ChapterId,
                launch.StageId,
                launch.BalanceProfileId,
                launch.EncounterVariantId,
                launch.LaunchId,
                launch.Generation,
                "verifier.live.session." + suffix,
                "verifier.live.token." + suffix,
                resetGeneration,
                "verifier.formal-item-input." + suffix,
                CanonicalItemCatalogContract.CatalogId,
                C1Lv1EarlyEncounterBalanceCatalog.canonicalSignature,
                facts);
        }

        private static C1FormalRealtimeBattleSessionRequest CopyRequestWithFacts(
            C1FormalRealtimeBattleSessionRequest source,
            string suffix,
            IEnumerable<C1FormalRealtimeBattleItemFactSnapshot> itemFacts)
        {
            return new C1FormalRealtimeBattleSessionRequest(
                source.schemaId,
                source.productContext,
                source.chapterId,
                source.stageId,
                source.balanceProfileId,
                source.encounterVariantId,
                source.launchId + "." + suffix,
                source.launchGeneration,
                source.sessionId + "." + suffix,
                source.sessionToken + "." + suffix,
                source.resetGeneration,
                source.itemBattleInputCanonical,
                source.expectedItemCatalogCanonical,
                source.expectedEnemyCatalogCanonical,
                itemFacts);
        }

        private static C1FormalRealtimeBattleItemFactSnapshot CreateItemFact(
            string baseItemId,
            string rarityVersionKey,
            bool isLit)
        {
            C1Lv1StarterItemProjectionResult result =
                C1Lv1StarterItemBaselineCatalog.TryGetProjection(
                    C1FormalRealtimeBattleSessionContract.ProductContext,
                    baseItemId,
                    rarityVersionKey,
                    C1Lv1StarterItemBaselineCatalog.ProfileRevision);
            if (result == null || !result.isSuccess || result.projection == null)
            {
                throw new InvalidOperationException(
                    "VERIFIER_ITEM_PROJECTION_MISSING " + rarityVersionKey);
            }

            C1Lv1StarterItemFactProjection projection = result.projection;
            string itemInstanceId = string.Equals(
                    baseItemId,
                    "I001",
                    StringComparison.Ordinal)
                ? CanonicalInitialItemAcquisitionPolicy.ItemInstanceId
                : string.Equals(
                    baseItemId,
                    "I002",
                    StringComparison.Ordinal)
                    ? RewardVerifierItemInstanceId
                    : "verifier.item.instance." + baseItemId;
            return new C1FormalRealtimeBattleItemFactSnapshot(
                rarityVersionKey,
                itemInstanceId,
                baseItemId,
                rarityVersionKey,
                isLit,
                projection.directDamage,
                decimal.ToInt64(projection.cooldownSeconds * 1000m),
                projection.canonicalSignature,
                C1Lv1StarterItemBaselineCatalog.canonicalSignature);
        }

        private static BattleLaunchContext CreateContext(
            string suffix,
            string stageId,
            long generation,
            string productContext = null)
        {
            bool stageOne = stageId ==
                C1Lv1EarlyEncounterBalanceContract.Stage1_1;
            bool stageThreeToFive = stageId ==
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3
                || stageId ==
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4
                || stageId ==
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5;
            string encounterVariant = stageOne
                ? C1Lv1EarlyEncounterBalanceContract.EncounterVariant1_1
                : stageId ==
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3
                    ? C1Stage3To5PlaytestEnemyEncounterContract
                        .EncounterVariant1_3
                    : stageId ==
                        C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4
                        ? C1Stage3To5PlaytestEnemyEncounterContract
                            .EncounterVariant1_4
                        : stageId ==
                            C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5
                            ? C1Stage3To5PlaytestEnemyEncounterContract
                                .EncounterVariant1_5
                            : C1Lv1EarlyEncounterBalanceContract
                                .EncounterVariant1_2;
            return new BattleLaunchContext(
                BattleLaunchContext.SchemaId,
                BattleLaunchContext.CurrentSchemaVersion,
                "verifier.launch." + suffix,
                "verifier.launch-token." + suffix,
                generation,
                productContext
                    ?? C1FormalRealtimeBattleSessionContract.ProductContext,
                C1FormalItemSessionContract.ChapterId,
                stageId,
                stageThreeToFive
                    ? C1Stage3To5PlaytestEnemyEncounterContract.BalanceProfileId
                    : C1Lv1EarlyEncounterBalanceContract.BalanceProfileId,
                encounterVariant,
                "campaign.normal.lv1.theme.c1",
                "campaign.normal.lv1.enemy-presentation.c1",
                "WorldMap",
                "WorldMap");
        }

        private static string Absolute(
            VerificationContext context,
            string relative)
        {
#if C1_FORMAL_REALTIME_BATTLE_SESSION_SCOPED_STANDALONE
            string projectRoot = Path.GetFullPath(Environment.CurrentDirectory);
#else
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
#endif
            return Path.GetFullPath(Path.Combine(
                projectRoot,
                relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static int CountOrdinal(string source, string token)
        {
            int count = 0;
            int index = 0;
            while (!string.IsNullOrEmpty(source)
                   && !string.IsNullOrEmpty(token)
                   && (index = source.IndexOf(
                       token,
                       index,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        private static string Slug(string value)
        {
            return string.Concat((value ?? string.Empty).Select(character =>
                char.IsLetterOrDigit(character)
                    ? char.ToLowerInvariant(character)
                    : '-')).Trim('-');
        }

        private sealed class VerificationContext
        {
            internal readonly List<CheckRow> Checks = new List<CheckRow>();
            internal readonly List<ScenarioTrace> Traces =
                new List<ScenarioTrace>();
            internal readonly List<AdmissionEvidence> Admissions =
                new List<AdmissionEvidence>();
            internal readonly List<RejectionEvidence> Rejections =
                new List<RejectionEvidence>();
            internal readonly List<Pool15CoverageEvidence> PoolCoverage =
                new List<Pool15CoverageEvidence>();
            internal readonly List<Pool15FilteringEvidence> Filtering =
                new List<Pool15FilteringEvidence>();
            internal readonly List<Pool15SensitivityEvidence> Sensitivity =
                new List<Pool15SensitivityEvidence>();
            internal readonly List<RejectionEvidence> PoolRejections =
                new List<RejectionEvidence>();
            internal readonly List<StageExtensionEvidence> StageExtensions =
                new List<StageExtensionEvidence>();
            internal readonly List<RejectionEvidence> StageRejections =
                new List<RejectionEvidence>();
            internal readonly HashSet<string> RequiredCueKinds =
                new HashSet<string>(StringComparer.Ordinal);
            internal readonly HashSet<string> SeenCueKinds =
                new HashSet<string>(StringComparer.Ordinal);
            internal int FailureCount => Checks.Count(row => !row.Passed);

            internal void Check(
                string name,
                string category,
                bool passed,
                string expected,
                string actual,
                string code)
            {
                Checks.Add(new CheckRow(
                    name,
                    category,
                    passed,
                    expected,
                    actual,
                    code));
            }
        }

        private sealed class CheckRow
        {
            internal CheckRow(
                string name,
                string category,
                bool passed,
                string expected,
                string actual,
                string code)
            {
                Name = name;
                Category = category;
                Passed = passed;
                Expected = expected;
                Actual = actual;
                Code = code;
            }

            internal string Name { get; }
            internal string Category { get; }
            internal bool Passed { get; }
            internal string Expected { get; }
            internal string Actual { get; }
            internal string Code { get; }
        }

        private sealed class Pool15ScenarioEvidence
        {
            internal Pool15ScenarioEvidence(
                string scenario,
                C1FormalItemBattleInputSnapshot formalSnapshot,
                C1FormalRealtimeBattleSessionAdapter adapter,
                C1FormalRealtimeBattleSessionStartSnapshot start,
                bool started)
            {
                Scenario = scenario;
                FormalSnapshot = formalSnapshot;
                Adapter = adapter;
                Start = start;
                Started = started;
            }

            internal string Scenario { get; }
            internal C1FormalItemBattleInputSnapshot FormalSnapshot { get; }
            internal C1FormalRealtimeBattleSessionAdapter Adapter { get; }
            internal C1FormalRealtimeBattleSessionStartSnapshot Start { get; }
            internal bool Started { get; }
        }

        private sealed class Pool15CoverageEvidence
        {
            internal Pool15CoverageEvidence(
                string baseItemId,
                string familyId,
                string variantId,
                string triggerId,
                string conditionId,
                string nativeUnitId,
                C1FormalRealtimeBattleEffectApplicationSnapshot application,
                bool passed)
            {
                BaseItemId = baseItemId;
                FamilyId = familyId;
                VariantId = variantId;
                TriggerId = triggerId;
                ConditionId = conditionId;
                NativeUnitId = nativeUnitId;
                Application = application;
                Passed = passed;
            }

            internal string BaseItemId { get; }
            internal string FamilyId { get; }
            internal string VariantId { get; }
            internal string TriggerId { get; }
            internal string ConditionId { get; }
            internal string NativeUnitId { get; }
            internal C1FormalRealtimeBattleEffectApplicationSnapshot Application
            {
                get;
            }
            internal bool Passed { get; }

            internal string Describe()
            {
                return Application == null
                    ? "application missing"
                    : Application.familyId + "/"
                      + Application.nativeUnitId + "/"
                      + Application.targetBefore + "->"
                      + Application.targetAfter + "/"
                      + Application.candidateDataMaturity;
            }
        }

        private sealed class Pool15FilteringEvidence
        {
            internal Pool15FilteringEvidence(
                string scenario,
                int expectedActiveCount,
                int actualActiveCount,
                bool passed)
            {
                Scenario = scenario;
                ExpectedActiveCount = expectedActiveCount;
                ActualActiveCount = actualActiveCount;
                Passed = passed;
            }

            internal string Scenario { get; }
            internal int ExpectedActiveCount { get; }
            internal int ActualActiveCount { get; }
            internal bool Passed { get; }
        }

        private sealed class Pool15SensitivityEvidence
        {
            internal Pool15SensitivityEvidence(
                string layout,
                int outerTickMs,
                int acceptedCount,
                string sourceOrder,
                string resolveTimes,
                bool passed)
            {
                Layout = layout;
                OuterTickMs = outerTickMs;
                AcceptedCount = acceptedCount;
                SourceOrder = sourceOrder;
                ResolveTimes = resolveTimes;
                Passed = passed;
            }

            internal string Layout { get; }
            internal int OuterTickMs { get; }
            internal int AcceptedCount { get; }
            internal string SourceOrder { get; }
            internal string ResolveTimes { get; }
            internal bool Passed { get; }
        }

        private sealed class AdmissionEvidence
        {
            internal AdmissionEvidence(
                string scenario,
                int expectedActiveCount,
                C1FormalRealtimeBattleSessionStartSnapshot start)
            {
                Scenario = scenario;
                ExpectedActiveCount = expectedActiveCount;
                Start = start;
            }

            internal string Scenario { get; }
            internal int ExpectedActiveCount { get; }
            internal C1FormalRealtimeBattleSessionStartSnapshot Start { get; }
            internal bool StartedNonTerminal;
            internal int ScheduledItemCount;
            internal int TickCount;
            internal C1FormalRealtimeBattleTerminalResult Terminal;
            internal readonly List<C1FormalRealtimeBattleCue> Cues =
                new List<C1FormalRealtimeBattleCue>();

            internal string Describe()
            {
                return "start=" + (StartedNonTerminal ? "accepted" : "rejected")
                    + ";active=" + ScheduledItemCount
                    + ";ticks=" + TickCount
                    + ";terminal=" + (Terminal == null
                        ? "missing"
                        : (Terminal.win ? "victory" : Terminal.lose
                            ? "defeat"
                            : "fault")
                          + "@" + Terminal.durationMs + "ms"
                          + "/enemy=" + Terminal.enemyApplicationCount
                          + "/playerHp=" + Terminal.playerSnapshot.currentHp
                          + "/item=" + Terminal.acceptedItemApplicationCount);
            }
        }

        private sealed class RejectionEvidence
        {
            internal RejectionEvidence(
                string scenario,
                bool rejected,
                string errorCode)
            {
                Scenario = scenario;
                Rejected = rejected;
                ErrorCode = errorCode ?? string.Empty;
            }

            internal string Scenario { get; }
            internal bool Rejected { get; }
            internal string ErrorCode { get; }
        }

        private sealed class StageExtensionEvidence
        {
            internal StageExtensionEvidence(
                string stageId,
                int expectedPool15Active,
                C1FormalRealtimeBattleSessionAdapter adapter,
                C1FormalRealtimeBattleSessionStartSnapshot start,
                bool startedNonTerminal)
            {
                StageId = stageId;
                ExpectedPool15Active = expectedPool15Active;
                Adapter = adapter;
                Start = start;
                StartedNonTerminal = startedNonTerminal;
            }

            internal string StageId { get; }
            internal int ExpectedPool15Active { get; }
            internal C1FormalRealtimeBattleSessionAdapter Adapter { get; }
            internal C1FormalRealtimeBattleSessionStartSnapshot Start { get; }
            internal bool StartedNonTerminal { get; }
            internal int TickCount;
            internal C1FormalRealtimeBattleSessionStateSnapshot FinalState;
            internal C1FormalRealtimeBattleTerminalResult Terminal;
            internal readonly List<C1FormalRealtimeBattleCue> Cues =
                new List<C1FormalRealtimeBattleCue>();

            internal string Describe()
            {
                C1FormalRealtimeBattleSessionStateSnapshot startState =
                    Start == null ? null : Start.stateSnapshot;
                return "start=" + (StartedNonTerminal
                        ? "accepted-non-terminal"
                        : "rejected")
                    + ";active=" + (startState == null
                        ? -1
                        : startState.activePool15SourceCount)
                    + ";actors=" + (startState == null
                        ? -1
                        : startState.actorSnapshots.Count)
                    + ";ticks=" + TickCount
                    + ";terminal=" + (Terminal == null
                        ? "missing"
                        : (Terminal.win ? "victory" : Terminal.lose
                            ? "defeat"
                            : "fault") + "@" + Terminal.durationMs + "ms"
                          + "/enemy=" + Terminal.enemyApplicationCount
                          + "/playerHp="
                          + Terminal.playerSnapshot.currentHp);
            }
        }

        private sealed class ScenarioTrace
        {
            internal ScenarioTrace(
                string scenario,
                C1FormalRealtimeBattleSessionRequest request,
                C1FormalRealtimeBattleSession session,
                C1FormalRealtimeBattleSessionStartSnapshot startSnapshot)
            {
                Scenario = scenario;
                Request = request;
                Session = session;
                StartSnapshot = startSnapshot;
            }

            internal string Scenario { get; }
            internal C1FormalRealtimeBattleSessionRequest Request { get; }
            internal C1FormalRealtimeBattleSession Session { get; }
            internal C1FormalRealtimeBattleSessionStartSnapshot StartSnapshot { get; }
            internal readonly List<C1FormalRealtimeBattleCue> Cues =
                new List<C1FormalRealtimeBattleCue>();
            internal C1FormalRealtimeBattleSessionStateSnapshot FinalState;
            internal C1FormalRealtimeBattleTerminalResult Terminal;
            internal int TickCount;
            internal bool StartedNonImmediate;
            internal bool TickAfterTerminalRejected;
        }

        private sealed class ScenarioFixture
        {
            internal ScenarioFixture(
                C1FormalRealtimeBattleSession session,
                C1FormalRealtimeBattleSessionRequest request)
            {
                Session = session;
                Request = request;
            }

            internal C1FormalRealtimeBattleSession Session { get; }
            internal C1FormalRealtimeBattleSessionRequest Request { get; }
        }
#endif

        private static C1FormalItemBattleInputSnapshot CreateStageTwoItemSnapshot(
            string suffix,
            string itemSessionCanonical,
            IEnumerable<C1FormalItemBattleItemRow> itemRows)
        {
            string arrangementCanonical = "verifier.arrangement." + suffix;
            string itemSystemCanonical = "verifier.item-system." + suffix;
            Vector2Int sourceAnchor = Vector2Int.zero;
            ConstructorInfo capacityConstructor = typeof(
                    C1FormalI031NianCapacityFact)
                .GetConstructors(
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .Single();
            C1FormalI031NianCapacityFact capacity =
                (C1FormalI031NianCapacityFact)capacityConstructor.Invoke(
                    new object[]
                    {
                        I031InventoryPlacementContract.SpecialIdentityId,
                        I031InventoryPlacementContract.ItemId,
                        I031InventoryPlacementContract.StablePlacementId,
                        sourceAnchor,
                        itemSessionCanonical,
                        arrangementCanonical,
                        itemSystemCanonical
                    });
            ConstructorInfo constructor = typeof(C1FormalItemBattleInputSnapshot)
                .GetConstructors(
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .Single();
            return (C1FormalItemBattleInputSnapshot)constructor.Invoke(
                new object[]
                {
                    itemSessionCanonical,
                    arrangementCanonical,
                    itemSystemCanonical,
                    I031InventoryPlacementContract.StablePlacementId,
                    (Vector2Int?)sourceAnchor,
                    capacity,
                    itemRows
                });
        }

        private static bool TryStartCumulativeItemSnapshot(
            string scenario,
            string stageId,
            long generation,
            IEnumerable<C1FormalItemBattleItemRow> rows,
            out C1FormalRealtimeBattleSessionAdapter adapter,
            out C1FormalItemBattleInputSnapshot itemSnapshot,
            out C1FormalRealtimeBattleSessionStartSnapshot start)
        {
            string itemSessionCanonical =
                "verifier.cumulative.item-session." + scenario;
            itemSnapshot = CreateStageTwoItemSnapshot(
                "cumulative-" + scenario,
                itemSessionCanonical,
                rows ?? Array.Empty<C1FormalItemBattleItemRow>());
            adapter = new C1FormalRealtimeBattleSessionAdapter();
            return adapter.TryStartFromCumulativeItemSnapshot(
                CreateContext(scenario, stageId, generation),
                itemSnapshot,
                itemSessionCanonical,
                "verifier.cumulative.session." + scenario,
                "verifier.cumulative.token." + scenario,
                out start);
        }

        private static bool SetSessionGuardForBoundaryVerification(
            C1FormalRealtimeBattleSession session,
            long currentGuard)
        {
            FieldInfo field = typeof(C1FormalRealtimeBattleSession).GetField(
                "playerGuard",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || session == null)
            {
                return false;
            }
            field.SetValue(session, currentGuard);
            return true;
        }

        private static bool SetSessionNianForBoundaryVerification(
            C1FormalRealtimeBattleSession session,
            int currentNian)
        {
            FieldInfo field = typeof(C1FormalRealtimeBattleSession).GetField(
                "currentNian",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null || session == null)
            {
                return false;
            }
            field.SetValue(session, currentNian);
            return true;
        }

        private static BattleLaunchContext CreateContext(
            string suffix,
            string stageId,
            long generation)
        {
            return new BattleLaunchContext(
                BattleLaunchContext.SchemaId,
                BattleLaunchContext.CurrentSchemaVersion,
                "verifier.launch." + suffix,
                "verifier.launch-token." + suffix,
                generation,
                C1FormalRealtimeBattleSessionContract.ProductContext,
                C1FormalItemSessionContract.ChapterId,
                stageId,
                TalismanBag.V04.Campaign.Chapter1
                    .Chapter1CampaignStageConfig.BalanceProfileIdValue,
                TalismanBag.V04.Campaign.Chapter1
                    .Chapter1CampaignStageConfig.EncounterVariantIdFor(stageId),
                "campaign.normal.lv1.theme.c1",
                TalismanBag.V04.Campaign.Chapter1
                    .Chapter1CampaignStageConfig
                    .EnemyPresentationProfileIdFor(stageId),
                "WorldMap",
                "WorldMap");
        }

        private sealed class VerificationContext
        {
            internal readonly List<CheckRow> Checks = new List<CheckRow>();
            internal int FailureCount => Checks.Count(row => !row.Passed);

            internal void Check(
                string name,
                string category,
                bool passed,
                string expected,
                string actual,
                string code)
            {
                Checks.Add(new CheckRow(
                    name,
                    category,
                    passed,
                    expected,
                    actual,
                    code));
            }
        }

        private sealed class CheckRow
        {
            internal CheckRow(
                string name,
                string category,
                bool passed,
                string expected,
                string actual,
                string code)
            {
                Name = name;
                Category = category;
                Passed = passed;
                Expected = expected;
                Actual = actual;
                Code = code;
            }

            internal string Name { get; }
            internal string Category { get; }
            internal bool Passed { get; }
            internal string Expected { get; }
            internal string Actual { get; }
            internal string Code { get; }
        }
    }
}

