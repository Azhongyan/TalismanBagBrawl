using UnityEngine;
using TalismanBag.V02.Formation;

namespace TalismanBag.Items
{
    [System.Serializable]
    public sealed class TalismanItemRuntime
    {
        public string runtimeId;
        public TalismanItemDefinition definition;
        public Vector2Int gridPosition;
        public int level = 1;
        public float cooldownTimer;
        public int triggerCount;
        public bool isPlaced;
        public bool isSealed;
        public float sealRemaining;
        public bool isTemporarilyDisabled;
        public float temporaryDisabledRemaining;
        public FormationPowerState powerState = FormationPowerState.Powered;

        public TalismanItemRuntime(TalismanItemDefinition definition, int level = 1)
        {
            runtimeId = System.Guid.NewGuid().ToString("N");
            this.definition = definition;
            this.level = Mathf.Max(1, level);
            gridPosition = new Vector2Int(-1, -1);
            cooldownTimer = definition != null ? definition.baseCooldown : 0f;
        }

        public void ResetForBattle(float cooldown)
        {
            cooldownTimer = cooldown;
            triggerCount = 0;
            isSealed = false;
            sealRemaining = 0f;
            isTemporarilyDisabled = false;
            temporaryDisabledRemaining = 0f;
        }
    }
}
