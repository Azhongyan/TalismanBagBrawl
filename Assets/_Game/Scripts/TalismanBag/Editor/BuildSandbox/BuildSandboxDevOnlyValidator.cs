#if UNITY_EDITOR
using System.Collections.Generic;
using TalismanBag.BuildSandbox;
using UnityEditor;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxDevOnlyValidator
    {
        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("devOnly Isolation");
            IReadOnlyList<BuildSandboxDevOnlyConfig> configs =
                BuildSandboxConfigValidator.CollectConfigAssets();

            if (configs.Count == 0)
            {
                report.AddInfo(
                    "DEVONLY_NO_CONFIG_ASSETS",
                    "No devOnly config assets found. Nothing can leak into product flow.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return report;
            }

            foreach (BuildSandboxDevOnlyConfig config in configs)
            {
                string path = AssetDatabase.GetAssetPath(config);
                if (!config.devOnly)
                {
                    report.AddError("DEVONLY_FALSE", "BuildSandbox config must keep devOnly=true.", path);
                }

                if (config.isEnabled)
                {
                    report.AddError("IS_ENABLED_TRUE", "BuildSandbox config must keep isEnabled=false.", path);
                }

                if (config.devOnly && !config.isEnabled)
                {
                    report.AddInfo("DEVONLY_ISOLATED", "Config is devOnly and disabled.", path);
                }
            }

            return report;
        }
    }
}
#endif
