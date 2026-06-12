using TalismanBag.Combat;
using TalismanBag.Grid;
using TalismanBag.Inventory;
using TalismanBag.Items;
using TalismanBag.UI;
using UnityEngine;

namespace TalismanBag.V02.Rewards
{
    public sealed class V02RewardInventoryAdapter : MonoBehaviour
    {
        [SerializeField] private PlayerTalismanInventory inventory;
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private Transform inventoryParent;
        [SerializeField] private DraggableTalismanItemView itemViewTemplate;

        public DraggableTalismanItemView AddTalisman(TalismanItemDefinition definition, int level = 1)
        {
            if (definition == null || itemViewTemplate == null || inventoryParent == null)
            {
                return null;
            }

            TalismanItemRuntime runtime = inventory != null
                ? inventory.AddItem(definition, level)
                : new TalismanItemRuntime(definition, level);

            DraggableTalismanItemView view = Instantiate(itemViewTemplate, inventoryParent);
            view.name = $"RewardItem_{definition.itemId}";
            view.gameObject.SetActive(true);
            view.BindRuntime(runtime, grid, rootCanvas, combatController);
            view.CaptureHome();
            combatController?.RegisterItemView(view);
            return view;
        }
    }
}
