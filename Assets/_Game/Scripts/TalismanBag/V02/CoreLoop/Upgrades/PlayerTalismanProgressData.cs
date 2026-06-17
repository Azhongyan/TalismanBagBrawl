using System;
using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Upgrades
{
    [Serializable]
    public sealed class PlayerTalismanProgressData
    {
        public List<TalismanLevelData> talismanLevels = new();

        public void Normalize()
        {
            talismanLevels ??= new List<TalismanLevelData>();
            Dictionary<string, int> mergedLevels = new();

            foreach (TalismanLevelData levelData in talismanLevels)
            {
                if (levelData == null || string.IsNullOrWhiteSpace(levelData.itemId))
                {
                    continue;
                }

                int level = Mathf.Max(1, levelData.level);
                if (!mergedLevels.TryGetValue(levelData.itemId, out int existingLevel) || level > existingLevel)
                {
                    mergedLevels[levelData.itemId] = level;
                }
            }

            talismanLevels.Clear();
            foreach (KeyValuePair<string, int> pair in mergedLevels)
            {
                talismanLevels.Add(new TalismanLevelData
                {
                    itemId = pair.Key,
                    level = pair.Value
                });
            }
        }

        public int GetLevel(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 1;
            }

            Normalize();
            foreach (TalismanLevelData levelData in talismanLevels)
            {
                if (levelData != null && string.Equals(levelData.itemId, itemId, StringComparison.Ordinal))
                {
                    return Mathf.Max(1, levelData.level);
                }
            }

            return 1;
        }

        public void SetLevel(string itemId, int level)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            Normalize();
            int safeLevel = Mathf.Max(1, level);
            foreach (TalismanLevelData levelData in talismanLevels)
            {
                if (levelData != null && string.Equals(levelData.itemId, itemId, StringComparison.Ordinal))
                {
                    levelData.level = safeLevel;
                    return;
                }
            }

            talismanLevels.Add(new TalismanLevelData
            {
                itemId = itemId,
                level = safeLevel
            });
        }
    }

    [Serializable]
    public sealed class TalismanLevelData
    {
        public string itemId;
        public int level = 1;
    }
}
