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
    public static class BattleSandboxEnemyEncounterPreviewValidator
    {
        public const string PackageName = BattleSandboxEnemyEncounterPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxEnemyEncounterPreview01/[QA Only] Run Enemy Encounter Preview";

        private static readonly string[] RequiredDeveloperKeys =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "dropBiasWeights",
            "bossSixKeyFullAnswer"
        };

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "Boss",
            "Boss六钥匙",
            "Boss 六钥匙",
            "previewWeight",
            "keyRequirements",
            "BuildProblemSeedDataset",
            "EnemyBossValidationPool"
        };

        private static readonly string[] RequiredSceneObjects =
        {
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyEncounterSelectorPanel",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyEncounterSelectorPanel/EnemySelector",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyEncounterSelectorPanel/PreviousEnemyButton",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyEncounterSelectorPanel/NextEnemyButton",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyEncounterSelectorPanel/PreviousBossButton",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyEncounterSelectorPanel/NextBossButton",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyEncounterSelectorPanel/ReadinessPreviewButton",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/EnemyInfoBlock",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/MechanicHintBlock",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/WeaknessWindowBlock",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/ReadinessPreviewBlock",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/FailureFeedbackBlock",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/DropBiasHintBlock",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/TestTargetBlock",
            "BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyEncounterPreviewPanel/FormalIsolationBlock",
            "BuildSandboxPreviewRoot/EnemyEncounterPreviewRuntime"
        };

        private static readonly string[] ControllerObjectReferenceFields =
        {
            "selectorTitle",
            "enemyInfoBlock",
            "mechanicHintBlock",
            "weaknessWindowBlock",
            "readinessPreviewBlock",
            "failureFeedbackBlock",
            "dropBiasHintBlock",
            "testTargetBlock",
            "isolationBlock",
            "previousEnemyButton",
            "nextEnemyButton",
            "previousBossButton",
            "nextBossButton",
            "readinessPreviewButton"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxPreviewContextValidator.BuildValidationReports();
            reports.Add(BattleSandboxPreviewSceneVerifier.Validate());
            reports.Add(BuildTuningDataPanelPreviewValidator.Validate());
            reports.Add(MechanicHintFeedbackPreviewValidator.Validate());
            reports.Add(Validate());
            return reports;
        }

        public static BattleSandboxEnemyEncounterPreview BuildDefaultPreview()
        {
            BuildSandboxPreviewContext context =
                BuildSandboxPreviewContextValidator.BuildDefaultContext();
            MechanicHintFeedbackPreview hintPreview =
                MechanicHintFeedbackPreviewValidator.BuildDefaultPreview();
            BuildTuningDataPanelPreview dataPanel =
                BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
            return BattleSandboxEnemyEncounterPreviewBuilder.Build(context, hintPreview, dataPanel);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattleSandbox Enemy Encounter Preview 01");
            BattleSandboxEnemyEncounterPreview preview = BuildDefaultPreview();
            BuildTuningDataPanelPreview dataPanel =
                BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();

            ValidateIsolation(report, preview);
            ValidateCoverage(report, preview);
            ValidateRows(report, preview);
            ValidatePlayerText(report, preview);
            ValidateDeveloperLinks(report, preview, dataPanel);
            ValidateSceneBinding(report);
            return report;
        }

        public static int CountPlayerTextLeaks(BattleSandboxEnemyEncounterPreview preview)
        {
            int leaks = 0;
            foreach (BattleSandboxEnemyEncounterRow row in Rows(preview))
            {
                foreach (string value in PlayerTextFields(row))
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        leaks++;
                        continue;
                    }

                    if (!ContainsNonAscii(value) || ContainsLatin(value))
                    {
                        leaks++;
                    }

                    if (ForbiddenPlayerAnswerTokens.Any(token =>
                            value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        leaks++;
                    }
                }
            }

            return leaks;
        }

        public static int CountDeveloperLinkLeaks(BattleSandboxEnemyEncounterPreview preview)
        {
            IReadOnlyList<BattleSandboxEncounterDeveloperAnswerLink> links =
                preview != null && preview.developerAnswerLinks != null
                    ? preview.developerAnswerLinks
                    : Array.Empty<BattleSandboxEncounterDeveloperAnswerLink>();
            int leaks = RequiredDeveloperKeys.Count(required =>
                !links.Any(link => link != null
                    && string.Equals(link.englishStableKey, required, StringComparison.Ordinal)));
            leaks += links.Count(link => link == null
                || !link.developerVisible
                || link.playerVisible
                || !link.maskedFromPlayer);
            return leaks;
        }

        public static int CountSceneBindingLeaks()
        {
            BuildSandboxValidationReport report = new("BattleSandbox Enemy Encounter Scene Binding Count");
            ValidateSceneBinding(report);
            return report.ErrorCount;
        }

        public static IReadOnlyList<string> GetRequiredSceneObjectPaths()
        {
            return RequiredSceneObjects;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyEncounterPreview preview)
        {
            if (preview == null)
            {
                report.AddError("ENCOUNTER_PREVIEW_NULL", "Enemy encounter preview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(BattleSandboxEnemyEncounterPreview));
            }

            if (!string.Equals(preview.referenceMode, BattleSandboxEnemyEncounterPreview.ReferenceMode, StringComparison.Ordinal))
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_REFERENCE_MODE_INVALID",
                    $"Reference mode mismatch. actual={preview.referenceMode}.",
                    nameof(BattleSandboxEnemyEncounterPreview));
            }

            if (!preview.devOnly)
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_DEVONLY_FALSE",
                    "Enemy encounter preview must remain devOnly=true.",
                    nameof(BattleSandboxEnemyEncounterPreview));
            }

            ValidateFalse(report, "ENCOUNTER_PREVIEW_ENABLED_TRUE", preview.isEnabled);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_RUNS_FORMAL_COMBAT", preview.runsFormalCombat);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_WRITES_FORMAL_FLOW", preview.writesFormalFlow);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_WRITES_SAVE", preview.writesFormalSaveData);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_GRANTS_REWARD", preview.grantsFormalReward);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_READS_ENEMY_DEFINITION", preview.readsFormalEnemyDefinition);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_READS_BOSS_CONFIG", preview.readsFormalBossConfig);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_CHANGES_DAMAGE", preview.changesFormalDamage);
            ValidateFalse(report, "ENCOUNTER_PREVIEW_SHOWS_FULL_ANSWERS", preview.playerUiShowsFullAnswers);

            if (!preview.playerUiChineseOnly)
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_CHINESE_ONLY_FALSE",
                    "Player-side preview copy must be Chinese-only.",
                    nameof(BattleSandboxEnemyEncounterPreview));
            }

            if (!preview.developerFullAnswersStayInDataPanel)
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_DEV_PANEL_RULE_FALSE",
                    "Full answer fields must stay in developer data panel links.",
                    nameof(BattleSandboxEnemyEncounterPreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "ENCOUNTER_PREVIEW_ISOLATION_PASS",
                    "Preview is devOnly, disabled, and disconnected from formal flow/combat/reward/save data.",
                    nameof(BattleSandboxEnemyEncounterPreview));
            }
        }

        private static void ValidateCoverage(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyEncounterPreview preview)
        {
            ValidateMinimum(report, "ENCOUNTER_PREVIEW_ENEMY_COUNT", "devOnly enemy option", preview?.EnemyCount ?? 0, 11);
            ValidateMinimum(report, "ENCOUNTER_PREVIEW_BOSS_COUNT", "devOnly boss option", preview?.BossCount ?? 0, 7);
            ValidateMinimum(report, "ENCOUNTER_PREVIEW_DISPLAYED_HINT_COUNT", "Displayed player hint", preview?.DisplayedHintCount ?? 0, 70);
            ValidateMinimum(report, "ENCOUNTER_PREVIEW_MASKED_FIELD_COUNT", "Masked answer field", preview?.MaskedAnswerFieldCount ?? 0, RequiredDeveloperKeys.Length);
        }

        private static void ValidateRows(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyEncounterPreview preview)
        {
            IReadOnlyList<BattleSandboxEnemyEncounterRow> rows = Rows(preview);
            ValidateMinimum(report, "ENCOUNTER_PREVIEW_ROW_COUNT", "Encounter preview row", rows.Count, 18);

            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (BattleSandboxEnemyEncounterRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("ENCOUNTER_PREVIEW_ROW_NULL", "Null encounter row.", nameof(BattleSandboxEnemyEncounterRow));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.encounterId)
                    || string.IsNullOrWhiteSpace(row.encounterKind)
                    || string.IsNullOrWhiteSpace(row.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(row.selectorLabel)
                    || string.IsNullOrWhiteSpace(row.sourceDataPath))
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_ROW_IDENTITY_MISSING",
                        $"Encounter row needs id, kind, Chinese display, selector, and source path. id={row.encounterId}.",
                        nameof(BattleSandboxEnemyEncounterRow));
                }

                if (!ids.Add(row.encounterId))
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_DUPLICATE_ID",
                        $"Duplicate encounter id: {row.encounterId}.",
                        nameof(BattleSandboxEnemyEncounterRow));
                }

                if (row.encounterKind != "enemy" && row.encounterKind != "boss")
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_KIND_INVALID",
                        $"Encounter kind must be enemy or boss. id={row.encounterId}, kind={row.encounterKind}.",
                        nameof(BattleSandboxEnemyEncounterRow));
                }

                if (row.FormalLeak)
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_ROW_SCOPE_LEAK",
                        $"Encounter row must remain devOnly, disabled, visible only as hint copy, and non-formal. id={row.encounterId}.",
                        nameof(BattleSandboxEnemyEncounterRow));
                }
            }
        }

        private static void ValidatePlayerText(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyEncounterPreview preview)
        {
            foreach (BattleSandboxEnemyEncounterRow row in Rows(preview))
            {
                foreach (string value in PlayerTextFields(row))
                {
                    ValidatePlayerChinese(report, value, row.encounterId);
                }
            }
        }

        private static void ValidateDeveloperLinks(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyEncounterPreview preview,
            BuildTuningDataPanelPreview dataPanel)
        {
            IReadOnlyList<BattleSandboxEncounterDeveloperAnswerLink> links =
                preview != null && preview.developerAnswerLinks != null
                    ? preview.developerAnswerLinks
                    : Array.Empty<BattleSandboxEncounterDeveloperAnswerLink>();
            HashSet<string> linkKeys = new(links.Select(link => link?.englishStableKey ?? string.Empty), StringComparer.Ordinal);
            HashSet<string> dataPanelKeys = new(
                dataPanel?.Rows?.Select(row => row.englishStableKey) ?? Array.Empty<string>(),
                StringComparer.Ordinal);

            foreach (string key in RequiredDeveloperKeys)
            {
                if (!linkKeys.Contains(key))
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_DEV_LINK_MISSING",
                        $"Missing developer answer link: {key}.",
                        nameof(BattleSandboxEncounterDeveloperAnswerLink));
                }

                if (!dataPanelKeys.Contains(key))
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_DATA_PANEL_FIELD_MISSING",
                        $"Developer data panel is missing sensitive field: {key}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }
            }

            foreach (BattleSandboxEncounterDeveloperAnswerLink link in links)
            {
                if (link == null)
                {
                    report.AddError("ENCOUNTER_PREVIEW_DEV_LINK_NULL", "Null developer answer link.", nameof(BattleSandboxEncounterDeveloperAnswerLink));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(link.englishStableKey)
                    || string.IsNullOrWhiteSpace(link.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(link.sourceDataPath)
                    || string.IsNullOrWhiteSpace(link.dataPanelSlot))
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_DEV_LINK_SOURCE_MISSING",
                        $"Developer answer link needs key, Chinese name, source path, and slot. key={link.englishStableKey}.",
                        nameof(BattleSandboxEncounterDeveloperAnswerLink));
                }

                if (!link.developerVisible || link.playerVisible || !link.maskedFromPlayer)
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_DEV_LINK_SCOPE_LEAK",
                        $"Developer answer link must stay masked from player UI. key={link.englishStableKey}.",
                        nameof(BattleSandboxEncounterDeveloperAnswerLink));
                }
            }
        }

        private static void ValidateSceneBinding(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string sceneAbsolutePath = Path.Combine(projectRoot, BuildSandboxPreviewSceneMarker.ScenePath);
            if (!File.Exists(sceneAbsolutePath))
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_SCENE_MISSING",
                    "V04 preview scene is missing.",
                    BuildSandboxPreviewSceneMarker.ScenePath);
                return;
            }

            Scene previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.OpenScene(BuildSandboxPreviewSceneMarker.ScenePath, OpenSceneMode.Single);
            try
            {
                if (scene.path != BuildSandboxPreviewSceneMarker.ScenePath)
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_SCENE_PATH_MISMATCH",
                        $"Unexpected scene path: {scene.path}.",
                        BuildSandboxPreviewSceneMarker.ScenePath);
                }

                Dictionary<string, Transform> pathMap = CollectPathMap(scene);
                foreach (string requiredPath in RequiredSceneObjects)
                {
                    if (!pathMap.TryGetValue(requiredPath, out Transform target))
                    {
                        report.AddError(
                            "ENCOUNTER_PREVIEW_SCENE_OBJECT_MISSING",
                            $"Missing scene object: {requiredPath}.",
                            requiredPath);
                        continue;
                    }

                    report.AddInfo("ENCOUNTER_PREVIEW_SCENE_OBJECT_PRESENT", requiredPath, requiredPath);
                    if (requiredPath.EndsWith("Button", StringComparison.Ordinal)
                        && target.GetComponent<Button>() == null)
                    {
                        report.AddError(
                            "ENCOUNTER_PREVIEW_BUTTON_MISSING",
                            $"Object must have Button: {requiredPath}.",
                            requiredPath);
                    }
                }

                BattleSandboxEnemyEncounterPreviewController controller =
                    UnityEngine.Object.FindObjectOfType<BattleSandboxEnemyEncounterPreviewController>(true);
                if (controller == null)
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_CONTROLLER_MISSING",
                        "Enemy encounter preview controller is missing.",
                        nameof(BattleSandboxEnemyEncounterPreviewController));
                    return;
                }

                if (!controller.DevOnly
                    || controller.IsEnabled
                    || controller.RunsFormalCombat
                    || controller.WritesFormalFlow
                    || controller.WritesFormalSaveData
                    || controller.GrantsFormalReward
                    || controller.ShowsCompleteAnswers)
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_CONTROLLER_SCOPE_LEAK",
                        "Controller isolation flags must stay devOnly=true and all formal/full-answer flags false.",
                        nameof(BattleSandboxEnemyEncounterPreviewController));
                }

                SerializedObject serialized = new(controller);
                foreach (string fieldName in ControllerObjectReferenceFields)
                {
                    SerializedProperty property = serialized.FindProperty(fieldName);
                    if (property == null || property.objectReferenceValue == null)
                    {
                        report.AddError(
                            "ENCOUNTER_PREVIEW_CONTROLLER_REF_MISSING",
                            $"Controller serialized reference missing: {fieldName}.",
                            nameof(BattleSandboxEnemyEncounterPreviewController));
                    }
                }

                if (scene.isDirty)
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_VERIFY_DIRTY_SCENE",
                        "Validator must not leave the preview scene dirty.",
                        BuildSandboxPreviewSceneMarker.ScenePath);
                }
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

        private static IReadOnlyList<BattleSandboxEnemyEncounterRow> Rows(
            BattleSandboxEnemyEncounterPreview preview)
        {
            return preview != null && preview.rows != null
                ? preview.rows
                : Array.Empty<BattleSandboxEnemyEncounterRow>();
        }

        private static IEnumerable<string> PlayerTextFields(BattleSandboxEnemyEncounterRow row)
        {
            if (row == null)
            {
                yield break;
            }

            yield return row.chineseDisplayName;
            yield return row.selectorLabel;
            yield return row.mapMechanicChinese;
            yield return row.encounterMechanicChinese;
            yield return row.bossSkillChinese;
            yield return row.weaknessWindowChinese;
            yield return row.testTargetChinese;
            yield return row.readinessPreviewChinese;
            yield return row.failureFeedbackChinese;
            yield return row.dropBiasAtmosphereChinese;
        }

        private static void ValidatePlayerChinese(
            BuildSandboxValidationReport report,
            string value,
            string key)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_PLAYER_TEXT_EMPTY",
                    $"Player preview text is empty. key={key}.",
                    nameof(BattleSandboxEnemyEncounterRow));
                return;
            }

            if (!ContainsNonAscii(value))
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_PLAYER_TEXT_NOT_CHINESE",
                    $"Player preview text must contain Chinese/non-ASCII display text. key={key}.",
                    nameof(BattleSandboxEnemyEncounterRow));
            }

            if (ContainsLatin(value))
            {
                report.AddError(
                    "ENCOUNTER_PREVIEW_PLAYER_TEXT_HAS_LATIN",
                    $"Player preview text contains Latin letters. key={key}, text={value}.",
                    nameof(BattleSandboxEnemyEncounterRow));
            }

            foreach (string token in ForbiddenPlayerAnswerTokens)
            {
                if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    report.AddError(
                        "ENCOUNTER_PREVIEW_PLAYER_TEXT_FORBIDDEN_TOKEN",
                        $"Player preview text exposes forbidden answer token: {token}. key={key}.",
                        nameof(BattleSandboxEnemyEncounterRow));
                }
            }
        }

        private static Dictionary<string, Transform> CollectPathMap(Scene scene)
        {
            Dictionary<string, Transform> map = new(StringComparer.Ordinal);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                CollectPath(root.transform, root.name, map);
            }

            return map;
        }

        private static void CollectPath(
            Transform target,
            string path,
            IDictionary<string, Transform> map)
        {
            map[path] = target;
            foreach (Transform child in target)
            {
                CollectPath(child, path + "/" + child.name, map);
            }
        }

        private static bool ContainsNonAscii(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Any(character => character > 127);
        }

        private static bool ContainsLatin(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Any(character => character <= 127 && char.IsLetter(character));
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "Expected false for this enemy encounter preview isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
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
                return;
            }

            report.AddInfo(code, $"{label} count pass. actual={actual}, expected>={expected}.", PackageName);
        }
    }
}
#endif
