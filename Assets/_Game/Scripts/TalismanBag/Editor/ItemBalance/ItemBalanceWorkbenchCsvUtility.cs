using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemBalance
{
    public sealed class ItemBalanceCsvChange
    {
        public bool isCandidateItemPower;
        public ItemBalanceProfile profile;
        public ItemBalanceRarityVersion version;
        public ItemBalanceRange range;
        public long minUnits;
        public long maxUnits;
        public long candidateItemPower;
        public string Summary => isCandidateItemPower
            ? $"{profile.baseItemId}@{version.rarity.ToStableKey()}@candidateItemPower: {version.candidateItemPower} -> {candidateItemPower}"
            : $"{profile.baseItemId}@{version.rarity.ToStableKey()}@{range.statId}: "
                + $"{range.minUnits}..{range.maxUnits} -> {minUnits}..{maxUnits}";
    }

    public sealed class ItemBalanceCsvImportPreview
    {
        public List<ItemBalanceCsvChange> changes = new();
        public List<string> errors = new();
        public bool canApply => errors.Count == 0 && changes.Count > 0;
    }

    public static class ItemBalanceWorkbenchCsvUtility
    {
        public static ItemBalanceCsvImportPreview PreviewImport(ItemBalanceWorkbenchCatalog catalog, string path)
        {
            ItemBalanceCsvImportPreview preview = new();
            if (catalog == null || string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                preview.errors.Add("CSV file or catalog is missing.");
                return preview;
            }

            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2)
            {
                preview.errors.Add("CSV has no data rows.");
                return preview;
            }

            string[] header = ParseLine(lines[0]).ToArray();
            int baseIndex = Array.IndexOf(header, "baseItemId");
            int rarityIndex = Array.IndexOf(header, "rarity");
            int statIndex = Array.IndexOf(header, "statId");
            int minIndex = Array.IndexOf(header, "minUnits");
            int maxIndex = Array.IndexOf(header, "maxUnits");
            int powerIndex = Array.IndexOf(header, "candidateItemPower");
            bool powerMode = baseIndex >= 0 && rarityIndex >= 0 && powerIndex >= 0;
            if (!powerMode && new[] { baseIndex, rarityIndex, statIndex, minIndex, maxIndex }.Any(value => value < 0))
            {
                preview.errors.Add("CSV requires baseItemId, rarity, statId, minUnits and maxUnits columns.");
                return preview;
            }

            Dictionary<string, ItemBalanceCsvChange> unique = new(StringComparer.Ordinal);
            for (int row = 1; row < lines.Length; row++)
            {
                if (string.IsNullOrWhiteSpace(lines[row])) continue;
                string[] values = ParseLine(lines[row]).ToArray();
                int required = powerMode ? new[] { baseIndex, rarityIndex, powerIndex }.Max()
                    : new[] { baseIndex, rarityIndex, statIndex, minIndex, maxIndex }.Max();
                if (values.Length <= required)
                {
                    preview.errors.Add($"Row {row + 1}: column count is incomplete.");
                    continue;
                }

                string baseItemId = values[baseIndex].Trim();
                if (baseItemId == "I031")
                {
                    preview.errors.Add($"Row {row + 1}: I031 is explicitly rejected.");
                    continue;
                }

                ItemBalanceProfile profile = catalog.FindProfile(baseItemId);
                if (profile == null)
                {
                    preview.errors.Add($"Row {row + 1}: unknown baseItemId '{baseItemId}'.");
                    continue;
                }

                if (!ItemInstanceRarityCatalog.TryParseStableKey(values[rarityIndex].Trim(), out ItemInstanceRarity rarity))
                {
                    preview.errors.Add($"Row {row + 1}: invalid rarity '{values[rarityIndex]}'.");
                    continue;
                }

                ItemBalanceRarityVersion version = profile.FindVersion(rarity);
                if (powerMode)
                {
                    if (version == null || !long.TryParse(values[powerIndex], NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out long power) || power <= 0)
                    {
                        preview.errors.Add($"Row {row + 1}: invalid candidateItemPower.");
                        continue;
                    }
                    string powerKey = baseItemId + "@" + rarity.ToStableKey() + "@candidateItemPower";
                    if (unique.ContainsKey(powerKey))
                    {
                        preview.errors.Add($"Row {row + 1}: duplicate import key '{powerKey}'.");
                        continue;
                    }
                    ItemBalanceCsvChange powerChange = new()
                    {
                        isCandidateItemPower = true,
                        profile = profile,
                        version = version,
                        candidateItemPower = power
                    };
                    unique.Add(powerKey, powerChange);
                    if (version.candidateItemPower != power) preview.changes.Add(powerChange);
                    continue;
                }
                string statId = values[statIndex].Trim();
                ItemBalanceRange range = version?.FindRange(statId);
                ItemBalanceStatDefinition definition = catalog.FindStat(statId);
                if (version == null || range == null || definition == null)
                {
                    preview.errors.Add($"Row {row + 1}: unknown version/stat '{baseItemId}@{rarity.ToStableKey()}@{statId}'.");
                    continue;
                }

                if (!long.TryParse(values[minIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out long min)
                    || !long.TryParse(values[maxIndex], NumberStyles.Integer, CultureInfo.InvariantCulture, out long max)
                    || min > max || definition.stepUnits <= 0
                    || min % definition.stepUnits != 0 || max % definition.stepUnits != 0)
                {
                    preview.errors.Add($"Row {row + 1}: invalid or step-misaligned range.");
                    continue;
                }

                string key = baseItemId + "@" + rarity.ToStableKey() + "@" + statId;
                if (unique.ContainsKey(key))
                {
                    preview.errors.Add($"Row {row + 1}: duplicate import key '{key}'.");
                    continue;
                }

                ItemBalanceCsvChange change = new()
                { profile = profile, version = version, range = range, minUnits = min, maxUnits = max };
                unique.Add(key, change);
                if (range.minUnits != min || range.maxUnits != max) preview.changes.Add(change);
            }

            ValidateMonotonicPreview(catalog, preview);
            return preview;
        }

        public static void Apply(ItemBalanceCsvImportPreview preview)
        {
            if (preview?.canApply != true) return;
            UnityEngine.Object[] objects = preview.changes.Select(value => value.profile)
                .Distinct().Cast<UnityEngine.Object>().ToArray();
            Undo.RecordObjects(objects, "Apply Item Balance CSV Import");
            foreach (ItemBalanceCsvChange change in preview.changes)
            {
                if (change.isCandidateItemPower)
                {
                    change.version.candidateItemPower = change.candidateItemPower;
                    change.version.candidateItemPowerOverridden = true;
                    change.version.candidateDisplaySummary = change.profile.displayName + " · "
                        + change.version.rarity.ToDisplayName() + " · 物品强度（候选）"
                        + change.candidateItemPower.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    change.range.minUnits = change.minUnits;
                    change.range.maxUnits = change.maxUnits;
                }
                EditorUtility.SetDirty(change.profile);
            }
            AssetDatabase.SaveAssets();
        }

        private static void ValidateMonotonicPreview(
            ItemBalanceWorkbenchCatalog catalog,
            ItemBalanceCsvImportPreview preview)
        {
            Dictionary<string, ItemBalanceCsvChange> changes = preview.changes.ToDictionary(value =>
                value.profile.baseItemId + "@" + value.version.rarity.ToStableKey() + "@"
                    + (value.isCandidateItemPower ? "candidateItemPower" : value.range.statId),
                StringComparer.Ordinal);
            foreach (ItemBalanceProfile profile in catalog.profiles.Where(value => value != null))
            foreach (string statId in new[] { profile.primaryStatId, profile.secondaryStatId, "nianCost", "cooldown" })
            {
                ItemBalanceStatDefinition definition = catalog.FindStat(statId);
                (long min, long max)? previous = null;
                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    ItemBalanceRange range = profile.FindVersion(rarity.rarity)?.FindRange(statId);
                    if (range == null) continue;
                    string key = profile.baseItemId + "@" + rarity.stableKey + "@" + statId;
                    (long min, long max) current = changes.TryGetValue(key, out ItemBalanceCsvChange change)
                        ? (change.minUnits, change.maxUnits)
                        : (range.minUnits, range.maxUnits);
                    if (previous.HasValue)
                    {
                        bool invalid = definition.direction == TalismanBag.Items.Generation.Stats.ItemStatDirection.HigherIsBetter
                            ? current.min < previous.Value.min || current.max < previous.Value.max
                            : current.min > previous.Value.min || current.max > previous.Value.max;
                        if (invalid) preview.errors.Add("Import would break monotonic direction: " + profile.baseItemId + "@" + statId);
                    }
                    previous = current;
                }
            }
        }

        public static IEnumerable<string> ParseLine(string line)
        {
            List<string> values = new();
            bool quoted = false;
            System.Text.StringBuilder current = new();
            for (int index = 0; index < (line ?? string.Empty).Length; index++)
            {
                char ch = line[index];
                if (ch == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    { current.Append('"'); index++; }
                    else quoted = !quoted;
                }
                else if (ch == ',' && !quoted)
                { values.Add(current.ToString()); current.Length = 0; }
                else current.Append(ch);
            }
            values.Add(current.ToString());
            return values;
        }
    }
}
