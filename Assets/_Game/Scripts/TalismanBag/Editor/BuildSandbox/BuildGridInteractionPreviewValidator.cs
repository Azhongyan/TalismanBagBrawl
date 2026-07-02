#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildGridInteractionPreviewValidator
    {
        public const string PackageName = BuildGridInteractionPreviewController.PackageName;

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "Boss六钥匙",
            "Boss 六钥匙"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                BattleSandboxPreviewSceneVerifier.Validate(),
                BuildSandboxPreviewContextValidator.Validate(),
                BattlePageViewAdapterValidator.Validate(),
                ValidateSceneBinding(),
                ValidateItemInfoPanelSamples(),
                ValidatePlacementSamples()
            };
        }

        public static BuildSandboxValidationReport ValidateSceneBinding()
        {
            BuildSandboxValidationReport report = new("BuildGrid Interaction Preview Scene Binding");
            BuildGridInteractionSceneBindingSnapshot snapshot = BuildSceneBindingSnapshot();

            if (!snapshot.SceneExists)
            {
                report.AddError(
                    "BUILD_GRID_PREVIEW_SCENE_MISSING",
                    "V04 Preview Scene is missing.",
                    BuildSandboxPreviewSceneMarker.ScenePath);
                return report;
            }

            ValidateTrue(report, "BUILD_GRID_PREVIEW_SCENE_BOUND", snapshot.ControllerPresent, "Grid interaction controller is bound.", "Controller missing.", BuildSandboxPreviewSceneMarker.ScenePath);
            ValidateTrue(report, "BUILD_GRID_BOARD_PRESENT", snapshot.BoardGridPresent, "BoardGridPreview exists.", "BoardGridPreview missing.", "BoardGridPreview");
            ValidateTrue(report, "BUILD_GRID_TRAY_PRESENT", snapshot.ItemTrayPresent, "ItemTrayPreview exists.", "ItemTrayPreview missing.", "ItemTrayPreview");
            ValidateTrue(report, "BUILD_GRID_INFO_PRESENT", snapshot.SelectedItemInfoPresent, "SelectedItemInfo exists.", "SelectedItemInfo missing.", "SelectedItemInfo");
            ValidateTrue(report, "BUILD_GRID_FEEDBACK_PRESENT", snapshot.PlacementFeedbackPresent, "PlacementFeedback exists.", "PlacementFeedback missing.", "PlacementFeedback");

            ValidateMinimum(report, "BUILD_GRID_BOARD_SLOT_COUNT", "Board slot", snapshot.BoardSlotCount, BuildGridInteractionPreviewController.BoardColumns * BuildGridInteractionPreviewController.BoardRows);
            ValidateMinimum(report, "BUILD_GRID_TRAY_SLOT_COUNT", "Tray slot", snapshot.TraySlotCount, BuildGridInteractionPreviewController.TrayColumns * BuildGridInteractionPreviewController.TrayRows);
            ValidateMinimum(report, "BUILD_GRID_CATEGORY_COUNT", "Category filter", snapshot.CategoryCount, BuildGridInteractionPreviewController.CategoryLabels.Length);
            ValidateMinimum(report, "BUILD_GRID_SHAPE_COUNT", "Shape", snapshot.ShapeCount, 4);

            if (!snapshot.ControllerDevOnly || snapshot.ControllerIsEnabled || snapshot.ControllerReadsFormalSave
                || snapshot.ControllerWritesFormalFlow || snapshot.ControllerWritesFormalUi || snapshot.ControllerTouchesFormalScene)
            {
                report.AddError(
                    "BUILD_GRID_CONTROLLER_ISOLATION_FAIL",
                    "Controller must stay devOnly=true, isEnabled=false, and formal surfaces=false.",
                    nameof(BuildGridInteractionPreviewController));
            }
            else
            {
                report.AddInfo(
                    "BUILD_GRID_CONTROLLER_ISOLATION_PASS",
                    "Controller is devOnly and isolated from formal flow/save/UI/scene surfaces.",
                    nameof(BuildGridInteractionPreviewController));
            }

            if (snapshot.ControllerShowsCompleteAnswers)
            {
                report.AddError(
                    "BUILD_GRID_COMPLETE_ANSWER_VISIBLE",
                    "Player-side grid interaction preview must not show complete answers.",
                    nameof(BuildGridInteractionPreviewController));
            }

            if (snapshot.BuildSettingsContainsPreview)
            {
                report.AddError(
                    "BUILD_GRID_PREVIEW_IN_BUILD_SETTINGS",
                    "V04 Preview Scene must not be added to Build Settings by this package.",
                    "ProjectSettings/EditorBuildSettings.asset");
            }
            else
            {
                report.AddInfo(
                    "BUILD_GRID_BUILDSETTINGS_UNTOUCHED",
                    "V04 Preview Scene is not present in Build Settings.",
                    "ProjectSettings/EditorBuildSettings.asset");
            }

            if (snapshot.PlayerTextLatinViolations.Count > 0)
            {
                foreach (string violation in snapshot.PlayerTextLatinViolations)
                {
                    report.AddError(
                        "BUILD_GRID_PLAYER_TEXT_NOT_CHINESE_ONLY",
                        "Player-facing text contains Latin letters: " + violation,
                        BuildSandboxPreviewSceneMarker.ScenePath);
                }
            }
            else
            {
                report.AddInfo(
                    "BUILD_GRID_PLAYER_TEXT_CHINESE_ONLY",
                    "Player-facing preview text is Chinese-only.",
                    BuildSandboxPreviewSceneMarker.ScenePath);
            }

            if (snapshot.ForbiddenAnswerTextViolations.Count > 0)
            {
                foreach (string violation in snapshot.ForbiddenAnswerTextViolations)
                {
                    report.AddError(
                        "BUILD_GRID_PLAYER_TEXT_FORBIDDEN_ANSWER",
                        "Player-facing text exposes forbidden answer token: " + violation,
                        BuildSandboxPreviewSceneMarker.ScenePath);
                }
            }
            else
            {
                report.AddInfo(
                    "BUILD_GRID_COMPLETE_ANSWERS_HIDDEN",
                    "Player-facing preview text does not expose complete answers.",
                    BuildSandboxPreviewSceneMarker.ScenePath);
            }

            return report;
        }

        public static BuildSandboxValidationReport ValidatePlacementSamples()
        {
            BuildSandboxValidationReport report = new("BuildGrid Interaction Preview Placement Samples");
            IReadOnlyList<BuildGridInteractionPlacementSample> samples = BuildPlacementSamples();

            ValidateTrue(report, "BUILD_GRID_SAMPLE_LEGAL", samples.Any(sample => sample.SampleId == "legal_place" && sample.IsValid), "Legal placement sample passes.", "Legal placement sample missing or failed.", PackageName);
            ValidateTrue(report, "BUILD_GRID_SAMPLE_ROTATION", samples.Any(sample => sample.SampleId == "rotation_place" && sample.IsValid), "Rotation sample passes.", "Rotation sample missing or failed.", PackageName);
            ValidateTrue(report, "BUILD_GRID_SAMPLE_OUT_OF_GRID", samples.Any(sample => sample.InvalidReason == ShapePlacementInvalidReason.OutOfGrid), "Out-of-grid sample is detected.", "Out-of-grid sample missing.", PackageName);
            ValidateTrue(report, "BUILD_GRID_SAMPLE_OVERLAP", samples.Any(sample => sample.InvalidReason == ShapePlacementInvalidReason.CellOccupied), "Overlap sample is detected.", "Overlap sample missing.", PackageName);

            foreach (BuildGridInteractionPlacementSample sample in samples)
            {
                report.AddInfo(
                    "BUILD_GRID_PLACEMENT_SAMPLE",
                    $"{sample.SampleId}: valid={sample.IsValid}, reason={sample.InvalidReason}, feedback={sample.FeedbackChinese}",
                    PackageName);
            }

            return report;
        }

        public static BuildSandboxValidationReport ValidateItemInfoPanelSamples()
        {
            BuildSandboxValidationReport report = new("BuildGrid Interaction ItemInfoPanel Samples");
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> items =
                BuildGridInteractionPreviewController.CreatePreviewItems();
            Dictionary<string, ItemShapeConfig> shapes = BuildGridInteractionPreviewController
                .CreatePreviewShapeConfigs()
                .ToDictionary(shape => shape.shapeId, StringComparer.Ordinal);

            string[] requiredLabels =
            {
                "名称：",
                "类型：",
                "品质：",
                "标签：",
                "基础效果：",
                "触发条件：",
                "冷却/频率：",
                "数值：",
                "词条：",
                "羁绊贡献：",
                "摆放形状：",
                "说明：",
                "多格形状：",
                "占用格子：",
                "旋转占格：",
                "词条预览：",
                "效果变动预览：",
                "事件预览：",
                "构筑贡献：",
                "配合线索："
            };

            ValidateTrue(
                report,
                "BUILD_GRID_ITEM_INFO_CLOSE_PRESENT",
                BuildSandboxItemInfoPanel.CloseButtonLabel == "×",
                "Sandbox ItemInfoPanel uses a non-Latin close mark.",
                "Sandbox ItemInfoPanel close mark is missing.",
                nameof(BuildSandboxItemInfoPanel));

            foreach (BuildGridInteractionPreviewController.PreviewItem item in items)
            {
                BuildGridInteractionItemInfoContext context = BuildItemInfoSampleContext(item, shapes);
                string body = BuildSandboxItemInfoPanel.BuildPlayerBodyText(item, context);
                foreach (string label in requiredLabels)
                {
                    if (body.IndexOf(label, StringComparison.Ordinal) < 0)
                    {
                        report.AddError(
                            "BUILD_GRID_ITEM_INFO_FIELD_MISSING",
                            $"ItemInfoPanel missing player field label: {label}",
                            item.DisplayName);
                    }
                }

                if (body.Any(character => character <= 127 && char.IsLetter(character)))
                {
                    report.AddError(
                        "BUILD_GRID_ITEM_INFO_LATIN_VISIBLE",
                        "ItemInfoPanel player text contains Latin letters.",
                        item.DisplayName);
                }

                foreach (string token in ForbiddenPlayerAnswerTokens)
                {
                    if (body.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "BUILD_GRID_ITEM_INFO_FORBIDDEN_ANSWER",
                            "ItemInfoPanel exposes forbidden answer token: " + token,
                            item.DisplayName);
                    }
                }

                report.AddInfo(
                    "BUILD_GRID_ITEM_INFO_SAMPLE",
                    $"ItemInfoPanel sample passes structure and masking checks: {item.DisplayName}.",
                    PackageName);
            }

            return report;
        }

        public static BuildGridInteractionSceneBindingSnapshot BuildSceneBindingSnapshot()
        {
            BuildGridInteractionSceneBindingSnapshot snapshot = new();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string sceneAbsolutePath = Path.Combine(projectRoot, BuildSandboxPreviewSceneMarker.ScenePath);
            snapshot.SceneExists = File.Exists(sceneAbsolutePath);
            snapshot.BuildSettingsContainsPreview = EditorBuildSettings.scenes.Any(scene =>
                scene != null && string.Equals(scene.path, BuildSandboxPreviewSceneMarker.ScenePath, StringComparison.Ordinal));

            if (!snapshot.SceneExists)
            {
                return snapshot;
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                Transform safeArea = FindDeepChildInScene(scene, "SafeAreaRoot");
                Transform battleArea = FindDeepChildInScene(scene, "BattleLikePreviewArea");
                Transform controlBar = FindDeepChildInScene(scene, "DevOnlyControlBar");
                snapshot.BoardGridPresent = FindDeepChildInScene(scene, "BoardGridPreview") != null;
                snapshot.ItemTrayPresent = FindDeepChildInScene(scene, "ItemTrayPreview") != null;
                snapshot.SelectedItemInfoPresent = FindDeepChildInScene(scene, "SelectedItemInfo") != null;
                snapshot.PlacementFeedbackPresent = FindDeepChildInScene(scene, "PlacementFeedback") != null;

                BuildGridInteractionPreviewController controller =
                    UnityEngine.Object.FindObjectOfType<BuildGridInteractionPreviewController>(true);
                snapshot.ControllerPresent = controller != null;
                if (controller != null)
                {
                    snapshot.ControllerDevOnly = controller.DevOnly;
                    snapshot.ControllerIsEnabled = controller.IsEnabled;
                    snapshot.ControllerReadsFormalSave = controller.ReadsFormalSaveData;
                    snapshot.ControllerWritesFormalFlow = controller.WritesFormalFlow;
                    snapshot.ControllerWritesFormalUi = controller.WritesFormalUi;
                    snapshot.ControllerTouchesFormalScene = controller.TouchesFormalScene;
                    snapshot.ControllerShowsCompleteAnswers = controller.ShowsCompleteAnswers;
                    snapshot.BoardSlotCount = controller.BoardSlotCount;
                    snapshot.TraySlotCount = controller.TraySlotCount;
                    snapshot.CategoryCount = controller.CategoryCount;
                    snapshot.ShapeCount = controller.ShapeCount;
                }

                if (battleArea != null)
                {
                    CollectPlayerTextViolations(battleArea, snapshot);
                }

                if (controlBar != null)
                {
                    CollectPlayerTextViolations(controlBar, snapshot);
                }

                snapshot.UiBindingRows = CollectUiBindingRows(scene);
            }
            finally
            {
                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != scene.path)
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }

            return snapshot;
        }

        public static IReadOnlyList<BuildGridInteractionPlacementSample> BuildPlacementSamples()
        {
            Dictionary<string, ItemShapeConfig> shapes = BuildGridInteractionPreviewController
                .CreatePreviewShapeConfigs()
                .ToDictionary(shape => shape.shapeId, StringComparer.Ordinal);
            List<BuildGridInteractionPlacementSample> samples = new();

            GridOccupancyMap legalMap = new(BuildGridInteractionPreviewController.BoardColumns, BuildGridInteractionPreviewController.BoardRows);
            ShapePlacementResult legal = legalMap.TryPlace(
                new ItemShapePlacementRequest("sample_legal", "Single1", new ItemShapeCell(0, 0)),
                shapes["Single1"]);
            samples.Add(ToSample("legal_place", "合法放置", legal));

            ShapePlacementResult rotation = ItemShapePlacementValidator.ValidatePlacement(
                new ItemShapePlacementRequest("sample_rotation", "Vertical2", new ItemShapeCell(1, 1), ItemShapeRotation.Rotation90),
                shapes["Vertical2"],
                new GridOccupancyMap(BuildGridInteractionPreviewController.BoardColumns, BuildGridInteractionPreviewController.BoardRows));
            samples.Add(ToSample("rotation_place", "旋转预览", rotation));

            ShapePlacementResult outOfGrid = ItemShapePlacementValidator.ValidatePlacement(
                new ItemShapePlacementRequest("sample_out", "Square4", new ItemShapeCell(4, 4)),
                shapes["Square4"],
                new GridOccupancyMap(BuildGridInteractionPreviewController.BoardColumns, BuildGridInteractionPreviewController.BoardRows));
            samples.Add(ToSample("out_of_grid", "越界检测", outOfGrid));

            GridOccupancyMap overlapMap = new(BuildGridInteractionPreviewController.BoardColumns, BuildGridInteractionPreviewController.BoardRows);
            _ = overlapMap.TryPlace(
                new ItemShapePlacementRequest("sample_blocker", "Single1", new ItemShapeCell(2, 2)),
                shapes["Single1"]);
            ShapePlacementResult overlap = ItemShapePlacementValidator.ValidatePlacement(
                new ItemShapePlacementRequest("sample_overlap", "Corner3", new ItemShapeCell(2, 2)),
                shapes["Corner3"],
                overlapMap);
            samples.Add(ToSample("overlap", "重叠检测", overlap));

            return samples;
        }

        private static BuildGridInteractionPlacementSample ToSample(
            string sampleId,
            string actionChinese,
            ShapePlacementResult result)
        {
            return new BuildGridInteractionPlacementSample
            {
                SampleId = sampleId,
                ActionChinese = actionChinese,
                ShapeId = result.ShapeId,
                Anchor = result.AnchorCell.ToString(),
                OccupiedCells = string.Join(";", result.OccupiedCells.Select(cell => cell.ToString())),
                IsValid = result.IsValid,
                InvalidReason = result.InvalidReason,
                FeedbackChinese = FormatFeedback(result)
            };
        }

        private static string FormatFeedback(ShapePlacementResult result)
        {
            if (result.IsValid)
            {
                return $"可以放置，占用 {result.OccupiedCells.Count} 格。";
            }

            return result.InvalidReason switch
            {
                ShapePlacementInvalidReason.OutOfGrid => "越界：有格子超出棋盘。",
                ShapePlacementInvalidReason.CellOccupied => "重叠：目标格子已被占用。",
                ShapePlacementInvalidReason.ShapeInvalid => "形状数据不可用。",
                ShapePlacementInvalidReason.MissingShapeConfig => "缺少形状配置。",
                _ => "当前位置不能放置。"
            };
        }

        private static BuildGridInteractionItemInfoContext BuildItemInfoSampleContext(
            BuildGridInteractionPreviewController.PreviewItem item,
            IReadOnlyDictionary<string, ItemShapeConfig> shapes)
        {
            if (item == null || shapes == null || !shapes.TryGetValue(item.ShapeId, out ItemShapeConfig shape))
            {
                return BuildGridInteractionItemInfoContext.Empty;
            }

            IReadOnlyList<ItemShapeCell> normalizedCells = NormalizeCells(
                ItemShapePlacementValidator.CalculateOccupiedCells(
                    shape,
                    new ItemShapeCell(0, 0),
                    item.Rotation));
            string cells = FormatChineseCells(normalizedCells);
            return new BuildGridInteractionItemInfoContext(
                $"{item.ShapeDisplayName}，共 {shape.cellCount} 格。",
                cells,
                $"正向：{cells}");
        }

        private static IReadOnlyList<ItemShapeCell> NormalizeCells(IReadOnlyList<ItemShapeCell> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return Array.Empty<ItemShapeCell>();
            }

            int minX = cells.Min(cell => cell.x);
            int minY = cells.Min(cell => cell.y);
            return cells
                .Select(cell => new ItemShapeCell(cell.x - minX, cell.y - minY))
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
        }

        private static string FormatChineseCells(IReadOnlyList<ItemShapeCell> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return "暂无占格";
            }

            return string.Join("、", cells.Select(cell => $"第{cell.x + 1}列第{cell.y + 1}行"));
        }

        private static void CollectPlayerTextViolations(
            Transform scope,
            BuildGridInteractionSceneBindingSnapshot snapshot)
        {
            foreach (Text text in scope.GetComponentsInChildren<Text>(true))
            {
                string value = text == null ? string.Empty : text.text;
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (value.Any(character => character <= 127 && char.IsLetter(character)))
                {
                    snapshot.PlayerTextLatinViolations.Add($"{BuildPath(text.transform)}=`{value}`");
                }

                foreach (string token in ForbiddenPlayerAnswerTokens)
                {
                    if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        snapshot.ForbiddenAnswerTextViolations.Add($"{BuildPath(text.transform)} contains {token}");
                    }
                }
            }
        }

        private static List<BuildGridInteractionUiBindingRow> CollectUiBindingRows(Scene scene)
        {
            List<BuildGridInteractionUiBindingRow> rows = new();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform target in root.GetComponentsInChildren<Transform>(true))
                {
                    bool relevant = target.GetComponent<BuildGridInteractionPreviewController>() != null
                        || target.GetComponent<BuildGridPreviewSlotView>() != null
                        || target.GetComponent<BuildItemTrayPreviewView>() != null
                        || target.GetComponent<BuildItemPreviewCardView>() != null
                        || target.GetComponent<BuildPlacementFeedbackView>() != null
                        || target.name.IndexOf("CategoryButton", StringComparison.Ordinal) >= 0
                        || target.name.IndexOf("TrayGridSlot", StringComparison.Ordinal) >= 0
                        || target.name.IndexOf("RotatePreviewButtonSlot", StringComparison.Ordinal) >= 0;
                    if (!relevant)
                    {
                        continue;
                    }

                    rows.Add(new BuildGridInteractionUiBindingRow
                    {
                        Path = BuildPath(target),
                        ActiveSelf = target.gameObject.activeSelf,
                        ComponentTypes = string.Join(";", target.GetComponents<Component>()
                            .Where(component => component != null)
                            .Select(component => component.GetType().Name))
                    });
                }
            }

            return rows;
        }

        private static void ValidateTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string passMessage,
            string failMessage,
            string path)
        {
            if (value)
            {
                report.AddInfo(code, passMessage, path);
            }
            else
            {
                report.AddError(code, failMessage, path);
            }
        }

        private static void ValidateMinimum(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual < expected)
            {
                report.AddError(code, $"{label} count too low. actual={actual}, expected>={expected}.", PackageName);
            }
            else
            {
                report.AddInfo(code, $"{label} count pass. actual={actual}, expected>={expected}.", PackageName);
            }
        }

        private static Transform FindDeepChildInScene(Scene scene, string objectName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == objectName)
                {
                    return root.transform;
                }

                Transform found = FindDeepChild(root.transform, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static string BuildPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }

    public sealed class BuildGridInteractionSceneBindingSnapshot
    {
        public bool SceneExists;
        public bool ControllerPresent;
        public bool BoardGridPresent;
        public bool ItemTrayPresent;
        public bool SelectedItemInfoPresent;
        public bool PlacementFeedbackPresent;
        public bool ControllerDevOnly = true;
        public bool ControllerIsEnabled;
        public bool ControllerReadsFormalSave;
        public bool ControllerWritesFormalFlow;
        public bool ControllerWritesFormalUi;
        public bool ControllerTouchesFormalScene;
        public bool ControllerShowsCompleteAnswers;
        public bool BuildSettingsContainsPreview;
        public int BoardSlotCount;
        public int TraySlotCount;
        public int CategoryCount;
        public int ShapeCount;
        public List<string> PlayerTextLatinViolations = new();
        public List<string> ForbiddenAnswerTextViolations = new();
        public List<BuildGridInteractionUiBindingRow> UiBindingRows = new();
    }

    public sealed class BuildGridInteractionPlacementSample
    {
        public string SampleId = string.Empty;
        public string ActionChinese = string.Empty;
        public string ShapeId = string.Empty;
        public string Anchor = string.Empty;
        public string OccupiedCells = string.Empty;
        public bool IsValid;
        public ShapePlacementInvalidReason InvalidReason;
        public string FeedbackChinese = string.Empty;
    }

    public sealed class BuildGridInteractionUiBindingRow
    {
        public string Path = string.Empty;
        public bool ActiveSelf;
        public string ComponentTypes = string.Empty;
    }
}
#endif
