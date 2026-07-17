using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SystemSnapshot
{
    public interface IEnemySystemDataValidator
    {
        IReadOnlyList<EnemySystemValidationIssue> Validate(EnemySystemSnapshotInput input);
    }

    public interface IEnemySystemSnapshotProvider
    {
        EnemySystemSnapshot CreateSnapshot(EnemySystemSnapshotInput input);
    }

    public sealed class EnemySystemValidationIssue
    {
        public EnemySystemValidationIssue(string code, EnemySystemValidationCategory category,
            string componentId, string path, string message)
        {
            Code = code ?? string.Empty;
            Category = category;
            ComponentId = componentId ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public EnemySystemValidationCategory Category { get; }
        public string ComponentId { get; }
        public string Path { get; }
        public string Message { get; }
    }

    public sealed class EnemySystemValidationException : Exception
    {
        public EnemySystemValidationException(IReadOnlyList<EnemySystemValidationIssue> issues)
            : base("Enemy system snapshot validation failed with "
                + (issues == null ? 0 : issues.Count).ToString(CultureInfo.InvariantCulture)
                + " issue(s).")
        {
            Issues = Array.AsReadOnly((issues ?? Array.Empty<EnemySystemValidationIssue>()).ToArray());
        }

        public IReadOnlyList<EnemySystemValidationIssue> Issues { get; }
    }

    public sealed class DefaultEnemySystemSnapshotProvider : IEnemySystemSnapshotProvider
    {
        public static readonly DefaultEnemySystemSnapshotProvider Instance =
            new DefaultEnemySystemSnapshotProvider(DefaultEnemySystemDataValidator.Instance);

        private readonly IEnemySystemDataValidator validator;

        public DefaultEnemySystemSnapshotProvider(IEnemySystemDataValidator validator)
        {
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public EnemySystemSnapshot CreateSnapshot(EnemySystemSnapshotInput input)
        {
            IReadOnlyList<EnemySystemValidationIssue> issues = validator.Validate(input);
            if (issues.Count > 0)
            {
                throw new EnemySystemValidationException(issues);
            }
            return new EnemySystemSnapshot(input);
        }
    }

    public sealed class DefaultEnemySystemDataValidator : IEnemySystemDataValidator
    {
        private static readonly string[] PlayerForbiddenTokens =
        {
            "EncounterCompositionCatalogSnapshot", "EncounterSlot", "quantity", "developerContentTagIds",
            "CarrierMechanicBinding", "MapRuleMechanicBinding", "SkillSequence", "CarrierSkillBinding",
            "BossPhaseEntryCondition", "BossPhasePlan", "PressureSourceBinding", "CounterWindowSourceBinding",
            "PressureCounterWindowBinding", "InternalOnly", "DeveloperOnly", "BuildCapabilityKey",
            "MinimumCapabilityBasisPoints", "RequirementGroup", "ReadinessBand", "CapabilityGap",
            "RequirementStatus", "MapRuleCapabilityAdjustment", "DeltaBasisPoints", "BuildCapabilitySnapshot",
            "EnemyReadinessEvaluationResult", "DropBias", "randomSeed", "hardSolution", "answerToken"
        };

        public static readonly DefaultEnemySystemDataValidator Instance = new DefaultEnemySystemDataValidator();

        public IReadOnlyList<EnemySystemValidationIssue> Validate(EnemySystemSnapshotInput input)
        {
            List<EnemySystemValidationIssue> issues = new List<EnemySystemValidationIssue>();
            if (input == null)
            {
                Add(issues, "E09_NULL_INPUT", EnemySystemValidationCategory.Schema, "E09", "input",
                    "EnemySystemSnapshotInput must not be null.");
                return Sort(issues);
            }

            Require(issues, input.EnemyDomainSnapshot, "E01", "EnemyDomainSnapshot");
            Require(issues, input.EnemyMechanicVocabularySnapshot, "E02", "EnemyMechanicVocabularySnapshot");
            Require(issues, input.EnemyValidationContentSnapshot, "E03", "EnemyValidationContentSnapshot");
            Require(issues, input.EncounterCompositionCatalogSnapshot, "E04", "EncounterCompositionCatalogSnapshot");
            Require(issues, input.EnemySkillBossPhaseCatalogSnapshot, "E05", "EnemySkillBossPhaseCatalogSnapshot");
            Require(issues, input.CounterWindowAndPressureCatalogSnapshot, "E06", "CounterWindowAndPressureCatalogSnapshot");
            if (issues.Count > 0)
            {
                return Sort(issues);
            }

            ValidateSchemas(input, issues);
            ValidateSignatures(input, issues);
            ValidateDomainAgreement(input, issues);
            IReadOnlyList<EnemySystemSchemaManifestEntry> manifest = EnemySystemSnapshotManifest.CreateExpected(input);
            ValidateManifestCore(manifest, manifest, issues);
            IReadOnlyList<EnemySystemIdentitySnapshot> identities = EnemySystemSnapshotBuilder.CreateIdentities(input);
            ValidateIdentities(identities, issues);
            ValidateIsolation(input, identities, issues);
            IReadOnlyList<EnemySystemRelationSnapshot> relations = EnemySystemSnapshotBuilder.CreateRelations(input, identities);
            ValidateRelations(relations, issues);
            ValidatePlayerSafe(input, issues);
            ValidateRootContract(issues);
            return Sort(issues);
        }

        public IReadOnlyList<EnemySystemValidationIssue> ValidateSchemaManifest(
            EnemySystemSnapshotInput input,
            IReadOnlyList<EnemySystemSchemaManifestEntry> candidate)
        {
            List<EnemySystemValidationIssue> issues = new List<EnemySystemValidationIssue>();
            if (input == null)
            {
                Add(issues, "E09_NULL_INPUT", EnemySystemValidationCategory.Schema, "E09", "input",
                    "EnemySystemSnapshotInput must not be null.");
                return Sort(issues);
            }
            IReadOnlyList<EnemySystemSchemaManifestEntry> expected = EnemySystemSnapshotManifest.CreateExpected(input);
            ValidateManifestCore(candidate, expected, issues);
            return Sort(issues);
        }

        public IReadOnlyList<EnemySystemValidationIssue> ValidateRelationIndex(
            IReadOnlyList<EnemySystemRelationSnapshot> relations)
        {
            List<EnemySystemValidationIssue> issues = new List<EnemySystemValidationIssue>();
            if (relations == null)
            {
                Add(issues, "E09_RELATION_INDEX_NULL", EnemySystemValidationCategory.Relation,
                    "E09", "RelationIndex", "Relation index must not be null.");
            }
            else
            {
                ValidateRelations(relations, issues);
            }
            return Sort(issues);
        }

        private static void ValidateSchemas(EnemySystemSnapshotInput input, ICollection<EnemySystemValidationIssue> issues)
        {
            Schema(issues, "E01", input.EnemyDomainSnapshot.SchemaId, input.EnemyDomainSnapshot.SchemaVersion,
                EnemyDomainSchema.SchemaId, EnemyDomainSchema.SchemaVersion);
            Schema(issues, "E02", input.EnemyMechanicVocabularySnapshot.SchemaId, input.EnemyMechanicVocabularySnapshot.SchemaVersion,
                EnemyMechanicVocabularySchema.SchemaId, EnemyMechanicVocabularySchema.SchemaVersion);
            Schema(issues, "E03", EnemyValidationNormalizationSchema.SchemaId, EnemyValidationNormalizationSchema.SchemaVersion,
                EnemyValidationNormalizationSchema.SchemaId, EnemyValidationNormalizationSchema.SchemaVersion);
            Schema(issues, "E04", input.EncounterCompositionCatalogSnapshot.SchemaId, input.EncounterCompositionCatalogSnapshot.SchemaVersion,
                EncounterCompositionSchema.SchemaId, EncounterCompositionSchema.SchemaVersion);
            Schema(issues, "E05", input.EnemySkillBossPhaseCatalogSnapshot.SchemaId, input.EnemySkillBossPhaseCatalogSnapshot.SchemaVersion,
                EnemySkillBossPhaseSchema.SchemaId, EnemySkillBossPhaseSchema.SchemaVersion);
            Schema(issues, "E06", input.CounterWindowAndPressureCatalogSnapshot.SchemaId, input.CounterWindowAndPressureCatalogSnapshot.SchemaVersion,
                CounterWindowAndPressureSchema.SchemaId, CounterWindowAndPressureSchema.SchemaVersion);
        }

        private static void ValidateSignatures(EnemySystemSnapshotInput input, ICollection<EnemySystemValidationIssue> issues)
        {
            Signature(issues, "E01", "CanonicalSignature", input.EnemyDomainSnapshot.BuildCanonicalSignature());
            Signature(issues, "E02", "CanonicalSignature", input.EnemyMechanicVocabularySnapshot.BuildCanonicalSignature());
            Signature(issues, "E03", "CanonicalSignature", input.EnemyValidationContentSnapshot.CanonicalSignature);
            Signature(issues, "E03", "PlayerSafeCanonicalSignature", input.EnemyValidationContentSnapshot.PlayerSafeCanonicalSignature);
            Signature(issues, "E04", "CanonicalSignature", input.EncounterCompositionCatalogSnapshot.CanonicalSignature);
            Signature(issues, "E05", "CanonicalSignature", input.EnemySkillBossPhaseCatalogSnapshot.CanonicalSignature);
            Signature(issues, "E05", "PlayerSafeCanonicalSignature", input.EnemySkillBossPhaseCatalogSnapshot.PlayerSafeCanonicalSignature);
            Signature(issues, "E06", "CanonicalSignature", input.CounterWindowAndPressureCatalogSnapshot.CanonicalSignature);
            Signature(issues, "E06", "PlayerSafeCanonicalSignature", input.CounterWindowAndPressureCatalogSnapshot.PlayerSafeCanonicalSignature);
        }

        private static void ValidateDomainAgreement(EnemySystemSnapshotInput input, ICollection<EnemySystemValidationIssue> issues)
        {
            string expected = input.EnemyDomainSnapshot.BuildCanonicalSignature();
            string actual = input.EnemyValidationContentSnapshot.DomainSnapshot == null
                ? string.Empty : input.EnemyValidationContentSnapshot.DomainSnapshot.BuildCanonicalSignature();
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                Add(issues, "E09_DOMAIN_SIGNATURE_MISMATCH", EnemySystemValidationCategory.Signature,
                    "E03", "DomainSnapshot", "E03 DomainSnapshot must equal the supplied E01 snapshot.");
            }
        }

        private static void ValidateManifestCore(IReadOnlyList<EnemySystemSchemaManifestEntry> candidate,
            IReadOnlyList<EnemySystemSchemaManifestEntry> expected, ICollection<EnemySystemValidationIssue> issues)
        {
            if (candidate == null || candidate.Count != 8)
            {
                Add(issues, "E09_MANIFEST_COUNT", EnemySystemValidationCategory.Schema, "E09", "SchemaManifest",
                    "Schema manifest must contain exactly eight entries.");
                if (candidate == null) return;
            }

            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (EnemySystemSchemaManifestEntry value in candidate)
            {
                if (value == null)
                {
                    Add(issues, "E09_MANIFEST_NULL_ENTRY", EnemySystemValidationCategory.Schema, "E09", "SchemaManifest",
                        "Schema manifest entries must not be null.");
                    continue;
                }
                if (!ids.Add(value.ComponentId))
                    Add(issues, "E09_MANIFEST_DUPLICATE", EnemySystemValidationCategory.Schema, value.ComponentId,
                        "SchemaManifest/" + value.ComponentId, "ComponentId must be unique.");
                EnemySystemSchemaManifestEntry match = expected.FirstOrDefault(item => item.ComponentId == value.ComponentId);
                if (match == null)
                {
                    Add(issues, "E09_MANIFEST_UNKNOWN_COMPONENT", EnemySystemValidationCategory.Schema, value.ComponentId,
                        "SchemaManifest/" + value.ComponentId, "Unexpected component contract.");
                    continue;
                }
                if (!ManifestEqual(value, match))
                    Add(issues, "E09_MANIFEST_CONTRACT_MISMATCH", EnemySystemValidationCategory.Schema, value.ComponentId,
                        "SchemaManifest/" + value.ComponentId, "Schema, mode, embedded flag, or signature does not match the authoritative contract.");
                if (!EnemySystemSnapshotCanonical.IsSignature(value.ContractCanonicalSignature))
                    Add(issues, "E09_MANIFEST_CONTRACT_SIGNATURE", EnemySystemValidationCategory.Signature, value.ComponentId,
                        "SchemaManifest/" + value.ComponentId + "/ContractCanonicalSignature", "Contract signature must use lowercase sha256 format.");
            }
            foreach (EnemySystemSchemaManifestEntry value in expected)
                if (!ids.Contains(value.ComponentId))
                    Add(issues, "E09_MANIFEST_MISSING_COMPONENT", EnemySystemValidationCategory.Schema, value.ComponentId,
                        "SchemaManifest/" + value.ComponentId, "Required component contract is missing.");
        }

        private static bool ManifestEqual(EnemySystemSchemaManifestEntry left, EnemySystemSchemaManifestEntry right)
        {
            return left.SchemaId == right.SchemaId && left.SchemaVersion == right.SchemaVersion
                && left.ComponentMode == right.ComponentMode && left.EmbeddedInRoot == right.EmbeddedInRoot
                && left.ComponentCanonicalSignature == right.ComponentCanonicalSignature
                && left.PlayerSafeCanonicalSignature == right.PlayerSafeCanonicalSignature
                && left.ContractCanonicalSignature == right.ContractCanonicalSignature;
        }

        private static void ValidateIdentities(IReadOnlyList<EnemySystemIdentitySnapshot> identities,
            ICollection<EnemySystemValidationIssue> issues)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> carriers = new HashSet<string>(StringComparer.Ordinal);
            foreach (EnemySystemIdentitySnapshot value in identities)
            {
                if (value.StableId.Length == 0 || value.StableId.Trim() != value.StableId)
                    Add(issues, "E09_ID_INVALID", EnemySystemValidationCategory.Identity, value.OwningComponentId,
                        value.IdentityKind + "/" + value.StableId, "Identity IDs must be non-empty and have no outer whitespace.");
                string key = value.IdentityKind + "\u001f" + value.StableId;
                if (!keys.Add(key))
                    Add(issues, "E09_ID_DUPLICATE", EnemySystemValidationCategory.Identity, value.OwningComponentId,
                        value.IdentityKind + "/" + value.StableId, "IdentityKind plus StableId must be unique.");
                if ((value.IdentityKind == EnemySystemIdentityKind.Enemy || value.IdentityKind == EnemySystemIdentityKind.Boss)
                    && !carriers.Add(value.StableId))
                    Add(issues, "E09_CARRIER_ID_CONFLICT", EnemySystemValidationCategory.Identity, value.OwningComponentId,
                        "Carrier/" + value.StableId, "Enemy and Boss share one carrier namespace.");
            }
        }

        private static void ValidateIsolation(EnemySystemSnapshotInput input,
            IReadOnlyList<EnemySystemIdentitySnapshot> identities, ICollection<EnemySystemValidationIssue> issues)
        {
            foreach (EnemySystemIdentitySnapshot value in identities)
                Isolation(issues, value.OwningComponentId, value.IdentityKind + "/" + value.StableId,
                    value.DevOnly, value.IsEnabled, value.EntersFormalFlow);

            foreach (CarrierSkillBindingSnapshot value in input.EnemySkillBossPhaseCatalogSnapshot.CarrierSkillBindings)
                Isolation(issues, "E05", "CarrierSkillBinding/" + value.CarrierId, value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
            foreach (PressureSourceBindingSnapshot value in input.CounterWindowAndPressureCatalogSnapshot.PressureSourceBindings)
                Isolation(issues, "E06", "PressureSource/" + value.SourceId, value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
            foreach (CounterWindowSourceBindingSnapshot value in input.CounterWindowAndPressureCatalogSnapshot.CounterWindowSourceBindings)
                Isolation(issues, "E06", "CounterWindowSource/" + value.SourceId, value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
            foreach (PressureCounterWindowBindingSnapshot value in input.CounterWindowAndPressureCatalogSnapshot.PressureCounterWindowBindings)
                Isolation(issues, "E06", "PressureWindow/" + value.BuildPressureProfileId + "/" + value.CounterWindowId,
                    value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
        }

        private static void ValidateRelations(IReadOnlyList<EnemySystemRelationSnapshot> relations,
            ICollection<EnemySystemValidationIssue> issues)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (EnemySystemRelationSnapshot value in relations)
            {
                string path = value.SourceKind + "/" + value.SourceId + "/" + value.RelationKind + "/"
                    + value.TargetKind + "/" + value.TargetId;
                string key = path;
                if (!keys.Add(key))
                    Add(issues, "E09_RELATION_DUPLICATE", EnemySystemValidationCategory.Relation,
                        value.OwningComponentId, path, "Relation identity must be unique.");
                if (!value.Resolved)
                    Add(issues, "E09_REFERENCE_UNRESOLVED", EnemySystemValidationCategory.Reference,
                        value.OwningComponentId, path, "Declared target does not resolve by ordinal identity and kind.");
                if (value.Resolved && !value.IsolationCompatible)
                    Add(issues, "E09_RELATION_ISOLATION_MISMATCH", EnemySystemValidationCategory.Isolation,
                        value.OwningComponentId, path, "Relation endpoints have incompatible isolation metadata.");
            }
        }

        private static void ValidatePlayerSafe(EnemySystemSnapshotInput input, ICollection<EnemySystemValidationIssue> issues)
        {
            EnemySystemPlayerSafeSnapshot player = new EnemySystemPlayerSafeSnapshot(input);
            if (!EnemySystemSnapshotCanonical.IsSignature(player.CanonicalSignature))
                Add(issues, "E09_PLAYER_SIGNATURE_FORMAT", EnemySystemValidationCategory.Signature, "E09",
                    "PlayerSafe/CanonicalSignature", "Player-safe root signature must use lowercase sha256 format.");

            string payload = player.BuildCanonicalPayload();
            foreach (string token in PlayerForbiddenTokens)
                if (payload.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    Add(issues, "E09_PLAYER_FORBIDDEN_TOKEN", EnemySystemValidationCategory.PlayerLeak, "E09",
                        "PlayerSafe/CanonicalPayload/" + token, "Player-safe canonical payload contains a forbidden field or answer token.");

            string[] expectedProperties =
            {
                "SchemaId", "SchemaVersion", "Enemies", "Bosses", "MechanicProfiles", "MapRules",
                "SkillPatterns", "BossPhases", "BuildPressureProfiles", "CounterWindows", "CanonicalSignature"
            };
            string[] actual = typeof(EnemySystemPlayerSafeSnapshot).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] expected = expectedProperties.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                Add(issues, "E09_PLAYER_PROPERTY_CLOSURE", EnemySystemValidationCategory.PlayerLeak, "E09",
                    "PlayerSafe/Properties", "Player-safe root properties exceed the explicit whitelist.");

            Type[] allowedProjectionTypes =
            {
                typeof(EnemyValidationPlayerProjection), typeof(SkillPatternPlayerProjection),
                typeof(BossPhasePlayerProjection), typeof(BuildPressurePlayerProjection),
                typeof(CounterWindowPlayerProjection)
            };
            foreach (PropertyInfo property in typeof(EnemySystemPlayerSafeSnapshot).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                Type type = property.PropertyType;
                if (type == typeof(string) || type == typeof(int)) continue;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                    && allowedProjectionTypes.Contains(type.GetGenericArguments()[0])) continue;
                Add(issues, "E09_PLAYER_TYPE_CLOSURE", EnemySystemValidationCategory.PlayerLeak, "E09",
                    "PlayerSafe/" + property.Name, "Player-safe root property type exceeds the projection whitelist.");
            }
        }

        private static void ValidateRootContract(ICollection<EnemySystemValidationIssue> issues)
        {
            PropertyInfo[] properties = typeof(EnemySystemSnapshot)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);
            string[] expectedProperties =
            {
                "SchemaId", "SchemaVersion", "EnemyDomainSnapshot", "EnemyMechanicVocabularySnapshot",
                "EnemyValidationContentSnapshot", "EncounterCompositionCatalogSnapshot",
                "EnemySkillBossPhaseCatalogSnapshot", "CounterWindowAndPressureCatalogSnapshot",
                "SchemaManifest", "IdentityIndex", "RelationIndex", "PlayerSafe", "CanonicalSignature",
                "PlayerSafeCanonicalSignature", "DevOnly", "IsEnabled", "EntersFormalFlow"
            };
            string[] actual = properties.Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] expected = expectedProperties.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            if (!actual.SequenceEqual(expected, StringComparer.Ordinal))
                Add(issues, "E09_ROOT_PROPERTY_CLOSURE", EnemySystemValidationCategory.Schema, "E09",
                    "Root/Properties", "Static root properties must exactly match the explicit 17-property whitelist.");

            foreach (PropertyInfo property in properties)
            {
                string fullName = property.PropertyType.FullName ?? property.PropertyType.Name;
                if (fullName.IndexOf("BuildCapabilitySnapshot", StringComparison.Ordinal) >= 0
                    || fullName.IndexOf("EnemyReadinessEvaluationResult", StringComparison.Ordinal) >= 0)
                    Add(issues, "E09_TRANSIENT_EMBEDDED", EnemySystemValidationCategory.Schema, "E09",
                        "Root/" + property.Name, "E07 and E08 transient objects must not be embedded in the static root.");
            }
        }

        private static void Schema(ICollection<EnemySystemValidationIssue> issues, string component,
            string actualId, int actualVersion, string expectedId, int expectedVersion)
        {
            if (actualId != expectedId || actualVersion != expectedVersion)
                Add(issues, "E09_SCHEMA_MISMATCH", EnemySystemValidationCategory.Schema, component, "Schema",
                    "Component schema id/version does not match its authoritative contract.");
        }

        private static void Signature(ICollection<EnemySystemValidationIssue> issues, string component,
            string path, string signature)
        {
            if (!EnemySystemSnapshotCanonical.IsSignature(signature))
                Add(issues, "E09_SIGNATURE_FORMAT", EnemySystemValidationCategory.Signature, component, path,
                    "Signature must use sha256 plus 64 lowercase hexadecimal characters.");
        }

        private static void Isolation(ICollection<EnemySystemValidationIssue> issues, string component,
            string path, bool devOnly, bool isEnabled, bool entersFormalFlow)
        {
            if (!devOnly || isEnabled || entersFormalFlow)
                Add(issues, "E09_ISOLATION_INVALID", EnemySystemValidationCategory.Isolation, component, path,
                    "Static enemy content must be devOnly=true, isEnabled=false, entersFormalFlow=false.");
        }

        private static void Require(ICollection<EnemySystemValidationIssue> issues, object value,
            string component, string path)
        {
            if (value == null)
                Add(issues, "E09_COMPONENT_NULL", EnemySystemValidationCategory.Schema, component, path,
                    "All six static snapshots are required.");
        }

        private static void Add(ICollection<EnemySystemValidationIssue> issues, string code,
            EnemySystemValidationCategory category, string component, string path, string message)
        {
            issues.Add(new EnemySystemValidationIssue(code, category, component, path, message));
        }

        private static IReadOnlyList<EnemySystemValidationIssue> Sort(IEnumerable<EnemySystemValidationIssue> issues)
        {
            return Array.AsReadOnly(issues.OrderBy(value => value.Category)
                .ThenBy(value => value.ComponentId, StringComparer.Ordinal)
                .ThenBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal).ToArray());
        }
    }
}
