using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EditorTools.EnemySystem
{
    internal enum EnemyValidationContentVerificationMutation
    {
        None = 0,
        EnemyDeveloperOnly = 1,
        BossDeveloperOnly = 2,
        PlayerSafe = 3
    }

    // The only E03 adapter allowed to read legacy BuildSandbox content.
    public static class EnemyValidationContentNormalizer
    {
        private const string CarrierBindingReason = "Guard-approved explicit legacy carrier-to-mechanic-profile normalization binding.";

        public static EnemyValidationContentSnapshot CreateSnapshot(bool reverseInputOrder = false)
        {
            return CreateSnapshotCore(reverseInputOrder, EnemyValidationContentVerificationMutation.None);
        }

        internal static EnemyValidationContentSnapshot CreateSnapshotForVerification(EnemyValidationContentVerificationMutation mutation)
        {
            return CreateSnapshotCore(false, mutation);
        }

        private static EnemyValidationContentSnapshot CreateSnapshotCore(bool reverseInputOrder, EnemyValidationContentVerificationMutation mutation)
        {
            EnemyBossValidationPool carrierPool = EnemyBossValidationPool.CreateDefault();
            BuildProblemSeedDataset problemData = BuildProblemSeedDataset.CreateDefault();
            List<CarrierMechanicBindingSource> bindings = CreateCarrierBindings().ToList();
            List<CarrierNormalizationSource> carriers = CreateCarriers(carrierPool).ToList();
            List<MechanicProfileNormalizationSource> profiles = CreateProfiles(problemData, carriers, bindings).ToList();
            List<MapRuleNormalizationSource> maps = CreateMapRules(problemData).ToList();
            List<NormalizationExceptionSource> exceptions = new List<NormalizationExceptionSource>
            {
                new NormalizationExceptionSource(NormalizationExceptionKind.Carrier, "dev_enemy_basic", "BASELINE_ONLY", "Baseline comparison carrier intentionally has no MechanicValidationProfile binding."),
                new NormalizationExceptionSource(NormalizationExceptionKind.MechanicProfile, "dev_enemy_polluted_tile_problem", "MAP_RULE_ONLY", "Polluted-tile pressure is intentionally carried by MapRule data and has no dedicated carrier.")
            };
            ApplyVerificationMutation(carriers, mutation);
            if (reverseInputOrder) { carriers.Reverse(); profiles.Reverse(); maps.Reverse(); bindings.Reverse(); exceptions.Reverse(); }
            EnemyMechanicVocabularySnapshot vocabulary = DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(DefaultEnemyMechanicVocabularyCatalog.CreateInput(reverseInputOrder));
            return DefaultEnemyValidationContentNormalizer.Instance.Normalize(new EnemyValidationNormalizationInput(carriers, profiles, maps, bindings, exceptions), vocabulary);
        }

        private static void ApplyVerificationMutation(List<CarrierNormalizationSource> carriers, EnemyValidationContentVerificationMutation mutation)
        {
            if (mutation == EnemyValidationContentVerificationMutation.None) return;

            ValidationCarrierKind kind = mutation == EnemyValidationContentVerificationMutation.BossDeveloperOnly
                ? ValidationCarrierKind.Boss
                : ValidationCarrierKind.Enemy;
            string id = kind == ValidationCarrierKind.Boss ? "dev_boss_shield_jinglei" : "dev_enemy_basic";
            int index = carriers.FindIndex(x => x.Kind == kind && string.Equals(x.Id, id, StringComparison.Ordinal));
            if (index < 0) throw new InvalidOperationException("GuardFix01 verification carrier not found: " + id);

            CarrierNormalizationSource source = carriers[index];
            string displayName = mutation == EnemyValidationContentVerificationMutation.PlayerSafe
                ? source.DisplayName + " [GuardFix01 player-safe mutation]"
                : source.DisplayName;
            IEnumerable<string> legacyFacts = mutation == EnemyValidationContentVerificationMutation.EnemyDeveloperOnly
                ? source.LegacyFacts.Concat(new[] { "guardFix01.enemyDeveloperOnly=mutated" })
                : mutation == EnemyValidationContentVerificationMutation.BossDeveloperOnly
                    ? source.LegacyFacts.Concat(new[] { "guardFix01.bossDeveloperOnly=mutated" })
                    : source.LegacyFacts;

            carriers[index] = new CarrierNormalizationSource(source.Kind, source.Id, displayName, source.LegacyType,
                source.Tokens, legacyFacts, source.DevOnly, source.IsEnabled, source.EntersFormalFlow);
        }

        private static IEnumerable<CarrierNormalizationSource> CreateCarriers(EnemyBossValidationPool pool)
        {
            foreach (BuildSandboxEnemyProfile value in pool.enemies)
            {
                yield return new CarrierNormalizationSource(ValidationCarrierKind.Enemy, value.enemyId, value.chineseRole, value.enemyType,
                    One(DefaultEnemyMechanicVocabularyCatalog.EnemyTypeSource, value.enemyType)
                        .Concat(Tokens(DefaultEnemyMechanicVocabularyCatalog.ValidationTagsSource, value.validationTags)),
                    Facts("validationTargetBuilds", value.validationTargetBuilds, "recommendedSynergies", value.recommendedSynergies, "notes", new[] { value.notes }),
                    value.devOnly, value.isEnabled, value.entersFormalFlow);
            }
            foreach (BuildSandboxBossProfile value in pool.bosses)
            {
                yield return new CarrierNormalizationSource(ValidationCarrierKind.Boss, value.bossId, value.chineseRole, value.bossMechanic,
                    One(DefaultEnemyMechanicVocabularyCatalog.BossMechanicSource, value.bossMechanic)
                        .Concat(Tokens(DefaultEnemyMechanicVocabularyCatalog.ValidationTagsSource, value.validationTags)),
                    Facts("validationTargetBuilds", value.validationTargetBuilds, "recommendedSynergies", value.recommendedSynergies, "notes", new[] { value.notes }),
                    value.devOnly, value.isEnabled, value.entersFormalFlow);
            }
        }

        private static IEnumerable<MechanicProfileNormalizationSource> CreateProfiles(BuildProblemSeedDataset data,
            IReadOnlyList<CarrierNormalizationSource> carriers, IReadOnlyList<CarrierMechanicBindingSource> bindings)
        {
            Dictionary<string, CarrierNormalizationSource> carrierById = carriers.ToDictionary(x => x.Id, StringComparer.Ordinal);
            Dictionary<string, List<LegacyVocabularyToken>> carrierTokensByProfile = new Dictionary<string, List<LegacyVocabularyToken>>(StringComparer.Ordinal);
            foreach (CarrierMechanicBindingSource binding in bindings)
            {
                if (!carrierTokensByProfile.TryGetValue(binding.ProfileId, out List<LegacyVocabularyToken> tokens)) carrierTokensByProfile[binding.ProfileId] = tokens = new List<LegacyVocabularyToken>();
                tokens.AddRange(carrierById[binding.CarrierId].Tokens);
            }

            foreach (EnemyProblemSeed value in data.enemyProblems)
            {
                yield return new MechanicProfileNormalizationSource(ValidationProfileKind.Enemy, value.problemType, value.displayName,
                    One(DefaultEnemyMechanicVocabularyCatalog.MechanicTypeSource, value.pressureType),
                    Tokens(DefaultEnemyMechanicVocabularyCatalog.RequiredCapabilitySource, value.hardSolutionTags),
                    Tokens(DefaultEnemyMechanicVocabularyCatalog.OptionalCapabilitySource, value.softSolutionTags), Array.Empty<LegacyVocabularyToken>(),
                    Facts("validatedBuildTags", value.validatedBuildTags, "recommendedAction", new[] { value.recommendedAction }, "failureHintId", new[] { value.failureHint.failureHintId }),
                    new[] { value.failureHint.failureHintId }, value.devOnly, value.isEnabled, value.entersFormalFlow);
            }
            foreach (BossProblemSeed value in data.bossProblems)
            {
                List<LegacyVocabularyToken> publicTokens = carrierTokensByProfile.TryGetValue(value.bossProblemId, out List<LegacyVocabularyToken> inherited) ? inherited : new List<LegacyVocabularyToken>();
                IEnumerable<LegacyVocabularyToken> counter = value.weaknessWindows.SelectMany(x => One(DefaultEnemyMechanicVocabularyCatalog.WeaknessMechanicSource, x.triggerCondition));
                yield return new MechanicProfileNormalizationSource(ValidationProfileKind.Boss, value.bossProblemId, value.displayName, publicTokens,
                    BossRequiredTokens(value.requiredProblemAttributes), Array.Empty<LegacyVocabularyToken>(), counter,
                    Facts("validationGoal", new[] { value.validationGoal }, "keyRequirements", value.keyRequirements.Select(x => x.keyId), "minimumKeysRequired", new[] { value.minimumKeysRequired.ToString() }),
                    value.weaknessWindows.Select(x => x.weaknessWindowId).Concat(value.dropBiases.Select(x => x.dropBiasId)),
                    value.devOnly, value.isEnabled, value.entersFormalFlow);
            }
        }

        private static IEnumerable<MapRuleNormalizationSource> CreateMapRules(BuildProblemSeedDataset data)
        {
            foreach (MapRuleSeed value in data.mapRules)
            {
                IEnumerable<LegacyVocabularyToken> tokens = Tokens(DefaultEnemyMechanicVocabularyCatalog.MapAffectedSource, value.affectedTags)
                    .Concat(Tokens(DefaultEnemyMechanicVocabularyCatalog.MapBuffSource, value.buffTags))
                    .Concat(Tokens(DefaultEnemyMechanicVocabularyCatalog.MapDebuffSource, value.debuffTags));
                yield return new MapRuleNormalizationSource(value.mapRuleId, value.displayName, tokens, value.enemyProblemIds, value.bossProblemIds,
                    Facts("dropBiasId", new[] { value.dropBiasId }, "warningText", new[] { value.warningText }, "description", new[] { value.description }),
                    value.devOnly, value.isEnabled, value.entersFormalFlow);
            }
        }

        private static IEnumerable<CarrierMechanicBindingSource> CreateCarrierBindings()
        {
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_shield_guard", "dev_enemy_shield_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_swarm_pack", "dev_enemy_swarm_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_poison_cultist", "dev_enemy_poison_burn_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_burning_wisp", "dev_enemy_poison_burn_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_spirit_thief", "dev_enemy_spirit_thief_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_seal_locker", "dev_enemy_seal_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_burst_assassin", "dev_enemy_burst_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_caster_chanter", "dev_enemy_caster_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_thick_blood", "dev_enemy_thick_blood_problem");
            yield return B(ValidationCarrierKind.Enemy, "dev_enemy_formation_eye_jammer", "dev_enemy_formation_eye_problem");
            yield return B(ValidationCarrierKind.Boss, "dev_boss_shield_jinglei", "dev_boss_black_furnace_shell");
            yield return B(ValidationCarrierKind.Boss, "dev_boss_swarm_lihuo", "dev_boss_paper_faces");
            yield return B(ValidationCarrierKind.Boss, "dev_boss_burst_huzhen", "dev_boss_bronze_formation_general");
            yield return B(ValidationCarrierKind.Boss, "dev_boss_debuff_jinge", "dev_boss_dirty_dream_mother");
            yield return B(ValidationCarrierKind.Boss, "dev_boss_caster_zhenhun", "dev_boss_spirit_thief_core");
            yield return B(ValidationCarrierKind.Boss, "dev_boss_energy_juneng", "dev_boss_spirit_thief_core");
            yield return B(ValidationCarrierKind.Boss, "dev_boss_hybrid_combo", "dev_boss_black_furnace_complex_eye");
        }

        private static CarrierMechanicBindingSource B(ValidationCarrierKind kind, string carrierId, string profileId) => new CarrierMechanicBindingSource(kind, carrierId, profileId, CarrierBindingReason);
        private static IEnumerable<LegacyVocabularyToken> One(string source, string key) { yield return new LegacyVocabularyToken(source, key); }
        private static IEnumerable<LegacyVocabularyToken> Tokens(string source, IEnumerable<string> keys) => (keys ?? Array.Empty<string>()).Select(key => new LegacyVocabularyToken(source, key));
        private static IEnumerable<LegacyVocabularyToken> BossRequiredTokens(IEnumerable<string> keys) => (keys ?? Array.Empty<string>()).Select(key =>
            new LegacyVocabularyToken(key == "BurstWindow" ? DefaultEnemyMechanicVocabularyCatalog.OptionalCapabilitySource : DefaultEnemyMechanicVocabularyCatalog.RequiredCapabilitySource, key));
        private static IEnumerable<string> Facts(string name1, IEnumerable<string> values1, string name2, IEnumerable<string> values2, string name3, IEnumerable<string> values3) =>
            (values1 ?? Array.Empty<string>()).Select(x => name1 + "=" + x).Concat((values2 ?? Array.Empty<string>()).Select(x => name2 + "=" + x)).Concat((values3 ?? Array.Empty<string>()).Select(x => name3 + "=" + x));
    }
}
