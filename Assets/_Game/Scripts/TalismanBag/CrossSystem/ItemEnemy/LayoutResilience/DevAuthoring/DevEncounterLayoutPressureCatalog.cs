using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring
{
    public static class DevEncounterLayoutPressureCatalog
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

        public static DevEncounterLayoutPressureAuthoringSource CreateCandidateSource()
        {
            return new DevEncounterLayoutPressureAuthoringSource(
                DevEncounterLayoutPressureAuthoringSchema.SchemaId,
                DevEncounterLayoutPressureAuthoringSchema.SchemaVersion,
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

        private static DevEncounterLayoutPressureAuthoringSourceRow FormationFurnaceAsh()
        {
            const string pressureId =
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1";
            return Row(FormationCandidate, FormationOwner, FormationGroup,
                "dev_seed_4_10_furnace_core", "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall", pressureId,
                new[]
                {
                    LayoutPressureKind.EyeRelocationOrDisruption,
                    LayoutPressureKind.StructuralConnectionCut
                },
                DomainFormationFurnaceAsh(), UsableFormationFurnaceAsh(), C(2, 1),
                EdgesFormationFurnaceAsh(), FormationClauses());
        }

        private static DevEncounterLayoutPressureAuthoringSourceRow FormationBluestoneCrack()
        {
            const string pressureId =
                "pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1";
            return Row(FormationCandidate, FormationOwner, FormationGroup,
                "dev_seed_4_10_thunder_fire_cross",
                "dev_encounter_4_10_thunder_fire_cross", "dev_map_bluestone_crack",
                pressureId,
                new[]
                {
                    LayoutPressureKind.EyeRelocationOrDisruption,
                    LayoutPressureKind.StructuralConnectionCut
                },
                DomainFormationBluestoneCrack(), UsableFormationBluestoneCrack(),
                C(3, 2), EdgesFormationBluestoneCrack(), FormationClauses());
        }

        private static DevEncounterLayoutPressureAuthoringSourceRow PollutionBluestoneDamp()
        {
            const string pressureId =
                "pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1";
            return Row(PollutionCandidate, PollutionOwner, PollutionGroup,
                "dev_seed_3_10_cleanse_corner", "dev_encounter_3_10_cleanse_corner",
                "dev_map_bluestone_damp", pressureId,
                new[] { LayoutPressureKind.PollutedCellMask },
                DomainPollutionBluestoneDamp(), UsablePollutionBluestoneDamp(),
                C(2, 2), EdgesPollutionBluestoneDamp(), PollutionClauses());
        }

        private static DevEncounterLayoutPressureAuthoringSourceRow PollutionFurnaceAsh()
        {
            const string pressureId =
                "pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1";
            return Row(PollutionCandidate, PollutionOwner, PollutionGroup,
                "dev_seed_4_10_furnace_core", "dev_encounter_4_10_furnace_core",
                "dev_map_furnace_ash_fall", pressureId,
                new[] { LayoutPressureKind.PollutedCellMask },
                DomainPollutionFurnaceAsh(), UsablePollutionFurnaceAsh(), C(2, 2),
                EdgesPollutionFurnaceAsh(), PollutionClauses());
        }

        private static DevEncounterLayoutPressureAuthoringSourceRow Row(
            string candidate, string owner, string group, string seed,
            string encounter, string map, string pressureId,
            LayoutPressureKind[] kinds, LayoutCellCoordinate[] domain,
            LayoutCellCoordinate[] usable, LayoutCellCoordinate eye,
            LayoutCellConnection[] edges,
            LayoutResiliencePredicateClauseKind[] clauses)
        {
            return new DevEncounterLayoutPressureAuthoringSourceRow(
                candidate, owner, group, PlacementShapeKey, seed, encounter, map,
                pressureId, true, false, false, false,
                new AuthoredLayoutPressureSourceInput(
                    AuthoredLayoutPressureAuthoringCompleteness.Complete,
                    pressureId, 5, kinds, domain, usable, eye, edges, clauses));
        }

        private static LayoutResiliencePredicateClauseKind[] FormationClauses()
        {
            return new[]
            {
                LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable,
                LayoutResiliencePredicateClauseKind
                    .EyeToCountedCoreStructurallyConnected
            };
        }

        private static LayoutResiliencePredicateClauseKind[] PollutionClauses()
        {
            return new[]
            {
                LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
                LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable
            };
        }

        private static LayoutCellCoordinate[] DomainFormationFurnaceAsh()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1), C(1,1), C(2,1), C(3,1), C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3), C(1,3), C(2,3), C(3,3), C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellCoordinate[] UsableFormationFurnaceAsh()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1), C(1,1), C(2,1), C(3,1), C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3), C(1,3), C(2,3), C(3,3), C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellCoordinate[] DomainFormationBluestoneCrack()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1), C(1,1), C(2,1), C(3,1), C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3), C(1,3), C(2,3), C(3,3), C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellCoordinate[] UsableFormationBluestoneCrack()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1), C(1,1), C(2,1), C(3,1), C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3), C(1,3), C(2,3), C(3,3), C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellCoordinate[] DomainPollutionBluestoneDamp()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1), C(1,1), C(2,1), C(3,1), C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3), C(1,3), C(2,3), C(3,3), C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellCoordinate[] UsablePollutionBluestoneDamp()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1),         C(2,1), C(3,1), C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3), C(1,3), C(2,3),         C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellCoordinate[] DomainPollutionFurnaceAsh()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1), C(1,1), C(2,1), C(3,1), C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3), C(1,3), C(2,3), C(3,3), C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellCoordinate[] UsablePollutionFurnaceAsh()
        {
            return new[]
            {
                C(0,0), C(1,0), C(2,0), C(3,0), C(4,0),
                C(0,1),         C(2,1),         C(4,1),
                C(0,2), C(1,2), C(2,2), C(3,2), C(4,2),
                C(0,3),         C(2,3),         C(4,3),
                C(0,4), C(1,4), C(2,4), C(3,4), C(4,4)
            };
        }

        private static LayoutCellConnection[] EdgesFormationFurnaceAsh()
        {
            return new[]
            {
                E(0,0,1,0), E(0,0,0,1), E(1,0,2,0), E(1,0,1,1),
                E(2,0,3,0), E(2,0,2,1), E(3,0,4,0), E(3,0,3,1),
                E(4,0,4,1), E(0,1,1,1), E(1,1,2,1), E(2,1,3,1),
                E(3,1,4,1), E(0,2,1,2), E(0,2,0,3), E(1,2,2,2),
                E(1,2,1,3), E(2,2,3,2), E(2,2,2,3), E(3,2,4,2),
                E(3,2,3,3), E(4,2,4,3), E(0,3,1,3), E(0,3,0,4),
                E(1,3,2,3), E(1,3,1,4), E(2,3,3,3), E(2,3,2,4),
                E(3,3,4,3), E(3,3,3,4), E(4,3,4,4), E(0,4,1,4),
                E(1,4,2,4), E(2,4,3,4), E(3,4,4,4)
            };
        }

        private static LayoutCellConnection[] EdgesFormationBluestoneCrack()
        {
            return new[]
            {
                E(0,0,1,0), E(0,0,0,1), E(1,0,2,0), E(1,0,1,1),
                E(2,0,2,1), E(3,0,4,0), E(3,0,3,1), E(4,0,4,1),
                E(0,1,1,1), E(0,1,0,2), E(1,1,2,1), E(1,1,1,2),
                E(2,1,2,2), E(3,1,4,1), E(3,1,3,2), E(4,1,4,2),
                E(0,2,1,2), E(0,2,0,3), E(1,2,2,2), E(1,2,1,3),
                E(2,2,2,3), E(3,2,4,2), E(3,2,3,3), E(4,2,4,3),
                E(0,3,1,3), E(0,3,0,4), E(1,3,2,3), E(1,3,1,4),
                E(2,3,2,4), E(3,3,4,3), E(3,3,3,4), E(4,3,4,4),
                E(0,4,1,4), E(1,4,2,4), E(3,4,4,4)
            };
        }

        private static LayoutCellConnection[] EdgesPollutionBluestoneDamp()
        {
            return new[]
            {
                E(0,0,1,0), E(0,0,0,1), E(1,0,2,0), E(2,0,3,0),
                E(2,0,2,1), E(3,0,4,0), E(3,0,3,1), E(4,0,4,1),
                E(0,1,0,2), E(2,1,3,1), E(2,1,2,2), E(3,1,4,1),
                E(3,1,3,2), E(4,1,4,2), E(0,2,1,2), E(0,2,0,3),
                E(1,2,2,2), E(1,2,1,3), E(2,2,3,2), E(2,2,2,3),
                E(3,2,4,2), E(4,2,4,3), E(0,3,1,3), E(0,3,0,4),
                E(1,3,2,3), E(1,3,1,4), E(2,3,2,4), E(4,3,4,4),
                E(0,4,1,4), E(1,4,2,4), E(2,4,3,4), E(3,4,4,4)
            };
        }

        private static LayoutCellConnection[] EdgesPollutionFurnaceAsh()
        {
            return new[]
            {
                E(0,0,1,0), E(0,0,0,1), E(1,0,2,0), E(2,0,3,0),
                E(2,0,2,1), E(3,0,4,0), E(4,0,4,1), E(0,1,0,2),
                E(2,1,2,2), E(4,1,4,2), E(0,2,1,2), E(0,2,0,3),
                E(1,2,2,2), E(2,2,3,2), E(2,2,2,3), E(3,2,4,2),
                E(4,2,4,3), E(0,3,0,4), E(2,3,2,4), E(4,3,4,4),
                E(0,4,1,4), E(1,4,2,4), E(2,4,3,4), E(3,4,4,4)
            };
        }

        private static LayoutCellCoordinate C(int x, int y)
        {
            return new LayoutCellCoordinate(x, y);
        }

        private static LayoutCellConnection E(int ax, int ay, int bx, int by)
        {
            return new LayoutCellConnection(C(ax, ay), C(bx, by));
        }
    }
}
