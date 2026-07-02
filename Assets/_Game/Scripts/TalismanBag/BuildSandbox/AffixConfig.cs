using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/Affix Config",
        fileName = "AffixConfig")]
    public sealed class AffixConfig : BuildSandboxDevOnlyConfig
    {
        public string affixId = "buildsandbox_affix";
        public string displayName = "BuildSandbox Affix";
        public string affixGroup = "general";
        public List<string> requiredTags = new();
        public List<string> allowedRarities = new();
        public int minRoll = 1;
        public int maxRoll = 1;
        public string previewResultToken = "affix_preview";
        [TextArea] public string previewSummary = "BuildSandbox affix preview only. No combat modifier is emitted.";
    }
}
