using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Battle
{
    public sealed class BattleInteractionLock : MonoBehaviour
    {
        public const string DefaultLockedHint = "阵势已启，战后可整备";

        [SerializeField] private bool locked;
        [SerializeField] private string lockedHint = DefaultLockedHint;

        public bool IsLocked => locked;
        public string LockedHint => string.IsNullOrWhiteSpace(lockedHint) ? DefaultLockedHint : lockedHint;

        public void SetLocked(bool value)
        {
            locked = value;
        }

        public bool TryRequireUnlocked(out string message)
        {
            if (!locked)
            {
                message = string.Empty;
                return true;
            }

            message = LockedHint;
            return false;
        }
    }
}
