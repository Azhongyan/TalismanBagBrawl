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
using TalismanBag.EnemySystem.SkillPhase;
using TalismanBag.EnemySystem.Vocabulary;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class EnemySkillBossPhaseSchemaVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const string EnemyId = "dev_enemy_caster_chanter";
        private const string EnemyProfileId = "dev_enemy_caster_problem";
        private const string BossIdA = "dev_boss_caster_zhenhun";
        private const string BossIdB = "dev_boss_energy_juneng";
        private const string BossProfileId = "dev_boss_spirit_thief_core";
        private const string HintKey = "player_hint.cast_interrupt_opportunity";
        private const string AlternateHintKey = "player_hint.burst_incoming";
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
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs"] = "894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0"
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
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhasePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhasePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillPatternSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillPatternSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/BossPhaseSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/BossPhaseSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhaseValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhaseValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs.meta",
            "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaReport.md",
            "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaSpec.csv",
            "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaFieldMatrix.csv",
            "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaFixtureRows.csv",
            "Docs/V0.4/Reports/EnemySkillBossPhaseSchemaLeakCheckReport.md"
        };

        private static readonly string[] RuntimeForbiddenTokens =
        {
            "TalismanBag.BuildSandbox",
            "TalismanBag.Battle",
            "TalismanBag.Item",
            "ItemSystem",
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
            "Reward",
            "SaveData",
            "PlayerPrefs",
            "V02FormationGridFrame",
            "DamageText",
            "BuildReadiness",
            "CounterWindow",
            "BuildPressure",
            "requiredCapability",
            "hardSolution",
            "BossKey",
            "weaknessThreshold",
            "DropBias"
        };

        private static readonly string[] TimerAndExecutionTokens =
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
            "currentHp",
            "currentHealth",
            "SelectTarget",
            "ApplyDamage",
            "DealDamage",
            "ChangePhase",
            "TransitionPhase"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/EnemySkillBossPhaseSchema01/[QA Only] Verify And Write Reports")]
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
            EnemySkillBossPhaseCatalogSnapshot catalog = null;
            EnemyValidationContentSnapshot references = null;
            try
            {
                references = EnemyValidationContentNormalizer.CreateSnapshot();
                EnemyMechanicVocabularySnapshot vocabulary =
                    DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                        DefaultEnemyMechanicVocabularyCatalog.CreateInput());
                E02E03ReferenceResolver resolver =
                    new E02E03ReferenceResolver(references, vocabulary);
                catalog = DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(
                    CreateFixture(),
                    resolver);
                RunChecks(checks, root, references, resolver, catalog);
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
                (pass ? "ENEMY_SKILL_BOSS_PHASE_SCHEMA_PASS " : "ENEMY_SKILL_BOSS_PHASE_SCHEMA_FAIL ")
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
                    "EnemySkillBossPhaseSchema01 verifier failed: "
                    + string.Join(
                        " | ",
                        checks.Where(value => !value.Passed)
                            .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static void RunChecks(
            List<Check> checks,
            string root,
            EnemyValidationContentSnapshot references,
            E02E03ReferenceResolver resolver,
            EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            Add(checks, "schema.id", EnemySkillBossPhaseSchema.SchemaId, catalog.SchemaId,
                string.Equals(catalog.SchemaId, EnemySkillBossPhaseSchema.SchemaId, StringComparison.Ordinal));
            Add(checks, "schema.version", "1", catalog.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                catalog.SchemaVersion == EnemySkillBossPhaseSchema.SchemaVersion);
            Add(checks, "fixture.patterns", ">=4", catalog.SkillPatterns.Count.ToString(CultureInfo.InvariantCulture),
                catalog.SkillPatterns.Count >= 4);
            Add(checks, "fixture.sequences", ">=3", catalog.SkillSequences.Count.ToString(CultureInfo.InvariantCulture),
                catalog.SkillSequences.Count >= 3);
            Add(checks, "fixture.carrierBindings", ">=2 with Enemy and Boss",
                catalog.CarrierSkillBindings.Count + " bindings",
                catalog.CarrierSkillBindings.Count >= 2
                    && catalog.CarrierSkillBindings.Any(value => value.CarrierKind == EnemySkillCarrierKind.Enemy)
                    && catalog.CarrierSkillBindings.Any(value => value.CarrierKind == EnemySkillCarrierKind.Boss));
            Add(checks, "fixture.phaseProfiles", ">=3", catalog.BossPhaseProfiles.Count.ToString(CultureInfo.InvariantCulture),
                catalog.BossPhaseProfiles.Count >= 3);
            Add(checks, "fixture.bossPlans", ">=2", catalog.BossPhasePlans.Count.ToString(CultureInfo.InvariantCulture),
                catalog.BossPhasePlans.Count >= 2);
            Add(checks, "fixture.instant", ">=1", catalog.SkillPatterns.Count(value => value.InternalOnly.SkillCastKind == SkillCastKind.Instant).ToString(CultureInfo.InvariantCulture),
                catalog.SkillPatterns.Any(value => value.InternalOnly.SkillCastKind == SkillCastKind.Instant));
            Add(checks, "fixture.channeled", ">=1", catalog.SkillPatterns.Count(value => value.InternalOnly.SkillCastKind == SkillCastKind.Channeled).ToString(CultureInfo.InvariantCulture),
                catalog.SkillPatterns.Any(value => value.InternalOnly.SkillCastKind == SkillCastKind.Channeled));

            int patternReuse = catalog.SkillSequences
                .SelectMany(sequence => sequence.Steps.Select(step => new { sequence.SkillSequenceId, step.SkillPatternId }))
                .GroupBy(value => value.SkillPatternId, StringComparer.Ordinal)
                .Count(group => group.Select(value => value.SkillSequenceId).Distinct(StringComparer.Ordinal).Count() > 1);
            Add(checks, "reuse.pattern", ">=1", patternReuse.ToString(CultureInfo.InvariantCulture), patternReuse >= 1);

            HashSet<string> carrierSequenceIds = new HashSet<string>(
                catalog.CarrierSkillBindings.SelectMany(value => value.SkillSequenceIds),
                StringComparer.Ordinal);
            HashSet<string> phaseSequenceIds = new HashSet<string>(
                catalog.BossPhaseProfiles.SelectMany(value => value.InternalOnly.SkillSequenceIds),
                StringComparer.Ordinal);
            int sequenceReuse = carrierSequenceIds.Intersect(phaseSequenceIds, StringComparer.Ordinal).Count();
            Add(checks, "reuse.sequence", ">=1", sequenceReuse.ToString(CultureInfo.InvariantCulture), sequenceReuse >= 1);

            int phaseReuse = catalog.BossPhasePlans
                .SelectMany(plan => plan.Entries.Select(entry => new { plan.BossId, entry.BossPhaseId }))
                .GroupBy(value => value.BossPhaseId, StringComparer.Ordinal)
                .Count(group => group.Select(value => value.BossId).Distinct(StringComparer.Ordinal).Count() > 1);
            Add(checks, "reuse.phase", ">=1", phaseReuse.ToString(CultureInfo.InvariantCulture), phaseReuse >= 1);

            HashSet<BossPhaseEntryConditionKind> conditionKinds = new HashSet<BossPhaseEntryConditionKind>(
                catalog.BossPhasePlans.SelectMany(plan => plan.Entries)
                    .Select(entry => entry.EntryCondition.Kind));
            Add(checks, "fixture.conditions", "EncounterStart;HealthRatioAtOrBelow;MechanicSignal",
                string.Join(";", conditionKinds.OrderBy(value => value)),
                conditionKinds.Contains(BossPhaseEntryConditionKind.EncounterStart)
                    && conditionKinds.Contains(BossPhaseEntryConditionKind.HealthRatioAtOrBelow)
                    && conditionKinds.Contains(BossPhaseEntryConditionKind.MechanicSignal));

            RunLookupChecks(checks, catalog);
            RunImmutabilityChecks(checks, resolver, catalog);
            RunSignatureChecks(checks, resolver, catalog);
            RunValidationRejectionChecks(checks, resolver);
            RunBaselineChecks(checks, root, references);
            RunLeakChecks(checks, root, catalog);
            AddHashChecks(checks, root, ProtectedHashes, "protected.e01e02e03e04", 18);
            AddHashChecks(checks, root, LegacyHashes, "protected.legacy", 3);
        }

        private static void RunLookupChecks(
            List<Check> checks,
            EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            const string patternId = "dev_skill_fixture_guard_cast";
            const string sequenceId = "dev_sequence_fixture_enemy_pressure";
            const string phaseId = "dev_phase_fixture_opening";
            Add(checks, "lookup.pattern.ordinal", "exact=true;case=false", string.Empty,
                catalog.TryGetSkillPatternById(patternId, out _)
                    && !catalog.TryGetSkillPatternById(patternId.ToUpperInvariant(), out _));
            Add(checks, "lookup.sequence.ordinal", "exact=true;case=false", string.Empty,
                catalog.TryGetSkillSequenceById(sequenceId, out _)
                    && !catalog.TryGetSkillSequenceById(sequenceId.ToUpperInvariant(), out _));
            Add(checks, "lookup.binding.ordinal", "exact=true;case=false", string.Empty,
                catalog.TryGetCarrierSkillBinding(EnemySkillCarrierKind.Enemy, EnemyId, out _)
                    && !catalog.TryGetCarrierSkillBinding(
                        EnemySkillCarrierKind.Enemy,
                        EnemyId.ToUpperInvariant(),
                        out _));
            Add(checks, "lookup.phase.ordinal", "exact=true;case=false", string.Empty,
                catalog.TryGetBossPhaseById(phaseId, out _)
                    && !catalog.TryGetBossPhaseById(phaseId.ToUpperInvariant(), out _));
            Add(checks, "lookup.plan.ordinal", "exact=true;case=false", string.Empty,
                catalog.TryGetBossPhasePlanByBossId(BossIdA, out _)
                    && !catalog.TryGetBossPhasePlanByBossId(BossIdA.ToUpperInvariant(), out _));
        }

        private static void RunImmutabilityChecks(
            List<Check> checks,
            E02E03ReferenceResolver resolver,
            EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            bool topReadOnly = IsReadOnly(catalog.SkillPatterns)
                && IsReadOnly(catalog.SkillSequences)
                && IsReadOnly(catalog.CarrierSkillBindings)
                && IsReadOnly(catalog.BossPhaseProfiles)
                && IsReadOnly(catalog.BossPhasePlans);
            Add(checks, "immutable.top.collections", "read-only", string.Empty, topReadOnly);

            bool nestedReadOnly = catalog.SkillPatterns.All(value =>
                    IsReadOnly(value.PlayerSafe.PlayerHintCategoryKeys)
                    && IsReadOnly(value.InternalOnly.MechanicProfileIds)
                    && IsReadOnly(value.DeveloperOnly.DeveloperDiagnosticCategoryKeys)
                    && IsReadOnly(value.DeveloperOnly.SourceReferenceIds))
                && catalog.SkillSequences.All(value => IsReadOnly(value.Steps))
                && catalog.CarrierSkillBindings.All(value => IsReadOnly(value.SkillSequenceIds))
                && catalog.BossPhaseProfiles.All(value =>
                    IsReadOnly(value.InternalOnly.SkillSequenceIds)
                    && IsReadOnly(value.InternalOnly.MechanicProfileIds)
                    && IsReadOnly(value.DeveloperOnly.DeveloperDiagnosticCategoryKeys)
                    && IsReadOnly(value.DeveloperOnly.SourceReferenceIds))
                && catalog.BossPhasePlans.All(value => IsReadOnly(value.Entries));
            Add(checks, "immutable.nested.collections", "read-only", string.Empty, nestedReadOnly);

            EnemySkillPatternSnapshot[] sourcePatterns = { Pattern("dev_skill_defensive_copy") };
            SkillSequenceStepSnapshot[] sourceSteps =
            {
                new SkillSequenceStepSnapshot(0, "dev_skill_defensive_copy", 0, 1)
            };
            string[] sourceSequenceIds = { "dev_sequence_defensive_copy" };
            BossPhasePlanEntrySnapshot[] sourcePlanEntries =
            {
                new BossPhasePlanEntrySnapshot(
                    0,
                    "dev_phase_defensive_copy",
                    new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.EncounterStart))
            };
            EnemySkillBossPhaseCatalogInput input = new EnemySkillBossPhaseCatalogInput(
                sourcePatterns,
                new[] { Sequence("dev_sequence_defensive_copy", sourceSteps) },
                new[] { Binding(EnemySkillCarrierKind.Boss, BossIdA, sourceSequenceIds) },
                new[] { Phase("dev_phase_defensive_copy", sourceSequenceIds) },
                new[] { Plan(BossIdA, sourcePlanEntries) });

            sourcePatterns[0] = Pattern("dev_skill_mutated_source");
            sourceSteps[0] = new SkillSequenceStepSnapshot(0, "dev_skill_mutated_source", 0, 1);
            sourceSequenceIds[0] = "dev_sequence_mutated_source";
            sourcePlanEntries[0] = new BossPhasePlanEntrySnapshot(
                0,
                "dev_phase_mutated_source",
                new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.EncounterStart));
            EnemySkillBossPhaseCatalogSnapshot defensive =
                DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(input, resolver);
            bool defensiveCopy = defensive.SkillPatterns[0].SkillPatternId == "dev_skill_defensive_copy"
                && defensive.SkillSequences[0].Steps[0].SkillPatternId == "dev_skill_defensive_copy"
                && defensive.CarrierSkillBindings[0].SkillSequenceIds[0] == "dev_sequence_defensive_copy"
                && defensive.BossPhasePlans[0].Entries[0].BossPhaseId == "dev_phase_defensive_copy";
            Add(checks, "immutable.defensive.copy", "source mutation isolated", string.Empty, defensiveCopy);
        }

        private static void RunSignatureChecks(
            List<Check> checks,
            E02E03ReferenceResolver resolver,
            EnemySkillBossPhaseCatalogSnapshot baseline)
        {
            Add(checks, "signature.full.format", "sha256:+64 lowercase hex", baseline.CanonicalSignature,
                IsCanonicalSignature(baseline.CanonicalSignature));
            Add(checks, "signature.player.format", "sha256:+64 lowercase hex", baseline.PlayerSafeCanonicalSignature,
                IsCanonicalSignature(baseline.PlayerSafeCanonicalSignature));

            EnemySkillBossPhaseCatalogSnapshot repeated = Snapshot(CreateFixture(), resolver);
            EnemySkillBossPhaseCatalogSnapshot reversed = Snapshot(CreateFixture(reverseInputOrder: true), resolver);
            Add(checks, "signature.deterministic.repeat", baseline.CanonicalSignature, repeated.CanonicalSignature,
                baseline.CanonicalSignature == repeated.CanonicalSignature
                    && baseline.PlayerSafeCanonicalSignature == repeated.PlayerSafeCanonicalSignature);
            Add(checks, "signature.input.permutation", baseline.CanonicalSignature, reversed.CanonicalSignature,
                baseline.CanonicalSignature == reversed.CanonicalSignature
                    && baseline.PlayerSafeCanonicalSignature == reversed.PlayerSafeCanonicalSignature);

            AddFullOnlySignatureMutation(checks, "signature.stepOrder", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.StepOrder), resolver));
            AddFullOnlySignatureMutation(checks, "signature.phaseOrder", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.PhaseOrder), resolver));
            AddFullOnlySignatureMutation(checks, "signature.castDuration", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.CastDuration), resolver));
            AddFullOnlySignatureMutation(checks, "signature.delay", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.Delay), resolver));
            AddFullOnlySignatureMutation(checks, "signature.repeat", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.Repeat), resolver));
            AddFullOnlySignatureMutation(checks, "signature.entryCondition", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.EntryCondition), resolver));
            AddFullOnlySignatureMutation(checks, "isolation.internal", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.InternalOnly), resolver));
            AddFullOnlySignatureMutation(checks, "isolation.developer", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.DeveloperOnly), resolver));

            AddBothSignatureMutation(checks, "signature.public.skill", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.PublicSkill), resolver));
            AddBothSignatureMutation(checks, "signature.public.intent", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.PublicIntent), resolver));
            AddBothSignatureMutation(checks, "signature.public.targetCue", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.PublicTargetCue), resolver));
            AddBothSignatureMutation(checks, "signature.public.phaseLabel", baseline,
                Snapshot(CreateFixture(mutation: FixtureMutation.PublicPhaseLabel), resolver));

            string playerPayload = baseline.BuildPlayerSafeCanonicalPayload();
            string[] forbidden =
            {
                "castDurationMilliseconds",
                "recoveryDurationMilliseconds",
                "skillSequenceId",
                "stepOrder",
                "delayAfterPreviousMilliseconds",
                "repeatCount",
                "carrierSkillBinding",
                "mechanicProfileId",
                "bossPhasePlan",
                "phaseOrder",
                "entryCondition",
                "healthThresholdBasisPoints",
                "mechanicSignalId",
                "developerDiagnostic",
                "sourceReference",
                "requiredCapability",
                "hardSolution",
                "DropBias"
            };
            int leaks = forbidden.Sum(token => Count(playerPayload, token));
            Add(checks, "isolation.playerSafe.payload", "0 forbidden tokens",
                leaks.ToString(CultureInfo.InvariantCulture), leaks == 0);
            Add(checks, "isolation.playerSafe.schema", "schemaId+schemaVersion present", string.Empty,
                playerPayload.IndexOf("schemaId", StringComparison.Ordinal) >= 0
                    && playerPayload.IndexOf("schemaVersion", StringComparison.Ordinal) >= 0);
        }

        private static void RunValidationRejectionChecks(
            List<Check> checks,
            E02E03ReferenceResolver resolver)
        {
            ExpectCode(checks, "reject.input.null", null, resolver, "INPUT_NULL");
            ExpectCode(checks, "reject.resolver.null", ValidationInput(), null, "REFERENCE_RESOLVER_NULL");
            ExpectCode(checks, "reject.schema.id",
                ValidationInput(schemaId: "EnemySkillBossPhase.V1"), resolver, "SCHEMA_ID_MISMATCH");
            ExpectCode(checks, "reject.schema.version",
                ValidationInput(schemaVersion: 2), resolver, "SCHEMA_VERSION_MISMATCH");
            ExpectCode(checks, "reject.id.empty",
                ValidationInput(patterns: new[] { Pattern("") }), resolver, "ID_EMPTY");
            ExpectCode(checks, "reject.id.whitespace",
                ValidationInput(patterns: new[] { Pattern(" dev_skill_validation") }), resolver, "ID_OUTER_WHITESPACE");
            ExpectCode(checks, "reject.id.caseReplacement",
                ValidationInput(sequences: new[]
                {
                    Sequence(steps: new[]
                    {
                        new SkillSequenceStepSnapshot(0, "DEV_SKILL_VALIDATION", 0, 1)
                    })
                }), resolver, "SKILL_PATTERN_REFERENCE_UNRESOLVED");

            EnemySkillPatternSnapshot validPattern = Pattern();
            SkillSequenceSnapshot validSequence = Sequence();
            BossPhaseProfileSnapshot validPhase = Phase();
            BossPhasePlanSnapshot validPlan = Plan();
            ExpectCode(checks, "reject.pattern.duplicate",
                ValidationInput(patterns: new[] { validPattern, validPattern }), resolver, "SKILL_PATTERN_ID_DUPLICATE");
            ExpectCode(checks, "reject.sequence.duplicate",
                ValidationInput(sequences: new[] { validSequence, validSequence }), resolver, "SKILL_SEQUENCE_ID_DUPLICATE");
            ExpectCode(checks, "reject.phase.duplicate",
                ValidationInput(phases: new[] { validPhase, validPhase }), resolver, "BOSS_PHASE_ID_DUPLICATE");
            ExpectCode(checks, "reject.plan.duplicateBoss",
                ValidationInput(plans: new[] { validPlan, validPlan }), resolver, "BOSS_PHASE_PLAN_BOSS_ID_DUPLICATE");
            ExpectCode(checks, "reject.sequence.empty",
                ValidationInput(sequences: new[] { Sequence(steps: Array.Empty<SkillSequenceStepSnapshot>()) }), resolver, "SKILL_SEQUENCE_EMPTY");
            ExpectCode(checks, "reject.plan.empty",
                ValidationInput(plans: new[] { Plan(entries: Array.Empty<BossPhasePlanEntrySnapshot>()) }), resolver, "BOSS_PHASE_PLAN_EMPTY");

            ExpectCode(checks, "reject.step.order.duplicate",
                ValidationInput(sequences: new[] { Sequence(steps: new[]
                {
                    new SkillSequenceStepSnapshot(0, "dev_skill_validation", 0, 1),
                    new SkillSequenceStepSnapshot(0, "dev_skill_validation", 0, 1)
                }) }), resolver, "STEP_ORDER_DUPLICATE");
            ExpectCode(checks, "reject.step.order.nonContiguous",
                ValidationInput(sequences: new[] { Sequence(steps: new[]
                {
                    new SkillSequenceStepSnapshot(1, "dev_skill_validation", 0, 1)
                }) }), resolver, "STEP_ORDER_NON_CONTIGUOUS");
            ExpectCode(checks, "reject.step.order.negative",
                ValidationInput(sequences: new[] { Sequence(steps: new[]
                {
                    new SkillSequenceStepSnapshot(-1, "dev_skill_validation", 0, 1)
                }) }), resolver, "STEP_ORDER_NEGATIVE");

            BossPhasePlanEntrySnapshot start = PlanEntry(0, "dev_phase_validation", BossPhaseEntryConditionKind.EncounterStart);
            ExpectCode(checks, "reject.phase.order.duplicate",
                ValidationInput(plans: new[] { Plan(entries: new[] { start, start }) }), resolver, "PHASE_ORDER_DUPLICATE");
            ExpectCode(checks, "reject.phase.order.nonContiguous",
                ValidationInput(plans: new[] { Plan(entries: new[]
                {
                    PlanEntry(1, "dev_phase_validation", BossPhaseEntryConditionKind.EncounterStart)
                }) }), resolver, "PHASE_ORDER_NON_CONTIGUOUS");
            ExpectCode(checks, "reject.phase.order.negative",
                ValidationInput(plans: new[] { Plan(entries: new[]
                {
                    PlanEntry(-1, "dev_phase_validation", BossPhaseEntryConditionKind.EncounterStart)
                }) }), resolver, "PHASE_ORDER_NEGATIVE");

            ExpectCode(checks, "reject.pattern.unresolved",
                ValidationInput(sequences: new[] { Sequence(steps: new[]
                {
                    new SkillSequenceStepSnapshot(0, "dev_skill_missing", 0, 1)
                }) }), resolver, "SKILL_PATTERN_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.sequence.unresolved",
                ValidationInput(bindings: new[]
                {
                    Binding(EnemySkillCarrierKind.Boss, BossIdA, new[] { "dev_sequence_missing" })
                }), resolver, "SKILL_SEQUENCE_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.carrier.unresolved",
                ValidationInput(bindings: new[]
                {
                    Binding(EnemySkillCarrierKind.Boss, "dev_boss_missing", new[] { "dev_sequence_validation" })
                }), resolver, "BOSS_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.planBoss.unresolved",
                ValidationInput(plans: new[] { Plan("dev_boss_missing") }), resolver, "BOSS_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.mechanic.unresolved",
                ValidationInput(patterns: new[] { Pattern(mechanicProfileId: "dev_profile_missing") }), resolver, "MECHANIC_PROFILE_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.carrier.kindMismatch",
                ValidationInput(
                    bindings: new[]
                    {
                        Binding(EnemySkillCarrierKind.Enemy, EnemyId, new[] { "dev_sequence_validation" })
                    }), resolver, "SKILL_MECHANIC_PROFILE_KIND_MISMATCH");
            ExpectCode(checks, "reject.phase.enemyMechanic",
                ValidationInput(phases: new[]
                {
                    Phase(mechanicProfileId: EnemyProfileId)
                }), resolver, "MECHANIC_PROFILE_KIND_MISMATCH");
            ExpectCode(checks, "reject.vocabulary.wrongCategory",
                ValidationInput(patterns: new[]
                {
                    Pattern(playerHintKey: DiagnosticKey)
                }), resolver, "PLAYER_HINT_CATEGORY_UNRESOLVED");
            ExpectCode(checks, "reject.vocabulary.unknown",
                ValidationInput(patterns: new[]
                {
                    Pattern(diagnosticKey: "diagnostic.unknown_fixture")
                }), resolver, "DEVELOPER_DIAGNOSTIC_CATEGORY_UNRESOLVED");

            ExpectCode(checks, "reject.instant.duration",
                ValidationInput(patterns: new[]
                {
                    Pattern(castKind: SkillCastKind.Instant, castDuration: 1)
                }), resolver, "INSTANT_CAST_DURATION_INVALID");
            ExpectCode(checks, "reject.channeled.duration",
                ValidationInput(patterns: new[]
                {
                    Pattern(castKind: SkillCastKind.Channeled, castDuration: 0)
                }), resolver, "CHANNELED_CAST_DURATION_INVALID");
            ExpectCode(checks, "reject.cast.duration.negative",
                ValidationInput(patterns: new[]
                {
                    Pattern(castKind: SkillCastKind.Instant, castDuration: -1)
                }), resolver, "CAST_DURATION_NEGATIVE");
            ExpectCode(checks, "reject.recovery.duration.negative",
                ValidationInput(patterns: new[]
                {
                    Pattern(recoveryDuration: -1)
                }), resolver, "RECOVERY_DURATION_NEGATIVE");
            ExpectCode(checks, "reject.step.delay.negative",
                ValidationInput(sequences: new[] { Sequence(steps: new[]
                {
                    new SkillSequenceStepSnapshot(0, "dev_skill_validation", -1, 1)
                }) }), resolver, "STEP_DELAY_NEGATIVE");
            ExpectCode(checks, "reject.step.repeat",
                ValidationInput(sequences: new[] { Sequence(steps: new[]
                {
                    new SkillSequenceStepSnapshot(0, "dev_skill_validation", 0, 0)
                }) }), resolver, "STEP_REPEAT_COUNT_INVALID");

            ExpectCode(checks, "reject.condition.fields",
                ValidationInput(plans: new[] { Plan(entries: new[]
                {
                    new BossPhasePlanEntrySnapshot(0, "dev_phase_validation",
                        new BossPhaseEntryConditionSnapshot(
                            BossPhaseEntryConditionKind.EncounterStart,
                            1,
                            "signal.invalid"))
                }) }), resolver, "ENTRY_CONDITION_FIELDS_INVALID");
            ExpectCode(checks, "reject.condition.healthThreshold",
                ValidationInput(plans: new[] { Plan(entries: new[]
                {
                    new BossPhasePlanEntrySnapshot(0, "dev_phase_validation",
                        new BossPhaseEntryConditionSnapshot(
                            BossPhaseEntryConditionKind.HealthRatioAtOrBelow,
                            10000))
                }) }), resolver, "ENTRY_CONDITION_FIELDS_INVALID");
            ExpectCode(checks, "reject.condition.signalEmpty",
                ValidationInput(plans: new[] { Plan(entries: new[]
                {
                    new BossPhasePlanEntrySnapshot(0, "dev_phase_validation",
                        new BossPhaseEntryConditionSnapshot(BossPhaseEntryConditionKind.MechanicSignal))
                }) }), resolver, "ENTRY_CONDITION_FIELDS_INVALID");
            ExpectCode(checks, "reject.firstPhase.condition",
                ValidationInput(plans: new[] { Plan(entries: new[]
                {
                    new BossPhasePlanEntrySnapshot(0, "dev_phase_validation",
                        new BossPhaseEntryConditionSnapshot(
                            BossPhaseEntryConditionKind.HealthRatioAtOrBelow,
                            5000))
                }) }), resolver, "FIRST_PHASE_CONDITION_INVALID");
            ExpectCode(checks, "reject.laterPhase.encounterStart",
                ValidationInput(plans: new[] { Plan(entries: new[]
                {
                    PlanEntry(0, "dev_phase_validation", BossPhaseEntryConditionKind.EncounterStart),
                    PlanEntry(1, "dev_phase_validation", BossPhaseEntryConditionKind.EncounterStart)
                }) }), resolver, "LATER_PHASE_ENCOUNTER_START_INVALID");

            ExpectCode(checks, "reject.isolation.pattern",
                ValidationInput(patterns: new[] { Pattern(devOnly: false) }), resolver, "DEV_ISOLATION_INVALID");
            ExpectCode(checks, "reject.isolation.sequence",
                ValidationInput(sequences: new[] { Sequence(isEnabled: true) }), resolver, "DEV_ISOLATION_INVALID");
            ExpectCode(checks, "reject.isolation.plan",
                ValidationInput(plans: new[] { Plan(entersFormalFlow: true) }), resolver, "DEV_ISOLATION_INVALID");
            ExpectCode(checks, "reject.source.path",
                ValidationInput(patterns: new[]
                {
                    Pattern(sourceReferenceId: "Assets/fixture.asset")
                }), resolver, "SOURCE_REFERENCE_PATH_FORBIDDEN");
        }

        private static void RunBaselineChecks(
            List<Check> checks,
            string root,
            EnemyValidationContentSnapshot references)
        {
            Add(checks, "baseline.e03.enemies", "11", references.Enemies.Count.ToString(CultureInfo.InvariantCulture),
                references.Enemies.Count == 11);
            Add(checks, "baseline.e03.bosses", "7", references.Bosses.Count.ToString(CultureInfo.InvariantCulture),
                references.Bosses.Count == 7);
            Add(checks, "baseline.e03.carriers", "18",
                (references.Enemies.Count + references.Bosses.Count).ToString(CultureInfo.InvariantCulture),
                references.Enemies.Count + references.Bosses.Count == 18);
            Add(checks, "baseline.e03.profiles", "16", references.MechanicProfiles.Count.ToString(CultureInfo.InvariantCulture),
                references.MechanicProfiles.Count == 16);
            Add(checks, "baseline.e03.enemyProfiles", "10",
                references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy).ToString(CultureInfo.InvariantCulture),
                references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy) == 10);
            Add(checks, "baseline.e03.bossProfiles", "6",
                references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss).ToString(CultureInfo.InvariantCulture),
                references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss) == 6);
            Add(checks, "baseline.e03.mapRules", "10", references.MapRules.Count.ToString(CultureInfo.InvariantCulture),
                references.MapRules.Count == 10);
            Add(checks, "baseline.e03.carrierBindings", "17",
                references.CarrierMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                references.CarrierMechanicBindings.Count == 17);
            Add(checks, "baseline.e03.mapBindings", "30",
                references.MapRuleMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                references.MapRuleMechanicBindings.Count == 30);
            Add(checks, "baseline.e03.exceptions", "2", references.Exceptions.Count.ToString(CultureInfo.InvariantCulture),
                references.Exceptions.Count == 2);

            string e04FixturePath = Path.Combine(
                root,
                "Docs/V0.4/Reports/EncounterCompositionSchemaFixtureRows.csv".Replace('/', Path.DirectorySeparatorChar));
            string[] lines = File.Exists(e04FixturePath)
                ? File.ReadAllLines(e04FixturePath, Encoding.UTF8)
                : Array.Empty<string>();
            string[][] cells = lines.Skip(1)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(ParseCsvLine)
                .Where(value => value.Length >= 2)
                .ToArray();
            int encounterCount = cells.Select(value => value[0]).Distinct(StringComparer.Ordinal).Count();
            int encounterWaveCount = cells.Select(value => value[0] + "\u001f" + value[1])
                .Distinct(StringComparer.Ordinal).Count();
            Add(checks, "baseline.e04.fixtureRows", "6", cells.Length.ToString(CultureInfo.InvariantCulture),
                cells.Length == 6);
            Add(checks, "baseline.e04.encounters", "2", encounterCount.ToString(CultureInfo.InvariantCulture),
                encounterCount == 2);
            Add(checks, "baseline.e04.encounterWaves", "4", encounterWaveCount.ToString(CultureInfo.InvariantCulture),
                encounterWaveCount == 4);
        }

        private static void RunLeakChecks(
            List<Check> checks,
            string root,
            EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            string runtimeDirectory = Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase".Replace('/', Path.DirectorySeparatorChar));
            string[] runtimeFiles = Directory.Exists(runtimeDirectory)
                ? Directory.GetFiles(runtimeDirectory, "*.cs", SearchOption.TopDirectoryOnly)
                : Array.Empty<string>();
            string runtimeText = string.Join(
                "\n",
                runtimeFiles.OrderBy(value => value, StringComparer.Ordinal)
                    .Select(value => File.ReadAllText(value, Encoding.UTF8)));
            int forbiddenRuntime = RuntimeForbiddenTokens.Sum(token => Count(runtimeText, token));
            int executionLeaks = TimerAndExecutionTokens.Sum(token => Count(runtimeText, token));
            Add(checks, "leak.runtimeFiles", "4", runtimeFiles.Length.ToString(CultureInfo.InvariantCulture),
                runtimeFiles.Length == 4);
            Add(checks, "leak.runtime.dependencies", "0", forbiddenRuntime.ToString(CultureInfo.InvariantCulture),
                forbiddenRuntime == 0);
            Add(checks, "leak.runtime.execution", "0", executionLeaks.ToString(CultureInfo.InvariantCulture),
                executionLeaks == 0);

            string verifierPath = Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs"
                    .Replace('/', Path.DirectorySeparatorChar));
            string verifierText = File.Exists(verifierPath)
                ? File.ReadAllText(verifierPath, Encoding.UTF8)
                : string.Empty;
            string directLegacyUsing = "using TalismanBag." + "BuildSandbox";
            Add(checks, "leak.verifier.directLegacyUsing", "0",
                Count(verifierText, directLegacyUsing).ToString(CultureInfo.InvariantCulture),
                Count(verifierText, directLegacyUsing) == 0);

            string normalizerPath = Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs"
                    .Replace('/', Path.DirectorySeparatorChar));
            string normalizerText = File.Exists(normalizerPath)
                ? File.ReadAllText(normalizerPath, Encoding.UTF8)
                : string.Empty;
            string soleReaderToken = "EnemyBossValidationPool." + "CreateDefault";
            int soleReaderCount = Count(normalizerText, soleReaderToken) + Count(verifierText, soleReaderToken);
            Add(checks, "leak.legacy.soleReader", "1", soleReaderCount.ToString(CultureInfo.InvariantCulture),
                soleReaderCount == 1);

            string combined = runtimeText + "\n" + verifierText + "\n" + catalog.BuildPlayerSafeCanonicalPayload();
            string[] chapterTokens = { "3" + "-10", "4" + "-10" };
            int chapterLeaks = chapterTokens.Sum(token => Count(combined, token));
            Add(checks, "leak.chapterIds", "0", chapterLeaks.ToString(CultureInfo.InvariantCulture),
                chapterLeaks == 0);

            string[] answerTokens =
            {
                "required" + "synergy",
                "required" + "affix",
                "required" + "stats",
                "hard" + "solutiontags",
                "minimum" + "keysrequired",
                "drop" + "bias"
            };
            string answerSurface = runtimeText.ToLowerInvariant()
                + "\n"
                + catalog.BuildPlayerSafeCanonicalPayload().ToLowerInvariant();
            int answerLeaks = answerTokens.Sum(token => Count(answerSurface, token));
            Add(checks, "leak.answerFields", "0", answerLeaks.ToString(CultureInfo.InvariantCulture),
                answerLeaks == 0);

            string[] prohibitedAreaTokens =
            {
                "SceneManager", "UnityEngine.SceneManagement", "PrefabUtility", "ScriptableObject",
                "Config", "Battle", "Board", "ItemSystem"
            };
            int prohibitedAreaLeaks = prohibitedAreaTokens.Sum(token => Count(runtimeText, token));
            Add(checks, "leak.prohibitedAreas", "0", prohibitedAreaLeaks.ToString(CultureInfo.InvariantCulture),
                prohibitedAreaLeaks == 0);
        }

        private static void RunGeneratedFileChecks(List<Check> checks, string root)
        {
            int existing = ExpectedPackageFiles.Count(relative => File.Exists(Absolute(root, relative)));
            Add(checks, "package.fileCount", "16", existing.ToString(CultureInfo.InvariantCulture),
                existing == 16);

            string runtimeDirectory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase");
            int runtimeArtifacts = Directory.Exists(runtimeDirectory)
                ? Directory.GetFiles(runtimeDirectory, "*", SearchOption.TopDirectoryOnly).Length
                : 0;
            Add(checks, "package.runtimeDirectoryArtifacts", "8", runtimeArtifacts.ToString(CultureInfo.InvariantCulture),
                runtimeArtifacts == 8);

            string reportsDirectory = Absolute(root, "Docs/V0.4/Reports");
            int reports = Directory.Exists(reportsDirectory)
                ? Directory.GetFiles(reportsDirectory, "EnemySkillBossPhaseSchema*", SearchOption.TopDirectoryOnly).Length
                : 0;
            Add(checks, "package.reportArtifacts", "5", reports.ToString(CultureInfo.InvariantCulture),
                reports == 5);

            string[] forbiddenExtensions = { ".unity", ".prefab", ".asset" };
            int forbiddenExtensionsFound = ExpectedPackageFiles.Count(
                value => forbiddenExtensions.Contains(Path.GetExtension(value), StringComparer.OrdinalIgnoreCase));
            Add(checks, "package.prohibitedExtensions", "0",
                forbiddenExtensionsFound.ToString(CultureInfo.InvariantCulture), forbiddenExtensionsFound == 0);

            int whitespaceIssues = ExpectedPackageFiles
                .Where(relative => File.Exists(Absolute(root, relative)))
                .Sum(relative => CountTrailingWhitespace(Absolute(root, relative)));
            Add(checks, "package.trailingWhitespace", "0", whitespaceIssues.ToString(CultureInfo.InvariantCulture),
                whitespaceIssues == 0);

            List<string> packageGuids = ExpectedPackageFiles
                .Where(relative => relative.EndsWith(".meta", StringComparison.Ordinal))
                .Where(relative => File.Exists(Absolute(root, relative)))
                .Select(relative => ReadMetaGuid(Absolute(root, relative)))
                .Where(value => !string.IsNullOrEmpty(value))
                .ToList();
            Add(checks, "package.metaGuidCount", "6", packageGuids.Count.ToString(CultureInfo.InvariantCulture),
                packageGuids.Count == 6);
            Add(checks, "package.metaGuidUnique", "6",
                packageGuids.Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                packageGuids.Distinct(StringComparer.Ordinal).Count() == 6);

            string assetsRoot = Absolute(root, "Assets");
            HashSet<string> allGuids = new HashSet<string>(StringComparer.Ordinal);
            int duplicateGuids = 0;
            if (Directory.Exists(assetsRoot))
            {
                foreach (string meta in Directory.GetFiles(assetsRoot, "*.meta", SearchOption.AllDirectories))
                {
                    string guid = ReadMetaGuid(meta);
                    if (!string.IsNullOrEmpty(guid) && !allGuids.Add(guid))
                    {
                        duplicateGuids++;
                    }
                }
            }
            Add(checks, "package.globalGuidConflicts", "0", duplicateGuids.ToString(CultureInfo.InvariantCulture),
                duplicateGuids == 0);
        }

        private enum FixtureMutation
        {
            None = 0,
            StepOrder = 1,
            PhaseOrder = 2,
            CastDuration = 3,
            Delay = 4,
            Repeat = 5,
            EntryCondition = 6,
            InternalOnly = 7,
            DeveloperOnly = 8,
            PublicSkill = 9,
            PublicIntent = 10,
            PublicTargetCue = 11,
            PublicPhaseLabel = 12
        }

        private static EnemySkillBossPhaseCatalogInput CreateFixture(
            bool reverseInputOrder = false,
            FixtureMutation mutation = FixtureMutation.None)
        {
            const string enemyInstantId = "dev_skill_fixture_guard_cast";
            const string enemyChannelId = "dev_skill_fixture_pressure_channel";
            const string bossInstantId = "dev_skill_fixture_spirit_pulse";
            const string bossChannelId = "dev_skill_fixture_core_channel";
            const string enemyPressureSequenceId = "dev_sequence_fixture_enemy_pressure";
            const string enemyEchoSequenceId = "dev_sequence_fixture_enemy_echo";
            const string bossSequenceId = "dev_sequence_fixture_boss_pressure";
            const string openingPhaseId = "dev_phase_fixture_opening";
            const string pressurePhaseId = "dev_phase_fixture_pressure";
            const string signalPhaseId = "dev_phase_fixture_signal";

            EnemySkillPatternSnapshot[] patterns =
            {
                FixturePattern(
                    enemyInstantId,
                    EnemyProfileId,
                    SkillCastKind.Instant,
                    0,
                    mutation == FixtureMutation.InternalOnly ? 350 : 300,
                    mutation == FixtureMutation.PublicSkill
                        ? "enemy.skill.fixture_guard_cast.alternate"
                        : "enemy.skill.fixture_guard_cast",
                    mutation == FixtureMutation.PublicIntent
                        ? "enemy.intent.fixture_probe.alternate"
                        : "enemy.intent.fixture_probe",
                    mutation == FixtureMutation.PublicTargetCue
                        ? "enemy.target_cue.fixture_frontline.alternate"
                        : "enemy.target_cue.fixture_frontline",
                    HintKey,
                    mutation == FixtureMutation.DeveloperOnly
                        ? "source.e05.fixture.alternate"
                        : "source.e05.fixture.guard_cast"),
                FixturePattern(
                    enemyChannelId,
                    EnemyProfileId,
                    SkillCastKind.Channeled,
                    1200,
                    500,
                    "enemy.skill.fixture_pressure_channel",
                    "enemy.intent.fixture_pressure",
                    "enemy.target_cue.fixture_zone",
                    AlternateHintKey,
                    "source.e05.fixture.pressure_channel"),
                FixturePattern(
                    bossInstantId,
                    BossProfileId,
                    SkillCastKind.Instant,
                    0,
                    450,
                    "boss.skill.fixture_spirit_pulse",
                    "boss.intent.fixture_pressure",
                    "boss.target_cue.fixture_field",
                    HintKey,
                    "source.e05.fixture.spirit_pulse"),
                FixturePattern(
                    bossChannelId,
                    BossProfileId,
                    SkillCastKind.Channeled,
                    mutation == FixtureMutation.CastDuration ? 1700 : 1600,
                    700,
                    "boss.skill.fixture_core_channel",
                    "boss.intent.fixture_channel",
                    "boss.target_cue.fixture_center",
                    AlternateHintKey,
                    "source.e05.fixture.core_channel")
            };

            SkillSequenceStepSnapshot[] enemyPressureSteps =
            {
                new SkillSequenceStepSnapshot(
                    0,
                    mutation == FixtureMutation.StepOrder ? enemyChannelId : enemyInstantId,
                    0,
                    1),
                new SkillSequenceStepSnapshot(
                    1,
                    mutation == FixtureMutation.StepOrder ? enemyInstantId : enemyChannelId,
                    mutation == FixtureMutation.Delay ? 150 : 100,
                    1)
            };
            SkillSequenceSnapshot[] sequences =
            {
                Sequence(enemyPressureSequenceId, enemyPressureSteps),
                Sequence(enemyEchoSequenceId, new[]
                {
                    new SkillSequenceStepSnapshot(
                        0,
                        enemyChannelId,
                        0,
                        mutation == FixtureMutation.Repeat ? 2 : 1)
                }),
                Sequence(bossSequenceId, new[]
                {
                    new SkillSequenceStepSnapshot(0, bossInstantId, 0, 1),
                    new SkillSequenceStepSnapshot(1, bossChannelId, 200, 1)
                })
            };

            CarrierSkillBindingSnapshot[] bindings =
            {
                Binding(
                    EnemySkillCarrierKind.Enemy,
                    EnemyId,
                    new[] { enemyPressureSequenceId, enemyEchoSequenceId }),
                Binding(EnemySkillCarrierKind.Boss, BossIdA, new[] { bossSequenceId }),
                Binding(EnemySkillCarrierKind.Boss, BossIdB, new[] { bossSequenceId })
            };

            BossPhaseProfileSnapshot[] phases =
            {
                FixturePhase(
                    openingPhaseId,
                    "boss.phase.fixture.opening",
                    "boss.phase_cue.fixture.opening",
                    bossSequenceId),
                FixturePhase(
                    pressurePhaseId,
                    mutation == FixtureMutation.PublicPhaseLabel
                        ? "boss.phase.fixture.pressure.alternate"
                        : "boss.phase.fixture.pressure",
                    "boss.phase_cue.fixture.pressure",
                    bossSequenceId),
                FixturePhase(
                    signalPhaseId,
                    "boss.phase.fixture.signal",
                    "boss.phase_cue.fixture.signal",
                    bossSequenceId)
            };

            BossPhasePlanEntrySnapshot[] planEntries =
            {
                PlanEntry(
                    0,
                    mutation == FixtureMutation.PhaseOrder ? pressurePhaseId : openingPhaseId,
                    BossPhaseEntryConditionKind.EncounterStart),
                new BossPhasePlanEntrySnapshot(
                    1,
                    mutation == FixtureMutation.PhaseOrder ? openingPhaseId : pressurePhaseId,
                    new BossPhaseEntryConditionSnapshot(
                        BossPhaseEntryConditionKind.HealthRatioAtOrBelow,
                        mutation == FixtureMutation.EntryCondition ? 6500 : 6000)),
                new BossPhasePlanEntrySnapshot(
                    2,
                    signalPhaseId,
                    new BossPhaseEntryConditionSnapshot(
                        BossPhaseEntryConditionKind.MechanicSignal,
                        0,
                        "boss.signal.fixture_escalate"))
            };
            BossPhasePlanSnapshot[] plans =
            {
                Plan(BossIdA, planEntries),
                Plan(BossIdB, planEntries)
            };

            if (reverseInputOrder)
            {
                Array.Reverse(patterns);
                Array.Reverse(sequences);
                Array.Reverse(bindings);
                Array.Reverse(phases);
                Array.Reverse(plans);
            }

            return new EnemySkillBossPhaseCatalogInput(
                patterns,
                sequences,
                bindings,
                phases,
                plans);
        }

        private static EnemySkillPatternSnapshot FixturePattern(
            string id,
            string mechanicProfileId,
            SkillCastKind castKind,
            int castDuration,
            int recoveryDuration,
            string publicSkillNameKey,
            string intentId,
            string publicTargetCueKey,
            string playerHintKey,
            string sourceReferenceId)
        {
            return new EnemySkillPatternSnapshot(
                new SkillPatternReference(id),
                new SkillPatternPlayerProjection(
                    id,
                    publicSkillNameKey,
                    intentId,
                    intentId + ".text",
                    publicTargetCueKey,
                    publicSkillNameKey + ".cast_cue",
                    new[] { playerHintKey }),
                new SkillPatternInternalSpec(
                    castKind,
                    castDuration,
                    recoveryDuration,
                    new[] { mechanicProfileId }),
                new SkillPatternDeveloperDiagnostics(
                    new[] { DiagnosticKey },
                    new[] { sourceReferenceId }));
        }

        private static BossPhaseProfileSnapshot FixturePhase(
            string id,
            string publicLabelKey,
            string publicEntryCueKey,
            string sequenceId)
        {
            return new BossPhaseProfileSnapshot(
                new BossPhaseReference(id),
                new BossPhasePlayerProjection(id, publicLabelKey, publicEntryCueKey),
                new BossPhaseInternalSpec(
                    new[] { sequenceId },
                    new[] { BossProfileId }),
                new BossPhaseDeveloperDiagnostics(
                    new[] { DiagnosticKey },
                    new[] { "source.e05.fixture.phase" }));
        }

        private static EnemySkillBossPhaseCatalogInput ValidationInput(
            IEnumerable<EnemySkillPatternSnapshot> patterns = null,
            IEnumerable<SkillSequenceSnapshot> sequences = null,
            IEnumerable<CarrierSkillBindingSnapshot> bindings = null,
            IEnumerable<BossPhaseProfileSnapshot> phases = null,
            IEnumerable<BossPhasePlanSnapshot> plans = null,
            string schemaId = EnemySkillBossPhaseSchema.SchemaId,
            int schemaVersion = EnemySkillBossPhaseSchema.SchemaVersion)
        {
            return new EnemySkillBossPhaseCatalogInput(
                patterns ?? new[] { Pattern() },
                sequences ?? new[] { Sequence() },
                bindings ?? new[]
                {
                    Binding(
                        EnemySkillCarrierKind.Boss,
                        BossIdA,
                        new[] { "dev_sequence_validation" })
                },
                phases ?? new[] { Phase() },
                plans ?? new[] { Plan() },
                schemaId,
                schemaVersion);
        }

        private static EnemySkillPatternSnapshot Pattern(
            string id = "dev_skill_validation",
            string mechanicProfileId = BossProfileId,
            SkillCastKind castKind = SkillCastKind.Channeled,
            int castDuration = 1200,
            int recoveryDuration = 300,
            string playerHintKey = HintKey,
            string diagnosticKey = DiagnosticKey,
            string sourceReferenceId = "source.e05.validation",
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            return new EnemySkillPatternSnapshot(
                new SkillPatternReference(id, devOnly, isEnabled, entersFormalFlow),
                new SkillPatternPlayerProjection(
                    id,
                    "validation.skill.name",
                    "validation.intent",
                    "validation.intent.text",
                    "validation.target.cue",
                    "validation.cast.cue",
                    new[] { playerHintKey }),
                new SkillPatternInternalSpec(
                    castKind,
                    castDuration,
                    recoveryDuration,
                    new[] { mechanicProfileId }),
                new SkillPatternDeveloperDiagnostics(
                    new[] { diagnosticKey },
                    new[] { sourceReferenceId }));
        }

        private static SkillSequenceSnapshot Sequence(
            string id = "dev_sequence_validation",
            IEnumerable<SkillSequenceStepSnapshot> steps = null,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            return new SkillSequenceSnapshot(
                id,
                steps ?? new[]
                {
                    new SkillSequenceStepSnapshot(0, "dev_skill_validation", 0, 1)
                },
                devOnly,
                isEnabled,
                entersFormalFlow);
        }

        private static CarrierSkillBindingSnapshot Binding(
            EnemySkillCarrierKind kind,
            string carrierId,
            IEnumerable<string> sequenceIds,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            return new CarrierSkillBindingSnapshot(
                kind,
                carrierId,
                sequenceIds,
                devOnly,
                isEnabled,
                entersFormalFlow);
        }

        private static BossPhaseProfileSnapshot Phase(
            string id = "dev_phase_validation",
            IEnumerable<string> sequenceIds = null,
            string mechanicProfileId = BossProfileId,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            return new BossPhaseProfileSnapshot(
                new BossPhaseReference(id, devOnly, isEnabled, entersFormalFlow),
                new BossPhasePlayerProjection(
                    id,
                    "validation.phase.label",
                    "validation.phase.entry_cue"),
                new BossPhaseInternalSpec(
                    sequenceIds ?? new[] { "dev_sequence_validation" },
                    new[] { mechanicProfileId }),
                new BossPhaseDeveloperDiagnostics(
                    new[] { DiagnosticKey },
                    new[] { "source.e05.validation.phase" }));
        }

        private static BossPhasePlanSnapshot Plan(
            string bossId = BossIdA,
            IEnumerable<BossPhasePlanEntrySnapshot> entries = null,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            return new BossPhasePlanSnapshot(
                bossId,
                entries ?? new[]
                {
                    PlanEntry(
                        0,
                        "dev_phase_validation",
                        BossPhaseEntryConditionKind.EncounterStart)
                },
                devOnly,
                isEnabled,
                entersFormalFlow);
        }

        private static BossPhasePlanEntrySnapshot PlanEntry(
            int order,
            string phaseId,
            BossPhaseEntryConditionKind kind)
        {
            return new BossPhasePlanEntrySnapshot(
                order,
                phaseId,
                new BossPhaseEntryConditionSnapshot(kind));
        }

        private static void WriteReports(
            string root,
            string mode,
            IReadOnlyList<Check> checks,
            EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            string reportDirectory = Path.Combine(
                root,
                ReportRoot.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(reportDirectory);

            WriteUtf8(
                Path.Combine(reportDirectory, "EnemySkillBossPhaseSchemaReport.md"),
                BuildMainReport(mode, checks, catalog));
            WriteUtf8(
                Path.Combine(reportDirectory, "EnemySkillBossPhaseSchemaSpec.csv"),
                BuildSpecCsv(checks));
            WriteUtf8(
                Path.Combine(reportDirectory, "EnemySkillBossPhaseSchemaFieldMatrix.csv"),
                BuildFieldMatrixCsv());
            WriteUtf8(
                Path.Combine(reportDirectory, "EnemySkillBossPhaseSchemaFixtureRows.csv"),
                BuildFixtureRowsCsv(catalog));
            WriteUtf8(
                Path.Combine(reportDirectory, "EnemySkillBossPhaseSchemaLeakCheckReport.md"),
                BuildLeakReport(root, checks, catalog));
        }

        private static string BuildMainReport(
            string mode,
            IReadOnlyList<Check> checks,
            EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            int passed = checks.Count(value => value.Passed);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Skill / Boss Phase Schema Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-EnemySkillBossPhaseSchema01`");
            builder.AppendLine("- Mode: `" + mode + "`");
            builder.AppendLine("- Schema: `" + catalog.SchemaId + "` / `" + catalog.SchemaVersion + "`");
            builder.AppendLine("- Checks: `" + passed + "/" + checks.Count + "` passed");
            builder.AppendLine();
            builder.AppendLine("## Fixture inventory");
            builder.AppendLine();
            builder.AppendLine("| Type | Count |");
            builder.AppendLine("|---|---:|");
            builder.AppendLine("| SkillPattern | " + catalog.SkillPatterns.Count + " |");
            builder.AppendLine("| SkillSequence | " + catalog.SkillSequences.Count + " |");
            builder.AppendLine("| CarrierSkillBinding | " + catalog.CarrierSkillBindings.Count + " |");
            builder.AppendLine("| BossPhaseProfile | " + catalog.BossPhaseProfiles.Count + " |");
            builder.AppendLine("| BossPhasePlan | " + catalog.BossPhasePlans.Count + " |");
            builder.AppendLine();
            builder.AppendLine("## Canonical signatures");
            builder.AppendLine();
            builder.AppendLine("- Full: `" + catalog.CanonicalSignature + "`");
            builder.AppendLine("- Player-safe: `" + catalog.PlayerSafeCanonicalSignature + "`");
            builder.AppendLine();
            builder.AppendLine("## Reuse and isolation");
            builder.AppendLine();
            AppendCheckLine(builder, checks, "reuse.pattern", "Pattern reuse");
            AppendCheckLine(builder, checks, "reuse.sequence", "Sequence reuse");
            AppendCheckLine(builder, checks, "reuse.phase", "Phase reuse");
            AppendCheckLine(builder, checks, "isolation.internal", "Internal-only isolation");
            AppendCheckLine(builder, checks, "isolation.developer", "Developer-only isolation");
            AppendCheckLine(builder, checks, "isolation.playerSafe.payload", "Player-safe payload isolation");
            builder.AppendLine();
            builder.AppendLine("## Verification checks");
            builder.AppendLine();
            builder.AppendLine("| Check | Expected | Actual | Result |");
            builder.AppendLine("|---|---|---|---|");
            foreach (Check check in checks.OrderBy(value => value.Id, StringComparer.Ordinal))
            {
                builder.Append("| `").Append(Markdown(check.Id)).Append("` | ")
                    .Append(Markdown(check.Expected)).Append(" | ")
                    .Append(Markdown(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<Check> checks)
        {
            List<string[]> rows = new List<string[]>
            {
                new[] { "checkId", "expected", "actual", "result" }
            };
            rows.AddRange(checks.OrderBy(value => value.Id, StringComparer.Ordinal)
                .Select(value => new[]
                {
                    value.Id,
                    value.Expected,
                    value.Actual,
                    value.Passed ? "PASS" : "FAIL"
                }));
            return Csv(rows);
        }

        private static string BuildFieldMatrixCsv()
        {
            List<string[]> rows = new List<string[]>
            {
                new[]
                {
                    "ownerType", "fieldName", "fieldType", "cardinality", "semantic",
                    "visibility", "sourceOfTruth", "futureConsumer"
                },
                Field("EnemySkillBossPhaseCatalogSnapshot", "SchemaId", "string", "1", "exact schema identity", "shared metadata", "E05", "E06+ read-only adapters"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "SchemaVersion", "int", "1", "schema version", "shared metadata", "E05", "E06+ read-only adapters"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "SkillPatterns", "IReadOnlyList<EnemySkillPatternSnapshot>", "0..N", "skill declarations", "three-layer", "E05", "E06 feedback contracts"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "SkillSequences", "IReadOnlyList<SkillSequenceSnapshot>", "0..N", "explicit ordered references", "internal-only", "E05", "future runtime adapter"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "CarrierSkillBindings", "IReadOnlyList<CarrierSkillBindingSnapshot>", "0..N", "carrier to reusable sequences", "internal-only", "E05 + E03 references", "future runtime adapter"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "BossPhaseProfiles", "IReadOnlyList<BossPhaseProfileSnapshot>", "0..N", "reusable phase declarations", "three-layer", "E05", "E06 feedback contracts"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "BossPhasePlans", "IReadOnlyList<BossPhasePlanSnapshot>", "0..N", "Boss to ordered phase references", "internal-only", "E05 + E03 references", "future runtime adapter"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "CanonicalSignature", "string", "1", "full SHA-256 signature", "developer", "E05 canonicalizer", "QA"),
                Field("EnemySkillBossPhaseCatalogSnapshot", "PlayerSafeCanonicalSignature", "string", "1", "player-safe SHA-256 signature", "player-safe metadata", "E05 canonicalizer", "E06 feedback contracts"),
                Field("EnemySkillPatternSnapshot", "SkillPatternReference", "SkillPatternReference", "1", "E01 stable reference and isolation flags", "shared metadata", "E01", "E05 lookup"),
                Field("EnemySkillPatternSnapshot", "PlayerSafe", "SkillPatternPlayerProjection", "1", "public name intent and cues", "player-safe", "E05", "E06 feedback contracts"),
                Field("EnemySkillPatternSnapshot", "InternalOnly", "SkillPatternInternalSpec", "1", "cast facts and mechanic references", "internal-only", "E05 + E03", "future runtime adapter"),
                Field("EnemySkillPatternSnapshot", "DeveloperOnly", "SkillPatternDeveloperDiagnostics", "1", "diagnostic categories and provenance IDs", "developer-only", "E05 + E02", "QA"),
                Field("SkillPatternPlayerProjection", "SkillPatternId", "string", "1", "stable public projection identity", "player-safe", "E01 reference", "E06 feedback contracts"),
                Field("SkillPatternPlayerProjection", "PublicSkillNameKey", "string", "1", "public skill label key", "player-safe", "E05", "E06 feedback contracts"),
                Field("SkillPatternPlayerProjection", "IntentId", "string", "1", "public intent identity", "player-safe", "E05", "E06 feedback contracts"),
                Field("SkillPatternPlayerProjection", "PublicIntentTextKey", "string", "1", "public intent copy key", "player-safe", "E05", "E06 feedback contracts"),
                Field("SkillPatternPlayerProjection", "PublicTargetCueKey", "string", "1", "target cue only; no selector", "player-safe", "E05", "E06 feedback contracts"),
                Field("SkillPatternPlayerProjection", "PublicCastCueKey", "string", "1", "cast presentation cue key", "player-safe", "E05", "E06 feedback contracts"),
                Field("SkillPatternPlayerProjection", "PlayerHintCategoryKeys", "IReadOnlyList<string>", "0..N", "E02 player hint categories", "player-safe", "E02", "E06 feedback contracts"),
                Field("SkillPatternInternalSpec", "SkillCastKind", "SkillCastKind", "1", "Instant or Channeled declaration", "internal-only", "E05", "future runtime adapter"),
                Field("SkillPatternInternalSpec", "CastDurationMilliseconds", "int", "1", "canonical cast duration fact", "internal-only", "E05", "future runtime adapter"),
                Field("SkillPatternInternalSpec", "RecoveryDurationMilliseconds", "int", "1", "canonical recovery duration fact", "internal-only", "E05", "future runtime adapter"),
                Field("SkillPatternInternalSpec", "MechanicProfileIds", "IReadOnlyList<string>", "1..N", "E03 mechanic profile references", "internal-only", "E03", "future runtime adapter"),
                Field("SkillPatternDeveloperDiagnostics", "DeveloperDiagnosticCategoryKeys", "IReadOnlyList<string>", "0..N", "E02 diagnostic categories", "developer-only", "E02", "QA"),
                Field("SkillPatternDeveloperDiagnostics", "SourceReferenceIds", "IReadOnlyList<string>", "0..N", "opaque provenance IDs; never paths", "developer-only", "E05", "QA"),
                Field("SkillSequenceSnapshot", "SkillSequenceId", "string", "1", "stable sequence identity", "internal-only", "E05", "future runtime adapter"),
                Field("SkillSequenceSnapshot", "Steps", "IReadOnlyList<SkillSequenceStepSnapshot>", "1..N", "ordered reusable Pattern references", "internal-only", "E05", "future runtime adapter"),
                Field("SkillSequenceStepSnapshot", "StepOrder", "int", "1", "explicit contiguous order", "internal-only", "E05", "future runtime adapter"),
                Field("SkillSequenceStepSnapshot", "SkillPatternId", "string", "1", "E05 Pattern reference", "internal-only", "E05", "future runtime adapter"),
                Field("SkillSequenceStepSnapshot", "DelayAfterPreviousMilliseconds", "int", "1", "declarative delay fact; no timer", "internal-only", "E05", "future runtime adapter"),
                Field("SkillSequenceStepSnapshot", "RepeatCount", "int", "1", "declarative repeat count", "internal-only", "E05", "future runtime adapter"),
                Field("CarrierSkillBindingSnapshot", "CarrierKind", "EnemySkillCarrierKind", "1", "Enemy or Boss discriminator", "internal-only", "E05", "future runtime adapter"),
                Field("CarrierSkillBindingSnapshot", "CarrierId", "string", "1", "E03 carrier reference", "internal-only", "E03", "future runtime adapter"),
                Field("CarrierSkillBindingSnapshot", "SkillSequenceIds", "IReadOnlyList<string>", "1..N", "reusable sequence references", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhaseProfileSnapshot", "BossPhaseReference", "BossPhaseReference", "1", "E01 stable reference and isolation flags", "shared metadata", "E01", "E05 lookup"),
                Field("BossPhaseProfileSnapshot", "PlayerSafe", "BossPhasePlayerProjection", "1", "public label and entry cue", "player-safe", "E05", "E06 feedback contracts"),
                Field("BossPhaseProfileSnapshot", "InternalOnly", "BossPhaseInternalSpec", "1", "sequence and Boss mechanic references", "internal-only", "E05 + E03", "future runtime adapter"),
                Field("BossPhaseProfileSnapshot", "DeveloperOnly", "BossPhaseDeveloperDiagnostics", "1", "diagnostic categories and provenance IDs", "developer-only", "E05 + E02", "QA"),
                Field("BossPhasePlayerProjection", "BossPhaseId", "string", "1", "stable public projection identity", "player-safe", "E01 reference", "E06 feedback contracts"),
                Field("BossPhasePlayerProjection", "PublicPhaseLabelKey", "string", "1", "public phase label key", "player-safe", "E05", "E06 feedback contracts"),
                Field("BossPhasePlayerProjection", "PublicPhaseEntryCueKey", "string", "1", "public phase entry cue key", "player-safe", "E05", "E06 feedback contracts"),
                Field("BossPhaseInternalSpec", "SkillSequenceIds", "IReadOnlyList<string>", "1..N", "reusable E05 sequences", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhaseInternalSpec", "MechanicProfileIds", "IReadOnlyList<string>", "1..N", "Boss-kind E03 profiles", "internal-only", "E03", "future runtime adapter"),
                Field("BossPhaseDeveloperDiagnostics", "DeveloperDiagnosticCategoryKeys", "IReadOnlyList<string>", "0..N", "E02 diagnostic categories", "developer-only", "E02", "QA"),
                Field("BossPhaseDeveloperDiagnostics", "SourceReferenceIds", "IReadOnlyList<string>", "0..N", "opaque provenance IDs; never paths", "developer-only", "E05", "QA"),
                Field("BossPhasePlanSnapshot", "BossId", "string", "1", "E03 Boss reference", "internal-only", "E03", "future runtime adapter"),
                Field("BossPhasePlanSnapshot", "Entries", "IReadOnlyList<BossPhasePlanEntrySnapshot>", "1..N", "ordered Phase references and conditions", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhasePlanEntrySnapshot", "PhaseOrder", "int", "1", "explicit contiguous order", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhasePlanEntrySnapshot", "BossPhaseId", "string", "1", "E05 Phase reference", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhasePlanEntrySnapshot", "EntryCondition", "BossPhaseEntryConditionSnapshot", "1", "declarative entry condition; no transition", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhaseEntryConditionSnapshot", "Kind", "BossPhaseEntryConditionKind", "1", "condition discriminator", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhaseEntryConditionSnapshot", "HealthThresholdBasisPoints", "int", "1", "declarative ratio threshold", "internal-only", "E05", "future runtime adapter"),
                Field("BossPhaseEntryConditionSnapshot", "MechanicSignalId", "string", "0..1", "opaque signal identity", "internal-only", "E05", "future runtime adapter"),
                Field("Isolation metadata", "DevOnly", "bool", "1", "must remain true", "developer-only", "E01/E05", "QA"),
                Field("Isolation metadata", "IsEnabled", "bool", "1", "must remain false", "developer-only", "E01/E05", "QA"),
                Field("Isolation metadata", "EntersFormalFlow", "bool", "1", "must remain false", "developer-only", "E01/E05", "QA")
            };
            return Csv(rows);
        }

        private static string BuildFixtureRowsCsv(EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            List<string[]> rows = new List<string[]>
            {
                new[]
                {
                    "rowKind", "ownerId", "order", "referenceId", "publicLabelOrCueKey",
                    "castKind", "durationMilliseconds", "entryConditionKind",
                    "thresholdBasisPoints", "playerVisible", "fixtureOnly"
                }
            };
            foreach (EnemySkillPatternSnapshot pattern in catalog.SkillPatterns)
            {
                rows.Add(new[]
                {
                    "SkillPattern", pattern.SkillPatternId, string.Empty,
                    string.Join(";", pattern.InternalOnly.MechanicProfileIds),
                    pattern.PlayerSafe.PublicSkillNameKey,
                    pattern.InternalOnly.SkillCastKind.ToString(),
                    pattern.InternalOnly.CastDurationMilliseconds.ToString(CultureInfo.InvariantCulture),
                    string.Empty, string.Empty, "true", "true"
                });
            }

            foreach (SkillSequenceSnapshot sequence in catalog.SkillSequences)
            {
                foreach (SkillSequenceStepSnapshot step in sequence.Steps)
                {
                    rows.Add(new[]
                    {
                        "SkillSequenceStep", sequence.SkillSequenceId,
                        step.StepOrder.ToString(CultureInfo.InvariantCulture),
                        step.SkillPatternId, string.Empty, string.Empty,
                        step.DelayAfterPreviousMilliseconds.ToString(CultureInfo.InvariantCulture),
                        string.Empty, string.Empty, "false", "true"
                    });
                }
            }

            foreach (CarrierSkillBindingSnapshot binding in catalog.CarrierSkillBindings)
            {
                for (int index = 0; index < binding.SkillSequenceIds.Count; index++)
                {
                    rows.Add(new[]
                    {
                        "CarrierSkillBinding", binding.CarrierKind + ":" + binding.CarrierId,
                        index.ToString(CultureInfo.InvariantCulture), binding.SkillSequenceIds[index],
                        string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                        "false", "true"
                    });
                }
            }

            foreach (BossPhaseProfileSnapshot phase in catalog.BossPhaseProfiles)
            {
                rows.Add(new[]
                {
                    "BossPhaseProfile", phase.BossPhaseId, string.Empty,
                    string.Join(";", phase.InternalOnly.SkillSequenceIds),
                    phase.PlayerSafe.PublicPhaseLabelKey, string.Empty, string.Empty,
                    string.Empty, string.Empty, "true", "true"
                });
            }

            foreach (BossPhasePlanSnapshot plan in catalog.BossPhasePlans)
            {
                foreach (BossPhasePlanEntrySnapshot entry in plan.Entries)
                {
                    rows.Add(new[]
                    {
                        "BossPhasePlanEntry", plan.BossId,
                        entry.PhaseOrder.ToString(CultureInfo.InvariantCulture),
                        entry.BossPhaseId, string.Empty, string.Empty, string.Empty,
                        entry.EntryCondition.Kind.ToString(),
                        entry.EntryCondition.HealthThresholdBasisPoints.ToString(CultureInfo.InvariantCulture),
                        "false", "true"
                    });
                }
            }

            return Csv(rows);
        }

        private static string BuildLeakReport(
            string root,
            IReadOnlyList<Check> checks,
            EnemySkillBossPhaseCatalogSnapshot catalog)
        {
            string runtimeDirectory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase");
            string runtimeText = Directory.Exists(runtimeDirectory)
                ? string.Join("\n", Directory.GetFiles(runtimeDirectory, "*.cs")
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .Select(value => File.ReadAllText(value, Encoding.UTF8)))
                : string.Empty;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Skill / Boss Phase Schema Leak Check");
            builder.AppendLine();
            builder.AppendLine("Every forbidden dependency is reported separately. Counts are literal-source matches in the four E05 runtime files unless stated otherwise.");
            builder.AppendLine();
            builder.AppendLine("## Forbidden runtime dependencies");
            builder.AppendLine();
            builder.AppendLine("| Token | Matches | Result |");
            builder.AppendLine("|---|---:|---|");
            foreach (string token in RuntimeForbiddenTokens)
            {
                int count = Count(runtimeText, token);
                builder.Append("| `").Append(Markdown(token)).Append("` | ")
                    .Append(count).Append(" | ").Append(count == 0 ? "PASS" : "FAIL").AppendLine(" |");
            }
            foreach (string token in TimerAndExecutionTokens)
            {
                int count = Count(runtimeText, token);
                builder.Append("| `").Append(Markdown(token)).Append("` | ")
                    .Append(count).Append(" | ").Append(count == 0 ? "PASS" : "FAIL").AppendLine(" |");
            }
            builder.AppendLine();
            builder.AppendLine("## Isolation payload");
            builder.AppendLine();
            builder.AppendLine("- Player-safe payload length: `"
                + catalog.BuildPlayerSafeCanonicalPayload().Length.ToString(CultureInfo.InvariantCulture)
                + "`");
            AppendCheckLine(builder, checks, "isolation.playerSafe.payload", "Forbidden player-safe fields");
            AppendCheckLine(builder, checks, "leak.chapterIds", "Formal chapter IDs");
            AppendCheckLine(builder, checks, "leak.answerFields", "Build answer / key / drop-bias fields");
            AppendCheckLine(builder, checks, "leak.verifier.directLegacyUsing", "Direct legacy adapter dependency");
            AppendCheckLine(builder, checks, "leak.legacy.soleReader", "Sole E03 legacy reader invariant");
            builder.AppendLine();
            builder.AppendLine("## Protected sources and baselines");
            builder.AppendLine();
            builder.AppendLine("| Check | Expected | Actual | Result |");
            builder.AppendLine("|---|---|---|---|");
            foreach (Check check in checks.Where(value =>
                value.Id.StartsWith("protected.", StringComparison.Ordinal)
                || value.Id.StartsWith("baseline.", StringComparison.Ordinal)
                || value.Id.StartsWith("package.", StringComparison.Ordinal)))
            {
                builder.Append("| `").Append(Markdown(check.Id)).Append("` | ")
                    .Append(Markdown(check.Expected)).Append(" | ")
                    .Append(Markdown(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }
            return builder.ToString();
        }

        private static void AddFullOnlySignatureMutation(
            List<Check> checks,
            string id,
            EnemySkillBossPhaseCatalogSnapshot baseline,
            EnemySkillBossPhaseCatalogSnapshot mutated)
        {
            Add(checks, id, "full changed; player-safe unchanged",
                "full=" + (baseline.CanonicalSignature != mutated.CanonicalSignature)
                    + ";player=" + (baseline.PlayerSafeCanonicalSignature == mutated.PlayerSafeCanonicalSignature),
                baseline.CanonicalSignature != mutated.CanonicalSignature
                    && baseline.PlayerSafeCanonicalSignature == mutated.PlayerSafeCanonicalSignature);
        }

        private static void AddBothSignatureMutation(
            List<Check> checks,
            string id,
            EnemySkillBossPhaseCatalogSnapshot baseline,
            EnemySkillBossPhaseCatalogSnapshot mutated)
        {
            Add(checks, id, "full changed; player-safe changed",
                "full=" + (baseline.CanonicalSignature != mutated.CanonicalSignature)
                    + ";player=" + (baseline.PlayerSafeCanonicalSignature != mutated.PlayerSafeCanonicalSignature),
                baseline.CanonicalSignature != mutated.CanonicalSignature
                    && baseline.PlayerSafeCanonicalSignature != mutated.PlayerSafeCanonicalSignature);
        }

        private static void ExpectCode(
            List<Check> checks,
            string id,
            EnemySkillBossPhaseCatalogInput input,
            IEnemySkillBossPhaseReferenceResolver resolver,
            string expectedCode)
        {
            IReadOnlyList<EnemySkillBossPhaseValidationIssue> issues =
                DefaultEnemySkillBossPhaseValidator.Instance.Validate(input, resolver);
            string actual = string.Join(";", issues.Select(value => value.Code).Distinct(StringComparer.Ordinal));
            Add(checks, id, expectedCode, actual,
                issues.Any(value => string.Equals(value.Code, expectedCode, StringComparison.Ordinal)));
        }

        private static EnemySkillBossPhaseCatalogSnapshot Snapshot(
            EnemySkillBossPhaseCatalogInput input,
            IEnemySkillBossPhaseReferenceResolver resolver)
        {
            return DefaultEnemySkillBossPhaseProvider.Instance.CreateSnapshot(input, resolver);
        }

        private static bool IsReadOnly<T>(IReadOnlyList<T> values)
        {
            IList<T> generic = values as IList<T>;
            if (generic != null)
            {
                return generic.IsReadOnly;
            }

            IList nonGeneric = values as IList;
            return nonGeneric != null && nonGeneric.IsReadOnly;
        }

        private static bool IsCanonicalSignature(string value)
        {
            if (value == null || value.Length != 71 || !value.StartsWith("sha256:", StringComparison.Ordinal))
            {
                return false;
            }

            for (int index = 7; index < value.Length; index++)
            {
                char character = value[index];
                if (!((character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f')))
                {
                    return false;
                }
            }
            return true;
        }

        private static void AddHashChecks(
            List<Check> checks,
            string root,
            IReadOnlyDictionary<string, string> expectedHashes,
            string idPrefix,
            int expectedCount)
        {
            int unchanged = 0;
            foreach (KeyValuePair<string, string> pair in expectedHashes)
            {
                string path = Absolute(root, pair.Key);
                string actual = File.Exists(path) ? Sha256(path) : "MISSING";
                bool pass = string.Equals(actual, pair.Value, StringComparison.Ordinal);
                if (pass)
                {
                    unchanged++;
                }
                Add(checks, idPrefix + "." + Path.GetFileName(pair.Key), pair.Value, actual, pass);
            }
            Add(checks, idPrefix + ".summary", expectedCount + "/" + expectedCount + " unchanged",
                unchanged + "/" + expectedCount + " unchanged",
                expectedHashes.Count == expectedCount && unchanged == expectedCount);
        }

        private static string Sha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
            }
        }

        private static int Count(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token))
            {
                return 0;
            }
            int count = 0;
            int offset = 0;
            while ((offset = text.IndexOf(token, offset, StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }
            return count;
        }

        private static int CountTrailingWhitespace(string path)
        {
            return File.ReadAllLines(path, Encoding.UTF8)
                .Count(line => line.EndsWith(" ", StringComparison.Ordinal)
                    || line.EndsWith("\t", StringComparison.Ordinal));
        }

        private static string ReadMetaGuid(string path)
        {
            foreach (string line in File.ReadLines(path, Encoding.UTF8))
            {
                if (line.StartsWith("guid: ", StringComparison.Ordinal))
                {
                    return line.Substring("guid: ".Length).Trim();
                }
            }
            return string.Empty;
        }

        private static string[] ParseCsvLine(string line)
        {
            List<string> cells = new List<string>();
            StringBuilder cell = new StringBuilder();
            bool quoted = false;
            for (int index = 0; index < (line ?? string.Empty).Length; index++)
            {
                char character = line[index];
                if (character == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        cell.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (character == ',' && !quoted)
                {
                    cells.Add(cell.ToString());
                    cell.Clear();
                }
                else
                {
                    cell.Append(character);
                }
            }
            cells.Add(cell.ToString());
            return cells.ToArray();
        }

        private static string Csv(IEnumerable<string[]> rows)
        {
            return string.Join(
                "\n",
                rows.Select(row => string.Join(",", row.Select(CsvCell)))) + "\n";
        }

        private static string CsvCell(string value)
        {
            string text = value ?? string.Empty;
            if (text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return text;
            }
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private static string[] Field(
            string ownerType,
            string fieldName,
            string fieldType,
            string cardinality,
            string semantic,
            string visibility,
            string sourceOfTruth,
            string futureConsumer)
        {
            return new[]
            {
                ownerType, fieldName, fieldType, cardinality, semantic,
                visibility, sourceOfTruth, futureConsumer
            };
        }

        private static void WriteUtf8(string path, string content)
        {
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static string Markdown(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static void AppendCheckLine(
            StringBuilder builder,
            IReadOnlyList<Check> checks,
            string id,
            string label)
        {
            Check check = checks.LastOrDefault(value => string.Equals(value.Id, id, StringComparison.Ordinal));
            builder.Append("- ").Append(label).Append(": `")
                .Append(check != null && check.Passed ? "PASS" : "FAIL")
                .AppendLine("`");
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
            throw new DirectoryNotFoundException(
                "Could not locate the Unity project root from " + Directory.GetCurrentDirectory() + ".");
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

        private sealed class E02E03ReferenceResolver : IEnemySkillBossPhaseReferenceResolver
        {
            private readonly EnemyValidationContentSnapshot references;
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly HashSet<string> carrierBindings;

            public E02E03ReferenceResolver(
                EnemyValidationContentSnapshot references,
                EnemyMechanicVocabularySnapshot vocabulary)
            {
                this.references = references ?? throw new ArgumentNullException(nameof(references));
                this.vocabulary = vocabulary ?? throw new ArgumentNullException(nameof(vocabulary));
                carrierBindings = new HashSet<string>(
                    references.CarrierMechanicBindings.Select(value =>
                        CarrierBindingIdentity(
                            value.Kind == ValidationCarrierKind.Enemy
                                ? EnemySkillCarrierKind.Enemy
                                : EnemySkillCarrierKind.Boss,
                            value.CarrierId,
                            value.MechanicProfileId)),
                    StringComparer.Ordinal);
            }

            public bool TryGetEnemy(string stableId, out EnemyArchetypeSnapshot enemy)
            {
                if (references.TryGetEnemy(stableId, out NormalizedEnemyCarrierSnapshot normalized))
                {
                    enemy = normalized.Domain;
                    return true;
                }
                enemy = null;
                return false;
            }

            public bool TryGetBoss(string stableId, out BossArchetypeSnapshot boss)
            {
                if (references.TryGetBoss(stableId, out NormalizedBossCarrierSnapshot normalized))
                {
                    boss = normalized.Domain;
                    return true;
                }
                boss = null;
                return false;
            }

            public bool TryGetMechanicProfileKind(string stableId, out ValidationProfileKind kind)
            {
                if (references.TryGetMechanicProfile(stableId, out NormalizedMechanicProfileSnapshot profile))
                {
                    kind = profile.Kind;
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
                return carrierBindings.Contains(CarrierBindingIdentity(kind, carrierId, mechanicProfileId));
            }

            public bool HasVocabularyKey(EnemyVocabularyCategory category, string stableKey)
            {
                return vocabulary.TryGetEntry(category, stableKey, out _);
            }

            private static string CarrierBindingIdentity(
                EnemySkillCarrierKind kind,
                string carrierId,
                string mechanicProfileId)
            {
                return kind + "\u001f" + (carrierId ?? string.Empty) + "\u001f" + (mechanicProfileId ?? string.Empty);
            }
        }
    }
}
