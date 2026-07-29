using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RequirementMigrationOverlay
{
    public static class LayoutResilienceRequirementChannelMigrationCatalog
    {
        private const string FormationCandidate =
            "layout_resilience.formation_eye.placement_shape";
        private const string FormationOwner = "dev_enemy_formation_eye_problem";
        private const string FormationGroup =
            "dev_enemy_formation_eye_problem.required";
        private const string PollutionCandidate =
            "layout_resilience.polluted_tile.placement_shape";
        private const string PollutionOwner = "dev_enemy_polluted_tile_problem";
        private const string PollutionGroup =
            "dev_enemy_polluted_tile_problem.required";
        private const string PlacementShapeKey = "capability.placement_shape";
        private const string LegacyEvidencePath =
            "Docs/V0.4/Reports/DevEncounterSeedPressureWindowRows.csv";

        public static LayoutResilienceRequirementChannelMigrationSource
            CreateOverlaySource()
        {
            return new LayoutResilienceRequirementChannelMigrationSource(
                LayoutResilienceRequirementChannelMigrationSchema.SchemaId,
                LayoutResilienceRequirementChannelMigrationSchema.SchemaVersion,
                true,
                false,
                true,
                false,
                new[]
                {
                    FormationFurnaceAsh(),
                    FormationBluestoneCrack(),
                    PollutionBluestoneDamp(),
                    PollutionFurnaceAsh()
                });
        }

        private static LayoutResilienceRequirementChannelMigrationSourceRow
            FormationFurnaceAsh()
        {
            return Row(
                "route.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                FormationCandidate, FormationOwner, FormationGroup, 5340,
                "dev_seed_4_10_furnace_core",
                "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1");
        }

        private static LayoutResilienceRequirementChannelMigrationSourceRow
            FormationBluestoneCrack()
        {
            return Row(
                "route.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1",
                FormationCandidate, FormationOwner, FormationGroup, 5340,
                "dev_seed_4_10_thunder_fire_cross",
                "dev_encounter_4_10_thunder_fire_cross",
                "dev_map_bluestone_crack",
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1");
        }

        private static LayoutResilienceRequirementChannelMigrationSourceRow
            PollutionBluestoneDamp()
        {
            return Row(
                "route.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1",
                PollutionCandidate, PollutionOwner, PollutionGroup, 5383,
                "dev_seed_3_10_cleanse_corner",
                "dev_encounter_3_10_cleanse_corner",
                "dev_map_bluestone_damp",
                "pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1");
        }

        private static LayoutResilienceRequirementChannelMigrationSourceRow
            PollutionFurnaceAsh()
        {
            return Row(
                "route.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1",
                PollutionCandidate, PollutionOwner, PollutionGroup, 5383,
                "dev_seed_4_10_furnace_core",
                "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall",
                "pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1");
        }

        private static LayoutResilienceRequirementChannelMigrationSourceRow Row(
            string routeId,
            string candidateId,
            string ownerId,
            string groupId,
            int legacyBasisPoints,
            string seedId,
            string encounterId,
            string mapRuleId,
            string pressureInputId)
        {
            return new LayoutResilienceRequirementChannelMigrationSourceRow(
                routeId,
                candidateId,
                ownerId,
                groupId,
                "Required",
                "All",
                "BuildCapability",
                PlacementShapeKey,
                legacyBasisPoints,
                LegacyEvidencePath,
                "Requirement|" + ownerId + "|" + groupId +
                    "|Required|All|BuildCapability|" + PlacementShapeKey,
                true,
                true,
                false,
                false,
                false,
                false,
                seedId,
                encounterId,
                mapRuleId,
                pressureInputId,
                EnemyRequirementChannel.StructuralPredicate,
                EnemyRequirementChannel.StructuralPredicate,
                EnemyRequirementApplicabilityState.Applicable,
                true,
                false,
                true,
                false,
                false,
                false);
        }
    }
}
