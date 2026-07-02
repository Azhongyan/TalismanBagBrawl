using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/Item Tag Config",
        fileName = "ItemTagConfig")]
    public sealed class ItemTagConfig : BuildSandboxDevOnlyConfig
    {
        public string itemId = "buildsandbox_item";
        public List<string> tags = new();
        public string baseFunction = "dev_preview";
        public int energyCost;
        public string itemCategory = "dev";
        public List<string> rarityAllowed = new();
    }
}
