using TalismanBag.V02.UI;
using TalismanBag.V03.Forge;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.Navigation
{
    public sealed class V03NavigationFlowController : MonoBehaviour
    {
        public const string UpgradeSceneName = "Scene_TalismanBag_V03_TalismanUpgrade";
        public const string UpgradeScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V03_TalismanUpgrade.unity";
        public const string TrialSceneName = "Scene_TalismanBag_V02_FormationCounter";
        public const string TrialScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity";
        private const float BottomNavWidth = 860f;
        private const float BottomNavHeight = 124f;
        private const float BottomNavMinBottomInset = 56f;
        private const float BottomNavMaxBottomInset = 104f;
        private const float BottomNavButtonStep = 168f;
        private const float BottomNavButtonWidth = 148f;
        private const float BottomNavButtonHeight = 82f;

        [SerializeField] private GameObject refinePageRoot;
        [SerializeField] private GameObject explorePageRoot;
        [SerializeField] private GameObject morePageRoot;
        [SerializeField] private GameObject secondaryBottomNavRoot;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button trialButton;
        [SerializeField] private Button refineButton;
        [SerializeField] private Button exploreButton;
        [SerializeField] private Button moreButton;
        [SerializeField] private V03ForgeFirstUpgradeGuideController forgeGuide;

        private MainHomeGreyboxPanel homePanel;
        private string resourceSummary;
        private string homeStatus;
        private bool buttonsBound;

        private void Awake()
        {
            BindButtons();
            EnsureForgeGuide();
            ApplyBottomNavSafeLayout();
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
            EnsureForgeGuide()?.Initialize(this, homePanel, refinePageRoot);
            ApplyBottomNavSafeLayout();
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
            ApplyBottomNavSafeLayout();
            secondaryBottomNavRoot?.transform.SetAsLastSibling();
            forgeGuide?.OnHomeShown();
        }

        public void ShowRefine()
        {
            forgeGuide?.OnRefineShown();
            LoadScene(
                UpgradeScenePath,
                UpgradeSceneName,
                $"[V0.3-NavigationFlow01] Upgrade scene is missing from Build Settings: {UpgradeScenePath}");
        }

        public void ShowExplore()
        {
            ShowSecondary(explorePageRoot);
            forgeGuide?.HideGuideSlot();
        }

        public void ShowMore()
        {
            ShowSecondary(morePageRoot);
            forgeGuide?.HideGuideSlot();
        }

        public void EnterTrial()
        {
            if (forgeGuide != null && forgeGuide.ShouldBlockTrialUntilFirstUpgrade())
            {
                ShowHome();
                forgeGuide.ShowFirstUpgradeHomeGuide();
                return;
            }

            forgeGuide?.HideGuideSlot();
            LoadScene(
                TrialScenePath,
                TrialSceneName,
                $"[V0.3-NavigationFlow01] Trial scene is missing from Build Settings: {TrialScenePath}");
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

        private void LoadScene(string scenePath, string sceneName, string missingSceneMessage)
        {
            if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
            {
                Debug.LogError(missingSceneMessage, this);
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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
            ApplyBottomNavSafeLayout();
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

        private V03ForgeFirstUpgradeGuideController EnsureForgeGuide()
        {
            if (forgeGuide != null)
            {
                return forgeGuide;
            }

            forgeGuide = GetComponent<V03ForgeFirstUpgradeGuideController>();
            if (forgeGuide == null)
            {
                forgeGuide = gameObject.AddComponent<V03ForgeFirstUpgradeGuideController>();
            }

            return forgeGuide;
        }

        private void ApplyBottomNavSafeLayout()
        {
            RectTransform rect = secondaryBottomNavRoot != null
                ? secondaryBottomNavRoot.transform as RectTransform
                : null;
            if (rect == null)
            {
                return;
            }

            Canvas canvas = secondaryBottomNavRoot.GetComponentInParent<Canvas>(true);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, ResolveBottomNavInset(canvas));
            rect.sizeDelta = new Vector2(BottomNavWidth, BottomNavHeight);

            Image background = secondaryBottomNavRoot.GetComponent<Image>();
            if (background != null)
            {
                background.color = Color.clear;
                background.raycastTarget = false;
            }

            Outline outline = secondaryBottomNavRoot.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }

            ApplyBottomNavButtonRect(homeButton, -BottomNavButtonStep * 2f);
            ApplyBottomNavButtonRect(refineButton, -BottomNavButtonStep);
            ApplyBottomNavButtonRect(trialButton, 0f);
            ApplyBottomNavButtonRect(exploreButton, BottomNavButtonStep);
            ApplyBottomNavButtonRect(moreButton, BottomNavButtonStep * 2f);
        }

        private static void ApplyBottomNavButtonRect(Button button, float x)
        {
            RectTransform rect = button != null ? button.transform as RectTransform : null;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(BottomNavButtonWidth, BottomNavButtonHeight);
        }

        private static float ResolveBottomNavInset(Canvas canvas)
        {
            float scaleFactor = canvas != null && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            float safeBottom = Screen.safeArea.yMin / scaleFactor;
            return Mathf.Clamp(
                safeBottom + 18f,
                BottomNavMinBottomInset,
                BottomNavMaxBottomInset);
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
