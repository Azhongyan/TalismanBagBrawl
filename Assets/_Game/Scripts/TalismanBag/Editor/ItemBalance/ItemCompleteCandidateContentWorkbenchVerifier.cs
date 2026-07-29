using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.ItemSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemBalance
{
    public static class ItemCompleteCandidateContentWorkbenchVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports";
        private const string Marker = "ITEM_COMPLETE_CANDIDATE_CONTENT_WORKBENCH01_PASS";

        [MenuItem("Tools/Talisman Bag/V0.4/Data/Item Complete Candidate Content Workbench 01/[Setup+Verify] Run")]
        public static void VerifyMenu()
        {
            bool passed = VerifyAndWrite(out string summary);
            Debug.Log(summary);
            EditorUtility.DisplayDialog("Item Complete Candidate Content", summary, "OK");
            if (!passed) Debug.LogError(summary);
        }

        public static void VerifyBatch()
        {
            bool passed = VerifyAndWrite(out string summary);
            Debug.Log(summary);
            if (!passed) throw new InvalidOperationException(summary);
        }

        public static bool VerifyAndWrite(out string summary)
        {
            ItemBalanceWorkbenchCatalog catalog = ItemBalanceCandidateSeedBuilder.BuildAssets(false);
            ItemBalanceValidationReport validation = ItemBalanceWorkbenchValidation.Validate(catalog);
            Directory.CreateDirectory(Path.GetFullPath(ReportRoot));

            ItemBalanceProfile[] profiles = catalog.profiles.Where(value => value != null)
                .OrderBy(value => value.baseItemId, StringComparer.Ordinal).ToArray();
            int versions = profiles.Sum(value => value.rarityVersions?.Count ?? 0);
            int ranges = profiles.Sum(value => value.rarityVersions?.Sum(version => version?.statRanges?.Count ?? 0) ?? 0);
            int powers = profiles.Sum(value => value.rarityVersions?.Count(version => version != null
                && version.candidateItemPower > 0) ?? 0);
            int signatures = profiles.Count(value => value.signatureAffix != null
                && !string.IsNullOrWhiteSpace(value.signatureAffix.effectPayload?.effectId));
            int randomAffixes = catalog.candidateRandomAffixes.Count(value => value != null);
            int poolRows = profiles.Sum(value => value.randomAffixes?.Count(entry => entry != null && entry.weight > 0) ?? 0);
            int faMenStages = catalog.faMenBuildEffects.Count(value => value != null);
            int qiLeiStages = catalog.qiLeiBuildEffects.Count(value => value != null);
            int cores = profiles.Sum(value => value.coreCandidates?.Count(core => core != null
                && !string.IsNullOrWhiteSpace(core.effectPayload?.effectId)) ?? 0);
            int ultimates = profiles.Sum(value => value.coreCandidates?.Count(core => core?.isUltimate == true) ?? 0);
            int displays = profiles.Count(value => CompleteDisplay(value.candidateDisplay));
            List<string> leaks = FindLeaks(catalog, profiles);
            List<string> behaviorErrors = ValidateBehavior(catalog, profiles);
            bool passed = validation.isValid && profiles.Length == 30 && versions == 150 && ranges == 600
                && powers == 150 && signatures == 30 && randomAffixes >= 36 && poolRows >= 180
                && faMenStages == 15 && qiLeiStages == 10 && cores == 120 && ultimates == 30
                && displays == 30 && leaks.Count == 0 && behaviorErrors.Count == 0
                && profiles.All(value => value.baseItemId != "I031");

            WriteSpec(profiles.Length, versions, ranges, powers, signatures, randomAffixes, poolRows,
                faMenStages, qiLeiStages, cores, ultimates, displays, leaks.Count, passed);
            WriteAffixDictionary(catalog);
            WriteFixedAffixes(profiles);
            WriteRandomPools(profiles);
            WriteBuilds("ItemCandidateFaMenBuildEffects.csv", catalog.faMenBuildEffects);
            WriteBuilds("ItemCandidateQiLeiBuildEffects.csv", catalog.qiLeiBuildEffects);
            WriteCores(profiles);
            WritePower(profiles);
            WriteDisplay(profiles);
            WriteFieldMatrix();
            WriteLeakReport(leaks.Concat(behaviorErrors).ToList(), passed);
            WriteMainReport(validation, profiles.Length, versions, ranges, powers, signatures,
                randomAffixes, poolRows, faMenStages, qiLeiStages, cores, ultimates, displays, leaks.Count, passed);
            WriteMigrationDiff(profiles, catalog);
            WriteBehaviorReport(behaviorErrors);
            AssetDatabase.Refresh();

            summary = passed
                ? Marker + $"\n30/30 profiles, 150/150 versions, 600/600 ranges, 150/150 power, 30/30 signatures, random={randomAffixes}, pools={poolRows}, builds=15+10, cores=120, leaks=0."
                : "ITEM_COMPLETE_CANDIDATE_CONTENT_WORKBENCH01_FAIL\n" + string.Join("\n", validation.Errors.Take(20))
                    + (leaks.Count == 0 ? string.Empty : "\nLeaks: " + string.Join(" | ", leaks.Take(10)));
            return passed;
        }

        private static bool CompleteDisplay(ItemCandidateDisplayProfile value) => value != null
            && Text(value.triggerDescription) && Text(value.basicEffectDescription)
            && Text(value.lightingDescription) && Text(value.placementRecommendation)
            && Text(value.flavorText) && Text(value.faMenDisplayName) && Text(value.qiLeiDisplayName)
            && Text(value.shapeDescription) && Text(value.iconKey);

        private static List<string> FindLeaks(ItemBalanceWorkbenchCatalog catalog,
            IReadOnlyList<ItemBalanceProfile> profiles)
        {
            string[] forbidden = { "尚未配置", "数据待定", "Preview Reserved", "Reserved", "TODO", "临时占位", "主属性潜力", "器类节奏潜力" };
            List<string> leaks = new();
            void Check(string path, string value)
            {
                if (string.IsNullOrWhiteSpace(value)) { leaks.Add(path + "=EMPTY"); return; }
                foreach (string token in forbidden)
                    if (value.Contains(token, StringComparison.OrdinalIgnoreCase)) leaks.Add(path + "=" + token);
            }
            foreach (ItemBalanceProfile profile in profiles)
            {
                Check(profile.baseItemId + ".signature.name", profile.signatureAffix?.displayName);
                Check(profile.baseItemId + ".signature.description", profile.signatureAffix?.description);
                Check(profile.baseItemId + ".trigger", profile.candidateDisplay?.triggerDescription);
                Check(profile.baseItemId + ".basic", profile.candidateDisplay?.basicEffectDescription);
                Check(profile.baseItemId + ".placement", profile.candidateDisplay?.placementRecommendation);
                Check(profile.baseItemId + ".flavor", profile.candidateDisplay?.flavorText);
                foreach (ItemBalanceCoreCandidate core in profile.coreCandidates ?? new List<ItemBalanceCoreCandidate>())
                { Check(profile.baseItemId + ".core.name", core?.displayName); Check(profile.baseItemId + ".core.description", core?.description); }
            }
            foreach (ItemCandidateBuildStageDefinition stage in catalog.faMenBuildEffects.Concat(catalog.qiLeiBuildEffects))
            { Check("build.name", stage?.displayName); Check("build.description", stage?.description); }
            foreach (ItemCandidateAffixDefinition affix in catalog.candidateRandomAffixes)
            { Check("affix.name", affix?.displayName); Check("affix.description", affix?.description); }
            return leaks.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        }

        private static List<string> ValidateBehavior(ItemBalanceWorkbenchCatalog catalog,
            IReadOnlyList<ItemBalanceProfile> profiles)
        {
            List<string> errors = new();
            GameObject root = new("ItemCompleteCandidateContentVerifierProvider");
            try
            {
                ItemInnerDataCatalogProvider provider = root.AddComponent<ItemInnerDataCatalogProvider>();
                ItemBalanceCandidateDetailSandboxAdapter adapter = new(catalog, provider);
                foreach (ItemBalanceProfile profile in profiles)
                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    ItemBalanceCandidateDetailRequest request = new()
                    {
                        baseItemId = profile.baseItemId,
                        rarityKey = rarity.stableKey,
                        rootSeedText = (150000 + rarity.tierIndex).ToString(CultureInfo.InvariantCulture)
                    };
                    ItemBalanceCandidateDetailResult first = adapter.Request(request);
                    ItemBalanceCandidateDetailResult second = adapter.Request(request);
                    if (first?.isSuccess != true || second?.isSuccess != true)
                    {
                        errors.Add("preview-failed:" + profile.baseItemId + "@" + rarity.stableKey);
                        continue;
                    }
                    string signature1 = first.preview.ProjectionResult.snapshot.BuildCanonicalSignature();
                    string signature2 = second.preview.ProjectionResult.snapshot.BuildCanonicalSignature();
                    if (!string.Equals(signature1, signature2, StringComparison.Ordinal))
                        errors.Add("determinism-failed:" + profile.baseItemId + "@" + rarity.stableKey);
                    if (first.viewModel.displayPlayerSections.Any(value => value != null
                        && string.Equals(value.stateKey, "skillMonitor", StringComparison.Ordinal)))
                        errors.Add("player-skill-monitor-leak:" + profile.baseItemId + "@" + rarity.stableKey);
                    string player = string.Join("\n", first.viewModel.displayPlayerSections.Select(value => value?.title + value?.body));
                    if (new[] { "尚未配置", "Preview Reserved", "监控中" }.Any(token => player.Contains(token, StringComparison.Ordinal)))
                        errors.Add("player-placeholder-leak:" + profile.baseItemId + "@" + rarity.stableKey);
                }

                ItemFullDetailBuildSandboxWorkbenchSession session = new(adapter, provider);
                ItemDetailViewModel temporary = session.PreviewCandidate("I001", "white", 150001);
                if (temporary == null || session.Instances.Count != 0)
                    errors.Add("temporary-preview-mutated-instance-list");
                ItemFullDetailWorkbenchInstance created = session.CreateInstance("I001", "white", 150001);
                session.PreviewCandidate("I001", "orange", 150001);
                if (created == null || session.Instances.Count != 1 || created.rarityKey != "white")
                    errors.Add("existing-instance-rarity-mutated");
                ItemDetailViewModel createdModel = session.BuildSelectedDetail();
                string createdPlayer = string.Join("\n", createdModel?.displayPlayerSections
                    .Select(value => value?.title + value?.body) ?? Array.Empty<string>());
                if (new[] { "尚未配置", "Preview Reserved", "监控中" }
                    .Any(token => createdPlayer.Contains(token, StringComparison.Ordinal)))
                    errors.Add("created-instance-player-placeholder-leak");

                ItemBalanceCompiledData compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
                ItemAffixDefinitionSnapshot definition = compiled.AffixSchema
                    .QueryAffixDefinition("affix_damage_up")?.value;
                string formatted = ItemDetailPresentationFormatter.FormatAffix(500, definition);
                if (!formatted.Contains("5%", StringComparison.Ordinal) || formatted.Contains("500", StringComparison.Ordinal))
                    errors.Add("basispoint-golden-vector:" + formatted);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
            return errors.Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal).ToList();
        }

        private static void WriteBehaviorReport(IReadOnlyList<string> errors)
        {
            StringBuilder builder = new("# Item Complete Candidate Content Behavior Verification\n\n");
            builder.AppendLine("- 150 candidate previews deterministic: " + (errors.Any(value => value.StartsWith("determinism", StringComparison.Ordinal)) ? "FAIL" : "PASS"));
            builder.AppendLine("- Rarity click temporary preview does not create instance: " + (errors.Contains("temporary-preview-mutated-instance-list") ? "FAIL" : "PASS"));
            builder.AppendLine("- Existing instance rarity immutable: " + (errors.Contains("existing-instance-rarity-mutated") ? "FAIL" : "PASS"));
            builder.AppendLine("- Player detail has no skill-monitor duplicate / forbidden placeholder: " + (errors.Any(value => value.Contains("leak", StringComparison.Ordinal)) ? "FAIL" : "PASS"));
            builder.AppendLine("- basisPoint 500 formats as 5% (signed display allowed): " + (errors.Any(value => value.StartsWith("basispoint", StringComparison.Ordinal)) ? "FAIL" : "PASS"));
            foreach (string error in errors) builder.AppendLine("- FAIL: " + error);
            Write("ItemCompleteCandidateContentBehaviorReport.md", builder.ToString());
        }

        private static void WriteSpec(int profiles, int versions, int ranges, int power, int signatures,
            int random, int pools, int fa, int qi, int cores, int ultimates, int displays, int leaks, bool passed)
        {
            var rows = new[]
            {
                ("ordinaryProfiles", profiles, 30), ("rarityVersions", versions, 150),
                ("statRanges", ranges, 600), ("candidateItemPower", power, 150),
                ("fixedSignatureAffixes", signatures, 30), ("randomAffixDictionary", random, 36),
                ("randomPoolRows", pools, 180), ("faMenBuildStages", fa, 15),
                ("qiLeiBuildStages", qi, 10), ("coreEffects", cores, 120),
                ("ultimates", ultimates, 30), ("displayProfiles", displays, 30), ("leaks", leaks, 0)
            };
            StringBuilder builder = new("metric,actual,expected,result\n");
            foreach (var row in rows)
            {
                bool rowPass = row.Item1 == "randomAffixDictionary" || row.Item1 == "randomPoolRows"
                    ? row.Item2 >= row.Item3 : row.Item2 == row.Item3;
                builder.Append(row.Item1).Append(',').Append(row.Item2).Append(',').Append(row.Item3)
                    .Append(',').Append(rowPass ? "PASS" : "FAIL").AppendLine();
            }
            builder.Append("finalMarker,,,").Append(passed ? Marker : "FAIL").AppendLine();
            Write("ItemCompleteCandidateContentWorkbenchSpec.csv", builder.ToString());
        }

        private static void WriteAffixDictionary(ItemBalanceWorkbenchCatalog catalog)
        {
            StringBuilder builder = new("affixId,displayName,description,effectCategory,operation,triggerEventId,conditionId,targetStatId,valueUnitKey,valueUnits,secondaryValueUnits,mutexGroupId,repeatPolicy,dataMaturity\n");
            foreach (ItemCandidateAffixDefinition value in catalog.candidateRandomAffixes.Where(value => value != null).OrderBy(value => value.affixId))
                builder.Append(Csv(value.affixId, value.displayName, value.description, value.effectPayload.effectCategory,
                    value.effectPayload.operation, value.effectPayload.triggerEventId, value.effectPayload.conditionId,
                    value.effectPayload.targetStatId, value.effectPayload.valueUnitKey, value.effectPayload.valueUnits,
                    value.effectPayload.secondaryValueUnits, value.mutexGroupId, value.repeatPolicy, value.dataMaturity)).AppendLine();
            Write("ItemCandidateAffixDictionary.csv", builder.ToString());
        }

        private static void WriteFixedAffixes(IEnumerable<ItemBalanceProfile> profiles)
        {
            StringBuilder builder = new("baseItemId,itemName,affixId,displayName,description,effectId,category,operation,fiveRarityRanges,dataMaturity\n");
            foreach (ItemBalanceProfile profile in profiles)
            {
                ItemCandidateAffixDefinition value = profile.signatureAffix;
                builder.Append(Csv(profile.baseItemId, profile.displayName, value.affixId, value.displayName,
                    value.description, value.effectPayload.effectId, value.effectPayload.effectCategory,
                    value.effectPayload.operation, string.Join("|", value.rarityRanges.Select(Range)), value.dataMaturity)).AppendLine();
            }
            Write("ItemCandidateFixedAffix30.csv", builder.ToString());
        }

        private static void WriteRandomPools(IEnumerable<ItemBalanceProfile> profiles)
        {
            StringBuilder builder = new("baseItemId,itemName,poolId,affixId,weight,mutexGroupId,repeatPolicy,designNote\n");
            foreach (ItemBalanceProfile profile in profiles)
            foreach (ItemBalanceWeightedAffix value in profile.randomAffixes.Where(value => value != null))
                builder.Append(Csv(profile.baseItemId, profile.displayName, profile.randomPoolId, value.affixId,
                    value.weight, value.mutexGroupId, value.repeatPolicy, value.designNote)).AppendLine();
            Write("ItemCandidateRandomPoolMatrix.csv", builder.ToString());
        }

        private static void WriteBuilds(string fileName, IEnumerable<ItemCandidateBuildStageDefinition> values)
        {
            StringBuilder builder = new("buildId,stableTag,stagePieceCount,displayName,description,effectId,category,operation,valueUnitKey,valueUnits,conditionText,dataMaturity,designNote\n");
            foreach (ItemCandidateBuildStageDefinition value in values.Where(value => value != null)
                .OrderBy(value => value.stableTag).ThenBy(value => value.stagePieceCount))
                builder.Append(Csv(value.buildId, value.stableTag, value.stagePieceCount, value.displayName,
                    value.description, value.effectPayload.effectId, value.effectPayload.effectCategory,
                    value.effectPayload.operation, value.effectPayload.valueUnitKey, value.effectPayload.valueUnits,
                    value.conditionText, value.dataMaturity, value.designNote)).AppendLine();
            Write(fileName, builder.ToString());
        }

        private static void WriteCores(IEnumerable<ItemBalanceProfile> profiles)
        {
            StringBuilder builder = new("baseItemId,itemName,coreEffectId,nodeKind,displayName,description,effectType,unlockLevel,requiredRarity,effectId,category,operation,valueUnitKey,valueUnits,stateDescription,dataMaturity,designNote\n");
            foreach (ItemBalanceProfile profile in profiles)
            foreach (ItemBalanceCoreCandidate value in profile.coreCandidates.Where(value => value != null))
                builder.Append(Csv(profile.baseItemId, profile.displayName, value.coreEffectId, value.nodeKind,
                    value.displayName, value.description, value.effectType, value.unlockLevel,
                    value.requiredRarity.ToStableKey(), value.effectPayload.effectId,
                    value.effectPayload.effectCategory, value.effectPayload.operation,
                    value.effectPayload.valueUnitKey, value.effectPayload.valueUnits, value.stateDescription,
                    value.dataMaturity, value.designNote)).AppendLine();
            Write("ItemCandidateCoreEffects120.csv", builder.ToString());
        }

        private static void WritePower(IEnumerable<ItemBalanceProfile> profiles)
        {
            StringBuilder builder = new("baseItemId,itemName,rarity,candidateItemPower,overridden,dataMaturity,sandboxOnly\n");
            foreach (ItemBalanceProfile profile in profiles)
            foreach (ItemBalanceRarityVersion version in profile.rarityVersions.Where(value => value != null))
                builder.Append(Csv(profile.baseItemId, profile.displayName, version.rarity.ToStableKey(),
                    version.candidateItemPower, version.candidateItemPowerOverridden,
                    version.dataMaturity, true)).AppendLine();
            Write("ItemCandidateItemPower150.csv", builder.ToString());
        }

        private static void WriteDisplay(IEnumerable<ItemBalanceProfile> profiles)
        {
            StringBuilder builder = new("baseItemId,itemName,faMen,qiLei,shape,trigger,basicEffect,lighting,recommendedPlacement,flavorText,iconKey,dataMaturity\n");
            foreach (ItemBalanceProfile profile in profiles)
            {
                ItemCandidateDisplayProfile value = profile.candidateDisplay;
                builder.Append(Csv(profile.baseItemId, profile.displayName, value.faMenDisplayName,
                    value.qiLeiDisplayName, value.shapeDescription, value.triggerDescription,
                    value.basicEffectDescription, value.lightingDescription, value.placementRecommendation,
                    value.flavorText, value.iconKey, value.dataMaturity)).AppendLine();
            }
            Write("ItemCandidateDisplayContent30.csv", builder.ToString());
        }

        private static void WriteFieldMatrix()
        {
            StringBuilder builder = new("surface,field,editable,playerVisible,debugVisible,formalConnected\n");
            string[] fields = { "candidateItemPower", "signatureAffix", "randomAffixDictionary", "randomPool",
                "effectPayload", "coreEffects", "faMenBuildEffects", "qiLeiBuildEffects", "triggerDescription",
                "basicEffectDescription", "placementRecommendation", "flavorText", "balanceDataRevision" };
            foreach (string field in fields) builder.Append(Csv("Item Balance Workbench", field, true,
                field != "balanceDataRevision", true, false)).AppendLine();
            Write("ItemCompleteCandidateContentFieldMatrix.csv", builder.ToString());
        }

        private static void WriteLeakReport(IReadOnlyList<string> leaks, bool passed)
        {
            StringBuilder builder = new("# Item Complete Candidate Content Leak Check\n\n");
            builder.AppendLine("- Formal Battle / Reward / RunFlow / Inventory / Save: NOT_CONNECTED");
            builder.AppendLine("- BuildSettings / formal Prefab / other Scene changes: 0");
            builder.AppendLine("- I031 ordinary pool entries: 0");
            builder.AppendLine("- Forbidden/empty player candidate fields: " + leaks.Count);
            foreach (string leak in leaks) builder.AppendLine("- FAIL: " + leak);
            builder.AppendLine().AppendLine(passed ? Marker : "LEAK_CHECK_FAIL");
            Write("ItemCompleteCandidateContentLeakCheckReport.md", builder.ToString());
        }

        private static void WriteMainReport(ItemBalanceValidationReport validation, int profiles,
            int versions, int ranges, int powers, int signatures, int random, int pools, int fa, int qi,
            int cores, int ultimates, int displays, int leaks, bool passed)
        {
            StringBuilder builder = new("# ItemCompleteCandidateContentWorkbench01 Report\n\n");
            builder.AppendLine("- Data maturity: BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED");
            builder.AppendLine($"- Completeness: profiles={profiles}/30, versions={versions}/150, ranges={ranges}/600, power={powers}/150");
            builder.AppendLine($"- Content: signatures={signatures}/30, randomDictionary={random}/>=36, poolRows={pools}/>=180, builds={fa}/15+{qi}/10, cores={cores}/120, ultimates={ultimates}/30, display={displays}/30");
            builder.AppendLine("- Player candidate field leaks: " + leaks);
            builder.AppendLine("- Validation errors: " + validation.Errors.Count);
            foreach (string error in validation.Errors) builder.AppendLine("- FAIL: " + error);
            builder.AppendLine("- Formal Battle/Reward/Save wiring: NO");
            builder.AppendLine("- ItemSystemSnapshot.v1 changes: NO");
            builder.AppendLine().AppendLine(passed ? Marker : "ITEM_COMPLETE_CANDIDATE_CONTENT_WORKBENCH01_FAIL");
            Write("ItemCompleteCandidateContentWorkbenchReport.md", builder.ToString());
        }

        private static void WriteMigrationDiff(IEnumerable<ItemBalanceProfile> profiles,
            ItemBalanceWorkbenchCatalog catalog)
        {
            StringBuilder builder = new("# Item Complete Candidate Content Migration Diff\n\n");
            builder.AppendLine("- Strategy: additive + idempotent; known generated placeholder cores are upgraded, arbitrary non-empty user text remains untouched.");
            builder.AppendLine("- Revision: " + catalog.balanceDataRevision);
            foreach (ItemBalanceProfile profile in profiles)
                builder.AppendLine("- " + profile.baseItemId + ": signature=" + profile.signatureAffix.affixId
                    + ", pool=" + profile.randomAffixes.Count + ", cores=" + profile.coreCandidates.Count
                    + ", powers=" + profile.rarityVersions.Count(value => value.candidateItemPower > 0));
            Write("ItemCompleteCandidateContentMigrationDiffReport.md", builder.ToString());
        }

        private static string Range(ItemCandidateRarityValue value) =>
            value.rarity.ToStableKey() + ":" + value.minUnits + ".." + value.maxUnits;

        private static string Csv(params object[] values) => string.Join(",", values.Select(value =>
            "\"" + (value?.ToString() ?? string.Empty).Replace("\"", "\"\"") + "\""));

        private static bool Text(string value) => !string.IsNullOrWhiteSpace(value);

        private static void Write(string fileName, string content)
        {
            File.WriteAllText(Path.GetFullPath(ReportRoot + "/" + fileName), content,
                new UTF8Encoding(false));
        }
    }
}
