using System;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleItemStatsSnapshot
    {
        public string statProfileId = string.Empty;
        public int attack;
        public int guard;
        public int spirit;
        public int control;
        public int shieldBreak;
        public int cleanse;
        public int manaGainPerTick;
        public int manaCostPerCast;
        public float castIntervalSeconds;

        public int computedDamage;
        public float computedCooldown;
        public int computedShieldValue;
        public float computedBreakShieldRate;
        public float computedControlDuration;
    }
}
