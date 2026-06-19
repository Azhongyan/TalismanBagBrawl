#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TalismanBag.Items;
using TalismanBag.V02.Config;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools
{
    public sealed class TalismanItemBalancePanel : EditorWindow
    {
        private const string SearchRoot = "Assets/_Game";

        private readonly List<ItemRow> rows = new();
        private Vector2 scrollPosition;
        private string searchText = string.Empty;
        private bool showVisualFields;
        private bool showTechnicalFields = true;
        private bool showDescriptionFields = true;
        private bool showDebugItems;
        private bool showDeprecatedItems;
        private bool showLegacyItems;

        [MenuItem("Tools/Talisman Bag/Item Balance Panel")]
        public static void Open()
        {
            TalismanItemBalancePanel window = GetWindow<TalismanItemBalancePanel>("Item Balance / 物品总控");
            window.minSize = new Vector2(980f, 620f);
            window.LoadItems();
            window.Show();
        }

        private void OnEnable()
        {
            LoadItems();
        }

        private void OnProjectChange()
        {
            LoadItems();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawNotice();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            int visibleCount = 0;
            foreach (ItemRow row in rows)
            {
                if (row.Definition == null ||
                    !ShouldShow(row.Definition) ||
                    !MatchesSearch(row.Definition))
                {
                    continue;
                }

                visibleCount++;
                DrawItem(row);
            }

            if (visibleCount == 0)
            {
                EditorGUILayout.HelpBox("No talisman assets matched the current filter. / 当前筛选没有匹配的符箓资产。", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Item Balance Panel / 物品属性总控面板", EditorStyles.boldLabel, GUILayout.Width(280f));

            GUILayout.Label("Search / 搜索", GUILayout.Width(82f));
            searchText = GUILayout.TextField(searchText, EditorStyles.toolbarTextField, GUILayout.MinWidth(180f));

            if (GUILayout.Button("Reload / 重新扫描", EditorStyles.toolbarButton, GUILayout.Width(132f)))
            {
                LoadItems();
            }

            if (GUILayout.Button("Save All / 保存全部", EditorStyles.toolbarButton, GUILayout.Width(132f)))
            {
                SaveAll();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            showTechnicalFields = EditorGUILayout.ToggleLeft("Technical / 技术字段", showTechnicalFields, GUILayout.Width(170f));
            showVisualFields = EditorGUILayout.ToggleLeft("Visual / 视觉字段", showVisualFields, GUILayout.Width(140f));
            showDescriptionFields = EditorGUILayout.ToggleLeft("Description / 描述字段", showDescriptionFields, GUILayout.Width(170f));
            showDebugItems = EditorGUILayout.ToggleLeft("Debug", showDebugItems, GUILayout.Width(80f));
            showDeprecatedItems = EditorGUILayout.ToggleLeft("Deprecated", showDeprecatedItems, GUILayout.Width(110f));
            showLegacyItems = EditorGUILayout.ToggleLeft("Legacy", showLegacyItems, GUILayout.Width(85f));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Loaded / 已载入: {rows.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawNotice()
        {
            EditorGUILayout.HelpBox(
                "This editor writes TalismanItemDefinition assets only. Some V0.2 combat formulas are still hard-coded in AutoCombatController and tooltip display code. / 本面板只修改符箓资产；部分 V0.2 战斗公式和弹窗展示数值仍在代码里硬写，需要后续再统一数据化。",
                MessageType.Warning);
        }

        private void DrawItem(ItemRow row)
        {
            TalismanItemDefinition definition = row.Definition;
            SerializedObject serializedObject = row.SerializedObject;
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            row.Expanded = EditorGUILayout.Foldout(row.Expanded, BuildHeader(definition), true);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Select / 选中", GUILayout.Width(96f)))
            {
                Selection.activeObject = definition;
                EditorGUIUtility.PingObject(definition);
            }

            if (GUILayout.Button("Duplicate Config / 复用机制", GUILayout.Width(150f)))
            {
                DuplicateConfig(definition);
            }

            if (GUILayout.Button("Save / 保存", GUILayout.Width(88f)))
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(definition);
                AssetDatabase.SaveAssets();
                ShowNotification(new GUIContent("Saved / 已保存"));
            }

            EditorGUILayout.EndHorizontal();

            if (row.Expanded)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();

                DrawSection("Identity / 基础信息");
                DrawProperty(serializedObject, "displayName", "Display Name / 显示名称");
                DrawProperty(serializedObject, "enabled", "Enabled / 启用");
                if (showTechnicalFields)
                {
                    DrawProperty(serializedObject, "itemId", "Item ID / 物品 ID");
                    DrawProperty(serializedObject, "itemType", "Item Type / 物品类型");
                    DrawProperty(serializedObject, "width", "Width / 宽度");
                    DrawProperty(serializedObject, "height", "Height / 高度");
                    DrawProperty(serializedObject, "rarity", "Rarity / 稀有度");
                    DrawProperty(serializedObject, "iconKey", "Icon Key / 图标键");
                    DrawProperty(serializedObject, "canDrop", "Can Drop / 可掉落");
                    DrawProperty(serializedObject, "canUpgrade", "Can Upgrade / 可培养");
                    DrawProperty(serializedObject, "unlockChapter", "Unlock Chapter / 解锁章节");
                    DrawProperty(serializedObject, "sourceType", "Source Type / 数据来源");
                    DrawProperty(serializedObject, "isDebugOnly", "Debug Only / 仅调试");
                    DrawProperty(serializedObject, "isDeprecated", "Deprecated / 已废弃");
                }

                DrawSection("Combat Numbers / 战斗数值");
                DrawProperty(serializedObject, "baseCooldown", "Base Cooldown / 基础冷却");
                DrawProperty(serializedObject, "manaCost", "Mana Cost / 灵气消耗");
                DrawProperty(serializedObject, "baseValue", "Base Value / 基础数值(伤害/护盾/治疗)");
                DrawItemValuePreview(definition);

                DrawSection("Tags & Effect / 标签与效果");
                DrawProperty(serializedObject, "elementType", "Legacy Element / 旧元素");
                DrawProperty(serializedObject, "elementTag", "Element Tag / 元素标签");
                DrawProperty(serializedObject, "functionTags", "Function Tags / 功能标签");
                DrawProperty(serializedObject, "counterTags", "Counter Tags / 克制标签");
                DrawProperty(serializedObject, "effectType", "Effect Type / 效果类型");
                DrawProperty(serializedObject, "effectId", "Effect ID / 效果 ID");
                DrawProperty(serializedObject, "effectParams", "Effect Params / 效果参数");
                DrawProperty(serializedObject, "mechanicTags", "Mechanic Tags / 机制标签");
                DrawProperty(serializedObject, "schoolTags", "School Tags / 流派标签");
                DrawProperty(serializedObject, "factionTags", "Faction Tags / 阵营标签（预留）");
                DrawProperty(serializedObject, "synergyTags", "Synergy Tags / 协同标签（预留）");
                DrawProperty(serializedObject, "comboTags", "Combo Tags / 连携标签（预留）");

                DrawSection("Formation Power / 阵法供能");
                DrawProperty(serializedObject, "requiresFormationPower", "Requires Power / 需要供能");
                DrawProperty(serializedObject, "energyRequired", "Energy Required / 供能需求");

                if (showVisualFields)
                {
                    DrawSection("Visual / 视觉");
                    DrawProperty(serializedObject, "icon", "Icon / 图标");
                    DrawProperty(serializedObject, "uiColor", "UI Color / UI 颜色");
                }

                if (showDescriptionFields)
                {
                    DrawSection("Descriptions / 描述");
                    DrawProperty(serializedObject, "description", "Description / 说明");
                    DrawProperty(serializedObject, "shortRoleDescription", "Short Role / 简短定位");
                    DrawProperty(serializedObject, "counterDescription", "Counter Note / 克制说明");
                }

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(definition);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void LoadItems()
        {
            rows.Clear();
            string[] guids = AssetDatabase.FindAssets("t:TalismanItemDefinition", new[] { SearchRoot });
            Array.Sort(guids, CompareItemGuids);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TalismanItemDefinition definition = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(path);
                if (definition != null)
                {
                    rows.Add(new ItemRow(definition));
                }
            }
        }

        private void SaveAll()
        {
            foreach (ItemRow row in rows)
            {
                if (row.Definition == null)
                {
                    continue;
                }

                row.SerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(row.Definition);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ShowNotification(new GUIContent("Saved all item assets / 已保存全部物品资产"));
        }

        private bool MatchesSearch(TalismanItemDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string needle = searchText.Trim();
            return Contains(definition.name, needle) ||
                   Contains(definition.itemId, needle) ||
                   Contains(definition.displayName, needle) ||
                   Contains(definition.itemType.ToString(), needle) ||
                   Contains(definition.effectType.ToString(), needle) ||
                   Contains(definition.elementTag.ToString(), needle) ||
                   Contains(definition.enabled ? "enabled 启用" : "disabled 停用", needle);
        }

        private bool ShouldShow(TalismanItemDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            if (!showDebugItems && (definition.isDebugOnly || definition.sourceType is CatalogSourceType.Debug or CatalogSourceType.Test))
            {
                return false;
            }

            if (!showDeprecatedItems && definition.isDeprecated)
            {
                return false;
            }

            return showLegacyItems || definition.sourceType != CatalogSourceType.Legacy;
        }

        private static int CompareItemGuids(string leftGuid, string rightGuid)
        {
            TalismanItemDefinition left = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(AssetDatabase.GUIDToAssetPath(leftGuid));
            TalismanItemDefinition right = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(AssetDatabase.GUIDToAssetPath(rightGuid));
            return string.Compare(BuildHeader(left), BuildHeader(right), StringComparison.OrdinalIgnoreCase);
        }

        private static bool Contains(string value, string needle)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string BuildHeader(TalismanItemDefinition definition)
        {
            if (definition == null)
            {
                return "Missing Item / 缺失物品";
            }

            string displayName = string.IsNullOrWhiteSpace(definition.displayName) ? definition.name : definition.displayName;
            string itemId = string.IsNullOrWhiteSpace(definition.itemId) ? "no_id" : definition.itemId;
            return $"{displayName} [{itemId}]";
        }

        private void DuplicateConfig(TalismanItemDefinition source)
        {
            if (source == null)
            {
                return;
            }

            string sourcePath = AssetDatabase.GetAssetPath(source);
            string directory = System.IO.Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
            string targetPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{source.name}_variant.asset");
            TalismanItemDefinition copy = Instantiate(source);
            copy.name = System.IO.Path.GetFileNameWithoutExtension(targetPath);
            copy.itemId = $"{source.itemId}_variant";
            copy.displayName = $"{source.displayName}变体";
            copy.isDebugOnly = true;
            copy.isDeprecated = false;
            copy.sourceType = CatalogSourceType.Debug;
            AssetDatabase.CreateAsset(copy, targetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = copy;
            EditorGUIUtility.PingObject(copy);
            LoadItems();
        }

        private static void DrawSection(string label)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private static void DrawItemValuePreview(TalismanItemDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            int levelTwoValue = Mathf.RoundToInt(definition.baseValue * 1.5f);
            EditorGUILayout.LabelField("Lv2 Value Preview / 二级数值预览", levelTwoValue.ToString(), EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Effect Hint / 效果提示", $"{definition.effectType} uses Base Value when runtime supports it.", EditorStyles.miniLabel);
        }

        private static void DrawProperty(SerializedObject serializedObject, string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox($"Missing property / 缺少字段: {propertyName}", MessageType.Error);
                return;
            }

            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
        }

        private sealed class ItemRow
        {
            public ItemRow(TalismanItemDefinition definition)
            {
                Definition = definition;
                SerializedObject = new SerializedObject(definition);
            }

            public TalismanItemDefinition Definition { get; }
            public SerializedObject SerializedObject { get; }
            public bool Expanded { get; set; }
        }
    }
}
#endif
