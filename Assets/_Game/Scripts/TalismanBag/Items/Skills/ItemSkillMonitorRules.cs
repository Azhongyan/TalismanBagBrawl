using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Build;

namespace TalismanBag.Items.Skills
{
    public interface IItemMainBuildSelectionProvider
    {
        ItemMainBuildSelectionInput GetMainBuildSelectionInput();
    }

    public interface IItemSkillMonitorSnapshotProvider
    {
        ItemSkillMonitorResolutionResult GetItemSkillMonitorSnapshot();
    }

    public enum ItemSkillMonitorSlotType
    {
        BasicAttack,
        Build2,
        Build4,
        Build6
    }

    public enum ItemSkillTriggerKind
    {
        PassiveTick,
        Condition,
        Cooldown,
        ChargeFull
    }

    public sealed class ItemMainBuildSelectionInput
    {
        public ItemMainBuildSelectionInput(
            string selectedMainBuildId,
            string selectionSource,
            int selectionRevision)
        {
            this.selectedMainBuildId = selectedMainBuildId ?? string.Empty;
            this.selectionSource = selectionSource ?? string.Empty;
            this.selectionRevision = Math.Max(0, selectionRevision);
        }

        public string selectedMainBuildId;
        public string selectionSource;
        public int selectionRevision;

        public static ItemMainBuildSelectionInput None(string source = "None")
        {
            return new ItemMainBuildSelectionInput(string.Empty, source, 0);
        }
    }

    public sealed class ItemSkillMonitorSlotSnapshot
    {
        public ItemSkillMonitorSlotSnapshot(
            ItemSkillMonitorSlotType slotType,
            int slotOrder,
            string displayName,
            string sourceBuildId,
            string sourceFaMenTag,
            int requiredBuildStage,
            ItemSkillTriggerKind triggerKind,
            bool isUnlocked,
            bool isMonitoring,
            string iconKey,
            string tooltipText,
            string presentationCueId,
            string chibiActionKey)
        {
            this.slotType = slotType;
            this.slotOrder = slotOrder;
            this.displayName = displayName ?? string.Empty;
            this.sourceBuildId = sourceBuildId ?? string.Empty;
            this.sourceFaMenTag = sourceFaMenTag ?? string.Empty;
            this.requiredBuildStage = Math.Max(0, requiredBuildStage);
            this.triggerKind = triggerKind;
            this.isUnlocked = isUnlocked;
            this.isMonitoring = isMonitoring;
            isTriggered = false;
            cooldown01 = 0f;
            charge01 = 0f;
            this.iconKey = iconKey ?? string.Empty;
            this.tooltipText = tooltipText ?? string.Empty;
            this.presentationCueId = presentationCueId ?? string.Empty;
            this.chibiActionKey = chibiActionKey ?? string.Empty;
        }

        public ItemSkillMonitorSlotType slotType;
        public int slotOrder;
        public string displayName;
        public string sourceBuildId;
        public string sourceFaMenTag;
        public int requiredBuildStage;
        public ItemSkillTriggerKind triggerKind;
        public bool isUnlocked;
        public bool isMonitoring;
        public bool isTriggered;
        public float cooldown01;
        public float charge01;
        public string iconKey;
        public string tooltipText;
        public string presentationCueId;
        public string chibiActionKey;
    }

    public sealed class ItemSkillMonitorResolutionResult
    {
        private readonly ItemSkillMonitorSlotSnapshot[] slots;
        private readonly string[] validationErrors;

        public ItemSkillMonitorResolutionResult(
            string selectedMainBuildId,
            bool selectedMainBuildFound,
            int selectedMainBuildCount,
            int selectedMainBuildStage,
            IReadOnlyList<ItemSkillMonitorSlotSnapshot> slots,
            IReadOnlyList<string> validationErrors)
        {
            this.selectedMainBuildId = selectedMainBuildId ?? string.Empty;
            this.selectedMainBuildFound = selectedMainBuildFound;
            this.selectedMainBuildCount = Math.Max(0, selectedMainBuildCount);
            this.selectedMainBuildStage = Math.Max(0, selectedMainBuildStage);
            this.slots = (slots ?? Array.Empty<ItemSkillMonitorSlotSnapshot>())
                .OrderBy(slot => slot.slotOrder)
                .ToArray();
            this.validationErrors = (validationErrors ?? Array.Empty<string>())
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        public string selectedMainBuildId;
        public bool selectedMainBuildFound;
        public int selectedMainBuildCount;
        public int selectedMainBuildStage;
        public IReadOnlyList<ItemSkillMonitorSlotSnapshot> Slots => slots;
        public IReadOnlyList<string> ValidationErrors => validationErrors;

        public ItemSkillMonitorSlotSnapshot FindSlot(ItemSkillMonitorSlotType slotType)
        {
            return slots.FirstOrDefault(slot => slot.slotType == slotType);
        }
    }

