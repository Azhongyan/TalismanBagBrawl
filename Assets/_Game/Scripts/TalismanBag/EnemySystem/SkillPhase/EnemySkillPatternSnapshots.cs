using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.SkillPhase
{
    public sealed class SkillPatternPlayerProjection
    {
        private readonly ReadOnlyCollection<string> playerHintCategoryKeys;

        public SkillPatternPlayerProjection(
            string skillPatternId,
            string publicSkillNameKey,
            string intentId,
            string publicIntentTextKey,
            string publicTargetCueKey,
            string publicCastCueKey,
            IEnumerable<string> playerHintCategoryKeys)
        {
            SkillPatternId = EnemySkillBossPhaseReadOnly.Text(skillPatternId);
            PublicSkillNameKey = EnemySkillBossPhaseReadOnly.Text(publicSkillNameKey);
            IntentId = EnemySkillBossPhaseReadOnly.Text(intentId);
            PublicIntentTextKey = EnemySkillBossPhaseReadOnly.Text(publicIntentTextKey);
            PublicTargetCueKey = EnemySkillBossPhaseReadOnly.Text(publicTargetCueKey);
            PublicCastCueKey = EnemySkillBossPhaseReadOnly.Text(publicCastCueKey);
            this.playerHintCategoryKeys = EnemySkillBossPhaseReadOnly.Strings(playerHintCategoryKeys);
        }

        public string SkillPatternId { get; }
        public string PublicSkillNameKey { get; }
        public string IntentId { get; }
        public string PublicIntentTextKey { get; }
        public string PublicTargetCueKey { get; }
        public string PublicCastCueKey { get; }
        public IReadOnlyList<string> PlayerHintCategoryKeys => playerHintCategoryKeys;

        internal SkillPatternPlayerProjection Clone()
        {
            return new SkillPatternPlayerProjection(
                SkillPatternId,
                PublicSkillNameKey,
                IntentId,
                PublicIntentTextKey,
                PublicTargetCueKey,
                PublicCastCueKey,
                playerHintCategoryKeys);
        }
    }

    public sealed class SkillPatternInternalSpec
    {
        private readonly ReadOnlyCollection<string> mechanicProfileIds;

        public SkillPatternInternalSpec(
            SkillCastKind skillCastKind,
            int castDurationMilliseconds,
            int recoveryDurationMilliseconds,
            IEnumerable<string> mechanicProfileIds)
        {
            SkillCastKind = skillCastKind;
            CastDurationMilliseconds = castDurationMilliseconds;
            RecoveryDurationMilliseconds = recoveryDurationMilliseconds;
            this.mechanicProfileIds = EnemySkillBossPhaseReadOnly.Strings(mechanicProfileIds);
        }

        public SkillCastKind SkillCastKind { get; }
        public int CastDurationMilliseconds { get; }
        public int RecoveryDurationMilliseconds { get; }
        public IReadOnlyList<string> MechanicProfileIds => mechanicProfileIds;

        internal SkillPatternInternalSpec Clone()
        {
            return new SkillPatternInternalSpec(
                SkillCastKind,
                CastDurationMilliseconds,
                RecoveryDurationMilliseconds,
                mechanicProfileIds);
        }
    }

    public sealed class SkillPatternDeveloperDiagnostics
    {
        private readonly ReadOnlyCollection<string> developerDiagnosticCategoryKeys;
        private readonly ReadOnlyCollection<string> sourceReferenceIds;

        public SkillPatternDeveloperDiagnostics(
            IEnumerable<string> developerDiagnosticCategoryKeys,
            IEnumerable<string> sourceReferenceIds)
        {
            this.developerDiagnosticCategoryKeys = EnemySkillBossPhaseReadOnly.Strings(
                developerDiagnosticCategoryKeys);
            this.sourceReferenceIds = EnemySkillBossPhaseReadOnly.Strings(sourceReferenceIds);
        }

        public IReadOnlyList<string> DeveloperDiagnosticCategoryKeys =>
            developerDiagnosticCategoryKeys;
        public IReadOnlyList<string> SourceReferenceIds => sourceReferenceIds;
        public bool DeveloperOnly => true;

        internal SkillPatternDeveloperDiagnostics Clone()
        {
            return new SkillPatternDeveloperDiagnostics(
                developerDiagnosticCategoryKeys,
                sourceReferenceIds);
        }
    }

    public sealed class EnemySkillPatternSnapshot
    {
        public EnemySkillPatternSnapshot(
            SkillPatternReference skillPatternReference,
            SkillPatternPlayerProjection playerSafe,
            SkillPatternInternalSpec internalOnly,
            SkillPatternDeveloperDiagnostics developerOnly)
        {
            SkillPatternReference = skillPatternReference == null
                ? null
                : new SkillPatternReference(
                    skillPatternReference.StableId,
                    skillPatternReference.DevOnly,
                    skillPatternReference.IsEnabled,
                    skillPatternReference.EntersFormalFlow);
            PlayerSafe = playerSafe == null ? null : playerSafe.Clone();
            InternalOnly = internalOnly == null ? null : internalOnly.Clone();
            DeveloperOnly = developerOnly == null ? null : developerOnly.Clone();
        }

        public SkillPatternReference SkillPatternReference { get; }
        public string SkillPatternId => SkillPatternReference == null
            ? string.Empty
            : SkillPatternReference.StableId;
        public SkillPatternPlayerProjection PlayerSafe { get; }
        public SkillPatternInternalSpec InternalOnly { get; }
        public SkillPatternDeveloperDiagnostics DeveloperOnly { get; }

        internal EnemySkillPatternSnapshot Clone()
        {
            return new EnemySkillPatternSnapshot(
                SkillPatternReference,
                PlayerSafe,
                InternalOnly,
                DeveloperOnly);
        }
    }

    public sealed class SkillSequenceStepSnapshot
    {
        public SkillSequenceStepSnapshot(
            int stepOrder,
            string skillPatternId,
            int delayAfterPreviousMilliseconds,
            int repeatCount)
        {
            StepOrder = stepOrder;
            SkillPatternId = EnemySkillBossPhaseReadOnly.Text(skillPatternId);
            DelayAfterPreviousMilliseconds = delayAfterPreviousMilliseconds;
            RepeatCount = repeatCount;
        }

        public int StepOrder { get; }
        public string SkillPatternId { get; }
        public int DelayAfterPreviousMilliseconds { get; }
        public int RepeatCount { get; }

        internal SkillSequenceStepSnapshot Clone()
        {
            return new SkillSequenceStepSnapshot(
                StepOrder,
                SkillPatternId,
                DelayAfterPreviousMilliseconds,
                RepeatCount);
        }
    }

    public sealed class SkillSequenceSnapshot : IEnemyDomainIsolationMetadata
    {
        private readonly ReadOnlyCollection<SkillSequenceStepSnapshot> steps;

        public SkillSequenceSnapshot(
            string skillSequenceId,
            IEnumerable<SkillSequenceStepSnapshot> steps,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            SkillSequenceId = EnemySkillBossPhaseReadOnly.Text(skillSequenceId);
            this.steps = EnemySkillBossPhaseReadOnly.Freeze(steps, value => value.Clone())
                .OrderBy(value => value == null ? int.MaxValue : value.StepOrder)
                .ThenBy(value => value == null ? string.Empty : value.SkillPatternId, StringComparer.Ordinal)
                .ToReadOnly();
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string SkillSequenceId { get; }
        public IReadOnlyList<SkillSequenceStepSnapshot> Steps => steps;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal SkillSequenceSnapshot Clone()
        {
            return new SkillSequenceSnapshot(
                SkillSequenceId,
                steps,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }

    public sealed class CarrierSkillBindingSnapshot : IEnemyDomainIsolationMetadata
    {
        private readonly ReadOnlyCollection<string> skillSequenceIds;

        public CarrierSkillBindingSnapshot(
            EnemySkillCarrierKind carrierKind,
            string carrierId,
            IEnumerable<string> skillSequenceIds,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            CarrierKind = carrierKind;
            CarrierId = EnemySkillBossPhaseReadOnly.Text(carrierId);
            this.skillSequenceIds = EnemySkillBossPhaseReadOnly.Strings(skillSequenceIds);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public EnemySkillCarrierKind CarrierKind { get; }
        public string CarrierId { get; }
        public IReadOnlyList<string> SkillSequenceIds => skillSequenceIds;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal CarrierSkillBindingSnapshot Clone()
        {
            return new CarrierSkillBindingSnapshot(
                CarrierKind,
                CarrierId,
                skillSequenceIds,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }
}
