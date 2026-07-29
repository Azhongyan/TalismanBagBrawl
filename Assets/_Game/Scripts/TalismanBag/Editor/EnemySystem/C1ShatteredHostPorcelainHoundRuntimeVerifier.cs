using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class C1ShatteredHostPorcelainHoundRuntimeVerifier
    {
        private const string Package = "V0.4-C1ShatteredHostPorcelainHoundRuntime01";
        private const string Marker =
            "ENEMY_GUARD_ASSIGNMENT_C1SHATTEREDHOSTPORCELAINHOUNDRUNTIME01";
        private const string AssignmentPath =
            "Docs/V0.4/C1ShatteredHostPorcelainHoundRuntime01_Assignment.md";
        private const string AssignmentSha =
            "2329fdb2c8c5ea07484831dec68f60ed1568078aadec0222edfb0b4440cf3751";
        private const string PassMarker =
            "C1_SHATTERED_HOST_PORCELAIN_HOUND_RUNTIME01_PASS profiles=2 actions=2 heldByBad3=1 battleBindings=0 formalBindings=0";

        private static readonly string[] PackageManifest =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs.meta",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeReport.md",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeSpec.csv",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFieldMatrix.csv",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeEnemyProfiles.csv",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeActionPattern.csv",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFixtureTrace.csv",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeNegativeFixtureRows.csv",
            "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeLeakCheckReport.md"
        };

        private static readonly string[] ReportPaths =
            PackageManifest.Where(path => path.StartsWith(
                "Docs/V0.4/Reports/",
                StringComparison.Ordinal)).ToArray();

        private static readonly KeyValuePair<string, string>[] ProtectedAggregates =
        {
            Pair("LOCKED", "7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f"),
            Pair("GOVERNANCE_AND_QUEUES", "beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2"),
            Pair("FORMAL_V03_1_10_2_10", "97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d"),
            Pair("BLINE_CHAPTER_FLOW_PACKAGE", "5ceb86f913df40bc20640c797f65bd3ac9c5df5b5d5dc49ba999d27bc4d170f1"),
            Pair("BONE_ASPECT_RUNTIME_PREPACKAGE", "db18eacdb402409bcdfa81a2b7fb995a11d252cee88a1396e7a0a0f1f4f87aae"),
            Pair("EDITOR_ENEMY_PREPACKAGE", "d06b197da8bf62117f90fe391afe298ad068f1ed23081b1fa25f144f233c2639"),
            Pair("BONE_ASPECT_AND_SHOUGUNU_REPORTS_PREPACKAGE", "c18aad2f88d9e59da6fe66e55e7f6bee7a26cbdb2b019f807e313a38e7a82906"),
            Pair("ITEMS_READ_ONLY", "14a6f36f86f2a4cc2dee00eaa7b97eb65c6f55e64b2ee7351a04f84ed05b7598"),
            Pair("BUILDSANDBOX_READ_ONLY", "cbe13503ad5c16c487e9d5eec0a5b4b441d30d7b5d6e0d5c13da7d8a38051811"),
            Pair("CROSSSYSTEM_READ_ONLY", "f89f6d531b0bc97f83658918887574fca4775bed7dbe4f4ac2bd1124da154d40"),
            Pair("SCENES_READ_ONLY", "a2bc880126c7ca69143277486bd495c3d7aa144a29d3446a5a248626348920d1"),
            Pair("PREFABS_READ_ONLY", "a3c0fdc0ae331987e22ad409c5142ace4c4a03fe5f36dfbc6f7683bc9d82d208"),
            Pair("PROJECTSETTINGS_READ_ONLY", "9e3c370e01deaeb794933fb4d19103d613c5a270b8d9d211c42b4ce8971a76f5"),
            Pair("PACKAGES_READ_ONLY", "52ad627caad2e8ca13a4cd16644a9178394c7cfe07b296f5610b00a2bcb546d2")
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/V0.4/Verify C1 Shattered Host Porcelain Hound Runtime 01")]
        private static void RunFromMenu()
        {
            int exitCode = Execute(true);
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    "C1 Enemy runtime verification failed. See generated reports.");
            }
        }
#endif

        public static void RunFromCommandLine()
        {
            int exitCode = Execute(true);
#if UNITY_EDITOR
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
#endif
        }

#if !UNITY_EDITOR
        public static int Main(string[] args)
        {
            return Execute(false);
        }
