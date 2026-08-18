using System;
using UnityEngine;

namespace TalismanBag.UnifiedBattle.Presentation.ExactNavigation
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxNavigationPresenter : MonoBehaviour
    {
        [SerializeField] private C1ExactBattleSandboxNavigationBarView view;

        private Action backAction;
        private Action primaryAction;
        private Func<bool> arrangementToggle;
        private Func<int> speedToggle;
        private Func<bool> battleLogToggle;
        private bool listenersBound;

        public C1ExactBattleSandboxNavigationBarView View => view;

        public bool ValidateAuthoredReferences()
        {
            return view != null
                   && view.transform == transform
                   && view.ValidateAuthoredReferences();
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            C1ExactBattleSandboxNavigationBarView configuredView)
        {
            view = configuredView;
        }
#endif

        public bool Bind(
            Action configuredBackAction,
            Action configuredPrimaryAction,
            Func<bool> configuredArrangementToggle,
            Func<int> configuredSpeedToggle,
            Func<bool> configuredBattleLogToggle,
            out string diagnostic)
        {
            if (!ValidateAuthoredReferences()
                || configuredBackAction == null
                || configuredPrimaryAction == null
                || configuredArrangementToggle == null
                || configuredSpeedToggle == null
                || configuredBattleLogToggle == null)
            {
                diagnostic = "UNIFIED_EXACT_NAVIGATION_BINDING_MISSING";
                return false;
            }

            Unbind();
            backAction = configuredBackAction;
            primaryAction = configuredPrimaryAction;
            arrangementToggle = configuredArrangementToggle;
            speedToggle = configuredSpeedToggle;
            battleLogToggle = configuredBattleLogToggle;
            view.BackButton.onClick.AddListener(HandleBack);
            view.PrimaryButton.onClick.AddListener(HandlePrimary);
            view.ArrangementButton.onClick.AddListener(HandleArrangement);
            view.SpeedButton.onClick.AddListener(HandleSpeed);
            view.BattleLogButton.onClick.AddListener(HandleBattleLog);
            listenersBound = true;
            diagnostic = "UNIFIED_EXACT_NAVIGATION_BOUND";
            return true;
        }

        public void Unbind()
        {
            if (listenersBound && view != null)
            {
                view.BackButton?.onClick.RemoveListener(HandleBack);
                view.PrimaryButton?.onClick.RemoveListener(HandlePrimary);
                view.ArrangementButton?.onClick.RemoveListener(
                    HandleArrangement);
                view.SpeedButton?.onClick.RemoveListener(HandleSpeed);
                view.BattleLogButton?.onClick.RemoveListener(HandleBattleLog);
            }

            listenersBound = false;
            backAction = null;
            primaryAction = null;
            arrangementToggle = null;
            speedToggle = null;
            battleLogToggle = null;
        }

        public void RenderPrimary(string label, bool interactable)
        {
            view?.RenderPrimary(label, interactable);
        }

        public void RenderAuxiliary(
            int playbackRate,
            bool arrangementInteractable,
            bool speedInteractable,
            bool battleLogInteractable)
        {
            view?.RenderAuxiliary(
                playbackRate,
                arrangementInteractable,
                speedInteractable,
                battleLogInteractable);
        }

        public void ResetPresentation()
        {
            view?.ResetRuntimeState();
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void HandleBack()
        {
            backAction?.Invoke();
        }

        private void HandlePrimary()
        {
            primaryAction?.Invoke();
        }

        private void HandleArrangement()
        {
            arrangementToggle?.Invoke();
        }

        private void HandleSpeed()
        {
            speedToggle?.Invoke();
        }

        private void HandleBattleLog()
        {
            battleLogToggle?.Invoke();
        }
    }
}
