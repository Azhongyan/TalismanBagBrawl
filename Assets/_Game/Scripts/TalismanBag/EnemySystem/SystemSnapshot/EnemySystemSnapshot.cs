using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.ReadinessEvaluation;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SystemSnapshot
{
    public static class EnemySystemSnapshotManifest
    {
        public static IReadOnlyList<EnemySystemSchemaManifestEntry> CreateExpected(
            EnemySystemSnapshotInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return Array.AsReadOnly(new[]
            {
                Entry("E01", EnemyDomainSchema.SchemaId, EnemyDomainSchema.SchemaVersion,
                    EnemySystemComponentMode.StaticEmbedded, true,
                    input.EnemyDomainSnapshot == null ? string.Empty : input.EnemyDomainSnapshot.BuildCanonicalSignature(), string.Empty),
                Entry("E02", EnemyMechanicVocabularySchema.SchemaId, EnemyMechanicVocabularySchema.SchemaVersion,
                    EnemySystemComponentMode.StaticEmbedded, true,
                    input.EnemyMechanicVocabularySnapshot == null ? string.Empty : input.EnemyMechanicVocabularySnapshot.BuildCanonicalSignature(), string.Empty),
                Entry("E03", EnemyValidationNormalizationSchema.SchemaId, EnemyValidationNormalizationSchema.SchemaVersion,
                    EnemySystemComponentMode.StaticEmbedded, true,
                    input.EnemyValidationContentSnapshot == null ? string.Empty : input.EnemyValidationContentSnapshot.CanonicalSignature,
                    input.EnemyValidationContentSnapshot == null ? string.Empty : input.EnemyValidationContentSnapshot.PlayerSafeCanonicalSignature),
                Entry("E04", EncounterCompositionSchema.SchemaId, EncounterCompositionSchema.SchemaVersion,
                    EnemySystemComponentMode.StaticEmbedded, true,
                    input.EncounterCompositionCatalogSnapshot == null ? string.Empty : input.EncounterCompositionCatalogSnapshot.CanonicalSignature, string.Empty),
                Entry("E05", EnemySkillBossPhaseSchema.SchemaId, EnemySkillBossPhaseSchema.SchemaVersion,
                    EnemySystemComponentMode.StaticEmbedded, true,
                    input.EnemySkillBossPhaseCatalogSnapshot == null ? string.Empty : input.EnemySkillBossPhaseCatalogSnapshot.CanonicalSignature,
                    input.EnemySkillBossPhaseCatalogSnapshot == null ? string.Empty : input.EnemySkillBossPhaseCatalogSnapshot.PlayerSafeCanonicalSignature),
                Entry("E06", CounterWindowAndPressureSchema.SchemaId, CounterWindowAndPressureSchema.SchemaVersion,
                    EnemySystemComponentMode.StaticEmbedded, true,
                    input.CounterWindowAndPressureCatalogSnapshot == null ? string.Empty : input.CounterWindowAndPressureCatalogSnapshot.CanonicalSignature,
                    input.CounterWindowAndPressureCatalogSnapshot == null ? string.Empty : input.CounterWindowAndPressureCatalogSnapshot.PlayerSafeCanonicalSignature),
                Entry("E07", BuildCapabilityReadSchema.SchemaId, BuildCapabilityReadSchema.SchemaVersion,
                    EnemySystemComponentMode.TransientContract, false, string.Empty, string.Empty),
                Entry("E08", EnemyOfflineReadinessSchema.SchemaId, EnemyOfflineReadinessSchema.SchemaVersion,
                    EnemySystemComponentMode.TransientContract, false, string.Empty, string.Empty)
            }.OrderBy(value => value.ComponentId, StringComparer.Ordinal).ToArray());
        }

        private static EnemySystemSchemaManifestEntry Entry(
            string componentId,
            string schemaId,
            int schemaVersion,
            EnemySystemComponentMode mode,
            bool embedded,
            string componentSignature,
            string playerSignature)
        {
            return new EnemySystemSchemaManifestEntry(componentId, schemaId, schemaVersion, mode,
                embedded, componentSignature, playerSignature,
                EnemySystemSnapshotCanonical.ManifestContract(schemaId, schemaVersion, mode, embedded));
        }
    }

    public sealed class EnemySystemSnapshot
    {
        private readonly ReadOnlyCollection<EnemySystemSchemaManifestEntry> schemaManifest;
        private readonly ReadOnlyCollection<EnemySystemIdentitySnapshot> identityIndex;
        private readonly ReadOnlyCollection<EnemySystemRelationSnapshot> relationIndex;

        internal EnemySystemSnapshot(EnemySystemSnapshotInput input)
        {
            SchemaId = EnemySystemSnapshotSchema.SchemaId;
            SchemaVersion = EnemySystemSnapshotSchema.SchemaVersion;
            EnemyDomainSnapshot = input.EnemyDomainSnapshot;
            EnemyMechanicVocabularySnapshot = input.EnemyMechanicVocabularySnapshot;
            EnemyValidationContentSnapshot = input.EnemyValidationContentSnapshot;
            EncounterCompositionCatalogSnapshot = input.EncounterCompositionCatalogSnapshot;
            EnemySkillBossPhaseCatalogSnapshot = input.EnemySkillBossPhaseCatalogSnapshot;
            CounterWindowAndPressureCatalogSnapshot = input.CounterWindowAndPressureCatalogSnapshot;
            schemaManifest = Array.AsReadOnly(EnemySystemSnapshotManifest.CreateExpected(input).ToArray());
            identityIndex = Array.AsReadOnly(EnemySystemSnapshotBuilder.CreateIdentities(input).ToArray());
            relationIndex = Array.AsReadOnly(EnemySystemSnapshotBuilder.CreateRelations(input, identityIndex).ToArray());
            PlayerSafe = new EnemySystemPlayerSafeSnapshot(input);
            PlayerSafeCanonicalSignature = PlayerSafe.CanonicalSignature;
            CanonicalSignature = EnemySystemSnapshotCanonical.Hash(BuildCanonicalPayload());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public EnemyDomainSnapshot EnemyDomainSnapshot { get; }
        public EnemyMechanicVocabularySnapshot EnemyMechanicVocabularySnapshot { get; }
        public EnemyValidationContentSnapshot EnemyValidationContentSnapshot { get; }
        public EncounterCompositionCatalogSnapshot EncounterCompositionCatalogSnapshot { get; }
        public EnemySkillBossPhaseCatalogSnapshot EnemySkillBossPhaseCatalogSnapshot { get; }
        public CounterWindowAndPressureCatalogSnapshot CounterWindowAndPressureCatalogSnapshot { get; }
        public IReadOnlyList<EnemySystemSchemaManifestEntry> SchemaManifest => schemaManifest;
        public IReadOnlyList<EnemySystemIdentitySnapshot> IdentityIndex => identityIndex;
        public IReadOnlyList<EnemySystemRelationSnapshot> RelationIndex => relationIndex;
        public EnemySystemPlayerSafeSnapshot PlayerSafe { get; }
        public string CanonicalSignature { get; }
        public string PlayerSafeCanonicalSignature { get; }
        public bool DevOnly => true;
        public bool IsEnabled => false;
        public bool EntersFormalFlow => false;

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder(32768);
            EnemySystemSnapshotCanonical.Field(builder, "schemaId", SchemaId);
            EnemySystemSnapshotCanonical.Field(builder, "schemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
            EnemySystemSnapshotCanonical.Field(builder, "isolation", EnemySystemSnapshotCanonical.Isolation(DevOnly, IsEnabled, EntersFormalFlow));
            EnemySystemSnapshotCanonical.Rows(builder, "schemaManifest", schemaManifest.Select(ManifestRow));
            EnemySystemSnapshotCanonical.Rows(builder, "identities", identityIndex.Select(IdentityRow));
            EnemySystemSnapshotCanonical.Rows(builder, "relations", relationIndex.Select(RelationRow));
            EnemySystemSnapshotCanonical.Field(builder, "playerSafeCanonicalSignature", PlayerSafeCanonicalSignature);
            return builder.ToString();
        }

        private static string ManifestRow(EnemySystemSchemaManifestEntry value)
        {
            return value.ComponentId + "|" + value.SchemaId + "|"
                + value.SchemaVersion.ToString(CultureInfo.InvariantCulture) + "|"
                + ((int)value.ComponentMode).ToString(CultureInfo.InvariantCulture) + "|"
                + (value.EmbeddedInRoot ? "1" : "0") + "|"
                + value.ComponentCanonicalSignature + "|" + value.PlayerSafeCanonicalSignature
                + "|" + value.ContractCanonicalSignature;
        }

        private static string IdentityRow(EnemySystemIdentitySnapshot value)
        {
            return ((int)value.IdentityKind).ToString(CultureInfo.InvariantCulture) + "|"
                + value.StableId + "|" + value.OwningComponentId + "|"
                + EnemySystemSnapshotCanonical.Isolation(value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
        }

        private static string RelationRow(EnemySystemRelationSnapshot value)
        {
            return ((int)value.SourceKind).ToString(CultureInfo.InvariantCulture) + "|" + value.SourceId + "|"
                + ((int)value.RelationKind).ToString(CultureInfo.InvariantCulture) + "|"
                + ((int)value.TargetKind).ToString(CultureInfo.InvariantCulture) + "|" + value.TargetId + "|"
                + value.OwningComponentId + "|" + (value.Resolved ? "1" : "0") + "|"
                + (value.IsolationCompatible ? "1" : "0");
        }
    }

    internal static class EnemySystemSnapshotBuilder
    {
        public static IReadOnlyList<EnemySystemIdentitySnapshot> CreateIdentities(EnemySystemSnapshotInput input)
        {
            List<EnemySystemIdentitySnapshot> result = new List<EnemySystemIdentitySnapshot>();
            foreach (EnemyArchetypeSnapshot value in input.EnemyDomainSnapshot.Enemies)
                AddIdentity(result, EnemySystemIdentityKind.Enemy, value.StableId, "E01", value);
            foreach (BossArchetypeSnapshot value in input.EnemyDomainSnapshot.Bosses)
                AddIdentity(result, EnemySystemIdentityKind.Boss, value.StableId, "E01", value);
            foreach (NormalizedMechanicProfileSnapshot value in input.EnemyValidationContentSnapshot.MechanicProfiles)
                AddIdentity(result, EnemySystemIdentityKind.MechanicProfile, value.Domain.StableId, "E03", value.Domain);
            foreach (NormalizedMapRuleSnapshot value in input.EnemyValidationContentSnapshot.MapRules)
                AddIdentity(result, EnemySystemIdentityKind.MapRule, value.Domain.StableId, "E03", value.Domain);
            foreach (EncounterCompositionSnapshot value in input.EncounterCompositionCatalogSnapshot.Encounters)
                AddIdentity(result, EnemySystemIdentityKind.Encounter, value.EncounterId, "E04", value.EncounterReference);
            foreach (EnemySkillPatternSnapshot value in input.EnemySkillBossPhaseCatalogSnapshot.SkillPatterns)
                AddIdentity(result, EnemySystemIdentityKind.SkillPattern, value.SkillPatternId, "E05", value.SkillPatternReference);
            foreach (SkillSequenceSnapshot value in input.EnemySkillBossPhaseCatalogSnapshot.SkillSequences)
                AddIdentity(result, EnemySystemIdentityKind.SkillSequence, value.SkillSequenceId, "E05", value);
            foreach (BossPhaseProfileSnapshot value in input.EnemySkillBossPhaseCatalogSnapshot.BossPhaseProfiles)
                AddIdentity(result, EnemySystemIdentityKind.BossPhase, value.BossPhaseId, "E05", value.BossPhaseReference);
            foreach (BossPhasePlanSnapshot value in input.EnemySkillBossPhaseCatalogSnapshot.BossPhasePlans)
                AddIdentity(result, EnemySystemIdentityKind.BossPhasePlan, value.BossId, "E05", value);
            foreach (BuildPressureProfileSnapshot value in input.CounterWindowAndPressureCatalogSnapshot.BuildPressureProfiles)
                AddIdentity(result, EnemySystemIdentityKind.BuildPressureProfile, value.BuildPressureProfileId, "E06", value);
            foreach (CounterWindowProfileSnapshot value in input.CounterWindowAndPressureCatalogSnapshot.CounterWindowProfiles)
                AddIdentity(result, EnemySystemIdentityKind.CounterWindow, value.CounterWindowId, "E06", value.CounterWindowReference);
            return Array.AsReadOnly(result.OrderBy(value => value.IdentityKind)
                .ThenBy(value => value.StableId, StringComparer.Ordinal).ToArray());
        }

        public static IReadOnlyList<EnemySystemRelationSnapshot> CreateRelations(
            EnemySystemSnapshotInput input,
            IReadOnlyList<EnemySystemIdentitySnapshot> identities)
        {
            List<EnemySystemRelationSnapshot> result = new List<EnemySystemRelationSnapshot>();
            Dictionary<string, EnemySystemIdentitySnapshot> lookup = identities
                .GroupBy(IdentityKey, StringComparer.Ordinal)
                .ToDictionary(value => value.Key, value => value.First(), StringComparer.Ordinal);

            foreach (EnemyArchetypeSnapshot carrier in input.EnemyDomainSnapshot.Enemies)
            {
                AddCarrierRelations(result, lookup, EnemySystemIdentityKind.Enemy, carrier.StableId,
                    carrier.MechanicProfileIds, carrier.SkillPatternIds, Array.Empty<string>(), "E01", carrier);
            }
            foreach (BossArchetypeSnapshot carrier in input.EnemyDomainSnapshot.Bosses)
            {
                AddCarrierRelations(result, lookup, EnemySystemIdentityKind.Boss, carrier.StableId,
                    carrier.MechanicProfileIds, carrier.SkillPatternIds, carrier.PhaseProfileIds, "E01", carrier);
            }

            AddOptionalRegistry(input, result, lookup);

            foreach (CarrierMechanicBindingSnapshot binding in input.EnemyValidationContentSnapshot.CarrierMechanicBindings)
            {
                EnemySystemIdentityKind sourceKind = binding.Kind == ValidationCarrierKind.Enemy
                    ? EnemySystemIdentityKind.Enemy : EnemySystemIdentityKind.Boss;
                EnemySystemIdentitySnapshot source = Find(lookup, sourceKind, binding.CarrierId);
                AddRelation(result, lookup, sourceKind, binding.CarrierId,
                    EnemySystemRelationKind.NormalizedCarrierMechanicProfile, EnemySystemIdentityKind.MechanicProfile,
                    binding.MechanicProfileId, "E03", source);
            }
            foreach (MapRuleMechanicBindingSnapshot binding in input.EnemyValidationContentSnapshot.MapRuleMechanicBindings)
            {
                EnemySystemIdentitySnapshot source = Find(lookup, EnemySystemIdentityKind.MapRule, binding.MapRuleId);
                AddRelation(result, lookup, EnemySystemIdentityKind.MapRule, binding.MapRuleId,
                    EnemySystemRelationKind.MapRuleMechanicProfile, EnemySystemIdentityKind.MechanicProfile,
                    binding.MechanicProfileId, "E03", source);
            }

            foreach (EncounterCompositionSnapshot encounter in input.EncounterCompositionCatalogSnapshot.Encounters)
            {
                EnemySystemIdentitySnapshot source = Find(lookup, EnemySystemIdentityKind.Encounter, encounter.EncounterId);
                foreach (string mapRuleId in encounter.MapRuleIds)
                    AddRelation(result, lookup, EnemySystemIdentityKind.Encounter, encounter.EncounterId,
                        EnemySystemRelationKind.EncounterMapRule, EnemySystemIdentityKind.MapRule, mapRuleId, "E04", source);
                foreach (EncounterWaveSnapshot wave in encounter.Waves)
                foreach (EncounterSlotSnapshot slot in wave.Slots)
                {
                    string slotId = encounter.EncounterId + "/" + wave.WaveId + "/" + slot.SlotId;
                    EnemySystemIdentityKind carrierKind = slot.Kind == EncounterSlotKind.Enemy
                        ? EnemySystemIdentityKind.Enemy : EnemySystemIdentityKind.Boss;
                    AddRelation(result, lookup, EnemySystemIdentityKind.EncounterSlot, slotId,
                        EnemySystemRelationKind.EncounterSlotCarrier, carrierKind, slot.CarrierId, "E04", source);
                    foreach (string profileId in slot.MechanicProfileIds)
                        AddRelation(result, lookup, EnemySystemIdentityKind.EncounterSlot, slotId,
                            EnemySystemRelationKind.EncounterSlotMechanicProfile,
                            EnemySystemIdentityKind.MechanicProfile, profileId, "E04", source);
                }
            }

            foreach (EnemySkillPatternSnapshot pattern in input.EnemySkillBossPhaseCatalogSnapshot.SkillPatterns)
            {
                EnemySystemIdentitySnapshot source = Find(lookup, EnemySystemIdentityKind.SkillPattern, pattern.SkillPatternId);
                foreach (string profileId in pattern.InternalOnly.MechanicProfileIds)
                    AddRelation(result, lookup, EnemySystemIdentityKind.SkillPattern, pattern.SkillPatternId,
                        EnemySystemRelationKind.SkillPatternMechanicProfile,
                        EnemySystemIdentityKind.MechanicProfile, profileId, "E05", source);
            }
            foreach (SkillSequenceSnapshot sequence in input.EnemySkillBossPhaseCatalogSnapshot.SkillSequences)
            {
                EnemySystemIdentitySnapshot source = Find(lookup, EnemySystemIdentityKind.SkillSequence, sequence.SkillSequenceId);
                foreach (SkillSequenceStepSnapshot step in sequence.Steps)
                    AddRelation(result, lookup, EnemySystemIdentityKind.SkillSequence, sequence.SkillSequenceId,
                        EnemySystemRelationKind.SkillSequencePattern, EnemySystemIdentityKind.SkillPattern,
                        step.SkillPatternId, "E05", source);
            }
            foreach (CarrierSkillBindingSnapshot binding in input.EnemySkillBossPhaseCatalogSnapshot.CarrierSkillBindings)
            {
                EnemySystemIdentityKind carrierKind = binding.CarrierKind == EnemySkillCarrierKind.Enemy
                    ? EnemySystemIdentityKind.Enemy : EnemySystemIdentityKind.Boss;
                EnemySystemIdentitySnapshot source = Find(lookup, carrierKind, binding.CarrierId);
                foreach (string sequenceId in binding.SkillSequenceIds)
                    AddRelation(result, lookup, EnemySystemIdentityKind.CarrierSkillBinding,
                        carrierKind + ":" + binding.CarrierId,
                        EnemySystemRelationKind.CarrierSkillSequence, EnemySystemIdentityKind.SkillSequence,
                        sequenceId, "E05", source);
            }
            foreach (BossPhaseProfileSnapshot phase in input.EnemySkillBossPhaseCatalogSnapshot.BossPhaseProfiles)
            {
                EnemySystemIdentitySnapshot source = Find(lookup, EnemySystemIdentityKind.BossPhase, phase.BossPhaseId);
                foreach (string sequenceId in phase.InternalOnly.SkillSequenceIds)
                    AddRelation(result, lookup, EnemySystemIdentityKind.BossPhase, phase.BossPhaseId,
                        EnemySystemRelationKind.BossPhaseSequence, EnemySystemIdentityKind.SkillSequence,
                        sequenceId, "E05", source);
            }
            foreach (BossPhasePlanSnapshot plan in input.EnemySkillBossPhaseCatalogSnapshot.BossPhasePlans)
            {
                EnemySystemIdentitySnapshot source = Find(lookup, EnemySystemIdentityKind.BossPhasePlan, plan.BossId);
                foreach (BossPhasePlanEntrySnapshot entry in plan.Entries)
                    AddRelation(result, lookup, EnemySystemIdentityKind.BossPhasePlan, plan.BossId,
                        EnemySystemRelationKind.BossPlanPhase, EnemySystemIdentityKind.BossPhase,
                        entry.BossPhaseId, "E05", source);
            }

            foreach (PressureSourceBindingSnapshot binding in input.CounterWindowAndPressureCatalogSnapshot.PressureSourceBindings)
            {
                EnemySystemIdentityKind targetKind = SourceKind(binding.SourceKind);
                string sourceId = binding.BuildPressureProfileId + "|" + binding.SourceKind + "|" + binding.SourceId;
                AddRelation(result, lookup, EnemySystemIdentityKind.PressureSource, sourceId,
                    EnemySystemRelationKind.PressureSourceTarget, targetKind, binding.SourceId, "E06", binding);
            }
            foreach (CounterWindowSourceBindingSnapshot binding in input.CounterWindowAndPressureCatalogSnapshot.CounterWindowSourceBindings)
            {
                EnemySystemIdentityKind targetKind = SourceKind(binding.SourceKind);
                string sourceId = binding.CounterWindowId + "|" + binding.SourceKind + "|" + binding.SourceId;
                AddRelation(result, lookup, EnemySystemIdentityKind.CounterWindowSource, sourceId,
                    EnemySystemRelationKind.CounterWindowSourceTarget, targetKind, binding.SourceId, "E06", binding);
            }
            foreach (PressureCounterWindowBindingSnapshot binding in input.CounterWindowAndPressureCatalogSnapshot.PressureCounterWindowBindings)
            {
                EnemySystemIdentitySnapshot source = Find(lookup, EnemySystemIdentityKind.BuildPressureProfile, binding.BuildPressureProfileId);
                AddRelation(result, lookup, EnemySystemIdentityKind.BuildPressureProfile, binding.BuildPressureProfileId,
                    EnemySystemRelationKind.PressureCounterWindow, EnemySystemIdentityKind.CounterWindow,
                    binding.CounterWindowId, "E06", source);
            }

            return Array.AsReadOnly(result.OrderBy(value => value.SourceKind)
                .ThenBy(value => value.SourceId, StringComparer.Ordinal)
                .ThenBy(value => value.RelationKind)
                .ThenBy(value => value.TargetKind)
                .ThenBy(value => value.TargetId, StringComparer.Ordinal)
                .ThenBy(value => value.OwningComponentId, StringComparer.Ordinal).ToArray());
        }

        private static void AddCarrierRelations(List<EnemySystemRelationSnapshot> result,
            IReadOnlyDictionary<string, EnemySystemIdentitySnapshot> lookup,
            EnemySystemIdentityKind sourceKind, string sourceId,
            IEnumerable<string> mechanics, IEnumerable<string> skills, IEnumerable<string> phases,
            string owner, IEnemyDomainIsolationMetadata source)
        {
            foreach (string id in mechanics)
                AddRelation(result, lookup, sourceKind, sourceId, EnemySystemRelationKind.CarrierMechanicProfile,
                    EnemySystemIdentityKind.MechanicProfile, id, owner, source);
            foreach (string id in skills)
                AddRelation(result, lookup, sourceKind, sourceId, EnemySystemRelationKind.CarrierSkillPattern,
                    EnemySystemIdentityKind.SkillPattern, id, owner, source);
            foreach (string id in phases)
                AddRelation(result, lookup, sourceKind, sourceId, EnemySystemRelationKind.BossPhaseReference,
                    EnemySystemIdentityKind.BossPhase, id, owner, source);
        }

        private static void AddOptionalRegistry(EnemySystemSnapshotInput input,
            List<EnemySystemRelationSnapshot> result,
            IReadOnlyDictionary<string, EnemySystemIdentitySnapshot> lookup)
        {
            foreach (MechanicProfileReference value in input.EnemyDomainSnapshot.MechanicProfiles)
                AddOptional(result, lookup, "MechanicProfile", EnemySystemIdentityKind.MechanicProfile, value, "E01");
            foreach (SkillPatternReference value in input.EnemyDomainSnapshot.SkillPatterns)
                AddOptional(result, lookup, "SkillPattern", EnemySystemIdentityKind.SkillPattern, value, "E01");
            foreach (BossPhaseReference value in input.EnemyDomainSnapshot.BossPhases)
                AddOptional(result, lookup, "BossPhase", EnemySystemIdentityKind.BossPhase, value, "E01");
            foreach (MapRuleReference value in input.EnemyDomainSnapshot.MapRules)
                AddOptional(result, lookup, "MapRule", EnemySystemIdentityKind.MapRule, value, "E01");
            foreach (EncounterReference value in input.EnemyDomainSnapshot.Encounters)
                AddOptional(result, lookup, "Encounter", EnemySystemIdentityKind.Encounter, value, "E01");
            // E01 CounterWindow references are the E02 vocabulary registry projection, not E06 instance references.
            // Only concrete E06 CounterWindowProfileSnapshot IDs participate in the E09 identity/relation graph.
        }

        private static void AddOptional(List<EnemySystemRelationSnapshot> result,
            IReadOnlyDictionary<string, EnemySystemIdentitySnapshot> lookup, string prefix,
            EnemySystemIdentityKind targetKind, EnemyDomainReference reference, string owner)
        {
            AddRelation(result, lookup, EnemySystemIdentityKind.OptionalRegistry,
                prefix + ":" + reference.StableId, EnemySystemRelationKind.OptionalRegistryReference,
                targetKind, reference.StableId, owner, reference);
        }

        private static void AddRelation(List<EnemySystemRelationSnapshot> result,
            IReadOnlyDictionary<string, EnemySystemIdentitySnapshot> lookup,
            EnemySystemIdentityKind sourceKind, string sourceId, EnemySystemRelationKind relationKind,
            EnemySystemIdentityKind targetKind, string targetId, string owner,
            IEnemyDomainIsolationMetadata source)
        {
            EnemySystemIdentitySnapshot target = Find(lookup, targetKind, targetId);
            bool compatible = target != null && source != null
                && source.DevOnly == target.DevOnly
                && source.IsEnabled == target.IsEnabled
                && source.EntersFormalFlow == target.EntersFormalFlow;
            result.Add(new EnemySystemRelationSnapshot(sourceKind, sourceId, relationKind, targetKind,
                targetId, owner, target != null, compatible));
        }

        private static EnemySystemIdentityKind SourceKind(PressureSourceKind kind)
        {
            switch (kind)
            {
                case PressureSourceKind.MechanicProfile: return EnemySystemIdentityKind.MechanicProfile;
                case PressureSourceKind.MapRule: return EnemySystemIdentityKind.MapRule;
                case PressureSourceKind.SkillPattern: return EnemySystemIdentityKind.SkillPattern;
                case PressureSourceKind.BossPhase: return EnemySystemIdentityKind.BossPhase;
                default: return EnemySystemIdentityKind.OptionalRegistry;
            }
        }

        private static void AddIdentity(List<EnemySystemIdentitySnapshot> result,
            EnemySystemIdentityKind kind, string id, string owner, IEnemyDomainIsolationMetadata isolation)
        {
            result.Add(new EnemySystemIdentitySnapshot(kind, id, owner,
                isolation != null && isolation.DevOnly,
                isolation != null && isolation.IsEnabled,
                isolation != null && isolation.EntersFormalFlow));
        }

        private static EnemySystemIdentitySnapshot Find(
            IReadOnlyDictionary<string, EnemySystemIdentitySnapshot> lookup,
            EnemySystemIdentityKind kind,
            string id)
        {
            lookup.TryGetValue(IdentityKey(kind, id), out EnemySystemIdentitySnapshot value);
            return value;
        }

        private static string IdentityKey(EnemySystemIdentitySnapshot value)
        {
            return IdentityKey(value.IdentityKind, value.StableId);
        }

        private static string IdentityKey(EnemySystemIdentityKind kind, string id)
        {
            return ((int)kind).ToString(CultureInfo.InvariantCulture) + "\u001f" + (id ?? string.Empty);
        }
    }
}
