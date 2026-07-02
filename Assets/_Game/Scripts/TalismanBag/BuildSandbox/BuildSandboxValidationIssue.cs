namespace TalismanBag.BuildSandbox
{
    public sealed class BuildSandboxValidationIssue
    {
        public BuildSandboxValidationIssue(
            BuildSandboxValidationLevel level,
            string code,
            string message,
            string assetPath = "")
        {
            Level = level;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            AssetPath = assetPath ?? string.Empty;
        }

        public BuildSandboxValidationLevel Level { get; }
        public string Code { get; }
        public string Message { get; }
        public string AssetPath { get; }

        public override string ToString()
        {
            string path = string.IsNullOrWhiteSpace(AssetPath) ? string.Empty : $" ({AssetPath})";
            return $"[{Level}] {Code}: {Message}{path}";
        }
    }
}
