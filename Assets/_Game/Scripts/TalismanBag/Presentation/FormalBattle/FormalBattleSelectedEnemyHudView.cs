using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleSelectedEnemyHudView : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text identityLabel;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private TMP_Text hpLabel;
        [SerializeField] private GameObject shellRoot;
        [SerializeField] private Image shellIconImage;
        [SerializeField] private TMP_Text shellLabel;
        [SerializeField] private FormalBattleStatusStripView statusStrip;

        private readonly Dictionary<Texture2D, Sprite> runtimeSprites =
            new Dictionary<Texture2D, Sprite>();

        private string actorBalanceId = string.Empty;
        private int stableActorOrder = -1;

        public string ActorBalanceId => actorBalanceId;
        public int StableActorOrder => stableActorOrder;
        public bool Bound => !string.IsNullOrEmpty(actorBalanceId);

        public bool Bind(
            C1FormalRealtimeBattleActorSnapshot snapshot,
            IReadOnlyList<C1FormalRealtimeBattleStatusSnapshot> statuses,
            FormalBattlePresentationProfile profile,
            long battleTimeMs,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (snapshot == null
                || profile == null
                || !profile.TryGetEnemyProfile(
                    snapshot.contentId,
                    snapshot.runtimeProfileId,
                    out FormalBattleEnemyVisualProfile visualProfile)
                || visualProfile.Idle == null
                || visualProfile.Idle.Frames.Length == 0
                || visualProfile.Idle.Frames[0] == null)
            {
                diagnostic = "FORMAL_SELECTED_ENEMY_HUD_IDENTITY_REJECTED";
                Clear();
                return false;
            }

            actorBalanceId = snapshot.actorBalanceId;
            stableActorOrder = snapshot.stableActorOrder;
            identityLabel.text = visualProfile.DisplayName;
            ApplyPortrait(visualProfile.Idle.Frames[0]);
            ApplyHp(snapshot.currentHp, snapshot.maxHp);
            bool showShell = HasVisibleShell(snapshot);
            if (!statusStrip.ArrangeWithLeadingPinned(
                    shellRoot.transform as RectTransform,
                    showShell))
            {
                diagnostic = "FORMAL_SELECTED_ENEMY_SHELL_STATUS_FLOW_INVALID";
                Clear();
                return false;
            }
            ApplyShell(snapshot);
            if (!statusStrip.Bind(
                    statuses,
                    profile,
                    battleTimeMs,
                    out diagnostic))
            {
                Clear();
                return false;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            diagnostic = "FORMAL_SELECTED_ENEMY_HUD_BOUND_" + actorBalanceId;
            return true;
        }

        public void ApplyHp(int currentHp, int maxHp)
        {
            int safeMax = Mathf.Max(1, maxHp);
            int safeCurrent = Mathf.Clamp(currentHp, 0, safeMax);
            hpFillImage.fillAmount = safeCurrent / (float)safeMax;
            hpLabel.text = safeCurrent.ToString(CultureInfo.InvariantCulture)
                + " / " + safeMax.ToString(CultureInfo.InvariantCulture);
        }

        public void Clear()
        {
            actorBalanceId = string.Empty;
            stableActorOrder = -1;
            if (portraitImage != null)
            {
                portraitImage.sprite = null;
                portraitImage.enabled = false;
            }
            if (identityLabel != null) identityLabel.text = string.Empty;
            if (hpFillImage != null) hpFillImage.fillAmount = 1f;
            if (hpLabel != null) hpLabel.text = string.Empty;
            if (shellRoot != null) shellRoot.SetActive(false);
            if (shellIconImage != null) shellIconImage.enabled = true;
            if (shellLabel != null) shellLabel.text = string.Empty;
            statusStrip?.Clear();
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        public bool ValidateAuthoredReferences()
        {
            return portraitImage != null
                   && identityLabel != null
                   && hpFillImage != null
                   && hpLabel != null
                   && shellRoot != null
                   && shellIconImage != null
                   && shellLabel != null
                   && statusStrip != null
                   && statusStrip.ValidateAuthoredReferences()
                   && !portraitImage.raycastTarget
                   && !hpFillImage.raycastTarget
                   && !shellIconImage.raycastTarget;
        }

        private void ApplyShell(C1FormalRealtimeBattleActorSnapshot snapshot)
        {
            int safeMax = Mathf.Max(1, snapshot.maxShell);
            int safeCurrent = Mathf.Clamp(snapshot.currentShell, 0, safeMax);
            bool showShell = HasVisibleShell(snapshot);
            shellRoot.SetActive(showShell);
            if (!showShell)
            {
                shellLabel.text = string.Empty;
                return;
            }

            shellLabel.text = safeCurrent.ToString(CultureInfo.InvariantCulture);
        }

        private static bool HasVisibleShell(
            C1FormalRealtimeBattleActorSnapshot snapshot)
        {
            return snapshot != null
                   && snapshot.hasShell
                   && snapshot.currentShell > 0;
        }

        private void ApplyPortrait(Texture2D texture)
        {
            if (!runtimeSprites.TryGetValue(texture, out Sprite sprite)
                || sprite == null)
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect);
                sprite.name = "FormalBattleSelectedHud_" + texture.name;
                sprite.hideFlags = HideFlags.DontSave;
                runtimeSprites[texture] = sprite;
            }
            portraitImage.sprite = sprite;
            portraitImage.enabled = true;
        }

        private void OnDestroy()
        {
            foreach (Sprite sprite in runtimeSprites.Values)
            {
                if (sprite == null) continue;
                if (Application.isPlaying) Destroy(sprite);
                else DestroyImmediate(sprite);
            }
            runtimeSprites.Clear();
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            Image configuredPortraitImage,
            TMP_Text configuredIdentityLabel,
            Image configuredHpFillImage,
            TMP_Text configuredHpLabel,
            GameObject configuredShellRoot,
            Image configuredShellIconImage,
            TMP_Text configuredShellLabel,
            FormalBattleStatusStripView configuredStatusStrip)
        {
            portraitImage = configuredPortraitImage;
            identityLabel = configuredIdentityLabel;
            hpFillImage = configuredHpFillImage;
            hpLabel = configuredHpLabel;
            shellRoot = configuredShellRoot;
            shellIconImage = configuredShellIconImage;
            shellLabel = configuredShellLabel;
            statusStrip = configuredStatusStrip;
        }

        public void AssignShellIconForEditor(
            GameObject configuredShellRoot,
            Image configuredShellIconImage,
            TMP_Text configuredShellLabel)
        {
            shellRoot = configuredShellRoot;
            shellIconImage = configuredShellIconImage;
            shellLabel = configuredShellLabel;
        }
#endif
    }
}
