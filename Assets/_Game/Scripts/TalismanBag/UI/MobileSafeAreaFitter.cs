using UnityEngine;

namespace TalismanBag.UI
{
    public sealed class MobileSafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform target;

        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            Apply();
        }

        private void OnEnable()
        {
            Apply();
        }

        private void LateUpdate()
        {
            Apply();
        }

        private void Apply()
        {
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new(Screen.width, Screen.height);
            if (safeArea == lastSafeArea && screenSize == lastScreenSize)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastScreenSize = screenSize;

            RectTransform rect = target != null ? target : transform as RectTransform;
            if (rect == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            if (safeArea.width <= 0f || safeArea.height <= 0f)
            {
                ApplyFullRect(rect);
                return;
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
                ApplyFullRect(rect);
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ApplyFullRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
