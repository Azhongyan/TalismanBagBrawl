using TalismanBag.V02.Run;

namespace TalismanBag.V02.CoreLoop.MainTrial
{
    public sealed class PostBattlePrepareRequest
    {
        public bool IsRequested { get; private set; }
        public string RequestedLevelId { get; private set; } = string.Empty;

        public bool Request(V02RoundConfig round)
        {
            if (IsRequested)
            {
                return false;
            }

            IsRequested = true;
            RequestedLevelId = round != null ? round.levelId : string.Empty;
            return true;
        }

        public bool ConsumeIfMatches(V02RoundConfig round)
        {
            if (!IsRequested)
            {
                return false;
            }

            bool matches = string.IsNullOrWhiteSpace(RequestedLevelId) ||
                           round == null ||
                           string.Equals(RequestedLevelId, round.levelId, System.StringComparison.Ordinal);
            Clear();
            return matches;
        }

        public void Clear()
        {
            IsRequested = false;
            RequestedLevelId = string.Empty;
        }
    }
}
