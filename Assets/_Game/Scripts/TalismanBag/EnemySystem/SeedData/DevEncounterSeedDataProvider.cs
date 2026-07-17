using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.SystemSnapshot;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SeedData
{
    public sealed class DefaultDevEncounterSeedDataProvider : IDevEncounterSeedDataProvider
    {
        public static readonly DefaultDevEncounterSeedDataProvider Instance =
            new DefaultDevEncounterSeedDataProvider(DefaultDevEncounterSeedDataValidator.Instance);

        private readonly IDevEncounterSeedDataValidator validator;

        public DefaultDevEncounterSeedDataProvider(IDevEncounterSeedDataValidator validator)
        {
            this.validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public DevEncounterSeedDataSnapshot CreateSnapshot(DevEncounterSeedDataInput input)
        {
            IReadOnlyList<DevEncounterSeedValidationIssue> inputIssues = validator.ValidateInput(input);
            if (inputIssues.Count > 0) throw new DevEncounterSeedValidationException(inputIssues);

            EnemyMechanicVocabularySnapshot vocabulary = input.EnemyMechanicVocabularySnapshot;
            EnemyValidationContentSnapshot normalization = input.EnemyValidationContentSnapshot;
            DevEncounterSeedCatalogContent content = DevEncounterSeedDataCatalog.CreateContent(normalization);
            DevEncounterSeedReferenceResolver resolver = new DevEncounterSeedReferenceResolver(normalization, vocabulary);

            EncounterCompositionCatalogSnapshot encounters = DefaultEncounterCompositionProvider.Instance
                .CreateSnapshot(content.EncounterInput, resolver);
            EnemySkillBossPhaseCatalogSnapshot skillPhase = DefaultEnemySkillBossPhaseProvider.Instance
                .CreateSnapshot(content.SkillPhaseInput, resolver);
            resolver.SkillPhase = skillPhase;
            CounterWindowAndPressureCatalogSnapshot pressureWindow = DefaultCounterWindowAndPressureProvider.Instance
                .CreateSnapshot(content.PressureWindowInput, resolver);
            EnemySystemSnapshot root = DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(
                new EnemySystemSnapshotInput(normalization.DomainSnapshot, vocabulary, normalization,
                    encounters, skillPhase, pressureWindow));

            DevEncounterSeedDataSnapshot snapshot = new DevEncounterSeedDataSnapshot(
                content.SeedProfiles, root, content.LegacyMappings);
            IReadOnlyList<DevEncounterSeedValidationIssue> issues = validator.ValidateSnapshot(snapshot);
            if (issues.Count > 0) throw new DevEncounterSeedValidationException(issues);
            return snapshot;
        }
    }

    internal sealed class DevEncounterSeedReferenceResolver :
        IEncounterCompositionReferenceResolver,
        IEnemySkillBossPhaseReferenceResolver,
        ICounterWindowAndPressureReferenceResolver
    {
        private readonly EnemyValidationContentSnapshot normalization;
        private readonly EnemyMechanicVocabularySnapshot vocabulary;
        private readonly HashSet<string> carrierBindings;

        public DevEncounterSeedReferenceResolver(EnemyValidationContentSnapshot normalization,
            EnemyMechanicVocabularySnapshot vocabulary)
        {
            this.normalization = normalization ?? throw new ArgumentNullException(nameof(normalization));
            this.vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
            carrierBindings = new HashSet<string>(normalization.CarrierMechanicBindings.Select(value =>
                value.Kind + "\u001f" + value.CarrierId + "\u001f" + value.MechanicProfileId),
                StringComparer.Ordinal);
        }

        public EnemySkillBossPhaseCatalogSnapshot SkillPhase { get; set; }

        public bool TryGetEnemy(string id, out EnemyArchetypeSnapshot value)
        {
            if (normalization.TryGetEnemy(id, out NormalizedEnemyCarrierSnapshot found))
            {
                value = found.Domain;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetBoss(string id, out BossArchetypeSnapshot value)
        {
            if (normalization.TryGetBoss(id, out NormalizedBossCarrierSnapshot found))
            {
                value = found.Domain;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetMapRule(string id, out MapRuleReference value)
        {
            if (normalization.TryGetMapRule(id, out NormalizedMapRuleSnapshot found))
            {
                value = found.Domain;
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetMechanicProfileKind(string id, out ValidationProfileKind kind)
        {
            if (normalization.TryGetMechanicProfile(id, out NormalizedMechanicProfileSnapshot found))
            {
                kind = found.Kind;
                return true;
            }
            kind = default(ValidationProfileKind);
            return false;
        }

        public bool HasCarrierMechanicBinding(EncounterSlotKind kind, string carrierId, string profileId)
        {
            ValidationCarrierKind mapped = kind == EncounterSlotKind.Enemy
                ? ValidationCarrierKind.Enemy : ValidationCarrierKind.Boss;
            return carrierBindings.Contains(mapped + "\u001f" + carrierId + "\u001f" + profileId);
        }

        public bool HasCarrierMechanicBinding(EnemySkillCarrierKind kind, string carrierId, string profileId)
        {
            ValidationCarrierKind mapped = kind == EnemySkillCarrierKind.Enemy
                ? ValidationCarrierKind.Enemy : ValidationCarrierKind.Boss;
            return carrierBindings.Contains(mapped + "\u001f" + carrierId + "\u001f" + profileId);
        }

        public bool IsIntentionalMechaniclessCarrier(EncounterSlotKind kind, string carrierId)
        {
            return normalization.Exceptions.Any(value => value.Kind == NormalizationExceptionKind.Carrier
                && string.Equals(value.SourceId, carrierId, StringComparison.Ordinal));
        }

        public bool HasVocabularyKey(EnemyVocabularyCategory category, string key)
        {
            return vocabulary.TryGetEntry(category, key, out _);
        }

        public bool HasMechanicProfile(string id)
        {
            return normalization.TryGetMechanicProfile(id, out _);
        }

        public bool HasMapRule(string id)
        {
            return normalization.TryGetMapRule(id, out _);
        }

        public bool HasSkillPattern(string id)
        {
            return SkillPhase != null && SkillPhase.TryGetSkillPatternById(id, out _);
        }

        public bool HasBossPhase(string id)
        {
            return SkillPhase != null && SkillPhase.TryGetBossPhaseById(id, out _);
        }

        public bool HasBuildCapabilityKey(string key)
        {
            return HasVocabularyKey(EnemyVocabularyCategory.BuildCapability, key);
        }
    }
}
