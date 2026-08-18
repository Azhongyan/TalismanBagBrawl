using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;

namespace TalismanBag.Items.CampaignBaseline
{
    public sealed class C1Lv1CampaignContextIdentity : IEquatable<C1Lv1CampaignContextIdentity>
    {
        public const string CampaignNormalLv1Id = "CAMPAIGN_NORMAL_LV1";

        private static readonly C1Lv1CampaignContextIdentity AcceptedIdentity =
            new C1Lv1CampaignContextIdentity(CampaignNormalLv1Id);

        private C1Lv1CampaignContextIdentity(string value)
        {
            this.value = value;
        }

        public string value { get; private set; }

        public static C1Lv1CampaignContextIdentity CampaignNormalLv1
        {
            get { return AcceptedIdentity; }
        }

        public static bool TryParse(
            string candidate,
            out C1Lv1CampaignContextIdentity identity)
        {
            if (string.Equals(candidate, CampaignNormalLv1Id, StringComparison.Ordinal))
            {
                identity = AcceptedIdentity;
                return true;
            }

            identity = null;
            return false;
        }

        public bool Equals(C1Lv1CampaignContextIdentity other)
        {
            return other != null
                && string.Equals(value, other.value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as C1Lv1CampaignContextIdentity);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(value);
        }

        public override string ToString()
        {
            return value;
        }
    }

    public static class C1Lv1StarterItemBaselineErrorCodes
    {
        public const string None = "NONE";
        public const string CampaignContextRequired = "CAMPAIGN_CONTEXT_REQUIRED";
        public const string UnsupportedCampaignContext = "UNSUPPORTED_CAMPAIGN_CONTEXT";
        public const string BaseItemIdRequired = "BASE_ITEM_ID_REQUIRED";
        public const string RarityVersionKeyRequired = "RARITY_VERSION_KEY_REQUIRED";
        public const string RarityVersionKeyMalformed = "RARITY_VERSION_KEY_MALFORMED";
        public const string RarityVersionKeyIdentityMismatch = "RARITY_VERSION_KEY_IDENTITY_MISMATCH";
        public const string UnsupportedRarity = "UNSUPPORTED_RARITY";
        public const string ProfileRevisionRequired = "PROFILE_REVISION_REQUIRED";
        public const string UnsupportedProfileRevision = "UNSUPPORTED_PROFILE_REVISION";
        public const string OrdinaryItemNotFound = "ORDINARY_ITEM_NOT_FOUND";
        public const string NonOrdinaryItem = "NON_ORDINARY_ITEM";
        public const string CatalogLookupFailed = "CATALOG_LOOKUP_FAILED";
        public const string BaselineIdentityNotFound = "BASELINE_IDENTITY_NOT_FOUND";
        public const string BaselineIdentityDuplicate = "BASELINE_IDENTITY_DUPLICATE";
        public const string BaselineCatalogInvalid = "BASELINE_CATALOG_INVALID";
        public const string SourceFixtureNull = "SOURCE_FIXTURE_NULL";
        public const string SourceFixtureEnumerationFailed = "SOURCE_FIXTURE_ENUMERATION_FAILED";
        public const string RowNull = "ROW_NULL";
        public const string RowCountInvalid = "ROW_COUNT_INVALID";
        public const string RequiredIdentifierEmpty = "REQUIRED_IDENTIFIER_EMPTY";
        public const string SchemaMismatch = "SCHEMA_MISMATCH";
        public const string ContextMismatch = "CONTEXT_MISMATCH";
        public const string RevisionMismatch = "REVISION_MISMATCH";
        public const string IdentitySetMismatch = "IDENTITY_SET_MISMATCH";
        public const string RarityMismatch = "RARITY_MISMATCH";
        public const string ApprovedValueMismatch = "APPROVED_VALUE_MISMATCH";
        public const string LightingRequirementMismatch = "LIGHTING_REQUIREMENT_MISMATCH";
        public const string CanonicalSignatureMissing = "CANONICAL_SIGNATURE_MISSING";
    }

    public sealed class C1Lv1StarterItemBaselineValidationError
    {
        public C1Lv1StarterItemBaselineValidationError(
            string errorCode,
            string path,
            string message)
        {
            this.errorCode = errorCode ?? string.Empty;
            this.path = path ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string errorCode { get; private set; }
        public string path { get; private set; }
        public string message { get; private set; }
    }

