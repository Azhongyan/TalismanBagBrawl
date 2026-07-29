using System;
using System.Globalization;
using System.Linq;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BattleBridge.NianResource
{
    [DisallowMultipleComponent]
    public sealed class BattleSandboxNianResourcePresenter : MonoBehaviour
    {
        private const string ManaTextName = "ManaText";
        private const string ManaBarName = "PlayerManaBar";
        private const string FillName = "Fill";
        private const string FloatingRootName =
            "EnemyCombatFeedbackFloatingRoot";
        private const string GeneratedAnchorName = "ManaGeneratedAnchor";
        private const string SpentAnchorName = "ManaSpentAnchor";

        private Text manaText;
        private Image manaFill;
        private RectTransform floatingRoot;
        private RectTransform generatedAnchor;
        private RectTransform spentAnchor;
        private BattleSandboxAuthoredTmpPresentation authoredTmpPresentation;
        private int currentGeneration;

        private bool authoredStateCaptured;
        private string authoredText = string.Empty;
        private Color authoredTextColor;
        private bool authoredTextActive;
        private float authoredFillAmount;
        private Color authoredFillColor;
        private bool authoredFillActive;

        public bool DevOnly => true;
        public bool OwnsResourceTruth => false;
        public bool WritesLayout => false;
        public Text ManaText => manaText;
        public Image ManaFill => manaFill;
        public RectTransform GeneratedAnchor => generatedAnchor;
        public RectTransform SpentAnchor => spentAnchor;
        public BattleSandboxAuthoredTmpPresentation AuthoredTmpPresentation =>
            authoredTmpPresentation;
        public bool HasVisibleBindings =>
            manaText != null
            && manaFill != null
            && generatedAnchor != null
            && spentAnchor != null;

        public void BeginGeneration(
            BattleSandboxNianResourceSnapshot snapshot)
        {
            ResolveBindings();
            CaptureAuthoredState();
            currentGeneration = snapshot == null
                ? 0
                : snapshot.resetGeneration;
            ApplySnapshot(snapshot, "I031 供念就绪");
        }

        public void BindPresentation(
            BattleSandboxAuthoredTmpPresentation presentation)
        {
            authoredTmpPresentation = presentation;
        }

        public void PresentPulse(
            BattleSandboxNianResourceSnapshot snapshot,
            BattleSandboxNianResourceApplication application,
            int generatedAmount)
        {
            if (snapshot == null || application == null)
            {
                return;
            }

            ResolveBindings();
            CaptureAuthoredState();
            string status = application.accepted
                ? string.Format(
                    CultureInfo.InvariantCulture,
                    "+{0} NP · {1} -{2} NP",
                    generatedAmount,
                    application.itemId,
                    application.nianCost)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} 需 {1} NP · 当前 {2} NP",
                    application.itemId,
                    application.nianCost,
                    snapshot.currentNian);
            if (application.reasonCode != "SPEND_ACCEPTED"
                && application.reasonCode != "INSUFFICIENT_NIAN")
            {
                status = "请求拒绝 · " + application.reasonCode;
            }
            ApplySnapshot(snapshot, status);
        }

        public void RestoreAuthoredState()
        {
            if (!authoredStateCaptured)
            {
                return;
            }

            if (manaText != null)
            {
                manaText.text = authoredText;
                manaText.color = authoredTextColor;
                manaText.gameObject.SetActive(authoredTextActive);
            }

            if (manaFill != null)
            {
                manaFill.fillAmount = authoredFillAmount;
                manaFill.color = authoredFillColor;
                manaFill.gameObject.SetActive(authoredFillActive);
            }

            authoredStateCaptured = false;
        }

        private void ApplySnapshot(
            BattleSandboxNianResourceSnapshot snapshot,
            string status)
        {
            if (snapshot == null)
            {
                return;
            }

            if (manaText != null)
            {
                manaText.gameObject.SetActive(true);
                manaText.text = string.Format(
                    CultureInfo.InvariantCulture,
                    "NP {0}/{1} · {2}",
                    snapshot.currentNian,
                    snapshot.maxNian,
                    status ?? string.Empty);
            }

            if (manaFill != null)
            {
                manaFill.gameObject.SetActive(true);
                manaFill.fillAmount = snapshot.maxNian <= 0
                    ? 0f
                    : Mathf.Clamp01(
                        (float)snapshot.currentNian / snapshot.maxNian);
            }
        }

        private void ResolveBindings()
        {
            RectTransform[] sceneRects =
                Resources.FindObjectsOfTypeAll<RectTransform>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (manaText == null)
            {
                RectTransform textRect = sceneRects.FirstOrDefault(value =>
                    string.Equals(
                        value.name, ManaTextName, StringComparison.Ordinal));
                manaText = textRect == null
                    ? null
                    : textRect.GetComponent<Text>();
            }

            if (manaFill == null)
            {
                RectTransform bar = sceneRects.FirstOrDefault(value =>
                    string.Equals(
                        value.name, ManaBarName, StringComparison.Ordinal));
                if (bar != null)
                {
                    manaFill = bar.GetComponentsInChildren<Image>(true)
                        .FirstOrDefault(value => value != null
                            && string.Equals(
                                value.name, FillName,
                                StringComparison.Ordinal));
                }
            }

            if (floatingRoot == null)
            {
                floatingRoot = sceneRects.FirstOrDefault(value =>
                    string.Equals(
                        value.name,
                        FloatingRootName,
                        StringComparison.Ordinal));
            }

            if (generatedAnchor == null)
            {
                generatedAnchor = sceneRects.FirstOrDefault(value =>
                    string.Equals(
                        value.name,
                        GeneratedAnchorName,
                        StringComparison.Ordinal));
            }

            if (spentAnchor == null)
            {
                spentAnchor = sceneRects.FirstOrDefault(value =>
                    string.Equals(
                        value.name,
                        SpentAnchorName,
                        StringComparison.Ordinal));
            }
        }

        private void CaptureAuthoredState()
        {
            if (authoredStateCaptured)
            {
                return;
            }

            if (manaText != null)
            {
                authoredText = manaText.text;
                authoredTextColor = manaText.color;
                authoredTextActive = manaText.gameObject.activeSelf;
            }

            if (manaFill != null)
            {
                authoredFillAmount = manaFill.fillAmount;
                authoredFillColor = manaFill.color;
                authoredFillActive = manaFill.gameObject.activeSelf;
            }

            authoredStateCaptured = true;
        }

        private void SpawnFloatingText(
            RectTransform anchor,
            string text,
            BattleSandboxAuthoredTmpStyle style,
            string eventId,
            string role,
            float delay)
        {
            if (floatingRoot == null
                || anchor == null
                || authoredTmpPresentation == null)
            {
                return;
            }

            authoredTmpPresentation.TrySpawn(
                currentGeneration,
                eventId,
                role,
                anchor,
                text,
                style,
                0.72f,
                delay,
                BattleSandboxAuthoredTmpPresentation
                    .DefaultReadableLifetime);
        }

        private void OnDisable()
        {
            RestoreAuthoredState();
        }
    }
}
