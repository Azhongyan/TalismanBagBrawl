using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;

namespace TalismanBag.BattleBridge.Formal
{
    internal sealed class C1FormalRealtimeBattleResolvedActor
    {
        internal C1FormalRealtimeBattleResolvedActor(
            string actorBalanceId,
            string contentId,
            string runtimeProfileId,
            string operatorProfileId,
            int occurrenceOrdinal,
            int maxHp,
            IEnumerable<string> actionIds,
            IEnumerable<string> effectRequestKeys,
            IEnumerable<string> downstreamConditions,
            string canonicalSignature,
            bool hasShell,
            string shellMechanicId,
            int maxShell,
            int initialShell,
            int shellRegenerationCount,
            string shellRegenerationPolicyId,
            string shellBreakTargetId,
            string shellBrokenStateId,
            string shellBreakCounterWindowId,
            string primaryAttackActionId = "",
            string primaryAttackEffectRequestKey = "",
            int primaryAttackDamage = 0,
            long primaryAttackFirstDueMilliseconds = 0L,
            long primaryAttackIntervalMilliseconds = 0L,
            IEnumerable<C1FormalEnemySkillDefinition> skills = null)
        {
            this.actorBalanceId = actorBalanceId ?? string.Empty;
            this.contentId = contentId ?? string.Empty;
            this.runtimeProfileId = runtimeProfileId ?? string.Empty;
            this.operatorProfileId = operatorProfileId ?? string.Empty;
            this.occurrenceOrdinal = occurrenceOrdinal;
            this.maxHp = maxHp;
            this.actionIds = Array.AsReadOnly((actionIds
                ?? Enumerable.Empty<string>()).ToArray());
            this.effectRequestKeys = Array.AsReadOnly((effectRequestKeys
                ?? Enumerable.Empty<string>()).ToArray());
            this.downstreamConditions = Array.AsReadOnly((downstreamConditions
                ?? Enumerable.Empty<string>()).ToArray());
            this.canonicalSignature = canonicalSignature ?? string.Empty;
            this.hasShell = hasShell;
            this.shellMechanicId = shellMechanicId ?? string.Empty;
            this.maxShell = maxShell;
            this.initialShell = initialShell;
            this.shellRegenerationCount = shellRegenerationCount;
            this.shellRegenerationPolicyId =
                shellRegenerationPolicyId ?? string.Empty;
            this.shellBreakTargetId = shellBreakTargetId ?? string.Empty;
            this.shellBrokenStateId = shellBrokenStateId ?? string.Empty;
            this.shellBreakCounterWindowId =
                shellBreakCounterWindowId ?? string.Empty;
            this.primaryAttackActionId =
                primaryAttackActionId ?? string.Empty;
            this.primaryAttackEffectRequestKey =
                primaryAttackEffectRequestKey ?? string.Empty;
            this.primaryAttackDamage = primaryAttackDamage;
            this.primaryAttackFirstDueMilliseconds =
                primaryAttackFirstDueMilliseconds;
            this.primaryAttackIntervalMilliseconds =
                primaryAttackIntervalMilliseconds;
            this.skills = Array.AsReadOnly((skills
                ?? Enumerable.Empty<C1FormalEnemySkillDefinition>()).ToArray());
        }

        internal string actorBalanceId { get; }
        internal string contentId { get; }
        internal string runtimeProfileId { get; }
        internal string operatorProfileId { get; }
        internal int occurrenceOrdinal { get; }
        internal int maxHp { get; }
        internal IReadOnlyList<string> actionIds { get; }
        internal IReadOnlyList<string> effectRequestKeys { get; }
        internal IReadOnlyList<string> downstreamConditions { get; }
        internal string canonicalSignature { get; }
        internal bool hasShell { get; }
        internal string shellMechanicId { get; }
        internal int maxShell { get; }
        internal int initialShell { get; }
        internal int shellRegenerationCount { get; }
        internal string shellRegenerationPolicyId { get; }
        internal string shellBreakTargetId { get; }
        internal string shellBrokenStateId { get; }
        internal string shellBreakCounterWindowId { get; }
        internal string primaryAttackActionId { get; }
        internal string primaryAttackEffectRequestKey { get; }
        internal int primaryAttackDamage { get; }
        internal long primaryAttackFirstDueMilliseconds { get; }
        internal long primaryAttackIntervalMilliseconds { get; }
        internal IReadOnlyList<C1FormalEnemySkillDefinition> skills { get; }
    }

    internal sealed class C1FormalRealtimeBattleResolvedBoneSwapDefinition
    {
        internal C1FormalRealtimeBattleResolvedBoneSwapDefinition(
            string contentId,
            string operatorProfileId,
            string actionId,
            string effectRequestKey,
            string acceptedSourceKind,
            int ratioBasisPoints,
            int capDamage,
            int telegraphMilliseconds,
            int recoverMilliseconds,
            string pendingPolicy)
        {
            ContentId = contentId ?? string.Empty;
            OperatorProfileId = operatorProfileId ?? string.Empty;
            ActionId = actionId ?? string.Empty;
            EffectRequestKey = effectRequestKey ?? string.Empty;
            AcceptedSourceKind = acceptedSourceKind ?? string.Empty;
            RatioBasisPoints = ratioBasisPoints;
            CapDamage = capDamage;
            TelegraphMilliseconds = telegraphMilliseconds;
            RecoverMilliseconds = recoverMilliseconds;
            PendingPolicy = pendingPolicy ?? string.Empty;
        }

