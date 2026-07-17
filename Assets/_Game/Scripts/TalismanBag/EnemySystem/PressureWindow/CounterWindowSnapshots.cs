using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;

namespace TalismanBag.EnemySystem.PressureWindow
{
    public sealed class CounterWindowPlayerProjection
    {
        public CounterWindowPlayerProjection(
            string counterWindowId,
            string publicWindowLabelKey,
            string publicWindowOpenCueKey,
            string publicWindowCloseCueKey)
        {
            CounterWindowId = CounterWindowAndPressureReadOnly.Text(counterWindowId);
            PublicWindowLabelKey = CounterWindowAndPressureReadOnly.Text(
                publicWindowLabelKey);
            PublicWindowOpenCueKey = CounterWindowAndPressureReadOnly.Text(
                publicWindowOpenCueKey);
            PublicWindowCloseCueKey = CounterWindowAndPressureReadOnly.Text(
                publicWindowCloseCueKey);
        }

        public string CounterWindowId { get; }
        public string PublicWindowLabelKey { get; }
        public string PublicWindowOpenCueKey { get; }
        public string PublicWindowCloseCueKey { get; }

        internal CounterWindowPlayerProjection Clone()
        {
            return new CounterWindowPlayerProjection(
                CounterWindowId,
                PublicWindowLabelKey,
                PublicWindowOpenCueKey,
                PublicWindowCloseCueKey);
        }
    }

    public sealed class CounterWindowConditionSnapshot
    {
        public CounterWindowConditionSnapshot(
            string conditionId,
            CounterWindowConditionKind kind,
            string referenceId)
        {
            ConditionId = CounterWindowAndPressureReadOnly.Text(conditionId);
            Kind = kind;
            ReferenceId = CounterWindowAndPressureReadOnly.Text(referenceId);
        }

        public string ConditionId { get; }
        public CounterWindowConditionKind Kind { get; }
        public string ReferenceId { get; }

        internal CounterWindowConditionSnapshot Clone()
        {
            return new CounterWindowConditionSnapshot(ConditionId, Kind, ReferenceId);
        }
    }

    public sealed class CounterWindowInternalSpec
    {
        private readonly ReadOnlyCollection<CounterWindowConditionSnapshot> openConditions;
        private readonly ReadOnlyCollection<CounterWindowConditionSnapshot> closeConditions;

        public CounterWindowInternalSpec(
            string counterWindowTypeKey,
            RequirementMatchMode openConditionMatchMode,
            IEnumerable<CounterWindowConditionSnapshot> openConditions,
            RequirementMatchMode closeConditionMatchMode,
            IEnumerable<CounterWindowConditionSnapshot> closeConditions,
            int maximumDurationMilliseconds)
        {
            CounterWindowTypeKey = CounterWindowAndPressureReadOnly.Text(
                counterWindowTypeKey);
            OpenConditionMatchMode = openConditionMatchMode;
            this.openConditions = FreezeConditions(openConditions);
            CloseConditionMatchMode = closeConditionMatchMode;
            this.closeConditions = FreezeConditions(closeConditions);
            MaximumDurationMilliseconds = maximumDurationMilliseconds;
        }

        public string CounterWindowTypeKey { get; }
        public RequirementMatchMode OpenConditionMatchMode { get; }
        public IReadOnlyList<CounterWindowConditionSnapshot> OpenConditions => openConditions;
        public RequirementMatchMode CloseConditionMatchMode { get; }
        public IReadOnlyList<CounterWindowConditionSnapshot> CloseConditions => closeConditions;
        public int MaximumDurationMilliseconds { get; }

        internal CounterWindowInternalSpec Clone()
        {
            return new CounterWindowInternalSpec(
                CounterWindowTypeKey,
                OpenConditionMatchMode,
                openConditions,
                CloseConditionMatchMode,
                closeConditions,
                MaximumDurationMilliseconds);
        }