    public static class ItemSkillMonitorResolver
    {
        public const string BuildIdPrefix = "famen:";
        public const string QiLeiBuildIdPrefix = "qilei:";

        private static readonly ItemSkillMonitorSlotType[] FixedSlotOrder =
        {
            ItemSkillMonitorSlotType.BasicAttack,
            ItemSkillMonitorSlotType.Build2,
            ItemSkillMonitorSlotType.Build4,
            ItemSkillMonitorSlotType.Build6
        };

        public static ItemSkillMonitorResolutionResult Resolve(
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemMainBuildSelectionInput selectionInput)
        {
            selectionInput ??= ItemMainBuildSelectionInput.None("MissingSelectionInput");
            string selectedMainBuildId = Normalize(selectionInput.selectedMainBuildId);
            List<string> validationErrors = new();

            if (string.IsNullOrWhiteSpace(selectedMainBuildId))
            {
                return new ItemSkillMonitorResolutionResult(
                    string.Empty,
                    false,
                    0,
                    0,
                    BuildSlots(null),
                    validationErrors);
            }

            ItemBuildTrackResult selectedTrack = ResolveSelectedTrack(
                buildSnapshot,
                selectedMainBuildId,
                validationErrors);

            bool found = selectedTrack != null && validationErrors.Count == 0;
            return new ItemSkillMonitorResolutionResult(
                selectedMainBuildId,
                found,
                found ? selectedTrack.litItemCount : 0,
                found ? selectedTrack.activeStagePieceCount : 0,
                BuildSlots(found ? selectedTrack : null),
                validationErrors);
        }

        public static string FormatOverview(ItemSkillMonitorResolutionResult result)
        {
            if (result == null)
            {
                return "Skill monitor preview: no snapshot.";
            }

            string selected = string.IsNullOrWhiteSpace(result.selectedMainBuildId)
                ? "None"
                : result.selectedMainBuildId;
            string slots = string.Join(
                "; ",
                result.Slots.Select(slot =>
                    $"{slot.displayName}:{(slot.isMonitoring ? "Monitoring" : "Locked")} unlocked={slot.isUnlocked} trigger={slot.isTriggered}"));
            string errors = FormatValidationErrors(result.ValidationErrors);
            return $"Skill monitor preview: selectedMainBuildId={selected}; found={result.selectedMainBuildFound}; count={result.selectedMainBuildCount}; stage={ItemBuildTrackResult.BuildStageLabel(result.selectedMainBuildStage)}; slots={slots}; validationErrors={errors}.";
        }

        public static string BuildStableCueKey(string sourceFaMenTag, ItemSkillMonitorSlotType slotType)
        {
            string tag = string.IsNullOrWhiteSpace(sourceFaMenTag)
                ? "none"
                : sourceFaMenTag.Trim();
            return $"{tag}_{BuildSlotKey(slotType)}";
        }

        public static string FormatValidationErrors(IReadOnlyList<string> validationErrors)
        {
            return validationErrors == null || validationErrors.Count == 0
                ? "None"
                : string.Join(" | ", validationErrors);
        }

