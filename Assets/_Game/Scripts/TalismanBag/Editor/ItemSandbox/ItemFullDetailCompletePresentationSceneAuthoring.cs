using System;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Items.Detail.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.EditorTools.ItemSandbox
{
    /// <summary>
    /// Manual-only scene-copy authoring. It never edits the formal Item detail prefab.
    /// </summary>
    public static class ItemFullDetailCompletePresentationSceneAuthoring
    {
        public const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        public const string ThemeAssetPath = "Assets/_Game/Configs/ItemDetail/ItemDetailVisualTheme.asset";
        public const string StructureReportPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationSceneStructureReport.md";

        [MenuItem("TalismanBag/V0.4/Item Sandbox/Manual Only/Bind Complete Presentation Theme")]
        public static void ApplyManualOnly()
        {
            ApplyForBatch();
        }

        public static void ApplyForBatch()
        {
            EnsureFolder("Assets/_Game/Configs/ItemDetail");
            ItemDetailVisualTheme theme = AssetDatabase.LoadAssetAtPath<ItemDetailVisualTheme>(ThemeAssetPath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ItemDetailVisualTheme>();
                AssetDatabase.CreateAsset(theme, ThemeAssetPath);
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ItemDetailPanelView[] panels = Resources.FindObjectsOfTypeAll<ItemDetailPanelView>()
                .Where(value => value != null && value.gameObject.scene == scene)
                .ToArray();
            if (panels.Length == 0)
            {
                throw new InvalidOperationException("Item detail panel was not found in the ItemSandbox scene copy.");
            }

            string[] before = panels.Select(DescribePanel).ToArray();

            foreach (ItemDetailPanelView panel in panels)
            {
                panel.ConfigureThemeEditor(theme);
                EditorUtility.SetDirty(panel);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Directory.CreateDirectory(Path.GetDirectoryName(StructureReportPath) ?? "Docs/V0.4/Reports");
            StringBuilder report = new();
            report.AppendLine("# ItemFullDetailCompletePresentation01 Scene Structure").AppendLine();
            report.AppendLine("- Authoring mode: Manual Only");
            report.AppendLine("- Scene: `" + ScenePath + "`");
            report.AppendLine("- Formal prefab modification: NONE").AppendLine();
            report.AppendLine("## Before binding").AppendLine();
            foreach (string line in before) report.AppendLine("- " + line);
            report.AppendLine().AppendLine("## After binding").AppendLine();
            foreach (ItemDetailPanelView panel in panels) report.AppendLine("- " + DescribePanel(panel));
            File.WriteAllText(StructureReportPath, report.ToString(), new UTF8Encoding(false));
            Debug.Log($"ITEM_FULL_DETAIL_COMPLETE_PRESENTATION01_AUTHORING_OK panels={panels.Length} theme={ThemeAssetPath}");
        }

        private static string DescribePanel(ItemDetailPanelView panel)
        {
            SerializedObject serialized = new(panel);
            int playerCount = serialized.FindProperty("playerSections")?.arraySize ?? 0;
            int debugCount = serialized.FindProperty("debugSections")?.arraySize ?? 0;
            string theme = serialized.FindProperty("visualTheme")?.objectReferenceValue == null ? "None" : ThemeAssetPath;
            string prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(panel.gameObject);
            return $"{GetPath(panel.transform)} | playerSections={playerCount} | debugSections={debugCount} | theme={theme} | prefabSource={(string.IsNullOrWhiteSpace(prefab) ? "None" : prefab)}";
        }

        private static string GetPath(Transform target)
        {
            string path = target.name;
            while (target.parent != null)
            {
                target = target.parent;
                path = target.name + "/" + path;
            }
            return path;
        }

        private static void EnsureFolder(string path)
        {
            string normalized = path.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }

            string parent = Path.GetDirectoryName(normalized)?.Replace('\\', '/');
            string name = Path.GetFileName(normalized);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Invalid asset folder path: " + path);
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
