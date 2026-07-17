using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.Vocabulary;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class CounterWindowAndPressureSchemaVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const string PressureShield = "dev_pressure_fixture_shield";
        private const string PressureCast = "dev_pressure_fixture_cast";
        private const string PressureStatus = "dev_pressure_fixture_status";
        private const string PressureFormation = "dev_pressure_fixture_formation";
        private const string WindowShell = "dev_window_fixture_shell_break";
        private const string WindowInterrupt = "dev_window_fixture_interrupt";
        private const string WindowPhase = "dev_window_fixture_phase_counter";
        private const string SkillGuard = "dev_skill_fixture_guard_cast";
        private const string SkillPressure = "dev_skill_fixture_pressure_channel";
        private const string SkillPulse = "dev_skill_fixture_spirit_pulse";
        private const string SkillCore = "dev_skill_fixture_core_channel";
        private const string PhaseOpening = "dev_phase_fixture_opening";
        private const string PhasePressure = "dev_phase_fixture_pressure";
        private const string PhaseSignal = "dev_phase_fixture_signal";
        private const string EnemyId = "dev_enemy_caster_chanter";
        private const string EnemyProfileId = "dev_enemy_caster_problem";
        private const string BossIdA = "dev_boss_caster_zhenhun";
        private const string BossIdB = "dev_boss_energy_juneng";
        private const string BossProfileId = "dev_boss_spirit_thief_core";
        private const string DiagnosticKey = "diagnostic.invalid_reference";

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyDomainPrimitives.cs"] = "83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyArchetypeSnapshots.cs"] = "7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainReferences.cs"] = "777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainSnapshot.cs"] = "AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainValidation.cs"] = "44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDomainDataContractVerifier.cs"] = "ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs"] = "243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs"] = "6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs"] = "49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs"] = "4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/EnemyValidationContentSnapshot.cs"] = "DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/EnemyValidationContentNormalizerCore.cs"] = "67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs"] = "452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizeVerifier.cs"] = "D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionPrimitives.cs"] = "9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionSnapshots.cs"] = "33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionValidation.cs"] = "72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs"] = "894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhasePrimitives.cs"] = "4F08886ABE8E6E7C9988E515A4CDDDD09D413001EF0B07E0FAFF3DADFE6FB6B6",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillPatternSnapshots.cs"] = "22A2FD124F32908141B93EB964D5DD1B3F099EF44BE72C9F367EF86557A2F499",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/BossPhaseSnapshots.cs"] = "B2FD5AA3367A8950D96F57AFC89C134B89A13A0557B55FDA4593DC78452F70F2",
                    ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhaseValidation.cs"] = "A175DF86360A993E9EE334D0EEE19C47725EB4CAED6BE613AA2788CD67CFB959",
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs"] = "3C7892A7A73A1464EE2D797B8F617EBA1639B1C7C6D4330FC166F64B32DAB267"
                });

        private static readonly IReadOnlyDictionary<string, string> LegacyHashes =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs"] = "C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9",
                    ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs"] = "14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060",
                    ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs"] = "A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4"
                });

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressurePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressurePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressureValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressureValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs.meta",
            "Docs/V0.4/Reports/CounterWindowAndPressureSchemaReport.md",
            "Docs/V0.4/Reports/CounterWindowAndPressureSchemaSpec.csv",
            "Docs/V0.4/Reports/CounterWindowAndPressureSchemaFieldMatrix.csv",
            "Docs/V0.4/Reports/CounterWindowAndPressureSchemaFixtureRows.csv",
            "Docs/V0.4/Reports/CounterWindowAndPressureSchemaLeakCheckReport.md"
        };

        private static readonly string[] RuntimeFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressurePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressureValidation.cs"
        };

        private static readonly string[] RuntimeForbiddenTokens =
        {
            "TalismanBag.BuildSandbox",
            "UnityEngine",
            "MonoBehaviour",
            "ScriptableObject",
            "GameObject",
            "Transform",
            "Addressables",
            "SceneManager",
            "BattleContract",
            "BattleBridge",
            "UnifiedBattlePage",
            "RunFlow",
            "PageState",
            "FormationState",
            "RewardConfig",
            "SaveData",
            "PlayerPrefs",
            "V02FormationGridFrame",
            "DamageText",
            "BuildReadiness",
            "BuildCapabilitySnapshot",
            "ItemSystem"
        };

        private static readonly string[] ExecutionForbiddenTokens =
        {
            "void Update(",
            "IEnumerator",
            "Coroutine",
            "StartCoroutine",
            "async ",
            "Task.Delay",
            "Thread.Sleep",
            "DateTime.Now",
            "Stopwatch",
            "ApplyDamage",
            "DealDamage",
            "event ",
            "ChangePhase",
            "TransitionPhase"
        };

        private enum FixtureMutation
        {
            None,
            PressureWeight,
            RequirementThreshold,
            Condition,
            Duration,
            Binding,
            Developer,
            PressurePlayer,
            WindowPlayer
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/CounterWindowAndPressureSchema01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }
#endif

        public static void VerifyBatch()
        {
            VerifyAndWrite(IsBatchMode(), "Unity batch");
        }

        public static void VerifyOffline()
        {
            VerifyAndWrite(false, "Pure C# same-source offline verifier");
        }

        public static int Main(string[] args)
        {
            try
            {
                VerifyOffline();
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
        }

        private static void VerifyAndWrite(bool exitWhenDone, string mode)
        {
            string root = FindProjectRoot();
            List<Check> checks = new List<Check>();
            CounterWindowAndPressureCatalogSnapshot catalog = null;
            try
            {
                EnemyValidationContentSnapshot normalization =
                    EnemyValidationContentNormalizer.CreateSnapshot();
                EnemyMechanicVocabularySnapshot vocabulary =
                    DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                        DefaultEnemyMechanicVocabularyCatalog.CreateInput());
                E02E03Resolver e05Resolver = new E02E03Resolver(normalization, vocabulary);
                EnemySkillBossPhaseCatalogSnapshot skillPhase =
                    DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(
                        CreateSkillPhaseFixture(),
                        e05Resolver);
                E02E03E05Resolver resolver = new E02E03E05Resolver(
                    normalization,
                    vocabulary,
                    skillPhase);
                catalog = Snapshot(CreateFixture(), resolver);
                RunChecks(checks, root, normalization, vocabulary, skillPhase, resolver, catalog);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
            }

            if (catalog != null)
            {
                WriteReports(root, mode, checks, catalog);
                RunGeneratedFileChecks(checks, root);
                WriteReports(root, mode, checks, catalog);
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine(
                (pass ? "COUNTER_WINDOW_AND_PRESSURE_SCHEMA_PASS " : "COUNTER_WINDOW_AND_PRESSURE_SCHEMA_FAIL ")
                + checks.Count(value => value.Passed)
                + "/"
                + checks.Count);
#if UNITY_EDITOR
            if (exitWhenDone)
            {
                EditorApplication.Exit(pass ? 0 : 1);
            }
#endif
            if (!pass)
            {
                throw new InvalidOperationException(
                    "CounterWindowAndPressureSchema01 verifier failed: "
                    + string.Join(
                        " | ",
                        checks.Where(value => !value.Passed)
                            .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static void RunChecks(
            List<Check> checks,
            string root,
            EnemyValidationContentSnapshot normalization,
            EnemyMechanicVocabularySnapshot vocabulary,
            EnemySkillBossPhaseCatalogSnapshot skillPhase,
            E02E03E05Resolver resolver,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            Add(checks, "schema.id", CounterWindowAndPressureSchema.SchemaId, catalog.SchemaId,
                string.Equals(catalog.SchemaId, CounterWindowAndPressureSchema.SchemaId, StringComparison.Ordinal));
            Add(checks, "schema.version", "1", catalog.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                catalog.SchemaVersion == CounterWindowAndPressureSchema.SchemaVersion);
            Add(checks, "fixture.pressures", "4", catalog.BuildPressureProfiles.Count.ToString(CultureInfo.InvariantCulture),
                catalog.BuildPressureProfiles.Count == 4);
            Add(checks, "fixture.windows", "3", catalog.CounterWindowProfiles.Count.ToString(CultureInfo.InvariantCulture),
                catalog.CounterWindowProfiles.Count == 3);
            Add(checks, "fixture.pressureSources", "6", catalog.PressureSourceBindings.Count.ToString(CultureInfo.InvariantCulture),
                catalog.PressureSourceBindings.Count == 6);
            Add(checks, "fixture.windowSources", "4", catalog.CounterWindowSourceBindings.Count.ToString(CultureInfo.InvariantCulture),
                catalog.CounterWindowSourceBindings.Count == 4);
            Add(checks, "fixture.pressureWindows", "5", catalog.PressureCounterWindowBindings.Count.ToString(CultureInfo.InvariantCulture),
                catalog.PressureCounterWindowBindings.Count == 5);

            int requirements = catalog.BuildPressureProfiles
                .SelectMany(value => value.DeveloperOnly.RequirementGroups)
                .Sum(value => value.Requirements.Count);
            Add(checks, "fixture.requirements", ">=8", requirements.ToString(CultureInfo.InvariantCulture),
                requirements >= 8);
            Add(checks, "requirements.anyRequired", "present", "evaluated",
                catalog.BuildPressureProfiles.SelectMany(value => value.DeveloperOnly.RequirementGroups)
                    .Any(value => value.RequirementRole == CapabilityRequirementRole.Required
                        && value.RequirementMatchMode == RequirementMatchMode.Any));
            Add(checks, "requirements.allRequired", "present", "evaluated",
                catalog.BuildPressureProfiles.SelectMany(value => value.DeveloperOnly.RequirementGroups)
                    .Any(value => value.RequirementRole == CapabilityRequirementRole.Required
                        && value.RequirementMatchMode == RequirementMatchMode.All));
            Add(checks, "requirements.recommended", "present", "evaluated",
                catalog.BuildPressureProfiles.SelectMany(value => value.DeveloperOnly.RequirementGroups)
                    .Any(value => value.RequirementRole == CapabilityRequirementRole.Recommended));

            RunReuseChecks(checks, catalog);
            RunConditionChecks(checks, catalog);
            RunLookupChecks(checks, catalog);
            RunImmutabilityChecks(checks, catalog);
            RunSignatureChecks(checks, resolver, catalog);
            RunValidationRejectionChecks(checks, resolver);
            RunBaselineChecks(checks, root, normalization, vocabulary, skillPhase);
            RunLeakChecks(checks, root, catalog);
        }

        private static void RunReuseChecks(
            List<Check> checks,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            bool pressureReused = catalog.PressureSourceBindings
                .GroupBy(value => value.BuildPressureProfileId, StringComparer.Ordinal)
                .Any(group => group.Select(value => value.SourceKind + ":" + value.SourceId)
                    .Distinct(StringComparer.Ordinal).Count() > 1);
            bool sourceMultiplePressures = catalog.PressureSourceBindings
                .GroupBy(value => value.SourceKind + ":" + value.SourceId, StringComparer.Ordinal)
                .Any(group => group.Select(value => value.BuildPressureProfileId)
                    .Distinct(StringComparer.Ordinal).Count() > 1);
            bool windowReused = catalog.CounterWindowSourceBindings
                .GroupBy(value => value.CounterWindowId, StringComparer.Ordinal)
                .Any(group => group.Select(value => value.SourceKind + ":" + value.SourceId)
                    .Distinct(StringComparer.Ordinal).Count() > 1);
            bool pressureMultipleWindows = catalog.PressureCounterWindowBindings
                .GroupBy(value => value.BuildPressureProfileId, StringComparer.Ordinal)
                .Any(group => group.Select(value => value.CounterWindowId)
                    .Distinct(StringComparer.Ordinal).Count() > 1);
            bool windowMultiplePressures = catalog.PressureCounterWindowBindings
                .GroupBy(value => value.CounterWindowId, StringComparer.Ordinal)
                .Any(group => group.Select(value => value.BuildPressureProfileId)
                    .Distinct(StringComparer.Ordinal).Count() > 1);
            Add(checks, "reuse.pressureBySources", "PASS", pressureReused ? "PASS" : "FAIL", pressureReused);
            Add(checks, "reuse.sourceToPressures", "PASS", sourceMultiplePressures ? "PASS" : "FAIL", sourceMultiplePressures);
            Add(checks, "reuse.windowBySources", "PASS", windowReused ? "PASS" : "FAIL", windowReused);
            Add(checks, "reuse.pressureToWindows", "PASS", pressureMultipleWindows ? "PASS" : "FAIL", pressureMultipleWindows);
            Add(checks, "reuse.windowToPressures", "PASS", windowMultiplePressures ? "PASS" : "FAIL", windowMultiplePressures);
        }

        private static void RunConditionChecks(
            List<Check> checks,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            CounterWindowConditionKind[] kinds = catalog.CounterWindowProfiles
                .SelectMany(value => value.InternalOnly.OpenConditions.Concat(value.InternalOnly.CloseConditions))
                .Select(value => value.Kind)
                .Distinct()
                .ToArray();
            Add(checks, "conditions.mechanicSignal", "present", "evaluated",
                kinds.Contains(CounterWindowConditionKind.MechanicSignal));
            Add(checks, "conditions.skillInterrupted", "present", "evaluated",
                kinds.Contains(CounterWindowConditionKind.SkillPatternInterrupted));
            Add(checks, "conditions.bossPhaseEntered", "present", "evaluated",
                kinds.Contains(CounterWindowConditionKind.BossPhaseEntered));
            Add(checks, "conditions.timedClose", "present", "evaluated",
                catalog.CounterWindowProfiles.Any(value => value.InternalOnly.MaximumDurationMilliseconds > 0
                    && value.InternalOnly.CloseConditions.Count == 0));
            Add(checks, "conditions.conditionClose", "present", "evaluated",
                catalog.CounterWindowProfiles.Any(value => value.InternalOnly.MaximumDurationMilliseconds == 0
                    && value.InternalOnly.CloseConditions.Count > 0));
            Add(checks, "conditions.firstOfTimeOrCondition", "present", "evaluated",
                catalog.CounterWindowProfiles.Any(value => value.InternalOnly.MaximumDurationMilliseconds > 0
                    && value.InternalOnly.CloseConditions.Count > 0));
        }

        private static void RunLookupChecks(
            List<Check> checks,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            Add(checks, "lookup.pressure.exact", "true", "evaluated",
                catalog.TryGetBuildPressureProfileById(PressureShield, out _));
            Add(checks, "lookup.pressure.ordinalCase", "false", "evaluated",
                !catalog.TryGetBuildPressureProfileById(PressureShield.ToUpperInvariant(), out _));
            Add(checks, "lookup.window.exact", "true", "evaluated",
                catalog.TryGetCounterWindowProfileById(WindowShell, out _));
            Add(checks, "lookup.window.ordinalCase", "false", "evaluated",
                !catalog.TryGetCounterWindowProfileById(WindowShell.ToUpperInvariant(), out _));
            Add(checks, "lookup.null", "false", "evaluated",
                !catalog.TryGetBuildPressureProfileById(null, out _)
                    && !catalog.TryGetCounterWindowProfileById(null, out _));
        }

        private static void RunImmutabilityChecks(
            List<Check> checks,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            AddReadOnlyCheck(checks, "immutable.pressures", catalog.BuildPressureProfiles);
            AddReadOnlyCheck(checks, "immutable.windows", catalog.CounterWindowProfiles);
            AddReadOnlyCheck(checks, "immutable.pressureSources", catalog.PressureSourceBindings);
            AddReadOnlyCheck(checks, "immutable.windowSources", catalog.CounterWindowSourceBindings);
            AddReadOnlyCheck(checks, "immutable.pressureWindows", catalog.PressureCounterWindowBindings);
            AddReadOnlyCheck(checks, "immutable.contributions",
                catalog.BuildPressureProfiles[0].InternalOnly.PressureChannelContributions);
            AddReadOnlyCheck(checks, "immutable.requirementGroups",
                catalog.BuildPressureProfiles[0].DeveloperOnly.RequirementGroups);
            AddReadOnlyCheck(checks, "immutable.requirements",
                catalog.BuildPressureProfiles[0].DeveloperOnly.RequirementGroups[0].Requirements);
            AddReadOnlyCheck(checks, "immutable.openConditions",
                catalog.CounterWindowProfiles[0].InternalOnly.OpenConditions);
            AddReadOnlyCheck(checks, "immutable.closeConditions",
                catalog.CounterWindowProfiles[0].InternalOnly.CloseConditions);

            List<BuildPressureProfileSnapshot> source = CreateFixture().BuildPressureProfiles.ToList();
            CounterWindowAndPressureCatalogInput input = new CounterWindowAndPressureCatalogInput(
                source,
                CreateFixture().CounterWindowProfiles,
                CreateFixture().PressureSourceBindings,
                CreateFixture().CounterWindowSourceBindings,
                CreateFixture().PressureCounterWindowBindings);
            int count = input.BuildPressureProfiles.Count;
            source.Clear();
            Add(checks, "immutable.defensiveCopy", count.ToString(CultureInfo.InvariantCulture),
                input.BuildPressureProfiles.Count.ToString(CultureInfo.InvariantCulture),
                input.BuildPressureProfiles.Count == count);
        }

        private static void RunSignatureChecks(
            List<Check> checks,
            E02E03E05Resolver resolver,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            CounterWindowAndPressureCatalogSnapshot repeated = Snapshot(CreateFixture(), resolver);
            CounterWindowAndPressureCatalogSnapshot reversed = Snapshot(CreateFixture(true), resolver);
            Add(checks, "signature.full.format", "sha256:+64 lowercase hex", catalog.CanonicalSignature,
                IsCanonicalSignature(catalog.CanonicalSignature));
            Add(checks, "signature.player.format", "sha256:+64 lowercase hex", catalog.PlayerSafeCanonicalSignature,
                IsCanonicalSignature(catalog.PlayerSafeCanonicalSignature));
            Add(checks, "signature.repeated.full", catalog.CanonicalSignature, repeated.CanonicalSignature,
                string.Equals(catalog.CanonicalSignature, repeated.CanonicalSignature, StringComparison.Ordinal));
            Add(checks, "signature.repeated.player", catalog.PlayerSafeCanonicalSignature, repeated.PlayerSafeCanonicalSignature,
                string.Equals(catalog.PlayerSafeCanonicalSignature, repeated.PlayerSafeCanonicalSignature, StringComparison.Ordinal));
            Add(checks, "signature.inputOrder.full", catalog.CanonicalSignature, reversed.CanonicalSignature,
                string.Equals(catalog.CanonicalSignature, reversed.CanonicalSignature, StringComparison.Ordinal));
            Add(checks, "signature.inputOrder.player", catalog.PlayerSafeCanonicalSignature, reversed.PlayerSafeCanonicalSignature,
                string.Equals(catalog.PlayerSafeCanonicalSignature, reversed.PlayerSafeCanonicalSignature, StringComparison.Ordinal));

            AddFullOnlyMutation(checks, resolver, catalog, FixtureMutation.PressureWeight, "pressureWeight");
            AddFullOnlyMutation(checks, resolver, catalog, FixtureMutation.RequirementThreshold, "requirementThreshold");
            AddFullOnlyMutation(checks, resolver, catalog, FixtureMutation.Condition, "condition");
            AddFullOnlyMutation(checks, resolver, catalog, FixtureMutation.Duration, "duration");
            AddFullOnlyMutation(checks, resolver, catalog, FixtureMutation.Binding, "binding");
            AddFullOnlyMutation(checks, resolver, catalog, FixtureMutation.Developer, "developer");
            AddBothMutation(checks, resolver, catalog, FixtureMutation.PressurePlayer, "pressurePlayer");
            AddBothMutation(checks, resolver, catalog, FixtureMutation.WindowPlayer, "windowPlayer");
        }

        private static void RunValidationRejectionChecks(
            List<Check> checks,
            E02E03E05Resolver resolver)
        {
            ExpectCode(checks, "validation.inputNull", "INPUT_NULL", null, resolver);
            CounterWindowAndPressureCatalogInput valid = CreateFixture();
            ExpectCode(checks, "validation.resolverNull", "REFERENCE_RESOLVER_NULL", valid, null);
            ExpectCode(checks, "validation.schemaId", "SCHEMA_ID_MISMATCH",
                new CounterWindowAndPressureCatalogInput(valid.BuildPressureProfiles, valid.CounterWindowProfiles,
                    valid.PressureSourceBindings, valid.CounterWindowSourceBindings, valid.PressureCounterWindowBindings,
                    "CounterWindowAndPressure.V1", 1), resolver);
            ExpectCode(checks, "validation.schemaVersion", "SCHEMA_VERSION_MISMATCH",
                new CounterWindowAndPressureCatalogInput(valid.BuildPressureProfiles, valid.CounterWindowProfiles,
                    valid.PressureSourceBindings, valid.CounterWindowSourceBindings, valid.PressureCounterWindowBindings,
                    CounterWindowAndPressureSchema.SchemaId, 2), resolver);

            BuildPressureProfileSnapshot basePressure = valid.BuildPressureProfiles[0];
            CounterWindowProfileSnapshot baseWindow = valid.CounterWindowProfiles[0];
            ExpectCode(checks, "validation.pressureIdEmpty", "ID_EMPTY",
                Replace(valid, pressures: new[] { Pressure("", OneContribution("pressure.shield"), DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.outerWhitespace", "ID_OUTER_WHITESPACE",
                Replace(valid, pressures: new[] { Pressure(" dev_pressure ", OneContribution("pressure.shield"), DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.pressureDuplicate", "BUILD_PRESSURE_PROFILE_ID_DUPLICATE",
                Replace(valid, pressures: valid.BuildPressureProfiles.Concat(new[] { basePressure })), resolver);
            ExpectCode(checks, "validation.windowDuplicate", "COUNTER_WINDOW_ID_DUPLICATE",
                Replace(valid, windows: valid.CounterWindowProfiles.Concat(new[] { baseWindow })), resolver);
            ExpectCode(checks, "validation.contributionsEmpty", "PRESSURE_CHANNEL_CONTRIBUTIONS_EMPTY",
                Replace(valid, pressures: new[] { Pressure(PressureShield, Array.Empty<PressureChannelContributionSnapshot>(), DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.pressureWeightRange", "PRESSURE_WEIGHT_OUT_OF_RANGE",
                Replace(valid, pressures: new[] { Pressure(PressureShield,
                    new[] { new PressureChannelContributionSnapshot("pressure.shield", 0) }, DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.pressureWeightTotal", "PRESSURE_WEIGHT_TOTAL_INVALID",
                Replace(valid, pressures: new[] { Pressure(PressureShield,
                    new[] { new PressureChannelContributionSnapshot("pressure.shield", 9999) }, DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.pressureKeyDuplicate", "PRESSURE_CHANNEL_KEY_DUPLICATE",
                Replace(valid, pressures: new[] { Pressure(PressureShield,
                    new[] { new PressureChannelContributionSnapshot("pressure.shield", 5000),
                        new PressureChannelContributionSnapshot("pressure.shield", 5000) }, DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.pressureKeyUnknown", "PRESSURE_CHANNEL_KEY_UNRESOLVED",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.unknown"), DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.caseStrict", "PRESSURE_CHANNEL_KEY_UNRESOLVED",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("PRESSURE.SHIELD"), DefaultGroups()) }), resolver);
            ExpectCode(checks, "validation.groupsEmpty", "REQUIREMENT_GROUPS_EMPTY",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"),
                    Array.Empty<BuildCapabilityRequirementGroupSnapshot>()) }), resolver);
            ExpectCode(checks, "validation.groupEmpty", "REQUIREMENT_GROUP_EMPTY",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"),
                    new[] { Group("group.empty", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                        Array.Empty<BuildCapabilityRequirementSnapshot>()) }) }), resolver);
            BuildCapabilityRequirementGroupSnapshot duplicateGroup = DefaultGroups()[0];
            ExpectCode(checks, "validation.groupDuplicate", "REQUIREMENT_GROUP_ID_DUPLICATE",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"),
                    new[] { duplicateGroup, duplicateGroup }) }), resolver);
            ExpectCode(checks, "validation.capabilityDuplicate", "BUILD_CAPABILITY_KEY_DUPLICATE",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"),
                    new[] { Group("group.duplicate", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        new[] { Requirement("capability.break_power", 4000), Requirement("capability.break_power", 6000) }) }) }), resolver);
            ExpectCode(checks, "validation.capabilityUnknown", "BUILD_CAPABILITY_KEY_UNRESOLVED",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"),
                    new[] { Group("group.unknown", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                        new[] { Requirement("capability.unknown", 4000) }) }) }), resolver);
            ExpectCode(checks, "validation.capabilityThreshold", "MINIMUM_CAPABILITY_OUT_OF_RANGE",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"),
                    new[] { Group("group.threshold", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                        new[] { Requirement("capability.break_power", 10001) }) }) }), resolver);
            ExpectCode(checks, "validation.hintUnknown", "PLAYER_HINT_CATEGORY_UNRESOLVED",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"), DefaultGroups(),
                    hintKeys: new[] { "player_hint.unknown" }) }), resolver);
            ExpectCode(checks, "validation.diagnosticUnknown", "DEVELOPER_DIAGNOSTIC_CATEGORY_UNRESOLVED",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"), DefaultGroups(),
                    diagnosticKeys: new[] { "diagnostic.unknown" }) }), resolver);
            ExpectCode(checks, "validation.pressurePlayerId", "BUILD_PRESSURE_PLAYER_ID_MISMATCH",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"), DefaultGroups(),
                    playerId: "dev_pressure_fixture_other") }), resolver);
            ExpectCode(checks, "validation.pressureIsolation", "DEV_ISOLATION_INVALID",
                Replace(valid, pressures: new[] { Pressure(PressureShield, OneContribution("pressure.shield"), DefaultGroups(),
                    devOnly: false) }), resolver);

            ExpectCode(checks, "validation.windowTypeUnknown", "COUNTER_WINDOW_TYPE_KEY_UNRESOLVED",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.unknown",
                    DefaultOpen(), Array.Empty<CounterWindowConditionSnapshot>(), 1000) }), resolver);
            ExpectCode(checks, "validation.openEmpty", "OPEN_CONDITIONS_EMPTY",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    Array.Empty<CounterWindowConditionSnapshot>(), Array.Empty<CounterWindowConditionSnapshot>(), 1000) }), resolver);
            CounterWindowConditionSnapshot duplicateCondition =
                new CounterWindowConditionSnapshot("condition.duplicate", CounterWindowConditionKind.MechanicSignal, "signal.fixture");
            ExpectCode(checks, "validation.conditionDuplicate", "CONDITION_ID_DUPLICATE",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    new[] { duplicateCondition }, new[] { duplicateCondition }, 1000) }), resolver);
            ExpectCode(checks, "validation.conditionKind", "CONDITION_KIND_INVALID",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    new[] { new CounterWindowConditionSnapshot("condition.invalid", (CounterWindowConditionKind)99, SkillPressure) },
                    Array.Empty<CounterWindowConditionSnapshot>(), 1000) }), resolver);
            ExpectCode(checks, "validation.conditionReference", "CONDITION_REFERENCE_UNRESOLVED",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    new[] { new CounterWindowConditionSnapshot("condition.unknown", CounterWindowConditionKind.SkillPatternStarted, "dev_skill_unknown") },
                    Array.Empty<CounterWindowConditionSnapshot>(), 1000) }), resolver);
            ExpectCode(checks, "validation.durationNegative", "MAXIMUM_DURATION_NEGATIVE",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    DefaultOpen(), Array.Empty<CounterWindowConditionSnapshot>(), -1) }), resolver);
            ExpectCode(checks, "validation.closeMissing", "WINDOW_CLOSE_RULE_MISSING",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    DefaultOpen(), Array.Empty<CounterWindowConditionSnapshot>(), 0) }), resolver);
            ExpectCode(checks, "validation.windowPlayerId", "COUNTER_WINDOW_PLAYER_ID_MISMATCH",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    DefaultOpen(), Array.Empty<CounterWindowConditionSnapshot>(), 1000,
                    playerId: "dev_window_fixture_other") }), resolver);
            ExpectCode(checks, "validation.windowIsolation", "DEV_ISOLATION_INVALID",
                Replace(valid, windows: new[] { Window(WindowShell, "counter_window.shell_break",
                    DefaultOpen(), Array.Empty<CounterWindowConditionSnapshot>(), 1000,
                    referenceEnabled: true) }), resolver);

            ExpectCode(checks, "validation.pressureSourceDuplicate", "PRESSURE_SOURCE_BINDING_DUPLICATE",
                Replace(valid, pressureSources: valid.PressureSourceBindings.Concat(new[] { valid.PressureSourceBindings[0] })), resolver);
            ExpectCode(checks, "validation.windowSourceDuplicate", "COUNTER_WINDOW_SOURCE_BINDING_DUPLICATE",
                Replace(valid, windowSources: valid.CounterWindowSourceBindings.Concat(new[] { valid.CounterWindowSourceBindings[0] })), resolver);
            ExpectCode(checks, "validation.pressureWindowDuplicate", "PRESSURE_COUNTER_WINDOW_BINDING_DUPLICATE",
                Replace(valid, pressureWindows: valid.PressureCounterWindowBindings.Concat(new[] { valid.PressureCounterWindowBindings[0] })), resolver);
            ExpectCode(checks, "validation.sourceUnresolved", "PRESSURE_SOURCE_REFERENCE_UNRESOLVED",
                Replace(valid, pressureSources: new[] { new PressureSourceBindingSnapshot(
                    PressureSourceKind.MechanicProfile, "dev_mechanic_unknown", PressureShield) }), resolver);
            ExpectCode(checks, "validation.pressureReference", "BUILD_PRESSURE_REFERENCE_UNRESOLVED",
                Replace(valid, pressureWindows: new[] { new PressureCounterWindowBindingSnapshot(
                    "dev_pressure_unknown", WindowShell) }), resolver);
            ExpectCode(checks, "validation.windowReference", "COUNTER_WINDOW_REFERENCE_UNRESOLVED",
                Replace(valid, pressureWindows: new[] { new PressureCounterWindowBindingSnapshot(
                    PressureShield, "dev_window_unknown") }), resolver);
            ExpectCode(checks, "validation.bindingIsolation", "DEV_ISOLATION_INVALID",
                Replace(valid, pressureSources: new[] { new PressureSourceBindingSnapshot(
                    PressureSourceKind.MechanicProfile, "dev_enemy_shield_problem", PressureShield,
                    true, true, false) }), resolver);
            ExpectCode(checks, "validation.playerSafeLeak", "PLAYER_SAFE_PAYLOAD_LEAK",
                Replace(valid, pressures: valid.BuildPressureProfiles.Select(value =>
                    value.BuildPressureProfileId == PressureShield
                        ? Pressure(PressureShield, value.InternalOnly.PressureChannelContributions,
                            value.DeveloperOnly.RequirementGroups, labelKey: "pressureChannelKey")
                        : value)), resolver);
        }

        private static void RunBaselineChecks(
            List<Check> checks,
            string root,
            EnemyValidationContentSnapshot normalization,
            EnemyMechanicVocabularySnapshot vocabulary,
            EnemySkillBossPhaseCatalogSnapshot skillPhase)
        {
            Add(checks, "baseline.e02.vocabulary", "63", vocabulary.Entries.Count.ToString(CultureInfo.InvariantCulture),
                vocabulary.Entries.Count == 63);
            Add(checks, "baseline.e03.carriers", "11/7/18",
                normalization.Enemies.Count + "/" + normalization.Bosses.Count + "/" + (normalization.Enemies.Count + normalization.Bosses.Count),
                normalization.Enemies.Count == 11 && normalization.Bosses.Count == 7);
            Add(checks, "baseline.e03.profiles", "10/6/16",
                normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy)
                    + "/" + normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss)
                    + "/" + normalization.MechanicProfiles.Count,
                normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy) == 10
                    && normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss) == 6);
            Add(checks, "baseline.e03.maps", "10", normalization.MapRules.Count.ToString(CultureInfo.InvariantCulture),
                normalization.MapRules.Count == 10);
            Add(checks, "baseline.e03.carrierBindings", "17", normalization.CarrierMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                normalization.CarrierMechanicBindings.Count == 17);
            Add(checks, "baseline.e03.mapBindings", "30", normalization.MapRuleMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                normalization.MapRuleMechanicBindings.Count == 30);
            Add(checks, "baseline.e03.exceptions", "2", normalization.Exceptions.Count.ToString(CultureInfo.InvariantCulture),
                normalization.Exceptions.Count == 2);
            Add(checks, "baseline.e05.fixture", "4/3/3/3/2",
                skillPhase.SkillPatterns.Count + "/" + skillPhase.SkillSequences.Count + "/"
                    + skillPhase.CarrierSkillBindings.Count + "/" + skillPhase.BossPhaseProfiles.Count + "/"
                    + skillPhase.BossPhasePlans.Count,
                skillPhase.SkillPatterns.Count == 4 && skillPhase.SkillSequences.Count == 3
                    && skillPhase.CarrierSkillBindings.Count == 3 && skillPhase.BossPhaseProfiles.Count == 3
                    && skillPhase.BossPhasePlans.Count == 2);

            string e04Path = Absolute(root, "Docs/V0.4/Reports/EncounterCompositionSchemaFixtureRows.csv");
            string[] e04Rows = File.Exists(e04Path)
                ? File.ReadAllLines(e04Path).Skip(1).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray()
                : Array.Empty<string>();
            int encounters = e04Rows.Select(value => CsvColumn(value, 0)).Distinct(StringComparer.Ordinal).Count();
            int waves = e04Rows.Select(value => CsvColumn(value, 0) + "\u001f" + CsvColumn(value, 1))
                .Distinct(StringComparer.Ordinal).Count();
            Add(checks, "baseline.e04.fixture", "2/4/6", encounters + "/" + waves + "/" + e04Rows.Length,
                encounters == 2 && waves == 4 && e04Rows.Length == 6);

            AddHashChecks(checks, root, "protected", ProtectedHashes);
            AddHashChecks(checks, root, "legacy", LegacyHashes);
        }

        private static void RunLeakChecks(
            List<Check> checks,
            string root,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            int runtimeLeaks = 0;
            foreach (string relative in RuntimeFiles)
            {
                string text = File.ReadAllText(Absolute(root, relative));
                foreach (string token in RuntimeForbiddenTokens.Concat(ExecutionForbiddenTokens))
                {
                    int count = Count(text, token);
                    runtimeLeaks += count;
                    Add(checks, "leak.runtime." + Path.GetFileName(relative) + "." + SafeId(token),
                        "0", count.ToString(CultureInfo.InvariantCulture), count == 0);
                }
            }

            Add(checks, "leak.runtime.total", "0", runtimeLeaks.ToString(CultureInfo.InvariantCulture),
                runtimeLeaks == 0);
            string packageText = string.Join("\n", RuntimeFiles.Select(value => File.ReadAllText(Absolute(root, value))));
            int chapters = Count(packageText, "3-10") + Count(packageText, "4-10");
            int answerLeaks = Count(packageText, "BossKey") + Count(packageText, "DropBias")
                + Count(packageText, "hardSolution") + Count(packageText, "formal reward");
            Add(checks, "leak.chapterHardcode", "0", chapters.ToString(CultureInfo.InvariantCulture), chapters == 0);
            Add(checks, "leak.answerAndReward", "0", answerLeaks.ToString(CultureInfo.InvariantCulture), answerLeaks == 0);

            string playerPayload = catalog.BuildPlayerSafeCanonicalPayload();
            string[] forbiddenPlayerTokens =
            {
                "pressureChannelKey", "weightBasisPoints", "counterWindowTypeKey",
                "openCondition", "closeCondition", "maximumDurationMilliseconds",
                "buildCapabilityKey", "requirementGroupId", "minimumCapabilityBasisPoints",
                "developerDiagnosticCategoryKeys", "sourceReferenceIds", "Binding"
            };
            int playerLeaks = forbiddenPlayerTokens.Sum(token => CountIgnoreCase(playerPayload, token));
            Add(checks, "leak.playerSafe", "0", playerLeaks.ToString(CultureInfo.InvariantCulture), playerLeaks == 0);

            string editorRoot = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem");
            string legacyUsingDirective = "using TalismanBag." + "BuildSandbox;";
            int legacyReaders = Directory.GetFiles(editorRoot, "*.cs", SearchOption.TopDirectoryOnly)
                .Count(path => File.ReadAllText(path).Contains(
                    legacyUsingDirective,
                    StringComparison.Ordinal));
            Add(checks, "leak.editorLegacyReader", "1", legacyReaders.ToString(CultureInfo.InvariantCulture),
                legacyReaders == 1);
        }

        private static void RunGeneratedFileChecks(List<Check> checks, string root)
        {
            int exists = ExpectedPackageFiles.Count(value => File.Exists(Absolute(root, value)));
            Add(checks, "package.expectedFiles", "16", exists.ToString(CultureInfo.InvariantCulture), exists == 16);
            string pressureRoot = Absolute(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow");
            int pressureFiles = Directory.Exists(pressureRoot)
                ? Directory.GetFiles(pressureRoot, "*", SearchOption.TopDirectoryOnly).Length
                : 0;
            Add(checks, "package.pressureWindowFiles", "8", pressureFiles.ToString(CultureInfo.InvariantCulture),
                pressureFiles == 8);
            string reports = Absolute(root, ReportRoot);
            int reportFiles = Directory.GetFiles(reports, "CounterWindowAndPressureSchema*", SearchOption.TopDirectoryOnly).Length;
            Add(checks, "package.reportFiles", "5", reportFiles.ToString(CultureInfo.InvariantCulture), reportFiles == 5);

            int trailing = ExpectedPackageFiles
                .Where(value => File.Exists(Absolute(root, value)))
                .Sum(value => CountTrailingWhitespace(Absolute(root, value)));
            Add(checks, "package.trailingWhitespace", "0", trailing.ToString(CultureInfo.InvariantCulture), trailing == 0);

            string[] metaPaths = ExpectedPackageFiles.Where(value => value.EndsWith(".meta", StringComparison.Ordinal)).ToArray();
            string[] allMeta = Directory.GetFiles(Absolute(root, "Assets"), "*.meta", SearchOption.AllDirectories);
            bool guidPass = true;
            foreach (string relative in metaPaths)
            {
                string guid = ReadMetaGuid(Absolute(root, relative));
                int count = allMeta.Count(path => string.Equals(ReadMetaGuid(path), guid, StringComparison.Ordinal));
                bool unique = guid.Length == 32 && count == 1;
                guidPass &= unique;
                Add(checks, "package.guid." + Path.GetFileName(relative), "unique", guid + " x" + count, unique);
            }
            Add(checks, "package.guidConflicts", "0", guidPass ? "0" : "non-zero", guidPass);
        }

        private static CounterWindowAndPressureCatalogInput CreateFixture(
            bool reverseInputOrder = false,
            FixtureMutation mutation = FixtureMutation.None)
        {
            int shieldWeight = mutation == FixtureMutation.PressureWeight ? 6900 : 7000;
            int castWeight = 10000 - shieldWeight;
            int threshold = mutation == FixtureMutation.RequirementThreshold ? 5200 : 5000;
            string pressureLabel = mutation == FixtureMutation.PressurePlayer
                ? "enemy.pressure.fixture.shield.changed"
                : "enemy.pressure.fixture.shield";
            string windowCue = mutation == FixtureMutation.WindowPlayer
                ? "enemy.window.fixture.shell.open.changed"
                : "enemy.window.fixture.shell.open";
            string interruptReference = mutation == FixtureMutation.Condition ? SkillGuard : SkillPressure;
            int shellDuration = mutation == FixtureMutation.Duration ? 1700 : 1500;

            List<BuildPressureProfileSnapshot> pressures = new List<BuildPressureProfileSnapshot>
            {
                Pressure(PressureShield,
                    new[] { new PressureChannelContributionSnapshot("pressure.shield", shieldWeight),
                        new PressureChannelContributionSnapshot("pressure.cast_interrupt", castWeight) },
                    new[] {
                        Group("group.shield.required.any", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                            new[] { Requirement("capability.break_power", threshold), Requirement("capability.thunder_chain", 4500) }),
                        Group("group.shield.recommended", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            new[] { Requirement("capability.burst_window", 3500) }) },
                    labelKey: pressureLabel,
                    sourceIds: mutation == FixtureMutation.Developer
                        ? new[] { "dev_source_fixture_shield", "dev_source_fixture_mutated" }
                        : new[] { "dev_source_fixture_shield" }),
                Pressure(PressureCast, OneContribution("pressure.cast_interrupt"),
                    new[] { Group("group.cast.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        new[] { Requirement("capability.control_power", 4000), Requirement("capability.interrupt_timing", 5500) }) },
                    hintKeys: new[] { "player_hint.cast_interrupt_opportunity" }),
                Pressure(PressureStatus, OneContribution("pressure.status_damage"),
                    new[] {
                        Group("group.status.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                            new[] { Requirement("capability.cleanse_power", 5000), Requirement("capability.debuff_counter", 4000) }),
                        Group("group.status.recommended", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            new[] { Requirement("capability.guard_power", 3000) }) },
                    hintKeys: new[] { "player_hint.status_pressure" }),
                Pressure(PressureFormation,
                    new[] { new PressureChannelContributionSnapshot("pressure.placement_formation", 6000),
                        new PressureChannelContributionSnapshot("pressure.resource_disruption", 4000) },
                    new[] {
                        Group("group.formation.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                            new[] { Requirement("capability.energy_stability", 5000), Requirement("capability.placement_shape", 4500) }),
                        Group("group.formation.recommended", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            new[] { Requirement("capability.control_power", 3000) }) },
                    hintKeys: new[] { "player_hint.formation_disrupted", "player_hint.energy_disrupted" })
            };

            List<CounterWindowProfileSnapshot> windows = new List<CounterWindowProfileSnapshot>
            {
                Window(WindowShell, "counter_window.shell_break",
                    new[] { new CounterWindowConditionSnapshot("condition.shell.signal", CounterWindowConditionKind.MechanicSignal, "signal.fixture.shell_broken") },
                    new[] { new CounterWindowConditionSnapshot("condition.shell.skill_completed", CounterWindowConditionKind.SkillPatternCompleted, SkillCore) },
                    shellDuration,
                    openCueKey: windowCue),
                Window(WindowInterrupt, "counter_window.interrupt_stagger",
                    new[] { new CounterWindowConditionSnapshot("condition.interrupt.skill", CounterWindowConditionKind.SkillPatternInterrupted, interruptReference) },
                    Array.Empty<CounterWindowConditionSnapshot>(), 1200),
                Window(WindowPhase, "counter_window.energy_counter_full_array",
                    new[] { new CounterWindowConditionSnapshot("condition.phase.entered", CounterWindowConditionKind.BossPhaseEntered, PhasePressure) },
                    new[] { new CounterWindowConditionSnapshot("condition.phase.exited", CounterWindowConditionKind.BossPhaseExited, PhasePressure) },
                    0)
            };

            List<PressureSourceBindingSnapshot> pressureSources = new List<PressureSourceBindingSnapshot>
            {
                new PressureSourceBindingSnapshot(PressureSourceKind.MechanicProfile, "dev_enemy_shield_problem", PressureShield),
                new PressureSourceBindingSnapshot(PressureSourceKind.MapRule, "dev_map_black_furnace_smoke", PressureShield),
                new PressureSourceBindingSnapshot(PressureSourceKind.SkillPattern, SkillPressure, PressureCast),
                new PressureSourceBindingSnapshot(PressureSourceKind.BossPhase, PhasePressure, PressureCast),
                new PressureSourceBindingSnapshot(PressureSourceKind.MechanicProfile, EnemyProfileId, PressureCast),
                new PressureSourceBindingSnapshot(PressureSourceKind.MechanicProfile, EnemyProfileId, PressureStatus)
            };
            List<CounterWindowSourceBindingSnapshot> windowSources = new List<CounterWindowSourceBindingSnapshot>
            {
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.MechanicProfile, "dev_enemy_shield_problem", WindowShell),
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.MapRule, "dev_map_black_furnace_smoke", WindowShell),
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.SkillPattern, SkillPressure, WindowInterrupt),
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.BossPhase, PhasePressure, WindowPhase)
            };
            List<PressureCounterWindowBindingSnapshot> pressureWindows = new List<PressureCounterWindowBindingSnapshot>
            {
                new PressureCounterWindowBindingSnapshot(PressureShield, WindowShell),
                new PressureCounterWindowBindingSnapshot(PressureShield, WindowInterrupt),
                new PressureCounterWindowBindingSnapshot(PressureCast, WindowInterrupt),
                new PressureCounterWindowBindingSnapshot(PressureCast, WindowPhase),
                mutation == FixtureMutation.Binding
                    ? new PressureCounterWindowBindingSnapshot(PressureStatus, WindowPhase)
                    : new PressureCounterWindowBindingSnapshot(PressureFormation, WindowPhase)
            };

            if (reverseInputOrder)
            {
                pressures.Reverse();
                windows.Reverse();
                pressureSources.Reverse();
                windowSources.Reverse();
                pressureWindows.Reverse();
            }

            return new CounterWindowAndPressureCatalogInput(
                pressures, windows, pressureSources, windowSources, pressureWindows);
        }

        private static BuildPressureProfileSnapshot Pressure(
            string id,
            IEnumerable<PressureChannelContributionSnapshot> contributions,
            IEnumerable<BuildCapabilityRequirementGroupSnapshot> groups,
            string playerId = null,
            IEnumerable<string> hintKeys = null,
            IEnumerable<string> diagnosticKeys = null,
            IEnumerable<string> sourceIds = null,
            string labelKey = "enemy.pressure.fixture.label",
            bool devOnly = true)
        {
            return new BuildPressureProfileSnapshot(
                id,
                new BuildPressurePlayerProjection(
                    playerId ?? id,
                    labelKey,
                    "enemy.pressure.fixture.hint",
                    hintKeys ?? new[] { "player_hint.shield_pressure" }),
                new BuildPressureInternalSpec(contributions),
                new BuildPressureDeveloperDiagnostics(
                    groups,
                    diagnosticKeys ?? new[] { DiagnosticKey },
                    sourceIds ?? new[] { "dev_source_fixture" }),
                devOnly,
                false,
                false);
        }

        private static CounterWindowProfileSnapshot Window(
            string id,
            string typeKey,
            IEnumerable<CounterWindowConditionSnapshot> open,
            IEnumerable<CounterWindowConditionSnapshot> close,
            int duration,
            string playerId = null,
            string openCueKey = "enemy.window.fixture.open",
            bool referenceEnabled = false)
        {
            return new CounterWindowProfileSnapshot(
                new CounterWindowReference(id, true, referenceEnabled, false),
                new CounterWindowPlayerProjection(
                    playerId ?? id,
                    "enemy.window.fixture.label",
                    openCueKey,
                    "enemy.window.fixture.close"),
                new CounterWindowInternalSpec(
                    typeKey,
                    RequirementMatchMode.Any,
                    open,
                    RequirementMatchMode.Any,
                    close,
                    duration),
                new CounterWindowDeveloperDiagnostics(
                    new[] { DiagnosticKey },
                    new[] { "dev_source_fixture" }));
        }

        private static BuildCapabilityRequirementGroupSnapshot[] DefaultGroups()
        {
            return new[]
            {
                Group("group.default", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                    new[] { Requirement("capability.break_power", 5000) })
            };
        }

        private static PressureChannelContributionSnapshot[] OneContribution(string key)
        {
            return new[] { new PressureChannelContributionSnapshot(key, 10000) };
        }

        private static CounterWindowConditionSnapshot[] DefaultOpen()
        {
            return new[] { new CounterWindowConditionSnapshot(
                "condition.default",
                CounterWindowConditionKind.MechanicSignal,
                "signal.fixture") };
        }

        private static BuildCapabilityRequirementGroupSnapshot Group(
            string id,
            CapabilityRequirementRole role,
            RequirementMatchMode mode,
            IEnumerable<BuildCapabilityRequirementSnapshot> requirements)
        {
            return new BuildCapabilityRequirementGroupSnapshot(id, role, mode, requirements);
        }

        private static BuildCapabilityRequirementSnapshot Requirement(string key, int threshold)
        {
            return new BuildCapabilityRequirementSnapshot(key, threshold);
        }

        private static CounterWindowAndPressureCatalogInput Replace(
            CounterWindowAndPressureCatalogInput source,
            IEnumerable<BuildPressureProfileSnapshot> pressures = null,
            IEnumerable<CounterWindowProfileSnapshot> windows = null,
            IEnumerable<PressureSourceBindingSnapshot> pressureSources = null,
            IEnumerable<CounterWindowSourceBindingSnapshot> windowSources = null,
            IEnumerable<PressureCounterWindowBindingSnapshot> pressureWindows = null)
        {
            return new CounterWindowAndPressureCatalogInput(
                pressures ?? source.BuildPressureProfiles,
                windows ?? source.CounterWindowProfiles,
                pressureSources ?? source.PressureSourceBindings,
                windowSources ?? source.CounterWindowSourceBindings,
                pressureWindows ?? source.PressureCounterWindowBindings,
                source.SchemaId,
                source.SchemaVersion);
        }

        private static EnemySkillBossPhaseCatalogInput CreateSkillPhaseFixture()
        {
            EnemySkillPatternSnapshot[] patterns =
            {
                SkillPattern(SkillGuard, EnemyProfileId, SkillCastKind.Instant, 0),
                SkillPattern(SkillPressure, EnemyProfileId, SkillCastKind.Channeled, 1200),
                SkillPattern(SkillPulse, BossProfileId, SkillCastKind.Instant, 0),
                SkillPattern(SkillCore, BossProfileId, SkillCastKind.Channeled, 1600)
            };
            SkillSequenceSnapshot[] sequences =
            {
                Sequence("dev_sequence_fixture_enemy_echo", SkillPressure),
                Sequence("dev_sequence_fixture_enemy_pressure", SkillGuard, SkillPressure),
                Sequence("dev_sequence_fixture_boss_pressure", SkillPulse, SkillCore)
            };
            CarrierSkillBindingSnapshot[] bindings =
            {
                new CarrierSkillBindingSnapshot(EnemySkillCarrierKind.Enemy, EnemyId,
                    new[] { "dev_sequence_fixture_enemy_echo", "dev_sequence_fixture_enemy_pressure" }),
                new CarrierSkillBindingSnapshot(EnemySkillCarrierKind.Boss, BossIdA,
                    new[] { "dev_sequence_fixture_boss_pressure" }),
                new CarrierSkillBindingSnapshot(EnemySkillCarrierKind.Boss, BossIdB,
                    new[] { "dev_sequence_fixture_boss_pressure" })
            };
            BossPhaseProfileSnapshot[] phases =
            {
                Phase(PhaseOpening), Phase(PhasePressure), Phase(PhaseSignal)
            };
            BossPhasePlanSnapshot[] plans =
            {
                Plan(BossIdA), Plan(BossIdB)
            };
            return new EnemySkillBossPhaseCatalogInput(patterns, sequences, bindings, phases, plans);
        }

        private static EnemySkillPatternSnapshot SkillPattern(
            string id,
            string mechanicProfileId,
            SkillCastKind kind,
            int duration)
        {
            return new EnemySkillPatternSnapshot(
                new SkillPatternReference(id),
                new SkillPatternPlayerProjection(
                    id, "enemy.skill.fixture.name", "intent.fixture",
                    "enemy.intent.fixture", "enemy.target.fixture", "enemy.cast.fixture",
                    new[] { "player_hint.cast_interrupt_opportunity" }),
                new SkillPatternInternalSpec(kind, duration, 100, new[] { mechanicProfileId }),
                new SkillPatternDeveloperDiagnostics(new[] { DiagnosticKey }, new[] { mechanicProfileId }));
        }

        private static SkillSequenceSnapshot Sequence(string id, params string[] patternIds)
        {
            return new SkillSequenceSnapshot(
                id,
                patternIds.Select((value, index) =>
                    new SkillSequenceStepSnapshot(index, value, index == 0 ? 0 : 100, 1)));
        }

        private static BossPhaseProfileSnapshot Phase(string id)
        {
            return new BossPhaseProfileSnapshot(
                new BossPhaseReference(id),
                new BossPhasePlayerProjection(id, "boss.phase.fixture.label", "boss.phase.fixture.enter"),
                new BossPhaseInternalSpec(new[] { "dev_sequence_fixture_boss_pressure" }, new[] { BossProfileId }),
                new BossPhaseDeveloperDiagnostics(new[] { DiagnosticKey }, new[] { BossProfileId }));
        }

        private static BossPhasePlanSnapshot Plan(string bossId)
        {
            return new BossPhasePlanSnapshot(
                bossId,
                new[]
                {
                    new BossPhasePlanEntrySnapshot(0, PhaseOpening,
                        new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.EncounterStart)),
                    new BossPhasePlanEntrySnapshot(1, PhasePressure,
                        new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.HealthRatioAtOrBelow, 6000)),
                    new BossPhasePlanEntrySnapshot(2, PhaseSignal,
                        new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.MechanicSignal, 0, "signal.fixture.phase"))
                });
        }

        private static void WriteReports(
            string root,
            string mode,
            IReadOnlyList<Check> checks,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            WriteUtf8(Absolute(root, ReportRoot + "CounterWindowAndPressureSchemaReport.md"),
                BuildMainReport(mode, checks, catalog));
            WriteUtf8(Absolute(root, ReportRoot + "CounterWindowAndPressureSchemaSpec.csv"),
                BuildSpecCsv(checks));
            WriteUtf8(Absolute(root, ReportRoot + "CounterWindowAndPressureSchemaFieldMatrix.csv"),
                BuildFieldMatrixCsv());
            WriteUtf8(Absolute(root, ReportRoot + "CounterWindowAndPressureSchemaFixtureRows.csv"),
                BuildFixtureRowsCsv(catalog));
            WriteUtf8(Absolute(root, ReportRoot + "CounterWindowAndPressureSchemaLeakCheckReport.md"),
                BuildLeakReport(checks));
        }

        private static string BuildMainReport(
            string mode,
            IReadOnlyList<Check> checks,
            CounterWindowAndPressureCatalogSnapshot catalog)
        {
            int groups = catalog.BuildPressureProfiles.Sum(value => value.DeveloperOnly.RequirementGroups.Count);
            int requirements = catalog.BuildPressureProfiles.SelectMany(value => value.DeveloperOnly.RequirementGroups)
                .Sum(value => value.Requirements.Count);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Counter Window And Pressure Schema Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-CounterWindowAndPressureSchema01`")
                .AppendLine("- Schema: `" + catalog.SchemaId + "` / `" + catalog.SchemaVersion + "`")
                .AppendLine("- Mode: `" + Markdown(mode) + "`")
                .AppendLine("- Result: `" + (checks.Count > 0 && checks.All(value => value.Passed) ? "PASS" : "FAIL") + "`")
                .AppendLine("- Verifier: `" + checks.Count(value => value.Passed) + "/" + checks.Count + "`")
                .AppendLine("- BuildPressureProfile: `" + catalog.BuildPressureProfiles.Count + "`")
                .AppendLine("- CounterWindowProfile: `" + catalog.CounterWindowProfiles.Count + "`")
                .AppendLine("- RequirementGroup / Requirement: `" + groups + " / " + requirements + "`")
                .AppendLine("- PressureSource / WindowSource / PressureWindow bindings: `"
                    + catalog.PressureSourceBindings.Count + " / " + catalog.CounterWindowSourceBindings.Count
                    + " / " + catalog.PressureCounterWindowBindings.Count + "`")
                .AppendLine("- CanonicalSignature: `" + catalog.CanonicalSignature + "`")
                .AppendLine("- PlayerSafeCanonicalSignature: `" + catalog.PlayerSafeCanonicalSignature + "`")
                .AppendLine()
                .AppendLine("## Required Properties")
                .AppendLine()
                .AppendLine("- Pressure reuse: `" + CheckResult(checks, "reuse.pressureBySources") + "`")
                .AppendLine("- CounterWindow reuse: `" + CheckResult(checks, "reuse.windowBySources") + "`")
                .AppendLine("- Any / All / Recommended groups: `"
                    + CheckResult(checks, "requirements.anyRequired") + " / "
                    + CheckResult(checks, "requirements.allRequired") + " / "
                    + CheckResult(checks, "requirements.recommended") + "`")
                .AppendLine("- Player-safe isolation: `" + CheckResult(checks, "leak.playerSafe") + "`")
                .AppendLine("- Protected E01-E05 hashes: `" + CheckPrefixResult(checks, "protected.hash.") + "`")
                .AppendLine("- Legacy hashes: `" + CheckPrefixResult(checks, "legacy.hash.") + "`")
                .AppendLine()
                .AppendLine("This package defines immutable data specifications only. It does not execute windows, consume time, subscribe to combat events, read a real Build, or connect formal flow.");
            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<Check> checks)
        {
            List<string[]> rows = new List<string[]> { new[] { "checkId", "expected", "actual", "status" } };
            rows.AddRange(checks.OrderBy(value => value.Id, StringComparer.Ordinal)
                .Select(value => new[] { value.Id, value.Expected, value.Actual, value.Passed ? "PASS" : "FAIL" }));
            return Csv(rows);
        }

        private static string BuildFieldMatrixCsv()
        {
            List<string[]> rows = new List<string[]>
            {
                new[] { "ownerType", "fieldName", "fieldType", "cardinality", "semantic", "visibility", "sourceOfTruth", "futureConsumer" },
                FieldRow("Catalog", "schemaId", "string", "1", "Schema identity", "public metadata", "E06", "Validator"),
                FieldRow("Catalog", "schemaVersion", "int", "1", "Schema version", "public metadata", "E06", "Validator"),
                FieldRow("BuildPressurePlayerProjection", "BuildPressureProfileId", "string", "1", "Public pressure identity", "player-safe", "E06", "Battle projection"),
                FieldRow("BuildPressurePlayerProjection", "PublicPressureLabelKey", "string", "1", "Public pressure label", "player-safe", "E06", "Battle projection"),
                FieldRow("BuildPressurePlayerProjection", "PublicPressureHintKey", "string", "1", "Public pressure hint", "player-safe", "E06", "Battle projection"),
                FieldRow("BuildPressurePlayerProjection", "PlayerHintCategoryKeys", "string[]", "0..N", "Public hint categories", "player-safe", "E02 keys / E06 refs", "Battle projection"),
                FieldRow("BuildPressureInternalSpec", "PressureChannelContributions", "PressureChannelContribution[]", "1..N", "Pressure composition", "internal-only", "E06", "Future readiness"),
                FieldRow("PressureChannelContribution", "PressureChannelKey", "string", "1", "Abstract pressure channel", "internal-only", "E02 key", "Future readiness"),
                FieldRow("PressureChannelContribution", "WeightBasisPoints", "int", "1", "Composition weight totaling 10000", "internal-only", "E06", "Future readiness"),
                FieldRow("BuildPressureDeveloperDiagnostics", "RequirementGroups", "RequirementGroup[]", "1..N", "Required/recommended multi-solution groups", "developer-only", "E06", "E08"),
                FieldRow("RequirementGroup", "RequirementRole", "enum", "1", "Required or Recommended", "developer-only", "E06", "E08"),
                FieldRow("RequirementGroup", "RequirementMatchMode", "enum", "1", "Any or All", "developer-only", "E06", "E08"),
                FieldRow("BuildCapabilityRequirement", "BuildCapabilityKey", "string", "1", "Abstract Build capability", "developer-only", "E02 key", "E07/E08"),
                FieldRow("BuildCapabilityRequirement", "MinimumCapabilityBasisPoints", "int", "1", "Capability threshold", "developer-only", "E06", "E08"),
                FieldRow("CounterWindowPlayerProjection", "CounterWindowId", "string", "1", "Public window identity", "player-safe", "E01 reference", "Battle projection"),
                FieldRow("CounterWindowPlayerProjection", "PublicWindowLabelKey", "string", "1", "Public window label", "player-safe", "E06", "Battle projection"),
                FieldRow("CounterWindowPlayerProjection", "PublicWindowOpenCueKey", "string", "1", "Public open cue", "player-safe", "E06", "Battle projection"),
                FieldRow("CounterWindowPlayerProjection", "PublicWindowCloseCueKey", "string", "1", "Public close cue", "player-safe", "E06", "Battle projection"),
                FieldRow("CounterWindowInternalSpec", "CounterWindowTypeKey", "string", "1", "Abstract window type", "internal-only", "E02 key", "Future runtime"),
                FieldRow("CounterWindowInternalSpec", "OpenConditions", "Condition[]", "1..N", "Open specification", "internal-only", "E06 refs E05", "Future runtime"),
                FieldRow("CounterWindowInternalSpec", "CloseConditions", "Condition[]", "0..N", "Close specification", "internal-only", "E06 refs E05", "Future runtime"),
                FieldRow("CounterWindowInternalSpec", "MaximumDurationMilliseconds", "int", "1", "Optional maximum duration", "internal-only", "E06", "Future runtime"),
                FieldRow("PressureSourceBinding", "SourceKind/SourceId", "enum/string", "1", "E03/E05 source reference", "internal-only", "E03/E05", "Catalog resolver"),
                FieldRow("CounterWindowSourceBinding", "SourceKind/SourceId", "enum/string", "1", "E03/E05 source reference", "internal-only", "E03/E05", "Catalog resolver"),
                FieldRow("PressureCounterWindowBinding", "BuildPressureProfileId/CounterWindowId", "string/string", "1", "Many-to-many pressure/window relation", "internal-only", "E06", "Future runtime")
            };
            return Csv(rows);
        }

        private static string BuildFixtureRowsCsv(CounterWindowAndPressureCatalogSnapshot catalog)
        {
            List<string[]> rows = new List<string[]>
            {
                new[] { "rowKind", "ownerId", "sourceKind", "sourceId", "referenceId", "role", "matchMode", "valueBasisPoints", "conditionKind", "durationMilliseconds", "playerVisible", "fixtureOnly" }
            };
            foreach (BuildPressureProfileSnapshot pressure in catalog.BuildPressureProfiles)
            {
                rows.Add(FixtureRow("BuildPressureProfile", pressure.BuildPressureProfileId, "", "", pressure.PlayerSafe.PublicPressureLabelKey,
                    "", "", "", "", "", "true", "true"));
                foreach (PressureChannelContributionSnapshot contribution in pressure.InternalOnly.PressureChannelContributions)
                {
                    rows.Add(FixtureRow("PressureChannelContribution", pressure.BuildPressureProfileId, "", "", contribution.PressureChannelKey,
                        "", "", contribution.WeightBasisPoints.ToString(CultureInfo.InvariantCulture), "", "", "false", "true"));
                }
                foreach (BuildCapabilityRequirementGroupSnapshot group in pressure.DeveloperOnly.RequirementGroups)
                {
                    foreach (BuildCapabilityRequirementSnapshot requirement in group.Requirements)
                    {
                        rows.Add(FixtureRow("BuildCapabilityRequirement", pressure.BuildPressureProfileId, "", group.RequirementGroupId,
                            requirement.BuildCapabilityKey, group.RequirementRole.ToString(), group.RequirementMatchMode.ToString(),
                            requirement.MinimumCapabilityBasisPoints.ToString(CultureInfo.InvariantCulture), "", "", "false", "true"));
                    }
                }
            }
            foreach (CounterWindowProfileSnapshot window in catalog.CounterWindowProfiles)
            {
                rows.Add(FixtureRow("CounterWindowProfile", window.CounterWindowId, "", "", window.PlayerSafe.PublicWindowLabelKey,
                    "", "", "", "", window.InternalOnly.MaximumDurationMilliseconds.ToString(CultureInfo.InvariantCulture), "true", "true"));
                foreach (CounterWindowConditionSnapshot condition in window.InternalOnly.OpenConditions)
                {
                    rows.Add(FixtureRow("CounterWindowOpenCondition", window.CounterWindowId, "", condition.ConditionId,
                        condition.ReferenceId, "", window.InternalOnly.OpenConditionMatchMode.ToString(), "", condition.Kind.ToString(),
                        window.InternalOnly.MaximumDurationMilliseconds.ToString(CultureInfo.InvariantCulture), "false", "true"));
                }
                foreach (CounterWindowConditionSnapshot condition in window.InternalOnly.CloseConditions)
                {
                    rows.Add(FixtureRow("CounterWindowCloseCondition", window.CounterWindowId, "", condition.ConditionId,
                        condition.ReferenceId, "", window.InternalOnly.CloseConditionMatchMode.ToString(), "", condition.Kind.ToString(),
                        window.InternalOnly.MaximumDurationMilliseconds.ToString(CultureInfo.InvariantCulture), "false", "true"));
                }
            }
            rows.AddRange(catalog.PressureSourceBindings.Select(value => FixtureRow("PressureSourceBinding",
                value.BuildPressureProfileId, value.SourceKind.ToString(), value.SourceId, value.BuildPressureProfileId,
                "", "", "", "", "", "false", "true")));
            rows.AddRange(catalog.CounterWindowSourceBindings.Select(value => FixtureRow("CounterWindowSourceBinding",
                value.CounterWindowId, value.SourceKind.ToString(), value.SourceId, value.CounterWindowId,
                "", "", "", "", "", "false", "true")));
            rows.AddRange(catalog.PressureCounterWindowBindings.Select(value => FixtureRow("PressureCounterWindowBinding",
                value.BuildPressureProfileId, "", "", value.CounterWindowId,
                "", "", "", "", "", "false", "true")));
            return Csv(rows);
        }

        private static string BuildLeakReport(IReadOnlyList<Check> checks)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Counter Window And Pressure Schema Leak Check")
                .AppendLine()
                .AppendLine("Every forbidden dependency is listed separately; no aggregate-only assertion is used.")
                .AppendLine()
                .AppendLine("## Runtime forbidden dependency scan")
                .AppendLine();
            foreach (Check check in checks.Where(value => value.Id.StartsWith("leak.runtime.", StringComparison.Ordinal))
                .OrderBy(value => value.Id, StringComparer.Ordinal))
            {
                builder.AppendLine("- `" + check.Id + "`: `" + (check.Passed ? "PASS" : "FAIL")
                    + "` (actual `" + Markdown(check.Actual) + "`)");
            }
            builder.AppendLine()
                .AppendLine("## Isolation and protected-source scan")
                .AppendLine();
            foreach (Check check in checks.Where(value =>
                    value.Id.StartsWith("leak.", StringComparison.Ordinal)
                    || value.Id.StartsWith("protected.hash.", StringComparison.Ordinal)
                    || value.Id.StartsWith("legacy.hash.", StringComparison.Ordinal)
                    || value.Id.StartsWith("package.guid", StringComparison.Ordinal)
                    || value.Id == "package.trailingWhitespace")
                .OrderBy(value => value.Id, StringComparer.Ordinal))
            {
                builder.AppendLine("- `" + check.Id + "`: `" + (check.Passed ? "PASS" : "FAIL")
                    + "` (expected `" + Markdown(check.Expected) + "`, actual `" + Markdown(check.Actual) + "`)");
            }
            return builder.ToString();
        }

        private static void AddFullOnlyMutation(
            List<Check> checks,
            E02E03E05Resolver resolver,
            CounterWindowAndPressureCatalogSnapshot baseline,
            FixtureMutation mutation,
            string name)
        {
            CounterWindowAndPressureCatalogSnapshot changed = Snapshot(CreateFixture(false, mutation), resolver);
            Add(checks, "signature." + name + ".fullChanges", "different", changed.CanonicalSignature,
                !string.Equals(baseline.CanonicalSignature, changed.CanonicalSignature, StringComparison.Ordinal));
            Add(checks, "signature." + name + ".playerStable", baseline.PlayerSafeCanonicalSignature,
                changed.PlayerSafeCanonicalSignature,
                string.Equals(baseline.PlayerSafeCanonicalSignature, changed.PlayerSafeCanonicalSignature, StringComparison.Ordinal));
        }

        private static void AddBothMutation(
            List<Check> checks,
            E02E03E05Resolver resolver,
            CounterWindowAndPressureCatalogSnapshot baseline,
            FixtureMutation mutation,
            string name)
        {
            CounterWindowAndPressureCatalogSnapshot changed = Snapshot(CreateFixture(false, mutation), resolver);
            Add(checks, "signature." + name + ".fullChanges", "different", changed.CanonicalSignature,
                !string.Equals(baseline.CanonicalSignature, changed.CanonicalSignature, StringComparison.Ordinal));
            Add(checks, "signature." + name + ".playerChanges", "different", changed.PlayerSafeCanonicalSignature,
                !string.Equals(baseline.PlayerSafeCanonicalSignature, changed.PlayerSafeCanonicalSignature, StringComparison.Ordinal));
        }

        private static void ExpectCode(
            List<Check> checks,
            string id,
            string code,
            CounterWindowAndPressureCatalogInput input,
            ICounterWindowAndPressureReferenceResolver resolver)
        {
            IReadOnlyList<CounterWindowAndPressureValidationIssue> issues =
                DefaultCounterWindowAndPressureValidator.Instance.Validate(input, resolver);
            string actual = string.Join(";", issues.Select(value => value.Code).Distinct(StringComparer.Ordinal));
            Add(checks, id, code, actual, issues.Any(value => string.Equals(value.Code, code, StringComparison.Ordinal)));
        }

        private static CounterWindowAndPressureCatalogSnapshot Snapshot(
            CounterWindowAndPressureCatalogInput input,
            ICounterWindowAndPressureReferenceResolver resolver)
        {
            return DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(input, resolver);
        }

        private static void AddReadOnlyCheck(List<Check> checks, string id, object value)
        {
            IList list = value as IList;
            bool readOnly = list != null && list.IsReadOnly;
            bool throws = false;
            if (list != null)
            {
                try { list.Add(null); }
                catch (NotSupportedException) { throws = true; }
            }
            Add(checks, id, "read-only and mutation rejected", "readOnly=" + readOnly + ",throws=" + throws,
                readOnly && throws);
        }

        private static bool IsCanonicalSignature(string value)
        {
            if (value == null || value.Length != 71 || !value.StartsWith("sha256:", StringComparison.Ordinal))
            {
                return false;
            }
            return value.Substring(7).All(character =>
                (character >= '0' && character <= '9') || (character >= 'a' && character <= 'f'));
        }

        private static void AddHashChecks(
            List<Check> checks,
            string root,
            string prefix,
            IReadOnlyDictionary<string, string> hashes)
        {
            foreach (KeyValuePair<string, string> pair in hashes.OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                string path = Absolute(root, pair.Key);
                string actual = File.Exists(path) ? Sha256(path) : "MISSING";
                Add(checks, prefix + ".hash." + SafeId(pair.Key), pair.Value, actual,
                    string.Equals(pair.Value, actual, StringComparison.Ordinal));
            }
        }

        private static string Sha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return string.Concat(sha.ComputeHash(stream)
                    .Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
            }
        }

        private static int Count(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token)) return 0;
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static int CountIgnoreCase(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token)) return 0;
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(token, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static int CountTrailingWhitespace(string path)
        {
            return File.ReadAllLines(path).Count(line => line.EndsWith(" ", StringComparison.Ordinal)
                || line.EndsWith("\t", StringComparison.Ordinal));
        }

        private static string ReadMetaGuid(string path)
        {
            if (!File.Exists(path)) return string.Empty;
            string line = File.ReadLines(path).FirstOrDefault(value => value.StartsWith("guid: ", StringComparison.Ordinal));
            return line == null ? string.Empty : line.Substring("guid: ".Length).Trim();
        }

        private static string CsvColumn(string line, int index)
        {
            string[] cells = (line ?? string.Empty).Split(',');
            return index < cells.Length ? cells[index].Trim().Trim('"') : string.Empty;
        }

        private static string[] FieldRow(
            string owner, string name, string type, string cardinality,
            string semantic, string visibility, string source, string consumer)
        {
            return new[] { owner, name, type, cardinality, semantic, visibility, source, consumer };
        }

        private static string[] FixtureRow(
            string rowKind, string ownerId, string sourceKind, string sourceId,
            string referenceId, string role, string matchMode, string valueBasisPoints,
            string conditionKind, string durationMilliseconds, string playerVisible,
            string fixtureOnly)
        {
            return new[] { rowKind, ownerId, sourceKind, sourceId, referenceId, role,
                matchMode, valueBasisPoints, conditionKind, durationMilliseconds,
                playerVisible, fixtureOnly };
        }

        private static string Csv(IEnumerable<string[]> rows)
        {
            return string.Join("\n", rows.Select(row => string.Join(",", row.Select(CsvCell)))) + "\n";
        }

        private static string CsvCell(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static string CheckResult(IReadOnlyList<Check> checks, string id)
        {
            Check check = checks.LastOrDefault(value => string.Equals(value.Id, id, StringComparison.Ordinal));
            return check != null && check.Passed ? "PASS" : "FAIL";
        }

        private static string CheckPrefixResult(IReadOnlyList<Check> checks, string prefix)
        {
            Check[] selected = checks.Where(value => value.Id.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
            return selected.Length > 0 && selected.All(value => value.Passed)
                ? selected.Length + "/" + selected.Length + " PASS"
                : selected.Count(value => value.Passed) + "/" + selected.Length + " PASS";
        }

        private static string Markdown(string value)
        {
            return (value ?? string.Empty).Replace("`", "'").Replace("\r", " ").Replace("\n", " ");
        }

        private static string SafeId(string value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char character in value ?? string.Empty)
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : '_');
            }
            return builder.ToString();
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(directory.FullName, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(directory.FullName, "Packages")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static bool IsBatchMode()
        {
#if UNITY_EDITOR
            return Application.isBatchMode;
#else
            return false;
#endif
        }

        private static void Add(
            ICollection<Check> checks,
            string id,
            string expected,
            string actual,
            bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id ?? string.Empty;
                Expected = expected ?? string.Empty;
                Actual = actual ?? string.Empty;
                Passed = passed;
            }
            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private sealed class E02E03Resolver : IEnemySkillBossPhaseReferenceResolver
        {
            private readonly EnemyValidationContentSnapshot normalization;
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly HashSet<string> carrierBindings;

            public E02E03Resolver(
                EnemyValidationContentSnapshot normalization,
                EnemyMechanicVocabularySnapshot vocabulary)
            {
                this.normalization = normalization ?? throw new ArgumentNullException(nameof(normalization));
                this.vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
                carrierBindings = new HashSet<string>(
                    normalization.CarrierMechanicBindings.Select(value =>
                        CarrierIdentity(
                            value.Kind == ValidationCarrierKind.Enemy
                                ? EnemySkillCarrierKind.Enemy
                                : EnemySkillCarrierKind.Boss,
                            value.CarrierId,
                            value.MechanicProfileId)),
                    StringComparer.Ordinal);
            }

            public bool TryGetEnemy(string stableId, out EnemyArchetypeSnapshot enemy)
            {
                if (normalization.TryGetEnemy(stableId, out NormalizedEnemyCarrierSnapshot value))
                {
                    enemy = value.Domain;
                    return true;
                }
                enemy = null;
                return false;
            }

            public bool TryGetBoss(string stableId, out BossArchetypeSnapshot boss)
            {
                if (normalization.TryGetBoss(stableId, out NormalizedBossCarrierSnapshot value))
                {
                    boss = value.Domain;
                    return true;
                }
                boss = null;
                return false;
            }

            public bool TryGetMechanicProfileKind(string stableId, out ValidationProfileKind kind)
            {
                if (normalization.TryGetMechanicProfile(stableId, out NormalizedMechanicProfileSnapshot value))
                {
                    kind = value.Kind;
                    return true;
                }
                kind = default(ValidationProfileKind);
                return false;
            }

            public bool HasCarrierMechanicBinding(
                EnemySkillCarrierKind kind,
                string carrierId,
                string mechanicProfileId)
            {
                return carrierBindings.Contains(CarrierIdentity(kind, carrierId, mechanicProfileId));
            }

            public bool HasVocabularyKey(EnemyVocabularyCategory category, string stableKey)
            {
                return vocabulary.TryGetEntry(category, stableKey, out _);
            }

            private static string CarrierIdentity(
                EnemySkillCarrierKind kind,
                string carrierId,
                string mechanicProfileId)
            {
                return kind + "\u001f" + (carrierId ?? string.Empty)
                    + "\u001f" + (mechanicProfileId ?? string.Empty);
            }
        }

        private sealed class E02E03E05Resolver :
            ICounterWindowAndPressureReferenceResolver
        {
            private readonly EnemyValidationContentSnapshot normalization;
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly EnemySkillBossPhaseCatalogSnapshot skillPhase;

            public E02E03E05Resolver(
                EnemyValidationContentSnapshot normalization,
                EnemyMechanicVocabularySnapshot vocabulary,
                EnemySkillBossPhaseCatalogSnapshot skillPhase)
            {
                this.normalization = normalization ?? throw new ArgumentNullException(nameof(normalization));
                this.vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
                this.skillPhase = skillPhase ?? throw new ArgumentNullException(nameof(skillPhase));
            }

            public bool HasVocabularyKey(EnemyVocabularyCategory category, string stableKey)
            {
                return vocabulary.TryGetEntry(category, stableKey, out _);
            }

            public bool HasMechanicProfile(string mechanicProfileId)
            {
                return normalization.TryGetMechanicProfile(mechanicProfileId, out _);
            }

            public bool HasMapRule(string mapRuleId)
            {
                return normalization.TryGetMapRule(mapRuleId, out _);
            }

            public bool HasSkillPattern(string skillPatternId)
            {
                return skillPhase.TryGetSkillPatternById(skillPatternId, out _);
            }

            public bool HasBossPhase(string bossPhaseId)
            {
                return skillPhase.TryGetBossPhaseById(bossPhaseId, out _);
            }
        }
    }
}
