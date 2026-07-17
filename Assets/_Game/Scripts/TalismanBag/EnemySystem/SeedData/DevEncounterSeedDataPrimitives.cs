using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.SeedData
{
    public static class DevEncounterSeedDataSchema
    {
        public const string SchemaId = "DevEncounterSeedData.v1";
        public const int SchemaVersion = 1;
    }

    public sealed class DevEncounterSeedDataInput
    {
        public DevEncounterSeedDataInput(
            EnemyMechanicVocabularySnapshot enemyMechanicVocabularySnapshot,
            EnemyValidationContentSnapshot enemyValidationContentSnapshot)
        {
            EnemyMechanicVocabularySnapshot = enemyMechanicVocabularySnapshot;
            EnemyValidationContentSnapshot = enemyValidationContentSnapshot;
        }

        public EnemyMechanicVocabularySnapshot EnemyMechanicVocabularySnapshot { get; }
        public EnemyValidationContentSnapshot EnemyValidationContentSnapshot { get; }
    }

    public enum DevEncounterSeedValidationCategory
    {
        Schema = 0,
        Identity = 1,
        Mapping = 2,
        Composition = 3,
        SkillPhase = 4,
        PressureWindow = 5,
        Reference = 6,
        Isolation = 7,
        Signature = 8,
        PlayerLeak = 9,
        Dependency = 10
    }

    public sealed class DevEncounterSeedValidationIssue
    {
        public DevEncounterSeedValidationIssue(string code,
            DevEncounterSeedValidationCategory category, string path, string message)
        {
            Code = code ?? string.Empty;
            Category = category;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public DevEncounterSeedValidationCategory Category { get; }
        public string Path { get; }
        public string Message { get; }
    }

    public sealed class DevEncounterSeedValidationException : ArgumentException
    {
        public DevEncounterSeedValidationException(IReadOnlyList<DevEncounterSeedValidationIssue> issues)
            : base("Dev encounter seed validation failed with "
                + (issues == null ? 0 : issues.Count).ToString(CultureInfo.InvariantCulture)
                + " issue(s).")
        {
            Issues = Array.AsReadOnly((issues ?? Array.Empty<DevEncounterSeedValidationIssue>()).ToArray());
        }

        public IReadOnlyList<DevEncounterSeedValidationIssue> Issues { get; }
    }

    public interface IDevEncounterSeedDataProvider
    {
        DevEncounterSeedDataSnapshot CreateSnapshot(DevEncounterSeedDataInput input);
    }

    public interface IDevEncounterSeedDataValidator
    {
        IReadOnlyList<DevEncounterSeedValidationIssue> ValidateInput(DevEncounterSeedDataInput input);
        IReadOnlyList<DevEncounterSeedValidationIssue> ValidateSnapshot(DevEncounterSeedDataSnapshot snapshot);
    }

    internal static class DevEncounterSeedDataCanonical
    {
        public static string Text(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Enumerable.Empty<T>()).ToArray());
        }

        public static ReadOnlyCollection<string> Set(IEnumerable<string> values)
        {
            return Array.AsReadOnly((values ?? Enumerable.Empty<string>())
                .Select(Text).Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToArray());
        }

        public static void Field(StringBuilder builder, string name, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(name).Append('=').Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }

        public static void Rows(StringBuilder builder, string name, IEnumerable<string> rows)
        {
            string[] stable = (rows ?? Enumerable.Empty<string>()).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Field(builder, name + ".count", stable.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < stable.Length; index++)
            {
                Field(builder, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", stable[index]);
            }
        }

        public static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte value in digest) builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }

        public static bool IsSignature(string value)
        {
            if (value == null || value.Length != 71 || !value.StartsWith("sha256:", StringComparison.Ordinal)) return false;
            for (int index = 7; index < value.Length; index++)
            {
                char c = value[index];
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
            }
            return true;
        }

        public static string Isolation(bool devOnly, bool enabled, bool formal)
        {
            return (devOnly ? "1" : "0") + "|" + (enabled ? "1" : "0") + "|" + (formal ? "1" : "0");
        }

        public static string Join(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).OrderBy(value => value, StringComparer.Ordinal));
        }
    }
}
