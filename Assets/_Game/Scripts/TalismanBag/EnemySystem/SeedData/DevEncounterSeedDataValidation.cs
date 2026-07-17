using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.SystemSnapshot;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SeedData
{
    public sealed class DefaultDevEncounterSeedDataValidator : IDevEncounterSeedDataValidator
    {
        public static readonly DefaultDevEncounterSeedDataValidator Instance =
            new DefaultDevEncounterSeedDataValidator();

        private static readonly string[] ForbiddenPlayerTokens =
        {
            "capability.", "minimumcapability", "basispoints", "threshold", "required", "recommended",
            "dev_balance_", "legacy", "shape", "preview", "item", "synergy", "dropbias", "drop_bias",
            "randomseed", "answer", "solution", "boss_key", "minimumkeys", "build."
        };

        private static readonly ExpectedSeed[] ExpectedSeeds =
        {
            new ExpectedSeed(DevEncounterSeedDataCatalog.SeedGuardWall, "3-10", "dev_encounter_3_10_guard_wall",
                "dev_map_copper_bell_night", "dev_enemy_caster_chanter", "dev_enemy_caster_problem",
                "dev_enemy_seal_locker", "dev_enemy_seal_problem", "dev_boss_burst_huzhen",
                "dev_boss_bronze_formation_general", "dev_balance_3_10_guard_wall"),
            new ExpectedSeed(DevEncounterSeedDataCatalog.SeedCleanseCorner, "3-10", "dev_encounter_3_10_cleanse_corner",
                "dev_map_bluestone_damp", "dev_enemy_poison_cultist", "dev_enemy_poison_burn_problem",
                "dev_enemy_burning_wisp", "dev_enemy_poison_burn_problem", "dev_boss_debuff_jinge",
                "dev_boss_dirty_dream_mother", "dev_balance_3_10_cleanse_corner"),
            new ExpectedSeed(DevEncounterSeedDataCatalog.SeedFurnaceCore, "4-10", "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall", "dev_enemy_spirit_thief", "dev_enemy_spirit_thief_problem",
                "dev_enemy_formation_eye_jammer", "dev_enemy_formation_eye_problem", "dev_boss_energy_juneng",
                "dev_boss_spirit_thief_core", "dev_balance_4_10_furnace_core"),
            new ExpectedSeed(DevEncounterSeedDataCatalog.SeedThunderFireCross, "4-10", "dev_encounter_4_10_thunder_fire_cross",
                "dev_map_bluestone_crack", "dev_enemy_formation_eye_jammer", "dev_enemy_formation_eye_problem",
                "dev_enemy_spirit_thief", "dev_enemy_spirit_thief_problem", "dev_boss_hybrid_combo",
                "dev_boss_black_furnace_complex_eye", "dev_balance_4_10_thunder_fire_cross")
        };

        public IReadOnlyList<DevEncounterSeedValidationIssue> ValidateInput(DevEncounterSeedDataInput input)
        {
            List<DevEncounterSeedValidationIssue> issues = new List<DevEncounterSeedValidationIssue>();
            if (input == null)
            {
                Add(issues, "E10_NULL_INPUT", DevEncounterSeedValidationCategory.Schema, "input",
                    "DevEncounterSeedDataInput must not be null.");
                return Sort(issues);
            }
            if (input.EnemyMechanicVocabularySnapshot == null)
                Add(issues, "E10_E02_NULL", DevEncounterSeedValidationCategory.Dependency,
                    "input.EnemyMechanicVocabularySnapshot", "E02 vocabulary snapshot is required.");
            if (input.EnemyValidationContentSnapshot == null)
                Add(issues, "E10_E03_NULL", DevEncounterSeedValidationCategory.Dependency,
                    "input.EnemyValidationContentSnapshot", "E03 normalization snapshot is required.");
            if (issues.Count > 0) return Sort(issues);

            if (input.EnemyMechanicVocabularySnapshot.SchemaId != EnemyMechanicVocabularySchema.SchemaId
                || input.EnemyMechanicVocabularySnapshot.SchemaVersion != EnemyMechanicVocabularySchema.SchemaVersion)
                Add(issues, "E10_E02_SCHEMA", DevEncounterSeedValidationCategory.Schema, "input.E02.Schema",
                    "E02 schema id/version must match its authoritative contract.");
            if (input.EnemyValidationContentSnapshot.SchemaId != EnemyValidationNormalizationSchema.SchemaId
                || input.EnemyValidationContentSnapshot.SchemaVersion != EnemyValidationNormalizationSchema.SchemaVersion)
                Add(issues, "E10_E03_SCHEMA", DevEncounterSeedValidationCategory.Schema, "input.E03.Schema",
                    "E03 schema id/version must match its authoritative contract.");
            if (input.EnemyValidationContentSnapshot.DomainSnapshot == null)
                Add(issues, "E10_E03_DOMAIN_NULL", DevEncounterSeedValidationCategory.Dependency,
                    "input.E03.DomainSnapshot", "E01 DomainSnapshot must be obtained from E03.");
            else if (!DevEncounterSeedDataCanonical.IsSignature(
                input.EnemyValidationContentSnapshot.DomainSnapshot.BuildCanonicalSignature()))
                Add(issues, "E10_E03_DOMAIN_SIGNATURE", DevEncounterSeedValidationCategory.Signature,
                    "input.E03.DomainSnapshot", "E03 DomainSnapshot canonical signature is invalid.");
            return Sort(issues);
        }

        public IReadOnlyList<DevEncounterSeedValidationIssue> ValidateSnapshot(DevEncounterSeedDataSnapshot snapshot)
        {
            List<DevEncounterSeedValidationIssue> issues = new List<DevEncounterSeedValidationIssue>();
            if (snapshot == null)
            {
                Add(issues, "E10_NULL_SNAPSHOT", DevEncounterSeedValidationCategory.Schema, "snapshot",
                    "DevEncounterSeedDataSnapshot must not be null.");
                return Sort(issues);
            }

            if (snapshot.SchemaId != DevEncounterSeedDataSchema.SchemaId
                || snapshot.SchemaVersion != DevEncounterSeedDataSchema.SchemaVersion)
                Add(issues, "E10_SCHEMA_MISMATCH", DevEncounterSeedValidationCategory.Schema, "snapshot.Schema",
                    "Schema must be DevEncounterSeedData.v1 version 1.");
            Isolation(issues, "snapshot", snapshot.DevOnly, snapshot.IsEnabled, snapshot.EntersFormalFlow);
            if (snapshot.EnemySystemSnapshot == null)
                Add(issues, "E10_E09_NULL", DevEncounterSeedValidationCategory.Dependency,
                    "snapshot.EnemySystemSnapshot", "E09 EnemySystemSnapshot is required.");
            if (snapshot.PlayerSafe == null)
                Add(issues, "E10_PLAYER_NULL", DevEncounterSeedValidationCategory.PlayerLeak,
                    "snapshot.PlayerSafe", "Player-safe projection is required.");
            if (!DevEncounterSeedDataCanonical.IsSignature(snapshot.CanonicalSignature))
                Add(issues, "E10_FULL_SIGNATURE", DevEncounterSeedValidationCategory.Signature,
                    "snapshot.CanonicalSignature", "Full signature must use lowercase sha256 format.");
            if (!DevEncounterSeedDataCanonical.IsSignature(snapshot.PlayerSafeCanonicalSignature))
                Add(issues, "E10_PLAYER_SIGNATURE", DevEncounterSeedValidationCategory.Signature,
                    "snapshot.PlayerSafeCanonicalSignature", "Player-safe signature must use lowercase sha256 format.");
            if (snapshot.PlayerSafe != null && snapshot.PlayerSafe.CanonicalSignature != snapshot.PlayerSafeCanonicalSignature)
                Add(issues, "E10_PLAYER_SIGNATURE_MISMATCH", DevEncounterSeedValidationCategory.Signature,
                    "snapshot.PlayerSafeCanonicalSignature", "Root and player-safe signatures must agree.");
            if (snapshot.CanonicalSignature != DevEncounterSeedDataCanonical.Hash(snapshot.BuildCanonicalPayload()))
                Add(issues, "E10_FULL_SIGNATURE_STALE", DevEncounterSeedValidationCategory.Signature,
                    "snapshot.CanonicalSignature", "Full signature does not match the immutable payload.");
            if (snapshot.PlayerSafe != null
                && snapshot.PlayerSafe.CanonicalSignature != DevEncounterSeedDataCanonical.Hash(snapshot.PlayerSafe.BuildCanonicalPayload()))
                Add(issues, "E10_PLAYER_SIGNATURE_STALE", DevEncounterSeedValidationCategory.Signature,
                    "snapshot.PlayerSafe.CanonicalSignature", "Player-safe signature does not match its whitelist payload.");

            ValidateSeeds(snapshot, issues);
            if (snapshot.EnemySystemSnapshot != null)
            {
                ValidateE09(snapshot.EnemySystemSnapshot, issues);
                ValidateComposition(snapshot, issues);
                ValidateSkillPhase(snapshot.EnemySystemSnapshot.EnemySkillBossPhaseCatalogSnapshot, issues);
                ValidatePressureWindow(snapshot.EnemySystemSnapshot.CounterWindowAndPressureCatalogSnapshot,
                    snapshot.EnemySystemSnapshot.EnemyValidationContentSnapshot,
                    snapshot.EnemySystemSnapshot.EnemySkillBossPhaseCatalogSnapshot, issues);
                ValidateSeedReferences(snapshot, issues);
            }
            ValidateLegacyMappings(snapshot, issues);
            ValidatePlayerSafe(snapshot, issues);
            return Sort(issues);
        }

        private static void ValidateSeeds(DevEncounterSeedDataSnapshot snapshot,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (snapshot.SeedProfiles.Count != 4)
                Add(issues, "E10_SEED_COUNT", DevEncounterSeedValidationCategory.Mapping,
                    "snapshot.SeedProfiles", "Exactly four seed profiles are required.");
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (DevEncounterSeedProfileSnapshot seed in snapshot.SeedProfiles)
            {
                if (seed == null)
                {
                    Add(issues, "E10_SEED_NULL", DevEncounterSeedValidationCategory.Schema,
                        "snapshot.SeedProfiles", "Seed entries must not be null.");
                    continue;
                }
                string path = "snapshot.SeedProfiles/" + seed.SeedId;
                if (!ValidId(seed.SeedId)) Add(issues, "E10_SEED_ID_INVALID",
                    DevEncounterSeedValidationCategory.Identity, path + ".SeedId", "SeedId format is invalid.");
                if (!ids.Add(seed.SeedId)) Add(issues, "E10_SEED_ID_DUPLICATE",
                    DevEncounterSeedValidationCategory.Identity, path + ".SeedId", "SeedId must be unique.");
                if (seed.PlayerSafe == null || seed.InternalOnly == null || seed.DeveloperOnly == null)
                    Add(issues, "E10_SEED_LAYER_NULL", DevEncounterSeedValidationCategory.Schema, path,
                        "Every seed requires player-safe, internal-only, and developer-only layers.");
                Isolation(issues, path, seed.DevOnly, seed.IsEnabled, seed.EntersFormalFlow);
                if (seed.InternalOnly != null)
                    Isolation(issues, path + ".InternalOnly", seed.InternalOnly.DevOnly,
                        seed.InternalOnly.IsEnabled, seed.InternalOnly.EntersFormalFlow);

                ExpectedSeed expected = ExpectedSeeds.FirstOrDefault(value => value.SeedId == seed.SeedId);
                if (expected == null)
                {
                    Add(issues, "E10_SEED_UNKNOWN", DevEncounterSeedValidationCategory.Mapping, path,
                        "Seed is not one of the four Guard-authorized mappings.");
                    continue;
                }
                if (seed.InternalOnly == null || seed.DeveloperOnly == null
                    || seed.InternalOnly.DevChapterLabel != expected.Chapter
                    || seed.InternalOnly.EncounterId != expected.EncounterId
                    || seed.InternalOnly.PrimaryMapRuleId != expected.MapRuleId
                    || seed.InternalOnly.BossCarrierId != expected.BossCarrierId
                    || seed.InternalOnly.BossPhasePlanId != expected.BossCarrierId
                    || seed.DeveloperOnly.SourceScenarioId != expected.LegacyStageId)
                    Add(issues, "E10_SEED_MAPPING_MISMATCH", DevEncounterSeedValidationCategory.Mapping,
                        path, "Seed chapter, encounter, map, Boss plan, or legacy mapping is incorrect.");
            }
            foreach (ExpectedSeed expected in ExpectedSeeds)
                if (!ids.Contains(expected.SeedId)) Add(issues, "E10_SEED_MISSING",
                    DevEncounterSeedValidationCategory.Mapping, "snapshot.SeedProfiles/" + expected.SeedId,
                    "A Guard-authorized seed is missing.");
        }

        private static void ValidateE09(EnemySystemSnapshot root,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (root.SchemaId != EnemySystemSnapshotSchema.SchemaId
                || root.SchemaVersion != EnemySystemSnapshotSchema.SchemaVersion)
                Add(issues, "E10_E09_SCHEMA", DevEncounterSeedValidationCategory.Schema,
                    "snapshot.EnemySystemSnapshot.Schema", "E09 schema id/version is invalid.");
            Isolation(issues, "snapshot.EnemySystemSnapshot", root.DevOnly, root.IsEnabled, root.EntersFormalFlow);
            if (!DevEncounterSeedDataCanonical.IsSignature(root.CanonicalSignature)
                || !DevEncounterSeedDataCanonical.IsSignature(root.PlayerSafeCanonicalSignature))
                Add(issues, "E10_E09_SIGNATURE", DevEncounterSeedValidationCategory.Signature,
                    "snapshot.EnemySystemSnapshot", "E09 Full and PlayerSafe signatures must be valid.");
            foreach (EnemySystemRelationSnapshot relation in root.RelationIndex)
            {
                if (!relation.Resolved) Add(issues, "E10_REFERENCE_UNRESOLVED",
                    DevEncounterSeedValidationCategory.Reference,
                    "E09/" + relation.SourceId + "/" + relation.TargetId,
                    "E09 relation target is unresolved.");
                if (!relation.IsolationCompatible) Add(issues, "E10_RELATION_ISOLATION",
                    DevEncounterSeedValidationCategory.Isolation,
                    "E09/" + relation.SourceId + "/" + relation.TargetId,
                    "E09 relation endpoints have incompatible isolation.");
            }
        }

        private static void ValidateComposition(DevEncounterSeedDataSnapshot snapshot,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            EncounterCompositionCatalogSnapshot catalog = snapshot.EnemySystemSnapshot.EncounterCompositionCatalogSnapshot;
            if (catalog == null)
            {
                Add(issues, "E10_E04_NULL", DevEncounterSeedValidationCategory.Composition,
                    "E09.E04", "E04 catalog is required.");
                return;
            }
            int waveCount = catalog.Encounters.Sum(value => value.Waves.Count);
            int slotCount = catalog.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count));
            int enemyCount = catalog.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count(slot => slot.Kind == EncounterSlotKind.Enemy)));
            int bossCount = catalog.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count(slot => slot.Kind == EncounterSlotKind.Boss)));
            if (catalog.Encounters.Count != 4 || waveCount != 12 || slotCount != 12 || enemyCount != 8 || bossCount != 4)
                Add(issues, "E10_COMPOSITION_COUNTS", DevEncounterSeedValidationCategory.Composition,
                    "E09.E04", "Composition must contain 4 encounters, 12 waves, 12 slots, 8 Enemy slots, and 4 Boss slots.");

            foreach (ExpectedSeed expected in ExpectedSeeds)
            {
                EncounterCompositionSnapshot encounter = catalog.Encounters.FirstOrDefault(value => value.EncounterId == expected.EncounterId);
                string path = "E09.E04/" + expected.EncounterId;
                if (encounter == null)
                {
                    Add(issues, "E10_ENCOUNTER_MISSING", DevEncounterSeedValidationCategory.Composition,
                        path, "Seed encounter is missing.");
                    continue;
                }
                if (encounter.MapRuleIds.Count != 1 || encounter.MapRuleIds[0] != expected.MapRuleId)
                    Add(issues, "E10_MAP_MAPPING", DevEncounterSeedValidationCategory.Mapping,
                        path + ".MapRuleIds", "Encounter must use its single authoritative MapRule.");
                if (encounter.Waves.Count != 3 || !encounter.Waves.Select(value => value.WaveOrder).SequenceEqual(new[] { 0, 1, 2 }))
                    Add(issues, "E10_WAVE_ORDER", DevEncounterSeedValidationCategory.Composition,
                        path + ".Waves", "Encounter waves must be contiguous ordinary/damping/Boss orders 0/1/2.");
                if (encounter.Waves.Any(value => value.Slots.Count != 1))
                    Add(issues, "E10_WAVE_SLOT_COUNT", DevEncounterSeedValidationCategory.Composition,
                        path + ".Waves", "Every wave must contain exactly one slot.");
                if (encounter.Waves.Count != 3 || encounter.Waves.Any(value => value.Slots.Count == 0)) continue;
                EncounterSlotSnapshot ordinary = encounter.Waves[0].Slots[0];
                EncounterSlotSnapshot damping = encounter.Waves[1].Slots[0];
                EncounterSlotSnapshot boss = encounter.Waves[2].Slots[0];
                if (!SlotMatches(ordinary, EncounterSlotKind.Enemy, EncounterSlotRole.Normal,
                    expected.OrdinaryCarrierId, expected.OrdinaryProfileId)
                    || !SlotMatches(damping, EncounterSlotKind.Enemy, EncounterSlotRole.Elite,
                        expected.DampingCarrierId, expected.DampingProfileId)
                    || !SlotMatches(boss, EncounterSlotKind.Boss, EncounterSlotRole.Boss,
                        expected.BossCarrierId, expected.BossProfileId))
                    Add(issues, "E10_SLOT_MAPPING", DevEncounterSeedValidationCategory.Mapping,
                        path + ".Slots", "Encounter slot carrier, kind, role, quantity, order, or mechanic mapping is invalid.");
            }
        }

        private static bool SlotMatches(EncounterSlotSnapshot slot, EncounterSlotKind kind,
            EncounterSlotRole role, string carrierId, string profileId)
        {
            return slot != null && slot.Kind == kind && slot.Role == role && slot.CarrierId == carrierId
                && slot.Quantity == 1 && slot.SpawnOrder == 0 && slot.MechanicProfileIds.Count == 1
                && slot.MechanicProfileIds[0] == profileId;
        }

        private static void ValidateSkillPhase(EnemySkillBossPhaseCatalogSnapshot catalog,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (catalog == null)
            {
                Add(issues, "E10_E05_NULL", DevEncounterSeedValidationCategory.SkillPhase,
                    "E09.E05", "E05 catalog is required.");
                return;
            }
            if (catalog.SkillPatterns.Count != 9 || catalog.SkillSequences.Count != 9
                || catalog.CarrierSkillBindings.Count != 10 || catalog.BossPhaseProfiles.Count != 8
                || catalog.BossPhasePlans.Count != 4)
                Add(issues, "E10_E05_COUNTS", DevEncounterSeedValidationCategory.SkillPhase,
                    "E09.E05", "E05 must contain 9 patterns, 9 sequences, 10 bindings, 8 phases, and 4 plans.");
            CarrierSkillBindingSnapshot poison = catalog.CarrierSkillBindings.FirstOrDefault(value => value.CarrierId == "dev_enemy_poison_cultist");
            CarrierSkillBindingSnapshot burning = catalog.CarrierSkillBindings.FirstOrDefault(value => value.CarrierId == "dev_enemy_burning_wisp");
            if (poison == null || burning == null || !poison.SkillSequenceIds.SequenceEqual(burning.SkillSequenceIds))
                Add(issues, "E10_STATUS_SEQUENCE_REUSE", DevEncounterSeedValidationCategory.SkillPhase,
                    "E09.E05.CarrierSkillBindings", "Poison and burning carriers must reuse the same mechanism-level sequence.");
            foreach (BossPhasePlanSnapshot plan in catalog.BossPhasePlans)
                if (plan.Entries.Count != 2 || plan.Entries[0].PhaseOrder != 0 || plan.Entries[1].PhaseOrder != 1)
                    Add(issues, "E10_BOSS_PLAN_SHAPE", DevEncounterSeedValidationCategory.SkillPhase,
                        "E09.E05.BossPhasePlans/" + plan.BossId, "Each Boss must have a two-phase contiguous plan.");
            foreach (EnemySkillPatternSnapshot value in catalog.SkillPatterns)
                Isolation(issues, "E09.E05.Pattern/" + value.SkillPatternId,
                    value.SkillPatternReference.DevOnly, value.SkillPatternReference.IsEnabled,
                    value.SkillPatternReference.EntersFormalFlow);
            foreach (CarrierSkillBindingSnapshot value in catalog.CarrierSkillBindings)
                Isolation(issues, "E09.E05.Binding/" + value.CarrierId, value.DevOnly, value.IsEnabled, value.EntersFormalFlow);
        }

        private static void ValidatePressureWindow(CounterWindowAndPressureCatalogSnapshot catalog,
            EnemyValidationContentSnapshot normalization,
            EnemySkillBossPhaseCatalogSnapshot skillPhase,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (catalog == null)
            {
                Add(issues, "E10_E06_NULL", DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06", "E06 catalog is required.");
                return;
            }
            if (catalog.BuildPressureProfiles.Count != 10 || catalog.CounterWindowProfiles.Count != 6)
                Add(issues, "E10_E06_COUNTS", DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06", "E06 must contain 10 pressures and 6 counter windows.");
            int groups = 0;
            int requirements = 0;
            int requiredRows = 0;
            int recommendedRows = 0;
            foreach (BuildPressureProfileSnapshot pressure in catalog.BuildPressureProfiles)
            {
                int weight = pressure.InternalOnly.PressureChannelContributions.Sum(value => value.WeightBasisPoints);
                if (weight != 10000) Add(issues, "E10_PRESSURE_WEIGHT",
                    DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06.Pressure/" + pressure.BuildPressureProfileId,
                    "Pressure channel weights must total 10000.");
                foreach (BuildCapabilityRequirementGroupSnapshot group in pressure.DeveloperOnly.RequirementGroups)
                {
                    groups++;
                    requirements += group.Requirements.Count;
                    if (group.RequirementRole == CapabilityRequirementRole.Required)
                    {
                        requiredRows += group.Requirements.Count;
                        if (group.RequirementMatchMode != RequirementMatchMode.All)
                            Add(issues, "E10_REQUIRED_MATCH_MODE", DevEncounterSeedValidationCategory.PressureWindow,
                                "E09.E06.Requirement/" + group.RequirementGroupId,
                                "Required groups must use All.");
                    }
                    else if (group.RequirementRole == CapabilityRequirementRole.Recommended)
                    {
                        recommendedRows += group.Requirements.Count;
                        if (group.RequirementMatchMode != RequirementMatchMode.Any)
                            Add(issues, "E10_RECOMMENDED_MATCH_MODE", DevEncounterSeedValidationCategory.PressureWindow,
                                "E09.E06.Requirement/" + group.RequirementGroupId,
                                "Recommended groups must use Any.");
                    }
                    else Add(issues, "E10_REQUIREMENT_ROLE", DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.Requirement/" + group.RequirementGroupId,
                        "Requirement role must be Required or Recommended.");
                    foreach (BuildCapabilityRequirementSnapshot requirement in group.Requirements)
                        if (requirement.MinimumCapabilityBasisPoints < 1
                            || requirement.MinimumCapabilityBasisPoints > 10000)
                            Add(issues, "E10_REQUIREMENT_THRESHOLD_RANGE",
                                DevEncounterSeedValidationCategory.PressureWindow,
                                "E09.E06.Requirement/" + group.RequirementGroupId + "/" + requirement.BuildCapabilityKey,
                                "Requirement thresholds must remain within 1..10000 basis points.");
                }
            }
            if (groups != 16 || requirements != 39 || requiredRows != 27 || recommendedRows != 12)
                Add(issues, "E10_REQUIREMENT_COUNTS", DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06.Requirements", "Requirement counts must be 16 groups / 39 rows / 27 required / 12 recommended.");

            string[] windowTypes = catalog.CounterWindowProfiles.Select(value => value.InternalOnly.CounterWindowTypeKey)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] expectedWindowTypes =
            {
                "counter_window.cleanse_reveal", "counter_window.clear_core_exposure",
                "counter_window.energy_counter_full_array", "counter_window.guard_rebound",
                "counter_window.interrupt_stagger", "counter_window.shell_break"
            };
            if (!windowTypes.SequenceEqual(expectedWindowTypes, StringComparer.Ordinal))
                Add(issues, "E10_WINDOW_TYPES", DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06.CounterWindows", "All six E02 CounterWindowType keys must be instantiated exactly once.");

            ValidateCapabilityFidelity(catalog, normalization, issues);
            ValidateChannelFidelity(catalog, normalization, issues);
            ValidatePressureSourceFidelity(catalog, skillPhase, issues);
            ValidateWindowDefinitions(catalog, issues);
            ValidateWindowSourceConditionCompatibility(catalog, issues);
            ValidatePressureWindowReachability(catalog, issues);
            ValidateMapSources(catalog, issues);
            int mechanicSources = catalog.PressureSourceBindings.Count(value => value.SourceKind == PressureSourceKind.MechanicProfile);
            int mapSources = catalog.PressureSourceBindings.Count(value => value.SourceKind == PressureSourceKind.MapRule);
            int skillSources = catalog.PressureSourceBindings.Count(value => value.SourceKind == PressureSourceKind.SkillPattern);
            int phaseSources = catalog.PressureSourceBindings.Count(value => value.SourceKind == PressureSourceKind.BossPhase);
            if (mechanicSources != 10 || mapSources != 12 || skillSources != 9 || phaseSources != 8)
                Add(issues, "E10_PRESSURE_SOURCE_COVERAGE", DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06.PressureSourceBindings", "Pressure sources must cover 10 profiles, 4x3 map rows, 9 patterns, and 8 phases.");
            int windowSkillRows = catalog.CounterWindowSourceBindings.Count(value => value.SourceKind == PressureSourceKind.SkillPattern);
            int windowSkillSources = catalog.CounterWindowSourceBindings.Where(value => value.SourceKind == PressureSourceKind.SkillPattern)
                .Select(value => value.SourceId).Distinct(StringComparer.Ordinal).Count();
            int windowPhaseRows = catalog.CounterWindowSourceBindings.Count(value => value.SourceKind == PressureSourceKind.BossPhase);
            int windowPhaseSources = catalog.CounterWindowSourceBindings.Where(value => value.SourceKind == PressureSourceKind.BossPhase)
                .Select(value => value.SourceId).Distinct(StringComparer.Ordinal).Count();
            int windowMapRows = catalog.CounterWindowSourceBindings.Count(value => value.SourceKind == PressureSourceKind.MapRule);
            int windowMapSources = catalog.CounterWindowSourceBindings.Where(value => value.SourceKind == PressureSourceKind.MapRule)
                .Select(value => value.SourceId).Distinct(StringComparer.Ordinal).Count();
            int windowMechanicRows = catalog.CounterWindowSourceBindings.Count(value => value.SourceKind == PressureSourceKind.MechanicProfile);
            if (catalog.CounterWindowSourceBindings.Count != 20
                || windowSkillRows != 9 || windowSkillSources != 9
                || windowPhaseRows != 9 || windowPhaseSources != 8
                || windowMapRows != 2 || windowMapSources != 2 || windowMechanicRows != 0)
                Add(issues, "E10_WINDOW_SOURCE_COVERAGE", DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06.CounterWindowSourceBindings",
                    "Counter-window sources must be 20 rows: SkillPattern 9/9 distinct, BossPhase 9/8 distinct, MapRule 2/2 distinct, MechanicProfile 0.");

            RequirePressureWindows(catalog, issues, "dev_boss_spirit_thief_core",
                DevEncounterSeedDataCatalog.WindowInterrupt, DevEncounterSeedDataCatalog.WindowEnergy);
            RequirePressureWindows(catalog, issues, "dev_boss_black_furnace_complex_eye",
                DevEncounterSeedDataCatalog.WindowShell, DevEncounterSeedDataCatalog.WindowClear,
                DevEncounterSeedDataCatalog.WindowEnergy);
            foreach (string pressureId in new[] { "dev_enemy_poison_burn_problem", "dev_enemy_polluted_tile_problem", "dev_boss_dirty_dream_mother" })
                RequirePressureWindows(catalog, issues, pressureId, DevEncounterSeedDataCatalog.WindowCleanse);
        }

        private static void ValidateCapabilityFidelity(CounterWindowAndPressureCatalogSnapshot catalog,
            EnemyValidationContentSnapshot normalization,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (normalization == null) return;
            foreach (BuildPressureProfileSnapshot pressure in catalog.BuildPressureProfiles)
            {
                if (!normalization.TryGetMechanicProfile(pressure.BuildPressureProfileId,
                    out NormalizedMechanicProfileSnapshot profile)) continue;
                List<BuildCapabilityRequirementGroupSnapshot> required = pressure.DeveloperOnly.RequirementGroups
                    .Where(value => value.RequirementRole == CapabilityRequirementRole.Required).ToList();
                bool requiredMatches = required.Count == 1
                    && required[0].RequirementMatchMode == RequirementMatchMode.All
                    && ExactKeys(required[0].Requirements.Select(value => value.BuildCapabilityKey),
                        profile.DeveloperOnly.RequiredCapabilityKeys);
                if (!requiredMatches)
                    Add(issues, "E10_REQUIRED_CAPABILITY_FIDELITY",
                        DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.Pressure/" + pressure.BuildPressureProfileId + "/Required",
                        "Required+All capability keys must exactly match the E03 required capability set.");

                List<BuildCapabilityRequirementGroupSnapshot> recommended = pressure.DeveloperOnly.RequirementGroups
                    .Where(value => value.RequirementRole == CapabilityRequirementRole.Recommended).ToList();
                bool recommendedMatches = profile.DeveloperOnly.OptionalCapabilityKeys.Count == 0
                    ? recommended.Count == 0
                    : recommended.Count == 1
                        && recommended[0].RequirementMatchMode == RequirementMatchMode.Any
                        && ExactKeys(recommended[0].Requirements.Select(value => value.BuildCapabilityKey),
                            profile.DeveloperOnly.OptionalCapabilityKeys);
                if (!recommendedMatches)
                    Add(issues, "E10_RECOMMENDED_CAPABILITY_FIDELITY",
                        DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.Pressure/" + pressure.BuildPressureProfileId + "/Recommended",
                        "Recommended+Any capability keys must exactly match the E03 optional capability set, or be absent when E03 has none.");
            }
        }

        private static void ValidateChannelFidelity(CounterWindowAndPressureCatalogSnapshot catalog,
            EnemyValidationContentSnapshot normalization,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (normalization == null) return;
            string[] complexChannels =
            {
                "pressure.cast_interrupt", "pressure.placement_formation",
                "pressure.resource_disruption", "pressure.shield", "pressure.status_damage"
            };
            foreach (BuildPressureProfileSnapshot pressure in catalog.BuildPressureProfiles)
            {
                if (!normalization.TryGetMechanicProfile(pressure.BuildPressureProfileId,
                    out NormalizedMechanicProfileSnapshot profile)) continue;
                IEnumerable<string> expected = pressure.BuildPressureProfileId == "dev_boss_black_furnace_complex_eye"
                    ? complexChannels : profile.PlayerSafe.PressureKeys;
                if (!ExactKeys(pressure.InternalOnly.PressureChannelContributions
                        .Select(value => value.PressureChannelKey), expected))
                    Add(issues, "E10_PRESSURE_CHANNEL_FIDELITY",
                        DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.Pressure/" + pressure.BuildPressureProfileId + "/Channels",
                        "Pressure channel keys must exactly match E03, except for the fixed five-channel complex-eye composite.");
            }
        }

        private static void ValidatePressureSourceFidelity(CounterWindowAndPressureCatalogSnapshot catalog,
            EnemySkillBossPhaseCatalogSnapshot skillPhase,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            Dictionary<string, string[]> mapMatrix = ExpectedMapPressureSources();
            foreach (PressureSourceBindingSnapshot source in catalog.PressureSourceBindings)
            {
                bool matches;
                switch (source.SourceKind)
                {
                    case PressureSourceKind.MechanicProfile:
                        matches = source.SourceId == source.BuildPressureProfileId;
                        break;
                    case PressureSourceKind.SkillPattern:
                        matches = skillPhase != null
                            && skillPhase.TryGetSkillPatternById(source.SourceId, out EnemySkillPatternSnapshot pattern)
                            && pattern.InternalOnly.MechanicProfileIds.Contains(source.BuildPressureProfileId,
                                StringComparer.Ordinal);
                        break;
                    case PressureSourceKind.BossPhase:
                        matches = skillPhase != null
                            && skillPhase.TryGetBossPhaseById(source.SourceId, out BossPhaseProfileSnapshot phase)
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
                if (!matches)
                    Add(issues, "E10_PRESSURE_SOURCE_FIDELITY",
                        DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.PressureSource/" + source.SourceKind + "/" + source.SourceId
                            + "/" + source.BuildPressureProfileId,
                        "Pressure source must semantically carry its target E03 mechanic profile.");
            }
        }

        private static void ValidateWindowDefinitions(CounterWindowAndPressureCatalogSnapshot catalog,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            Dictionary<string, string> expectedSignals = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DevEncounterSeedDataCatalog.WindowInterrupt] = "signal.cast.interrupted",
                [DevEncounterSeedDataCatalog.WindowCleanse] = "signal.status.cleansed",
                [DevEncounterSeedDataCatalog.WindowEnergy] = "signal.energy.countered",
                [DevEncounterSeedDataCatalog.WindowGuard] = "signal.guard.succeeded",
                [DevEncounterSeedDataCatalog.WindowShell] = "signal.shell.broken",
                [DevEncounterSeedDataCatalog.WindowClear] = "signal.core.exposed"
            };
            Dictionary<string, int> expectedDurations = new Dictionary<string, int>(StringComparer.Ordinal)
            {
                [DevEncounterSeedDataCatalog.WindowInterrupt] = 1200,
                [DevEncounterSeedDataCatalog.WindowCleanse] = 1300,
                [DevEncounterSeedDataCatalog.WindowEnergy] = 1500,
                [DevEncounterSeedDataCatalog.WindowGuard] = 1100,
                [DevEncounterSeedDataCatalog.WindowShell] = 1400,
                [DevEncounterSeedDataCatalog.WindowClear] = 1600
            };
            foreach (KeyValuePair<string, string> expected in expectedSignals)
            {
                CounterWindowProfileSnapshot window = catalog.CounterWindowProfiles
                    .FirstOrDefault(value => value.CounterWindowId == expected.Key);
                CounterWindowInternalSpec spec = window == null ? null : window.InternalOnly;
                CounterWindowConditionSnapshot condition = spec != null && spec.OpenConditions.Count == 1
                    ? spec.OpenConditions[0] : null;
                bool matches = spec != null
                    && spec.OpenConditionMatchMode == RequirementMatchMode.All
                    && condition != null && condition.Kind == CounterWindowConditionKind.MechanicSignal
                    && condition.ReferenceId == expected.Value
                    && spec.CloseConditionMatchMode == RequirementMatchMode.Any
                    && spec.CloseConditions.Count == 0
                    && spec.MaximumDurationMilliseconds == expectedDurations[expected.Key];
                if (!matches)
                    Add(issues, "E10_WINDOW_OPEN_CONDITION_FIDELITY",
                        DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.CounterWindow/" + expected.Key,
                        "CounterWindow must keep its fixed duration and expose exactly the assigned MechanicSignal open condition with no close condition.");
            }
        }

        private static void ValidateWindowSourceConditionCompatibility(
            CounterWindowAndPressureCatalogSnapshot catalog,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            Dictionary<string, CounterWindowProfileSnapshot> windows = catalog.CounterWindowProfiles
                .ToDictionary(value => value.CounterWindowId, StringComparer.Ordinal);
            foreach (CounterWindowSourceBindingSnapshot source in catalog.CounterWindowSourceBindings)
            {
                bool compatible = windows.TryGetValue(source.CounterWindowId,
                        out CounterWindowProfileSnapshot window)
                    && WindowSourceConditionCompatible(source, window);
                if (!compatible)
                    Add(issues, "E10_WINDOW_SOURCE_CONDITION_MISMATCH",
                        DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.CounterWindowSource/" + source.SourceKind + "/" + source.SourceId
                            + "/" + source.CounterWindowId,
                        "CounterWindowSource is incompatible with the target window open-condition source semantics.");
            }
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

        private static void ValidatePressureWindowReachability(CounterWindowAndPressureCatalogSnapshot catalog,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            foreach (PressureCounterWindowBindingSnapshot binding in catalog.PressureCounterWindowBindings)
            {
                HashSet<string> pressureSources = new HashSet<string>(catalog.PressureSourceBindings
                    .Where(value => value.BuildPressureProfileId == binding.BuildPressureProfileId)
                    .Select(value => SourceKey(value.SourceKind, value.SourceId)), StringComparer.Ordinal);
                bool reachable = catalog.CounterWindowSourceBindings
                    .Where(value => value.CounterWindowId == binding.CounterWindowId)
                    .Select(value => SourceKey(value.SourceKind, value.SourceId)).Any(pressureSources.Contains);
                if (!reachable)
                    Add(issues, "E10_PRESSURE_WINDOW_SOURCE_UNREACHABLE",
                        DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.PressureWindow/" + binding.BuildPressureProfileId + "/" + binding.CounterWindowId,
                        "Pressure and CounterWindow relation must share at least one exact SourceKind/SourceId origin.");
            }
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

        private static void ValidateMapSources(CounterWindowAndPressureCatalogSnapshot catalog,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            Dictionary<string, string[]> expected = ExpectedMapPressureSources();
            foreach (KeyValuePair<string, string[]> pair in expected)
            {
                string[] actual = catalog.PressureSourceBindings
                    .Where(value => value.SourceKind == PressureSourceKind.MapRule && value.SourceId == pair.Key)
                    .Select(value => value.BuildPressureProfileId).OrderBy(value => value, StringComparer.Ordinal).ToArray();
                if (!actual.SequenceEqual(pair.Value.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal))
                    Add(issues, "E10_MAP_SOURCE_MATRIX", DevEncounterSeedValidationCategory.PressureWindow,
                        "E09.E06.MapSource/" + pair.Key, "MapRule pressure-source matrix does not match the Guard assignment.");
            }
        }

        private static void RequirePressureWindows(CounterWindowAndPressureCatalogSnapshot catalog,
            ICollection<DevEncounterSeedValidationIssue> issues, string pressureId, params string[] windowIds)
        {
            HashSet<string> actual = new HashSet<string>(catalog.PressureCounterWindowBindings
                .Where(value => value.BuildPressureProfileId == pressureId).Select(value => value.CounterWindowId),
                StringComparer.Ordinal);
            foreach (string windowId in windowIds)
                if (!actual.Contains(windowId)) Add(issues, "E10_PRESSURE_WINDOW_REUSE",
                    DevEncounterSeedValidationCategory.PressureWindow,
                    "E09.E06.PressureWindow/" + pressureId + "/" + windowId,
                    "Required many-to-many pressure/window relation is missing.");
        }

        private static void ValidateSeedReferences(DevEncounterSeedDataSnapshot snapshot,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            EnemySystemSnapshot root = snapshot.EnemySystemSnapshot;
            HashSet<string> encounters = new HashSet<string>(root.EncounterCompositionCatalogSnapshot.Encounters.Select(value => value.EncounterId), StringComparer.Ordinal);
            HashSet<string> maps = new HashSet<string>(root.EnemyValidationContentSnapshot.MapRules.Select(value => value.Domain.StableId), StringComparer.Ordinal);
            HashSet<string> plans = new HashSet<string>(root.EnemySkillBossPhaseCatalogSnapshot.BossPhasePlans.Select(value => value.BossId), StringComparer.Ordinal);
            HashSet<string> pressures = new HashSet<string>(root.CounterWindowAndPressureCatalogSnapshot.BuildPressureProfiles.Select(value => value.BuildPressureProfileId), StringComparer.Ordinal);
            HashSet<string> windows = new HashSet<string>(root.CounterWindowAndPressureCatalogSnapshot.CounterWindowProfiles.Select(value => value.CounterWindowId), StringComparer.Ordinal);
            foreach (DevEncounterSeedProfileSnapshot seed in snapshot.SeedProfiles.Where(value => value != null && value.InternalOnly != null))
            {
                string path = "snapshot.SeedProfiles/" + seed.SeedId + ".InternalOnly";
                if (!encounters.Contains(seed.InternalOnly.EncounterId)) Reference(issues, path + ".EncounterId");
                if (!maps.Contains(seed.InternalOnly.PrimaryMapRuleId)) Reference(issues, path + ".PrimaryMapRuleId");
                if (!plans.Contains(seed.InternalOnly.BossPhasePlanId)) Reference(issues, path + ".BossPhasePlanId");
                foreach (string id in seed.InternalOnly.BuildPressureProfileIds) if (!pressures.Contains(id)) Reference(issues, path + ".BuildPressureProfileIds/" + id);
                foreach (string id in seed.InternalOnly.CounterWindowIds) if (!windows.Contains(id)) Reference(issues, path + ".CounterWindowIds/" + id);
            }
        }

        private static void ValidateLegacyMappings(DevEncounterSeedDataSnapshot snapshot,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (snapshot.LegacyMappings.Count != 4)
                Add(issues, "E10_LEGACY_COUNT", DevEncounterSeedValidationCategory.Mapping,
                    "snapshot.LegacyMappings", "Legacy mapping must contain exactly four rows.");
            foreach (ExpectedSeed expected in ExpectedSeeds)
            {
                DevEncounterSeedLegacyMappingSnapshot row = snapshot.LegacyMappings.FirstOrDefault(value => value != null && value.LegacyStageId == expected.LegacyStageId);
                if (row == null || row.SeedId != expected.SeedId || row.MapRuleId != expected.MapRuleId
                    || row.PrimaryEnemyProblemId != expected.LegacyPrimaryProblemId
                    || row.BossProblemId != expected.BossProfileId || row.BossCarrierId != expected.BossCarrierId
                    || row.RuntimeConsumesLegacy)
                    Add(issues, "E10_LEGACY_MAPPING", DevEncounterSeedValidationCategory.Mapping,
                        "snapshot.LegacyMappings/" + expected.LegacyStageId,
                        "Legacy mapping row is missing, incorrect, or consumed at runtime.");
            }
        }

        private static void ValidatePlayerSafe(DevEncounterSeedDataSnapshot snapshot,
            ICollection<DevEncounterSeedValidationIssue> issues)
        {
            if (snapshot.PlayerSafe == null) return;
            string[] rootExpected = { "SchemaId", "SchemaVersion", "SeedProfiles", "EnemySystemSnapshot", "CanonicalSignature" };
            PropertyInfo[] rootProperties = typeof(DevEncounterSeedPlayerSafeSnapshot).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (!rootProperties.Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal)
                .SequenceEqual(rootExpected.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal))
                Add(issues, "E10_PLAYER_ROOT_CLOSURE", DevEncounterSeedValidationCategory.PlayerLeak,
                    "snapshot.PlayerSafe.Properties", "Player-safe root properties exceed the five-field whitelist.");
            string[] seedExpected = { "SeedId", "PublicEncounterLabelKey", "PublicAtmosphereCueKey", "PublicFailureObservationKey", "PlayerHintCategoryKeys" };
            PropertyInfo[] seedProperties = typeof(DevEncounterSeedPlayerProjection).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (!seedProperties.Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal)
                .SequenceEqual(seedExpected.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal))
                Add(issues, "E10_PLAYER_SEED_CLOSURE", DevEncounterSeedValidationCategory.PlayerLeak,
                    "snapshot.PlayerSafe.SeedProfiles.Properties", "Player seed projection exceeds the five-field whitelist.");
            if (snapshot.PlayerSafe.SeedProfiles.Count != 4)
                Add(issues, "E10_PLAYER_SEED_COUNT", DevEncounterSeedValidationCategory.PlayerLeak,
                    "snapshot.PlayerSafe.SeedProfiles", "Player-safe projection must contain four seeds.");
            string payload = snapshot.PlayerSafe.BuildCanonicalPayload();
            foreach (string token in ForbiddenPlayerTokens)
                if (payload.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    Add(issues, "E10_PLAYER_FORBIDDEN_TOKEN", DevEncounterSeedValidationCategory.PlayerLeak,
                        "snapshot.PlayerSafe.Payload/" + token, "Player-safe payload contains an answer or developer-only token.");
            foreach (DevEncounterSeedPlayerProjection seed in snapshot.PlayerSafe.SeedProfiles.Where(value => value != null))
                foreach (string hint in seed.PlayerHintCategoryKeys)
                    if (!hint.StartsWith("player_hint.", StringComparison.Ordinal))
                        Add(issues, "E10_PLAYER_HINT_CATEGORY", DevEncounterSeedValidationCategory.PlayerLeak,
                            "snapshot.PlayerSafe/" + seed.SeedId + "/" + hint,
                            "Player hint categories must use the E02 PlayerHintCategory namespace.");
        }

        private static bool ValidId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() != value) return false;
            foreach (char c in value)
                if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '_' && c != '.' && c != '-') return false;
            return true;
        }

        private static void Reference(ICollection<DevEncounterSeedValidationIssue> issues, string path)
        {
            Add(issues, "E10_SEED_REFERENCE_UNRESOLVED", DevEncounterSeedValidationCategory.Reference,
                path, "Seed internal reference does not resolve in E03/E04/E05/E06.");
        }

        private static void Isolation(ICollection<DevEncounterSeedValidationIssue> issues,
            string path, bool devOnly, bool enabled, bool formal)
        {
            if (!devOnly || enabled || formal)
                Add(issues, "E10_ISOLATION_INVALID", DevEncounterSeedValidationCategory.Isolation,
                    path, "Content must be devOnly=true, isEnabled=false, entersFormalFlow=false.");
        }

        private static void Add(ICollection<DevEncounterSeedValidationIssue> issues, string code,
            DevEncounterSeedValidationCategory category, string path, string message)
        {
            issues.Add(new DevEncounterSeedValidationIssue(code, category, path, message));
        }

        private static IReadOnlyList<DevEncounterSeedValidationIssue> Sort(
            IEnumerable<DevEncounterSeedValidationIssue> issues)
        {
            return Array.AsReadOnly(issues.OrderBy(value => value.Category)
                .ThenBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal).ToArray());
        }

        private sealed class ExpectedSeed
        {
            public ExpectedSeed(string seedId, string chapter, string encounterId, string mapRuleId,
                string ordinaryCarrierId, string ordinaryProfileId, string dampingCarrierId,
                string dampingProfileId, string bossCarrierId, string bossProfileId, string legacyStageId)
            {
                SeedId = seedId; Chapter = chapter; EncounterId = encounterId; MapRuleId = mapRuleId;
                OrdinaryCarrierId = ordinaryCarrierId; OrdinaryProfileId = ordinaryProfileId;
                DampingCarrierId = dampingCarrierId; DampingProfileId = dampingProfileId;
                BossCarrierId = bossCarrierId; BossProfileId = bossProfileId; LegacyStageId = legacyStageId;
                LegacyPrimaryProblemId = seedId == DevEncounterSeedDataCatalog.SeedCleanseCorner
                    ? "dev_enemy_polluted_tile_problem" : ordinaryProfileId;
            }
            public string SeedId { get; }
            public string Chapter { get; }
            public string EncounterId { get; }
            public string MapRuleId { get; }
            public string OrdinaryCarrierId { get; }
            public string OrdinaryProfileId { get; }
            public string DampingCarrierId { get; }
            public string DampingProfileId { get; }
            public string BossCarrierId { get; }
            public string BossProfileId { get; }
            public string LegacyStageId { get; }
            public string LegacyPrimaryProblemId { get; }
        }
    }
}
