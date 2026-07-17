using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.Normalization
{
    public interface IEnemyValidationContentNormalizer
    {
        EnemyValidationContentSnapshot Normalize(EnemyValidationNormalizationInput input, IEnemyMechanicVocabularyLookup vocabulary);
    }

    public sealed class EnemyValidationNormalizationException : ArgumentException
    {
        public EnemyValidationNormalizationException(string message) : base(message) { }
    }

    public sealed class DefaultEnemyValidationContentNormalizer : IEnemyValidationContentNormalizer
    {
        public static readonly DefaultEnemyValidationContentNormalizer Instance = new DefaultEnemyValidationContentNormalizer();

        private static readonly IReadOnlyDictionary<string, string> HintByMechanic = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mechanic.basic_pressure"] = "player_hint.basic_pressure",
            ["mechanic.layered_shield"] = "player_hint.shield_pressure",
            ["mechanic.swarm_summon"] = "player_hint.multi_target_pressure",
            ["mechanic.poison"] = "player_hint.status_pressure",
            ["mechanic.burning"] = "player_hint.status_pressure",
            ["mechanic.energy_drain"] = "player_hint.energy_disrupted",
            ["mechanic.talisman_seal"] = "player_hint.talisman_sealed",
            ["mechanic.burst_spike"] = "player_hint.burst_incoming",
            ["mechanic.long_cast"] = "player_hint.cast_interrupt_opportunity",
            ["mechanic.high_health_endurance"] = "player_hint.sustained_pressure",
            ["mechanic.polluted_tile"] = "player_hint.formation_disrupted",
            ["mechanic.formation_eye_disruption"] = "player_hint.formation_disrupted"
        };

        public EnemyValidationContentSnapshot Normalize(EnemyValidationNormalizationInput input, IEnemyMechanicVocabularyLookup vocabulary)
        {
            if (input == null) throw new EnemyValidationNormalizationException("Normalization input is required.");
            if (vocabulary == null) throw new EnemyValidationNormalizationException("EnemyMechanicVocabulary.v1 lookup is required.");
            ValidateInput(input);

            List<NormalizedEnemyCarrierSnapshot> enemies = new List<NormalizedEnemyCarrierSnapshot>();
            List<NormalizedBossCarrierSnapshot> bosses = new List<NormalizedBossCarrierSnapshot>();
            foreach (CarrierNormalizationSource source in input.Carriers.OrderBy(x => x.Id, StringComparer.Ordinal))
            {
                Resolution resolution = Resolve(source.Tokens, vocabulary);
                EnemyValidationPlayerProjection player = Player(source.Id, source.DisplayName, resolution);
                EnemyValidationDeveloperProjection developer = Developer("EnemyBossValidationPool", source.Id,
                    Array.Empty<string>(), Array.Empty<string>(), source.LegacyFacts, Array.Empty<string>(), resolution.Trace);
                if (source.Kind == ValidationCarrierKind.Enemy)
                {
                    enemies.Add(new NormalizedEnemyCarrierSnapshot(
                        new EnemyArchetypeSnapshot(source.Id, source.DisplayName, EnemyArchetypeCategory.Normal,
                            "legacy.devonly.validation", source.LegacyType, devOnly: source.DevOnly,
                            isEnabled: source.IsEnabled, entersFormalFlow: source.EntersFormalFlow), player, developer));
                }
                else
                {
                    bosses.Add(new NormalizedBossCarrierSnapshot(
                        new BossArchetypeSnapshot(source.Id, source.DisplayName, "legacy.devonly.validation", source.LegacyType,
                            devOnly: source.DevOnly, isEnabled: source.IsEnabled, entersFormalFlow: source.EntersFormalFlow), player, developer));
                }
            }

            List<NormalizedMechanicProfileSnapshot> profiles = new List<NormalizedMechanicProfileSnapshot>();
            foreach (MechanicProfileNormalizationSource source in input.Profiles.OrderBy(x => x.Id, StringComparer.Ordinal))
            {
                Resolution publicResult = Resolve(source.PublicTokens, vocabulary);
                Resolution requiredResult = Resolve(source.RequiredTokens, vocabulary);
                Resolution optionalResult = Resolve(source.OptionalTokens, vocabulary);
                Resolution counterResult = Resolve(source.CounterTokens, vocabulary);
                publicResult.Merge(counterResult);
                List<LegacyMappingTrace> trace = publicResult.Trace.Concat(requiredResult.Trace).Concat(optionalResult.Trace).ToList();
                profiles.Add(new NormalizedMechanicProfileSnapshot(source.Kind,
                    new MechanicProfileReference(source.Id, source.DevOnly, source.IsEnabled, source.EntersFormalFlow),
                    Player(source.Id, source.DisplayName, publicResult),
                    Developer("BuildProblemSeedDataset", source.Id, requiredResult.Capabilities, optionalResult.Capabilities,
                        source.LegacyFacts, source.SourceReferences, trace)));
            }

            List<NormalizedMapRuleSnapshot> maps = new List<NormalizedMapRuleSnapshot>();
            List<MapRuleMechanicBindingSnapshot> mapBindings = new List<MapRuleMechanicBindingSnapshot>();
            foreach (MapRuleNormalizationSource source in input.MapRules.OrderBy(x => x.Id, StringComparer.Ordinal))
            {
                Resolution resolution = Resolve(source.PublicTokens, vocabulary);
                maps.Add(new NormalizedMapRuleSnapshot(new MapRuleReference(source.Id, source.DevOnly, source.IsEnabled, source.EntersFormalFlow),
                    Player(source.Id, source.DisplayName, resolution),
                    Developer("BuildProblemSeedDataset.MapRuleSeed", source.Id, Array.Empty<string>(), Array.Empty<string>(),
                        source.LegacyFacts, source.EnemyProfileIds.Concat(source.BossProfileIds), resolution.Trace)));
                mapBindings.AddRange(source.EnemyProfileIds.Select(id => new MapRuleMechanicBindingSnapshot(source.Id, ValidationProfileKind.Enemy, id, "Read-only MapRuleSeed.enemyProblemIds reference.")));
                mapBindings.AddRange(source.BossProfileIds.Select(id => new MapRuleMechanicBindingSnapshot(source.Id, ValidationProfileKind.Boss, id, "Read-only MapRuleSeed.bossProblemIds reference.")));
            }

            List<CarrierMechanicBindingSnapshot> carrierBindings = input.CarrierBindings
                .Select(x => new CarrierMechanicBindingSnapshot(x.Kind, x.CarrierId, x.ProfileId, x.Reason)).ToList();
            List<NormalizationExceptionSnapshot> exceptions = input.Exceptions
                .Select(x => new NormalizationExceptionSnapshot(x.Kind, x.SourceId, x.Status, x.Reason)).ToList();

            EnemyDomainSnapshot domain = DefaultEnemyDomainSnapshotProvider.Instance.CreateSnapshot(new EnemyDomainSnapshotInput(
                enemies: enemies.Select(x => x.Domain).ToArray(), bosses: bosses.Select(x => x.Domain).ToArray(),
                mechanicProfiles: profiles.Select(x => x.Domain).ToArray(), mapRules: maps.Select(x => x.Domain).ToArray(),
                counterWindows: profiles.SelectMany(x => x.PlayerSafe.CounterWindowTypeKeys).Distinct(StringComparer.Ordinal)
                    .Select(id => new CounterWindowReference(id)).ToArray()));

            return new EnemyValidationContentSnapshot(enemies, bosses, profiles, maps, carrierBindings, mapBindings, exceptions, domain);
        }

        private static void ValidateInput(EnemyValidationNormalizationInput input)
        {
            RequireUnique(input.Carriers.Select(x => x.Id), "carrier"); RequireUnique(input.Profiles.Select(x => x.Id), "mechanic profile"); RequireUnique(input.MapRules.Select(x => x.Id), "map rule");
            if (input.Carriers.Any(x => string.IsNullOrWhiteSpace(x.Id)) || input.Profiles.Any(x => string.IsNullOrWhiteSpace(x.Id)) || input.MapRules.Any(x => string.IsNullOrWhiteSpace(x.Id)))
                throw new EnemyValidationNormalizationException("Stable IDs must be non-empty.");
            if (input.Carriers.Any(x => !x.DevOnly || x.IsEnabled || x.EntersFormalFlow) || input.Profiles.Any(x => !x.DevOnly || x.IsEnabled || x.EntersFormalFlow) || input.MapRules.Any(x => !x.DevOnly || x.IsEnabled || x.EntersFormalFlow))
                throw new EnemyValidationNormalizationException("All E03 inputs must remain devOnly=true, isEnabled=false, entersFormalFlow=false.");
            HashSet<string> enemyIds = new HashSet<string>(input.Carriers.Where(x => x.Kind == ValidationCarrierKind.Enemy).Select(x => x.Id), StringComparer.Ordinal);
            HashSet<string> bossIds = new HashSet<string>(input.Carriers.Where(x => x.Kind == ValidationCarrierKind.Boss).Select(x => x.Id), StringComparer.Ordinal);
            Dictionary<string, ValidationProfileKind> profiles = input.Profiles.ToDictionary(x => x.Id, x => x.Kind, StringComparer.Ordinal);
            foreach (CarrierMechanicBindingSource binding in input.CarrierBindings)
            {
                bool carrierOk = binding.Kind == ValidationCarrierKind.Enemy ? enemyIds.Contains(binding.CarrierId) : bossIds.Contains(binding.CarrierId);
                ValidationProfileKind expected = binding.Kind == ValidationCarrierKind.Enemy ? ValidationProfileKind.Enemy : ValidationProfileKind.Boss;
                if (!carrierOk || !profiles.TryGetValue(binding.ProfileId, out ValidationProfileKind actual) || actual != expected)
                    throw new EnemyValidationNormalizationException("Unresolved carrier binding: " + binding.CarrierId + " -> " + binding.ProfileId);
            }
            foreach (MapRuleNormalizationSource map in input.MapRules)
            {
                if (map.EnemyProfileIds.Any(id => !profiles.TryGetValue(id, out ValidationProfileKind kind) || kind != ValidationProfileKind.Enemy)
                    || map.BossProfileIds.Any(id => !profiles.TryGetValue(id, out ValidationProfileKind kind) || kind != ValidationProfileKind.Boss))
                    throw new EnemyValidationNormalizationException("Unresolved MapRule binding in " + map.Id);
            }
        }

        private static void RequireUnique(IEnumerable<string> ids, string label)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in ids) if (!seen.Add(id)) throw new EnemyValidationNormalizationException("Duplicate ordinal " + label + " ID: " + id);
        }

        private static Resolution Resolve(IEnumerable<LegacyVocabularyToken> tokens, IEnemyMechanicVocabularyLookup vocabulary)
        {
            Resolution result = new Resolution();
            foreach (LegacyVocabularyToken token in tokens ?? Array.Empty<LegacyVocabularyToken>())
            {
                if (!vocabulary.TryGetLegacyMapping(token.SourceKind, token.LegacyKey, out LegacyEnemyVocabularyMappingSnapshot mapping))
                    throw new EnemyValidationNormalizationException("Unresolved legacy vocabulary key: " + token.SourceKind + " / " + token.LegacyKey);
                string status = mapping.Status == LegacyEnemyVocabularyMappingStatus.OutOfScope ? "OUT_OF_SCOPE" : "MAPPED";
                result.Trace.Add(new LegacyMappingTrace(mapping.LegacySourceKind, mapping.LegacyKey, status, mapping.Targets.Select(x => x.StableKey), mapping.Reason));
                foreach (EnemyVocabularyKeyReferenceSnapshot target in mapping.Targets)
                {
                    if (target.Category == EnemyVocabularyCategory.Mechanic) result.Mechanics.Add(target.StableKey);
                    else if (target.Category == EnemyVocabularyCategory.PressureChannel) result.Pressures.Add(target.StableKey);
                    else if (target.Category == EnemyVocabularyCategory.BuildCapability) result.Capabilities.Add(target.StableKey);
                    else if (target.Category == EnemyVocabularyCategory.CounterWindowType) result.CounterWindows.Add(target.StableKey);
                    else if (target.Category == EnemyVocabularyCategory.PlayerHintCategory) result.Hints.Add(target.StableKey);
                }
            }
            foreach (string mechanic in result.Mechanics) if (HintByMechanic.TryGetValue(mechanic, out string hint)) result.Hints.Add(hint);
            return result;
        }

        private static EnemyValidationPlayerProjection Player(string id, string name, Resolution r) => new EnemyValidationPlayerProjection(id, name, r.Mechanics, r.Pressures, r.Hints, r.CounterWindows);
        private static EnemyValidationDeveloperProjection Developer(string kind, string id, IEnumerable<string> required, IEnumerable<string> optional, IEnumerable<string> facts, IEnumerable<string> refs, IEnumerable<LegacyMappingTrace> trace) => new EnemyValidationDeveloperProjection(kind, id, required, optional, facts, refs, trace);

        private sealed class Resolution
        {
            public readonly HashSet<string> Mechanics = new HashSet<string>(StringComparer.Ordinal);
            public readonly HashSet<string> Pressures = new HashSet<string>(StringComparer.Ordinal);
            public readonly HashSet<string> Hints = new HashSet<string>(StringComparer.Ordinal);
            public readonly HashSet<string> CounterWindows = new HashSet<string>(StringComparer.Ordinal);
            public readonly HashSet<string> Capabilities = new HashSet<string>(StringComparer.Ordinal);
            public readonly List<LegacyMappingTrace> Trace = new List<LegacyMappingTrace>();
            public void Merge(Resolution other) { Mechanics.UnionWith(other.Mechanics); Pressures.UnionWith(other.Pressures); Hints.UnionWith(other.Hints); CounterWindows.UnionWith(other.CounterWindows); Capabilities.UnionWith(other.Capabilities); Trace.AddRange(other.Trace); }
        }
    }
}