    public sealed class C1Lv1StarterItemBaselineValidationResult
    {
        private readonly ReadOnlyCollection<C1Lv1StarterItemBaselineValidationError> errorRows;

        internal C1Lv1StarterItemBaselineValidationResult(
            IEnumerable<C1Lv1StarterItemBaselineValidationError> errors,
            string canonicalSignature)
        {
            errorRows = Array.AsReadOnly((errors
                ?? Enumerable.Empty<C1Lv1StarterItemBaselineValidationError>()).ToArray());
            this.canonicalSignature = canonicalSignature ?? string.Empty;
        }

        public bool isValid
        {
            get { return errorRows.Count == 0; }
        }

        public IReadOnlyList<C1Lv1StarterItemBaselineValidationError> errors
        {
            get { return errorRows; }
        }

        public string canonicalSignature { get; private set; }

        public bool HasError(string errorCode)
        {
            return errorRows.Any(delegate(C1Lv1StarterItemBaselineValidationError error)
            {
                return string.Equals(error.errorCode, errorCode, StringComparison.Ordinal);
            });
        }
    }

    public sealed class C1Lv1StarterItemBaselineProfile
    {
        internal C1Lv1StarterItemBaselineProfile(
            string schemaId,
            C1Lv1CampaignContextIdentity campaignContext,
            string profileRevision,
            string baseItemId,
            ItemInstanceRarity rarity,
            string rarityKey,
            string rarityVersionKey,
            int directDamage,
            decimal cooldownSeconds,
            string lightingRequirement)
        {
            this.schemaId = schemaId ?? string.Empty;
            this.campaignContext = campaignContext;
            this.profileRevision = profileRevision ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.rarity = rarity;
            this.rarityKey = rarityKey ?? string.Empty;
            this.rarityVersionKey = rarityVersionKey ?? string.Empty;
            this.directDamage = directDamage;
            this.cooldownSeconds = cooldownSeconds;
            this.lightingRequirement = lightingRequirement ?? string.Empty;
            canonicalSignature = C1Lv1StarterItemBaselineCanonical.ProfileSignature(this);
        }

        public string schemaId { get; private set; }
        public C1Lv1CampaignContextIdentity campaignContext { get; private set; }
        public string profileRevision { get; private set; }
        public string baseItemId { get; private set; }
        public ItemInstanceRarity rarity { get; private set; }
        public string rarityKey { get; private set; }
        public string rarityVersionKey { get; private set; }
        public int directDamage { get; private set; }
        public decimal cooldownSeconds { get; private set; }
        public string lightingRequirement { get; private set; }
        public string canonicalSignature { get; private set; }
    }

    public sealed class C1Lv1StarterItemFactProjection
    {
        internal C1Lv1StarterItemFactProjection(C1Lv1StarterItemBaselineProfile source)
        {
            schemaId = C1Lv1StarterItemBaselineCatalog.ProjectionSchemaId;
            campaignContext = source.campaignContext;
            profileRevision = source.profileRevision;
            baseItemId = source.baseItemId;
            rarity = source.rarity;
            rarityKey = source.rarityKey;
            rarityVersionKey = source.rarityVersionKey;
            directDamage = source.directDamage;
            cooldownSeconds = source.cooldownSeconds;
            lightingRequirement = source.lightingRequirement;
            sourceCanonicalSignature = source.canonicalSignature;
            canonicalSignature = C1Lv1StarterItemBaselineCanonical.ProjectionSignature(this);
        }

        public string schemaId { get; private set; }
        public C1Lv1CampaignContextIdentity campaignContext { get; private set; }
        public string profileRevision { get; private set; }
        public string baseItemId { get; private set; }
        public ItemInstanceRarity rarity { get; private set; }
        public string rarityKey { get; private set; }
        public string rarityVersionKey { get; private set; }
        public int directDamage { get; private set; }
        public decimal cooldownSeconds { get; private set; }
        public string lightingRequirement { get; private set; }
        public string sourceCanonicalSignature { get; private set; }
        public string canonicalSignature { get; private set; }
    }

    public sealed class C1Lv1StarterItemProjectionResult
    {
        private C1Lv1StarterItemProjectionResult(
            C1Lv1StarterItemFactProjection projection,
            string errorCode,
            string errorMessage)
        {
            this.projection = projection;
            this.errorCode = errorCode ?? string.Empty;
            this.errorMessage = errorMessage ?? string.Empty;
        }

