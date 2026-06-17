using System.Collections.Generic;
using TalismanBag.Enemies;
using UnityEngine;

namespace TalismanBag.V02.Balance
{
    public sealed class V02BuildTestRunner : MonoBehaviour
    {
        [SerializeField] private List<EnemyDefinition> testEnemies = new();
        [SerializeField] private V02CounterMultiplierConfig multiplierConfig;

        public void RunShieldEnemyTest()
        {
            RunBalanceTest("turtle_guardian_shield");
        }

        public void RunGroupEnemyTest()
        {
            RunBalanceTest("imp_swarm");
        }

        public void RunPoisonEnemyTest()
        {
            RunBalanceTest("red_poison_beast");
        }

        public void RunStealEnemyTest()
        {
            RunBalanceTest("energy_thief_ghost");
        }

        public void RunSealEnemyTest()
        {
            RunBalanceTest("seal_talisman_taoist");
        }

        public void RunBossTest()
        {
            RunBalanceTest("formation_breaker_boss");
        }

        public void RunBalanceTest(string enemyId)
        {
            EnemyDefinition enemy = FindEnemy(enemyId);
            if (enemy == null)
            {
                Debug.LogWarning($"[BalanceTest] Missing enemy: {enemyId}");
                return;
            }

            Debug.Log(V02BuildBenchmarkUtility.BuildEnemyReport(enemy, multiplierConfig));
        }

        private EnemyDefinition FindEnemy(string enemyId)
        {
            if (testEnemies == null)
            {
                return null;
            }

            foreach (EnemyDefinition enemy in testEnemies)
            {
                if (enemy != null && enemy.enemyId == enemyId)
                {
                    return enemy;
                }
            }

            return null;
        }
    }
}
