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
        public MainTrialRuntimeLoadoutSnapshotData runtimeLoadoutSnapshot = new();

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

            runtimeLoadoutSnapshot ??= new MainTrialRuntimeLoadoutSnapshotData();
            runtimeLoadoutSnapshot.Normalize();
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

    [Serializable]
    public sealed class MainTrialRuntimeLoadoutSnapshotData
    {
        public int snapshotVersion = 1;
        public bool hasSnapshot;
        public string roundId;
        public List<MainTrialRuntimeTalismanSnapshotData> talismans = new();
        public List<string> appliedRewardIds = new();

        public void Normalize()
        {
            if (snapshotVersion <= 0)
            {
                snapshotVersion = 1;
            }

            roundId ??= string.Empty;
            talismans ??= new List<MainTrialRuntimeTalismanSnapshotData>();
            for (int i = talismans.Count - 1; i >= 0; i--)
            {
                if (talismans[i] == null)
                {
                    talismans.RemoveAt(i);
                    continue;
                }

                talismans[i].Normalize();
                if (string.IsNullOrEmpty(talismans[i].itemId))
                {
                    talismans.RemoveAt(i);
                }
            }

            appliedRewardIds ??= new List<string>();
            NormalizeStringList(appliedRewardIds);
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

    [Serializable]
    public sealed class MainTrialRuntimeTalismanSnapshotData
    {
        public string itemId;
        public int level = 1;
        public bool isPlaced;
        public int gridX = -1;
        public int gridY = -1;

        public void Normalize()
        {
            itemId = string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
            if (level <= 0)
            {
                level = 1;
            }

            if (!isPlaced)
            {
                gridX = -1;
                gridY = -1;
            }
        }
    }
}
