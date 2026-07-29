using System;
using System.Collections.Generic;
using TalismanBag.BattleBridge.NianResource;

namespace TalismanBag.BuildSandbox
{
    public sealed class ItemLivingGradientOutlineAcceptedEventGate
    {
        private const int RememberedKeyCap = 64;
        private readonly HashSet<string> acceptedKeys =
            new(StringComparer.Ordinal);
        private readonly Queue<string> acceptedKeyOrder = new();
        private int generation;
        private long lastAcceptedTick = -1L;

        public int AcceptedCount { get; private set; }
        public int RejectedCount { get; private set; }
        public int Generation => generation;
        public long LastAcceptedTick => lastAcceptedTick;
        public string LastRejectReason { get; private set; } =
            string.Empty;

        public void Reset(int resetGeneration)
        {
            generation = Math.Max(0, resetGeneration);
            lastAcceptedTick = -1L;
            acceptedKeys.Clear();
            acceptedKeyOrder.Clear();
            LastRejectReason = string.Empty;
        }

        public bool TryAccept(
            BattleSandboxAcceptedItemPresentationEvent accepted,
            int currentResetGeneration)
        {
            if (accepted == null
                || !accepted.IsAcceptedCorrelation)
            {
                return Reject("CORRELATION_INVALID");
            }
            if (currentResetGeneration <= 0
                || accepted.resetGeneration
                    != currentResetGeneration)
            {
                return Reject("GENERATION_MISMATCH");
            }
            if (generation != currentResetGeneration)
            {
                Reset(currentResetGeneration);
            }
            if (accepted.battleTick < lastAcceptedTick)
            {
                return Reject("STALE_BATTLE_TICK");
            }

            string key = BuildKey(
                accepted.resetGeneration,
                accepted.battleTick,
                accepted.pulseEventId);
            if (!acceptedKeys.Add(key))
            {
                return Reject("DUPLICATE_EVENT");
            }

            acceptedKeyOrder.Enqueue(key);
            while (acceptedKeyOrder.Count > RememberedKeyCap)
            {
                acceptedKeys.Remove(acceptedKeyOrder.Dequeue());
            }
            lastAcceptedTick = accepted.battleTick;
            AcceptedCount++;
            LastRejectReason = string.Empty;
            return true;
        }

        public static string BuildKey(
            int resetGeneration,
            long battleTick,
            string pulseEventId)
        {
            return resetGeneration
                + ":"
                + battleTick
                + ":"
                + (pulseEventId ?? string.Empty).Trim();
        }

        private bool Reject(string reason)
        {
            RejectedCount++;
            LastRejectReason = reason;
            return false;
        }
    }
}
