using System;
using System.Collections.Generic;
using TalismanBag.Grid;
using TalismanBag.Items;
using UnityEngine;

namespace TalismanBag.Combo
{
    public sealed class ComboResolver : MonoBehaviour
    {
        public const string FireSpirit = "fire_spirit";
        public const string ShieldPill = "shield_pill";
        public const string ThunderSeal = "thunder_seal";
        public const string FireSword = "fire_sword";
        public const string ExorcismArray = "exorcism_array";
        public const string WaterPill = "water_pill";

        [SerializeField] private TalismanBagGrid grid;

        private readonly HashSet<string> activeComboIds = new();

        public event Action CombosChanged;

        private void Awake()
        {
            if (grid != null)
            {
                grid.GridChanged += RefreshCombos;
            }
        }

        private void Start()
        {
            RefreshCombos();
        }

        private void OnDestroy()
        {
            if (grid != null)
            {
                grid.GridChanged -= RefreshCombos;
            }
        }

        public void SetGrid(TalismanBagGrid bagGrid)
        {
            if (grid != null)
            {
                grid.GridChanged -= RefreshCombos;
            }

            grid = bagGrid;
            if (grid != null)
            {
                grid.GridChanged += RefreshCombos;
            }

            RefreshCombos();
        }

        public bool IsComboActive(string comboId)
        {
            return activeComboIds.Contains(comboId);
        }

        public List<string> GetActiveComboIds()
        {
            return new List<string>(activeComboIds);
        }

        public void RefreshCombos()
        {
            activeComboIds.Clear();

            if (grid != null)
            {
                foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
                {
                    if (item?.definition == null)
                    {
                        continue;
                    }

                    Vector2Int position = item.gridPosition;
                    switch (item.definition.itemId)
                    {
                        case "fire_talisman_basic":
                            AddIfAdjacent(position, "spirit_stone_basic", FireSpirit);
                            break;
                        case "qi_pill_basic":
                            AddIfAdjacent(position, "shield_talisman_basic", ShieldPill);
                            AddIfAdjacent(position, "water_talisman_basic", WaterPill);
                            break;
                        case "thunder_talisman_basic":
                            AddIfAdjacent(position, "seal_basic", ThunderSeal);
                            break;
                        case "sword_pill_basic":
                            AddIfAdjacent(position, "fire_talisman_basic", FireSword);
                            break;
                        case "peach_wood_basic":
                            AddIfAdjacent(position, "exorcism_bell_basic", ExorcismArray);
                            break;
                        case "water_talisman_basic":
                            AddIfAdjacent(position, "qi_pill_basic", WaterPill);
                            break;
                    }
                }
            }

            CombosChanged?.Invoke();
        }

        public static string GetComboDisplayName(string comboId)
        {
            return comboId switch
            {
                FireSpirit => "火灵连发",
                ShieldPill => "护丹",
                ThunderSeal => "雷印",
                FireSword => "火剑流",
                ExorcismArray => "驱邪阵",
                WaterPill => "水丹回气",
                _ => comboId
            };
        }

        private void AddIfAdjacent(Vector2Int position, string adjacentItemId, string comboId)
        {
            if (grid.HasAdjacentItemId(position, adjacentItemId))
            {
                activeComboIds.Add(comboId);
            }
        }
    }
}
