using TalismanBag.V02.Config;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Rewards
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/CoreLoop/Idle Drop Config", fileName = "IdleDropConfig")]
    public sealed class IdleDropConfig : ScriptableObject
    {
        public string configId;
        public string displayName;
        public RewardDropTable normalStageDropTable;
        [Min(1)] public int rollsPerStageClear = 1;
        public bool grantOnNormalStageClear = true;
        public bool isDebugOnly;
        public bool isDeprecated;
        public CatalogSourceType sourceType = CatalogSourceType.Production;

        public string GetCatalogLabel()
        {
            string readableName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            string readableId = string.IsNullOrWhiteSpace(configId) ? "no_id" : configId.Trim();
            return $"{readableName} [{readableId}]";
        }

        public static IdleDropConfig LoadDefault()
        {
            return UnityEngine.Resources.Load<IdleDropConfig>("CoreLoop/IdleDropConfig");
        }
    }
}