        public bool isSuccess
        {
            get
            {
                return projection != null
                    && string.Equals(
                        errorCode,
                        C1Lv1StarterItemBaselineErrorCodes.None,
                        StringComparison.Ordinal);
            }
        }

        public C1Lv1StarterItemFactProjection projection { get; private set; }
        public string errorCode { get; private set; }
        public string errorMessage { get; private set; }

        internal static C1Lv1StarterItemProjectionResult Success(
            C1Lv1StarterItemFactProjection projection)
        {
            return new C1Lv1StarterItemProjectionResult(
                projection,
                C1Lv1StarterItemBaselineErrorCodes.None,
                string.Empty);
        }

        internal static C1Lv1StarterItemProjectionResult Failure(
            string errorCode,
            string errorMessage)
        {
            return new C1Lv1StarterItemProjectionResult(null, errorCode, errorMessage);
        }
    }

    public static class C1FormalItemActionCostDiagnosticCodes
    {
        public const string None = "NONE";
        public const string ActionDefinitionRequired =
            "ITEM_ACTION_DEFINITION_REQUIRED";
        public const string ActionDefinitionMismatch =
            "ITEM_ACTION_DEFINITION_MISMATCH";
        public const string TimingMismatch = "ITEM_ACTION_TIMING_MISMATCH";
    }

    public sealed class C1FormalItemActionCostFact
    {
        internal C1FormalItemActionCostFact(
            string baseItemId,
            string rarityKey,
            string actionDefinitionId,
            string resourceKey,
            int cost,
            string sourceRevision,
            string sourceProfileRevision,
            string sourceProfileIdentity,
            string sourceProjectionIdentity,
            long cadenceMilliseconds,
            long firstOffsetMilliseconds)
        {
            this.baseItemId = baseItemId ?? string.Empty;
            this.rarityKey = rarityKey ?? string.Empty;
            rarityVersionKey = this.baseItemId + "@" + this.rarityKey;
            this.actionDefinitionId = actionDefinitionId ?? string.Empty;
            this.resourceKey = resourceKey ?? string.Empty;
            this.cost = cost;
            this.sourceRevision = sourceRevision ?? string.Empty;
            this.sourceProfileRevision = sourceProfileRevision ?? string.Empty;
            sourceProfileCanonicalSignature = sourceProfileIdentity ?? string.Empty;
            sourceProjectionCanonicalSignature = sourceProjectionIdentity ?? string.Empty;
            this.cadenceMilliseconds = cadenceMilliseconds;
            this.firstOffsetMilliseconds = firstOffsetMilliseconds;
        }

        internal C1FormalItemActionCostFact(
            C1Lv1StarterItemFactProjection source,
            string actionDefinitionId,
            string resourceKey,
            int cost,
            string sourceRevision,
            long cadenceMilliseconds,
            long firstOffsetMilliseconds)
        {
            baseItemId = source.baseItemId;
            rarityKey = source.rarityKey;
            rarityVersionKey = source.rarityVersionKey;
            this.actionDefinitionId = actionDefinitionId;
            this.resourceKey = resourceKey;
            this.cost = cost;
            this.sourceRevision = sourceRevision;
            sourceProfileRevision = source.profileRevision;
            sourceProfileCanonicalSignature = source.sourceCanonicalSignature;
            sourceProjectionCanonicalSignature = source.canonicalSignature;
            this.cadenceMilliseconds = cadenceMilliseconds;
            this.firstOffsetMilliseconds = firstOffsetMilliseconds;
        }

        public string baseItemId { get; }
        public string rarityKey { get; }
        public string rarityVersionKey { get; }
        public string actionDefinitionId { get; }
        public string resourceKey { get; }
        public int cost { get; }
        public string sourceRevision { get; }
        public string sourceProfileRevision { get; }
        public string sourceProfileCanonicalSignature { get; }
        public string sourceProjectionCanonicalSignature { get; }
        public long cadenceMilliseconds { get; }
        public long firstOffsetMilliseconds { get; }
    }

    public sealed class C1FormalItemActionCostProjectionResult
    {
        private C1FormalItemActionCostProjectionResult(
            C1FormalItemActionCostFact fact,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.fact = fact;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool isSuccess => fact != null
            && string.Equals(
                diagnosticCode,
                C1FormalItemActionCostDiagnosticCodes.None,
                StringComparison.Ordinal);
        public C1FormalItemActionCostFact fact { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }

        internal static C1FormalItemActionCostProjectionResult Success(
            C1FormalItemActionCostFact fact)
        {
            return new C1FormalItemActionCostProjectionResult(
                fact,
                C1FormalItemActionCostDiagnosticCodes.None,
                string.Empty);
        }

        internal static C1FormalItemActionCostProjectionResult Failure(
            string diagnosticCode,
            string diagnosticMessage)
        {
            return new C1FormalItemActionCostProjectionResult(
                null,
                diagnosticCode,
                diagnosticMessage);
        }
    }