        internal string ContentId { get; }
        internal string OperatorProfileId { get; }
        internal string ActionId { get; }
        internal string EffectRequestKey { get; }
        internal string AcceptedSourceKind { get; }
        internal int RatioBasisPoints { get; }
        internal int CapDamage { get; }
        internal int TelegraphMilliseconds { get; }
        internal int RecoverMilliseconds { get; }
        internal string PendingPolicy { get; }
    }

    internal sealed class C1FormalRealtimeBattleResolvedAttackWave
    {
        internal C1FormalRealtimeBattleResolvedAttackWave(
            string cadenceScope,
            string effectRequestKey,
            decimal firstResolveSeconds,
            decimal intervalSeconds,
            decimal damagePerApplication,
            int applicationsPerWave,
            string canonicalSignature)
        {
            this.cadenceScope = cadenceScope ?? string.Empty;
            this.effectRequestKey = effectRequestKey ?? string.Empty;
            this.firstResolveSeconds = firstResolveSeconds;
            this.intervalSeconds = intervalSeconds;
            this.damagePerApplication = damagePerApplication;
            this.applicationsPerWave = applicationsPerWave;
            this.canonicalSignature = canonicalSignature ?? string.Empty;
        }

        internal string cadenceScope { get; }
        internal string effectRequestKey { get; }
        internal decimal firstResolveSeconds { get; }
        internal decimal intervalSeconds { get; }
        internal decimal damagePerApplication { get; }
        internal int applicationsPerWave { get; }
        internal string canonicalSignature { get; }
    }

    internal sealed class C1FormalRealtimeBattleResolvedEncounter
    {
        internal C1FormalRealtimeBattleResolvedEncounter(
            string schemaId,
            string productContext,
            string balanceProfileId,
            string encounterVariantId,
            string stageId,
            string catalogIdentity,
            string profileIdentity,
            IEnumerable<C1FormalRealtimeBattleResolvedActor> actors,
            C1FormalRealtimeBattleResolvedBoneSwapDefinition boneSwapRelease)
        {
            this.schemaId = schemaId ?? string.Empty;
            this.productContext = productContext ?? string.Empty;
            this.balanceProfileId = balanceProfileId ?? string.Empty;
            this.encounterVariantId = encounterVariantId ?? string.Empty;
            this.stageId = stageId ?? string.Empty;
            catalogCanonicalSignature = catalogIdentity ?? string.Empty;
            profileCanonicalSignature = profileIdentity ?? string.Empty;
            this.actors = Array.AsReadOnly((actors
                ?? Enumerable.Empty<C1FormalRealtimeBattleResolvedActor>())
                .ToArray());
            attackWave = null;
            resolvedBoneSwapRelease = boneSwapRelease;
        }

        internal string schemaId { get; }
        internal string productContext { get; }
        internal string balanceProfileId { get; }
        internal string encounterVariantId { get; }
        internal string stageId { get; }
        internal string catalogCanonicalSignature { get; }
        internal string profileCanonicalSignature { get; }
        internal IReadOnlyList<C1FormalRealtimeBattleResolvedActor> actors
        {
            get;
        }
        internal C1FormalRealtimeBattleResolvedAttackWave attackWave { get; }
        internal C1FormalRealtimeBattleResolvedBoneSwapDefinition
            resolvedBoneSwapRelease { get; }
        internal bool isStage3To5 =>
            C1FormalRealtimeBattleEncounterResolver.IsStage3To5(stageId);
    }

    internal static class C1FormalRealtimeBattleEncounterResolver
    {
        internal static bool IsStage3To5(string stageId)
        {
            return string.Equals(stageId, "1-3", StringComparison.Ordinal)
                || string.Equals(stageId, "1-4", StringComparison.Ordinal)
                || string.Equals(stageId, "1-5", StringComparison.Ordinal);
        }

        internal static bool TryResolve(
            C1FormalRealtimeBattleSessionRequest request,
            out C1FormalRealtimeBattleResolvedEncounter resolved,
            out C1FormalRealtimeBattleError error)
        {
            resolved = null;
            if (request == null)
            {
                error = Error(
                    C1FormalRealtimeBattleErrorCodes.RequestNull,
                    "The live Battle Session request is null.");
                return false;
            }

            return C1FormalRealtimeBattleCanonicalEncounterResolver.TryResolve(
                request,
                out resolved,
                out error);
        }

        private static C1FormalRealtimeBattleError Error(
            string code,
            string message)
        {
            return new C1FormalRealtimeBattleError(code, message);
        }
    }
}
