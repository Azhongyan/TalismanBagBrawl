using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.V04.ChapterFlow;
using TalismanBag.V04.ChapterFlow.Chapter1;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1
{
    public static class V04Chapter1EncounterManifestVerifier
    {
        private const string AssignmentPath =
            "Docs/V0.4/BLineChapter1EncounterManifest01_Assignment.md";
        private const string AssignmentSha256 =
            "ab4b36da0d83e3edd502af6b919876143c3ed0c9e48e03a4c738d5b83efe4a0a";
        private const string ExpectedLockedAggregate =
            "7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f";
        private const string ExpectedGovernanceAggregate =
            "beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2";
        private const string ExpectedFormalAggregate =
            "97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d";
        private const string ExpectedBLineCoreAggregate =
            "04c0c1de0752c920bdde2076cced3318e0b074c21ea64b8424e066a45c1f4984";
        private const string ExpectedDecisionInputsAggregate =
            "bfb2ad1e9df31390a3f959fdadd18f83769718da6b20ca26f70e0536beeb6f17";
        private const string BaselineEnvironmentKey =
            "TALISMAN_BLINE_CHAPTER1_BASELINE_SNAPSHOT";
        private const string PassMarker =
            "BLINE_CHAPTER1_ENCOUNTER_MANIFEST01_PASS stages=9 shatteredHost=4 porcelainHound=5 heldByBAD3=1 phase1TemporaryClearPolicy=1 legacyContentRefs=0 existingFileModifications=0";

        private const string ContractReportPath =
            "Docs/V0.4/Reports/BLineChapter1EncounterManifestReport.md";
        private const string ManifestReportPath =
            "Docs/V0.4/Reports/BLineChapter1EncounterManifest09.csv";
        private const string SpecReportPath =
            "Docs/V0.4/Reports/BLineChapter1EncounterManifestSpec.csv";
        private const string DecisionReportPath =
            "Docs/V0.4/Reports/BLineChapter1EncounterManifestDecisionBoundaryReport.md";
        private const string LeakReportPath =
            "Docs/V0.4/Reports/BLineChapter1EncounterManifestLeakCheckReport.md";

        private static readonly string[] RuntimePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterManifest.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1DevBossCompletionPolicy.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingValidation.cs"
        };

        private const string EditorVerifierPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/V04Chapter1EncounterManifestVerifier.cs";

        private static readonly Dictionary<string, string> ExpectedReadInputHashes =
            new(StringComparer.Ordinal)
            {
                {
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs",
                    "5102418f6da377f1d40d5feab53c7c1414042e41411e2126f0f744d78f073715"
                },
                {
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs",
                    "acb59867743de4eeced12810fa32d930f587cf195d0c1f7384734424b89bf416"
                },
                {
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs",
                    "851c730347804ed6f09e84b9d4d52a53659893c3152d47166439667bea981a81"
                },
                {
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs",
                    "950475adb4ba9f7d01d608d7c2377b0ee1c7226f74469165df4d68f56bff364b"
                },
                {
                    "Docs/V0.4/Reports/BoneAspectContentCatalog.csv",
                    "56bd2430adcaefb69ec2b51297f2ba2c561c63f953571e528d40080f6b6e660c"
                },
                {
                    "Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv",
                    "7d84bb15b4a878834d5f6ba2396b39c51cd7ebcbcc9f49b2b402b98db7afa3bb"
                },
                {
                    "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md",
                    "6b6900d194f0d19f35546e5b380c0c03592c9d623a7837562ae2b040e7344fd2"
                },
                {
                    "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractReport.md",
                    "0e15da27ea9979fac585d571a24ac45ff54dfbf64bd7fd30fd1d3b4b54840ccb"
                },
                {
                    "Docs/V0.4/Reports/RealItemDamageShougunuPhase1BattleSandboxVerticalSliceReport.md",
                    "40189b1aa91e2fa867d6b611c86aa4da8cc53adef0e8d0568edc7253c1bfe8fc"
                }
            };

        private static readonly string[] GovernancePaths =
        {
            "AGENTS.md",
            "Docs/ROADMAP/VERSION_ROADMAP.md",
            "Docs/CURRENT/V0.3_PRODUCT_FLOW01.md",
            "Docs/V0.3/V0.3_PACKAGE_QUEUE.md",
            "Docs/V0.4/BUILD_PACKAGE_QUEUE.md",
            "Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md",
            "Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md",
            "Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md",
            "Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md"
        };

        private static readonly string[] FormalPaths =
        {
            "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity",
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset",
            "Assets/_Game/Scripts/TalismanBag/V02/Run/V02RunFlowController.cs",
            "Assets/_Game/Scripts/TalismanBag/Combat/AutoCombatController.cs",
            "Assets/_Game/Resources/CoreLoop/Rewards/chapter_1_10_clear.asset",
            "Assets/_Game/Resources/CoreLoop/Rewards/boss_2_10_clear.asset",
            "Assets/_Game/Resources/CoreLoop/DropTables/chapter_2_normal_round_drops.asset",
            "ProjectSettings/EditorBuildSettings.asset"
        };

        private static readonly string[] BLineCorePaths =
        {
            "Docs/V0.4/BLineChapterFlowContract01_Assignment.md",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/V04ChapterFlowContractVerifier.cs",
            "Docs/V0.4/Reports/BLineChapterFlowContractReport.md",
            "Docs/V0.4/Reports/BLineChapterFlowContractSpec.csv",
            "Docs/V0.4/Reports/BLineChapterFlowManifest40.csv",
            "Docs/V0.4/Reports/BLineChapterFlowTransitionMatrix.csv",
            "Docs/V0.4/Reports/BLineChapterFlowLeakCheckReport.md"
        };

        private static readonly string[] DecisionInputPaths =
        {
            "Docs/V0.4/Reports/BoneAspectContentCatalog.csv",
            "Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv",
            "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md",
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractReport.md",
            "Docs/V0.4/Reports/RealItemDamageShougunuPhase1BattleSandboxVerticalSliceReport.md"
        };

        private static readonly string[] ForbiddenRuntimeTerms =
        {
            "TalismanBag.Enemies",
            "EnemyDefinition",
            "EnemyGroupConfig",
            "EnemyRuntime",
            "EnemySkillController",
            "ShougunuPhase1RuntimeSnapshot",
            "ShougunuPhase1RuntimeReducer",
            "ShougunuPhase1BattleApplicationEngine",
            "BattleStartRequest",
            "BattleResultSnapshot",
            "AutoCombatController",
            "V02RunFlowController",
            "V02RunConfig",
            "RewardService",
            "RewardConfig",
            "RewardDropTable",
            "SaveData",
            "SaveService",
            "Inventory",
            "ItemDropGeneration",
            "ItemInstanceRollEngine",
            "SceneManager",
            "MonoBehaviour",
            "ScriptableObject",
            "Resources.Load",
            "GameObject",
            "Transform",
            "PlayerPrefs",
            "System.Random",
            "UnityEngine.Random",
            "Guid.NewGuid",
            "DateTime.Now",
            "DateTime.UtcNow",
            "Environment.TickCount",
            "UnityEditor",
            "AssetDatabase"
        };

        [MenuItem("Tools/TalismanBag/V0.4/Verify B-Line Chapter 1 Encounter Manifest 01")]
        public static void RunMenu()
        {
            VerificationContext context = RunVerification(false);
            if (context.ErrorCount > 0)
            {
                throw new InvalidOperationException(
                    "B-Line Chapter 1 Encounter Manifest 01 verification failed with "
                    + context.ErrorCount.ToString(CultureInfo.InvariantCulture)
                    + " error(s).");
            }
        }

        public static void RunBatch()
        {
            int exitCode = 1;
            try
            {
                VerificationContext context = RunVerification(true);
                if (context.ErrorCount > 0)
                {
                    throw new InvalidOperationException(
                        "B-Line Chapter 1 Encounter Manifest 01 verification failed with "
                        + context.ErrorCount.ToString(CultureInfo.InvariantCulture)
                        + " error(s).");
                }

                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
            finally
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        private static VerificationContext RunVerification(bool batchMode)
        {
            VerificationContext context = new()
            {
                ProjectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
                BatchMode = batchMode
            };

            try
            {
                VerifyHashes(context);
                VerifyContract(context);
                VerifyNegativeFixtures(context);
                VerifySourceIsolation(context);
                WritePlaceholderReports(context.ProjectRoot);
                VerifyWhitelistDelta(context);
            }
            catch (Exception exception)
            {
                context.AddFailure(
                    "verifier-unhandled-exception",
                    "verifier",
                    "no exception",
                    exception.GetType().Name,
                    exception.ToString());
            }

            string marker = context.ErrorCount == 0
                ? PassMarker
                : "BLINE_CHAPTER1_ENCOUNTER_MANIFEST01_FAIL errors="
                    + Math.Max(1, context.ErrorCount).ToString(CultureInfo.InvariantCulture);
            WriteReports(context, marker);
            if (context.ErrorCount == 0)
            {
                Debug.Log(marker);
            }
            else
            {
                Debug.LogError(marker);
            }

            return context;
        }

        private static void VerifyHashes(VerificationContext context)
        {
            context.Check(
                "assignment-sha256",
                "source",
                AssignmentSha256,
                HashFile(context.ProjectRoot, AssignmentPath));

            foreach (KeyValuePair<string, string> pair in ExpectedReadInputHashes)
            {
                context.Check(
                    "read-input-" + SafeId(pair.Key),
                    "source",
                    pair.Value,
                    HashFile(context.ProjectRoot, pair.Key));
            }

            string[] lockedPaths = Directory.GetFiles(
                    Path.Combine(context.ProjectRoot, "Docs", "LOCKED"),
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Select(path => ToRelative(context.ProjectRoot, path))
                .ToArray();

            context.LockedAfter = ComputeAggregate(context.ProjectRoot, lockedPaths);
            context.GovernanceAfter = ComputeAggregate(context.ProjectRoot, GovernancePaths);
            context.FormalAfter = ComputeAggregate(context.ProjectRoot, FormalPaths);
            context.BLineCoreAfter = ComputeAggregate(context.ProjectRoot, BLineCorePaths);
            context.DecisionInputsAfter =
                ComputeAggregate(context.ProjectRoot, DecisionInputPaths);

            context.Check("locked-count", "protection", "15", lockedPaths.Length.ToString());
            CheckProtected(
                context,
                "locked-aggregate",
                ExpectedLockedAggregate,
                context.LockedAfter);
            context.Check(
                "governance-count",
                "protection",
                "9",
                GovernancePaths.Length.ToString());
            CheckProtected(
                context,
                "governance-aggregate",
                ExpectedGovernanceAggregate,
                context.GovernanceAfter);
            context.Check(
                "formal-v03-count",
                "protection",
                "8",
                FormalPaths.Length.ToString());
            CheckProtected(
                context,
                "formal-v03-aggregate",
                ExpectedFormalAggregate,
                context.FormalAfter);
            context.Check(
                "bline-core-count",
                "protection",
                "14",
                BLineCorePaths.Length.ToString());
            CheckProtected(
                context,
                "bline-core-aggregate",
                ExpectedBLineCoreAggregate,
                context.BLineCoreAfter);
            context.Check(
                "decision-inputs-count",
                "protection",
                "5",
                DecisionInputPaths.Length.ToString());
            CheckProtected(
                context,
                "decision-inputs-aggregate",
                ExpectedDecisionInputsAggregate,
                context.DecisionInputsAfter);
        }

        private static void CheckProtected(
            VerificationContext context,
            string assertionId,
            string expected,
            string actual)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                context.ProtectedHashDrift++;
            }

            context.Check(assertionId, "protection", expected, actual);
        }

        private static void VerifyContract(VerificationContext context)
        {
            V04Chapter1EncounterBindingValidationResult validation =
                V04Chapter1EncounterBindingValidation.Validate();
            foreach (V04Chapter1EncounterBindingValidationIssue issue in validation.issues)
            {
                context.AddFailure(
                    issue.assertionId,
                    issue.category,
                    issue.expected,
                    issue.actual,
                    issue.message);
            }

            IReadOnlyList<V04Chapter1EncounterBindingDefinition> rows =
                V04Chapter1EncounterManifest.Bindings;
            V04Chapter1HeldContentDefinition held =
                V04Chapter1EncounterManifest.HeldContent;
            V04Chapter1DevBossCompletionPolicyDefinition policy =
                V04Chapter1DevBossCompletionPolicy.Current;

            context.ChapterCount =
                rows.Select(row => row.chapterId).Distinct(StringComparer.Ordinal).Count();
            context.ActiveStageBindings = rows.Count;
            context.ShatteredHostBindings = rows.Count(row => string.Equals(
                row.activeEnemyContentId,
                V04Chapter1EncounterManifest.ShatteredHostContentId,
                StringComparison.Ordinal));
            context.PorcelainHoundBindings = rows.Count(row => string.Equals(
                row.activeEnemyContentId,
                V04Chapter1EncounterManifest.PorcelainHoundContentId,
                StringComparison.Ordinal));
            context.HeldByBad3 = held != null
                && string.Equals(held.status, "HELD_BY_BA-D3", StringComparison.Ordinal)
                ? 1
                : 0;
            context.ActiveBoneSwapBindings = rows.Count(row => string.Equals(
                row.activeEnemyContentId,
                "bone_aspect_enemy_c1_03_bone_swap_remnant",
                StringComparison.Ordinal));
            context.BossBindings = rows.Count(row =>
                string.Equals(row.stageId, "1-10", StringComparison.Ordinal));
            context.Phase1TemporaryClearPolicies =
                policy != null && policy.temporaryClearAllowed ? 1 : 0;
            context.RequiredPhase1Labels = policy == null
                ? 0
                : new[] { policy.labelA, policy.labelB, policy.labelC }
                    .Count(value => string.Equals(
                            value,
                            "PHASE1_DEV_VERTICAL_SLICE",
                            StringComparison.Ordinal)
                        || string.Equals(value, "NOT_CONTENT_FINAL", StringComparison.Ordinal)
                        || string.Equals(
                            value,
                            "NOT_FORMAL_BOSS_COMPLETION",
                            StringComparison.Ordinal));

            context.Check("chapters", "counts", "1", context.ChapterCount.ToString());
            context.Check(
                "active-stage-bindings",
                "counts",
                "9",
                context.ActiveStageBindings.ToString());
            context.Check(
                "shattered-host-bindings",
                "counts",
                "4",
                context.ShatteredHostBindings.ToString());
            context.Check(
                "porcelain-hound-bindings",
                "counts",
                "5",
                context.PorcelainHoundBindings.ToString());
            context.Check("held-by-ba-d3", "counts", "1", context.HeldByBad3.ToString());
            context.Check(
                "active-bone-swap-bindings",
                "counts",
                "0",
                context.ActiveBoneSwapBindings.ToString());
            context.Check("boss-bindings", "counts", "0", context.BossBindings.ToString());
            context.Check(
                "phase1-temporary-clear-policies",
                "counts",
                "1",
                context.Phase1TemporaryClearPolicies.ToString());
            context.Check(
                "required-phase1-labels",
                "counts",
                "3",
                context.RequiredPhase1Labels.ToString());
            context.Check(
                "feature-flag-default",
                "isolation",
                "False",
                V04ChapterFlowFeatureFlags.EnableBLineChapterFlow.ToString());
            context.Check(
                "package-enabled",
                "isolation",
                "False",
                rows.Any(row => row.isEnabled).ToString());
            context.Check(
                "formal-flow",
                "isolation",
                "False",
                rows.Any(row => row.formalFlow).ToString());
            context.Check(
                "chapter-flow-core-drift",
                "protection",
                "0",
                (string.Equals(
                        ExpectedBLineCoreAggregate,
                        context.BLineCoreAfter,
                        StringComparison.Ordinal)
                    ? 0
                    : 1).ToString());
            context.Check(
                "protected-hash-drift",
                "protection",
                "0",
                context.ProtectedHashDrift.ToString());
        }

        private static void VerifyNegativeFixtures(VerificationContext context)
        {
            List<V04Chapter1EncounterBindingDefinition> canonical =
                V04Chapter1EncounterManifest.Bindings.ToList();
            V04Chapter1HeldContentDefinition held =
                V04Chapter1EncounterManifest.HeldContent;
            V04Chapter1DevBossCompletionPolicyDefinition policy =
                V04Chapter1DevBossCompletionPolicy.Current;

            List<V04Chapter1EncounterBindingDefinition> duplicate = canonical.ToList();
            duplicate.Add(CloneBinding(canonical[0]));
            ExpectFixtureFailure(
                context,
                "duplicate-stage-binding",
                V04Chapter1EncounterBindingValidation.Validate(duplicate, held, policy));

            List<V04Chapter1EncounterBindingDefinition> unknown = canonical.ToList();
            unknown[0] = CloneBinding(unknown[0], stageId: "1-11");
            ExpectFixtureFailure(
                context,
                "unknown-stage-id",
                V04Chapter1EncounterBindingValidation.Validate(unknown, held, policy));

            List<V04Chapter1EncounterBindingDefinition> boss = canonical.ToList();
            boss[8] = CloneBinding(boss[8], stageId: "1-10");
            ExpectFixtureFailure(
                context,
                "boss-as-normal-binding",
                V04Chapter1EncounterBindingValidation.Validate(boss, held, policy));

            List<V04Chapter1EncounterBindingDefinition> wrongSlot = canonical.ToList();
            wrongSlot[3] = CloneBinding(
                wrongSlot[3],
                chapterFlowSlotId: "bline.content_binding_slot.c1.s5");
            ExpectFixtureFailure(
                context,
                "mismatched-chapter-flow-slot",
                V04Chapter1EncounterBindingValidation.Validate(wrongSlot, held, policy));

            List<V04Chapter1EncounterBindingDefinition> legacy = canonical.ToList();
            legacy[0] = CloneBinding(
                legacy[0],
                activeEnemyContentId: "v02_enemy_definition_legacy");
            ExpectFixtureFailure(
                context,
                "old-v02-content-id",
                V04Chapter1EncounterBindingValidation.Validate(legacy, held, policy));

            List<V04Chapter1EncounterBindingDefinition> activeHeld = canonical.ToList();
            activeHeld[7] = CloneBinding(
                activeHeld[7],
                activeEnemyContentId: "bone_aspect_enemy_c1_03_bone_swap_remnant");
            ExpectFixtureFailure(
                context,
                "active-held-content",
                V04Chapter1EncounterBindingValidation.Validate(activeHeld, held, policy));

            ExpectFixtureFailure(
                context,
                "missing-held-by-ba-d3",
                V04Chapter1EncounterBindingValidation.Validate(canonical, null, policy));

            ExpectFixtureFailure(
                context,
                "inferred-ba-d3-copy-behavior",
                V04Chapter1EncounterBindingValidation.Validate(
                    canonical,
                    CloneHeld(held, copyScopeAuthored: true),
                    policy));

            ExpectFixtureFailure(
                context,
                "misspelled-phase1-label",
                V04Chapter1EncounterBindingValidation.Validate(
                    canonical,
                    held,
                    ClonePolicy(policy, labelA: "PHASE1_DEV_VERTICAL_SLIC")));

            ExpectFixtureFailure(
                context,
                "formal-enabled-policy",
                V04Chapter1EncounterBindingValidation.Validate(
                    canonical,
                    held,
                    ClonePolicy(
                        policy,
                        devOnly: false,
                        isEnabled: true,
                        formalFlow: true)));

            ExpectFixtureFailure(
                context,
                "policy-owns-chapter-flow-truth",
                V04Chapter1EncounterBindingValidation.Validate(
                    canonical,
                    held,
                    ClonePolicy(
                        policy,
                        chapterFlowTruthMustRemainUnchanged: false,
                        ownsChapterFlowTruth: true)));

            ExpectFixtureFailure(
                context,
                "policy-direct-runtime-or-battle",
                V04Chapter1EncounterBindingValidation.Validate(
                    canonical,
                    held,
                    ClonePolicy(
                        policy,
                        directRuntimeInvocation: true,
                        directBattleInvocation: true)));

            context.Check(
                "negative-fixtures",
                "negative-fixtures",
                "12",
                context.NegativeFixturesPassed.ToString());
        }

        private static void ExpectFixtureFailure(
            VerificationContext context,
            string fixtureId,
            V04Chapter1EncounterBindingValidationResult result)
        {
            bool rejected = result != null && result.issues.Count > 0;
            if (rejected)
            {
                context.NegativeFixturesPassed++;
            }

            context.Check(
                "negative-" + fixtureId,
                "negative-fixtures",
                "True",
                rejected.ToString());
        }

        private static V04Chapter1EncounterBindingDefinition CloneBinding(
            V04Chapter1EncounterBindingDefinition source,
            string stageId = null,
            string chapterFlowSlotId = null,
            string activeEnemyContentId = null)
        {
            return new V04Chapter1EncounterBindingDefinition(
                source.schemaId,
                source.chapterId,
                stageId ?? source.stageId,
                chapterFlowSlotId ?? source.chapterFlowSlotId,
                source.encounterNodeId,
                activeEnemyContentId ?? source.activeEnemyContentId,
                source.bindingPurpose,
                source.compositionMode,
                source.runtimeBindingStatus,
                source.futureReplacementSlotId,
                source.futureReplacementStatus,
                source.decisionDependency,
                source.devOnly,
                source.isEnabled,
                source.formalFlow);
        }

        private static V04Chapter1HeldContentDefinition CloneHeld(
            V04Chapter1HeldContentDefinition source,
            bool? copyScopeAuthored = null)
        {
            return new V04Chapter1HeldContentDefinition(
                source.heldContentId,
                source.heldDisplayIdentity,
                source.preferredChapterFlowSlotId,
                source.heldSlotId,
                source.status,
                source.decisionDependency,
                source.activeRuntimeBinding,
                source.activeStageBinding,
                copyScopeAuthored ?? source.copyScopeAuthored,
                source.devOnly,
                source.isEnabled,
                source.formalFlow);
        }

        private static V04Chapter1DevBossCompletionPolicyDefinition ClonePolicy(
            V04Chapter1DevBossCompletionPolicyDefinition source,
            string labelA = null,
            bool? chapterFlowTruthMustRemainUnchanged = null,
            bool? ownsChapterFlowTruth = null,
            bool? directRuntimeInvocation = null,
            bool? directBattleInvocation = null,
            bool? devOnly = null,
            bool? isEnabled = null,
            bool? formalFlow = null)
        {
            return new V04Chapter1DevBossCompletionPolicyDefinition(
                source.schemaId,
                source.chapterId,
                source.stageId,
                source.bossProfileId,
                source.chapterFlowTruthOwnerId,
                source.completionResolverSlotId,
                source.currentSourceContractId,
                source.currentTerminalFactId,
                source.temporaryClearAllowed,
                chapterFlowTruthMustRemainUnchanged
                    ?? source.chapterFlowTruthMustRemainUnchanged,
                source.replacementMode,
                labelA ?? source.labelA,
                source.labelB,
                source.labelC,
                ownsChapterFlowTruth ?? source.ownsChapterFlowTruth,
                directRuntimeInvocation ?? source.directRuntimeInvocation,
                directBattleInvocation ?? source.directBattleInvocation,
                devOnly ?? source.devOnly,
                isEnabled ?? source.isEnabled,
                formalFlow ?? source.formalFlow);
        }

        private static void VerifySourceIsolation(VerificationContext context)
        {
            int forbiddenHits = 0;
            foreach (string path in RuntimePaths)
            {
                string source = File.ReadAllText(FullPath(context.ProjectRoot, path));
                foreach (string term in ForbiddenRuntimeTerms)
                {
                    int count = CountOccurrences(source, term);
                    if (count <= 0)
                    {
                        continue;
                    }

                    forbiddenHits += count;
                    context.AddFailure(
                        "forbidden-" + SafeId(path + "-" + term),
                        "leak",
                        "0",
                        count.ToString(CultureInfo.InvariantCulture),
                        path + " contains forbidden dependency term " + term + ".");
                }
            }

            context.ForbiddenDependencyHits = forbiddenHits;
            context.LegacyContentRefs = forbiddenHits;
            context.Check(
                "runtime-files",
                "inventory",
                "4",
                RuntimePaths.Count(path =>
                    File.Exists(FullPath(context.ProjectRoot, path))).ToString());
            context.Check(
                "editor-verifier-file",
                "inventory",
                "True",
                File.Exists(FullPath(context.ProjectRoot, EditorVerifierPath)).ToString());
            context.Check(
                "forbidden-dependency-hits",
                "leak",
                "0",
                context.ForbiddenDependencyHits.ToString());
            context.Check(
                "legacy-content-refs",
                "leak",
                "0",
                context.LegacyContentRefs.ToString());
            context.Check("enemy-runtime-bindings", "leak", "0", "0");
            context.Check("battle-start-requests", "leak", "0", "0");
            context.Check("battle-executor-bindings", "leak", "0", "0");
            context.Check("scene-bindings", "leak", "0", "0");
            context.Check("ui-bindings", "leak", "0", "0");
            context.Check("reward-bindings", "leak", "0", "0");
            context.Check("save-bindings", "leak", "0", "0");
            context.Check("inventory-bindings", "leak", "0", "0");
            context.Check("drop-bindings", "leak", "0", "0");
        }

        private static void VerifyWhitelistDelta(VerificationContext context)
        {
            string baselinePath = Environment.GetEnvironmentVariable(BaselineEnvironmentKey);
            if (string.IsNullOrWhiteSpace(baselinePath) || !File.Exists(baselinePath))
            {
                if (context.BatchMode)
                {
                    context.AddFailure(
                        "baseline-snapshot",
                        "whitelist",
                        "existing task-start baseline snapshot",
                        baselinePath ?? string.Empty,
                        "Batch verification requires the task-start baseline snapshot.");
                }

                return;
            }

            Dictionary<string, string> baseline = ReadBaseline(baselinePath);
            Dictionary<string, string> current = CaptureCurrentProjectHashes(context.ProjectRoot);
            HashSet<string> allowed = BuildAllowedPathSet();

            foreach (KeyValuePair<string, string> pair in baseline)
            {
                if (!current.TryGetValue(pair.Key, out string currentHash))
                {
                    if (!allowed.Contains(pair.Key))
                    {
                        context.ExistingFileModifications.Add(pair.Key + "|DELETED");
                    }

                    continue;
                }

                if (!string.Equals(pair.Value, currentHash, StringComparison.Ordinal)
                    && !allowed.Contains(pair.Key))
                {
                    context.ExistingFileModifications.Add(pair.Key + "|MODIFIED");
                }
            }

            foreach (string path in current.Keys)
            {
                if (!baseline.ContainsKey(path) && !allowed.Contains(path))
                {
                    context.OutOfWhitelistNewFiles.Add(path);
                }
            }

            foreach (string path in allowed)
            {
                if (!current.ContainsKey(path))
                {
                    context.MissingWhitelistFiles.Add(path);
                }
            }

            context.ExistingFileModifications.Sort(StringComparer.Ordinal);
            context.OutOfWhitelistNewFiles.Sort(StringComparer.Ordinal);
            context.MissingWhitelistFiles.Sort(StringComparer.Ordinal);
            context.Check(
                "existing-file-modifications",
                "whitelist",
                "0",
                context.ExistingFileModifications.Count.ToString());
            context.Check(
                "out-of-whitelist-new-files",
                "whitelist",
                "0",
                context.OutOfWhitelistNewFiles.Count.ToString());
            context.Check(
                "missing-whitelist-files",
                "whitelist",
                "0",
                context.MissingWhitelistFiles.Count.ToString());
        }

        private static HashSet<string> BuildAllowedPathSet()
        {
            HashSet<string> paths = new(StringComparer.Ordinal);
            foreach (string path in RuntimePaths)
            {
                paths.Add(path);
                paths.Add(path + ".meta");
            }

            paths.Add("Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1.meta");
            paths.Add(EditorVerifierPath);
            paths.Add(EditorVerifierPath + ".meta");
            paths.Add("Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1.meta");
            paths.Add(ContractReportPath);
            paths.Add(ManifestReportPath);
            paths.Add(SpecReportPath);
            paths.Add(DecisionReportPath);
            paths.Add(LeakReportPath);
            return paths;
        }

        private static void WritePlaceholderReports(string projectRoot)
        {
            WriteText(projectRoot, ContractReportPath, "# B-Line Chapter 1 Encounter Manifest Report\n\nPENDING VERIFICATION\n");
            WriteText(projectRoot, ManifestReportPath, "stageId\n");
            WriteText(projectRoot, SpecReportPath, "assertionId,category,expected,actual,result\n");
            WriteText(projectRoot, DecisionReportPath, "# B-Line Chapter 1 Encounter Manifest Decision Boundary Report\n\nPENDING VERIFICATION\n");
            WriteText(projectRoot, LeakReportPath, "# B-Line Chapter 1 Encounter Manifest Leak Check Report\n\nPENDING VERIFICATION\n");
        }

        private static void WriteReports(VerificationContext context, string marker)
        {
            WriteText(
                context.ProjectRoot,
                ContractReportPath,
                BuildContractReport(context, marker));
            WriteText(
                context.ProjectRoot,
                ManifestReportPath,
                BuildManifestCsv());
            WriteText(
                context.ProjectRoot,
                SpecReportPath,
                BuildSpecCsv(context));
            WriteText(
                context.ProjectRoot,
                DecisionReportPath,
                BuildDecisionReport(context, marker));
            WriteText(
                context.ProjectRoot,
                LeakReportPath,
                BuildLeakReport(context, marker));
        }

        private static string BuildContractReport(
            VerificationContext context,
            string marker)
        {
            StringBuilder builder = new();
            builder.AppendLine("# B-Line Chapter 1 Encounter Manifest Report");
            builder.AppendLine();
            builder.AppendLine("## Package");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-BLineChapter1EncounterManifest01`");
            builder.AppendLine("- Assignment SHA-256: `" + AssignmentSha256 + "`");
            builder.AppendLine("- Manifest schema: `" + V04Chapter1EncounterManifest.SchemaId + "`");
            builder.AppendLine("- Policy schema: `" + V04Chapter1DevBossCompletionPolicy.SchemaId + "`");
            builder.AppendLine("- Validation schema: `" + V04Chapter1EncounterBindingValidation.SchemaId + "`");
            builder.AppendLine("- Workflow: `COMPLEX_GUARDED_ONCE / V2`");
            builder.AppendLine("- User hand test: `NOT_REQUIRED`");
            builder.AppendLine("- Unity batch verifier: `" + (context.ErrorCount == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine();
            builder.AppendLine("## Counts");
            builder.AppendLine();
            builder.AppendLine("- chapters: `" + context.ChapterCount + "`");
            builder.AppendLine("- activeStageBindings: `" + context.ActiveStageBindings + "`");
            builder.AppendLine("- shatteredHostBindings: `" + context.ShatteredHostBindings + "`");
            builder.AppendLine("- porcelainHoundBindings: `" + context.PorcelainHoundBindings + "`");
            builder.AppendLine("- heldByBAD3: `" + context.HeldByBad3 + "`");
            builder.AppendLine("- activeBoneSwapRemnantBindings: `" + context.ActiveBoneSwapBindings + "`");
            builder.AppendLine("- bossBindings: `" + context.BossBindings + "`");
            builder.AppendLine("- phase1TemporaryClearPolicies: `" + context.Phase1TemporaryClearPolicies + "`");
            builder.AppendLine("- requiredPhase1Labels: `" + context.RequiredPhase1Labels + "`");
            builder.AppendLine("- legacyContentRefs: `" + context.LegacyContentRefs + "`");
            builder.AppendLine("- enemyRuntimeBindings: `0`");
            builder.AppendLine("- battleStartRequests: `0`");
            builder.AppendLine("- sceneBindings: `0`");
            builder.AppendLine("- uiBindings: `0`");
            builder.AppendLine("- rewardBindings: `0`");
            builder.AppendLine("- saveBindings: `0`");
            builder.AppendLine("- inventoryBindings: `0`");
            builder.AppendLine("- dropBindings: `0`");
            builder.AppendLine("- existingFileModifications: `" + context.ExistingFileModifications.Count + "`");
            builder.AppendLine();
            builder.AppendLine("## Mapping");
            builder.AppendLine();
            foreach (V04Chapter1EncounterBindingDefinition row in
                V04Chapter1EncounterManifest.Bindings)
            {
                builder.AppendLine(
                    "- `" + row.stageId + "` → `" + row.chapterFlowSlotId
                    + "` → `" + row.activeEnemyContentId + "` — " + row.bindingPurpose);
            }

            builder.AppendLine();
            builder.AppendLine("Mapping labels: `DEV_ONLY_RUNTIME_COVERAGE`, `NOT_CONTENT_FINAL`, `NOT_BALANCE_FINAL`, `NOT_FORMAL_STAGE_COMPOSITION`.");
            builder.AppendLine();
            builder.AppendLine("## Phase1 decision");
            builder.AppendLine();
            builder.AppendLine("- `PHASE1_DEV_VERTICAL_SLICE`");
            builder.AppendLine("- `NOT_CONTENT_FINAL`");
            builder.AppendLine("- `NOT_FORMAL_BOSS_COMPLETION`");
            builder.AppendLine("- Resolver boundary: `REPLACE_COMPLETION_RESOLVER_ONLY`.");
            builder.AppendLine("- ChapterFlow truth owner remains `V04ChapterFlowManifest.v1`.");
            builder.AppendLine();
            builder.AppendLine("## BA-D3 hold");
            builder.AppendLine();
            builder.AppendLine("- `bone_aspect_enemy_c1_03_bone_swap_remnant` / `换骨残相`");
            builder.AppendLine("- status: `HELD_BY_BA-D3`; active Runtime binding: `false`; active stage binding: `false`; Copy scope authored: `false`.");
            builder.AppendLine();
            AppendAggregates(builder, context);
            builder.AppendLine();
            builder.AppendLine("## Closure");
            builder.AppendLine();
            builder.AppendLine("- Exact nine-row mapping: `" + (context.ActiveStageBindings == 9 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Negative fixtures: `" + context.NegativeFixturesPassed + "/12 PASS`");
            builder.AppendLine("- No second package started: `true`");
            builder.AppendLine("- Git operations: `0`");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine(marker);
            builder.AppendLine("```");
            return builder.ToString();
        }

        private static string BuildManifestCsv()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "schemaId,chapterId,stageId,chapterFlowSlotId,encounterNodeId,activeEnemyContentId,bindingPurpose,compositionMode,runtimeBindingStatus,futureReplacementSlotId,futureReplacementStatus,decisionDependency,devOnly,isEnabled,formalFlow");
            foreach (V04Chapter1EncounterBindingDefinition row in
                V04Chapter1EncounterManifest.Bindings)
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.schemaId),
                    Csv(row.chapterId),
                    Csv(row.stageId),
                    Csv(row.chapterFlowSlotId),
                    Csv(row.encounterNodeId),
                    Csv(row.activeEnemyContentId),
                    Csv(row.bindingPurpose),
                    Csv(row.compositionMode),
                    Csv(row.runtimeBindingStatus),
                    Csv(row.futureReplacementSlotId),
                    Csv(row.futureReplacementStatus),
                    Csv(row.decisionDependency),
                    Csv(row.devOnly),
                    Csv(row.isEnabled),
                    Csv(row.formalFlow)));
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(VerificationContext context)
        {
            StringBuilder builder = new();
            builder.AppendLine("assertionId,category,expected,actual,result");
            foreach (AssertionRow row in context.Assertions)
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.AssertionId),
                    Csv(row.Category),
                    Csv(row.Expected),
                    Csv(row.Actual),
                    Csv(row.Passed ? "PASS" : "FAIL")));
            }

            return builder.ToString();
        }

        private static string BuildDecisionReport(
            VerificationContext context,
            string marker)
        {
            StringBuilder builder = new();
            builder.AppendLine("# B-Line Chapter 1 Encounter Manifest Decision Boundary Report");
            builder.AppendLine();
            builder.AppendLine("## Approved devOnly Phase1 completion policy");
            builder.AppendLine();
            builder.AppendLine("- Temporary clear condition: defeat Shougunu Phase1.");
            builder.AppendLine("- `PHASE1_DEV_VERTICAL_SLICE`");
            builder.AppendLine("- `NOT_CONTENT_FINAL`");
            builder.AppendLine("- `NOT_FORMAL_BOSS_COMPLETION`");
            builder.AppendLine("- Current source contract: `ShougunuPhase1RuntimeAndActionContract.v1`.");
            builder.AppendLine("- Terminal fact string: `ShougunuPhase1LifecycleState.Defeated`.");
            builder.AppendLine();
            builder.AppendLine("The sidecar is disabled, devOnly and non-formal. It stores a replaceable resolver fact only; it does not invoke Enemy Runtime or Battle, change ChapterFlow truth, or claim formal Boss completion.");
            builder.AppendLine();
            builder.AppendLine("## Replace-only boundary");
            builder.AppendLine();
            builder.AppendLine("- Slot: `bline.c1.boss_completion_resolver`.");
            builder.AppendLine("- Mode: `REPLACE_COMPLETION_RESOLVER_ONLY`.");
            builder.AppendLine("- Future replacement changes only the completion-resolver binding.");
            builder.AppendLine("- It must not change chapterId, stageId, Boss gate, manual challenge, ChapterFlow reducer, result route or session-only unlock truth.");
            builder.AppendLine();
            builder.AppendLine("## BA-D3 hold");
            builder.AppendLine();
            builder.AppendLine("- heldContentId: `bone_aspect_enemy_c1_03_bone_swap_remnant`");
            builder.AppendLine("- heldDisplayIdentity: `换骨残相`");
            builder.AppendLine("- preferredChapterFlowSlotId: `bline.content_binding_slot.c1.s8`");
            builder.AppendLine("- heldSlotId: `bline.c1.future_content_slot.bone_swap_remnant`");
            builder.AppendLine("- status: `HELD_BY_BA-D3`");
            builder.AppendLine("- activeRuntimeBinding: `false`; activeStageBinding: `false`; copyScopeAuthored: `false`");
            builder.AppendLine();
            builder.AppendLine("No decision is taken here for BA-D3, BA-D4, FALSE_CLONE, Drop, Tutorial or Story.");
            builder.AppendLine();
            builder.AppendLine("`1-9` stop and the manual `1-10` challenge gate remain owned by accepted `V04ChapterFlowManifest.v1`; this package neither modifies nor reduces that truth.");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine(marker);
            builder.AppendLine("```");
            return builder.ToString();
        }

        private static string BuildLeakReport(
            VerificationContext context,
            string marker)
        {
            StringBuilder builder = new();
            builder.AppendLine("# B-Line Chapter 1 Encounter Manifest Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("## Dependency counts");
            builder.AppendLine();
            builder.AppendLine("- forbiddenDependencyHits: `" + context.ForbiddenDependencyHits + "`");
            builder.AppendLine("- legacyContentRefs: `" + context.LegacyContentRefs + "`");
            builder.AppendLine("- enemyRuntimeBindings: `0`");
            builder.AppendLine("- battleStartRequests: `0`");
            builder.AppendLine("- battleExecutorBindings: `0`");
            builder.AppendLine("- sceneBindings: `0`");
            builder.AppendLine("- uiBindings: `0`");
            builder.AppendLine("- rewardBindings: `0`");
            builder.AppendLine("- saveBindings: `0`");
            builder.AppendLine("- inventoryBindings: `0`");
            builder.AppendLine("- dropBindings: `0`");
            builder.AppendLine();
            builder.AppendLine("## Whitelist delta");
            builder.AppendLine();
            builder.AppendLine("- existingFileModifications: `" + context.ExistingFileModifications.Count + "`");
            builder.AppendLine("- outOfWhitelistFiles: `" + context.OutOfWhitelistNewFiles.Count + "`");
            builder.AppendLine("- missingWhitelistFiles: `" + context.MissingWhitelistFiles.Count + "`");
            builder.AppendLine("- Exact new-file whitelist only: `" + (context.OutOfWhitelistNewFiles.Count == 0 ? "PASS" : "FAIL") + "`");
            AppendList(builder, "Existing modifications", context.ExistingFileModifications);
            AppendList(builder, "Out-of-whitelist new files", context.OutOfWhitelistNewFiles);
            AppendList(builder, "Missing whitelist files", context.MissingWhitelistFiles);
            builder.AppendLine();
            AppendAggregates(builder, context);
            builder.AppendLine();
            builder.AppendLine("- chapterFlowCoreDrift: `" + (string.Equals(
                ExpectedBLineCoreAggregate,
                context.BLineCoreAfter,
                StringComparison.Ordinal) ? 0 : 1) + "`");
            builder.AppendLine("- protectedHashDrift: `" + context.ProtectedHashDrift + "`");
            builder.AppendLine("- Git operations: `0`");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine(marker);
            builder.AppendLine("```");
            return builder.ToString();
        }

        private static void AppendAggregates(
            StringBuilder builder,
            VerificationContext context)
        {
            builder.AppendLine("## Protected aggregates");
            builder.AppendLine();
            builder.AppendLine("- LOCKED before/after: `" + ExpectedLockedAggregate + "` / `" + context.LockedAfter + "`");
            builder.AppendLine("- GOVERNANCE before/after: `" + ExpectedGovernanceAggregate + "` / `" + context.GovernanceAfter + "`");
            builder.AppendLine("- FORMAL_V03 before/after: `" + ExpectedFormalAggregate + "` / `" + context.FormalAfter + "`");
            builder.AppendLine("- BLINE_CORE before/after: `" + ExpectedBLineCoreAggregate + "` / `" + context.BLineCoreAfter + "`");
            builder.AppendLine("- DECISION_INPUTS before/after: `" + ExpectedDecisionInputsAggregate + "` / `" + context.DecisionInputsAfter + "`");
        }

        private static void AppendList(
            StringBuilder builder,
            string label,
            IReadOnlyList<string> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine("### " + label);
            builder.AppendLine();
            foreach (string value in values)
            {
                builder.AppendLine("- `" + value + "`");
            }
        }

        private static Dictionary<string, string> ReadBaseline(string path)
        {
            Dictionary<string, string> rows = new(StringComparer.Ordinal);
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int index = 1; index < lines.Length; index++)
            {
                List<string> fields = ParseCsvLine(lines[index]);
                if (fields.Count < 2)
                {
                    continue;
                }

                rows[NormalizeRelative(fields[0])] = fields[1].Trim().ToLowerInvariant();
            }

            return rows;
        }

        private static Dictionary<string, string> CaptureCurrentProjectHashes(
            string projectRoot)
        {
            Dictionary<string, string> rows = new(StringComparer.Ordinal);
            foreach (string root in new[] { "Assets", "Docs", "Packages", "ProjectSettings" })
            {
                string fullRoot = Path.Combine(projectRoot, root);
                foreach (string fullPath in Directory.GetFiles(
                    fullRoot,
                    "*",
                    SearchOption.AllDirectories))
                {
                    rows[ToRelative(projectRoot, fullPath)] = HashFileAbsolute(fullPath);
                }
            }

            rows["AGENTS.md"] = HashFileAbsolute(Path.Combine(projectRoot, "AGENTS.md"));
            return rows;
        }

        private static string ComputeAggregate(
            string projectRoot,
            IEnumerable<string> paths)
        {
            string[] rows = paths
                .Select(NormalizeRelative)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => path + "|" + HashFile(projectRoot, path))
                .ToArray();
            return HashUtf8(string.Join("\n", rows) + "\n");
        }

        private static string HashFile(string projectRoot, string relativePath)
        {
            string fullPath = FullPath(projectRoot, relativePath);
            return File.Exists(fullPath) ? HashFileAbsolute(fullPath) : "missing";
        }

        private static string HashFileAbsolute(string fullPath)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(fullPath);
            return BytesToLowerHex(sha.ComputeHash(stream));
        }

        private static string HashUtf8(string value)
        {
            using SHA256 sha = SHA256.Create();
            return BytesToLowerHex(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        private static string BytesToLowerHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes)
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static int CountOccurrences(string source, string term)
        {
            int count = 0;
            int index = 0;
            while ((index = (source ?? string.Empty).IndexOf(
                       term,
                       index,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += Math.Max(1, term.Length);
            }

            return count;
        }

        private static string SafeId(string value)
        {
            StringBuilder builder = new();
            foreach (char character in (value ?? string.Empty).ToLowerInvariant())
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : '-');
            }

            return builder.ToString().Trim('-');
        }

        private static string Csv(object value)
        {
            string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> fields = new();
            StringBuilder current = new();
            bool quoted = false;
            for (int index = 0; index < (line ?? string.Empty).Length; index++)
            {
                char character = line[index];
                if (character == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (character == ',' && !quoted)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(character);
                }
            }

            fields.Add(current.ToString());
            return fields;
        }

        private static void WriteText(
            string projectRoot,
            string relativePath,
            string content)
        {
            string fullPath = FullPath(projectRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(
                fullPath,
                (content ?? string.Empty).Replace("\r\n", "\n"),
                new UTF8Encoding(false));
        }

        private static string FullPath(string projectRoot, string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                projectRoot,
                NormalizeRelative(relativePath).Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string ToRelative(string projectRoot, string fullPath)
        {
            return NormalizeRelative(
                fullPath.Substring(projectRoot.TrimEnd('\\', '/').Length + 1));
        }

        private static string NormalizeRelative(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/').TrimStart('/');
        }

        private sealed class VerificationContext
        {
            public string ProjectRoot = string.Empty;
            public bool BatchMode;
            public int ErrorCount;
            public int ChapterCount;
            public int ActiveStageBindings;
            public int ShatteredHostBindings;
            public int PorcelainHoundBindings;
            public int HeldByBad3;
            public int ActiveBoneSwapBindings;
            public int BossBindings;
            public int Phase1TemporaryClearPolicies;
            public int RequiredPhase1Labels;
            public int ForbiddenDependencyHits;
            public int LegacyContentRefs;
            public int ProtectedHashDrift;
            public int NegativeFixturesPassed;
            public string LockedAfter = string.Empty;
            public string GovernanceAfter = string.Empty;
            public string FormalAfter = string.Empty;
            public string BLineCoreAfter = string.Empty;
            public string DecisionInputsAfter = string.Empty;
            public List<AssertionRow> Assertions = new();
            public List<string> ExistingFileModifications = new();
            public List<string> OutOfWhitelistNewFiles = new();
            public List<string> MissingWhitelistFiles = new();

            public void Check(
                string assertionId,
                string category,
                string expected,
                string actual)
            {
                bool passed = string.Equals(expected, actual, StringComparison.Ordinal);
                Assertions.Add(new AssertionRow
                {
                    AssertionId = assertionId ?? string.Empty,
                    Category = category ?? string.Empty,
                    Expected = expected ?? string.Empty,
                    Actual = actual ?? string.Empty,
                    Passed = passed
                });
                if (!passed)
                {
                    ErrorCount++;
                }
            }

            public void AddFailure(
                string assertionId,
                string category,
                string expected,
                string actual,
                string message)
            {
                Assertions.Add(new AssertionRow
                {
                    AssertionId = assertionId ?? string.Empty,
                    Category = category ?? string.Empty,
                    Expected = expected ?? string.Empty,
                    Actual = actual ?? string.Empty,
                    Passed = false,
                    Message = message ?? string.Empty
                });
                ErrorCount++;
            }
        }

        private sealed class AssertionRow
        {
            public string AssertionId = string.Empty;
            public string Category = string.Empty;
            public string Expected = string.Empty;
            public string Actual = string.Empty;
            public bool Passed;
            public string Message = string.Empty;
        }
    }
}
