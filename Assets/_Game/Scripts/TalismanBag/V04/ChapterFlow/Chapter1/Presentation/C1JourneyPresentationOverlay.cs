using System;
using UnityEngine;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(740)]
    public sealed class C1JourneyPresentationOverlay : MonoBehaviour
    {
        public const KeyCode PresentationToggleKey = KeyCode.F9;
        public const KeyCode PanelToggleKey = KeyCode.F10;

        private C1JourneyPresentationController controller;
        private bool diagnosticsVisible;
        private bool presentationVisible;
        private bool panelVisible;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle tagStyle;

        public bool DiagnosticsVisible => diagnosticsVisible;

        public void Bind(C1JourneyPresentationController value)
        {
            controller = value;
        }

        public void SetDiagnosticsVisible(bool visible)
        {
            diagnosticsVisible = visible;
            presentationVisible = visible;
            panelVisible = visible;
        }

        private void Update()
        {
            if (!diagnosticsVisible)
            {
                return;
            }
            if (Input.GetKeyDown(PresentationToggleKey))
            {
                presentationVisible = !presentationVisible;
            }
            if (Input.GetKeyDown(PanelToggleKey))
            {
                panelVisible = !panelVisible;
            }
        }

        private void OnGUI()
        {
            if (!diagnosticsVisible)
            {
                return;
            }
            EnsureStyles();
            if (presentationVisible && controller != null)
            {
                DrawJourneyStage();
                if (panelVisible)
                {
                    DrawDeveloperPanel();
                }
            }
            DrawControlDock();
        }

        private void DrawControlDock()
        {
            float width = Mathf.Min(520f, Screen.width - 20f);
            Rect dock = new(
                Screen.width - width - 10f,
                10f,
                width,
                118f);
            DrawRect(dock, new Color(0.02f, 0.02f, 0.025f, 0.94f));
            float buttonWidth = (dock.width - 32f) / 3f;
            if (GUI.Button(new Rect(
                    dock.x + 8f,
                    dock.y + 8f,
                    buttonWidth,
                    30f),
                "连续战斗面板"))
            {
                controller?.ToggleFlowPanel();
            }
            if (GUI.Button(new Rect(
                    dock.x + 12f + buttonWidth,
                    dock.y + 8f,
                    buttonWidth,
                    30f),
                presentationVisible
                    ? "旅程表现：显示"
                    : "旅程表现：隐藏"))
            {
                presentationVisible = !presentationVisible;
            }
            if (GUI.Button(new Rect(
                    dock.x + 16f + buttonWidth * 2f,
                    dock.y + 8f,
                    buttonWidth,
                    30f),
                panelVisible
                    ? "开发面板：显示"
                    : "开发面板：隐藏"))
            {
                panelVisible = !panelVisible;
            }
            if (GUI.Button(new Rect(
                    dock.x + 8f,
                    dock.y + 43f,
                    buttonWidth,
                    30f),
                "1. 开始第一章"))
            {
                controller?.RequestStartChapter1Session();
            }
            if (GUI.Button(new Rect(
                    dock.x + 12f + buttonWidth,
                    dock.y + 43f,
                    buttonWidth,
                    30f),
                "2. 开始当前关"))
            {
                controller?.RequestStartCurrentNormalStage();
            }
            if (GUI.Button(new Rect(
                    dock.x + 16f + buttonWidth * 2f,
                    dock.y + 43f,
                    buttonWidth,
                    30f),
                "3. 继续 / 下一关"))
            {
                controller?.RequestContinueFromCurrentBoundary();
            }
            GUI.Label(
                new Rect(
                    dock.x + 10f,
                    dock.y + 78f,
                    dock.width - 20f,
                    32f),
                controller?.FlowStatus ?? "Flow: NOT_BOUND",
                bodyStyle);
        }

        private void DrawJourneyStage()
        {
            C1JourneyStageBeat beat = controller.CurrentBeat;
            if (beat == null)
            {
                DrawRect(
                    new Rect(0f, 0f, Screen.width, 68f),
                    new Color(0.04f, 0.04f, 0.05f, 0.92f));
                GUI.Label(
                    new Rect(22f, 12f, Screen.width - 44f, 46f),
                    "V0.4 第一章三段旅程 · 使用右上角控制条打开连续战斗面板并启动 Chapter 1",
                    titleStyle);
                return;
            }
            DrawRect(
                new Rect(0f, 0f, Screen.width, 78f),
                new Color(0.025f, 0.025f, 0.03f, 0.90f));
            GUI.Label(
                new Rect(22f, 12f, Screen.width - 44f, 38f),
                "第一章 · " + beat.stageId + " · "
                    + beat.segmentDisplayName,
                titleStyle);
            GUI.Label(
                new Rect(24f, 48f, Screen.width - 48f, 26f),
                "Encounter Node: " + beat.encounterNodeId
                    + " · Authored Slot: "
                    + controller.AuthoredSlotPath
                    + " · owns=" + controller.OwnsAuthoredSlot,
                bodyStyle);

            if (beat.transitionBeat != C1JourneyTransitionBeat.Hold
                && controller.BeatElapsed < 1.2f)
            {
                float alpha = 1f - Mathf.Clamp01(
                    controller.BeatElapsed / 1.2f);
                DrawRect(
                    new Rect(
                        Screen.width * 0.22f,
                        86f,
                        Screen.width * 0.56f,
                        74f),
                    new Color(0.01f, 0.01f, 0.015f, alpha * 0.88f));
                GUI.Label(
                    new Rect(
                        Screen.width * 0.23f,
                        90f,
                        Screen.width * 0.54f,
                        66f),
                    BeatLabel(beat.transitionBeat),
                    titleStyle);
            }
        }

        private void DrawDeveloperPanel()
        {
            C1JourneyStageBeat beat = controller.CurrentBeat;
            Rect panel = new(
                18f,
                Screen.height - 252f,
                Mathf.Min(620f, Screen.width - 36f),
                234f);
            DrawRect(panel, new Color(0.025f, 0.025f, 0.03f, 0.94f));
            GUILayout.BeginArea(new Rect(
                panel.x + 12f,
                panel.y + 10f,
                panel.width - 24f,
                panel.height - 20f));
            GUILayout.Label(
                "DEV ONLY · AUTHORED SLOT HANDOFF · F9 状态栏 / F10 面板",
                tagStyle);
            if (beat != null)
            {
                GUILayout.Label(
                    "Runtime content: " + beat.runtimeContentId,
                    bodyStyle);
                GUILayout.Label(
                    "Presentation identity: " + beat.presentationContentId,
                    bodyStyle);
                GUILayout.Label(
                    "visualProfileKey: " + beat.visualProfileKey,
                    bodyStyle);
                if (!string.IsNullOrEmpty(beat.fallbackTag))
                {
                    GUILayout.Label(
                        beat.fallbackTag
                            + " · Runtime remains "
                            + beat.runtimeContentId
                            + " · " + C1JourneyPresentationProfile
                                .ThirdEnemyRuntimeStatus,
                        tagStyle);
                }
            }
            GUILayout.Label(
                "Visual state: " + controller.CurrentState
                    + " · " + controller.Diagnostic,
                bodyStyle);
            GUILayout.BeginHorizontal();
            foreach (C1JourneyEnemyVisualState state in
                C1JourneyPresentationProfile.FiveEnemyStates)
            {
                if (GUILayout.Button(
                    state.ToString(),
                    GUILayout.Height(30f)))
                {
                    controller.PreviewState(state);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(
                "五按钮只驱动 admitted ordinary authored-slot cue；不写 Flow、Enemy Runtime、Battle、Save。Boss admission 时按钮会被拒绝。",
                bodyStyle);
            GUILayout.EndArea();
        }

        private static string BeatLabel(C1JourneyTransitionBeat beat)
        {
            return beat switch
            {
                C1JourneyTransitionBeat.ChapterStartToSegmentA =>
                    "CHAPTER START\n丁巷外沿 · Segment A Reveal",
                C1JourneyTransitionBeat.SegmentAToSegmentB =>
                    "A → B\n骨器铺外巷",
                C1JourneyTransitionBeat.SegmentBToSegmentC =>
                    "B → C\n骨器铺内堂",
                C1JourneyTransitionBeat.Stage19ExitToBossGateAndReveal =>
                    "1-9 ENCOUNTER EXIT\n1-10 Boss Gate → Boss Reveal",
                _ => string.Empty
            };
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.96f, 0.88f, 0.72f) }
            };
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(0.92f, 0.92f, 0.90f) }
            };
            tagStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.62f, 0.22f) }
            };
        }
    }
}
