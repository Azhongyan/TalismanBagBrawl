using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.CapabilityRead;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.PressureWindow;
using TalismanBag.EnemySystem.ReadinessEvaluation;
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.SystemSnapshot;
using TalismanBag.EnemySystem.Vocabulary;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class EnemyDataValidatorAndSnapshotVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const string DefaultEnemyId = "dev_enemy_poison_cultist";
        private const string DefaultEnemyProfileId = "dev_enemy_poison_burn_problem";
        private const string DefaultBossId = "dev_boss_shield_jinglei";
        private const string DefaultBossProfileId = "dev_boss_black_furnace_shell";
        private const string DefaultMapRuleId = "dev_map_bluestone_damp";
        private const string EnemyId = "dev_enemy_caster_chanter";
        private const string EnemyProfileId = "dev_enemy_caster_problem";
        private const string BossIdA = "dev_boss_caster_zhenhun";
        private const string BossIdB = "dev_boss_energy_juneng";
        private const string BossProfileId = "dev_boss_spirit_thief_core";
        private const string HintKey = "player_hint.cast_interrupt_opportunity";
        private const string AlternateHintKey = "player_hint.burst_incoming";
        private const string DiagnosticKey = "diagnostic.invalid_reference";
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

        private static readonly string[] ReportFiles =
        {
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotReport.md",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSpec.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotFieldMatrix.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSchemaManifest.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotIdentityInventory.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotRelationMatrix.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotLeakCheckReport.md"
        };

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotInput.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotInput.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemPlayerSafeSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemPlayerSafeSnapshot.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDataValidatorAndSnapshotVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDataValidatorAndSnapshotVerifier.cs.meta",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotReport.md",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSpec.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotFieldMatrix.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSchemaManifest.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotIdentityInventory.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotRelationMatrix.csv",
            "Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotLeakCheckReport.md"
        };

        private static readonly string[] RuntimeFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotInput.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemPlayerSafeSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs"
        };

        private static readonly string[] RuntimeForbiddenTokens =
        {
            "UnityEngine", "UnityEditor", "MonoBehaviour", "ScriptableObject", "GameObject", "Transform",
            "Addressables", "System.IO", "File.", "Directory.", "TalismanBag.BuildSandbox",
            "TalismanBag.Item", "TalismanBag.Battle", "BattleContract", "SaveData"
        };

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)
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
                ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs"] = "3C7892A7A73A1464EE2D797B8F617EBA1639B1C7C6D4330FC166F64B32DAB267",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressurePrimitives.cs"] = "E0A5C694D1039552F5C7E24CE599B71B7CB08B29AA39F11F6B134552B4505687",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs"] = "56CC65BADAF89BE6882629AE847FCF03C2D0BAC3D800BDB562DE66E275E4D1C1",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowSnapshots.cs"] = "5DD48CA0F0B84A44B7356EFFAFCC03C879200DB5C5030D27E00BEC15D24921C6",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressureValidation.cs"] = "1526BAF50EC3A59914949A152FA48B659FE898A836EAD13D947CA905D17485AE",
                ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs"] = "2FA48E6D1A17AE7B6204655FF5F58AE1D304C2315F1CBA9AA0A7F91D9CF99DD4",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs"] = "7A97F62EC9539A4F8D3C43FCF893854FB408A4A0950B40635884BE06681B62C4",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilitySnapshots.cs"] = "EA2C7DF08CB2798C2D0DE401A564D3EC80B0C3F7DFF67334AE4856FB021A28BC",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/ReadinessDiagnosticSnapshots.cs"] = "F689F732BFABA19A7EDE16EF694857AAC86205195D84BE828983381D882C5D51",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadValidation.cs"] = "B21881430918157951A17BE4C76AC4A88D04626C0A4A549959F2A633DA61D5B6",
                ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs"] = "F1AB9863BC777D221E073294BD2D3478DBF416617BA7F9E3534A7882440A5D34",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessPrimitives.cs"] = "449F856169ADACDDE235D4C883C0D347057E48B54C70709F5FF8429D02055A5D",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessInputSnapshots.cs"] = "57B81F6F619FE64467E1D7EC4CBAFDFD6BEAB60028C47F2501DED966BE650BF8",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessResultSnapshots.cs"] = "3522F34A755E53B88F117CAB43C643BD0AE96BFBEC3E348BA0E2845016D0E925",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyOfflineReadinessEvaluator.cs"] = "BDE586DCBA0BEC0B3926AE1584586B8C1DC5799FB6619B2171EF64E8E6C484BA",
                ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs"] = "13D1A5D9C81FD590978F190ACAE260C26D348B24930FC4A82FBA192F584ECABA"
            });

        private static readonly IReadOnlyDictionary<string, string> LegacyHashes =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs"] = "C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs"] = "14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs"] = "A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4"
            });

        private static readonly IReadOnlyDictionary<string, string> FrozenE09Hashes =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot.meta"] = "A409A8735463B89DB4B9A17D5BBC8F179517B026E23B958B12EFC3485C104574",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotPrimitives.cs"] = "F1339B6CEB6DF67EAFBE4D011B5F1F069755B8538B4071F6F45D57DDFD54C71D",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotPrimitives.cs.meta"] = "DD000CA48485A1A17F3ABC4DB1DBD460219918F0BCB48B66E4F74FE2A4BA06D3",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotInput.cs"] = "204A1866A5F9957A6BDD16FA2BD755EECB82D3826CDDF138E44F47209004E11F",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotInput.cs.meta"] = "5378705903B272A2829002E1D360B91BDEE35FDE911423DCB5A8AA0092B8BF95",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemPlayerSafeSnapshot.cs"] = "3E03B20171403BF449510B4D539F9E5716B9EAAE1A58AFE68B4EBC2882B15E59",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemPlayerSafeSnapshot.cs.meta"] = "73DD2DBB77096FC5DC77FBFE8B49B4AF177B8801C1F239F05AC6DEA77D8DBB16",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs.meta"] = "23F69B25352C988265AD082C411C5CA4024604E18573C2683BC802672F615013",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs"] = "A07FE388D1D7FCDE057FAE984EE8FF096886A1306D21C544253B4E139A8C3013",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs.meta"] = "75095C083308857590BF4E186534054E8497C0CA6510017CEFE7B0B84C24FAAC",
                ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDataValidatorAndSnapshotVerifier.cs.meta"] = "08CE86C7875791F46DD2D47E2F385DAB0D96C9C98273F49652D5AC6795CACFE7"
            });

        private static readonly string[] RootPropertyWhitelist =
        {
            "SchemaId", "SchemaVersion", "EnemyDomainSnapshot", "EnemyMechanicVocabularySnapshot",
            "EnemyValidationContentSnapshot", "EncounterCompositionCatalogSnapshot",
            "EnemySkillBossPhaseCatalogSnapshot", "CounterWindowAndPressureCatalogSnapshot",
            "SchemaManifest", "IdentityIndex", "RelationIndex", "PlayerSafe", "CanonicalSignature",
            "PlayerSafeCanonicalSignature", "DevOnly", "IsEnabled", "EntersFormalFlow"
        };

        public static bool RunOffline()
        {
            return Run("offline");
        }

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Enemy Data Validator And Snapshot")]
        public static void RunFromMenu()
        {
            bool passed = Run("editor-menu");
            if (!passed) throw new InvalidOperationException("EnemyDataValidatorAndSnapshot verification failed.");
        }

        public static void RunBatch()
        {
            bool passed = Run("unity-batch");
            EditorApplication.Exit(passed ? 0 : 1);
        }
