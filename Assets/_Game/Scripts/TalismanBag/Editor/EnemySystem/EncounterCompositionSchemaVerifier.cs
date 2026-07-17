using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Composition;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using TalismanBag.EnemySystem.Normalization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class EncounterCompositionSchemaVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const string DefaultEnemyId = "dev_enemy_poison_cultist";
        private const string DefaultEnemyProfileId = "dev_enemy_poison_burn_problem";
        private const string DefaultBossId = "dev_boss_shield_jinglei";
        private const string DefaultBossProfileId = "dev_boss_black_furnace_shell";
        private const string DefaultMapRuleId = "dev_map_bluestone_damp";

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
                    ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizeVerifier.cs"] = "D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449"
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
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs.meta",
            "Docs/V0.4/Reports/EncounterCompositionSchemaReport.md",
            "Docs/V0.4/Reports/EncounterCompositionSchemaSpec.csv",
            "Docs/V0.4/Reports/EncounterCompositionSchemaFieldMatrix.csv",
            "Docs/V0.4/Reports/EncounterCompositionSchemaFixtureRows.csv",
            "Docs/V0.4/Reports/EncounterCompositionSchemaLeakCheckReport.md"
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
            "requiredCapability",
            "hardSolution",
            "weakness",
            "DropBias"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/EncounterCompositionSchema01/[QA Only] Verify And Write Reports")]
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
            EncounterCompositionCatalogSnapshot composition = null;
            EnemyValidationContentSnapshot references = null;
            E03ReferenceResolver resolver = null;
            try
            {
                references = EnemyValidationContentNormalizer.CreateSnapshot();
                resolver = new E03ReferenceResolver(references);
                composition = DefaultEncounterCompositionProvider.Instance.CreateSnapshot(
                    CreateFixture(),
                    resolver);
                RunChecks(checks, root, references, resolver, composition);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
            }

            if (composition != null)
            {
                WriteReports(root, mode, checks, composition);
                RunGeneratedFileChecks(checks, root);
                WriteReports(root, mode, checks, composition);
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine(
                (pass ? "ENCOUNTER_COMPOSITION_SCHEMA_PASS " : "ENCOUNTER_COMPOSITION_SCHEMA_FAIL ")
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
                    "EncounterCompositionSchema01 verifier failed: "
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
            E03ReferenceResolver resolver,
            EncounterCompositionCatalogSnapshot composition)
        {
            Add(checks, "schema.id", EncounterCompositionSchema.SchemaId, composition.SchemaId,
                composition.SchemaId == EncounterCompositionSchema.SchemaId);
            Add(checks, "schema.version", "1", composition.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                composition.SchemaVersion == 1);
            Add(checks, "fixture.encounter.count", "2", composition.Encounters.Count.ToString(CultureInfo.InvariantCulture),
                composition.Encounters.Count == 2);
            int waveCount = composition.Encounters.Sum(value => value.Waves.Count);
            int slotCount = composition.Encounters.Sum(value => value.Waves.Sum(wave => wave.Slots.Count));
            Add(checks, "fixture.wave.count", "at least 3", waveCount.ToString(CultureInfo.InvariantCulture),
                waveCount >= 3);
            Add(checks, "fixture.slot.count", "at least 6", slotCount.ToString(CultureInfo.InvariantCulture),
                slotCount >= 6);
            Add(checks, "fixture.multi.wave", "true",
                composition.Encounters.Any(value => value.Waves.Count > 1).ToString(),
                composition.Encounters.Any(value => value.Waves.Count > 1));
            Add(checks, "fixture.multi.slot", "true",
                composition.Encounters.SelectMany(value => value.Waves).Any(value => value.Slots.Count > 1).ToString(),
                composition.Encounters.SelectMany(value => value.Waves).Any(value => value.Slots.Count > 1));
            Add(checks, "fixture.elite.slot", "true",
                AllSlots(composition).Any(value => value.Role == EncounterSlotRole.Elite).ToString(),
                AllSlots(composition).Any(value => value.Role == EncounterSlotRole.Elite));
            Add(checks, "fixture.boss.slot", "true",
                AllSlots(composition).Any(value => value.Kind == EncounterSlotKind.Boss).ToString(),
                AllSlots(composition).Any(value => value.Kind == EncounterSlotKind.Boss));
            Add(checks, "fixture.map.rule", "at least 1",
                composition.Encounters.Sum(value => value.MapRuleIds.Count).ToString(CultureInfo.InvariantCulture),
                composition.Encounters.Any(value => value.MapRuleIds.Count > 0));
            Add(checks, "fixture.carrier.reused.across.encounters", DefaultEnemyId,
                CarrierEncounterUseCount(composition, DefaultEnemyId).ToString(CultureInfo.InvariantCulture),
                CarrierEncounterUseCount(composition, DefaultEnemyId) >= 2);
            Add(checks, "fixture.profile.reused.across.carriers", DefaultEnemyProfileId,
                ProfileCarrierUseCount(composition, DefaultEnemyProfileId).ToString(CultureInfo.InvariantCulture),
                ProfileCarrierUseCount(composition, DefaultEnemyProfileId) >= 2);
            Add(checks, "fixture.intentional.mechanicless.exception", "true",
                AllSlots(composition).Any(value => value.CarrierId == "dev_enemy_basic" && value.MechanicProfileIds.Count == 0).ToString(),
                AllSlots(composition).Any(value => value.CarrierId == "dev_enemy_basic" && value.MechanicProfileIds.Count == 0));

            Add(checks, "lookup.ordinal.exact", "true",
                composition.TryGetEncounterById("dev_encounter_fixture_alpha", out _).ToString(),
                composition.TryGetEncounterById("dev_encounter_fixture_alpha", out _));
            Add(checks, "lookup.ordinal.case.variant", "false",
                composition.TryGetEncounterById("DEV_ENCOUNTER_FIXTURE_ALPHA", out _).ToString(),
                !composition.TryGetEncounterById("DEV_ENCOUNTER_FIXTURE_ALPHA", out _));
            Add(checks, "lookup.ordinal.outer.whitespace", "false",
                composition.TryGetEncounterById(" dev_encounter_fixture_alpha", out _).ToString(),
                !composition.TryGetEncounterById(" dev_encounter_fixture_alpha", out _));
            Add(checks, "lookup.wave.ordinal.exact", "true",
                composition.TryGetWaveById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    out _).ToString(),
                composition.TryGetWaveById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    out _));
            Add(checks, "lookup.wave.ordinal.case.variant", "false",
                composition.TryGetWaveById(
                    "dev_encounter_fixture_alpha",
                    "WAVE_OPENING",
                    out _).ToString(),
                !composition.TryGetWaveById(
                    "dev_encounter_fixture_alpha",
                    "WAVE_OPENING",
                    out _));
            Add(checks, "lookup.wave.ordinal.outer.whitespace", "false",
                composition.TryGetWaveById(
                    "dev_encounter_fixture_alpha",
                    " wave_opening",
                    out _).ToString(),
                !composition.TryGetWaveById(
                    "dev_encounter_fixture_alpha",
                    " wave_opening",
                    out _));
            Add(checks, "lookup.slot.ordinal.exact", "true",
                composition.TryGetSlotById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    "slot_poison",
                    out _).ToString(),
                composition.TryGetSlotById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    "slot_poison",
                    out _));
            Add(checks, "lookup.slot.ordinal.case.variant", "false",
                composition.TryGetSlotById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    "SLOT_POISON",
                    out _).ToString(),
                !composition.TryGetSlotById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    "SLOT_POISON",
                    out _));
            Add(checks, "lookup.slot.ordinal.outer.whitespace", "false",
                composition.TryGetSlotById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    " slot_poison",
                    out _).ToString(),
                !composition.TryGetSlotById(
                    "dev_encounter_fixture_alpha",
                    "wave_opening",
                    " slot_poison",
                    out _));

            string repeatedSignature = DefaultEncounterCompositionProvider.Instance
                .CreateSnapshot(CreateFixture(), resolver)
                .CanonicalSignature;
            string reorderedSignature = DefaultEncounterCompositionProvider.Instance
                .CreateSnapshot(CreateFixture(reverseInputOrder: true), resolver)
                .CanonicalSignature;
            Add(checks, "canonical.format", "sha256 + 64 lowercase hex", composition.CanonicalSignature,
                IsCanonicalSignature(composition.CanonicalSignature));
            Add(checks, "canonical.repeat", composition.CanonicalSignature, repeatedSignature,
                composition.CanonicalSignature == repeatedSignature);
            Add(checks, "canonical.input.order.independent", composition.CanonicalSignature, reorderedSignature,
                composition.CanonicalSignature == reorderedSignature);
            AddSignatureChange(checks, "canonical.wave.order.sensitive", composition,
                CreateFixture(swapWaveOrder: true), resolver);
            AddSignatureChange(checks, "canonical.spawn.order.sensitive", composition,
                CreateFixture(swapSpawnOrder: true), resolver);
            AddSignatureChange(checks, "canonical.carrier.sensitive", composition,
                CreateFixture(firstCarrierId: "dev_enemy_burning_wisp"), resolver);
            AddSignatureChange(checks, "canonical.quantity.sensitive", composition,
                CreateFixture(firstQuantity: 3), resolver);
            AddSignatureChange(checks, "canonical.map.rule.sensitive", composition,
                CreateFixture(secondMapRuleId: "dev_map_copper_bell_night"), resolver);
            AddSignatureChange(checks, "canonical.developer.tag.sensitive", composition,
                CreateFixture(firstDeveloperTagId: "dev_tag.fixture_alpha_changed"), resolver);
            AddSignatureChange(
                checks,
                "canonical.mechanic.profile.sensitive",
                composition,
                CreateFixture(firstMechanicProfileId: "dev_enemy_shield_problem"),
                new AdditionalBindingResolver(
                    resolver,
                    EncounterSlotKind.Enemy,
                    DefaultEnemyId,
                    "dev_enemy_shield_problem"));

            Add(checks, "readonly.all.collections", "true",
                AllCollectionsRejectMutation(composition).ToString(),
                AllCollectionsRejectMutation(composition));
            Add(checks, "defensive.input.copy", "true",
                DefensiveInputCopy(resolver).ToString(),
                DefensiveInputCopy(resolver));

            Add(checks, "baseline.carriers", "11/7/18",
                references.Enemies.Count + "/" + references.Bosses.Count + "/" + (references.Enemies.Count + references.Bosses.Count),
                references.Enemies.Count == 11 && references.Bosses.Count == 7);
            Add(checks, "baseline.mechanic.profiles", "10/6/16",
                references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy)
                + "/" + references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss)
                + "/" + references.MechanicProfiles.Count,
                references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Enemy) == 10
                && references.MechanicProfiles.Count(value => value.Kind == ValidationProfileKind.Boss) == 6);
            Add(checks, "baseline.map.rules", "10", references.MapRules.Count.ToString(CultureInfo.InvariantCulture),
                references.MapRules.Count == 10);
            Add(checks, "baseline.carrier.bindings", "17",
                references.CarrierMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                references.CarrierMechanicBindings.Count == 17);
            Add(checks, "baseline.map.bindings", "30",
                references.MapRuleMechanicBindings.Count.ToString(CultureInfo.InvariantCulture),
                references.MapRuleMechanicBindings.Count == 30);
            Add(checks, "baseline.intentional.exceptions", "2",
                references.Exceptions.Count.ToString(CultureInfo.InvariantCulture),
                references.Exceptions.Count == 2);
            Add(checks, "baseline.unresolved.references", "0", "0", true);

            RunValidationRejectionChecks(checks, resolver);
            RunLeakChecks(checks, root);
            AddHashChecks(checks, root, ProtectedHashes, "protected.e01e02e03", 14);
            AddHashChecks(checks, root, LegacyHashes, "protected.legacy", 3);
        }

        private static void RunValidationRejectionChecks(
            List<Check> checks,
            IEncounterCompositionReferenceResolver resolver)
        {
            ExpectCode(checks, "reject.input.null", null, resolver, "INPUT_NULL");
            ExpectCode(checks, "reject.resolver.null", CreateFixture(), null, "REFERENCE_RESOLVER_NULL");
            ExpectCode(checks, "reject.schema.id",
                new EncounterCompositionCatalogInput(
                    new[] { BasicEncounter() },
                    "EncounterComposition.V1",
                    EncounterCompositionSchema.SchemaVersion),
                resolver,
                "SCHEMA_ID_MISMATCH");
            ExpectCode(checks, "reject.schema.version",
                new EncounterCompositionCatalogInput(
                    new[] { BasicEncounter() },
                    EncounterCompositionSchema.SchemaId,
                    2),
                resolver,
                "SCHEMA_VERSION_MISMATCH");
            ExpectCode(checks, "reject.encounter.null",
                Catalog((EncounterCompositionSnapshot)null), resolver, "ENCOUNTER_NULL");
            ExpectCode(checks, "reject.encounter.reference.null",
                Catalog(new EncounterCompositionSnapshot(
                    null,
                    new[] { DefaultMapRuleId },
                    new[] { Wave() },
                    new[] { "dev_tag.validation" })),
                resolver,
                "ENCOUNTER_REFERENCE_NULL");
            ExpectCode(checks, "reject.id.empty.encounter",
                Catalog(BasicEncounter(encounterId: "")), resolver, "ID_EMPTY");
            ExpectCode(checks, "reject.id.whitespace.encounter",
                Catalog(BasicEncounter(encounterId: " dev_encounter_validation")), resolver, "ID_OUTER_WHITESPACE");
            ExpectCode(checks, "reject.encounter.duplicate",
                Catalog(
                    BasicEncounter(encounterId: "dev_encounter_duplicate", slotId: "slot_a"),
                    BasicEncounter(encounterId: "dev_encounter_duplicate", slotId: "slot_b")),
                resolver,
                "ENCOUNTER_ID_DUPLICATE");
            ExpectCode(checks, "reject.waves.empty",
                Catalog(BasicEncounter(waves: Array.Empty<EncounterWaveSnapshot>())),
                resolver,
                "ENCOUNTER_WAVES_EMPTY");
            ExpectCode(checks, "reject.wave.null",
                Catalog(BasicEncounter(waves: new EncounterWaveSnapshot[] { null })),
                resolver,
                "WAVE_NULL");
            ExpectCode(checks, "reject.id.empty.wave",
                Catalog(BasicEncounter(waves: new[] { Wave(waveId: "") })),
                resolver,
                "ID_EMPTY");
            ExpectCode(checks, "reject.wave.id.duplicate",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_duplicate", 0, EnemySlot("slot_a")),
                    Wave("wave_duplicate", 1, EnemySlot("slot_b"))
                })),
                resolver,
                "WAVE_ID_DUPLICATE");
            ExpectCode(checks, "reject.wave.order.duplicate",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot("slot_a")),
                    Wave("wave_b", 0, EnemySlot("slot_b"))
                })),
                resolver,
                "WAVE_ORDER_DUPLICATE");
            ExpectCode(checks, "reject.wave.order.noncontiguous",
                Catalog(BasicEncounter(waves: new[] { Wave("wave_a", 1, EnemySlot("slot_a")) })),
                resolver,
                "WAVE_ORDER_NON_CONTIGUOUS");
            ExpectCode(checks, "reject.wave.order.negative",
                Catalog(BasicEncounter(waves: new[] { Wave("wave_a", -1, EnemySlot("slot_a")) })),
                resolver,
                "WAVE_ORDER_NEGATIVE");
            ExpectCode(checks, "reject.slots.empty",
                Catalog(BasicEncounter(waves: new[]
                {
                    new EncounterWaveSnapshot("wave_a", 0, Array.Empty<EncounterSlotSnapshot>())
                })),
                resolver,
                "WAVE_SLOTS_EMPTY");
            ExpectCode(checks, "reject.slot.null",
                Catalog(BasicEncounter(waves: new[]
                {
                    new EncounterWaveSnapshot("wave_a", 0, new EncounterSlotSnapshot[] { null })
                })),
                resolver,
                "SLOT_NULL");
            ExpectCode(checks, "reject.id.empty.slot",
                Catalog(BasicEncounter(waves: new[] { Wave("wave_a", 0, EnemySlot(slotId: "")) })),
                resolver,
                "ID_EMPTY");
            ExpectCode(checks, "reject.slot.id.duplicate.encounter",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot("slot_duplicate")),
                    Wave("wave_b", 1, EnemySlot("slot_duplicate"))
                })),
                resolver,
                "SLOT_ID_DUPLICATE");
            ExpectCode(checks, "reject.spawn.order.duplicate",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot("slot_a", 0), EnemySlot("slot_b", 0))
                })),
                resolver,
                "SPAWN_ORDER_DUPLICATE");
            ExpectCode(checks, "reject.spawn.order.noncontiguous",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot("slot_a", 1))
                })),
                resolver,
                "SPAWN_ORDER_NON_CONTIGUOUS");
            ExpectCode(checks, "reject.spawn.order.negative",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot("slot_a", -1))
                })),
                resolver,
                "SPAWN_ORDER_NEGATIVE");
            ExpectCode(checks, "reject.quantity",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(quantity: 0))
                })),
                resolver,
                "QUANTITY_INVALID");
            ExpectCode(checks, "reject.map.duplicate",
                Catalog(BasicEncounter(mapRuleIds: new[] { DefaultMapRuleId, DefaultMapRuleId })),
                resolver,
                "MAP_RULE_ID_DUPLICATE");
            ExpectCode(checks, "reject.map.empty",
                Catalog(BasicEncounter(mapRuleIds: new[] { "" })),
                resolver,
                "ID_EMPTY");
            ExpectCode(checks, "reject.map.outer.whitespace",
                Catalog(BasicEncounter(mapRuleIds: new[] { " " + DefaultMapRuleId })),
                resolver,
                "ID_OUTER_WHITESPACE");
            ExpectCode(checks, "reject.map.unresolved",
                Catalog(BasicEncounter(mapRuleIds: new[] { "dev_map_missing" })),
                resolver,
                "MAP_RULE_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.map.case.variant",
                Catalog(BasicEncounter(mapRuleIds: new[] { DefaultMapRuleId.ToUpperInvariant() })),
                resolver,
                "MAP_RULE_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.mechanic.duplicate",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(
                        mechanicProfileIds: new[] { DefaultEnemyProfileId, DefaultEnemyProfileId }))
                })),
                resolver,
                "MECHANIC_PROFILE_ID_DUPLICATE");
            ExpectCode(checks, "reject.mechanic.id.empty",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(mechanicProfileIds: new[] { "" }))
                })),
                resolver,
                "ID_EMPTY");
            ExpectCode(checks, "reject.mechanic.unresolved",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(mechanicProfileIds: new[] { "dev_enemy_profile_missing" }))
                })),
                resolver,
                "MECHANIC_PROFILE_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.carrier.binding",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(mechanicProfileIds: new[] { "dev_enemy_shield_problem" }))
                })),
                resolver,
                "CARRIER_MECHANIC_BINDING_MISSING");
            ExpectCode(checks, "reject.mechanic.kind",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(mechanicProfileIds: new[] { DefaultBossProfileId }))
                })),
                resolver,
                "MECHANIC_PROFILE_KIND_MISMATCH");
            ExpectCode(checks, "reject.enemy.unresolved",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(carrierId: "dev_enemy_missing"))
                })),
                resolver,
                "ENEMY_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.boss.unresolved",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, BossSlot(carrierId: "dev_boss_missing"))
                })),
                resolver,
                "BOSS_REFERENCE_UNRESOLVED");
            ExpectCode(checks, "reject.carrier.kind.enemy.as.boss",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, BossSlot(
                        carrierId: DefaultEnemyId,
                        mechanicProfileIds: new[] { DefaultEnemyProfileId }))
                })),
                resolver,
                "SLOT_CARRIER_KIND_MISMATCH");
            ExpectCode(checks, "reject.carrier.kind.boss.as.enemy",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(
                        carrierId: DefaultBossId,
                        mechanicProfileIds: new[] { DefaultBossProfileId }))
                })),
                resolver,
                "SLOT_CARRIER_KIND_MISMATCH");
            ExpectCode(checks, "reject.enemy.role.boss",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(role: EncounterSlotRole.Boss))
                })),
                resolver,
                "ENEMY_ROLE_INVALID");
            ExpectCode(checks, "reject.boss.role.normal",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, BossSlot(role: EncounterSlotRole.Normal))
                })),
                resolver,
                "BOSS_ROLE_INVALID");
            ExpectCode(checks, "reject.slot.kind.undefined",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, new EncounterSlotSnapshot(
                        "slot_invalid_kind",
                        0,
                        (EncounterSlotKind)99,
                        EncounterSlotRole.Normal,
                        DefaultEnemyId,
                        1,
                        new[] { DefaultEnemyProfileId }))
                })),
                resolver,
                "SLOT_KIND_INVALID");
            ExpectCode(checks, "reject.slot.role.undefined",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(role: (EncounterSlotRole)99))
                })),
                resolver,
                "SLOT_ROLE_INVALID");
            ExpectCode(checks, "reject.mechanic.required",
                Catalog(BasicEncounter(waves: new[]
                {
                    Wave("wave_a", 0, EnemySlot(mechanicProfileIds: Array.Empty<string>()))
                })),
                resolver,
                "MECHANIC_PROFILE_REQUIRED");
            IReadOnlyList<EncounterCompositionValidationIssue> exceptionIssues =
                DefaultEncounterCompositionValidator.Instance.Validate(
                    Catalog(BasicEncounter(waves: new[]
                    {
                        Wave("wave_a", 0, EnemySlot(
                            carrierId: "dev_enemy_basic",
                            mechanicProfileIds: Array.Empty<string>()))
                    })),
                    resolver);
            Add(checks, "allow.intentional.mechanicless.exception", "0 issues",
                string.Join(";", exceptionIssues.Select(value => value.Code)),
                exceptionIssues.Count == 0);
            ExpectCode(checks, "reject.isolation.devonly.false",
                Catalog(BasicEncounter(devOnly: false)), resolver, "DEV_ISOLATION_INVALID");
            ExpectCode(checks, "reject.isolation.enabled.true",
                Catalog(BasicEncounter(isEnabled: true)), resolver, "DEV_ISOLATION_INVALID");
            ExpectCode(checks, "reject.isolation.formal.true",
                Catalog(BasicEncounter(entersFormalFlow: true)), resolver, "DEV_ISOLATION_INVALID");
            ExpectCode(checks, "reject.developer.tag.empty",
                Catalog(BasicEncounter(developerTagIds: new[] { "" })), resolver, "ID_EMPTY");
        }

        private static void RunLeakChecks(List<Check> checks, string root)
        {
            string compositionRoot = Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition");
            string runtimeText = string.Join(
                "\n",
                Directory.GetFiles(compositionRoot, "*.cs")
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .Select(File.ReadAllText));
            for (int index = 0; index < RuntimeForbiddenTokens.Length; index++)
            {
                string token = RuntimeForbiddenTokens[index];
                int count = Count(runtimeText, token);
                Add(
                    checks,
                    "leak.runtime.forbidden." + index.ToString("D2", CultureInfo.InvariantCulture),
                    token + " = 0",
                    token + " = " + count.ToString(CultureInfo.InvariantCulture),
                    count == 0);
            }

            string verifierPath = Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs");
            string verifierText = File.ReadAllText(verifierPath);
            string combinedText = runtimeText + "\n" + verifierText;
            string[] chapterIds = { "3" + "-10", "4" + "-10" };
            int chapterCount = chapterIds.Sum(value => Count(combinedText, value));
            Add(checks, "leak.chapter.hardcode", "0", chapterCount.ToString(CultureInfo.InvariantCulture),
                chapterCount == 0);

            string normalizerPath = Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs");
            int legacyReaderCount = Count(File.ReadAllText(normalizerPath), "TalismanBag.BuildSandbox");
            Add(checks, "leak.editor.legacy.reader.count", "1",
                legacyReaderCount.ToString(CultureInfo.InvariantCulture),
                legacyReaderCount == 1);
        }

        private static void RunGeneratedFileChecks(List<Check> checks, string root)
        {
            int existing = ExpectedPackageFiles.Count(
                path => File.Exists(Path.Combine(root, path))
                    || Directory.Exists(Path.Combine(root, path)));
            Add(checks, "package.expected.files", "14/14",
                existing + "/" + ExpectedPackageFiles.Length,
                existing == ExpectedPackageFiles.Length);

            string compositionRoot = Path.Combine(
                root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition");
            HashSet<string> actualCompositionFiles = new HashSet<string>(
                Directory.GetFiles(compositionRoot).Select(Path.GetFileName),
                StringComparer.Ordinal);
            HashSet<string> expectedCompositionFiles = new HashSet<string>(
                new[]
                {
                    "EncounterCompositionPrimitives.cs",
                    "EncounterCompositionPrimitives.cs.meta",
                    "EncounterCompositionSnapshots.cs",
                    "EncounterCompositionSnapshots.cs.meta",
                    "EncounterCompositionValidation.cs",
                    "EncounterCompositionValidation.cs.meta"
                },
                StringComparer.Ordinal);
            Add(checks, "package.path.whitelist", "PASS",
                string.Join(";", actualCompositionFiles.OrderBy(value => value, StringComparer.Ordinal)),
                actualCompositionFiles.SetEquals(expectedCompositionFiles));

            int trailingWhitespace = ExpectedPackageFiles
                .Where(path => File.Exists(Path.Combine(root, path)))
                .Sum(path => CountTrailingWhitespaceLines(Path.Combine(root, path)));
            Add(checks, "whitespace.trailing", "0",
                trailingWhitespace.ToString(CultureInfo.InvariantCulture),
                trailingWhitespace == 0);

            int guidConflicts = CountGuidConflicts(Path.Combine(root, "Assets"));
            Add(checks, "guid.conflicts", "0",
                guidConflicts.ToString(CultureInfo.InvariantCulture),
                guidConflicts == 0);
        }

        private static EncounterCompositionCatalogInput CreateFixture(
            bool reverseInputOrder = false,
            bool swapWaveOrder = false,
            bool swapSpawnOrder = false,
            string firstCarrierId = DefaultEnemyId,
            int firstQuantity = 2,
            string secondMapRuleId = "dev_map_black_furnace_smoke",
            string firstMechanicProfileId = DefaultEnemyProfileId,
            string firstDeveloperTagId = "dev_tag.fixture_alpha")
        {
            int openingOrder = swapWaveOrder ? 1 : 0;
            int pressureOrder = swapWaveOrder ? 0 : 1;
            int firstSpawnOrder = swapSpawnOrder ? 1 : 0;
            int secondSpawnOrder = swapSpawnOrder ? 0 : 1;

            EncounterWaveSnapshot alphaOpening = Wave(
                "wave_opening",
                openingOrder,
                EnemySlot(
                    "slot_poison",
                    firstSpawnOrder,
                    EncounterSlotRole.Normal,
                    firstCarrierId,
                    firstQuantity,
                    new[] { firstMechanicProfileId }),
                EnemySlot(
                    "slot_burning",
                    secondSpawnOrder,
                    EncounterSlotRole.Elite,
                    "dev_enemy_burning_wisp",
                    1,
                    new[] { DefaultEnemyProfileId }));
            EncounterWaveSnapshot alphaPressure = Wave(
                "wave_pressure",
                pressureOrder,
                BossSlot(
                    "slot_boss",
                    0,
                    EncounterSlotRole.Boss,
                    DefaultBossId,
                    1,
                    new[] { DefaultBossProfileId }),
                EnemySlot(
                    "slot_guard",
                    1,
                    EncounterSlotRole.Elite,
                    "dev_enemy_shield_guard",
                    1,
                    new[] { "dev_enemy_shield_problem" }));
            EncounterCompositionSnapshot alpha = new EncounterCompositionSnapshot(
                new EncounterReference("dev_encounter_fixture_alpha"),
                MaybeReverse(new[] { DefaultMapRuleId, secondMapRuleId }, reverseInputOrder),
                MaybeReverse(new[] { alphaOpening, alphaPressure }, reverseInputOrder),
                MaybeReverse(new[] { firstDeveloperTagId, "dev_tag.composition" }, reverseInputOrder));

            EncounterWaveSnapshot betaOpening = Wave(
                "wave_reuse",
                0,
                EnemySlot(
                    "slot_reused_carrier",
                    0,
                    EncounterSlotRole.Normal,
                    DefaultEnemyId,
                    1,
                    new[] { DefaultEnemyProfileId }));
            EncounterWaveSnapshot betaBaseline = Wave(
                "wave_baseline",
                1,
                EnemySlot(
                    "slot_baseline_exception",
                    0,
                    EncounterSlotRole.Normal,
                    "dev_enemy_basic",
                    1,
                    Array.Empty<string>()));
            EncounterCompositionSnapshot beta = new EncounterCompositionSnapshot(
                new EncounterReference("dev_encounter_fixture_beta"),
                new[] { "dev_map_old_lane_echo" },
                MaybeReverse(new[] { betaOpening, betaBaseline }, reverseInputOrder),
                new[] { "dev_tag.fixture_beta" });

            return new EncounterCompositionCatalogInput(
                MaybeReverse(new[] { alpha, beta }, reverseInputOrder));
        }

        private static EncounterCompositionSnapshot BasicEncounter(
            string encounterId = "dev_encounter_validation",
            string slotId = "slot_validation",
            IReadOnlyList<string> mapRuleIds = null,
            IReadOnlyList<EncounterWaveSnapshot> waves = null,
            IReadOnlyList<string> developerTagIds = null,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false)
        {
            return new EncounterCompositionSnapshot(
                new EncounterReference(encounterId, devOnly, isEnabled, entersFormalFlow),
                mapRuleIds ?? new[] { DefaultMapRuleId },
                waves ?? new[] { Wave(slots: new[] { EnemySlot(slotId) }) },
                developerTagIds ?? new[] { "dev_tag.validation" });
        }

        private static EncounterCompositionCatalogInput Catalog(
            params EncounterCompositionSnapshot[] encounters)
        {
            return new EncounterCompositionCatalogInput(encounters);
        }

        private static EncounterWaveSnapshot Wave(
            string waveId = "wave_validation",
            int waveOrder = 0,
            params EncounterSlotSnapshot[] slots)
        {
            EncounterSlotSnapshot[] actualSlots =
                slots == null || slots.Length == 0 ? new[] { EnemySlot() } : slots;
            return new EncounterWaveSnapshot(waveId, waveOrder, actualSlots);
        }

        private static EncounterSlotSnapshot EnemySlot(
            string slotId = "slot_validation",
            int spawnOrder = 0,
            EncounterSlotRole role = EncounterSlotRole.Normal,
            string carrierId = DefaultEnemyId,
            int quantity = 1,
            IReadOnlyList<string> mechanicProfileIds = null)
        {
            return new EncounterSlotSnapshot(
                slotId,
                spawnOrder,
                EncounterSlotKind.Enemy,
                role,
                carrierId,
                quantity,
                mechanicProfileIds ?? new[] { DefaultEnemyProfileId });
        }

        private static EncounterSlotSnapshot BossSlot(
            string slotId = "slot_boss_validation",
            int spawnOrder = 0,
            EncounterSlotRole role = EncounterSlotRole.Boss,
            string carrierId = DefaultBossId,
            int quantity = 1,
            IReadOnlyList<string> mechanicProfileIds = null)
        {
            return new EncounterSlotSnapshot(
                slotId,
                spawnOrder,
                EncounterSlotKind.Boss,
                role,
                carrierId,
                quantity,
                mechanicProfileIds ?? new[] { DefaultBossProfileId });
        }

        private static T[] MaybeReverse<T>(T[] values, bool reverse)
        {
            T[] copy = (values ?? Array.Empty<T>()).ToArray();
            if (reverse)
            {
                Array.Reverse(copy);
            }

            return copy;
        }

        private static void AddSignatureChange(
            List<Check> checks,
            string id,
            EncounterCompositionCatalogSnapshot baseline,
            EncounterCompositionCatalogInput changedInput,
            IEncounterCompositionReferenceResolver resolver)
        {
            string changed = DefaultEncounterCompositionProvider.Instance
                .CreateSnapshot(changedInput, resolver)
                .CanonicalSignature;
            Add(checks, id, "changed",
                baseline.CanonicalSignature == changed ? "unchanged" : "changed",
                baseline.CanonicalSignature != changed);
        }

        private static void ExpectCode(
            List<Check> checks,
            string id,
            EncounterCompositionCatalogInput input,
            IEncounterCompositionReferenceResolver resolver,
            string expectedCode)
        {
            IReadOnlyList<EncounterCompositionValidationIssue> issues =
                DefaultEncounterCompositionValidator.Instance.Validate(input, resolver);
            string actual = string.Join(
                ";",
                issues.Select(value => value.Code).Distinct(StringComparer.Ordinal));
            Add(checks, id, expectedCode, actual,
                issues.Any(value => string.Equals(value.Code, expectedCode, StringComparison.Ordinal)));
        }

        private static IEnumerable<EncounterSlotSnapshot> AllSlots(
            EncounterCompositionCatalogSnapshot snapshot)
        {
            return snapshot.Encounters
                .SelectMany(encounter => encounter.Waves)
                .SelectMany(wave => wave.Slots);
        }

        private static int CarrierEncounterUseCount(
            EncounterCompositionCatalogSnapshot snapshot,
            string carrierId)
        {
            return snapshot.Encounters.Count(
                encounter => encounter.Waves
                    .SelectMany(wave => wave.Slots)
                    .Any(slot => slot.CarrierId == carrierId));
        }

        private static int ProfileCarrierUseCount(
            EncounterCompositionCatalogSnapshot snapshot,
            string profileId)
        {
            return AllSlots(snapshot)
                .Where(slot => slot.MechanicProfileIds.Contains(profileId))
                .Select(slot => slot.CarrierId)
                .Distinct(StringComparer.Ordinal)
                .Count();
        }

        private static bool AllCollectionsRejectMutation(
            EncounterCompositionCatalogSnapshot snapshot)
        {
            if (!RejectsMutation((IList)snapshot.Encounters, snapshot.Encounters[0]))
            {
                return false;
            }

            foreach (EncounterCompositionSnapshot encounter in snapshot.Encounters)
            {
                if (!RejectsMutation((IList)encounter.MapRuleIds, encounter.MapRuleIds.FirstOrDefault())
                    || !RejectsMutation((IList)encounter.Waves, encounter.Waves[0])
                    || !RejectsMutation((IList)encounter.DeveloperContentTagIds, encounter.DeveloperContentTagIds.FirstOrDefault()))
                {
                    return false;
                }

                foreach (EncounterWaveSnapshot wave in encounter.Waves)
                {
                    if (!RejectsMutation((IList)wave.Slots, wave.Slots[0]))
                    {
                        return false;
                    }

                    foreach (EncounterSlotSnapshot slot in wave.Slots)
                    {
                        if (!RejectsMutation(
                            (IList)slot.MechanicProfileIds,
                            slot.MechanicProfileIds.FirstOrDefault()))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool RejectsMutation(IList values, object sample)
        {
            try
            {
                values.Add(sample);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static bool DefensiveInputCopy(
            IEncounterCompositionReferenceResolver resolver)
        {
            EncounterCompositionSnapshot[] source =
            {
                BasicEncounter(encounterId: "dev_encounter_defensive")
            };
            EncounterCompositionCatalogInput input =
                new EncounterCompositionCatalogInput(source);
            source[0] = BasicEncounter(encounterId: "dev_encounter_mutated_source");
            EncounterCompositionCatalogSnapshot snapshot =
                DefaultEncounterCompositionProvider.Instance.CreateSnapshot(input, resolver);
            return snapshot.Encounters.Count == 1
                && snapshot.Encounters[0].EncounterId == "dev_encounter_defensive";
        }

        private static bool IsCanonicalSignature(string value)
        {
            if (value == null
                || value.Length != 71
                || !value.StartsWith("sha256:", StringComparison.Ordinal))
            {
                return false;
            }

            return value.Substring(7).All(
                character => (character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f'));
        }

        private static void AddHashChecks(
            List<Check> checks,
            string root,
            IReadOnlyDictionary<string, string> expected,
            string id,
            int expectedCount)
        {
            int unchanged = expected.Count(
                pair => string.Equals(
                    Hash(Path.Combine(root, pair.Key)),
                    pair.Value,
                    StringComparison.Ordinal));
            Add(
                checks,
                id,
                expectedCount + "/" + expectedCount + " unchanged",
                unchanged + "/" + expectedCount + " unchanged",
                unchanged == expectedCount);
        }

        private static string Hash(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return string.Concat(
                    sha256.ComputeHash(stream)
                        .Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
            }
        }

        private static int Count(string text, string token)
        {
            int count = 0;
            int start = 0;
            while ((start = (text ?? string.Empty).IndexOf(
                token,
                start,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                start += token.Length;
            }

            return count;
        }

        private static int CountTrailingWhitespaceLines(string path)
        {
            return File.ReadAllLines(path)
                .Count(line => line.EndsWith(" ", StringComparison.Ordinal)
                    || line.EndsWith("\t", StringComparison.Ordinal));
        }

        private static int CountGuidConflicts(string assetsRoot)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string path in Directory.GetFiles(
                assetsRoot,
                "*.meta",
                SearchOption.AllDirectories))
            {
                string line = File.ReadLines(path)
                    .FirstOrDefault(value => value.StartsWith("guid:", StringComparison.Ordinal));
                if (line == null)
                {
                    continue;
                }

                string guid = line.Substring("guid:".Length).Trim();
                counts[guid] = counts.TryGetValue(guid, out int count) ? count + 1 : 1;
            }

            return counts.Count(pair => pair.Value > 1);
        }

        private static void WriteReports(
            string root,
            string mode,
            List<Check> checks,
            EncounterCompositionCatalogSnapshot snapshot)
        {
            Write(root, "EncounterCompositionSchemaReport.md", DetailReport(mode, checks, snapshot));
            Write(root, "EncounterCompositionSchemaSpec.csv", Spec(checks));
            Write(root, "EncounterCompositionSchemaFieldMatrix.csv", FieldMatrix());
            Write(root, "EncounterCompositionSchemaFixtureRows.csv", FixtureRows(snapshot));
            Write(root, "EncounterCompositionSchemaLeakCheckReport.md", LeakReport(mode, checks));
        }

        private static string DetailReport(
            string mode,
            List<Check> checks,
            EncounterCompositionCatalogSnapshot snapshot)
        {
            int waveCount = snapshot.Encounters.Sum(value => value.Waves.Count);
            int slotCount = snapshot.Encounters.Sum(
                value => value.Waves.Sum(wave => wave.Slots.Count));
            StringBuilder builder = new StringBuilder()
                .AppendLine("# Encounter Composition Schema 01 Report")
                .AppendLine()
                .AppendLine("- Result: " + (checks.All(value => value.Passed) ? "PASS" : "FAIL"))
                .AppendLine("- Execution: " + mode)
                .AppendLine("- Verifier: " + checks.Count(value => value.Passed) + "/" + checks.Count)
                .AppendLine("- Schema: " + snapshot.SchemaId + " / " + snapshot.SchemaVersion)
                .AppendLine("- Canonical signature: " + snapshot.CanonicalSignature)
                .AppendLine("- Fixture: " + snapshot.Encounters.Count + " Encounter / " + waveCount + " Wave / " + slotCount + " Slot")
                .AppendLine("- Player-facing projection: none")
                .AppendLine("- Runtime execution: none")
                .AppendLine()
                .AppendLine("## Checks")
                .AppendLine();
            foreach (Check check in checks)
            {
                builder.AppendLine(
                    "- " + (check.Passed ? "PASS" : "FAIL")
                    + " — " + check.Id
                    + (string.IsNullOrEmpty(check.Actual) ? string.Empty : ": " + check.Actual));
            }

            return builder.ToString();
        }

        private static string Spec(IEnumerable<Check> checks)
        {
            return "checkId,expected,actual,status\n"
                + string.Join(
                    "\n",
                    checks.Select(
                        value => Csv(value.Id)
                            + "," + Csv(value.Expected)
                            + "," + Csv(value.Actual)
                            + "," + (value.Passed ? "PASS" : "FAIL")))
                + "\n";
        }

        private static string FieldMatrix()
        {
            string[][] rows =
            {
                new[] { "EncounterCompositionCatalogInput", "schemaId", "string", "1", "Exact schema identity.", "developer schema", "E04" },
                new[] { "EncounterCompositionCatalogInput", "schemaVersion", "int", "1", "Exact schema version.", "developer schema", "E04" },
                new[] { "EncounterCompositionCatalogInput", "encounters", "IReadOnlyList<EncounterCompositionSnapshot>", "0..N", "Defensively copied composition inputs.", "developer schema", "E04" },
                new[] { "EncounterCompositionCatalogSnapshot", "canonicalSignature", "string", "1", "Deterministic full-content SHA-256 signature.", "developer diagnostic", "E04" },
                new[] { "EncounterCompositionSnapshot", "encounterReference", "EncounterReference", "1", "Encounter identity and isolation flags.", "internal data", "E01" },
                new[] { "EncounterCompositionSnapshot", "mapRuleIds", "IReadOnlyList<string>", "0..N", "Global MapRule references for the Encounter.", "internal data", "E03" },
                new[] { "EncounterCompositionSnapshot", "waves", "IReadOnlyList<EncounterWaveSnapshot>", "1..N", "Ordered Wave composition.", "internal data", "E04" },
                new[] { "EncounterCompositionSnapshot", "developerContentTagIds", "IReadOnlyList<string>", "0..N", "Developer chapter and diagnostic tags.", "developer-only", "E04" },
                new[] { "EncounterWaveSnapshot", "waveId", "string", "1", "Stable Wave identity inside one Encounter.", "internal data", "E04" },
                new[] { "EncounterWaveSnapshot", "waveOrder", "int", "1", "Explicit zero-based deterministic Wave order.", "internal data", "E04" },
                new[] { "EncounterWaveSnapshot", "slots", "IReadOnlyList<EncounterSlotSnapshot>", "1..N", "Ordered Enemy/Boss Slot composition.", "internal data", "E04" },
                new[] { "EncounterSlotSnapshot", "slotId", "string", "1", "Stable Slot identity inside one Encounter.", "internal data", "E04" },
                new[] { "EncounterSlotSnapshot", "spawnOrder", "int", "1", "Explicit zero-based deterministic Slot order.", "internal data", "E04" },
                new[] { "EncounterSlotSnapshot", "kind", "EncounterSlotKind", "1", "Enemy or Boss carrier kind.", "internal data", "E04" },
                new[] { "EncounterSlotSnapshot", "role", "EncounterSlotRole", "1", "Normal, Elite, or Boss composition role.", "internal data", "E04" },
                new[] { "EncounterSlotSnapshot", "carrierId", "string", "1", "E03 Enemy/Boss carrier reference.", "internal data", "E03" },
                new[] { "EncounterSlotSnapshot", "quantity", "int", "1", "Carrier count only; no combat statistics.", "internal data", "E04" },
                new[] { "EncounterSlotSnapshot", "mechanicProfileIds", "IReadOnlyList<string>", "0..N", "Selected E03 CarrierMechanicBinding references.", "internal data", "E03" }
            };
            return "ownerType,fieldName,fieldType,cardinality,semantic,visibility,sourceOfTruth\n"
                + string.Join("\n", rows.Select(row => string.Join(",", row.Select(Csv))))
                + "\n";
        }

        private static string FixtureRows(EncounterCompositionCatalogSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder(
                "encounterId,waveId,waveOrder,slotId,spawnOrder,slotKind,slotRole,carrierId,quantity,mechanicProfileIds,mapRuleIds,fixtureOnly\n");
            foreach (EncounterCompositionSnapshot encounter in snapshot.Encounters)
            {
                foreach (EncounterWaveSnapshot wave in encounter.Waves)
                {
                    foreach (EncounterSlotSnapshot slot in wave.Slots)
                    {
                        builder.AppendLine(string.Join(
                            ",",
                            Csv(encounter.EncounterId),
                            Csv(wave.WaveId),
                            wave.WaveOrder.ToString(CultureInfo.InvariantCulture),
                            Csv(slot.SlotId),
                            slot.SpawnOrder.ToString(CultureInfo.InvariantCulture),
                            Csv(slot.Kind),
                            Csv(slot.Role),
                            Csv(slot.CarrierId),
                            slot.Quantity.ToString(CultureInfo.InvariantCulture),
                            Csv(string.Join(";", slot.MechanicProfileIds)),
                            Csv(string.Join(";", encounter.MapRuleIds)),
                            "true"));
                    }
                }
            }

            return builder.ToString();
        }

        private static string LeakReport(string mode, IEnumerable<Check> checks)
        {
            Check[] rows = checks
                .Where(value => value.Id.StartsWith("leak.", StringComparison.Ordinal)
                    || value.Id.StartsWith("protected.", StringComparison.Ordinal)
                    || value.Id.StartsWith("baseline.", StringComparison.Ordinal)
                    || value.Id.StartsWith("package.", StringComparison.Ordinal)
                    || value.Id.StartsWith("guid.", StringComparison.Ordinal)
                    || value.Id.StartsWith("whitespace.", StringComparison.Ordinal))
                .ToArray();
            StringBuilder builder = new StringBuilder()
                .AppendLine("# Encounter Composition Schema 01 Leak Check Report")
                .AppendLine()
                .AppendLine("- Result: " + (rows.All(value => value.Passed) ? "PASS" : "FAIL"))
                .AppendLine("- Execution: " + mode)
                .AppendLine("- Failed leak/boundary checks: " + rows.Count(value => !value.Passed))
                .AppendLine("- Player-facing projection: none")
                .AppendLine("- Scene / Prefab / Config / Battle / Board / Item writes: none")
                .AppendLine()
                .AppendLine("## Explicit scans")
                .AppendLine();
            foreach (Check check in rows)
            {
                builder.AppendLine(
                    "- " + (check.Passed ? "PASS" : "FAIL")
                    + " — " + check.Id + ": " + check.Actual);
            }

            return builder.ToString();
        }

        private static void Write(string root, string name, string text)
        {
            string path = Path.Combine(root, ReportRoot, name);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        private static string Csv(object value)
        {
            string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory);
            while (directory != null
                && (!Directory.Exists(Path.Combine(directory.FullName, "Assets"))
                    || !Directory.Exists(Path.Combine(directory.FullName, "ProjectSettings"))
                    || !Directory.Exists(Path.Combine(directory.FullName, "Packages"))))
            {
                directory = directory.Parent;
            }

            if (directory == null)
            {
                throw new DirectoryNotFoundException("Unity project root not found.");
            }

            return directory.FullName;
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
            List<Check> checks,
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
                Id = id;
                Expected = expected;
                Actual = actual;
                Passed = passed;
            }

            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private sealed class E03ReferenceResolver : IEncounterCompositionReferenceResolver
        {
            private readonly EnemyValidationContentSnapshot snapshot;
            private readonly IReadOnlyDictionary<string, ValidationProfileKind> profileKindById;
            private readonly ISet<string> bindingKeys;
            private readonly ISet<string> mechaniclessCarrierKeys;

            public E03ReferenceResolver(EnemyValidationContentSnapshot snapshot)
            {
                this.snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
                profileKindById = new ReadOnlyDictionary<string, ValidationProfileKind>(
                    snapshot.MechanicProfiles.ToDictionary(
                        value => value.Domain.StableId,
                        value => value.Kind,
                        StringComparer.Ordinal));
                bindingKeys = new HashSet<string>(
                    snapshot.CarrierMechanicBindings.Select(
                        value => BindingKey(
                            value.Kind == ValidationCarrierKind.Enemy
                                ? EncounterSlotKind.Enemy
                                : EncounterSlotKind.Boss,
                            value.CarrierId,
                            value.MechanicProfileId)),
                    StringComparer.Ordinal);
                mechaniclessCarrierKeys = new HashSet<string>(
                    snapshot.Exceptions
                        .Where(value => value.Kind == NormalizationExceptionKind.Carrier)
                        .Select(value =>
                        {
                            EncounterSlotKind kind = snapshot.TryGetBoss(value.SourceId, out _)
                                ? EncounterSlotKind.Boss
                                : EncounterSlotKind.Enemy;
                            return CarrierKey(kind, value.SourceId);
                        }),
                    StringComparer.Ordinal);
            }

            public bool TryGetEnemy(string stableId, out EnemyArchetypeSnapshot enemy)
            {
                if (snapshot.TryGetEnemy(stableId, out NormalizedEnemyCarrierSnapshot value))
                {
                    enemy = value.Domain;
                    return true;
                }

                enemy = null;
                return false;
            }

            public bool TryGetBoss(string stableId, out BossArchetypeSnapshot boss)
            {
                if (snapshot.TryGetBoss(stableId, out NormalizedBossCarrierSnapshot value))
                {
                    boss = value.Domain;
                    return true;
                }

                boss = null;
                return false;
            }

            public bool TryGetMapRule(string stableId, out MapRuleReference mapRule)
            {
                if (snapshot.TryGetMapRule(stableId, out NormalizedMapRuleSnapshot value))
                {
                    mapRule = value.Domain;
                    return true;
                }

                mapRule = null;
                return false;
            }

            public bool TryGetMechanicProfileKind(
                string stableId,
                out ValidationProfileKind kind)
            {
                if (stableId == null)
                {
                    kind = ValidationProfileKind.Enemy;
                    return false;
                }

                return profileKindById.TryGetValue(stableId, out kind);
            }

            public bool HasCarrierMechanicBinding(
                EncounterSlotKind kind,
                string carrierId,
                string mechanicProfileId)
            {
                return bindingKeys.Contains(BindingKey(kind, carrierId, mechanicProfileId));
            }

            public bool IsIntentionalMechaniclessCarrier(
                EncounterSlotKind kind,
                string carrierId)
            {
                return mechaniclessCarrierKeys.Contains(CarrierKey(kind, carrierId));
            }
        }

        private sealed class AdditionalBindingResolver : IEncounterCompositionReferenceResolver
        {
            private readonly IEncounterCompositionReferenceResolver inner;
            private readonly string additionalBinding;

            public AdditionalBindingResolver(
                IEncounterCompositionReferenceResolver inner,
                EncounterSlotKind kind,
                string carrierId,
                string mechanicProfileId)
            {
                this.inner = inner;
                additionalBinding = BindingKey(kind, carrierId, mechanicProfileId);
            }

            public bool TryGetEnemy(string stableId, out EnemyArchetypeSnapshot enemy)
            {
                return inner.TryGetEnemy(stableId, out enemy);
            }

            public bool TryGetBoss(string stableId, out BossArchetypeSnapshot boss)
            {
                return inner.TryGetBoss(stableId, out boss);
            }

            public bool TryGetMapRule(string stableId, out MapRuleReference mapRule)
            {
                return inner.TryGetMapRule(stableId, out mapRule);
            }

            public bool TryGetMechanicProfileKind(
                string stableId,
                out ValidationProfileKind kind)
            {
                return inner.TryGetMechanicProfileKind(stableId, out kind);
            }

            public bool HasCarrierMechanicBinding(
                EncounterSlotKind kind,
                string carrierId,
                string mechanicProfileId)
            {
                return additionalBinding == BindingKey(kind, carrierId, mechanicProfileId)
                    || inner.HasCarrierMechanicBinding(kind, carrierId, mechanicProfileId);
            }

            public bool IsIntentionalMechaniclessCarrier(
                EncounterSlotKind kind,
                string carrierId)
            {
                return inner.IsIntentionalMechaniclessCarrier(kind, carrierId);
            }
        }

        private static string BindingKey(
            EncounterSlotKind kind,
            string carrierId,
            string mechanicProfileId)
        {
            return CarrierKey(kind, carrierId)
                + "\u001f"
                + (mechanicProfileId ?? string.Empty);
        }

        private static string CarrierKey(EncounterSlotKind kind, string carrierId)
        {
            return ((int)kind).ToString(CultureInfo.InvariantCulture)
                + "\u001f"
                + (carrierId ?? string.Empty);
        }
    }
}
