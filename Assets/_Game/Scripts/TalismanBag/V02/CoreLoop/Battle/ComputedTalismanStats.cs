using System;

namespace TalismanBag.V02.CoreLoop.Battle
{
    [Serializable]
    public sealed class ComputedTalismanStats
    {
        public int computedDamage;
        public float computedCooldown;
        public int computedShieldValue;
        public float computedBreakShieldRate = 1f;
        public float computedControlDuration = 1f;
    }
}
