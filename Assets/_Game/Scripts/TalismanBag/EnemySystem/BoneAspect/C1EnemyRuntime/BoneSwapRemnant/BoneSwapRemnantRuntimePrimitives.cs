using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant
{
    public enum BoneSwapRemnantRuntimeState
    {
        Idle = 0,
        Telegraphing = 1,
        Recovering = 2,
        Defeated = 3,
        Invalid = 4
    }

    public enum BoneSwapRemnantTuningDisposition
    {
        Undefined = 0,
        SYNTHETIC_FIXTURE_ONLY = 1,
        CAMPAIGN_RELEASED = 2
    }

    public enum BoneSwapRemnantPendingPolicy
    {
        Undefined = 0,
        LATEST_WINS_ONE_SLOT = 1
    }

    public enum BoneSwapResolvedDamageSourceKind
    {
        Undefined = 0,
        PLAYER_RESOLVED_SINGLE_TARGET_DIRECT_DAMAGE = 1
    }

    public enum BoneSwapRemnantTargetRequestKind
    {
        Undefined = 0,
        PLAYER_PRIMARY_TARGET = 1
    }

    public enum BoneSwapRemnantCueKind
    {
        Undefined = 0,
        CopyTelegraph = 1,
        CopyReturnRequested = 2,
        Reset = 3,
        Defeated = 4
    }

    public static class BoneSwapRemnantRuntimeContract
    {
        public const string SchemaId = "BoneSwapRemnantRuntimeOperator.v1";
        public const string NormalizedFactSchemaId =
            "BoneSwapResolvedDirectDamageFact.v1";
        public const string ContentId =
            C1EnemyRuntimeContract.BoneSwapRemnantContentId;
        public const string OperatorProfileId =
            "bone_aspect.runtime.c1.bone_swap_remnant.operator.v1";
        public const string ActionId =
            "c1.bone_swap_remnant.return_last_direct_pulse";
        public const string EffectRequestKey =
            "battle.effect_request.direct_player_damage";
        public const bool DevOnly = true;
        public const bool IsEnabled = false;
        public const bool EntersFormalFlow = false;
        public const bool RuntimeBoundToBattle = false;
    }

    internal static class BoneSwapRemnantMath
    {
        public static int ComputeReturnAmount(
            int resolvedDamage,
            int ratioBasisPoints,
            int capDamage)
        {
            if (resolvedDamage <= 0
                || ratioBasisPoints <= 0
                || ratioBasisPoints > 10000
                || capDamage <= 0)
            {
                return 0;
            }

            long scaled = ((long)resolvedDamage * ratioBasisPoints) / 10000L;
            long capped = Math.Min(scaled, (long)capDamage);
            return capped <= 0L ? 0 : (int)capped;
        }

        public static bool TryAddTicks(long left, long right, out long value)
        {
            value = 0L;
            if (left < 0L || right < 0L || left > long.MaxValue - right)
            {
                return false;
            }

            value = left + right;
            return true;
        }
    }

    internal static class BoneSwapRemnantCanonical
    {
        public static string Hash(params string[] fields)
        {
            return Hash((IEnumerable<string>)fields);
        }

        public static string Hash(IEnumerable<string> fields)
        {
            StringBuilder payload = new StringBuilder();
            foreach (string field in fields ?? Array.Empty<string>())
            {
                string value = field ?? string.Empty;
                payload.Append(value.Length.ToString(CultureInfo.InvariantCulture));
                payload.Append(':');
                payload.Append(value);
                payload.Append(';');
            }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(payload.ToString());
                return string.Concat(sha.ComputeHash(bytes).Select(value =>
                    value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        public static string Number(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string Flag(bool value)
        {
            return value ? "1" : "0";
        }

        public static bool IsStableId(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && string.Equals(value, value.Trim(), StringComparison.Ordinal);
        }

        public static string JoinSignatures<T>(
            IEnumerable<T> values,
            Func<T, string> selector,
            bool sort)
        {
            IEnumerable<string> signatures = (values ?? Array.Empty<T>())
                .Select(value => selector(value) ?? string.Empty);
            if (sort)
            {
                signatures = signatures.OrderBy(
                    value => value,
                    StringComparer.Ordinal);
            }
            return string.Join(",", signatures);
        }
    }
}
