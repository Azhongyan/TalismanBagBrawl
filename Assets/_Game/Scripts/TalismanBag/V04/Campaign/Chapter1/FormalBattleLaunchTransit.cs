using System;
using TalismanBag.Contracts.Battle;
using UnityEngine;

namespace TalismanBag.V04.Campaign.Chapter1
{
    public static class FormalBattleLaunchTransit
    {
        private static BattleLaunchContext pendingContext;
        private static long lastPublishedGeneration;

        public static long NextGeneration => lastPublishedGeneration + 1L;
        public static bool HasPendingContext => pendingContext != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAtSubsystemRegistration()
        {
            pendingContext = null;
            lastPublishedGeneration = 0L;
        }

        public static bool TryPublish(
            Chapter1CampaignStageConfig config,
            BattleLaunchContext candidate,
            out BattleLaunchContext published,
            out string diagnostic)
        {
            published = null;
            if (pendingContext != null)
            {
                if (pendingContext.HasSameEnvelope(candidate))
                {
                    published = pendingContext;
                    diagnostic = "FORMAL_LAUNCH_PUBLISH_IDEMPOTENT";
                    return true;
                }

                diagnostic = "FORMAL_LAUNCH_PENDING_CONFLICT";
                return false;
            }

            if (!ValidateCandidate(config, candidate, out diagnostic))
            {
                return false;
            }

            long expectedGeneration = lastPublishedGeneration + 1L;
            if (candidate.Generation != expectedGeneration)
            {
                diagnostic = candidate.Generation <= lastPublishedGeneration
                    ? "FORMAL_LAUNCH_STALE_GENERATION"
                    : "FORMAL_LAUNCH_GENERATION_MISMATCH";
                return false;
            }

            pendingContext = candidate;
            lastPublishedGeneration = candidate.Generation;
            published = pendingContext;
            diagnostic = "FORMAL_LAUNCH_PUBLISHED";
            return true;
        }

        public static bool TryConsume(
            out BattleLaunchContext consumed,
            out Chapter1CampaignStageConfig config,
            out string diagnostic)
        {
            consumed = null;
            config = null;
            BattleLaunchContext candidate = pendingContext;
            if (candidate == null)
            {
                diagnostic = "FORMAL_LAUNCH_CONTEXT_MISSING";
                return false;
            }

            if (!Chapter1CampaignStageCatalog.TryGet(
                    candidate.StageId,
                    out config,
                    out diagnostic))
            {
                pendingContext = null;
                return false;
            }

            return TryConsume(config, out consumed, out diagnostic);
        }

        public static bool TryConsume(
            Chapter1CampaignStageConfig config,
            out BattleLaunchContext consumed,
            out string diagnostic)
        {
            consumed = null;
            BattleLaunchContext candidate = pendingContext;
            if (candidate == null)
            {
                diagnostic = "FORMAL_LAUNCH_CONTEXT_MISSING";
                return false;
            }

            if (!ValidateCandidate(config, candidate, out diagnostic))
            {
                pendingContext = null;
                return false;
            }

            if (candidate.Generation != lastPublishedGeneration)
            {
                pendingContext = null;
                diagnostic = "FORMAL_LAUNCH_CONSUME_GENERATION_MISMATCH";
                return false;
            }

            pendingContext = null;
            consumed = candidate;
            diagnostic = "FORMAL_LAUNCH_CONSUMED";
            return true;
        }

        public static bool TryRevoke(
            string launchId,
            string token,
            long generation,
            out string diagnostic)
        {
            if (pendingContext == null)
            {
                diagnostic = "FORMAL_LAUNCH_REVOKE_NO_PENDING";
                return false;
            }

            if (!EqualsOrdinal(pendingContext.LaunchId, launchId)
                || !EqualsOrdinal(pendingContext.Token, token)
                || pendingContext.Generation != generation)
            {
                diagnostic = "FORMAL_LAUNCH_REVOKE_ENVELOPE_MISMATCH";
                return false;
            }

            pendingContext = null;
            diagnostic = "FORMAL_LAUNCH_REVOKED";
            return true;
        }

        public static void ClearResidualContext()
        {
            pendingContext = null;
        }

#if UNITY_EDITOR
        public static void ResetForDeterministicValidation()
        {
            ResetAtSubsystemRegistration();
        }
#endif

        private static bool ValidateCandidate(
            Chapter1CampaignStageConfig config,
            BattleLaunchContext candidate,
            out string diagnostic)
        {
            diagnostic = "FORMAL_LAUNCH_STAGE_CONFIG_MISSING";
            if (config == null || !config.TryValidate(out diagnostic))
            {
                return false;
            }

            if (!config.RouteEnabled)
            {
                diagnostic = "FORMAL_LAUNCH_STAGE_ROUTE_DISABLED";
                return false;
            }

            if (candidate == null)
            {
                diagnostic = "FORMAL_LAUNCH_CONTEXT_MISSING";
                return false;
            }

            if (!EqualsOrdinal(candidate.Schema, BattleLaunchContext.SchemaId)
                || candidate.SchemaVersion != BattleLaunchContext.CurrentSchemaVersion)
            {
                diagnostic = "FORMAL_LAUNCH_SCHEMA_MISMATCH";
                return false;
            }

            if (!EqualsOrdinal(
                    candidate.ProductContext,
                    Chapter1CampaignStageConfig.ProductContextId))
            {
                diagnostic = "FORMAL_LAUNCH_PRODUCT_CONTEXT_REJECTED";
                return false;
            }

            if (string.IsNullOrWhiteSpace(candidate.LaunchId)
                || string.IsNullOrWhiteSpace(candidate.Token))
            {
                diagnostic = "FORMAL_LAUNCH_TOKEN_MISSING";
                return false;
            }

            if (!EqualsOrdinal(
                    candidate.SourceRoute,
                    Chapter1CampaignStageConfig.SourceRouteId)
                || !EqualsOrdinal(
                    candidate.ReturnRoute,
                    Chapter1CampaignStageConfig.ReturnRouteId))
            {
                diagnostic = "FORMAL_LAUNCH_ROUTE_MISMATCH";
                return false;
            }

            if (!config.MatchesContext(candidate))
            {
                diagnostic = "FORMAL_LAUNCH_STAGE_CONFIG_MISMATCH";
                return false;
            }

            diagnostic = "FORMAL_LAUNCH_CONTEXT_VALID";
            return true;
        }

        private static bool EqualsOrdinal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }
    }
}