        private static ReadOnlyCollection<CounterWindowConditionSnapshot> FreezeConditions(
            IEnumerable<CounterWindowConditionSnapshot> values)
        {
            return CounterWindowAndPressureReadOnly.Freeze(values, value => value.Clone())
                .OrderBy(
                    value => value == null ? string.Empty : value.ConditionId,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? int.MinValue : (int)value.Kind)
                .ThenBy(
                    value => value == null ? string.Empty : value.ReferenceId,
                    StringComparer.Ordinal)
                .ToReadOnly();
        }
    }

    public sealed class CounterWindowDeveloperDiagnostics
    {
        private readonly ReadOnlyCollection<string> developerDiagnosticCategoryKeys;
        private readonly ReadOnlyCollection<string> sourceReferenceIds;

        public CounterWindowDeveloperDiagnostics(
            IEnumerable<string> developerDiagnosticCategoryKeys,
            IEnumerable<string> sourceReferenceIds)
        {
            this.developerDiagnosticCategoryKeys =
                CounterWindowAndPressureReadOnly.Strings(
                    developerDiagnosticCategoryKeys);
            this.sourceReferenceIds = CounterWindowAndPressureReadOnly.Strings(
                sourceReferenceIds);
        }

        public IReadOnlyList<string> DeveloperDiagnosticCategoryKeys =>
            developerDiagnosticCategoryKeys;
        public IReadOnlyList<string> SourceReferenceIds => sourceReferenceIds;
        public bool DeveloperOnly => true;

        internal CounterWindowDeveloperDiagnostics Clone()
        {
            return new CounterWindowDeveloperDiagnostics(
                developerDiagnosticCategoryKeys,
                sourceReferenceIds);
        }
    }

    public sealed class CounterWindowProfileSnapshot
    {
        public CounterWindowProfileSnapshot(
            CounterWindowReference counterWindowReference,
            CounterWindowPlayerProjection playerSafe,
            CounterWindowInternalSpec internalOnly,
            CounterWindowDeveloperDiagnostics developerOnly)
        {
            CounterWindowReference = counterWindowReference == null
                ? null
                : new CounterWindowReference(
                    counterWindowReference.StableId,
                    counterWindowReference.DevOnly,
                    counterWindowReference.IsEnabled,
                    counterWindowReference.EntersFormalFlow);
            PlayerSafe = playerSafe == null ? null : playerSafe.Clone();
            InternalOnly = internalOnly == null ? null : internalOnly.Clone();
            DeveloperOnly = developerOnly == null ? null : developerOnly.Clone();
        }

        public CounterWindowReference CounterWindowReference { get; }
        public string CounterWindowId => CounterWindowReference == null
            ? string.Empty
            : CounterWindowReference.StableId;
        public CounterWindowPlayerProjection PlayerSafe { get; }
        public CounterWindowInternalSpec InternalOnly { get; }
        public CounterWindowDeveloperDiagnostics DeveloperOnly { get; }

        internal CounterWindowProfileSnapshot Clone()
        {
            return new CounterWindowProfileSnapshot(
                CounterWindowReference,
                PlayerSafe,
                InternalOnly,
                DeveloperOnly);
        }
    }

