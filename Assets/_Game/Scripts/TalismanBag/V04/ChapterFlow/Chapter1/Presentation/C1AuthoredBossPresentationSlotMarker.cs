using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class C1AuthoredBossPresentationSlotMarker :
        MonoBehaviour
    {
        [SerializeField] private string stableBossSlotId = "BossSlot";
        [SerializeField] private Image visualRenderer;

        public string StableBossSlotId =>
            stableBossSlotId ?? string.Empty;
        public Image VisualRenderer => visualRenderer;
    }
}
