using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Lighting;

namespace TalismanBag.Items.Awakening
{
    public interface IItemCoreAwakeningReadOnlyInputProvider
    {
        ItemCoreAwakeningInput GetAwakeningInput(string itemId, string placementId);
    }

    public interface IItemCoreAwakeningSnapshotProvider
    {
        ItemCoreAwakeningResolutionResult GetCoreAwakeningSnapshot();
    }

    public interface IItemCoreEffectDefinitionProvider
    {
        IReadOnlyList<ItemCoreEffectDefinition> GetDefinitions(string itemId);
    }

    public enum ItemCoreAwakeningNodeKind
    {
        Core1,
        Core2,
        Core3,
        Ultimate
    }

    public enum ItemCoreAwakeningStateKey
    {
        Locked,
        UnlockedInactive,
        Active
    }

    public enum ItemCoreAwakeningBlockedReason
    {
        None,
        LevelTooLow,
        ItemNotLit,
        AwakeningUnsupported,
        DefinitionMissing,
        ReservedRarityGate
    }

    public sealed class ItemCoreEffectDefinition
    {
        public ItemCoreEffectDefinition(
            string itemId,
            string coreEffectId,
            ItemCoreAwakeningNodeKind nodeKind,
            int unlockLevel,
            string previewText = "Preview Reserved",
            string requiredRarityKey = "")
        {
            this.itemId = itemId ?? string.Empty;
            this.coreEffectId = coreEffectId ?? string.Empty;
            this.nodeKind = nodeKind;
            this.unlockLevel = unlockLevel;
            this.previewText = string.IsNullOrWhiteSpace(previewText) ? "Preview Reserved" : previewText;
            this.requiredRarityKey = requiredRarityKey ?? string.Empty;
        }

        public string itemId;
        public string coreEffectId;
        public ItemCoreAwakeningNodeKind nodeKind;
        public int unlockLevel;
        public string previewText;
        public string requiredRarityKey;
    }

    public sealed class DefaultItemCoreEffectDefinitionProvider : IItemCoreEffectDefinitionProvider
    {
        public static readonly DefaultItemCoreEffectDefinitionProvider Instance = new();

        public IReadOnlyList<ItemCoreEffectDefinition> GetDefinitions(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || string.Equals(itemId, "I031", StringComparison.Ordinal))
            {
                return Array.Empty<ItemCoreEffectDefinition>();
            }

