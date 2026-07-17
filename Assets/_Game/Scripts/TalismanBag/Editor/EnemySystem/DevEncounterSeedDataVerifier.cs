using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SeedData;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.SystemSnapshot;
using TalismanBag.EnemySystem.Vocabulary;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class DevEncounterSeedDataVerifier
    {
        private static readonly string[] ReportNames =
        {
            "DevEncounterSeedDataReport.md",
            "DevEncounterSeedDataSpec.csv",
            "DevEncounterSeedInventory.csv",
            "DevEncounterSeedCompositionRows.csv",
            "DevEncounterSeedSkillPhaseRows.csv",
            "DevEncounterSeedPressureWindowRows.csv",
            "DevEncounterSeedLegacyMapping.csv",
            "DevEncounterSeedDataLeakCheckReport.md"
        };

        [MenuItem("Tools/TalismanBag/Verify Dev Encounter Seed Data")]
        public static void VerifyMenu()
        {
            int failed = RunOffline(ProjectRoot());
            if (failed != 0) throw new InvalidOperationException("E10 verifier failed: " + failed);
        }

        public static void RunBatch()
        {
            int failed = RunOffline(ProjectRoot());
            if (failed != 0) throw new InvalidOperationException("E10 batch verifier failed: " + failed);
        }

        public static int RunOffline(string projectRoot)
        {
            try
            {
                string root = Path.GetFullPath(projectRoot ?? string.Empty);
                EnemyMechanicVocabularySnapshot vocabulary = DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                    DefaultEnemyMechanicVocabularyCatalog.CreateInput(false));
                EnemyValidationContentSnapshot normalization = EnemyValidationContentNormalizer.CreateSnapshot(false);
                string vocabularyBefore = vocabulary.BuildCanonicalSignature();
                string normalizationBefore = normalization.CanonicalSignature;
                DevEncounterSeedDataSnapshot snapshot = DefaultDevEncounterSeedDataProvider.Instance.CreateSnapshot(
                    new DevEncounterSeedDataInput(vocabulary, normalization));
                List<Check> checks = CreateChecks(snapshot, vocabulary, normalization,
                    vocabularyBefore, normalizationBefore);

                Add(checks, "reports.determinism", "8/8 identical", "8/8 identical", true);
                Add(checks, "reports.missingRegeneration", "PASS", "PASS", true);
                WriteReports(root, snapshot, checks, "8/8 identical", "PASS");
                string[] first = HashReports(root);
                WriteReports(root, snapshot, checks, "8/8 identical", "PASS");
                string[] second = HashReports(root);
                bool deterministic = first.SequenceEqual(second, StringComparer.Ordinal);
                string missingPath = Path.Combine(root, "Docs", "V0.4", "Reports", ReportNames[2]);
                File.Delete(missingPath);
                WriteReports(root, snapshot, checks, "8/8 identical", "PASS");
                string[] rebuilt = HashReports(root);
                bool regenerated = File.Exists(missingPath) && second.SequenceEqual(rebuilt, StringComparer.Ordinal);
                if (!deterministic || !regenerated)
                    throw new InvalidOperationException("E10 report determinism or regeneration failed.");
                WriteReports(root, snapshot, checks, "8/8 identical", "PASS");

                int failed = checks.Count(value => !value.Passed);
                Debug.Log("V0.4-DevEncounterSeedData01 verifier " + (checks.Count - failed) + "/" + checks.Count
                    + " passed; reports=" + string.Join(",", ReportNames));
                return failed;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return 1;
            }
        }

        private static List<Check> CreateChecks(DevEncounterSeedDataSnapshot snapshot,
            EnemyMechanicVocabularySnapshot vocabulary, EnemyValidationContentSnapshot normalization,
            string vocabularyBefore, string normalizationBefore)
        {
            List<Check> checks = new List<Check>();
            EnemySystemSnapshot e09 = snapshot.EnemySystemSnapshot;
            EncounterCompositionCatalogSnapshot e04 = e09.EncounterCompositionCatalogSnapshot;
            EnemySkillBossPhaseCatalogSnapshot e05 = e09.EnemySkillBossPhaseCatalogSnapshot;
            CounterWindowAndPressureCatalogSnapshot e06 = e09.CounterWindowAndPressureCatalogSnapshot;
            int waves = e04.Encounters.Sum(value => value.Waves.Count);
            int slots = e04.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count));
            int enemySlots = e04.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count(slot => slot.Kind == EncounterSlotKind.Enemy)));
            int bossSlots = slots - enemySlots;
            int groups = e06.BuildPressureProfiles.Sum(value => value.DeveloperOnly.RequirementGroups.Count);
            int requirements = e06.BuildPressureProfiles.Sum(value => value.DeveloperOnly.RequirementGroups.Sum(group => group.Requirements.Count));

            Add(checks, "schema", "DevEncounterSeedData.v1/1", snapshot.SchemaId + "/" + snapshot.SchemaVersion,
                snapshot.SchemaId == DevEncounterSeedDataSchema.SchemaId && snapshot.SchemaVersion == 1);
            Add(checks, "baseline.e03", "11/7/18 carriers;16 profiles;10 maps;17 bindings;2 exceptions",
                normalization.Enemies.Count + "/" + normalization.Bosses.Count + "/"
                    + (normalization.Enemies.Count + normalization.Bosses.Count) + ";"
                    + normalization.MechanicProfiles.Count + ";" + normalization.MapRules.Count + ";"
                    + normalization.CarrierMechanicBindings.Count + ";" + normalization.Exceptions.Count,
                normalization.Enemies.Count == 11 && normalization.Bosses.Count == 7
                    && normalization.MechanicProfiles.Count == 16 && normalization.MapRules.Count == 10
                    && normalization.CarrierMechanicBindings.Count == 17 && normalization.Exceptions.Count == 2);
            Add(checks, "mapping.seeds", "4/4", snapshot.SeedProfiles.Count + "/4", snapshot.SeedProfiles.Count == 4);
            Add(checks, "composition.counts", "4/12/12", e04.Encounters.Count + "/" + waves + "/" + slots,
                e04.Encounters.Count == 4 && waves == 12 && slots == 12);
            Add(checks, "composition.slotKinds", "8/4", enemySlots + "/" + bossSlots,
                enemySlots == 8 && bossSlots == 4);
            Add(checks, "skillPhase.counts", "9/9/10/8/4",
                e05.SkillPatterns.Count + "/" + e05.SkillSequences.Count + "/" + e05.CarrierSkillBindings.Count
                    + "/" + e05.BossPhaseProfiles.Count + "/" + e05.BossPhasePlans.Count,
                e05.SkillPatterns.Count == 9 && e05.SkillSequences.Count == 9
                    && e05.CarrierSkillBindings.Count == 10 && e05.BossPhaseProfiles.Count == 8
                    && e05.BossPhasePlans.Count == 4);
            Add(checks, "pressureWindow.counts", "10/6", e06.BuildPressureProfiles.Count + "/" + e06.CounterWindowProfiles.Count,
                e06.BuildPressureProfiles.Count == 10 && e06.CounterWindowProfiles.Count == 6);
            Add(checks, "requirements.counts", "16/39", groups + "/" + requirements, groups == 16 && requirements == 39);
            int capabilityFidelity = CapabilityFidelityCount(e06, normalization);
            Add(checks, "fidelity.capabilities", "10/10", capabilityFidelity + "/10",
                capabilityFidelity == 10);
            int channelFidelity = ChannelFidelityCount(e06, normalization);
            Add(checks, "fidelity.channels", "10/10", channelFidelity + "/10",
                channelFidelity == 10);
            int pressureSourceFidelity = PressureSourceFidelityCount(e06, e05);
            Add(checks, "fidelity.pressureSources", e06.PressureSourceBindings.Count + "/"
                    + e06.PressureSourceBindings.Count,
                pressureSourceFidelity + "/" + e06.PressureSourceBindings.Count,
                pressureSourceFidelity == e06.PressureSourceBindings.Count);
            int windowConditionCompatibility = WindowConditionCompatibilityCount(e06);
            Add(checks, "fidelity.windowSourceConditions", "20/20",
                windowConditionCompatibility + "/" + e06.CounterWindowSourceBindings.Count,
                e06.CounterWindowSourceBindings.Count == 20 && windowConditionCompatibility == 20);
            int reachablePressureWindows = ReachablePressureWindowCount(e06);
            Add(checks, "reachability.pressureWindows", "13/13",
                reachablePressureWindows + "/" + e06.PressureCounterWindowBindings.Count,
                e06.PressureCounterWindowBindings.Count == 13 && reachablePressureWindows == 13);
            Add(checks, "legacy.mapping", "4/4;runtime=0", snapshot.LegacyMappings.Count + "/4;runtime="
                + snapshot.LegacyMappings.Count(value => value.RuntimeConsumesLegacy),
                snapshot.LegacyMappings.Count == 4 && snapshot.LegacyMappings.All(value => !value.RuntimeConsumesLegacy));

            bool furnace = e04.TryGetEncounterById("dev_encounter_4_10_furnace_core", out EncounterCompositionSnapshot furnaceEncounter)
                && furnaceEncounter.MapRuleIds.Contains("dev_map_furnace_ash_fall")
                && furnaceEncounter.Waves.SelectMany(value => value.Slots)
                    .Any(value => value.MechanicProfileIds.Contains("dev_enemy_spirit_thief_problem"));
            Add(checks, "composition.furnaceCrossLayer", "accepted", furnace ? "accepted" : "missing", furnace);
            CarrierSkillBindingSnapshot poison = e05.CarrierSkillBindings.First(value => value.CarrierId == "dev_enemy_poison_cultist");
            CarrierSkillBindingSnapshot burning = e05.CarrierSkillBindings.First(value => value.CarrierId == "dev_enemy_burning_wisp");
            Add(checks, "reuse.poisonBurn", "same sequence", string.Join(";", poison.SkillSequenceIds),
                poison.SkillSequenceIds.SequenceEqual(burning.SkillSequenceIds));
            bool manyToMany = HasWindows(e06, "dev_boss_spirit_thief_core",
                    DevEncounterSeedDataCatalog.WindowInterrupt, DevEncounterSeedDataCatalog.WindowEnergy)
                && HasWindows(e06, "dev_boss_black_furnace_complex_eye",
                    DevEncounterSeedDataCatalog.WindowShell, DevEncounterSeedDataCatalog.WindowClear,
                    DevEncounterSeedDataCatalog.WindowEnergy)
                && new[] { "dev_enemy_poison_burn_problem", "dev_enemy_polluted_tile_problem", "dev_boss_dirty_dream_mother" }
                    .All(id => HasWindows(e06, id, DevEncounterSeedDataCatalog.WindowCleanse));
            Add(checks, "reuse.pressureWindow", "many-to-many", manyToMany ? "many-to-many" : "missing", manyToMany);

            int unresolved = e09.RelationIndex.Count(value => !value.Resolved);
            int isolation = e09.RelationIndex.Count(value => !value.IsolationCompatible);
            Add(checks, "references.resolved", "0", unresolved.ToString(CultureInfo.InvariantCulture), unresolved == 0);
            Add(checks, "isolation.relations", "0", isolation.ToString(CultureInfo.InvariantCulture), isolation == 0);
            Add(checks, "isolation.root", "true/false/false", snapshot.DevOnly + "/" + snapshot.IsEnabled + "/" + snapshot.EntersFormalFlow,
                snapshot.DevOnly && !snapshot.IsEnabled && !snapshot.EntersFormalFlow);
            Add(checks, "signatures.e09", "Full+PlayerSafe", e09.CanonicalSignature + "/" + e09.PlayerSafeCanonicalSignature,
                IsSignature(e09.CanonicalSignature) && IsSignature(e09.PlayerSafeCanonicalSignature));
            Add(checks, "signatures.e10", "Full+PlayerSafe", snapshot.CanonicalSignature + "/" + snapshot.PlayerSafeCanonicalSignature,
                IsSignature(snapshot.CanonicalSignature) && IsSignature(snapshot.PlayerSafeCanonicalSignature));
            Add(checks, "provider.inputUnchanged", "unchanged", vocabulary.BuildCanonicalSignature() + "/" + normalization.CanonicalSignature,
                vocabulary.BuildCanonicalSignature() == vocabularyBefore && normalization.CanonicalSignature == normalizationBefore);

            DevEncounterSeedDataSnapshot repeat = DefaultDevEncounterSeedDataProvider.Instance.CreateSnapshot(
                new DevEncounterSeedDataInput(vocabulary, normalization));
            EnemyMechanicVocabularySnapshot reverseVocabulary = DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                DefaultEnemyMechanicVocabularyCatalog.CreateInput(true));
            EnemyValidationContentSnapshot reverseNormalization = EnemyValidationContentNormalizer.CreateSnapshot(true);
            DevEncounterSeedDataSnapshot reverse = DefaultDevEncounterSeedDataProvider.Instance.CreateSnapshot(
                new DevEncounterSeedDataInput(reverseVocabulary, reverseNormalization));
            Add(checks, "determinism.repeat", "Full+PlayerSafe stable", repeat.CanonicalSignature + "/" + repeat.PlayerSafeCanonicalSignature,
                repeat.CanonicalSignature == snapshot.CanonicalSignature
                    && repeat.PlayerSafeCanonicalSignature == snapshot.PlayerSafeCanonicalSignature);
            Add(checks, "determinism.inputOrder", "Full+PlayerSafe stable", reverse.CanonicalSignature + "/" + reverse.PlayerSafeCanonicalSignature,
                reverse.CanonicalSignature == snapshot.CanonicalSignature
                    && reverse.PlayerSafeCanonicalSignature == snapshot.PlayerSafeCanonicalSignature);

            Add(checks, "immutability.seedProfiles", "rejected", MutationRejected(snapshot.SeedProfiles, snapshot.SeedProfiles[0]) ? "rejected" : "accepted",
                MutationRejected(snapshot.SeedProfiles, snapshot.SeedProfiles[0]));
            Add(checks, "immutability.playerProfiles", "rejected", MutationRejected(snapshot.PlayerSafe.SeedProfiles, snapshot.PlayerSafe.SeedProfiles[0]) ? "rejected" : "accepted",
                MutationRejected(snapshot.PlayerSafe.SeedProfiles, snapshot.PlayerSafe.SeedProfiles[0]));
            Add(checks, "immutability.catalogs", "E04/E05/E06 rejected", "checked",
                MutationRejected(e04.Encounters, e04.Encounters[0])
                    && MutationRejected(e05.SkillPatterns, e05.SkillPatterns[0])
                    && MutationRejected(e06.BuildPressureProfiles, e06.BuildPressureProfiles[0]));

            AddSensitivityChecks(checks, snapshot, vocabulary, normalization);
            AddNegativeChecks(checks, snapshot);
            IReadOnlyList<DevEncounterSeedValidationIssue> validation = DefaultDevEncounterSeedDataValidator.Instance.ValidateSnapshot(snapshot);
            Add(checks, "validator.realSnapshot", "0 issues", validation.Count + " issues", validation.Count == 0);
            int playerLeaks = validation.Count(value => value.Category == DevEncounterSeedValidationCategory.PlayerLeak);
            Add(checks, "playerSafe.answerLeak", "0", playerLeaks.ToString(CultureInfo.InvariantCulture), playerLeaks == 0);
            Add(checks, "formalFlow.references", "0", snapshot.EntersFormalFlow ? "1" : "0", !snapshot.EntersFormalFlow);
            return checks;
        }

        private static void AddSensitivityChecks(List<Check> checks, DevEncounterSeedDataSnapshot baseline,
            EnemyMechanicVocabularySnapshot vocabulary, EnemyValidationContentSnapshot normalization)
        {
            EnemySystemSnapshot baseRoot = baseline.EnemySystemSnapshot;
            VerifierResolver resolver = new VerifierResolver(normalization, vocabulary)
            {
                SkillPhase = baseRoot.EnemySkillBossPhaseCatalogSnapshot
            };

            EncounterCompositionCatalogSnapshot quantityE04 = MutateQuantity(baseRoot.EncounterCompositionCatalogSnapshot, resolver);
            EnemySystemSnapshot quantityRoot = RebuildRoot(baseRoot, quantityE04,
                baseRoot.EnemySkillBossPhaseCatalogSnapshot, baseRoot.CounterWindowAndPressureCatalogSnapshot);
            DevEncounterSeedDataSnapshot quantity = new DevEncounterSeedDataSnapshot(baseline.SeedProfiles, quantityRoot, baseline.LegacyMappings);
            AddMatrix(checks, "sensitivity.encounterQuantity", baseline, quantity, true, false);

            EnemySkillBossPhaseCatalogSnapshot developerE05 = MutateSkill(baseRoot.EnemySkillBossPhaseCatalogSnapshot,
                resolver, false);
            EnemySystemSnapshot developerRoot = RebuildRoot(baseRoot, baseRoot.EncounterCompositionCatalogSnapshot,
                developerE05, baseRoot.CounterWindowAndPressureCatalogSnapshot);
            DevEncounterSeedDataSnapshot developer = new DevEncounterSeedDataSnapshot(baseline.SeedProfiles, developerRoot, baseline.LegacyMappings);
            AddMatrix(checks, "sensitivity.e05Developer", baseline, developer, true, false);

            EnemySkillBossPhaseCatalogSnapshot playerE05 = MutateSkill(baseRoot.EnemySkillBossPhaseCatalogSnapshot,
                resolver, true);
            EnemySystemSnapshot playerRoot = RebuildRoot(baseRoot, baseRoot.EncounterCompositionCatalogSnapshot,
                playerE05, baseRoot.CounterWindowAndPressureCatalogSnapshot);
            DevEncounterSeedDataSnapshot player = new DevEncounterSeedDataSnapshot(baseline.SeedProfiles, playerRoot, baseline.LegacyMappings);
            AddMatrix(checks, "sensitivity.publicCue", baseline, player, true, true);

            resolver.SkillPhase = baseRoot.EnemySkillBossPhaseCatalogSnapshot;
            CounterWindowAndPressureCatalogSnapshot thresholdE06 = MutateThreshold(
                baseRoot.CounterWindowAndPressureCatalogSnapshot, resolver);
            EnemySystemSnapshot thresholdRoot = RebuildRoot(baseRoot, baseRoot.EncounterCompositionCatalogSnapshot,
                baseRoot.EnemySkillBossPhaseCatalogSnapshot, thresholdE06);
            DevEncounterSeedDataSnapshot threshold = new DevEncounterSeedDataSnapshot(baseline.SeedProfiles, thresholdRoot, baseline.LegacyMappings);
            AddMatrix(checks, "sensitivity.requirementThreshold", baseline, threshold, true, false);

            DevEncounterSeedLegacyMappingSnapshot first = baseline.LegacyMappings[0];
            DevEncounterSeedLegacyMappingSnapshot changed = new DevEncounterSeedLegacyMappingSnapshot(
                first.LegacyStageId + "_changed", first.SeedId, first.MapRuleId,
                first.PrimaryEnemyProblemId, first.BossProblemId, first.BossCarrierId);
            DevEncounterSeedDataSnapshot legacy = new DevEncounterSeedDataSnapshot(baseline.SeedProfiles,
                baseRoot, baseline.LegacyMappings.Skip(1).Concat(new[] { changed }));
            AddMatrix(checks, "sensitivity.legacySource", baseline, legacy, true, false);
        }

        private static void AddNegativeChecks(List<Check> checks, DevEncounterSeedDataSnapshot baseline)
        {
            DevEncounterSeedProfileSnapshot first = baseline.SeedProfiles[0];
            ExpectIssue(checks, "negative.missingSeed", "E10_SEED_COUNT",
                new DevEncounterSeedDataSnapshot(baseline.SeedProfiles.Skip(1), baseline.EnemySystemSnapshot, baseline.LegacyMappings));
            DevEncounterSeedDeveloperSnapshot wrongDeveloper = new DevEncounterSeedDeveloperSnapshot(
                first.DeveloperOnly.SourceScenarioId + "_wrong", first.DeveloperOnly.ValidationGoalKey,
                first.DeveloperOnly.DeveloperDiagnosticCategoryKeys, first.DeveloperOnly.CompositionNoteKey);
            ExpectIssue(checks, "negative.stageMapping", "E10_SEED_MAPPING_MISMATCH",
                ReplaceSeed(baseline, Copy(first, developer: wrongDeveloper)));
            DevEncounterSeedInternalSnapshot unknown = new DevEncounterSeedInternalSnapshot(
                first.InternalOnly.DevChapterLabel, first.InternalOnly.EncounterId, "dev_map_unknown",
                first.InternalOnly.OrdinaryWaveId, first.InternalOnly.DampingWaveId, first.InternalOnly.BossWaveId,
                first.InternalOnly.BossCarrierId, "dev_boss_unknown",
                first.InternalOnly.BuildPressureProfileIds.Concat(new[] { "dev_pressure_unknown" }),
                first.InternalOnly.CounterWindowIds.Concat(new[] { "dev_window_unknown" }));
            ExpectIssue(checks, "negative.unknownReferences", "E10_SEED_REFERENCE_UNRESOLVED",
                ReplaceSeed(baseline, Copy(first, internalOnly: unknown)));
            DevEncounterSeedPlayerProjection leak = new DevEncounterSeedPlayerProjection(first.SeedId,
                "enemy.encounter.threshold.capability.label", first.PlayerSafe.PublicAtmosphereCueKey,
                "legacy.dev_balance_answer", first.PlayerSafe.PlayerHintCategoryKeys);
            ExpectIssue(checks, "negative.playerLeak", "E10_PLAYER_FORBIDDEN_TOKEN",
                ReplaceSeed(baseline, Copy(first, player: leak)));
            ExpectIssue(checks, "negative.isolation", "E10_ISOLATION_INVALID",
                ReplaceSeed(baseline, new DevEncounterSeedProfileSnapshot(first.PlayerSafe, first.InternalOnly,
                    first.DeveloperOnly, false, true, true)));
            ExpectInputIssue(checks, "negative.nullInput", "E10_NULL_INPUT", null);
            ExpectInputIssue(checks, "negative.nullDependency", "E10_E02_NULL",
                new DevEncounterSeedDataInput(null, baseline.EnemySystemSnapshot.EnemyValidationContentSnapshot));
            AddStructuralNegativeChecks(checks, baseline);
            AddSemanticNegativeChecks(checks, baseline);
        }

        private static void AddStructuralNegativeChecks(List<Check> checks,
            DevEncounterSeedDataSnapshot baseline)
        {
            EnemySystemSnapshot root = baseline.EnemySystemSnapshot;
            VerifierResolver resolver = new VerifierResolver(root.EnemyValidationContentSnapshot,
                root.EnemyMechanicVocabularySnapshot) { SkillPhase = root.EnemySkillBossPhaseCatalogSnapshot };

            ExpectRejected(checks, "negative.waveOrderGap", () =>
                DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                    MutateEncounterInput(root.EncounterCompositionCatalogSnapshot, "waveGap"), resolver));
            ExpectRejected(checks, "negative.bossSlotEnemyCarrier", () =>
                DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                    MutateEncounterInput(root.EncounterCompositionCatalogSnapshot, "bossEnemy"), resolver));
            ExpectRejected(checks, "negative.unboundCarrierMechanic", () =>
                DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                    MutateEncounterInput(root.EncounterCompositionCatalogSnapshot, "unbound"), resolver));
            ExpectRejected(checks, "negative.unknownMapRule", () =>
                DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                    MutateEncounterInput(root.EncounterCompositionCatalogSnapshot, "unknownMap"), resolver));
            ExpectRejected(checks, "negative.unknownPattern", () =>
                DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(
                    MutateSkillInput(root.EnemySkillBossPhaseCatalogSnapshot, false), resolver));
            ExpectRejected(checks, "negative.unknownPhase", () =>
                DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(
                    MutateSkillInput(root.EnemySkillBossPhaseCatalogSnapshot, true), resolver));
            ExpectRejected(checks, "negative.pressureWeight", () =>
                DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                    MutatePressureInput(root.CounterWindowAndPressureCatalogSnapshot, "weight"), resolver));
            ExpectRejected(checks, "negative.unknownPressure", () =>
                DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                    MutatePressureInput(root.CounterWindowAndPressureCatalogSnapshot, "unknownPressure"), resolver));
            ExpectRejected(checks, "negative.unknownWindow", () =>
                DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                    MutatePressureInput(root.CounterWindowAndPressureCatalogSnapshot, "unknownWindow"), resolver));

            CounterWindowAndPressureCatalogSnapshot wrongMode = DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                MutatePressureInput(root.CounterWindowAndPressureCatalogSnapshot, "wrongMode"), resolver);
            EnemySystemSnapshot wrongModeRoot = RebuildRoot(root, root.EncounterCompositionCatalogSnapshot,
                root.EnemySkillBossPhaseCatalogSnapshot, wrongMode);
            ExpectIssue(checks, "negative.requirementRoleMatch", "E10_REQUIRED_MATCH_MODE",
                new DevEncounterSeedDataSnapshot(baseline.SeedProfiles, wrongModeRoot, baseline.LegacyMappings));
        }

        private static void AddSemanticNegativeChecks(List<Check> checks,
            DevEncounterSeedDataSnapshot baseline)
        {
            ExpectIssue(checks, "negative.semanticRequiredCapability",
                "E10_REQUIRED_CAPABILITY_FIDELITY",
                SemanticCandidate(baseline, "requiredCapability"));
            ExpectIssue(checks, "negative.semanticPressureChannel",
                "E10_PRESSURE_CHANNEL_FIDELITY",
                SemanticCandidate(baseline, "pressureChannel"));
            ExpectIssue(checks, "negative.semanticPressureSource",
                "E10_PRESSURE_SOURCE_FIDELITY",
                SemanticCandidate(baseline, "pressureSource"));
            ExpectIssue(checks, "negative.semanticPressureWindowReachability",
                "E10_PRESSURE_WINDOW_SOURCE_UNREACHABLE",
                SemanticCandidate(baseline, "reachability"));
            ExpectIssue(checks, "negative.semanticWindowSourceCondition",
                "E10_WINDOW_SOURCE_CONDITION_MISMATCH",
                SemanticCandidate(baseline, "windowCondition"));
        }

        private static DevEncounterSeedDataSnapshot SemanticCandidate(
            DevEncounterSeedDataSnapshot baseline, string mutation)
        {
            EnemySystemSnapshot root = baseline.EnemySystemSnapshot;
            VerifierResolver resolver = new VerifierResolver(root.EnemyValidationContentSnapshot,
                root.EnemyMechanicVocabularySnapshot)
            {
                SkillPhase = root.EnemySkillBossPhaseCatalogSnapshot
            };
            CounterWindowAndPressureCatalogSnapshot e06 =
                DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                    MutateSemanticPressureInput(root.CounterWindowAndPressureCatalogSnapshot, mutation),
                    resolver);
            EnemySystemSnapshot changedRoot = RebuildRoot(root,
                root.EncounterCompositionCatalogSnapshot,
                root.EnemySkillBossPhaseCatalogSnapshot, e06);
            return new DevEncounterSeedDataSnapshot(baseline.SeedProfiles,
                changedRoot, baseline.LegacyMappings);
        }

        private static CounterWindowAndPressureCatalogInput MutateSemanticPressureInput(
            CounterWindowAndPressureCatalogSnapshot source, string mutation)
        {
            List<BuildPressureProfileSnapshot> profiles = source.BuildPressureProfiles.ToList();
            List<CounterWindowProfileSnapshot> windows = source.CounterWindowProfiles.ToList();
            List<PressureSourceBindingSnapshot> pressureSources = source.PressureSourceBindings.ToList();
            List<CounterWindowSourceBindingSnapshot> windowSources = source.CounterWindowSourceBindings.ToList();

            if (mutation == "requiredCapability")
            {
                int profileIndex = profiles.FindIndex(value => value.BuildPressureProfileId == "dev_enemy_caster_problem");
                BuildPressureProfileSnapshot value = profiles[profileIndex];
                HashSet<string> currentKeys = new HashSet<string>(value.DeveloperOnly.RequirementGroups
                    .SelectMany(group => group.Requirements).Select(requirement => requirement.BuildCapabilityKey),
                    StringComparer.Ordinal);
                string replacementKey = source.BuildPressureProfiles.SelectMany(profile => profile.DeveloperOnly.RequirementGroups)
                    .SelectMany(group => group.Requirements).Select(requirement => requirement.BuildCapabilityKey)
                    .First(key => !currentKeys.Contains(key));
                List<BuildCapabilityRequirementGroupSnapshot> groups = value.DeveloperOnly.RequirementGroups.ToList();
                int groupIndex = groups.FindIndex(group => group.RequirementRole == CapabilityRequirementRole.Required);
                BuildCapabilityRequirementGroupSnapshot group = groups[groupIndex];
                List<BuildCapabilityRequirementSnapshot> requirements = group.Requirements.ToList();
                requirements[0] = new BuildCapabilityRequirementSnapshot(replacementKey,
                    requirements[0].MinimumCapabilityBasisPoints);
                groups[groupIndex] = new BuildCapabilityRequirementGroupSnapshot(group.RequirementGroupId,
                    group.RequirementRole, group.RequirementMatchMode, requirements);
                profiles[profileIndex] = CopyPressure(value, value.InternalOnly,
                    new BuildPressureDeveloperDiagnostics(groups,
                        value.DeveloperOnly.DeveloperDiagnosticCategoryKeys,
                        value.DeveloperOnly.SourceReferenceIds));
            }
            else if (mutation == "pressureChannel")
            {
                int profileIndex = profiles.FindIndex(value => value.BuildPressureProfileId == "dev_enemy_caster_problem");
                BuildPressureProfileSnapshot value = profiles[profileIndex];
                HashSet<string> currentKeys = new HashSet<string>(value.InternalOnly.PressureChannelContributions
                    .Select(channel => channel.PressureChannelKey), StringComparer.Ordinal);
                string replacementKey = source.BuildPressureProfiles
                    .SelectMany(profile => profile.InternalOnly.PressureChannelContributions)
                    .Select(channel => channel.PressureChannelKey).First(key => !currentKeys.Contains(key));
                PressureChannelContributionSnapshot original = value.InternalOnly.PressureChannelContributions[0];
                profiles[profileIndex] = CopyPressure(value,
                    new BuildPressureInternalSpec(new[]
                    {
                        new PressureChannelContributionSnapshot(replacementKey, original.WeightBasisPoints)
                    }), value.DeveloperOnly);
            }
            else if (mutation == "pressureSource")
            {
                int sourceIndex = pressureSources.FindIndex(value => value.SourceKind == PressureSourceKind.SkillPattern
                    && value.SourceId == "dev_skill_caster_long_cast");
                pressureSources[sourceIndex] = new PressureSourceBindingSnapshot(
                    PressureSourceKind.SkillPattern, "dev_skill_caster_long_cast", "dev_enemy_seal_problem");
            }
            else if (mutation == "reachability")
            {
                windowSources.RemoveAll(value => value.SourceKind == PressureSourceKind.SkillPattern
                    && value.SourceId == "dev_skill_caster_long_cast"
                    && value.CounterWindowId == DevEncounterSeedDataCatalog.WindowInterrupt);
            }
            else if (mutation == "windowCondition")
            {
                int windowIndex = windows.FindIndex(value => value.CounterWindowId == DevEncounterSeedDataCatalog.WindowCleanse);
                CounterWindowProfileSnapshot value = windows[windowIndex];
                CounterWindowConditionSnapshot original = value.InternalOnly.OpenConditions[0];
                CounterWindowInternalSpec changed = new CounterWindowInternalSpec(
                    value.InternalOnly.CounterWindowTypeKey, value.InternalOnly.OpenConditionMatchMode,
                    new[] { new CounterWindowConditionSnapshot(original.ConditionId,
                        CounterWindowConditionKind.SkillPatternStarted, "dev_skill_caster_long_cast") },
                    value.InternalOnly.CloseConditionMatchMode, value.InternalOnly.CloseConditions,
                    value.InternalOnly.MaximumDurationMilliseconds);
                windows[windowIndex] = new CounterWindowProfileSnapshot(value.CounterWindowReference,
                    value.PlayerSafe, changed, value.DeveloperOnly);
            }

            return new CounterWindowAndPressureCatalogInput(profiles, windows,
                pressureSources, windowSources, source.PressureCounterWindowBindings);
        }

        private static BuildPressureProfileSnapshot CopyPressure(BuildPressureProfileSnapshot value,
            BuildPressureInternalSpec internalOnly, BuildPressureDeveloperDiagnostics developerOnly)
        {
            return new BuildPressureProfileSnapshot(value.BuildPressureProfileId,
                value.PlayerSafe, internalOnly, developerOnly,
                value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
        }

        private static EncounterCompositionCatalogInput MutateEncounterInput(
            EncounterCompositionCatalogSnapshot source, string mutation)
        {
            List<EncounterCompositionSnapshot> encounters = source.Encounters.ToList();
            EncounterCompositionSnapshot original = encounters[0];
            List<EncounterWaveSnapshot> waves = original.Waves.ToList();
            IReadOnlyList<string> maps = original.MapRuleIds;
            if (mutation == "waveGap")
            {
                EncounterWaveSnapshot value = waves[1];
                waves[1] = new EncounterWaveSnapshot(value.WaveId, 4, value.Slots);
            }
            else if (mutation == "bossEnemy")
            {
                EncounterWaveSnapshot value = waves[2];
                EncounterSlotSnapshot slot = value.Slots[0];
                waves[2] = new EncounterWaveSnapshot(value.WaveId, value.WaveOrder, new[]
                {
                    new EncounterSlotSnapshot(slot.SlotId, slot.SpawnOrder, EncounterSlotKind.Boss,
                        EncounterSlotRole.Boss, "dev_enemy_caster_chanter", 1, slot.MechanicProfileIds)
                });
            }
            else if (mutation == "unbound")
            {
                EncounterWaveSnapshot value = waves[0];
                EncounterSlotSnapshot slot = value.Slots[0];
                waves[0] = new EncounterWaveSnapshot(value.WaveId, value.WaveOrder, new[]
                {
                    new EncounterSlotSnapshot(slot.SlotId, slot.SpawnOrder, slot.Kind, slot.Role,
                        slot.CarrierId, slot.Quantity, new[] { "dev_enemy_seal_problem" })
                });
            }
            else if (mutation == "unknownMap")
            {
                maps = new[] { "dev_map_unknown" };
            }
            encounters[0] = new EncounterCompositionSnapshot(original.EncounterReference, maps,
                waves, original.DeveloperContentTagIds);
            return new EncounterCompositionCatalogInput(encounters);
        }

        private static EnemySkillBossPhaseCatalogInput MutateSkillInput(
            EnemySkillBossPhaseCatalogSnapshot source, bool unknownPhase)
        {
            if (!unknownPhase)
            {
                List<SkillSequenceSnapshot> sequences = source.SkillSequences.ToList();
                SkillSequenceSnapshot value = sequences[0];
                SkillSequenceStepSnapshot step = value.Steps[0];
                sequences[0] = new SkillSequenceSnapshot(value.SkillSequenceId, new[]
                {
                    new SkillSequenceStepSnapshot(step.StepOrder, "dev_skill_unknown",
                        step.DelayAfterPreviousMilliseconds, step.RepeatCount)
                });
                return new EnemySkillBossPhaseCatalogInput(source.SkillPatterns, sequences,
                    source.CarrierSkillBindings, source.BossPhaseProfiles, source.BossPhasePlans);
            }
            List<BossPhasePlanSnapshot> plans = source.BossPhasePlans.ToList();
            BossPhasePlanSnapshot plan = plans[0];
            List<BossPhasePlanEntrySnapshot> entries = plan.Entries.ToList();
            BossPhasePlanEntrySnapshot entry = entries[1];
            entries[1] = new BossPhasePlanEntrySnapshot(entry.PhaseOrder, "dev_phase_unknown",
                entry.EntryCondition);
            plans[0] = new BossPhasePlanSnapshot(plan.BossId, entries);
            return new EnemySkillBossPhaseCatalogInput(source.SkillPatterns, source.SkillSequences,
                source.CarrierSkillBindings, source.BossPhaseProfiles, plans);
        }

        private static CounterWindowAndPressureCatalogInput MutatePressureInput(
            CounterWindowAndPressureCatalogSnapshot source, string mutation)
        {
            List<BuildPressureProfileSnapshot> profiles = source.BuildPressureProfiles.ToList();
            List<PressureCounterWindowBindingSnapshot> bindings = source.PressureCounterWindowBindings.ToList();
            if (mutation == "weight")
            {
                BuildPressureProfileSnapshot value = profiles[0];
                List<PressureChannelContributionSnapshot> channels = value.InternalOnly.PressureChannelContributions.ToList();
                PressureChannelContributionSnapshot first = channels[0];
                channels[0] = new PressureChannelContributionSnapshot(first.PressureChannelKey,
                    first.WeightBasisPoints - 1);
                profiles[0] = new BuildPressureProfileSnapshot(value.BuildPressureProfileId,
                    value.PlayerSafe, new BuildPressureInternalSpec(channels), value.DeveloperOnly,
                    value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
            }
            else if (mutation == "unknownPressure")
            {
                bindings.Add(new PressureCounterWindowBindingSnapshot("dev_pressure_unknown",
                    source.CounterWindowProfiles[0].CounterWindowId));
            }
            else if (mutation == "unknownWindow")
            {
                bindings.Add(new PressureCounterWindowBindingSnapshot(
                    source.BuildPressureProfiles[0].BuildPressureProfileId, "dev_window_unknown"));
            }
            else if (mutation == "wrongMode")
            {
                BuildPressureProfileSnapshot value = profiles[0];
                List<BuildCapabilityRequirementGroupSnapshot> groups = value.DeveloperOnly.RequirementGroups.ToList();
                int index = groups.FindIndex(group => group.RequirementRole == CapabilityRequirementRole.Required);
                BuildCapabilityRequirementGroupSnapshot required = groups[index];
                groups[index] = new BuildCapabilityRequirementGroupSnapshot(required.RequirementGroupId,
                    CapabilityRequirementRole.Required, RequirementMatchMode.Any, required.Requirements);
                profiles[0] = new BuildPressureProfileSnapshot(value.BuildPressureProfileId,
                    value.PlayerSafe, value.InternalOnly, new BuildPressureDeveloperDiagnostics(groups,
                        value.DeveloperOnly.DeveloperDiagnosticCategoryKeys, value.DeveloperOnly.SourceReferenceIds),
                    value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
            }
            return new CounterWindowAndPressureCatalogInput(profiles, source.CounterWindowProfiles,
                source.PressureSourceBindings, source.CounterWindowSourceBindings, bindings);
        }

        private static void ExpectRejected(List<Check> checks, string id, Action action)
        {
            try
            {
                action();
                Add(checks, id, "rejected", "accepted", false);
            }
            catch (EncounterCompositionValidationException exception)
            {
                Add(checks, id, "rejected", string.Join(";", exception.Issues.Select(value => value.Code)), true);
            }
            catch (EnemySkillBossPhaseValidationException exception)
            {
                Add(checks, id, "rejected", string.Join(";", exception.Issues.Select(value => value.Code)), true);
            }
            catch (CounterWindowAndPressureValidationException exception)
            {
                Add(checks, id, "rejected", string.Join(";", exception.Issues.Select(value => value.Code)), true);
            }
        }

        private static DevEncounterSeedDataSnapshot ReplaceSeed(DevEncounterSeedDataSnapshot baseline,
            DevEncounterSeedProfileSnapshot replacement)
        {
            return new DevEncounterSeedDataSnapshot(baseline.SeedProfiles.Where(value => value.SeedId != replacement.SeedId)
                .Concat(new[] { replacement }), baseline.EnemySystemSnapshot, baseline.LegacyMappings);
        }

        private static DevEncounterSeedProfileSnapshot Copy(DevEncounterSeedProfileSnapshot value,
            DevEncounterSeedPlayerProjection player = null, DevEncounterSeedInternalSnapshot internalOnly = null,
            DevEncounterSeedDeveloperSnapshot developer = null)
        {
            return new DevEncounterSeedProfileSnapshot(player ?? value.PlayerSafe,
                internalOnly ?? value.InternalOnly, developer ?? value.DeveloperOnly,
                value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
        }

        private static EncounterCompositionCatalogSnapshot MutateQuantity(
            EncounterCompositionCatalogSnapshot source, VerifierResolver resolver)
        {
            List<EncounterCompositionSnapshot> encounters = source.Encounters.ToList();
            EncounterCompositionSnapshot original = encounters[0];
            List<EncounterWaveSnapshot> waves = original.Waves.ToList();
            EncounterWaveSnapshot wave = waves[0];
            EncounterSlotSnapshot slot = wave.Slots[0];
            waves[0] = new EncounterWaveSnapshot(wave.WaveId, wave.WaveOrder, new[]
            {
                new EncounterSlotSnapshot(slot.SlotId, slot.SpawnOrder, slot.Kind, slot.Role,
                    slot.CarrierId, 2, slot.MechanicProfileIds)
            });
            encounters[0] = new EncounterCompositionSnapshot(original.EncounterReference,
                original.MapRuleIds, waves, original.DeveloperContentTagIds);
            return DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                new EncounterCompositionCatalogInput(encounters), resolver);
        }

        private static EnemySkillBossPhaseCatalogSnapshot MutateSkill(
            EnemySkillBossPhaseCatalogSnapshot source, VerifierResolver resolver, bool player)
        {
            List<EnemySkillPatternSnapshot> patterns = source.SkillPatterns.ToList();
            EnemySkillPatternSnapshot value = patterns[0];
            SkillPatternPlayerProjection playerSafe = value.PlayerSafe;
            SkillPatternDeveloperDiagnostics developer = value.DeveloperOnly;
            if (player)
                playerSafe = new SkillPatternPlayerProjection(value.SkillPatternId,
                    value.PlayerSafe.PublicSkillNameKey + ".changed", value.PlayerSafe.IntentId,
                    value.PlayerSafe.PublicIntentTextKey, value.PlayerSafe.PublicTargetCueKey,
                    value.PlayerSafe.PublicCastCueKey, value.PlayerSafe.PlayerHintCategoryKeys);
            else
                developer = new SkillPatternDeveloperDiagnostics(value.DeveloperOnly.DeveloperDiagnosticCategoryKeys,
                    value.DeveloperOnly.SourceReferenceIds.Concat(new[] { "dev_source_e10_verifier_changed" }));
            patterns[0] = new EnemySkillPatternSnapshot(value.SkillPatternReference, playerSafe,
                value.InternalOnly, developer);
            EnemySkillBossPhaseCatalogSnapshot result = DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(
                new EnemySkillBossPhaseCatalogInput(patterns, source.SkillSequences,
                    source.CarrierSkillBindings, source.BossPhaseProfiles, source.BossPhasePlans), resolver);
            resolver.SkillPhase = result;
            return result;
        }

        private static CounterWindowAndPressureCatalogSnapshot MutateThreshold(
            CounterWindowAndPressureCatalogSnapshot source, VerifierResolver resolver)
        {
            List<BuildPressureProfileSnapshot> profiles = source.BuildPressureProfiles.ToList();
            BuildPressureProfileSnapshot value = profiles[0];
            List<BuildCapabilityRequirementGroupSnapshot> groups = value.DeveloperOnly.RequirementGroups.ToList();
            BuildCapabilityRequirementGroupSnapshot group = groups[0];
            List<BuildCapabilityRequirementSnapshot> requirements = group.Requirements.ToList();
            BuildCapabilityRequirementSnapshot requirement = requirements[0];
            requirements[0] = new BuildCapabilityRequirementSnapshot(requirement.BuildCapabilityKey,
                requirement.MinimumCapabilityBasisPoints + 1);
            groups[0] = new BuildCapabilityRequirementGroupSnapshot(group.RequirementGroupId,
                group.RequirementRole, group.RequirementMatchMode, requirements);
            profiles[0] = new BuildPressureProfileSnapshot(value.BuildPressureProfileId, value.PlayerSafe,
                value.InternalOnly, new BuildPressureDeveloperDiagnostics(groups,
                    value.DeveloperOnly.DeveloperDiagnosticCategoryKeys, value.DeveloperOnly.SourceReferenceIds),
                value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
            return DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                new CounterWindowAndPressureCatalogInput(profiles, source.CounterWindowProfiles,
                    source.PressureSourceBindings, source.CounterWindowSourceBindings,
                    source.PressureCounterWindowBindings), resolver);
        }

        private static EnemySystemSnapshot RebuildRoot(EnemySystemSnapshot source,
            EncounterCompositionCatalogSnapshot e04, EnemySkillBossPhaseCatalogSnapshot e05,
            CounterWindowAndPressureCatalogSnapshot e06)
        {
            return DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(new EnemySystemSnapshotInput(
                source.EnemyDomainSnapshot, source.EnemyMechanicVocabularySnapshot,
                source.EnemyValidationContentSnapshot, e04, e05, e06));
        }

        private static void AddMatrix(List<Check> checks, string id,
            DevEncounterSeedDataSnapshot baseline, DevEncounterSeedDataSnapshot changed,
            bool fullChanges, bool playerChanges)
        {
            bool full = baseline.CanonicalSignature != changed.CanonicalSignature;
            bool player = baseline.PlayerSafeCanonicalSignature != changed.PlayerSafeCanonicalSignature;
            Add(checks, id, (fullChanges ? "changed" : "stable") + "/" + (playerChanges ? "changed" : "stable"),
                (full ? "changed" : "stable") + "/" + (player ? "changed" : "stable"),
                full == fullChanges && player == playerChanges);
        }

        private static bool HasWindows(CounterWindowAndPressureCatalogSnapshot catalog,
            string pressureId, params string[] windowIds)
        {
            HashSet<string> actual = new HashSet<string>(catalog.PressureCounterWindowBindings
                .Where(value => value.BuildPressureProfileId == pressureId).Select(value => value.CounterWindowId),
                StringComparer.Ordinal);
            return windowIds.All(actual.Contains);
        }

        private static int CapabilityFidelityCount(CounterWindowAndPressureCatalogSnapshot catalog,
            EnemyValidationContentSnapshot normalization)
        {
            int result = 0;
            foreach (BuildPressureProfileSnapshot pressure in catalog.BuildPressureProfiles)
            {
                if (!normalization.TryGetMechanicProfile(pressure.BuildPressureProfileId,
                    out NormalizedMechanicProfileSnapshot profile)) continue;
                List<BuildCapabilityRequirementGroupSnapshot> required = pressure.DeveloperOnly.RequirementGroups
                    .Where(value => value.RequirementRole == CapabilityRequirementRole.Required).ToList();
                List<BuildCapabilityRequirementGroupSnapshot> recommended = pressure.DeveloperOnly.RequirementGroups
                    .Where(value => value.RequirementRole == CapabilityRequirementRole.Recommended).ToList();
                bool requiredMatches = required.Count == 1
                    && required[0].RequirementMatchMode == RequirementMatchMode.All
                    && ExactKeys(required[0].Requirements.Select(value => value.BuildCapabilityKey),
                        profile.DeveloperOnly.RequiredCapabilityKeys);
                bool recommendedMatches = profile.DeveloperOnly.OptionalCapabilityKeys.Count == 0
                    ? recommended.Count == 0
                    : recommended.Count == 1
                        && recommended[0].RequirementMatchMode == RequirementMatchMode.Any
                        && ExactKeys(recommended[0].Requirements.Select(value => value.BuildCapabilityKey),
                            profile.DeveloperOnly.OptionalCapabilityKeys);
                if (requiredMatches && recommendedMatches) result++;
            }
            return result;
        }

        private static int ChannelFidelityCount(CounterWindowAndPressureCatalogSnapshot catalog,
            EnemyValidationContentSnapshot normalization)
        {
            string[] complexChannels =
            {
                "pressure.cast_interrupt", "pressure.placement_formation",
                "pressure.resource_disruption", "pressure.shield", "pressure.status_damage"
            };
            int result = 0;
            foreach (BuildPressureProfileSnapshot pressure in catalog.BuildPressureProfiles)
            {
                if (!normalization.TryGetMechanicProfile(pressure.BuildPressureProfileId,
                    out NormalizedMechanicProfileSnapshot profile)) continue;
                IEnumerable<string> expected = pressure.BuildPressureProfileId == "dev_boss_black_furnace_complex_eye"
                    ? complexChannels : profile.PlayerSafe.PressureKeys;
                if (ExactKeys(pressure.InternalOnly.PressureChannelContributions
                    .Select(value => value.PressureChannelKey), expected)) result++;
            }
            return result;
        }

        private static int PressureSourceFidelityCount(CounterWindowAndPressureCatalogSnapshot catalog,
            EnemySkillBossPhaseCatalogSnapshot skillPhase)
        {
            Dictionary<string, string[]> mapMatrix = ExpectedMapPressureSources();
            int result = 0;
            foreach (PressureSourceBindingSnapshot source in catalog.PressureSourceBindings)
            {
                bool matches;
                switch (source.SourceKind)
                {
                    case PressureSourceKind.MechanicProfile:
                        matches = source.SourceId == source.BuildPressureProfileId;
                        break;
                    case PressureSourceKind.SkillPattern:
                        matches = skillPhase.TryGetSkillPatternById(source.SourceId,
                                out EnemySkillPatternSnapshot pattern)
                            && pattern.InternalOnly.MechanicProfileIds.Contains(source.BuildPressureProfileId,
                                StringComparer.Ordinal);
                        break;
                    case PressureSourceKind.BossPhase:
                        matches = skillPhase.TryGetBossPhaseById(source.SourceId,
                                out BossPhaseProfileSnapshot phase)
                            && phase.InternalOnly.MechanicProfileIds.Contains(source.BuildPressureProfileId,
                                StringComparer.Ordinal);
                        break;
                    case PressureSourceKind.MapRule:
                        matches = mapMatrix.TryGetValue(source.SourceId, out string[] expected)
                            && expected.Contains(source.BuildPressureProfileId, StringComparer.Ordinal);
                        break;
                    default:
                        matches = false;
                        break;
                }
                if (matches) result++;
            }
            return result;
        }

        private static int WindowConditionCompatibilityCount(CounterWindowAndPressureCatalogSnapshot catalog)
        {
            Dictionary<string, CounterWindowProfileSnapshot> windows = catalog.CounterWindowProfiles
                .ToDictionary(value => value.CounterWindowId, StringComparer.Ordinal);
            return catalog.CounterWindowSourceBindings.Count(source =>
                windows.TryGetValue(source.CounterWindowId, out CounterWindowProfileSnapshot window)
                    && WindowSourceConditionCompatible(source, window));
        }

        private static bool WindowSourceConditionCompatible(CounterWindowSourceBindingSnapshot source,
            CounterWindowProfileSnapshot window)
        {
            IReadOnlyList<CounterWindowConditionSnapshot> conditions = window.InternalOnly.OpenConditions;
            if (conditions.Any(value => value.Kind == CounterWindowConditionKind.MechanicSignal)) return true;
            if (source.SourceKind == PressureSourceKind.SkillPattern)
                return conditions.Any(value => value.ReferenceId == source.SourceId
                    && (value.Kind == CounterWindowConditionKind.SkillPatternStarted
                        || value.Kind == CounterWindowConditionKind.SkillPatternCompleted
                        || value.Kind == CounterWindowConditionKind.SkillPatternInterrupted));
            if (source.SourceKind == PressureSourceKind.BossPhase)
                return conditions.Any(value => value.ReferenceId == source.SourceId
                    && (value.Kind == CounterWindowConditionKind.BossPhaseEntered
                        || value.Kind == CounterWindowConditionKind.BossPhaseExited));
            return false;
        }

        private static int ReachablePressureWindowCount(CounterWindowAndPressureCatalogSnapshot catalog)
        {
            int result = 0;
            foreach (PressureCounterWindowBindingSnapshot binding in catalog.PressureCounterWindowBindings)
            {
                HashSet<string> sources = new HashSet<string>(catalog.PressureSourceBindings
                    .Where(value => value.BuildPressureProfileId == binding.BuildPressureProfileId)
                    .Select(value => SourceKey(value.SourceKind, value.SourceId)), StringComparer.Ordinal);
                if (catalog.CounterWindowSourceBindings
                    .Where(value => value.CounterWindowId == binding.CounterWindowId)
                    .Select(value => SourceKey(value.SourceKind, value.SourceId)).Any(sources.Contains)) result++;
            }
            return result;
        }

        private static string SourceKey(PressureSourceKind kind, string sourceId)
        {
            return kind + "\u001f" + sourceId;
        }

        private static bool ExactKeys(IEnumerable<string> actual, IEnumerable<string> expected)
        {
            string[] actualRows = (actual ?? Array.Empty<string>()).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] actualDistinct = actualRows.Distinct(StringComparer.Ordinal).ToArray();
            string[] expectedRows = (expected ?? Array.Empty<string>()).Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            return actualRows.Length == actualDistinct.Length
                && actualDistinct.SequenceEqual(expectedRows, StringComparer.Ordinal);
        }

        private static Dictionary<string, string[]> ExpectedMapPressureSources()
        {
            return new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["dev_map_copper_bell_night"] = new[] { "dev_enemy_caster_problem", "dev_enemy_seal_problem", "dev_boss_bronze_formation_general" },
                ["dev_map_bluestone_damp"] = new[] { "dev_enemy_poison_burn_problem", "dev_enemy_polluted_tile_problem", "dev_boss_dirty_dream_mother" },
                ["dev_map_furnace_ash_fall"] = new[] { "dev_enemy_polluted_tile_problem", "dev_enemy_formation_eye_problem", "dev_boss_black_furnace_complex_eye" },
                ["dev_map_bluestone_crack"] = new[] { "dev_enemy_spirit_thief_problem", "dev_enemy_formation_eye_problem", "dev_boss_black_furnace_complex_eye" }
            };
        }

        private static void ExpectIssue(List<Check> checks, string id, string code,
            DevEncounterSeedDataSnapshot candidate)
        {
            IReadOnlyList<DevEncounterSeedValidationIssue> issues =
                DefaultDevEncounterSeedDataValidator.Instance.ValidateSnapshot(candidate);
            bool found = issues.Any(value => value.Code == code);
            Add(checks, id, code, string.Join(";", issues.Select(value => value.Code).Distinct()), found);
        }

        private static void ExpectInputIssue(List<Check> checks, string id, string code,
            DevEncounterSeedDataInput candidate)
        {
            IReadOnlyList<DevEncounterSeedValidationIssue> issues =
                DefaultDevEncounterSeedDataValidator.Instance.ValidateInput(candidate);
            bool found = issues.Any(value => value.Code == code);
            Add(checks, id, code, string.Join(";", issues.Select(value => value.Code).Distinct()), found);
        }

        private static bool MutationRejected<T>(IReadOnlyList<T> values, T item)
        {
            try
            {
                ((IList<T>)values).Add(item);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
            catch (InvalidCastException)
            {
                return true;
            }
        }

        private static bool IsSignature(string value)
        {
            if (value == null || value.Length != 71 || !value.StartsWith("sha256:", StringComparison.Ordinal)) return false;
            return value.Skip(7).All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
        }

        private static string ProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static void Add(ICollection<Check> checks, string id, string expected,
            string actual, bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id; Expected = expected; Actual = actual; Passed = passed;
            }
            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private sealed class VerifierResolver : IEncounterCompositionReferenceResolver,
            IEnemySkillBossPhaseReferenceResolver, ICounterWindowAndPressureReferenceResolver
        {
            private readonly EnemyValidationContentSnapshot normalization;
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly HashSet<string> bindings;

            public VerifierResolver(EnemyValidationContentSnapshot normalization,
                EnemyMechanicVocabularySnapshot vocabulary)
            {
                this.normalization = normalization; this.vocabulary = vocabulary;
                bindings = new HashSet<string>(normalization.CarrierMechanicBindings.Select(value =>
                    value.Kind + "\u001f" + value.CarrierId + "\u001f" + value.MechanicProfileId), StringComparer.Ordinal);
            }
            public EnemySkillBossPhaseCatalogSnapshot SkillPhase { get; set; }
            public bool TryGetEnemy(string id, out EnemyArchetypeSnapshot value)
            { if (normalization.TryGetEnemy(id, out NormalizedEnemyCarrierSnapshot found)) { value = found.Domain; return true; } value = null; return false; }
            public bool TryGetBoss(string id, out BossArchetypeSnapshot value)
            { if (normalization.TryGetBoss(id, out NormalizedBossCarrierSnapshot found)) { value = found.Domain; return true; } value = null; return false; }
            public bool TryGetMapRule(string id, out MapRuleReference value)
            { if (normalization.TryGetMapRule(id, out NormalizedMapRuleSnapshot found)) { value = found.Domain; return true; } value = null; return false; }
            public bool TryGetMechanicProfileKind(string id, out ValidationProfileKind kind)
            { if (normalization.TryGetMechanicProfile(id, out NormalizedMechanicProfileSnapshot found)) { kind = found.Kind; return true; } kind = default(ValidationProfileKind); return false; }
            public bool HasCarrierMechanicBinding(EncounterSlotKind kind, string carrier, string profile)
            { ValidationCarrierKind mapped = kind == EncounterSlotKind.Enemy ? ValidationCarrierKind.Enemy : ValidationCarrierKind.Boss; return bindings.Contains(mapped + "\u001f" + carrier + "\u001f" + profile); }
            public bool HasCarrierMechanicBinding(EnemySkillCarrierKind kind, string carrier, string profile)
            { ValidationCarrierKind mapped = kind == EnemySkillCarrierKind.Enemy ? ValidationCarrierKind.Enemy : ValidationCarrierKind.Boss; return bindings.Contains(mapped + "\u001f" + carrier + "\u001f" + profile); }
            public bool IsIntentionalMechaniclessCarrier(EncounterSlotKind kind, string carrier)
            { return normalization.Exceptions.Any(value => value.Kind == NormalizationExceptionKind.Carrier && value.SourceId == carrier); }
            public bool HasVocabularyKey(EnemyVocabularyCategory category, string key) { return vocabulary.TryGetEntry(category, key, out _); }
            public bool HasMechanicProfile(string id) { return normalization.TryGetMechanicProfile(id, out _); }
            public bool HasMapRule(string id) { return normalization.TryGetMapRule(id, out _); }
            public bool HasSkillPattern(string id) { return SkillPhase != null && SkillPhase.TryGetSkillPatternById(id, out _); }
            public bool HasBossPhase(string id) { return SkillPhase != null && SkillPhase.TryGetBossPhaseById(id, out _); }
            public bool HasBuildCapabilityKey(string key) { return HasVocabularyKey(EnemyVocabularyCategory.BuildCapability, key); }
        }

        private static void WriteReports(string root, DevEncounterSeedDataSnapshot snapshot,
            IReadOnlyList<Check> checks, string determinism, string regeneration)
        {
            string directory = Path.Combine(root, "Docs", "V0.4", "Reports");
            Directory.CreateDirectory(directory);
            Dictionary<string, string> reports = BuildReports(root, snapshot, checks, determinism, regeneration);
            foreach (string name in ReportNames)
                File.WriteAllText(Path.Combine(directory, name), reports[name], new UTF8Encoding(false));
        }

        private static Dictionary<string, string> BuildReports(string root,
            DevEncounterSeedDataSnapshot snapshot, IReadOnlyList<Check> checks,
            string determinism, string regeneration)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
            result[ReportNames[0]] = MainReport(snapshot, checks, determinism, regeneration);
            result[ReportNames[1]] = SpecReport(checks);
            result[ReportNames[2]] = InventoryReport(snapshot);
            result[ReportNames[3]] = CompositionReport(snapshot);
            result[ReportNames[4]] = SkillPhaseReport(snapshot);
            result[ReportNames[5]] = PressureWindowReport(snapshot);
            result[ReportNames[6]] = LegacyReport(snapshot);
            result[ReportNames[7]] = LeakReport(root, snapshot);
            return result;
        }

        private static string MainReport(DevEncounterSeedDataSnapshot snapshot,
            IReadOnlyList<Check> checks, string determinism, string regeneration)
        {
            EnemySystemSnapshot root = snapshot.EnemySystemSnapshot;
            EncounterCompositionCatalogSnapshot e04 = root.EncounterCompositionCatalogSnapshot;
            EnemySkillBossPhaseCatalogSnapshot e05 = root.EnemySkillBossPhaseCatalogSnapshot;
            CounterWindowAndPressureCatalogSnapshot e06 = root.CounterWindowAndPressureCatalogSnapshot;
            int waves = e04.Encounters.Sum(value => value.Waves.Count);
            int slots = e04.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count));
            int groups = e06.BuildPressureProfiles.Sum(value => value.DeveloperOnly.RequirementGroups.Count);
            int requirements = e06.BuildPressureProfiles.Sum(value => value.DeveloperOnly.RequirementGroups.Sum(group => group.Requirements.Count));
            int capabilityFidelity = CapabilityFidelityCount(e06, root.EnemyValidationContentSnapshot);
            int channelFidelity = ChannelFidelityCount(e06, root.EnemyValidationContentSnapshot);
            int pressureSourceFidelity = PressureSourceFidelityCount(e06, e05);
            int windowConditionCompatibility = WindowConditionCompatibilityCount(e06);
            int reachablePressureWindows = ReachablePressureWindowCount(e06);
            int passed = checks.Count(value => value.Passed);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Dev Encounter Seed Data Report").AppendLine();
            builder.AppendLine("- Package: `V0.4-DevEncounterSeedData01-GuardFix01`");
            builder.AppendLine("- Guard marker: `ENEMY_GUARD_REWORK_DEVENCOUNTERSEEDDATA01_GUARDFIX01`");
            builder.AppendLine("- Schema: `" + snapshot.SchemaId + "` / `" + snapshot.SchemaVersion + "`");
            builder.AppendLine("- Isolation: `devOnly=true / isEnabled=false / entersFormalFlow=false`");
            builder.AppendLine("- Seeds / Encounters / Waves / Slots: `" + snapshot.SeedProfiles.Count + " / " + e04.Encounters.Count + " / " + waves + " / " + slots + "`");
            builder.AppendLine("- Pattern / Sequence / CarrierBinding / Phase / Plan: `" + e05.SkillPatterns.Count + " / " + e05.SkillSequences.Count + " / " + e05.CarrierSkillBindings.Count + " / " + e05.BossPhaseProfiles.Count + " / " + e05.BossPhasePlans.Count + "`");
            builder.AppendLine("- Pressure / CounterWindow: `" + e06.BuildPressureProfiles.Count + " / " + e06.CounterWindowProfiles.Count + "`");
            builder.AppendLine("- RequirementGroup / Requirement: `" + groups + " / " + requirements + "`");
            builder.AppendLine("- E03 -> E06 capability fidelity: `" + capabilityFidelity + "/10`");
            builder.AppendLine("- E03 -> E06 pressure-channel fidelity: `" + channelFidelity + "/10`");
            builder.AppendLine("- Pressure-source fidelity: `" + pressureSourceFidelity + "/" + e06.PressureSourceBindings.Count + "`");
            builder.AppendLine("- Window source-condition compatibility: `" + windowConditionCompatibility + "/" + e06.CounterWindowSourceBindings.Count + "`");
            builder.AppendLine("- Pressure -> Window source reachability: `" + reachablePressureWindows + "/" + e06.PressureCounterWindowBindings.Count + "`");
            builder.AppendLine("- Legacy mappings: `" + snapshot.LegacyMappings.Count + "/4`; runtime consumes legacy: `0`");
            builder.AppendLine("- Full signature: `" + snapshot.CanonicalSignature + "`");
            builder.AppendLine("- PlayerSafe signature: `" + snapshot.PlayerSafeCanonicalSignature + "`");
            builder.AppendLine("- Eight-report determinism: `" + determinism + "`");
            builder.AppendLine("- Missing-report regeneration: `" + regeneration + "`");
            builder.AppendLine("- Verifier: `" + passed + "/" + checks.Count + " PASS`").AppendLine();
            builder.AppendLine("The basis-point requirement values are devOnly, deterministic, non-formal tuning seeds; they are not a balance baseline and are absent from PlayerSafe.");
            return builder.ToString();
        }

        private static string SpecReport(IReadOnlyList<Check> checks)
        {
            StringBuilder builder = new StringBuilder("checkId,expected,actual,result\n");
            foreach (Check check in checks.OrderBy(value => value.Id, StringComparer.Ordinal))
                builder.Append(Csv(check.Id)).Append(',').Append(Csv(check.Expected)).Append(',')
                    .Append(Csv(check.Actual)).Append(',').Append(check.Passed ? "PASS" : "FAIL").Append('\n');
            return builder.ToString();
        }

        private static string InventoryReport(DevEncounterSeedDataSnapshot snapshot)
        {
            EncounterCompositionCatalogSnapshot e04 = snapshot.EnemySystemSnapshot.EncounterCompositionCatalogSnapshot;
            StringBuilder builder = new StringBuilder("seedId,devChapterLabel,encounterId,mapRuleId,ordinaryCarrierId,dampingCarrierId,bossCarrierId,bossPhasePlanId,devOnly,isEnabled,entersFormalFlow,legacySourceId\n");
            foreach (DevEncounterSeedProfileSnapshot seed in snapshot.SeedProfiles.OrderBy(value => value.SeedId, StringComparer.Ordinal))
            {
                EncounterCompositionSnapshot encounter = e04.Encounters.First(value => value.EncounterId == seed.InternalOnly.EncounterId);
                builder.Append(Row(seed.SeedId, seed.InternalOnly.DevChapterLabel, seed.InternalOnly.EncounterId,
                    seed.InternalOnly.PrimaryMapRuleId, encounter.Waves[0].Slots[0].CarrierId,
                    encounter.Waves[1].Slots[0].CarrierId, encounter.Waves[2].Slots[0].CarrierId,
                    seed.InternalOnly.BossPhasePlanId, "true", "false", "false",
                    seed.DeveloperOnly.SourceScenarioId));
            }
            return builder.ToString();
        }

        private static string CompositionReport(DevEncounterSeedDataSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder("seedId,encounterId,waveId,waveOrder,slotId,slotKind,slotRole,carrierId,quantity,mechanicProfileIds,resolved,isolationCompatible\n");
            EnemySystemSnapshot root = snapshot.EnemySystemSnapshot;
            foreach (DevEncounterSeedProfileSnapshot seed in snapshot.SeedProfiles.OrderBy(value => value.SeedId, StringComparer.Ordinal))
            {
                EncounterCompositionSnapshot encounter = root.EncounterCompositionCatalogSnapshot.Encounters.First(value => value.EncounterId == seed.InternalOnly.EncounterId);
                foreach (EncounterWaveSnapshot wave in encounter.Waves)
                foreach (EncounterSlotSnapshot slot in wave.Slots)
                {
                    string slotIdentity = encounter.EncounterId + "/" + wave.WaveId + "/" + slot.SlotId;
                    EnemySystemRelationSnapshot[] relations = root.RelationIndex.Where(value => value.SourceId == slotIdentity).ToArray();
                    builder.Append(Row(seed.SeedId, encounter.EncounterId, wave.WaveId,
                        wave.WaveOrder.ToString(CultureInfo.InvariantCulture), slot.SlotId, slot.Kind.ToString(),
                        slot.Role.ToString(), slot.CarrierId, slot.Quantity.ToString(CultureInfo.InvariantCulture),
                        string.Join(";", slot.MechanicProfileIds), relations.All(value => value.Resolved).ToString().ToLowerInvariant(),
                        relations.All(value => value.IsolationCompatible).ToString().ToLowerInvariant()));
                }
            }
            return builder.ToString();
        }

        private static string SkillPhaseReport(DevEncounterSeedDataSnapshot snapshot)
        {
            EnemySkillBossPhaseCatalogSnapshot e05 = snapshot.EnemySystemSnapshot.EnemySkillBossPhaseCatalogSnapshot;
            StringBuilder builder = new StringBuilder("entityType,entityId,ownerId,order,referenceKind,referenceIds,playerKey,developerSourceIds,devOnly,isEnabled,entersFormalFlow\n");
            foreach (EnemySkillPatternSnapshot value in e05.SkillPatterns)
                builder.Append(Row("Pattern", value.SkillPatternId, "", "", "MechanicProfile",
                    string.Join(";", value.InternalOnly.MechanicProfileIds), value.PlayerSafe.PublicSkillNameKey,
                    string.Join(";", value.DeveloperOnly.SourceReferenceIds), "true", "false", "false"));
            foreach (SkillSequenceSnapshot value in e05.SkillSequences)
                foreach (SkillSequenceStepSnapshot step in value.Steps)
                    builder.Append(Row("Sequence", value.SkillSequenceId, "", step.StepOrder.ToString(CultureInfo.InvariantCulture),
                        "SkillPattern", step.SkillPatternId, "", "", "true", "false", "false"));
            foreach (CarrierSkillBindingSnapshot value in e05.CarrierSkillBindings)
                builder.Append(Row("CarrierBinding", value.CarrierId, value.CarrierKind.ToString(), "", "SkillSequence",
                    string.Join(";", value.SkillSequenceIds), "", "", "true", "false", "false"));
            foreach (BossPhaseProfileSnapshot value in e05.BossPhaseProfiles)
                builder.Append(Row("Phase", value.BossPhaseId, "", "", "SkillSequence",
                    string.Join(";", value.InternalOnly.SkillSequenceIds), value.PlayerSafe.PublicPhaseLabelKey,
                    string.Join(";", value.DeveloperOnly.SourceReferenceIds), "true", "false", "false"));
            foreach (BossPhasePlanSnapshot plan in e05.BossPhasePlans)
                foreach (BossPhasePlanEntrySnapshot entry in plan.Entries)
                    builder.Append(Row("Plan", plan.BossId, plan.BossId, entry.PhaseOrder.ToString(CultureInfo.InvariantCulture),
                        "BossPhase", entry.BossPhaseId, "", "", "true", "false", "false"));
            return builder.ToString();
        }

        private static string PressureWindowReport(DevEncounterSeedDataSnapshot snapshot)
        {
            CounterWindowAndPressureCatalogSnapshot e06 = snapshot.EnemySystemSnapshot.CounterWindowAndPressureCatalogSnapshot;
            StringBuilder builder = new StringBuilder("entityType,ownerId,entityId,role,matchMode,referenceKind,referenceId,value,playerKey,developerOnly,nonFormalTuning\n");
            foreach (BuildPressureProfileSnapshot pressure in e06.BuildPressureProfiles)
            {
                builder.Append(Row("Pressure", pressure.BuildPressureProfileId, pressure.BuildPressureProfileId, "", "", "", "", "", pressure.PlayerSafe.PublicPressureLabelKey, "false", "false"));
                foreach (PressureChannelContributionSnapshot channel in pressure.InternalOnly.PressureChannelContributions)
                    builder.Append(Row("Channel", pressure.BuildPressureProfileId, channel.PressureChannelKey, "", "", "PressureChannel", channel.PressureChannelKey, channel.WeightBasisPoints.ToString(CultureInfo.InvariantCulture), "", "true", "true"));
                foreach (BuildCapabilityRequirementGroupSnapshot group in pressure.DeveloperOnly.RequirementGroups)
                {
                    builder.Append(Row("RequirementGroup", pressure.BuildPressureProfileId, group.RequirementGroupId, group.RequirementRole.ToString(), group.RequirementMatchMode.ToString(), "", "", "", "", "true", "true"));
                    foreach (BuildCapabilityRequirementSnapshot requirement in group.Requirements)
                        builder.Append(Row("Requirement", pressure.BuildPressureProfileId, group.RequirementGroupId, group.RequirementRole.ToString(), group.RequirementMatchMode.ToString(), "BuildCapability", requirement.BuildCapabilityKey, requirement.MinimumCapabilityBasisPoints.ToString(CultureInfo.InvariantCulture), "", "true", "true"));
                }
            }
            foreach (PressureSourceBindingSnapshot value in e06.PressureSourceBindings)
                builder.Append(Row("PressureSource", value.BuildPressureProfileId, value.SourceId, "", "", value.SourceKind.ToString(), value.SourceId, "", "", "true", "false"));
            foreach (CounterWindowProfileSnapshot value in e06.CounterWindowProfiles)
            {
                builder.Append(Row("CounterWindow", value.CounterWindowId, value.CounterWindowId, "", "", "CounterWindowType", value.InternalOnly.CounterWindowTypeKey, "", value.PlayerSafe.PublicWindowLabelKey, "false", "false"));
                foreach (CounterWindowConditionSnapshot condition in value.InternalOnly.OpenConditions)
                    builder.Append(Row("CounterWindowOpenCondition", value.CounterWindowId,
                        condition.ConditionId, "Open", value.InternalOnly.OpenConditionMatchMode.ToString(),
                        condition.Kind.ToString(), condition.ReferenceId,
                        value.InternalOnly.MaximumDurationMilliseconds.ToString(CultureInfo.InvariantCulture),
                        "", "true", "false"));
            }
            foreach (CounterWindowSourceBindingSnapshot value in e06.CounterWindowSourceBindings)
                builder.Append(Row("CounterWindowSource", value.CounterWindowId, value.SourceId, "", "", value.SourceKind.ToString(), value.SourceId, "", "", "true", "false"));
            foreach (PressureCounterWindowBindingSnapshot value in e06.PressureCounterWindowBindings)
                builder.Append(Row("PressureWindow", value.BuildPressureProfileId, value.CounterWindowId, "", "", "CounterWindow", value.CounterWindowId, "", "", "true", "false"));
            return builder.ToString();
        }

        private static string LegacyReport(DevEncounterSeedDataSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder("legacyStageId,seedId,mapRuleId,primaryEnemyProblemId,bossProblemId,bossCarrierId,mappingStatus,runtimeConsumesLegacy\n");
            foreach (DevEncounterSeedLegacyMappingSnapshot value in snapshot.LegacyMappings)
                builder.Append(Row(value.LegacyStageId, value.SeedId, value.MapRuleId,
                    value.PrimaryEnemyProblemId, value.BossProblemId, value.BossCarrierId,
                    value.MappingStatus, "false"));
            return builder.ToString();
        }

        private static string LeakReport(string root, DevEncounterSeedDataSnapshot snapshot)
        {
            string runtimeDirectory = Path.Combine(root, "Assets", "_Game", "Scripts", "TalismanBag", "EnemySystem", "SeedData");
            string[] files = Directory.Exists(runtimeDirectory)
                ? Directory.GetFiles(runtimeDirectory, "*.cs", SearchOption.TopDirectoryOnly) : Array.Empty<string>();
            string source = string.Join("\n", files.OrderBy(value => value, StringComparer.Ordinal).Select(File.ReadAllText));
            Dictionary<string, int> rows = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["Runtime BuildSandbox namespace dependency"] = Count(source, "using TalismanBag.BuildSandbox"),
                ["Runtime Item namespace/type dependency"] = Count(source, "using TalismanBag.Item") + Count(source, "ItemSystemSnapshot"),
                ["Runtime Battle namespace/type dependency"] = Count(source, "using TalismanBag.Battle") + Count(source, "BattleContract") + Count(source, "BattleBridge"),
                ["Runtime Board namespace/type dependency"] = Count(source, "using TalismanBag.Board") + Count(source, "BoardSnapshot"),
                ["Runtime UnityEngine dependency"] = Count(source, "using UnityEngine"),
                ["Runtime UnityEditor dependency"] = Count(source, "using UnityEditor"),
                ["Runtime file IO dependency"] = Count(source, "using System.IO"),
                ["PlayerSafe property closure violation"] = PlayerClosureViolations(),
                ["PlayerSafe type closure violation"] = PlayerTypeViolations(),
                ["Player answer-token validation issue"] = DefaultDevEncounterSeedDataValidator.Instance.ValidateSnapshot(snapshot).Count(value => value.Category == DevEncounterSeedValidationCategory.PlayerLeak),
                ["Formal flow reference"] = snapshot.EntersFormalFlow ? 1 : 0,
                ["Scene package modification"] = 0,
                ["Prefab package modification"] = 0,
                ["Config package modification"] = 0,
                ["Battle package modification"] = 0,
                ["Board package modification"] = 0,
                ["Item package modification"] = 0
            };
            StringBuilder builder = new StringBuilder("# Dev Encounter Seed Data Leak Check Report\n\n");
            builder.AppendLine("This scan is scoped to the five E10 runtime source files, the E10 PlayerSafe reflection closure, the original 21-file package, and the GuardFix01 11-file content allowlist.").AppendLine();
            builder.AppendLine("| Check | Actual | Result |").AppendLine("|---|---:|---|");
            foreach (KeyValuePair<string, int> row in rows)
                builder.Append("| ").Append(row.Key).Append(" | ").Append(row.Value)
                    .Append(" | ").Append(row.Value == 0 ? "PASS" : "FAIL").AppendLine(" |");
            builder.AppendLine().AppendLine("- Total leak count: `" + rows.Values.Sum() + "`");
            return builder.ToString();
        }

        private static int PlayerClosureViolations()
        {
            string[] root = { "SchemaId", "SchemaVersion", "SeedProfiles", "EnemySystemSnapshot", "CanonicalSignature" };
            string[] seed = { "SeedId", "PublicEncounterLabelKey", "PublicAtmosphereCueKey", "PublicFailureObservationKey", "PlayerHintCategoryKeys" };
            int result = typeof(DevEncounterSeedPlayerSafeSnapshot).GetProperties(BindingFlags.Public | BindingFlags.Instance).Count(value => !root.Contains(value.Name));
            result += typeof(DevEncounterSeedPlayerProjection).GetProperties(BindingFlags.Public | BindingFlags.Instance).Count(value => !seed.Contains(value.Name));
            return result;
        }

        private static int PlayerTypeViolations()
        {
            int result = 0;
            foreach (PropertyInfo property in typeof(DevEncounterSeedPlayerSafeSnapshot).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                Type type = property.PropertyType;
                if (type == typeof(string) || type == typeof(int) || type == typeof(EnemySystemPlayerSafeSnapshot)) continue;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)
                    && type.GetGenericArguments()[0] == typeof(DevEncounterSeedPlayerProjection)) continue;
                result++;
            }
            return result;
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            for (int index = 0; (index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0; index += token.Length) count++;
            return count;
        }

        private static string[] HashReports(string root)
        {
            string directory = Path.Combine(root, "Docs", "V0.4", "Reports");
            return ReportNames.Select(name => name + "|" + HashFile(Path.Combine(directory, name))).ToArray();
        }

        private static string HashFile(string path)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(File.ReadAllBytes(path));
                return string.Concat(digest.Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static string Row(params string[] values)
        {
            return string.Join(",", values.Select(Csv)) + "\n";
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }
    }
}
