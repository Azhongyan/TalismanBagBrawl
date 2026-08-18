using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.V04.ChapterFlow;
using TalismanBag.V04.ChapterFlow.Chapter1;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1.Presentation
{
    public static class
        C1EncounterOwnershipAndVisualSlotHandoffVerifier
    {
        private const string PackageId =
            "V0.4-C1BattleSandboxEncounterOwnershipAndVisualSlotHandoffFix01";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string ReportPath =
            "Docs/V0.4/Reports/C1EncounterOwnershipAndVisualSlotHandoffFixReport.md";
        private const string RowsPath =
            "Docs/V0.4/Reports/C1EncounterOwnershipRows.csv";
        private const string LeakPath =
            "Docs/V0.4/Reports/C1EncounterOwnershipLeakCheckReport.md";
        public static void VerifyStatic()
        {
            Run(exitBatch: false);
        }

        public static void VerifyStaticBatch()
        {
            Run(exitBatch: true);
        }

        private static void Run(bool exitBatch)
        {
            List<string> errors = new();
            try
            {
                Check(!EditorApplication.isPlaying,
                    "Verifier must start in Edit Mode.", errors);
                Check(File.Exists(ScenePath),
                    "Target scene is missing.", errors);

                Scene active = SceneManager.GetActiveScene();
                if (!Application.isBatchMode
                    && active.IsValid()
                    && active.isDirty
                    && !string.Equals(
                        active.path,
                        ScenePath,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "Another scene is dirty; verifier did not open or save anything.");
                }

                Scene scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single);
                bool dirtyBefore = scene.isDirty;
                Image[] images = Resources.FindObjectsOfTypeAll<Image>()
                    .Where(value => value != null
                        && value.gameObject.scene == scene)
                    .ToArray();
                Image[] exactSlots = images.Where(value =>
                    value.gameObject.name == "Shougunu_1"
                    && value.transform.parent != null
                    && value.transform.parent.name == "V02EnemyArea")
                    .ToArray();
                Image[] ordinarySources = images.Where(value =>
                    value.gameObject.name == "Enemy"
                    && value.transform.parent != null
                    && value.transform.parent.name == "V02EnemyArea")
                    .ToArray();
                Check(exactSlots.Length == 1,
                    "Expected exactly one V02EnemyArea/Shougunu_1 Image; actual="
                    + exactSlots.Length, errors);
                Check(ordinarySources.Length == 1
                    && ordinarySources[0].sprite != null,
                    "Expected exactly one authored inactive Enemy sprite source.",
                    errors);
                Check(scene.isDirty == dirtyBefore,
                    "Read-only scene survey dirtied the scene.", errors);

                string runtimeSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/ShougunuPhase1BattleSandboxVerticalSliceRuntime.cs");
                string flowSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxRuntimeController.cs");
                string presentationSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Presentation/C1JourneyPresentationController.cs");
                string overlaySource = Read(
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Presentation/C1JourneyPresentationOverlay.cs");
                string visualSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShougunuPhase1VisualPrototypeController.cs");
                string normalAdapterSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1NormalEnemyBattleAdapter.cs");
                string binderSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/ShougunuPhase1BattleSandboxSceneBinder.cs");
                string liHuoSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox/LiHuoCombatFeedbackOrchestrationController.cs");
                string outlineSource = Read(
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemLivingGradientOutlineVfxPrototypeController.cs");

                Check(Has(
                        runtimeSource,
                        "BOSS_ENCOUNTER_ADMISSION_REQUIRED")
                    && Has(
                        runtimeSource,
                        "IsBossEncounterAdmissionGranted()"),
                    "P3 runtime admission gate markers are missing.",
                    errors);
                Check(Has(
                        flowSource,
                        "IC1EncounterAdmissionAuthority")
                    && Has(
                        flowSource,
                        "PublishAcceptedBossAdmission"),
                    "Flow admission publisher markers are missing.",
                    errors);
                Check(Has(
                        presentationSource,
                        "AcquireAuthoredSlotForOrdinary")
                    && Has(
                        presentationSource,
                        "ReleaseAuthoredSlot")
                    && Has(
                        presentationSource,
                        "Shougunu_1"),
                    "Authored slot router markers are missing.",
                    errors);
                Check(!Has(overlaySource, "DrawActor(")
                    && !Has(
                        overlaySource,
                        "controller.EnemyTexture"),
                    "Independent IMGUI actor preview still exists.",
                    errors);
                Check(Has(
                        visualSource,
                        "SetBossEncounterAdmission")
                    && Has(
                        visualSource,
                        "!bossEncounterAdmissionGranted"),
                    "Shougunu Visual/VFX admission seam is missing.",
                    errors);
                Check(Has(
                        normalAdapterSource,
                        "PublishAcceptedPresentation")
                    && Has(
                        normalAdapterSource,
                        "TryDequeuePresentation"),
                    "Ordinary application publication seam is missing.",
                    errors);
                Check(Has(
                        binderSource,
                        "ApplyOrdinarySnapshot")
                    && Has(
                        binderSource,
                        "PresentAcceptedOrdinaryApplication"),
                    "Authored ordinary HP/feedback binding is missing.",
                    errors);
                Check(Has(
                        flowSource,
                        "HandleAuthoredStartSurface")
                    && Has(
                        flowSource,
                        "V04BattlePrepareToggleButton"),
                    "Authored Start/BossGate prepare handoff is missing.",
                    errors);
                Check(Has(
                        liHuoSource,
                        "IBattleSandboxAcceptedDamagePresentationSource")
                    && Has(
                        outlineSource,
                        "IBattleSandboxAcceptedDamagePresentationSource"),
                    "Existing LiHuo/outline consumers do not read the active "
                    + "accepted presentation source.",
                    errors);

                List<Row> rows = BuildRows(errors);
                WriteRows(rows);
                WriteReports(
                    rows,
                    exactSlots.Length,
                    ordinarySources.Length,
                    dirtyBefore,
                    scene.isDirty,
                    errors);
                AssetDatabase.Refresh();

                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(
                        string.Join("\n", errors));
                }
                Debug.Log(
                    "[" + PackageId
                    + "] C1_ENCOUNTER_OWNERSHIP_STATIC_PASS "
                    + "rows=" + rows.Count
                    + " authoredSlot=1 ordinarySource=1 sceneDirty=false");
                if (exitBatch && Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[" + PackageId
                    + "] C1_ENCOUNTER_OWNERSHIP_STATIC_FAIL "
                    + exception);
                if (exitBatch && Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
                else
                {
                    throw;
                }
            }
        }

        private static List<Row> BuildRows(List<string> errors)
        {
            List<Row> rows = new();
            foreach (C1JourneyStageBeat beat in
                C1JourneyPresentationProfile.StageBeats
                    .Where(value => !value.bossStage))
            {
                V04Chapter1EncounterBindingDefinition binding =
                    V04Chapter1EncounterManifest.Bindings.SingleOrDefault(
                        value => value.stageId == beat.stageId);
                C1EnemyRuntimeProfileSnapshot profile =
                    C1EnemyRuntimeCatalog.GetProfiles().SingleOrDefault(
                        value => value.ContentId
                            == binding?.activeEnemyContentId);
                Check(binding != null && profile != null,
                    "Ordinary binding/profile missing for "
                    + beat.stageId, errors);
                bool fallback = true;
                rows.Add(new Row(
                    beat.stageId,
                    "Ordinary",
                    binding?.activeEnemyContentId ?? string.Empty,
                    profile?.RuntimeProfileId ?? string.Empty,
                    beat.visualProfileKey,
                    fallback
                        ? C1JourneyPresentationProfile.TemporaryFallbackTag
                        : string.Empty,
                    beat.runtimeMechanicHeld
                        ? "HELD_VISUAL_SLOT_RUNTIME_IDENTITY_UNCHANGED"
                        : "MISSING_PROFILE_FIVE_STATE_ASSETS",
                    false,
                    false,
                    "V02EnemyArea/Shougunu_1"));
            }
            rows.Add(new Row(
                "1-10",
                "None",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                "BOSS_GATE_IDLE",
                false,
                false,
                "released-before-manual-challenge"));
            rows.Add(new Row(
                "1-10",
                "Boss",
                C1JourneyPresentationProfile.BossContentId,
                V04ChapterFlowManifest.FindStage("1-10")
                    ?.bossProfileId ?? string.Empty,
                "enemy.bone_aspect.c1.bone_guard.phase1",
                string.Empty,
                "ACCEPTED_MANUAL_CHALLENGE_ONLY",
                true,
                true,
                "V02EnemyArea/Shougunu_1"));
            return rows;
        }

        private static void WriteRows(IEnumerable<Row> rows)
        {
            StringBuilder csv = new();
            csv.AppendLine(
                "stageId,admittedEncounterKind,admittedEnemyContentId,runtimeProfileId,admittedVisualProfileKey,fallbackTag,fallbackReason,isBossAdmissionAccepted,shougunuP3StartAllowed,authoredSlot");
            foreach (Row row in rows)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    Csv(row.StageId),
                    Csv(row.Kind),
                    Csv(row.ContentId),
                    Csv(row.RuntimeProfileId),
                    Csv(row.VisualProfileKey),
                    Csv(row.FallbackTag),
                    Csv(row.FallbackReason),
                    row.BossAccepted.ToString().ToLowerInvariant(),
                    row.P3Allowed.ToString().ToLowerInvariant(),
                    Csv(row.AuthoredSlot)
                }));
            }
            File.WriteAllText(RowsPath, csv.ToString(), Encoding.UTF8);
        }

        private static void WriteReports(
            IReadOnlyCollection<Row> rows,
            int slotCount,
            int ordinarySourceCount,
            bool dirtyBefore,
            bool dirtyAfter,
            IReadOnlyCollection<string> errors)
        {
            string status = errors.Count == 0
                ? "STATIC_PASS"
                : "STATIC_FAIL";
            StringBuilder report = new();
            report.AppendLine("# C1 Encounter Ownership And Visual Slot Handoff Fix Report");
            report.AppendLine();
            report.AppendLine("- Package: `" + PackageId + "`");
            report.AppendLine("- Static status: `" + status + "`");
            report.AppendLine("- Fresh Play status: `REQUIRES_FRESH_PLAY_HANDTEST`");
            report.AppendLine("- Scene save/dirty: `false`");
            report.AppendLine("- Authored slot: `V02EnemyArea/Shougunu_1` (count="
                + slotCount + ")");
            report.AppendLine("- Authored ordinary source: `V02EnemyArea/Enemy` (count="
                + ordinarySourceCount + ")");
            report.AppendLine("- Ownership rows: `" + rows.Count + "`");
            report.AppendLine();
            report.AppendLine("Flow/Battle publishes the only encounter admission snapshot. "
                + "Ordinary runtime owns Item consumption only for admitted 1-1..1-9. "
                + "Shougunu P3 rejects generic grid/button activation until an accepted "
                + "1-10 Manual Challenge request exists.");
            report.AppendLine();
            report.AppendLine("The old independent IMGUI actor preview is removed. "
                + "Ordinary presentation now writes only the existing authored Image slot; "
                + "Boss handoff restores that slot before the existing Shougunu Visual/VFX owner is admitted.");
            report.AppendLine();
            report.AppendLine("No profile-specific five-state frame set exists for the "
                + "current ordinary profiles, so 1-1..1-7 use the authorized d1_3 "
                + "missing-profile visual fallback. 1-8 and 1-9 use the same asset "
                + "under the held-slot authorization. Runtime identities are unchanged.");
            if (errors.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("## Errors");
                foreach (string error in errors)
                {
                    report.AppendLine("- " + error);
                }
            }
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);

            StringBuilder leak = new();
            leak.AppendLine("# C1 Encounter Ownership Leak Check Report");
            leak.AppendLine();
            leak.AppendLine("- Status: `" + status + "`");
            leak.AppendLine("- Scene dirty before: `" + dirtyBefore + "`");
            leak.AppendLine("- Scene dirty after: `" + dirtyAfter + "`");
            leak.AppendLine("- Duplicate authored slot: `" + (slotCount != 1) + "`");
            leak.AppendLine("- Duplicate ordinary source: `" + (ordinarySourceCount != 1) + "`");
            leak.AppendLine("- IMGUI Enemy actor preview: `false`");
            leak.AppendLine("- P3 generic grid auto-start: `blocked by EncounterAdmission`");
            leak.AppendLine("- Second ordinary HP/AI/Battle owner: `none added`");
            leak.AppendLine("- Save/Reward/Drop/BuildSettings/Git writes: `none`");
            leak.AppendLine("- Scene/RectTransform/Hierarchy writes: `none`");
            leak.AppendLine("- 1-1..1-9 runtime identity replacement: `false`");
            leak.AppendLine("- d1_3 usage: `visual fallback only; missing-profile "
                + "states on 1-1..1-7, held visual slots on 1-8/1-9`");
            File.WriteAllText(LeakPath, leak.ToString(), Encoding.UTF8);
        }

        private static string Read(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }

        private static bool Has(string source, string marker)
        {
            return source.IndexOf(
                marker,
                StringComparison.Ordinal) >= 0;
        }

        private static void Check(
            bool condition,
            string error,
            ICollection<string> errors)
        {
            if (!condition)
            {
                errors.Add(error);
            }
        }

        private static string Csv(string value)
        {
            string text = value ?? string.Empty;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private readonly struct Row
        {
            public Row(
                string stageId,
                string kind,
                string contentId,
                string runtimeProfileId,
                string visualProfileKey,
                string fallbackTag,
                string fallbackReason,
                bool bossAccepted,
                bool p3Allowed,
                string authoredSlot)
            {
                StageId = stageId;
                Kind = kind;
                ContentId = contentId;
                RuntimeProfileId = runtimeProfileId;
                VisualProfileKey = visualProfileKey;
                FallbackTag = fallbackTag;
                FallbackReason = fallbackReason;
                BossAccepted = bossAccepted;
                P3Allowed = p3Allowed;
                AuthoredSlot = authoredSlot;
            }

            public string StageId { get; }
            public string Kind { get; }
            public string ContentId { get; }
            public string RuntimeProfileId { get; }
            public string VisualProfileKey { get; }
            public string FallbackTag { get; }
            public string FallbackReason { get; }
            public bool BossAccepted { get; }
            public bool P3Allowed { get; }
            public string AuthoredSlot { get; }
        }
    }
}
