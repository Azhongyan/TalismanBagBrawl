using TalismanBag.V02.UI;
using TalismanBag.V03.Forge;
using TalismanBag.V03.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.MainHome
{
    public sealed class V03MainHomeSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private MainHomeGreyboxPanel homePanel;
        [SerializeField] private V03NavigationFlowController navigation;
        [SerializeField] private string resourceSummary =
            "灵石 0　符纸 0\n朱砂 0　初阶符胚 0\n修为 0";
        [SerializeField] private string status = "小店已开门，选择店内区域查看。";

        private V03ForgeFirstUpgradeGuideController fallbackForgeGuide;

        private void Start()
        {
            if (homePanel == null)
            {
                Debug.LogError("[V0.3-MainHome] MainHomeRoot reference is missing.", this);
                return;
            }

            EnsureFullBackgroundSlot();
            DisableHomeRootGraphics();

            if (navigation == null)
            {
                navigation = GetComponent<V03NavigationFlowController>();
            }

            if (navigation == null)
            {
                navigation = FindObjectOfType<V03NavigationFlowController>(true);
            }

            if (navigation != null)
            {
                navigation.Initialize(homePanel, resourceSummary, status);
                return;
            }

            homePanel.Show(
                MainHomeGreyboxPanel.HomeTitle,
                resourceSummary,
                status,
                OnRefineRequested,
                OnTrialRequested,
                null);
            EnsureFallbackForgeGuide()?.OnHomeShown();
        }

        private void OnRefineRequested()
        {
            EnsureFallbackForgeGuide()?.HideGuideSlot();
            LoadScene(
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.UpgradeSceneName,
                $"[V0.3-MainHome] Upgrade scene is missing from Build Settings: {V03NavigationFlowController.UpgradeScenePath}");
        }

        private void OnTrialRequested()
        {
            V03ForgeFirstUpgradeGuideController guide = EnsureFallbackForgeGuide();
            if (guide != null && guide.ShouldBlockTrialUntilFirstUpgrade())
            {
                guide.ShowFirstUpgradeHomeGuide();
                return;
            }

            guide?.HideGuideSlot();
            LoadScene(
                V03NavigationFlowController.TrialScenePath,
                V03NavigationFlowController.TrialSceneName,
                $"[V0.3-MainHome] Trial scene is missing from Build Settings: {V03NavigationFlowController.TrialScenePath}");
        }

        private V03ForgeFirstUpgradeGuideController EnsureFallbackForgeGuide()
        {
            if (fallbackForgeGuide != null)
            {
                return fallbackForgeGuide;
            }

            fallbackForgeGuide = GetComponent<V03ForgeFirstUpgradeGuideController>();
            if (fallbackForgeGuide == null)
            {
                fallbackForgeGuide = gameObject.AddComponent<V03ForgeFirstUpgradeGuideController>();
            }

            fallbackForgeGuide.Initialize(null, homePanel, null);
            return fallbackForgeGuide;
        }

        private void EnsureFullBackgroundSlot()
        {
            Canvas canvas = homePanel.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                return;
            }

            Transform existing = canvas.transform.Find("FullBackgroundImageSlot");
            GameObject slotObject = existing != null
                ? existing.gameObject
                : new GameObject("FullBackgroundImageSlot", typeof(RectTransform), typeof(Image));
            slotObject.transform.SetParent(canvas.transform, false);
            slotObject.transform.SetAsFirstSibling();
            slotObject.SetActive(true);

            RectTransform rect = slotObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = slotObject.AddComponent<RectTransform>();
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            Image image = slotObject.GetComponent<Image>();
            if (image == null)
            {
                image = slotObject.AddComponent<Image>();
            }

            image.color = Color.black;
            image.raycastTarget = false;
        }

        private void DisableHomeRootGraphics()
        {
            Image rootImage = homePanel.GetComponent<Image>();
            if (rootImage != null)
            {
                rootImage.enabled = false;
                rootImage.raycastTarget = false;
            }

            Outline rootOutline = homePanel.GetComponent<Outline>();
            if (rootOutline != null)
            {
                rootOutline.enabled = false;
            }
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
    }
}
