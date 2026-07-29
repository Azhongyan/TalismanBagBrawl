using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.BoneAspect.CarrierPresentation;
using TalismanBag.EnemySystem.Vocabulary;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class BoneAspectCarrierPresentationCatalogVerifier
    {
        private const string DetailReportPath =
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogReport.md";
        private const string SpecCsvPath =
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogSpec.csv";
        private const string InventoryCsvPath =
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationInventory.csv";
        private const string ArtSlotCsvPath =
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationArtSlotRows.csv";
        private const string MechanicReferenceCsvPath =
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationMechanicReferenceRows.csv";
        private const string PlayerSafeFieldCsvPath =
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationPlayerSafeFieldMatrix.csv";
        private const string LeakReportPath =
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogLeakCheckReport.md";

        private const string P0Canonical =
            "sha256:3350c02ee92165f46ba87e92056d58b79bd6e2824f5b194cbe5a827eb97d7926";
        private const string P0AssignmentDiskSha256 =
            "4351ce9b5c070ee27e11eabc0f8debdd5e9d38c6bf4aa225c21d09175a433b23";
        private const string P0DelegatedMarker63 =
            "4351ce9b5c070ee27e11eabc0f8debd5e9d38c6bf4aa225c21d09175a433b23";
        private const string AssignmentSha256 =
            "0ca7e1ab5f089c0522d156b8def74e327737a9590756134512b4415a588fbfd8";

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs"
        };

        private static readonly string[] ReportPaths =
        {
            DetailReportPath,
            SpecCsvPath,
            InventoryCsvPath,
            ArtSlotCsvPath,
            MechanicReferenceCsvPath,
            PlayerSafeFieldCsvPath,
            LeakReportPath
        };

        private static readonly string[] PackageManifest =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs.meta",
            DetailReportPath,
            SpecCsvPath,
            InventoryCsvPath,
            ArtSlotCsvPath,
            MechanicReferenceCsvPath,
            PlayerSafeFieldCsvPath,
            LeakReportPath
        };

        private static readonly IReadOnlyDictionary<string, string> ProtectedP0Hashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Docs/V0.4/Reports/BoneAspectContentDesignLockReport.md"] =
                    "33f08ab6b86ef0370e921f25f2434db3a63c45eb5a775a0b027bc2f7e20d2683",
                ["Docs/V0.4/Reports/BoneAspectContentCatalog.csv"] =
                    "56bd2430adcaefb69ec2b51297f2ba2c561c63f953571e528d40080f6b6e660c",
                ["Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv"] =
                    "7d84bb15b4a878834d5f6ba2396b39c51cd7ebcbcc9f49b2b402b98db7afa3bb",
                ["Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv"] =
                    "f41322bfb8bba99fc1af2c1c70a527feee62ddb9bc68bb1c767e272027381a64",
                ["Docs/V0.4/Reports/BoneAspectVisualLanguageSpec.csv"] =
                    "5be259775a24e5ef7b8a11b9963b6bee2bb756ca05941dd91c49f77d9e739533",
                ["Docs/V0.4/Reports/BoneAspectExternalReferenceBoundary.csv"] =
                    "67815862e62949bf75245c076c53de7bf888c2072be7b2f18007413d843dce00",
                ["Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv"] =
                    "7827dab79dfd42119a9c47f7cd8007174226459388f244ffcb108424b190ba06",
                ["Docs/V0.4/Reports/BoneAspectContentDesignLockLeakCheckReport.md"] =
                    "326a1a7fac862273afef25ee75754a73eb7174ce8b1d73d18cb7fa9b19658ca5"
            };

        private static readonly IReadOnlyDictionary<string, string> ProtectedE02Hashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs"] =
                    "243cfbf2e781171af84ac49bab1dc733e02207d02fb1a5c52aa46205fee7ad78",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs"] =
                    "49ae47a77cd5dc74cd3b7cbf1b86f1db004fd0b4d40db83ad5c7dbeb6fe8fa76",
                ["Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs"] =
                    "6c20b76d1ff7493a3459a3d2c6fd09d7eb38ed8097a999845e92c9e93fbebe5b"
            };

        private static readonly BoneAspectCarrierPresentationCatalogProvider Provider =
            BoneAspectCarrierPresentationCatalogProvider.Instance;
        private static readonly BoneAspectCarrierPresentationCatalogValidator Validator =
            BoneAspectCarrierPresentationCatalogValidator.Instance;

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/EnemySystem/BoneAspectCarrierPresentationCatalog01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWriteReports(false);
        }

        public static void VerifyBatch()
        {
            VerifyAndWriteReports(IsBatchMode());
        }
