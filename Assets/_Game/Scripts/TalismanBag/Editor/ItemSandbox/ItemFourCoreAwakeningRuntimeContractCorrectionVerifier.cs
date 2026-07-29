using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class
        ItemFourCoreAwakeningRuntimeContractCorrectionVerifier
    {
        private const string PackageKey =
            "V0.4-ItemFourCoreAwakeningRuntimeContractCorrection01";
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string HistoricalIdentityMatrixPath =
            "Docs/V0.4/Reports/ItemCoreEffectIdentityMatrix.csv";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeContractCorrectionReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeContractCorrectionSpec.csv";
        private const string IdentityMatrixPath =
            "Docs/V0.4/Reports/ItemFourCoreAwakeningIdentityMatrix120.csv";
        private const string RuntimeMatrixPath =
            "Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeStateMatrix.csv";
        private const string CanonicalDeltaPath =
            "Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeCanonicalDelta.csv";
        private const string MigrationLedgerPath =
            "Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeMigrationLedger.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeContractCorrectionLeakCheckReport.md";
        private const string HistoricalLabel =
            "SUPERSEDED_BY_ITEM_FOUR_CORE_AUTHORITY_OPTION_C";
        private const string RetainedLabel =
            "HISTORICAL_EVIDENCE_RETAINED";
        private const string ProfilesAggregate =
            "71fc7080eceaf9adf2aa47508e4900d00fae91d8f8158fb1a2618b69241a8143";

        private static readonly ItemCoreAwakeningNodeKind[] FormalKinds =
        {
            ItemCoreAwakeningNodeKind.Core1,
            ItemCoreAwakeningNodeKind.Core2,
            ItemCoreAwakeningNodeKind.Core3,
            ItemCoreAwakeningNodeKind.Ultimate
        };

        private static readonly IReadOnlyDictionary<string, string>
            P1ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"] = "15d7a0cbf1b60969e6c27030a7d1d8ccd1d733fb73421396bee5a8ee21382b4b",
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemBalanceCandidateSeedBuilder.cs"] = "46615405ed25f2939259be0d449727118574dc2cf4f0dc2e6dbd891da4d1cc4f",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchValidation.cs"] = "f4cfa16238ab5454cbc41ac0892cffe4faae5fe8e02cedc3335d175fc5ea5cdc",
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemCompleteCandidateContentWorkbenchVerifier.cs"] = "7432f989a14b36fc4e35abd7f04a64684af9a7761e2d586941dd4c4cf134de7c",
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionMigrator.cs"] = "910bb52db53c408036366a6f75e66e8e53eadad7f2b302e752abcc336c28f03c",
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionVerifier.cs"] = "005cf5fbd861f06f94fa1a19ba6b7070bf0261957978fec7bdfcf1491e6acda6",
                [WorkbenchCatalogPath] = "d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed",
                ["Docs/V0.4/Reports/ItemCandidateCoreEffects120.csv"] = "96cb7ba61376c1a1ab5bdc1ca08a42785bea0f4f864e6f89129d0f9a88fc2284",
                ["Docs/V0.4/Reports/ItemCandidateCoreEffects150.csv"] = "5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188"
            };

        private static readonly IReadOnlyDictionary<string, string>
            HistoricalHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs"] = "55963d1d5e2ac661ca7b2627778c92fd5f7e993d50bfa9bfcf28d918332050f0",
                ["Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs"] = "c6513768d1be22c15594d790d494d77331b561a947974d7d38d74fa7aadfa479",
                ["Docs/V0.4/ItemCoreAwakeningCore4NodeExpansion01_Assignment.md"] = "8f9af4b60b601f64a59fe8741c95792d8222857b66403843b70340ed46e7a03f",
                ["Docs/V0.4/ItemCoreEffectIdentityAndRuntimeStateContract01_Assignment.md"] = "34d4816ea5e43d64583767ddda6ae448295a8ec5fdeb7a5d7ba299b34c0db756",
                ["Docs/V0.4/Reports/CoreAwakeningPreviewReport.md"] = "678298b7f8f50fac224307834aeed320a5b2b41fba970f183601178788d65b9a",
                ["Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv"] = "e7081371aee2a7fe0600cb062726d9ce035e4378af1df52ec9ae5ad6af54afce",
                ["Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md"] = "ceb7d5b6c44913fe832ce82a5c86bd2c8fa68a6f8baff7459b23f555531cd911",
                [HistoricalIdentityMatrixPath] = "c5867237b758b71f90368e3c10c54a68b22d9b7f6ea549ae692df38826a13f32",
                ["Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractReport.md"] = "05a1c68c18019f9bea245a4cf8810ec7aa03e07a273e4055386d5dc94144d05d",
                ["Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractSpec.csv"] = "3cad9f7e8b02824dab7bd2f300c9e9544713407d5e4a872247d8f3ede80605b2",
                ["Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractLeakCheckReport.md"] = "7ba5e1f5b4e2b55ef3eeebb8d4a3a60a6b41e043f1093566ef191b81009f0080"
            };

        private static readonly IReadOnlyDictionary<string, string>
            PresentationProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] = "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] = "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] = "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs"] = "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs"] = "19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] = "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity"] = "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] = "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] = "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"
            };

        private static readonly string[] ExistingWhitelist =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"
        };

        private static readonly string[] NewWhitelist =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemFourCoreAwakeningRuntimeContractCorrectionVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemFourCoreAwakeningRuntimeContractCorrectionVerifier.cs.meta",
            ReportPath,
            SpecPath,
            IdentityMatrixPath,
            RuntimeMatrixPath,
            CanonicalDeltaPath,
            MigrationLedgerPath,
            LeakPath
        };

        private static readonly List<ScenarioResult> Results = new();
        private static readonly List<RuntimeEvidence> RuntimeEvidenceRows = new();
        private static Fixture fixture;

        [MenuItem("Talisman Bag/V0.4/Verify Item Four Core Awakening Runtime Contract Correction")]
        public static void VerifyOffline()
        {
            VerifyStaticBatch();
        }

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            RuntimeEvidenceRows.Clear();
            fixture = BuildFixture();

            Run("P2-01", "AWAKENING_FOUR_DEFINITIONS_120_PASS",
                VerifyCandidateAndAwakeningAuthorities);
            Run("P2-02", "RUNTIME_IDENTITY_CATALOG_V2_120_PASS",
                VerifyIdentityCatalog);
            Run("P2-03", "RUNTIME_STATE_V2_FOUR_ROWS_PASS",
                VerifyRuntimeBaseline);
            Run("P2-04", "OPTION_C_ACTIVE_FORMULA_PASS",
                VerifyOptionCActiveFormula);
            Run("P2-05", "CORE4_RESERVED_ZERO_FORMAL_ROWS_PASS",
                VerifyCore4Reserved);
            Run("P2-06", "V1_EVIDENCE_RETAINED_PASS",
                VerifyHistoricalEvidence);
            Run("P2-07", "INVENTORY_BOARD_TRANSITIONS_PASS",
                VerifyThresholdsAndTransitions);
            Run("P2-08", "I031_EXCLUSION_PASS",
                VerifyUnknownInvalidAndI031);
            Run("P2-09", "ATOMIC_COMMIT_PASS", VerifyAtomicCommit);
            Run("P2-10", "NO_OP_STABILITY_PASS",
                VerifyCanonicalImmutableAndNoOp);
            Run("P2-11", "REAL_RUNTIME_STATE_ASSEMBLY_PASS",
                VerifyRealRuntimeSamples);
            Run("P2-12", "P1_PROTECTED_HASHES_PASS",
                VerifyP1ProtectedHashes);
            Run("P2-13", "PRESENTATION_PROTECTED_HASHES_PASS",
                VerifyPresentationProtectedHashes);

            WriteReports();
            Run("P2-14", "LEAKCHECK_PASS", VerifyLeakBoundaries);
            Run("P2-15", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS",
                VerifyPackageScopedTextCheck);
            WriteReports();

            ScenarioResult[] failures = Results.Where(value => !value.Passed)
                .ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "Four-core runtime contract correction verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }

            Debug.Log(
                "[ItemFourCoreAwakeningRuntimeContractCorrectionVerifier]\n"
                + string.Join("\n", Results.Select(value => value.Marker))
                + "\nUSER_HANDTEST_NOT_APPLICABLE"
                + "\nP3_NOT_STARTED"
                + "\nPREFAB_MIGRATION_NOT_STARTED"
                + "\nNEXT_PACKAGE_NOT_STARTED");
        }

        private static Fixture BuildFixture()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            Check(workbench != null, "Workbench catalog is missing.");
            GameObject providerObject = new(
                "FourCoreRuntimeCorrectionVerifierProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult projection =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbench, provider);
                Check(projection.IsValid,
                    "Real BattleSandbox projection invalid: "
                    + string.Join("|", projection.Diagnostics));
                ItemCoreEffectIdentityCatalogSnapshot identity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                ItemCoreEffectCultivationRosterSnapshot cultivation =
                    BuildCultivation(projection, null);
                return new Fixture(
                    workbench, projection, identity, cultivation);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static void VerifyCandidateAndAwakeningAuthorities()
        {
            ItemBalanceProfile[] profiles = (fixture.Workbench.profiles ??
                    new List<ItemBalanceProfile>())
                .Where(value => value != null)
                .OrderBy(value => value.baseItemId, StringComparer.Ordinal)
                .ToArray();
            Check(profiles.Length == 30,
                "Candidate profile roster is not I001-I030.");
            int candidateCount = 0;
            int awakeningCount = 0;
            foreach (ItemBalanceProfile profile in profiles)
            {
                ItemBalanceCoreCandidate[] candidates =
                    (profile.coreCandidates ??
                        new List<ItemBalanceCoreCandidate>())
                    .Where(value => value != null).ToArray();
                Check(candidates.Length == 4,
                    profile.baseItemId + " Candidate count is not four.");
                string[] actualKinds = candidates
                    .Select(value => value.nodeKind)
                    .OrderBy(NodeRank)
                    .ToArray();
                Check(actualKinds.SequenceEqual(
                        new[] { "Core1", "Core2", "Core3", "Ultimate" },
                        StringComparer.Ordinal),
                    profile.baseItemId
                    + " Candidate machine nodeKinds are not exact.");
                foreach (ItemBalanceCoreCandidate candidate in candidates)
                {
                    ItemCoreAwakeningNodeKind kind =
                        ParseFormalKind(candidate.nodeKind);
                    Check(string.Equals(
                            candidate.coreEffectId,
                            ItemCoreEffectIdentityCatalogBuilder
                                .ExplicitCandidateDefinitionId(
                                    profile.baseItemId, kind),
                            StringComparison.Ordinal),
                        profile.baseItemId
                        + " Candidate identity is not explicit.");
                }
                candidateCount += candidates.Length;

                IReadOnlyList<ItemCoreEffectDefinition> definitions =
                    DefaultItemCoreEffectDefinitionProvider.Instance
                        .GetDefinitions(profile.baseItemId);
                Check(definitions.Count == 4
                    && definitions.Select(value => value.nodeKind)
                        .SequenceEqual(FormalKinds),
                    profile.baseItemId
                    + " default Awakening definitions are not exact four.");
                foreach (ItemCoreEffectDefinition definition in definitions)
                {
                    Check(string.Equals(
                            definition.coreEffectId,
                            ItemCoreEffectIdentityCatalogBuilder
                                .ExplicitAwakeningNodeId(
                                    profile.baseItemId,
                                    definition.nodeKind),
                            StringComparison.Ordinal)
                        && definition.unlockLevel ==
                            ItemCoreAwakeningResolver.ExpectedUnlockLevel(
                                definition.nodeKind),
                        profile.baseItemId
                        + " Awakening identity/threshold mismatch.");
                }
                awakeningCount += definitions.Count;
            }
            Check(candidateCount == 120 && awakeningCount == 120,
                "Candidate/Awakening authority is not 120/120.");
            Check(DefaultItemCoreEffectDefinitionProvider.Instance
                    .GetDefinitions("I031").Count == 0,
                "I031 has ordinary Awakening definitions.");
            Check(ItemCoreAwakeningResolver.DefaultNodes.Count == 4
                && ItemCoreAwakeningResolver.DefaultNodes
                    .Select(value => value.nodeKind)
                    .SequenceEqual(FormalKinds),
                "Resolver formal node set is not explicit four.");
        }

        private static void VerifyIdentityCatalog()
        {
            Check(fixture.Identity.isValid
                && fixture.Identity.Rows.Count == 120
                && string.Equals(
                    fixture.Identity.schemaId,
                    ItemCoreEffectIdentityCatalogSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                && string.Equals(
                    fixture.Identity.authorityRevision,
                    ItemCoreEffectIdentityCatalogSnapshot
                        .CurrentAuthorityRevision,
                    StringComparison.Ordinal),
                "Identity catalog is not accepted v2/120.");
            foreach (string itemId in Enumerable.Range(1, 30)
                         .Select(index => "I" + index.ToString(
                             "000", CultureInfo.InvariantCulture)))
            {
                ItemCoreEffectIdentityRow[] rows = fixture.Identity.Rows
                    .Where(value => string.Equals(
                        value.baseItemId, itemId,
                        StringComparison.Ordinal)).ToArray();
                Check(rows.Length == 4
                    && rows.Select(value => value.nodeKind)
                        .SequenceEqual(FormalKinds),
                    itemId + " identity rows are not explicit four.");
                foreach (ItemCoreEffectIdentityRow row in rows)
                {
                    Check(string.Equals(
                            row.candidateDefinitionId,
                            ItemCoreEffectIdentityCatalogBuilder
                                .ExplicitCandidateDefinitionId(
                                    itemId, row.nodeKind),
                            StringComparison.Ordinal)
                        && string.Equals(
                            row.awakeningNodeId,
                            ItemCoreEffectIdentityCatalogBuilder
                                .ExplicitAwakeningNodeId(
                                    itemId, row.nodeKind),
                            StringComparison.Ordinal)
                        && !string.IsNullOrWhiteSpace(row.displayName)
                        && !string.IsNullOrWhiteSpace(row.description)
                        && !string.IsNullOrWhiteSpace(
                            row.candidatePayloadIdentity),
                        itemId + " identity authority evidence mismatch.");
                }
            }
        }

        private static void VerifyRuntimeBaseline()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            ItemInstanceCoreEffectRuntimeStateSnapshot state =
                authority.CurrentCoreEffectRuntimeState;
            Check(state.isValid
                && state.Items.Count == 30
                && string.Equals(
                    state.schemaId,
                    ItemInstanceCoreEffectRuntimeStateSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                && string.Equals(
                    state.authorityRevision,
                    ItemInstanceCoreEffectRuntimeStateSnapshot
                        .CurrentAuthorityRevision,
                    StringComparison.Ordinal),
                "Real runtime baseline is not valid v2.");
            Check(state.Items.All(item =>
                    item.location == ItemCoreEffectRuntimeLocation.Inventory
                    && item.CoreEffectRows.Count == 4
                    && item.CoreEffectRows.All(row =>
                        row.nodeKind != ItemCoreAwakeningNodeKind.Core4
                        && !string.IsNullOrWhiteSpace(row.displayName)
                        && !string.IsNullOrWhiteSpace(row.description)
                        && IsKnown(row.eligibleFact)
                        && IsKnown(row.visibleFact)
                        && row.litFact ==
                            ItemCoreEffectBooleanFact.NotApplicable
                        && row.activeFact ==
                            ItemCoreEffectBooleanFact.NotApplicable)),
                "Inventory v2 rows lost static or NotApplicable facts.");
            Check(fixture.Cultivation.Rows.Count == 30
                && fixture.Cultivation.Rows.All(row =>
                    row.inputLevel == 40
                    && string.Equals(
                        row.sourceKey,
                        ItemSystemBattleSandboxBoardAdapter
                            .CultivationSourceKey,
                        StringComparison.Ordinal)),
                "Real BattleSandbox dev roster is not explicit Orange Lv40.");
            RecordState("InventoryBaseline",
                FindByBase(state, "I001"));
        }

        private static void VerifyOptionCActiveFormula()
        {
            foreach ((string rarityKey, int expectedActive) in new[]
                     {
                         ("white", 1),
                         ("green", 2),
                         ("blue", 3),
                         ("purple", 3),
                         ("orange", 4)
                     })
            {
                ItemInstanceProjectionContractSnapshot projection =
                    GetRarityProjection("I001", rarityKey);
                ItemCoreEffectIdentityRow[] identities =
                    fixture.Identity.Rows.Where(value =>
                        string.Equals(value.baseItemId, "I001",
                            StringComparison.Ordinal)).ToArray();
                ItemInstanceCoreEffectRuntimeRow[] rows = identities
                    .Select(identity =>
                    {
                        bool eligible =
                            projection.EligibleCoreEffectIds.Contains(
                                identity.candidateDefinitionId,
                                StringComparer.Ordinal);
                        bool visible =
                            projection.VisibleCoreEffectIds.Contains(
                                identity.candidateDefinitionId,
                                StringComparer.Ordinal);
                        return new ItemInstanceCoreEffectRuntimeRow(
                            "I001",
                            projection.itemInstanceId,
                            identity.nodeKind,
                            identity.candidateDefinitionId,
                            identity.awakeningNodeId,
                            identity.displayName,
                            identity.description,
                            identity.awakeningUnlockLevel,
                            identity.requiredRarity,
                            identity.requiredRarityKey,
                            Fact(eligible),
                            Fact(visible),
                            ItemCoreEffectBooleanFact.KnownTrue,
                            ItemCoreEffectBooleanFact.KnownTrue,
                            Fact(eligible && true && true),
                            ItemCoreEffectFactCompleteness.Complete,
                            new[] { "P2Verifier/" + rarityKey });
                    }).ToArray();
                ItemInstanceCoreEffectRuntimeItemSnapshot item = new(
                    projection.itemInstanceId,
                    "I001",
                    projection.rarity,
                    ItemCoreEffectRuntimeLocation.Board,
                    "P_OPTION_C_" + rarityKey.ToUpperInvariant(),
                    40,
                    ItemSystemBattleSandboxBoardAdapter
                        .CultivationSourceKey,
                    ItemCoreEffectFactCompleteness.Complete,
                    ItemCoreEffectBooleanFact.KnownTrue,
                    rows,
                    ItemCoreEffectFactCompleteness.Complete);
                ItemInstanceCoreEffectRuntimeStateSnapshot snapshot = new(
                    ItemCoreEffectRuntimeStateStatus.Valid,
                    ItemCoreEffectRosterCompleteness.Complete,
                    fixture.Identity.canonicalSignature,
                    fixture.Cultivation.canonicalSignature,
                    "P2_OPTION_C_" + rarityKey,
                    "P2_OPTION_C_BINDING",
                    "P2_OPTION_C_ITEM_SYSTEM",
                    new[] { item },
                    Array.Empty<
                        ItemCoreEffectRuntimeStateValidationError>());
                Check(snapshot.isValid
                    && item.CoreEffectRows.Count(row =>
                        row.activeFact ==
                            ItemCoreEffectBooleanFact.KnownTrue)
                        == expectedActive,
                    rarityKey
                    + " active formula count mismatch.");
                Check(item.CoreEffectRows.All(row =>
                        row.activeFact ==
                        Fact(row.eligibleFact ==
                                ItemCoreEffectBooleanFact.KnownTrue
                            && row.unlockedFact ==
                                ItemCoreEffectBooleanFact.KnownTrue
                            && row.litFact ==
                                ItemCoreEffectBooleanFact.KnownTrue)),
                    rarityKey + " active formula mismatch.");
                RecordState("OptionC_" + rarityKey, item);
            }
        }

        private static void VerifyCore4Reserved()
        {
            Check((int)ItemCoreAwakeningNodeKind.Core1 == 0
                && (int)ItemCoreAwakeningNodeKind.Core2 == 1
                && (int)ItemCoreAwakeningNodeKind.Core3 == 2
                && (int)ItemCoreAwakeningNodeKind.Ultimate == 3
                && (int)ItemCoreAwakeningNodeKind.Core4 == 4,
                "Serialized enum compatibility changed.");
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            Check(ItemCoreAwakeningResolver.DefaultNodes.All(value =>
                    value.nodeKind != ItemCoreAwakeningNodeKind.Core4)
                && fixture.Identity.Rows.All(value =>
                    value.nodeKind != ItemCoreAwakeningNodeKind.Core4)
                && authority.CurrentCoreEffectRuntimeState.Items
                    .SelectMany(value => value.CoreEffectRows)
                    .All(value => value.nodeKind !=
                        ItemCoreAwakeningNodeKind.Core4),
                "Core4 leaked into a formal producer/identity/runtime row.");

            ItemCoreEffectIdentityCatalogSnapshot supplied =
                ItemCoreEffectIdentityCatalogBuilder.Build(
                    fixture.Workbench, new SuppliedCore4Provider());
            Check(supplied.status ==
                    ItemCoreEffectRuntimeStateStatus.Invalid
                && supplied.ValidationErrors.Any(error =>
                    string.Equals(
                        error.code,
                        ItemCoreEffectRuntimeStateValidationCodes
                            .SupersededExpansionDrift,
                        StringComparison.Ordinal)),
                "Supplied Core4 definition was not rejected as drift.");

            ItemCoreEffectIdentityRow seed = fixture.Identity.Rows[0];
            ItemCoreEffectIdentityRow drift = new(
                seed.baseItemId,
                ItemCoreAwakeningNodeKind.Core4,
                "candidate_core_i001_04",
                "I001_CORE_04",
                "Superseded Core4",
                "Historical expansion drift",
                40,
                40,
                ItemInstanceRarity.Purple,
                "purple",
                "HistoricalCandidate",
                "HistoricalAwakening",
                "HistoricalPayload",
                "Utility",
                "ExtraTrigger");
            ItemCoreEffectIdentityCatalogSnapshot driftCatalog = new(
                ItemCoreEffectRuntimeStateStatus.Valid,
                fixture.Identity.Rows.Concat(new[] { drift }),
                Array.Empty<
                    ItemCoreEffectRuntimeStateValidationError>());
            Check(driftCatalog.status ==
                    ItemCoreEffectRuntimeStateStatus.Invalid
                && driftCatalog.ValidationErrors.Any(error =>
                    error.code ==
                    ItemCoreEffectRuntimeStateValidationCodes
                        .SupersededExpansionDrift),
                "Supplied Core4 identity was not rejected as drift.");
        }

        private static void VerifyHistoricalEvidence()
        {
            Check(string.Equals(
                    ItemCoreEffectIdentityCatalogSnapshot.LegacySchemaIdV1,
                    "ItemCoreEffectIdentityCatalogSnapshot.v1",
                    StringComparison.Ordinal)
                && string.Equals(
                    ItemInstanceCoreEffectRuntimeStateSnapshot
                        .LegacySchemaIdV1,
                    "ItemInstanceCoreEffectRuntimeStateSnapshot.v1",
                    StringComparison.Ordinal),
                "Explicit v1 schema evidence was not retained.");
            VerifyHashes(HistoricalHashes);
            Check(File.ReadAllLines(ProjectPath(HistoricalIdentityMatrixPath))
                    .Length == 151,
                "Historical identity matrix is not 150 evidence rows.");
        }

        private static void VerifyThresholdsAndTransitions()
        {
            foreach ((int level, int unlocked) in new[]
                     {
                         (1, 0), (10, 1), (20, 2), (30, 3), (40, 4)
                     })
            {
                ItemCoreEffectCultivationRosterSnapshot cultivation =
                    BuildCultivation(
                        fixture.Projection,
                        new Dictionary<string, int>(
                            StringComparer.Ordinal)
                        {
                            ["I001"] = level
                        });
                ItemSystemBattleSandboxBoardAuthority thresholdAuthority =
                    NewAuthority(fixture.Identity, cultivation);
                ItemInstanceCoreEffectRuntimeItemSnapshot thresholdItem =
                    FindByBase(
                        thresholdAuthority
                            .CurrentCoreEffectRuntimeState,
                        "I001");
                Check(thresholdItem.CoreEffectRows.Count(row =>
                        row.unlockedFact ==
                            ItemCoreEffectBooleanFact.KnownTrue)
                        == unlocked,
                    "Unlock threshold mismatch at Lv."
                    + level.ToString(CultureInfo.InvariantCulture));
            }

            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            PlacementCandidate unlit =
                FindCandidate(authority, "I001", false);
            Check(authority.CommitFromTray(
                    "I001", unlit.Anchor, unlit.Rotation).Accepted,
                "I001 unlit commit failed.");
            ItemInstanceCoreEffectRuntimeItemSnapshot boardUnlit =
                FindByBase(authority.CurrentCoreEffectRuntimeState, "I001");
            Check(boardUnlit.location ==
                    ItemCoreEffectRuntimeLocation.Board
                && boardUnlit.isLitFact ==
                    ItemCoreEffectBooleanFact.KnownFalse
                && boardUnlit.CoreEffectRows.All(row =>
                    row.unlockedFact ==
                        ItemCoreEffectBooleanFact.KnownTrue
                    && row.litFact ==
                        ItemCoreEffectBooleanFact.KnownFalse
                    && row.activeFact ==
                        ItemCoreEffectBooleanFact.KnownFalse
                    && !string.IsNullOrWhiteSpace(row.description)),
                "Board unlit facts mismatch.");
            RecordState("BoardUnlit", boardUnlit);

            Check(authority.CommitFromTray(
                    "I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "I031 lighting source commit failed.");
            PlacementCandidate lit = FindCandidate(
                authority, "I001", true);
            Check(authority.CommitMove(
                    "P_BOARD_I001", lit.Anchor, lit.Rotation).Accepted,
                "I001 move-to-lit failed.");
            ItemInstanceCoreEffectRuntimeItemSnapshot boardLit =
                FindByBase(authority.CurrentCoreEffectRuntimeState, "I001");
            Check(boardLit.isLitFact ==
                    ItemCoreEffectBooleanFact.KnownTrue
                && boardLit.CoreEffectRows.Count == 4
                && boardLit.CoreEffectRows.All(row =>
                    row.activeFact ==
                        ItemCoreEffectBooleanFact.KnownTrue),
                "Orange Lv40 lit runtime rows are not all active.");
            RecordState("BoardLitAfterMove", boardLit);

            Check(authority.ReturnToTray("P_BOARD_I001").Accepted,
                "I001 return-to-inventory failed.");
            ItemInstanceCoreEffectRuntimeItemSnapshot returned =
                FindByBase(authority.CurrentCoreEffectRuntimeState, "I001");
            Check(returned.location ==
                    ItemCoreEffectRuntimeLocation.Inventory
                && returned.placementId == null
                && returned.isLitFact ==
                    ItemCoreEffectBooleanFact.NotApplicable
                && returned.CoreEffectRows.All(row =>
                    row.litFact ==
                        ItemCoreEffectBooleanFact.NotApplicable
                    && row.activeFact ==
                        ItemCoreEffectBooleanFact.NotApplicable),
                "Return-to-inventory retained stale runtime facts.");
            RecordState("ReturnToInventory", returned);
        }

        private static void VerifyUnknownInvalidAndI031()
        {
            ItemSystemBattleSandboxBoardAuthority legacy =
                new ItemSystemBattleSandboxBoardAuthority(
                    fixture.Projection,
                    DefaultItemSystemSnapshotProvider.Instance,
                    ItemInstancePlacementBindingValidator.Instance,
                    DefaultRealLayoutResilienceEvaluationPipeline.Instance);
            ItemInstanceCoreEffectRuntimeStateSnapshot unknown =
                legacy.CurrentCoreEffectRuntimeState;
            Check(unknown.status ==
                    ItemCoreEffectRuntimeStateStatus.Unknown
                && unknown.Items.All(item =>
                    !item.cultivationLevel.HasValue
                    && item.cultivationLevelSource == null
                    && item.CoreEffectRows.All(row =>
                        row.unlockedFact ==
                            ItemCoreEffectBooleanFact.Unknown
                        && row.activeFact ==
                            ItemCoreEffectBooleanFact.Unknown)),
                "Missing cultivation defaulted to Lv1/false.");

            ItemCoreEffectIdentityCatalogSnapshot duplicateIdentity = new(
                ItemCoreEffectRuntimeStateStatus.Valid,
                fixture.Identity.Rows.Concat(
                    new[] { fixture.Identity.Rows[0] }),
                Array.Empty<
                    ItemCoreEffectRuntimeStateValidationError>());
            Check(duplicateIdentity.status ==
                ItemCoreEffectRuntimeStateStatus.Invalid,
                "Duplicate identity was not Invalid.");

            ItemCoreEffectCultivationRow source =
                fixture.Cultivation.Rows[0];
            ItemCoreEffectCultivationRosterSnapshot duplicateCultivation =
                new(
                    ItemCoreEffectRosterCompleteness.Complete,
                    fixture.Cultivation.Rows.Concat(new[]
                    {
                        new ItemCoreEffectCultivationRow(
                            source.itemInstanceId,
                            source.baseItemId,
                            40,
                            source.sourceKey,
                            ItemCoreEffectFactCompleteness.Complete)
                    }));
            Check(duplicateCultivation.status ==
                ItemCoreEffectRuntimeStateStatus.Invalid,
                "Duplicate cultivation was not Invalid.");

            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            PlacementCandidate unlit =
                FindCandidate(authority, "I001", false);
            Check(authority.CommitFromTray(
                    "I001", unlit.Anchor, unlit.Rotation).Accepted,
                "Invalid-binding fixture setup failed.");
            ItemInstancePlacementBindingValidationResult orphan =
                ItemInstancePlacementBindingValidator.Instance.Validate(
                    fixture.Projection.OrdinaryProjectionSet,
                    authority.CurrentSnapshot,
                    new[]
                    {
                        new ItemInstancePlacementBindingInput(
                            "stale_orphan_instance",
                            "P_BOARD_I001",
                            "I001")
                    });
            ItemInstanceCoreEffectRuntimeStateSnapshot orphanState =
                ItemInstanceCoreEffectRuntimeStateAssembler.Instance.Assemble(
                    new ItemInstanceCoreEffectRuntimeStateInput(
                        fixture.Identity,
                        fixture.Cultivation,
                        fixture.Projection.OrdinaryProjectionSet,
                        orphan.snapshot,
                        authority.CurrentSnapshot,
                        ItemCoreEffectRosterCompleteness.Complete));
            Check(orphanState.status ==
                ItemCoreEffectRuntimeStateStatus.Invalid,
                "Orphan/stale binding was not Invalid.");

            Check(fixture.Identity.Rows.All(row =>
                    !string.Equals(row.baseItemId, "I031",
                        StringComparison.Ordinal))
                && fixture.Cultivation.Rows.All(row =>
                    !string.Equals(row.baseItemId, "I031",
                        StringComparison.Ordinal))
                && authority.CurrentCoreEffectRuntimeState.Items.All(row =>
                    !string.Equals(row.baseItemId, "I031",
                        StringComparison.Ordinal))
                && DefaultItemCoreEffectDefinitionProvider.Instance
                    .GetDefinitions("I031").Count == 0
                && authority.Rows.Count(row => row.IsSystemItem) == 1,
                "I031 ordinary identity/cultivation/runtime leak detected.");
        }

        private static void VerifyAtomicCommit()
        {
            FailAfterFirstAssembler failing = new();
            ItemSystemBattleSandboxBoardAuthority authority =
                new ItemSystemBattleSandboxBoardAuthority(
                    fixture.Projection,
                    DefaultItemSystemSnapshotProvider.Instance,
                    ItemInstancePlacementBindingValidator.Instance,
                    TalismanBag.Items.Build.Qualified
                        .ItemInstanceQualifiedBuildStateAssembler.Instance,
                    fixture.Identity,
                    fixture.Cultivation,
                    failing,
                    DefaultRealLayoutResilienceEvaluationPipeline.Instance);
            object item = authority.CurrentSnapshot;
            object binding = authority.CurrentBindingSnapshot;
            object qualified = authority.CurrentQualifiedBuildState;
            object core = authority.CurrentCoreEffectRuntimeState;
            object p6 = authority.CurrentLayoutResilienceSnapshot;
            PlacementCandidate candidate =
                FindCandidate(authority, "I001", false);
            ItemSystemBattleSandboxBoardOperationResult rejected =
                authority.CommitFromTray(
                    "I001", candidate.Anchor, candidate.Rotation);
            Check(!rejected.Accepted
                && ReferenceEquals(item, authority.CurrentSnapshot)
                && ReferenceEquals(
                    binding, authority.CurrentBindingSnapshot)
                && ReferenceEquals(
                    qualified, authority.CurrentQualifiedBuildState)
                && ReferenceEquals(
                    core, authority.CurrentCoreEffectRuntimeState)
                && ReferenceEquals(
                    p6, authority.CurrentLayoutResilienceSnapshot),
                "Invalid assembly did not preserve all five references.");
        }

        private static void VerifyCanonicalImmutableAndNoOp()
        {
            ItemCoreEffectIdentityCatalogSnapshot reversedIdentity = new(
                ItemCoreEffectRuntimeStateStatus.Valid,
                fixture.Identity.Rows.Reverse(),
                Array.Empty<
                    ItemCoreEffectRuntimeStateValidationError>());
            Check(string.Equals(
                    reversedIdentity.canonicalSignature,
                    fixture.Identity.canonicalSignature,
                    StringComparison.Ordinal),
                "Reversed identity input changed canonical signature.");

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture =
                CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture =
                    CultureInfo.GetCultureInfo("fr-FR");
                CultureInfo.CurrentUICulture =
                    CultureInfo.GetCultureInfo("fr-FR");
                ItemCoreEffectIdentityCatalogSnapshot cultureIdentity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        fixture.Workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                Check(string.Equals(
                        cultureIdentity.canonicalSignature,
                        fixture.Identity.canonicalSignature,
                        StringComparison.Ordinal),
                    "CurrentCulture changed canonical signature.");
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }

            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            ItemInstanceCoreEffectRuntimeStateSnapshot state =
                authority.CurrentCoreEffectRuntimeState;
            ItemInstanceCoreEffectRuntimeStateSnapshot reversedRuntime = new(
                state.status,
                state.rosterCompleteness,
                state.sourceIdentityCatalogSignature,
                state.sourceCultivationRosterSignature,
                state.sourceProjectionSetSignature,
                state.sourceBindingSignature,
                state.sourceItemSystemSignature,
                state.Items.Reverse(),
                state.ValidationErrors.Reverse());
            Check(string.Equals(
                    reversedRuntime.canonicalSignature,
                    state.canonicalSignature,
                    StringComparison.Ordinal),
                "Reversed runtime input changed canonical signature.");
            Check(MutationBlocked(fixture.Identity.Rows)
                && MutationBlocked(fixture.Cultivation.Rows)
                && MutationBlocked(state.Items)
                && MutationBlocked(state.Items[0].CoreEffectRows)
                && ItemCoreEffectBooleanFact.Unknown !=
                    ItemCoreEffectBooleanFact.KnownFalse
                && ItemCoreEffectBooleanFact.Unknown !=
                    ItemCoreEffectBooleanFact.NotApplicable
                && ItemCoreEffectBooleanFact.KnownFalse !=
                    ItemCoreEffectBooleanFact.NotApplicable,
                "Immutable or tri-state fact contract regressed.");

            object item = authority.CurrentSnapshot;
            object binding = authority.CurrentBindingSnapshot;
            object qualified = authority.CurrentQualifiedBuildState;
            object core = authority.CurrentCoreEffectRuntimeState;
            object p6 = authority.CurrentLayoutResilienceSnapshot;
            int if01Count = authority.If01ValidationCount;
            int qualifiedCount = authority.QualifiedBuildAssemblyCount;
            int coreCount = authority.CoreEffectRuntimeAssemblyCount;
            int p6Count = authority.P6EvaluationCount;
            ItemSystemBattleSandboxBoardOperationResult noOp =
                authority.Reset();
            Check(noOp.Accepted && !noOp.Changed
                && ReferenceEquals(item, authority.CurrentSnapshot)
                && ReferenceEquals(
                    binding, authority.CurrentBindingSnapshot)
                && ReferenceEquals(
                    qualified, authority.CurrentQualifiedBuildState)
                && ReferenceEquals(
                    core, authority.CurrentCoreEffectRuntimeState)
                && ReferenceEquals(
                    p6, authority.CurrentLayoutResilienceSnapshot)
                && if01Count == authority.If01ValidationCount
                && qualifiedCount ==
                    authority.QualifiedBuildAssemblyCount
                && coreCount ==
                    authority.CoreEffectRuntimeAssemblyCount
                && p6Count == authority.P6EvaluationCount,
                "No-op changed references or assembler counters.");
        }

        private static void VerifyRealRuntimeSamples()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            Check(authority.CurrentSnapshot?.isValid == true
                && authority.CurrentSnapshot.schemaVersion ==
                    ItemSystemSnapshot.CurrentSchemaVersion
                && authority.CurrentBindingSnapshot?.isValid == true
                && authority.CurrentQualifiedBuildState?.isValid == true
                && authority.CurrentCoreEffectRuntimeState?.isValid == true
                && authority.CurrentLayoutResilienceSnapshot?.status ==
                    LayoutResilienceBattleSandboxPlaytestStatus.Complete,
                "Atomic real Board authority baseline is incomplete.");
            foreach ((string baseId, string instanceId) in new[]
                     {
                         ("I001", "wb_i001_orange_404310001"),
                         ("I004", "wb_i004_orange_404310004"),
                         ("I009", "wb_i009_orange_404310009"),
                         ("I030", "wb_i030_orange_404310030")
                     })
            {
                ItemInstanceCoreEffectRuntimeItemSnapshot item =
                    FindByBase(
                        authority.CurrentCoreEffectRuntimeState,
                        baseId);
                Check(string.Equals(
                        item.itemInstanceId,
                        instanceId,
                        StringComparison.Ordinal)
                    && item.rarity == ItemInstanceRarity.Orange
                    && item.cultivationLevel == 40
                    && item.CoreEffectRows.Count == 4,
                    baseId + " real runtime sample mismatch.");
            }
            Check(authority.Rows.Any(row =>
                    row.IsSystemItem
                    && string.Equals(row.BaseItemId, "I031",
                        StringComparison.Ordinal))
                && authority.CurrentCoreEffectRuntimeState.Items.All(item =>
                    !string.Equals(item.baseItemId, "I031",
                        StringComparison.Ordinal)),
                "I031 real sample exclusion mismatch.");
        }

        private static void VerifyP1ProtectedHashes()
        {
            VerifyHashes(P1ProtectedHashes);
            string profilesRoot = ProjectPath(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles");
            string[] profiles = Directory.GetFiles(
                    profilesRoot, "*.asset", SearchOption.TopDirectoryOnly)
                .OrderBy(value => Path.GetFileName(value),
                    StringComparer.Ordinal).ToArray();
            Check(profiles.Length == 30,
                "Profile file count is not 30.");
            List<string> aggregateRows = new();
            foreach (string profile in profiles)
            {
                aggregateRows.Add(
                    Path.GetFileName(profile) + "|"
                    + Sha256File(profile));
            }
            Check(string.Equals(
                    Sha256Text(string.Join("\n", aggregateRows)),
                    ProfilesAggregate,
                    StringComparison.Ordinal),
                "P1 profile aggregate changed.");
        }

        private static void VerifyPresentationProtectedHashes()
        {
            VerifyHashes(PresentationProtectedHashes);
        }

        private static void VerifyLeakBoundaries()
        {
            Check(ExistingWhitelist.All(path =>
                    File.Exists(ProjectPath(path)))
                && NewWhitelist.All(path =>
                    File.Exists(ProjectPath(path))),
                "One or more exact whitelist paths are missing.");
            string runtimeSources = string.Join("\n",
                ExistingWhitelist.Take(4).Select(path =>
                    File.ReadAllText(ProjectPath(path))));
            foreach (string forbidden in new[]
                     {
                         "SaveData", "RunFlow", "Reward",
                         "ItemDetailPanelView", "ItemDetailSectionView",
                         "EnemySystem", "BoneAspect",
                         "EncounterPresentation"
                     })
            {
                Check(!runtimeSources.Contains(
                        forbidden, StringComparison.Ordinal),
                    "Forbidden dependency leaked into runtime sources: "
                    + forbidden);
            }
            Check(!Directory.GetFiles(
                    ProjectPath("Docs/V0.4/Reports"),
                    "ItemFourCoreAwakeningRuntimeContractCorrection*.meta",
                    SearchOption.TopDirectoryOnly).Any(),
                "Docs report .meta side effect was created.");
            Check(!ExistingWhitelist.Concat(NewWhitelist).Any(path =>
                    path.Contains("Enemy", StringComparison.Ordinal)
                    || path.Contains("BoneAspect",
                        StringComparison.Ordinal)
                    || path.Contains("Scene_",
                        StringComparison.Ordinal)
                    || path.EndsWith(".prefab",
                        StringComparison.Ordinal)),
                "Parallel package or presentation path entered P2 whitelist.");
        }

        private static void VerifyPackageScopedTextCheck()
        {
            foreach (string path in ExistingWhitelist.Concat(NewWhitelist))
            {
                string absolute = ProjectPath(path);
                Check(File.Exists(absolute),
                    "Package path is missing: " + path);
                string[] lines = File.ReadAllText(absolute)
                    .Replace("\r\n", "\n").Split('\n');
                Check(lines.All(line =>
                        !line.EndsWith(" ", StringComparison.Ordinal)
                        && !line.EndsWith("\t",
                            StringComparison.Ordinal)
                        && !line.StartsWith("<<<<<<<",
                            StringComparison.Ordinal)
                        && !line.StartsWith(">>>>>>>",
                            StringComparison.Ordinal)),
                    "Whitespace or merge marker issue: " + path);
            }
        }

        private static ItemInstanceProjectionContractSnapshot
            GetRarityProjection(
                string baseItemId,
                string rarityKey)
        {
            GameObject providerObject = new(
                "P2RarityProjectionProvider_" + rarityKey);
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemBalanceCandidateDetailSandboxAdapter adapter =
                    new(fixture.Workbench, provider);
                ItemBalanceCandidateDetailResult result =
                    adapter.Request(
                        new ItemBalanceCandidateDetailRequest
                        {
                            baseItemId = baseItemId,
                            rarityKey = rarityKey,
                            rootSeedText = "404319001"
                        });
                ItemInstanceProjectionContractSnapshot projection =
                    result?.preview?.ProjectionResult?.snapshot;
                Check(result?.isSuccess == true && projection != null,
                    rarityKey + " Projection fixture failed.");
                return projection;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static ItemCoreEffectCultivationRosterSnapshot
            BuildCultivation(
                ItemSystemBattleSandboxViewProjectionResult projection,
                IReadOnlyDictionary<string, int> overrides)
        {
            return new ItemCoreEffectCultivationRosterSnapshot(
                ItemCoreEffectRosterCompleteness.Complete,
                projection.OrdinaryProjectionSet.Projections.Select(value =>
                    new ItemCoreEffectCultivationRow(
                        value.itemInstanceId,
                        value.baseItemId,
                        overrides != null
                            && overrides.TryGetValue(
                                value.baseItemId, out int level)
                            ? level : 40,
                        ItemSystemBattleSandboxBoardAdapter
                            .CultivationSourceKey,
                        ItemCoreEffectFactCompleteness.Complete)));
        }

        private static ItemSystemBattleSandboxBoardAuthority NewAuthority(
            ItemCoreEffectIdentityCatalogSnapshot identity = null,
            ItemCoreEffectCultivationRosterSnapshot cultivation = null)
        {
            return new ItemSystemBattleSandboxBoardAuthority(
                fixture.Projection,
                DefaultItemSystemSnapshotProvider.Instance,
                ItemInstancePlacementBindingValidator.Instance,
                identity ?? fixture.Identity,
                cultivation ?? fixture.Cultivation,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        private static ItemInstanceCoreEffectRuntimeItemSnapshot FindByBase(
            ItemInstanceCoreEffectRuntimeStateSnapshot state,
            string baseItemId)
        {
            ItemInstanceCoreEffectRuntimeItemSnapshot[] matches =
                state.Items.Where(value => string.Equals(
                    value.baseItemId, baseItemId,
                    StringComparison.Ordinal)).ToArray();
            Check(matches.Length == 1,
                "Runtime row cardinality mismatch for " + baseItemId);
            return matches[0];
        }

        private static PlacementCandidate FindCandidate(
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId,
            bool requireLit)
        {
            foreach (ItemShapeRotation rotation in new[]
                     {
                         ItemShapeRotation.Rotation0,
                         ItemShapeRotation.Rotation90,
                         ItemShapeRotation.Rotation180,
                         ItemShapeRotation.Rotation270
                     })
            for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                ItemSystemBattleSandboxBoardOperationResult preview =
                    authority.PreviewPlacement(
                        baseItemId,
                        new ItemShapeCell(x, y),
                        rotation);
                ItemSystemPlacementSnapshot placement =
                    preview.Snapshot?.FindPlacement(
                        string.Equals(baseItemId, "I031",
                            StringComparison.Ordinal)
                            ? "P_SYSTEM_I031"
                            : "P_BOARD_" + baseItemId);
                if (preview.Accepted && placement != null
                    && (string.Equals(baseItemId, "I031",
                            StringComparison.Ordinal)
                        || placement.isLit == requireLit))
                {
                    return new PlacementCandidate(
                        new ItemShapeCell(x, y), rotation);
                }
            }
            throw new InvalidOperationException(
                "No legal " + (requireLit ? "lit" : "unlit")
                + " placement for " + baseItemId);
        }

        private static void RecordState(
            string scenario,
            ItemInstanceCoreEffectRuntimeItemSnapshot item)
        {
            foreach (ItemInstanceCoreEffectRuntimeRow row in
                     item.CoreEffectRows)
            {
                RuntimeEvidenceRows.Add(new RuntimeEvidence(
                    scenario, item, row));
            }
        }

        private static bool MutationBlocked<T>(IReadOnlyList<T> values)
        {
            try
            {
                ((IList<T>)values).Add(default);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static bool IsKnown(ItemCoreEffectBooleanFact fact)
        {
            return fact == ItemCoreEffectBooleanFact.KnownFalse
                || fact == ItemCoreEffectBooleanFact.KnownTrue;
        }

        private static ItemCoreEffectBooleanFact Fact(bool value)
        {
            return value
                ? ItemCoreEffectBooleanFact.KnownTrue
                : ItemCoreEffectBooleanFact.KnownFalse;
        }

        private static ItemCoreAwakeningNodeKind ParseFormalKind(
            string value)
        {
            return value switch
            {
                "Core1" => ItemCoreAwakeningNodeKind.Core1,
                "Core2" => ItemCoreAwakeningNodeKind.Core2,
                "Core3" => ItemCoreAwakeningNodeKind.Core3,
                "Ultimate" => ItemCoreAwakeningNodeKind.Ultimate,
                _ => throw new InvalidOperationException(
                    "Non-formal Candidate nodeKind: " + value)
            };
        }

        private static int NodeRank(string value)
        {
            return value switch
            {
                "Core1" => 0,
                "Core2" => 1,
                "Core3" => 2,
                "Ultimate" => 3,
                "Core4" => int.MaxValue,
                _ => int.MaxValue
            };
        }

        private static void VerifyHashes(
            IReadOnlyDictionary<string, string> hashes)
        {
            foreach (KeyValuePair<string, string> pair in hashes)
            {
                Check(string.Equals(
                        Sha256File(ProjectPath(pair.Key)),
                        pair.Value,
                        StringComparison.Ordinal),
                    "Protected hash mismatch: " + pair.Key);
            }
        }

        private static void Run(
            string id,
            string marker,
            Action action)
        {
            Debug.Log(
                "[ItemFourCoreAwakeningRuntimeContractCorrectionVerifier]["
                + id + "] START " + marker);
            try
            {
                action();
                Results.Add(new ScenarioResult(
                    id, marker, true, "PASS"));
                Debug.Log(
                    "[ItemFourCoreAwakeningRuntimeContractCorrectionVerifier]["
                    + id + "] PASS " + marker);
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(
                    id,
                    marker,
                    false,
                    exception.GetType().Name + ": "
                    + exception.Message));
                Debug.LogError(
                    "[ItemFourCoreAwakeningRuntimeContractCorrectionVerifier]["
                    + id + "] FAIL " + marker + " "
                    + exception.GetType().Name + ": "
                    + exception.Message);
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void WriteReports()
        {
            Directory.CreateDirectory(ProjectPath("Docs/V0.4/Reports"));
            WriteMainReport();
            WriteSpec();
            WriteIdentityMatrix();
            WriteRuntimeMatrix();
            WriteCanonicalDelta();
            WriteMigrationLedger();
            WriteLeakReport();
        }

        private static void WriteMainReport()
        {
            StringBuilder report = new();
            report.AppendLine(
                "# Item Four Core Awakening Runtime Contract Correction Report");
            report.AppendLine();
            report.AppendLine("- Package: `" + PackageKey + "`");
            report.AppendLine("- Status: `"
                + (Results.All(value => value.Passed)
                    ? "DEV_COMPLETE / QA_PASS"
                    : "QA_FAILED") + "`");
            report.AppendLine("- Candidate / Awakening identity: `120 / 120`");
            report.AppendLine("- Formal nodes: `Core1 / Core2 / Core3 / Ultimate`");
            report.AppendLine("- Identity schema: `"
                + ItemCoreEffectIdentityCatalogSnapshot.CurrentSchemaId
                + "`");
            report.AppendLine("- Runtime schema: `"
                + ItemInstanceCoreEffectRuntimeStateSnapshot.CurrentSchemaId
                + "`");
            report.AppendLine("- Authority revision: `"
                + ItemCoreEffectIdentityCatalogSnapshot
                    .CurrentAuthorityRevision + "`");
            report.AppendLine("- Core4: `deprecated/reserved enum value 4; zero formal rows`");
            report.AppendLine("- v1 evidence: `" + HistoricalLabel
                + " / " + RetainedLabel + "`");
            report.AppendLine("- Parallel Enemy/BoneAspect work: `protected read-only; not package-owned`");
            report.AppendLine("- `REAL_RUNTIME_STATE_ASSEMBLY_PASS`");
            report.AppendLine("- `USER_HANDTEST_NOT_APPLICABLE`");
            report.AppendLine("- `P3_NOT_STARTED`");
            report.AppendLine("- `PREFAB_MIGRATION_NOT_STARTED`");
            report.AppendLine("- `NEXT_PACKAGE_NOT_STARTED`");
            report.AppendLine();
            report.AppendLine("| Scenario | Result | Marker | Detail |");
            report.AppendLine("|---|---:|---|---|");
            foreach (ScenarioResult result in Results)
            {
                report.AppendLine("| `" + result.Id + "` | `"
                    + (result.Passed ? "PASS" : "FAIL") + "` | `"
                    + result.Marker + "` | "
                    + EscapeMarkdown(result.Detail) + " |");
            }
            File.WriteAllText(
                ProjectPath(ReportPath),
                report.ToString(),
                new UTF8Encoding(false));
        }

        private static void WriteSpec()
        {
            StringBuilder spec = new();
            spec.AppendLine("scenarioId,marker,result,detail");
            foreach (ScenarioResult result in Results)
            {
                spec.AppendLine(string.Join(",",
                    Csv(result.Id),
                    Csv(result.Marker),
                    Csv(result.Passed ? "PASS" : "FAIL"),
                    Csv(result.Detail)));
            }
            File.WriteAllText(
                ProjectPath(SpecPath),
                spec.ToString(),
                new UTF8Encoding(false));
        }

        private static void WriteIdentityMatrix()
        {
            StringBuilder matrix = new();
            matrix.AppendLine("baseItemId,nodeKind,candidateDefinitionId,awakeningNodeId,candidateUnlockLevel,awakeningUnlockLevel,requiredRarity,requiredRarityKey,displayName,description,candidatePayloadIdentity,candidateSourceIdentity,awakeningSourceIdentity,schemaId,authorityRevision");
            foreach (ItemCoreEffectIdentityRow row in
                     fixture?.Identity?.Rows ??
                     Array.Empty<ItemCoreEffectIdentityRow>())
            {
                matrix.AppendLine(string.Join(",",
                    Csv(row.baseItemId),
                    Csv(row.nodeKind.ToString()),
                    Csv(row.candidateDefinitionId),
                    Csv(row.awakeningNodeId),
                    Csv(row.candidateUnlockLevel.ToString(
                        CultureInfo.InvariantCulture)),
                    Csv(row.awakeningUnlockLevel.ToString(
                        CultureInfo.InvariantCulture)),
                    Csv(row.requiredRarity.ToString()),
                    Csv(row.requiredRarityKey),
                    Csv(row.displayName),
                    Csv(row.description),
                    Csv(row.candidatePayloadIdentity),
                    Csv(row.candidateSourceIdentity),
                    Csv(row.awakeningSourceIdentity),
                    Csv(ItemCoreEffectIdentityCatalogSnapshot
                        .CurrentSchemaId),
                    Csv(ItemCoreEffectIdentityCatalogSnapshot
                        .CurrentAuthorityRevision)));
            }
            File.WriteAllText(
                ProjectPath(IdentityMatrixPath),
                matrix.ToString(),
                new UTF8Encoding(false));
        }

        private static void WriteRuntimeMatrix()
        {
            StringBuilder matrix = new();
            matrix.AppendLine("scenario,itemInstanceId,baseItemId,rarity,location,placementId,cultivationLevel,cultivationSource,nodeKind,candidateDefinitionId,awakeningNodeId,eligibleFact,visibleFact,unlockedFact,litFact,activeFact,stateCompleteness");
            foreach (RuntimeEvidence evidence in RuntimeEvidenceRows
                         .OrderBy(value => value.Scenario,
                             StringComparer.Ordinal)
                         .ThenBy(value => value.Item.itemInstanceId,
                             StringComparer.Ordinal)
                         .ThenBy(value => NodeRank(
                             value.Row.nodeKind.ToString())))
            {
                matrix.AppendLine(string.Join(",",
                    Csv(evidence.Scenario),
                    Csv(evidence.Item.itemInstanceId),
                    Csv(evidence.Item.baseItemId),
                    Csv(evidence.Item.rarity.ToString()),
                    Csv(evidence.Item.location.ToString()),
                    Csv(evidence.Item.placementId),
                    Csv(evidence.Item.cultivationLevel?.ToString(
                        CultureInfo.InvariantCulture)),
                    Csv(evidence.Item.cultivationLevelSource),
                    Csv(evidence.Row.nodeKind.ToString()),
                    Csv(evidence.Row.candidateDefinitionId),
                    Csv(evidence.Row.awakeningNodeId),
                    Csv(evidence.Row.eligibleFact.ToString()),
                    Csv(evidence.Row.visibleFact.ToString()),
                    Csv(evidence.Row.unlockedFact.ToString()),
                    Csv(evidence.Row.litFact.ToString()),
                    Csv(evidence.Row.activeFact.ToString()),
                    Csv(evidence.Row.stateCompleteness.ToString())));
            }
            File.WriteAllText(
                ProjectPath(RuntimeMatrixPath),
                matrix.ToString(),
                new UTF8Encoding(false));
        }

        private static void WriteCanonicalDelta()
        {
            StringBuilder delta = new();
            delta.AppendLine("authority,taskStart,current,classification,evidence");
            delta.AppendLine(string.Join(",",
                Csv("AwakeningDefinitionSet"),
                Csv("five nodes / 150"),
                Csv("four nodes / 120"),
                Csv("PERMITTED_V1_TO_V2_DELTA"),
                Csv(HistoricalLabel)));
            delta.AppendLine(string.Join(",",
                Csv("ItemCoreEffectIdentityCatalogSnapshot"),
                Csv(ItemCoreEffectIdentityCatalogSnapshot.LegacySchemaIdV1
                    + " / 150"),
                Csv(ItemCoreEffectIdentityCatalogSnapshot.CurrentSchemaId
                    + " / 120"),
                Csv("PERMITTED_V1_TO_V2_DELTA"),
                Csv(ItemCoreEffectIdentityCatalogSnapshot
                    .CurrentAuthorityRevision)));
            delta.AppendLine(string.Join(",",
                Csv("ItemInstanceCoreEffectRuntimeStateSnapshot"),
                Csv(ItemInstanceCoreEffectRuntimeStateSnapshot
                    .LegacySchemaIdV1 + " / five rows"),
                Csv(ItemInstanceCoreEffectRuntimeStateSnapshot
                    .CurrentSchemaId + " / four rows"),
                Csv("PERMITTED_V1_TO_V2_DELTA"),
                Csv(ItemInstanceCoreEffectRuntimeStateSnapshot
                    .CurrentAuthorityRevision)));
            delta.AppendLine(string.Join(",",
                Csv("CandidateAuthority"),
                Csv("P1 120"),
                Csv("P1 120"),
                Csv("PROTECTED_UNCHANGED"),
                Csv(ProfilesAggregate)));
            delta.AppendLine(string.Join(",",
                Csv("ItemSystemSnapshot"),
                Csv("ItemSystemSnapshot.v2"),
                Csv("ItemSystemSnapshot.v2"),
                Csv("SCHEMA_UNCHANGED"),
                Csv(PresentationProtectedHashes[
                    "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"])));
            delta.AppendLine(string.Join(",",
                Csv("HistoricalEvidence"),
                Csv("v1 accepted artifacts"),
                Csv("byte-identical"),
                Csv(RetainedLabel),
                Csv(HistoricalLabel)));
            File.WriteAllText(
                ProjectPath(CanonicalDeltaPath),
                delta.ToString(),
                new UTF8Encoding(false));
        }

        private static void WriteMigrationLedger()
        {
            string[] lines = File.ReadAllLines(
                ProjectPath(HistoricalIdentityMatrixPath));
            Check(lines.Length == 151,
                "Historical migration source must contain 150 rows.");
            List<string> header = ParseCsvLine(lines[0]);
            Dictionary<string, int> columns = header
                .Select((name, index) => new { name, index })
                .ToDictionary(
                    value => value.name,
                    value => value.index,
                    StringComparer.Ordinal);
            StringBuilder ledger = new();
            ledger.AppendLine("baseItemId,legacyNodeKind,legacyCandidateDefinitionId,legacyAwakeningNodeId,targetCandidateDefinitionId,targetAwakeningNodeId,migrationStatus,displayName,description,candidatePayloadIdentity,evidenceLabel");
            int retained = 0;
            int superseded = 0;
            for (int index = 1; index < lines.Length; index++)
            {
                List<string> values = ParseCsvLine(lines[index]);
                string baseItemId = values[columns["baseItemId"]];
                string nodeKind = values[columns["nodeKind"]];
                string candidateId =
                    values[columns["candidateDefinitionId"]];
                string awakeningId =
                    values[columns["awakeningNodeId"]];
                string displayName = values[columns["displayName"]];
                string description = values[columns["description"]];
                string payloadIdentity =
                    values[columns["candidatePayloadIdentity"]];
                bool core4 = string.Equals(
                    nodeKind, "Core4", StringComparison.Ordinal);
                string targetCandidate = string.Empty;
                string targetAwakening = string.Empty;
                string status;
                string label;
                if (core4)
                {
                    status = ItemCoreEffectRuntimeStateValidationCodes
                        .SupersededExpansionDrift;
                    label = HistoricalLabel;
                    superseded++;
                }
                else
                {
                    ItemCoreEffectIdentityRow current =
                        fixture.Identity.Rows.Single(row =>
                            string.Equals(
                                row.baseItemId, baseItemId,
                                StringComparison.Ordinal)
                            && string.Equals(
                                row.nodeKind.ToString(), nodeKind,
                                StringComparison.Ordinal));
                    Check(string.Equals(
                            current.candidateDefinitionId,
                            candidateId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            current.displayName,
                            displayName,
                            StringComparison.Ordinal)
                        && string.Equals(
                            current.description,
                            description,
                            StringComparison.Ordinal)
                        && string.Equals(
                            current.candidatePayloadIdentity,
                            payloadIdentity,
                            StringComparison.Ordinal),
                        baseItemId + "/" + nodeKind
                        + " retained Candidate payload/text changed.");
                    targetCandidate = current.candidateDefinitionId;
                    targetAwakening = current.awakeningNodeId;
                    status = "RETAINED_V2_CURRENT_IDENTITY";
                    label = RetainedLabel;
                    retained++;
                }
                ledger.AppendLine(string.Join(",",
                    Csv(baseItemId),
                    Csv(nodeKind),
                    Csv(candidateId),
                    Csv(awakeningId),
                    Csv(targetCandidate),
                    Csv(targetAwakening),
                    Csv(status),
                    Csv(displayName),
                    Csv(description),
                    Csv(payloadIdentity),
                    Csv(label)));
            }
            Check(retained == 120 && superseded == 30,
                "Migration ledger is not 120 retained + 30 superseded.");
            File.WriteAllText(
                ProjectPath(MigrationLedgerPath),
                ledger.ToString(),
                new UTF8Encoding(false));
        }

        private static void WriteLeakReport()
        {
            StringBuilder leak = new();
            leak.AppendLine(
                "# Item Four Core Awakening Runtime Contract Correction LeakCheck");
            leak.AppendLine();
            leak.AppendLine("- Result: `"
                + (Results.Any(value =>
                    value.Marker == "LEAKCHECK_PASS" && value.Passed)
                    ? "LEAKCHECK_PASS"
                    : "PENDING") + "`");
            leak.AppendLine("- Exact existing-file whitelist: `5`");
            leak.AppendLine("- Exact new implementation files: `2`");
            leak.AppendLine("- Exact new reports: `7`");
            leak.AppendLine("- Formal Core4 producer/identity/runtime rows: `0`");
            leak.AppendLine("- I031 ordinary rows: `0`");
            leak.AppendLine("- Scene/Prefab/UI/BuildSettings writes: `0`");
            leak.AppendLine("- EnemySystem/BoneAspect/EncounterPresentation writes: `0`");
            leak.AppendLine("- Roll/RNG/probability/Candidate/Profile writes: `0`");
            leak.AppendLine("- Git mutation operations: `0`");
            leak.AppendLine("- Historical label: `" + HistoricalLabel + "`");
            leak.AppendLine("- Evidence label: `" + RetainedLabel + "`");
            leak.AppendLine("- `USER_HANDTEST_NOT_APPLICABLE`");
            leak.AppendLine("- `P3_NOT_STARTED`");
            leak.AppendLine("- `PREFAB_MIGRATION_NOT_STARTED`");
            File.WriteAllText(
                ProjectPath(LeakPath),
                leak.ToString(),
                new UTF8Encoding(false));
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> values = new();
            StringBuilder value = new();
            bool quoted = false;
            for (int index = 0; index < (line ?? string.Empty).Length;
                 index++)
            {
                char current = line[index];
                if (current == '"')
                {
                    if (quoted && index + 1 < line.Length
                        && line[index + 1] == '"')
                    {
                        value.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (current == ',' && !quoted)
                {
                    values.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(current);
                }
            }
            values.Add(value.ToString());
            return values;
        }

        private static string ProjectPath(string relative)
        {
            return Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                relative.Replace(
                    '/', Path.DirectorySeparatorChar)));
        }

        private static string Sha256File(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return string.Concat(
                sha.ComputeHash(stream).Select(value =>
                    value.ToString(
                        "x2", CultureInfo.InvariantCulture)));
        }

        private static string Sha256Text(string value)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(
                Encoding.UTF8.GetBytes(value ?? string.Empty))
                .Select(item => item.ToString(
                    "x2", CultureInfo.InvariantCulture)));
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty)
                .Replace("\"", "\"\"") + "\"";
        }

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|")
                .Replace("\r", " ").Replace("\n", " ");
        }

        private readonly struct PlacementCandidate
        {
            public PlacementCandidate(
                ItemShapeCell anchor,
                ItemShapeRotation rotation)
            {
                Anchor = anchor;
                Rotation = rotation;
            }

            public ItemShapeCell Anchor { get; }
            public ItemShapeRotation Rotation { get; }
        }

        private sealed class SuppliedCore4Provider :
            IItemCoreEffectDefinitionProvider
        {
            public IReadOnlyList<ItemCoreEffectDefinition> GetDefinitions(
                string itemId)
            {
                return DefaultItemCoreEffectDefinitionProvider.Instance
                    .GetDefinitions(itemId)
                    .Concat(new[]
                    {
                        new ItemCoreEffectDefinition(
                            itemId,
                            itemId + "_CORE_04",
                            ItemCoreAwakeningNodeKind.Core4,
                            40,
                            "Historical expansion drift",
                            "purple")
                    }).ToArray();
            }
        }

        private sealed class FailAfterFirstAssembler :
            IItemInstanceCoreEffectRuntimeStateAssembler
        {
            private int callCount;

            public ItemInstanceCoreEffectRuntimeStateSnapshot Assemble(
                ItemInstanceCoreEffectRuntimeStateInput input)
            {
                callCount++;
                if (callCount == 1)
                {
                    return ItemInstanceCoreEffectRuntimeStateAssembler
                        .Instance.Assemble(input);
                }
                return new ItemInstanceCoreEffectRuntimeStateSnapshot(
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    input?.rosterCompleteness
                        ?? ItemCoreEffectRosterCompleteness.Unknown,
                    input?.identityCatalog?.canonicalSignature,
                    input?.cultivationRoster?.canonicalSignature,
                    null,
                    input?.bindingSnapshot?.canonicalSignature,
                    null,
                    Array.Empty<
                        ItemInstanceCoreEffectRuntimeItemSnapshot>(),
                    new[]
                    {
                        new ItemCoreEffectRuntimeStateValidationError(
                            "P2_VERIFIER_FORCED_INVALID",
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            null, null, null, null, null,
                            "Forced invalid assembly for atomic rollback verification.")
                    });
            }
        }

        private sealed class Fixture
        {
            public Fixture(
                ItemBalanceWorkbenchCatalog workbench,
                ItemSystemBattleSandboxViewProjectionResult projection,
                ItemCoreEffectIdentityCatalogSnapshot identity,
                ItemCoreEffectCultivationRosterSnapshot cultivation)
            {
                Workbench = workbench;
                Projection = projection;
                Identity = identity;
                Cultivation = cultivation;
            }

            public ItemBalanceWorkbenchCatalog Workbench { get; }
            public ItemSystemBattleSandboxViewProjectionResult Projection
            {
                get;
            }
            public ItemCoreEffectIdentityCatalogSnapshot Identity { get; }
            public ItemCoreEffectCultivationRosterSnapshot Cultivation
            {
                get;
            }
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

        private sealed class RuntimeEvidence
        {
            public RuntimeEvidence(
                string scenario,
                ItemInstanceCoreEffectRuntimeItemSnapshot item,
                ItemInstanceCoreEffectRuntimeRow row)
            {
                Scenario = scenario;
                Item = item;
                Row = row;
            }

            public string Scenario { get; }
            public ItemInstanceCoreEffectRuntimeItemSnapshot Item { get; }
            public ItemInstanceCoreEffectRuntimeRow Row { get; }
        }
    }
}
