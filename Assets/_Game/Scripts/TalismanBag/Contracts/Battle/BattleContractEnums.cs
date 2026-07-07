namespace TalismanBag.Contracts.Battle
{
    public enum BattleContractEnergyState
    {
        Unknown = 0,
        None = 1,
        WeakPulse = 2,
        Powered = 3,
        Suppressed = 4
    }

    public enum BattleResultType
    {
        Unknown = 0,
        Win = 1,
        Lose = 2,
        Abandon = 3
    }

    public enum BattleEntrySource
    {
        Unknown = 0,
        V02RunFlow = 1,
        V03MainTrial = 2,
        V04BuildSandbox = 3,
        EditorValidation = 4
    }
}
