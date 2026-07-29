using System;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime
{
    public static class C1EnemyRuntimeReducer
    {
        public static C1EnemyRuntimeSnapshot CreatePresent(
            string runtimeProfileId,
            string enemyInstanceId,
            int resetGeneration,
            long battleTick)
        {
            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.FindProfile(runtimeProfileId);
            if (profile == null)
            {
                throw new ArgumentException("Unknown runtime profile.", "runtimeProfileId");
            }
            if (string.IsNullOrWhiteSpace(enemyInstanceId))
            {
                throw new ArgumentException("Enemy instance ID is required.", "enemyInstanceId");
            }
            if (resetGeneration < 0)
            {
                throw new ArgumentOutOfRangeException("resetGeneration");
            }

            C1EnemyActionPatternSnapshot action =
                C1EnemyRuntimeCatalog.FindActionForProfile(runtimeProfileId);
            C1EnemyRuntimeStateData data = NewData(
                profile,
                enemyInstanceId,
                resetGeneration,
                battleTick,
                action.FirstDueTick);
            data.Revision = 1L;
            data.Lifecycle = C1EnemyLifecycle.Present;
            data.Targetable = false;
            AddCue(data, C1EnemyCueKind.Presence, battleTick, "presence");
            return Finalize(data);
        }

        public static C1EnemyTransitionResult Activate(
            C1EnemyRuntimeSnapshot snapshot,
            long battleTick)
        {
            if (snapshot == null)
            {
                return new C1EnemyTransitionResult(null, false, "STATE_NULL");
            }
            if (snapshot.Lifecycle != C1EnemyLifecycle.Present)
            {
                return Reject(snapshot, "PRESENT_STATE_REQUIRED");
            }
            C1EnemyRuntimeStateData data = snapshot.ToMutable();
            data.Revision++;
            data.Lifecycle = C1EnemyLifecycle.Active;
            data.Targetable = true;
            data.LastSchedulerTick = battleTick;
            return Accept(data);
        }

        public static C1EnemyTransitionResult ApplyBattleApplication(
            C1EnemyRuntimeSnapshot snapshot,
            C1EnemyBattleApplicationResult application)
        {
            string error = C1EnemyRuntimeValidation.ValidateApplication(snapshot, application);
            if (!string.IsNullOrEmpty(error))
            {
                return Reject(snapshot, error);
            }

            C1EnemyRuntimeStateData data = snapshot.ToMutable();
            data.Revision++;
            data.LastAcceptedApplicationSequence = application.ApplicationSequence;
            data.LastAcceptedBattleTick = application.BattleTick;
            data.AcceptedApplicationCount++;
            data.AcceptedApplicationEventIds.Add(application.ApplicationEventId);

            if (application.ActualShellDeltaApplied > 0)
            {
                int previousShell = data.CurrentShell;
                data.CurrentShell -= application.ActualShellDeltaApplied;
                AddCue(
                    data,
                    C1EnemyCueKind.ShellHit,
                    application.BattleTick,
                    application.ApplicationEventId);
                if (previousShell > 0 && data.CurrentShell == 0)
                {
                    data.ShellState = C1EnemyShellState.Broken;
                    AddCue(
                        data,
                        C1EnemyCueKind.ShellBreak,
                        application.BattleTick,
                        application.ApplicationEventId);
                    if (!data.ShellBreakRequestEmitted)
                    {
                        data.ShellBreakRequestEmitted = true;
                        AddRequest(
                            data,
                            C1EnemyRuntimeContract.ShellBreakCounterWindow,
                            application.BattleTick,
                            application.ApplicationEventId,
                            string.Empty,
                            3000);
                        AddCue(
                            data,
                            C1EnemyCueKind.CounterWindowRequested,
                            application.BattleTick,
                            application.ApplicationEventId);
                    }
                }
            }

            if (application.ActualHpDeltaApplied > 0)
            {
                data.CurrentHp -= application.ActualHpDeltaApplied;
                AddCue(
                    data,
                    C1EnemyCueKind.Hit,
                    application.BattleTick,
                    application.ApplicationEventId);
                if (data.CurrentHp == 0)
                {
                    data.Lifecycle = C1EnemyLifecycle.Defeated;
                    data.Targetable = false;
                    data.ActiveAction = null;
                    AddCue(
                        data,
                        C1EnemyCueKind.Defeated,
                        application.BattleTick,
                        application.ApplicationEventId);
                    if (data.ContentId == C1EnemyRuntimeContract.ShatteredHostContentId
                        && !data.PostDefeatFieldRequestEmitted)
                    {
                        data.PostDefeatFieldRequestEmitted = true;
                        AddRequest(
                            data,
                            C1EnemyRuntimeContract.PostDefeatFieldRequest,
                            application.BattleTick,
                            application.ApplicationEventId,
                            "SHORT",
                            0);
                        AddCue(
                            data,
                            C1EnemyCueKind.PostDefeatFieldRequested,
                            application.BattleTick,
                            application.ApplicationEventId);
                    }
                }
            }

            return Accept(data);
        }

        public static C1EnemyTransitionResult Reset(
            C1EnemyRuntimeSnapshot snapshot,
            long battleTick)
        {
            if (snapshot == null)
            {
                return new C1EnemyTransitionResult(null, false, "STATE_NULL");
            }
            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.FindProfile(snapshot.RuntimeProfileId);
            C1EnemyActionPatternSnapshot action =
                C1EnemyRuntimeCatalog.FindActionForProfile(snapshot.RuntimeProfileId);
            C1EnemyRuntimeStateData data = NewData(
                profile,
                snapshot.EnemyInstanceId,
                snapshot.ResetGeneration + 1,
                battleTick,
                action.FirstDueTick);
            data.Revision = snapshot.Revision + 1L;
            data.Lifecycle = C1EnemyLifecycle.Present;
            data.Targetable = false;
            AddCue(data, C1EnemyCueKind.Reset, battleTick, "reset");
            return Accept(data);
        }

        public static C1EnemyTransitionResult AddDeveloperDiagnosticForFixture(
            C1EnemyRuntimeSnapshot snapshot,
            string diagnostic)
        {
            if (snapshot == null)
            {
                return new C1EnemyTransitionResult(null, false, "STATE_NULL");
            }
            C1EnemyRuntimeStateData data = snapshot.ToMutable();
            data.DeveloperDiagnostics.Add(diagnostic ?? string.Empty);
            return Accept(data);
        }

        internal static C1EnemyRuntimeSnapshot Finalize(C1EnemyRuntimeStateData data)
        {
            return new C1EnemyRuntimeSnapshot(data);
        }

        internal static void AddCue(
            C1EnemyRuntimeStateData data,
            C1EnemyCueKind kind,
            long battleTick,
            string sourceIdentity)
        {
            long sequence = data.NextCueSequence++;
            string cueId = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "cue:g{0}:s{1}",
                data.ResetGeneration,
                sequence);
            C1EnemyCueSnapshot cue = new C1EnemyCueSnapshot(
                cueId,
                sequence,
                kind,
                data.ResetGeneration,
                data.Revision,
                sourceIdentity,
                battleTick);
            data.Cues.Add(cue);
            data.LatestCue = cue;
        }

        internal static void AddRequest(
            C1EnemyRuntimeStateData data,
            string requestKey,
            long battleTick,
            string sourceIdentity,
            string lifetimeClass,
            int requestedDurationTicks)
        {
            long sequence = data.NextRequestSequence++;
            string requestId = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "request:g{0}:s{1}",
                data.ResetGeneration,
                sequence);
            data.PendingRequests.Add(new C1EnemyEffectRequestSnapshot(
                requestId,
                sequence,
                requestKey,
                data.ResetGeneration,
                data.Revision,
                sourceIdentity,
                battleTick,
                lifetimeClass,
                requestedDurationTicks));
        }

        private static C1EnemyRuntimeStateData NewData(
            C1EnemyRuntimeProfileSnapshot profile,
            string enemyInstanceId,
            int resetGeneration,
            long battleTick,
            long firstDueOffset)
        {
            return new C1EnemyRuntimeStateData
            {
                EnemyInstanceId = enemyInstanceId,
                RuntimeProfileId = profile.RuntimeProfileId,
                ContentId = profile.ContentId,
                PresentationKey = profile.PresentationKey,
                ResetGeneration = resetGeneration,
                MaxHp = profile.MaxHp,
                CurrentHp = profile.MaxHp,
                ShellMax = profile.ShellMax,
                CurrentShell = profile.InitialShell,
                ShellState = profile.ShellMax == 0
                    ? C1EnemyShellState.NotApplicable
                    : C1EnemyShellState.Intact,
                SelfPossessionState =
                    profile.ContentId == C1EnemyRuntimeContract.ShatteredHostContentId
                        ? C1EnemySelfPossessionState.DeclaredOwnStateOnly
                        : C1EnemySelfPossessionState.NotApplicable,
                SelfPossessionGameplayEffectAuthored = false,
                LastAcceptedBattleTick = -1L,
                LastAcceptedApplicationSequence = -1L,
                LastSchedulerTick = battleTick,
                NextActionDueTick = battleTick + firstDueOffset
            };
        }

        private static C1EnemyTransitionResult Accept(C1EnemyRuntimeStateData data)
        {
            return new C1EnemyTransitionResult(Finalize(data), true, string.Empty);
        }

        private static C1EnemyTransitionResult Reject(
            C1EnemyRuntimeSnapshot snapshot,
            string error)
        {
            return new C1EnemyTransitionResult(snapshot, false, error);
        }
    }
}
