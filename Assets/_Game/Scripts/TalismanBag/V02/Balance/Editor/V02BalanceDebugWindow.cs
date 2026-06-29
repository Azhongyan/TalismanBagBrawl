using System.Collections.Generic;
using System.Linq;
using TalismanBag.Enemies;
using TalismanBag.Items;
using TalismanBag.V02.Config.EditorTools;
using TalismanBag.V02.CoreLoop.MainTrial;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.Formation;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Balance.EditorTools
{
    public sealed class V02BalanceDebugWindow : EditorWindow
    {
        private const string V02AssetRoot = "Assets/_Game/ScriptableObjects/TalismanBag/V02";
        private const string V02ItemPath = "Assets/_Game/ScriptableObjects/TalismanBag/V02/Items";
        private const string V02EnemyPath = "Assets/_Game/ScriptableObjects/TalismanBag/V02";
        private const string EnemyDropdownPath = "Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies";
        private const string V02BalancePath = "Assets/_Game/ScriptableObjects/TalismanBag/V02/Balance";
        private const int ExpectedRoundCount = 10;

        private readonly List<TalismanItemDefinition> talismans = new();
        private readonly List<EnemyDefinition> enemies = new();
        private readonly List<EnemyDefinition> enemyDropdownEnemies = new();
        private readonly List<V02RunConfig> runConfigs = new();
        private readonly List<V02RewardPoolConfig> rewardPools = new();
        private readonly Dictionary<int, bool> roundFoldouts = new();
        private V02CounterMultiplierConfig multiplierConfig;
        private V02FormationBalanceConfig formationConfig;
        private V02RunConfig runConfig;
        private V02RewardPoolConfig rewardPool;
        private bool buildCounterMatrixFoldout = true;
        private int selectedTab;
        private int selectedEnemyIndex;
        private Vector2 mainScroll;
        private Vector2 outputScroll;
        private string qaRewardTableId = "chapter_1_10_clear";
        private string qaLastResult = "QA actions have not run yet. / 尚未执行 QA 操作。";
        private string reportText = "[Build Benchmark / 构筑对标] Press a button to generate a report. / 点击按钮生成报告。";

        [MenuItem("Tools/Talisman Bag/V0.2/Data/[Manual Only] Balance Debug Panel")]
        public static void Open()
        {
            V02BalanceDebugWindow window = GetWindow<V02BalanceDebugWindow>("V0.2 Balance / 平衡");
            window.minSize = new Vector2(760f, 540f);
            window.ReloadAssets();
        }

        private void OnEnable()
        {
            ReloadAssets();
        }

        private void OnGUI()
        {
            DrawHeader();
            selectedTab = GUILayout.SelectionGrid(
                selectedTab,
                new[]
                {
                    "Entry / 总入口",
                    "Counter Config / 克制倍率",
                    "Formation Config / 阵法供能",
                    "Round Config / 关卡配置",
                    "Reward Weights / 奖励权重",
                    "Build Benchmark / 构筑对标",
                    "Read Only Values / 只读数值",
                    "Patch Format / 补丁格式",
                    "Stage QA / 整包验收"
                },
                3,
                EditorStyles.toolbarButton,
                GUILayout.Height(66f));
            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
            switch (selectedTab)
            {
                case 0:
                    DrawEntryTab();
                    break;
                case 1:
                    DrawCounterConfigTab();
                    break;
                case 2:
                    DrawFormationConfigTab();
                    break;
                case 3:
                    DrawRoundConfigTab();
                    break;
                case 4:
                    DrawRewardWeightsTab();
                    break;
                case 5:
                    DrawBuildBenchmarkTab();
                    break;
                case 6:
                    DrawReadOnlyValuesTab();
                    break;
                case 7:
                    DrawPatchFormatTab();
                    break;
                default:
                    DrawStageQaTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("V0.2 Balance Debug Panel / V0.2 平衡调试面板", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Editor-only tool. It does not create game UI, does not touch Canvas/debug popup, and does not write talisman values. / 仅限 Unity 编辑器使用；不会创建游戏内 UI，不会碰 Canvas/debug 弹窗，也不会写入符箓数值。", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reload Assets / 重新扫描", GUILayout.Width(170f)))
                {
                    ReloadAssets();
                }

                if (GUILayout.Button("Item Panel / 符箓面板", GUILayout.Width(150f)))
                {
                    TalismanBag.EditorTools.TalismanItemBalancePanel.Open();
                }

                if (GUILayout.Button("Enemy Identity / 怪物识别", GUILayout.Width(170f)))
                {
                    TalismanBag.EditorTools.TalismanEnemyBalancePanel.Open();
                }

                multiplierConfig = (V02CounterMultiplierConfig)EditorGUILayout.ObjectField("Counter Config / 克制配置", multiplierConfig, typeof(V02CounterMultiplierConfig), false);
                formationConfig = (V02FormationBalanceConfig)EditorGUILayout.ObjectField("Formation Config / 阵法配置", formationConfig, typeof(V02FormationBalanceConfig), false);
            }
        }

        private void DrawEntryTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Unified Balance Entry / 统一调参入口", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use the existing Item and Enemy panels for detailed editing. This window keeps shared V0.2 pacing, reward, benchmark, and read-only snapshots together. / 详细符箓和怪物编辑继续复用现有面板；这里集中管理 V0.2 关卡节奏、奖励权重、构筑对标和只读快照。", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Item Balance Panel / 打开符箓面板", GUILayout.Height(32f)))
                {
                    TalismanBag.EditorTools.TalismanItemBalancePanel.Open();
                }

                if (GUILayout.Button("Open Enemy Identity Panel / 打开怪物识别面板", GUILayout.Height(32f)))
                {
                    TalismanBag.EditorTools.TalismanEnemyBalancePanel.Open();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Active Config Assets / 当前配置资产", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            runConfig = (V02RunConfig)EditorGUILayout.ObjectField("Run Config / 关卡配置", runConfig, typeof(V02RunConfig), false);
            rewardPool = (V02RewardPoolConfig)EditorGUILayout.ObjectField("Reward Pool / 奖励池", rewardPool, typeof(V02RewardPoolConfig), false);
            multiplierConfig = (V02CounterMultiplierConfig)EditorGUILayout.ObjectField("Counter Config / 克制配置", multiplierConfig, typeof(V02CounterMultiplierConfig), false);
            formationConfig = (V02FormationBalanceConfig)EditorGUILayout.ObjectField("Formation Config / 阵法配置", formationConfig, typeof(V02FormationBalanceConfig), false);
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Loaded Snapshot / 已载入快照", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Talismans / 符箓", talismans.Count.ToString());
            EditorGUILayout.LabelField("Enemies / 怪物", enemies.Count.ToString());
            EditorGUILayout.LabelField("Run Configs / 关卡配置", runConfigs.Count.ToString());
            EditorGUILayout.LabelField("Reward Pools / 奖励池", rewardPools.Count.ToString());
        }

        private void DrawCounterConfigTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Counter Config / 克制倍率", EditorStyles.boldLabel);
            multiplierConfig = (V02CounterMultiplierConfig)EditorGUILayout.ObjectField("Counter Config / 克制配置资产", multiplierConfig, typeof(V02CounterMultiplierConfig), false);
            if (multiplierConfig == null)
            {
                EditorGUILayout.HelpBox("Missing V02CounterMultiplierConfig. / 缺少 V02CounterMultiplierConfig。", MessageType.Warning);
                if (GUILayout.Button("Create Counter Config / 创建克制配置", GUILayout.Height(30f)))
                {
                    multiplierConfig = CreateBalanceAsset<V02CounterMultiplierConfig>("CounterMultiplierConfig_V02.asset");
                }

                return;
            }

            SerializedObject serializedConfig = new(multiplierConfig);
            serializedConfig.Update();
            EditorGUI.BeginChangeCheck();
            DrawSerializedProperty(serializedConfig, "strongCounterMultiplier", "Strong Counter / 强克制");
            DrawSerializedProperty(serializedConfig, "lightCounterMultiplier", "Light Counter / 轻克制");
            DrawSerializedProperty(serializedConfig, "neutralMultiplier", "Neutral / 普通");
            DrawSerializedProperty(serializedConfig, "resistedMultiplier", "Resisted / 被抵抗");
            DrawSerializedProperty(serializedConfig, "hardResistedMultiplier", "Hard Resisted / 强抵抗");
            DrawSerializedProperty(serializedConfig, "rewardShieldBreakMultiplier", "Reward Shield Break / 奖励破盾");
            DrawSerializedProperty(serializedConfig, "groupClearMultiplier", "Group Clear / 清群");
            DrawBuildCounterMatrix(serializedConfig);
            if (EditorGUI.EndChangeCheck())
            {
                serializedConfig.ApplyModifiedProperties();
                EditorUtility.SetDirty(multiplierConfig);
            }

            DrawSaveAssetsButton("Save Counter Config / 保存克制配置");
        }

        private void DrawBuildCounterMatrix(SerializedObject serializedConfig)
        {
            SerializedProperty matrix = serializedConfig.FindProperty("buildCounterMatrix");
            EditorGUILayout.Space(10f);
            buildCounterMatrixFoldout = EditorGUILayout.Foldout(buildCounterMatrixFoldout, "Build Counter Matrix / 构筑克制矩阵", true);
            if (!buildCounterMatrixFoldout)
            {
                return;
            }

            if (matrix == null)
            {
                EditorGUILayout.HelpBox("Missing buildCounterMatrix property. / 缺少构筑克制矩阵字段。", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Benchmark-only data. Exact buildId + enemyId rows override the fallback tag logic. / 仅用于 Benchmark：精确命中的 buildId + enemyId 行会覆盖旧标签推导。", MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Rows / 行数: {matrix.arraySize}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Row / 新增一行", GUILayout.Width(150f)))
                {
                    int index = matrix.arraySize;
                    matrix.InsertArrayElementAtIndex(index);
                    ResetBuildCounterMatrixRow(matrix.GetArrayElementAtIndex(index));
                }
            }

            if (matrix.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No matrix rows yet. Benchmark will use FallbackTagLogic until a row is added. / 当前没有矩阵行；Benchmark 会继续使用旧标签逻辑。", MessageType.Info);
                return;
            }

            int removeIndex = -1;
            for (int i = 0; i < matrix.arraySize; i++)
            {
                SerializedProperty row = matrix.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Row {i + 1} / 矩阵行 {i + 1}", EditorStyles.boldLabel);
                        if (GUILayout.Button("Delete / 删除", GUILayout.Width(110f)))
                        {
                            removeIndex = i;
                        }
                    }

                    DrawChildProperty(row, "buildId", "Build ID / 构筑 ID");
                    DrawChildProperty(row, "enemyId", "Enemy ID / 敌人 ID");
                    DrawChildProperty(row, "relation", "Relation / 克制关系");
                    DrawChildProperty(row, "multiplier", "Multiplier / 倍率");
                    DrawChildProperty(row, "note", "Note / 备注");
                }
            }

            if (removeIndex >= 0)
            {
                matrix.DeleteArrayElementAtIndex(removeIndex);
            }
        }

        private static void ResetBuildCounterMatrixRow(SerializedProperty row)
        {
            if (row == null)
            {
                return;
            }

            SetChildString(row, "buildId", string.Empty);
            SetChildString(row, "enemyId", string.Empty);
            SetChildEnum(row, "relation", (int)CounterRelation.Neutral);
            SetChildFloat(row, "multiplier", 1f);
            SetChildString(row, "note", string.Empty);
        }

        private static void SetChildString(SerializedProperty parent, string propertyName, string value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetChildEnum(SerializedProperty parent, string propertyName, int value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.enumValueIndex = value;
            }
        }

        private static void SetChildFloat(SerializedProperty parent, string propertyName, float value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private void DrawFormationConfigTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Formation Config / 阵法供能", EditorStyles.boldLabel);
            formationConfig = (V02FormationBalanceConfig)EditorGUILayout.ObjectField("Formation Config / 阵法配置资产", formationConfig, typeof(V02FormationBalanceConfig), false);
            if (formationConfig == null)
            {
                EditorGUILayout.HelpBox("Missing V02FormationBalanceConfig. / 缺少 V02FormationBalanceConfig。", MessageType.Warning);
                if (GUILayout.Button("Create Formation Config / 创建阵法配置", GUILayout.Height(30f)))
                {
                    formationConfig = CreateBalanceAsset<V02FormationBalanceConfig>("FormationBalanceConfig_V02.asset");
                }

                return;
            }

            SerializedObject serializedConfig = new(formationConfig);
            serializedConfig.Update();
            EditorGUI.BeginChangeCheck();
            DrawSerializedProperty(serializedConfig, "formationEyeCrossPowerEnabled", "Eye Cross Power / 阵眼十字供能");
            DrawSerializedProperty(serializedConfig, "formationEyeDiagonalWeakPowerEnabled", "Eye Diagonal Weak Power / 阵眼斜角弱供能");
            DrawSerializedProperty(serializedConfig, "weakPowerCooldownMultiplier", "Weak Power Cooldown Multiplier / 弱供能冷却倍率");
            DrawSerializedProperty(serializedConfig, "unpoweredTalismansCanTrigger", "Unpowered Can Trigger / 未供能可触发");
            DrawSerializedProperty(serializedConfig, "spiritStoneNineGridPowerEnabled", "Spirit Stone Nine Grid / 聚灵石九宫供能");
            DrawSerializedProperty(serializedConfig, "spiritLinkBetweenStonesEnabled", "Spirit Link Enabled / 聚灵石连线可用");
            DrawSerializedProperty(serializedConfig, "upgradedEyeNineGridEnabled", "Upgraded Eye Nine Grid / 阵眼强化九宫可用");
            DrawSerializedProperty(serializedConfig, "stealEnergyDisableDuration", "Steal Energy Disable Duration / 偷灵失效时间");
            DrawSerializedProperty(serializedConfig, "sealDuration", "Default Seal Duration / 默认封印时间");
            DrawSerializedProperty(serializedConfig, "powerHighlightEnabled", "Power Highlight / 供能高亮");
            DrawSerializedProperty(serializedConfig, "unpoweredHintEnabled", "Unpowered Hint / 未供能提示");
            if (EditorGUI.EndChangeCheck())
            {
                serializedConfig.ApplyModifiedProperties();
                EditorUtility.SetDirty(formationConfig);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply To Open Scene Resolver / 应用到当前场景供能解析器", GUILayout.Height(30f)))
                {
                    ApplyFormationConfigToOpenScene();
                }

                if (GUILayout.Button("Refresh Resolver / 刷新供能显示", GUILayout.Height(30f)))
                {
                    RefreshOpenSceneFormationResolvers();
                }
            }

            DrawSaveAssetsButton("Save Formation Config / 保存阵法配置");
        }

        private void DrawRoundConfigTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Round Config / 关卡配置", EditorStyles.boldLabel);
            runConfig = (V02RunConfig)EditorGUILayout.ObjectField("Run Config / 关卡配置资产", runConfig, typeof(V02RunConfig), false);
            if (runConfig == null)
            {
                EditorGUILayout.HelpBox("Missing V02RunConfig. / 缺少 V02RunConfig。", MessageType.Warning);
                return;
            }

            SerializedObject serializedRun = new(runConfig);
            serializedRun.Update();
            EditorGUI.BeginChangeCheck();

            DrawSerializedProperty(serializedRun, "runId", "Run ID / 流程 ID");
            DrawSerializedProperty(serializedRun, "displayName", "Display Name / 显示名称");

            SerializedProperty rounds = serializedRun.FindProperty("rounds");
            if (rounds == null)
            {
                EditorGUILayout.HelpBox("Missing rounds property. / 缺少关卡列表字段。", MessageType.Error);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Rounds / 关卡：{rounds.arraySize}/{ExpectedRoundCount}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Expand All / 全部展开", GUILayout.Width(150f)))
                    {
                        SetAllRoundFoldouts(rounds.arraySize, true);
                    }

                    if (GUILayout.Button("Collapse All / 全部折叠", GUILayout.Width(150f)))
                    {
                        SetAllRoundFoldouts(rounds.arraySize, false);
                    }

                    if (GUILayout.Button("Add Round / 新增关卡", GUILayout.Width(150f)))
                    {
                        rounds.InsertArrayElementAtIndex(rounds.arraySize);
                    }

                    if (GUILayout.Button("Remove Last / 删除最后一关", GUILayout.Width(170f)) && rounds.arraySize > 0)
                    {
                        rounds.DeleteArrayElementAtIndex(rounds.arraySize - 1);
                    }
                }

                for (int i = 0; i < rounds.arraySize; i++)
                {
                    SerializedProperty round = rounds.GetArrayElementAtIndex(i);
                    DrawRoundConfigElement(round, i);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedRun.ApplyModifiedProperties();
                EditorUtility.SetDirty(runConfig);
            }

            DrawSaveAssetsButton("Save Run Config / 保存关卡配置");
        }

        private void DrawRoundConfigElement(SerializedProperty round, int index)
        {
            if (round == null)
            {
                return;
            }

            string title = GetRoundTitle(round, index);
            EnemyDefinition enemy = GetRoundEnemy(round);
            string enemyLabel = enemy != null ? BuildEnemyOptionLabel(enemy) : "未选择";
            string header = $"Round {index + 1}/{ExpectedRoundCount} / 第 {index + 1} 关    {title}    Enemy / 敌人：{enemyLabel}";

            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool expanded = GetRoundFoldout(index);
                using (new EditorGUILayout.HorizontalScope())
                {
                    expanded = EditorGUILayout.Foldout(expanded, header, true);
                    roundFoldouts[index] = expanded;

                    if (enemy != null && GUILayout.Button("Select Enemy / 选中怪物", GUILayout.Width(150f)))
                    {
                        Selection.activeObject = enemy;
                        EditorGUIUtility.PingObject(enemy);
                    }
                }

                if (!expanded)
                {
                    return;
                }

                EditorGUI.indentLevel++;
                DrawChildProperty(round, "levelId", "Level ID / 主线关卡 ID");
                DrawChildProperty(round, "roundIndex", "Round Index / 关卡序号");
                DrawChildProperty(round, "roundTitle", "Round Title / 关卡标题（建议：盾：周期护盾）");
                DrawRoundEnemyDropdown(round, "Enemy / 本关敌人");
                DrawChildProperty(round, "teachingGoal", "Teaching Goal / 教学目标");
                DrawChildProperty(round, "preBattleHint", "Pre-Battle Hint / 战前提示");
                DrawChildProperty(round, "recommendedCounterTags", "Recommended Counters / 推荐克制标签");
                DrawChildProperty(round, "isBossRound", "Boss Round / Boss 关");

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Pacing / 节奏目标", EditorStyles.boldLabel);
                DrawChildProperty(round, "targetDurationMin", "Target Min Duration / 目标最短时长");
                DrawChildProperty(round, "targetDurationMax", "Target Max Duration / 目标最长时长");
                DrawChildProperty(round, "targetHpLossMin", "Target Min HP Loss / 目标最小掉血");
                DrawChildProperty(round, "targetHpLossMax", "Target Max HP Loss / 目标最大掉血");
                DrawChildProperty(round, "strongCounterExpectedDuration", "Strong Counter Duration / 强克制预期时长");
                DrawChildProperty(round, "neutralExpectedDuration", "Neutral Duration / 普通构筑预期时长");
                DrawChildProperty(round, "badBuildExpectedDuration", "Bad Build Duration / 错误构筑预期时长");
                DrawChildProperty(round, "expectedPlayerHpRemainStrongCounter", "Strong Counter HP Remain / 强克制剩余血量");
                DrawChildProperty(round, "expectedPlayerHpRemainNeutral", "Neutral HP Remain / 普通剩余血量");
                DrawChildProperty(round, "expectedPlayerHpRemainBadBuild", "Bad Build HP Remain / 错误构筑剩余血量");
                DrawChildProperty(round, "benchmarkRule", "Benchmark Rule / 对标判定规则");
                DrawChildProperty(round, "benchmarkTargets", "Benchmark Targets / 构筑目标");
                EditorGUI.indentLevel--;
            }
        }

        private void DrawRoundEnemyDropdown(SerializedProperty round, string label)
        {
            SerializedProperty enemyProperty = round.FindPropertyRelative("enemy");
            if (enemyProperty == null)
            {
                EditorGUILayout.HelpBox("Missing property / 缺少字段: enemy", MessageType.Warning);
                return;
            }

            EnemyDefinition currentEnemy = enemyProperty.objectReferenceValue as EnemyDefinition;
            List<EnemyDefinition> options = BuildEnemyDropdownOptions(currentEnemy);
            string[] labels = new string[options.Count + 1];
            labels[0] = "None / 未选择";

            int selectedIndex = 0;
            for (int i = 0; i < options.Count; i++)
            {
                EnemyDefinition option = options[i];
                labels[i + 1] = BuildEnemyOptionLabel(option);
                if (option == currentEnemy)
                {
                    selectedIndex = i + 1;
                }
            }

            int nextIndex = EditorGUILayout.Popup(label, selectedIndex, labels);
            enemyProperty.objectReferenceValue = nextIndex <= 0 ? null : options[nextIndex - 1];
        }

        private List<EnemyDefinition> BuildEnemyDropdownOptions(EnemyDefinition currentEnemy)
        {
            List<EnemyDefinition> options = new();
            if (currentEnemy != null)
            {
                options.Add(currentEnemy);
            }

            foreach (EnemyDefinition enemy in enemyDropdownEnemies)
            {
                if (enemy != null && !options.Contains(enemy))
                {
                    options.Add(enemy);
                }
            }

            return options;
        }

        private static string BuildEnemyOptionLabel(EnemyDefinition enemy)
        {
            if (enemy == null)
            {
                return "Missing Enemy / 怪物缺失";
            }

            string displayName = string.IsNullOrWhiteSpace(enemy.displayName) ? enemy.name : enemy.displayName.Trim();
            string enemyId = string.IsNullOrWhiteSpace(enemy.enemyId) ? enemy.name : enemy.enemyId.Trim();
            string group = IsFormalV02Enemy(enemy) ? "V0.2 / 正式" : "Legacy / 待归档";
            string disabled = enemy.enabled ? string.Empty : "  [Disabled / 已禁用]";
            return $"{group} | {displayName} / {enemyId}{disabled}";
        }

        private static bool IsFormalV02Enemy(EnemyDefinition enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(enemy).Replace('\\', '/');
            return path.Contains("/TalismanBag/V02/Enemies/");
        }

        private static EnemyDefinition GetRoundEnemy(SerializedProperty round)
        {
            return round?.FindPropertyRelative("enemy")?.objectReferenceValue as EnemyDefinition;
        }

        private static string GetRoundTitle(SerializedProperty round, int index)
        {
            string title = round?.FindPropertyRelative("roundTitle")?.stringValue;
            return string.IsNullOrWhiteSpace(title) ? $"第 {index + 1} 关" : title.Trim();
        }

        private bool GetRoundFoldout(int index)
        {
            if (roundFoldouts.TryGetValue(index, out bool expanded))
            {
                return expanded;
            }

            bool defaultExpanded = index == 0;
            roundFoldouts[index] = defaultExpanded;
            return defaultExpanded;
        }

        private void SetAllRoundFoldouts(int roundCount, bool expanded)
        {
            for (int i = 0; i < roundCount; i++)
            {
                roundFoldouts[i] = expanded;
            }
        }

        private void DrawRewardWeightsTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Reward Weights / 奖励权重", EditorStyles.boldLabel);
            rewardPool = (V02RewardPoolConfig)EditorGUILayout.ObjectField("Reward Pool / 奖励池资产", rewardPool, typeof(V02RewardPoolConfig), false);
            if (rewardPool == null)
            {
                EditorGUILayout.HelpBox("Missing V02RewardPoolConfig. / 缺少 V02RewardPoolConfig。", MessageType.Warning);
                return;
            }

            SerializedObject serializedPool = new(rewardPool);
            serializedPool.Update();
            EditorGUI.BeginChangeCheck();
            DrawSerializedProperty(serializedPool, "poolId", "Pool ID / 奖励池 ID");
            DrawSerializedProperty(serializedPool, "rewards", "Reward Assets / 奖励资产列表");
            if (EditorGUI.EndChangeCheck())
            {
                serializedPool.ApplyModifiedProperties();
                EditorUtility.SetDirty(rewardPool);
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Reward Rows / 奖励数据", EditorStyles.boldLabel);
            if (rewardPool.rewards == null || rewardPool.rewards.Count == 0)
            {
                EditorGUILayout.HelpBox("Reward pool has no rewards. / 当前奖励池没有奖励。", MessageType.Info);
            }
            else
            {
                foreach (V02RewardDefinition reward in rewardPool.rewards)
                {
                    DrawRewardRow(reward);
                }
            }

            DrawSaveAssetsButton("Save Reward Config / 保存奖励配置");
        }

        private void DrawRewardRow(V02RewardDefinition reward)
        {
            if (reward == null)
            {
                EditorGUILayout.HelpBox("Missing reward asset. / 奖励资产缺失。", MessageType.Warning);
                return;
            }

            SerializedObject serializedReward = new(reward);
            serializedReward.Update();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{reward.displayName} / {reward.rewardId}", EditorStyles.boldLabel);
                    if (GUILayout.Button("Select / 选中", GUILayout.Width(96f)))
                    {
                        Selection.activeObject = reward;
                        EditorGUIUtility.PingObject(reward);
                    }
                }

                EditorGUI.BeginChangeCheck();
                DrawSerializedProperty(serializedReward, "displayName", "Display Name / 显示名称");
                DrawSerializedProperty(serializedReward, "rewardId", "Reward ID / 奖励 ID");
                DrawSerializedProperty(serializedReward, "enabled", "Enabled / 启用");
                DrawSerializedProperty(serializedReward, "rewardType", "Reward Type / 奖励类型");
                DrawSerializedProperty(serializedReward, "talismanToAdd", "Talisman To Add / 新增符箓");
                DrawSerializedProperty(serializedReward, "formationModifierType", "Formation Modifier / 阵法改造");
                DrawSerializedProperty(serializedReward, "buildModifierType", "Build Modifier / 流派强化");
                DrawSerializedProperty(serializedReward, "modifierValue", "Modifier Value / 强化数值");
                DrawSerializedProperty(serializedReward, "helpfulAgainstTags", "Helpful Against / 克制标签");
                DrawSerializedProperty(serializedReward, "relatedFunctionTags", "Related Functions / 相关功能");
                DrawSerializedProperty(serializedReward, "baseWeight", "Base Weight / 基础权重");
                DrawSerializedProperty(serializedReward, "nextEnemyBonusWeight", "Next Enemy Bonus / 下一关克制加权");
                DrawSerializedProperty(serializedReward, "forceAsCounterOption", "Force Counter Option / 强制作为克制选项");
                DrawSerializedProperty(serializedReward, "shortDescription", "Short Description / 简短说明");
                DrawSerializedProperty(serializedReward, "detailedDescription", "Detailed Description / 详细说明");

                if (EditorGUI.EndChangeCheck())
                {
                    serializedReward.ApplyModifiedProperties();
                    EditorUtility.SetDirty(reward);
                }
            }
        }

        private void DrawBuildBenchmarkTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Build Route Benchmark / 构筑线路对标", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Generates route-comparison reports from enemy assets and counter multipliers. This is an estimate tool, not a battle log. / 根据敌人资产与克制倍率生成构筑线路对比报告；这是估算工具，不是战斗日志。", MessageType.None);

            string[] enemyNames = BuildEnemyNames();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(enemyNames.Length == 0);
                selectedEnemyIndex = EditorGUILayout.Popup("Enemy / 敌人", Mathf.Clamp(selectedEnemyIndex, 0, Mathf.Max(0, enemyNames.Length - 1)), enemyNames);
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Select Enemy / 选中敌人", GUILayout.Width(170f)) && enemies.Count > 0)
                {
                    Selection.activeObject = enemies[Mathf.Clamp(selectedEnemyIndex, 0, enemies.Count - 1)];
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Current Enemy Report / 当前敌人报告", GUILayout.Height(32f)))
                {
                    EnemyDefinition enemy = enemies.Count > 0 ? enemies[Mathf.Clamp(selectedEnemyIndex, 0, enemies.Count - 1)] : null;
                    V02RoundConfig round = FindRoundForEnemy(enemy);
                    reportText = round != null
                        ? V02BuildBenchmarkUtility.BuildEnemyReport(round, multiplierConfig)
                        : V02BuildBenchmarkUtility.BuildEnemyReport(enemy, multiplierConfig);
                    Debug.Log(reportText);
                }

                if (GUILayout.Button("All Enemies Report / 全敌人报告", GUILayout.Height(32f)))
                {
                    reportText = runConfig != null
                        ? V02BuildBenchmarkUtility.BuildAllRoundsReport(runConfig, multiplierConfig)
                        : V02BuildBenchmarkUtility.BuildAllEnemiesReport(enemies, multiplierConfig);
                    Debug.Log(reportText);
                }

                if (GUILayout.Button("Route Matrix / 路线矩阵", GUILayout.Height(32f)))
                {
                    reportText = runConfig != null
                        ? V02BuildBenchmarkUtility.BuildRouteMatrix(runConfig, multiplierConfig)
                        : V02BuildBenchmarkUtility.BuildRouteMatrix(enemies, multiplierConfig);
                    Debug.Log(reportText);
                }

                if (GUILayout.Button("Target Report / 目标报告", GUILayout.Height(32f)))
                {
                    reportText = V02BuildBenchmarkUtility.BuildBenchmarkTargetReport(runConfig, multiplierConfig);
                    Debug.Log(reportText);
                }

                if (GUILayout.Button("Copy / 复制", GUILayout.Width(110f), GUILayout.Height(32f)))
                {
                    GUIUtility.systemCopyBuffer = reportText;
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Report / 报告输出", EditorStyles.boldLabel);
            outputScroll = EditorGUILayout.BeginScrollView(outputScroll, GUILayout.MinHeight(280f));
            EditorGUILayout.TextArea(reportText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void DrawReadOnlyValuesTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Read Only Snapshot / 只读快照", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This tab is intentionally read-only for now, because current task says not to change talisman values. / 这个标签页目前刻意保持只读，因为当前只用于查看快照，不直接改符箓数值。", MessageType.None);

            DrawCounterConfigReadOnly();
            DrawFormationConfigReadOnly();
            DrawTalismanReadOnlyList();
            DrawEnemyReadOnlyList();
        }

        private void DrawCounterConfigReadOnly()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Counter Multipliers / 克制倍率", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            if (multiplierConfig == null)
            {
                EditorGUILayout.LabelField("Missing V02CounterMultiplierConfig. / 缺少 V02 克制倍率配置。");
            }
            else
            {
                EditorGUILayout.FloatField("Strong Counter / 强克制", multiplierConfig.strongCounterMultiplier);
                EditorGUILayout.FloatField("Light Counter / 轻克制", multiplierConfig.lightCounterMultiplier);
                EditorGUILayout.FloatField("Neutral / 普通", multiplierConfig.neutralMultiplier);
                EditorGUILayout.FloatField("Resisted / 被抵抗", multiplierConfig.resistedMultiplier);
                EditorGUILayout.FloatField("Hard Resisted / 强抵抗", multiplierConfig.hardResistedMultiplier);
                EditorGUILayout.FloatField("Reward Shield Break / 奖励破盾", multiplierConfig.rewardShieldBreakMultiplier);
                EditorGUILayout.FloatField("Group Clear / 清群", multiplierConfig.groupClearMultiplier);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawFormationConfigReadOnly()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Formation Config / 阵法配置", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            if (formationConfig == null)
            {
                EditorGUILayout.LabelField("Missing V02FormationBalanceConfig. / 缺少 V02 阵法配置。");
            }
            else
            {
                EditorGUILayout.Toggle("Eye Cross Power / 阵眼十字供能", formationConfig.formationEyeCrossPowerEnabled);
                EditorGUILayout.Toggle("Eye Diagonal Weak Power / 阵眼斜角弱供能", formationConfig.formationEyeDiagonalWeakPowerEnabled);
                EditorGUILayout.FloatField("Weak Power Cooldown / 弱供能冷却倍率", formationConfig.weakPowerCooldownMultiplier);
                EditorGUILayout.Toggle("Unpowered Can Trigger / 未供能可触发", formationConfig.unpoweredTalismansCanTrigger);
                EditorGUILayout.Toggle("Spirit Stone Nine Grid / 聚灵石九宫供能", formationConfig.spiritStoneNineGridPowerEnabled);
                EditorGUILayout.Toggle("Spirit Link / 聚灵石连线", formationConfig.spiritLinkBetweenStonesEnabled);
                EditorGUILayout.Toggle("Upgraded Eye Nine Grid / 阵眼强化九宫", formationConfig.upgradedEyeNineGridEnabled);
                EditorGUILayout.FloatField("Steal Energy Duration / 偷灵失效时间", formationConfig.stealEnergyDisableDuration);
                EditorGUILayout.FloatField("Default Seal Duration / 默认封印时间", formationConfig.sealDuration);
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawTalismanReadOnlyList()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField($"Talismans / 符箓 ({talismans.Count})", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            foreach (TalismanItemDefinition item in talismans)
            {
                if (item == null)
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.ObjectField("Asset / 资产", item, typeof(TalismanItemDefinition), false);
                    EditorGUILayout.LabelField("Item ID / 物品 ID", item.itemId);
                    EditorGUILayout.LabelField("Display Name / 显示名称", item.displayName);
                    EditorGUILayout.FloatField("Base Cooldown / 基础冷却", item.baseCooldown);
                    EditorGUILayout.IntField("Mana Cost / 耗灵", item.manaCost);
                    EditorGUILayout.IntField("Base Value / 基础数值", item.baseValue);
                    EditorGUILayout.Toggle("Requires Power / 需要供能", item.requiresFormationPower);
                    EditorGUILayout.EnumPopup("Element Tag / 元素标签", item.elementTag);
                    EditorGUILayout.LabelField("Effect Type / 效果类型", item.effectType.ToString());
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DrawEnemyReadOnlyList()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField($"Enemies / 敌人 ({enemies.Count})", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            foreach (EnemyDefinition enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.ObjectField("Asset / 资产", enemy, typeof(EnemyDefinition), false);
                    EditorGUILayout.LabelField("Enemy ID / 敌人 ID", enemy.enemyId);
                    EditorGUILayout.LabelField("Threat Name / 怪物识别名", enemy.GetDisplayName());
                    EditorGUILayout.LabelField("Avatar Glyph / 头像字", enemy.GetAvatarGlyph());
                    EditorGUILayout.LabelField("Class / 职能", enemy.enemyClass);
                    EditorGUILayout.LabelField("Archetype / 原型", enemy.enemyArchetype);
                    EditorGUILayout.IntField("Max HP / 最大气血", enemy.maxHp);
                    EditorGUILayout.IntField("Attack Damage / 攻击伤害", enemy.attackDamage);
                    EditorGUILayout.FloatField("Attack Interval / 攻击间隔", enemy.attackInterval);
                    EditorGUILayout.IntField("Skills / 技能数", enemy.skills != null ? enemy.skills.Count : 0);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private static void DrawPatchFormatTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("AI Patch Format / AI 参数补丁格式", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this stable format when asking AI for the next balance patch. Apply changes manually in assets or a future editable panel. / 后续让 AI 给调参建议时使用这个稳定格式；当前请手动在资产或总控面板里应用修改。", MessageType.None);
            EditorGUILayout.TextArea(
                "# Format / 格式：Target[row_id].field: old -> new\n" +
                "EnemyBalanceRow[turtle_guardian_shield].maxHp: 120 -> 135\n" +
                "EnemyBalanceRow[turtle_guardian_shield].skillCooldown: 10 -> 8\n" +
                "CounterBalanceConfig.thunderVsShieldMultiplier: 1.8 -> 2.2\n" +
                "CounterBalanceConfig.fireVsShieldMultiplier: 0.7 -> 0.55\n" +
                "RewardBalanceRow[reward_add_spirit_stone].baseWeight: 10 -> 14",
                GUILayout.MinHeight(120f));
        }

        private void DrawStageQaTab()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("StageConfigPanel01 QA / Debug Only", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "These controls are QA-only. Save reset uses SaveService; jumps prepare state through MainTrialFlowService and then execute the normal V02RunFlowController startup route; reward simulation uses RewardService.",
                MessageType.Warning);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to use save, jump, reward, and state controls. Validation, refresh, and opening StageConfigPanel01 remain available in Edit Mode.",
                    MessageType.Info);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("[QA / Debug Only] Refresh Config", GUILayout.Height(30f)))
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    ReloadAssets();
                    qaLastResult = "Config assets refreshed through AssetDatabase.";
                }

                if (GUILayout.Button("[QA / Debug Only] Validate DataCatalog", GUILayout.Height(30f)))
                {
                    RunQaDataCatalogValidation();
                }

                if (GUILayout.Button("[QA / Debug Only] Open StageConfigPanel01", GUILayout.Height(30f)))
                {
                    DataCatalogEditorWindow.OpenTab(StageConfigPanelTab.Validation);
                    qaLastResult = "Opened StageConfigPanel01 Validation tab.";
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Main Trial Flow / 主线流程", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("[QA / Debug Only] Reset CoreLoop Save", GUILayout.Height(32f)))
                    {
                        ExecuteRunFlowAction(flow => flow.QaResetCoreLoopSave(), "CoreLoop save reset through SaveService.");
                    }

                    if (GUILayout.Button("[QA / Debug Only] Start Main Trial 1-1", GUILayout.Height(32f)))
                    {
                        ExecuteRunFlowAction(flow => flow.QaStartMainTrialFromOneOne(), "Main Trial restarted from formal 1-1 route.");
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("[QA / Debug Only] Jump To 1-10 Boss", GUILayout.Height(32f)))
                    {
                        ExecuteRunFlowAction(flow => flow.QaJumpToChapterOneBoss(), "Prepared and entered the formal 1-10 Boss route.");
                    }

                    if (GUILayout.Button("[QA / Debug Only] Jump To 2-10 Boss Ready", GUILayout.Height(32f)))
                    {
                        ExecuteRunFlowAction(flow => flow.QaJumpToChapterTwoBossReady(), "Prepared the formal 2-10 BossInfo route.");
                    }
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Reward Service / 奖励服务", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("rewardTableId", GUILayout.Width(120f));
                qaRewardTableId = EditorGUILayout.TextField(qaRewardTableId);
                using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                {
                    if (GUILayout.Button("[QA / Debug Only] Simulate Reward", GUILayout.Width(220f)))
                    {
                        SimulateQaReward();
                    }
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Read-only State / 只读状态", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("[QA / Debug Only] Print MainTrial State", GUILayout.Height(30f)))
                {
                    PrintQaMainTrialState();
                }

                if (GUILayout.Button("[QA / Debug Only] Print Resource / Inventory", GUILayout.Height(30f)))
                {
                    PrintQaResourceInventorySummary();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(qaLastResult, MessageType.None);
        }

        private void ExecuteRunFlowAction(System.Action<V02RunFlowController> action, string successMessage)
        {
            V02RunFlowController flow = FindObjectOfType<V02RunFlowController>(true);
            if (flow == null)
            {
                qaLastResult = "Missing V02RunFlowController in the active Play Mode scene.";
                Debug.LogWarning($"[StageConfigPanel01][QA] {qaLastResult}");
                return;
            }

            action?.Invoke(flow);
            qaLastResult = successMessage;
            Debug.Log($"[StageConfigPanel01][QA] {successMessage}");
        }

        private void SimulateQaReward()
        {
            V02RunFlowController flow = FindObjectOfType<V02RunFlowController>(true);
            if (flow == null)
            {
                qaLastResult = "Missing V02RunFlowController; reward was not granted.";
                Debug.LogWarning($"[StageConfigPanel01][QA] {qaLastResult}");
                return;
            }

            RewardResult result = flow.QaGrantRewardById(qaRewardTableId);
            qaLastResult = result != null && result.HasRewards
                ? $"RewardService granted {result.rewards.Count} entries from '{result.rewardId}'."
                : $"No reward granted for rewardTableId '{qaRewardTableId}'.";
            Debug.Log($"[StageConfigPanel01][QA] {qaLastResult}");
        }

        private void RunQaDataCatalogValidation()
        {
            IReadOnlyList<DataCatalogValidationResult> results = DataCatalogValidator.Validate(DataCatalog.Collect());
            int errors = results.Count(result => result.Level == DataCatalogValidationLevel.Error);
            int warnings = results.Count(result => result.Level == DataCatalogValidationLevel.Warning);
            int infos = results.Count(result => result.Level == DataCatalogValidationLevel.Info);
            qaLastResult = $"DataCatalog Error={errors}, Warning={warnings}, Info={infos}.";
            Debug.Log($"[StageConfigPanel01][QA] {qaLastResult}");
        }

        private void PrintQaMainTrialState()
        {
            SaveData saveData = SaveService.GetOrCreate().EnsureLoaded();
            MainTrialProgressData progress = saveData.mainTrialProgressData ?? new MainTrialProgressData();
            progress.Normalize();
            MainTrialFlowService flowService = FindObjectOfType<MainTrialFlowService>(true);
            MainTrialStartupRoute route = flowService != null ? flowService.GetStartupRoute() : null;
            string summary =
                "[StageConfigPanel01][QA][MainTrial] " +
                $"mainTrialPhase={progress.mainTrialPhase}, " +
                $"currentRoundId={progress.currentRoundId}, " +
                $"currentMainTrialLevelId={progress.currentMainTrialLevelId}, " +
                $"chapterOneBossRewardClaimed={progress.chapterOneBossRewardClaimed}, " +
                $"firstUpgradeCompleted={progress.firstUpgradeCompleted}, " +
                $"chapterTwoBossCleared={progress.chapterTwoBossCleared}, " +
                $"coreLoopCompleted={progress.coreLoopCompleted}, " +
                $"startupRoute={(route != null ? route.routeType.ToString() : "Unavailable")}";
            Debug.Log(summary);
            qaLastResult = summary;
        }

        private void PrintQaResourceInventorySummary()
        {
            SaveData saveData = SaveService.GetOrCreate().EnsureLoaded();
            saveData.Normalize();
            string items = saveData.itemInventoryData?.items == null || saveData.itemInventoryData.items.Count == 0
                ? "empty"
                : string.Join(", ", saveData.itemInventoryData.items.Select(item => $"{item.itemId}x{item.amount}"));
            string summary =
                "[StageConfigPanel01][QA][Economy] " +
                $"SpiritStone={saveData.resourceData.spiritStone}, " +
                $"TalismanPaper={saveData.resourceData.talismanPaper}, " +
                $"Cinnabar={saveData.resourceData.cinnabar}, " +
                $"BasicTalismanEmbryo={saveData.resourceData.basicTalismanEmbryo}, " +
                $"Cultivation={saveData.resourceData.cultivation}; " +
                $"Inventory={items}";
            Debug.Log(summary);
            qaLastResult = summary;
        }

        private void DrawSaveAssetsButton(string label)
        {
            EditorGUILayout.Space(8f);
            if (GUILayout.Button(label, GUILayout.Height(30f)))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ShowNotification(new GUIContent("Saved / 已保存"));
            }
        }

        private T CreateBalanceAsset<T>(string fileName) where T : ScriptableObject
        {
            EnsureBalanceFolder();
            string path = $"{V02BalancePath}/{fileName}";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
            ReloadAssets();
            return asset;
        }

        private static void EnsureBalanceFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game"))
            {
                AssetDatabase.CreateFolder("Assets", "_Game");
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Game/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets/_Game", "ScriptableObjects");
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Game/ScriptableObjects/TalismanBag"))
            {
                AssetDatabase.CreateFolder("Assets/_Game/ScriptableObjects", "TalismanBag");
            }

            if (!AssetDatabase.IsValidFolder("Assets/_Game/ScriptableObjects/TalismanBag/V02"))
            {
                AssetDatabase.CreateFolder("Assets/_Game/ScriptableObjects/TalismanBag", "V02");
            }

            if (!AssetDatabase.IsValidFolder(V02BalancePath))
            {
                AssetDatabase.CreateFolder("Assets/_Game/ScriptableObjects/TalismanBag/V02", "Balance");
            }
        }

        private void ApplyFormationConfigToOpenScene()
        {
            if (formationConfig == null)
            {
                ShowNotification(new GUIContent("Missing formation config / 缺少阵法配置"));
                return;
            }

            int applied = 0;
            foreach (FormationPowerResolver resolver in FindObjectsOfType<FormationPowerResolver>(true))
            {
                if (resolver == null)
                {
                    continue;
                }

                resolver.BindFormationBalanceConfig(formationConfig);
                EditorUtility.SetDirty(resolver);
                applied++;
            }

            ShowNotification(new GUIContent($"Applied to resolvers / 已应用：{applied}"));
        }

        private void RefreshOpenSceneFormationResolvers()
        {
            int refreshed = 0;
            foreach (FormationPowerResolver resolver in FindObjectsOfType<FormationPowerResolver>(true))
            {
                if (resolver == null)
                {
                    continue;
                }

                resolver.RefreshPowerStates();
                EditorUtility.SetDirty(resolver);
                refreshed++;
            }

            ShowNotification(new GUIContent($"Refreshed resolvers / 已刷新：{refreshed}"));
        }

        private static void DrawSerializedProperty(SerializedObject serializedObject, string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox($"Missing property / 缺少字段: {propertyName}", MessageType.Warning);
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }

        private static void DrawChildProperty(SerializedProperty parent, string propertyName, string label)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox($"Missing property / 缺少字段: {propertyName}", MessageType.Warning);
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }

        private void ReloadAssets()
        {
            talismans.Clear();
            enemies.Clear();
            enemyDropdownEnemies.Clear();
            runConfigs.Clear();
            rewardPools.Clear();
            talismans.AddRange(LoadAssets<TalismanItemDefinition>(V02ItemPath));
            enemies.AddRange(LoadAssets<EnemyDefinition>(V02EnemyPath));
            enemyDropdownEnemies.AddRange(LoadAssets<EnemyDefinition>(EnemyDropdownPath));
            runConfigs.AddRange(LoadAssets<V02RunConfig>(V02AssetRoot));
            rewardPools.AddRange(LoadAssets<V02RewardPoolConfig>(V02AssetRoot));
            multiplierConfig = multiplierConfig != null ? multiplierConfig : LoadFirstAsset<V02CounterMultiplierConfig>(V02BalancePath);
            formationConfig = formationConfig != null ? formationConfig : LoadFirstAsset<V02FormationBalanceConfig>(V02BalancePath);
            runConfig = runConfig != null ? runConfig : FirstOrDefault(runConfigs);
            rewardPool = rewardPool != null ? rewardPool : FirstOrDefault(rewardPools);
            selectedEnemyIndex = Mathf.Clamp(selectedEnemyIndex, 0, Mathf.Max(0, enemies.Count - 1));
            Repaint();
        }

        private V02RoundConfig FindRoundForEnemy(EnemyDefinition enemy)
        {
            if (enemy == null || runConfig?.rounds == null)
            {
                return null;
            }

            foreach (V02RoundConfig round in runConfig.rounds)
            {
                if (round != null && round.enemy == enemy)
                {
                    return round;
                }
            }

            return null;
        }

        private string[] BuildEnemyNames()
        {
            if (enemies.Count == 0)
            {
                return new[] { "No enemies found / 未找到敌人" };
            }

            string[] result = new string[enemies.Count];
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyDefinition enemy = enemies[i];
                result[i] = enemy != null ? $"【{enemy.GetAvatarGlyph()}】{enemy.GetDisplayName()} / {enemy.enemyId}" : "Missing enemy / 敌人缺失";
            }

            return result;
        }

        private static List<T> LoadAssets<T>(string searchPath) where T : Object
        {
            List<T> result = new();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null && !result.Contains(asset))
                {
                    result.Add(asset);
                }
            }

            result.Sort((left, right) => string.Compare(left.name, right.name, System.StringComparison.Ordinal));
            return result;
        }

        private static T LoadFirstAsset<T>(string searchPath) where T : Object
        {
            List<T> assets = LoadAssets<T>(searchPath);
            return assets.Count > 0 ? assets[0] : null;
        }

        private static T FirstOrDefault<T>(List<T> assets) where T : Object
        {
            return assets != null && assets.Count > 0 ? assets[0] : null;
        }
    }
}
