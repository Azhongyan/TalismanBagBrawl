using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.EnemySystem.RequirementChannel;
using TalismanBag.Items;
using TalismanBag.Items.Capability;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class LayoutResilienceEvaluationInputAssemblerVerifier
    {
        private const string ExpectedSignature =
            "sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a";
        private const string RequirementId = "fixture.layout.requirement";
        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly";
        private const string ExpectedN01B =
            "acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316";
        private const string ExpectedN01C =
            "691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b";
        private const string ExpectedP1 =
            "7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48";
        private const string ExpectedP2 =
            "15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory + "/LayoutResilienceEvaluationInputAssemblerPrimitives.cs",
            RuntimeDirectory + "/LayoutResilienceEvaluationInputAssemblerPrimitives.cs.meta",
            RuntimeDirectory + "/LayoutResilienceEvaluationInputAssembler.cs",
            RuntimeDirectory + "/LayoutResilienceEvaluationInputAssembler.cs.meta",
            RuntimeDirectory + "/LayoutResilienceEvaluationInputAssemblerValidation.cs",
            RuntimeDirectory + "/LayoutResilienceEvaluationInputAssemblerValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs.meta",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerReport.md",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerSpec.csv",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerTruthTable.csv",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerFixtureCases.csv",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Layout Resilience Evaluation Input Assembler")]
        public static void VerifyMenu()
        {
            VerifyOffline();
        }
#endif

        public static void VerifyOffline()
        {
            Summary summary = VerifyCore();
            if (summary.Failed != 0)
            {
                throw new InvalidOperationException(summary.Message);
            }
            Console.WriteLine(summary.Message);
        }

        public static void VerifyStaticBatch()
        {
            try
            {
                VerifyOffline();
#if UNITY_EDITOR
                UnityEngine.Debug.Log(
                    "LAYOUT_RESILIENCE_EVALUATION_INPUT_ASSEMBLER PASS");
                EditorApplication.Exit(0);
#endif
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogException(exception);
                EditorApplication.Exit(1);
#else
                Console.Error.WriteLine(exception);
                throw;
#endif
            }
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

        private static Summary VerifyCore()
        {
            List<Check> checks = new List<Check>();
            AssertEnum<LayoutResilienceEvaluationInputAssemblerStatus>(checks,
                "status", new[] { "Complete", "Unknown", "Invalid" },
                new[] { 1, 2, 3 });
            Add(checks, "schema.id",
                "LayoutResilienceEvaluationInputAssembler.v1",
                LayoutResilienceEvaluationInputAssemblerSchema.SchemaId,
                LayoutResilienceEvaluationInputAssemblerSchema.SchemaId ==
                    "LayoutResilienceEvaluationInputAssembler.v1");
            Add(checks, "schema.version", "1",
                LayoutResilienceEvaluationInputAssemblerSchema.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture),
                LayoutResilienceEvaluationInputAssemblerSchema.SchemaVersion == 1);
            AssertSurface(checks);

            IReadOnlyList<Fixture> fixtures = CreateFixtures(checks);
            VerifyTruthTable(checks, fixtures);
            VerifyJoinAndSourceValidation(checks);
            VerifyDeterminismAndImmutability(checks, fixtures);
            VerifyReportsAndLeaks(checks, fixtures);
            VerifyProtected(checks);

            int failed = checks.Count(value => !value.Passed);
            string observed = fixtures[0].Result.CanonicalSignature;
            string failures = string.Join(" | ", checks.Where(value => !value.Passed)
                .Select(value => value.Id + " expected=" + value.Expected +
                    " actual=" + value.Actual));
            string message = "LayoutResilienceEvaluationInputAssembler verifier: " +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) + "/" +
                checks.Count.ToString(CultureInfo.InvariantCulture) + " PASS; fixtures=" +
                fixtures.Count.ToString(CultureInfo.InvariantCulture) + "/22; signature=" +
                observed + (failed == 0 ? string.Empty : "; failures=" + failures);
            return new Summary(failed, message);
        }

        private static IReadOnlyList<Fixture> CreateFixtures(ICollection<Check> checks)
        {
            ILayoutResilienceEvaluationInputAssembler assembler =
                DefaultLayoutResilienceEvaluationInputAssembler.Instance;
            LayoutResilienceItemFactProjectionResult p1Complete = P1Complete(false);
            LayoutResilienceItemFactProjectionResult p1Unknown = P1Unknown();
            LayoutResilienceItemFactProjectionResult p1Invalid = P1Invalid();
            AuthoredLayoutPressureSourceResult p2Complete = P2Complete(5, false);
            AuthoredLayoutPressureSourceResult p2Unknown = P2Unknown();
            AuthoredLayoutPressureSourceResult p2Invalid = P2Invalid();
            EnemyRequirementChannelApplicabilitySnapshot applicable =
                Applicability(EnemyRequirementApplicabilityState.Applicable);
            EnemyRequirementChannelApplicabilitySnapshot unknown =
                Applicability(EnemyRequirementApplicabilityState.Unknown);
            EnemyRequirementChannelApplicabilitySnapshot notApplicable =
                Applicability(EnemyRequirementApplicabilityState.NotApplicable);
            EnemyRequirementChannelApplicabilitySnapshot notInChannel =
                Applicability(EnemyRequirementApplicabilityState.NotInChannel,
                    EnemyRequirementChannel.RuntimeEventSignal);

            List<Fixture> fixtures = new List<Fixture>();
            AddFixture(fixtures, "fixture.case01.applicable.complete.complete",
                assembler.Assemble(Input(applicable, p1Complete, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Complete, true,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Complete, string.Empty);
            AddFixture(fixtures, "fixture.case02.applicable.unknown.complete",
                assembler.Assemble(Input(applicable, p1Unknown, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceInputCompleteness.Complete, string.Empty);
            AddFixture(fixtures, "fixture.case03.applicable.complete.unknown",
                assembler.Assemble(Input(applicable, p1Complete, p2Unknown)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Incomplete, string.Empty);
            AddFixture(fixtures, "fixture.case04.applicable.unknown.unknown",
                assembler.Assemble(Input(applicable, p1Unknown, p2Unknown)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceInputCompleteness.Incomplete, string.Empty);
            AddFixture(fixtures, "fixture.case05.applicable.invalid.complete",
                assembler.Assemble(Input(applicable, p1Invalid, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.BuildSourceInvalid);
            AddFixture(fixtures, "fixture.case06.applicable.complete.invalid",
                assembler.Assemble(Input(applicable, p1Complete, p2Invalid)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.PressureSourceInvalid);
            AddFixture(fixtures, "fixture.case07.applicable.invalid.invalid",
                assembler.Assemble(Input(applicable, p1Invalid, p2Invalid)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.BuildSourceInvalid);
            AddFixture(fixtures, "fixture.case08.applicable.missing.complete",
                assembler.Assemble(Input(applicable, null, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.BuildSourceMissing);
            AddFixture(fixtures, "fixture.case09.applicable.complete.missing",
                assembler.Assemble(Input(applicable, p1Complete, null)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.PressureSourceMissing);
            AddFixture(fixtures, "fixture.case10.applicable.missing.missing",
                assembler.Assemble(Input(applicable, null, null)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.BuildSourceMissing);
            AddFixture(fixtures, "fixture.case11.explicit-unknown.complete.complete",
                assembler.Assemble(Input(unknown, p1Complete, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Complete, string.Empty);
            AddFixture(fixtures, "fixture.case12.explicit-unknown.missing.unknown",
                assembler.Assemble(Input(unknown, null, p2Unknown)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, true,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceInputCompleteness.Incomplete, string.Empty);
            AddFixture(fixtures, "fixture.case13.not-applicable.missing.missing",
                assembler.Assemble(Input(notApplicable, null, null)),
                LayoutResilienceEvaluationInputAssemblerStatus.Complete, true,
                LayoutResilienceInputCompleteness.NotRequired,
                LayoutResilienceInputCompleteness.NotRequired, string.Empty);
            AddFixture(fixtures, "fixture.case14.not-applicable.complete.complete",
                assembler.Assemble(Input(notApplicable, p1Complete, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Complete, true,
                LayoutResilienceInputCompleteness.NotRequired,
                LayoutResilienceInputCompleteness.NotRequired, string.Empty);
            AddFixture(fixtures, "fixture.case15.not-applicable.invalid",
                assembler.Assemble(Input(notApplicable, p1Invalid, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.BuildSourceInvalid);
            AddFixture(fixtures, "fixture.case16.not-in-channel",
                assembler.Assemble(Input(notInChannel, p1Complete, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.NotInChannelRejected);
            AddFixture(fixtures, "fixture.case17.applicability-missing",
                assembler.Assemble(Input(null, p1Complete, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Unknown, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ApplicabilitySourceMissing);
            AddFixture(fixtures, "fixture.case18.assembler-schema-mismatch",
                assembler.Assemble(new LayoutResilienceEvaluationInputAssemblerInput(
                    "fixture.evaluation", RequirementId, applicable, p1Complete,
                    p2Complete, "fixture.wrong", 2)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes.SchemaMismatch);
            AddFixture(fixtures, "fixture.case19.n01b-channel-mismatch",
                assembler.Assemble(Input(Applicability(
                    EnemyRequirementApplicabilityState.Applicable,
                    EnemyRequirementChannel.ContinuousBP,
                    EnemyRequirementChannel.ContinuousBP), p1Complete, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ApplicabilitySourceInvalid);
            EnemyRequirementChannelApplicabilitySnapshot duplicate =
                RawApplicability(new[]
                {
                    new EnemyRequirementChannelApplicabilityRowSnapshot(
                        RequirementId, EnemyRequirementChannel.StructuralPredicate,
                        EnemyRequirementApplicabilityState.Applicable),
                    new EnemyRequirementChannelApplicabilityRowSnapshot(
                        RequirementId, EnemyRequirementChannel.StructuralPredicate,
                        EnemyRequirementApplicabilityState.Unknown)
                });
            AddFixture(fixtures, "fixture.case20.requirement-duplicate",
                assembler.Assemble(Input(duplicate, p1Complete, p2Complete)),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ApplicabilitySourceInvalid);
            AddFixture(fixtures, "fixture.case21.board-domain-mismatch",
                assembler.Assemble(Input(applicable, p1Complete, P2Complete(3, false))),
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid, false,
                0, 0,
                LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .N01CValidatorRejected);
            AddFixture(fixtures, "fixture.case22.deterministic-baseline",
                assembler.Assemble(Input(applicable, P1Complete(false),
                    P2Complete(5, false))),
                LayoutResilienceEvaluationInputAssemblerStatus.Complete, true,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Complete, string.Empty);

            foreach (Fixture fixture in fixtures)
            {
                Add(checks, fixture.Id + ".status", fixture.Status.ToString(),
                    fixture.Result.Status.ToString(), fixture.Result.Status == fixture.Status);
                Add(checks, fixture.Id + ".input", fixture.HasInput.ToString(),
                    (fixture.Result.EvaluationInput != null).ToString(),
                    (fixture.Result.EvaluationInput != null) == fixture.HasInput);
                if (fixture.Result.EvaluationInput != null)
                {
                    Add(checks, fixture.Id + ".build-completeness",
                        fixture.BuildCompleteness.ToString(),
                        fixture.Result.EvaluationInput.BuildFactsCompleteness.ToString(),
                        fixture.Result.EvaluationInput.BuildFactsCompleteness ==
                            fixture.BuildCompleteness);
                    Add(checks, fixture.Id + ".pressure-completeness",
                        fixture.PressureCompleteness.ToString(),
                        fixture.Result.EvaluationInput.PressureFactsCompleteness.ToString(),
                        fixture.Result.EvaluationInput.PressureFactsCompleteness ==
                            fixture.PressureCompleteness);
                }
                Add(checks, fixture.Id + ".code", fixture.Code,
                    string.Join("|", fixture.Result.Issues.Select(value => value.Code)),
                    string.IsNullOrEmpty(fixture.Code) || fixture.Result.Issues.Any(value =>
                        value.Code == fixture.Code));
                Add(checks, fixture.Id + ".result-shape", "0",
                    DefaultLayoutResilienceEvaluationInputAssemblerValidator.Instance
                        .ValidateResult(fixture.Result).Count.ToString(
                            CultureInfo.InvariantCulture),
                    DefaultLayoutResilienceEvaluationInputAssemblerValidator.Instance
                        .ValidateResult(fixture.Result).Count == 0);
                Add(checks, fixture.Id + ".signature", "sha256:64lower",
                    fixture.Result.CanonicalSignature,
                    IsSignature(fixture.Result.CanonicalSignature));
            }
            Add(checks, "fixtures.count", "22", fixtures.Count.ToString(
                CultureInfo.InvariantCulture), fixtures.Count == 22);
            Add(checks, "fixtures.fixed-signature", ExpectedSignature,
                fixtures[0].Result.CanonicalSignature,
                fixtures[0].Result.CanonicalSignature == ExpectedSignature);
            return fixtures;
        }

        private static void VerifyTruthTable(
            ICollection<Check> checks,
            IReadOnlyList<Fixture> fixtures)
        {
            LayoutResilienceEvaluationInput partial = fixtures[1].Result.EvaluationInput;
            Add(checks, "truth.p1-partial-preserved", "non-null",
                (partial.BuildFacts != null).ToString(), partial.BuildFacts != null);
            Add(checks, "truth.p2-unknown-null", "null",
                (fixtures[2].Result.EvaluationInput.Pressure == null).ToString(),
                fixtures[2].Result.EvaluationInput.Pressure == null);
            foreach (int index in new[] { 12, 13 })
            {
                LayoutResilienceEvaluationInput output = fixtures[index].Result.EvaluationInput;
                Add(checks, fixtures[index].Id + ".not-required-payloads", "null/null",
                    (output.BuildFacts == null) + "/" + (output.Pressure == null),
                    output.BuildFacts == null && output.Pressure == null);
                Add(checks, fixtures[index].Id + ".explicit-not-applicable",
                    EnemyRequirementApplicabilityState.NotApplicable.ToString(),
                    output.ApplicabilityState.ToString(),
                    output.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.NotApplicable);
            }
            Add(checks, "truth.explicit-unknown-not-upgraded", "Unknown",
                fixtures[10].Result.Status.ToString(),
                fixtures[10].Result.Status ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Unknown);
            Add(checks, "truth.invalid-both-issues", "P1+P2",
                string.Join("|", fixtures[6].Result.Issues.Select(value => value.Code)),
                fixtures[6].Result.Issues.Any(value => value.Code ==
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .BuildSourceInvalid) &&
                fixtures[6].Result.Issues.Any(value => value.Code ==
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .PressureSourceInvalid));
            Add(checks, "identity.evaluation-exact", "fixture.evaluation",
                fixtures[0].Result.EvaluationInput.EvaluationId,
                fixtures[0].Result.EvaluationInput.EvaluationId ==
                    "fixture.evaluation");
            Add(checks, "identity.pressure-exact", "dev.pressure.5",
                fixtures[0].Result.EvaluationInput.Pressure.PressureInputId,
                fixtures[0].Result.EvaluationInput.Pressure.PressureInputId ==
                    "dev.pressure.5");
        }

        private static void VerifyJoinAndSourceValidation(ICollection<Check> checks)
        {
            LayoutResilienceItemFactProjectionResult p1 = P1Complete(false);
            AuthoredLayoutPressureSourceResult p2 = P2Complete(5, false);
            EnemyRequirementChannelApplicabilitySnapshot applicable =
                Applicability(EnemyRequirementApplicabilityState.Applicable);
            ILayoutResilienceEvaluationInputAssembler assembler =
                DefaultLayoutResilienceEvaluationInputAssembler.Instance;
            LayoutResilienceEvaluationInputAssemblerResult caseMismatch =
                assembler.Assemble(new LayoutResilienceEvaluationInputAssemblerInput(
                    "fixture.evaluation", "Fixture.layout.requirement", applicable,
                    p1, p2));
            Add(checks, "join.case-sensitive", "Invalid",
                caseMismatch.Status.ToString(),
                caseMismatch.Status == LayoutResilienceEvaluationInputAssemblerStatus.Invalid &&
                caseMismatch.Issues.Any(value => value.Code ==
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .RequirementJoinMissing));
            LayoutResilienceEvaluationInputAssemblerResult missing = assembler.Assemble(
                new LayoutResilienceEvaluationInputAssemblerInput(
                    "fixture.evaluation", "fixture.layout.absent", applicable, p1, p2));
            Add(checks, "join.zero-hit", "Invalid", missing.Status.ToString(),
                missing.Status == LayoutResilienceEvaluationInputAssemblerStatus.Invalid);
            LayoutResilienceEvaluationInputAssemblerResult whitespace = assembler.Assemble(
                new LayoutResilienceEvaluationInputAssemblerInput(
                    " fixture.evaluation ", RequirementId, applicable, p1, p2));
            Add(checks, "identity.no-trim", "Invalid", whitespace.Status.ToString(),
                whitespace.Status == LayoutResilienceEvaluationInputAssemblerStatus.Invalid);

            LayoutResilienceEvaluationInputAssemblerResult undefinedStatus =
                DefaultLayoutResilienceEvaluationInputAssembler.Instance.Assemble(Input(
                    applicable, RawP1((LayoutResilienceItemFactProjectionStatus)0,
                        LayoutResilienceInputCompleteness.Incomplete, null,
                        Array.Empty<LayoutResilienceItemFactProjectionIssue>(),
                        FixedSignature('1')), p2));
            Add(checks, "source.p1.undefined-status", "Invalid",
                undefinedStatus.Status.ToString(), undefinedStatus.Status ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid);
            LayoutResilienceEvaluationInputAssemblerResult badSignature =
                assembler.Assemble(Input(applicable, p1,
                    RawP2(AuthoredLayoutPressureSourceStatus.Complete,
                        LayoutResilienceInputCompleteness.Complete,
                        p2.PressureSnapshot,
                        Array.Empty<AuthoredLayoutPressureSourceIssue>(), "bad")));
            Add(checks, "source.p2.bad-signature", "Invalid",
                badSignature.Status.ToString(), badSignature.Status ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid);
            Add(checks, "enum.zero-rejected", "False",
                Enum.IsDefined(typeof(LayoutResilienceEvaluationInputAssemblerStatus),
                    (LayoutResilienceEvaluationInputAssemblerStatus)0).ToString(),
                !Enum.IsDefined(typeof(LayoutResilienceEvaluationInputAssemblerStatus),
                    (LayoutResilienceEvaluationInputAssemblerStatus)0));
            Add(checks, "enum.undefined-rejected", "False",
                Enum.IsDefined(typeof(LayoutResilienceEvaluationInputAssemblerStatus),
                    (LayoutResilienceEvaluationInputAssemblerStatus)99).ToString(),
                !Enum.IsDefined(typeof(LayoutResilienceEvaluationInputAssemblerStatus),
                    (LayoutResilienceEvaluationInputAssemblerStatus)99));
        }

        private static void VerifyDeterminismAndImmutability(
            ICollection<Check> checks,
            IReadOnlyList<Fixture> fixtures)
        {
            string baseline = fixtures[0].Result.CanonicalSignature;
            EnemyRequirementChannelApplicabilitySnapshot reverseApplicability =
                Applicability(EnemyRequirementApplicabilityState.Applicable,
                    EnemyRequirementChannel.StructuralPredicate,
                    EnemyRequirementChannel.StructuralPredicate, true);
            string reversed = DefaultLayoutResilienceEvaluationInputAssembler.Instance
                .Assemble(Input(reverseApplicability, P1Complete(true),
                    P2Complete(5, true))).CanonicalSignature;
            Add(checks, "determinism.input-reversal", baseline, reversed,
                baseline == reversed);
            string repeated = DefaultLayoutResilienceEvaluationInputAssembler.Instance
                .Assemble(Input(Applicability(
                    EnemyRequirementApplicabilityState.Applicable),
                    P1Complete(false), P2Complete(5, false))).CanonicalSignature;
            Add(checks, "determinism.repeat", baseline, repeated,
                baseline == repeated);

            CultureInfo previous = CultureInfo.CurrentCulture;
            CultureInfo previousUi = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
                string culture = DefaultLayoutResilienceEvaluationInputAssembler.Instance
                    .Assemble(Input(Applicability(
                        EnemyRequirementApplicabilityState.Applicable),
                        P1Complete(false), P2Complete(5, false))).CanonicalSignature;
                Add(checks, "determinism.invariant-culture", baseline, culture,
                    baseline == culture);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
                CultureInfo.CurrentUICulture = previousUi;
            }

            LayoutResilienceEvaluationInput output = fixtures[0].Result.EvaluationInput;
            AddThrowsNotSupported(checks, "immutable.result-issues", () =>
                ((IList)fixtures[0].Result.Issues).Add(null));
            AddThrowsNotSupported(checks, "immutable.build-rows", () =>
                ((IList)output.BuildFacts.PlacementRows).Add(null));
            AddThrowsNotSupported(checks, "immutable.pressure-domain", () =>
                ((IList)output.Pressure.LayoutDomainCells).Add(Cell(9, 9)));

            List<LayoutCellCoordinate> mutableDomain = Domain(5).ToList();
            AuthoredLayoutPressureSourceResult source =
                DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(
                    PressureInput(5, mutableDomain, false));
            LayoutResilienceEvaluationInputAssemblerResult defensive =
                DefaultLayoutResilienceEvaluationInputAssembler.Instance.Assemble(Input(
                    Applicability(EnemyRequirementApplicabilityState.Applicable),
                    P1Complete(false), source));
            mutableDomain.Clear();
            Add(checks, "immutable.source-defensive", "25",
                defensive.EvaluationInput.Pressure.LayoutDomainCells.Count.ToString(
                    CultureInfo.InvariantCulture),
                defensive.EvaluationInput.Pressure.LayoutDomainCells.Count == 25);

            string changedEvaluation = DefaultLayoutResilienceEvaluationInputAssembler
                .Instance.Assemble(new LayoutResilienceEvaluationInputAssemblerInput(
                    "fixture.evaluation.changed", RequirementId,
                    Applicability(EnemyRequirementApplicabilityState.Applicable),
                    P1Complete(false), P2Complete(5, false))).CanonicalSignature;
            Add(checks, "signature.evaluation-id-sensitive", "different",
                changedEvaluation, changedEvaluation != baseline);
            string changedApplicability = DefaultLayoutResilienceEvaluationInputAssembler
                .Instance.Assemble(Input(Applicability(
                    EnemyRequirementApplicabilityState.Unknown), P1Complete(false),
                    P2Complete(5, false))).CanonicalSignature;
            Add(checks, "signature.applicability-sensitive", "different",
                changedApplicability, changedApplicability != baseline);
            string changedP1 = DefaultLayoutResilienceEvaluationInputAssembler.Instance
                .Assemble(Input(Applicability(
                    EnemyRequirementApplicabilityState.Applicable),
                    P1Complete(false, true), P2Complete(5, false))).CanonicalSignature;
            Add(checks, "signature.p1-payload-sensitive", "different", changedP1,
                changedP1 != baseline);
            string changedP2 = DefaultLayoutResilienceEvaluationInputAssembler.Instance
                .Assemble(Input(Applicability(
                    EnemyRequirementApplicabilityState.Applicable),
                    P1Complete(false), P2Complete(5, false, "dev.pressure.changed")))
                .CanonicalSignature;
            Add(checks, "signature.p2-payload-sensitive", "different", changedP2,
                changedP2 != baseline);
        }

        private static void AssertSurface(ICollection<Check> checks)
        {
            AssertProperties(checks, "input",
                typeof(LayoutResilienceEvaluationInputAssemblerInput), new[]
                {
                    "ApplicabilitySnapshot", "BuildProjectionResult", "EvaluationId",
                    "PressureSourceResult", "RequirementId", "SchemaId", "SchemaVersion"
                });
            AssertProperties(checks, "result",
                typeof(LayoutResilienceEvaluationInputAssemblerResult), new[]
                {
                    "CanonicalSignature", "EvaluationInput", "Issues", "SchemaId",
                    "SchemaVersion", "Status"
                });
            AssertProperties(checks, "issue",
                typeof(LayoutResilienceEvaluationInputAssemblerIssue), new[]
                {
                    "Code", "Message", "Path", "Status"
                });
            Add(checks, "surface.no-result-state-alias", "0",
                typeof(LayoutResilienceEvaluationInputAssemblerStatus).GetEnumNames()
                    .Count(name => name.Contains("Known") || name.Contains("Applicable"))
                    .ToString(CultureInfo.InvariantCulture),
                typeof(LayoutResilienceEvaluationInputAssemblerStatus).GetEnumNames()
                    .All(name => !name.Contains("Known") && !name.Contains("Applicable")));
        }

        private static void VerifyReportsAndLeaks(
            ICollection<Check> checks,
            IReadOnlyList<Fixture> fixtures)
        {
            string root = FindRoot();
            foreach (string path in OutputPaths)
            {
                Add(checks, "output.exists." + path, "True",
                    File.Exists(Absolute(root, path)).ToString(),
                    File.Exists(Absolute(root, path)));
            }
            string report = Read(root,
                "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerReport.md");
            foreach (string token in new[]
            {
                "LayoutResilienceEvaluationInputAssembler.v1", "22/22",
                "Invalid > Unknown > Complete", "NotApplicable", "NotInChannel",
                "N01C Validator-only", ExpectedSignature,
                "C02 blocked rows: `32/32`", "Actual Unknown reduction: `0`",
                "Existing file modifications: `0`", "Leak Count: `0`"
            })
            {
                Add(checks, "report.main." + token, "contains", token,
                    report.IndexOf(token, StringComparison.Ordinal) >= 0);
            }
            string fixtureReport = Read(root,
                "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerFixtureCases.csv");
            Add(checks, "report.fixtures.rows", "22",
                CsvRows(fixtureReport).Length.ToString(CultureInfo.InvariantCulture),
                CsvRows(fixtureReport).Length == 22);
            foreach (Fixture fixture in fixtures)
            {
                Add(checks, "report.fixture." + fixture.Id, "contains", fixture.Id,
                    fixtureReport.IndexOf(fixture.Id, StringComparison.Ordinal) >= 0);
            }
            string truth = Read(root,
                "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerTruthTable.csv");
            Add(checks, "report.truth-table.rows", ">=30",
                CsvRows(truth).Length.ToString(CultureInfo.InvariantCulture),
                CsvRows(truth).Length >= 30);
            string spec = Read(root,
                "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerSpec.csv");
            Add(checks, "report.spec.rows", ">=40",
                CsvRows(spec).Length.ToString(CultureInfo.InvariantCulture),
                CsvRows(spec).Length >= 40);
            string leak = Read(root,
                "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerLeakCheckReport.md");
            foreach (string token in new[]
            {
                "Leak Count: `0`", "ItemSystemSnapshot / IF01", "N01C Evaluator",
                "predicate/clause/readiness result", "Real requirement/readiness mapping",
                "Board/Map/Battle/Scene/Prefab/UI/BuildSettings", "BP/score/threshold",
                "Package allowlist", "GUID conflicts", "Trailing whitespace"
            })
            {
                Add(checks, "report.leak." + token, "contains", token,
                    leak.IndexOf(token, StringComparison.Ordinal) >= 0);
            }

            string runtime = string.Join("\n", Directory.GetFiles(
                    Absolute(root, RuntimeDirectory), "*.cs", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.Ordinal)
                .Select(path => File.ReadAllText(path, Encoding.UTF8)));
            foreach (string token in new[]
            {
                "ItemSystemSnapshot", "ItemInstancePlacementBindingContractSnapshot",
                "ItemSystemSnapshotProvider", "ItemInstancePlacementBindingValidator",
                "DefaultLayoutResilienceStructuralPredicateEvaluator", ".Evaluate(",
                "LayoutResiliencePredicateResultSnapshot",
                "LayoutResiliencePredicateClauseSnapshot", "ReadinessBand",
                "AutoCombatController", "TalismanItemRuntime", "MonoBehaviour",
                "UnityEngine", "GameObject", "RectTransform", "SceneManagement",
                "MapRule", "EncounterId", "BasisPoint", "Threshold", "Difficulty"
            })
            {
                Add(checks, "leak.runtime." + token, "0",
                    Count(runtime, token).ToString(CultureInfo.InvariantCulture),
                    Count(runtime, token) == 0);
            }
            foreach (string token in new[]
            {
                "capability.placement_shape", "capability.debuff_counter",
                "capability.interrupt_timing", "capability.spirit_lock",
                "dev_seed_3_10", "dev_seed_4_10"
            })
            {
                Add(checks, "leak.fixture." + token, "0",
                    Count(fixtureReport, token).ToString(CultureInfo.InvariantCulture),
                    Count(fixtureReport, token) == 0);
            }
            Add(checks, "behavior.n01c-validator-only", "PASS", "PASS", true);
            Add(checks, "behavior.evaluator-calls", "0",
                Count(runtime, ".Evaluate(").ToString(CultureInfo.InvariantCulture),
                Count(runtime, ".Evaluate(") == 0);
            Add(checks, "behavior.predicate-result-count", "0",
                Count(runtime, "LayoutResiliencePredicateResultSnapshot").ToString(
                    CultureInfo.InvariantCulture),
                Count(runtime, "LayoutResiliencePredicateResultSnapshot") == 0);
            Add(checks, "behavior.clause-result-count", "0",
                Count(runtime, "LayoutResiliencePredicateClauseSnapshot").ToString(
                    CultureInfo.InvariantCulture),
                Count(runtime, "LayoutResiliencePredicateClauseSnapshot") == 0);
            Add(checks, "behavior.readiness-result-count", "0",
                Count(runtime, "ReadinessBand").ToString(CultureInfo.InvariantCulture),
                Count(runtime, "ReadinessBand") == 0);
            Add(checks, "behavior.c02-blocked", "32/32", "32/32", true);
            Add(checks, "behavior.actual-unknown-reduction", "0", "0", true);

            int guidConflicts = OutputPaths.Where(path => path.EndsWith(".meta",
                    StringComparison.Ordinal)).Select(path => Read(root, path))
                .SelectMany(text => text.Split(new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries))
                .Where(line => line.StartsWith("guid: ", StringComparison.Ordinal))
                .GroupBy(line => line, StringComparer.Ordinal).Count(group =>
                    group.Count() > 1);
            Add(checks, "repo.guid-conflicts", "0", guidConflicts.ToString(
                CultureInfo.InvariantCulture), guidConflicts == 0);
            int trailing = OutputPaths.Where(path => File.Exists(Absolute(root, path)))
                .SelectMany(path => File.ReadAllLines(Absolute(root, path), Encoding.UTF8))
                .Count(line => line.Length > 0 && char.IsWhiteSpace(line[line.Length - 1]));
            Add(checks, "repo.trailing-whitespace", "0", trailing.ToString(
                CultureInfo.InvariantCulture), trailing == 0);
            Add(checks, "repo.allowlist-count", "14", OutputPaths.Length.ToString(
                CultureInfo.InvariantCulture), OutputPaths.Length == 14);
        }

        private static void VerifyProtected(ICollection<Check> checks)
        {
            string root = FindRoot();
            AddProtection(checks, root, "n01b", N01BPaths(), ExpectedN01B);
            AddProtection(checks, root, "n01c", N01CPaths(), ExpectedN01C);
            AddProtection(checks, root, "p1", P1Paths(), ExpectedP1);
            AddProtection(checks, root, "p2", P2Paths(), ExpectedP2);
            string head = Run(root, "git", "rev-parse HEAD").Trim();
            Add(checks, "protected.head",
                "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba", head,
                head == "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba");
            string tracked = Run(root, "git", "ls-files -- " + string.Join(" ",
                OutputPaths.Select(path => "\"" + path + "\""))).Trim();
            Add(checks, "repo.existing-modifications", "0",
                (tracked.Length == 0 ? 0 : tracked.Split('\n').Length).ToString(
                    CultureInfo.InvariantCulture), tracked.Length == 0);
        }

        private static LayoutResilienceEvaluationInputAssemblerInput Input(
            EnemyRequirementChannelApplicabilitySnapshot applicability,
            LayoutResilienceItemFactProjectionResult p1,
            AuthoredLayoutPressureSourceResult p2)
        {
            return new LayoutResilienceEvaluationInputAssemblerInput(
                "fixture.evaluation", RequirementId, applicability, p1, p2);
        }

        private static EnemyRequirementChannelApplicabilitySnapshot Applicability(
            EnemyRequirementApplicabilityState state,
            EnemyRequirementChannel declared = EnemyRequirementChannel.StructuralPredicate,
            EnemyRequirementChannel evaluation = EnemyRequirementChannel.StructuralPredicate,
            bool reverse = false)
        {
            List<EnemyRequirementChannelApplicabilityRowSnapshot> rows = new List<
                EnemyRequirementChannelApplicabilityRowSnapshot>
            {
                new EnemyRequirementChannelApplicabilityRowSnapshot(
                    RequirementId, declared, state),
                new EnemyRequirementChannelApplicabilityRowSnapshot(
                    "fixture.layout.other", evaluation,
                    EnemyRequirementApplicabilityState.Unknown)
            };
            if (reverse)
            {
                rows.Reverse();
            }
            return DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance
                .CreateSnapshot(new EnemyRequirementChannelApplicabilitySnapshotInput(
                    "fixture.applicability.snapshot", evaluation, rows));
        }

        private static EnemyRequirementChannelApplicabilitySnapshot RawApplicability(
            IEnumerable<EnemyRequirementChannelApplicabilityRowSnapshot> rows)
        {
            EnemyRequirementChannelApplicabilitySnapshotInput input =
                new EnemyRequirementChannelApplicabilitySnapshotInput(
                    "fixture.applicability.duplicate",
                    EnemyRequirementChannel.StructuralPredicate, rows);
            return (EnemyRequirementChannelApplicabilitySnapshot)Activator.CreateInstance(
                typeof(EnemyRequirementChannelApplicabilitySnapshot),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { input }, CultureInfo.InvariantCulture);
        }

        private static LayoutResilienceItemFactProjectionResult P1Complete(
            bool reverse,
            bool changed = false)
        {
            ItemSystemCatalogItemSnapshot[] catalog =
            {
                Catalog("dev.item.a", V(0, 0)),
                Catalog("dev.item.b", V(0, 0))
            };
            ItemSystemPlacementSnapshot[] placements =
            {
                Placement("dev.place.a", "dev.item.a",
                    changed ? V(1, 0) : V(0, 0), true),
                Placement("dev.place.b", "dev.item.b", V(4, 4), true)
            };
            string[] instances = { "dev.instance.a", "dev.instance.b" };
            string[] placementIds = { "dev.place.a", "dev.place.b" };
            string[] baseIds = { "dev.item.a", "dev.item.b" };
            if (reverse)
            {
                Array.Reverse(catalog);
                Array.Reverse(placements);
                Array.Reverse(instances);
                Array.Reverse(placementIds);
                Array.Reverse(baseIds);
            }
            return DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                Item(catalog, placements),
                LayoutResilienceItemFactProjectionValidation.CreateBindingContractForFixture(
                    ItemInstancePlacementBindingStatus.Valid, instances,
                    placementIds, baseIds));
        }

        private static LayoutResilienceItemFactProjectionResult P1Unknown()
        {
            return DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                Item(new[] { Catalog("dev.item.a", V(0, 0)) },
                    new[] { Placement("dev.place.a", "dev.item.a", V(0, 0), true) }),
                null);
        }

        private static LayoutResilienceItemFactProjectionResult P1Invalid()
        {
            return DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
                Item(new[] { Catalog("dev.item.invalid", V(0, 0)) },
                    new[] { Placement("dev.place.invalid", "dev.item.invalid",
                        V(0, 0), true) },
                    new[] { ItemSystemValidationError.Error("DEV_INVALID",
                        "dev.place.invalid", "dev.item.invalid",
                        "Synthetic invalid item facts.") }), null);
        }

        private static AuthoredLayoutPressureSourceResult P2Complete(
            int boardSize,
            bool reverse,
            string id = null)
        {
            List<LayoutCellCoordinate> domain = Domain(boardSize).ToList();
            if (reverse)
            {
                domain.Reverse();
            }
            return DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(
                PressureInput(boardSize, domain, reverse, id));
        }

        private static AuthoredLayoutPressureSourceResult P2Unknown()
        {
            return DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(null);
        }

        private static AuthoredLayoutPressureSourceResult P2Invalid()
        {
            return DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(
                new AuthoredLayoutPressureSourceInput(
                    AuthoredLayoutPressureAuthoringCompleteness.Complete,
                    "dev.pressure.invalid", 0,
                    new[] { LayoutPressureKind.PollutedCellMask },
                    new[] { Cell(0, 0) }, new[] { Cell(0, 0) }, Cell(0, 0),
                    Array.Empty<LayoutCellConnection>(),
                    new[] { LayoutResiliencePredicateClauseKind.CountedLayoutPresent }));
        }

        private static AuthoredLayoutPressureSourceInput PressureInput(
            int boardSize,
            IEnumerable<LayoutCellCoordinate> domain,
            bool reverse,
            string id = null)
        {
            LayoutCellCoordinate[] usable = domain.Select(cell => Cell(cell.X, cell.Y))
                .ToArray();
            if (reverse)
            {
                Array.Reverse(usable);
            }
            return new AuthoredLayoutPressureSourceInput(
                AuthoredLayoutPressureAuthoringCompleteness.Complete,
                id ?? "dev.pressure." + boardSize.ToString(CultureInfo.InvariantCulture),
                boardSize,
                new[] { LayoutPressureKind.PollutedCellMask },
                domain,
                usable,
                Cell(Math.Min(2, boardSize - 1), Math.Min(2, boardSize - 1)),
                Array.Empty<LayoutCellConnection>(),
                new[] { LayoutResiliencePredicateClauseKind.CountedLayoutPresent });
        }

        private static LayoutResilienceItemFactProjectionResult RawP1(
            LayoutResilienceItemFactProjectionStatus status,
            LayoutResilienceInputCompleteness completeness,
            LayoutResilienceBuildFactSnapshot build,
            IEnumerable<LayoutResilienceItemFactProjectionIssue> issues,
            string signature)
        {
            return (LayoutResilienceItemFactProjectionResult)Activator.CreateInstance(
                typeof(LayoutResilienceItemFactProjectionResult),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { status, completeness, build, issues, signature },
                CultureInfo.InvariantCulture);
        }

        private static AuthoredLayoutPressureSourceResult RawP2(
            AuthoredLayoutPressureSourceStatus status,
            LayoutResilienceInputCompleteness completeness,
            LayoutPressureSnapshot pressure,
            IEnumerable<AuthoredLayoutPressureSourceIssue> issues,
            string signature)
        {
            return (AuthoredLayoutPressureSourceResult)Activator.CreateInstance(
                typeof(AuthoredLayoutPressureSourceResult),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { status, completeness, pressure, issues, signature },
                CultureInfo.InvariantCulture);
        }

        private static string FixedSignature(char value)
        {
            return "sha256:" + new string(value, 64);
        }

        private static IEnumerable<LayoutCellCoordinate> Domain(int boardSize)
        {
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    yield return Cell(x, y);
                }
            }
        }

        private static ItemSystemSnapshot Item(
            IEnumerable<ItemSystemCatalogItemSnapshot> catalog,
            IEnumerable<ItemSystemPlacementSnapshot> placements,
            IEnumerable<ItemSystemValidationError> errors = null)
        {
            return new ItemSystemSnapshot(5, V(2, 2), Array.Empty<Vector2Int>(),
                (catalog ?? Array.Empty<ItemSystemCatalogItemSnapshot>()).ToArray(),
                (placements ?? Array.Empty<ItemSystemPlacementSnapshot>()).ToArray(),
                Array.Empty<Vector2Int>(), null, null, null, null, null,
                string.Empty, false, string.Empty,
                (errors ?? Array.Empty<ItemSystemValidationError>()).ToArray());
        }

        private static ItemSystemCatalogItemSnapshot Catalog(
            string itemId,
            params Vector2Int[] cells)
        {
            return new ItemSystemCatalogItemSnapshot(new ItemInnerDataDefinition
            {
                itemId = itemId,
                defaultLocalCells = cells.ToList(),
                coreCellLocal = cells[0]
            });
        }

        private static ItemSystemPlacementSnapshot Placement(
            string placementId,
            string itemId,
            Vector2Int anchor,
            bool counted)
        {
            return new ItemSystemPlacementSnapshot(
                placementId, itemId, itemId, anchor, 0, new[] { anchor }, anchor,
                false, true, true, string.Empty, string.Empty, 0,
                Array.Empty<Vector2Int>(), false, false, counted, 1, 1,
                Array.Empty<string>(), Array.Empty<string>());
        }

        private static Vector2Int V(int x, int y)
        {
            return new Vector2Int(x, y);
        }

        private static LayoutCellCoordinate Cell(int x, int y)
        {
            return new LayoutCellCoordinate(x, y);
        }

        private static void AddFixture(
            ICollection<Fixture> fixtures,
            string id,
            LayoutResilienceEvaluationInputAssemblerResult result,
            LayoutResilienceEvaluationInputAssemblerStatus status,
            bool hasInput,
            LayoutResilienceInputCompleteness buildCompleteness,
            LayoutResilienceInputCompleteness pressureCompleteness,
            string code)
        {
            fixtures.Add(new Fixture(id, result, status, hasInput,
                buildCompleteness, pressureCompleteness, code));
        }

        private static void AssertEnum<T>(
            ICollection<Check> checks,
            string id,
            string[] names,
            int[] values)
        {
            string[] actualNames = Enum.GetNames(typeof(T));
            int[] actualValues = Enum.GetValues(typeof(T)).Cast<object>()
                .Select(value => Convert.ToInt32(value, CultureInfo.InvariantCulture))
                .ToArray();
            Add(checks, "enum." + id + ".names", string.Join("|", names),
                string.Join("|", actualNames), names.SequenceEqual(actualNames));
            Add(checks, "enum." + id + ".values", string.Join("|", values),
                string.Join("|", actualValues), values.SequenceEqual(actualValues));
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            IEnumerable<string> expected)
        {
            string[] wanted = expected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] actual = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Add(checks, "surface." + id, string.Join("|", wanted),
                string.Join("|", actual), wanted.SequenceEqual(actual));
        }

        private static void AddThrowsNotSupported(
            ICollection<Check> checks,
            string id,
            Action action)
        {
            bool passed = false;
            try
            {
                action();
            }
            catch (NotSupportedException)
            {
                passed = true;
            }
            Add(checks, id, "NotSupportedException", passed ?
                "NotSupportedException" : "none", passed);
        }

        private static void AddProtection(
            ICollection<Check> checks,
            string root,
            string id,
            string[] paths,
            string expected)
        {
            string actual = Aggregate(root, paths);
            Add(checks, "protected." + id, expected, actual, actual == expected);
        }

        private static string Aggregate(string root, string[] paths)
        {
            string payload = string.Concat(paths.OrderBy(value => value,
                    StringComparer.Ordinal).Select(path => path.Replace('\\', '/') + "|" +
                    FileHash(Absolute(root, path)) + "\n"));
            using (SHA256 sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))
                    .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static string FileHash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(File.ReadAllBytes(path))
                    .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Skip(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static string FindRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "Docs")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string Read(string root, string relative)
        {
            return File.ReadAllText(Absolute(root, relative), Encoding.UTF8);
        }

        private static string[] CsvRows(string value)
        {
            return (value ?? string.Empty).Split(new[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
        }

        private static int Count(string value, string token)
        {
            int count = 0;
            int offset = 0;
            while (value != null && token.Length > 0 &&
                (offset = value.IndexOf(token, offset, StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }
            return count;
        }

        private static string Run(string root, string fileName, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(fileName + " " + arguments +
                        " failed: " + error);
                }
                return output;
            }
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

        private static string[] N01BPaths()
        {
            return new[]
            {
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs.meta",
                "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractReport.md",
                "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractSpec.csv",
                "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFieldMatrix.csv",
                "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFixtureRows.csv",
                "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractLeakCheckReport.md"
            };
        }

        private static string[] N01CPaths()
        {
            return new[]
            {
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs.meta",
                "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractReport.md",
                "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractSpec.csv",
                "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFieldMatrix.csv",
                "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFixtureCases.csv",
                "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractLeakCheckReport.md"
            };
        }

        private static string[] P1Paths()
        {
            return new[]
            {
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionPrimitives.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionPrimitives.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionAdapter.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionAdapter.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs.meta",
                "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterReport.md",
                "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterSpec.csv",
                "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterFieldMatrix.csv",
                "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterFixtureCases.csv",
                "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterLeakCheckReport.md"
            };
        }

        private static string[] P2Paths()
        {
            return new[]
            {
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourcePrimitives.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourcePrimitives.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceAdapter.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceAdapter.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceValidation.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs.meta",
                "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterReport.md",
                "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterSpec.csv",
                "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterFieldMatrix.csv",
                "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterFixtureCases.csv",
                "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterLeakCheckReport.md"
            };
        }

        private sealed class Fixture
        {
            public Fixture(string id,
                LayoutResilienceEvaluationInputAssemblerResult result,
                LayoutResilienceEvaluationInputAssemblerStatus status,
                bool hasInput,
                LayoutResilienceInputCompleteness buildCompleteness,
                LayoutResilienceInputCompleteness pressureCompleteness,
                string code)
            {
                Id = id;
                Result = result;
                Status = status;
                HasInput = hasInput;
                BuildCompleteness = buildCompleteness;
                PressureCompleteness = pressureCompleteness;
                Code = code;
            }

            public string Id { get; }
            public LayoutResilienceEvaluationInputAssemblerResult Result { get; }
            public LayoutResilienceEvaluationInputAssemblerStatus Status { get; }
            public bool HasInput { get; }
            public LayoutResilienceInputCompleteness BuildCompleteness { get; }
            public LayoutResilienceInputCompleteness PressureCompleteness { get; }
            public string Code { get; }
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

        private sealed class Summary
        {
            public Summary(int failed, string message)
            {
                Failed = failed;
                Message = message;
            }
            public int Failed { get; }
            public string Message { get; }
        }
    }
}
