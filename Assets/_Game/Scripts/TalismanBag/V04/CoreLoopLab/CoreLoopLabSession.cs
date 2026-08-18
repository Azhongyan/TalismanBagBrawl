using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Contracts.Battle;

namespace TalismanBag.V04.CoreLoopLab
{
    public static class CoreLoopLabFixedContent
    {
        public const string ProductContext = "PLAYTEST_VERTICAL_SLICE";

        public static IReadOnlyList<string> RewardIds =>
            CoreLoopLabRewardProfile.Shared.RewardCandidateBaseItemIds;

        public static bool IsTwoBattleProfile(CoreLoopLabProfile profile)
        {
            return profile == CoreLoopLabProfile.B_TwoBattleRewardLoop
                || profile ==
                    CoreLoopLabProfile.C_TwoBattleSemanticPresentation;
        }

        public static string ContractProfileId(CoreLoopLabProfile profile)
        {
            return profile switch
            {
                CoreLoopLabProfile.A_CurrentSingleBattle =>
                    BattleSandboxDevBattleSessionRequest.ProfileA,
                CoreLoopLabProfile.B_TwoBattleRewardLoop =>
                    BattleSandboxDevBattleSessionRequest.ProfileB,
                _ => BattleSandboxDevBattleSessionRequest.ProfileC
            };
        }

        public static string MechanicsProfileId(CoreLoopLabProfile profile)
        {
            return IsTwoBattleProfile(profile)
                ? BattleSandboxDevBattleSessionRequest.TwoBattleMechanics
                : BattleSandboxDevBattleSessionRequest.SingleBattleMechanics;
        }
    }

    public sealed class CoreLoopLabSession
    {
        private readonly List<string> logicLedger = new();
        private readonly List<string> temporaryLoadoutRewardIds = new();

        public CoreLoopLabSession(CoreLoopLabProfile profile)
        {
            Profile = profile;
            Phase = CoreLoopLabPhase.FirstPreparation;
            Record(CoreLoopLabFixedContent.IsTwoBattleProfile(profile)
                ? "session:start|flow=two_battle_fixed_reward"
                : "session:start|flow=current_single_battle");
        }

        public CoreLoopLabProfile Profile { get; }
        public CoreLoopLabPhase Phase { get; private set; }
        public string SelectedRewardId { get; private set; } = string.Empty;
        public IReadOnlyList<string> LogicLedger => logicLedger;
        public IReadOnlyList<string> TemporaryLoadoutRewardIds =>
            temporaryLoadoutRewardIds;

        public bool AcceptBattleStarted(
            int battleIndex,
            BattleSandboxDevBattleSessionAcceptedStart acceptedStart)
        {
            CoreLoopLabPhase expected = battleIndex == 1
                ? CoreLoopLabPhase.FirstPreparation
                : CoreLoopLabPhase.SecondPreparation;
            BattleSandboxDevBattleSessionSnapshot snapshot =
                acceptedStart?.StartSnapshot;
            if (Phase != expected
                || snapshot == null
                || acceptedStart.AcceptedBattleIndex != battleIndex
                || snapshot.BattleIndex != battleIndex
                || !string.Equals(snapshot.MechanicsProfileId,
                    CoreLoopLabFixedContent.MechanicsProfileId(Profile),
                    StringComparison.Ordinal)
                || (battleIndex == 2
                    && !string.Equals(snapshot.SelectedRewardBaseItemId,
                        SelectedRewardId,
                        StringComparison.Ordinal)))
            {
                return false;
            }

            Phase = battleIndex == 1
                ? CoreLoopLabPhase.FirstBattle
                : CoreLoopLabPhase.SecondBattle;
            Record(BuildBattleEntry("battle:start", battleIndex, snapshot));
            return true;
        }

        public bool AcceptBattleCompleted(
            int battleIndex,
            BattleSandboxDevBattleSessionSnapshot snapshot)
        {
            CoreLoopLabPhase expected = battleIndex == 1
                ? CoreLoopLabPhase.FirstBattle
                : CoreLoopLabPhase.SecondBattle;
            if (Phase != expected || snapshot?.IsTerminal != true)
            {
                return false;
            }

            Record(BuildBattleEntry("battle:result", battleIndex, snapshot));
            if (battleIndex == 1
                && CoreLoopLabFixedContent.IsTwoBattleProfile(Profile))
            {
                Phase = CoreLoopLabPhase.RewardChoice;
                Record("reward:offer|ids="
                    + string.Join(",", CoreLoopLabFixedContent.RewardIds));
            }
            else
            {
                Phase = CoreLoopLabPhase.Result;
                Record("session:result");
            }
            return true;
        }

