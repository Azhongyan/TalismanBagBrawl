using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemCombatEffectRequestContractVerifier
    {
        private const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/"
            + "ItemBalanceWorkbenchCatalog.asset";
        private const string AssignmentPath =
            "Docs/V0.4/ItemCombatEffectRequestContract01_Assignment.md";
        private const string AssignmentHash =
            "6dd4cad1fc930d0b969e9ff92455b1cbbdb52a40f8b4809add69819b7e9ec35e";
        private const string CultivationSource =
            "BattleSandboxDevHandtestAllLv40";

        private const string ReportPath =
            "Docs/V0.4/Reports/ItemCombatEffectRequestContractReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemCombatEffectRequestContractSpec.csv";
        private const string SamplePath =
            "Docs/V0.4/Reports/ItemCombatEffectRequestRealSampleMatrix.csv";
        private const string LineagePath =
            "Docs/V0.4/Reports/ItemCombatEffectRequestFieldLineage.csv";
        private const string UnsupportedPath =
            "Docs/V0.4/Reports/ItemCombatEffectRequestUnsupportedTelemetry.csv";
        private const string CanonicalPath =
            "Docs/V0.4/Reports/ItemCombatEffectRequestCanonicalMatrix.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemCombatEffectRequestLeakCheckReport.md";

        private static readonly string[] OrdinaryFixtureItems =
        {
            "I007", "I008", "I009", "I010", "I011", "I012"
        };

        private static readonly IReadOnlyDictionary<string, long> ExpectedDamage =
            new Dictionary<string, long>(StringComparer.Ordinal)
            {
                ["I007"] = 29L,
                ["I008"] = 42L,
                ["I009"] = 53L,
                ["I010"] = 46L,
                ["I011"] = 55L,
                ["I012"] = 76L
            };

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs"] = "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967",
                ["Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs"] = "335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] = "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"] = "798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs"] = "95341ddbc3572a1d116287ac4edb221698d56b1b7133768daa45fbee02c0179d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs"] = "451b6d3603278a7bcbc983b69e428685c1ebdb9391ebc02cbe9644448118166a",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs"] = "e4ca29265d3bb3c18e7b1ff8c3de73190a1dc72b8b2c1d01f03f529343c6c904",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs"] = "f3534fff59bd46852c5918755caa63a79f4beec7915c876fa51aeb9ded0fd0e6",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] = "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"] = "15d7a0cbf1b60969e6c27030a7d1d8ccd1d733fb73421396bee5a8ee21382b4b",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs"] = "19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemGeneratedInstanceSnapshot.cs"] = "8ed5da129d0819978c98e0ef53deac273cc9dbc9d10f5dc9e28cbeb71fba18ef",
                ["Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs"] = "606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs"] = "4e24931a20c3774278431fa0b3052ce3b5deb40dd0d74e00cb731e2307a00e86",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] = "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"] = "116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ModifierEventBridge.cs"] = "e5b2180d4468d73af3fa616efdab1bd21a3769072289f04ab658aa672c898449",
                ["Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset"] = "d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity"] = "ff2d422476b94b525c0c48c261543ac8f73d55a41be1b70340b485cb73ee0796",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] = "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxRuntimeLoop.cs"] = "301adbab43046acde70cfc51ef85ee60642b15e52caaf7ebc980dc4c3fcff1ab",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"] = "519a7b69fc16a9c50563ceb34f614967ae58854c57f4846d82bdcdee9093523a",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs"] = "a7957ffd262dc5231622e8762c644ebad1ccb5d0b662325a4fe9ec62d23d497f",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs"] = "448a69762ec157214809e1d43cd19a134d77f7928742cc890a94a41b1588d56e",
                ["Docs/V0.4/ShougunuPhase1RuntimeAndActionContract01_Assignment.md"] = "40f244475a2aabadffb8ba440c011ec8a7dae46c69043db7f93e928788c0c757"
            };

        private static readonly string[] PackageFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Combat.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCombatEffectRequestContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCombatEffectRequestContractVerifier.cs.meta",
            ReportPath,
            SpecPath,
            SamplePath,
            LineagePath,
            UnsupportedPath,
            CanonicalPath,
            LeakPath
        };

        private static readonly List<ScenarioResult> Results = new();
        private static Fixture fixture;
        private static ItemCombatEffectRequestSnapshot knownZeroSnapshot;
        private static ItemCombatEffectRequestSnapshot unknownSnapshot;
        private static ItemCombatEffectRequestSnapshot invalidSnapshot;

        [MenuItem("Talisman Bag/V0.4/Verify Item Combat Effect Request Contract")]
        public static void VerifyOffline()
        {
            VerifyStaticBatch();
        }

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            fixture = BuildFixture(CurrentPlacements());
            Run("S01", "COMPONENT_FIXTURE_PASS", VerifyComponentFixture);
            Run("S02", "REAL_CURRENT_ITEM_STATE_ASSEMBLY_PASS",
                VerifyRealCurrentState);
            Run("S03", "ITEM_COMBAT_EFFECT_REQUEST_CONTRACT_PASS",
                VerifyRequestContract);
            Run("S04", "DIRECT_DAMAGE_REQUESTS_29_42_53_46_55_76_PASS",
                VerifyExactDamage);
            Run("S05", "LIHUO_BUILD2_800BP_PASS", VerifyLiHuo);
            Run("S06", "I008_ADJACENCY_THREE_PASS", VerifyAdjacency);
            Run("S07", "I011_TRADEOFF_DAMAGE_BRANCH_PASS", VerifyTradeoff);
            Run("S08", "UNSUPPORTED_UNKNOWN_NOT_ZERO_PASS",
                VerifyUnsupportedAndKnownZero);
            Run("S09", "STALE_MISMATCH_FAIL_CLOSED_PASS",
                VerifyFailClosed);
            Run("S10", "I031_EXCLUSION_PASS", VerifyI031Exclusion);
            Run("S11", "UPSTREAM_CANONICAL_IMMUTABLE_PASS",
                VerifyCanonicalAndImmutable);
            Run("S12", "PROTECTED_HASHES_PASS", VerifyProtectedHashes);
            WriteReports();
            Run("S13", "LEAKCHECK_PASS", VerifyLeakBoundaries);
            WriteReports();
            Run("S14", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS",
                VerifyPackageScopedTextCheck);
            WriteReports();
            ScenarioResult[] failures = Results.Where(value => !value.Passed)
                .ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "ItemCombatEffect request verifier failed: "
                    + string.Join("; ", failures.Select(value =>
                        value.Id + "=" + value.Detail)));
            }
            Debug.Log(
                "[ItemCombatEffectRequestContractVerifier]\n"
                + string.Join("\n", Results.Select(value => value.Marker))
                + "\nUSER_HANDTEST_NOT_APPLICABLE"
                + "\nBATTLE_BRIDGE_NOT_STARTED");
        }

        private static Fixture BuildFixture(
            IReadOnlyList<ItemSystemPlacementInput> placementInputs)
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            Check(workbench != null, "Workbench catalog is missing.");
            GameObject providerObject = new("ItemCombatRequestVerifierProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult view =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbench, provider);
                Check(view.IsValid, "Real ProjectionSet invalid: "
                    + string.Join("|", view.Diagnostics));
                ItemCoreEffectIdentityCatalogSnapshot identity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                Check(identity.isValid, "Core identity catalog invalid.");
                ItemCoreEffectCultivationRosterSnapshot cultivation =
                    new ItemCoreEffectCultivationRosterSnapshot(
                        ItemCoreEffectRosterCompleteness.Complete,
                        view.OrdinaryProjectionSet.Projections.Select(value =>
                            new ItemCoreEffectCultivationRow(
                                value.itemInstanceId,
                                value.baseItemId,
                                40,
                                CultivationSource,
                                ItemCoreEffectFactCompleteness.Complete)));
                Check(cultivation.isValid, "Cultivation roster invalid.");

                string[] placedOrdinary = placementInputs
                    .Where(value => value != null
                        && !string.Equals(value.itemId, "I031",
                            StringComparison.Ordinal))
                    .Select(value => value.itemId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                Dictionary<string, string> placementByBase =
                    placementInputs.Where(value => value != null)
                        .ToDictionary(
                            value => value.itemId,
                            value => value.placementId,
                            StringComparer.Ordinal);
                ItemCoreAwakeningInput[] awakeningInputs =
                    placedOrdinary.Select(baseId =>
                        new ItemCoreAwakeningInput(
                            baseId,
                            placementByBase[baseId],
                            40,
                            true,
                            CultivationSource,
                            "orange")).ToArray();
                ItemSystemSnapshot itemSystem =
                    DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                        new ItemSystemSnapshotInput(
                            placementInputs,
                            ItemSystemBoardConfigInput.Default(),
                            awakeningInputs,
                            null,
                            ItemInnerDataCatalog.AllItems,
                            new[] { I031InventoryPlacementContract.OwnedBoard() }));
                Check(itemSystem.isValid, "Production ItemSystem invalid: "
                    + string.Join("|", itemSystem.validationErrors.Select(
                        value => value.ToDiagnosticString())));

                ItemInstanceProjectionContractSnapshot[] placedProjections =
                    placedOrdinary.Select(baseId =>
                        FindProjection(view, baseId)).ToArray();
                ItemInstanceProjectionSetSnapshot placedProjectionSet =
                    CreateProjectionSet(placedProjections);
                ItemInstancePlacementBindingInput[] bindingInputs =
                    placedProjections.Select(value =>
                        new ItemInstancePlacementBindingInput(
                            value.itemInstanceId,
                            placementByBase[value.baseItemId],
                            value.baseItemId)).ToArray();
                ItemInstancePlacementBindingContractSnapshot binding =
                    ItemInstancePlacementBindingValidator.Instance.Validate(
                        placedProjectionSet,
                        itemSystem,
                        bindingInputs).snapshot;
                Check(binding.isValid, "Production binding invalid: "
                    + string.Join("|", binding.ValidationErrors.Select(
                        value => value.ToDiagnosticString())));
                ItemInstanceQualifiedBuildStateSnapshot qualified =
                    ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                        new ItemInstanceQualifiedBuildStateInput(
                            view.OrdinaryProjectionSet,
                            binding,
                            itemSystem,
                            QualifiedBuildRosterCompleteness
                                .CompleteOwnedRoster));
                Check(qualified.isValid, "Production QualifiedBuild invalid: "
                    + string.Join("|", qualified.ValidationErrors.Select(
                        value => value.ToDiagnosticString())));
                ItemInstanceCoreEffectRuntimeStateSnapshot core =
                    ItemInstanceCoreEffectRuntimeStateAssembler.Instance.Assemble(
                        new ItemInstanceCoreEffectRuntimeStateInput(
                            identity,
                            cultivation,
                            view.OrdinaryProjectionSet,
                            binding,
                            itemSystem,
                            ItemCoreEffectRosterCompleteness.Complete));
                Check(core.isValid, "Production CoreRuntime invalid: "
                    + string.Join("|", core.ValidationErrors.Select(
                        value => value.code + ":" + value.message)));
                ItemCombatEffectRequestSnapshot request =
                    ItemCombatEffectRequestAssembler.Instance.Assemble(
                        view.OrdinaryProjectionSet,
                        itemSystem,
                        qualified,
                        core);
                return new Fixture(
                    workbench, view, identity, cultivation, itemSystem,
                    binding, qualified, core, request);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static IReadOnlyList<ItemSystemPlacementInput>
            CurrentPlacements()
        {
            return new[]
            {
                new ItemSystemPlacementInput(
                    "P_SYSTEM_I031", "I031", new Vector2Int(0, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I009", "I009", new Vector2Int(1, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I012", "I012", new Vector2Int(2, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I010", "I010", new Vector2Int(1, 1), 90),
                new ItemSystemPlacementInput(
                    "P_BOARD_I008", "I008", new Vector2Int(0, 2), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I011", "I011", new Vector2Int(1, 3), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I007", "I007", new Vector2Int(0, 4), 0)
            };
        }

        private static void VerifyComponentFixture()
        {
            Check(ItemCombatEffectRequestSnapshot.CurrentSchemaId ==
                "ItemCombatEffectRequestSnapshot.v1", "Schema mismatch.");
            Check(ItemCombatEffectRequestSnapshot.CurrentAuthorityRevision ==
                "ITEM_COMBAT_EFFECT_REQUEST_CONTRACT01_R1",
                "Authority revision mismatch.");
            Check(typeof(IItemCombatEffectRequestAssembler)
                    .GetMethod("Assemble") != null,
                "Stateless assembler seam missing.");
            Check(fixture.Request.devOnly
                && !fixture.Request.entersFormalBattle,
                "devOnly/formal-flow flags invalid.");
            Check(fixture.View.OrdinaryProjectionSet.Projections.Count == 30
                && fixture.View.OrdinaryProjectionSet.Projections.All(
                    value => !string.Equals(
                        value.baseItemId, "I031",
                        StringComparison.Ordinal)),
                "Ordinary roster is not exact I001-I030.");
            CheckCollectionReadOnly(fixture.Request.Requests,
                "Requests collection");
            CheckCollectionReadOnly(fixture.Request.UnsupportedTelemetry,
                "UnsupportedTelemetry collection");
        }

        private static void VerifyRealCurrentState()
        {
            Check(fixture.ItemSystem.boardSize == 5
                && fixture.ItemSystem.eyeCell == new Vector2Int(2, 2),
                "Board authority geometry mismatch.");
            Check(fixture.ItemSystem.placements.Count == 7,
                "Expected I031 plus six ordinary placements.");
            foreach (string baseId in OrdinaryFixtureItems)
            {
                ItemInstanceProjectionContractSnapshot projection =
                    FindProjection(fixture.View, baseId);
                long expectedRoot = 404310000L + int.Parse(
                    baseId.Substring(1),
                    CultureInfo.InvariantCulture);
                Check(projection.itemInstanceId ==
                    "wb_" + baseId.ToLowerInvariant()
                    + "_orange_"
                    + expectedRoot.ToString(CultureInfo.InvariantCulture),
                    baseId + " instance identity mismatch.");
                Check(projection.rootSeed == expectedRoot,
                    baseId + " root seed mismatch.");
                Check(projection.rarity == ItemInstanceRarity.Orange,
                    baseId + " rarity mismatch.");
                ItemSystemPlacementSnapshot placement =
                    fixture.ItemSystem.placements.Single(value =>
                        string.Equals(value.itemId, baseId,
                            StringComparison.Ordinal));
                Check(placement.isLit && placement.isCountedInBuild,
                    baseId + " is not lit/countable.");
                ItemInstanceCoreEffectRuntimeItemSnapshot core =
                    fixture.Core.Find(projection.itemInstanceId);
                Check(core != null
                    && core.cultivationLevel == 40
                    && string.Equals(
                        core.cultivationLevelSource,
                        CultivationSource,
                        StringComparison.Ordinal)
                    && core.location ==
                        ItemCoreEffectRuntimeLocation.Board
                    && core.CoreEffectRows.Count == 4
                    && core.CoreEffectRows.All(value =>
                        value.activeFact ==
                            ItemCoreEffectBooleanFact.KnownTrue)
                    && core.CoreEffectRows.Select(value => value.nodeKind)
                        .SequenceEqual(new[]
                        {
                            ItemCoreAwakeningNodeKind.Core1,
                            ItemCoreAwakeningNodeKind.Core2,
                            ItemCoreAwakeningNodeKind.Core3,
                            ItemCoreAwakeningNodeKind.Ultimate
                        }),
                    baseId + " four-core Lv40 source mismatch.");
            }
            ItemInstanceQualifiedBuildTrackSnapshot liHuo =
                fixture.Qualified.FindFaMenTrack("famen:lihuo");
            Check(liHuo != null
                && liHuo.qualifiedItemCount == 4
                && liHuo.activeStagePieceCount == 4,
                "LiHuo Build2/4 active and Build6 inactive facts mismatch.");
        }

        private static void VerifyRequestContract()
        {
            ItemCombatEffectRequestSnapshot value = fixture.Request;
            Check(value.status == ItemCombatEffectRequestSnapshotStatus.Valid
                && value.requestCount == 6
                && value.ValidationErrors.Count == 0,
                "Valid six-request snapshot not produced: "
                + string.Join("|", value.ValidationErrors.Select(error =>
                    error.status + ":" + error.code + ":"
                    + error.itemInstanceId + ":"
                    + error.placementId)));
            Check(value.Requests.Select(row => row.sourceBaseItemId)
                    .SequenceEqual(OrdinaryFixtureItems,
                        StringComparer.Ordinal),
                "Request order/identity mismatch.");
            Check(value.Requests.All(row =>
                    row.requestKind ==
                        ItemCombatEffectRequestKind.DirectFlatDamage
                    && row.magnitudeCompleteness ==
                        ItemCombatEffectMagnitudeCompleteness.Complete
                    && row.requiresBattleTargetValidation
                    && row.targetRequestKind ==
                        ItemCombatEffectTargetRequestKind
                            .SingleHostileDamageableRuntimeActor
                    && row.isLitFact ==
                        ItemInstanceQualifiedBuildBooleanFact.True
                    && row.sourceIsCountedFact ==
                        ItemInstanceQualifiedBuildBooleanFact.True
                    && row.Contributions.Any(contribution =>
                        contribution.sourceKind ==
                            ItemCombatEffectContributionSourceKind.BaseStat
                        && contribution.sourceEffectId == "stat:damage")
                    && row.Contributions.Any(contribution =>
                        contribution.sourceEffectId ==
                            "affix_damage_up:effect")),
                "Required request provenance/facts are incomplete.");
            Check(value.sourceQualifiedProjectionSetIdentity ==
                    fixture.Qualified.sourceProjectionSetIdentity
                && value.sourceBindingCanonicalSignature ==
                    fixture.Binding.canonicalSignature
                && value.sourceQualifiedBuildCanonicalSignature ==
                    fixture.Qualified.canonicalSignature
                && value.sourceCoreRuntimeCanonicalSignature ==
                    fixture.Core.canonicalSignature,
                "Upstream signature preservation mismatch.");
        }

        private static void VerifyExactDamage()
        {
            foreach (KeyValuePair<string, long> expected in ExpectedDamage)
            {
                ItemCombatEffectRequestRow row = Request(expected.Key);
                Check(row.resolvedPreMitigationDamageUnits == expected.Value,
                    expected.Key + " damage mismatch.");
            }
        }

        private static void VerifyLiHuo()
        {
            string[] actual = fixture.Request.Requests.Where(row =>
                    row.Contributions.Any(contribution =>
                        contribution.sourceEffectId ==
                            ItemCombatEffectRequestAssembler
                                .LiHuoBuild2EffectId
                        && contribution.appliedBasisPoints == 800L))
                .Select(row => row.sourceBaseItemId)
                .ToArray();
            Check(actual.SequenceEqual(
                    new[] { "I007", "I009", "I010", "I012" },
                    StringComparer.Ordinal),
                "Build2 was not restricted to four qualified LiHuo rows.");
            foreach (string baseId in actual)
            {
                Check(Request(baseId).faMenBuildId == "famen:lihuo"
                    && Request(baseId).faMenBuildCount == 4
                    && Request(baseId).faMenActiveStagePieceCount == 4,
                    baseId + " LiHuo track facts mismatch.");
            }
            Check(fixture.Request.UnsupportedTelemetry.Count(value =>
                    value.sourceEffectId ==
                        ItemCombatEffectRequestAssembler.LiHuoBuild4EffectId
                    && value.machineOperation == "ExtraTrigger"
                    && value.disposition ==
                        ItemCombatEffectUnsupportedDisposition.NotExecuted) == 4,
                "Build4 ExtraTrigger telemetry mismatch.");
            Check(fixture.Request.UnsupportedTelemetry.Count(value =>
                    value.sourceEffectId ==
                        ItemCombatEffectRequestAssembler.LiHuoBuild6EffectId
                    && value.reasonCode == "BUILD6_STAGE_INACTIVE") == 4,
                "Build6 inactive telemetry mismatch.");
        }

        private static void VerifyAdjacency()
        {
            ItemCombatEffectRequestContribution adjacent =
                Request("I008").Contributions.Single(value =>
                    value.sourceEffectId ==
                        "affix_adjacent_damage:effect");
            Check(adjacent.rawUnits == 613L
                && adjacent.stackCount == 3
                && adjacent.appliedBasisPoints == 1839L,
                "I008 general adjacency contribution mismatch.");
            string assemblerText = Read(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/"
                + "ItemCombatEffectRequestAssembler.cs");
            Check(!assemblerText.Contains(
                    "case \"I008\"", StringComparison.Ordinal)
                && !assemblerText.Contains(
                    "baseItemId == \"I008\"", StringComparison.Ordinal),
                "I008 item-id special case leaked into assembler.");
        }

        private static void VerifyTradeoff()
        {
            ItemCombatEffectRequestContribution trade =
                Request("I011").Contributions.Single(value =>
                    value.sourceEffectId ==
                        "affix_damage_cost_trade:damage");
            Check(trade.rawUnits == 2200L
                && trade.appliedBasisPoints == 2200L,
                "I011 damage trade branch mismatch.");
            Check(fixture.Request.UnsupportedTelemetry.Any(value =>
                    value.sourceBaseItemId == "I011"
                    && value.sourceEffectId ==
                        "affix_damage_cost_trade:resource_cost"
                    && value.magnitudePresence ==
                        ItemCombatEffectMagnitudePresence.Missing
                    && !value.magnitudeUnits.HasValue
                    && value.disposition ==
                        ItemCombatEffectUnsupportedDisposition.NotExecuted
                    && value.reasonCode ==
                        "RESOURCE_OWNER_NOT_IN_ITEM_REQUEST_CONTRACT_V1"),
                "I011 resource-cost branch was not explicitly NotExecuted.");
        }

        private static void VerifyUnsupportedAndKnownZero()
        {
            Check(fixture.Request.UnsupportedTelemetry.Any(value =>
                    value.sourceBaseItemId == "I012"
                    && value.sourceEffectId ==
                        "affix_cooldown_reduction:effect"
                    && value.magnitudePresence ==
                        ItemCombatEffectMagnitudePresence.Conflicted
                    && value.magnitudeUnits == 1584L
                    && value.machineUnit == "turn"
                    && value.disposition ==
                        ItemCombatEffectUnsupportedDisposition.NotExecuted),
                "I012 cooldown conflict telemetry mismatch.");
            Check(fixture.Request.UnsupportedTelemetry.Where(value =>
                    value.sourceKind ==
                        ItemCombatEffectUnsupportedSourceKind.Core)
                .All(value =>
                    value.magnitudePresence ==
                        ItemCombatEffectMagnitudePresence.Missing
                    && !value.magnitudeUnits.HasValue
                    && value.disposition ==
                        ItemCombatEffectUnsupportedDisposition.Unknown),
                "Core Unknown magnitudes were exposed as numeric zero.");
            Check(fixture.Request.Requests.All(row =>
                    row.Contributions.All(contribution =>
                        !row.ActiveCoreEffectIds.Contains(
                            contribution.sourceEffectId,
                            StringComparer.Ordinal))),
                "Active core identity became a numeric contribution.");

            Fixture knownZero = BuildFixture(new[]
            {
                new ItemSystemPlacementInput(
                    "P_SYSTEM_I031", "I031", new Vector2Int(0, 0), 0),
                new ItemSystemPlacementInput(
                    "P_BOARD_I008", "I008", new Vector2Int(1, 0), 0)
            });
            knownZeroSnapshot = knownZero.Request;
            ItemCombatEffectRequestContribution zero =
                knownZeroSnapshot.Requests.Single().Contributions.Single(
                    value => value.sourceEffectId ==
                        "affix_adjacent_damage:effect");
            Check(zero.rawUnits == 613L
                && zero.stackCount == 0
                && zero.appliedBasisPoints == 0L,
                "Known-zero adjacency fixture is not explicit zero.");
            unknownSnapshot =
                ItemCombatEffectRequestAssembler.Instance.Assemble(
                    knownZero.View.OrdinaryProjectionSet,
                    knownZero.ItemSystem,
                    knownZero.Qualified,
                    new ItemInstanceCoreEffectRuntimeStateSnapshot(
                        ItemCoreEffectRuntimeStateStatus.Unknown,
                        ItemCoreEffectRosterCompleteness.Unknown,
                        knownZero.Core.sourceIdentityCatalogSignature,
                        knownZero.Core.sourceCultivationRosterSignature,
                        knownZero.Core.sourceProjectionSetSignature,
                        knownZero.Core.sourceBindingSignature,
                        knownZero.Core.sourceItemSystemSignature,
                        Array.Empty<
                            ItemInstanceCoreEffectRuntimeItemSnapshot>(),
                        Array.Empty<
                            ItemCoreEffectRuntimeStateValidationError>()));
            Check(unknownSnapshot.status ==
                    ItemCombatEffectRequestSnapshotStatus.Unknown
                && unknownSnapshot.requestCount == 0
                && unknownSnapshot.canonicalSignature !=
                    knownZeroSnapshot.canonicalSignature
                && unknownSnapshot.BuildCanonicalPayload().Contains(
                    "Missing", StringComparison.Ordinal)
                && !unknownSnapshot.UnsupportedTelemetry.Any(value =>
                    value.magnitudePresence ==
                        ItemCombatEffectMagnitudePresence.Present
                    && value.magnitudeUnits == 0L),
                "Unknown facts leaked numeric zero or matched Known Zero.");
        }

        private static void VerifyFailClosed()
        {
            List<ItemCombatEffectRequestSnapshot> rejected = new();
            ItemSystemPlacementSnapshot source =
                fixture.ItemSystem.placements.Single(value =>
                    value.itemId == "I007");
            rejected.Add(AssembleWith(CloneItemSystem(
                fixture.ItemSystem,
                fixture.ItemSystem.placements.Select(value =>
                    value == source
                        ? ClonePlacement(
                            source, placementId: "P_STALE_I007")
                        : value))));
            rejected.Add(AssembleWith(CloneItemSystem(
                fixture.ItemSystem,
                fixture.ItemSystem.placements.Select(value =>
                    value == source
                        ? ClonePlacement(source, itemId: "I008")
                        : value))));
            rejected.Add(AssembleWith(CloneItemSystem(
                fixture.ItemSystem,
                fixture.ItemSystem.placements.Concat(new[] { source }))));
            rejected.Add(AssembleWith(CloneItemSystem(
                fixture.ItemSystem,
                fixture.ItemSystem.placements.Concat(new[]
                {
                    ClonePlacement(
                        source,
                        placementId: "P_ORPHAN_I013",
                        itemId: "I013")
                }))));
            rejected.Add(AssembleWith(CloneItemSystem(
                fixture.ItemSystem,
                fixture.ItemSystem.placements,
                fixture.ItemSystem.selectedMainBuildSource
                + "|STALE_SIGNATURE")));

            ItemInstanceProjectionContractSnapshot projection =
                FindProjection(fixture.View, "I007");
            ItemInstanceProjectionSetSnapshot instanceMismatch =
                CreateProjectionSet(
                    fixture.View.OrdinaryProjectionSet.Projections.Select(
                        value => value == projection
                            ? CloneProjection(
                                value,
                                "stale_instance_i007",
                                value.baseItemId)
                            : value));
            rejected.Add(ItemCombatEffectRequestAssembler.Instance.Assemble(
                instanceMismatch,
                fixture.ItemSystem,
                fixture.Qualified,
                fixture.Core));
            ItemInstanceProjectionSetSnapshot duplicateIdentity =
                CreateProjectionSet(
                    fixture.View.OrdinaryProjectionSet.Projections.Concat(
                        new[] { projection }));
            rejected.Add(ItemCombatEffectRequestAssembler.Instance.Assemble(
                duplicateIdentity,
                fixture.ItemSystem,
                fixture.Qualified,
                fixture.Core));
            Check(rejected.All(value =>
                    value.status ==
                        ItemCombatEffectRequestSnapshotStatus.Invalid
                    && value.requestCount == 0),
                "One or more stale/mismatch/duplicate/orphan cases did not "
                + "fail closed.");
            invalidSnapshot = rejected[0];
        }

        private static void VerifyI031Exclusion()
        {
            Check(fixture.ItemSystem.placements.Count(value =>
                    value.itemId == "I031") == 1
                && fixture.Request.Requests.All(value =>
                    value.sourceBaseItemId != "I031")
                && fixture.Request.UnsupportedTelemetry.All(value =>
                    value.sourceBaseItemId != "I031"),
                "I031 leaked into ordinary ItemCombat rows.");
        }

        private static void VerifyCanonicalAndImmutable()
        {
            string projectionBefore =
                fixture.View.OrdinaryProjectionSet.BuildCanonicalSignature();
            string itemBefore = fixture.ItemSystem.BuildDebugSignature();
            string qualifiedBefore = fixture.Qualified.canonicalSignature;
            string coreBefore = fixture.Core.canonicalSignature;
            ItemCombatEffectRequestSnapshot repeated =
                ItemCombatEffectRequestAssembler.Instance.Assemble(
                    fixture.View.OrdinaryProjectionSet,
                    fixture.ItemSystem,
                    fixture.Qualified,
                    fixture.Core);
            Check(repeated.canonicalSignature ==
                    fixture.Request.canonicalSignature
                && repeated.BuildCanonicalPayload() ==
                    fixture.Request.BuildCanonicalPayload(),
                "Same input was not deterministic.");
            Check(projectionBefore ==
                    fixture.View.OrdinaryProjectionSet
                        .BuildCanonicalSignature()
                && itemBefore == fixture.ItemSystem.BuildDebugSignature()
                && qualifiedBefore == fixture.Qualified.canonicalSignature
                && coreBefore == fixture.Core.canonicalSignature,
                "Assembler mutated upstream canonical content.");

            ItemInstanceProjectionSetSnapshot reversedProjection =
                CreateProjectionSet(
                    fixture.View.OrdinaryProjectionSet.Projections.Reverse());
            ItemSystemSnapshot reversedItem = CloneItemSystem(
                fixture.ItemSystem,
                fixture.ItemSystem.placements.Reverse());
            ItemInstanceQualifiedBuildStateSnapshot reversedQualified =
                ItemInstanceQualifiedBuildStateAssembler.Instance.Assemble(
                    new ItemInstanceQualifiedBuildStateInput(
                        reversedProjection,
                        fixture.Binding,
                        reversedItem,
                        QualifiedBuildRosterCompleteness
                            .CompleteOwnedRoster));
            ItemInstanceCoreEffectRuntimeStateSnapshot reversedCore =
                ItemInstanceCoreEffectRuntimeStateAssembler.Instance.Assemble(
                    new ItemInstanceCoreEffectRuntimeStateInput(
                        fixture.Identity,
                        fixture.Cultivation,
                        reversedProjection,
                        fixture.Binding,
                        reversedItem,
                        ItemCoreEffectRosterCompleteness.Complete));
            ItemCombatEffectRequestSnapshot reordered =
                ItemCombatEffectRequestAssembler.Instance.Assemble(
                    reversedProjection,
                    reversedItem,
                    reversedQualified,
                    reversedCore);
            Check(reordered.canonicalSignature ==
                    fixture.Request.canonicalSignature,
                "Input order perturbation changed canonical output.");
            string[] semanticSignatures =
            {
                fixture.Request.canonicalSignature,
                knownZeroSnapshot?.canonicalSignature,
                unknownSnapshot?.canonicalSignature,
                invalidSnapshot?.canonicalSignature
            };
            Check(semanticSignatures.All(value =>
                    !string.IsNullOrWhiteSpace(value))
                && semanticSignatures.Distinct(
                    StringComparer.Ordinal).Count() ==
                    semanticSignatures.Length,
                "Valid/NotExecuted, Known Zero, Unknown and Invalid "
                + "canonicals are not distinct.");
            Check(ReferenceEquals(
                    fixture.View.OrdinaryProjectionSet,
                    fixture.View.OrdinaryProjectionSet)
                && ReferenceEquals(fixture.ItemSystem, fixture.ItemSystem)
                && ReferenceEquals(fixture.Qualified, fixture.Qualified)
                && ReferenceEquals(fixture.Core, fixture.Core),
                "Upstream object identity changed.");
        }

        private static void VerifyProtectedHashes()
        {
            Check(HashFile(ProjectPath(AssignmentPath)) == AssignmentHash,
                "Assignment SHA-256 drifted.");
            foreach (KeyValuePair<string, string> entry in ProtectedHashes)
            {
                string path = ProjectPath(entry.Key);
                Check(File.Exists(path), "Protected file missing: " + entry.Key);
                Check(HashFile(path) == entry.Value,
                    "Protected hash drift: " + entry.Key);
            }
            string profileRoot = ProjectPath(
                "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles");
            string[] profiles = Directory.GetFiles(
                    profileRoot,
                    "ItemBalanceProfile_I*.asset",
                    SearchOption.TopDirectoryOnly)
                .OrderBy(Path.GetFileName, StringComparer.Ordinal)
                .ToArray();
            Check(profiles.Length == 30, "Profile roster is not 30/30.");
            string payload = string.Join("\n", profiles.Select(path =>
                Path.GetFileName(path) + "|" + HashFile(path)));
            Check(HashText(payload) ==
                "71fc7080eceaf9adf2aa47508e4900d00fae91d8f8158fb1a2618b69241a8143",
                "30-profile aggregate hash drift.");
        }

        private static void VerifyLeakBoundaries()
        {
            string[] runtimeFiles =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/"
                + "ItemCombatEffectRequestContract.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/"
                + "ItemCombatEffectRequestAssembler.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Combat/"
                + "ItemCombatEffectRequestValidation.cs"
            };
            string runtime = string.Join("\n", runtimeFiles.Select(Read));
            string[] forbidden =
            {
                "using TalismanBag.Enemy",
                "using TalismanBag.Visual",
                "using TalismanBag.BuildSandbox",
                "using UnityEngine.SceneManagement",
                "UnityEngine.UI",
                "SceneManager",
                "EnemyHp",
                "DamageLedger",
                "TargetLedger",
                "ModifierEventBridge"
            };
            Check(forbidden.All(value =>
                    !runtime.Contains(value, StringComparison.Ordinal)),
                "Forbidden Battle/Enemy/Visual/Scene/UI owner leaked.");
            Check(!runtime.Contains("float ", StringComparison.Ordinal)
                && !runtime.Contains("double ", StringComparison.Ordinal),
                "Floating-point damage arithmetic leaked.");
            Check(fixture.Request.entersFormalBattle == false,
                "Contract entered formal Battle.");
            Write(LeakPath,
                "# Item Combat Effect Request Leak Check\n\n"
                + "- Result: PASS\n"
                + "- Runtime owner: Item devOnly request projection\n"
                + "- Forbidden type imports: 0\n"
                + "- Enemy HP/shell/application ownership: 0\n"
                + "- Battle clock/target/damage ledger ownership: 0\n"
                + "- Scene/Prefab/UI/Visual writes: 0\n"
                + "- Formal Battle entry: false\n"
                + "- I008 item-id special cases: 0\n"
                + "- Floating-point damage arithmetic: 0\n");
        }

        private static void VerifyPackageScopedTextCheck()
        {
            Check(PackageFiles.Length == 16
                && PackageFiles.Distinct(
                    StringComparer.Ordinal).Count() == 16
                && PackageFiles.All(path =>
                    File.Exists(ProjectPath(path))),
                "Whitelist is not exact 16/16.");
            string combatRoot = ProjectPath(
                "Assets/_Game/Scripts/TalismanBag/Items/Combat");
            string[] combatFiles = Directory.GetFiles(
                    combatRoot, "*", SearchOption.TopDirectoryOnly)
                .Select(path => path.Substring(ProjectRoot.Length + 1)
                    .Replace('\\', '/'))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] expectedCombat = PackageFiles.Where(value =>
                    value.StartsWith(
                        "Assets/_Game/Scripts/TalismanBag/Items/Combat/",
                        StringComparison.Ordinal))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Check(combatFiles.SequenceEqual(
                    expectedCombat, StringComparer.Ordinal),
                "Combat directory contains a non-whitelist file.");
            Check(!Directory.GetFiles(
                    ProjectPath("Docs/V0.4/Reports"),
                    "ItemCombatEffectRequest*.meta",
                    SearchOption.TopDirectoryOnly).Any(),
                "Forbidden report .meta was created.");
            foreach (string relative in PackageFiles)
            {
                string text = Read(relative);
                Check(!text.Contains(
                        new string('<', 7), StringComparison.Ordinal)
                    && !text.Contains(
                        new string('=', 7), StringComparison.Ordinal)
                    && !text.Contains(
                        new string('>', 7), StringComparison.Ordinal),
                    "Merge marker in " + relative);
                string[] lines = text.Replace(
                        "\r\n", "\n")
                    .Split('\n');
                Check(lines.All(line =>
                        line.Length == line.TrimEnd(' ', '\t').Length),
                    "Trailing whitespace in " + relative);
            }
        }

        private static void WriteReports()
        {
            StringBuilder report = new();
            report.AppendLine("# Item Combat Effect Request Contract")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemCombatEffectRequestContract01`")
                .AppendLine("- Schema: `ItemCombatEffectRequestSnapshot.v1`")
                .AppendLine("- Authority: `ITEM_COMBAT_EFFECT_REQUEST_CONTRACT01_R1`")
                .AppendLine("- Status: "
                    + (Results.All(value => value.Passed) ? "PASS" : "IN_PROGRESS"))
                .AppendLine("- Request count: "
                    + fixture.Request.requestCount.ToString(
                        CultureInfo.InvariantCulture))
                .AppendLine("- Telemetry count: "
                    + fixture.Request.telemetryCount.ToString(
                        CultureInfo.InvariantCulture))
                .AppendLine("- devOnly: true")
                .AppendLine("- entersFormalBattle: false")
                .AppendLine("- User handtest: not applicable")
                .AppendLine("- Battle bridge: not started")
                .AppendLine()
                .AppendLine("## Verifier scenarios")
                .AppendLine()
                .AppendLine("| ID | Marker | Result | Detail |")
                .AppendLine("|---|---|---|---|");
            foreach (ScenarioResult result in Results)
            {
                report.Append("| ").Append(result.Id)
                    .Append(" | ").Append(result.Marker)
                    .Append(" | ").Append(result.Passed ? "PASS" : "FAIL")
                    .Append(" | ").Append(EscapeMarkdown(result.Detail))
                    .AppendLine(" |");
            }
            report.AppendLine()
                .AppendLine("## Canonical sources")
                .AppendLine()
                .AppendLine("- Projection: `"
                    + fixture.Request.sourceProjectionSetCanonicalSignature + "`")
                .AppendLine("- Qualified projection identity: `"
                    + fixture.Request.sourceQualifiedProjectionSetIdentity + "`")
                .AppendLine("- Binding: `"
                    + fixture.Request.sourceBindingCanonicalSignature + "`")
                .AppendLine("- ItemSystem: `"
                    + fixture.Request.sourceItemSystemCanonicalSignature + "`")
                .AppendLine("- QualifiedBuild: `"
                    + fixture.Request.sourceQualifiedBuildCanonicalSignature + "`")
                .AppendLine("- CoreRuntime: `"
                    + fixture.Request.sourceCoreRuntimeCanonicalSignature + "`")
                .AppendLine("- Output: `"
                    + fixture.Request.canonicalSignature + "`");
            Write(ReportPath, report.ToString());

            StringBuilder spec = new();
            spec.AppendLine("scenario_id,marker,result,detail");
            foreach (ScenarioResult result in Results)
            {
                spec.Append(Csv(result.Id)).Append(',')
                    .Append(Csv(result.Marker)).Append(',')
                    .Append(Csv(result.Passed ? "PASS" : "FAIL")).Append(',')
                    .Append(Csv(result.Detail)).AppendLine();
            }
            Write(SpecPath, spec.ToString());

            StringBuilder samples = new();
            samples.AppendLine(
                "base_item_id,item_instance_id,placement_id,rarity,root_seed,"
                + "base_damage,damage_up_bp,adjacency_bp,trade_bp,build2_bp,"
                + "additive_bp,resolved_damage,active_core_count");
            foreach (ItemCombatEffectRequestRow row in fixture.Request.Requests)
            {
                samples.Append(Csv(row.sourceBaseItemId)).Append(',')
                    .Append(Csv(row.sourceItemInstanceId)).Append(',')
                    .Append(Csv(row.sourcePlacementId)).Append(',')
                    .Append(Csv(row.sourceRarity.ToString())).Append(',')
                    .Append(row.sourceRootSeed.ToString(CultureInfo.InvariantCulture))
                    .Append(',')
                    .Append(row.baseDamageRawUnits.ToString(
                        CultureInfo.InvariantCulture)).Append(',')
                    .Append(Applied(row, "affix_damage_up:effect")).Append(',')
                    .Append(Applied(row, "affix_adjacent_damage:effect")).Append(',')
                    .Append(Applied(row, "affix_damage_cost_trade:damage")).Append(',')
                    .Append(Applied(
                        row,
                        ItemCombatEffectRequestAssembler.LiHuoBuild2EffectId))
                    .Append(',')
                    .Append(row.additiveBasisPoints.ToString(
                        CultureInfo.InvariantCulture)).Append(',')
                    .Append(row.resolvedPreMitigationDamageUnits.ToString(
                        CultureInfo.InvariantCulture)).Append(',')
                    .Append(row.ActiveCoreEffectIds.Count.ToString(
                        CultureInfo.InvariantCulture)).AppendLine();
            }
            Write(SamplePath, samples.ToString());

            Write(LineagePath,
                "output_field,authoritative_source,transformation\n"
                + Csv("sourceItemInstanceId") + ","
                + Csv("ProjectionSet.v1.itemInstanceId") + ","
                + Csv("exact identity join") + "\n"
                + Csv("sourcePlacementId") + ","
                + Csv("ItemSystemSnapshot.v2.placementId") + ","
                + Csv("exact IF01 placement join") + "\n"
                + Csv("sourceBaseItemId") + ","
                + Csv("Projection/base + placement/item") + ","
                + Csv("exact equality") + "\n"
                + Csv("baseDamageRawUnits") + ","
                + Csv("Projection stat damage/rawUnits") + ","
                + Csv("exact one non-negative row") + "\n"
                + Csv("additiveBasisPoints") + ","
                + Csv("approved affix/build contributions") + ","
                + Csv("checked integer sum") + "\n"
                + Csv("resolvedPreMitigationDamageUnits") + ","
                + Csv("baseDamage + additiveBasisPoints") + ","
                + Csv("floor(base*(10000+bp)/10000)") + "\n"
                + Csv("activeCoreEffectIds") + ","
                + Csv("CoreRuntime.v2 active rows") + ","
                + Csv("identity preservation only") + "\n"
                + Csv("UnsupportedTelemetry") + ","
                + Csv("unapproved machine facts") + ","
                + Csv("Missing/Conflicted + NotExecuted/Unknown") + "\n");

            StringBuilder unsupported = new();
            unsupported.AppendLine(
                "item_instance_id,base_item_id,placement_id,effect_id,"
                + "source_kind,machine_operation,machine_unit,"
                + "magnitude_presence,magnitude_units,disposition,reason_code,"
                + "source_identities");
            foreach (ItemCombatEffectUnsupportedTelemetry row in
                     fixture.Request.UnsupportedTelemetry)
            {
                unsupported.Append(Csv(row.sourceItemInstanceId)).Append(',')
                    .Append(Csv(row.sourceBaseItemId)).Append(',')
                    .Append(Csv(row.sourcePlacementId)).Append(',')
                    .Append(Csv(row.sourceEffectId)).Append(',')
                    .Append(Csv(row.sourceKind.ToString())).Append(',')
                    .Append(Csv(row.machineOperation)).Append(',')
                    .Append(Csv(row.machineUnit)).Append(',')
                    .Append(Csv(row.magnitudePresence.ToString())).Append(',')
                    .Append(Csv(row.magnitudeUnits?.ToString(
                        CultureInfo.InvariantCulture) ?? string.Empty))
                    .Append(',')
                    .Append(Csv(row.disposition.ToString())).Append(',')
                    .Append(Csv(row.reasonCode)).Append(',')
                    .Append(Csv(string.Join("|", row.SourceIdentities)))
                    .AppendLine();
            }
            Write(UnsupportedPath, unsupported.ToString());

            StringBuilder canonical = new();
            canonical.AppendLine(
                "case,status,request_count,telemetry_count,canonical_signature,"
                + "zero_semantics");
            AppendCanonical(
                canonical, "current_valid_not_executed", fixture.Request,
                "unsupported rows retain Missing/Conflicted");
            AppendCanonical(
                canonical, "known_zero", knownZeroSnapshot,
                "adjacency present stack=0 applied=0");
            AppendCanonical(
                canonical, "unknown", unknownSnapshot,
                "missing fact; zero request");
            AppendCanonical(
                canonical, "invalid", invalidSnapshot,
                "identity/signature contradiction; zero request");
            Write(CanonicalPath, canonical.ToString());
        }

        private static ItemCombatEffectRequestSnapshot AssembleWith(
            ItemSystemSnapshot itemSystem)
        {
            return ItemCombatEffectRequestAssembler.Instance.Assemble(
                fixture.View.OrdinaryProjectionSet,
                itemSystem,
                fixture.Qualified,
                fixture.Core);
        }

        private static ItemSystemSnapshot CloneItemSystem(
            ItemSystemSnapshot source,
            IEnumerable<ItemSystemPlacementSnapshot> placements,
            string selectedMainBuildSource = null)
        {
            return new ItemSystemSnapshot(
                source.boardSize,
                source.eyeCell,
                source.ArrayBonusCells,
                source.catalogItems,
                placements.ToArray(),
                source.LitRangeCells,
                source.lightingResults,
                source.arrayBonusResults,
                source.buildSnapshot,
                source.awakeningResults,
                source.skillMonitorSnapshot,
                source.selectedMainBuildId,
                source.selectedMainBuildIsExplicit,
                selectedMainBuildSource ?? source.selectedMainBuildSource,
                Array.Empty<ItemSystemValidationError>(),
                source.i031State);
        }

        private static ItemSystemPlacementSnapshot ClonePlacement(
            ItemSystemPlacementSnapshot source,
            string placementId = null,
            string itemId = null)
        {
            return new ItemSystemPlacementSnapshot(
                placementId ?? source.placementId,
                itemId ?? source.itemId,
                source.displayName,
                source.anchorCell,
                source.rotation,
                source.OccupiedCells,
                source.coreCellWorld,
                source.isLightingSource,
                source.isDirectLit,
                source.isLit,
                source.litByItemId,
                source.litByPlacementId,
                source.litDepth,
                source.OccupiedArrayBonusCells,
                source.isOnArrayBonusCell,
                source.isArrayBonusActive,
                source.isCountedInBuild,
                source.inputLevel,
                source.resolvedLevel,
                source.UnlockedCoreEffectIds,
                source.ActiveCoreEffectIds);
        }

        private static ItemInstanceProjectionContractSnapshot CloneProjection(
            ItemInstanceProjectionContractSnapshot source,
            string itemInstanceId,
            string baseItemId)
        {
            return CreateNonPublic<
                ItemInstanceProjectionContractSnapshot>(
                source.sourceSchemaId,
                source.sourceGenerationAlgorithmId,
                source.generationDataStatus,
                itemInstanceId,
                baseItemId,
                source.rarity,
                source.rarityKey,
                source.generationVersion,
                source.rootSeed,
                source.cultivationPotentialProfileId,
                source.Stats,
                source.Affixes,
                source.EligibleCoreEffectIds,
                source.VisibleCoreEffectIds,
                source.buildQualification,
                source.sourceCanonicalSignature);
        }

        private static ItemInstanceProjectionSetSnapshot CreateProjectionSet(
            IEnumerable<ItemInstanceProjectionContractSnapshot> values)
        {
            return CreateNonPublic<ItemInstanceProjectionSetSnapshot>(
                values.ToArray(),
                Array.Empty<ItemInstanceProjectionValidationError>());
        }

        private static T CreateNonPublic<T>(params object[] arguments)
        {
            return (T)Activator.CreateInstance(
                typeof(T),
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic,
                null,
                arguments,
                CultureInfo.InvariantCulture);
        }

        private static ItemInstanceProjectionContractSnapshot FindProjection(
            ItemSystemBattleSandboxViewProjectionResult view,
            string baseId)
        {
            return view.OrdinaryProjectionSet.Projections.Single(value =>
                string.Equals(
                    value.baseItemId, baseId, StringComparison.Ordinal));
        }

        private static ItemCombatEffectRequestRow Request(string baseId)
        {
            return fixture.Request.Requests.Single(value =>
                string.Equals(
                    value.sourceBaseItemId, baseId, StringComparison.Ordinal));
        }

        private static string Applied(
            ItemCombatEffectRequestRow row,
            string effectId)
        {
            ItemCombatEffectRequestContribution contribution =
                row.Contributions.FirstOrDefault(value => string.Equals(
                    value.sourceEffectId,
                    effectId,
                    StringComparison.Ordinal));
            return (contribution?.appliedBasisPoints ?? 0L).ToString(
                CultureInfo.InvariantCulture);
        }

        private static void AppendCanonical(
            StringBuilder builder,
            string name,
            ItemCombatEffectRequestSnapshot snapshot,
            string semantics)
        {
            if (snapshot == null)
            {
                return;
            }
            builder.Append(Csv(name)).Append(',')
                .Append(Csv(snapshot.status.ToString())).Append(',')
                .Append(snapshot.requestCount.ToString(
                    CultureInfo.InvariantCulture)).Append(',')
                .Append(snapshot.telemetryCount.ToString(
                    CultureInfo.InvariantCulture)).Append(',')
                .Append(Csv(snapshot.canonicalSignature)).Append(',')
                .Append(Csv(semantics)).AppendLine();
        }

        private static void CheckCollectionReadOnly<T>(
            IReadOnlyList<T> values,
            string label)
        {
            bool blocked = false;
            try
            {
                ((ICollection<T>)values).Add(default);
            }
            catch (NotSupportedException)
            {
                blocked = true;
            }
            Check(blocked, label + " permits external mutation.");
        }

        private static void Run(string id, string marker, Action action)
        {
            try
            {
                action();
                Results.Add(new ScenarioResult(
                    id, marker, true, "verified"));
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(
                    id,
                    marker,
                    false,
                    exception.GetType().Name + ": " + exception.Message));
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string ProjectRoot =>
            Directory.GetParent(Application.dataPath)?.FullName
            ?? throw new InvalidOperationException("Project root unavailable.");

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                ProjectRoot,
                relativePath.Replace(
                    '/', Path.DirectorySeparatorChar)));
        }

        private static string Read(string relativePath)
        {
            return File.ReadAllText(ProjectPath(relativePath));
        }

        private static void Write(string relativePath, string text)
        {
            File.WriteAllText(
                ProjectPath(relativePath),
                text,
                new UTF8Encoding(false));
        }

        private static string HashFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string HashText(string text)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    new UTF8Encoding(false).GetBytes(text ?? string.Empty)))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty)
                .Replace("\"", "\"\"")
                .Replace("\r", " ")
                .Replace("\n", " ") + "\"";
        }

        private static string EscapeMarkdown(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private sealed class Fixture
        {
            public Fixture(
                ItemBalanceWorkbenchCatalog workbench,
                ItemSystemBattleSandboxViewProjectionResult view,
                ItemCoreEffectIdentityCatalogSnapshot identity,
                ItemCoreEffectCultivationRosterSnapshot cultivation,
                ItemSystemSnapshot itemSystem,
                ItemInstancePlacementBindingContractSnapshot binding,
                ItemInstanceQualifiedBuildStateSnapshot qualified,
                ItemInstanceCoreEffectRuntimeStateSnapshot core,
                ItemCombatEffectRequestSnapshot request)
            {
                Workbench = workbench;
                View = view;
                Identity = identity;
                Cultivation = cultivation;
                ItemSystem = itemSystem;
                Binding = binding;
                Qualified = qualified;
                Core = core;
                Request = request;
            }

            public ItemBalanceWorkbenchCatalog Workbench { get; }
            public ItemSystemBattleSandboxViewProjectionResult View { get; }
            public ItemCoreEffectIdentityCatalogSnapshot Identity { get; }
            public ItemCoreEffectCultivationRosterSnapshot Cultivation { get; }
            public ItemSystemSnapshot ItemSystem { get; }
            public ItemInstancePlacementBindingContractSnapshot Binding { get; }
            public ItemInstanceQualifiedBuildStateSnapshot Qualified { get; }
            public ItemInstanceCoreEffectRuntimeStateSnapshot Core { get; }
            public ItemCombatEffectRequestSnapshot Request { get; }
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
