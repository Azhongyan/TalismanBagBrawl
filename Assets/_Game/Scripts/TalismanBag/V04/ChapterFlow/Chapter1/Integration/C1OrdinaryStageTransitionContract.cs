namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public enum C1OrdinaryStageTransitionPhase
    {
        None = 0,
        VictoryAccepted = 1,
        ExitRun = 2,
        FadeOut = 3,
        StageSwitchWhileBlack = 4,
        FadeIn = 5,
        PlayerEntry = 6,
        EnemyReveal = 7,
        NextBattleReady = 8,
        BattleRunning = 9
    }

    public sealed class C1OrdinaryStageTransitionSnapshot
    {
        public static readonly C1OrdinaryStageTransitionSnapshot None =
            new(
                0L,
                C1OrdinaryStageTransitionPhase.None,
                string.Empty,
                string.Empty);

        public C1OrdinaryStageTransitionSnapshot(
            long sequence,
            C1OrdinaryStageTransitionPhase phase,
            string currentStageId,
            string nextStageId)
        {
            Sequence = sequence;
            Phase = phase;
            CurrentStageId = currentStageId ?? string.Empty;
            NextStageId = nextStageId ?? string.Empty;
        }

        public long Sequence { get; }
        public C1OrdinaryStageTransitionPhase Phase { get; }
        public string CurrentStageId { get; }
        public string NextStageId { get; }
        public bool HasSequence => Sequence > 0L;
    }
}
