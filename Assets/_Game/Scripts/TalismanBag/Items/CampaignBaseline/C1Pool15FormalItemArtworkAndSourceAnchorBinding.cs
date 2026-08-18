using System;
using System.Globalization;
using System.Linq;
using TalismanBag.Presentation.Items;
using UnityEngine;

namespace TalismanBag.Items.CampaignBaseline
{
    public sealed class C1Pool15FormalItemSourceAnchorRequest
    {
        public const string SchemaId =
            "C1Pool15FormalItemSourceAnchorRequest.v2";

        public C1Pool15FormalItemSourceAnchorRequest(
            string itemInstanceId,
            string expectedProductContext,
            string expectedSessionToken,
            long expectedResetGeneration,
            string expectedSessionCanonicalSignature,
            string expectedArrangementCanonicalSignature,
            string expectedItemSystemCanonicalSignature,
            string expectedInstanceCanonicalSignature,
            string expectedArtworkIdentity,
            string expectedEffectFamilyKey,
            string expectedPresentationStyleKey,
            string expectedCueIdentity,
            string expectedCatalogCanonicalSignature,
            string expectedCatalogRowCanonicalSignature)
        {
            schemaId = SchemaId;
            this.itemInstanceId = Normalize(itemInstanceId);
            this.expectedProductContext = Normalize(expectedProductContext);
            this.expectedSessionToken = Normalize(expectedSessionToken);
            this.expectedResetGeneration = expectedResetGeneration;
            this.expectedSessionCanonicalSignature = Normalize(
                expectedSessionCanonicalSignature);
            this.expectedArrangementCanonicalSignature = Normalize(
                expectedArrangementCanonicalSignature);
            this.expectedItemSystemCanonicalSignature = Normalize(
                expectedItemSystemCanonicalSignature);
            this.expectedInstanceCanonicalSignature =
                expectedInstanceCanonicalSignature ?? string.Empty;
            this.expectedArtworkIdentity = Normalize(expectedArtworkIdentity);
            this.expectedEffectFamilyKey = Normalize(expectedEffectFamilyKey);
            this.expectedPresentationStyleKey = Normalize(
                expectedPresentationStyleKey);
            this.expectedCueIdentity = Normalize(expectedCueIdentity);
            this.expectedCatalogCanonicalSignature = Normalize(
                expectedCatalogCanonicalSignature);
            this.expectedCatalogRowCanonicalSignature = Normalize(
                expectedCatalogRowCanonicalSignature);
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("|", new[]
            {
                schemaId,
                this.itemInstanceId,
                this.expectedProductContext,
                this.expectedSessionToken,
                expectedResetGeneration.ToString(CultureInfo.InvariantCulture),
                this.expectedSessionCanonicalSignature,
                this.expectedArrangementCanonicalSignature,
                this.expectedItemSystemCanonicalSignature,
                this.expectedInstanceCanonicalSignature,
                this.expectedArtworkIdentity,
                this.expectedEffectFamilyKey,
                this.expectedPresentationStyleKey,
                this.expectedCueIdentity,
                this.expectedCatalogCanonicalSignature,
                this.expectedCatalogRowCanonicalSignature
            }));
        }

        public string schemaId { get; }
        public string itemInstanceId { get; }
        public string expectedProductContext { get; }
        public string expectedSessionToken { get; }
        public long expectedResetGeneration { get; }
        public string expectedSessionCanonicalSignature { get; }
        public string expectedArrangementCanonicalSignature { get; }
        public string expectedItemSystemCanonicalSignature { get; }
        public string expectedInstanceCanonicalSignature { get; }
        public string expectedArtworkIdentity { get; }
        public string expectedEffectFamilyKey { get; }
        public string expectedPresentationStyleKey { get; }
        public string expectedCueIdentity { get; }
        public string expectedCatalogCanonicalSignature { get; }
        public string expectedCatalogRowCanonicalSignature { get; }
        public string canonicalSignature { get; }

