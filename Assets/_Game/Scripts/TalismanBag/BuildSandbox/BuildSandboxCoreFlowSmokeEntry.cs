namespace TalismanBag.BuildSandbox
{
    public static class BuildSandboxCoreFlowSmokeEntry
    {
        public static BuildSandboxValidationReport ValidatePlaceholder()
        {
            BuildSandboxValidationReport report = new("CoreFlow Smoke Placeholder");

            if (BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddInfo(
                    "CORE_FLOW_PLACEHOLDER_ONLY",
                    "Placeholder smoke entry is isolated. It does not load scenes, mutate saves, or enter product flow.");
            }
            else
            {
                report.AddError(
                    "CORE_FLOW_FLAGS_ENABLED",
                    "CoreFlow smoke placeholder requires every BuildSandbox feature flag to default false.");
            }

            return report;
        }
    }
}
