using TalismanBag.Contracts.Battle;
using UnityEngine;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleFeedbackReceiverLanes : MonoBehaviour
    {
        [SerializeField] private RectTransform incomingDamageLane;
        [SerializeField] private RectTransform benefitLane;
        [SerializeField] private RectTransform statusLane;

        public RectTransform IncomingDamageLane => incomingDamageLane;
        public RectTransform BenefitLane => benefitLane;
        public RectTransform StatusLane => statusLane;

        public RectTransform Resolve(string resultKind)
        {
            switch (resultKind)
            {
                case C1FormalRealtimeBattleFeedbackResultKinds.HpDamage:
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellDamage:
                case C1FormalRealtimeBattleFeedbackResultKinds.GuardDamage:
                    return incomingDamageLane;
                case C1FormalRealtimeBattleFeedbackResultKinds.Heal:
                case C1FormalRealtimeBattleFeedbackResultKinds.GuardGain:
                case C1FormalRealtimeBattleFeedbackResultKinds.ShellGain:
                case C1FormalRealtimeBattleFeedbackResultKinds.NianGain:
                case C1FormalRealtimeBattleFeedbackResultKinds.NianSpend:
                    return benefitLane;
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffApply:
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRefresh:
                case C1FormalRealtimeBattleFeedbackResultKinds.BuffRemove:
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffApply:
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffRefresh:
                case C1FormalRealtimeBattleFeedbackResultKinds.DebuffRemove:
                case C1FormalRealtimeBattleFeedbackResultKinds.Control:
                case C1FormalRealtimeBattleFeedbackResultKinds.Cleanse:
                    return statusLane;
                default:
                    return null;
            }
        }

        public bool ValidateAuthoredReferences()
        {
            return incomingDamageLane != null
                   && benefitLane != null
                   && statusLane != null
                   && incomingDamageLane != benefitLane
                   && incomingDamageLane != statusLane
                   && benefitLane != statusLane;
        }

#if UNITY_EDITOR
        public void AssignForEditor(
            RectTransform configuredIncomingDamageLane,
            RectTransform configuredBenefitLane,
            RectTransform configuredStatusLane)
        {
            incomingDamageLane = configuredIncomingDamageLane;
            benefitLane = configuredBenefitLane;
            statusLane = configuredStatusLane;
        }
#endif
    }
}