        public bool IsStructurallyValid =>
            string.Equals(schemaId, SchemaId, StringComparison.Ordinal)
            && itemInstanceId.Length > 0
            && expectedProductContext.Length > 0
            && expectedSessionToken.Length > 0
            && expectedResetGeneration >= 0
            && expectedSessionCanonicalSignature.Length > 0
            && expectedArrangementCanonicalSignature.Length > 0
            && expectedItemSystemCanonicalSignature.Length > 0
            && expectedInstanceCanonicalSignature.Length > 0
            && expectedArtworkIdentity.Length > 0
            && expectedEffectFamilyKey.Length > 0
            && expectedPresentationStyleKey.Length > 0
            && expectedCueIdentity.Length > 0
            && expectedCatalogCanonicalSignature.Length > 0
            && expectedCatalogRowCanonicalSignature.Length > 0
            && canonicalSignature.Length > 0;

        public bool MatchesSnapshot(C1FormalItemSessionSnapshot snapshot)
        {
            return FirstSnapshotRejectCondition(snapshot).Length == 0;
        }

        public string FirstSnapshotRejectCondition(
            C1FormalItemSessionSnapshot snapshot)
        {
            if (!IsStructurallyValid) return "REQUEST_STRUCTURALLY_INVALID";
            if (snapshot == null) return "SNAPSHOT_MISSING";
            C1FormalItemRosterEntrySnapshot[] matches = snapshot.roster
                .Where(value => value != null && string.Equals(
                    value.itemInstanceId,
                    itemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            if (matches.Length != 1)
                return "SNAPSHOT_ROSTER_MATCH_COUNT_"
                       + matches.Length.ToString(CultureInfo.InvariantCulture);
            if (!string.Equals(expectedProductContext,
                    snapshot.productContext,
                    StringComparison.Ordinal))
                return "SNAPSHOT_PRODUCT_CONTEXT_MISMATCH";
            if (!string.Equals(expectedSessionToken,
                    snapshot.sessionToken,
                    StringComparison.Ordinal))
                return "SNAPSHOT_SESSION_TOKEN_MISMATCH";
            if (expectedResetGeneration != snapshot.resetGeneration)
                return "SNAPSHOT_RESET_GENERATION_MISMATCH";
            if (!string.Equals(expectedSessionCanonicalSignature,
                    snapshot.canonicalSignature,
                    StringComparison.Ordinal))
                return "SNAPSHOT_SESSION_IDENTITY_MISMATCH";
            if (!string.Equals(expectedArrangementCanonicalSignature,
                    snapshot.arrangementCanonicalSignature,
                    StringComparison.Ordinal))
                return "SNAPSHOT_ARRANGEMENT_IDENTITY_MISMATCH";
            if (!string.Equals(expectedItemSystemCanonicalSignature,
                    snapshot.itemSystemCanonicalSignature,
                    StringComparison.Ordinal))
                return "SNAPSHOT_ITEM_SYSTEM_IDENTITY_MISMATCH";
            if (!string.Equals(expectedInstanceCanonicalSignature,
                    matches[0].instanceCanonicalSignature,
                    StringComparison.Ordinal))
                return "SNAPSHOT_INSTANCE_IDENTITY_MISMATCH";
            return string.Empty;
        }

        public bool MatchesCatalog(
            C1FormalItemPresentationCatalogSnapshot catalog,
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemArtworkLightingState lightingState)
        {
            return IsStructurallyValid
                   && catalog != null
                   && roster != null
                   && string.Equals(itemInstanceId,
                       roster.itemInstanceId,
                       StringComparison.Ordinal)
                   && string.Equals(expectedCatalogCanonicalSignature,
                       catalog.canonicalSignature,
                       StringComparison.Ordinal)
                   && catalog.TryResolveArtwork(
                       roster.baseItemId,
                       roster.rarityKey,
                       lightingState,
                       out C1FormalItemPresentationRowSnapshot row,
                       out _,
                       out string artworkIdentity)
                   && string.Equals(expectedArtworkIdentity,
                       artworkIdentity,
                       StringComparison.Ordinal)
                   && string.Equals(expectedEffectFamilyKey,
                       row.effectFamilyKey,
                       StringComparison.Ordinal)
                   && string.Equals(expectedPresentationStyleKey,
                       row.presentationStyleKey,
                       StringComparison.Ordinal)
                   && string.Equals(expectedCueIdentity,
                       row.cueIdentity,
                       StringComparison.Ordinal)
                   && string.Equals(expectedCatalogRowCanonicalSignature,
                       row.rowCanonicalSignature,
                       StringComparison.Ordinal);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class C1Pool15FormalItemSourceAnchorBinding
    {
        public const string SchemaId =
            "C1Pool15FormalItemSourceAnchorBinding.v3";

        internal C1Pool15FormalItemSourceAnchorBinding(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemRosterEntrySnapshot roster,
            C1FormalItemPlacementSnapshot placement,
            ItemSystemPlacementSnapshot itemSystemPlacement,
            RectTransform sourceAnchor,
            Sprite artwork,
            string artworkIdentity,
            C1FormalItemPresentationCatalogSnapshot catalog,
            C1FormalItemPresentationRowSnapshot catalogRow)
        {
            schemaId = SchemaId;
            productContext = snapshot.productContext;
            sessionToken = snapshot.sessionToken;
            resetGeneration = snapshot.resetGeneration;
            sessionCanonicalSignature = snapshot.canonicalSignature;
            arrangementCanonicalSignature =
                snapshot.arrangementCanonicalSignature;
            itemSystemCanonicalSignature = snapshot.itemSystemCanonicalSignature;
            itemInstanceId = roster.itemInstanceId;
            baseItemId = roster.baseItemId;
            rarityKey = roster.rarityKey;
            instanceCanonicalSignature = roster.instanceCanonicalSignature;
            placementId = placement.placementId;
            anchorCell = placement.anchorCell;
            rotation = placement.rotation;
            isLit = itemSystemPlacement.isLit;
            this.sourceAnchor = sourceAnchor;
            this.artwork = artwork;
            this.artworkIdentity = artworkIdentity ?? string.Empty;
            effectFamilyKey = catalogRow.effectFamilyKey;
            presentationStyleKey = catalogRow.presentationStyleKey;
            cueIdentity = catalogRow.cueIdentity;
            catalogCanonicalSignature = catalog.canonicalSignature;
            catalogRowCanonicalSignature = catalogRow.rowCanonicalSignature;
            canonicalSignature = C1FormalItemCanonical.Hash(string.Join("|", new[]
            {
                schemaId,
                productContext,
                sessionToken,
                resetGeneration.ToString(CultureInfo.InvariantCulture),
                sessionCanonicalSignature,
                arrangementCanonicalSignature,
                itemSystemCanonicalSignature,
                itemInstanceId,
                baseItemId,
                rarityKey,
                instanceCanonicalSignature,
                placementId,
                C1FormalItemCanonical.Cell(anchorCell),
                rotation.ToString(CultureInfo.InvariantCulture),
                isLit ? "lit" : "unlit",
                this.artworkIdentity,
                effectFamilyKey,
                presentationStyleKey,
                cueIdentity,
                catalogCanonicalSignature,
                catalogRowCanonicalSignature
            }));
        }

        public string schemaId { get; }
        public string productContext { get; }
        public string sessionToken { get; }
        public long resetGeneration { get; }
        public string sessionCanonicalSignature { get; }
        public string arrangementCanonicalSignature { get; }
        public string itemSystemCanonicalSignature { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string rarityKey { get; }
        public string instanceCanonicalSignature { get; }
        public string placementId { get; }
        public Vector2Int anchorCell { get; }
        public int rotation { get; }
        public bool isLit { get; }
        public RectTransform sourceAnchor { get; }
        private ItemRarityContourBloomVfx currentPresentationCarrier;
        public ItemRarityContourBloomVfx presentationCarrier =>
            currentPresentationCarrier != null
            && currentPresentationCarrier.HasAppliedPresentation
                ? currentPresentationCarrier
                : null;
        public Sprite artwork { get; }
        public string artworkIdentity { get; }
        public string effectFamilyKey { get; }
        public string presentationStyleKey { get; }
        public string cueIdentity { get; }
        public string catalogCanonicalSignature { get; }
        public string catalogRowCanonicalSignature { get; }
        public string canonicalSignature { get; }

        internal bool BindPresentationCarrier(
            ItemRarityContourBloomVfx carrier)
        {
            if (carrier == null || !carrier.HasAppliedPresentation)
                return false;
            currentPresentationCarrier = carrier;
            return true;
        }

        public bool Matches(C1Pool15FormalItemSourceAnchorRequest request)
        {
            return request != null
                   && request.IsStructurallyValid
                   && string.Equals(itemInstanceId, request.itemInstanceId,
                       StringComparison.Ordinal)
                   && string.Equals(productContext,
                       request.expectedProductContext,
                       StringComparison.Ordinal)
                   && string.Equals(sessionToken,
                       request.expectedSessionToken,
                       StringComparison.Ordinal)
                   && resetGeneration == request.expectedResetGeneration
                   && string.Equals(sessionCanonicalSignature,
                       request.expectedSessionCanonicalSignature,
                       StringComparison.Ordinal)
                   && string.Equals(arrangementCanonicalSignature,
                       request.expectedArrangementCanonicalSignature,
                       StringComparison.Ordinal)
                   && string.Equals(itemSystemCanonicalSignature,
                       request.expectedItemSystemCanonicalSignature,
                       StringComparison.Ordinal)
                   && string.Equals(instanceCanonicalSignature,
                       request.expectedInstanceCanonicalSignature,
                       StringComparison.Ordinal)
                   && string.Equals(artworkIdentity,
                       request.expectedArtworkIdentity,
                       StringComparison.Ordinal)
                   && string.Equals(effectFamilyKey,
                       request.expectedEffectFamilyKey,
                       StringComparison.Ordinal)
                   && string.Equals(presentationStyleKey,
                       request.expectedPresentationStyleKey,
                       StringComparison.Ordinal)
                   && string.Equals(cueIdentity,
                       request.expectedCueIdentity,
                       StringComparison.Ordinal)
                   && string.Equals(catalogCanonicalSignature,
                       request.expectedCatalogCanonicalSignature,
                       StringComparison.Ordinal)
                   && string.Equals(catalogRowCanonicalSignature,
                       request.expectedCatalogRowCanonicalSignature,
                       StringComparison.Ordinal);
        }
    }

    public static class C1Pool15FormalItemArtworkAndSourceAnchorBinding
    {
        public static bool TryResolveArtwork(
            C1FormalItemPresentationCatalogSnapshot catalog,
            string authoritativeBaseItemId,
            string authoritativeRarityKey,
            C1FormalItemArtworkLightingState lightingState,
            out C1FormalItemPresentationRowSnapshot row,
            out Sprite artwork,
            out string artworkIdentity)
        {
            row = null;
            artwork = null;
            artworkIdentity = string.Empty;
            return catalog != null
                   && catalog.TryResolveArtwork(
                       authoritativeBaseItemId,
                       authoritativeRarityKey,
                       lightingState,
                       out row,
                       out artwork,
                       out artworkIdentity);
        }

        public static bool TryCreateCurrentSourceAnchorRequest(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemPresentationCatalogSnapshot catalog,
            string itemInstanceId,
            out C1Pool15FormalItemSourceAnchorRequest request)
        {
            return TryCreateCurrentSourceAnchorRequest(
                snapshot,
                catalog,
                itemInstanceId,
                out request,
                out _);
        }

        public static bool TryCreateCurrentSourceAnchorRequest(
            C1FormalItemSessionSnapshot snapshot,
            C1FormalItemPresentationCatalogSnapshot catalog,
            string itemInstanceId,
            out C1Pool15FormalItemSourceAnchorRequest request,
            out string firstRejectCondition)
        {
            request = null;
            if (snapshot == null)
            {
                firstRejectCondition = "SNAPSHOT_MISSING";
                return false;
            }
            if (catalog == null)
            {
                firstRejectCondition = "CATALOG_MISSING";
                return false;
            }
            if (string.IsNullOrWhiteSpace(itemInstanceId))
            {
                firstRejectCondition = "ITEM_INSTANCE_ID_MISSING";
                return false;
            }
            string exactItemInstanceId = itemInstanceId.Trim();
            C1FormalItemRosterEntrySnapshot[] rosters = snapshot.roster
                .Where(value => value != null && string.Equals(
                    value.itemInstanceId,
                    exactItemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            if (rosters.Length != 1)
            {
                firstRejectCondition = "ROSTER_MATCH_COUNT_"
                                       + rosters.Length.ToString(
                                           CultureInfo.InvariantCulture);
                return false;
            }
            if (rosters[0].isSpecialLightingSource)
            {
                firstRejectCondition = "SPECIAL_LIGHTING_SOURCE";
                return false;
            }
            C1FormalItemRosterEntrySnapshot roster = rosters[0];
            if (!catalog.TryResolveExact(
                    roster.baseItemId,
                    roster.rarityKey,
                    out C1FormalItemPresentationRowSnapshot row,
                    out string catalogDiagnostic))
            {
                firstRejectCondition = catalogDiagnostic;
                return false;
            }
            if (!row.TryResolveArtwork(
                    C1FormalItemArtworkLightingState.Lit,
                    out _,
                    out string artworkIdentity))
            {
                firstRejectCondition = "LIT_ARTWORK_UNAVAILABLE";
                return false;
            }
            if (row.isSpecialLightingSource)
            {
                firstRejectCondition = "CATALOG_ROW_SPECIAL";
                return false;
            }

            request = new C1Pool15FormalItemSourceAnchorRequest(
                exactItemInstanceId,
                snapshot.productContext,
                snapshot.sessionToken,
                snapshot.resetGeneration,
                snapshot.canonicalSignature,
                snapshot.arrangementCanonicalSignature,
                snapshot.itemSystemCanonicalSignature,
                roster.instanceCanonicalSignature,
                artworkIdentity,
                row.effectFamilyKey,
                row.presentationStyleKey,
                row.cueIdentity,
                catalog.canonicalSignature,
                row.rowCanonicalSignature);
            firstRejectCondition = FirstStructuralRejectCondition(request);
            if (firstRejectCondition.Length > 0) return false;
            firstRejectCondition = request.FirstSnapshotRejectCondition(snapshot);
            return firstRejectCondition.Length == 0;
        }

        private static string FirstStructuralRejectCondition(
            C1Pool15FormalItemSourceAnchorRequest request)
        {
            if (request == null) return "REQUEST_MISSING";
            if (request.expectedProductContext.Length == 0)
                return "PRODUCT_CONTEXT_MISSING";
            if (request.expectedSessionToken.Length == 0)
                return "SESSION_TOKEN_MISSING";
            if (request.expectedSessionCanonicalSignature.Length == 0)
                return "SESSION_SIGNATURE_MISSING";
            if (request.expectedArrangementCanonicalSignature.Length == 0)
                return "ARRANGEMENT_SIGNATURE_MISSING";
            if (request.expectedItemSystemCanonicalSignature.Length == 0)
                return "ITEM_SYSTEM_SIGNATURE_MISSING";
            if (request.expectedInstanceCanonicalSignature.Length == 0)
                return "INSTANCE_SIGNATURE_MISSING";
            if (request.expectedArtworkIdentity.Length == 0)
                return "ARTWORK_IDENTITY_MISSING";
            if (request.expectedEffectFamilyKey.Length == 0)
                return "EFFECT_FAMILY_MISSING";
            if (request.expectedPresentationStyleKey.Length == 0)
                return "PRESENTATION_STYLE_MISSING";
            if (request.expectedCueIdentity.Length == 0)
                return "CUE_IDENTITY_MISSING";
            if (request.expectedCatalogCanonicalSignature.Length == 0)
                return "CATALOG_SIGNATURE_MISSING";
            if (request.expectedCatalogRowCanonicalSignature.Length == 0)
                return "CATALOG_ROW_SIGNATURE_MISSING";
            if (request.canonicalSignature.Length == 0)
                return "REQUEST_SIGNATURE_MISSING";
            return request.IsStructurallyValid
                ? string.Empty
                : "REQUEST_STRUCTURALLY_INVALID";
        }

        internal static bool IsPlacedLitSourceCandidate(
            int rosterMatchCount,
            int formalPlacementMatchCount,
            int itemSystemPlacementMatchCount,
            int trayPlacementMatchCount,
            int activeTrayPlacementMatchCount,
            int liveAnchorMatchCount,
            bool exactCatalogOrdinaryIdentity,
            bool exactIdentityAligned,
            bool requestCatalogAligned,
            bool isLit)
        {
            return rosterMatchCount == 1
                   && formalPlacementMatchCount == 1
                   && itemSystemPlacementMatchCount == 1
                   && trayPlacementMatchCount <= 1
                   && activeTrayPlacementMatchCount == 0
                   && liveAnchorMatchCount == 1
                   && exactCatalogOrdinaryIdentity
                   && exactIdentityAligned
                   && requestCatalogAligned
                   && isLit;
        }
    }
}
