using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EditorTools.ItemGeneration;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemFullDetailBuildSandboxWorkbenchRegressionRunner
    {
        public const string Marker = "ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_REGRESSION_PASS";
        private const string IntegrityReportPath = "Docs/V0.4/Reports/ItemFullDetailBuildSandboxWorkbenchRegressionIntegrityReport.md";
        private static readonly string[] ProtectedRelativePaths =
        {
            "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab",
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity",
            "ProjectSettings/EditorBuildSettings.asset"
        };

        private static readonly List<ProtectedHashSample> ProtectedHashSamples = new();

        public static void RunBatch()
        {
            ProtectedHashSamples.Clear();
            try
            {
                Run();
                WriteIntegrityReport(true, string.Empty);
                Debug.Log(Marker);
            }
            catch (Exception exception)
            {
                WriteIntegrityReport(false, exception.Message);
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Run()
        {
            RunVerifier("ItemInnerDataCatalogVerifier", ItemInnerDataCatalogVerifier.VerifyMenu);
            RunVerifier("ItemGridPlacementAndEyeRuleVerifier", ItemGridPlacementAndEyeRuleVerifier.VerifyMenu);
            RunVerifier("JuNianLightingAndAdjacentRelayVerifier", JuNianLightingAndAdjacentRelayVerifier.VerifyMenu);
            RunVerifier("ArrayBonusCellResolverVerifier", ArrayBonusCellResolverVerifier.VerifyMenu);
            RunVerifier("BuildSynergyCoreVerifier", BuildSynergyCoreVerifier.VerifyMenu);
            RunVerifier("CoreAwakeningPreviewVerifier", CoreAwakeningPreviewVerifier.VerifyMenu);
            RunVerifier("ItemSkillTriggerContractVerifier", ItemSkillTriggerContractVerifier.VerifyMenu);
            RunVerifier("ItemSystemValidatorAndSnapshotVerifier", ItemSystemValidatorAndSnapshotVerifier.VerifyMenu);
            RunVerifier("ItemDetailProjectionCompleteVerifier", ItemDetailProjectionCompleteVerifier.VerifyMenu);

            RunVerifier("ItemRarityInstanceFoundationVerifier", ItemRarityInstanceFoundationVerifier.VerifyMenu);
            RunVerifier("ItemStatRangeSchemaVerifier", ItemStatRangeSchemaVerifier.VerifyMenu);
            RunVerifier("ItemCorePotentialAndBuildEligibilitySchemaVerifier", ItemCorePotentialAndBuildEligibilitySchemaVerifier.VerifyMenu);
            RunVerifier("ItemAffixPoolAndRangeSchemaVerifier", ItemAffixPoolAndRangeSchemaVerifier.VerifyMenu);
            RunVerifier("ItemInstanceRollEngineVerifier", ItemInstanceRollEngineVerifier.VerifyMenu);
            RunVerifier("ItemDropGenerationSandboxVerifier", ItemDropGenerationSandboxVerifier.VerifyMenu);
            RunVerifier("ItemGenerationSimulationValidator", ItemGenerationSimulationValidator.VerifyMenu);
            RunVerifier("ItemInstanceProjectionContractVerifier", ItemInstanceProjectionContractVerifier.VerifyMenu);

            List<string> errors = new();
            RequireReport(errors, "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleReport.md", "- Verification: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS");
            RequireReport(errors, "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS");

            RequireCsv(errors, "Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv", 159);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv", 39);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaSpec.csv", 69);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaSpec.csv", 88);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemInstanceRollEngineSpec.csv", 72);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemDropGenerationSandboxSpec.csv", 87);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemDropGenerationSandboxDeterminism.csv", 6);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemGenerationSimulationValidatorSpec.csv", 65);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemGenerationSimulationDistribution.csv", 46);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemGenerationSimulationDeterminism.csv", 12);
            RequireCsv(errors, "Docs/V0.4/Reports/ItemInstanceProjectionContractSpec.csv", 156);

            if (errors.Count > 0)
            {
                throw new InvalidOperationException(string.Join(" | ", errors));
            }
        }

        private static void RunVerifier(string verifierName, Action verifier)
        {
            Dictionary<string, string> before = SnapshotProtectedHashes();
            Debug.Log("Protected hash gate BEFORE " + verifierName + ": " + FormatSnapshot(before));
            verifier();
            Dictionary<string, string> after = SnapshotProtectedHashes();
            Debug.Log("Protected hash gate AFTER " + verifierName + ": " + FormatSnapshot(after));

            List<string> mutations = new();
            foreach (string path in ProtectedRelativePaths)
            {
                string beforeHash = before.TryGetValue(path, out string left) ? left : "MISSING";
                string afterHash = after.TryGetValue(path, out string right) ? right : "MISSING";
                bool pass = string.Equals(beforeHash, afterHash, StringComparison.Ordinal);
                ProtectedHashSamples.Add(new ProtectedHashSample(verifierName, path, beforeHash, afterHash, pass));
                if (!pass)
                {
                    mutations.Add($"{path} {beforeHash} -> {afterHash}");
                }
            }

            if (mutations.Count > 0)
            {
                throw new InvalidOperationException("Protected file hash changed during "
                    + verifierName + ": " + string.Join(" | ", mutations));
            }
        }

        private static Dictionary<string, string> SnapshotProtectedHashes()
        {
            Dictionary<string, string> hashes = new(StringComparer.Ordinal);
            foreach (string path in ProtectedRelativePaths)
            {
                hashes[path] = File.Exists(path) ? Sha256(path) : "MISSING";
            }
            return hashes;
        }

        private static string FormatSnapshot(IReadOnlyDictionary<string, string> hashes)
        {
            return string.Join("; ", ProtectedRelativePaths.Select(path =>
                path + "=" + (hashes.TryGetValue(path, out string value) ? value : "MISSING")));
        }

        private static string Sha256(string path)
        {
            using SHA256 hash = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static void WriteIntegrityReport(bool passed, string error)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(IntegrityReportPath) ?? "Docs/V0.4/Reports");
            StringBuilder builder = new();
            builder.AppendLine("# ItemFullDetailBuildSandboxWorkbench Regression Integrity Report")
                .AppendLine()
                .AppendLine("- Status: **" + (passed ? "PASS" : "FAIL") + "**")
                .AppendLine("- Protected files: ItemDetailPanel.prefab, Scene_TalismanBag_V04_ItemSandbox.unity, EditorBuildSettings.asset")
                .AppendLine("- Rule: every historical verifier must leave all protected hashes unchanged.");
            if (!string.IsNullOrWhiteSpace(error))
            {
                builder.AppendLine("- Error: `" + error.Replace("`", "'") + "`");
            }

            builder.AppendLine()
                .AppendLine("## Hash Gate Samples")
                .AppendLine()
                .AppendLine("| Verifier | Path | Before | After | Status |")
                .AppendLine("| --- | --- | --- | --- | --- |");
            foreach (ProtectedHashSample sample in ProtectedHashSamples)
            {
                builder.AppendLine("| " + sample.verifierName
                    + " | `" + sample.path + "`"
                    + " | `" + sample.beforeHash + "`"
                    + " | `" + sample.afterHash + "`"
                    + " | " + (sample.pass ? "PASS" : "FAIL") + " |");
            }
            File.WriteAllText(IntegrityReportPath, builder.ToString(), new UTF8Encoding(false));
        }

        private static void RequireReport(List<string> errors, string path, string marker)
        {
            if (!File.Exists(path) || !File.ReadAllText(path).Contains(marker, StringComparison.Ordinal))
            {
                errors.Add(path + " missing marker " + marker);
            }
        }

        private static void RequireCsv(List<string> errors, string path, int expectedRows)
        {
            if (!File.Exists(path))
            {
                errors.Add(path + " missing");
                return;
            }

            string[] lines = File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            int rows = Math.Max(0, lines.Length - 1);
            int failures = lines.Skip(1).Count(line =>
            {
                string last = line.Substring(line.LastIndexOf(',') + 1).Trim().Trim('"');
                return string.Equals(last, "FAIL", StringComparison.OrdinalIgnoreCase);
            });
            if (rows != expectedRows || failures != 0)
            {
                errors.Add($"{path} expected {expectedRows}/0fail actual {rows}/{failures}fail");
            }
        }

        private readonly struct ProtectedHashSample
        {
            public ProtectedHashSample(string verifierName, string path, string beforeHash, string afterHash, bool pass)
            {
                this.verifierName = verifierName;
                this.path = path;
                this.beforeHash = beforeHash;
                this.afterHash = afterHash;
                this.pass = pass;
            }

            public readonly string verifierName;
            public readonly string path;
            public readonly string beforeHash;
            public readonly string afterHash;
            public readonly bool pass;
        }
    }
}
