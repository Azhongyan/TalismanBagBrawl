using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.Contracts
{
    public abstract class EnemyDomainReference : IEnemyDomainIsolationMetadata
    {
        protected EnemyDomainReference(
            string stableId,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            StableId = EnemyDomainReadOnly.Text(stableId);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string StableId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
    }

    public sealed class MechanicProfileReference : EnemyDomainReference
    {
        public MechanicProfileReference(string stableId, bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
            : base(stableId, devOnly, isEnabled, entersFormalFlow)
        {
        }

        internal MechanicProfileReference Clone()
        {
            return new MechanicProfileReference(StableId, DevOnly, IsEnabled, EntersFormalFlow);
        }
    }

    public sealed class SkillPatternReference : EnemyDomainReference
    {
        public SkillPatternReference(string stableId, bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
            : base(stableId, devOnly, isEnabled, entersFormalFlow)
        {
        }

        internal SkillPatternReference Clone()
        {
            return new SkillPatternReference(StableId, DevOnly, IsEnabled, EntersFormalFlow);
        }
    }

    public sealed class BossPhaseReference : EnemyDomainReference
    {
        public BossPhaseReference(string stableId, bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
            : base(stableId, devOnly, isEnabled, entersFormalFlow)
        {
        }

        internal BossPhaseReference Clone()
        {
            return new BossPhaseReference(StableId, DevOnly, IsEnabled, EntersFormalFlow);
        }
    }

    public sealed class MapRuleReference : EnemyDomainReference
    {
        public MapRuleReference(string stableId, bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
            : base(stableId, devOnly, isEnabled, entersFormalFlow)
        {
        }

        internal MapRuleReference Clone()
        {
            return new MapRuleReference(StableId, DevOnly, IsEnabled, EntersFormalFlow);
        }
    }

    public sealed class EncounterReference : EnemyDomainReference
    {
        public EncounterReference(string stableId, bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
            : base(stableId, devOnly, isEnabled, entersFormalFlow)
        {
        }

        internal EncounterReference Clone()
        {
            return new EncounterReference(StableId, DevOnly, IsEnabled, EntersFormalFlow);
        }
    }

    public sealed class CounterWindowReference : EnemyDomainReference
    {
        public CounterWindowReference(string stableId, bool devOnly = true, bool isEnabled = false, bool entersFormalFlow = false)
            : base(stableId, devOnly, isEnabled, entersFormalFlow)
        {
        }

        internal CounterWindowReference Clone()
        {
            return new CounterWindowReference(StableId, DevOnly, IsEnabled, EntersFormalFlow);
        }
    }
}
