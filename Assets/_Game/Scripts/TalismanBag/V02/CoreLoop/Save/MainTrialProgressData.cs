using System;
using System.Collections.Generic;

namespace TalismanBag.V02.CoreLoop.Save
{
    public enum MainTrialPhase
    {
        NotStarted,
        Chapter1InProgress,
        Chapter1BossCleared,
        Chapter1RewardClaimed,
        FirstUpgradeRequired,
        FirstUpgradeDone,
        Chapter2InProgress,
        Chapter2BossReady,
        Chapter2BossInProgress,
        Chapter2Cleared,
        CoreLoopComplete
    }

    [Serializable]
    public sealed class MainTrialProgressData
    {
        public MainTrialPhase mainTrialPhase = MainTrialPhase.NotStarted;
        public bool mainTrialPhaseInitialized;
        public string currentRoundId;
        public string highestClearedLevelId;
        public bool chapterOneBossCleared;
        public bool chapterOneBossRewardClaimed;
        public List<string> chapterOneFixedRewardClaimedLevelIds = new();
        public List<string> tutorialGuideOnceFlags = new();
        public bool firstUpgradeCompleted;
        public bool chapterOneCultivationCompleted;
        public string firstCultivatedTalismanId;
        public string currentMainTrialLevelId;
        public bool chapterTwoUnlocked;
        public int chapterTwoCurrentRoundNumber = 1;
        public bool chapterTwoBossUnlocked;
        public bool chapterTwoBossCleared;
        public bool chapterTwoBossDefeated;
        public bool coreLoopCompleted;

        public void Normalize()
        {
            currentRoundId ??= string.Empty;
            highestClearedLevelId ??= string.Empty;
            chapterOneFixedRewardClaimedLevelIds ??= new List<string>();
            tutorialGuideOnceFlags ??= new List<string>();
            NormalizeStringList(chapterOneFixedRewardClaimedLevelIds);
            NormalizeStringList(tutorialGuideOnceFlags);

            firstCultivatedTalismanId ??= string.Empty;
            currentMainTrialLevelId ??= string.Empty;
            if (chapterTwoCurrentRoundNumber <= 0)
            {
                chapterTwoCurrentRoundNumber = 1;
            }

        }

        private static void NormalizeStringList(List<string> values)
        {
            for (int i = values.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(values[i]))
                {
                    values.RemoveAt(i);
                    continue;
                }

                values[i] = values[i].Trim();
            }
        }
    }
}
