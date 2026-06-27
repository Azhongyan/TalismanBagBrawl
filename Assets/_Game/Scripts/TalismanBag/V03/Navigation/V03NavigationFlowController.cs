using TalismanBag.V02.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.Navigation
{
    public sealed class V03NavigationFlowController : MonoBehaviour
    {
        public const string TrialSceneName = "Scene_TalismanBag_V02_FormationCounter";
        public const string TrialScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity";

        [SerializeField] private GameObject refinePageRoot;
        [SerializeField] private GameObject explorePageRoot;
        [SerializeField] private GameObject morePageRoot;
        [SerializeField] private GameObject secondaryBottomNavRoot;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button trialButton;
        [SerializeField] private Button refineButton;
        [SerializeField] private Button exploreButton;
        [SerializeField] private Button moreButton;

        private MainHomeGreyboxPanel homePanel;
        private string resourceSummary;
        private string homeStatus;
        private bool buttonsBound;

        private void Awake()
        {
            BindButtons();
            SetSecondaryRoots(false, false, false, false);
        }

        private void OnDestroy()
        {
            if (!buttonsBound)
            {
                return;
            }

            homeButton?.onClick.RemoveListener(ShowHome);
            trialButton?.onClick.RemoveListener(EnterTrial);
            refineButton?.onClick.RemoveListener(ShowRefine);
            exploreButton?.onClick.RemoveListener(ShowExplore);
            moreButton?.onClick.RemoveListener(ShowMore);
        }

        public void Initialize(
            MainHomeGreyboxPanel panel,
            string resources,
            string status)
        {
            homePanel = panel;
            resourceSummary = resources;
            homeStatus = status;
            BindButtons();
            ShowHome();
        }

        public void ShowHome()
        {
            SetSecondaryRoots(false, false, false, true);
            if (homePanel == null)
            {
                Debug.LogError("[V0.3-NavigationFlow01] MainHomeRoot reference is missing.", this);
                return;
            }

            homePanel.Show(
                MainHomeGreyboxPanel.HomeTitle,
                resourceSummary,
                homeStatus,
                ShowRefine,
                EnterTrial,
                null);
            secondaryBottomNavRoot?.transform.SetAsLastSibling();
        }

        public void ShowRefine()
        {
            ShowSecondary(refinePageRoot);
        }

        public void ShowExplore()
        {
            ShowSecondary(explorePageRoot);
        }

        public void ShowMore()
        {
            ShowSecondary(morePageRoot);
        }

        public void EnterTrial()
        {
            if (SceneUtility.GetBuildIndexByScenePath(TrialScenePath) < 0)
            {
                Debug.LogError(
                    $"[V0.3-NavigationFlow01] Trial scene is missing from Build Settings: {TrialScenePath}",
                    this);
                return;
            }

            SceneManager.LoadScene(TrialSceneName, LoadSceneMode.Single);
        }

        private void ShowSecondary(GameObject targetRoot)
        {
            if (targetRoot == null)
            {
                Debug.LogError("[V0.3-NavigationFlow01] Secondary page reference is missing.", this);
                return;
            }

            homePanel?.Hide();
            SetSecondaryRoots(
                targetRoot == refinePageRoot,
                targetRoot == explorePageRoot,
                targetRoot == morePageRoot,
                true);
        }

        private void SetSecondaryRoots(
            bool showRefine,
            bool showExplore,
            bool showMore,
            bool showBottomNav)
        {
            SetActive(refinePageRoot, showRefine);
            SetActive(explorePageRoot, showExplore);
            SetActive(morePageRoot, showMore);
            SetActive(secondaryBottomNavRoot, showBottomNav);
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            homeButton?.onClick.AddListener(ShowHome);
            trialButton?.onClick.AddListener(EnterTrial);
            refineButton?.onClick.AddListener(ShowRefine);
            exploreButton?.onClick.AddListener(ShowExplore);
            moreButton?.onClick.AddListener(ShowMore);
            buttonsBound = true;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
