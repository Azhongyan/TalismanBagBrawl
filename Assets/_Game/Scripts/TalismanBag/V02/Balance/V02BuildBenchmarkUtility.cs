using System.Collections.Generic;
using System.Text;
using TalismanBag.Enemies;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.V02.Balance
{
    public static class V02BuildBenchmarkUtility
    {
        public static string BuildEnemyReport(EnemyDefinition enemy, V02CounterMultiplierConfig multiplierConfig)
        {
            if (enemy == null)
            {
                return "[BuildBenchmark] Missing enemy.";
            }

            StringBuilder builder = new();
            builder.AppendLine($"[BuildBenchmark] Enemy={enemy.enemyId} name={enemy.displayName} class={enemy.enemyClass} archetype={enemy.enemyArchetype}");
            builder.AppendLine($"[BuildBenchmark] Stats hp={enemy.maxHp} atk={enemy.attackDamage}/{enemy.attackInterval:0.##}s targetCap={GetTargetDurationCap(enemy):0.#}s");
            foreach (TestBuild build in GetBuilds())
            {
                AppendBuildResult(builder, enemy, build, multiplierConfig);
            }

            return builder.ToString();
        }

        public static string BuildAllEnemiesReport(IEnumerable<EnemyDefinition> enemies, V02CounterMultiplierConfig multiplierConfig)
        {
            StringBuilder builder = new();
            builder.AppendLine("[BuildBenchmark] All Enemies");
            bool hasEnemy = false;
            if (enemies != null)
            {
                foreach (EnemyDefinition enemy in enemies)
                {
                    if (enemy == null)
                    {
                        continue;
                    }

                    hasEnemy = true;
                    builder.AppendLine();
                    builder.Append(BuildEnemyReport(enemy, multiplierConfig));
                }
            }

            if (!hasEnemy)
            {
                builder.AppendLine("[BuildBenchmark] No enemies found.");
            }

            return builder.ToString();
        }

        public static string BuildRouteMatrix(IEnumerable<EnemyDefinition> enemies, V02CounterMultiplierConfig multiplierConfig)
        {
            StringBuilder builder = new();
            builder.AppendLine("[BuildBenchmarkMatrix]");
            builder.Append("Enemy");
            foreach (TestBuild build in GetBuilds())
            {
                builder.Append($",{build.name}");
            }

            builder.AppendLine();

            bool hasEnemy = false;
            if (enemies != null)
            {
                foreach (EnemyDefinition enemy in enemies)
                {
                    if (enemy == null)
                    {
                        continue;
                    }

                    hasEnemy = true;
                    builder.Append(string.IsNullOrEmpty(enemy.enemyId) ? enemy.name : enemy.enemyId);
                    foreach (TestBuild build in GetBuilds())
                    {
                        BuildBenchmarkResult result = Evaluate(enemy, build, multiplierConfig);
                        builder.Append($",{result.grade}:{result.duration:0.#}s:{result.relation}");
                    }

                    builder.AppendLine();
                }
            }

            if (!hasEnemy)
            {
                builder.AppendLine("No enemies found.");
            }

            return builder.ToString();
        }

        public static BuildBenchmarkResult Evaluate(EnemyDefinition enemy, string buildName, V02CounterMultiplierConfig multiplierConfig)
        {
            foreach (TestBuild build in GetBuilds())
            {
                if (build.name == buildName)
                {
                    return Evaluate(enemy, build, multiplierConfig);
                }
            }

            return default;
        }

        private static void AppendBuildResult(StringBuilder builder, EnemyDefinition enemy, TestBuild build, V02CounterMultiplierConfig multiplierConfig)
        {
            BuildBenchmarkResult result = Evaluate(enemy, build, multiplierConfig);
            builder.AppendLine(
                $"[BuildBenchmark] {result.buildName}: relation={result.relation} mult={result.multiplier:0.##} dps={result.estimatedDps:0.0} duration={result.duration:0.0}s taken={result.damageTaken} grade={result.grade} note={result.note}");
        }

        private static BuildBenchmarkResult Evaluate(EnemyDefinition enemy, TestBuild build, V02CounterMultiplierConfig multiplierConfig)
        {
            if (enemy == null)
            {
                return default;
            }

            CounterRelation relation = ResolveRelation(build, enemy);
            float multiplier = GetMultiplier(relation, multiplierConfig);
            float dps = Mathf.Max(1f, build.baseDps * multiplier);
            float duration = enemy.maxHp / dps;
            int damageTaken = EstimateDamageTaken(enemy, duration, relation);
            float targetCap = GetTargetDurationCap(enemy);
            string grade = ResolveGrade(duration, damageTaken, targetCap, relation);
            string note = ResolveNote(enemy, build, relation, duration, damageTaken, targetCap);

            return new BuildBenchmarkResult
            {
                enemyId = enemy.enemyId,
                buildName = build.name,
                relation = relation,
                multiplier = multiplier,
                estimatedDps = dps,
                duration = duration,
                damageTaken = damageTaken,
                grade = grade,
                note = note
            };
        }

        private static float GetMultiplier(CounterRelation relation, V02CounterMultiplierConfig multiplierConfig)
        {
            if (multiplierConfig != null)
            {
                return multiplierConfig.GetMultiplier(relation);
            }

            return relation switch
            {
                CounterRelation.StrongCounter => 1.8f,
                CounterRelation.LightCounter => 1.35f,
                CounterRelation.Resisted => 0.7f,
                CounterRelation.HardResisted => 0.55f,
                _ => 1f
            };
        }

        private static CounterRelation ResolveRelation(TestBuild build, EnemyDefinition enemy)
        {
            if (!build.powered)
            {
                return CounterRelation.HardResisted;
            }

            bool countersWeakness = Intersects(build.counters, enemy.weaknessTags);
            bool vulnerableFunction = Intersects(build.functions, enemy.vulnerableFunctions);
            bool vulnerableElement = enemy.vulnerableElements != null && enemy.vulnerableElements.Contains(build.element);
            if (countersWeakness && (vulnerableFunction || vulnerableElement))
            {
                return CounterRelation.StrongCounter;
            }

            if (countersWeakness || vulnerableFunction || vulnerableElement)
            {
                return CounterRelation.LightCounter;
            }

            bool resistedFunction = Intersects(build.functions, enemy.resistedFunctions);
            bool resistedElement = enemy.resistedElements != null && enemy.resistedElements.Contains(build.element);
            if (resistedFunction && resistedElement)
            {
                return CounterRelation.HardResisted;
            }

            if (resistedFunction || resistedElement)
            {
                return CounterRelation.Resisted;
            }

            return CounterRelation.Neutral;
        }

        private static int EstimateDamageTaken(EnemyDefinition enemy, float duration, CounterRelation relation)
        {
            float attackInterval = Mathf.Max(0.5f, enemy.attackInterval);
            float damage = Mathf.Floor(duration / attackInterval) * enemy.attackDamage;
            damage += EstimateSkillPressure(enemy, duration, relation);
            return Mathf.RoundToInt(damage);
        }

        private static float EstimateSkillPressure(EnemyDefinition enemy, float duration, CounterRelation relation)
        {
            float mitigation = relation == CounterRelation.StrongCounter ? 0.45f :
                relation == CounterRelation.LightCounter ? 0.7f :
                relation == CounterRelation.Resisted || relation == CounterRelation.HardResisted ? 1.25f : 1f;

            return enemy.enemyId switch
            {
                "turtle_guardian_shield" => Mathf.Floor(duration / 10f) * 6f * mitigation,
                "imp_swarm" => Mathf.Floor(duration / 7f) * 12f * mitigation,
                "red_poison_beast" => Mathf.Floor(duration / 7f) * 9f * mitigation,
                "energy_thief_ghost" => Mathf.Floor(duration / 7f) * 8f * mitigation,
                "seal_talisman_taoist" => Mathf.Floor(duration / 8f) * 10f * mitigation,
                "formation_breaker_boss" => (Mathf.Floor(duration / 6f) * 12f + Mathf.Floor(duration / 7f) * 8f) * mitigation,
                _ => 0f
            };
        }

        private static float GetTargetDurationCap(EnemyDefinition enemy)
        {
            return enemy.enemyId switch
            {
                "mountain_imp_basic" => 60f,
                "turtle_guardian_shield" => 100f,
                "imp_swarm" => 110f,
                "red_poison_beast" => 120f,
                "energy_thief_ghost" => 120f,
                "seal_talisman_taoist" => 130f,
                "formation_breaker_boss" => 180f,
                _ => 120f
            };
        }

        private static string ResolveGrade(float duration, int damageTaken, float targetCap, CounterRelation relation)
        {
            if (duration <= targetCap * 0.65f && damageTaken < 60 && relation == CounterRelation.StrongCounter)
            {
                return "Good";
            }

            if (duration <= targetCap && damageTaken < 100 && relation != CounterRelation.HardResisted)
            {
                return "Pass";
            }

            if (duration <= targetCap * 1.25f && damageTaken < 130)
            {
                return "Weak";
            }

            return "Fail";
        }

        private static string ResolveNote(EnemyDefinition enemy, TestBuild build, CounterRelation relation, float duration, int damageTaken, float targetCap)
        {
            if (!build.powered)
            {
                return "Unpowered control build";
            }

            if (relation == CounterRelation.StrongCounter)
            {
                return "Expected counter route";
            }

            if (duration > targetCap)
            {
                return "Duration above target";
            }

            if (damageTaken >= 100)
            {
                return "Player pressure high";
            }

            return "Reference";
        }

        private static List<TestBuild> GetBuilds()
        {
            return new List<TestBuild>
            {
                new("FireBasicBuild", ElementTag.Fire, 8f, true, new[] { FunctionTag.Damage, FunctionTag.Burn }, new CounterTag[0]),
                new("ThunderShieldBreakBuild", ElementTag.Thunder, 11f, true, new[] { FunctionTag.Damage, FunctionTag.ShieldBreak }, new[] { CounterTag.Shield }),
                new("ChainGroupClearBuild", ElementTag.Thunder, 10f, true, new[] { FunctionTag.Damage, FunctionTag.Chain, FunctionTag.AoE }, new[] { CounterTag.Group, CounterTag.Summon }),
                new("PurifyAntiPoisonBuild", ElementTag.Water, 6f, true, new[] { FunctionTag.Cleanse, FunctionTag.AntiPoison, FunctionTag.Shield }, new[] { CounterTag.Poison, CounterTag.Burn }),
                new("SoulAntiStealBuild", ElementTag.Soul, 8f, true, new[] { FunctionTag.Damage, FunctionTag.AntiGhost }, new[] { CounterTag.Ghost, CounterTag.StealEnergy }),
                new("SpreadAntiSealBuild", ElementTag.Metal, 7f, true, new[] { FunctionTag.Damage, FunctionTag.Cleanse, FunctionTag.AntiSeal }, new[] { CounterTag.Seal }),
                new("BossReadyBuild", ElementTag.Thunder, 13f, true, new[] { FunctionTag.Damage, FunctionTag.ShieldBreak, FunctionTag.Chain, FunctionTag.Cleanse, FunctionTag.AntiGhost, FunctionTag.AntiSeal }, new[] { CounterTag.Shield, CounterTag.Group, CounterTag.Summon, CounterTag.Seal, CounterTag.Ghost, CounterTag.StealEnergy, CounterTag.Boss }),
                new("BadUnpoweredBuild", ElementTag.Fire, 3f, false, new[] { FunctionTag.Damage, FunctionTag.Burn }, new CounterTag[0])
            };
        }

        private static bool Intersects<T>(IEnumerable<T> left, ICollection<T> right)
        {
            if (left == null || right == null || right.Count == 0)
            {
                return false;
            }

            foreach (T value in left)
            {
                if (right.Contains(value))
                {
                    return true;
                }
            }

            return false;
        }

        public struct BuildBenchmarkResult
        {
            public string enemyId;
            public string buildName;
            public CounterRelation relation;
            public float multiplier;
            public float estimatedDps;
            public float duration;
            public int damageTaken;
            public string grade;
            public string note;
        }

        private readonly struct TestBuild
        {
            public readonly string name;
            public readonly ElementTag element;
            public readonly float baseDps;
            public readonly bool powered;
            public readonly FunctionTag[] functions;
            public readonly CounterTag[] counters;

            public TestBuild(string name, ElementTag element, float baseDps, bool powered, FunctionTag[] functions, CounterTag[] counters)
            {
                this.name = name;
                this.element = element;
                this.baseDps = baseDps;
                this.powered = powered;
                this.functions = functions;
                this.counters = counters;
            }
        }
    }
}
