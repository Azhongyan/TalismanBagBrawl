using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.V02.Balance
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/Counter Multiplier Config", fileName = "V02CounterMultiplierConfig")]
    public sealed class V02CounterMultiplierConfig : ScriptableObject
    {
        public float strongCounterMultiplier = 1.8f;
        public float lightCounterMultiplier = 1.35f;
        public float neutralMultiplier = 1f;
        public float resistedMultiplier = 0.7f;
        public float hardResistedMultiplier = 0.55f;

        [Header("V0.2 Specific Hooks")]
        public float rewardShieldBreakMultiplier = 2.5f;
        public float groupClearMultiplier = 1.6f;

        [Header("Build Counter Matrix")]
        public List<BuildCounterMatrixRow> buildCounterMatrix = new();

        public float GetMultiplier(CounterRelation relation)
        {
            return relation switch
            {
                CounterRelation.StrongCounter => strongCounterMultiplier,
                CounterRelation.LightCounter => lightCounterMultiplier,
                CounterRelation.Resisted => resistedMultiplier,
                CounterRelation.HardResisted => hardResistedMultiplier,
                CounterRelation.Neutral => neutralMultiplier,
                _ => neutralMultiplier
            };
        }

        public bool TryGetBuildCounterMatrixRow(string buildId, string enemyId, out BuildCounterMatrixRow row)
        {
            row = null;

            if (string.IsNullOrEmpty(buildId) || string.IsNullOrEmpty(enemyId) || buildCounterMatrix == null)
            {
                return false;
            }

            for (int i = 0; i < buildCounterMatrix.Count; i++)
            {
                BuildCounterMatrixRow current = buildCounterMatrix[i];
                if (current == null)
                {
                    continue;
                }

                if (current.buildId == buildId && current.enemyId == enemyId)
                {
                    row = current;
                    return true;
                }
            }

            return false;
        }
    }
}
