namespace TalismanBag.V02.EnemySkills
{
    public sealed class EnemySkillRuntime
    {
        public EnemySkillDefinition definition;
        public float cooldownTimer;
        public float castTimer;
        public bool isCasting;

        public EnemySkillRuntime(EnemySkillDefinition definition)
        {
            this.definition = definition;
            cooldownTimer = definition != null ? definition.initialDelay : 0f;
            castTimer = 0f;
            isCasting = false;
        }
    }
}
