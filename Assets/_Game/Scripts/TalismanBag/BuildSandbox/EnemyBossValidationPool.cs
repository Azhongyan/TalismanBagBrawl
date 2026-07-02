using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildSandboxEnemyProfile
    {
        public string enemyId = string.Empty;
        public string chineseRole = string.Empty;
        public string enemyType = string.Empty;
        public List<string> validationTargetBuilds = new();
        public List<string> validationTags = new();
        public List<string> recommendedSynergies = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public bool referencesFormalEnemyPool;
        public bool referencesFormalBossPool;
        public string notes = "devOnly enemy validation profile; disabled; not connected to formal flow.";
    }

    [Serializable]
    public sealed class BuildSandboxBossProfile
    {
        public string bossId = string.Empty;
        public string chineseRole = string.Empty;
        public string bossMechanic = string.Empty;
        public List<string> validationTargetBuilds = new();
        public List<string> validationTags = new();
        public List<string> recommendedSynergies = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool simulatorReadable = true;
        public bool entersFormalFlow;
        public bool referencesFormalEnemyPool;
        public bool referencesFormalBossPool;
        public string notes = "devOnly boss validation profile; disabled; not connected to formal flow.";
    }

    [Serializable]
    public sealed class EnemyBossValidationPool
    {
        public string packageName = "V0.3/V0.4-BuildSandbox-EnemyBossValidationPool01";
        public bool devOnly = true;
        public bool isEnabled;
        public List<BuildSandboxEnemyProfile> enemies = new();
        public List<BuildSandboxBossProfile> bosses = new();

        public static EnemyBossValidationPool CreateDefault()
        {
            return new EnemyBossValidationPool
            {
                enemies = CreateDefaultEnemies().ToList(),
                bosses = CreateDefaultBosses().ToList()
            };
        }

        public BuildSandboxEnemyProfile FindEnemy(string enemyId)
        {
            return enemies.FirstOrDefault(enemy =>
                string.Equals(enemy.enemyId, enemyId, StringComparison.Ordinal));
        }

        public BuildSandboxBossProfile FindBoss(string bossId)
        {
            return bosses.FirstOrDefault(boss =>
                string.Equals(boss.bossId, bossId, StringComparison.Ordinal));
        }

        private static IEnumerable<BuildSandboxEnemyProfile> CreateDefaultEnemies()
        {
            yield return Enemy(
                "dev_enemy_basic",
                "普通怪",
                "basic",
                new[] { "baseline_build" },
                new[] { "basic", "baseline" },
                new[] { "惊雷", "离火" },
                "Baseline body used to compare Build deltas.");

            yield return Enemy(
                "dev_enemy_shield_guard",
                "护盾怪",
                "shield_guard",
                new[] { "jing_lei_break", "hu_zhen_guard" },
                new[] { "shield", "break_test" },
                new[] { "惊雷", "护阵" },
                "Checks shield break and guard response.");

            yield return Enemy(
                "dev_enemy_poison_cultist",
                "毒怪",
                "poison",
                new[] { "jing_e_cleanse" },
                new[] { "poison", "negative_status" },
                new[] { "净厄" },
                "Checks cleanse and negative status handling.");

            yield return Enemy(
                "dev_enemy_burning_wisp",
                "燃烧怪",
                "burning",
                new[] { "li_huo_damage" },
                new[] { "burning", "dot_pressure" },
                new[] { "离火", "净厄" },
                "Checks fire pressure and cleanse overlap.");

            yield return Enemy(
                "dev_enemy_spirit_thief",
                "偷灵怪",
                "spirit_thief",
                new[] { "ju_neng_energy" },
                new[] { "energy_drain", "resource_disrupt" },
                new[] { "聚能" },
                "Checks energy steal and energy coverage.");

            yield return Enemy(
                "dev_enemy_seal_locker",
                "封符怪",
                "seal_lock",
                new[] { "zhen_hun_control", "jing_e_cleanse" },
                new[] { "seal_lock", "control" },
                new[] { "镇魂", "净厄" },
                "Checks seal lock and control resistance.");

            yield return Enemy(
                "dev_enemy_swarm_pack",
                "群怪",
                "swarm",
                new[] { "li_huo_aoe" },
                new[] { "swarm", "multi_target" },
                new[] { "离火" },
                "Checks multi-target pressure.");

            yield return Enemy(
                "dev_enemy_burst_assassin",
                "高爆发怪",
                "burst",
                new[] { "hu_zhen_survive" },
                new[] { "burst", "low_hp_pressure" },
                new[] { "护阵" },
                "Checks high burst survival.");

            yield return Enemy(
                "dev_enemy_caster_chanter",
                "施法怪",
                "caster",
                new[] { "zhen_hun_interrupt" },
                new[] { "caster", "skill_cast" },
                new[] { "镇魂" },
                "Checks cast interruption and control timing.");

            yield return Enemy(
                "dev_enemy_thick_blood",
                "厚血怪",
                "high_hp",
                new[] { "li_huo_damage", "jing_lei_break" },
                new[] { "high_hp", "long_fight" },
                new[] { "离火", "惊雷" },
                "Checks long-fight damage scaling.");

            yield return Enemy(
                "dev_enemy_formation_eye_jammer",
                "阵眼干扰怪",
                "formation_eye_disrupt",
                new[] { "ju_neng_energy", "hu_zhen_guard" },
                new[] { "formation_eye", "placement_disrupt" },
                new[] { "聚能", "护阵" },
                "Checks formation-eye and placement disruption.");
        }

        private static IEnumerable<BuildSandboxBossProfile> CreateDefaultBosses()
        {
            yield return Boss(
                "dev_boss_shield_jinglei",
                "护盾Boss：验证惊雷",
                "shield_boss",
                new[] { "jing_lei_break" },
                new[] { "shield", "boss", "break_test" },
                new[] { "惊雷" },
                "Boss shell focused on shield-break validation.");

            yield return Boss(
                "dev_boss_swarm_lihuo",
                "群怪Boss：验证离火",
                "swarm_boss",
                new[] { "li_huo_aoe" },
                new[] { "swarm", "boss", "multi_target" },
                new[] { "离火" },
                "Boss shell focused on multi-target damage validation.");

            yield return Boss(
                "dev_boss_burst_huzhen",
                "高爆发Boss：验证护阵",
                "burst_boss",
                new[] { "hu_zhen_survive" },
                new[] { "burst", "boss", "low_hp_pressure" },
                new[] { "护阵" },
                "Boss shell focused on high burst mitigation.");

            yield return Boss(
                "dev_boss_debuff_jinge",
                "负面状态Boss：验证净厄",
                "debuff_boss",
                new[] { "jing_e_cleanse" },
                new[] { "negative_status", "boss", "cleanse" },
                new[] { "净厄" },
                "Boss shell focused on cleanse validation.");

            yield return Boss(
                "dev_boss_caster_zhenhun",
                "施法Boss：验证镇魂",
                "caster_boss",
                new[] { "zhen_hun_control" },
                new[] { "caster", "boss", "skill_cast" },
                new[] { "镇魂" },
                "Boss shell focused on control and cast interruption.");

            yield return Boss(
                "dev_boss_energy_juneng",
                "供能干扰Boss：验证聚能",
                "energy_jammer_boss",
                new[] { "ju_neng_energy" },
                new[] { "energy_drain", "boss", "resource_disrupt" },
                new[] { "聚能" },
                "Boss shell focused on energy disruption.");

            yield return Boss(
                "dev_boss_hybrid_combo",
                "混合机制Boss：验证组合Build",
                "hybrid_combo_boss",
                new[] { "combo_build" },
                new[] { "hybrid", "boss", "combo" },
                new[] { "惊雷", "离火", "护阵", "净厄", "镇魂", "聚能" },
                "Boss shell focused on mixed Build combinations.");
        }

        private static BuildSandboxEnemyProfile Enemy(
            string enemyId,
            string chineseRole,
            string enemyType,
            IEnumerable<string> targetBuilds,
            IEnumerable<string> tags,
            IEnumerable<string> synergies,
            string notes)
        {
            return new BuildSandboxEnemyProfile
            {
                enemyId = enemyId,
                chineseRole = chineseRole,
                enemyType = enemyType,
                validationTargetBuilds = targetBuilds.ToList(),
                validationTags = tags.ToList(),
                recommendedSynergies = synergies.ToList(),
                devOnly = true,
                isEnabled = false,
                simulatorReadable = true,
                entersFormalFlow = false,
                referencesFormalEnemyPool = false,
                referencesFormalBossPool = false,
                notes = notes
            };
        }

        private static BuildSandboxBossProfile Boss(
            string bossId,
            string chineseRole,
            string mechanic,
            IEnumerable<string> targetBuilds,
            IEnumerable<string> tags,
            IEnumerable<string> synergies,
            string notes)
        {
            return new BuildSandboxBossProfile
            {
                bossId = bossId,
                chineseRole = chineseRole,
                bossMechanic = mechanic,
                validationTargetBuilds = targetBuilds.ToList(),
                validationTags = tags.ToList(),
                recommendedSynergies = synergies.ToList(),
                devOnly = true,
                isEnabled = false,
                simulatorReadable = true,
                entersFormalFlow = false,
                referencesFormalEnemyPool = false,
                referencesFormalBossPool = false,
                notes = notes
            };
        }
    }
}
