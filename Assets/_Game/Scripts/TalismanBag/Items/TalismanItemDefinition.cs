using System.Collections.Generic;
using TalismanBag.V02.Config;
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

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [CreateAssetMenu(menuName = "Talisman Bag/Item Definition", fileName = "TalismanItemDefinition")]
    public sealed class TalismanItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public bool enabled = true;
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

        [Header("Stage Config Catalog")]
        public string iconKey;
        public ItemRarity rarity;
        public bool canDrop = true;
        public bool canUpgrade;
        public int unlockChapter = 1;
        public string effectId;
        public List<string> effectParams = new();
        public List<string> elementTags = new();
        public List<string> mechanicTags = new();
        public List<string> schoolTags = new();
        public List<string> factionTags = new();
        public List<string> synergyTags = new();
        public List<string> comboTags = new();
        public bool isDebugOnly;
        public bool isDeprecated;
        public CatalogSourceType sourceType = CatalogSourceType.Unknown;

        public string GetCatalogLabel()
        {
            string readableName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            string readableId = string.IsNullOrWhiteSpace(itemId) ? "no_id" : itemId.Trim();
            return $"{readableName} [{readableId}]";
        }
    }
}
