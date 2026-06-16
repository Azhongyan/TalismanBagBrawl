using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.V02.Status
{
    public sealed class StatusEffectController : MonoBehaviour
    {
        private readonly Dictionary<string, StatusEffectRuntime> statuses = new();
        private readonly List<StatusEffectRuntime> visibleBuffer = new();

        public event Action StatusesChanged;

        public void AddStatus(StatusEffectDefinition definition, string sourceId, string sourceType, int stack = 1)
        {
            AddStatus(definition, sourceId, sourceType, stack, -1f);
        }

        public void AddStatus(StatusEffectDefinition definition, string sourceId, string sourceType, int stack, float durationOverride)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.statusId))
            {
                return;
            }

            string id = definition.statusId;
            if (statuses.TryGetValue(id, out StatusEffectRuntime runtime))
            {
                runtime.sourceId = sourceId;
                runtime.sourceType = sourceType;
                runtime.Refresh(stack, durationOverride);
            }
            else
            {
                runtime = new StatusEffectRuntime(definition, sourceId, sourceType, stack);
                if (definition.hasDuration && durationOverride >= 0f)
                {
                    runtime.remainingTime = durationOverride;
                }

                statuses[id] = runtime;
            }

            NotifyChanged();
        }

        public void SetStatus(StatusEffectDefinition definition, string sourceId, string sourceType, int stack, float durationOverride = -1f)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.statusId))
            {
                return;
            }

            if (stack <= 0)
            {
                RemoveStatus(definition.statusId);
                return;
            }

            string id = definition.statusId;
            if (statuses.TryGetValue(id, out StatusEffectRuntime runtime))
            {
                runtime.sourceId = sourceId;
                runtime.sourceType = sourceType;
                runtime.SetStack(stack, durationOverride);
            }
            else
            {
                runtime = new StatusEffectRuntime(definition, sourceId, sourceType, stack);
                if (definition.hasDuration && durationOverride >= 0f)
                {
                    runtime.remainingTime = durationOverride;
                }

                statuses[id] = runtime;
            }

            NotifyChanged();
        }

        public void RemoveStatus(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return;
            }

            if (statuses.Remove(statusId))
            {
                NotifyChanged();
            }
        }

        public void RemoveStatusByType(StatusEffectType type)
        {
            bool changed = false;
            foreach (string id in statuses.Where(pair => pair.Value.definition != null && pair.Value.definition.statusType == type).Select(pair => pair.Key).ToArray())
            {
                statuses.Remove(id);
                changed = true;
            }

            if (changed)
            {
                NotifyChanged();
            }
        }

        public void ClearDispellableDebuffs()
        {
            bool changed = false;
            foreach (string id in statuses
                         .Where(pair => pair.Value.definition != null &&
                                        pair.Value.definition.isDispellable &&
                                        pair.Value.definition.polarity == StatusPolarity.Debuff)
                         .Select(pair => pair.Key)
                         .ToArray())
            {
                statuses.Remove(id);
                changed = true;
            }

            if (changed)
            {
                NotifyChanged();
            }
        }

        public void ClearAll()
        {
            if (statuses.Count == 0)
            {
                return;
            }

            statuses.Clear();
            NotifyChanged();
        }

        public bool HasStatus(string statusId)
        {
            return !string.IsNullOrWhiteSpace(statusId) && statuses.ContainsKey(statusId);
        }

        public bool HasStatusType(StatusEffectType type)
        {
            return statuses.Values.Any(status => status.definition != null && status.definition.statusType == type);
        }

        public int GetStackCount(string statusId)
        {
            return !string.IsNullOrWhiteSpace(statusId) && statuses.TryGetValue(statusId, out StatusEffectRuntime status)
                ? status.stackCount
                : 0;
        }

        public List<StatusEffectRuntime> GetVisibleStatuses()
        {
            visibleBuffer.Clear();
            visibleBuffer.AddRange(statuses.Values
                .Where(status => status.definition != null && status.definition.isVisible)
                .OrderBy(status => status.definition.displayPriority)
                .ThenByDescending(status => status.stackCount)
                .ThenBy(status => status.definition.displayName));
            return visibleBuffer;
        }

        public List<StatusEffectRuntime> GetAvatarStatuses(int maxCount)
        {
            List<StatusEffectRuntime> visible = GetVisibleStatuses();
            if (maxCount <= 0 || visible.Count <= maxCount)
            {
                return visible;
            }

            return visible.Take(maxCount).ToList();
        }

        private void Update()
        {
            TickStatuses(Time.deltaTime);
        }

        private void TickStatuses(float deltaTime)
        {
            if (deltaTime <= 0f || statuses.Count == 0)
            {
                return;
            }

            bool changed = false;
            foreach (StatusEffectRuntime runtime in statuses.Values)
            {
                if (runtime.definition == null || !runtime.definition.hasDuration)
                {
                    continue;
                }

                runtime.remainingTime -= deltaTime;
            }

            foreach (string id in statuses.Where(pair => pair.Value.IsExpired).Select(pair => pair.Key).ToArray())
            {
                statuses.Remove(id);
                changed = true;
            }

            if (changed)
            {
                NotifyChanged();
            }
        }

        private void NotifyChanged()
        {
            StatusesChanged?.Invoke();
        }
    }
}
