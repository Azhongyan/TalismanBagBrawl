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
        [SerializeField] private GameObject homeUpgradeGuideRoot;
        [SerializeField] private GameObject homeTrialGuideRoot;
        [SerializeField] private Text homeUpgradeImageSlotText;
        [SerializeField] private Text homeTrialImageSlotText;

        private MainTrialFlowService mainTrialFlowService;

        public void Initialize(
            V03NavigationFlowController navigationController,
            MainHomeGreyboxPanel panel,
            GameObject refineRoot)
        {
            navigation = navigationController;
            homePanel = panel;
            uiParent = ResolveUiParent(panel, navigationController);
            BindGuideRoots();
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
            SetGuideRootActive(homeUpgradeGuideRoot, false);
            if (homeTrialGuideRoot != homeUpgradeGuideRoot)
            {
                SetGuideRootActive(homeTrialGuideRoot, false);
            }
        }

        private MainTrialPhase GetCurrentPhase()
        {
            MainTrialFlowService flowService = ResolveMainTrialFlowService();
            return flowService != null
                ? flowService.GetCurrentPhase()
                : MainTrialPhase.NotStarted;
        }

        private MainTrialFlowService ResolveMainTrialFlowService()
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

            Debug.LogError(
                "[V0.3-BootGuideUpgradeRuntimeLock01] MainTrialFlowService is missing; " +
                "runtime service creation is disabled.",
                this);
            return null;
        }

        private void ShowGuideImageSlot(string slotName, string slotText)
        {
            GameObject root = ResolveGuideRoot(slotName);
            if (root == null)
            {
                Debug.LogError(
                    $"[V0.3-BootGuideUpgradeRuntimeLock01] MainHome guide slot '{slotName}' is missing; " +
                    "runtime guide overlay creation is disabled.",
                    this);
                return;
            }

            HideGuideSlot();
            root.SetActive(true);

            Text slotTextComponent = string.Equals(
                    slotName,
                    "V03_GuideImageSlot_HomeTrial",
                    System.StringComparison.Ordinal)
                ? homeTrialImageSlotText
                : homeUpgradeImageSlotText;
            if (slotTextComponent != null)
            {
                slotTextComponent.text = slotText;
            }
        }

        private GameObject ResolveGuideRoot(string slotName)
        {
            BindGuideRoots();
            if (string.Equals(slotName, "V03_GuideImageSlot_HomeTrial", System.StringComparison.Ordinal))
            {
                return homeTrialGuideRoot;
            }

            return homeUpgradeGuideRoot;
        }

        private void BindGuideRoots()
        {
            if (uiParent == null)
            {
                uiParent = ResolveUiParent(homePanel, navigation);
            }

            if (uiParent == null)
            {
                return;
            }

            homeUpgradeGuideRoot ??= FindGameObject(uiParent, "V03_GuideImageSlot_HomeUpgrade");
            homeTrialGuideRoot ??= FindGameObject(uiParent, "V03_GuideImageSlot_HomeTrial");

            GameObject sharedRoot = FindGameObject(uiParent, "V03_GuideImageSlotRoot");
            homeUpgradeGuideRoot ??= sharedRoot;
            homeTrialGuideRoot ??= sharedRoot;

            homeUpgradeImageSlotText ??= FindText(homeUpgradeGuideRoot, "V03_GuideImageSlotText");
            homeTrialImageSlotText ??= FindText(homeTrialGuideRoot, "V03_GuideImageSlotText");
            homeUpgradeImageSlotText ??= FindText(homeUpgradeGuideRoot, "Text");
            homeTrialImageSlotText ??= FindText(homeTrialGuideRoot, "Text");
        }

        private static void SetGuideRootActive(GameObject root, bool active)
        {
            if (root != null && root.activeSelf != active)
            {
                root.SetActive(active);
            }
        }

        private static GameObject FindGameObject(Transform parent, string objectName)
        {
            Transform found = FindDeepChild(parent, objectName);
            return found != null ? found.gameObject : null;
        }

        private static Text FindText(GameObject root, string objectName)
        {
            Transform found = root != null ? FindDeepChild(root.transform, objectName) : null;
            return found != null ? found.GetComponent<Text>() : null;
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
