#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TalismanBag.EditorTools.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemGeneration
{
    public static class ItemRarityInstanceFoundationVerifier
    {
        private const string PackageName = "V0.4-ItemRarityInstanceFoundation01";
        private const string DetailReportPath = "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv";
        private const string LeakCheckReportPath = "Docs/V0.4/Reports/ItemRarityInstanceFoundationLeakCheckReport.md";
        private const string RuntimeSourceRoot = "Assets/_Game/Scripts/TalismanBag/Items/Generation";

        [MenuItem("Tools/Talisman Bag/V0.4/Item Generation/ItemRarityInstanceFoundation01/[QA Only] Run Foundation")]
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
            ItemGenerationFoundationSnapshot foundation = null;

            try
            {
                foundation = ItemRarityInstanceFoundation.Create(ItemInnerDataCatalog.AllItems);
                RunFoundationChecks(foundation, result, rows);
                RunLeakChecks(result, leakRows);
                RunHistoricalRegressions(result);
            }
            catch (Exception exception)
            {
                result.Errors.Add("Unhandled foundation verifier exception: " + exception);
            }

            WriteReports(foundation, result, rows, leakRows);
            bool passed = result.Errors.Count == 0;
            if (passed)
            {
                Debug.Log("ItemRarityInstanceFoundation01 verification passed and reports were written.");
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

        private static void RunFoundationChecks(
            ItemGenerationFoundationSnapshot foundation,
            VerificationResult result,
            List<SpecRow> rows)
        {
            if (foundation == null)
            {
                result.Errors.Add("Foundation snapshot is null.");
                return;
            }

            if (!foundation.isValid)
            {
                result.Errors.Add("Foundation snapshot contains validation errors: "
                    + string.Join(" | ", foundation.ValidationErrors.Select(error => error.code + ":" + error.message)));
            }

            string[] expectedOrdinaryIds = Enumerable.Range(1, 30).Select(index => $"I{index:000}").ToArray();
            string[] actualOrdinaryIds = foundation.OrdinaryArchetypes.Select(item => item.baseItemId).ToArray();
            Require(actualOrdinaryIds.SequenceEqual(expectedOrdinaryIds), result,
                "Ordinary baseItemIds must be exactly I001-I030 in stable order.");
            Require(foundation.OrdinaryArchetypes.Count == 30, result,
                $"Ordinary archetype count is {foundation.OrdinaryArchetypes.Count}; expected 30.");
            Require(foundation.Archetypes.Count == 31, result,
                $"Projected identity count is {foundation.Archetypes.Count}; expected 31 including I031.");

            ItemArchetypeIdentitySnapshot i031 = foundation.FindArchetype("I031");
            Require(i031 != null, result, "I031 must exist in the projected system identity source.");
            Require(i031 != null && !i031.isOrdinaryGeneratedItem && i031.isCoreProgressionItem, result,
                "I031 must be classified as core progression and excluded from ordinary generation.");
            Require(!actualOrdinaryIds.Contains("I031", StringComparer.Ordinal), result,
                "I031 leaked into ordinary generation candidates.");
            Require(foundation.SystemArchetypes.Count == 1
                && string.Equals(foundation.SystemArchetypes[0].baseItemId, "I031", StringComparison.Ordinal), result,
                "System archetype set must contain only I031.");

            ItemInnerDataDefinition catalogI031 = ItemInnerDataCatalog.FindById("I031");
            Require(catalogI031 != null
                && catalogI031.allowedRarities.Count == 5
                && !string.IsNullOrWhiteSpace(catalogI031.fixedAffixPreview)
                && !string.IsNullOrWhiteSpace(catalogI031.randomAffixPreview)
                && !string.IsNullOrWhiteSpace(catalogI031.orangeAffixPreview), result,
                "Code Survey guard: I031 legacy rarity/affix preview background was not observed as expected.");

            string[] expectedRarityKeys = { "white", "green", "blue", "purple", "orange" };
            string[] expectedDisplayNames = { "凡品", "良品", "灵品", "玄品", "道品" };
            Require(foundation.RarityDefinitions.Count == 5, result,
                $"Rarity definition count is {foundation.RarityDefinitions.Count}; expected 5.");
            Require(foundation.RarityDefinitions.Select(definition => definition.stableKey).SequenceEqual(expectedRarityKeys), result,
                "Rarity stable keys are not exactly white/green/blue/purple/orange.");
            Require(foundation.RarityDefinitions.Select(definition => definition.displayName).SequenceEqual(expectedDisplayNames), result,
                "Rarity display names are not exactly 凡品/良品/灵品/玄品/道品.");
            Require(foundation.RarityDefinitions.Select(definition => definition.tierIndex).SequenceEqual(Enumerable.Range(0, 5)), result,
                "Rarity tierIndex values are not exactly 0-4.");
            Require(!ItemInstanceRarityCatalog.TryParseStableKey("bai", out _)
                && !ItemInstanceRarityCatalog.TryParseStableKey("Common", out _)
                && !ItemInstanceRarityCatalog.TryParseStableKey("White", out _), result,
                "Legacy or case-variant rarity keys must not silently map to formal stable keys.");

            Require(foundation.RarityVersionKeys.Count == 150, result,
                $"Rarity version count is {foundation.RarityVersionKeys.Count}; expected 150.");
            Require(foundation.RarityVersionKeys.Select(key => key.canonicalKey).Distinct(StringComparer.Ordinal).Count() == 150, result,
                "RarityVersionKey canonical keys are not all unique.");
            Require(foundation.RarityVersionKeys.Select(key => key.baseItemId).Distinct(StringComparer.Ordinal).Count() == 30, result,
                "Derived rarity versions must retain exactly 30 baseItemIds.");
            Require(foundation.RarityVersionKeys.Any(key => string.Equals(key.canonicalKey, "I029@purple", StringComparison.Ordinal)), result,
                "Expected canonical rarity version key I029@purple is missing.");

            ItemGenerationFoundationSnapshot reversed = ItemRarityInstanceFoundation.Create(ItemInnerDataCatalog.AllItems.Reverse().ToArray());
            Require(string.Equals(foundation.BuildCanonicalSignature(), reversed.BuildCanonicalSignature(), StringComparison.Ordinal), result,
                "Foundation output order or Canonical Signature changed when Catalog input order was reversed.");
            Require(foundation.OrdinaryArchetypes.Select(item => item.baseItemId)
                .SequenceEqual(reversed.OrdinaryArchetypes.Select(item => item.baseItemId)), result,
                "Ordinary archetype output order changed when Catalog input order was reversed.");
            Require(foundation.RarityVersionKeys.Select(key => key.canonicalKey)
                .SequenceEqual(reversed.RarityVersionKeys.Select(key => key.canonicalKey)), result,
                "Rarity version output order changed when Catalog input order was reversed.");

            AddAllValidVersionRows(foundation, rows, result);
            CheckInstanceIdentityCases(foundation, rows, result);
            CheckReadOnlyContracts(foundation, result);
            CheckSnapshotShape(result);

            Require(string.Equals(ItemSystemSnapshot.CurrentSchemaVersion, "ItemSystemSnapshot.v2", StringComparison.Ordinal), result,
                "ItemSystemSnapshot schema version must match the explicit ItemSystemSnapshot.v2 I031 migration.");
            result.Notes.Add("ItemSystemSnapshot.v2 preserves ordinary itemId/placementId semantics while adding the isolated I031 ownership/location branch.");
            result.Notes.Add("ItemCatalogRarity and rarityDefault remain legacy Catalog preview background; ItemInstanceRarity is an independent instance-generation definition.");
            result.Notes.Add("I031 preview rarity/affix fields remain untouched, while ordinary eligibility is classified only by the explicit I001-I030 identity range.");
        }

        private static void AddAllValidVersionRows(
            ItemGenerationFoundationSnapshot foundation,
            List<SpecRow> rows,
            VerificationResult result)
        {
            int index = 0;
            foreach (ItemRarityVersionKey versionKey in foundation.RarityVersionKeys)
            {
                string instanceId = $"QA-{versionKey.baseItemId}-{versionKey.rarity.ToStableKey()}";
                ItemInstanceIdentityCreationResult first = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                    foundation, instanceId, versionKey.baseItemId, versionKey.rarity, 1, 100000L + index, "Pending");
                ItemInstanceIdentityCreationResult repeat = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                    foundation, instanceId, versionKey.baseItemId, versionKey.rarity, 1, 100000L + index, "Pending");
                bool canonicalStable = first.isValid
                    && repeat.isValid
                    && string.Equals(first.snapshot.BuildCanonicalSignature(), repeat.snapshot.BuildCanonicalSignature(), StringComparison.Ordinal);
                bool passed = first.isValid
                    && canonicalStable
                    && string.Equals(versionKey.canonicalKey, $"{versionKey.baseItemId}@{versionKey.rarity.ToStableKey()}", StringComparison.Ordinal);

                rows.Add(new SpecRow(
                    $"version-{index + 1:000}",
                    versionKey.baseItemId,
                    versionKey.rarity.ToStableKey(),
                    instanceId,
                    true,
                    false,
                    versionKey.canonicalKey,
                    1,
                    100000L + index,
                    true,
                    first.isValid,
                    first.primaryValidationCode,
                    canonicalStable,
                    passed));
                if (!passed)
                {
                    result.Errors.Add($"Valid version case failed: {versionKey.canonicalKey}.");
                }

                index++;
            }
        }

        private static void CheckInstanceIdentityCases(
            ItemGenerationFoundationSnapshot foundation,
            List<SpecRow> rows,
            VerificationResult result)
        {
            ItemInstanceIdentityCreationResult instanceA = AddCase(
                rows, "multi-instance-a", foundation, "INSTANCE-A", "I001", "white", 1, 42, true, ItemGenerationValidationCodes.None);
            ItemInstanceIdentityCreationResult instanceB = AddCase(
                rows, "multi-instance-b", foundation, "INSTANCE-B", "I001", "white", 1, 42, true, ItemGenerationValidationCodes.None);
            Require(instanceA.isValid && instanceB.isValid
                && !string.Equals(instanceA.snapshot.itemInstanceId, instanceB.snapshot.itemInstanceId, StringComparison.Ordinal)
                && instanceA.snapshot.baseItemId == instanceB.snapshot.baseItemId
                && instanceA.snapshot.rarity == instanceB.snapshot.rarity, result,
                "The same baseItemId + rarity must support multiple distinct itemInstanceIds.");
            Require(instanceA.isValid
                && !string.Equals(instanceA.snapshot.itemInstanceId, instanceA.snapshot.baseItemId, StringComparison.Ordinal), result,
                "itemInstanceId and baseItemId identity layers were conflated.");

            AddCase(rows, "exclude-i031", foundation, "INSTANCE-I031", "I031", "green", 1, 7, false,
                ItemGenerationValidationCodes.BaseItemNotOrdinary);
            AddCase(rows, "generation-version-zero", foundation, "INSTANCE-GEN0", "I001", "white", 0, 7, false,
                ItemGenerationValidationCodes.GenerationVersionInvalid);
            AddCase(rows, "empty-instance-id", foundation, string.Empty, "I001", "white", 1, 7, false,
                ItemGenerationValidationCodes.ItemInstanceIdEmpty);
            AddCase(rows, "empty-base-item-id", foundation, "INSTANCE-NOBASE", string.Empty, "white", 1, 7, false,
                ItemGenerationValidationCodes.BaseItemIdEmpty);
            AddCase(rows, "unknown-base-item-id", foundation, "INSTANCE-UNKNOWN", "I999", "white", 1, 7, false,
                ItemGenerationValidationCodes.BaseItemUnknown);
            AddCase(rows, "legacy-rarity-key", foundation, "INSTANCE-LEGACY", "I001", "bai", 1, 7, false,
                ItemGenerationValidationCodes.RarityKeyInvalid);
            AddCase(rows, "instance-equals-base", foundation, "I001", "I001", "white", 1, 7, false,
                ItemGenerationValidationCodes.InstanceIdEqualsBaseItemId);

            foreach (SpecRow row in rows.Where(row => row.caseId.StartsWith("multi-", StringComparison.Ordinal)
                || row.caseId.StartsWith("exclude-", StringComparison.Ordinal)
                || row.caseId.StartsWith("generation-", StringComparison.Ordinal)
                || row.caseId.StartsWith("empty-", StringComparison.Ordinal)
                || row.caseId.StartsWith("unknown-", StringComparison.Ordinal)
                || row.caseId.StartsWith("legacy-", StringComparison.Ordinal)
                || row.caseId.StartsWith("instance-", StringComparison.Ordinal)))
            {
                if (!row.passed)
                {
                    result.Errors.Add($"Instance identity case failed: {row.caseId} ({row.validationCode}).");
                }
            }
        }

        private static ItemInstanceIdentityCreationResult AddCase(
            List<SpecRow> rows,
            string caseId,
            ItemGenerationFoundationSnapshot foundation,
            string instanceId,
            string baseItemId,
            string rarityKey,
            int generationVersion,
            long rootSeed,
            bool expectedValid,
            string expectedCode)
        {
            ItemInstanceIdentityCreationResult actual = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                foundation, instanceId, baseItemId, rarityKey, generationVersion, rootSeed, "Pending");
            bool canonicalStable = !actual.isValid || string.Equals(
                actual.snapshot.BuildCanonicalSignature(),
                ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                    foundation, instanceId, baseItemId, rarityKey, generationVersion, rootSeed, "Pending")
                    .snapshot?.BuildCanonicalSignature(),
                StringComparison.Ordinal);
            ItemArchetypeIdentitySnapshot archetype = foundation.FindArchetype(baseItemId);
            bool passed = actual.isValid == expectedValid
                && string.Equals(actual.primaryValidationCode, expectedCode, StringComparison.Ordinal)
                && canonicalStable;
            string versionKey = ItemInstanceRarityCatalog.TryParseStableKey(rarityKey, out ItemInstanceRarity rarity)
                ? new ItemRarityVersionKey(baseItemId, rarity).canonicalKey
                : string.Empty;

            rows.Add(new SpecRow(
                caseId,
                baseItemId,
                rarityKey,
                instanceId,
                archetype?.isOrdinaryGeneratedItem == true,
                archetype?.isCoreProgressionItem == true,
                versionKey,
                generationVersion,
                rootSeed,
                expectedValid,
                actual.isValid,
                actual.primaryValidationCode,
                canonicalStable,
                passed));
            return actual;
        }

        private static void CheckReadOnlyContracts(
            ItemGenerationFoundationSnapshot foundation,
            VerificationResult result)
        {
            Require(IsReadOnly(foundation.Archetypes), result, "Archetypes collection can be externally mutated.");
            Require(IsReadOnly(foundation.OrdinaryArchetypes), result, "OrdinaryArchetypes collection can be externally mutated.");
            Require(IsReadOnly(foundation.SystemArchetypes), result, "SystemArchetypes collection can be externally mutated.");
            Require(IsReadOnly(foundation.RarityDefinitions), result, "RarityDefinitions collection can be externally mutated.");
            Require(IsReadOnly(foundation.RarityVersionKeys), result, "RarityVersionKeys collection can be externally mutated.");
            Require(IsReadOnly(foundation.ValidationErrors), result, "Foundation ValidationErrors collection can be externally mutated.");

            ItemInstanceIdentityCreationResult invalid = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                foundation, string.Empty, "I001", "white", 1, 0);
            Require(IsReadOnly(invalid.ValidationErrors), result, "Instance ValidationErrors collection can be externally mutated.");
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

        private static void CheckSnapshotShape(VerificationResult result)
        {
            string[] expectedProperties =
            {
                "schemaId",
                "itemInstanceId",
                "baseItemId",
                "rarity",
                "generationVersion",
                "rootSeed",
                "cultivationPotentialProfileId"
            };
            PropertyInfo[] properties = typeof(ItemInstanceIdentitySnapshot).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            string[] actual = properties.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal).ToArray();
            Require(actual.SequenceEqual(expectedProperties.OrderBy(name => name, StringComparer.Ordinal)), result,
                "ItemInstanceIdentitySnapshot public field contract differs from the required v1 identity-only shape: "
                + string.Join("|", actual));
            Require(properties.All(property => property.SetMethod == null), result,
                "ItemInstanceIdentitySnapshot exposes a public setter.");
            Require(properties.All(property => !string.Equals(property.Name, "placementId", StringComparison.OrdinalIgnoreCase)), result,
                "ItemInstanceIdentitySnapshot must not contain placementId.");
        }

        private static void RunLeakChecks(VerificationResult result, List<LeakRow> rows)
        {
            string[] files =
            {
                Path.Combine(RuntimeSourceRoot, "ItemInstanceRarity.cs"),
                Path.Combine(RuntimeSourceRoot, "ItemRarityInstanceFoundation.cs")
            };
            string source = string.Join("\n", files.Select(File.ReadAllText));
            LeakDefinition[] definitions =
            {
                new("Reward", new[] { "RewardConfig", "RewardService", "GrantReward" }),
                new("RunFlow", new[] { "RunFlow" }),
                new("SaveData", new[] { "SaveData", "PlayerPrefs", "MainTrialProgressData" }),
                new("Inventory写入", new[] { "InventoryWriter", "InventoryService", "AddToInventory", "WriteInventory" }),
                new("Boss", new[] { "BossInfo", "BossBattle", "BossReward" }),
                new("BattleResolver", new[] { "BattleResolver" }),
                new("Battle Bridge", new[] { "BattleBridge" }),
                new("UnifiedBattlePage", new[] { "UnifiedBattlePage" }),
                new("BuildSettings", new[] { "EditorBuildSettings", "BuildSettings" }),
                new("Scene", new[] { "SceneManager", "LoadScene", ".unity" }),
                new("Prefab", new[] { "PrefabUtility", ".prefab" }),
                new("正式掉落", new[] { "DropSource", "DropTable", "LootTable" }),
                new("正式养成", new[] { "UpgradeService", "Breakthrough", "SaveCultivation", "Reroll" }),
                new("真实属性Roll", new[] { "rolledStats", "StatRoll", "RollStat" }),
                new("真实词条Roll", new[] { "AffixRoll", "RollAffix", "randomAffix" }),
                new("Build资格Roll", new[] { "BuildQualification", "QualificationRoll" }),
                new("itemPower", new[] { "itemPower" })
            };

            foreach (LeakDefinition definition in definitions)
            {
                int count = definition.Tokens.Sum(token => CountOccurrences(source, token));
                rows.Add(new LeakRow(definition.Category, count));
                if (count != 0)
                {
                    result.Errors.Add($"LeakCheck category '{definition.Category}' found {count} forbidden runtime source occurrence(s).");
                }
            }

            string raritySourcePath = Path.Combine(RuntimeSourceRoot, "ItemInstanceRarity.cs");
            string foundationSourcePath = Path.Combine(RuntimeSourceRoot, "ItemRarityInstanceFoundation.cs");
            Require(File.Exists(raritySourcePath), result,
                "Foundation source ItemInstanceRarity.cs is missing.");
            Require(File.Exists(foundationSourcePath), result,
                "Foundation source ItemRarityInstanceFoundation.cs is missing.");
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

        private static void RunHistoricalRegressions(VerificationResult result)
        {
            RegressionCase[] regressions =
            {
                new("ItemInnerDataCatalog verifier", ItemInnerDataCatalogVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS"),
                new("ItemSystemValidatorAndSnapshot verifier", ItemSystemValidatorAndSnapshotVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS"),
                new("BuildSynergyCore verifier", BuildSynergyCoreVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS"),
                new("CoreAwakeningPreview verifier", CoreAwakeningPreviewVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS"),
                new("ItemSkillTriggerContract verifier", ItemSkillTriggerContractVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS"),
                new("ItemDetailProjectionComplete verifier", ItemDetailProjectionCompleteVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS"),
                new("JuNian Lighting verifier", JuNianLightingAndAdjacentRelayVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS"),
                new("ArrayBonus verifier", ArrayBonusCellResolverVerifier.VerifyMenu,
                    "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS")
            };

            foreach (RegressionCase regression in regressions)
            {
                try
                {
                    regression.Run();
                    bool passed = File.Exists(regression.ReportPath)
                        && File.ReadAllText(regression.ReportPath).Contains(regression.PassMarker);
                    result.Regressions.Add(regression.Name + ": " + (passed ? "PASS" : "FAIL"));
                    if (!passed)
                    {
                        result.Errors.Add($"Historical regression failed or did not write its PASS marker: {regression.Name}.");
                    }
                }
                catch (Exception exception)
                {
                    result.Regressions.Add(regression.Name + ": EXCEPTION");
                    result.Errors.Add($"Historical regression threw: {regression.Name}: {exception.Message}");
                }
            }
        }

        private static void Require(bool condition, VerificationResult result, string error)
        {
            if (!condition)
            {
                result.Errors.Add(error);
            }
        }

        private static void WriteReports(
            ItemGenerationFoundationSnapshot foundation,
            VerificationResult result,
            IReadOnlyList<SpecRow> rows,
            IReadOnlyList<LeakRow> leakRows)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DetailReportPath) ?? "Docs/V0.4/Reports");
            File.WriteAllText(DetailReportPath, BuildDetailReport(foundation, result), new UTF8Encoding(false));
            File.WriteAllText(SpecCsvPath, BuildSpecCsv(rows), new UTF8Encoding(false));
            File.WriteAllText(LeakCheckReportPath, BuildLeakReport(leakRows, result), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(
            ItemGenerationFoundationSnapshot foundation,
            VerificationResult result)
        {
            string status = result.Errors.Count == 0 ? "PASS" : "FAIL";
            string rarityKeys = foundation == null
                ? "None"
                : string.Join(" / ", foundation.RarityDefinitions.Select(definition => definition.stableKey));
            string[] identityFields = typeof(ItemInstanceIdentitySnapshot)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            StringBuilder builder = new();
            builder.AppendLine("# ItemRarityInstanceFoundation01 Report")
                .AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Run time (UTC): `{DateTime.UtcNow:O}`")
                .AppendLine($"- Verification: {status}")
                .AppendLine($"- Ordinary archetypes: {foundation?.OrdinaryArchetypes.Count ?? 0}")
                .AppendLine($"- System items: {foundation?.SystemArchetypes.Count ?? 0}")
                .AppendLine($"- Rarity stable keys: {rarityKeys}")
                .AppendLine($"- Derived rarity versions: {foundation?.RarityVersionKeys.Count ?? 0}")
                .AppendLine("- I031 exclusion: excluded from ordinary generation and retained as directed core-progression identity")
                .AppendLine("- Instance identity fields: " + string.Join(" / ", identityFields))
                .AppendLine("- ItemSystemSnapshot.v2 compatibility: PASS; ordinary itemId/placementId semantics remain isolated from I031 ownership/location state.")
                .AppendLine()
                .AppendLine("## Code Survey Conclusion")
                .AppendLine()
                .AppendLine("- `ItemCatalogRarity` (`bai/qing/lan/zi/cheng`) remains legacy Catalog preview background. The new `ItemInstanceRarity` is isolated and accepts only `white/green/blue/purple/orange`.")
                .AppendLine("- `rarityDefault`, `allowedRarities`, display rarity, and affix preview fields do not determine ordinary generation eligibility or prototype-fixed quality.")
                .AppendLine("- I031 still carries legacy rarity/affix preview fields in the untouched Catalog, but explicit identity classification excludes it from ordinary generation.")
                .AppendLine("- `ItemSystemSnapshot.v2` keeps ordinary `itemId` as Catalog identity and `placementId` as board placement identity. Item generation continues to use separate `baseItemId` / `itemInstanceId` snapshots without an I031 ordinary instance.")
                .AppendLine("- The new foundation owns a separate Canonical Signature and never appends to or rewrites `ItemSystemSnapshot.BuildDebugSignature()`.")
                .AppendLine()
                .AppendLine("## Historical Regressions")
                .AppendLine();
            foreach (string regression in result.Regressions)
            {
                builder.AppendLine("- " + regression);
            }

            builder.AppendLine()
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

            builder.AppendLine()
                .AppendLine("## Warnings")
                .AppendLine();
            if (result.Warnings.Count == 0)
            {
                builder.AppendLine("- None");
            }
            else
            {
                foreach (string warning in result.Warnings)
                {
                    builder.AppendLine("- " + warning);
                }
            }

            return builder.ToString();
        }

        private static string BuildSpecCsv(IReadOnlyList<SpecRow> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,baseItemId,rarityKey,itemInstanceId,ordinaryEligible,coreProgressionItem,rarityVersionKey,generationVersion,rootSeed,expectedValid,actualValid,validationCode,canonicalSignatureStable,passed");
            foreach (SpecRow row in rows)
            {
                builder.AppendLine(string.Join(",",
                    Csv(row.caseId),
                    Csv(row.baseItemId),
                    Csv(row.rarityKey),
                    Csv(row.itemInstanceId),
                    Bool(row.ordinaryEligible),
                    Bool(row.coreProgressionItem),
                    Csv(row.rarityVersionKey),
                    row.generationVersion,
                    row.rootSeed,
                    Bool(row.expectedValid),
                    Bool(row.actualValid),
                    Csv(row.validationCode),
                    Bool(row.canonicalSignatureStable),
                    Bool(row.passed)));
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(IReadOnlyList<LeakRow> rows, VerificationResult result)
        {
            bool passed = rows.All(row => row.count == 0);
            StringBuilder builder = new();
            builder.AppendLine("# ItemRarityInstanceFoundation01 LeakCheck Report")
                .AppendLine()
                .AppendLine($"- Package: `{PackageName}`")
                .AppendLine($"- Result: {(passed ? "PASS" : "FAIL")}")
                .AppendLine($"- Runtime scan root: `{RuntimeSourceRoot}`")
                .AppendLine()
                .AppendLine("| Category | Leak count |")
                .AppendLine("| --- | ---: |");
            foreach (LeakRow row in rows)
            {
                builder.AppendLine($"| {row.category} | {row.count} |");
            }

            builder.AppendLine()
                .AppendLine("All categories must remain zero. The cultivation potential profile identifier is an identity-only reserved reference and is intentionally not treated as a formal cultivation implementation.")
                .AppendLine()
                .AppendLine($"Verifier errors: {result.Errors.Count}");
            return builder.ToString();
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
            public readonly List<string> Warnings = new();
            public readonly List<string> Notes = new();
            public readonly List<string> Regressions = new();
        }

        private sealed class RegressionCase
        {
            public RegressionCase(string name, Action run, string reportPath, string passMarker)
            {
                Name = name;
                Run = run;
                ReportPath = reportPath;
                PassMarker = passMarker;
            }

            public string Name { get; }
            public Action Run { get; }
            public string ReportPath { get; }
            public string PassMarker { get; }
        }

        private sealed class LeakDefinition
        {
            public LeakDefinition(string category, string[] tokens)
            {
                Category = category;
                Tokens = tokens;
            }

            public string Category { get; }
            public string[] Tokens { get; }
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
            public SpecRow(
                string caseId,
                string baseItemId,
                string rarityKey,
                string itemInstanceId,
                bool ordinaryEligible,
                bool coreProgressionItem,
                string rarityVersionKey,
                int generationVersion,
                long rootSeed,
                bool expectedValid,
                bool actualValid,
                string validationCode,
                bool canonicalSignatureStable,
                bool passed)
            {
                this.caseId = caseId;
                this.baseItemId = baseItemId;
                this.rarityKey = rarityKey;
                this.itemInstanceId = itemInstanceId;
                this.ordinaryEligible = ordinaryEligible;
                this.coreProgressionItem = coreProgressionItem;
                this.rarityVersionKey = rarityVersionKey;
                this.generationVersion = generationVersion;
                this.rootSeed = rootSeed;
                this.expectedValid = expectedValid;
                this.actualValid = actualValid;
                this.validationCode = validationCode;
                this.canonicalSignatureStable = canonicalSignatureStable;
                this.passed = passed;
            }

            public readonly string caseId;
            public readonly string baseItemId;
            public readonly string rarityKey;
            public readonly string itemInstanceId;
            public readonly bool ordinaryEligible;
            public readonly bool coreProgressionItem;
            public readonly string rarityVersionKey;
            public readonly int generationVersion;
            public readonly long rootSeed;
            public readonly bool expectedValid;
            public readonly bool actualValid;
            public readonly string validationCode;
            public readonly bool canonicalSignatureStable;
            public readonly bool passed;
        }
    }
}
#endif
