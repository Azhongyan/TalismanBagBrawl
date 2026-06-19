#if UNITY_EDITOR
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Upgrades;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public sealed class DataCatalog
    {
        private const string SearchRoot = "Assets/_Game";

        public readonly List<TalismanItemDefinition> Items = new();
        public readonly List<EnemyDefinition> Enemies = new();
        public readonly List<EnemyGroupConfig> EnemyGroups = new();
        public readonly List<V02RunConfig> RunConfigs = new();
        public readonly List<RewardConfig> Rewards = new();
        public readonly List<RewardDropTable> DropTables = new();
        public readonly List<IdleDropConfig> IdleDropConfigs = new();
        public readonly List<BossInfoConfig> BossConfigs = new();
        public readonly List<TalismanUpgradeConfig> UpgradeConfigs = new();

        public static DataCatalog Collect()
        {
            DataCatalog catalog = new();
            LoadAssets(catalog.Items);
            LoadAssets(catalog.Enemies);
            LoadAssets(catalog.EnemyGroups);
            LoadAssets(catalog.RunConfigs);
            LoadAssets(catalog.Rewards);
            LoadAssets(catalog.DropTables);
            LoadAssets(catalog.IdleDropConfigs);
            LoadAssets(catalog.BossConfigs);
            LoadAssets(catalog.UpgradeConfigs);
            return catalog;
        }

        public static string GetPath(Object asset)
        {
            return asset != null ? AssetDatabase.GetAssetPath(asset) : string.Empty;
        }

        private static void LoadAssets<T>(List<T> destination) where T : Object
        {
            destination.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { SearchRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null && !destination.Contains(asset))
                {
                    destination.Add(asset);
                }
            }

            destination.Sort((left, right) =>
                string.Compare(GetPath(left), GetPath(right), System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
#endif
