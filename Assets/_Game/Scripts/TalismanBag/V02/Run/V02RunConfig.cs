using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.V02.Run
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/Run Config", fileName = "V02RunConfig")]
    public sealed class V02RunConfig : ScriptableObject
    {
        public string runId;
        public string displayName;
        public List<V02RoundConfig> rounds = new();
    }
}
