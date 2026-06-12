using TalismanBag.V02.Formation;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class FormationPowerUI : MonoBehaviour
    {
        [SerializeField] private FormationPowerResolver resolver;
        [SerializeField] private Text summaryText;
        [SerializeField] private Text detailText;
        [SerializeField] private Text hintText;

        private const string DefaultHint =
            "\u7b26\u7b93\u5fc5\u987b\u88ab\u9635\u773c\u6216\u805a\u7075\u77f3\u4f9b\u80fd\u624d\u4f1a\u89e6\u53d1\u3002\n" +
            "\u79fb\u52a8\u805a\u7075\u77f3\uff0c\u53ef\u4ee5\u6539\u53d8\u9635\u6cd5\u8303\u56f4\u3002";

        private void OnEnable()
        {
            if (resolver != null)
            {
                resolver.PowerStatesChanged += RefreshFromResolver;
            }
        }

        private void Start()
        {
            Refresh(resolver);
        }

        private void OnDisable()
        {
            if (resolver != null)
            {
                resolver.PowerStatesChanged -= RefreshFromResolver;
            }
        }

        public void Bind(FormationPowerResolver powerResolver)
        {
            if (resolver != null)
            {
                resolver.PowerStatesChanged -= RefreshFromResolver;
            }

            resolver = powerResolver;

            if (isActiveAndEnabled && resolver != null)
            {
                resolver.PowerStatesChanged += RefreshFromResolver;
            }

            Refresh(resolver);
        }

        public void Refresh(FormationPowerResolver powerResolver)
        {
            if (powerResolver == null)
            {
                SetText(summaryText, "\u53ef\u6fc0\u6d3b\u7b26\u7b93\uff1a0 / 0");
                SetText(detailText, "\u5f31\u4f9b\u80fd\uff1a0\n\u672a\u4f9b\u80fd\uff1a0");
                SetText(hintText, DefaultHint);
                return;
            }

            int placed = powerResolver.GetPlacedTalismanCount();
            int powered = powerResolver.GetPoweredItemCount();
            int weak = powerResolver.GetWeakPoweredItemCount();
            int unpowered = powerResolver.GetUnpoweredItems().Count;

            SetText(summaryText, $"\u53ef\u6fc0\u6d3b\u7b26\u7b93\uff1a{powered} / {placed}");
            SetText(detailText, $"\u5f31\u4f9b\u80fd\uff1a{weak}\n\u672a\u4f9b\u80fd\uff1a{unpowered}");
            string modifierHint = BuildModifierHint(powerResolver);
            SetText(hintText, !string.IsNullOrEmpty(modifierHint)
                ? modifierHint
                : unpowered > 0
                ? "\u672a\u4f9b\u80fd\u7b26\u7b93\u4e0d\u4f1a\u89e6\u53d1"
                : DefaultHint);
        }

        private void RefreshFromResolver()
        {
            Refresh(resolver);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static string BuildModifierHint(FormationPowerResolver powerResolver)
        {
            if (powerResolver?.RunModifierState == null || !powerResolver.RunModifierState.HasAnyModifier)
            {
                return string.Empty;
            }

            string result = string.Empty;
            if (powerResolver.RunModifierState.eyeCoreNineGridUnlocked)
            {
                result += "阵眼已强化：九宫格供能\n";
            }

            if (powerResolver.RunModifierState.spiritLinkBetweenStonesUnlocked)
            {
                result += "灵气连线：同排/同列聚灵石之间供能\n";
            }

            if (powerResolver.RunModifierState.outerRingDefenseBoostUnlocked)
            {
                result += "外圈护阵：外圈护身符/净化符增强";
            }

            return result.TrimEnd();
        }
    }
}
