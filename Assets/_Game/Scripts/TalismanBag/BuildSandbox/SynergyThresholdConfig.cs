using System;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class SynergyThresholdConfig
    {
        public int pieceCount = 2;
        public string tierLabel = "2-piece";
        public string effectToken = "dev_preview";
        [TextArea] public string effectSummary = "BuildSandbox preview only. No combat modifier is emitted.";
        public bool devOnly = true;
        public bool isEnabled;
    }
}
