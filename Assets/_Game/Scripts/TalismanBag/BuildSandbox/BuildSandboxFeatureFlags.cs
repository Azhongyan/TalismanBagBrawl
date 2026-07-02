namespace TalismanBag.BuildSandbox
{
    public readonly struct BuildSandboxFeatureFlagDefinition
    {
        public BuildSandboxFeatureFlagDefinition(string key, bool defaultValue, string scope)
        {
            Key = key;
            DefaultValue = defaultValue;
            Scope = scope;
        }

        public string Key { get; }
        public bool DefaultValue { get; }
        public string Scope { get; }
    }

    public static class BuildSandboxFeatureFlags
    {
        public const bool EnableSynergyBuild = false;
        public const bool EnableAffixSystem = false;
        public const bool EnableDevBuildContent = false;
        public const bool EnableBuildModifierInCombat = false;
        public const bool EnableBuildDebugReport = false;
        public const bool EnableItemShapeOccupancy = false;
        public const bool EnableShapePlacementSandbox = false;
        public const bool EnableShapeRotation = false;

        public static readonly BuildSandboxFeatureFlagDefinition[] All =
        {
            new(nameof(EnableSynergyBuild), EnableSynergyBuild, "Synergy build sandbox"),
            new(nameof(EnableAffixSystem), EnableAffixSystem, "Affix and rarity sandbox"),
            new(nameof(EnableDevBuildContent), EnableDevBuildContent, "devOnly content pools"),
            new(nameof(EnableBuildModifierInCombat), EnableBuildModifierInCombat, "Combat modifier bridge"),
            new(nameof(EnableBuildDebugReport), EnableBuildDebugReport, "Debug report export"),
            new(nameof(EnableItemShapeOccupancy), EnableItemShapeOccupancy, "Item shape occupancy"),
            new(nameof(EnableShapePlacementSandbox), EnableShapePlacementSandbox, "Shape placement sandbox"),
            new(nameof(EnableShapeRotation), EnableShapeRotation, "Shape rotation sandbox")
        };

        public static bool AreAllDefaultsDisabled()
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].DefaultValue)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
