using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.Items;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.BattleBridge.Formal
{
    public sealed class C1FormalRealtimeBattleSessionAdapter
    {
        private readonly C1FormalRealtimeBattleSession session;
        private readonly Dictionary<string, string> acceptedRefreshInputs =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, C1FormalRealtimeBattleItemRefreshResult>
            acceptedRefreshResults =
                new Dictionary<string, C1FormalRealtimeBattleItemRefreshResult>(
                    StringComparer.Ordinal);

        public C1FormalRealtimeBattleSessionAdapter()
            : this(new C1FormalRealtimeBattleSession())
        {
        }

        public C1FormalRealtimeBattleSessionAdapter(
            C1FormalRealtimeBattleSession session)
        {
            this.session = session
                ?? throw new ArgumentNullException(nameof(session));
        }

        public bool IsActive => session.IsActive;
        public bool IsPaused => session.IsPaused;
        public bool IsTerminal => session.IsTerminal;
        public long CurrentResetGeneration => session.CurrentResetGeneration;
        public long CurrentSessionGeneration => session.CurrentSessionGeneration;
        public C1FormalRealtimeBattleSession Session => session;

        public bool TryStart(
            BattleLaunchContext context,
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string expectedItemSessionCanonicalSignature,
            string sessionId,
            string sessionToken,
            out C1FormalRealtimeBattleSessionStartSnapshot startSnapshot)
        {
            return TryStartFromCumulativeItemSnapshot(
                context,
                itemSnapshot,
                expectedItemSessionCanonicalSignature,
                sessionId,
                sessionToken,
                out startSnapshot);
        }

        public bool TryStartFromCumulativeItemSnapshot(
            BattleLaunchContext context,
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string expectedItemSessionCanonicalSignature,
            string sessionId,
            string sessionToken,
            out C1FormalRealtimeBattleSessionStartSnapshot startSnapshot)
        {
            string stageId = context == null ? string.Empty : context.StageId;
            if (!TryTranslateCanonicalItemSnapshot(
                    stageId,
                    itemSnapshot,
                    expectedItemSessionCanonicalSignature,
                    C1FormalRealtimeBattleErrorCodes.ItemInputRejected,
                    out CumulativeItemProjection projection,
                    out string translationErrorCode,
                    out string translationErrorMessage))
            {
                startSnapshot = RejectedStart(
                    translationErrorCode,
                    translationErrorMessage);
                return false;
            }

            C1FormalRealtimeBattleSessionRequest liveRequest =
                new C1FormalRealtimeBattleSessionRequest(
                    C1FormalRealtimeBattleSessionContract.RequestSchemaId,
                    context.ProductContext,
                    context.ChapterId,
                    context.StageId,
                    context.BalanceProfileId,
                    context.EncounterVariantId,
                    context.LaunchId,
                    context.Generation,
                    sessionId,
                    sessionToken,
                    session.CurrentResetGeneration,
                    itemSnapshot.canonicalSignature,
                    itemSnapshot.arrangementCanonicalSignature,
                    CanonicalItemCatalogContract.CatalogId,
                    C1FormalEnemyDefinitionCatalog.CatalogId,
                    projection.directItemFacts,
                    itemSnapshot.canonicalSignature,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Array.Empty<
                        C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>(),
                    projection.nianCapacityFact);
            bool started = session.TryStart(liveRequest, out startSnapshot);
            if (started)
            {
                ActiveSessionToken = sessionToken ?? string.Empty;
                ActiveStageId = stageId;
                acceptedRefreshInputs.Clear();
                acceptedRefreshResults.Clear();
            }

            return started;
        }

        public bool TryStart(
            BattleLaunchContext context,
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string expectedItemSessionCanonicalSignature,
            string sessionId,
            out C1FormalRealtimeBattleSessionStartSnapshot startSnapshot)
        {
            string token = context == null
                ? string.Empty
                : string.Join(":", new[]
                {
                    context.Token,
                    "live",
                    session.CurrentResetGeneration.ToString(
                        CultureInfo.InvariantCulture)
                });
            return TryStart(
                context,
                itemSnapshot,
                expectedItemSessionCanonicalSignature,
                sessionId,
                token,
                out startSnapshot);
        }

        public bool TryAdvanceBy(
            long deltaMs,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            return session.TryAdvanceBy(deltaMs, out tickResult);
        }

        public bool TryAdvanceTo(
            long targetBattleTimeMs,
            out C1FormalRealtimeBattleTickResult tickResult)
        {
            return session.TryAdvanceTo(targetBattleTimeMs, out tickResult);
        }

        public bool TryPause(out C1FormalRealtimeBattleTickResult tickResult)
        {
            return session.TryPause(out tickResult);
        }

        public bool TryResume(out C1FormalRealtimeBattleTickResult tickResult)
        {
            return session.TryResume(out tickResult);
        }

        public bool TryRefreshItemsWhilePaused(
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string expectedItemSessionCanonicalSignature,
            string refreshCommandId,
            out C1FormalRealtimeBattleItemRefreshResult refreshResult)
        {
            return TryRefreshFromCumulativeItemSnapshotWhilePaused(
                itemSnapshot,
                expectedItemSessionCanonicalSignature,
                refreshCommandId,
                out refreshResult);
        }

        public bool TryRefreshFromCumulativeItemSnapshotWhilePaused(
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string expectedItemSessionCanonicalSignature,
            string refreshCommandId,
            out C1FormalRealtimeBattleItemRefreshResult refreshResult)
        {
            return TryRefreshItemsWhilePausedCore(
                itemSnapshot,
                expectedItemSessionCanonicalSignature,
                refreshCommandId,
                out refreshResult);
        }

        private bool TryRefreshItemsWhilePausedCore(
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string expectedItemSessionCanonicalSignature,
            string refreshCommandId,
            out C1FormalRealtimeBattleItemRefreshResult refreshResult)
        {
            if (!session.TryGetSnapshot(
                    out C1FormalRealtimeBattleSessionStateSnapshot state)
                || state == null)
            {
                refreshResult = RejectedRefresh(
                    C1FormalRealtimeBattleErrorCodes.LifecycleRejected,
                    "No active live Battle Session exists.",
                    itemSnapshot,
                    refreshCommandId,
                    state);
                return false;
            }

            string normalizedCommandId = string.IsNullOrWhiteSpace(
                refreshCommandId)
                ? string.Empty
                : refreshCommandId.Trim();
            string adapterInputCanonical = string.Join("|", new[]
            {
                itemSnapshot == null
                    ? string.Empty
                    : itemSnapshot.canonicalSignature,
                string.IsNullOrWhiteSpace(
                    expectedItemSessionCanonicalSignature)
                    ? string.Empty
                    : expectedItemSessionCanonicalSignature.Trim()
            });
            if (normalizedCommandId.Length > 0
                && acceptedRefreshInputs.TryGetValue(
                    normalizedCommandId,
                    out string acceptedInput))
            {
                C1FormalRealtimeBattleItemRefreshResult acceptedResult =
                    acceptedRefreshResults[normalizedCommandId];
                if (!string.Equals(
                        acceptedInput,
                        adapterInputCanonical,
                        StringComparison.Ordinal))
                {
                    refreshResult = RejectedRefresh(
                        C1FormalRealtimeBattleErrorCodes.RefreshCommandConflict,
                        "refreshCommandId was already accepted with another Item input.",
                        itemSnapshot,
                        normalizedCommandId,
                        state);
                    return false;
                }

                if (acceptedResult.stateSnapshot == null
                    || !string.Equals(
                        state.canonicalSignature,
                        acceptedResult.stateSnapshot.canonicalSignature,
                        StringComparison.Ordinal))
                {
                    refreshResult = RejectedRefresh(
                        C1FormalRealtimeBattleErrorCodes.RefreshStateStale,
                        "The accepted Item refresh no longer targets the current state.",
                        itemSnapshot,
                        normalizedCommandId,
                        state);
                    return false;
                }

                refreshResult = acceptedResult;
                return true;
            }

            if (!TryTranslateCanonicalItemSnapshot(
                    ActiveStageId,
                    itemSnapshot,
                    expectedItemSessionCanonicalSignature,
                    C1FormalRealtimeBattleErrorCodes.RefreshInputRejected,
                    out CumulativeItemProjection projection,
                    out string translationCode,
                    out string translationMessage))
            {
                refreshResult = RejectedRefresh(
                    translationCode,
                    translationMessage,
                    itemSnapshot,
                    normalizedCommandId,
                    state);
                return false;
            }

            C1FormalRealtimeBattleItemRefreshRequest request =
                new C1FormalRealtimeBattleItemRefreshRequest(
                    C1FormalRealtimeBattleSessionContract
                        .ItemRefreshRequestSchemaId,
                    state.sessionId,
                    ActiveSessionToken,
                    state.sessionGeneration,
                    state.resetGeneration,
                    refreshCommandId,
                    state.battleTimeMs,
                    state.canonicalSignature,
                    itemSnapshot.canonicalSignature,
                    expectedItemSessionCanonicalSignature,
                    CanonicalItemCatalogContract.CatalogId,
                    projection.directItemFacts,
                    true,
                    itemSnapshot.canonicalSignature,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Array.Empty<
                        C1FormalRealtimeBattlePool15ActiveItemFactSnapshot>(),
                    projection.nianCapacityFact);
            bool accepted = session.TryRefreshItemsWhilePaused(
                request,
                out refreshResult);
            if (accepted && refreshResult != null && refreshResult.accepted)
            {
                acceptedRefreshInputs.Add(
                    normalizedCommandId,
                    adapterInputCanonical);
                acceptedRefreshResults.Add(normalizedCommandId, refreshResult);
            }

            return accepted;
        }

        public bool TryGetSnapshot(
            out C1FormalRealtimeBattleSessionStateSnapshot snapshot)
        {
            return session.TryGetSnapshot(out snapshot);
        }

        public long Reset()
        {
            if (!session.IsActive
                || !session.TryGetSnapshot(
                    out C1FormalRealtimeBattleSessionStateSnapshot snapshot))
            {
                ActiveSessionToken = string.Empty;
                ActiveStageId = string.Empty;
                acceptedRefreshInputs.Clear();
                acceptedRefreshResults.Clear();
                return session.CurrentResetGeneration;
            }

            session.TryReset(
                snapshot.sessionId,
                ActiveSessionToken,
                snapshot.sessionGeneration,
                snapshot.resetGeneration,
                out C1FormalRealtimeBattleTickResult _);
            ActiveSessionToken = string.Empty;
            ActiveStageId = string.Empty;
            acceptedRefreshInputs.Clear();
            acceptedRefreshResults.Clear();
            return session.CurrentResetGeneration;
        }

        public long Unbind()
        {
            return Reset();
        }

        private string ActiveSessionToken { get; set; } = string.Empty;
        private string ActiveStageId { get; set; } = string.Empty;

        private sealed class CumulativeItemProjection
        {
            internal CumulativeItemProjection(
                IEnumerable<C1FormalRealtimeBattleItemFactSnapshot>
                    directItemFacts,
                C1FormalRealtimeBattleNianCapacitySnapshot nianCapacityFact)
            {
                this.directItemFacts = Array.AsReadOnly((directItemFacts
                    ?? Enumerable.Empty<
                        C1FormalRealtimeBattleItemFactSnapshot>()).ToArray());
                this.nianCapacityFact = nianCapacityFact;
            }

            internal IReadOnlyList<C1FormalRealtimeBattleItemFactSnapshot>
                directItemFacts { get; }
            internal C1FormalRealtimeBattleNianCapacitySnapshot
                nianCapacityFact { get; }
        }

        private static bool TryTranslateCanonicalItemSnapshot(
            string stageId,
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string expectedItemSessionCanonicalSignature,
            string rejectionCode,
            out CumulativeItemProjection projection,
            out string errorCode,
            out string message)
        {
            projection = null;
            bool acceptedStage = IsCurrentCanonicalEnemyStage(stageId);
            string expectedSession = string.IsNullOrWhiteSpace(
                    expectedItemSessionCanonicalSignature)
                ? string.Empty
                : expectedItemSessionCanonicalSignature.Trim();
            if (!acceptedStage
                || itemSnapshot == null
                || !string.Equals(
                    itemSnapshot.schemaId,
                    C1FormalItemSessionContract.BattleInputSchemaId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    itemSnapshot.productContext,
                    C1FormalRealtimeBattleSessionContract.ProductContext,
                    StringComparison.Ordinal)
                || expectedSession.Length == 0
                || !string.Equals(
                    itemSnapshot.sessionCanonicalSignature,
                    expectedSession,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(itemSnapshot.canonicalSignature)
                || string.IsNullOrWhiteSpace(
                    itemSnapshot.arrangementCanonicalSignature)
                || string.IsNullOrWhiteSpace(
                    itemSnapshot.itemSystemCanonicalSignature))
            {
                errorCode = rejectionCode;
                message = "The canonical Item snapshot is missing or stale.";
                return false;
            }

            if (!TryProjectNianCapacity(
                    itemSnapshot,
                    out C1FormalRealtimeBattleNianCapacitySnapshot
                        nianCapacityFact,
                    out message))
            {
                errorCode = C1FormalRealtimeBattleErrorCodes
                    .NianCapacityRejected;
                return false;
            }

            IReadOnlyList<C1FormalItemBattleItemRow> rows =
                itemSnapshot.itemRows;
            if (rows == null
                || rows.Count > CanonicalItemCatalogContract.OrdinaryItemCount
                || rows.Any(row => row == null)
                || rows.Select(row => row.itemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() != rows.Count)
            {
                errorCode = rejectionCode;
                message = "The canonical Item snapshot contains invalid or duplicate ItemInstance rows.";
                return false;
            }

            List<C1FormalRealtimeBattleItemFactSnapshot> activeItems =
                new List<C1FormalRealtimeBattleItemFactSnapshot>();
            foreach (C1FormalItemBattleItemRow row in rows.OrderBy(
                         value => value.itemInstanceId,
                         StringComparer.Ordinal))
            {
                bool placementAccepted = row.isPlaced
                    ? row.isLit.HasValue
                    : !row.isLit.HasValue;
                ItemGeneratedInstanceSnapshot generated =
                    row.generatedInstance;
                CanonicalItemDefinition definition = row.canonicalDefinition;
                if (!placementAccepted
                    || generated == null
                    || generated.identity == null
                    || definition == null
                    || !definition.isOrdinaryDropEligible
                    || !string.Equals(
                        generated.itemInstanceId,
                        row.itemInstanceId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        generated.baseItemId,
                        row.baseItemId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        definition.baseItemId,
                        row.baseItemId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        row.itemCatalogCanonicalSignature,
                        CanonicalItemCatalogContract.CatalogId,
                        StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(row.instanceCanonicalSignature))
                {
                    errorCode = rejectionCode;
                    message = "An Item row is not hydrated from the canonical Item catalog.";
                    return false;
                }

                if (!IsActiveItemRow(row))
                {
                    continue;
                }

                long cooldownTurns = ReadGeneratedStat(generated, "cooldown");
                long nianCostUnits = ReadGeneratedStat(generated, "nianCost");
                long damageUnits = ReadGeneratedStat(generated, "damage");
                if (cooldownTurns <= 0L
                    || cooldownTurns > long.MaxValue / 400L
                    || nianCostUnits <= 0L
                    || nianCostUnits > int.MaxValue
                    || damageUnits < 0L
                    || damageUnits > int.MaxValue)
                {
                    errorCode = C1FormalRealtimeBattleErrorCodes
                        .NianActionCostRejected;
                    message = "A canonical active Item has invalid rolled Battle stats.";
                    return false;
                }

                long cadenceMs = cooldownTurns * 400L;
                C1FormalRealtimeBattleItemActionCostSnapshot actionCost =
                    new C1FormalRealtimeBattleItemActionCostSnapshot(
                        row.itemInstanceId,
                        row.baseItemId,
                        row.rarityKey,
                        row.baseItemId + "@" + row.rarityKey,
                        "canonical-item-action",
                        nianCapacityFact.resourceKey,
                        checked((int)nianCostUnits),
                        CanonicalItemCatalogContract.CatalogVersion,
                        CanonicalItemCatalogContract.CatalogVersion,
                        CanonicalItemCatalogContract.CatalogId,
                        row.instanceCanonicalSignature,
                        cadenceMs,
                        cadenceMs);
                activeItems.Add(
                    new C1FormalRealtimeBattleItemFactSnapshot(
                        row.itemInstanceId,
                        row.itemInstanceId,
                        row.baseItemId,
                        row.baseItemId + "@" + row.rarityKey,
                        true,
                        checked((int)damageUnits),
                        cadenceMs,
                        row.instanceCanonicalSignature,
                        CanonicalItemCatalogContract.CatalogId,
                        actionCost,
                        null,
                        generated,
                        definition,
                        row.placementId,
                        row.anchorCell.GetValueOrDefault().x,
                        row.anchorCell.GetValueOrDefault().y,
                        row.rotation,
                        row.isDirectLit == true));
            }

            projection = new CumulativeItemProjection(
                activeItems,
                nianCapacityFact);
            errorCode = C1FormalRealtimeBattleErrorCodes.None;
            message = string.Empty;
            return true;
        }

        private static bool IsCurrentCanonicalEnemyStage(string stageId)
        {
            return string.Equals(stageId, "1-1", StringComparison.Ordinal)
                || string.Equals(stageId, "1-2", StringComparison.Ordinal)
                || string.Equals(stageId, "1-3", StringComparison.Ordinal)
                || string.Equals(stageId, "1-4", StringComparison.Ordinal)
                || string.Equals(stageId, "1-5", StringComparison.Ordinal);
        }

        private static long ReadGeneratedStat(
            ItemGeneratedInstanceSnapshot generated,
            string statId)
        {
            ItemGeneratedStatSnapshot row = generated?.GeneratedStats?
                .SingleOrDefault(value => string.Equals(
                    value.statId,
                    statId,
                    StringComparison.Ordinal));
            return row == null ? 0L : row.rawUnits;
        }

        private static bool TryProjectNianCapacity(
            C1FormalItemBattleInputSnapshot itemSnapshot,
            out C1FormalRealtimeBattleNianCapacitySnapshot projected,
            out string message)
        {
            projected = null;
            C1FormalI031NianCapacityFact fact =
                itemSnapshot == null ? null : itemSnapshot.i031NianCapacityFact;
            bool accepted = fact != null
                && string.Equals(
                    fact.schemaId,
                    C1FormalI031NianCapacityProjection.SchemaId,
                    StringComparison.Ordinal)
                && string.Equals(
                    fact.productContext,
                    itemSnapshot.productContext,
                    StringComparison.Ordinal)
                && string.Equals(
                    fact.resourceKey,
                    C1FormalI031NianCapacityProjection.ResourceKey,
                    StringComparison.Ordinal)
                && fact.initialNian >= 0
                && fact.maxNian > 0
                && fact.initialNian <= fact.maxNian
                && fact.generationAmount > 0
                && fact.generationIntervalMilliseconds > 0L
                && !string.IsNullOrWhiteSpace(fact.sourceRevision)
                && !string.IsNullOrWhiteSpace(fact.sourceProfileId)
                && string.Equals(
                    fact.specialItemInstanceId,
                    I031InventoryPlacementContract.SpecialIdentityId,
                    StringComparison.Ordinal)
                && string.Equals(
                    fact.baseItemId,
                    I031InventoryPlacementContract.ItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    fact.stablePlacementId,
                    itemSnapshot.sourcePlacementId,
                    StringComparison.Ordinal)
                && itemSnapshot.sourceAnchorCell.HasValue
                && fact.anchorCell == itemSnapshot.sourceAnchorCell.Value
                && string.Equals(
                    fact.sessionCanonicalSignature,
                    itemSnapshot.sessionCanonicalSignature,
                    StringComparison.Ordinal)
                && string.Equals(
                    fact.arrangementCanonicalSignature,
                    itemSnapshot.arrangementCanonicalSignature,
                    StringComparison.Ordinal)
                && string.Equals(
                    fact.itemSystemCanonicalSignature,
                    itemSnapshot.itemSystemCanonicalSignature,
                    StringComparison.Ordinal);
            if (!accepted)
            {
                message = "The formal I031 Nian capacity fact is missing or stale.";
                return false;
            }

            projected = new C1FormalRealtimeBattleNianCapacitySnapshot(
                fact.productContext,
                fact.resourceKey,
                fact.initialNian,
                fact.maxNian,
                fact.generationAmount,
                fact.generationIntervalMilliseconds,
                fact.sourceRevision,
                fact.sourceProfileId,
                fact.specialItemInstanceId,
                fact.baseItemId,
                fact.stablePlacementId,
                fact.anchorCell.x,
                fact.anchorCell.y,
                fact.sessionCanonicalSignature,
                fact.arrangementCanonicalSignature,
                fact.itemSystemCanonicalSignature);
            message = string.Empty;
            return true;
        }

        private static bool IsActiveItemRow(C1FormalItemBattleItemRow row)
        {
            return row != null
                   && row.isPlaced
                   && row.isLit.GetValueOrDefault();
        }

        private C1FormalRealtimeBattleItemRefreshResult RejectedRefresh(
            string errorCode,
            string message,
            C1FormalItemBattleInputSnapshot itemSnapshot,
            string refreshCommandId,
            C1FormalRealtimeBattleSessionStateSnapshot state)
        {
            return new C1FormalRealtimeBattleItemRefreshResult(
                false,
                new C1FormalRealtimeBattleError(errorCode, message),
                state == null ? string.Empty : state.sessionId,
                state == null ? session.CurrentSessionGeneration :
                    state.sessionGeneration,
                state == null ? session.CurrentResetGeneration :
                    state.resetGeneration,
                state == null ? session.BattleTimeMs : state.battleTimeMs,
                itemSnapshot == null
                    ? string.Empty
                    : itemSnapshot.canonicalSignature,
                string.Empty,
                0,
                0,
                0,
                0,
                state,
                Array.Empty<C1FormalRealtimeBattleCue>());
        }

        private static C1FormalRealtimeBattleSessionStartSnapshot RejectedStart(
            string code,
            string message)
        {
            return new C1FormalRealtimeBattleSessionStartSnapshot(
                false,
                new C1FormalRealtimeBattleError(code, message),
                null,
                Array.Empty<C1FormalRealtimeBattleCue>());
        }
    }
}
