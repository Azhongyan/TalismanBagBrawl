using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class DevChapterProfile
    {
        public string chapterId = string.Empty;
        public string chineseRole = string.Empty;
        public List<string> validationTargetBuilds = new();
        public string enemyId = string.Empty;
        public string bossId = string.Empty;
        public List<string> validationTags = new();
        public List<string> recommendedSynergies = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public bool usesFormalStageData;
        public bool usesFormalFlowHook;
        public bool appearsInFormalEntrance;
        public bool usesProductReward;
        public bool usesProductDrop;
        public string notes = "devOnly chapter validation profile; disabled; simulator readable only.";
    }

    [Serializable]
    public sealed class DevChapterContentPool
    {
        public string packageName = "V0.3/V0.4-BuildSandbox-DevChapterContentPool01";
        public bool devOnly = true;
        public bool isEnabled;
        public List<DevChapterProfile> chapters = new();

        public static DevChapterContentPool CreateDefault()
        {
            return new DevChapterContentPool
            {
                chapters = CreateDefaultChapters().ToList()
            };
        }

        public DevChapterProfile FindChapter(string chapterId)
        {
            return chapters.FirstOrDefault(chapter =>
                string.Equals(chapter.chapterId, chapterId, StringComparison.Ordinal));
        }

        public IReadOnlyList<BuildSimulationScenario> CreateSimulationScenarios(
            EnemyBossValidationPool enemyBossPool = null,
            BuildSandboxLayoutSnapshot layoutSnapshot = null,
            BuildEvaluationResult buildEvaluation = null,
            AffixRarityEvaluationResult affixEvaluation = null,
            CombatModifierBundle modifierBundle = null,
            EffectEventBundle eventBundle = null)
        {
            EnemyBossValidationPool pool = enemyBossPool ?? EnemyBossValidationPool.CreateDefault();
            return chapters
                .Where(chapter => chapter != null)
                .Select((chapter, index) => CreateScenario(
                    chapter,
                    pool,
                    layoutSnapshot,
                    buildEvaluation,
                    affixEvaluation,
                    modifierBundle,
                    eventBundle,
                    index + 1))
                .ToList();
        }

        private static IEnumerable<DevChapterProfile> CreateDefaultChapters()
        {
            foreach (DevChapterProfile profile in BuildSeries(
                         "DevBuildTest_Thunder",
                         "惊雷开发测试",
                         "jing_lei_break",
                         new[] { "惊雷" },
                         new[]
                         {
                             "dev_enemy_shield_guard",
                             "dev_enemy_thick_blood",
                             "dev_enemy_basic",
                             "dev_enemy_formation_eye_jammer",
                             "dev_enemy_caster_chanter",
                             "dev_enemy_swarm_pack",
                             "dev_enemy_burst_assassin",
                             "dev_enemy_seal_locker",
                             "dev_enemy_spirit_thief",
                             "dev_enemy_burning_wisp"
                         },
                         new[] { "thunder", "shield_break", "build_test" }))
            {
                yield return profile;
            }

            foreach (DevChapterProfile profile in BuildSeries(
                         "DevBuildTest_Fire",
                         "离火开发测试",
                         "li_huo_aoe",
                         new[] { "离火" },
                         new[]
                         {
                             "dev_enemy_swarm_pack",
                             "dev_enemy_thick_blood",
                             "dev_enemy_burning_wisp",
                             "dev_enemy_basic",
                             "dev_enemy_poison_cultist",
                             "dev_enemy_shield_guard",
                             "dev_enemy_caster_chanter",
                             "dev_enemy_burst_assassin",
                             "dev_enemy_formation_eye_jammer",
                             "dev_enemy_seal_locker"
                         },
                         new[] { "fire", "aoe", "damage_build" }))
            {
                yield return profile;
            }

            foreach (DevChapterProfile profile in BuildSeries(
                         "DevBuildTest_Shield",
                         "护阵开发测试",
                         "hu_zhen_survive",
                         new[] { "护阵" },
                         new[]
                         {
                             "dev_enemy_burst_assassin",
                             "dev_enemy_shield_guard",
                             "dev_enemy_formation_eye_jammer",
                             "dev_enemy_thick_blood",
                             "dev_enemy_basic",
                             "dev_enemy_caster_chanter",
                             "dev_enemy_swarm_pack",
                             "dev_enemy_spirit_thief",
                             "dev_enemy_poison_cultist",
                             "dev_enemy_burning_wisp"
                         },
                         new[] { "shield", "survival", "guard_build" }))
            {
                yield return profile;
            }

            foreach (DevChapterProfile profile in BuildSeries(
                         "DevBuildTest_Cleanse",
                         "净厄开发测试",
                         "jing_e_cleanse",
                         new[] { "净厄" },
                         new[]
                         {
                             "dev_enemy_poison_cultist",
                             "dev_enemy_burning_wisp",
                             "dev_enemy_seal_locker",
                             "dev_enemy_basic",
                             "dev_enemy_caster_chanter",
                             "dev_enemy_spirit_thief",
                             "dev_enemy_swarm_pack",
                             "dev_enemy_shield_guard",
                             "dev_enemy_formation_eye_jammer",
                             "dev_enemy_thick_blood"
                         },
                         new[] { "cleanse", "negative_status", "purify_build" }))
            {
                yield return profile;
            }

            foreach (DevChapterProfile profile in BuildSeries(
                         "DevBuildTest_Control",
                         "镇魂开发测试",
                         "zhen_hun_control",
                         new[] { "镇魂" },
                         new[]
                         {
                             "dev_enemy_caster_chanter",
                             "dev_enemy_seal_locker",
                             "dev_enemy_swarm_pack",
                             "dev_enemy_burst_assassin",
                             "dev_enemy_basic",
                             "dev_enemy_formation_eye_jammer",
                             "dev_enemy_spirit_thief",
                             "dev_enemy_poison_cultist",
                             "dev_enemy_burning_wisp",
                             "dev_enemy_thick_blood"
                         },
                         new[] { "control", "interrupt", "zhenhun_build" }))
            {
                yield return profile;
            }

            foreach (DevChapterProfile profile in BuildSeries(
                         "DevBuildTest_Energy",
                         "聚能开发测试",
                         "ju_neng_energy",
                         new[] { "聚能" },
                         new[]
                         {
                             "dev_enemy_spirit_thief",
                             "dev_enemy_formation_eye_jammer",
                             "dev_enemy_basic",
                             "dev_enemy_caster_chanter",
                             "dev_enemy_shield_guard",
                             "dev_enemy_burst_assassin",
                             "dev_enemy_swarm_pack",
                             "dev_enemy_thick_blood",
                             "dev_enemy_poison_cultist",
                             "dev_enemy_seal_locker"
                         },
                         new[] { "energy", "resource", "juneng_build" }))
            {
                yield return profile;
            }

            string[] mixedBosses =
            {
                "dev_boss_hybrid_combo",
                "dev_boss_shield_jinglei",
                "dev_boss_swarm_lihuo",
                "dev_boss_energy_juneng",
                "dev_boss_debuff_jinge"
            };
            for (int i = 0; i < mixedBosses.Length; i++)
            {
                yield return CreateProfile(
                    $"DevBossTest_Mixed_{i + 1:00}",
                    $"混合Boss开发测试 {i + 1:00}",
                    new[] { "combo_build" },
                    string.Empty,
                    mixedBosses[i],
                    new[] { "mixed", "boss", "combo_build" },
                    new[] { "惊雷", "离火", "护阵", "净厄", "镇魂", "聚能" },
                    "Boss mixed validation profile; devOnly and disabled.");
            }
        }

        private static IEnumerable<DevChapterProfile> BuildSeries(
            string idPrefix,
            string rolePrefix,
            string targetBuild,
            IEnumerable<string> synergies,
            IReadOnlyList<string> enemyIds,
            IEnumerable<string> tags)
        {
            for (int i = 0; i < 10; i++)
            {
                yield return CreateProfile(
                    $"{idPrefix}_{i + 1:00}",
                    $"{rolePrefix} {i + 1:00}",
                    new[] { targetBuild },
                    enemyIds[i % enemyIds.Count],
                    string.Empty,
                    tags,
                    synergies,
                    "Build test chapter profile; devOnly and disabled.");
            }
        }

        private static DevChapterProfile CreateProfile(
            string chapterId,
            string chineseRole,
            IEnumerable<string> targetBuilds,
            string enemyId,
            string bossId,
            IEnumerable<string> tags,
            IEnumerable<string> synergies,
            string notes)
        {
            return new DevChapterProfile
            {
                chapterId = chapterId,
                chineseRole = chineseRole,
                validationTargetBuilds = Clean(targetBuilds),
                enemyId = enemyId ?? string.Empty,
                bossId = bossId ?? string.Empty,
                validationTags = Clean(tags),
                recommendedSynergies = Clean(synergies),
                devOnly = true,
                isEnabled = false,
                simulatorReadable = true,
                entersFormalFlow = false,
                usesFormalStageData = false,
                usesFormalFlowHook = false,
                appearsInFormalEntrance = false,
                usesProductReward = false,
                usesProductDrop = false,
                notes = notes
            };
        }

        private static BuildSimulationScenario CreateScenario(
            DevChapterProfile chapter,
            EnemyBossValidationPool pool,
            BuildSandboxLayoutSnapshot layoutSnapshot,
            BuildEvaluationResult buildEvaluation,
            AffixRarityEvaluationResult affixEvaluation,
            CombatModifierBundle modifierBundle,
            EffectEventBundle eventBundle,
            int seed)
        {
            BuildSandboxEnemyProfile enemy = string.IsNullOrWhiteSpace(chapter.enemyId)
                ? null
                : pool.FindEnemy(chapter.enemyId);
            BuildSandboxBossProfile boss = string.IsNullOrWhiteSpace(chapter.bossId)
                ? null
                : pool.FindBoss(chapter.bossId);

            List<string> tags = Merge(chapter.validationTags, enemy?.validationTags, boss?.validationTags);
            List<string> synergies = Merge(
                chapter.recommendedSynergies,
                enemy?.recommendedSynergies,
                boss?.recommendedSynergies);

            return new BuildSimulationScenario
            {
                buildId = chapter.chapterId,
                displayName = chapter.chineseRole,
                enemyType = enemy?.enemyType ?? "devOnly_chapter_shell",
                bossMechanic = boss?.bossMechanic ?? "none",
                enemyProfileId = enemy?.enemyId ?? string.Empty,
                enemyChineseRole = enemy?.chineseRole ?? string.Empty,
                bossProfileId = boss?.bossId ?? string.Empty,
                bossChineseRole = boss?.chineseRole ?? string.Empty,
                validationTags = tags,
                recommendedSynergies = synergies,
                buildItemIds = ResolveBuildSeedItems(chapter.validationTargetBuilds),
                itemRarity = "dev_chapter_mixed_preview",
                affixCombination = affixEvaluation?.affixIds?.ToList() ?? new List<string>(),
                energyCondition = "dev_chapter_powered_sample",
                placementRelation = FormatStrings(chapter.validationTargetBuilds),
                simulatorReadable = chapter.simulatorReadable
                    && (enemy == null || enemy.simulatorReadable)
                    && (boss == null || boss.simulatorReadable),
                entersFormalFlow = chapter.entersFormalFlow
                    || chapter.usesFormalStageData
                    || chapter.usesFormalFlowHook
                    || chapter.appearsInFormalEntrance
                    || chapter.usesProductReward
                    || chapter.usesProductDrop,
                devOnly = chapter.devOnly,
                isEnabled = chapter.isEnabled,
                batchSeed = seed,
                layoutSnapshot = layoutSnapshot ?? new BuildSandboxLayoutSnapshot(),
                buildEvaluation = buildEvaluation ?? new BuildEvaluationResult(),
                affixEvaluation = affixEvaluation ?? new AffixRarityEvaluationResult(),
                modifierBundle = modifierBundle ?? new CombatModifierBundle(),
                eventBundle = eventBundle ?? new EffectEventBundle(),
                notes = "devOnly chapter profile read by BuildSimulationRunner; not connected to product entry."
            };
        }

        private static List<string> ResolveBuildSeedItems(IEnumerable<string> targetBuilds)
        {
            HashSet<string> targets = new(targetBuilds ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            List<string> itemIds = new();
            if (targets.Contains("jing_lei_break"))
            {
                itemIds.Add("dev_seed_thunder_talisman");
            }

            if (targets.Contains("li_huo_aoe"))
            {
                itemIds.Add("dev_seed_fire_talisman");
            }

            if (targets.Contains("hu_zhen_survive"))
            {
                itemIds.Add("dev_seed_guardian_ward");
            }

            if (targets.Contains("jing_e_cleanse"))
            {
                itemIds.Add("dev_seed_cleanse_seal");
            }

            if (targets.Contains("zhen_hun_control"))
            {
                itemIds.Add("dev_seed_control_chime");
            }

            if (targets.Contains("ju_neng_energy"))
            {
                itemIds.Add("dev_seed_energy_lamp");
            }

            if (targets.Contains("combo_build"))
            {
                itemIds.AddRange(new[]
                {
                    "dev_seed_thunder_talisman",
                    "dev_seed_fire_talisman",
                    "dev_seed_guardian_ward",
                    "dev_seed_energy_lamp"
                });
            }

            if (itemIds.Count == 0)
            {
                itemIds.Add("dev_seed_buildsandbox_talisman");
            }

            return itemIds.Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToList();
        }

        private static List<string> Merge(params IEnumerable<string>[] sources)
        {
            return sources
                .Where(source => source != null)
                .SelectMany(source => source)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static List<string> Clean(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", Clean(values));
        }
    }
}
