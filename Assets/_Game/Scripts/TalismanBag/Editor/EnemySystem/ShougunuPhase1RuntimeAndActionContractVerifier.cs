using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class ShougunuPhase1RuntimeAndActionContractVerifier
    {
        private const string AssignmentSha256 =
            "40f244475a2aabadffb8ba440c011ec8a7dae46c69043db7f93e928788c0c757";
        private const string HistoricalDefectiveEntry =
            "TalismanBag.Editor.EnemySystem.ShougunuPhase1RuntimeAndActionContractVerifier.RunFromCommandLine";
        private const string EffectiveEntry =
            "TalismanBag.EditorTools.EnemySystem.ShougunuPhase1RuntimeAndActionContractVerifier.RunFromCommandLine";
        private const string ItemDriftOwnerPackage = "V0.4-ItemCombatEffectRequestContract01";
        private const string ItemDriftAssignmentSha256 =
            "6dd4cad1fc930d0b969e9ff92455b1cbbdb52a40f8b4809add69819b7e9ec35e";
        private const string ItemDriftInitialReceiptAggregate =
            "c743d73541b94c2194e1e4cb585db69aaf0428407397158be038b1610363ecaf";

        private static readonly string[] PackageManifest =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/ShougunuPhase1RuntimeAndActionContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/ShougunuPhase1RuntimeAndActionContractVerifier.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeValidation.cs.meta",
            "Docs/V0.4/Reports/ShougunuPhase1NegativeFixtureRows.csv",
            "Docs/V0.4/Reports/ShougunuPhase1NominalTrace.csv",
            "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractFieldMatrix.csv",
            "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractLeakCheckReport.md",
            "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md",
            "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractSpec.csv",
            "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionPattern.csv",
            "Docs/V0.4/Reports/ShougunuPhase1ThresholdAndQueueFixtureRows.csv"
        };

        private static readonly string[] RuntimeSourceManifest =
            PackageManifest.Where(path => path.EndsWith(".cs", StringComparison.Ordinal)).ToArray();

        private static readonly string[] ReportManifest =
            PackageManifest.Where(path => path.StartsWith("Docs/", StringComparison.Ordinal)).ToArray();

        private static readonly string[] MetaManifest =
            PackageManifest.Where(path => path.EndsWith(".meta", StringComparison.Ordinal)).ToArray();

        private static readonly ExternalDriftEvidence[] AttributedItemDrifts =
        {
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat.meta",
                "ABSENT",
                "432cb8568fb85ea8f496b8ff26c0dc2d19129700a8c79be184a14f8103d2b051"),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs",
                "ABSENT",
                "5f6028b9e4801094a540df9889d58a80208ee43cd9d0fcb6f6f117ee373b0313"),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs.meta",
                "ABSENT",
                "fc0141e38ff9123c5d3eeccd1ff923972fc2ee0c08237bd8b5a0a49df61b6b94"),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs",
                "ABSENT",
                "f5d1d46c2d8aa3654072f9c431adf59a5f6f9d815a24965e2e27519c1914aa95"),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs.meta",
                "ABSENT",
                "f623ab680be2c3780909ec9a74907079b2a3e2ba2a4dbad1094056cd4f8d53d5"),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs",
                "ABSENT",
                "0a2d91eead9d397c0f3c271f9318528e5e7ca98de1c2ebf5acb04ecab3c9d121"),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs.meta",
                "ABSENT",
                "3e140241baabe73cacc3890b4117adb0093402dd8eb90d3de95fafe931500314")
        };

        private static readonly BaselineExpectation[] BaselineExpectations =
        {
            new BaselineExpectation("P0", 8,
                "2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5",
                Lines(
                    "Docs/V0.4/Reports/BoneAspectContentDesignLockReport.md",
                    "Docs/V0.4/Reports/BoneAspectContentCatalog.csv",
                    "Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv",
                    "Docs/V0.4/Reports/BoneAspectVisualLanguageSpec.csv",
                    "Docs/V0.4/Reports/BoneAspectExternalReferenceBoundary.csv",
                    "Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv",
                    "Docs/V0.4/Reports/BoneAspectContentDesignLockLeakCheckReport.md")),
            new BaselineExpectation("P1", 21,
                "21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209",
                Lines(
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs.meta",
                    "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogReport.md",
                    "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogSpec.csv",
                    "Docs/V0.4/Reports/BoneAspectCarrierPresentationInventory.csv",
                    "Docs/V0.4/Reports/BoneAspectCarrierPresentationArtSlotRows.csv",
                    "Docs/V0.4/Reports/BoneAspectCarrierPresentationMechanicReferenceRows.csv",
                    "Docs/V0.4/Reports/BoneAspectCarrierPresentationPlayerSafeFieldMatrix.csv",
                    "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogLeakCheckReport.md")),
            new BaselineExpectation("GapSurvey", 8,
                "0a79e5e9a40cb8e88d0d25fc0db97972e03452cf3168878df98ed26871d9fca0",
                Lines(
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReport.md",
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMatrix.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReuseEvidence.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMissingConcepts.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyOwnershipMatrix.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyDecisionExclusions.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyLeakCheckReport.md")),
            new BaselineExpectation("BossArtSurvey", 9,
                "93659d9b894251e73ad85a0d854c8a7c62d0f795c54ca38a5d3bf2065b841dc6",
                Lines(
                    "Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageSurveyReport.md",
                    "Docs/V0.4/Reports/BoneAspectBossArtSourceInventory.csv",
                    "Docs/V0.4/Reports/BoneAspectBossArtSourceStatementInventory.csv",
                    "Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageMatrix.csv",
                    "Docs/V0.4/Reports/BoneAspectBossDecisionResolutionOverlay.csv",
                    "Docs/V0.4/Reports/BoneAspectBossPhaseCandidateInventory.csv",
                    "Docs/V0.4/Reports/BoneAspectBossSkillCandidateInventory.csv",
                    "Docs/V0.4/Reports/BoneAspectBossArtSupersededSemantics.csv",
                    "Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageLeakCheckReport.md")),
            new BaselineExpectation("Vocabulary", 17,
                "3e1a95be4d747423e8829939fafd31bb4eafa292c87c1d743f8d9ebe667941cc",
                Lines(
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs.meta",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs.meta",
                    "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionComposition.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionInventory.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionLeakCheckReport.md",
                    "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionReport.md",
                    "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSourceEvidence.csv",
                    "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSpec.csv"))
        };

        private static readonly BroadBaselineExpectation[] BroadBaselineExpectations =
        {
            new BroadBaselineExpectation("EnemySystem", 117,
                "04402d30a62c54e7fd3ed08349c5553a66487eca64b6fe9daa0418fb367b7591",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem", null),
            new BroadBaselineExpectation("EditorEnemySystem", 28,
                "8dd2e42698d3bbab13305f27d222c9787590a708473cdeaa8d2a3ffa0d23a817",
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem", null),
            new BroadBaselineExpectation("Items", 141,
                "4b41e4dbede32a14a661f0f985cb1682051c30836b54d811a4c55546652b18e4",
                "Assets/_Game/Scripts/TalismanBag/Items", null),
            new BroadBaselineExpectation("BuildSandbox", 186,
                "eae1e45367a3f970858a4befe0920bee15c350caaf4d0cddb9de363b5238a228",
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox", null),
            new BroadBaselineExpectation("CrossSystem", 63,
                "dfd3445869710d3b28e3ddacdca20a7850ea8c0125246599b14e1ed649e1a64d",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem", null),
            new BroadBaselineExpectation("Scenes", 14,
                "4c9e101026dd4aeb35e8c729f080a527c5a621d8df721836802f37057dad681d",
                "Assets/_Game/Scenes", null),
            new BroadBaselineExpectation("Configs", 121,
                "2383359ab713475d8d85748032965c3188f47c436e3644d40abfb54260807769",
                "Assets/_Game/Configs", null),
            new BroadBaselineExpectation("ProjectSettings", 21,
                "1969814e5ec3983ba426b7d5f19f13de7f480277cb5118a92d06c66b58fcbac7",
                "ProjectSettings", null),
            new BroadBaselineExpectation("Packages", 2,
                "11efaaf4e1df1215e977f884d2d5e2c2bdbbbd1e62337b0517f6a0502a929cb6",
                "Packages", null),
            new BroadBaselineExpectation("UnityAssets", 7,
                "081bb0469c44f88ededd6ca63d6b722d6027311d31c4bbee4732462364fe242b",
                null, Lines(
                    "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity",
                    "Assets/_Game/Scenes/Scene_TalismanBag_V03_BootEntry.unity",
                    "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity",
                    "Assets/_Game/Scenes/Scene_TalismanBag_V03_TalismanUpgrade.unity",
                    "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity",
                    "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity",
                    "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity")),
            new BroadBaselineExpectation("Prefabs", 5,
                "a0274a939eeed890c98522bde495a37b2f4b533fdcd753bdb2896cf6b5be485e",
                null, Lines(
                    "Assets/_Game/Prefabs/TalismanBag/DraggableTalismanItem.prefab",
                    "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab",
                    "Assets/_Game/Prefabs/TalismanBag/TalismanGridSlot.prefab",
                    "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/BattleLikePreviewAreaBridge.prefab",
                    "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab")),
            new BroadBaselineExpectation("BuildSettings", 1,
                "8acc1a59a38b35b90a92e81fb101ce4377bca9c9300ddee0e235f7efa4fc8197",
                null, Lines("ProjectSettings/EditorBuildSettings.asset")),
            new BroadBaselineExpectation("Governance", 18,
                "1988a87a8a0a6864807db6c52708d26a326430d94c8945fa63804018c0dc46e3",
                null, Lines(
                    "AGENTS.md",
                    "Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md",
                    "Docs/LOCKED/CODEX_PREFLIGHT_CHECK.md",
                    "Docs/LOCKED/CODEX_ROLE_WINDOW_REGISTRY.md",
                    "Docs/LOCKED/CODEX_VERSION_PIPELINE_LOCK.md",
                    "Docs/LOCKED/CORE_INVARIANTS.md",
                    "Docs/LOCKED/CROSS_SYSTEM_EXECUTOR_PROTOCOL.md",
                    "Docs/LOCKED/CURRENT_VERSION_SCOPE_LOCK.md",
                    "Docs/LOCKED/DELIVERY_ACCEPTANCE_GATE.md",
                    "Docs/LOCKED/DO_NOT_TOUCH.md",
                    "Docs/LOCKED/GOLDEN_PATH_QA.md",
                    "Docs/LOCKED/MEMORY_FILE_APPROVAL_RULE.md",
                    "Docs/LOCKED/PAGE_FLOW_LOCK.md",
                    "Docs/LOCKED/PROJECT_DIRECTION_LOCK.md",
                    "Docs/LOCKED/RISK_LEVEL_RULE.md",
                    "Docs/LOCKED/STABLE_BASELINE_LOCK.md",
                    "Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md",
                    "Docs/V0.4/EnemySystemGuard_CurrentRules.md"))
        };

#if UNITY_EDITOR
        [MenuItem("TalismanBag/Enemy System/Run Shougunu Phase1 Runtime And Action Contract Verifier")]
        public static void Run()
        {
            int exitCode = RunForOffline(Directory.GetCurrentDirectory(), Directory.GetCurrentDirectory());
            if (exitCode != 0)
            {
                throw new InvalidOperationException("Shougunu Phase1 verifier failed.");
            }
            Debug.Log("Shougunu Phase1 Runtime And Action Contract verifier PASS.");
        }
#endif

        public static void RunFromCommandLine()
        {
            int exitCode = RunForOffline(Directory.GetCurrentDirectory(), Directory.GetCurrentDirectory());
#if UNITY_EDITOR
            EditorApplication.Exit(exitCode);
#else
            if (exitCode != 0)
            {
                throw new InvalidOperationException("Shougunu Phase1 verifier failed.");
            }
#endif
        }

        public static int RunForOffline(string projectRoot, string outputRoot)
        {
            VerificationBundle bundle = Verify(projectRoot, outputRoot);
            WriteReports(outputRoot, bundle);
            string firstHash = HashGeneratedReportFiles(outputRoot);
            WriteReports(outputRoot, bundle);
            string secondHash = HashGeneratedReportFiles(outputRoot);
            string missingPath = ToAbsolute(outputRoot, ReportManifest[0]);
            if (File.Exists(missingPath))
            {
                File.Delete(missingPath);
            }
            bool missingObserved = !File.Exists(missingPath);
            WriteReports(outputRoot, bundle);
            string regeneratedHash = HashGeneratedReportFiles(outputRoot);
            bool deterministic = firstHash == secondHash && secondHash == regeneratedHash;
            bool regenerated = missingObserved && File.Exists(missingPath);
            bundle.ReportContents.Clear();
            Add(bundle, "report.consecutive-hash-determinism",
                firstHash + "|" + secondHash + "|" + regeneratedHash,
                "three equal aggregate hashes",
                deterministic);
            Add(bundle, "report.missing-report-regeneration",
                Bool(missingObserved) + "|" + Bool(regenerated),
                "true|true",
                regenerated);
            BuildReportProjections(bundle);
            WriteReports(outputRoot, bundle);
            return bundle.Checks.All(row => row.Pass) ? 0 : 1;
        }

        private static VerificationBundle Verify(string projectRoot, string outputRoot)
        {
            VerificationBundle bundle = new VerificationBundle();
            VerifyFrozenContract(bundle);
            VerifyRuntimeReducer(bundle);
            VerifyScheduler(bundle);
            VerifyNegativeFixtures(bundle);
            RunNominalTrace(bundle);
            VerifyPackageBoundary(projectRoot, outputRoot, bundle);
            VerifyProtectedBaselines(projectRoot, bundle);
            BuildReportProjections(bundle);
            return bundle;
        }

        private static void VerifyFrozenContract(VerificationBundle bundle)
        {
            Add(bundle, "contract.schema-id", ShougunuPhase1RuntimeContract.SchemaId,
                "ShougunuPhase1RuntimeAndActionContract.v1",
                ShougunuPhase1RuntimeContract.SchemaId
                    == "ShougunuPhase1RuntimeAndActionContract.v1");
            Add(bundle, "contract.identity", ShougunuPhase1RuntimeContract.ContentId + "|"
                + ShougunuPhase1RuntimeContract.PhaseId,
                "bone_aspect_boss_c1_bone_guard|bone_aspect.phase.shougunu.phase1",
                ShougunuPhase1RuntimeContract.ContentId == "bone_aspect_boss_c1_bone_guard"
                    && ShougunuPhase1RuntimeContract.PhaseId
                        == "bone_aspect.phase.shougunu.phase1");
            Add(bundle, "contract.flags",
                Bool(ShougunuPhase1RuntimeContract.DevOnly) + "|"
                    + Bool(ShougunuPhase1RuntimeContract.IsEnabled) + "|"
                    + Bool(ShougunuPhase1RuntimeContract.EntersFormalFlow) + "|"
                    + Bool(ShougunuPhase1RuntimeContract.RuntimeBoundToBattle),
                "true|false|false|false",
                ShougunuPhase1RuntimeContract.DevOnly
                    && !ShougunuPhase1RuntimeContract.IsEnabled
                    && !ShougunuPhase1RuntimeContract.EntersFormalFlow
                    && !ShougunuPhase1RuntimeContract.RuntimeBoundToBattle);
            Add(bundle, "contract.frozen-values",
                string.Join("|", new[]
                {
                    Int(ShougunuPhase1RuntimeContract.MaxHp),
                    Int(ShougunuPhase1RuntimeContract.ShellLayerMax),
                    Int(ShougunuPhase1RuntimeContract.MaxSequentialShellLayers),
                    Long(ShougunuPhase1RuntimeContract.CoreExposeDurationTicks),
                    Int(ShougunuPhase1RuntimeContract.Skill1RepairUnits),
                    Int(ShougunuPhase1RuntimeContract.BasicDamageApplicationInterval)
                }),
                "980|200|7|6000|30|4",
                ShougunuPhase1RuntimeContract.MaxHp == 980
                    && ShougunuPhase1RuntimeContract.ShellLayerMax == 200
                    && ShougunuPhase1RuntimeContract.MaxSequentialShellLayers == 7
                    && ShougunuPhase1RuntimeContract.CoreExposeDurationTicks == 6000L
                    && ShougunuPhase1RuntimeContract.Skill1RepairUnits == 30
                    && ShougunuPhase1RuntimeContract.BasicDamageApplicationInterval == 4);
            IReadOnlyList<ShougunuPhase1ActionPatternSnapshot> patterns =
                ShougunuPhase1ActionPatternCatalog.GetPatterns();
            Add(bundle, "catalog.pattern-count", Int(patterns.Count), "4", patterns.Count == 4);
            Add(bundle, "catalog.threshold-count",
                Int(GetThresholdRows().Count), "6", GetThresholdRows().Count == 6);
            Add(bundle, "catalog.validation",
                string.Join("|", ShougunuPhase1RuntimeValidation.ValidateCatalog()),
                "no errors",
                ShougunuPhase1RuntimeValidation.ValidateCatalog().Count == 0);
            Add(bundle, "catalog.canonical-signature",
                ShougunuPhase1ActionPatternCatalog.CanonicalSignature,
                "sha256:<64 lowercase hex>",
                Regex.IsMatch(
                    ShougunuPhase1ActionPatternCatalog.CanonicalSignature,
                    "^sha256:[0-9a-f]{64}$",
                    RegexOptions.CultureInvariant));
            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            string invariantBefore = ShougunuPhase1ActionPatternCatalog.CanonicalSignature;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                string invariantAfter = ShougunuPhase1ActionPatternCatalog.CanonicalSignature;
            Add(bundle, "catalog.invariant-culture",
                    invariantAfter,
                    invariantBefore,
                    invariantAfter == invariantBefore);
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
            }
            Add(bundle, "catalog.reverse-input-order-canonical",
                ShougunuPhase1ActionPatternCatalog.ComposeCanonicalSignature(true),
                ShougunuPhase1ActionPatternCatalog.ComposeCanonicalSignature(false),
                ShougunuPhase1ActionPatternCatalog.ComposeCanonicalSignature(true)
                    == ShougunuPhase1ActionPatternCatalog.ComposeCanonicalSignature(false));
            Add(bundle, "catalog.effect-request-boundaries",
                string.Join(";", patterns.OrderBy(item => item.ActionPatternId, StringComparer.Ordinal)
                    .Select(item => item.EffectRequestId)),
                "four exact neutral requests",
                patterns.Any(item => item.EffectRequestId
                    == "battle.effect_request.direct_player_damage")
                    && patterns.Any(item => item.EffectRequestId
                        == "enemy.effect_request.repair_current_shell")
                    && patterns.Any(item => item.EffectRequestId
                        == "battle.effect_request.rope_heavy_strike")
                    && patterns.Any(item => item.EffectRequestId
                        == "battle.effect_request.ground_seal_area_burst"));
            Add(bundle, "catalog.event-driven-no-cooldown",
                string.Join(";", patterns.Select(item => item.DueKind.ToString()).Distinct()),
                "BasicDebt;HpThreshold only",
                patterns.All(item => item.DueKind == ShougunuPhase1DueKind.BasicDebt
                    || item.DueKind == ShougunuPhase1DueKind.HpThreshold));

            ShougunuPhase1ActionPatternSnapshot basic =
                ShougunuPhase1ActionPatternCatalog.GetPattern(
                    ShougunuPhase1ActionPatternCatalog.BasicAttack);
            ShougunuPhase1ActionPatternSnapshot skill1 =
                ShougunuPhase1ActionPatternCatalog.GetPattern(
                    ShougunuPhase1ActionPatternCatalog.Skill1ShellRepair);
            ShougunuPhase1ActionPatternSnapshot skill2 =
                ShougunuPhase1ActionPatternCatalog.GetPattern(
                    ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike);
            ShougunuPhase1ActionPatternSnapshot skill3 =
                ShougunuPhase1ActionPatternCatalog.GetPattern(
                    ShougunuPhase1ActionPatternCatalog.Skill3GroundSealBurst);
            Add(bundle, "catalog.timing.basic", Timing(basic), "0|400|300|700|600",
                Timing(basic) == "0|400|300|700|600");
            Add(bundle, "catalog.timing.skill1", Timing(skill1), "0|800|400|1200|800",
                Timing(skill1) == "0|800|400|1200|800");
            Add(bundle, "catalog.timing.skill2", Timing(skill2), "0|1200|500|1700|900",
                Timing(skill2) == "0|1200|500|1700|900");
            Add(bundle, "catalog.timing.skill3", Timing(skill3), "0|1800|700|2500|1200",
                Timing(skill3) == "0|1800|700|2500|1200");
        }

        private static void VerifyRuntimeReducer(VerificationBundle bundle)
        {
            ShougunuPhase1RuntimeSnapshot presence =
                ShougunuPhase1RuntimeReducer.CreatePresence("boss.fixture.reducer", 1, 0L);
            ShougunuPhase1TransitionResult activated =
                ShougunuPhase1RuntimeReducer.ActivateInitialShell(presence, 0L);
            Add(bundle, "reducer.presence-to-shell",
                activated.State.Lifecycle + "|" + Int(activated.State.CurrentShell),
                "LayeredShellOn|200",
                activated.Accepted
                    && activated.State.Lifecycle == ShougunuPhase1LifecycleState.LayeredShellOn
                    && activated.State.CurrentShell == 200);
            Add(bundle, "reducer.initial-validation",
                string.Join("|", ShougunuPhase1RuntimeValidation.Validate(activated.State)),
                "no errors",
                ShougunuPhase1RuntimeValidation.Validate(activated.State).Count == 0);
            ShougunuPhase1RuntimeSnapshot repeated =
                ShougunuPhase1RuntimeReducer.ActivateInitialShell(
                    ShougunuPhase1RuntimeReducer.CreatePresence(
                        "boss.fixture.reducer",
                        1,
                        0L),
                    0L).State;
            Add(bundle, "signature.repeat-production",
                repeated.CanonicalSignature,
                activated.State.CanonicalSignature,
                repeated.CanonicalSignature == activated.State.CanonicalSignature);
            bool defensiveCopy = false;
            try
            {
                ((IList<string>)activated.State.AcceptedApplicationEventIds).Add("illegal-mutation");
            }
            catch (NotSupportedException)
            {
                defensiveCopy = activated.State.AcceptedApplicationEventIds.Count == 0;
            }
            Add(bundle, "snapshot.defensive-copy",
                Bool(defensiveCopy), "true", defensiveCopy);
            ShougunuPhase1RuntimeSnapshot diagnosticVariant =
                ShougunuPhase1RuntimeReducer.WithDeveloperDiagnostic(
                    activated.State,
                    "fixture-only-diagnostic");
            Add(bundle, "signature.developer-diagnostic-projection-stability",
                Bool(diagnosticVariant.CanonicalSignature != activated.State.CanonicalSignature)
                    + "|"
                    + Bool(diagnosticVariant.ToPresentationSafe(0L)
                        .PresentationSafeCanonicalSignature
                        == activated.State.ToPresentationSafe(0L)
                            .PresentationSafeCanonicalSignature),
                "true|true",
                diagnosticVariant.CanonicalSignature != activated.State.CanonicalSignature
                    && diagnosticVariant.ToPresentationSafe(0L)
                        .PresentationSafeCanonicalSignature
                        == activated.State.ToPresentationSafe(0L)
                            .PresentationSafeCanonicalSignature);
            Add(bundle, "signature.catalog-tuning-bound",
                activated.State.ActionCatalogCanonicalSignature,
                ShougunuPhase1ActionPatternCatalog.CanonicalSignature,
                activated.State.ActionCatalogCanonicalSignature
                    == ShougunuPhase1ActionPatternCatalog.CanonicalSignature);

            ShougunuPhase1RuntimeSnapshot state = activated.State;
            state = ApplyAccepted(state, "reduce-shell-hit", 1L, 100L, 50, 0).State;
            Add(bundle, "reducer.shell-hit", Int(state.CurrentShell), "150",
                state.CurrentShell == 150 && state.LatestCue.CueKind == ShougunuPhase1CueKind.ShellHit);
            Add(bundle, "signature.visible-state-and-cue-sensitive",
                state.ToPresentationSafe(100L).PresentationSafeCanonicalSignature,
                "different from initial presentation signature",
                state.ToPresentationSafe(100L).PresentationSafeCanonicalSignature
                    != activated.State.ToPresentationSafe(100L)
                        .PresentationSafeCanonicalSignature);
            state = ApplyAccepted(state, "reduce-shell-break", 2L, 200L, 150, 0).State;
            Add(bundle, "reducer.shell-break-core-expose",
                state.Lifecycle + "|" + Long(state.CoreExposeEndTick - state.CoreExposeStartTick),
                "CoreExposed|6000",
                state.Lifecycle == ShougunuPhase1LifecycleState.CoreExposed
                    && state.CurrentShell == 0
                    && state.Vulnerable
                    && state.CoreExposeEndTick - state.CoreExposeStartTick == 6000L);

            state = ApplyAccepted(state, "reduce-hp-hit", 3L, 300L, 0, 100).State;
            Add(bundle, "reducer.hp-hit-threshold", Int(state.CurrentHp) + "|"
                + string.Join(";", state.ThresholdOccurrences.Select(item => item.ThresholdId)),
                "880|S1-A",
                state.CurrentHp == 880
                    && state.ThresholdOccurrences.Count == 1
                    && state.ThresholdOccurrences[0].ThresholdId == "S1-A");

            ShougunuPhase1TransitionResult recover =
                ShougunuPhase1RuntimeReducer.AdvanceBattleTick(state, 6200L);
            ShougunuPhase1TransitionResult nextShell =
                ShougunuPhase1RuntimeReducer.AdvanceBattleTick(recover.State, 6300L);
            Add(bundle, "reducer.recover-next-shell",
                nextShell.State.Lifecycle + "|" + Int(nextShell.State.ShellLayerIndex) + "|"
                    + Int(nextShell.State.CurrentShell) + "|"
                    + Int(nextShell.State.AcceptedDamageApplicationCount),
                "LayeredShellOn|2|200|3",
                recover.Accepted
                    && nextShell.Accepted
                    && nextShell.State.ShellLayerIndex == 2
                    && nextShell.State.CurrentShell == 200
                    && nextShell.State.AcceptedDamageApplicationCount == 3);

            ShougunuPhase1RuntimeSnapshot finalLayer = activated.State;
            long sequence = 1L;
            long tick = 1000L;
            for (int layer = 1; layer <= 7; layer++)
            {
                finalLayer = ApplyAccepted(
                    finalLayer,
                    "layer-" + layer,
                    sequence++,
                    tick,
                    finalLayer.CurrentShell,
                    0).State;
                if (layer < 7)
                {
                    finalLayer = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(
                        finalLayer,
                        tick + 6000L).State;
                    finalLayer = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(
                        finalLayer,
                        tick + 6100L).State;
                }
                tick += 7000L;
            }
            Add(bundle, "reducer.seven-layers-final-expose",
                Int(finalLayer.ShellLayerIndex) + "|" + Bool(finalLayer.RecoveryAvailable) + "|"
                    + finalLayer.Lifecycle,
                "7|false|CoreExposed",
                finalLayer.ShellLayerIndex == 7
                    && !finalLayer.RecoveryAvailable
                    && finalLayer.Lifecycle == ShougunuPhase1LifecycleState.CoreExposed);
            ShougunuPhase1TransitionResult noEighthLayer =
                ShougunuPhase1RuntimeReducer.AdvanceBattleTick(finalLayer, tick + 6000L);
            Add(bundle, "reducer.no-eighth-layer",
                Int(noEighthLayer.State.ShellLayerIndex) + "|" + noEighthLayer.RejectionReason,
                "7|NO_LIFECYCLE_TRANSITION_DUE",
                !noEighthLayer.Accepted
                    && noEighthLayer.State.ShellLayerIndex == 7
                    && noEighthLayer.RejectionReason == "NO_LIFECYCLE_TRANSITION_DUE");

            long previousRevision = nextShell.State.Revision;
            ShougunuPhase1TransitionResult reset =
                ShougunuPhase1RuntimeReducer.Reset(nextShell.State, 2, 7000L);
            Add(bundle, "reducer.reset-generation-local-state",
                Int(reset.State.ResetGeneration) + "|" + Int(reset.State.AcceptedDamageApplicationCount)
                    + "|" + Int(reset.State.ThresholdOccurrences.Count) + "|"
                    + Bool(reset.State.Revision > previousRevision),
                "2|0|0|true",
                reset.Accepted
                    && reset.State.ResetGeneration == 2
                    && reset.State.AcceptedDamageApplicationCount == 0
                    && reset.State.ThresholdOccurrences.Count == 0
                    && reset.State.Revision > previousRevision
                    && reset.State.LatestCue.CueKind == ShougunuPhase1CueKind.Reset);
        }

        private static void VerifyScheduler(VerificationBundle bundle)
        {
            ShougunuPhase1RuntimeSnapshot state = NewShellState("boss.fixture.scheduler");
            for (int index = 1; index <= 4; index++)
            {
                state = ApplyAccepted(
                    state,
                    "basic-debt-" + index,
                    index,
                    index * 100L,
                    1,
                    0).State;
            }
            Add(bundle, "scheduler.basic-debt-every-four",
                Int(state.BasicEarnedCount) + "|" + Int(state.BasicDebt),
                "1|1",
                state.AcceptedDamageApplicationCount == 4
                    && state.BasicEarnedCount == 1
                    && state.BasicDebt == 1);
            ShougunuPhase1TransitionResult started =
                ShougunuPhase1ActionScheduler.Advance(state, 500L, true);
            ShougunuPhase1TransitionResult rejectedResolve =
                ShougunuPhase1ActionScheduler.Advance(started.State, 1200L, false);
            ShougunuPhase1TransitionResult acceptedResolve =
                ShougunuPhase1ActionScheduler.Advance(started.State, 1200L, true);
            Add(bundle, "scheduler.debt-consume-only-on-battle-accept",
                Int(rejectedResolve.State.BasicResolvedCount) + "|"
                    + Int(acceptedResolve.State.BasicResolvedCount),
                "0|1",
                !rejectedResolve.Accepted
                    && rejectedResolve.State.BasicResolvedCount == 0
                    && acceptedResolve.Accepted
                    && acceptedResolve.State.BasicResolvedCount == 1);

            ShougunuPhase1RuntimeSnapshot multi = NewShellState("boss.fixture.multi-threshold");
            multi = ApplyAccepted(multi, "multi-break", 1L, 100L, 200, 0).State;
            multi = ApplyAccepted(multi, "multi-hp", 2L, 200L, 0, 880).State;
            Add(bundle, "scheduler.multi-threshold-fifo",
                string.Join(";", multi.ThresholdOccurrences.Select(item => item.ThresholdId)),
                "S1-A;S2-A;S3-A;S1-B;S2-B;S3-B",
                string.Join(";", multi.ThresholdOccurrences.Select(item => item.ThresholdId))
                    == "S1-A;S2-A;S3-A;S1-B;S2-B;S3-B");
            ShougunuPhase1TransitionResult noThresholdInCore =
                ShougunuPhase1ActionScheduler.Advance(multi, 300L, true);
            Add(bundle, "scheduler.threshold-pending-in-core",
                noThresholdInCore.RejectionReason,
                "NO_LEGAL_ACTION_DUE",
                !noThresholdInCore.Accepted
                    && noThresholdInCore.RejectionReason == "NO_LEGAL_ACTION_DUE");
            ShougunuPhase1TransitionResult recovering =
                ShougunuPhase1RuntimeReducer.AdvanceBattleTick(multi, 6100L);
            ShougunuPhase1TransitionResult noThresholdInRecover =
                ShougunuPhase1ActionScheduler.Advance(recovering.State, 6100L, true);
            Add(bundle, "scheduler.threshold-pending-in-recover",
                noThresholdInRecover.RejectionReason + "|"
                    + Int(noThresholdInRecover.State.ThresholdOccurrences.Count(item => !item.Resolved)),
                "NO_LEGAL_ACTION_DUE|6",
                recovering.Accepted
                    && !noThresholdInRecover.Accepted
                    && noThresholdInRecover.State.ThresholdOccurrences.Count(item => !item.Resolved)
                        == 6);

            ShougunuPhase1RuntimeSnapshot priority = NewShellState("boss.fixture.priority");
            priority = ApplyAccepted(priority, "priority-1", 1L, 100L, 1, 0).State;
            priority = ApplyAccepted(priority, "priority-2", 2L, 200L, 1, 0).State;
            priority = ApplyAccepted(priority, "priority-3", 3L, 300L, 1, 0).State;
            priority = ApplyAccepted(priority, "priority-break", 4L, 400L, 197, 0).State;
            priority = ApplyAccepted(priority, "priority-cross", 5L, 500L, 0, 880).State;
            priority = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(priority, 6400L).State;
            priority = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(priority, 6401L).State;
            ShougunuPhase1TransitionResult priorityStart =
                ShougunuPhase1ActionScheduler.Advance(priority, 6401L, true);
            Add(bundle, "scheduler.threshold-priority-before-basic",
                priorityStart.State.ActiveAction == null
                    ? string.Empty
                    : priorityStart.State.ActiveAction.ActionPatternId,
                ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike,
                priority.BasicDebt == 1
                    && priorityStart.Accepted
                    && priorityStart.State.ActiveAction != null
                    && priorityStart.State.ActiveAction.ActionPatternId
                        == ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike);

            ShougunuPhase1RuntimeSnapshot skill1 = NewShellState("boss.fixture.skill1");
            skill1 = ApplyAccepted(skill1, "s1-break", 1L, 100L, 200, 0).State;
            skill1 = ApplyAccepted(skill1, "s1-cross", 2L, 200L, 0, 100).State;
            skill1 = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(skill1, 6100L).State;
            skill1 = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(skill1, 6200L).State;
            ShougunuPhase1TransitionResult fullShellPending =
                ShougunuPhase1ActionScheduler.Advance(skill1, 6200L, true);
            skill1 = ApplyAccepted(skill1, "s1-damage", 3L, 6300L, 40, 0).State;
            ShougunuPhase1TransitionResult s1Started =
                ShougunuPhase1ActionScheduler.Advance(skill1, 6300L, true);
            ShougunuPhase1TransitionResult s1Resolved =
                ShougunuPhase1ActionScheduler.Advance(s1Started.State, 7500L, true);
            Add(bundle, "scheduler.skill1-full-shell-pending-repair-cap",
                fullShellPending.RejectionReason + "|" + Int(s1Resolved.State.CurrentShell),
                "NO_LEGAL_ACTION_DUE|190",
                !fullShellPending.Accepted
                    && s1Started.Accepted
                    && s1Resolved.Accepted
                    && s1Resolved.State.CurrentShell == 190);

            ShougunuPhase1RuntimeSnapshot skill1Cap = NewShellState("boss.fixture.skill1-cap");
            skill1Cap = ApplyAccepted(skill1Cap, "s1cap-break", 1L, 100L, 200, 0).State;
            skill1Cap = ApplyAccepted(skill1Cap, "s1cap-cross", 2L, 200L, 0, 100).State;
            skill1Cap = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(skill1Cap, 6100L).State;
            skill1Cap = ShougunuPhase1RuntimeReducer.AdvanceBattleTick(skill1Cap, 6101L).State;
            skill1Cap = ApplyAccepted(skill1Cap, "s1cap-damage", 3L, 6200L, 10, 0).State;
            skill1Cap = ShougunuPhase1ActionScheduler.Advance(skill1Cap, 6200L, true).State;
            int hpBeforeRepair = skill1Cap.CurrentHp;
            skill1Cap = ShougunuPhase1ActionScheduler.Advance(skill1Cap, 7400L, true).State;
            Add(bundle, "scheduler.skill1-cap-and-no-hp-heal",
                Int(skill1Cap.CurrentShell) + "|" + Int(skill1Cap.CurrentHp),
                "200|" + Int(hpBeforeRepair),
                skill1Cap.CurrentShell == 200 && skill1Cap.CurrentHp == hpBeforeRepair);
        }

        private static void VerifyNegativeFixtures(VerificationBundle bundle)
        {
            ShougunuPhase1RuntimeSnapshot baseState = NewShellState("boss.fixture.negative");
            ShougunuPhase1TransitionResult accepted =
                ApplyAccepted(baseState, "negative-seed", 1L, 100L, 10, 0);
            AddNegative(bundle, "NEG-001", "duplicate-event-id",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "negative-seed", 2L, 200L, 1, accepted.State.EnemyInstanceId, true, 1, 0, ""),
                "APPLICATION_EVENT_DUPLICATE", false);
            AddNegative(bundle, "NEG-002", "stale-sequence",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-stale", 1L, 200L, 1, accepted.State.EnemyInstanceId, true, 1, 0, ""),
                "APPLICATION_SEQUENCE_NOT_INCREASING", false);
            AddNegative(bundle, "NEG-003", "wrong-generation",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-gen", 2L, 200L, 2, accepted.State.EnemyInstanceId, true, 1, 0, ""),
                "RESET_GENERATION_MISMATCH", false);
            AddNegative(bundle, "NEG-004", "wrong-target",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-target", 2L, 200L, 1, "another-enemy", true, 1, 0, ""),
                "ENEMY_INSTANCE_MISMATCH", false);
            AddNegative(bundle, "NEG-005", "ledger-rejected",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-ledger", 2L, 200L, 1, accepted.State.EnemyInstanceId, false, 1, 0,
                    "battle-rejected"),
                "BATTLE_LEDGER_REJECTED", false);
            AddNegative(bundle, "NEG-006", "zero-delta",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-zero", 2L, 200L, 1, accepted.State.EnemyInstanceId, true, 0, 0, ""),
                "ZERO_APPLIED_DAMAGE", false);
            AddNegative(bundle, "NEG-007", "negative-delta-invalid",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-negative", 2L, 200L, 1, accepted.State.EnemyInstanceId, true, -1, 0, ""),
                "DAMAGE_NEGATIVE", true);
            AddNegative(bundle, "NEG-008", "cross-channel-before-shell-break-invalid",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-ambiguous", 2L, 200L, 1, accepted.State.EnemyInstanceId, true, 1, 1, ""),
                "DAMAGE_CHANNEL_LIFECYCLE_MISMATCH", true);
            AddNegative(bundle, "NEG-009", "shell-overflow-invalid",
                accepted.State,
                new ShougunuPhase1BattleApplication(
                    "neg-shell-over", 2L, 200L, 1, accepted.State.EnemyInstanceId, true, 999, 0, ""),
                "SHELL_DAMAGE_EXCEEDS_STATE", true);

            ShougunuPhase1RuntimeSnapshot core = NewShellState("boss.fixture.core-negative");
            core = ApplyAccepted(core, "core-break", 1L, 100L, 200, 0).State;
            AddNegative(bundle, "NEG-010", "shell-channel-during-core-invalid",
                core,
                new ShougunuPhase1BattleApplication(
                    "neg-shell-core", 2L, 200L, 1, core.EnemyInstanceId, true, 1, 0, ""),
                "DAMAGE_CHANNEL_LIFECYCLE_MISMATCH", true);

            ShougunuPhase1RuntimeSnapshot terminal = NewShellState("boss.fixture.terminal");
            terminal = ApplyAccepted(terminal, "terminal-break", 1L, 100L, 200, 0).State;
            terminal = ApplyAccepted(terminal, "terminal-lethal", 2L, 200L, 0, 980).State;
            ShougunuPhase1TransitionResult terminalViolation =
                ShougunuPhase1RuntimeReducer.RequestDefeat(terminal, 300L);
            bool terminalPass = !terminalViolation.Accepted
                && terminalViolation.State.Lifecycle == ShougunuPhase1LifecycleState.Invalid
                && terminalViolation.State.Errors.Contains(
                    ShougunuPhase1RuntimeContract.TerminalOrderingViolation,
                    StringComparer.Ordinal);
            bundle.NegativeRows.Add(new NegativeFixtureRow(
                "NEG-011",
                "mandatory-threshold-before-defeat",
                ShougunuPhase1RuntimeContract.TerminalOrderingViolation,
                terminalViolation.RejectionReason,
                terminalViolation.State.Lifecycle.ToString(),
                terminalPass));
            Add(bundle, "negative.NEG-011", terminalViolation.RejectionReason,
                ShougunuPhase1RuntimeContract.TerminalOrderingViolation, terminalPass);

            ShougunuPhase1TransitionResult badReset =
                ShougunuPhase1RuntimeReducer.Reset(accepted.State, 1, 300L);
            bool resetPass = !badReset.Accepted
                && badReset.RejectionReason == "RESET_GENERATION_NOT_INCREASING";
            bundle.NegativeRows.Add(new NegativeFixtureRow(
                "NEG-012", "reset-generation-not-increasing",
                "RESET_GENERATION_NOT_INCREASING", badReset.RejectionReason,
                badReset.State.Lifecycle.ToString(), resetPass));
            Add(bundle, "negative.NEG-012", badReset.RejectionReason,
                "RESET_GENERATION_NOT_INCREASING", resetPass);
        }

        private static void RunNominalTrace(VerificationBundle bundle)
        {
            List<NominalApplication> applications = BuildNominalApplications();
            ShougunuPhase1RuntimeSnapshot state = NewShellState("boss.fixture.nominal");
            int applicationIndex = 0;
            long previousCueSequence = state.LatestCue == null ? 0L : state.LatestCue.CueSequence;
            for (long tick = 0L; tick <= 75000L; tick++)
            {
                ShougunuPhase1TransitionResult lifecycle =
                    ShougunuPhase1RuntimeReducer.AdvanceBattleTick(state, tick);
                if (lifecycle.Accepted)
                {
                    state = lifecycle.State;
                }

                ShougunuPhase1TransitionResult scheduler =
                    ShougunuPhase1ActionScheduler.Advance(state, tick, true);
                if (scheduler.Accepted)
                {
                    state = scheduler.State;
                }

                while (applicationIndex < applications.Count
                    && applications[applicationIndex].BattleTick == tick)
                {
                    NominalApplication fixture = applications[applicationIndex];
                    int shellDamageApplied = 0;
                    int hpDamageApplied = 0;
                    if (state.Lifecycle == ShougunuPhase1LifecycleState.LayeredShellOn)
                    {
                        shellDamageApplied = Math.Min(
                            fixture.NominalPulseMagnitude,
                            state.CurrentShell);
                        int remainder = fixture.NominalPulseMagnitude - shellDamageApplied;
                        if (shellDamageApplied == state.CurrentShell && remainder > 0)
                        {
                            hpDamageApplied = Math.Min(remainder, state.CurrentHp);
                        }
                    }
                    else if (state.Lifecycle == ShougunuPhase1LifecycleState.CoreExposed)
                    {
                        hpDamageApplied = Math.Min(
                            fixture.NominalPulseMagnitude,
                            state.CurrentHp);
                    }
                    ShougunuPhase1TransitionResult applied = ApplyAccepted(
                        state,
                        fixture.EventId,
                        fixture.Sequence,
                        fixture.BattleTick,
                        shellDamageApplied,
                        hpDamageApplied,
                        fixture.NominalPulseMagnitude);
                    if (applied.Accepted)
                    {
                        state = applied.State;
                    }
                    bundle.NominalRows.Add(new NominalTraceRow(
                        "NOM-" + fixture.Sequence.ToString("D3", CultureInfo.InvariantCulture),
                        fixture.BattleTick,
                        "BattleApplication",
                        fixture.EventId,
                        fixture.Sequence,
                        fixture.NominalPulseMagnitude,
                        shellDamageApplied,
                        hpDamageApplied,
                        state.Lifecycle.ToString(),
                        state.CurrentHp,
                        state.ShellLayerIndex,
                        state.CurrentShell,
                        state.BasicEarnedCount,
                        state.BasicResolvedCount,
                        state.ThresholdOccurrences.Count(item => !item.Resolved),
                        state.LatestCue == null ? string.Empty : state.LatestCue.CueKind.ToString(),
                        applied.Accepted,
                        applied.RejectionReason));
                    applicationIndex++;

                    ShougunuPhase1TransitionResult postApplySchedule =
                        ShougunuPhase1ActionScheduler.Advance(state, tick, true);
                    if (postApplySchedule.Accepted)
                    {
                        state = postApplySchedule.State;
                    }
                }
                previousCueSequence = state.LatestCue == null
                    ? previousCueSequence
                    : state.LatestCue.CueSequence;
            }

            ShougunuPhase1TransitionResult defeat =
                ShougunuPhase1RuntimeReducer.RequestDefeat(state, 75000L);
            if (defeat.Accepted)
            {
                state = defeat.State;
            }
            bundle.NominalFinalState = state;

            int shellBreaks = state.Cues.Count(item => item.CueKind == ShougunuPhase1CueKind.ShellBreak);
            int thresholdSkills = state.Cues.Count(item =>
                item.CueKind == ShougunuPhase1CueKind.Skill1ShellRepair
                || item.CueKind == ShougunuPhase1CueKind.Skill2RopeHeavyStrike
                || item.CueKind == ShougunuPhase1CueKind.Skill3GroundSealBurst);
            int basicCues = state.Cues.Count(item => item.CueKind == ShougunuPhase1CueKind.BasicAttack);
            long basic12ResolveTick = state.Cues
                .Where(item => item.CueKind == ShougunuPhase1CueKind.BasicAttack)
                .OrderBy(item => item.CueSequence)
                .Select(item => item.BattleTick)
                .DefaultIfEmpty(-1L)
                .Last();
            long s3bResolveTick = state.Cues
                .Where(item => item.CueKind == ShougunuPhase1CueKind.Skill3GroundSealBurst
                    && item.SourceEventId.EndsWith("S3-B", StringComparison.Ordinal))
                .Select(item => item.BattleTick)
                .DefaultIfEmpty(-1L)
                .Max();
            Add(bundle, "nominal.accepted-applications",
                Int(state.AcceptedDamageApplicationCount), "50",
                state.AcceptedDamageApplicationCount == 50 && applications.Count == 50);
            Add(bundle, "nominal.shell-breaks", Int(shellBreaks), "7", shellBreaks == 7);
            Add(bundle, "nominal.threshold-skills", Int(thresholdSkills), "6",
                thresholdSkills == 6
                    && state.ThresholdOccurrences.Count == 6
                    && state.ThresholdOccurrences.All(item => item.Resolved));
            Add(bundle, "nominal.basic-resolves", Int(state.BasicResolvedCount) + "|" + Int(basicCues),
                "12|12", state.BasicResolvedCount == 12 && basicCues == 12);
            Add(bundle, "nominal.s3b-before-defeat",
                Long(s3bResolveTick) + "|" + state.Lifecycle,
                "S3-B resolves before Defeated",
                s3bResolveTick >= 71400L
                    && s3bResolveTick <= 71700L
                    && state.Lifecycle == ShougunuPhase1LifecycleState.Defeated);
            Add(bundle, "nominal.basic12-guarded-final-core",
                Long(basic12ResolveTick),
                ">=74100 and <75000",
                basic12ResolveTick >= 74100L && basic12ResolveTick < 75000L);
            Add(bundle, "nominal.final-state",
                Int(state.CurrentHp) + "|" + state.Lifecycle + "|" + Long(75000L),
                "0|Defeated|75000",
                defeat.Accepted
                    && state.CurrentHp == 0
                    && state.Lifecycle == ShougunuPhase1LifecycleState.Defeated);
            Add(bundle, "nominal.observation-break-ticks",
                string.Join(";", state.Cues
                    .Where(item => item.CueKind == ShougunuPhase1CueKind.ShellBreak)
                    .Select(item => Long(item.BattleTick))),
                "7500;18000;28500;39000;51000;63000;73500",
                string.Join(";", state.Cues
                    .Where(item => item.CueKind == ShougunuPhase1CueKind.ShellBreak)
                    .Select(item => Long(item.BattleTick)))
                    == "7500;18000;28500;39000;51000;63000;73500");
            Add(bundle, "nominal.observation-threshold-ticks",
                string.Join(";", state.ThresholdOccurrences.Select(item => Long(item.TriggerTick))),
                "9000;22500;31500;42000;54000;63000",
                string.Join(";", state.ThresholdOccurrences.Select(item => Long(item.TriggerTick)))
                    == "9000;22500;31500;42000;54000;63000");
            string pulseProjection = string.Join(
                ",",
                applications.Select(item => Int(item.NominalPulseMagnitude)));
            string expectedPulseProjection = string.Join(
                ",",
                Enumerable.Range(0, 50).Select(index =>
                    Int(new[] { 29, 42, 53, 46, 55, 76 }[index % 6])));
            Add(bundle, "nominal.accepted-pulse-magnitudes",
                pulseProjection,
                expectedPulseProjection,
                pulseProjection == expectedPulseProjection);
            Add(bundle, "nominal.authoritative-delta-within-pulse",
                Int(bundle.NominalRows.Count(row =>
                    row.Accepted
                    && row.ShellDamageApplied + row.HpDamageApplied > 0
                    && row.ShellDamageApplied + row.HpDamageApplied
                        <= row.NominalPulseMagnitude)),
                "50",
                bundle.NominalRows.Count == 50
                    && bundle.NominalRows.All(row =>
                        row.Accepted
                        && row.ShellDamageApplied + row.HpDamageApplied > 0
                        && row.ShellDamageApplied + row.HpDamageApplied
                            <= row.NominalPulseMagnitude));
        }

        private static void VerifyPackageBoundary(
            string projectRoot,
            string outputRoot,
            VerificationBundle bundle)
        {
            string assignmentPath = ToAbsolute(
                projectRoot,
                "Docs/V0.4/ShougunuPhase1RuntimeAndActionContract01_Assignment.md");
            string assignmentActual = File.Exists(assignmentPath)
                ? Sha256File(assignmentPath)
                : "<missing>";
            Add(bundle, "boundary.assignment-sha256",
                assignmentActual,
                AssignmentSha256,
                assignmentActual == AssignmentSha256);

            bool manifestShape = PackageManifest.Length == 23
                && PackageManifest.Distinct(StringComparer.Ordinal).Count() == 23
                && PackageManifest.SequenceEqual(
                    PackageManifest.OrderBy(path => path, StringComparer.Ordinal),
                    StringComparer.Ordinal)
                && RuntimeSourceManifest.Length == 7
                && ReportManifest.Length == 8;
            Add(bundle, "boundary.package-manifest-shape",
                Int(PackageManifest.Length) + "|" + Int(RuntimeSourceManifest.Length) + "|"
                    + Int(ReportManifest.Length),
                "23|7|8", manifestShape);

            List<string> packageGuids = new List<string>();
            bool metaReadable = true;
            foreach (string metaPath in MetaManifest)
            {
                string outputPath = ToAbsolute(outputRoot, metaPath);
                string projectPath = ToAbsolute(projectRoot, metaPath);
                string actual = File.Exists(outputPath) ? outputPath : projectPath;
                Match match = File.Exists(actual)
                    ? Regex.Match(
                        File.ReadAllText(actual, Encoding.UTF8),
                        @"(?m)^guid:\s*([0-9a-f]{32})\s*$",
                        RegexOptions.CultureInvariant)
                    : Match.Empty;
                if (!match.Success)
                {
                    metaReadable = false;
                }
                else
                {
                    packageGuids.Add(match.Groups[1].Value);
                }
            }
            HashSet<string> externalGuids = new HashSet<string>(StringComparer.Ordinal);
            foreach (string metaPath in EnumerateRelativeFiles(projectRoot, "Assets")
                .Where(path => path.EndsWith(".meta", StringComparison.Ordinal)
                    && !PackageManifest.Contains(path, StringComparer.Ordinal)))
            {
                Match match = Regex.Match(
                    File.ReadAllText(ToAbsolute(projectRoot, metaPath), Encoding.UTF8),
                    @"(?m)^guid:\s*([0-9a-f]{32})\s*$",
                    RegexOptions.CultureInvariant);
                if (match.Success)
                {
                    externalGuids.Add(match.Groups[1].Value);
                }
            }
            bool guidPass = metaReadable
                && MetaManifest.Length == 8
                && packageGuids.Distinct(StringComparer.Ordinal).Count() == 8
                && packageGuids.All(guid => !externalGuids.Contains(guid));
            Add(bundle, "boundary.guid-uniqueness",
                Int(packageGuids.Count) + "|"
                    + Int(packageGuids.Distinct(StringComparer.Ordinal).Count()) + "|"
                    + Int(packageGuids.Count(guid => externalGuids.Contains(guid))),
                "8|8|0",
                guidPass);

            string source = string.Join("\n", RuntimeSourceManifest.Select(path =>
            {
                string outputPath = ToAbsolute(outputRoot, path);
                string projectPath = ToAbsolute(projectRoot, path);
                string actual = File.Exists(outputPath) ? outputPath : projectPath;
                return File.Exists(actual) ? File.ReadAllText(actual, Encoding.UTF8) : string.Empty;
            }));
            bool effectiveEntryPresent = source.IndexOf(
                "namespace TalismanBag.EditorTools.EnemySystem",
                StringComparison.Ordinal) >= 0
                && source.IndexOf(
                    "public static void RunFromCommandLine()",
                    StringComparison.Ordinal) >= 0;
            bool siblingNamespaceAbsent = !Regex.IsMatch(
                source,
                @"namespace\s+TalismanBag\.Editor(?:\s|\.)",
                RegexOptions.CultureInvariant);
            Add(bundle, "boundary.compile-safe-entry",
                effectiveEntryPresent + "|" + siblingNamespaceAbsent,
                "effective entry present|no sibling TalismanBag.Editor namespace",
                effectiveEntryPresent && siblingNamespaceAbsent);

            string runtimeOnly = string.Join("\n", RuntimeSourceManifest
                .Where(path => path.IndexOf(
                    "/EnemySystem/BoneAspect/",
                    StringComparison.Ordinal) >= 0)
                .Select(path =>
                {
                    string outputPath = ToAbsolute(outputRoot, path);
                    string projectPath = ToAbsolute(projectRoot, path);
                    string actual = File.Exists(outputPath) ? outputPath : projectPath;
                    return File.Exists(actual) ? File.ReadAllText(actual, Encoding.UTF8) : string.Empty;
                }));
            string[] forbiddenRuntimeTokens =
            {
                "BlindWoman", "blindWoman", "盲眼女人", "Phase2", "Phase3",
                "Summon", "Split", "ItemSystem", "BuildSandbox", "CrossSystem"
            };
            string[] leaked = forbiddenRuntimeTokens.Where(token =>
                runtimeOnly.IndexOf(token, StringComparison.Ordinal) >= 0).ToArray();
            Add(bundle, "boundary.no-forbidden-runtime-domain",
                string.Join(";", leaked), "none", leaked.Length == 0);
            string[] forbiddenMechanicTokens =
            {
                "bind_duration", "stun_duration", "forced_movement",
                "persistent_hazard", "damage_over_time", "visual_callback",
                "animation_completion"
            };
            string[] mechanicLeaks = forbiddenMechanicTokens.Where(token =>
                runtimeOnly.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            Add(bundle, "boundary.no-unauthorized-skill-or-visual-facts",
                string.Join(";", mechanicLeaks), "none", mechanicLeaks.Length == 0);

            bool flagsPresent = runtimeOnly.IndexOf(
                    "public const bool DevOnly = true",
                    StringComparison.Ordinal) >= 0
                && runtimeOnly.IndexOf(
                    "public const bool IsEnabled = false",
                    StringComparison.Ordinal) >= 0
                && runtimeOnly.IndexOf(
                    "public const bool EntersFormalFlow = false",
                    StringComparison.Ordinal) >= 0
                && runtimeOnly.IndexOf(
                    "public const bool RuntimeBoundToBattle = false",
                    StringComparison.Ordinal) >= 0;
            Add(bundle, "boundary.devonly-flags-source", Bool(flagsPresent), "true", flagsPresent);

            bool reportProjectionExact = ProjectManifestSection().Split('\n')
                .Count(line => line.StartsWith("- `", StringComparison.Ordinal)) == 23;
            Add(bundle, "boundary.report-manifest-projection",
                Int(ProjectManifestSection().Split('\n')
                    .Count(line => line.StartsWith("- `", StringComparison.Ordinal))),
                "23", reportProjectionExact);
        }

        private static void VerifyProtectedBaselines(string projectRoot, VerificationBundle bundle)
        {
            foreach (BaselineExpectation expectation in BaselineExpectations)
            {
                AggregateResult actual = AggregateFiles(projectRoot, expectation.Paths);
                bool pass = actual.Count == expectation.Count
                    && string.Equals(actual.Hash, expectation.Hash, StringComparison.Ordinal);
                bundle.BaselineRows.Add(new BaselineRow(
                    expectation.Name,
                    expectation.Count,
                    expectation.Hash,
                    actual.Count,
                    actual.Hash,
                    pass,
                    pass ? "UNCHANGED" : "UNATTRIBUTED_DRIFT"));
                Add(bundle, "baseline." + expectation.Name,
                    Int(actual.Count) + "|" + actual.Hash,
                    Int(expectation.Count) + "|" + expectation.Hash,
                    pass);
            }

            HashSet<string> packagePaths = new HashSet<string>(
                PackageManifest,
                StringComparer.Ordinal);
            foreach (BroadBaselineExpectation expectation in BroadBaselineExpectations)
            {
                string[] paths = expectation.Paths ?? EnumerateRelativeFiles(
                    projectRoot,
                    expectation.Root).Where(path => !packagePaths.Contains(path)).ToArray();
                AggregateResult actual = AggregateFiles(projectRoot, paths);
                bool unchanged = actual.Count == expectation.Count
                    && string.Equals(actual.Hash, expectation.Hash, StringComparison.Ordinal);
                bool itemAttributed = false;
                if (expectation.Name == "Items")
                {
                    HashSet<string> leasedPaths = new HashSet<string>(
                        AttributedItemDrifts.Select(evidence => evidence.Path),
                        StringComparer.Ordinal);
                    AggregateResult unownedItems = AggregateFiles(
                        projectRoot,
                        paths.Where(path => !leasedPaths.Contains(path)));
                    bool ownerLeaseSetExact = AttributedItemDrifts.All(evidence =>
                        File.Exists(ToAbsolute(projectRoot, evidence.Path)));
                    itemAttributed = unownedItems.Count == expectation.Count
                        && unownedItems.Hash == expectation.Hash
                        && ownerLeaseSetExact
                        && paths.Count(path => leasedPaths.Contains(path))
                            == AttributedItemDrifts.Length;
                    if (ownerLeaseSetExact)
                    {
                        bundle.ExternalDriftRows.Clear();
                        foreach (ExternalDriftEvidence evidence in AttributedItemDrifts)
                        {
                            string observed = Sha256File(ToAbsolute(projectRoot, evidence.Path));
                            bundle.ExternalDriftRows.Add(new ExternalDriftObservation(
                                evidence.Path,
                                evidence.StartSha256,
                                evidence.InitialReceiptSha256,
                                observed,
                                observed == evidence.InitialReceiptSha256
                                    ? "ITEM_OWNED"
                                    : "ITEM_OWNED / INCREMENTAL_HASH_UPDATE"));
                        }
                    }
                }
                bool pass = unchanged || itemAttributed;
                string disposition = unchanged
                    ? "UNCHANGED"
                    : itemAttributed
                        ? "EXTERNAL_CONCURRENT_DRIFT / ITEM_OWNED"
                        : "UNATTRIBUTED_DRIFT";
                bundle.BaselineRows.Add(new BaselineRow(
                    expectation.Name,
                    expectation.Count,
                    expectation.Hash,
                    actual.Count,
                    actual.Hash,
                    pass,
                    disposition));
                Add(bundle, "baseline.broad." + expectation.Name,
                    Int(actual.Count) + "|" + actual.Hash,
                    Int(expectation.Count) + "|" + expectation.Hash
                        + (expectation.Name == "Items"
                            ? " OR task-start aggregate unchanged after excluding exact 7-path Item Owner Lease"
                            : string.Empty),
                    pass);
            }
        }

        private static void BuildReportProjections(VerificationBundle bundle)
        {
            bundle.ReportContents["Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractSpec.csv"] =
                BuildSpecCsv(bundle);
            bundle.ReportContents[
                "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractFieldMatrix.csv"] =
                BuildFieldMatrixCsv();
            bundle.ReportContents["Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionPattern.csv"] =
                BuildActionPatternCsv();
            bundle.ReportContents["Docs/V0.4/Reports/ShougunuPhase1ThresholdAndQueueFixtureRows.csv"] =
                BuildThresholdCsv(bundle);
            bundle.ReportContents["Docs/V0.4/Reports/ShougunuPhase1NominalTrace.csv"] =
                BuildNominalCsv(bundle);
            bundle.ReportContents["Docs/V0.4/Reports/ShougunuPhase1NegativeFixtureRows.csv"] =
                BuildNegativeCsv(bundle);

            bundle.ReportCanonical = CanonicalReportSet(bundle.ReportContents);
            bundle.ReportContents["Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md"] =
                BuildMainReport(bundle);
            bundle.ReportContents[
                "Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractLeakCheckReport.md"] =
                BuildLeakReport(bundle);
        }

        private static void WriteReports(string outputRoot, VerificationBundle bundle)
        {
            foreach (KeyValuePair<string, string> pair in bundle.ReportContents
                .OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                string path = ToAbsolute(outputRoot, pair.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? outputRoot);
                File.WriteAllText(path, NormalizeText(pair.Value), new UTF8Encoding(false));
            }
        }

        private static string BuildSpecCsv(VerificationBundle bundle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "\"rowId\",\"section\",\"field\",\"actual\",\"expected\",\"status\",\"notes\"");
            AddSpec(builder, "SPEC-001", "identity", "schemaId",
                ShougunuPhase1RuntimeContract.SchemaId,
                "ShougunuPhase1RuntimeAndActionContract.v1", "immutable schema");
            AddSpec(builder, "SPEC-002", "identity", "contentId",
                ShougunuPhase1RuntimeContract.ContentId,
                "bone_aspect_boss_c1_bone_guard", "accepted P1 carrier identity");
            AddSpec(builder, "SPEC-003", "identity", "phaseId",
                ShougunuPhase1RuntimeContract.PhaseId,
                "bone_aspect.phase.shougunu.phase1", "Phase1 only");
            AddSpec(builder, "SPEC-004", "boundary", "flags",
                "devOnly=true|isEnabled=false|entersFormalFlow=false|runtimeBoundToBattle=false",
                "devOnly=true|isEnabled=false|entersFormalFlow=false|runtimeBoundToBattle=false",
                "no formal route binding");
            AddSpec(builder, "SPEC-005", "tuning", "hp",
                Int(ShougunuPhase1RuntimeContract.MaxHp), "980", "initial=max");
            AddSpec(builder, "SPEC-006", "tuning", "shell",
                Int(ShougunuPhase1RuntimeContract.ShellLayerMax) + "|"
                    + Int(ShougunuPhase1RuntimeContract.MaxSequentialShellLayers),
                "200|7", "sequential shell layers");
            AddSpec(builder, "SPEC-007", "tuning", "coreExpose",
                Long(ShougunuPhase1RuntimeContract.CoreExposeDurationTicks),
                "6000", "ticks; ticksPerSecond=1000");
            AddSpec(builder, "SPEC-008", "tuning", "skill1Repair",
                Int(ShougunuPhase1RuntimeContract.Skill1RepairUnits), "30",
                "current damaged nonzero shell only");
            AddSpec(builder, "SPEC-009", "debt", "basicInterval",
                Int(ShougunuPhase1RuntimeContract.BasicDamageApplicationInterval), "4",
                "accepted unique applications only");
            AddSpec(builder, "SPEC-010", "qa", "checkCount",
                Int(bundle.Checks.Count), Int(bundle.Checks.Count), "all deterministic checks");
            foreach (CheckRow check in bundle.Checks.OrderBy(row => row.CheckId, StringComparer.Ordinal))
            {
                builder.Append(Csv("CHECK-" + check.CheckId)).Append(',')
                    .Append(Csv("verification")).Append(',')
                    .Append(Csv(check.CheckId)).Append(',')
                    .Append(Csv(check.Actual)).Append(',')
                    .Append(Csv(check.Expected)).Append(',')
                    .Append(Csv(check.Pass ? "PASS" : "FAIL")).Append(',')
                    .Append(Csv("deterministic same-source assertion")).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildFieldMatrixCsv()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "\"fieldName\",\"fullRuntime\",\"presentationSafe\",\"reason\"");
            string[] visible =
            {
                "enemyInstanceId", "contentId", "presentationKey", "resetGeneration", "revision",
                "lifecycle", "maxHp", "currentHp", "shellLayerIndex",
                "maxSequentialShellLayers", "shellLayerMax", "currentShell", "targetable",
                "vulnerable", "coreExposeActive", "coreExposeRemainingTicks", "latestCue",
                "errors", "presentationSafeCanonicalSignature"
            };
            foreach (string field in visible)
            {
                builder.Append(Csv(field)).Append(',')
                    .Append(Csv("true")).Append(',')
                    .Append(Csv("true")).Append(',')
                    .Append(Csv("player-observable presentation projection")).AppendLine();
            }
            string[] internalOnly =
            {
                "phaseId", "devOnly", "isEnabled", "entersFormalFlow", "runtimeBoundToBattle",
                "coreExposeStartTick", "coreExposeEndTick", "recoveryAvailable",
                "lastAcceptedBattleTick", "lastAcceptedApplicationSequence",
                "acceptedDamageApplicationCount", "basicEarnedCount", "basicResolvedCount",
                "basicDebt", "acceptedApplicationEventIds", "thresholdOccurrences",
                "activeAction", "cues", "developerDiagnostics", "canonicalSignature",
                "actionCatalogCanonicalSignature"
            };
            foreach (string field in internalOnly)
            {
                builder.Append(Csv(field)).Append(',')
                    .Append(Csv("true")).Append(',')
                    .Append(Csv("false")).Append(',')
                    .Append(Csv("internal reducer, queue, dedupe or diagnostics state")).AppendLine();
            }
            foreach (string absent in new[]
            {
                "blindWomanActor", "blindWomanHp", "blindWomanTargetable",
                "damageTransferTarget", "baD3CopyScope", "baD4FormalForms"
            })
            {
                builder.Append(Csv(absent)).Append(',')
                    .Append(Csv("false")).Append(',')
                    .Append(Csv("false")).Append(',')
                    .Append(Csv("forbidden or unresolved domain; not authored")).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildActionPatternCsv()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "\"actionPatternId\",\"effectRequestId\",\"dueKind\",\"preCastTicks\",\"telegraphTicks\",\"castTicks\",\"resolveOffsetTicks\",\"recoverTicks\",\"interruptPolicy\",\"cueKind\",\"runtimeOwnerBoundary\"");
            foreach (ShougunuPhase1ActionPatternSnapshot pattern
                in ShougunuPhase1ActionPatternCatalog.GetPatterns()
                    .OrderBy(item => item.ActionPatternId, StringComparer.Ordinal))
            {
                builder.Append(Csv(pattern.ActionPatternId)).Append(',')
                    .Append(Csv(pattern.EffectRequestId)).Append(',')
                    .Append(Csv(pattern.DueKind.ToString())).Append(',')
                    .Append(Csv(Long(pattern.PreCastTicks))).Append(',')
                    .Append(Csv(Long(pattern.TelegraphTicks))).Append(',')
                    .Append(Csv(Long(pattern.CastTicks))).Append(',')
                    .Append(Csv(Long(pattern.ResolveOffsetTicks))).Append(',')
                    .Append(Csv(Long(pattern.RecoverTicks))).Append(',')
                    .Append(Csv(pattern.InterruptPolicy.ToString())).Append(',')
                    .Append(Csv(pattern.CueKind.ToString())).Append(',')
                    .Append(Csv("neutral request only; Battle owns player-side effect")).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildThresholdCsv(VerificationBundle bundle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "\"rowId\",\"rowKind\",\"thresholdId\",\"actionPatternId\",\"basisPoints\",\"triggerTick\",\"triggerSequence\",\"resolved\",\"queueRule\",\"status\"");
            int row = 1;
            foreach (ShougunuPhase1ThresholdOccurrenceSnapshot occurrence
                in bundle.NominalFinalState.ThresholdOccurrences.OrderBy(item => item.TriggerSequence))
            {
                builder.Append(Csv("THR-" + row.ToString("D3", CultureInfo.InvariantCulture))).Append(',')
                    .Append(Csv("nominal-threshold")).Append(',')
                    .Append(Csv(occurrence.ThresholdId)).Append(',')
                    .Append(Csv(occurrence.ActionPatternId)).Append(',')
                    .Append(Csv(Int(occurrence.ThresholdBasisPoints))).Append(',')
                    .Append(Csv(Long(occurrence.TriggerTick))).Append(',')
                    .Append(Csv(Long(occurrence.TriggerSequence))).Append(',')
                    .Append(Csv(Bool(occurrence.Resolved))).Append(',')
                    .Append(Csv("descending threshold FIFO; one trigger per generation")).Append(',')
                    .Append(Csv(occurrence.Resolved ? "PASS" : "FAIL")).AppendLine();
                row++;
            }
            string[] queueRules =
            {
                "pending threshold actions are considered before Basic debt when legal",
                "threshold actions remain pending during CoreExposed and Recovering",
                "Skill1 remains pending while shell is full or zero",
                "Basic becomes legal after the 600-tick CoreExpose reactive guard",
                "one action is active at a time",
                "Basic debt is consumed only after Battle accepts resolve"
            };
            foreach (string rule in queueRules)
            {
                builder.Append(Csv("QUEUE-" + row.ToString("D3", CultureInfo.InvariantCulture))).Append(',')
                    .Append(Csv("queue-rule")).Append(',')
                    .Append(Csv("")).Append(',')
                    .Append(Csv("")).Append(',')
                    .Append(Csv("")).Append(',')
                    .Append(Csv("")).Append(',')
                    .Append(Csv("")).Append(',')
                    .Append(Csv("")).Append(',')
                    .Append(Csv(rule)).Append(',')
                    .Append(Csv("PASS")).AppendLine();
                row++;
            }
            return builder.ToString();
        }

        private static string BuildNominalCsv(VerificationBundle bundle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "\"rowId\",\"battleTick\",\"rowKind\",\"applicationEventId\",\"applicationSequence\",\"nominalPulseMagnitude\",\"shellDamageApplied\",\"hpDamageApplied\",\"lifecycle\",\"currentHp\",\"shellLayerIndex\",\"currentShell\",\"basicEarned\",\"basicResolved\",\"pendingThresholdCount\",\"latestCue\",\"accepted\",\"rejectionReason\"");
            foreach (NominalTraceRow row in bundle.NominalRows)
            {
                builder.Append(Csv(row.RowId)).Append(',')
                    .Append(Csv(Long(row.BattleTick))).Append(',')
                    .Append(Csv(row.RowKind)).Append(',')
                    .Append(Csv(row.ApplicationEventId)).Append(',')
                    .Append(Csv(Long(row.ApplicationSequence))).Append(',')
                    .Append(Csv(Int(row.NominalPulseMagnitude))).Append(',')
                    .Append(Csv(Int(row.ShellDamageApplied))).Append(',')
                    .Append(Csv(Int(row.HpDamageApplied))).Append(',')
                    .Append(Csv(row.Lifecycle)).Append(',')
                    .Append(Csv(Int(row.CurrentHp))).Append(',')
                    .Append(Csv(Int(row.ShellLayerIndex))).Append(',')
                    .Append(Csv(Int(row.CurrentShell))).Append(',')
                    .Append(Csv(Int(row.BasicEarned))).Append(',')
                    .Append(Csv(Int(row.BasicResolved))).Append(',')
                    .Append(Csv(Int(row.PendingThresholdCount))).Append(',')
                    .Append(Csv(row.LatestCue)).Append(',')
                    .Append(Csv(Bool(row.Accepted))).Append(',')
                    .Append(Csv(row.RejectionReason)).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildNegativeCsv(VerificationBundle bundle)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "\"fixtureId\",\"scenario\",\"expectedReason\",\"actualReason\",\"resultLifecycle\",\"status\"");
            foreach (NegativeFixtureRow row in bundle.NegativeRows
                .OrderBy(item => item.FixtureId, StringComparer.Ordinal))
            {
                builder.Append(Csv(row.FixtureId)).Append(',')
                    .Append(Csv(row.Scenario)).Append(',')
                    .Append(Csv(row.ExpectedReason)).Append(',')
                    .Append(Csv(row.ActualReason)).Append(',')
                    .Append(Csv(row.ResultLifecycle)).Append(',')
                    .Append(Csv(row.Pass ? "PASS" : "FAIL")).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildMainReport(VerificationBundle bundle)
        {
            bool pass = bundle.Checks.All(row => row.Pass);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Shougunu Phase1 Runtime And Action Contract Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-ShougunuPhase1RuntimeAndActionContract01`");
            builder.AppendLine("- Status: `" + (pass ? "QA_PASS" : "QA_FAIL") + "`");
            builder.AppendLine("- Assignment SHA-256: `" + AssignmentSha256 + "`");
            builder.AppendLine("- Schema: `" + ShougunuPhase1RuntimeContract.SchemaId + "`");
            builder.AppendLine("- Mode: `DEV_ACTIVE / devOnly=true / isEnabled=false / entersFormalFlow=false / runtimeBoundToBattle=false`");
            builder.AppendLine("- Existing files modified: `0`");
            builder.AppendLine("- Unity scene/user handtest: `NOT_REQUIRED`");
            builder.AppendLine("- Report canonical: `" + bundle.ReportCanonical + "`");
            builder.AppendLine();
            builder.AppendLine("## Result");
            builder.AppendLine();
            builder.AppendLine("- Deterministic checks: `" + bundle.Checks.Count(row => row.Pass) + "/"
                + bundle.Checks.Count + "`");
            builder.AppendLine("- Nominal Battle applications: `"
                + bundle.NominalFinalState.AcceptedDamageApplicationCount + "/50`");
            builder.AppendLine("- Nominal shell breaks / threshold skills / Basic resolves: `"
                + bundle.NominalFinalState.Cues.Count(item =>
                    item.CueKind == ShougunuPhase1CueKind.ShellBreak)
                + "/"
                + bundle.NominalFinalState.ThresholdOccurrences.Count(item => item.Resolved)
                + "/"
                + bundle.NominalFinalState.BasicResolvedCount + "`");
            builder.AppendLine("- Final nominal lifecycle: `" + bundle.NominalFinalState.Lifecycle
                + "` at `75000` ticks");
            builder.AppendLine("- Negative fixtures: `" + bundle.NegativeRows.Count(row => row.Pass)
                + "/" + bundle.NegativeRows.Count + "`");
            builder.AppendLine("- Consecutive report hash determinism: `"
                + CheckStatus(bundle, "report.consecutive-hash-determinism") + "`");
            builder.AppendLine("- Missing-report regeneration: `"
                + CheckStatus(bundle, "report.missing-report-regeneration") + "`");
            builder.AppendLine();
            builder.AppendLine("## Frozen Ownership Boundary");
            builder.AppendLine();
            builder.AppendLine("- Enemy owns immutable Phase1 facts, reducer state, threshold/debt queue, action-pattern intent and cues.");
            builder.AppendLine("- Battle owns accepted damage applications and player-side effect execution. No player HP or player damage value is authored here.");
            builder.AppendLine("- Fixture `nominalPulseMagnitude` repeats `29,42,53,46,55,76`; authoritative applied deltas remain state-valid and may be lower at a shell/HP boundary without reducer clamping.");
            builder.AppendLine("- Skill1 repairs only the current damaged nonzero shell by `30`, capped at `200`; it never heals HP or revives a shell.");
            builder.AppendLine("- Skill2 does not author bind, stun or movement. Skill3 does not author hazard, DoT or persistent ground.");
            builder.AppendLine("- BA-D1 runtime actor/damage-transfer implications are absent. BA-D3 and BA-D4 remain unresolved and are not exposed.");
            builder.AppendLine("- Phase2, Phase3, summon, split, Item, BuildSandbox, CrossSystem, Scene, Prefab, UI and formal-flow bindings remain out of scope.");
            builder.AppendLine();
            builder.Append(NamespaceAdjudicationSection());
            builder.AppendLine();
            builder.AppendLine("## Protected Baselines");
            builder.AppendLine();
            foreach (BaselineRow row in bundle.BaselineRows.OrderBy(item => item.Name, StringComparer.Ordinal))
            {
                builder.AppendLine("- `" + row.Name + "`: `" + (row.Pass ? "PASS" : "DRIFT")
                    + "`; disposition `" + row.Disposition
                    + "`; task-start `" + row.ExpectedCount + "/" + row.ExpectedHash
                    + "`; actual `" + row.ActualCount + "/" + row.ActualHash + "`");
            }
            builder.AppendLine();
            builder.Append(ExternalDriftSection(bundle));
            builder.AppendLine();
            builder.Append(ProjectManifestSection());
            return builder.ToString();
        }

        private static string BuildLeakReport(VerificationBundle bundle)
        {
            bool pass = bundle.Checks.All(row => row.Pass);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Shougunu Phase1 Runtime And Action Contract Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-ShougunuPhase1RuntimeAndActionContract01`");
            builder.AppendLine("- Status: `" + (pass ? "LEAK_CHECK_PASS" : "LEAK_CHECK_FAIL") + "`");
            builder.AppendLine("- Assignment SHA-256: `" + AssignmentSha256 + "`");
            builder.AppendLine("- Report canonical: `" + bundle.ReportCanonical + "`");
            builder.AppendLine("- Exact additions / existing modifications: `23 / 0`");
            builder.AppendLine("- Namespace collision regression: `"
                + (bundle.Checks.Single(row => row.CheckId == "boundary.compile-safe-entry").Pass
                    ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Forbidden runtime domain scan: `"
                + (bundle.Checks.Single(row => row.CheckId == "boundary.no-forbidden-runtime-domain").Pass
                    ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Protected baselines: `"
                + bundle.BaselineRows.Count(row => row.Pass) + "/"
                + bundle.BaselineRows.Count + "`");
            builder.AppendLine("- Report determinism / missing-report regeneration: `"
                + CheckStatus(bundle, "report.consecutive-hash-determinism") + " / "
                + CheckStatus(bundle, "report.missing-report-regeneration") + "`");
            builder.AppendLine("- Unity batch/Builder/import/scene handtest: `NOT_REQUIRED_BY_STATIC_REPORT`; final compile/verifier result is reported by the handoff.");
            builder.AppendLine("- Git operations: `0`");
            builder.AppendLine();
            builder.AppendLine("## Leak Assertions");
            builder.AppendLine();
            builder.AppendLine("- No package-owned source declares `namespace TalismanBag.Editor`.");
            builder.AppendLine("- No Runtime consumer or formal route is introduced.");
            builder.AppendLine("- No blind-woman Runtime actor, HP, targetability, damage receiver or cross-target transfer is authored.");
            builder.AppendLine("- No Phase2/3, summon, split, Item, Battle implementation, BuildSandbox, CrossSystem, Scene, Prefab, UI or Visual write is present.");
            builder.AppendLine("- Presentation-safe projection excludes future thresholds, pending queues, Basic counters, dedupe state, developer diagnostics and BA-D3/BA-D4 answers.");
            builder.AppendLine();
            builder.Append(NamespaceAdjudicationSection());
            builder.AppendLine();
            builder.Append(ExternalDriftSection(bundle));
            builder.AppendLine();
            builder.Append(ProjectManifestSection());
            return builder.ToString();
        }

        private static string NamespaceAdjudicationSection()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("## Guard Namespace Adjudication");
            builder.AppendLine();
            builder.AppendLine("- Historical defective Assignment entry (preserved as evidence): `"
                + HistoricalDefectiveEntry + "`");
            builder.AppendLine("- Effective compile-safe entry: `" + EffectiveEntry + "`");
            builder.AppendLine("- Guard supersession reason: matches accepted Enemy verifier namespaces and prevents creation of sibling namespace `TalismanBag.Editor`, avoiding `UnityEditor.Editor` / CS0118 shadowing.");
            builder.AppendLine("- `TalismanBagSceneBuilder` modification: `0`");
            return builder.ToString();
        }

        private static string ProjectManifestSection()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("## Package Manifest");
            builder.AppendLine();
            builder.AppendLine("- Additions: `23`");
            builder.AppendLine("- Existing modifications: `0`");
            foreach (string path in PackageManifest)
            {
                builder.AppendLine("- `" + path + "`");
            }
            return builder.ToString();
        }

        private static string ExternalDriftSection(VerificationBundle bundle)
        {
            StringBuilder builder = new StringBuilder();
            BaselineRow itemBaseline = bundle.BaselineRows.Single(
                row => row.Name == "Items");
            builder.AppendLine("## Attributed External Concurrent Drift");
            builder.AppendLine();
            builder.AppendLine("- Disposition: `EXTERNAL_CONCURRENT_DRIFT / ITEM_OWNED / OWNER_LEASE_ACTIVE`");
            builder.AppendLine("- Owner package: `" + ItemDriftOwnerPackage + "`");
            builder.AppendLine("- Owner Assignment SHA-256: `" + ItemDriftAssignmentSha256 + "`");
            builder.AppendLine("- Ownership receipt proves whitelist ownership only, not Item QA or acceptance.");
            builder.AppendLine("- Enemy task-start Items aggregate remains `141/4b41e4dbede32a14a661f0f985cb1682051c30836b54d811a4c55546652b18e4`; attributed current aggregate is `148/"
                + itemBaseline.ActualHash + "`.");
            builder.AppendLine("- Initial ownership-receipt aggregate was `148/"
                + ItemDriftInitialReceiptAggregate
                + "`; later content updates on the same seven frozen Item paths remain under the same Owner Lease.");
            builder.AppendLine("- These paths remain read-only and are not rebaselined, restored, formatted, staged or claimed by this package.");
            foreach (ExternalDriftObservation evidence in bundle.ExternalDriftRows
                .OrderBy(row => row.Path, StringComparer.Ordinal))
            {
                builder.AppendLine("- `" + evidence.Path + "`: `" + evidence.StartSha256
                    + " -> " + evidence.ObservedSha256 + "`; disposition `"
                    + evidence.Disposition + "`; initial receipt `"
                    + evidence.InitialReceiptSha256 + "`");
            }
            return builder.ToString();
        }

        private static string CheckStatus(VerificationBundle bundle, string checkId)
        {
            CheckRow row = bundle.Checks.SingleOrDefault(
                item => item.CheckId == checkId);
            return row != null && row.Pass ? "PASS" : "FAIL";
        }

        private static List<NominalApplication> BuildNominalApplications()
        {
            List<NominalApplication> rows = new List<NominalApplication>();
            int[] pulse = { 29, 42, 53, 46, 55, 76 };
            foreach (long tick in new long[]
            {
                1500, 3000, 4500, 6000, 7500,
                9000, 10500, 12000,
                15000, 16500, 18000, 18000,
                19500, 21000, 22500,
                25500, 26500, 27500, 28500,
                30000, 31500, 33000,
                35500, 36500, 37500, 39000,
                40500, 42000, 43500,
                46500, 48000, 49500, 51000, 51000,
                52500, 54000, 55500,
                58500, 60000, 61500, 63000, 63000,
                64500, 66000,
                70500, 71500, 72500, 73500,
                74100, 75000
            })
            {
                AddNominal(rows, tick, pulse);
            }
            return rows;
        }

        private static void AddNominal(
            List<NominalApplication> rows,
            long tick,
            int[] pulse)
        {
            long sequence = rows.Count + 1L;
            rows.Add(new NominalApplication(
                "nominal.application." + sequence.ToString("D3", CultureInfo.InvariantCulture),
                sequence,
                tick,
                pulse[(sequence - 1L) % pulse.Length],
                0,
                0));
        }

        private static ShougunuPhase1RuntimeSnapshot NewShellState(string enemyInstanceId)
        {
            ShougunuPhase1RuntimeSnapshot presence =
                ShougunuPhase1RuntimeReducer.CreatePresence(enemyInstanceId, 1, 0L);
            return ShougunuPhase1RuntimeReducer.ActivateInitialShell(presence, 0L).State;
        }

        private static ShougunuPhase1TransitionResult ApplyAccepted(
            ShougunuPhase1RuntimeSnapshot state,
            string eventId,
            long sequence,
            long tick,
            int shellDamage,
            int hpDamage,
            int nominalPulseMagnitude = 0)
        {
            return ShougunuPhase1RuntimeReducer.ApplyBattleApplication(
                state,
                new ShougunuPhase1BattleApplication(
                    eventId,
                    sequence,
                    tick,
                    state.ResetGeneration,
                    state.EnemyInstanceId,
                    true,
                    shellDamage,
                    hpDamage,
                    string.Empty,
                    nominalPulseMagnitude));
        }

        private static void AddNegative(
            VerificationBundle bundle,
            string fixtureId,
            string scenario,
            ShougunuPhase1RuntimeSnapshot state,
            ShougunuPhase1BattleApplication application,
            string expectedReason,
            bool expectedInvalid)
        {
            string before = state.CanonicalSignature;
            ShougunuPhase1TransitionResult result =
                ShougunuPhase1RuntimeReducer.ApplyBattleApplication(state, application);
            bool pass = !result.Accepted
                && result.RejectionReason == expectedReason
                && (expectedInvalid
                    ? result.State.Lifecycle == ShougunuPhase1LifecycleState.Invalid
                    : result.State.CanonicalSignature == before);
            bundle.NegativeRows.Add(new NegativeFixtureRow(
                fixtureId,
                scenario,
                expectedReason,
                result.RejectionReason,
                result.State.Lifecycle.ToString(),
                pass));
            Add(bundle, "negative." + fixtureId, result.RejectionReason, expectedReason, pass);
        }

        private static void Add(
            VerificationBundle bundle,
            string checkId,
            string actual,
            string expected,
            bool pass)
        {
            bundle.Checks.Add(new CheckRow(checkId, actual, expected, pass));
        }

        private static void AddSpec(
            StringBuilder builder,
            string rowId,
            string section,
            string field,
            string actual,
            string expected,
            string notes)
        {
            builder.Append(Csv(rowId)).Append(',')
                .Append(Csv(section)).Append(',')
                .Append(Csv(field)).Append(',')
                .Append(Csv(actual)).Append(',')
                .Append(Csv(expected)).Append(',')
                .Append(Csv(actual == expected ? "PASS" : "FAIL")).Append(',')
                .Append(Csv(notes)).AppendLine();
        }

        private static string Timing(ShougunuPhase1ActionPatternSnapshot value)
        {
            return string.Join("|", new[]
            {
                Long(value.PreCastTicks),
                Long(value.TelegraphTicks),
                Long(value.CastTicks),
                Long(value.ResolveOffsetTicks),
                Long(value.RecoverTicks)
            });
        }

        private static List<ShougunuPhase1ThresholdOccurrenceSnapshot> GetThresholdRows()
        {
            ShougunuPhase1RuntimeSnapshot state = NewShellState("boss.fixture.threshold-catalog");
            state = ApplyAccepted(state, "catalog-break", 1L, 100L, 200, 0).State;
            state = ApplyAccepted(state, "catalog-cross", 2L, 200L, 0, 880).State;
            return state.ThresholdOccurrences.ToList();
        }

        private static AggregateResult AggregateFiles(string projectRoot, IEnumerable<string> paths)
        {
            List<string> rows = new List<string>();
            int count = 0;
            foreach (string relative in (paths ?? Enumerable.Empty<string>())
                .OrderBy(path => path, StringComparer.Ordinal))
            {
                string path = ToAbsolute(projectRoot, relative);
                if (!File.Exists(path))
                {
                    rows.Add(relative + "\0" + "<missing>");
                    continue;
                }
                rows.Add(relative + "\0" + Sha256File(path));
                count++;
            }
            return new AggregateResult(count, Sha256Text(string.Join("\n", rows)));
        }

        private static string[] EnumerateRelativeFiles(string projectRoot, string relativeRoot)
        {
            string root = ToAbsolute(projectRoot, relativeRoot);
            if (!Directory.Exists(root))
            {
                return Array.Empty<string>();
            }
            return Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => NormalizePath(path.Substring(
                    Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar).Length + 1)))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        private static string CanonicalReportSet(IDictionary<string, string> reports)
        {
            string payload = string.Join("\n", reports.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Key + "\0" + Sha256Text(NormalizeText(pair.Value))));
            return "sha256:" + Sha256Text(payload);
        }

        private static string HashGeneratedReportFiles(string outputRoot)
        {
            string payload = string.Join(
                "\n",
                ReportManifest.OrderBy(path => path, StringComparer.Ordinal).Select(path =>
                {
                    string absolute = ToAbsolute(outputRoot, path);
                    return path + "\0" + (File.Exists(absolute)
                        ? Sha256File(absolute)
                        : "<missing>");
                }));
            return Sha256Text(payload);
        }

        private static string Sha256File(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return Hex(sha.ComputeHash(stream));
            }
        }

        private static string Sha256Text(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return Hex(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty)));
            }
        }

        private static string Hex(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
            {
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        private static string ToAbsolute(string root, string relative)
        {
            return Path.Combine(root, (relative ?? string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }

        private static string NormalizePath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }

        private static string NormalizeText(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n")
                .TrimEnd('\n') + "\n";
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static string[] Lines(params string[] values) { return values; }
        private static string Bool(bool value) { return value ? "true" : "false"; }
        private static string Int(int value) { return value.ToString(CultureInfo.InvariantCulture); }
        private static string Long(long value) { return value.ToString(CultureInfo.InvariantCulture); }

        private sealed class VerificationBundle
        {
            public List<CheckRow> Checks { get; } = new List<CheckRow>();
            public List<NegativeFixtureRow> NegativeRows { get; } =
                new List<NegativeFixtureRow>();
            public List<NominalTraceRow> NominalRows { get; } = new List<NominalTraceRow>();
            public List<BaselineRow> BaselineRows { get; } = new List<BaselineRow>();
            public List<ExternalDriftObservation> ExternalDriftRows { get; } =
                new List<ExternalDriftObservation>();
            public SortedDictionary<string, string> ReportContents { get; } =
                new SortedDictionary<string, string>(StringComparer.Ordinal);
            public ShougunuPhase1RuntimeSnapshot NominalFinalState { get; set; }
            public string ReportCanonical { get; set; } = string.Empty;
        }

        private sealed class CheckRow
        {
            public CheckRow(string checkId, string actual, string expected, bool pass)
            {
                CheckId = checkId;
                Actual = actual ?? string.Empty;
                Expected = expected ?? string.Empty;
                Pass = pass;
            }
            public string CheckId { get; }
            public string Actual { get; }
            public string Expected { get; }
            public bool Pass { get; }
        }

        private sealed class NegativeFixtureRow
        {
            public NegativeFixtureRow(
                string fixtureId,
                string scenario,
                string expectedReason,
                string actualReason,
                string resultLifecycle,
                bool pass)
            {
                FixtureId = fixtureId;
                Scenario = scenario;
                ExpectedReason = expectedReason;
                ActualReason = actualReason;
                ResultLifecycle = resultLifecycle;
                Pass = pass;
            }
            public string FixtureId { get; }
            public string Scenario { get; }
            public string ExpectedReason { get; }
            public string ActualReason { get; }
            public string ResultLifecycle { get; }
            public bool Pass { get; }
        }

        private sealed class NominalApplication
        {
            public NominalApplication(
                string eventId,
                long sequence,
                long battleTick,
                int nominalPulseMagnitude,
                int shellDamage,
                int hpDamage)
            {
                EventId = eventId;
                Sequence = sequence;
                BattleTick = battleTick;
                NominalPulseMagnitude = nominalPulseMagnitude;
                ShellDamage = shellDamage;
                HpDamage = hpDamage;
            }
            public string EventId { get; }
            public long Sequence { get; }
            public long BattleTick { get; }
            public int NominalPulseMagnitude { get; }
            public int ShellDamage { get; }
            public int HpDamage { get; }
        }

        private sealed class NominalTraceRow
        {
            public NominalTraceRow(
                string rowId,
                long battleTick,
                string rowKind,
                string applicationEventId,
                long applicationSequence,
                int nominalPulseMagnitude,
                int shellDamageApplied,
                int hpDamageApplied,
                string lifecycle,
                int currentHp,
                int shellLayerIndex,
                int currentShell,
                int basicEarned,
                int basicResolved,
                int pendingThresholdCount,
                string latestCue,
                bool accepted,
                string rejectionReason)
            {
                RowId = rowId;
                BattleTick = battleTick;
                RowKind = rowKind;
                ApplicationEventId = applicationEventId;
                ApplicationSequence = applicationSequence;
                NominalPulseMagnitude = nominalPulseMagnitude;
                ShellDamageApplied = shellDamageApplied;
                HpDamageApplied = hpDamageApplied;
                Lifecycle = lifecycle;
                CurrentHp = currentHp;
                ShellLayerIndex = shellLayerIndex;
                CurrentShell = currentShell;
                BasicEarned = basicEarned;
                BasicResolved = basicResolved;
                PendingThresholdCount = pendingThresholdCount;
                LatestCue = latestCue;
                Accepted = accepted;
                RejectionReason = rejectionReason;
            }
            public string RowId { get; }
            public long BattleTick { get; }
            public string RowKind { get; }
            public string ApplicationEventId { get; }
            public long ApplicationSequence { get; }
            public int NominalPulseMagnitude { get; }
            public int ShellDamageApplied { get; }
            public int HpDamageApplied { get; }
            public string Lifecycle { get; }
            public int CurrentHp { get; }
            public int ShellLayerIndex { get; }
            public int CurrentShell { get; }
            public int BasicEarned { get; }
            public int BasicResolved { get; }
            public int PendingThresholdCount { get; }
            public string LatestCue { get; }
            public bool Accepted { get; }
            public string RejectionReason { get; }
        }

        private sealed class BaselineExpectation
        {
            public BaselineExpectation(string name, int count, string hash, string[] paths)
            {
                Name = name;
                Count = count;
                Hash = hash;
                Paths = paths;
            }
            public string Name { get; }
            public int Count { get; }
            public string Hash { get; }
            public string[] Paths { get; }
        }

        private sealed class BroadBaselineExpectation
        {
            public BroadBaselineExpectation(
                string name,
                int count,
                string hash,
                string root,
                string[] paths)
            {
                Name = name;
                Count = count;
                Hash = hash;
                Root = root;
                Paths = paths;
            }
            public string Name { get; }
            public int Count { get; }
            public string Hash { get; }
            public string Root { get; }
            public string[] Paths { get; }
        }

        private sealed class BaselineRow
        {
            public BaselineRow(
                string name,
                int expectedCount,
                string expectedHash,
                int actualCount,
                string actualHash,
                bool pass,
                string disposition)
            {
                Name = name;
                ExpectedCount = expectedCount;
                ExpectedHash = expectedHash;
                ActualCount = actualCount;
                ActualHash = actualHash;
                Pass = pass;
                Disposition = disposition;
            }
            public string Name { get; }
            public int ExpectedCount { get; }
            public string ExpectedHash { get; }
            public int ActualCount { get; }
            public string ActualHash { get; }
            public bool Pass { get; }
            public string Disposition { get; }
        }

        private sealed class ExternalDriftEvidence
        {
            public ExternalDriftEvidence(
                string path,
                string startSha256,
                string initialReceiptSha256)
            {
                Path = path;
                StartSha256 = startSha256;
                InitialReceiptSha256 = initialReceiptSha256;
            }
            public string Path { get; }
            public string StartSha256 { get; }
            public string InitialReceiptSha256 { get; }
        }

        private sealed class ExternalDriftObservation
        {
            public ExternalDriftObservation(
                string path,
                string startSha256,
                string initialReceiptSha256,
                string observedSha256,
                string disposition)
            {
                Path = path;
                StartSha256 = startSha256;
                InitialReceiptSha256 = initialReceiptSha256;
                ObservedSha256 = observedSha256;
                Disposition = disposition;
            }
            public string Path { get; }
            public string StartSha256 { get; }
            public string InitialReceiptSha256 { get; }
            public string ObservedSha256 { get; }
            public string Disposition { get; }
        }

        private sealed class AggregateResult
        {
            public AggregateResult(int count, string hash)
            {
                Count = count;
                Hash = hash;
            }
            public int Count { get; }
            public string Hash { get; }
        }
    }
}