        public bool SelectFixedReward(string rewardId)
        {
            string safeRewardId = rewardId?.Trim() ?? string.Empty;
            if (Phase != CoreLoopLabPhase.RewardChoice
                || !CoreLoopLabFixedContent.RewardIds.Contains(
                    safeRewardId,
                    StringComparer.Ordinal))
            {
                return false;
            }

            SelectedRewardId = safeRewardId;
            temporaryLoadoutRewardIds.Clear();
            temporaryLoadoutRewardIds.Add(safeRewardId);
            Record("reward:selected|id=" + safeRewardId);
            Phase = CoreLoopLabPhase.SecondPreparation;
            Record("prepare:second");
            return true;
        }

        public CoreLoopLabSessionSnapshot BuildSnapshot()
        {
            string[] ledgerCopy = logicLedger.ToArray();
            return new CoreLoopLabSessionSnapshot(
                Profile,
                Phase,
                SelectedRewardId,
                temporaryLoadoutRewardIds.ToArray(),
                ledgerCopy,
                CoreLoopLabStableFingerprint.Compute(
                    string.Join("\n", ledgerCopy)));
        }

        private void Record(string entry)
        {
            logicLedger.Add(entry ?? string.Empty);
        }

        private static string BuildBattleEntry(
            string kind,
            int battleIndex,
            BattleSandboxDevBattleSessionSnapshot snapshot)
        {
            return string.Join("|", new[]
            {
                kind,
                "index=" + battleIndex.ToString(CultureInfo.InvariantCulture),
                "mechanics=" + snapshot.MechanicsProfileId,
                "encounter=" + snapshot.EncounterProfileId,
                "profileFingerprint=" + snapshot.EncounterProfileFingerprint,
                "playerMax=" + snapshot.PlayerMaxHp.ToString(
                    CultureInfo.InvariantCulture),
                "playerFinal=" + snapshot.PlayerCurrentHp.ToString(
                    CultureInfo.InvariantCulture),
                "shieldFinal=" + snapshot.PlayerCurrentShield.ToString(
                    CultureInfo.InvariantCulture),
                "enemyMax=" + snapshot.EnemyMaxHp.ToString(
                    CultureInfo.InvariantCulture),
                "enemyFinal=" + snapshot.EnemyCurrentHp.ToString(
                    CultureInfo.InvariantCulture),
                "nianGenerated=" + snapshot.NianGenerated.ToString(
                    CultureInfo.InvariantCulture),
                "nianSpent=" + snapshot.NianSpent.ToString(
                    CultureInfo.InvariantCulture),
                "nianRejected=" + snapshot.NianRejected.ToString(
                    CultureInfo.InvariantCulture),
                "itemsAccepted=" + snapshot.AcceptedItemApplicationCount
                    .ToString(CultureInfo.InvariantCulture),
                "itemsRejected=" + snapshot.RejectedItemApplicationCount
                    .ToString(CultureInfo.InvariantCulture),
                "basic=" + snapshot.AcceptedEnemyBasicAttackCount.ToString(
                    CultureInfo.InvariantCulture),
                "skill=" + snapshot.AcceptedEnemySkillCount.ToString(
                    CultureInfo.InvariantCulture),
                "playerDamage=" + snapshot.AcceptedEnemyPlayerDamage.ToString(
                    CultureInfo.InvariantCulture),
                "durationMs=" + snapshot.ElapsedMilliseconds.ToString(
                    CultureInfo.InvariantCulture),
                "outcome=" + snapshot.Outcome,
                "ledger=" + snapshot.NonPresentationLedgerFingerprint
            });
        }
    }

    public static class CoreLoopLabStableFingerprint
    {
        public static string Compute(string value)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            foreach (char character in value ?? string.Empty)
            {
                hash ^= character;
                hash *= prime;
            }
            return hash.ToString("X16", CultureInfo.InvariantCulture);
        }
    }
}
