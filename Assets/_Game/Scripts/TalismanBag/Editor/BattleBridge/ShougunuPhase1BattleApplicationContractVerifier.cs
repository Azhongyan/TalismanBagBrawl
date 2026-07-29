using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BattleBridge
{
    public static class ShougunuPhase1BattleApplicationContractVerifier
    {
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/"
            + "ItemBalanceWorkbenchCatalog.asset";
        private const string AssignmentPath =
            "Docs/V0.4/ShougunuPhase1BattleApplicationContract01_Assignment.md";
        private const string AssignmentHash =
            "e1885df02293773ec9eae4bf3eaa1b632d1261102675418f755e957cb444ea1a";
        private const string ItemAssignmentPath =
            "Docs/V0.4/ItemCombatEffectRequestContract01_Assignment.md";
        private const string ItemAssignmentHash =
            "6dd4cad1fc930d0b969e9ff92455b1cbbdb52a40f8b4809add69819b7e9ec35e";
        private const string EnemyAssignmentPath =
            "Docs/V0.4/ShougunuPhase1RuntimeAndActionContract01_Assignment.md";
        private const string EnemyAssignmentHash =
            "40f244475a2aabadffb8ba440c011ec8a7dae46c69043db7f93e928788c0c757";
        private const string CultivationSource =
            "BattleSandboxDevHandtestAllLv40";

        private const string ReportPath =
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractSpec.csv";
        private const string NominalPath =
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationNominalTrace.csv";
        private const string PlayerPath =
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationPlayerTrace.csv";
        private const string DisplayPath =
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationDisplayEvents.csv";
        private const string NegativePath =
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationNegativeFixtures.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ShougunuPhase1BattleApplicationLeakCheckReport.md";

        private static readonly string[] ExpectedItems =
        {
            "I007", "I008", "I009", "I010", "I011", "I012"
        };

        private static readonly int[] ExpectedDamage =
        {
            29, 42, 53, 46, 55, 76
        };

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/"
                + "ShougunuPhase1BattleApplicationContracts.cs",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                + "ShougunuPhase1BattleApplicationEngine.cs"
        };

        private static readonly string[] PackagePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/"
                + "ShougunuPhase1BattleApplicationContracts.cs",
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/"
                + "ShougunuPhase1BattleApplicationContracts.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge.meta",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1.meta",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                + "ShougunuPhase1BattleApplicationEngine.cs",
            "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/"
                + "ShougunuPhase1BattleApplicationEngine.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/"
                + "ShougunuPhase1BattleApplicationContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/"
                + "ShougunuPhase1BattleApplicationContractVerifier.cs.meta",
            ReportPath,
            SpecPath,
            NominalPath,
            PlayerPath,
            DisplayPath,
            NegativePath,
            LeakPath
        };

        private static readonly string[] ForbiddenRuntimeReferences =
        {
            "AutoCombatController",
            "MainTrialFlowService",
            "V02RunFlowController",
            "V03NavigationFlowController",
            "SaveData",
            "PlayerPrefs",
            "RewardService",
            "RewardConfig",
            "DropTable",
            "BossInfoPanel",
            "Scene_TalismanBag_V04_BattleSandboxPreview",
            "UnityEngine.UI",
            "GameObject.Find",
            "FindObjectOfType",
            "Resources.Load",
            "Visual"
        };

        private static readonly List<ScenarioResult> Results = new();
        private static readonly List<NegativeResult> Negatives = new();
        private static Fixture fixture;
        private static ShougunuPhase1BattleApplicationTrace nominal;

        [MenuItem(
            "Talisman Bag/V0.4/Verify Shougunu Phase1 Battle Application Contract")]
        public static void VerifyStaticBatch()
        {
            Results.Clear();
            Negatives.Clear();
            fixture = BuildFixture(CurrentPlacements());
            nominal = ShougunuPhase1BattleApplicationEngine
                .Create(fixture.Request)
                .RunNominalTrace();

            Run("S01", "ASSIGNMENT_AND_UPSTREAM_HASHES_PASS",
                VerifyAssignmentAndUpstreamHashes);
            Run("S02", "TERMINAL_SCHEMAS_AND_CONSTANTS_PASS",
                VerifyTerminalSchemasAndConstants);
            Run("S03", "REAL_ITEM_SNAPSHOT_CONSUMED_PASS",
                VerifyRealItemSnapshot);
            Run("S04", "IMMUTABLE_BATTLE_CONTRACTS_PASS",
                VerifyImmutability);
            Run("S05", "NOMINAL_75S_50_APPLICATIONS_PASS",
                VerifyNominalItemApplications);
            Run("S06", "SIX_SKILLS_TWELVE_BASICS_SEVEN_BREAKS_PASS",
                VerifyNominalEnemyOrdering);
            Run("S07", "PLAYER_9999_DAMAGE_AND_SURVIVAL_PASS",
                VerifyPlayerTrace);
            Run("S08", "DISPLAY_EVENT_MODEL_PASS",
                VerifyDisplayEvents);
            Run("S09", "NEGATIVE_AND_RESET_FIXTURES_PASS",
                VerifyNegativeAndResetFixtures);
            Run("S10", "STATIC_FORBIDDEN_REFERENCE_SCAN_PASS",
                VerifyForbiddenReferenceScan);
            Run("S11", "REPORTS_REPRODUCIBLE_PASS",
                VerifyReportsReproducible);
            WriteReports();
            Run("S12", "PACKAGE_SCOPED_TEXT_CHECK_PASS",
                VerifyPackageScopedTextCheck);
            WriteReports();
            ScenarioResult[] failures = Results.Where(value => !value.Passed)
                .ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "Shougunu Phase1 Battle application verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }

            Debug.Log(
                "[ShougunuPhase1BattleApplicationContractVerifier]\n"
                + string.Join("\n", Results.Select(value => value.Marker))
                + "\nSHOUGUNU_PHASE1_BATTLE_APPLICATION_CONTRACT_PASS"
                + "\nUSER_HANDTEST_NOT_APPLICABLE"
                + "\nSCENE_TOUCHED_FALSE"
                + "\nP3_NOT_STARTED");
        }

        public static void RunFromCommandLine()
        {
            try
            {
                VerifyStaticBatch();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void VerifyAssignmentAndUpstreamHashes()
        {
            Check(HashFile(ProjectPath(AssignmentPath)) == AssignmentHash,
                "Battle Assignment SHA-256 mismatch.");
            Check(HashFile(ProjectPath(ItemAssignmentPath)) == ItemAssignmentHash,
                "Item Assignment SHA-256 mismatch.");
            Check(HashFile(ProjectPath(EnemyAssignmentPath)) == EnemyAssignmentHash,
                "Enemy Assignment SHA-256 mismatch.");
        }

        private static void VerifyTerminalSchemasAndConstants()
        {
            Check(ItemCombatEffectRequestSnapshot.CurrentSchemaId
                    == "ItemCombatEffectRequestSnapshot.v1",
                "Item terminal schema mismatch.");
            Check(ShougunuPhase1RuntimeContract.SchemaId
                    == "ShougunuPhase1RuntimeAndActionContract.v1",
                "Enemy terminal schema mismatch.");
            Check(ShougunuPhase1RuntimeContract.MaxHp == 980,
                "Enemy max HP mismatch.");
            Check(ShougunuPhase1RuntimeContract.ShellLayerMax == 200,
                "Enemy shell max mismatch.");
            Check(ShougunuPhase1RuntimeContract.MaxSequentialShellLayers == 7,
                "Enemy shell layer count mismatch.");
            Check(ShougunuPhase1RuntimeContract.CoreExposeDurationTicks == 6000,
                "Enemy expose duration mismatch.");
            Check(ShougunuPhase1RuntimeContract.Skill1RepairUnits == 30,
                "Enemy repair amount mismatch.");
            Check(ShougunuPhase1RuntimeContract.BasicDamageApplicationInterval == 4,
                "Enemy Basic interval mismatch.");
            Check(ShougunuPhase1BattleApplicationEngine.TicksPerSecond == 1000
                    && ShougunuPhase1BattleApplicationEngine
                        .ItemPulseIntervalTicks == 1500L
                    && ShougunuPhase1BattleApplicationEngine
                        .NominalDurationTicks == 75000L
                    && ShougunuPhase1BattleApplicationEngine
                        .NominalAcceptedItemApplications == 50,
                "Battle clock constants mismatch.");
            Check(BattleSandboxPlayerCombatantSnapshot.CurrentSchemaId
                    == "BattleSandboxPlayerCombatantSnapshot.v1"
                    && BattleSandboxPlayerCombatantSnapshot.FixtureMaxHp == 9999,
                "Player contract mismatch.");
        }

        private static void VerifyRealItemSnapshot()
        {
            ItemCombatEffectRequestSnapshot request = fixture.Request;
            Check(request.schemaId
                    == ItemCombatEffectRequestSnapshot.CurrentSchemaId
                    && request.status
                        == ItemCombatEffectRequestSnapshotStatus.Valid
                    && request.devOnly
                    && !request.entersFormalBattle,
                "Real Item snapshot is not valid devOnly terminal input.");
            Check(request.Requests.Count == 6,
                "Real Item request count must be six.");
            for (int index = 0; index < ExpectedItems.Length; index++)
            {
                ItemCombatEffectRequestRow row = request.Requests.Single(value =>
                    string.Equals(
                        value.sourceBaseItemId,
                        ExpectedItems[index],
                        StringComparison.Ordinal));
                Check(row.resolvedPreMitigationDamageUnits
                        == ExpectedDamage[index],
                    "Real Item magnitude mismatch for " + ExpectedItems[index]);
                Check(row.isLitFact
                        == ItemInstanceQualifiedBuildBooleanFact.True
                        && row.sourceIsCountedFact
                            == ItemInstanceQualifiedBuildBooleanFact.True,
                    "Real Item accepted-lit/source truth mismatch for "
                    + ExpectedItems[index]);
                ItemInstanceQualifiedBuildBooleanFact expectedQualified =
                    string.Equals(
                        ExpectedItems[index],
                        "I011",
                        StringComparison.Ordinal)
                        ? ItemInstanceQualifiedBuildBooleanFact.False
                        : ItemInstanceQualifiedBuildBooleanFact.True;
                Check(row.qualifiedIsCountedFact == expectedQualified,
                    "Real Item qualified truth mismatch for "
                    + ExpectedItems[index]);
            }
        }

        private static void VerifyImmutability()
        {
            Type[] contracts =
            {
                typeof(BattleSandboxPlayerCombatantSnapshot),
                typeof(BattleSandboxPlayerEffectApplicationResult),
                typeof(ShougunuPhase1BattleApplicationContext),
                typeof(ShougunuPhase1ItemApplicationResult),
                typeof(ShougunuPhase1EnemyActionApplicationResult),
                typeof(ShougunuPhase1BattleDisplayEvent),
                typeof(ShougunuPhase1BattleApplicationLedger),
                typeof(ShougunuPhase1BattleApplicationTrace)
            };
            foreach (Type contract in contracts)
            {
                Check(contract.IsSealed, contract.Name + " is not sealed.");
                Check(contract.GetFields(
                        BindingFlags.Instance
                        | BindingFlags.Public).Length == 0,
                    contract.Name + " exposes mutable public fields.");
                Check(contract.GetProperties(
                        BindingFlags.Instance
                        | BindingFlags.Public)
                        .All(value => !value.CanWrite),
                    contract.Name + " exposes writable properties.");
            }
            AssertReadOnly(
                nominal.Rows,
                "Trace.Rows");
            AssertReadOnly(
                nominal.finalContext.ledger.Events,
                "Ledger.Events");
            AssertReadOnly(
                nominal.finalContext.DisplayEvents,
                "DisplayEvents");
            AssertReadOnly(
                nominal.PlayerResults,
                "PlayerResults");
            AssertReadOnly(
                nominal.EnemyActionResults,
                "EnemyActionResults");
        }

        private static void VerifyNominalItemApplications()
        {
            Check(nominal.Rows.Count == 50, "Nominal trace row count is not 50.");
            Check(nominal.finalContext.battleTick == 75000L,
                "Nominal final Battle tick is not 75000.");
            Check(nominal.finalContext.enemy.AcceptedDamageApplicationCount == 50,
                "Enemy accepted application count is not 50.");
            for (int index = 0; index < nominal.Rows.Count; index++)
            {
                ShougunuPhase1BattleApplicationTraceRow row =
                    nominal.Rows[index];
                int itemIndex = index % ExpectedItems.Length;
                Check(row.applicationSequence == index + 1L,
                    "Application sequence is unstable at " + (index + 1));
                Check(row.battleTick == (index + 1L) * 1500L,
                    "Pulse tick is unstable at " + (index + 1));
                Check(row.sourceBaseItemId == ExpectedItems[itemIndex],
                    "Item order mismatch at " + (index + 1));
                Check(row.resolvedPreMitigationDamageUnits
                        == ExpectedDamage[itemIndex],
                    "Item magnitude mismatch at " + (index + 1));
                Check(row.accepted, "Nominal Item request rejected at "
                    + (index + 1) + ": " + row.reason);
            }
        }

        private static void VerifyNominalEnemyOrdering()
        {
            ShougunuPhase1RuntimeSnapshot enemy = nominal.finalContext.enemy;
            Check(enemy.Lifecycle == ShougunuPhase1LifecycleState.Defeated,
                "Enemy final lifecycle is not Defeated.");
            Check(enemy.CurrentHp == 0,
                "Enemy final HP is not zero.");
            Check(enemy.BasicEarnedCount == 12
                    && enemy.BasicResolvedCount == 12
                    && enemy.BasicDebt == 0,
                "Basic 12/12/0 terminal fact mismatch.");
            Check(enemy.ShellLayerIndex == 7,
                "Seven shell breaks were not preserved.");
            Check(enemy.ThresholdOccurrences.Count == 6
                    && enemy.ThresholdOccurrences.All(value => value.Resolved),
                "Six threshold occurrences did not resolve.");

            string basic = ShougunuPhase1ActionPatternCatalog.BasicAttack;
            string skill1 =
                ShougunuPhase1ActionPatternCatalog.Skill1ShellRepair;
            string skill2 =
                ShougunuPhase1ActionPatternCatalog.Skill2RopeHeavyStrike;
            string skill3 =
                ShougunuPhase1ActionPatternCatalog.Skill3GroundSealBurst;
            Check(ActionCount(basic) == 12, "Basic resolve count is not 12.");
            Check(ActionCount(skill1) == 2, "Skill1 resolve count is not 2.");
            Check(ActionCount(skill2) == 2, "Skill2 resolve count is not 2.");
            Check(ActionCount(skill3) == 2, "Skill3 resolve count is not 2.");
            Check(nominal.EnemyActionResults
                    .Where(value => value.actionPatternId == skill1)
                    .All(value => value.enemyShellRepairApplied >= 0
                        && value.enemyShellRepairApplied
                            <= ShougunuPhase1RuntimeContract.Skill1RepairUnits),
                "Skill1 repair escaped reducer-bounded 0..30 range.");
            Check(nominal.EnemyActionResults
                    .Where(value => value.actionPatternId == skill1)
                    .Any(value => value.enemyShellRepairApplied > 0),
                "Skill1 never repaired the current shell.");

            long secondSkill3Tick = nominal.EnemyActionResults
                .Where(value => value.actionPatternId == skill3)
                .OrderBy(value => value.battleTick)
                .Last().battleTick;
            long basic12Tick = nominal.EnemyActionResults
                .Where(value => value.actionPatternId == basic)
                .OrderBy(value => value.battleTick)
                .Last().battleTick;
            ShougunuPhase1BattleLedgerEvent defeated =
                nominal.finalContext.ledger.Events.Last(value =>
                    value.eventKind
                        == ShougunuPhase1BattleLedgerEventKind.Defeat
                    && value.decision
                        == ShougunuPhase1BattleApplicationDecision.Accepted);
            Check(secondSkill3Tick < defeated.battleTick,
                "S3-B did not resolve before Defeated.");
            Check(basic12Tick < defeated.battleTick,
                "Basic12 did not resolve before Defeated.");
            Check(defeated.battleTick == 75000L,
                "Defeated was not committed at tick 75000.");
        }

        private static void VerifyPlayerTrace()
        {
            ShougunuPhase1BattleApplicationSession fresh =
                ShougunuPhase1BattleApplicationEngine.Create(fixture.Request);
            Check(fresh.Current.player.maxHp == 9999
                    && fresh.Current.player.currentHp == 9999
                    && fresh.Current.player.shield == 0
                    && !fresh.Current.player.defeated,
                "Player activation is not 9999/9999.");
            Check(nominal.PlayerResults.Count == 16,
                "Expected 12 Basic + 2 Skill2 + 2 Skill3 player hits.");
            Check(nominal.PlayerResults.All(value =>
                    value.decision
                        == ShougunuPhase1BattleApplicationDecision.Accepted
                    && value.hpDamageApplied > 0
                    && value.afterHp < value.beforeHp),
                "Player did not take normal clamped damage.");
            Check(nominal.finalContext.player.currentHp > 0
                    && nominal.finalContext.player.currentHp < 9999
                    && !nominal.finalContext.player.defeated,
                "Nominal player survival fixture is invalid.");
            Check(nominal.PlayerResults
                    .Count(value => value.actionPatternId
                        == ShougunuPhase1ActionPatternCatalog.BasicAttack) == 12,
                "Basic player-hit count mismatch.");
            Check(nominal.PlayerResults
                    .Count(value => value.actionPatternId
                        == ShougunuPhase1ActionPatternCatalog
                            .Skill2RopeHeavyStrike) == 2,
                "Skill2 direct player-hit count mismatch.");
            Check(nominal.PlayerResults
                    .Count(value => value.actionPatternId
                        == ShougunuPhase1ActionPatternCatalog
                            .Skill3GroundSealBurst) == 2,
                "Skill3 area-burst player-hit count mismatch.");

            BattleSandboxPlayerCombatantSnapshot fragile =
                new BattleSandboxPlayerCombatantSnapshot(
                    7,
                    1L,
                    ShougunuPhase1BattleApplicationEngine.PlayerActorId,
                    1,
                    5,
                    false,
                    string.Empty,
                    0,
                    new[] { "explicit clamp/Defeated verifier fixture" });
            BattleSandboxPlayerCombatantSnapshot defeated =
                BattleSandboxPlayerCombatantReducer.ApplyEnemyDamage(
                    fragile,
                    7,
                    "fixture.enemy.overkill",
                    9999);
            Check(defeated.currentHp == 0
                    && defeated.shield == 0
                    && defeated.defeated
                    && defeated.acceptedEnemyEffectCount == 1,
                "Player overkill did not clamp shield/HP and set Defeated.");
        }

        private static void VerifyDisplayEvents()
        {
            IReadOnlyList<ShougunuPhase1BattleDisplayEvent> events =
                nominal.finalContext.DisplayEvents;
            ShougunuPhase1BattleDisplayChannel[] requiredChannels =
            {
                ShougunuPhase1BattleDisplayChannel.ItemToEnemyShellDamage,
                ShougunuPhase1BattleDisplayChannel.ItemToEnemyHpDamage,
                ShougunuPhase1BattleDisplayChannel.EnemyShellBreak,
                ShougunuPhase1BattleDisplayChannel.EnemyCoreExpose,
                ShougunuPhase1BattleDisplayChannel.EnemyRecover,
                ShougunuPhase1BattleDisplayChannel.EnemyDefeated,
                ShougunuPhase1BattleDisplayChannel.EnemyToPlayerDamage,
                ShougunuPhase1BattleDisplayChannel.EnemyToPlayerStatusOrArea,
                ShougunuPhase1BattleDisplayChannel.EnemySkillIntent,
                ShougunuPhase1BattleDisplayChannel.EnemySkillResolved
            };
            foreach (ShougunuPhase1BattleDisplayChannel channel
                in requiredChannels)
            {
                Check(events.Any(value => value.channel == channel),
                    "Missing display channel " + channel);
            }
            ShougunuPhase1BattleAnchorRoute[] requiredAnchors =
            {
                ShougunuPhase1BattleAnchorRoute.DamageDealtAnchor,
                ShougunuPhase1BattleAnchorRoute.ShieldBreakAnchor,
                ShougunuPhase1BattleAnchorRoute.DamageTakenAnchor,
                ShougunuPhase1BattleAnchorRoute.StatusDamageAnchor,
                ShougunuPhase1BattleAnchorRoute.MechanicFloatingText,
                ShougunuPhase1BattleAnchorRoute.PlayerHitFeedback
            };
            foreach (ShougunuPhase1BattleAnchorRoute anchor
                in requiredAnchors)
            {
                Check(events.Any(value => value.anchorRoute == anchor),
                    "Missing anchor route " + anchor);
            }
            Check(events.All(value => value.resetGeneration == 1),
                "Nominal display generation mismatch.");
        }

        private static void VerifyNegativeAndResetFixtures()
        {
            ItemCombatEffectRequestRow row = TerminalRow("I007");
            ShougunuPhase1ItemRequestSourceFacts realFacts =
                ShougunuPhase1ItemRequestSourceFacts.FromSnapshot(
                    fixture.Request);
            ShougunuPhase1BattleApplicationSession session =
                ShougunuPhase1BattleApplicationEngine.Create(fixture.Request);
            ShougunuPhase1ItemApplicationResult accepted = Apply(
                session, realFacts, row, "request.seed", "application.seed",
                ShougunuPhase1BattleApplicationEngine.EnemyInstanceId,
                1, 1L, 1L, session.Current.enemy.Revision);
            Check(accepted.accepted, "Negative fixture seed was rejected.");
            int acceptedCount = session.Current.enemy
                .AcceptedDamageApplicationCount;

            RecordNegative("duplicate_request", Apply(
                session, realFacts, row, "request.seed", "application.dup.req",
                ShougunuPhase1BattleApplicationEngine.EnemyInstanceId,
                1, 2L, 2L, session.Current.enemy.Revision),
                "DUPLICATE_REQUEST_REJECTED", acceptedCount);
            RecordNegative("duplicate_application", Apply(
                session, realFacts, row, "request.dup.app", "application.seed",
                ShougunuPhase1BattleApplicationEngine.EnemyInstanceId,
                1, 2L, 2L, session.Current.enemy.Revision),
                "DUPLICATE_APPLICATION_REJECTED", acceptedCount);
            RecordNegative("stale_reset_generation", ApplyFresh(
                realFacts, row, "stale.gen", 0, null, null),
                "STALE_RESET_GENERATION_REJECTED", 0);
            RecordNegative("stale_enemy_revision", ApplyFresh(
                realFacts, row, "stale.rev", 1, 0L, null),
                "STALE_ENEMY_REVISION_REJECTED", 0);
            RecordNegative("wrong_target", ApplyFresh(
                realFacts, row, "wrong.target", 1, null, "other.enemy"),
                "INVALID_TARGET_REJECTED", 0);

            ShougunuPhase1ItemRequestSourceFacts formal =
                new ShougunuPhase1ItemRequestSourceFacts(
                    ItemCombatEffectRequestSnapshot.CurrentSchemaId,
                    ItemCombatEffectRequestSnapshotStatus.Valid,
                    true,
                    true,
                    "formal.fixture");
            RecordNegative("formal_request", ApplyFresh(
                formal, row, "formal", 1, null, null),
                "FORMAL_ITEM_REQUEST_REJECTED", 0);
            ShougunuPhase1ItemRequestSourceFacts nonDev =
                new ShougunuPhase1ItemRequestSourceFacts(
                    ItemCombatEffectRequestSnapshot.CurrentSchemaId,
                    ItemCombatEffectRequestSnapshotStatus.Valid,
                    false,
                    false,
                    "nondev.fixture");
            RecordNegative("non_dev_request", ApplyFresh(
                nonDev, row, "nondev", 1, null, null),
                "ITEM_SNAPSHOT_NOT_DEV_ONLY", 0);

            RecordNegative("zero_damage", ApplyFresh(
                realFacts, CloneDamage(row, 0L), "zero", 1, null, null),
                "ZERO_DAMAGE_REJECTED", 0);
            RecordNegative("negative_damage", ApplyFresh(
                realFacts, CloneDamage(row, -1L), "negative", 1, null, null),
                "NEGATIVE_DAMAGE_REJECTED", 0);
            RecordNegative("forged_qualified_truth", ApplyFresh(
                realFacts,
                CloneRow(
                    row,
                    row.resolvedPreMitigationDamageUnits,
                    ItemInstanceQualifiedBuildBooleanFact.False),
                "forged.qualified",
                1,
                null,
                null),
                "ITEM_ROW_NOT_FROM_TERMINAL_SNAPSHOT",
                0);

            ShougunuPhase1BattleApplicationSession telemetrySession =
                ShougunuPhase1BattleApplicationEngine.Create(fixture.Request);
            foreach (ItemCombatEffectUnsupportedTelemetry telemetry
                in UnsupportedFixtures())
            {
                int before = telemetrySession.Current.enemy
                    .AcceptedDamageApplicationCount;
                ShougunuPhase1BattleLedgerEvent observed =
                    telemetrySession.ObserveUnsupportedTelemetry(telemetry, 0L);
                Check(observed.decision
                        == ShougunuPhase1BattleApplicationDecision
                            .IgnoredDiagnosticOnly
                        && observed.amount == 0
                        && telemetrySession.Current.enemy
                            .AcceptedDamageApplicationCount == before,
                    "Unsupported telemetry became damage.");
                Negatives.Add(new NegativeResult(
                    "unsupported_" + telemetry.disposition + "_"
                        + telemetry.magnitudePresence,
                    observed.decision.ToString(),
                    observed.reason,
                    0,
                    before,
                    true));
            }

            ShougunuPhase1BattleApplicationSession resetSession =
                ShougunuPhase1BattleApplicationEngine.Create(fixture.Request);
            ShougunuPhase1ItemApplicationResult old = Apply(
                resetSession,
                realFacts,
                row,
                "request.old.gen",
                "application.old.gen",
                ShougunuPhase1BattleApplicationEngine.EnemyInstanceId,
                1,
                1L,
                1L,
                resetSession.Current.enemy.Revision);
            Check(old.accepted, "Reset fixture seed failed.");
            ShougunuPhase1BattleApplicationContext reset =
                resetSession.Reset(100L);
            Check(reset.resetGeneration == 2
                    && reset.player.maxHp == 9999
                    && reset.player.currentHp == 9999
                    && reset.player.shield == 0
                    && !reset.player.defeated,
                "Reset did not restore player 9999/9999.");
            Check(reset.ledger.Events.All(value =>
                    value.resetGeneration == 2)
                    && reset.DisplayEvents.All(value =>
                        value.resetGeneration == 2)
                    && !reset.ledger.Events.Any(value =>
                        value.canonicalEvidence.Contains(
                            "application.old.gen")),
                "Old-generation event leaked after reset.");
            ShougunuPhase1ItemApplicationResult replay = Apply(
                resetSession,
                realFacts,
                row,
                "request.old.gen",
                "application.old.gen",
                ShougunuPhase1BattleApplicationEngine.EnemyInstanceId,
                1,
                1L,
                1L,
                resetSession.Current.enemy.Revision);
            RecordNegative(
                "old_generation_replay",
                replay,
                "STALE_RESET_GENERATION_REJECTED",
                0);
            Check(Negatives.All(value => value.Passed),
                "One or more negative fixtures failed.");
        }

        private static void VerifyForbiddenReferenceScan()
        {
            foreach (string path in RuntimeSourcePaths)
            {
                string source = Read(path);
                foreach (string forbidden in ForbiddenRuntimeReferences)
                {
                    Check(source.IndexOf(
                            forbidden,
                            StringComparison.Ordinal) < 0,
                        path + " references forbidden symbol " + forbidden);
                }
                Check(source.IndexOf(".unity", StringComparison.OrdinalIgnoreCase)
                        < 0,
                    path + " references a Scene asset.");
                Check(source.IndexOf(".prefab", StringComparison.OrdinalIgnoreCase)
                        < 0,
                    path + " references a Prefab asset.");
            }
        }

        private static void VerifyPackageScopedTextCheck()
        {
            foreach (string path in PackagePaths.Where(value =>
                value.EndsWith(".cs", StringComparison.Ordinal)
                || value.EndsWith(".md", StringComparison.Ordinal)
                || value.EndsWith(".csv", StringComparison.Ordinal)
                || value.EndsWith(".meta", StringComparison.Ordinal)))
            {
                if (!File.Exists(ProjectPath(path)))
                {
                    continue;
                }
                string text = Read(path);
                Check(!text.Contains(new string('<', 7))
                        && !text.Contains(new string('=', 7))
                        && !text.Contains(new string('>', 7)),
                    path + " contains a merge marker.");
                string[] lines = text.Replace("\r\n", "\n").Split('\n');
                Check(lines.All(value =>
                        value.Length == value.TrimEnd(' ', '\t').Length),
                    path + " contains trailing whitespace.");
            }
        }

        private static void VerifyReportsReproducible()
        {
            IReadOnlyDictionary<string, string> first = BuildReports();
            IReadOnlyDictionary<string, string> second = BuildReports();
            Check(first.Count == 7 && second.Count == 7,
                "Expected seven reports.");
            foreach (KeyValuePair<string, string> pair in first)
            {
                Check(second.TryGetValue(pair.Key, out string text)
                        && HashText(pair.Value) == HashText(text),
                    "Report is not reproducible: " + pair.Key);
            }
        }

        private static int ActionCount(string actionPatternId)
        {
            return nominal.EnemyActionResults.Count(value =>
                value.decision
                    == ShougunuPhase1BattleApplicationDecision.Accepted
                && string.Equals(
                    value.actionPatternId,
                    actionPatternId,
                    StringComparison.Ordinal));
        }

        private static ItemCombatEffectRequestRow TerminalRow(string baseId)
        {
            return fixture.Request.Requests.Single(value =>
                string.Equals(
                    value.sourceBaseItemId,
                    baseId,
                    StringComparison.Ordinal));
        }

        private static ShougunuPhase1ItemApplicationResult ApplyFresh(
            ShougunuPhase1ItemRequestSourceFacts facts,
            ItemCombatEffectRequestRow row,
            string id,
            int generation,
            long? expectedRevision,
            string target)
        {
            ShougunuPhase1BattleApplicationSession fresh =
                ShougunuPhase1BattleApplicationEngine.Create(fixture.Request);
            return Apply(
                fresh,
                facts,
                row,
                "request." + id,
                "application." + id,
                target
                    ?? ShougunuPhase1BattleApplicationEngine.EnemyInstanceId,
                generation,
                1L,
                1L,
                expectedRevision ?? fresh.Current.enemy.Revision);
        }

        private static ShougunuPhase1ItemApplicationResult Apply(
            ShougunuPhase1BattleApplicationSession session,
            ShougunuPhase1ItemRequestSourceFacts facts,
            ItemCombatEffectRequestRow row,
            string requestEventId,
            string applicationEventId,
            string target,
            int generation,
            long tick,
            long sequence,
            long expectedRevision)
        {
            return session.ApplyItemRequest(
                facts,
                row,
                requestEventId,
                applicationEventId,
                target,
                ShougunuPhase1RuntimeContract.ContentId,
                ShougunuPhase1RuntimeContract.PhaseId,
                generation,
                tick,
                sequence,
                expectedRevision);
        }

        private static void RecordNegative(
            string name,
            ShougunuPhase1ItemApplicationResult result,
            string expectedReason,
            int expectedAcceptedCount)
        {
            bool passed = result.decision
                    == ShougunuPhase1BattleApplicationDecision.Rejected
                && string.Equals(
                    result.reason,
                    expectedReason,
                    StringComparison.Ordinal)
                && result.shellDamageApplied == 0
                && result.hpDamageApplied == 0
                && result.enemy.AcceptedDamageApplicationCount
                    == expectedAcceptedCount;
            Negatives.Add(new NegativeResult(
                name,
                result.decision.ToString(),
                result.reason,
                result.shellDamageApplied + result.hpDamageApplied,
                result.enemy.AcceptedDamageApplicationCount,
                passed));
        }

        private static IEnumerable<ItemCombatEffectUnsupportedTelemetry>
            UnsupportedFixtures()
        {
            yield return new ItemCombatEffectUnsupportedTelemetry(
                "fixture.item",
                "I007",
                "fixture.placement",
                "fixture.unknown",
                ItemCombatEffectUnsupportedSourceKind.TextSemantic,
                "unknown",
                "none",
                ItemCombatEffectMagnitudePresence.Missing,
                null,
                ItemCombatEffectUnsupportedDisposition.Unknown,
                "FIXTURE_UNKNOWN",
                new[] { "fixture.unknown" });
            yield return new ItemCombatEffectUnsupportedTelemetry(
                "fixture.item",
                "I007",
                "fixture.placement",
                "fixture.not_executed",
                ItemCombatEffectUnsupportedSourceKind.Core,
                "not_executed",
                "none",
                ItemCombatEffectMagnitudePresence.Present,
                99L,
                ItemCombatEffectUnsupportedDisposition.NotExecuted,
                "FIXTURE_NOT_EXECUTED",
                new[] { "fixture.not_executed" });
            yield return new ItemCombatEffectUnsupportedTelemetry(
                "fixture.item",
                "I007",
                "fixture.placement",
                "fixture.conflicted",
                ItemCombatEffectUnsupportedSourceKind.Affix,
                "conflicted",
                "raw",
                ItemCombatEffectMagnitudePresence.Conflicted,
                999L,
                ItemCombatEffectUnsupportedDisposition.Rejected,
                "FIXTURE_CONFLICTED",
                new[] { "fixture.conflicted" });
        }

        private static ItemCombatEffectRequestRow CloneDamage(
            ItemCombatEffectRequestRow source,
            long damage)
        {
            return CloneRow(source, damage, source.qualifiedIsCountedFact);
        }

        private static ItemCombatEffectRequestRow CloneRow(
            ItemCombatEffectRequestRow source,
            long damage,
            ItemInstanceQualifiedBuildBooleanFact qualifiedIsCountedFact)
        {
            return new ItemCombatEffectRequestRow(
                source.requestId,
                source.requestKind,
                source.sourceItemInstanceId,
                source.sourceBaseItemId,
                source.sourcePlacementId,
                source.sourceRarity,
                source.sourceRootSeed,
                source.sourceFaMenTag,
                source.sourceQiLeiTag,
                source.SourceEffectIds,
                source.baseDamageRawUnits,
                source.additiveBasisPoints,
                damage,
                source.magnitudeCompleteness,
                source.requiresBattleTargetValidation,
                source.targetRequestKind,
                source.isLitFact,
                source.sourceIsCountedFact,
                qualifiedIsCountedFact,
                source.faMenBuildId,
                source.faMenBuildCount,
                source.faMenActiveStagePieceCount,
                source.ActiveCoreEffectIds,
                source.sourceProjectionCanonicalSignature,
                source.sourcePlacementCanonicalFacts,
                source.Contributions);
        }

        private static Fixture BuildFixture(
            IReadOnlyList<ItemSystemPlacementInput> placementInputs)
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            Check(workbench != null, "Workbench catalog is missing.");
            GameObject providerObject =
                new GameObject("ShougunuBattleApplicationVerifierProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult view =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbench,
                        provider);
                Check(view.IsValid, "Real ProjectionSet invalid: "
                    + string.Join("|", view.Diagnostics));

                ItemCoreEffectIdentityCatalogSnapshot identity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                Check(identity.isValid, "Core identity catalog invalid.");
                ItemCoreEffectCultivationRosterSnapshot cultivation =
                    new ItemCoreEffectCultivationRosterSnapshot(
                        ItemCoreEffectRosterCompleteness.Complete,
                        view.OrdinaryProjectionSet.Projections.Select(value =>
                            new ItemCoreEffectCultivationRow(
                                value.itemInstanceId,
                                value.baseItemId,
                                40,
                                CultivationSource,
                                ItemCoreEffectFactCompleteness.Complete)));
                Check(cultivation.isValid, "Cultivation roster invalid.");

                string[] placedOrdinary = placementInputs
                    .Where(value => value != null
                        && !string.Equals(
                            value.itemId,
                            "I031",
                            StringComparison.Ordinal))
                    .Select(value => value.itemId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                Dictionary<string, string> placementByBase =
                    placementInputs.Where(value => value != null)
                        .ToDictionary(
                            value => value.itemId,
                            value => value.placementId,
                            StringComparer.Ordinal);
                ItemCoreAwakeningInput[] awakeningInputs =
                    placedOrdinary.Select(baseId =>
                        new ItemCoreAwakeningInput(
                            baseId,
                            placementByBase[baseId],
                            40,
                            true,
                            CultivationSource,
                            "orange")).ToArray();
                ItemSystemSnapshot itemSystem =
                    DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                        new ItemSystemSnapshotInput(
                            placementInputs,
                            ItemSystemBoardConfigInput.Default(),
                            awakeningInputs,
                            null,
                            ItemInnerDataCatalog.AllItems,
                            new[]
                            {
                                I031InventoryPlacementContract.OwnedBoard()
                            }));
                Check(itemSystem.isValid, "Production ItemSystem invalid: "
                    + string.Join("|", itemSystem.validationErrors.Select(
                        value => value.ToDiagnosticString())));

                ItemInstanceProjectionContractSnapshot[] placedProjections =
                    placedOrdinary.Select(baseId =>
                        FindProjection(view, baseId)).ToArray();
                ItemInstanceProjectionSetSnapshot placedProjectionSet =
                    CreateProjectionSet(placedProjections);
                ItemInstancePlacementBindingInput[] bindingInputs =
                    placedProjections.Select(value =>
                        new ItemInstancePlacementBindingInput(
                            value.itemInstanceId,
                            placementByBase[value.baseItemId],
                            value.baseItemId)).ToArray();
                ItemInstancePlacementBindingContractSnapshot binding =
                    ItemInstancePlacementBindingValidator.Instance.Validate(
                        placedProjectionSet,
                        itemSystem,
                        bindingInputs).snapshot;
                Check(binding.isValid, "Production binding invalid.");
                ItemInstanceQualifiedBuildStateSnapshot qualified =
                    ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                        new ItemInstanceQualifiedBuildStateInput(
                            view.OrdinaryProjectionSet,
                            binding,
                            itemSystem,
                            QualifiedBuildRosterCompleteness
                                .CompleteOwnedRoster));
                Check(qualified.isValid, "Production QualifiedBuild invalid.");
                ItemInstanceCoreEffectRuntimeStateSnapshot core =
                    ItemInstanceCoreEffectRuntimeStateAssembler.Instance
                        .Assemble(
                            new ItemInstanceCoreEffectRuntimeStateInput(
                                identity,
                                cultivation,
                                view.OrdinaryProjectionSet,
                                binding,
                                itemSystem,
                                ItemCoreEffectRosterCompleteness.Complete));
                Check(core.isValid, "Production CoreRuntime invalid.");
                ItemCombatEffectRequestSnapshot request =
                    ItemCombatEffectRequestAssembler.Instance.Assemble(
                        view.OrdinaryProjectionSet,
                        itemSystem,
                        qualified,
                        core);
                return new Fixture(request);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static IReadOnlyList<ItemSystemPlacementInput>
            CurrentPlacements()
        {
            return new[]
            {
                new ItemSystemPlacementInput(
                    "P_SYSTEM_I031", "I031", new Vector2Int(0, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I009", "I009", new Vector2Int(1, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I012", "I012", new Vector2Int(2, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I010", "I010", new Vector2Int(1, 1), 90),
                new ItemSystemPlacementInput(
                    "P_BOARD_I008", "I008", new Vector2Int(0, 2), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I011", "I011", new Vector2Int(1, 3), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I007", "I007", new Vector2Int(0, 4), 0)
            };
        }

        private static ItemInstanceProjectionSetSnapshot CreateProjectionSet(
            IEnumerable<ItemInstanceProjectionContractSnapshot> values)
        {
            return CreateNonPublic<ItemInstanceProjectionSetSnapshot>(
                values.ToArray(),
                Array.Empty<ItemInstanceProjectionValidationError>());
        }

        private static T CreateNonPublic<T>(params object[] arguments)
        {
            return (T)Activator.CreateInstance(
                typeof(T),
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic,
                null,
                arguments,
                CultureInfo.InvariantCulture);
        }

        private static ItemInstanceProjectionContractSnapshot FindProjection(
            ItemSystemBattleSandboxViewProjectionResult view,
            string baseId)
        {
            return view.OrdinaryProjectionSet.Projections.Single(value =>
                string.Equals(
                    value.baseItemId,
                    baseId,
                    StringComparison.Ordinal));
        }

        private static void WriteReports()
        {
            foreach (KeyValuePair<string, string> report in BuildReports())
            {
                Write(report.Key, report.Value);
            }
        }

        private static IReadOnlyDictionary<string, string> BuildReports()
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ReportPath] = BuildContractReport(),
                [SpecPath] = BuildSpecCsv(),
                [NominalPath] = BuildNominalCsv(),
                [PlayerPath] = BuildPlayerCsv(),
                [DisplayPath] = BuildDisplayCsv(),
                [NegativePath] = BuildNegativeCsv(),
                [LeakPath] = BuildLeakReport()
            };
        }

        private static string BuildContractReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                    "# Shougunu Phase1 Battle Application Contract Report")
                .AppendLine()
                .AppendLine("- classification = `COMPLEX_GUARDED_ONCE`")
                .AppendLine("- package = `"
                    + ShougunuPhase1BattleApplicationEngine.PackageId + "`")
                .AppendLine("- package status = `"
                    + (Results.All(value => value.Passed)
                        ? "PACKAGE_COMPLETE"
                        : "AUTOMATED_QA_FAILED") + "`")
                .AppendLine("- terminal item schema consumed = `"
                    + fixture.Request.schemaId + "`")
                .AppendLine("- terminal enemy schema consumed = `"
                    + ShougunuPhase1RuntimeContract.SchemaId + "`")
                .AppendLine("- sceneTouched = `false`")
                .AppendLine("- visualTouched = `false`")
                .AppendLine("- formalRouteTouched = `false`")
                .AppendLine("- saveTouched = `false`")
                .AppendLine("- rewardTouched = `false`")
                .AppendLine("- autoCombatTouched = `false`")
                .AppendLine("- user handtest = `NOT_APPLICABLE`")
                .AppendLine("- P3 = `NOT_STARTED`")
                .AppendLine()
                .AppendLine("## Terminal nominal facts")
                .AppendLine()
                .AppendLine("- final tick = `"
                    + nominal.finalContext.battleTick + "`")
                .AppendLine("- accepted Item applications = `"
                    + nominal.Rows.Count + "`")
                .AppendLine("- shell breaks = `"
                    + nominal.finalContext.enemy.ShellLayerIndex + "`")
                .AppendLine("- threshold skills resolved = `"
                    + nominal.finalContext.enemy.ThresholdOccurrences
                        .Count(value => value.Resolved) + "`")
                .AppendLine("- BasicAttack resolved = `"
                    + nominal.finalContext.enemy.BasicResolvedCount + "`")
                .AppendLine("- player HP = `"
                    + nominal.finalContext.player.currentHp + "/"
                    + nominal.finalContext.player.maxHp + "`")
                .AppendLine("- enemy lifecycle = `"
                    + nominal.finalContext.enemy.Lifecycle + "`")
                .AppendLine()
                .AppendLine("## Automated QA")
                .AppendLine()
                .AppendLine("| ID | Marker | Status | Detail |")
                .AppendLine("|---|---|---|---|");
            foreach (ScenarioResult result in Results)
            {
                builder.Append("| ").Append(result.Id).Append(" | ")
                    .Append(result.Marker).Append(" | ")
                    .Append(result.Passed ? "PASS" : "FAIL").Append(" | ")
                    .Append(EscapeMarkdown(result.Detail)).AppendLine(" |");
            }
            return builder.ToString();
        }

        private static string BuildSpecCsv()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("key,value,evidence");
            AppendSpec(builder, "classification", "COMPLEX_GUARDED_ONCE",
                AssignmentHash);
            AppendSpec(builder, "itemSchema", fixture.Request.schemaId,
                fixture.Request.canonicalSignature);
            AppendSpec(builder, "enemySchema",
                ShougunuPhase1RuntimeContract.SchemaId,
                nominal.finalContext.enemy.CanonicalSignature);
            AppendSpec(builder, "pulseTicks", "1500",
                "Battle-owned deterministic clock");
            AppendSpec(builder, "acceptedApplications",
                nominal.Rows.Count.ToString(CultureInfo.InvariantCulture),
                "Nominal trace");
            AppendSpec(builder, "finalTick",
                nominal.finalContext.battleTick.ToString(
                    CultureInfo.InvariantCulture),
                "Nominal trace");
            AppendSpec(builder, "playerFixture",
                "9999/9999;not_invulnerable",
                "BattleSandboxPlayerCombatantSnapshot.v1");
            foreach (ItemCombatEffectRequestRow row in fixture.Request.Requests
                .OrderBy(value => value.sourceBaseItemId, StringComparer.Ordinal))
            {
                AppendSpec(
                    builder,
                    "itemTruth." + row.sourceBaseItemId,
                    row.isLitFact + "/"
                        + row.sourceIsCountedFact + "/"
                        + row.qualifiedIsCountedFact,
                    row.requestId);
            }
            AppendSpec(builder, "sceneTouched", "false", "P2 data-only");
            AppendSpec(builder, "visualTouched", "false", "P2 data-only");
            AppendSpec(builder, "formalRouteTouched", "false", "devOnly");
            AppendSpec(builder, "saveTouched", "false", "static scan");
            AppendSpec(builder, "rewardTouched", "false", "static scan");
            AppendSpec(builder, "autoCombatTouched", "false", "static scan");
            return builder.ToString();
        }

        private static void AppendSpec(
            StringBuilder builder,
            string key,
            string value,
            string evidence)
        {
            builder.Append(Csv(key)).Append(',')
                .Append(Csv(value)).Append(',')
                .Append(Csv(evidence)).AppendLine();
        }

        private static string BuildNominalCsv()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "applicationSequence,battleTick,sourceBaseItemId,"
                + "sourceItemInstanceId,requestId,"
                + "resolvedPreMitigationDamageUnits,shellDamageApplied,"
                + "hpDamageApplied,enemyLifecycle,enemyCurrentHp,"
                + "enemyCurrentShell,basicEarned,basicResolved,basicDebt,"
                + "thresholdOccurrences,activeActionId,activeActionStatus,"
                + "playerCurrentHp,displayEventCount,accepted,reason");
            foreach (ShougunuPhase1BattleApplicationTraceRow row
                in nominal.Rows)
            {
                builder.Append(row.applicationSequence).Append(',')
                    .Append(row.battleTick).Append(',')
                    .Append(Csv(row.sourceBaseItemId)).Append(',')
                    .Append(Csv(row.sourceItemInstanceId)).Append(',')
                    .Append(Csv(row.requestId)).Append(',')
                    .Append(row.resolvedPreMitigationDamageUnits).Append(',')
                    .Append(row.shellDamageApplied).Append(',')
                    .Append(row.hpDamageApplied).Append(',')
                    .Append(Csv(row.enemyLifecycle)).Append(',')
                    .Append(row.enemyCurrentHp).Append(',')
                    .Append(row.enemyCurrentShell).Append(',')
                    .Append(row.basicEarned).Append(',')
                    .Append(row.basicResolved).Append(',')
                    .Append(row.basicDebt).Append(',')
                    .Append(Csv(row.thresholdOccurrences)).Append(',')
                    .Append(Csv(row.activeActionId)).Append(',')
                    .Append(Csv(row.activeActionStatus)).Append(',')
                    .Append(row.playerCurrentHp).Append(',')
                    .Append(row.displayEventCount).Append(',')
                    .Append(row.accepted ? "true" : "false").Append(',')
                    .Append(Csv(row.reason)).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildPlayerCsv()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "battleTick,enemyEffectId,actionPatternId,requestedDamage,"
                + "shieldDamageApplied,hpDamageApplied,beforeHp,afterHp,"
                + "defeated,decision");
            foreach (BattleSandboxPlayerEffectApplicationResult row
                in nominal.PlayerResults)
            {
                builder.Append(row.battleTick).Append(',')
                    .Append(Csv(row.enemyEffectId)).Append(',')
                    .Append(Csv(row.actionPatternId)).Append(',')
                    .Append(row.requestedDamage).Append(',')
                    .Append(row.shieldDamageApplied).Append(',')
                    .Append(row.hpDamageApplied).Append(',')
                    .Append(row.beforeHp).Append(',')
                    .Append(row.afterHp).Append(',')
                    .Append(row.defeated ? "true" : "false").Append(',')
                    .Append(Csv(row.decision.ToString())).AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildDisplayCsv()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "displayEventId,ledgerEventId,battleTick,resetGeneration,"
                + "sequence,sourceKind,sourceId,targetActorId,channel,amount,"
                + "messageKey,priority,aggregationKey,anchorRoute,colorPolicy,"
                + "playerVisible,developerOnly");
            foreach (ShougunuPhase1BattleDisplayEvent row
                in nominal.finalContext.DisplayEvents)
            {
                builder.Append(Csv(row.displayEventId)).Append(',')
                    .Append(Csv(row.ledgerEventId)).Append(',')
                    .Append(row.battleTick).Append(',')
                    .Append(row.resetGeneration).Append(',')
                    .Append(row.sequence).Append(',')
                    .Append(Csv(row.sourceKind.ToString())).Append(',')
                    .Append(Csv(row.sourceId)).Append(',')
                    .Append(Csv(row.targetActorId)).Append(',')
                    .Append(Csv(row.channel.ToString())).Append(',')
                    .Append(row.amount).Append(',')
                    .Append(Csv(row.messageKey)).Append(',')
                    .Append(row.priority).Append(',')
                    .Append(Csv(row.aggregationKey)).Append(',')
                    .Append(Csv(row.anchorRoute.ToString())).Append(',')
                    .Append(Csv(row.colorPolicy)).Append(',')
                    .Append(row.playerVisible ? "true" : "false").Append(',')
                    .Append(row.developerOnly ? "true" : "false")
                    .AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildNegativeCsv()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                "fixture,decision,reason,damageApplied,"
                + "acceptedDamageApplicationCount,status");
            foreach (NegativeResult row in Negatives)
            {
                builder.Append(Csv(row.Name)).Append(',')
                    .Append(Csv(row.Decision)).Append(',')
                    .Append(Csv(row.Reason)).Append(',')
                    .Append(row.DamageApplied).Append(',')
                    .Append(row.AcceptedCount).Append(',')
                    .Append(row.Passed ? "PASS" : "FAIL").AppendLine();
            }
            return builder.ToString();
        }

        private static string BuildLeakReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(
                    "# Shougunu Phase1 Battle Application Leak Check")
                .AppendLine()
                .AppendLine("- static forbidden reference scan = `"
                    + ResultStatus("S10") + "`")
                .AppendLine("- package-scoped text check = `"
                    + ResultStatus("S12") + "`")
                .AppendLine("- sceneTouched = `false`")
                .AppendLine("- visualTouched = `false`")
                .AppendLine("- formalRouteTouched = `false`")
                .AppendLine("- saveTouched = `false`")
                .AppendLine("- rewardTouched = `false`")
                .AppendLine("- autoCombatTouched = `false`")
                .AppendLine("- Scene/Prefab binding = `none`")
                .AppendLine("- reset old-event leakage = `none`")
                .AppendLine("- upstream Item/Enemy mutation = `none`")
                .AppendLine("- Git command = `NOT_EXECUTED_BY_USER_DIRECTIVE`")
                .AppendLine(
                    "- equivalent scoped whitespace/conflict-marker audit = `"
                    + ResultStatus("S12") + "`");
            return builder.ToString();
        }

        private static string ResultStatus(string id)
        {
            ScenarioResult result = Results.FirstOrDefault(value =>
                value.Id == id);
            return result != null && result.Passed ? "PASS" : "FAIL";
        }

        private static void AssertReadOnly<T>(
            IReadOnlyList<T> values,
            string label)
        {
            bool blocked = false;
            try
            {
                ((ICollection<T>)values).Add(default);
            }
            catch (NotSupportedException)
            {
                blocked = true;
            }
            Check(blocked, label + " permits external mutation.");
        }

        private static void Run(string id, string marker, Action action)
        {
            try
            {
                action();
                Results.Add(new ScenarioResult(
                    id, marker, true, "verified"));
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(
                    id,
                    marker,
                    false,
                    exception.GetType().Name + ": " + exception.Message));
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string ProjectRoot =>
            Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException("Project root unavailable.");

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                ProjectRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string Read(string relativePath)
        {
            return File.ReadAllText(ProjectPath(relativePath));
        }

        private static void Write(string relativePath, string text)
        {
            File.WriteAllText(
                ProjectPath(relativePath),
                text,
                new UTF8Encoding(false));
        }

        private static string HashFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string HashText(string text)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    new UTF8Encoding(false).GetBytes(text ?? string.Empty)))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty)
                .Replace("\"", "\"\"")
                .Replace("\r", " ")
                .Replace("\n", " ") + "\"";
        }

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private sealed class Fixture
        {
            public Fixture(ItemCombatEffectRequestSnapshot request)
            {
                Request = request;
            }

            public ItemCombatEffectRequestSnapshot Request { get; }
        }

        private sealed class ScenarioResult
        {
            public ScenarioResult(
                string id,
                string marker,
                bool passed,
                string detail)
            {
                Id = id;
                Marker = marker;
                Passed = passed;
                Detail = detail;
            }

            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }

        private sealed class NegativeResult
        {
            public NegativeResult(
                string name,
                string decision,
                string reason,
                int damageApplied,
                int acceptedCount,
                bool passed)
            {
                Name = name;
                Decision = decision;
                Reason = reason;
                DamageApplied = damageApplied;
                AcceptedCount = acceptedCount;
                Passed = passed;
            }

            public string Name { get; }
            public string Decision { get; }
            public string Reason { get; }
            public int DamageApplied { get; }
            public int AcceptedCount { get; }
            public bool Passed { get; }
        }
    }
}
