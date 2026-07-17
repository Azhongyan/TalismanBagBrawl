using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SkillPhase;

namespace TalismanBag.EnemySystem.SeedData
{
    public static class DevEncounterSeedDataCatalog
    {
        public const string SeedGuardWall = "dev_seed_3_10_guard_wall";
        public const string SeedCleanseCorner = "dev_seed_3_10_cleanse_corner";
        public const string SeedFurnaceCore = "dev_seed_4_10_furnace_core";
        public const string SeedThunderFireCross = "dev_seed_4_10_thunder_fire_cross";

        public const string WindowInterrupt = "dev_window_interrupt_stagger";
        public const string WindowCleanse = "dev_window_cleanse_reveal";
        public const string WindowEnergy = "dev_window_energy_counter_full_array";
        public const string WindowGuard = "dev_window_guard_rebound";
        public const string WindowShell = "dev_window_shell_break";
        public const string WindowClear = "dev_window_clear_core_exposure";

        internal static DevEncounterSeedCatalogContent CreateContent(
            EnemyValidationContentSnapshot normalization)
        {
            if (normalization == null) throw new ArgumentNullException(nameof(normalization));
            DevEncounterSeedProfileSnapshot[] seeds = CreateSeeds();
            return new DevEncounterSeedCatalogContent(
                seeds,
                new EncounterCompositionCatalogInput(CreateEncounters()),
                CreateSkillPhaseInput(),
                CreatePressureWindowInput(normalization),
                CreateLegacyMappings());
        }

        private static DevEncounterSeedProfileSnapshot[] CreateSeeds()
        {
            return new[]
            {
                Seed(SeedGuardWall, "3-10", "dev_encounter_3_10_guard_wall", "dev_map_copper_bell_night",
                    "dev_boss_burst_huzhen",
                    new[] { "dev_enemy_caster_problem", "dev_enemy_seal_problem", "dev_boss_bronze_formation_general" },
                    new[] { WindowInterrupt, WindowGuard },
                    "dev_balance_3_10_guard_wall", "guard_wall", "cast_and_guard"),
                Seed(SeedCleanseCorner, "3-10", "dev_encounter_3_10_cleanse_corner", "dev_map_bluestone_damp",
                    "dev_boss_debuff_jinge",
                    new[] { "dev_enemy_poison_burn_problem", "dev_enemy_polluted_tile_problem", "dev_boss_dirty_dream_mother" },
                    new[] { WindowCleanse },
                    "dev_balance_3_10_cleanse_corner", "cleanse_corner", "status_and_pollution"),
                Seed(SeedFurnaceCore, "4-10", "dev_encounter_4_10_furnace_core", "dev_map_furnace_ash_fall",
                    "dev_boss_energy_juneng",
                    new[] { "dev_enemy_polluted_tile_problem", "dev_enemy_formation_eye_problem", "dev_enemy_spirit_thief_problem", "dev_boss_spirit_thief_core", "dev_boss_black_furnace_complex_eye" },
                    new[] { WindowInterrupt, WindowEnergy, WindowShell, WindowClear },
                    "dev_balance_4_10_furnace_core", "furnace_core", "cross_layer_furnace"),
                Seed(SeedThunderFireCross, "4-10", "dev_encounter_4_10_thunder_fire_cross", "dev_map_bluestone_crack",
                    "dev_boss_hybrid_combo",
                    new[] { "dev_enemy_spirit_thief_problem", "dev_enemy_formation_eye_problem", "dev_boss_black_furnace_complex_eye" },
                    new[] { WindowEnergy, WindowShell, WindowClear },
                    "dev_balance_4_10_thunder_fire_cross", "thunder_fire_cross", "composite_eye")
            };
        }

