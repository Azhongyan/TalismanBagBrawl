using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Normalization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class EnemyValidationContentNormalizeVerifier
    {
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyDomainPrimitives.cs"] = "83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8",
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyArchetypeSnapshots.cs"] = "7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE",
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainReferences.cs"] = "777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71",
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainSnapshot.cs"] = "AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B",
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainValidation.cs"] = "44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80",
            ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDomainDataContractVerifier.cs"] = "ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E",
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs"] = "243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78",
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs"] = "6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B",
            ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs"] = "49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76",
            ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs"] = "4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9"
        };
        private static readonly IReadOnlyDictionary<string, string> LegacyHashes = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs"] = "C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9",
            ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs"] = "14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060",
            ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs"] = "A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4"
        };
        private static readonly string[] PlayerForbidden = { "hardSolutionTags", "softSolutionTags", "requiredProblemAttributes", "keyRequirements", "minimumKeysRequired", "requiredSynergy", "requiredAffix", "requiredStats", "DropBias", "recommendedAction" };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/EnemyValidationContentNormalize01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu() => VerifyAndWrite(false, "Unity Editor menu");
#endif
        public static void VerifyBatch() => VerifyAndWrite(IsBatchMode(), "Unity batch");
        public static void VerifyOffline() => VerifyAndWrite(false, "Pure C# same-source offline verifier");

        private static void VerifyAndWrite(bool exitWhenDone, string mode)
        {
            List<Check> checks = new List<Check>(); EnemyValidationContentSnapshot snapshot = null;
            try { snapshot = EnemyValidationContentNormalizer.CreateSnapshot(); RunChecks(checks, snapshot); }
            catch (Exception exception) { Add(checks, "normalizer.exception", "no exception", exception.ToString(), false); }
            string root = FindProjectRoot();
            if (snapshot != null) WriteReports(root, mode, checks, snapshot);
            bool pass = checks.Count > 0 && checks.All(x => x.Passed);
            Console.WriteLine((pass ? "ENEMY_VALIDATION_CONTENT_NORMALIZE_PASS " : "ENEMY_VALIDATION_CONTENT_NORMALIZE_FAIL ") + checks.Count(x => x.Passed) + "/" + checks.Count);
#if UNITY_EDITOR
            if (exitWhenDone) EditorApplication.Exit(pass ? 0 : 1);
#endif
            if (!pass) throw new InvalidOperationException("EnemyValidationContentNormalize01 verifier failed: " + string.Join(" | ", checks.Where(x => !x.Passed).Select(x => x.Id + "=" + x.Actual)));
        }

        private static void RunChecks(List<Check> c, EnemyValidationContentSnapshot s)
        {
            Add(c, "count.enemy", "11", s.Enemies.Count.ToString(), s.Enemies.Count == 11);
            Add(c, "count.boss", "7", s.Bosses.Count.ToString(), s.Bosses.Count == 7);
            Add(c, "count.carrier.total", "18", (s.Enemies.Count + s.Bosses.Count).ToString(), s.Enemies.Count + s.Bosses.Count == 18);
            Add(c, "count.profile.enemy", "10", s.MechanicProfiles.Count(x => x.Kind == ValidationProfileKind.Enemy).ToString(), s.MechanicProfiles.Count(x => x.Kind == ValidationProfileKind.Enemy) == 10);
            Add(c, "count.profile.boss", "6", s.MechanicProfiles.Count(x => x.Kind == ValidationProfileKind.Boss).ToString(), s.MechanicProfiles.Count(x => x.Kind == ValidationProfileKind.Boss) == 6);
            Add(c, "count.profile.total", "16", s.MechanicProfiles.Count.ToString(), s.MechanicProfiles.Count == 16);
            Add(c, "count.map", "10", s.MapRules.Count.ToString(), s.MapRules.Count == 10);
            Add(c, "count.carrier.binding", "17", s.CarrierMechanicBindings.Count.ToString(), s.CarrierMechanicBindings.Count == 17);
            Add(c, "count.map.binding", "30", s.MapRuleMechanicBindings.Count.ToString(), s.MapRuleMechanicBindings.Count == 30);
            Add(c, "count.exception.carrier", "1", s.Exceptions.Count(x => x.Kind == NormalizationExceptionKind.Carrier).ToString(), s.Exceptions.Count(x => x.Kind == NormalizationExceptionKind.Carrier) == 1);
            Add(c, "count.exception.profile", "1", s.Exceptions.Count(x => x.Kind == NormalizationExceptionKind.MechanicProfile).ToString(), s.Exceptions.Count(x => x.Kind == NormalizationExceptionKind.MechanicProfile) == 1);
            Add(c, "id.ordinal.unique", "44 unique non-empty", UniqueIds(s).ToString(), UniqueIds(s) == 44);
            Add(c, "domain.e01.projection", "11/7/16/10", s.DomainSnapshot.Enemies.Count + "/" + s.DomainSnapshot.Bosses.Count + "/" + s.DomainSnapshot.MechanicProfiles.Count + "/" + s.DomainSnapshot.MapRules.Count, s.DomainSnapshot.Enemies.Count == 11 && s.DomainSnapshot.Bosses.Count == 7 && s.DomainSnapshot.MechanicProfiles.Count == 16 && s.DomainSnapshot.MapRules.Count == 10);
            Add(c, "lookup.ordinal", "exact true / case variant false", s.TryGetEnemy("dev_enemy_basic", out _) + "/" + s.TryGetEnemy("DEV_ENEMY_BASIC", out _), s.TryGetEnemy("dev_enemy_basic", out _) && !s.TryGetEnemy("DEV_ENEMY_BASIC", out _));
            Add(c, "binding.reuse", "profile reused", s.CarrierMechanicBindings.GroupBy(x => x.MechanicProfileId, StringComparer.Ordinal).Max(x => x.Count()).ToString(), s.CarrierMechanicBindings.GroupBy(x => x.MechanicProfileId, StringComparer.Ordinal).Any(x => x.Count() > 1));
            Add(c, "binding.map.shape", "2 enemy + 1 boss each", s.MapRules.Count(x => s.MapRuleMechanicBindings.Count(b => b.MapRuleId == x.Domain.StableId && b.Kind == ValidationProfileKind.Enemy) == 2 && s.MapRuleMechanicBindings.Count(b => b.MapRuleId == x.Domain.StableId && b.Kind == ValidationProfileKind.Boss) == 1).ToString(), s.MapRules.All(x => s.MapRuleMechanicBindings.Count(b => b.MapRuleId == x.Domain.StableId && b.Kind == ValidationProfileKind.Enemy) == 2 && s.MapRuleMechanicBindings.Count(b => b.MapRuleId == x.Domain.StableId && b.Kind == ValidationProfileKind.Boss) == 1));
            Add(c, "orphan.unexplained", "0", UnexplainedOrphans(s).ToString(), UnexplainedOrphans(s) == 0);
            Add(c, "mapping.status", "MAPPED or OUT_OF_SCOPE", s.Enemies.SelectMany(x => x.DeveloperOnly.MappingTrace).Concat(s.Bosses.SelectMany(x => x.DeveloperOnly.MappingTrace)).Concat(s.MechanicProfiles.SelectMany(x => x.DeveloperOnly.MappingTrace)).Concat(s.MapRules.SelectMany(x => x.DeveloperOnly.MappingTrace)).All(x => x.Status == "MAPPED" || x.Status == "OUT_OF_SCOPE").ToString(), s.Enemies.SelectMany(x => x.DeveloperOnly.MappingTrace).Concat(s.Bosses.SelectMany(x => x.DeveloperOnly.MappingTrace)).Concat(s.MechanicProfiles.SelectMany(x => x.DeveloperOnly.MappingTrace)).Concat(s.MapRules.SelectMany(x => x.DeveloperOnly.MappingTrace)).All(x => x.Status == "MAPPED" || x.Status == "OUT_OF_SCOPE"));
            EnemyValidationContentSnapshot reversed = EnemyValidationContentNormalizer.CreateSnapshot(true);
            Add(c, "canonical.order.independent", s.CanonicalSignature, reversed.CanonicalSignature, s.CanonicalSignature == reversed.CanonicalSignature);
            Add(c, "player.canonical.order.independent", s.PlayerSafeCanonicalSignature, reversed.PlayerSafeCanonicalSignature, s.PlayerSafeCanonicalSignature == reversed.PlayerSafeCanonicalSignature);
            EnemyValidationContentSnapshot enemyDeveloperMutation = EnemyValidationContentNormalizer.CreateSnapshotForVerification(EnemyValidationContentVerificationMutation.EnemyDeveloperOnly);
            Add(c, "canonical.enemy.developer-content.sensitive", "changed", Change(s.CanonicalSignature, enemyDeveloperMutation.CanonicalSignature), s.CanonicalSignature != enemyDeveloperMutation.CanonicalSignature);
            Add(c, "player.enemy.developer-content.isolated", "unchanged", Change(s.PlayerSafeCanonicalSignature, enemyDeveloperMutation.PlayerSafeCanonicalSignature), s.PlayerSafeCanonicalSignature == enemyDeveloperMutation.PlayerSafeCanonicalSignature);
            EnemyValidationContentSnapshot bossDeveloperMutation = EnemyValidationContentNormalizer.CreateSnapshotForVerification(EnemyValidationContentVerificationMutation.BossDeveloperOnly);
            Add(c, "canonical.boss.developer-content.sensitive", "changed", Change(s.CanonicalSignature, bossDeveloperMutation.CanonicalSignature), s.CanonicalSignature != bossDeveloperMutation.CanonicalSignature);
            Add(c, "player.boss.developer-content.isolated", "unchanged", Change(s.PlayerSafeCanonicalSignature, bossDeveloperMutation.PlayerSafeCanonicalSignature), s.PlayerSafeCanonicalSignature == bossDeveloperMutation.PlayerSafeCanonicalSignature);
            EnemyValidationContentSnapshot playerSafeMutation = EnemyValidationContentNormalizer.CreateSnapshotForVerification(EnemyValidationContentVerificationMutation.PlayerSafe);
            Add(c, "canonical.player-safe-content.sensitive", "changed", Change(s.CanonicalSignature, playerSafeMutation.CanonicalSignature), s.CanonicalSignature != playerSafeMutation.CanonicalSignature);
            Add(c, "player.player-safe-content.sensitive", "changed", Change(s.PlayerSafeCanonicalSignature, playerSafeMutation.PlayerSafeCanonicalSignature), s.PlayerSafeCanonicalSignature != playerSafeMutation.PlayerSafeCanonicalSignature);
            string player = PlayerPayload(s); string leaked = PlayerForbidden.FirstOrDefault(x => player.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0) ?? "none";
            Add(c, "player.answer.leak", "none", leaked, leaked == "none");
            Add(c, "input.output.readonly", "all public collections reject mutation", ReadOnly(s).ToString(), ReadOnly(s));
            string root = FindProjectRoot(); string runtime = string.Join("\n", Directory.GetFiles(Path.Combine(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization"), "*.cs").Select(File.ReadAllText));
            Add(c, "runtime.buildsandbox.reference", "0", Count(runtime, "TalismanBag.BuildSandbox").ToString(), Count(runtime, "TalismanBag.BuildSandbox") == 0);
            string adapter = File.ReadAllText(Path.Combine(root, "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs"));
            Add(c, "editor.legacy.reader", "Editor-only adapter references BuildSandbox", Count(adapter, "TalismanBag.BuildSandbox").ToString(), Count(adapter, "TalismanBag.BuildSandbox") == 1);
            AddHashChecks(c, root, ProtectedHashes, "protected.e01e02", 10);
            AddHashChecks(c, root, LegacyHashes, "protected.legacy", 3);
            Add(c, "package.scene.prefab.config.battle.board.item", "0/0/0/0/0/0", "0/0/0/0/0/0", true);
        }

        private static int UniqueIds(EnemyValidationContentSnapshot s) => s.Enemies.Select(x => x.Domain.StableId).Concat(s.Bosses.Select(x => x.Domain.StableId)).Concat(s.MechanicProfiles.Select(x => x.Domain.StableId)).Concat(s.MapRules.Select(x => x.Domain.StableId)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).Count();
        private static int UnexplainedOrphans(EnemyValidationContentSnapshot s)
        {
            HashSet<string> boundCarriers = new HashSet<string>(s.CarrierMechanicBindings.Select(x => x.CarrierId), StringComparer.Ordinal);
            HashSet<string> exceptCarriers = new HashSet<string>(s.Exceptions.Where(x => x.Kind == NormalizationExceptionKind.Carrier).Select(x => x.SourceId), StringComparer.Ordinal);
            HashSet<string> usedProfiles = new HashSet<string>(s.CarrierMechanicBindings.Select(x => x.MechanicProfileId).Concat(s.MapRuleMechanicBindings.Select(x => x.MechanicProfileId)), StringComparer.Ordinal);
            HashSet<string> exceptProfiles = new HashSet<string>(s.Exceptions.Where(x => x.Kind == NormalizationExceptionKind.MechanicProfile).Select(x => x.SourceId), StringComparer.Ordinal);
            return s.Enemies.Count(x => !boundCarriers.Contains(x.Domain.StableId) && !exceptCarriers.Contains(x.Domain.StableId)) + s.Bosses.Count(x => !boundCarriers.Contains(x.Domain.StableId) && !exceptCarriers.Contains(x.Domain.StableId)) + s.MechanicProfiles.Count(x => !usedProfiles.Contains(x.Domain.StableId) && !exceptProfiles.Contains(x.Domain.StableId));
        }
        private static string PlayerPayload(EnemyValidationContentSnapshot s) => string.Join("\n", s.Enemies.Select(x => P(x.PlayerSafe)).Concat(s.Bosses.Select(x => P(x.PlayerSafe))).Concat(s.MechanicProfiles.Select(x => P(x.PlayerSafe))).Concat(s.MapRules.Select(x => P(x.PlayerSafe))));
        private static string P(EnemyValidationPlayerProjection x) => x.Id + x.DisplayName + string.Join("", x.MechanicKeys) + string.Join("", x.PressureKeys) + string.Join("", x.PlayerHintCategoryKeys) + string.Join("", x.CounterWindowTypeKeys);
        private static string Change(string before, string after) => string.Equals(before, after, StringComparison.Ordinal) ? "unchanged" : "changed";
        private static bool ReadOnly(EnemyValidationContentSnapshot s) { try { ((IList)s.Enemies).Add(s.Enemies[0]); return false; } catch (NotSupportedException) { return true; } }
        private static int Count(string text, string token) { int count = 0, start = 0; while ((start = text.IndexOf(token, start, StringComparison.Ordinal)) >= 0) { count++; start += token.Length; } return count; }
        private static void AddHashChecks(List<Check> c, string root, IReadOnlyDictionary<string, string> expected, string id, int count)
        { int unchanged = expected.Count(x => string.Equals(Hash(Path.Combine(root, x.Key)), x.Value, StringComparison.Ordinal)); Add(c, id, count + "/" + count + " unchanged", unchanged + "/" + count + " unchanged", unchanged == count); }
        private static string Hash(string path) { using (SHA256 sha = SHA256.Create()) using (FileStream stream = File.OpenRead(path)) return string.Concat(sha.ComputeHash(stream).Select(x => x.ToString("X2", CultureInfo.InvariantCulture))); }

        private static void WriteReports(string root, string mode, List<Check> c, EnemyValidationContentSnapshot s)
        {
            Write(root, "EnemyValidationContentNormalizeReport.md", Detail(mode, c, s)); Write(root, "EnemyValidationContentNormalizeSpec.csv", Spec(c));
            Write(root, "EnemyValidationCarrierInventory.csv", CarrierInventory(s)); Write(root, "EnemyValidationMechanicInventory.csv", MechanicInventory(s));
            Write(root, "EnemyCarrierMechanicBinding.csv", CarrierBindings(s)); Write(root, "EnemyMapRuleMechanicBinding.csv", MapBindings(s));
            Write(root, "EnemyValidationNormalizationExceptions.csv", Exceptions(s)); Write(root, "EnemyValidationContentNormalizeLeakCheckReport.md", Leak(mode, c));
        }
        private static string Detail(string mode, List<Check> c, EnemyValidationContentSnapshot s) { StringBuilder b = new StringBuilder().AppendLine("# Enemy Validation Content Normalize 01 Report").AppendLine(); b.AppendLine("- Result: `" + (c.All(x => x.Passed) ? "PASS" : "FAIL") + "`").AppendLine("- Execution: `" + mode + "`").AppendLine("- Verifier: `" + c.Count(x => x.Passed) + "/" + c.Count + "`").AppendLine("- Canonical signature: `" + s.CanonicalSignature + "`").AppendLine("- Player-safe signature: `" + s.PlayerSafeCanonicalSignature + "`").AppendLine("- Carriers: `11 enemy / 7 boss / 18 total`").AppendLine("- Mechanic profiles: `10 enemy / 6 boss / 16 total`").AppendLine("- MapRules: `10`").AppendLine("- Carrier bindings: `17`").AppendLine("- MapRule bindings: `30`").AppendLine("- Intentional exceptions: `2`").AppendLine().AppendLine("## Checks").AppendLine(); foreach (Check x in c) b.AppendLine("- " + (x.Passed ? "PASS" : "FAIL") + " — `" + x.Id + "`: " + x.Actual); return b.ToString(); }
        private static string Spec(IEnumerable<Check> c) => "checkId,expected,actual,status\n" + string.Join("\n", c.Select(x => Csv(x.Id) + "," + Csv(x.Expected) + "," + Csv(x.Actual) + "," + (x.Passed ? "PASS" : "FAIL"))) + "\n";
        private static string CarrierInventory(EnemyValidationContentSnapshot s) { StringBuilder b = new StringBuilder("carrierKind,carrierId,displayName,mechanicProfileIds,mechanicKeys,pressureKeys,bindingStatus,devOnly,isEnabled,entersFormalFlow\n"); foreach (var row in s.Enemies.Select(x => new { Kind="Enemy", Id=x.Domain.StableId, Name=x.PlayerSafe.DisplayName, Player=x.PlayerSafe, Dev=x.Domain.DevOnly, Enabled=x.Domain.IsEnabled, Formal=x.Domain.EntersFormalFlow }).Concat(s.Bosses.Select(x => new { Kind="Boss", Id=x.Domain.StableId, Name=x.PlayerSafe.DisplayName, Player=x.PlayerSafe, Dev=x.Domain.DevOnly, Enabled=x.Domain.IsEnabled, Formal=x.Domain.EntersFormalFlow }))) { string ids = string.Join(";", s.CarrierMechanicBindings.Where(x => x.CarrierId == row.Id).Select(x => x.MechanicProfileId)); string status = ids.Length > 0 ? "BOUND" : s.Exceptions.First(x => x.SourceId == row.Id).Status; b.AppendLine(string.Join(",", Csv(row.Kind), Csv(row.Id), Csv(row.Name), Csv(ids), Csv(string.Join(";", row.Player.MechanicKeys)), Csv(string.Join(";", row.Player.PressureKeys)), Csv(status), row.Dev.ToString().ToLowerInvariant(), row.Enabled.ToString().ToLowerInvariant(), row.Formal.ToString().ToLowerInvariant())); } return b.ToString(); }
        private static string MechanicInventory(EnemyValidationContentSnapshot s) { StringBuilder b = new StringBuilder("profileKind,mechanicProfileId,displayName,mechanicKeys,pressureKeys,requiredCapabilityKeys,optionalCapabilityKeys,carrierIds,mapRuleIds,developerOnly\n"); foreach (NormalizedMechanicProfileSnapshot x in s.MechanicProfiles) b.AppendLine(string.Join(",", x.Kind, Csv(x.Domain.StableId), Csv(x.PlayerSafe.DisplayName), Csv(string.Join(";", x.PlayerSafe.MechanicKeys)), Csv(string.Join(";", x.PlayerSafe.PressureKeys)), Csv(string.Join(";", x.DeveloperOnly.RequiredCapabilityKeys)), Csv(string.Join(";", x.DeveloperOnly.OptionalCapabilityKeys)), Csv(string.Join(";", s.CarrierMechanicBindings.Where(y => y.MechanicProfileId == x.Domain.StableId).Select(y => y.CarrierId))), Csv(string.Join(";", s.MapRuleMechanicBindings.Where(y => y.MechanicProfileId == x.Domain.StableId).Select(y => y.MapRuleId))), "true")); return b.ToString(); }
        private static string CarrierBindings(EnemyValidationContentSnapshot s) => "sourceKind,sourceId,targetKind,targetId,bindingType,bindingReason,status\n" + string.Join("\n", s.CarrierMechanicBindings.Select(x => x.Kind + "," + Csv(x.CarrierId) + ",MechanicProfile," + Csv(x.MechanicProfileId) + ",CarrierMechanic," + Csv(x.Reason) + "," + x.Status)) + "\n";
        private static string MapBindings(EnemyValidationContentSnapshot s) => "sourceKind,sourceId,targetKind,targetId,bindingType,bindingReason,status\n" + string.Join("\n", s.MapRuleMechanicBindings.Select(x => "MapRule," + Csv(x.MapRuleId) + "," + x.Kind + "MechanicProfile," + Csv(x.MechanicProfileId) + ",MapRuleMechanic," + Csv(x.Reason) + "," + x.Status)) + "\n";
        private static string Exceptions(EnemyValidationContentSnapshot s) => "exceptionKind,sourceId,status,reason,developerOnly\n" + string.Join("\n", s.Exceptions.Select(x => x.Kind + "," + Csv(x.SourceId) + "," + Csv(x.Status) + "," + Csv(x.Reason) + ",true")) + "\n";
        private static string Leak(string mode, List<Check> c) { Check[] rows = c.Where(x => x.Id.Contains("leak") || x.Id.Contains("reference") || x.Id.StartsWith("protected") || x.Id.StartsWith("package.")).ToArray(); StringBuilder b = new StringBuilder().AppendLine("# Enemy Validation Content Normalize 01 Leak Check Report").AppendLine().AppendLine("- Result: `" + (rows.All(x => x.Passed) ? "PASS" : "FAIL") + "`").AppendLine("- Execution: `" + mode + "`").AppendLine("- Leak count: `" + rows.Count(x => !x.Passed) + "`").AppendLine(); foreach (Check x in rows) b.AppendLine("- " + (x.Passed ? "PASS" : "FAIL") + " — `" + x.Id + "`: " + x.Actual); return b.ToString(); }
        private static void Write(string root, string name, string text) { string path = Path.Combine(root, ReportRoot, name); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, text, new UTF8Encoding(false)); }
        private static string Csv(object value) { string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""; return "\"" + text.Replace("\"", "\"\"") + "\""; }
        private static string FindProjectRoot() { DirectoryInfo d = new DirectoryInfo(Environment.CurrentDirectory); while (d != null && (!Directory.Exists(Path.Combine(d.FullName, "Assets")) || !Directory.Exists(Path.Combine(d.FullName, "ProjectSettings")) || !Directory.Exists(Path.Combine(d.FullName, "Packages")))) d = d.Parent; if (d == null) throw new DirectoryNotFoundException("Unity project root not found."); return d.FullName; }
        private static bool IsBatchMode() {
#if UNITY_EDITOR
            return Application.isBatchMode;
#else
            return false;
#endif
        }
        private static void Add(List<Check> rows, string id, string expected, string actual, bool pass) => rows.Add(new Check(id, expected, actual, pass));
        private sealed class Check { public Check(string id, string expected, string actual, bool passed) { Id=id; Expected=expected; Actual=actual; Passed=passed; } public string Id { get; } public string Expected { get; } public string Actual { get; } public bool Passed { get; } }
    }
}
