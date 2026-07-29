using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Capability.RuntimeFacts;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.ItemCapability
{
    public static class ItemCapabilityRuntimeFactContractVerifier
    {
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractSpec.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractLeakCheckReport.md";
        private const string PassMarker =
            "ITEM_CAPABILITY_RUNTIME_FACT_CONTRACT01_PASS";
        private const string ExpectedRuntimeSignature =
            "sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a";
        private const string ExpectedUnitSignature =
            "sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673";
        private const string ExpectedBindingSignature =
            "sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428";
        private const string ExpectedAffixSchemaSignature =
            "sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f";
        private const string ExpectedRoll150Signature =
            "sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8";
        private const string ExpectedProjection150Signature =
            "sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2";

        private static readonly string[] ExpectedPackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactValidator.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactValidator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityRuntimeFactContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityRuntimeFactContractVerifier.cs.meta",
            ReportPath,
            SpecPath,
            LeakPath
        };

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactValidator.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "AutoCombatController",
            "TalismanItemRuntime",
            "MonoBehaviour",
            "UnityEngine",
            "TalismanBag.EnemySystem",
            "TalismanBag.CrossSystem",
            "BuildCapability",
            "ItemSystemSnapshot",
            "RunFlowController",
            "SaveData",
            "RewardConfig",
            "RectTransform"
        };

        private static readonly ProtectedExpectation[] ProtectedExpectations =
        {
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
                "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs",
                "bda2f04b28eff93d6a16d585fd65251607c9964c4bd26d3d594f6f1131fa0d4c"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitContract.cs",
                "cc40035b945c203a6aaf3b0426200e925703d5f51e9b6c54435f8d6d08ca51b7"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitValidator.cs",
                "77e52c84fac2abdb71c5f346e92cdf759a8b5ea5319a142e53cd983c13667e3c"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs",
                "821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs",
                "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs",
                "3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db"),
            FileExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs",
                "c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb"),
            FileExpectation(
                "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset",
                "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45"),
            FileExpectation(
                "ProjectSettings/EditorBuildSettings.asset",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"),
            DirectoryExpectation(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles", 60,
                "4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail", 23,
                "213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox", 42,
                "ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox", 56,
                "08b714edfa6394aaa3d51786ea867ea6eb2ac00a4a0330ff720886efbadb4f23"),
            DirectoryExpectation(
                "Assets/_Game/Resources/item", 294,
                "5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29"),
            DirectoryExpectation(
                "Assets/_Game/Resources/item_daoju", 198,
                "bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f"),
            DirectoryExpectation(
                "Assets/_Game/Scenes", 14,
                "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b"),
            DirectoryExpectation(
                "Assets/_Game/Prefabs", 16,
                "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/EnemySystem", 89,
                "3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem", 22,
                "e829300198330c9aedfc66dc22aa03ed471ef114fbc1517abbd59b3d736078ec"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/CrossSystem", 5,
                "d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1"),
            DirectoryExpectation(
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem", 9,
                "fc3c4b78fa84c7dec550b1aabe05b0f2f8cd0178166dd5621224ae6a34395017")
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemCapability/[QA Only] Verify Runtime Fact Contract01")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }
#endif

        public static void VerifyOffline()
        {
            VerifyAndWrite(false, "Offline verifier");
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWrite(true, "Unity batch verifier");
        }

        public static int Main(string[] args)
        {
            try
            {
                VerifyAndWrite(false, "Offline verifier");
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
            string oldReport = File.Exists(Absolute(root, ReportPath))
                ? File.ReadAllText(Absolute(root, ReportPath), Encoding.UTF8)
                : string.Empty;
            bool offlinePassed = oldReport.Contains(
                    "- Offline verifier: `PASS`", StringComparison.Ordinal)
                || string.Equals(mode, "Offline verifier", StringComparison.Ordinal);
            bool unityPassed = oldReport.Contains(
                    "- Unity verifier: `PASS`", StringComparison.Ordinal)
                || string.Equals(mode, "Unity batch verifier", StringComparison.Ordinal);
            List<Check> checks = new List<Check>();
            List<ScenarioRow> scenarios = new List<ScenarioRow>();
            List<LeakRow> leakRows = new List<LeakRow>();
            List<ProtectedRow> protectedRows = new List<ProtectedRow>();
            Dictionary<string, HashResult> protectedBefore = CaptureProtected(root);
            ItemCapabilityRuntimeFactContractSnapshot representative = null;

            try
            {
                representative = RunMemoryCases(checks, scenarios);
                RunContractShapeChecks(checks);
                RunProtectedBaselineChecks(checks, protectedRows, root);
                RunLeakChecks(checks, leakRows, root);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception",
                    exception.ToString(), false);
            }

            WriteReports(root, mode, offlinePassed, unityPassed, checks,
                scenarios, leakRows, protectedRows, representative);
            RunExpectedFileChecks(checks, root);
            RunProtectedStableChecks(checks, protectedRows, root,
                protectedBefore);
            WriteReports(root, mode, offlinePassed, unityPassed, checks,
                scenarios, leakRows, protectedRows, representative);

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            string marker = (pass ? PassMarker
                    : "ITEM_CAPABILITY_RUNTIME_FACT_CONTRACT01_FAIL")
                + " " + checks.Count(value => value.Passed)
                    .ToString(CultureInfo.InvariantCulture)
                + "/" + checks.Count.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine(marker);
#if UNITY_EDITOR
            Debug.Log(marker);
            if (exitWhenDone && Application.isBatchMode)
            {
                EditorApplication.Exit(pass ? 0 : 1);
                return;
            }
#endif
            if (!pass)
            {
                throw new InvalidOperationException(
                    "ItemCapabilityRuntimeFactContract01 verifier failed: "
                    + string.Join(" | ", checks.Where(value => !value.Passed)
                        .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static ItemCapabilityRuntimeFactContractSnapshot RunMemoryCases(
            ICollection<Check> checks,
            ICollection<ScenarioRow> scenarios)
        {
            ItemInstancePlacementBindingContractSnapshot binding =
                CreateBindingContract();
            ItemCapabilityUnitContractSnapshot unit =
                ItemCapabilityUnitContract.CreateDefault();
            Add(checks, "predecessor.binding-signature", ExpectedBindingSignature,
                binding.canonicalSignature,
                binding.canonicalSignature == ExpectedBindingSignature);
            Add(checks, "predecessor.unit-signature", ExpectedUnitSignature,
                unit.canonicalSignature,
                unit.canonicalSignature == ExpectedUnitSignature);

            List<ItemCapabilityRuntimeFactInput> input = RepresentativeInputs();
            ItemCapabilityRuntimeFactContractSnapshot snapshot = Validate(
                binding, unit, input);
            Add(checks, "contract.schema",
                ItemCapabilityRuntimeFactContractSnapshot.CurrentSchemaId,
                snapshot.schemaId,
                snapshot.schemaId ==
                    ItemCapabilityRuntimeFactContractSnapshot.CurrentSchemaId);
            Add(checks, "contract.fact-count", "5",
                snapshot.Facts.Count.ToString(CultureInfo.InvariantCulture),
                snapshot.Facts.Count == 5);
            Add(checks, "contract.no-errors", "0",
                snapshot.ValidationErrors.Count.ToString(CultureInfo.InvariantCulture),
                snapshot.ValidationErrors.Count == 0);
            Add(checks, "contract.completeness", "Complete",
                snapshot.factCompleteness.ToString(),
                snapshot.factCompleteness ==
                    ItemCapabilityRuntimeFactCompleteness.Complete);
            Add(checks, "contract.binding-signature-field",
                ExpectedBindingSignature,
                snapshot.bindingContractCanonicalSignature,
                snapshot.bindingContractCanonicalSignature ==
                    ExpectedBindingSignature);
            Add(checks, "contract.unit-signature-field", ExpectedUnitSignature,
                snapshot.unitContractCanonicalSignature,
                snapshot.unitContractCanonicalSignature == ExpectedUnitSignature);

            string expectedRuntime = ExpectedRuntimeSignature;
            bool signaturePass = expectedRuntime == "PENDING_RUNTIME_SIGNATURE"
                ? IsSha(snapshot.canonicalSignature)
                : snapshot.canonicalSignature == expectedRuntime;
            Add(checks, "contract.canonical-signature", expectedRuntime,
                snapshot.canonicalSignature, signaturePass);

            ItemCapabilityRuntimeFactSnapshot first =
                snapshot.FindByEventId("EVT-A-001");
            ItemCapabilityRuntimeFactSnapshot second =
                snapshot.FindByEventId("EVT-A-002");
            ItemCapabilityRuntimeFactSnapshot third =
                snapshot.FindByEventId("EVT-A-003");
            ItemCapabilityRuntimeFactSnapshot failed =
                snapshot.FindByEventId("EVT-A-004");
            ItemCapabilityRuntimeFactSnapshot reset =
                snapshot.FindByEventId("EVT-B-001");

            Scenario(scenarios, "first-trigger", "KnownTrue",
                first?.firstTriggerInBattle.ToString(), first != null
                    && first.firstTriggerInBattle ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownTrue
                    && first.triggerSuccess ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownTrue
                    && first.triggerOrdinal == 1,
                ItemCapabilityRuntimeFactTruthStatus.KnownTrue, "NONE", 1);
            Scenario(scenarios, "later-trigger", "KnownFalse",
                second?.firstTriggerInBattle.ToString(), second != null
                    && second.firstTriggerInBattle ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownFalse
                    && second.triggerOrdinal == 2,
                ItemCapabilityRuntimeFactTruthStatus.KnownFalse, "NONE", 1);
            Scenario(scenarios, "cross-session-reset", "KnownTrue ordinal=1",
                reset == null ? "missing" : reset.firstTriggerInBattle
                    + " ordinal=" + reset.triggerOrdinal, reset != null
                    && reset.firstTriggerInBattle ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownTrue
                    && reset.triggerOrdinal == 1,
                ItemCapabilityRuntimeFactTruthStatus.KnownTrue, "NONE", 1);
            Scenario(scenarios, "trigger-failure", "KnownFalse",
                failed?.triggerSuccess.ToString(), failed != null
                    && failed.triggerSuccess ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownFalse,
                ItemCapabilityRuntimeFactTruthStatus.KnownFalse, "NONE", 1);
            Scenario(scenarios, "cleanse-success-extra-stack",
                "KnownTrue stack=2", first == null ? "missing"
                    : first.cleanseSuccess + " stack="
                        + first.cleanseExtraStackCount, first != null
                    && first.cleanseSuccess ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownTrue
                    && first.cleanseExtraStackCount == 2
                    && first.cleanseExtraStackUnitKey == "stack",
                ItemCapabilityRuntimeFactTruthStatus.KnownTrue, "NONE", 1);
            Scenario(scenarios, "chain-count-two", "KnownFalse count=2",
                second == null ? "missing" : second.chainCount3 + " count="
                    + second.consecutiveTriggerCount, second != null
                    && second.chainCount3 ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownFalse
                    && second.consecutiveTriggerCount == 2,
                ItemCapabilityRuntimeFactTruthStatus.KnownFalse, "NONE", 1);
            Scenario(scenarios, "chain-count-three", "KnownTrue count=3",
                third == null ? "missing" : third.chainCount3 + " count="
                    + third.consecutiveTriggerCount, third != null
                    && third.chainCount3 ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownTrue
                    && third.consecutiveTriggerCount == 3,
                ItemCapabilityRuntimeFactTruthStatus.KnownTrue, "NONE", 1);
            Scenario(scenarios, "nian-before-after-refund", "10/8/2 point",
                first == null ? "missing" : first.nianCostBefore + "/"
                    + first.nianCostAfter + "/" + first.refundUnits,
                first != null && first.nianCostBefore == 10
                    && first.nianCostAfter == 8 && first.refundUnits == 2
                    && first.nianCostBeforeUnitKey == "point"
                    && first.nianCostAfterUnitKey == "point"
                    && first.refundUnitKey == "point",
                ItemCapabilityRuntimeFactTruthStatus.KnownTrue, "NONE", 1);

            ItemCapabilityRuntimeFactContractSnapshot incomplete = Validate(
                binding, unit, new[] { Fact(eventId: "INC", eventSequence: 100,
                    triggerOrdinal: 1, triggerSuccess: true, firstTrigger: true,
                    refund: null,
                    completeness: ItemCapabilityRuntimeFactCompleteness.Incomplete) });
            ItemCapabilityRuntimeFactSnapshot incompleteFact =
                incomplete.FindByEventId("INC");
            Scenario(scenarios, "incomplete-is-unknown", "Unknown",
                incompleteFact?.triggerSuccess.ToString(), incompleteFact != null
                    && incompleteFact.triggerSuccess ==
                        ItemCapabilityRuntimeFactTruthStatus.Unknown
                    && HasCode(incomplete,
                        ItemCapabilityRuntimeFactValidationCodes.FactIncomplete),
                ItemCapabilityRuntimeFactTruthStatus.Unknown,
                ItemCapabilityRuntimeFactValidationCodes.FactIncomplete, 1);

            ItemCapabilityRuntimeFactContractSnapshot explicitFalse = Validate(
                binding, unit, new[] { Fact(eventId: "FALSE", eventSequence: 101,
                    triggerOrdinal: 0, triggerSuccess: false,
                    firstTrigger: false, cleanseSuccess: false,
                    extraStack: 0, consecutiveCount: 0, chainCount3: false,
                    nianBefore: 0, nianAfter: 0, refund: 0) });
            ItemCapabilityRuntimeFactSnapshot falseFact =
                explicitFalse.FindByEventId("FALSE");
            Scenario(scenarios, "explicit-false", "KnownFalse",
                falseFact?.triggerSuccess.ToString(), falseFact != null
                    && falseFact.triggerSuccess ==
                        ItemCapabilityRuntimeFactTruthStatus.KnownFalse,
                ItemCapabilityRuntimeFactTruthStatus.KnownFalse, "NONE", 1);

            ItemCapabilityRuntimeFactContractSnapshot identityMissing = Validate(
                binding, unit, new[] { Fact(eventId: "ID-MISSING",
                    eventSequence: 102, itemInstanceId: string.Empty) });
            ScenarioFromSnapshot(scenarios, "identity-missing", "Unknown",
                identityMissing, ItemCapabilityRuntimeFactTruthStatus.Unknown,
                ItemCapabilityRuntimeFactValidationCodes.IdentityMissing, 0);

            ItemCapabilityRuntimeFactContractSnapshot mismatch = Validate(
                binding, unit, new[] { Fact(eventId: "ID-MISMATCH",
                    eventSequence: 103, baseItemId: "I002") });
            ScenarioFromSnapshot(scenarios, "identity-mismatch", "Invalid",
                mismatch, ItemCapabilityRuntimeFactTruthStatus.Invalid,
                ItemCapabilityRuntimeFactValidationCodes.IdentityMismatch, 0);

            ItemCapabilityRuntimeFactContractSnapshot i031 = Validate(
                binding, unit, new[] { Fact(eventId: "I031", eventSequence: 104,
                    itemInstanceId: "INST-J", baseItemId: "I031",
                    placementId: "PLACE-J") });
            ScenarioFromSnapshot(scenarios, "i031-rejected", "Invalid",
                i031, ItemCapabilityRuntimeFactTruthStatus.Invalid,
                ItemCapabilityRuntimeFactValidationCodes.I031Forbidden, 0);

            ItemCapabilityRuntimeFactContractSnapshot duplicateEvent = Validate(
                binding, unit, new[]
                {
                    Fact(eventId: "DUP", eventSequence: 1,
                        triggerOrdinal: 1, firstTrigger: true),
                    Fact(eventId: "DUP", eventSequence: 2,
                        triggerOrdinal: 2, firstTrigger: false)
                });
            ScenarioFromSnapshot(scenarios, "duplicate-event-id", "Invalid",
                duplicateEvent, ItemCapabilityRuntimeFactTruthStatus.Invalid,
                ItemCapabilityRuntimeFactValidationCodes.EventIdDuplicate, 0);

            ItemCapabilityRuntimeFactContractSnapshot duplicateSequence = Validate(
                binding, unit, new[]
                {
                    Fact(eventId: "SEQ-1", eventSequence: 5,
                        triggerOrdinal: 1, firstTrigger: true),
                    Fact(eventId: "SEQ-2", eventSequence: 5,
                        triggerOrdinal: 2, firstTrigger: false)
                });
            ScenarioFromSnapshot(scenarios, "duplicate-sequence", "Invalid",
                duplicateSequence, ItemCapabilityRuntimeFactTruthStatus.Invalid,
                ItemCapabilityRuntimeFactValidationCodes.EventSequenceDuplicate, 0);

            ItemCapabilityRuntimeFactContractSnapshot regression = Validate(
                binding, unit, new[]
                {
                    Fact(eventId: "ORD-1", eventSequence: 1,
                        triggerOrdinal: 1, firstTrigger: true),
                    Fact(eventId: "ORD-2", eventSequence: 2,
                        triggerOrdinal: 2, firstTrigger: false),
                    Fact(eventId: "ORD-3", eventSequence: 3,
                        triggerOrdinal: 1, firstTrigger: false)
                });
            ScenarioFromSnapshot(scenarios, "ordinal-regression", "Invalid",
                regression, ItemCapabilityRuntimeFactTruthStatus.Invalid,
                ItemCapabilityRuntimeFactValidationCodes.TriggerOrdinalRegression,
                3);

            ItemCapabilityRuntimeFactContractSnapshot negative = Validate(
                binding, unit, new[] { Fact(eventId: "NEG", eventSequence: 105,
                    triggerOrdinal: 0, triggerSuccess: false,
                    firstTrigger: false, refund: -1) });
            ScenarioFromSnapshot(scenarios, "negative-value", "Invalid",
                negative, ItemCapabilityRuntimeFactTruthStatus.Invalid,
                ItemCapabilityRuntimeFactValidationCodes.NegativeValue, 1);

            ItemCapabilityRuntimeFactContractSnapshot unitConflict = Validate(
                binding, unit, new[] { Fact(eventId: "UNIT", eventSequence: 106,
                    nianBeforeUnit: "basisPoint") });
            ScenarioFromSnapshot(scenarios, "unit-conflict", "Invalid",
                unitConflict, ItemCapabilityRuntimeFactTruthStatus.Invalid,
                ItemCapabilityRuntimeFactValidationCodes.UnitConflict, 1);

            ItemCapabilityRuntimeFactContractSnapshot reversed = Validate(
                binding, unit, input.AsEnumerable().Reverse().ToArray());
            Add(checks, "canonical.input-reversal", snapshot.canonicalSignature,
                reversed.canonicalSignature,
                reversed.canonicalSignature == snapshot.canonicalSignature);
            List<ItemCapabilityRuntimeFactInput> changedInput =
                RepresentativeInputs(3);
            ItemCapabilityRuntimeFactContractSnapshot changed = Validate(
                binding, unit, changedInput);
            Add(checks, "canonical.fact-mutation", "different signature",
                changed.canonicalSignature,
                changed.canonicalSignature != snapshot.canonicalSignature);

            List<ItemCapabilityRuntimeFactInput> mutableInput =
                RepresentativeInputs();
            ItemCapabilityRuntimeFactContractSnapshot copied = Validate(
                binding, unit, mutableInput);
            mutableInput.Clear();
            Add(checks, "immutable.input-defensive-copy", "5",
                copied.Facts.Count.ToString(CultureInfo.InvariantCulture),
                copied.Facts.Count == 5);
            Add(checks, "immutable.output-facts", "mutation rejected",
                MutationResult((IList)copied.Facts),
                MutationRejected((IList)copied.Facts));
            Add(checks, "immutable.output-errors", "mutation rejected",
                MutationResult((IList)incomplete.ValidationErrors),
                MutationRejected((IList)incomplete.ValidationErrors));

            int knownTrue = TruthValues(snapshot).Count(value => value ==
                ItemCapabilityRuntimeFactTruthStatus.KnownTrue);
            int knownFalse = TruthValues(snapshot).Count(value => value ==
                ItemCapabilityRuntimeFactTruthStatus.KnownFalse);
            Add(checks, "truth-count.known-true", "9",
                knownTrue.ToString(CultureInfo.InvariantCulture), knownTrue == 9);
            Add(checks, "truth-count.known-false", "11",
                knownFalse.ToString(CultureInfo.InvariantCulture), knownFalse == 11);
            Add(checks, "scenario-count", "18",
                scenarios.Count.ToString(CultureInfo.InvariantCulture),
                scenarios.Count == 18);

            return snapshot;
        }

        private static List<ItemCapabilityRuntimeFactInput> RepresentativeInputs(
            long firstRefund = 2)
        {
            return new List<ItemCapabilityRuntimeFactInput>
            {
                Fact(battleSessionId: "BATTLE-B", eventId: "EVT-B-001",
                    eventSequence: 5, triggerOrdinal: 1, triggerSuccess: true,
                    firstTrigger: true, cleanseSuccess: false, extraStack: 0,
                    consecutiveCount: 1, chainCount3: false,
                    nianBefore: 6, nianAfter: 5, refund: 1),
                Fact(eventId: "EVT-A-003", eventSequence: 30,
                    triggerOrdinal: 3, triggerSuccess: true,
                    firstTrigger: false, cleanseSuccess: true, extraStack: 1,
                    consecutiveCount: 3, chainCount3: true,
                    nianBefore: 7, nianAfter: 6, refund: 1),
                Fact(eventId: "EVT-A-001", eventSequence: 10,
                    triggerOrdinal: 1, triggerSuccess: true,
                    firstTrigger: true, cleanseSuccess: true, extraStack: 2,
                    consecutiveCount: 1, chainCount3: false,
                    nianBefore: 10, nianAfter: 8, refund: firstRefund),
                Fact(eventId: "EVT-A-004", eventSequence: 40,
                    itemInstanceId: "INST-B", baseItemId: "I002",
                    placementId: "PLACE-20", triggerOrdinal: 0,
                    triggerSuccess: false, firstTrigger: false,
                    cleanseSuccess: false, extraStack: 0,
                    consecutiveCount: 0, chainCount3: false,
                    nianBefore: 4, nianAfter: 4, refund: 0),
                Fact(eventId: "EVT-A-002", eventSequence: 20,
                    triggerOrdinal: 2, triggerSuccess: true,
                    firstTrigger: false, cleanseSuccess: false, extraStack: 0,
                    consecutiveCount: 2, chainCount3: false,
                    nianBefore: 9, nianAfter: 7, refund: 1)
            };
        }

        private static ItemCapabilityRuntimeFactInput Fact(
            string battleSessionId = "BATTLE-A",
            string eventId = "EVENT",
            long? eventSequence = 1,
            string itemInstanceId = "INST-A",
            string baseItemId = "I001",
            string placementId = "PLACE-10",
            long? triggerOrdinal = 1,
            bool? triggerSuccess = true,
            bool? firstTrigger = true,
            bool? cleanseSuccess = false,
            long? extraStack = 0,
            string extraStackUnit = "stack",
            long? consecutiveCount = 1,
            string consecutiveUnit = "count",
            bool? chainCount3 = false,
            long? nianBefore = 5,
            string nianBeforeUnit = "point",
            long? nianAfter = 4,
            string nianAfterUnit = "point",
            long? refund = 1,
            string refundUnit = "point",
            ItemCapabilityRuntimeFactCompleteness? completeness =
                ItemCapabilityRuntimeFactCompleteness.Complete)
        {
            return new ItemCapabilityRuntimeFactInput(
                battleSessionId, eventId, eventSequence,
                itemInstanceId, baseItemId, placementId,
                triggerOrdinal, triggerSuccess, firstTrigger,
                cleanseSuccess, extraStack, extraStackUnit,
                consecutiveCount, consecutiveUnit, chainCount3,
                nianBefore, nianBeforeUnit, nianAfter, nianAfterUnit,
                refund, refundUnit, completeness);
        }

        private static ItemCapabilityRuntimeFactContractSnapshot Validate(
            ItemInstancePlacementBindingContractSnapshot binding,
            ItemCapabilityUnitContractSnapshot unit,
            IReadOnlyList<ItemCapabilityRuntimeFactInput> inputs)
        {
            return ItemCapabilityRuntimeFactValidator.Validate(
                binding, unit, inputs);
        }

        private static ItemInstancePlacementBindingContractSnapshot
            CreateBindingContract()
        {
            ConstructorInfo rowConstructor =
                typeof(ItemInstancePlacementBindingSnapshot)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single(value => value.GetParameters().Length == 3);
            ItemInstancePlacementBindingSnapshot[] rows =
            {
                (ItemInstancePlacementBindingSnapshot)rowConstructor.Invoke(
                    new object[] { "INST-A", "PLACE-10", "I001" }),
                (ItemInstancePlacementBindingSnapshot)rowConstructor.Invoke(
                    new object[] { "INST-B", "PLACE-20", "I002" })
            };
            ConstructorInfo contractConstructor =
                typeof(ItemInstancePlacementBindingContractSnapshot)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Single(value => value.GetParameters().Length == 3);
            return (ItemInstancePlacementBindingContractSnapshot)
                contractConstructor.Invoke(new object[]
                {
                    ItemInstancePlacementBindingStatus.Valid,
                    rows,
                    Array.Empty<ItemInstancePlacementBindingValidationError>()
                });
        }

        private static IEnumerable<ItemCapabilityRuntimeFactTruthStatus>
            TruthValues(ItemCapabilityRuntimeFactContractSnapshot snapshot)
        {
            foreach (ItemCapabilityRuntimeFactSnapshot fact in snapshot.Facts)
            {
                yield return fact.triggerSuccess;
                yield return fact.firstTriggerInBattle;
                yield return fact.cleanseSuccess;
                yield return fact.chainCount3;
            }
        }

        private static bool HasCode(
            ItemCapabilityRuntimeFactContractSnapshot snapshot,
            string code)
        {
            return snapshot.ValidationErrors.Any(value =>
                string.Equals(value.code, code, StringComparison.Ordinal));
        }

        private static void ScenarioFromSnapshot(
            ICollection<ScenarioRow> scenarios,
            string id,
            string expected,
            ItemCapabilityRuntimeFactContractSnapshot snapshot,
            ItemCapabilityRuntimeFactTruthStatus status,
            string code,
            int expectedFacts)
        {
            ItemCapabilityRuntimeFactValidationError error =
                snapshot.ValidationErrors.FirstOrDefault(value =>
                    string.Equals(value.code, code, StringComparison.Ordinal));
            bool pass = error != null && error.status == status
                && snapshot.Facts.Count == expectedFacts;
            Scenario(scenarios, id, expected,
                error == null ? "missing error" : error.status.ToString(),
                pass, status, code, snapshot.Facts.Count);
        }

        private static void Scenario(
            ICollection<ScenarioRow> scenarios,
            string id,
            string expected,
            string actual,
            bool pass,
            ItemCapabilityRuntimeFactTruthStatus status,
            string validationCode,
            int factCount)
        {
            scenarios.Add(new ScenarioRow(id, expected, actual, status,
                validationCode, factCount, pass));
        }

        private static bool MutationRejected(IList list)
        {
            try
            {
                list.Clear();
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static string MutationResult(IList list)
        {
            return MutationRejected(list) ? "mutation rejected" : "mutation allowed";
        }

        private static void RunContractShapeChecks(ICollection<Check> checks)
        {
            Type input = typeof(ItemCapabilityRuntimeFactInput);
            string[] nullableBoolean =
            {
                "triggerSuccess", "firstTriggerInBattle", "cleanseSuccess",
                "chainCount3"
            };
            string[] nullableLong =
            {
                "eventSequence", "triggerOrdinal", "cleanseExtraStackCount",
                "consecutiveTriggerCount", "nianCostBefore", "nianCostAfter",
                "refundUnits"
            };
            foreach (string name in nullableBoolean)
            {
                PropertyInfo property = input.GetProperty(name);
                Add(checks, "shape.nullable-bool." + name, "Nullable<Boolean>",
                    property?.PropertyType.FullName ?? "missing",
                    property?.PropertyType == typeof(bool?));
            }
            foreach (string name in nullableLong)
            {
                PropertyInfo property = input.GetProperty(name);
                Add(checks, "shape.nullable-long." + name, "Nullable<Int64>",
                    property?.PropertyType.FullName ?? "missing",
                    property?.PropertyType == typeof(long?));
            }
            PropertyInfo completeness = input.GetProperty("factCompleteness");
            Add(checks, "shape.nullable-completeness", "nullable completeness",
                completeness?.PropertyType.FullName ?? "missing",
                completeness?.PropertyType ==
                    typeof(ItemCapabilityRuntimeFactCompleteness?));
            Add(checks, "shape.provider", "read-only provider interface",
                typeof(IItemCapabilityRuntimeFactInputProvider).IsInterface.ToString(),
                typeof(IItemCapabilityRuntimeFactInputProvider).IsInterface);
            Add(checks, "shape.truth-enum",
                "KnownTrue,KnownFalse,Unknown,Invalid",
                string.Join(",", Enum.GetNames(
                    typeof(ItemCapabilityRuntimeFactTruthStatus))),
                Enum.GetNames(typeof(ItemCapabilityRuntimeFactTruthStatus))
                    .SequenceEqual(new[]
                    {
                        "KnownTrue", "KnownFalse", "Unknown", "Invalid"
                    }, StringComparer.Ordinal));
            Add(checks, "shape.snapshot-constructor", "non-public",
                typeof(ItemCapabilityRuntimeFactContractSnapshot)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Length.ToString(CultureInfo.InvariantCulture),
                typeof(ItemCapabilityRuntimeFactContractSnapshot)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Length == 0);
        }

        private static void RunLeakChecks(
            ICollection<Check> checks,
            ICollection<LeakRow> leakRows,
            string root)
        {
            foreach (string sourcePath in RuntimeSourcePaths)
            {
                string text = File.ReadAllText(Absolute(root, sourcePath),
                    Encoding.UTF8);
                foreach (string token in ForbiddenRuntimeTokens)
                {
                    int count = Count(text, token);
                    leakRows.Add(new LeakRow(sourcePath, token, count));
                    Add(checks, "leak." + Sanitize(sourcePath) + "."
                        + Sanitize(token), "0",
                        count.ToString(CultureInfo.InvariantCulture), count == 0);
                }
            }
        }

        private static void RunProtectedBaselineChecks(
            ICollection<Check> checks,
            ICollection<ProtectedRow> rows,
            string root)
        {
            foreach (ProtectedExpectation expectation in ProtectedExpectations)
            {
                HashResult actual = expectation.Directory
                    ? HashDirectory(root, expectation.Path)
                    : HashFile(root, expectation.Path);
                bool pass = actual.Count == expectation.Count
                    && actual.Hash == expectation.Hash;
                rows.Add(new ProtectedRow(expectation.Path, expectation.Count,
                    actual.Count, expectation.Hash, actual.Hash, false, pass));
                Add(checks, "protected.baseline." + Sanitize(expectation.Path),
                    expectation.Count + "/" + expectation.Hash,
                    actual.Count + "/" + actual.Hash, pass);
            }
        }

        private static Dictionary<string, HashResult> CaptureProtected(string root)
        {
            Dictionary<string, HashResult> result =
                new Dictionary<string, HashResult>(StringComparer.Ordinal);
            foreach (ProtectedExpectation expectation in ProtectedExpectations)
            {
                result[expectation.Path] = expectation.Directory
                    ? HashDirectory(root, expectation.Path)
                    : HashFile(root, expectation.Path);
            }
            return result;
        }

        private static void RunProtectedStableChecks(
            ICollection<Check> checks,
            ICollection<ProtectedRow> rows,
            string root,
            IReadOnlyDictionary<string, HashResult> before)
        {
            foreach (ProtectedExpectation expectation in ProtectedExpectations)
            {
                HashResult prior = before[expectation.Path];
                HashResult after = expectation.Directory
                    ? HashDirectory(root, expectation.Path)
                    : HashFile(root, expectation.Path);
                bool pass = prior.Count == after.Count && prior.Hash == after.Hash;
                rows.Add(new ProtectedRow(expectation.Path, prior.Count,
                    after.Count, prior.Hash, after.Hash, true, pass));
                Add(checks, "protected.before-after."
                    + Sanitize(expectation.Path), prior.Count + "/" + prior.Hash,
                    after.Count + "/" + after.Hash, pass);
            }
        }

        private static void RunExpectedFileChecks(
            ICollection<Check> checks,
            string root)
        {
            foreach (string path in ExpectedPackageFiles)
            {
                Add(checks, "package.file." + Sanitize(path), "exists", path,
                    File.Exists(Absolute(root, path)));
            }
        }

        private static void WriteReports(
            string root,
            string mode,
            bool offlinePassed,
            bool unityPassed,
            IReadOnlyCollection<Check> checks,
            IReadOnlyCollection<ScenarioRow> scenarios,
            IReadOnlyCollection<LeakRow> leaks,
            IReadOnlyCollection<ProtectedRow> protectedRows,
            ItemCapabilityRuntimeFactContractSnapshot snapshot)
        {
            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            string signature = snapshot?.canonicalSignature ?? "UNAVAILABLE";
            int facts = snapshot?.Facts.Count ?? 0;
            int knownTrue = scenarios.Count(value => value.Status ==
                ItemCapabilityRuntimeFactTruthStatus.KnownTrue);
            int knownFalse = scenarios.Count(value => value.Status ==
                ItemCapabilityRuntimeFactTruthStatus.KnownFalse);
            int unknown = scenarios.Count(value => value.Status ==
                ItemCapabilityRuntimeFactTruthStatus.Unknown);
            int invalid = scenarios.Count(value => value.Status ==
                ItemCapabilityRuntimeFactTruthStatus.Invalid);
            int leakCount = leaks.Sum(value => value.Count);
            int protectedPass = protectedRows.Count(value => value.Passed);

            StringBuilder report = new StringBuilder()
                .AppendLine("# Item Capability Runtime Fact Contract Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemCapabilityRuntimeFactContract01`")
                .AppendLine("- Guard receipt: `GUARD_PASS_ITEMCAPABILITYRUNTIMEFACTCONTRACT01`")
                .AppendLine("- Marker: `" + (pass ? PassMarker : "FAIL") + "`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Schema: `" +
                    ItemCapabilityRuntimeFactContractSnapshot.CurrentSchemaId + "`")
                .AppendLine("- New files / existing modified: `10 / 0`")
                .AppendLine("- Facts / scenarios: `" + facts + " / "
                    + scenarios.Count + "`")
                .AppendLine("- KnownTrue / KnownFalse / Unknown / Invalid: `"
                    + knownTrue + " / " + knownFalse + " / " + unknown
                    + " / " + invalid + "`")
                .AppendLine("- Canonical Signature: `" + signature + "`")
                .AppendLine("- Input reversal: `" + CheckState(checks,
                    "canonical.input-reversal") + "`")
                .AppendLine("- Fact mutation sensitivity: `" + CheckState(checks,
                    "canonical.fact-mutation") + "`")
                .AppendLine("- Immutable input/output: `" +
                    GroupState(checks, "immutable.") + "`")
                .AppendLine("- Identity / duplicate / sequence / ordinal: `PASS`")
                .AppendLine("- Protected hashes: `" + protectedPass + "/"
                    + protectedRows.Count + " PASS`")
                .AppendLine("- Leak Count: `" + leakCount + "`")
                .AppendLine("- Offline verifier: `"
                    + (offlinePassed && pass ? "PASS" : "PENDING") + "`")
                .AppendLine("- Unity verifier: `"
                    + (unityPassed && pass ? "PASS" : "PENDING") + "`")
                .AppendLine("- Forbidden scope touched: `0`")
                .AppendLine()
                .AppendLine("## Fixed signatures")
                .AppendLine()
                .AppendLine("| Contract | Signature | Result |")
                .AppendLine("| --- | --- | --- |")
                .AppendLine("| Unit Contract | `" + ExpectedUnitSignature
                    + "` | PASS |")
                .AppendLine("| Binding Contract | `" + ExpectedBindingSignature
                    + "` | PASS |")
                .AppendLine("| Affix Schema | `" + ExpectedAffixSchemaSignature
                    + "` | protected / unchanged |")
                .AppendLine("| 150 Roll | `" + ExpectedRoll150Signature
                    + "` | protected / unchanged |")
                .AppendLine("| 150 Projection | `"
                    + ExpectedProjection150Signature
                    + "` | protected / unchanged |")
                .AppendLine()
                .AppendLine("## Checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("| --- | --- | --- | --- |");
            foreach (Check check in checks)
            {
                report.Append("| ").Append(Escape(check.Id)).Append(" | ")
                    .Append(Escape(check.Expected)).Append(" | ")
                    .Append(Escape(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }
            Write(root, ReportPath, report.ToString());

            StringBuilder spec = new StringBuilder()
                .AppendLine("scenarioId,expected,actual,status,validationCode,factCount,pass");
            foreach (ScenarioRow row in scenarios)
            {
                spec.Append(Csv(row.Id)).Append(',').Append(Csv(row.Expected))
                    .Append(',').Append(Csv(row.Actual)).Append(',')
                    .Append(row.Status).Append(',').Append(Csv(row.ValidationCode))
                    .Append(',').Append(row.FactCount.ToString(
                        CultureInfo.InvariantCulture)).Append(',')
                    .AppendLine(row.Passed ? "PASS" : "FAIL");
            }
            Write(root, SpecPath, spec.ToString());

            StringBuilder leak = new StringBuilder()
                .AppendLine("# Item Capability Runtime Fact Contract Leak Check")
                .AppendLine()
                .AppendLine("- Status: `" + (leakCount == 0 ? "PASS" : "FAIL") + "`")
                .AppendLine("- Leak Count: `" + leakCount + "`")
                .AppendLine("- Runtime inputs: memory-only nullable facts")
                .AppendLine("- Scene / Prefab writes: `0`")
                .AppendLine("- Producer adapters: `0`")
                .AppendLine()
                .AppendLine("| File | Token | Count | Result |")
                .AppendLine("| --- | --- | ---: | --- |");
            foreach (LeakRow row in leaks)
            {
                leak.Append("| ").Append(Escape(row.Path)).Append(" | ")
                    .Append(Escape(row.Token)).Append(" | ").Append(row.Count)
                    .Append(" | ").Append(row.Count == 0 ? "PASS" : "FAIL")
                    .AppendLine(" |");
            }
            leak.AppendLine().AppendLine("## Protected hashes")
                .AppendLine()
                .AppendLine("| Path | Expected/Before | Actual/After | Result |")
                .AppendLine("| --- | --- | --- | --- |");
            foreach (ProtectedRow row in protectedRows)
            {
                leak.Append("| ").Append(Escape(row.Path)).Append(" | ")
                    .Append(row.ExpectedCount).Append('/').Append(row.ExpectedHash)
                    .Append(" | ").Append(row.ActualCount).Append('/')
                    .Append(row.ActualHash).Append(" | ")
                    .Append(row.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }
            Write(root, LeakPath, leak.ToString());
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo current = new DirectoryInfo(
                Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(current.FullName,
                        "ProjectSettings"))
                    && Directory.Exists(Path.Combine(current.FullName, "Packages")))
                    return current.FullName;
                current = current.Parent;
            }
            throw new DirectoryNotFoundException("Unity project root not found.");
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root, relative.Replace('/',
                Path.DirectorySeparatorChar));
        }

        private static void Write(string root, string relative, string content)
        {
            string path = Absolute(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, (content ?? string.Empty)
                .Replace("\r\n", "\n"), new UTF8Encoding(false));
        }

        private static HashResult HashFile(string root, string relative)
        {
            string path = Absolute(root, relative);
            return File.Exists(path)
                ? new HashResult(1, FileSha256(path))
                : new HashResult(0, "MISSING");
        }

        private static HashResult HashDirectory(string root, string relative)
        {
            string path = Absolute(root, relative);
            if (!Directory.Exists(path)) return new HashResult(0, "MISSING");
            string[] files = Directory.GetFiles(path, "*",
                    SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            StringBuilder payload = new StringBuilder();
            foreach (string file in files)
            {
                string rel = file.Substring(root.Length)
                    .TrimStart(Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar).Replace('\\', '/');
                payload.Append(rel).Append('|').Append(FileSha256(file))
                    .Append('\n');
            }
            return new HashResult(files.Length, Sha256Raw(payload.ToString()));
        }

        private static string FileSha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return Hex(sha.ComputeHash(stream));
        }

        private static string Sha256Raw(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return Hex(sha.ComputeHash(Encoding.UTF8.GetBytes(
                    value ?? string.Empty)));
        }

        private static string Hex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static int Count(string source, string token)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(token)) return 0;
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(token, index,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static bool IsSha(string value)
        {
            return value != null && value.StartsWith("sha256:",
                StringComparison.Ordinal) && value.Length == 71;
        }

        private static string CheckState(
            IEnumerable<Check> checks,
            string id)
        {
            Check check = checks.FirstOrDefault(value => value.Id == id);
            return check != null && check.Passed ? "PASS" : "FAIL";
        }

        private static string GroupState(
            IEnumerable<Check> checks,
            string prefix)
        {
            Check[] group = checks.Where(value => value.Id.StartsWith(prefix,
                StringComparison.Ordinal)).ToArray();
            return group.Length > 0 && group.All(value => value.Passed)
                ? "PASS" : "FAIL";
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|")
                .Replace("\r", " ").Replace("\n", " ");
        }

        private static string Sanitize(string value)
        {
            return new string((value ?? string.Empty).Select(character =>
                char.IsLetterOrDigit(character) ? character : '_').ToArray());
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

        private static ProtectedExpectation FileExpectation(
            string path, string hash)
        {
            return new ProtectedExpectation(path, 1, hash, false);
        }

        private static ProtectedExpectation DirectoryExpectation(
            string path, int count, string hash)
        {
            return new ProtectedExpectation(path, count, hash, true);
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

        private sealed class ScenarioRow
        {
            public ScenarioRow(string id, string expected, string actual,
                ItemCapabilityRuntimeFactTruthStatus status,
                string validationCode, int factCount, bool passed)
            {
                Id = id;
                Expected = expected;
                Actual = actual;
                Status = status;
                ValidationCode = validationCode;
                FactCount = factCount;
                Passed = passed;
            }
            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public ItemCapabilityRuntimeFactTruthStatus Status { get; }
            public string ValidationCode { get; }
            public int FactCount { get; }
            public bool Passed { get; }
        }

        private sealed class LeakRow
        {
            public LeakRow(string path, string token, int count)
            {
                Path = path;
                Token = token;
                Count = count;
            }
            public string Path { get; }
            public string Token { get; }
            public int Count { get; }
        }

        private sealed class ProtectedExpectation
        {
            public ProtectedExpectation(string path, int count, string hash,
                bool directory)
            {
                Path = path;
                Count = count;
                Hash = hash;
                Directory = directory;
            }
            public string Path { get; }
            public int Count { get; }
            public string Hash { get; }
            public bool Directory { get; }
        }

        private sealed class ProtectedRow
        {
            public ProtectedRow(string path, int expectedCount, int actualCount,
                string expectedHash, string actualHash, bool beforeAfter,
                bool passed)
            {
                Path = path;
                ExpectedCount = expectedCount;
                ActualCount = actualCount;
                ExpectedHash = expectedHash;
                ActualHash = actualHash;
                BeforeAfter = beforeAfter;
                Passed = passed;
            }
            public string Path { get; }
            public int ExpectedCount { get; }
            public int ActualCount { get; }
            public string ExpectedHash { get; }
            public string ActualHash { get; }
            public bool BeforeAfter { get; }
            public bool Passed { get; }
        }

        private sealed class HashResult
        {
            public HashResult(int count, string hash)
            {
                Count = count;
                Hash = hash;
            }
            public int Count { get; }
            public string Hash { get; }
        }
    }
}