        private static ItemBuildTrackResult ResolveSelectedTrack(
            ItemBuildSynergyResolutionResult buildSnapshot,
            string selectedMainBuildId,
            List<string> validationErrors)
        {
            if (!IsLegalBuildIdFormat(selectedMainBuildId))
            {
                validationErrors.Add($"selectedMainBuildId '{selectedMainBuildId}' format is invalid; expected famen:<stableKey>.");
                return null;
            }

            if (selectedMainBuildId.StartsWith(QiLeiBuildIdPrefix, StringComparison.Ordinal))
            {
                validationErrors.Add($"selectedMainBuildId '{selectedMainBuildId}' points to qiLei Build; only faMen Build can be main Build.");
                return null;
            }

            if (buildSnapshot == null)
            {
                validationErrors.Add("Build snapshot is null; emitted four locked skill monitor slots.");
                return null;
            }

            List<ItemBuildTrackResult> matches = (buildSnapshot.FaMenBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Where(track => track != null && string.Equals(track.buildId, selectedMainBuildId, StringComparison.Ordinal))
                .ToList();

            if (matches.Count == 0)
            {
                validationErrors.Add($"selectedMainBuildId '{selectedMainBuildId}' was not found in faMen Build snapshot.");
                return null;
            }

            if (matches.Count > 1)
            {
                validationErrors.Add($"duplicate faMen build track '{selectedMainBuildId}' found in Build snapshot; main Build selection is locked.");
                return null;
            }

            return matches[0];
        }

        private static IReadOnlyList<ItemSkillMonitorSlotSnapshot> BuildSlots(ItemBuildTrackResult selectedTrack)
        {
            List<ItemSkillMonitorSlotSnapshot> slots = new();
            for (int i = 0; i < FixedSlotOrder.Length; i++)
            {
                ItemSkillMonitorSlotType slotType = FixedSlotOrder[i];
                bool unlocked = IsSlotUnlocked(slotType, selectedTrack);
                bool hasMainBuild = selectedTrack != null;
                string sourceBuildId = hasMainBuild ? selectedTrack.buildId : string.Empty;
                string sourceFaMenTag = hasMainBuild ? selectedTrack.stableTag : string.Empty;
                string cueKey = BuildStableCueKey(sourceFaMenTag, slotType);
                slots.Add(new ItemSkillMonitorSlotSnapshot(
                    slotType,
                    i,
                    BuildDisplayName(slotType),
                    sourceBuildId,
                    sourceFaMenTag,
                    RequiredStage(slotType),
                    TriggerKind(slotType),
                    unlocked,
                    hasMainBuild && unlocked,
                    cueKey,
                    BuildTooltip(slotType, selectedTrack),
                    cueKey,
                    cueKey));
            }

            return slots;
        }

        private static bool IsSlotUnlocked(ItemSkillMonitorSlotType slotType, ItemBuildTrackResult selectedTrack)
        {
            if (selectedTrack == null)
            {
                return false;
            }

            return slotType switch
            {
                ItemSkillMonitorSlotType.BasicAttack => selectedTrack.litItemCount >= 1,
                ItemSkillMonitorSlotType.Build2 => selectedTrack.build2Active,
                ItemSkillMonitorSlotType.Build4 => selectedTrack.build4Active,
                ItemSkillMonitorSlotType.Build6 => selectedTrack.build6Active,
                _ => false
            };
        }

        private static int RequiredStage(ItemSkillMonitorSlotType slotType)
        {
            return slotType switch
            {
                ItemSkillMonitorSlotType.BasicAttack => 1,
                ItemSkillMonitorSlotType.Build2 => 2,
                ItemSkillMonitorSlotType.Build4 => 4,
                ItemSkillMonitorSlotType.Build6 => 6,
                _ => 0
            };
        }

        private static ItemSkillTriggerKind TriggerKind(ItemSkillMonitorSlotType slotType)
        {
            return slotType switch
            {
                ItemSkillMonitorSlotType.BasicAttack => ItemSkillTriggerKind.PassiveTick,
                ItemSkillMonitorSlotType.Build2 => ItemSkillTriggerKind.Condition,
                ItemSkillMonitorSlotType.Build4 => ItemSkillTriggerKind.Cooldown,
                ItemSkillMonitorSlotType.Build6 => ItemSkillTriggerKind.ChargeFull,
                _ => ItemSkillTriggerKind.Condition
            };
        }

        private static string BuildDisplayName(ItemSkillMonitorSlotType slotType)
        {
            return slotType switch
            {
                ItemSkillMonitorSlotType.BasicAttack => "普攻",
                ItemSkillMonitorSlotType.Build2 => "Build2",
                ItemSkillMonitorSlotType.Build4 => "Build4",
                ItemSkillMonitorSlotType.Build6 => "Build6",
                _ => slotType.ToString()
            };
        }

        private static string BuildTooltip(ItemSkillMonitorSlotType slotType, ItemBuildTrackResult selectedTrack)
        {
            string buildName = selectedTrack == null ? "None" : selectedTrack.displayName;
            return $"{BuildDisplayName(slotType)} preview for selected main Build {buildName}; contract-only, no skill trigger, cooldown, charge, damage, heal, shield, status, animation, or battle settlement.";
        }

        private static string BuildSlotKey(ItemSkillMonitorSlotType slotType)
        {
            return slotType switch
            {
                ItemSkillMonitorSlotType.BasicAttack => "basic_attack",
                ItemSkillMonitorSlotType.Build2 => "build2",
                ItemSkillMonitorSlotType.Build4 => "build4",
                ItemSkillMonitorSlotType.Build6 => "build6",
                _ => slotType.ToString().ToLowerInvariant()
            };
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static bool IsLegalBuildIdFormat(string selectedMainBuildId)
        {
            if (string.IsNullOrWhiteSpace(selectedMainBuildId))
            {
                return true;
            }

            if (selectedMainBuildId.StartsWith(QiLeiBuildIdPrefix, StringComparison.Ordinal))
            {
                return true;
            }

            if (!selectedMainBuildId.StartsWith(BuildIdPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            string tag = selectedMainBuildId.Substring(BuildIdPrefix.Length);
            return tag.Length > 0 && tag.All(character =>
                character >= 'a' && character <= 'z'
                || character >= '0' && character <= '9'
                || character == '_');
        }
    }
}