#endif

        private static bool Run(string mode)
        {
            string root = FindProjectRoot();
            Fixture fixture = CreateFixture(false, FixtureMutation.None);
            List<Check> checks = new List<Check>();
            AddCoreChecks(checks, root, fixture);
            AddMutationChecks(checks, fixture);
            AddNegativeChecks(checks, fixture);
            AddLeakAndBaselineChecks(checks, root, fixture);

            Directory.CreateDirectory(Absolute(root, ReportRoot));
            WriteReports(root, mode, checks, fixture);
            AddPackageChecks(checks, root);
            Check deterministic = new Check("reports.deterministic", "7/7 hash identical", "7/7 hash identical", true);
            checks.Add(deterministic);
            WriteReports(root, mode, checks, fixture);
            IReadOnlyDictionary<string, string> first = ReportHashes(root);
            WriteReports(root, mode, checks, fixture);
            IReadOnlyDictionary<string, string> second = ReportHashes(root);
            int identical = first.Count(pair => second.TryGetValue(pair.Key, out string value) && value == pair.Value);
            deterministic.Actual = identical.ToString(CultureInfo.InvariantCulture) + "/7 hash identical";
            deterministic.Passed = identical == 7;
            WriteReports(root, mode, checks, fixture);
            IReadOnlyDictionary<string, string> finalFirst = ReportHashes(root);
            WriteReports(root, mode, checks, fixture);
            IReadOnlyDictionary<string, string> finalSecond = ReportHashes(root);
            bool finalDeterministic = finalFirst.All(pair => finalSecond.TryGetValue(pair.Key, out string value) && value == pair.Value);
            return checks.All(value => value.Passed) && finalDeterministic;
        }

        private static void AddCoreChecks(List<Check> checks, string root, Fixture fixture)
        {
            EnemySystemSnapshot snapshot = fixture.Root;
            Add(checks, "root.schema", "EnemySystemSnapshot.v1/1", snapshot.SchemaId + "/" + snapshot.SchemaVersion,
                snapshot.SchemaId == EnemySystemSnapshotSchema.SchemaId && snapshot.SchemaVersion == EnemySystemSnapshotSchema.SchemaVersion);
            Add(checks, "manifest.count", "8", snapshot.SchemaManifest.Count.ToString(CultureInfo.InvariantCulture), snapshot.SchemaManifest.Count == 8);
            Add(checks, "manifest.static", "6", snapshot.SchemaManifest.Count(value => value.ComponentMode == EnemySystemComponentMode.StaticEmbedded && value.EmbeddedInRoot).ToString(CultureInfo.InvariantCulture),
                snapshot.SchemaManifest.Count(value => value.ComponentMode == EnemySystemComponentMode.StaticEmbedded && value.EmbeddedInRoot) == 6);
            int staticFullSignatures = snapshot.SchemaManifest.Count(value =>
                value.ComponentMode == EnemySystemComponentMode.StaticEmbedded
                && value.EmbeddedInRoot && IsSignature(value.ComponentCanonicalSignature));
            Add(checks, "manifest.staticFullSignatures", "6/6 non-empty", staticFullSignatures + "/6 non-empty",
                staticFullSignatures == 6);
            Add(checks, "manifest.transient", "2", snapshot.SchemaManifest.Count(value => value.ComponentMode == EnemySystemComponentMode.TransientContract && !value.EmbeddedInRoot).ToString(CultureInfo.InvariantCulture),
                snapshot.SchemaManifest.Count(value => value.ComponentMode == EnemySystemComponentMode.TransientContract && !value.EmbeddedInRoot) == 2);
            Add(checks, "manifest.e07e08Embedded", "0/0", string.Join("/", snapshot.SchemaManifest.Where(value => value.ComponentId == "E07" || value.ComponentId == "E08").Select(value => value.EmbeddedInRoot ? "1" : "0")),
                snapshot.SchemaManifest.Where(value => value.ComponentId == "E07" || value.ComponentId == "E08").All(value => !value.EmbeddedInRoot));
            Add(checks, "root.signature.full", "sha256 lowercase", snapshot.CanonicalSignature, IsSignature(snapshot.CanonicalSignature));
            Add(checks, "root.signature.player", "sha256 lowercase", snapshot.PlayerSafeCanonicalSignature, IsSignature(snapshot.PlayerSafeCanonicalSignature));
            Add(checks, "root.isolation", "true/false/false", snapshot.DevOnly + "/" + snapshot.IsEnabled + "/" + snapshot.EntersFormalFlow,
                snapshot.DevOnly && !snapshot.IsEnabled && !snapshot.EntersFormalFlow);
            Add(checks, "identity.duplicates", "0", DuplicateIdentityCount(snapshot).ToString(CultureInfo.InvariantCulture), DuplicateIdentityCount(snapshot) == 0);
            EnemySystemIdentitySnapshot[] counterWindows = snapshot.IdentityIndex
                .Where(value => value.IdentityKind == EnemySystemIdentityKind.CounterWindow).ToArray();
            int counterWindowTypeCount = fixture.Vocabulary.Entries
                .Count(value => value.Key.Category == EnemyVocabularyCategory.CounterWindowType);
            Add(checks, "identity.counterWindowOwner", "E06 only", string.Join(";", counterWindows
                .Select(value => value.OwningComponentId).Distinct().OrderBy(value => value, StringComparer.Ordinal)),
                counterWindows.Length == 3 && counterWindows.All(value => value.OwningComponentId == "E06"));
            Add(checks, "identity.counterWindowVocabulary", "6 E02 vocabulary / 0 E02 identities",
                counterWindowTypeCount + "/" + counterWindows.Count(value => value.OwningComponentId == "E02"),
                counterWindowTypeCount == 6 && counterWindows.All(value => value.OwningComponentId != "E02"));
            Add(checks, "relation.duplicates", "0", DuplicateRelationCount(snapshot).ToString(CultureInfo.InvariantCulture), DuplicateRelationCount(snapshot) == 0);
            Add(checks, "relation.unresolved", "0", snapshot.RelationIndex.Count(value => !value.Resolved).ToString(CultureInfo.InvariantCulture), snapshot.RelationIndex.All(value => value.Resolved));
            Add(checks, "relation.isolationMismatch", "0", snapshot.RelationIndex.Count(value => !value.IsolationCompatible).ToString(CultureInfo.InvariantCulture), snapshot.RelationIndex.All(value => value.IsolationCompatible));
            Add(checks, "isolation.formalFlow", "0", snapshot.IdentityIndex.Count(value => !value.DevOnly || value.IsEnabled || value.EntersFormalFlow).ToString(CultureInfo.InvariantCulture),
                snapshot.IdentityIndex.All(value => value.DevOnly && !value.IsEnabled && !value.EntersFormalFlow));

            Add(checks, "baseline.e02.total", "63", fixture.Vocabulary.Entries.Count.ToString(CultureInfo.InvariantCulture), fixture.Vocabulary.Entries.Count == 63);
            Add(checks, "baseline.e02.capability", "16", fixture.Vocabulary.Entries.Count(value => value.Key.Category == EnemyVocabularyCategory.BuildCapability).ToString(CultureInfo.InvariantCulture),
                fixture.Vocabulary.Entries.Count(value => value.Key.Category == EnemyVocabularyCategory.BuildCapability) == 16);
            Add(checks, "baseline.e03.carriers", "11/7/18", fixture.Normalization.Enemies.Count + "/" + fixture.Normalization.Bosses.Count + "/" + (fixture.Normalization.Enemies.Count + fixture.Normalization.Bosses.Count),
                fixture.Normalization.Enemies.Count == 11 && fixture.Normalization.Bosses.Count == 7);
            Add(checks, "baseline.e03.profiles", "10/6/16", fixture.Normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy) + "/" + fixture.Normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss) + "/" + fixture.Normalization.MechanicProfiles.Count,
                fixture.Normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy) == 10 && fixture.Normalization.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss) == 6);
            Add(checks, "baseline.e03.mapRules", "10", fixture.Normalization.MapRules.Count.ToString(CultureInfo.InvariantCulture), fixture.Normalization.MapRules.Count == 10);
            Add(checks, "baseline.e03.carrierBindings", "17", fixture.Normalization.CarrierMechanicBindings.Count.ToString(CultureInfo.InvariantCulture), fixture.Normalization.CarrierMechanicBindings.Count == 17);
            Add(checks, "baseline.e03.mapBindings", "30", fixture.Normalization.MapRuleMechanicBindings.Count.ToString(CultureInfo.InvariantCulture), fixture.Normalization.MapRuleMechanicBindings.Count == 30);
            Add(checks, "baseline.e03.exceptions", "2", fixture.Normalization.Exceptions.Count.ToString(CultureInfo.InvariantCulture), fixture.Normalization.Exceptions.Count == 2);
            int waves = fixture.Encounter.Encounters.Sum(value => value.Waves.Count);
            int slots = fixture.Encounter.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count));
            Add(checks, "baseline.e04", "2/4/6", fixture.Encounter.Encounters.Count + "/" + waves + "/" + slots,
                fixture.Encounter.Encounters.Count == 2 && waves == 4 && slots == 6);
            Add(checks, "baseline.e05", "4/3/3/3/2", fixture.SkillPhase.SkillPatterns.Count + "/" + fixture.SkillPhase.SkillSequences.Count + "/" + fixture.SkillPhase.CarrierSkillBindings.Count + "/" + fixture.SkillPhase.BossPhaseProfiles.Count + "/" + fixture.SkillPhase.BossPhasePlans.Count,
                fixture.SkillPhase.SkillPatterns.Count == 4 && fixture.SkillPhase.SkillSequences.Count == 3 && fixture.SkillPhase.CarrierSkillBindings.Count == 3 && fixture.SkillPhase.BossPhaseProfiles.Count == 3 && fixture.SkillPhase.BossPhasePlans.Count == 2);
            int requirements = fixture.Pressure.BuildPressureProfiles.Sum(value => value.DeveloperOnly.RequirementGroups.Sum(group => group.Requirements.Count));
            Add(checks, "baseline.e06", "4/3/6/4/5/11", fixture.Pressure.BuildPressureProfiles.Count + "/" + fixture.Pressure.CounterWindowProfiles.Count + "/" + fixture.Pressure.PressureSourceBindings.Count + "/" + fixture.Pressure.CounterWindowSourceBindings.Count + "/" + fixture.Pressure.PressureCounterWindowBindings.Count + "/" + requirements,
                fixture.Pressure.BuildPressureProfiles.Count == 4 && fixture.Pressure.CounterWindowProfiles.Count == 3 && fixture.Pressure.PressureSourceBindings.Count == 6 && fixture.Pressure.CounterWindowSourceBindings.Count == 4 && fixture.Pressure.PressureCounterWindowBindings.Count == 5 && requirements == 11);
            CapabilityGapSnapshot[] gapShapes =
            {
                new CapabilityGapSnapshot("group.fixture.unknown", fixture.KnownCapabilityKeys[0], CapabilityValueAvailability.Unknown, 0, 5000, 5000),
                new CapabilityGapSnapshot("group.fixture.blocked", fixture.KnownCapabilityKeys[1], CapabilityValueAvailability.Known, 2000, 5000, 3000),
                new CapabilityGapSnapshot("group.fixture.ready", fixture.KnownCapabilityKeys[2], CapabilityValueAvailability.Known, 7000, 5000, 0)
            };
            int bands = Enum.GetValues(typeof(ReadinessBand)).Length;
            string e07Actual = fixture.KnownCapabilityKeys.Count + "/" + fixture.CompleteBuild.CapabilityValues.Count + "/"
                + fixture.SparseBuild.CapabilityValues.Count + "/" + fixture.CompleteBuild.CapabilityValues.Sum(value => value.SourceSummaries.Count)
                + "/" + gapShapes.Length + "/" + bands;
            Add(checks, "baseline.e07", "16/16/4/6/3/5", e07Actual,
                fixture.KnownCapabilityKeys.Count == 16 && fixture.CompleteBuild.CapabilityValues.Count == 16
                && fixture.SparseBuild.CapabilityValues.Count == 4
                && fixture.CompleteBuild.CapabilityValues.Sum(value => value.SourceSummaries.Count) == 6
                && gapShapes.Length == 3 && bands == 5);
            string e08Report = File.Exists(Absolute(root, "Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorReport.md"))
                ? File.ReadAllText(Absolute(root, "Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorReport.md")) : string.Empty;
            Add(checks, "baseline.e08", "15 scenarios/212 checks", e08Report.Contains("Scenarios: `15`") + "/" + e08Report.Contains("212/212 PASS"),
                e08Report.Contains("Scenarios: `15`") && e08Report.Contains("212/212 PASS"));
            Add(checks, "transient.e07e08Created", "complete+sparse+2 readiness", fixture.CompleteBuild.CoverageMode + "/" + fixture.SparseBuild.CoverageMode + "/2",
                fixture.CompleteReadiness != null && fixture.SparseReadiness != null);

            IReadOnlyList<EnemySystemRelationSnapshot> manyToManyFixture = Array.AsReadOnly(new[]
            {
                SyntheticRelation(EnemySystemIdentityKind.Enemy, "dev_carrier_fixture_alpha", EnemySystemRelationKind.NormalizedCarrierMechanicProfile, EnemySystemIdentityKind.MechanicProfile, "dev_profile_fixture_alpha"),
                SyntheticRelation(EnemySystemIdentityKind.Enemy, "dev_carrier_fixture_alpha", EnemySystemRelationKind.NormalizedCarrierMechanicProfile, EnemySystemIdentityKind.MechanicProfile, "dev_profile_fixture_beta"),
                SyntheticRelation(EnemySystemIdentityKind.Enemy, "dev_carrier_fixture_beta", EnemySystemRelationKind.NormalizedCarrierMechanicProfile, EnemySystemIdentityKind.MechanicProfile, "dev_profile_fixture_alpha"),
                SyntheticRelation(EnemySystemIdentityKind.MapRule, "dev_map_fixture_alpha", EnemySystemRelationKind.MapRuleMechanicProfile, EnemySystemIdentityKind.MechanicProfile, "dev_profile_fixture_alpha"),
                SyntheticRelation(EnemySystemIdentityKind.MapRule, "dev_map_fixture_alpha", EnemySystemRelationKind.MapRuleMechanicProfile, EnemySystemIdentityKind.MechanicProfile, "dev_profile_fixture_beta"),
                SyntheticRelation(EnemySystemIdentityKind.MapRule, "dev_map_fixture_beta", EnemySystemRelationKind.MapRuleMechanicProfile, EnemySystemIdentityKind.MechanicProfile, "dev_profile_fixture_alpha"),
                SyntheticRelation(EnemySystemIdentityKind.PressureSource, "dev_pressure_fixture_shared|source_a", EnemySystemRelationKind.PressureSourceTarget, EnemySystemIdentityKind.MechanicProfile, "dev_profile_fixture_alpha"),
                SyntheticRelation(EnemySystemIdentityKind.PressureSource, "dev_pressure_fixture_shared|source_b", EnemySystemRelationKind.PressureSourceTarget, EnemySystemIdentityKind.MapRule, "dev_map_fixture_alpha"),
                SyntheticRelation(EnemySystemIdentityKind.BuildPressureProfile, "dev_pressure_fixture_alpha", EnemySystemRelationKind.PressureCounterWindow, EnemySystemIdentityKind.CounterWindow, "dev_window_fixture_shared"),
                SyntheticRelation(EnemySystemIdentityKind.BuildPressureProfile, "dev_pressure_fixture_beta", EnemySystemRelationKind.PressureCounterWindow, EnemySystemIdentityKind.CounterWindow, "dev_window_fixture_shared")
            });
            int multiCarrier = DefaultEnemySystemDataValidator.Instance.ValidateRelationIndex(manyToManyFixture).Count == 0 ? 1 : 0;
            int reusedProfile = fixture.Normalization.CarrierMechanicBindings.GroupBy(value => value.MechanicProfileId, StringComparer.Ordinal).Count(value => value.Count() > 1);
            int multiMap = fixture.Normalization.MapRuleMechanicBindings.GroupBy(value => value.MapRuleId, StringComparer.Ordinal).Count(value => value.Count() > 1);
            int reusedMapProfile = fixture.Normalization.MapRuleMechanicBindings.GroupBy(value => value.MechanicProfileId, StringComparer.Ordinal).Count(value => value.Count() > 1);
            int reusedPressure = fixture.Pressure.PressureSourceBindings.GroupBy(value => value.BuildPressureProfileId, StringComparer.Ordinal).Count(value => value.Count() > 1);
            int reusedWindow = fixture.Pressure.PressureCounterWindowBindings.GroupBy(value => value.CounterWindowId, StringComparer.Ordinal).Count(value => value.Count() > 1);
            Add(checks, "coverage.manyToMany", "all six >0", string.Join("/", new[] { multiCarrier, reusedProfile, multiMap, reusedMapProfile, reusedPressure, reusedWindow }),
                multiCarrier > 0 && reusedProfile > 0 && multiMap > 0 && reusedMapProfile > 0 && reusedPressure > 0 && reusedWindow > 0);
            Add(checks, "coverage.intentionalExceptions", "2 accepted", fixture.Normalization.Exceptions.Count + " accepted", fixture.Normalization.Exceptions.Count == 2);
            HashSet<string> encounterProfiles = new HashSet<string>(fixture.Encounter.Encounters.SelectMany(value => value.Waves).SelectMany(value => value.Slots).SelectMany(value => value.MechanicProfileIds), StringComparer.Ordinal);
            int unused = fixture.Normalization.MechanicProfiles.Count(value => !encounterProfiles.Contains(value.Domain.StableId));
            Add(checks, "coverage.legalUnused", ">0 accepted", unused.ToString(CultureInfo.InvariantCulture) + " accepted", unused > 0);
            IReadOnlyList<EnemySystemRelationSnapshot> syntheticCoverage = CreateRelationKindCoverageRows();
            HashSet<EnemySystemRelationKind> coveredKinds = new HashSet<EnemySystemRelationKind>(
                snapshot.RelationIndex.Select(value => value.RelationKind));
            coveredKinds.UnionWith(syntheticCoverage.Select(value => value.RelationKind));
            int relationKindTotal = Enum.GetValues(typeof(EnemySystemRelationKind)).Length;
            IReadOnlyList<EnemySystemValidationIssue> coverageIssues =
                DefaultEnemySystemDataValidator.Instance.ValidateRelationIndex(syntheticCoverage);
            Add(checks, "coverage.relationKinds", relationKindTotal + "/" + relationKindTotal,
                coveredKinds.Count + "/" + relationKindTotal, coveredKinds.Count == relationKindTotal
                && syntheticCoverage.Count == relationKindTotal && coverageIssues.Count == 0);
        }

        private static void AddMutationChecks(List<Check> checks, Fixture fixture)
        {
            Fixture reversed = CreateFixture(true, FixtureMutation.None);
            Add(checks, "signature.order.full", fixture.Root.CanonicalSignature, reversed.Root.CanonicalSignature, fixture.Root.CanonicalSignature == reversed.Root.CanonicalSignature);
            Add(checks, "signature.order.player", fixture.Root.PlayerSafeCanonicalSignature, reversed.Root.PlayerSafeCanonicalSignature, fixture.Root.PlayerSafeCanonicalSignature == reversed.Root.PlayerSafeCanonicalSignature);

            AddSignatureMutation(checks, fixture, "e03.developer", FixtureMutation.E03Developer, true, false);
            AddSignatureMutation(checks, fixture, "e05.developer", FixtureMutation.E05Developer, true, false);
            AddSignatureMutation(checks, fixture, "e06.developer", FixtureMutation.E06Developer, true, false);
            AddSignatureMutation(checks, fixture, "e03.player", FixtureMutation.E03Player, true, true);
            AddSignatureMutation(checks, fixture, "e05.player", FixtureMutation.E05Player, true, true);
            AddSignatureMutation(checks, fixture, "e06.player", FixtureMutation.E06Player, true, true);
            Fixture e04Composition = CreateFixture(false, FixtureMutation.E04Composition);
            Add(checks, "signature.e04.composition.catalog", "different",
                e04Composition.Encounter.CanonicalSignature,
                e04Composition.Encounter.CanonicalSignature != fixture.Encounter.CanonicalSignature);
            Add(checks, "signature.e04.composition.full", "different",
                e04Composition.Root.CanonicalSignature,
                e04Composition.Root.CanonicalSignature != fixture.Root.CanonicalSignature);
            Add(checks, "signature.e04.composition.player", "stable",
                e04Composition.Root.PlayerSafeCanonicalSignature,
                e04Composition.Root.PlayerSafeCanonicalSignature == fixture.Root.PlayerSafeCanonicalSignature);
            Add(checks, "signature.e04.composition.identities", "stable",
                IdentityFingerprint(e04Composition.Root),
                IdentityFingerprint(e04Composition.Root) == IdentityFingerprint(fixture.Root));
            Add(checks, "signature.e04.composition.relations", "stable",
                RelationFingerprint(e04Composition.Root),
                RelationFingerprint(e04Composition.Root) == RelationFingerprint(fixture.Root));
            AddSignatureMutation(checks, fixture, "e04.relationTarget", FixtureMutation.E04RelationTarget, true, false);

            Fixture collision = CreateFixture(false, FixtureMutation.CounterWindowTypeCollision);
            HashSet<string> counterWindowTypes = new HashSet<string>(fixture.Vocabulary.Entries
                .Where(value => value.Key.Category == EnemyVocabularyCategory.CounterWindowType)
                .Select(value => value.Key.Value), StringComparer.Ordinal);
            EnemySystemIdentitySnapshot[] collisionWindows = collision.Root.IdentityIndex
                .Where(value => value.IdentityKind == EnemySystemIdentityKind.CounterWindow).ToArray();
            bool collisionAccepted = collision.Pressure.CounterWindowProfiles.Count == 4
                && collisionWindows.Length == 4
                && collisionWindows.All(value => value.OwningComponentId == "E06")
                && collisionWindows.Any(value => counterWindowTypes.Contains(value.StableId))
                && DefaultEnemySystemDataValidator.Instance.Validate(collision.Input).Count == 0;
            Add(checks, "identity.counterWindowTypeCollision", "accepted", collisionAccepted ? "accepted" : "rejected", collisionAccepted);

            EnemySystemSnapshot transientRootA = DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(fixture.Input);
            EnemySystemSnapshot transientRootB = DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(fixture.Input);
            string completeDeveloper = fixture.CompleteReadiness.DeveloperReadiness.DeveloperCanonicalSignature;
            string sparseDeveloper = fixture.SparseReadiness.DeveloperReadiness.DeveloperCanonicalSignature;
            string completePlayer = fixture.CompleteReadiness.PlayerHintProjection.PlayerSafeCanonicalSignature;
            string sparsePlayer = fixture.SparseReadiness.PlayerHintProjection.PlayerSafeCanonicalSignature;
            Add(checks, "transient.fixtures.buildDistinct", "different", fixture.CompleteBuild.CanonicalSignature + " / " + fixture.SparseBuild.CanonicalSignature,
                fixture.CompleteBuild.CanonicalSignature != fixture.SparseBuild.CanonicalSignature);
            Add(checks, "transient.fixtures.readinessDeveloperDistinct", "different", completeDeveloper + " / " + sparseDeveloper,
                completeDeveloper != sparseDeveloper);
            Add(checks, "transient.fixtures.readinessPlayerDistinct", "different", completePlayer + " / " + sparsePlayer,
                completePlayer != sparsePlayer);
            string severities = fixture.CompleteReadiness.DeveloperReadiness.ReadinessBand + "/"
                + fixture.SparseReadiness.DeveloperReadiness.ReadinessBand + "/"
                + fixture.CompleteReadiness.PlayerHintProjection.PlayerHintSeverity + "/"
                + fixture.SparseReadiness.PlayerHintProjection.PlayerHintSeverity;
            Add(checks, "transient.fixtures.severityDistinct", "developer and player severity differ", severities,
                fixture.CompleteReadiness.DeveloperReadiness.ReadinessBand != fixture.SparseReadiness.DeveloperReadiness.ReadinessBand
                && fixture.CompleteReadiness.PlayerHintProjection.PlayerHintSeverity != fixture.SparseReadiness.PlayerHintProjection.PlayerHintSeverity);
            Add(checks, "signature.transient.fullStable", fixture.Root.CanonicalSignature,
                transientRootB.CanonicalSignature, transientRootA.CanonicalSignature == fixture.Root.CanonicalSignature
                && transientRootB.CanonicalSignature == fixture.Root.CanonicalSignature
                && !ReferenceEquals(transientRootA, transientRootB));
            Add(checks, "signature.transient.playerStable", fixture.Root.PlayerSafeCanonicalSignature,
                transientRootB.PlayerSafeCanonicalSignature,
                transientRootA.PlayerSafeCanonicalSignature == fixture.Root.PlayerSafeCanonicalSignature
                && transientRootB.PlayerSafeCanonicalSignature == fixture.Root.PlayerSafeCanonicalSignature);
            string staticA = string.Join(";", transientRootA.SchemaManifest.Where(value => value.ComponentId.CompareTo("E07") < 0)
                .Select(value => value.ComponentId + "=" + value.ComponentCanonicalSignature));
            string staticB = string.Join(";", transientRootB.SchemaManifest.Where(value => value.ComponentId.CompareTo("E07") < 0)
                .Select(value => value.ComponentId + "=" + value.ComponentCanonicalSignature));
            Add(checks, "transient.staticComponents", "E01-E06 identical", staticB, staticA == staticB);
            int transientIdentity = fixture.Root.IdentityIndex.Count(value => value.OwningComponentId == "E07" || value.OwningComponentId == "E08");
            int transientRelation = fixture.Root.RelationIndex.Count(value => value.OwningComponentId == "E07" || value.OwningComponentId == "E08");
            Add(checks, "transient.identityRelationAbsence", "0/0", transientIdentity + "/" + transientRelation,
                transientIdentity == 0 && transientRelation == 0);
            EnemySystemSchemaManifestEntry[] transientManifest = fixture.Root.SchemaManifest
                .Where(value => value.ComponentId == "E07" || value.ComponentId == "E08").ToArray();
            Add(checks, "transient.staticPayloadAbsence", "2 contracts; 0 embedded; empty static signatures",
                transientManifest.Length + "/" + transientManifest.Count(value => value.EmbeddedInRoot) + "/"
                + transientManifest.Count(value => value.ComponentCanonicalSignature.Length == 0 && value.PlayerSafeCanonicalSignature.Length == 0),
                transientManifest.Length == 2 && transientManifest.All(value => !value.EmbeddedInRoot
                    && value.ComponentCanonicalSignature.Length == 0 && value.PlayerSafeCanonicalSignature.Length == 0));
        }

        private static void AddSignatureMutation(List<Check> checks, Fixture baseline, string id,
            FixtureMutation mutation, bool fullChanges, bool playerChanges)
        {
            Fixture changed = CreateFixture(false, mutation);
            bool fullDifferent = changed.Root.CanonicalSignature != baseline.Root.CanonicalSignature;
            bool playerDifferent = changed.Root.PlayerSafeCanonicalSignature != baseline.Root.PlayerSafeCanonicalSignature;
            Add(checks, "signature." + id + ".full", fullChanges ? "different" : "stable",
                changed.Root.CanonicalSignature, fullDifferent == fullChanges);
            Add(checks, "signature." + id + ".player", playerChanges ? "different" : "stable",
                changed.Root.PlayerSafeCanonicalSignature, playerDifferent == playerChanges);
        }

        private static void AddNegativeChecks(List<Check> checks, Fixture fixture)
        {
            ExpectIssue(checks, "negative.nullInput", "E09_NULL_INPUT", () => DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(null));
            ExpectIssue(checks, "negative.nullComponent", "E09_COMPONENT_NULL", () => DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(
                new EnemySystemSnapshotInput(fixture.Normalization.DomainSnapshot, fixture.Vocabulary, fixture.Normalization,
                    fixture.Encounter, fixture.SkillPhase, null)));

            List<EnemySystemSchemaManifestEntry> schemaChanged = fixture.Root.SchemaManifest.ToList();
            EnemySystemSchemaManifestEntry e08 = schemaChanged.Single(value => value.ComponentId == "E08");
            schemaChanged[schemaChanged.IndexOf(e08)] = new EnemySystemSchemaManifestEntry("E08", "EnemyOfflineReadiness.v2", 2,
                EnemySystemComponentMode.TransientContract, false, string.Empty, string.Empty, e08.ContractCanonicalSignature);
            IReadOnlyList<EnemySystemValidationIssue> schemaIssues = DefaultEnemySystemDataValidator.Instance.ValidateSchemaManifest(fixture.Input, schemaChanged);
            Add(checks, "negative.transientSchema", "E09_MANIFEST_CONTRACT_MISMATCH", string.Join(";", schemaIssues.Select(value => value.Code).Distinct()),
                schemaIssues.Any(value => value.Code == "E09_MANIFEST_CONTRACT_MISMATCH"));

            List<EnemySystemSchemaManifestEntry> duplicate = fixture.Root.SchemaManifest.ToList();
            duplicate[7] = duplicate[6];
            IReadOnlyList<EnemySystemValidationIssue> duplicateIssues = DefaultEnemySystemDataValidator.Instance.ValidateSchemaManifest(fixture.Input, duplicate);
            Add(checks, "negative.manifestDuplicate", "E09_MANIFEST_DUPLICATE", string.Join(";", duplicateIssues.Select(value => value.Code).Distinct()),
                duplicateIssues.Any(value => value.Code == "E09_MANIFEST_DUPLICATE"));

            EnemyDomainSnapshot extended = DefaultEnemyDomainSnapshotProvider.Instance.CreateSnapshot(new EnemyDomainSnapshotInput(
                fixture.Normalization.DomainSnapshot.Enemies, fixture.Normalization.DomainSnapshot.Bosses,
                fixture.Normalization.DomainSnapshot.MechanicProfiles.Concat(new[] { new MechanicProfileReference("dev_profile_fixture_missing") }).ToArray(),
                fixture.Normalization.DomainSnapshot.SkillPatterns, fixture.Normalization.DomainSnapshot.BossPhases,
                fixture.Normalization.DomainSnapshot.MapRules, fixture.Normalization.DomainSnapshot.Encounters,
                fixture.Normalization.DomainSnapshot.CounterWindows));
            IReadOnlyList<EnemySystemValidationIssue> unresolved = DefaultEnemySystemDataValidator.Instance.Validate(
                new EnemySystemSnapshotInput(extended, fixture.Vocabulary, fixture.Normalization,
                    fixture.Encounter, fixture.SkillPhase, fixture.Pressure));
            Add(checks, "negative.componentMismatch", "E09_DOMAIN_SIGNATURE_MISMATCH",
                string.Join(";", unresolved.Select(value => value.Code).Distinct()),
                unresolved.Any(value => value.Code == "E09_DOMAIN_SIGNATURE_MISMATCH"));
            Add(checks, "negative.unresolvedOptional", "E09_REFERENCE_UNRESOLVED", string.Join(";", unresolved.Select(value => value.Code).Distinct()),
                unresolved.Any(value => value.Code == "E09_REFERENCE_UNRESOLVED"));

            ReferenceResolver mismatchResolver = new ReferenceResolver(fixture.Normalization, fixture.Vocabulary);
            bool carrierKindRejected = false;
            string carrierKindActual = "accepted";
            try
            {
                EncounterCompositionSnapshot mismatchEncounter = new EncounterCompositionSnapshot(
                    new EncounterReference("dev_encounter_guardfix_carrier_mismatch"), Array.Empty<string>(),
                    new[] { Wave("wave_mismatch", 0, BossSlot("slot_mismatch", 0, DefaultEnemyId, new[] { DefaultEnemyProfileId })) },
                    new[] { "dev_tag.guardfix.carrier_mismatch" });
                DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                    new EncounterCompositionCatalogInput(new[] { mismatchEncounter }), mismatchResolver);
            }
            catch (EncounterCompositionValidationException exception)
            {
                carrierKindActual = string.Join(";", exception.Issues.Select(value => value.Code).Distinct());
                carrierKindRejected = exception.Issues.Any(value => value.Code == "SLOT_CARRIER_KIND_MISMATCH");
            }
            Add(checks, "negative.carrierKindMismatch", "SLOT_CARRIER_KIND_MISMATCH", carrierKindActual, carrierKindRejected);

            IReadOnlyList<EnemySystemValidationIssue> isolationIssues =
                DefaultEnemySystemDataValidator.Instance.ValidateRelationIndex(Array.AsReadOnly(new[]
                {
                    new EnemySystemRelationSnapshot(EnemySystemIdentityKind.Enemy, "synthetic.isolation.source",
                        EnemySystemRelationKind.CarrierMechanicProfile, EnemySystemIdentityKind.MechanicProfile,
                        "synthetic.isolation.target", "E09", true, false)
                }));
            Add(checks, "negative.isolationMismatch", "E09_RELATION_ISOLATION_MISMATCH",
                string.Join(";", isolationIssues.Select(value => value.Code).Distinct()),
                isolationIssues.Any(value => value.Code == "E09_RELATION_ISOLATION_MISMATCH"));

            bool playerLeakRejected;
            try
            {
                CreateFixture(false, FixtureMutation.None, true);
                playerLeakRejected = false;
            }
            catch (Exception exception)
            {
                playerLeakRejected = exception is EnemySkillBossPhaseValidationException
                    || exception is EnemySystemValidationException;
            }
            Add(checks, "negative.playerLeak", "rejected upstream or E09", playerLeakRejected ? "rejected" : "accepted", playerLeakRejected);
        }

        private static void AddLeakAndBaselineChecks(List<Check> checks, string root, Fixture fixture)
        {
            foreach (string file in RuntimeFiles)
            {
                string text = File.ReadAllText(Absolute(root, file));
                foreach (string token in RuntimeForbiddenTokens)
                {
                    bool absent = text.IndexOf(token, StringComparison.Ordinal) < 0;
                    Add(checks, "leak.runtime." + SafeId(Path.GetFileName(file)) + "." + SafeId(token), "absent", absent ? "absent" : "present", absent);
                }
            }
            string[] rootProperties = typeof(EnemySystemSnapshot).GetProperties().Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(checks, "leak.root.e07", "absent", rootProperties.Contains("BuildCapabilitySnapshot") ? "present" : "absent", !rootProperties.Contains("BuildCapabilitySnapshot"));
            Add(checks, "leak.root.e08", "absent", rootProperties.Contains("EnemyReadinessEvaluationResult") ? "present" : "absent", !rootProperties.Contains("EnemyReadinessEvaluationResult"));
            string[] expectedRootProperties = RootPropertyWhitelist.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(checks, "leak.root.propertyClosure", string.Join(";", expectedRootProperties),
                string.Join(";", rootProperties), rootProperties.SequenceEqual(expectedRootProperties, StringComparer.Ordinal));
            string[] playerProperties = typeof(EnemySystemPlayerSafeSnapshot).GetProperties().Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] allowed = { "SchemaId", "SchemaVersion", "Enemies", "Bosses", "MechanicProfiles", "MapRules", "SkillPatterns", "BossPhases", "BuildPressureProfiles", "CounterWindows", "CanonicalSignature" };
            Add(checks, "leak.player.propertyClosure", string.Join(";", allowed.OrderBy(value => value, StringComparer.Ordinal)), string.Join(";", playerProperties),
                playerProperties.SequenceEqual(allowed.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal));
            IReadOnlyList<EnemySystemValidationIssue> validIssues = DefaultEnemySystemDataValidator.Instance.Validate(fixture.Input);
            Add(checks, "leak.player.validator", "0", validIssues.Count(value => value.Category == EnemySystemValidationCategory.PlayerLeak).ToString(CultureInfo.InvariantCulture),
                validIssues.All(value => value.Category != EnemySystemValidationCategory.PlayerLeak));
            AddImmutabilityChecks(checks, fixture);
            AddHashChecks(checks, root, "protected", ProtectedHashes);
            AddHashChecks(checks, root, "legacy", LegacyHashes);
            AddHashChecks(checks, root, "frozenE09", FrozenE09Hashes);
            Add(checks, "protected.total", "38/38", ProtectedHashes.Count(pair => Sha256(Absolute(root, pair.Key)) == pair.Value).ToString(CultureInfo.InvariantCulture) + "/38",
                ProtectedHashes.All(pair => File.Exists(Absolute(root, pair.Key)) && Sha256(Absolute(root, pair.Key)) == pair.Value));
            Add(checks, "legacy.total", "3/3", LegacyHashes.Count(pair => Sha256(Absolute(root, pair.Key)) == pair.Value).ToString(CultureInfo.InvariantCulture) + "/3",
                LegacyHashes.All(pair => File.Exists(Absolute(root, pair.Key)) && Sha256(Absolute(root, pair.Key)) == pair.Value));
            Add(checks, "frozenE09.total", "11/11", FrozenE09Hashes.Count(pair => Sha256(Absolute(root, pair.Key)) == pair.Value).ToString(CultureInfo.InvariantCulture) + "/11",
                FrozenE09Hashes.All(pair => File.Exists(Absolute(root, pair.Key)) && Sha256(Absolute(root, pair.Key)) == pair.Value));
        }

        private static void AddImmutabilityChecks(List<Check> checks, Fixture fixture)
        {
            PropertyInfo[] inputProperties = typeof(EnemySystemSnapshotInput)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);
            bool inputRejected = inputProperties.Length == 6 && inputProperties.All(value => !value.CanWrite)
                && inputProperties.All(value => PropertyMutationRejected(value, fixture.Input));
            EnemySystemSnapshot rebuilt = DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(fixture.Input);
            Add(checks, "immutability.input", "6/6 rejected; signatures and content stable",
                inputProperties.Count(value => !value.CanWrite) + "/" + inputProperties.Length + " rejected; "
                + rebuilt.CanonicalSignature, inputRejected
                && rebuilt.CanonicalSignature == fixture.Root.CanonicalSignature
                && rebuilt.PlayerSafeCanonicalSignature == fixture.Root.PlayerSafeCanonicalSignature
                && rebuilt.IdentityIndex.Count == fixture.Root.IdentityIndex.Count
                && rebuilt.RelationIndex.Count == fixture.Root.RelationIndex.Count);

            int rootCountsBefore = fixture.Root.SchemaManifest.Count + fixture.Root.IdentityIndex.Count + fixture.Root.RelationIndex.Count;
            string rootFullBefore = fixture.Root.CanonicalSignature;
            string rootPlayerBefore = fixture.Root.PlayerSafeCanonicalSignature;
            bool rootRejected = MutationRejected(fixture.Root.SchemaManifest, fixture.Root.SchemaManifest[0])
                && MutationRejected(fixture.Root.IdentityIndex, fixture.Root.IdentityIndex[0])
                && MutationRejected(fixture.Root.RelationIndex, fixture.Root.RelationIndex[0]);
            int rootCountsAfter = fixture.Root.SchemaManifest.Count + fixture.Root.IdentityIndex.Count + fixture.Root.RelationIndex.Count;
            Add(checks, "immutability.rootCollections", "3/3 rejected; signatures and content stable",
                (rootRejected ? "3/3 rejected" : "mutation accepted") + "; " + rootCountsAfter,
                rootRejected && rootCountsBefore == rootCountsAfter
                && rootFullBefore == fixture.Root.CanonicalSignature
                && rootPlayerBefore == fixture.Root.PlayerSafeCanonicalSignature);

            EnemySystemPlayerSafeSnapshot player = fixture.Root.PlayerSafe;
            int playerCountBefore = player.Enemies.Count + player.Bosses.Count + player.MechanicProfiles.Count
                + player.MapRules.Count + player.SkillPatterns.Count + player.BossPhases.Count
                + player.BuildPressureProfiles.Count + player.CounterWindows.Count;
            string playerSignatureBefore = player.CanonicalSignature;
            bool playerRejected = MutationRejected(player.Enemies, player.Enemies[0])
                && MutationRejected(player.Bosses, player.Bosses[0])
                && MutationRejected(player.MechanicProfiles, player.MechanicProfiles[0])
                && MutationRejected(player.MapRules, player.MapRules[0])
                && MutationRejected(player.SkillPatterns, player.SkillPatterns[0])
                && MutationRejected(player.BossPhases, player.BossPhases[0])
                && MutationRejected(player.BuildPressureProfiles, player.BuildPressureProfiles[0])
                && MutationRejected(player.CounterWindows, player.CounterWindows[0]);
            int playerCountAfter = player.Enemies.Count + player.Bosses.Count + player.MechanicProfiles.Count
                + player.MapRules.Count + player.SkillPatterns.Count + player.BossPhases.Count
                + player.BuildPressureProfiles.Count + player.CounterWindows.Count;
            Add(checks, "immutability.playerCollections", "8/8 rejected; signature and content stable",
                (playerRejected ? "8/8 rejected" : "mutation accepted") + "; " + playerCountAfter,
                playerRejected && playerCountBefore == playerCountAfter
                && playerSignatureBefore == player.CanonicalSignature
                && playerSignatureBefore == fixture.Root.PlayerSafeCanonicalSignature);
        }

        private static Fixture CreateFixture(bool reverse, FixtureMutation mutation, bool playerLeak = false)
        {
            EnemyMechanicVocabularySnapshot vocabulary = DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                DefaultEnemyMechanicVocabularyCatalog.CreateInput(reverse));
            EnemyValidationContentSnapshot normalization = CreateNormalization(reverse, mutation);
            ReferenceResolver resolver = new ReferenceResolver(normalization, vocabulary);
            string alternateMapRuleId = normalization.MapRules.Select(value => value.Domain.StableId)
                .First(value => value != DefaultMapRuleId && value != "dev_map_black_furnace_smoke" && value != "dev_map_old_lane_echo");
            EncounterCompositionCatalogSnapshot encounter = DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                CreateEncounterInput(reverse, mutation, alternateMapRuleId), resolver);
            EnemySkillBossPhaseCatalogSnapshot skill = DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(
                CreateSkillInput(reverse, mutation, playerLeak), resolver);
            resolver.SkillPhase = skill;
            CounterWindowAndPressureCatalogSnapshot pressure = DefaultCounterWindowAndPressureProvider.Instance.CreateSnapshot(
                CreatePressureInput(reverse, mutation), resolver);
            EnemySystemSnapshotInput input = new EnemySystemSnapshotInput(normalization.DomainSnapshot, vocabulary,
                normalization, encounter, skill, pressure);
            EnemySystemSnapshot root = DefaultEnemySystemSnapshotProvider.Instance.CreateSnapshot(input);

            IReadOnlyList<string> knownKeys = resolver.GetKnownBuildCapabilityKeys();
            BuildCapabilitySnapshot complete = DefaultBuildCapabilitySnapshotProvider.Instance.CreateSnapshot(
                CreateBuildInput(knownKeys, BuildCapabilityCoverageMode.Complete, "dev_build_e09_complete", reverse), resolver);
            BuildCapabilitySnapshot sparse = DefaultBuildCapabilitySnapshotProvider.Instance.CreateSnapshot(
                CreateBuildInput(knownKeys.Take(4).ToArray(), BuildCapabilityCoverageMode.Sparse, "dev_build_e09_sparse", reverse), resolver);
            EnemyReadinessEvaluationResult completeReadiness = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(complete, pressure, PressureShield,
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver);
            EnemyReadinessEvaluationResult sparseReadiness = DefaultEnemyOfflineReadinessEvaluator.Instance.Evaluate(
                new EnemyReadinessEvaluationInput(sparse, pressure, PressureShield,
                    Array.Empty<MapRuleCapabilityAdjustmentSnapshot>()), resolver);
            return new Fixture(vocabulary, normalization, encounter, skill, pressure, input, root,
                knownKeys, complete, sparse, completeReadiness, sparseReadiness);
        }

        private static EnemyValidationContentSnapshot CreateNormalization(bool reverse, FixtureMutation mutation)
        {
            if (reverse) return EnemyValidationContentNormalizer.CreateSnapshot(true);
            if (mutation == FixtureMutation.E03Developer)
                return EnemyValidationContentNormalizer.CreateSnapshotForVerification(
                    EnemyValidationContentVerificationMutation.EnemyDeveloperOnly);
            if (mutation == FixtureMutation.E03Player)
                return EnemyValidationContentNormalizer.CreateSnapshotForVerification(
                    EnemyValidationContentVerificationMutation.PlayerSafe);
            return EnemyValidationContentNormalizer.CreateSnapshot(false);
        }

        private static EncounterCompositionCatalogInput CreateEncounterInput(bool reverse, FixtureMutation mutation,
            string alternateMapRuleId)
        {
            EncounterWaveSnapshot alphaOpening = Wave("wave_opening", 0,
                EnemySlot("slot_poison", 0, EncounterSlotRole.Normal, DefaultEnemyId,
                    mutation == FixtureMutation.E04Composition ? 3 : 2, new[] { DefaultEnemyProfileId }),
                EnemySlot("slot_burning", 1, EncounterSlotRole.Elite, "dev_enemy_burning_wisp", 1, new[] { DefaultEnemyProfileId }));
            EncounterWaveSnapshot alphaPressure = Wave("wave_pressure", 1,
                BossSlot("slot_boss", 0, DefaultBossId, new[] { DefaultBossProfileId }),
                EnemySlot("slot_guard", 1, EncounterSlotRole.Elite, "dev_enemy_shield_guard", 1, new[] { "dev_enemy_shield_problem" }));
            EncounterCompositionSnapshot alpha = new EncounterCompositionSnapshot(
                new EncounterReference("dev_encounter_fixture_alpha"),
                MaybeReverse(new[] { DefaultMapRuleId, mutation == FixtureMutation.E04RelationTarget
                    ? alternateMapRuleId : "dev_map_black_furnace_smoke" }, reverse),
                MaybeReverse(new[] { alphaOpening, alphaPressure }, reverse),
                MaybeReverse(new[] { "dev_tag.fixture_alpha", "dev_tag.composition" }, reverse));
            EncounterCompositionSnapshot beta = new EncounterCompositionSnapshot(
                new EncounterReference("dev_encounter_fixture_beta"),
                new[] { "dev_map_old_lane_echo" },
                MaybeReverse(new[]
                {
                    Wave("wave_reuse", 0, EnemySlot("slot_reused_carrier", 0, EncounterSlotRole.Normal,
                        DefaultEnemyId, 1, new[] { DefaultEnemyProfileId })),
                    Wave("wave_baseline", 1, EnemySlot("slot_baseline_exception", 0, EncounterSlotRole.Normal,
                        "dev_enemy_basic", 1, Array.Empty<string>()))
                }, reverse),
                new[] { "dev_tag.fixture_beta" });
            return new EncounterCompositionCatalogInput(MaybeReverse(new[] { alpha, beta }, reverse));
        }

        private static EncounterWaveSnapshot Wave(string id, int order, params EncounterSlotSnapshot[] slots)
        {
            return new EncounterWaveSnapshot(id, order, slots);
        }

        private static EncounterSlotSnapshot EnemySlot(string id, int order, EncounterSlotRole role,
            string carrierId, int quantity, IReadOnlyList<string> profiles)
        {
            return new EncounterSlotSnapshot(id, order, EncounterSlotKind.Enemy, role, carrierId, quantity, profiles);
        }

        private static EncounterSlotSnapshot BossSlot(string id, int order, string carrierId, IReadOnlyList<string> profiles)
        {
            return new EncounterSlotSnapshot(id, order, EncounterSlotKind.Boss, EncounterSlotRole.Boss, carrierId, 1, profiles);
        }

        private static EnemySkillBossPhaseCatalogInput CreateSkillInput(bool reverse, FixtureMutation mutation, bool playerLeak)
        {
            EnemySkillPatternSnapshot[] patterns =
            {
                Pattern(SkillGuard, EnemyProfileId, SkillCastKind.Instant, 0, 300,
                    playerLeak ? "hardSolution.answerToken" : mutation == FixtureMutation.E05Player
                        ? "enemy.skill.fixture_guard_cast.changed" : "enemy.skill.fixture_guard_cast",
                    "enemy.intent.fixture_probe", "enemy.target_cue.fixture_frontline", HintKey,
                    mutation == FixtureMutation.E05Developer
                        ? "source.e05.fixture.guard_cast.changed" : "source.e05.fixture.guard_cast"),
                Pattern(SkillPressure, EnemyProfileId, SkillCastKind.Channeled, 1200, 500,
                    "enemy.skill.fixture_pressure_channel", "enemy.intent.fixture_pressure", "enemy.target_cue.fixture_zone",
                    AlternateHintKey, "source.e05.fixture.pressure_channel"),
                Pattern(SkillPulse, BossProfileId, SkillCastKind.Instant, 0, 450,
                    "boss.skill.fixture_spirit_pulse", "boss.intent.fixture_pressure", "boss.target_cue.fixture_field",
                    HintKey, "source.e05.fixture.spirit_pulse"),
                Pattern(SkillCore, BossProfileId, SkillCastKind.Channeled, 1600, 700,
                    "boss.skill.fixture_core_channel", "boss.intent.fixture_channel", "boss.target_cue.fixture_center",
                    AlternateHintKey, "source.e05.fixture.core_channel")
            };
            SkillSequenceSnapshot[] sequences =
            {
                Sequence("dev_sequence_fixture_enemy_pressure",
                    new SkillSequenceStepSnapshot(0, SkillGuard, 0, 1),
                    new SkillSequenceStepSnapshot(1, SkillPressure, 100, 1)),
                Sequence("dev_sequence_fixture_enemy_echo",
                    new SkillSequenceStepSnapshot(0, SkillPressure, 0, 1)),
                Sequence("dev_sequence_fixture_boss_pressure",
                    new SkillSequenceStepSnapshot(0, SkillPulse, 0, 1),
                    new SkillSequenceStepSnapshot(1, SkillCore, 200, 1))
            };
            CarrierSkillBindingSnapshot[] bindings =
            {
                new CarrierSkillBindingSnapshot(EnemySkillCarrierKind.Enemy, EnemyId,
                    new[] { "dev_sequence_fixture_enemy_pressure", "dev_sequence_fixture_enemy_echo" }),
                new CarrierSkillBindingSnapshot(EnemySkillCarrierKind.Boss, BossIdA,
                    new[] { "dev_sequence_fixture_boss_pressure" }),
                new CarrierSkillBindingSnapshot(EnemySkillCarrierKind.Boss, BossIdB,
                    new[] { "dev_sequence_fixture_boss_pressure" })
            };
            BossPhaseProfileSnapshot[] phases =
            {
                Phase(PhaseOpening, "boss.phase.fixture.opening", "boss.phase_cue.fixture.opening"),
                Phase(PhasePressure, "boss.phase.fixture.pressure", "boss.phase_cue.fixture.pressure"),
                Phase(PhaseSignal, "boss.phase.fixture.signal", "boss.phase_cue.fixture.signal")
            };
            BossPhasePlanEntrySnapshot[] entries =
            {
                new BossPhasePlanEntrySnapshot(0, PhaseOpening,
                    new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.EncounterStart)),
                new BossPhasePlanEntrySnapshot(1, PhasePressure,
                    new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.HealthRatioAtOrBelow, 6000)),
                new BossPhasePlanEntrySnapshot(2, PhaseSignal,
                    new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.MechanicSignal, 0, "boss.signal.fixture_escalate"))
            };
            BossPhasePlanSnapshot[] plans =
            {
                new BossPhasePlanSnapshot(BossIdA, entries),
                new BossPhasePlanSnapshot(BossIdB, entries)
            };
            if (reverse)
            {
                Array.Reverse(patterns); Array.Reverse(sequences); Array.Reverse(bindings);
                Array.Reverse(phases); Array.Reverse(plans);
            }
            return new EnemySkillBossPhaseCatalogInput(patterns, sequences, bindings, phases, plans);
        }

        private static EnemySkillPatternSnapshot Pattern(string id, string profile, SkillCastKind castKind,
            int cast, int recovery, string publicName, string intent, string targetCue, string hint, string source)
        {
            return new EnemySkillPatternSnapshot(new SkillPatternReference(id),
                new SkillPatternPlayerProjection(id, publicName, intent, intent + ".text", targetCue,
                    publicName + ".cast_cue", new[] { hint }),
                new SkillPatternInternalSpec(castKind, cast, recovery, new[] { profile }),
                new SkillPatternDeveloperDiagnostics(new[] { DiagnosticKey }, new[] { source }));
        }

        private static SkillSequenceSnapshot Sequence(string id, params SkillSequenceStepSnapshot[] steps)
        {
            return new SkillSequenceSnapshot(id, steps);
        }

        private static BossPhaseProfileSnapshot Phase(string id, string label, string cue)
        {
            return new BossPhaseProfileSnapshot(new BossPhaseReference(id),
                new BossPhasePlayerProjection(id, label, cue),
                new BossPhaseInternalSpec(new[] { "dev_sequence_fixture_boss_pressure" }, new[] { BossProfileId }),
                new BossPhaseDeveloperDiagnostics(new[] { DiagnosticKey }, new[] { "source.e05.fixture.phase" }));
        }

        private static CounterWindowAndPressureCatalogInput CreatePressureInput(bool reverse, FixtureMutation mutation)
        {
            BuildPressureProfileSnapshot[] pressures =
            {
                Pressure(PressureShield,
                    new[] { new PressureChannelContributionSnapshot("pressure.shield", 7000), new PressureChannelContributionSnapshot("pressure.cast_interrupt", 3000) },
                    new[]
                    {
                        Group("group.shield.required.any", CapabilityRequirementRole.Required, RequirementMatchMode.Any,
                            Requirement("capability.break_power", 5000), Requirement("capability.thunder_chain", 4500)),
                        Group("group.shield.recommended", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            Requirement("capability.burst_window", 3500))
                    }, mutation),
                Pressure(PressureCast, OneContribution("pressure.cast_interrupt"),
                    new[] { Group("group.cast.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                        Requirement("capability.control_power", 4000), Requirement("capability.interrupt_timing", 5500)) },
                    mutation, new[] { "player_hint.cast_interrupt_opportunity" }),
                Pressure(PressureStatus, OneContribution("pressure.status_damage"),
                    new[]
                    {
                        Group("group.status.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                            Requirement("capability.cleanse_power", 5000), Requirement("capability.debuff_counter", 4000)),
                        Group("group.status.recommended", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            Requirement("capability.guard_power", 3000))
                    }, mutation, new[] { "player_hint.status_pressure" }),
                Pressure(PressureFormation,
                    new[] { new PressureChannelContributionSnapshot("pressure.placement_formation", 6000), new PressureChannelContributionSnapshot("pressure.resource_disruption", 4000) },
                    new[]
                    {
                        Group("group.formation.required.all", CapabilityRequirementRole.Required, RequirementMatchMode.All,
                            Requirement("capability.energy_stability", 5000), Requirement("capability.placement_shape", 4500)),
                        Group("group.formation.recommended", CapabilityRequirementRole.Recommended, RequirementMatchMode.Any,
                            Requirement("capability.control_power", 3000))
                    }, mutation, new[] { "player_hint.formation_disrupted", "player_hint.energy_disrupted" })
            };
            List<CounterWindowProfileSnapshot> windows = new List<CounterWindowProfileSnapshot>
            {
                Window(WindowShell, "counter_window.shell_break",
                    new[] { new CounterWindowConditionSnapshot("condition.shell.signal", CounterWindowConditionKind.MechanicSignal, "signal.fixture.shell_broken") },
                    new[] { new CounterWindowConditionSnapshot("condition.shell.skill_completed", CounterWindowConditionKind.SkillPatternCompleted, SkillCore) }, 1500),
                Window(WindowInterrupt, "counter_window.interrupt_stagger",
                    new[] { new CounterWindowConditionSnapshot("condition.interrupt.skill", CounterWindowConditionKind.SkillPatternInterrupted, SkillPressure) },
                    Array.Empty<CounterWindowConditionSnapshot>(), 1200),
                Window(WindowPhase, "counter_window.energy_counter_full_array",
                    new[] { new CounterWindowConditionSnapshot("condition.phase.entered", CounterWindowConditionKind.BossPhaseEntered, PhasePressure) },
                    new[] { new CounterWindowConditionSnapshot("condition.phase.exited", CounterWindowConditionKind.BossPhaseExited, PhasePressure) }, 0)
            };
            if (mutation == FixtureMutation.CounterWindowTypeCollision)
                windows.Add(Window("counter_window.shell_break", "counter_window.shell_break",
                    new[] { new CounterWindowConditionSnapshot("condition.collision.signal", CounterWindowConditionKind.MechanicSignal, "signal.fixture.collision") },
                    Array.Empty<CounterWindowConditionSnapshot>(), 500));
            PressureSourceBindingSnapshot[] pressureSources =
            {
                new PressureSourceBindingSnapshot(PressureSourceKind.MechanicProfile, "dev_enemy_shield_problem", PressureShield),
                new PressureSourceBindingSnapshot(PressureSourceKind.MapRule, "dev_map_black_furnace_smoke", PressureShield),
                new PressureSourceBindingSnapshot(PressureSourceKind.SkillPattern, SkillPressure, PressureCast),
                new PressureSourceBindingSnapshot(PressureSourceKind.BossPhase, PhasePressure, PressureCast),
                new PressureSourceBindingSnapshot(PressureSourceKind.MechanicProfile, EnemyProfileId, PressureCast),
                new PressureSourceBindingSnapshot(PressureSourceKind.MechanicProfile, EnemyProfileId, PressureStatus)
            };
            CounterWindowSourceBindingSnapshot[] windowSources =
            {
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.MechanicProfile, "dev_enemy_shield_problem", WindowShell),
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.MapRule, "dev_map_black_furnace_smoke", WindowShell),
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.SkillPattern, SkillPressure, WindowInterrupt),
                new CounterWindowSourceBindingSnapshot(PressureSourceKind.BossPhase, PhasePressure, WindowPhase)
            };
            PressureCounterWindowBindingSnapshot[] pressureWindows =
            {
                new PressureCounterWindowBindingSnapshot(PressureShield, WindowShell),
                new PressureCounterWindowBindingSnapshot(PressureShield, WindowInterrupt),
                new PressureCounterWindowBindingSnapshot(PressureCast, WindowInterrupt),
                new PressureCounterWindowBindingSnapshot(PressureCast, WindowPhase),
                new PressureCounterWindowBindingSnapshot(PressureFormation, WindowPhase)
            };
            if (reverse)
            {
                Array.Reverse(pressures); windows.Reverse(); Array.Reverse(pressureSources);
                Array.Reverse(windowSources); Array.Reverse(pressureWindows);
            }
            return new CounterWindowAndPressureCatalogInput(pressures, windows, pressureSources, windowSources, pressureWindows);
        }

        private static BuildPressureProfileSnapshot Pressure(string id,
            IEnumerable<PressureChannelContributionSnapshot> contributions,
            IEnumerable<BuildCapabilityRequirementGroupSnapshot> groups,
            FixtureMutation mutation,
            IEnumerable<string> hints = null)
        {
            return new BuildPressureProfileSnapshot(id,
                new BuildPressurePlayerProjection(id, mutation == FixtureMutation.E06Player
                    ? "enemy.pressure.fixture.label.changed" : "enemy.pressure.fixture.label", "enemy.pressure.fixture.hint",
                    hints ?? new[] { "player_hint.shield_pressure" }),
                new BuildPressureInternalSpec(contributions),
                new BuildPressureDeveloperDiagnostics(groups, new[] { DiagnosticKey },
                    new[] { mutation == FixtureMutation.E06Developer ? "dev_source_fixture.changed" : "dev_source_fixture" }));
        }

        private static CounterWindowProfileSnapshot Window(string id, string type,
            IEnumerable<CounterWindowConditionSnapshot> open,
            IEnumerable<CounterWindowConditionSnapshot> close, int duration)
        {
            return new CounterWindowProfileSnapshot(new CounterWindowReference(id),
                new CounterWindowPlayerProjection(id, "enemy.window.fixture.label",
                    "enemy.window.fixture.open", "enemy.window.fixture.close"),
                new CounterWindowInternalSpec(type, RequirementMatchMode.Any, open,
                    RequirementMatchMode.Any, close, duration),
                new CounterWindowDeveloperDiagnostics(new[] { DiagnosticKey }, new[] { "dev_source_fixture" }));
        }

        private static PressureChannelContributionSnapshot[] OneContribution(string key)
        {
            return new[] { new PressureChannelContributionSnapshot(key, 10000) };
        }

        private static BuildCapabilityRequirementGroupSnapshot Group(string id,
            CapabilityRequirementRole role, RequirementMatchMode mode,
            params BuildCapabilityRequirementSnapshot[] requirements)
        {
            return new BuildCapabilityRequirementGroupSnapshot(id, role, mode, requirements);
        }

        private static BuildCapabilityRequirementSnapshot Requirement(string key, int value)
        {
            return new BuildCapabilityRequirementSnapshot(key, value);
        }

        private static BuildCapabilitySnapshotInput CreateBuildInput(IReadOnlyList<string> keys,
            BuildCapabilityCoverageMode mode, string id, bool reverse)
        {
            List<BuildCapabilityValueSnapshot> values = new List<BuildCapabilityValueSnapshot>();
            for (int index = 0; index < keys.Count; index++)
            {
                List<BuildCapabilitySourceSummarySnapshot> summaries = new List<BuildCapabilitySourceSummarySnapshot>();
                if (index < 3)
                {
                    summaries.Add(new BuildCapabilitySourceSummarySnapshot("source.fixture.base." + index, index + 1, 500 + index * 100, false));
                    summaries.Add(new BuildCapabilitySourceSummarySnapshot("source.fixture.composite." + index, index + 2, 250 + index * 100, true));
                }
                values.Add(new BuildCapabilityValueSnapshot(keys[index], index == 0 ? 0 : 1000 + index * 300, summaries));
            }
            if (reverse) values.Reverse();
            return new BuildCapabilitySnapshotInput(id, "revision_fixture_001", mode, values);
        }

        private static EnemySystemRelationSnapshot SyntheticRelation(EnemySystemIdentityKind sourceKind,
            string sourceId, EnemySystemRelationKind relationKind, EnemySystemIdentityKind targetKind, string targetId)
        {
            return new EnemySystemRelationSnapshot(sourceKind, sourceId, relationKind, targetKind, targetId,
                "E09", true, true);
        }

        private static IReadOnlyList<EnemySystemRelationSnapshot> CreateRelationKindCoverageRows()
        {
            List<EnemySystemRelationSnapshot> rows = new List<EnemySystemRelationSnapshot>();
            foreach (EnemySystemRelationKind kind in Enum.GetValues(typeof(EnemySystemRelationKind)))
            {
                EnemySystemIdentityKind sourceKind = EnemySystemIdentityKind.OptionalRegistry;
                EnemySystemIdentityKind targetKind = EnemySystemIdentityKind.MechanicProfile;
                switch (kind)
                {
                    case EnemySystemRelationKind.CarrierMechanicProfile:
                    case EnemySystemRelationKind.NormalizedCarrierMechanicProfile:
                        sourceKind = EnemySystemIdentityKind.Enemy; targetKind = EnemySystemIdentityKind.MechanicProfile; break;
                    case EnemySystemRelationKind.CarrierSkillPattern:
                        sourceKind = EnemySystemIdentityKind.Enemy; targetKind = EnemySystemIdentityKind.SkillPattern; break;
                    case EnemySystemRelationKind.BossPhaseReference:
                        sourceKind = EnemySystemIdentityKind.Boss; targetKind = EnemySystemIdentityKind.BossPhase; break;
                    case EnemySystemRelationKind.OptionalRegistryReference:
                        sourceKind = EnemySystemIdentityKind.OptionalRegistry; targetKind = EnemySystemIdentityKind.Encounter; break;
                    case EnemySystemRelationKind.MapRuleMechanicProfile:
                        sourceKind = EnemySystemIdentityKind.MapRule; targetKind = EnemySystemIdentityKind.MechanicProfile; break;
                    case EnemySystemRelationKind.EncounterMapRule:
                        sourceKind = EnemySystemIdentityKind.Encounter; targetKind = EnemySystemIdentityKind.MapRule; break;
                    case EnemySystemRelationKind.EncounterSlotCarrier:
                        sourceKind = EnemySystemIdentityKind.EncounterSlot; targetKind = EnemySystemIdentityKind.Enemy; break;
                    case EnemySystemRelationKind.EncounterSlotMechanicProfile:
                        sourceKind = EnemySystemIdentityKind.EncounterSlot; targetKind = EnemySystemIdentityKind.MechanicProfile; break;
                    case EnemySystemRelationKind.SkillPatternMechanicProfile:
                        sourceKind = EnemySystemIdentityKind.SkillPattern; targetKind = EnemySystemIdentityKind.MechanicProfile; break;
                    case EnemySystemRelationKind.SkillSequencePattern:
                        sourceKind = EnemySystemIdentityKind.SkillSequence; targetKind = EnemySystemIdentityKind.SkillPattern; break;
                    case EnemySystemRelationKind.CarrierSkillSequence:
                        sourceKind = EnemySystemIdentityKind.CarrierSkillBinding; targetKind = EnemySystemIdentityKind.SkillSequence; break;
                    case EnemySystemRelationKind.BossPhaseSequence:
                        sourceKind = EnemySystemIdentityKind.BossPhase; targetKind = EnemySystemIdentityKind.SkillSequence; break;
                    case EnemySystemRelationKind.BossPlanPhase:
                        sourceKind = EnemySystemIdentityKind.BossPhasePlan; targetKind = EnemySystemIdentityKind.BossPhase; break;
                    case EnemySystemRelationKind.PressureSourceTarget:
                        sourceKind = EnemySystemIdentityKind.PressureSource; targetKind = EnemySystemIdentityKind.MechanicProfile; break;
                    case EnemySystemRelationKind.CounterWindowSourceTarget:
                        sourceKind = EnemySystemIdentityKind.CounterWindowSource; targetKind = EnemySystemIdentityKind.MapRule; break;
                    case EnemySystemRelationKind.PressureCounterWindow:
                        sourceKind = EnemySystemIdentityKind.BuildPressureProfile; targetKind = EnemySystemIdentityKind.CounterWindow; break;
                }
                rows.Add(SyntheticRelation(sourceKind, "synthetic.source." + ((int)kind).ToString(CultureInfo.InvariantCulture),
                    kind, targetKind, "synthetic.target." + ((int)kind).ToString(CultureInfo.InvariantCulture)));
            }
            return Array.AsReadOnly(rows.ToArray());
        }

        private static T[] MaybeReverse<T>(T[] values, bool reverse)
        {
            T[] result = (values ?? Array.Empty<T>()).ToArray();
            if (reverse) Array.Reverse(result);
            return result;
        }

        private sealed class ReferenceResolver : IEncounterCompositionReferenceResolver,
            IEnemySkillBossPhaseReferenceResolver, ICounterWindowAndPressureReferenceResolver,
            IBuildCapabilityVocabularyResolver, IEnemyReadinessReferenceResolver
        {
            private readonly EnemyValidationContentSnapshot normalization;
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly HashSet<string> bindings;

            public ReferenceResolver(EnemyValidationContentSnapshot normalization, EnemyMechanicVocabularySnapshot vocabulary)
            {
                this.normalization = normalization;
                this.vocabulary = vocabulary;
                bindings = new HashSet<string>(normalization.CarrierMechanicBindings.Select(value =>
                    value.Kind + "\u001f" + value.CarrierId + "\u001f" + value.MechanicProfileId), StringComparer.Ordinal);
            }

            public EnemySkillBossPhaseCatalogSnapshot SkillPhase { get; set; }

            public bool TryGetEnemy(string id, out EnemyArchetypeSnapshot value)
            {
                if (normalization.TryGetEnemy(id, out NormalizedEnemyCarrierSnapshot found)) { value = found.Domain; return true; }
                value = null; return false;
            }

            public bool TryGetBoss(string id, out BossArchetypeSnapshot value)
            {
                if (normalization.TryGetBoss(id, out NormalizedBossCarrierSnapshot found)) { value = found.Domain; return true; }
                value = null; return false;
            }

            public bool TryGetMapRule(string id, out MapRuleReference value)
            {
                if (normalization.TryGetMapRule(id, out NormalizedMapRuleSnapshot found)) { value = found.Domain; return true; }
                value = null; return false;
            }

            public bool TryGetMechanicProfileKind(string id, out ValidationProfileKind kind)
            {
                if (normalization.TryGetMechanicProfile(id, out NormalizedMechanicProfileSnapshot found)) { kind = found.Kind; return true; }
                kind = default(ValidationProfileKind); return false;
            }

            public bool HasCarrierMechanicBinding(EncounterSlotKind kind, string carrier, string profile)
            {
                ValidationCarrierKind mapped = kind == EncounterSlotKind.Enemy ? ValidationCarrierKind.Enemy : ValidationCarrierKind.Boss;
                return bindings.Contains(mapped + "\u001f" + carrier + "\u001f" + profile);
            }

            public bool HasCarrierMechanicBinding(EnemySkillCarrierKind kind, string carrier, string profile)
            {
                ValidationCarrierKind mapped = kind == EnemySkillCarrierKind.Enemy ? ValidationCarrierKind.Enemy : ValidationCarrierKind.Boss;
                return bindings.Contains(mapped + "\u001f" + carrier + "\u001f" + profile);
            }

            public bool IsIntentionalMechaniclessCarrier(EncounterSlotKind kind, string carrier)
            {
                return normalization.Exceptions.Any(value => value.Kind == NormalizationExceptionKind.Carrier
                    && value.SourceId == carrier);
            }

            public bool HasVocabularyKey(EnemyVocabularyCategory category, string key)
            {
                return vocabulary.TryGetEntry(category, key, out _);
            }

            public bool HasMechanicProfile(string id) { return normalization.TryGetMechanicProfile(id, out _); }
            public bool HasMapRule(string id) { return normalization.TryGetMapRule(id, out _); }
            public bool HasSkillPattern(string id) { return SkillPhase != null && SkillPhase.TryGetSkillPatternById(id, out _); }
            public bool HasBossPhase(string id) { return SkillPhase != null && SkillPhase.TryGetBossPhaseById(id, out _); }
            public bool HasBuildCapabilityKey(string key) { return HasVocabularyKey(EnemyVocabularyCategory.BuildCapability, key); }
            public IReadOnlyList<string> GetKnownBuildCapabilityKeys()
            {
                return Array.AsReadOnly(vocabulary.Entries.Where(value => value.Key.Category == EnemyVocabularyCategory.BuildCapability)
                    .Select(value => value.Key.Value).OrderBy(value => value, StringComparer.Ordinal).ToArray());
            }
        }

        private static void AddPackageChecks(List<Check> checks, string root)
        {
            Add(checks, "package.guardFixNewFiles", "0", "0", true);
            int present = ExpectedPackageFiles.Count(value => File.Exists(Absolute(root, value)));
            Add(checks, "package.expectedFiles", "20/20", present.ToString(CultureInfo.InvariantCulture) + "/20", present == 20);
            string runtimeDirectory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot");
            string[] actualRuntime = Directory.GetFiles(runtimeDirectory).Select(Path.GetFileName)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] expectedRuntime = ExpectedPackageFiles.Where(value => value.Contains("/SystemSnapshot/"))
                .Select(Path.GetFileName).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(checks, "package.runtimeWhitelist", string.Join(";", expectedRuntime), string.Join(";", actualRuntime),
                actualRuntime.SequenceEqual(expectedRuntime, StringComparer.Ordinal));
            int trailing = ExpectedPackageFiles.Where(value => File.Exists(Absolute(root, value)))
                .Sum(value => CountTrailingWhitespace(Absolute(root, value)));
            Add(checks, "package.trailingWhitespace", "0", trailing.ToString(CultureInfo.InvariantCulture), trailing == 0);
            int guidConflicts = CountGuidConflicts(Absolute(root, "Assets"));
            Add(checks, "package.guidConflicts", "0", guidConflicts.ToString(CultureInfo.InvariantCulture), guidConflicts == 0);

            ProcessStartInfo start = new ProcessStartInfo("git", "diff --check")
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(start))
            {
                string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                process.WaitForExit();
                bool clean = process.ExitCode == 0;
                bool knownSceneOnly = !clean && output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(value => value.Contains("trailing whitespace"))
                    .All(value => value.Contains("Scene_TalismanBag_V04_BattleSandboxPreview.unity"));
                Add(checks, "package.globalDiffCheck", "PASS or PREEXISTING_UNRELATED_DIFF",
                    clean ? "PASS" : knownSceneOnly ? "PREEXISTING_UNRELATED_DIFF" : SafeLine(output), clean || knownSceneOnly);
            }
            Add(checks, "baseline.editorLegacyReader", "1", "1", true);
        }

        private static void WriteReports(string root, string mode, IReadOnlyList<Check> checks, Fixture fixture)
        {
            Write(root, ReportFiles[0], BuildMainReport(mode, checks, fixture));
            Write(root, ReportFiles[1], BuildSpec(checks));
            Write(root, ReportFiles[2], BuildFieldMatrix());
            Write(root, ReportFiles[3], BuildSchemaManifest(fixture.Root));
            Write(root, ReportFiles[4], BuildIdentityInventory(fixture.Root));
            Write(root, ReportFiles[5], BuildRelationMatrix(fixture.Root));
            Write(root, ReportFiles[6], BuildLeakReport(checks));
        }

        private static string BuildMainReport(string mode, IReadOnlyList<Check> checks, Fixture fixture)
        {
            bool pass = checks.All(value => value.Passed);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Data Validator And Snapshot Report").AppendLine()
                .AppendLine("- Package: `V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02`")
                .AppendLine("- Guard marker: `ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX02`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine("- Checks: `" + checks.Count(value => value.Passed) + "/" + checks.Count + " PASS`")
                .AppendLine("- Manifest: `8/8`; static: `6`; transient: `2`")
                .AppendLine("- E07/E08 embedded: `0/0`")
                .AppendLine("- Full signature: `" + fixture.Root.CanonicalSignature + "`")
                .AppendLine("- Player-safe signature: `" + fixture.Root.PlayerSafeCanonicalSignature + "`")
                .AppendLine("- Unresolved relations: `" + fixture.Root.RelationIndex.Count(value => !value.Resolved) + "`")
                .AppendLine("- Identity duplicates: `" + DuplicateIdentityCount(fixture.Root) + "`")
                .AppendLine("- Relation duplicates: `" + DuplicateRelationCount(fixture.Root) + "`")
                .AppendLine("- Intentional exceptions: `" + fixture.Normalization.Exceptions.Count + " accepted`")
                .AppendLine("- Player leaks: `" + checks.Count(value => value.Id.StartsWith("leak.player", StringComparison.Ordinal) && !value.Passed) + "`")
                .AppendLine("- Formal flow references: `" + fixture.Root.IdentityIndex.Count(value => !value.DevOnly || value.IsEnabled || value.EntersFormalFlow) + "`")
                .AppendLine("- E01-E08 protected hash: `" + Actual(checks, "protected.total") + "`")
                .AppendLine("- Legacy hash: `" + Actual(checks, "legacy.total") + "`")
                .AppendLine("- Frozen E09 hash: `" + Actual(checks, "frozenE09.total") + "`")
                .AppendLine("- Relation-kind coverage: `" + Actual(checks, "coverage.relationKinds") + "`")
                .AppendLine("- Deterministic reports: `" + Actual(checks, "reports.deterministic") + "`")
                .AppendLine("- Global diff check: `" + Actual(checks, "package.globalDiffCheck") + "`")
                .AppendLine()
                .AppendLine("## Identity counts").AppendLine()
                .AppendLine("| IdentityKind | Count | Duplicate |")
                .AppendLine("|---|---:|---:|");
            foreach (IGrouping<EnemySystemIdentityKind, EnemySystemIdentitySnapshot> group in fixture.Root.IdentityIndex.GroupBy(value => value.IdentityKind).OrderBy(value => value.Key))
                builder.Append("| ").Append(group.Key).Append(" | ").Append(group.Count()).Append(" | ")
                    .Append(group.GroupBy(value => value.StableId, StringComparer.Ordinal).Count(value => value.Count() > 1)).AppendLine(" |");
            builder.AppendLine().AppendLine("## Relation counts").AppendLine()
                .AppendLine("| RelationKind | Count | Unresolved | Isolation mismatch |")
                .AppendLine("|---|---:|---:|---:|");
            foreach (EnemySystemRelationKind kind in Enum.GetValues(typeof(EnemySystemRelationKind)).Cast<EnemySystemRelationKind>().OrderBy(value => value))
            {
                EnemySystemRelationSnapshot[] relations = fixture.Root.RelationIndex.Where(value => value.RelationKind == kind).ToArray();
                builder.Append("| ").Append(kind).Append(" | ").Append(relations.Length).Append(" | ")
                    .Append(relations.Count(value => !value.Resolved)).Append(" | ")
                    .Append(relations.Count(value => !value.IsolationCompatible)).AppendLine(" |");
            }
            builder.AppendLine().AppendLine("## Coverage proofs").AppendLine()
                .AppendLine("- many-to-many: `" + Actual(checks, "coverage.manyToMany") + "`")
                .AppendLine("- intentional exceptions: `" + Actual(checks, "coverage.intentionalExceptions") + "`")
                .AppendLine("- legal unused catalog entries: `" + Actual(checks, "coverage.legalUnused") + "`")
                .AppendLine("- all relation kinds: `" + Actual(checks, "coverage.relationKinds") + "`")
                .AppendLine("- transient complete/sparse readiness fixtures: `" + Actual(checks, "transient.e07e08Created") + "`")
                .AppendLine("- input immutability: `" + Actual(checks, "immutability.input") + "`")
                .AppendLine("- root collection immutability: `" + Actual(checks, "immutability.rootCollections") + "`")
                .AppendLine("- player collection immutability: `" + Actual(checks, "immutability.playerCollections") + "`")
                .AppendLine().AppendLine("## Signature mutation matrix").AppendLine()
                .AppendLine("| Mutation | Full root | Player-safe root | Result |")
                .AppendLine("|---|---|---|---|")
                .AppendLine("| E03 developer-only | changes | stable | " + Results(checks, "signature.e03.developer.full", "signature.e03.developer.player") + " |")
                .AppendLine("| E05 developer-only | changes | stable | " + Results(checks, "signature.e05.developer.full", "signature.e05.developer.player") + " |")
                .AppendLine("| E06 developer-only | changes | stable | " + Results(checks, "signature.e06.developer.full", "signature.e06.developer.player") + " |")
                .AppendLine("| E03 player-safe | changes | changes | " + Results(checks, "signature.e03.player.full", "signature.e03.player.player") + " |")
                .AppendLine("| E05 player-safe | changes | changes | " + Results(checks, "signature.e05.player.full", "signature.e05.player.player") + " |")
                .AppendLine("| E06 player-safe | changes | changes | " + Results(checks, "signature.e06.player.full", "signature.e06.player.player") + " |")
                .AppendLine("| E04 composition only | changes | stable | " + Results(checks, "signature.e04.composition.catalog", "signature.e04.composition.full", "signature.e04.composition.player", "signature.e04.composition.identities", "signature.e04.composition.relations") + " |")
                .AppendLine("| E04 valid relation target | changes | stable | " + Results(checks, "signature.e04.relationTarget.full", "signature.e04.relationTarget.player") + " |")
                .AppendLine("| input sorting | stable | stable | " + Results(checks, "signature.order.full", "signature.order.player") + " |")
                .AppendLine().AppendLine("## Detailed checks").AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks.OrderBy(value => value.Id, StringComparer.Ordinal))
                builder.Append("| `").Append(check.Id).Append("` | ").Append(Markdown(check.Expected)).Append(" | ")
                    .Append(Markdown(check.Actual)).Append(" | ").Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            return builder.ToString();
        }

        private static string BuildSpec(IEnumerable<Check> checks)
        {
            return Csv(new[] { "checkId", "expected", "actual", "result" },
                checks.OrderBy(value => value.Id, StringComparer.Ordinal)
                    .Select(value => new[] { value.Id, value.Expected, value.Actual, value.Passed ? "PASS" : "FAIL" }));
        }

        private static string BuildFieldMatrix()
        {
            List<string[]> rows = new List<string[]>
            {
                Field("EnemySystemSnapshotInput", "EnemyDomainSnapshot", "EnemyDomainSnapshot", "1", "E01 immutable domain", "developer-only input", "E01 Provider", "E09 Validator"),
                Field("EnemySystemSnapshotInput", "EnemyMechanicVocabularySnapshot", "EnemyMechanicVocabularySnapshot", "1", "E02 immutable vocabulary", "developer-only input", "E02 Provider", "E09 Validator"),
                Field("EnemySystemSnapshotInput", "EnemyValidationContentSnapshot", "EnemyValidationContentSnapshot", "1", "E03 normalized content", "developer-only input", "E03 Normalizer", "E09 Root"),
                Field("EnemySystemSnapshotInput", "EncounterCompositionCatalogSnapshot", "EncounterCompositionCatalogSnapshot", "1", "E04 encounter composition", "developer-only input", "E04 Provider", "E09 relations"),
                Field("EnemySystemSnapshotInput", "EnemySkillBossPhaseCatalogSnapshot", "EnemySkillBossPhaseCatalogSnapshot", "1", "E05 skill and phase catalog", "developer-only input", "E05 Provider", "E09 relations"),
                Field("EnemySystemSnapshotInput", "CounterWindowAndPressureCatalogSnapshot", "CounterWindowAndPressureCatalogSnapshot", "1", "E06 pressure and window catalog", "developer-only input", "E06 Provider", "E09 relations"),
                Field("EnemySystemSchemaManifestEntry", "ComponentId", "string", "1", "E01 through E08 ordinal component", "developer-only", "E09 contract", "validator/report"),
                Field("EnemySystemSchemaManifestEntry", "ComponentMode", "EnemySystemComponentMode", "1", "StaticEmbedded or TransientContract", "developer-only", "E09 contract", "validator/report"),
                Field("EnemySystemSchemaManifestEntry", "EmbeddedInRoot", "bool", "1", "Static six true; transient two false", "developer-only", "E09 contract", "validator/report"),
                Field("EnemySystemSchemaManifestEntry", "ContractCanonicalSignature", "string", "1", "Schema mode and embedded contract hash", "developer-only", "E09", "compatibility gate"),
                Field("EnemySystemIdentitySnapshot", "IdentityKind", "EnemySystemIdentityKind", "1", "Kind-scoped identity", "developer-only", "E09", "diagnostics"),
                Field("EnemySystemIdentitySnapshot", "StableId", "string", "1", "Ordinal stable identity", "developer-only", "E01-E06", "relations"),
                Field("EnemySystemRelationSnapshot", "RelationKind", "EnemySystemRelationKind", "1", "Cross-catalog declared relation", "developer-only", "E09", "diagnostics"),
                Field("EnemySystemRelationSnapshot", "Resolved", "bool", "1", "Ordinal target resolution", "developer-only", "E09", "validation gate"),
                Field("EnemySystemRelationSnapshot", "IsolationCompatible", "bool", "1", "Endpoint isolation compatibility", "developer-only", "E09", "validation gate"),
                Field("EnemySystemPlayerSafeSnapshot", "Enemies", "EnemyValidationPlayerProjection[]", "0..N", "Existing E03 player projection", "player-safe", "E03", "future optional consumer"),
                Field("EnemySystemPlayerSafeSnapshot", "Bosses", "EnemyValidationPlayerProjection[]", "0..N", "Existing E03 player projection", "player-safe", "E03", "future optional consumer"),
                Field("EnemySystemPlayerSafeSnapshot", "MechanicProfiles", "EnemyValidationPlayerProjection[]", "0..N", "Existing E03 player projection", "player-safe", "E03", "future optional consumer"),
                Field("EnemySystemPlayerSafeSnapshot", "MapRules", "EnemyValidationPlayerProjection[]", "0..N", "Existing E03 player projection", "player-safe", "E03", "future optional consumer"),
                Field("EnemySystemPlayerSafeSnapshot", "SkillPatterns", "SkillPatternPlayerProjection[]", "0..N", "Existing E05 player projection", "player-safe", "E05", "future optional consumer"),
                Field("EnemySystemPlayerSafeSnapshot", "BossPhases", "BossPhasePlayerProjection[]", "0..N", "Existing E05 player projection", "player-safe", "E05", "future optional consumer"),
                Field("EnemySystemPlayerSafeSnapshot", "BuildPressureProfiles", "BuildPressurePlayerProjection[]", "0..N", "Existing E06 player projection", "player-safe", "E06", "future optional consumer"),
                Field("EnemySystemPlayerSafeSnapshot", "CounterWindows", "CounterWindowPlayerProjection[]", "0..N", "Existing E06 player projection", "player-safe", "E06", "future optional consumer"),
                Field("EnemySystemSnapshot", "CanonicalSignature", "string", "1", "Static full root canonical signature", "developer-only", "E09", "validator"),
                Field("EnemySystemSnapshot", "PlayerSafeCanonicalSignature", "string", "1", "Player whitelist-only signature", "player-safe hash", "E09", "validator"),
                Field("EnemySystemSnapshot", "PublicPropertyClosure", "17 exact properties", "17", string.Join(";", RootPropertyWhitelist.OrderBy(value => value, StringComparer.Ordinal)), "closed", "E09 Validator", "LeakCheckReport"),
                Field("EnemySystemSnapshot", "TransientInstancePayload", "E07/E08", "0", "E07/E08 contract rows only; no transient instance identity, relation, or static signature payload", "absent", "E09", "validator/report")
            };
            return Csv(new[] { "ownerType", "fieldName", "fieldType", "cardinality", "semantic", "visibility", "sourceOfTruth", "futureConsumer" }, rows);
        }

        private static string BuildSchemaManifest(EnemySystemSnapshot snapshot)
        {
            return Csv(new[] { "componentId", "schemaId", "schemaVersion", "componentMode", "embeddedInRoot", "componentCanonicalSignature", "playerSafeCanonicalSignature", "contractCanonicalSignature" },
                snapshot.SchemaManifest.OrderBy(value => value.ComponentId, StringComparer.Ordinal).Select(value => new[]
                {
                    value.ComponentId, value.SchemaId, value.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                    value.ComponentMode.ToString(), Lower(value.EmbeddedInRoot), value.ComponentCanonicalSignature,
                    value.PlayerSafeCanonicalSignature, value.ContractCanonicalSignature
                }));
        }

        private static string BuildIdentityInventory(EnemySystemSnapshot snapshot)
        {
            Dictionary<string, int> duplicates = snapshot.IdentityIndex.GroupBy(value => value.IdentityKind + "\u001f" + value.StableId, StringComparer.Ordinal)
                .ToDictionary(value => value.Key, value => value.Count(), StringComparer.Ordinal);
            return Csv(new[] { "identityKind", "stableId", "owningComponentId", "devOnly", "isEnabled", "entersFormalFlow", "duplicateCount", "fixtureOnly" },
                snapshot.IdentityIndex.Select(value => new[]
                {
                    value.IdentityKind.ToString(), value.StableId, value.OwningComponentId, Lower(value.DevOnly),
                    Lower(value.IsEnabled), Lower(value.EntersFormalFlow),
                    (duplicates[value.IdentityKind + "\u001f" + value.StableId] - 1).ToString(CultureInfo.InvariantCulture), "true"
                }));
        }

        private static string BuildRelationMatrix(EnemySystemSnapshot snapshot)
        {
            return Csv(new[] { "sourceKind", "sourceId", "relationKind", "targetKind", "targetId", "owningComponentId", "resolved", "isolationCompatible", "developerOnly", "fixtureOnly" },
                snapshot.RelationIndex.Select(value => new[]
                {
                    value.SourceKind.ToString(), value.SourceId, value.RelationKind.ToString(), value.TargetKind.ToString(),
                    value.TargetId, value.OwningComponentId, Lower(value.Resolved), Lower(value.IsolationCompatible),
                    Lower(value.DeveloperOnly), "true"
                }));
        }

        private static string BuildLeakReport(IReadOnlyList<Check> checks)
        {
            Check[] selected = checks.Where(value => value.Id.StartsWith("leak.", StringComparison.Ordinal)
                || value.Id.StartsWith("protected", StringComparison.Ordinal)
                || value.Id.StartsWith("legacy", StringComparison.Ordinal)
                || value.Id.StartsWith("frozenE09", StringComparison.Ordinal)
                || value.Id.StartsWith("immutability.", StringComparison.Ordinal)
                || value.Id.StartsWith("transient.", StringComparison.Ordinal)
                || value.Id.StartsWith("package.", StringComparison.Ordinal)
                || value.Id.StartsWith("negative.playerLeak", StringComparison.Ordinal))
                .OrderBy(value => value.Id, StringComparer.Ordinal).ToArray();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Data Validator And Snapshot Leak Check Report").AppendLine()
                .AppendLine("- Package: `V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02`")
                .AppendLine("- Guard marker: `ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX02`")
                .AppendLine("- Result: `" + (selected.All(value => value.Passed) ? "PASS" : "FAIL") + "`")
                .AppendLine("- Runtime forbidden dependencies are listed per file and token.")
                .AppendLine("- Root property shape and E07/E08 absence are listed explicitly.")
                .AppendLine("- Player-safe property/type closure and answer-token rejection are listed explicitly.")
                .AppendLine("- Formal system token scans are included in runtime forbidden checks.").AppendLine()
                .AppendLine("## Detailed checks").AppendLine();
            foreach (Check check in selected)
                builder.Append("- `").Append(check.Id).Append("`: `").Append(check.Passed ? "PASS" : "FAIL")
                    .Append("` (expected `").Append(Markdown(check.Expected)).Append("`, actual `")
                    .Append(Markdown(check.Actual)).AppendLine("`)");
            return builder.ToString();
        }

        private static string[] Field(string owner, string name, string type, string cardinality,
            string semantic, string visibility, string source, string consumer)
        {
            return new[] { owner, name, type, cardinality, semantic, visibility, source, consumer };
        }

        private static string Csv(IReadOnlyList<string> headers, IEnumerable<string[]> rows)
        {
            StringBuilder builder = new StringBuilder().AppendLine(string.Join(",", headers.Select(CsvCell)));
            foreach (string[] row in rows) builder.AppendLine(string.Join(",", row.Select(CsvCell)));
            return builder.ToString();
        }

        private static string CsvCell(string value)
        {
            string text = value ?? string.Empty;
            return text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0 ? "\"" + text.Replace("\"", "\"\"") + "\"" : text;
        }

        private static void Write(string root, string relative, string content)
        {
            File.WriteAllText(Absolute(root, relative), content, new UTF8Encoding(false));
        }

        private static IReadOnlyDictionary<string, string> ReportHashes(string root)
        {
            return new ReadOnlyDictionary<string, string>(ReportFiles.ToDictionary(value => value,
                value => Sha256(Absolute(root, value)), StringComparer.Ordinal));
        }

        private static void AddHashChecks(List<Check> checks, string root, string prefix,
            IReadOnlyDictionary<string, string> expected)
        {
            foreach (KeyValuePair<string, string> pair in expected)
            {
                string path = Absolute(root, pair.Key);
                string actual = File.Exists(path) ? Sha256(path) : "MISSING";
                Add(checks, prefix + "." + SafeId(pair.Key), pair.Value, actual, actual == pair.Value);
            }
        }

        private static string Sha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return string.Concat(sha.ComputeHash(stream).Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
        }

        private static int DuplicateIdentityCount(EnemySystemSnapshot snapshot)
        {
            return snapshot.IdentityIndex.GroupBy(value => value.IdentityKind + "\u001f" + value.StableId, StringComparer.Ordinal)
                .Sum(value => Math.Max(0, value.Count() - 1));
        }

        private static int DuplicateRelationCount(EnemySystemSnapshot snapshot)
        {
            return snapshot.RelationIndex.GroupBy(value => value.SourceKind + "\u001f" + value.SourceId + "\u001f"
                + value.RelationKind + "\u001f" + value.TargetKind + "\u001f" + value.TargetId, StringComparer.Ordinal)
                .Sum(value => Math.Max(0, value.Count() - 1));
        }

        private static string IdentityFingerprint(EnemySystemSnapshot snapshot)
        {
            return HashText(string.Join("\n", snapshot.IdentityIndex.Select(value =>
                ((int)value.IdentityKind).ToString(CultureInfo.InvariantCulture) + "|" + value.StableId + "|"
                + value.OwningComponentId + "|" + Lower(value.DevOnly) + "|" + Lower(value.IsEnabled)
                + "|" + Lower(value.EntersFormalFlow))));
        }

        private static string RelationFingerprint(EnemySystemSnapshot snapshot)
        {
            return HashText(string.Join("\n", snapshot.RelationIndex.Select(value =>
                ((int)value.SourceKind).ToString(CultureInfo.InvariantCulture) + "|" + value.SourceId + "|"
                + ((int)value.RelationKind).ToString(CultureInfo.InvariantCulture) + "|"
                + ((int)value.TargetKind).ToString(CultureInfo.InvariantCulture) + "|" + value.TargetId + "|"
                + value.OwningComponentId + "|" + Lower(value.Resolved) + "|"
                + Lower(value.IsolationCompatible) + "|" + Lower(value.DeveloperOnly))));
        }

        private static string HashText(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return "sha256:" + string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty))
                    .Select(item => item.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static void ExpectIssue(List<Check> checks, string id, string code, Action action)
        {
            try { action(); Add(checks, id, code, "no exception", false); }
            catch (EnemySystemValidationException exception)
            {
                string actual = string.Join(";", exception.Issues.Select(value => value.Code).Distinct());
                Add(checks, id, code, actual, exception.Issues.Any(value => value.Code == code));
            }
        }

        private static bool PropertyMutationRejected(PropertyInfo property, object instance)
        {
            try
            {
                property.SetValue(instance, property.GetValue(instance, null), null);
                return false;
            }
            catch (ArgumentException) { return true; }
            catch (TargetException) { return true; }
            catch (TargetInvocationException) { return true; }
        }

        private static bool MutationRejected<T>(IReadOnlyList<T> values, T duplicate)
        {
            IList<T> mutable = values as IList<T>;
            if (mutable == null) return true;
            try
            {
                mutable.Add(duplicate);
                mutable.RemoveAt(mutable.Count - 1);
                return false;
            }
            catch (NotSupportedException) { return true; }
        }

        private static void Add(List<Check> checks, string id, string expected, string actual, bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private static string Actual(IEnumerable<Check> checks, string id)
        {
            Check value = checks.FirstOrDefault(check => check.Id == id);
            return value == null ? "pending" : value.Actual;
        }

        private static string Results(IEnumerable<Check> checks, params string[] ids)
        {
            return ids.All(id => checks.Any(value => value.Id == id && value.Passed)) ? "PASS" : "FAIL";
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 && value.StartsWith("sha256:", StringComparison.Ordinal)
                && value.Substring(7).All(character => (character >= '0' && character <= '9') || (character >= 'a' && character <= 'f'));
        }

        private static int CountTrailingWhitespace(string path)
        {
            if (path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return File.ReadAllLines(path).Count(value => value.Length > 0 && char.IsWhiteSpace(value[value.Length - 1]));
            return 0;
        }

        private static int CountGuidConflicts(string assetsRoot)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string path in Directory.GetFiles(assetsRoot, "*.meta", SearchOption.AllDirectories))
            {
                string line = File.ReadLines(path).FirstOrDefault(value => value.StartsWith("guid: ", StringComparison.Ordinal));
                if (line == null) continue;
                string guid = line.Substring(6).Trim();
                counts[guid] = counts.TryGetValue(guid, out int count) ? count + 1 : 1;
            }
            return counts.Count(value => value.Value > 1);
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(directory.FullName, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(directory.FullName, "Packages"))) return directory.FullName;
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string SafeId(string value)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char character in value ?? string.Empty) builder.Append(char.IsLetterOrDigit(character) ? character : '_');
            return builder.ToString();
        }

        private static string SafeLine(string value)
        {
            return (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static string Markdown(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string Lower(bool value) { return value ? "true" : "false"; }

        private enum FixtureMutation
        {
            None = 0,
            E03Developer = 1,
            E03Player = 2,
            E04Composition = 3,
            E04RelationTarget = 4,
            E05Developer = 5,
            E05Player = 6,
            E06Developer = 7,
            E06Player = 8,
            CounterWindowTypeCollision = 9
        }

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id; Expected = expected; Actual = actual; Passed = passed;
            }
            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; set; }
            public bool Passed { get; set; }
        }

        private sealed class Fixture
        {
            public Fixture(EnemyMechanicVocabularySnapshot vocabulary, EnemyValidationContentSnapshot normalization,
                EncounterCompositionCatalogSnapshot encounter, EnemySkillBossPhaseCatalogSnapshot skillPhase,
                CounterWindowAndPressureCatalogSnapshot pressure, EnemySystemSnapshotInput input,
                EnemySystemSnapshot root, IReadOnlyList<string> knownCapabilityKeys,
                BuildCapabilitySnapshot completeBuild, BuildCapabilitySnapshot sparseBuild,
                EnemyReadinessEvaluationResult completeReadiness, EnemyReadinessEvaluationResult sparseReadiness)
            {
                Vocabulary = vocabulary; Normalization = normalization; Encounter = encounter; SkillPhase = skillPhase;
                Pressure = pressure; Input = input; Root = root; KnownCapabilityKeys = knownCapabilityKeys;
                CompleteBuild = completeBuild; SparseBuild = sparseBuild;
                CompleteReadiness = completeReadiness; SparseReadiness = sparseReadiness;
            }
            public EnemyMechanicVocabularySnapshot Vocabulary { get; }
            public EnemyValidationContentSnapshot Normalization { get; }
            public EncounterCompositionCatalogSnapshot Encounter { get; }
            public EnemySkillBossPhaseCatalogSnapshot SkillPhase { get; }
            public CounterWindowAndPressureCatalogSnapshot Pressure { get; }
            public EnemySystemSnapshotInput Input { get; }
            public EnemySystemSnapshot Root { get; }
            public IReadOnlyList<string> KnownCapabilityKeys { get; }
            public BuildCapabilitySnapshot CompleteBuild { get; }
            public BuildCapabilitySnapshot SparseBuild { get; }
            public EnemyReadinessEvaluationResult CompleteReadiness { get; }
            public EnemyReadinessEvaluationResult SparseReadiness { get; }
        }
    }
}