    public static class C1Lv1FormalItemActionCostCatalog
    {
        public const string ActionDefinitionId =
            "C1_CAMPAIGN_LV1_PERIODIC_DIRECT_DAMAGE_ACTION";
        public const string ResourceKey = "nian";
        public const int Cost = 1;
        public const string SourceRevision =
            "C1_CAMPAIGN_LV1_ITEM_ACTION_COST_PLAYTEST_V1";
        public const long CadenceMilliseconds = 2000L;
        public const long FirstOffsetMilliseconds = 2000L;

        public static C1FormalItemActionCostProjectionResult TryGetActionCost(
            string campaignContext,
            string baseItemId,
            string rarityVersionKey,
            string profileRevision,
            string actionDefinitionId,
            long cadenceMilliseconds,
            long firstOffsetMilliseconds)
        {
            if (string.IsNullOrWhiteSpace(actionDefinitionId))
            {
                return C1FormalItemActionCostProjectionResult.Failure(
                    C1FormalItemActionCostDiagnosticCodes
                        .ActionDefinitionRequired,
                    "actionDefinitionId is required.");
            }

            if (!string.Equals(
                    actionDefinitionId,
                    ActionDefinitionId,
                    StringComparison.Ordinal))
            {
                return C1FormalItemActionCostProjectionResult.Failure(
                    C1FormalItemActionCostDiagnosticCodes
                        .ActionDefinitionMismatch,
                    "The action definition is not the released Lv1 periodic action.");
            }

            C1Lv1StarterItemProjectionResult projection =
                C1Lv1StarterItemBaselineCatalog.TryGetProjection(
                    campaignContext,
                    baseItemId,
                    rarityVersionKey,
                    profileRevision);
            if (projection == null || !projection.isSuccess
                || projection.projection == null)
            {
                return C1FormalItemActionCostProjectionResult.Failure(
                    projection == null
                        ? C1Lv1StarterItemBaselineErrorCodes
                            .BaselineIdentityNotFound
                        : projection.errorCode,
                    projection == null
                        ? "The released Lv1 Item profile is unavailable."
                        : projection.errorMessage);
            }

            decimal cadenceSeconds = cadenceMilliseconds / 1000m;
            if (cadenceMilliseconds != CadenceMilliseconds
                || firstOffsetMilliseconds != FirstOffsetMilliseconds
                || cadenceSeconds != projection.projection.cooldownSeconds)
            {
                return C1FormalItemActionCostProjectionResult.Failure(
                    C1FormalItemActionCostDiagnosticCodes.TimingMismatch,
                    "The action timing does not match the released Lv1 cooldown.");
            }

            return C1FormalItemActionCostProjectionResult.Success(
                new C1FormalItemActionCostFact(
                    projection.projection,
                    ActionDefinitionId,
                    ResourceKey,
                    Cost,
                    SourceRevision,
                    CadenceMilliseconds,
                    FirstOffsetMilliseconds));
        }

        internal static bool DefinesActiveAction(string baseItemId)
        {
            return string.Equals(baseItemId, "I001", StringComparison.Ordinal)
                || string.Equals(baseItemId, "I002", StringComparison.Ordinal);
        }
    }

    public static class C1Lv1StarterItemBaselineCatalog
    {
        public const string BaselineSchemaId = "C1Lv1StarterItemBaseline.v1";
        public const string ProjectionSchemaId = "C1Lv1StarterItemFactProjection.v1";
        public const string ProfileRevision = "C1LV1_STARTER_ITEM_BASELINE_R1";
        public const string WhiteRarityKey = "white";
        public const string LightingRequirement = "LIT_REQUIRED";

        private const string I001Id = "I001";
        private const string I002Id = "I002";
        private const int I001ApprovedDirectDamage = 82;
        private const int I002ApprovedDirectDamage = 28;
        private const decimal ApprovedCooldownSeconds = 2m;

