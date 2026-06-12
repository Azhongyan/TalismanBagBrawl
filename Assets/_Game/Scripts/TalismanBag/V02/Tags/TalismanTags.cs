namespace TalismanBag.V02.Tags
{
    public enum ElementTag
    {
        None,
        Fire,
        Thunder,
        Water,
        Wood,
        Metal,
        Earth,
        Sword,
        Soul
    }

    public enum FunctionTag
    {
        None,

        Damage,
        Burst,
        Burn,
        Chain,
        AoE,

        Shield,
        Heal,
        Cleanse,
        Defense,

        EnergySource,
        Enhance,

        ShieldBreak,
        AntiGhost,
        AntiSeal,
        AntiPoison
    }

    public enum CounterTag
    {
        None,

        Shield,
        Poison,
        Burn,
        Seal,
        Ghost,
        StealEnergy,
        Summon,
        Group,
        Charge,
        Boss
    }

    public enum EffectType
    {
        None,
        DealDamage,
        ApplyBurn,
        ChainDamage,
        GainShield,
        Heal,
        CleanseStatus,
        SuppressGhost,
        GenerateEnergy,
        EnhanceAdjacent
    }
}
