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
    public static class BattleSandboxEnemyCombatFeedbackUiReuseValidator
    {
        public const string PackageName = BattleSandboxEnemyCombatFeedbackPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxEnemyCombatFeedbackUiReuse01/[QA Only] Run Combat Feedback UI Reuse";

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
            "dropBias",
            "Boss",
            "keyRequirements",
            "previewWeight",
            "BuildProblemSeedDataset",
            "EnemyBossValidationPool",
            "答案",
            "解法",
            "权重",
            "六钥匙",
            "题目",
            "准备度"
        };

        private static readonly string[] RequiredSceneObjectNames =
        {
            "EnemyCombatFeedbackControlPanel",
            "PreviousFeedbackButton",
            "NextFeedbackButton",
            "TriggerFloatingFeedbackButton",
            "ControlStatusText",
            "EnemyCombatFeedbackPanel",
            "BossStateText",
            "BossSkillText",
            "BossCastBarRoot",
            "BossCastFill",
            "BossCastTimerText",
            "CombatLogText",
            "EnemyCombatFeedbackFloatingRoot",
            "MechanicFloatingText",
            "EnemyCombatFeedbackDeveloperPanel",
            "MaskedDeveloperFieldsText",
            "EnemyCombatFeedbackRuntime"
        };

        private static readonly string[] ObsoletePlayerPanelNames =
        {
            "EnemyEncounterSelectorPanel",
            "EnemyEncounterPreviewPanel",
            "EnemyEncounterPreviewRuntime"
        };

        private static readonly string[] PlayerVisibleRootNames =
        {
            "EnemyCombatFeedbackControlPanel",
            "EnemyCombatFeedbackPanel",
            "EnemyCombatFeedbackFloatingRoot"
        };

        private static readonly string[] ControllerObjectReferenceFields =
        {
            "previewTitleText",
            "bossStateText",
            "bossSkillText",
            "castTimerText",
            "castFillImage",
            "mechanicFloatingText",
            "mechanicFloatingCanvasGroup",
            "combatLogText",
            "controlStatusText",
            "previousFeedbackButton",
            "nextFeedbackButton",
            "triggerFloatingButton"
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

        public static BattleSandboxEnemyCombatFeedbackPreview BuildDefaultPreview()
        {
            BuildSandboxPreviewContext context =
                BuildSandboxPreviewContextValidator.BuildDefaultContext();
            BuildTuningDataPanelPreview dataPanel =
                BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
            MechanicHintFeedbackPreview hintPreview =
                MechanicHintFeedbackPreviewValidator.BuildDefaultPreview();
            return BattleSandboxEnemyCombatFeedbackBuilder.Build(context, hintPreview, dataPanel);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattleSandbox Enemy Combat Feedback UI Reuse 01");
            BattleSandboxEnemyCombatFeedbackPreview preview = BuildDefaultPreview();
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

        public static int CountPlayerTextLeaks(BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            int leaks = 0;
            foreach (BattleSandboxEnemyCombatFeedbackRow row in Rows(preview))
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

        public static int CountDeveloperLinkLeaks(BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            IReadOnlyList<BattleSandboxCombatFeedbackDeveloperLink> links =
                preview != null && preview.developerAnswerLinks != null
                    ? preview.developerAnswerLinks
                    : Array.Empty<BattleSandboxCombatFeedbackDeveloperLink>();
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
            BuildSandboxValidationReport report = new("BattleSandbox Enemy Combat Feedback Scene Binding Count");
            ValidateSceneBinding(report);
            return report.ErrorCount;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            if (preview == null)
            {
                report.AddError("COMBAT_FEEDBACK_NULL", "Combat feedback preview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "COMBAT_FEEDBACK_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(BattleSandboxEnemyCombatFeedbackPreview));
            }

            if (!string.Equals(preview.referenceMode, BattleSandboxEnemyCombatFeedbackPreview.ReferenceMode, StringComparison.Ordinal))
            {
                report.AddError(
                    "COMBAT_FEEDBACK_REFERENCE_MODE_INVALID",
                    $"Reference mode mismatch. actual={preview.referenceMode}.",
                    nameof(BattleSandboxEnemyCombatFeedbackPreview));
            }

            ValidateTrue(report, "COMBAT_FEEDBACK_DEVONLY_TRUE", preview.devOnly);
            ValidateFalse(report, "COMBAT_FEEDBACK_ENABLED_TRUE", preview.isEnabled);
            ValidateTrue(report, "COMBAT_FEEDBACK_FLOATING_LANGUAGE_FALSE", preview.usesFloatingCombatTextLanguage);
            ValidateTrue(report, "COMBAT_FEEDBACK_CAST_BAR_LANGUAGE_FALSE", preview.usesEnemyIntentCastBarLanguage);
            ValidateTrue(report, "COMBAT_FEEDBACK_BOSSINFO_LANGUAGE_FALSE", preview.usesBossInfoLanguage);
            ValidateTrue(report, "COMBAT_FEEDBACK_TOPIC_PANEL_NOT_REMOVED", preview.playerTopicPanelRemoved);
            ValidateTrue(report, "COMBAT_FEEDBACK_CHINESE_ONLY_FALSE", preview.playerUiChineseOnly);
            ValidateTrue(report, "COMBAT_FEEDBACK_DEV_PANEL_RULE_FALSE", preview.developerFullAnswersStayInDataPanel);
            ValidateFalse(report, "COMBAT_FEEDBACK_SHOWS_FULL_ANSWERS", preview.playerUiShowsFullAnswers);
            ValidateFalse(report, "COMBAT_FEEDBACK_RUNS_FORMAL_COMBAT", preview.runsFormalCombat);
            ValidateFalse(report, "COMBAT_FEEDBACK_CALLS_FORMAL_DAMAGE", preview.callsFormalDamageSettlement);
            ValidateFalse(report, "COMBAT_FEEDBACK_WRITES_FORMAL_FLOW", preview.writesFormalFlow);
            ValidateFalse(report, "COMBAT_FEEDBACK_WRITES_SAVE", preview.writesFormalSaveData);
            ValidateFalse(report, "COMBAT_FEEDBACK_GRANTS_REWARD", preview.grantsFormalReward);
            ValidateFalse(report, "COMBAT_FEEDBACK_ADVANCES_CHAPTER", preview.advancesChapter);
            ValidateFalse(report, "COMBAT_FEEDBACK_OPENS_FEATURE_FLAG", preview.opensFeatureFlag);

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "COMBAT_FEEDBACK_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "COMBAT_FEEDBACK_ISOLATION_PASS",
                    "Preview is devOnly, disabled, and disconnected from formal flow/combat/reward/save data.",
                    nameof(BattleSandboxEnemyCombatFeedbackPreview));
            }
        }

        private static void ValidateCoverage(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            ValidateMinimum(report, "COMBAT_FEEDBACK_ROW_COUNT", "Combat feedback row", preview?.RowCount ?? 0, 24);
            ValidateMinimum(report, "COMBAT_FEEDBACK_PLAYER_ROW_COUNT", "Player visible feedback row", preview?.PlayerVisibleRowCount ?? 0, 24);
            ValidateMinimum(report, "COMBAT_FEEDBACK_BOSS_STATE_COUNT", "Boss state feedback row", preview?.BossStateRowCount ?? 0, 7);
            ValidateMinimum(report, "COMBAT_FEEDBACK_CAST_BAR_COUNT", "Cast bar feedback row", preview?.CastBarRowCount ?? 0, 8);
            ValidateMinimum(report, "COMBAT_FEEDBACK_FLOATING_COUNT", "Floating feedback row", preview?.FloatingFeedbackRowCount ?? 0, 18);
            ValidateMinimum(report, "COMBAT_FEEDBACK_MECHANIC_COUNT", "Mechanic feedback row", preview?.MechanicFeedbackRowCount ?? 0, 8);
            ValidateMinimum(report, "COMBAT_FEEDBACK_MASKED_FIELD_COUNT", "Masked answer field", preview?.MaskedAnswerFieldCount ?? 0, RequiredDeveloperKeys.Length);
        }

        private static void ValidateRows(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            IReadOnlyList<BattleSandboxEnemyCombatFeedbackRow> rows = Rows(preview);
            HashSet<string> ids = new(StringComparer.Ordinal);
            HashSet<string> allowedKinds = new(StringComparer.Ordinal)
            {
                BattleSandboxEnemyCombatFeedbackKinds.BossState,
                BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast,
                BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback,
                BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow,
                BattleSandboxEnemyCombatFeedbackKinds.FailureFeedback,
                BattleSandboxEnemyCombatFeedbackKinds.EnemyState
            };

            foreach (BattleSandboxEnemyCombatFeedbackRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("COMBAT_FEEDBACK_ROW_NULL", "Null combat feedback row.", nameof(BattleSandboxEnemyCombatFeedbackRow));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.feedbackId)
                    || string.IsNullOrWhiteSpace(row.feedbackKind)
                    || string.IsNullOrWhiteSpace(row.reuseSourceComponent)
                    || string.IsNullOrWhiteSpace(row.sourceDataPath)
                    || string.IsNullOrWhiteSpace(row.developerDataPanelFieldKey))
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_ROW_IDENTITY_MISSING",
                        $"Feedback row needs id, kind, reuse source, source path, and developer field key. id={row.feedbackId}.",
                        nameof(BattleSandboxEnemyCombatFeedbackRow));
                }

                if (!ids.Add(row.feedbackId))
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_DUPLICATE_ID",
                        $"Duplicate feedback id: {row.feedbackId}.",
                        nameof(BattleSandboxEnemyCombatFeedbackRow));
                }

                if (!allowedKinds.Contains(row.feedbackKind))
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_KIND_INVALID",
                        $"Feedback kind is invalid. id={row.feedbackId}, kind={row.feedbackKind}.",
                        nameof(BattleSandboxEnemyCombatFeedbackRow));
                }

                if (!row.usesFloatingCombatTextLanguage)
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_FLOATING_LANGUAGE_MISSING",
                        $"Feedback row must keep floating combat text language. id={row.feedbackId}.",
                        nameof(BattleSandboxEnemyCombatFeedbackRow));
                }

                if (row.feedbackKind == BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast
                    && !row.usesEnemyCastBarLanguage)
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_CAST_ROW_NO_CAST_BAR",
                        $"Boss skill cast row must use enemy cast bar language. id={row.feedbackId}.",
                        nameof(BattleSandboxEnemyCombatFeedbackRow));
                }

                if (row.FormalLeak)
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_ROW_SCOPE_LEAK",
                        $"Combat feedback row must remain devOnly, disabled, non-formal, and non-answer. id={row.feedbackId}.",
                        nameof(BattleSandboxEnemyCombatFeedbackRow));
                }
            }
        }

        private static void ValidatePlayerText(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            foreach (BattleSandboxEnemyCombatFeedbackRow row in Rows(preview))
            {
                foreach (string value in PlayerTextFields(row))
                {
                    ValidatePlayerChinese(report, value, row.feedbackId);
                }
            }
        }

        private static void ValidateDeveloperLinks(
            BuildSandboxValidationReport report,
            BattleSandboxEnemyCombatFeedbackPreview preview,
            BuildTuningDataPanelPreview dataPanel)
        {
            IReadOnlyList<BattleSandboxCombatFeedbackDeveloperLink> links =
                preview != null && preview.developerAnswerLinks != null
                    ? preview.developerAnswerLinks
                    : Array.Empty<BattleSandboxCombatFeedbackDeveloperLink>();
            HashSet<string> linkKeys = new(links.Select(link => link?.englishStableKey ?? string.Empty), StringComparer.Ordinal);
            HashSet<string> dataPanelKeys = new(
                dataPanel?.Rows?.Select(row => row.englishStableKey) ?? Array.Empty<string>(),
                StringComparer.Ordinal);

            foreach (string key in RequiredDeveloperKeys)
            {
                if (!linkKeys.Contains(key))
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_DEV_LINK_MISSING",
                        $"Missing developer answer link: {key}.",
                        nameof(BattleSandboxCombatFeedbackDeveloperLink));
                }

                if (!dataPanelKeys.Contains(key))
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_DATA_PANEL_FIELD_MISSING",
                        $"Developer data panel is missing sensitive field: {key}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }
            }

            foreach (BattleSandboxCombatFeedbackDeveloperLink link in links)
            {
                if (link == null)
                {
                    report.AddError("COMBAT_FEEDBACK_DEV_LINK_NULL", "Null developer answer link.", nameof(BattleSandboxCombatFeedbackDeveloperLink));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(link.englishStableKey)
                    || string.IsNullOrWhiteSpace(link.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(link.sourceDataPath)
                    || string.IsNullOrWhiteSpace(link.dataPanelSlot))
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_DEV_LINK_SOURCE_MISSING",
                        $"Developer answer link needs key, Chinese name, source path, and slot. key={link.englishStableKey}.",
                        nameof(BattleSandboxCombatFeedbackDeveloperLink));
                }

                if (!link.developerVisible || link.playerVisible || !link.maskedFromPlayer)
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_DEV_LINK_SCOPE_LEAK",
                        $"Developer answer link must stay masked from player UI. key={link.englishStableKey}.",
                        nameof(BattleSandboxCombatFeedbackDeveloperLink));
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
                    "COMBAT_FEEDBACK_SCENE_MISSING",
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
                        "COMBAT_FEEDBACK_SCENE_PATH_MISMATCH",
                        $"Unexpected scene path: {scene.path}.",
                        BuildSandboxPreviewSceneMarker.ScenePath);
                }

                Dictionary<string, Transform> names = CollectNameMap(scene);
                foreach (string requiredName in RequiredSceneObjectNames)
                {
                    if (!names.TryGetValue(requiredName, out Transform target))
                    {
                        report.AddError(
                            "COMBAT_FEEDBACK_SCENE_OBJECT_MISSING",
                            $"Missing scene object: {requiredName}.",
                            requiredName);
                        continue;
                    }

                    report.AddInfo("COMBAT_FEEDBACK_SCENE_OBJECT_PRESENT", BuildPath(target), BuildPath(target));
                    if (requiredName.EndsWith("Button", StringComparison.Ordinal)
                        && target.GetComponent<Button>() == null)
                    {
                        report.AddError(
                            "COMBAT_FEEDBACK_BUTTON_MISSING",
                            $"Object must have Button: {requiredName}.",
                            BuildPath(target));
                    }
                }

                foreach (string obsoleteName in ObsoletePlayerPanelNames)
                {
                    if (names.TryGetValue(obsoleteName, out Transform obsolete)
                        && obsolete.gameObject.activeInHierarchy)
                    {
                        report.AddError(
                            "COMBAT_FEEDBACK_OBSOLETE_TOPIC_PANEL_ACTIVE",
                            $"Obsolete player-side topic panel/runtime must be removed or inactive: {obsoleteName}.",
                            BuildPath(obsolete));
                    }
                }

                ValidateActivePlayerText(report, names);
                ValidateCastBar(report, names);
                ValidateController(report);

                if (scene.isDirty)
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_VERIFY_DIRTY_SCENE",
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

        private static void ValidateActivePlayerText(
            BuildSandboxValidationReport report,
            IReadOnlyDictionary<string, Transform> names)
        {
            foreach (string rootName in PlayerVisibleRootNames)
            {
                if (!names.TryGetValue(rootName, out Transform root))
                {
                    continue;
                }

                foreach (Text text in root.GetComponentsInChildren<Text>(true))
                {
                    if (text == null || !text.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    string value = text.text ?? string.Empty;
                    if (ContainsLatin(value))
                    {
                        report.AddError(
                            "COMBAT_FEEDBACK_SCENE_PLAYER_TEXT_HAS_LATIN",
                            $"Player feedback scene text contains Latin letters: {value}.",
                            BuildPath(text.transform));
                    }

                    foreach (string token in ForbiddenPlayerAnswerTokens)
                    {
                        if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            report.AddError(
                                "COMBAT_FEEDBACK_SCENE_PLAYER_TEXT_FORBIDDEN_TOKEN",
                                $"Player feedback scene text exposes forbidden answer token: {token}.",
                                BuildPath(text.transform));
                        }
                    }
                }
            }
        }

        private static void ValidateCastBar(
            BuildSandboxValidationReport report,
            IReadOnlyDictionary<string, Transform> names)
        {
            if (!names.TryGetValue("BossCastFill", out Transform fillTransform))
            {
                return;
            }

            Image fill = fillTransform.GetComponent<Image>();
            if (fill == null)
            {
                report.AddError("COMBAT_FEEDBACK_CAST_FILL_IMAGE_MISSING", "BossCastFill must have Image.", BuildPath(fillTransform));
                return;
            }

            if (fill.type != Image.Type.Filled)
            {
                report.AddError(
                    "COMBAT_FEEDBACK_CAST_FILL_NOT_FILLED",
                    "BossCastFill must use Image.Type.Filled to match cast-bar progress language.",
                    BuildPath(fillTransform));
            }
            else
            {
                report.AddInfo(
                    "COMBAT_FEEDBACK_CAST_BAR_PASS",
                    "Boss cast bar uses filled Image progress language.",
                    BuildPath(fillTransform));
            }
        }

        private static void ValidateController(BuildSandboxValidationReport report)
        {
            BattleSandboxEnemyCombatFeedbackController controller =
                UnityEngine.Object.FindObjectOfType<BattleSandboxEnemyCombatFeedbackController>(true);
            if (controller == null)
            {
                report.AddError(
                    "COMBAT_FEEDBACK_CONTROLLER_MISSING",
                    "Combat feedback controller is missing.",
                    nameof(BattleSandboxEnemyCombatFeedbackController));
                return;
            }

            if (!controller.DevOnly
                || controller.IsEnabled
                || controller.RunsFormalCombat
                || controller.CallsFormalDamageSettlement
                || controller.WritesFormalFlow
                || controller.WritesFormalSaveData
                || controller.GrantsFormalReward
                || controller.AdvancesChapter
                || controller.OpensFeatureFlag
                || controller.ShowsCompleteAnswers)
            {
                report.AddError(
                    "COMBAT_FEEDBACK_CONTROLLER_SCOPE_LEAK",
                    "Controller isolation flags must stay devOnly=true and all formal/full-answer flags false.",
                    nameof(BattleSandboxEnemyCombatFeedbackController));
            }

            SerializedObject serialized = new(controller);
            foreach (string fieldName in ControllerObjectReferenceFields)
            {
                SerializedProperty property = serialized.FindProperty(fieldName);
                if (property == null || property.objectReferenceValue == null)
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_CONTROLLER_REF_MISSING",
                        $"Controller serialized reference missing: {fieldName}.",
                        nameof(BattleSandboxEnemyCombatFeedbackController));
                }
            }
        }

        private static IReadOnlyList<BattleSandboxEnemyCombatFeedbackRow> Rows(
            BattleSandboxEnemyCombatFeedbackPreview preview)
        {
            return preview != null && preview.rows != null
                ? preview.rows
                : Array.Empty<BattleSandboxEnemyCombatFeedbackRow>();
        }

        private static IEnumerable<string> PlayerTextFields(BattleSandboxEnemyCombatFeedbackRow row)
        {
            if (row == null)
            {
                yield break;
            }

            yield return row.bossDisplayNameChinese;
            yield return row.stateLineChinese;
            yield return row.castSkillLineChinese;
            yield return row.floatingTextChinese;
            yield return row.combatLogLineChinese;
        }

        private static void ValidatePlayerChinese(
            BuildSandboxValidationReport report,
            string value,
            string key)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                report.AddError(
                    "COMBAT_FEEDBACK_PLAYER_TEXT_EMPTY",
                    $"Player feedback text is empty. key={key}.",
                    nameof(BattleSandboxEnemyCombatFeedbackRow));
                return;
            }

            if (!ContainsNonAscii(value))
            {
                report.AddError(
                    "COMBAT_FEEDBACK_PLAYER_TEXT_NOT_CHINESE",
                    $"Player feedback text must contain Chinese/non-ASCII display text. key={key}.",
                    nameof(BattleSandboxEnemyCombatFeedbackRow));
            }

            if (ContainsLatin(value))
            {
                report.AddError(
                    "COMBAT_FEEDBACK_PLAYER_TEXT_HAS_LATIN",
                    $"Player feedback text contains Latin letters. key={key}, text={value}.",
                    nameof(BattleSandboxEnemyCombatFeedbackRow));
            }

            foreach (string token in ForbiddenPlayerAnswerTokens)
            {
                if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    report.AddError(
                        "COMBAT_FEEDBACK_PLAYER_TEXT_FORBIDDEN_TOKEN",
                        $"Player feedback text exposes forbidden answer token: {token}. key={key}.",
                        nameof(BattleSandboxEnemyCombatFeedbackRow));
                }
            }
        }

        private static Dictionary<string, Transform> CollectNameMap(Scene scene)
        {
            Dictionary<string, Transform> map = new(StringComparer.Ordinal);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                CollectName(root.transform, map);
            }

            return map;
        }

        private static void CollectName(Transform target, IDictionary<string, Transform> map)
        {
            if (!map.ContainsKey(target.name))
            {
                map.Add(target.name, target);
            }

            foreach (Transform child in target)
            {
                CollectName(child, map);
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

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "Expected false for this combat feedback isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
        }

        private static void ValidateTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (!value)
            {
                report.AddError(code, "Expected true for this combat feedback UI reuse flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Required flag remains true.", PackageName);
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