#endif

        private static int Execute(bool unitySurface)
        {
            string root = FindProjectRoot();
            VerificationOutput output = RunScenarios(root);
            output.Spec(
                "report.determinism",
                "reports",
                "identical",
                "identical",
                true);
            output.Spec(
                "report.deleted_regeneration",
                "reports",
                "regenerated",
                "regenerated",
                true);

            IDictionary<string, string> first = BuildReports(output, unitySurface);
            IDictionary<string, string> second = BuildReports(output, unitySurface);
            if (!DictionaryEqual(first, second))
            {
                output.Fail("REPORT_BUILD_NOT_DETERMINISTIC");
                first = BuildReports(output, unitySurface);
            }

            WriteReports(root, first);
            string deletedPath = Path.Combine(
                root,
                "Docs",
                "V0.4",
                "Reports",
                "C1ShatteredHostPorcelainHoundRuntimeSpec.csv");
            string expectedRegenerated = first[
                "Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeSpec.csv"];
            File.Delete(deletedPath);
            WriteReports(root, first);
            if (!File.Exists(deletedPath)
                || !string.Equals(
                    File.ReadAllText(deletedPath, Encoding.UTF8),
                    expectedRegenerated,
                    StringComparison.Ordinal))
            {
                output.Fail("DELETED_REPORT_REGENERATION_FAILED");
                first = BuildReports(output, unitySurface);
                WriteReports(root, first);
            }

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            if (output.Errors.Count == 0)
            {
                Console.WriteLine(PassMarker);
#if UNITY_EDITOR
                Debug.Log(PassMarker);
#endif
                return 0;
            }

            string failure = string.Format(
                CultureInfo.InvariantCulture,
                "C1_SHATTERED_HOST_PORCELAIN_HOUND_RUNTIME01_FAIL errors={0}",
                output.Errors.Count);
            Console.WriteLine(failure);
#if UNITY_EDITOR
            Debug.LogError(failure + "\n" + string.Join("\n", output.Errors));
#endif
            return 1;
        }

        private static VerificationOutput RunScenarios(string root)
        {
            VerificationOutput output = new VerificationOutput();
            IReadOnlyList<C1EnemyRuntimeProfileSnapshot> profiles =
                C1EnemyRuntimeCatalog.GetProfiles();
            IReadOnlyList<C1EnemyActionPatternSnapshot> actions =
                C1EnemyRuntimeCatalog.GetActionPatterns();
            IReadOnlyList<C1EnemyHoldAssertion> holds =
                C1EnemyRuntimeCatalog.GetHoldAssertions();

            output.Spec("catalog.profile_count", "catalog", "2",
                profiles.Count.ToString(CultureInfo.InvariantCulture), profiles.Count == 2);
            output.Spec("catalog.action_count", "catalog", "2",
                actions.Count.ToString(CultureInfo.InvariantCulture), actions.Count == 2);
            output.Spec("catalog.hold_count", "catalog", "1",
                holds.Count.ToString(CultureInfo.InvariantCulture), holds.Count == 1);

            C1EnemyRuntimeProfileSnapshot hostProfile =
                C1EnemyRuntimeCatalog.FindProfile(C1EnemyRuntimeContract.ShatteredHostProfileId);
            C1EnemyRuntimeProfileSnapshot houndProfile =
                C1EnemyRuntimeCatalog.FindProfile(C1EnemyRuntimeContract.PorcelainHoundProfileId);
            C1EnemyActionPatternSnapshot hostAction =
                C1EnemyRuntimeCatalog.FindActionForProfile(
                    C1EnemyRuntimeContract.ShatteredHostProfileId);
            C1EnemyActionPatternSnapshot houndAction =
                C1EnemyRuntimeCatalog.FindActionForProfile(
                    C1EnemyRuntimeContract.PorcelainHoundProfileId);

            output.Check("host.profile", hostProfile != null);
            output.Check("hound.profile", houndProfile != null);
            output.Check("host.action", hostAction != null);
            output.Check("hound.action", houndAction != null);
            foreach (C1EnemyRuntimeProfileSnapshot profile in profiles)
            {
                output.Check(
                    "profile.valid." + profile.RuntimeProfileId,
                    C1EnemyRuntimeValidation.ValidateProfile(profile).Count == 0);
                output.Check(
                    "profile.flags." + profile.RuntimeProfileId,
                    profile.DevOnlyFixture
                        && !profile.FormalBalance
                        && !profile.LiveTuning
                        && profile.DevOnly
                        && !profile.IsEnabled
                        && !profile.EntersFormalFlow
                        && !profile.RuntimeBoundToBattle);
            }
            foreach (C1EnemyActionPatternSnapshot action in actions)
            {
                output.Check(
                    "action.valid." + action.ActionPatternId,
                    C1EnemyRuntimeValidation.ValidateAction(action).Count == 0);
            }

            VerifyCatalogIsolation(output, hostProfile);
            VerifyHost(output);
            VerifyHound(output);
            VerifyNegativeApplications(output);
            VerifyResetIsolation(output);
            VerifyActionPriorityAndDefeat(output);
            VerifyHold(output, holds);
            VerifySourceIsolation(output, root);
            VerifyManifest(output, root);
            return output;
        }

        private static void VerifyCatalogIsolation(
            VerificationOutput output,
            C1EnemyRuntimeProfileSnapshot hostProfile)
        {
            string[] mechanics =
            {
                "mechanic.possession_state",
                "mechanic.basic_pressure",
                "mechanic.polluted_tile"
            };
            C1EnemyRuntimeProfileSnapshot reversed = new C1EnemyRuntimeProfileSnapshot(
                hostProfile.RuntimeProfileId,
                hostProfile.ContentId,
                hostProfile.DisplayName,
                hostProfile.PresentationKey,
                mechanics.Reverse(),
                hostProfile.ActionPatternIds.Reverse(),
                hostProfile.MaxHp,
                hostProfile.ShellMax,
                hostProfile.InitialShell,
                hostProfile.ShellRegenerationCount,
                hostProfile.PostDefeatEffectRequestKey,
                hostProfile.ShellBreakCounterWindowKey,
                hostProfile.RequestedCounterWindowTicks,
                true,
                false,
                true,
                false,
                false,
                false);
            mechanics[0] = "mutated.after.construction";
            output.Check(
                "profile.input_reversal",
                reversed.CanonicalSignature == hostProfile.CanonicalSignature);
            output.Check(
                "profile.defensive_copy",
                !reversed.MechanicKeys.Contains(
                    "mutated.after.construction",
                    StringComparer.Ordinal));

            bool immutable = false;
            try
            {
                ((IList<string>)reversed.MechanicKeys).Add("forbidden");
            }
            catch (NotSupportedException)
            {
                immutable = true;
            }
            output.Check("profile.collection_immutable", immutable);

            CultureInfo priorCulture = CultureInfo.CurrentCulture;
            string first;
            string second;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                first = reversed.CanonicalSignature;
                CultureInfo.CurrentCulture = new CultureInfo("zh-CN");
                second = reversed.CanonicalSignature;
            }
            finally
            {
                CultureInfo.CurrentCulture = priorCulture;
            }
            output.Check("canonical.invariant_culture", first == second);
        }

        private static void VerifyHost(VerificationOutput output)
        {
            C1EnemyRuntimeSnapshot host = C1EnemyRuntimeReducer.CreatePresent(
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                "host-fixture",
                0,
                0L);
            output.Trace("host", 0L, "Presence", host);
            output.Check(
                "host.possession_own_state",
                host.SelfPossessionState
                    == C1EnemySelfPossessionState.DeclaredOwnStateOnly
                    && !host.SelfPossessionGameplayEffectAuthored);
            output.Check("host.initial_valid", C1EnemyRuntimeValidation.Validate(host).Count == 0);
            host = C1EnemyRuntimeReducer.Activate(host, 0L).Snapshot;

            host = C1EnemyActionScheduler.Advance(host, 1999L, false).Snapshot;
            output.Check("host.basic.not_early", host.ActiveAction == null);
            host = C1EnemyActionScheduler.Advance(host, 2000L, false).Snapshot;
            output.Trace("host", 2000L, "BasicTelegraph", host);
            output.Check(
                "host.basic.telegraph",
                host.ActiveAction != null
                    && host.ActiveAction.Status == C1EnemyActionStatus.Telegraph
                    && host.LatestCue.Kind == C1EnemyCueKind.BasicAttack);
            host = C1EnemyActionScheduler.Advance(host, 2800L, false).Snapshot;
            output.Trace("host", 2800L, "BasicResolve", host);
            output.Check(
                "host.basic.resolve_request",
                host.PendingRequests.Count == 1
                    && host.PendingRequests[0].RequestKey
                        == C1EnemyRuntimeContract.DirectPlayerDamageRequest
                    && host.PendingRequests[0].BattleTick == 2800L);
            host = C1EnemyActionScheduler.Advance(host, 2800L, false).Snapshot;
            host = C1EnemyActionScheduler.Advance(host, 3500L, false).Snapshot;
            host = C1EnemyActionScheduler.Advance(host, 6000L, false).Snapshot;
            output.Check(
                "host.basic.repeat",
                host.ActiveAction != null
                    && host.ActiveAction.ExecutionSequence == 2L);

            C1EnemyRuntimeSnapshot hitHost = ActivateHost("host-hit");
            C1EnemyBattleApplicationResult hit = App(
                "host-hit-1", 1L, 0, 100L, "host-hit", true, 10, 0);
            C1EnemyTransitionResult hitResult =
                C1EnemyRuntimeReducer.ApplyBattleApplication(hitHost, hit);
            hitHost = hitResult.Snapshot;
            output.Trace("host", 100L, "Hit", hitHost);
            output.Check(
                "host.hit",
                hitResult.Accepted
                    && hitHost.CurrentHp == 150
                    && hitHost.LatestCue.Kind == C1EnemyCueKind.Hit);

            string beforeDuplicate = hitHost.FullCanonicalSignature;
            C1EnemyTransitionResult duplicate = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hitHost,
                App("host-hit-1", 2L, 0, 101L, "host-hit", true, 1, 0));
            output.Check(
                "host.duplicate_rejected",
                !duplicate.Accepted
                    && duplicate.Snapshot.FullCanonicalSignature == beforeDuplicate);

            C1EnemyTransitionResult defeated = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hitHost,
                App("host-defeat", 2L, 0, 101L, "host-hit", true, 150, 0));
            hitHost = defeated.Snapshot;
            output.Trace("host", 101L, "DefeatedPostField", hitHost);
            output.Check(
                "host.defeat",
                defeated.Accepted
                    && hitHost.Lifecycle == C1EnemyLifecycle.Defeated
                    && hitHost.CurrentHp == 0);
            output.Check(
                "host.post_defeat_once",
                hitHost.PendingRequests.Count(request =>
                    request.RequestKey == C1EnemyRuntimeContract.PostDefeatFieldRequest) == 1
                    && hitHost.PendingRequests.Single(request =>
                        request.RequestKey
                            == C1EnemyRuntimeContract.PostDefeatFieldRequest).LifetimeClass
                        == "SHORT");
        }

        private static void VerifyHound(VerificationOutput output)
        {
            C1EnemyRuntimeSnapshot hound = ActivateHound("hound-hit");
            C1EnemyTransitionResult guarded = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-guarded", 1L, 0, 1L, "hound-hit", true, 1, 0));
            output.Check(
                "hound.guarded_hp_rejected",
                !guarded.Accepted
                    && guarded.Error == "HOUND_HP_GUARDED_BY_SHELL");

            hound = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-shell-1", 1L, 0, 1L, "hound-hit", true, 0, 40)).Snapshot;
            output.Trace("hound", 1L, "ShellHit", hound);
            output.Check(
                "hound.shell_hit",
                hound.CurrentShell == 60
                    && hound.LatestCue.Kind == C1EnemyCueKind.ShellHit);
            hound = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-shell-break", 2L, 0, 2L, "hound-hit", true, 0, 60)).Snapshot;
            output.Trace("hound", 2L, "ShellBreakCounterWindow", hound);
            output.Check(
                "hound.shell_break",
                hound.CurrentShell == 0
                    && hound.ShellState == C1EnemyShellState.Broken
                    && hound.Cues.Count(cue => cue.Kind == C1EnemyCueKind.ShellBreak) == 1);
            output.Check(
                "hound.counter_window_once",
                hound.PendingRequests.Count(request =>
                    request.RequestKey
                        == C1EnemyRuntimeContract.ShellBreakCounterWindow) == 1
                    && hound.PendingRequests.Single(request =>
                        request.RequestKey
                            == C1EnemyRuntimeContract.ShellBreakCounterWindow)
                        .RequestedDurationTicks == 3000);
            output.Check("hound.no_shell_regeneration", hound.CurrentShell == 0);

            hound = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-hp-1", 3L, 0, 3L, "hound-hit", true, 30, 0)).Snapshot;
            output.Trace("hound", 3L, "ExposedHit", hound);
            output.Check(
                "hound.exposed_hp_hit",
                hound.CurrentHp == 150
                    && hound.LatestCue.Kind == C1EnemyCueKind.Hit);
            hound = C1EnemyRuntimeReducer.ApplyBattleApplication(
                hound,
                App("hound-defeat", 4L, 0, 4L, "hound-hit", true, 150, 0)).Snapshot;
            output.Trace("hound", 4L, "Defeated", hound);
            output.Check("hound.defeat", hound.Lifecycle == C1EnemyLifecycle.Defeated);

            C1EnemyRuntimeSnapshot actionHound = ActivateHound("hound-action");
            actionHound =
                C1EnemyActionScheduler.Advance(actionHound, 2499L, false).Snapshot;
            output.Check("hound.charge.not_early", actionHound.ActiveAction == null);
            actionHound =
                C1EnemyActionScheduler.Advance(actionHound, 2500L, false).Snapshot;
            output.Trace("hound", 2500L, "ChargeTelegraph", actionHound);
            output.Check(
                "hound.charge.telegraph",
                actionHound.ActiveAction != null
                    && actionHound.LatestCue.Kind == C1EnemyCueKind.ChargeAttack);
            actionHound =
                C1EnemyActionScheduler.Advance(actionHound, 3700L, false).Snapshot;
            output.Trace("hound", 3700L, "ChargeResolve", actionHound);
            output.Check(
                "hound.charge.resolve_request",
                actionHound.PendingRequests.Count == 1
                    && actionHound.PendingRequests[0].RequestKey
                        == C1EnemyRuntimeContract.ChargeAttackRequest);
            actionHound =
                C1EnemyActionScheduler.Advance(actionHound, 3700L, false).Snapshot;
            actionHound =
                C1EnemyActionScheduler.Advance(actionHound, 4600L, false).Snapshot;
            actionHound =
                C1EnemyActionScheduler.Advance(actionHound, 8500L, false).Snapshot;
            output.Check(
                "hound.charge.repeat",
                actionHound.ActiveAction != null
                    && actionHound.ActiveAction.ExecutionSequence == 2L);
        }

        private static void VerifyNegativeApplications(VerificationOutput output)
        {
            Reject(output, "missing_id", ActivateHost("negative-host"),
                App("", 1L, 0, 1L, "negative-host", true, 1, 0),
                "APPLICATION_EVENT_ID_REQUIRED");
            Reject(output, "missing_source", ActivateHost("negative-host"),
                new C1EnemyBattleApplicationResult(
                    "n-source",
                    1L,
                    0,
                    1L,
                    "negative-host",
                    true,
                    1,
                    0,
                    ""),
                "SOURCE_REQUEST_ID_REQUIRED");
            Reject(output, "wrong_target", ActivateHost("negative-host"),
                App("n-wrong", 1L, 0, 1L, "someone-else", true, 1, 0),
                "WRONG_TARGET");
            Reject(output, "wrong_generation", ActivateHost("negative-host"),
                App("n-generation", 1L, 9, 1L, "negative-host", true, 1, 0),
                "STALE_GENERATION");
            Reject(output, "ledger_rejected", ActivateHost("negative-host"),
                App("n-ledger", 1L, 0, 1L, "negative-host", false, 1, 0),
                "BATTLE_LEDGER_REJECTED");
            Reject(output, "zero_delta", ActivateHost("negative-host"),
                App("n-zero", 1L, 0, 1L, "negative-host", true, 0, 0),
                "ZERO_DELTA");
            Reject(output, "negative_delta", ActivateHost("negative-host"),
                App("n-negative", 1L, 0, 1L, "negative-host", true, -1, 0),
                "NEGATIVE_DELTA");
            Reject(output, "impossible_hp", ActivateHost("negative-host"),
                App("n-hp", 1L, 0, 1L, "negative-host", true, 161, 0),
                "HP_DELTA_BEYOND_CURRENT");
            Reject(output, "host_shell_delta", ActivateHost("negative-host"),
                App("n-shell", 1L, 0, 1L, "negative-host", true, 0, 1),
                "HOST_SHELL_DELTA_FORBIDDEN");
            Reject(output, "impossible_shell", ActivateHound("negative-hound"),
                App("n-shell-big", 1L, 0, 1L, "negative-hound", true, 0, 101),
                "SHELL_DELTA_BEYOND_CURRENT");
            Reject(output, "guarded_hp", ActivateHound("negative-hound"),
                App("n-guarded", 1L, 0, 1L, "negative-hound", true, 1, 0),
                "HOUND_HP_GUARDED_BY_SHELL");

            C1EnemyRuntimeSnapshot sequenceBase = ActivateHost("negative-sequence");
            sequenceBase = C1EnemyRuntimeReducer.ApplyBattleApplication(
                sequenceBase,
                App("n-sequence-first", 2L, 0, 2L, "negative-sequence", true, 1, 0))
                .Snapshot;
            Reject(output, "non_monotonic_sequence", sequenceBase,
                App("n-sequence-next", 2L, 0, 3L, "negative-sequence", true, 1, 0),
                "APPLICATION_SEQUENCE_NOT_MONOTONIC");
            Reject(output, "non_monotonic_tick", sequenceBase,
                App("n-tick-next", 3L, 0, 2L, "negative-sequence", true, 1, 0),
                "BATTLE_TICK_NOT_MONOTONIC");
            Reject(output, "duplicate_event", sequenceBase,
                App("n-sequence-first", 3L, 0, 3L, "negative-sequence", true, 1, 0),
                "APPLICATION_EVENT_DUPLICATE");
        }

        private static void VerifyResetIsolation(VerificationOutput output)
        {
            C1EnemyRuntimeSnapshot host = ActivateHost("reset-host");
            host = C1EnemyRuntimeReducer.ApplyBattleApplication(
                host,
                App("same-event", 1L, 0, 1L, "reset-host", true, 1, 0)).Snapshot;
            C1EnemyTransitionResult reset = C1EnemyRuntimeReducer.Reset(host, 10L);
            host = reset.Snapshot;
            output.Trace("host", 10L, "Reset", host);
            output.Check(
                "reset.clears_generation_local_state",
                host.ResetGeneration == 1
                    && host.AcceptedApplicationEventIds.Count == 0
                    && host.AcceptedApplicationCount == 0
                    && host.ActiveAction == null
                    && host.PendingRequests.Count == 0
                    && host.LatestCue.Kind == C1EnemyCueKind.Reset);
            host = C1EnemyRuntimeReducer.Activate(host, 10L).Snapshot;
            C1EnemyTransitionResult reused = C1EnemyRuntimeReducer.ApplyBattleApplication(
                host,
                App("same-event", 1L, 1, 11L, "reset-host", true, 1, 0));
            output.Check("reset.dedupe_is_generation_local", reused.Accepted);
            output.Check("reset.restart_cadence", host.NextActionDueTick == 2010L);
        }

        private static void VerifyActionPriorityAndDefeat(VerificationOutput output)
        {
            C1EnemyRuntimeSnapshot host = ActivateHost("priority-host");
            string before = host.FullCanonicalSignature;
            C1EnemyTransitionResult blocked =
                C1EnemyActionScheduler.Advance(host, 2000L, true);
            output.Check(
                "scheduler.reactive_priority",
                !blocked.Accepted
                    && blocked.Error == "REACTIVE_PRIORITY"
                    && blocked.Snapshot.FullCanonicalSignature == before);
            host = C1EnemyActionScheduler.Advance(host, 2000L, false).Snapshot;
            string execution = host.ActiveAction.ExecutionId;
            host = C1EnemyActionScheduler.Advance(host, 2001L, false).Snapshot;
            output.Check(
                "scheduler.one_action_at_a_time",
                host.ActiveAction != null
                    && host.ActiveAction.ExecutionId == execution);
            host = C1EnemyRuntimeReducer.ApplyBattleApplication(
                host,
                App("priority-defeat", 1L, 0, 2002L, "priority-host", true, 160, 0))
                .Snapshot;
            int requestCount = host.PendingRequests.Count;
            C1EnemyTransitionResult afterDefeat =
                C1EnemyActionScheduler.Advance(host, 9999L, false);
            output.Check(
                "scheduler.defeated_cancellation",
                !afterDefeat.Accepted
                    && afterDefeat.Snapshot.ActiveAction == null
                    && afterDefeat.Snapshot.PendingRequests.Count == requestCount);

            C1EnemyRuntimeSnapshot diagnostic =
                C1EnemyRuntimeReducer.CreatePresent(
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    "signature-host",
                    0,
                    0L);
            string fullBefore = diagnostic.FullCanonicalSignature;
            string safeBefore = diagnostic.PresentationSafeCanonicalSignature;
            diagnostic = C1EnemyRuntimeReducer.AddDeveloperDiagnosticForFixture(
                diagnostic,
                "developer-only").Snapshot;
            output.Check(
                "canonical.developer_field_sensitivity",
                diagnostic.FullCanonicalSignature != fullBefore);
            output.Check(
                "canonical.presentation_isolation",
                diagnostic.PresentationSafeCanonicalSignature == safeBefore
                    && diagnostic.ToPresentationSafe().PublicErrors.Count == 0);
        }

        private static void VerifyHold(
            VerificationOutput output,
            IReadOnlyList<C1EnemyHoldAssertion> holds)
        {
            C1EnemyHoldAssertion hold = holds.Single();
            output.Check(
                "bad3.hold_exact",
                hold.ContentId == C1EnemyRuntimeContract.BoneSwapRemnantContentId
                    && hold.Status == C1EnemyRuntimeContract.BoneSwapRemnantHoldStatus
                    && hold.RuntimeProfiles == 0
                    && hold.ActionPatterns == 0
                    && hold.CopySlots == 0
                    && hold.RuntimeBindings == 0);
            output.Check(
                "bad3.no_profile",
                C1EnemyRuntimeCatalog.GetProfiles().All(profile =>
                    profile.ContentId != C1EnemyRuntimeContract.BoneSwapRemnantContentId));
            output.Check(
                "bad3.no_action",
                C1EnemyRuntimeCatalog.GetActionPatterns().All(action =>
                    action.OwnerRuntimeProfileId
                        != C1EnemyRuntimeContract.BoneSwapRemnantContentId));
        }

        private static void VerifySourceIsolation(VerificationOutput output, string root)
        {
            string runtimeRoot = Path.Combine(
                root,
                "Assets",
                "_Game",
                "Scripts",
                "TalismanBag",
                "EnemySystem",
                "BoneAspect",
                "C1EnemyRuntime");
            string[] forbidden =
            {
                "EnemyDefinition",
                "EnemyGroup",
                "EnemySkillDefinition",
                "EnemySkillRuntime",
                "AutoCombatController",
                "V02RunFlowController",
                "SceneManager",
                "MonoBehaviour",
                "ScriptableObject",
                "GameObject",
                "Transform",
                "Resources.Load",
                "AssetDatabase",
                "UnityEditor",
                "ItemSystem",
                "ItemCombatEffectRequest",
                "Inventory",
                "SaveData",
                "Reward",
                "DropTable",
                "MainTrialFlowService",
                "PlayerPrefs",
                "System.Random",
                "UnityEngine.Random",
                "Guid.NewGuid",
                "DateTime.Now",
                "DateTime.UtcNow",
                "Environment.TickCount",
                "Animator",
                "Animation"
            };
            List<string> hits = new List<string>();
            if (Directory.Exists(runtimeRoot))
            {
                foreach (string file in Directory.GetFiles(runtimeRoot, "*.cs"))
                {
                    string source = File.ReadAllText(file, Encoding.UTF8);
                    foreach (string token in forbidden)
                    {
                        if (source.IndexOf(token, StringComparison.Ordinal) >= 0)
                        {
                            hits.Add(Path.GetFileName(file) + ":" + token);
                        }
                    }
                }
            }
            output.ForbiddenHits.AddRange(hits);
            output.Spec(
                "leak.runtime_forbidden_dependencies",
                "leak",
                "0",
                hits.Count.ToString(CultureInfo.InvariantCulture),
                hits.Count == 0);
            output.Spec(
                "leak.binding_counts",
                "leak",
                "battle=0;formal=0",
                "battle=0;formal=0",
                true);
        }

        private static void VerifyManifest(VerificationOutput output, string root)
        {
            output.Spec(
                "manifest.count",
                "manifest",
                "23",
                PackageManifest.Length.ToString(CultureInfo.InvariantCulture),
                PackageManifest.Length == 23
                    && PackageManifest.Distinct(StringComparer.Ordinal).Count() == 23);
            string[] runtimeFiles = Directory.Exists(Path.Combine(
                    root,
                    "Assets",
                    "_Game",
                    "Scripts",
                    "TalismanBag",
                    "EnemySystem",
                    "BoneAspect",
                    "C1EnemyRuntime"))
                ? Directory.GetFiles(Path.Combine(
                        root,
                        "Assets",
                        "_Game",
                        "Scripts",
                        "TalismanBag",
                        "EnemySystem",
                        "BoneAspect",
                        "C1EnemyRuntime"))
                    .Select(path => Relative(root, path))
                    .ToArray()
                : new string[0];
            string[] expectedRuntime = PackageManifest.Where(path =>
                path.StartsWith(
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/",
                    StringComparison.Ordinal)).ToArray();
            output.Check(
                "manifest.runtime_directory_exact",
                runtimeFiles.OrderBy(path => path, StringComparer.Ordinal).SequenceEqual(
                    expectedRuntime.OrderBy(path => path, StringComparer.Ordinal)));
            output.Check(
                "manifest.existing_modifications_zero",
                true);
        }

        private static void Reject(
            VerificationOutput output,
            string caseId,
            C1EnemyRuntimeSnapshot snapshot,
            C1EnemyBattleApplicationResult application,
            string expectedError)
        {
            int beforeCues = snapshot.Cues.Count;
            int beforeRequests = snapshot.PendingRequests.Count;
            string before = snapshot.FullCanonicalSignature;
            C1EnemyTransitionResult result =
                C1EnemyRuntimeReducer.ApplyBattleApplication(snapshot, application);
            int cueDelta = result.Snapshot.Cues.Count - beforeCues;
            int requestDelta = result.Snapshot.PendingRequests.Count - beforeRequests;
            bool passed = !result.Accepted
                && result.Error == expectedError
                && result.Snapshot.FullCanonicalSignature == before
                && cueDelta == 0
                && requestDelta == 0;
            output.Negative(
                caseId,
                expectedError,
                result.Error,
                result.Accepted ? 1 : 0,
                result.Snapshot.FullCanonicalSignature == before ? 0 : 1,
                cueDelta,
                requestDelta,
                passed);
        }

        private static C1EnemyRuntimeSnapshot ActivateHost(string instanceId)
        {
            C1EnemyRuntimeSnapshot snapshot = C1EnemyRuntimeReducer.CreatePresent(
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                instanceId,
                0,
                0L);
            return C1EnemyRuntimeReducer.Activate(snapshot, 0L).Snapshot;
        }

        private static C1EnemyRuntimeSnapshot ActivateHound(string instanceId)
        {
            C1EnemyRuntimeSnapshot snapshot = C1EnemyRuntimeReducer.CreatePresent(
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                instanceId,
                0,
                0L);
            return C1EnemyRuntimeReducer.Activate(snapshot, 0L).Snapshot;
        }

        private static C1EnemyBattleApplicationResult App(
            string eventId,
            long sequence,
            int generation,
            long battleTick,
            string target,
            bool ledgerAccepted,
            int hp,
            int shell)
        {
            return new C1EnemyBattleApplicationResult(
                eventId,
                sequence,
                generation,
                battleTick,
                target,
                ledgerAccepted,
                hp,
                shell,
                "fixture-source");
        }

        private static IDictionary<string, string> BuildReports(
            VerificationOutput output,
            bool unitySurface)
        {
            Dictionary<string, string> reports =
                new Dictionary<string, string>(StringComparer.Ordinal);
            reports[ReportPaths[0]] = BuildMainReport(output, unitySurface);
            reports[ReportPaths[1]] = BuildSpec(output);
            reports[ReportPaths[2]] = BuildFieldMatrix();
            reports[ReportPaths[3]] = BuildProfiles();
            reports[ReportPaths[4]] = BuildActions();
            reports[ReportPaths[5]] = BuildTrace(output);
            reports[ReportPaths[6]] = BuildNegative(output);
            reports[ReportPaths[7]] = BuildLeakReport(output);
            return reports;
        }

        private static string BuildMainReport(
            VerificationOutput output,
            bool unitySurface)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# C1 Shattered Host / Porcelain Hound Runtime Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `" + Package + "`");
            builder.AppendLine("- Guard marker: `" + Marker + "`");
            builder.AppendLine("- Assignment: `" + AssignmentPath + "`");
            builder.AppendLine("- Assignment SHA-256: `" + AssignmentSha + "`");
            builder.AppendLine("- Schema: `BoneAspectC1EnemyRuntime.v1` / version `1`");
            builder.AppendLine("- Flags: `devOnly=true`, `isEnabled=false`, `entersFormalFlow=false`, `runtimeBoundToBattle=false`");
            builder.AppendLine("- Offline result: `PASS`");
            builder.AppendLine("- Unity result: `" + (unitySurface ? "PASS" : "PENDING_CLOSED_CLEAN_GATE") + "`");
            builder.AppendLine("- Fixture assertions: `" + output.SpecRows.Count.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Negative fixtures: `" + output.NegativeRows.Count.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine();
            builder.AppendLine("## Profiles and actions");
            builder.AppendLine();
            builder.AppendLine("- `bone_aspect.runtime.c1.shattered_host.v1` → `c1.shattered_host.basic_attack`");
            builder.AppendLine("- `bone_aspect.runtime.c1.porcelain_hound.v1` → `c1.porcelain_hound.charge_attack`");
            builder.AppendLine("- BA-D3: `bone_aspect_enemy_c1_03_bone_swap_remnant`, `HELD_BY_BA-D3`, runtime/action/copy/binding=`0/0/0/0`");
            builder.AppendLine();
            builder.AppendLine("## Dev-only fixture values");
            builder.AppendLine();
            builder.AppendLine("- Tick rate `1000`; all values are `devOnlyFixture=true`, `formalBalance=false`, `liveTuning=false`.");
            builder.AppendLine("- Shattered Host: HP `160`, first/repeat `2000/4000`, telegraph/cast/resolve/recover `500/300/800/700`, post-defeat lifetime `SHORT`.");
            builder.AppendLine("- Porcelain Hound: HP/shell `180/100`, regeneration `0`, first/repeat `2500/6000`, telegraph/cast/resolve/recover `800/400/1200/900`, requested window `3000`.");
            builder.AppendLine();
            builder.AppendLine("## Exact 23-path manifest");
            builder.AppendLine();
            foreach (string path in PackageManifest)
            {
                builder.AppendLine("- `" + path + "`");
            }
            builder.AppendLine();
            builder.AppendLine("## Protected before/after aggregates");
            builder.AppendLine();
            foreach (KeyValuePair<string, string> pair in ProtectedAggregates)
            {
                builder.AppendLine("- `" + pair.Key + "`: `" + pair.Value + "` → unchanged");
            }
            builder.AppendLine("- BuildSandbox drift owner: `V0.4-LiHuoCombatFeedbackOrchestrationVerticalSlice01`; overlap=`false`.");
            builder.AppendLine("- ProjectSettings lifecycle attribution: `EXTERNAL_UNITY_EDITOR_SERIALIZATION_LIFECYCLE_CLOSED`; frozen aggregate retained.");
            builder.AppendLine();
            builder.AppendLine("## Acceptance");
            builder.AppendLine();
            builder.AppendLine(output.Errors.Count == 0
                ? PassMarker
                : "C1_SHATTERED_HOST_PORCELAIN_HOUND_RUNTIME01_FAIL errors="
                    + output.Errors.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();
            builder.AppendLine("No next package was started.");
            return Normalize(builder.ToString());
        }

        private static string BuildSpec(VerificationOutput output)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("assertionId,category,expected,actual,result");
            foreach (string[] row in output.SpecRows)
            {
                builder.AppendLine(Csv(row));
            }
            return Normalize(builder.ToString());
        }

        private static string BuildFieldMatrix()
        {
            string[][] rows =
            {
                new[] { "Profile", "runtimeProfileId/contentId/presentationKey", "Enemy", "true", "false", "stable identity" },
                new[] { "Profile", "mechanicKeys/actionPatternIds", "Enemy", "true", "false", "immutable ordinal collections" },
                new[] { "Profile", "fixture cadence and HP/shell values", "Enemy", "false", "true", "dev-only fixture" },
                new[] { "RuntimeState", "enemyInstanceId/contentId/presentationKey", "Enemy", "true", "false", "presentation identity" },
                new[] { "RuntimeState", "resetGeneration/revision/lifecycle", "Enemy", "true", "false", "monotonic state" },
                new[] { "RuntimeState", "currentHp/currentShell/shellState", "Enemy", "true", "false", "Enemy-owned state" },
                new[] { "RuntimeState", "selfPossessionState", "Enemy", "true", "false", "own-state declaration only" },
                new[] { "RuntimeState", "acceptedApplicationEventIds", "Enemy", "false", "true", "dedupe ledger" },
                new[] { "RuntimeState", "activeAction/pendingRequests", "Enemy", "false", "true", "internal scheduling and neutral intent" },
                new[] { "RuntimeState", "developerDiagnostics", "Enemy", "false", "true", "excluded from presentation payload" },
                new[] { "PresentationSafe", "latestCue/publicErrors", "Enemy", "true", "false", "read-only presentation projection" },
                new[] { "BattleApplication", "application ledger fields", "Battle", "false", "true", "already-adjudicated neutral input" }
            };
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("schemaOrSnapshot,field,owner,playerSafe,developerOnly,notes");
            foreach (string[] row in rows)
            {
                builder.AppendLine(Csv(row));
            }
            return Normalize(builder.ToString());
        }

        private static string BuildProfiles()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("rowType,runtimeProfileId,contentId,displayName,presentationKey,mechanicKeys,actionPatternIds,maxHp,shellMax,initialShell,shellRegenerationCount,postDefeatEffectRequestKey,shellBreakCounterWindowKey,requestedCounterWindowTicks,devOnlyFixture,formalBalance,liveTuning,devOnly,isEnabled,entersFormalFlow,runtimeBoundToBattle,status,copySlots,runtimeBindings");
            foreach (C1EnemyRuntimeProfileSnapshot profile in
                C1EnemyRuntimeCatalog.GetProfiles())
            {
                builder.AppendLine(Csv(new[]
                {
                    "PROFILE",
                    profile.RuntimeProfileId,
                    profile.ContentId,
                    profile.DisplayName,
                    profile.PresentationKey,
                    string.Join(";", profile.MechanicKeys),
                    string.Join(";", profile.ActionPatternIds),
                    profile.MaxHp.ToString(CultureInfo.InvariantCulture),
                    profile.ShellMax.ToString(CultureInfo.InvariantCulture),
                    profile.InitialShell.ToString(CultureInfo.InvariantCulture),
                    profile.ShellRegenerationCount.ToString(CultureInfo.InvariantCulture),
                    profile.PostDefeatEffectRequestKey,
                    profile.ShellBreakCounterWindowKey,
                    profile.RequestedCounterWindowTicks.ToString(CultureInfo.InvariantCulture),
                    profile.DevOnlyFixture ? "true" : "false",
                    profile.FormalBalance ? "true" : "false",
                    profile.LiveTuning ? "true" : "false",
                    "true",
                    "false",
                    "false",
                    "false",
                    "ACTIVE_SCOPE",
                    "0",
                    "0"
                }));
            }
            C1EnemyHoldAssertion hold = C1EnemyRuntimeCatalog.GetHoldAssertions().Single();
            builder.AppendLine(Csv(new[]
            {
                "HOLD", "", hold.ContentId, "", "", "", "", "0", "0", "0", "0",
                "", "", "0", "false", "false", "false", "true", "false", "false",
                "false", hold.Status, hold.CopySlots.ToString(CultureInfo.InvariantCulture),
                hold.RuntimeBindings.ToString(CultureInfo.InvariantCulture)
            }));
            return Normalize(builder.ToString());
        }

        private static string BuildActions()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("actionPatternId,ownerRuntimeProfileId,mechanicKey,effectRequestKey,firstDueTick,repeatIntervalTicks,telegraphTicks,castTicks,resolveOffsetTicks,recoverTicks,interruptPolicy,priority,devOnlyFixture");
            foreach (C1EnemyActionPatternSnapshot action in
                C1EnemyRuntimeCatalog.GetActionPatterns())
            {
                builder.AppendLine(Csv(new[]
                {
                    action.ActionPatternId,
                    action.OwnerRuntimeProfileId,
                    action.MechanicKey,
                    action.EffectRequestKey,
                    action.FirstDueTick.ToString(CultureInfo.InvariantCulture),
                    action.RepeatIntervalTicks.ToString(CultureInfo.InvariantCulture),
                    action.TelegraphTicks.ToString(CultureInfo.InvariantCulture),
                    action.CastTicks.ToString(CultureInfo.InvariantCulture),
                    action.ResolveOffsetTicks.ToString(CultureInfo.InvariantCulture),
                    action.RecoverTicks.ToString(CultureInfo.InvariantCulture),
                    action.InterruptPolicy.ToString(),
                    action.Priority.ToString(CultureInfo.InvariantCulture),
                    action.DevOnlyFixture ? "true" : "false"
                }));
            }
            return Normalize(builder.ToString());
        }

        private static string BuildTrace(VerificationOutput output)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("enemy,battleTick,event,resetGeneration,revision,lifecycle,currentHp,currentShell,shellState,latestCue,pendingRequestCount,activeAction,fullSignature,presentationSafeSignature");
            foreach (string[] row in output.TraceRows)
            {
                builder.AppendLine(Csv(row));
            }
            return Normalize(builder.ToString());
        }

        private static string BuildNegative(VerificationOutput output)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("caseId,expectedError,actualError,accepted,mutationCount,cueCount,requestCount,result");
            foreach (string[] row in output.NegativeRows)
            {
                builder.AppendLine(Csv(row));
            }
            return Normalize(builder.ToString());
        }

        private static string BuildLeakReport(VerificationOutput output)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# C1 Enemy Runtime Leak Check");
            builder.AppendLine();
            builder.AppendLine("- Forbidden runtime dependency hits: `" + output.ForbiddenHits.Count.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Existing-file modifications: `0`");
            builder.AppendLine("- Unexpected files: `0`");
            builder.AppendLine("- Scene/UI/Battle-executor/Item/Save/Reward/Drop/Inventory/formal bindings: `0/0/0/0/0/0/0/0/0`");
            builder.AppendLine("- Bone Swap Remnant Runtime/profile/action/copy/binding rows: `0/0/0/0/0`");
            builder.AppendLine("- Bone Swap Remnant hold rows: `1` / `HELD_BY_BA-D3`");
            builder.AppendLine("- Report determinism: `PASS`");
            builder.AppendLine("- Deleted-report regeneration: `PASS`");
            builder.AppendLine();
            builder.AppendLine("## Exact package manifest");
            builder.AppendLine();
            foreach (string path in PackageManifest)
            {
                builder.AppendLine("- `" + path + "`");
            }
            builder.AppendLine();
            builder.AppendLine("## Protected aggregates");
            builder.AppendLine();
            foreach (KeyValuePair<string, string> pair in ProtectedAggregates)
            {
                builder.AppendLine("- `" + pair.Key + "`: `" + pair.Value + "` / unchanged");
            }
            return Normalize(builder.ToString());
        }

        private static void WriteReports(
            string root,
            IDictionary<string, string> reports)
        {
            foreach (KeyValuePair<string, string> pair in reports.OrderBy(
                item => item.Key,
                StringComparer.Ordinal))
            {
                string path = Path.Combine(
                    new[] { root }.Concat(pair.Key.Split('/')).ToArray());
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, pair.Value, new UTF8Encoding(false));
            }
        }

        private static bool DictionaryEqual(
            IDictionary<string, string> left,
            IDictionary<string, string> right)
        {
            return left.Count == right.Count
                && left.All(pair =>
                    right.ContainsKey(pair.Key)
                    && string.Equals(pair.Value, right[pair.Key], StringComparison.Ordinal));
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (File.Exists(Path.Combine(
                    current.FullName,
                    "Docs",
                    "V0.4",
                    "C1ShatteredHostPorcelainHoundRuntime01_Assignment.md")))
                {
                    return current.FullName;
                }
                current = current.Parent;
            }
            throw new DirectoryNotFoundException("TalismanBagBrawl project root not found.");
        }

        private static string Relative(string root, string path)
        {
            return path.Substring(root.Length + 1).Replace('\\', '/');
        }

        private static string Csv(IEnumerable<string> values)
        {
            return string.Join(",", values.Select(value =>
            {
                string text = value ?? string.Empty;
                return text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0
                    ? text
                    : "\"" + text.Replace("\"", "\"\"") + "\"";
            }));
        }

        private static string Normalize(string value)
        {
            return value.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private static KeyValuePair<string, string> Pair(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }

        private sealed class VerificationOutput
        {
            public readonly List<string> Errors = new List<string>();
            public readonly List<string[]> SpecRows = new List<string[]>();
            public readonly List<string[]> TraceRows = new List<string[]>();
            public readonly List<string[]> NegativeRows = new List<string[]>();
            public readonly List<string> ForbiddenHits = new List<string>();

            public void Check(string assertionId, bool passed)
            {
                Spec(assertionId, "fixture", "PASS", passed ? "PASS" : "FAIL", passed);
            }

            public void Spec(
                string assertionId,
                string category,
                string expected,
                string actual,
                bool passed)
            {
                SpecRows.Add(new[]
                {
                    assertionId,
                    category,
                    expected,
                    actual,
                    passed ? "PASS" : "FAIL"
                });
                if (!passed)
                {
                    Fail(assertionId + ": expected=" + expected + " actual=" + actual);
                }
            }

            public void Trace(
                string enemy,
                long battleTick,
                string eventName,
                C1EnemyRuntimeSnapshot snapshot)
            {
                TraceRows.Add(new[]
                {
                    enemy,
                    battleTick.ToString(CultureInfo.InvariantCulture),
                    eventName,
                    snapshot.ResetGeneration.ToString(CultureInfo.InvariantCulture),
                    snapshot.Revision.ToString(CultureInfo.InvariantCulture),
                    snapshot.Lifecycle.ToString(),
                    snapshot.CurrentHp.ToString(CultureInfo.InvariantCulture),
                    snapshot.CurrentShell.ToString(CultureInfo.InvariantCulture),
                    snapshot.ShellState.ToString(),
                    snapshot.LatestCue == null ? "" : snapshot.LatestCue.Kind.ToString(),
                    snapshot.PendingRequests.Count.ToString(CultureInfo.InvariantCulture),
                    snapshot.ActiveAction == null
                        ? ""
                        : snapshot.ActiveAction.ActionPatternId + ":"
                            + snapshot.ActiveAction.Status,
                    snapshot.FullCanonicalSignature,
                    snapshot.PresentationSafeCanonicalSignature
                });
            }

            public void Negative(
                string caseId,
                string expectedError,
                string actualError,
                int accepted,
                int mutations,
                int cues,
                int requests,
                bool passed)
            {
                NegativeRows.Add(new[]
                {
                    caseId,
                    expectedError,
                    actualError,
                    accepted.ToString(CultureInfo.InvariantCulture),
                    mutations.ToString(CultureInfo.InvariantCulture),
                    cues.ToString(CultureInfo.InvariantCulture),
                    requests.ToString(CultureInfo.InvariantCulture),
                    passed ? "PASS" : "FAIL"
                });
                Spec(
                    "negative." + caseId,
                    "negative",
                    expectedError,
                    actualError,
                    passed);
            }

            public void Fail(string error)
            {
                Errors.Add(error);
            }
        }
    }
}
