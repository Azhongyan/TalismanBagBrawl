using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using TalismanBag.Items.Skills;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    public enum ItemFullDetailPlacementFailureReason
    {
        None = 0,
        NoSelection = 1,
        AlreadyPlaced = 2,
        DefinitionMissing = 3,
        InvalidRotation = 4,
        OutOfBounds = 5,
        EyeCellBlocked = 6,
        CellOccupied = 7,
        DuplicateJuNian = 8,
        DuplicateBaseItemPlaced = 9,
        PlacementMissing = 10
    }

    public sealed class ItemFullDetailPlacementPreview
    {
        internal ItemFullDetailPlacementPreview(
            string baseItemId,
            Vector2Int anchorCell,
            int rotation,
            IEnumerable<Vector2Int> occupiedCells,
            ItemFullDetailPlacementFailureReason failureReason)
        {
            this.baseItemId = baseItemId ?? string.Empty;
            this.anchorCell = anchorCell;
            this.rotation = rotation;
            OccupiedCells = Array.AsReadOnly((occupiedCells ?? Array.Empty<Vector2Int>()).ToArray());
            this.failureReason = failureReason;
        }

        public string baseItemId { get; }
        public Vector2Int anchorCell { get; }
        public int rotation { get; }
        public IReadOnlyList<Vector2Int> OccupiedCells { get; }
        public ItemFullDetailPlacementFailureReason failureReason { get; }
        public bool isLegal => failureReason == ItemFullDetailPlacementFailureReason.None;
    }

    public sealed class ItemFullDetailWorkbenchInstance
    {
        internal ItemFullDetailWorkbenchInstance(ItemBalanceCandidateDetailResult candidate)
        {
            Candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
            Projection = candidate.detailProjection?.projection;
            if (Projection == null)
            {
                throw new ArgumentException("Candidate result has no generated instance projection.", nameof(candidate));
            }
        }

        public ItemBalanceCandidateDetailResult Candidate { get; }
        public ItemInstanceProjectionContractSnapshot Projection { get; }
        public string itemInstanceId => Projection.itemInstanceId;
        public string baseItemId => Projection.baseItemId;
        public string rarityKey => Projection.rarityKey;
        public long rootSeed => Projection.rootSeed;
        public ItemBuildQualification buildQualification => Projection.buildQualification;
    }

    public sealed class ItemFullDetailWorkbenchPlacement
    {
        internal ItemFullDetailWorkbenchPlacement(
            string placementId,
            string itemInstanceId,
            string baseItemId,
            Vector2Int anchorCell,
            int rotation,
            bool isJuNian)
        {
            this.placementId = placementId ?? string.Empty;
            this.itemInstanceId = itemInstanceId ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.anchorCell = anchorCell;
            this.rotation = NormalizeRotation(rotation);
            this.isJuNian = isJuNian;
        }

        public string placementId { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public Vector2Int anchorCell { get; }
        public int rotation { get; }
        public bool isJuNian { get; }

        internal ItemFullDetailWorkbenchPlacement Move(Vector2Int cell) =>
            new(placementId, itemInstanceId, baseItemId, cell, rotation, isJuNian);

        internal ItemFullDetailWorkbenchPlacement RotateClockwise() =>
            new(placementId, itemInstanceId, baseItemId, anchorCell, rotation + 90, isJuNian);

        internal ItemFullDetailWorkbenchPlacement ReplaceInstance(string nextItemInstanceId) =>
            new(placementId, nextItemInstanceId, baseItemId, anchorCell, rotation, isJuNian);

        private static int NormalizeRotation(int value)
        {
            int normalized = value % 360;
            return normalized < 0 ? normalized + 360 : normalized;
        }
    }

    public sealed class ItemFullDetailWorkbenchSnapshot
    {
        internal ItemFullDetailWorkbenchSnapshot(
            ItemSystemSnapshot placementSnapshot,
            ItemLightingResolutionResult lighting,
            ItemArrayBonusResolutionResult array,
            ItemBuildSynergyResolutionResult build,
            ItemCoreAwakeningResolutionResult awakening,
            ItemSkillMonitorResolutionResult skillMonitor,
            IEnumerable<string> validationErrors)
        {
            this.placementSnapshot = placementSnapshot;
            this.lighting = lighting;
            this.array = array;
            this.build = build;
            this.awakening = awakening;
            this.skillMonitor = skillMonitor;
            ValidationErrors = Array.AsReadOnly((validationErrors ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }

        public ItemSystemSnapshot placementSnapshot { get; }
        public ItemLightingResolutionResult lighting { get; }
        public ItemArrayBonusResolutionResult array { get; }
        public ItemBuildSynergyResolutionResult build { get; }
        public ItemCoreAwakeningResolutionResult awakening { get; }
        public ItemSkillMonitorResolutionResult skillMonitor { get; }
        public IReadOnlyList<string> ValidationErrors { get; }
    }

    /// <summary>
    /// Workbench presentation seam over the shared Package A qualification resolver.
    /// It owns no independent qualification filter or Build threshold logic.
    /// </summary>
    public static class ItemFullDetailQualifiedBuildAdapter
    {
        public static ItemBuildSynergyResolutionResult Resolve(
            ItemSystemSnapshot itemSystemSnapshot,
            IReadOnlyDictionary<string, ItemBuildQualification> qualificationByPlacement)
        {
            return ItemInstanceQualifiedBuildContributionResolver.Resolve(
                itemSystemSnapshot,
                qualificationByPlacement);
        }
    }

    public static class ItemFullDetailCandidateAwakeningAdapter
    {
        private static readonly int[] FallbackNodeLevels = { 10, 20, 30, 40 };

        public static ItemCoreAwakeningResolutionResult Resolve(
            ItemLightingResolutionResult lighting,
            IReadOnlyDictionary<string, ItemFullDetailWorkbenchInstance> instanceByPlacement,
            int sandboxLevel)
        {
            List<ItemCoreAwakeningItemResult> results = new();
            List<string> errors = new();
            int level = Mathf.Clamp(sandboxLevel, 1, 40);
            foreach (ItemLightingItemResult lit in lighting?.ItemResults ?? Array.Empty<ItemLightingItemResult>())
            {
                if (lit == null)
                {
                    continue;
                }

                if (lit.isLightingSource)
                {
                    results.Add(new ItemCoreAwakeningItemResult(
                        lit.itemId, lit.placementId, sandboxLevel, level, false, string.Empty,
                        lit.isLit, true, false, false, Array.Empty<ItemCoreAwakeningNodeState>(),
                        Array.Empty<string>(), "ItemFullDetailBuildSandboxWorkbench"));
                    continue;
                }

                if (!instanceByPlacement.TryGetValue(lit.placementId, out ItemFullDetailWorkbenchInstance instance))
                {
                    errors.Add($"missing generated instance for awakening placementId '{lit.placementId}'.");
                    continue;
                }

                ItemInstanceProjectionContractSnapshot projection = instance.Projection;
                HashSet<string> visible = new(projection.VisibleCoreEffectIds, StringComparer.Ordinal);
                List<ItemCoreAwakeningNodeState> nodes = new();
                string[] formalEligibleIds = projection.EligibleCoreEffectIds
                    .Where(coreId => FormalCandidateRank(
                        projection.baseItemId, coreId) < int.MaxValue)
                    .OrderBy(coreId => FormalCandidateRank(
                        projection.baseItemId, coreId))
                    .ToArray();
                foreach (string coreId in formalEligibleIds)
                {
                    if (!TryResolveFormalNodeKind(
                            projection.baseItemId,
                            coreId,
                            out ItemCoreAwakeningNodeKind kind,
                            out int formalRank))
                    {
                        errors.Add(
                            $"unsupported formal core identity '{coreId}' for '{projection.baseItemId}'.");
                        continue;
                    }
                    int unlockLevel = instance.Candidate.CoreUnlockLevels.TryGetValue(coreId, out int configuredLevel)
                        ? configuredLevel
                        : FallbackNodeLevels[formalRank];
                    bool isVisible = visible.Contains(coreId);
                    bool unlocked = isVisible && level >= unlockLevel;
                    bool active = unlocked && lit.isLit;
                    ItemDetailTextLine detailLine =
                        instance.Candidate.viewModel.displayCoreEffects
                            .SingleOrDefault(value => value != null
                                && string.Equals(
                                    value.stateKey,
                                    coreId,
                                    StringComparison.Ordinal));
                    string previewText = detailLine == null
                        ? "核心效果校验失败"
                        : detailLine.title + "\n" + detailLine.body;
                    nodes.Add(new ItemCoreAwakeningNodeState(
                        projection.baseItemId,
                        coreId,
                        kind,
                        unlockLevel,
                        unlocked,
                        active,
                        active ? ItemCoreAwakeningStateKey.Active
                            : unlocked ? ItemCoreAwakeningStateKey.UnlockedInactive : ItemCoreAwakeningStateKey.Locked,
                        active ? ItemCoreAwakeningBlockedReason.None
                            : !isVisible ? ItemCoreAwakeningBlockedReason.ReservedRarityGate
                            : !unlocked ? ItemCoreAwakeningBlockedReason.LevelTooLow
                            : ItemCoreAwakeningBlockedReason.ItemNotLit,
                        previewText,
                        projection.rarityKey));
                }

                results.Add(new ItemCoreAwakeningItemResult(
                    lit.itemId,
                    lit.placementId,
                    sandboxLevel,
                    level,
                    string.Equals(projection.rarityKey, "orange", StringComparison.Ordinal),
                    projection.rarityKey,
                    lit.isLit,
                    false,
                    nodes.Count > 0,
                    lit.isLit,
                    nodes,
                    Array.Empty<string>(),
                    "ItemFullDetailBuildSandboxWorkbench"));
            }

            return new ItemCoreAwakeningResolutionResult(results, errors);
        }

        private static bool TryResolveFormalNodeKind(
            string baseItemId,
            string candidateDefinitionId,
            out ItemCoreAwakeningNodeKind nodeKind,
            out int rank)
        {
            rank = FormalCandidateRank(
                baseItemId, candidateDefinitionId);
            nodeKind = rank switch
            {
                0 => ItemCoreAwakeningNodeKind.Core1,
                1 => ItemCoreAwakeningNodeKind.Core2,
                2 => ItemCoreAwakeningNodeKind.Core3,
                3 => ItemCoreAwakeningNodeKind.Ultimate,
                _ => default
            };
            return rank >= 0 && rank < FallbackNodeLevels.Length;
        }

        private static int FormalCandidateRank(
            string baseItemId,
            string candidateDefinitionId)
        {
            string prefix = "candidate_core_"
                + (baseItemId ?? string.Empty).Trim()
                    .ToLowerInvariant();
            string[] identities =
            {
                prefix + "_01",
                prefix + "_02",
                prefix + "_03",
                prefix + "_ultimate"
            };
            for (int index = 0;
                 index < identities.Length;
                 index++)
            {
                if (string.Equals(
                        identities[index],
                        candidateDefinitionId,
                        StringComparison.Ordinal))
                {
                    return index;
                }
            }
            return int.MaxValue;
        }
    }

    public sealed class ItemFullDetailBuildSandboxWorkbenchSession
    {
        public const string JuNianInstanceId = "FIXED_CORE_I031";
        private const string BuildTitleHex = "F2EDE2";
        private const string BuildActiveHex = "87B66A";
        private const string BuildInactiveHex = "8A8A8A";
        private readonly ItemBalanceCandidateDetailSandboxAdapter candidateAdapter;
        private readonly IItemDetailViewModelProvider catalogProvider;
        private readonly List<ItemFullDetailWorkbenchInstance> instances = new();
        private readonly List<ItemFullDetailWorkbenchPlacement> placements = new();
        private readonly List<ItemDetailArrayModifierDefinition> arrayModifierCandidates;
        private readonly ReadOnlyCollection<ItemInnerDataDefinition> catalog;
        private int placementSequence;
        private string selectedInstanceId = string.Empty;
        private string selectedPlacementId = string.Empty;
        private string selectedMainBuildId = string.Empty;
        private int sandboxLevel = 10;

        public ItemFullDetailBuildSandboxWorkbenchSession(
            ItemBalanceCandidateDetailSandboxAdapter candidateAdapter,
            IItemDetailViewModelProvider catalogProvider)
        {
            this.candidateAdapter = candidateAdapter ?? throw new ArgumentNullException(nameof(candidateAdapter));
            this.catalogProvider = catalogProvider ?? throw new ArgumentNullException(nameof(catalogProvider));
            catalog = Array.AsReadOnly(ItemInnerDataCatalog.AllItems.Select(CloneCatalogItem).ToArray());
            arrayModifierCandidates = ItemDetailArrayModifierResolver.BuildDefaultCandidateDefinitions()
                .Select(ItemDetailArrayModifierResolver.CloneDefinition)
                .ToList();
            Snapshot = RebuildSnapshot();
        }

        public IReadOnlyList<ItemFullDetailWorkbenchInstance> Instances => instances.AsReadOnly();
        public IReadOnlyList<ItemFullDetailWorkbenchPlacement> Placements => placements.AsReadOnly();
        public IReadOnlyList<ItemDetailArrayModifierDefinition> ArrayModifierCandidates => arrayModifierCandidates.AsReadOnly();
        public string SelectedInstanceId => selectedInstanceId;
        public string SelectedPlacementId => selectedPlacementId;
        public string SelectedMainBuildId => selectedMainBuildId;
        public int SandboxLevel => sandboxLevel;
        public ItemFullDetailWorkbenchSnapshot Snapshot { get; private set; }

        public ItemFullDetailWorkbenchInstance CreateInstance(string baseItemId, string rarityKey, long rootSeed)
        {
            ItemBalanceCandidateDetailResult candidate = candidateAdapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = baseItemId,
                rarityKey = rarityKey,
                rootSeedText = rootSeed.ToString(CultureInfo.InvariantCulture)
            });
            if (!candidate.isSuccess)
            {
                return null;
            }

            ItemFullDetailWorkbenchInstance instance = new(candidate);
            ItemFullDetailWorkbenchInstance existing = instances.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, instance.itemInstanceId, StringComparison.Ordinal));
            if (existing != null)
            {
                selectedInstanceId = existing.itemInstanceId;
                return existing;
            }

            instances.Add(instance);
            instances.Sort((left, right) => string.CompareOrdinal(left.itemInstanceId, right.itemInstanceId));
            selectedInstanceId = instance.itemInstanceId;
            selectedPlacementId = string.Empty;
            Snapshot = RebuildSnapshot();
            return instance;
        }

        public bool TryUpdateArrayModifierCandidate(ItemDetailArrayModifierDefinition definition)
        {
            ItemDetailArrayModifierDefinition clone = ItemDetailArrayModifierResolver.CloneDefinition(definition);
            if (!ItemDetailArrayModifierResolver.IsCandidateDefinitionValid(clone))
            {
                return false;
            }

            int index = arrayModifierCandidates.FindIndex(value =>
                string.Equals(value.modifierId, clone.modifierId, StringComparison.Ordinal));
            if (index >= 0)
            {
                arrayModifierCandidates[index] = clone;
            }
            else
            {
                arrayModifierCandidates.Add(clone);
            }

            arrayModifierCandidates.Sort((left, right) =>
                string.CompareOrdinal(left?.modifierId ?? string.Empty, right?.modifierId ?? string.Empty));
            Snapshot = RebuildSnapshot("ArrayModifierCandidateEdit");
            return true;
        }

        public bool TryRerollSelectedPlacement(string rarityKey, long rootSeed, out ItemFullDetailWorkbenchInstance instance)
        {
            instance = null;
            int placementIndex = placements.FindIndex(value =>
                string.Equals(value.placementId, selectedPlacementId, StringComparison.Ordinal));
            if (placementIndex < 0 || placements[placementIndex].isJuNian)
            {
                return false;
            }

            ItemFullDetailWorkbenchPlacement placement = placements[placementIndex];
            ItemBalanceCandidateDetailResult candidate = candidateAdapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = placement.baseItemId,
                rarityKey = string.IsNullOrWhiteSpace(rarityKey) ? CurrentRarityKey(placement.itemInstanceId) : rarityKey,
                rootSeedText = rootSeed.ToString(CultureInfo.InvariantCulture)
            });
            if (candidate?.isSuccess != true)
            {
                return false;
            }

            ItemFullDetailWorkbenchInstance nextInstance = new(candidate);
            ItemFullDetailWorkbenchInstance existing = instances.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, nextInstance.itemInstanceId, StringComparison.Ordinal));
            if (existing != null)
            {
                nextInstance = existing;
            }
            else
            {
                instances.Add(nextInstance);
            }

            string oldInstanceId = placement.itemInstanceId;
            placements[placementIndex] = placement.ReplaceInstance(nextInstance.itemInstanceId);
            if (!string.Equals(oldInstanceId, nextInstance.itemInstanceId, StringComparison.Ordinal)
                && placements.All(value => !string.Equals(value.itemInstanceId, oldInstanceId, StringComparison.Ordinal)))
            {
                instances.RemoveAll(value => string.Equals(value.itemInstanceId, oldInstanceId, StringComparison.Ordinal));
            }

            instances.Sort((left, right) => string.CompareOrdinal(left.itemInstanceId, right.itemInstanceId));
            selectedInstanceId = nextInstance.itemInstanceId;
            selectedPlacementId = placement.placementId;
            instance = nextInstance;
            Snapshot = RebuildSnapshot("RerollPlacedInstance");
            return true;
        }

        public ItemDetailViewModel PreviewCandidate(string baseItemId, string rarityKey, long rootSeed)
        {
            ItemBalanceCandidateDetailResult candidate = candidateAdapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = baseItemId,
                rarityKey = rarityKey,
                rootSeedText = rootSeed.ToString(CultureInfo.InvariantCulture)
            });
            ItemDetailViewModel model = candidate?.isSuccess == true
                ? candidate.viewModel?.Clone()
                : null;
            ApplySandboxLevelPreview(model);
            ApplySandboxCoreUnlockPreview(model, candidate);
            return model;
        }

        public bool SelectInstance(string itemInstanceId)
        {
            if (!string.Equals(itemInstanceId, JuNianInstanceId, StringComparison.Ordinal)
                && instances.All(value => !string.Equals(value.itemInstanceId, itemInstanceId, StringComparison.Ordinal)))
            {
                return false;
            }

            selectedInstanceId = itemInstanceId ?? string.Empty;
            selectedPlacementId = placements.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, selectedInstanceId, StringComparison.Ordinal))?.placementId ?? string.Empty;
            return true;
        }

        public bool SelectPlacement(string placementId)
        {
            ItemFullDetailWorkbenchPlacement placement = placements.FirstOrDefault(value =>
                string.Equals(value.placementId, placementId, StringComparison.Ordinal));
            if (placement == null)
            {
                return false;
            }

            selectedPlacementId = placement.placementId;
            selectedInstanceId = placement.itemInstanceId;
            return true;
        }

        public ItemFullDetailPlacementPreview PreviewPlacement(
            string baseItemId,
            Vector2Int anchorCell,
            int rotation,
            string ignoredPlacementId = "",
            bool isJuNian = false)
        {
            ItemFullDetailWorkbenchPlacement candidate = new(
                "PREVIEW_ONLY",
                "PREVIEW_ONLY",
                baseItemId,
                anchorCell,
                rotation,
                isJuNian);
            ItemInnerDataDefinition definition = ItemInnerDataCatalog.FindById(baseItemId);
            Vector2Int[] occupied = definition == null
                ? Array.Empty<Vector2Int>()
                : definition.ShapeCells.Select(cell => anchorCell + Rotate(cell, candidate.rotation)).ToArray();
            return new ItemFullDetailPlacementPreview(
                baseItemId,
                anchorCell,
                candidate.rotation,
                occupied,
                EvaluatePlacement(candidate, ignoredPlacementId));
        }

        public bool PlaceSelected(Vector2Int anchorCell)
        {
            return PlaceSelected(anchorCell, out _);
        }

        public bool PlaceSelected(Vector2Int anchorCell, out ItemFullDetailPlacementFailureReason failureReason)
        {
            if (string.IsNullOrWhiteSpace(selectedInstanceId))
            {
                failureReason = ItemFullDetailPlacementFailureReason.NoSelection;
                return false;
            }

            bool isJuNian = string.Equals(selectedInstanceId, JuNianInstanceId, StringComparison.Ordinal);
            if (isJuNian && placements.Any(value => value.isJuNian))
            {
                failureReason = ItemFullDetailPlacementFailureReason.DuplicateJuNian;
                return false;
            }

            if (placements.Any(value => string.Equals(value.itemInstanceId, selectedInstanceId, StringComparison.Ordinal)))
            {
                failureReason = ItemFullDetailPlacementFailureReason.AlreadyPlaced;
                return false;
            }

            string baseItemId = isJuNian
                ? "I031"
                : instances.FirstOrDefault(value => string.Equals(value.itemInstanceId, selectedInstanceId, StringComparison.Ordinal))?.baseItemId;
            if (string.IsNullOrWhiteSpace(baseItemId))
            {
                failureReason = ItemFullDetailPlacementFailureReason.DefinitionMissing;
                return false;
            }

            ItemFullDetailWorkbenchPlacement placement = new(
                NextPlacementId(), selectedInstanceId, baseItemId, anchorCell, 0, isJuNian);
            failureReason = EvaluatePlacement(placement, string.Empty);
            if (failureReason != ItemFullDetailPlacementFailureReason.None)
            {
                return false;
            }

            placements.Add(placement);
            selectedPlacementId = placement.placementId;
            Snapshot = RebuildSnapshot();
            failureReason = ItemFullDetailPlacementFailureReason.None;
            return true;
        }

        public bool MoveSelected(Vector2Int anchorCell)
        {
            return MoveSelected(anchorCell, out _);
        }

        public bool MoveSelected(Vector2Int anchorCell, out ItemFullDetailPlacementFailureReason failureReason)
        {
            int index = placements.FindIndex(value => string.Equals(value.placementId, selectedPlacementId, StringComparison.Ordinal));
            if (index < 0)
            {
                failureReason = ItemFullDetailPlacementFailureReason.PlacementMissing;
                return false;
            }

            ItemFullDetailWorkbenchPlacement moved = placements[index].Move(anchorCell);
            failureReason = EvaluatePlacement(moved, moved.placementId);
            if (failureReason != ItemFullDetailPlacementFailureReason.None)
            {
                return false;
            }

            placements[index] = moved;
            Snapshot = RebuildSnapshot();
            failureReason = ItemFullDetailPlacementFailureReason.None;
            return true;
        }

        public bool RotateSelected()
        {
            return RotateSelected(out _);
        }

        public bool RotateSelected(out ItemFullDetailPlacementFailureReason failureReason)
        {
            int index = placements.FindIndex(value => string.Equals(value.placementId, selectedPlacementId, StringComparison.Ordinal));
            if (index < 0)
            {
                failureReason = ItemFullDetailPlacementFailureReason.PlacementMissing;
                return false;
            }

            ItemFullDetailWorkbenchPlacement rotated = placements[index].RotateClockwise();
            failureReason = EvaluatePlacement(rotated, rotated.placementId);
            if (failureReason != ItemFullDetailPlacementFailureReason.None)
            {
                return false;
            }

            placements[index] = rotated;
            Snapshot = RebuildSnapshot();
            failureReason = ItemFullDetailPlacementFailureReason.None;
            return true;
        }

        public bool RemoveSelected()
        {
            int index = placements.FindIndex(value => string.Equals(value.placementId, selectedPlacementId, StringComparison.Ordinal));
            if (index < 0)
            {
                return false;
            }

            placements.RemoveAt(index);
            selectedPlacementId = string.Empty;
            selectedMainBuildId = string.Empty;
            Snapshot = RebuildSnapshot();
            return true;
        }

        public bool RemoveUnplacedInstance(string itemInstanceId)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceId)
                || placements.Any(value => string.Equals(value.itemInstanceId, itemInstanceId, StringComparison.Ordinal)))
            {
                return false;
            }

            int removed = instances.RemoveAll(value =>
                string.Equals(value.itemInstanceId, itemInstanceId, StringComparison.Ordinal));
            if (removed == 0)
            {
                return false;
            }

            if (string.Equals(selectedInstanceId, itemInstanceId, StringComparison.Ordinal))
            {
                selectedInstanceId = string.Empty;
                selectedPlacementId = string.Empty;
            }

            Snapshot = RebuildSnapshot();
            return true;
        }

        public void SelectJuNian()
        {
            selectedInstanceId = JuNianInstanceId;
            selectedPlacementId = placements.FirstOrDefault(value => value.isJuNian)?.placementId ?? string.Empty;
        }

        public void SetSandboxLevel(int level)
        {
            sandboxLevel = Mathf.Clamp(level, 1, 40);
            Snapshot = RebuildSnapshot();
        }

        public bool SelectMainBuild(string buildId, string source = "UserClick")
        {
            string normalized = string.IsNullOrWhiteSpace(buildId) ? string.Empty : buildId.Trim();
            if (!string.IsNullOrEmpty(normalized)
                && (Snapshot?.build?.FaMenBuilds ?? Array.Empty<ItemBuildTrackResult>())
                    .All(track => !string.Equals(track.buildId, normalized, StringComparison.Ordinal)
                        || track.litItemCount <= 0))
            {
                return false;
            }

            selectedMainBuildId = normalized;
            Snapshot = RebuildSnapshot(source);
            return true;
        }

        public bool LoadValidationLayout(out IReadOnlyList<long> seeds)
        {
            instances.Clear();
            placements.Clear();
            selectedMainBuildId = string.Empty;
            selectedPlacementId = string.Empty;
            placementSequence = 0;
            List<long> found = new();
            string[] baseItemIds = Enumerable.Range(1, 6).Select(index => $"I{index:000}").ToArray();
            for (int itemIndex = 0; itemIndex < baseItemIds.Length; itemIndex++)
            {
                bool accepted = false;
                for (long seed = 610000L; seed < 620000L; seed++)
                {
                    ItemFullDetailWorkbenchInstance instance = CreateInstance(baseItemIds[itemIndex], "orange", seed);
                    if (instance != null
                        && (instance.buildQualification == ItemBuildQualification.FaMenOnly
                            || instance.buildQualification == ItemBuildQualification.Dual))
                    {
                        found.Add(seed);
                        accepted = true;
                        break;
                    }
                    if (instance != null)
                    {
                        RemoveUnplacedInstance(instance.itemInstanceId);
                    }
                }

                if (!accepted)
                {
                    break;
                }
            }

            if (found.Count != 6)
            {
                instances.Clear();
                Snapshot = RebuildSnapshot();
                seeds = Array.AsReadOnly(found.ToArray());
                return false;
            }

            Vector2Int[] cells =
            {
                new(0, 0),
                new(1, 0), new(2, 0), new(4, 0), new(2, 1), new(4, 2), new(2, 3)
            };
            SelectJuNian();
            if (!PlaceSelected(cells[0]))
            {
                seeds = Array.AsReadOnly(found.ToArray());
                return false;
            }

            ItemFullDetailWorkbenchInstance[] ordered = baseItemIds
                .Select(baseItemId => instances.Single(value => string.Equals(value.baseItemId, baseItemId, StringComparison.Ordinal)))
                .ToArray();
            for (int index = 0; index < ordered.Length; index++)
            {
                SelectInstance(ordered[index].itemInstanceId);
                if (!PlaceSelected(cells[index + 1]))
                {
                    seeds = Array.AsReadOnly(found.ToArray());
                    return false;
                }
            }

            selectedMainBuildId = string.Empty;
            Snapshot = RebuildSnapshot();
            seeds = Array.AsReadOnly(found.ToArray());
            return Snapshot.build.FindFaMenBuild("famen:zhenlei")?.litItemCount == 6;
        }

        public ItemDetailViewModel BuildSelectedDetail()
        {
            if (string.Equals(selectedInstanceId, JuNianInstanceId, StringComparison.Ordinal))
            {
                ItemDetailViewModel source = catalogProvider.GetDetailViewModel("I031");
                return ComposePlaced(source, selectedPlacementId, null);
            }

            ItemFullDetailWorkbenchInstance instance = instances.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, selectedInstanceId, StringComparison.Ordinal));
            if (instance == null)
            {
                return null;
            }

            ItemFullDetailWorkbenchPlacement placement = placements.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, instance.itemInstanceId, StringComparison.Ordinal));
            ItemDetailViewModel model = ComposePlaced(instance.Candidate.viewModel, placement?.placementId, instance);
            return model;
        }

        private ItemDetailViewModel ComposePlaced(
            ItemDetailViewModel source,
            string placementId,
            ItemFullDetailWorkbenchInstance instance)
        {
            ItemDetailViewModel model = ItemDetailProjectionComposer.Compose(new ItemDetailProjectionInput
            {
                catalogBaseModel = source,
                contextKind = string.IsNullOrWhiteSpace(placementId)
                    ? ItemDetailProjectionContextKind.GeneratedInstancePreview
                    : ItemDetailProjectionContextKind.PlacedInstance,
                placementId = placementId ?? string.Empty,
                lightingSnapshot = Snapshot.lighting,
                arrayBonusSnapshot = Snapshot.array,
                buildSnapshot = Snapshot.build,
                coreAwakeningSnapshot = Snapshot.awakening,
                skillMonitorSnapshot = Snapshot.skillMonitor
            });
            if (instance == null)
            {
                ApplySandboxLevelPreview(model);
                return model;
            }

            // Keep the generated-instance trace (including formattedValue/candidateRange)
            // and only append placed-state facts below. The generic state composer does not
            // own generation schema facts.
            model.displayDebugSections = instance.Candidate.viewModel.displayDebugSections
                .Select(value => new ItemDetailSectionViewModel(
                    value.title, value.body, value.stateKey, value.keepWhenEmpty))
                .ToList();

            ItemBuildSynergyItemResult buildItem = string.IsNullOrWhiteSpace(placementId)
                ? null
                : Snapshot.build.FindPlacementResult(placementId);
            ItemCoreAwakeningItemResult awakening = string.IsNullOrWhiteSpace(placementId)
                ? null
                : Snapshot.awakening.FindPlacementResult(placementId);
            ItemArrayBonusItemResult arrayItem = string.IsNullOrWhiteSpace(placementId)
                ? null
                : Snapshot.array.FindPlacementResult(placementId);
            IReadOnlyList<ItemDetailResolvedArrayModifier> arrayModifiers =
                ResolveArrayModifiers(instance, arrayItem, buildItem, awakening);
            ApplyGeneratedFactSections(model, instance, buildItem, awakening, arrayModifiers);
            if (awakening == null)
            {
                ApplySandboxCoreUnlockPreview(model, instance.Candidate);
            }
            ItemDetailSectionViewModel hiddenBasicSection = model.displayPlayerSections.FirstOrDefault(value => value != null
                && string.Equals(value.stateKey, "basic", StringComparison.Ordinal));
            string hiddenBasicText = hiddenBasicSection?.body ?? string.Empty;
            model.displayPlayerSections = BuildPlayerSectionsInPrefabSlotOrder(model.displayPlayerSections);
            string validation = Snapshot.ValidationErrors.Count == 0
                ? "None"
                : string.Join(" | ", Snapshot.ValidationErrors);
            if (model.displayDebugSections.Count > 0)
            {
                model.displayDebugSections[0].body +=
                    "\nitemInstanceId: " + instance.itemInstanceId
                    + "\nbaseItemId: " + instance.baseItemId
                    + "\nplacementId: " + (placementId ?? "None")
                    + "\nrarity: " + instance.Projection.rarity
                    + "\ngenerationVersion: " + instance.Projection.generationVersion.ToString(CultureInfo.InvariantCulture)
                    + "\nrootSeed: " + instance.rootSeed.ToString(CultureInfo.InvariantCulture)
                    + "\ncultivationPotentialProfileId: " + instance.Projection.cultivationPotentialProfileId
                    + "\nplayerHiddenPreviewState: 已生成实例 · "
                    + (string.IsNullOrWhiteSpace(placementId) ? "未摆放" : "已摆放")
                    + " · Sandbox only · 不写正式存档"
                    + "\nplayerHiddenItemPower: " + model.displayItemPower;
            }

            if (model.displayDebugSections.Count > 1)
            {
                model.displayDebugSections[1].body +=
                    "\nBuildQualification: " + instance.buildQualification
                    + "\ncountedInBuild: " + (buildItem?.countedInBuild == true)
                    + "\nselectedMainBuildId: " + (string.IsNullOrWhiteSpace(selectedMainBuildId) ? "None" : selectedMainBuildId)
                    + "\nunlockedCoreEffects: " + FormatIds(awakening?.UnlockedCoreEffectIds)
                    + "\nactiveCoreEffects: " + FormatIds(awakening?.ActiveCoreEffectIds)
                    + "\nplayerHiddenBasic: " + OneLine(hiddenBasicText);
            }

            if (model.displayDebugSections.Count > 2)
            {
                model.displayDebugSections[2].body +=
                    "\nvalidationErrors: " + validation
                    + "\nBALANCE_CANDIDATE\nNOT_LIVE_LOCKED\nNOT_BATTLE_CONNECTED";
            }

            ApplySandboxLevelPreview(model);
            return model;
        }

        private void ApplySandboxLevelPreview(ItemDetailViewModel model)
        {
            if (model == null)
            {
                return;
            }

            int level = Mathf.Clamp(sandboxLevel, 1, 40);
            model.statusFlags ??= new ItemDetailStatusFlags();
            model.statusFlags.inputLevel = level;
            model.statusFlags.resolvedLevel = level;
            model.statusFlags.itemLevel = level;
            model.awakeningPreview ??= new ItemAwakeningPreview();
            model.awakeningPreview.inputLevel = level;
            model.awakeningPreview.resolvedLevel = level;
            model.awakeningPreview.itemLevel = level;
        }

        private void ApplySandboxCoreUnlockPreview(
            ItemDetailViewModel model,
            ItemBalanceCandidateDetailResult candidate)
        {
            ItemInstanceProjectionContractSnapshot projection = candidate?.detailProjection?.projection;
            if (model == null || candidate?.isSuccess != true || projection == null)
            {
                return;
            }

            int level = Mathf.Clamp(sandboxLevel, 1, 40);
            HashSet<string> visibleCoreIds = new(projection.VisibleCoreEffectIds, StringComparer.Ordinal);
            ItemDetailTextLine[] displayLines = (candidate.viewModel?.displayCoreEffects
                    ?? new List<ItemDetailTextLine>())
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.title))
                .Take(4)
                .ToArray();
            List<ItemDetailCoreEffectRowState> rowStates = new(displayLines.Length);
            List<string> unlockedIds = new();
            List<string> lockedIds = new();
            List<int> unlockedLevels = new();
            List<int> lockedLevels = new();

            for (int index = 0; index < displayLines.Length; index++)
            {
                string coreEffectId = displayLines[index].stateKey ?? string.Empty;
                int unlockLevel = candidate.CoreUnlockLevels.TryGetValue(coreEffectId, out int configuredLevel)
                    ? Mathf.Clamp(configuredLevel, 1, 40)
                    : FormalCandidateUnlockLevel(
                        projection.baseItemId,
                        coreEffectId);
                bool unlocked = visibleCoreIds.Contains(coreEffectId) && level >= unlockLevel;
                rowStates.Add(unlocked
                    ? ItemDetailCoreEffectRowState.UnlockedInactive
                    : ItemDetailCoreEffectRowState.Locked);
                if (unlocked)
                {
                    unlockedIds.Add(coreEffectId);
                    unlockedLevels.Add(unlockLevel);
                }
                else
                {
                    lockedIds.Add(coreEffectId);
                    lockedLevels.Add(unlockLevel);
                }
            }

            ItemDetailSectionViewModel coreSection = model.displayPlayerSections.FirstOrDefault(value => value != null
                && (string.Equals(value.stateKey, "coreEffect", StringComparison.Ordinal)
                    || string.Equals(value.stateKey, "awakening", StringComparison.Ordinal)));
            if (coreSection != null)
            {
                coreSection.stateKey = "coreEffect";
                coreSection.body = BuildCoreUnlockPreviewFacts(displayLines, rowStates);
                coreSection.coreEffectRowStates = rowStates;
            }

            model.statusFlags ??= new ItemDetailStatusFlags();
            model.statusFlags.coreEffectUnlocked = unlockedIds.Count > 0;
            model.statusFlags.coreEffectActive = false;
            model.statusFlags.unlockedCoreEffectCount = unlockedIds.Count;
            model.statusFlags.activeCoreEffectCount = 0;
            model.statusFlags.currentAwakeningNodeLevel = unlockedLevels.DefaultIfEmpty(0).Max();
            model.statusFlags.nextAwakeningNodeLevel = lockedLevels.DefaultIfEmpty(0).Min();

            model.awakeningPreview ??= new ItemAwakeningPreview();
            model.awakeningPreview.coreEffectUnlocked = unlockedIds.Count > 0;
            model.awakeningPreview.coreEffectActive = false;
            model.awakeningPreview.unlockedCoreEffectCount = unlockedIds.Count;
            model.awakeningPreview.activeCoreEffectCount = 0;
            model.awakeningPreview.currentNodeText = FormatPreviewNodeText("当前开窍", unlockedLevels.DefaultIfEmpty(0).Max());
            model.awakeningPreview.nextNodeText = FormatPreviewNodeText("下一节点", lockedLevels.DefaultIfEmpty(0).Min());
            model.awakeningPreview.unlockedNodeLevelsText = FormatPreviewLevels(unlockedLevels);
            model.awakeningPreview.activeNodeLevelsText = "None";
            model.awakeningPreview.unlockedCoreEffectIdsText = FormatIds(unlockedIds);
            model.awakeningPreview.activeCoreEffectIdsText = "None";
            model.awakeningPreview.lockedCoreEffectIdsText = FormatIds(lockedIds);
        }

        private static int FormalCandidateUnlockLevel(
            string baseItemId,
            string candidateDefinitionId)
        {
            string prefix = "candidate_core_"
                + (baseItemId ?? string.Empty).Trim()
                    .ToLowerInvariant();
            if (string.Equals(
                    candidateDefinitionId,
                    prefix + "_01",
                    StringComparison.Ordinal))
            {
                return 10;
            }
            if (string.Equals(
                    candidateDefinitionId,
                    prefix + "_02",
                    StringComparison.Ordinal))
            {
                return 20;
            }
            if (string.Equals(
                    candidateDefinitionId,
                    prefix + "_03",
                    StringComparison.Ordinal))
            {
                return 30;
            }
            if (string.Equals(
                    candidateDefinitionId,
                    prefix + "_ultimate",
                    StringComparison.Ordinal))
            {
                return 40;
            }
            throw new InvalidOperationException(
                "Unknown formal Candidate core identity: "
                + candidateDefinitionId);
        }

        private static string FormatPreviewNodeText(string label, int level)
        {
            return (label ?? string.Empty) + "：" + (level > 0 ? "Lv." + level : "None");
        }

        private static string FormatPreviewLevels(IEnumerable<int> levels)
        {
            int[] values = (levels ?? Array.Empty<int>()).Distinct().OrderBy(value => value).ToArray();
            return values.Length == 0 ? "None" : string.Join(" / ", values.Select(value => "Lv." + value));
        }

        private static List<ItemDetailSectionViewModel> BuildPlayerSectionsInPrefabSlotOrder(
            IReadOnlyList<ItemDetailSectionViewModel> source)
        {
            ItemDetailSectionViewModel stats = PlayerSectionOrHidden(source, "stats");
            ItemDetailSectionViewModel fixedAffix = PlayerSectionOrHidden(source, "fixedAffix");
            ItemDetailSectionViewModel randomAffix = PlayerSectionOrHidden(source, "randomAffix");
            ItemDetailSectionViewModel orange = PlayerSectionOrHidden(source, "orange");
            ItemDetailSectionViewModel core = PlayerSectionOrHidden(source, "coreEffect", "核心效果", "awakening");
            ItemDetailSectionViewModel faMenBuild = PlayerSectionOrHidden(source, "famenBuild");
            ItemDetailSectionViewModel qiLeiBuild = PlayerSectionOrHidden(source, "qileiBuild");
            ItemDetailSectionViewModel placement = PlayerSectionOrHidden(source, "placement", "推荐摆放");
            ItemDetailSectionViewModel flavor = PlayerSectionOrHidden(source, "flavor", "旧物日记");

            bool firstGroupVisible = AnyVisible(stats, fixedAffix, randomAffix, orange);
            bool coreVisible = IsVisiblePlayerSection(core);
            bool faMenBuildVisible = IsVisiblePlayerSection(faMenBuild);
            bool qiLeiBuildVisible = IsVisiblePlayerSection(qiLeiBuild);
            bool buildGroupVisible = faMenBuildVisible || qiLeiBuildVisible;
            bool tailGroupVisible = AnyVisible(placement, flavor);

            return new List<ItemDetailSectionViewModel>
            {
                stats,
                fixedAffix,
                randomAffix,
                orange,
                firstGroupVisible && coreVisible
                    ? DividerSection("smallDividerBeforeCore", false)
                    : HiddenPlayerSection("smallDividerBeforeCore"),
                core,
                (firstGroupVisible || coreVisible) && buildGroupVisible
                    ? DividerSection("largeDividerBeforeBuild", true)
                    : HiddenPlayerSection("largeDividerBeforeBuild"),
                faMenBuild,
                faMenBuildVisible && qiLeiBuildVisible
                    ? DividerSection("smallDividerBetweenBuilds", false)
                    : HiddenPlayerSection("smallDividerBetweenBuilds"),
                qiLeiBuild,
                buildGroupVisible && tailGroupVisible
                    ? DividerSection("largeDividerBeforePlacement", true)
                    : HiddenPlayerSection("largeDividerBeforePlacement"),
                placement,
                flavor,
                HiddenPlayerSection("trigger"),
                HiddenPlayerSection("basic")
            };
        }

        private static ItemDetailSectionViewModel PlayerSectionOrHidden(
            IReadOnlyList<ItemDetailSectionViewModel> source,
            string outputStateKey,
            string outputTitle = null,
            params string[] alternativeStateKeys)
        {
            ItemDetailSectionViewModel section = FindPlayerSection(source, outputStateKey, alternativeStateKeys);
            if (section == null || IsEmptyPlayerSectionBody(section.body))
            {
                return HiddenPlayerSection(outputStateKey);
            }

            return new ItemDetailSectionViewModel(
                string.IsNullOrWhiteSpace(outputTitle) ? section.title : outputTitle,
                section.body,
                outputStateKey,
                true,
                section.coreEffectRowStates);
        }

        private static ItemDetailSectionViewModel FindPlayerSection(
            IReadOnlyList<ItemDetailSectionViewModel> source,
            string stateKey,
            IReadOnlyList<string> alternativeStateKeys)
        {
            if (source == null)
            {
                return null;
            }

            ItemDetailSectionViewModel match = source.FirstOrDefault(value => value != null
                && string.Equals(value.stateKey, stateKey, StringComparison.Ordinal));
            if (match != null || alternativeStateKeys == null)
            {
                return match;
            }

            foreach (string alternative in alternativeStateKeys)
            {
                match = source.FirstOrDefault(value => value != null
                    && string.Equals(value.stateKey, alternative, StringComparison.Ordinal));
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static ItemDetailSectionViewModel HiddenPlayerSection(string stateKey)
        {
            return new ItemDetailSectionViewModel(string.Empty, string.Empty, stateKey, false);
        }

        private static bool AnyVisible(params ItemDetailSectionViewModel[] sections)
        {
            return sections != null && sections.Any(IsVisiblePlayerSection);
        }

        private static bool IsVisiblePlayerSection(ItemDetailSectionViewModel section)
        {
            return section != null && (!string.IsNullOrWhiteSpace(section.body) || section.keepWhenEmpty);
        }

        private static ItemDetailSectionViewModel DividerSection(string stateKey, bool large)
        {
            return new ItemDetailSectionViewModel(
                string.Empty,
                string.Empty,
                stateKey,
                true);
        }

        private static bool IsEmptyPlayerSectionBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return true;
            }

            string normalized = body.Replace("\r", string.Empty).Trim();
            return string.Equals(normalized, "当前无数据", StringComparison.Ordinal)
                || normalized.All(value => value == '─' || char.IsWhiteSpace(value));
        }

        private void ApplyGeneratedFactSections(
            ItemDetailViewModel model,
            ItemFullDetailWorkbenchInstance instance,
            ItemBuildSynergyItemResult buildItem,
            ItemCoreAwakeningItemResult awakening,
            IReadOnlyList<ItemDetailResolvedArrayModifier> arrayModifiers)
        {
            ApplyStatArrayModifiers(model, arrayModifiers);
            ItemDetailSectionViewModel fixedSection = model.displayPlayerSections.FirstOrDefault(value =>
                string.Equals(value.stateKey, "fixedAffix", StringComparison.Ordinal));
            ItemDetailSectionViewModel randomSection = model.displayPlayerSections.FirstOrDefault(value =>
                string.Equals(value.stateKey, "randomAffix", StringComparison.Ordinal));
            if (fixedSection != null)
            {
                fixedSection.body = BuildTextLineFacts(
                    instance.Candidate.viewModel.displayFixedAffixes,
                    FirstArrayModifier(arrayModifiers, ItemDetailArrayModifierTargetKind.Signature));
            }

            if (randomSection != null)
            {
                randomSection.body = BuildTextLineFacts(
                    instance.Candidate.viewModel.displayRandomAffixes,
                    FirstArrayModifier(arrayModifiers, ItemDetailArrayModifierTargetKind.RandomAffix));
            }

            ItemDetailSectionViewModel awakeningSection = model.displayPlayerSections.FirstOrDefault(value =>
                string.Equals(value.stateKey, "awakening", StringComparison.Ordinal));
            if (awakeningSection != null)
            {
                awakeningSection.title = "核心效果";
                awakeningSection.stateKey = "coreEffect";
                awakeningSection.body = BuildCoreNameFacts(
                    instance.Candidate.viewModel.displayCoreEffects,
                    awakening,
                    arrayModifiers);
                awakeningSection.coreEffectRowStates = BuildCoreRowStates(
                    instance.Candidate.viewModel.displayCoreEffects,
                    awakening);
            }

            ItemBuildTrackResult faMenTrack = ResolveCandidateTrack(instance, buildItem, true);
            ItemBuildTrackResult qiLeiTrack = ResolveCandidateTrack(instance, buildItem, false);
            ReplaceBuildSection(model, "famenBuild", instance.Candidate.viewModel.displayFaMenBuilds,
                faMenTrack, buildItem?.faMenBuildId, instance.buildQualification, instance.Projection.rarity,
                arrayModifiers);
            ReplaceBuildSection(model, "qileiBuild", instance.Candidate.viewModel.displayQiLeiBuilds,
                qiLeiTrack, buildItem?.qiLeiBuildId, instance.buildQualification, instance.Projection.rarity,
                arrayModifiers);
        }

        private IReadOnlyList<ItemDetailResolvedArrayModifier> ResolveArrayModifiers(
            ItemFullDetailWorkbenchInstance instance,
            ItemArrayBonusItemResult arrayItem,
            ItemBuildSynergyItemResult buildItem,
            ItemCoreAwakeningItemResult awakening)
        {
            return ItemDetailArrayModifierResolver.Resolve(new ItemDetailArrayModifierResolutionContext
            {
                baseItemId = instance?.baseItemId ?? string.Empty,
                rarity = instance?.Projection?.rarity ?? ItemInstanceRarity.White,
                isOnArrayBonusCell = arrayItem?.isOnArrayBonusCell == true,
                isArrayBonusActive = arrayItem?.isArrayBonusActive == true,
                activeCoreEffectIds = awakening?.ActiveCoreEffectIds ?? Array.Empty<string>(),
                faMenActiveStagePieceCount = buildItem?.faMenActiveStagePieceCount ?? 0,
                qiLeiActiveStagePieceCount = buildItem?.qiLeiActiveStagePieceCount ?? 0
            }, arrayModifierCandidates);
        }

        private ItemBuildTrackResult ResolveCandidateTrack(
            ItemFullDetailWorkbenchInstance instance,
            ItemBuildSynergyItemResult buildItem,
            bool faMen)
        {
            ItemBuildSynergyResolutionResult build = Snapshot?.build;
            if (build == null)
            {
                return null;
            }

            string directBuildId = faMen ? buildItem?.faMenBuildId : buildItem?.qiLeiBuildId;
            if (!string.IsNullOrWhiteSpace(directBuildId))
            {
                ItemBuildTrackResult direct = faMen
                    ? build.FindFaMenBuild(directBuildId)
                    : build.FindQiLeiBuild(directBuildId);
                if (direct != null)
                {
                    return direct;
                }
            }

            ItemInnerDataDefinition definition = ItemInnerDataCatalog.FindById(instance?.baseItemId);
            string stableTag = faMen ? definition?.FaMenKey : definition?.QiLeiKey;
            if (string.IsNullOrWhiteSpace(stableTag))
            {
                return null;
            }

            IReadOnlyList<ItemBuildTrackResult> tracks = faMen ? build.FaMenBuilds : build.QiLeiBuilds;
            return tracks.FirstOrDefault(track => track != null
                && string.Equals(track.stableTag, stableTag, StringComparison.Ordinal));
        }

        private static void ApplyStatArrayModifiers(
            ItemDetailViewModel model,
            IReadOnlyList<ItemDetailResolvedArrayModifier> arrayModifiers)
        {
            ItemDetailResolvedArrayModifier modifier =
                FirstArrayModifier(arrayModifiers, ItemDetailArrayModifierTargetKind.Stat);
            if (model == null || modifier == null)
            {
                return;
            }

            ItemDetailSectionViewModel statsSection = model.displayPlayerSections.FirstOrDefault(value =>
                value != null && string.Equals(value.stateKey, "stats", StringComparison.Ordinal));
            if (statsSection == null || string.IsNullOrWhiteSpace(statsSection.body))
            {
                return;
            }

            string[] lines = statsSection.body.Replace("\r", string.Empty).Split('\n');
            int targetIndex = Array.FindIndex(lines, line => line.Contains("伤害：", StringComparison.Ordinal));
            if (targetIndex < 0)
            {
                targetIndex = Array.FindIndex(lines, line => !string.IsNullOrWhiteSpace(line));
            }

            if (targetIndex < 0)
            {
                return;
            }

            lines[targetIndex] = ItemDetailArrayModifierResolver.AppendInlineModifier(lines[targetIndex], modifier);
            statsSection.body = string.Join("\n", lines);
        }

        private static string BuildTextLineFacts(
            IReadOnlyList<ItemDetailTextLine> values,
            ItemDetailResolvedArrayModifier arrayModifier = null)
        {
            string[] lines = (values ?? Array.Empty<ItemDetailTextLine>()).Where(value => value != null)
                .Select(value => string.IsNullOrWhiteSpace(value.body)
                    ? "  " + value.title
                    : "  " + value.title + "：" + value.body)
                .ToArray();
            if (lines.Length > 0 && arrayModifier != null)
            {
                lines[0] = ItemDetailArrayModifierResolver.AppendInlineModifier(lines[0], arrayModifier);
            }

            return lines.Length == 0 ? "  数据校验失败" : string.Join("\n", lines);
        }

        private static string BuildCoreNameFacts(
            IReadOnlyList<ItemDetailTextLine> values,
            ItemCoreAwakeningItemResult awakening,
            IReadOnlyList<ItemDetailResolvedArrayModifier> arrayModifiers)
        {
            ItemDetailTextLine[] displayLines = (values ?? Array.Empty<ItemDetailTextLine>())
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.title))
                .Take(4)
                .ToArray();
            Dictionary<string, ItemCoreAwakeningNodeState> nodeByCoreId = (awakening?.NodeStates
                    ?? Array.Empty<ItemCoreAwakeningNodeState>())
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.coreEffectId))
                .GroupBy(node => node.coreEffectId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            if (displayLines.Length > 0)
            {
                string[] rows = displayLines
                    .Select(display =>
                    {
                        string coreEffectId = display.stateKey ?? string.Empty;
                        nodeByCoreId.TryGetValue(coreEffectId, out ItemCoreAwakeningNodeState node);
                        string name = StripRichText(display.title);
                        string detail = !string.IsNullOrWhiteSpace(display.body)
                            ? display.body
                            : string.Empty;
                        string text = string.IsNullOrWhiteSpace(detail) ? name : name + "：" + detail;
                        text = ItemDetailArrayModifierResolver.AppendInlineModifier(
                            text,
                            FirstArrayModifier(
                                arrayModifiers,
                                ItemDetailArrayModifierTargetKind.CoreEffect,
                                coreEffectId));
                        return "  " + (node?.isUnlocked == true ? text : GreyText(text));
                    })
                    .ToArray();
                return rows.Length == 0 ? "  数据校验失败" : string.Join("\n", rows);
            }

            string[] fallback = (awakening?.NodeStates ?? Array.Empty<ItemCoreAwakeningNodeState>())
                .OrderBy(node => node.unlockLevel)
                .Take(4)
                .Select(node =>
                {
                    string text = NonEmpty(node.coreEffectId, node.displayName);
                    return "  " + (node.isUnlocked ? text : GreyText(text));
                })
                .ToArray();
            return fallback.Length == 0 ? "  数据校验失败" : string.Join("\n", fallback);
        }

        private static List<ItemDetailCoreEffectRowState> BuildCoreRowStates(
            IReadOnlyList<ItemDetailTextLine> values,
            ItemCoreAwakeningItemResult awakening)
        {
            Dictionary<string, ItemCoreAwakeningNodeState> nodeByCoreId = (awakening?.NodeStates
                    ?? Array.Empty<ItemCoreAwakeningNodeState>())
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.coreEffectId))
                .GroupBy(node => node.coreEffectId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            ItemDetailTextLine[] displayLines = (values ?? Array.Empty<ItemDetailTextLine>())
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.title))
                .Take(4)
                .ToArray();
            if (displayLines.Length > 0)
            {
                return displayLines.Select(display =>
                {
                    nodeByCoreId.TryGetValue(display.stateKey ?? string.Empty, out ItemCoreAwakeningNodeState node);
                    return node?.isActive == true
                        ? ItemDetailCoreEffectRowState.Active
                        : node?.isUnlocked == true
                            ? ItemDetailCoreEffectRowState.UnlockedInactive
                            : ItemDetailCoreEffectRowState.Locked;
                }).ToList();
            }

            return (awakening?.NodeStates ?? Array.Empty<ItemCoreAwakeningNodeState>())
                .OrderBy(node => node.unlockLevel)
                .Take(4)
                .Select(node => node.isActive
                    ? ItemDetailCoreEffectRowState.Active
                    : node.isUnlocked
                        ? ItemDetailCoreEffectRowState.UnlockedInactive
                        : ItemDetailCoreEffectRowState.Locked)
                .ToList();
        }

        private static string BuildCoreUnlockPreviewFacts(
            IReadOnlyList<ItemDetailTextLine> values,
            IReadOnlyList<ItemDetailCoreEffectRowState> rowStates)
        {
            string[] rows = (values ?? Array.Empty<ItemDetailTextLine>())
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.title))
                .Take(4)
                .Select((display, index) =>
                {
                    string name = StripRichText(display.title);
                    string detail = display.body ?? string.Empty;
                    string text = string.IsNullOrWhiteSpace(detail) ? name : name + "：" + detail;
                    bool unlocked = rowStates != null
                        && index < rowStates.Count
                        && rowStates[index] != ItemDetailCoreEffectRowState.Locked;
                    return "  " + (unlocked ? text : GreyText(text));
                })
                .ToArray();
            return rows.Length == 0 ? "  数据校验失败" : string.Join("\n", rows);
        }

        private static ItemDetailResolvedArrayModifier FirstArrayModifier(
            IReadOnlyList<ItemDetailResolvedArrayModifier> modifiers,
            ItemDetailArrayModifierTargetKind targetKind,
            string targetId = null)
        {
            return (modifiers ?? Array.Empty<ItemDetailResolvedArrayModifier>())
                .FirstOrDefault(value => value != null
                    && value.isVisible
                    && value.targetKind == targetKind
                    && (string.IsNullOrWhiteSpace(targetId)
                        || string.Equals(value.targetId, targetId, StringComparison.Ordinal)));
        }

        private static string OneLine(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "None"
                : value.Replace("\r", string.Empty).Replace("\n", " | ").Trim();
        }

        private string CurrentRarityKey(string itemInstanceId)
        {
            return instances.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, itemInstanceId, StringComparison.Ordinal))?.rarityKey ?? "orange";
        }

        private static string StripRichText(string value)
        {
            return (value ?? string.Empty)
                .Replace("<color=#8A8A8A>", string.Empty)
                .Replace("</color>", string.Empty)
                .Replace("<b>", string.Empty)
                .Replace("</b>", string.Empty)
                .Trim();
        }

        private static string GreyText(string value)
        {
            return "<color=#8A8A8A>" + (value ?? string.Empty) + "</color>";
        }

        private static string NonEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private void ReplaceBuildSection(
            ItemDetailViewModel model,
            string stateKey,
            IReadOnlyList<ItemDetailBuildPreview> candidates,
            ItemBuildTrackResult track,
            string activeBuildId,
            ItemBuildQualification qualification,
            ItemInstanceRarity rarity,
            IReadOnlyList<ItemDetailResolvedArrayModifier> arrayModifiers = null)
        {
            ItemDetailSectionViewModel section = model.displayPlayerSections.FirstOrDefault(value => value != null
                && string.Equals(value.stateKey, stateKey, StringComparison.Ordinal));
            if (section == null) return;
            section.title = string.Equals(stateKey, "famenBuild", StringComparison.Ordinal)
                ? ItemBuildPlayerPresentationFormatter.FaMenSectionTitle
                : ItemBuildPlayerPresentationFormatter.QiLeiSectionTitle;
            int currentCount = track?.litItemCount ?? 0;
            int maxPieceCount = track?.maxPieceCount ?? ResolveBuildMaxPieceCount(candidates);
            bool faMen = string.Equals(stateKey, "famenBuild", StringComparison.Ordinal);
            if (!BuildQualificationAllows(qualification, faMen))
            {
                section.title = string.Empty;
                section.body = string.Empty;
                return;
            }

            section.body = FormatBuildSectionBody(
                faMen, candidates, track, currentCount, maxPieceCount, "  ", rarity, arrayModifiers);
            if (UseExplicitBuildProgressProjection())
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(activeBuildId))
            {
                section.body = "  实例无该类Build资格，不计入当前Build。\n" + BuildCandidateStageRows(candidates, 0);
                return;
            }
            section.body = "  资格：" + activeBuildId + "；点亮计数=" + currentCount
                + "\n" + BuildCandidateStageRows(candidates, currentCount);
        }

        private static bool BuildQualificationAllows(ItemBuildQualification qualification, bool faMen)
        {
            return faMen
                ? qualification == ItemBuildQualification.FaMenOnly || qualification == ItemBuildQualification.Dual
                : qualification == ItemBuildQualification.QiLeiOnly || qualification == ItemBuildQualification.Dual;
        }

        public string BuildTrackEffectSummary(ItemBuildTrackResult track)
        {
            return BuildTrackEffectSummary(track, "    ");
        }

        private string BuildTrackEffectSummary(ItemBuildTrackResult track, string indent)
        {
            if (track == null)
            {
                return indent + "构筑阶段暂无数据。";
            }

            IReadOnlyList<ItemDetailBuildPreview> stages = candidateAdapter.BuildTrackStagePreview(
                track.stableTag,
                track.trackKind == ItemBuildTrackKind.FaMen,
                track.litItemCount);
            return FormatBuildSectionBody(
                track.trackKind == ItemBuildTrackKind.FaMen,
                stages,
                track,
                track.litItemCount,
                track.maxPieceCount,
                indent,
                ItemInstanceRarity.Green);
        }

        private static string BuildCandidateStageRows(IReadOnlyList<ItemDetailBuildPreview> candidates, int count)
        {
            string[] rows = (candidates ?? Array.Empty<ItemDetailBuildPreview>()).Where(value => value != null)
                .OrderBy(ResolveBuildStageThreshold)
                .Select(value => "  " + NonEmpty(value.buildName, FormatBuildStageName(value))
                    + "：\n  " + value.previewText)
                .ToArray();
            return rows.Length == 0 ? "  构筑阶段暂无数据。" : string.Join("\n", rows);
        }

        private static string BuildCandidateStageRowsWithProgress(
            IReadOnlyList<ItemDetailBuildPreview> candidates,
            int count,
            int maxPieceCount)
        {
            string[] rows = (candidates ?? Array.Empty<ItemDetailBuildPreview>()).Where(value => value != null)
                .OrderBy(ResolveBuildStageThreshold)
                .Select(value => "  " + NonEmpty(value.buildName, FormatBuildStageName(value))
                    + "：\n  " + value.previewText)
                .ToArray();
            return rows.Length == 0 ? "  构筑阶段暂无数据。" : string.Join("\n", rows);
        }

        private static string FormatBuildSectionBody(
            bool faMen,
            IReadOnlyList<ItemDetailBuildPreview> candidates,
            ItemBuildTrackResult track,
            int currentCount,
            int maxPieceCount,
            string indent,
            ItemInstanceRarity rarity,
            IReadOnlyList<ItemDetailResolvedArrayModifier> arrayModifiers = null)
        {
            int safeMax = Math.Max(0, maxPieceCount);
            if (safeMax == 0)
            {
                safeMax = ResolveBuildMaxPieceCount(candidates);
            }

            ItemBuildPlayerPresentationStage[] stages =
                (candidates ?? Array.Empty<ItemDetailBuildPreview>())
                .Where(value => value != null)
                .OrderBy(ResolveBuildStageThreshold)
                .Select(value =>
                {
                    int threshold = ResolveBuildStageThreshold(value);
                    bool isActive = currentCount >= threshold;
                    string detail = ItemDetailArrayModifierResolver.AppendInlineModifier(
                        NonEmpty(value.previewText, "未配置"),
                        FindBuildStageArrayModifier(
                            arrayModifiers,
                            faMen,
                            threshold),
                        isActive);
                    return new ItemBuildPlayerPresentationStage(
                        threshold,
                        detail,
                        isActive);
                })
                .ToArray();
            return ItemBuildPlayerPresentationFormatter.FormatTrack(
                faMen,
                track?.stableTag,
                Math.Max(0, currentCount),
                safeMax,
                stages,
                track?.SourceItemIds,
                indent,
                RarityColorHex(rarity),
                faMen && track == null
                    ? candidates?.FirstOrDefault(value => value != null)?.buildName
                    : null);
        }

        private static string FormatFaMenBuildMemberRows(ItemBuildTrackResult track, string indent)
        {
            if (track == null || string.IsNullOrWhiteSpace(track.stableTag))
            {
                return string.Empty;
            }

            HashSet<string> activeItemIds = new(
                track.SourceItemIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            ItemInnerDataDefinition[] members = ItemInnerDataCatalog.AllItems
                .Where(item => item != null
                    && !item.isLightingSource
                    && string.Equals(NormalizeBuildStableTag(item.FaMenKey), NormalizeBuildStableTag(track.stableTag), StringComparison.Ordinal))
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            if (members.Length == 0)
            {
                return string.Empty;
            }

            return string.Join("\n", members.Select(item =>
            {
                bool active = activeItemIds.Contains(item.itemId);
                string color = active ? BuildActiveHex : BuildInactiveHex;
                return indent + BuildColoredText("-" + item.displayName, color, active);
            }));
        }

        private static string NormalizeBuildStableTag(string value)
        {
            string text = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (text.StartsWith("famen:", StringComparison.Ordinal))
            {
                text = text.Substring("famen:".Length);
            }
            else if (text.StartsWith("qilei:", StringComparison.Ordinal))
            {
                text = text.Substring("qilei:".Length);
            }

            int colon = text.IndexOf(':');
            return colon >= 0 ? text.Substring(0, colon) : text;
        }

        private static string FormatBuildStageRow(
            ItemDetailBuildPreview value,
            bool faMen,
            int currentCount,
            ItemInstanceRarity rarity,
            string indent,
            IReadOnlyList<ItemDetailResolvedArrayModifier> arrayModifiers)
        {
            int threshold = ResolveBuildStageThreshold(value);
            bool isActive = currentCount >= threshold;
            string label = FormatBuildStageName(value);
            string detailText = value?.previewText;
            if (!faMen
                && !string.IsNullOrWhiteSpace(value?.buildName)
                && !string.Equals(value.buildName, label, StringComparison.Ordinal))
            {
                detailText = value.buildName + "：" + NonEmpty(detailText, "未配置");
            }
            string color = isActive ? BuildActiveHex : BuildInactiveHex;
            return indent + BuildColoredText(label + "：", color, true)
                + "\n" + indent + FormatBuildStageDetail(
                    detailText,
                    isActive,
                    rarity,
                    FindBuildStageArrayModifier(arrayModifiers, faMen, threshold));
        }

        private static string FormatBuildStageDetail(
            string value,
            bool isActive,
            ItemInstanceRarity rarity,
            ItemDetailResolvedArrayModifier arrayModifier = null)
        {
            string text = NonEmpty(value, "未配置");
            if (!isActive)
            {
                return BuildColoredText(
                    ItemDetailArrayModifierResolver.AppendInlineModifier(text, arrayModifier, false),
                    BuildInactiveHex,
                    false);
            }

            return BuildColoredText(
                ItemDetailArrayModifierResolver.AppendInlineModifier(
                    HighlightBuildCoreValue(text, RarityColorHex(rarity)),
                    arrayModifier),
                BuildActiveHex,
                false);
        }

        private static ItemDetailResolvedArrayModifier FindBuildStageArrayModifier(
            IReadOnlyList<ItemDetailResolvedArrayModifier> modifiers,
            bool faMen,
            int threshold)
        {
            ItemDetailArrayModifierTargetKind targetKind = faMen
                ? ItemDetailArrayModifierTargetKind.FaMenBuildStage
                : ItemDetailArrayModifierTargetKind.QiLeiBuildStage;
            string suffix = ":" + threshold.ToString(CultureInfo.InvariantCulture);
            return (modifiers ?? Array.Empty<ItemDetailResolvedArrayModifier>())
                .FirstOrDefault(value => value != null
                    && value.isVisible
                    && value.targetKind == targetKind
                    && (value.targetId ?? string.Empty).EndsWith(suffix, StringComparison.Ordinal));
        }

        private static string HighlightBuildCoreValue(string value, string colorHex)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value ?? string.Empty;
            }

            StringBuilder builder = new(value.Length + 32);
            bool highlighted = false;
            for (int index = 0; index < value.Length;)
            {
                if (!IsBuildValueStart(value, index))
                {
                    builder.Append(value[index]);
                    index++;
                    continue;
                }

                int end = index + 1;
                while (end < value.Length && !IsBuildValueTerminator(value[end]))
                {
                    end++;
                }

                builder.Append(BuildColoredText(value.Substring(index, end - index), colorHex, true));
                highlighted = true;
                index = end;
            }

            return highlighted ? builder.ToString() : value;
        }

        private static bool IsBuildValueStart(string value, int index)
        {
            char current = value[index];
            if (current == '+' || current == '-' || current == '×')
            {
                return index + 1 < value.Length && char.IsDigit(value[index + 1]);
            }

            return char.IsDigit(current)
                && (index == 0 || char.IsWhiteSpace(value[index - 1]));
        }

        private static bool IsBuildValueTerminator(char value)
        {
            return char.IsWhiteSpace(value)
                || value == '；'
                || value == ';'
                || value == '，'
                || value == ','
                || value == '。'
                || value == ')'
                || value == '）';
        }

        private static string BuildColoredText(string value, string colorHex, bool bold)
        {
            string text = value ?? string.Empty;
            if (bold)
            {
                text = "<b>" + text + "</b>";
            }

            return "<color=#" + colorHex + ">" + text + "</color>";
        }

        private static string RarityColorHex(ItemInstanceRarity rarity)
        {
            return rarity switch
            {
                ItemInstanceRarity.White => "E3D8C3",
                ItemInstanceRarity.Green => "87B66A",
                ItemInstanceRarity.Blue => "68A9E6",
                ItemInstanceRarity.Purple => "B27DDF",
                ItemInstanceRarity.Orange => "E4A14B",
                _ => "D8CCB7"
            };
        }

        private static string FormatBuildStageState(int currentCount, int stagePieceCount)
        {
            return currentCount >= stagePieceCount ? "已激活" : "未激活";
        }

        private static bool UseExplicitBuildProgressProjection()
        {
            return true;
        }

        private static int ResolveBuildMaxPieceCount(IReadOnlyList<ItemDetailBuildPreview> candidates)
        {
            int max = 0;
            foreach (ItemDetailBuildPreview candidate in candidates ?? Array.Empty<ItemDetailBuildPreview>())
            {
                max = Math.Max(max, ResolveBuildStageThreshold(candidate));
            }
            return max;
        }

        private static string FormatBuildStageName(ItemDetailBuildPreview preview)
        {
            int threshold = ResolveBuildStageThreshold(preview);
            return threshold > 0
                ? threshold.ToString(CultureInfo.InvariantCulture) + "件效果"
                : preview?.buildName ?? "构筑效果";
        }

        private static int ResolveBuildStageThreshold(ItemDetailBuildPreview preview)
        {
            if (preview == null)
            {
                return 0;
            }

            if (TryParseBuildProgress(preview.progressText, out _, out int max)
                && max > 0)
            {
                return max;
            }

            string name = preview.buildName ?? string.Empty;
            if (name.EndsWith("6", StringComparison.Ordinal)
                || name.Contains("Build6", StringComparison.Ordinal))
            {
                return 6;
            }
            if (name.EndsWith("4", StringComparison.Ordinal)
                || name.Contains("Build4", StringComparison.Ordinal))
            {
                return 4;
            }
            return 2;
        }

        private static bool TryParseBuildProgress(string progressText, out int current, out int max)
        {
            current = 0;
            max = 0;
            string[] parts = (progressText ?? string.Empty).Split('/');
            return parts.Length >= 2
                && int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out current)
                && int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out max);
        }

        private static string FormatBuildProgress(int currentCount, int maxPieceCount)
        {
            return currentCount.ToString(CultureInfo.InvariantCulture)
                + "/"
                + maxPieceCount.ToString(CultureInfo.InvariantCulture);
        }

        private static string BuildAffixFactText(
            ItemFullDetailWorkbenchInstance instance,
            string slotKind,
            IReadOnlyList<ItemDetailTextLine> displayLines,
            string emptyText)
        {
            ItemInstanceProjectionAffixSnapshot[] facts = instance.Projection.Affixes
                .Where(value => string.Equals(value.slotKind.ToString(), slotKind, StringComparison.Ordinal))
                .ToArray();
            if (facts.Length == 0)
            {
                return "  " + emptyText;
            }

            List<string> lines = new();
            for (int index = 0; index < facts.Length; index++)
            {
                ItemInstanceProjectionAffixSnapshot fact = facts[index];
                ItemDetailTextLine display = displayLines != null && index < displayLines.Count
                    ? displayLines[index]
                    : null;
                string title = string.IsNullOrWhiteSpace(display?.title) ? "未命名词条" : display.title;
                string value = string.IsNullOrWhiteSpace(display?.body)
                    ? "显示值不可用"
                    : display.body;
                lines.Add("  " + title + "：" + value);
            }

            return string.Join("\n", lines);
        }

        private ItemFullDetailWorkbenchSnapshot RebuildSnapshot(string selectionSource = "UserClick")
        {
            ItemSystemPlacementInput[] inputs = placements
                .Select(value => new ItemSystemPlacementInput(value.placementId, value.baseItemId, value.anchorCell, value.rotation))
                .ToArray();
            ItemSystemSnapshot system = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(
                new ItemSystemSnapshotInput(inputs, catalogItems: catalog));
            ItemLightingResolutionResult lighting = system.ToLightingResolutionResult();
            ItemArrayBonusResolutionResult array = system.ToArrayBonusResolutionResult();
            Dictionary<string, ItemBuildQualification> qualifications = placements
                .Where(value => !value.isJuNian)
                .ToDictionary(
                    value => value.placementId,
                    value => instances.First(instance => string.Equals(instance.itemInstanceId, value.itemInstanceId, StringComparison.Ordinal)).buildQualification,
                    StringComparer.Ordinal);
            ItemBuildSynergyResolutionResult build =
                ItemFullDetailQualifiedBuildAdapter.Resolve(system, qualifications);
            if (!string.IsNullOrWhiteSpace(selectedMainBuildId)
                && build.FaMenBuilds.All(track => !string.Equals(track.buildId, selectedMainBuildId, StringComparison.Ordinal)
                    || track.litItemCount <= 0))
            {
                selectedMainBuildId = string.Empty;
            }
            Dictionary<string, ItemFullDetailWorkbenchInstance> instanceByPlacement = placements
                .Where(value => !value.isJuNian)
                .ToDictionary(
                    value => value.placementId,
                    value => instances.First(instance => string.Equals(instance.itemInstanceId, value.itemInstanceId, StringComparison.Ordinal)),
                    StringComparer.Ordinal);
            ItemCoreAwakeningResolutionResult awakening = ItemFullDetailCandidateAwakeningAdapter.Resolve(
                lighting, instanceByPlacement, sandboxLevel);
            ItemMainBuildSelectionInput selection = string.IsNullOrWhiteSpace(selectedMainBuildId)
                ? ItemMainBuildSelectionInput.None("ExplicitNone")
                : new ItemMainBuildSelectionInput(selectedMainBuildId, selectionSource, 1);
            ItemSkillMonitorResolutionResult skill = ItemSkillMonitorResolver.Resolve(build, selection);
            List<string> errors = system.validationErrors.Select(value => value.ToDiagnosticString()).ToList();
            errors.AddRange(build.ValidationErrors);
            errors.AddRange(awakening.ValidationErrors);
            errors.AddRange(skill.ValidationErrors);
            return new ItemFullDetailWorkbenchSnapshot(system, lighting, array, build, awakening, skill, errors);
        }

        private bool IsLegalPlacement(ItemFullDetailWorkbenchPlacement candidate, string ignoredPlacementId)
        {
            return EvaluatePlacement(candidate, ignoredPlacementId) == ItemFullDetailPlacementFailureReason.None;
        }

        private ItemFullDetailPlacementFailureReason EvaluatePlacement(
            ItemFullDetailWorkbenchPlacement candidate,
            string ignoredPlacementId)
        {
            ItemInnerDataDefinition definition = ItemInnerDataCatalog.FindById(candidate.baseItemId);
            if (definition == null)
            {
                return ItemFullDetailPlacementFailureReason.DefinitionMissing;
            }

            if (candidate.rotation % 90 != 0)
            {
                return ItemFullDetailPlacementFailureReason.InvalidRotation;
            }

            if (!candidate.isJuNian && placements.Any(value => !value.isJuNian
                && string.Equals(value.baseItemId, candidate.baseItemId, StringComparison.Ordinal)
                && !string.Equals(value.placementId, ignoredPlacementId, StringComparison.Ordinal)))
            {
                return ItemFullDetailPlacementFailureReason.DuplicateBaseItemPlaced;
            }

            Vector2Int[] occupied = definition.ShapeCells
                .Select(cell => candidate.anchorCell + Rotate(cell, candidate.rotation))
                .ToArray();
            if (occupied.Any(cell => !ItemGridPlacementRulePreview.IsWithinBoard(cell)))
            {
                return ItemFullDetailPlacementFailureReason.OutOfBounds;
            }

            if (occupied.Contains(ItemGridPlacementRulePreview.EyeCell))
            {
                return ItemFullDetailPlacementFailureReason.EyeCellBlocked;
            }

            HashSet<Vector2Int> locked = new();
            foreach (ItemFullDetailWorkbenchPlacement placement in placements)
            {
                if (string.Equals(placement.placementId, ignoredPlacementId, StringComparison.Ordinal))
                {
                    continue;
                }

                ItemInnerDataDefinition placedDefinition = ItemInnerDataCatalog.FindById(placement.baseItemId);
                foreach (Vector2Int cell in placedDefinition.ShapeCells)
                {
                    locked.Add(placement.anchorCell + Rotate(cell, placement.rotation));
                }
            }

            if (occupied.Any(locked.Contains))
            {
                return ItemFullDetailPlacementFailureReason.CellOccupied;
            }

            if (candidate.isJuNian && placements.Any(value => value.isJuNian
                && !string.Equals(value.placementId, ignoredPlacementId, StringComparison.Ordinal)))
            {
                return ItemFullDetailPlacementFailureReason.DuplicateJuNian;
            }

            return ItemFullDetailPlacementFailureReason.None;
        }

        private string NextPlacementId()
        {
            placementSequence++;
            return "P_WORKBENCH_" + placementSequence.ToString("D4", CultureInfo.InvariantCulture);
        }

        private static Vector2Int Rotate(Vector2Int cell, int rotation)
        {
            return rotation switch
            {
                90 => new Vector2Int(-cell.y, cell.x),
                180 => new Vector2Int(-cell.x, -cell.y),
                270 => new Vector2Int(cell.y, -cell.x),
                _ => cell
            };
        }

        private static string FormatIds(IReadOnlyList<string> ids)
        {
            return ids == null || ids.Count == 0 ? "None" : string.Join("|", ids);
        }

        private static ItemInnerDataDefinition CloneCatalogItem(ItemInnerDataDefinition item)
        {
            return new ItemInnerDataDefinition
            {
                itemId = item.itemId,
                displayName = item.displayName,
                itemFamily = item.itemFamily,
                faMenTag = item.faMenTag,
                qiLeiTag = item.qiLeiTag,
                shapeId = item.shapeId,
                defaultLocalCells = item.defaultLocalCells.ToList(),
                coreCellLocal = item.coreCellLocal,
                displayRarityName = item.displayRarityName,
                rarityDefault = item.rarityDefault,
                allowedRarities = item.allowedRarities.ToList(),
                basePowerText = item.basePowerText,
                itemPower = item.itemPower,
                primaryStats = item.primaryStats.Select(value => new ItemInnerStatLine(value.label, value.value, value.hint)).ToList(),
                triggerText = item.triggerText,
                basicEffectText = item.basicEffectText,
                coreEffectPreviewText = item.coreEffectPreviewText,
                awakeningPreview = item.awakeningPreview,
                fixedAffixPreview = item.fixedAffixPreview,
                randomAffixPreview = item.randomAffixPreview,
                orangeAffixPreview = item.orangeAffixPreview,
                placementHint = item.placementHint,
                flavorText = item.flavorText,
                iconPlaceholderKey = item.iconPlaceholderKey,
                isLightingSource = item.isLightingSource
            };
        }
    }
}
