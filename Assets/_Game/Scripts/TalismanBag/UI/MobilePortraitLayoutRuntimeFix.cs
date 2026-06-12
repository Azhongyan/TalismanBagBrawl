using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    [DefaultExecutionOrder(-10000)]
    public sealed class MobilePortraitLayoutRuntimeFix : MonoBehaviour
    {
        private const string SafeAreaRootName = "MobileSafeAreaRoot";
        private const string BackgroundName = "Background";

        private static bool isApplying;

        private Rect lastSafeArea;
        private Rect lastRootRect;
        private Vector2Int lastScreenSize;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ApplyInEditorAfterScriptsReload()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying)
                {
                    return;
                }

                Apply(false);
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            };
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyAfterSceneLoad()
        {
            Apply(true);
        }

        private void Awake()
        {
            Apply(Application.isPlaying);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RectTransform rect = transform as RectTransform;
            Rect rootRect = rect != null ? rect.rect : Rect.zero;
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new(Screen.width, Screen.height);
            if (safeArea == lastSafeArea && rootRect == lastRootRect && screenSize == lastScreenSize)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastRootRect = rootRect;
            lastScreenSize = screenSize;
            Apply(true);
        }

        public static void Apply()
        {
            Apply(Application.isPlaying);
        }

        private static void Apply(bool useSafeArea)
        {
            if (isApplying)
            {
                return;
            }

            isApplying = true;
            try
            {
                Canvas canvas = Object.FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    return;
                }

                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.5f;
                }

                RectTransform canvasRect = canvas.transform as RectTransform;
                if (canvasRect == null)
                {
                    return;
                }

                RectTransform safeRoot = FindOrCreateSafeAreaRoot(canvasRect);
                MoveMainUiUnderSafeArea(canvasRect, safeRoot);
                if (useSafeArea)
                {
                    ApplySafeArea(safeRoot);
                }
                else
                {
                    ApplyFullRect(safeRoot);
                }

                Canvas.ForceUpdateCanvases();
                ApplyPortraitLayout(safeRoot);
            }
            finally
            {
                isApplying = false;
            }
        }

        private static RectTransform FindOrCreateSafeAreaRoot(RectTransform canvasRect)
        {
            Transform existing = canvasRect.Find(SafeAreaRootName);
            if (existing != null && existing is RectTransform existingRect)
            {
                if (existing.GetComponent<MobileSafeAreaFitter>() == null)
                {
                    existing.gameObject.AddComponent<MobileSafeAreaFitter>();
                }

                if (Application.isPlaying && existing.GetComponent<MobilePortraitLayoutRuntimeFix>() == null)
                {
                    existing.gameObject.AddComponent<MobilePortraitLayoutRuntimeFix>();
                }

                return existingRect;
            }

            GameObject root = new(SafeAreaRootName, typeof(RectTransform), typeof(MobileSafeAreaFitter));
            root.transform.SetParent(canvasRect, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            if (Application.isPlaying)
            {
                root.AddComponent<MobilePortraitLayoutRuntimeFix>();
            }

            return rect;
        }

        private static void MoveMainUiUnderSafeArea(RectTransform canvasRect, RectTransform safeRoot)
        {
            List<Transform> children = new();
            for (int i = 0; i < canvasRect.childCount; i++)
            {
                Transform child = canvasRect.GetChild(i);
                if (child == safeRoot || child.name == BackgroundName)
                {
                    continue;
                }

                children.Add(child);
            }

            foreach (Transform child in children)
            {
                child.SetParent(safeRoot, false);
            }
        }

        private static void ApplySafeArea(RectTransform safeRoot)
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                ApplyFullRect(safeRoot);
                return;
            }

            Rect safeArea = Screen.safeArea;
            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            }

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            anchorMin.x = Mathf.Clamp01(anchorMin.x);
            anchorMin.y = Mathf.Clamp01(anchorMin.y);
            anchorMax.x = Mathf.Clamp01(anchorMax.x);
            anchorMax.y = Mathf.Clamp01(anchorMax.y);

            if (anchorMax.x <= anchorMin.x || anchorMax.y <= anchorMin.y)
            {
                ApplyFullRect(safeRoot);
                return;
            }

            safeRoot.anchorMin = anchorMin;
            safeRoot.anchorMax = anchorMax;
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
        }

        private static void ApplyFullRect(RectTransform safeRoot)
        {
            safeRoot.anchorMin = Vector2.zero;
            safeRoot.anchorMax = Vector2.one;
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
        }

        private static void ApplyPortraitLayout(RectTransform safeRoot)
        {
            float rootWidth = safeRoot.rect.width > 100f ? safeRoot.rect.width : 1080f;
            float rootHeight = safeRoot.rect.height > 100f ? safeRoot.rect.height : 1920f;
            float panelWidth = Mathf.Min(1040f, Mathf.Max(320f, rootWidth - 32f));

            SetTop(safeRoot, "TopStatusBar", 12f, 140f, Mathf.Min(1020f, panelWidth));
            SetTop(safeRoot, "EnemyArea", 166f, 250f, Mathf.Min(1020f, panelWidth));
            SetTop(safeRoot, "AutoCombatStage", 434f, 160f, Mathf.Min(1020f, panelWidth));

            float bottomHeight = 390f;
            float bottomInset = 20f;
            float infoHeight = 224f;
            float gridHeight = 660f;
            float gap = 18f;
            float bottomTop = rootHeight - bottomInset - bottomHeight;
            float gridTop = Mathf.Max(608f, bottomTop - infoHeight - gridHeight - gap * 2f);
            float infoTop = gridTop + gridHeight + gap;

            SetTop(safeRoot, "TalismanBagFrame", gridTop, gridHeight, 660f);
            SetTop(safeRoot, "InfoLogArea", infoTop, infoHeight, Mathf.Min(1020f, panelWidth));
            SetBottom(safeRoot, "BottomOperationArea", bottomInset, bottomHeight, panelWidth);
            SetBottom(safeRoot, "ThumbButtons", 72f, 118f, Mathf.Min(1000f, panelWidth - 40f));

            float shopHeight = Mathf.Min(1040f, Mathf.Max(900f, rootHeight - 720f));
            float shopBottom = Mathf.Max(bottomHeight + bottomInset + 20f, 420f);
            SetBottom(safeRoot, "ShopPanel", shopBottom, shopHeight, Mathf.Min(1000f, panelWidth - 20f));

            float resultHeight = Mathf.Min(1180f, rootHeight - 160f);
            SetMiddle(safeRoot, "BattleResultPanel", resultHeight, Mathf.Min(920f, panelWidth - 80f));
        }

        private static void SetTop(RectTransform parent, string childName, float topInset, float height, float width)
        {
            RectTransform rect = FindChildRect(parent, childName);
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -topInset);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetBottom(RectTransform parent, string childName, float bottomInset, float height, float width)
        {
            RectTransform rect = FindChildRect(parent, childName);
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottomInset);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetMiddle(RectTransform parent, string childName, float height, float width)
        {
            RectTransform rect = FindChildRect(parent, childName);
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);
        }

        private static RectTransform FindChildRect(RectTransform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child as RectTransform : null;
        }
    }
}
