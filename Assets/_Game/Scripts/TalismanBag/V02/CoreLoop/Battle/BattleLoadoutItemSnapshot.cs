using System;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Battle
{
    [Serializable]
    public sealed class BattleLoadoutItemSnapshot
    {
        public string runtimeId;
        public string itemId;
        public string displayName;
        public int level = 1;
        public Vector2Int gridPosition;
        public bool isPowered;
        public ComputedTalismanStats computedStats = new();
    }
}
