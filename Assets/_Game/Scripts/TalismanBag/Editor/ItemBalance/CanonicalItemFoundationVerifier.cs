using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.V04.RewardDrop.Runtime.CampaignLoot;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.ItemBalance
{
    public static class CanonicalItemFoundationVerifier
    {
        private const string CatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string PresentationCatalogPath =
            "Assets/_Game/Resources/V04/ItemPresentation/C1FormalItemPresentationCatalog.asset";

        [MenuItem("Tools/Talisman Bag/V0.4/Verify Canonical Item Foundation")]
        public static void Verify()
        {
            List<string> errors = Run();
            if (errors.Count > 0)
            {
                Debug.LogError("[CanonicalItemFoundation] FAIL " + string.Join(" | ", errors));
                return;
            }

            Debug.Log("[CanonicalItemFoundation] PASS catalog=31 ordinaryDrop=30 "
                + "combatRules=30 rarityVersions=150 I001=ordinary I031=special "
                + "instanceRoll=PASS dropRoll=PASS deterministic=PASS");
        }

        public static List<string> Run()
        {
            List<string> errors = new();
            ItemBalanceWorkbenchCatalog source =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(CatalogPath);
            if (!CanonicalItemDefinitionResolver.TryCreate(source, out CanonicalItemDefinitionResolver resolver,
                    out IReadOnlyList<string> catalogErrors))
            {
                errors.AddRange(catalogErrors);
                return errors;
            }

            CanonicalItemDefinition[] definitions = Enumerable.Range(1, 31)
                .Select(index => resolver.GetDefinition("I" + index.ToString("000")))
                .Where(value => value != null)
                .ToArray();
            if (definitions.Length != 31) errors.Add("catalog count is not 31");
            if (definitions.Count(value => value.isOrdinaryDropEligible) != 30)
                errors.Add("ordinary count is not 30");
            if (resolver.GetDefinition("I001")?.isOrdinaryDropEligible != true)
                errors.Add("I001 is not an ordinary drop definition");
            if (resolver.GetDefinition("I031")?.isOrdinaryDropEligible != false)
                errors.Add("I031 entered the ordinary drop set");
            foreach (CanonicalItemDefinition definition in definitions.Where(value =>
                         value.isOrdinaryDropEligible))
            {
                if (definition.combatEffect == null
                    || string.IsNullOrWhiteSpace(definition.combatEffect.effectId)
                    || string.IsNullOrWhiteSpace(definition.combatEffect.triggerEventId)
                    || string.IsNullOrWhiteSpace(definition.combatEffect.conditionId)
                    || string.IsNullOrWhiteSpace(definition.combatEffect.targetSelector)
                    || string.IsNullOrWhiteSpace(definition.combatEffect.targetStatId)
                    || string.IsNullOrWhiteSpace(definition.combatEffect.valueUnitKey))
                {
                    errors.Add("combat effect is incomplete: " + definition.baseItemId);
                }

                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    CanonicalItemRarityProfile rarityProfile = resolver.GetRarityProfile(
                        definition.baseItemId,
                        rarity.rarity);
                    if (rarityProfile == null || rarityProfile.StatRanges.Count != 4)
                    {
                        errors.Add("rarity stat profile is incomplete: "
                                   + definition.baseItemId + "@" + rarity.stableKey);
                    }
                }

                if (!CanonicalItemPresentationIdentityResolver.TryResolve(
                        definition,
                        out _,
                        out _,
                        out _))
                {
                    errors.Add("formal presentation identity is incomplete: "
                               + definition.baseItemId);
                }
            }

            C1FormalItemPresentationCatalog presentationCatalog =
                AssetDatabase.LoadAssetAtPath<C1FormalItemPresentationCatalog>(
                    PresentationCatalogPath);
            C1FormalItemPresentationCatalogResult presentationResult =
                presentationCatalog != null
                    ? presentationCatalog.Resolve(resolver)
                    : null;
            if (presentationResult?.accepted != true)
            {
                errors.Add("canonical presentation catalog failed: "
                           + (presentationResult?.diagnosticCode
                              ?? "CATALOG_ASSET_MISSING"));
            }
            else
            {
                int expectedRows = 30 * ItemInstanceRarityCatalog.All.Count + 1;
                if (presentationResult.snapshot.Rows.Count != expectedRows)
                {
                    errors.Add("canonical presentation row count invalid: "
                               + presentationResult.snapshot.Rows.Count);
                }

                foreach (CanonicalItemDefinition definition in definitions.Where(
                             value => value.isOrdinaryDropEligible))
                {
                    foreach (ItemInstanceRarityDefinition rarity in
                             ItemInstanceRarityCatalog.All)
                    {
                        if (!presentationResult.snapshot.TryResolveExact(
                                definition.baseItemId,
                                rarity.stableKey,
                                out _,
                                out string diagnostic))
                        {
                            errors.Add("canonical presentation row missing: "
                                       + definition.baseItemId + "@"
                                       + rarity.stableKey + " / " + diagnostic);
                        }
                    }
                }
            }

            ItemInstanceRollResult i004 = CanonicalItemInstanceFactory.Create(
                resolver, "verify-i004", "I004", ItemInstanceRarity.Blue, 404L);
            if (!i004.isSuccess || i004.snapshot.GeneratedStats.Count == 0)
                errors.Add("I004 canonical instance roll failed");

            CanonicalItemDropRequest request = new(
                "verify-drop",
                "verify-drop-instance",
                "canonical-foundation-verifier",
                "stage-reward",
                1,
                3,
                3104L,
                new Dictionary<string, int>(),
                null);
            CanonicalItemDropResult first = CanonicalItemDropPolicy.Roll(resolver, request);
            CanonicalItemDropResult second = CanonicalItemDropPolicy.Roll(resolver, request);
            if (!first.isSuccess || !second.isSuccess)
            {
                errors.Add("canonical drop roll failed");
                return errors;
            }

            if (first.Definition.baseItemId == "I031") errors.Add("drop selected I031");
            if (first.Definition.baseItemId != second.Definition.baseItemId
                || first.rarity != second.rarity
                || !SameGeneratedValues(first.ItemRoll.snapshot, second.ItemRoll.snapshot))
            {
                errors.Add("same drop input produced different output");
            }

            CanonicalItemDropPolicyProfile profile = CanonicalItemDropPolicyProfile.PlaytestV1;
            CanonicalItemRarityDropRule white = profile.RarityRules
                .Single(value => value.rarity == ItemInstanceRarity.White);
            CanonicalItemRarityDropRule orange = profile.RarityRules
                .Single(value => value.rarity == ItemInstanceRarity.Orange);
            if (white.WeightAtStage(5) >= white.WeightAtStage(1)
                || orange.WeightAtStage(5) <= orange.WeightAtStage(1))
            {
                errors.Add("stage rarity curve does not progress");
            }

            Dictionary<string, int> campaignCopies = new(StringComparer.Ordinal);
            for (int stageIndex = 1; stageIndex <= 5; stageIndex++)
            {
                string stageId = "1-" + stageIndex;
                CanonicalItemDropResult campaignReward =
                    C1CampaignDirectLootPoolAndPolicy.SelectCanonical(
                        resolver,
                        "verify-campaign-" + stageId,
                        "verify-campaign-instance-" + stageId,
                        stageId,
                        stageIndex,
                        9000L + stageIndex,
                        campaignCopies,
                        null);
                if (campaignReward?.isSuccess != true
                    || campaignReward.Definition == null
                    || campaignReward.ItemRoll?.snapshot == null
                    || campaignReward.Definition.baseItemId == "I031"
                    || !string.Equals(
                        campaignReward.Definition.baseItemId,
                        campaignReward.ItemRoll.snapshot.baseItemId,
                        StringComparison.Ordinal))
                {
                    errors.Add("formal campaign canonical reward failed: " + stageId);
                    continue;
                }

                string baseItemId = campaignReward.Definition.baseItemId;
                campaignCopies.TryGetValue(baseItemId, out int copies);
                campaignCopies[baseItemId] = copies + 1;
            }

            return errors;
        }

        private static bool SameGeneratedValues(
            ItemGeneratedInstanceSnapshot left,
            ItemGeneratedInstanceSnapshot right)
        {
            if (left == null || right == null) return false;
            bool statsMatch = left.GeneratedStats.Select(value => (value.statId, value.rawUnits))
                .SequenceEqual(right.GeneratedStats.Select(value => (value.statId, value.rawUnits)));
            bool affixesMatch = left.GeneratedAffixes
                .Select(value => (value.slotId, value.affixId, value.rawUnits))
                .SequenceEqual(right.GeneratedAffixes
                    .Select(value => (value.slotId, value.affixId, value.rawUnits)));
            return statsMatch && affixesMatch && left.buildQualification == right.buildQualification;
        }
    }
}
