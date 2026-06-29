using System.Collections;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V03.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.BootEntry
{
    public sealed class V03BootEntryFlowController : MonoBehaviour
    {
        public const string HomeSceneName = "Scene_TalismanBag_V03_MainHome";
        public const string HomeScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity";

        [SerializeField] private float loadingSeconds = 0.75f;
        [SerializeField] private string serverLabel = "青石坡一区";
        [SerializeField] private string serverStatus = "区服占位 · 当前仅作本地流程演示";
        [SerializeField] private Canvas bootCanvas;
        [SerializeField] private GameObject loadingPage;
        [SerializeField] private GameObject startGamePage;
        [SerializeField] private GameObject openingStoryPanel;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button skipOpeningStoryButton;
        [SerializeField] private Text serverLabelText;
        [SerializeField] private Text serverStatusText;

        private bool isKnownPlayer;
        private bool sceneBindingsReady;
        private bool buttonsBound;

        private void Awake()
        {
            sceneBindingsReady = BindSceneObjects();
            if (sceneBindingsReady)
            {
                BindButtons();
                UpdateServerTexts();
            }
        }

        private void Start()
        {
            if (!sceneBindingsReady)
            {
                Debug.LogError(
                    "[V0.3-BootGuideUpgradeRuntimeLock01] BootEntry scene nodes are incomplete; " +
                    "runtime page creation is disabled.",
                    this);
                return;
            }

            isKnownPlayer = SaveService.GetOrCreate().HasSave();
            ShowOnly(loadingPage);
            StartCoroutine(EnterStartPageAfterLoading());
        }

        private void OnDestroy()
        {
            if (!buttonsBound)
            {
                return;
            }

            startGameButton?.onClick.RemoveListener(StartGame);
            skipOpeningStoryButton?.onClick.RemoveListener(SkipOpeningStory);
        }

        private IEnumerator EnterStartPageAfterLoading()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, loadingSeconds));
            ShowStartGamePage();
        }

        private void ShowStartGamePage()
        {
            ShowOnly(startGamePage);
        }

        private void StartGame()
        {
            if (isKnownPlayer)
            {
                LoadHome();
                return;
            }

            ShowOnly(openingStoryPanel);
        }

        private void SkipOpeningStory()
        {
            LoadTrial();
        }

        private void LoadTrial()
        {
            LoadScene(V03NavigationFlowController.TrialScenePath, V03NavigationFlowController.TrialSceneName);
        }

        private void LoadHome()
        {
            LoadScene(HomeScenePath, HomeSceneName);
        }

        private void LoadScene(string scenePath, string sceneName)
        {
            if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
            {
                Debug.LogError($"[V0.3-BootEntryFlow01] Scene is missing from Build Settings: {scenePath}", this);
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        private bool BindSceneObjects()
        {
            bootCanvas ??= GetComponentInChildren<Canvas>(true);
            Transform root = bootCanvas != null ? bootCanvas.transform : transform;

            loadingPage ??= FindGameObject(root, "LoadingPage");
            startGamePage ??= FindGameObject(root, "StartGamePage");
            openingStoryPanel ??= FindGameObject(root, "OpeningStoryPanel");
            startGameButton ??= FindButton(startGamePage != null ? startGamePage.transform : root, "开始游戏Button");
            startGameButton ??= FindButton(startGamePage != null ? startGamePage.transform : root, "StartGameButton");
            skipOpeningStoryButton ??= FindButton(
                openingStoryPanel != null ? openingStoryPanel.transform : root,
                "跳过Button");
            skipOpeningStoryButton ??= FindButton(
                openingStoryPanel != null ? openingStoryPanel.transform : root,
                "SkipOpeningStoryButton");

            bool ok = bootCanvas != null &&
                      loadingPage != null &&
                      startGamePage != null &&
                      openingStoryPanel != null &&
                      startGameButton != null &&
                      skipOpeningStoryButton != null;

            if (!ok)
            {
                Debug.LogError(
                    "[V0.3-BootGuideUpgradeRuntimeLock01] BootEntry requires scene-authored " +
                    "BootEntryCanvas, LoadingPage, StartGamePage, OpeningStoryPanel, " +
                    "start button, and skip button. Runtime creation is disabled.",
                    this);
            }

            return ok;
        }

        private void BindButtons()
        {
            if (buttonsBound)
            {
                return;
            }

            startGameButton.onClick.AddListener(StartGame);
            skipOpeningStoryButton.onClick.AddListener(SkipOpeningStory);
            buttonsBound = true;
        }

        private void UpdateServerTexts()
        {
            if (serverLabelText != null)
            {
                serverLabelText.text = serverLabel;
            }

            if (serverStatusText != null)
            {
                serverStatusText.text = serverStatus;
            }
        }

        private static GameObject FindGameObject(Transform parent, string objectName)
        {
            Transform found = FindDeepChild(parent, objectName);
            return found != null ? found.gameObject : null;
        }

        private static Button FindButton(Transform parent, string objectName)
        {
            Transform found = FindDeepChild(parent, objectName);
            return found != null ? found.GetComponent<Button>() : null;
        }

        private static Transform FindDeepChild(Transform parent, string objectName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            foreach (Transform child in parent)
            {
                if (child.name == objectName)
                {
                    return child;
                }

                Transform found = FindDeepChild(child, objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void ShowOnly(GameObject activePage)
        {
            SetPageActive(loadingPage, activePage == loadingPage);
            SetPageActive(startGamePage, activePage == startGamePage);
            SetPageActive(openingStoryPanel, activePage == openingStoryPanel);
        }

        private static void SetPageActive(GameObject page, bool active)
        {
            if (page != null && page.activeSelf != active)
            {
                page.SetActive(active);
            }
        }
    }
}