        private static DevEncounterSeedProfileSnapshot Seed(string seedId, string chapter,
            string encounterId, string mapRuleId, string bossCarrierId,
            IEnumerable<string> pressureIds, IEnumerable<string> windowIds,
            string legacyStageId, string publicKey, string noteKey)
        {
            return new DevEncounterSeedProfileSnapshot(
                new DevEncounterSeedPlayerProjection(seedId,
                    "enemy.encounter." + publicKey + ".label",
                    "enemy.encounter." + publicKey + ".atmosphere_cue",
                    "enemy.encounter." + publicKey + ".failure_observation",
                    HintKeys(seedId)),
                new DevEncounterSeedInternalSnapshot(chapter, encounterId, mapRuleId,
                    "wave_ordinary", "wave_damping", "wave_boss", bossCarrierId, bossCarrierId,
                    pressureIds, windowIds),
                new DevEncounterSeedDeveloperSnapshot(legacyStageId,
                    "diagnostic.goal." + publicKey,
                    new[] { "diagnostic.invalid_reference", "diagnostic.canonical_signature_mismatch", "diagnostic.player_answer_leak" },
                    "diagnostic.composition." + noteKey));
        }

        private static IEnumerable<string> HintKeys(string seedId)
        {
            switch (seedId)
            {
                case SeedGuardWall:
                    return new[] { "player_hint.cast_interrupt_opportunity", "player_hint.talisman_sealed", "player_hint.burst_incoming" };
                case SeedCleanseCorner:
                    return new[] { "player_hint.status_pressure", "player_hint.formation_disrupted" };
                case SeedFurnaceCore:
                    return new[] { "player_hint.energy_disrupted", "player_hint.formation_disrupted", "player_hint.cast_interrupt_opportunity" };
                default:
                    return new[] { "player_hint.energy_disrupted", "player_hint.formation_disrupted", "player_hint.burst_incoming" };
            }
        }

        private static EncounterCompositionSnapshot[] CreateEncounters()
        {
            return new[]
            {
                Encounter("dev_encounter_3_10_guard_wall", "dev_map_copper_bell_night",
                    "dev_enemy_caster_chanter", "dev_enemy_caster_problem",
                    "dev_enemy_seal_locker", "dev_enemy_seal_problem",
                    "dev_boss_burst_huzhen", "dev_boss_bronze_formation_general"),
                Encounter("dev_encounter_3_10_cleanse_corner", "dev_map_bluestone_damp",
                    "dev_enemy_poison_cultist", "dev_enemy_poison_burn_problem",
                    "dev_enemy_burning_wisp", "dev_enemy_poison_burn_problem",
                    "dev_boss_debuff_jinge", "dev_boss_dirty_dream_mother"),
                Encounter("dev_encounter_4_10_furnace_core", "dev_map_furnace_ash_fall",
                    "dev_enemy_spirit_thief", "dev_enemy_spirit_thief_problem",
                    "dev_enemy_formation_eye_jammer", "dev_enemy_formation_eye_problem",
                    "dev_boss_energy_juneng", "dev_boss_spirit_thief_core"),
                Encounter("dev_encounter_4_10_thunder_fire_cross", "dev_map_bluestone_crack",
                    "dev_enemy_formation_eye_jammer", "dev_enemy_formation_eye_problem",
                    "dev_enemy_spirit_thief", "dev_enemy_spirit_thief_problem",
                    "dev_boss_hybrid_combo", "dev_boss_black_furnace_complex_eye")
            };
        }

        private static EncounterCompositionSnapshot Encounter(string encounterId, string mapRuleId,
            string ordinaryCarrier, string ordinaryProfile, string dampingCarrier, string dampingProfile,
            string bossCarrier, string bossProfile)
        {
            return new EncounterCompositionSnapshot(new EncounterReference(encounterId),
                new[] { mapRuleId },
                new[]
                {
                    new EncounterWaveSnapshot("wave_ordinary", 0, new[]
                    {
                        new EncounterSlotSnapshot("slot_ordinary", 0, EncounterSlotKind.Enemy,
                            EncounterSlotRole.Normal, ordinaryCarrier, 1, new[] { ordinaryProfile })
                    }),
                    new EncounterWaveSnapshot("wave_damping", 1, new[]
                    {
                        new EncounterSlotSnapshot("slot_damping", 0, EncounterSlotKind.Enemy,
                            EncounterSlotRole.Elite, dampingCarrier, 1, new[] { dampingProfile })
                    }),
                    new EncounterWaveSnapshot("wave_boss", 2, new[]
                    {
                        new EncounterSlotSnapshot("slot_boss", 0, EncounterSlotKind.Boss,
                            EncounterSlotRole.Boss, bossCarrier, 1, new[] { bossProfile })
                    })
                },
                new[] { "dev_tag.encounter_seed", "dev_tag.e10" });
        }

