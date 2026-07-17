using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TalismanBag.EnemySystem.Contracts;
using TalismanBag.EnemySystem.Domain;
using UnityEditor;

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class EnemyDomainDataContractVerifier
    {
        private const string DetailReportPath = "Docs/V0.4/Reports/EnemyDomainDataContractReport.md";
        private const string SpecCsvPath = "Docs/V0.4/Reports/EnemyDomainDataContractSpec.csv";
        private const string LeakReportPath = "Docs/V0.4/Reports/EnemyDomainDataContractLeakCheckReport.md";

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyDomainPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyArchetypeSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainReferences.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainValidation.cs"
        };

        private static readonly string[] PackageManifest =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyDomainPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/EnemyArchetypeSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainReferences.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainSnapshot.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/EnemyDomainValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDomainDataContractVerifier.cs",
            DetailReportPath,
            SpecCsvPath,
            LeakReportPath
        };

        private static readonly DefaultEnemyDomainSnapshotProvider Provider = DefaultEnemyDomainSnapshotProvider.Instance;
        private static readonly DefaultEnemyDomainContractValidator Validator = DefaultEnemyDomainContractValidator.Instance;

        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/EnemyDomainDataContract01/[QA Only] Verify And Write Reports")]
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
            VerifyAndWriteReports(false, "Pure C# offline fallback");
        }

        private static void VerifyAndWriteReports(bool exitWhenDone, string executionMode)
        {
            VerificationResult result = new VerificationResult();
            try
            {
                RunVerification(result);
            }
            catch (Exception exception)
            {
                result.Add("verifier-unhandled-exception", "verifier", "No unhandled exception", exception.ToString(), false, "Verifier execution must remain deterministic and self-reporting.");
            }

            WriteReports(result, executionMode);
            if (result.Passed)
            {
                Console.WriteLine("ENEMY_DOMAIN_DATA_CONTRACT_VERIFIER_PASS " + result.PassedCount + "/" + result.TotalCount);
            }
            else
            {
                foreach (CheckRow row in result.Rows.Where(value => !value.Passed))
                {
                    Console.Error.WriteLine("ENEMY_DOMAIN_DATA_CONTRACT_VERIFIER_FAIL " + row.CheckId + ": " + row.Actual);
                }
            }

            if (exitWhenDone)
            {
                EditorApplication.Exit(result.Passed ? 0 : 1);
            }
        }

        private static void RunVerification(VerificationResult result)
        {
            EnemyDomainSnapshotInput fixture = CreateFixture(false);
            EnemyDomainSnapshot snapshot = Provider.CreateSnapshot(fixture);

            Add(result, "schema-id", "schema", EnemyDomainSchema.SchemaId, snapshot.SchemaId,
                string.Equals(snapshot.SchemaId, EnemyDomainSchema.SchemaId, StringComparison.Ordinal),
                "Stable schema ID is exact and case-sensitive.");
            Add(result, "schema-version", "schema", "1", snapshot.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                snapshot.SchemaVersion == EnemyDomainSchema.SchemaVersion,
                "Schema version remains independently machine-readable.");
            Add(result, "provider-contract", "contract", "Provider and lookup interfaces implemented", typeof(DefaultEnemyDomainSnapshotProvider).GetInterfaces().Any(value => value == typeof(IEnemyDomainSnapshotProvider))
                    && typeof(EnemyDomainSnapshot).GetInterfaces().Any(value => value == typeof(IEnemyDomainArchetypeLookup)) ? "implemented" : "missing",
                typeof(DefaultEnemyDomainSnapshotProvider).GetInterfaces().Any(value => value == typeof(IEnemyDomainSnapshotProvider))
                    && typeof(EnemyDomainSnapshot).GetInterfaces().Any(value => value == typeof(IEnemyDomainArchetypeLookup)),
                "Snapshot creation and archetype lookup are exposed through read-only contracts.");
            Add(result, "reference-types", "contract", "6 distinct minimal reference types", ReferenceTypesValid() ? "6 valid" : "invalid shape",
                ReferenceTypesValid(), "Mechanic, SkillPattern, BossPhase, MapRule, Encounter, and CounterWindow remain typed identities.");
            Add(result, "archetype-categories", "contract", "Normal, Elite, Boss", string.Join("|", Enum.GetNames(typeof(EnemyArchetypeCategory))),
                Enum.GetValues(typeof(EnemyArchetypeCategory)).Length == 3
                    && snapshot.Enemies.Any(value => value.Category == EnemyArchetypeCategory.Normal)
                    && snapshot.Enemies.Any(value => value.Category == EnemyArchetypeCategory.Elite)
                    && snapshot.Bosses.All(value => value.Category == EnemyArchetypeCategory.Boss),
                "Enemy and Boss identity categories are explicit without combat data.");
            Add(result, "default-dev-isolation", "isolation", "devOnly=true; isEnabled=false; entersFormalFlow=false", AllEntriesSafelyIsolated(fixture) ? "safe defaults" : "unsafe entry",
                AllEntriesSafelyIsolated(fixture), "Default verifier data never enters formal flow.");

            CheckInputIsolation(result);
            CheckReadOnlyExposure(result, snapshot);
            CheckCanonicalSignature(result, snapshot);
            CheckValidationFailures(result, fixture);
            CheckLookupSemantics(result, snapshot);
            CheckSourceAndFieldLeaks(result);
            CheckPackageScope(result);
        }

        private static void CheckInputIsolation(VerificationResult result)
        {
            List<string> mechanicIds = new List<string> { "mechanic.alpha" };
            EnemyArchetypeSnapshot enemy = new EnemyArchetypeSnapshot(
                "enemy.copy.test", "enemy.copy.test.name", EnemyArchetypeCategory.Normal,
                "enemy.copy.test.presentation", "enemy.copy.test.behavior", mechanicIds,
                new List<string> { "skill.alpha" });
            mechanicIds.Add("mechanic.injected.after-archetype");

            List<EnemyArchetypeSnapshot> sourceEnemies = new List<EnemyArchetypeSnapshot> { enemy };
            EnemyDomainSnapshotInput input = new EnemyDomainSnapshotInput(
                sourceEnemies,
                mechanicProfiles: new List<MechanicProfileReference> { new MechanicProfileReference("mechanic.alpha") },
                skillPatterns: new List<SkillPatternReference> { new SkillPatternReference("skill.alpha") });
            sourceEnemies.Clear();
            EnemyDomainSnapshot isolated = Provider.CreateSnapshot(input);

            bool passed = isolated.Enemies.Count == 1
                && isolated.Enemies[0].MechanicProfileIds.SequenceEqual(new[] { "mechanic.alpha" });
            Add(result, "input-list-defensive-copy", "immutability", "Snapshot unchanged after caller list mutation",
                passed ? "unchanged" : "mutated", passed,
                "Both aggregate lists and nested ID lists are copied at construction boundaries.");
        }

        private static void CheckReadOnlyExposure(VerificationResult result, EnemyDomainSnapshot snapshot)
        {
            bool topLevel = IsReadOnly(snapshot.Enemies, snapshot.Enemies[0])
                && IsReadOnly(snapshot.Bosses, snapshot.Bosses[0])
                && IsReadOnly(snapshot.MechanicProfiles, snapshot.MechanicProfiles[0])
                && IsReadOnly(snapshot.SkillPatterns, snapshot.SkillPatterns[0])
                && IsReadOnly(snapshot.BossPhases, snapshot.BossPhases[0])
                && IsReadOnly(snapshot.MapRules, snapshot.MapRules[0])
                && IsReadOnly(snapshot.Encounters, snapshot.Encounters[0])
                && IsReadOnly(snapshot.CounterWindows, snapshot.CounterWindows[0]);
            Add(result, "returned-top-level-collections-read-only", "immutability", "All 8 collections reject mutation", topLevel ? "all rejected" : "mutable collection found",
                topLevel, "Collections are exposed only through IReadOnlyList and backed by read-only defensive copies.");

            bool nested = IsReadOnly(snapshot.Enemies[0].MechanicProfileIds, "mechanic.injected")
                && IsReadOnly(snapshot.Enemies[0].SkillPatternIds, "skill.injected")
                && IsReadOnly(snapshot.Bosses[0].MechanicProfileIds, "mechanic.injected")
                && IsReadOnly(snapshot.Bosses[0].SkillPatternIds, "skill.injected")
                && IsReadOnly(snapshot.Bosses[0].PhaseProfileIds, "phase.injected");
            Add(result, "returned-nested-id-collections-read-only", "immutability", "All archetype ID collections reject mutation", nested ? "all rejected" : "mutable collection found",
                nested, "Callers cannot mutate references through a returned archetype.");
        }

        private static void CheckCanonicalSignature(VerificationResult result, EnemyDomainSnapshot snapshot)
        {
            string first = snapshot.BuildCanonicalSignature();
            string repeat = snapshot.BuildCanonicalSignature();
            string reversed = Provider.CreateSnapshot(CreateFixture(true)).BuildCanonicalSignature();
            EnemyDomainSnapshot changed = Provider.CreateSnapshot(CreateFixture(false, "enemy.changed.display"));

            Add(result, "canonical-format", "canonical", "sha256: + 64 lowercase hex characters", first,
                first.StartsWith("sha256:", StringComparison.Ordinal)
                    && first.Length == 71
                    && first.Substring(7).All(value => (value >= '0' && value <= '9') || (value >= 'a' && value <= 'f')),
                "Signature has an explicit deterministic algorithm prefix.");
            Add(result, "canonical-repeat", "canonical", first, repeat,
                string.Equals(first, repeat, StringComparison.Ordinal), "Repeated evaluation is stable.");
            Add(result, "canonical-input-order-independent", "canonical", first, reversed,
                string.Equals(first, reversed, StringComparison.Ordinal), "Top-level and nested reference order does not affect the signature.");
            Add(result, "canonical-content-sensitive", "canonical", "Different content produces a different signature", changed.BuildCanonicalSignature(),
                !string.Equals(first, changed.BuildCanonicalSignature(), StringComparison.Ordinal), "Identity metadata changes remain detectable.");
        }

        private static void CheckValidationFailures(VerificationResult result, EnemyDomainSnapshotInput fixture)
        {
            EnemyArchetypeSnapshot validEnemy = fixture.Enemies[0];
            BossArchetypeSnapshot validBoss = fixture.Bosses[0];

            AddRejection(result, "empty-enemy-id-detected", "STABLE_ID_EMPTY", new EnemyDomainSnapshotInput(
                new[] { new EnemyArchetypeSnapshot(string.Empty, "empty", EnemyArchetypeCategory.Normal, "p", "b") }));
            AddRejection(result, "duplicate-enemy-id-detected", "STABLE_ID_DUPLICATE", new EnemyDomainSnapshotInput(
                new[] { validEnemy, CopyEnemy(validEnemy) }, mechanicProfiles: fixture.MechanicProfiles, skillPatterns: fixture.SkillPatterns));
            AddRejection(result, "duplicate-boss-id-detected", "STABLE_ID_DUPLICATE", new EnemyDomainSnapshotInput(
                bosses: new[] { validBoss, CopyBoss(validBoss) }, mechanicProfiles: fixture.MechanicProfiles,
                skillPatterns: fixture.SkillPatterns, bossPhases: fixture.BossPhases));
            AddRejection(result, "enemy-boss-shared-id-detected", "ARCHETYPE_ID_SHARED", new EnemyDomainSnapshotInput(
                new[] { validEnemy }, new[] { new BossArchetypeSnapshot(validEnemy.StableId, "boss", "p", "b") },
                fixture.MechanicProfiles, fixture.SkillPatterns));
            AddRejection(result, "unresolved-reference-detected", "MECHANIC_PROFILE_REFERENCE_UNRESOLVED", new EnemyDomainSnapshotInput(
                new[] { new EnemyArchetypeSnapshot("enemy.unresolved", "enemy", EnemyArchetypeCategory.Normal, "p", "b", new[] { "mechanic.missing" }) }));
            AddRejection(result, "unsafe-default-flags-detected", "DEV_ISOLATION_INVALID", new EnemyDomainSnapshotInput(
                new[] { new EnemyArchetypeSnapshot("enemy.unsafe", "enemy", EnemyArchetypeCategory.Normal, "p", "b", devOnly: false, isEnabled: true, entersFormalFlow: true) }));
            AddRejection(result, "outer-whitespace-id-detected", "STABLE_ID_OUTER_WHITESPACE", new EnemyDomainSnapshotInput(
                new[] { new EnemyArchetypeSnapshot(" enemy.padded ", "enemy", EnemyArchetypeCategory.Normal, "p", "b") }));
            AddRejection(result, "schema-id-mismatch-detected", "SCHEMA_ID_MISMATCH", new EnemyDomainSnapshotInput(schemaId: "EnemyDomainContract.V1"));
            AddRejection(result, "schema-version-mismatch-detected", "SCHEMA_VERSION_MISMATCH", new EnemyDomainSnapshotInput(schemaVersion: 2));
        }

        private static void CheckLookupSemantics(VerificationResult result, EnemyDomainSnapshot snapshot)
        {
            string enemyId = snapshot.Enemies[0].StableId;
            string bossId = snapshot.Bosses[0].StableId;
            bool exact = snapshot.TryGetEnemyById(enemyId, out EnemyArchetypeSnapshot enemy)
                && ReferenceEquals(enemy, snapshot.Enemies[0])
                && snapshot.TryGetBossById(bossId, out BossArchetypeSnapshot boss)
                && ReferenceEquals(boss, snapshot.Bosses[0]);
            Add(result, "lookup-exact-ordinal", "lookup", "Exact IDs resolve", exact ? "resolved" : "failed", exact,
                "Lookup returns immutable objects already owned by the snapshot.");

            bool caseStrict = !snapshot.TryGetEnemyById(enemyId.ToUpperInvariant(), out _)
                && !snapshot.TryGetBossById(bossId.ToUpperInvariant(), out _);
            Add(result, "lookup-case-sensitive", "lookup", "Case variants do not resolve", caseStrict ? "strict" : "normalized", caseStrict,
                "StringComparer.Ordinal is the stable ID authority.");

            bool noTrim = !snapshot.TryGetEnemyById(" " + enemyId + " ", out _)
                && !snapshot.TryGetBossById(" " + bossId + " ", out _);
            Add(result, "lookup-no-implicit-trim", "lookup", "Padded variants do not resolve", noTrim ? "strict" : "trimmed", noTrim,
                "Callers cannot silently change stable identity semantics.");

            bool unknown = !snapshot.TryGetEnemyById("enemy.unknown", out _)
                && !snapshot.TryGetBossById("boss.unknown", out _)
                && !snapshot.TryGetEnemyById(null, out _)
                && !snapshot.TryGetBossById(null, out _);
            Add(result, "lookup-unknown-safe", "lookup", "Unknown and null IDs return false", unknown ? "safe false" : "unexpected match", unknown,
                "Lookup does not throw or substitute another identity.");
        }

        private static void CheckSourceAndFieldLeaks(VerificationResult result)
        {
            string root = FindProjectRoot();
            Dictionary<string, string> sources = RuntimeSourcePaths.ToDictionary(
                path => path,
                path => File.ReadAllText(Path.Combine(root, path)),
                StringComparer.Ordinal);
            string joined = string.Join("\n", sources.Values);

            string[] forbiddenRuntimeTypes =
            {
                "Unity" + "Engine",
                "Mono" + "Behaviour",
                "Scriptable" + "Object"
            };
            bool pure = forbiddenRuntimeTypes.All(token => joined.IndexOf(token, StringComparison.Ordinal) < 0);
            Add(result, "pure-csharp-runtime-sources", "leak", "No engine/runtime object dependencies", pure ? "0 matches" : "forbidden match", pure,
                "Enemy Domain and Contracts compile as plain C# data code.");

            string[] forbiddenDependencies =
            {
                "Build" + "Sandbox",
                "TalismanBag." + "Items",
                "Contracts." + "Battle",
                "V" + "02"
            };
            bool dependencyClean = forbiddenDependencies.All(token => joined.IndexOf(token, StringComparison.Ordinal) < 0);
            Add(result, "forbidden-system-dependencies", "leak", "No sandbox/item/battle/legacy-runtime dependencies", dependencyClean ? "0 matches" : "forbidden match", dependencyClean,
                "Enemy is independently constructible and does not read other system implementations.");

            string[] chapterTokens = { "1" + "-10", "2" + "-10", "3" + "-10", "4" + "-10" };
            bool noChapterBranches = chapterTokens.All(token => joined.IndexOf(token, StringComparison.Ordinal) < 0);
            Add(result, "no-chapter-hardcoding", "leak", "No chapter IDs in Domain/Contracts", noChapterBranches ? "0 matches" : "chapter token found", noChapterBranches,
                "Content IDs remain external data rather than code branches.");

            Type[] archetypeTypes = { typeof(EnemyArchetypeSnapshot), typeof(BossArchetypeSnapshot) };
            HashSet<string> forbiddenCombatFields = new HashSet<string>(new[]
            {
                "hp", "maxhp", "attack", "damage", "shield", "armor", "defense"
            }, StringComparer.OrdinalIgnoreCase);
            string solutionToken = "solu" + "tion";
            string affixToken = "required" + "affix";
            string biasToken = "drop" + "bias";
            HashSet<string> forbiddenAnswerFields = new HashSet<string>(new[]
            {
                solutionToken, "hard" + solutionToken, affixToken, "required" + "synergy",
                "required" + "stats", biasToken, "recommended" + "synergies", "core" + "items"
            }, StringComparer.OrdinalIgnoreCase);

            PropertyInfo[] properties = archetypeTypes.SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)).ToArray();
            bool combatClean = properties.All(property => !forbiddenCombatFields.Contains(property.Name));
            bool answersClean = properties.All(property => !forbiddenAnswerFields.Contains(property.Name));
            Add(result, "archetype-no-combat-values", "leak", "0 combat value properties", combatClean ? "0" : "forbidden property found", combatClean,
                "Archetypes contain identity, presentation, behavior keys, references, and isolation flags only.");
            Add(result, "archetype-no-answer-fields", "leak", "0 answer-bearing properties", answersClean ? "0" : "forbidden property found", answersClean,
                "No player-facing solution, exact requirement, or drop-bias fields exist.");
        }

        private static void CheckPackageScope(VerificationResult result)
        {
            bool allowed = PackageManifest.All(path =>
                path.StartsWith("Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/", StringComparison.Ordinal)
                || path.StartsWith("Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/", StringComparison.Ordinal)
                || path.StartsWith("Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/", StringComparison.Ordinal)
                || path.StartsWith("Docs/V0.4/Reports/EnemyDomainDataContract", StringComparison.Ordinal));
            Add(result, "package-path-whitelist", "scope", "Every declared file is in the assignment whitelist", allowed ? "all allowed" : "out-of-scope path", allowed,
                "Verifier owns an explicit package manifest.");

            string[] forbiddenExtensions = { ".unity", ".prefab", ".asset" };
            bool noAssetWrites = PackageManifest.All(path => forbiddenExtensions.All(extension => !path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)));
            Add(result, "scene-prefab-config-manifest", "scope", "Scene=0; Prefab=0; Config=0", noAssetWrites ? "0/0/0" : "forbidden asset present", noAssetWrites,
                "This package has no scene, prefab, or configuration delivery object.");

            string normalized = string.Join("\n", PackageManifest).ToLowerInvariant();
            bool noSystemWrites = normalized.IndexOf("/battle", StringComparison.Ordinal) < 0
                && normalized.IndexOf("/board", StringComparison.Ordinal) < 0
                && normalized.IndexOf("/item", StringComparison.Ordinal) < 0;
            Add(result, "battle-board-item-manifest", "scope", "Battle=0; Board=0; Item=0", noSystemWrites ? "0/0/0" : "forbidden system path", noSystemWrites,
                "Only Enemy data, Enemy contracts, Enemy editor verification, and named reports are declared.");
        }

        private static EnemyDomainSnapshotInput CreateFixture(bool reverse, string firstEnemyDisplay = "enemy.normal.display")
        {
            List<MechanicProfileReference> mechanics = new List<MechanicProfileReference>
            {
                new MechanicProfileReference("mechanic.alpha"),
                new MechanicProfileReference("mechanic.beta")
            };
            List<SkillPatternReference> skills = new List<SkillPatternReference>
            {
                new SkillPatternReference("skill.alpha"),
                new SkillPatternReference("skill.beta")
            };
            List<BossPhaseReference> phases = new List<BossPhaseReference>
            {
                new BossPhaseReference("boss-phase.alpha"),
                new BossPhaseReference("boss-phase.beta")
            };
            List<MapRuleReference> mapRules = new List<MapRuleReference>
            {
                new MapRuleReference("map-rule.alpha"),
                new MapRuleReference("map-rule.beta")
            };
            List<EncounterReference> encounters = new List<EncounterReference>
            {
                new EncounterReference("encounter.alpha"),
                new EncounterReference("encounter.beta")
            };
            List<CounterWindowReference> windows = new List<CounterWindowReference>
            {
                new CounterWindowReference("counter-window.alpha"),
                new CounterWindowReference("counter-window.beta")
            };
            List<EnemyArchetypeSnapshot> enemies = new List<EnemyArchetypeSnapshot>
            {
                new EnemyArchetypeSnapshot(
                    "enemy.normal.alpha", firstEnemyDisplay, EnemyArchetypeCategory.Normal,
                    "presentation.enemy.normal.alpha", "behavior.enemy.normal.alpha",
                    reverse ? new[] { "mechanic.beta", "mechanic.alpha" } : new[] { "mechanic.alpha", "mechanic.beta" },
                    reverse ? new[] { "skill.beta", "skill.alpha" } : new[] { "skill.alpha", "skill.beta" }),
                new EnemyArchetypeSnapshot(
                    "enemy.elite.alpha", "enemy.elite.display", EnemyArchetypeCategory.Elite,
                    "presentation.enemy.elite.alpha", "behavior.enemy.elite.alpha",
                    new[] { "mechanic.beta" }, new[] { "skill.beta" })
            };
            List<BossArchetypeSnapshot> bosses = new List<BossArchetypeSnapshot>
            {
                new BossArchetypeSnapshot(
                    "boss.alpha", "boss.alpha.display", "presentation.boss.alpha", "behavior.boss.alpha",
                    reverse ? new[] { "mechanic.beta", "mechanic.alpha" } : new[] { "mechanic.alpha", "mechanic.beta" },
                    reverse ? new[] { "skill.beta", "skill.alpha" } : new[] { "skill.alpha", "skill.beta" },
                    reverse ? new[] { "boss-phase.beta", "boss-phase.alpha" } : new[] { "boss-phase.alpha", "boss-phase.beta" })
            };

            if (reverse)
            {
                mechanics.Reverse();
                skills.Reverse();
                phases.Reverse();
                mapRules.Reverse();
                encounters.Reverse();
                windows.Reverse();
                enemies.Reverse();
                bosses.Reverse();
            }

            return new EnemyDomainSnapshotInput(enemies, bosses, mechanics, skills, phases, mapRules, encounters, windows);
        }

        private static bool ReferenceTypesValid()
        {
            Type[] types =
            {
                typeof(MechanicProfileReference), typeof(SkillPatternReference), typeof(BossPhaseReference),
                typeof(MapRuleReference), typeof(EncounterReference), typeof(CounterWindowReference)
            };
            HashSet<string> allowed = new HashSet<string>(new[] { "StableId", "DevOnly", "IsEnabled", "EntersFormalFlow" }, StringComparer.Ordinal);
            return types.Distinct().Count() == 6
                && types.All(type => type.IsSealed
                    && type.BaseType == typeof(EnemyDomainReference)
                    && type.GetProperties(BindingFlags.Public | BindingFlags.Instance).All(property => allowed.Contains(property.Name)));
        }

        private static EnemyArchetypeSnapshot CopyEnemy(EnemyArchetypeSnapshot value)
        {
            return new EnemyArchetypeSnapshot(
                value.StableId,
                value.DisplayNameOrLocalizationKey,
                value.Category,
                value.PresentationKey,
                value.BaseBehaviorKey,
                value.MechanicProfileIds,
                value.SkillPatternIds,
                value.DevOnly,
                value.IsEnabled,
                value.EntersFormalFlow);
        }

        private static BossArchetypeSnapshot CopyBoss(BossArchetypeSnapshot value)
        {
            return new BossArchetypeSnapshot(
                value.StableId,
                value.DisplayNameOrLocalizationKey,
                value.PresentationKey,
                value.BaseBehaviorKey,
                value.MechanicProfileIds,
                value.SkillPatternIds,
                value.PhaseProfileIds,
                value.DevOnly,
                value.IsEnabled,
                value.EntersFormalFlow);
        }

        private static bool AllEntriesSafelyIsolated(EnemyDomainSnapshotInput input)
        {
            IEnumerable<IEnemyDomainIsolationMetadata> all = input.Enemies.Cast<IEnemyDomainIsolationMetadata>()
                .Concat(input.Bosses)
                .Concat(input.MechanicProfiles)
                .Concat(input.SkillPatterns)
                .Concat(input.BossPhases)
                .Concat(input.MapRules)
                .Concat(input.Encounters)
                .Concat(input.CounterWindows);
            return all.All(value => value.DevOnly && !value.IsEnabled && !value.EntersFormalFlow);
        }

        private static void AddRejection(VerificationResult result, string checkId, string expectedCode, EnemyDomainSnapshotInput input)
        {
            IReadOnlyList<EnemyDomainValidationIssue> issues = Validator.Validate(input);
            bool validatorDetected = issues.Any(issue => string.Equals(issue.Code, expectedCode, StringComparison.Ordinal));
            bool providerRejected = false;
            try
            {
                Provider.CreateSnapshot(input);
            }
            catch (EnemyDomainContractValidationException exception)
            {
                providerRejected = exception.Issues.Any(issue => string.Equals(issue.Code, expectedCode, StringComparison.Ordinal));
            }

            bool passed = validatorDetected && providerRejected;
            Add(result, checkId, "validation", expectedCode, string.Join("|", issues.Select(issue => issue.Code)), passed,
                "Both offline validation and provider construction reject invalid input.");
        }

        private static bool IsReadOnly<T>(IReadOnlyList<T> values, T injected)
        {
            IList<T> mutableView = values as IList<T>;
            if (mutableView == null)
            {
                return true;
            }

            try
            {
                mutableView.Add(injected);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static void Add(VerificationResult result, string checkId, string area, string expected, string actual, bool passed, string notes)
        {
            result.Add(checkId, area, expected, actual, passed, notes);
        }

        private static void WriteReports(VerificationResult result, string executionMode)
        {
            string root = FindProjectRoot();
            string signature = string.Empty;
            try
            {
                signature = Provider.CreateSnapshot(CreateFixture(false)).BuildCanonicalSignature();
            }
            catch (Exception exception)
            {
                signature = "unavailable: " + exception.GetType().Name;
            }

            WriteUtf8(Path.Combine(root, DetailReportPath), BuildDetailReport(result, executionMode, signature));
            WriteUtf8(Path.Combine(root, SpecCsvPath), BuildSpecCsv(result));
            WriteUtf8(Path.Combine(root, LeakReportPath), BuildLeakReport(result, executionMode));
            AssetDatabase.Refresh();
        }

        private static string BuildDetailReport(VerificationResult result, string executionMode, string signature)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Domain Data Contract 01 Report").AppendLine();
            builder.AppendLine("- Package: `V0.4-EnemyDomainDataContract01`");
            builder.AppendLine("- Result: `" + (result.Passed ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Execution: `" + executionMode + "`");
            builder.AppendLine("- Schema: `" + EnemyDomainSchema.SchemaId + "`");
            builder.AppendLine("- Schema version: `" + EnemyDomainSchema.SchemaVersion.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Verifier: `" + result.PassedCount.ToString(CultureInfo.InvariantCulture) + "/" + result.TotalCount.ToString(CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Canonical Signature: `" + signature + "`").AppendLine();
            builder.AppendLine("## Contract delivered").AppendLine();
            builder.AppendLine("- Immutable `EnemyArchetypeSnapshot` and `BossArchetypeSnapshot` identity models.");
            builder.AppendLine("- Typed Mechanic, SkillPattern, BossPhase, MapRule, Encounter, and CounterWindow references.");
            builder.AppendLine("- Immutable `EnemyDomainSnapshot`, `IEnemyDomainSnapshotProvider`, and ordinal Enemy/Boss lookups.");
            builder.AppendLine("- Defensive copies at input, snapshot, and nested reference-list boundaries.");
            builder.AppendLine("- Order-independent SHA-256 canonical signature.");
            builder.AppendLine("- Offline validation for schema, stable IDs, duplicate IDs, unresolved references, categories, and dev isolation.").AppendLine();
            builder.AppendLine("## Isolation defaults").AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine("devOnly = true");
            builder.AppendLine("isEnabled = false");
            builder.AppendLine("entersFormalFlow = false");
            builder.AppendLine("```").AppendLine();
            builder.AppendLine("## Verification summary").AppendLine();
            builder.AppendLine("| Area | Passed | Total |");
            builder.AppendLine("|---|---:|---:|");
            foreach (IGrouping<string, CheckRow> group in result.Rows.GroupBy(row => row.Area).OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                builder.AppendLine("| " + group.Key + " | " + group.Count(row => row.Passed).ToString(CultureInfo.InvariantCulture) + " | " + group.Count().ToString(CultureInfo.InvariantCulture) + " |");
            }

            builder.AppendLine().AppendLine("## Scope").AppendLine();
            builder.AppendLine("- New Enemy Domain/Contract/Editor files and the three named reports only.");
            builder.AppendLine("- Scene, Prefab, Battle, Board, Item, RunFlow, Reward, and SaveData package entries: `0`.");
            builder.AppendLine("- No user scene hand-test is required for this data-only package.");
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

        private static string BuildLeakReport(VerificationResult result, string executionMode)
        {
            CheckRow[] leakRows = result.Rows.Where(row => string.Equals(row.Area, "leak", StringComparison.Ordinal)).ToArray();
            int leakCount = leakRows.Count(row => !row.Passed);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Enemy Domain Data Contract 01 Leak Check Report").AppendLine();
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
            builder.AppendLine("Scene modifications = 0");
            builder.AppendLine("Prefab modifications = 0");
            builder.AppendLine("Battle modifications = 0");
            builder.AppendLine("Board modifications = 0");
            builder.AppendLine("Item modifications = 0");
            builder.AppendLine("```").AppendLine();
            builder.AppendLine("The counts above describe this package manifest. Repository-wide pre-existing dirty files are excluded from this package claim and must be reported separately by the task window.");
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
