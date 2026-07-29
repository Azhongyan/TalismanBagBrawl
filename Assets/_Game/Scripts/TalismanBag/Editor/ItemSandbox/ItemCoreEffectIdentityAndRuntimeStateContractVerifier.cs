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
        ItemCoreEffectIdentityAndRuntimeStateContractVerifier
    {
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractSpec.csv";
        private const string MatrixPath =
            "Docs/V0.4/Reports/ItemCoreEffectIdentityMatrix.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractLeakCheckReport.md";

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs"] = "ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs"] = "1c1504948012b94dcbb1cc91310becd2df169bb736003737c5109f19cd72cc06",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs"] = "294e73d516189f541d0e1e92e0efad583a634980d4b4b10ad0d2117075cf224b",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"] = "c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceProfile.cs"] = "fd2b3700b0a500fd7f1450c5e1acff5a90617f657251bd49136cf712005fb91d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchCatalog.cs"] = "8ac332b9905f912e8c6aaa9facb116fb49f4ab0523f0bad90a350682301d3428",
                ["Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset"] = "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45",
                ["Docs/V0.4/Reports/ItemCandidateCoreEffects150.csv"] = "5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs"] = "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967",
                ["Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs"] = "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] = "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] = "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"] = "798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs"] = "95341ddbc3572a1d116287ac4edb221698d56b1b7133768daa45fbee02c0179d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs"] = "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs"] = "ced80cfa99b330ca3f5d67fc22198641de817105608b678603c33343ab555e94",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] = "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs"] = "957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] = "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity"] = "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] = "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                ["Docs/V0.4/ItemCoreAwakeningCore4NodeExpansion01_Assignment.md"] = "8f9af4b60b601f64a59fe8741c95792d8222857b66403843b70340ed46e7a03f"
            };

        private static readonly List<ScenarioResult> Results = new();
        private static Fixture fixture;

        [MenuItem("Talisman Bag/V0.4/Verify Item Core Effect Identity And Runtime State Contract")]
        public static void VerifyOffline()
        {
            VerifyStaticBatch();
        }

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            fixture = BuildFixture();
            Run("S01", "COMPONENT_FIXTURE_PASS", VerifyComponentFixture);
            Run("S02", "IDENTITY_MAPPING_150_OF_150_PASS",
                VerifyIdentityMapping);
            Run("S03", "CORE4_ULTIMATE_SEPARATION_PASS",
                VerifyCore4UltimateSeparation);
            Run("S04", "REAL_RUNTIME_STATE_ASSEMBLY_PASS",
                VerifyRealRuntimeBaseline);
            Run("S05", "EXPLICIT_DEV_CULTIVATION_SOURCE_PASS",
                VerifyExplicitCultivation);
            Run("S06", "MISSING_CULTIVATION_REMAINS_UNKNOWN_PASS",
                VerifyUnknownCultivation);
            Run("S07", "INVENTORY_BOARD_TRANSITIONS_PASS",
                VerifyTransitions);
            Run("S08", "I031_EXCLUSION_PASS", VerifyI031Exclusion);
            Run("S09", "ATOMIC_COMMIT_PASS", VerifyAtomicCommit);
            Run("S10", "NO_OP_STABILITY_PASS", VerifyNoOp);
            Run("S11", "Canonical/Ordinal/immutable PASS",
                VerifyCanonicalAndImmutable);
            Run("S12", "Invalid contradictions PASS",
                VerifyInvalidCultivation);
            Run("S13", "White/Green/Blue/Purple/Orange Projection facts PASS",
                VerifyRarityProjectionFacts);
            Run("S14", "Real sample identities PASS", VerifyRealSamples);
            Run("S14A", "DEPENDENCY_REGRESSION_PASS",
                VerifyDependencyRegressions);
            Run("S15", "PROTECTED_HASHES_PASS", VerifyProtectedHashes);
            Run("S16", "LEAKCHECK_PASS", VerifyLeakBoundaries);
            WriteReports();
            Run("S17", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS",
                VerifyPackageDiffCheck);
            WriteReports();
            ScenarioResult[] failures = Results.Where(value => !value.Passed)
                .ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "ItemCoreEffect identity/runtime verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }
            UnityEngine.Debug.Log(
                "[ItemCoreEffectIdentityAndRuntimeStateContractVerifier]\n"
                + string.Join("\n", Results.Select(value => value.Marker))
                + "\nUSER_HANDTEST_NOT_APPLICABLE"
                + "\nNEXT_PACKAGE_NOT_STARTED");
        }

        private static Fixture BuildFixture()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            Check(workbench != null, "Workbench catalog is missing.");
            GameObject providerObject = new(
                "CoreEffectRuntimeVerifierProvider");
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
                return new Fixture(workbench, projection, identity,
                    cultivation);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static void VerifyComponentFixture()
        {
            Check(ItemCoreEffectIdentityCatalogSnapshot.CurrentSchemaId ==
                "ItemCoreEffectIdentityCatalogSnapshot.v1",
                "Identity schema mismatch.");
            Check(ItemCoreEffectCultivationRosterSnapshot.CurrentSchemaId ==
                "ItemCoreEffectCultivationRosterSnapshot.v1",
                "Cultivation schema mismatch.");
            Check(
                ItemInstanceCoreEffectRuntimeStateSnapshot.CurrentSchemaId ==
                "ItemInstanceCoreEffectRuntimeStateSnapshot.v1",
                "Runtime schema mismatch.");
            Check(typeof(IItemInstanceCoreEffectRuntimeStateAssembler)
                    .GetMethod("Assemble") != null,
                "Stateless assembler seam is missing.");
            foreach ((int level, int unlocked) in new[]
                     {
                         (1, 0), (10, 1), (20, 2), (30, 3), (40, 5)
                     })
            {
                ItemCoreEffectCultivationRosterSnapshot roster =
                    BuildCultivation(fixture.Projection,
                        new Dictionary<string, int>(StringComparer.Ordinal)
                        {
                            ["I001"] = level
                        });
                ItemSystemBattleSandboxBoardAuthority authority =
                    NewAuthority(fixture.Identity, roster);
                ItemInstanceCoreEffectRuntimeItemSnapshot item =
                    FindByBase(authority.CurrentCoreEffectRuntimeState,
                        "I001");
                Check(item.location ==
                    ItemCoreEffectRuntimeLocation.Inventory,
                    "Level fixture did not remain Inventory.");
                Check(item.CoreEffectRows.Count(row =>
                        row.unlockedFact ==
                        ItemCoreEffectBooleanFact.KnownTrue) == unlocked,
                    "Resolver-backed unlock count mismatch at Lv." + level);
                Check(item.CoreEffectRows.All(row =>
                        row.litFact ==
                            ItemCoreEffectBooleanFact.NotApplicable
                        && row.activeFact ==
                            ItemCoreEffectBooleanFact.NotApplicable),
                    "Inventory runtime facts must be NotApplicable.");
            }
        }

        private static void VerifyIdentityMapping()
        {
            Check(fixture.Identity.isValid
                && fixture.Identity.Rows.Count == 150,
                "Identity catalog is not Valid 150/150.");
            string[] expected = Enumerable.Range(1, 30)
                .Select(index => "I" + index.ToString(
                    "000", CultureInfo.InvariantCulture)).ToArray();
            Check(fixture.Identity.Rows.Select(value => value.baseItemId)
                    .Distinct(StringComparer.Ordinal)
                    .SequenceEqual(expected, StringComparer.Ordinal),
                "Identity roster is not exact I001-I030.");
            foreach (string itemId in expected)
            {
                ItemCoreEffectIdentityRow[] rows = fixture.Identity.Rows
                    .Where(value => string.Equals(value.baseItemId, itemId,
                        StringComparison.Ordinal)).ToArray();
                Check(rows.Length == 5, itemId + " does not have five rows.");
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
                            StringComparison.Ordinal),
                        itemId + " explicit mapping mismatch for "
                        + row.nodeKind);
                    Check(!string.IsNullOrWhiteSpace(row.displayName)
                        && !string.IsNullOrWhiteSpace(row.description)
                        && !string.IsNullOrWhiteSpace(
                            row.candidatePayloadIdentity),
                        itemId + " static Candidate evidence missing.");
                }
            }
        }

        private static void VerifyCore4UltimateSeparation()
        {
            foreach (string itemId in fixture.Identity.Rows
                         .Select(value => value.baseItemId)
                         .Distinct(StringComparer.Ordinal))
            {
                ItemCoreEffectIdentityRow core4 = fixture.Identity.Find(
                    itemId, ItemCoreAwakeningNodeKind.Core4);
                ItemCoreEffectIdentityRow ultimate = fixture.Identity.Find(
                    itemId, ItemCoreAwakeningNodeKind.Ultimate);
                Check(core4 != null && ultimate != null
                    && core4.awakeningNodeId.EndsWith(
                        "_CORE_04", StringComparison.Ordinal)
                    && ultimate.awakeningNodeId.EndsWith(
                        "_CORE_ULT", StringComparison.Ordinal)
                    && !string.Equals(core4.awakeningNodeId,
                        ultimate.awakeningNodeId, StringComparison.Ordinal)
                    && core4.candidateEffectOperation == "ExtraTrigger"
                    && ultimate.candidateEffectCategory ==
                        "MechanicConversion"
                    && ultimate.candidateEffectOperation == "Convert",
                    itemId + " Core4/Ultimate separation mismatch.");
            }
        }

        private static void VerifyRealRuntimeBaseline()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            ItemInstanceCoreEffectRuntimeStateSnapshot state =
                authority.CurrentCoreEffectRuntimeState;
            Check(state.isValid && state.Items.Count == 30,
                "Real authority did not assemble 30 Valid items.");
            Check(state.Items.All(item =>
                    item.location == ItemCoreEffectRuntimeLocation.Inventory
                    && item.CoreEffectRows.Count == 5
                    && item.CoreEffectRows.All(row =>
                        row.eligibleFact ==
                            ItemCoreEffectBooleanFact.KnownTrue
                        && row.visibleFact ==
                            ItemCoreEffectBooleanFact.KnownTrue
                        && row.litFact ==
                            ItemCoreEffectBooleanFact.NotApplicable
                        && row.activeFact ==
                            ItemCoreEffectBooleanFact.NotApplicable)),
                "Inventory static/runtime facts are not preserved.");
        }

        private static void VerifyExplicitCultivation()
        {
            Check(fixture.Cultivation.isValid
                && fixture.Cultivation.Rows.Count == 30
                && fixture.Cultivation.Rows.All(row =>
                    row.inputLevel == 40
                    && row.factCompleteness ==
                        ItemCoreEffectFactCompleteness.Complete
                    && string.Equals(row.sourceKey,
                        ItemSystemBattleSandboxBoardAdapter
                            .CultivationSourceKey,
                        StringComparison.Ordinal)),
                "BattleSandbox explicit Lv40 roster mismatch.");
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            Check(authority.CurrentSnapshot.placements.All(placement =>
                    string.Equals(placement.itemId, "I031",
                        StringComparison.Ordinal)
                    || placement.inputLevel == 40),
                "Accepted placement silently used Lv1.");
        }

        private static void VerifyUnknownCultivation()
        {
            ItemSystemBattleSandboxBoardAuthority legacy =
                new ItemSystemBattleSandboxBoardAuthority(
                    fixture.Projection,
                    DefaultItemSystemSnapshotProvider.Instance,
                    ItemInstancePlacementBindingValidator.Instance,
                    DefaultRealLayoutResilienceEvaluationPipeline.Instance);
            ItemInstanceCoreEffectRuntimeStateSnapshot state =
                legacy.CurrentCoreEffectRuntimeState;
            Check(state.status ==
                ItemCoreEffectRuntimeStateStatus.Unknown,
                "Missing cultivation did not remain Unknown.");
            Check(state.Items.All(item =>
                    !item.cultivationLevel.HasValue
                    && item.cultivationLevelSource == null
                    && item.CoreEffectRows.All(row =>
                        row.unlockedFact ==
                            ItemCoreEffectBooleanFact.Unknown
                        && row.activeFact ==
                            ItemCoreEffectBooleanFact.Unknown)),
                "Unknown cultivation leaked Lv1 or false facts.");
        }

        private static void VerifyTransitions()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            PlacementCandidate unlit = FindCandidate(
                authority, "I001", false);
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
                "Board unlit facts or static text mismatch.");

            Check(authority.CommitFromTray(
                    "I031", new ItemShapeCell(1, 1),
                    ItemShapeRotation.Rotation0).Accepted,
                "I031 lighting source commit failed.");
            PlacementCandidate lit = FindCandidate(
                authority, "I001", true);
            Check(authority.CommitMove(
                    "P_BOARD_I001", lit.Anchor, lit.Rotation).Accepted,
                "I001 move-to-lit commit failed.");
            ItemInstanceCoreEffectRuntimeItemSnapshot boardLit =
                FindByBase(authority.CurrentCoreEffectRuntimeState, "I001");
            Check(boardLit.isLitFact ==
                    ItemCoreEffectBooleanFact.KnownTrue
                && boardLit.CoreEffectRows.All(row =>
                    row.litFact ==
                        ItemCoreEffectBooleanFact.KnownTrue
                    && row.activeFact ==
                        ItemCoreEffectBooleanFact.KnownTrue),
                "Board lit/active facts mismatch.");

            Check(authority.ReturnToTray(
                    "P_BOARD_I001").Accepted,
                "I001 return failed.");
            ItemInstanceCoreEffectRuntimeItemSnapshot inventory =
                FindByBase(authority.CurrentCoreEffectRuntimeState, "I001");
            Check(inventory.location ==
                    ItemCoreEffectRuntimeLocation.Inventory
                && inventory.placementId == null
                && inventory.isLitFact ==
                    ItemCoreEffectBooleanFact.NotApplicable
                && inventory.CoreEffectRows.All(row =>
                    row.litFact ==
                        ItemCoreEffectBooleanFact.NotApplicable
                    && row.activeFact ==
                        ItemCoreEffectBooleanFact.NotApplicable),
                "Return-to-Inventory retained stale Board facts.");
        }

        private static void VerifyI031Exclusion()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            Check(fixture.Identity.Rows.All(row =>
                    !string.Equals(row.baseItemId, "I031",
                        StringComparison.Ordinal))
                && fixture.Cultivation.Rows.All(row =>
                    !string.Equals(row.baseItemId, "I031",
                        StringComparison.Ordinal))
                && authority.CurrentCoreEffectRuntimeState.Items.All(row =>
                    !string.Equals(row.baseItemId, "I031",
                        StringComparison.Ordinal))
                && authority.Rows.Count(row => row.IsSystemItem) == 1,
                "I031 ordinary identity/runtime leak detected.");
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
            ItemSystemSnapshot beforeItem = authority.CurrentSnapshot;
            ItemInstancePlacementBindingContractSnapshot beforeBinding =
                authority.CurrentBindingSnapshot;
            object beforeQualified = authority.CurrentQualifiedBuildState;
            object beforeCore = authority.CurrentCoreEffectRuntimeState;
            object beforeP6 = authority.CurrentLayoutResilienceSnapshot;
            PlacementCandidate candidate = FindCandidate(
                authority, "I001", false);
            ItemSystemBattleSandboxBoardOperationResult rejected =
                authority.CommitFromTray(
                    "I001", candidate.Anchor, candidate.Rotation);
            Check(!rejected.Accepted
                && ReferenceEquals(beforeItem, authority.CurrentSnapshot)
                && ReferenceEquals(beforeBinding,
                    authority.CurrentBindingSnapshot)
                && ReferenceEquals(beforeQualified,
                    authority.CurrentQualifiedBuildState)
                && ReferenceEquals(beforeCore,
                    authority.CurrentCoreEffectRuntimeState)
                && ReferenceEquals(beforeP6,
                    authority.CurrentLayoutResilienceSnapshot),
                "Invalid core assembly did not atomically preserve five states.");
        }

        private static void VerifyNoOp()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            object item = authority.CurrentSnapshot;
            object binding = authority.CurrentBindingSnapshot;
            object qualified = authority.CurrentQualifiedBuildState;
            object core = authority.CurrentCoreEffectRuntimeState;
            object p6 = authority.CurrentLayoutResilienceSnapshot;
            int if01 = authority.If01ValidationCount;
            int build = authority.QualifiedBuildAssemblyCount;
            int coreCount = authority.CoreEffectRuntimeAssemblyCount;
            int p6Count = authority.P6EvaluationCount;
            ItemSystemBattleSandboxBoardOperationResult result =
                authority.Reset();
            Check(result.Accepted && !result.Changed
                && ReferenceEquals(item, authority.CurrentSnapshot)
                && ReferenceEquals(binding,
                    authority.CurrentBindingSnapshot)
                && ReferenceEquals(qualified,
                    authority.CurrentQualifiedBuildState)
                && ReferenceEquals(core,
                    authority.CurrentCoreEffectRuntimeState)
                && ReferenceEquals(p6,
                    authority.CurrentLayoutResilienceSnapshot)
                && if01 == authority.If01ValidationCount
                && build == authority.QualifiedBuildAssemblyCount
                && coreCount == authority.CoreEffectRuntimeAssemblyCount
                && p6Count == authority.P6EvaluationCount,
                "No-op changed identity or call counts.");
        }

        private static void VerifyCanonicalAndImmutable()
        {
            ItemCoreEffectIdentityCatalogSnapshot reversedIdentity = new(
                ItemCoreEffectRuntimeStateStatus.Valid,
                fixture.Identity.Rows.Reverse(),
                Array.Empty<
                    ItemCoreEffectRuntimeStateValidationError>());
            ItemCoreEffectCultivationRosterSnapshot reversedCultivation = new(
                ItemCoreEffectRosterCompleteness.Complete,
                fixture.Cultivation.Rows.Reverse());
            Check(string.Equals(
                    reversedIdentity.canonicalSignature,
                    fixture.Identity.canonicalSignature,
                    StringComparison.Ordinal)
                && string.Equals(
                    reversedCultivation.canonicalSignature,
                    fixture.Cultivation.canonicalSignature,
                    StringComparison.Ordinal),
                "Reversed input changed canonical signatures.");
            bool identityBlocked = MutationBlocked(fixture.Identity.Rows);
            bool cultivationBlocked =
                MutationBlocked(fixture.Cultivation.Rows);
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            bool itemBlocked = MutationBlocked(
                authority.CurrentCoreEffectRuntimeState.Items);
            bool coreBlocked = MutationBlocked(
                authority.CurrentCoreEffectRuntimeState.Items[0]
                    .CoreEffectRows);
            Check(identityBlocked && cultivationBlocked
                && itemBlocked && coreBlocked,
                "A public collection remained externally mutable.");
            Check(
                ItemCoreEffectBooleanFact.Unknown !=
                    ItemCoreEffectBooleanFact.KnownFalse
                && ItemCoreEffectBooleanFact.Unknown !=
                    ItemCoreEffectBooleanFact.NotApplicable
                && ItemCoreEffectBooleanFact.KnownFalse !=
                    ItemCoreEffectBooleanFact.NotApplicable,
                "Unknown/NotApplicable/KnownFalse collapsed.");
        }

        private static void VerifyInvalidCultivation()
        {
            ItemCoreEffectCultivationRow source =
                fixture.Cultivation.Rows[0];
            ItemCoreEffectCultivationRosterSnapshot duplicate = new(
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
            Check(duplicate.status ==
                ItemCoreEffectRuntimeStateStatus.Invalid,
                "Duplicate cultivation identity was not Invalid.");
            ItemCoreEffectCultivationRosterSnapshot illegal = new(
                ItemCoreEffectRosterCompleteness.Complete,
                new[]
                {
                    new ItemCoreEffectCultivationRow(
                        "illegal_i031", "I031", 40, "fixture",
                        ItemCoreEffectFactCompleteness.Complete)
                });
            Check(illegal.status ==
                ItemCoreEffectRuntimeStateStatus.Invalid,
                "I031 cultivation row was not Invalid.");

            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            PlacementCandidate unlit = FindCandidate(
                authority, "I001", false);
            Check(authority.CommitFromTray(
                    "I001", unlit.Anchor, unlit.Rotation).Accepted,
                "Contradiction fixture could not place I001 unlit.");
            ItemSystemSnapshot sourceSnapshot = authority.CurrentSnapshot;
            ItemCoreAwakeningItemResult sourceAwakening = sourceSnapshot
                .ToCoreAwakeningResolutionResult()
                .FindPlacementResult("P_BOARD_I001");
            ItemCoreAwakeningNodeState[] badNodes =
                sourceAwakening.NodeStates.Select((node, index) =>
                    index == 0
                        ? new ItemCoreAwakeningNodeState(
                            node.itemId,
                            node.coreEffectId,
                            node.nodeKind,
                            node.unlockLevel,
                            false,
                            true,
                            ItemCoreAwakeningStateKey.Active,
                            ItemCoreAwakeningBlockedReason.None,
                            node.previewText,
                            node.requiredRarityKey)
                        : new ItemCoreAwakeningNodeState(
                            node.itemId,
                            node.coreEffectId,
                            node.nodeKind,
                            node.unlockLevel,
                            node.isUnlocked,
                            node.isActive,
                            Enum.TryParse(node.stateKey,
                                out ItemCoreAwakeningStateKey stateKey)
                                ? stateKey
                                : ItemCoreAwakeningStateKey.Locked,
                            Enum.TryParse(node.blockedReason,
                                out ItemCoreAwakeningBlockedReason blocked)
                                ? blocked
                                : ItemCoreAwakeningBlockedReason.None,
                            node.previewText,
                            node.requiredRarityKey))
                    .ToArray();
            ItemCoreAwakeningItemResult badAwakening = new(
                sourceAwakening.itemId,
                sourceAwakening.placementId,
                sourceAwakening.inputLevel,
                sourceAwakening.resolvedLevel,
                sourceAwakening.highRarityUltimatePreview,
                sourceAwakening.requiredRarityKey,
                false,
                sourceAwakening.isLightingSource,
                sourceAwakening.supportsAwakening,
                sourceAwakening.basicEffectActive,
                badNodes,
                Array.Empty<string>(),
                sourceAwakening.inputSource);
            ItemSystemSnapshot contradictory = new(
                sourceSnapshot.boardSize,
                sourceSnapshot.eyeCell,
                sourceSnapshot.ArrayBonusCells,
                sourceSnapshot.catalogItems,
                sourceSnapshot.placements,
                sourceSnapshot.LitRangeCells,
                sourceSnapshot.lightingResults,
                sourceSnapshot.arrayBonusResults,
                sourceSnapshot.buildSnapshot,
                sourceSnapshot.awakeningResults.Select(value =>
                    string.Equals(value.placementId,
                        sourceAwakening.placementId,
                        StringComparison.Ordinal)
                        ? new ItemSystemAwakeningResultSnapshot(badAwakening)
                        : value).ToArray(),
                sourceSnapshot.skillMonitorSnapshot,
                sourceSnapshot.selectedMainBuildId,
                sourceSnapshot.selectedMainBuildIsExplicit,
                sourceSnapshot.selectedMainBuildSource,
                Array.Empty<ItemSystemValidationError>(),
                sourceSnapshot.i031State);
            ItemInstanceCoreEffectRuntimeStateSnapshot rejected =
                ItemInstanceCoreEffectRuntimeStateAssembler.Instance.Assemble(
                    new ItemInstanceCoreEffectRuntimeStateInput(
                        fixture.Identity,
                        fixture.Cultivation,
                        fixture.Projection.OrdinaryProjectionSet,
                        authority.CurrentBindingSnapshot,
                        contradictory,
                        ItemCoreEffectRosterCompleteness.Complete));
            Check(rejected.status ==
                    ItemCoreEffectRuntimeStateStatus.Invalid
                && rejected.ValidationErrors.Any(error =>
                    error.code ==
                    ItemCoreEffectRuntimeStateValidationCodes
                        .ActiveStateContradiction),
                "active-without-unlocked/lit contradiction was not Invalid.");
        }

        private static void VerifyRarityProjectionFacts()
        {
            GameObject providerObject = new(
                "CoreEffectRarityVerifierProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemBalanceCandidateDetailSandboxAdapter adapter =
                    new(fixture.Workbench, provider);
                foreach ((string key, int visibleCount) in new[]
                         {
                             ("white", 1), ("green", 2), ("blue", 3),
                             ("purple", 4), ("orange", 5)
                         })
                {
                    ItemBalanceCandidateDetailResult result =
                        adapter.Request(new ItemBalanceCandidateDetailRequest
                        {
                            baseItemId = "I001",
                            rarityKey = key,
                            rootSeedText = "404319001"
                        });
                    ItemInstanceProjectionContractSnapshot projection =
                        result?.preview?.ProjectionResult?.snapshot;
                    Check(result?.isSuccess == true && projection != null,
                        key + " Projection fixture failed.");
                    ItemBalanceProfile profile =
                        fixture.Workbench.FindProfile("I001");
                    ItemBalanceRarityVersion version = profile.FindVersion(
                        projection.rarity);
                    Check(projection.EligibleCoreEffectIds.SequenceEqual(
                            version.eligibleCoreEffectIds
                                .OrderBy(value => value,
                                    StringComparer.Ordinal),
                            StringComparer.Ordinal)
                        && projection.VisibleCoreEffectIds.SequenceEqual(
                            version.visibleCoreEffectIds
                                .OrderBy(value => value,
                                    StringComparer.Ordinal),
                            StringComparer.Ordinal)
                        && projection.VisibleCoreEffectIds.Count ==
                            visibleCount,
                        key + " eligible/visible Projection facts mismatch.");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static void VerifyRealSamples()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            foreach ((string baseId, string instanceId) in new[]
                     {
                         ("I001", "wb_i001_orange_404310001"),
                         ("I004", "wb_i004_orange_404310004"),
                         ("I006", "wb_i006_orange_404310006"),
                         ("I009", "wb_i009_orange_404310009")
                     })
            {
                ItemInstanceCoreEffectRuntimeItemSnapshot row =
                    FindByBase(authority.CurrentCoreEffectRuntimeState,
                        baseId);
                Check(string.Equals(row.itemInstanceId, instanceId,
                        StringComparison.Ordinal)
                    && row.CoreEffectRows.Count == 5,
                    baseId + " real deterministic identity mismatch.");
            }
        }

        private static void VerifyDependencyRegressions()
        {
            ItemSystemBattleSandboxBoardAuthority authority = NewAuthority();
            Check(ItemCoreAwakeningResolver.DefaultNodes.Count == 5
                && ItemCoreAwakeningResolver.DefaultNodes.Any(value =>
                    value.nodeKind == ItemCoreAwakeningNodeKind.Core4)
                && ItemCoreAwakeningResolver.DefaultNodes.Any(value =>
                    value.nodeKind == ItemCoreAwakeningNodeKind.Ultimate),
                "CoreAwakeningPreview01 five-node regression.");
            Check(authority.CurrentSnapshot?.isValid == true
                && authority.CurrentSnapshot.schemaVersion ==
                    ItemSystemSnapshot.CurrentSchemaVersion,
                "ItemSystemValidatorAndSnapshot01 regression.");
            Check(fixture.Projection.OrdinaryProjectionSet?.isValid == true
                && fixture.Projection.OrdinaryProjectionSet.Projections
                    .Count == 30,
                "ItemInstanceProjectionContract01 regression.");
            Check(authority.CurrentBindingSnapshot?.isValid == true,
                "ItemInstancePlacementBindingContract01 regression.");
            Check(authority.CurrentQualifiedBuildState?.isValid == true,
                "ItemInstanceQualifiedBuildStateAdapter01 regression.");
            Check(authority.CurrentLayoutResilienceSnapshot?.status ==
                LayoutResilienceBattleSandboxPlaytestStatus.Complete,
                "BattleSandbox BoardAuthority/P6 regression.");
            Check(authority.CurrentCoreEffectRuntimeState?.isValid == true,
                "Core-effect runtime integration regression.");
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                Check(string.Equals(
                        Sha256File(ProjectPath(pair.Key)),
                        pair.Value,
                        StringComparison.Ordinal),
                    "Protected hash mismatch: " + pair.Key);
            }
            string profilesRoot = ProjectPath(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles");
            string[] profiles = Directory.GetFiles(
                    profilesRoot, "*.asset", SearchOption.TopDirectoryOnly)
                .OrderBy(value => Path.GetFileName(value),
                    StringComparer.Ordinal).ToArray();
            Check(profiles.Length == 30,
                "Profiles30 file count mismatch.");
            StringBuilder aggregate = new();
            foreach (string profile in profiles)
            {
                string relative = "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/"
                    + Path.GetFileName(profile);
                aggregate.Append(relative).Append('|')
                    .Append(Sha256File(profile)).Append('\n');
            }
            Check(string.Equals(
                    Sha256Text(aggregate.ToString()),
                    "a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0",
                    StringComparison.Ordinal),
                "Profiles30 aggregate mismatch.");
        }

        private static void VerifyLeakBoundaries()
        {
            string[] allowedExisting =
            {
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs",
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"
            };
            string[] newPaths =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState.meta",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs.meta",
                ReportPath, SpecPath, MatrixPath, LeakPath
            };
            Check(allowedExisting.All(value =>
                    File.Exists(ProjectPath(value)))
                && newPaths.All(value => File.Exists(ProjectPath(value))),
                "One or more frozen whitelist paths are missing.");
            string joined = string.Join("\n", new[]
            {
                File.ReadAllText(ProjectPath(newPaths[1])),
                File.ReadAllText(ProjectPath(newPaths[3])),
                File.ReadAllText(ProjectPath(newPaths[5]))
            });
            Check(!joined.Contains("SaveData", StringComparison.Ordinal)
                && !joined.Contains("RunFlow", StringComparison.Ordinal)
                && !joined.Contains("Reward", StringComparison.Ordinal)
                && !joined.Contains("ItemDetailPanelView",
                    StringComparison.Ordinal)
                && !joined.Contains("ItemDetailSectionView",
                    StringComparison.Ordinal),
                "Forbidden formal/UI dependency leaked into package sources.");
        }

        private static void VerifyPackageDiffCheck()
        {
            string[] paths =
            {
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs",
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState.meta",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs.meta",
                ReportPath, SpecPath, MatrixPath, LeakPath
            };
            foreach (string path in paths)
            {
                Check(File.Exists(ProjectPath(path)),
                    "Package path missing: " + path);
                string text = File.ReadAllText(ProjectPath(path));
                string[] lines = text.Replace("\r\n", "\n").Split('\n');
                Check(lines.All(line =>
                        !line.EndsWith(" ", StringComparison.Ordinal)
                        && !line.EndsWith("\t",
                            StringComparison.Ordinal)
                        && !line.StartsWith("<<<<<<<",
                            StringComparison.Ordinal)
                        && !line.StartsWith(">>>>>>>",
                            StringComparison.Ordinal)),
                    "Whitespace/merge marker issue: " + path);
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
            ItemInstanceCoreEffectRuntimeItemSnapshot[] matches = state.Items
                .Where(value => string.Equals(value.baseItemId, baseItemId,
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

        private static void Run(
            string id,
            string marker,
            Action action)
        {
            UnityEngine.Debug.Log(
                "[ItemCoreEffectIdentityAndRuntimeStateContractVerifier]["
                + id + "] START " + marker);
            try
            {
                action();
                Results.Add(new ScenarioResult(id, marker, true, "PASS"));
                UnityEngine.Debug.Log(
                    "[ItemCoreEffectIdentityAndRuntimeStateContractVerifier]["
                    + id + "] PASS " + marker);
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(
                    id, marker, false,
                    exception.GetType().Name + ": " + exception.Message));
                UnityEngine.Debug.LogError(
                    "[ItemCoreEffectIdentityAndRuntimeStateContractVerifier]["
                    + id + "] FAIL " + marker + " "
                    + exception.GetType().Name + ": " + exception.Message);
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
            StringBuilder report = new();
            report.AppendLine("# Item Core Effect Identity And Runtime State Contract Report");
            report.AppendLine();
            report.AppendLine("- Package: `V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01`");
            report.AppendLine("- Status: `"
                + (Results.All(value => value.Passed)
                    ? "DEV_COMPLETE / QA_PASS"
                    : "QA_FAILED") + "`");
            report.AppendLine("- Identity coverage: `"
                + (fixture?.Identity?.Rows.Count ?? 0) + "/150`");
            report.AppendLine("- Runtime owner: `ItemSystemBattleSandboxBoardAuthority`");
            report.AppendLine("- Cultivation source: `BattleSandboxDevHandtestAllLv40`");
            report.AppendLine("- Core effects are not executed by this package.");
            report.AppendLine("- No player-visible detail change occurs until separately assigned `V0.4-ItemDetailQualifiedCoreEffectProjection01` consumes this state.");
            report.AppendLine("- `USER_HANDTEST_NOT_APPLICABLE`");
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
            File.WriteAllText(ProjectPath(ReportPath),
                report.ToString(), new UTF8Encoding(false));

            StringBuilder spec = new();
            spec.AppendLine("scenarioId,marker,result,detail");
            foreach (ScenarioResult result in Results)
            {
                spec.AppendLine(string.Join(",", Csv(result.Id),
                    Csv(result.Marker),
                    Csv(result.Passed ? "PASS" : "FAIL"),
                    Csv(result.Detail)));
            }
            File.WriteAllText(ProjectPath(SpecPath),
                spec.ToString(), new UTF8Encoding(false));

            StringBuilder matrix = new();
            matrix.AppendLine("baseItemId,nodeKind,candidateDefinitionId,awakeningNodeId,candidateUnlockLevel,awakeningUnlockLevel,requiredRarity,requiredRarityKey,displayName,description,candidatePayloadIdentity,candidateSourceIdentity,awakeningSourceIdentity");
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
                    Csv(row.awakeningSourceIdentity)));
            }
            File.WriteAllText(ProjectPath(MatrixPath),
                matrix.ToString(), new UTF8Encoding(false));

            StringBuilder leak = new();
            leak.AppendLine("# Item Core Effect Identity And Runtime State Contract LeakCheck");
            leak.AppendLine();
            leak.AppendLine("- Result: `"
                + (Results.Any(value => value.Marker == "LEAKCHECK_PASS"
                    && value.Passed) ? "LEAKCHECK_PASS" : "PENDING") + "`");
            leak.AppendLine("- Scene/Prefab/UI/SaveData/formal Battle writes: `0`");
            leak.AppendLine("- I031 ordinary identity/cultivation/runtime rows: `0`");
            leak.AppendLine("- ExtraTrigger/Convert execution paths added: `0`");
            leak.AppendLine("- Next package started: `false`");
            File.WriteAllText(ProjectPath(LeakPath),
                leak.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string ProjectPath(string relative)
        {
            return Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string Sha256File(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return string.Concat(sha.ComputeHash(stream).Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static string Sha256Text(string value)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(
                Encoding.UTF8.GetBytes(value ?? string.Empty)).Select(item =>
                item.ToString("x2", CultureInfo.InvariantCulture)));
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
                    return ItemInstanceCoreEffectRuntimeStateAssembler.Instance
                        .Assemble(input);
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
                            "VERIFIER_FORCED_INVALID",
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
            public ItemSystemBattleSandboxViewProjectionResult Projection { get; }
            public ItemCoreEffectIdentityCatalogSnapshot Identity { get; }
            public ItemCoreEffectCultivationRosterSnapshot Cultivation { get; }
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
    }
}
