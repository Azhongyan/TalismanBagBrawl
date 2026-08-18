using System;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.Items.Combat;
using TalismanBag.V04.ChapterFlow.Chapter1;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1.Presentation
{
    internal sealed class C1EncounterOwnershipFreshPlayContext
    {
        public V04Chapter1BattleSandboxRuntimeController Flow;
        public ShougunuPhase1BattleSandboxVerticalSliceRuntime P3;
        public ShougunuPhase1BattleSandboxSceneBinder Binder;
        public C1JourneyPresentationController Journey;
        public ItemSystemBattleSandboxBoardAdapter ItemAdapter;
        public LiHuoCombatFeedbackOrchestrationController LiHuo;
        public ItemLivingGradientOutlineVfxPrototypeController Outline;
        public Image AuthoredSlot;
        public ShougunuPhase1VisualPrototypeController BossVisual;

        public bool Ready =>
            Flow != null
            && P3 != null
            && Binder?.IsBindingComplete == true
            && Journey != null
            && ItemAdapter?.Authority != null
            && LiHuo != null
            && Outline != null
            && AuthoredSlot != null
            && BossVisual != null
            && Flow.AuthoredStartSurfaceBound;
    }

    internal static class C1EncounterOwnershipFreshPlayProbe
    {
        private static readonly string[] StageIds =
        {
            "1-1", "1-2", "1-3", "1-4", "1-5",
            "1-6", "1-7", "1-8", "1-9"
        };

        public static C1EncounterOwnershipFreshPlayContext Resolve(
            Scene scene)
        {
            Image slot = Single<Image>(
                scene,
                value => string.Equals(
                        value.gameObject.name,
                        "Shougunu_1",
                        StringComparison.Ordinal)
                    && value.transform.parent != null
                    && string.Equals(
                        value.transform.parent.name,
                        "V02EnemyArea",
                        StringComparison.Ordinal));
            return new C1EncounterOwnershipFreshPlayContext
            {
                Flow = Single<
                    V04Chapter1BattleSandboxRuntimeController>(scene),
                P3 = Single<
                    ShougunuPhase1BattleSandboxVerticalSliceRuntime>(scene),
                Binder = Single<
                    ShougunuPhase1BattleSandboxSceneBinder>(scene),
                Journey =
                    Single<C1JourneyPresentationController>(scene),
                ItemAdapter =
                    Single<ItemSystemBattleSandboxBoardAdapter>(scene),
                LiHuo =
                    Single<
                        LiHuoCombatFeedbackOrchestrationController>(scene),
                Outline =
                    Single<
                        ItemLivingGradientOutlineVfxPrototypeController>(
                            scene),
                AuthoredSlot = slot,
                BossVisual = slot?.GetComponent<
                    ShougunuPhase1VisualPrototypeController>()
            };
        }

        public static bool CommitOfficialFixture(
            IItemSystemBattleSandboxBoardAuthority authority,
            out string failure)
        {
            failure = string.Empty;
            if (authority?.CurrentSnapshot?.placements?.Count != 0)
            {
                failure = "FRESH_FIXTURE_BOARD_NOT_EMPTY";
                return false;
            }
            (string id, int x, int y, ItemShapeRotation rotation)[] rows =
            {
                ("I031", 0, 0, ItemShapeRotation.Rotation0),
                ("I009", 1, 0, ItemShapeRotation.Rotation0),
                ("I012", 2, 0, ItemShapeRotation.Rotation0),
                ("I010", 1, 1, ItemShapeRotation.Rotation90),
                ("I008", 0, 2, ItemShapeRotation.Rotation0),
                ("I011", 1, 3, ItemShapeRotation.Rotation0),
                ("I007", 2, 4, ItemShapeRotation.Rotation0)
            };
            foreach ((string id, int x, int y, ItemShapeRotation rotation)
                in rows)
            {
                ItemSystemBattleSandboxBoardOperationResult result =
                    authority.CommitFromTray(
                        id,
                        new ItemShapeCell(x, y),
                        rotation);
                if (!result.Accepted)
                {
                    failure = "OFFICIAL_FIXTURE_REJECTED:"
                        + id + ":" + result.DiagnosticCode;
                    return false;
                }
            }
            if (!V04Chapter1BossPhase1CompletionAdapter
                .TryValidateBossReadiness(
                    authority,
                    out string readiness))
            {
                failure = "OFFICIAL_FIXTURE_NOT_READY:" + readiness;
                return false;
            }
            return true;
        }

        public static bool ValidateAuthoritativeFirstEncounterTuning(
            IItemSystemBattleSandboxBoardAuthority authority,
            out string failure)
        {
            failure = string.Empty;
            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.FindProfile(
                    C1EnemyRuntimeContract.ShatteredHostProfileId);
            if (!V04Chapter1NormalEnemyBattleAdapter.TryPrepare(
                    authority,
                    profile,
                    resetGeneration: 1,
                    out V04Chapter1NormalEnemyBattleAdapter preview,
                    out string diagnostic))
            {
                failure =
                    "AUTHORITATIVE_FIRST_ENCOUNTER_PREFLIGHT_REJECTED:"
                    + diagnostic;
                return false;
            }

            ItemCombatEffectRequestRow[] damageRows =
                preview.ItemRequestSnapshot.Requests
                    .Where(value => value != null
                        && value.requestKind ==
                            ItemCombatEffectRequestKind.DirectFlatDamage
                        && value.resolvedPreMitigationDamageUnits > 0L)
                    .OrderBy(value => value.sourceItemInstanceId,
                        StringComparer.Ordinal)
                    .ThenBy(value => value.sourcePlacementId,
                        StringComparer.Ordinal)
                    .ThenBy(value => (int)value.requestKind)
                    .ToArray();
            if (damageRows.Length == 0 || profile == null)
            {
                failure =
                    "AUTHORITATIVE_FIRST_ENCOUNTER_PREFLIGHT_INCOMPLETE";
                return false;
            }

            long cadence =
                V04Chapter1ContinuousBattleIntegrationContract
                    .ItemApplicationCadenceTicks;
            long durability = checked(
                (long)profile.MaxHp + profile.ShellMax);
            long accumulatedDamage = 0L;
            int ordinal = 0;
            while (accumulatedDamage < durability && ordinal < 4096)
            {
                accumulatedDamage = checked(
                    accumulatedDamage
                    + damageRows[ordinal % damageRows.Length]
                        .resolvedPreMitigationDamageUnits);
                ordinal++;
            }
            if (accumulatedDamage < durability)
            {
                failure =
                    "AUTHORITATIVE_FIRST_ENCOUNTER_DAMAGE_SIMULATION_LIMIT";
                return false;
            }
            long terminalTick = checked(cadence * ordinal);

            C1EnemyRuntimeSnapshot simulated = preview.Enemy;
            while (simulated?.Lifecycle == C1EnemyLifecycle.Active)
            {
                C1EnemyActiveActionSnapshot action =
                    simulated.ActiveAction;
                long tick = action == null
                    ? simulated.NextActionDueTick
                    : action.Status == C1EnemyActionStatus.Telegraph
                        ? action.ResolveTick
                        : action.RecoveryEndTick;
                if (tick >= terminalTick)
                {
                    break;
                }
                C1EnemyTransitionResult advanced =
                    C1EnemyActionScheduler.Advance(
                        simulated,
                        tick,
                        reactiveCuePending: false);
                if (advanced.Accepted && advanced.Snapshot != null)
                {
                    simulated = advanced.Snapshot;
                }
            }
            int basicBeforeTerminal = simulated.Cues.Count(
                value => value.Kind == C1EnemyCueKind.BasicAttack
                    && value.BattleTick < terminalTick);
            int skillBeforeTerminal = simulated.Cues.Count(
                value => value.Kind == C1EnemyCueKind.Skill
                    && value.BattleTick < terminalTick);
            int basicResolvedBeforeTerminal =
                simulated.PendingRequests.Count(value =>
                    value.BattleTick < terminalTick
                    && string.Equals(
                        value.RequestKey,
                        C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                        StringComparison.Ordinal));
            int skillResolvedBeforeTerminal =
                simulated.PendingRequests.Count(value =>
                    value.BattleTick < terminalTick
                    && string.Equals(
                        value.RequestKey,
                        C1EnemyRuntimeContract.PollutedPulseSkillRequest,
                        StringComparison.Ordinal));
            if (terminalTick != 21000L
                || ordinal != 14
                || basicBeforeTerminal < 4
                || skillBeforeTerminal < 2
                || basicResolvedBeforeTerminal < 4
                || skillResolvedBeforeTerminal < 2)
            {
                string damageSequence = string.Join(
                    "/",
                    damageRows.Select(value =>
                        value.resolvedPreMitigationDamageUnits.ToString(
                            CultureInfo.InvariantCulture)));
                int actionCount = C1EnemyRuntimeCatalog
                    .GetActionPatterns()
                    .Count(value => string.Equals(
                        value.OwnerRuntimeProfileId,
                        profile.RuntimeProfileId,
                        StringComparison.Ordinal));
                long firstDueTick = C1EnemyRuntimeCatalog
                    .GetActionPatterns()
                    .Where(value => string.Equals(
                        value.OwnerRuntimeProfileId,
                        profile.RuntimeProfileId,
                        StringComparison.Ordinal))
                    .Select(value => value.FirstDueTick)
                    .DefaultIfEmpty(-1L)
                    .Min();
                failure =
                    "AUTHORITATIVE_FIRST_ENCOUNTER_TUNING_NOT_READY:"
                    + "profile=" + profile.RuntimeProfileId
                    + ",hp=" + profile.MaxHp
                    + ",shell=" + profile.ShellMax
                    + ",cadenceTicks=" + cadence
                    + ",terminalTick=" + terminalTick
                    + ",firstDueTick=" + firstDueTick
                    + ",actionPatterns=" + actionCount
                    + ",basicBeforeTerminal="
                    + basicBeforeTerminal
                    + ",skillBeforeTerminal="
                    + skillBeforeTerminal
                    + ",basicResolvedBeforeTerminal="
                    + basicResolvedBeforeTerminal
                    + ",skillResolvedBeforeTerminal="
                    + skillResolvedBeforeTerminal
                    + ",applications=" + ordinal
                    + ",damage=" + damageSequence;
                return false;
            }
            return true;
        }

        public static bool ValidateRunningStage(
            C1EncounterOwnershipFreshPlayContext context,
            int stageIndex,
            out string failure)
        {
            failure = string.Empty;
            if (stageIndex < 0 || stageIndex >= StageIds.Length)
            {
                failure = "STAGE_INDEX_INVALID_" + stageIndex;
                return false;
            }
            string expectedStage = StageIds[stageIndex];
            C1EnemyRuntimeSnapshot enemy =
                context.Flow.NormalBattle?.Enemy;
            V04Chapter1EncounterBindingDefinition binding =
                V04Chapter1EncounterManifest.Bindings.SingleOrDefault(
                    value => string.Equals(
                        value.stageId,
                        expectedStage,
                        StringComparison.Ordinal));
            if (!string.Equals(
                    context.Flow.Snapshot?.currentStageId,
                    expectedStage,
                    StringComparison.Ordinal)
                || enemy == null
                || binding == null
                || !string.Equals(
                    enemy.ContentId,
                    binding.activeEnemyContentId,
                    StringComparison.Ordinal)
                || context.Flow.EncounterAdmission?.IsOrdinary != true
                || !string.Equals(
                    context.Flow.EncounterAdmission.RuntimeProfileId,
                    enemy.RuntimeProfileId,
                    StringComparison.Ordinal))
            {
                failure = "STAGE_RUNTIME_ID_MISMATCH:"
                    + context.Flow.Snapshot?.currentStageId + "/"
                    + enemy?.ContentId + "/"
                    + enemy?.RuntimeProfileId;
                return false;
            }
            if (context.P3.HasActiveSession
                || !context.Binder.OrdinaryAdmissionActive
                || !context.Binder.OrdinaryGenerationActive
                || !context.Journey.OwnsAuthoredSlot)
            {
                failure = "ORDINARY_OWNER_OR_SLOT_NOT_ACTIVE";
                return false;
            }
            string hp = context.Binder.EnemyHpText?.text ?? string.Empty;
            if (!hp.Contains(
                    enemy.CurrentHp.ToString(
                        CultureInfo.InvariantCulture))
                || !hp.Contains(
                    enemy.MaxHp.ToString(
                        CultureInfo.InvariantCulture))
                || hp.Contains("980/980"))
            {
                failure = "AUTHORED_HP_STILL_BOSS:" + hp;
                return false;
            }
            string fallback =
                C1JourneyPresentationProfile.TemporaryFallbackTag
                + (stageIndex >= 7
                    ? "_HELD_VISUAL_SLOT_"
                    : "_MISSING_PROFILE_STATES_")
                + expectedStage;
            if (!context.Journey.Diagnostic.Contains(fallback))
            {
                failure = "VISUAL_FALLBACK_MISMATCH:"
                    + context.Journey.Diagnostic;
                return false;
            }
            int activeSources = Resources
                .FindObjectsOfTypeAll<MonoBehaviour>()
                .Where(value => value != null
                    && value.gameObject.scene
                        == context.Flow.gameObject.scene)
                .OfType<
                    IBattleSandboxAcceptedDamagePresentationSource>()
                .Count(value => value.HasActivePresentationSession);
            if (activeSources != 1)
            {
                failure =
                    "ACTIVE_PRESENTATION_SOURCE_COUNT_"
                    + activeSources;
                return false;
            }
            return true;
        }

        public static int VisualStateBit(
            C1JourneyEnemyVisualState state)
        {
            return state switch
            {
                C1JourneyEnemyVisualState.Idle => 1,
                C1JourneyEnemyVisualState.BasicAttack => 2,
                C1JourneyEnemyVisualState.Skill => 4,
                C1JourneyEnemyVisualState.Hit => 8,
                C1JourneyEnemyVisualState.Death => 16,
                _ => 0
            };
        }

        public static string Fingerprint(Image image)
        {
            RectTransform rect = image?.rectTransform;
            if (image == null || rect == null)
            {
                return string.Empty;
            }
            return string.Join("|", new[]
            {
                rect.anchoredPosition.ToString("R"),
                rect.sizeDelta.ToString("R"),
                rect.anchorMin.ToString("R"),
                rect.anchorMax.ToString("R"),
                rect.pivot.ToString("R"),
                rect.localScale.ToString("R"),
                rect.localRotation.eulerAngles.ToString("R"),
                image.sprite?.name ?? string.Empty,
                image.material?.name ?? string.Empty,
                image.color.ToString(),
                image.enabled.ToString(),
                image.preserveAspect.ToString(),
                image.raycastTarget.ToString()
            });
        }

        public static bool HasNoRuntimeRootLeak()
        {
            return !Resources.FindObjectsOfTypeAll<GameObject>()
                .Any(value => value != null
                    && (string.Equals(
                            value.name,
                            C1JourneyPresentationProfile.RuntimeRootName,
                            StringComparison.Ordinal)
                        || string.Equals(
                            value.name,
                            C1JourneyPresentationProfile.PanelRootName,
                            StringComparison.Ordinal)
                        || string.Equals(
                            value.name,
                            V04Chapter1ContinuousBattleIntegrationContract
                                .InjectedRootName,
                            StringComparison.Ordinal)));
        }

        private static T Single<T>(
            Scene scene,
            Func<T, bool> predicate = null)
            where T : Component
        {
            T[] values = Resources.FindObjectsOfTypeAll<T>()
                .Where(value => value != null
                    && value.gameObject.scene == scene
                    && (predicate == null || predicate(value)))
                .ToArray();
            return values.Length == 1 ? values[0] : null;
        }
    }
}
