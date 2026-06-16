namespace TalismanBag.V02.Status
{
    public enum StatusEffectType
    {
        None,

        DamageOverTime,
        Shield,
        DamageBoost,
        DamageReduction,
        CooldownModifier,
        HealOverTime,

        CleanseReady,
        CounterReady,
        PassiveReady,
        PassiveTriggered,

        Seal,
        Stun,
        Silence,
        Slow,

        EnergyDisrupt,
        FormationDisrupt,
        EyeCoreDisabled,
        SpiritStoneDisabled,

        EnemyIntent,
        BossPhase,

        Vulnerable,
        ArmorBreak,
        ShieldBreak
    }

    public enum StatusPolarity
    {
        Neutral,
        Buff,
        Debuff,
        Intent,
        Passive
    }

    public enum StatusDisplayPriority
    {
        Critical = 0,
        High = 10,
        Medium = 20,
        Low = 30
    }
}
