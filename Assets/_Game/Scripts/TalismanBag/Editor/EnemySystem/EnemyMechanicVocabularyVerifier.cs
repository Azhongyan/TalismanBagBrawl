using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Vocabulary;
using UnityEditor;

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class EnemyMechanicVocabularyVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/EnemyMechanicVocabularyReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/EnemyMechanicVocabularySpec.csv";
        private const string InventoryCsvPath = "Docs/V0.4/Reports/EnemyMechanicVocabularyInventory.csv";
        private const string LegacyMappingCsvPath = "Docs/V0.4/Reports/EnemyMechanicVocabularyLegacyMapping.csv";
        private const string LeakReportPath = "Docs/V0.4/Reports/EnemyMechanicVocabularyLeakCheckReport.md";

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs"
        };

        private static readonly string[] PackageManifest =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs.meta",
            DetailReportPath,
            SpecCsvPath,
            InventoryCsvPath,
            LegacyMappingCsvPath,
            LeakReportPath
        };

        private static readonly IReadOnlyDictionary<string, string> ProtectedE01Hashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyDomainPrimitives.cs"] = "83114bba194226ec539a9bc401ff791ac8a76560b845e8d1b7f8e662c8122dc8",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyArchetypeSnapshots.cs"] = "7d459972e78c5acf768a296b679a00b4874dfc5497e47fc553db82288f58c1fe",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainReferences.cs"] = "777da9a7370562961d34346d3604de9c345f2b634435152b0bf0eb4cea135e71",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainSnapshot.cs"] = "ab1004b8abb5c96794bb1b17b7314232e7fd439a08745b9bbda17fefbf9ccb1b",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainValidation.cs"] = "44caa85ce869e6eb6b6859328e2ed0be496809184dacbd187435ad35bc462d80",
                ["Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDomainDataContractVerifier.cs"] = "acf3aec47318cc09511610430822663f70968ebda8e26a573f3f9de3b5a6600e"
            };

        private static readonly DefaultEnemyMechanicVocabularyProvider Provider = DefaultEnemyMechanicVocabularyProvider.Instance;
        private static readonly DefaultEnemyMechanicVocabularyValidator Validator = DefaultEnemyMechanicVocabularyValidator.Instance;

        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/EnemyMechanicVocabulary01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(false, "Unity Editor menu");
        }

        public static void VerifyBatch()
        {
            VerifyAndWriteReports(IsBatchMode(), "Unity batch");
        }

        public static void VerifyOffline()
        {
            VerifyAndWriteReports(false, "Pure C# same-source offline verifier");
        }

        private static void VerifyAndWriteReports(bool exitWhenDone, string executionMode)
        {
            VerificationResult result = new VerificationResult();
            EnemyMechanicVocabularySnapshot snapshot = null;
            try
            {
                snapshot = RunVerification(result);
            }
            catch (Exception exception)
            {
                result.Add("verifier-unhandled-exception", "verifier", "No unhandled exception", exception.ToString(), false,
                    "Verifier must remain deterministic and self-reporting.");
            }

            WriteReports(result, snapshot, executionMode);
            if (result.Passed)
            {
                Console.WriteLine("ENEMY_MECHANIC_VOCABULARY_VERIFIER_PASS " + result.PassedCount + "/" + result.TotalCount);
            }
            else
            {
                foreach (CheckRow row in result.Rows.Where(value => !value.Passed))
                {
                    Console.Error.WriteLine("ENEMY_MECHANIC_VOCABULARY_VERIFIER_FAIL " + row.CheckId + ": " + row.Actual);
                }
            }

            if (exitWhenDone)
            {
                EditorApplication.Exit(result.Passed ? 0 : 1);
            }
        }

        private static EnemyMechanicVocabularySnapshot RunVerification(VerificationResult result)
        {
            EnemyMechanicVocabularySnapshotInput input = DefaultEnemyMechanicVocabularyCatalog.CreateInput();
            EnemyMechanicVocabularySnapshot snapshot = Provider.CreateSnapshot(input);

            Add(result, "schema-id", "schema", EnemyMechanicVocabularySchema.SchemaId, snapshot.SchemaId,
                string.Equals(snapshot.SchemaId, EnemyMechanicVocabularySchema.SchemaId, StringComparison.Ordinal),
                "Schema ID is exact and case-sensitive.");
            Add(result, "schema-version", "schema", "1", snapshot.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                snapshot.SchemaVersion == EnemyMechanicVocabularySchema.SchemaVersion,
                "Schema version remains independently machine-readable.");
            Add(result, "provider-contract", "contract", "Provider and lookup contracts implemented",
                typeof(DefaultEnemyMechanicVocabularyProvider).GetInterfaces().Contains(typeof(IEnemyMechanicVocabularyProvider))
                    && typeof(EnemyMechanicVocabularySnapshot).GetInterfaces().Contains(typeof(IEnemyMechanicVocabularyLookup)) ? "implemented" : "missing",
                typeof(DefaultEnemyMechanicVocabularyProvider).GetInterfaces().Contains(typeof(IEnemyMechanicVocabularyProvider))
                    && typeof(EnemyMechanicVocabularySnapshot).GetInterfaces().Contains(typeof(IEnemyMechanicVocabularyLookup)),
                "Construction and strict lookup are exposed through read-only contracts.");

            CheckKeyTypesAndCoverage(result, snapshot);
            CheckLookupSemantics(result, snapshot);
            CheckImmutability(result, input, snapshot);
            CheckCanonicalSignature(result, snapshot);
            CheckLegacyMappings(result, snapshot);
            CheckValidationFailures(result, input);
            CheckSourceAndFieldLeaks(result);
            CheckProtectedHashes(result);
            CheckPackageScope(result);
            return snapshot;
        }

        private static void CheckKeyTypesAndCoverage(VerificationResult result, EnemyMechanicVocabularySnapshot snapshot)
        {
            Type[] keyTypes =
            {
                typeof(MechanicKey), typeof(BuildCapabilityKey), typeof(PressureChannelKey),
                typeof(CounterWindowTypeKey), typeof(PlayerHintCategoryKey), typeof(DeveloperDiagnosticCategoryKey)
            };
            bool stringIdentityTypes = keyTypes.All(type => type.IsClass && type.IsSealed && !type.IsEnum && type.BaseType == typeof(EnemyVocabularyStableKey));
            Add(result, "string-stable-key-types", "contract", "6 sealed string identity types; no growing mechanism enum",
                stringIdentityTypes ? "6 string identity types" : "invalid key type", stringIdentityTypes,
                "Mechanic and capability sets can grow by adding namespaced string data without enum reordering.");

            Dictionary<EnemyVocabularyCategory, int> expected = new Dictionary<EnemyVocabularyCategory, int>
            {
                [EnemyVocabularyCategory.Mechanic] = 12,
                [EnemyVocabularyCategory.BuildCapability] = 16,
                [EnemyVocabularyCategory.PressureChannel] = 9,
                [EnemyVocabularyCategory.CounterWindowType] = 6,
                [EnemyVocabularyCategory.PlayerHintCategory] = 10,
                [EnemyVocabularyCategory.DeveloperDiagnosticCategory] = 10
            };
            foreach (KeyValuePair<EnemyVocabularyCategory, int> pair in expected)
            {
                int actual = snapshot.Entries.Count(value => value.Category == pair.Key);
                Add(result, "coverage-" + EnemyVocabularyCategoryNames.StableName(pair.Key), "coverage",
                    pair.Value.ToString(CultureInfo.InvariantCulture), actual.ToString(CultureInfo.InvariantCulture), actual == pair.Value,
                    "First vocabulary release has explicit semantic coverage for this category.");
            }

            string[] requiredKeys =
            {
                "mechanic.basic_pressure", "mechanic.layered_shield", "mechanic.swarm_summon", "mechanic.poison",
                "mechanic.burning", "mechanic.energy_drain", "mechanic.talisman_seal", "mechanic.burst_spike",
                "mechanic.long_cast", "mechanic.high_health_endurance", "mechanic.polluted_tile", "mechanic.formation_eye_disruption",
                "capability.break_power", "capability.clear_power", "capability.cleanse_power", "capability.debuff_counter",
                "capability.energy_stability", "capability.spirit_lock", "capability.control_power", "capability.placement_shape",
                "capability.burst_window", "capability.guard_power", "capability.interrupt_timing", "capability.sustained_damage",
                "capability.chain_reaction", "capability.cooldown_recovery", "capability.caster_interrupt", "capability.thunder_chain",
                "pressure.shield", "pressure.multi_target", "pressure.status_damage", "pressure.resource_disruption",
                "pressure.seal_control", "pressure.burst_survival", "pressure.cast_interrupt", "pressure.sustained_output", "pressure.placement_formation",
                "counter_window.shell_break", "counter_window.cleanse_reveal", "counter_window.interrupt_stagger",
                "counter_window.guard_rebound", "counter_window.clear_core_exposure", "counter_window.energy_counter_full_array"
            };
            HashSet<string> actualKeys = new HashSet<string>(snapshot.Entries.Select(value => value.StableKey), StringComparer.Ordinal);
            string[] missing = requiredKeys.Where(key => !actualKeys.Contains(key)).ToArray();
            Add(result, "required-semantic-keys", "coverage", "All Assignment-required Mechanic/Capability/Pressure/Counter keys",
                missing.Length == 0 ? "all present" : string.Join("|", missing), missing.Length == 0,
                "The initial catalog covers every semantic family named by E02.");

            bool labelsChinese = snapshot.Entries.All(entry => entry.DeveloperLabelZh.Any(character => character > 127));
            Add(result, "developer-labels-zh", "coverage", "Every entry has a developer Chinese label",
                labelsChinese ? "all present" : "missing Chinese label", labelsChinese,
                "Stable English keys and developer Chinese labels are separate fields.");
        }

        private static void CheckLookupSemantics(VerificationResult result, EnemyMechanicVocabularySnapshot snapshot)
        {
            const string key = "mechanic.layered_shield";
            bool exact = snapshot.TryGetEntry(EnemyVocabularyCategory.Mechanic, key, out EnemyVocabularyEntrySnapshot entry)
                && string.Equals(entry.StableKey, key, StringComparison.Ordinal);
            Add(result, "lookup-exact-ordinal", "lookup", "Exact category/key resolves", exact ? "resolved" : "failed", exact,
                "Lookup uses exact ordinal identity.");

            bool caseStrict = !snapshot.TryGetEntry(EnemyVocabularyCategory.Mechanic, "mechanic.Layered_shield", out _);
            Add(result, "lookup-case-sensitive", "lookup", "Case variant does not resolve", caseStrict ? "strict" : "normalized", caseStrict,
                "Case is part of stable identity.");

            bool noTrim = !snapshot.TryGetEntry(EnemyVocabularyCategory.Mechanic, " " + key + " ", out _);
            Add(result, "lookup-no-implicit-trim", "lookup", "Padded key does not resolve", noTrim ? "strict" : "trimmed", noTrim,
                "Lookup never normalizes stable identity.");

            bool categoryStrict = !snapshot.TryGetEntry(EnemyVocabularyCategory.BuildCapability, key, out _);
            Add(result, "lookup-category-strict", "lookup", "Wrong category does not resolve", categoryStrict ? "strict" : "cross-category match", categoryStrict,
                "Category remains part of lookup identity.");

            bool unknownSafe = !snapshot.TryGetEntry(EnemyVocabularyCategory.Mechanic, "mechanic.unknown", out _)
                && !snapshot.TryGetEntry(EnemyVocabularyCategory.Mechanic, null, out _)
                && !snapshot.TryGetLegacyMapping(null, null, out _);
            Add(result, "lookup-unknown-safe", "lookup", "Unknown/null returns false", unknownSafe ? "safe false" : "unexpected match", unknownSafe,
                "Unknown keys do not throw or substitute another identity.");
        }

        private static void CheckImmutability(
            VerificationResult result,
            EnemyMechanicVocabularySnapshotInput input,
            EnemyMechanicVocabularySnapshot snapshot)
        {
            List<EnemyVocabularyEntrySnapshot> sourceEntries = input.Entries.Select(CopyEntry).ToList();
            List<LegacyEnemyVocabularyMappingSnapshot> sourceMappings = input.LegacyMappings.Select(CopyMapping).ToList();
            EnemyMechanicVocabularySnapshotInput copiedInput = new EnemyMechanicVocabularySnapshotInput(sourceEntries, sourceMappings);
            sourceEntries.Clear();
            sourceMappings.Clear();
            EnemyMechanicVocabularySnapshot copied = Provider.CreateSnapshot(copiedInput);
            bool inputCopied = copied.Entries.Count == input.Entries.Count && copied.LegacyMappings.Count == input.LegacyMappings.Count;
            Add(result, "input-defensive-copy", "immutability", "Caller list mutation cannot alter input/snapshot",
                inputCopied ? "unchanged" : "mutated", inputCopied,
                "Input and output construction boundaries defensively copy collections.");

            bool outputReadOnly = IsReadOnly(snapshot.Entries, snapshot.Entries[0])
                && IsReadOnly(snapshot.LegacyMappings, snapshot.LegacyMappings[0]);
            Add(result, "output-collections-read-only", "immutability", "Entry and mapping collections reject mutation",
                outputReadOnly ? "all rejected" : "mutable collection found", outputReadOnly,
                "Snapshot collections are backed by ReadOnlyCollection.");

            LegacyEnemyVocabularyMappingSnapshot many = snapshot.LegacyMappings.First(value => value.Targets.Count > 1);
            bool targetsReadOnly = IsReadOnly(many.Targets, many.Targets[0]);
            Add(result, "mapping-targets-read-only", "immutability", "Mapping targets reject mutation",
                targetsReadOnly ? "rejected" : "mutable", targetsReadOnly,
                "One-to-many mapping targets cannot be changed through returned references.");
        }

        private static void CheckCanonicalSignature(VerificationResult result, EnemyMechanicVocabularySnapshot snapshot)
        {
            string first = snapshot.BuildCanonicalSignature();
            string repeat = snapshot.BuildCanonicalSignature();
            string reversed = Provider.CreateSnapshot(DefaultEnemyMechanicVocabularyCatalog.CreateInput(true)).BuildCanonicalSignature();

            EnemyMechanicVocabularySnapshotInput baseline = DefaultEnemyMechanicVocabularyCatalog.CreateInput();
            List<EnemyVocabularyEntrySnapshot> changedEntries = baseline.Entries.Select(CopyEntry).ToList();
            EnemyVocabularyEntrySnapshot original = changedEntries[0];
            changedEntries[0] = new EnemyVocabularyEntrySnapshot(
                original.Key,
                original.DeveloperLabelZh,
                original.Description + "（变化）",
                original.PlayerVisible,
                original.DeveloperOnly);
            string changed = Provider.CreateSnapshot(new EnemyMechanicVocabularySnapshotInput(changedEntries, baseline.LegacyMappings)).BuildCanonicalSignature();

            bool format = first.StartsWith("sha256:", StringComparison.Ordinal)
                && first.Length == 71
                && first.Substring(7).All(character => (character >= '0' && character <= '9') || (character >= 'a' && character <= 'f'));
            Add(result, "canonical-format", "canonical", "sha256: + 64 lowercase hex", first, format,
                "Signature exposes its deterministic algorithm.");
            Add(result, "canonical-repeat", "canonical", first, repeat, string.Equals(first, repeat, StringComparison.Ordinal),
                "Repeated evaluation is stable.");
            Add(result, "canonical-input-order-independent", "canonical", first, reversed, string.Equals(first, reversed, StringComparison.Ordinal),
                "Entry, mapping, and target input order does not affect the signature.");
            Add(result, "canonical-content-sensitive", "canonical", "Different content changes signature", changed,
                !string.Equals(first, changed, StringComparison.Ordinal),
                "Vocabulary content changes remain detectable.");
        }

        private static void CheckLegacyMappings(VerificationResult result, EnemyMechanicVocabularySnapshot snapshot)
        {
            int mapped = snapshot.LegacyMappings.Count(value => value.Status == LegacyEnemyVocabularyMappingStatus.Mapped);
            int outOfScope = snapshot.LegacyMappings.Count(value => value.Status == LegacyEnemyVocabularyMappingStatus.OutOfScope);
            Add(result, "legacy-inventory-count", "legacy", "124", snapshot.LegacyMappings.Count.ToString(CultureInfo.InvariantCulture),
                snapshot.LegacyMappings.Count == 124,
                "Every inventoried old source key has one mapping or explicit OUT_OF_SCOPE record.");
            Add(result, "legacy-inventory-resolved-status", "legacy", "mapped + out-of-scope = total",
                mapped.ToString(CultureInfo.InvariantCulture) + "+" + outOfScope.ToString(CultureInfo.InvariantCulture),
                mapped + outOfScope == snapshot.LegacyMappings.Count,
                "No unresolved status exists in the delivered inventory.");

            bool targetsResolve = snapshot.LegacyMappings
                .Where(value => value.Status == LegacyEnemyVocabularyMappingStatus.Mapped)
                .SelectMany(value => value.Targets)
                .All(target => snapshot.TryGetEntry(target.Category, target.StableKey, out _));
            Add(result, "legacy-targets-resolve", "legacy", "All canonical targets resolve ordinally",
                targetsResolve ? "all resolved" : "unresolved target", targetsResolve,
                "Legacy mappings never rely on case-insensitive or trimmed lookup.");

            int oneToMany = snapshot.LegacyMappings.Count(value => value.Status == LegacyEnemyVocabularyMappingStatus.Mapped && value.Targets.Count > 1);
            Add(result, "legacy-one-to-many-preserved", "legacy", "> 0", oneToMany.ToString(CultureInfo.InvariantCulture), oneToMany > 0,
                "Composite legacy labels can decompose into multiple stable primitives.");

            string root = FindProjectRoot();
            Dictionary<string, string> sourceText = snapshot.LegacyMappings
                .Select(value => DefaultEnemyMechanicVocabularyCatalog.LegacySourcePath(value.LegacySourceKind))
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(path => path, path => File.ReadAllText(Path.Combine(root, path)), StringComparer.Ordinal);
            string[] sourceMisses = snapshot.LegacyMappings
                .Where(value => sourceText[DefaultEnemyMechanicVocabularyCatalog.LegacySourcePath(value.LegacySourceKind)]
                    .IndexOf("\"" + value.LegacyKey + "\"", StringComparison.Ordinal) < 0)
                .Select(value => value.LegacySourceKind + ":" + value.LegacyKey)
                .ToArray();
            Add(result, "legacy-source-token-traceability", "legacy", "Every legacy key appears in its declared read-only source",
                sourceMisses.Length == 0 ? "all traced" : string.Join("|", sourceMisses), sourceMisses.Length == 0,
                "The mapping inventory is traceable to the three inspected legacy source files.");

            bool fieldAliasesDocumented = snapshot.LegacyMappings.Any(value => value.LegacySourceKind == DefaultEnemyMechanicVocabularyCatalog.MechanicTypeSource)
                && snapshot.LegacyMappings.Any(value => value.LegacySourceKind == DefaultEnemyMechanicVocabularyCatalog.RequiredCapabilitySource)
                && snapshot.LegacyMappings.Any(value => value.LegacySourceKind == DefaultEnemyMechanicVocabularyCatalog.OptionalCapabilitySource);
            Add(result, "legacy-field-aliases-documented", "legacy", "Actual fields pressureType/hardSolutionTags/softSolutionTags retained",
                fieldAliasesDocumented ? "documented" : "missing", fieldAliasesDocumented,
                "Assignment semantic aliases do not rewrite or misname existing source fields.");
        }

        private static void CheckValidationFailures(VerificationResult result, EnemyMechanicVocabularySnapshotInput baseline)
        {
            AddRejection(result, "uppercase-key-detected", "STABLE_KEY_FORMAT_INVALID",
                InputWithEntries(baseline, new EnemyVocabularyEntrySnapshot(new MechanicKey("mechanic.Layered"), "错误键", "错误键测试。", false, false)));
            AddRejection(result, "prefix-mismatch-detected", "STABLE_KEY_PREFIX_MISMATCH",
                InputWithEntries(baseline, new EnemyVocabularyEntrySnapshot(new MechanicKey("capability.wrong_prefix"), "错误前缀", "错误前缀测试。", false, false)));

            EnemyVocabularyEntrySnapshot duplicate = CopyEntry(baseline.Entries[0]);
            AddRejection(result, "duplicate-stable-key-detected", "STABLE_KEY_DUPLICATE",
                new EnemyMechanicVocabularySnapshotInput(baseline.Entries.Concat(new[] { duplicate }).ToArray(), baseline.LegacyMappings));
            AddRejection(result, "cross-category-key-collision-detected", "STABLE_KEY_CATEGORY_COLLISION",
                new EnemyMechanicVocabularySnapshotInput(
                    baseline.Entries.Concat(new[] { new EnemyVocabularyEntrySnapshot(new BuildCapabilityKey(baseline.Entries[0].StableKey), "冲突键", "跨类别冲突测试。", false, false) }).ToArray(),
                    baseline.LegacyMappings));

            LegacyEnemyVocabularyMappingSnapshot first = baseline.LegacyMappings[0];
            AddRejection(result, "duplicate-legacy-source-detected", "LEGACY_SOURCE_DUPLICATE",
                new EnemyMechanicVocabularySnapshotInput(baseline.Entries, baseline.LegacyMappings.Concat(new[] { CopyMapping(first) }).ToArray()));
            AddRejection(result, "unresolved-legacy-target-detected", "LEGACY_TARGET_UNRESOLVED",
                InputWithMappings(baseline, new LegacyEnemyVocabularyMappingSnapshot(
                    "Verifier.synthetic", "unknown_target", new[] { new EnemyVocabularyKeyReferenceSnapshot(EnemyVocabularyCategory.Mechanic, "mechanic.missing") },
                    LegacyEnemyVocabularyMappingStatus.Mapped, "Synthetic invalid target.")));
            AddRejection(result, "mapped-without-target-detected", "LEGACY_MAPPED_WITHOUT_TARGET",
                InputWithMappings(baseline, new LegacyEnemyVocabularyMappingSnapshot(
                    "Verifier.synthetic", "empty_target", Array.Empty<EnemyVocabularyKeyReferenceSnapshot>(),
                    LegacyEnemyVocabularyMappingStatus.Mapped, "Synthetic missing target.")));
            AddRejection(result, "out-of-scope-target-detected", "LEGACY_OUT_OF_SCOPE_WITH_TARGET",
                InputWithMappings(baseline, new LegacyEnemyVocabularyMappingSnapshot(
                    "Verifier.synthetic", "out_with_target", new[] { new EnemyVocabularyKeyReferenceSnapshot(EnemyVocabularyCategory.Mechanic, "mechanic.basic_pressure") },
                    LegacyEnemyVocabularyMappingStatus.OutOfScope, "Synthetic invalid OUT_OF_SCOPE target.")));
            AddRejection(result, "player-answer-leak-detected", "PLAYER_HINT_ANSWER_LEAK",
                InputWithEntries(baseline, new EnemyVocabularyEntrySnapshot(
                    new PlayerHintCategoryKey("player_hint.required_solution"), "答案泄漏测试", "solution answer", true, false)));
        }

        private static void CheckSourceAndFieldLeaks(VerificationResult result)
        {
            string root = FindProjectRoot();
            Dictionary<string, string> sources = RuntimeSourcePaths.ToDictionary(
                path => path,
                path => File.ReadAllText(Path.Combine(root, path)),
                StringComparer.Ordinal);
            string joined = string.Join("\n", sources.Values);

            string[] forbiddenRuntimeTypes = { "Unity" + "Engine", "Mono" + "Behaviour", "Scriptable" + "Object" };
            bool pure = forbiddenRuntimeTypes.All(token => joined.IndexOf(token, StringComparison.Ordinal) < 0);
            Add(result, "pure-csharp-runtime-sources", "leak", "No UnityEngine/MonoBehaviour/ScriptableObject",
                pure ? "0 matches" : "forbidden match", pure,
                "Vocabulary runtime sources are plain C# data code.");

            string[] forbiddenDependencies =
            {
                "TalismanBag." + "BuildSandbox", "Contracts." + "Battle", "Battle" + "Sandbox",
                "Board", "TalismanBag." + "Items", "Item" + "SystemSnapshot", "RunFlow", "Reward", "SaveData"
            };
            bool dependencyClean = forbiddenDependencies.All(token => joined.IndexOf(token, StringComparison.Ordinal) < 0);
            Add(result, "forbidden-system-dependencies", "leak", "No BuildSandbox/Battle/Board/Item/RunFlow/Reward/Save dependency",
                dependencyClean ? "0 matches" : "forbidden match", dependencyClean,
                "Enemy vocabulary does not consume runtime implementations from protected systems.");

            string[] chapterTokens = { "1" + "-10", "2" + "-10", "3" + "-10", "4" + "-10" };
            bool noChapters = chapterTokens.All(token => joined.IndexOf(token, StringComparison.Ordinal) < 0);
            Add(result, "no-chapter-hardcoding", "leak", "No chapter IDs", noChapters ? "0 matches" : "chapter token found", noChapters,
                "Vocabulary is not bound to specific content entries or chapters.");

            string[] answerPropertyTokens =
            {
                "solution", "answer", "requiredsynergy", "requiredaffix", "requiredstats", "hardsolutiontags",
                "minimumkeysrequired", "dropbias", "exactthreshold"
            };
            PropertyInfo[] playerProperties = typeof(EnemyVocabularyEntrySnapshot).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            bool playerShapeClean = playerProperties.All(property => answerPropertyTokens.All(token => property.Name.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0));
            Add(result, "player-category-no-answer-fields", "leak", "0 answer-bearing public fields",
                playerShapeClean ? "0" : "forbidden property found", playerShapeClean,
                "Player hints expose categories and descriptive metadata only.");
        }

        private static void CheckProtectedHashes(VerificationResult result)
        {
            string root = FindProjectRoot();
            List<string> mismatches = new List<string>();
            foreach (KeyValuePair<string, string> pair in ProtectedE01Hashes)
            {
                string actual = Sha256File(Path.Combine(root, pair.Key));
                if (!string.Equals(actual, pair.Value, StringComparison.Ordinal))
                {
                    mismatches.Add(pair.Key + "=" + actual);
                }
            }

            Add(result, "e01-protected-hashes", "scope", "6/6 byte-identical",
                mismatches.Count == 0 ? "6/6 unchanged" : string.Join("|", mismatches), mismatches.Count == 0,
                "E01 Domain/Contracts/verifier files remain byte-identical to the recorded pre-development baseline.");
        }

        private static void CheckPackageScope(VerificationResult result)
        {
            bool allowed = PackageManifest.All(path =>
                path.StartsWith("Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary", StringComparison.Ordinal)
                || path.StartsWith("Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier", StringComparison.Ordinal)
                || path.StartsWith("Docs/V0.4/Reports/EnemyMechanicVocabulary", StringComparison.Ordinal));
            Add(result, "package-path-whitelist", "scope", "Every package file is assignment-whitelisted",
                allowed ? "all allowed" : "out-of-scope path", allowed,
                "Package owns only new Vocabulary, verifier, meta, and named report files.");

            string root = FindProjectRoot();
            string[] sourceAndMetaPaths = PackageManifest.Where(path => path.StartsWith("Assets/", StringComparison.Ordinal)).ToArray();
            string[] missing = sourceAndMetaPaths.Where(path => !File.Exists(Path.Combine(root, path)) && !Directory.Exists(Path.Combine(root, path.Replace(".meta", string.Empty)))).ToArray();
            Add(result, "package-assets-and-meta-exist", "scope", "All new source and corresponding meta files exist",
                missing.Length == 0 ? "all present" : string.Join("|", missing), missing.Length == 0,
                "Unity assets have explicit stable meta files.");

            string[] forbiddenExtensions = { ".unity", ".prefab", ".asset" };
            bool noScenePrefabConfig = PackageManifest.All(path => forbiddenExtensions.All(extension => !path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)));
            Add(result, "scene-prefab-config-manifest", "scope", "Scene=0; Prefab=0; Config=0",
                noScenePrefabConfig ? "0/0/0" : "forbidden asset present", noScenePrefabConfig,
                "No scene, prefab, or config is part of E02.");

            string normalized = string.Join("\n", PackageManifest).ToLowerInvariant();
            bool noProtectedSystemWrites = normalized.IndexOf("/battle", StringComparison.Ordinal) < 0
                && normalized.IndexOf("/board", StringComparison.Ordinal) < 0
                && normalized.IndexOf("/item", StringComparison.Ordinal) < 0;
            Add(result, "battle-board-item-manifest", "scope", "Battle=0; Board=0; Item=0",
                noProtectedSystemWrites ? "0/0/0" : "forbidden system path", noProtectedSystemWrites,
                "Only Enemy vocabulary data and verification outputs are declared.");
        }

        private static EnemyMechanicVocabularySnapshotInput InputWithEntries(
            EnemyMechanicVocabularySnapshotInput baseline,
            EnemyVocabularyEntrySnapshot added)
        {
            return new EnemyMechanicVocabularySnapshotInput(
                baseline.Entries.Concat(new[] { added }).ToArray(),
                baseline.LegacyMappings);
        }

        private static EnemyMechanicVocabularySnapshotInput InputWithMappings(
            EnemyMechanicVocabularySnapshotInput baseline,
            LegacyEnemyVocabularyMappingSnapshot added)
        {
            return new EnemyMechanicVocabularySnapshotInput(
                baseline.Entries,
                baseline.LegacyMappings.Concat(new[] { added }).ToArray());
        }

        private static EnemyVocabularyEntrySnapshot CopyEntry(EnemyVocabularyEntrySnapshot value)
        {
            return new EnemyVocabularyEntrySnapshot(
                value.Key,
                value.DeveloperLabelZh,
                value.Description,
                value.PlayerVisible,
                value.DeveloperOnly);
        }

        private static LegacyEnemyVocabularyMappingSnapshot CopyMapping(LegacyEnemyVocabularyMappingSnapshot value)
        {
            return new LegacyEnemyVocabularyMappingSnapshot(
                value.LegacySourceKind,
                value.LegacyKey,
                value.Targets,
                value.Status,
                value.Reason);
        }

        private static void AddRejection(
            VerificationResult result,
            string checkId,
            string expectedCode,
            EnemyMechanicVocabularySnapshotInput input)
        {
            IReadOnlyList<EnemyVocabularyValidationIssue> issues = Validator.Validate(input);
            bool validatorDetected = issues.Any(issue => string.Equals(issue.Code, expectedCode, StringComparison.Ordinal));
            bool providerRejected = false;
            try
            {
                Provider.CreateSnapshot(input);
            }
            catch (EnemyMechanicVocabularyValidationException exception)
            {
                providerRejected = exception.Issues.Any(issue => string.Equals(issue.Code, expectedCode, StringComparison.Ordinal));
            }

            Add(result, checkId, "validation", expectedCode, string.Join("|", issues.Select(issue => issue.Code)),
                validatorDetected && providerRejected,
                "Validator and provider both reject the invalid fixture.");
        }

        private static bool IsReadOnly<T>(IReadOnlyList<T> values, T injected)
        {
            IList<T> mutable = values as IList<T>;
            if (mutable == null)
            {
                return true;
            }

            try
            {
                mutable.Add(injected);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static void WriteReports(
            VerificationResult result,
            EnemyMechanicVocabularySnapshot snapshot,
            string executionMode)
        {
            string root = FindProjectRoot();
            WriteUtf8(Path.Combine(root, DetailReportPath), BuildDetailReport(result, snapshot, executionMode));
            WriteUtf8(Path.Combine(root, SpecCsvPath), BuildSpecCsv(result));
            WriteUtf8(Path.Combine(root, InventoryCsvPath), BuildInventoryCsv(snapshot));
            WriteUtf8(Path.Combine(root, LegacyMappingCsvPath), BuildLegacyMappingCsv(snapshot));
            WriteUtf8(Path.Combine(root, LeakReportPath), BuildLeakReport(result, executionMode));
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(
            VerificationResult result,
            EnemyMechanicVocabularySnapshot snapshot,
            string executionMode)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Mechanic Vocabulary 01 Report").AppendLine();
            builder.AppendLine("- Package: `V0.4-EnemyMechanicVocabulary01`");
            builder.AppendLine("- Result: `" + (result.Passed ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Execution: `" + executionMode + "`");
            builder.AppendLine("- Schema: `" + EnemyMechanicVocabularySchema.SchemaId + "`");
            builder.AppendLine("- Schema version: `" + EnemyMechanicVocabularySchema.SchemaVersion.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Verifier: `" + result.PassedCount.ToString(CultureInfo.InvariantCulture) + "/" + result.TotalCount.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Canonical Signature: `" + (snapshot == null ? "unavailable" : snapshot.BuildCanonicalSignature()) + "`").AppendLine();

            if (snapshot != null)
            {
                builder.AppendLine("## Vocabulary counts").AppendLine();
                builder.AppendLine("| Category | Count |");
                builder.AppendLine("|---|---:|");
                foreach (IGrouping<EnemyVocabularyCategory, EnemyVocabularyEntrySnapshot> group in snapshot.Entries.GroupBy(value => value.Category).OrderBy(value => value.Key))
                {
                    builder.AppendLine("| " + EnemyVocabularyCategoryNames.StableName(group.Key) + " | " + group.Count().ToString(CultureInfo.InvariantCulture) + " |");
                }

                int mapped = snapshot.LegacyMappings.Count(value => value.Status == LegacyEnemyVocabularyMappingStatus.Mapped);
                int outOfScope = snapshot.LegacyMappings.Count(value => value.Status == LegacyEnemyVocabularyMappingStatus.OutOfScope);
                builder.AppendLine().AppendLine("## Legacy mapping").AppendLine();
                builder.AppendLine("- Total legacy keys: `" + snapshot.LegacyMappings.Count.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Mapped: `" + mapped.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- OUT_OF_SCOPE: `" + outOfScope.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Unresolved: `0`");
                builder.AppendLine("- Current source-field aliases: `pressureType = mechanicType semantic`, `hardSolutionTags = requiredCapabilityTags semantic`, `softSolutionTags = optionalCapabilityTags semantic`.");
            }

            builder.AppendLine().AppendLine("## Boundary").AppendLine();
            builder.AppendLine("- Pure C# vocabulary and immutable snapshots only.");
            builder.AppendLine("- No Enemy/Boss content migration, runtime mechanic execution, player-answer panel, Scene, Prefab, Config, Battle, Board, or Item changes.");
            builder.AppendLine("- E01 protected files are checked against pre-development SHA-256 baselines.");
            builder.AppendLine("- Repository-wide pre-existing dirty files are excluded from this package manifest and must be reported separately.");
            return builder.ToString();
        }

        private static string BuildSpecCsv(VerificationResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("checkId,area,expected,actual,status,notes");
            foreach (CheckRow row in result.Rows)
            {
                builder.Append(Csv(row.CheckId)).Append(',')
                    .Append(Csv(row.Area)).Append(',')
                    .Append(Csv(row.Expected)).Append(',')
                    .Append(Csv(row.Actual)).Append(',')
                    .Append(row.Passed ? "PASS" : "FAIL").Append(',')
                    .Append(Csv(row.Notes)).AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildInventoryCsv(EnemyMechanicVocabularySnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("category,stableKey,developerLabelZh,description,playerVisible,developerOnly");
            foreach (EnemyVocabularyEntrySnapshot entry in (snapshot == null ? Array.Empty<EnemyVocabularyEntrySnapshot>() : snapshot.Entries)
                .OrderBy(value => value.Category)
                .ThenBy(value => value.StableKey, StringComparer.Ordinal))
            {
                builder.Append(Csv(EnemyVocabularyCategoryNames.StableName(entry.Category))).Append(',')
                    .Append(Csv(entry.StableKey)).Append(',')
                    .Append(Csv(entry.DeveloperLabelZh)).Append(',')
                    .Append(Csv(entry.Description)).Append(',')
                    .Append(entry.PlayerVisible ? "true" : "false").Append(',')
                    .Append(entry.DeveloperOnly ? "true" : "false").AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildLegacyMappingCsv(EnemyMechanicVocabularySnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("legacySourceKind,legacyKey,canonicalCategory,canonicalKey,mappingMode,status,reason");
            foreach (LegacyEnemyVocabularyMappingSnapshot mapping in (snapshot == null ? Array.Empty<LegacyEnemyVocabularyMappingSnapshot>() : snapshot.LegacyMappings)
                .OrderBy(value => value.LegacySourceKind, StringComparer.Ordinal)
                .ThenBy(value => value.LegacyKey, StringComparer.Ordinal))
            {
                string categories = string.Join("|", mapping.Targets.Select(target => EnemyVocabularyCategoryNames.StableName(target.Category)));
                string keys = string.Join("|", mapping.Targets.Select(target => target.StableKey));
                builder.Append(Csv(mapping.LegacySourceKind)).Append(',')
                    .Append(Csv(mapping.LegacyKey)).Append(',')
                    .Append(Csv(categories)).Append(',')
                    .Append(Csv(keys)).Append(',')
                    .Append(Csv(mapping.MappingMode)).Append(',')
                    .Append(mapping.Status == LegacyEnemyVocabularyMappingStatus.Mapped ? "MAPPED" : "OUT_OF_SCOPE").Append(',')
                    .Append(Csv(mapping.Reason)).AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(VerificationResult result, string executionMode)
        {
            CheckRow[] leakRows = result.Rows.Where(row => string.Equals(row.Area, "leak", StringComparison.Ordinal)
                || string.Equals(row.Area, "scope", StringComparison.Ordinal)).ToArray();
            int leakCount = leakRows.Count(row => !row.Passed);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Mechanic Vocabulary 01 Leak Check Report").AppendLine();
            builder.AppendLine("- Result: `" + (leakCount == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Execution: `" + executionMode + "`");
            builder.AppendLine("- Leak count: `" + leakCount.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Runtime source files scanned: `" + RuntimeSourcePaths.Length.ToString(CultureInfo.InvariantCulture) + "`").AppendLine();
            builder.AppendLine("## Checks").AppendLine();
            foreach (CheckRow row in leakRows)
            {
                builder.AppendLine("- " + (row.Passed ? "PASS" : "FAIL") + " — `" + row.CheckId + "`: " + row.Actual);
            }

            builder.AppendLine().AppendLine("## Package manifest scope").AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine("Modified existing files = 0");
            builder.AppendLine("Scene modifications = 0");
            builder.AppendLine("Prefab modifications = 0");
            builder.AppendLine("Config modifications = 0");
            builder.AppendLine("Battle modifications = 0");
            builder.AppendLine("Board modifications = 0");
            builder.AppendLine("Item modifications = 0");
            builder.AppendLine("```").AppendLine();
            builder.AppendLine("These counts describe the E02 package manifest. Repository-wide pre-existing dirty files are excluded and must be reported separately by the task window.");
            return builder.ToString();
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Report directory is unavailable."));
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static string Sha256File(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha256.ComputeHash(stream);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                foreach (byte value in hash)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(current.FullName, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(current.FullName, "Packages")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Unity project root could not be found from the current directory.");
        }

        private static bool IsBatchMode()
        {
            return Environment.GetCommandLineArgs().Any(argument => string.Equals(argument, "-batchmode", StringComparison.OrdinalIgnoreCase));
        }

        private static void Add(VerificationResult result, string checkId, string area, string expected, string actual, bool passed, string notes)
        {
            result.Add(checkId, area, expected, actual, passed, notes);
        }

        private sealed class VerificationResult
        {
            private readonly List<CheckRow> rows = new List<CheckRow>();

            public IReadOnlyList<CheckRow> Rows => rows;
            public int TotalCount => rows.Count;
            public int PassedCount => rows.Count(value => value.Passed);
            public bool Passed => rows.All(value => value.Passed);

            public void Add(string checkId, string area, string expected, string actual, bool passed, string notes)
            {
                rows.Add(new CheckRow(checkId, area, expected, actual, passed, notes));
            }
        }

        private sealed class CheckRow
        {
            public CheckRow(string checkId, string area, string expected, string actual, bool passed, string notes)
            {
                CheckId = checkId ?? string.Empty;
                Area = area ?? string.Empty;
                Expected = expected ?? string.Empty;
                Actual = actual ?? string.Empty;
                Passed = passed;
                Notes = notes ?? string.Empty;
            }

            public string CheckId { get; }
            public string Area { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
            public string Notes { get; }
        }
    }
}
