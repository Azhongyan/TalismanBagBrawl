using TalismanBag.V02.CoreLoop.MainTrial;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.UI;
using TalismanBag.V03.Navigation;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V03.Forge
{
    public sealed class V03ForgeFirstUpgradeGuideController : MonoBehaviour
    {
        private V03NavigationFlowController navigation;
        private MainHomeGreyboxPanel homePanel;
        private Transform uiParent;
        private GameObject guideRoot;
        private Text imageSlotText;
        private MainTrialFlowService mainTrialFlowService;

        public void Initialize(
            V03NavigationFlowController navigationController,
            MainHomeGreyboxPanel panel,
            GameObject refineRoot)
        {
            navigation = navigationController;
            homePanel = panel;
            uiParent = ResolveUiParent(panel, navigationController);
            EnsureGuideRoot();
        }

        public void OnHomeShown()
        {
            MainTrialPhase phase = GetCurrentPhase();
            if (phase == MainTrialPhase.FirstUpgradeRequired ||
                phase == MainTrialPhase.Chapter1RewardClaimed)
            {
                ShowGuideImageSlot(
                    "V03_GuideImageSlot_HomeUpgrade",
                    "图片插槽占位");
                return;
            }

            if (phase == MainTrialPhase.FirstUpgradeDone)
            {
                ShowGuideImageSlot(
                    "V03_GuideImageSlot_HomeTrial",
                    "图片插槽占位");
                return;
            }

            HideGuideSlot();
        }

        public void OnRefineShown()
        {
            HideGuideSlot();
        }

        public bool ShouldBlockTrialUntilFirstUpgrade()
        {
            MainTrialPhase phase = GetCurrentPhase();
            return phase == MainTrialPhase.FirstUpgradeRequired ||
                   phase == MainTrialPhase.Chapter1RewardClaimed;
        }

        public void ShowFirstUpgradeHomeGuide()
        {
            ShowGuideImageSlot(
                "V03_GuideImageSlot_HomeUpgrade",
                "图片插槽占位");
        }

        public void HideGuideSlot()
        {
            if (guideRoot != null)
            {
                guideRoot.SetActive(false);
            }
        }

        private MainTrialPhase GetCurrentPhase()
        {
            return EnsureMainTrialFlowService().GetCurrentPhase();
        }

        private MainTrialFlowService EnsureMainTrialFlowService()
        {
            if (mainTrialFlowService != null)
            {
                return mainTrialFlowService;
            }

            mainTrialFlowService = FindObjectOfType<MainTrialFlowService>(true);
            if (mainTrialFlowService != null)
            {
                mainTrialFlowService.Bind(SaveService.GetOrCreate());
                return mainTrialFlowService;
            }

            GameObject serviceObject = new("V03MainTrialFlowService_Runtime");
            mainTrialFlowService = serviceObject.AddComponent<MainTrialFlowService>();
            mainTrialFlowService.Bind(SaveService.GetOrCreate());
            return mainTrialFlowService;
        }

        private void ShowGuideImageSlot(string slotName, string slotText)
        {
            GameObject root = EnsureGuideRoot();
            if (root == null)
            {
                return;
            }

            root.name = slotName;
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            if (imageSlotText != null)
            {
                imageSlotText.text = slotText;
            }
        }

        private GameObject EnsureGuideRoot()
        {
            if (guideRoot != null)
            {
                return guideRoot;
            }

            if (uiParent == null)
            {
                uiParent = ResolveUiParent(homePanel, navigation);
            }

            if (uiParent == null)
            {
                return null;
            }

            guideRoot = new GameObject("V03_GuideImageSlotRoot", typeof(RectTransform), typeof(CanvasGroup));
            guideRoot.transform.SetParent(uiParent, false);
            RectTransform rootRect = guideRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = Vector2.zero;

            CanvasGroup canvasGroup = guideRoot.GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            CreateMask(guideRoot.transform);
            CreateImageSlot(guideRoot.transform);
            guideRoot.SetActive(false);
            return guideRoot;
        }

        private void CreateMask(Transform parent)
        {
            GameObject maskObject = new("V03_GuideBlackMask", typeof(RectTransform), typeof(Image));
            maskObject.transform.SetParent(parent, false);
            RectTransform rect = maskObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            Image image = maskObject.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);
            image.raycastTarget = false;
        }

        private void CreateImageSlot(Transform parent)
        {
            GameObject slotObject = new("V03_GuideImageSlot", typeof(RectTransform), typeof(Image), typeof(Outline));
            slotObject.transform.SetParent(parent, false);
            RectTransform rect = slotObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -160f);
            rect.sizeDelta = new Vector2(760f, 360f);

            Image image = slotObject.GetComponent<Image>();
            image.color = new Color(0.06f, 0.065f, 0.06f, 0.88f);
            image.raycastTarget = false;

            Outline outline = slotObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.88f, 0.78f, 0.48f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);

            imageSlotText = CreateText(
                "V03_GuideImageSlotText",
                slotObject.transform,
                "图片插槽占位",
                34,
                FontStyle.Bold,
                new Color(0.92f, 0.86f, 0.66f));
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            string value,
            int fontSize,
            FontStyle style,
            Color color)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = value;
            return text;
        }

        private static Transform ResolveUiParent(
            MainHomeGreyboxPanel panel,
            V03NavigationFlowController navigationController)
        {
            Canvas canvas = panel != null
                ? panel.GetComponentInParent<Canvas>(true)
                : null;
            if (canvas != null)
            {
                return canvas.transform;
            }

            return navigationController != null ? navigationController.transform : null;
        }
    }
}
