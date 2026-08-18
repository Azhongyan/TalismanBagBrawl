using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Balance
{
    public sealed class ItemBalanceValidationReport
    {
        internal ItemBalanceValidationReport(IEnumerable<string> errors, IEnumerable<string> warnings)
        {
            Errors = Array.AsReadOnly((errors ?? Array.Empty<string>()).Distinct().OrderBy(value => value).ToArray());
            Warnings = Array.AsReadOnly((warnings ?? Array.Empty<string>()).Distinct().OrderBy(value => value).ToArray());
        }

        public bool isValid => Errors.Count == 0;
        public IReadOnlyList<string> Errors { get; }
        public IReadOnlyList<string> Warnings { get; }
    }

    public static class ItemBalanceWorkbenchValidation
    {
        private static readonly string[] RequiredStats =
        {
            "damage", "break", "guard", "heal", "cleanse", "control", "duration", "nianCost", "cooldown"
        };

        public static ItemBalanceValidationReport Validate(ItemBalanceWorkbenchCatalog catalog, bool compile = true)
        {
            List<string> errors = new();
            List<string> warnings = new();
            if (catalog == null)
            {
                errors.Add("CATALOG_NULL: Workbench catalog is missing.");
                return new ItemBalanceValidationReport(errors, warnings);
            }

            if (catalog.dataMaturity != ItemBalanceWorkbenchCatalog.BalanceCandidate
                || catalog.editState != ItemBalanceWorkbenchCatalog.Editable
                || catalog.liveState != ItemBalanceWorkbenchCatalog.NotLiveLocked
                || catalog.battleState != ItemBalanceWorkbenchCatalog.NotBattleConnected)
                errors.Add("STATE_INVALID: Catalog must remain BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED.");

            ValidateDefinitions(catalog, errors);
            ValidateCompleteCandidateCatalog(catalog, errors);
            ValidateProfiles(catalog, errors, warnings);
            ValidateBuild(catalog, errors);
            ValidateDrop(catalog, errors);
            if (compile && errors.Count == 0) ValidateCompiled(catalog, errors);
            return new ItemBalanceValidationReport(errors, warnings);
        }

        private static void ValidateDefinitions(ItemBalanceWorkbenchCatalog catalog, List<string> errors)
        {
            foreach (string statId in RequiredStats)
                if (catalog.statDefinitions.Count(value => value != null && value.statId == statId) != 1)
                    errors.Add("STAT_DICTIONARY_INVALID: Required stat must exist exactly once: " + statId);
            foreach (ItemBalanceStatDefinition definition in catalog.statDefinitions.Where(value => value != null))
            {
                if (string.IsNullOrWhiteSpace(definition.statId) || definition.stepUnits <= 0)
                    errors.Add("STAT_DEFINITION_INVALID: " + definition.statId);
            }

            string[] requiredAffixes =
            {
                "affix_damage_up", "affix_break_up", "affix_guard_up", "affix_heal_up",
                "affix_cleanse_up", "affix_control_up", "affix_nian_efficiency", "affix_cooldown_reduction"
            };
            foreach (string affixId in requiredAffixes)
                if (catalog.affixDefinitions.Count(value => value != null && value.affixId == affixId) != 1)
                    errors.Add("AFFIX_DICTIONARY_INVALID: Required affix must exist exactly once: " + affixId);
        }

        private static void ValidateProfiles(
            ItemBalanceWorkbenchCatalog catalog,
            List<string> errors,
            List<string> warnings)
        {
            ItemBalanceProfile[] profiles = catalog.profiles.Where(value => value != null).ToArray();
            if (profiles.Length != 30) errors.Add("PROFILE_COUNT_INVALID: Expected 30 profiles, got " + profiles.Length);
            if (profiles.Any(value => value.baseItemId == "I031")) errors.Add("I031_FORBIDDEN: I031 cannot enter ordinary profiles.");
            foreach (IGrouping<string, ItemBalanceProfile> duplicate in profiles.GroupBy(value => value.baseItemId)
                .Where(group => group.Count() > 1))
                errors.Add("PROFILE_DUPLICATE: " + duplicate.Key);
            for (int index = 1; index <= 30; index++)
            {
                string expected = "I" + index.ToString("000");
                if (profiles.All(value => value.baseItemId != expected)) errors.Add("PROFILE_MISSING: " + expected);
            }

            int versionCount = 0;
            foreach (ItemBalanceProfile profile in profiles)
            {
                if (profile.rarityVersions.Count != 5)
                    errors.Add($"RARITY_COUNT_INVALID: {profile.baseItemId} has {profile.rarityVersions.Count} versions.");
                if (string.IsNullOrWhiteSpace(profile.primaryStatId) || string.IsNullOrWhiteSpace(profile.secondaryStatId)
                    || profile.primaryStatId == profile.secondaryStatId)
                    errors.Add("STAT_ROLE_INVALID: " + profile.baseItemId);
                if (profile.randomAffixes.Count(value => value != null && value.weight > 0) < 6)
                    errors.Add("RANDOM_POOL_INVALID: " + profile.baseItemId + " must have at least six positive entries.");
                if (profile.randomAffixes.Any(value => value != null && value.affixId == profile.fixedAffixId))
                    errors.Add("AFFIX_REPEAT_INVALID: " + profile.baseItemId + " fixed affix appears in random pool.");
                if (profile.coreCandidates.Count != 4)
                    errors.Add("CORE_CANDIDATE_COUNT_INVALID: " + profile.baseItemId);
                if (profile.signatureAffix == null || !ValidAffix(profile.signatureAffix))
                    errors.Add("SIGNATURE_AFFIX_INVALID: " + profile.baseItemId);
                if (profile.candidateDisplay == null || Empty(profile.candidateDisplay.triggerDescription)
                    || Empty(profile.candidateDisplay.basicEffectDescription)
                    || Empty(profile.candidateDisplay.placementRecommendation)
                    || Empty(profile.candidateDisplay.flavorText))
                    errors.Add("DISPLAY_PROFILE_INVALID: " + profile.baseItemId);
                ValidatePresentationIdentity(profile, errors);
                foreach (ItemBalanceCoreCandidate core in profile.coreCandidates.Where(value => value != null))
                    if (Empty(core.displayName) || Empty(core.description) || !ValidPayload(core.effectPayload))
                        errors.Add("CORE_CONTENT_INVALID: " + profile.baseItemId + "@" + core.coreEffectId);

                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    ItemBalanceRarityVersion version = profile.FindVersion(rarity.rarity);
                    if (version == null)
                    {
                        errors.Add($"VERSION_MISSING: {profile.baseItemId}@{rarity.stableKey}");
                        continue;
                    }

                    versionCount++;
                    if (version.versionKey != profile.baseItemId + "@" + rarity.stableKey)
                        errors.Add("VERSION_KEY_INVALID: " + version.versionKey);
                    if (version.dataMaturity != ItemBalanceWorkbenchCatalog.BalanceCandidate)
                        errors.Add("VERSION_MATURITY_INVALID: " + version.versionKey);
                    if (version.candidateItemPower <= 0 || Empty(version.candidateDisplaySummary))
                        errors.Add("CANDIDATE_ITEM_POWER_INVALID: " + version.versionKey);
                    string[] expectedStats = { profile.primaryStatId, profile.secondaryStatId, "nianCost", "cooldown" };
                    if (version.statRanges.Count != 4 || expectedStats.Any(id => version.FindRange(id) == null))
                        errors.Add("STAT_SET_INVALID: " + version.versionKey + " must contain exactly four expected ranges.");
                    foreach (ItemBalanceRange range in version.statRanges.Where(value => value != null))
                    {
                        ItemBalanceStatDefinition definition = catalog.FindStat(range.statId);
                        if (definition == null || range.minUnits > range.maxUnits || definition.stepUnits <= 0
                            || range.minUnits % definition.stepUnits != 0 || range.maxUnits % definition.stepUnits != 0)
                            errors.Add("STAT_RANGE_INVALID: " + version.versionKey + "@" + range.statId);
                    }

                    int expectedCoreCount = rarity.rarity switch
                    {
                        ItemInstanceRarity.White => 1,
                        ItemInstanceRarity.Green => 2,
                        ItemInstanceRarity.Blue => 3,
                        ItemInstanceRarity.Purple => 3,
                        ItemInstanceRarity.Orange => 4,
                        _ => 0
                    };
                    if (version.eligibleCoreEffectIds.Count != expectedCoreCount
                        || version.visibleCoreEffectIds.Count != expectedCoreCount)
                        errors.Add("CORE_PROFILE_INVALID: " + version.versionKey);
                    if (rarity.rarity != ItemInstanceRarity.Orange
                        && version.eligibleCoreEffectIds.Any(id => id.EndsWith("_ultimate", StringComparison.Ordinal)))
                        errors.Add("CORE_ULTIMATE_LEAK: " + version.versionKey);
                }

                ValidateMonotonic(catalog, profile, errors);
            }

            if (versionCount != 150) errors.Add("VERSION_COUNT_INVALID: Expected 150, got " + versionCount);
            if (catalog.affixDefinitions.Any(value => value != null && value.affixId == "affix_duration_up"))
                warnings.Add("AFFIX_DURATION_BRIDGE: affix_duration_up is a candidate-only definition required by duration secondary-stat pools.");
        }

        private static void ValidatePresentationIdentity(ItemBalanceProfile profile, List<string> errors)
        {
            ItemCandidateDisplayProfile presentation = profile.candidateDisplay;
            if (presentation == null) return;

            bool hasAnyIdentity = !Empty(presentation.effectFamilyKey)
                                  || !Empty(presentation.presentationStyleKey)
                                  || !Empty(presentation.cueIdentity);
            bool hasCompleteIdentity = !Empty(presentation.effectFamilyKey)
                                       && !Empty(presentation.presentationStyleKey)
                                       && !Empty(presentation.cueIdentity);
            if (presentation.presentationState == ItemPresentationConfirmationState.Unconfirmed)
            {
                if (hasAnyIdentity)
                    errors.Add("PRESENTATION_UNCONFIRMED_HAS_IDENTITY: " + profile.baseItemId);
                return;
            }

            if (!hasCompleteIdentity)
                errors.Add("PRESENTATION_IDENTITY_INCOMPLETE: " + profile.baseItemId);
        }

        private static void ValidateMonotonic(
            ItemBalanceWorkbenchCatalog catalog,
            ItemBalanceProfile profile,
            List<string> errors)
        {
            string[] statIds = { profile.primaryStatId, profile.secondaryStatId, "nianCost", "cooldown" };
            foreach (string statId in statIds)
            {
                ItemBalanceStatDefinition definition = catalog.FindStat(statId);
                ItemBalanceRange previous = null;
                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    ItemBalanceRange current = profile.FindVersion(rarity.rarity)?.FindRange(statId);
                    if (current == null || previous == null) { previous = current; continue; }
                    bool invalid = definition?.direction == ItemStatDirection.HigherIsBetter
                        ? current.minUnits < previous.minUnits || current.maxUnits < previous.maxUnits
                        : definition?.direction == ItemStatDirection.LowerIsBetter
                            && (current.minUnits > previous.minUnits || current.maxUnits > previous.maxUnits);
                    if (invalid) errors.Add("STAT_DIRECTION_INVALID: " + profile.baseItemId + "@" + statId);
                    previous = current;
                }
            }
        }

        private static void ValidateBuild(ItemBalanceWorkbenchCatalog catalog, List<string> errors)
        {
            if (catalog.buildPolicies.Count(value => value != null) != 5)
                errors.Add("BUILD_POLICY_COUNT_INVALID: Expected five policies.");
            foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
            {
                ItemBalanceBuildPolicy policy = catalog.FindBuildPolicy(rarity.rarity);
                if (policy == null || policy.TotalPositiveWeight <= 0)
                    errors.Add("BUILD_POLICY_INVALID: " + rarity.stableKey);
                if (rarity.rarity == ItemInstanceRarity.White && policy != null
                    && (policy.noneWeight != 100 || policy.faMenWeight != 0 || policy.qiLeiWeight != 0 || policy.dualWeight != 0))
                    errors.Add("WHITE_BUILD_POLICY_INVALID: White must remain None 100.");
            }
        }

        private static void ValidateCompleteCandidateCatalog(ItemBalanceWorkbenchCatalog catalog,
            List<string> errors)
        {
            if (!string.Equals(catalog.balanceDataRevision, ItemBalanceWorkbenchCatalog.CompleteCandidateRevision,
                StringComparison.Ordinal))
                errors.Add("BALANCE_DATA_REVISION_INVALID");
            if (catalog.candidateRandomAffixes.Count(value => value != null) < 36)
                errors.Add("CANDIDATE_RANDOM_AFFIX_COUNT_INVALID");
            foreach (ItemCandidateAffixDefinition affix in catalog.candidateRandomAffixes.Where(value => value != null))
                if (!ValidAffix(affix)) errors.Add("CANDIDATE_RANDOM_AFFIX_INVALID: " + affix.affixId);
            if (catalog.faMenBuildEffects.Count(value => value != null) != 15)
                errors.Add("FAMEN_BUILD_EFFECT_COUNT_INVALID");
            if (catalog.qiLeiBuildEffects.Count(value => value != null) != 10)
                errors.Add("QILEI_BUILD_EFFECT_COUNT_INVALID");
            foreach (ItemCandidateBuildStageDefinition stage in catalog.faMenBuildEffects
                .Concat(catalog.qiLeiBuildEffects).Where(value => value != null))
                if (Empty(stage.buildId) || Empty(stage.displayName) || Empty(stage.description)
                    || Empty(stage.conditionText) || !ValidPayload(stage.effectPayload))
                    errors.Add("BUILD_EFFECT_INVALID: " + stage.buildId);
        }

        private static bool ValidAffix(ItemCandidateAffixDefinition value)
        {
            return value != null && !Empty(value.affixId) && !Empty(value.displayName)
                && !Empty(value.description) && ValidPayload(value.effectPayload)
                && value.rarityRanges?.Count(range => range != null) == 5;
        }

        private static bool ValidPayload(ItemCandidateEffectPayload value)
        {
            return value != null && !Empty(value.effectId) && !Empty(value.displayName)
                && !Empty(value.description) && !Empty(value.triggerEventId)
                && !Empty(value.conditionId) && !Empty(value.targetSelector)
                && !Empty(value.targetStatId) && !Empty(value.valueUnitKey)
                && value.valueUnits != 0 && !Empty(value.parameterProfileId)
                && !Empty(value.mutexGroupId) && !Empty(value.dataMaturity)
                && !Empty(value.designNote);
        }

        private static bool Empty(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            string[] forbidden = { "尚未配置", "数据待定", "Preview Reserved", "Reserved", "TODO", "临时占位" };
            return forbidden.Any(token => value.Contains(token, StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidateDrop(ItemBalanceWorkbenchCatalog catalog, List<string> errors)
        {
            if (catalog.dropSegments.Count(value => value != null) != 5)
                errors.Add("DROP_SEGMENT_COUNT_INVALID: Expected five segments.");
            ItemBalanceDropSegment first = catalog.dropSegments.FirstOrDefault(value => value != null);
            if (first == null || first.minStage != 1 || first.maxStage != 10 || first.whiteWeight != 100
                || first.greenWeight != 0 || first.blueWeight != 0 || first.purpleWeight != 0 || first.orangeWeight != 0)
                errors.Add("DROP_WHITE_LOCK_INVALID: Stage 1-10 must remain white 100.");
        }

        private static void ValidateCompiled(ItemBalanceWorkbenchCatalog catalog, List<string> errors)
        {
            try
            {
                ItemBalanceCompiledData compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
                if (!compiled.Foundation.isValid) errors.AddRange(compiled.Foundation.ValidationErrors.Select(value => "FOUNDATION: " + value.code));
                if (!compiled.StatSchema.isValid) errors.AddRange(compiled.StatSchema.ValidationErrors.Select(value => "STAT_SCHEMA: " + value.code));
                if (!compiled.AffixSchema.isValid) errors.AddRange(compiled.AffixSchema.ValidationErrors.Select(value => "AFFIX_SCHEMA: " + value.code));
                if (!compiled.CoreBuildSchema.isValid) errors.AddRange(compiled.CoreBuildSchema.ValidationErrors.Select(value => "CORE_SCHEMA: " + value.code));
                if (compiled.BuildProfiles.Count != 4) errors.Add("BUILD_PROFILE_COMPILE_COUNT_INVALID");
                if (compiled.DropProfiles.Count != 5) errors.Add("DROP_PROFILE_COMPILE_COUNT_INVALID");
            }
            catch (Exception exception)
            {
                errors.Add("COMPILE_EXCEPTION: " + exception.GetType().Name + " " + exception.Message);
            }
        }
    }
}
