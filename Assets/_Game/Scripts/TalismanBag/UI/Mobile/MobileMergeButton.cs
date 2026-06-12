using TalismanBag.Combat;
using TalismanBag.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI.Mobile
{
    public sealed class MobileMergeButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text label;
        [SerializeField] private ItemMergeSystem mergeSystem;
        [SerializeField] private AutoCombatController combatController;

        private void Awake()
        {
            button?.onClick.AddListener(MergeAll);
            Refresh();
        }

        public void Refresh()
        {
            bool canMerge = mergeSystem != null && mergeSystem.CanMergeAny();
            if (label != null)
            {
                label.text = canMerge ? "自动合成" : "暂无可合成";
            }

            if (button != null)
            {
                button.interactable = canMerge;
            }
        }

        private void MergeAll()
        {
            combatController?.AutoMergeDuplicateLevelOneItems();
            Refresh();
        }
    }
}
