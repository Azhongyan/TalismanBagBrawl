#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.V02.CoreLoop.MainTrial;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Upgrades;
using TalismanBag.V03.Forge;
using TalismanBag.V03.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.EditorTools
{
    public static class V03TalismanUpgradeSceneBuilder
    {
        private const string RootName = "TalismanUpgradeSceneRoot";
        private const string PageRootName = "V03TalismanUpgradePageRoot";
        private const string ItemTrayRootName = "V03Upgrade_TalismanListPanel";
        private const string LegacyItemTrayRootName = "V02BottomOperationArea";
        private const int ExpectedItemTraySlotCount = 40;
        private const int ExpectedPreviewCardCount = 5;

        private static readonly string[] RequiredItemTrayTabs =
        {
            "All",
            "Talisman",
            "Tool",
            "Material",
            "Consumable",
            "Special"
        };

        private static readonly string[] RequiredBottomNavButtons =
        {
            "BottomNav_Home",
            "BottomNav_Develop",
            "BottomNav_Trial",
            "BottomNav_Explore",
            "BottomNav_More"
        };

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Build Upgrade Scene")]
        public static void BuildSceneBatch()
        {
            if (!ConfirmSceneWrite(
                    "Build Upgrade Scene",
                    "This Guard Only tool rebuilds and saves:\n" +
                    V03NavigationFlowController.UpgradeScenePath + "\n\n" +
                    "It generates the editable upgrade UI, writes the scene, and may update Build Settings.\n" +
                    "Use in Edit Mode after saving or backing up open work, with Guard or user confirmation."))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new(RootName);
            V03TalismanUpgradeSceneController controller = root.AddComponent<V03TalismanUpgradeSceneController>();
            controller.RebuildEditablePreview();

            Require(
                EditorSceneManager.SaveScene(scene, V03NavigationFlowController.UpgradeScenePath),
                "Could not save V03 talisman upgrade scene.");

            AppendBuildSettingsScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            VerifyStaticScene();

            Debug.Log("[V0.3-ForgeFirstUpgradeGuide01] UPGRADE_SCENE_BUILD_SUCCESS");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[QA Only] Verify Upgrade Scene")]
        public static void VerifyStaticBatch()
        {
            EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            VerifyStaticScene();
        }

        [MenuItem("Tools/Talisman Bag/V0.3/DevelopUpgradePage01-SlotAuthoring01/[QA Only] Verify Upgrade Slot Contract")]
        public static void VerifyUpgradeSlotContractBatch()
        {
            EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            VerifyStaticScene();
            Debug.Log("[V0.3-DevelopUpgradePage01-SlotAuthoring01] UPGRADE_SLOT_CONTRACT_VERIFY_SUCCESS");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Rebuild Editable Upgrade Preview")]
        public static void RebuildEditablePreviewInOpenScene()
        {
            if (!ConfirmSceneWrite(
                    "Rebuild Editable Upgrade Preview",
                    "This Guard Only tool rewrites the editable preview UI in the open upgrade scene.\n\n" +
                    "Target scene:\n" + V03NavigationFlowController.UpgradeScenePath + "\n\n" +
                    "It writes generated UI under V03TalismanUpgradePageRoot and saves the scene.\n" +
                    "Use in Edit Mode after saving or backing up open work, with Guard or user confirmation."))
            {
                return;
            }

            V03TalismanUpgradeSceneController controller =
                UnityEngine.Object.FindObjectOfType<V03TalismanUpgradeSceneController>(true);
            Require(controller != null, "Upgrade scene controller is missing.");

            controller.RebuildEditablePreview();
            Scene scene = controller.gameObject.scene;
            Require(scene.IsValid(), "Upgrade scene is invalid.");
            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Could not save editable upgrade scene preview.");
            Debug.Log("[V0.3-ForgeFirstUpgradeGuide01] UPGRADE_EDITABLE_PREVIEW_REBUILD_SUCCESS");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Bind Upgrade Runtime Lock Services")]
        public static void BindRuntimeLockServicesBatch()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError(
                    "[V0.3-BootGuideUpgradeRuntimeLock01] Bind Upgrade Runtime Lock Services must run in Edit Mode. " +
                    "Exit Play Mode and rerun the menu item.");
                return;
            }

            if (!ConfirmSceneWrite(
                    "Bind Upgrade Runtime Lock Services",
                    "Use only for Scene_TalismanBag_V03_TalismanUpgrade, in Edit Mode.\n\n" +
                    "This writes scene nodes if missing:\n" +
                    "- V03Upgrade_ResourceService\n" +
                    "- V03Upgrade_UpgradeService\n" +
                    "- V03Upgrade_MainTrialFlowService\n" +
                    "- EventSystem\n\n" +
                    "Target scene:\n" + V03NavigationFlowController.UpgradeScenePath + "\n\n" +
                    "Run only with Guard or user confirmation, after saving or backing up open work.\n" +
                    "After it finishes, press Ctrl+S once to make the scene save state explicit."))
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            GameObject root = scene
                .GetRootGameObjects()
                .FirstOrDefault(gameObject => gameObject.name == RootName);
            Require(root != null, "Upgrade scene root is missing.");
            Require(
                root.GetComponent<V03TalismanUpgradeSceneController>() != null,
                "Upgrade scene controller is missing.");

            _ = EnsureSceneService<ResourceService>(root.transform, "V03Upgrade_ResourceService");
            _ = EnsureSceneService<UpgradeService>(root.transform, "V03Upgrade_UpgradeService");
            _ = EnsureSceneService<MainTrialFlowService>(root.transform, "V03Upgrade_MainTrialFlowService");
            _ = EnsureEventSystem(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            Require(EditorSceneManager.SaveScene(scene), "Could not save upgrade runtime-lock service nodes.");
            AssetDatabase.SaveAssets();
            Debug.Log("[V0.3-BootGuideUpgradeRuntimeLock01] UPGRADE_RUNTIME_LOCK_SCENE_NODES_BOUND");
        }

        [MenuItem("Tools/Talisman Bag/V0.3/ForgeFirstUpgradeGuide01/[Writes Scene][Guard Only] Bind Upgrade Runtime Lock Services", true)]
        private static bool CanBindRuntimeLockServicesBatch()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        public static void VerifyStaticScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != V03NavigationFlowController.UpgradeScenePath)
            {
                scene = EditorSceneManager.OpenScene(V03NavigationFlowController.UpgradeScenePath, OpenSceneMode.Single);
            }

            GameObject root = scene
                .GetRootGameObjects()
                .FirstOrDefault(gameObject => gameObject.name == RootName);
            Require(root != null, "Upgrade scene root is missing.");
            Require(
                root.GetComponent<V03TalismanUpgradeSceneController>() != null,
                "Upgrade scene controller is missing.");
            Require(
                root.GetComponents<V03TalismanUpgradeSceneController>().Length == 1,
                "Upgrade scene must contain exactly one upgrade controller on the root.");
            Require(
                UnityEngine.Object.FindObjectOfType<ResourceService>(true) != null,
                "Upgrade scene must contain a scene-authored ResourceService.");
            Require(
                UnityEngine.Object.FindObjectOfType<UpgradeService>(true) != null,
                "Upgrade scene must contain a scene-authored UpgradeService.");
            Require(
                UnityEngine.Object.FindObjectOfType<MainTrialFlowService>(true) != null,
                "Upgrade scene must contain a scene-authored MainTrialFlowService.");
            Require(
                UnityEngine.Object.FindObjectOfType<EventSystem>(true) != null,
                "Upgrade scene must contain a scene-authored EventSystem.");
            VerifyUpgradeSlotContract();
            Require(!scene.isDirty, "Static verification must not leave scene dirty.");
            VerifyBuildSettings();
        }

        private static void VerifyUpgradeSlotContract()
        {
            Canvas[] canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
            Require(canvases.Length == 1, $"Upgrade scene must contain exactly one Canvas; found {canvases.Length}.");

            Transform pageRoot = RequireDeepChild(canvases[0].transform, PageRootName);
            Require(pageRoot.GetComponent<RectTransform>() != null, $"{PageRootName} must have RectTransform.");
            Require(pageRoot.gameObject.activeSelf, $"{PageRootName} must be active by default.");

            RequireUnique(pageRoot, "V03Upgrade_BackgroundImageSlot");
            RequireImage(pageRoot, "V03Upgrade_BackgroundImageSlot");
            RequireUnique(pageRoot, "V03Upgrade_ResourceStrip");
            RequireText(pageRoot, "ResourceText");
            RequireUnique(pageRoot, "V03Upgrade_DetailPanel");
            RequireText(pageRoot, "ItemName");
            RequireText(pageRoot, "ItemLevel");
            RequireTextUnder(pageRoot, "BeforeBlock", "Body");
            RequireTextUnder(pageRoot, "AfterBlock", "Body");
            RequireText(pageRoot, "CostText");
            RequireText(pageRoot, "StatusText");
            RequireButtonWithText(pageRoot, "UpgradeButton");
            RequireButtonWithText(pageRoot, "InfoButton");

            VerifyItemTrayContract(pageRoot);
            VerifyInfoPopupContract(pageRoot);
            VerifyGuideContract(pageRoot);
            VerifyBottomNavContract(pageRoot);
        }

        private static void VerifyItemTrayContract(Transform pageRoot)
        {
            Require(FindDeepChild(pageRoot, LegacyItemTrayRootName) == null,
                $"Legacy {LegacyItemTrayRootName} must not be used as the authored upgrade slot root.");

            Transform tray = RequireDeepChild(pageRoot, ItemTrayRootName);
            Require(tray.GetComponent<RectTransform>() != null, $"{ItemTrayRootName} must have RectTransform.");
            Require(tray.GetComponent<Image>() != null, $"{ItemTrayRootName} must have Image.");
            RequireText(tray, "ItemTrayTitle");
            RequireText(tray, "ItemTrayEmptyState");

            Transform tabs = RequireDeepChild(tray, "ItemTrayCategoryTabs");
            Require(tabs.GetComponent<GridLayoutGroup>() != null, "ItemTrayCategoryTabs must have GridLayoutGroup.");
            foreach (string category in RequiredItemTrayTabs)
            {
                RequireButtonWithText(tabs, $"ItemTrayTab_{category}");
            }

            Transform scroll = RequireDeepChild(tray, "ItemTrayScroll");
            ScrollRect scrollRect = RequireComponent<ScrollRect>(scroll, "ItemTrayScroll must have ScrollRect.");
            Transform viewport = RequireDeepChild(scroll, "Viewport");
            Require(viewport.GetComponent<Mask>() != null, "ItemTrayScroll/Viewport must have Mask.");
            Transform content = RequireDeepChild(viewport, "Content");
            Require(content.GetComponent<GridLayoutGroup>() != null, "ItemTrayScroll/Viewport/Content must have GridLayoutGroup.");
            Require(content.GetComponent<ContentSizeFitter>() != null, "ItemTrayScroll/Viewport/Content must have ContentSizeFitter.");
            Require(scrollRect.content == content, "ItemTrayScroll ScrollRect.content must point to Content.");
            Require(scrollRect.viewport == viewport, "ItemTrayScroll ScrollRect.viewport must point to Viewport.");
            Require(scrollRect.verticalScrollbar != null, "ItemTrayScroll must bind ItemTrayVerticalScrollbar.");
            RequireDeepChild(scroll, "ItemTrayVerticalScrollbar");

            List<Transform> slots = FindChildrenWithPrefix(content, "ItemTrayGridSlot_");
            Require(slots.Count == ExpectedItemTraySlotCount,
                $"Upgrade item tray must contain exactly {ExpectedItemTraySlotCount} ItemTrayGridSlot_* slots; found {slots.Count}.");
            for (int i = 1; i <= ExpectedItemTraySlotCount; i++)
            {
                string slotName = $"ItemTrayGridSlot_{i:00}";
                Transform slot = RequireDeepChild(content, slotName);
                Require(slot.parent == content, $"{slotName} must be a direct child of Content.");
                Require(slot.GetComponent<RectTransform>() != null, $"{slotName} must have RectTransform.");
                Require(slot.GetComponent<Image>() != null, $"{slotName} must have Image.");
                Require(slot.GetComponent<Outline>() != null, $"{slotName} must have Outline.");
            }

            for (int i = 1; i <= ExpectedPreviewCardCount; i++)
            {
                string cardName = $"TalismanCard_{i:00}";
                Transform card = RequireDeepChild(tray, cardName);
                Require(card.GetComponent<Button>() != null, $"{cardName} must have Button.");
                Require(card.GetComponent<Image>() != null, $"{cardName} must have Image.");
                RequireText(card, "Name");
                RequireText(card, "Level");
                RequireText(card, "ItemId");
                RequireText(card, "Amount");
            }

            Transform templateRoot = RequireDeepChild(pageRoot, "V03ItemTrayTemplates");
            Require(!templateRoot.gameObject.activeSelf, "V03ItemTrayTemplates must stay inactive by default.");
            RequireDeepChild(tray, "ItemTrayBattleLockedOverlay");
        }

        private static void VerifyInfoPopupContract(Transform pageRoot)
        {
            Transform popupRoot = RequireDeepChild(pageRoot, "V03Upgrade_ItemInfoPopup");
            Require(popupRoot.GetComponent<RectTransform>() != null, "V03Upgrade_ItemInfoPopup must have RectTransform.");
            Require(popupRoot.GetComponent<Image>() != null, "V03Upgrade_ItemInfoPopup must have Image.");
            Transform popupPanel = RequireDeepChild(popupRoot, "PopupPanel");
            Require(popupPanel.GetComponent<Image>() != null, "PopupPanel must have Image.");
            RequireText(popupPanel, "PopupTitle");
            RequireText(popupPanel, "PopupBody");
            RequireButtonWithText(popupPanel, "PopupCloseButton");
        }

        private static void VerifyGuideContract(Transform pageRoot)
        {
            Transform guideRoot = RequireDeepChild(pageRoot, "V03Upgrade_GuideOverlay");
            Require(guideRoot.GetComponent<CanvasGroup>() != null, "V03Upgrade_GuideOverlay must have CanvasGroup.");
            RequireImage(guideRoot, "BlackMask");
            Transform guideSlot = RequireDeepChild(guideRoot, "V03Upgrade_GuideImageSlot");
            Require(guideSlot.GetComponent<Image>() != null, "V03Upgrade_GuideImageSlot must have Image.");
            RequireText(guideSlot, "Text");
        }

        private static void VerifyBottomNavContract(Transform pageRoot)
        {
            Transform bottomBar = RequireDeepChild(pageRoot, "BottomBar_Global");
            Require(bottomBar.GetComponent<Image>() != null, "BottomBar_Global must have Image.");
            foreach (string buttonName in RequiredBottomNavButtons)
            {
                RequireButtonWithText(bottomBar, buttonName);
            }
        }

        private static void RequireUnique(Transform parent, string objectName)
        {
            List<Transform> matches = FindChildrenByName(parent, objectName);
            Require(matches.Count == 1, $"{objectName} must exist exactly once under {parent.name}; found {matches.Count}.");
        }

        private static Transform RequireDeepChild(Transform parent, string objectName)
        {
            Transform child = FindDeepChild(parent, objectName);
            Require(child != null, $"{BuildTransformPath(parent)} is missing required child {objectName}.");
            return child;
        }

        private static void RequireImage(Transform parent, string objectName)
        {
            Transform child = RequireDeepChild(parent, objectName);
            Require(child.GetComponent<Image>() != null, $"{objectName} must have Image.");
        }

        private static void RequireText(Transform parent, string objectName)
        {
            Transform child = RequireDeepChild(parent, objectName);
            Require(child.GetComponent<Text>() != null, $"{BuildTransformPath(child)} must have Text.");
        }

        private static void RequireTextUnder(Transform parent, string childName, string textName)
        {
            Transform child = RequireDeepChild(parent, childName);
            RequireText(child, textName);
        }

        private static void RequireButtonWithText(Transform parent, string objectName)
        {
            Transform child = RequireDeepChild(parent, objectName);
            Require(child.GetComponent<Button>() != null, $"{BuildTransformPath(child)} must have Button.");
            Require(
                child.GetComponentsInChildren<Text>(true).Length > 0,
                $"{BuildTransformPath(child)} must contain a Text label.");
        }

        private static T RequireComponent<T>(Transform target, string message) where T : Component
        {
            T component = target != null ? target.GetComponent<T>() : null;
            Require(component != null, message);
            return component;
        }

        private static List<Transform> FindChildrenByName(Transform root, string objectName)
        {
            List<Transform> matches = new();
            if (root == null)
            {
                return matches;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == objectName)
                {
                    matches.Add(child);
                }
            }

            return matches;
        }

        private static List<Transform> FindChildrenWithPrefix(Transform root, string prefix)
        {
            List<Transform> matches = new();
            if (root == null)
            {
                return matches;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    matches.Add(child);
                }
            }

            return matches;
        }

        private static string BuildTransformPath(Transform target)
        {
            if (target == null)
            {
                return "<null>";
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private static EventSystem EnsureEventSystem(Transform root)
        {
            EventSystem existing = UnityEngine.Object.FindObjectOfType<EventSystem>(true);
            if (existing != null)
            {
                return existing;
            }

            Transform existingObject = FindDeepChild(root, "EventSystem");
            GameObject eventSystemObject = existingObject != null ? existingObject.gameObject : new GameObject("EventSystem");
            if (eventSystemObject.transform.parent == null)
            {
                eventSystemObject.transform.SetParent(root, false);
            }

            EventSystem eventSystem = eventSystemObject.GetComponent<EventSystem>();
            eventSystem ??= eventSystemObject.AddComponent<EventSystem>();
            if (eventSystemObject.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            return eventSystem;
        }

        private static T EnsureSceneService<T>(Transform root, string objectName) where T : Component
        {
            T existing = UnityEngine.Object.FindObjectOfType<T>(true);
            if (existing != null)
            {
                return existing;
            }

            Transform existingObject = FindDeepChild(root, objectName);
            GameObject serviceObject = existingObject != null ? existingObject.gameObject : new GameObject(objectName);
            if (serviceObject.transform.parent == null)
            {
                serviceObject.transform.SetParent(root, false);
            }

            T service = serviceObject.GetComponent<T>();
            return service != null ? service : serviceObject.AddComponent<T>();
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

        private static void AppendBuildSettingsScenes()
        {
            EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
            string[] paths = current.Select(scene => scene.path).ToArray();
            string[] expected =
            {
                V03NavigationFlow01SceneBuilder.BootEntryScenePath,
                V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            if (paths.SequenceEqual(expected))
            {
                return;
            }

            string[] missingUpgradeOnly =
            {
                V03NavigationFlow01SceneBuilder.BootEntryScenePath,
                V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            Require(
                paths.SequenceEqual(missingUpgradeOnly),
                "Build Settings contains an unexpected scene list; refusing to reorder or overwrite it.");

            EditorBuildSettings.scenes = new[]
            {
                current[0],
                current[1],
                new EditorBuildSettingsScene(V03NavigationFlowController.UpgradeScenePath, true),
                current[2]
            };
        }

        private static void VerifyBuildSettings()
        {
            string[] expected =
            {
                V03NavigationFlow01SceneBuilder.BootEntryScenePath,
                V03NavigationFlow01SceneBuilder.MainHomeScenePath,
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.TrialScenePath
            };
            Require(
                EditorBuildSettings.scenes.Select(scene => scene.path).SequenceEqual(expected),
                "Build Settings order does not match the ForgeFirstUpgradeGuide01 contract.");
            Require(
                EditorBuildSettings.scenes.All(scene => scene.enabled),
                "All ForgeFirstUpgradeGuide01 Build Settings scenes must remain enabled.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static bool ConfirmSceneWrite(string title, string message)
        {
            return EditorUtility.DisplayDialog(
                "[Writes Scene][Guard Only] " + title,
                message,
                "Proceed",
                "Cancel");
        }
    }
}
#endif