        private static EnemySkillBossPhaseCatalogInput CreateSkillPhaseInput()
        {
            EnemySkillPatternSnapshot[] patterns =
            {
                Pattern("dev_skill_caster_long_cast", "dev_enemy_caster_problem", SkillCastKind.Channeled, 1600, 450, "caster_long_cast", "player_hint.cast_interrupt_opportunity"),
                Pattern("dev_skill_seal_lock", "dev_enemy_seal_problem", SkillCastKind.Instant, 0, 650, "seal_lock", "player_hint.talisman_sealed"),
                Pattern("dev_skill_status_spread", "dev_enemy_poison_burn_problem", SkillCastKind.Channeled, 900, 500, "status_spread", "player_hint.status_pressure"),
                Pattern("dev_skill_spirit_drain", "dev_enemy_spirit_thief_problem", SkillCastKind.Channeled, 1200, 500, "spirit_drain", "player_hint.energy_disrupted"),
                Pattern("dev_skill_formation_eye_disruption", "dev_enemy_formation_eye_problem", SkillCastKind.Instant, 0, 700, "formation_eye_disruption", "player_hint.formation_disrupted"),
                Pattern("dev_skill_bronze_general_burst", "dev_boss_bronze_formation_general", SkillCastKind.Channeled, 1300, 650, "bronze_general_burst", "player_hint.burst_incoming"),
                Pattern("dev_skill_dirty_dream_pressure", "dev_boss_dirty_dream_mother", SkillCastKind.Channeled, 1000, 600, "dirty_dream_pressure", "player_hint.status_pressure"),
                Pattern("dev_skill_spirit_core_channel", "dev_boss_spirit_thief_core", SkillCastKind.Channeled, 1700, 700, "spirit_core_channel", "player_hint.energy_disrupted"),
                Pattern("dev_skill_complex_eye_composite", "dev_boss_black_furnace_complex_eye", SkillCastKind.Channeled, 1800, 750, "complex_eye_composite", "player_hint.formation_disrupted")
            };

            SkillSequenceSnapshot[] sequences =
            {
                Sequence("dev_sequence_caster_long_cast", "dev_skill_caster_long_cast"),
                Sequence("dev_sequence_seal_lock", "dev_skill_seal_lock"),
                Sequence("dev_sequence_status_spread", "dev_skill_status_spread"),
                Sequence("dev_sequence_spirit_drain", "dev_skill_spirit_drain"),
                Sequence("dev_sequence_formation_eye_disruption", "dev_skill_formation_eye_disruption"),
                Sequence("dev_sequence_bronze_general_burst", "dev_skill_bronze_general_burst"),
                Sequence("dev_sequence_dirty_dream_pressure", "dev_skill_dirty_dream_pressure"),
                Sequence("dev_sequence_spirit_core_channel", "dev_skill_spirit_core_channel"),
                Sequence("dev_sequence_complex_eye_composite", "dev_skill_complex_eye_composite")
            };

            CarrierSkillBindingSnapshot[] bindings =
            {
                Binding(EnemySkillCarrierKind.Enemy, "dev_enemy_caster_chanter", "dev_sequence_caster_long_cast"),
                Binding(EnemySkillCarrierKind.Enemy, "dev_enemy_seal_locker", "dev_sequence_seal_lock"),
                Binding(EnemySkillCarrierKind.Enemy, "dev_enemy_poison_cultist", "dev_sequence_status_spread"),
                Binding(EnemySkillCarrierKind.Enemy, "dev_enemy_burning_wisp", "dev_sequence_status_spread"),
                Binding(EnemySkillCarrierKind.Enemy, "dev_enemy_spirit_thief", "dev_sequence_spirit_drain"),
                Binding(EnemySkillCarrierKind.Enemy, "dev_enemy_formation_eye_jammer", "dev_sequence_formation_eye_disruption"),
                Binding(EnemySkillCarrierKind.Boss, "dev_boss_burst_huzhen", "dev_sequence_bronze_general_burst"),
                Binding(EnemySkillCarrierKind.Boss, "dev_boss_debuff_jinge", "dev_sequence_dirty_dream_pressure"),
                Binding(EnemySkillCarrierKind.Boss, "dev_boss_energy_juneng", "dev_sequence_spirit_core_channel"),
                Binding(EnemySkillCarrierKind.Boss, "dev_boss_hybrid_combo", "dev_sequence_complex_eye_composite")
            };

            List<BossPhaseProfileSnapshot> phases = new List<BossPhaseProfileSnapshot>();
            List<BossPhasePlanSnapshot> plans = new List<BossPhasePlanSnapshot>();
            AddBoss(phases, plans, "dev_boss_burst_huzhen", "bronze_general", "dev_sequence_bronze_general_burst", "dev_boss_bronze_formation_general");
            AddBoss(phases, plans, "dev_boss_debuff_jinge", "dirty_dream", "dev_sequence_dirty_dream_pressure", "dev_boss_dirty_dream_mother");
            AddBoss(phases, plans, "dev_boss_energy_juneng", "spirit_core", "dev_sequence_spirit_core_channel", "dev_boss_spirit_thief_core");
            AddBoss(phases, plans, "dev_boss_hybrid_combo", "complex_eye", "dev_sequence_complex_eye_composite", "dev_boss_black_furnace_complex_eye");
            return new EnemySkillBossPhaseCatalogInput(patterns, sequences, bindings, phases, plans);
        }

