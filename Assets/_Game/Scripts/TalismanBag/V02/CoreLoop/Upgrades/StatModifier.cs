using System;

namespace TalismanBag.V02.CoreLoop.Upgrades
{
    [Serializable]
    public sealed class StatModifier
    {
        public float damageMultiplier = 1f;
        public float cooldownMultiplier = 1f;
        public float shieldMultiplier = 1f;
        public float breakShieldMultiplier = 1f;
        public float controlDurationMultiplier = 1f;
        public string summary;
    }
}
