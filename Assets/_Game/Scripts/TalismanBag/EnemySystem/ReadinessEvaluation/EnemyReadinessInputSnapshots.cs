using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.PressureWindow;

namespace TalismanBag.EnemySystem.ReadinessEvaluation
{
    public sealed class MapRuleCapabilityAdjustmentSnapshot
    {
        public MapRuleCapabilityAdjustmentSnapshot(
            string mapRuleId,
            string buildCapabilityKey,
            int deltaBasisPoints,
            string developerReasonId)
        {
            MapRuleId = EnemyReadinessReadOnly.Text(mapRuleId);
            BuildCapabilityKey = EnemyReadinessReadOnly.Text(buildCapabilityKey);
            DeltaBasisPoints = deltaBasisPoints;
            DeveloperReasonId = EnemyReadinessReadOnly.Text(developerReasonId);
        }

        public string MapRuleId { get; }
        public string BuildCapabilityKey { get; }
        public int DeltaBasisPoints { get; }
        public string DeveloperReasonId { get; }

        internal MapRuleCapabilityAdjustmentSnapshot Clone()
        {
            return new MapRuleCapabilityAdjustmentSnapshot(
                MapRuleId,
                BuildCapabilityKey,
                DeltaBasisPoints,
                DeveloperReasonId);
        }
    }

    public sealed class EnemyReadinessEvaluationInput
    {
        private readonly ReadOnlyCollection<MapRuleCapabilityAdjustmentSnapshot>
            mapRuleCapabilityAdjustments;

        public EnemyReadinessEvaluationInput(
            BuildCapabilitySnapshot buildCapabilitySnapshot,
            CounterWindowAndPressureCatalogSnapshot pressureCatalog,
            string buildPressureProfileId,
            IEnumerable<MapRuleCapabilityAdjustmentSnapshot>
                mapRuleCapabilityAdjustments)
        {
            BuildCapabilitySnapshot = buildCapabilitySnapshot;
            PressureCatalog = pressureCatalog;
            BuildPressureProfileId = EnemyReadinessReadOnly.Text(
                buildPressureProfileId);
            this.mapRuleCapabilityAdjustments =
                EnemyReadinessReadOnly.Freeze(
                        mapRuleCapabilityAdjustments,
                        value => value.Clone())
                    .OrderBy(
                        value => value == null ? string.Empty : value.MapRuleId,
                        StringComparer.Ordinal)
                    .ThenBy(
                        value => value == null
                            ? string.Empty
                            : value.BuildCapabilityKey,
                        StringComparer.Ordinal)
                    .ThenBy(
                        value => value == null
                            ? string.Empty
                            : value.DeveloperReasonId,
                        StringComparer.Ordinal)
                    .ThenBy(value => value == null ? 0 : value.DeltaBasisPoints)
                    .ToReadOnly();
        }

        public BuildCapabilitySnapshot BuildCapabilitySnapshot { get; }
        public CounterWindowAndPressureCatalogSnapshot PressureCatalog { get; }
        public string BuildPressureProfileId { get; }
        public IReadOnlyList<MapRuleCapabilityAdjustmentSnapshot>
            MapRuleCapabilityAdjustments => mapRuleCapabilityAdjustments;
    }
}
