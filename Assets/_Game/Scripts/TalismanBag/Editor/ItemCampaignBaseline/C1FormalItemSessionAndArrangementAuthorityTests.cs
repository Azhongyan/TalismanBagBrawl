using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TalismanBag.Items;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.V04.RewardDrop.Contracts;
using TalismanBag.V04.RewardDrop.Runtime.CampaignLoot;
using UnityEngine;

#if C1_FORMAL_ITEM_SESSION_SCOPED_STANDALONE
namespace UnityEngine
{
    public readonly struct Vector2Int : IEquatable<Vector2Int>
    {
        public Vector2Int(int x, int y) { this.x = x; this.y = y; }
        public int x { get; }
        public int y { get; }
        public static Vector2Int zero => new Vector2Int(0, 0);
        public static Vector2Int operator +(Vector2Int a, Vector2Int b) =>
            new Vector2Int(a.x + b.x, a.y + b.y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) =>
            new Vector2Int(a.x - b.x, a.y - b.y);
        public static bool operator ==(Vector2Int a, Vector2Int b) => a.Equals(b);
        public static bool operator !=(Vector2Int a, Vector2Int b) => !a.Equals(b);
        public bool Equals(Vector2Int other) => x == other.x && y == other.y;
        public override bool Equals(object obj) => obj is Vector2Int other && Equals(other);
        public override int GetHashCode() { unchecked { return (x * 397) ^ y; } }
        public override string ToString() => "(" + x + ", " + y + ")";
    }

    public static class Mathf
    {
        public static int Abs(int value) => Math.Abs(value);
    }
}

