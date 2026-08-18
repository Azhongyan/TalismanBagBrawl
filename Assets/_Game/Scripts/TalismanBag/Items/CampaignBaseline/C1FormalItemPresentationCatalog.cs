using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using UnityEngine;

namespace TalismanBag.Items.CampaignBaseline
{
    public static class C1FormalItemPresentationCatalogDiagnostics
    {
        public const string None = "NONE";
        public const string CatalogNull = "C1_ITEM_PRESENTATION_CATALOG_NULL";
        public const string RowsNull = "C1_ITEM_PRESENTATION_ROWS_NULL";
        public const string RowNull = "C1_ITEM_PRESENTATION_ROW_NULL";
        public const string IdentityMissing =
            "C1_ITEM_PRESENTATION_IDENTITY_MISSING";
        public const string IdentityDuplicate =
            "C1_ITEM_PRESENTATION_IDENTITY_DUPLICATE";
        public const string ArtworkMissing =
            "C1_ITEM_PRESENTATION_ARTWORK_MISSING";
        public const string ArtworkIdentityMissing =
            "C1_ITEM_PRESENTATION_ARTWORK_IDENTITY_MISSING";
        public const string FamilyMissing =
            "C1_ITEM_PRESENTATION_EFFECT_FAMILY_MISSING";
        public const string StyleMissing =
            "C1_ITEM_PRESENTATION_STYLE_MISSING";
        public const string CueMissing = "C1_ITEM_PRESENTATION_CUE_MISSING";
        public const string SpecialVariantInvalid =
            "C1_ITEM_PRESENTATION_SPECIAL_VARIANT_INVALID";
        public const string EnumerationFailed =
            "C1_ITEM_PRESENTATION_ENUMERATION_FAILED";
        public const string LookupUnknown = "C1_ITEM_PRESENTATION_LOOKUP_UNKNOWN";
        public const string LookupRarityMismatch =
            "C1_ITEM_PRESENTATION_LOOKUP_RARITY_MISMATCH";
    }

    [Serializable]
    public sealed class C1FormalItemPresentationCatalogRowDefinition
    {
        [SerializeField] private string baseItemId = string.Empty;
        [SerializeField] private string rarityKey = string.Empty;
        [SerializeField] private Sprite artwork;
        [SerializeField] private string artworkIdentity = string.Empty;
        [SerializeField] private string effectFamilyKey = string.Empty;
        [SerializeField] private string presentationStyleKey = string.Empty;
        [SerializeField] private string cueIdentity = string.Empty;
        [SerializeField] private bool isSpecialLightingSource;
        [SerializeField] private Sprite unlitArtwork;
        [SerializeField] private string unlitArtworkIdentity = string.Empty;
        [SerializeField] private Sprite litArtwork;
        [SerializeField] private string litArtworkIdentity = string.Empty;

        public string BaseItemId => Normalize(baseItemId);
        public string RarityKey => Normalize(rarityKey);
        public Sprite Artwork => artwork;
        public string ArtworkIdentity => Normalize(artworkIdentity);
        public string EffectFamilyKey => Normalize(effectFamilyKey);
        public string PresentationStyleKey => Normalize(presentationStyleKey);
        public string CueIdentity => Normalize(cueIdentity);
        public bool IsSpecialLightingSource => isSpecialLightingSource;
        public Sprite UnlitArtwork => unlitArtwork;
        public string UnlitArtworkIdentity => Normalize(unlitArtworkIdentity);
        public Sprite LitArtwork => litArtwork;
        public string LitArtworkIdentity => Normalize(litArtworkIdentity);

