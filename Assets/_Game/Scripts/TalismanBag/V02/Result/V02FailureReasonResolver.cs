using TalismanBag.V02.Feedback;
using TalismanBag.V02.Run;
using UnityEngine;

namespace TalismanBag.V02.Result
{
    public sealed class V02FailureReasonResolver : MonoBehaviour
    {
        public V02FailureReasonResult ResolveFailureReason(V02FailureTracker tracker, V02RunFlowController runFlow)
        {
            if (tracker == null)
            {
                return LowDefense();
            }

            int round = tracker.roundLostIndex > 0 ? tracker.roundLostIndex : runFlow != null ? runFlow.CurrentRoundNumber : 0;

            if (tracker.unpoweredTriggerBlockedCount >= 5)
            {
                return new V02FailureReasonResult(
                    V02FailureReason.UnpoweredFormation,
                    "\u5927\u91cf\u7b26\u7b93\u672a\u4f9b\u80fd",
                    "\u591a\u4e2a\u7b26\u7b93\u6ca1\u6709\u88ab\u9635\u773c\u6216\u805a\u7075\u77f3\u4f9b\u80fd\uff0c\u6218\u6597\u4e2d\u6ca1\u6709\u89e6\u53d1\u3002",
                    "\u628a\u6838\u5fc3\u7b26\u7b93\u653e\u5230\u9635\u773c\u6216\u805a\u7075\u77f3\u4f9b\u80fd\u8303\u56f4\u5185\u3002");
            }

            if ((round == 2 || round == 7 || tracker.bossShieldPhaseDuration > 8) && tracker.shieldNotBrokenCount >= 3)
            {
                return new V02FailureReasonResult(
                    V02FailureReason.LackShieldBreak,
                    "\u7f3a\u5c11\u7834\u76fe",
                    "\u654c\u4eba\u7684\u62a4\u76fe\u6ca1\u6709\u88ab\u53ca\u65f6\u51fb\u7834\uff0c\u4f60\u7684\u8f93\u51fa\u88ab\u62a4\u76fe\u5438\u6536\u4e86\u3002",
                    "\u5e26\u4e0a\u96f7\u7b26\u6216\u5251\u4e38\uff0c\u5e76\u786e\u4fdd\u5b83\u4eec\u88ab\u4f9b\u80fd\u3002");
            }

            if (tracker.poisonDamageTaken + tracker.burnDamageTaken >= 18)
            {
                return new V02FailureReasonResult(
                    V02FailureReason.LackCleanse,
                    "\u7f3a\u5c11\u51c0\u5316",
                    "\u4f60\u88ab\u6bd2\u548c\u71c3\u70e7\u6301\u7eed\u6d88\u8017\uff0c\u4f46\u6ca1\u6709\u53ca\u65f6\u51c0\u5316\u3002",
                    "\u9009\u62e9\u51c0\u5316\u7b26\uff0c\u6216\u63d0\u9ad8\u62a4\u8eab\u7b26\u9632\u5fa1\u3002");
            }

            if (tracker.stealEnergyHitCount >= 2 && tracker.stealEnergyCounteredCount == 0)
            {
                return new V02FailureReasonResult(
                    V02FailureReason.FormationEnergyBroken,
                    "\u4f9b\u80fd\u7ed3\u6784\u88ab\u7834\u574f",
                    "\u5077\u7075\u654c\u4eba\u7834\u574f\u4e86\u4f60\u7684\u9635\u773c\u6216\u805a\u7075\u77f3\u4f9b\u80fd\uff0c\u6838\u5fc3\u7b26\u7b93\u77ed\u6682\u5931\u6548\u3002",
                    "\u4f7f\u7528\u9547\u9b42\u7b26\u53cd\u5236\u5077\u7075\uff0c\u6216\u589e\u52a0\u805a\u7075\u77f3\u5f62\u6210\u5907\u7528\u4f9b\u80fd\u3002");
            }

            if (tracker.sealHitCount >= 3 && tracker.sealCleansedCount == 0)
            {
                return new V02FailureReasonResult(
                    V02FailureReason.CoreLineSealed,
                    "\u6838\u5fc3\u7b26\u88ab\u5c01",
                    "\u4f60\u7684\u6838\u5fc3\u7b26\u7b93\u88ab\u5c01\u5370\u540e\u6ca1\u6709\u53ca\u65f6\u89e3\u5c01\uff0c\u8f93\u51fa\u5faa\u73af\u4e2d\u65ad\u3002",
                    "\u4e0d\u8981\u628a\u6240\u6709\u8f93\u51fa\u7b26\u653e\u5728\u540c\u4e00\u884c\u6216\u540c\u4e00\u5217\uff0c\u5e76\u4f7f\u7528\u51c0\u5316\u7b26\u89e3\u5c01\u3002");
            }

            if ((round == 3 || round == 7) && tracker.bossSummonDamageTaken >= 18)
            {
                return new V02FailureReasonResult(
                    V02FailureReason.LackGroupClear,
                    "\u7f3a\u5c11\u6e05\u7fa4",
                    "\u4f60\u7f3a\u5c11\u6e05\u7fa4\u624b\u6bb5\uff0c\u88ab\u5c0f\u5996\u7fa4\u6301\u7eed\u538b\u5236\u3002",
                    "\u9009\u62e9\u8fde\u9501\u96f7\u7b26\uff0c\u6216\u5f3a\u5316\u706b\u7b26\u71c3\u70e7\u3002");
            }

            if (round == 7)
            {
                return new V02FailureReasonResult(
                    V02FailureReason.BossMixedPressure,
                    "Boss\u7efc\u5408\u538b\u529b\u4e0d\u8db3",
                    "\u4f60\u7684\u9635\u6cd5\u7f3a\u5c11\u5b8c\u6574\u6027\uff0c\u65e0\u6cd5\u540c\u65f6\u5e94\u5bf9\u62a4\u76fe\u3001\u53ec\u5524\u548c\u5c01\u9635\u773c\u3002",
                    "\u4fdd\u8bc1\u81f3\u5c11\u62e5\u6709\u7834\u76fe\u3001\u6e05\u7fa4\u3001\u51c0\u5316\u6216\u5907\u7528\u4f9b\u80fd\u4e2d\u7684\u4e24\u5230\u4e09\u79cd\u80fd\u529b\u3002");
            }

            return LowDefense();
        }

        private static V02FailureReasonResult LowDefense()
        {
            return new V02FailureReasonResult(
                V02FailureReason.LowDefense,
                "\u9632\u5fa1\u6216\u56de\u590d\u4e0d\u8db3",
                "\u4f60\u7684\u9635\u6cd5\u5728\u627f\u538b\u65f6\u6ca1\u6709\u8db3\u591f\u62a4\u76fe\u6216\u56de\u590d\u3002",
                "\u5c1d\u8bd5\u4fdd\u7559\u62a4\u8eab\u7b26\u3001\u51c0\u5316\u7b26\u6216\u66f4\u7a33\u5b9a\u7684\u4f9b\u80fd\u4f4d\u7f6e\u3002");
        }
    }
}
