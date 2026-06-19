#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.V02.Config;
using TalismanBag.V02.EnemySkills;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools
{
    public sealed class TalismanEnemyBalancePanel : EditorWindow
    {
        private const string SearchRoot = "Assets/_Game";

        private readonly List<EnemyRow> rows = new();
        private Vector2 scrollPosition;
        private string searchText = string.Empty;
        private bool showLegacySpecialFields = true;
        private bool showV02Fields = true;
        private bool showSkillDetails = true;
        private bool showVisualFields;
        private bool showDebugEnemies;
        private bool showDeprecatedEnemies;
        private bool showLegacyEnemies;

        [MenuItem("Tools/Talisman Bag/Enemy Identity Panel")]
        public static void Open()
        {
            TalismanEnemyBalancePanel window = GetWindow<TalismanEnemyBalancePanel>("Enemy Identity / 怪物识别");
            window.minSize = new Vector2(980f, 620f);
            window.LoadEnemies();
            window.Show();
        }

        private void OnEnable()
        {
            LoadEnemies();
        }

        private void OnProjectChange()
        {
            LoadEnemies();
            Repaint();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawNotice();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            int visibleCount = 0;
            foreach (EnemyRow row in rows)
            {
                if (row.Definition == null ||
                    !ShouldShow(row.Definition) ||
                    !MatchesSearch(row.Definition))
                {
                    continue;
                }

                visibleCount++;
                DrawEnemy(row);
            }

            if (visibleCount == 0)
            {
                EditorGUILayout.HelpBox("No enemy assets matched the current filter. / 当前筛选没有匹配的怪物资产。", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Enemy Identity Panel / 怪物识别数据面板", EditorStyles.boldLabel, GUILayout.Width(320f));

            GUILayout.Label("Search / 搜索", GUILayout.Width(82f));
            searchText = GUILayout.TextField(searchText, EditorStyles.toolbarTextField, GUILayout.MinWidth(180f));

            if (GUILayout.Button("Reload / 重新扫描", EditorStyles.toolbarButton, GUILayout.Width(132f)))
            {
                LoadEnemies();
            }

            if (GUILayout.Button("Save All / 保存全部", EditorStyles.toolbarButton, GUILayout.Width(132f)))
            {
                SaveAll();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            showLegacySpecialFields = EditorGUILayout.ToggleLeft("Legacy Special / 旧特殊技能", showLegacySpecialFields, GUILayout.Width(190f));
            showV02Fields = EditorGUILayout.ToggleLeft("V0.2 Fields / V0.2 字段", showV02Fields, GUILayout.Width(180f));
            showSkillDetails = EditorGUILayout.ToggleLeft("Inline Skills / 内联技能", showSkillDetails, GUILayout.Width(170f));
            showVisualFields = EditorGUILayout.ToggleLeft("Visual / 视觉字段", showVisualFields, GUILayout.Width(140f));
            showDebugEnemies = EditorGUILayout.ToggleLeft("Debug", showDebugEnemies, GUILayout.Width(80f));
            showDeprecatedEnemies = EditorGUILayout.ToggleLeft("Deprecated", showDeprecatedEnemies, GUILayout.Width(110f));
            showLegacyEnemies = EditorGUILayout.ToggleLeft("Legacy", showLegacyEnemies, GUILayout.Width(85f));
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Loaded / 已载入: {rows.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawNotice()
        {
            EditorGUILayout.HelpBox(
                "This editor writes EnemyDefinition assets only. Runtime boss phase logic and some combat behaviors may still be controlled by code. / 本面板只修改怪物资产；Boss 阶段逻辑和部分战斗行为仍可能由代码控制。",
                MessageType.Warning);
            EditorGUILayout.HelpBox(
                "V0.2 naming rule: Threat Name is the combat-readable enemy name, usually one Chinese character. Avatar Glyph follows the first character of Threat Name. Examples: 盾, 毒, 召, 封, 将. / V0.2 命名规则：怪物名先服务战斗识别，头像字跟随识别名首字。",
                MessageType.Info);
        }

        private void DrawEnemy(EnemyRow row)
        {
            EnemyDefinition definition = row.Definition;
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
                DrawProperty(serializedObject, "displayName", "Threat Name / 怪物识别名");
                DrawAvatarGlyphPreview(definition);
                DrawProperty(serializedObject, "enabled", "Enabled / 启用");
                DrawProperty(serializedObject, "enemyId", "Enemy ID / 怪物 ID");
                DrawProperty(serializedObject, "enemyType", "Enemy Type / 怪物类型");

                DrawSection("Combat Numbers / 战斗数值");
                DrawProperty(serializedObject, "maxHp", "Max HP / 最大气血");
                DrawProperty(serializedObject, "attackDamage", "Attack Damage / 攻击伤害");
                DrawProperty(serializedObject, "attackInterval", "Attack Interval / 攻击间隔");
                DrawProperty(serializedObject, "baseShield", "Base Shield / 基础护盾");

                DrawSection("Player-Facing Notes / 玩家提示文本");
                DrawProperty(serializedObject, "weaknessText", "Weakness Text / 弱点提示");
                DrawProperty(serializedObject, "dangerText", "Danger Text / 危险提示");
                DrawProperty(serializedObject, "recommendedBuildText", "Recommended Build / 推荐构筑");

                if (showLegacySpecialFields)
                {
                    DrawSection("Legacy Special Timers / 旧特殊行为");
                    DrawProperty(serializedObject, "chargeInterval", "Charge Interval / 蓄力间隔");
                    DrawProperty(serializedObject, "chargeDuration", "Charge Duration / 蓄力时长");
                    DrawProperty(serializedObject, "chargeAttackDamage", "Charge Damage / 蓄力伤害");
                    DrawProperty(serializedObject, "manaDrainInterval", "Mana Drain Interval / 偷灵间隔");
                    DrawProperty(serializedObject, "manaDrainAmount", "Mana Drain Amount / 偷灵数值");
                    DrawProperty(serializedObject, "sealInterval", "Seal Interval / 封印间隔");
                    DrawProperty(serializedObject, "sealDuration", "Seal Duration / 封印时长");
                    DrawProperty(serializedObject, "ghostShadowInterval", "Ghost Shadow Interval / 鬼影间隔");
                    DrawProperty(serializedObject, "ghostShadowDamage", "Ghost Shadow Damage / 鬼影伤害");
                }

                if (showV02Fields)
                {
                    DrawSection("V0.2 Identity & Intent / V0.2 身份与意图");
                    DrawProperty(serializedObject, "enemyClass", "Enemy Class / 怪物职能");
                    DrawProperty(serializedObject, "enemyArchetype", "Enemy Archetype / 怪物原型");
                    DrawProperty(serializedObject, "elementType", "Element Type / 元素（预留）");
                    DrawProperty(serializedObject, "factionType", "Faction Type / 阵营（预留）");
                    DrawProperty(serializedObject, "mechanicTags", "Mechanic Tags / 机制标签");
                    DrawProperty(serializedObject, "dropTableId", "Drop Table ID / 掉落表 ID");
                    DrawProperty(serializedObject, "sourceType", "Source Type / 数据来源");
                    DrawProperty(serializedObject, "isDebugOnly", "Debug Only / 仅调试");
                    DrawProperty(serializedObject, "isDeprecated", "Deprecated / 已废弃");
                    DrawProperty(serializedObject, "intentText", "Intent Text / 意图说明");
                    DrawProperty(serializedObject, "recommendedCounterText", "Recommended Counter / 推荐克制");

                    DrawSection("V0.2 Weakness & Resistance / V0.2 弱点与抗性");
                    DrawProperty(serializedObject, "weaknessTags", "Weakness Tags / 弱点标签");
                    DrawProperty(serializedObject, "resistTags", "Resist Tags / 抗性标签");
                    DrawProperty(serializedObject, "resistedElements", "Resisted Elements / 抵抗元素");
                    DrawProperty(serializedObject, "resistedFunctions", "Resisted Functions / 抵抗功能");
                    DrawProperty(serializedObject, "vulnerableElements", "Vulnerable Elements / 易伤元素");
                    DrawProperty(serializedObject, "vulnerableFunctions", "Vulnerable Functions / 易伤功能");

                    DrawSection("V0.2 Skills / V0.2 技能列表");
                    DrawProperty(serializedObject, "skills", "Skill Assets / 技能资产");
                    if (showSkillDetails)
                    {
                        DrawSkillDetails(definition);
                    }
                }

                if (showVisualFields)
                {
                    DrawSection("Visual / 视觉");
                    DrawProperty(serializedObject, "icon", "Icon Sprite / 头像图片（可选）");
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

        private void LoadEnemies()
        {
            rows.Clear();
            string[] guids = AssetDatabase.FindAssets("t:EnemyDefinition", new[] { SearchRoot });
            Array.Sort(guids, CompareEnemyGuids);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyDefinition definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
                if (definition != null)
                {
                    rows.Add(new EnemyRow(definition));
                }
            }
        }

        private void SaveAll()
        {
            foreach (EnemyRow row in rows)
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
            ShowNotification(new GUIContent("Saved all enemy assets / 已保存全部怪物资产"));
        }

        private bool MatchesSearch(EnemyDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string needle = searchText.Trim();
            return Contains(definition.name, needle) ||
                   Contains(definition.enemyId, needle) ||
                   Contains(definition.displayName, needle) ||
                   Contains(definition.GetAvatarGlyph(), needle) ||
                   Contains(definition.enemyType.ToString(), needle) ||
                   Contains(definition.enemyClass, needle) ||
                   Contains(definition.enemyArchetype, needle) ||
                   Contains(definition.intentText, needle) ||
                   Contains(definition.recommendedCounterText, needle) ||
                   Contains(definition.enabled ? "enabled 启用" : "disabled 停用", needle) ||
                   ContainsSkill(definition, needle);
        }

        private bool ShouldShow(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            if (!showDebugEnemies && (definition.isDebugOnly || definition.sourceType is CatalogSourceType.Debug or CatalogSourceType.Test))
            {
                return false;
            }

            if (!showDeprecatedEnemies && definition.isDeprecated)
            {
                return false;
            }

            return showLegacyEnemies || definition.sourceType != CatalogSourceType.Legacy;
        }

        private static int CompareEnemyGuids(string leftGuid, string rightGuid)
        {
            EnemyDefinition left = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(leftGuid));
            EnemyDefinition right = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(rightGuid));
            return string.Compare(BuildHeader(left), BuildHeader(right), StringComparison.OrdinalIgnoreCase);
        }

        private static bool Contains(string value, string needle)
        {
            return !string.IsNullOrEmpty(value) &&
                   value.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string BuildHeader(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return "Missing Enemy / 缺失怪物";
            }

            string displayName = definition.GetDisplayName();
            string enemyId = string.IsNullOrWhiteSpace(definition.enemyId) ? "no_id" : definition.enemyId;
            return $"【{definition.GetAvatarGlyph()}】{displayName} [{enemyId}]";
        }

        private static void DrawAvatarGlyphPreview(EnemyDefinition definition)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Avatar Glyph / 头像字", definition != null ? definition.GetAvatarGlyph() : "?");
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawSection(string label)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private void DrawSkillDetails(EnemyDefinition definition)
        {
            if (definition?.skills == null || definition.skills.Count == 0)
            {
                EditorGUILayout.HelpBox("No skill assets linked. / 当前怪物没有挂载技能资产。", MessageType.Info);
                return;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < definition.skills.Count; i++)
            {
                EnemySkillDefinition skill = definition.skills[i];
                if (skill == null)
                {
                    EditorGUILayout.HelpBox($"Skill slot {i + 1} is empty. / 第 {i + 1} 个技能槽为空。", MessageType.Warning);
                    continue;
                }

                SerializedObject skillObject = new(skill);
                skillObject.Update();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}. {BuildSkillHeader(skill)}", EditorStyles.boldLabel);
                if (GUILayout.Button("Select Skill / 选中技能", GUILayout.Width(150f)))
                {
                    Selection.activeObject = skill;
                    EditorGUIUtility.PingObject(skill);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                DrawProperty(skillObject, "displayName", "Display Name / 显示名称");
                DrawProperty(skillObject, "skillId", "Skill ID / 技能 ID");
                DrawProperty(skillObject, "skillType", "Skill Type / 技能类型");
                DrawProperty(skillObject, "initialDelay", "First Cast Delay / 首次释放时间");
                DrawProperty(skillObject, "cooldown", "Cooldown / 技能冷却");
                DrawProperty(skillObject, "castTime", "Cast Time / 施法时间");
                DrawProperty(skillObject, "value", "Value / 技能强度");
                DrawProperty(skillObject, "duration", "Duration / 持续时间");
                DrawProperty(skillObject, "skillTags", "Skill Tags / 技能标签");
                DrawProperty(skillObject, "intentText", "Intent Text / 意图说明");
                DrawProperty(skillObject, "effectDescription", "Effect Description / 效果说明");

                if (EditorGUI.EndChangeCheck())
                {
                    skillObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(skill);
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUI.indentLevel--;
        }

        private static string BuildSkillHeader(EnemySkillDefinition skill)
        {
            if (skill == null)
            {
                return "Missing Skill / 技能缺失";
            }

            string displayName = string.IsNullOrWhiteSpace(skill.displayName) ? skill.name : skill.displayName;
            string skillId = string.IsNullOrWhiteSpace(skill.skillId) ? "no_id" : skill.skillId;
            return $"{displayName} / {skillId}";
        }

        private static bool ContainsSkill(EnemyDefinition definition, string needle)
        {
            if (definition?.skills == null)
            {
                return false;
            }

            foreach (EnemySkillDefinition skill in definition.skills)
            {
                if (skill == null)
                {
                    continue;
                }

                if (Contains(skill.name, needle) ||
                    Contains(skill.skillId, needle) ||
                    Contains(skill.displayName, needle) ||
                    Contains(skill.skillType.ToString(), needle) ||
                    Contains(skill.intentText, needle) ||
                    Contains(skill.effectDescription, needle))
                {
                    return true;
                }
            }

            return false;
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

        private sealed class EnemyRow
        {
            public EnemyRow(EnemyDefinition definition)
            {
                Definition = definition;
                SerializedObject = new SerializedObject(definition);
            }

            public EnemyDefinition Definition { get; }
            public SerializedObject SerializedObject { get; }
            public bool Expanded { get; set; }
        }
    }
}
#endif
