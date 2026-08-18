using System;
using System.Collections.Generic;

namespace TalismanBag.V04.CoreLoopLab
{
    public enum CoreLoopLabProfile
    {
        A_CurrentSingleBattle = 0,
        B_TwoBattleRewardLoop = 1,
        C_TwoBattleSemanticPresentation = 2
    }

    public enum CoreLoopLabPhase
    {
        FirstPreparation = 0,
        FirstBattle = 1,
        RewardChoice = 2,
        SecondPreparation = 3,
        SecondBattle = 4,
        Result = 5
    }

    public sealed class CoreLoopLabSessionSnapshot
    {
        public CoreLoopLabSessionSnapshot(
            CoreLoopLabProfile profile,
            CoreLoopLabPhase phase,
            string selectedRewardId,
            IReadOnlyList<string> temporaryLoadoutRewardIds,
            IReadOnlyList<string> logicLedger,
            string logicLedgerFingerprint)
        {
            Profile = profile;
            Phase = phase;
            SelectedRewardId = selectedRewardId ?? string.Empty;
            TemporaryLoadoutRewardIds = temporaryLoadoutRewardIds
                ?? Array.Empty<string>();
            LogicLedger = logicLedger ?? Array.Empty<string>();
            LogicLedgerFingerprint = logicLedgerFingerprint ?? string.Empty;
        }

        public CoreLoopLabProfile Profile { get; }
        public CoreLoopLabPhase Phase { get; }
        public string SelectedRewardId { get; }
        public IReadOnlyList<string> TemporaryLoadoutRewardIds { get; }
        public IReadOnlyList<string> LogicLedger { get; }
        public string LogicLedgerFingerprint { get; }
    }
}
