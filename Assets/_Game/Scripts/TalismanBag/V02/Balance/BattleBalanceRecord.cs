using System.Text;

namespace TalismanBag.V02.Balance
{
    public sealed class BattleBalanceRecord
    {
        public string battleId;
        public string levelId;
        public int levelIndex;
        public string intendedRole;
        public string enemyId;
        public string enemyClass;
        public string enemyArchetype;
        public int enemyMaxHp;
        public float enemyAttackDamage;
        public float enemyAttackInterval;
        public int enemySkillCastCount;
        public string playerBuildTags;
        public int poweredTalismanCount;
        public int unpoweredTalismanCount;
        public int playerTotalDamage;
        public int playerDamageTaken;
        public float battleDuration;
        public float playerHpRemainPercent;
        public float playerHpLossPercent;
        public string counterRelation;
        public bool playerWon;
        public int counterTriggerCount;
        public int shieldBreakCount;
        public int cleanseCount;
        public int soulSuppressCount;
        public int chainClearCount;
        public int unsealCount;
        public string result;
        public string reason;
        public string inferredFailureReason;

        public string ToLogString()
        {
            StringBuilder builder = new();
            builder.AppendLine($"[Balance] battle={battleId} levelId={levelId} levelIndex={levelIndex} role={intendedRole} enemy={enemyId} class={enemyClass} archetype={enemyArchetype}");
            builder.AppendLine($"[Balance] result={result} reason={reason} won={playerWon}");
            builder.AppendLine($"[Balance] hp={enemyMaxHp} atk={enemyAttackDamage:0.#} interval={enemyAttackInterval:0.##} skills={enemySkillCastCount} duration={battleDuration:0.0}s");
            builder.AppendLine($"[Balance] build={playerBuildTags} powered={poweredTalismanCount} unpowered={unpoweredTalismanCount} relation={counterRelation}");
            builder.AppendLine($"[Balance] damageDealt={playerTotalDamage} damageTaken={playerDamageTaken} hpRemain={playerHpRemainPercent:0.0%} hpLoss={playerHpLossPercent:0.0%}");
            builder.AppendLine($"[Balance] counters={counterTriggerCount} shieldBreak={shieldBreakCount} cleanse={cleanseCount} soul={soulSuppressCount} chain={chainClearCount} unseal={unsealCount}");
            builder.Append($"[Balance] failure={inferredFailureReason}");
            return builder.ToString();
        }
    }
}