        public static C1FormalItemPresentationCatalogRowDefinition CreateSigned(
            string baseItemId,
            string rarityKey,
            Sprite artwork,
            string artworkIdentity,
            string effectFamilyKey,
            string presentationStyleKey,
            string cueIdentity,
            string sourceProfileSignature,
            bool isSpecialLightingSource = false,
            Sprite unlitArtwork = null,
            string unlitArtworkIdentity = null,
            Sprite litArtwork = null,
            string litArtworkIdentity = null)
        {
            _ = sourceProfileSignature;
            C1FormalItemPresentationCatalogRowDefinition definition = new()
            {
                baseItemId = Normalize(baseItemId),
                rarityKey = Normalize(rarityKey),
                artwork = artwork,
                artworkIdentity = Normalize(artworkIdentity),
                effectFamilyKey = Normalize(effectFamilyKey),
                presentationStyleKey = Normalize(presentationStyleKey),
                cueIdentity = Normalize(cueIdentity),
                isSpecialLightingSource = isSpecialLightingSource,
                unlitArtwork = unlitArtwork,
                unlitArtworkIdentity = Normalize(unlitArtworkIdentity),
                litArtwork = litArtwork,
                litArtworkIdentity = Normalize(litArtworkIdentity)
            };
            return definition;
        }

