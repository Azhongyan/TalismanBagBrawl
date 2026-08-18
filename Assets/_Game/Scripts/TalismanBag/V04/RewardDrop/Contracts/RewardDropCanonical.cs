using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.V04.RewardDrop.Contracts
{
    public static class RewardDropCanonical
    {
        public const string EncodingName = "UTF-8";
        public const string SignatureAlgorithm = "SHA-256";

        public static string NormalizeIdentifier(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        public static bool EqualsOrdinal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        public static string ComputeSignature(string canonicalPayload)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(canonicalPayload ?? string.Empty);
            byte[] hash;
            using (SHA256 sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(bytes);
            }

            StringBuilder hex = new StringBuilder(hash.Length * 2);
            for (int index = 0; index < hash.Length; index++)
            {
                hex.Append(hash[index].ToString("x2", CultureInfo.InvariantCulture));
            }

            return hex.ToString();
        }

        public static IReadOnlyList<string> NormalizeAndSortOrdinalSet(
            IEnumerable<string> values)
        {
            string[] normalized = (values ?? Enumerable.Empty<string>())
                .Select(NormalizeIdentifier)
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            return Array.AsReadOnly(normalized);
        }

        internal static IReadOnlyList<T> CopyReadOnly<T>(IEnumerable<T> values)
        {
            T[] copy = values == null ? Array.Empty<T>() : values.ToArray();
            return new ReadOnlyCollection<T>(copy);
        }

        internal static IReadOnlyList<RewardAcquisitionMode> NormalizeModeSet(
            IEnumerable<RewardAcquisitionMode> values)
        {
            RewardAcquisitionMode[] copy = (values ?? Enumerable.Empty<RewardAcquisitionMode>())
                .Distinct()
                .OrderBy(value => value.ToString(), StringComparer.Ordinal)
                .ToArray();
            return Array.AsReadOnly(copy);
        }

        internal static string CreatePayload(Action<RewardDropCanonicalBuilder> write)
        {
            RewardDropCanonicalBuilder builder = new RewardDropCanonicalBuilder();
            write(builder);
            return builder.Build();
        }

        internal static void AddLaunchContext(
            RewardDropCanonicalBuilder builder,
            string keyPrefix,
            RewardLaunchContextIdentity context)
        {
            builder.AddString(keyPrefix + ".schemaId", context == null ? string.Empty : context.schemaId);
            builder.AddString(
                keyPrefix + ".launchContextId",
                context == null ? string.Empty : context.launchContextId);
            builder.AddEnum(
                keyPrefix + ".launchContextKind",
                context == null ? 0 : (int)context.launchContextKind);
            builder.AddEnum(
                keyPrefix + ".persistencePolicy",
                context == null ? 0 : (int)context.persistencePolicy);
            builder.AddInt(
                keyPrefix + ".contextVersion",
                context == null ? 0 : context.contextVersion);
        }

        internal static void AddSubject(
            RewardDropCanonicalBuilder builder,
            string keyPrefix,
            RewardSubjectIdentity subject)
        {
            builder.AddString(keyPrefix + ".schemaId", subject == null ? string.Empty : subject.schemaId);
            builder.AddString(
                keyPrefix + ".subjectId",
                subject == null ? string.Empty : subject.subjectId);
            builder.AddEnum(
                keyPrefix + ".subjectKind",
                subject == null ? 0 : (int)subject.subjectKind);
            builder.AddString(
                keyPrefix + ".sourceCatalogId",
                subject == null ? string.Empty : subject.sourceCatalogId);
            builder.AddInt(
                keyPrefix + ".classificationVersion",
                subject == null ? 0 : subject.classificationVersion);
        }
    }

    internal sealed class RewardDropCanonicalBuilder
    {
        private readonly StringBuilder buffer = new StringBuilder();

        public void AddString(string key, string value)
        {
            string normalizedValue = value ?? string.Empty;
            int byteCount = Encoding.UTF8.GetByteCount(normalizedValue);
            buffer.Append(key);
            buffer.Append("=s");
            buffer.Append(byteCount.ToString(CultureInfo.InvariantCulture));
            buffer.Append(':');
            buffer.Append(normalizedValue);
            buffer.Append(';');
        }

        public void AddInt(string key, int value)
        {
            buffer.Append(key);
            buffer.Append("=i");
            buffer.Append(value.ToString(CultureInfo.InvariantCulture));
            buffer.Append(';');
        }

        public void AddLong(string key, long value)
        {
            buffer.Append(key);
            buffer.Append("=l");
            buffer.Append(value.ToString(CultureInfo.InvariantCulture));
            buffer.Append(';');
        }

        public void AddBool(string key, bool value)
        {
            buffer.Append(key);
            buffer.Append(value ? "=b1;" : "=b0;");
        }

        public void AddEnum(string key, int value)
        {
            buffer.Append(key);
            buffer.Append("=e");
            buffer.Append(value.ToString(CultureInfo.InvariantCulture));
            buffer.Append(';');
        }

        public string Build()
        {
            return buffer.ToString();
        }
    }
}
