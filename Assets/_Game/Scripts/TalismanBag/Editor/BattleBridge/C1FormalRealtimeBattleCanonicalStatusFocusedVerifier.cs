using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TalismanBag.BattleBridge.Formal;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.Items.Balance;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;

namespace TalismanBag.Editor.BattleBridge
{
    /// <summary>
    /// Focused executable proof for the canonical timed-status path. It does
    /// not invoke the retired report/hash verifier and does not depend on an
    /// Item-id branch inside Battle.
    /// </summary>
    public static class
        C1FormalRealtimeBattleCanonicalStatusFocusedVerifier
    {
        private const string SourceInstanceId =
            "verifier.canonical-status.i007";

        public static int RunOrThrow()
        {
#if C1_FORMAL_REALTIME_BATTLE_CANONICAL_STATUS_STANDALONE
            Console.WriteLine("CANONICAL_STATUS_STEP definition");
#endif
            CanonicalItemDefinition definition =
                CreateI007CanonicalDefinition();
            VerifyCanonicalData(definition);
#if C1_FORMAL_REALTIME_BATTLE_CANONICAL_STATUS_STANDALONE
            Console.WriteLine("CANONICAL_STATUS_STEP stack-refresh");
#endif
            VerifyStackRefreshAndDamage(definition);
#if C1_FORMAL_REALTIME_BATTLE_CANONICAL_STATUS_STANDALONE
            Console.WriteLine("CANONICAL_STATUS_STEP expiry");
#endif
            VerifyFinalTickThenExpiry(definition);
            return 3;
        }

        private static void VerifyCanonicalData(
            CanonicalItemDefinition definition)
        {
            CanonicalItemEffectDefinition effect =
                definition?.combatEffect;
            Require(definition != null
                    && definition.isOrdinaryDropEligible,
                "I007 must be an ordinary canonical Item definition.");
            Require(effect != null
                    && effect.hasCompleteStatusSemantics
                    && effect.statusKey == "afterglow"
                    && effect.statusFamilyKey == "PERIODIC_DAMAGE"
                    && effect.stackLimit == 3
                    && effect.durationUnits == 5000L
                    && effect.firstTickDelayUnits == 1000L
                    && effect.tickIntervalUnits == 1000L
                    && effect.tickDamageRatioBasisPoints == 1000L
                    && effect.reapplyPolicy ==
                        "ADD_STACK_AND_REFRESH"
                    && effect.tickSchedulePolicy ==
                        "PRESERVE_ON_REFRESH"
                    && !effect.canCrit
                    && !effect.countsAsHit
                    && !effect.autoConsumeAtMaxStack,
                "I007_STATUS_PLAYTEST_V1 canonical data is incomplete.");
        }

        private static void VerifyStackRefreshAndDamage(
            CanonicalItemDefinition definition)
        {
            C1FormalRealtimeBattleSession session = StartSession(
                "stack-refresh",
                definition,
                10,
                5L,
                7007L);
            Require(session.TryAdvanceTo(2000L, out _),
                "The first canonical I007 action did not resolve.");
            C1FormalRealtimeBattleStatusSnapshot first =
                session.StatusSnapshots.Single();
            Require(first.stackCount == 1
                    && first.nextTickAtBattleTimeMs == 3000L
                    && first.expireAtBattleTimeMs == 7000L,
                "The first Afterglow stack or schedule is wrong.");
            long stateAfterApplication = EnemyHpAndShell(
                session,
                first.targetActorId);

            Require(session.TryAdvanceTo(3000L, out _),
                "The first Afterglow tick did not resolve.");
            long stateAfterTick = EnemyHpAndShell(
                session,
                first.targetActorId);
            Require(stateAfterTick < stateAfterApplication,
                "Afterglow tick did not mutate live enemy HP/shell.");
            Require(session.CueLedger.Any(cue =>
                    cue.cueKind ==
                        C1FormalRealtimeBattleCueKinds.StatusTickAccepted
                    && cue.sourceItemInstanceId.StartsWith(
                        SourceInstanceId,
                        StringComparison.Ordinal)
                    && cue.appliedDamage > 0),
                "Afterglow mutation did not emit a source-owned tick cue.");

            Require(session.TryAdvanceTo(6000L, out _),
                "Afterglow did not reach the third action.");
            C1FormalRealtimeBattleStatusSnapshot capped =
                session.StatusSnapshots.Single();
            Require(capped.stackCount == 3
                    && capped.Contributions.Count == 3
                    && capped.nextTickAtBattleTimeMs == 7000L
                    && capped.expireAtBattleTimeMs == 11000L,
                "Afterglow stack growth or refresh-without-tick-reset failed.");

            Require(session.TryAdvanceTo(8000L, out _),
                "The max-stack refresh did not resolve.");
            C1FormalRealtimeBattleStatusSnapshot refreshed =
                session.StatusSnapshots.Single();
            Require(refreshed.stackCount == 3
                    && refreshed.Contributions.Count == 3
                    && refreshed.nextTickAtBattleTimeMs == 9000L
                    && refreshed.expireAtBattleTimeMs == 13000L,
                "Max-stack reapply must refresh duration without adding a stack.");
        }

        private static void VerifyFinalTickThenExpiry(
            CanonicalItemDefinition definition)
        {
            C1FormalRealtimeBattleSession session = StartSession(
                "expiry",
                definition,
                10,
                20L,
                7008L);
            Require(session.TryAdvanceTo(8000L, out _),
                "The expiry scenario did not apply Afterglow.");
            Require(session.StatusSnapshots.Single().expireAtBattleTimeMs
                    == 13000L,
                "The expiry scenario did not bind the five-second lifetime.");
            Require(session.TryAdvanceTo(13000L, out _),
                "The expiry boundary did not resolve.");
            Require(session.StatusSnapshots.Count == 0,
                "Afterglow remained after its authoritative expiry.");

            C1FormalRealtimeBattleCue[] boundary = session.CueLedger
                .Where(cue => cue.battleTimeMs == 13000L
                    && (cue.cueKind ==
                            C1FormalRealtimeBattleCueKinds
                                .StatusTickAccepted
                        || cue.cueKind ==
                            C1FormalRealtimeBattleCueKinds.StatusRemoved))
                .ToArray();
            Require(boundary.Length >= 2
                    && boundary[0].cueKind ==
                        C1FormalRealtimeBattleCueKinds.StatusTickAccepted
                    && boundary[boundary.Length - 1].cueKind ==
                        C1FormalRealtimeBattleCueKinds.StatusRemoved,
                "At equal time, the final tick must resolve before expiry.");
        }

        private static C1FormalRealtimeBattleSession StartSession(
            string suffix,
            CanonicalItemDefinition definition,
            int directDamage,
            long cooldownTurns,
            long seed)
        {
            C1FormalRealtimeBattleItemFactSnapshot fact = CreateItemFact(
                suffix,
                definition,
                directDamage,
                cooldownTurns,
                seed);
            string itemInput = "verifier.canonical-status.input." + suffix;
            string arrangement =
                "verifier.canonical-status.arrangement." + suffix;
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
                    "verifier.i031.instance",
                    "I031",
                    "verifier.i031.placement",
                    0,
                    0,
                    itemInput,
                    arrangement,
                    CanonicalItemCatalogContract.CatalogId);
            C1FormalRealtimeBattleSessionRequest request =
                new C1FormalRealtimeBattleSessionRequest(
                    C1FormalRealtimeBattleSessionContract.RequestSchemaId,
                    C1FormalRealtimeBattleSessionContract.ProductContext,
                    C1FormalItemSessionContract.ChapterId,
                    "1-3",
                    "campaign.normal.lv1.balance.identity.c1",
                    "campaign.normal.lv1.encounter.c1.1-3",
                    "verifier.canonical-status.launch." + suffix,
                    seed,
                    "verifier.canonical-status.session." + suffix,
                    "verifier.canonical-status.token." + suffix,
                    0L,
                    itemInput,
                    arrangement,
                    CanonicalItemCatalogContract.CatalogId,
                    C1FormalEnemyDefinitionCatalog.CatalogId,
                    new[] { fact },
                    itemInput,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Array.Empty<
                        C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>(),
                    nian);
            C1FormalRealtimeBattleSession session =
                new C1FormalRealtimeBattleSession();
            Require(session.TryStart(request, out
                    C1FormalRealtimeBattleSessionStartSnapshot start)
                    && start != null
                    && start.accepted,
                "Canonical I007 live Session start was rejected: "
                + (start?.error?.errorCode ?? "missing-start"));
            return session;
        }

