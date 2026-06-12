using System.Collections.Generic;
using TalismanBag.Items;
using UnityEngine;

namespace TalismanBag.Run
{
    [CreateAssetMenu(menuName = "TalismanBag/Run Config", fileName = "RunConfig")]
    public sealed class RunConfig : ScriptableObject
    {
        public string runId;
        public string displayName;
        public List<RoundConfig> rounds = new();
        public int startingSpiritJade;
        public List<TalismanItemDefinition> startingItems = new();
    }
}
