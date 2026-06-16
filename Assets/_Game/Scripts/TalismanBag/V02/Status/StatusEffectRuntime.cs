using System;
using UnityEngine;

namespace TalismanBag.V02.Status
{
    [Serializable]
    public sealed class StatusEffectRuntime
    {
        public StatusEffectDefinition definition;
        public string sourceId;
        public string sourceType;
        public int stackCount;
        public float remainingTime;

        public bool IsExpired => definition != null && definition.hasDuration && remainingTime <= 0f;

        public StatusEffectRuntime(StatusEffectDefinition definition, string sourceId, string sourceType, int stackCount)
        {
            this.definition = definition;
            this.sourceId = sourceId;
            this.sourceType = sourceType;
            this.stackCount = Mathf.Max(1, stackCount);
            remainingTime = definition != null ? definition.defaultDuration : 0f;
        }

        public void Refresh(int stack, float durationOverride = -1f)
        {
            if (definition == null)
            {
                return;
            }

            stackCount = definition.stackable
                ? Mathf.Clamp(stackCount + Mathf.Max(1, stack), 1, Mathf.Max(1, definition.maxStacks))
                : Mathf.Max(1, stack);

            if (definition.hasDuration)
            {
                remainingTime = durationOverride >= 0f ? durationOverride : definition.defaultDuration;
            }
        }

        public void SetStack(int stack, float durationOverride = -1f)
        {
            if (definition == null)
            {
                return;
            }

            stackCount = definition.stackable
                ? Mathf.Clamp(stack, 1, Mathf.Max(1, definition.maxStacks))
                : Mathf.Max(1, stack);

            if (definition.hasDuration)
            {
                remainingTime = durationOverride >= 0f ? durationOverride : definition.defaultDuration;
            }
        }
    }
}