        private static EnemySkillPatternSnapshot Pattern(string id, string profileId,
            SkillCastKind kind, int castMilliseconds, int recoveryMilliseconds,
            string publicKey, string hintKey)
        {
            return new EnemySkillPatternSnapshot(new SkillPatternReference(id),
                new SkillPatternPlayerProjection(id, "enemy.skill." + publicKey + ".label",
                    "enemy.skill." + publicKey + ".intent", "enemy.skill." + publicKey + ".intent_text",
                    "enemy.skill." + publicKey + ".target_cue", "enemy.skill." + publicKey + ".cast_cue",
                    new[] { hintKey }),
                new SkillPatternInternalSpec(kind, castMilliseconds, recoveryMilliseconds, new[] { profileId }),
                new SkillPatternDeveloperDiagnostics(new[] { "diagnostic.invalid_reference" },
                    new[] { "dev_source_e10_skill_" + publicKey }));
        }

        private static SkillSequenceSnapshot Sequence(string id, string patternId)
        {
            return new SkillSequenceSnapshot(id, new[] { new SkillSequenceStepSnapshot(0, patternId, 0, 1) });
        }

        private static CarrierSkillBindingSnapshot Binding(EnemySkillCarrierKind kind, string id, string sequenceId)
        {
            return new CarrierSkillBindingSnapshot(kind, id, new[] { sequenceId });
        }