        public static string ComputeCanonicalSignature(
            C1FormalItemPresentationCatalogRowDefinition definition)
        {
            if (definition == null) return string.Empty;
            return C1FormalItemPresentationRowSnapshot.SchemaId + ":"
                   + definition.BaseItemId + "@" + definition.RarityKey;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class C1FormalItemPresentationRowSnapshot
    {
        public const string SchemaId = "C1FormalItemPresentationRow.v1";
        public const int SchemaVersion = 1;

        internal C1FormalItemPresentationRowSnapshot(
            C1FormalItemPresentationCatalogRowDefinition source)
        {
            schemaId = SchemaId;
            schemaVersion = SchemaVersion;
            baseItemId = source.BaseItemId;
            rarityKey = source.RarityKey;
            rarityVersionIdentity = baseItemId + "@" + rarityKey;
            artwork = source.Artwork;
            artworkIdentity = source.ArtworkIdentity;
            effectFamilyKey = source.EffectFamilyKey;
            presentationStyleKey = source.PresentationStyleKey;
            cueIdentity = source.CueIdentity;
            isSpecialLightingSource = source.IsSpecialLightingSource;
            unlitArtwork = source.UnlitArtwork;
            unlitArtworkIdentity = source.UnlitArtworkIdentity;
            litArtwork = source.LitArtwork;
            litArtworkIdentity = source.LitArtworkIdentity;
            rowCanonicalSignature = C1FormalItemPresentationCatalogRowDefinition
                .ComputeCanonicalSignature(source);
        }

        public string schemaId { get; }
        public int schemaVersion { get; }
        public string baseItemId { get; }
        public string rarityKey { get; }
        public string rarityVersionIdentity { get; }
        public Sprite artwork { get; }
        public string artworkIdentity { get; }
        public string effectFamilyKey { get; }
        public string presentationStyleKey { get; }
        public string cueIdentity { get; }
        public bool isSpecialLightingSource { get; }
        public Sprite unlitArtwork { get; }
        public string unlitArtworkIdentity { get; }
        public Sprite litArtwork { get; }
        public string litArtworkIdentity { get; }
        public string rowCanonicalSignature { get; }

        public bool TryResolveArtwork(
            C1FormalItemArtworkLightingState lightingState,
            out Sprite resolvedArtwork,
            out string resolvedArtworkIdentity)
        {
            resolvedArtwork = null;
            resolvedArtworkIdentity = string.Empty;
            if (!Enum.IsDefined(typeof(C1FormalItemArtworkLightingState),
                    lightingState))
                return false;
            if (isSpecialLightingSource)
            {
                bool lit = lightingState == C1FormalItemArtworkLightingState.Lit;
                resolvedArtwork = lit ? litArtwork : unlitArtwork;
                resolvedArtworkIdentity = lit
                    ? litArtworkIdentity
                    : unlitArtworkIdentity;
            }
            else
            {
                resolvedArtwork = artwork;
                resolvedArtworkIdentity = artworkIdentity;
            }

            return !ReferenceEquals(resolvedArtwork, null)
                   && !string.IsNullOrWhiteSpace(resolvedArtworkIdentity);
        }
    }

    public sealed class C1FormalItemPresentationCatalogSnapshot
    {
        public const string SchemaId = "C1FormalItemPresentationCatalog.v1";
        public const int SchemaVersion = 1;

        private readonly ReadOnlyCollection<C1FormalItemPresentationRowSnapshot>
            rows;
        private readonly IReadOnlyDictionary<string,
            C1FormalItemPresentationRowSnapshot> rowsByIdentity;

        internal C1FormalItemPresentationCatalogSnapshot(
            IEnumerable<C1FormalItemPresentationRowSnapshot> sourceRows,
            string canonicalSignature)
        {
            schemaId = SchemaId;
            schemaVersion = SchemaVersion;
            rows = Array.AsReadOnly(sourceRows
                .OrderBy(value => value.rarityVersionIdentity,
                    StringComparer.Ordinal)
                .ToArray());
            rowsByIdentity = new ReadOnlyDictionary<string,
                C1FormalItemPresentationRowSnapshot>(rows.ToDictionary(
                    value => value.rarityVersionIdentity,
                    value => value,
                    StringComparer.Ordinal));
            this.canonicalSignature = canonicalSignature;
        }

        public string schemaId { get; }
        public int schemaVersion { get; }
        public IReadOnlyList<C1FormalItemPresentationRowSnapshot> Rows => rows;
        public string canonicalSignature { get; }

        public bool TryResolveExact(
            string baseItemId,
            string rarityKey,
            out C1FormalItemPresentationRowSnapshot row,
            out string diagnostic)
        {
            row = null;
            string exactBaseItemId = Normalize(baseItemId);
            string exactRarityKey = Normalize(rarityKey);
            if (exactBaseItemId.Length == 0)
            {
                diagnostic = C1FormalItemPresentationCatalogDiagnostics
                    .IdentityMissing;
                return false;
            }

            if (rowsByIdentity.TryGetValue(
                    exactBaseItemId + "@" + exactRarityKey,
                    out row))
            {
                diagnostic = C1FormalItemPresentationCatalogDiagnostics.None;
                return true;
            }

            diagnostic = rows.Any(value => string.Equals(
                value.baseItemId,
                exactBaseItemId,
                StringComparison.Ordinal))
                ? C1FormalItemPresentationCatalogDiagnostics.LookupRarityMismatch
                : C1FormalItemPresentationCatalogDiagnostics.LookupUnknown;
            return false;
        }

        public bool TryResolveArtwork(
            string baseItemId,
            string rarityKey,
            C1FormalItemArtworkLightingState lightingState,
            out C1FormalItemPresentationRowSnapshot row,
            out Sprite artwork,
            out string artworkIdentity)
        {
            artwork = null;
            artworkIdentity = string.Empty;
            return TryResolveExact(baseItemId, rarityKey, out row, out _)
                   && row.TryResolveArtwork(
                       lightingState,
                       out artwork,
                       out artworkIdentity);
        }

        public bool TryResolveUniqueBase(
            string baseItemId,
            C1FormalItemArtworkLightingState lightingState,
            out C1FormalItemPresentationRowSnapshot row,
            out Sprite artwork,
            out string artworkIdentity)
        {
            row = null;
            artwork = null;
            artworkIdentity = string.Empty;
            string exactBaseItemId = Normalize(baseItemId);
            C1FormalItemPresentationRowSnapshot[] matches = rows
                .Where(value => string.Equals(
                    value.baseItemId,
                    exactBaseItemId,
                    StringComparison.Ordinal))
                .ToArray();
            return matches.Length == 1
                   && (row = matches[0]).TryResolveArtwork(
                       lightingState,
                       out artwork,
                       out artworkIdentity);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public sealed class C1FormalItemPresentationCatalogResult
    {
        private C1FormalItemPresentationCatalogResult(
            C1FormalItemPresentationCatalogSnapshot snapshot,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.snapshot = snapshot;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool accepted => snapshot != null
            && string.Equals(diagnosticCode,
                C1FormalItemPresentationCatalogDiagnostics.None,
                StringComparison.Ordinal);
        public C1FormalItemPresentationCatalogSnapshot snapshot { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }

        internal static C1FormalItemPresentationCatalogResult Success(
            C1FormalItemPresentationCatalogSnapshot snapshot)
        {
            return new C1FormalItemPresentationCatalogResult(
                snapshot,
                C1FormalItemPresentationCatalogDiagnostics.None,
                string.Empty);
        }

        internal static C1FormalItemPresentationCatalogResult Failure(
            string diagnosticCode,
            string diagnosticMessage)
        {
            return new C1FormalItemPresentationCatalogResult(
                null,
                diagnosticCode,
                diagnosticMessage);
        }
    }

    [CreateAssetMenu(
        fileName = "C1FormalItemPresentationCatalog",
        menuName = "TalismanBag/V0.4/Items/Formal Item Presentation Catalog")]
    public sealed class C1FormalItemPresentationCatalog : ScriptableObject
    {
        public const string PresentationSourceIdentity =
            "C1FormalItemPresentationCatalog.presentation-only.v1";

        [SerializeField] private C1FormalItemPresentationCatalogRowDefinition[]
            rows = Array.Empty<C1FormalItemPresentationCatalogRowDefinition>();

        public C1FormalItemPresentationCatalogResult Resolve()
        {
            return TryCreateSnapshot(
                PresentationSourceIdentity,
                rows,
                string.Empty);
        }

        public C1FormalItemPresentationCatalogResult Resolve(
            CanonicalItemDefinitionResolver canonicalResolver)
        {
            if (canonicalResolver == null)
            {
                return Resolve();
            }

            List<C1FormalItemPresentationCatalogRowDefinition> resolvedRows =
                (rows ?? Array.Empty<
                        C1FormalItemPresentationCatalogRowDefinition>())
                .Where(value => value != null && value.IsSpecialLightingSource)
                .ToList();
            foreach (CanonicalItemDefinition definition in
                     canonicalResolver.OrdinaryDropDefinitions)
            {
                if (!CanonicalItemPresentationIdentityResolver.TryResolve(
                        definition,
                        out string effectFamilyKey,
                        out string presentationStyleKey,
                        out string cueIdentity))
                {
                    continue;
                }

                foreach (ItemInstanceRarityDefinition rarity in
                         ItemInstanceRarityCatalog.All)
                {
                    if (!CanonicalItemArtworkResolver.TryResolve(
                            definition,
                            rarity.rarity,
                            out Sprite artwork,
                            out string artworkResourcePath))
                    {
                        return Failure(
                            C1FormalItemPresentationCatalogDiagnostics
                                .ArtworkMissing,
                            "Canonical artwork missing: "
                            + definition.baseItemId + "@" + rarity.stableKey
                            + ".");
                    }

                    resolvedRows.Add(
                        C1FormalItemPresentationCatalogRowDefinition
                            .CreateSigned(
                                definition.baseItemId,
                                rarity.stableKey,
                                artwork,
                                artworkResourcePath,
                                effectFamilyKey,
                                presentationStyleKey,
                                cueIdentity,
                                string.Empty));
                }
            }

            return TryCreateSnapshot(
                PresentationSourceIdentity,
                resolvedRows,
                string.Empty);
        }

        public static C1FormalItemPresentationCatalogResult TryCreateSnapshot(
            string sourceProfileSignature,
            IEnumerable<C1FormalItemPresentationCatalogRowDefinition> rows,
            string expectedCatalogCanonicalSignature)
        {
            // Legacy serialized signature fields are intentionally ignored.
            // This asset owns presentation data only; gameplay identity and
            // combat facts come from the canonical Item authority.
            _ = sourceProfileSignature;
            _ = expectedCatalogCanonicalSignature;
            if (rows == null)
                return Failure(C1FormalItemPresentationCatalogDiagnostics.RowsNull,
                    "rows are required.");

            C1FormalItemPresentationCatalogRowDefinition[] materialized;
            try
            {
                materialized = rows.ToArray();
            }
            catch (Exception exception)
            {
                return Failure(
                    C1FormalItemPresentationCatalogDiagnostics.EnumerationFailed,
                    "rows enumeration failed: " + exception.GetType().Name);
            }

            if (materialized.Any(value => value == null))
                return Failure(C1FormalItemPresentationCatalogDiagnostics.RowNull,
                    "Every row is required.");
            if (materialized.Any(value => value.BaseItemId.Length == 0
                                          || (value.RarityKey.Length == 0
                                              && !value
                                                  .IsSpecialLightingSource)))
                return Failure(
                    C1FormalItemPresentationCatalogDiagnostics.IdentityMissing,
                    "Every exact baseItemId@rarityKey identity is required.");
            if (materialized.Select(value => value.BaseItemId + "@"
                                             + value.RarityKey)
                    .Distinct(StringComparer.Ordinal).Count()
                != materialized.Length)
                return Failure(
                    C1FormalItemPresentationCatalogDiagnostics.IdentityDuplicate,
                    "Presentation identities must be unique.");

            foreach (C1FormalItemPresentationCatalogRowDefinition row in
                     materialized)
            {
                string diagnostic = ValidateRow(row);
                if (!string.Equals(diagnostic,
                        C1FormalItemPresentationCatalogDiagnostics.None,
                        StringComparison.Ordinal))
                    return Failure(diagnostic,
                        "Presentation row rejected: " + row.BaseItemId + "@"
                        + row.RarityKey + ".");
            }

            C1FormalItemPresentationRowSnapshot[] snapshots = materialized
                .Select(value => new C1FormalItemPresentationRowSnapshot(value))
                .OrderBy(value => value.rarityVersionIdentity,
                    StringComparer.Ordinal)
                .ToArray();
            string canonicalSignature = ComputeCatalogCanonicalSignature(
                PresentationSourceIdentity,
                snapshots.Select(value => value.rowCanonicalSignature));

            return C1FormalItemPresentationCatalogResult.Success(
                new C1FormalItemPresentationCatalogSnapshot(
                    snapshots,
                    canonicalSignature));
        }

        public static string ComputeCatalogCanonicalSignature(
            string sourceProfileSignature,
            IEnumerable<string> rowCanonicalSignatures)
        {
            string[] ordered = (rowCanonicalSignatures
                                ?? Enumerable.Empty<string>())
                .Select(Normalize)
                .Where(value => value.Length > 0)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            return C1FormalItemPresentationCatalogSnapshot.SchemaId + ":"
                   + Normalize(sourceProfileSignature) + ":"
                   + string.Join(",", ordered);
        }

        public void AssignForEditor(
            string configuredSourceProfileSignature,
            C1FormalItemPresentationCatalogRowDefinition[] configuredRows)
        {
            _ = configuredSourceProfileSignature;
            rows = (configuredRows
                    ?? Array.Empty<C1FormalItemPresentationCatalogRowDefinition>())
                .ToArray();
        }

        private static string ValidateRow(
            C1FormalItemPresentationCatalogRowDefinition row)
        {
            if (ReferenceEquals(row.Artwork, null))
                return C1FormalItemPresentationCatalogDiagnostics.ArtworkMissing;
            if (row.ArtworkIdentity.Length == 0)
                return C1FormalItemPresentationCatalogDiagnostics
                    .ArtworkIdentityMissing;
            if (row.EffectFamilyKey.Length == 0)
                return C1FormalItemPresentationCatalogDiagnostics.FamilyMissing;
            if (row.PresentationStyleKey.Length == 0)
                return C1FormalItemPresentationCatalogDiagnostics.StyleMissing;
            if (row.CueIdentity.Length == 0)
                return C1FormalItemPresentationCatalogDiagnostics.CueMissing;
            if (row.IsSpecialLightingSource
                && (ReferenceEquals(row.UnlitArtwork, null)
                    || ReferenceEquals(row.LitArtwork, null)
                    || row.UnlitArtworkIdentity.Length == 0
                    || row.LitArtworkIdentity.Length == 0
                    || ReferenceEquals(row.UnlitArtwork, row.LitArtwork)
                    || string.Equals(row.UnlitArtworkIdentity,
                        row.LitArtworkIdentity,
                        StringComparison.Ordinal)))
                return C1FormalItemPresentationCatalogDiagnostics
                    .SpecialVariantInvalid;
            return C1FormalItemPresentationCatalogDiagnostics.None;
        }

        private static C1FormalItemPresentationCatalogResult Failure(
            string code,
            string message)
        {
            return C1FormalItemPresentationCatalogResult.Failure(code, message);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