        private static readonly ReadOnlyCollection<C1Lv1StarterItemBaselineProfile> ProfileRows =
            Array.AsReadOnly(new[]
            {
                NewApprovedProfile(I001Id, I001ApprovedDirectDamage),
                NewApprovedProfile(I002Id, I002ApprovedDirectDamage)
            });

        private static readonly C1Lv1StarterItemBaselineValidationResult CatalogValidation =
            ValidateProfiles(ProfileRows);

        public static IReadOnlyList<C1Lv1StarterItemBaselineProfile> All
        {
            get { return ProfileRows; }
        }

        public static C1Lv1StarterItemBaselineValidationResult Validation
        {
            get { return CatalogValidation; }
        }

        public static string canonicalSignature
        {
            get { return CatalogValidation.canonicalSignature; }
        }

        public static C1Lv1StarterItemProjectionResult TryGetProjection(
            string campaignContext,
            string baseItemId,
            string rarityVersionKey,
            string profileRevision)
        {
            C1Lv1CampaignContextIdentity parsedContext;
            if (string.IsNullOrWhiteSpace(campaignContext))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.CampaignContextRequired,
                    "campaignContext is required.");
            }

            if (!C1Lv1CampaignContextIdentity.TryParse(campaignContext, out parsedContext))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.UnsupportedCampaignContext,
                    "campaignContext is not supported by this carrier.");
            }

