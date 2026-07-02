using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/Dev Only Config",
        fileName = "BuildSandboxDevOnlyConfig")]
    public class BuildSandboxDevOnlyConfig : ScriptableObject
    {
        public string configId = "buildsandbox_dev_only";
        public bool devOnly = true;
        public bool isEnabled;
        public List<string> referencedSystemKeys = new();
        [TextArea] public string notes =
            "BuildSandbox config. Keep devOnly=true and isEnabled=false until a future Guard assignment opens it.";
    }
}