    public sealed class CounterWindowSourceBindingSnapshot : IEnemyDomainIsolationMetadata
    {
        public CounterWindowSourceBindingSnapshot(
            PressureSourceKind sourceKind,
            string sourceId,
            string counterWindowId,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            SourceKind = sourceKind;
            SourceId = CounterWindowAndPressureReadOnly.Text(sourceId);
            CounterWindowId = CounterWindowAndPressureReadOnly.Text(counterWindowId);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public PressureSourceKind SourceKind { get; }
        public string SourceId { get; }
        public string CounterWindowId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal CounterWindowSourceBindingSnapshot Clone()
        {
            return new CounterWindowSourceBindingSnapshot(
                SourceKind,
                SourceId,
                CounterWindowId,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }

    public sealed class PressureCounterWindowBindingSnapshot : IEnemyDomainIsolationMetadata
    {
        public PressureCounterWindowBindingSnapshot(
            string buildPressureProfileId,
            string counterWindowId,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            BuildPressureProfileId = CounterWindowAndPressureReadOnly.Text(
                buildPressureProfileId);
            CounterWindowId = CounterWindowAndPressureReadOnly.Text(counterWindowId);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public string BuildPressureProfileId { get; }
        public string CounterWindowId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }

        internal PressureCounterWindowBindingSnapshot Clone()
        {
            return new PressureCounterWindowBindingSnapshot(
                BuildPressureProfileId,
                CounterWindowId,
                DevOnly,
                IsEnabled,
                EntersFormalFlow);
        }
    }

    public sealed class CounterWindowAndPressureCatalogInput
    {
        private readonly ReadOnlyCollection<BuildPressureProfileSnapshot>
            buildPressureProfiles;
        private readonly ReadOnlyCollection<CounterWindowProfileSnapshot>
            counterWindowProfiles;
        private readonly ReadOnlyCollection<PressureSourceBindingSnapshot>
            pressureSourceBindings;
        private readonly ReadOnlyCollection<CounterWindowSourceBindingSnapshot>
            counterWindowSourceBindings;
        private readonly ReadOnlyCollection<PressureCounterWindowBindingSnapshot>
            pressureCounterWindowBindings;

        public CounterWindowAndPressureCatalogInput(
            IEnumerable<BuildPressureProfileSnapshot> buildPressureProfiles,
            IEnumerable<CounterWindowProfileSnapshot> counterWindowProfiles,
            IEnumerable<PressureSourceBindingSnapshot> pressureSourceBindings,
            IEnumerable<CounterWindowSourceBindingSnapshot> counterWindowSourceBindings,
            IEnumerable<PressureCounterWindowBindingSnapshot> pressureCounterWindowBindings,
            string schemaId = CounterWindowAndPressureSchema.SchemaId,
            int schemaVersion = CounterWindowAndPressureSchema.SchemaVersion)
        {
            SchemaId = CounterWindowAndPressureReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            this.buildPressureProfiles = CounterWindowAndPressureReadOnly.Freeze(
                buildPressureProfiles,
                value => value.Clone());
            this.counterWindowProfiles = CounterWindowAndPressureReadOnly.Freeze(
                counterWindowProfiles,
                value => value.Clone());
            this.pressureSourceBindings = CounterWindowAndPressureReadOnly.Freeze(
                pressureSourceBindings,
                value => value.Clone());
            this.counterWindowSourceBindings = CounterWindowAndPressureReadOnly.Freeze(
                counterWindowSourceBindings,
                value => value.Clone());
            this.pressureCounterWindowBindings = CounterWindowAndPressureReadOnly.Freeze(
                pressureCounterWindowBindings,
                value => value.Clone());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<BuildPressureProfileSnapshot> BuildPressureProfiles =>
            buildPressureProfiles;
        public IReadOnlyList<CounterWindowProfileSnapshot> CounterWindowProfiles =>
            counterWindowProfiles;
        public IReadOnlyList<PressureSourceBindingSnapshot> PressureSourceBindings =>
            pressureSourceBindings;
        public IReadOnlyList<CounterWindowSourceBindingSnapshot> CounterWindowSourceBindings =>
            counterWindowSourceBindings;
        public IReadOnlyList<PressureCounterWindowBindingSnapshot>
            PressureCounterWindowBindings => pressureCounterWindowBindings;
    }

    public sealed class CounterWindowAndPressureCatalogSnapshot :
        ICounterWindowAndPressureLookup
    {
        private readonly ReadOnlyCollection<BuildPressureProfileSnapshot>
            buildPressureProfiles;
        private readonly ReadOnlyCollection<CounterWindowProfileSnapshot>
            counterWindowProfiles;
        private readonly ReadOnlyCollection<PressureSourceBindingSnapshot>
            pressureSourceBindings;
        private readonly ReadOnlyCollection<CounterWindowSourceBindingSnapshot>
            counterWindowSourceBindings;
        private readonly ReadOnlyCollection<PressureCounterWindowBindingSnapshot>
            pressureCounterWindowBindings;
        private readonly IReadOnlyDictionary<string, BuildPressureProfileSnapshot> pressureById;
        private readonly IReadOnlyDictionary<string, CounterWindowProfileSnapshot> windowById;
        private readonly string playerSafePayload;

        internal CounterWindowAndPressureCatalogSnapshot(
            CounterWindowAndPressureCatalogInput input)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            buildPressureProfiles = CounterWindowAndPressureReadOnly.Freeze(
                    input.BuildPressureProfiles,
                    value => value.Clone())
                .OrderBy(value => value.BuildPressureProfileId, StringComparer.Ordinal)
                .ToReadOnly();
            counterWindowProfiles = CounterWindowAndPressureReadOnly.Freeze(
                    input.CounterWindowProfiles,
                    value => value.Clone())
                .OrderBy(value => value.CounterWindowId, StringComparer.Ordinal)
                .ToReadOnly();
            pressureSourceBindings = CounterWindowAndPressureReadOnly.Freeze(
                    input.PressureSourceBindings,
                    value => value.Clone())
                .OrderBy(value => value.SourceKind)
                .ThenBy(value => value.SourceId, StringComparer.Ordinal)
                .ThenBy(value => value.BuildPressureProfileId, StringComparer.Ordinal)
                .ToReadOnly();
            counterWindowSourceBindings = CounterWindowAndPressureReadOnly.Freeze(
                    input.CounterWindowSourceBindings,
                    value => value.Clone())
                .OrderBy(value => value.SourceKind)
                .ThenBy(value => value.SourceId, StringComparer.Ordinal)
                .ThenBy(value => value.CounterWindowId, StringComparer.Ordinal)
                .ToReadOnly();
            pressureCounterWindowBindings = CounterWindowAndPressureReadOnly.Freeze(
                    input.PressureCounterWindowBindings,
                    value => value.Clone())
                .OrderBy(value => value.BuildPressureProfileId, StringComparer.Ordinal)
                .ThenBy(value => value.CounterWindowId, StringComparer.Ordinal)
                .ToReadOnly();

            pressureById = Dictionary(
                buildPressureProfiles,
                value => value.BuildPressureProfileId);
            windowById = Dictionary(counterWindowProfiles, value => value.CounterWindowId);

            string fullPayload = CounterWindowAndPressureCanonical.Catalog(
                SchemaId,
                SchemaVersion,
                buildPressureProfiles,
                counterWindowProfiles,
                pressureSourceBindings,
                counterWindowSourceBindings,
                pressureCounterWindowBindings);
            playerSafePayload = CounterWindowAndPressureCanonical.PlayerSafeCatalog(
                SchemaId,
                SchemaVersion,
                buildPressureProfiles,
                counterWindowProfiles);
            CanonicalSignature = CounterWindowAndPressureCanonical.Hash(fullPayload);
            PlayerSafeCanonicalSignature = CounterWindowAndPressureCanonical.Hash(
                playerSafePayload);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<BuildPressureProfileSnapshot> BuildPressureProfiles =>
            buildPressureProfiles;
        public IReadOnlyList<CounterWindowProfileSnapshot> CounterWindowProfiles =>
            counterWindowProfiles;
        public IReadOnlyList<PressureSourceBindingSnapshot> PressureSourceBindings =>
            pressureSourceBindings;
        public IReadOnlyList<CounterWindowSourceBindingSnapshot> CounterWindowSourceBindings =>
            counterWindowSourceBindings;
        public IReadOnlyList<PressureCounterWindowBindingSnapshot>
            PressureCounterWindowBindings => pressureCounterWindowBindings;
        public string CanonicalSignature { get; }
        public string PlayerSafeCanonicalSignature { get; }

        public string BuildPlayerSafeCanonicalPayload()
        {
            return playerSafePayload;
        }

        public bool TryGetBuildPressureProfileById(
            string buildPressureProfileId,
            out BuildPressureProfileSnapshot profile)
        {
            if (buildPressureProfileId != null)
            {
                return pressureById.TryGetValue(buildPressureProfileId, out profile);
            }

            profile = null;
            return false;
        }

        public bool TryGetCounterWindowProfileById(
            string counterWindowId,
            out CounterWindowProfileSnapshot profile)
        {
            if (counterWindowId != null)
            {
                return windowById.TryGetValue(counterWindowId, out profile);
            }

            profile = null;
            return false;
        }

        internal static string SourceBindingIdentity(
            PressureSourceKind sourceKind,
            string sourceId,
            string targetId)
        {
            return ((int)sourceKind).ToString(CultureInfo.InvariantCulture)
                + "\u001f"
                + (sourceId ?? string.Empty)
                + "\u001f"
                + (targetId ?? string.Empty);
        }

        internal static string PressureWindowIdentity(string pressureId, string windowId)
        {
            return (pressureId ?? string.Empty)
                + "\u001f"
                + (windowId ?? string.Empty);
        }

        private static IReadOnlyDictionary<string, T> Dictionary<T>(
            IEnumerable<T> values,
            Func<T, string> key)
        {
            return new ReadOnlyDictionary<string, T>(
                values.ToDictionary(key, value => value, StringComparer.Ordinal));
        }
    }

    internal static class CounterWindowAndPressureCanonical
    {
        public static string Catalog(
            string schemaId,
            int schemaVersion,
            IEnumerable<BuildPressureProfileSnapshot> pressures,
            IEnumerable<CounterWindowProfileSnapshot> windows,
            IEnumerable<PressureSourceBindingSnapshot> pressureSources,
            IEnumerable<CounterWindowSourceBindingSnapshot> windowSources,
            IEnumerable<PressureCounterWindowBindingSnapshot> pressureWindows)
        {
            StringBuilder builder = new StringBuilder(16384);
            Field(builder, "schemaId", schemaId);
            Field(builder, "schemaVersion", schemaVersion.ToString(CultureInfo.InvariantCulture));
            Rows(builder, "buildPressureProfiles", pressures, Pressure);
            Rows(builder, "counterWindowProfiles", windows, Window);
            Rows(builder, "pressureSourceBindings", pressureSources, PressureSource);
            Rows(builder, "counterWindowSourceBindings", windowSources, WindowSource);
            Rows(builder, "pressureCounterWindowBindings", pressureWindows, PressureWindow);
            return builder.ToString();
        }

        public static string PlayerSafeCatalog(
            string schemaId,
            int schemaVersion,
            IEnumerable<BuildPressureProfileSnapshot> pressures,
            IEnumerable<CounterWindowProfileSnapshot> windows)
        {
            StringBuilder builder = new StringBuilder(4096);
            Field(builder, "schemaId", schemaId);
            Field(builder, "schemaVersion", schemaVersion.ToString(CultureInfo.InvariantCulture));
            Rows(builder, "buildPressureProfiles", pressures, value => PressurePlayer(value.PlayerSafe));
            Rows(builder, "counterWindowProfiles", windows, value => WindowPlayer(value.PlayerSafe));
            return builder.ToString();
        }

        private static string Pressure(BuildPressureProfileSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "buildPressureProfileId", value.BuildPressureProfileId);
            Isolation(builder, value);
            Field(builder, "playerSafe", PressurePlayer(value.PlayerSafe));
            Rows(builder, "pressureChannelContributions", value.InternalOnly.PressureChannelContributions, Contribution);
            Rows(builder, "requirementGroups", value.DeveloperOnly.RequirementGroups, RequirementGroup);
            Strings(builder, "developerDiagnosticCategoryKeys", value.DeveloperOnly.DeveloperDiagnosticCategoryKeys);
            Strings(builder, "sourceReferenceIds", value.DeveloperOnly.SourceReferenceIds);
            return builder.ToString();
        }

        private static string PressurePlayer(BuildPressurePlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "buildPressureProfileId", value.BuildPressureProfileId);
            Field(builder, "publicPressureLabelKey", value.PublicPressureLabelKey);
            Field(builder, "publicPressureHintKey", value.PublicPressureHintKey);
            Strings(builder, "playerHintCategoryKeys", value.PlayerHintCategoryKeys);
            return builder.ToString();
        }

        private static string Contribution(PressureChannelContributionSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "pressureChannelKey", value.PressureChannelKey);
            Field(builder, "weightBasisPoints", value.WeightBasisPoints.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static string RequirementGroup(BuildCapabilityRequirementGroupSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "requirementGroupId", value.RequirementGroupId);
            Field(builder, "requirementRole", ((int)value.RequirementRole).ToString(CultureInfo.InvariantCulture));
            Field(builder, "requirementMatchMode", ((int)value.RequirementMatchMode).ToString(CultureInfo.InvariantCulture));
            Rows(builder, "requirements", value.Requirements, Requirement);
            return builder.ToString();
        }

        private static string Requirement(BuildCapabilityRequirementSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "buildCapabilityKey", value.BuildCapabilityKey);
            Field(builder, "minimumCapabilityBasisPoints", value.MinimumCapabilityBasisPoints.ToString(CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static string Window(CounterWindowProfileSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "counterWindowId", value.CounterWindowId);
            Isolation(builder, value.CounterWindowReference);
            Field(builder, "playerSafe", WindowPlayer(value.PlayerSafe));
            Field(builder, "counterWindowTypeKey", value.InternalOnly.CounterWindowTypeKey);
            Field(builder, "openConditionMatchMode", ((int)value.InternalOnly.OpenConditionMatchMode).ToString(CultureInfo.InvariantCulture));
            Rows(builder, "openConditions", value.InternalOnly.OpenConditions, Condition);
            Field(builder, "closeConditionMatchMode", ((int)value.InternalOnly.CloseConditionMatchMode).ToString(CultureInfo.InvariantCulture));
            Rows(builder, "closeConditions", value.InternalOnly.CloseConditions, Condition);
            Field(builder, "maximumDurationMilliseconds", value.InternalOnly.MaximumDurationMilliseconds.ToString(CultureInfo.InvariantCulture));
            Strings(builder, "developerDiagnosticCategoryKeys", value.DeveloperOnly.DeveloperDiagnosticCategoryKeys);
            Strings(builder, "sourceReferenceIds", value.DeveloperOnly.SourceReferenceIds);
            return builder.ToString();
        }

        private static string WindowPlayer(CounterWindowPlayerProjection value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "counterWindowId", value.CounterWindowId);
            Field(builder, "publicWindowLabelKey", value.PublicWindowLabelKey);
            Field(builder, "publicWindowOpenCueKey", value.PublicWindowOpenCueKey);
            Field(builder, "publicWindowCloseCueKey", value.PublicWindowCloseCueKey);
            return builder.ToString();
        }

        private static string Condition(CounterWindowConditionSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "conditionId", value.ConditionId);
            Field(builder, "kind", ((int)value.Kind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "referenceId", value.ReferenceId);
            return builder.ToString();
        }

        private static string PressureSource(PressureSourceBindingSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "sourceKind", ((int)value.SourceKind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "sourceId", value.SourceId);
            Field(builder, "buildPressureProfileId", value.BuildPressureProfileId);
            Isolation(builder, value);
            return builder.ToString();
        }

        private static string WindowSource(CounterWindowSourceBindingSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "sourceKind", ((int)value.SourceKind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "sourceId", value.SourceId);
            Field(builder, "counterWindowId", value.CounterWindowId);
            Isolation(builder, value);
            return builder.ToString();
        }

        private static string PressureWindow(PressureCounterWindowBindingSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "buildPressureProfileId", value.BuildPressureProfileId);
            Field(builder, "counterWindowId", value.CounterWindowId);
            Isolation(builder, value);
            return builder.ToString();
        }

        private static void Rows<T>(
            StringBuilder builder,
            string name,
            IEnumerable<T> values,
            Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .Select(canonical)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void Strings(
            StringBuilder builder,
            string name,
            IEnumerable<string> values)
        {
            string[] rows = (values ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            WriteRows(builder, name, rows);
        }

        private static void WriteRows(
            StringBuilder builder,
            string name,
            IReadOnlyList<string> rows)
        {
            Field(builder, name + ".count", rows.Count.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Count; index++)
            {
                Field(
                    builder,
                    name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                    rows[index]);
            }
        }

        private static void Isolation(
            StringBuilder builder,
            IEnemyDomainIsolationMetadata value)
        {
            Field(builder, "devOnly", value.DevOnly ? "1" : "0");
            Field(builder, "isEnabled", value.IsEnabled ? "1" : "0");
            Field(builder, "entersFormalFlow", value.EntersFormalFlow ? "1" : "0");
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeName)
                .Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeValue)
                .Append(';');
        }

        public static string Hash(string payload)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(payload ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte value in bytes)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