        private static C1FormalRealtimeBattleItemFactSnapshot CreateItemFact(
            string suffix,
            CanonicalItemDefinition definition,
            int directDamage,
            long cooldownTurns,
            long seed)
        {
            string itemInstanceId = SourceInstanceId + "." + suffix;
            ItemInstanceIdentityCreationResult identity =
                ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                    ItemRarityInstanceFoundation.Create(),
                    itemInstanceId,
                    "I007",
                    ItemInstanceRarity.White,
                    1,
                    seed,
                    string.Empty);
            Require(identity != null && identity.isValid
                    && identity.snapshot != null,
                "The canonical I007 generated identity is invalid.");
            ItemGeneratedStatSnapshot[] stats =
            {
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "damage",
                    (long)directDamage),
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "nianCost",
                    1L),
                CreateInternal<ItemGeneratedStatSnapshot>(
                    "cooldown",
                    cooldownTurns)
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
            string rarityVersionKey = "I007@white";
            C1FormalRealtimeBattleItemActionCostSnapshot actionCost =
                new C1FormalRealtimeBattleItemActionCostSnapshot(
                    itemInstanceId,
                    "I007",
                    "white",
                    rarityVersionKey,
                    "canonical-item-action",
                    C1FormalI031NianCapacityProjection.ResourceKey,
                    1,
                    CanonicalItemCatalogContract.CatalogVersion,
                    CanonicalItemCatalogContract.CatalogVersion,
                    CanonicalItemCatalogContract.CatalogId,
                    projection,
                    cooldownMs,
                    cooldownMs);
            return new C1FormalRealtimeBattleItemFactSnapshot(
                itemInstanceId,
                itemInstanceId,
                "I007",
                rarityVersionKey,
                true,
                directDamage,
                cooldownMs,
                projection,
                CanonicalItemCatalogContract.CatalogId,
                actionCost,
                null,
                generated,
                definition);
        }

        private static CanonicalItemDefinition
            CreateI007CanonicalDefinition()
        {
            ItemBalanceWorkbenchCatalog source =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    "Assets/_Game/Configs/ItemBalanceWorkbench/"
                    + "ItemBalanceWorkbenchCatalog.asset");
            Require(source != null,
                "The canonical Item catalog asset is missing.");
            Require(CanonicalItemDefinitionResolver.TryCreate(
                    source,
                    out CanonicalItemDefinitionResolver resolver,
                    out IReadOnlyList<string> errors),
                "The canonical Item catalog is invalid: "
                + string.Join("|", errors));
            CanonicalItemDefinition definition =
                resolver.GetDefinition("I007");
            Require(definition != null,
                "The canonical Item catalog has no I007 definition.");
            return definition;
        }

        private static long EnemyHpAndShell(
            C1FormalRealtimeBattleSession session,
            string actorBalanceId)
        {
            Require(session.TryGetSnapshot(out
                    C1FormalRealtimeBattleSessionStateSnapshot snapshot),
                "The live Session snapshot is unavailable.");
            C1FormalRealtimeBattleActorSnapshot actor = snapshot
                .actorSnapshots.Single(value => value.actorBalanceId
                    == actorBalanceId);
            return (long)actor.currentHp + actor.currentShell;
        }

        private static T CreateInternal<T>(params object[] arguments)
        {
            ConstructorInfo[] constructors = typeof(T).GetConstructors(
                BindingFlags.Instance | BindingFlags.NonPublic);
            ConstructorInfo constructor = constructors.Single(value =>
                value.GetParameters().Length == arguments.Length);
            return (T)constructor.Invoke(arguments);
        }

        private static void SetAutoProperty(
            object target,
            string propertyName,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                "<" + propertyName + ">k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(field != null,
                "Missing immutable backing field: " + propertyName);
            field.SetValue(target, value);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}

#if C1_FORMAL_REALTIME_BATTLE_CANONICAL_STATUS_STANDALONE
public static class C1FormalRealtimeBattleCanonicalStatusFocusedProgram
{
    public static int Main()
    {
        TalismanBag.Editor.BattleBridge
            .C1FormalRealtimeBattleCanonicalStatusFocusedVerifier
            .RunOrThrow();
        Console.WriteLine("CANONICAL_STATUS_FOCUSED_PASS");
        return 0;
    }
}
#endif
