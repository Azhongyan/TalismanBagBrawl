using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.PressureWindow
{
    public sealed class BuildPressurePlayerProjection
    {
        private readonly ReadOnlyCollection<string> playerHintCategoryKeys;

        public BuildPressurePlayerProjection(
            string buildPressureProfileId,
            string publicPressureLabelKey,
            string publicPressureHintKey,
            IEnumerable<string> playerHintCategoryKeys)
        {
            BuildPressureProfileId = CounterWindowAndPressureReadOnly.Text(
                buildPressureProfileId);
            PublicPressureLabelKey = CounterWindowAndPressureReadOnly.Text(
                publicPressureLabelKey);
            PublicPressureHintKey = CounterWindowAndPressureReadOnly.Text(
                publicPressureHintKey);
            this.playerHintCategoryKeys = CounterWindowAndPressureReadOnly.Strings(
                playerHintCategoryKeys);
        }

        public string BuildPressureProfileId { get; }
        public string PublicPressureLabelKey { get; }
        public string PublicPressureHintKey { get; }
        public IReadOnlyList<string> PlayerHintCategoryKeys => playerHintCategoryKeys;

        internal BuildPressurePlayerProjection Clone()
        {
            return new BuildPressurePlayerProjection(
                BuildPressureProfileId,
                PublicPressureLabelKey,
                PublicPressureHintKey,
                playerHintCategoryKeys);
        }
    }

    public sealed class PressureChannelContributionSnapshot
    {
        public PressureChannelContributionSnapshot(
            string pressureChannelKey,
            int weightBasisPoints)
        {
            PressureChannelKey = CounterWindowAndPressureReadOnly.Text(pressureChannelKey);
            WeightBasisPoints = weightBasisPoints;
        }

        public string PressureChannelKey { get; }
        public int WeightBasisPoints { get; }

        internal PressureChannelContributionSnapshot Clone()
        {
            return new PressureChannelContributionSnapshot(
                PressureChannelKey,
                WeightBasisPoints);
        }
    }

    public sealed class BuildCapabilityRequirementSnapshot
    {
        public BuildCapabilityRequirementSnapshot(
            string buildCapabilityKey,
            int minimumCapabilityBasisPoints)
        {
            BuildCapabilityKey = CounterWindowAndPressureReadOnly.Text(buildCapabilityKey);
            MinimumCapabilityBasisPoints = minimumCapabilityBasisPoints;
        }

        public string BuildCapabilityKey { get; }
        public int MinimumCapabilityBasisPoints { get; }

        internal BuildCapabilityRequirementSnapshot Clone()
        {
            return new BuildCapabilityRequirementSnapshot(
                BuildCapabilityKey,
                MinimumCapabilityBasisPoints);
        }
    }

    public sealed class BuildCapabilityRequirementGroupSnapshot
    {
        private readonly ReadOnlyCollection<BuildCapabilityRequirementSnapshot> requirements;

        public BuildCapabilityRequirementGroupSnapshot(
            string requirementGroupId,
            CapabilityRequirementRole requirementRole,
            RequirementMatchMode requirementMatchMode,
            IEnumerable<BuildCapabilityRequirementSnapshot> requirements)
        {
            RequirementGroupId = CounterWindowAndPressureReadOnly.Text(requirementGroupId);
            RequirementRole = requirementRole;
            RequirementMatchMode = requirementMatchMode;
            this.requirements = CounterWindowAndPressureReadOnly.Freeze(
                    requirements,
                    value => value.Clone())
                .OrderBy(
                    value => value == null ? string.Empty : value.BuildCapabilityKey,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null
                    ? int.MinValue
                    : value.MinimumCapabilityBasisPoints)
                .ToReadOnly();
        }

        public string RequirementGroupId { get; }
        public CapabilityRequirementRole RequirementRole { get; }
        public RequirementMatchMode RequirementMatchMode { get; }
        public IReadOnlyList<BuildCapabilityRequirementSnapshot> Requirements => requirements;

        internal BuildCapabilityRequirementGroupSnapshot Clone()
        {
            return new BuildCapabilityRequirementGroupSnapshot(
                RequirementGroupId,
                RequirementRole,
                RequirementMatchMode,
                requirements);
        }
    }

    public sealed class BuildPressureInternalSpec
    {
        private readonly ReadOnlyCollection<PressureChannelContributionSnapshot> contributions;

        public BuildPressureInternalSpec(
            IEnumerable<PressureChannelContributionSnapshot> pressureChannelContributions)
        {
            contributions = CounterWindowAndPressureReadOnly.Freeze(
                    pressureChannelContributions,
                    value => value.Clone())
                .OrderBy(
                    value => value == null ? string.Empty : value.PressureChannelKey,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? int.MinValue : value.WeightBasisPoints)
                .ToReadOnly();
        }

        public IReadOnlyList<PressureChannelContributionSnapshot>
            PressureChannelContributions => contributions;

        internal BuildPressureInternalSpec Clone()
        {
            return new BuildPressureInternalSpec(contributions);
        }
    }

    public sealed class BuildPressureDeveloperDiagnostics
    {
        private readonly ReadOnlyCollection<BuildCapabilityRequirementGroupSnapshot>
            requirementGroups;
        private readonly ReadOnlyCollection<string> developerDiagnosticCategoryKeys;
        private readonly ReadOnlyCollection<string> sourceReferenceIds;

        public BuildPressureDeveloperDiagnostics(
            IEnumerable<BuildCapabilityRequirementGroupSnapshot> requirementGroups,
            IEnumerable<string> developerDiagnosticCategoryKeys,
            IEnumerable<string> sourceReferenceIds)
        {
            this.requirementGroups = CounterWindowAndPressureReadOnly.Freeze(
                    requirementGroups,
                    value => value.Clone())
                .OrderBy(
                    value => value == null ? string.Empty : value.RequirementGroupId,
                    StringComparer.Ordinal)
                .ToReadOnly();
            this.developerDiagnosticCategoryKeys =
                CounterWindowAndPressureReadOnly.Strings(
                    developerDiagnosticCategoryKeys);
            this.sourceReferenceIds = CounterWindowAndPressureReadOnly.Strings(
                sourceReferenceIds);
        }

        public IReadOnlyList<BuildCapabilityRequirementGroupSnapshot> RequirementGroups =>
            requirementGroups;
        public IReadOnlyList<string> DeveloperDiagnosticCategoryKeys =>
            developerDiagnosticCategoryKeys;
        public IReadOnlyList<string> SourceReferenceIds => sourceReferenceIds;
        public bool DeveloperOnly => true;

        internal BuildPressureDeveloperDiagnostics Clone()
        {
            return new BuildPressureDeveloperDiagnostics(
                requirementGroups,
                developerDiagnosticCategoryKeys,
                sourceReferenceIds);
        }
    }

    public sealed class BuildPressureProfileSnapshot : IEnemyDomainIsolationMetadata
    {
        public BuildPressureProfileSnapshot(
            string buildPressureProfileId,
            BuildPressurePlayerProjection playerSafe,
            BuildPressureInternalSpec internalOnly,
            BuildPressureDeveloperDiagnostics developerOnly,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            BuildPressureProfileId = CounterWindowAndPressureReadOnly.Text(
                buildPressureProfileId);
            PlayerSafe = playerSafe == null ? null : playerSafe.Clone();
            InternalOnly = internalOnly == null ? null : internalOnly.Clone();
            DeveloperOnly = developerOnly == null ? null : developerOnly.Clone();
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string BuildPressureProfileId { get; }
        public BuildPressurePlayerProjection PlayerSafe { get; }
        public BuildPressureInternalSpec InternalOnly { get; }
        public BuildPressureDeveloperDiagnostics DeveloperOnly { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal BuildPressureProfileSnapshot Clone()
        {
            return new BuildPressureProfileSnapshot(
                BuildPressureProfileId,
                PlayerSafe,
                InternalOnly,
                DeveloperOnly,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }

    public sealed class PressureSourceBindingSnapshot : IEnemyDomainIsolationMetadata
    {
        public PressureSourceBindingSnapshot(
            PressureSourceKind sourceKind,
            string sourceId,
            string buildPressureProfileId,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            SourceKind = sourceKind;
            SourceId = CounterWindowAndPressureReadOnly.Text(sourceId);
            BuildPressureProfileId = CounterWindowAndPressureReadOnly.Text(
                buildPressureProfileId);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public PressureSourceKind SourceKind { get; }
        public string SourceId { get; }
        public string BuildPressureProfileId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal PressureSourceBindingSnapshot Clone()
        {
            return new PressureSourceBindingSnapshot(
                SourceKind,
                SourceId,
                BuildPressureProfileId,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }
}