            string stableItemId = itemId.Trim();
            return new[]
            {
                new ItemCoreEffectDefinition(stableItemId, $"{stableItemId}_CORE_01", ItemCoreAwakeningNodeKind.Core1, 10),
                new ItemCoreEffectDefinition(stableItemId, $"{stableItemId}_CORE_02", ItemCoreAwakeningNodeKind.Core2, 20),
                new ItemCoreEffectDefinition(stableItemId, $"{stableItemId}_CORE_03", ItemCoreAwakeningNodeKind.Core3, 30),
                new ItemCoreEffectDefinition(stableItemId, $"{stableItemId}_CORE_ULT", ItemCoreAwakeningNodeKind.Ultimate, 40, "Preview Reserved", "Reserved")
            };
        }
    }

    public sealed class ItemCoreAwakeningInput
    {
        public ItemCoreAwakeningInput(
            string itemId,
            string placementId,
            int inputLevel,
            bool highRarityUltimatePreview = false,
            string inputSource = "ItemSandboxPreview",
            string requiredRarityKey = "")
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = NormalizePlacementId(placementId, this.itemId);
            this.inputLevel = inputLevel;
            resolvedLevel = inputLevel;
            this.highRarityUltimatePreview = highRarityUltimatePreview;
            this.inputSource = inputSource ?? string.Empty;
            this.requiredRarityKey = requiredRarityKey ?? string.Empty;
        }

        public string itemId;
        public string placementId;
        public int inputLevel;
        public int resolvedLevel;
        public bool highRarityUltimatePreview;
        public string requiredRarityKey;
        public string inputSource;

        public int itemLevel
        {
            get => resolvedLevel;
            set
            {
                inputLevel = value;
                resolvedLevel = value;
            }
        }

        private static string NormalizePlacementId(string placementId, string itemId)
        {
            return string.IsNullOrWhiteSpace(placementId)
                ? itemId ?? string.Empty
                : placementId;
        }
    }

    public sealed class ItemCoreAwakeningNodeDefinition
    {
        public ItemCoreAwakeningNodeDefinition(
            int requiredLevel,
            ItemCoreAwakeningNodeKind nodeKind,
            string nodeId,
            string displayName,
            string effectLabel)
        {
            this.requiredLevel = requiredLevel;
            this.nodeKind = nodeKind;
            this.nodeId = nodeId ?? string.Empty;
            this.displayName = displayName ?? string.Empty;
            this.effectLabel = string.IsNullOrWhiteSpace(effectLabel) ? "Preview Reserved" : effectLabel;
        }

        public int requiredLevel;
        public ItemCoreAwakeningNodeKind nodeKind;
        public string nodeId;
        public string displayName;
        public string effectLabel;
        public bool isUltimate => nodeKind == ItemCoreAwakeningNodeKind.Ultimate;
    }

    public sealed class ItemCoreAwakeningNodeState
    {
        public ItemCoreAwakeningNodeState(
            string itemId,
            string coreEffectId,
            ItemCoreAwakeningNodeKind nodeKind,
            int unlockLevel,
            bool isUnlocked,
            bool isActive,
            ItemCoreAwakeningStateKey stateKey,
            ItemCoreAwakeningBlockedReason blockedReason,
            string previewText,
            string requiredRarityKey)
        {
            this.itemId = itemId ?? string.Empty;
            this.coreEffectId = coreEffectId ?? string.Empty;
            this.nodeKind = nodeKind;
            this.unlockLevel = unlockLevel;
            this.isUnlocked = isUnlocked;
            this.isActive = isActive;
            this.stateKey = stateKey.ToString();
            this.blockedReason = blockedReason.ToString();
            this.previewText = string.IsNullOrWhiteSpace(previewText) ? "Preview Reserved" : previewText;
            this.requiredRarityKey = requiredRarityKey ?? string.Empty;
        }

        public string coreEffectId;
        public string itemId;
        public ItemCoreAwakeningNodeKind nodeKind;
        public int unlockLevel;
        public bool isUnlocked;
        public bool isActive;
        public string stateKey;
        public string blockedReason;
        public string previewText;
        public string requiredRarityKey;

        public int requiredLevel => unlockLevel;
        public string nodeId => coreEffectId;
        public string displayName => $"{nodeKind} Lv.{unlockLevel}";
        public string effectLabel => previewText;
    }

    public sealed class ItemCoreAwakeningItemResult
    {
        private readonly ItemCoreAwakeningNodeState[] nodeStates;
        private readonly string[] validationErrors;

        public ItemCoreAwakeningItemResult(
            string itemId,
            string placementId,
            int inputLevel,
            int resolvedLevel,
            bool highRarityUltimatePreview,
            string requiredRarityKey,
            bool isLit,
            bool isLightingSource,
            bool supportsAwakening,
            bool basicEffectActive,
            IReadOnlyList<ItemCoreAwakeningNodeState> nodeStates,
            IReadOnlyList<string> validationErrors,
            string inputSource)
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = string.IsNullOrWhiteSpace(placementId) ? this.itemId : placementId;
            this.inputLevel = inputLevel;
            this.resolvedLevel = Math.Max(1, Math.Min(40, resolvedLevel));
            this.highRarityUltimatePreview = highRarityUltimatePreview;
            this.requiredRarityKey = requiredRarityKey ?? string.Empty;
            this.isLit = isLit;
            this.isLightingSource = isLightingSource;
            this.supportsAwakening = supportsAwakening;
            this.basicEffectActive = basicEffectActive;
            this.nodeStates = (nodeStates ?? Array.Empty<ItemCoreAwakeningNodeState>()).ToArray();
            this.validationErrors = NormalizeErrors(validationErrors);
            this.inputSource = inputSource ?? string.Empty;
        }

        public string itemId;
        public string placementId;
        public int inputLevel;
        public int resolvedLevel;
        public bool highRarityUltimatePreview;
        public string requiredRarityKey;
        public bool isLit;
        public bool isLightingSource;
        public bool supportsAwakening;
        public bool basicEffectActive;
        public bool coreEffectUnlocked => unlockedCoreEffectCount > 0;
        public bool coreEffectActive => activeCoreEffectCount > 0;
        public int unlockedCoreEffectCount => nodeStates.Count(node => node.isUnlocked);
        public int activeCoreEffectCount => nodeStates.Count(node => node.isActive);
        public int currentUnlockedNodeLevel => nodeStates.Where(node => node.isUnlocked).Select(node => node.unlockLevel).DefaultIfEmpty(0).Max();
        public int nextUnlockLevel => nodeStates.Where(node => !node.isUnlocked).Select(node => node.unlockLevel).DefaultIfEmpty(0).Min();
        public int nextNodeLevel => nextUnlockLevel;
        public int itemLevel => resolvedLevel;
        public string inputSource;
        public IReadOnlyList<ItemCoreAwakeningNodeState> NodeStates => nodeStates;
        public IReadOnlyList<string> ValidationErrors => validationErrors;
        public IReadOnlyList<int> UnlockedNodeLevels => nodeStates.Where(node => node.isUnlocked).Select(node => node.unlockLevel).ToArray();
        public IReadOnlyList<int> ActiveNodeLevels => nodeStates.Where(node => node.isActive).Select(node => node.unlockLevel).ToArray();
        public IReadOnlyList<string> UnlockedCoreEffectIds => nodeStates.Where(node => node.isUnlocked).Select(node => node.coreEffectId).Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();
        public IReadOnlyList<string> ActiveCoreEffectIds => nodeStates.Where(node => node.isActive).Select(node => node.coreEffectId).Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();
        public IReadOnlyList<string> LockedCoreEffectIds => nodeStates.Where(node => !node.isUnlocked).Select(node => node.coreEffectId).Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();

        private static string[] NormalizeErrors(IReadOnlyList<string> errors)
        {
            return (errors ?? Array.Empty<string>())
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }
    }

    public sealed class ItemCoreAwakeningResolutionResult
    {
        private readonly ItemCoreAwakeningItemResult[] itemResults;
        private readonly string[] validationErrors;

        public ItemCoreAwakeningResolutionResult(
            IReadOnlyList<ItemCoreAwakeningItemResult> itemResults,
            IReadOnlyList<string> validationErrors)
        {
            this.itemResults = (itemResults ?? Array.Empty<ItemCoreAwakeningItemResult>())
                .OrderBy(item => item.placementId, StringComparer.Ordinal)
                .ThenBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();

            this.validationErrors = (validationErrors ?? Array.Empty<string>())
                .Concat(this.itemResults.SelectMany(item => item.ValidationErrors))
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        public IReadOnlyList<ItemCoreAwakeningItemResult> ItemResults => itemResults;
        public IReadOnlyList<string> ValidationErrors => validationErrors;

        public ItemCoreAwakeningItemResult FindPlacementResult(string placementId)
        {
            return itemResults.FirstOrDefault(item => string.Equals(item.placementId, placementId, StringComparison.Ordinal));
        }

        public ItemCoreAwakeningItemResult FindItemResult(string itemId)
        {
            return itemResults.FirstOrDefault(item => string.Equals(item.itemId, itemId, StringComparison.Ordinal));
        }
    }

    public static class ItemCoreAwakeningResolver
    {
        private static readonly ItemCoreAwakeningNodeDefinition[] DefaultNodeDefinitions =
        {
            new(10, ItemCoreAwakeningNodeKind.Core1, "Core1", "Lv.10 Core 1", "Preview Reserved"),
            new(20, ItemCoreAwakeningNodeKind.Core2, "Core2", "Lv.20 Core 2", "Preview Reserved"),
            new(30, ItemCoreAwakeningNodeKind.Core3, "Core3", "Lv.30 Core 3", "Preview Reserved"),
            new(40, ItemCoreAwakeningNodeKind.Ultimate, "Ultimate", "Lv.40 Ultimate", "Preview Reserved")
        };

        public static IReadOnlyList<ItemCoreAwakeningNodeDefinition> DefaultNodes => DefaultNodeDefinitions;

        public static ItemCoreAwakeningResolutionResult Resolve(
            ItemLightingResolutionResult lightingResult,
            IReadOnlyList<ItemCoreAwakeningInput> inputs)
        {
            return Resolve(lightingResult, inputs, DefaultItemCoreEffectDefinitionProvider.Instance);
        }

        public static ItemCoreAwakeningResolutionResult Resolve(
            ItemLightingResolutionResult lightingResult,
            IReadOnlyList<ItemCoreAwakeningInput> inputs,
            IItemCoreEffectDefinitionProvider definitionProvider)
        {
            List<string> validationErrors = new();
            Dictionary<string, ItemCoreAwakeningInput> inputsByPlacement = BuildInputMap(inputs, validationErrors);
            List<ItemCoreAwakeningItemResult> results = new();

            if (lightingResult == null)
            {
                validationErrors.Add("lightingResult is null; emitted an empty CoreAwakening snapshot.");
                return new ItemCoreAwakeningResolutionResult(results, validationErrors);
            }

            foreach (ItemLightingItemResult lightingItem in lightingResult.ItemResults)
            {
                if (lightingItem == null)
                {
                    validationErrors.Add("null lighting item ignored by CoreAwakening resolver.");
                    continue;
                }

                string placementId = string.IsNullOrWhiteSpace(lightingItem.placementId)
                    ? lightingItem.itemId
                    : lightingItem.placementId;
                if (string.IsNullOrWhiteSpace(placementId))
                {
                    validationErrors.Add($"empty placementId for itemId '{lightingItem.itemId}' ignored by CoreAwakening resolver.");
                    continue;
                }

                inputsByPlacement.TryGetValue(placementId, out ItemCoreAwakeningInput input);
                if (input == null)
                {
                    validationErrors.Add($"missing awakening input for placementId '{placementId}'; defaulted to Lv.1.");
                    input = new ItemCoreAwakeningInput(lightingItem.itemId, placementId, 1, false, "MissingInputDefaultLv1");
                }

                results.Add(ResolveItem(lightingItem, input, definitionProvider));
            }

            return new ItemCoreAwakeningResolutionResult(results, validationErrors);
        }

        public static ItemCoreAwakeningItemResult ResolveItem(
            ItemLightingItemResult lightingItem,
            ItemCoreAwakeningInput input)
        {
            return ResolveItem(lightingItem, input, DefaultItemCoreEffectDefinitionProvider.Instance);
        }

        public static ItemCoreAwakeningItemResult ResolveItem(
            ItemLightingItemResult lightingItem,
            ItemCoreAwakeningInput input,
            IItemCoreEffectDefinitionProvider definitionProvider)
        {
            List<string> validationErrors = new();

            if (lightingItem == null)
            {
                ItemCoreAwakeningInput fallbackInput = input ?? new ItemCoreAwakeningInput(string.Empty, string.Empty, 1, false, "MissingLightingDefaultLv1");
                LevelResolution fallbackLevel = ResolveLevel(fallbackInput, validationErrors);
                validationErrors.Add("lighting item is null.");
                return new ItemCoreAwakeningItemResult(
                    fallbackInput.itemId,
                    fallbackInput.placementId,
                    fallbackLevel.inputLevel,
                    fallbackLevel.resolvedLevel,
                    fallbackInput.highRarityUltimatePreview,
                    fallbackInput.requiredRarityKey,
                    false,
                    false,
                    false,
                    false,
                    Array.Empty<ItemCoreAwakeningNodeState>(),
                    validationErrors,
                    fallbackInput.inputSource);
            }

            input ??= new ItemCoreAwakeningInput(lightingItem.itemId, lightingItem.placementId, 1, false, "MissingInputDefaultLv1");
            if (string.Equals(input.inputSource, "MissingInputDefaultLv1", StringComparison.Ordinal))
            {
                validationErrors.Add($"missing awakening input for placementId '{lightingItem.placementId}'; defaulted to Lv.1.");
            }

            LevelResolution level = ResolveLevel(input, validationErrors);
            bool isLightingSource = lightingItem.isLightingSource;
            bool sourceUnsupported = isLightingSource || string.Equals(lightingItem.itemId, "I031", StringComparison.Ordinal);
            bool basicEffectActive = lightingItem.isLit && !sourceUnsupported;
            IReadOnlyList<ItemCoreEffectDefinition> definitions = GetDefinitions(definitionProvider, lightingItem.itemId, sourceUnsupported, validationErrors);
            Dictionary<ItemCoreAwakeningNodeKind, ItemCoreEffectDefinition> definitionsByKind = ValidateDefinitions(
                lightingItem.itemId,
                sourceUnsupported,
                definitions,
                validationErrors);
            bool supportsAwakening = !sourceUnsupported && definitionsByKind.Count > 0;

            List<ItemCoreAwakeningNodeState> nodes = new();
            if (sourceUnsupported)
            {
                return new ItemCoreAwakeningItemResult(
                    lightingItem.itemId,
                    lightingItem.placementId,
                    level.inputLevel,
                    level.resolvedLevel,
                    input.highRarityUltimatePreview,
                    input.requiredRarityKey,
                    lightingItem.isLit,
                    lightingItem.isLightingSource,
                    false,
                    false,
                    nodes,
                    validationErrors,
                    input.inputSource);
            }

            foreach (ItemCoreAwakeningNodeDefinition expectedNode in DefaultNodeDefinitions)
            {
                if (!definitionsByKind.TryGetValue(expectedNode.nodeKind, out ItemCoreEffectDefinition definition))
                {
                    nodes.Add(BuildMissingDefinitionNode(lightingItem.itemId, expectedNode));
                    continue;
                }

                bool unlocked = supportsAwakening && level.resolvedLevel >= definition.unlockLevel;
                bool active = lightingItem.isLit && unlocked;
                ItemCoreAwakeningStateKey stateKey = active
                    ? ItemCoreAwakeningStateKey.Active
                    : unlocked ? ItemCoreAwakeningStateKey.UnlockedInactive : ItemCoreAwakeningStateKey.Locked;
                ItemCoreAwakeningBlockedReason blockedReason = ResolveBlockedReason(
                    definition,
                    lightingItem.isLit,
                    unlocked,
                    active,
                    input.highRarityUltimatePreview);

                nodes.Add(new ItemCoreAwakeningNodeState(
                    definition.itemId,
                    definition.coreEffectId,
                    definition.nodeKind,
                    definition.unlockLevel,
                    unlocked,
                    active,
                    stateKey,
                    blockedReason,
                    definition.previewText,
                    definition.requiredRarityKey));
            }

            return new ItemCoreAwakeningItemResult(
                lightingItem.itemId,
                lightingItem.placementId,
                level.inputLevel,
                level.resolvedLevel,
                input.highRarityUltimatePreview,
                input.requiredRarityKey,
                lightingItem.isLit,
                lightingItem.isLightingSource,
                supportsAwakening,
                basicEffectActive,
                nodes,
                validationErrors,
                input.inputSource);
        }

        public static string BuildStatusText(ItemCoreAwakeningItemResult result)
        {
            if (result == null)
            {
                return "Core awakening: no preview input.";
            }

            if (!result.supportsAwakening)
            {
                return $"Core awakening: inputLv={result.inputLevel}, resolvedLv={result.resolvedLevel}; supportsAwakening=false.";
            }

            return $"Core awakening: inputLv={result.inputLevel}, resolvedLv={result.resolvedLevel}; unlocked={FormatIds(result.UnlockedCoreEffectIds)}, active={FormatIds(result.ActiveCoreEffectIds)}, nextLv={FormatLevel(result.nextUnlockLevel)}.";
        }

        public static string FormatOverview(ItemCoreAwakeningResolutionResult result)
        {
            if (result == null)
            {
                return "CoreAwakening overview: no snapshot.";
            }

            int unlocked = result.ItemResults.Count(item => item.coreEffectUnlocked);
            int active = result.ItemResults.Count(item => item.coreEffectActive);
            string errors = result.ValidationErrors.Count == 0
                ? "None"
                : string.Join(" | ", result.ValidationErrors);
            return $"CoreAwakening overview: nodes Lv10/Lv20/Lv30/Lv40; unlocked placements={unlocked}; active placements={active}; validationErrors={errors}.";
        }

        public static string FormatNodeLevels(IReadOnlyList<int> levels)
        {
            return levels == null || levels.Count == 0
                ? "None"
                : string.Join("|", levels.Select(level => $"Lv.{level}"));
        }

        public static string FormatIds(IReadOnlyList<string> ids)
        {
            string[] normalized = (ids ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToArray();
            return normalized.Length == 0
                ? "None"
                : string.Join("|", normalized);
        }

        public static int ExpectedUnlockLevel(ItemCoreAwakeningNodeKind nodeKind)
        {
            return nodeKind switch
            {
                ItemCoreAwakeningNodeKind.Core1 => 10,
                ItemCoreAwakeningNodeKind.Core2 => 20,
                ItemCoreAwakeningNodeKind.Core3 => 30,
                ItemCoreAwakeningNodeKind.Ultimate => 40,
                _ => 0
            };
        }

        private static IReadOnlyList<ItemCoreEffectDefinition> GetDefinitions(
            IItemCoreEffectDefinitionProvider definitionProvider,
            string itemId,
            bool sourceUnsupported,
            List<string> validationErrors)
        {
            if (definitionProvider == null)
            {
                if (!sourceUnsupported)
                {
                    validationErrors.Add($"definitionProvider is null for itemId '{itemId}'.");
                }

                return Array.Empty<ItemCoreEffectDefinition>();
            }

            try
            {
                IReadOnlyList<ItemCoreEffectDefinition> definitions = definitionProvider.GetDefinitions(itemId);
                if (definitions == null)
                {
                    if (!sourceUnsupported)
                    {
                        validationErrors.Add($"definitionProvider returned null definitions for itemId '{itemId}'.");
                    }

                    return Array.Empty<ItemCoreEffectDefinition>();
                }

                return definitions;
            }
            catch (Exception exception)
            {
                validationErrors.Add($"definitionProvider threw for itemId '{itemId}': {exception.Message}");
                return Array.Empty<ItemCoreEffectDefinition>();
            }
        }

        private static Dictionary<ItemCoreAwakeningNodeKind, ItemCoreEffectDefinition> ValidateDefinitions(
            string itemId,
            bool sourceUnsupported,
            IReadOnlyList<ItemCoreEffectDefinition> definitions,
            List<string> validationErrors)
        {
            Dictionary<ItemCoreAwakeningNodeKind, ItemCoreEffectDefinition> byKind = new();
            if (sourceUnsupported)
            {
                return byKind;
            }

            List<ItemCoreEffectDefinition> validDefinitions = new();
            HashSet<string> seenEffectIds = new(StringComparer.Ordinal);
            foreach (ItemCoreEffectDefinition definition in StableDefinitionOrder(definitions))
            {
                if (definition == null)
                {
                    validationErrors.Add($"null core effect definition for itemId '{itemId}' ignored.");
                    continue;
                }

                if (!string.Equals(definition.itemId, itemId, StringComparison.Ordinal))
                {
                    validationErrors.Add($"core effect definition itemId mismatch for current itemId '{itemId}': definition itemId '{definition.itemId}'.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(definition.coreEffectId))
                {
                    validationErrors.Add($"empty coreEffectId for itemId '{itemId}' nodeKind '{definition.nodeKind}' ignored.");
                    continue;
                }

                if (!seenEffectIds.Add(definition.coreEffectId))
                {
                    validationErrors.Add($"duplicate coreEffectId '{definition.coreEffectId}' for itemId '{itemId}' ignored after stable first definition.");
                    continue;
                }

                int expectedUnlockLevel = ExpectedUnlockLevel(definition.nodeKind);
                if (definition.unlockLevel != expectedUnlockLevel)
                {
                    validationErrors.Add($"nodeKind '{definition.nodeKind}' unlockLevel mismatch for itemId '{itemId}': expected {expectedUnlockLevel}, actual {definition.unlockLevel}.");
                    continue;
                }

                validDefinitions.Add(definition);
            }

            foreach (ItemCoreEffectDefinition definition in validDefinitions
                .OrderBy(definition => definition.nodeKind)
                .ThenBy(definition => definition.unlockLevel)
                .ThenBy(definition => definition.coreEffectId, StringComparer.Ordinal))
            {
                if (byKind.ContainsKey(definition.nodeKind))
                {
                    validationErrors.Add($"duplicate nodeKind '{definition.nodeKind}' for itemId '{itemId}' ignored after stable first definition.");
                    continue;
                }

                byKind[definition.nodeKind] = definition;
            }

            if (definitions == null || definitions.Count == 0)
            {
                validationErrors.Add($"ordinary itemId '{itemId}' has no core effect definitions.");
            }

            foreach (ItemCoreAwakeningNodeDefinition expectedNode in DefaultNodeDefinitions)
            {
                if (!byKind.ContainsKey(expectedNode.nodeKind))
                {
                    validationErrors.Add($"missing core effect definition for itemId '{itemId}' nodeKind '{expectedNode.nodeKind}'.");
                }
            }

            return byKind;
        }

        private static IEnumerable<ItemCoreEffectDefinition> StableDefinitionOrder(IReadOnlyList<ItemCoreEffectDefinition> definitions)
        {
            return (definitions ?? Array.Empty<ItemCoreEffectDefinition>())
                .Select((definition, index) => new { definition, index })
                .OrderBy(item => item.definition?.coreEffectId ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(item => item.definition?.nodeKind ?? ItemCoreAwakeningNodeKind.Core1)
                .ThenBy(item => item.definition?.unlockLevel ?? 0)
                .ThenBy(item => item.index)
                .Select(item => item.definition);
        }

        private static ItemCoreAwakeningNodeState BuildMissingDefinitionNode(
            string itemId,
            ItemCoreAwakeningNodeDefinition expectedNode)
        {
            return new ItemCoreAwakeningNodeState(
                itemId,
                string.Empty,
                expectedNode.nodeKind,
                expectedNode.requiredLevel,
                false,
                false,
                ItemCoreAwakeningStateKey.Locked,
                ItemCoreAwakeningBlockedReason.DefinitionMissing,
                "Preview Reserved",
                string.Empty);
        }

        private static ItemCoreAwakeningBlockedReason ResolveBlockedReason(
            ItemCoreEffectDefinition definition,
            bool isLit,
            bool unlocked,
            bool active,
            bool highRarityUltimatePreview)
        {
            if (active)
            {
                return ItemCoreAwakeningBlockedReason.None;
            }

            if (unlocked && !isLit)
            {
                return ItemCoreAwakeningBlockedReason.ItemNotLit;
            }

            if (definition.nodeKind == ItemCoreAwakeningNodeKind.Ultimate && highRarityUltimatePreview)
            {
                return ItemCoreAwakeningBlockedReason.ReservedRarityGate;
            }

            return ItemCoreAwakeningBlockedReason.LevelTooLow;
        }

        private static LevelResolution ResolveLevel(
            ItemCoreAwakeningInput input,
            List<string> validationErrors)
        {
            int inputLevel = input?.inputLevel ?? 1;
            int resolvedLevel = Math.Max(1, Math.Min(40, inputLevel));
            if (inputLevel < 1)
            {
                validationErrors.Add($"inputLevel {inputLevel} below 1 for placementId '{input?.placementId}'; resolvedLevel=1.");
            }
            else if (inputLevel > 40)
            {
                validationErrors.Add($"inputLevel {inputLevel} above 40 for placementId '{input?.placementId}'; resolvedLevel=40.");
            }

            if (input != null)
            {
                input.resolvedLevel = resolvedLevel;
            }

            return new LevelResolution(inputLevel, resolvedLevel);
        }

        private static Dictionary<string, ItemCoreAwakeningInput> BuildInputMap(
            IReadOnlyList<ItemCoreAwakeningInput> inputs,
            List<string> validationErrors)
        {
            Dictionary<string, ItemCoreAwakeningInput> inputsByPlacement = new(StringComparer.Ordinal);
            foreach (ItemCoreAwakeningInput input in inputs ?? Array.Empty<ItemCoreAwakeningInput>())
            {
                if (input == null)
                {
                    validationErrors.Add("null awakening input ignored.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(input.placementId))
                {
                    validationErrors.Add($"awakening input for itemId '{input.itemId}' has empty placementId and was ignored.");
                    continue;
                }

                if (inputsByPlacement.ContainsKey(input.placementId))
                {
                    validationErrors.Add($"duplicate awakening input placementId '{input.placementId}' ignored after the first occurrence.");
                    continue;
                }

                inputsByPlacement[input.placementId] = input;
            }

            return inputsByPlacement;
        }

        private static string FormatLevel(int level)
        {
            return level > 0 ? $"Lv.{level}" : "None";
        }

        private readonly struct LevelResolution
        {
            public LevelResolution(int inputLevel, int resolvedLevel)
            {
                this.inputLevel = inputLevel;
                this.resolvedLevel = resolvedLevel;
            }

            public readonly int inputLevel;
            public readonly int resolvedLevel;
        }
    }
}
