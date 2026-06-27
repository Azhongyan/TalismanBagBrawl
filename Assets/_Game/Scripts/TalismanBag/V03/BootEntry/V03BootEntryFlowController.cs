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

        private Canvas canvas;
        private GameObject loadingPage;
        private GameObject startGamePage;
        private GameObject openingStoryPanel;
        private bool isKnownPlayer;

        private void Awake()
        {
            EnsureCanvas();
            BuildPages();
        }

        private void Start()
        {
            isKnownPlayer = SaveService.GetOrCreate().HasSave();
            ShowOnly(loadingPage);
            StartCoroutine(EnterStartPageAfterLoading());
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

        private void EnsureCanvas()
        {
            canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                return;
            }

            GameObject canvasObject = new("BootEntryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
        }

        private void BuildPages()
        {
            loadingPage = CreatePage("LoadingPage", new Color(0.035f, 0.047f, 0.043f, 1f));
            CreateLabel(loadingPage.transform, "符箓背包", 58, FontStyle.Bold, new Vector2(0f, 170f), new Vector2(900f, 90f));
            CreateLabel(loadingPage.transform, "照灯小铺正在点灯", 30, FontStyle.Normal, new Vector2(0f, 74f), new Vector2(840f, 56f));
            CreateLabel(loadingPage.transform, "Loading...", 24, FontStyle.Normal, new Vector2(0f, -30f), new Vector2(520f, 48f));

            startGamePage = CreatePage("StartGamePage", new Color(0.052f, 0.066f, 0.058f, 1f));
            CreateLabel(startGamePage.transform, "符箓背包", 60, FontStyle.Bold, new Vector2(0f, 310f), new Vector2(900f, 94f));
            CreateLabel(startGamePage.transform, "明箓山下 · 青石坊", 28, FontStyle.Normal, new Vector2(0f, 230f), new Vector2(760f, 54f));
            CreateInfoBox(startGamePage.transform, "区服", $"{serverLabel}\n{serverStatus}", new Vector2(0f, 76f));
            CreateButton(startGamePage.transform, "开始游戏", new Vector2(0f, -178f), StartGame);

            openingStoryPanel = CreatePage("OpeningStoryPanel", new Color(0.045f, 0.04f, 0.034f, 1f));
            CreateLabel(openingStoryPanel.transform, "开场剧情", 46, FontStyle.Bold, new Vector2(0f, 270f), new Vector2(820f, 82f));
            CreateLabel(
                openingStoryPanel.transform,
                "夜里，照灯小铺门前的铜铃自己响了一声。\n林照灯把旧符匣推到你面前：先去青石坡试一场，回来再说。",
                28,
                FontStyle.Normal,
                new Vector2(0f, 64f),
                new Vector2(840f, 240f));
            CreateButton(openingStoryPanel.transform, "跳过", new Vector2(0f, -214f), SkipOpeningStory);

            ShowOnly(loadingPage);
        }

        private GameObject CreatePage(string pageName, Color backgroundColor)
        {
            GameObject page = new(pageName, typeof(RectTransform), typeof(Image));
            page.transform.SetParent(canvas.transform, false);
            RectTransform rect = page.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            page.GetComponent<Image>().color = backgroundColor;
            return page;
        }

        private void CreateInfoBox(Transform parent, string title, string body, Vector2 position)
        {
            GameObject box = new("ServerSlot", typeof(RectTransform), typeof(Image), typeof(Outline));
            box.transform.SetParent(parent, false);
            RectTransform rect = box.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(760f, 190f);
            box.GetComponent<Image>().color = new Color(0.105f, 0.13f, 0.11f, 0.95f);
            box.GetComponent<Outline>().effectColor = new Color(0.52f, 0.58f, 0.44f, 0.85f);

            CreateLabel(box.transform, title, 24, FontStyle.Bold, new Vector2(0f, 42f), new Vector2(680f, 44f));
            CreateLabel(box.transform, body, 24, FontStyle.Normal, new Vector2(0f, -34f), new Vector2(680f, 86f));
        }

        private Text CreateLabel(
            Transform parent,
            string value,
            int fontSize,
            FontStyle fontStyle,
            Vector2 position,
            Vector2 size)
        {
            GameObject label = new("Text", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(parent, false);
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = new Color(0.92f, 0.94f, 0.84f, 1f);
            text.text = value;
            return text;
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new($"{label}Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(420f, 88f);
            buttonObject.GetComponent<Image>().color = new Color(0.58f, 0.43f, 0.22f, 1f);
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);

            Text text = CreateLabel(buttonObject.transform, label, 30, FontStyle.Bold, Vector2.zero, rect.sizeDelta);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            return button;
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
