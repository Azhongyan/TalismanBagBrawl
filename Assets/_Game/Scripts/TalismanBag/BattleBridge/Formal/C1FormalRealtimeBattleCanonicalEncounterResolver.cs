using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.V04.ChapterFlow.Chapter1;

namespace TalismanBag.BattleBridge.Formal
{
    // This projection is deliberately bounded to the currently released
    // single-wave formal route: stages 1-1 through 1-5.
    internal static class C1FormalRealtimeBattleCanonicalEncounterResolver
    {
        private const string ResolvedSchemaId =
            "C1FormalEnemyResolvedEncounter.v1";

        internal static bool TryResolve(
            C1FormalRealtimeBattleSessionRequest request,
            out C1FormalRealtimeBattleResolvedEncounter resolved,
            out C1FormalRealtimeBattleError error)
        {
            resolved = null;
            if (!IsCurrentFormalStage(request.stageId))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.ContextRejected,
                    "The canonical Enemy projection is limited to formal stages 1-1 through 1-5.");
                return false;
            }
            if (!string.Equals(
                    request.expectedEnemyCatalogCanonical,
                    C1FormalEnemyDefinitionCatalog.CatalogId,
                    StringComparison.Ordinal))
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.EnemyCatalogMismatch,
                    "The formal Enemy catalog does not match the current campaign definitions.");
                return false;
            }

            V04Chapter1StageWavePlanDefinition stage =
                V04Chapter1StageWaveEncounterTruth.FindPlan(request.stageId);
            if (stage == null || stage.waves.Count != 1)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.EnemyProfileRejected,
                    "A released 1-1 through 1-5 stage must declare exactly one wave.");
                return false;
            }

            V04Chapter1WavePlanDefinition wave = stage.waves[0];
            var actors = new List<C1FormalRealtimeBattleResolvedActor>();
            foreach (V04Chapter1ActorPlanDefinition plan in wave.actors
                         .OrderBy(value => value.slotOrdinal))
            {
                C1FormalEnemyDefinition definition =
                    C1FormalEnemyDefinitionCatalog.FindByContentId(
                        plan.enemyContentId);
                if (!IsAcceptedDefinition(definition, plan))
                {
                    error = Error(
                        C1FormalRealtimeBattleErrorCodes.EnemyProfileRejected,
                        "A Stage Actor does not resolve to one formal Enemy definition.");
                    return false;
                }

                C1FormalEnemyAttackDefinition attack =
                    definition.PrimaryAttack;
                actors.Add(new C1FormalRealtimeBattleResolvedActor(
                    plan.waveEntryId,
                    definition.ContentId,
                    definition.RuntimeProfileId,
                    definition.RuntimeProfileId,
                    plan.occurrenceOrdinal,
                    definition.MaxHp,
                    definition.ActionIds,
                    definition.EffectRequestKeys,
                    Array.Empty<string>(),
                    definition.ContentId,
                    definition.MaxShell > 0,
                    definition.MaxShell > 0
                        ? "mechanic.layered_shield"
                        : string.Empty,
                    definition.MaxShell,
                    definition.InitialShell,
                    0,
                    "NO_REGENERATION",
                    "enemy.resource.shell",
                    "enemy.shell_state.broken",
                    "counter_window.shell_break",
                    attack.ActionId,
                    attack.EffectRequestKey,
                    attack.Damage,
                    attack.FirstDueMilliseconds,
                    attack.RepeatIntervalMilliseconds,
                    definition.Skills));
            }

            if (actors.Count == 0)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.EnemyProfileRejected,
                    "The formal stage wave has no Enemy actors.");
                return false;
            }

            resolved = new C1FormalRealtimeBattleResolvedEncounter(
                ResolvedSchemaId,
                request.productContext,
                request.balanceProfileId,
                request.encounterVariantId,
                request.stageId,
                C1FormalEnemyDefinitionCatalog.CatalogId,
                wave.waveId,
                actors,
                (C1FormalRealtimeBattleResolvedBoneSwapDefinition)null);
            error = Error(C1FormalRealtimeBattleErrorCodes.None, string.Empty);
            return true;
        }

        private static bool IsCurrentFormalStage(string stageId)
        {
            return string.Equals(stageId, "1-1", StringComparison.Ordinal)
                || string.Equals(stageId, "1-2", StringComparison.Ordinal)
                || string.Equals(stageId, "1-3", StringComparison.Ordinal)
                || string.Equals(stageId, "1-4", StringComparison.Ordinal)
                || string.Equals(stageId, "1-5", StringComparison.Ordinal);
        }

        private static bool IsAcceptedDefinition(
            C1FormalEnemyDefinition definition,
            V04Chapter1ActorPlanDefinition plan)
        {
            return definition != null
                && plan != null
                && plan.HasRuntimeProfile
                && string.Equals(
                    definition.ContentId,
                    plan.enemyContentId,
                    StringComparison.Ordinal)
                && string.Equals(
                    definition.RuntimeProfileId,
                    plan.runtimeProfileId,
                    StringComparison.Ordinal)
                && string.Equals(
                    definition.PresentationKey,
                    plan.visualProfileKey,
                    StringComparison.Ordinal)
                && definition.MaxHp > 0
                && definition.MaxShell >= 0
                && definition.PrimaryAttack != null
                && definition.PrimaryAttack.Damage > 0
                && definition.PrimaryAttack.FirstDueMilliseconds > 0L
                && definition.PrimaryAttack.RepeatIntervalMilliseconds > 0L
                && definition.ActionIds.Count ==
                    definition.EffectRequestKeys.Count
                && definition.ActionIds.Contains(
                    definition.PrimaryAttack.ActionId,
                    StringComparer.Ordinal);
        }

        private static C1FormalRealtimeBattleError Error(
            string code,
            string message)
        {
            return new C1FormalRealtimeBattleError(code, message);
        }
    }
}
