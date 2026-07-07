using System;
using System.Collections.Generic;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleResultSnapshot
    {
        public string resultId = string.Empty;
        public string requestId = string.Empty;
        public BattleResultType resultType = BattleResultType.Unknown;
        public bool win;
        public bool lose;
        public bool abandon;
        public string roundId = string.Empty;
        public bool bossDefeated;
        public float durationSeconds;
        public string chapterProgressDelta = string.Empty;
        public List<string> rewardPreview = new();
        public List<string> itemDrops = new();
        public string buildPerformanceSummary = string.Empty;
        public List<string> eventSummary = new();
        public string rewardClaimToken = string.Empty;
        public string nextRouteHint = string.Empty;
        public bool devOnly = true;
        public bool shouldWriteSave;
        public bool shouldGrantReward;
    }
}
