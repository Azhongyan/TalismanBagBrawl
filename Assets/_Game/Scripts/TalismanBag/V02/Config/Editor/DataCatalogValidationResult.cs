using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public sealed class DataCatalogValidationResult
    {
        public DataCatalogValidationResult(
            DataCatalogValidationLevel level,
            string code,
            string message,
            Object context = null,
            string assetPath = "")
        {
            Level = level;
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            Context = context;
            AssetPath = assetPath ?? string.Empty;
        }

        public DataCatalogValidationLevel Level { get; }
        public string Code { get; }
        public string Message { get; }
        public Object Context { get; }
        public string AssetPath { get; }

        public override string ToString()
        {
            string path = string.IsNullOrWhiteSpace(AssetPath) ? string.Empty : $" ({AssetPath})";
            return $"[{Level}] {Code}: {Message}{path}";
        }
    }
}
