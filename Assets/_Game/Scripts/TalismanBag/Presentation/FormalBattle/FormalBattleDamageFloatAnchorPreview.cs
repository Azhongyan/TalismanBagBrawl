using TMPro;
using UnityEngine;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleDamageFloatAnchorPreview : MonoBehaviour
    {
        [SerializeField] private FormalBattlePresentationProfile profile;
        [SerializeField] private TMP_Text previewLabel;
        [SerializeField] private string previewPayload = "-82";
        [SerializeField] private string previewStyleKey =
            FormalBattleDamageFloatStyleKeys.Damage;

        private void Awake()
        {
            if (previewLabel != null)
            {
                previewLabel.gameObject.SetActive(false);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyAuthoringPreview();
        }

        public void AssignForEditor(
            FormalBattlePresentationProfile configuredProfile,
            TMP_Text configuredPreviewLabel,
            string configuredPreviewPayload,
            string configuredPreviewStyleKey)
        {
            profile = configuredProfile;
            previewLabel = configuredPreviewLabel;
            previewPayload = configuredPreviewPayload ?? string.Empty;
            previewStyleKey = configuredPreviewStyleKey ?? string.Empty;
            ApplyAuthoringPreview();
        }

        private void ApplyAuthoringPreview()
        {
            if (Application.isPlaying || previewLabel == null)
            {
                return;
            }

            previewLabel.gameObject.SetActive(true);
            previewLabel.text = previewPayload ?? string.Empty;
            previewLabel.raycastTarget = false;
            if (profile == null)
            {
                return;
            }

            previewLabel.font = profile.DamageFont;
            if (profile.TryGetDamageFloatStyle(
                    previewStyleKey,
                    out FormalBattleDamageFloatVisualStyle style))
            {
                previewLabel.enableVertexGradient = true;
                previewLabel.colorGradient = style.Gradient;
            }
        }
#endif
    }
}
