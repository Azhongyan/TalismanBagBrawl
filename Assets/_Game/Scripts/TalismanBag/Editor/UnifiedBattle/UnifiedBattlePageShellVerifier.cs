#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.UnifiedBattle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class UnifiedBattlePageShellVerifier
    {
        private static readonly string[] PlayerVisibleSlotNames =
        {
            UnifiedBattlePageShellSlotNames.BoardArea,
            UnifiedBattlePageShellSlotNames.ItemTrayArea,
            UnifiedBattlePageShellSlotNames.EnemyInfoArea,
            UnifiedBattlePageShellSlotNames.BossCastBarSlot,
            UnifiedBattlePageShellSlotNames.BattleFeedbackLayer,
            UnifiedBattlePageShellSlotNames.StoryGuidePopupLayer,
            UnifiedBattlePageShellSlotNames.ResultRewardPlaceholder
        };

        private static readonly string[] ForbiddenPlayerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "bossSixKeyFullAnswer",
            "problemReadinessFullAnswer"
        };

        private static readonly string[] ForbiddenRuntimeReferenceTokens =
        {
            "SaveData",
            "PlayerPrefs",
            "MainTrialProgressData",
            "RewardService",
            "V02RunFlowController",
            "MainTrialFlowService",
            "AutoCombatController",
            "PageState",
            "FormationState",
            "V02FormationGridFrame",
            "DamageText",
            "BossInfoPanel",
            "SceneManager.LoadScene",
            "EditorBuildSettings.scenes"
        };

        public static BuildSandboxValidationReport Validate(out UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            snapshot = BuildSourceStaticSnapshot();
            BuildSandboxValidationReport report = new("UnifiedBattlePageShell01");

            ValidateCodeDefinedSlots(report, snapshot);
            ValidateSampleData(report, snapshot);
            ValidateRuntimeSourceReferences(report, snapshot);
            ValidateBuildSettingsIsolation(report, snapshot);
            ValidateSceneOrPrefabAsset(report, snapshot);

            return report;
        }

        public static string[] GetRequiredObjectPaths()
        {
            return UnifiedBattlePageShellSlotNames.RequiredSlots
                .Select(slot => slot == UnifiedBattlePageShellSlotNames.BattlePageRoot
                    ? UnifiedBattlePageShellSlotNames.BattlePageRoot
                    : UnifiedBattlePageShellSlotNames.BattlePageRoot + "/" + slot)
                .ToArray();
        }

        public static IReadOnlyList<UnifiedBattlePageShellHierarchyRow> BuildSourceStaticHierarchyRows()
        {
            List<UnifiedBattlePageShellHierarchyRow> rows = new()
            {
                new(
                    UnifiedBattlePageShellSlotNames.BattlePageRoot,
                    true,
                    "RectTransform;Image;UnifiedBattlePageShellMarker;UnifiedBattlePageShell",
                    "code-defined")
            };

            foreach (string slot in UnifiedBattlePageShellSlotNames.RequiredSlots)
            {
                if (slot == UnifiedBattlePageShellSlotNames.BattlePageRoot)
                {
                    continue;
                }

                rows.Add(new(
                    UnifiedBattlePageShellSlotNames.BattlePageRoot + "/" + slot,
                    true,
                    "RectTransform;Image;Outline;Text",
                    "code-defined"));
            }

            return rows;
        }

        private static void ValidateCodeDefinedSlots(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            string[] requiredSlots = UnifiedBattlePageShellSlotNames.RequiredSlots;
            snapshot.RequiredSlotCount = requiredSlots.Length;
            snapshot.CodeDefinedSlotCount = requiredSlots.Distinct(StringComparer.Ordinal).Count();

            if (snapshot.CodeDefinedSlotCount == requiredSlots.Length)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_CODE_DEFINED_SLOTS_PRESENT",
                    "All required slots are defined as stable English slot names.",
                    nameof(UnifiedBattlePageShellSlotNames));
                return;
            }

            report.AddError(
                "UNIFIED_SHELL_CODE_DEFINED_SLOTS_MISSING",
                $"Required slot constants are incomplete. required={requiredSlots.Length}, defined={snapshot.CodeDefinedSlotCount}.",
                nameof(UnifiedBattlePageShellSlotNames));
        }

        private static void ValidateSampleData(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            UnifiedBattlePageShellSampleBinding sample = UnifiedBattlePageShellSampleData.CreateDefault();
            BattleResultSnapshot result = sample.resultSnapshot;
            BuildEvaluationSnapshot build = sample.buildEvaluationSnapshot;
            List<string> playerValues = new();
            playerValues.AddRange(sample.enemySnapshot.weaknessHints ?? new List<string>());
            playerValues.AddRange(build.playerVisibleHints ?? new List<string>());
            playerValues.Add(build.readinessSummary);
            playerValues.Add(build.recommendedAction);
            playerValues.Add(result.nextRouteHint);

            snapshot.SampleLayoutItemCount = sample.layoutSnapshot.placedItems.Count;
            snapshot.SampleResultDevOnly = result.devOnly;
            snapshot.SampleResultShouldWriteSave = result.shouldWriteSave;
            snapshot.SampleResultShouldGrantReward = result.shouldGrantReward;
            snapshot.PlayerVisibleAnswerLeakCount = playerValues.Count(ContainsForbiddenPlayerToken);

            if (sample.startRequest.devOnly && !sample.startRequest.formalFlow)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_START_REQUEST_DEVONLY",
                    "Sample BattleStartRequest is devOnly and formalFlow=false.",
                    nameof(UnifiedBattlePageShellSampleData));
            }
            else
            {
                report.AddError(
                    "UNIFIED_SHELL_START_REQUEST_FORMAL",
                    "Sample BattleStartRequest must stay devOnly and formalFlow=false.",
                    nameof(UnifiedBattlePageShellSampleData));
            }

            if (snapshot.SampleLayoutItemCount > 0)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_SAMPLE_LAYOUT_BOUND",
                    $"Sample BattleLayoutSnapshot has items={snapshot.SampleLayoutItemCount}.",
                    nameof(UnifiedBattlePageShellSampleData));
            }
            else
            {
                report.AddError(
                    "UNIFIED_SHELL_SAMPLE_LAYOUT_EMPTY",
                    "Sample BattleLayoutSnapshot must include placeholder item data.",
                    nameof(UnifiedBattlePageShellSampleData));
            }

            if (result.devOnly && !result.shouldWriteSave && !result.shouldGrantReward)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_RESULT_PLACEHOLDER_SAFE",
                    "ResultRewardPlaceholder sample does not write save or grant reward.",
                    nameof(UnifiedBattlePageShellSampleData));
            }
            else
            {
                report.AddError(
                    "UNIFIED_SHELL_RESULT_PLACEHOLDER_UNSAFE",
                    "ResultRewardPlaceholder sample must be devOnly/no-save/no-reward.",
                    nameof(UnifiedBattlePageShellSampleData));
            }

            if (snapshot.PlayerVisibleAnswerLeakCount == 0)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_PLAYER_FIELD_SPLIT_PASS",
                    "Player-visible sample fields do not contain answer-layer tokens.",
                    nameof(UnifiedBattlePageShellSampleData));
            }
            else
            {
                report.AddError(
                    "UNIFIED_SHELL_PLAYER_FIELD_SPLIT_LEAK",
                    $"Player-visible sample fields leaked answer tokens count={snapshot.PlayerVisibleAnswerLeakCount}.",
                    nameof(UnifiedBattlePageShellSampleData));
            }
        }

        private static void ValidateRuntimeSourceReferences(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string runtimePath = Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/UnifiedBattle");
            if (!Directory.Exists(runtimePath))
            {
                report.AddError(
                    "UNIFIED_SHELL_RUNTIME_FOLDER_MISSING",
                    "UnifiedBattle runtime folder is missing.",
                    "Assets/_Game/Scripts/TalismanBag/UnifiedBattle");
                snapshot.ForbiddenRuntimeReferenceCount++;
                return;
            }

            foreach (string file in Directory.GetFiles(runtimePath, "*.cs", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                foreach (string token in ForbiddenRuntimeReferenceTokens)
                {
                    if (text.IndexOf(token, StringComparison.Ordinal) >= 0)
                    {
                        snapshot.ForbiddenRuntimeReferenceCount++;
                        report.AddError(
                            "UNIFIED_SHELL_FORBIDDEN_RUNTIME_REFERENCE",
                            $"Runtime UnifiedBattle source references forbidden token `{token}`.",
                            ToAssetPath(projectRoot, file));
                    }
                }
            }

            if (snapshot.ForbiddenRuntimeReferenceCount == 0)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_RUNTIME_REFERENCE_PASS",
                    "Runtime UnifiedBattle source has no forbidden formal-flow/save/reward/Boss references.",
                    "Assets/_Game/Scripts/TalismanBag/UnifiedBattle");
            }
        }

        private static void ValidateBuildSettingsIsolation(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            snapshot.SceneInBuildSettings = EditorBuildSettings.scenes.Any(scene =>
                scene != null && string.Equals(scene.path, UnifiedBattlePageShellMarker.ScenePath, StringComparison.Ordinal));
            if (snapshot.SceneInBuildSettings)
            {
                report.AddError(
                    "UNIFIED_SHELL_SCENE_IN_BUILD_SETTINGS",
                    "UnifiedBattle shell scene must not be added to Build Settings in this package.",
                    "ProjectSettings/EditorBuildSettings.asset");
                return;
            }

            report.AddInfo(
                "UNIFIED_SHELL_BUILD_SETTINGS_ISOLATED",
                "UnifiedBattle shell scene is not present in Build Settings.",
                "ProjectSettings/EditorBuildSettings.asset");
        }

        private static void ValidateSceneOrPrefabAsset(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string sceneAbsolute = Path.Combine(projectRoot, UnifiedBattlePageShellMarker.ScenePath);
            string prefabAbsolute = Path.Combine(projectRoot, UnifiedBattlePageShellMarker.PrefabPath);

            snapshot.SceneExists = File.Exists(sceneAbsolute);
            snapshot.PrefabExists = File.Exists(prefabAbsolute);

            if (!snapshot.SceneExists && !snapshot.PrefabExists)
            {
                snapshot.ValidationMode = "SOURCE_STATIC_CODE_DEFINED";
                report.AddWarning(
                    "UNIFIED_SHELL_ASSET_NOT_BUILT",
                    "No physical shell scene/prefab asset exists yet; Editor menu defines a devOnly builder path for manual Unity generation.",
                    UnifiedBattlePageShellMarker.ScenePath);
                return;
            }

            snapshot.ValidationMode = "UNITY_ASSET_STATIC";

            if (snapshot.SceneExists)
            {
                ValidateSceneAsset(report, snapshot);
            }

            if (snapshot.PrefabExists)
            {
                ValidatePrefabAsset(report, snapshot);
            }
        }

        private static void ValidateSceneAsset(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(UnifiedBattlePageShellMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                if (scene.path != UnifiedBattlePageShellMarker.ScenePath)
                {
                    report.AddError(
                        "UNIFIED_SHELL_SCENE_PATH_MISMATCH",
                        $"Unexpected scene path {scene.path}.",
                        UnifiedBattlePageShellMarker.ScenePath);
                    return;
                }

                UnifiedBattlePageShell shell = UnityEngine.Object.FindObjectOfType<UnifiedBattlePageShell>(true);
                UnifiedBattlePageShellMarker marker = UnityEngine.Object.FindObjectOfType<UnifiedBattlePageShellMarker>(true);
                ValidateShellComponent(report, snapshot, shell, marker, "scene");
            }
            finally
            {
                if (previousScene.IsValid()
                    && !string.IsNullOrEmpty(previousScene.path)
                    && previousScene.path != scene.path
                    && File.Exists(Path.Combine(projectRoot, previousScene.path)))
                {
                    EditorSceneManager.OpenScene(previousScene.path, OpenSceneMode.Single);
                }
            }
        }

        private static void ValidatePrefabAsset(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(UnifiedBattlePageShellMarker.PrefabPath);
            if (prefab == null)
            {
                report.AddError(
                    "UNIFIED_SHELL_PREFAB_LOAD_FAILED",
                    "Prefab exists on disk but could not be loaded.",
                    UnifiedBattlePageShellMarker.PrefabPath);
                return;
            }

            UnifiedBattlePageShell shell = prefab.GetComponentInChildren<UnifiedBattlePageShell>(true);
            UnifiedBattlePageShellMarker marker = prefab.GetComponentInChildren<UnifiedBattlePageShellMarker>(true);
            ValidateShellComponent(report, snapshot, shell, marker, "prefab");
        }

        private static void ValidateShellComponent(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot,
            UnifiedBattlePageShell shell,
            UnifiedBattlePageShellMarker marker,
            string source)
        {
            if (shell == null)
            {
                report.AddError(
                    "UNIFIED_SHELL_COMPONENT_MISSING",
                    $"UnifiedBattlePageShell component is missing from {source}.",
                    source);
                return;
            }

            List<string> missing = shell.CollectMissingRequiredSlots();
            snapshot.AssetSlotPresentCount = UnifiedBattlePageShellSlotNames.RequiredSlots.Length - missing.Count;
            foreach (string slot in missing)
            {
                report.AddError("UNIFIED_SHELL_SLOT_MISSING", $"{source} missing slot {slot}.", slot);
            }

            if (missing.Count == 0)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_ASSET_SLOTS_PRESENT",
                    $"{source} has all required slots.",
                    source);
            }

            if (marker == null)
            {
                report.AddError(
                    "UNIFIED_SHELL_MARKER_MISSING",
                    $"UnifiedBattlePageShellMarker is missing from {source}.",
                    source);
                return;
            }

            if (marker.DevOnly && !marker.IsEnabled && !marker.FormalFlow && !marker.ConnectedToFormalRoute)
            {
                report.AddInfo(
                    "UNIFIED_SHELL_MARKER_ISOLATED",
                    $"{source} marker is devOnly=true, isEnabled=false, formalFlow=false.",
                    source);
            }
            else
            {
                report.AddError(
                    "UNIFIED_SHELL_MARKER_LEAK",
                    $"{source} marker must stay devOnly=true/isEnabled=false/formalFlow=false/connectedToFormalRoute=false.",
                    source);
            }

            IReadOnlyDictionary<string, Transform> slotMap = shell.BuildSlotMap();
            Transform diagnosticsSlot = slotMap[UnifiedBattlePageShellSlotNames.DevOnlyDiagnosticsSlot];
            foreach (string playerSlotName in PlayerVisibleSlotNames)
            {
                Transform playerSlot = slotMap[playerSlotName];
                if (playerSlot != null && diagnosticsSlot != null && playerSlot == diagnosticsSlot)
                {
                    report.AddError(
                        "UNIFIED_SHELL_DIAGNOSTICS_NOT_SEPARATE",
                        "DevOnlyDiagnosticsSlot must not share a Transform with player-visible slots.",
                        playerSlotName);
                }
            }
        }

        private static UnifiedBattlePageShellValidationSnapshot BuildSourceStaticSnapshot()
        {
            return new UnifiedBattlePageShellValidationSnapshot
            {
                ValidationMode = "SOURCE_STATIC_CODE_DEFINED",
                RequiredSlotCount = UnifiedBattlePageShellSlotNames.RequiredSlots.Length,
                CodeDefinedSlotCount = UnifiedBattlePageShellSlotNames.RequiredSlots.Distinct(StringComparer.Ordinal).Count()
            };
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return ForbiddenPlayerTokens.Any(token =>
                value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string ToAssetPath(string projectRoot, string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(projectRoot)
                || string.IsNullOrWhiteSpace(absolutePath)
                || !absolutePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return absolutePath;
            }

            return absolutePath.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace("\\", "/");
        }
    }

    public sealed class UnifiedBattlePageShellValidationSnapshot
    {
        public string ValidationMode = "SOURCE_STATIC_CODE_DEFINED";
        public int RequiredSlotCount;
        public int CodeDefinedSlotCount;
        public int AssetSlotPresentCount;
        public int SampleLayoutItemCount;
        public bool SampleResultDevOnly;
        public bool SampleResultShouldWriteSave;
        public bool SampleResultShouldGrantReward;
        public int PlayerVisibleAnswerLeakCount;
        public int ForbiddenRuntimeReferenceCount;
        public bool SceneExists;
        public bool PrefabExists;
        public bool SceneInBuildSettings;

        public int LeakCount =>
            PlayerVisibleAnswerLeakCount
            + ForbiddenRuntimeReferenceCount
            + (SampleResultDevOnly ? 0 : 1)
            + (SampleResultShouldWriteSave ? 1 : 0)
            + (SampleResultShouldGrantReward ? 1 : 0)
            + (SceneInBuildSettings ? 1 : 0);
    }

    public readonly struct UnifiedBattlePageShellHierarchyRow
    {
        public UnifiedBattlePageShellHierarchyRow(
            string path,
            bool activeSelf,
            string componentTypes,
            string source)
        {
            Path = path;
            ActiveSelf = activeSelf;
            ComponentTypes = componentTypes;
            Source = source;
        }

        public string Path { get; }
        public bool ActiveSelf { get; }
        public string ComponentTypes { get; }
        public string Source { get; }
    }
}
#endif
