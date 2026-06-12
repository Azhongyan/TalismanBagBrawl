namespace TalismanBag.V02.Result
{
    public enum V02FailureReason
    {
        None,
        UnpoweredFormation,
        LackShieldBreak,
        LackCleanse,
        FormationEnergyBroken,
        CoreLineSealed,
        LackGroupClear,
        BossMixedPressure,
        LowDefense
    }

    public readonly struct V02FailureReasonResult
    {
        public readonly V02FailureReason reason;
        public readonly string title;
        public readonly string description;
        public readonly string suggestion;

        public V02FailureReasonResult(V02FailureReason reason, string title, string description, string suggestion)
        {
            this.reason = reason;
            this.title = title;
            this.description = description;
            this.suggestion = suggestion;
        }
    }
}
