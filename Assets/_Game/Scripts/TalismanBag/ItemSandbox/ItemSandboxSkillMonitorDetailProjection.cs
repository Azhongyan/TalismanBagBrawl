using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Skills;

namespace TalismanBag.ItemSandbox
{
    public static class ItemSandboxSkillMonitorDetailProjection
    {
        public static ItemDetailViewModel Project(
            ItemDetailViewModel model,
            ItemSkillMonitorResolutionResult skillMonitorResult)
        {
            if (model == null || skillMonitorResult == null)
            {
                return model;
            }

            model.skillMonitorPreview.selectedMainBuildId = string.IsNullOrWhiteSpace(skillMonitorResult.selectedMainBuildId)
                ? "None"
                : skillMonitorResult.selectedMainBuildId;
            model.skillMonitorPreview.selectedMainBuildFound = skillMonitorResult.selectedMainBuildFound;
            model.skillMonitorPreview.selectedMainBuildCountText = skillMonitorResult.selectedMainBuildCount.ToString();
            model.skillMonitorPreview.selectedMainBuildStageText = ItemBuildTrackResult.BuildStageLabel(skillMonitorResult.selectedMainBuildStage);
            model.skillMonitorPreview.validationErrorsText = ItemSkillMonitorResolver.FormatValidationErrors(skillMonitorResult.ValidationErrors);
            model.skillMonitorPreview.previewText = ItemSkillMonitorResolver.FormatOverview(skillMonitorResult);
            model.skillMonitorPreview.slotPreviewLines = BuildSlotLines(skillMonitorResult);

            model.displayPrimaryStats.Add(new ItemDetailStatLine(
                "skillMonitorSelectedMainBuild",
                model.skillMonitorPreview.selectedMainBuildId,
                "Explicit Sandbox Preview input only; BuildSynergyCore selectedMainBuildId stays reserved."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine(
                "skillMonitorMainBuildFound",
                skillMonitorResult.selectedMainBuildFound ? "true" : "false",
                "Only faMen Build ids can drive this contract preview."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine(
                "skillMonitorMainBuildCount",
                skillMonitorResult.selectedMainBuildCount.ToString(),
                "Lit item count for the explicitly selected main Build."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine(
                "skillMonitorMainBuildStage",
                ItemBuildTrackResult.BuildStageLabel(skillMonitorResult.selectedMainBuildStage),
                "Active stage for the explicitly selected main Build."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine(
                "skillMonitorValidationErrors",
                model.skillMonitorPreview.validationErrorsText,
                "Read-only validation output; empty selection is legal."));

            return model;
        }

        private static List<ItemDetailTextLine> BuildSlotLines(ItemSkillMonitorResolutionResult result)
        {
            List<ItemDetailTextLine> lines = new();
            foreach (ItemSkillMonitorSlotSnapshot slot in result.Slots.OrderBy(slot => slot.slotOrder))
            {
                string state = slot.isMonitoring ? "Monitoring" : "Locked";
                string body =
                    $"state={state}; sourceBuildId={FormatNullable(slot.sourceBuildId)}; sourceFaMenTag={FormatNullable(slot.sourceFaMenTag)}; requiredBuildStage={slot.requiredBuildStage}; triggerKind={slot.triggerKind}; isUnlocked={FormatBool(slot.isUnlocked)}; isMonitoring={FormatBool(slot.isMonitoring)}; isTriggered={FormatBool(slot.isTriggered)}; cooldown01={slot.cooldown01:0}; charge01={slot.charge01:0}; iconKey={slot.iconKey}; presentationCueId={slot.presentationCueId}; chibiActionKey={slot.chibiActionKey}.";
                lines.Add(new ItemDetailTextLine(slot.displayName, body, state));
            }

            return lines;
        }

        private static string FormatBool(bool value)
        {
            return value ? "true" : "false";
        }

        private static string FormatNullable(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "None" : value;
        }
    }
}