public static class C1FormalItemSessionScopedProgram
{
    public static int Main()
    {
        return TalismanBag.Editor.ItemCampaignBaseline
            .C1FormalItemSessionAndArrangementAuthorityTests.RunFocused();
    }
}
#endif

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public static class C1FormalItemSessionAndArrangementAuthorityTests
    {
        private const string TerminalMarker =
            "C1_FORMAL_ITEM_SESSION_AND_ARRANGEMENT_AUTHORITY_PASS";
        private const string AuthorityPath =
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1FormalItemSessionAndArrangementAuthority.cs";
        private static int assertionCount;

        public static int RunFocused()
        {
            try
            {
                assertionCount = 0;
                VerifyInitialSession();
                C1FormalItemSessionAuthority entitled = VerifyEntitlement();
                VerifyTrayLayoutAndTransactions();
                BattleEvidence evidence = VerifyRealLightingAndTransactions(entitled);
                VerifyFailClosedMatrix();
                VerifyResetAndDeterminism(evidence);
                VerifyImmutabilityAndIsolation();
                Console.WriteLine(TerminalMarker
                    + " / assertions=" + assertionCount.ToString(CultureInfo.InvariantCulture)
                    + " / initial=" + CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_FORMAL_ITEM_SESSION_AND_ARRANGEMENT_AUTHORITY_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        public static int RunRev10I031NianCapacityFact()
        {
            try
            {
                assertionCount = 0;
                C1FormalItemSessionAuthority authority = NewAuthority(
                    "rev10-i031-nian-capacity",
                    1);
                C1FormalItemSessionSnapshot current = authority.Current;
                C1FormalItemPlacementSnapshot sourcePlacement = current
                    .FindPlacementByInstanceId(
                        I031InventoryPlacementContract.SpecialIdentityId);
                C1FormalItemBattleInputSnapshot battle = authority
                    .CreateBattleInputSnapshot();
                C1FormalI031NianCapacityFact capacity =
                    battle.i031NianCapacityFact;

                Require(capacity != null,
                    "REV10 current I031 source publishes capacity");
                Equal(C1FormalI031NianCapacityProjection.SchemaId,
                    capacity.schemaId,
                    "REV10 capacity schema");
                Equal(C1FormalItemSessionContract.ProductContext,
                    capacity.productContext,
                    "REV10 capacity product context");
                Equal("nian", capacity.resourceKey,
                    "REV10 capacity resource");
                Equal(50, capacity.initialNian,
                    "REV10 initial Nian");
                Equal(50, capacity.maxNian,
                    "REV10 maximum Nian");
                Equal(10, capacity.generationAmount,
                    "REV10 generated Nian per interval");
                Equal(500L, capacity.generationIntervalMilliseconds,
                    "REV10 Nian generation interval");
                Equal(
                    "C1_CAMPAIGN_LV1_I031_NIAN_ECONOMY_FORMAL_V1",
                    capacity.sourceRevision,
                    "REV10 source revision");
                Equal(
                    "campaign.normal.lv1.item.i031.nian-economy.formal.v1",
                    capacity.sourceProfileId,
                    "REV10 source profile");
                Equal(I031InventoryPlacementContract.SpecialIdentityId,
                    capacity.specialItemInstanceId,
                    "REV10 exact special identity");
                Equal(I031InventoryPlacementContract.ItemId,
                    capacity.baseItemId,
                    "REV10 exact base identity");
                Equal(I031InventoryPlacementContract.StablePlacementId,
                    capacity.stablePlacementId,
                    "REV10 stable placement identity");
                Equal(sourcePlacement.anchorCell, capacity.anchorCell,
                    "REV10 current I031 anchor");
                Equal(current.canonicalSignature,
                    capacity.sessionCanonicalSignature,
                    "REV10 current session lineage");
                Equal(current.arrangementCanonicalSignature,
                    capacity.arrangementCanonicalSignature,
                    "REV10 current arrangement lineage");
                Equal(current.itemSystemCanonicalSignature,
                    capacity.itemSystemCanonicalSignature,
                    "REV10 current ItemSystem lineage");

                C1FormalI031NianCapacityProjectionRequest currentRequest =
                    C1FormalI031NianCapacityProjection.CreateCurrentRequest(
                        current);
                C1FormalI031NianCapacityProjectionRequest staleRequest =
                    new C1FormalI031NianCapacityProjectionRequest(
                        currentRequest.expectedSessionCanonicalSignature,
                        currentRequest.expectedArrangementCanonicalSignature
                        + ":stale",
                        currentRequest.expectedItemSystemCanonicalSignature,
                        currentRequest.specialItemInstanceId,
                        currentRequest.baseItemId,
                        currentRequest.stablePlacementId,
                        currentRequest.anchorCell);
                C1FormalI031NianCapacityProjectionResult stale =
                    C1FormalI031NianCapacityProjection.TryProject(
                        current,
                        staleRequest);
                Require(stale != null && !stale.isSuccess
                        && stale.fact == null,
                    "REV10 stale projection rejects without a placeholder");
                Equal(
                    C1FormalI031NianCapacityProjection
                        .UnavailableDiagnosticCode,
                    stale.diagnosticCode,
                    "REV10 stale projection diagnostic");

                int ordinaryCount = battle.itemRows.Count;
                C1FormalItemSessionOperationResult returned = authority.Submit(
                    Arrangement(
                        authority,
                        "rev10-return-i031",
                        C1FormalItemArrangementCommandKind.ReturnToTray,
                        I031InventoryPlacementContract.SpecialIdentityId,
                        Vector2Int.zero,
                        0,
                        false));
                Require(returned.accepted && returned.changed,
                    "REV10 I031 return-to-Tray is accepted");
                C1FormalItemBattleInputSnapshot withoutSource = authority
                    .CreateBattleInputSnapshot();
                Require(withoutSource.i031NianCapacityFact == null,
                    "REV10 Tray I031 publishes no capacity fact");
                Equal(string.Empty, withoutSource.sourcePlacementId,
                    "REV10 Tray I031 has no Board source placement");
                Equal(ordinaryCount, withoutSource.itemRows.Count,
                    "REV10 ordinary Battle rows survive missing source");
                C1FormalItemBattleItemRow i001 = withoutSource.itemRows.Single(
                    row => string.Equals(
                        row.itemInstanceId,
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                        StringComparison.Ordinal));
                Require(i001.actionCostFact != null
                        && i001.actionCostFact.cost == 1,
                    "REV10 ordinary I001 action fact remains intact");

                string authoritySource = File.ReadAllText(
                    Path.GetFullPath(AuthorityPath));
                foreach (string prohibited in new[]
                         {
                             "BattleSandboxNianResourceSnapshot",
                             "I031NianSourceSnapshot",
                             "Lv40 Orange",
                             "DevSession",
                             "Shougunu"
                         })
                {
                    Require(authoritySource.IndexOf(
                                prohibited,
                                StringComparison.Ordinal) < 0,
                        "REV10 authority excludes " + prohibited);
                }

                Console.WriteLine(
                    "C1_REV10_I031_NIAN_CAPACITY_FACT_PASS"
                    + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture)
                    + " / initial=" + capacity.initialNian
                    + " / max=" + capacity.maxNian);
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_REV10_I031_NIAN_CAPACITY_FACT_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        private static C1FormalItemSessionAuthority VerifyInitialSession()
        {
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create("focused-session", 1);
            Require(creation != null && creation.isSuccess,
                "initial session creation must succeed: "
                + (creation == null ? "null" : creation.diagnosticCode));
            C1FormalItemSessionAuthority authority = creation.authority;
            C1FormalItemSessionSnapshot snapshot = authority.Current;
            Equal(C1FormalItemSessionContract.SessionSchemaId, snapshot.schemaId,
                "session schema");
            Equal(C1FormalItemSessionContract.ProductContext, snapshot.productContext,
                "product context");
            Equal(2, snapshot.roster.Count, "initial roster count");
            Equal(2, snapshot.placements.Count, "initial placement count");
            Equal(C1FormalItemSessionContract.TrayLayoutSchemaId,
                snapshot.trayLayout.schemaId,
                "initial Tray layout schema");
            Equal(5, snapshot.trayLayout.columnCount, "initial Tray columns");
            Equal(13, snapshot.trayLayout.rowCount, "initial Tray rows");
            Equal(65, snapshot.trayLayout.cellCount, "initial Tray cells");
            Equal(2, snapshot.trayLayout.placements.Count,
                "initial Tray row count");
            Require(snapshot.trayLayout.placements.All(value =>
                    !value.isActiveInTray),
                "initial Board items retain inactive remembered Tray rows");
            Require(snapshot.itemSystemSnapshot.isValid,
                "initial ItemSystemSnapshot.v2 must be valid");
            Equal(ItemSystemSnapshot.CurrentSchemaVersion,
                snapshot.itemSystemSnapshot.schemaVersion,
                "real ItemSystem schema");

            C1FormalItemRosterEntrySnapshot i031 = snapshot.FindRosterEntry(
                I031InventoryPlacementContract.SpecialIdentityId);
            Require(i031 != null && i031.isSpecialLightingSource,
                "I031 must be the explicit special source roster row");
            Equal(I031InventoryPlacementContract.ItemId, i031.baseItemId,
                "I031 base identity");
            Equal(string.Empty, i031.rarityKey,
                "I031 rarity remains absent");
            Require(i031.ordinaryInstance == null,
                "I031 must not become an ordinary instance");

            Require(CanonicalInitialItemAcquisitionPolicy.TryCreate(
                    authority.CanonicalItemResolver,
                    out CanonicalItemDefinition selectedDefinition,
                    out ItemGeneratedInstanceSnapshot generatedInitialItem,
                    out string initialDiagnostic),
                initialDiagnostic);
            C1FormalItemRosterEntrySnapshot i001 = snapshot.FindRosterEntry(
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
            Require(i001 != null && !i001.isSpecialLightingSource,
                "canonical initial ordinary roster row");
            Equal(selectedDefinition.baseItemId, i001.baseItemId,
                "initial base identity follows canonical policy");
            Equal(generatedInitialItem.itemInstanceId, i001.itemInstanceId,
                "initial generated instance identity");
            Equal("white", i001.rarityKey, "initial rarity identity");
            Equal(string.Empty,
                i001.ordinaryInstance.ordinaryIdentity.cultivationPotentialProfileId,
                "unsupported cultivation remains absent");

            C1FormalItemPlacementSnapshot sourcePlacement =
                snapshot.FindPlacementByInstanceId(
                    I031InventoryPlacementContract.SpecialIdentityId);
            C1FormalItemPlacementSnapshot i001Placement =
                snapshot.FindPlacementByInstanceId(i001.itemInstanceId);
            Equal(new Vector2Int(1, 1), sourcePlacement.anchorCell,
                "initial I031 anchor");
            Equal(I031InventoryPlacementContract.StablePlacementId,
                sourcePlacement.placementId, "I031 stable placement identity");
            Equal(new Vector2Int(0, 1), i001Placement.anchorCell,
                "initial I001 anchor");
            ItemSystemPlacementSnapshot resolved = snapshot.itemSystemSnapshot
                .FindPlacement(i001Placement.placementId);
            Require(resolved != null && resolved.isLit && resolved.isDirectLit,
                "I001 direct lighting must come from real ItemSystem");
            Equal(I031InventoryPlacementContract.StablePlacementId,
                resolved.litByPlacementId, "I001 real lighting source placement");

            C1FormalItemBattleInputSnapshot battle =
                authority.CreateBattleInputSnapshot();
            Equal(1, battle.itemRows.Count,
                "initial Battle input excludes I031 ordinary row");
            Equal(i001.itemInstanceId, battle.itemRows[0].itemInstanceId,
                "initial Battle I001 instance");
            Equal(true, battle.itemRows[0].isLit,
                "initial Battle real I001 lighting");
            Equal(I031InventoryPlacementContract.StablePlacementId,
                battle.sourcePlacementId,
                "Battle evidence retains I031 placement");
            return authority;
        }

        private static void VerifyTrayLayoutAndTransactions()
        {
            Equal(1, (int)C1FormalItemArrangementCommandKind.PlaceFromTray,
                "PlaceFromTray enum value preserved");
            Equal(2, (int)C1FormalItemArrangementCommandKind.MoveOnBoard,
                "MoveOnBoard enum value preserved");
            Equal(3, (int)C1FormalItemArrangementCommandKind.ReturnToTray,
                "ReturnToTray enum value preserved");
            Equal(4, (int)C1FormalItemArrangementCommandKind.MoveWithinTray,
                "MoveWithinTray additive enum value");

            C1FormalItemSessionAuthority authority = NewAuthority("tray-layout", 1);
            RewardFixture fixture = RewardFixture.Create(
                authority, "tray-layout", null);
            Require(authority.AcceptCanonicalCampaignLootEntitlement(
                    fixture.Command(authority, "tray-entitle")).accepted,
                "Tray entitlement setup");
            string i002 = fixture.ItemInstanceId;
            string i001 = CanonicalInitialItemAcquisitionPolicy.ItemInstanceId;
            C1FormalItemTrayPlacementSnapshot initialI002 = authority.Current
                .trayLayout.FindPlacementByInstanceId(i002);
            Require(initialI002 != null && initialI002.isActiveInTray,
                "entitled I002 is active in authoritative Tray layout");
            Equal(Vector2Int.zero, initialI002.anchorCell,
                "entitled I002 first deterministic anchor");
            Require(initialI002.occupiedCells.Count > 0
                    && C1FormalItemTrayLayoutRules.IsInsideTray(
                        initialI002.occupiedCells),
                "entitled I002 occupied cells are legal");

            string i001Remembered = authority.Current.trayLayout
                .FindPlacementByInstanceId(i001).canonicalSignature;
            string i031Remembered = authority.Current.trayLayout
                .FindPlacementByInstanceId(
                    I031InventoryPlacementContract.SpecialIdentityId)
                .canonicalSignature;
            C1FormalItemArrangementCommand move = Arrangement(
                authority,
                "tray-move-i002",
                C1FormalItemArrangementCommandKind.MoveWithinTray,
                i002,
                new Vector2Int(2, 5),
                270);
            C1FormalItemSessionOperationResult moved = authority.Submit(move);
            Require(moved.accepted && moved.changed,
                "valid MoveWithinTray accepts atomically");
            C1FormalItemTrayPlacementSnapshot movedI002 = authority.Current
                .trayLayout.FindPlacementByInstanceId(i002);
            Equal(new Vector2Int(2, 5), movedI002.anchorCell,
                "MoveWithinTray authoritative anchor");
            Equal(270, movedI002.rotation,
                "MoveWithinTray authoritative rotation");
            Equal(i001Remembered, authority.Current.trayLayout
                    .FindPlacementByInstanceId(i001).canonicalSignature,
                "MoveWithinTray preserves unrelated I001 row");
            Equal(i031Remembered, authority.Current.trayLayout
                    .FindPlacementByInstanceId(
                        I031InventoryPlacementContract.SpecialIdentityId)
                    .canonicalSignature,
                "MoveWithinTray preserves unrelated I031 row");

            C1FormalItemSessionSnapshot afterMove = authority.Current;
            C1FormalItemSessionOperationResult duplicate = authority.Submit(move);
            Require(duplicate.accepted && !duplicate.changed
                    && ReferenceEquals(afterMove, authority.Current),
                "MoveWithinTray exact duplicate is idempotent");
            C1FormalItemSessionOperationResult semanticNoOp = authority.Submit(
                Arrangement(
                    authority,
                    "tray-move-i002-noop",
                    C1FormalItemArrangementCommandKind.MoveWithinTray,
                    i002,
                    new Vector2Int(2, 5),
                    270));
            Require(semanticNoOp.accepted && !semanticNoOp.changed
                    && ReferenceEquals(afterMove, authority.Current),
                "MoveWithinTray same placement keeps exact snapshot");

            RejectArrangement(authority, Arrangement(
                    authority,
                    "tray-bounds",
                    C1FormalItemArrangementCommandKind.MoveWithinTray,
                    i002,
                    new Vector2Int(4, 12),
                    0),
                C1FormalItemSessionDiagnosticCodes.TrayPlacementBounds,
                "Tray bounds rejection");
            RejectArrangement(authority, new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.MoveWithinTray,
                    "wrong-token",
                    authority.Current.resetGeneration,
                    "tray-stale-token",
                    i002,
                    authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(new Vector2Int(1, 8), 0)),
                C1FormalItemSessionDiagnosticCodes.SessionTokenMismatch,
                "Tray stale token");
            RejectArrangement(authority, new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.MoveWithinTray,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration + 1,
                    "tray-stale-generation",
                    i002,
                    authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(new Vector2Int(1, 8), 0)),
                C1FormalItemSessionDiagnosticCodes.ResetGenerationStale,
                "Tray stale generation");
            RejectArrangement(authority, new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.MoveWithinTray,
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    "tray-stale-signature",
                    i002,
                    new string('0', 64),
                    new C1FormalItemPlacementCandidate(new Vector2Int(1, 8), 0)),
                C1FormalItemSessionDiagnosticCodes.ExpectedSessionSignatureStale,
                "Tray stale signature");

            Require(authority.Submit(Arrangement(
                    authority,
                    "tray-place-i002-board",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002,
                    new Vector2Int(3, 3),
                    0)).accepted,
                "Tray remembered return board placement setup");
            C1FormalItemTrayPlacementSnapshot inactiveI002 = authority.Current
                .trayLayout.FindPlacementByInstanceId(i002);
            Require(!inactiveI002.isActiveInTray
                    && inactiveI002.anchorCell == new Vector2Int(2, 5)
                    && inactiveI002.rotation == 270,
                "PlaceFromTray retains remembered Tray placement");
            Require(authority.Submit(Arrangement(
                    authority,
                    "tray-return-i002-remembered",
                    C1FormalItemArrangementCommandKind.ReturnToTray,
                    i002,
                    default,
                    0,
                    false)).accepted,
                "ReturnToTray restores remembered candidate");
            C1FormalItemTrayPlacementSnapshot restoredI002 = authority.Current
                .trayLayout.FindPlacementByInstanceId(i002);
            Equal(new Vector2Int(2, 5), restoredI002.anchorCell,
                "remembered return anchor restored");
            Equal(270, restoredI002.rotation,
                "remembered return rotation restored");

            Require(authority.Submit(Arrangement(
                    authority,
                    "tray-place-i002-board-again",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002,
                    new Vector2Int(3, 3),
                    0)).accepted,
                "occupied remembered fallback board setup");
            Require(authority.Submit(Arrangement(
                    authority,
                    "tray-return-i001",
                    C1FormalItemArrangementCommandKind.ReturnToTray,
                    i001,
                    default,
                    0,
                    false)).accepted,
                "return I001 to Tray for fallback occupancy");
            Require(authority.Submit(Arrangement(
                    authority,
                    "tray-move-i001-to-remembered",
                    C1FormalItemArrangementCommandKind.MoveWithinTray,
                    i001,
                    new Vector2Int(2, 5),
                    0)).accepted,
                "move I001 onto I002 remembered anchor");
            Require(authority.Submit(Arrangement(
                    authority,
                    "tray-return-i002-fallback",
                    C1FormalItemArrangementCommandKind.ReturnToTray,
                    i002,
                    default,
                    0,
                    false)).accepted,
                "occupied remembered return uses first legal fallback");
            C1FormalItemTrayPlacementSnapshot fallbackI002 = authority.Current
                .trayLayout.FindPlacementByInstanceId(i002);
            Require(fallbackI002.isActiveInTray
                    && fallbackI002.anchorCell != new Vector2Int(2, 5),
                "occupied remembered anchor changed to deterministic fallback");
            Equal(new Vector2Int(2, 5), authority.Current.trayLayout
                    .FindPlacementByInstanceId(i001).anchorCell,
                "fallback keeps unrelated I001 anchor stable");
            RejectArrangement(authority, Arrangement(
                    authority,
                    "tray-collision",
                    C1FormalItemArrangementCommandKind.MoveWithinTray,
                    i002,
                    new Vector2Int(2, 4),
                    0),
                C1FormalItemSessionDiagnosticCodes.TrayPlacementCollision,
                "Tray collision rejection");

            MethodInfo firstLegal = typeof(C1FormalItemSessionAuthority).GetMethod(
                "TryFindFirstLegalTrayPlacement",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(firstLegal != null,
                "bounded full-Tray first-legal seam exists");
            ItemSystemCatalogItemSnapshot i001Catalog = authority.Current
                .itemSystemSnapshot.catalogItems.Single(value =>
                    value.itemId == "I001");
            HashSet<Vector2Int> full = new HashSet<Vector2Int>(
                Enumerable.Range(0, 65).Select(index => new Vector2Int(
                    index % 5,
                    index / 5)));
            object[] arguments =
            {
                "full-grid-probe",
                i001Catalog.ShapeCells,
                0,
                full,
                true,
                null
            };
            Require(!(bool)firstLegal.Invoke(null, arguments)
                    && arguments[5] == null,
                "full Tray rejects without partial placement");
        }

        private static C1FormalItemSessionAuthority VerifyEntitlement()
        {
            C1FormalItemSessionAuthority unavailable = NewAuthority("unavailable", 1);
            C1FormalItemSessionSnapshot beforeUnavailable = unavailable.Current;
            C1FormalItemSessionOperationResult unavailableResult = unavailable.Submit(
                Arrangement(unavailable,
                    "pre-entitlement-i002",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    "focused.unavailable.reward.instance",
                    new Vector2Int(3, 3), 0));
            RejectUnchanged(unavailableResult, beforeUnavailable,
                C1FormalItemSessionDiagnosticCodes.ItemUnavailable,
                "I002 before entitlement");

            C1FormalItemSessionAuthority authority = NewAuthority("entitled", 1);
            RewardFixture fixture = RewardFixture.Create(
                authority, "accepted", null);
            C1FormalCampaignLootEntitlementCommand command = fixture.Command(
                authority,
                "entitle-canonical-reward");
            C1FormalItemSessionSnapshot prior = authority.Current;
            C1FormalCampaignLootEntitlementAcceptanceResult result =
                authority.AcceptCanonicalCampaignLootEntitlement(command);
            Require(result.accepted && result.changed,
                "canonical direct loot must entitle once");
            Equal(3, authority.Current.roster.Count,
                "entitlement roster count");
            C1FormalItemRosterEntrySnapshot i002 = authority.Current.FindRosterEntry(
                fixture.ItemInstanceId);
            Require(i002 != null && i002.ordinaryInstance != null,
                "canonical reward must materialize one ordinary instance");
            Equal(fixture.SelectedBaseItemId,
                i002.baseItemId,
                "canonical reward keeps the selected base Item identity");
            Equal(fixture.ItemInstanceId,
                i002.itemInstanceId, "canonical reward instance identity");
            Require(!ReferenceEquals(prior, authority.Current),
                "accepted entitlement atomically replaces the snapshot");

            C1FormalItemSessionSnapshot after = authority.Current;
            C1FormalCampaignLootEntitlementAcceptanceResult duplicateSameCommand =
                authority.AcceptCanonicalCampaignLootEntitlement(command);
            Require(duplicateSameCommand.accepted && !duplicateSameCommand.changed,
                "exact duplicate command is an unchanged no-op");
            Equal(C1FormalItemSessionDiagnosticCodes.DuplicateAcceptedNoOp,
                duplicateSameCommand.diagnosticCode,
                "duplicate command diagnostic");
            Require(ReferenceEquals(after, authority.Current),
                "duplicate command keeps byte-identical snapshot");

            C1FormalCampaignLootEntitlementAcceptanceResult duplicateReward =
                authority.AcceptCanonicalCampaignLootEntitlement(fixture.Command(
                    authority,
                    "entitle-canonical-reward-repeat"));
            Require(duplicateReward.accepted && !duplicateReward.changed,
                "same canonical grant under a new command is idempotent");
            Require(ReferenceEquals(after, authority.Current),
                "repeated reward keeps exact snapshot object");

            RewardFixture conflicting = RewardFixture.Create(
                authority,
                "conflict",
                new string('f', 64));
            C1FormalItemSessionSnapshot beforeConflict = authority.Current;
            C1FormalCampaignLootEntitlementAcceptanceResult conflict =
                authority.AcceptCanonicalCampaignLootEntitlement(
                    conflicting.Command(
                    authority,
                    command.commandId));
            RejectCampaignLootUnchanged(
                conflict,
                beforeConflict,
                C1FormalItemSessionDiagnosticCodes.CommandConflict,
                "conflicting canonical reward command");
            return authority;
        }

        private static BattleEvidence VerifyRealLightingAndTransactions(
            C1FormalItemSessionAuthority authority)
        {
            string i002 = RewardedItemInstanceId(authority);
            C1FormalItemSessionOperationResult weakPlace = authority.Submit(
                Arrangement(authority, "place-i002-weak",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002, new Vector2Int(3, 3), 0));
            Require(weakPlace.accepted && weakPlace.changed,
                "legal weak I002 placement");
            ItemSystemPlacementSnapshot weakResolved = authority.Current
                .itemSystemSnapshot.FindPlacement(i002);
            Require(weakResolved != null && !weakResolved.isLit
                    && !weakResolved.isDirectLit,
                "weak I002 must be authoritatively unlit");
            C1FormalItemBattleInputSnapshot weakBattle =
                authority.CreateBattleInputSnapshot();
            C1FormalItemBattleItemRow weakI002 = weakBattle.itemRows
                .Single(row => row.itemInstanceId == i002);
            Equal(false, weakI002.isLit,
                "weak Battle row consumes real unlit fact");

            C1FormalItemSessionOperationResult targetMove = authority.Submit(
                Arrangement(authority, "move-i002-target",
                    C1FormalItemArrangementCommandKind.MoveOnBoard,
                    i002, new Vector2Int(0, 2), 0));
            Require(targetMove.accepted && targetMove.changed,
                "legal target I002 move");
            ItemSystemPlacementSnapshot targetResolved = authority.Current
                .itemSystemSnapshot.FindPlacement(i002);
            Require(targetResolved != null && targetResolved.isLit
                    && !targetResolved.isDirectLit && targetResolved.litDepth == 1,
                "target I002 must be real relay-lit through I001");
            Equal(CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                targetResolved.litByPlacementId,
                "target I002 relay source is stable I001 instance placement");
            C1FormalItemBattleInputSnapshot targetBattle =
                authority.CreateBattleInputSnapshot();
            Equal(true, targetBattle.itemRows.Single(row =>
                    row.itemInstanceId == i002).isLit,
                "target Battle row consumes real lit fact");
            Require(!string.Equals(weakBattle.canonicalSignature,
                    targetBattle.canonicalSignature, StringComparison.Ordinal),
                "weak and target Battle signatures must differ");

            C1FormalItemSessionOperationResult returnI002 = authority.Submit(
                Arrangement(authority, "return-i002",
                    C1FormalItemArrangementCommandKind.ReturnToTray,
                    i002, default, 0, false));
            Require(returnI002.accepted && returnI002.changed,
                "I002 return to tray");
            C1FormalItemBattleItemRow trayI002 = authority.CreateBattleInputSnapshot()
                .itemRows.Single(row => row.itemInstanceId == i002);
            Require(!trayI002.isPlaced && !trayI002.isLit.HasValue,
                "unplaced I002 lighting remains absent rather than fabricated false");

            C1FormalItemSessionOperationResult moveI001 = authority.Submit(
                Arrangement(authority, "move-i001",
                    C1FormalItemArrangementCommandKind.MoveOnBoard,
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                    new Vector2Int(0, 0), 0));
            Require(moveI001.accepted, "I001 move transaction");
            C1FormalItemSessionOperationResult returnI001 = authority.Submit(
                Arrangement(authority, "return-i001",
                    C1FormalItemArrangementCommandKind.ReturnToTray,
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                    default, 0, false));
            Require(returnI001.accepted, "I001 return transaction");
            Require(authority.Submit(Arrangement(authority, "place-i001",
                C1FormalItemArrangementCommandKind.PlaceFromTray,
                CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                new Vector2Int(0, 1), 0)).accepted,
                "I001 place transaction");

            Require(authority.Submit(Arrangement(authority, "move-i031",
                C1FormalItemArrangementCommandKind.MoveOnBoard,
                I031InventoryPlacementContract.SpecialIdentityId,
                new Vector2Int(1, 0), 0)).accepted,
                "I031 move transaction");
            Require(authority.Submit(Arrangement(authority, "return-i031",
                C1FormalItemArrangementCommandKind.ReturnToTray,
                I031InventoryPlacementContract.SpecialIdentityId,
                default, 0, false)).accepted,
                "I031 return transaction");
            Require(authority.Submit(Arrangement(authority, "place-i031",
                C1FormalItemArrangementCommandKind.PlaceFromTray,
                I031InventoryPlacementContract.SpecialIdentityId,
                new Vector2Int(1, 1), 0)).accepted,
                "I031 place transaction");
            return new BattleEvidence(weakBattle, targetBattle);
        }

        private static void VerifyFailClosedMatrix()
        {
            C1FormalItemSessionAuthority authority = NewAuthority("reject", 1);
            RewardFixture fixture = RewardFixture.Create(
                authority, "reject", null);
            Require(authority.AcceptCanonicalCampaignLootEntitlement(
                fixture.Command(authority, "entitle")).accepted,
                "rejection authority entitlement setup");
            string i002 = fixture.ItemInstanceId;

            RejectArrangement(authority, new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    "wrong-token", authority.Current.resetGeneration,
                    "wrong-token", i002, authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(new Vector2Int(3, 3), 0)),
                C1FormalItemSessionDiagnosticCodes.SessionTokenMismatch,
                "wrong token");
            RejectArrangement(authority, new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    authority.Current.sessionToken, 99,
                    "wrong-generation", i002, authority.Current.canonicalSignature,
                    new C1FormalItemPlacementCandidate(new Vector2Int(3, 3), 0)),
                C1FormalItemSessionDiagnosticCodes.ResetGenerationStale,
                "stale generation");
            RejectArrangement(authority, new C1FormalItemArrangementCommand(
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    authority.Current.sessionToken, authority.Current.resetGeneration,
                    "stale-signature", i002, new string('0', 64),
                    new C1FormalItemPlacementCandidate(new Vector2Int(3, 3), 0)),
                C1FormalItemSessionDiagnosticCodes.ExpectedSessionSignatureStale,
                "stale expected signature");
            RejectArrangement(authority, Arrangement(authority, "unknown",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    "unknown-instance", new Vector2Int(3, 3), 0),
                C1FormalItemSessionDiagnosticCodes.ItemInstanceUnknown,
                "unknown identity");
            RejectArrangement(authority, Arrangement(authority, "source-mismatch",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    I031InventoryPlacementContract.StablePlacementId,
                    new Vector2Int(3, 3), 0),
                C1FormalItemSessionDiagnosticCodes.ItemInstanceUnknown,
                "I031 source identity mismatch");
            RejectArrangement(authority, Arrangement(authority, "eye",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002, new Vector2Int(2, 2), 0),
                C1FormalItemSessionDiagnosticCodes.ItemSystemSnapshotInvalid,
                "eye coverage");
            RejectArrangement(authority, Arrangement(authority, "bounds",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002, new Vector2Int(4, 4), 0),
                C1FormalItemSessionDiagnosticCodes.ItemSystemSnapshotInvalid,
                "out of bounds");
            RejectArrangement(authority, Arrangement(authority, "rotation",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002, new Vector2Int(3, 3), 45),
                C1FormalItemSessionDiagnosticCodes.ItemSystemSnapshotInvalid,
                "unsupported rotation");
            RejectArrangement(authority, Arrangement(authority, "overlap",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002, new Vector2Int(0, 0), 0),
                C1FormalItemSessionDiagnosticCodes.ItemSystemSnapshotInvalid,
                "overlap");

            C1FormalItemArrangementCommand accepted = Arrangement(authority,
                "dedupe-command", C1FormalItemArrangementCommandKind.PlaceFromTray,
                i002, new Vector2Int(3, 3), 0);
            Require(authority.Submit(accepted).accepted,
                "dedupe setup accepted command");
            C1FormalItemSessionSnapshot afterAccepted = authority.Current;
            C1FormalItemSessionOperationResult duplicate = authority.Submit(accepted);
            Require(duplicate.accepted && !duplicate.changed
                    && ReferenceEquals(afterAccepted, authority.Current),
                "exact duplicate arrangement command no-op");
            C1FormalItemArrangementCommand conflict = new C1FormalItemArrangementCommand(
                C1FormalItemArrangementCommandKind.MoveOnBoard,
                accepted.sessionToken, accepted.resetGeneration,
                accepted.commandId, accepted.itemInstanceId,
                accepted.expectedPriorSessionCanonicalSignature,
                new C1FormalItemPlacementCandidate(new Vector2Int(0, 2), 0));
            RejectArrangement(authority, conflict,
                C1FormalItemSessionDiagnosticCodes.CommandConflict,
                "conflicting duplicate command id");
            RejectArrangement(authority, Arrangement(authority, "already-placed",
                    C1FormalItemArrangementCommandKind.PlaceFromTray,
                    i002, new Vector2Int(3, 3), 0),
                C1FormalItemSessionDiagnosticCodes.ItemAlreadyPlaced,
                "duplicate instance placement");
        }

        private static void VerifyResetAndDeterminism(BattleEvidence evidence)
        {
            C1FormalItemSessionAuthority authority = NewAuthority("reset", 1);
            RewardFixture fixture = RewardFixture.Create(
                authority, "reset", null);
            Require(authority.AcceptCanonicalCampaignLootEntitlement(
                fixture.Command(authority, "entitle")).accepted,
                "reset entitlement setup");
            Require(authority.Submit(Arrangement(authority, "place-i002",
                C1FormalItemArrangementCommandKind.PlaceFromTray,
                fixture.ItemInstanceId,
                new Vector2Int(0, 2), 0)).accepted,
                "reset placement setup");
            C1FormalItemResetCommand reset = new C1FormalItemResetCommand(
                authority.Current.sessionToken,
                authority.Current.resetGeneration,
                "reset-command",
                authority.Current.canonicalSignature);
            C1FormalItemSessionOperationResult resetResult = authority.Reset(reset);
            Require(resetResult.accepted && resetResult.changed,
                "reset succeeds atomically");
            Equal(2L, authority.Current.resetGeneration,
                "reset generation increments");
            Equal(2, authority.Current.roster.Count,
                "reset removes only temporary I002 entitlement");
            Require(authority.Current.FindRosterEntry(
                    fixture.ItemInstanceId) == null,
                "reset I002 absent");
            C1FormalItemSessionAuthority freshGenerationTwo =
                NewAuthority("reset", 2);
            Equal(freshGenerationTwo.Current.canonicalSignature,
                authority.Current.canonicalSignature,
                "reset restores exact deterministic initial state for generation");
            C1FormalItemSessionSnapshot resetSnapshot = authority.Current;
            C1FormalItemSessionOperationResult resetDuplicate = authority.Reset(reset);
            Require(resetDuplicate.accepted && !resetDuplicate.changed
                    && ReferenceEquals(resetSnapshot, authority.Current),
                "duplicate reset command is unchanged no-op");

            CultureInfo original = CultureInfo.CurrentCulture;
            CultureInfo originalUi = CultureInfo.CurrentUICulture;
            try
            {
                foreach (string cultureName in new[] { "en-US", "tr-TR", "zh-CN" })
                {
                    CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                    C1FormalItemSessionAuthority repeated = NewAuthority("entitled", 1);
                    RewardFixture cultureFixture = RewardFixture.Create(
                        repeated, "accepted", null);
                    Require(repeated.AcceptCanonicalCampaignLootEntitlement(
                        cultureFixture.Command(repeated, "entitle")).accepted,
                        "culture entitlement " + cultureName);
                    Require(repeated.Submit(Arrangement(repeated, "place-i002-weak",
                        C1FormalItemArrangementCommandKind.PlaceFromTray,
                        cultureFixture.ItemInstanceId,
                        new Vector2Int(3, 3), 0)).accepted,
                        "culture weak placement " + cultureName);
                    C1FormalItemBattleInputSnapshot weak =
                        repeated.CreateBattleInputSnapshot();
                    Require(repeated.Submit(Arrangement(repeated, "move-i002-target",
                        C1FormalItemArrangementCommandKind.MoveOnBoard,
                        cultureFixture.ItemInstanceId,
                        new Vector2Int(0, 2), 0)).accepted,
                        "culture target placement " + cultureName);
                    C1FormalItemBattleInputSnapshot target =
                        repeated.CreateBattleInputSnapshot();
                    Equal(evidence.weakBattle.canonicalSignature,
                        weak.canonicalSignature,
                        "culture weak signature " + cultureName);
                    Equal(evidence.targetBattle.canonicalSignature,
                        target.canonicalSignature,
                        "culture target signature " + cultureName);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
                CultureInfo.CurrentUICulture = originalUi;
            }
        }

        private static void VerifyImmutabilityAndIsolation()
        {
            Type[] immutableTypes =
            {
                typeof(C1FormalItemPlacementCandidate),
                typeof(C1FormalItemArrangementCommand),
                typeof(C1FormalItemResetCommand),
                typeof(C1FormalCampaignLootEntitlementCommand),
                typeof(C1FormalItemOrdinaryInstanceSnapshot),
                typeof(C1FormalItemRosterEntrySnapshot),
                typeof(C1FormalItemPlacementSnapshot),
                typeof(C1FormalItemTrayPlacementSnapshot),
                typeof(C1FormalItemTrayLayoutSnapshot),
                typeof(C1FormalItemBattleItemRow),
                typeof(C1FormalI031NianCapacityFact),
                typeof(C1FormalI031NianCapacityProjectionRequest),
                typeof(C1FormalI031NianCapacityProjectionResult),
                typeof(C1FormalItemBattleInputSnapshot),
                typeof(C1FormalItemSessionSnapshot)
            };
            foreach (Type type in immutableTypes)
            {
                foreach (PropertyInfo property in type.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance))
                {
                    Require(property.SetMethod == null || !property.SetMethod.IsPublic,
                        type.Name + "." + property.Name + " is immutable");
                }
            }

            string source = File.ReadAllText(Path.GetFullPath(AuthorityPath));
            foreach (string token in new[]
                     {
                         "ItemInstanceRollEngine",
                         "ItemInstanceProjectionQaFixture",
                         "PlayerPrefs",
                         "Resources.FindObjectsOfTypeAll",
                         "GameObject.Find",
                         "BuildSandbox",
                         "CoreLoopLab",
                         "TalismanBag.V02",
                         "DateTime.Now",
                         "Guid.NewGuid",
                         "System.Random"
                     })
            {
                Require(source.IndexOf(token, StringComparison.Ordinal) < 0,
                    "authority isolation token " + token);
            }

            foreach (FieldInfo field in typeof(C1FormalItemSessionAuthority)
                         .GetFields(BindingFlags.Static | BindingFlags.Public
                                    | BindingFlags.NonPublic))
            {
                Require(field.IsLiteral || field.IsInitOnly,
                    "authority has no static mutable state: " + field.Name);
            }
        }

        private static C1FormalItemSessionAuthority NewAuthority(
            string token,
            long generation)
        {
            C1FormalItemSessionCreationResult result =
                C1FormalItemSessionAuthority.Create(token, generation);
            Require(result.isSuccess,
                "authority creation " + token + ": " + result.diagnosticCode);
            return result.authority;
        }

        private static C1FormalItemArrangementCommand Arrangement(
            C1FormalItemSessionAuthority authority,
            string commandId,
            C1FormalItemArrangementCommandKind kind,
            string instanceId,
            Vector2Int cell,
            int rotation,
            bool includeCandidate = true)
        {
            return new C1FormalItemArrangementCommand(
                kind,
                authority.Current.sessionToken,
                authority.Current.resetGeneration,
                commandId,
                instanceId,
                authority.Current.canonicalSignature,
                includeCandidate
                    ? new C1FormalItemPlacementCandidate(cell, rotation)
                    : null);
        }

        private static void RejectArrangement(
            C1FormalItemSessionAuthority authority,
            C1FormalItemArrangementCommand command,
            string expectedCode,
            string label)
        {
            C1FormalItemSessionSnapshot before = authority.Current;
            C1FormalItemSessionOperationResult result = authority.Submit(command);
            RejectUnchanged(result, before, expectedCode, label);
        }

        private static void RejectUnchanged(
            C1FormalItemSessionOperationResult result,
            C1FormalItemSessionSnapshot before,
            string expectedCode,
            string label)
        {
            Require(result != null && !result.accepted && !result.changed,
                label + " rejects");
            Equal(expectedCode, result.diagnosticCode,
                label + " stable diagnostic");
            Require(ReferenceEquals(before, result.snapshot),
                label + " returns exact prior snapshot");
        }

        private static void RejectCampaignLootUnchanged(
            C1FormalCampaignLootEntitlementAcceptanceResult result,
            C1FormalItemSessionSnapshot before,
            string expectedCode,
            string label)
        {
            Require(result != null && !result.accepted && !result.changed,
                label + " rejects");
            Equal(expectedCode, result.diagnosticCode,
                label + " stable diagnostic");
            Require(ReferenceEquals(before, result.snapshot),
                label + " returns exact prior snapshot");
        }

        private static string RewardedItemInstanceId(
            C1FormalItemSessionAuthority authority)
        {
            return authority.Current.roster
                .Where(row => !row.isSpecialLightingSource
                    && !string.Equals(
                        row.itemInstanceId,
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                        StringComparison.Ordinal))
                .Select(row => row.itemInstanceId)
                .Single();
        }

        private static void Equal<T>(T expected, T actual, string label)
        {
            assertionCount++;
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(label + " expected <"
                    + (expected == null ? "null" : expected.ToString())
                    + "> but was <"
                    + (actual == null ? "null" : actual.ToString()) + ">.");
            }
        }

        private static void Require(bool condition, string label)
        {
            assertionCount++;
            if (!condition) throw new InvalidOperationException(label + ".");
        }

        private sealed class BattleEvidence
        {
            public BattleEvidence(
                C1FormalItemBattleInputSnapshot weakBattle,
                C1FormalItemBattleInputSnapshot targetBattle)
            {
                this.weakBattle = weakBattle;
                this.targetBattle = targetBattle;
            }

            public readonly C1FormalItemBattleInputSnapshot weakBattle;
            public readonly C1FormalItemBattleInputSnapshot targetBattle;
        }

        private sealed class RewardFixture
        {
            private readonly C1CampaignDirectLootGrantBundle grant;

            private RewardFixture(C1CampaignDirectLootGrantBundle grant)
            {
                this.grant = grant;
            }

            public string ItemInstanceId => grant.generatedItem.itemInstanceId;
            public string SelectedBaseItemId => grant.selectedBaseItemId;

            public C1FormalCampaignLootEntitlementCommand Command(
                C1FormalItemSessionAuthority authority,
                string commandId)
            {
                return new C1FormalCampaignLootEntitlementCommand(
                    authority.Current.sessionToken,
                    authority.Current.resetGeneration,
                    commandId,
                    authority.Current.canonicalSignature,
                    grant);
            }

            public static RewardFixture Create(
                C1FormalItemSessionAuthority authority,
                string suffix,
                string overrideInstanceSignature)
            {
                long seed = string.IsNullOrEmpty(overrideInstanceSignature)
                    ? 1001L
                    : 2001L;
                C1CampaignDirectLootSessionCreateResult session =
                    C1CampaignDirectLootSessionResolver.CreateSession(
                        authority.Current.sessionToken,
                        seed);
                Require(session?.accepted == true && session.snapshot != null,
                    session?.diagnosticCode ?? "fixture direct loot session");
                StageClearFact stageClear = new StageClearFact(
                    "focused.stage-clear." + suffix,
                    "focused.battle-result." + suffix,
                    "focused.battle-request." + suffix,
                    "focused.fingerprint." + suffix,
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
                    resolved?.diagnosticCode ?? "fixture canonical direct loot");
                return new RewardFixture(resolved.grant);
            }
        }
    }
}
