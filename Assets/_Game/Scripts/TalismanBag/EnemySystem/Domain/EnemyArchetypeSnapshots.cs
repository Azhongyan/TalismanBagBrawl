using System.Collections.Generic;

namespace TalismanBag.EnemySystem.Domain
{
    public sealed class EnemyArchetypeSnapshot : IEnemyDomainIsolationMetadata
    {
        private readonly IReadOnlyList<string> mechanicProfileIds;
        private readonly IReadOnlyList<string> skillPatternIds;

        public EnemyArchetypeSnapshot(
            string stableId,
            string displayNameOrLocalizationKey,
            EnemyArchetypeCategory category,
            string presentationKey,
            string baseBehaviorKey,
            IReadOnlyList<string> mechanicProfileIds = null,
            IReadOnlyList<string> skillPatternIds = null,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            StableId = EnemyDomainReadOnly.Text(stableId);
            DisplayNameOrLocalizationKey = EnemyDomainReadOnly.Text(displayNameOrLocalizationKey);
            Category = category;
            PresentationKey = EnemyDomainReadOnly.Text(presentationKey);
            BaseBehaviorKey = EnemyDomainReadOnly.Text(baseBehaviorKey);
            this.mechanicProfileIds = EnemyDomainReadOnly.Ids(mechanicProfileIds);
            this.skillPatternIds = EnemyDomainReadOnly.Ids(skillPatternIds);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string StableId { get; }
        public string DisplayNameOrLocalizationKey { get; }
        public EnemyArchetypeCategory Category { get; }
        public string PresentationKey { get; }
        public string BaseBehaviorKey { get; }
        public IReadOnlyList<string> MechanicProfileIds => mechanicProfileIds;
        public IReadOnlyList<string> SkillPatternIds => skillPatternIds;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal EnemyArchetypeSnapshot Clone()
        {
            return new EnemyArchetypeSnapshot(
                StableId,
                DisplayNameOrLocalizationKey,
                Category,
                PresentationKey,
                BaseBehaviorKey,
                mechanicProfileIds,
                skillPatternIds,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }

    public sealed class BossArchetypeSnapshot : IEnemyDomainIsolationMetadata
    {
        private readonly IReadOnlyList<string> mechanicProfileIds;
        private readonly IReadOnlyList<string> skillPatternIds;
        private readonly IReadOnlyList<string> phaseProfileIds;

        public BossArchetypeSnapshot(
            string stableId,
            string displayNameOrLocalizationKey,
            string presentationKey,
            string baseBehaviorKey,
            IReadOnlyList<string> mechanicProfileIds = null,
            IReadOnlyList<string> skillPatternIds = null,
            IReadOnlyList<string> phaseProfileIds = null,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            StableId = EnemyDomainReadOnly.Text(stableId);
            DisplayNameOrLocalizationKey = EnemyDomainReadOnly.Text(displayNameOrLocalizationKey);
            PresentationKey = EnemyDomainReadOnly.Text(presentationKey);
            BaseBehaviorKey = EnemyDomainReadOnly.Text(baseBehaviorKey);
            this.mechanicProfileIds = EnemyDomainReadOnly.Ids(mechanicProfileIds);
            this.skillPatternIds = EnemyDomainReadOnly.Ids(skillPatternIds);
            this.phaseProfileIds = EnemyDomainReadOnly.Ids(phaseProfileIds);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string StableId { get; }
        public string DisplayNameOrLocalizationKey { get; }
        public EnemyArchetypeCategory Category => EnemyArchetypeCategory.Boss;
        public string PresentationKey { get; }
        public string BaseBehaviorKey { get; }
        public IReadOnlyList<string> MechanicProfileIds => mechanicProfileIds;
        public IReadOnlyList<string> SkillPatternIds => skillPatternIds;
        public IReadOnlyList<string> PhaseProfileIds => phaseProfileIds;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal BossArchetypeSnapshot Clone()
        {
            return new BossArchetypeSnapshot(
                StableId,
                DisplayNameOrLocalizationKey,
                PresentationKey,
                BaseBehaviorKey,
                mechanicProfileIds,
                skillPatternIds,
                phaseProfileIds,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }
}