            if (string.IsNullOrWhiteSpace(baseItemId))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.BaseItemIdRequired,
                    "baseItemId is required.");
            }

            if (string.IsNullOrWhiteSpace(rarityVersionKey))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.RarityVersionKeyRequired,
                    "rarityVersionKey is required.");
            }

            if (string.IsNullOrWhiteSpace(profileRevision))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.ProfileRevisionRequired,
                    "profileRevision is required.");
            }

            if (!string.Equals(profileRevision, ProfileRevision, StringComparison.Ordinal))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.UnsupportedProfileRevision,
                    "profileRevision is not supported by this carrier.");
            }

            C1Lv1StarterItemProjectionResult identityFailure =
                ValidateOrdinaryIdentityForQuery(baseItemId);
            if (identityFailure != null)
            {
                return identityFailure;
            }

            string versionBaseItemId;
            string versionRarityKey;
            C1Lv1StarterItemProjectionResult versionFailure = ParseVersionKey(
                rarityVersionKey,
                out versionBaseItemId,
                out versionRarityKey);
            if (versionFailure != null)
            {
                return versionFailure;
            }

            if (!string.Equals(versionBaseItemId, baseItemId, StringComparison.Ordinal))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.RarityVersionKeyIdentityMismatch,
                    "rarityVersionKey does not belong to baseItemId.");
            }

            if (!string.Equals(versionRarityKey, WhiteRarityKey, StringComparison.Ordinal))
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.UnsupportedRarity,
                    "Only the white rarity is supported by this carrier.");
            }

            if (!CatalogValidation.isValid)
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.BaselineCatalogInvalid,
                    "The baseline catalog failed its immutable validation.");
            }

            C1Lv1StarterItemBaselineProfile[] matches = ProfileRows
                .Where(delegate(C1Lv1StarterItemBaselineProfile profile)
                {
                    return profile.campaignContext.Equals(parsedContext)
                        && string.Equals(profile.baseItemId, baseItemId, StringComparison.Ordinal)
                        && string.Equals(
                            profile.rarityVersionKey,
                            rarityVersionKey,
                            StringComparison.Ordinal)
                        && string.Equals(
                            profile.profileRevision,
                            profileRevision,
                            StringComparison.Ordinal);
                })
                .ToArray();

            if (matches.Length == 0)
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.BaselineIdentityNotFound,
                    "No baseline row matches the requested stable identity.");
            }

            if (matches.Length != 1)
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.BaselineIdentityDuplicate,
                    "More than one baseline row matches the requested stable identity.");
            }

            return C1Lv1StarterItemProjectionResult.Success(
                new C1Lv1StarterItemFactProjection(matches[0]));
        }

        public static C1Lv1StarterItemBaselineValidationResult ValidateProfiles(
            IEnumerable<C1Lv1StarterItemBaselineProfile> source)
        {
            List<C1Lv1StarterItemBaselineValidationError> errors =
                new List<C1Lv1StarterItemBaselineValidationError>();

            if (source == null)
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.SourceFixtureNull,
                    "profiles",
                    "Profile fixture is null."));
                return new C1Lv1StarterItemBaselineValidationResult(errors, string.Empty);
            }

            C1Lv1StarterItemBaselineProfile[] rows;
            try
            {
                rows = source.ToArray();
            }
            catch (Exception exception)
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.SourceFixtureEnumerationFailed,
                    "profiles",
                    "Profile fixture enumeration failed: " + exception.GetType().Name));
                return new C1Lv1StarterItemBaselineValidationResult(errors, string.Empty);
            }

            if (rows.Length != 2)
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.RowCountInvalid,
                    "profiles",
                    "The baseline catalog must contain exactly two rows."));
            }

            for (int index = 0; index < rows.Length; index++)
            {
                ValidateProfile(rows[index], index, errors);
            }

            C1Lv1StarterItemBaselineProfile[] nonNullRows = rows
                .Where(delegate(C1Lv1StarterItemBaselineProfile row) { return row != null; })
                .ToArray();

            IGrouping<string, C1Lv1StarterItemBaselineProfile>[] duplicateGroups = nonNullRows
                .GroupBy(ProfileIdentity, StringComparer.Ordinal)
                .Where(delegate(IGrouping<string, C1Lv1StarterItemBaselineProfile> group)
                {
                    return group.Count() > 1;
                })
                .ToArray();
            foreach (IGrouping<string, C1Lv1StarterItemBaselineProfile> duplicate in duplicateGroups)
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.BaselineIdentityDuplicate,
                    "profiles",
                    "Duplicate baseline identity: " + duplicate.Key));
            }

            string[] identities = nonNullRows
                .Select(delegate(C1Lv1StarterItemBaselineProfile row)
                {
                    return row.rarityVersionKey;
                })
                .OrderBy(delegate(string identity) { return identity; }, StringComparer.Ordinal)
                .ToArray();
            string[] expectedIdentities = { I001Id + "@" + WhiteRarityKey, I002Id + "@" + WhiteRarityKey };
            if (!identities.SequenceEqual(expectedIdentities, StringComparer.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.IdentitySetMismatch,
                    "profiles",
                    "The baseline identity set must be exactly I001@white and I002@white."));
            }

            string signature = C1Lv1StarterItemBaselineCanonical.CatalogSignature(nonNullRows);
            if (string.IsNullOrWhiteSpace(signature))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.CanonicalSignatureMissing,
                    "profiles.canonicalSignature",
                    "Catalog canonical signature is required."));
            }

            return new C1Lv1StarterItemBaselineValidationResult(errors, signature);
        }

        private static C1Lv1StarterItemBaselineProfile NewApprovedProfile(
            string baseItemId,
            int directDamage)
        {
            return new C1Lv1StarterItemBaselineProfile(
                BaselineSchemaId,
                C1Lv1CampaignContextIdentity.CampaignNormalLv1,
                ProfileRevision,
                baseItemId,
                ItemInstanceRarity.White,
                WhiteRarityKey,
                baseItemId + "@" + WhiteRarityKey,
                directDamage,
                ApprovedCooldownSeconds,
                LightingRequirement);
        }

        private static void ValidateProfile(
            C1Lv1StarterItemBaselineProfile profile,
            int index,
            ICollection<C1Lv1StarterItemBaselineValidationError> errors)
        {
            string path = "profiles[" + index.ToString(CultureInfo.InvariantCulture) + "]";
            if (profile == null)
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.RowNull,
                    path,
                    "Baseline row is null."));
                return;
            }

            if (string.IsNullOrWhiteSpace(profile.schemaId)
                || profile.campaignContext == null
                || string.IsNullOrWhiteSpace(profile.profileRevision)
                || string.IsNullOrWhiteSpace(profile.baseItemId)
                || string.IsNullOrWhiteSpace(profile.rarityKey)
                || string.IsNullOrWhiteSpace(profile.rarityVersionKey)
                || string.IsNullOrWhiteSpace(profile.lightingRequirement))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.RequiredIdentifierEmpty,
                    path,
                    "A required baseline identifier is empty."));
            }

            if (!string.Equals(profile.schemaId, BaselineSchemaId, StringComparison.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.SchemaMismatch,
                    path + ".schemaId",
                    "Baseline schema does not match."));
            }

            if (profile.campaignContext == null
                || !profile.campaignContext.Equals(C1Lv1CampaignContextIdentity.CampaignNormalLv1))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.ContextMismatch,
                    path + ".campaignContext",
                    "Baseline context does not match."));
            }

            if (!string.Equals(profile.profileRevision, ProfileRevision, StringComparison.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.RevisionMismatch,
                    path + ".profileRevision",
                    "Baseline revision does not match."));
            }

            if (profile.rarity != ItemInstanceRarity.White
                || !string.Equals(profile.rarityKey, WhiteRarityKey, StringComparison.Ordinal)
                || !string.Equals(
                    profile.rarityVersionKey,
                    profile.baseItemId + "@" + WhiteRarityKey,
                    StringComparison.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.RarityMismatch,
                    path + ".rarity",
                    "Baseline rarity identity must be white."));
            }

            int expectedDamage;
            if (string.Equals(profile.baseItemId, I001Id, StringComparison.Ordinal))
            {
                expectedDamage = I001ApprovedDirectDamage;
            }
            else if (string.Equals(profile.baseItemId, I002Id, StringComparison.Ordinal))
            {
                expectedDamage = I002ApprovedDirectDamage;
            }
            else
            {
                expectedDamage = int.MinValue;
            }

            if (profile.directDamage != expectedDamage
                || profile.cooldownSeconds != ApprovedCooldownSeconds)
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.ApprovedValueMismatch,
                    path + ".approvedValues",
                    "Baseline values do not match the accepted profile."));
            }

            if (!string.Equals(
                profile.lightingRequirement,
                LightingRequirement,
                StringComparison.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.LightingRequirementMismatch,
                    path + ".lightingRequirement",
                    "Lighting requirement does not match."));
            }

            if (string.IsNullOrWhiteSpace(profile.canonicalSignature))
            {
                errors.Add(Error(
                    C1Lv1StarterItemBaselineErrorCodes.CanonicalSignatureMissing,
                    path + ".canonicalSignature",
                    "Profile canonical signature is required."));
            }

            string identityErrorCode;
            if (!TryValidateOrdinaryIdentity(profile.baseItemId, out identityErrorCode))
            {
                errors.Add(Error(
                    identityErrorCode,
                    path + ".baseItemId",
                    "The base Item identity is not an ordinary catalog Item."));
            }
        }

        private static C1Lv1StarterItemProjectionResult ValidateOrdinaryIdentityForQuery(
            string baseItemId)
        {
            string errorCode;
            if (TryValidateOrdinaryIdentity(baseItemId, out errorCode))
            {
                return null;
            }

            string message = string.Equals(
                errorCode,
                C1Lv1StarterItemBaselineErrorCodes.NonOrdinaryItem,
                StringComparison.Ordinal)
                ? "The requested catalog identity is not an ordinary Item."
                : "The requested ordinary Item identity was not found.";
            return Failure(errorCode, message);
        }

        private static bool TryValidateOrdinaryIdentity(
            string baseItemId,
            out string errorCode)
        {
            ItemInnerDataDefinition definition;
            try
            {
                definition = ItemInnerDataCatalog.FindById(baseItemId);
            }
            catch (Exception)
            {
                errorCode = C1Lv1StarterItemBaselineErrorCodes.CatalogLookupFailed;
                return false;
            }

            if (definition == null)
            {
                errorCode = C1Lv1StarterItemBaselineErrorCodes.OrdinaryItemNotFound;
                return false;
            }

            if (definition.isLightingSource)
            {
                errorCode = C1Lv1StarterItemBaselineErrorCodes.NonOrdinaryItem;
                return false;
            }

            errorCode = C1Lv1StarterItemBaselineErrorCodes.None;
            return true;
        }

        private static C1Lv1StarterItemProjectionResult ParseVersionKey(
            string rarityVersionKey,
            out string baseItemId,
            out string rarityKey)
        {
            baseItemId = string.Empty;
            rarityKey = string.Empty;
            int separatorIndex = rarityVersionKey.IndexOf('@');
            if (separatorIndex <= 0
                || separatorIndex != rarityVersionKey.LastIndexOf('@')
                || separatorIndex == rarityVersionKey.Length - 1)
            {
                return Failure(
                    C1Lv1StarterItemBaselineErrorCodes.RarityVersionKeyMalformed,
                    "rarityVersionKey must be baseItemId@rarity.");
            }

            baseItemId = rarityVersionKey.Substring(0, separatorIndex);
            rarityKey = rarityVersionKey.Substring(separatorIndex + 1);
            return null;
        }

        private static string ProfileIdentity(C1Lv1StarterItemBaselineProfile profile)
        {
            return (profile.campaignContext == null ? string.Empty : profile.campaignContext.value)
                + "\u001f" + profile.baseItemId
                + "\u001f" + profile.rarityVersionKey
                + "\u001f" + profile.profileRevision;
        }

        private static C1Lv1StarterItemProjectionResult Failure(
            string errorCode,
            string errorMessage)
        {
            return C1Lv1StarterItemProjectionResult.Failure(errorCode, errorMessage);
        }

        private static C1Lv1StarterItemBaselineValidationError Error(
            string errorCode,
            string path,
            string message)
        {
            return new C1Lv1StarterItemBaselineValidationError(errorCode, path, message);
        }
    }

    internal static class C1Lv1StarterItemBaselineCanonical
    {
        internal static string ProfileSignature(C1Lv1StarterItemBaselineProfile profile)
        {
            if (profile == null)
            {
                return string.Empty;
            }

            string canonical = string.Join("|", new[]
            {
                "schemaId=" + profile.schemaId,
                "campaignContext=" + ContextValue(profile.campaignContext),
                "profileRevision=" + profile.profileRevision,
                "baseItemId=" + profile.baseItemId,
                "rarity=" + profile.rarityKey,
                "rarityVersionKey=" + profile.rarityVersionKey,
                "directDamage=" + profile.directDamage.ToString(CultureInfo.InvariantCulture),
                "cooldownSeconds=" + profile.cooldownSeconds.ToString("0.############################", CultureInfo.InvariantCulture),
                "lightingRequirement=" + profile.lightingRequirement
            });
            return Sha256(canonical);
        }

        internal static string CatalogSignature(
            IEnumerable<C1Lv1StarterItemBaselineProfile> profiles)
        {
            C1Lv1StarterItemBaselineProfile[] ordered = (profiles
                ?? Enumerable.Empty<C1Lv1StarterItemBaselineProfile>())
                .Where(delegate(C1Lv1StarterItemBaselineProfile profile) { return profile != null; })
                .OrderBy(delegate(C1Lv1StarterItemBaselineProfile profile)
                {
                    return ContextValue(profile.campaignContext)
                        + "\u001f" + profile.baseItemId
                        + "\u001f" + profile.rarityVersionKey
                        + "\u001f" + profile.profileRevision;
                }, StringComparer.Ordinal)
                .ToArray();

            StringBuilder canonical = new StringBuilder();
            canonical.Append("schemaId=");
            canonical.Append(C1Lv1StarterItemBaselineCatalog.BaselineSchemaId);
            canonical.Append("|profileRevision=");
            canonical.Append(C1Lv1StarterItemBaselineCatalog.ProfileRevision);
            canonical.Append("|rowCount=");
            canonical.Append(ordered.Length.ToString(CultureInfo.InvariantCulture));
            foreach (C1Lv1StarterItemBaselineProfile profile in ordered)
            {
                canonical.Append('\n');
                canonical.Append(ProfileSignature(profile));
            }

            return Sha256(canonical.ToString());
        }

        internal static string ProjectionSignature(C1Lv1StarterItemFactProjection projection)
        {
            if (projection == null)
            {
                return string.Empty;
            }

            string canonical = string.Join("|", new[]
            {
                "schemaId=" + projection.schemaId,
                "campaignContext=" + ContextValue(projection.campaignContext),
                "profileRevision=" + projection.profileRevision,
                "baseItemId=" + projection.baseItemId,
                "rarity=" + projection.rarityKey,
                "rarityVersionKey=" + projection.rarityVersionKey,
                "directDamage=" + projection.directDamage.ToString(CultureInfo.InvariantCulture),
                "cooldownSeconds=" + projection.cooldownSeconds.ToString("0.############################", CultureInfo.InvariantCulture),
                "lightingRequirement=" + projection.lightingRequirement,
                "sourceCanonicalSignature=" + projection.sourceCanonicalSignature
            });
            return Sha256(canonical);
        }

        private static string ContextValue(C1Lv1CampaignContextIdentity context)
        {
            return context == null ? string.Empty : context.value;
        }

        private static string Sha256(string value)
        {
            byte[] bytes = new UTF8Encoding(false).GetBytes(value ?? string.Empty);
            byte[] hash;
            using (SHA256 algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(bytes);
            }

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            foreach (byte part in hash)
            {
                hex.Append(part.ToString("x2", CultureInfo.InvariantCulture));
            }

            return hex.ToString();
        }
    }
}
