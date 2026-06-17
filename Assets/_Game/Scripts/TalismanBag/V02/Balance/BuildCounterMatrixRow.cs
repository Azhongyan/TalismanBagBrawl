using System;
using UnityEngine;

namespace TalismanBag.V02.Balance
{
    [Serializable]
    public class BuildCounterMatrixRow
    {
        public string buildId;
        public string enemyId;
        public CounterRelation relation = CounterRelation.Neutral;
        public float multiplier = 1f;

        [TextArea]
        public string note;
    }
}
