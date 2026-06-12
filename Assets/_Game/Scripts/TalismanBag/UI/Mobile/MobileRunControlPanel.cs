using TalismanBag.Run;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI.Mobile
{
    public sealed class MobileRunControlPanel : MonoBehaviour
    {
        [SerializeField] private RunFlowControllerV2 runFlowController;
        [SerializeField] private Button startCombatButton;
        [SerializeField] private Button nextRoundButton;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            startCombatButton?.onClick.AddListener(() => runFlowController?.StartCombat());
            nextRoundButton?.onClick.AddListener(() => runFlowController?.ContinueToNextRound());
            restartButton?.onClick.AddListener(() => runFlowController?.RestartRun());
        }

        private void Update()
        {
            if (runFlowController != null)
            {
                Refresh(runFlowController.State);
            }
        }

        public void Refresh(RunState state)
        {
            bool showStart = state == RunState.Prep || state == RunState.Boss;
            bool showNext = state == RunState.RoundWin || state == RunState.Reward || state == RunState.Shop || state == RunState.BagUpgrade;
            bool showRestart = state != RunState.None;

            SetButtonVisible(startCombatButton, showStart);
            SetButtonVisible(nextRoundButton, showNext);
            SetButtonVisible(restartButton, showRestart);
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }
    }
}
