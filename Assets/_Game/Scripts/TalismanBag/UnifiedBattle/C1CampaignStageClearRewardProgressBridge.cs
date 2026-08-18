using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.V04.RewardDrop.Contracts;
using TalismanBag.V04.RewardDrop.Runtime.CampaignLoot;
using TalismanBag.V04.WorldMap;

namespace TalismanBag.UnifiedBattle
{
    public sealed class C1CampaignStageRewardProgressResult
    {
        internal C1CampaignStageRewardProgressResult(
            StageClearFact stageClearFact,
            C1CampaignDirectLootGrantBundle grant,
            C1FormalCampaignLootAcceptedEntitlementReceipt itemReceipt,
            V04CampaignStageCompletionSnapshot completion,
            bool replay,
            bool itemChanged,
            bool completionChanged)
        {
            this.stageClearFact = stageClearFact;
            this.grant = grant;
            this.itemReceipt = itemReceipt;
            this.completion = completion;
            this.replay = replay;
            this.itemChanged = itemChanged;
            this.completionChanged = completionChanged;
        }

        public StageClearFact stageClearFact { get; }
        public C1CampaignDirectLootGrantBundle grant { get; }
        public C1FormalCampaignLootAcceptedEntitlementReceipt itemReceipt { get; }
        public V04CampaignStageCompletionSnapshot completion { get; }
        public bool replay { get; }
        public bool itemChanged { get; }
        public bool completionChanged { get; }
        public string selectedBaseItemId => grant == null
            ? string.Empty
            : grant.selectedBaseItemId;
        public string itemInstanceId => itemReceipt == null
            ? string.Empty
            : itemReceipt.itemInstanceId;
        public string rarityIdentity => itemReceipt == null
            ? string.Empty
            : itemReceipt.rarityVersionIdentity;
    }

    public sealed class C1CampaignStageClearRewardProgressBridge
    {
        public const string AcceptedDiagnostic =
            "C1_CAMPAIGN_STAGE_REWARD_ITEM_COMPLETION_ACCEPTED";
        public const string DuplicateAcceptedDiagnostic =
            "C1_CAMPAIGN_STAGE_REWARD_ITEM_COMPLETION_DUPLICATE_NO_OP";

        private readonly C1FormalItemSessionAuthority itemAuthority;
        private readonly V04CampaignStageCompletionRepository completionRepository;
        private C1CampaignDirectLootSessionSnapshot rewardSession;
        private readonly Dictionary<string, C1CampaignStageRewardProgressResult>
            acceptedByTerminal = new Dictionary<string, C1CampaignStageRewardProgressResult>(
                StringComparer.Ordinal);

        private C1CampaignStageClearRewardProgressBridge(
            C1FormalItemSessionAuthority itemAuthority,
            V04CampaignStageCompletionRepository completionRepository,
            C1CampaignDirectLootSessionSnapshot rewardSession)
        {
            this.itemAuthority = itemAuthority;
            this.completionRepository = completionRepository;
            this.rewardSession = rewardSession;
        }

        public C1CampaignDirectLootSessionSnapshot RewardSession => rewardSession;
        public V04CampaignStageCompletionSnapshot Completion =>
            completionRepository.Snapshot;

        public static bool TryCreate(
            C1FormalItemSessionAuthority itemAuthority,
            V04CampaignStageCompletionRepository completionRepository,
            string runSessionId,
            long rootSeed,
            out C1CampaignStageClearRewardProgressBridge bridge,
            out string diagnostic)
        {
            bridge = null;
            if (itemAuthority == null || completionRepository == null)
            {
                diagnostic = "C1_CAMPAIGN_STAGE_BRIDGE_OWNER_MISSING";
                return false;
            }

            V04CampaignStageCompletionSnapshot completion =
                completionRepository.Load();
            C1CampaignCompletionOnlyProgressProof proof =
                C1CampaignCompletionOnlyProgressProof.Create(
                    completion.completedStageIds);
            C1CampaignDirectLootSessionCreateResult creation =
                C1CampaignDirectLootSessionResolver.CreateSession(
                    runSessionId,
                    rootSeed,
                    proof);
            if (creation == null || !creation.accepted
                || creation.snapshot == null)
            {
                diagnostic = creation == null
                    ? "C1_CAMPAIGN_REWARD_SESSION_RESULT_MISSING"
                    : creation.diagnosticCode;
                return false;
            }

            bridge = new C1CampaignStageClearRewardProgressBridge(
                itemAuthority,
                completionRepository,
                creation.snapshot);
            diagnostic = "C1_CAMPAIGN_STAGE_BRIDGE_CREATED";
            return true;
        }

