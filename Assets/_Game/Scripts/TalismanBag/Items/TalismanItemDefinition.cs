using System.Collections.Generic;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.Items
{
    public enum TalismanItemType
    {
        SpiritStone,
        AttackTalisman,
        ShieldTalisman,
        Pill,
        PassiveTool,
        SupportTalisman
    }

    public enum ElementType
    {
        None,
        Fire,
        Water,
        Wood,
        Metal,
        Earth,
        Thunder
    }

    [CreateAssetMenu(menuName = "Talisman Bag/Item Definition", fileName = "TalismanItemDefinition")]
    public sealed class TalismanItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public TalismanItemType itemType;
        public ElementType elementType;
        public Sprite icon;
        public int width = 1;
        public int height = 1;
        public float baseCooldown = 1f;
        public int manaCost;
        public int baseValue;
        public Color uiColor = Color.white;
        [TextArea] public string description;

        [Header("V0.2 Tags")]
        public ElementTag elementTag = ElementTag.None;
        public List<FunctionTag> functionTags = new();
        public List<CounterTag> counterTags = new();
        public EffectType effectType = EffectType.None;

        [Header("V0.2 Formation")]
        public int energyRequired = 1;
        public bool requiresFormationPower = true;

        [Header("V0.2 Description")]
        [TextArea] public string shortRoleDescription;
        [TextArea] public string counterDescription;
    }
}
