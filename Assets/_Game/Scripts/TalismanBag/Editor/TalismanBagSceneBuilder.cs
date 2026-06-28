#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using TalismanBag.Combat;
using TalismanBag.Combo;
using TalismanBag.Debugging;
using TalismanBag.Economy;
using TalismanBag.Enemies;
using TalismanBag.Feedback;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.Progression;
using TalismanBag.Run;
using TalismanBag.Shop;
using TalismanBag.UI;
using TalismanBag.V02.Boss;
using TalismanBag.V02.Balance;
using TalismanBag.V02.Counters;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Formation;
using TalismanBag.V02.Result;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.Run;
using TalismanBag.V02.Status;
using TalismanBag.V02.Tags;
using TalismanBag.V02.UI;
using TalismanBag.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools
{
    public static class TalismanBagSceneBuilder
    {
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_Run15Min.unity";
        private const string V02ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity";
        private static readonly Vector2 ReferenceResolution = new(1080f, 1920f);

        public static void BuildAll()
        {
            EnsureFolders();
            ConfigurePortraitPlayerSettings();
            EnsurePortraitGameViewSize();
            Dictionary<string, TalismanItemDefinition> items = CreateItemDefinitions();
            Dictionary<string, EnemyDefinition> enemies = CreateEnemyDefinitions();
            RunConfig runConfig = CreateRunConfig(items, enemies);
            ShopPoolConfig shopPool = CreateShopPoolConfig(items);
            ShopItemPriceConfig priceConfig = CreateShopPriceConfig(items);
            DraggableTalismanItemView itemPrefab = CreatePrefabs(items);
            CreateScene(items, enemies, runConfig, shopPool, priceConfig, itemPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Talisman Bag phase 5 mobile playtest scene generated.");
        }

        public static void BuildV02FormationCounter()
        {
            EnsureFolders();
            ConfigurePortraitPlayerSettings();
            EnsurePortraitGameViewSize();
            CreateItemDefinitions();
            Dictionary<string, TalismanItemDefinition> items = CreateV02ItemDefinitions();
            Dictionary<string, EnemyDefinition> enemies = CreateV02EnemyDefinitions();
            V02RewardPoolConfig rewardPool = CreateV02RewardPool(items);
            V02RunConfig runConfig = CreateV02RunConfig(enemies);
            V02CounterMultiplierConfig multiplierConfig = CreateV02CounterMultiplierConfig();
            V02FormationBalanceConfig formationConfig = CreateV02FormationBalanceConfig();
            CreateV02FormationCounterScene(items, enemies["mountain_imp_basic"], enemies, rewardPool, runConfig, multiplierConfig, formationConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Talisman Bag V0.2 formation counter scene generated.");
        }

        [MenuItem("Tools/Talisman Bag/Ensure V02 Stage Progress Bar")]
        public static void EnsureV02StageProgressBarInScene()
        {
            Scene scene = FindLoadedScene(V02ScenePath);
            if (!scene.IsValid())
            {
                scene = EditorSceneManager.OpenScene(V02ScenePath, OpenSceneMode.Additive);
            }

            if (!scene.IsValid())
            {
                Debug.LogError($"Unable to open V02 scene: {V02ScenePath}");
                return;
            }

            Transform progressParent = FindStageProgressParentInScene(scene);
            if (progressParent == null)
            {
                Debug.LogError("MobileSafeAreaRoot or Canvas not found. Build the V02 formation scene before adding stage progress.");
                return;
            }

            V02StageProgressBar progressBar = FindComponentInScene<V02StageProgressBar>(scene);
            if (progressBar == null)
            {
                progressBar = CreateV02StageProgressBar(progressParent);
            }

            if (progressBar == null)
            {
                Debug.LogError("Failed to create V02StageProgressBar_Runtime.");
                return;
            }

            progressBar.gameObject.name = "V02StageProgressBar_Runtime";
            progressBar.SetProgress("1", 4, 9, 10);

            V02RunFlowController runFlow = FindComponentInScene<V02RunFlowController>(scene);
            if (runFlow != null)
            {
                SetField(runFlow, "stageProgressBar", progressBar);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, V02ScenePath);
            AssetDatabase.ImportAsset(V02ScenePath);
            Debug.Log("Ensured V02StageProgressBar_Runtime in V02 formation scene.");
        }

        private static Scene FindLoadedScene(string scenePath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid() && string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return scene;
                }
            }

            return default;
        }

        private static GameObject FindGameObjectInScene(Scene scene, string objectName)
        {
            if (!scene.IsValid())
            {
                return null;
            }

            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                Transform found = FindChildRecursive(rootObjects[i].transform, objectName);
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindStageProgressParentInScene(Scene scene)
        {
            GameObject safeArea = FindGameObjectInScene(scene, "MobileSafeAreaRoot");
            if (safeArea != null)
            {
                return safeArea.transform;
            }

            Canvas canvas = FindComponentInScene<Canvas>(scene);
            return canvas != null ? canvas.transform : null;
        }

        private static V02StageProgressBar CreateV02StageProgressBar(Transform parent)
        {
            return V02StageProgressBar.CreateRuntime(
                parent,
                new Vector2(0f, -188f),
                new Vector2(820f, 72f),
                true);
        }

        private static T FindComponentInScene<T>(Scene scene) where T : Component
        {
            if (!scene.IsValid())
            {
                return null;
            }

            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                T component = rootObjects[i].GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (string.Equals(root.name, objectName, StringComparison.Ordinal))
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/_Game",
                "Assets/_Game/Scripts",
                "Assets/_Game/Scripts/TalismanBag",
                "Assets/_Game/Scripts/TalismanBag/Grid",
                "Assets/_Game/Scripts/TalismanBag/Items",
                "Assets/_Game/Scripts/TalismanBag/Combat",
                "Assets/_Game/Scripts/TalismanBag/UI",
                "Assets/_Game/Scripts/TalismanBag/UI/Mobile",
                "Assets/_Game/Scripts/TalismanBag/Run",
                "Assets/_Game/Scripts/TalismanBag/Economy",
                "Assets/_Game/Scripts/TalismanBag/Inventory",
                "Assets/_Game/Scripts/TalismanBag/Progression",
                "Assets/_Game/Scripts/TalismanBag/Balance",
                "Assets/_Game/Scripts/TalismanBag/Shop",
                "Assets/_Game/Scripts/TalismanBag/Enemies",
                "Assets/_Game/Scripts/TalismanBag/Combo",
                "Assets/_Game/Scripts/TalismanBag/Feedback",
                "Assets/_Game/Scripts/TalismanBag/VFX",
                "Assets/_Game/Scripts/TalismanBag/Debug",
                "Assets/_Game/Scripts/TalismanBag/V02",
                "Assets/_Game/Scripts/TalismanBag/V02/Balance",
                "Assets/_Game/Scripts/TalismanBag/V02/Enemies",
                "Assets/_Game/Scripts/TalismanBag/V02/EnemySkills",
                "Assets/_Game/Scripts/TalismanBag/V02/Grid",
                "Assets/_Game/Scripts/TalismanBag/V02/Formation",
                "Assets/_Game/Scripts/TalismanBag/V02/Items",
                "Assets/_Game/Scripts/TalismanBag/V02/Tags",
                "Assets/_Game/Scripts/TalismanBag/V02/Rewards",
                "Assets/_Game/Scripts/TalismanBag/V02/Run",
                "Assets/_Game/Scripts/TalismanBag/V02/Boss",
                "Assets/_Game/Scripts/TalismanBag/V02/Result",
                "Assets/_Game/Scripts/TalismanBag/V02/UI",
                "Assets/_Game/Prefabs",
                "Assets/_Game/Prefabs/TalismanBag",
                "Assets/_Game/Prefabs/TalismanBag/V02",
                "Assets/_Game/ScriptableObjects",
                "Assets/_Game/ScriptableObjects/TalismanBag",
                "Assets/_Game/ScriptableObjects/TalismanBag/V02",
                "Assets/_Game/ScriptableObjects/TalismanBag/V02/Balance",
                "Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies",
                "Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/Skills",
                "Assets/_Game/ScriptableObjects/TalismanBag/V02/Items",
                "Assets/_Game/ScriptableObjects/TalismanBag/V02/Rewards",
                "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs",
                "Assets/_Game/ScriptableObjects/TalismanBag/Enemies",
                "Assets/_Game/ScriptableObjects/TalismanBag/RunConfigs",
                "Assets/_Game/ScriptableObjects/TalismanBag/EnemyConfigs",
                "Assets/_Game/ScriptableObjects/TalismanBag/ShopPools",
                "Assets/_Game/Scenes"
            };

            foreach (string folder in folders)
            {
                if (AssetDatabase.IsValidFolder(folder))
                {
                    continue;
                }

                string parent = System.IO.Path.GetDirectoryName(folder)?.Replace("\\", "/");
                string name = System.IO.Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static void ConfigurePortraitPlayerSettings()
        {
            PlayerSettings.companyName = "Prototype";
            PlayerSettings.productName = "TalismanBagPrototype";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
        }

        private static void EnsurePortraitGameViewSize()
        {
            try
            {
                Assembly editorAssembly = typeof(Editor).Assembly;
                Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
                Type sizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
                Type sizeTypeEnum = editorAssembly.GetType("UnityEditor.GameViewSizeType");
                Type groupTypeEnum = editorAssembly.GetType("UnityEditor.GameViewSizeGroupType");
                if (sizesType == null || sizeType == null || sizeTypeEnum == null || groupTypeEnum == null)
                {
                    return;
                }

                Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                object instance = singletonType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
                object standaloneGroup = Enum.Parse(groupTypeEnum, "Standalone");
                object group = sizesType.GetMethod("GetGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(instance, new[] { standaloneGroup });
                if (group == null)
                {
                    return;
                }

                MethodInfo getTotalCount = group.GetType().GetMethod("GetTotalCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo getSize = group.GetType().GetMethod("GetGameViewSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                MethodInfo addCustomSize = group.GetType().GetMethod("AddCustomSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                HashSet<string> existingLabels = new();
                int count = getTotalCount != null ? (int)getTotalCount.Invoke(group, null) : 0;
                for (int i = 0; i < count; i++)
                {
                    object size = getSize?.Invoke(group, new object[] { i });
                    string label = size?.GetType().GetProperty("baseText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(size) as string;
                    if (!string.IsNullOrEmpty(label))
                    {
                        existingLabels.Add(label);
                    }
                }

                object fixedResolution = Enum.Parse(sizeTypeEnum, "FixedResolution");
                ConstructorInfo constructor = sizeType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) },
                    null);
                AddGameViewSizeIfMissing(sizeType, fixedResolution, constructor, addCustomSize, group, existingLabels, 1080, 1920, "1080x1920 Portrait");
                AddGameViewSizeIfMissing(sizeType, fixedResolution, constructor, addCustomSize, group, existingLabels, 1200, 2640, "1200x2640 Huawei P40 Pro");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not add portrait Game View size automatically: {exception.Message}");
            }
        }

        private static void AddGameViewSizeIfMissing(
            Type sizeType,
            object fixedResolution,
            ConstructorInfo constructor,
            MethodInfo addCustomSize,
            object group,
            HashSet<string> existingLabels,
            int width,
            int height,
            string label)
        {
            if (existingLabels.Contains(label))
            {
                Debug.Log($"Game View size already exists: {label}");
                return;
            }

            object newSize = constructor != null
                ? constructor.Invoke(new object[] { fixedResolution, width, height, label })
                : Activator.CreateInstance(sizeType, fixedResolution, width, height, label);
            addCustomSize?.Invoke(group, new[] { newSize });
            existingLabels.Add(label);
            Debug.Log($"Game View size added: {label}");
        }

        private static Dictionary<string, TalismanItemDefinition> CreateItemDefinitions()
        {
            Dictionary<string, TalismanItemDefinition> definitions = new();
            AddItem(definitions, "spirit_stone_basic", "聚灵石", TalismanItemType.SpiritStone, ElementType.None, 2f, 0, 12, new Color(0.28f, 0.68f, 1f), "每 2 秒产生 12 点灵气。");
            AddItem(definitions, "fire_talisman_basic", "火符", TalismanItemType.AttackTalisman, ElementType.Fire, 2.5f, 10, 12, new Color(1f, 0.42f, 0.2f), "消耗 10 灵气造成火焰伤害，贴近聚灵石会加速。");
            AddItem(definitions, "shield_talisman_basic", "护身符", TalismanItemType.ShieldTalisman, ElementType.Earth, 5f, 8, 18, new Color(0.45f, 0.9f, 0.55f), "消耗 8 灵气获得 18 护盾，护盾上限 50。");
            AddItem(definitions, "qi_pill_basic", "回气丹", TalismanItemType.Pill, ElementType.Water, 8f, 12, 20, new Color(0.95f, 0.5f, 0.92f), "低血时恢复气血，贴近护身符治疗更高。");
            AddItem(definitions, "thunder_talisman_basic", "雷符", TalismanItemType.AttackTalisman, ElementType.Thunder, 3f, 16, 18, new Color(0.62f, 0.58f, 1f), "雷伤害克制鬼怪，可打断剑修蓄力。");
            AddItem(definitions, "seal_basic", "法印", TalismanItemType.PassiveTool, ElementType.None, 0f, 0, 0, new Color(0.9f, 0.82f, 0.42f), "被动道具，强化相邻雷符和火符。");
            AddItem(definitions, "sword_pill_basic", "小剑丸", TalismanItemType.AttackTalisman, ElementType.Metal, 2.2f, 8, 8, new Color(0.8f, 0.85f, 0.9f), "快速物理攻击，贴近火符激活火剑流。");
            AddItem(definitions, "peach_wood_basic", "桃木牌", TalismanItemType.PassiveTool, ElementType.Wood, 0f, 0, 0, new Color(0.72f, 0.48f, 0.28f), "被动压制鬼怪，贴近驱邪铃激活驱邪阵。");
            AddItem(definitions, "exorcism_bell_basic", "驱邪铃", TalismanItemType.AttackTalisman, ElementType.Metal, 4f, 10, 12, new Color(0.96f, 0.74f, 0.34f), "对鬼怪有效，贴近桃木牌激活驱邪阵。");
            AddItem(definitions, "water_talisman_basic", "水符", TalismanItemType.SupportTalisman, ElementType.Water, 4f, 12, 10, new Color(0.35f, 0.82f, 0.95f), "恢复 10 气血，贴近回气丹可缩短冷却。");
            return definitions;
        }

        private static void AddItem(
            Dictionary<string, TalismanItemDefinition> definitions,
            string itemId,
            string displayName,
            TalismanItemType itemType,
            ElementType elementType,
            float cooldown,
            int manaCost,
            int baseValue,
            Color color,
            string description)
        {
            string path = $"Assets/_Game/ScriptableObjects/TalismanBag/{itemId}.asset";
            TalismanItemDefinition definition = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<TalismanItemDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.itemId = itemId;
            definition.displayName = displayName;
            definition.itemType = itemType;
            definition.elementType = elementType;
            definition.width = 1;
            definition.height = 1;
            definition.baseCooldown = cooldown;
            definition.manaCost = manaCost;
            definition.baseValue = baseValue;
            definition.uiColor = color;
            definition.description = description;
            EditorUtility.SetDirty(definition);
            definitions[itemId] = definition;
        }

        private static Dictionary<string, TalismanItemDefinition> CreateV02ItemDefinitions()
        {
            Dictionary<string, TalismanItemDefinition> definitions = new();
            AddV02Item(definitions, "fire_talisman_basic", "\u706b\u7b26", TalismanItemType.AttackTalisman, ElementType.Fire, 2.5f, 10, 12, new Color(1f, 0.42f, 0.2f),
                ElementTag.Fire, new[] { FunctionTag.Damage, FunctionTag.Burn }, new[] { CounterTag.Group }, EffectType.DealDamage, 1, true,
                "单体火焰伤害，可叠加灼烧压力。", "适合压制成群小怪。");
            AddV02Item(definitions, "thunder_talisman_basic", "\u96f7\u7b26", TalismanItemType.AttackTalisman, ElementType.Thunder, 3f, 16, 18, new Color(0.62f, 0.58f, 1f),
                ElementTag.Thunder, new[] { FunctionTag.Damage, FunctionTag.ShieldBreak }, new[] { CounterTag.Shield }, EffectType.DealDamage, 1, true,
                "雷法伤害，带破盾倾向。", "克制带护盾的敌人。");
            AddV02Item(definitions, "sword_pill_basic", "\u5251\u4e38", TalismanItemType.AttackTalisman, ElementType.Metal, 2.2f, 8, 8, new Color(0.8f, 0.85f, 0.9f),
                ElementTag.Sword, new[] { FunctionTag.Damage, FunctionTag.Burst }, new[] { CounterTag.Charge, CounterTag.Boss }, EffectType.DealDamage, 1, true,
                "单体爆发法器，适合抓输出窗口。", "克制蓄力和首领阶段。");
            AddV02Item(definitions, "chain_thunder_talisman_basic", "\u8fde\u9501\u96f7\u7b26", TalismanItemType.AttackTalisman, ElementType.Thunder, 3.8f, 18, 14, new Color(0.48f, 0.72f, 1f),
                ElementTag.Thunder, new[] { FunctionTag.Damage, FunctionTag.Chain, FunctionTag.AoE }, new[] { CounterTag.Group, CounterTag.Summon }, EffectType.ChainDamage, 1, true,
                "连锁雷法伤害，用于清理多目标。", "克制群体敌人和召唤物。");
            AddV02Item(definitions, "shield_talisman_basic", "\u62a4\u8eab\u7b26", TalismanItemType.ShieldTalisman, ElementType.Earth, 5f, 8, 18, new Color(0.45f, 0.9f, 0.55f),
                ElementTag.Earth, new[] { FunctionTag.Shield, FunctionTag.Defense }, new[] { CounterTag.Charge }, EffectType.GainShield, 1, true,
                "生成护盾，用于承受爆发和蓄力攻击。", "克制蓄力压力。");
            AddV02Item(definitions, "purify_talisman_basic", "\u51c0\u5316\u7b26", TalismanItemType.SupportTalisman, ElementType.Water, 6f, 8, 0, new Color(0.58f, 0.92f, 1f),
                ElementTag.Water, new[] { FunctionTag.Cleanse, FunctionTag.Defense, FunctionTag.AntiPoison, FunctionTag.AntiSeal }, new[] { CounterTag.Poison, CounterTag.Burn, CounterTag.Seal }, EffectType.CleanseStatus, 1, true,
                "净化异常状态，针对中毒、灼烧、封印。", "克制中毒、灼烧和封印。");
            AddV02Item(definitions, "soul_suppress_talisman_basic", "\u9547\u9b42\u7b26", TalismanItemType.SupportTalisman, ElementType.None, 4.5f, 10, 10, new Color(0.72f, 0.62f, 0.96f),
                ElementTag.Soul, new[] { FunctionTag.AntiGhost, FunctionTag.Defense }, new[] { CounterTag.Ghost, CounterTag.StealEnergy }, EffectType.SuppressGhost, 1, true,
                "镇压鬼怪和偷取灵气的敌方意图。", "克制鬼怪和偷灵气效果。");
            AddV02Item(definitions, "spirit_stone_basic", "\u805a\u7075\u77f3", TalismanItemType.SpiritStone, ElementType.Earth, 2f, 0, 12, new Color(0.28f, 0.68f, 1f),
                ElementTag.Earth, new[] { FunctionTag.EnergySource }, Array.Empty<CounterTag>(), EffectType.GenerateEnergy, 0, false,
                "阵法供能源，可为附近符箓供能。", "不需要外部阵法供能。");
            AddV02Item(definitions, "qi_pill_basic", "\u4e39\u836f", TalismanItemType.Pill, ElementType.Wood, 8f, 12, 20, new Color(0.95f, 0.5f, 0.92f),
                ElementTag.Wood, new[] { FunctionTag.Heal }, new[] { CounterTag.Poison, CounterTag.Burn }, EffectType.Heal, 1, true,
                "战斗中恢复气血。", "帮助稳定中毒和灼烧压力。");
            AddV02Item(definitions, "seal_basic", "\u6cd5\u5370", TalismanItemType.PassiveTool, ElementType.Metal, 0f, 0, 0, new Color(0.9f, 0.82f, 0.42f),
                ElementTag.Metal, new[] { FunctionTag.Enhance }, Array.Empty<CounterTag>(), EffectType.EnhanceAdjacent, 0, true,
                "阵法核心工具，用于强化相邻符箓。", "本阶段暂无直接克制目标。");
            return definitions;
        }

        private static void AddV02Item(
            Dictionary<string, TalismanItemDefinition> definitions,
            string itemId,
            string displayName,
            TalismanItemType itemType,
            ElementType elementType,
            float cooldown,
            int manaCost,
            int baseValue,
            Color color,
            ElementTag elementTag,
            IEnumerable<FunctionTag> functionTags,
            IEnumerable<CounterTag> counterTags,
            EffectType effectType,
            int energyRequired,
            bool requiresFormationPower,
            string shortRoleDescription,
            string counterDescription)
        {
            string path = $"Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/{itemId}.asset";
            TalismanItemDefinition definition = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<TalismanItemDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.itemId = itemId;
            definition.displayName = displayName;
            definition.itemType = itemType;
            definition.elementType = elementType;
            definition.width = 1;
            definition.height = 1;
            definition.baseCooldown = cooldown;
            definition.manaCost = manaCost;
            definition.baseValue = baseValue;
            definition.uiColor = color;
            definition.description = shortRoleDescription;
            definition.elementTag = elementTag;
            definition.functionTags = new List<FunctionTag>(functionTags);
            definition.counterTags = new List<CounterTag>(counterTags);
            definition.effectType = effectType;
            definition.energyRequired = energyRequired;
            definition.requiresFormationPower = requiresFormationPower;
            definition.shortRoleDescription = shortRoleDescription;
            definition.counterDescription = counterDescription;
            EditorUtility.SetDirty(definition);
            definitions[itemId] = definition;
        }

        private static Dictionary<string, EnemyDefinition> CreateEnemyDefinitions()
        {
            Dictionary<string, EnemyDefinition> enemies = new();
            AddEnemy(enemies, "ghost_basic", "普通鬼怪", EnemyType.Ghost, 80, 8, 2.5f, "弱火 / 弱雷", "攻频较快", "火灵连发 / 护丹");
            AddEnemy(enemies, "ghost_elite", "强化鬼怪", EnemyType.Ghost, 110, 10, 2.2f, "弱雷 / 怕驱邪 / 怕桃木", "攻击更快", "雷符 / 驱邪阵");
            AddEnemy(enemies, "sword_cultivator_basic", "剑修来袭", EnemyType.SwordCultivator, 120, 14, 3f, "可打断 / 怕火剑流", "蓄力连斩", "雷印 / 火剑流 / 护丹", chargeInterval: 8f, chargeDuration: 2f, chargeDamage: 30);
            AddEnemy(enemies, "evil_cultivator_basic", "邪修封阵", EnemyType.EvilCultivator, 140, 12, 3.5f, "怕稳定输出 / 怕充足灵气", "会封印 / 会吸灵", "分散阵型 / 双灵气", manaDrainInterval: 6f, manaDrainAmount: 10, sealInterval: 10f, sealDuration: 4f);
            AddEnemy(enemies, "ghost_swarm", "鬼群压境", EnemyType.GhostSwarm, 160, 9, 2f, "怕驱邪 / 弱雷", "鬼影攻击", "驱邪阵 / 雷法 / 火灵连发", ghostShadowInterval: 7f, ghostShadowDamage: 8);
            AddEnemy(enemies, "sword_cultivator_elite", "剑修精英", EnemyType.SwordCultivator, 180, 16, 2.8f, "可打断 / 怕火剑流", "高伤连斩", "雷印 / 火剑流 / 护丹", chargeInterval: 7f, chargeDuration: 2f, chargeDamage: 38);
            AddEnemy(enemies, "heart_demon_boss", "心魔邪修", EnemyType.Boss, 260, 14, 3f, "可打断 / 怕稳定阵法 / 怕雷印爆发", "吸灵 / 封印 / 心魔冲击 / 半血狂暴", "雷印 / 护丹 / 火剑流 / 多聚灵石", chargeInterval: 14f, chargeDuration: 3f, chargeDamage: 36, manaDrainInterval: 6f, manaDrainAmount: 12, sealInterval: 9f, sealDuration: 4f);
            AddEnemy(enemies, "heart_demon_boss_placeholder", "心魔邪修", EnemyType.Boss, 240, 16, 3f, "可打断 / 怕稳定阵法", "高血量 / 高压力", "双灵气 / 雷印 / 护丹", manaDrainInterval: 6f, manaDrainAmount: 12, sealInterval: 10f, sealDuration: 4f);
            return enemies;
        }

        private static void AddEnemy(
            Dictionary<string, EnemyDefinition> enemies,
            string enemyId,
            string displayName,
            EnemyType enemyType,
            int hp,
            int damage,
            float interval,
            string weakness,
            string danger,
            string recommendedBuild,
            float chargeInterval = 0f,
            float chargeDuration = 0f,
            int chargeDamage = 0,
            float manaDrainInterval = 0f,
            int manaDrainAmount = 0,
            float sealInterval = 0f,
            float sealDuration = 0f,
            float ghostShadowInterval = 0f,
            int ghostShadowDamage = 0)
        {
            string path = $"Assets/_Game/ScriptableObjects/TalismanBag/Enemies/{enemyId}.asset";
            EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.enemyId = enemyId;
            definition.displayName = displayName;
            definition.enemyType = enemyType;
            definition.maxHp = hp;
            definition.attackDamage = damage;
            definition.attackInterval = interval;
            definition.weaknessText = weakness;
            definition.dangerText = danger;
            definition.recommendedBuildText = recommendedBuild;
            definition.chargeInterval = chargeInterval;
            definition.chargeDuration = chargeDuration;
            definition.chargeAttackDamage = chargeDamage;
            definition.manaDrainInterval = manaDrainInterval;
            definition.manaDrainAmount = manaDrainAmount;
            definition.sealInterval = sealInterval;
            definition.sealDuration = sealDuration;
            definition.ghostShadowInterval = ghostShadowInterval;
            definition.ghostShadowDamage = ghostShadowDamage;
            EditorUtility.SetDirty(definition);
            enemies[enemyId] = definition;
        }

        private static EnemyDefinition CreateV02TestEnemy()
        {
            const string path = "Assets/_Game/ScriptableObjects/TalismanBag/V02/v02_mountain_imp.asset";
            EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.enemyId = "Deprecated_v02_mountain_imp";
            definition.displayName = "攻";
            definition.enabled = false;
            definition.enemyType = EnemyType.Ghost;
            definition.maxHp = 80;
            definition.attackDamage = 8;
            definition.attackInterval = 2.5f;
            definition.weaknessText = "\u65e0\u7279\u6b8a";
            definition.dangerText = "\u666e\u901a\u653b\u51fb";
            definition.recommendedBuildText = "Deprecated legacy config. Use mountain_imp_basic for 1-1.";
            definition.chargeInterval = 0f;
            definition.chargeDuration = 0f;
            definition.chargeAttackDamage = 0;
            definition.manaDrainInterval = 0f;
            definition.manaDrainAmount = 0;
            definition.sealInterval = 0f;
            definition.sealDuration = 0f;
            definition.ghostShadowInterval = 0f;
            definition.ghostShadowDamage = 0;
            definition.enemyClass = "Deprecated";
            definition.enemyArchetype = "Legacy v02 mountain imp config";
            definition.intentText = "Deprecated legacy config. Use mountain_imp_basic for 1-1.";
            definition.recommendedCounterText = "Deprecated legacy config. Use mountain_imp_basic for 1-1.";
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static Dictionary<string, EnemyDefinition> CreateV02EnemyDefinitions()
        {
            Dictionary<string, EnemySkillDefinition> skills = CreateV02EnemySkills();
            Dictionary<string, EnemyDefinition> enemies = new();

            AddV02Enemy(enemies, "mountain_imp_basic", "攻", EnemyType.Ghost, 80, 8, 2.5f,
                "教学", "普通攻击", "普通攻击",
                "把火符放到阵眼或聚灵石供能范围内。",
                Array.Empty<CounterTag>(), Array.Empty<EnemySkillDefinition>());

            AddV02Enemy(enemies, "turtle_guardian_shield", "盾", EnemyType.Ghost, 110, 7, 2.8f,
                "护盾", "周期护盾", "正在积蓄护盾",
                "雷符可以破盾，剑丸爆发也有效。",
                new[] { CounterTag.Shield }, new[] { skills["turtle_guardian_gain_shield"] });

            AddV02Enemy(enemies, "imp_swarm", "召", EnemyType.GhostSwarm, 140, 9, 2f,
                "召唤", "群体压制", "准备呼唤更多小妖",
                "连锁雷符和火符燃烧更适合清群。",
                new[] { CounterTag.Group, CounterTag.Summon }, new[] { skills["imp_swarm_summon"] });

            AddV02Enemy(enemies, "red_poison_beast", "毒", EnemyType.Ghost, 130, 8, 2.8f,
                "毒火", "持续伤害", "准备喷吐毒火",
                "净化符可以清除毒和燃烧，护身符可以抵抗持续伤害。",
                new[] { CounterTag.Poison, CounterTag.Burn }, new[] { skills["red_poison_apply_poison"] });

            AddV02Enemy(enemies, "energy_thief_ghost", "偷", EnemyType.Ghost, 135, 8, 2.8f,
                "偷灵", "破坏供能", "准备偷取灵气",
                "镇魂符可以反制偷灵。保护聚灵石可以稳定阵法。",
                new[] { CounterTag.Ghost, CounterTag.StealEnergy }, new[] { skills["energy_thief_steal"] });

            AddV02Enemy(enemies, "seal_talisman_taoist", "封", EnemyType.EvilCultivator, 150, 10, 3f,
                "封印", "行列封锁", "准备封印符箓",
                "净化符可以解封。不要把所有输出符放在同一行或同一列。",
                new[] { CounterTag.Seal }, new[] { skills["seal_taoist_row_column"] });

            AddV02Enemy(enemies, "formation_breaker_elite", "\u7834", EnemyType.EvilCultivator, 210, 10, 2.5f,
                "\u7cbe\u82f1", "\u5c0f\u7efc\u5408", "\u8f6e\u6362\u62a4\u76fe\u3001\u53ec\u5524\u548c\u5c01\u9501\u5c0f\u538b\u529b",
                "\u96f7\u7b26\u7834\u76fe\uff0c\u8fde\u9501\u96f7\u7b26\u6e05\u7fa4\uff0c\u51c0\u5316\u7b26\u5904\u7406\u5c01\u9501\u3002",
                new[] { CounterTag.Shield, CounterTag.Group, CounterTag.Seal },
                new[] { skills["turtle_guardian_gain_shield"], skills["imp_swarm_summon"], skills["seal_taoist_row_column"] });

            AddV02Enemy(enemies, "shield_swarm_trial", "\u7fa4", EnemyType.GhostSwarm, 230, 9, 2.2f,
                "\u590d\u5408", "\u62a4\u76fe + \u7fa4\u4f53\u538b\u5236", "\u5148\u7ed3\u76fe\uff0c\u518d\u53ec\u5524\u5c0f\u5996\u538b\u5236\u9635\u76d8",
                "\u96f7\u7b26\u6253\u5f00\u62a4\u76fe\uff0c\u8fde\u9501\u96f7\u7b26\u548c\u706b\u7b26\u5904\u7406\u7fa4\u602a\u538b\u529b\u3002",
                new[] { CounterTag.Shield, CounterTag.Group, CounterTag.Summon },
                new[] { skills["turtle_guardian_gain_shield"], skills["imp_swarm_summon"] });

            AddV02Enemy(enemies, "poison_seal_thief_trial", "\u538b", EnemyType.EvilCultivator, 250, 9, 2.6f,
                "\u590d\u5408", "\u6bd2 + \u5c01 + \u5077\u7075", "\u6301\u7eed\u65bd\u538b\uff0c\u6253\u65ad\u4f9b\u80fd\u5e76\u5c01\u9501\u9635\u76d8",
                "\u51c0\u5316\u7b26\u5904\u7406\u6bd2\u548c\u5c01\u5370\uff0c\u9547\u9b42\u7b26\u53cd\u5236\u5077\u7075\u3002",
                new[] { CounterTag.Poison, CounterTag.Seal, CounterTag.StealEnergy, CounterTag.Ghost },
                new[] { skills["red_poison_apply_poison"], skills["seal_taoist_row_column"], skills["energy_thief_steal"] });

            AddV02Enemy(enemies, "formation_breaker_boss", "将", EnemyType.Boss, 240, 12, 2.8f,
                "首领", "综合压制", "准备轮换破阵手段",
                "需要破盾、清群、净化和稳定供能。",
                new[] { CounterTag.Shield, CounterTag.Group, CounterTag.Seal, CounterTag.Boss },
                new[] { skills["boss_phase_shield"], skills["boss_phase_summon"], skills["boss_phase_seal"] });

            ApplyV02EnemyBalanceProfiles(enemies, skills);
            return enemies;
        }

        private static Dictionary<string, EnemySkillDefinition> CreateV02EnemySkills()
        {
            Dictionary<string, EnemySkillDefinition> skills = new();
            AddV02Skill(skills, "turtle_guardian_gain_shield", "结盾", EnemySkillType.GainShield,
                "盾正在结盾！", "每 5 秒获得护盾", 1f, 5f, 1f, 25, 0, new[] { CounterTag.Shield });
            AddV02Skill(skills, "imp_swarm_summon", "召唤", EnemySkillType.SummonMinions,
                "召正在呼唤更多小妖！", "造成一次群体压制伤害", 1f, 6f, 1f, 10, 0, new[] { CounterTag.Group, CounterTag.Summon });
            AddV02Skill(skills, "red_poison_apply_poison", "毒火", EnemySkillType.ApplyPoison,
                "毒正在喷吐毒火！", "叠加中毒和燃烧", 1f, 4f, 1f, 1, 0, new[] { CounterTag.Poison, CounterTag.Burn });
            AddV02Skill(skills, "energy_thief_steal", "偷灵", EnemySkillType.StealEnergy,
                "偷正在偷取灵气！", "使供能源附近符箓短暂失效", 1f, 6f, 1f, 0, 3, new[] { CounterTag.Ghost, CounterTag.StealEnergy });
            AddV02Skill(skills, "seal_taoist_row_column", "封列", EnemySkillType.SealRowOrColumn,
                "封准备封印一列符箓！", "封印一行或一列符箓", 1f, 8f, 1.2f, 0, 3, new[] { CounterTag.Seal });
            AddV02Skill(skills, "boss_phase_shield", "将·盾", EnemySkillType.BossPhaseShield,
                "将正在凝聚护盾！", "获得护盾", 1f, 7f, 1f, 30, 0, new[] { CounterTag.Shield, CounterTag.Boss });
            AddV02Skill(skills, "boss_phase_summon", "将·召", EnemySkillType.BossPhaseSummon,
                "将正在召唤小妖！", "造成群体压制伤害", 3f, 9f, 1f, 12, 0, new[] { CounterTag.Group, CounterTag.Summon, CounterTag.Boss });
            AddV02Skill(skills, "boss_phase_seal", "将·封", EnemySkillType.BossPhaseSealEye,
                "将准备封印阵眼附近格子！", "封印一行或一列符箓", 5f, 11f, 1.2f, 0, 3, new[] { CounterTag.Seal, CounterTag.Boss });
            return skills;
        }

        private static void AddV02Skill(
            Dictionary<string, EnemySkillDefinition> skills,
            string skillId,
            string displayName,
            EnemySkillType skillType,
            string intentText,
            string effectDescription,
            float initialDelay,
            float cooldown,
            float castTime,
            int value,
            int duration,
            IEnumerable<CounterTag> skillTags)
        {
            string path = $"Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/Skills/{skillId}.asset";
            EnemySkillDefinition definition = AssetDatabase.LoadAssetAtPath<EnemySkillDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<EnemySkillDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            definition.skillId = skillId;
            definition.displayName = displayName;
            definition.skillType = skillType;
            definition.intentText = intentText;
            definition.effectDescription = effectDescription;
            definition.initialDelay = initialDelay;
            definition.cooldown = cooldown;
            definition.castTime = castTime;
            definition.value = value;
            definition.duration = duration;
            definition.skillTags = new List<CounterTag>(skillTags);
            EditorUtility.SetDirty(definition);
            skills[skillId] = definition;
        }

        private static void ApplyV02EnemyBalanceProfiles(Dictionary<string, EnemyDefinition> enemies, Dictionary<string, EnemySkillDefinition> skills)
        {
            if (enemies.TryGetValue("mountain_imp_basic", out EnemyDefinition mountainImp))
            {
                SetV02EnemyNumbers(mountainImp, 70, 6, 2.5f, "\u6559\u5b66", "\u666e\u901a\u653b\u51fb");
                SetV02EnemyAffinity(mountainImp,
                    Array.Empty<ElementTag>(),
                    Array.Empty<FunctionTag>(),
                    Array.Empty<ElementTag>(),
                    Array.Empty<FunctionTag>());
                mountainImp.skills = new List<EnemySkillDefinition>();
                EditorUtility.SetDirty(mountainImp);
            }

            if (enemies.TryGetValue("turtle_guardian_shield", out EnemyDefinition turtle))
            {
                SetV02EnemyNumbers(turtle, 120, 7, 2.8f, "\u62a4\u76fe", "\u5468\u671f\u62a4\u76fe");
                SetV02EnemyAffinity(turtle,
                    new[] { ElementTag.Fire },
                    new[] { FunctionTag.Burn },
                    new[] { ElementTag.Thunder },
                    new[] { FunctionTag.ShieldBreak, FunctionTag.Burst });
                SetV02SkillNumbers(skills, "turtle_guardian_gain_shield", 4f, 10f, 35, 0);
            }

            if (enemies.TryGetValue("imp_swarm", out EnemyDefinition swarm))
            {
                SetV02EnemyNumbers(swarm, 150, 8, 2f, "\u7fa4\u602a", "\u7fa4\u4f53\u538b\u5236");
                SetV02EnemyAffinity(swarm,
                    new[] { ElementTag.Sword, ElementTag.Metal },
                    new[] { FunctionTag.Burst },
                    new[] { ElementTag.Fire, ElementTag.Thunder },
                    new[] { FunctionTag.Chain, FunctionTag.AoE, FunctionTag.Burn });
                SetV02SkillNumbers(skills, "imp_swarm_summon", 4f, 7f, 12, 0);
            }

            if (enemies.TryGetValue("red_poison_beast", out EnemyDefinition poison))
            {
                SetV02EnemyNumbers(poison, 130, 7, 3f, "\u6bd2\u706b", "\u6301\u7eed\u6d88\u8017");
                SetV02EnemyAffinity(poison,
                    new[] { ElementTag.Fire },
                    new[] { FunctionTag.Burn },
                    new[] { ElementTag.Water, ElementTag.Wood },
                    new[] { FunctionTag.Cleanse, FunctionTag.AntiPoison, FunctionTag.Shield, FunctionTag.Defense });
                SetV02SkillNumbers(skills, "red_poison_apply_poison", 4f, 7f, 1, 0);
            }

            if (enemies.TryGetValue("energy_thief_ghost", out EnemyDefinition thief))
            {
                SetV02EnemyNumbers(thief, 135, 8, 2.8f, "\u5077\u7075", "\u7834\u574f\u4f9b\u80fd");
                SetV02EnemyAffinity(thief,
                    Array.Empty<ElementTag>(),
                    new[] { FunctionTag.EnergySource },
                    new[] { ElementTag.Soul },
                    new[] { FunctionTag.AntiGhost, FunctionTag.Enhance });
                SetV02SkillNumbers(skills, "energy_thief_steal", 4f, 7f, 0, 4);
            }

            if (enemies.TryGetValue("seal_talisman_taoist", out EnemyDefinition seal))
            {
                SetV02EnemyNumbers(seal, 155, 8, 3f, "\u5c01\u5370", "\u884c\u5217\u5c01\u9501");
                SetV02EnemyAffinity(seal,
                    Array.Empty<ElementTag>(),
                    new[] { FunctionTag.Burst },
                    new[] { ElementTag.Water },
                    new[] { FunctionTag.Cleanse, FunctionTag.AntiSeal });
                SetV02SkillNumbers(skills, "seal_taoist_row_column", 5f, 8f, 0, 3, 1.2f);
            }

            if (enemies.TryGetValue("formation_breaker_elite", out EnemyDefinition elite))
            {
                SetV02EnemyNumbers(elite, 210, 10, 2.5f, "\u7cbe\u82f1", "\u5c0f\u7efc\u5408");
                SetV02EnemyAffinity(elite,
                    new[] { ElementTag.Fire },
                    new[] { FunctionTag.Damage },
                    new[] { ElementTag.Thunder, ElementTag.Water },
                    new[] { FunctionTag.ShieldBreak, FunctionTag.Chain, FunctionTag.AoE, FunctionTag.Cleanse, FunctionTag.AntiSeal });
            }

            if (enemies.TryGetValue("shield_swarm_trial", out EnemyDefinition shieldSwarm))
            {
                SetV02EnemyNumbers(shieldSwarm, 230, 9, 2.2f, "\u590d\u5408", "\u62a4\u76fe + \u7fa4\u4f53\u538b\u5236");
                SetV02EnemyAffinity(shieldSwarm,
                    new[] { ElementTag.Sword, ElementTag.Metal },
                    new[] { FunctionTag.Burst },
                    new[] { ElementTag.Thunder, ElementTag.Fire },
                    new[] { FunctionTag.ShieldBreak, FunctionTag.Chain, FunctionTag.AoE, FunctionTag.Burn });
            }

            if (enemies.TryGetValue("poison_seal_thief_trial", out EnemyDefinition pressure))
            {
                SetV02EnemyNumbers(pressure, 250, 9, 2.6f, "\u590d\u5408", "\u6bd2 + \u5c01 + \u5077\u7075");
                SetV02EnemyAffinity(pressure,
                    new[] { ElementTag.Fire },
                    new[] { FunctionTag.Burst },
                    new[] { ElementTag.Water, ElementTag.Soul },
                    new[] { FunctionTag.Cleanse, FunctionTag.AntiGhost, FunctionTag.AntiSeal, FunctionTag.AntiPoison });
            }

            if (enemies.TryGetValue("formation_breaker_boss", out EnemyDefinition boss))
            {
                SetV02EnemyNumbers(boss, 260, 12, 2.8f, "Boss", "\u7efc\u5408\u538b\u529b");
                SetV02EnemyAffinity(boss,
                    new[] { ElementTag.Fire },
                    new[] { FunctionTag.Damage },
                    new[] { ElementTag.Thunder, ElementTag.Soul, ElementTag.Water },
                    new[] { FunctionTag.ShieldBreak, FunctionTag.Chain, FunctionTag.AoE, FunctionTag.Cleanse, FunctionTag.AntiGhost, FunctionTag.AntiSeal });
                SetV02SkillNumbers(skills, "boss_phase_shield", 4f, 5f, 30, 0);
                SetV02SkillNumbers(skills, "boss_phase_summon", 4f, 6f, 12, 0);
                SetV02SkillNumbers(skills, "boss_phase_seal", 5f, 7f, 0, 3, 1.2f);
            }
        }

        private static void SetV02EnemyNumbers(EnemyDefinition enemy, int hp, int damage, float interval, string enemyClass, string archetype)
        {
            enemy.maxHp = hp;
            enemy.attackDamage = damage;
            enemy.attackInterval = interval;
            enemy.enemyClass = enemyClass;
            enemy.enemyArchetype = archetype;
            enemy.dangerText = archetype;
            EditorUtility.SetDirty(enemy);
        }

        private static void SetV02EnemyAffinity(
            EnemyDefinition enemy,
            IEnumerable<ElementTag> resistedElements,
            IEnumerable<FunctionTag> resistedFunctions,
            IEnumerable<ElementTag> vulnerableElements,
            IEnumerable<FunctionTag> vulnerableFunctions)
        {
            enemy.resistedElements = new List<ElementTag>(resistedElements);
            enemy.resistedFunctions = new List<FunctionTag>(resistedFunctions);
            enemy.vulnerableElements = new List<ElementTag>(vulnerableElements);
            enemy.vulnerableFunctions = new List<FunctionTag>(vulnerableFunctions);
            EditorUtility.SetDirty(enemy);
        }

        private static void SetV02SkillNumbers(Dictionary<string, EnemySkillDefinition> skills, string skillId, float initialDelay, float cooldown, int value, int duration, float castTime = 1f)
        {
            if (!skills.TryGetValue(skillId, out EnemySkillDefinition skill) || skill == null)
            {
                return;
            }

            skill.initialDelay = initialDelay;
            skill.cooldown = cooldown;
            skill.castTime = castTime;
            skill.value = value;
            skill.duration = duration;
            EditorUtility.SetDirty(skill);
        }

        private static void AddV02Enemy(
            Dictionary<string, EnemyDefinition> enemies,
            string enemyId,
            string displayName,
            EnemyType enemyType,
            int hp,
            int damage,
            float interval,
            string enemyClass,
            string archetype,
            string intentText,
            string recommendedCounterText,
            IEnumerable<CounterTag> weaknessTags,
            IEnumerable<EnemySkillDefinition> skills)
        {
            string path = $"Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/{enemyId}.asset";
            EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }

            List<CounterTag> weaknesses = new(weaknessTags);
            definition.enemyId = enemyId;
            definition.displayName = displayName;
            definition.enemyType = enemyType;
            definition.maxHp = hp;
            definition.attackDamage = damage;
            definition.attackInterval = interval;
            definition.weaknessText = TalismanTagUtility.JoinCounterTags(weaknesses);
            definition.dangerText = archetype;
            definition.recommendedBuildText = recommendedCounterText;
            definition.enemyClass = enemyClass;
            definition.enemyArchetype = archetype;
            definition.intentText = intentText;
            definition.recommendedCounterText = recommendedCounterText;
            definition.weaknessTags = weaknesses;
            definition.resistTags = new List<CounterTag>();
            definition.skills = new List<EnemySkillDefinition>(skills);
            definition.chargeInterval = 0f;
            definition.chargeDuration = 0f;
            definition.chargeAttackDamage = 0;
            definition.manaDrainInterval = 0f;
            definition.manaDrainAmount = 0;
            definition.sealInterval = 0f;
            definition.sealDuration = 0f;
            definition.ghostShadowInterval = 0f;
            definition.ghostShadowDamage = 0;
            EditorUtility.SetDirty(definition);
            enemies[enemyId] = definition;
        }

        private static List<EnemyDefinition> CreateV02EnemyList(Dictionary<string, EnemyDefinition> enemies)
        {
            string[] ids =
            {
                "mountain_imp_basic",
                "turtle_guardian_shield",
                "imp_swarm",
                "red_poison_beast",
                "energy_thief_ghost",
                "seal_talisman_taoist",
                "formation_breaker_elite",
                "shield_swarm_trial",
                "poison_seal_thief_trial",
                "formation_breaker_boss"
            };

            List<EnemyDefinition> result = new();
            foreach (string id in ids)
            {
                if (enemies.TryGetValue(id, out EnemyDefinition enemy) && enemy != null)
                {
                    result.Add(enemy);
                }
            }

            return result;
        }

        private static V02RunConfig CreateV02RunConfig(Dictionary<string, EnemyDefinition> enemies)
        {
            const string path = "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset";
            V02RunConfig config = AssetDatabase.LoadAssetAtPath<V02RunConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<V02RunConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.runId = "v02_main_trial_1_10";
            config.displayName = "V0.2 Main Trial 1-10";
            config.rounds = new List<V02RoundConfig>
            {
                CreateV02Round(1, "\u653b\uff1a\u57fa\u7840\u5e03\u9635", enemies["mountain_imp_basic"],
                    "\u7406\u89e3\u7b26\u7b93\u5fc5\u987b\u88ab\u9635\u773c\u6216\u805a\u7075\u77f3\u4f9b\u80fd\u624d\u4f1a\u89e6\u53d1\u3002",
                    "\u628a\u706b\u7b26\u653e\u5230\u9635\u773c\u6216\u805a\u7075\u77f3\u4f9b\u80fd\u8303\u56f4\u5185\u3002\u672a\u4f9b\u80fd\u7684\u7b26\u7b93\u4e0d\u4f1a\u89e6\u53d1\u3002",
                    false, 45f, 60f, 0.2f, 0.3f),
                CreateV02Round(2, "\u76fe\uff1a\u5468\u671f\u62a4\u76fe", enemies["turtle_guardian_shield"],
                    "\u7406\u89e3\u62a4\u76fe\u654c\u4eba\u9700\u8981\u96f7\u7b26\u6216\u7206\u53d1\u5904\u7406\u3002",
                    "\u654c\u4eba\u4f1a\u5468\u671f\u6027\u83b7\u5f97\u62a4\u76fe\u3002\u96f7\u7b26\u5bf9\u62a4\u76fe\u6548\u679c\u66f4\u597d\uff0c\u5251\u4e38\u7206\u53d1\u4e5f\u6709\u6548\u3002",
                    false, 75f, 100f, 0.3f, 0.4f),
                CreateV02Round(3, "\u53ec\uff1a\u7fa4\u4f53\u538b\u5236", enemies["imp_swarm"],
                    "\u7406\u89e3\u591a\u76ee\u6807\u654c\u4eba\u9700\u8981\u8fde\u9501\u3001\u8303\u56f4\u6216\u71c3\u70e7\u5904\u7406\u3002",
                    "\u53ec\u4f1a\u7528\u6570\u91cf\u538b\u5236\u4f60\u3002\u8fde\u9501\u96f7\u7b26\u548c\u706b\u7b26\u71c3\u70e7\u66f4\u9002\u5408\u6e05\u7fa4\u3002",
                    false, 80f, 110f, 0.35f, 0.45f),
                CreateV02Round(4, "\u6bd2\uff1a\u6301\u7eed\u4f24\u5bb3", enemies["red_poison_beast"],
                    "\u7406\u89e3\u4e0d\u80fd\u53ea\u5806\u8f93\u51fa\uff0c\u9632\u5fa1\u548c\u51c0\u5316\u4e5f\u91cd\u8981\u3002",
                    "\u6bd2\u4f1a\u53e0\u52a0\u4e2d\u6bd2\u548c\u71c3\u70e7\u3002\u51c0\u5316\u7b26\u53ef\u4ee5\u6e05\u9664\u72b6\u6001\uff0c\u62a4\u8eab\u7b26\u53ef\u4ee5\u62b5\u6297\u6301\u7eed\u4f24\u5bb3\u3002",
                    false, 90f, 120f, 0.4f, 0.5f),
                CreateV02Round(5, "\u5077\uff1a\u4f9b\u80fd\u5e72\u6270", enemies["energy_thief_ghost"],
                    "\u7406\u89e3\u9635\u773c\u548c\u805a\u7075\u77f3\u662f\u6838\u5fc3\u8d44\u6e90\uff0c\u4f1a\u88ab\u654c\u4eba\u9488\u5bf9\u3002",
                    "\u5077\u4f1a\u5077\u53d6\u9635\u773c\u6216\u805a\u7075\u77f3\u7075\u6c14\uff0c\u4f7f\u5468\u56f4\u7b26\u7b93\u77ed\u6682\u5931\u6548\u3002\u9547\u9b42\u7b26\u53ef\u4ee5\u53cd\u5236\u5077\u7075\u3002",
                    false, 90f, 120f, 0.42f, 0.52f),
                CreateV02Round(6, "\u5c01\uff1a\u884c\u5217\u5c01\u9501", enemies["seal_talisman_taoist"],
                    "\u7406\u89e3\u6838\u5fc3\u8f93\u51fa\u4e0d\u80fd\u5168\u90e8\u5806\u5728\u540c\u4e00\u884c\u6216\u540c\u4e00\u5217\u3002",
                    "\u5c01\u4f1a\u5c01\u5370\u4e00\u884c\u6216\u4e00\u5217\u7b26\u7b93\u3002\u51c0\u5316\u7b26\u53ef\u4ee5\u89e3\u5c01\uff0c\u4e0d\u8981\u628a\u6240\u6709\u6838\u5fc3\u8f93\u51fa\u5806\u5728\u540c\u4e00\u6761\u7ebf\u4e0a\u3002",
                    false, 100f, 130f, 0.45f, 0.55f),
                CreateV02Round(7, "\u7834\uff1a\u7efc\u5408\u538b\u5236", enemies["formation_breaker_elite"],
                    "\u7efc\u5408\u68c0\u9a8c\u7834\u76fe\u3001\u6e05\u7fa4\u3001\u9635\u773c\u4fdd\u62a4\u548c\u9635\u6cd5\u7a33\u5b9a\u6027\u3002",
                    "\u7834\u4f1a\u8f6e\u6362\u62a4\u76fe\u3001\u53ec\u5524\u548c\u5c01\u9501\u5c0f\u538b\u529b\u3002\u9700\u8981\u7834\u76fe\u3001\u6e05\u7fa4\u3001\u51c0\u5316\u548c\u7a33\u5b9a\u4f9b\u80fd\u3002",
                    false, 110f, 140f, 0.5f, 0.6f),
                CreateV02Round(8, "\u7ec4\uff1a\u62a4\u76fe\u7fa4\u538b", enemies["shield_swarm_trial"],
                    "\u540c\u65f6\u68c0\u9a8c\u7834\u76fe\u4e0e\u6e05\u7fa4\uff0c\u9635\u76d8\u9700\u8981\u517c\u987e\u5355\u70b9\u7206\u53d1\u548c\u8303\u56f4\u538b\u5236\u3002",
                    "\u654c\u4eba\u4f1a\u5148\u7ed3\u76fe\u518d\u53ec\u5524\u5c0f\u5996\u3002\u96f7\u7b26\u7834\u76fe\uff0c\u8fde\u9501\u96f7\u7b26\u548c\u706b\u7b26\u5904\u7406\u7fa4\u602a\u3002",
                    false, 60f, 70f, 0.55f, 0.65f),
                CreateV02Round(9, "\u538b\uff1a\u6bd2\u5c01\u5077\u7075", enemies["poison_seal_thief_trial"],
                    "\u68c0\u9a8c\u51c0\u5316\u3001\u9547\u9b42\u548c\u4f9b\u80fd\u5197\u4f59\uff0c\u4e0d\u80fd\u53ea\u9760\u8f93\u51fa\u786c\u6253\u3002",
                    "\u654c\u4eba\u4f1a\u53e0\u52a0\u6bd2\u3001\u5c01\u9501\u9635\u76d8\u5e76\u5077\u53d6\u4f9b\u80fd\u3002\u51c0\u5316\u7b26\u548c\u9547\u9b42\u7b26\u662f\u5173\u952e\u53cd\u5236\u3002",
                    false, 70f, 80f, 0.6f, 0.7f),
                CreateV02Round(10, "\u9996\uff1a\u5165\u95e8\u7834\u9635\u5996", enemies["formation_breaker_boss"],
                    "\u4f5c\u4e3a\u5165\u95e8 Boss \u5173\uff0c\u7efc\u5408\u68c0\u9a8c\u7834\u76fe\u3001\u6e05\u7fa4\u3001\u51c0\u5316\u3001\u9547\u9b42\u4e0e\u9635\u773c\u4fdd\u62a4\u3002",
                    "Boss \u4f1a\u5206\u9636\u6bb5\u4f7f\u7528\u62a4\u76fe\u3001\u53ec\u5524\u3001\u5c01\u9501\u4e0e\u4f9b\u80fd\u5e72\u6270\u3002\u786e\u4fdd\u6838\u5fc3\u7b26\u7b93\u90fd\u88ab\u4f9b\u80fd\u3002",
                    true, 90f, 110f, 0.65f, 0.75f)
            };

            EditorUtility.SetDirty(config);
            return config;
        }

        private static V02RoundConfig CreateV02Round(int index, string title, EnemyDefinition enemy, string teachingGoal, string preBattleHint, bool isBossRound, float minDuration, float maxDuration, float minHpLoss, float maxHpLoss)
        {
            return new V02RoundConfig
            {
                levelId = $"1-{index}",
                roundIndex = index,
                roundTitle = title,
                enemy = enemy,
                intendedRole = teachingGoal,
                teachingGoal = teachingGoal,
                preBattleHint = preBattleHint,
                isBossRound = isBossRound,
                targetDurationMin = minDuration,
                targetDurationMax = maxDuration,
                targetHpLossMin = minHpLoss,
                targetHpLossMax = maxHpLoss,
                benchmarkRule = new BenchmarkPassFailRule
                {
                    passDurationMax = maxDuration,
                    weakDurationMax = maxDuration * 1.25f,
                    passHpLossMax = maxHpLoss,
                    weakHpLossMax = Mathf.Clamp01(maxHpLoss + 0.2f)
                }
            };
        }

        private static V02RewardPoolConfig CreateV02RewardPool(Dictionary<string, TalismanItemDefinition> items)
        {
            const string poolPath = "Assets/_Game/ScriptableObjects/TalismanBag/V02/Rewards/RewardPool_V02_Basic.asset";
            V02RewardPoolConfig pool = AssetDatabase.LoadAssetAtPath<V02RewardPoolConfig>(poolPath);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<V02RewardPoolConfig>();
                AssetDatabase.CreateAsset(pool, poolPath);
            }

            List<V02RewardDefinition> rewards = new()
            {
                AddV02Reward("reward_add_thunder_talisman", "获得雷符", V02RewardType.NewTalisman, "获得 1 张雷符。", "用于破盾。", items["thunder_talisman_basic"], V02FormationModifierType.None, V02BuildModifierType.None, 0f, new[] { CounterTag.Shield }, new[] { FunctionTag.ShieldBreak }, 12),
                AddV02Reward("reward_add_sword_pill", "获得剑丸", V02RewardType.NewTalisman, "获得 1 枚剑丸。", "用于单体爆发，适合打 Boss 和蓄力敌人。", items["sword_pill_basic"], V02FormationModifierType.None, V02BuildModifierType.None, 0f, new[] { CounterTag.Charge, CounterTag.Boss }, new[] { FunctionTag.Burst }, 10),
                AddV02Reward("reward_add_chain_thunder", "获得连锁雷符", V02RewardType.NewTalisman, "获得 1 张连锁雷符。", "用于清群和处理召唤。", items["chain_thunder_talisman_basic"], V02FormationModifierType.None, V02BuildModifierType.None, 0f, new[] { CounterTag.Group, CounterTag.Summon }, new[] { FunctionTag.Chain, FunctionTag.AoE }, 10),
                AddV02Reward("reward_add_purify_talisman", "获得净化符", V02RewardType.NewTalisman, "获得 1 张净化符。", "清除中毒、燃烧和封印。", items["purify_talisman_basic"], V02FormationModifierType.None, V02BuildModifierType.None, 0f, new[] { CounterTag.Poison, CounterTag.Burn, CounterTag.Seal }, new[] { FunctionTag.Cleanse, FunctionTag.AntiSeal }, 11),
                AddV02Reward("reward_add_soul_suppress", "获得镇魂符", V02RewardType.NewTalisman, "获得 1 张镇魂符。", "反制鬼修和偷灵。", items["soul_suppress_talisman_basic"], V02FormationModifierType.None, V02BuildModifierType.None, 0f, new[] { CounterTag.Ghost, CounterTag.StealEnergy }, new[] { FunctionTag.AntiGhost }, 11),
                AddV02Reward("reward_add_spirit_stone", "获得聚灵石", V02RewardType.NewTalisman, "获得 1 块聚灵石。", "扩展阵法供能范围。", items["spirit_stone_basic"], V02FormationModifierType.None, V02BuildModifierType.None, 0f, new[] { CounterTag.StealEnergy, CounterTag.Seal, CounterTag.Boss }, new[] { FunctionTag.EnergySource }, 9),
                AddV02Reward("reward_upgrade_eye_core_nine_grid", "阵眼强化", V02RewardType.FormationModifier, "阵眼供能变为九宫格。", "提升阵法稳定性，让更多符箓被阵眼直接供能。", null, V02FormationModifierType.UpgradeEyeCoreToNineGrid, V02BuildModifierType.None, 0f, new[] { CounterTag.StealEnergy, CounterTag.Seal, CounterTag.Boss }, new[] { FunctionTag.Enhance }, 9),
                AddV02Reward("reward_spirit_link_between_stones", "灵气连线", V02RewardType.FormationModifier, "同排或同列聚灵石之间形成供能连线。", "连线经过的格子视为被供能。", null, V02FormationModifierType.SpiritLinkBetweenStones, V02BuildModifierType.None, 0f, new[] { CounterTag.Group, CounterTag.StealEnergy, CounterTag.Seal, CounterTag.Boss }, new[] { FunctionTag.EnergySource, FunctionTag.Enhance }, 8),
                AddV02Reward("reward_outer_ring_defense_boost", "外圈护阵", V02RewardType.FormationModifier, "外圈护身符护盾 +30%，外圈净化符冷却 -15%。", "鼓励把防御符放到阵法外圈形成防护结构。", null, V02FormationModifierType.OuterRingDefenseBoost, V02BuildModifierType.None, 0f, new[] { CounterTag.Poison, CounterTag.Burn, CounterTag.Seal, CounterTag.Charge, CounterTag.Boss }, new[] { FunctionTag.Defense, FunctionTag.Cleanse }, 8),
                AddV02Reward("reward_fire_burn_plus_one", "火符燃烧强化", V02RewardType.BuildModifier, "火符命中时额外叠加 1 层燃烧。", "强化火符持续输出，对召有帮助。", null, V02FormationModifierType.None, V02BuildModifierType.FireBurnPlusOne, 1f, new[] { CounterTag.Group, CounterTag.Summon }, new[] { FunctionTag.Burn }, 8),
                AddV02Reward("reward_thunder_shieldbreak_boost", "雷符破盾强化", V02RewardType.BuildModifier, "雷符对护盾伤害倍率提高。", "雷符破盾倍率从 2.0 提高到 2.5。", null, V02FormationModifierType.None, V02BuildModifierType.ThunderShieldBreakBoost, 0.5f, new[] { CounterTag.Shield, CounterTag.Boss }, new[] { FunctionTag.ShieldBreak }, 9),
                AddV02Reward("reward_sword_crit_boost", "剑丸爆发强化", V02RewardType.BuildModifier, "剑丸有概率造成额外伤害。", "剑丸 25% 概率造成 1.8 倍伤害。", null, V02FormationModifierType.None, V02BuildModifierType.SwordCritBoost, 0.25f, new[] { CounterTag.Charge, CounterTag.Boss }, new[] { FunctionTag.Burst }, 8),
                AddV02Reward("reward_shield_amount_boost", "护身符强化", V02RewardType.BuildModifier, "护身符生成的护盾提高。", "护盾 +30%。", null, V02FormationModifierType.None, V02BuildModifierType.ShieldAmountBoost, 0.3f, new[] { CounterTag.Poison, CounterTag.Burn, CounterTag.Charge, CounterTag.Boss }, new[] { FunctionTag.Shield, FunctionTag.Defense }, 8),
                AddV02Reward("reward_cleanse_cooldown_reduction", "净化符强化", V02RewardType.BuildModifier, "净化符冷却降低。", "净化符冷却 -25%。", null, V02FormationModifierType.None, V02BuildModifierType.CleanseCooldownReduction, 0.25f, new[] { CounterTag.Poison, CounterTag.Burn, CounterTag.Seal }, new[] { FunctionTag.Cleanse }, 8)
            };

            pool.poolId = "RewardPool_V02_Basic";
            pool.rewards = rewards;
            EditorUtility.SetDirty(pool);
            return pool;
        }

        private static V02RewardDefinition AddV02Reward(
            string rewardId,
            string displayName,
            V02RewardType rewardType,
            string shortDescription,
            string detailedDescription,
            TalismanItemDefinition talismanToAdd,
            V02FormationModifierType formationModifierType,
            V02BuildModifierType buildModifierType,
            float modifierValue,
            IEnumerable<CounterTag> helpfulAgainstTags,
            IEnumerable<FunctionTag> relatedFunctionTags,
            int baseWeight)
        {
            string path = $"Assets/_Game/ScriptableObjects/TalismanBag/V02/Rewards/{rewardId}.asset";
            V02RewardDefinition reward = AssetDatabase.LoadAssetAtPath<V02RewardDefinition>(path);
            if (reward == null)
            {
                reward = ScriptableObject.CreateInstance<V02RewardDefinition>();
                AssetDatabase.CreateAsset(reward, path);
            }

            reward.rewardId = rewardId;
            reward.displayName = displayName;
            reward.rewardType = rewardType;
            reward.shortDescription = shortDescription;
            reward.detailedDescription = detailedDescription;
            reward.talismanToAdd = talismanToAdd;
            reward.formationModifierType = formationModifierType;
            reward.buildModifierType = buildModifierType;
            reward.modifierValue = modifierValue;
            reward.helpfulAgainstTags = new List<CounterTag>(helpfulAgainstTags);
            reward.relatedFunctionTags = new List<FunctionTag>(relatedFunctionTags);
            reward.baseWeight = baseWeight;
            EditorUtility.SetDirty(reward);
            return reward;
        }

        private static V02CounterMultiplierConfig CreateV02CounterMultiplierConfig()
        {
            const string path = "Assets/_Game/ScriptableObjects/TalismanBag/V02/Balance/CounterMultiplierConfig_V02.asset";
            V02CounterMultiplierConfig config = AssetDatabase.LoadAssetAtPath<V02CounterMultiplierConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<V02CounterMultiplierConfig>();
                AssetDatabase.CreateAsset(config, path);
                config.strongCounterMultiplier = 1.8f;
                config.lightCounterMultiplier = 1.35f;
                config.neutralMultiplier = 1f;
                config.resistedMultiplier = 0.7f;
                config.hardResistedMultiplier = 0.55f;
                config.rewardShieldBreakMultiplier = 2.5f;
                config.groupClearMultiplier = 1.6f;
            }

            EditorUtility.SetDirty(config);
            return config;
        }

        private static V02FormationBalanceConfig CreateV02FormationBalanceConfig()
        {
            const string path = "Assets/_Game/ScriptableObjects/TalismanBag/V02/Balance/FormationBalanceConfig_V02.asset";
            V02FormationBalanceConfig config = AssetDatabase.LoadAssetAtPath<V02FormationBalanceConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<V02FormationBalanceConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            return config;
        }

        private static void CreateV02FormationCounterScene(Dictionary<string, TalismanItemDefinition> definitions, EnemyDefinition testEnemy, Dictionary<string, EnemyDefinition> testEnemies, V02RewardPoolConfig rewardPool, V02RunConfig runConfig, V02CounterMultiplierConfig multiplierConfig, V02FormationBalanceConfig formationConfig)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            CreateStretchPanel("Background", canvasObject.transform, new Color(0.065f, 0.07f, 0.062f));
            Transform uiRoot = CreateSafeAreaRoot(canvasObject.transform);

            GameObject gridModel = new("TalismanBagGrid", typeof(TalismanBagGrid));
            gridModel.transform.SetParent(uiRoot, false);
            TalismanBagGrid grid = gridModel.GetComponent<TalismanBagGrid>();
            SetField(grid, "width", 5);
            SetField(grid, "height", 5);
            grid.Initialize(5, 5);

            GameObject systems = new("TalismanBagV02Systems",
                typeof(TalismanCombatUI),
                typeof(BattleLogUI),
                typeof(AutoCombatController),
                typeof(ComboResolver),
                typeof(BattleEventRouter),
                typeof(BattleStatsTracker),
                typeof(ComboHighlightController),
                typeof(FloatingCombatText),
                typeof(TalismanTriggerFeedback),
                typeof(EnemyFeedbackController),
                typeof(BattleResultPanel),
                typeof(FormationPowerResolver),
                typeof(FormationPowerUI),
                typeof(EnemySkillController),
                typeof(CounterMatchResolver),
                typeof(CounterFeedbackController),
                typeof(V02FailureTracker),
                typeof(V02FailureReasonResolver),
                typeof(V02RunStatsTracker),
                typeof(BattleBalanceLogger),
                typeof(V02BuildTestRunner),
                typeof(V02BossPhaseController),
                typeof(TalismanBag.Inventory.PlayerTalismanInventory),
                typeof(V02RunModifierState),
                typeof(V02RewardInventoryAdapter),
                typeof(V02RewardController),
                typeof(V02RunFlowController),
                typeof(V02RewardPanel),
                typeof(V02EnemyIntentUI),
                typeof(V02EnemyPreviewPanel),
                typeof(V02TalismanTooltipUI),
                typeof(V02RunResultPanel),
                typeof(V02DebugPopupController),
                typeof(V02FormationDebugController));
            systems.transform.SetParent(uiRoot, false);

            TalismanCombatUI combatUI = systems.GetComponent<TalismanCombatUI>();
            BattleLogUI battleLogUI = systems.GetComponent<BattleLogUI>();
            AutoCombatController combat = systems.GetComponent<AutoCombatController>();
            ComboResolver comboResolver = systems.GetComponent<ComboResolver>();
            BattleEventRouter eventRouter = systems.GetComponent<BattleEventRouter>();
            BattleStatsTracker statsTracker = systems.GetComponent<BattleStatsTracker>();
            ComboHighlightController comboHighlight = systems.GetComponent<ComboHighlightController>();
            FloatingCombatText floatingText = systems.GetComponent<FloatingCombatText>();
            TalismanTriggerFeedback triggerFeedback = systems.GetComponent<TalismanTriggerFeedback>();
            EnemyFeedbackController enemyFeedback = systems.GetComponent<EnemyFeedbackController>();
            BattleResultPanel resultPanel = systems.GetComponent<BattleResultPanel>();
            FormationPowerResolver powerResolver = systems.GetComponent<FormationPowerResolver>();
            FormationPowerUI powerUI = systems.GetComponent<FormationPowerUI>();
            EnemySkillController enemySkillController = systems.GetComponent<EnemySkillController>();
            CounterMatchResolver counterMatchResolver = systems.GetComponent<CounterMatchResolver>();
            CounterFeedbackController counterFeedbackController = systems.GetComponent<CounterFeedbackController>();
            V02FailureTracker failureTracker = systems.GetComponent<V02FailureTracker>();
            V02FailureReasonResolver failureReasonResolver = systems.GetComponent<V02FailureReasonResolver>();
            V02RunStatsTracker runStatsTracker = systems.GetComponent<V02RunStatsTracker>();
            BattleBalanceLogger balanceLogger = systems.GetComponent<BattleBalanceLogger>();
            V02BuildTestRunner buildTestRunner = systems.GetComponent<V02BuildTestRunner>();
            V02BossPhaseController bossPhaseController = systems.GetComponent<V02BossPhaseController>();
            TalismanBag.Inventory.PlayerTalismanInventory playerInventory = systems.GetComponent<TalismanBag.Inventory.PlayerTalismanInventory>();
            V02RunModifierState runModifierState = systems.GetComponent<V02RunModifierState>();
            V02RewardInventoryAdapter rewardInventoryAdapter = systems.GetComponent<V02RewardInventoryAdapter>();
            V02RewardController rewardController = systems.GetComponent<V02RewardController>();
            V02RunFlowController v02RunFlow = systems.GetComponent<V02RunFlowController>();
            V02RewardPanel rewardPanel = systems.GetComponent<V02RewardPanel>();
            V02EnemyIntentUI intentUI = systems.GetComponent<V02EnemyIntentUI>();
            V02EnemyPreviewPanel previewPanel = systems.GetComponent<V02EnemyPreviewPanel>();
            V02TalismanTooltipUI tooltipUI = systems.GetComponent<V02TalismanTooltipUI>();
            V02RunResultPanel runResultPanel = systems.GetComponent<V02RunResultPanel>();
            V02DebugPopupController debugPopupController = systems.GetComponent<V02DebugPopupController>();
            V02FormationDebugController debugController = systems.GetComponent<V02FormationDebugController>();
            StatusEffectController playerStatusController = CreateStatusEffectController(systems.transform, "PlayerStatusEffects");
            StatusEffectController enemyStatusController = CreateStatusEffectController(systems.transform, "EnemyStatusEffects");

            Text currentLevelText = CreateV02TopBar(uiRoot, combatUI);
            CreateV02EnemyArea(uiRoot, combatUI, out RectTransform enemyRect, out Graphic enemyGraphic, out Image chargeFill, out Text chargeText, out StatusAnchorUI enemyBuffAnchor, out StatusAnchorUI enemyDebuffAnchor);
            CreateV02CombatStage(uiRoot, combatUI, previewPanel, intentUI, out StatusAnchorUI playerBuffAnchor, out StatusAnchorUI playerDebuffAnchor);
            TalismanGridSlotView[] slots = CreateV02Grid(uiRoot, grid);
            CreateV02InfoArea(uiRoot, powerUI, tooltipUI, battleLogUI, out Text hoverHintText, out Text roundInfoText, out Text prepHintText);
            V02StageProgressBar stageProgressBar = CreateV02StageProgressBar(uiRoot);
            CreateV02BottomControls(uiRoot, definitions, grid, canvas, combat, debugController, out List<DraggableTalismanItemView> initialItems, out Transform inventoryContent, out DraggableTalismanItemView itemTemplate);
            RectTransform feedbackRoot = CreateFeedbackRoot(uiRoot);
            StatusTooltipPanel statusTooltipPanel = CreateStatusTooltipPanel(feedbackRoot);
            CreateResultPanel(uiRoot, resultPanel);
            CreateV02RunResultPanel(uiRoot, runResultPanel);
            CreateV02RewardPanel(uiRoot, rewardPanel);
            V02BossRewardPanel bossRewardPanel = V02BossRewardPanel.CreateRuntime(uiRoot);
            BossInfoPanel bossInfoPanel = BossInfoPanel.CreateRuntime(uiRoot);
            CreateV02DebugPopup(uiRoot, debugController, debugPopupController);

            foreach (TalismanGridSlotView slot in slots)
            {
                SetField(slot, "hoverHintText", hoverHintText);
            }

            SetField(comboResolver, "grid", grid);
            SetField(statsTracker, "eventRouter", eventRouter);
            SetField(statsTracker, "comboResolver", comboResolver);
            SetField(comboHighlight, "grid", grid);
            SetField(comboHighlight, "comboResolver", comboResolver);
            SetField(comboHighlight, "slotViews", slots);
            SetField(floatingText, "eventRouter", eventRouter);
            SetField(floatingText, "floatingRoot", feedbackRoot);
            SetField(floatingText, "anchorLayout", feedbackRoot.GetComponentInChildren<FloatingCombatTextAnchorLayout>(true));
            SetField(triggerFeedback, "eventRouter", eventRouter);
            SetField(enemyFeedback, "eventRouter", eventRouter);
            SetField(enemyFeedback, "enemyVisual", enemyRect);
            SetField(enemyFeedback, "enemyFlashTarget", enemyGraphic);
            SetField(enemyFeedback, "chargeFill", chargeFill);
            SetField(enemyFeedback, "chargeText", chargeText);
            SetField(battleLogUI, "eventRouter", eventRouter);
            SetField(battleLogUI, "maxLines", 3);
            ConfigureStatusAnchor(playerBuffAnchor, playerStatusController, statusTooltipPanel, StatusPolarity.Buff);
            ConfigureStatusAnchor(playerDebuffAnchor, playerStatusController, statusTooltipPanel, StatusPolarity.Debuff);
            ConfigureStatusAnchor(enemyBuffAnchor, enemyStatusController, statusTooltipPanel, StatusPolarity.Buff);
            ConfigureStatusAnchor(enemyDebuffAnchor, enemyStatusController, statusTooltipPanel, StatusPolarity.Debuff);

            SetField(enemySkillController, "combatController", combat);
            SetField(enemySkillController, "eventRouter", eventRouter);
            SetField(enemySkillController, "intentUI", intentUI);
            SetField(counterMatchResolver, "multiplierConfig", multiplierConfig);
            SetField(counterFeedbackController, "eventRouter", eventRouter);
            SetField(runStatsTracker, "eventRouter", eventRouter);
            SetField(balanceLogger, "combatController", combat);
            SetField(balanceLogger, "eventRouter", eventRouter);
            SetField(balanceLogger, "grid", grid);
            SetField(balanceLogger, "powerResolver", powerResolver);
            SetField(balanceLogger, "runStatsTracker", runStatsTracker);
            SetField(balanceLogger, "failureTracker", failureTracker);
            SetField(balanceLogger, "failureReasonResolver", failureReasonResolver);
            SetField(balanceLogger, "multiplierConfig", multiplierConfig);
            SetField(buildTestRunner, "testEnemies", CreateV02EnemyList(testEnemies));
            SetField(buildTestRunner, "multiplierConfig", multiplierConfig);
            SetField(bossPhaseController, "combatController", combat);
            SetField(bossPhaseController, "eventRouter", eventRouter);
            SetField(bossPhaseController, "failureTracker", failureTracker);
            SetField(bossPhaseController, "runStatsTracker", runStatsTracker);

            SetField(powerResolver, "grid", grid);
            SetField(powerResolver, "slotViews", slots);
            SetField(powerResolver, "formationEyePosition", FormationPowerResolver.GetDefaultEyeCorePosition(5, 5));
            SetField(powerResolver, "weakCooldownMultiplier", 1.35f);
            SetField(powerResolver, "formationBalanceConfig", formationConfig);
            SetField(powerResolver, "runModifierState", runModifierState);
            powerResolver.RefreshPowerStates();
            powerUI.Bind(powerResolver);

            SetField(rewardInventoryAdapter, "inventory", playerInventory);
            SetField(rewardInventoryAdapter, "grid", grid);
            SetField(rewardInventoryAdapter, "rootCanvas", canvas);
            SetField(rewardInventoryAdapter, "combatController", combat);
            SetField(rewardInventoryAdapter, "inventoryParent", inventoryContent);
            SetField(rewardInventoryAdapter, "itemViewTemplate", itemTemplate);

            SetField(rewardController, "rewardPool", rewardPool);
            SetField(rewardController, "inventory", playerInventory);
            SetField(rewardController, "formationPowerResolver", powerResolver);
            SetField(rewardController, "runModifierState", runModifierState);
            SetField(rewardController, "rewardPanel", rewardPanel);
            SetField(rewardController, "inventoryAdapter", rewardInventoryAdapter);
            SetField(rewardController, "battleLogUI", battleLogUI);
            SetField(rewardController, "optionCount", 3);

            SetField(v02RunFlow, "runConfig", runConfig);
            SetField(v02RunFlow, "combatController", combat);
            SetField(v02RunFlow, "rewardController", rewardController);
            SetField(v02RunFlow, "runModifierState", runModifierState);
            SetField(v02RunFlow, "failureTracker", failureTracker);
            SetField(v02RunFlow, "failureReasonResolver", failureReasonResolver);
            SetField(v02RunFlow, "runStatsTracker", runStatsTracker);
            SetField(v02RunFlow, "runResultPanel", runResultPanel);
            SetField(v02RunFlow, "bossRewardPanel", bossRewardPanel);
            SetField(v02RunFlow, "bossInfoPanel", bossInfoPanel);
            SetField(v02RunFlow, "battleLogUI", battleLogUI);
            SetField(v02RunFlow, "roundInfoText", roundInfoText);
            SetField(v02RunFlow, "prepHintText", prepHintText);
            SetField(v02RunFlow, "currentLevelText", currentLevelText);
            SetField(v02RunFlow, "stageProgressBar", stageProgressBar);
            SetField(v02RunFlow, "testEnemies", CreateV02EnemyList(testEnemies));

            SetField(combat, "grid", grid);
            SetField(combat, "comboResolver", comboResolver);
            SetField(combat, "eventRouter", eventRouter);
            SetField(combat, "statsTracker", statsTracker);
            SetField(combat, "resultPanel", resultPanel);
            SetField(combat, "comboHighlightController", comboHighlight);
            SetField(combat, "floatingCombatText", floatingText);
            SetField(combat, "talismanTriggerFeedback", triggerFeedback);
            SetField(combat, "combatUI", combatUI);
            SetField(combat, "battleLogUI", battleLogUI);
            SetField(combat, "feedbackRoot", feedbackRoot);
            SetField(combat, "v02RunFlowController", v02RunFlow);
            SetField(combat, "formationPowerResolver", powerResolver);
            SetField(combat, "enemySkillController", enemySkillController);
            SetField(combat, "counterMatchResolver", counterMatchResolver);
            SetField(combat, "counterFeedbackController", counterFeedbackController);
            SetField(combat, "v02FailureTracker", failureTracker);
            SetField(combat, "v02RunStatsTracker", runStatsTracker);
            SetField(combat, "v02BossPhaseController", bossPhaseController);
            SetField(combat, "v02CounterMultiplierConfig", multiplierConfig);
            SetField(combat, "battleBalanceLogger", balanceLogger);
            SetField(combat, "v02RunModifierState", runModifierState);
            SetField(combat, "v02EnemyIntentUI", intentUI);
            SetField(combat, "v02EnemyPreviewPanel", previewPanel);
            SetField(combat, "playerStatusController", playerStatusController);
            SetField(combat, "enemyStatusController", enemyStatusController);
            SetField(combat, "playerBuffAnchor", playerBuffAnchor);
            SetField(combat, "playerDebuffAnchor", playerDebuffAnchor);
            SetField(combat, "enemyBuffAnchor", enemyBuffAnchor);
            SetField(combat, "enemyDebuffAnchor", enemyDebuffAnchor);
            SetField(combat, "statusTooltipPanel", statusTooltipPanel);
            SetField(combat, "slotViews", slots);
            SetField(combat, "itemViews", initialItems);

            SetField(debugController, "grid", grid);
            SetField(debugController, "combatController", combat);
            SetField(debugController, "powerResolver", powerResolver);
            SetField(debugController, "v02RunFlowController", v02RunFlow);
            SetField(debugController, "rewardController", rewardController);
            SetField(debugController, "runModifierState", runModifierState);
            SetField(debugController, "failureTracker", failureTracker);
            SetField(debugController, "runStatsTracker", runStatsTracker);
            SetField(debugController, "buildTestRunner", buildTestRunner);
            SetField(debugController, "formationPowerUI", powerUI);
            SetField(debugController, "tooltipUI", tooltipUI);
            SetField(debugController, "battleLogUI", battleLogUI);
            SetField(debugController, "testEnemy", testEnemy);
            SetField(debugController, "testEnemies", CreateV02EnemyList(testEnemies));
            SetField(debugController, "slotViews", slots);
            SetField(debugController, "itemViews", initialItems);

            foreach (DraggableTalismanItemView item in initialItems)
            {
                SetField(item, "combatController", combat);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, V02ScenePath);
            AssetDatabase.ImportAsset(V02ScenePath);
        }

        private static Text CreateV02TopBar(Transform parent, TalismanCombatUI combatUI)
        {
            GameObject topBar = CreatePanel("V02TopStatusBar", parent, new Vector2(0f, -12f), new Vector2(1020f, 128f), new Color(0.105f, 0.12f, 0.095f), TextAnchor.UpperCenter);
            GridLayoutGroup layout = topBar.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(240f, 46f);
            layout.spacing = new Vector2(10f, 10f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            layout.padding = new RectOffset(14, 14, 14, 14);
            layout.childAlignment = TextAnchor.MiddleCenter;

            Text hp = CreateStatusText("HPText", topBar.transform, "100/100 \u6c14\u8840", new Color(1f, 0.86f, 0.78f));
            Image hpFill = CreateHpBarForStatusText(hp);
            Text shield = CreateStatusText("ShieldText", topBar.transform, "\u62a4\u76fe 0", new Color(0.78f, 0.94f, 1f));
            Text mana = CreateStatusText("ManaText", topBar.transform, "0/100 \u7075\u6c14", new Color(0.72f, 0.9f, 1f));
            Text state = CreateStatusText("StateText", topBar.transform, "V0.2", new Color(0.82f, 1f, 0.7f));
            Text currentLevel = CreateStatusText("CurrentLevelText", topBar.transform, "1-1", new Color(1f, 0.88f, 0.56f));

            SetField(combatUI, "hpText", hp);
            SetField(combatUI, "hpFillImage", hpFill);
            SetField(combatUI, "shieldText", shield);
            SetField(combatUI, "manaText", mana);
            SetField(combatUI, "stateText", state);
            SetField(combatUI, "hpFlashTarget", hp);
            return currentLevel;
        }

        private static void CreateV02EnemyArea(
            Transform parent,
            TalismanCombatUI combatUI,
            out RectTransform enemyRect,
            out Graphic enemyGraphic,
            out Image chargeFill,
            out Text chargeText,
            out StatusAnchorUI enemyBuffAnchor,
            out StatusAnchorUI enemyDebuffAnchor)
        {
            GameObject panel = CreatePanel("V02EnemyArea", parent, new Vector2(0f, -156f), new Vector2(1020f, 248f), new Color(0.095f, 0.105f, 0.088f), TextAnchor.UpperCenter);
            GameObject enemy = CreatePanel("EnemySilhouette", panel.transform, new Vector2(-330f, 0f), new Vector2(196f, 196f), new Color(0.2f, 0.13f, 0.18f), TextAnchor.MiddleCenter);
            Text enemyFace = CreateText("EnemyFace", enemy.transform, "\u653b", 70, FontStyle.Bold, new Color(1f, 0.72f, 0.78f));
            SetRect(enemyFace.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            enemyBuffAnchor = CreateStatusAnchor("EnemyBuffAnchor", enemy.transform, new Vector2(116f, 82f), new Vector2(320f, 48f), StatusPolarity.Buff, TextAnchor.MiddleRight);
            enemyDebuffAnchor = CreateStatusAnchor("EnemyDebuffAnchor", enemy.transform, new Vector2(116f, 34f), new Vector2(320f, 48f), StatusPolarity.Debuff, TextAnchor.MiddleRight);

            GameObject shield = CreatePanel("ShieldFeedback", panel.transform, new Vector2(-330f, 0f), new Vector2(220f, 220f), new Color(0.25f, 0.85f, 1f, 0f), TextAnchor.MiddleCenter);
            Image shieldImage = shield.GetComponent<Image>();

            Text title = CreateText("EnemyTitle", panel.transform, "\u3010\u653b\u3011\u653b", 34, FontStyle.Bold, Color.white);
            title.alignment = TextAnchor.MiddleLeft;
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -34f), new Vector2(420f, 44f));

            Text hp = CreateText("EnemyHPText", panel.transform, "80/80 \u6c14\u8840", 28, FontStyle.Bold, new Color(1f, 0.82f, 0.7f));
            hp.alignment = TextAnchor.MiddleLeft;
            SetRect(hp.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -84f), new Vector2(520f, 40f));

            Text weak = CreateText("WeaknessTags", panel.transform, "\u65e0\u7279\u6b8a  \u53ef\u6d4b\u8bd5\u4f9b\u80fd", 24, FontStyle.Bold, new Color(0.86f, 1f, 0.72f));
            weak.alignment = TextAnchor.MiddleLeft;
            SetRect(weak.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -132f), new Vector2(540f, 36f));

            GameObject chargeTrack = CreatePanel("ChargeTrack", panel.transform, new Vector2(300f, -76f), new Vector2(520f, 22f), new Color(0.18f, 0.13f, 0.11f), TextAnchor.MiddleCenter);
            chargeFill = CreateStretchPanel("ChargeFill", chargeTrack.transform, new Color(0.95f, 0.45f, 0.22f)).GetComponent<Image>();
            RectTransform fillRect = chargeFill.GetComponent<RectTransform>();
            fillRect.anchorMax = new Vector2(0f, 1f);
            chargeText = CreateText("ChargeText", panel.transform, string.Empty, 20, FontStyle.Bold, new Color(1f, 0.76f, 0.5f));
            SetRect(chargeText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -170f), new Vector2(520f, 30f));

            SetField(combatUI, "enemyFaceText", enemyFace);
            SetField(combatUI, "enemyTitleText", title);
            SetField(combatUI, "enemyHpText", hp);
            SetField(combatUI, "enemyVisual", enemy.GetComponent<RectTransform>());
            SetField(combatUI, "enemyFlashTarget", enemy.GetComponent<Image>());
            SetField(combatUI, "shieldFeedback", shieldImage);
            enemyRect = enemy.GetComponent<RectTransform>();
            enemyGraphic = enemy.GetComponent<Image>();
        }

        private static void CreateV02CombatStage(Transform parent, TalismanCombatUI combatUI, V02EnemyPreviewPanel previewPanel, V02EnemyIntentUI intentUI, out StatusAnchorUI playerBuffAnchor, out StatusAnchorUI playerDebuffAnchor)
        {
            GameObject panel = CreatePanel("V02AutoCombatStage", parent, new Vector2(0f, -428f), new Vector2(1020f, 176f), new Color(0.085f, 0.095f, 0.08f), TextAnchor.UpperCenter);

            GameObject playerAvatar = CreatePanel("V02PlayerAvatar", panel.transform, new Vector2(-442f, 8f), new Vector2(126f, 126f), new Color(0.17f, 0.12f, 0.08f), TextAnchor.MiddleCenter);
            GameObject playerHit = CreatePanel("PlayerHitFeedback", playerAvatar.transform, Vector2.zero, new Vector2(126f, 126f), new Color(1f, 0.28f, 0.22f, 0f), TextAnchor.MiddleCenter);
            Image playerHitImage = playerHit.GetComponent<Image>();
            playerHitImage.raycastTarget = false;
            CanvasRenderer playerHitRenderer = playerHit.GetComponent<CanvasRenderer>();
            playerHitRenderer.cullTransparentMesh = false;
            SetRect(playerHit.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Text playerFace = CreateText("PlayerAvatarGlyph", playerAvatar.transform, "\u4fee", 48, FontStyle.Bold, new Color(1f, 0.86f, 0.55f));
            SetRect(playerFace.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, 8f), Vector2.zero);
            Text playerName = CreateText("PlayerAvatarName", playerAvatar.transform, "\u4fee\u58eb", 20, FontStyle.Bold, new Color(0.96f, 0.9f, 0.75f));
            SetRect(playerName.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(-18f, 26f));
            playerBuffAnchor = CreateStatusAnchor("PlayerBuffAnchor", playerAvatar.transform, new Vector2(128f, 66f), new Vector2(290f, 48f), StatusPolarity.Buff);
            playerDebuffAnchor = CreateStatusAnchor("PlayerDebuffAnchor", playerAvatar.transform, new Vector2(128f, 18f), new Vector2(290f, 48f), StatusPolarity.Debuff);
            SetField(combatUI, "playerHitFeedback", playerHitImage);

            GameObject preview = CreatePanel("V02EnemyPreviewPanel", panel.transform, new Vector2(-148f, 0f), new Vector2(410f, 142f), new Color(0.07f, 0.082f, 0.092f), TextAnchor.MiddleCenter);
            Text previewTitle = CreateText("EnemyPreviewTitle", preview.transform, "敌人：【攻】攻", 21, FontStyle.Bold, new Color(0.9f, 0.96f, 1f));
            previewTitle.alignment = TextAnchor.MiddleLeft;
            SetRect(previewTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(-28f, 28f));
            Text previewBody = CreateText("EnemyPreviewBody", preview.transform, string.Empty, 16, FontStyle.Normal, Color.white);
            previewBody.alignment = TextAnchor.UpperLeft;
            previewBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            previewBody.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(previewBody.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -18f), new Vector2(-28f, -44f));

            GameObject intent = CreatePanel("V02EnemyIntentPanel", panel.transform, new Vector2(318f, 36f), new Vector2(360f, 66f), new Color(0.13f, 0.08f, 0.075f), TextAnchor.MiddleCenter);
            Text intentTitle = CreateText("IntentTitle", intent.transform, "敌人意图", 18, FontStyle.Bold, new Color(1f, 0.82f, 0.62f));
            intentTitle.alignment = TextAnchor.MiddleLeft;
            SetRect(intentTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(-24f, 24f));
            Text intentBody = CreateText("IntentBody", intent.transform, "普通攻击", 18, FontStyle.Bold, Color.white);
            intentBody.alignment = TextAnchor.MiddleLeft;
            SetRect(intentBody.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-28f, 15f), new Vector2(-88f, 28f));
            Text intentTimer = CreateText("IntentTimer", intent.transform, string.Empty, 20, FontStyle.Bold, new Color(1f, 0.68f, 0.42f));
            intentTimer.alignment = TextAnchor.MiddleRight;
            SetRect(intentTimer.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-22f, 15f), new Vector2(78f, 28f));

            SetField(previewPanel, "panel", preview);
            SetField(previewPanel, "titleText", previewTitle);
            SetField(previewPanel, "bodyText", previewBody);
            SetField(intentUI, "panel", intent);
            SetField(intentUI, "titleText", intentTitle);
            SetField(intentUI, "intentText", intentBody);
            SetField(intentUI, "timerText", intentTimer);
        }

        private static TalismanGridSlotView[] CreateV02Grid(Transform parent, TalismanBagGrid grid)
        {
            GameObject frame = CreatePanel("V02FormationGridFrame", parent, new Vector2(0f, -610f), new Vector2(660f, 560f), new Color(0.09f, 0.105f, 0.085f), TextAnchor.UpperCenter);
            Outline outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(0.45f, 0.82f, 1f);
            outline.effectDistance = new Vector2(4f, -4f);

            GameObject gridObject = new("GridSlots_5x5", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(frame.transform, false);
            SetRect(gridObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -47f), new Vector2(600f, 600f));
            GridLayoutGroup layout = gridObject.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(112f, 112f);
            layout.spacing = new Vector2(10f, 10f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;

            List<TalismanGridSlotView> slots = new();
            for (int y = 4; y >= 0; y--)
            {
                for (int x = 0; x < 5; x++)
                {
                    Vector2Int position = new(x, y);
                    GameObject slotObject = CreateSlot(position, grid);
                    slotObject.transform.SetParent(gridObject.transform, false);
                    TalismanGridSlotView slot = slotObject.GetComponent<TalismanGridSlotView>();
                    slot.SetFormationEye(position == FormationPowerResolver.GetDefaultEyeCorePosition(5, 5));
                    slots.Add(slot);
                }
            }

            Text caption = CreateText("GridCaption", frame.transform, "5x5 V0.2 Formation Counter", 25, FontStyle.Bold, new Color(0.82f, 0.94f, 1f));
            SetRect(caption.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f), new Vector2(560f, 34f));
            return slots.ToArray();
        }

        private static void CreateV02InfoArea(Transform parent, FormationPowerUI powerUI, V02TalismanTooltipUI tooltipUI, BattleLogUI battleLogUI, out Text hoverHintText, out Text roundInfoText, out Text prepHintText)
        {
            GameObject panel = CreatePanel("V02FormationInfoArea", parent, new Vector2(0f, -1192f), new Vector2(1020f, 320f), new Color(0.085f, 0.095f, 0.078f), TextAnchor.UpperCenter);
            Button infoClose = CreateButton("FormationInfoCloseButton", panel.transform, "\u00D7", new Color(0.36f, 0.24f, 0.2f), 30);
            SetRect(infoClose.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-22f, -18f), new Vector2(54f, 54f));

            Text title = CreateText("InfoTitle", panel.transform, "\u9635\u6cd5\u4f9b\u80fd", 28, FontStyle.Bold, new Color(0.86f, 0.95f, 1f));
            title.alignment = TextAnchor.MiddleLeft;
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(440f, 40f));
            roundInfoText = title;

            Text summary = CreateText("PowerSummary", panel.transform, "\u53ef\u6fc0\u6d3b\u7b26\u7b93\uff1a0 / 0", 30, FontStyle.Bold, new Color(0.82f, 1f, 0.86f));
            summary.alignment = TextAnchor.MiddleLeft;
            SetRect(summary.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -76f), new Vector2(430f, 42f));

            Text detail = CreateText("PowerDetails", panel.transform, "\u5f31\u4f9b\u80fd\uff1a0\n\u672a\u4f9b\u80fd\uff1a0", 24, FontStyle.Bold, new Color(0.92f, 0.92f, 1f));
            detail.alignment = TextAnchor.UpperLeft;
            SetRect(detail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -122f), new Vector2(430f, 80f));

            Text hint = CreateText("PowerHint", panel.transform, "\u7b26\u7b93\u5fc5\u987b\u88ab\u9635\u773c\u6216\u805a\u7075\u77f3\u4f9b\u80fd\u624d\u4f1a\u89e6\u53d1\u3002\n\u79fb\u52a8\u805a\u7075\u77f3\uff0c\u53ef\u4ee5\u6539\u53d8\u9635\u6cd5\u8303\u56f4\u3002", 22, FontStyle.Bold, new Color(1f, 0.84f, 0.58f));
            hint.alignment = TextAnchor.UpperLeft;
            hint.horizontalOverflow = HorizontalWrapMode.Wrap;
            SetRect(hint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(500f, -202f), new Vector2(470f, 58f));

            hoverHintText = CreateText("SlotHoverHint", panel.transform, string.Empty, 21, FontStyle.Bold, new Color(0.72f, 0.9f, 1f));
            hoverHintText.alignment = TextAnchor.MiddleLeft;
            SetRect(hoverHintText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(500f, -276f), new Vector2(470f, 32f));

            Text preBattleTitle = CreateText("PreBattleHintTitle", panel.transform, "\u6218\u524d\u63d0\u793a", 22, FontStyle.Bold, new Color(1f, 0.82f, 0.45f));
            preBattleTitle.alignment = TextAnchor.MiddleLeft;
            SetRect(preBattleTitle.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -198f), new Vector2(180f, 30f));

            Text preBattleHint = CreateText("PreBattleHintText", panel.transform, "\u3010\u653b\u3011\u653b\n\u89c2\u5bdf\u654c\u4eba\u5a01\u80c1\uff0c\u5f00\u6218\u524d\u8c03\u6574\u9635\u76d8\u3002", 19, FontStyle.Bold, new Color(1f, 0.92f, 0.7f));
            preBattleHint.alignment = TextAnchor.UpperLeft;
            preBattleHint.horizontalOverflow = HorizontalWrapMode.Wrap;
            preBattleHint.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(preBattleHint.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -226f), new Vector2(430f, 54f));
            prepHintText = preBattleHint;

            GameObject logPanel = CreatePanel("BattleLogPanel", panel.transform, new Vector2(250f, 26f), new Vector2(470f, 116f), new Color(0.075f, 0.083f, 0.073f), TextAnchor.MiddleCenter);
            Text logTitle = CreateText("BattleLogTitle", logPanel.transform, "\u6700\u8fd1 3 \u6761\u5173\u952e\u65e5\u5fd7", 22, FontStyle.Bold, new Color(1f, 0.82f, 0.45f));
            SetRect(logTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -22f), new Vector2(430f, 32f));

            Text logBody = CreateText("BattleLogText", logPanel.transform, string.Empty, 20, FontStyle.Normal, Color.white);
            logBody.alignment = TextAnchor.UpperLeft;
            logBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            logBody.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(logBody.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(420f, 54f));

            GameObject tooltipPanel = CreatePanel("V02TagTooltipPanel", panel.transform, new Vector2(250f, -76f), new Vector2(470f, 104f), new Color(0.065f, 0.078f, 0.092f), TextAnchor.MiddleCenter);
            Button tooltipClose = CreateButton("TooltipCloseButton", tooltipPanel.transform, "\u00D7", new Color(0.36f, 0.24f, 0.2f), 30);
            SetRect(tooltipClose.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-22f, -18f), new Vector2(54f, 54f));
            Text tooltipTitle = CreateText("TooltipTitle", tooltipPanel.transform, "点击符箓查看标签", 21, FontStyle.Bold, new Color(0.82f, 0.94f, 1f));
            tooltipTitle.alignment = TextAnchor.MiddleLeft;
            SetRect(tooltipTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(-30f, 28f));

            Text tooltipBody = CreateText("TooltipBody", tooltipPanel.transform, string.Empty, 15, FontStyle.Normal, Color.white);
            tooltipBody.alignment = TextAnchor.UpperLeft;
            tooltipBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            tooltipBody.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(tooltipBody.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(-30f, -44f));

            SetField(powerUI, "summaryText", summary);
            SetField(powerUI, "detailText", detail);
            SetField(powerUI, "hintText", hint);
            SetField(tooltipUI, "panel", tooltipPanel);
            SetField(tooltipUI, "titleText", tooltipTitle);
            SetField(tooltipUI, "bodyText", tooltipBody);
            SetField(tooltipUI, "closeButton", tooltipClose);
            SetField(battleLogUI, "logText", logBody);
        }

        private static void CreateV02BottomControls(
            Transform parent,
            Dictionary<string, TalismanItemDefinition> definitions,
            TalismanBagGrid grid,
            Canvas canvas,
            AutoCombatController combat,
            V02FormationDebugController debugController,
            out List<DraggableTalismanItemView> initialItems,
            out Transform inventoryContent,
            out DraggableTalismanItemView itemTemplate)
        {
            GameObject bottom = CreatePanel("V02BottomOperationArea", parent, new Vector2(0f, 20f), new Vector2(1040f, 360f), new Color(0.095f, 0.108f, 0.088f), TextAnchor.LowerCenter);

            Text caption = CreateText("InventoryCaption", bottom.transform, "\u7b26\u7b93\u680f\uff1a\u62d6\u5230\u9635\u76d8\u8c03\u6574\u9635\u6cd5", 24, FontStyle.Bold, new Color(0.86f, 0.92f, 0.82f));
            caption.alignment = TextAnchor.MiddleLeft;
            SetRect(caption.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(-56f, 34f));

            GameObject scrollObject = new("InventoryScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(bottom.transform, false);
            SetRect(scrollObject.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -102f), new Vector2(980f, 128f));
            scrollObject.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.068f);

            GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            SetRect(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            SetRect(contentRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(0f, 112f));
            HorizontalLayoutGroup itemLayout = content.GetComponent<HorizontalLayoutGroup>();
            itemLayout.childForceExpandWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.childAlignment = TextAnchor.MiddleLeft;
            itemLayout.spacing = 18f;
            itemLayout.padding = new RectOffset(10, 10, 0, 0);
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            inventoryContent = content.transform;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            initialItems = new List<DraggableTalismanItemView>();
            string[] ids =
            {
                "fire_talisman_basic",
                "thunder_talisman_basic",
                "sword_pill_basic",
                "chain_thunder_talisman_basic",
                "shield_talisman_basic",
                "purify_talisman_basic",
                "soul_suppress_talisman_basic",
                "spirit_stone_basic",
                "qi_pill_basic",
                "seal_basic"
            };
            foreach (string id in ids)
            {
                GameObject item = CreateItemView(definitions[id], grid, canvas, combat);
                item.transform.SetParent(content.transform, false);
                initialItems.Add(item.GetComponent<DraggableTalismanItemView>());
            }

            itemTemplate = initialItems.Count > 0 ? initialItems[0] : null;

            GameObject actionButtons = new("V02PrimaryActionButtons", typeof(RectTransform), typeof(GridLayoutGroup));
            actionButtons.transform.SetParent(bottom.transform, false);
            SetRect(actionButtons.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(980f, 98f));
            GridLayoutGroup actionLayout = actionButtons.GetComponent<GridLayoutGroup>();
            actionLayout.cellSize = new Vector2(230f, 88f);
            actionLayout.spacing = new Vector2(18f, 0f);
            actionLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            actionLayout.constraintCount = 4;
            actionLayout.childAlignment = TextAnchor.MiddleCenter;

            Button refreshAction = CreateButton("RefreshPowerButton", actionButtons.transform, "\u5237\u65b0\u4f9b\u80fd", new Color(0.28f, 0.34f, 0.48f), 27);
            Button startAction = CreateButton("StartBattleButton", actionButtons.transform, "\u5f00\u59cb\u6597\u6cd5", new Color(0.62f, 0.32f, 0.18f), 30);
            Button resetAction = CreateButton("ResetFormationButton", actionButtons.transform, "\u91cd\u7f6e\u9635\u76d8", new Color(0.28f, 0.42f, 0.34f), 27);
            Button postBattleAction = CreateButton("PostBattlePrepareButton", actionButtons.transform, "\u6218\u540e\u9a7b\u9635", new Color(0.42f, 0.34f, 0.22f), 25);

            SetField(debugController, "refreshPowerButton", refreshAction);
            SetField(debugController, "startBattleButton", startAction);
            SetField(debugController, "resetFormationButton", resetAction);
            SetField(debugController, "postBattlePrepareButton", postBattleAction);
            return;

            /*
            GameObject buttons = new("DebugButtons", typeof(RectTransform), typeof(GridLayoutGroup));
            buttons.transform.SetParent(bottom.transform, false);
            SetRect(buttons.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(980f, 292f));
            GridLayoutGroup layout = buttons.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(134f, 40f);
            layout.spacing = new Vector2(6f, 6f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 7;
            layout.childAlignment = TextAnchor.MiddleCenter;

            Button powered = CreateButton("AutoPlacePoweredButton", buttons.transform, "摆放供能", new Color(0.24f, 0.46f, 0.5f), 18);
            Button unpowered = CreateButton("AutoPlaceUnpoweredButton", buttons.transform, "摆放断供", new Color(0.46f, 0.28f, 0.24f), 18);
            Button refresh = CreateButton("RefreshPowerButton", buttons.transform, "刷新供能", new Color(0.28f, 0.34f, 0.48f), 18);
            Button start = CreateButton("StartBattleButton", buttons.transform, "开始测试", new Color(0.62f, 0.32f, 0.18f), 18);
            Button reset = CreateButton("ResetFormationButton", buttons.transform, "重置阵盘", new Color(0.28f, 0.42f, 0.34f), 18);
            Button printTags = CreateButton("PrintSelectedTagsButton", buttons.transform, "打印标签", new Color(0.24f, 0.38f, 0.52f), 18);
            Button giveAll = CreateButton("GiveAllV02TalismansButton", buttons.transform, "发放全部", new Color(0.36f, 0.36f, 0.22f), 18);
            Button tagQuery = CreateButton("TestTagQueryButton", buttons.transform, "查询标签", new Color(0.32f, 0.28f, 0.5f), 18);
            Button counterMatch = CreateButton("TestCounterMatchButton", buttons.transform, "测试克制", new Color(0.44f, 0.26f, 0.42f), 18);
            Button spawnMountain = CreateButton("SpawnMountainImpButton", buttons.transform, "敌:小妖", new Color(0.22f, 0.36f, 0.32f), 18);
            Button spawnTurtle = CreateButton("SpawnTurtleGuardianButton", buttons.transform, "敌:护法", new Color(0.26f, 0.42f, 0.48f), 18);
            Button spawnSwarm = CreateButton("SpawnImpSwarmButton", buttons.transform, "敌:妖群", new Color(0.35f, 0.32f, 0.24f), 18);
            Button spawnPoison = CreateButton("SpawnRedPoisonBeastButton", buttons.transform, "敌:毒妖", new Color(0.42f, 0.24f, 0.22f), 18);
            Button spawnThief = CreateButton("SpawnEnergyThiefGhostButton", buttons.transform, "敌:偷灵", new Color(0.32f, 0.26f, 0.46f), 18);
            Button spawnSeal = CreateButton("SpawnSealTaoistButton", buttons.transform, "敌:封符", new Color(0.4f, 0.32f, 0.18f), 18);
            Button spawnBoss = CreateButton("SpawnFormationBreakerBossButton", buttons.transform, "敌:Boss", new Color(0.48f, 0.22f, 0.28f), 18);
            Button forceSkill = CreateButton("ForceEnemySkillButton", buttons.transform, "强制敌技", new Color(0.58f, 0.28f, 0.16f), 18);
            Button clearStatus = CreateButton("ClearPlayerStatusButton", buttons.transform, "清状态", new Color(0.22f, 0.4f, 0.36f), 18);
            Button clearSeals = CreateButton("ClearV02SealsButton", buttons.transform, "清封印", new Color(0.32f, 0.34f, 0.42f), 18);
            Button testShieldBreak = CreateButton("TestShieldBreakButton", buttons.transform, "测破盾", new Color(0.35f, 0.46f, 0.62f), 18);
            Button testCleansePoison = CreateButton("TestCleansePoisonButton", buttons.transform, "测净化", new Color(0.24f, 0.48f, 0.34f), 18);
            Button testCleanseSeal = CreateButton("TestCleanseSealButton", buttons.transform, "测解封", new Color(0.26f, 0.42f, 0.54f), 18);
            Button testSoulSuppress = CreateButton("TestSoulSuppressButton", buttons.transform, "测镇魂", new Color(0.44f, 0.28f, 0.56f), 18);
            Button testChainClear = CreateButton("TestChainClearButton", buttons.transform, "测清群", new Color(0.48f, 0.42f, 0.22f), 18);
            Button testCounterLog = CreateButton("TestCounterLogPriorityButton", buttons.transform, "测日志", new Color(0.38f, 0.34f, 0.46f), 18);
            Button openRewardPanel = CreateButton("OpenRewardPanelButton", buttons.transform, "开奖励", new Color(0.42f, 0.5f, 0.28f), 14);
            Button setNextTurtle = CreateButton("SetNextTurtleButton", buttons.transform, "下护盾", new Color(0.24f, 0.42f, 0.52f), 14);
            Button setNextSwarm = CreateButton("SetNextSwarmButton", buttons.transform, "下群怪", new Color(0.38f, 0.34f, 0.2f), 14);
            Button setNextPoison = CreateButton("SetNextPoisonButton", buttons.transform, "下毒怪", new Color(0.44f, 0.24f, 0.22f), 14);
            Button setNextThief = CreateButton("SetNextThiefButton", buttons.transform, "下偷灵", new Color(0.34f, 0.26f, 0.48f), 14);
            Button setNextSeal = CreateButton("SetNextSealButton", buttons.transform, "下封印", new Color(0.42f, 0.32f, 0.18f), 14);
            Button setNextBoss = CreateButton("SetNextBossButton", buttons.transform, "下Boss", new Color(0.5f, 0.22f, 0.3f), 14);
            Button giveEyeCore = CreateButton("GiveEyeCoreRewardButton", buttons.transform, "给阵眼", new Color(0.28f, 0.42f, 0.58f), 14);
            Button giveSpiritLink = CreateButton("GiveSpiritLinkRewardButton", buttons.transform, "给连线", new Color(0.24f, 0.46f, 0.5f), 14);
            Button giveOuterRing = CreateButton("GiveOuterRingRewardButton", buttons.transform, "给外圈", new Color(0.34f, 0.44f, 0.28f), 14);
            Button printModifiers = CreateButton("PrintRunModifiersButton", buttons.transform, "印强化", new Color(0.34f, 0.34f, 0.42f), 14);
            Button resetModifiers = CreateButton("ResetRunModifiersButton", buttons.transform, "清强化", new Color(0.42f, 0.28f, 0.24f), 14);

            SetField(debugController, "autoPlacePoweredButton", powered);
            SetField(debugController, "autoPlaceUnpoweredButton", unpowered);
            SetField(debugController, "refreshPowerButton", refresh);
            SetField(debugController, "startBattleButton", start);
            SetField(debugController, "resetFormationButton", reset);
            SetField(debugController, "printSelectedTagsButton", printTags);
            SetField(debugController, "giveAllTalismansButton", giveAll);
            SetField(debugController, "testTagQueryButton", tagQuery);
            SetField(debugController, "testCounterMatchButton", counterMatch);
            SetField(debugController, "spawnMountainImpButton", spawnMountain);
            SetField(debugController, "spawnTurtleGuardianButton", spawnTurtle);
            SetField(debugController, "spawnImpSwarmButton", spawnSwarm);
            SetField(debugController, "spawnRedPoisonBeastButton", spawnPoison);
            SetField(debugController, "spawnEnergyThiefGhostButton", spawnThief);
            SetField(debugController, "spawnSealTaoistButton", spawnSeal);
            SetField(debugController, "spawnFormationBreakerBossButton", spawnBoss);
            SetField(debugController, "forceEnemySkillButton", forceSkill);
            SetField(debugController, "clearPlayerStatusButton", clearStatus);
            SetField(debugController, "clearSealsButton", clearSeals);
            SetField(debugController, "testShieldBreakButton", testShieldBreak);
            SetField(debugController, "testCleansePoisonButton", testCleansePoison);
            SetField(debugController, "testCleanseSealButton", testCleanseSeal);
            SetField(debugController, "testSoulSuppressButton", testSoulSuppress);
            SetField(debugController, "testChainClearButton", testChainClear);
            SetField(debugController, "testCounterLogPriorityButton", testCounterLog);
            SetField(debugController, "openRewardPanelButton", openRewardPanel);
            SetField(debugController, "setNextTurtleButton", setNextTurtle);
            SetField(debugController, "setNextSwarmButton", setNextSwarm);
            SetField(debugController, "setNextPoisonButton", setNextPoison);
            SetField(debugController, "setNextThiefButton", setNextThief);
            SetField(debugController, "setNextSealButton", setNextSeal);
            SetField(debugController, "setNextBossButton", setNextBoss);
            SetField(debugController, "giveEyeCoreRewardButton", giveEyeCore);
            SetField(debugController, "giveSpiritLinkRewardButton", giveSpiritLink);
            SetField(debugController, "giveOuterRingRewardButton", giveOuterRing);
            SetField(debugController, "printRunModifiersButton", printModifiers);
            SetField(debugController, "resetRunModifiersButton", resetModifiers);
            */
        }

        private static void CreateV02DebugPopup(Transform parent, V02FormationDebugController debugController, V02DebugPopupController popupController)
        {
            Button openButton = CreateButton("DebugOpenButton", parent, "debug", new Color(0.18f, 0.2f, 0.24f), 22);
            SetRect(openButton.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(126f, 58f));

            GameObject popup = CreatePanel("V02DebugPopup", parent, Vector2.zero, new Vector2(980f, 1320f), new Color(0.045f, 0.052f, 0.052f, 0.98f), TextAnchor.MiddleCenter);
            Outline outline = popup.AddComponent<Outline>();
            outline.effectColor = new Color(0.5f, 0.82f, 1f, 0.95f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText("DebugPopupTitle", popup.transform, "debug", 34, FontStyle.Bold, new Color(0.86f, 0.95f, 1f));
            title.alignment = TextAnchor.MiddleLeft;
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(-160f, 54f));

            Button closeButton = CreateButton("DebugCloseButton", popup.transform, "\u5173\u95ed", new Color(0.42f, 0.26f, 0.24f), 24);
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(128f, 60f));

            GameObject buttonGrid = new("DebugButtonGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            buttonGrid.transform.SetParent(popup.transform, false);
            SetRect(buttonGrid.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(900f, 1100f));
            GridLayoutGroup layout = buttonGrid.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(210f, 58f);
            layout.spacing = new Vector2(14f, 12f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            layout.childAlignment = TextAnchor.UpperCenter;

            Button MakeDebugButton(string name, string label, Color color, int fontSize = 20)
            {
                return CreateButton(name, buttonGrid.transform, label, color, fontSize);
            }

            Button powered = MakeDebugButton("AutoPlacePoweredButton", "\u6446\u653e\u4f9b\u80fd", new Color(0.24f, 0.46f, 0.5f));
            Button unpowered = MakeDebugButton("AutoPlaceUnpoweredButton", "\u6446\u653e\u65ad\u4f9b", new Color(0.46f, 0.28f, 0.24f));
            Button printTags = MakeDebugButton("PrintSelectedTagsButton", "\u6253\u5370\u6807\u7b7e", new Color(0.24f, 0.38f, 0.52f));
            Button giveAll = MakeDebugButton("GiveAllV02TalismansButton", "\u53d1\u653e\u5168\u90e8", new Color(0.36f, 0.36f, 0.22f));
            Button tagQuery = MakeDebugButton("TestTagQueryButton", "\u67e5\u8be2\u6807\u7b7e", new Color(0.32f, 0.28f, 0.5f));
            Button counterMatch = MakeDebugButton("TestCounterMatchButton", "\u6d4b\u8bd5\u514b\u5236", new Color(0.44f, 0.26f, 0.42f));
            Button spawnMountain = MakeDebugButton("SpawnMountainImpButton", "\u654c:\u5c71\u5996", new Color(0.22f, 0.36f, 0.32f));
            Button spawnTurtle = MakeDebugButton("SpawnTurtleGuardianButton", "\u654c:\u62a4\u6cd5", new Color(0.26f, 0.42f, 0.48f));
            Button spawnSwarm = MakeDebugButton("SpawnImpSwarmButton", "\u654c:\u5996\u7fa4", new Color(0.35f, 0.32f, 0.24f));
            Button spawnPoison = MakeDebugButton("SpawnRedPoisonBeastButton", "\u654c:\u6bd2\u5996", new Color(0.42f, 0.24f, 0.22f));
            Button spawnThief = MakeDebugButton("SpawnEnergyThiefGhostButton", "\u654c:\u5077\u7075", new Color(0.32f, 0.26f, 0.46f));
            Button spawnSeal = MakeDebugButton("SpawnSealTaoistButton", "\u654c:\u5c01\u5370", new Color(0.4f, 0.32f, 0.18f));
            Button spawnBoss = MakeDebugButton("SpawnFormationBreakerBossButton", "\u654c:Boss", new Color(0.48f, 0.22f, 0.28f));
            Button forceSkill = MakeDebugButton("ForceEnemySkillButton", "\u5f3a\u5236\u654c\u6280", new Color(0.58f, 0.28f, 0.16f));
            Button clearStatus = MakeDebugButton("ClearPlayerStatusButton", "\u6e05\u72b6\u6001", new Color(0.22f, 0.4f, 0.36f));
            Button clearSeals = MakeDebugButton("ClearV02SealsButton", "\u6e05\u5c01\u5370", new Color(0.32f, 0.34f, 0.42f));
            Button testShieldBreak = MakeDebugButton("TestShieldBreakButton", "\u6d4b\u7834\u76fe", new Color(0.35f, 0.46f, 0.62f));
            Button testCleansePoison = MakeDebugButton("TestCleansePoisonButton", "\u6d4b\u51c0\u5316\u6bd2", new Color(0.24f, 0.48f, 0.34f), 18);
            Button testCleanseSeal = MakeDebugButton("TestCleanseSealButton", "\u6d4b\u89e3\u5c01", new Color(0.26f, 0.42f, 0.54f));
            Button testSoulSuppress = MakeDebugButton("TestSoulSuppressButton", "\u6d4b\u9547\u9b42", new Color(0.44f, 0.28f, 0.56f));
            Button testChainClear = MakeDebugButton("TestChainClearButton", "\u6d4b\u6e05\u7fa4", new Color(0.48f, 0.42f, 0.22f));
            Button testCounterLog = MakeDebugButton("TestCounterLogPriorityButton", "\u6d4b\u65e5\u5fd7", new Color(0.38f, 0.34f, 0.46f));
            Button openRewardPanel = MakeDebugButton("OpenRewardPanelButton", "\u5f00\u5956\u52b1", new Color(0.42f, 0.5f, 0.28f));
            Button setNextTurtle = MakeDebugButton("SetNextTurtleButton", "\u4e0b\u573a\u62a4\u6cd5", new Color(0.24f, 0.42f, 0.52f), 18);
            Button setNextSwarm = MakeDebugButton("SetNextSwarmButton", "\u4e0b\u573a\u7fa4\u602a", new Color(0.38f, 0.34f, 0.2f), 18);
            Button setNextPoison = MakeDebugButton("SetNextPoisonButton", "\u4e0b\u573a\u6bd2\u602a", new Color(0.44f, 0.24f, 0.22f), 18);
            Button setNextThief = MakeDebugButton("SetNextThiefButton", "\u4e0b\u573a\u5077\u7075", new Color(0.34f, 0.26f, 0.48f), 18);
            Button setNextSeal = MakeDebugButton("SetNextSealButton", "\u4e0b\u573a\u5c01\u5370", new Color(0.42f, 0.32f, 0.18f), 18);
            Button setNextBoss = MakeDebugButton("SetNextBossButton", "\u4e0b\u573aBoss", new Color(0.5f, 0.22f, 0.3f), 18);
            Button giveEyeCore = MakeDebugButton("GiveEyeCoreRewardButton", "\u7ed9\u9635\u773c", new Color(0.28f, 0.42f, 0.58f));
            Button giveSpiritLink = MakeDebugButton("GiveSpiritLinkRewardButton", "\u7ed9\u8fde\u7ebf", new Color(0.24f, 0.46f, 0.5f));
            Button giveOuterRing = MakeDebugButton("GiveOuterRingRewardButton", "\u7ed9\u5916\u5708", new Color(0.34f, 0.44f, 0.28f));
            Button printModifiers = MakeDebugButton("PrintRunModifiersButton", "\u6253\u5370\u5f3a\u5316", new Color(0.34f, 0.34f, 0.42f), 18);
            Button resetModifiers = MakeDebugButton("ResetRunModifiersButton", "\u6e05\u5f3a\u5316", new Color(0.42f, 0.28f, 0.24f));
            Button startRun = MakeDebugButton("StartNewV02RunButton", "New Run", new Color(0.26f, 0.42f, 0.34f), 18);
            Button skipRound1 = MakeDebugButton("SkipRound1Button", "Round 1", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound2 = MakeDebugButton("SkipRound2Button", "Round 2", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound3 = MakeDebugButton("SkipRound3Button", "Round 3", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound4 = MakeDebugButton("SkipRound4Button", "Round 4", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound5 = MakeDebugButton("SkipRound5Button", "Round 5", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound6 = MakeDebugButton("SkipRound6Button", "Round 6", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound7 = MakeDebugButton("SkipRound7Button", "Round 7", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound8 = MakeDebugButton("SkipRound8Button", "Round 8", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound9 = MakeDebugButton("SkipRound9Button", "Round 9", new Color(0.24f, 0.34f, 0.44f), 18);
            Button skipRound10 = MakeDebugButton("SkipRound10Button", "Round 10 Boss", new Color(0.42f, 0.22f, 0.3f), 17);
            Button forceWin = MakeDebugButton("ForceWinCurrentRoundButton", "Force Win", new Color(0.32f, 0.48f, 0.26f), 18);
            Button forceLose = MakeDebugButton("ForceLoseCurrentRoundButton", "Force Lose", new Color(0.52f, 0.24f, 0.22f), 18);
            Button antiShield = MakeDebugButton("GiveAntiShieldBuildButton", "\u7ed9\u7834\u76fe", new Color(0.26f, 0.42f, 0.56f), 18);
            Button antiGroup = MakeDebugButton("GiveAntiGroupBuildButton", "\u7ed9\u6e05\u7fa4", new Color(0.36f, 0.34f, 0.22f), 18);
            Button antiPoison = MakeDebugButton("GiveAntiPoisonBuildButton", "\u7ed9\u51c0\u5316", new Color(0.26f, 0.46f, 0.34f), 18);
            Button antiSteal = MakeDebugButton("GiveAntiStealBuildButton", "\u7ed9\u53cd\u5077\u7075", new Color(0.34f, 0.28f, 0.52f), 17);
            Button antiSeal = MakeDebugButton("GiveAntiSealBuildButton", "\u7ed9\u9632\u5c01", new Color(0.42f, 0.34f, 0.2f), 18);
            Button bossReady = MakeDebugButton("GiveBossReadyBuildButton", "Boss Ready", new Color(0.52f, 0.32f, 0.2f), 18);
            Button printFailure = MakeDebugButton("PrintFailureTrackerButton", "Print Fail", new Color(0.34f, 0.28f, 0.34f), 18);
            Button printStats = MakeDebugButton("PrintRunStatsButton", "Print Stats", new Color(0.28f, 0.34f, 0.46f), 18);
            Button resetRun = MakeDebugButton("ResetV02RunButton", "Reset Run", new Color(0.42f, 0.28f, 0.24f), 18);
            Button balanceShield = MakeDebugButton("RunBalanceShieldButton", "Bal Shield", new Color(0.22f, 0.4f, 0.52f), 17);
            Button balanceGroup = MakeDebugButton("RunBalanceGroupButton", "Bal Group", new Color(0.36f, 0.34f, 0.2f), 17);
            Button balancePoison = MakeDebugButton("RunBalancePoisonButton", "Bal Poison", new Color(0.42f, 0.24f, 0.24f), 17);
            Button balanceSteal = MakeDebugButton("RunBalanceStealButton", "Bal Steal", new Color(0.34f, 0.28f, 0.5f), 17);
            Button balanceSeal = MakeDebugButton("RunBalanceSealButton", "Bal Seal", new Color(0.42f, 0.34f, 0.18f), 17);
            Button balanceBoss = MakeDebugButton("RunBalanceBossButton", "Bal Boss", new Color(0.52f, 0.24f, 0.3f), 17);

            SetField(debugController, "autoPlacePoweredButton", powered);
            SetField(debugController, "autoPlaceUnpoweredButton", unpowered);
            SetField(debugController, "printSelectedTagsButton", printTags);
            SetField(debugController, "giveAllTalismansButton", giveAll);
            SetField(debugController, "testTagQueryButton", tagQuery);
            SetField(debugController, "testCounterMatchButton", counterMatch);
            SetField(debugController, "spawnMountainImpButton", spawnMountain);
            SetField(debugController, "spawnTurtleGuardianButton", spawnTurtle);
            SetField(debugController, "spawnImpSwarmButton", spawnSwarm);
            SetField(debugController, "spawnRedPoisonBeastButton", spawnPoison);
            SetField(debugController, "spawnEnergyThiefGhostButton", spawnThief);
            SetField(debugController, "spawnSealTaoistButton", spawnSeal);
            SetField(debugController, "spawnFormationBreakerBossButton", spawnBoss);
            SetField(debugController, "forceEnemySkillButton", forceSkill);
            SetField(debugController, "clearPlayerStatusButton", clearStatus);
            SetField(debugController, "clearSealsButton", clearSeals);
            SetField(debugController, "testShieldBreakButton", testShieldBreak);
            SetField(debugController, "testCleansePoisonButton", testCleansePoison);
            SetField(debugController, "testCleanseSealButton", testCleanseSeal);
            SetField(debugController, "testSoulSuppressButton", testSoulSuppress);
            SetField(debugController, "testChainClearButton", testChainClear);
            SetField(debugController, "testCounterLogPriorityButton", testCounterLog);
            SetField(debugController, "openRewardPanelButton", openRewardPanel);
            SetField(debugController, "setNextTurtleButton", setNextTurtle);
            SetField(debugController, "setNextSwarmButton", setNextSwarm);
            SetField(debugController, "setNextPoisonButton", setNextPoison);
            SetField(debugController, "setNextThiefButton", setNextThief);
            SetField(debugController, "setNextSealButton", setNextSeal);
            SetField(debugController, "setNextBossButton", setNextBoss);
            SetField(debugController, "giveEyeCoreRewardButton", giveEyeCore);
            SetField(debugController, "giveSpiritLinkRewardButton", giveSpiritLink);
            SetField(debugController, "giveOuterRingRewardButton", giveOuterRing);
            SetField(debugController, "printRunModifiersButton", printModifiers);
            SetField(debugController, "resetRunModifiersButton", resetModifiers);
            SetField(debugController, "startNewV02RunButton", startRun);
            SetField(debugController, "skipRound1Button", skipRound1);
            SetField(debugController, "skipRound2Button", skipRound2);
            SetField(debugController, "skipRound3Button", skipRound3);
            SetField(debugController, "skipRound4Button", skipRound4);
            SetField(debugController, "skipRound5Button", skipRound5);
            SetField(debugController, "skipRound6Button", skipRound6);
            SetField(debugController, "skipRound7Button", skipRound7);
            SetField(debugController, "skipRound8Button", skipRound8);
            SetField(debugController, "skipRound9Button", skipRound9);
            SetField(debugController, "skipRound10Button", skipRound10);
            SetField(debugController, "forceWinCurrentRoundButton", forceWin);
            SetField(debugController, "forceLoseCurrentRoundButton", forceLose);
            SetField(debugController, "giveAntiShieldBuildButton", antiShield);
            SetField(debugController, "giveAntiGroupBuildButton", antiGroup);
            SetField(debugController, "giveAntiPoisonBuildButton", antiPoison);
            SetField(debugController, "giveAntiStealBuildButton", antiSteal);
            SetField(debugController, "giveAntiSealBuildButton", antiSeal);
            SetField(debugController, "giveBossReadyBuildButton", bossReady);
            SetField(debugController, "printFailureTrackerButton", printFailure);
            SetField(debugController, "printRunStatsButton", printStats);
            SetField(debugController, "resetV02RunButton", resetRun);
            SetField(debugController, "runBalanceShieldButton", balanceShield);
            SetField(debugController, "runBalanceGroupButton", balanceGroup);
            SetField(debugController, "runBalancePoisonButton", balancePoison);
            SetField(debugController, "runBalanceStealButton", balanceSteal);
            SetField(debugController, "runBalanceSealButton", balanceSeal);
            SetField(debugController, "runBalanceBossButton", balanceBoss);

            SetField(popupController, "panel", popup);
            SetField(popupController, "openButton", openButton);
            SetField(popupController, "closeButton", closeButton);
            popup.SetActive(false);
        }

        private static void CreateV02RunResultPanel(Transform parent, V02RunResultPanel resultPanel)
        {
            GameObject panel = CreatePanel("V02RunResultPanel", parent, Vector2.zero, new Vector2(930f, 1180f), new Color(0.052f, 0.058f, 0.052f, 0.98f), TextAnchor.MiddleCenter);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.88f, 0.72f, 0.38f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText("RunResultTitle", panel.transform, "\u901a\u5173\u6210\u529f", 48, FontStyle.Bold, new Color(1f, 0.88f, 0.56f));
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -74f), new Vector2(820f, 72f));

            Text summary = CreateText("RunResultSummary", panel.transform, string.Empty, 28, FontStyle.Bold, new Color(0.92f, 1f, 0.86f));
            summary.alignment = TextAnchor.UpperLeft;
            summary.horizontalOverflow = HorizontalWrapMode.Wrap;
            SetRect(summary.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(800f, 150f));

            Text reason = CreateText("RunResultReason", panel.transform, string.Empty, 25, FontStyle.Bold, Color.white);
            reason.alignment = TextAnchor.UpperLeft;
            reason.horizontalOverflow = HorizontalWrapMode.Wrap;
            reason.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(reason.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -390f), new Vector2(800f, 260f));

            Text details = CreateText("RunResultDetails", panel.transform, string.Empty, 23, FontStyle.Normal, new Color(0.82f, 0.92f, 1f));
            details.alignment = TextAnchor.UpperLeft;
            details.horizontalOverflow = HorizontalWrapMode.Wrap;
            details.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(details.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 170f), new Vector2(800f, 260f));

            Button restart = CreateButton("RestartV02RunButton", panel.transform, "\u91cd\u65b0\u5f00\u59cb", new Color(0.52f, 0.32f, 0.18f), 30);
            SetRect(restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 56f), new Vector2(520f, 94f));

            SetField(resultPanel, "panel", panel);
            SetField(resultPanel, "titleText", title);
            SetField(resultPanel, "summaryText", summary);
            SetField(resultPanel, "reasonText", reason);
            SetField(resultPanel, "detailText", details);
            SetField(resultPanel, "restartButton", restart);
            panel.SetActive(false);
        }

        private static void CreateV02RewardPanel(Transform parent, V02RewardPanel rewardPanel)
        {
            GameObject panel = CreatePanel("V02RewardPanel", parent, Vector2.zero, new Vector2(980f, 1420f), new Color(0.055f, 0.064f, 0.058f, 0.97f), TextAnchor.MiddleCenter);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.82f, 0.72f, 0.42f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText("RewardTitle", panel.transform, "选择奖励", 42, FontStyle.Bold, new Color(1f, 0.88f, 0.56f));
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -54f), new Vector2(860f, 58f));

            Text nextEnemy = CreateText("RewardNextEnemy", panel.transform, "下一关：【盾】盾", 26, FontStyle.Bold, new Color(0.84f, 0.96f, 1f));
            nextEnemy.alignment = TextAnchor.MiddleCenter;
            SetRect(nextEnemy.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(860f, 42f));

            GameObject[] cards = new GameObject[3];
            Text[] names = new Text[3];
            Text[] types = new Text[3];
            Text[] descriptions = new Text[3];
            Text[] tags = new Text[3];
            Text[] recommends = new Text[3];
            Button[] buttons = new Button[3];

            for (int i = 0; i < 3; i++)
            {
                GameObject card = CreatePanel($"RewardCard_{i + 1}", panel.transform, new Vector2(0f, -230f - i * 330f), new Vector2(880f, 286f), new Color(0.105f, 0.118f, 0.096f), TextAnchor.UpperCenter);
                cards[i] = card;

                Text type = CreateText("RewardType", card.transform, "新符箓", 22, FontStyle.Bold, new Color(0.68f, 0.9f, 1f));
                type.alignment = TextAnchor.MiddleLeft;
                SetRect(type.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(240f, 34f));
                types[i] = type;

                Text name = CreateText("RewardName", card.transform, "获得雷符", 34, FontStyle.Bold, Color.white);
                name.alignment = TextAnchor.MiddleLeft;
                SetRect(name.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-20f, -72f), new Vector2(-260f, 48f));
                names[i] = name;

                Text description = CreateText("RewardDescription", card.transform, "获得 1 张雷符。", 24, FontStyle.Normal, new Color(0.92f, 0.95f, 0.9f));
                description.alignment = TextAnchor.UpperLeft;
                description.horizontalOverflow = HorizontalWrapMode.Wrap;
                SetRect(description.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-20f, -126f), new Vector2(-260f, 68f));
                descriptions[i] = description;

                Text tagText = CreateText("RewardTags", card.transform, "适合克制：护盾", 21, FontStyle.Bold, new Color(1f, 0.86f, 0.55f));
                tagText.alignment = TextAnchor.MiddleLeft;
                tagText.horizontalOverflow = HorizontalWrapMode.Wrap;
                SetRect(tagText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-20f, 74f), new Vector2(-260f, 36f));
                tags[i] = tagText;

                Text recommend = CreateText("RewardRecommend", card.transform, "推荐：克制下一关", 21, FontStyle.Bold, new Color(0.72f, 1f, 0.68f));
                recommend.alignment = TextAnchor.MiddleLeft;
                SetRect(recommend.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-20f, 32f), new Vector2(-260f, 34f));
                recommends[i] = recommend;

                Button choose = CreateButton("ChooseRewardButton", card.transform, "选择", new Color(0.56f, 0.36f, 0.18f), 28);
                SetRect(choose.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-34f, -10f), new Vector2(180f, 96f));
                buttons[i] = choose;
            }

            SetField(rewardPanel, "panel", panel);
            SetField(rewardPanel, "titleText", title);
            SetField(rewardPanel, "nextEnemyText", nextEnemy);
            SetField(rewardPanel, "optionCards", cards);
            SetField(rewardPanel, "optionNameTexts", names);
            SetField(rewardPanel, "optionTypeTexts", types);
            SetField(rewardPanel, "optionDescriptionTexts", descriptions);
            SetField(rewardPanel, "optionTagTexts", tags);
            SetField(rewardPanel, "optionRecommendTexts", recommends);
            SetField(rewardPanel, "optionButtons", buttons);
            panel.SetActive(false);
        }

        private static RunConfig CreateRunConfig(Dictionary<string, TalismanItemDefinition> items, Dictionary<string, EnemyDefinition> enemies)
        {
            const string path = "Assets/_Game/ScriptableObjects/TalismanBag/RunConfigs/RunConfig_15Min_Test.asset";
            RunConfig config = AssetDatabase.LoadAssetAtPath<RunConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<RunConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.runId = "RunConfig_15Min_Test";
            config.displayName = "15分钟竖版验证局";
            config.startingSpiritJade = 0;
            config.startingItems = new List<TalismanItemDefinition>
            {
                items["spirit_stone_basic"],
                items["fire_talisman_basic"],
                items["shield_talisman_basic"],
                items["qi_pill_basic"]
            };
            config.rounds = new List<RoundConfig>
            {
                CreateRound(1, enemies["ghost_basic"], 6, false, "Round 1 初遇鬼怪", "教学基础循环：聚灵石产灵气，火符消耗灵气攻击。", "30-45秒", "把火符放在聚灵石旁边，火符会释放得更快。"),
                CreateRound(2, enemies["ghost_elite"], 8, false, "Round 2 强化鬼怪", "引导玩家理解鬼怪可被雷法、桃木和驱邪克制。", "45-60秒", "鬼怪害怕雷法、桃木和驱邪类法器。"),
                CreateRound(3, enemies["sword_cultivator_basic"], 10, false, "Round 3 剑修来袭", "理解剑修需要护盾、回血、雷符打断和火剑流。", "60-75秒", "剑修蓄力连斩时，可以用雷符打断。护身符和回气丹能提高生存。"),
                CreateRound(4, enemies["evil_cultivator_basic"], 12, true, "Round 4 邪修封阵", "测试玩家是否理解阵型不能过度依赖单个核心道具。", "60-75秒", "邪修会封印阵位并吸走灵气，不要把所有核心道具挤在一起。"),
                CreateRound(5, enemies["ghost_swarm"], 14, false, "Round 5 鬼群压境", "检验玩家是否形成稳定输出循环。", "75-90秒", "这一战会检验持续输出。驱邪阵和雷法对鬼群很有效。"),
                CreateRound(6, enemies["sword_cultivator_elite"], 16, false, "Round 6 剑修精英", "检验玩家的防御、回血和打断能力。", "75-90秒", "剑修精英连斩更频繁。雷符打断、护身符和回气丹很重要。"),
                CreateRound(7, enemies["heart_demon_boss"], 0, false, "Round 7 心魔邪修", "最终 Boss 综合检验灵气供应、防御回血、输出循环、雷符打断和阵型稳定。", "90-150秒", "心魔邪修会吸灵、封印和蓄力。稳定灵气、防御和雷符打断都很重要。")
            };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static RoundConfig CreateRound(int index, EnemyDefinition enemy, int reward, bool unlockBagExpansion, string title, string goal, string targetDuration, string hint)
        {
            return new RoundConfig
            {
                roundIndex = index,
                enemy = enemy,
                rewardSpiritJade = reward,
                unlockBagExpansion = unlockBagExpansion,
                roundTitle = title,
                battleGoalText = goal,
                targetDurationText = targetDuration,
                roundHint = hint,
                isBossRound = enemy != null && enemy.enemyType == EnemyType.Boss
            };
        }

        private static ShopPoolConfig CreateShopPoolConfig(Dictionary<string, TalismanItemDefinition> items)
        {
            const string path = "Assets/_Game/ScriptableObjects/TalismanBag/ShopPools/ShopPool_BasicTalismans.asset";
            ShopPoolConfig config = AssetDatabase.LoadAssetAtPath<ShopPoolConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ShopPoolConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.poolId = "ShopPool_BasicTalismans";
            config.entries = new List<ShopPoolEntry>
            {
                new() { item = items["fire_talisman_basic"], price = 6, weight = 12 },
                new() { item = items["spirit_stone_basic"], price = 7, weight = 10 },
                new() { item = items["shield_talisman_basic"], price = 6, weight = 10 },
                new() { item = items["qi_pill_basic"], price = 6, weight = 10 },
                new() { item = items["thunder_talisman_basic"], price = 8, weight = 8 },
                new() { item = items["seal_basic"], price = 5, weight = 8 },
                new() { item = items["sword_pill_basic"], price = 7, weight = 8 },
                new() { item = items["peach_wood_basic"], price = 5, weight = 8 },
                new() { item = items["exorcism_bell_basic"], price = 5, weight = 8 },
                new() { item = items["water_talisman_basic"], price = 6, weight = 8 }
            };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static ShopItemPriceConfig CreateShopPriceConfig(Dictionary<string, TalismanItemDefinition> items)
        {
            const string path = "Assets/_Game/ScriptableObjects/TalismanBag/ShopPools/shop_prices_basic.asset";
            ShopItemPriceConfig config = AssetDatabase.LoadAssetAtPath<ShopItemPriceConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ShopItemPriceConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.prices = new List<ShopItemPriceEntry>
            {
                new() { item = items["fire_talisman_basic"], price = 6 },
                new() { item = items["spirit_stone_basic"], price = 7 },
                new() { item = items["shield_talisman_basic"], price = 6 },
                new() { item = items["qi_pill_basic"], price = 6 },
                new() { item = items["thunder_talisman_basic"], price = 8 },
                new() { item = items["seal_basic"], price = 5 },
                new() { item = items["sword_pill_basic"], price = 7 },
                new() { item = items["peach_wood_basic"], price = 5 },
                new() { item = items["exorcism_bell_basic"], price = 5 },
                new() { item = items["water_talisman_basic"], price = 6 }
            };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static DraggableTalismanItemView CreatePrefabs(Dictionary<string, TalismanItemDefinition> definitions)
        {
            GameObject slot = CreateSlot(Vector2Int.zero, null);
            PrefabUtility.SaveAsPrefabAsset(slot, "Assets/_Game/Prefabs/TalismanBag/TalismanGridSlot.prefab");
            UnityEngine.Object.DestroyImmediate(slot);

            GameObject item = CreateItemView(definitions["fire_talisman_basic"], null, null, null);
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(item, "Assets/_Game/Prefabs/TalismanBag/DraggableTalismanItem.prefab");
            UnityEngine.Object.DestroyImmediate(item);
            return saved.GetComponent<DraggableTalismanItemView>();
        }

        private static void CreateScene(
            Dictionary<string, TalismanItemDefinition> definitions,
            Dictionary<string, EnemyDefinition> enemies,
            RunConfig runConfig,
            ShopPoolConfig shopPool,
            ShopItemPriceConfig priceConfig,
            DraggableTalismanItemView itemPrefab)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            CreateStretchPanel("Background", canvasObject.transform, new Color(0.07f, 0.065f, 0.052f));
            Transform uiRoot = CreateSafeAreaRoot(canvasObject.transform);

            GameObject gridModel = new("TalismanBagGrid", typeof(TalismanBagGrid));
            gridModel.transform.SetParent(uiRoot, false);
            TalismanBagGrid grid = gridModel.GetComponent<TalismanBagGrid>();

            GameObject systems = new("TalismanBagPhase4Systems",
                typeof(TalismanCombatUI),
                typeof(BattleLogUI),
                typeof(AutoCombatController),
                typeof(ComboResolver),
                typeof(EnemyPreviewUI),
                typeof(RunFlowControllerV2),
                typeof(ShopControllerV2),
                typeof(SpiritJadeWallet),
                typeof(TalismanBag.Inventory.PlayerTalismanInventory),
                typeof(BagExpansionController),
                typeof(BagPowerSlotController),
                typeof(ItemLevelSystem),
                typeof(ItemMergeSystem),
                typeof(TutorialHintController),
                typeof(CombatSpeedController),
                typeof(BattleEventRouter),
                typeof(BattleStatsTracker),
                typeof(ComboHighlightController),
                typeof(FloatingCombatText),
                typeof(TalismanTriggerFeedback),
                typeof(ManaFlowView),
                typeof(EnemyFeedbackController),
                typeof(BattleResultPanel),
                typeof(PlaytestSessionLogger),
                typeof(PlaytestDebugPanel));
            systems.transform.SetParent(uiRoot, false);
            TalismanCombatUI combatUI = systems.GetComponent<TalismanCombatUI>();
            BattleLogUI battleLogUI = systems.GetComponent<BattleLogUI>();
            AutoCombatController combat = systems.GetComponent<AutoCombatController>();
            ComboResolver comboResolver = systems.GetComponent<ComboResolver>();
            EnemyPreviewUI enemyPreviewUI = systems.GetComponent<EnemyPreviewUI>();
            RunFlowControllerV2 runFlow = systems.GetComponent<RunFlowControllerV2>();
            ShopControllerV2 shop = systems.GetComponent<ShopControllerV2>();
            SpiritJadeWallet wallet = systems.GetComponent<SpiritJadeWallet>();
            TalismanBag.Inventory.PlayerTalismanInventory playerInventory = systems.GetComponent<TalismanBag.Inventory.PlayerTalismanInventory>();
            BagExpansionController bagExpansion = systems.GetComponent<BagExpansionController>();
            BagPowerSlotController powerSlots = systems.GetComponent<BagPowerSlotController>();
            ItemLevelSystem itemLevelSystem = systems.GetComponent<ItemLevelSystem>();
            ItemMergeSystem itemMergeSystem = systems.GetComponent<ItemMergeSystem>();
            TutorialHintController tutorialHint = systems.GetComponent<TutorialHintController>();
            CombatSpeedController speedController = systems.GetComponent<CombatSpeedController>();
            BattleEventRouter eventRouter = systems.GetComponent<BattleEventRouter>();
            BattleStatsTracker statsTracker = systems.GetComponent<BattleStatsTracker>();
            ComboHighlightController comboHighlight = systems.GetComponent<ComboHighlightController>();
            FloatingCombatText floatingText = systems.GetComponent<FloatingCombatText>();
            TalismanTriggerFeedback triggerFeedback = systems.GetComponent<TalismanTriggerFeedback>();
            ManaFlowView manaFlow = systems.GetComponent<ManaFlowView>();
            EnemyFeedbackController enemyFeedback = systems.GetComponent<EnemyFeedbackController>();
            BattleResultPanel resultPanel = systems.GetComponent<BattleResultPanel>();
            PlaytestSessionLogger playtestLogger = systems.GetComponent<PlaytestSessionLogger>();
            PlaytestDebugPanel playtestDebugPanel = systems.GetComponent<PlaytestDebugPanel>();

            CreateTopBar(uiRoot, combatUI, out Text jadeText);
            CreateEnemyArea(uiRoot, combatUI, enemyPreviewUI, runFlow, out RectTransform enemyRect, out Graphic enemyGraphic, out Image chargeFill, out Text chargeText);
            CreateCombatStage(uiRoot, tutorialHint);
            TalismanGridSlotView[] slots = CreateGrid(uiRoot, grid);
            CreateInfoArea(uiRoot, comboHighlight, out ComboStatusUI comboStatusUI, battleLogUI);
            Transform inventoryParent = CreateBottomControls(uiRoot, definitions, grid, canvas, combat, out List<DraggableTalismanItemView> initialItems);
            CreateButtons(uiRoot, combat, speedController);
            CreateShop(uiRoot, shop, definitions, shopPool, priceConfig, grid, canvas, combat, runFlow, battleLogUI, wallet, statsTracker, playerInventory, inventoryParent, itemPrefab);
            RectTransform feedbackRoot = CreateFeedbackRoot(uiRoot);
            CreateResultPanel(uiRoot, resultPanel);
            CreatePlaytestDebugPanel(uiRoot, playtestDebugPanel, runFlow, combat, shop, wallet, bagExpansion, resultPanel, statsTracker, definitions);
            CreateVersionInfo(uiRoot, playtestDebugPanel);
            SetField(resultPanel, "runFlowController", runFlow);
            SetField(wallet, "jadeText", jadeText);
            playerInventory.ResetInventory(runConfig.startingItems);
            SetField(bagExpansion, "slotViews", slots);
            bagExpansion.Initialize(slots);
            powerSlots.Bind(bagExpansion);
            powerSlots.Initialize(slots);
            itemMergeSystem.BindInventory(playerInventory);

            SetField(comboResolver, "grid", grid);
            SetField(statsTracker, "eventRouter", eventRouter);
            SetField(statsTracker, "comboResolver", comboResolver);
            SetField(playtestLogger, "eventRouter", eventRouter);
            SetField(comboHighlight, "grid", grid);
            SetField(comboHighlight, "comboResolver", comboResolver);
            SetField(comboHighlight, "slotViews", slots);
            SetField(comboStatusUI, "highlightController", comboHighlight);
            SetField(floatingText, "eventRouter", eventRouter);
            SetField(floatingText, "floatingRoot", feedbackRoot);
            SetField(triggerFeedback, "eventRouter", eventRouter);
            SetField(manaFlow, "eventRouter", eventRouter);
            SetField(manaFlow, "flowRoot", feedbackRoot);
            SetField(enemyFeedback, "eventRouter", eventRouter);
            SetField(enemyFeedback, "enemyVisual", enemyRect);
            SetField(enemyFeedback, "enemyFlashTarget", enemyGraphic);
            SetField(enemyFeedback, "chargeFill", chargeFill);
            SetField(enemyFeedback, "chargeText", chargeText);
            SetField(battleLogUI, "eventRouter", eventRouter);
            SetField(battleLogUI, "maxLines", 3);

            SetField(combat, "grid", grid);
            SetField(combat, "comboResolver", comboResolver);
            SetField(combat, "eventRouter", eventRouter);
            SetField(combat, "statsTracker", statsTracker);
            SetField(combat, "resultPanel", resultPanel);
            SetField(combat, "comboHighlightController", comboHighlight);
            SetField(combat, "floatingCombatText", floatingText);
            SetField(combat, "talismanTriggerFeedback", triggerFeedback);
            SetField(combat, "combatUI", combatUI);
            SetField(combat, "battleLogUI", battleLogUI);
            SetField(combat, "comboStatusUI", comboStatusUI);
            SetField(combat, "feedbackRoot", feedbackRoot);
            SetField(combat, "runFlowController", runFlow);
            SetField(combat, "shopController", shop);
            SetField(combat, "inventory", playerInventory);
            SetField(combat, "bagExpansionController", bagExpansion);
            SetField(combat, "combatSpeedController", speedController);
            SetField(combat, "slotViews", slots);
            SetField(combat, "itemViews", initialItems);

            SetField(runFlow, "combatController", combat);
            SetField(runFlow, "shopController", shop);
            SetField(runFlow, "enemyPreviewUI", enemyPreviewUI);
            SetField(runFlow, "tutorialHintController", tutorialHint);
            SetField(runFlow, "wallet", wallet);
            SetField(runFlow, "bagExpansionController", bagExpansion);
            SetField(runFlow, "runConfig", runConfig);
            SetField(tutorialHint, "grid", grid);

            SetField(itemLevelSystem, "combatController", combat);

            foreach (DraggableTalismanItemView item in initialItems)
            {
                SetField(item, "combatController", combat);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.ImportAsset(ScenePath);
            GUID sceneGuid = new(AssetDatabase.AssetPathToGUID(ScenePath));
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(sceneGuid, true)
            };
        }

        private static void CreateTopBar(Transform parent, TalismanCombatUI combatUI, out Text jadeText)
        {
            GameObject topBar = CreatePanel("TopStatusBar", parent, new Vector2(0f, -12f), new Vector2(1020f, 140f), new Color(0.12f, 0.105f, 0.082f), TextAnchor.UpperCenter);
            GridLayoutGroup layout = topBar.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(320f, 54f);
            layout.spacing = new Vector2(12f, 12f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.childAlignment = TextAnchor.MiddleCenter;

            Text hp = CreateStatusText("HPText", topBar.transform, "100/100 气血", new Color(1f, 0.88f, 0.82f));
            Image hpFill = CreateHpBarForStatusText(hp);
            Text shield = CreateStatusText("ShieldText", topBar.transform, "护盾 0", new Color(0.82f, 0.94f, 1f));
            Text mana = CreateStatusText("ManaText", topBar.transform, "0/100 灵气", new Color(0.74f, 0.9f, 1f));
            Text jade = CreateStatusText("JadeText", topBar.transform, "灵玉 0", new Color(1f, 0.86f, 0.48f));
            Text state = CreateStatusText("StateText", topBar.transform, "战前整理", new Color(0.78f, 1f, 0.72f));
            Text round = CreateStatusText("RoundTopText", topBar.transform, "15分钟验证", new Color(0.92f, 0.86f, 0.72f));

            SetField(combatUI, "hpText", hp);
            SetField(combatUI, "hpFillImage", hpFill);
            SetField(combatUI, "shieldText", shield);
            SetField(combatUI, "manaText", mana);
            SetField(combatUI, "stateText", state);
            SetField(combatUI, "hpFlashTarget", hp);
            jadeText = jade;
        }

        private static void CreateEnemyArea(
            Transform parent,
            TalismanCombatUI combatUI,
            EnemyPreviewUI previewUI,
            RunFlowControllerV2 runFlow,
            out RectTransform enemyRect,
            out Graphic enemyGraphic,
            out Image chargeFill,
            out Text chargeText)
        {
            GameObject panel = CreatePanel("EnemyArea", parent, new Vector2(0f, -166f), new Vector2(1020f, 250f), new Color(0.105f, 0.087f, 0.076f), TextAnchor.UpperCenter);
            GameObject enemy = CreatePanel("EnemySilhouette", panel.transform, new Vector2(-330f, 0f), new Vector2(210f, 210f), new Color(0.23f, 0.14f, 0.2f), TextAnchor.MiddleCenter);
            Text enemyFace = CreateText("EnemyFace", enemy.transform, "敌", 72, FontStyle.Bold, new Color(1f, 0.72f, 0.82f));
            SetRect(enemyFace.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject shield = CreatePanel("ShieldFeedback", panel.transform, new Vector2(-330f, 0f), new Vector2(230f, 230f), new Color(0.25f, 0.85f, 1f, 0f), TextAnchor.MiddleCenter);
            Image shieldImage = shield.GetComponent<Image>();

            Text round = CreateText("RoundText", panel.transform, "回合 1/4", 30, FontStyle.Bold, new Color(1f, 0.82f, 0.45f));
            SetRect(round.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -34f), new Vector2(260f, 42f));

            Text enemyName = CreateText("EnemyText", panel.transform, "敌人：普通鬼怪", 34, FontStyle.Bold, Color.white);
            enemyName.alignment = TextAnchor.MiddleLeft;
            SetRect(enemyName.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -82f), new Vector2(420f, 46f));

            Text hp = CreateText("EnemyHPText", panel.transform, "80/80 气血", 28, FontStyle.Bold, new Color(1f, 0.68f, 0.68f));
            hp.alignment = TextAnchor.MiddleLeft;
            SetRect(hp.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -128f), new Vector2(420f, 38f));

            Text weak = CreateText("WeaknessText", panel.transform, "弱雷  怕驱邪", 26, FontStyle.Bold, new Color(0.7f, 0.9f, 1f));
            weak.alignment = TextAnchor.MiddleLeft;
            SetRect(weak.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -178f), new Vector2(460f, 34f));

            Text danger = CreateText("DangerText", panel.transform, "攻频高", 25, FontStyle.Bold, new Color(1f, 0.78f, 0.5f));
            danger.alignment = TextAnchor.MiddleLeft;
            danger.horizontalOverflow = HorizontalWrapMode.Wrap;
            danger.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(danger.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, -216f), new Vector2(520f, 58f));

            GameObject chargeBar = CreatePanel("SwordChargeBar", panel.transform, new Vector2(230f, -100f), new Vector2(350f, 24f), new Color(0.18f, 0.12f, 0.1f), TextAnchor.MiddleCenter);
            GameObject chargeFillObject = CreatePanel("Fill", chargeBar.transform, Vector2.zero, new Vector2(350f, 24f), new Color(1f, 0.48f, 0.2f), TextAnchor.MiddleLeft);
            chargeFill = chargeFillObject.GetComponent<Image>();
            chargeFill.type = Image.Type.Filled;
            chargeFill.fillMethod = Image.FillMethod.Horizontal;
            chargeFill.fillOrigin = 0;
            chargeFill.fillAmount = 0f;
            chargeText = CreateText("ChargeText", panel.transform, "", 24, FontStyle.Bold, new Color(1f, 0.76f, 0.35f));
            SetRect(chargeText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(230f, 22f), new Vector2(360f, 32f));
            chargeBar.SetActive(false);
            chargeText.gameObject.SetActive(false);

            Text status = CreateText("RunStatusText", panel.transform, "战前整理：观察弱点后调整阵型", 23, FontStyle.Bold, new Color(0.78f, 1f, 0.72f));
            status.alignment = TextAnchor.MiddleRight;
            status.horizontalOverflow = HorizontalWrapMode.Wrap;
            SetRect(status.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-22f, -40f), new Vector2(265f, 72f));

            SetField(combatUI, "enemyHpText", hp);
            SetField(combatUI, "enemyVisual", enemy.GetComponent<RectTransform>());
            SetField(combatUI, "enemyFlashTarget", enemy.GetComponent<Image>());
            SetField(combatUI, "shieldFeedback", shieldImage);
            SetField(previewUI, "roundText", round);
            SetField(previewUI, "enemyText", enemyName);
            SetField(previewUI, "weaknessText", weak);
            SetField(previewUI, "dangerText", danger);
            SetField(runFlow, "runStatusText", status);
            enemyRect = enemy.GetComponent<RectTransform>();
            enemyGraphic = enemy.GetComponent<Image>();
        }

        private static void CreateCombatStage(Transform parent, TutorialHintController tutorialHint)
        {
            GameObject panel = CreatePanel("AutoCombatStage", parent, new Vector2(0f, -434f), new Vector2(1020f, 160f), new Color(0.095f, 0.082f, 0.07f), TextAnchor.UpperCenter);
            Text label = CreateText("StageLabel", panel.transform, "符箓飞出 / 雷击 / 护盾 / 克制飘字", 26, FontStyle.Bold, new Color(1f, 0.78f, 0.42f));
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SetField(tutorialHint, "hintText", label);
        }

        private static TalismanGridSlotView[] CreateGrid(Transform parent, TalismanBagGrid grid)
        {
            GameObject frame = CreatePanel("TalismanBagFrame", parent, new Vector2(0f, -608f), new Vector2(660f, 660f), new Color(0.12f, 0.09f, 0.065f), TextAnchor.UpperCenter);
            Outline outline = frame.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.58f, 0.22f);
            outline.effectDistance = new Vector2(4f, -4f);

            GameObject gridObject = new("GridSlots", typeof(RectTransform), typeof(GridLayoutGroup));
            gridObject.transform.SetParent(frame.transform, false);
            SetRect(gridObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(600f, 600f));
            GridLayoutGroup layout = gridObject.GetComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(112f, 112f);
            layout.spacing = new Vector2(10f, 10f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 5;
            layout.childAlignment = TextAnchor.MiddleCenter;

            List<TalismanGridSlotView> slots = new();
            for (int y = 4; y >= 0; y--)
            {
                for (int x = 0; x < 5; x++)
                {
                    GameObject slotObject = CreateSlot(new Vector2Int(x, y), grid);
                    slotObject.transform.SetParent(gridObject.transform, false);
                    slots.Add(slotObject.GetComponent<TalismanGridSlotView>());
                }
            }

            Text caption = CreateText("GridCaption", frame.transform, "5x5 符箓袋阵盘", 28, FontStyle.Bold, new Color(1f, 0.85f, 0.5f));
            SetRect(caption.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(480f, 38f));
            return slots.ToArray();
        }

        private static void CreateInfoArea(
            Transform parent,
            ComboHighlightController highlightController,
            out ComboStatusUI comboStatusUI,
            BattleLogUI battleLogUI)
        {
            GameObject panel = CreatePanel("InfoLogArea", parent, new Vector2(0f, -1286f), new Vector2(1020f, 224f), new Color(0.1f, 0.085f, 0.07f), TextAnchor.UpperCenter);

            GameObject comboPanel = CreatePanel("ComboStatusPanel", panel.transform, new Vector2(-255f, 0f), new Vector2(480f, 210f), new Color(0.12f, 0.095f, 0.072f), TextAnchor.MiddleCenter);
            comboStatusUI = comboPanel.AddComponent<ComboStatusUI>();
            Text comboText = CreateText("ComboStatusText", comboPanel.transform, "未形成有效阵法", 25, FontStyle.Bold, new Color(1f, 0.86f, 0.48f));
            comboText.alignment = TextAnchor.UpperLeft;
            comboText.horizontalOverflow = HorizontalWrapMode.Wrap;
            comboText.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(comboText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(430f, 168f));
            SetField(comboStatusUI, "comboText", comboText);
            SetField(comboStatusUI, "highlightController", highlightController);

            GameObject logPanel = CreatePanel("BattleLogPanel", panel.transform, new Vector2(255f, 0f), new Vector2(480f, 210f), new Color(0.105f, 0.09f, 0.078f), TextAnchor.MiddleCenter);
            Text title = CreateText("BattleLogTitle", logPanel.transform, "关键日志", 24, FontStyle.Bold, new Color(1f, 0.82f, 0.45f));
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(420f, 34f));

            Text body = CreateText("BattleLogText", logPanel.transform, string.Empty, 21, FontStyle.Normal, Color.white);
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(420f, 130f));
            SetField(battleLogUI, "logText", body);
        }

        private static Transform CreateBottomControls(
            Transform parent,
            Dictionary<string, TalismanItemDefinition> definitions,
            TalismanBagGrid grid,
            Canvas canvas,
            AutoCombatController controller,
            out List<DraggableTalismanItemView> initialItems)
        {
            GameObject bottom = CreatePanel("BottomOperationArea", parent, new Vector2(0f, 20f), new Vector2(1040f, 390f), new Color(0.12f, 0.102f, 0.08f), TextAnchor.LowerCenter);

            Text caption = CreateText("InventoryCaption", bottom.transform, "道具栏：拖到阵盘，战斗后从商店补强", 24, FontStyle.Bold, new Color(0.88f, 0.82f, 0.68f));
            caption.alignment = TextAnchor.MiddleLeft;
            SetRect(caption.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(-56f, 34f));

            GameObject scrollObject = new("InventoryScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollObject.transform.SetParent(bottom.transform, false);
            SetRect(scrollObject.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(980f, 132f));
            scrollObject.GetComponent<Image>().color = new Color(0.095f, 0.08f, 0.065f);

            GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObject.transform, false);
            SetRect(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            SetRect(contentRect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(0f, 112f));
            HorizontalLayoutGroup itemLayout = content.GetComponent<HorizontalLayoutGroup>();
            itemLayout.childForceExpandWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.childAlignment = TextAnchor.MiddleLeft;
            itemLayout.spacing = 16f;
            itemLayout.padding = new RectOffset(10, 10, 0, 0);
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            initialItems = new List<DraggableTalismanItemView>();
            string[] ids = { "spirit_stone_basic", "fire_talisman_basic", "shield_talisman_basic", "qi_pill_basic" };
            foreach (string id in ids)
            {
                GameObject item = CreateItemView(definitions[id], grid, canvas, controller);
                item.transform.SetParent(content.transform, false);
                DraggableTalismanItemView view = item.GetComponent<DraggableTalismanItemView>();
                initialItems.Add(view);
            }

            return content.transform;
        }

        private static void CreateButtons(Transform parent, AutoCombatController controller, CombatSpeedController speedController)
        {
            GameObject panel = CreatePanel("ThumbButtons", parent, new Vector2(0f, 72f), new Vector2(1000f, 118f), new Color(0f, 0f, 0f, 0f), TextAnchor.LowerCenter);
            HorizontalLayoutGroup layout = panel.AddComponent<HorizontalLayoutGroup>();
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.spacing = 14f;
            layout.padding = new RectOffset(0, 0, 0, 0);

            Button start = CreateButton("StartButton", panel.transform, "开始斗法", new Color(0.72f, 0.32f, 0.18f), 30);
            Button speed = CreateButton("SpeedToggleButton", panel.transform, "1x", new Color(0.26f, 0.36f, 0.54f), 28);
            Button auto = CreateButton("AutoPlaceButton", panel.transform, "自动合成", new Color(0.36f, 0.48f, 0.25f), 26);
            Button reset = CreateButton("ResetButton", panel.transform, "重置摆放", new Color(0.32f, 0.42f, 0.5f), 26);
            Button randomItem = CreateButton("RandomShopItemButton", panel.transform, "补道具", new Color(0.46f, 0.38f, 0.18f), 24);

            GameObject debugPanel = new("HiddenDebugButtons", typeof(RectTransform));
            debugPanel.transform.SetParent(parent, false);
            debugPanel.SetActive(false);
            Button damage = CreateButton("DamageButton", debugPanel.transform, "Damage Player 20", new Color(0.48f, 0.24f, 0.28f));
            Button mana = CreateButton("ManaButton", debugPanel.transform, "Add Mana 30", new Color(0.24f, 0.38f, 0.6f));
            Button skipSword = CreateButton("SkipSwordButton", debugPanel.transform, "Skip To Sword", new Color(0.42f, 0.32f, 0.58f));
            Button skipEvil = CreateButton("SkipEvilButton", debugPanel.transform, "Skip To Evil", new Color(0.42f, 0.22f, 0.42f));
            Button clearSeals = CreateButton("ClearSealsButton", debugPanel.transform, "Clear All Seals", new Color(0.26f, 0.44f, 0.44f));
            Button triggerFeedback = CreateButton("TriggerAllFeedbackButton", debugPanel.transform, "Trigger Feedback", new Color(0.32f, 0.46f, 0.62f));
            Button toggleCombo = CreateButton("ToggleComboHighlightButton", debugPanel.transform, "Toggle Combos", new Color(0.55f, 0.42f, 0.18f));
            Button floating = CreateButton("TestFloatingTextButton", debugPanel.transform, "Test Floating", new Color(0.38f, 0.5f, 0.3f));
            Button forceCharge = CreateButton("ForceSwordChargeButton", debugPanel.transform, "Force Charge", new Color(0.52f, 0.28f, 0.2f));
            Button forceSeal = CreateButton("ForceSealButton", debugPanel.transform, "Force Seal", new Color(0.28f, 0.28f, 0.36f));

            SetField(controller, "startButton", start);
            SetField(controller, "resetButton", reset);
            SetField(controller, "autoPlaceButton", auto);
            SetField(controller, "damagePlayerButton", damage);
            SetField(controller, "addManaButton", mana);
            SetField(controller, "skipSwordButton", skipSword);
            SetField(controller, "skipEvilButton", skipEvil);
            SetField(controller, "addRandomShopItemButton", randomItem);
            SetField(controller, "clearSealsButton", clearSeals);
            SetField(controller, "triggerAllFeedbackButton", triggerFeedback);
            SetField(controller, "toggleComboHighlightButton", toggleCombo);
            SetField(controller, "testFloatingTextButton", floating);
            SetField(controller, "forceSwordChargeButton", forceCharge);
            SetField(controller, "forceSealButton", forceSeal);
            SetField(speedController, "toggleButton", speed);
            SetField(speedController, "label", speed.GetComponentInChildren<Text>());
        }

        private static void CreateShop(
            Transform parent,
            ShopControllerV2 shop,
            Dictionary<string, TalismanItemDefinition> definitions,
            ShopPoolConfig shopPool,
            ShopItemPriceConfig priceConfig,
            TalismanBagGrid grid,
            Canvas canvas,
            AutoCombatController combat,
            RunFlowControllerV2 runFlow,
            BattleLogUI battleLog,
            SpiritJadeWallet wallet,
            BattleStatsTracker statsTracker,
            TalismanBag.Inventory.PlayerTalismanInventory playerInventory,
            Transform inventoryParent,
            DraggableTalismanItemView itemPrefab)
        {
            GameObject panel = CreatePanel("ShopPanel", parent, new Vector2(0f, 430f), new Vector2(1000f, 980f), new Color(0.08f, 0.075f, 0.06f, 0.98f), TextAnchor.LowerCenter);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.75f, 0.25f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText("ShopTitle", panel.transform, "商店：选择一个新道具", 34, FontStyle.Bold, new Color(1f, 0.86f, 0.5f));
            title.alignment = TextAnchor.MiddleLeft;
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(-72f, 50f));

            ShopOptionView[] options = new ShopOptionView[4];
            for (int i = 0; i < options.Length; i++)
            {
                GameObject option = CreateShopOption(panel.transform, i);
                options[i] = option.GetComponent<ShopOptionView>();
            }

            Text status = CreateText("ShopStatus", panel.transform, "灵玉：0\n下一场敌人见上方情报", 23, FontStyle.Bold, Color.white);
            status.alignment = TextAnchor.MiddleLeft;
            status.horizontalOverflow = HorizontalWrapMode.Wrap;
            status.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(status.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(-72f, 112f));

            Button refresh = CreateButton("RefreshShopButton", panel.transform, "刷新 2 灵玉", new Color(0.42f, 0.36f, 0.18f), 26);
            SetRect(refresh.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-330f, 48f), new Vector2(260f, 88f));

            Button autoMerge = CreateButton("ShopAutoMergeButton", panel.transform, "自动合成", new Color(0.36f, 0.48f, 0.25f), 25);
            SetRect(autoMerge.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(260f, 88f));

            Button next = CreateButton("NextRoundButton", panel.transform, "下一场", new Color(0.35f, 0.55f, 0.28f), 30);
            SetRect(next.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(330f, 48f), new Vector2(260f, 88f));

            SetField(shop, "shopPanel", panel);
            SetField(shop, "optionViews", options);
            SetField(shop, "nextRoundButton", next);
            SetField(shop, "refreshButton", refresh);
            SetField(shop, "autoMergeButton", autoMerge);
            SetField(shop, "statusText", status);
            SetField(shop, "priceConfig", priceConfig);
            SetField(shop, "shopPool", shopPool);
            SetField(shop, "inventory", playerInventory);
            SetField(shop, "refreshCost", 2);
            SetField(shop, "optionCount", 4);
            SetField(shop, "itemPool", new[]
            {
                definitions["fire_talisman_basic"],
                definitions["spirit_stone_basic"],
                definitions["shield_talisman_basic"],
                definitions["qi_pill_basic"],
                definitions["thunder_talisman_basic"],
                definitions["seal_basic"],
                definitions["sword_pill_basic"],
                definitions["peach_wood_basic"],
                definitions["exorcism_bell_basic"],
                definitions["water_talisman_basic"]
            });
            SetField(shop, "inventoryParent", inventoryParent);
            SetField(shop, "itemViewPrefab", itemPrefab);
            SetField(shop, "grid", grid);
            SetField(shop, "rootCanvas", canvas);
            SetField(shop, "combatController", combat);
            SetField(shop, "statsTracker", statsTracker);
            SetField(shop, "runFlowController", runFlow);
            SetField(shop, "battleLogUI", battleLog);
            SetField(shop, "wallet", wallet);
            panel.SetActive(false);
        }

        private static GameObject CreateShopOption(Transform parent, int index)
        {
            GameObject option = new($"ShopOption_{index + 1}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(ShopOptionView));
            option.transform.SetParent(parent, false);
            SetRect(option.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f - index * 158f), new Vector2(900f, 150f));
            Image background = option.GetComponent<Image>();
            background.color = new Color(0.22f, 0.18f, 0.13f);
            Button button = option.GetComponent<Button>();
            button.targetGraphic = background;

            Text name = CreateText("Name", option.transform, "道具", 28, FontStyle.Bold, Color.black);
            name.alignment = TextAnchor.MiddleLeft;
            SetRect(name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(120f, -34f), new Vector2(400f, 40f));
            Text desc = CreateText("Description", option.transform, "说明", 21, FontStyle.Normal, Color.black);
            desc.alignment = TextAnchor.UpperLeft;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(desc.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(120f, -26f), new Vector2(500f, -64f));

            Text price = CreateText("Price", option.transform, "价格 0", 22, FontStyle.Bold, new Color(0.15f, 0.1f, 0.02f));
            SetRect(price.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-138f, -34f), new Vector2(150f, 34f));

            Text buy = CreateText("BuyText", option.transform, "购买", 26, FontStyle.Bold, Color.white);
            GameObject buyBg = CreatePanel("BuyButtonVisual", option.transform, new Vector2(-112f, -88f), new Vector2(170f, 58f), new Color(0.35f, 0.55f, 0.28f), TextAnchor.UpperRight);
            buyBg.GetComponent<Image>().raycastTarget = false;
            buy.transform.SetParent(buyBg.transform, false);
            SetRect(buy.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject swatch = CreatePanel("IconSwatch", option.transform, new Vector2(58f, -73f), new Vector2(84f, 84f), Color.white, TextAnchor.UpperLeft);
            swatch.GetComponent<Image>().raycastTarget = false;

            ShopOptionView view = option.GetComponent<ShopOptionView>();
            SetField(view, "selectButton", button);
            SetField(view, "background", background);
            SetField(view, "nameText", name);
            SetField(view, "descriptionText", desc);
            SetField(view, "priceText", price);
            SetField(view, "buyText", buy);
            return option;
        }

        private static RectTransform CreateFeedbackRoot(Transform parent)
        {
            GameObject root = new("FeedbackRoot", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            CreateFloatingTextAnchors(rect);
            return rect;
        }

        private static StatusEffectController CreateStatusEffectController(Transform parent, string name)
        {
            GameObject controllerObject = new(name, typeof(StatusEffectController));
            controllerObject.transform.SetParent(parent, false);
            return controllerObject.GetComponent<StatusEffectController>();
        }

        private static StatusAnchorUI CreateStatusAnchor(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, StatusPolarity polarity, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            GameObject anchorObject = new(name, typeof(RectTransform), typeof(StatusAnchorUI), typeof(HorizontalLayoutGroup));
            anchorObject.transform.SetParent(parent, false);
            RectTransform rect = anchorObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            HorizontalLayoutGroup layout = anchorObject.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = alignment;
            layout.spacing = 6f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            StatusAnchorUI anchor = anchorObject.GetComponent<StatusAnchorUI>();
            SetField(anchor, "iconAlignment", alignment);
            SetField(anchor, "filterByPolarity", true);
            SetField(anchor, "polarityFilter", polarity);
            return anchor;
        }

        private static void ConfigureStatusAnchor(StatusAnchorUI anchor, StatusEffectController controller, StatusTooltipPanel tooltipPanel, StatusPolarity polarity)
        {
            SetField(anchor, "controller", controller);
            SetField(anchor, "tooltipPanel", tooltipPanel);
            SetField(anchor, "filterByPolarity", true);
            SetField(anchor, "polarityFilter", polarity);
        }

        private static StatusTooltipPanel CreateStatusTooltipPanel(Transform parent)
        {
            GameObject panelObject = new("StatusTooltipRuntime", typeof(RectTransform), typeof(StatusTooltipPanel));
            panelObject.transform.SetParent(parent, false);
            SetRect(panelObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return panelObject.GetComponent<StatusTooltipPanel>();
        }

        private static void CreateFloatingTextAnchors(RectTransform root)
        {
            GameObject anchorRoot = new("FloatingTextAnchors", typeof(RectTransform), typeof(FloatingCombatTextAnchorLayout));
            anchorRoot.transform.SetParent(root, false);
            RectTransform anchorRootRect = anchorRoot.GetComponent<RectTransform>();
            SetRect(anchorRootRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            FloatingCombatTextAnchorLayout layout = anchorRoot.GetComponent<FloatingCombatTextAnchorLayout>();
            SetField(layout, "damageDealtAnchor", CreateFeedbackAnchor("DamageDealtAnchor", anchorRoot.transform, new Vector2(320f, 480f)));
            SetField(layout, "damageTakenAnchor", CreateFeedbackAnchor("DamageTakenAnchor", anchorRoot.transform, new Vector2(-320f, 480f)));
            SetField(layout, "manaGeneratedAnchor", CreateFeedbackAnchor("ManaGeneratedAnchor", anchorRoot.transform, new Vector2(0f, 320f)));
            SetField(layout, "manaSpentAnchor", CreateFeedbackAnchor("ManaSpentAnchor", anchorRoot.transform, new Vector2(0f, 260f)));
            SetField(layout, "shieldGainedAnchor", CreateFeedbackAnchor("ShieldGainedAnchor", anchorRoot.transform, new Vector2(-320f, 390f)));
            SetField(layout, "healReceivedAnchor", CreateFeedbackAnchor("HealReceivedAnchor", anchorRoot.transform, new Vector2(-320f, 540f)));
            SetField(layout, "shieldBreakAnchor", CreateFeedbackAnchor("ShieldBreakAnchor", anchorRoot.transform, new Vector2(320f, 390f)));
            SetField(layout, "cleanseAnchor", CreateFeedbackAnchor("CleanseAnchor", anchorRoot.transform, new Vector2(-180f, 360f)));
            SetField(layout, "unsealAnchor", CreateFeedbackAnchor("UnsealAnchor", anchorRoot.transform, new Vector2(-80f, 420f)));
            SetField(layout, "soulSuppressAnchor", CreateFeedbackAnchor("SoulSuppressAnchor", anchorRoot.transform, new Vector2(320f, 540f)));
            SetField(layout, "chainClearAnchor", CreateFeedbackAnchor("ChainClearAnchor", anchorRoot.transform, new Vector2(320f, 330f)));
            SetField(layout, "formationProtectedAnchor", CreateFeedbackAnchor("FormationProtectedAnchor", anchorRoot.transform, new Vector2(0f, 180f)));
            SetField(layout, "guardReduceAnchor", CreateFeedbackAnchor("GuardReduceAnchor", anchorRoot.transform, new Vector2(-320f, 330f)));
            SetField(layout, "counterFailedAnchor", CreateFeedbackAnchor("CounterFailedAnchor", anchorRoot.transform, new Vector2(0f, 520f)));
            SetField(layout, "statusDamageAnchor", CreateFeedbackAnchor("StatusDamageAnchor", anchorRoot.transform, new Vector2(-210f, 430f)));
            SetField(layout, "enemyInterruptedAnchor", CreateFeedbackAnchor("EnemyInterruptedAnchor", anchorRoot.transform, new Vector2(320f, 610f)));
            SetField(layout, "enemyEnragedAnchor", CreateFeedbackAnchor("EnemyEnragedAnchor", anchorRoot.transform, new Vector2(320f, 560f)));
            SetField(layout, "itemSealedAnchor", CreateFeedbackAnchor("ItemSealedAnchor", anchorRoot.transform, new Vector2(0f, 110f)));
            SetField(layout, "itemUnsealedAnchor", CreateFeedbackAnchor("ItemUnsealedAnchor", anchorRoot.transform, new Vector2(0f, 150f)));
        }

        private static RectTransform CreateFeedbackAnchor(string name, Transform parent, Vector2 position)
        {
            GameObject anchor = new(name, typeof(RectTransform));
            anchor.transform.SetParent(parent, false);
            RectTransform rect = anchor.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(160f, 40f);
            return rect;
        }

        private static void CreatePlaytestDebugPanel(
            Transform parent,
            PlaytestDebugPanel debugPanel,
            RunFlowControllerV2 runFlow,
            AutoCombatController combat,
            ShopControllerV2 shop,
            SpiritJadeWallet wallet,
            BagExpansionController bagExpansion,
            BattleResultPanel resultPanel,
            BattleStatsTracker statsTracker,
            Dictionary<string, TalismanItemDefinition> definitions)
        {
            GameObject panel = CreatePanel("PlaytestDebugPanel", parent, Vector2.zero, new Vector2(940f, 2000f), new Color(0.055f, 0.05f, 0.048f, 0.98f), TextAnchor.MiddleCenter);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(0.8f, 0.58f, 0.25f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText("DebugTitle", panel.transform, "Playtest Debug", 40, FontStyle.Bold, new Color(1f, 0.86f, 0.5f));
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -64f), new Vector2(820f, 58f));

            Button[] skipButtons = new Button[7];
            for (int i = 0; i < skipButtons.Length; i++)
            {
                int col = i % 4;
                int row = i / 4;
                Button button = CreateButton($"DebugSkipRound{i + 1}", panel.transform, $"Skip R{i + 1}", new Color(0.26f, 0.34f, 0.5f), 24);
                SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-315f + col * 210f, -150f - row * 104f), new Vector2(180f, 82f));
                skipButtons[i] = button;
            }

            Button addJade = CreateDebugButton(panel.transform, "DebugAddJade", "Add 20 Spirit Jade", -250f, 335f);
            Button giveAll = CreateDebugButton(panel.transform, "DebugGiveAll", "Give All Basic Items", 250f, 335f);
            Button giveCore = CreateDebugButton(panel.transform, "DebugGiveCore", "Give Lv2 Core Build", -250f, 225f);
            Button forceBoss = CreateDebugButton(panel.transform, "DebugBossP2", "Force Boss Phase 2", 250f, 225f);
            Button win = CreateDebugButton(panel.transform, "DebugWin", "Finish Run Win", -250f, 115f);
            Button lose = CreateDebugButton(panel.transform, "DebugLose", "Finish Run Lose", 250f, 115f);
            Button reset = CreateDebugButton(panel.transform, "DebugResetRun", "Reset Run", 0f, 5f);
            Button recommended = CreateDebugButton(panel.transform, "DebugRecommendedBuild", "Recommended Build", 0f, -105f);
            Button openShop = CreateDebugButton(panel.transform, "DebugOpenShop", "Open Shop", -250f, -215f);
            Button rerollShop = CreateDebugButton(panel.transform, "DebugRerollShop", "Reroll Shop", 250f, -215f);
            Button randomShopItem = CreateDebugButton(panel.transform, "DebugRandomShopItem", "Give Random Shop Item", 0f, -325f);
            Button duplicateFire = CreateDebugButton(panel.transform, "DebugDuplicateFire", "Duplicate Fire x2", -250f, -435f);
            Button duplicateThunder = CreateDebugButton(panel.transform, "DebugDuplicateThunder", "Duplicate Thunder x2", 250f, -435f);
            Button autoMerge = CreateDebugButton(panel.transform, "DebugAutoMergeAll", "Auto Merge All", 0f, -545f);
            Button unlockPower = CreateDebugButton(panel.transform, "DebugUnlockPowerSlots", "Unlock Power Slots", -250f, -655f);
            Button lockPower = CreateDebugButton(panel.transform, "DebugLockPowerSlots", "Lock Power Slots", 250f, -655f);
            Button startBoss = CreateDebugButton(panel.transform, "DebugStartBossFight", "Start Boss Fight", -250f, -745f);
            Button antiBossBuild = CreateDebugButton(panel.transform, "DebugAntiBossBuild", "Give Anti Boss Build", 250f, -745f);
            Button bossCharge = CreateDebugButton(panel.transform, "DebugBossCharge", "Force Boss Charge", -250f, -845f);
            Button bossEnrage = CreateDebugButton(panel.transform, "DebugBossEnrage", "Force Boss Enrage", 250f, -845f);
            Button bossSeal = CreateDebugButton(panel.transform, "DebugBossSeal", "Force Boss Seal", -250f, -945f);
            Button bossDrain = CreateDebugButton(panel.transform, "DebugBossManaDrain", "Force Boss Mana Drain", 250f, -945f);

            SetField(debugPanel, "panel", panel);
            SetField(debugPanel, "skipRoundButtons", skipButtons);
            SetField(debugPanel, "addJadeButton", addJade);
            SetField(debugPanel, "giveAllItemsButton", giveAll);
            SetField(debugPanel, "giveRecommendedBuildButton", recommended);
            SetField(debugPanel, "giveLv2CoreBuildButton", giveCore);
            SetField(debugPanel, "openShopButton", openShop);
            SetField(debugPanel, "rerollShopButton", rerollShop);
            SetField(debugPanel, "giveRandomShopItemButton", randomShopItem);
            SetField(debugPanel, "giveDuplicateFireButton", duplicateFire);
            SetField(debugPanel, "giveDuplicateThunderButton", duplicateThunder);
            SetField(debugPanel, "autoMergeAllButton", autoMerge);
            SetField(debugPanel, "unlockPowerSlotsButton", unlockPower);
            SetField(debugPanel, "lockPowerSlotsButton", lockPower);
            SetField(debugPanel, "startBossFightButton", startBoss);
            SetField(debugPanel, "forceBossChargeButton", bossCharge);
            SetField(debugPanel, "forceBossEnrageButton", bossEnrage);
            SetField(debugPanel, "forceBossSealButton", bossSeal);
            SetField(debugPanel, "forceBossManaDrainButton", bossDrain);
            SetField(debugPanel, "giveAntiBossBuildButton", antiBossBuild);
            SetField(debugPanel, "forceBossPhase2Button", forceBoss);
            SetField(debugPanel, "finishRunWinButton", win);
            SetField(debugPanel, "finishRunLoseButton", lose);
            SetField(debugPanel, "resetRunButton", reset);
            SetField(debugPanel, "runFlowController", runFlow);
            SetField(debugPanel, "combatController", combat);
            SetField(debugPanel, "shopController", shop);
            SetField(debugPanel, "bagExpansionController", bagExpansion);
            SetField(debugPanel, "wallet", wallet);
            SetField(debugPanel, "resultPanel", resultPanel);
            SetField(debugPanel, "statsTracker", statsTracker);
            SetField(debugPanel, "coreBuildItems", new[]
            {
                definitions["spirit_stone_basic"],
                definitions["fire_talisman_basic"],
                definitions["shield_talisman_basic"],
                definitions["qi_pill_basic"],
                definitions["thunder_talisman_basic"],
                definitions["sword_pill_basic"]
            });

            panel.SetActive(false);
        }

        private static Button CreateDebugButton(Transform parent, string name, string label, float x, float y)
        {
            Button button = CreateButton(name, parent, label, new Color(0.22f, 0.25f, 0.24f), 24);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(420f, 88f));
            return button;
        }

        private static void CreateVersionInfo(Transform parent, PlaytestDebugPanel debugPanel)
        {
            Text version = CreateText("VersionInfoLabel", parent, VersionInfoUI.VersionText, 20, FontStyle.Normal, new Color(0.72f, 0.68f, 0.58f, 0.88f));
            version.alignment = TextAnchor.MiddleRight;
            version.raycastTarget = true;
            SetRect(version.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-22f, 10f), new Vector2(460f, 34f));

            VersionInfoUI versionInfo = version.gameObject.AddComponent<VersionInfoUI>();
            SetField(versionInfo, "label", version);
            SetField(versionInfo, "debugPanel", debugPanel);
        }

        private static void CreateResultPanel(Transform parent, BattleResultPanel resultPanel)
        {
            GameObject panel = CreatePanel("BattleResultPanel", parent, new Vector2(0f, 0f), new Vector2(920f, 1180f), new Color(0.075f, 0.065f, 0.055f, 0.98f), TextAnchor.MiddleCenter);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.78f, 0.32f);
            outline.effectDistance = new Vector2(4f, -4f);

            Text title = CreateText("ResultTitle", panel.transform, "战斗胜利", 44, FontStyle.Bold, new Color(1f, 0.86f, 0.5f));
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(780f, 72f));

            Text body = CreateText("ResultBody", panel.transform, "", 28, FontStyle.Normal, Color.white);
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(780f, 830f));

            Button close = CreateButton("CloseResultButton", panel.transform, "重新开始", new Color(0.35f, 0.42f, 0.3f), 30);
            SetRect(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 72f), new Vector2(360f, 92f));

            SetField(resultPanel, "panel", panel);
            SetField(resultPanel, "titleText", title);
            SetField(resultPanel, "bodyText", body);
            SetField(resultPanel, "closeButton", close);
            panel.SetActive(false);
        }

        private static GameObject CreateSlot(Vector2Int position, TalismanBagGrid grid)
        {
            GameObject slot = new($"Slot_{position.x}_{position.y}", typeof(RectTransform), typeof(Image), typeof(TalismanGridSlotView));
            slot.GetComponent<RectTransform>().sizeDelta = new Vector2(112f, 112f);
            Image border = slot.GetComponent<Image>();
            border.color = new Color(0.45f, 0.35f, 0.2f);
            border.raycastTarget = true;

            GameObject bgObject = new("Background", typeof(RectTransform), typeof(Image));
            bgObject.transform.SetParent(slot.transform, false);
            RectTransform bgRect = bgObject.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = new Vector2(6f, 6f);
            bgRect.offsetMax = new Vector2(-6f, -6f);
            Image bg = bgObject.GetComponent<Image>();
            bg.color = new Color(0.16f, 0.13f, 0.1f);
            bg.raycastTarget = false;

            GameObject anchorObject = new("ItemAnchor", typeof(RectTransform));
            anchorObject.transform.SetParent(slot.transform, false);
            RectTransform anchor = anchorObject.GetComponent<RectTransform>();
            anchor.anchorMin = new Vector2(0.5f, 0.5f);
            anchor.anchorMax = new Vector2(0.5f, 0.5f);
            anchor.pivot = new Vector2(0.5f, 0.5f);
            anchor.sizeDelta = new Vector2(104f, 104f);

            GameObject sealedOverlayObject = new("SealedOverlay", typeof(RectTransform), typeof(Image));
            sealedOverlayObject.transform.SetParent(slot.transform, false);
            RectTransform sealedRect = sealedOverlayObject.GetComponent<RectTransform>();
            sealedRect.anchorMin = Vector2.zero;
            sealedRect.anchorMax = Vector2.one;
            sealedRect.offsetMin = new Vector2(6f, 6f);
            sealedRect.offsetMax = new Vector2(-6f, -6f);
            Image sealedOverlay = sealedOverlayObject.GetComponent<Image>();
            sealedOverlay.color = new Color(0.15f, 0.15f, 0.15f, 0.72f);
            sealedOverlay.raycastTarget = false;
            sealedOverlayObject.SetActive(false);

            Text sealText = CreateText("SealText", sealedOverlayObject.transform, "\u5c01", 38, FontStyle.Bold, new Color(0.82f, 0.82f, 0.82f));
            SetRect(sealText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject enhancedBadgeObject = new("EnhancedBadge", typeof(RectTransform), typeof(Image));
            enhancedBadgeObject.transform.SetParent(slot.transform, false);
            RectTransform enhancedBadgeRect = enhancedBadgeObject.GetComponent<RectTransform>();
            SetRect(enhancedBadgeRect, Vector2.one, Vector2.one, Vector2.one, new Vector2(-8f, -8f), new Vector2(58f, 26f));
            Image enhancedBadge = enhancedBadgeObject.GetComponent<Image>();
            enhancedBadge.color = new Color(0.42f, 0.86f, 1f, 0.92f);
            enhancedBadge.raycastTarget = false;
            Text enhancedBadgeText = CreateText("Text", enhancedBadgeObject.transform, "\u9635\u773c", 16, FontStyle.Bold, new Color(0.08f, 0.12f, 0.14f));
            SetRect(enhancedBadgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            enhancedBadgeObject.SetActive(false);

            GameObject formationEyeBadgeObject = new("FormationEyeBadge", typeof(RectTransform), typeof(Image));
            formationEyeBadgeObject.transform.SetParent(slot.transform, false);
            RectTransform eyeBadgeRect = formationEyeBadgeObject.GetComponent<RectTransform>();
            SetRect(eyeBadgeRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(86f, 36f));
            Image eyeBadge = formationEyeBadgeObject.GetComponent<Image>();
            eyeBadge.color = new Color(1f, 0.82f, 0.22f, 0.95f);
            eyeBadge.raycastTarget = false;
            Text eyeBadgeText = CreateText("Text", formationEyeBadgeObject.transform, "\u9635\u773c", 18, FontStyle.Bold, new Color(0.12f, 0.08f, 0.03f));
            SetRect(eyeBadgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            formationEyeBadgeObject.SetActive(false);

            GameObject powerBadgeObject = new("PowerBadge", typeof(RectTransform), typeof(Image));
            powerBadgeObject.transform.SetParent(slot.transform, false);
            RectTransform powerBadgeRect = powerBadgeObject.GetComponent<RectTransform>();
            SetRect(powerBadgeRect, Vector2.one, Vector2.one, Vector2.one, new Vector2(-8f, -8f), new Vector2(34f, 34f));
            Image powerBadge = powerBadgeObject.GetComponent<Image>();
            powerBadge.color = new Color(0.78f, 0.94f, 1f, 0.95f);
            powerBadge.raycastTarget = false;
            Text powerBadgeText = CreateText("Text", powerBadgeObject.transform, "\u4f9b", 16, FontStyle.Bold, new Color(0.05f, 0.12f, 0.14f));
            SetRect(powerBadgeText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            powerBadgeObject.SetActive(false);

            TalismanGridSlotView view = slot.GetComponent<TalismanGridSlotView>();
            SetField(view, "grid", grid);
            SetField(view, "gridPosition", position);
            SetField(view, "background", bg);
            SetField(view, "border", border);
            SetField(view, "sealedOverlay", sealedOverlay);
            SetField(view, "enhancedBadge", enhancedBadgeObject);
            SetField(view, "formationEyeBadge", formationEyeBadgeObject);
            SetField(view, "powerBadge", powerBadgeObject);
            SetField(view, "powerBadgeText", powerBadgeText);
            SetField(view, "itemAnchor", anchor);
            return slot;
        }

        private static GameObject CreateItemView(TalismanItemDefinition definition, TalismanBagGrid grid, Canvas canvas, AutoCombatController controller)
        {
            GameObject item = new($"Item_{definition.itemId}", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(DraggableTalismanItemView));
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(104f, 104f);
            Image background = item.GetComponent<Image>();
            background.color = definition.uiColor;
            background.raycastTarget = true;

            Text label = CreateText("Label", item.transform, definition.displayName, 22, FontStyle.Bold, Color.black);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(item.transform, false);
            Image icon = iconObject.GetComponent<Image>();
            icon.enabled = false;
            icon.raycastTarget = false;

            DraggableTalismanItemView drag = item.GetComponent<DraggableTalismanItemView>();
            SetField(drag, "definition", definition);
            SetField(drag, "grid", grid);
            SetField(drag, "rootCanvas", canvas);
            SetField(drag, "canvasGroup", item.GetComponent<CanvasGroup>());
            SetField(drag, "background", background);
            SetField(drag, "icon", icon);
            SetField(drag, "label", label);
            SetField(drag, "combatController", controller);
            return item;
        }

        private static Text CreateStatusText(string name, Transform parent, string content, Color color)
        {
            Text text = CreateText(name, parent, content, 27, FontStyle.Bold, color);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Image CreateHpBarForStatusText(Text hpText)
        {
            RectTransform hpRect = hpText.rectTransform;
            if (hpRect.parent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }

            GameObject barObject = new("PlayerHPBar", typeof(RectTransform), typeof(LayoutElement), typeof(Image));
            barObject.transform.SetParent(hpRect.parent, false);
            barObject.transform.SetSiblingIndex(hpRect.GetSiblingIndex());

            LayoutElement layoutElement = barObject.GetComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            RectTransform barRect = barObject.GetComponent<RectTransform>();
            SetRect(barRect, hpRect.anchorMin, hpRect.anchorMax, hpRect.pivot, hpRect.anchoredPosition, hpRect.sizeDelta);
            barRect.localScale = Vector3.one;

            Image background = barObject.GetComponent<Image>();
            background.color = new Color(0.18f, 0.025f, 0.025f, 0.78f);
            background.raycastTarget = false;

            GameObject fillObject = new("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(barObject.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            fillRect.pivot = new Vector2(0f, 0.5f);

            Image fill = fillObject.GetComponent<Image>();
            fill.color = new Color(0.86f, 0.08f, 0.06f, 0.95f);
            fill.raycastTarget = false;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 1f;
            return fill;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color, TextAnchor anchor)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Vector2 anchorVector = AnchorToVector(anchor);
            SetRect(panel.GetComponent<RectTransform>(), anchorVector, anchorVector, anchorVector, anchoredPosition, size);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static GameObject CreateStretchPanel(string name, Transform parent, Color color)
        {
            GameObject panel = new(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Transform CreateSafeAreaRoot(Transform parent)
        {
            GameObject root = new("MobileSafeAreaRoot", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Type fitterType = Type.GetType("TalismanBag.UI.MobileSafeAreaFitter, Assembly-CSharp");
            if (fitterType != null)
            {
                root.AddComponent(fitterType);
            }

            return root.transform;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color, int fontSize = 22)
        {
            GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Text", buttonObject.transform, label, fontSize, FontStyle.Bold, Color.white);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return button;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle style, Color color)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static Vector2 AnchorToVector(TextAnchor anchor)
        {
            return anchor switch
            {
                TextAnchor.UpperLeft => new Vector2(0f, 1f),
                TextAnchor.UpperCenter => new Vector2(0.5f, 1f),
                TextAnchor.UpperRight => new Vector2(1f, 1f),
                TextAnchor.MiddleLeft => new Vector2(0f, 0.5f),
                TextAnchor.MiddleCenter => new Vector2(0.5f, 0.5f),
                TextAnchor.MiddleRight => new Vector2(1f, 0.5f),
                TextAnchor.LowerLeft => new Vector2(0f, 0f),
                TextAnchor.LowerCenter => new Vector2(0.5f, 0f),
                TextAnchor.LowerRight => new Vector2(1f, 0f),
                _ => new Vector2(0.5f, 0.5f),
            };
        }

        private static void SetField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                Debug.LogError($"Field '{fieldName}' not found on {target.GetType().Name}.");
                return;
            }

            field.SetValue(target, value);
            if (target is UnityEngine.Object unityObject)
            {
                EditorUtility.SetDirty(unityObject);
            }
        }
    }
}
#endif
