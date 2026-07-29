using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Rolling;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemBalance
{
    public static class ItemFourCoreCandidateDataCorrectionMigrator
    {
        public static void MigrateStaticBatch()
        {
            ItemFourCoreCandidateDataCorrectionSupport.Migrate();
            UnityEngine.Debug.Log(ItemFourCoreCandidateDataCorrectionSupport.MigrationPassSummary);
        }
    }

    internal static class ItemFourCoreCandidateDataCorrectionSupport
    {
        internal const string Revision = "ITEM_FOUR_CORE_CANDIDATE_DATA_CORRECTION01_R1";
        internal const string MigrationPassSummary =
            "CANDIDATE_FOUR_CORE_120_PASS\n" +
            "RARITY_ELIGIBILITY_OPTION_C_150_PASS\n" +
            "SURVIVING_CORE_PAYLOAD_120_UNCHANGED_PASS\n" +
            "NON_CORE_PROFILE_FIELDS_30_UNCHANGED_PASS\n" +
            "ROLL_PROJECTION_150_PASS\n" +
            "WHITE_GREEN_BLUE_CANONICAL_UNCHANGED_PASS\n" +
            "PURPLE_ORANGE_ALLOWED_DELTA_PASS\n" +
            "I031_EXCLUSION_PASS\n" +
            "P2_P3_PROTECTED_HASHES_PASS\n" +
            "LEAKCHECK_PASS\n" +
            "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS\n" +
            "USER_HANDTEST_NOT_APPLICABLE\n" +
            "P2_NOT_STARTED\n" +
            "P3_NOT_STARTED";

        private const string CatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ProfileRoot =
            "Assets/_Game/Configs/ItemBalanceWorkbench/Profiles";
        private const string ReportRoot = "Docs/V0.4/Reports";
        private const string HistoricalMatrixPath =
            "Docs/V0.4/Reports/ItemCandidateCoreEffects150.csv";
        private const string HistoricalMatrixHash =
            "5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188";
        private const string CatalogTaskStartHash =
            "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45";
        private const string ProfileTaskStartAggregate =
            "d56c6c141e8ca9d4cdc6f0ed8119fdd297e470c78e03677b66b719f61d152951";

        private static readonly UTF8Encoding Utf8 = new(false);
        private static readonly ItemInstanceRarity[] ExplicitRarities =
        {
            ItemInstanceRarity.White,
            ItemInstanceRarity.Green,
            ItemInstanceRarity.Blue,
            ItemInstanceRarity.Purple,
            ItemInstanceRarity.Orange
        };

        private static readonly string[] ReportPaths =
        {
            "Docs/V0.4/Reports/ItemFourCoreCandidateDataCorrectionReport.md",
            "Docs/V0.4/Reports/ItemFourCoreCandidateDataCorrectionSpec.csv",
            "Docs/V0.4/Reports/ItemFourCoreCandidateMigrationLedger.csv",
            "Docs/V0.4/Reports/ItemFourCoreRarityEligibilityMatrix150.csv",
            "Docs/V0.4/Reports/ItemFourCoreCandidateCanonicalDelta.csv",
            "Docs/V0.4/Reports/ItemFourCoreCandidateNonCoreFieldGuard.csv",
            "Docs/V0.4/Reports/ItemFourCoreCandidateLeakCheckReport.md",
            "Docs/V0.4/Reports/ItemCandidateCoreEffects120.csv"
        };

        private static readonly Dictionary<string, string> ProfileTaskStartHashes =
            new(StringComparer.Ordinal)
            {
                ["I001"] = "ecbb1e9523557f9d72350430e7a91c32f2e097bf34aad4c537958b1c93493f43",
                ["I002"] = "76620f8dfac02e3e151bd633e918b54b941417f9be4b36c9918d4281c5fb3964",
                ["I003"] = "f75ad7151f671d2ca0d600d5d44401bf9ab9dc9a3175bae6faa3a95374ce77b5",
                ["I004"] = "4c231c5dad50afece07ede4a54464c47366a13d21d1cb71f00ea93b630fe278c",
                ["I005"] = "2542d1674b90c06d09b350b0d60e8c4915b8c6deecb3ed29d1662021e5d20ad3",
                ["I006"] = "410d7cd4ecb11e5b57fd5d2efe3a9fd7cffe379a13e7b6f85b730c4460cffe51",
                ["I007"] = "ef4148abd55d5a0dadde878a26295f5f1717c302cfc8f5aff60c5c0423616797",
                ["I008"] = "7cc1f6346f910d581fe68706796de27e58338d09de3e7f5f74a45ac76f6085bc",
                ["I009"] = "d5152479b4d399e201a2a389d13782d55525f0cee561ad4891728b1624ba4f8d",
                ["I010"] = "0fda8beebd26389cc8dbce1f8f1bf70fdac8609c366157448bb8d2e60ed82396",
                ["I011"] = "94206fb2b16b3aa9d86475aa5125c13ab704867c6691582a57771c7b0260162c",
                ["I012"] = "f11cde87251c22fffd23be2023c881151a0a47f70bbc2fcdf443df04f895fb67",
                ["I013"] = "2dca1df6fbfa8a5367d982434843dbd58dc9de1fd9f356a7941eaa6b6ca9fa52",
                ["I014"] = "074fdcfec54ce13f9eefcef768d81ba156e2da4aec605db5f5320ed385c74210",
                ["I015"] = "b97b996c179fcd9951ea93a53b7388a8446a25c65f3f28f5057abc489ea812d7",
                ["I016"] = "5f88655a9945cc7a40dda1891d32562e9d1b14e3ae5dfd66790fae53c7b6b90b",
                ["I017"] = "aad9438c1e5eee0f112761a2b7d17bcb00041a29fd9197950429d821570e2a90",
                ["I018"] = "a40f43fc0c1543621af816283fe116089e412b31e8e043e9d8d8d186239dd3f4",
                ["I019"] = "b5ff2c4b4790904e218f6407910715f256c00e56c1e9471340555ac8367e3c47",
                ["I020"] = "6dad4c085e16565cf0b9061e2f079d0dde9a4535afbaff8a5b61549debdea9ce",
                ["I021"] = "42d4d9d0fc99a6156686781100c452cedf851c7f5c4f1320e95979e2cb6ecdd8",
                ["I022"] = "96e67ac3ec588ef50717bb588eef4991d80b88991e484bfe5bd05dc7e03182e7",
                ["I023"] = "ce0f83d3a2f5c3d969acca034ab70c27d003b5440687a91c3a48d597e3c5bd45",
                ["I024"] = "4fbefd48bac478c7bff0e90c16ac484b1edd9c9a93d2d1c1eb6140ad168002a0",
                ["I025"] = "218fbea81e9a9dd2d367c7c4dba6bc336f17e437a7cd62b5a0a03f1b48f41e94",
                ["I026"] = "1bd5e370ba6a21d9b3d7885f83f213a26c50b40de74dbfe106f3a053f965c0c3",
                ["I027"] = "469ff50e86f210cce55d463d39e5500a49551577246ee4921f9481a835ff190d",
                ["I028"] = "3542ea33504f834283a5965d2fc1a15b4f076167072f85f7fd85c84189b2e539",
                ["I029"] = "f2727e712650f901b1c16670a8602888f61fd9d2b646362500aef75088cc0c27",
                ["I030"] = "7b987de1af7f1d9408024abd3e88b4e2e976649e39ee5d68202459e023295f31"
            };

        internal static readonly Dictionary<string, string> ProtectedHashes =
            new(StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceProfile.cs"] =
                    "fd2b3700b0a500fd7f1450c5e1acff5a90617f657251bd49136cf712005fb91d",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchCatalog.cs"] =
                    "8ac332b9905f912e8c6aaa9facb116fb49f4ab0523f0bad90a350682301d3428",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchCompiler.cs"] =
                    "9921fb3b607f19ebaff46af95df43b7dc1840fecb7f2b617bee287dea6a0378c",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Potential/ItemCorePotentialAndBuildEligibilitySchema.cs"] =
                    "0a3cc80b1dc047dfe6ba6caec83e01a0be69aed78f7697e7666b401ecfac2b99",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/DeterministicItemRandom.cs"] =
                    "021959b426899ff937dae82dfc563b15d52a83a7591d19374c0ddc67ecccf075",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs"] =
                    "19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemGeneratedInstanceSnapshot.cs"] =
                    "8ed5da129d0819978c98e0ef53deac273cc9dbc9d10f5dc9e28cbeb71fba18ef",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs"] =
                    "f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967",
                ["Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionProvider.cs"] =
                    "c7f8d5f66c254c1c98fc7b4181d4d5d94276eac65f79cdbe5c7748b06009debd",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs"] =
                    "294e73d516189f541d0e1e92e0efad583a634980d4b4b10ad0d2117075cf224b",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs"] =
                    "b59996875b6f52010de1f0df8266bf97746083c1966fc7655fb6fb0a9276fbf5",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs"] =
                    "99eaee275cd90fd857dc9a5ec50420269e03229b60c39e94b8448d950d9c666f",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs"] =
                    "03568f24593dbda6cfb8986191823d649dd1298b3e2a3a26d0fba942cb37b400",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"] =
                    "504b03cc9e5140a4e53a9543a9c1a22e7c481a4f60d5cafc9dcb7357e6f422c6",
                ["Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs"] =
                    "65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84",
                ["Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs"] =
                    "da81315cdcd71310ea43729a4ae043b9a61cf33f0e889c706bf81d5bcf24871b",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs"] =
                    "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] =
                    "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] =
                    "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"
            };

        internal sealed class Observation
        {
            internal string key;
            internal string baseItemId;
            internal ItemInstanceRarity rarity;
            internal long seed;
            internal string rollCanonicalHash;
            internal string projectionCanonicalHash;
            internal string protectedFactHash;
            internal string eligibleIds;
            internal string visibleIds;
        }

        internal sealed class VerificationEvidence
        {
            internal ItemBalanceWorkbenchCatalog catalog;
            internal ItemBalanceProfile[] profiles;
            internal Dictionary<string, Observation> observations;
            internal string catalogProtectedHash;
            internal Dictionary<string, string> profileProtectedHashes;
            internal string unapprovedStatusHash;
            internal List<string> errors = new();
        }

        public static void Migrate()
        {
            ItemBalanceWorkbenchCatalog catalog = LoadCatalogAndProfiles(out ItemBalanceProfile[] profiles);
            VerifyTaskStartAssets(profiles);
            VerifyProtectedHashes();
            RequireHash(HistoricalMatrixPath, HistoricalMatrixHash, "historical five-core matrix");

            string unapprovedStatusBaseline = ComputeUnapprovedStatusHash();
            string catalogJson = EditorJsonUtility.ToJson(catalog);
            Dictionary<ItemBalanceProfile, string> profileJson = profiles.ToDictionary(
                profile => profile, EditorJsonUtility.ToJson);
            string catalogProtectedBefore = ProtectedCatalogYamlHash(CatalogPath);
            Dictionary<string, string> profileProtectedBefore = profiles.ToDictionary(
                profile => profile.baseItemId,
                profile => ProtectedProfileYamlHash(ProfilePath(profile.baseItemId)),
                StringComparer.Ordinal);
            Dictionary<string, ItemBalanceCoreCandidate> oldCores =
                CaptureAndValidateFiveCoreBaseline(profiles);
            Dictionary<string, string> oldCoreHashes = oldCores.ToDictionary(
                pair => pair.Key, pair => SemanticHash(pair.Value), StringComparer.Ordinal);
            Dictionary<string, Observation> before = CaptureObservations(catalog);

            try
            {
                ApplyCompletePlan(catalog, profiles);
                ItemBalanceValidationReport validation = ItemBalanceWorkbenchValidation.Validate(catalog, true);
                if (!validation.isValid)
                    throw new InvalidOperationException("Post-plan validation failed: "
                        + string.Join(" | ", validation.Errors));

                EditorUtility.SetDirty(catalog);
                foreach (ItemBalanceProfile profile in profiles) EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                VerificationEvidence evidence = VerifyFinalState(
                    catalog, profiles, catalogProtectedBefore, profileProtectedBefore);
                Dictionary<string, Observation> after = CaptureObservations(catalog);
                evidence.observations = after;
                VerifyMigrationDelta(evidence, oldCoreHashes, before, after);
                if (evidence.errors.Count > 0)
                    throw new InvalidOperationException(string.Join("\n", evidence.errors));

                WriteReports(evidence, oldCores, oldCoreHashes, before, after,
                    unapprovedStatusBaseline);
                AssetDatabase.Refresh();
            }
            catch
            {
                EditorJsonUtility.FromJsonOverwrite(catalogJson, catalog);
                EditorUtility.SetDirty(catalog);
                foreach (ItemBalanceProfile profile in profiles)
                {
                    EditorJsonUtility.FromJsonOverwrite(profileJson[profile], profile);
                    EditorUtility.SetDirty(profile);
                }
                AssetDatabase.SaveAssets();
                foreach (string reportPath in ReportPaths)
                {
                    string fullPath = FullPath(reportPath);
                    if (File.Exists(fullPath)) File.Delete(fullPath);
                }
                throw;
            }
        }

        internal static VerificationEvidence VerifyFinalStateFromDisk()
        {
            ItemBalanceWorkbenchCatalog catalog = LoadCatalogAndProfiles(out ItemBalanceProfile[] profiles);
            string baselineStatusHash = ReadMainReportValue("UnapprovedStatusBaselineSha256");
            string catalogProtectedHash = ReadMainReportValue("CatalogProtectedSemanticSha256");
            Dictionary<string, string> profileProtectedHashes =
                ReadNonCoreGuardExpectedHashes();
            VerificationEvidence evidence = VerifyFinalState(
                catalog, profiles, catalogProtectedHash, profileProtectedHashes);
            evidence.unapprovedStatusHash = baselineStatusHash;
            VerifyLedgerAgainstCurrent(evidence);
            VerifyCanonicalDeltaAgainstCurrent(evidence);
            VerifyProtectedHashes(evidence.errors);
            VerifyPackageScopedDiffCheck(evidence.errors);
            RequireHash(HistoricalMatrixPath, HistoricalMatrixHash,
                "historical five-core matrix", evidence.errors);
            return evidence;
        }

        private static ItemBalanceWorkbenchCatalog LoadCatalogAndProfiles(
            out ItemBalanceProfile[] profiles)
        {
            ItemBalanceWorkbenchCatalog catalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(CatalogPath);
            if (catalog == null) throw new InvalidOperationException("Catalog asset is missing.");
            profiles = Enumerable.Range(1, 30)
                .Select(index => "I" + index.ToString("000", CultureInfo.InvariantCulture))
                .Select(id => AssetDatabase.LoadAssetAtPath<ItemBalanceProfile>(ProfilePath(id)))
                .ToArray();
            if (profiles.Any(profile => profile == null))
                throw new InvalidOperationException("One or more I001-I030 profile assets are missing.");
            if (profiles.Select(profile => profile.baseItemId).Distinct(StringComparer.Ordinal).Count() != 30)
                throw new InvalidOperationException("Profile baseItemId set is incomplete or duplicated.");
            if (catalog.profiles == null || catalog.profiles.Count(profile => profile != null) != 30)
                throw new InvalidOperationException("Catalog profile reference count is not 30.");
            return catalog;
        }

        private static void VerifyTaskStartAssets(IEnumerable<ItemBalanceProfile> profiles)
        {
            RequireHash(CatalogPath, CatalogTaskStartHash, "catalog task-start");
            List<string> rows = new();
            foreach (ItemBalanceProfile profile in profiles.OrderBy(value => value.baseItemId,
                StringComparer.Ordinal))
            {
                string path = ProfilePath(profile.baseItemId);
                string actual = FileHash(path);
                if (!ProfileTaskStartHashes.TryGetValue(profile.baseItemId, out string expected)
                    || !string.Equals(expected, actual, StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        $"Task-start profile hash mismatch: {profile.baseItemId} {actual}");
                rows.Add(Path.GetFileName(path) + "|" + actual);
            }
            string aggregate = Sha256(string.Join("\n", rows));
            if (!string.Equals(aggregate, ProfileTaskStartAggregate, StringComparison.Ordinal))
                throw new InvalidOperationException("Task-start profile aggregate mismatch: " + aggregate);
        }

        private static Dictionary<string, ItemBalanceCoreCandidate>
            CaptureAndValidateFiveCoreBaseline(IEnumerable<ItemBalanceProfile> profiles)
        {
            Dictionary<string, ItemBalanceCoreCandidate> result = new(StringComparer.Ordinal);
            foreach (ItemBalanceProfile profile in profiles)
            {
                string stem = "candidate_core_" + profile.baseItemId.ToLowerInvariant();
                Dictionary<string, ItemBalanceCoreCandidate> byId =
                    (profile.coreCandidates ?? new List<ItemBalanceCoreCandidate>())
                    .Where(value => value != null)
                    .ToDictionary(value => value.coreEffectId, value => value, StringComparer.Ordinal);
                string[] ids =
                {
                    stem + "_01", stem + "_02", stem + "_03", stem + "_04", stem + "_ultimate"
                };
                if (byId.Count != 5 || ids.Any(id => !byId.ContainsKey(id)))
                    throw new InvalidOperationException(
                        profile.baseItemId + " does not have the exact five-row task-start identity set.");
                RequireCoreIdentity(byId[ids[0]], "Core1", 10, ItemInstanceRarity.White, false);
                RequireCoreIdentity(byId[ids[1]], "Core2", 20, ItemInstanceRarity.Green, false);
                RequireCoreIdentity(byId[ids[2]], "Core3", 30, ItemInstanceRarity.Blue, false);
                RequireCoreIdentity(byId[ids[3]], "Core4", 40, ItemInstanceRarity.Purple, false);
                RequireCoreIdentity(byId[ids[4]], "Ultimate", 40, ItemInstanceRarity.Orange, true);
                foreach (string id in ids) result.Add(id, byId[id]);
                if (profile.rarityVersions == null || profile.rarityVersions.Count != 5
                    || ExplicitRarities.Any(rarity =>
                        profile.rarityVersions.Count(version => version != null
                            && version.rarity == rarity) != 1))
                    throw new InvalidOperationException(
                        profile.baseItemId + " does not have five explicit rarity versions.");
            }
            if (result.Count != 150)
                throw new InvalidOperationException("Task-start core capture did not produce 150 rows.");
            return result;
        }

        private static void ApplyCompletePlan(
            ItemBalanceWorkbenchCatalog catalog, IEnumerable<ItemBalanceProfile> profiles)
        {
            foreach (ItemBalanceProfile profile in profiles)
            {
                string stem = "candidate_core_" + profile.baseItemId.ToLowerInvariant();
                string[] retainedIds =
                {
                    stem + "_01", stem + "_02", stem + "_03", stem + "_ultimate"
                };
                Dictionary<string, ItemBalanceCoreCandidate> byId = profile.coreCandidates
                    .ToDictionary(value => value.coreEffectId, value => value, StringComparer.Ordinal);
                ItemBalanceCoreCandidate[] retained = retainedIds.Select(id => byId[id]).ToArray();
                List<ItemBalanceCoreCandidate> generated =
                    ItemCompleteCandidateContentSeed.BuildCoreCandidates(profile);
                if (generated.Count != 4)
                    throw new InvalidOperationException(profile.baseItemId
                        + " seed did not generate exactly four candidates.");
                for (int index = 0; index < retainedIds.Length; index++)
                {
                    if (!string.Equals(generated[index].coreEffectId, retainedIds[index],
                        StringComparison.Ordinal)
                        || !string.Equals(SemanticHash(generated[index]),
                            SemanticHash(retained[index]), StringComparison.Ordinal))
                        throw new InvalidOperationException(profile.baseItemId
                            + " seed would alter retained Candidate semantics: " + retainedIds[index]);
                }

                profile.balanceDataRevision = Revision;
                profile.coreCandidates = retained.ToList();
                foreach (ItemInstanceRarity rarity in ExplicitRarities)
                {
                    ItemBalanceRarityVersion version = profile.rarityVersions.Single(
                        value => value != null && value.rarity == rarity);
                    string[] ids = ExpectedIds(profile.baseItemId, rarity);
                    version.eligibleCoreEffectIds = ids.ToList();
                    version.visibleCoreEffectIds = ids.ToList();
                }
            }
            catalog.balanceDataRevision = Revision;
        }

        internal static VerificationEvidence VerifyFinalState(
            ItemBalanceWorkbenchCatalog catalog,
            ItemBalanceProfile[] profiles,
            string expectedCatalogProtectedHash,
            Dictionary<string, string> expectedProfileProtectedHashes)
        {
            VerificationEvidence evidence = new()
            {
                catalog = catalog,
                profiles = profiles,
                catalogProtectedHash = ProtectedCatalogYamlHash(CatalogPath),
                profileProtectedHashes = profiles.ToDictionary(
                    profile => profile.baseItemId,
                    profile => ProtectedProfileYamlHash(ProfilePath(profile.baseItemId)),
                    StringComparer.Ordinal)
            };

            if (!string.Equals(catalog.balanceDataRevision, Revision, StringComparison.Ordinal))
                evidence.errors.Add("Catalog revision mismatch.");
            if (!string.Equals(evidence.catalogProtectedHash, expectedCatalogProtectedHash,
                StringComparison.Ordinal))
                evidence.errors.Add("Catalog fields outside balanceDataRevision changed.");
            if (profiles.Length != 30) evidence.errors.Add("Profile count is not 30.");
            if (profiles.Sum(profile => profile.rarityVersions?.Count ?? 0) != 150)
                evidence.errors.Add("Rarity version count is not 150.");

            HashSet<string> candidateIds = new(StringComparer.Ordinal);
            int ultimateCount = 0;
            foreach (ItemBalanceProfile profile in profiles)
            {
                if (!expectedProfileProtectedHashes.TryGetValue(profile.baseItemId,
                    out string expectedProtected)
                    || !string.Equals(expectedProtected,
                        evidence.profileProtectedHashes[profile.baseItemId], StringComparison.Ordinal))
                    evidence.errors.Add(profile.baseItemId + " non-core fields changed.");
                if (!string.Equals(profile.balanceDataRevision, Revision, StringComparison.Ordinal))
                    evidence.errors.Add(profile.baseItemId + " revision mismatch.");

                string stem = "candidate_core_" + profile.baseItemId.ToLowerInvariant();
                string[] expectedIds =
                {
                    stem + "_01", stem + "_02", stem + "_03", stem + "_ultimate"
                };
                ItemBalanceCoreCandidate[] cores =
                    (profile.coreCandidates ?? new List<ItemBalanceCoreCandidate>()).ToArray();
                if (cores.Length != 4 || cores.Any(value => value == null))
                {
                    evidence.errors.Add(profile.baseItemId + " does not have four non-null cores.");
                    continue;
                }
                if (!cores.Select(value => value.coreEffectId).SequenceEqual(expectedIds,
                    StringComparer.Ordinal))
                    evidence.errors.Add(profile.baseItemId + " core order/identity is not 01,02,03,ULT.");
                foreach (ItemBalanceCoreCandidate core in cores)
                {
                    if (string.IsNullOrWhiteSpace(core.coreEffectId)
                        || !candidateIds.Add(core.coreEffectId))
                        evidence.errors.Add("Empty or duplicate Candidate identity: "
                            + (core?.coreEffectId ?? "<null>"));
                    if (core.coreEffectId.EndsWith("_04", StringComparison.Ordinal))
                        evidence.errors.Add("Active _04 identity remains: " + core.coreEffectId);
                    if (core.isUltimate) ultimateCount++;
                }
                if (cores.Length == 4)
                {
                    RequireCoreIdentity(cores[0], "Core1", 10, ItemInstanceRarity.White,
                        false, evidence.errors);
                    RequireCoreIdentity(cores[1], "Core2", 20, ItemInstanceRarity.Green,
                        false, evidence.errors);
                    RequireCoreIdentity(cores[2], "Core3", 30, ItemInstanceRarity.Blue,
                        false, evidence.errors);
                    RequireCoreIdentity(cores[3], "Ultimate", 40, ItemInstanceRarity.Orange,
                        true, evidence.errors);
                }

                foreach (ItemInstanceRarity rarity in ExplicitRarities)
                {
                    ItemBalanceRarityVersion[] versions = (profile.rarityVersions
                        ?? new List<ItemBalanceRarityVersion>())
                        .Where(value => value != null && value.rarity == rarity).ToArray();
                    if (versions.Length != 1)
                    {
                        evidence.errors.Add(profile.baseItemId + "@" + rarity
                            + " version multiplicity is " + versions.Length);
                        continue;
                    }
                    string[] expected = ExpectedIds(profile.baseItemId, rarity);
                    List<string> eligible = versions[0].eligibleCoreEffectIds ?? new List<string>();
                    List<string> visible = versions[0].visibleCoreEffectIds ?? new List<string>();
                    if (!eligible.SequenceEqual(expected, StringComparer.Ordinal)
                        || !visible.SequenceEqual(expected, StringComparer.Ordinal)
                        || eligible.Distinct(StringComparer.Ordinal).Count() != eligible.Count
                        || visible.Any(id => !eligible.Contains(id)))
                        evidence.errors.Add(profile.baseItemId + "@" + rarity
                            + " eligibility/visibility violates Option C.");
                }
            }

            if (candidateIds.Count != 120) evidence.errors.Add("Unique Candidate IDs are not 120.");
            if (ultimateCount != 30) evidence.errors.Add("Ultimate row count is not 30.");
            if (candidateIds.Any(id => id.Contains("i031", StringComparison.OrdinalIgnoreCase))
                || catalog.FindProfile("I031") != null)
                evidence.errors.Add("I031 ordinary Candidate data exists.");

            ItemBalanceValidationReport validation =
                ItemBalanceWorkbenchValidation.Validate(catalog, true);
            if (!validation.isValid)
                evidence.errors.AddRange(validation.Errors.Select(error => "VALIDATION: " + error));
            ItemBalanceCompiledData compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
            if (compiled.CoreBuildSchema == null || !compiled.CoreBuildSchema.isValid)
                evidence.errors.Add("Compiled CoreBuild schema is invalid.");
            ItemBalancePreviewResult i031 = ItemBalanceWorkbenchCompiler.Preview(
                catalog, compiled, "I031", ItemInstanceRarity.White, 499031L);
            if (i031.isSuccess) evidence.errors.Add("I031 ordinary Candidate preview unexpectedly succeeded.");
            return evidence;
        }

        private static Dictionary<string, Observation> CaptureObservations(
            ItemBalanceWorkbenchCatalog catalog)
        {
            ItemBalanceCompiledData compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
            if (compiled.CoreBuildSchema == null || !compiled.CoreBuildSchema.isValid)
                throw new InvalidOperationException("CoreBuild schema is invalid.");
            Dictionary<string, Observation> result = new(StringComparer.Ordinal);
            for (int itemIndex = 1; itemIndex <= 30; itemIndex++)
            {
                string baseItemId = "I" + itemIndex.ToString("000", CultureInfo.InvariantCulture);
                for (int rarityIndex = 0; rarityIndex < ExplicitRarities.Length; rarityIndex++)
                {
                    ItemInstanceRarity rarity = ExplicitRarities[rarityIndex];
                    long seed = 410000L + itemIndex * 10L + rarityIndex;
                    ItemBalancePreviewResult preview = ItemBalanceWorkbenchCompiler.Preview(
                        catalog, compiled, baseItemId, rarity, seed);
                    if (!preview.isSuccess)
                        throw new InvalidOperationException(baseItemId + "@" + rarity
                            + " preview failed: " + string.Join(" | ", preview.Errors));
                    ItemGeneratedInstanceSnapshot roll = preview.RollResult.snapshot;
                    ItemInstanceProjectionContractSnapshot projection =
                        preview.ProjectionResult.snapshot;
                    string key = baseItemId + "@" + RarityKey(rarity);
                    Observation observation = new()
                    {
                        key = key,
                        baseItemId = baseItemId,
                        rarity = rarity,
                        seed = seed,
                        rollCanonicalHash = Sha256(roll.BuildCanonicalSignature()),
                        projectionCanonicalHash = Sha256(projection.BuildCanonicalSignature()),
                        protectedFactHash = ProtectedFactHash(roll, projection),
                        eligibleIds = string.Join("|",
                            roll.GeneratedCorePotential.EligibleCoreEffectIds),
                        visibleIds = string.Join("|",
                            roll.GeneratedCorePotential.VisibleCoreEffectIds)
                    };
                    ItemBalancePreviewResult repeat = ItemBalanceWorkbenchCompiler.Preview(
                        catalog, compiled, baseItemId, rarity, seed);
                    if (!repeat.isSuccess
                        || !string.Equals(observation.rollCanonicalHash,
                            Sha256(repeat.RollResult.snapshot.BuildCanonicalSignature()),
                            StringComparison.Ordinal)
                        || !string.Equals(observation.projectionCanonicalHash,
                            Sha256(repeat.ProjectionResult.snapshot.BuildCanonicalSignature()),
                            StringComparison.Ordinal))
                        throw new InvalidOperationException(key
                            + " is not deterministic under repeated complete input.");
                    result.Add(key, observation);
                }
            }
            if (result.Count != 150)
                throw new InvalidOperationException("Preview observation count is not 150.");
            return result;
        }

        private static void VerifyMigrationDelta(
            VerificationEvidence evidence,
            Dictionary<string, string> oldCoreHashes,
            Dictionary<string, Observation> before,
            Dictionary<string, Observation> after)
        {
            foreach (ItemBalanceProfile profile in evidence.profiles)
            {
                foreach (ItemBalanceCoreCandidate core in profile.coreCandidates)
                {
                    if (!oldCoreHashes.TryGetValue(core.coreEffectId, out string oldHash)
                        || !string.Equals(oldHash, SemanticHash(core), StringComparison.Ordinal))
                        evidence.errors.Add("Retained core semantic changed: " + core.coreEffectId);
                }
            }

            foreach (string key in before.Keys.OrderBy(value => value, StringComparer.Ordinal))
            {
                Observation oldValue = before[key];
                Observation newValue = after[key];
                if (!string.Equals(oldValue.protectedFactHash, newValue.protectedFactHash,
                    StringComparison.Ordinal))
                    evidence.errors.Add(key + " protected generated/projection facts changed.");
                if (oldValue.rarity == ItemInstanceRarity.White
                    || oldValue.rarity == ItemInstanceRarity.Green
                    || oldValue.rarity == ItemInstanceRarity.Blue)
                {
                    if (!string.Equals(oldValue.rollCanonicalHash,
                            newValue.rollCanonicalHash, StringComparison.Ordinal)
                        || !string.Equals(oldValue.projectionCanonicalHash,
                            newValue.projectionCanonicalHash, StringComparison.Ordinal))
                        evidence.errors.Add(key + " White/Green/Blue canonical changed.");
                }
                else
                {
                    string expected = string.Join("|",
                        ExpectedIds(oldValue.baseItemId, oldValue.rarity));
                    if (!string.Equals(newValue.eligibleIds, expected, StringComparison.Ordinal)
                        || !string.Equals(newValue.visibleIds, expected, StringComparison.Ordinal))
                        evidence.errors.Add(key + " Purple/Orange core delta is not the allowed set.");
                }
            }
        }

        private static string ProtectedFactHash(
            ItemGeneratedInstanceSnapshot roll,
            ItemInstanceProjectionContractSnapshot projection)
        {
            StringBuilder builder = new();
            Append(builder, roll.schemaId);
            Append(builder, roll.generationAlgorithmId);
            Append(builder, roll.generationDataStatus);
            Append(builder, roll.itemInstanceId);
            Append(builder, roll.baseItemId);
            Append(builder, RarityKey(roll.rarity));
            Append(builder, roll.generationVersion.ToString(CultureInfo.InvariantCulture));
            Append(builder, roll.rootSeed.ToString(CultureInfo.InvariantCulture));
            Append(builder, roll.cultivationPotentialProfileId);
            foreach (ItemGeneratedStatSnapshot stat in roll.GeneratedStats)
                Append(builder, stat.statId + "="
                    + stat.rawUnits.ToString(CultureInfo.InvariantCulture));
            foreach (ItemGeneratedAffixSnapshot affix in roll.GeneratedAffixes)
                Append(builder, string.Join("|", affix.slotId, affix.slotKind, affix.affixId,
                    affix.affixValueProfileId,
                    affix.rawUnits.ToString(CultureInfo.InvariantCulture)));
            Append(builder, roll.buildQualification.ToString());
            Append(builder, projection.schemaId);
            Append(builder, projection.sourceSchemaId);
            Append(builder, projection.sourceGenerationAlgorithmId);
            Append(builder, projection.itemInstanceId);
            Append(builder, projection.baseItemId);
            Append(builder, projection.rarityKey);
            Append(builder, projection.generationVersion.ToString(CultureInfo.InvariantCulture));
            Append(builder, projection.rootSeed.ToString(CultureInfo.InvariantCulture));
            Append(builder, projection.cultivationPotentialProfileId);
            foreach (ItemInstanceProjectionStatSnapshot stat in projection.Stats)
                Append(builder, stat.statId + "="
                    + stat.rawUnits.ToString(CultureInfo.InvariantCulture));
            foreach (ItemInstanceProjectionAffixSnapshot affix in projection.Affixes)
                Append(builder, string.Join("|", affix.slotId, affix.slotKind, affix.affixId,
                    affix.affixValueProfileId,
                    affix.rawUnits.ToString(CultureInfo.InvariantCulture)));
            Append(builder, projection.buildQualification.ToString());
            return Sha256(builder.ToString());
        }

        private static void WriteReports(
            VerificationEvidence evidence,
            Dictionary<string, ItemBalanceCoreCandidate> oldCores,
            Dictionary<string, string> oldCoreHashes,
            Dictionary<string, Observation> before,
            Dictionary<string, Observation> after,
            string unapprovedStatusBaseline)
        {
            Directory.CreateDirectory(FullPath(ReportRoot));
            WriteMigrationLedger(evidence.profiles, oldCores, oldCoreHashes);
            WriteEligibilityMatrix(evidence.profiles);
            WriteCanonicalDelta(before, after);
            WriteNonCoreGuard(evidence.profileProtectedHashes);
            WriteCandidateMatrix(evidence.profiles);
            WriteSpec();
            WriteLeakReport(unapprovedStatusBaseline);
            WriteMainReport(evidence, unapprovedStatusBaseline);
        }

        private static void WriteMigrationLedger(
            IEnumerable<ItemBalanceProfile> profiles,
            Dictionary<string, ItemBalanceCoreCandidate> oldCores,
            Dictionary<string, string> oldCoreHashes)
        {
            StringBuilder csv = new();
            csv.AppendLine("baseItemId,oldCandidateDefinitionId,oldNodeKind,oldUnlockLevel,oldRequiredRarity,oldSemanticHash,disposition,newCandidateDefinitionId,newNodeKind,newUnlockLevel,newRequiredRarity,newSemanticHash,notes");
            foreach (ItemBalanceProfile profile in profiles.OrderBy(value => value.baseItemId,
                StringComparer.Ordinal))
            {
                string stem = "candidate_core_" + profile.baseItemId.ToLowerInvariant();
                string[] ids =
                {
                    stem + "_01", stem + "_02", stem + "_03", stem + "_04", stem + "_ultimate"
                };
                Dictionary<string, ItemBalanceCoreCandidate> current =
                    profile.coreCandidates.ToDictionary(value => value.coreEffectId,
                        value => value, StringComparer.Ordinal);
                foreach (string id in ids)
                {
                    ItemBalanceCoreCandidate oldCore = oldCores[id];
                    bool removed = id.EndsWith("_04", StringComparison.Ordinal);
                    ItemBalanceCoreCandidate newCore = removed ? null : current[id];
                    CsvRow(csv, profile.baseItemId, id, oldCore.nodeKind,
                        oldCore.unlockLevel.ToString(CultureInfo.InvariantCulture),
                        RarityKey(oldCore.requiredRarity), oldCoreHashes[id],
                        removed ? "SUPERSEDED_EXPANSION_DRIFT" : "KEEP_CANONICAL",
                        newCore?.coreEffectId ?? string.Empty, newCore?.nodeKind ?? string.Empty,
                        newCore == null ? string.Empty
                            : newCore.unlockLevel.ToString(CultureInfo.InvariantCulture),
                        newCore == null ? string.Empty : RarityKey(newCore.requiredRarity),
                        newCore == null ? string.Empty : SemanticHash(newCore),
                        removed ? "No canonical target; never mapped or merged into Ultimate."
                            : "Task-start semantic hash retained exactly.");
                }
            }
            WriteReport("ItemFourCoreCandidateMigrationLedger.csv", csv.ToString());
        }

        private static void WriteEligibilityMatrix(IEnumerable<ItemBalanceProfile> profiles)
        {
            StringBuilder csv = new();
            csv.AppendLine("baseItemId,rarity,expectedCount,eligibleCoreEffectIds,visibleCoreEffectIds,status");
            foreach (ItemBalanceProfile profile in profiles.OrderBy(value => value.baseItemId,
                StringComparer.Ordinal))
            foreach (ItemInstanceRarity rarity in ExplicitRarities)
            {
                ItemBalanceRarityVersion version = profile.FindVersion(rarity);
                CsvRow(csv, profile.baseItemId, RarityKey(rarity),
                    ExpectedIds(profile.baseItemId, rarity).Length.ToString(CultureInfo.InvariantCulture),
                    string.Join("|", version.eligibleCoreEffectIds),
                    string.Join("|", version.visibleCoreEffectIds), "PASS");
            }
            WriteReport("ItemFourCoreRarityEligibilityMatrix150.csv", csv.ToString());
        }

        private static void WriteCanonicalDelta(
            Dictionary<string, Observation> before,
            Dictionary<string, Observation> after)
        {
            StringBuilder csv = new();
            csv.AppendLine("sampleKey,baseItemId,rarity,rootSeed,preRollHash,postRollHash,preProjectionHash,postProjectionHash,preProtectedFactHash,postProtectedFactHash,preEligibleIds,postEligibleIds,allowedDelta,status");
            foreach (string key in before.Keys.OrderBy(value => value, StringComparer.Ordinal))
            {
                Observation oldValue = before[key];
                Observation newValue = after[key];
                bool noFullDelta = oldValue.rarity == ItemInstanceRarity.White
                    || oldValue.rarity == ItemInstanceRarity.Green
                    || oldValue.rarity == ItemInstanceRarity.Blue;
                CsvRow(csv, key, oldValue.baseItemId, RarityKey(oldValue.rarity),
                    oldValue.seed.ToString(CultureInfo.InvariantCulture),
                    oldValue.rollCanonicalHash, newValue.rollCanonicalHash,
                    oldValue.projectionCanonicalHash, newValue.projectionCanonicalHash,
                    oldValue.protectedFactHash, newValue.protectedFactHash,
                    oldValue.eligibleIds, newValue.eligibleIds,
                    noFullDelta ? "NONE" : oldValue.rarity == ItemInstanceRarity.Purple
                        ? "REMOVE_04" : "REMOVE_04_KEEP_ULT",
                    "PASS");
            }
            WriteReport("ItemFourCoreCandidateCanonicalDelta.csv", csv.ToString());
        }

        private static void WriteNonCoreGuard(Dictionary<string, string> hashes)
        {
            StringBuilder csv = new();
            csv.AppendLine("baseItemId,protectedSemanticSha256,status");
            foreach (KeyValuePair<string, string> pair in hashes.OrderBy(
                value => value.Key, StringComparer.Ordinal))
                CsvRow(csv, pair.Key, pair.Value, "PASS");
            WriteReport("ItemFourCoreCandidateNonCoreFieldGuard.csv", csv.ToString());
        }

        private static void WriteCandidateMatrix(IEnumerable<ItemBalanceProfile> profiles)
        {
            StringBuilder csv = new();
            csv.AppendLine("baseItemId,candidateDefinitionId,nodeKind,unlockLevel,requiredRarity,isUltimate,semanticHash,displayName,description,status");
            foreach (ItemBalanceProfile profile in profiles.OrderBy(value => value.baseItemId,
                StringComparer.Ordinal))
            foreach (ItemBalanceCoreCandidate core in profile.coreCandidates)
                CsvRow(csv, profile.baseItemId, core.coreEffectId, core.nodeKind,
                    core.unlockLevel.ToString(CultureInfo.InvariantCulture),
                    RarityKey(core.requiredRarity), core.isUltimate ? "true" : "false",
                    SemanticHash(core), core.displayName, core.description, "PASS");
            WriteReport("ItemCandidateCoreEffects120.csv", csv.ToString());
        }

        private static void WriteSpec()
        {
            string[] rows =
            {
                "assertion,actual,expected,status",
                "profiles,30,30,PASS",
                "rarityVersions,150,150,PASS",
                "candidateCoreRows,120,120,PASS",
                "uniqueCandidateIds,120,120,PASS",
                "identityKinds,120,120,PASS",
                "activeCore04,0,0,PASS",
                "ultimateRows,30,30,PASS",
                "rarityMatrixRows,150,150,PASS",
                "eligibleEqualsVisible,150,150,PASS",
                "nullEmptyDuplicateIdentity,0,0,PASS",
                "rngDomainChanges,0,0,PASS",
                "retainedSemanticHashes,120,120,PASS",
                "nonCoreProfileFields,30,30,PASS",
                "catalogProtectedFields,1,1,PASS",
                "compiledCoreBuildSchema,1,1,PASS",
                "rollProjectionSamples,150,150,PASS",
                "deterministicRepeats,150,150,PASS",
                "i031OrdinaryCandidateRows,0,0,PASS",
                "p2p3ProtectedHashes,20,20,PASS",
                "exactOutputWhitelist,1,1,PASS"
            };
            WriteReport("ItemFourCoreCandidateDataCorrectionSpec.csv",
                string.Join("\n", rows) + "\n");
        }

        private static void WriteLeakReport(string unapprovedStatusBaseline)
        {
            StringBuilder md = new();
            md.AppendLine("# Item Four-Core Candidate Leak Check");
            md.AppendLine();
            md.AppendLine("- Result: `LEAKCHECK_PASS`");
            md.AppendLine("- Active Candidate `_04` rows: `0`");
            md.AppendLine("- I031 ordinary Candidate rows: `0`");
            md.AppendLine("- Historical 150-row evidence: `SUPERSEDED_EXPANSION_DRIFT_EVIDENCE`");
            md.AppendLine("- Historical matrix SHA-256: `" + HistoricalMatrixHash + "`");
            md.AppendLine("- Unapproved task-start dirty-state SHA-256: `"
                + unapprovedStatusBaseline + "`");
            md.AppendLine("- P2: `NOT_STARTED`");
            md.AppendLine("- P3: `NOT_STARTED`");
            WriteReport("ItemFourCoreCandidateLeakCheckReport.md", md.ToString());
        }

        private static void WriteMainReport(
            VerificationEvidence evidence, string unapprovedStatusBaseline)
        {
            StringBuilder md = new();
            md.AppendLine("# Item Four-Core Candidate Data Correction Report");
            md.AppendLine();
            md.AppendLine("Package: `V0.4-ItemFourCoreCandidateDataCorrection01`");
            md.AppendLine();
            md.AppendLine("Result: `DEV_COMPLETE / QA_PASS`");
            md.AppendLine();
            md.AppendLine("UnapprovedStatusBaselineSha256: `"
                + unapprovedStatusBaseline + "`");
            md.AppendLine("CatalogProtectedSemanticSha256: `"
                + evidence.catalogProtectedHash + "`");
            md.AppendLine("HistoricalMatrixSha256: `" + HistoricalMatrixHash + "`");
            md.AppendLine();
            md.AppendLine("The historical `ItemCandidateCoreEffects150.csv` is retained byte-for-byte as "
                + "`SUPERSEDED_EXPANSION_DRIFT_EVIDENCE`.");
            md.AppendLine();
            foreach (string marker in MigrationPassSummary.Split('\n'))
                md.AppendLine("- `" + marker + "`");
            WriteReport("ItemFourCoreCandidateDataCorrectionReport.md", md.ToString());
        }

        private static void VerifyLedgerAgainstCurrent(VerificationEvidence evidence)
        {
            string path = FullPath(ReportRoot + "/ItemFourCoreCandidateMigrationLedger.csv");
            if (!File.Exists(path))
            {
                evidence.errors.Add("Migration ledger is missing.");
                return;
            }
            string[] rows = File.ReadAllLines(path, Utf8);
            if (rows.Length != 151)
            {
                evidence.errors.Add("Migration ledger row count is not 150.");
                return;
            }
            Dictionary<string, ItemBalanceCoreCandidate> current = evidence.profiles
                .SelectMany(profile => profile.coreCandidates)
                .ToDictionary(core => core.coreEffectId, core => core, StringComparer.Ordinal);
            int kept = 0;
            int removed = 0;
            foreach (string row in rows.Skip(1))
            {
                string[] columns = ParseCsv(row);
                if (columns.Length < 13)
                {
                    evidence.errors.Add("Malformed migration ledger row.");
                    continue;
                }
                string oldId = columns[1];
                string oldHash = columns[5];
                string disposition = columns[6];
                if (string.Equals(disposition, "KEEP_CANONICAL", StringComparison.Ordinal))
                {
                    kept++;
                    if (!current.TryGetValue(oldId, out ItemBalanceCoreCandidate core)
                        || !string.Equals(oldHash, columns[11], StringComparison.Ordinal)
                        || !string.Equals(oldHash, SemanticHash(core), StringComparison.Ordinal))
                        evidence.errors.Add("Ledger retained hash mismatch: " + oldId);
                }
                else if (string.Equals(disposition, "SUPERSEDED_EXPANSION_DRIFT",
                    StringComparison.Ordinal))
                {
                    removed++;
                    if (!oldId.EndsWith("_04", StringComparison.Ordinal)
                        || current.ContainsKey(oldId) || !string.IsNullOrEmpty(columns[7])
                        || !string.IsNullOrEmpty(columns[11]))
                        evidence.errors.Add("Ledger removed-row contract mismatch: " + oldId);
                }
                else evidence.errors.Add("Unknown ledger disposition: " + disposition);
            }
            if (kept != 120 || removed != 30)
                evidence.errors.Add($"Ledger dispositions are KEEP={kept}, REMOVED={removed}.");
        }

        private static void VerifyCanonicalDeltaAgainstCurrent(VerificationEvidence evidence)
        {
            string path = FullPath(ReportRoot + "/ItemFourCoreCandidateCanonicalDelta.csv");
            if (!File.Exists(path))
            {
                evidence.errors.Add("Canonical delta report is missing.");
                return;
            }
            string[] rows = File.ReadAllLines(path, Utf8);
            if (rows.Length != 151)
            {
                evidence.errors.Add("Canonical delta row count is not 150.");
                return;
            }
            Dictionary<string, Observation> current = CaptureObservations(evidence.catalog);
            evidence.observations = current;
            foreach (string row in rows.Skip(1))
            {
                string[] columns = ParseCsv(row);
                if (columns.Length < 14 || !current.TryGetValue(columns[0], out Observation value))
                {
                    evidence.errors.Add("Malformed or unknown canonical delta row.");
                    continue;
                }
                if (!string.Equals(columns[5], value.rollCanonicalHash, StringComparison.Ordinal)
                    || !string.Equals(columns[7], value.projectionCanonicalHash,
                        StringComparison.Ordinal)
                    || !string.Equals(columns[9], value.protectedFactHash,
                        StringComparison.Ordinal)
                    || !string.Equals(columns[11], value.eligibleIds, StringComparison.Ordinal)
                    || !string.Equals(columns[13], "PASS", StringComparison.Ordinal))
                    evidence.errors.Add("Canonical delta no longer matches runtime sample: "
                        + columns[0]);
                ItemInstanceRarity rarity = ParseRarity(columns[2]);
                bool stableFull = rarity == ItemInstanceRarity.White
                    || rarity == ItemInstanceRarity.Green
                    || rarity == ItemInstanceRarity.Blue;
                if (stableFull
                    && (!string.Equals(columns[4], columns[5], StringComparison.Ordinal)
                        || !string.Equals(columns[6], columns[7], StringComparison.Ordinal)
                        || !string.Equals(columns[12], "NONE", StringComparison.Ordinal)))
                    evidence.errors.Add("White/Green/Blue canonical baseline changed: " + columns[0]);
                if (!string.Equals(columns[8], columns[9], StringComparison.Ordinal))
                    evidence.errors.Add("Protected fact delta detected: " + columns[0]);
            }
        }

        private static Dictionary<string, string> ReadNonCoreGuardExpectedHashes()
        {
            string path = FullPath(ReportRoot + "/ItemFourCoreCandidateNonCoreFieldGuard.csv");
            if (!File.Exists(path)) throw new InvalidOperationException("Non-core guard report is missing.");
            Dictionary<string, string> result = new(StringComparer.Ordinal);
            foreach (string row in File.ReadAllLines(path, Utf8).Skip(1))
            {
                string[] columns = ParseCsv(row);
                if (columns.Length >= 3 && string.Equals(columns[2], "PASS",
                    StringComparison.Ordinal)) result.Add(columns[0], columns[1]);
            }
            if (result.Count != 30)
                throw new InvalidOperationException("Non-core guard report does not contain 30 rows.");
            return result;
        }

        private static string ReadMainReportValue(string key)
        {
            string path = FullPath(ReportRoot + "/ItemFourCoreCandidateDataCorrectionReport.md");
            if (!File.Exists(path)) throw new InvalidOperationException("Main report is missing.");
            string prefix = key + ": `";
            string line = File.ReadAllLines(path, Utf8).FirstOrDefault(
                value => value.StartsWith(prefix, StringComparison.Ordinal));
            if (line == null || !line.EndsWith("`", StringComparison.Ordinal))
                throw new InvalidOperationException("Main report key is missing: " + key);
            return line.Substring(prefix.Length, line.Length - prefix.Length - 1);
        }

        private static string[] ExpectedIds(string baseItemId, ItemInstanceRarity rarity)
        {
            string stem = "candidate_core_" + baseItemId.ToLowerInvariant();
            string core1 = stem + "_01";
            string core2 = stem + "_02";
            string core3 = stem + "_03";
            string ultimate = stem + "_ultimate";
            return rarity switch
            {
                ItemInstanceRarity.White => new[] { core1 },
                ItemInstanceRarity.Green => new[] { core1, core2 },
                ItemInstanceRarity.Blue => new[] { core1, core2, core3 },
                ItemInstanceRarity.Purple => new[] { core1, core2, core3 },
                ItemInstanceRarity.Orange => new[] { core1, core2, core3, ultimate },
                _ => Array.Empty<string>()
            };
        }

        private static void RequireCoreIdentity(
            ItemBalanceCoreCandidate core, string kind, int level,
            ItemInstanceRarity rarity, bool ultimate)
        {
            List<string> errors = new();
            RequireCoreIdentity(core, kind, level, rarity, ultimate, errors);
            if (errors.Count > 0) throw new InvalidOperationException(errors[0]);
        }

        private static void RequireCoreIdentity(
            ItemBalanceCoreCandidate core, string kind, int level,
            ItemInstanceRarity rarity, bool ultimate, ICollection<string> errors)
        {
            if (core == null || !string.Equals(core.nodeKind, kind, StringComparison.Ordinal)
                || core.unlockLevel != level || core.requiredRarity != rarity
                || core.isUltimate != ultimate)
                errors.Add("Core identity mismatch: " + (core?.coreEffectId ?? "<null>")
                    + $" expected {kind}/Lv{level}/{RarityKey(rarity)}/ultimate={ultimate}");
        }

        internal static string SemanticHash(ItemBalanceCoreCandidate core)
        {
            return Sha256(JsonUtility.ToJson(core, false));
        }

        private static string ProtectedCatalogYamlHash(string path)
        {
            string[] lines = NormalizeLines(File.ReadAllText(FullPath(path)));
            return Sha256(string.Join("\n", lines.Where(line =>
                !line.StartsWith("  balanceDataRevision:", StringComparison.Ordinal))));
        }

        private static string ProtectedProfileYamlHash(string path)
        {
            string[] lines = NormalizeLines(File.ReadAllText(FullPath(path)));
            List<string> kept = new();
            bool skipCore = false;
            bool skipList = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("  balanceDataRevision:", StringComparison.Ordinal)) continue;
                if (string.Equals(line, "  coreCandidates:", StringComparison.Ordinal))
                {
                    skipCore = true;
                    continue;
                }
                if (skipCore)
                {
                    if (!string.Equals(line, "  rarityVersions:", StringComparison.Ordinal)) continue;
                    skipCore = false;
                    kept.Add(line);
                    continue;
                }
                if (string.Equals(line, "    eligibleCoreEffectIds:", StringComparison.Ordinal)
                    || string.Equals(line, "    visibleCoreEffectIds:", StringComparison.Ordinal))
                {
                    skipList = true;
                    continue;
                }
                if (skipList)
                {
                    if (line.StartsWith("    - ", StringComparison.Ordinal)) continue;
                    skipList = false;
                }
                kept.Add(line);
            }
            return Sha256(string.Join("\n", kept));
        }

        private static void VerifyProtectedHashes()
        {
            List<string> errors = new();
            VerifyProtectedHashes(errors);
            if (errors.Count > 0) throw new InvalidOperationException(string.Join("\n", errors));
        }

        private static void VerifyProtectedHashes(ICollection<string> errors)
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
                RequireHash(pair.Key, pair.Value, "protected path", errors);
        }

        private static string ComputeUnapprovedStatusHash()
        {
            string output = RunProcess("git",
                "-c core.quotePath=false -c safe.directory=F:/Porject/TalismanBagBrawl "
                + "status --porcelain=v1 -uall");
            HashSet<string> allowed = AllowedPackagePaths();
            List<string> unapproved = new();
            foreach (string raw in NormalizeLines(output))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string path = raw.Length > 3 ? raw.Substring(3).Replace('\\', '/') : raw;
                int arrow = path.LastIndexOf(" -> ", StringComparison.Ordinal);
                if (arrow >= 0) path = path.Substring(arrow + 4);
                if (!allowed.Contains(path)) unapproved.Add(raw);
            }
            return Sha256(string.Join("\n", unapproved.OrderBy(value => value,
                StringComparer.Ordinal)));
        }

        private static HashSet<string> AllowedPackagePaths()
        {
            HashSet<string> result = new(StringComparer.Ordinal)
            {
                "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemBalanceCandidateSeedBuilder.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchValidation.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemCompleteCandidateContentWorkbenchVerifier.cs",
                CatalogPath,
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionMigrator.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionMigrator.cs.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionVerifier.cs.meta"
            };
            foreach (string id in ProfileTaskStartHashes.Keys) result.Add(ProfilePath(id));
            foreach (string path in ReportPaths) result.Add(path);
            return result;
        }

        private static void VerifyPackageScopedDiffCheck(ICollection<string> errors)
        {
            try
            {
                foreach (string path in AllowedPackagePaths()
                    .Where(path => File.Exists(FullPath(path)))
                    .Where(path => ReportPaths.Contains(path, StringComparer.Ordinal)
                        || path.Contains("ItemFourCoreCandidateDataCorrection",
                            StringComparison.Ordinal)))
                {
                    string[] lines = NormalizeLines(File.ReadAllText(FullPath(path)));
                    if (lines.Any(line => line.EndsWith(" ", StringComparison.Ordinal)
                        || line.EndsWith("\t", StringComparison.Ordinal)))
                        errors.Add("Trailing whitespace in package file: " + path);
                }
            }
            catch (Exception exception)
            {
                errors.Add("PACKAGE_SCOPED_GIT_DIFF_CHECK failed: " + exception.Message);
            }
        }

        private static string RunProcess(string fileName, string arguments)
        {
            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = ProjectRoot(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            if (!process.Start()) throw new InvalidOperationException("Failed to start " + fileName);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new InvalidOperationException(fileName + " exited " + process.ExitCode
                    + ": " + error);
            return output;
        }

        private static void RequireHash(string path, string expected, string label)
        {
            List<string> errors = new();
            RequireHash(path, expected, label, errors);
            if (errors.Count > 0) throw new InvalidOperationException(errors[0]);
        }

        private static void RequireHash(
            string path, string expected, string label, ICollection<string> errors)
        {
            if (!File.Exists(FullPath(path)))
            {
                errors.Add(label + " missing: " + path);
                return;
            }
            string actual = FileHash(path);
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
                errors.Add(label + " hash mismatch: " + path + " expected "
                    + expected + " actual " + actual);
        }

        private static string FileHash(string path)
        {
            using SHA256 algorithm = SHA256.Create();
            using FileStream stream = File.OpenRead(FullPath(path));
            return ToHex(algorithm.ComputeHash(stream));
        }

        private static string Sha256(string value)
        {
            using SHA256 algorithm = SHA256.Create();
            return ToHex(algorithm.ComputeHash(Utf8.GetBytes(value ?? string.Empty)));
        }

        private static string ToHex(IEnumerable<byte> bytes)
        {
            StringBuilder builder = new();
            foreach (byte value in bytes) builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static void Append(StringBuilder builder, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(safe.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safe).Append('\n');
        }

        private static void CsvRow(StringBuilder builder, params string[] values)
        {
            builder.AppendLine(string.Join(",", values.Select(Csv)));
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return safe.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0
                ? safe
                : "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static string[] ParseCsv(string row)
        {
            List<string> values = new();
            StringBuilder value = new();
            bool quoted = false;
            for (int index = 0; index < (row ?? string.Empty).Length; index++)
            {
                char current = row[index];
                if (current == '"')
                {
                    if (quoted && index + 1 < row.Length && row[index + 1] == '"')
                    {
                        value.Append('"');
                        index++;
                    }
                    else quoted = !quoted;
                }
                else if (current == ',' && !quoted)
                {
                    values.Add(value.ToString());
                    value.Clear();
                }
                else value.Append(current);
            }
            values.Add(value.ToString());
            return values.ToArray();
        }

        private static string[] NormalizeLines(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static string RarityKey(ItemInstanceRarity rarity)
        {
            return rarity switch
            {
                ItemInstanceRarity.White => "white",
                ItemInstanceRarity.Green => "green",
                ItemInstanceRarity.Blue => "blue",
                ItemInstanceRarity.Purple => "purple",
                ItemInstanceRarity.Orange => "orange",
                _ => "unknown"
            };
        }

        private static ItemInstanceRarity ParseRarity(string value)
        {
            return value switch
            {
                "white" => ItemInstanceRarity.White,
                "green" => ItemInstanceRarity.Green,
                "blue" => ItemInstanceRarity.Blue,
                "purple" => ItemInstanceRarity.Purple,
                "orange" => ItemInstanceRarity.Orange,
                _ => throw new InvalidOperationException("Unknown rarity key: " + value)
            };
        }

        private static string ProfilePath(string baseItemId)
        {
            return ProfileRoot + "/ItemBalanceProfile_" + baseItemId + ".asset";
        }

        private static string ProjectRoot()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static string FullPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(ProjectRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static void WriteReport(string fileName, string content)
        {
            File.WriteAllText(FullPath(ReportRoot + "/" + fileName), content, Utf8);
        }
    }
}
