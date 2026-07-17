using System;
using System.Collections.Generic;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Lighting;

namespace TalismanBag.ItemSandbox
{
    public static class ItemSandboxCoreAwakeningPreviewCatalog
    {
        public static IReadOnlyList<ItemCoreAwakeningInput> CreatePreviewInputs(ItemLightingResolutionResult lightingResult)
        {
            List<ItemCoreAwakeningInput> inputs = new();
            foreach (ItemLightingItemResult item in lightingResult?.ItemResults ?? Array.Empty<ItemLightingItemResult>())
            {
                if (item == null)
                {
                    continue;
                }

                inputs.Add(new ItemCoreAwakeningInput(
                    item.itemId,
                    item.placementId,
                    ResolvePreviewLevel(item),
                    highRarityUltimatePreview: false,
                    inputSource: "ItemSandboxGreyboxLevelPreview"));
            }

            return inputs;
        }

        public static int ResolvePreviewLevel(ItemLightingItemResult item)
        {
            if (item == null)
            {
                return 1;
            }

            if (item.isLightingSource)
            {
                return 40;
            }

            return item.itemId switch
            {
                "I001" => 1,
                "I004" => 30,
                "I007" => 10,
                "I013" => 40,
                "I015" => 20,
                _ => ResolveFallbackLevel(item.itemId)
            };
        }

        public static string BuildPreviewLevelHint(ItemCoreAwakeningItemResult result)
        {
            if (result == null)
            {
                return "Awakening preview: no selected item.";
            }

            return $"inputLevel={result.inputLevel} resolvedLevel={result.resolvedLevel} coreUnlocked={result.coreEffectUnlocked} coreActive={result.coreEffectActive} activeEffects={ItemCoreAwakeningResolver.FormatIds(result.ActiveCoreEffectIds)} nextUnlockLevel={result.nextUnlockLevel}";
        }

        private static int ResolveFallbackLevel(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return 1;
            }

            int hash = ComputeStableHash(itemId);
            return hash % 5 switch
            {
                0 => 1,
                1 => 10,
                2 => 20,
                3 => 30,
                _ => 40
            };
        }

        private static int ComputeStableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++)
                {
                    hash = hash * 31 + value[i];
                }

                return hash & int.MaxValue;
            }
        }
    }
}
