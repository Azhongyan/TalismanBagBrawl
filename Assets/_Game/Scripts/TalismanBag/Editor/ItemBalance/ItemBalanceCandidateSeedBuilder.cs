using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Stats;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemBalance
{
    public static class ItemBalanceCandidateSeedBuilder
    {
        public const string AssetRoot = "Assets/_Game/Configs/ItemBalanceWorkbench";
        public const string ProfileRoot = AssetRoot + "/Profiles";
        public const string CatalogPath = AssetRoot + "/ItemBalanceWorkbenchCatalog.asset";

        private static readonly (string primary, string secondary)[] StatRoles =
        {
            ("damage", "break"), ("damage", "control"), ("break", "damage"),
            ("control", "damage"), ("break", "control"), ("damage", "control"),
            ("damage", "duration"), ("damage", "duration"), ("damage", "duration"),
            ("damage", "duration"), ("damage", "duration"), ("damage", "duration"),
            ("guard", "duration"), ("guard", "duration"), ("guard", "control"),
            ("guard", "duration"), ("guard", "control"), ("guard", "duration"),
            ("cleanse", "heal"), ("cleanse", "duration"), ("cleanse", "guard"),
            ("heal", "cleanse"), ("cleanse", "control"), ("cleanse", "heal"),
            ("control", "duration"), ("damage", "control"), ("break", "control"),
            ("control", "break"), ("control", "break"), ("damage", "control")
        };

        private static readonly long[,] Anchors =
        {
            { 8, 24, 2, 5, 3, 6 }, { 12, 34, 3, 6, 3, 6 },
            { 18, 48, 4, 7, 4, 7 }, { 14, 38, 3, 6, 2, 5 },
            { 16, 44, 3, 7, 3, 6 }, { 24, 64, 5, 9, 5, 8 }
        };

        [MenuItem("Tools/Talisman Bag/V0.4/Data/Item Balance Workbench/[Setup] Create Missing Candidate Assets")]
        public static void CreateMissingCandidateAssets()
        {
            BuildAssets(false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/Data/Item Balance Workbench/[Setup] Rebuild All Candidate Assets")]
        private static void RebuildAllCandidateAssets()
        {
            if (!EditorUtility.DisplayDialog("Rebuild candidate assets?",
                    "This overwrites all 30 editable profiles with the candidate seed. This action supports Undo only for loaded objects; use version control for a full rollback.",
                    "Rebuild", "Cancel")) return;
            BuildAssets(true);
        }

        public static void BuildCandidateAssetsBatch()
        {
            BuildAssets(false);
        }

        public static ItemBalanceWorkbenchCatalog BuildAssets(bool overwriteProfiles)
        {
            EnsureFolder(AssetRoot);
            EnsureFolder(ProfileRoot);
            WriteBackupManifest();
            ItemBalanceWorkbenchCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ItemBalanceWorkbenchCatalog>();
                InitializeCatalog(catalog);
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            else if (catalog.statDefinitions == null || catalog.statDefinitions.Count == 0)
            {
                Undo.RecordObject(catalog, "Initialize Item Balance Catalog");
                InitializeCatalog(catalog);
            }
            EnsureCompleteCatalog(catalog);

            List<ItemBalanceProfile> profiles = new();
            for (int index = 1; index <= 30; index++)
            {
                string itemId = "I" + index.ToString("000", CultureInfo.InvariantCulture);
                string path = ProfileRoot + "/ItemBalanceProfile_" + itemId + ".asset";
                ItemBalanceProfile profile = AssetDatabase.LoadAssetAtPath<ItemBalanceProfile>(path);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<ItemBalanceProfile>();
                    SeedProfile(catalog, profile, index);
                    AssetDatabase.CreateAsset(profile, path);
                }
                else if (overwriteProfiles)
                {
                    Undo.RecordObject(profile, "Reset Item Balance Candidate Seed");
                    SeedProfile(catalog, profile, index);
                }

                EnsureCompleteProfile(catalog, profile);

                EditorUtility.SetDirty(profile);
                profiles.Add(profile);
            }

            Undo.RecordObject(catalog, "Update Item Balance Profile Catalog");
            catalog.profiles = profiles.OrderBy(value => value.baseItemId, StringComparer.Ordinal).ToList();
            catalog.balanceDataRevision = ItemCompleteCandidateContentSeed.Revision;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ItemBalanceWorkbench] Candidate assets ready: profiles={profiles.Count}, versions={profiles.Sum(p => p.rarityVersions.Count)}.");
            return catalog;
        }

        public static void ResetSelectedProfile(ItemBalanceWorkbenchCatalog catalog, ItemBalanceProfile profile)
        {
            if (catalog == null || profile == null) return;
            int index = int.Parse(profile.baseItemId.Substring(1), CultureInfo.InvariantCulture);
            Undo.RecordObject(profile, "Reset Selected Item Candidate Seed");
            SeedProfile(catalog, profile, index);
            EditorUtility.SetDirty(profile);
        }

        public static void GenerateSuggestedBands(ItemBalanceWorkbenchCatalog catalog, ItemBalanceProfile profile)
        {
            if (catalog == null || profile == null) return;
            Undo.RecordObject(profile, "Generate Suggested Item Bands");
            string[] stats = { profile.primaryStatId, profile.secondaryStatId, "nianCost", "cooldown" };
            Dictionary<string, (long min, long max)> totals = stats.ToDictionary(stat => stat,
                stat => GetTotalRange(profile.qiLeiPosition, stat,
                    stat == profile.secondaryStatId && stat != profile.primaryStatId));
            foreach (ItemBalanceRarityVersion version in profile.rarityVersions)
            {
                foreach (string stat in stats)
                {
                    ItemBalanceStatDefinition definition = catalog.FindStat(stat);
                    ItemBalanceBandSlice band = catalog.FindBand(definition.direction, version.rarity);
                    ItemBalanceRange range = version.FindRange(stat);
                    if (range == null)
                    {
                        range = new ItemBalanceRange { statId = stat };
                        version.statRanges.Add(range);
                    }

                    ApplyBand(range, totals[stat], band, Math.Max(1L, definition.stepUnits));
                }
                version.statRanges = version.statRanges.OrderBy(value => Array.IndexOf(stats, value.statId)).ToList();
            }
            EditorUtility.SetDirty(profile);
        }

        private static void InitializeCatalog(ItemBalanceWorkbenchCatalog catalog)
        {
            catalog.balanceDataRevision = ItemCompleteCandidateContentSeed.Revision;
            catalog.dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
            catalog.editState = ItemBalanceWorkbenchCatalog.Editable;
            catalog.liveState = ItemBalanceWorkbenchCatalog.NotLiveLocked;
            catalog.battleState = ItemBalanceWorkbenchCatalog.NotBattleConnected;
            catalog.statDefinitions = new List<ItemBalanceStatDefinition>
            {
                Stat("damage", "伤害", "flat", ItemStatDirection.HigherIsBetter),
                Stat("break", "破盾", "flat", ItemStatDirection.HigherIsBetter),
                Stat("guard", "护势", "flat", ItemStatDirection.HigherIsBetter),
                Stat("heal", "回复", "flat", ItemStatDirection.HigherIsBetter),
                Stat("cleanse", "净化", "stack", ItemStatDirection.HigherIsBetter),
                Stat("control", "控制", "point", ItemStatDirection.HigherIsBetter),
                Stat("duration", "持续", "turn", ItemStatDirection.HigherIsBetter),
                Stat("nianCost", "耗念", "point", ItemStatDirection.LowerIsBetter),
                Stat("cooldown", "冷却", "turn", ItemStatDirection.LowerIsBetter)
            };
            catalog.higherBandTemplate = Bands(new[] { 0f, .20f, .45f, .65f, .85f },
                new[] { .25f, .50f, .75f, .90f, 1f });
            catalog.lowerBandTemplate = Bands(new[] { .75f, .50f, .25f, .10f, 0f },
                new[] { 1f, .80f, .55f, .35f, .20f });
            catalog.affixDefinitions = BuildAffixes();
            catalog.buildPolicies = new List<ItemBalanceBuildPolicy>
            {
                Build(ItemInstanceRarity.White, 100, 0, 0, 0),
                Build(ItemInstanceRarity.Green, 70, 15, 15, 0),
                Build(ItemInstanceRarity.Blue, 45, 25, 25, 5),
                Build(ItemInstanceRarity.Purple, 20, 30, 30, 20),
                Build(ItemInstanceRarity.Orange, 5, 20, 20, 55)
            };
            catalog.dropSegments = new List<ItemBalanceDropSegment>
            {
                Drop(1, 10, false, 100, 0, 0, 0, 0),
                Drop(11, 20, false, 70, 25, 5, 0, 0),
                Drop(21, 30, false, 45, 35, 17, 3, 0),
                Drop(31, 40, false, 25, 35, 28, 10, 2),
                Drop(41, 0, true, 10, 25, 35, 22, 8)
            };
            catalog.previewCoefficients = new List<ItemBalancePreviewCoefficient>
            {
                Coef("BreakPower", "break", 1f), Coef("CleansePower", "cleanse", 1f),
                Coef("ControlPower", "control", 1f), Coef("GuardPower", "guard", 1f),
                Coef("GuardPower", "heal", .4f), Coef("EnergyStability", "nianCost", -1f),
                Coef("ClearPower", "damage", .6f), Coef("ClearPower", "duration", .4f),
                Coef("BurstWindow", "damage", .5f), Coef("BurstWindow", "control", .5f),
                Coef("BurstWindow", "cooldown", -.3f)
            };
            catalog.candidateItemPowerCoefficients = ItemCompleteCandidateContentSeed.DefaultPowerCoefficients();
            catalog.candidateRandomAffixes = ItemCompleteCandidateContentSeed.BuildRandomAffixDictionary();
            catalog.faMenBuildEffects = ItemCompleteCandidateContentSeed.BuildFaMenStages();
            catalog.qiLeiBuildEffects = ItemCompleteCandidateContentSeed.BuildQiLeiStages();
        }

        private static void SeedProfile(ItemBalanceWorkbenchCatalog catalog, ItemBalanceProfile profile, int index)
        {
            ItemInnerDataDefinition source = ItemInnerDataCatalog.FindById("I" + index.ToString("000"));
            (string primary, string secondary) roles = StatRoles[index - 1];
            profile.baseItemId = source.itemId;
            profile.displayName = source.displayName;
            profile.faMenTag = source.FaMenKey;
            profile.qiLeiTag = source.QiLeiKey;
            profile.qiLeiPosition = ((index - 1) % 6) + 1;
            profile.primaryStatId = roles.primary;
            profile.secondaryStatId = roles.secondary;
            profile.fixedAffixId = AffixForStat(roles.primary);
            profile.randomPoolId = "candidate_pool_" + source.itemId.ToLowerInvariant();
            profile.randomAffixes = new List<ItemBalanceWeightedAffix>
            {
                new() { affixId = AffixForStat(roles.secondary), weight = 4 },
                new() { affixId = "affix_nian_efficiency", weight = 3 },
                new() { affixId = "affix_cooldown_reduction", weight = 3 }
            };
            profile.coreCandidates = ItemCompleteCandidateContentSeed.BuildCoreCandidates(profile);
            profile.signatureAffix = ItemCompleteCandidateContentSeed.BuildSignatureAffix(profile);
            profile.candidateDisplay = ItemCompleteCandidateContentSeed.BuildDisplayProfile(profile);
            profile.balanceDataRevision = ItemCompleteCandidateContentSeed.Revision;
            profile.rarityVersions = new List<ItemBalanceRarityVersion>();
            foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
            {
                int count = rarity.tierIndex + 1;
                string[] coreIds = profile.coreCandidates.Take(count).Select(value => value.coreEffectId).ToArray();
                profile.rarityVersions.Add(new ItemBalanceRarityVersion
                {
                    rarity = rarity.rarity,
                    versionKey = profile.baseItemId + "@" + rarity.stableKey,
                    cultivationPotentialProfileId = "candidate_core_profile_" + profile.baseItemId.ToLowerInvariant() + "_" + rarity.stableKey,
                    dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate,
                    eligibleCoreEffectIds = coreIds.ToList(),
                    visibleCoreEffectIds = coreIds.ToList(),
                    statRanges = new List<ItemBalanceRange>()
                });
            }
            GenerateSuggestedBands(catalog, profile);
            EnsureCompleteProfile(catalog, profile);
        }

        public static void EnsureCompleteCandidateContentBatch()
        {
            BuildAssets(false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/Data/Item Balance Workbench/[Setup] Additive Complete Candidate Content")]
        public static void EnsureCompleteCandidateContentMenu()
        {
            ItemBalanceWorkbenchCatalog catalog = BuildAssets(false);
            Debug.Log($"[ItemCompleteCandidateContent] additive migration complete: revision={catalog.balanceDataRevision}, profiles={catalog.profiles.Count}.");
        }

        private static void EnsureCompleteCatalog(ItemBalanceWorkbenchCatalog catalog)
        {
            if (catalog == null) return;
            catalog.balanceDataRevision = ItemCompleteCandidateContentSeed.Revision;
            catalog.candidateItemPowerCoefficients ??= new List<ItemCandidateItemPowerCoefficient>();
            MergeById(catalog.candidateItemPowerCoefficients,
                ItemCompleteCandidateContentSeed.DefaultPowerCoefficients(), value => value.coefficientId);
            catalog.candidateRandomAffixes ??= new List<ItemCandidateAffixDefinition>();
            MergeById(catalog.candidateRandomAffixes,
                ItemCompleteCandidateContentSeed.BuildRandomAffixDictionary(), value => value.affixId);
            catalog.affixDefinitions ??= new List<ItemBalanceAffixDefinition>();
            foreach (ItemCandidateAffixDefinition candidate in catalog.candidateRandomAffixes.Where(value => value != null))
            {
                if (catalog.FindAffix(candidate.affixId) != null) continue;
                catalog.affixDefinitions.Add(new ItemBalanceAffixDefinition
                {
                    affixId = candidate.affixId,
                    displayName = candidate.displayName,
                    unitKey = candidate.effectPayload?.valueUnitKey ?? "flat",
                    direction = ItemStatDirection.HigherIsBetter,
                    decimalPlaces = 0,
                    stepUnits = 1,
                    roundingMode = ItemStatRoundingMode.Nearest,
                    rarityRanges = (candidate.rarityRanges ?? new List<ItemCandidateRarityValue>())
                        .Where(value => value != null)
                        .Select(value => new ItemBalanceAffixRarityRange
                        {
                            rarity = value.rarity,
                            minUnits = Math.Min(value.minUnits, value.maxUnits),
                            maxUnits = Math.Max(value.minUnits, value.maxUnits)
                        }).ToList()
                });
            }
            catalog.faMenBuildEffects ??= new List<ItemCandidateBuildStageDefinition>();
            MergeById(catalog.faMenBuildEffects,
                ItemCompleteCandidateContentSeed.BuildFaMenStages(), value => value.buildId);
            catalog.qiLeiBuildEffects ??= new List<ItemCandidateBuildStageDefinition>();
            MergeById(catalog.qiLeiBuildEffects,
                ItemCompleteCandidateContentSeed.BuildQiLeiStages(), value => value.buildId);
            EditorUtility.SetDirty(catalog);
        }

        private static void EnsureCompleteProfile(ItemBalanceWorkbenchCatalog catalog, ItemBalanceProfile profile)
        {
            if (catalog == null || profile == null || string.IsNullOrWhiteSpace(profile.baseItemId)) return;
            profile.balanceDataRevision = ItemCompleteCandidateContentSeed.Revision;
            if (profile.signatureAffix == null || string.IsNullOrWhiteSpace(profile.signatureAffix.affixId))
                profile.signatureAffix = ItemCompleteCandidateContentSeed.BuildSignatureAffix(profile);
            if (profile.candidateDisplay == null || string.IsNullOrWhiteSpace(profile.candidateDisplay.triggerDescription))
                profile.candidateDisplay = ItemCompleteCandidateContentSeed.BuildDisplayProfile(profile);

            List<ItemBalanceCoreCandidate> seededCores = ItemCompleteCandidateContentSeed.BuildCoreCandidates(profile);
            profile.coreCandidates ??= new List<ItemBalanceCoreCandidate>();
            foreach (ItemBalanceCoreCandidate seed in seededCores)
            {
                ItemBalanceCoreCandidate target = profile.coreCandidates.FirstOrDefault(value => value != null
                    && string.Equals(value.coreEffectId, seed.coreEffectId, StringComparison.Ordinal));
                if (target == null)
                {
                    profile.coreCandidates.Add(seed);
                    continue;
                }

                bool legacy = IsLegacyPlaceholder(target);
                if (legacy || string.IsNullOrWhiteSpace(target.displayName)) target.displayName = seed.displayName;
                if (legacy || string.IsNullOrWhiteSpace(target.description)) target.description = seed.description;
                target.isUltimate = seed.isUltimate;
                if (string.IsNullOrWhiteSpace(target.nodeKind)) target.nodeKind = seed.nodeKind;
                if (target.unlockLevel <= 0) target.unlockLevel = seed.unlockLevel;
                target.requiredRarity = seed.requiredRarity;
                if (string.IsNullOrWhiteSpace(target.effectType)) target.effectType = seed.effectType;
                if (target.effectPayload == null || string.IsNullOrWhiteSpace(target.effectPayload.effectId))
                    target.effectPayload = seed.effectPayload;
                if (string.IsNullOrWhiteSpace(target.stateDescription)) target.stateDescription = seed.stateDescription;
                if (string.IsNullOrWhiteSpace(target.dataMaturity)) target.dataMaturity = seed.dataMaturity;
                if (string.IsNullOrWhiteSpace(target.designNote)) target.designNote = seed.designNote;
            }
            profile.coreCandidates = profile.coreCandidates.Where(value => value != null)
                .OrderBy(value => value.isUltimate ? 4 : Math.Max(0, value.unlockLevel / 10 - 1)).Take(5).ToList();

            List<ItemBalanceWeightedAffix> seededPool = ItemCompleteCandidateContentSeed.BuildRandomPool(
                profile, catalog.candidateRandomAffixes);
            profile.randomAffixes ??= new List<ItemBalanceWeightedAffix>();
            foreach (ItemBalanceWeightedAffix seed in seededPool)
            {
                ItemBalanceWeightedAffix target = profile.randomAffixes.FirstOrDefault(value => value != null
                    && string.Equals(value.affixId, seed.affixId, StringComparison.Ordinal));
                if (target == null) profile.randomAffixes.Add(seed);
                else
                {
                    if (target.weight <= 0) target.weight = seed.weight;
                    if (string.IsNullOrWhiteSpace(target.mutexGroupId)) target.mutexGroupId = seed.mutexGroupId;
                    if (string.IsNullOrWhiteSpace(target.repeatPolicy)) target.repeatPolicy = seed.repeatPolicy;
                    if (string.IsNullOrWhiteSpace(target.designNote)) target.designNote = seed.designNote;
                }
            }
            profile.randomAffixes = profile.randomAffixes.Where(value => value != null)
                .GroupBy(value => value.affixId, StringComparer.Ordinal).Select(group => group.First()).ToList();

            profile.rarityVersions ??= new List<ItemBalanceRarityVersion>();
            foreach (ItemBalanceRarityVersion version in profile.rarityVersions.Where(value => value != null))
            {
                int visibleCount = version.rarity.ToTierIndex() + 1;
                string[] coreIds = profile.coreCandidates.Take(visibleCount).Select(value => value.coreEffectId).ToArray();
                version.eligibleCoreEffectIds ??= new List<string>();
                version.visibleCoreEffectIds ??= new List<string>();
                foreach (string id in coreIds)
                {
                    if (!version.eligibleCoreEffectIds.Contains(id)) version.eligibleCoreEffectIds.Add(id);
                    if (!version.visibleCoreEffectIds.Contains(id)) version.visibleCoreEffectIds.Add(id);
                }
                version.eligibleCoreEffectIds = version.eligibleCoreEffectIds.Where(coreIds.Contains).ToList();
                version.visibleCoreEffectIds = version.visibleCoreEffectIds.Where(coreIds.Contains).ToList();
                if (!version.candidateItemPowerOverridden && version.candidateItemPower <= 0)
                    version.candidateItemPower = ItemCompleteCandidateContentSeed.CalculateCandidateItemPower(
                        profile, version, catalog.candidateItemPowerCoefficients);
                if (string.IsNullOrWhiteSpace(version.candidateDisplaySummary))
                    version.candidateDisplaySummary = profile.displayName + " · " + version.rarity.ToDisplayName()
                        + " · 物品强度（候选）" + version.candidateItemPower.ToString(CultureInfo.InvariantCulture);
            }
            EditorUtility.SetDirty(profile);
        }

        private static bool IsLegacyPlaceholder(ItemBalanceCoreCandidate value)
        {
            if (value == null) return true;
            string[] legacyNames = { "主属性潜力", "副属性潜力", "器类节奏潜力", "法门协同潜力", "终极效果潜力" };
            return legacyNames.Contains(value.displayName)
                || (value.description ?? string.Empty).Contains("尚未接入正式战斗行为", StringComparison.Ordinal)
                || (value.description ?? string.Empty).Contains("尚未实现、解锁或激活", StringComparison.Ordinal);
        }

        private static void MergeById<T>(List<T> target, IEnumerable<T> seeds, Func<T, string> id)
        {
            foreach (T seed in seeds ?? Array.Empty<T>())
            {
                string key = id(seed);
                if (!target.Any(value => value != null && string.Equals(id(value), key, StringComparison.Ordinal)))
                    target.Add(seed);
            }
        }

        private static void WriteBackupManifest()
        {
            string reportRoot = Path.GetFullPath("Docs/V0.4/Reports");
            Directory.CreateDirectory(reportRoot);
            List<string> paths = new();
            if (File.Exists(Path.GetFullPath(CatalogPath))) paths.Add(Path.GetFullPath(CatalogPath));
            string profileRoot = Path.GetFullPath(ProfileRoot);
            if (Directory.Exists(profileRoot)) paths.AddRange(Directory.GetFiles(profileRoot, "*.asset"));
            StringBuilder builder = new();
            builder.AppendLine("path,sha256,length");
            foreach (string path in paths.OrderBy(value => value, StringComparer.Ordinal))
            {
                byte[] bytes = File.ReadAllBytes(path);
                using SHA256 sha = SHA256.Create();
                string hash = string.Concat(sha.ComputeHash(bytes).Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
                builder.Append('"').Append(path.Replace("\"", "\"\"")).Append("\",")
                    .Append(hash).Append(',').Append(bytes.Length.ToString(CultureInfo.InvariantCulture)).AppendLine();
            }
            File.WriteAllText(Path.Combine(reportRoot, "ItemCompleteCandidateContentBackupManifest.csv"), builder.ToString(), new UTF8Encoding(false));
        }

        private static List<ItemBalanceCoreCandidate> BuildCoreCandidates(ItemBalanceProfile profile)
        {
            string stem = "candidate_core_" + profile.baseItemId.ToLowerInvariant();
            string[] names = { "主属性潜力", "副属性潜力", "器类节奏潜力", "法门协同潜力", "终极效果潜力" };
            string[] descriptions =
            {
                $"候选：强化 {profile.primaryStatId} 方向；尚未接入正式战斗行为。",
                $"候选：强化 {profile.secondaryStatId} 方向；尚未接入正式战斗行为。",
                $"候选：强化 {profile.qiLeiTag} 器类节奏；尚未接入正式战斗行为。",
                $"候选：强化 {profile.faMenTag} 法门协同；尚未接入正式战斗行为。",
                "候选：保留道品终极效果潜力；尚未实现、解锁或激活。"
            };
            List<ItemBalanceCoreCandidate> result = new();
            for (int i = 0; i < 5; i++)
                result.Add(new ItemBalanceCoreCandidate
                {
                    coreEffectId = i == 4 ? stem + "_ultimate" : stem + "_0" + (i + 1),
                    displayName = names[i], description = descriptions[i], isUltimate = i == 4
                });
            return result;
        }

        private static (long min, long max) GetTotalRange(int position, string statId, bool secondary)
        {
            if (statId == "cleanse") return (1, 5);
            if (statId == "control") return (1, 10);
            if (statId == "duration") return (1, 6);
            int row = Math.Max(0, Math.Min(5, position - 1));
            if (statId == "nianCost") return (Anchors[row, 2], Anchors[row, 3]);
            if (statId == "cooldown") return (Anchors[row, 4], Anchors[row, 5]);
            double scale = statId == "break" ? .90 : statId == "guard" ? 1.15 : statId == "heal" ? .75 : 1.0;
            if (secondary) scale *= .70;
            long min = Math.Max(1, Round(Anchors[row, 0] * scale));
            long max = Math.Max(min, Round(Anchors[row, 1] * scale));
            return (min, max);
        }

        private static void ApplyBand(ItemBalanceRange target, (long min, long max) total,
            ItemBalanceBandSlice band, long step)
        {
            double min01 = band?.min01 ?? 0d;
            double max01 = band?.max01 ?? 1d;
            long span = total.max - total.min;
            target.minUnits = Align(total.min + Round(span * min01), step);
            target.maxUnits = Align(total.min + Round(span * max01), step);
            target.minUnits = Math.Max(total.min, Math.Min(total.max, target.minUnits));
            target.maxUnits = Math.Max(target.minUnits, Math.Min(total.max, target.maxUnits));
        }

        private static List<ItemBalanceAffixDefinition> BuildAffixes()
        {
            List<ItemBalanceAffixDefinition> result = new();
            result.Add(PercentAffix("affix_damage_up", "伤害提升"));
            result.Add(PercentAffix("affix_break_up", "破盾提升"));
            result.Add(PercentAffix("affix_guard_up", "护势提升"));
            result.Add(PercentAffix("affix_heal_up", "回复提升"));
            result.Add(DiscreteAffix("affix_cleanse_up", "净化提升", "stack", 1, 5));
            result.Add(DiscreteAffix("affix_control_up", "控制提升", "point", 1, 10));
            result.Add(DiscreteAffix("affix_duration_up", "持续提升", "turn", 1, 6));
            result.Add(PercentAffix("affix_nian_efficiency", "耗念效率"));
            result.Add(PercentAffix("affix_cooldown_reduction", "冷却缩减"));
            return result;
        }

        private static ItemBalanceAffixDefinition PercentAffix(string id, string name)
        {
            long[,] values = { { 200, 400 }, { 350, 650 }, { 550, 900 }, { 800, 1300 }, { 1200, 1800 } };
            return Affix(id, name, "basisPoint", values);
        }

        private static ItemBalanceAffixDefinition DiscreteAffix(string id, string name, string unit, long min, long max)
        {
            long[,] values = new long[5, 2];
            float[] a = { 0f, .20f, .45f, .65f, .85f };
            float[] b = { .25f, .50f, .75f, .90f, 1f };
            for (int i = 0; i < 5; i++)
            {
                values[i, 0] = min + Round((max - min) * a[i]);
                values[i, 1] = Math.Max(values[i, 0], min + Round((max - min) * b[i]));
            }
            return Affix(id, name, unit, values);
        }

        private static ItemBalanceAffixDefinition Affix(string id, string name, string unit, long[,] values)
        {
            ItemBalanceAffixDefinition result = new()
            {
                affixId = id, displayName = name, unitKey = unit,
                direction = ItemStatDirection.HigherIsBetter, stepUnits = 1,
                roundingMode = ItemStatRoundingMode.Nearest
            };
            for (int i = 0; i < 5; i++) result.rarityRanges.Add(new ItemBalanceAffixRarityRange
            { rarity = ItemInstanceRarityCatalog.All[i].rarity, minUnits = values[i, 0], maxUnits = values[i, 1] });
            return result;
        }

        private static string AffixForStat(string statId)
        {
            return statId switch
            {
                "damage" => "affix_damage_up", "break" => "affix_break_up",
                "guard" => "affix_guard_up", "heal" => "affix_heal_up",
                "cleanse" => "affix_cleanse_up", "control" => "affix_control_up",
                "duration" => "affix_duration_up", "nianCost" => "affix_nian_efficiency",
                "cooldown" => "affix_cooldown_reduction", _ => string.Empty
            };
        }

        private static ItemBalanceStatDefinition Stat(string id, string name, string unit, ItemStatDirection direction) =>
            new() { statId = id, displayName = name, unitKey = unit, direction = direction, stepUnits = 1 };
        private static List<ItemBalanceBandSlice> Bands(float[] min, float[] max) =>
            ItemInstanceRarityCatalog.All.Select((rarity, index) => new ItemBalanceBandSlice
            { rarity = rarity.rarity, min01 = min[index], max01 = max[index] }).ToList();
        private static ItemBalanceBuildPolicy Build(ItemInstanceRarity rarity, long none, long fa, long qi, long dual) =>
            new() { rarity = rarity, noneWeight = none, faMenWeight = fa, qiLeiWeight = qi, dualWeight = dual };
        private static ItemBalanceDropSegment Drop(int min, int max, bool open, long white, long green, long blue, long purple, long orange) =>
            new() { minStage = min, maxStage = max, openEnded = open, whiteWeight = white, greenWeight = green, blueWeight = blue, purpleWeight = purple, orangeWeight = orange };
        private static ItemBalancePreviewCoefficient Coef(string dimension, string stat, float coefficient) =>
            new() { dimensionId = dimension, statId = stat, coefficient = coefficient };
        private static long Round(double value) => (long)Math.Round(value, MidpointRounding.AwayFromZero);
        private static long Align(long value, long step) => step <= 1 ? value : Round((double)value / step) * step;

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }
    }
}