        private static void AddBoss(ICollection<BossPhaseProfileSnapshot> phases,
            ICollection<BossPhasePlanSnapshot> plans, string bossId, string publicKey,
            string sequenceId, string mechanicProfileId)
        {
            string openingId = "dev_phase_" + publicKey + "_opening";
            string pressureId = "dev_phase_" + publicKey + "_pressure";
            phases.Add(Phase(openingId, publicKey + "_opening", sequenceId, mechanicProfileId));
            phases.Add(Phase(pressureId, publicKey + "_pressure", sequenceId, mechanicProfileId));
            plans.Add(new BossPhasePlanSnapshot(bossId, new[]
            {
                new BossPhasePlanEntrySnapshot(0, openingId,
                    new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.EncounterStart)),
                new BossPhasePlanEntrySnapshot(1, pressureId,
                    new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.HealthRatioAtOrBelow, 5000))
            }));
        }

        private static BossPhaseProfileSnapshot Phase(string id, string publicKey,
            string sequenceId, string mechanicProfileId)
        {
            return new BossPhaseProfileSnapshot(new BossPhaseReference(id),
                new BossPhasePlayerProjection(id, "boss.phase." + publicKey + ".label",
                    "boss.phase." + publicKey + ".entry_cue"),
                new BossPhaseInternalSpec(new[] { sequenceId }, new[] { mechanicProfileId }),
                new BossPhaseDeveloperDiagnostics(new[] { "diagnostic.invalid_reference" },
                    new[] { "dev_source_e10_phase_" + publicKey }));
        }

        private static CounterWindowAndPressureCatalogInput CreatePressureWindowInput(
            EnemyValidationContentSnapshot normalization)
        {
            BuildPressureProfileSnapshot[] pressures =
            {
                Pressure(normalization, "dev_enemy_caster_problem", OneChannel("pressure.cast_interrupt"), "caster", "player_hint.cast_interrupt_opportunity"),
                Pressure(normalization, "dev_enemy_seal_problem", OneChannel("pressure.seal_control"), "seal", "player_hint.talisman_sealed"),
                Pressure(normalization, "dev_enemy_poison_burn_problem", OneChannel("pressure.status_damage"), "status", "player_hint.status_pressure"),
                Pressure(normalization, "dev_enemy_polluted_tile_problem", OneChannel("pressure.placement_formation"), "polluted_tile", "player_hint.formation_disrupted"),
                Pressure(normalization, "dev_enemy_spirit_thief_problem", OneChannel("pressure.resource_disruption"), "spirit_thief", "player_hint.energy_disrupted"),
                Pressure(normalization, "dev_enemy_formation_eye_problem", OneChannel("pressure.placement_formation"), "formation_eye", "player_hint.formation_disrupted"),
                Pressure(normalization, "dev_boss_bronze_formation_general", OneChannel("pressure.burst_survival"), "bronze_general", "player_hint.burst_incoming"),
                Pressure(normalization, "dev_boss_dirty_dream_mother", OneChannel("pressure.status_damage"), "dirty_dream", "player_hint.status_pressure"),
                Pressure(normalization, "dev_boss_spirit_thief_core", new[]
                {
                    new PressureChannelContributionSnapshot("pressure.cast_interrupt", 5000),
                    new PressureChannelContributionSnapshot("pressure.resource_disruption", 5000)
                }, "spirit_core", "player_hint.energy_disrupted"),
                Pressure(normalization, "dev_boss_black_furnace_complex_eye", new[]
                {
                    new PressureChannelContributionSnapshot("pressure.shield", 2000),
                    new PressureChannelContributionSnapshot("pressure.status_damage", 1500),
                    new PressureChannelContributionSnapshot("pressure.placement_formation", 2500),
                    new PressureChannelContributionSnapshot("pressure.resource_disruption", 2000),
                    new PressureChannelContributionSnapshot("pressure.cast_interrupt", 2000)
                }, "complex_eye", "player_hint.formation_disrupted")
            };

            CounterWindowProfileSnapshot[] windows =
            {
                Window(WindowInterrupt, "counter_window.interrupt_stagger", "interrupt", CounterWindowConditionKind.MechanicSignal, "signal.cast.interrupted", 1200),
                Window(WindowCleanse, "counter_window.cleanse_reveal", "cleanse", CounterWindowConditionKind.MechanicSignal, "signal.status.cleansed", 1300),
                Window(WindowEnergy, "counter_window.energy_counter_full_array", "energy", CounterWindowConditionKind.MechanicSignal, "signal.energy.countered", 1500),
                Window(WindowGuard, "counter_window.guard_rebound", "guard", CounterWindowConditionKind.MechanicSignal, "signal.guard.succeeded", 1100),
                Window(WindowShell, "counter_window.shell_break", "shell", CounterWindowConditionKind.MechanicSignal, "signal.shell.broken", 1400),
                Window(WindowClear, "counter_window.clear_core_exposure", "clear", CounterWindowConditionKind.MechanicSignal, "signal.core.exposed", 1600)
            };

            List<PressureSourceBindingSnapshot> pressureSources = new List<PressureSourceBindingSnapshot>();
            foreach (BuildPressureProfileSnapshot pressure in pressures)
                pressureSources.Add(new PressureSourceBindingSnapshot(PressureSourceKind.MechanicProfile,
                    pressure.BuildPressureProfileId, pressure.BuildPressureProfileId));

            AddMapSources(pressureSources, "dev_map_copper_bell_night", "dev_enemy_caster_problem", "dev_enemy_seal_problem", "dev_boss_bronze_formation_general");
            AddMapSources(pressureSources, "dev_map_bluestone_damp", "dev_enemy_poison_burn_problem", "dev_enemy_polluted_tile_problem", "dev_boss_dirty_dream_mother");
            AddMapSources(pressureSources, "dev_map_furnace_ash_fall", "dev_enemy_polluted_tile_problem", "dev_enemy_formation_eye_problem", "dev_boss_black_furnace_complex_eye");
            AddMapSources(pressureSources, "dev_map_bluestone_crack", "dev_enemy_spirit_thief_problem", "dev_enemy_formation_eye_problem", "dev_boss_black_furnace_complex_eye");

            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_caster_long_cast", "dev_enemy_caster_problem");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_seal_lock", "dev_enemy_seal_problem");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_status_spread", "dev_enemy_poison_burn_problem");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_spirit_drain", "dev_enemy_spirit_thief_problem");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_formation_eye_disruption", "dev_enemy_formation_eye_problem");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_bronze_general_burst", "dev_boss_bronze_formation_general");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_dirty_dream_pressure", "dev_boss_dirty_dream_mother");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_spirit_core_channel", "dev_boss_spirit_thief_core");
            AddSource(pressureSources, PressureSourceKind.SkillPattern, "dev_skill_complex_eye_composite", "dev_boss_black_furnace_complex_eye");

            AddPhaseSources(pressureSources, "bronze_general", "dev_boss_bronze_formation_general");
            AddPhaseSources(pressureSources, "dirty_dream", "dev_boss_dirty_dream_mother");
            AddPhaseSources(pressureSources, "spirit_core", "dev_boss_spirit_thief_core");
            AddPhaseSources(pressureSources, "complex_eye", "dev_boss_black_furnace_complex_eye");

            CounterWindowSourceBindingSnapshot[] windowSources =
            {
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_caster_long_cast", WindowInterrupt),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_seal_lock", WindowGuard),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_status_spread", WindowCleanse),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_spirit_drain", WindowEnergy),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_formation_eye_disruption", WindowEnergy),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_bronze_general_burst", WindowGuard),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_dirty_dream_pressure", WindowCleanse),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_spirit_core_channel", WindowInterrupt),
                WindowSource(PressureSourceKind.SkillPattern, "dev_skill_complex_eye_composite", WindowShell),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_bronze_general_opening", WindowGuard),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_bronze_general_pressure", WindowGuard),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_dirty_dream_opening", WindowCleanse),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_dirty_dream_pressure", WindowCleanse),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_spirit_core_opening", WindowInterrupt),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_spirit_core_pressure", WindowEnergy),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_complex_eye_opening", WindowShell),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_complex_eye_pressure", WindowClear),
                WindowSource(PressureSourceKind.MapRule, "dev_map_bluestone_damp", WindowCleanse),
                WindowSource(PressureSourceKind.MapRule, "dev_map_furnace_ash_fall", WindowCleanse),
                WindowSource(PressureSourceKind.BossPhase, "dev_phase_complex_eye_pressure", WindowEnergy)
            };

            PressureCounterWindowBindingSnapshot[] pressureWindows =
            {
                PressureWindow("dev_enemy_caster_problem", WindowInterrupt),
                PressureWindow("dev_enemy_seal_problem", WindowGuard),
                PressureWindow("dev_enemy_poison_burn_problem", WindowCleanse),
                PressureWindow("dev_enemy_polluted_tile_problem", WindowCleanse),
                PressureWindow("dev_enemy_spirit_thief_problem", WindowEnergy),
                PressureWindow("dev_enemy_formation_eye_problem", WindowEnergy),
                PressureWindow("dev_boss_bronze_formation_general", WindowGuard),
                PressureWindow("dev_boss_dirty_dream_mother", WindowCleanse),
                PressureWindow("dev_boss_spirit_thief_core", WindowInterrupt),
                PressureWindow("dev_boss_spirit_thief_core", WindowEnergy),
                PressureWindow("dev_boss_black_furnace_complex_eye", WindowShell),
                PressureWindow("dev_boss_black_furnace_complex_eye", WindowClear),
                PressureWindow("dev_boss_black_furnace_complex_eye", WindowEnergy)
            };

            return new CounterWindowAndPressureCatalogInput(pressures, windows,
                pressureSources, windowSources, pressureWindows);
        }

        private static BuildPressureProfileSnapshot Pressure(EnemyValidationContentSnapshot normalization,
            string profileId, IEnumerable<PressureChannelContributionSnapshot> channels,
            string publicKey, string hintKey)
        {
            if (!normalization.TryGetMechanicProfile(profileId, out NormalizedMechanicProfileSnapshot profile))
                throw new InvalidOperationException("E10 mechanic profile not found: " + profileId);
            List<BuildCapabilityRequirementGroupSnapshot> groups = new List<BuildCapabilityRequirementGroupSnapshot>
            {
                Group(profileId + ".required", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                    profile.DeveloperOnly.RequiredCapabilityKeys.Select(key => Requirement(profileId, key, true)))
            };
            if (profile.DeveloperOnly.OptionalCapabilityKeys.Count > 0)
            {
                groups.Add(Group(profileId + ".recommended", CapabilityRequirementRole.Recommended,
                    RequirementMatchMode.Any,
                    profile.DeveloperOnly.OptionalCapabilityKeys.Select(key => Requirement(profileId, key, false))));
            }
            return new BuildPressureProfileSnapshot(profileId,
                new BuildPressurePlayerProjection(profileId,
                    "enemy.pressure." + publicKey + ".label", "enemy.pressure." + publicKey + ".hint",
                    new[] { hintKey }),
                new BuildPressureInternalSpec(channels),
                new BuildPressureDeveloperDiagnostics(groups,
                    new[] { "diagnostic.capability_gap", "diagnostic.invalid_reference" },
                    new[] { "dev_source_e10_pressure_" + publicKey, "dev_tuning_nonformal_basis_points" }));
        }

        private static BuildCapabilityRequirementGroupSnapshot Group(string id,
            CapabilityRequirementRole role, RequirementMatchMode mode,
            IEnumerable<BuildCapabilityRequirementSnapshot> requirements)
        {
            return new BuildCapabilityRequirementGroupSnapshot(id, role, mode, requirements);
        }

        private static BuildCapabilityRequirementSnapshot Requirement(string profileId, string key, bool required)
        {
            unchecked
            {
                uint hash = 2166136261;
                string source = profileId + "|" + key + "|" + (required ? "required" : "recommended");
                for (int index = 0; index < source.Length; index++) hash = (hash ^ source[index]) * 16777619;
                int minimum = (required ? 4200 : 2800) + (int)(hash % (required ? 2201u : 1601u));
                return new BuildCapabilityRequirementSnapshot(key, minimum);
            }
        }

        private static PressureChannelContributionSnapshot[] OneChannel(string key)
        {
            return new[] { new PressureChannelContributionSnapshot(key, 10000) };
        }

        private static CounterWindowProfileSnapshot Window(string id, string typeKey,
            string publicKey, CounterWindowConditionKind kind, string referenceId, int durationMilliseconds)
        {
            return new CounterWindowProfileSnapshot(new CounterWindowReference(id),
                new CounterWindowPlayerProjection(id, "enemy.window." + publicKey + ".label",
                    "enemy.window." + publicKey + ".open_cue", "enemy.window." + publicKey + ".close_cue"),
                new CounterWindowInternalSpec(typeKey, RequirementMatchMode.All,
                    new[] { new CounterWindowConditionSnapshot("condition." + publicKey + ".open", kind, referenceId) },
                    RequirementMatchMode.Any, Array.Empty<CounterWindowConditionSnapshot>(), durationMilliseconds),
                new CounterWindowDeveloperDiagnostics(new[] { "diagnostic.invalid_reference" },
                    new[] { "dev_source_e10_window_" + publicKey }));
        }

        private static void AddMapSources(ICollection<PressureSourceBindingSnapshot> result,
            string mapRuleId, params string[] pressureIds)
        {
            foreach (string pressureId in pressureIds) AddSource(result, PressureSourceKind.MapRule, mapRuleId, pressureId);
        }

        private static void AddPhaseSources(ICollection<PressureSourceBindingSnapshot> result,
            string phaseKey, string pressureId)
        {
            AddSource(result, PressureSourceKind.BossPhase, "dev_phase_" + phaseKey + "_opening", pressureId);
            AddSource(result, PressureSourceKind.BossPhase, "dev_phase_" + phaseKey + "_pressure", pressureId);
        }

        private static void AddSource(ICollection<PressureSourceBindingSnapshot> result,
            PressureSourceKind kind, string sourceId, string pressureId)
        {
            result.Add(new PressureSourceBindingSnapshot(kind, sourceId, pressureId));
        }

        private static CounterWindowSourceBindingSnapshot WindowSource(
            PressureSourceKind kind, string sourceId, string windowId)
        {
            return new CounterWindowSourceBindingSnapshot(kind, sourceId, windowId);
        }

        private static PressureCounterWindowBindingSnapshot PressureWindow(string pressureId, string windowId)
        {
            return new PressureCounterWindowBindingSnapshot(pressureId, windowId);
        }

        private static DevEncounterSeedLegacyMappingSnapshot[] CreateLegacyMappings()
        {
            return new[]
            {
                new DevEncounterSeedLegacyMappingSnapshot("dev_balance_3_10_guard_wall", SeedGuardWall,
                    "dev_map_copper_bell_night", "dev_enemy_caster_problem", "dev_boss_bronze_formation_general", "dev_boss_burst_huzhen"),
                new DevEncounterSeedLegacyMappingSnapshot("dev_balance_3_10_cleanse_corner", SeedCleanseCorner,
                    "dev_map_bluestone_damp", "dev_enemy_polluted_tile_problem", "dev_boss_dirty_dream_mother", "dev_boss_debuff_jinge"),
                new DevEncounterSeedLegacyMappingSnapshot("dev_balance_4_10_furnace_core", SeedFurnaceCore,
                    "dev_map_furnace_ash_fall", "dev_enemy_spirit_thief_problem", "dev_boss_spirit_thief_core", "dev_boss_energy_juneng"),
                new DevEncounterSeedLegacyMappingSnapshot("dev_balance_4_10_thunder_fire_cross", SeedThunderFireCross,
                    "dev_map_bluestone_crack", "dev_enemy_formation_eye_problem", "dev_boss_black_furnace_complex_eye", "dev_boss_hybrid_combo")
            };
        }
    }

    internal sealed class DevEncounterSeedCatalogContent
    {
        public DevEncounterSeedCatalogContent(IEnumerable<DevEncounterSeedProfileSnapshot> seedProfiles,
            EncounterCompositionCatalogInput encounterInput,
            EnemySkillBossPhaseCatalogInput skillPhaseInput,
            CounterWindowAndPressureCatalogInput pressureWindowInput,
            IEnumerable<DevEncounterSeedLegacyMappingSnapshot> legacyMappings)
        {
            SeedProfiles = DevEncounterSeedDataCanonical.Freeze(seedProfiles);
            EncounterInput = encounterInput;
            SkillPhaseInput = skillPhaseInput;
            PressureWindowInput = pressureWindowInput;
            LegacyMappings = DevEncounterSeedDataCanonical.Freeze(legacyMappings);
        }

        public IReadOnlyList<DevEncounterSeedProfileSnapshot> SeedProfiles { get; }
        public EncounterCompositionCatalogInput EncounterInput { get; }
        public EnemySkillBossPhaseCatalogInput SkillPhaseInput { get; }
        public CounterWindowAndPressureCatalogInput PressureWindowInput { get; }
        public IReadOnlyList<DevEncounterSeedLegacyMappingSnapshot> LegacyMappings { get; }
    }
}
