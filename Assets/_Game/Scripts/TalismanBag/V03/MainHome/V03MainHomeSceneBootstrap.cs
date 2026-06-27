using TalismanBag.V02.UI;
using TalismanBag.V03.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.V03.MainHome
{
    public sealed class V03MainHomeSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private MainHomeGreyboxPanel homePanel;
        [SerializeField] private string resourceSummary =
            "灵石 0　符纸 0\n朱砂 0　初阶符胚 0\n修为 0";
        [SerializeField] private string status = "小店已开门，选择店内区域查看。";

        private void Start()
        {
            if (homePanel == null)
            {
                Debug.LogError("[V0.3-MainHome] MainHomeRoot reference is missing.", this);
                return;
            }

            homePanel.Show(
                MainHomeGreyboxPanel.HomeTitle,
                resourceSummary,
                status,
                OnRefineRequested,
                OnTrialRequested,
                null);
        }

        private static void OnRefineRequested()
        {
            Debug.Log("[V0.3-MainHome] 炼符入口保持当前版本占位，未接入受保护流程。");
        }

        private static void OnTrialRequested()
        {
            if (SceneUtility.GetBuildIndexByScenePath(V03NavigationFlowController.TrialScenePath) < 0)
            {
                Debug.LogError(
                    $"[V0.3-MainHome] Trial scene is missing from Build Settings: {V03NavigationFlowController.TrialScenePath}");
                return;
            }

            SceneManager.LoadScene(V03NavigationFlowController.TrialSceneName, LoadSceneMode.Single);
        }
    }
}