        public bool TryAcceptVictory(
            BattleLaunchContext context,
            C1FormalRealtimeBattleTerminalResult terminal,
            out C1CampaignStageRewardProgressResult result,
            out string diagnostic)
        {
            result = null;
            if (context == null || terminal == null || !terminal.accepted
                || !terminal.win || terminal.lose
                || terminal.battleResultSnapshot == null
                || !string.Equals(
                    context.StageId,
                    terminal.stageId,
                    StringComparison.Ordinal))
            {
                diagnostic = "C1_CAMPAIGN_STAGE_VICTORY_REQUIRED";
                return false;
            }

            string terminalIdentity = terminal.canonicalSignature ?? string.Empty;
            if (acceptedByTerminal.TryGetValue(terminalIdentity, out result))
            {
                diagnostic = DuplicateAcceptedDiagnostic;
                return true;
            }

            BattleResultSnapshot battleResult = terminal.battleResultSnapshot;
            StageClearFact fact = new StageClearFact(
                StableId(context, terminal, "stage-clear"),
                battleResult.resultId,
                battleResult.requestId,
                terminal.canonicalSignature,
                context.ChapterId,
                context.StageId,
                rewardSession.runSessionId,
                rewardSession.nextExpectedClearSequence,
                false,
                RewardLaunchContextIdentity.CampaignNormalLv1);
            C1CampaignDirectLootResolveResult reward =
                C1CampaignDirectLootSessionResolver.Resolve(
                    rewardSession,
                    fact,
                    itemAuthority.CanonicalItemResolver,
                    itemAuthority.Current.roster
                        .Where(value => !value.isSpecialLightingSource)
                        .GroupBy(value => value.baseItemId, StringComparer.Ordinal)
                        .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal),
                    itemAuthority.Current.itemSystemSnapshot.ToBuildSynergyResolutionResult());
            if (reward == null || !reward.accepted || reward.snapshot == null
                || reward.grant == null || !reward.grant.IsCanonicalValidV2())
            {
                diagnostic = reward == null
                    ? "C1_CAMPAIGN_DIRECT_LOOT_RESULT_MISSING"
                    : reward.diagnosticCode;
                return false;
            }

            string commandId = "c1.mainline.canonical-item.accept." +
                reward.grant.canonicalSignature;
            C1FormalCampaignLootEntitlementAcceptanceResult item =
                itemAuthority.AcceptCanonicalCampaignLootEntitlement(
                    new C1FormalCampaignLootEntitlementCommand(
                        itemAuthority.Current.sessionToken,
                        itemAuthority.Current.resetGeneration,
                        commandId,
                        itemAuthority.Current.canonicalSignature,
                        reward.grant));
            C1FormalCampaignLootAcceptedEntitlementReceipt receipt = item == null
                ? null
                : item.acceptedEntitlementReceipt;
            if (item == null || !item.accepted || item.snapshot == null
                || receipt == null
                || item.snapshot.FindRosterEntry(receipt.itemInstanceId) == null
                || item.entitlementHistory == null
                || item.entitlementHistory.FindByStageClearId(
                    fact.stageClearId) == null)
            {
                diagnostic = item == null
                    ? "C1_CAMPAIGN_ITEM_ENTITLEMENT_RESULT_MISSING"
                    : item.diagnosticCode;
                return false;
            }

            bool wasCleared = completionRepository.IsCleared(context.StageId);
            if (!completionRepository.MarkCleared(context.StageId))
            {
                diagnostic = "C1_CAMPAIGN_COMPLETION_MARK_REJECTED stageId=" +
                             context.StageId;
                return false;
            }

            rewardSession = reward.snapshot;
            result = new C1CampaignStageRewardProgressResult(
                fact,
                reward.grant,
                receipt,
                completionRepository.Snapshot,
                reward.grant.admissionLineage.acceptedAcquisitionMode ==
                RewardAcquisitionMode.RepeatChallengeOnline,
                item.changed,
                !wasCleared);
            acceptedByTerminal.Add(terminalIdentity, result);
            diagnostic = reward.isIdempotentRetry || !item.changed
                ? DuplicateAcceptedDiagnostic
                : AcceptedDiagnostic;
            return true;
        }

        public bool TryCreateCumulativeBattleInput(
            out C1FormalItemBattleInputSnapshot itemInput,
            out string diagnostic)
        {
            itemInput = null;
            C1FormalItemSessionSnapshot itemSession = itemAuthority.Current;
            C1FormalItemBattleInputSnapshot source =
                itemAuthority.CreateBattleInputSnapshot();
            if (source == null || itemSession == null
                || !string.Equals(
                    source.sessionCanonicalSignature,
                    itemSession.canonicalSignature,
                    StringComparison.Ordinal))
            {
                diagnostic = "C1_CAMPAIGN_ITEM_BATTLE_SNAPSHOT_MISSING";
                return false;
            }

            itemInput = source;
            diagnostic = "C1_CAMPAIGN_CUMULATIVE_ITEM_BATTLE_INPUT_VALID";
            return true;
        }

        private static string StableId(
            BattleLaunchContext context,
            C1FormalRealtimeBattleTerminalResult terminal,
            string suffix)
        {
            return string.Join(".", new[]
            {
                "c1.mainline",
                context.LaunchId,
                context.Generation.ToString(CultureInfo.InvariantCulture),
                terminal.sessionId,
                suffix
            });
        }
    }
}
