using System;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Save;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Tutorial
{
    public sealed class TutorialGuideService : MonoBehaviour
    {
        [SerializeField] private TutorialGuideConfig guideConfig;
        [SerializeField] private RewardService rewardService;
        [SerializeField] private SaveService saveService;

        public static TutorialGuideService Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public static TutorialGuideService GetOrCreate()
        {
            if (Instance != null)
            {
                return Instance;
            }

            TutorialGuideService existing = FindObjectOfType<TutorialGuideService>(true);
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            GameObject serviceObject = new("CoreLoopTutorialGuideService_Runtime");
            TutorialGuideService service = serviceObject.AddComponent<TutorialGuideService>();
            Instance = service;
            return service;
        }

        public void Bind(TutorialGuideConfig config, RewardService rewards, SaveService saves)
        {
            if (config != null)
            {
                guideConfig = config;
            }

            if (rewards != null)
            {
                rewardService = rewards;
            }

            if (saves != null)
            {
                saveService = saves;
            }
        }

        public TutorialGuideRow FindRow(string levelId, TutorialGuideTrigger trigger)
        {
            TutorialGuideConfig config = EnsureConfig();
            return config != null ? config.FindRow(levelId, trigger) : null;
        }

        public bool IsCompleted(TutorialGuideRow row)
        {
            if (row == null || !row.consumeOnce)
            {
                return false;
            }

            string onceFlag = row.GetSafeOnceFlag();
            if (string.IsNullOrEmpty(onceFlag))
            {
                return false;
            }

            SaveData saveData = EnsureSaveService().EnsureLoaded();
            MainTrialProgressData progress = saveData.mainTrialProgressData;
            progress?.Normalize();
            if (progress == null)
            {
                return false;
            }

            if (progress.tutorialGuideOnceFlags != null &&
                progress.tutorialGuideOnceFlags.Contains(onceFlag))
            {
                return true;
            }

            return IsLegacyCompleted(row, progress);
        }

        public RewardResult GrantReward(TutorialGuideRow row)
        {
            if (row == null || row.rewardConfig == null)
            {
                return null;
            }

            return EnsureRewardService().GrantConfig(row.rewardConfig);
        }

        public void MarkCompleted(TutorialGuideRow row)
        {
            if (row == null || !row.consumeOnce)
            {
                return;
            }

            string onceFlag = row.GetSafeOnceFlag();
            if (string.IsNullOrEmpty(onceFlag))
            {
                return;
            }

            SaveService saves = EnsureSaveService();
            SaveData saveData = saves.EnsureLoaded();
            saveData.mainTrialProgressData ??= new MainTrialProgressData();
            MainTrialProgressData progress = saveData.mainTrialProgressData;
            progress.Normalize();

            if (!progress.tutorialGuideOnceFlags.Contains(onceFlag))
            {
                progress.tutorialGuideOnceFlags.Add(onceFlag);
            }

            MarkLegacyCompatibility(row, progress);
            saves.Save();
        }

        public bool AreRewardItemsInSave(TutorialGuideRow row)
        {
            if (row?.rewardConfig?.rewards == null)
            {
                return true;
            }

            SaveData saveData = EnsureSaveService().EnsureLoaded();
            foreach (RewardEntry reward in row.rewardConfig.rewards)
            {
                if (reward == null || reward.rewardType != RewardType.Item || string.IsNullOrWhiteSpace(reward.itemId))
                {
                    continue;
                }

                if (saveData.itemInventoryData == null || !saveData.itemInventoryData.HasItem(reward.itemId))
                {
                    return false;
                }
            }

            return true;
        }

        private TutorialGuideConfig EnsureConfig()
        {
            if (guideConfig != null)
            {
                return guideConfig;
            }

            guideConfig = UnityEngine.Resources.Load<TutorialGuideConfig>("CoreLoop/TutorialGuideConfig_Fix03");
            return guideConfig;
        }

        private RewardService EnsureRewardService()
        {
            if (rewardService != null)
            {
                return rewardService;
            }

            rewardService = FindObjectOfType<RewardService>(true);
            if (rewardService != null)
            {
                return rewardService;
            }

            GameObject serviceObject = new("CoreLoopRewardService_Runtime");
            rewardService = serviceObject.AddComponent<RewardService>();
            return rewardService;
        }

        private SaveService EnsureSaveService()
        {
            saveService ??= SaveService.GetOrCreate();
            return saveService;
        }

        private static bool IsLegacyCompleted(TutorialGuideRow row, MainTrialProgressData progress)
        {
            if (row.markChapterOneBossClear && progress.chapterOneBossRewardClaimed)
            {
                return true;
            }

            string levelId = row.GetSafeLevelId();
            return row.trigger == TutorialGuideTrigger.RoundWin &&
                   row.HasReward &&
                   progress.chapterOneFixedRewardClaimedLevelIds != null &&
                   progress.chapterOneFixedRewardClaimedLevelIds.Contains(levelId);
        }

        private static void MarkLegacyCompatibility(TutorialGuideRow row, MainTrialProgressData progress)
        {
            if (row.trigger == TutorialGuideTrigger.RoundWin && row.HasReward)
            {
                string levelId = row.GetSafeLevelId();
                if (!string.IsNullOrEmpty(levelId) &&
                    !progress.chapterOneFixedRewardClaimedLevelIds.Contains(levelId))
                {
                    progress.chapterOneFixedRewardClaimedLevelIds.Add(levelId);
                }
            }

            // Main-trial completion fields are written only by MainTrialFlowService.
        }
    }
}
