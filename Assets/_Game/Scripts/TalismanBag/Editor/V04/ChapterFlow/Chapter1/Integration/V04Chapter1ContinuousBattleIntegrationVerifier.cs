using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.V04.ChapterFlow;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1.Integration
{
    public static class V04Chapter1ContinuousBattleIntegrationVerifier
    {
        private const string AssignmentPath =
            "Docs/V0.4/BLineChapter1ContinuousBattleIntegration01_Assignment.md";
        private const string AssignmentSha =
            "31c724c15d2539c042acca0b37b9708cb386db73c12a8bce02ddc8a69928b5b3";
        private const string InputsAggregate =
            "315090e977138c2befdb04f50e4f68ef4cea6bb8d641e9074c79494917ac603b";
        private const string LockedAggregate =
            "7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f";
        private const string GovernanceAggregate =
            "beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2";
        private const string FormalAggregate =
            "97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d";
        private const string ProjectSettingsAggregate =
            "9e3c370e01deaeb794933fb4d19103d613c5a270b8d9d211c42b4ce8971a76f5";
        private const string PassMarker =
            "BLINE_CHAPTER1_CONTINUOUS_BATTLE_INTEGRATION01_AUTOMATED_PASS stages=10 normalBattles=9 bossGates=1 manualBossChallenges=1 phase1TemporaryClears=1 sessionUnlocks=1 saveWrites=0 rewardGrants=0 legacyRefs=0 existingFileModifications=0";

        private static readonly string[] InputPaths =
        {
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterManifest.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1DevBossCompletionPolicy.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs",
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleStartRequest.cs",
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleResultSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/ShougunuPhase1BattleApplicationContracts.cs",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/ShougunuPhase1BattleApplicationEngine.cs",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/ShougunuPhase1BattleSandboxVerticalSliceRuntime.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs",
            "Docs/V0.4/Reports/BLineChapterFlowContractReport.md",
            "Docs/V0.4/Reports/BLineChapter1EncounterManifestReport.md",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeReport.md",
            "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md",
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractReport.md",
            "Docs/V0.4/Reports/RealItemDamageShougunuPhase1BattleSandboxVerticalSliceReport.md",
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity"
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

        private static readonly string[] OutputPaths =
        {
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration.meta",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowSession.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowSession.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1NormalEnemyBattleAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1NormalEnemyBattleAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BossPhase1CompletionAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BossPhase1CompletionAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxRuntimeController.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxRuntimeController.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxOverlay.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxOverlay.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousBattleIntegrationVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousBattleIntegrationVerifier.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousHandtestMenu.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousHandtestMenu.cs.meta",
            "Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationReport.md",
            "Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationSpec.csv",
            "Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationStageTrace.csv",
            "Docs/V0.4/Reports/BLineChapter1ContinuousBattleRequestResultMatrix.csv",
            "Docs/V0.4/Reports/BLineChapter1Phase1TemporaryClearReport.md",
            "Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationLeakCheckReport.md",
            "Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationManualTest.md",
            "Docs/V0.4/Reports/BLineChapter1ContinuousBattleSurfaceBindingMap.csv"
        };

        private static readonly string[] RuntimeSourcePaths =
            OutputPaths.Where(value =>
                    value.StartsWith(
                        "Assets/_Game/Scripts/TalismanBag/V04/",
                        StringComparison.Ordinal)
                    && value.EndsWith(".cs", StringComparison.Ordinal))
                .ToArray();

        private static readonly Dictionary<string, string> KeyHashes =
            new(StringComparer.Ordinal)
            {
                [InputPaths[0]] = "5102418f6da377f1d40d5feab53c7c1414042e41411e2126f0f744d78f073715",
                [InputPaths[3]] = "acb59867743de4eeced12810fa32d930f587cf195d0c1f7384734424b89bf416",
                [InputPaths[4]] = "e4df582ed79893e33e933746e126b3ceacb533b5d52444ec87d3012bed91cd73",
                [InputPaths[8]] = "930930f5b0ebc1c2f9966bf84add15117af79473110fc64e887b22fa41f2d283",
                [InputPaths[9]] = "6ea795694f303deea7397a65abc3a778e169bb3f9a7d013522eb1c19d926dd13",
                [InputPaths[11]] = "a6eeb38da79d77311b8f947f2adcf22db14aab2a9af6342a74b4a0dd0b7fb599",
                [InputPaths[14]] = "b7aac7d6e033212d4f3d2b242d663eb604104c7db5edaabe442e8b1c51f4ae17",
                [InputPaths[15]] = "9c3994f0f2b2bd48431e7255dfd93e1ba57ccf4decc3e670d0d426dbe837d822",
                [InputPaths[30]] = "f5d1d46c2d8aa3654072f9c431adf59a5f6f9d815a24965e2e27519c1914aa95",
                [InputPaths[31]] = "5f6028b9e4801094a540df9889d58a80208ee43cd9d0fcb6f6f117ee373b0313",
                [InputPaths[26]] = "496325305715701e553ee2a8b3523045c5d272a1a5adfdc32c24a8b71f681a03",
                [InputPaths[34]] = "119a0ac4a11630260ffeb003cf007472b0d2a792f6f0141c5c89ea76c53a20bd",
                [InputPaths[38]] = "be7e9536573cf7708f4d64961661c1fd60dd275f969e2b7a003a045b0d07306e"
            };

        [MenuItem(
            "Tools/TalismanBag/V0.4/Verify B-Line Chapter 1 Continuous Battle Integration 01")]
        public static void RunMenu()
        {
            Verification verification = Run();
            if (verification.Errors.Count > 0)
            {
                throw new InvalidOperationException(
                    "B-Line Chapter 1 continuous integration verification failed: "
                    + verification.Errors.Count.ToString(
                        CultureInfo.InvariantCulture));
            }
        }

        public static void RunBatch()
        {
            int exitCode = 1;
            try
            {
                Verification verification = Run();
                if (verification.Errors.Count > 0)
                {
                    throw new InvalidOperationException(
                        string.Join("\n", verification.Errors));
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

        private static Verification Run()
        {
            Verification verification = new()
            {
                Root = Path.GetFullPath(
                    Path.Combine(Application.dataPath, ".."))
            };
            try
            {
                VerifyFrozenInputs(verification);
                VerifyOutputs(verification);
                VerifyPureContract(verification);
                VerifySourceIsolation(verification);
                VerifyReports(verification);
                VerifySceneBoundary(verification);
            }
            catch (Exception exception)
            {
                verification.Errors.Add(
                    "UNHANDLED_" + exception.GetType().Name + ": "
                    + exception.Message);
            }

            if (verification.Errors.Count == 0)
            {
                Debug.Log(PassMarker);
            }
            else
            {
                Debug.LogError(
                    "BLINE_CHAPTER1_CONTINUOUS_BATTLE_INTEGRATION01_FAIL errors="
                    + Math.Max(1, verification.Errors.Count)
                        .ToString(CultureInfo.InvariantCulture));
                foreach (string error in verification.Errors)
                {
                    Debug.LogError(error);
                }
            }
            return verification;
        }

        private static void VerifyFrozenInputs(Verification verification)
        {
            Check(
                verification,
                "ASSIGNMENT_SHA",
                AssignmentSha,
                HashFile(verification.Root, AssignmentPath));
            Check(
                verification,
                "INPUT_COUNT",
                "39",
                InputPaths.Length.ToString(CultureInfo.InvariantCulture));
            Check(
                verification,
                "INPUT_AGGREGATE",
                InputsAggregate,
                Aggregate(
                    verification.Root,
                    InputPaths,
                    StringComparer.Ordinal));
            foreach (KeyValuePair<string, string> pair in KeyHashes)
            {
                Check(
                    verification,
                    "KEY_HASH_" + pair.Key,
                    pair.Value,
                    HashFile(verification.Root, pair.Key));
            }

            string[] locked = Directory.GetFiles(
                    Path.Combine(verification.Root, "Docs", "LOCKED"),
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Select(value => Relative(verification.Root, value))
                .ToArray();
            Check(
                verification,
                "LOCKED_COUNT",
                "15",
                locked.Length.ToString(CultureInfo.InvariantCulture));
            Check(
                verification,
                "LOCKED_AGGREGATE",
                LockedAggregate,
                Aggregate(
                    verification.Root,
                    locked,
                    StringComparer.Ordinal));
            Check(
                verification,
                "GOVERNANCE_AGGREGATE",
                GovernanceAggregate,
                Aggregate(
                    verification.Root,
                    GovernancePaths,
                    StringComparer.Ordinal));
            Check(
                verification,
                "FORMAL_AGGREGATE",
                FormalAggregate,
                Aggregate(
                    verification.Root,
                    FormalPaths,
                    StringComparer.Ordinal));

            string[] projectSettings = Directory.GetFiles(
                    Path.Combine(verification.Root, "ProjectSettings"),
                    "*",
                    SearchOption.TopDirectoryOnly)
                .Select(value => Relative(verification.Root, value))
                .ToArray();
            Check(
                verification,
                "PROJECTSETTINGS_COUNT",
                "21",
                projectSettings.Length.ToString(
                    CultureInfo.InvariantCulture));
            Check(
                verification,
                "PROJECTSETTINGS_AGGREGATE",
                ProjectSettingsAggregate,
                Aggregate(
                    verification.Root,
                    projectSettings,
                    StringComparer.OrdinalIgnoreCase));
        }

        private static void VerifyOutputs(Verification verification)
        {
            Check(
                verification,
                "OUTPUT_COUNT",
                "28",
                OutputPaths.Length.ToString(CultureInfo.InvariantCulture));
            foreach (string path in OutputPaths)
            {
                if (!File.Exists(Full(verification.Root, path)))
                {
                    verification.Errors.Add("MISSING_OUTPUT: " + path);
                }
            }

            string runtimeDirectory = Full(
                verification.Root,
                "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration");
            string editorDirectory = Full(
                verification.Root,
                "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration");
            HashSet<string> expectedInDirectories = new(
                OutputPaths.Where(path =>
                    path.StartsWith(
                        "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/",
                        StringComparison.Ordinal)
                    || path.StartsWith(
                        "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/",
                        StringComparison.Ordinal)),
                StringComparer.Ordinal);
            foreach (string file in Directory.GetFiles(
                runtimeDirectory,
                "*",
                SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetFiles(
                    editorDirectory,
                    "*",
                    SearchOption.TopDirectoryOnly)))
            {
                string relative = Relative(verification.Root, file);
                if (!expectedInDirectories.Contains(relative))
                {
                    verification.Errors.Add(
                        "UNEXPECTED_INTEGRATION_OUTPUT: " + relative);
                }
            }
        }

        private static void VerifyPureContract(Verification verification)
        {
            V04Chapter1ContinuousValidationResult validation =
                V04Chapter1ContinuousFlowValidation.ValidateNominal();
            verification.Errors.AddRange(validation.errors);
            Check(verification, "TRACE_STAGES", "10",
                validation.trace.Count.ToString(CultureInfo.InvariantCulture));
            Check(verification, "NORMAL_BATTLES", "9",
                validation.normalBattles.ToString(CultureInfo.InvariantCulture));
            Check(verification, "BOSS_GATES", "1",
                validation.bossGates.ToString(CultureInfo.InvariantCulture));
            Check(verification, "MANUAL_BOSS", "1",
                validation.manualBossChallenges.ToString(CultureInfo.InvariantCulture));
            Check(verification, "TEMPORARY_CLEAR", "1",
                validation.phase1TemporaryClears.ToString(CultureInfo.InvariantCulture));
            Check(verification, "SESSION_UNLOCK", "1",
                validation.sessionUnlocks.ToString(CultureInfo.InvariantCulture));
            Check(verification, "SAVE_WRITES", "0",
                validation.saveWrites.ToString(CultureInfo.InvariantCulture));
            Check(verification, "REWARD_GRANTS", "0",
                validation.rewardGrants.ToString(CultureInfo.InvariantCulture));
            Check(
                verification,
                "FEATURE_DEFAULT_FALSE",
                "False",
                V04ChapterFlowFeatureFlags.EnableBLineChapterFlow.ToString());
        }

        private static void VerifySourceIsolation(Verification verification)
        {
            string normal = File.ReadAllText(
                Full(
                    verification.Root,
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1NormalEnemyBattleAdapter.cs"),
                Encoding.UTF8);
            string controller = File.ReadAllText(
                Full(
                    verification.Root,
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxRuntimeController.cs"),
                Encoding.UTF8);
            string bossAdapter = File.ReadAllText(
                Full(
                    verification.Root,
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BossPhase1CompletionAdapter.cs"),
                Encoding.UTF8);
            string menu = File.ReadAllText(
                Full(
                    verification.Root,
                    "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousHandtestMenu.cs"),
                Encoding.UTF8);
            Require(
                verification,
                normal.Contains(
                    "ItemCombatEffectRequestAssembler.Instance.Assemble")
                && normal.Contains(
                    "IItemSystemBattleSandboxBoardAuthority")
                && normal.Contains(
                    "ItemCombatEffectRequestSnapshot")
                && normal.Contains(
                    "sourceProjectionSetCanonicalSignature")
                && normal.Contains("tick + 1L")
                && normal.Contains("handledEnemyRequestIds"),
                "REAL_ITEM_ENEMY_ADAPTER_STATIC_CONSUMPTION");
            Require(
                verification,
                !controller.Contains(
                    "HandleAuthoredBattleStateButton")
                && controller.Contains(
                    "SceneBinder.AuthoredResetButton.onClick.Invoke()")
                && controller.Contains(
                    "IsApprovedTemporaryTerminal")
                && controller.Contains("BossHandoffTimeoutSeconds")
                && controller.Contains("BOSS_HANDOFF_TIMEOUT"),
                "SHOUGUNU_AUTHORED_BUTTON_HANDOFF_WITH_BOUNDED_DIAGNOSTIC");
            int readinessIndex = controller.IndexOf(
                ".TryValidateBossReadiness",
                StringComparison.Ordinal);
            int manualChallengeIndex = controller.IndexOf(
                "flow.ConfirmManualBossChallenge()",
                StringComparison.Ordinal);
            int bossRequestIndex = controller.IndexOf(
                "flow.RequestCurrentBattle(",
                readinessIndex,
                StringComparison.Ordinal);
            int authoredButtonIndex = controller.IndexOf(
                "InvokeAcceptedAuthoredBossButtonOnce();",
                bossRequestIndex,
                StringComparison.Ordinal);
            Require(
                verification,
                readinessIndex >= 0
                && manualChallengeIndex > readinessIndex
                && bossRequestIndex > manualChallengeIndex
                && authoredButtonIndex > bossRequestIndex,
                "BOSS_READINESS_BEFORE_REDUCER_REQUEST_BEFORE_AUTHORED_BUTTON");
            Require(
                verification,
                bossAdapter.Contains(
                    "ShougunuPhase1BattleApplicationEngine.ItemIds")
                && bossAdapter.Contains(
                    "ItemCombatEffectRequestAssembler.Instance.Assemble")
                && bossAdapter.Contains(
                    "ItemCombatEffectRequestKind.DirectFlatDamage")
                && bossAdapter.Contains(
                    "resolvedPreMitigationDamageUnits <= 0")
                && bossAdapter.Contains("i031State?.isOwned")
                && bossAdapter.Contains("i031State?.isPlaced")
                && bossAdapter.Contains(
                    "ChapterFlow remains at BossGate"),
                "EXACT_SHOUGUNU_REAL_ITEM_READINESS_GATE");
            Require(
                verification,
                menu.Contains(
                    "EditorPrefs.GetBool")
                && menu.Contains("false)")
                && menu.Contains("HideFlags.DontSaveInEditor")
                && menu.Contains("HideFlags.DontSaveInBuild")
                && menu.Contains(".TargetScenePath"),
                "DEFAULT_OFF_DONTSAVE_EXACT_SCENE_INJECTION");

            string[] forbidden =
            {
                "Enemy" + "Definition",
                "Enemy" + "Group",
                "EnemySkill" + "Runtime",
                "Drop" + "Table",
                "AutoCombat" + "Controller",
                "V02RunFlow" + "Controller",
                "MainTrialFlow" + "Service",
                "Player" + "Prefs",
                "Save" + "Data",
                "Guid." + "NewGuid",
                "DateTime." + "Now",
                "DateTime." + "UtcNow",
                "UnityEngine." + "Random",
                "System." + "Random",
                "GetInstance" + "ID"
            };
            foreach (string path in RuntimeSourcePaths)
            {
                string source = File.ReadAllText(
                    Full(verification.Root, path),
                    Encoding.UTF8);
                foreach (string term in forbidden)
                {
                    if (source.Contains(term))
                    {
                        verification.Errors.Add(
                            "PROHIBITED_RUNTIME_REFERENCE: "
                            + path + " -> " + term);
                    }
                }
            }
        }

        private static void VerifyReports(Verification verification)
        {
            foreach (string path in OutputPaths.Where(
                value => value.StartsWith(
                    "Docs/V0.4/Reports/",
                    StringComparison.Ordinal)))
            {
                string source = File.ReadAllText(
                    Full(verification.Root, path),
                    Encoding.UTF8);
                Require(
                    verification,
                    source.Contains(
                        V04Chapter1ContinuousBattleIntegrationContract
                            .Phase1DevVerticalSlice)
                    && source.Contains(
                        V04Chapter1ContinuousBattleIntegrationContract
                            .NotContentFinal)
                    && source.Contains(
                        V04Chapter1ContinuousBattleIntegrationContract
                            .NotFormalBossCompletion),
                    "REPORT_LABELS_" + path);
            }
            string main = File.ReadAllText(
                Full(
                    verification.Root,
                    "Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationReport.md"),
                Encoding.UTF8);
            Require(
                verification,
                main.Contains(PassMarker)
                && main.Contains("USER_HANDTEST_FAILED")
                && main.Contains("USER_RETEST_READY")
                && main.Contains(
                    "AuthoredResetButton.onClick.Invoke()")
                && main.Contains(AssignmentSha),
                "MAIN_REPORT_CLOSURE");
            string manual = File.ReadAllText(
                Full(
                    verification.Root,
                    "Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationManualTest.md"),
                Encoding.UTF8);
            Require(
                verification,
                manual.Contains("1-1")
                && manual.Contains("1-9")
                && manual.Contains("1-10")
                && manual.Contains("I007")
                && manual.Contains("I012")
                && manual.Contains("I031")
                && manual.Contains("BossGate")
                && manual.Contains("authored Boss surface")
                && manual.Contains("visibly switch")
                && manual.Contains("exit/re-enter Play Mode"),
                "ONE_CONTINUOUS_HANDTEST");
        }

        private static void VerifySceneBoundary(Verification verification)
        {
            Check(
                verification,
                "SCENE_SHA",
                "be7e9536573cf7708f4d64961661c1fd60dd275f969e2b7a003a045b0d07306e",
                HashFile(
                    verification.Root,
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TargetScenePath));
            UnityEngine.SceneManagement.Scene activeScene =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                Require(
                    verification,
                    !activeScene.isDirty,
                    "ACTIVE_SCENE_NOT_DIRTY");
            }
        }

        private static void Check(
            Verification verification,
            string id,
            string expected,
            string actual)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
            {
                verification.Errors.Add(
                    id + " expected=" + expected + " actual=" + actual);
            }
        }

        private static void Require(
            Verification verification,
            bool condition,
            string id)
        {
            if (!condition)
            {
                verification.Errors.Add(id);
            }
        }

        private static string Aggregate(
            string root,
            IEnumerable<string> paths,
            IComparer<string> comparer)
        {
            string payload = string.Join(
                "\n",
                paths.Select(Normalize)
                    .OrderBy(value => value, comparer)
                    .Select(value => value + "|"
                        + HashFile(root, value))) + "\n";
            return HashBytes(Encoding.UTF8.GetBytes(payload));
        }

        private static string HashFile(string root, string path)
        {
            string full = Full(root, path);
            if (!File.Exists(full))
            {
                return "missing";
            }
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(full);
            return Bytes(sha.ComputeHash(stream));
        }

        private static string HashBytes(byte[] bytes)
        {
            using SHA256 sha = SHA256.Create();
            return Bytes(sha.ComputeHash(bytes));
        }

        private static string Bytes(byte[] bytes)
        {
            return string.Concat(bytes.Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static string Full(string root, string path)
        {
            return Path.GetFullPath(Path.Combine(
                root,
                Normalize(path).Replace(
                    '/',
                    Path.DirectorySeparatorChar)));
        }

        private static string Relative(string root, string full)
        {
            return Normalize(
                full.Substring(root.TrimEnd('\\', '/').Length + 1));
        }

        private static string Normalize(string path)
        {
            return (path ?? string.Empty)
                .Replace('\\', '/')
                .TrimStart('/');
        }

        private sealed class Verification
        {
            public string Root = string.Empty;
            public readonly List<string> Errors = new();
        }
    }
}
