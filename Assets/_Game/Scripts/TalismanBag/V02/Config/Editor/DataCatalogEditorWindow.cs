#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Tutorial;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public enum StageConfigPanelTab
    {
        Stage,
        Enemy,
        EnemyGroup,
        Reward,
        DropTable,
        Boss,
        Tutorial,
        Item,
        Validation
    }

    public sealed class DataCatalogEditorWindow : EditorWindow
    {
        private static readonly string[] TabLabels =
        {
            "Stage / 关卡",
            "Enemy / 怪物",
            "EnemyGroup / 怪物组",
            "Reward / 奖励",
            "DropTable / 掉落",
            "Boss",
            "Tutorial / 教学",
            "Item / 道具",
            "Validation / 校验"
        };

        private readonly Dictionary<int, bool> assetFoldouts = new();
        private DataCatalog catalog;
        private IReadOnlyList<DataCatalogValidationResult> validationResults;
        private StageConfigPanelTab selectedTab;
        private Vector2 scroll;
        private bool showInfo;

        [MenuItem("Tools/Talisman Bag/V0.2/Data/[Manual Only] Stage Config Panel 01")]
        public static void Open()
        {
            OpenTab(StageConfigPanelTab.Stage);
        }

        public static void OpenTab(StageConfigPanelTab tab)
        {
            DataCatalogEditorWindow window = GetWindow<DataCatalogEditorWindow>("Stage Config Panel 01");
            window.minSize = new Vector2(880f, 620f);
            window.selectedTab = tab;
            window.scroll = Vector2.zero;
            window.Reload();
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            Reload();
        }

        private void OnProjectChange()
        {
            Reload();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();

            int previousTab = (int)selectedTab;
            selectedTab = (StageConfigPanelTab)GUILayout.SelectionGrid(
                previousTab,
                TabLabels,
                5,
                EditorStyles.toolbarButton,
                GUILayout.Height(44f));
            if ((int)selectedTab != previousTab)
            {
                scroll = Vector2.zero;
            }

            scroll = EditorGUILayout.BeginScrollView(
                scroll,
                false,
                true,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            switch (selectedTab)
            {
                case StageConfigPanelTab.Stage:
                    DrawStages();
                    break;
                case StageConfigPanelTab.Enemy:
                    DrawEnemies();
                    break;
                case StageConfigPanelTab.EnemyGroup:
                    DrawEnemyGroups();
                    break;
                case StageConfigPanelTab.Reward:
                    DrawRewards();
                    break;
                case StageConfigPanelTab.DropTable:
                    DrawDrops();
                    break;
                case StageConfigPanelTab.Boss:
                    DrawBosses();
                    break;
                case StageConfigPanelTab.Tutorial:
                    DrawTutorials();
                    break;
                case StageConfigPanelTab.Item:
                    DrawItems();
                    break;
                default:
                    DrawValidation();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(
                    "StageConfigPanel01 / 数据配置主面板",
                    EditorStyles.boldLabel,
                    GUILayout.Width(250f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reload / 重新扫描", EditorStyles.toolbarButton, GUILayout.Width(140f)))
                {
                    Reload();
                }

                if (GUILayout.Button("Validate / 校验", EditorStyles.toolbarButton, GUILayout.Width(130f)))
                {
                    validationResults = DataCatalogValidator.Validate(catalog);
                    selectedTab = StageConfigPanelTab.Validation;
                    scroll = Vector2.zero;
                }
            }

            EditorGUILayout.HelpBox(
                "本面板只编辑 ScriptableObject 配置并显示校验结果；不会发放奖励、修改存档、改变数值或控制 CoreLoop 流程。",
                MessageType.Info);
        }

        private void DrawStages()
        {
            DrawTabHeader("Stage / 关卡", "编辑 RunConfig 中的章节与关卡配置。关卡列表和每个关卡均使用 Foldout。");
            int visibleCount = 0;
            foreach (V02RunConfig runConfig in catalog.RunConfigs)
            {
                if (runConfig == null)
                {
                    continue;
                }

                visibleCount++;
                bool expanded = DrawAssetHeader(runConfig, runConfig.name);
                if (!expanded)
                {
                    continue;
                }

                SerializedObject serialized = new(runConfig);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    StageConfigPanelEditorUi.ObjectField("Run Config / 流程配置", runConfig, typeof(V02RunConfig));
                    DrawStageCollection(serialized.FindProperty("rounds"), "Chapter 1 / 第一章");
                    DrawStageCollection(serialized.FindProperty("chapterTwoRounds"), "Chapter 2 / 第二章");
                    ApplyChanges(serialized, runConfig);
                }
            }

            DrawEmptyState(visibleCount, "No RunConfig assets found.");
            DrawSaveButton("Save Stage Configs / 保存关卡配置");
        }

        private void DrawEnemies()
        {
            DrawTabHeader("Enemy / 怪物", "统一编辑 EnemyDefinition。数组与技能等复杂字段默认折叠。");
            int visibleCount = 0;
            foreach (EnemyDefinition enemy in catalog.Enemies)
            {
                if (enemy == null || enemy.isDebugOnly || enemy.isDeprecated)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(enemy, enemy.GetCatalogLabel(), "Enemy Asset / 怪物资产");
            }

            DrawEmptyState(visibleCount, "No production enemy assets found.");
            DrawSaveButton("Save Enemies / 保存怪物配置");
        }

        private void DrawEnemyGroups()
        {
            DrawTabHeader("EnemyGroup / 怪物组", "怪物组成员列表使用 Foldout，避免横向展开。");
            int visibleCount = 0;
            foreach (EnemyGroupConfig group in catalog.EnemyGroups)
            {
                if (group == null || group.isDebugOnly || group.isDeprecated)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(group, group.GetCatalogLabel(), "Enemy Group / 怪物组资产");
            }

            DrawEmptyState(visibleCount, "No production enemy groups found.");
            DrawSaveButton("Save Enemy Groups / 保存怪物组");
        }

        private void DrawRewards()
        {
            DrawTabHeader("Reward / 奖励", "奖励条目列表使用 Foldout，字段采用固定 Label 宽度。");
            int visibleCount = 0;
            foreach (RewardConfig reward in catalog.Rewards)
            {
                if (reward == null || reward.isDebugOnly || reward.isDeprecated)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(reward, reward.GetCatalogLabel(), "Reward Asset / 奖励资产");
            }

            DrawEmptyState(visibleCount, "No production reward configs found.");
            DrawSaveButton("Save Rewards / 保存奖励配置");
        }

        private void DrawDrops()
        {
            DrawTabHeader("DropTable / 掉落", "集中编辑主动掉落表与挂机掉落配置。");
            int visibleCount = 0;
            foreach (RewardDropTable table in catalog.DropTables)
            {
                if (table == null || table.isDebugOnly || table.isDeprecated)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(table, table.GetCatalogLabel(), "Drop Table / 掉落表");
            }

            foreach (IdleDropConfig config in catalog.IdleDropConfigs)
            {
                if (config == null || config.isDebugOnly || config.isDeprecated)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(config, config.GetCatalogLabel(), "Idle Drop / 挂机掉落");
            }

            DrawEmptyState(visibleCount, "No production drop configs found.");
            DrawSaveButton("Save Drop Tables / 保存掉落配置");
        }

        private void DrawBosses()
        {
            DrawTabHeader("Boss", "Boss 阶段、引用和复杂配置使用 Foldout。");
            int visibleCount = 0;
            foreach (BossInfoConfig boss in catalog.BossConfigs)
            {
                if (boss == null || boss.isDebugOnly || boss.isDeprecated)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(boss, boss.GetCatalogLabel(), "Boss Asset / Boss 资产");
            }

            DrawEmptyState(visibleCount, "No production Boss configs found.");
            DrawSaveButton("Save Boss Configs / 保存 Boss 配置");
        }

        private void DrawTutorials()
        {
            DrawTabHeader("Tutorial / 教学", "教学步骤列表使用 Foldout；本页不执行教学或发奖。");
            int visibleCount = 0;
            foreach (TutorialGuideConfig tutorial in catalog.TutorialConfigs)
            {
                if (tutorial == null)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(tutorial, tutorial.name, "Tutorial Asset / 教学资产");
            }

            DrawEmptyState(visibleCount, "No TutorialGuideConfig assets found.");
            DrawSaveButton("Save Tutorials / 保存教学配置");
        }

        private void DrawItems()
        {
            DrawTabHeader("Item / 道具", "统一编辑 TalismanItemDefinition。标签、效果参数等列表默认折叠。");
            int visibleCount = 0;
            foreach (TalismanItemDefinition item in catalog.Items)
            {
                if (item == null || item.isDebugOnly || item.isDeprecated)
                {
                    continue;
                }

                visibleCount++;
                DrawAssetEditor(item, item.GetCatalogLabel(), "Item Asset / 道具资产");
            }

            DrawEmptyState(visibleCount, "No production item assets found.");
            DrawSaveButton("Save Items / 保存道具配置");
        }

        private void DrawValidation()
        {
            DrawTabHeader("Validation / 校验", "只读显示 DataCatalog 校验结果。");
            validationResults ??= DataCatalogValidator.Validate(catalog);
            int errors = 0;
            int warnings = 0;
            int infos = 0;
            foreach (DataCatalogValidationResult result in validationResults)
            {
                switch (result.Level)
                {
                    case DataCatalogValidationLevel.Error:
                        errors++;
                        break;
                    case DataCatalogValidationLevel.Warning:
                        warnings++;
                        break;
                    default:
                        infos++;
                        break;
                }
            }

            EditorGUILayout.LabelField(
                $"Error: {errors}    Warning: {warnings}    Info: {infos}",
                EditorStyles.boldLabel);
            showInfo = EditorGUILayout.ToggleLeft("Show Info / 显示提示", showInfo);
            foreach (DataCatalogValidationResult result in validationResults)
            {
                if (!showInfo && result.Level == DataCatalogValidationLevel.Info)
                {
                    continue;
                }

                MessageType type = result.Level switch
                {
                    DataCatalogValidationLevel.Error => MessageType.Error,
                    DataCatalogValidationLevel.Warning => MessageType.Warning,
                    _ => MessageType.Info
                };
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.HelpBox(result.ToString(), type);
                    if (result.Context != null &&
                        GUILayout.Button("Select Asset / 定位资产", GUILayout.Width(150f)))
                    {
                        Selection.activeObject = result.Context;
                        EditorGUIUtility.PingObject(result.Context);
                    }
                }
            }
        }

        private void DrawAssetEditor<T>(T asset, string title, string objectLabel) where T : UnityEngine.Object
        {
            if (!DrawAssetHeader(asset, title))
            {
                return;
            }

            SerializedObject serialized = new(asset);
            serialized.Update();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                StageConfigPanelEditorUi.ObjectField(objectLabel, asset, typeof(T));
                StageConfigPanelEditorUi.DrawAllProperties(serialized);
                ApplyChanges(serialized, asset);
            }
        }

        private bool DrawAssetHeader(UnityEngine.Object asset, string title)
        {
            int key = asset.GetInstanceID();
            assetFoldouts.TryGetValue(key, out bool expanded);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                expanded = EditorGUILayout.Foldout(expanded, title, true);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select / 定位", GUILayout.Width(100f)))
                {
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            assetFoldouts[key] = expanded;
            return expanded;
        }

        private static void DrawStageCollection(SerializedProperty rounds, string title)
        {
            EditorGUILayout.Space(4f);
            if (rounds == null)
            {
                EditorGUILayout.HelpBox($"Missing stage collection: {title}", MessageType.Error);
                return;
            }

            rounds.isExpanded = EditorGUILayout.Foldout(
                rounds.isExpanded,
                $"{title} ({rounds.arraySize})",
                true);
            if (!rounds.isExpanded)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                for (int i = 0; i < rounds.arraySize; i++)
                {
                    SerializedProperty round = rounds.GetArrayElementAtIndex(i);
                    string stageId = round.FindPropertyRelative("levelId")?.stringValue;
                    string displayName = round.FindPropertyRelative("roundTitle")?.stringValue;
                    string header = $"{i + 1}. {displayName} [{stageId}]";
                    StageConfigPanelEditorUi.PropertyField(round, header);
                }
            }
        }

        private static void DrawTabHeader(string title, string description)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(description, MessageType.None);
        }

        private static void DrawEmptyState(int visibleCount, string message)
        {
            if (visibleCount == 0)
            {
                EditorGUILayout.HelpBox(message, MessageType.Info);
            }
        }

        private static void ApplyChanges(SerializedObject serialized, UnityEngine.Object asset)
        {
            if (serialized.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(asset);
            }
        }

        private static void DrawSaveButton(string label)
        {
            EditorGUILayout.Space(6f);
            if (GUILayout.Button(label, GUILayout.Height(30f)))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void Reload()
        {
            catalog = DataCatalog.Collect();
            validationResults = null;
            assetFoldouts.Clear();
        }
    }
}
#endif
