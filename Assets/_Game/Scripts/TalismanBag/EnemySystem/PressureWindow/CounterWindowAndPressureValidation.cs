using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.PressureWindow
{
    public sealed class CounterWindowAndPressureValidationIssue
    {
        public CounterWindowAndPressureValidationIssue(
            string code,
            string path,
            string message)
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

    public sealed class CounterWindowAndPressureValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<CounterWindowAndPressureValidationIssue> issues;

        public CounterWindowAndPressureValidationException(
            IReadOnlyList<CounterWindowAndPressureValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly(
                (issues ?? Array.Empty<CounterWindowAndPressureValidationIssue>()).ToArray());
        }

        public IReadOnlyList<CounterWindowAndPressureValidationIssue> Issues => issues;

        private static string BuildMessage(
            IReadOnlyList<CounterWindowAndPressureValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(
                "Counter-window and pressure schema validation failed.");
            foreach (CounterWindowAndPressureValidationIssue issue
                in issues ?? Array.Empty<CounterWindowAndPressureValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class DefaultCounterWindowAndPressureProvider :
        ICounterWindowAndPressureProvider
    {
        public static readonly DefaultCounterWindowAndPressureProvider Instance =
            new DefaultCounterWindowAndPressureProvider();

        private readonly ICounterWindowAndPressureValidator validator;

        public DefaultCounterWindowAndPressureProvider(
            ICounterWindowAndPressureValidator validator = null)
        {
            this.validator = validator ?? DefaultCounterWindowAndPressureValidator.Instance;
        }

        public CounterWindowAndPressureCatalogSnapshot CreateSnapshot(
            CounterWindowAndPressureCatalogInput input,
            ICounterWindowAndPressureReferenceResolver resolver)
        {
            IReadOnlyList<CounterWindowAndPressureValidationIssue> issues =
                validator.Validate(input, resolver);
            if (issues.Count > 0)
            {
                throw new CounterWindowAndPressureValidationException(issues);
            }

            return new CounterWindowAndPressureCatalogSnapshot(input);
        }
    }

    public sealed class DefaultCounterWindowAndPressureValidator :
        ICounterWindowAndPressureValidator
    {
        public static readonly DefaultCounterWindowAndPressureValidator Instance =
            new DefaultCounterWindowAndPressureValidator();

        private static readonly string[] PlayerSafeForbiddenTokens =
        {
            "pressureChannelKey",
            "weightBasisPoints",
            "counterWindowTypeKey",
            "openCondition",
            "closeCondition",
            "maximumDurationMilliseconds",
            "sourceBinding",
            "pressureCounterWindowBinding",
            "buildCapabilityKey",
            "requirementGroupId",
            "minimumCapabilityBasisPoints",
            "developerDiagnosticCategoryKeys",
            "sourceReferenceIds"
        };

        public IReadOnlyList<CounterWindowAndPressureValidationIssue> Validate(
            CounterWindowAndPressureCatalogInput input,
            ICounterWindowAndPressureReferenceResolver resolver)
        {
            List<CounterWindowAndPressureValidationIssue> issues =
                new List<CounterWindowAndPressureValidationIssue>();
            if (input == null)
            {
                issues.Add(Issue("INPUT_NULL", "$", "Catalog input is required."));
                return Array.AsReadOnly(issues.ToArray());
            }

            if (!string.Equals(
                input.SchemaId,
                CounterWindowAndPressureSchema.SchemaId,
                StringComparison.Ordinal))
            {
                issues.Add(Issue(
                    "SCHEMA_ID_MISMATCH",
                    "schemaId",
                    "Schema ID must match CounterWindowAndPressure.v1 exactly."));
            }

            if (input.SchemaVersion != CounterWindowAndPressureSchema.SchemaVersion)
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
                    "A read-only E02/E03/E05 reference resolver is required."));
            }

            Dictionary<string, BuildPressureProfileSnapshot> pressures =
                ValidatePressures(input.BuildPressureProfiles, resolver, issues);
            Dictionary<string, CounterWindowProfileSnapshot> windows =
                ValidateWindows(input.CounterWindowProfiles, resolver, issues);
            ValidatePressureSourceBindings(
                input.PressureSourceBindings,
                pressures,
                resolver,
                issues);
            ValidateCounterWindowSourceBindings(
                input.CounterWindowSourceBindings,
                windows,
                resolver,
                issues);
            ValidatePressureCounterWindowBindings(
                input.PressureCounterWindowBindings,
                pressures,
                windows,
                issues);

            if (issues.Count == 0)
            {
                string payload = new CounterWindowAndPressureCatalogSnapshot(input)
                    .BuildPlayerSafeCanonicalPayload();
                foreach (string token in PlayerSafeForbiddenTokens)
                {
                    if (payload.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        issues.Add(Issue(
                            "PLAYER_SAFE_PAYLOAD_LEAK",
                            "playerSafeCanonicalPayload",
                            "Player-safe payload contains forbidden internal field token: "
                            + token
                            + "."));
                    }
                }
            }

            return Array.AsReadOnly(issues.ToArray());
        }

        private static Dictionary<string, BuildPressureProfileSnapshot> ValidatePressures(
            IReadOnlyList<BuildPressureProfileSnapshot> values,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            Dictionary<string, BuildPressureProfileSnapshot> byId =
                new Dictionary<string, BuildPressureProfileSnapshot>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                BuildPressureProfileSnapshot pressure = values[index];
                string path = "buildPressureProfiles[" + index + "]";
                if (pressure == null)
                {
                    issues.Add(Issue(
                        "BUILD_PRESSURE_PROFILE_NULL",
                        path,
                        "BuildPressureProfile cannot be null."));
                    continue;
                }

                ValidateId(
                    pressure.BuildPressureProfileId,
                    path + ".buildPressureProfileId",
                    issues);
                ValidateIsolation(pressure, path, issues);
                AddUnique(
                    byId,
                    pressure.BuildPressureProfileId,
                    pressure,
                    path + ".buildPressureProfileId",
                    "BUILD_PRESSURE_PROFILE_ID_DUPLICATE",
                    issues);

                ValidatePressurePlayer(pressure, path, resolver, issues);
                ValidatePressureInternal(pressure.InternalOnly, path + ".internalOnly", resolver, issues);
                ValidatePressureDeveloper(pressure.DeveloperOnly, path + ".developerOnly", resolver, issues);
            }

            return byId;
        }

        private static void ValidatePressurePlayer(
            BuildPressureProfileSnapshot owner,
            string path,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            BuildPressurePlayerProjection player = owner.PlayerSafe;
            if (player == null)
            {
                issues.Add(Issue(
                    "BUILD_PRESSURE_PLAYER_PROJECTION_NULL",
                    path + ".playerSafe",
                    "Player-safe pressure projection is required."));
                return;
            }

            if (!string.Equals(
                player.BuildPressureProfileId,
                owner.BuildPressureProfileId,
                StringComparison.Ordinal))
            {
                issues.Add(Issue(
                    "BUILD_PRESSURE_PLAYER_ID_MISMATCH",
                    path + ".playerSafe.buildPressureProfileId",
                    "Player-safe owner ID must equal BuildPressureProfileId exactly."));
            }

            ValidateId(
                player.BuildPressureProfileId,
                path + ".playerSafe.buildPressureProfileId",
                issues);
            ValidateId(
                player.PublicPressureLabelKey,
                path + ".playerSafe.publicPressureLabelKey",
                issues);
            ValidateId(
                player.PublicPressureHintKey,
                path + ".playerSafe.publicPressureHintKey",
                issues);
            ValidateVocabularyKeys(
                player.PlayerHintCategoryKeys,
                EnemyVocabularyCategory.PlayerHintCategory,
                path + ".playerSafe.playerHintCategoryKeys",
                "PLAYER_HINT_CATEGORY",
                resolver,
                issues);
        }

        private static void ValidatePressureInternal(
            BuildPressureInternalSpec value,
            string path,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            if (value == null)
            {
                issues.Add(Issue(
                    "BUILD_PRESSURE_INTERNAL_SPEC_NULL",
                    path,
                    "Internal pressure specification is required."));
                return;
            }

            if (value.PressureChannelContributions.Count == 0)
            {
                issues.Add(Issue(
                    "PRESSURE_CHANNEL_CONTRIBUTIONS_EMPTY",
                    path + ".pressureChannelContributions",
                    "At least one pressure-channel contribution is required."));
                return;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            int total = 0;
            for (int index = 0;
                index < value.PressureChannelContributions.Count;
                index++)
            {
                PressureChannelContributionSnapshot contribution =
                    value.PressureChannelContributions[index];
                string itemPath = path + ".pressureChannelContributions[" + index + "]";
                if (contribution == null)
                {
                    issues.Add(Issue(
                        "PRESSURE_CHANNEL_CONTRIBUTION_NULL",
                        itemPath,
                        "PressureChannelContribution cannot be null."));
                    continue;
                }

                ValidateId(
                    contribution.PressureChannelKey,
                    itemPath + ".pressureChannelKey",
                    issues);
                if (!seen.Add(contribution.PressureChannelKey))
                {
                    issues.Add(Issue(
                        "PRESSURE_CHANNEL_KEY_DUPLICATE",
                        itemPath + ".pressureChannelKey",
                        "PressureChannelKey cannot repeat within a profile."));
                }

                if (resolver != null && !resolver.HasVocabularyKey(
                    EnemyVocabularyCategory.PressureChannel,
                    contribution.PressureChannelKey))
                {
                    issues.Add(Issue(
                        "PRESSURE_CHANNEL_KEY_UNRESOLVED",
                        itemPath + ".pressureChannelKey",
                        "PressureChannelKey is unknown or belongs to the wrong E02 category."));
                }

                if (contribution.WeightBasisPoints < 1
                    || contribution.WeightBasisPoints > 10000)
                {
                    issues.Add(Issue(
                        "PRESSURE_WEIGHT_OUT_OF_RANGE",
                        itemPath + ".weightBasisPoints",
                        "WeightBasisPoints must be in 1..10000."));
                }

                total += contribution.WeightBasisPoints;
            }

            if (total != 10000)
            {
                issues.Add(Issue(
                    "PRESSURE_WEIGHT_TOTAL_INVALID",
                    path + ".pressureChannelContributions",
                    "Pressure-channel weights must total exactly 10000."));
            }
        }

        private static void ValidatePressureDeveloper(
            BuildPressureDeveloperDiagnostics value,
            string path,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            if (value == null)
            {
                issues.Add(Issue(
                    "BUILD_PRESSURE_DIAGNOSTICS_NULL",
                    path,
                    "Developer pressure diagnostics are required."));
                return;
            }

            if (value.RequirementGroups.Count == 0)
            {
                issues.Add(Issue(
                    "REQUIREMENT_GROUPS_EMPTY",
                    path + ".requirementGroups",
                    "At least one capability requirement group is required."));
            }

            HashSet<string> groupIds = new HashSet<string>(StringComparer.Ordinal);
            for (int groupIndex = 0; groupIndex < value.RequirementGroups.Count; groupIndex++)
            {
                BuildCapabilityRequirementGroupSnapshot group =
                    value.RequirementGroups[groupIndex];
                string groupPath = path + ".requirementGroups[" + groupIndex + "]";
                if (group == null)
                {
                    issues.Add(Issue(
                        "REQUIREMENT_GROUP_NULL",
                        groupPath,
                        "RequirementGroup cannot be null."));
                    continue;
                }

                ValidateId(group.RequirementGroupId, groupPath + ".requirementGroupId", issues);
                if (!groupIds.Add(group.RequirementGroupId))
                {
                    issues.Add(Issue(
                        "REQUIREMENT_GROUP_ID_DUPLICATE",
                        groupPath + ".requirementGroupId",
                        "RequirementGroupId cannot repeat within a profile."));
                }

                if (!Enum.IsDefined(
                    typeof(CapabilityRequirementRole),
                    group.RequirementRole))
                {
                    issues.Add(Issue(
                        "REQUIREMENT_ROLE_INVALID",
                        groupPath + ".requirementRole",
                        "RequirementRole must be Required or Recommended."));
                }

                if (!Enum.IsDefined(
                    typeof(RequirementMatchMode),
                    group.RequirementMatchMode))
                {
                    issues.Add(Issue(
                        "REQUIREMENT_MATCH_MODE_INVALID",
                        groupPath + ".requirementMatchMode",
                        "RequirementMatchMode must be Any or All."));
                }

                if (group.Requirements.Count == 0)
                {
                    issues.Add(Issue(
                        "REQUIREMENT_GROUP_EMPTY",
                        groupPath + ".requirements",
                        "RequirementGroup must contain at least one requirement."));
                }

                HashSet<string> capabilityKeys = new HashSet<string>(StringComparer.Ordinal);
                for (int requirementIndex = 0;
                    requirementIndex < group.Requirements.Count;
                    requirementIndex++)
                {
                    BuildCapabilityRequirementSnapshot requirement =
                        group.Requirements[requirementIndex];
                    string requirementPath = groupPath
                        + ".requirements["
                        + requirementIndex
                        + "]";
                    if (requirement == null)
                    {
                        issues.Add(Issue(
                            "BUILD_CAPABILITY_REQUIREMENT_NULL",
                            requirementPath,
                            "BuildCapabilityRequirement cannot be null."));
                        continue;
                    }

                    ValidateId(
                        requirement.BuildCapabilityKey,
                        requirementPath + ".buildCapabilityKey",
                        issues);
                    if (!capabilityKeys.Add(requirement.BuildCapabilityKey))
                    {
                        issues.Add(Issue(
                            "BUILD_CAPABILITY_KEY_DUPLICATE",
                            requirementPath + ".buildCapabilityKey",
                            "BuildCapabilityKey cannot repeat within a group."));
                    }

                    if (resolver != null && !resolver.HasVocabularyKey(
                        EnemyVocabularyCategory.BuildCapability,
                        requirement.BuildCapabilityKey))
                    {
                        issues.Add(Issue(
                            "BUILD_CAPABILITY_KEY_UNRESOLVED",
                            requirementPath + ".buildCapabilityKey",
                            "BuildCapabilityKey is unknown or belongs to the wrong E02 category."));
                    }

                    if (requirement.MinimumCapabilityBasisPoints < 1
                        || requirement.MinimumCapabilityBasisPoints > 10000)
                    {
                        issues.Add(Issue(
                            "MINIMUM_CAPABILITY_OUT_OF_RANGE",
                            requirementPath + ".minimumCapabilityBasisPoints",
                            "MinimumCapabilityBasisPoints must be in 1..10000."));
                    }
                }
            }

            ValidateDeveloperDiagnostics(
                value.DeveloperDiagnosticCategoryKeys,
                value.SourceReferenceIds,
                path,
                resolver,
                issues);
        }

        private static Dictionary<string, CounterWindowProfileSnapshot> ValidateWindows(
            IReadOnlyList<CounterWindowProfileSnapshot> values,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            Dictionary<string, CounterWindowProfileSnapshot> byId =
                new Dictionary<string, CounterWindowProfileSnapshot>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                CounterWindowProfileSnapshot window = values[index];
                string path = "counterWindowProfiles[" + index + "]";
                if (window == null)
                {
                    issues.Add(Issue(
                        "COUNTER_WINDOW_PROFILE_NULL",
                        path,
                        "CounterWindowProfile cannot be null."));
                    continue;
                }

                if (window.CounterWindowReference == null)
                {
                    issues.Add(Issue(
                        "COUNTER_WINDOW_REFERENCE_NULL",
                        path + ".counterWindowReference",
                        "E01 CounterWindowReference is required."));
                }
                else
                {
                    ValidateId(
                        window.CounterWindowId,
                        path + ".counterWindowReference.stableId",
                        issues);
                    ValidateIsolation(
                        window.CounterWindowReference,
                        path + ".counterWindowReference",
                        issues);
                }

                AddUnique(
                    byId,
                    window.CounterWindowId,
                    window,
                    path + ".counterWindowReference.stableId",
                    "COUNTER_WINDOW_ID_DUPLICATE",
                    issues);
                ValidateWindowPlayer(window, path, issues);
                ValidateWindowInternal(window.InternalOnly, path + ".internalOnly", resolver, issues);

                if (window.DeveloperOnly == null)
                {
                    issues.Add(Issue(
                        "COUNTER_WINDOW_DIAGNOSTICS_NULL",
                        path + ".developerOnly",
                        "Developer window diagnostics are required."));
                }
                else
                {
                    ValidateDeveloperDiagnostics(
                        window.DeveloperOnly.DeveloperDiagnosticCategoryKeys,
                        window.DeveloperOnly.SourceReferenceIds,
                        path + ".developerOnly",
                        resolver,
                        issues);
                }
            }

            return byId;
        }

        private static void ValidateWindowPlayer(
            CounterWindowProfileSnapshot owner,
            string path,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            CounterWindowPlayerProjection player = owner.PlayerSafe;
            if (player == null)
            {
                issues.Add(Issue(
                    "COUNTER_WINDOW_PLAYER_PROJECTION_NULL",
                    path + ".playerSafe",
                    "Player-safe window projection is required."));
                return;
            }

            if (!string.Equals(
                player.CounterWindowId,
                owner.CounterWindowId,
                StringComparison.Ordinal))
            {
                issues.Add(Issue(
                    "COUNTER_WINDOW_PLAYER_ID_MISMATCH",
                    path + ".playerSafe.counterWindowId",
                    "Player-safe owner ID must equal CounterWindowId exactly."));
            }

            ValidateId(player.CounterWindowId, path + ".playerSafe.counterWindowId", issues);
            ValidateId(
                player.PublicWindowLabelKey,
                path + ".playerSafe.publicWindowLabelKey",
                issues);
            ValidateId(
                player.PublicWindowOpenCueKey,
                path + ".playerSafe.publicWindowOpenCueKey",
                issues);
            ValidateId(
                player.PublicWindowCloseCueKey,
                path + ".playerSafe.publicWindowCloseCueKey",
                issues);
        }

        private static void ValidateWindowInternal(
            CounterWindowInternalSpec value,
            string path,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            if (value == null)
            {
                issues.Add(Issue(
                    "COUNTER_WINDOW_INTERNAL_SPEC_NULL",
                    path,
                    "Internal counter-window specification is required."));
                return;
            }

            ValidateId(value.CounterWindowTypeKey, path + ".counterWindowTypeKey", issues);
            if (resolver != null && !resolver.HasVocabularyKey(
                EnemyVocabularyCategory.CounterWindowType,
                value.CounterWindowTypeKey))
            {
                issues.Add(Issue(
                    "COUNTER_WINDOW_TYPE_KEY_UNRESOLVED",
                    path + ".counterWindowTypeKey",
                    "CounterWindowTypeKey is unknown or belongs to the wrong E02 category."));
            }

            ValidateMatchMode(
                value.OpenConditionMatchMode,
                path + ".openConditionMatchMode",
                issues);
            ValidateMatchMode(
                value.CloseConditionMatchMode,
                path + ".closeConditionMatchMode",
                issues);

            if (value.OpenConditions.Count == 0)
            {
                issues.Add(Issue(
                    "OPEN_CONDITIONS_EMPTY",
                    path + ".openConditions",
                    "At least one open condition is required."));
            }

            HashSet<string> conditionIds = new HashSet<string>(StringComparer.Ordinal);
            ValidateConditions(
                value.OpenConditions,
                path + ".openConditions",
                resolver,
                conditionIds,
                issues);
            ValidateConditions(
                value.CloseConditions,
                path + ".closeConditions",
                resolver,
                conditionIds,
                issues);

            if (value.MaximumDurationMilliseconds < 0)
            {
                issues.Add(Issue(
                    "MAXIMUM_DURATION_NEGATIVE",
                    path + ".maximumDurationMilliseconds",
                    "MaximumDurationMilliseconds cannot be negative."));
            }

            if (value.MaximumDurationMilliseconds == 0
                && value.CloseConditions.Count == 0)
            {
                issues.Add(Issue(
                    "WINDOW_CLOSE_RULE_MISSING",
                    path,
                    "A zero-duration window requires at least one close condition."));
            }
        }

        private static void ValidateConditions(
            IReadOnlyList<CounterWindowConditionSnapshot> values,
            string path,
            ICounterWindowAndPressureReferenceResolver resolver,
            ISet<string> conditionIds,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            for (int index = 0; index < values.Count; index++)
            {
                CounterWindowConditionSnapshot condition = values[index];
                string itemPath = path + "[" + index + "]";
                if (condition == null)
                {
                    issues.Add(Issue(
                        "COUNTER_WINDOW_CONDITION_NULL",
                        itemPath,
                        "CounterWindowCondition cannot be null."));
                    continue;
                }

                ValidateId(condition.ConditionId, itemPath + ".conditionId", issues);
                if (!conditionIds.Add(condition.ConditionId))
                {
                    issues.Add(Issue(
                        "CONDITION_ID_DUPLICATE",
                        itemPath + ".conditionId",
                        "ConditionId cannot repeat within a window."));
                }

                if (!Enum.IsDefined(typeof(CounterWindowConditionKind), condition.Kind))
                {
                    issues.Add(Issue(
                        "CONDITION_KIND_INVALID",
                        itemPath + ".kind",
                        "CounterWindowConditionKind is unknown."));
                    continue;
                }

                ValidateId(condition.ReferenceId, itemPath + ".referenceId", issues);
                if (resolver == null)
                {
                    continue;
                }

                bool resolved;
                switch (condition.Kind)
                {
                    case CounterWindowConditionKind.MechanicSignal:
                        resolved = !string.IsNullOrWhiteSpace(condition.ReferenceId);
                        break;
                    case CounterWindowConditionKind.SkillPatternStarted:
                    case CounterWindowConditionKind.SkillPatternCompleted:
                    case CounterWindowConditionKind.SkillPatternInterrupted:
                        resolved = resolver.HasSkillPattern(condition.ReferenceId);
                        break;
                    case CounterWindowConditionKind.BossPhaseEntered:
                    case CounterWindowConditionKind.BossPhaseExited:
                        resolved = resolver.HasBossPhase(condition.ReferenceId);
                        break;
                    default:
                        resolved = false;
                        break;
                }

                if (!resolved)
                {
                    issues.Add(Issue(
                        "CONDITION_REFERENCE_UNRESOLVED",
                        itemPath + ".referenceId",
                        "Condition ReferenceId is empty, incompatible, or unresolved for its kind."));
                }
            }
        }

        private static void ValidatePressureSourceBindings(
            IReadOnlyList<PressureSourceBindingSnapshot> values,
            IReadOnlyDictionary<string, BuildPressureProfileSnapshot> pressures,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                PressureSourceBindingSnapshot binding = values[index];
                string path = "pressureSourceBindings[" + index + "]";
                if (binding == null)
                {
                    issues.Add(Issue(
                        "PRESSURE_SOURCE_BINDING_NULL",
                        path,
                        "PressureSourceBinding cannot be null."));
                    continue;
                }

                ValidateSourceBinding(
                    binding.SourceKind,
                    binding.SourceId,
                    binding.BuildPressureProfileId,
                    path,
                    resolver,
                    identities,
                    "PRESSURE_SOURCE_BINDING_DUPLICATE",
                    issues);
                ValidateIsolation(binding, path, issues);
                if (!pressures.ContainsKey(binding.BuildPressureProfileId))
                {
                    issues.Add(Issue(
                        "BUILD_PRESSURE_REFERENCE_UNRESOLVED",
                        path + ".buildPressureProfileId",
                        "BuildPressureProfile ID does not resolve with ordinal semantics."));
                }
            }
        }

        private static void ValidateCounterWindowSourceBindings(
            IReadOnlyList<CounterWindowSourceBindingSnapshot> values,
            IReadOnlyDictionary<string, CounterWindowProfileSnapshot> windows,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                CounterWindowSourceBindingSnapshot binding = values[index];
                string path = "counterWindowSourceBindings[" + index + "]";
                if (binding == null)
                {
                    issues.Add(Issue(
                        "COUNTER_WINDOW_SOURCE_BINDING_NULL",
                        path,
                        "CounterWindowSourceBinding cannot be null."));
                    continue;
                }

                ValidateSourceBinding(
                    binding.SourceKind,
                    binding.SourceId,
                    binding.CounterWindowId,
                    path,
                    resolver,
                    identities,
                    "COUNTER_WINDOW_SOURCE_BINDING_DUPLICATE",
                    issues);
                ValidateIsolation(binding, path, issues);
                if (!windows.ContainsKey(binding.CounterWindowId))
                {
                    issues.Add(Issue(
                        "COUNTER_WINDOW_REFERENCE_UNRESOLVED",
                        path + ".counterWindowId",
                        "CounterWindow ID does not resolve with ordinal semantics."));
                }
            }
        }

        private static void ValidateSourceBinding(
            PressureSourceKind sourceKind,
            string sourceId,
            string targetId,
            string path,
            ICounterWindowAndPressureReferenceResolver resolver,
            ISet<string> identities,
            string duplicateCode,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            bool kindDefined = Enum.IsDefined(typeof(PressureSourceKind), sourceKind);
            if (!kindDefined)
            {
                issues.Add(Issue(
                    "PRESSURE_SOURCE_KIND_INVALID",
                    path + ".sourceKind",
                    "SourceKind must be MechanicProfile, MapRule, SkillPattern, or BossPhase."));
            }

            ValidateId(sourceId, path + ".sourceId", issues);
            ValidateId(targetId, path + ".targetId", issues);
            string identity = CounterWindowAndPressureCatalogSnapshot.SourceBindingIdentity(
                sourceKind,
                sourceId,
                targetId);
            if (!identities.Add(identity))
            {
                issues.Add(Issue(
                    duplicateCode,
                    path,
                    "Duplicate exact SourceKind/SourceId/target binding."));
            }

            if (resolver != null && kindDefined && !ResolveSource(sourceKind, sourceId, resolver))
            {
                issues.Add(Issue(
                    "PRESSURE_SOURCE_REFERENCE_UNRESOLVED",
                    path + ".sourceId",
                    "Source ID does not resolve for its declared kind."));
            }
        }

        private static void ValidatePressureCounterWindowBindings(
            IReadOnlyList<PressureCounterWindowBindingSnapshot> values,
            IReadOnlyDictionary<string, BuildPressureProfileSnapshot> pressures,
            IReadOnlyDictionary<string, CounterWindowProfileSnapshot> windows,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                PressureCounterWindowBindingSnapshot binding = values[index];
                string path = "pressureCounterWindowBindings[" + index + "]";
                if (binding == null)
                {
                    issues.Add(Issue(
                        "PRESSURE_COUNTER_WINDOW_BINDING_NULL",
                        path,
                        "PressureCounterWindowBinding cannot be null."));
                    continue;
                }

                ValidateId(
                    binding.BuildPressureProfileId,
                    path + ".buildPressureProfileId",
                    issues);
                ValidateId(binding.CounterWindowId, path + ".counterWindowId", issues);
                ValidateIsolation(binding, path, issues);
                string identity = CounterWindowAndPressureCatalogSnapshot.PressureWindowIdentity(
                    binding.BuildPressureProfileId,
                    binding.CounterWindowId);
                if (!identities.Add(identity))
                {
                    issues.Add(Issue(
                        "PRESSURE_COUNTER_WINDOW_BINDING_DUPLICATE",
                        path,
                        "Duplicate exact BuildPressureProfile/CounterWindow binding."));
                }

                if (!pressures.ContainsKey(binding.BuildPressureProfileId))
                {
                    issues.Add(Issue(
                        "BUILD_PRESSURE_REFERENCE_UNRESOLVED",
                        path + ".buildPressureProfileId",
                        "BuildPressureProfile ID does not resolve with ordinal semantics."));
                }

                if (!windows.ContainsKey(binding.CounterWindowId))
                {
                    issues.Add(Issue(
                        "COUNTER_WINDOW_REFERENCE_UNRESOLVED",
                        path + ".counterWindowId",
                        "CounterWindow ID does not resolve with ordinal semantics."));
                }
            }
        }

        private static bool ResolveSource(
            PressureSourceKind sourceKind,
            string sourceId,
            ICounterWindowAndPressureReferenceResolver resolver)
        {
            switch (sourceKind)
            {
                case PressureSourceKind.MechanicProfile:
                    return resolver.HasMechanicProfile(sourceId);
                case PressureSourceKind.MapRule:
                    return resolver.HasMapRule(sourceId);
                case PressureSourceKind.SkillPattern:
                    return resolver.HasSkillPattern(sourceId);
                case PressureSourceKind.BossPhase:
                    return resolver.HasBossPhase(sourceId);
                default:
                    return false;
            }
        }

        private static void ValidateDeveloperDiagnostics(
            IReadOnlyList<string> diagnosticKeys,
            IReadOnlyList<string> sourceReferenceIds,
            string path,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            ValidateVocabularyKeys(
                diagnosticKeys,
                EnemyVocabularyCategory.DeveloperDiagnosticCategory,
                path + ".developerDiagnosticCategoryKeys",
                "DEVELOPER_DIAGNOSTIC_CATEGORY",
                resolver,
                issues);

            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < sourceReferenceIds.Count; index++)
            {
                string id = sourceReferenceIds[index];
                string idPath = path + ".sourceReferenceIds[" + index + "]";
                ValidateId(id, idPath, issues);
                if (!seen.Add(id))
                {
                    issues.Add(Issue(
                        "SOURCE_REFERENCE_ID_DUPLICATE",
                        idPath,
                        "SourceReferenceId cannot repeat."));
                }

                if (id.IndexOf("/", StringComparison.Ordinal) >= 0
                    || id.IndexOf("\\", StringComparison.Ordinal) >= 0
                    || id.IndexOf(":", StringComparison.Ordinal) >= 0)
                {
                    issues.Add(Issue(
                        "SOURCE_REFERENCE_PATH_FORBIDDEN",
                        idPath,
                        "SourceReferenceIds must be stable IDs, not disk, Asset, or Scene paths."));
                }
            }
        }

        private static void ValidateVocabularyKeys(
            IReadOnlyList<string> values,
            EnemyVocabularyCategory category,
            string path,
            string codePrefix,
            ICounterWindowAndPressureReferenceResolver resolver,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < values.Count; index++)
            {
                string key = values[index];
                string keyPath = path + "[" + index + "]";
                ValidateId(key, keyPath, issues);
                if (!seen.Add(key))
                {
                    issues.Add(Issue(
                        codePrefix + "_DUPLICATE",
                        keyPath,
                        "Vocabulary key cannot repeat."));
                }

                if (resolver != null && !resolver.HasVocabularyKey(category, key))
                {
                    issues.Add(Issue(
                        codePrefix + "_UNRESOLVED",
                        keyPath,
                        "Vocabulary key is unknown or belongs to the wrong E02 category."));
                }
            }
        }

        private static void ValidateMatchMode(
            RequirementMatchMode value,
            string path,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            if (!Enum.IsDefined(typeof(RequirementMatchMode), value))
            {
                issues.Add(Issue(
                    "MATCH_MODE_INVALID",
                    path,
                    "Match mode must be Any or All."));
            }
        }

        private static void ValidateIsolation(
            IEnemyDomainIsolationMetadata value,
            string path,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            if (!value.DevOnly || value.IsEnabled || value.EntersFormalFlow)
            {
                issues.Add(Issue(
                    "DEV_ISOLATION_INVALID",
                    path,
                    "Required isolation is devOnly=true, isEnabled=false, entersFormalFlow=false."));
            }
        }

        private static void AddUnique<T>(
            IDictionary<string, T> values,
            string id,
            T value,
            string path,
            string code,
            ICollection<CounterWindowAndPressureValidationIssue> issues)
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
            ICollection<CounterWindowAndPressureValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                issues.Add(Issue(
                    "ID_EMPTY",
                    path,
                    "Stable IDs and keys must not be empty or whitespace."));
                return;
            }

            if (!string.Equals(id, id.Trim(), StringComparison.Ordinal))
            {
                issues.Add(Issue(
                    "ID_OUTER_WHITESPACE",
                    path,
                    "Stable IDs and keys are never trimmed implicitly."));
            }
        }

        private static CounterWindowAndPressureValidationIssue Issue(
            string code,
            string path,
            string message)
        {
            return new CounterWindowAndPressureValidationIssue(code, path, message);
        }
    }
}
