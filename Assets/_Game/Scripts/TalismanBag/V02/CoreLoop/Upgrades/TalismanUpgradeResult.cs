namespace TalismanBag.V02.CoreLoop.Upgrades
{
    public sealed class TalismanUpgradeResult
    {
        public bool success;
        public string itemId;
        public int fromLevel;
        public int toLevel;
        public string message;
        public TalismanLevelConfig levelConfig;

        public static TalismanUpgradeResult Failed(string itemId, int currentLevel, string message)
        {
            return new TalismanUpgradeResult
            {
                success = false,
                itemId = itemId,
                fromLevel = currentLevel,
                toLevel = currentLevel,
                message = message
            };
        }

        public static TalismanUpgradeResult Succeeded(string itemId, int fromLevel, int toLevel, TalismanLevelConfig config)
        {
            return new TalismanUpgradeResult
            {
                success = true,
                itemId = itemId,
                fromLevel = fromLevel,
                toLevel = toLevel,
                levelConfig = config,
                message = "培养成功"
            };
        }
    }
}
