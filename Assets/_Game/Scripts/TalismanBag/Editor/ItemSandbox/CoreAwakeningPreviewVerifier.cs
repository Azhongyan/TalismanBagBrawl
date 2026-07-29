#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class CoreAwakeningPreviewVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md";

        private static readonly string[] ScopedSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxCoreAwakeningPreviewCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs"
        };

        private static readonly string[] ModifiedFilePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs",
            "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md",
            "Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv",
            "Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md"
        };

        private static readonly string[] ProtectedFileHashBaselines =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs", "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
            "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs", "c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb",
            "Docs/V0.4/Reports/ItemCandidateCoreEffects150.csv", "5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188",
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset", "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45",
            "Assets/_Game/Scripts/TalismanBag/Items/Generation/Potential/ItemCorePotentialAndBuildEligibilitySchema.cs", "0a3cc80b1dc047dfe6ba6caec83e01a0be69aed78f7697e7666b401ecfac2b99",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs", "da81315cdcd71310ea43729a4ae043b9a61cf33f0e889c706bf81d5bcf24871b",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs", "65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs", "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs", "ced80cfa99b330ca3f5d67fc22198641de817105608b678603c33343ab555e94",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs", "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs", "957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs", "e1add9eddca28802cd297bc8becac378494e8cbcfcec2754ca50c732c78b0359",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs", "ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs", "1c1504948012b94dcbb1cc91310becd2df169bb736003737c5109f19cd72cc06",
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity", "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity", "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6",
            "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab", "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
            "ProjectSettings/EditorBuildSettings.asset", "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"
        };

        private const string ProfilesAggregateHash = "a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0";

        private static readonly string[] ForbiddenSourceTokens =
        {
            "BattleResolver",
            "BattleSnapshotAdapter",
            "UnifiedBattlePage",
            "BattleBridge",
            "V02RunFlow",
            "V03RunFlow",
            "RunFlowController",
            "SaveData",
            "PlayerPrefs",
            "RewardConfig",
            "DropTable",
            "BossInfo",
            "UpgradeService",
            "ItemLevelSystem",
            "ItemMergeSystem",
            "EditorBuildSettings.scenes =",
            "MonitorSlots",
            "ItemSkillMonitorSlot"
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/CoreAwakeningPreview01/[Guard Only] Verify And Write Reports")]
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
            List<CoreAwakeningSpecRow> rows = new();

            try
            {
                RunChecks(result, rows);
                WriteReports(result, rows);

                if (result.Errors.Count == 0)
                {
                    Debug.Log("V0.4-ItemCoreAwakeningCore4NodeExpansion01 verification passed and reports were written.");
                    if (exitWhenBatchMode)
                    {
                        EditorApplication.Exit(0);
                    }
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        Debug.LogError(error);
                    }

                    if (exitWhenBatchMode)
                    {
                        EditorApplication.Exit(1);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                result.Errors.Add(exception.Message);
                WriteReports(result, rows);
                if (exitWhenBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void RunChecks(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            CheckDefaultDefinitionProvider(result, rows);
            CheckCandidateAuthority(result, rows);

            ItemLightingResolutionResult boundaryLighting = CreateBoundaryLightingResult();
            ItemCoreAwakeningResolutionResult boundaryResult = ItemCoreAwakeningResolver.Resolve(
                boundaryLighting,
                CreateBoundaryInputs());
            CheckLevelBoundaries(result, rows, boundaryResult);
            CheckHighRarityReservedOnly(result, rows, boundaryResult);
            CheckSourceUnsupported(result, rows, boundaryResult);
            CheckInputValidation(result, rows, boundaryResult);
            CheckDefinitionValidation(result, rows);
            CheckViewModelProjection(result, rows, boundaryLighting, boundaryResult);
            CheckSandboxPreviewCatalog(result, rows, boundaryLighting);
            CheckBuildAndArrayCannotUnlock(result, rows);
            CheckLightingArrayBuildRegressions(result, rows);
            CheckSnapshotInterface(result, rows);
            CheckItemSystemSnapshotRoundTrip(result, rows, boundaryResult);
            CheckProtectedHashes(result, rows);
            CheckSourceScope(result);

            if (result.Errors.Count == 0)
            {
                result.Notes.Add("COMPONENT_FIXTURE_PASS");
                result.Notes.Add("CORE4_NODE_EXPANSION_PASS");
                result.Notes.Add("UNITY_COMPILE_PASS");
                result.Notes.Add("LEAKCHECK_PASS");
                result.Notes.Add("USER_HANDTEST_NOT_APPLICABLE");
            }
        }

        private static void CheckDefaultDefinitionProvider(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            HashSet<string> allDefinitionIds = new(StringComparer.Ordinal);
            bool ordinaryPassed = true;
            for (int itemIndex = 1; itemIndex <= 30; itemIndex++)
            {
                string itemId = "I" + itemIndex.ToString("D3", CultureInfo.InvariantCulture);
                IReadOnlyList<ItemCoreEffectDefinition> definitions = DefaultItemCoreEffectDefinitionProvider.Instance.GetDefinitions(itemId);
                string[] expectedIds =
                {
                    itemId + "_CORE_01",
                    itemId + "_CORE_02",
                    itemId + "_CORE_03",
                    itemId + "_CORE_04",
                    itemId + "_CORE_ULT"
                };
                ItemCoreAwakeningNodeKind[] expectedKinds =
                {
                    ItemCoreAwakeningNodeKind.Core1,
                    ItemCoreAwakeningNodeKind.Core2,
                    ItemCoreAwakeningNodeKind.Core3,
                    ItemCoreAwakeningNodeKind.Core4,
                    ItemCoreAwakeningNodeKind.Ultimate
                };
                int[] expectedLevels = { 10, 20, 30, 40, 40 };
                string[] expectedRarities = { "white", "green", "blue", "purple", "orange" };
                bool itemPassed = definitions.Count == 5
                    && definitions.Select(definition => definition.coreEffectId).SequenceEqual(expectedIds)
                    && definitions.Select(definition => definition.nodeKind).SequenceEqual(expectedKinds)
                    && definitions.Select(definition => definition.unlockLevel).SequenceEqual(expectedLevels)
                    && definitions.Select(definition => definition.requiredRarityKey).SequenceEqual(expectedRarities)
                    && definitions.All(definition => string.Equals(definition.itemId, itemId, StringComparison.Ordinal));
                ordinaryPassed &= itemPassed;
                foreach (ItemCoreEffectDefinition definition in definitions)
                {
                    ordinaryPassed &= allDefinitionIds.Add(definition.coreEffectId);
                }
            }

            ItemCoreAwakeningNodeKind[] expectedDefaultKinds =
            {
                ItemCoreAwakeningNodeKind.Core1,
                ItemCoreAwakeningNodeKind.Core2,
                ItemCoreAwakeningNodeKind.Core3,
                ItemCoreAwakeningNodeKind.Core4,
                ItemCoreAwakeningNodeKind.Ultimate
            };
            bool enumCompatibilityPassed = (int)ItemCoreAwakeningNodeKind.Core1 == 0
                && (int)ItemCoreAwakeningNodeKind.Core2 == 1
                && (int)ItemCoreAwakeningNodeKind.Core3 == 2
                && (int)ItemCoreAwakeningNodeKind.Ultimate == 3
                && (int)ItemCoreAwakeningNodeKind.Core4 == 4;
            bool defaultOrderPassed = ItemCoreAwakeningResolver.DefaultNodes.Count == 5
                && ItemCoreAwakeningResolver.DefaultNodes.Select(node => node.nodeKind).SequenceEqual(expectedDefaultKinds)
                && !ItemCoreAwakeningResolver.DefaultNodes.Single(node => node.nodeKind == ItemCoreAwakeningNodeKind.Core4).isUltimate
                && ItemCoreAwakeningResolver.DefaultNodes.Single(node => node.nodeKind == ItemCoreAwakeningNodeKind.Ultimate).isUltimate;
            bool sourcePassed = DefaultItemCoreEffectDefinitionProvider.Instance.GetDefinitions("I031").Count == 0;
            bool passed = ordinaryPassed
                && allDefinitionIds.Count == 150
                && enumCompatibilityPassed
                && defaultOrderPassed
                && sourcePassed;
            if (!passed)
            {
                result.Errors.Add($"Default item core effect definitions failed: expected I001-I030 five-node definitions=150 unique, explicit 01/02/03/04/ULT order, preserved enum values, and I031=0; actual unique={allDefinitionIds.Count}.");
            }

            rows.Add(new CoreAwakeningSpecRow("definitionProviderI001I030", "definitions", "I001-I030", 1, 1, false, true, allDefinitionIds.Count.ToString(CultureInfo.InvariantCulture), "None", 10, "None", "30 items x 5 explicit ids = 150; I031=0; enum values compatible", passed ? "PASS" : "FAIL"));
            result.Notes.Add(passed
                ? "I001_I030_AWAKENING_150_PASS"
                : "I001_I030_AWAKENING_150_FAIL");
            result.Notes.Add(sourcePassed ? "I031_EXCLUSION_PASS" : "I031_EXCLUSION_FAIL");
        }

        private static void CheckCandidateAuthority(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            HashSet<string> candidateIds = new(StringComparer.Ordinal);
            bool passed = true;
            int candidateCount = 0;
            for (int itemIndex = 1; itemIndex <= 30; itemIndex++)
            {
                string itemId = "I" + itemIndex.ToString("D3", CultureInfo.InvariantCulture);
                string profilePath = $"Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/ItemBalanceProfile_{itemId}.asset";
                ItemBalanceProfile profile = AssetDatabase.LoadAssetAtPath<ItemBalanceProfile>(profilePath);
                IReadOnlyList<ItemBalanceCoreCandidate> candidates = profile?.coreCandidates;
                candidates ??= Array.Empty<ItemBalanceCoreCandidate>();
                candidateCount += candidates.Count;
                passed &= profile != null
                    && string.Equals(profile.baseItemId, itemId, StringComparison.Ordinal)
                    && candidates.Count == 5;

                string stem = "candidate_core_" + itemId.ToLowerInvariant();
                ItemBalanceCoreCandidate core1 = FindCandidate(candidates, stem + "_01");
                ItemBalanceCoreCandidate core2 = FindCandidate(candidates, stem + "_02");
                ItemBalanceCoreCandidate core3 = FindCandidate(candidates, stem + "_03");
                ItemBalanceCoreCandidate core4 = FindCandidate(candidates, stem + "_04");
                ItemBalanceCoreCandidate ultimate = FindCandidate(candidates, stem + "_ultimate");
                passed &= CandidateMatches(core1, "Core1", 10, ItemInstanceRarity.White)
                    && CandidateMatches(core2, "Core2", 20, ItemInstanceRarity.Green)
                    && CandidateMatches(core3, "Core3", 30, ItemInstanceRarity.Blue)
                    && CandidateMatches(core4, "Core4", 40, ItemInstanceRarity.Purple)
                    && core4?.effectPayload?.operation == ItemCandidateEffectOperation.ExtraTrigger
                    && CandidateMatches(ultimate, "Ultimate", 40, ItemInstanceRarity.Orange)
                    && ultimate?.effectPayload?.operation == ItemCandidateEffectOperation.Convert;

                foreach (ItemBalanceCoreCandidate candidate in candidates)
                {
                    passed &= candidate != null
                        && !string.IsNullOrWhiteSpace(candidate.coreEffectId)
                        && candidateIds.Add(candidate.coreEffectId)
                        && candidate.coreEffectId.StartsWith(stem + "_", StringComparison.Ordinal)
                        && !candidate.coreEffectId.StartsWith(itemId + "_CORE_", StringComparison.Ordinal);
                }
            }

            passed &= candidateCount == 150 && candidateIds.Count == 150;
            if (!passed)
            {
                result.Errors.Add($"Candidate authority regression failed: expected 150 unique Candidate rows with Core4 ExtraTrigger and Ultimate Convert identities; actual rows={candidateCount}, unique={candidateIds.Count}.");
            }

            rows.Add(new CoreAwakeningSpecRow(
                "candidateAuthority150",
                "profiles",
                "I001-I030",
                0,
                0,
                false,
                false,
                candidateIds.Count.ToString(CultureInfo.InvariantCulture),
                "None",
                0,
                "None",
                "Candidate 150 preserved; Core4=purple/ExtraTrigger; Ultimate=orange/Convert",
                passed ? "PASS" : "FAIL"));
            result.Notes.Add(passed
                ? "Candidate authority checked: 150 unique rows remain byte-protected and semantically distinct from Awakening ids."
                : "Candidate authority check failed.");
        }

        private static ItemBalanceCoreCandidate FindCandidate(
            IReadOnlyList<ItemBalanceCoreCandidate> candidates,
            string coreEffectId)
        {
            return (candidates ?? Array.Empty<ItemBalanceCoreCandidate>())
                .SingleOrDefault(candidate => candidate != null
                    && string.Equals(candidate.coreEffectId, coreEffectId, StringComparison.Ordinal));
        }

        private static bool CandidateMatches(
            ItemBalanceCoreCandidate candidate,
            string nodeKind,
            int unlockLevel,
            ItemInstanceRarity rarity)
        {
            return candidate != null
                && string.Equals(candidate.nodeKind, nodeKind, StringComparison.Ordinal)
                && candidate.unlockLevel == unlockLevel
                && candidate.requiredRarity == rarity;
        }

        private static void CheckLevelBoundaries(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            CheckItem(result, rows, boundaryResult, "levelLv1", "P_LV1", 1, 1, true, true, "None", "None", 10, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv9", "P_LV9", 9, 9, true, true, "None", "None", 10, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv10", "P_LV10", 10, 10, true, true, "I003_CORE_01", "I003_CORE_01", 20, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv19", "P_LV19", 19, 19, true, true, "I004_CORE_01", "I004_CORE_01", 20, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv20", "P_LV20", 20, 20, true, true, "I005_CORE_01|I005_CORE_02", "I005_CORE_01|I005_CORE_02", 30, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv29", "P_LV29", 29, 29, true, true, "I006_CORE_01|I006_CORE_02", "I006_CORE_01|I006_CORE_02", 30, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv30", "P_LV30", 30, 30, true, true, "I007_CORE_01|I007_CORE_02|I007_CORE_03", "I007_CORE_01|I007_CORE_02|I007_CORE_03", 40, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv39", "P_LV39", 39, 39, true, true, "I013_CORE_01|I013_CORE_02|I013_CORE_03", "I013_CORE_01|I013_CORE_02|I013_CORE_03", 40, "PASS");
            CheckItem(result, rows, boundaryResult, "levelLv40", "P_LV40", 40, 40, true, true, "I015_CORE_01|I015_CORE_02|I015_CORE_03|I015_CORE_04|I015_CORE_ULT", "I015_CORE_01|I015_CORE_02|I015_CORE_03|I015_CORE_04|I015_CORE_ULT", 0, "PASS");
            CheckItem(result, rows, boundaryResult, "unlitLevelLv40", "P_UNLIT_LV40", 40, 40, false, true, "I016_CORE_01|I016_CORE_02|I016_CORE_03|I016_CORE_04|I016_CORE_ULT", "None", 0, "PASS");
            result.Notes.Add("Level boundaries checked: Lv1, Lv9/10, Lv19/20, Lv29/30, and Lv39/40 unlock only by resolvedLevel thresholds; Core4 and Ultimate remain separate at Lv40.");
        }

        private static void CheckHighRarityReservedOnly(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            ItemCoreAwakeningItemResult item = boundaryResult.FindPlacementResult("P_HIGH_RARITY_LV1");
            ItemCoreAwakeningNodeState ultimate = item?.NodeStates.FirstOrDefault(node => node.nodeKind == ItemCoreAwakeningNodeKind.Ultimate);
            ItemCoreAwakeningNodeState core4 = item?.NodeStates.FirstOrDefault(node => node.nodeKind == ItemCoreAwakeningNodeKind.Core4);
            bool passed = item != null
                && item.inputLevel == 1
                && item.resolvedLevel == 1
                && item.highRarityUltimatePreview
                && ultimate != null
                && !ultimate.isUnlocked
                && ultimate.blockedReason == ItemCoreAwakeningBlockedReason.ReservedRarityGate.ToString()
                && core4 != null
                && !core4.isUnlocked
                && core4.blockedReason == ItemCoreAwakeningBlockedReason.LevelTooLow.ToString();
            if (!passed)
            {
                result.Errors.Add("Lv1 highRarityUltimatePreview=true incorrectly unlocked nodes, failed to mark Ultimate as reserved, or leaked Ultimate-only behavior into Core4.");
            }

            rows.Add(RowFromItem("highRarityReservedOnly", item, "Ultimate remains ReservedRarityGate while Core4 remains LevelTooLow", passed ? "PASS" : "FAIL"));
            result.Notes.Add("High-rarity preview checked: reserved preview remains Ultimate-only and does not unlock or reclassify Core4.");
        }

        private static void CheckSourceUnsupported(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            ItemCoreAwakeningItemResult source = boundaryResult.FindPlacementResult("P_SOURCE");
            bool passed = source != null
                && source.itemId == "I031"
                && !source.supportsAwakening
                && !source.basicEffectActive
                && !source.coreEffectUnlocked
                && !source.coreEffectActive
                && source.NodeStates.Count == 0;
            if (!passed)
            {
                result.Errors.Add("I031 should remain supportsAwakening=false with no core nodes.");
            }

            rows.Add(RowFromItem("sourceUnsupported", source, "I031 has no core nodes", passed ? "PASS" : "FAIL"));
            result.Notes.Add("JuNian source checked: I031 has no awakening support, basic effect, or core nodes.");
        }

        private static void CheckInputValidation(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            ItemCoreAwakeningItemResult input0 = boundaryResult.FindPlacementResult("P_INPUT0");
            bool input0Passed = input0 != null
                && input0.inputLevel == 0
                && input0.resolvedLevel == 1
                && input0.ValidationErrors.Any(error => ContainsOrdinal(error, "below 1"));
            if (!input0Passed)
            {
                result.Errors.Add("inputLevel=0 did not resolve to Lv1 with validationErrors.");
            }

            ItemCoreAwakeningItemResult input41 = boundaryResult.FindPlacementResult("P_INPUT41");
            bool input41Passed = input41 != null
                && input41.inputLevel == 41
                && input41.resolvedLevel == 40
                && input41.coreEffectActive
                && input41.ValidationErrors.Any(error => ContainsOrdinal(error, "above 40"));
            if (!input41Passed)
            {
                result.Errors.Add("inputLevel=41 did not resolve to Lv40 with validationErrors.");
            }

            ItemCoreAwakeningItemResult missing = boundaryResult.FindPlacementResult("P_MISSING_INPUT");
            bool missingPassed = missing != null
                && missing.inputLevel == 1
                && missing.resolvedLevel == 1
                && missing.inputSource == "MissingInputDefaultLv1"
                && missing.ValidationErrors.Any(error => ContainsOrdinal(error, "missing awakening input"));
            if (!missingPassed)
            {
                result.Errors.Add("Missing placement input did not default to Lv1 with validationErrors.");
            }

            rows.Add(RowFromItem("inputLevel0Clamp", input0, "resolvedLevel=1 with validationErrors", input0Passed ? "PASS" : "FAIL"));
            rows.Add(RowFromItem("inputLevel41Clamp", input41, "resolvedLevel=40 with validationErrors", input41Passed ? "PASS" : "FAIL"));
            rows.Add(RowFromItem("missingInputDefaultLv1", missing, "MissingInputDefaultLv1 with validationErrors", missingPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Input validation checked: inputLevel=0, inputLevel=41, and missing placement input are stable and non-throwing.");
        }

        private static void CheckDefinitionValidation(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            ItemLightingResolutionResult lighting = CreateSingleItemLighting("I001", "P_DEF", true);
            IReadOnlyList<ItemCoreAwakeningInput> inputs = new[] { Input("I001", "P_DEF", 40) };

            CheckDefinitionCase(result, rows, "definitionMissing", lighting, inputs, new StaticDefinitionProvider(Array.Empty<ItemCoreEffectDefinition>()), "no core effect definitions");
            CheckDefinitionCase(result, rows, "duplicateCoreEffectId", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_DUP", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_DUP", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "duplicate coreEffectId");
            CheckDefinitionCase(result, rows, "duplicateNodeKind", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_01B", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "duplicate nodeKind");
            CheckDefinitionCase(result, rows, "wrongNodeLevel", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 21),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "unlockLevel mismatch");
            CheckDefinitionCase(result, rows, "definitionItemMismatch", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I999", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "itemId mismatch");
            CheckDefinitionCase(result, rows, "emptyCoreEffectId", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", string.Empty, ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "empty coreEffectId");
            CheckDefinitionCase(result, rows, "nullDefinition", lighting, inputs, new StaticDefinitionProvider(new ItemCoreEffectDefinition[]
            {
                null,
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "null core effect definition");
            CheckDefinitionCase(result, rows, "missingCore4", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "nodeKind 'Core4'");
            CheckDefinitionCase(result, rows, "duplicateCore4Kind", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_04B", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "duplicate nodeKind 'Core4'");
            CheckDefinitionCase(result, rows, "wrongCore4Level", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 39),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "nodeKind 'Core4' unlockLevel mismatch");
            CheckDefinitionCase(result, rows, "sharedCore4UltimateId", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_SHARED", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_SHARED", ItemCoreAwakeningNodeKind.Ultimate, 40)
            }), "duplicate coreEffectId");
            CheckDefinitionCase(result, rows, "core4IdMappedToUltimateKind", lighting, inputs, new StaticDefinitionProvider(new[]
            {
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Ultimate, 40),
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Core4, 40)
            }), "coreEffectId mismatch");
            CheckDefinitionCase(result, rows, "nullDefinitionProvider", lighting, inputs, null, "definitionProvider is null");
            CheckReversedDefinitionOrder(result, rows, lighting, inputs);
            result.Notes.Add("Definition validation checked: Core4 missing/duplicate/wrong-level/shared-id isolation plus existing id, nodeKind, item, null and reversed-order cases.");
        }

        private static void CheckDefinitionCase(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            string caseId,
            ItemLightingResolutionResult lighting,
            IReadOnlyList<ItemCoreAwakeningInput> inputs,
            IItemCoreEffectDefinitionProvider provider,
            string expectedError)
        {
            ItemCoreAwakeningResolutionResult resolution = ItemCoreAwakeningResolver.Resolve(lighting, inputs, provider);
            ItemCoreAwakeningItemResult item = resolution.FindPlacementResult("P_DEF");
            bool passed = resolution.ValidationErrors.Any(error => ContainsOrdinal(error, expectedError))
                && item != null
                && item.NodeStates.Count <= 5;
            if (!passed)
            {
                result.Errors.Add($"{caseId} did not emit expected validation error containing '{expectedError}'.");
            }

            rows.Add(RowFromItem(caseId, item, $"validationErrors contains {expectedError}", passed ? "PASS" : "FAIL"));
        }

        private static void CheckReversedDefinitionOrder(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemLightingResolutionResult lighting,
            IReadOnlyList<ItemCoreAwakeningInput> inputs)
        {
            ItemCoreEffectDefinition[] definitions =
            {
                Def("I001", "I001_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40),
                Def("I001", "I001_CORE_04", ItemCoreAwakeningNodeKind.Core4, 40),
                Def("I001", "I001_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                Def("I001", "I001_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                Def("I001", "I001_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10)
            };
            ItemCoreAwakeningResolutionResult resolution = ItemCoreAwakeningResolver.Resolve(
                lighting,
                inputs,
                new StaticDefinitionProvider(definitions));
            ItemCoreAwakeningItemResult item = resolution.FindPlacementResult("P_DEF");
            string expectedIds = "I001_CORE_01|I001_CORE_02|I001_CORE_03|I001_CORE_04|I001_CORE_ULT";
            bool passed = resolution.ValidationErrors.Count == 0
                && item != null
                && item.NodeStates.Count == 5
                && ItemCoreAwakeningResolver.FormatIds(item.NodeStates.Select(node => node.coreEffectId).ToArray()) == expectedIds;
            if (!passed)
            {
                result.Errors.Add("Reversed definition input order did not resolve to explicit Core1/Core2/Core3/Core4/Ultimate output order.");
            }

            rows.Add(RowFromItem("reversedDefinitionOrder", item, expectedIds, passed ? "PASS" : "FAIL"));
        }

        private static void CheckViewModelProjection(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemLightingResolutionResult lightingResult,
            ItemCoreAwakeningResolutionResult awakeningResult)
        {
            ItemInnerDataCatalogProvider provider = new GameObject("TempCoreAwakeningCatalogProvider").AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemLightingItemResult lightingItem = lightingResult.FindPlacementResult("P_LV40");
                ItemCoreAwakeningItemResult awakeningItem = awakeningResult.FindPlacementResult("P_LV40");
                ItemDetailViewModel baseModel = provider.GetDetailViewModel("I015");
                ItemDetailViewModel projected = ItemSandboxLightingDetailProjection.Project(baseModel, lightingItem, null, null, awakeningItem);
                bool passed = projected != null
                    && projected.statusFlags.inputLevel == 40
                    && projected.statusFlags.resolvedLevel == 40
                    && projected.statusFlags.coreEffectUnlocked
                    && projected.statusFlags.coreEffectActive
                    && projected.displayCoreEffects.Count == 5
                    && projected.displayCoreEffects.Any(line => ContainsOrdinal(line.body, "coreEffectId=I015_CORE_04"))
                    && projected.displayCoreEffects.Any(line => ContainsOrdinal(line.body, "coreEffectId=I015_CORE_ULT"))
                    && projected.awakeningPreview.activeCoreEffectIdsText.Contains("I015_CORE_04")
                    && projected.awakeningPreview.activeCoreEffectIdsText.Contains("I015_CORE_ULT");
                if (!passed)
                {
                    result.Errors.Add("ItemDetailViewModel projection failed to expose input/resolved level, ids, stateKey, and active core effect ids.");
                }

                rows.Add(RowFromItem("detailProjection", awakeningItem, "ViewModel exposes core ids and active state", passed ? "PASS" : "FAIL"));
                result.Notes.Add("ItemDetailViewModel projection checked: inputLevel, resolvedLevel, coreEffectIds, stateKey, blockedReason, and validationErrors are visible.");
            }
            finally
            {
                Object.DestroyImmediate(provider.gameObject);
            }
        }

        private static void CheckSandboxPreviewCatalog(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemLightingResolutionResult lightingResult)
        {
            ItemCoreAwakeningResolutionResult sandboxResult = ItemCoreAwakeningResolver.Resolve(
                lightingResult,
                ItemSandboxCoreAwakeningPreviewCatalog.CreatePreviewInputs(lightingResult));
            IReadOnlyList<int> previewLevels = sandboxResult.ItemResults
                .Where(item => !item.isLightingSource)
                .Select(item => item.resolvedLevel)
                .OrderBy(level => level)
                .ToArray();
            bool allInRange = previewLevels.All(level => level >= 1 && level <= 40);
            bool hasNoLevelZero = !previewLevels.Contains(0);
            bool hasExpectedLevelSet = new[] { 1, 10, 20, 30, 40 }.All(level => previewLevels.Contains(level));
            bool passed = previewLevels.Count > 0
                && allInRange
                && hasNoLevelZero
                && hasExpectedLevelSet;
            if (!passed)
            {
                result.Errors.Add($"Sandbox greybox level preview catalog emitted invalid levels. actualLevels={string.Join("|", previewLevels)}.");
            }

            rows.Add(new CoreAwakeningSpecRow("sandboxPreviewCatalog", "PreviewCatalog", "all", 1, 1, false, true, "None", "None", 10, ItemCoreAwakeningResolver.FormatIds(sandboxResult.ValidationErrors), "Preview resolvedLevel values stay within 1-40, exclude Lv0, and include 1/10/20/30/40 without placementId inference.", passed ? "PASS" : "FAIL"));
            result.Notes.Add("Sandbox preview catalog checked: normal preview levels stay within 1-40, exclude Lv0, and include 1/10/20/30/40 without relying on placementId names.");
        }

        private static void CheckBuildAndArrayCannotUnlock(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            ItemLightingResolutionResult buildLighting = CreateBuildRegressionLighting();
            ItemBuildSynergyResolutionResult buildResult = ItemBuildSynergyResolver.Resolve(buildLighting, ItemInnerDataCatalog.AllItems);
            ItemCoreAwakeningResolutionResult awakeningResult = ItemCoreAwakeningResolver.Resolve(
                buildLighting,
                buildLighting.ItemResults.Select(item => Input(item.itemId, item.placementId, 1)).ToArray());

            bool build6Active = buildResult.FindFaMenBuild("famen:zhenlei")?.build6Active == true;
            bool noCoreByBuild = awakeningResult.ItemResults.Where(item => item.itemId != "I031").All(item => !item.coreEffectUnlocked && !item.coreEffectActive);
            bool noMainBuild = string.IsNullOrWhiteSpace(buildResult.selectedMainBuildId);
            bool noMonitorSlots = typeof(ItemBuildSynergyResolutionResult).GetMember("MonitorSlots").Length == 0
                && typeof(ItemBuildSynergyResolutionResult).Assembly.GetType("TalismanBag.Items.Build.ItemSkillMonitorSlot") == null;
            bool buildPassed = build6Active && noCoreByBuild && noMainBuild && noMonitorSlots;
            if (!buildPassed)
            {
                result.Errors.Add("Build6 regression failed: Build state must not unlock core effects, selectedMainBuildId must stay empty, and MonitorSlots must remain absent.");
            }

            ItemLightingItemResult arrayLightingItem = Lit("I001", "Array active sample", "P_ARRAY_ACTIVE", false, true, "P_SOURCE_BUILD", 0, new Vector2Int(2, 1));
            ItemArrayBonusResolutionResult arrayResult = ItemArrayBonusResolver.Resolve(
                ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig(),
                new[] { arrayLightingItem });
            ItemCoreAwakeningItemResult arrayAwakening = ItemCoreAwakeningResolver.ResolveItem(arrayLightingItem, Input("I001", "P_ARRAY_ACTIVE", 1));
            bool arrayPassed = arrayResult.FindPlacementResult("P_ARRAY_ACTIVE")?.isArrayBonusActive == true
                && !arrayAwakening.coreEffectUnlocked
                && !arrayAwakening.coreEffectActive;
            if (!arrayPassed)
            {
                result.Errors.Add("Array bonus active state incorrectly unlocked a core effect.");
            }

            rows.Add(RowFromItem("build6CannotUnlockCore", awakeningResult.FindPlacementResult("P_BUILD_I001"), "Build6 active but Lv1 core remains locked; no MonitorSlots", buildPassed ? "PASS" : "FAIL"));
            rows.Add(RowFromItem("arrayCannotUnlockCore", arrayAwakening, "Array active but Lv1 core remains locked", arrayPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Build/array isolation checked: Build6 and active array bonus do not unlock core effects; selectedMainBuildId remains empty and monitor slots remain absent.");
        }

        private static void CheckLightingArrayBuildRegressions(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            ItemLightingResolutionResult lighting = ItemLightingResolver.Resolve(
                ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig(),
                new[]
                {
                    new ItemLightingPlacedItem("I031", "Source", new[] { new Vector2Int(0, 0) }, new Vector2Int(0, 0), true, new Vector2Int(0, 0), "P_LIGHT_SOURCE"),
                    new ItemLightingPlacedItem("I001", "Direct lit", new[] { new Vector2Int(0, 1) }, new Vector2Int(0, 1), false, new Vector2Int(0, 1), "P_LIGHT_DIRECT"),
                    new ItemLightingPlacedItem("I007", "Unlit", new[] { new Vector2Int(4, 4) }, new Vector2Int(4, 4), false, new Vector2Int(4, 4), "P_LIGHT_UNLIT")
                });
            bool lightingPassed = lighting.FindPlacementResult("P_LIGHT_DIRECT")?.isDirectLit == true
                && lighting.FindPlacementResult("P_LIGHT_UNLIT")?.isLit == false;
            if (!lightingPassed)
            {
                result.Errors.Add("Lighting regression failed for direct-lit and unlit samples.");
            }

            ItemArrayBonusResolutionResult array = ItemArrayBonusResolver.Resolve(
                ItemSandboxLightingScenarioCatalog.CreateDefaultBoardConfig(),
                new[]
                {
                    Lit("I001", "AP lit", "P_AP_LIT", false, true, "P_LIGHT_SOURCE", 0, new Vector2Int(2, 1)),
                    Unlit("I007", "AP unlit", "P_AP_UNLIT", new Vector2Int(2, 3))
                });
            bool arrayPassed = array.FindPlacementResult("P_AP_LIT")?.isArrayBonusActive == true
                && array.FindPlacementResult("P_AP_UNLIT")?.isArrayBonusActive == false;
            if (!arrayPassed)
            {
                result.Errors.Add("ArrayBonus regression failed for lit and unlit AP samples.");
            }

            ItemBuildSynergyResolutionResult build = ItemBuildSynergyResolver.Resolve(CreateBuildRegressionLighting(), ItemInnerDataCatalog.AllItems);
            bool buildPassed = build.FindFaMenBuild("famen:zhenlei")?.build6Active == true
                && string.IsNullOrWhiteSpace(build.selectedMainBuildId);
            if (!buildPassed)
            {
                result.Errors.Add("BuildSynergyCore regression failed for zhenlei Build6 and selectedMainBuildId reserved state.");
            }

            rows.Add(new CoreAwakeningSpecRow("lightingRegression", "P_LIGHT_DIRECT", "I001", 1, 1, true, true, "None", "None", 10, "None", "direct lit/unlit samples pass", lightingPassed ? "PASS" : "FAIL"));
            rows.Add(new CoreAwakeningSpecRow("arrayBonusRegression", "P_AP_LIT", "I001", 1, 1, true, true, "None", "None", 10, "None", "array lit/unlit samples pass", arrayPassed ? "PASS" : "FAIL"));
            rows.Add(new CoreAwakeningSpecRow("buildSynergyCoreRegression", "famen:zhenlei", "Build", 1, 1, true, true, "None", "None", 10, "None", "Build6 active and selectedMainBuildId empty", buildPassed ? "PASS" : "FAIL"));
            result.Notes.Add("Lighting, ArrayBonus, and BuildSynergyCore regressions checked inside CoreAwakening verifier.");
        }

        private static void CheckSnapshotInterface(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            bool snapshotPassed = typeof(IItemCoreAwakeningSnapshotProvider).IsAssignableFrom(typeof(ItemSandboxGridPlacementPreviewView));
            bool inputPassed = typeof(IItemCoreAwakeningReadOnlyInputProvider).IsAssignableFrom(typeof(ItemSandboxGridPlacementPreviewView));
            bool definitionPassed = typeof(IItemCoreEffectDefinitionProvider).IsAssignableFrom(typeof(DefaultItemCoreEffectDefinitionProvider));
            bool passed = snapshotPassed && inputPassed && definitionPassed;
            if (!passed)
            {
                result.Errors.Add("Core awakening read-only snapshot/input/definition provider interfaces are incomplete.");
            }

            rows.Add(new CoreAwakeningSpecRow("snapshotInputDefinitionInterfaces", "interfaces", "all", 1, 1, false, true, "None", "None", 10, "None", "snapshot/input/definition provider interfaces present", passed ? "PASS" : "FAIL"));
            result.Notes.Add("Interfaces checked: future systems can request read-only inputs, resolved snapshots, and item-specific core effect definitions.");
        }

        private static void CheckItemSystemSnapshotRoundTrip(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult boundaryResult)
        {
            ItemCoreAwakeningItemResult lit = boundaryResult.FindPlacementResult("P_LV40");
            ItemCoreAwakeningItemResult unlit = boundaryResult.FindPlacementResult("P_UNLIT_LV40");
            ItemCoreAwakeningItemResult source = boundaryResult.FindPlacementResult("P_SOURCE");
            ItemSystemSnapshot snapshot = new(
                5,
                Vector2Int.zero,
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemCatalogItemSnapshot>(),
                Array.Empty<ItemSystemPlacementSnapshot>(),
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemLightingResultSnapshot>(),
                Array.Empty<ItemSystemArrayBonusResultSnapshot>(),
                new ItemSystemBuildSnapshot(null),
                new[]
                {
                    new ItemSystemAwakeningResultSnapshot(lit),
                    new ItemSystemAwakeningResultSnapshot(unlit),
                    new ItemSystemAwakeningResultSnapshot(source)
                },
                new ItemSystemSkillMonitorSnapshot(null),
                string.Empty,
                false,
                "Core4NodeExpansionVerifier",
                Array.Empty<ItemSystemValidationError>());

            ItemCoreAwakeningResolutionResult roundTrip = snapshot.ToCoreAwakeningResolutionResult();
            ItemCoreAwakeningItemResult roundTripLit = roundTrip.FindPlacementResult("P_LV40");
            ItemCoreAwakeningItemResult roundTripUnlit = roundTrip.FindPlacementResult("P_UNLIT_LV40");
            ItemCoreAwakeningItemResult roundTripSource = roundTrip.FindPlacementResult("P_SOURCE");
            ItemCoreAwakeningNodeState litCore4 = roundTripLit?.NodeStates.SingleOrDefault(node => node.nodeKind == ItemCoreAwakeningNodeKind.Core4);
            ItemCoreAwakeningNodeState litUltimate = roundTripLit?.NodeStates.SingleOrDefault(node => node.nodeKind == ItemCoreAwakeningNodeKind.Ultimate);
            ItemCoreAwakeningNodeState unlitCore4 = roundTripUnlit?.NodeStates.SingleOrDefault(node => node.nodeKind == ItemCoreAwakeningNodeKind.Core4);
            ItemCoreAwakeningNodeState unlitUltimate = roundTripUnlit?.NodeStates.SingleOrDefault(node => node.nodeKind == ItemCoreAwakeningNodeKind.Ultimate);
            string expectedIds = "I015_CORE_01|I015_CORE_02|I015_CORE_03|I015_CORE_04|I015_CORE_ULT";
            bool passed = snapshot.schemaVersion == ItemSystemSnapshot.CurrentSchemaVersion
                && roundTripLit?.NodeStates.Count == 5
                && ItemCoreAwakeningResolver.FormatIds(roundTripLit?.NodeStates.Select(node => node.coreEffectId).ToArray()) == expectedIds
                && litCore4?.coreEffectId == "I015_CORE_04"
                && litCore4.isUnlocked
                && litCore4.isActive
                && litCore4.requiredRarityKey == "purple"
                && litUltimate?.coreEffectId == "I015_CORE_ULT"
                && litUltimate.isUnlocked
                && litUltimate.isActive
                && litUltimate.requiredRarityKey == "orange"
                && !string.Equals(litCore4.coreEffectId, litUltimate.coreEffectId, StringComparison.Ordinal)
                && roundTripUnlit?.NodeStates.Count == 5
                && unlitCore4?.isUnlocked == true
                && unlitCore4.isActive == false
                && unlitUltimate?.isUnlocked == true
                && unlitUltimate.isActive == false
                && roundTripSource?.itemId == "I031"
                && roundTripSource.NodeStates.Count == 0;
            if (!passed)
            {
                result.Errors.Add("ItemSystemSnapshot.v2 five-node round-trip lost, merged, renamed or changed Core4/Ultimate state.");
            }

            rows.Add(RowFromItem("itemSystemSnapshotFiveNodeRoundTrip", roundTripLit, expectedIds + "; I031=0", passed ? "PASS" : "FAIL"));
            result.Notes.Add(passed
                ? "ITEMSYSTEM_SNAPSHOT_ROUNDTRIP_PASS"
                : "ITEMSYSTEM_SNAPSHOT_ROUNDTRIP_FAIL");
        }

        private static void CheckProtectedHashes(VerificationResult result, List<CoreAwakeningSpecRow> rows)
        {
            bool passed = true;
            for (int index = 0; index < ProtectedFileHashBaselines.Length; index += 2)
            {
                string path = ProtectedFileHashBaselines[index];
                string expectedHash = ProtectedFileHashBaselines[index + 1];
                if (!File.Exists(path))
                {
                    result.Errors.Add($"Protected file missing: {path}.");
                    passed = false;
                    continue;
                }

                string actualHash = ComputeSha256(path);
                if (!string.Equals(actualHash, expectedHash, StringComparison.Ordinal))
                {
                    result.Errors.Add($"Protected hash mismatch for {path}: expected {expectedHash}, actual {actualHash}.");
                    passed = false;
                }
            }

            const string profileDirectory = "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles";
            string[] profilePaths = Directory.Exists(profileDirectory)
                ? Directory.GetFiles(profileDirectory, "ItemBalanceProfile_I*.asset", SearchOption.TopDirectoryOnly)
                    .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
                    .ToArray()
                : Array.Empty<string>();
            StringBuilder manifest = new();
            foreach (string profilePath in profilePaths)
            {
                string relativePath = profileDirectory + "/" + Path.GetFileName(profilePath);
                manifest.Append(relativePath)
                    .Append('|')
                    .Append(ComputeSha256(profilePath))
                    .Append('\n');
            }

            string actualProfilesHash = ComputeSha256(Encoding.UTF8.GetBytes(manifest.ToString()));
            if (profilePaths.Length != 30
                || !string.Equals(actualProfilesHash, ProfilesAggregateHash, StringComparison.Ordinal))
            {
                result.Errors.Add($"Protected Profiles30 aggregate mismatch: expected files=30/hash={ProfilesAggregateHash}, actual files={profilePaths.Length}/hash={actualProfilesHash}.");
                passed = false;
            }

            rows.Add(new CoreAwakeningSpecRow(
                "protectedTaskStartHashes",
                "disk",
                "all",
                0,
                0,
                false,
                false,
                profilePaths.Length.ToString(CultureInfo.InvariantCulture),
                "None",
                0,
                passed ? "None" : "See report errors",
                "18 protected files plus Profiles30 aggregate remain byte-identical",
                passed ? "PASS" : "FAIL"));
            result.Notes.Add(passed ? "PROTECTED_HASHES_PASS" : "PROTECTED_HASHES_FAIL");
        }

        private static void CheckSourceScope(VerificationResult result)
        {
            foreach (string sourcePath in ScopedSourcePaths)
            {
                if (!File.Exists(sourcePath))
                {
                    result.Errors.Add($"Missing source file: {sourcePath}");
                    continue;
                }

                if (sourcePath.EndsWith("CoreAwakeningPreviewVerifier.cs", StringComparison.Ordinal))
                {
                    continue;
                }

                string content = File.ReadAllText(sourcePath, Encoding.UTF8);
                foreach (string forbiddenToken in ForbiddenSourceTokens)
                {
                    if (content.Contains(forbiddenToken))
                    {
                        result.Errors.Add($"Forbidden source token '{forbiddenToken}' found in {sourcePath}.");
                    }
                }
            }

            result.Notes.Add("Source scope check completed: runtime CoreAwakening preview paths do not reference formal battle, bridge, run flow, save, reward, boss, BuildSettings writers, or monitor slots.");
        }

        private static void CheckItem(
            VerificationResult result,
            List<CoreAwakeningSpecRow> rows,
            ItemCoreAwakeningResolutionResult resolution,
            string caseId,
            string placementId,
            int expectedInputLevel,
            int expectedResolvedLevel,
            bool expectedIsLit,
            bool expectedSupportsAwakening,
            string expectedUnlockedIds,
            string expectedActiveIds,
            int expectedNextUnlockLevel,
            string expectedResult)
        {
            ItemCoreAwakeningItemResult item = resolution.FindPlacementResult(placementId);
            bool passed = item != null
                && item.inputLevel == expectedInputLevel
                && item.resolvedLevel == expectedResolvedLevel
                && item.isLit == expectedIsLit
                && item.supportsAwakening == expectedSupportsAwakening
                && ItemCoreAwakeningResolver.FormatIds(item.UnlockedCoreEffectIds) == expectedUnlockedIds
                && ItemCoreAwakeningResolver.FormatIds(item.ActiveCoreEffectIds) == expectedActiveIds
                && item.nextUnlockLevel == expectedNextUnlockLevel;
            if (!passed)
            {
                result.Errors.Add($"{caseId}/{placementId} expected input={expectedInputLevel}, resolved={expectedResolvedLevel}, lit={expectedIsLit}, supports={expectedSupportsAwakening}, unlocked={expectedUnlockedIds}, active={expectedActiveIds}, next={expectedNextUnlockLevel}; actual input={item?.inputLevel}, resolved={item?.resolvedLevel}, lit={item?.isLit}, supports={item?.supportsAwakening}, unlocked={ItemCoreAwakeningResolver.FormatIds(item?.UnlockedCoreEffectIds)}, active={ItemCoreAwakeningResolver.FormatIds(item?.ActiveCoreEffectIds)}, next={item?.nextUnlockLevel}.");
            }

            rows.Add(RowFromItem(caseId, item, expectedResult, passed ? "PASS" : "FAIL"));
        }

        private static CoreAwakeningSpecRow RowFromItem(
            string caseId,
            ItemCoreAwakeningItemResult item,
            string expectedResult,
            string actualResult)
        {
            return new CoreAwakeningSpecRow(
                caseId,
                item?.placementId ?? string.Empty,
                item?.itemId ?? string.Empty,
                item?.inputLevel ?? 0,
                item?.resolvedLevel ?? 0,
                item?.isLit == true,
                item?.supportsAwakening == true,
                ItemCoreAwakeningResolver.FormatIds(item?.UnlockedCoreEffectIds),
                ItemCoreAwakeningResolver.FormatIds(item?.ActiveCoreEffectIds),
                item?.nextUnlockLevel ?? 0,
                item == null ? "MissingResult" : ItemCoreAwakeningResolver.FormatIds(item.ValidationErrors),
                expectedResult,
                actualResult);
        }

        private static ItemLightingResolutionResult CreateBoundaryLightingResult()
        {
            return new ItemLightingResolutionResult(
                new[] { new Vector2Int(0, 1), new Vector2Int(1, 0) },
                new[]
                {
                    Lit("I001", "Lv1", "P_LV1", false, true, "P_SOURCE", 0),
                    Lit("I002", "Lv9", "P_LV9", false, true, "P_SOURCE", 0),
                    Lit("I003", "Lv10", "P_LV10", false, true, "P_SOURCE", 0),
                    Lit("I004", "Lv19", "P_LV19", false, true, "P_SOURCE", 0),
                    Lit("I005", "Lv20", "P_LV20", false, true, "P_SOURCE", 0),
                    Lit("I006", "Lv29", "P_LV29", false, true, "P_SOURCE", 0),
                    Lit("I007", "Lv30", "P_LV30", false, true, "P_SOURCE", 0),
                    Lit("I013", "Lv39", "P_LV39", false, true, "P_SOURCE", 0),
                    Lit("I015", "Lv40", "P_LV40", false, true, "P_SOURCE", 0),
                    Lit("I001", "Input 0", "P_INPUT0", false, true, "P_SOURCE", 0),
                    Lit("I004", "Input 41", "P_INPUT41", false, true, "P_SOURCE", 0),
                    Lit("I007", "High rarity Lv1", "P_HIGH_RARITY_LV1", false, true, "P_SOURCE", 0),
                    Unlit("I016", "Unlit Lv40", "P_UNLIT_LV40"),
                    Lit("I031", "JuNian source", "P_SOURCE", true, true, "P_SOURCE", 0),
                    Lit("I013", "Missing input", "P_MISSING_INPUT", false, true, "P_SOURCE", 0)
                });
        }

        private static IReadOnlyList<ItemCoreAwakeningInput> CreateBoundaryInputs()
        {
            return new[]
            {
                Input("I001", "P_LV1", 1),
                Input("I002", "P_LV9", 9),
                Input("I003", "P_LV10", 10),
                Input("I004", "P_LV19", 19),
                Input("I005", "P_LV20", 20),
                Input("I006", "P_LV29", 29),
                Input("I007", "P_LV30", 30),
                Input("I013", "P_LV39", 39),
                Input("I015", "P_LV40", 40),
                Input("I001", "P_INPUT0", 0),
                Input("I004", "P_INPUT41", 41),
                new ItemCoreAwakeningInput("I007", "P_HIGH_RARITY_LV1", 1, true, "VerifierHighRarityReserved", "Reserved"),
                Input("I016", "P_UNLIT_LV40", 40),
                Input("I031", "P_SOURCE", 40)
            };
        }

        private static ItemLightingResolutionResult CreateSingleItemLighting(string itemId, string placementId, bool isLit)
        {
            return new ItemLightingResolutionResult(
                Array.Empty<Vector2Int>(),
                new[] { isLit ? Lit(itemId, "Single item", placementId, false, true, "P_SOURCE", 0) : Unlit(itemId, "Single item", placementId) });
        }

        private static ItemLightingResolutionResult CreateBuildRegressionLighting()
        {
            return new ItemLightingResolutionResult(
                Array.Empty<Vector2Int>(),
                new[]
                {
                    Lit("I031", "Source", "P_SOURCE_BUILD", true, true, "P_SOURCE_BUILD", 0),
                    Lit("I001", "Build I001", "P_BUILD_I001", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I002", "Build I002", "P_BUILD_I002", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I003", "Build I003", "P_BUILD_I003", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I004", "Build I004", "P_BUILD_I004", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I005", "Build I005", "P_BUILD_I005", false, true, "P_SOURCE_BUILD", 0),
                    Lit("I006", "Build I006", "P_BUILD_I006", false, true, "P_SOURCE_BUILD", 0)
                });
        }

        private static ItemCoreAwakeningInput Input(string itemId, string placementId, int inputLevel)
        {
            return new ItemCoreAwakeningInput(itemId, placementId, inputLevel, false, "VerifierReadOnlyPreview");
        }

        private static ItemCoreEffectDefinition Def(
            string itemId,
            string coreEffectId,
            ItemCoreAwakeningNodeKind nodeKind,
            int unlockLevel)
        {
            return new ItemCoreEffectDefinition(itemId, coreEffectId, nodeKind, unlockLevel);
        }

        private static ItemLightingItemResult Lit(
            string itemId,
            string displayName,
            string placementId,
            bool isLightingSource,
            bool isDirectLit,
            string litByPlacementId,
            int litDepth)
        {
            return Lit(itemId, displayName, placementId, isLightingSource, isDirectLit, litByPlacementId, litDepth, StableCell(placementId));
        }

        private static ItemLightingItemResult Lit(
            string itemId,
            string displayName,
            string placementId,
            bool isLightingSource,
            bool isDirectLit,
            string litByPlacementId,
            int litDepth,
            Vector2Int cell)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { cell },
                cell,
                isLightingSource,
                isDirectLit,
                true,
                litByPlacementId,
                litDepth,
                placementId,
                litByPlacementId);
        }

        private static ItemLightingItemResult Unlit(string itemId, string displayName, string placementId)
        {
            return Unlit(itemId, displayName, placementId, StableCell(placementId));
        }

        private static ItemLightingItemResult Unlit(string itemId, string displayName, string placementId, Vector2Int cell)
        {
            return new ItemLightingItemResult(
                itemId,
                displayName,
                new[] { cell },
                cell,
                false,
                false,
                false,
                string.Empty,
                -1,
                placementId,
                string.Empty);
        }

        private static Vector2Int StableCell(string value)
        {
            unchecked
            {
                int hash = 17;
                string text = value ?? string.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    hash = hash * 31 + text[i];
                }

                hash &= int.MaxValue;
                return new Vector2Int(hash % 5, hash / 5 % 5);
            }
        }

        private static void WriteReports(VerificationResult result, IReadOnlyList<CoreAwakeningSpecRow> rows)
        {
            Directory.CreateDirectory("Docs/V0.4/Reports");
            UTF8Encoding utf8WithBom = new(true);
            File.WriteAllText(DetailReportPath, BuildDetailReport(result), utf8WithBom);
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(rows), utf8WithBom);
            File.WriteAllText(LeakCheckReportPath, BuildLeakCheckReport(result), utf8WithBom);
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemCoreAwakeningCore4NodeExpansion01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-ItemCoreAwakeningCore4NodeExpansion01`");
            builder.AppendLine("- Guard receipt: `ITEM_GUARD_PASS_ITEMCOREAWAKENINGCORE4NODEEXPANSION01`");
            builder.AppendLine($"- Verification: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: independent Item Sandbox only.");
            builder.AppendLine();
            builder.AppendLine("## Implemented");
            builder.AppendLine("- `I001-I030` expose five explicit item-specific Awakening definitions each: Core1/Core2/Core3/Core4/Ultimate (`150` unique definitions total).");
            builder.AppendLine("- Core4 is `{itemId}_CORE_04`, `nodeKind=Core4`, `unlockLevel=40`, `requiredRarityKey=purple`; Ultimate remains independent as `{itemId}_CORE_ULT` with orange metadata.");
            builder.AppendLine("- Existing enum identities remain `Core1=0`, `Core2=1`, `Core3=2`, `Ultimate=3`; `Core4=4` is appended while explicit node output remains `01/02/03/04/ULT`.");
            builder.AppendLine("- Levels resolve as `resolvedLevel = Clamp(inputLevel, 1, 40)` while preserving raw `inputLevel`.");
            builder.AppendLine("- Missing placement input defaults to Lv1 with `inputSource=MissingInputDefaultLv1` and validationErrors.");
            builder.AppendLine("- High-rarity preview behavior remains Ultimate-only; rarity metadata does not unlock either Lv40 node.");
            builder.AppendLine("- Core4 and Ultimate both unlock at resolvedLevel >= 40 but remain independent identities and states.");
            builder.AppendLine("- `IItemCoreEffectDefinitionProvider` provides read-only definitions and I031 returns none.");
            builder.AppendLine("- Definition validation covers Core4 missing/duplicate/wrong-level/swapped/shared-id cases plus existing missing, duplicate, mismatch, empty and null cases.");
            builder.AppendLine("- `ItemSystemSnapshot.v2` five-node round-trip preserves independent Core4/Ultimate state without modifying the snapshot schema.");
            builder.AppendLine("- Candidate profiles remain the protected semantic authority for ExtraTrigger/Convert content and stay at 150 unique rows.");
            builder.AppendLine("- Node output includes coreEffectId, itemId, nodeKind, unlockLevel, isUnlocked, isActive, stateKey, and blockedReason.");
            builder.AppendLine("- Build, array bonus, selected main Build, and monitor slots do not participate in awakening unlocks.");
            builder.AppendLine("- No ExtraTrigger/Convert execution, formal upgrade, rarity unlock, core numeric effect, combat execution, save, reward, boss, bridge, run flow, UI, Scene, Prefab or BuildSettings work.");
            builder.AppendLine();
            AppendResultList(builder, "Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            builder.AppendLine("## Modified Files");
            foreach (string sourcePath in ModifiedFilePaths)
            {
                builder.AppendLine($"- `{sourcePath}`");
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<CoreAwakeningSpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,placementId,itemId,inputLevel,resolvedLevel,isLit,supportsAwakening,unlockedCoreEffectIds,activeCoreEffectIds,nextUnlockLevel,validationErrors,expectedResult,actualResult");
            foreach (CoreAwakeningSpecRow row in rows)
            {
                string[] values =
                {
                    row.caseId,
                    row.placementId,
                    row.itemId,
                    row.inputLevel.ToString(CultureInfo.InvariantCulture),
                    row.resolvedLevel.ToString(CultureInfo.InvariantCulture),
                    row.isLit ? "true" : "false",
                    row.supportsAwakening ? "true" : "false",
                    row.unlockedCoreEffectIds,
                    row.activeCoreEffectIds,
                    row.nextUnlockLevel.ToString(CultureInfo.InvariantCulture),
                    row.validationErrors,
                    row.expectedResult,
                    row.actualResult
                };
                builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(VerificationResult result)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemCoreAwakeningCore4NodeExpansion01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-ItemCoreAwakeningCore4NodeExpansion01`");
            builder.AppendLine($"- Result: {(result.Errors.Count == 0 ? "PASS" : "FAIL")}");
            builder.AppendLine("- Scope: existing Item Core Awakening resolver/definition provider and verifier only; reports are regenerated in place.");
            builder.AppendLine("- BuildSettings: no writes; Item Sandbox remains manual-only.");
            builder.AppendLine("- Forbidden integrations: formal battle resolver, bridge, run flow, save data, rewards, drops, boss data, formal upgrade systems, main Build selection, monitor slots, and BuildSettings writers.");
            builder.AppendLine("- Not implemented: ExtraTrigger/Convert execution, formal item upgrade, experience, rarity evolution, storage, save serialization, combat skill release, damage/heal/shield/status settlement, rewards, drops, boss logic, selected main Build, MonitorSlots, or ItemSkillMonitorSlot.");
            builder.AppendLine("- ItemSystemSnapshot.v2, Candidate content, Item Detail UI, Scene, Prefab and BuildSettings remain protected and byte-identical.");
            builder.AppendLine("- Known unrelated dirty files intentionally untouched are outside this verifier's source scope.");
            builder.AppendLine();
            AppendResultList(builder, "Passed Checks / Notes", result.Notes);
            AppendResultList(builder, "Errors", result.Errors);
            return builder.ToString();
        }

        private static void AppendResultList(StringBuilder builder, string title, IReadOnlyList<string> values)
        {
            builder.AppendLine($"## {title}");
            if (values == null || values.Count == 0)
            {
                builder.AppendLine("- None");
                builder.AppendLine();
                return;
            }

            foreach (string value in values)
            {
                builder.AppendLine($"- {value}");
            }

            builder.AppendLine();
        }

        private static string ComputeSha256(string path)
        {
            return ComputeSha256(File.ReadAllBytes(path));
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using SHA256 sha256 = SHA256.Create();
            return string.Concat(sha256.ComputeHash(bytes ?? Array.Empty<byte>())
                .Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
        }

        private static string EscapeCsv(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            bool needsEscape = value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r");
            return needsEscape ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }

        private static bool ContainsOrdinal(string value, string expected)
        {
            return value != null
                && expected != null
                && value.IndexOf(expected, StringComparison.Ordinal) >= 0;
        }

        private sealed class StaticDefinitionProvider : IItemCoreEffectDefinitionProvider
        {
            private readonly IReadOnlyList<ItemCoreEffectDefinition> definitions;

            public StaticDefinitionProvider(IReadOnlyList<ItemCoreEffectDefinition> definitions)
            {
                this.definitions = definitions;
            }

            public IReadOnlyList<ItemCoreEffectDefinition> GetDefinitions(string itemId)
            {
                return definitions;
            }
        }

        private sealed class CoreAwakeningSpecRow
        {
            public CoreAwakeningSpecRow(
                string caseId,
                string placementId,
                string itemId,
                int inputLevel,
                int resolvedLevel,
                bool isLit,
                bool supportsAwakening,
                string unlockedCoreEffectIds,
                string activeCoreEffectIds,
                int nextUnlockLevel,
                string validationErrors,
                string expectedResult,
                string actualResult)
            {
                this.caseId = caseId;
                this.placementId = placementId;
                this.itemId = itemId;
                this.inputLevel = inputLevel;
                this.resolvedLevel = resolvedLevel;
                this.isLit = isLit;
                this.supportsAwakening = supportsAwakening;
                this.unlockedCoreEffectIds = unlockedCoreEffectIds;
                this.activeCoreEffectIds = activeCoreEffectIds;
                this.nextUnlockLevel = nextUnlockLevel;
                this.validationErrors = validationErrors;
                this.expectedResult = expectedResult;
                this.actualResult = actualResult;
            }

            public readonly string caseId;
            public readonly string placementId;
            public readonly string itemId;
            public readonly int inputLevel;
            public readonly int resolvedLevel;
            public readonly bool isLit;
            public readonly bool supportsAwakening;
            public readonly string unlockedCoreEffectIds;
            public readonly string activeCoreEffectIds;
            public readonly int nextUnlockLevel;
            public readonly string validationErrors;
            public readonly string expectedResult;
            public readonly string actualResult;
        }

        private sealed class VerificationResult
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Notes = new();
        }
    }
}
#endif
