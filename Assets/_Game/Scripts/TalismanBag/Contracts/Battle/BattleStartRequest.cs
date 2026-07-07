using System;
using System.Collections.Generic;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleStartRequest
    {
        public string requestId = string.Empty;
        public string chapterId = string.Empty;
        public string stageId = string.Empty;
        public string roundId = string.Empty;
        public bool isBossStage;
        public string enemyProfileId = string.Empty;
        public string bossProfileId = string.Empty;
        public BattleEntrySource entrySource = BattleEntrySource.Unknown;
        public string allowedItemRosterId = string.Empty;
        public bool formalFlow;
        public bool devOnly = true;

        public string sourceRoute = string.Empty;
        public string sourceScene = string.Empty;
        public string sourceController = string.Empty;
        public string seedId = string.Empty;
        public List<string> validationWarnings = new();
    }
}
