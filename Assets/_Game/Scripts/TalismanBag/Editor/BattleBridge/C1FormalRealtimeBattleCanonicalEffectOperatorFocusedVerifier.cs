using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TalismanBag.BattleBridge.Formal;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.Items.Balance;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.BattleBridge
{
    public static class
        C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier
    {
        private const string CatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ProofPrefix =
            "verifier.canonical-effect-runtime-proof";

        [MenuItem(
            "Tools/TalismanBag/QA/Canonical Effect Operator Focused Verify")]
        public static void RunFromMenu()
        {
            int verified = RunOrThrow();
            Debug.Log(
                "[CanonicalEffectRuntimeProof] COMPLETE checks=" + verified);
        }

        public static void RunBatch()
        {
            try
            {
                RunFromMenu();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[CanonicalEffectRuntimeProof] BATCH_FAILED "
                    + exception);
                EditorApplication.Exit(1);
            }
        }

        internal static void VerifyDirectDamageFeedbackFactOrThrow()
        {
            CanonicalItemDefinitionResolver resolver = LoadResolver();
            CanonicalItemDefinition definition = resolver
                .OrdinaryDropDefinitions.FirstOrDefault(value =>
                    string.Equals(
                        value.primaryStatId,
                        "damage",
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.combatEffect?.targetStatId,
                        "damage",
                        StringComparison.Ordinal));
            RequireSemantic(definition != null,
                "Canonical Catalog has no ordinary direct-damage definition.");
            C1FormalRealtimeBattleItemFactSnapshot fact = CreateItemFact(
                "feedback-direct-damage",
                definition,
                10,
                0,
                0,
                0,
                1,
                5L,
                true,
                true,
                42001L);
            C1FormalRealtimeBattleSession session = StartSession(
                "feedback-direct-damage",
                new[] { fact },
                42001L);
            Require(session.TryAdvanceTo(2000L, out _),
                "Direct-damage Item action did not resolve.");
            C1FormalRealtimeBattleCue cue = session.CueLedger
                .FirstOrDefault(value => value.cueKind ==
                        C1FormalRealtimeBattleCueKinds.DamageFloatPayload
                    && value.sourceItemInstanceId == fact.itemInstanceId
                    && value.appliedDamage > 0);
            Require(cue != null
                    && cue.triggerKind ==
                        C1FormalRealtimeBattleFeedbackTriggerKinds.ItemTrigger
                    && cue.deliveryKind ==
                        C1FormalRealtimeBattleFeedbackDeliveryKinds.Instant
                    && cue.resultKind ==
                        C1FormalRealtimeBattleFeedbackResultKinds.HpDamage
                    && cue.targetStableOrder >= 0
                    && cue.acceptedApplicationEventId.Length > 0,
                "Canonical direct-damage Item feedback fact is incomplete.");
        }

        public static int RunOrThrow()
        {
            CanonicalItemDefinitionResolver resolver = LoadResolver();
            int grammarCount = VerifyGrammarCoverage(resolver);
            IReadOnlyList<ProofResult> results = RunRuntimeProofs(resolver);
            foreach (ProofResult result in results)
            {
                Debug.Log(
                    "[CanonicalEffectRuntimeProof] " + result.Name + "="
                    + result.Classification + " detail=" + result.Detail);
            }

            ProofResult[] gaps = results.Where(result =>
                    result.Classification != ProofClassification.Pass)
                .ToArray();
            if (gaps.Length > 0)
            {
                throw new InvalidOperationException(
                    "Canonical Effect Runtime Proof found gaps: "
                    + string.Join(", ", gaps.Select(result =>
                        result.Name + "=" + result.Classification + "("
                        + result.Detail + ")")));
            }
            return grammarCount + results.Count;
        }

        private static CanonicalItemDefinitionResolver LoadResolver()
        {
            ItemBalanceWorkbenchCatalog catalog = LoadCatalog();
            Require(CanonicalItemDefinitionResolver.TryCreate(
                    catalog,
                    out CanonicalItemDefinitionResolver resolver,
                    out IReadOnlyList<string> errors),
                "Canonical Item catalog is invalid: "
                + string.Join(",", errors ?? Array.Empty<string>()));
            return resolver;
        }

        private static ItemBalanceWorkbenchCatalog LoadCatalog()
        {
            ItemBalanceWorkbenchCatalog catalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    CatalogPath);
            Require(catalog != null, "Canonical Item catalog is missing.");
            return catalog;
        }

        private static void RequireWhiteLoadRanges(ItemBalanceProfile profile)
        {
            ItemBalanceRarityVersion white =
                profile?.FindVersion(ItemInstanceRarity.White);
            ItemBalanceRange nian = white?.FindRange("nianCost");
            ItemBalanceRange cooldown = white?.FindRange("cooldown");
            Require(white != null
                    && nian != null
                    && cooldown != null
                    && nian.maxUnits > 0L
                    && cooldown.minUnits > 0L,
                "Canonical white load ranges are invalid: "
                + (profile?.baseItemId ?? "missing"));
        }

        private static decimal MaximumWhiteLoadPerSecond(
            ItemBalanceProfile profile)
        {
            ItemBalanceRarityVersion white =
                profile.FindVersion(ItemInstanceRarity.White);
            return white.FindRange("nianCost").maxUnits
                   / (white.FindRange("cooldown").minUnits * 0.4m);
        }

        private static int VerifyGrammarCoverage(
            CanonicalItemDefinitionResolver resolver)
        {
            HashSet<string> grammarShapes = new HashSet<string>(
                StringComparer.Ordinal);
            HashSet<C1FormalRealtimeBattleCanonicalMutationKind> kinds =
                new HashSet<
                    C1FormalRealtimeBattleCanonicalMutationKind>();
            for (int index = 1;
                 index <= CanonicalItemCatalogContract.OrdinaryItemCount;
                 index++)
            {
                string baseItemId = "I" + index.ToString("D3");
                CanonicalItemDefinition definition =
                    resolver.GetDefinition(baseItemId);
                Require(definition != null
                        && definition.isOrdinaryDropEligible
                        && definition.combatEffect != null,
                    "Ordinary canonical definition is incomplete: "
                    + baseItemId);
                Require(C1FormalRealtimeBattleCanonicalEffectOperator
                        .TryResolve(
                            definition.combatEffect,
                            out C1FormalRealtimeBattleCanonicalEffectPlan plan),
                    "Formal Battle has no grammar operator for " + baseItemId
                    + ": " + GrammarKey(definition.combatEffect));
                grammarShapes.Add(GrammarKey(definition.combatEffect));
                kinds.Add(plan.MutationKind);
            }

            Require(grammarShapes.Count == 22,
                "Canonical Catalog grammar shape count changed: "
                + grammarShapes.Count);
            C1FormalRealtimeBattleCanonicalMutationKind[] requiredKinds =
                Enum.GetValues(
                        typeof(C1FormalRealtimeBattleCanonicalMutationKind))
                    .Cast<C1FormalRealtimeBattleCanonicalMutationKind>()
                    .Where(value => value !=
                        C1FormalRealtimeBattleCanonicalMutationKind.Unsupported)
                    .ToArray();
            Require(requiredKinds.All(kinds.Contains),
                "One or more canonical grammar families are not reachable from the canonical Catalog: "
                + string.Join(",", requiredKinds.Where(value =>
                    !kinds.Contains(value))));
            return CanonicalItemCatalogContract.OrdinaryItemCount;
        }

        private static IReadOnlyList<ProofResult> RunRuntimeProofs(
            CanonicalItemDefinitionResolver resolver)
        {
            return new[]
            {
                RunProof("I004_EXTRA_TARGET", () =>
                    VerifyI004ExtraTarget(resolver)),
                RunProof("I007_AFTERGLOW", () =>
                    VerifyI007Afterglow(resolver)),
                RunProof("GUARD", () => VerifyGuard(resolver)),
                RunProof("I014_HORIZONTAL_GUARD", () =>
                    VerifyI014HorizontalGuard(resolver)),
                RunProof("HEAL", () => VerifyHeal(resolver)),
                RunProof("ENEMY_ACTION_DELAY", () =>
                    VerifyEnemyActionDelay(resolver)),
                RunProof("I031_BASE_LOAD_ENVELOPE", () =>
                    VerifyI031BaseLoadEnvelope(resolver)),
                RunProof("I031_FAIR_READY_RETRY", () =>
                    VerifyI031FairReadyRetry(resolver)),
                RunProof("CLEANSE_REAL_PLAYER_STATUS", () =>
                    VerifyCleanseRealPlayerStatus(resolver)),
                RunProof("CLEANSE_NO_STATUS_ZERO_BENEFIT_ACTIVATION", () =>
                    VerifyCleanseNoStatusZeroBenefitActivation(resolver)),
                RunProof("HEAL_FULL_HP_ZERO_BENEFIT_ACTIVATION", () =>
                    VerifyHealFullHpZeroBenefitActivation(resolver)),
                RunProof("CLEANSE_REWARD_CAPABILITY_ADMISSION", () =>
                    VerifyCleanseRewardCapabilityAdmission(resolver)),
                RunProof("TRAY_UNLIT", () => VerifyTrayUnlit(resolver))
            };
        }

        private static ProofResult RunProof(string name, Action proof)
        {
            try
            {
                proof();
                return new ProofResult(
                    name,
                    ProofClassification.Pass,
                    "authoritative runtime behavior satisfied");
            }
            catch (SemanticGapException exception)
            {
                return new ProofResult(
                    name,
                    ProofClassification.SemanticGap,
                    exception.Message);
            }
            catch (Exception exception)
            {
                return new ProofResult(
                    name,
                    ProofClassification.CodeGap,
                    exception.Message);
            }
        }

        private static void VerifyI004ExtraTarget(
            CanonicalItemDefinitionResolver resolver)
        {
            C1FormalRealtimeBattleItemFactSnapshot fact = CreateItemFact(
                "i004-extra-target",
                RequireDefinition(resolver, "I004"),
                10,
                0,
                0,
                0,
                1,
                5L,
                true,
                true,
                4004L);
            C1FormalRealtimeBattleSession session = StartSession(
                "i004-extra-target",
                new[] { fact },
                4004L);
            C1FormalRealtimeBattleSessionStateSnapshot before =
                Snapshot(session);
            Dictionary<string, C1FormalRealtimeBattleActorSnapshot> prior =
                ActorsByIdentity(before);
            int nianBefore = before.nianStateSnapshot.currentNian;

            Require(session.TryAdvanceTo(2000L, out _),
                "I004 action did not resolve.");
            C1FormalRealtimeBattleSessionStateSnapshot after =
                Snapshot(session);
            C1FormalRealtimeBattleActorSnapshot[] changed = after
                .actorSnapshots.Where(actor =>
                {
                    C1FormalRealtimeBattleActorSnapshot old =
                        prior[ActorIdentity(actor)];
                    return actor.currentHp < old.currentHp
                           || actor.currentShell < old.currentShell;
                }).ToArray();
            Require(changed.Length >= 2,
                "I004 did not mutate two real enemy actors.");
            Require(changed.Any(actor => actor.currentHp
                    < prior[ActorIdentity(actor)].currentHp),
                "I004 did not mutate real enemy HP.");
            Require(changed.Any(actor => actor.hasShell
                    && actor.currentShell
                    < prior[ActorIdentity(actor)].currentShell),
                "I004 extra target did not mutate real enemy Shell.");
            Require(after.nianStateSnapshot.currentNian == nianBefore - 1,
                "I004 real mutation did not commit its Nian spend.");
            Require(session.CueLedger.Count(cue => cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.DamageFloatPayload
                    && cue.sourceItemInstanceId == fact.itemInstanceId
                    && cue.appliedDamage > 0
                    && !string.IsNullOrWhiteSpace(cue.floatPayload)) >= 2,
                "I004 real multi-target damage did not emit readable damage payloads.");
        }

        private static void VerifyI007Afterglow(
            CanonicalItemDefinitionResolver resolver)
        {
            CanonicalItemDefinition definition =
                RequireDefinition(resolver, "I007");
            RequireSemantic(definition.combatEffect != null
                            && definition.combatEffect
                                .hasCompleteStatusSemantics,
                "I007 Afterglow status semantics are incomplete.");
            C1FormalRealtimeBattleSession session = StartSession(
                "i007-afterglow",
                new[]
                {
                    CreateItemFact(
                        "i007-afterglow",
                        definition,
                        10,
                        0,
                        0,
                        0,
                        1,
                        5L,
                        true,
                        false,
                        7007L)
                },
                7007L);
            Require(session.TryAdvanceTo(2000L, out _),
                "I007 action did not resolve.");
            Require(session.StatusSnapshots.Count == 1,
                "I007 did not create a real BattleStatusState.");
            C1FormalRealtimeBattleStatusSnapshot status =
                session.StatusSnapshots.Single();
            Require(Snapshot(session).statusSnapshots.Any(value =>
                    value.targetActorId == status.targetActorId
                    && value.targetStableOrder == status.targetStableOrder
                    && value.statusKey == status.statusKey
                    && value.stackCount == status.stackCount),
                "Enemy status was absent from the authoritative state snapshot.");
            long stateBeforeTick = EnemyHpAndShell(
                session,
                status.targetActorId);
            Require(session.TryAdvanceTo(3000L, out _),
                "I007 Afterglow tick did not resolve.");
            long stateAfterTick = EnemyHpAndShell(
                session,
                status.targetActorId);
            Require(stateAfterTick < stateBeforeTick,
                "I007 Afterglow tick did not mutate live HP/Shell.");
        }

        private static void VerifyGuard(
            CanonicalItemDefinitionResolver resolver)
        {
            C1FormalRealtimeBattleSession session = StartSession(
                "guard",
                new[]
                {
                    CreateItemFact(
                        "guard",
                        RequireDefinition(resolver, "I015"),
                        0,
                        0,
                        0,
                        0,
                        1,
                        5L,
                        true,
                        true,
                        15015L)
                },
                15015L);
            Require(Snapshot(session).playerSnapshot.guard > 0L,
                "Direct-lit Guard did not mutate live player Guard.");
        }

        private static void VerifyI014HorizontalGuard(
            CanonicalItemDefinitionResolver resolver)
        {
            CanonicalItemDefinition definition =
                RequireDefinition(resolver, "I014");
            Require(C1FormalRealtimeBattleCanonicalEffectOperator.TryResolve(
                    definition.combatEffect,
                    out C1FormalRealtimeBattleCanonicalEffectPlan plan)
                    && plan.MutationKind ==
                    C1FormalRealtimeBattleCanonicalMutationKind
                        .PlayerGuardPercent
                    && string.Equals(
                        plan.TriggerEventId,
                        "on_layout_evaluate",
                        StringComparison.Ordinal),
                "I014 must retain its Canonical horizontal layout Guard plan.");
            C1FormalRealtimeBattleItemFactSnapshot source = CreateItemFact(
                "i014-horizontal-guard",
                definition,
                0,
                0,
                0,
                0,
                1,
                5L,
                true,
                false,
                14014L);
            C1FormalRealtimeBattleItemFactSnapshot neighbor = CreateItemFact(
                "i014-horizontal-neighbor",
                RequireDefinition(resolver, "I007"),
                10,
                0,
                0,
                0,
                1,
                5L,
                true,
                false,
                7014L,
                1,
                0);
            C1FormalRealtimeBattleSession session = StartSession(
                "i014-horizontal-guard",
                new[] { source, neighbor },
                14014L,
                out C1FormalRealtimeBattleSessionStartSnapshot start);
            long expectedGuard =
                C1FormalRealtimeBattleCanonicalEffectOperator.ApplyBasisPoints(
                    C1FormalRealtimeBattleSessionContract.StartingPlayerMaxHp,
                    plan.ValueUnits);
            Require(expectedGuard > 0L
                    && Snapshot(session).playerSnapshot.guard == expectedGuard,
                "I014 horizontal adjacency did not mutate live player Guard.");
            C1FormalRealtimeBattleCue accepted = start.initialCues
                .SingleOrDefault(cue => cue != null
                    && string.Equals(
                        cue.cueKind,
                        C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceItemInstanceId,
                        source.itemInstanceId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.sourceBaseItemId,
                        "I014",
                        StringComparison.Ordinal)
                    && string.Equals(
                        cue.effectVariantId,
                        plan.EffectId,
                        StringComparison.Ordinal));
            Require(accepted != null
                    && !string.IsNullOrEmpty(
                        accepted.acceptedApplicationEventId)
                    && accepted.requestedDamage == expectedGuard
                    && accepted.appliedDamage == expectedGuard,
                "I014 Guard mutation lacks its same-instance accepted cue "
                + "and application identity.");
        }

        private static void VerifyHeal(
            CanonicalItemDefinitionResolver resolver)
        {
            C1FormalRealtimeBattleSession session = StartSession(
                "heal",
                new[]
                {
                    CreateItemFact(
                        "heal",
                        RequireDefinition(resolver, "I019"),
                        0,
                        0,
                        12,
                        0,
                        1,
                        15L,
                        true,
                        false,
                        19019L)
                },
                19019L);
            Require(session.TryAdvanceTo(4500L, out _),
                "Enemy damage did not resolve before Heal proof.");
            int damagedHp = Snapshot(session).playerSnapshot.currentHp;
            Require(damagedHp
                    < Snapshot(session).playerSnapshot.maxHp,
                "Heal proof did not establish real missing player HP.");
            Require(session.TryAdvanceTo(6000L, out _),
                "Heal item action did not resolve.");
            int healedHp = Snapshot(session).playerSnapshot.currentHp;
            Require(healedHp > damagedHp,
                "Canonical Heal did not mutate live player HP.");
        }

        private static void VerifyEnemyActionDelay(
            CanonicalItemDefinitionResolver resolver)
        {
            C1FormalRealtimeBattleSession session = StartSession(
                "enemy-action-delay",
                new[]
                {
                    CreateItemFact(
                        "enemy-action-delay",
                        RequireDefinition(resolver, "I025"),
                        10,
                        0,
                        0,
                        0,
                        1,
                        5L,
                        true,
                        false,
                        25025L)
                },
                25025L);
            Dictionary<string, long> enemyDueBefore = Snapshot(session)
                .scheduledActions.Where(IsPendingEnemyAction)
                .ToDictionary(action => action.actionId,
                    action => action.dueBattleTimeMs,
                    StringComparer.Ordinal);
            Require(enemyDueBefore.Count > 0,
                "Enemy Action Delay proof has no pending enemy action.");
            Require(session.TryAdvanceTo(2000L, out _),
                "I025 action did not resolve.");
            Dictionary<string, long> enemyDueAfter = Snapshot(session)
                .scheduledActions.Where(IsPendingEnemyAction)
                .ToDictionary(action => action.actionId,
                    action => action.dueBattleTimeMs,
                    StringComparer.Ordinal);
            Require(enemyDueBefore.Any(pair =>
                    enemyDueAfter.TryGetValue(pair.Key, out long after)
                    && after > pair.Value),
                "I025 did not delay a real pending enemy Action.");
        }

        private static void VerifyI031BaseLoadEnvelope(
            CanonicalItemDefinitionResolver resolver)
        {
            const int supportedItemCount = 5;
            ItemBalanceWorkbenchCatalog catalog = LoadCatalog();
            ItemBalanceProfile[] profiles = catalog.profiles
                .Where(profile => profile != null)
                .ToArray();
            Require(profiles.Length ==
                    CanonicalItemCatalogContract.OrdinaryItemCount,
                "I031 load envelope did not read every Canonical Item profile.");
            foreach (ItemBalanceProfile profile in profiles)
            {
                RequireWhiteLoadRanges(profile);
            }

            ItemBalanceProfile maximumLoadProfile = profiles
                .OrderByDescending(MaximumWhiteLoadPerSecond)
                .First();
            ItemBalanceRarityVersion maximumLoadVersion =
                maximumLoadProfile.FindVersion(ItemInstanceRarity.White);
            int maximumWhiteSustainedCost = checked((int)maximumLoadVersion
                .FindRange("nianCost").maxUnits);
            long maximumWhiteSustainedCooldownUnits = maximumLoadVersion
                .FindRange("cooldown").minUnits;
            int maximumWhiteBurstCost = checked((int)profiles.Max(profile =>
                profile.FindVersion(ItemInstanceRarity.White)
                    .FindRange("nianCost").maxUnits));
            long generatedPerSecond = checked(
                C1FormalI031NianCapacityProjection.GenerationAmount * 1000L
                / C1FormalI031NianCapacityProjection
                    .GenerationIntervalMilliseconds);
            long requiredSustainedSupply = checked((long)Math.Ceiling(
                MaximumWhiteLoadPerSecond(maximumLoadProfile)
                * supportedItemCount));
            Require(generatedPerSecond >= requiredSustainedSupply,
                "I031 Lv1 does not supply the formal five-white-item load envelope.");
            Require(C1FormalI031NianCapacityProjection.MaxNian
                    >= supportedItemCount * maximumWhiteBurstCost,
                "I031 Lv1 cannot carry the formal five-white-item burst envelope.");

            CanonicalItemDefinition definition =
                RequireDefinition(resolver, maximumLoadProfile.baseItemId);
            C1FormalRealtimeBattleItemFactSnapshot[] facts = Enumerable
                .Range(0, supportedItemCount)
                .Select(index => CreateItemFact(
                    "i031-load-envelope-" + index,
                    definition,
                    1,
                    0,
                    0,
                    0,
                    maximumWhiteSustainedCost,
                    maximumWhiteSustainedCooldownUnits,
                    true,
                    false,
                    310310L + index))
                .ToArray();
            C1FormalRealtimeBattleSession session = StartSession(
                "i031-load-envelope",
                facts,
                310310L);
            Require(session.TryAdvanceTo(9600L, out _),
                "Five-item I031 load envelope did not advance.");
            C1FormalRealtimeBattleSessionStateSnapshot state =
                Snapshot(session);
            foreach (C1FormalRealtimeBattleItemFactSnapshot fact in facts)
            {
                int acceptedSpends = session.NianTransactionLedger.Count(row =>
                    row.transactionKind ==
                        C1FormalRealtimeBattleNianTransactionKinds.Spend
                    && string.Equals(
                        row.sourceItemInstanceId,
                        fact.itemInstanceId,
                        StringComparison.Ordinal));
                Require(acceptedSpends >= 5,
                    "I031 Lv1 starved a supported five-item source: "
                    + fact.itemInstanceId + " spends=" + acceptedSpends);
            }
            Require(!state.scheduledActions.Any(action =>
                    action.executed
                    && string.Equals(
                        action.diagnosticCode,
                        C1FormalRealtimeBattleErrorCodes.NianInsufficient,
                        StringComparison.Ordinal)),
                "A supported five-white-item load produced Nian insufficiency.");
        }

        private static void VerifyI031FairReadyRetry(
            CanonicalItemDefinitionResolver resolver)
        {
            ItemBalanceWorkbenchCatalog catalog = LoadCatalog();
            ItemBalanceProfile maximumCostProfile = catalog.profiles
                .Where(profile => profile != null)
                .OrderByDescending(profile => profile
                    .FindVersion(ItemInstanceRarity.White)
                    .FindRange("nianCost").maxUnits)
                .First();
            RequireWhiteLoadRanges(maximumCostProfile);
            ItemBalanceRarityVersion maximumCostVersion =
                maximumCostProfile.FindVersion(ItemInstanceRarity.White);
            int maximumCost = checked((int)maximumCostVersion
                .FindRange("nianCost").maxUnits);
            long cooldownUnits = maximumCostVersion
                .FindRange("cooldown").minUnits;
            long firstOffsetMs = checked(cooldownUnits * 400L);
            long generationInterval =
                C1FormalI031NianCapacityProjection
                    .GenerationIntervalMilliseconds;
            long nextSupplyPulseMs = checked(
                (firstOffsetMs / generationInterval + 1L)
                * generationInterval);
            CanonicalItemDefinition definition =
                RequireDefinition(resolver, maximumCostProfile.baseItemId);
            C1FormalRealtimeBattleItemFactSnapshot[] facts = Enumerable
                .Range(0, 3)
                .Select(index => CreateItemFact(
                    "i031-fair-retry-" + index,
                    definition,
                    1,
                    0,
                    0,
                    0,
                    maximumCost,
                    cooldownUnits,
                    true,
                    false,
                    310320L + index))
                .ToArray();
            C1FormalRealtimeBattleSession session = StartSession(
                "i031-fair-retry",
                facts,
                310320L);
            Require(session.TryAdvanceTo(firstOffsetMs - 1L, out _),
                "I031 fair retry did not reach the forced low-Nian boundary.");
            SetCurrentNian(session, 0);
            Require(session.TryAdvanceTo(firstOffsetMs, out _),
                "I031 fair retry did not reach the first overloaded boundary.");
            C1FormalRealtimeBattleSessionStateSnapshot firstBoundary =
                Snapshot(session);
            int waitingForNextPulse = firstBoundary.scheduledActions.Count(action =>
                !action.executed
                && string.Equals(
                    action.actionKind,
                    C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                    StringComparison.Ordinal)
                && action.dueBattleTimeMs == nextSupplyPulseMs
                && facts.Any(fact => string.Equals(
                    fact.sourceId,
                    action.sourceId,
                    StringComparison.Ordinal)));
            Require(waitingForNextPulse == facts.Length,
                "Nian-insufficient Ready items did not all wait for the next supply pulse: waiting="
                + waitingForNextPulse);

            Require(session.TryAdvanceTo(nextSupplyPulseMs, out _),
                "I031 fair retry did not process the next supply pulse.");
            long followingSupplyPulseMs = checked(
                nextSupplyPulseMs + generationInterval);
            C1FormalRealtimeBattleSessionStateSnapshot afterSupply =
                Snapshot(session);
            int waitingForFollowingPulse = afterSupply.scheduledActions.Count(
                action => !action.executed
                    && string.Equals(
                        action.actionKind,
                        C1FormalRealtimeBattleScheduledActionKinds.ItemTrigger,
                        StringComparison.Ordinal)
                    && action.dueBattleTimeMs == followingSupplyPulseMs
                    && facts.Any(fact => string.Equals(
                        fact.sourceId,
                        action.sourceId,
                        StringComparison.Ordinal)));
            Require(waitingForFollowingPulse == facts.Length - 1,
                "The first supply pulse did not commit exactly one Ready item and keep the rest waiting: waiting="
                + waitingForFollowingPulse);

            Require(session.TryAdvanceTo(
                    checked(nextSupplyPulseMs + 2L * generationInterval),
                    out _),
                "I031 fair retry did not advance through the supply pulses.");
            foreach (C1FormalRealtimeBattleItemFactSnapshot fact in facts)
            {
                Require(session.NianTransactionLedger.Any(row =>
                        row.transactionKind ==
                            C1FormalRealtimeBattleNianTransactionKinds.Spend
                        && string.Equals(
                            row.sourceItemInstanceId,
                            fact.itemInstanceId,
                            StringComparison.Ordinal)),
                    "I031 fair retry permanently starved "
                    + fact.itemInstanceId);
            }
        }

        private static void VerifyTrayUnlit(
            CanonicalItemDefinitionResolver resolver)
        {
            C1FormalRealtimeBattleItemFactSnapshot unlit = CreateItemFact(
                "tray-unlit",
                RequireDefinition(resolver, "I004"),
                10,
                0,
                0,
                0,
                1,
                5L,
                false,
                false,
                44004L);
            C1FormalRealtimeBattleSession session =
                new C1FormalRealtimeBattleSession();
            bool accepted = session.TryStart(
                CreateRequest("tray-unlit", new[] { unlit }, 44004L),
                out _);
            Require(!accepted,
                "Tray/unlit Item was accepted into the live Session.");
            Require(session.NianTransactionLedger.Count == 0
                    && !session.CueLedger.Any(cue => cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted),
                "Tray/unlit Item consumed Nian or produced a success cue.");
        }

        private static void VerifyHealFullHpZeroBenefitActivation(
            CanonicalItemDefinitionResolver resolver)
        {
            CanonicalItemDefinition definition = resolver
                .OrdinaryDropDefinitions.SingleOrDefault(value =>
                    string.Equals(
                        value.primaryStatId,
                        "heal",
                        StringComparison.Ordinal));
            RequireSemantic(definition != null,
                "Canonical Catalog has no ordinary primary-Heal definition.");
            C1FormalRealtimeBattleItemFactSnapshot fact = CreateItemFact(
                "heal-full-hp-zero-benefit",
                definition,
                0,
                0,
                12,
                0,
                1,
                5L,
                true,
                false,
                22022L);
            C1FormalRealtimeBattleSession session = StartSession(
                "heal-full-hp-zero-benefit",
                new[] { fact },
                22022L);
            C1FormalRealtimeBattleSessionStateSnapshot before =
                Snapshot(session);
            Dictionary<string, long> enemyStateBefore = before.actorSnapshots
                .ToDictionary(
                    ActorIdentity,
                    actor => (long)actor.currentHp + actor.currentShell,
                    StringComparer.Ordinal);
            int nianBefore = before.nianStateSnapshot.currentNian;
            int acceptedBefore = before.acceptedItemApplicationCount;
            Require(before.playerSnapshot.currentHp
                    == before.playerSnapshot.maxHp,
                "Full-HP Heal proof did not start at full player HP.");
            Require(session.TryAdvanceTo(2000L, out _),
                "Full-HP Heal action boundary did not resolve.");
            C1FormalRealtimeBattleSessionStateSnapshot after =
                Snapshot(session);
            Require(after.actorSnapshots.All(actor =>
                    enemyStateBefore[ActorIdentity(actor)]
                    == (long)actor.currentHp + actor.currentShell),
                "Full-HP Heal unexpectedly changed real enemy state.");
            Require(after.playerSnapshot.currentHp
                        == before.playerSnapshot.currentHp
                    && after.playerSnapshot.guard
                        == before.playerSnapshot.guard,
                "Full-HP Heal unexpectedly changed player HP/Guard.");
            Require(after.nianStateSnapshot.currentNian == nianBefore - 1,
                "Full-HP Heal did not commit exactly one Nian spend.");
            Require(after.acceptedItemApplicationCount
                        == acceptedBefore + 1,
                "Full-HP Heal activation was not accepted exactly once.");
            Require(session.CueLedger.Any(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted
                    && cue.battleTimeMs == 2000L
                    && cue.sourceId == fact.itemInstanceId),
                "Full-HP Heal did not emit its exact-source activation cue.");
            Require(session.CueLedger.Any(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.ApplicationFeedbackPayload
                    && cue.battleTimeMs == 2000L
                    && cue.sourceItemInstanceId == fact.itemInstanceId
                    && cue.effectVariantId ==
                        C1FormalRealtimeBattleApplicationFeedbackKinds.Heal
                    && cue.requestedDamage == 12
                    && cue.appliedDamage == 0
                    && cue.floatPayload == "HP +0"),
                "Full-HP Heal did not emit its truthful zero-benefit feedback.");
        }

        private static void VerifyCleanseRealPlayerStatus(
            CanonicalItemDefinitionResolver resolver)
        {
            CanonicalItemDefinition definition = RequireCleanseDefinition(resolver);
            C1FormalRealtimeBattleItemFactSnapshot fact = CreateItemFact(
                "cleanse-real-player-status",
                definition,
                0,
                0,
                0,
                0,
                1,
                13L,
                true,
                false,
                19020L,
                cleanse: 3L);
            C1FormalRealtimeBattleSession session = StartSession(
                "cleanse-real-player-status",
                new[] { fact },
                19020L);

            Require(session.TryAdvanceTo(4500L, out _),
                "Shattered Host first action did not resolve.");
            Require(session.GetPlayerStatusStackCount("pollution") == 1,
                "Shattered Host polluted pulse did not create a real player status.");
            C1FormalRealtimeBattleSessionStateSnapshot before = Snapshot(session);
            Require(before.statusSnapshots.Any(status =>
                    status.targetActorId ==
                        C1FormalRealtimeBattleSessionContract.PlayerActorId
                    && status.targetStableOrder == -1
                    && status.statusKey == "pollution"
                    && status.stackCount == 1),
                "Player pollution was absent from the authoritative state snapshot.");
            int nianBefore = before.nianStateSnapshot.currentNian;
            int acceptedBefore = before.acceptedItemApplicationCount;

            Require(session.TryAdvanceTo(5200L, out _),
                "Cleanse action boundary did not resolve.");
            C1FormalRealtimeBattleSessionStateSnapshot after = Snapshot(session);
            Require(session.GetPlayerStatusStackCount("pollution") == 0,
                "Cleanse did not remove the real player pollution status.");
            Require(after.statusSnapshots.All(status =>
                    status.targetActorId !=
                        C1FormalRealtimeBattleSessionContract.PlayerActorId
                    || status.statusKey != "pollution"),
                "Removed player pollution remained in the authoritative state snapshot.");
            Require(after.nianStateSnapshot.currentNian == nianBefore - 1,
                "Successful Cleanse did not commit exactly one Nian spend.");
            Require(after.acceptedItemApplicationCount == acceptedBefore + 1,
                "Successful Cleanse was not accepted exactly once.");
            Require(session.CueLedger.Any(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted
                    && cue.battleTimeMs == 5200L
                    && cue.sourceId == fact.itemInstanceId),
                "Successful Cleanse did not emit its exact-source success cue.");
        }

        private static void VerifyCleanseNoStatusZeroBenefitActivation(
            CanonicalItemDefinitionResolver resolver)
        {
            CanonicalItemDefinition definition = RequireCleanseDefinition(resolver);
            C1FormalRealtimeBattleItemFactSnapshot fact = CreateItemFact(
                "cleanse-no-status",
                definition,
                0,
                0,
                0,
                0,
                1,
                5L,
                true,
                false,
                20020L,
                cleanse: 1L);
            C1FormalRealtimeBattleSession session = StartSession(
                "cleanse-no-status",
                new[] { fact },
                20020L);
            C1FormalRealtimeBattleSessionStateSnapshot before = Snapshot(session);
            int nianBefore = before.nianStateSnapshot.currentNian;
            int acceptedBefore = before.acceptedItemApplicationCount;

            Require(session.TryAdvanceTo(2000L, out _),
                "No-status Cleanse action boundary did not resolve.");
            C1FormalRealtimeBattleSessionStateSnapshot after = Snapshot(session);
            Require(session.GetPlayerStatusStackCount("pollution") == 0,
                "No-status Cleanse unexpectedly created or retained pollution.");
            Require(after.nianStateSnapshot.currentNian == nianBefore - 1,
                "No-status Cleanse did not commit exactly one Nian spend.");
            Require(after.acceptedItemApplicationCount == acceptedBefore + 1,
                "No-status Cleanse activation was not accepted exactly once.");
            Require(session.CueLedger.Any(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.ItemTriggerAccepted
                    && cue.battleTimeMs == 2000L
                    && cue.sourceId == fact.itemInstanceId),
                "No-status Cleanse did not emit its exact-source activation cue.");
            Require(!session.CueLedger.Any(cue =>
                    (cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.StatusRemoved
                     || cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.StatusRefreshed)
                    && cue.battleTimeMs == 2000L
                    && cue.sourceId == fact.itemInstanceId),
                "No-status Cleanse incorrectly emitted a successful application cue.");
            Require(session.CueLedger.Any(cue => cue.cueKind ==
                    C1FormalRealtimeBattleCueKinds.ApplicationFeedbackPayload
                    && cue.battleTimeMs == 2000L
                    && cue.sourceItemInstanceId == fact.itemInstanceId
                    && cue.effectVariantId ==
                        C1FormalRealtimeBattleApplicationFeedbackKinds.Cleanse
                    && cue.requestedDamage == 1
                    && cue.appliedDamage == 0
                    && cue.floatPayload == "CLEANSE 0"),
                "No-status Cleanse did not emit its truthful zero-benefit feedback.");
        }

        private static void VerifyCleanseRewardCapabilityAdmission(
            CanonicalItemDefinitionResolver resolver)
        {
            CanonicalItemDefinition cleanseDefinition =
                RequireCleanseDefinition(resolver);
            CanonicalItemDefinition ordinaryDefinition = resolver
                .OrdinaryDropDefinitions.First(value =>
                    !value.requiresCleanseablePlayerStatus);
            CanonicalItemDropRequest withoutCapability = CreateDropRequest(
                "without-cleanse-capability",
                Array.Empty<string>());
            CanonicalItemDropRequest withCapability = CreateDropRequest(
                "with-cleanse-capability",
                new[]
                {
                    CanonicalItemBattleCapabilities.PlayerCleanseableStatus
                });

            Require(!CanonicalItemDropPolicy.IsBattleCapabilityCompatible(
                    cleanseDefinition,
                    withoutCapability),
                "Cleanse Item was admitted without a cleanseable player status provider.");
            Require(CanonicalItemDropPolicy.IsBattleCapabilityCompatible(
                    cleanseDefinition,
                    withCapability),
                "Cleanse Item was not admitted when the next battle provides a cleanseable status.");
            Require(CanonicalItemDropPolicy.IsBattleCapabilityCompatible(
                    ordinaryDefinition,
                    withoutCapability),
                "Capability filtering incorrectly removed an unrelated ordinary Item.");
        }

        private static CanonicalItemDefinition RequireCleanseDefinition(
            CanonicalItemDefinitionResolver resolver)
        {
            CanonicalItemDefinition definition = resolver
                .OrdinaryDropDefinitions.FirstOrDefault(value =>
                    value.requiresCleanseablePlayerStatus);
            RequireSemantic(definition != null,
                "Canonical Catalog has no ordinary Cleanse definition.");
            return definition;
        }

        private static CanonicalItemDropRequest CreateDropRequest(
            string suffix,
            IEnumerable<string> battleCapabilities)
        {
            return new CanonicalItemDropRequest(
                ProofPrefix + ".drop." + suffix,
                ProofPrefix + ".drop.item." + suffix,
                ProofPrefix + ".drop.context." + suffix,
                "battle-reward",
                1,
                1,
                20260813L,
                new Dictionary<string, int>(),
                null,
                battleCapabilities);
        }

        private static C1FormalRealtimeBattleSession StartSession(
            string suffix,
            IReadOnlyList<C1FormalRealtimeBattleItemFactSnapshot> facts,
            long seed)
        {
            return StartSession(suffix, facts, seed, out _);
        }

        private static C1FormalRealtimeBattleSession StartSession(
            string suffix,
            IReadOnlyList<C1FormalRealtimeBattleItemFactSnapshot> facts,
            long seed,
            out C1FormalRealtimeBattleSessionStartSnapshot start)
        {
            C1FormalRealtimeBattleSession session =
                new C1FormalRealtimeBattleSession();
            Require(session.TryStart(
                    CreateRequest(suffix, facts, seed),
                    out start)
                    && start != null
                    && start.accepted,
                "Live Session start rejected: "
                + (start?.error?.errorCode ?? "missing-start"));
            return session;
        }

        private static C1FormalRealtimeBattleSessionRequest CreateRequest(
            string suffix,
            IReadOnlyList<C1FormalRealtimeBattleItemFactSnapshot> facts,
            long seed)
        {
            string itemInput = ProofPrefix + ".input." + suffix;
            string arrangement = ProofPrefix + ".arrangement." + suffix;
            C1FormalRealtimeBattleNianCapacitySnapshot nian =
                new C1FormalRealtimeBattleNianCapacitySnapshot(
                    C1FormalRealtimeBattleSessionContract.ProductContext,
                    C1FormalI031NianCapacityProjection.ResourceKey,
                    C1FormalI031NianCapacityProjection.InitialNian,
                    C1FormalI031NianCapacityProjection.MaxNian,
                    C1FormalI031NianCapacityProjection.GenerationAmount,
                    C1FormalI031NianCapacityProjection
                        .GenerationIntervalMilliseconds,
                    C1FormalI031NianCapacityProjection.SourceRevision,
                    C1FormalI031NianCapacityProjection.SourceProfileId,
                    ProofPrefix + ".i031.instance." + suffix,
                    "I031",
                    ProofPrefix + ".i031.placement." + suffix,
                    0,
                    0,
                    itemInput,
                    arrangement,
                    CanonicalItemCatalogContract.CatalogId);
            return new C1FormalRealtimeBattleSessionRequest(
                C1FormalRealtimeBattleSessionContract.RequestSchemaId,
                C1FormalRealtimeBattleSessionContract.ProductContext,
                C1FormalItemSessionContract.ChapterId,
                "1-3",
                "campaign.normal.lv1.balance.identity.c1",
                "campaign.normal.lv1.encounter.c1.1-3",
                ProofPrefix + ".launch." + suffix,
                seed,
                ProofPrefix + ".session." + suffix,
                ProofPrefix + ".token." + suffix,
                0L,
                itemInput,
                arrangement,
                CanonicalItemCatalogContract.CatalogId,
                C1FormalEnemyDefinitionCatalog.CatalogId,
                facts,
                itemInput,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<
                    C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>(),
                nian);
        }

        private static C1FormalRealtimeBattleItemFactSnapshot CreateItemFact(
            string suffix,
            CanonicalItemDefinition definition,
            int directDamage,
            long guard,
            long heal,
            long breakValue,
            int nianCost,
            long cooldownTurns,
            bool isLit,
            bool isDirectLit,
            long seed,
            int anchorX = 0,
            int anchorY = 0,
            int rotation = 0,
            long cleanse = 0L)
        {
            string baseItemId = definition.baseItemId;
            string itemInstanceId = ProofPrefix + ".item." + suffix;
            ItemInstanceIdentityCreationResult identity =
                ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                    ItemRarityInstanceFoundation.Create(),
                    itemInstanceId,
                    baseItemId,
                    ItemInstanceRarity.White,
                    1,
                    seed,
                    string.Empty);
            Require(identity != null && identity.isValid
                    && identity.snapshot != null,
                "Generated Item identity is invalid: " + baseItemId);
            ItemGeneratedStatSnapshot[] stats =
            {
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "damage",
                    (long)directDamage),
                CreateInternal<ItemGeneratedStatSnapshot>("guard", guard),
                CreateInternal<ItemGeneratedStatSnapshot>("heal", heal),
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "break",
                    breakValue),
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "nianCost",
                    (long)nianCost),
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "cooldown",
                    cooldownTurns),
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "cleanse",
                    cleanse)
            };
            ItemGeneratedInstanceSnapshot generated =
                CreateInternal<ItemGeneratedInstanceSnapshot>(
                    ItemGenerationDataStatus.PlaytestV1Canonical,
                    identity.snapshot,
                    stats,
                    Array.Empty<ItemGeneratedAffixSnapshot>(),
                    null,
                    ItemBuildQualification.None);
            string projection = generated.BuildCanonicalSignature();
            long cooldownMs = checked(cooldownTurns * 400L);
            string rarityVersionKey = baseItemId + "@white";
            C1FormalRealtimeBattleItemActionCostSnapshot actionCost =
                new C1FormalRealtimeBattleItemActionCostSnapshot(
                    itemInstanceId,
                    baseItemId,
                    "white",
                    rarityVersionKey,
                    "canonical-item-action",
                    C1FormalI031NianCapacityProjection.ResourceKey,
                    nianCost,
                    CanonicalItemCatalogContract.CatalogVersion,
                    CanonicalItemCatalogContract.CatalogVersion,
                    CanonicalItemCatalogContract.CatalogId,
                    projection,
                    cooldownMs,
                    cooldownMs);
            return new C1FormalRealtimeBattleItemFactSnapshot(
                itemInstanceId,
                itemInstanceId,
                baseItemId,
                rarityVersionKey,
                isLit,
                directDamage,
                cooldownMs,
                projection,
                CanonicalItemCatalogContract.CatalogId,
                actionCost,
                null,
                generated,
                definition,
                ProofPrefix + ".placement." + suffix,
                anchorX,
                anchorY,
                rotation,
                isDirectLit);
        }

        private static CanonicalItemDefinition RequireDefinition(
            CanonicalItemDefinitionResolver resolver,
            string baseItemId)
        {
            CanonicalItemDefinition definition =
                resolver.GetDefinition(baseItemId);
            RequireSemantic(definition != null
                            && definition.isOrdinaryDropEligible,
                "Canonical definition is unavailable: " + baseItemId);
            return definition;
        }

        private static C1FormalRealtimeBattleSessionStateSnapshot Snapshot(
            C1FormalRealtimeBattleSession session)
        {
            Require(session.TryGetSnapshot(out
                    C1FormalRealtimeBattleSessionStateSnapshot snapshot),
                "Live Session snapshot is unavailable.");
            return snapshot;
        }

        private static void SetCurrentNian(
            C1FormalRealtimeBattleSession session,
            int currentNian)
        {
            FieldInfo field = typeof(C1FormalRealtimeBattleSession).GetField(
                "currentNian",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(field != null, "Live Session current Nian field is unavailable.");
            field.SetValue(session, currentNian);
        }

        private static Dictionary<string,
                C1FormalRealtimeBattleActorSnapshot>
            ActorsByIdentity(C1FormalRealtimeBattleSessionStateSnapshot state)
        {
            return state.actorSnapshots.ToDictionary(
                ActorIdentity,
                actor => actor,
                StringComparer.Ordinal);
        }

        private static string ActorIdentity(
            C1FormalRealtimeBattleActorSnapshot actor)
        {
            return actor.actorBalanceId + "|" + actor.occurrenceOrdinal;
        }

        private static bool IsPendingEnemyAction(
            C1FormalRealtimeBattleScheduledActionSnapshot action)
        {
            return action != null
                   && !action.executed
                   && action.sourceOwner ==
                   C1FormalRealtimeBattleSessionContract.EnemyOwner;
        }

        private static long EnemyHpAndShell(
            C1FormalRealtimeBattleSession session,
            string actorBalanceId)
        {
            C1FormalRealtimeBattleActorSnapshot actor = Snapshot(session)
                .actorSnapshots.Single(value => value.actorBalanceId
                    == actorBalanceId);
            return (long)actor.currentHp + actor.currentShell;
        }

        private static T CreateInternal<T>(params object[] arguments)
        {
            ConstructorInfo constructor = typeof(T).GetConstructors(
                    BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(value =>
                    value.GetParameters().Length == arguments.Length);
            return (T)constructor.Invoke(arguments);
        }

        private static string GrammarKey(
            CanonicalItemEffectDefinition effect)
        {
            return effect.operation + "|" + effect.targetStatId + "|"
                + effect.valueUnitKey;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void RequireSemantic(bool condition, string message)
        {
            if (!condition)
            {
                throw new SemanticGapException(message);
            }
        }

        private enum ProofClassification
        {
            Pass,
            CodeGap,
            SemanticGap
        }

        private sealed class ProofResult
        {
            internal ProofResult(
                string name,
                ProofClassification classification,
                string detail)
            {
                Name = name;
                Classification = classification;
                Detail = detail ?? string.Empty;
            }

            internal string Name { get; }
            internal ProofClassification Classification { get; }
            internal string Detail { get; }
        }

        private sealed class SemanticGapException : Exception
        {
            internal SemanticGapException(string message) : base(message)
            {
            }
        }
    }
}
