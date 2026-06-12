using TalismanBag.Combat;
using UnityEngine;

namespace TalismanBag.Progression
{
    public sealed class ItemLevelSystem : MonoBehaviour
    {
        [SerializeField] private AutoCombatController combatController;

        public void AutoMergeAvailableItems()
        {
            combatController?.AutoMergeDuplicateLevelOneItems();
        }
    }
}
