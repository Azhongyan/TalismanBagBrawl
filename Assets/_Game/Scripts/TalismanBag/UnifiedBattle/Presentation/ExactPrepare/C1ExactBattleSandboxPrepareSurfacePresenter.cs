using System;
using UnityEngine;

namespace TalismanBag.UnifiedBattle.Presentation.ExactPrepare
{
    public enum C1ExactBattleSandboxPrepareSurfaceState
    {
        Closed = 0,
        Opening = 1,
        Open = 2,
        Closing = 3
    }

    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxPrepareSurfacePresenter :
        MonoBehaviour
    {
        public const float SourceMotionFactor = 9f;

        [SerializeField] private C1ExactBattleSandboxPrepareSurfaceView view;

        private readonly C1ExactBattleSandboxPrepareMotion motion =
            new C1ExactBattleSandboxPrepareMotion(SourceMotionFactor);
        private int lifecycleGeneration;

        public C1ExactBattleSandboxPrepareSurfaceView View => view;
        public C1ExactBattleSandboxPrepareSurfaceState State => motion.State;
        public int LifecycleGeneration => lifecycleGeneration;
        public bool IsTransitioning => State ==
                                       C1ExactBattleSandboxPrepareSurfaceState
                                           .Opening
                                       || State ==
                                       C1ExactBattleSandboxPrepareSurfaceState
                                           .Closing;

        public bool ValidateAuthoredReferences()
        {
            return view != null
                   && view.transform == transform
                   && view.ValidateAuthoredReferences();
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            C1ExactBattleSandboxPrepareSurfaceView configuredView)
        {
            view = configuredView;
        }
#endif

        public void ResetForLifecycle(int generation)
        {
            lifecycleGeneration = generation;
            motion.SnapClosed();
            view?.ApplyNormalizedProgress(motion.Progress);
            view?.SetInputEnabled(false);
        }

        public bool TryBeginOpen(int generation)
        {
            if (generation != lifecycleGeneration || !motion.TryBeginOpen())
            {
                return false;
            }

            view?.SetInputEnabled(false);
            return true;
        }

        public bool TryBeginClose(int generation)
        {
            if (generation != lifecycleGeneration || !motion.TryBeginClose())
            {
                return false;
            }

            view?.SetInputEnabled(false);
            return true;
        }

        public bool Tick(
            float unscaledDeltaTime,
            out C1ExactBattleSandboxPrepareSurfaceState completedState,
            out int completedGeneration)
        {
            bool completed = motion.Advance(unscaledDeltaTime);
            view?.ApplyNormalizedProgress(motion.Progress);
            completedState = motion.State;
            completedGeneration = lifecycleGeneration;
            return completed;
        }

        public bool SetOpenInputEnabled(bool enabled, int generation)
        {
            bool accepted = generation == lifecycleGeneration
                            && State ==
                            C1ExactBattleSandboxPrepareSurfaceState.Open
                            && enabled;
            view?.SetInputEnabled(accepted);
            return accepted;
        }

        private void OnDisable()
        {
            view?.SetInputEnabled(false);
        }
    }

    internal sealed class C1ExactBattleSandboxPrepareMotion
    {
        private const float CompletionEpsilon = 0.0005f;
        private readonly float motionFactor;

        internal C1ExactBattleSandboxPrepareMotion(float configuredMotionFactor)
        {
            motionFactor = configuredMotionFactor > 0f
                ? configuredMotionFactor
                : C1ExactBattleSandboxPrepareSurfacePresenter.SourceMotionFactor;
            SnapClosed();
        }

        internal C1ExactBattleSandboxPrepareSurfaceState State { get; private set; }
        internal float Progress { get; private set; }

        internal bool TryBeginOpen()
        {
            if (State != C1ExactBattleSandboxPrepareSurfaceState.Closed)
            {
                return false;
            }

            State = C1ExactBattleSandboxPrepareSurfaceState.Opening;
            return true;
        }

        internal bool TryBeginClose()
        {
            if (State != C1ExactBattleSandboxPrepareSurfaceState.Open
                && State != C1ExactBattleSandboxPrepareSurfaceState.Opening)
            {
                return false;
            }

            State = C1ExactBattleSandboxPrepareSurfaceState.Closing;
            return true;
        }

        internal bool Advance(float unscaledDeltaTime)
        {
            if (State != C1ExactBattleSandboxPrepareSurfaceState.Opening
                && State != C1ExactBattleSandboxPrepareSurfaceState.Closing)
            {
                return false;
            }

            float target = State ==
                           C1ExactBattleSandboxPrepareSurfaceState.Opening
                ? 1f
                : 0f;
            float safeDelta = Math.Max(0f, unscaledDeltaTime);
            float blend = 1f - (float)Math.Exp(-motionFactor * safeDelta);
            Progress += (target - Progress) * blend;
            if (Math.Abs(target - Progress) > CompletionEpsilon)
            {
                return false;
            }

            Progress = target;
            State = target > 0.5f
                ? C1ExactBattleSandboxPrepareSurfaceState.Open
                : C1ExactBattleSandboxPrepareSurfaceState.Closed;
            return true;
        }

        internal void SnapClosed()
        {
            Progress = 0f;
            State = C1ExactBattleSandboxPrepareSurfaceState.Closed;
        }
    }
}
