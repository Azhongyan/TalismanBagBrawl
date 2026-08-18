using TalismanBag.Items.CampaignBaseline;
using UnityEngine;

namespace TalismanBag.UnifiedBattle.Presentation.ExactPrepare
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxPrepareSurfaceView : MonoBehaviour
    {
        public static readonly Vector2 SourceOpenAnchoredPosition =
            new Vector2(0f, -54.5f);
        public static readonly Vector2 SourceClosedAnchoredPosition =
            new Vector2(0f, -804.5f);

        [SerializeField] private RectTransform motionRoot;
        [SerializeField] private CanvasGroup inputGate;
        [SerializeField] private C1ExactBattleSandboxItemBoardView boardView;
        [SerializeField] private C1ExactBattleSandboxItemTrayView trayView;

        public RectTransform MotionRoot => motionRoot;
        public CanvasGroup InputGate => inputGate;
        public C1ExactBattleSandboxItemBoardView BoardView => boardView;
        public C1ExactBattleSandboxItemTrayView TrayView => trayView;

        public bool ValidateAuthoredReferences()
        {
            return motionRoot != null
                   && motionRoot == transform
                   && inputGate != null
                   && inputGate.transform == motionRoot
                   && boardView != null
                   && trayView != null
                   && boardView.transform.IsChildOf(motionRoot)
                   && trayView.transform.IsChildOf(motionRoot)
                   && boardView.ValidateAuthoredReferences()
                   && trayView.ValidateAuthoredReferences()
                   && SourceOpenAnchoredPosition.y
                   - SourceClosedAnchoredPosition.y == 750f;
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            RectTransform configuredMotionRoot,
            CanvasGroup configuredInputGate,
            C1ExactBattleSandboxItemBoardView configuredBoardView,
            C1ExactBattleSandboxItemTrayView configuredTrayView)
        {
            motionRoot = configuredMotionRoot;
            inputGate = configuredInputGate;
            boardView = configuredBoardView;
            trayView = configuredTrayView;
        }
#endif

        public void ApplyNormalizedProgress(float normalizedProgress)
        {
            if (motionRoot == null)
            {
                return;
            }

            motionRoot.anchoredPosition = Vector2.LerpUnclamped(
                SourceClosedAnchoredPosition,
                SourceOpenAnchoredPosition,
                Mathf.Clamp01(normalizedProgress));
        }

        public void SetInputEnabled(bool enabled)
        {
            if (inputGate == null)
            {
                return;
            }

            inputGate.alpha = 1f;
            inputGate.interactable = enabled;
            inputGate.blocksRaycasts = enabled;
        }
    }
}
