#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class StageConfigPanelEditorUi
    {
        public const float LabelWidth = 220f;

        public static void PropertyField(SerializedProperty property, string label = null)
        {
            if (property == null)
            {
                return;
            }

            GUIContent content = new(
                string.IsNullOrWhiteSpace(label) ? property.displayName : label,
                property.tooltip);

            if (IsComplex(property))
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, content, true);
                if (!property.isExpanded)
                {
                    return;
                }

                using (new EditorGUI.IndentLevelScope())
                {
                    DrawVisibleChildren(property);
                }

                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(content, GUILayout.Width(LabelWidth));
                EditorGUILayout.PropertyField(
                    property,
                    GUIContent.none,
                    true,
                    GUILayout.MinWidth(80f),
                    GUILayout.ExpandWidth(true));
            }
        }

        public static void ObjectField(string label, Object value, System.Type objectType)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(LabelWidth));
                EditorGUILayout.ObjectField(
                    value,
                    objectType,
                    false,
                    GUILayout.MinWidth(80f),
                    GUILayout.ExpandWidth(true));
            }
        }

        public static void ReadOnlyTextField(string label, string value)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(LabelWidth));
                EditorGUILayout.TextField(value ?? string.Empty, GUILayout.ExpandWidth(true));
            }
        }

        public static void ReadOnlyLabel(string label, string value, GUIStyle style = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(LabelWidth));
                EditorGUILayout.LabelField(
                    value ?? string.Empty,
                    style ?? EditorStyles.label,
                    GUILayout.ExpandWidth(true));
            }
        }

        public static void DrawAllProperties(SerializedObject serializedObject)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        PropertyField(iterator);
                    }
                }
                else
                {
                    PropertyField(iterator);
                }
            }
        }

        private static bool IsComplex(SerializedProperty property)
        {
            return (property.isArray && property.propertyType != SerializedPropertyType.String) ||
                   property.propertyType == SerializedPropertyType.Generic;
        }

        private static void DrawVisibleChildren(SerializedProperty parent)
        {
            SerializedProperty child = parent.Copy();
            SerializedProperty end = child.GetEndProperty();
            bool enterChildren = true;
            while (child.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                PropertyField(child);
            }
        }
    }
}
#endif
