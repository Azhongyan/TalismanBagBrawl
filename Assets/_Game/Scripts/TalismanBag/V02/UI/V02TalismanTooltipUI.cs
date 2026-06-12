using TalismanBag.Items;
using TalismanBag.UI;
using TalismanBag.V02.Tags;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02TalismanTooltipUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        private TalismanItemDefinition selectedDefinition;

        public TalismanItemDefinition SelectedDefinition => selectedDefinition;

        private void OnEnable()
        {
            DraggableTalismanItemView.ItemClicked += OnItemClicked;
        }

        private void OnDisable()
        {
            DraggableTalismanItemView.ItemClicked -= OnItemClicked;
        }

        public void Show(TalismanItemDefinition definition)
        {
            selectedDefinition = definition;
            if (panel != null)
            {
                panel.SetActive(definition != null);
            }

            if (definition == null)
            {
                SetText(titleText, string.Empty);
                SetText(bodyText, string.Empty);
                return;
            }

            SetText(titleText, definition.displayName);
            SetText(bodyText,
                $"元素：{TalismanTagUtility.GetElementTagName(definition.elementTag)}\n" +
                $"功能：{TalismanTagUtility.JoinFunctionTags(definition)}\n" +
                $"克制：{TalismanTagUtility.JoinCounterTags(definition)}\n" +
                $"效果：{TalismanTagUtility.GetEffectTypeName(definition.effectType)}\n" +
                $"灵气需求：{definition.energyRequired}\n" +
                $"供能：{(definition.requiresFormationPower ? "需要阵法供能" : "独立供能源")}\n\n" +
                $"{definition.shortRoleDescription}\n{definition.counterDescription}");
        }

        public void Clear()
        {
            Show(null);
        }

        public void PrintSelectedTags()
        {
            Debug.Log(TalismanTagUtility.BuildDebugSummary(selectedDefinition));
        }

        private void OnItemClicked(DraggableTalismanItemView view)
        {
            Show(view != null ? view.Definition : null);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