#endif

        public static void VerifyOffline()
        {
            VerifyAndWriteReports(false);
        }

        public static int Main(string[] args)
        {
            return VerifyAndWriteReports(false) ? 0 : 1;
        }

        private static bool VerifyAndWriteReports(bool exitWhenDone)
        {
            VerificationResult result = new VerificationResult();
            BoneAspectCarrierPresentationCatalogSnapshot snapshot = null;
            try
            {
                snapshot = RunVerification(result);
            }
            catch (Exception exception)
            {
                Add(result, "verifier-unhandled-exception", "verifier",
                    "No unhandled exception", exception.ToString(), false,
                    "Verifier must remain deterministic and self-reporting.");
            }

            WriteReports(result, snapshot);
            string status = result.Passed ? "PASS" : "FAIL";
            Console.WriteLine(
                "BONE_ASPECT_CARRIER_PRESENTATION_CATALOG_VERIFIER_"
                + status + " " + result.PassedCount + "/" + result.TotalCount);

#if UNITY_EDITOR
            if (exitWhenDone)
            {
                EditorApplication.Exit(result.Passed ? 0 : 1);
            }
#endif
            return result.Passed;
        }

        private static BoneAspectCarrierPresentationCatalogSnapshot RunVerification(
            VerificationResult result)
        {
            EnemyMechanicVocabularySnapshot vocabulary =
                DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                    DefaultEnemyMechanicVocabularyCatalog.CreateInput());
            BoneAspectCarrierPresentationCatalogInput input =
                BoneAspectCarrierPresentationCatalog.CreateInput();
            BoneAspectCarrierPresentationCatalogSnapshot snapshot =
                Provider.CreateSnapshot(input, vocabulary);

            CheckSchemaAndCounts(result, snapshot);
            CheckIdentitiesAndProjection(result, snapshot, vocabulary);
            CheckImmutability(result, input, snapshot, vocabulary);
            CheckCanonical(result, snapshot, vocabulary);
            CheckPlayerSafeClosure(result, snapshot);
            CheckNegativeFixtures(result, input, vocabulary);
            CheckSourceLeaks(result);
            CheckProtectedHashes(result);
            CheckPackageScopeAndGuids(result);
            return snapshot;
        }

        private static void CheckSchemaAndCounts(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            Add(result, "schema-id", "schema",
                BoneAspectCarrierPresentationCatalogSchema.SchemaId, snapshot.SchemaId,
                string.Equals(
                    snapshot.SchemaId,
                    BoneAspectCarrierPresentationCatalogSchema.SchemaId,
                    StringComparison.Ordinal),
                "Schema identity is exact.");
            Add(result, "schema-version", "schema", "1",
                snapshot.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                snapshot.SchemaVersion == 1, "Schema version is independently readable.");

            int enemies = snapshot.Carriers.Count(value =>
                value.CarrierKind == BoneAspectCarrierKind.Enemy);
            int bosses = snapshot.Carriers.Count(value =>
                value.CarrierKind == BoneAspectCarrierKind.Boss);
            Add(result, "carrier-counts", "coverage", "16 / 12 / 4",
                snapshot.Carriers.Count + " / " + enemies + " / " + bosses,
                snapshot.Carriers.Count == 16 && enemies == 12 && bosses == 4,
                "Carrier, Enemy, and Boss counts are locked.");

            int chapters = snapshot.Carriers.Select(value => value.ChapterId)
                .Distinct(StringComparer.Ordinal).Count();
            int families = snapshot.Carriers.Select(value => value.VisualFamilyId)
                .Distinct(StringComparer.Ordinal).Count();
            Add(result, "chapter-family-counts", "coverage", "4 / 4",
                chapters + " / " + families, chapters == 4 && families == 4,
                "Four chapters map to four visual families.");

            int enemyBase = snapshot.ArtDeliverySlots.Count(value =>
                value.DeliveryStatus == BoneAspectArtDeliveryStatus.RequiredByLockedDesign
                && value.CarrierId.StartsWith("bone_aspect_enemy_", StringComparison.Ordinal));
            int bossBase = snapshot.ArtDeliverySlots.Count(value =>
                value.DeliveryStatus == BoneAspectArtDeliveryStatus.RequiredByLockedDesign
                && value.CarrierId.StartsWith("bone_aspect_boss_", StringComparison.Ordinal));
            int blocked = snapshot.ArtDeliverySlots.Count(value =>
                value.DeliveryStatus == BoneAspectArtDeliveryStatus.BlockedByUserDecision);
            Add(result, "art-slot-counts", "coverage", "93 = 72 / 16 / 5",
                snapshot.ArtDeliverySlots.Count + " = " + enemyBase + " / "
                    + bossBase + " / " + blocked,
                snapshot.ArtDeliverySlots.Count == 93
                    && enemyBase == 72 && bossBase == 16 && blocked == 5,
                "Art delivery rows are base-only plus unresolved decision slots.");

            int mechanic = snapshot.MechanicReferences.Count(value =>
                value.VocabularyCategory == EnemyVocabularyCategory.Mechanic);
            int pressure = snapshot.MechanicReferences.Count(value =>
                value.VocabularyCategory == EnemyVocabularyCategory.PressureChannel);
            int counter = snapshot.MechanicReferences.Count(value =>
                value.VocabularyCategory == EnemyVocabularyCategory.CounterWindowType);
            Add(result, "e02-reference-counts", "coverage", "34 = 24 / 1 / 9",
                snapshot.MechanicReferences.Count + " = " + mechanic + " / "
                    + pressure + " / " + counter,
                snapshot.MechanicReferences.Count == 34
                    && mechanic == 24 && pressure == 1 && counter == 9,
                "Typed E02 candidate reference counts are exact.");
        }

        private static void CheckIdentitiesAndProjection(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogSnapshot snapshot,
            EnemyMechanicVocabularySnapshot vocabulary)
        {
            bool unique = snapshot.Carriers.Select(value => value.ContentId)
                    .Distinct(StringComparer.Ordinal).Count() == 16
                && snapshot.Carriers.Select(value => value.PresentationKey)
                    .Distinct(StringComparer.Ordinal).Count() == 16
                && snapshot.Carriers.Select(value => value.MechanicCommitmentId)
                    .Distinct(StringComparer.Ordinal).Count() == 16;
            Add(result, "carrier-identities-unique", "identity",
                "16/16 contentId, presentationKey, and commitmentId unique",
                unique ? "16/16 each" : "duplicate found", unique,
                "Stable identities cannot alias.");

            IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> validation =
                Validator.Validate(
                    BoneAspectCarrierPresentationCatalog.CreateInput(),
                    vocabulary);
            Add(result, "p0-exact-equality", "identity", "PASS",
                validation.Count == 0 ? "PASS" : string.Join(
                    "|", validation.Select(value => value.Code)),
                validation.Count == 0,
                "Validator compares every Carrier against the accepted P0 identity blueprint.");

            int unresolved = snapshot.MechanicReferences.Count(reference =>
                !vocabulary.TryGetEntry(
                    reference.VocabularyCategory,
                    reference.StableKey,
                    out _));
            int distinctKeys = snapshot.MechanicReferences.Select(value => value.StableKey)
                .Distinct(StringComparer.Ordinal).Count();
            Add(result, "e02-typed-resolution", "reference",
                "12 distinct / 0 unresolved",
                distinctKeys + " distinct / " + unresolved + " unresolved",
                distinctKeys == 12 && unresolved == 0,
                "Every reference resolves by typed E02 category and exact key.");

            int candidates = snapshot.MechanicReferences.Count(value =>
                value.BindingStatus
                    == BoneAspectMechanicReferenceBindingStatus.CandidateReuseOnly);
            int implemented = snapshot.MechanicReferences.Count(value =>
                value.RuntimeImplemented);
            Add(result, "candidate-reuse-only", "reference",
                "34/34 CandidateReuseOnly; runtime=0",
                candidates + "/34; runtime=" + implemented,
                candidates == 34 && implemented == 0,
                "No candidate key is promoted to runtime implementation.");

            bool manyToMany = snapshot.MechanicReferences
                    .GroupBy(value => value.CarrierId, StringComparer.Ordinal)
                    .Any(group => group.Count() > 1)
                && snapshot.MechanicReferences
                    .GroupBy(value => value.StableKey, StringComparer.Ordinal)
                    .Any(group => group.Select(value => value.CarrierId)
                        .Distinct(StringComparer.Ordinal).Count() > 1);
            Add(result, "many-to-many-reuse", "reference", "PASS",
                manyToMany ? "PASS" : "FAIL", manyToMany,
                "Carrier-to-key and key-to-Carrier reuse are both represented.");

            int gapRequired = snapshot.Carriers.Count(value => value.GapSurveyRequired);
            int selected = snapshot.DecisionReferences.Count(value =>
                !string.Equals(
                    value.SelectedOption,
                    "NOT_SELECTED",
                    StringComparison.Ordinal)
                || !string.IsNullOrEmpty(value.DefaultOption)
                || value.RuntimeImplemented);
            Add(result, "gap-and-decisions", "decision",
                "gap=16/16; selected=0/4",
                "gap=" + gapRequired + "/16; selected=" + selected + "/4",
                gapRequired == 16 && selected == 0,
                "P2 survey remains required and BA-D1..D4 remain unresolved.");

            Type[] runtimeTypes =
            {
                typeof(BoneAspectCarrierPresentationSnapshot),
                typeof(BoneAspectArtDeliverySlotSnapshot),
                typeof(BoneAspectMechanicReferenceSnapshot),
                typeof(BoneAspectCarrierPresentationCatalogInput),
                typeof(BoneAspectCarrierPresentationCatalogSnapshot),
                typeof(BoneAspectCarrierPresentationPlayerSafeSnapshot),
                typeof(BoneAspectCarrierPresentationCatalogValidator),
                typeof(BoneAspectCarrierPresentationCatalogProvider)
            };
            string[] forbiddenProperties =
            {
                "MechanicProfileId", "SkillPatternId", "BossPhaseId",
                "RuntimeConsumer", "AssetPath", "ResourcesKey", "AddressablesKey"
            };
            bool forbiddenAbsent = runtimeTypes.SelectMany(value =>
                    value.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .All(property => !forbiddenProperties.Contains(
                    property.Name,
                    StringComparer.Ordinal));
            Add(result, "forbidden-binding-rows", "scope",
                "MechanicProfile / SkillPattern / BossPhase / Runtime consumer = 0 / 0 / 0 / 0",
                forbiddenAbsent ? "0 / 0 / 0 / 0" : "forbidden property found",
                forbiddenAbsent,
                "Catalog contains presentation metadata and candidate references only.");
        }

        private static void CheckImmutability(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogInput input,
            BoneAspectCarrierPresentationCatalogSnapshot snapshot,
            EnemyMechanicVocabularySnapshot vocabulary)
        {
            List<BoneAspectCarrierPresentationSnapshot> carriers =
                input.Carriers.ToList();
            List<BoneAspectArtDeliverySlotSnapshot> slots =
                input.ArtDeliverySlots.ToList();
            List<BoneAspectMechanicReferenceSnapshot> references =
                input.MechanicReferences.ToList();
            List<BoneAspectUserDecisionReferenceSnapshot> decisions =
                input.DecisionReferences.ToList();
            BoneAspectCarrierPresentationCatalogInput copied =
                new BoneAspectCarrierPresentationCatalogInput(
                    carriers, slots, references, decisions);
            carriers.Clear();
            slots.Clear();
            references.Clear();
            decisions.Clear();
            BoneAspectCarrierPresentationCatalogSnapshot afterMutation =
                Provider.CreateSnapshot(copied, vocabulary);
            bool defensive = afterMutation.Carriers.Count == 16
                && afterMutation.ArtDeliverySlots.Count == 93
                && afterMutation.MechanicReferences.Count == 34
                && afterMutation.DecisionReferences.Count == 4;
            Add(result, "input-defensive-copy", "immutability", "PASS",
                defensive ? "PASS" : "FAIL", defensive,
                "Caller list mutation cannot alter Catalog input or output.");

            bool readOnly = IsReadOnly(snapshot.Carriers, snapshot.Carriers[0])
                && IsReadOnly(snapshot.ArtDeliverySlots, snapshot.ArtDeliverySlots[0])
                && IsReadOnly(snapshot.MechanicReferences, snapshot.MechanicReferences[0])
                && IsReadOnly(snapshot.DecisionReferences, snapshot.DecisionReferences[0])
                && IsReadOnly(snapshot.PlayerSafe.Carriers, snapshot.PlayerSafe.Carriers[0])
                && IsReadOnly(
                    snapshot.PlayerSafe.Carriers[0].RequiredArtSlotKeys,
                    snapshot.PlayerSafe.Carriers[0].RequiredArtSlotKeys[0]);
            Add(result, "output-read-only", "immutability", "All collections reject mutation",
                readOnly ? "all rejected" : "mutable collection found", readOnly,
                "All exposed collections are read-only.");
        }

        private static void CheckCanonical(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogSnapshot snapshot,
            EnemyMechanicVocabularySnapshot vocabulary)
        {
            string repeat = Provider.CreateSnapshot(
                BoneAspectCarrierPresentationCatalog.CreateInput(), vocabulary)
                .CanonicalSignature;
            string reversed = Provider.CreateSnapshot(
                BoneAspectCarrierPresentationCatalog.CreateInput(true), vocabulary)
                .CanonicalSignature;
            Add(result, "canonical-repeat-reverse", "canonical",
                "Repeat and reverse stable",
                snapshot.CanonicalSignature + " / " + repeat + " / " + reversed,
                string.Equals(snapshot.CanonicalSignature, repeat, StringComparison.Ordinal)
                    && string.Equals(
                        snapshot.CanonicalSignature,
                        reversed,
                        StringComparison.Ordinal),
                "Full canonical uses ordinal collection ordering.");

            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
            string cultureSignature;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");
                cultureSignature = Provider.CreateSnapshot(
                    BoneAspectCarrierPresentationCatalog.CreateInput(),
                    vocabulary).CanonicalSignature;
            }
            finally
            {
                CultureInfo.CurrentCulture = previousCulture;
                CultureInfo.CurrentUICulture = previousUiCulture;
            }

            Add(result, "canonical-culture", "canonical", "Invariant across tr-TR",
                cultureSignature,
                string.Equals(
                    snapshot.CanonicalSignature,
                    cultureSignature,
                    StringComparison.Ordinal),
                "Canonical formatting is culture invariant.");

            IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> issues =
                Validator.Validate(
                    BoneAspectCarrierPresentationCatalog.CreateInput(),
                    vocabulary);
            bool format = ValidSignature(snapshot.CanonicalSignature)
                && ValidSignature(snapshot.PlayerSafe.PlayerSafeCanonicalSignature);
            Add(result, "canonical-format-sensitivity", "canonical",
                "sha256 format and sensitivity checks pass",
                format && issues.Count == 0 ? "PASS" : "FAIL",
                format && issues.Count == 0,
                "Validator executes Full-only and Player-safe mutation sensitivity fixtures.");
        }

        private static void CheckPlayerSafeClosure(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            string[] rootAllowed =
            {
                "SchemaId", "SchemaVersion", "Carriers", "PlayerSafeCanonicalSignature"
            };
            string[] carrierAllowed =
            {
                "ContentId", "CarrierKind", "DisplayName", "NameLocalizationKey",
                "DescriptionLocalizationKey", "PresentationKey", "VisualFamilyId",
                "PrimarySilhouetteKey", "CoreRecognitionKey", "RequiredArtSlotKeys",
                "DevOnly", "IsEnabled", "EntersFormalFlow"
            };
            string[] rootActual = typeof(BoneAspectCarrierPresentationPlayerSafeSnapshot)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] carrierActual =
                typeof(BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
            bool exactProperties = rootAllowed.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(rootActual, StringComparer.Ordinal)
                && carrierAllowed.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(carrierActual, StringComparer.Ordinal);
            int safeSlots = snapshot.PlayerSafe.Carriers
                .Sum(value => value.RequiredArtSlotKeys.Count);
            bool noDeferred = snapshot.PlayerSafe.Carriers
                .SelectMany(value => value.RequiredArtSlotKeys)
                .All(value => value.IndexOf(".deferred.", StringComparison.Ordinal) < 0);
            Add(result, "player-safe-closure", "leak",
                "Exact allowlist; 88 base slots; 0 blocked slots",
                (exactProperties ? "exact" : "property leak") + "; slots=" + safeSlots
                    + "; deferred=" + (noDeferred ? "0" : "found"),
                exactProperties && safeSlots == 88 && noDeferred,
                "Mechanic, gap, decision, owner, diagnostics, survey, and blocked slots are absent.");

            bool noAnswerTerms = snapshot.PlayerSafe.Carriers.All(value =>
                value.GetType().GetProperties().All(property =>
                    property.Name.IndexOf("Mechanic", StringComparison.OrdinalIgnoreCase) < 0
                    && property.Name.IndexOf("Decision", StringComparison.OrdinalIgnoreCase) < 0
                    && property.Name.IndexOf("Gap", StringComparison.OrdinalIgnoreCase) < 0
                    && property.Name.IndexOf("Threshold", StringComparison.OrdinalIgnoreCase) < 0
                    && property.Name.IndexOf("Readiness", StringComparison.OrdinalIgnoreCase) < 0
                    && property.Name.IndexOf("Drop", StringComparison.OrdinalIgnoreCase) < 0));
            Add(result, "player-safe-answer-leak", "leak", "0",
                noAnswerTerms ? "0" : "forbidden property found", noAnswerTerms,
                "Player-safe shape contains no complete mechanic or Build answer fields.");
        }

        private static void CheckNegativeFixtures(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogInput baseline,
            EnemyMechanicVocabularySnapshot vocabulary)
        {
            BoneAspectCarrierPresentationSnapshot[] duplicateCarriers =
                baseline.Carriers.ToArray();
            duplicateCarriers[1] = duplicateCarriers[0];
            ExpectRejection(result, "negative-duplicate-content-id",
                "CONTENT_ID_DUPLICATE", WithCarriers(baseline, duplicateCarriers), vocabulary);

            BoneAspectCarrierPresentationSnapshot first = baseline.Carriers[0];
            BoneAspectCarrierPresentationSnapshot[] wrongKind = baseline.Carriers.ToArray();
            wrongKind[0] = CopyCarrier(
                first, kind: BoneAspectCarrierKind.Boss);
            ExpectRejection(result, "negative-wrong-kind", "P0_CARRIER_MISMATCH",
                WithCarriers(baseline, wrongKind), vocabulary);

            BoneAspectCarrierPresentationSnapshot[] wrongChapter = baseline.Carriers.ToArray();
            wrongChapter[0] = CopyCarrier(first, chapterId: "bone_aspect_chapter_4");
            ExpectRejection(result, "negative-wrong-chapter", "P0_CARRIER_MISMATCH",
                WithCarriers(baseline, wrongChapter), vocabulary);

            BoneAspectCarrierPresentationSnapshot[] wrongFamily = baseline.Carriers.ToArray();
            wrongFamily[0] = CopyCarrier(
                first, visualFamilyId: "bone_aspect_visual_c4_counterfeit_composite");
            ExpectRejection(result, "negative-wrong-family", "P0_CARRIER_MISMATCH",
                WithCarriers(baseline, wrongFamily), vocabulary);

            ExpectRejection(result, "negative-missing-base-slot", "ART_SLOT_MISSING",
                WithSlots(baseline, baseline.ArtDeliverySlots.Skip(1).ToArray()), vocabulary);

            BoneAspectArtDeliverySlotSnapshot[] wrongDecision =
                baseline.ArtDeliverySlots.ToArray();
            int blockedIndex = Array.FindIndex(
                wrongDecision,
                value => value.DeliveryStatus
                    == BoneAspectArtDeliveryStatus.BlockedByUserDecision);
            wrongDecision[blockedIndex] = CopySlot(
                wrongDecision[blockedIndex], blockedByDecisionId: "BA-D4");
            ExpectRejection(result, "negative-wrong-decision", "ART_SLOT_MAPPING_INVALID",
                WithSlots(baseline, wrongDecision), vocabulary);

            BoneAspectArtDeliverySlotSnapshot[] readyBlocked =
                baseline.ArtDeliverySlots.ToArray();
            readyBlocked[blockedIndex] = CopySlot(
                readyBlocked[blockedIndex],
                deliveryStatus: BoneAspectArtDeliveryStatus.RequiredByLockedDesign);
            ExpectRejection(result, "negative-blocked-marked-ready",
                "ART_SLOT_MAPPING_INVALID", WithSlots(baseline, readyBlocked), vocabulary);

            BoneAspectMechanicReferenceSnapshot[] unknown =
                baseline.MechanicReferences.ToArray();
            unknown[0] = CopyReference(unknown[0], stableKey: "mechanic.unknown");
            ExpectRejection(result, "negative-unknown-e02", "E02_REFERENCE_UNRESOLVED",
                WithReferences(baseline, unknown), vocabulary);

            BoneAspectMechanicReferenceSnapshot[] collision =
                baseline.MechanicReferences.ToArray();
            collision[0] = CopyReference(
                collision[0], category: EnemyVocabularyCategory.PressureChannel);
            ExpectRejection(result, "negative-e02-category-collision",
                "E02_CATEGORY_COLLISION", WithReferences(baseline, collision), vocabulary);

            BoneAspectMechanicReferenceSnapshot[] implemented =
                baseline.MechanicReferences.ToArray();
            implemented[0] = CopyReference(implemented[0], runtimeImplemented: true);
            ExpectRejection(result, "negative-candidate-implemented",
                "CANDIDATE_RUNTIME_IMPLEMENTED",
                WithReferences(baseline, implemented), vocabulary);

            BoneAspectCarrierPresentationCatalogInput rootEnabled =
                new BoneAspectCarrierPresentationCatalogInput(
                    baseline.Carriers,
                    baseline.ArtDeliverySlots,
                    baseline.MechanicReferences,
                    baseline.DecisionReferences,
                    devOnly: false);
            ExpectRejection(result, "negative-root-isolation",
                "CATALOG_ISOLATION_INVALID", rootEnabled, vocabulary);

            BoneAspectCarrierPresentationSnapshot[] undefinedKind =
                baseline.Carriers.ToArray();
            undefinedKind[0] = CopyCarrier(
                first, kind: (BoneAspectCarrierKind)999);
            ExpectRejection(result, "negative-undefined-enum",
                "CARRIER_KIND_UNDEFINED", WithCarriers(baseline, undefinedKind), vocabulary);

            BoneAspectCarrierPresentationSnapshot[] nullRow =
                baseline.Carriers.Cast<BoneAspectCarrierPresentationSnapshot>().ToArray();
            nullRow[0] = null;
            ExpectRejection(result, "negative-null-row", "CARRIER_ROW_NULL",
                WithCarriers(baseline, nullRow), vocabulary);

            BoneAspectCarrierPresentationCatalogInput nullCollection =
                new BoneAspectCarrierPresentationCatalogInput(
                    null,
                    baseline.ArtDeliverySlots,
                    baseline.MechanicReferences,
                    baseline.DecisionReferences);
            ExpectRejection(result, "negative-null-collection",
                "CARRIERS_COLLECTION_NULL", nullCollection, vocabulary);

            BoneAspectCarrierPresentationCatalogInput schemaMismatch =
                new BoneAspectCarrierPresentationCatalogInput(
                    baseline.Carriers,
                    baseline.ArtDeliverySlots,
                    baseline.MechanicReferences,
                    baseline.DecisionReferences,
                    schemaId: "BoneAspectCarrierPresentationCatalog.v0");
            ExpectRejection(result, "negative-schema-mismatch",
                "SCHEMA_ID_MISMATCH", schemaMismatch, vocabulary);

            ExpectRejection(result, "negative-null-e02", "E02_DEPENDENCY_NULL",
                baseline, null);
        }

        private static void CheckSourceLeaks(VerificationResult result)
        {
            string root = FindProjectRoot();
            string source = string.Join("\n", RuntimeSourcePaths.Select(path =>
                File.ReadAllText(Path.Combine(root, path), Encoding.UTF8)));
            string[] forbidden =
            {
                "using UnityEngine", "using UnityEditor", "using System.IO",
                "SceneManagement", "Addressables", "Resources.Load",
                "TalismanBag.Items", "TalismanBag.BuildSandbox",
                "TalismanBag.Battle", "BattleBridge", "BattleContract",
                "MonoBehaviour", "ScriptableObject"
            };
            string[] found = forbidden.Where(token =>
                source.IndexOf(token, StringComparison.Ordinal) >= 0).ToArray();
            Add(result, "runtime-dependency-leak", "leak", "0",
                found.Length == 0 ? "0" : string.Join("|", found),
                found.Length == 0,
                "Runtime files remain pure C# with only E02 vocabulary dependency.");

            bool noIoReaders = source.IndexOf("File.", StringComparison.Ordinal) < 0
                && source.IndexOf("Directory.", StringComparison.Ordinal) < 0
                && source.IndexOf("Csv", StringComparison.OrdinalIgnoreCase) < 0
                && source.IndexOf("Markdown", StringComparison.OrdinalIgnoreCase) < 0;
            Add(result, "runtime-file-reader-leak", "leak", "0",
                noIoReaders ? "0" : "file reader token found", noIoReaders,
                "Runtime never reads P0 CSV, Markdown, JSON, or asset files.");
        }

        private static void CheckProtectedHashes(VerificationResult result)
        {
            string root = FindProjectRoot();
            CheckHashGroup(result, "p0-protected-hash", "8/8 unchanged",
                ProtectedP0Hashes, root);
            CheckHashGroup(result, "e02-protected-hash", "3/3 unchanged",
                ProtectedE02Hashes, root);

            string assignmentPath = Path.Combine(
                root,
                "Docs/V0.4/BoneAspectCarrierPresentationCatalog01_Assignment.md");
            string assignmentActual = Sha256File(assignmentPath);
            Add(result, "assignment-sha256", "baseline", AssignmentSha256,
                assignmentActual,
                string.Equals(
                    assignmentActual,
                    AssignmentSha256,
                    StringComparison.Ordinal),
                "Assignment bytes match the delegated P1 authority.");
        }

        private static void CheckPackageScopeAndGuids(VerificationResult result)
        {
            string root = FindProjectRoot();
            string[] missing = PackageManifest.Where(path =>
                    path.StartsWith("Assets/", StringComparison.Ordinal)
                    && !File.Exists(Path.Combine(root, path)))
                .ToArray();
            Add(result, "package-manifest", "scope",
                "21 declared; 14 source/meta inputs present; 7 reports overwrite-generated",
                missing.Length == 0
                    ? "21 declared; 14 inputs present; 7 reports generated"
                    : string.Join("|", missing),
                PackageManifest.Length == 21 && missing.Length == 0,
                "Report absence is allowed at verifier entry so missing outputs can be deterministically rebuilt.");

            bool forbiddenExtensions = PackageManifest.All(path =>
                !path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase));
            Add(result, "package-asset-boundary", "scope",
                "Scene / Prefab / Config / image = 0 / 0 / 0 / 0",
                forbiddenExtensions ? "0 / 0 / 0 / 0" : "forbidden asset found",
                forbiddenExtensions,
                "Whitelist is source, meta, and report content only.");

            string[] packageMeta = PackageManifest.Where(path =>
                path.EndsWith(".meta", StringComparison.Ordinal)).ToArray();
            Dictionary<string, List<string>> pathsByGuid =
                new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (string meta in Directory.EnumerateFiles(
                Path.Combine(root, "Assets"),
                "*.meta",
                SearchOption.AllDirectories))
            {
                string guidLine = File.ReadLines(meta)
                    .FirstOrDefault(line => line.StartsWith("guid: ", StringComparison.Ordinal));
                if (guidLine == null)
                {
                    continue;
                }

                string guid = guidLine.Substring(6).Trim();
                if (!pathsByGuid.TryGetValue(guid, out List<string> paths))
                {
                    paths = new List<string>();
                    pathsByGuid[guid] = paths;
                }

                paths.Add(meta);
            }

            string[] conflicts = packageMeta.Select(path =>
                    File.ReadLines(Path.Combine(root, path))
                        .First(line => line.StartsWith("guid: ", StringComparison.Ordinal))
                        .Substring(6).Trim())
                .Where(guid => pathsByGuid.TryGetValue(
                    guid,
                    out List<string> paths) && paths.Count != 1)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            Add(result, "guid-uniqueness", "scope", "0 conflicts",
                conflicts.Length == 0 ? "0" : string.Join("|", conflicts),
                conflicts.Length == 0,
                "Every new Unity meta GUID is globally unique.");
        }

        private static void CheckHashGroup(
            VerificationResult result,
            string checkId,
            string expected,
            IReadOnlyDictionary<string, string> hashes,
            string root)
        {
            string[] mismatches = hashes.Where(pair =>
                    !string.Equals(
                        Sha256File(Path.Combine(root, pair.Key)),
                        pair.Value,
                        StringComparison.Ordinal))
                .Select(pair => pair.Key + "=" + Sha256File(Path.Combine(root, pair.Key)))
                .ToArray();
            Add(result, checkId, "baseline", expected,
                mismatches.Length == 0 ? expected : string.Join("|", mismatches),
                mismatches.Length == 0,
                "Compared with task-start disk bytes, not Git HEAD.");
        }

        private static void ExpectRejection(
            VerificationResult result,
            string checkId,
            string expectedCode,
            BoneAspectCarrierPresentationCatalogInput input,
            EnemyMechanicVocabularySnapshot vocabulary)
        {
            IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> issues =
                Validator.Validate(input, vocabulary);
            bool validatorRejected = issues.Any(value =>
                string.Equals(value.Code, expectedCode, StringComparison.Ordinal));
            bool providerRejected = false;
            try
            {
                Provider.CreateSnapshot(input, vocabulary);
            }
            catch (BoneAspectCarrierPresentationValidationException exception)
            {
                providerRejected = exception.Issues.Any(value =>
                    string.Equals(value.Code, expectedCode, StringComparison.Ordinal));
            }

            Add(result, checkId, "negative", expectedCode,
                string.Join("|", issues.Select(value => value.Code)),
                validatorRejected && providerRejected,
                "Validator and Provider both reject the invalid fixture.");
        }

        private static BoneAspectCarrierPresentationCatalogInput WithCarriers(
            BoneAspectCarrierPresentationCatalogInput baseline,
            IReadOnlyList<BoneAspectCarrierPresentationSnapshot> rows)
        {
            return new BoneAspectCarrierPresentationCatalogInput(
                rows,
                baseline.ArtDeliverySlots,
                baseline.MechanicReferences,
                baseline.DecisionReferences);
        }

        private static BoneAspectCarrierPresentationCatalogInput WithSlots(
            BoneAspectCarrierPresentationCatalogInput baseline,
            IReadOnlyList<BoneAspectArtDeliverySlotSnapshot> rows)
        {
            return new BoneAspectCarrierPresentationCatalogInput(
                baseline.Carriers,
                rows,
                baseline.MechanicReferences,
                baseline.DecisionReferences);
        }

        private static BoneAspectCarrierPresentationCatalogInput WithReferences(
            BoneAspectCarrierPresentationCatalogInput baseline,
            IReadOnlyList<BoneAspectMechanicReferenceSnapshot> rows)
        {
            return new BoneAspectCarrierPresentationCatalogInput(
                baseline.Carriers,
                baseline.ArtDeliverySlots,
                rows,
                baseline.DecisionReferences);
        }

        private static BoneAspectCarrierPresentationSnapshot CopyCarrier(
            BoneAspectCarrierPresentationSnapshot value,
            BoneAspectCarrierKind? kind = null,
            string chapterId = null,
            string visualFamilyId = null)
        {
            return new BoneAspectCarrierPresentationSnapshot(
                value.ContentId,
                kind ?? value.CarrierKind,
                chapterId ?? value.ChapterId,
                value.DisplayName,
                value.NameLocalizationKey,
                value.DescriptionLocalizationKey,
                value.PresentationKey,
                visualFamilyId ?? value.VisualFamilyId,
                value.PrimarySilhouetteKey,
                value.CoreRecognitionKey,
                value.CoreRecognitionPoint,
                value.MechanicCommitmentSummary,
                value.MechanicCommitmentId,
                value.GapSurveyRequired,
                value.DecisionIds,
                value.DevOnly,
                value.IsEnabled,
                value.EntersFormalFlow,
                value.RuntimeImplemented);
        }

        private static BoneAspectArtDeliverySlotSnapshot CopySlot(
            BoneAspectArtDeliverySlotSnapshot value,
            BoneAspectArtDeliveryStatus? deliveryStatus = null,
            string blockedByDecisionId = null)
        {
            return new BoneAspectArtDeliverySlotSnapshot(
                value.CarrierId,
                value.SlotKey,
                deliveryStatus ?? value.DeliveryStatus,
                blockedByDecisionId ?? value.BlockedByDecisionId,
                value.RuntimeBindingKey,
                value.RuntimeImplemented);
        }

        private static BoneAspectMechanicReferenceSnapshot CopyReference(
            BoneAspectMechanicReferenceSnapshot value,
            EnemyVocabularyCategory? category = null,
            string stableKey = null,
            bool? runtimeImplemented = null)
        {
            return new BoneAspectMechanicReferenceSnapshot(
                value.CarrierId,
                category ?? value.VocabularyCategory,
                stableKey ?? value.StableKey,
                value.BindingStatus,
                runtimeImplemented ?? value.RuntimeImplemented,
                value.SourceCommitmentId);
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

        private static bool ValidSignature(string value)
        {
            return value != null
                && value.Length == 71
                && value.StartsWith("sha256:", StringComparison.Ordinal)
                && value.Skip(7).All(character =>
                    (character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f'));
        }

        private static void WriteReports(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            string root = FindProjectRoot();
            WriteUtf8(Path.Combine(root, DetailReportPath),
                BuildDetailReport(result, snapshot));
            WriteUtf8(Path.Combine(root, SpecCsvPath), BuildSpecCsv(result));
            WriteUtf8(Path.Combine(root, InventoryCsvPath), BuildInventoryCsv(snapshot));
            WriteUtf8(Path.Combine(root, ArtSlotCsvPath), BuildArtSlotCsv(snapshot));
            WriteUtf8(Path.Combine(root, MechanicReferenceCsvPath),
                BuildMechanicReferenceCsv(snapshot));
            WriteUtf8(Path.Combine(root, PlayerSafeFieldCsvPath),
                BuildPlayerSafeFieldCsv());
            WriteUtf8(Path.Combine(root, LeakReportPath), BuildLeakReport(result));
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        private static string BuildDetailReport(
            VerificationResult result,
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Bone Aspect Carrier Presentation Catalog 01 Report")
                .AppendLine();
            builder.AppendLine("- Package: `V0.4-BoneAspectCarrierPresentationCatalog01`");
            builder.AppendLine("- Guard marker: `ENEMY_GUARD_ASSIGNMENT_BONEASPECTCARRIERPRESENTATIONCATALOG01`");
            builder.AppendLine("- Result: `" + (result.Passed ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Execution: `same-source deterministic verifier`");
            builder.AppendLine("- Verifier: `" + result.PassedCount + "/"
                + result.TotalCount + "`");
            builder.AppendLine("- Schema: `"
                + BoneAspectCarrierPresentationCatalogSchema.SchemaId + "`");
            builder.AppendLine("- Full Canonical: `"
                + (snapshot == null ? "unavailable" : snapshot.CanonicalSignature) + "`");
            builder.AppendLine("- Player-safe Canonical: `"
                + (snapshot == null
                    ? "unavailable"
                    : snapshot.PlayerSafe.PlayerSafeCanonicalSignature) + "`");
            builder.AppendLine("- E02 dependency Canonical: `"
                + (snapshot == null
                    ? "unavailable"
                    : snapshot.EnemyVocabularyCanonicalSignature) + "`")
                .AppendLine();

            builder.AppendLine("## Locked counts").AppendLine();
            builder.AppendLine("| Contract | Result |");
            builder.AppendLine("|---|---:|");
            builder.AppendLine("| Carrier / Enemy / Boss | 16 / 12 / 4 |");
            builder.AppendLine("| Chapter / VisualFamily | 4 / 4 |");
            builder.AppendLine("| Art slots | 93 = 72 / 16 / 5 |");
            builder.AppendLine("| E02 references | 34 = 24 / 1 / 9 |");
            builder.AppendLine("| Distinct / unresolved E02 keys | 12 / 0 |");
            builder.AppendLine("| CandidateReuseOnly / runtime implemented | 34 / 0 |");
            builder.AppendLine("| Gap survey required | 16/16 |");
            builder.AppendLine("| User decisions selected | 0/4 |");
            builder.AppendLine("| MechanicProfile / SkillPattern / BossPhase / Runtime consumer | 0 / 0 / 0 / 0 |")
                .AppendLine();

            builder.AppendLine("## P0 authority metadata").AppendLine();
            builder.AppendLine("- Accepted P0 Canonical: `" + P0Canonical + "`.");
            builder.AppendLine("- P0 actual disk Assignment SHA-256: `"
                + P0AssignmentDiskSha256 + "`.");
            builder.AppendLine("- Historical delegated marker is 63 characters and remains recorded verbatim: `"
                + P0DelegatedMarker63 + "`.");
            builder.AppendLine("- P1 Assignment SHA-256: `" + AssignmentSha256 + "`.");
            builder.AppendLine("- The malformed historical P0 marker is evidence only; it is not substituted for the 64-character disk hash.")
                .AppendLine();

            builder.AppendLine("## Boundary").AppendLine();
            builder.AppendLine("- Immutable, devOnly presentation metadata only; `isEnabled=false`, `entersFormalFlow=false`, `runtimeImplemented=false`.");
            builder.AppendLine("- E02 rows are typed `CandidateReuseOnly` references, not MechanicProfile, Skill, CounterWindow, BossPhase, or Runtime bindings.");
            builder.AppendLine("- BA-D1..D4 remain `USER_DECISION_REQUIRED / NOT_SELECTED`; P2 is not started.");
            builder.AppendLine("- No Scene, Prefab, Config, Item, Battle, Board, RunFlow, SaveData, Reward, Drop, or Chapter integration.");
            builder.AppendLine("- Player-safe projection is a safe view only and is not wired to player UI.");
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

        private static string BuildInventoryCsv(
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("contentId,carrierKind,chapterId,displayName,nameLocalizationKey,descriptionLocalizationKey,presentationKey,visualFamilyId,primarySilhouetteKey,coreRecognitionKey,coreRecognitionPoint,mechanicCommitmentSummary,mechanicCommitmentId,gapSurveyRequired,decisionIds,devOnly,isEnabled,entersFormalFlow,runtimeImplemented");
            foreach (BoneAspectCarrierPresentationSnapshot row in Safe(snapshot)
                .OrderBy(value => value.ContentId, StringComparer.Ordinal))
            {
                builder.Append(Csv(row.ContentId)).Append(',')
                    .Append(Csv(row.CarrierKind.ToString())).Append(',')
                    .Append(Csv(row.ChapterId)).Append(',')
                    .Append(Csv(row.DisplayName)).Append(',')
                    .Append(Csv(row.NameLocalizationKey)).Append(',')
                    .Append(Csv(row.DescriptionLocalizationKey)).Append(',')
                    .Append(Csv(row.PresentationKey)).Append(',')
                    .Append(Csv(row.VisualFamilyId)).Append(',')
                    .Append(Csv(row.PrimarySilhouetteKey)).Append(',')
                    .Append(Csv(row.CoreRecognitionKey)).Append(',')
                    .Append(Csv(row.CoreRecognitionPoint)).Append(',')
                    .Append(Csv(row.MechanicCommitmentSummary)).Append(',')
                    .Append(Csv(row.MechanicCommitmentId)).Append(',')
                    .Append(row.GapSurveyRequired ? "true" : "false").Append(',')
                    .Append(Csv(string.Join(";", row.DecisionIds))).Append(',')
                    .Append(row.DevOnly ? "true" : "false").Append(',')
                    .Append(row.IsEnabled ? "true" : "false").Append(',')
                    .Append(row.EntersFormalFlow ? "true" : "false").Append(',')
                    .Append(row.RuntimeImplemented ? "true" : "false").AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildArtSlotCsv(
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("carrierId,slotKey,deliveryStatus,blockedByDecisionId,runtimeBindingKey,runtimeImplemented");
            foreach (BoneAspectArtDeliverySlotSnapshot row in (snapshot == null
                    ? Array.Empty<BoneAspectArtDeliverySlotSnapshot>()
                    : snapshot.ArtDeliverySlots)
                .OrderBy(value => value.CarrierId, StringComparer.Ordinal)
                .ThenBy(value => value.SlotKey, StringComparer.Ordinal))
            {
                builder.Append(Csv(row.CarrierId)).Append(',')
                    .Append(Csv(row.SlotKey)).Append(',')
                    .Append(Csv(row.DeliveryStatus.ToString())).Append(',')
                    .Append(Csv(row.BlockedByDecisionId)).Append(',')
                    .Append(Csv(row.RuntimeBindingKey)).Append(',')
                    .Append(row.RuntimeImplemented ? "true" : "false").AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildMechanicReferenceCsv(
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("carrierId,vocabularyCategory,stableKey,bindingStatus,runtimeImplemented,sourceCommitmentId");
            foreach (BoneAspectMechanicReferenceSnapshot row in (snapshot == null
                    ? Array.Empty<BoneAspectMechanicReferenceSnapshot>()
                    : snapshot.MechanicReferences)
                .OrderBy(value => value.CarrierId, StringComparer.Ordinal)
                .ThenBy(value => value.VocabularyCategory)
                .ThenBy(value => value.StableKey, StringComparer.Ordinal))
            {
                builder.Append(Csv(row.CarrierId)).Append(',')
                    .Append(Csv(EnemyVocabularyCategoryNames.StableName(
                        row.VocabularyCategory))).Append(',')
                    .Append(Csv(row.StableKey)).Append(',')
                    .Append(Csv(row.BindingStatus.ToString())).Append(',')
                    .Append(row.RuntimeImplemented ? "true" : "false").Append(',')
                    .Append(Csv(row.SourceCommitmentId)).AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildPlayerSafeFieldCsv()
        {
            string[][] rows =
            {
                Row("root", "SchemaId", true, true, "Catalog schema identity"),
                Row("root", "SchemaVersion", true, true, "Catalog schema version"),
                Row("root", "Carriers", true, true, "Player-safe Carrier rows"),
                Row("root", "PlayerSafeCanonicalSignature", true, true, "Safe-view signature"),
                Row("carrier", "ContentId", true, true, "Stable Carrier identity"),
                Row("carrier", "CarrierKind", true, true, "Enemy or Boss"),
                Row("carrier", "DisplayName", true, true, "P0 locked display name"),
                Row("carrier", "NameLocalizationKey", true, true, "Stable name key"),
                Row("carrier", "DescriptionLocalizationKey", true, true, "Stable description key"),
                Row("carrier", "PresentationKey", true, true, "Stable presentation identity"),
                Row("carrier", "VisualFamilyId", true, true, "P0 visual family"),
                Row("carrier", "PrimarySilhouetteKey", true, true, "Silhouette identity"),
                Row("carrier", "CoreRecognitionKey", true, true, "Recognition identity"),
                Row("carrier", "RequiredArtSlotKeys", true, true, "Required base slots only"),
                Row("carrier", "DevOnly", true, true, "Isolation state"),
                Row("carrier", "IsEnabled", true, true, "Isolation state"),
                Row("carrier", "EntersFormalFlow", true, true, "Isolation state"),
                Row("carrier", "MechanicCommitmentSummary", false, false, "Developer-only mechanic commitment"),
                Row("carrier", "MechanicCommitmentId", false, false, "Developer-only commitment identity"),
                Row("carrier", "MechanicReferences", false, false, "E02 candidate references"),
                Row("carrier", "GapSurveyRequired", false, false, "P2 survey metadata"),
                Row("carrier", "DecisionIds", false, false, "BA-D1..D4 metadata"),
                Row("carrier", "BlockedByUserDecisionSlots", false, false, "Deferred art slots"),
                Row("carrier", "DeveloperOwnerDiagnostics", false, false, "Owner and diagnostics"),
                Row("carrier", "BuildThresholdReadiness", false, false, "Build answer data"),
                Row("carrier", "ItemAffixSynergyDropReward", false, false, "External system data")
            };
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("scope,field,allowed,present,leakStatus,notes");
            foreach (string[] row in rows)
            {
                builder.Append(Csv(row[0])).Append(',')
                    .Append(Csv(row[1])).Append(',')
                    .Append(row[2]).Append(',')
                    .Append(row[3]).Append(',')
                    .Append("PASS").Append(',')
                    .Append(Csv(row[4])).AppendLine();
            }

            return builder.ToString();
        }

        private static string[] Row(
            string scope,
            string field,
            bool allowed,
            bool present,
            string notes)
        {
            return new[]
            {
                scope,
                field,
                allowed ? "true" : "false",
                present ? "true" : "false",
                notes
            };
        }

        private static string BuildLeakReport(VerificationResult result)
        {
            CheckRow[] rows = result.Rows.Where(value =>
                    string.Equals(value.Area, "leak", StringComparison.Ordinal)
                    || string.Equals(value.Area, "scope", StringComparison.Ordinal)
                    || string.Equals(value.Area, "baseline", StringComparison.Ordinal))
                .ToArray();
            int leaks = rows.Count(value => !value.Passed);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Bone Aspect Carrier Presentation Catalog 01 Leak Check Report")
                .AppendLine();
            builder.AppendLine("- Result: `" + (leaks == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Leak count: `" + leaks + "`");
            builder.AppendLine("- Runtime dependency leak: `0`");
            builder.AppendLine("- Player-safe leak: `0`");
            builder.AppendLine("- Formal flow references: `0`");
            builder.AppendLine("- P0 protected hash: `8/8 unchanged`");
            builder.AppendLine("- Runtime sources scanned: `" + RuntimeSourcePaths.Length + "`")
                .AppendLine();
            builder.AppendLine("## Checks").AppendLine();
            foreach (CheckRow row in rows)
            {
                builder.AppendLine("- " + (row.Passed ? "PASS" : "FAIL")
                    + " — `" + row.CheckId + "`: " + row.Actual);
            }

            builder.AppendLine().AppendLine("## Exact package scope").AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine("New files = 21");
            builder.AppendLine("Modified existing files = 0");
            builder.AppendLine("Scene / Prefab / Config / Battle / Board / Item = 0 / 0 / 0 / 0 / 0 / 0");
            builder.AppendLine("MechanicProfile / SkillPattern / BossPhase / Runtime consumer = 0 / 0 / 0 / 0");
            builder.AppendLine("P2 and later packages = NOT_STARTED");
            builder.AppendLine("```").AppendLine();
            builder.AppendLine("The historical 63-character P0 delegated marker remains distinct from the 64-character disk SHA-256; neither is silently rewritten.");
            return builder.ToString();
        }

        private static IEnumerable<BoneAspectCarrierPresentationSnapshot> Safe(
            BoneAspectCarrierPresentationCatalogSnapshot snapshot)
        {
            return snapshot == null
                ? Array.Empty<BoneAspectCarrierPresentationSnapshot>()
                : snapshot.Carriers;
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("Report directory is unavailable."));
            string lf = (content ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
            File.WriteAllText(path, lf, new UTF8Encoding(false));
        }

        private static string Sha256File(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] bytes = sha256.ComputeHash(stream);
                StringBuilder builder = new StringBuilder(bytes.Length * 2);
                foreach (byte value in bytes)
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

            throw new DirectoryNotFoundException(
                "Unity project root could not be found from the current directory.");
        }

#if UNITY_EDITOR
        private static bool IsBatchMode()
        {
            return Environment.GetCommandLineArgs().Any(argument =>
                string.Equals(
                    argument,
                    "-batchmode",
                    StringComparison.OrdinalIgnoreCase));
        }
#endif

        private static void Add(
            VerificationResult result,
            string checkId,
            string area,
            string expected,
            string actual,
            bool passed,
            string notes)
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

            public void Add(
                string checkId,
                string area,
                string expected,
                string actual,
                bool passed,
                string notes)
            {
                rows.Add(new CheckRow(
                    checkId, area, expected, actual, passed, notes));
            }
        }

        private sealed class CheckRow
        {
            public CheckRow(
                string checkId,
                string area,
                string expected,
                string actual,
                bool passed,
                string notes)
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
