using System;
using System.Collections.Generic;
using TalismanBag.Items;

namespace TalismanBag.V02.CoreLoop.Battle
{
    [Serializable]
    public sealed class BattleLoadoutSnapshot
    {
        public List<BattleLoadoutItemSnapshot> items = new();

        public BattleLoadoutItemSnapshot Find(TalismanItemRuntime runtimeItem)
        {
            if (runtimeItem == null || items == null)
            {
                return null;
            }

            foreach (BattleLoadoutItemSnapshot item in items)
            {
                if (item == null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(runtimeItem.runtimeId) && string.Equals(item.runtimeId, runtimeItem.runtimeId, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            if (runtimeItem.definition == null)
            {
                return null;
            }

            foreach (BattleLoadoutItemSnapshot item in items)
            {
                if (item != null &&
                    string.Equals(item.itemId, runtimeItem.definition.itemId, StringComparison.Ordinal) &&
                    item.gridPosition == runtimeItem.gridPosition)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
