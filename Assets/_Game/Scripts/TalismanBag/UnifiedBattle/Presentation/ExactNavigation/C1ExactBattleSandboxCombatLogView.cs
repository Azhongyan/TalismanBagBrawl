using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UnifiedBattle.Presentation.ExactNavigation
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxCombatLogView : MonoBehaviour
    {
        private const int MaximumVisibleCueRows = 16;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text combatLogText;

        public CanvasGroup CanvasGroup => canvasGroup;
        public Text CombatLogText => combatLogText;
        public bool IsOpen => canvasGroup != null && canvasGroup.alpha > 0.5f;

        public bool ValidateAuthoredReferences()
        {
            return canvasGroup != null
                   && canvasGroup.transform == transform
                   && combatLogText != null
                   && combatLogText.transform.parent == transform
                   && !combatLogText.raycastTarget
                   && !canvasGroup.interactable
                   && !canvasGroup.blocksRaycasts;
        }

        public void Render(
            string authoritativeStatus,
            IReadOnlyList<C1FormalRealtimeBattleCue> acceptedCues,
            bool open)
        {
            if (combatLogText != null)
            {
                combatLogText.text = open
                    ? BuildVisibleText(authoritativeStatus, acceptedCues)
                    : string.Empty;
            }

            ApplyVisibility(open);
        }

        public void ResetPresentation()
        {
            if (combatLogText != null)
            {
                combatLogText.text = string.Empty;
            }

            ApplyVisibility(false);
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            CanvasGroup configuredCanvasGroup,
            Text configuredCombatLogText)
        {
            canvasGroup = configuredCanvasGroup;
            combatLogText = configuredCombatLogText;
        }
#endif

        private void ApplyVisibility(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.ignoreParentGroups = false;
        }

        private static string BuildVisibleText(
            string authoritativeStatus,
            IReadOnlyList<C1FormalRealtimeBattleCue> acceptedCues)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("正式战斗状态");
            builder.AppendLine(authoritativeStatus ?? string.Empty);
            builder.AppendLine("正式战斗日志（只读）");

            int count = acceptedCues == null ? 0 : acceptedCues.Count;
            int start = Mathf.Max(0, count - MaximumVisibleCueRows);
            for (int index = start; index < count; index++)
            {
                C1FormalRealtimeBattleCue cue = acceptedCues[index];
                if (cue == null)
                {
                    continue;
                }

                builder.Append('#')
                    .Append(cue.sequence.ToString(CultureInfo.InvariantCulture))
                    .Append("  ")
                    .Append((cue.battleTimeMs / 1000m).ToString(
                        "0.000",
                        CultureInfo.InvariantCulture))
                    .Append("s  ")
                    .Append(cue.cueKind)
                    .Append("  ")
                    .Append(cue.sourceId);
                if (!string.IsNullOrEmpty(cue.targetActorId))
                {
                    builder.Append(" -> ").Append(cue.targetActorId);
                }
                if (cue.appliedDamage != 0)
                {
                    builder.Append("  damage=").Append(
                        cue.appliedDamage.ToString(
                            CultureInfo.InvariantCulture));
                }

                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }
    }
}
