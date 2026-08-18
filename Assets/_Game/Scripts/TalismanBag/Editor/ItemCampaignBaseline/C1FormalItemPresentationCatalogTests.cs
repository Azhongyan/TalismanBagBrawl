using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using TalismanBag.Items;
using TalismanBag.Items.CampaignBaseline;
using UnityEngine;

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public static class C1FormalItemPresentationCatalogTests
    {
        public const string TerminalMarker =
            "C1_FORMAL_ITEM_PRESENTATION_CATALOG_TESTS_PASS";

        private static int assertionCount;

        public static int RunFocused()
        {
            try
            {
                RunAllOrThrow();
                Console.WriteLine(TerminalMarker + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_FORMAL_ITEM_PRESENTATION_CATALOG_TESTS_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        public static void RunAllOrThrow()
        {
            assertionCount = 0;
            VerifyPresentationRowsAndExactLookup();
            VerifyRejectionMatrix();
            VerifyLegacySignatureArgumentsDoNotOwnPresentation();
            VerifyImmutableSnapshot();
            VerifySyntheticFutureItemUsesPresentationDataOnly();
        }

        private static void VerifyPresentationRowsAndExactLookup()
        {
            C1FormalItemPresentationCatalogRowDefinition ordinary =
                OrdinaryRow("I003", "white", "ENEMY_CONTROL");
            C1FormalItemPresentationCatalogRowDefinition special = SpecialRow();
            C1FormalItemPresentationCatalogResult result = Resolve(
                ordinary,
                special);

            Require(result.accepted && result.snapshot.Rows.Count == 2,
                "PRESENTATION_CATALOG_REJECTED " + result.diagnosticCode);
            Require(result.snapshot.TryResolveExact(
                        "I003",
                        "white",
                        out C1FormalItemPresentationRowSnapshot exact,
                        out string diagnostic)
                    && string.Equals(
                        diagnostic,
                        C1FormalItemPresentationCatalogDiagnostics.None,
                        StringComparison.Ordinal)
                    && string.Equals(
                        exact.effectFamilyKey,
                        "ENEMY_CONTROL",
                        StringComparison.Ordinal),
                "EXACT_PRESENTATION_LOOKUP_FAILED");
            Require(!result.snapshot.TryResolveExact(
                        "I003",
                        "green",
                        out _,
                        out diagnostic)
                    && string.Equals(
                        diagnostic,
                        C1FormalItemPresentationCatalogDiagnostics
                            .LookupRarityMismatch,
                        StringComparison.Ordinal),
                "RARITY_MISMATCH_MUST_FAIL_CLOSED");
            Require(!result.snapshot.TryResolveExact(
                        "UNKNOWN",
                        "white",
                        out _,
                        out diagnostic)
                    && string.Equals(
                        diagnostic,
                        C1FormalItemPresentationCatalogDiagnostics.LookupUnknown,
                        StringComparison.Ordinal),
                "UNKNOWN_LOOKUP_MUST_FAIL_CLOSED");
            Require(result.snapshot.TryResolveArtwork(
                        I031InventoryPlacementContract.ItemId,
                        string.Empty,
                        C1FormalItemArtworkLightingState.Unlit,
                        out C1FormalItemPresentationRowSnapshot specialSnapshot,
                        out Sprite unlit,
                        out string unlitIdentity)
                    && result.snapshot.TryResolveArtwork(
                        I031InventoryPlacementContract.ItemId,
                        string.Empty,
                        C1FormalItemArtworkLightingState.Lit,
                        out _,
                        out Sprite lit,
                        out string litIdentity)
                    && specialSnapshot.isSpecialLightingSource
                    && !ReferenceEquals(unlit, lit)
                    && !string.Equals(
                        unlitIdentity,
                        litIdentity,
                        StringComparison.Ordinal),
                "SPECIAL_LIGHTING_VARIANTS_NOT_EXACT");
        }

        private static void VerifyRejectionMatrix()
        {
            Equal(C1FormalItemPresentationCatalogDiagnostics.RowsNull,
                C1FormalItemPresentationCatalog.TryCreateSnapshot(
                    string.Empty,
                    null,
                    string.Empty).diagnosticCode,
                "null rows");
            Equal(C1FormalItemPresentationCatalogDiagnostics.RowNull,
                ResolveRaw(new C1FormalItemPresentationCatalogRowDefinition[]
                {
                    null
                }).diagnosticCode,
                "null row");

            C1FormalItemPresentationCatalogRowDefinition valid =
                OrdinaryRow("I003", "white", "ENEMY_CONTROL");
            Equal(C1FormalItemPresentationCatalogDiagnostics.IdentityDuplicate,
                ResolveRaw(new[] { valid, valid }).diagnosticCode,
                "duplicate exact identity");
            AssertRejected(CreateOrdinary(
                    "I003", "white", null, "i003.art", "ENEMY_CONTROL",
                    "presentation.i003", "cue.i003"),
                C1FormalItemPresentationCatalogDiagnostics.ArtworkMissing,
                "missing artwork");
            AssertRejected(CreateOrdinary(
                    "I003", "white", FakeSprite(), string.Empty,
                    "ENEMY_CONTROL", "presentation.i003", "cue.i003"),
                C1FormalItemPresentationCatalogDiagnostics
                    .ArtworkIdentityMissing,
                "missing artwork identity");
            AssertRejected(CreateOrdinary(
                    "I003", "white", FakeSprite(), "i003.art", string.Empty,
                    "presentation.i003", "cue.i003"),
                C1FormalItemPresentationCatalogDiagnostics.FamilyMissing,
                "missing family");
            AssertRejected(CreateOrdinary(
                    "I003", "white", FakeSprite(), "i003.art",
                    "ENEMY_CONTROL", string.Empty, "cue.i003"),
                C1FormalItemPresentationCatalogDiagnostics.StyleMissing,
                "missing style");
            AssertRejected(CreateOrdinary(
                    "I003", "white", FakeSprite(), "i003.art",
                    "ENEMY_CONTROL", "presentation.i003", string.Empty),
                C1FormalItemPresentationCatalogDiagnostics.CueMissing,
                "missing cue");

            C1FormalItemPresentationCatalogRowDefinition invalidSpecial =
                C1FormalItemPresentationCatalogRowDefinition.CreateSigned(
                    I031InventoryPlacementContract.ItemId,
                    string.Empty,
                    FakeSprite(),
                    "special.art",
                    I031InventoryPlacementContract.SpecialIdentityId,
                    I031InventoryPlacementContract.SpecialIdentityId,
                    I031InventoryPlacementContract.SpecialIdentityId,
                    string.Empty,
                    true,
                    FakeSprite(),
                    "same",
                    FakeSprite(),
                    "same");
            AssertRejected(invalidSpecial,
                C1FormalItemPresentationCatalogDiagnostics.SpecialVariantInvalid,
                "special variant identity collision");
        }

        private static void VerifyLegacySignatureArgumentsDoNotOwnPresentation()
        {
            C1FormalItemPresentationCatalogRowDefinition legacy =
                OrdinaryRow("I003", "white", "ENEMY_CONTROL");
            C1FormalItemPresentationCatalogResult result =
                C1FormalItemPresentationCatalog.TryCreateSnapshot(
                    "legacy.catalog.profile",
                    new[] { legacy },
                    "legacy.catalog.declaration");

            Require(result.accepted,
                "LEGACY_SIGNATURE_ARGUMENT_STILL_BLOCKS_PRESENTATION "
                + result.diagnosticCode);
            Require(string.Equals(
                    result.snapshot.Rows[0].rowCanonicalSignature,
                    C1FormalItemPresentationCatalogRowDefinition
                        .ComputeCanonicalSignature(legacy),
                    StringComparison.Ordinal),
                "RUNTIME_PRESENTATION_ROW_IDENTITY_USED_LEGACY_DECLARATION");
        }

        private static void VerifyImmutableSnapshot()
        {
            C1FormalItemPresentationCatalogRowDefinition original =
                OrdinaryRow("I003", "white", "ENEMY_CONTROL");
            C1FormalItemPresentationCatalogRowDefinition[] source = { original };
            C1FormalItemPresentationCatalogResult result = Resolve(source);
            Require(result.accepted, "IMMUTABLE_FIXTURE_REJECTED");
            source[0] = OrdinaryRow("I004", "white", "TARGET_SPREAD");
            Require(result.snapshot.Rows.Count == 1
                    && string.Equals(
                        result.snapshot.Rows[0].baseItemId,
                        "I003",
                        StringComparison.Ordinal),
                "SNAPSHOT_RETAINED_MUTABLE_SOURCE_ARRAY");

            bool mutationRejected = false;
            try
            {
                ((IList<C1FormalItemPresentationRowSnapshot>)result.snapshot.Rows)
                    [0] = null;
            }
            catch (NotSupportedException)
            {
                mutationRejected = true;
            }
            Require(mutationRejected, "SNAPSHOT_ROWS_ARE_MUTABLE");

            C1FormalItemPresentationCatalogResult reordered = Resolve(
                OrdinaryRow("I004", "white", "TARGET_SPREAD"),
                OrdinaryRow("I003", "white", "ENEMY_CONTROL"));
            C1FormalItemPresentationCatalogResult ordered = Resolve(
                OrdinaryRow("I003", "white", "ENEMY_CONTROL"),
                OrdinaryRow("I004", "white", "TARGET_SPREAD"));
            Require(reordered.accepted
                    && ordered.accepted
                    && string.Equals(
                        reordered.snapshot.canonicalSignature,
                        ordered.snapshot.canonicalSignature,
                        StringComparison.Ordinal),
                "CATALOG_IDENTITY_DEPENDS_ON_ENUMERATION_ORDER");
        }

        private static void VerifySyntheticFutureItemUsesPresentationDataOnly()
        {
            Sprite artwork = FakeSprite();
            C1FormalItemPresentationCatalogRowDefinition future = CreateOrdinary(
                "I900",
                "white",
                artwork,
                "future.i900.art.v1",
                "DIRECT_TEMPO",
                "presentation.future.i900.v1",
                "cue.future.i900.v1");
            C1FormalItemPresentationCatalogResult result = Resolve(future);

            Require(result.accepted
                    && result.snapshot.TryResolveArtwork(
                        "I900",
                        "white",
                        C1FormalItemArtworkLightingState.PlacementAbsent,
                        out C1FormalItemPresentationRowSnapshot row,
                        out Sprite resolved,
                        out string identity)
                    && ReferenceEquals(artwork, resolved)
                    && string.Equals(
                        row.presentationStyleKey,
                        "presentation.future.i900.v1",
                        StringComparison.Ordinal)
                    && string.Equals(
                        identity,
                        "future.i900.art.v1",
                        StringComparison.Ordinal),
                "SYNTHETIC_FUTURE_ITEM_DID_NOT_RESOLVE_FROM_PRESENTATION_DATA");
        }

        private static C1FormalItemPresentationCatalogRowDefinition OrdinaryRow(
            string baseItemId,
            string rarityKey,
            string family)
        {
            return CreateOrdinary(
                baseItemId,
                rarityKey,
                FakeSprite(),
                baseItemId.ToLowerInvariant() + ".art",
                family,
                "presentation." + baseItemId.ToLowerInvariant(),
                "cue." + baseItemId.ToLowerInvariant());
        }

        private static C1FormalItemPresentationCatalogRowDefinition CreateOrdinary(
            string baseItemId,
            string rarityKey,
            Sprite artwork,
            string artworkIdentity,
            string family,
            string style,
            string cue)
        {
            return C1FormalItemPresentationCatalogRowDefinition.CreateSigned(
                baseItemId,
                rarityKey,
                artwork,
                artworkIdentity,
                family,
                style,
                cue,
                string.Empty);
        }

        private static C1FormalItemPresentationCatalogRowDefinition SpecialRow()
        {
            Sprite unlit = FakeSprite();
            Sprite lit = FakeSprite();
            return C1FormalItemPresentationCatalogRowDefinition.CreateSigned(
                I031InventoryPlacementContract.ItemId,
                string.Empty,
                unlit,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.SpecialIdentityId,
                string.Empty,
                true,
                unlit,
                "special.i031.unlit",
                lit,
                "special.i031.lit");
        }

        private static C1FormalItemPresentationCatalogResult Resolve(
            params C1FormalItemPresentationCatalogRowDefinition[] rows)
        {
            return ResolveRaw(rows);
        }

        private static C1FormalItemPresentationCatalogResult ResolveRaw(
            C1FormalItemPresentationCatalogRowDefinition[] rows)
        {
            return C1FormalItemPresentationCatalog.TryCreateSnapshot(
                string.Empty,
                rows,
                string.Empty);
        }

        private static void AssertRejected(
            C1FormalItemPresentationCatalogRowDefinition row,
            string expectedDiagnostic,
            string label)
        {
            Equal(expectedDiagnostic,
                ResolveRaw(new[] { row }).diagnosticCode,
                label);
        }

        private static Sprite FakeSprite()
        {
            return (Sprite)FormatterServices.GetUninitializedObject(
                typeof(Sprite));
        }

        private static void Require(bool condition, string message)
        {
            assertionCount++;
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Equal(string expected, string actual, string label)
        {
            Require(string.Equals(expected, actual, StringComparison.Ordinal),
                label + " expected=" + expected + " actual=" + actual);
        }
    }
}
