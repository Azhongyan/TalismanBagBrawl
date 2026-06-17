using System;
using System.Collections.Generic;
using System.Text;
using TalismanBag.Enemies;
using TalismanBag.V02.Run;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.V02.Balance
{
    public static class V02BuildBenchmarkUtility
    {
        private const string RelationSourceMatrix = "Matrix";
        private const string RelationSourceFallbackTagLogic = "FallbackTagLogic";
        private const string TargetSourceBenchmarkTarget = "BenchmarkTarget";
        private const string TargetSourceRoundBenchmarkRule = "RoundBenchmarkRule";
        private const string TargetSourceFallbackMetadata = "FallbackMetadata";

        public static string BuildEnemyReport(EnemyDefinition enemy, V02CounterMultiplierConfig multiplierConfig)
        {
            return BuildEnemyReport(null, enemy, multiplierConfig);
        }

        public static string BuildEnemyReport(V02RoundConfig roundConfig, V02CounterMultiplierConfig multiplierConfig)
        {
            return BuildEnemyReport(roundConfig, roundConfig?.enemy, multiplierConfig);
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
                    builder.Append(BuildEnemyReport(null, enemy, multiplierConfig));
                }
            }

            if (!hasEnemy)
            {
                builder.AppendLine("[BuildBenchmark] No enemies found.");
            }

            return builder.ToString();
        }

        public static string BuildAllRoundsReport(V02RunConfig runConfig, V02CounterMultiplierConfig multiplierConfig)
        {
            StringBuilder builder = new();
            builder.AppendLine("[BuildBenchmark] All Rounds");
            bool hasRound = false;
            if (runConfig?.rounds != null)
            {
                foreach (V02RoundConfig round in runConfig.rounds)
                {
                    if (round?.enemy == null)
                    {
                        continue;
                    }

                    hasRound = true;
                    builder.AppendLine();
                    builder.Append(BuildEnemyReport(round, multiplierConfig));
                }
            }

            if (!hasRound)
            {
                builder.AppendLine("[BuildBenchmark] No rounds found.");
            }

            return builder.ToString();
        }

        public static string BuildRouteMatrix(IEnumerable<EnemyDefinition> enemies, V02CounterMultiplierConfig multiplierConfig)
        {
            StringBuilder builder = CreateRouteMatrixHeader();
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
                    AppendRouteMatrixRow(builder, null, enemy, multiplierConfig);
                }
            }

            if (!hasEnemy)
            {
                builder.AppendLine("No enemies found.");
            }

            return builder.ToString();
        }

        public static string BuildRouteMatrix(V02RunConfig runConfig, V02CounterMultiplierConfig multiplierConfig)
        {
            StringBuilder builder = CreateRouteMatrixHeader();
            bool hasRound = false;
            if (runConfig?.rounds != null)
            {
                foreach (V02RoundConfig round in runConfig.rounds)
                {
                    if (round?.enemy == null)
                    {
                        continue;
                    }

                    hasRound = true;
                    AppendRouteMatrixRow(builder, round, round.enemy, multiplierConfig);
                }
            }

            if (!hasRound)
            {
                builder.AppendLine("No rounds found.");
            }

            return builder.ToString();
        }

        public static string BuildBenchmarkTargetReport(V02RunConfig runConfig, V02CounterMultiplierConfig multiplierConfig)
        {
            StringBuilder builder = new();
            builder.AppendLine("[BuildBenchmarkTargetReport]");
            builder.AppendLine("levelId,enemyId,buildId,relation,multiplier,relationSource,actualDuration,grade,expectedGrade,expectedDurationMin,expectedDurationMax,targetSource,resultMatch,actualHpLoss,passHpLossMax,weakHpLossMax,passDurationMax,weakDurationMax,gradeReason");

            bool hasTarget = false;
            if (runConfig?.rounds != null)
            {
                foreach (V02RoundConfig round in runConfig.rounds)
                {
                    if (round?.enemy == null || round.benchmarkTargets == null)
                    {
                        continue;
                    }

                    foreach (V02BuildBenchmarkTargetRow target in round.benchmarkTargets)
                    {
                        if (target == null || string.IsNullOrWhiteSpace(target.buildId))
                        {
                            continue;
                        }

                        BuildBenchmarkResult result = Evaluate(round, target.buildId, multiplierConfig);
                        hasTarget = true;
                        builder.AppendLine(
                            $"{result.levelId},{result.enemyId},{result.buildName},{result.relation},{result.multiplier:0.##},{result.relationSource},{result.duration:0.0},{result.grade},{result.expectedGrade},{result.expectedDurationMin:0.#},{result.expectedDurationMax:0.#},{result.targetSource},{result.resultMatch},{result.actualHpLoss:0.##},{result.passHpLossMax:0.##},{result.weakHpLossMax:0.##},{result.passDurationMax:0.#},{result.weakDurationMax:0.#},{result.gradeReason}");
                    }
                }
            }

            if (!hasTarget)
            {
                builder.AppendLine("No benchmarkTargets found.");
            }

            return builder.ToString();
        }

        public static BuildBenchmarkResult Evaluate(EnemyDefinition enemy, string buildName, V02CounterMultiplierConfig multiplierConfig)
        {
            foreach (TestBuild build in GetBuilds())
            {
                if (build.name == buildName)
                {
                    return Evaluate(null, enemy, build, multiplierConfig);
                }
            }

            return default;
        }

        public static BuildBenchmarkResult Evaluate(V02RoundConfig roundConfig, string buildName, V02CounterMultiplierConfig multiplierConfig)
        {
            foreach (TestBuild build in GetBuilds())
            {
                if (build.name == buildName)
                {
                    return Evaluate(roundConfig, roundConfig?.enemy, build, multiplierConfig);
                }
            }

            return default;
        }

        private static string BuildEnemyReport(V02RoundConfig roundConfig, EnemyDefinition enemy, V02CounterMultiplierConfig multiplierConfig)
        {
            if (enemy == null)
            {
                return "[BuildBenchmark] Missing enemy.";
            }

            StringBuilder builder = new();
            BenchmarkLevelMetadata metadata = ResolveLevelMetadata(roundConfig, enemy);
            float targetCap = ResolveTargetDurationCap(roundConfig, enemy);
            builder.AppendLine("[BuildBenchmark]");
            builder.AppendLine($"LevelId: {metadata.levelId}");
            builder.AppendLine($"LevelIndex: {metadata.levelIndex}");
            builder.AppendLine($"Role: {metadata.intendedRole}");
            builder.AppendLine($"Enemy: {enemy.enemyId}");
            builder.AppendLine($"EnemyClass: {enemy.enemyClass}");
            builder.AppendLine($"EnemyArchetype: {enemy.enemyArchetype}");
            builder.AppendLine($"TargetSource: {(roundConfig != null ? TargetSourceRoundBenchmarkRule : TargetSourceFallbackMetadata)}");
            builder.AppendLine($"[BuildBenchmark] Stats hp={enemy.maxHp} atk={enemy.attackDamage}/{enemy.attackInterval:0.##}s targetCap={targetCap:0.#}s");
            foreach (TestBuild build in GetBuilds())
            {
                AppendBuildResult(builder, roundConfig, enemy, build, multiplierConfig);
            }

            return builder.ToString();
        }

        private static StringBuilder CreateRouteMatrixHeader()
        {
            StringBuilder builder = new();
            builder.AppendLine("[BuildBenchmarkMatrix]");
            builder.Append("levelId,levelIndex,intendedRole,enemyId");
            foreach (TestBuild build in GetBuilds())
            {
                builder.Append($",{build.name}");
            }

            builder.AppendLine();
            return builder;
        }

        private static void AppendRouteMatrixRow(StringBuilder builder, V02RoundConfig roundConfig, EnemyDefinition enemy, V02CounterMultiplierConfig multiplierConfig)
        {
            BenchmarkLevelMetadata metadata = ResolveLevelMetadata(roundConfig, enemy);
            builder.Append($"{metadata.levelId},{metadata.levelIndex},{metadata.intendedRole},{(string.IsNullOrEmpty(enemy.enemyId) ? enemy.name : enemy.enemyId)}");
            foreach (TestBuild build in GetBuilds())
            {
                BuildBenchmarkResult result = Evaluate(roundConfig, enemy, build, multiplierConfig);
                builder.Append($",{result.grade}:{result.duration:0.#}s:{result.relation}:{result.relationSource}:{result.targetSource}:{result.resultMatch}");
            }

            builder.AppendLine();
        }

        private static void AppendBuildResult(StringBuilder builder, V02RoundConfig roundConfig, EnemyDefinition enemy, TestBuild build, V02CounterMultiplierConfig multiplierConfig)
        {
            BuildBenchmarkResult result = Evaluate(roundConfig, enemy, build, multiplierConfig);
            builder.AppendLine(
                $"[BuildBenchmark] Build={result.buildName} Enemy={result.enemyId} Relation={result.relation} Multiplier={result.multiplier:0.##} RelationSource={result.relationSource} Duration={result.duration:0.0}s HpLossEstimate={result.damageTaken}% CounterTriggerCount={result.expectedCounterTriggerCount} Result={result.grade} ExpectedGrade={result.expectedGrade} ExpectedDuration={FormatDurationWindow(result.expectedDurationMin, result.expectedDurationMax)} TargetSource={result.targetSource} ResultMatch={result.resultMatch} Reason={result.note} Dps={result.estimatedDps:0.0}");
        }

        private static BuildBenchmarkResult Evaluate(V02RoundConfig roundConfig, EnemyDefinition enemy, TestBuild build, V02CounterMultiplierConfig multiplierConfig)
        {
            if (enemy == null)
            {
                return default;
            }

            bool hasMatrixRow = TryGetMatrixRow(build, enemy, multiplierConfig, out BuildCounterMatrixRow matrixRow);
            CounterRelation relation = hasMatrixRow ? matrixRow.relation : ResolveRelation(build, enemy);
            float multiplier = hasMatrixRow ? matrixRow.multiplier : GetMultiplier(relation, multiplierConfig);
            float dps = Mathf.Max(1f, build.baseDps * multiplier);
            float duration = enemy.maxHp / dps;
            int damageTaken = EstimateDamageTaken(enemy, duration, relation);
            float targetCap = ResolveTargetDurationCap(roundConfig, enemy);
            BenchmarkPassFailRule rule = roundConfig != null ? ResolveBenchmarkRule(roundConfig, enemy) : null;
            float actualHpLoss = damageTaken / 100f;
            string grade = roundConfig != null
                ? ResolveGrade(duration, damageTaken, rule)
                : ResolveGrade(duration, damageTaken, targetCap, relation);
            string gradeReason = rule != null
                ? ResolveGradeReason(duration, actualHpLoss, rule)
                : "FallbackMetadataRule";
            string note = ResolveNote(enemy, build, relation, duration, damageTaken, targetCap);
            BenchmarkLevelMetadata metadata = ResolveLevelMetadata(roundConfig, enemy);
            bool hasTarget = TryGetBenchmarkTarget(roundConfig, build.name, out V02BuildBenchmarkTargetRow target);
            float expectedDurationMin = hasTarget ? target.expectedDurationMin : 0f;
            float expectedDurationMax = hasTarget ? target.expectedDurationMax : 0f;
            string expectedGrade = hasTarget ? target.expectedGrade : string.Empty;

            return new BuildBenchmarkResult
            {
                levelId = metadata.levelId,
                levelIndex = metadata.levelIndex,
                intendedRole = metadata.intendedRole,
                enemyId = enemy.enemyId,
                buildName = build.name,
                relation = relation,
                multiplier = multiplier,
                relationSource = hasMatrixRow ? RelationSourceMatrix : RelationSourceFallbackTagLogic,
                estimatedDps = dps,
                duration = duration,
                damageTaken = damageTaken,
                grade = grade,
                note = note,
                expectedCounterTriggerCount = relation == CounterRelation.StrongCounter || relation == CounterRelation.LightCounter ? 1 : 0,
                expectedGrade = expectedGrade,
                expectedDurationMin = expectedDurationMin,
                expectedDurationMax = expectedDurationMax,
                targetSource = hasTarget
                    ? TargetSourceBenchmarkTarget
                    : roundConfig != null ? TargetSourceRoundBenchmarkRule : TargetSourceFallbackMetadata,
                resultMatch = hasTarget && ResultMatchesTarget(grade, duration, target),
                actualHpLoss = actualHpLoss,
                passHpLossMax = rule != null ? rule.passHpLossMax : 1f,
                weakHpLossMax = rule != null ? rule.weakHpLossMax : 1f,
                passDurationMax = rule != null ? rule.passDurationMax : targetCap,
                weakDurationMax = rule != null ? rule.weakDurationMax : targetCap * 1.25f,
                gradeReason = gradeReason
            };
        }

        private static bool TryGetMatrixRow(TestBuild build, EnemyDefinition enemy, V02CounterMultiplierConfig multiplierConfig, out BuildCounterMatrixRow row)
        {
            row = null;

            if (multiplierConfig == null || enemy == null)
            {
                return false;
            }

            return multiplierConfig.TryGetBuildCounterMatrixRow(build.name, enemy.enemyId, out row);
        }

        private static bool TryGetBenchmarkTarget(V02RoundConfig roundConfig, string buildId, out V02BuildBenchmarkTargetRow target)
        {
            target = null;
            if (roundConfig?.benchmarkTargets == null || string.IsNullOrWhiteSpace(buildId))
            {
                return false;
            }

            foreach (V02BuildBenchmarkTargetRow row in roundConfig.benchmarkTargets)
            {
                if (row != null && string.Equals(row.buildId, buildId, StringComparison.Ordinal))
                {
                    target = row;
                    return true;
                }
            }

            return false;
        }

        private static bool ResultMatchesTarget(string grade, float duration, V02BuildBenchmarkTargetRow target)
        {
            if (target == null || !GradeMatches(grade, target.expectedGrade))
            {
                return false;
            }

            float min = Mathf.Max(0f, target.expectedDurationMin);
            float max = target.expectedDurationMax > 0f ? target.expectedDurationMax : float.PositiveInfinity;
            return duration >= min && duration <= max;
        }

        private static bool GradeMatches(string grade, string expectedGrade)
        {
            if (string.IsNullOrWhiteSpace(grade) || string.IsNullOrWhiteSpace(expectedGrade))
            {
                return false;
            }

            string[] tokens = expectedGrade.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawToken in tokens)
            {
                string token = rawToken.Trim();
                if (string.Equals(grade, token, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(token, "Good", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(grade, "Pass", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string FormatDurationWindow(float min, float max)
        {
            return max > 0f ? $"{min:0.#}-{max:0.#}s" : "None";
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
                "formation_breaker_elite" => (Mathf.Floor(duration / 10f) * 6f + Mathf.Floor(duration / 8f) * 10f) * mitigation,
                "shield_swarm_trial" => (Mathf.Floor(duration / 10f) * 6f + Mathf.Floor(duration / 7f) * 12f) * mitigation,
                "poison_seal_thief_trial" => (Mathf.Floor(duration / 7f) * 9f + Mathf.Floor(duration / 8f) * 10f + Mathf.Floor(duration / 7f) * 8f) * mitigation,
                "formation_breaker_boss" => (Mathf.Floor(duration / 6f) * 12f + Mathf.Floor(duration / 7f) * 8f) * mitigation,
                _ => 0f
            };
        }

        private static float ResolveTargetDurationCap(V02RoundConfig roundConfig, EnemyDefinition enemy)
        {
            if (roundConfig != null)
            {
                BenchmarkPassFailRule rule = ResolveBenchmarkRule(roundConfig, enemy);
                if (rule.passDurationMax > 0f)
                {
                    return rule.passDurationMax;
                }

                if (roundConfig.targetDurationMax > 0f)
                {
                    return roundConfig.targetDurationMax;
                }
            }

            return GetTargetDurationCap(enemy);
        }

        private static BenchmarkPassFailRule ResolveBenchmarkRule(V02RoundConfig roundConfig, EnemyDefinition enemy)
        {
            BenchmarkPassFailRule source = roundConfig?.benchmarkRule;
            float fallbackDurationMax = roundConfig != null && roundConfig.targetDurationMax > 0f
                ? roundConfig.targetDurationMax
                : GetTargetDurationCap(enemy);
            float passDurationMax = source != null && source.passDurationMax > 0f
                ? source.passDurationMax
                : fallbackDurationMax;
            float weakDurationMax = source != null && source.weakDurationMax > 0f
                ? source.weakDurationMax
                : Mathf.Max(passDurationMax * 1.25f, fallbackDurationMax);
            float fallbackHpLossMax = roundConfig != null && roundConfig.targetHpLossMax > 0f
                ? roundConfig.targetHpLossMax
                : 0.6f;
            float passHpLossMax = source != null && source.passHpLossMax > 0f
                ? source.passHpLossMax
                : fallbackHpLossMax;
            float weakHpLossMax = source != null && source.weakHpLossMax > 0f
                ? source.weakHpLossMax
                : Mathf.Min(1f, Mathf.Max(passHpLossMax + 0.2f, passHpLossMax * 1.25f));

            return new BenchmarkPassFailRule
            {
                passDurationMax = passDurationMax,
                weakDurationMax = weakDurationMax,
                passHpLossMax = passHpLossMax,
                weakHpLossMax = weakHpLossMax,
                expectedCounterTriggerMin = source != null ? source.expectedCounterTriggerMin : 0,
                expectedSkillCastMin = source != null ? source.expectedSkillCastMin : 0
            };
        }

        private static float GetTargetDurationCap(EnemyDefinition enemy)
        {
            return enemy?.enemyId switch
            {
                "mountain_imp_basic" => 60f,
                "turtle_guardian_shield" => 100f,
                "imp_swarm" => 110f,
                "red_poison_beast" => 120f,
                "seal_talisman_taoist" => 130f,
                "energy_thief_ghost" => 120f,
                "shield_swarm_trial" => 70f,
                "poison_seal_thief_trial" => 80f,
                "formation_breaker_elite" => 140f,
                "formation_breaker_boss" => 110f,
                _ => 120f
            };
        }

        private static string ResolveGrade(float duration, int damageTaken, V02RoundConfig roundConfig, EnemyDefinition enemy)
        {
            return ResolveGrade(duration, damageTaken, ResolveBenchmarkRule(roundConfig, enemy));
        }

        private static string ResolveGrade(float duration, int damageTaken, BenchmarkPassFailRule rule)
        {
            float hpLossRatio = Mathf.Clamp01(damageTaken / 100f);
            if (duration <= rule.passDurationMax && hpLossRatio <= rule.passHpLossMax)
            {
                return "Pass";
            }

            if (duration <= rule.weakDurationMax && hpLossRatio <= rule.weakHpLossMax)
            {
                return "Weak";
            }

            return "Fail";
        }

        private static string ResolveGradeReason(float duration, float actualHpLoss, BenchmarkPassFailRule rule)
        {
            bool passDuration = duration <= rule.passDurationMax;
            bool passHpLoss = actualHpLoss <= rule.passHpLossMax;
            if (passDuration && passHpLoss)
            {
                return "PassWithinDurationAndHpLoss";
            }

            bool weakDuration = duration <= rule.weakDurationMax;
            bool weakHpLoss = actualHpLoss <= rule.weakHpLossMax;
            if (weakDuration && weakHpLoss)
            {
                if (!passDuration && !passHpLoss)
                {
                    return "WeakDurationAndHpLossAbovePass";
                }

                if (!passDuration)
                {
                    return "WeakDurationAbovePass";
                }

                if (!passHpLoss)
                {
                    return "WeakHpLossAbovePass";
                }

                return "WeakWithinThreshold";
            }

            if (!weakDuration && !weakHpLoss)
            {
                return "FailDurationAndHpLossAboveWeak";
            }

            if (!weakDuration)
            {
                return "FailDurationAboveWeak";
            }

            if (!weakHpLoss)
            {
                return "FailHpLossAboveWeak";
            }

            return "FailUnknown";
        }

        private static string ResolveGrade(float duration, int damageTaken, float targetCap, CounterRelation relation)
        {
            if (duration <= targetCap * 0.65f && damageTaken < 60 && relation == CounterRelation.StrongCounter)
            {
                return "Pass";
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
            public string levelId;
            public int levelIndex;
            public string intendedRole;
            public string enemyId;
            public string buildName;
            public CounterRelation relation;
            public float multiplier;
            public string relationSource;
            public float estimatedDps;
            public float duration;
            public int damageTaken;
            public int expectedCounterTriggerCount;
            public string grade;
            public string note;
            public string expectedGrade;
            public float expectedDurationMin;
            public float expectedDurationMax;
            public string targetSource;
            public bool resultMatch;
            public float actualHpLoss;
            public float passHpLossMax;
            public float weakHpLossMax;
            public float passDurationMax;
            public float weakDurationMax;
            public string gradeReason;
        }

        private static BenchmarkLevelMetadata ResolveLevelMetadata(V02RoundConfig roundConfig, EnemyDefinition enemy)
        {
            if (roundConfig != null)
            {
                return new BenchmarkLevelMetadata(roundConfig.levelId, roundConfig.roundIndex, roundConfig.ResolvedIntendedRole);
            }

            return GetLevelMetadata(enemy);
        }

        private static BenchmarkLevelMetadata GetLevelMetadata(EnemyDefinition enemy)
        {
            return enemy?.enemyId switch
            {
                "mountain_imp_basic" => new BenchmarkLevelMetadata("1-1", 1, "Formation power tutorial"),
                "turtle_guardian_shield" => new BenchmarkLevelMetadata("1-2", 2, "Shield break tutorial"),
                "imp_swarm" => new BenchmarkLevelMetadata("1-3", 3, "Group clear tutorial"),
                "red_poison_beast" => new BenchmarkLevelMetadata("1-4", 4, "Poison sustain tutorial"),
                "seal_talisman_taoist" => new BenchmarkLevelMetadata("1-5", 5, "Seal line tutorial"),
                "energy_thief_ghost" => new BenchmarkLevelMetadata("1-6", 6, "Energy disruption tutorial"),
                "shield_swarm_trial" => new BenchmarkLevelMetadata("1-7", 7, "Shield and group check"),
                "poison_seal_thief_trial" => new BenchmarkLevelMetadata("1-8", 8, "Poison seal steal check"),
                "formation_breaker_elite" => new BenchmarkLevelMetadata("1-9", 9, "Mixed pressure check"),
                "formation_breaker_boss" => new BenchmarkLevelMetadata("1-10", 10, "Boss comprehensive check"),
                _ => new BenchmarkLevelMetadata(string.Empty, 0, string.Empty)
            };
        }

        private readonly struct BenchmarkLevelMetadata
        {
            public readonly string levelId;
            public readonly int levelIndex;
            public readonly string intendedRole;

            public BenchmarkLevelMetadata(string levelId, int levelIndex, string intendedRole)
            {
                this.levelId = levelId;
                this.levelIndex = levelIndex;
                this.intendedRole = intendedRole;
            }
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
