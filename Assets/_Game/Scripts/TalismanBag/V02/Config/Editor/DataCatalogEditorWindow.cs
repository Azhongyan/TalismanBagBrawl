#if UNITY_EDITOR
using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Upgrades;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public sealed class DataCatalogEditorWindow : EditorWindow
    {
        private DataCatalog catalog;
        private IReadOnlyList<DataCatalogValidationResult> validationResults;
        private int selectedTab;
        private Vector2 scroll;
        private bool showInfo;

        [MenuItem("Tools/Talisman Bag/Stage Config Panel 01")]
        public static void Open()
        {
            DataCatalogEditorWindow window = GetWindow<DataCatalogEditorWindow>("Stage Config Panel 01");
            window.minSize = new Vector2(920f, 620f);
            window.Reload();
            window.Show();
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
            selectedTab = GUILayout.Toolbar(selectedTab, new[]
            {
                "Validate / 校验",
                "Stage / 关卡",
                "Enemy Group / 怪物组",
                "Reward / 奖励",
                "Drop / 掉落",
                "Boss",
                "Upgrade / 培养"
            });

            scroll = EditorGUILayout.BeginScrollView(scroll);
            switch (selectedTab)
            {
                case 0:
                    DrawValidation();
                    break;
                case 1:
                    DrawStages();
                    break;
                case 2:
                    DrawEnemyGroups();
                    break;
                case 3:
                    DrawRewards();
                    break;
                case 4:
                    DrawDrops();
                    break;
                case 5:
                    DrawBosses();
                    break;
                default:
                    DrawUpgrades();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("StageConfigPanel01 / 数据配置入口", EditorStyles.boldLabel, GUILayout.Width(260f));
                if (GUILayout.Button("Reload / 重新扫描", EditorStyles.toolbarButton, GUILayout.Width(140f)))
                {
                    Reload();
                }

                if (GUILayout.Button("Validate Data Catalog", EditorStyles.toolbarButton, GUILayout.Width(180f)))
                {
                    validationResults = DataCatalogValidator.Validate(catalog);
                    selectedTab = 0;
                }

                if (GUILayout.Button("Item Panel / 道具", EditorStyles.toolbarButton, GUILayout.Width(120f)))
                {
                    TalismanBag.EditorTools.TalismanItemBalancePanel.Open();
                }

                if (GUILayout.Button("Enemy Panel / 怪物", EditorStyles.toolbarButton, GUILayout.Width(130f)))
                {
                    TalismanBag.EditorTools.TalismanEnemyBalancePanel.Open();
                }
            }

            EditorGUILayout.HelpBox(
                "配置面板只编辑配置与显示校验结果；不会发奖励、修改 SaveData、跳关或控制正式主线。",
                MessageType.Info);
        }

        private void DrawValidation()
        {
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

            EditorGUILayout.LabelField($"Error: {errors}    Warning: {warnings}    Info: {infos}", EditorStyles.boldLabel);
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
                    if (result.Context != null && GUILayout.Button("Select Asset / 定位资产", GUILayout.Width(150f)))
                    {
                        Selection.activeObject = result.Context;
                        EditorGUIUtility.PingObject(result.Context);
                    }
                }
            }
        }

        private void DrawStages()
        {
            foreach (V02RunConfig runConfig in catalog.RunConfigs)
            {
                if (runConfig == null)
                {
                    continue;
                }

                SerializedObject serialized = new(runConfig);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.ObjectField("Run Config / 流程配置", runConfig, typeof(V02RunConfig), false);
                    DrawStageCollection(serialized.FindProperty("rounds"), "Chapter 1 / 第一章");
                    DrawStageCollection(serialized.FindProperty("chapterTwoRounds"), "Chapter 2 / 第二章");
                    if (serialized.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(runConfig);
                    }

                    if (GUILayout.Button("Save Stage Config / 保存关卡配置", GUILayout.Height(28f)))
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        private static void DrawStageCollection(SerializedProperty rounds, string title)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField($"{title} ({rounds?.arraySize ?? 0})", EditorStyles.boldLabel);
            if (rounds == null)
            {
                EditorGUILayout.HelpBox("Missing stage collection.", MessageType.Error);
                return;
            }

            for (int i = 0; i < rounds.arraySize; i++)
            {
                SerializedProperty round = rounds.GetArrayElementAtIndex(i);
                string stageId = round.FindPropertyRelative("levelId")?.stringValue;
                string displayName = round.FindPropertyRelative("roundTitle")?.stringValue;
                EditorGUILayout.PropertyField(round, new GUIContent($"{displayName} [{stageId}]"), true);
            }
        }

        private void DrawEnemyGroups()
        {
            foreach (EnemyGroupConfig group in catalog.EnemyGroups)
            {
                if (group == null || group.isDebugOnly || group.isDeprecated)
                {
                    continue;
                }

                SerializedObject serialized = new(group);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(group.GetCatalogLabel(), EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Asset / 资产", group, typeof(EnemyGroupConfig), false);
                    DrawAllProperties(serialized);
                    if (serialized.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(group);
                    }
                }
            }

            DrawSaveButton("Save Enemy Groups / 保存怪物组");
        }

        private void DrawRewards()
        {
            foreach (RewardConfig reward in catalog.Rewards)
            {
                if (reward == null || reward.isDebugOnly || reward.isDeprecated)
                {
                    continue;
                }

                SerializedObject serialized = new(reward);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(reward.GetCatalogLabel(), EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Asset / 资产", reward, typeof(RewardConfig), false);
                    DrawAllProperties(serialized);
                    if (serialized.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(reward);
                    }
                }
            }

            DrawSaveButton("Save Rewards / 保存奖励配置");
        }

        private void DrawDrops()
        {
            foreach (RewardDropTable table in catalog.DropTables)
            {
                if (table == null || table.isDebugOnly || table.isDeprecated)
                {
                    continue;
                }

                SerializedObject serialized = new(table);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(table.GetCatalogLabel(), EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Drop Table / 掉落表", table, typeof(RewardDropTable), false);
                    DrawAllProperties(serialized);
                    if (serialized.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(table);
                    }
                }
            }

            foreach (IdleDropConfig config in catalog.IdleDropConfigs)
            {
                if (config == null || config.isDebugOnly || config.isDeprecated)
                {
                    continue;
                }

                SerializedObject serialized = new(config);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(config.GetCatalogLabel(), EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Idle Drop Config / 巡行掉落配置", config, typeof(IdleDropConfig), false);
                    DrawAllProperties(serialized);
                    if (serialized.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(config);
                    }
                }
            }

            DrawSaveButton("Save Drop Configs / 保存掉落配置");
        }

        private void DrawBosses()
        {
            foreach (BossInfoConfig config in catalog.BossConfigs)
            {
                if (config == null || config.isDebugOnly || config.isDeprecated)
                {
                    continue;
                }

                SerializedObject serialized = new(config);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(config.GetCatalogLabel(), EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Boss Config", config, typeof(BossInfoConfig), false);
                    DrawAllProperties(serialized);
                    if (serialized.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(config);
                    }
                }
            }

            DrawSaveButton("Save Boss Configs / 保存 Boss 配置");
        }

        private void DrawUpgrades()
        {
            foreach (TalismanUpgradeConfig config in catalog.UpgradeConfigs)
            {
                if (config == null || config.isDebugOnly || config.isDeprecated)
                {
                    continue;
                }

                SerializedObject serialized = new(config);
                serialized.Update();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(config.GetCatalogLabel(), EditorStyles.boldLabel);
                    EditorGUILayout.ObjectField("Upgrade Config / 培养配置", config, typeof(TalismanUpgradeConfig), false);
                    DrawAllProperties(serialized);
                    if (serialized.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(config);
                    }
                }
            }

            DrawSaveButton("Save Upgrade Configs / 保存培养配置");
        }

        private static void DrawAllProperties(SerializedObject serialized)
        {
            SerializedProperty iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }
        }

        private static void DrawSaveButton(string label)
        {
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
        }
    }
}
#endif
