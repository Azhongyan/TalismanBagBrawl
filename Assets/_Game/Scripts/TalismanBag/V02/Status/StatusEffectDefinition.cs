using UnityEngine;

namespace TalismanBag.V02.Status
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/Status Effect")]
    public sealed class StatusEffectDefinition : ScriptableObject
    {
        public string statusId;
        public string displayName;

        [TextArea]
        public string description;

        public Sprite icon;
        public string glyph;
        public Color displayColor = Color.white;

        public StatusEffectType statusType;
        public StatusPolarity polarity;
        public StatusDisplayPriority displayPriority = StatusDisplayPriority.Medium;

        [Header("Stack")]
        public bool stackable;
        public int maxStacks = 1;

        [Header("Duration")]
        public bool hasDuration = true;
        public float defaultDuration = 3f;

        [Header("Display")]
        public bool isVisible = true;
        public bool showOnAvatar = true;
        public bool showOnGrid;
        public bool showCountdown = true;
        public bool showStackCount = true;

        [Header("Rules")]
        public bool isDispellable = true;
        public bool isCounterable;
    }
}
