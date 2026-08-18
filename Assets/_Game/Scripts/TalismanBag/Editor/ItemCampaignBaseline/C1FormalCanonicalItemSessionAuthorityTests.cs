using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.V04.RewardDrop.Contracts;
using TalismanBag.V04.RewardDrop.Runtime.CampaignLoot;

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public static class C1FormalCanonicalItemSessionAuthorityTests
    {
        public const string TerminalMarker =
            "C1_FORMAL_CANONICAL_ITEM_SESSION_AUTHORITY_PASS";

        private static int assertionCount;

        public static int RunFocused()
        {
            try
            {
                assertionCount = 0;
                const string runSessionId = "focused.canonical.item.session";
                C1FormalItemSessionCreationResult creation =
                    C1FormalItemSessionAuthority.Create(runSessionId, 1L);
                Require(creation?.isSuccess == true && creation.authority != null,
                    creation?.diagnosticCode ?? "authority creation failed");
                C1FormalItemSessionAuthority authority = creation.authority;

                VerifyCanonicalInitialItem(authority);
                string rewardedItemInstanceId = AcceptOneCanonicalReward(authority);
                VerifyDuplicateRewardIsAcceptedOnce(authority);
                VerifySameBaseReplacement();
                VerifyResetRemovesRunReward(authority, rewardedItemInstanceId);

                Console.WriteLine(TerminalMarker + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_FORMAL_CANONICAL_ITEM_SESSION_AUTHORITY_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        private static void VerifyCanonicalInitialItem(
            C1FormalItemSessionAuthority authority)
        {
            Require(CanonicalInitialItemAcquisitionPolicy.TryCreate(
                    authority.CanonicalItemResolver,
                    out CanonicalItemDefinition selectedDefinition,
                    out ItemGeneratedInstanceSnapshot generatedItem,
                    out string diagnostic),
                diagnostic);
            C1FormalItemRosterEntrySnapshot initial = authority.Current
                .FindRosterEntry(CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
            Require(initial?.ordinaryInstance != null,
                "canonical initial Item is roster-owned");
            Equal(selectedDefinition.baseItemId, initial.baseItemId,
                "initial Item uses the policy-selected canonical definition");
            Equal(generatedItem.itemInstanceId, initial.itemInstanceId,
                "initial Item keeps the generated instance identity");
            C1FormalItemBattleItemRow battleRow = authority
                .CreateBattleInputSnapshot().itemRows.Single(row =>
                    row.itemInstanceId == initial.itemInstanceId);
            Require(battleRow.isPlaced && battleRow.isLit == true,
                "canonical initial Item is placed and lit");
        }

        private static string AcceptOneCanonicalReward(
            C1FormalItemSessionAuthority authority)
        {
            C1CampaignDirectLootGrantBundle grant = CreateCanonicalGrant(authority);
            C1FormalCampaignLootEntitlementAcceptanceResult accepted = authority
                .AcceptCanonicalCampaignLootEntitlement(
                    CreateCommand(authority, "focused.canonical.reward.accept", grant));
            Require(accepted?.accepted == true && accepted.changed,
                accepted?.diagnosticCode ?? "canonical reward acceptance failed");
            C1FormalCampaignLootAcceptedEntitlementReceipt receipt =
                accepted.acceptedEntitlementReceipt;
            Require(receipt != null && authority.Current.FindRosterEntry(
                    receipt.itemInstanceId) != null,
                "accepted reward is roster-owned by the same Item instance");
            C1FormalItemTrayPlacementSnapshot tray = authority.Current.trayLayout
                .FindPlacementByInstanceId(receipt.itemInstanceId);
            Require(tray != null && tray.isActiveInTray,
                "accepted reward appears once in Tray");
            C1FormalItemBattleItemRow battleRow = authority
                .CreateBattleInputSnapshot().itemRows.Single(row =>
                    row.itemInstanceId == receipt.itemInstanceId);
            Require(!battleRow.isPlaced && !battleRow.isLit.HasValue,
                "unplaced reward does not enter live Battle input as active");
            Equal(1, authority.CampaignLootEntitlementHistory.acceptedCount,
                "one canonical reward entitlement is recorded");
            return receipt.itemInstanceId;
        }

        private static void VerifyDuplicateRewardIsAcceptedOnce(
            C1FormalItemSessionAuthority authority)
        {
            C1FormalCampaignLootAcceptedEntitlementReceipt receipt = authority
                .CampaignLootEntitlementHistory.AcceptedReceipts.Single();
            C1CampaignDirectLootGrantBundle grant = CreateCanonicalGrant(authority);
            int rosterCount = authority.Current.roster.Count;
            C1FormalItemSessionSnapshot before = authority.Current;

            C1FormalCampaignLootEntitlementAcceptanceResult duplicate = authority
                .AcceptCanonicalCampaignLootEntitlement(
                    CreateCommand(authority, "focused.canonical.reward.retry", grant));
            Require(duplicate?.accepted == true && !duplicate.changed,
                duplicate?.diagnosticCode ?? "canonical reward retry failed");
            Equal(rosterCount, authority.Current.roster.Count,
                "duplicate reward adds no second Item");
            Equal(1, authority.CampaignLootEntitlementHistory.acceptedCount,
                "duplicate reward adds no second entitlement");
            Require(ReferenceEquals(before, authority.Current),
                "duplicate reward preserves the exact Item session snapshot");
            Equal(receipt.itemInstanceId,
                duplicate.acceptedEntitlementReceipt.itemInstanceId,
                "duplicate reward resolves to the accepted Item instance");
        }

        private static void VerifySameBaseReplacement()
        {
            const string runSessionId =
                "focused.canonical.same-base.replacement";
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(runSessionId, 1L);
            Require(creation?.isSuccess == true && creation.authority != null,
                creation?.diagnosticCode
                ?? "same-base replacement authority creation failed");
            C1FormalItemSessionAuthority authority = creation.authority;
            DuplicateRewardSequence sequence = FindDuplicateRewardSequence(
                authority);
            for (int index = 0; index < sequence.grants.Count; index++)
            {
                C1FormalCampaignLootEntitlementAcceptanceResult accepted =
                    authority.AcceptCanonicalCampaignLootEntitlement(
                        CreateCommand(
                            authority,
                            "focused.same-base.entitle."
                            + index.ToString(CultureInfo.InvariantCulture),
                            sequence.grants[index]));
                Require(accepted?.accepted == true && accepted.changed,
                    accepted?.diagnosticCode
                    ?? "same-base sequence entitlement failed");
            }

            C1CampaignDirectLootGrantBundle firstGrant =
                sequence.grants[sequence.firstIndex];
            C1CampaignDirectLootGrantBundle secondGrant =
                sequence.grants[sequence.secondIndex];
            Equal(firstGrant.selectedBaseItemId,
                secondGrant.selectedBaseItemId,
                "same-base sequence selects one base identity twice");
            Require(!string.Equals(
                    firstGrant.generatedItem.itemInstanceId,
                    secondGrant.generatedItem.itemInstanceId,
                    StringComparison.Ordinal),
                "same-base sequence retains two exact Item instances");

            C1FormalItemPlacementSnapshot firstPlacement =
                PlaceAtFirstLegalBoardCandidate(
                    authority,
                    firstGrant.generatedItem.itemInstanceId);
            C1FormalItemTrayPlacementSnapshot incomingTray = authority.Current
                .trayLayout.FindPlacementByInstanceId(
                    secondGrant.generatedItem.itemInstanceId);
            Require(incomingTray != null && incomingTray.isActiveInTray,
                "second same-base instance remains active in Tray");

            C1FormalItemSessionOperationResult replaced = authority.Submit(
                new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind
                        .ReplaceSameBaseFromTray,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    "focused.same-base.replace",
                    secondGrant.generatedItem.itemInstanceId,
                    authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(
                        firstPlacement.anchorCell,
                        firstPlacement.rotation)));
            Require(replaced?.accepted == true && replaced.changed,
                replaced?.diagnosticCode
                ?? "same-base atomic replacement failed");
            Require(authority.Current.FindPlacementByInstanceId(
                        firstGrant.generatedItem.itemInstanceId) == null,
                "replaced instance leaves Board");
            C1FormalItemPlacementSnapshot secondPlacement = authority.Current
                .FindPlacementByInstanceId(
                    secondGrant.generatedItem.itemInstanceId);
            Require(secondPlacement != null
                    && secondPlacement.anchorCell == firstPlacement.anchorCell
                    && secondPlacement.rotation == firstPlacement.rotation,
                "incoming instance owns the accepted Board placement");
            C1FormalItemTrayPlacementSnapshot returnedFirst = authority.Current
                .trayLayout.FindPlacementByInstanceId(
                    firstGrant.generatedItem.itemInstanceId);
            Require(returnedFirst != null && returnedFirst.isActiveInTray
                    && returnedFirst.anchorCell == incomingTray.anchorCell
                    && returnedFirst.rotation == incomingTray.rotation
                    && returnedFirst.occupiedCells.SequenceEqual(
                        incomingTray.occupiedCells),
                "outgoing instance atomically takes the vacated Tray slot");
            Require(!authority.Current.trayLayout.FindPlacementByInstanceId(
                    secondGrant.generatedItem.itemInstanceId).isActiveInTray,
                "incoming instance leaves Tray exactly once");
            Equal(1,
                authority.Current.placements.Count(value => string.Equals(
                    value.baseItemId,
                    firstGrant.selectedBaseItemId,
                    StringComparison.Ordinal)),
                "only one same-base Item remains deployed");
        }

        private static DuplicateRewardSequence FindDuplicateRewardSequence(
            C1FormalItemSessionAuthority authority)
        {
            string initialBaseItemId = authority.Current.FindRosterEntry(
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId)
                .baseItemId;
            for (long seed = 1L; seed <= 2048L; seed++)
            {
                C1CampaignDirectLootSessionCreateResult created =
                    C1CampaignDirectLootSessionResolver.CreateSession(
                        authority.Current.sessionToken,
                        seed);
                if (created?.accepted != true || created.snapshot == null)
                    continue;
                C1CampaignDirectLootSessionSnapshot rewardSession =
                    created.snapshot;
                Dictionary<string, int> currentCopies = authority.Current.roster
                    .Where(row => !row.isSpecialLightingSource)
                    .GroupBy(row => row.baseItemId, StringComparer.Ordinal)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Count(),
                        StringComparer.Ordinal);
                Dictionary<string, int> firstIndexByBase =
                    new(StringComparer.Ordinal);
                List<C1CampaignDirectLootGrantBundle> grants = new();
                for (int stageIndex = 0;
                     stageIndex
                     < C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count;
                     stageIndex++)
                {
                    string stageId = C1CampaignDirectLootPoolAndPolicy
                        .EligibleStageIds[stageIndex];
                    string suffix = seed.ToString(CultureInfo.InvariantCulture)
                                    + "." + stageId;
                    StageClearFact stageClear = new(
                        "focused.same-base.stage-clear." + suffix,
                        "focused.same-base.battle-result." + suffix,
                        "focused.same-base.battle-request." + suffix,
                        "focused.same-base.result." + suffix,
                        C1CampaignDirectLootPoolAndPolicy.ChapterId,
                        stageId,
                        authority.Current.sessionToken,
                        stageIndex + 1,
                        false,
                        RewardLaunchContextIdentity.CampaignNormalLv1);
                    C1CampaignDirectLootResolveResult resolved =
                        C1CampaignDirectLootSessionResolver.Resolve(
                            rewardSession,
                            stageClear,
                            authority.CanonicalItemResolver,
                            currentCopies,
                            authority.Current.itemSystemSnapshot
                                .ToBuildSynergyResolutionResult());
                    if (resolved?.accepted != true || resolved.grant == null)
                        break;
                    rewardSession = resolved.snapshot;
                    C1CampaignDirectLootGrantBundle grant = resolved.grant;
                    grants.Add(grant);
                    if (!string.Equals(
                            grant.selectedBaseItemId,
                            initialBaseItemId,
                            StringComparison.Ordinal)
                        && firstIndexByBase.TryGetValue(
                            grant.selectedBaseItemId,
                            out int firstIndex))
                    {
                        return new DuplicateRewardSequence(
                            grants,
                            firstIndex,
                            grants.Count - 1);
                    }
                    if (!firstIndexByBase.ContainsKey(grant.selectedBaseItemId))
                        firstIndexByBase.Add(
                            grant.selectedBaseItemId,
                            grants.Count - 1);
                    currentCopies.TryGetValue(
                        grant.selectedBaseItemId,
                        out int copies);
                    currentCopies[grant.selectedBaseItemId] = copies + 1;
                }
            }
            throw new InvalidOperationException(
                "No deterministic duplicate reward sequence was found.");
        }

        private static C1FormalItemPlacementSnapshot
            PlaceAtFirstLegalBoardCandidate(
                C1FormalItemSessionAuthority authority,
                string itemInstanceId)
        {
            int attempt = 0;
            foreach (int rotation in new[] { 0, 90, 180, 270 })
            {
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 5; x++)
                    {
                        C1FormalItemSessionOperationResult result =
                            authority.Submit(
                                new C1FormalItemArrangementCommand(
                                    C1FormalItemArrangementCommandKind
                                        .PlaceFromTray,
                                    authority.Current.sessionToken,
                                    authority.Current.resetGeneration,
                                    "focused.same-base.place."
                                    + attempt.ToString(
                                        CultureInfo.InvariantCulture),
                                    itemInstanceId,
                                    authority.Current.canonicalSignature,
                                    new C1FormalItemPlacementCandidate(
                                        new UnityEngine.Vector2Int(x, y),
                                        rotation)));
                        attempt++;
                        if (result?.accepted == true)
                        {
                            return authority.Current
                                .FindPlacementByInstanceId(itemInstanceId);
                        }
                    }
                }
            }
            throw new InvalidOperationException(
                "No legal Board placement was found for the first duplicate.");
        }

        private static void VerifyResetRemovesRunReward(
            C1FormalItemSessionAuthority authority,
            string rewardedItemInstanceId)
        {
            C1FormalItemSessionOperationResult reset = authority.Reset(
                new C1FormalItemResetCommand(
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    "focused.canonical.item.reset",
                    authority.Current.canonicalSignature));
            Require(reset?.accepted == true && reset.changed,
                reset?.diagnosticCode ?? "authority reset failed");
            Require(authority.Current.FindRosterEntry(rewardedItemInstanceId) == null,
                "reset removes the run reward");
            Require(authority.Current.FindRosterEntry(
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId) != null,
                "reset rebuilds canonical initial acquisition");
            Equal(0, authority.CampaignLootEntitlementHistory.acceptedCount,
                "reset clears in-memory reward entitlement history");
        }

        private static C1CampaignDirectLootGrantBundle CreateCanonicalGrant(
            C1FormalItemSessionAuthority authority)
        {
            C1CampaignDirectLootSessionCreateResult session =
                C1CampaignDirectLootSessionResolver.CreateSession(
                    authority.Current.sessionToken,
                    771501L);
            Require(session?.accepted == true && session.snapshot != null,
                session?.diagnosticCode ?? "direct loot session creation failed");
            StageClearFact stageClear = new StageClearFact(
                "focused.canonical.stage-clear.1-1",
                "focused.canonical.battle-result.1-1",
                "focused.canonical.battle-request.1-1",
                "focused.canonical.result.1-1",
                C1CampaignDirectLootPoolAndPolicy.ChapterId,
                C1CampaignDirectLootPoolAndPolicy.EligibleStageIds[0],
                authority.Current.sessionToken,
                1,
                false,
                RewardLaunchContextIdentity.CampaignNormalLv1);
            Dictionary<string, int> currentCopies = authority.Current.roster
                .Where(row => !row.isSpecialLightingSource)
                .GroupBy(row => row.baseItemId, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count(),
                    StringComparer.Ordinal);
            C1CampaignDirectLootResolveResult resolved =
                C1CampaignDirectLootSessionResolver.Resolve(
                    session.snapshot,
                    stageClear,
                    authority.CanonicalItemResolver,
                    currentCopies,
                    authority.Current.itemSystemSnapshot
                        .ToBuildSynergyResolutionResult());
            Require(resolved?.accepted == true && resolved.grant != null,
                resolved?.diagnosticCode ?? "canonical direct loot failed");
            return resolved.grant;
        }

        private static C1FormalCampaignLootEntitlementCommand CreateCommand(
            C1FormalItemSessionAuthority authority,
            string commandId,
            C1CampaignDirectLootGrantBundle grant)
        {
            return new C1FormalCampaignLootEntitlementCommand(
                authority.Current.sessionToken,
                authority.Current.resetGeneration,
                commandId,
                authority.Current.canonicalSignature,
                grant);
        }

        private sealed class DuplicateRewardSequence
        {
            public DuplicateRewardSequence(
                IReadOnlyList<C1CampaignDirectLootGrantBundle> grants,
                int firstIndex,
                int secondIndex)
            {
                this.grants = grants;
                this.firstIndex = firstIndex;
                this.secondIndex = secondIndex;
            }

            public IReadOnlyList<C1CampaignDirectLootGrantBundle> grants { get; }
            public int firstIndex { get; }
            public int secondIndex { get; }
        }

        private static void Equal<T>(T expected, T actual, string label)
        {
            assertionCount++;
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(label + " expected <"
                    + expected + "> but was <" + actual + ">.");
            }
        }

        private static void Require(bool condition, string label)
        {
            assertionCount++;
            if (!condition) throw new InvalidOperationException(label + ".");
        }
    }
}
