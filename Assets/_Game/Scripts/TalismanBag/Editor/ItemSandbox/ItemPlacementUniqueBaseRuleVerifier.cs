#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemPlacementUniqueBaseRuleVerifier
    {
        private const string ReportPath = "Docs/V0.4/Reports/ItemPlacementUniqueBaseRuleReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemPlacementUniqueBaseRuleSpec.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemPlacementUniqueBaseRuleLeakCheckReport.md";
        private const string PassMarker = "ITEM_PLACEMENT_UNIQUE_BASE_RULE01_PASS";

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxV04BoardFullDetailAdapter.cs"
        };

        private static readonly string[] ProtectedPaths =
        {
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity",
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity",
            "Assets/_Game/Prefabs",
            "ProjectSettings/EditorBuildSettings.asset"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "BattleContract", "BattleBridge", "UnifiedBattlePage", "V02RunFlow", "V03RunFlow",
            "SaveData", "RewardConfig", "BossController", "DropTable", "EditorBuildSettings.scenes ="
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemPlacementUniqueBaseRule01/Verify")]
        public static void VerifyMenu()
        {
            Verification verification = Verify();
            if (verification.Rows.Any(row => !row.pass))
            {
                throw new InvalidOperationException(string.Join(" | ", verification.Rows.Where(row => !row.pass).Select(row => row.id + ": " + row.evidence)));
            }
        }

        public static void VerifyBatch()
        {
            try
            {
                Verification verification = Verify();
                if (verification.Rows.All(row => row.pass))
                {
                    Debug.Log(PassMarker);
                    EditorApplication.Exit(0);
                }
                else
                {
                    foreach (Row row in verification.Rows.Where(row => !row.pass))
                    {
                        Debug.LogError(row.id + ": " + row.evidence);
                    }
                    EditorApplication.Exit(1);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static Verification Verify()
        {
            Verification result = new();
            Dictionary<string, string> protectedBefore = ProtectedPaths.ToDictionary(path => path, HashPath, StringComparer.Ordinal);
            ItemBalanceWorkbenchCatalog workbench = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                ItemSandboxDetailUiSceneBuilder.ItemBalanceWorkbenchCatalogPath);
            Add(result, "WORKBENCH_CATALOG", "fixture", "Item Sandbox workbench catalog exists.", workbench != null,
                ItemSandboxDetailUiSceneBuilder.ItemBalanceWorkbenchCatalogPath);

            if (workbench != null)
            {
                GameObject providerObject = new("ItemPlacementUniqueBaseRuleVerifierProvider");
                ItemInnerDataCatalogProvider catalogProvider = providerObject.AddComponent<ItemInnerDataCatalogProvider>();
                try
                {
                    ItemBalanceCandidateDetailSandboxAdapter adapter = new(workbench, catalogProvider);
                    VerifySandboxRules(result, adapter, catalogProvider);
                    VerifySnapshotAndBuildRules(result);
                    VerifyLegalBuild6(result, adapter, catalogProvider);
                    VerifyReadableFeedback(result);
                }
                finally
                {
                    Object.DestroyImmediate(providerObject);
                }
            }

            VerifyRuntimeScope(result);
            foreach (string path in ProtectedPaths)
            {
                string after = HashPath(path);
                Add(result, "PROTECTED_" + Sanitize(path), "leak",
                    "Verifier does not rewrite protected Scene/Prefab/BuildSettings content.",
                    string.Equals(protectedBefore[path], after, StringComparison.Ordinal),
                    "before=" + protectedBefore[path] + "; after=" + after);
            }

            WriteReports(result);
            AssetDatabase.Refresh();
            return result;
        }

        private static void VerifySandboxRules(
            Verification result,
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemInnerDataCatalogProvider catalogProvider)
        {
            ItemFullDetailBuildSandboxWorkbenchSession rarity = NewSession(adapter, catalogProvider);
            ItemFullDetailWorkbenchInstance white = rarity.CreateInstance("I001", "white", 710001L);
            ItemFullDetailWorkbenchInstance orange = rarity.CreateInstance("I001", "orange", 710002L);
            rarity.SelectJuNian();
            bool raritySource = rarity.PlaceSelected(new Vector2Int(0, 0));
            rarity.SelectInstance(white?.itemInstanceId);
            bool rarityFirst = rarity.PlaceSelected(new Vector2Int(1, 0));
            rarity.SelectInstance(orange?.itemInstanceId);
            bool raritySecond = rarity.PlaceSelected(new Vector2Int(2, 0), out ItemFullDetailPlacementFailureReason rarityReason);
            Add(result, "SAME_BASE_DIFFERENT_RARITY_REJECTED", "sandbox",
                "I001 with different rarities may be owned but cannot be co-placed.",
                white != null && orange != null && white.itemInstanceId != orange.itemInstanceId
                    && raritySource && rarityFirst && !raritySecond
                    && rarityReason == ItemFullDetailPlacementFailureReason.DuplicateBaseItemPlaced
                    && rarity.Placements.Count(value => value.baseItemId == "I001") == 1,
                $"white={white?.itemInstanceId}; orange={orange?.itemInstanceId}; second={raritySecond}/{rarityReason}");

            ItemFullDetailBuildSandboxWorkbenchSession seed = NewSession(adapter, catalogProvider);
            ItemFullDetailWorkbenchInstance seedA = seed.CreateInstance("I001", "orange", 720001L);
            ItemFullDetailWorkbenchInstance seedB = seed.CreateInstance("I001", "orange", 720002L);
            seed.SelectJuNian();
            bool seedSource = seed.PlaceSelected(new Vector2Int(0, 0));
            seed.SelectInstance(seedA?.itemInstanceId);
            bool seedFirst = seed.PlaceSelected(new Vector2Int(1, 0));
            seed.SelectInstance(seedB?.itemInstanceId);
            bool seedSecond = seed.PlaceSelected(new Vector2Int(2, 0), out ItemFullDetailPlacementFailureReason seedReason);
            Add(result, "SAME_BASE_DIFFERENT_SEED_REJECTED", "sandbox",
                "I001 with different seeds cannot be co-placed.",
                seedA != null && seedB != null && seedA.rootSeed != seedB.rootSeed
                    && seedSource && seedFirst && !seedSecond
                    && seedReason == ItemFullDetailPlacementFailureReason.DuplicateBaseItemPlaced,
                $"seeds={seedA?.rootSeed}|{seedB?.rootSeed}; second={seedSecond}/{seedReason}");

            ItemFullDetailBuildSandboxWorkbenchSession distinct = NewSession(adapter, catalogProvider);
            ItemFullDetailWorkbenchInstance i001 = distinct.CreateInstance("I001", "orange", 730001L);
            ItemFullDetailWorkbenchInstance i002 = distinct.CreateInstance("I002", "orange", 730002L);
            distinct.SelectJuNian();
            bool distinctSource = distinct.PlaceSelected(new Vector2Int(0, 0));
            distinct.SelectInstance(i001?.itemInstanceId);
            bool firstDistinct = distinct.PlaceSelected(new Vector2Int(1, 0));
            distinct.SelectInstance(i002?.itemInstanceId);
            bool secondDistinct = distinct.PlaceSelected(new Vector2Int(2, 0), out ItemFullDetailPlacementFailureReason distinctReason);
            Add(result, "DIFFERENT_BASE_ALLOWED", "sandbox",
                "I001 and I002 can be placed together when board geometry is legal.",
                distinctSource && firstDistinct && secondDistinct
                    && distinctReason == ItemFullDetailPlacementFailureReason.None
                    && distinct.Placements.Count(value => !value.isJuNian) == 2,
                $"I001={firstDistinct}; I002={secondDistinct}/{distinctReason}");

            ItemFullDetailBuildSandboxWorkbenchSession juNian = NewSession(adapter, catalogProvider);
            juNian.SelectJuNian();
            bool firstSource = juNian.PlaceSelected(new Vector2Int(0, 0));
            juNian.SelectJuNian();
            bool secondSource = juNian.PlaceSelected(new Vector2Int(4, 4), out ItemFullDetailPlacementFailureReason juNianReason);
            Add(result, "I031_UNIQUE_SOURCE_RETAINED", "sandbox",
                "I031 keeps its existing unique-source rejection.",
                firstSource && !secondSource && juNianReason == ItemFullDetailPlacementFailureReason.DuplicateJuNian,
                $"first={firstSource}; second={secondSource}/{juNianReason}");
        }

        private static void VerifySnapshotAndBuildRules(Verification result)
        {
            ItemSystemSnapshot duplicate = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(new[]
                {
                    Placement("P_SOURCE", "I031", 0, 0),
                    Placement("P_I001_A", "I001", 1, 0),
                    Placement("P_I001_B", "I001", 2, 0)
                }));
            ItemSystemValidationError duplicateError = duplicate.validationErrors.FirstOrDefault(error =>
                string.Equals(error.code, ItemSystemValidationCodes.DuplicateBaseItemPlaced, StringComparison.Ordinal));
            bool metadata = duplicateError != null
                && duplicateError.itemId == "I001"
                && duplicateError.placementId == "P_I001_B"
                && duplicateError.message.Contains("P_I001_A", StringComparison.Ordinal)
                && duplicateError.message.Contains("baseItemId/itemId", StringComparison.Ordinal);
            Add(result, "SNAPSHOT_DUPLICATE_ERROR_METADATA", "snapshot",
                "Provider output is invalid and reports DUPLICATE_BASE_ITEM_PLACED with base item and placement ids.",
                !duplicate.isValid && metadata,
                duplicateError?.ToDiagnosticString() ?? "missing");

            ItemSystemSnapshot forged = new(
                duplicate.boardSize,
                duplicate.eyeCell,
                duplicate.ArrayBonusCells,
                duplicate.catalogItems,
                duplicate.placements,
                duplicate.LitRangeCells,
                duplicate.lightingResults,
                duplicate.arrayBonusResults,
                duplicate.buildSnapshot,
                duplicate.awakeningResults,
                duplicate.skillMonitorSnapshot,
                duplicate.selectedMainBuildId,
                duplicate.selectedMainBuildIsExplicit,
                duplicate.selectedMainBuildSource,
                Array.Empty<ItemSystemValidationError>());
            IReadOnlyList<ItemSystemValidationError> forgedErrors = new ItemSystemValidator().ValidateSnapshot(forged);
            ItemSystemValidationError forgedDuplicate = forgedErrors.FirstOrDefault(error =>
                string.Equals(error.code, ItemSystemValidationCodes.DuplicateBaseItemPlaced, StringComparison.Ordinal));
            Add(result, "FORGED_SNAPSHOT_VALIDATOR_REJECTS", "validator",
                "Standalone validator rejects a manually constructed duplicate-base snapshot.",
                forged.isValid && forgedDuplicate != null
                    && forgedDuplicate.itemId == "I001"
                    && forgedDuplicate.placementId == "P_I001_B",
                forgedDuplicate?.ToDiagnosticString() ?? "missing");

            ItemSystemBuildTrackSnapshot track = duplicate.buildSnapshot.FindFaMenBuild("famen:zhenlei");
            ItemSystemBuildItemSnapshot duplicateBuildItem = duplicate.buildSnapshot.FindPlacementResult("P_I001_B");
            Add(result, "DUPLICATE_BASE_CANNOT_BUILD2", "build",
                "A duplicate I001 is excluded and cannot turn one legal base item into Build2.",
                track != null && track.litItemCount == 1 && track.activeStagePieceCount == 0
                    && duplicateBuildItem != null && !duplicateBuildItem.countedInBuild
                    && track.SourcePlacementIds.SequenceEqual(new[] { "P_I001_A" }),
                $"count={track?.litItemCount}; activeStage={track?.activeStagePieceCount}; duplicateCounted={duplicateBuildItem?.countedInBuild}");
        }

        private static void VerifyLegalBuild6(
            Verification result,
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemInnerDataCatalogProvider catalogProvider)
        {
            ItemFullDetailBuildSandboxWorkbenchSession session = NewSession(adapter, catalogProvider);
            bool loaded = session.LoadValidationLayout(out IReadOnlyList<long> seeds);
            string[] placedBaseIds = session.Placements.Where(value => !value.isJuNian)
                .Select(value => value.baseItemId).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            ItemBuildTrackResult build6 = session.Snapshot?.build?.FindFaMenBuild("famen:zhenlei");
            bool pass = loaded
                && placedBaseIds.SequenceEqual(Enumerable.Range(1, 6).Select(index => $"I{index:000}"))
                && placedBaseIds.Distinct(StringComparer.Ordinal).Count() == 6
                && session.Snapshot.placementSnapshot.isValid
                && build6?.litItemCount == 6
                && build6.build6Active;
            Add(result, "LEGAL_BUILD6_USES_DISTINCT_BASES", "build",
                "The legal validation Build6 uses I001-I006 exactly once each.", pass,
                $"ids={string.Join("|", placedBaseIds)}; seeds={string.Join("|", seeds)}; count={build6?.litItemCount}; Build6={build6?.build6Active}");
        }

        private static void VerifyReadableFeedback(Verification result)
        {
            string feedback = ItemSandboxV04BoardFullDetailAdapter.DescribePlacementFailure(
                ItemFullDetailPlacementFailureReason.DuplicateBaseItemPlaced, "I001");
            Add(result, "READABLE_DUPLICATE_FEEDBACK", "feedback",
                "Sandbox feedback names the duplicate base id and explains the one-per-layout rule.",
                feedback.Contains("I001", StringComparison.Ordinal)
                    && feedback.Contains("重复上阵", StringComparison.Ordinal)
                    && feedback.Contains("只能放置 1 件", StringComparison.Ordinal)
                    && feedback.Contains("Seed", StringComparison.Ordinal),
                feedback);
        }

        private static void VerifyRuntimeScope(Verification result)
        {
            foreach (string path in RuntimeSourcePaths)
            {
                if (!File.Exists(path))
                {
                    Add(result, "SOURCE_" + Sanitize(path), "leak", "Scoped runtime source exists.", false, "missing");
                    continue;
                }

                string source = File.ReadAllText(path, Encoding.UTF8);
                string[] findings = ForbiddenRuntimeTokens.Where(token => source.Contains(token, StringComparison.Ordinal)).ToArray();
                Add(result, "SCOPE_" + Sanitize(path), "leak",
                    "Scoped runtime source does not connect forbidden formal systems.",
                    findings.Length == 0, findings.Length == 0 ? "clean" : string.Join("|", findings));
            }
        }

        private static ItemFullDetailBuildSandboxWorkbenchSession NewSession(
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemInnerDataCatalogProvider provider)
        {
            return new ItemFullDetailBuildSandboxWorkbenchSession(adapter, provider);
        }

        private static ItemSystemPlacementInput Placement(string placementId, string itemId, int x, int y)
        {
            return new ItemSystemPlacementInput(placementId, itemId, new Vector2Int(x, y), 0);
        }

        private static void Add(Verification result, string id, string area, string expectation, bool pass, string evidence)
        {
            result.Rows.Add(new Row(id, area, expectation, pass, evidence ?? string.Empty));
        }

        private static void WriteReports(Verification result)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            int passed = result.Rows.Count(row => row.pass);
            bool allPass = passed == result.Rows.Count;
            UTF8Encoding encoding = new(false);

            StringBuilder report = new();
            report.AppendLine("# ItemPlacementUniqueBaseRule01 Report").AppendLine()
                .AppendLine("- Package: `ItemPlacementUniqueBaseRule01`")
                .AppendLine("- Guard receipt target: `GUARD_PASS_ITEMPLACEMENTUNIQUEBASERULE01`")
                .AppendLine("- Result: **" + (allPass ? "PASS" : "FAIL") + "**")
                .AppendLine("- Checks: " + passed + "/" + result.Rows.Count).AppendLine()
                .AppendLine("## Rule").AppendLine()
                .AppendLine("- Inventory ownership may contain multiple ordinary instances with the same baseItemId/itemId.")
                .AppendLine("- One placed layout may contain only one ordinary item per baseItemId/itemId, independent of rarity, affixes, Seed, or itemInstanceId.")
                .AppendLine("- I031 remains governed by its existing unique-source rule.")
                .AppendLine("- Invalid duplicate bases are reported as `DUPLICATE_BASE_ITEM_PLACED` and excluded from Build counts.").AppendLine()
                .AppendLine("## Checks").AppendLine();
            foreach (Row row in result.Rows)
            {
                report.AppendLine("- " + (row.pass ? "PASS" : "FAIL") + " `" + row.id + "` — " + row.expectation + " Evidence: " + row.evidence);
            }
            report.AppendLine().AppendLine("## Legal Build6")
                .AppendLine("- I031 `(0,0)`; distinct orange I001-I006 anchors `(1,0) (2,0) (4,0) (2,1) (4,2) (2,3)`.")
                .AppendLine("- The verifier discovers deterministic qualifying seeds and records them in `LEGAL_BUILD6_USES_DISTINCT_BASES`.").AppendLine()
                .AppendLine("## Scope")
                .AppendLine("- Item Sandbox / Item System only. No formal V0.4 Battle, V0.3 RunFlow, SaveData, Reward, Boss, drop, cultivation, or BuildSettings integration.")
                .AppendLine("- Scene, Prefab, and BuildSettings protected hashes remain unchanged during verification.").AppendLine()
                .AppendLine("## Marker").AppendLine(allPass ? PassMarker : "ITEM_PLACEMENT_UNIQUE_BASE_RULE01_FAIL");
            File.WriteAllText(ReportPath, report.ToString(), encoding);

            StringBuilder csv = new("caseId,area,status,expectation,evidence\n");
            foreach (Row row in result.Rows)
            {
                csv.Append(Csv(row.id)).Append(',').Append(Csv(row.area)).Append(',')
                    .Append(row.pass ? "PASS" : "FAIL").Append(',')
                    .Append(Csv(row.expectation)).Append(',').Append(Csv(row.evidence)).Append('\n');
            }
            File.WriteAllText(SpecPath, csv.ToString(), encoding);

            Row[] leakRows = result.Rows.Where(row => row.area == "leak").ToArray();
            StringBuilder leak = new();
            leak.AppendLine("# ItemPlacementUniqueBaseRule01 Leak Check Report").AppendLine()
                .AppendLine("- Result: **" + (leakRows.All(row => row.pass) ? "PASS" : "FAIL") + "**")
                .AppendLine("- Runtime scope: Item placement/snapshot/Build/Sandbox feedback only.")
                .AppendLine("- Scene / Prefab / BuildSettings writes: none.")
                .AppendLine("- Formal Battle / bridge / RunFlow / Save / Reward / Boss / drop / cultivation connections: none.").AppendLine();
            foreach (Row row in leakRows)
            {
                leak.AppendLine("- " + (row.pass ? "PASS" : "FAIL") + " `" + row.id + "`: " + row.evidence);
            }
            File.WriteAllText(LeakPath, leak.ToString(), encoding);
        }

        private static string HashPath(string relativePath)
        {
            string absolute = Path.GetFullPath(relativePath);
            if (!File.Exists(absolute) && !Directory.Exists(absolute))
            {
                return "MISSING";
            }

            string[] files = Directory.Exists(absolute)
                ? Directory.GetFiles(absolute, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal).ToArray()
                : new[] { absolute };
            using SHA256 hash = SHA256.Create();
            string projectRoot = Directory.GetCurrentDirectory();
            foreach (string file in files)
            {
                byte[] name = Encoding.UTF8.GetBytes(file.Replace(projectRoot, string.Empty));
                hash.TransformBlock(name, 0, name.Length, name, 0);
                byte[] bytes = File.ReadAllBytes(file);
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }
            hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            return BitConverter.ToString(hash.Hash).Replace("-", string.Empty);
        }

        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        private static string Sanitize(string value) => new((value ?? string.Empty).Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray());

        private sealed class Verification
        {
            public readonly List<Row> Rows = new();
        }

        private readonly struct Row
        {
            public Row(string id, string area, string expectation, bool pass, string evidence)
            {
                this.id = id;
                this.area = area;
                this.expectation = expectation;
                this.pass = pass;
                this.evidence = evidence;
            }

            public readonly string id;
            public readonly string area;
            public readonly string expectation;
            public readonly bool pass;
            public readonly string evidence;
        }
    }
}
#endif
