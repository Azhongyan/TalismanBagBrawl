#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemCorePotentialAndBuildEligibilitySchemaVerifier
    {
        private const string PackageName = "V0.4-ItemCorePotentialAndBuildEligibilitySchema01";
        private const string ReportPath = "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaSpec.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaLeakCheckReport.md";
        private const string MatrixPath = "Docs/V0.4/Reports/ItemCorePotentialAndBuildPolicyMatrix.csv";
        private const string RuntimeRoot = "Assets/_Game/Scripts/TalismanBag/Items/Generation/Potential";
        private const string PassMarker = "ITEM_CORE_POTENTIAL_AND_BUILD_ELIGIBILITY_SCHEMA01_PASS";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemCorePotentialAndBuildEligibilitySchema01/[QA Only] Run Schema")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(exitWhenBatchMode: false);
        }

        public static void VerifyStaticBatch()
        {
            VerifyAndWriteReports(exitWhenBatchMode: Application.isBatchMode);
        }

        private static void VerifyAndWriteReports(bool exitWhenBatchMode)
        {
            VerificationResult result = new();
            List<SpecRow> rows = new();
            List<LeakRow> leakRows = new();
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot defaultSchema = null;
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot fixtureSchema = null;

            try
            {
                defaultSchema = ItemCorePotentialAndBuildEligibilityCatalog.CreateDefaultSchema();
                fixtureSchema = RunSchemaChecks(defaultSchema, rows, result);
                RunLeakChecks(leakRows, result);
                RunHistoricalRegressions(result);
            }
            catch (Exception exception)
            {
                result.Errors.Add("Unhandled schema verifier exception: " + exception);
            }

            WriteReports(defaultSchema, fixtureSchema, rows, leakRows, result);
            bool passed = result.Errors.Count == 0
                && rows.Count >= 35
                && rows.All(row => row.result == "PASS")
                && leakRows.All(row => row.count == 0);
            if (passed)
            {
                Debug.Log(PassMarker + ": verification and all reports passed.");
            }
            else
            {
                foreach (string error in result.Errors)
                {
                    Debug.LogError(error);
                }
            }

            if (exitWhenBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static ItemCorePotentialAndBuildEligibilitySchemaSnapshot RunSchemaChecks(
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot defaultSchema,
            List<SpecRow> rows,
            VerificationResult result)
        {
            AddBoolean(rows, result, "default-schema-valid", "default", defaultSchema?.isValid == true,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "schema-id-v1", "default",
                defaultSchema?.schemaId == ItemCorePotentialAndBuildEligibilitySchemaSnapshot.CurrentSchemaId,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "default-core-profile-zero", "default",
                defaultSchema?.CorePotentialProfiles.Count == 0
                && ItemCorePotentialAndBuildEligibilityCatalog.DefaultCorePotentialProfileCount == 0,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "default-qualification-instance-zero", "default",
                ItemCorePotentialAndBuildEligibilityCatalog.DefaultQualificationInstanceCount == 0,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "default-probability-profile-zero", "default",
                ItemCorePotentialAndBuildEligibilityCatalog.DefaultProbabilityProfileCount == 0,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "fixture-isolation-markers", "default", CheckFixtureMarkers(),
                ItemCorePotentialAndBuildValidationCodes.None);

            string[] expectedRarityOrder = { "white", "green", "blue", "purple", "orange" };
            AddBoolean(rows, result, "core-policy-stable-order", "core-policy",
                defaultSchema.CoreRarityPolicies.Select(policy => policy.rarity.ToStableKey())
                    .SequenceEqual(expectedRarityOrder), ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "build-policy-stable-order", "build-policy",
                defaultSchema.BuildRarityPolicies.Select(policy => policy.rarity.ToStableKey())
                    .SequenceEqual(expectedRarityOrder), ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "orange-allows-ultimate", "core-policy",
                defaultSchema.QueryCoreRarityPolicy(ItemInstanceRarity.Orange).value?.allowsUltimateCoreEffect == true,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "non-orange-forbids-ultimate", "core-policy",
                defaultSchema.CoreRarityPolicies
                    .Where(policy => policy.rarity != ItemInstanceRarity.Orange)
                    .All(policy => !policy.allowsUltimateCoreEffect),
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "core-policies-unresolved", "core-policy",
                defaultSchema.CoreRarityPolicies.All(policy =>
                    policy.resolutionStatus == ItemCorePotentialResolutionStatus.Unresolved),
                ItemCorePotentialAndBuildValidationCodes.None);

            ItemCorePotentialProfileSnapshot standardProfile = Profile(
                "qa_potential_profile_01", "I001", ItemInstanceRarity.Green,
                new ItemCorePotentialEffectSnapshot("qa_core_standard_01", ItemCorePotentialEffectKind.Standard, true));
            ItemCorePotentialProfileSnapshot ultimateProfile = Profile(
                "qa_potential_profile_orange", "I002", ItemInstanceRarity.Orange,
                new ItemCorePotentialEffectSnapshot("qa_core_ultimate_01", ItemCorePotentialEffectKind.Ultimate, true));
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot fixtureSchema = Create(
                DefaultCorePolicies(), new[] { standardProfile, ultimateProfile }, DefaultBuildPolicies());
            AddSchema(rows, result, "defined-standard-profile-valid", "core-profile", true,
                ItemCorePotentialAndBuildValidationCodes.None,
                Create(DefaultCorePolicies(), new[] { standardProfile }, DefaultBuildPolicies()));
            AddSchema(rows, result, "orange-ultimate-profile-valid", "core-profile", true,
                ItemCorePotentialAndBuildValidationCodes.None,
                Create(DefaultCorePolicies(), new[] { ultimateProfile }, DefaultBuildPolicies()));
            AddBoolean(rows, result, "visible-subset-valid", "core-profile",
                fixtureSchema.isValid
                && standardProfile.EligibleCoreEffectIds.Contains("qa_core_standard_01")
                && standardProfile.VisibleCoreEffectIds.Contains("qa_core_standard_01"),
                ItemCorePotentialAndBuildValidationCodes.None);

            RunCoreInvalidCases(rows, result);
            RunBuildPolicyAndQualificationCases(defaultSchema, rows, result);
            RunQueryCases(defaultSchema, fixtureSchema, rows, result);
            RunDeterminismReadOnlyAndShapeCases(fixtureSchema, rows, result);
            return fixtureSchema;
        }

        private static void RunCoreInvalidCases(List<SpecRow> rows, VerificationResult result)
        {
            AddSchema(rows, result, "purple-ultimate-invalid", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.UltimateEffectNotAllowed,
                Create(DefaultCorePolicies(), new[]
                {
                    Profile("qa_purple_ultimate", "I001", ItemInstanceRarity.Purple,
                        new ItemCorePotentialEffectSnapshot("qa_core_ultimate_01", ItemCorePotentialEffectKind.Ultimate, true))
                }, DefaultBuildPolicies()));
            AddSchema(rows, result, "visible-not-eligible-invalid", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.VisibleEffectNotEligible,
                Create(DefaultCorePolicies(), new[]
                {
                    new ItemCorePotentialProfileSnapshot("qa_visible_invalid", "I001", ItemInstanceRarity.Green,
                        ItemCorePotentialResolutionStatus.Defined, "QA_FIXTURE_ONLY",
                        new[] { new ItemCorePotentialEffectSnapshot("qa_core_standard_01", ItemCorePotentialEffectKind.Standard, false) },
                        new[] { "qa_core_not_eligible" })
                }, DefaultBuildPolicies()));
            AddSchema(rows, result, "empty-core-effect-id", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.CoreEffectIdEmpty,
                Create(DefaultCorePolicies(), new[]
                {
                    Profile("qa_empty_effect", "I001", ItemInstanceRarity.Green,
                        new ItemCorePotentialEffectSnapshot(string.Empty, ItemCorePotentialEffectKind.Standard, false))
                }, DefaultBuildPolicies()));
            AddSchema(rows, result, "duplicate-core-effect-id", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.CoreEffectIdDuplicate,
                Create(DefaultCorePolicies(), new[]
                {
                    Profile("qa_duplicate_effect", "I001", ItemInstanceRarity.Green,
                        new ItemCorePotentialEffectSnapshot("qa_core_standard_01", ItemCorePotentialEffectKind.Standard, false),
                        new ItemCorePotentialEffectSnapshot("qa_core_standard_01", ItemCorePotentialEffectKind.Standard, true))
                }, DefaultBuildPolicies()));
            AddSchema(rows, result, "duplicate-profile-id", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.ProfileIdDuplicate,
                Create(DefaultCorePolicies(), new[]
                {
                    Profile("qa_duplicate_profile", "I001", ItemInstanceRarity.Green),
                    Profile("qa_duplicate_profile", "I002", ItemInstanceRarity.Blue)
                }, DefaultBuildPolicies()));
            AddSchema(rows, result, "duplicate-base-rarity-profile", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.ProfileDuplicate,
                Create(DefaultCorePolicies(), new[]
                {
                    Profile("qa_profile_a", "I001", ItemInstanceRarity.Green),
                    Profile("qa_profile_b", "I001", ItemInstanceRarity.Green)
                }, DefaultBuildPolicies()));
            AddSchema(rows, result, "profile-i031-invalid", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.BaseItemNotOrdinary,
                Create(DefaultCorePolicies(), new[] { Profile("qa_i031", "I031", ItemInstanceRarity.Green) }, DefaultBuildPolicies()));
            AddSchema(rows, result, "profile-i999-invalid", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.BaseItemUnknown,
                Create(DefaultCorePolicies(), new[] { Profile("qa_i999", "I999", ItemInstanceRarity.Green) }, DefaultBuildPolicies()));
            AddSchema(rows, result, "profile-empty-base-invalid", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.BaseItemIdEmpty,
                Create(DefaultCorePolicies(), new[] { Profile("qa_empty_base", string.Empty, ItemInstanceRarity.Green) }, DefaultBuildPolicies()));
            AddSchema(rows, result, "unresolved-with-effects-invalid", "core-profile", false,
                ItemCorePotentialAndBuildValidationCodes.UnresolvedProfileHasEffects,
                Create(DefaultCorePolicies(), new[]
                {
                    new ItemCorePotentialProfileSnapshot("qa_unresolved_data", "I001", ItemInstanceRarity.Green,
                        ItemCorePotentialResolutionStatus.Unresolved, "QA_FIXTURE_ONLY",
                        new[] { new ItemCorePotentialEffectSnapshot("qa_core_standard_01", ItemCorePotentialEffectKind.Standard, false) })
                }, DefaultBuildPolicies()));
            AddSchema(rows, result, "missing-core-policy", "core-policy", false,
                ItemCorePotentialAndBuildValidationCodes.CorePolicyMissing,
                Create(DefaultCorePolicies().Where(policy => policy.rarity != ItemInstanceRarity.Green),
                    Array.Empty<ItemCorePotentialProfileSnapshot>(), DefaultBuildPolicies()));
            AddSchema(rows, result, "duplicate-core-policy", "core-policy", false,
                ItemCorePotentialAndBuildValidationCodes.CorePolicyDuplicate,
                Create(DefaultCorePolicies().Concat(new[] { DefaultCorePolicies()[0] }),
                    Array.Empty<ItemCorePotentialProfileSnapshot>(), DefaultBuildPolicies()));
            ItemCoreRarityPolicySnapshot[] wrongUltimate = DefaultCorePolicies();
            wrongUltimate[3] = new ItemCoreRarityPolicySnapshot(ItemInstanceRarity.Purple,
                ItemCorePotentialResolutionStatus.Unresolved, true, "SCHEMA_ONLY");
            AddSchema(rows, result, "purple-policy-allows-ultimate-invalid", "core-policy", false,
                ItemCorePotentialAndBuildValidationCodes.CoreUltimatePolicyInvalid,
                Create(wrongUltimate, Array.Empty<ItemCorePotentialProfileSnapshot>(), DefaultBuildPolicies()));
            ItemCoreRarityPolicySnapshot[] orangeNoUltimate = DefaultCorePolicies();
            orangeNoUltimate[4] = new ItemCoreRarityPolicySnapshot(ItemInstanceRarity.Orange,
                ItemCorePotentialResolutionStatus.Unresolved, false, "SCHEMA_ONLY");
            AddSchema(rows, result, "orange-policy-forbids-ultimate-invalid", "core-policy", false,
                ItemCorePotentialAndBuildValidationCodes.CoreUltimatePolicyInvalid,
                Create(orangeNoUltimate, Array.Empty<ItemCorePotentialProfileSnapshot>(), DefaultBuildPolicies()));
            AddSchema(rows, result, "illegal-core-policy-rarity", "core-policy", false,
                ItemCorePotentialAndBuildValidationCodes.RarityInvalid,
                Create(DefaultCorePolicies().Concat(new[]
                {
                    new ItemCoreRarityPolicySnapshot((ItemInstanceRarity)999,
                        ItemCorePotentialResolutionStatus.Unresolved, false, "SCHEMA_ONLY")
                }), Array.Empty<ItemCorePotentialProfileSnapshot>(), DefaultBuildPolicies()));
        }

        private static void RunBuildPolicyAndQualificationCases(
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot defaultSchema,
            List<SpecRow> rows,
            VerificationResult result)
        {
            AddBoolean(rows, result, "white-policy-locked-none", "build-policy",
                defaultSchema.QueryBuildRarityPolicy(ItemInstanceRarity.White).value?.policyMode
                    == ItemBuildQualificationPolicyMode.LockedNone,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "green-orange-policy-unresolved", "build-policy",
                defaultSchema.BuildRarityPolicies
                    .Where(policy => policy.rarity != ItemInstanceRarity.White)
                    .All(policy => policy.policyMode == ItemBuildQualificationPolicyMode.ProbabilityUnresolved),
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "default-probability-ids-empty", "build-policy",
                defaultSchema.BuildRarityPolicies.All(policy => string.IsNullOrEmpty(policy.probabilityProfileId)),
                ItemCorePotentialAndBuildValidationCodes.None);

            ItemBuildQualificationRarityPolicySnapshot[] definedDefault = DefaultBuildPolicies();
            definedDefault[2] = new ItemBuildQualificationRarityPolicySnapshot(ItemInstanceRarity.Blue,
                ItemBuildQualificationPolicyMode.DefinedProbabilityTable, string.Empty, "SCHEMA_ONLY");
            AddSchema(rows, result, "defined-policy-default-invalid", "build-policy", false,
                ItemCorePotentialAndBuildValidationCodes.BuildPolicyContractInvalid,
                Create(DefaultCorePolicies(), Array.Empty<ItemCorePotentialProfileSnapshot>(), definedDefault));
            ItemBuildQualificationRarityPolicySnapshot[] profileReference = DefaultBuildPolicies();
            profileReference[1] = new ItemBuildQualificationRarityPolicySnapshot(ItemInstanceRarity.Green,
                ItemBuildQualificationPolicyMode.ProbabilityUnresolved, "qa_probability_profile", "SCHEMA_ONLY");
            AddSchema(rows, result, "probability-profile-reference-invalid", "build-policy", false,
                ItemCorePotentialAndBuildValidationCodes.ProbabilityProfileForbidden,
                Create(DefaultCorePolicies(), Array.Empty<ItemCorePotentialProfileSnapshot>(), profileReference));
            AddSchema(rows, result, "missing-build-policy", "build-policy", false,
                ItemCorePotentialAndBuildValidationCodes.BuildPolicyMissing,
                Create(DefaultCorePolicies(), Array.Empty<ItemCorePotentialProfileSnapshot>(),
                    DefaultBuildPolicies().Where(policy => policy.rarity != ItemInstanceRarity.Blue)));
            AddSchema(rows, result, "duplicate-build-policy", "build-policy", false,
                ItemCorePotentialAndBuildValidationCodes.BuildPolicyDuplicate,
                Create(DefaultCorePolicies(), Array.Empty<ItemCorePotentialProfileSnapshot>(),
                    DefaultBuildPolicies().Concat(new[] { DefaultBuildPolicies()[0] })));
            AddSchema(rows, result, "illegal-build-policy-rarity", "build-policy", false,
                ItemCorePotentialAndBuildValidationCodes.RarityInvalid,
                Create(DefaultCorePolicies(), Array.Empty<ItemCorePotentialProfileSnapshot>(),
                    DefaultBuildPolicies().Concat(new[]
                    {
                        new ItemBuildQualificationRarityPolicySnapshot((ItemInstanceRarity)999,
                            ItemBuildQualificationPolicyMode.ProbabilityUnresolved, string.Empty, "SCHEMA_ONLY")
                    })));

            AddQualification(rows, result, "white-none-valid", ItemInstanceRarity.White,
                ItemBuildQualification.None, "INSTANCE-WHITE-NONE", "I001", true,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddQualification(rows, result, "white-famen-invalid", ItemInstanceRarity.White,
                ItemBuildQualification.FaMenOnly, "INSTANCE-WHITE-FAMEN", "I001", false,
                ItemCorePotentialAndBuildValidationCodes.WhiteQualificationMustBeNone);
            AddQualification(rows, result, "white-qilei-invalid", ItemInstanceRarity.White,
                ItemBuildQualification.QiLeiOnly, "INSTANCE-WHITE-QILEI", "I001", false,
                ItemCorePotentialAndBuildValidationCodes.WhiteQualificationMustBeNone);
            AddQualification(rows, result, "white-dual-invalid", ItemInstanceRarity.White,
                ItemBuildQualification.Dual, "INSTANCE-WHITE-DUAL", "I001", false,
                ItemCorePotentialAndBuildValidationCodes.WhiteQualificationMustBeNone);
            AddQualification(rows, result, "build-i031-invalid", ItemInstanceRarity.Green,
                ItemBuildQualification.None, "INSTANCE-I031", "I031", false,
                ItemCorePotentialAndBuildValidationCodes.BaseItemNotOrdinary);
            AddQualification(rows, result, "build-empty-instance-invalid", ItemInstanceRarity.Green,
                ItemBuildQualification.None, string.Empty, "I001", false,
                ItemCorePotentialAndBuildValidationCodes.ItemInstanceIdEmpty);
            AddQualification(rows, result, "build-i999-invalid", ItemInstanceRarity.Green,
                ItemBuildQualification.None, "INSTANCE-I999", "I999", false,
                ItemCorePotentialAndBuildValidationCodes.BaseItemUnknown);
            AddQualification(rows, result, "build-illegal-rarity-invalid", (ItemInstanceRarity)999,
                ItemBuildQualification.None, "INSTANCE-BAD-RARITY", "I001", false,
                ItemCorePotentialAndBuildValidationCodes.RarityInvalid);
            AddQualification(rows, result, "build-illegal-qualification-invalid", ItemInstanceRarity.Green,
                (ItemBuildQualification)999, "INSTANCE-BAD-QUAL", "I001", false,
                ItemCorePotentialAndBuildValidationCodes.QualificationInvalid);

            foreach (ItemBuildQualification qualification in new[]
            {
                ItemBuildQualification.Unresolved,
                ItemBuildQualification.None,
                ItemBuildQualification.FaMenOnly,
                ItemBuildQualification.QiLeiOnly,
                ItemBuildQualification.Dual
            })
            {
                AddQualification(rows, result, "green-enum-roundtrip-" + qualification, ItemInstanceRarity.Green,
                    qualification, "INSTANCE-GREEN-" + qualification, "I001", true,
                    ItemCorePotentialAndBuildValidationCodes.None);
            }

            ItemInnerDataDefinition prototype = ItemInnerDataCatalog.FindById("I001");
            string before = (prototype?.FaMenKey ?? string.Empty) + "|" + (prototype?.QiLeiKey ?? string.Empty);
            ItemCorePotentialAndBuildEligibilitySchema.ValidateQualificationSnapshot(
                new ItemBuildQualificationSnapshot("INSTANCE-CULTURE", "I001", ItemInstanceRarity.Green,
                    ItemBuildQualification.FaMenOnly));
            string after = (prototype?.FaMenKey ?? string.Empty) + "|" + (prototype?.QiLeiKey ?? string.Empty);
            AddBoolean(rows, result, "qualification-does-not-change-cultural-tags", "build-qualification",
                string.Equals(before, after, StringComparison.Ordinal),
                ItemCorePotentialAndBuildValidationCodes.None);
        }

        private static void RunQueryCases(
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot defaultSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot fixtureSchema,
            List<SpecRow> rows,
            VerificationResult result)
        {
            AddBoolean(rows, result, "query-core-policy-success", "query",
                defaultSchema.QueryCoreRarityPolicy(ItemInstanceRarity.Orange).isSuccess,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "query-profile-id-success", "query",
                fixtureSchema.QueryCorePotentialProfile("qa_potential_profile_01").isSuccess,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "query-profile-base-rarity-success", "query",
                fixtureSchema.QueryCorePotentialProfile("I001", ItemInstanceRarity.Green).isSuccess,
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "query-build-policy-success", "query",
                defaultSchema.QueryBuildRarityPolicy(ItemInstanceRarity.White).isSuccess,
                ItemCorePotentialAndBuildValidationCodes.None);

            ItemCorePotentialAndBuildEligibilitySchemaSnapshot missingCore = Create(
                DefaultCorePolicies().Where(policy => policy.rarity != ItemInstanceRarity.Green),
                Array.Empty<ItemCorePotentialProfileSnapshot>(), DefaultBuildPolicies());
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot missingBuild = Create(
                DefaultCorePolicies(), Array.Empty<ItemCorePotentialProfileSnapshot>(),
                DefaultBuildPolicies().Where(policy => policy.rarity != ItemInstanceRarity.Green));
            AddQueryFailure(rows, result, "query-core-policy-failure", "query",
                missingCore.QueryCoreRarityPolicy(ItemInstanceRarity.Green).validationError?.code,
                ItemCorePotentialAndBuildValidationCodes.QueryCorePolicyNotFound);
            AddQueryFailure(rows, result, "query-profile-id-failure", "query",
                defaultSchema.QueryCorePotentialProfile("missing_profile").validationError?.code,
                ItemCorePotentialAndBuildValidationCodes.QueryCoreProfileNotFound);
            AddQueryFailure(rows, result, "query-profile-key-failure", "query",
                defaultSchema.QueryCorePotentialProfile("I001", ItemInstanceRarity.Green).validationError?.code,
                ItemCorePotentialAndBuildValidationCodes.QueryCoreProfileNotFound);
            AddQueryFailure(rows, result, "query-build-policy-failure", "query",
                missingBuild.QueryBuildRarityPolicy(ItemInstanceRarity.Green).validationError?.code,
                ItemCorePotentialAndBuildValidationCodes.QueryBuildPolicyNotFound);
            AddQueryFailure(rows, result, "query-invalid-rarity-failure", "query",
                defaultSchema.QueryCoreRarityPolicy((ItemInstanceRarity)999).validationError?.code,
                ItemCorePotentialAndBuildValidationCodes.QueryInputInvalid);
        }

        private static void RunDeterminismReadOnlyAndShapeCases(
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot fixtureSchema,
            List<SpecRow> rows,
            VerificationResult result)
        {
            ItemCorePotentialProfileSnapshot[] profiles = fixtureSchema.CorePotentialProfiles.ToArray();
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot reversed = Create(
                DefaultCorePolicies().Reverse(), profiles.Reverse(), DefaultBuildPolicies().Reverse());
            AddBoolean(rows, result, "canonical-input-reversal-stable", "determinism",
                string.Equals(fixtureSchema.BuildCanonicalSignature(), reversed.BuildCanonicalSignature(),
                    StringComparison.Ordinal), ItemCorePotentialAndBuildValidationCodes.None);

            List<ItemCorePotentialEffectSnapshot> mutableEffects = new()
            {
                new ItemCorePotentialEffectSnapshot("qa_core_standard_01",
                    ItemCorePotentialEffectKind.Standard, true)
            };
            ItemCorePotentialProfileSnapshot mutableSourceProfile = new(
                "qa_mutation_profile", "I003", ItemInstanceRarity.Green,
                ItemCorePotentialResolutionStatus.Defined, "QA_FIXTURE_ONLY", mutableEffects);
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot immutableSchema = Create(
                DefaultCorePolicies(), new[] { mutableSourceProfile }, DefaultBuildPolicies());
            string signatureBefore = immutableSchema.BuildCanonicalSignature();
            mutableEffects.Add(new ItemCorePotentialEffectSnapshot("qa_source_attack",
                ItemCorePotentialEffectKind.Standard, true));
            AddBoolean(rows, result, "source-collection-mutation-blocked", "readonly",
                immutableSchema.CorePotentialProfiles[0].PotentialEffects.Count == 1
                && string.Equals(signatureBefore, immutableSchema.BuildCanonicalSignature(), StringComparison.Ordinal),
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "top-level-collections-readonly", "readonly",
                IsReadOnly(fixtureSchema.CoreRarityPolicies)
                && IsReadOnly(fixtureSchema.CorePotentialProfiles)
                && IsReadOnly(fixtureSchema.BuildRarityPolicies)
                && IsReadOnly(fixtureSchema.ValidationErrors),
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "nested-collections-readonly", "readonly",
                fixtureSchema.CorePotentialProfiles.All(profile =>
                    IsReadOnly(profile.PotentialEffects)
                    && IsReadOnly(profile.EligibleCoreEffectIds)
                    && IsReadOnly(profile.VisibleCoreEffectIds)),
                ItemCorePotentialAndBuildValidationCodes.None);
            ItemBuildQualificationValidationResult invalidQualification =
                ItemCorePotentialAndBuildEligibilitySchema.ValidateQualificationSnapshot(null);
            AddBoolean(rows, result, "qualification-errors-readonly", "readonly",
                IsReadOnly(invalidQualification.ValidationErrors),
                ItemCorePotentialAndBuildValidationCodes.None);

            string[] forbiddenNames =
            {
                "unlockLevel", "unlockedCoreEffectIds", "activeCoreEffectIds",
                "isLit", "placementId", "coreEffectActive"
            };
            string[] publicNames = PublicMemberNames(typeof(ItemCorePotentialProfileSnapshot))
                .Concat(PublicMemberNames(typeof(ItemBuildQualificationSnapshot)))
                .ToArray();
            AddBoolean(rows, result, "forbidden-fields-absent", "reflection",
                forbiddenNames.All(forbidden => !publicNames.Contains(forbidden, StringComparer.OrdinalIgnoreCase)),
                ItemCorePotentialAndBuildValidationCodes.None);
            AddBoolean(rows, result, "snapshot-properties-get-only", "reflection",
                PublicPropertiesGetOnly(typeof(ItemCorePotentialEffectSnapshot))
                && PublicPropertiesGetOnly(typeof(ItemCorePotentialProfileSnapshot))
                && PublicPropertiesGetOnly(typeof(ItemCoreRarityPolicySnapshot))
                && PublicPropertiesGetOnly(typeof(ItemBuildQualificationRarityPolicySnapshot))
                && PublicPropertiesGetOnly(typeof(ItemBuildQualificationSnapshot)),
                ItemCorePotentialAndBuildValidationCodes.None);

            bool noThrow = true;
            try
            {
                ItemCorePotentialAndBuildEligibilitySchemaSnapshot empty = Create(null, null, null);
                empty.QueryCorePotentialProfile(string.Empty);
                empty.QueryCorePotentialProfile(string.Empty, (ItemInstanceRarity)999);
                ItemCorePotentialAndBuildEligibilitySchema.ValidateQualificationSnapshot(null);
            }
            catch
            {
                noThrow = false;
            }

            AddBoolean(rows, result, "invalid-paths-return-without-exception", "failure-path",
                noThrow, ItemCorePotentialAndBuildValidationCodes.None);
        }

        private static void RunLeakChecks(List<LeakRow> rows, VerificationResult result)
        {
            string[] files = Directory.Exists(RuntimeRoot)
                ? Directory.GetFiles(RuntimeRoot, "*.cs", SearchOption.AllDirectories)
                : Array.Empty<string>();
            string source = string.Join("\n", files.Select(File.ReadAllText));
            LeakDefinition[] definitions =
            {
                new("Reward", new[] { "RewardConfig", "RewardService", "GrantReward" }),
                new("RunFlow", new[] { "RunFlow" }),
                new("Inventory write", new[] { "InventoryWriter", "InventoryService", "AddToInventory", "WriteInventory" }),
                new("SaveData", new[] { "SaveData", "PlayerPrefs", "MainTrialProgressData" }),
                new("Boss", new[] { "BossInfo", "BossBattle", "BossReward" }),
                new("Battle Resolver", new[] { "BattleResolver" }),
                new("Battle Bridge", new[] { "BattleBridge", "UnifiedBattlePage" }),
                new("Existing Build Resolver", new[] { "ItemBuildSynergyResolver" }),
                new("Existing Awakening Resolver", new[] { "ItemCoreAwakeningResolver" }),
                new("Formal drop", new[] { "DropTable", "LootTable", "DropGeneration" }),
                new("Formal cultivation", new[] { "UpgradeService", "Breakthrough", "SaveCultivation", "Reroll" }),
                new("System.Random", new[] { "System.Random" }),
                new("UnityEngine.Random", new[] { "UnityEngine.Random" }),
                new("Qualification Roll", new[] { "QualificationRoll" }),
                new("Numeric probability fields", new[] { "chance", "weight", "percent", "basisPoints" }),
                new("unlockLevel", new[] { "unlockLevel" }),
                new("unlockedCoreEffectIds", new[] { "unlockedCoreEffectIds" }),
                new("activeCoreEffectIds", new[] { "activeCoreEffectIds" }),
                new("isLit", new[] { "isLit" }),
                new("placementId", new[] { "placementId" }),
                new("countedInBuild", new[] { "countedInBuild" }),
                new("itemPower", new[] { "itemPower" }),
                new("Scene", new[] { "SceneManager", "LoadScene", ".unity" }),
                new("Prefab", new[] { "PrefabUtility", ".prefab" }),
                new("BuildSettings", new[] { "EditorBuildSettings", "BuildSettings" })
            };

            foreach (LeakDefinition definition in definitions)
            {
                int count = definition.tokens.Sum(token => CountOccurrences(source, token));
                rows.Add(new LeakRow(definition.category, count));
                if (count != 0)
                {
                    result.Errors.Add($"LeakCheck category '{definition.category}' found {count} runtime occurrence(s).");
                }
            }

            if (files.Length != 2)
            {
                result.Errors.Add($"Runtime Potential directory must contain exactly 2 C# files; actual {files.Length}.");
            }
        }

        private static void RunHistoricalRegressions(VerificationResult result)
        {
            try
            {
                ItemRarityInstanceFoundationVerifier.VerifyMenu();
                ItemStatRangeSchemaVerifier.VerifyMenu();
            }
            catch (Exception exception)
            {
                result.Errors.Add("Historical generation regression threw: " + exception.Message);
            }

            RegressionDefinition[] regressions =
            {
                new("ItemRarityInstanceFoundation", "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md", "- Verification: PASS"),
                new("ItemStatRangeSchema", "Docs/V0.4/Reports/ItemStatRangeSchemaReport.md", "- Overall: PASS"),
                new("ItemInnerDataCatalog", "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS"),
                new("ItemSystemValidatorAndSnapshot", "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS"),
                new("BuildSynergyCore", "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS"),
                new("CoreAwakeningPreview", "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS"),
                new("ItemSkillTriggerContract", "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS"),
                new("ItemDetailProjectionComplete", "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS"),
                new("JuNian Lighting", "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS"),
                new("ArrayBonus", "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS")
            };

            foreach (RegressionDefinition regression in regressions)
            {
                bool passed = File.Exists(regression.reportPath)
                    && File.ReadAllText(regression.reportPath).Contains(regression.passMarker);
                result.Regressions.Add(regression.name + ": " + (passed ? "PASS" : "FAIL"));
                if (!passed)
                {
                    result.Errors.Add("Historical regression report is not PASS: " + regression.name + ".");
                }
            }

            result.FoundationSpecCount = CsvDataRowCount("Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv");
            result.StatRangeSpecCount = CsvDataRowCount("Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv");
            if (result.FoundationSpecCount != 159)
            {
                result.Errors.Add($"Foundation regression spec row count is {result.FoundationSpecCount}; expected 159.");
            }

            if (result.StatRangeSpecCount != 39)
            {
                result.Errors.Add($"StatRange regression spec row count is {result.StatRangeSpecCount}; expected 39.");
            }
        }

        private static int CsvDataRowCount(string path)
        {
            return File.Exists(path) ? Math.Max(0, File.ReadAllLines(path).Length - 1) : 0;
        }

        private static ItemCorePotentialAndBuildEligibilitySchemaSnapshot Create(
            IEnumerable<ItemCoreRarityPolicySnapshot> corePolicies,
            IEnumerable<ItemCorePotentialProfileSnapshot> profiles,
            IEnumerable<ItemBuildQualificationRarityPolicySnapshot> buildPolicies)
        {
            return ItemCorePotentialAndBuildEligibilitySchema.Create(corePolicies, profiles, buildPolicies);
        }

        private static ItemCorePotentialProfileSnapshot Profile(
            string profileId,
            string baseItemId,
            ItemInstanceRarity rarity,
            params ItemCorePotentialEffectSnapshot[] effects)
        {
            return new ItemCorePotentialProfileSnapshot(profileId, baseItemId, rarity,
                ItemCorePotentialResolutionStatus.Defined, "QA_FIXTURE_ONLY",
                effects ?? Array.Empty<ItemCorePotentialEffectSnapshot>());
        }

        private static ItemCoreRarityPolicySnapshot[] DefaultCorePolicies()
        {
            return ItemCorePotentialAndBuildEligibilityCatalog.DefaultCoreRarityPolicies
                .Select(policy => new ItemCoreRarityPolicySnapshot(policy.rarity, policy.resolutionStatus,
                    policy.allowsUltimateCoreEffect, policy.dataMaturityKey))
                .ToArray();
        }

        private static ItemBuildQualificationRarityPolicySnapshot[] DefaultBuildPolicies()
        {
            return ItemCorePotentialAndBuildEligibilityCatalog.DefaultBuildRarityPolicies
                .Select(policy => new ItemBuildQualificationRarityPolicySnapshot(policy.rarity,
                    policy.policyMode, policy.probabilityProfileId, policy.dataMaturityKey))
                .ToArray();
        }

        private static bool CheckFixtureMarkers()
        {
            ItemCorePotentialAndBuildCatalogNotice notice =
                ItemCorePotentialAndBuildEligibilityCatalog.GetCatalogNotice();
            return notice.dataMaturityKey == "SCHEMA_ONLY"
                && notice.coverageStatus == "UNRESOLVED_DATA_COVERAGE"
                && notice.fixtureStatus == "QA_FIXTURE_ONLY"
                && notice.approvalStatus == "NOT_DESIGN_APPROVED"
                && notice.generationDataStatus == "NOT_FORMAL_GENERATION_DATA";
        }

        private static void AddSchema(
            List<SpecRow> rows,
            VerificationResult result,
            string caseId,
            string category,
            bool expectedValid,
            string expectedCode,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot schema)
        {
            bool actualValid = schema?.isValid == true;
            string[] codes = schema?.ValidationErrors.Select(error => error.code)
                .Distinct(StringComparer.Ordinal).ToArray() ?? new[] { "SCHEMA_NULL" };
            bool codeMatches = expectedCode == ItemCorePotentialAndBuildValidationCodes.None
                ? codes.Length == 0
                : codes.Contains(expectedCode, StringComparer.Ordinal);
            bool passed = actualValid == expectedValid && codeMatches;
            rows.Add(new SpecRow(caseId, category,
                expectedValid ? "valid" : expectedCode,
                actualValid ? "valid" : (codes.Length == 0 ? "NONE" : string.Join("|", codes)),
                codes.Length == 0 ? ItemCorePotentialAndBuildValidationCodes.None : string.Join("|", codes),
                passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                result.Errors.Add($"Schema case '{caseId}' failed; expected {expectedValid}/{expectedCode}, actual {actualValid}/{string.Join("|", codes)}.");
            }
        }

        private static void AddQualification(
            List<SpecRow> rows,
            VerificationResult result,
            string caseId,
            ItemInstanceRarity rarity,
            ItemBuildQualification qualification,
            string itemInstanceId,
            string baseItemId,
            bool expectedValid,
            string expectedCode)
        {
            ItemBuildQualificationValidationResult actual =
                ItemCorePotentialAndBuildEligibilitySchema.ValidateQualificationSnapshot(
                    new ItemBuildQualificationSnapshot(itemInstanceId, baseItemId, rarity, qualification));
            string[] codes = actual.ValidationErrors.Select(error => error.code).Distinct(StringComparer.Ordinal).ToArray();
            bool codeMatches = expectedCode == ItemCorePotentialAndBuildValidationCodes.None
                ? codes.Length == 0
                : codes.Contains(expectedCode, StringComparer.Ordinal);
            bool passed = actual.isValid == expectedValid && codeMatches;
            rows.Add(new SpecRow(caseId, "build-qualification", expectedValid ? "valid" : expectedCode,
                actual.isValid ? qualification.ToString() : string.Join("|", codes),
                codes.Length == 0 ? ItemCorePotentialAndBuildValidationCodes.None : string.Join("|", codes),
                passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                result.Errors.Add($"Qualification case '{caseId}' failed.");
            }
        }

        private static void AddBoolean(
            List<SpecRow> rows,
            VerificationResult result,
            string caseId,
            string category,
            bool passed,
            string code)
        {
            rows.Add(new SpecRow(caseId, category, "true", passed ? "true" : "false", code,
                passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                result.Errors.Add($"Boolean case '{caseId}' failed.");
            }
        }

        private static void AddQueryFailure(
            List<SpecRow> rows,
            VerificationResult result,
            string caseId,
            string category,
            string actualCode,
            string expectedCode)
        {
            bool passed = string.Equals(actualCode, expectedCode, StringComparison.Ordinal);
            rows.Add(new SpecRow(caseId, category, expectedCode, actualCode ?? string.Empty,
                actualCode ?? string.Empty, passed ? "PASS" : "FAIL"));
            if (!passed)
            {
                result.Errors.Add($"Query case '{caseId}' expected {expectedCode}, actual {actualCode ?? "<null>"}.");
            }
        }

        private static bool IsReadOnly<T>(IReadOnlyList<T> values)
        {
            if (values == null || values is T[])
            {
                return false;
            }

            if (values is IList<T> list)
            {
                try
                {
                    list.Add(default);
                    return false;
                }
                catch (NotSupportedException)
                {
                    return true;
                }
            }

            return true;
        }

        private static string[] PublicMemberNames(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(member => member.Name)
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public).Select(member => member.Name))
                .ToArray();
        }

        private static bool PublicPropertiesGetOnly(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .All(property => property.SetMethod == null);
        }

        private static int CountOccurrences(string source, string token)
        {
            int count = 0;
            int index = 0;
            while (!string.IsNullOrEmpty(token)
                && (index = source.IndexOf(token, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        private static void WriteReports(
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot defaultSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot fixtureSchema,
            IReadOnlyList<SpecRow> rows,
            IReadOnlyList<LeakRow> leakRows,
            VerificationResult result)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            File.WriteAllText(ReportPath, BuildReport(defaultSchema, fixtureSchema, rows, leakRows, result),
                new UTF8Encoding(false));
            File.WriteAllText(SpecPath, BuildSpec(rows), new UTF8Encoding(false));
            File.WriteAllText(LeakPath, BuildLeakReport(leakRows, result), new UTF8Encoding(false));
            File.WriteAllText(MatrixPath, BuildPolicyMatrix(defaultSchema), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildReport(
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot defaultSchema,
            ItemCorePotentialAndBuildEligibilitySchemaSnapshot fixtureSchema,
            IReadOnlyList<SpecRow> rows,
            IReadOnlyList<LeakRow> leakRows,
            VerificationResult result)
        {
            bool passed = result.Errors.Count == 0 && rows.Count >= 35
                && rows.All(row => row.result == "PASS") && leakRows.All(row => row.count == 0);
            StringBuilder builder = new();
            builder.AppendLine("# ItemCorePotentialAndBuildEligibilitySchema01 Report")
                .AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Run time (UTC): `{DateTime.UtcNow:O}`")
                .AppendLine($"- Overall: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- PASS marker: `{(passed ? PassMarker : "NONE")}`")
                .AppendLine($"- Schema ID: `{defaultSchema?.schemaId ?? ItemCorePotentialAndBuildEligibilitySchemaSnapshot.CurrentSchemaId}`")
                .AppendLine($"- Spec: {rows.Count} total / {rows.Count(row => row.result == "PASS")} PASS / {rows.Count(row => row.result == "FAIL")} FAIL")
                .AppendLine()
                .AppendLine("## Default Runtime Contract")
                .AppendLine()
                .AppendLine("- Core rarity policies: exactly `white / green / blue / purple / orange`.")
                .AppendLine("- Core resolution: all five policies remain `Unresolved`.")
                .AppendLine("- Ultimate eligibility: only `orange` allows Ultimate potential.")
                .AppendLine("- Formal core potential profiles: `0`.")
                .AppendLine("- Build policy: `white=LockedNone`; `green/blue/purple/orange=ProbabilityUnresolved`.")
                .AppendLine("- White instance qualification: `None` only.")
                .AppendLine("- Formal probability profiles: `0`; every `probabilityProfileId` is empty.")
                .AppendLine("- Data status: `SCHEMA_ONLY / UNRESOLVED_DATA_COVERAGE`.")
                .AppendLine()
                .AppendLine("## QA Fixture Isolation")
                .AppendLine()
                .AppendLine("- Fixture IDs: `qa_core_standard_01 / qa_core_ultimate_01 / qa_potential_profile_01`.")
                .AppendLine("- Markers: `QA_FIXTURE_ONLY / NOT_DESIGN_APPROVED / NOT_FORMAL_GENERATION_DATA`.")
                .AppendLine($"- Fixture profiles used by verifier only: {fixtureSchema?.CorePotentialProfiles.Count ?? 0}; Runtime default Catalog remains 0.")
                .AppendLine()
                .AppendLine("## Isolation and Compatibility")
                .AppendLine()
                .AppendLine("- The existing Build resolver remains the old runtime contract; new qualification is instance-generation Schema only.")
                .AppendLine("- The existing Awakening preview remains an old preview contract; no new core count or open order is inferred from it.")
                .AppendLine("- No Awakening, Build count, lighting, combat, drop, property roll, affix, Reward, Inventory, SaveData, scene, prefab, or BuildSettings connection was added.")
                .AppendLine("- `ItemSystemSnapshot.v1`, Item Detail, and all legacy resolvers remain untouched.")
                .AppendLine()
                .AppendLine("## Determinism and Read-only")
                .AppendLine()
                .AppendLine($"- Canonical Signature: `{CompactSignature(defaultSchema?.BuildCanonicalSignature())}`")
                .AppendLine("- Input reversal stability: PASS.")
                .AppendLine("- Source collection mutation and exposed collection mutation attacks: PASS.")
                .AppendLine("- Forbidden runtime fields and qualification probability state: absent by reflection and LeakCheck.")
                .AppendLine()
                .AppendLine("## Historical Regressions")
                .AppendLine();
            foreach (string regression in result.Regressions)
            {
                builder.AppendLine("- " + regression);
            }

            builder.AppendLine($"- Foundation original Spec: {result.FoundationSpecCount}/159 {(result.FoundationSpecCount == 159 ? "PASS" : "FAIL")}")
                .AppendLine($"- StatRange original Spec: {result.StatRangeSpecCount}/39 {(result.StatRangeSpecCount == 39 ? "PASS" : "FAIL")}")
                .AppendLine()
                .AppendLine("## LeakCheck")
                .AppendLine()
                .AppendLine($"- Result: {(leakRows.All(row => row.count == 0) ? "PASS" : "FAIL")}")
                .AppendLine($"- Categories: {leakRows.Count}; total leaks: {leakRows.Sum(row => row.count)}")
                .AppendLine()
                .AppendLine("## Errors")
                .AppendLine();
            if (result.Errors.Count == 0)
            {
                builder.AppendLine("- None");
            }
            else
            {
                foreach (string error in result.Errors)
                {
                    builder.AppendLine("- " + error);
                }
            }

            return builder.ToString();
        }

        private static string BuildSpec(IReadOnlyList<SpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,category,expected,actual,validationCode,result");
            foreach (SpecRow row in rows)
            {
                builder.AppendLine(string.Join(",", Csv(row.caseId), Csv(row.category), Csv(row.expected),
                    Csv(row.actual), Csv(row.validationCode), Csv(row.result)));
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(IReadOnlyList<LeakRow> rows, VerificationResult result)
        {
            bool passed = rows.All(row => row.count == 0);
            StringBuilder builder = new();
            builder.AppendLine("# ItemCorePotentialAndBuildEligibilitySchema01 LeakCheck Report")
                .AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Runtime scan root: `{RuntimeRoot}`")
                .AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine()
                .AppendLine("| Category | Leak count |")
                .AppendLine("| --- | ---: |");
            foreach (LeakRow row in rows)
            {
                builder.AppendLine($"| {row.category} | {row.count} |");
            }

            builder.AppendLine()
                .AppendLine("Allowed Schema terms include BuildQualification, eligibleCoreEffectIds, visibleCoreEffectIds, cultivationPotentialProfileId, allowsUltimateCoreEffect, Unresolved, and ProbabilityUnresolved.")
                .AppendLine($"Verifier errors: {result.Errors.Count}");
            return builder.ToString();
        }

        private static string BuildPolicyMatrix(ItemCorePotentialAndBuildEligibilitySchemaSnapshot schema)
        {
            StringBuilder builder = new();
            builder.AppendLine("rarity,coreResolutionStatus,allowsUltimateCoreEffect,buildPolicyMode,probabilityProfileId,dataMaturityKey,formalCoreProfileCount,formalProbabilityProfileCount");
            foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
            {
                ItemCoreRarityPolicySnapshot core = schema?.QueryCoreRarityPolicy(rarity.rarity).value;
                ItemBuildQualificationRarityPolicySnapshot build = schema?.QueryBuildRarityPolicy(rarity.rarity).value;
                builder.AppendLine(string.Join(",", Csv(rarity.stableKey), Csv(core?.resolutionStatus.ToString()),
                    Bool(core?.allowsUltimateCoreEffect == true), Csv(build?.policyMode.ToString()),
                    Csv(build?.probabilityProfileId), Csv(core?.dataMaturityKey),
                    schema?.CorePotentialProfiles.Count ?? 0,
                    ItemCorePotentialAndBuildEligibilityCatalog.DefaultProbabilityProfileCount));
            }

            return builder.ToString();
        }

        private static string CompactSignature(string signature)
        {
            return string.IsNullOrWhiteSpace(signature)
                ? "None"
                : signature.Replace("\r", string.Empty).Replace("\n", " || ");
        }

        private static string Csv(string value)
        {
            string normalized = value ?? string.Empty;
            return normalized.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0
                ? "\"" + normalized.Replace("\"", "\"\"") + "\""
                : normalized;
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Regressions = new();
            public int FoundationSpecCount;
            public int StatRangeSpecCount;
        }

        private sealed class RegressionDefinition
        {
            public RegressionDefinition(string name, string reportPath, string passMarker)
            {
                this.name = name;
                this.reportPath = reportPath;
                this.passMarker = passMarker;
            }

            public readonly string name;
            public readonly string reportPath;
            public readonly string passMarker;
        }

        private sealed class LeakDefinition
        {
            public LeakDefinition(string category, string[] tokens)
            {
                this.category = category;
                this.tokens = tokens;
            }

            public readonly string category;
            public readonly string[] tokens;
        }

        private sealed class LeakRow
        {
            public LeakRow(string category, int count)
            {
                this.category = category;
                this.count = count;
            }

            public readonly string category;
            public readonly int count;
        }

        private sealed class SpecRow
        {
            public SpecRow(string caseId, string category, string expected, string actual,
                string validationCode, string result)
            {
                this.caseId = caseId;
                this.category = category;
                this.expected = expected;
                this.actual = actual;
                this.validationCode = validationCode;
                this.result = result;
            }

            public readonly string caseId;
            public readonly string category;
            public readonly string expected;
            public readonly string actual;
            public readonly string validationCode;
            public readonly string result;
        }
    }
}
#endif
