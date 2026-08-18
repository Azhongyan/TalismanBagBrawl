using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TalismanBag.V04.WorldMap
{
    public interface IV04CampaignStageCompletionStorage
    {
        bool HasKey(string key);

        string GetString(string key);

        void SetString(string key, string value);

        void DeleteKey(string key);
    }

    public sealed class V04CampaignStageCompletionSnapshot
    {
        public readonly string schemaId;
        public readonly string campaignId;
        public readonly IReadOnlyList<string> completedStageIds;

        internal V04CampaignStageCompletionSnapshot(
            string schemaId,
            string campaignId,
            IEnumerable<string> completedStageIds)
        {
            this.schemaId = schemaId;
            this.campaignId = campaignId;
            this.completedStageIds = new ReadOnlyCollection<string>(
                (completedStageIds ?? Array.Empty<string>()).ToList());
        }

        public bool IsCleared(string stageId)
        {
            string normalized = V04CampaignStageCompletionRepository.NormalizeStageId(stageId);
            return completedStageIds.Contains(normalized, StringComparer.Ordinal);
        }
    }

    public sealed class V04CampaignStageCompletionRepository
    {
        public const string SchemaId = "V04CampaignStageCompletion.v1";
        public const int SchemaVersion = 1;
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string CampaignId = "bone_aspect_chapter_1";
        public const string StorageKey =
            "TalismanBag.V04.CampaignStageCompletion.bone_aspect_chapter_1.v1";

        private static readonly ReadOnlyCollection<string> SupportedStageIds =
            new ReadOnlyCollection<string>(
                new List<string>
                {
                    "1-1",
                    "1-2",
                    "1-3",
                    "1-4",
                    "1-5"
                });

        private readonly IV04CampaignStageCompletionStorage storage;
        private List<string> completedStageIds = new List<string>();

        public V04CampaignStageCompletionRepository()
            : this(new PlayerPrefsCompletionStorage())
        {
        }

        public V04CampaignStageCompletionRepository(
            IV04CampaignStageCompletionStorage storage)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public V04CampaignStageCompletionSnapshot Snapshot => CreateSnapshot();

        public V04CampaignStageCompletionSnapshot Load()
        {
            List<string> loadedStageIds = new List<string>();
            if (storage.HasKey(StorageKey))
            {
                string payload = storage.GetString(StorageKey);
                if (!TryDeserialize(payload, out loadedStageIds))
                {
                    loadedStageIds = new List<string>();
                }
            }

            completedStageIds = loadedStageIds;
            return CreateSnapshot();
        }

        public bool IsCleared(string stageId)
        {
            string normalized = NormalizeStageId(stageId);
            return completedStageIds.Contains(normalized, StringComparer.Ordinal);
        }

        public bool MarkCleared(string stageId)
        {
            string normalized = NormalizeStageId(stageId);
            int stageIndex = FindSupportedStageIndex(normalized);
            if (stageIndex < 0)
            {
                return false;
            }

            if (stageIndex < completedStageIds.Count)
            {
                return string.Equals(
                    completedStageIds[stageIndex],
                    normalized,
                    StringComparison.Ordinal);
            }

            if (stageIndex != completedStageIds.Count)
            {
                return false;
            }

            List<string> nextStageIds = new List<string>(completedStageIds)
            {
                normalized
            };
            storage.SetString(StorageKey, Serialize(nextStageIds));
            completedStageIds = nextStageIds;
            return true;
        }

        public void Reset()
        {
            storage.DeleteKey(StorageKey);
            completedStageIds = new List<string>();
        }

        internal static string NormalizeStageId(string stageId)
        {
            return string.IsNullOrWhiteSpace(stageId) ? string.Empty : stageId.Trim();
        }

        private V04CampaignStageCompletionSnapshot CreateSnapshot()
        {
            return new V04CampaignStageCompletionSnapshot(
                SchemaId,
                CampaignId,
                completedStageIds);
        }

        private static int FindSupportedStageIndex(string stageId)
        {
            for (int index = 0; index < SupportedStageIds.Count; index++)
            {
                if (string.Equals(
                    SupportedStageIds[index],
                    stageId,
                    StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool TryDeserialize(
            string payload,
            out List<string> normalizedStageIds)
        {
            normalizedStageIds = new List<string>();
            if (string.IsNullOrWhiteSpace(payload))
            {
                return false;
            }

            StrictPayloadReader reader = new StrictPayloadReader(payload);
            if (!reader.TryRead(out StoredPayload storedPayload)
                || !string.Equals(storedPayload.schemaId, SchemaId, StringComparison.Ordinal)
                || storedPayload.version != SchemaVersion
                || !string.Equals(storedPayload.campaignId, CampaignId, StringComparison.Ordinal))
            {
                return false;
            }

            return TryNormalizeCompletedStageIds(
                storedPayload.completedStageIds,
                out normalizedStageIds);
        }

        private static bool TryNormalizeCompletedStageIds(
            IReadOnlyList<string> storedStageIds,
            out List<string> normalizedStageIds)
        {
            normalizedStageIds = new List<string>();
            if (storedStageIds == null || storedStageIds.Count > SupportedStageIds.Count)
            {
                return false;
            }

            bool[] seen = new bool[SupportedStageIds.Count];
            for (int index = 0; index < storedStageIds.Count; index++)
            {
                string normalized = NormalizeStageId(storedStageIds[index]);
                int supportedIndex = FindSupportedStageIndex(normalized);
                if (supportedIndex < 0 || seen[supportedIndex])
                {
                    return false;
                }

                seen[supportedIndex] = true;
            }

            for (int index = 0; index < storedStageIds.Count; index++)
            {
                if (!seen[index])
                {
                    return false;
                }

                normalizedStageIds.Add(SupportedStageIds[index]);
            }

            return true;
        }

        private static string Serialize(IReadOnlyList<string> stageIds)
        {
            StringBuilder builder = new StringBuilder(192);
            builder.Append("{\"schemaId\":\"")
                .Append(EscapeJsonString(SchemaId))
                .Append("\",\"version\":")
                .Append(SchemaVersion.ToString(CultureInfo.InvariantCulture))
                .Append(",\"campaignId\":\"")
                .Append(EscapeJsonString(CampaignId))
                .Append("\",\"completedStageIds\":[");

            for (int index = 0; index < stageIds.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                builder.Append('\"')
                    .Append(EscapeJsonString(stageIds[index]))
                    .Append('\"');
            }

            return builder.Append("]}").ToString();
        }

        private static string EscapeJsonString(string value)
        {
            StringBuilder builder = new StringBuilder(value?.Length ?? 0);
            foreach (char character in value ?? string.Empty)
            {
                switch (character)
                {
                    case '\"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        if (character < 0x20)
                        {
                            builder.Append("\\u")
                                .Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            builder.Append(character);
                        }

                        break;
                }
            }

            return builder.ToString();
        }

        private sealed class PlayerPrefsCompletionStorage : IV04CampaignStageCompletionStorage
        {
            public bool HasKey(string key)
            {
                return PlayerPrefs.HasKey(key);
            }

            public string GetString(string key)
            {
                return PlayerPrefs.GetString(key, string.Empty);
            }

            public void SetString(string key, string value)
            {
                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
            }

            public void DeleteKey(string key)
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        private sealed class StoredPayload
        {
            public string schemaId;
            public int version;
            public string campaignId;
            public List<string> completedStageIds;
        }

        private sealed class StrictPayloadReader
        {
            private readonly string source;
            private int position;

            public StrictPayloadReader(string source)
            {
                this.source = source ?? string.Empty;
            }

            public bool TryRead(out StoredPayload payload)
            {
                payload = new StoredPayload();
                bool hasSchemaId = false;
                bool hasVersion = false;
                bool hasCampaignId = false;
                bool hasCompletedStageIds = false;

                SkipWhitespace();
                if (!TryConsume('{'))
                {
                    return false;
                }

                SkipWhitespace();
                if (TryConsume('}'))
                {
                    return false;
                }

                while (true)
                {
                    if (!TryReadString(out string propertyName))
                    {
                        return false;
                    }

                    SkipWhitespace();
                    if (!TryConsume(':'))
                    {
                        return false;
                    }

                    SkipWhitespace();
                    switch (propertyName)
                    {
                        case "schemaId":
                            if (hasSchemaId || !TryReadString(out payload.schemaId))
                            {
                                return false;
                            }

                            hasSchemaId = true;
                            break;
                        case "version":
                            if (hasVersion || !TryReadNonNegativeInt(out payload.version))
                            {
                                return false;
                            }

                            hasVersion = true;
                            break;
                        case "campaignId":
                            if (hasCampaignId || !TryReadString(out payload.campaignId))
                            {
                                return false;
                            }

                            hasCampaignId = true;
                            break;
                        case "completedStageIds":
                            if (hasCompletedStageIds
                                || !TryReadStringArray(out payload.completedStageIds))
                            {
                                return false;
                            }

                            hasCompletedStageIds = true;
                            break;
                        default:
                            return false;
                    }

                    SkipWhitespace();
                    if (TryConsume('}'))
                    {
                        break;
                    }

                    if (!TryConsume(','))
                    {
                        return false;
                    }

                    SkipWhitespace();
                }

                SkipWhitespace();
                return position == source.Length
                    && hasSchemaId
                    && hasVersion
                    && hasCampaignId
                    && hasCompletedStageIds;
            }

            private bool TryReadStringArray(out List<string> values)
            {
                values = new List<string>();
                if (!TryConsume('['))
                {
                    return false;
                }

                SkipWhitespace();
                if (TryConsume(']'))
                {
                    return true;
                }

                while (true)
                {
                    if (!TryReadString(out string value))
                    {
                        return false;
                    }

                    values.Add(value);
                    SkipWhitespace();
                    if (TryConsume(']'))
                    {
                        return true;
                    }

                    if (!TryConsume(','))
                    {
                        return false;
                    }

                    SkipWhitespace();
                }
            }

            private bool TryReadNonNegativeInt(out int value)
            {
                value = 0;
                int start = position;
                while (position < source.Length
                       && source[position] >= '0'
                       && source[position] <= '9')
                {
                    position++;
                }

                if (position == start)
                {
                    return false;
                }

                string token = source.Substring(start, position - start);
                return int.TryParse(
                    token,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            private bool TryReadString(out string value)
            {
                value = string.Empty;
                if (!TryConsume('\"'))
                {
                    return false;
                }

                StringBuilder builder = new StringBuilder();
                while (position < source.Length)
                {
                    char character = source[position++];
                    if (character == '\"')
                    {
                        value = builder.ToString();
                        return true;
                    }

                    if (character < 0x20)
                    {
                        return false;
                    }

                    if (character != '\\')
                    {
                        builder.Append(character);
                        continue;
                    }

                    if (position >= source.Length)
                    {
                        return false;
                    }

                    char escape = source[position++];
                    switch (escape)
                    {
                        case '\"':
                            builder.Append('\"');
                            break;
                        case '\\':
                            builder.Append('\\');
                            break;
                        case '/':
                            builder.Append('/');
                            break;
                        case 'b':
                            builder.Append('\b');
                            break;
                        case 'f':
                            builder.Append('\f');
                            break;
                        case 'n':
                            builder.Append('\n');
                            break;
                        case 'r':
                            builder.Append('\r');
                            break;
                        case 't':
                            builder.Append('\t');
                            break;
                        case 'u':
                            if (!TryReadUnicodeEscape(out char unicodeCharacter))
                            {
                                return false;
                            }

                            builder.Append(unicodeCharacter);
                            break;
                        default:
                            return false;
                    }
                }

                return false;
            }

            private bool TryReadUnicodeEscape(out char value)
            {
                value = default;
                if (position + 4 > source.Length)
                {
                    return false;
                }

                int codePoint = 0;
                for (int index = 0; index < 4; index++)
                {
                    int digit = HexValue(source[position++]);
                    if (digit < 0)
                    {
                        return false;
                    }

                    codePoint = (codePoint << 4) | digit;
                }

                value = (char)codePoint;
                return true;
            }

            private static int HexValue(char value)
            {
                if (value >= '0' && value <= '9')
                {
                    return value - '0';
                }

                if (value >= 'a' && value <= 'f')
                {
                    return value - 'a' + 10;
                }

                return value >= 'A' && value <= 'F' ? value - 'A' + 10 : -1;
            }

            private void SkipWhitespace()
            {
                while (position < source.Length)
                {
                    char character = source[position];
                    if (character != ' '
                        && character != '\t'
                        && character != '\r'
                        && character != '\n')
                    {
                        return;
                    }

                    position++;
                }
            }

            private bool TryConsume(char expected)
            {
                if (position >= source.Length || source[position] != expected)
                {
                    return false;
                }

                position++;
                return true;
            }
        }
    }
}
