using System;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using UnityEngine;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(730)]
    public sealed class V04Chapter1BattleSandboxOverlay : MonoBehaviour
    {
        public const KeyCode VisibilityToggleKey = KeyCode.F6;
        private const string PackageId =
            "V0.4-DevPanelRuntimeRootSeparationLightFix01";

        private V04Chapter1BattleSandboxRuntimeController controller;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle warningStyle;
        private bool diagnosticsVisible;
        private bool panelVisible;

        public bool IsVisible => diagnosticsVisible && panelVisible;
        public bool DiagnosticsVisible => diagnosticsVisible;

        public void Bind(
            V04Chapter1BattleSandboxRuntimeController runtimeController)
        {
            controller = runtimeController;
        }

        public void SetPanelVisible(bool visible)
        {
            panelVisible = visible;
        }

        public void SetDiagnosticsVisible(bool visible)
        {
            diagnosticsVisible = visible;
            if (!visible)
            {
                panelVisible = false;
            }
        }

        private void Awake()
        {
            diagnosticsVisible = false;
            panelVisible = false;
        }

        private void Update()
        {
            if (!diagnosticsVisible
                || !Input.GetKeyDown(VisibilityToggleKey))
            {
                return;
            }
            SetPanelVisible(!panelVisible);
            Debug.Log(
                "[" + PackageId + "] F6 B-Line panel = "
                + (panelVisible ? "VISIBLE" : "HIDDEN"));
        }

        private void OnGUI()
        {
            if (!diagnosticsVisible
                || !panelVisible
                || controller == null)
            {
                return;
            }
            EnsureStyles();

            Rect panel = new Rect(18f, 18f, 470f, 650f);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(
                panel.x + 14f,
                panel.y + 12f,
                panel.width - 28f,
                panel.height - 24f));
            GUILayout.Label("V0.4 · B-Line Chapter 1 Continuous Battle", titleStyle);
            GUILayout.Label(
                "DEV ONLY · DEFAULT OFF · NOT FORMAL FLOW",
                warningStyle);
            GUILayout.Space(5f);

            string stage = controller.Snapshot?.currentStageId ?? "-";
            string phase = controller.Snapshot?.phase.ToString() ?? "Disabled";
            GUILayout.Label("Stage / Phase: " + stage + " / " + phase, bodyStyle);
            if (stage == "1-10")
            {
                GUILayout.Label(
                    "Boss readiness: "
                    + V04Chapter1BossPhase1CompletionAdapter
                        .RequiredBossItemChain,
                    warningStyle);
                GUILayout.Label(
                    phase == V04ChapterFlowPhase.BossGate.ToString()
                        ? "Keep the board editable at BossGate. Manual Challenge "
                            + "will not advance ChapterFlow until every prerequisite passes."
                        : "Boss handoff uses the existing authored Button; "
                            + "direct Runtime-only toggling is forbidden.",
                    bodyStyle);
            }

            C1EnemyRuntimeSnapshot enemy = controller.NormalBattle?.Enemy;
            if (enemy != null)
            {
                GUILayout.Label(
                    "Enemy content: " + enemy.ContentId,
                    bodyStyle);
                GUILayout.Label(
                    "Runtime profile: " + enemy.RuntimeProfileId,
                    bodyStyle);
                GUILayout.Label(
                    "Presentation: " + enemy.PresentationKey,
                    bodyStyle);
                GUILayout.Label(
                    string.Format(
                        "Enemy HP {0}/{1} · Shell {2}/{3} · Player HP {4}/{5}",
                        enemy.CurrentHp,
                        enemy.MaxHp,
                        enemy.CurrentShell,
                        enemy.ShellMax,
                        controller.NormalBattle.PlayerCurrentHp,
                        V04Chapter1ContinuousBattleIntegrationContract
                            .PlayerMaxHpFixture),
                    bodyStyle);
            }
            else
            {
                V04Chapter1EncounterBindingDefinition binding =
                    V04Chapter1EncounterManifest.Bindings.FirstOrDefault(
                        value => value.stageId == stage);
                C1EnemyRuntimeProfileSnapshot profile =
                    C1EnemyRuntimeCatalog.GetProfiles().FirstOrDefault(
                        value => value.ContentId ==
                            binding?.activeEnemyContentId);
                if (binding != null && profile != null)
                {
                    GUILayout.Label(
                        "Next enemy: " + binding.activeEnemyContentId,
                        bodyStyle);
                    GUILayout.Label(
                        "Profile / Presentation: "
                        + profile.RuntimeProfileId + " / "
                        + profile.PresentationKey,
                        bodyStyle);
                }
            }

            GUILayout.Space(5f);
            GUILayout.Label(
                "Diagnostic: " + controller.LastDiagnosticCode,
                warningStyle);
            GUILayout.Label(controller.LastDiagnosticDetail, bodyStyle);
            GUILayout.Space(8f);

            if (GUILayout.Button("Start Chapter 1 Session", GUILayout.Height(34f)))
            {
                controller.StartChapter1Session();
            }
            if (GUILayout.Button(
                "Start Current Normal Stage",
                GUILayout.Height(34f)))
            {
                controller.StartCurrentNormalStage();
            }
            if (GUILayout.Button(
                "Continue From Settlement / Drop Candidate",
                GUILayout.Height(34f)))
            {
                controller.ContinueFromSettlementOrDropCandidate();
            }
            if (GUILayout.Button(
                "Manual Challenge Shougunu Phase1 (requires I007-I012 + I031)",
                GUILayout.Height(34f)))
            {
                controller.ManualChallengeShougunuPhase1();
            }
            if (GUILayout.Button("Reset Dev Session", GUILayout.Height(34f)))
            {
                controller.ResetDevSession();
            }

            GUILayout.Space(10f);
            GUILayout.Label(
                V04Chapter1ContinuousBattleIntegrationContract
                    .Phase1DevVerticalSlice,
                warningStyle);
            GUILayout.Label(
                V04Chapter1ContinuousBattleIntegrationContract
                    .NotContentFinal,
                warningStyle);
            GUILayout.Label(
                V04Chapter1ContinuousBattleIntegrationContract
                    .NotFormalBossCompletion,
                warningStyle);
            GUILayout.Label(
                "Drop/Reward/Inventory/Save writes: 0 / 0 / 0 / 0",
                bodyStyle);
            GUILayout.Label(
                controller.IsSessionOnlyChapter2Unlocked
                    ? "Chapter 1 complete · Chapter 2 unlocked for this session"
                    : "Chapter 2 remains locked outside this Play session",
                bodyStyle);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true
            };
            warningStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.72f, 0.25f) }
            };
        }
    }
}
