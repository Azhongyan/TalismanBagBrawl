using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.Feedback;
using TalismanBag.V02.UI;
using UnityEngine;

namespace TalismanBag.V02.EnemySkills
{
    public sealed class EnemySkillController : MonoBehaviour
    {
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private V02EnemyIntentUI intentUI;

        private EnemyRuntime enemyRuntime;

        public void Initialize(EnemyRuntime enemy)
        {
            enemyRuntime = enemy;
            if (enemyRuntime == null)
            {
                intentUI?.ClearIntent();
                return;
            }

            enemyRuntime.currentIntentText = string.IsNullOrWhiteSpace(enemyRuntime.definition?.intentText)
                ? "普通攻击"
                : enemyRuntime.definition.intentText;
            enemyRuntime.isCastingSkill = false;
            enemyRuntime.currentCastingSkill = null;
            intentUI?.ShowIntent(enemyRuntime.currentIntentText, 0f);
        }

        public void Tick(float deltaTime)
        {
            if (enemyRuntime == null || enemyRuntime.skillRuntimes == null || enemyRuntime.skillRuntimes.Count == 0)
            {
                intentUI?.ShowIntent("普通攻击", 0f);
                return;
            }

            if (enemyRuntime.currentCastingSkill != null && enemyRuntime.currentCastingSkill.isCasting)
            {
                UpdateCasting(deltaTime);
                return;
            }

            foreach (EnemySkillRuntime skill in enemyRuntime.skillRuntimes)
            {
                if (skill?.definition == null)
                {
                    continue;
                }

                skill.cooldownTimer -= deltaTime;
                if (skill.cooldownTimer <= 0f)
                {
                    StartCasting(skill);
                    return;
                }
            }

            intentUI?.ShowIntent(GetIdleIntentText(), 0f);
        }

        public void ForceFirstSkill()
        {
            if (enemyRuntime == null || enemyRuntime.skillRuntimes == null || enemyRuntime.skillRuntimes.Count == 0)
            {
                EmitLog("[敌技] 当前敌人没有可强制释放的技能");
                return;
            }

            StartCasting(enemyRuntime.skillRuntimes[0]);
        }

        private void UpdateCasting(float deltaTime)
        {
            EnemySkillRuntime skill = enemyRuntime.currentCastingSkill;
            skill.castTimer -= deltaTime;
            enemyRuntime.currentIntentText = GetSkillIntent(skill.definition);
            intentUI?.ShowIntent(enemyRuntime.currentIntentText, Mathf.Max(0f, skill.castTimer));
            if (skill.castTimer <= 0f)
            {
                CompleteCasting(skill);
            }
        }

        private void StartCasting(EnemySkillRuntime skill)
        {
            if (skill?.definition == null || enemyRuntime == null)
            {
                return;
            }

            skill.isCasting = true;
            skill.castTimer = Mathf.Max(0f, skill.definition.castTime);
            enemyRuntime.isCastingSkill = true;
            enemyRuntime.currentCastingSkill = skill;
            enemyRuntime.currentIntentText = GetSkillIntent(skill.definition);
            intentUI?.ShowIntent(enemyRuntime.currentIntentText, skill.castTimer);
            EmitLog($"[敌技] {enemyRuntime.definition.GetReadableLabel()} 准备释放：{skill.definition.displayName}");

            if (skill.castTimer <= 0f)
            {
                CompleteCasting(skill);
            }
        }

        private void CompleteCasting(EnemySkillRuntime skill)
        {
            if (skill?.definition == null || enemyRuntime == null)
            {
                return;
            }

            ResolveSkillEffect(skill.definition);
            skill.isCasting = false;
            skill.castTimer = 0f;
            skill.cooldownTimer = Mathf.Max(0.1f, skill.definition.cooldown);
            enemyRuntime.isCastingSkill = false;
            enemyRuntime.currentCastingSkill = null;
            enemyRuntime.currentIntentText = GetIdleIntentText();
            intentUI?.ShowIntent(enemyRuntime.currentIntentText, 0f);
        }

        private void ResolveSkillEffect(EnemySkillDefinition skill)
        {
            switch (skill.skillType)
            {
                case EnemySkillType.GainShield:
                case EnemySkillType.BossPhaseShield:
                    combatController?.V02AddEnemyShield(Mathf.Max(0, skill.value), $"[敌技] {enemyRuntime.definition.GetReadableLabel()} 获得 {skill.value} 点护盾");
                    break;
                case EnemySkillType.SummonMinions:
                case EnemySkillType.BossPhaseSummon:
                    combatController?.V02DealDamageToPlayer(Mathf.Max(0, skill.value), $"[敌技] {enemyRuntime.definition.GetReadableLabel()} 发动群体压制，造成 {skill.value} 点伤害");
                    break;
                case EnemySkillType.ApplyPoison:
                    combatController?.V02AddPlayerStatus(Mathf.Max(0, skill.value), Mathf.Max(0, skill.value), $"[敌技] {enemyRuntime.definition.GetReadableLabel()} 施加毒火，毒 {skill.value}，燃烧 {skill.value}");
                    break;
                case EnemySkillType.ApplyBurn:
                    combatController?.V02AddPlayerStatus(0, Mathf.Max(0, skill.value), $"[敌技] {enemyRuntime.definition.GetReadableLabel()} 施加燃烧 {skill.value}");
                    break;
                case EnemySkillType.StealEnergy:
                    if (combatController != null && combatController.V02TryCounterStealEnergy(skill))
                    {
                        break;
                    }

                    combatController?.V02ApplyEnergyDisruption(Mathf.Max(0, skill.duration), $"[敌技] {enemyRuntime.definition.GetReadableLabel()} 偷取灵气，阵法供能短暂紊乱");
                    break;
                case EnemySkillType.SealRowOrColumn:
                case EnemySkillType.BossPhaseSealEye:
                    combatController?.V02SealRandomRowOrColumn(Mathf.Max(0, skill.duration), $"[敌技] {enemyRuntime.definition.GetReadableLabel()} 封印了一行或一列符箓");
                    break;
            }
        }

        private string GetIdleIntentText()
        {
            return string.IsNullOrWhiteSpace(enemyRuntime?.definition?.intentText)
                ? "普通攻击"
                : enemyRuntime.definition.intentText;
        }

        private static string GetSkillIntent(EnemySkillDefinition skill)
        {
            if (skill == null)
            {
                return "普通攻击";
            }

            return string.IsNullOrWhiteSpace(skill.intentText) ? skill.displayName : skill.intentText;
        }

        private void EmitLog(string message)
        {
            if (eventRouter != null)
            {
                eventRouter.Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, message);
            }
        }
    }
}
