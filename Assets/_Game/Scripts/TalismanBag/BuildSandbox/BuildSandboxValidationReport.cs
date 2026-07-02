using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public sealed class BuildSandboxValidationReport
    {
        private readonly List<BuildSandboxValidationIssue> issues = new();

        public BuildSandboxValidationReport(string name)
        {
            Name = name ?? string.Empty;
        }

        public string Name { get; }
        public IReadOnlyList<BuildSandboxValidationIssue> Issues => issues;
        public bool Passed => ErrorCount == 0;
        public int ErrorCount => issues.Count(issue => issue.Level == BuildSandboxValidationLevel.Error);
        public int WarningCount => issues.Count(issue => issue.Level == BuildSandboxValidationLevel.Warning);
        public int InfoCount => issues.Count(issue => issue.Level == BuildSandboxValidationLevel.Info);

        public void Add(
            BuildSandboxValidationLevel level,
            string code,
            string message,
            string assetPath = "")
        {
            issues.Add(new BuildSandboxValidationIssue(level, code, message, assetPath));
        }

        public void AddInfo(string code, string message, string assetPath = "")
        {
            Add(BuildSandboxValidationLevel.Info, code, message, assetPath);
        }

        public void AddWarning(string code, string message, string assetPath = "")
        {
            Add(BuildSandboxValidationLevel.Warning, code, message, assetPath);
        }

        public void AddError(string code, string message, string assetPath = "")
        {
            Add(BuildSandboxValidationLevel.Error, code, message, assetPath);
        }
    }
}
