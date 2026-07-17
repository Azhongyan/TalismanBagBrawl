using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SkillPhase
{
    public sealed class EnemySkillBossPhaseValidationIssue
    {
        public EnemySkillBossPhaseValidationIssue(string code, string path, string message)
        {
            Code = code ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public string Path { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Code + " @ " + Path + ": " + Message;
        }
    }

    public sealed class EnemySkillBossPhaseValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<EnemySkillBossPhaseValidationIssue> issues;

        public EnemySkillBossPhaseValidationException(
            IReadOnlyList<EnemySkillBossPhaseValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly(
                (issues ?? Array.Empty<EnemySkillBossPhaseValidationIssue>()).ToArray());
        }

        public IReadOnlyList<EnemySkillBossPhaseValidationIssue> Issues => issues;

        private static string BuildMessage(
            IReadOnlyList<EnemySkillBossPhaseValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(
                "Enemy skill and Boss phase schema validation failed.");
            foreach (EnemySkillBossPhaseValidationIssue issue
                in issues ?? Array.Empty<EnemySkillBossPhaseValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class DefaultEnemySkillBossPhaseProvider : IEnemySkillBossPhaseProvider
    {
        public static readonly DefaultEnemySkillBossPhaseProvider Instance =
            new DefaultEnemySkillBossPhaseProvider();

        private readonly IEnemySkillBossPhaseValidator validator;

        public DefaultEnemySkillBossPhaseProvider(IEnemySkillBossPhaseValidator validator = null)
        {
            this.validator = validator ?? DefaultEnemySkillBossPhaseValidator.Instance;
        }

        public EnemySkillBossPhaseCatalogSnapshot CreateSnapshot(
            EnemySkillBossPhaseCatalogInput input,
            IEnemySkillBossPhaseReferenceResolver resolver)
        {
            IReadOnlyList<EnemySkillBossPhaseValidationIssue> issues =
                validator.Validate(input, resolver);
            if (issues.Count > 0)
            {
                throw new EnemySkillBossPhaseValidationException(issues);
            }

            return new EnemySkillBossPhaseCatalogSnapshot(input);
        }
    }

    public sealed class DefaultEnemySkillBossPhaseValidator : IEnemySkillBossPhaseValidator
    {
        public static readonly DefaultEnemySkillBossPhaseValidator Instance =
            new DefaultEnemySkillBossPhaseValidator();

        public IReadOnlyList<EnemySkillBossPhaseValidationIssue> Validate(
            EnemySkillBossPhaseCatalogInput input,
            IEnemySkillBossPhaseReferenceResolver resolver)
        {
            List<EnemySkillBossPhaseValidationIssue> issues =
                new List<EnemySkillBossPhaseValidationIssue>();
            if (input == null)
            {
                issues.Add(Issue("INPUT_NULL", "$", "Catalog input is required."));
                return Array.AsReadOnly(issues.ToArray());
            }

            if (!string.Equals(
                input.SchemaId,
                EnemySkillBossPhaseSchema.SchemaId,
                StringComparison.Ordinal))
            {
                issues.Add(Issue(
                    "SCHEMA_ID_MISMATCH",
                    "schemaId",
                    "Schema ID must match EnemySkillBossPhase.v1 exactly."));
            }

            if (input.SchemaVersion != EnemySkillBossPhaseSchema.SchemaVersion)
            {
                issues.Add(Issue(
                    "SCHEMA_VERSION_MISMATCH",
                    "schemaVersion",
                    "Schema version must equal 1."));
            }

            if (resolver == null)
            {
                issues.Add(Issue(
                    "REFERENCE_RESOLVER_NULL",
                    "$",
                    "A read-only E02/E03 reference resolver is required."));
            }

            Dictionary<string, EnemySkillPatternSnapshot> patterns =
                ValidatePatterns(input.SkillPatterns, resolver, issues);
            Dictionary<string, SkillSequenceSnapshot> sequences =
                ValidateSequences(input.SkillSequences, patterns, issues);
            ValidateCarrierBindings(
                input.CarrierSkillBindings,
                resolver,
                patterns,
                sequences,
                issues);
            Dictionary<string, BossPhaseProfileSnapshot> phases =
                ValidateBossPhases(
                    input.BossPhaseProfiles,
                    resolver,
                    patterns,
                    sequences,
                    issues);
            ValidateBossPlans(
                input.BossPhasePlans,
                resolver,
                patterns,
                sequences,
                phases,
                issues);

            return Array.AsReadOnly(issues.ToArray());
        }

        private static Dictionary<string, EnemySkillPatternSnapshot> ValidatePatterns(
            IReadOnlyList<EnemySkillPatternSnapshot> values,
            IEnemySkillBossPhaseReferenceResolver resolver,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            Dictionary<string, EnemySkillPatternSnapshot> byId =
                new Dictionary<string, EnemySkillPatternSnapshot>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                EnemySkillPatternSnapshot pattern = values[index];
                string path = "skillPatterns[" + index + "]";
                if (pattern == null)
                {
                    issues.Add(Issue("SKILL_PATTERN_NULL", path, "SkillPattern cannot be null."));
                    continue;
                }

                if (pattern.SkillPatternReference == null)
                {
                    issues.Add(Issue(
                        "SKILL_PATTERN_REFERENCE_NULL",
                        path + ".skillPatternReference",
                        "E01 SkillPatternReference is required."));
                }
                else
                {
                    ValidateId(pattern.SkillPatternId, path + ".skillPatternReference.stableId", issues);
                    ValidateIsolation(pattern.SkillPatternReference, path + ".skillPatternReference", issues);
                }

                AddUnique(
                    byId,
                    pattern.SkillPatternId,
                    pattern,
                    path + ".skillPatternReference.stableId",
                    "SKILL_PATTERN_ID_DUPLICATE",
                    issues);

                if (pattern.PlayerSafe == null)
                {
                    issues.Add(Issue("SKILL_PLAYER_PROJECTION_NULL", path + ".playerSafe", "Player-safe projection is required."));
                }
                else
                {
                    if (!string.Equals(
                        pattern.PlayerSafe.SkillPatternId,
                        pattern.SkillPatternId,
                        StringComparison.Ordinal))
                    {
                        issues.Add(Issue(
                            "SKILL_PLAYER_ID_MISMATCH",
                            path + ".playerSafe.skillPatternId",
                            "Player-safe SkillPatternId must equal its E01 reference exactly."));
                    }

                    ValidateId(pattern.PlayerSafe.SkillPatternId, path + ".playerSafe.skillPatternId", issues);
                    ValidateId(pattern.PlayerSafe.PublicSkillNameKey, path + ".playerSafe.publicSkillNameKey", issues);
                    ValidateId(pattern.PlayerSafe.IntentId, path + ".playerSafe.intentId", issues);
                    ValidateId(pattern.PlayerSafe.PublicIntentTextKey, path + ".playerSafe.publicIntentTextKey", issues);
                    ValidateId(pattern.PlayerSafe.PublicTargetCueKey, path + ".playerSafe.publicTargetCueKey", issues);
                    ValidateId(pattern.PlayerSafe.PublicCastCueKey, path + ".playerSafe.publicCastCueKey", issues);
                    ValidateVocabularyKeys(
                        pattern.PlayerSafe.PlayerHintCategoryKeys,
                        EnemyVocabularyCategory.PlayerHintCategory,
                        path + ".playerSafe.playerHintCategoryKeys",
                        "PLAYER_HINT_CATEGORY",
                        resolver,
                        issues);
                }

                if (pattern.InternalOnly == null)
                {
                    issues.Add(Issue("SKILL_INTERNAL_SPEC_NULL", path + ".internalOnly", "Internal skill specification is required."));
                }
                else
                {
                    ValidateSkillInternal(pattern.InternalOnly, path + ".internalOnly", resolver, issues);
                }

                if (pattern.DeveloperOnly == null)
                {
                    issues.Add(Issue("SKILL_DIAGNOSTICS_NULL", path + ".developerOnly", "Developer diagnostics are required."));
                }
                else
                {
                    ValidateDeveloperDiagnostics(
                        pattern.DeveloperOnly.DeveloperDiagnosticCategoryKeys,
                        pattern.DeveloperOnly.SourceReferenceIds,
                        path + ".developerOnly",
                        resolver,
                        issues);
                }
            }

            return byId;
        }

        private static void ValidateSkillInternal(
            SkillPatternInternalSpec value,
            string path,
            IEnemySkillBossPhaseReferenceResolver resolver,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            if (!Enum.IsDefined(typeof(SkillCastKind), value.SkillCastKind))
            {
                issues.Add(Issue("SKILL_CAST_KIND_INVALID", path + ".skillCastKind", "SkillCastKind must be Instant or Channeled."));
            }

            if (value.CastDurationMilliseconds < 0)
            {
                issues.Add(Issue("CAST_DURATION_NEGATIVE", path + ".castDurationMilliseconds", "Cast duration cannot be negative."));
            }

            if (value.RecoveryDurationMilliseconds < 0)
            {
                issues.Add(Issue("RECOVERY_DURATION_NEGATIVE", path + ".recoveryDurationMilliseconds", "Recovery duration cannot be negative."));
            }

            if (value.SkillCastKind == SkillCastKind.Instant
                && value.CastDurationMilliseconds != 0)
            {
                issues.Add(Issue("INSTANT_CAST_DURATION_INVALID", path + ".castDurationMilliseconds", "Instant skills require a zero cast duration."));
            }

            if (value.SkillCastKind == SkillCastKind.Channeled
                && value.CastDurationMilliseconds <= 0)
            {
                issues.Add(Issue("CHANNELED_CAST_DURATION_INVALID", path + ".castDurationMilliseconds", "Channeled skills require a positive cast duration."));
            }

            ValidateMechanicProfileIds(
                value.MechanicProfileIds,
                null,
                path + ".mechanicProfileIds",
                resolver,
                issues);
        }

        private static Dictionary<string, SkillSequenceSnapshot> ValidateSequences(
            IReadOnlyList<SkillSequenceSnapshot> values,
            IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patterns,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            Dictionary<string, SkillSequenceSnapshot> byId =
                new Dictionary<string, SkillSequenceSnapshot>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                SkillSequenceSnapshot sequence = values[index];
                string path = "skillSequences[" + index + "]";
                if (sequence == null)
                {
                    issues.Add(Issue("SKILL_SEQUENCE_NULL", path, "SkillSequence cannot be null."));
                    continue;
                }

                ValidateId(sequence.SkillSequenceId, path + ".skillSequenceId", issues);
                ValidateIsolation(sequence, path, issues);
                AddUnique(
                    byId,
                    sequence.SkillSequenceId,
                    sequence,
                    path + ".skillSequenceId",
                    "SKILL_SEQUENCE_ID_DUPLICATE",
                    issues);

                if (sequence.Steps.Count == 0)
                {
                    issues.Add(Issue("SKILL_SEQUENCE_EMPTY", path + ".steps", "SkillSequence requires at least one step."));
                    continue;
                }

                Dictionary<int, int> firstByOrder = new Dictionary<int, int>();
                for (int stepIndex = 0; stepIndex < sequence.Steps.Count; stepIndex++)
                {
                    SkillSequenceStepSnapshot step = sequence.Steps[stepIndex];
                    string stepPath = path + ".steps[" + stepIndex + "]";
                    if (step == null)
                    {
                        issues.Add(Issue("SKILL_SEQUENCE_STEP_NULL", stepPath, "Sequence step cannot be null."));
                        continue;
                    }

                    if (step.StepOrder < 0)
                    {
                        issues.Add(Issue("STEP_ORDER_NEGATIVE", stepPath + ".stepOrder", "StepOrder must be non-negative."));
                    }

                    if (firstByOrder.TryGetValue(step.StepOrder, out int firstIndex))
                    {
                        issues.Add(Issue("STEP_ORDER_DUPLICATE", stepPath + ".stepOrder", "Duplicate StepOrder; first seen at index " + firstIndex + "."));
                    }
                    else
                    {
                        firstByOrder.Add(step.StepOrder, stepIndex);
                    }

                    ValidateId(step.SkillPatternId, stepPath + ".skillPatternId", issues);
                    if (!patterns.ContainsKey(step.SkillPatternId))
                    {
                        issues.Add(Issue("SKILL_PATTERN_REFERENCE_UNRESOLVED", stepPath + ".skillPatternId", "SkillPattern ID does not resolve with ordinal semantics."));
                    }

                    if (step.DelayAfterPreviousMilliseconds < 0)
                    {
                        issues.Add(Issue("STEP_DELAY_NEGATIVE", stepPath + ".delayAfterPreviousMilliseconds", "Step delay cannot be negative."));
                    }

                    if (step.RepeatCount < 1)
                    {
                        issues.Add(Issue("STEP_REPEAT_COUNT_INVALID", stepPath + ".repeatCount", "RepeatCount must be at least one."));
                    }
                }

                ValidateContiguousOrder(
                    sequence.Steps.Where(value => value != null).Select(value => value.StepOrder),
                    path + ".steps",
                    "STEP_ORDER_NON_CONTIGUOUS",
                    issues);
            }

            return byId;
        }

        private static void ValidateCarrierBindings(
            IReadOnlyList<CarrierSkillBindingSnapshot> values,
            IEnemySkillBossPhaseReferenceResolver resolver,
            IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patterns,
            IReadOnlyDictionary<string, SkillSequenceSnapshot> sequences,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            Dictionary<string, int> firstByCarrier = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                CarrierSkillBindingSnapshot binding = values[index];
                string path = "carrierSkillBindings[" + index + "]";
                if (binding == null)
                {
                    issues.Add(Issue("CARRIER_SKILL_BINDING_NULL", path, "CarrierSkillBinding cannot be null."));
                    continue;
                }

                bool kindDefined = Enum.IsDefined(typeof(EnemySkillCarrierKind), binding.CarrierKind);
                if (!kindDefined)
                {
                    issues.Add(Issue("CARRIER_KIND_INVALID", path + ".carrierKind", "Carrier kind must be Enemy or Boss."));
                }

                ValidateId(binding.CarrierId, path + ".carrierId", issues);
                ValidateIsolation(binding, path, issues);
                string identity = EnemySkillBossPhaseCatalogSnapshot.CarrierIdentity(
                    binding.CarrierKind,
                    binding.CarrierId);
                if (firstByCarrier.TryGetValue(identity, out int firstIndex))
                {
                    issues.Add(Issue("CARRIER_SKILL_BINDING_DUPLICATE", path + ".carrierId", "Duplicate exact Carrier binding; first seen at index " + firstIndex + "."));
                }
                else
                {
                    firstByCarrier.Add(identity, index);
                }

                bool carrierResolved = ResolveCarrier(binding.CarrierKind, binding.CarrierId, resolver);
                if (resolver != null && kindDefined && !carrierResolved)
                {
                    issues.Add(Issue(
                        binding.CarrierKind == EnemySkillCarrierKind.Enemy
                            ? "ENEMY_REFERENCE_UNRESOLVED"
                            : "BOSS_REFERENCE_UNRESOLVED",
                        path + ".carrierId",
                        "Carrier ID does not resolve for its declared kind."));
                }

                if (binding.SkillSequenceIds.Count == 0)
                {
                    issues.Add(Issue("CARRIER_SEQUENCE_IDS_EMPTY", path + ".skillSequenceIds", "Carrier binding requires at least one SkillSequence."));
                }

                HashSet<string> seenSequences = new HashSet<string>(StringComparer.Ordinal);
                for (int sequenceIndex = 0;
                    sequenceIndex < binding.SkillSequenceIds.Count;
                    sequenceIndex++)
                {
                    string sequenceId = binding.SkillSequenceIds[sequenceIndex];
                    string sequencePath = path + ".skillSequenceIds[" + sequenceIndex + "]";
                    ValidateId(sequenceId, sequencePath, issues);
                    if (!seenSequences.Add(sequenceId))
                    {
                        issues.Add(Issue("CARRIER_SEQUENCE_ID_DUPLICATE", sequencePath, "Carrier binding cannot repeat a SkillSequence ID."));
                    }

                    if (!sequences.TryGetValue(sequenceId, out SkillSequenceSnapshot sequence))
                    {
                        issues.Add(Issue("SKILL_SEQUENCE_REFERENCE_UNRESOLVED", sequencePath, "SkillSequence ID does not resolve with ordinal semantics."));
                        continue;
                    }

                    ValidateSequenceForCarrier(
                        sequence,
                        binding.CarrierKind,
                        binding.CarrierId,
                        carrierResolved,
                        patterns,
                        resolver,
                        sequencePath,
                        issues);
                }
            }
        }

        private static Dictionary<string, BossPhaseProfileSnapshot> ValidateBossPhases(
            IReadOnlyList<BossPhaseProfileSnapshot> values,
            IEnemySkillBossPhaseReferenceResolver resolver,
            IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patterns,
            IReadOnlyDictionary<string, SkillSequenceSnapshot> sequences,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            Dictionary<string, BossPhaseProfileSnapshot> byId =
                new Dictionary<string, BossPhaseProfileSnapshot>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                BossPhaseProfileSnapshot phase = values[index];
                string path = "bossPhaseProfiles[" + index + "]";
                if (phase == null)
                {
                    issues.Add(Issue("BOSS_PHASE_NULL", path, "BossPhaseProfile cannot be null."));
                    continue;
                }

                if (phase.BossPhaseReference == null)
                {
                    issues.Add(Issue("BOSS_PHASE_REFERENCE_NULL", path + ".bossPhaseReference", "E01 BossPhaseReference is required."));
                }
                else
                {
                    ValidateId(phase.BossPhaseId, path + ".bossPhaseReference.stableId", issues);
                    ValidateIsolation(phase.BossPhaseReference, path + ".bossPhaseReference", issues);
                }

                AddUnique(
                    byId,
                    phase.BossPhaseId,
                    phase,
                    path + ".bossPhaseReference.stableId",
                    "BOSS_PHASE_ID_DUPLICATE",
                    issues);

                if (phase.PlayerSafe == null)
                {
                    issues.Add(Issue("BOSS_PHASE_PLAYER_PROJECTION_NULL", path + ".playerSafe", "Player-safe phase projection is required."));
                }
                else
                {
                    if (!string.Equals(phase.PlayerSafe.BossPhaseId, phase.BossPhaseId, StringComparison.Ordinal))
                    {
                        issues.Add(Issue("BOSS_PHASE_PLAYER_ID_MISMATCH", path + ".playerSafe.bossPhaseId", "Player-safe BossPhaseId must equal its E01 reference exactly."));
                    }

                    ValidateId(phase.PlayerSafe.BossPhaseId, path + ".playerSafe.bossPhaseId", issues);
                    ValidateId(phase.PlayerSafe.PublicPhaseLabelKey, path + ".playerSafe.publicPhaseLabelKey", issues);
                    ValidateId(phase.PlayerSafe.PublicPhaseEntryCueKey, path + ".playerSafe.publicPhaseEntryCueKey", issues);
                }

                if (phase.InternalOnly == null)
                {
                    issues.Add(Issue("BOSS_PHASE_INTERNAL_SPEC_NULL", path + ".internalOnly", "Internal phase specification is required."));
                }
                else
                {
                    ValidateReferenceIds(
                        phase.InternalOnly.SkillSequenceIds,
                        sequences,
                        path + ".internalOnly.skillSequenceIds",
                        "BOSS_PHASE_SEQUENCE",
                        issues);
                    ValidateMechanicProfileIds(
                        phase.InternalOnly.MechanicProfileIds,
                        ValidationProfileKind.Boss,
                        path + ".internalOnly.mechanicProfileIds",
                        resolver,
                        issues);

                    for (int sequenceIndex = 0;
                        sequenceIndex < phase.InternalOnly.SkillSequenceIds.Count;
                        sequenceIndex++)
                    {
                        if (sequences.TryGetValue(
                            phase.InternalOnly.SkillSequenceIds[sequenceIndex],
                            out SkillSequenceSnapshot sequence))
                        {
                            ValidateSequenceMechanicKind(
                                sequence,
                                ValidationProfileKind.Boss,
                                patterns,
                                resolver,
                                path + ".internalOnly.skillSequenceIds[" + sequenceIndex + "]",
                                issues);
                        }
                    }
                }

                if (phase.DeveloperOnly == null)
                {
                    issues.Add(Issue("BOSS_PHASE_DIAGNOSTICS_NULL", path + ".developerOnly", "Developer phase diagnostics are required."));
                }
                else
                {
                    ValidateDeveloperDiagnostics(
                        phase.DeveloperOnly.DeveloperDiagnosticCategoryKeys,
                        phase.DeveloperOnly.SourceReferenceIds,
                        path + ".developerOnly",
                        resolver,
                        issues);
                }
            }

            return byId;
        }

        private static void ValidateBossPlans(
            IReadOnlyList<BossPhasePlanSnapshot> values,
            IEnemySkillBossPhaseReferenceResolver resolver,
            IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patterns,
            IReadOnlyDictionary<string, SkillSequenceSnapshot> sequences,
            IReadOnlyDictionary<string, BossPhaseProfileSnapshot> phases,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            Dictionary<string, int> firstByBoss = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                BossPhasePlanSnapshot plan = values[index];
                string path = "bossPhasePlans[" + index + "]";
                if (plan == null)
                {
                    issues.Add(Issue("BOSS_PHASE_PLAN_NULL", path, "BossPhasePlan cannot be null."));
                    continue;
                }

                ValidateId(plan.BossId, path + ".bossId", issues);
                ValidateIsolation(plan, path, issues);
                if (firstByBoss.TryGetValue(plan.BossId, out int firstIndex))
                {
                    issues.Add(Issue("BOSS_PHASE_PLAN_BOSS_ID_DUPLICATE", path + ".bossId", "Duplicate Boss plan; first seen at index " + firstIndex + "."));
                }
                else
                {
                    firstByBoss.Add(plan.BossId, index);
                }

                bool bossResolved = resolver != null && resolver.TryGetBoss(plan.BossId, out _);
                if (resolver != null && !bossResolved)
                {
                    issues.Add(Issue("BOSS_REFERENCE_UNRESOLVED", path + ".bossId", "Boss ID does not resolve with ordinal semantics."));
                }

                if (plan.Entries.Count == 0)
                {
                    issues.Add(Issue("BOSS_PHASE_PLAN_EMPTY", path + ".entries", "BossPhasePlan requires at least one entry."));
                    continue;
                }

                Dictionary<int, int> firstByOrder = new Dictionary<int, int>();
                for (int entryIndex = 0; entryIndex < plan.Entries.Count; entryIndex++)
                {
                    BossPhasePlanEntrySnapshot entry = plan.Entries[entryIndex];
                    string entryPath = path + ".entries[" + entryIndex + "]";
                    if (entry == null)
                    {
                        issues.Add(Issue("BOSS_PHASE_PLAN_ENTRY_NULL", entryPath, "Boss phase plan entry cannot be null."));
                        continue;
                    }

                    if (entry.PhaseOrder < 0)
                    {
                        issues.Add(Issue("PHASE_ORDER_NEGATIVE", entryPath + ".phaseOrder", "PhaseOrder must be non-negative."));
                    }

                    if (firstByOrder.TryGetValue(entry.PhaseOrder, out int firstOrderIndex))
                    {
                        issues.Add(Issue("PHASE_ORDER_DUPLICATE", entryPath + ".phaseOrder", "Duplicate PhaseOrder; first seen at index " + firstOrderIndex + "."));
                    }
                    else
                    {
                        firstByOrder.Add(entry.PhaseOrder, entryIndex);
                    }

                    ValidateId(entry.BossPhaseId, entryPath + ".bossPhaseId", issues);
                    if (!phases.TryGetValue(entry.BossPhaseId, out BossPhaseProfileSnapshot phase))
                    {
                        issues.Add(Issue("BOSS_PHASE_REFERENCE_UNRESOLVED", entryPath + ".bossPhaseId", "BossPhase ID does not resolve with ordinal semantics."));
                    }
                    else if (bossResolved)
                    {
                        ValidatePhaseForBoss(
                            phase,
                            plan.BossId,
                            patterns,
                            sequences,
                            resolver,
                            entryPath,
                            issues);
                    }

                    ValidateEntryCondition(entry.EntryCondition, entryPath + ".entryCondition", issues);
                    if (entry.EntryCondition != null)
                    {
                        if (entry.PhaseOrder == 0
                            && entry.EntryCondition.Kind != BossPhaseEntryConditionKind.EncounterStart)
                        {
                            issues.Add(Issue("FIRST_PHASE_CONDITION_INVALID", entryPath + ".entryCondition.kind", "The first phase must use EncounterStart."));
                        }

                        if (entry.PhaseOrder > 0
                            && entry.EntryCondition.Kind == BossPhaseEntryConditionKind.EncounterStart)
                        {
                            issues.Add(Issue("LATER_PHASE_ENCOUNTER_START_INVALID", entryPath + ".entryCondition.kind", "Only the first phase may use EncounterStart."));
                        }
                    }
                }

                ValidateContiguousOrder(
                    plan.Entries.Where(value => value != null).Select(value => value.PhaseOrder),
                    path + ".entries",
                    "PHASE_ORDER_NON_CONTIGUOUS",
                    issues);
            }
        }

        private static void ValidateEntryCondition(
            BossPhaseEntryConditionSnapshot condition,
            string path,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            if (condition == null)
            {
                issues.Add(Issue("ENTRY_CONDITION_NULL", path, "Boss phase entry condition is required."));
                return;
            }

            if (!Enum.IsDefined(typeof(BossPhaseEntryConditionKind), condition.Kind))
            {
                issues.Add(Issue("ENTRY_CONDITION_KIND_INVALID", path + ".kind", "Entry condition kind is unknown."));
                return;
            }

            bool thresholdValid = condition.HealthThresholdBasisPoints == 0;
            bool signalValid = string.IsNullOrEmpty(condition.MechanicSignalId);
            switch (condition.Kind)
            {
                case BossPhaseEntryConditionKind.EncounterStart:
                case BossPhaseEntryConditionKind.PreviousPhaseCompleted:
                    break;
                case BossPhaseEntryConditionKind.HealthRatioAtOrBelow:
                    thresholdValid = condition.HealthThresholdBasisPoints >= 1
                        && condition.HealthThresholdBasisPoints <= 9999;
                    break;
                case BossPhaseEntryConditionKind.MechanicSignal:
                    signalValid = !string.IsNullOrWhiteSpace(condition.MechanicSignalId)
                        && string.Equals(
                            condition.MechanicSignalId,
                            condition.MechanicSignalId.Trim(),
                            StringComparison.Ordinal);
                    break;
            }

            if (!thresholdValid || !signalValid)
            {
                issues.Add(Issue("ENTRY_CONDITION_FIELDS_INVALID", path, "Entry condition threshold and signal fields do not match the selected kind."));
            }

            if (condition.Kind == BossPhaseEntryConditionKind.MechanicSignal)
            {
                ValidateId(condition.MechanicSignalId, path + ".mechanicSignalId", issues);
            }
        }

        private static void ValidateSequenceForCarrier(
            SkillSequenceSnapshot sequence,
            EnemySkillCarrierKind carrierKind,
            string carrierId,
            bool carrierResolved,
            IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patterns,
            IEnemySkillBossPhaseReferenceResolver resolver,
            string path,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            ValidationProfileKind expectedKind = carrierKind == EnemySkillCarrierKind.Enemy
                ? ValidationProfileKind.Enemy
                : ValidationProfileKind.Boss;
            ValidateSequenceMechanicKind(sequence, expectedKind, patterns, resolver, path, issues);
            if (resolver == null || !carrierResolved)
            {
                return;
            }

            foreach (SkillSequenceStepSnapshot step in sequence.Steps.Where(value => value != null))
            {
                if (!patterns.TryGetValue(step.SkillPatternId, out EnemySkillPatternSnapshot pattern)
                    || pattern.InternalOnly == null)
                {
                    continue;
                }

                foreach (string profileId in pattern.InternalOnly.MechanicProfileIds)
                {
                    if (!resolver.HasCarrierMechanicBinding(carrierKind, carrierId, profileId))
                    {
                        issues.Add(Issue(
                            "CARRIER_MECHANIC_BINDING_MISSING",
                            path,
                            "E03 has no Carrier-to-MechanicProfile binding for "
                            + carrierId
                            + " -> "
                            + profileId
                            + "."));
                    }
                }
            }
        }

        private static void ValidateSequenceMechanicKind(
            SkillSequenceSnapshot sequence,
            ValidationProfileKind expectedKind,
            IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patterns,
            IEnemySkillBossPhaseReferenceResolver resolver,
            string path,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            if (resolver == null)
            {
                return;
            }

            foreach (SkillSequenceStepSnapshot step in sequence.Steps.Where(value => value != null))
            {
                if (!patterns.TryGetValue(step.SkillPatternId, out EnemySkillPatternSnapshot pattern)
                    || pattern.InternalOnly == null)
                {
                    continue;
                }

                foreach (string profileId in pattern.InternalOnly.MechanicProfileIds)
                {
                    if (resolver.TryGetMechanicProfileKind(profileId, out ValidationProfileKind actual)
                        && actual != expectedKind)
                    {
                        issues.Add(Issue("SKILL_MECHANIC_PROFILE_KIND_MISMATCH", path, "SkillPattern MechanicProfile kind is incompatible with its carrier or Boss phase."));
                    }
                }
            }
        }

        private static void ValidatePhaseForBoss(
            BossPhaseProfileSnapshot phase,
            string bossId,
            IReadOnlyDictionary<string, EnemySkillPatternSnapshot> patterns,
            IReadOnlyDictionary<string, SkillSequenceSnapshot> sequences,
            IEnemySkillBossPhaseReferenceResolver resolver,
            string path,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            if (phase.InternalOnly == null || resolver == null)
            {
                return;
            }

            foreach (string profileId in phase.InternalOnly.MechanicProfileIds)
            {
                if (!resolver.HasCarrierMechanicBinding(
                    EnemySkillCarrierKind.Boss,
                    bossId,
                    profileId))
                {
                    issues.Add(Issue("BOSS_PHASE_CARRIER_MECHANIC_BINDING_MISSING", path, "BossPhase MechanicProfile is not bound to this Boss in E03."));
                }
            }

            foreach (string sequenceId in phase.InternalOnly.SkillSequenceIds)
            {
                if (sequences.TryGetValue(sequenceId, out SkillSequenceSnapshot sequence))
                {
                    ValidateSequenceForCarrier(
                        sequence,
                        EnemySkillCarrierKind.Boss,
                        bossId,
                        true,
                        patterns,
                        resolver,
                        path,
                        issues);
                }
            }
        }

        private static void ValidateMechanicProfileIds(
            IReadOnlyList<string> values,
            ValidationProfileKind? expectedKind,
            string path,
            IEnemySkillBossPhaseReferenceResolver resolver,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                string id = values[index];
                string idPath = path + "[" + index + "]";
                ValidateId(id, idPath, issues);
                if (!seen.Add(id))
                {
                    issues.Add(Issue("MECHANIC_PROFILE_ID_DUPLICATE", idPath, "MechanicProfile ID cannot repeat."));
                }

                if (resolver == null)
                {
                    continue;
                }

                if (!resolver.TryGetMechanicProfileKind(id, out ValidationProfileKind actualKind))
                {
                    issues.Add(Issue("MECHANIC_PROFILE_REFERENCE_UNRESOLVED", idPath, "MechanicProfile ID does not resolve with ordinal semantics."));
                }
                else if (expectedKind.HasValue && actualKind != expectedKind.Value)
                {
                    issues.Add(Issue("MECHANIC_PROFILE_KIND_MISMATCH", idPath, "MechanicProfile kind does not match the required Enemy/Boss kind."));
                }
            }
        }

        private static void ValidateDeveloperDiagnostics(
            IReadOnlyList<string> diagnosticKeys,
            IReadOnlyList<string> sourceReferenceIds,
            string path,
            IEnemySkillBossPhaseReferenceResolver resolver,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            ValidateVocabularyKeys(
                diagnosticKeys,
                EnemyVocabularyCategory.DeveloperDiagnosticCategory,
                path + ".developerDiagnosticCategoryKeys",
                "DEVELOPER_DIAGNOSTIC_CATEGORY",
                resolver,
                issues);

            HashSet<string> seenSources = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < sourceReferenceIds.Count; index++)
            {
                string id = sourceReferenceIds[index];
                string idPath = path + ".sourceReferenceIds[" + index + "]";
                ValidateId(id, idPath, issues);
                if (!seenSources.Add(id))
                {
                    issues.Add(Issue("SOURCE_REFERENCE_ID_DUPLICATE", idPath, "SourceReferenceId cannot repeat."));
                }

                if (id.IndexOf("/", StringComparison.Ordinal) >= 0
                    || id.IndexOf("\\", StringComparison.Ordinal) >= 0
                    || id.IndexOf(":", StringComparison.Ordinal) >= 0)
                {
                    issues.Add(Issue("SOURCE_REFERENCE_PATH_FORBIDDEN", idPath, "SourceReferenceIds must be stable IDs, not disk, Asset, or Scene paths."));
                }
            }
        }

        private static void ValidateVocabularyKeys(
            IReadOnlyList<string> values,
            EnemyVocabularyCategory category,
            string path,
            string codePrefix,
            IEnemySkillBossPhaseReferenceResolver resolver,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                string key = values[index];
                string keyPath = path + "[" + index + "]";
                ValidateId(key, keyPath, issues);
                if (!seen.Add(key))
                {
                    issues.Add(Issue(codePrefix + "_DUPLICATE", keyPath, "Vocabulary key cannot repeat."));
                }

                if (resolver != null && !resolver.HasVocabularyKey(category, key))
                {
                    issues.Add(Issue(codePrefix + "_UNRESOLVED", keyPath, "Vocabulary key is unknown or belongs to the wrong E02 category."));
                }
            }
        }

        private static void ValidateReferenceIds<T>(
            IReadOnlyList<string> values,
            IReadOnlyDictionary<string, T> known,
            string path,
            string codePrefix,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                string id = values[index];
                string idPath = path + "[" + index + "]";
                ValidateId(id, idPath, issues);
                if (!seen.Add(id))
                {
                    issues.Add(Issue(codePrefix + "_ID_DUPLICATE", idPath, "Reference ID cannot repeat."));
                }

                if (!known.ContainsKey(id))
                {
                    issues.Add(Issue(codePrefix + "_REFERENCE_UNRESOLVED", idPath, "Reference ID does not resolve with ordinal semantics."));
                }
            }
        }

        private static bool ResolveCarrier(
            EnemySkillCarrierKind kind,
            string carrierId,
            IEnemySkillBossPhaseReferenceResolver resolver)
        {
            if (resolver == null || !Enum.IsDefined(typeof(EnemySkillCarrierKind), kind))
            {
                return false;
            }

            return kind == EnemySkillCarrierKind.Enemy
                ? resolver.TryGetEnemy(carrierId, out _)
                : resolver.TryGetBoss(carrierId, out _);
        }

        private static void ValidateIsolation(
            TalismanBag.EnemySystem.Domain.IEnemyDomainIsolationMetadata value,
            string path,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            if (!value.DevOnly || value.IsEnabled || value.EntersFormalFlow)
            {
                issues.Add(Issue("DEV_ISOLATION_INVALID", path, "Required isolation is devOnly=true, isEnabled=false, entersFormalFlow=false."));
            }
        }

        private static void ValidateContiguousOrder(
            IEnumerable<int> values,
            string path,
            string code,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            int[] ordered = (values ?? Array.Empty<int>())
                .Distinct()
                .OrderBy(value => value)
                .ToArray();
            bool contiguous = ordered.Length == 0
                || ordered.Select((value, index) => value == index).All(value => value);
            if (!contiguous)
            {
                issues.Add(Issue(code, path, "Explicit order must be exactly 0..N-1."));
            }
        }

        private static void AddUnique<T>(
            IDictionary<string, T> values,
            string id,
            T value,
            string path,
            string code,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            if (values.ContainsKey(id))
            {
                issues.Add(Issue(code, path, "Duplicate exact stable ID."));
            }
            else
            {
                values.Add(id, value);
            }
        }

        private static void ValidateId(
            string id,
            string path,
            ICollection<EnemySkillBossPhaseValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                issues.Add(Issue("ID_EMPTY", path, "Stable IDs and keys must not be empty or whitespace."));
                return;
            }

            if (!string.Equals(id, id.Trim(), StringComparison.Ordinal))
            {
                issues.Add(Issue("ID_OUTER_WHITESPACE", path, "Stable IDs and keys are never trimmed implicitly."));
            }
        }

        private static EnemySkillBossPhaseValidationIssue Issue(
            string code,
            string path,
            string message)
        {
            return new EnemySkillBossPhaseValidationIssue(code, path, message);
        }
    }
}
