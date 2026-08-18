using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.BoneAspect.ShougunuPhase1
{
    public static class ShougunuPhase1ActionPatternCatalog
    {
        public const string BasicAttack = "shougunu.phase1.basic_attack";
        public const string Skill1ShellRepair = "shougunu.phase1.skill1.shell_repair";
        public const string Skill2RopeHeavyStrike = "shougunu.phase1.skill2.rope_heavy_strike";
        public const string Skill3GroundSealBurst = "shougunu.phase1.skill3.ground_seal_burst";

        private static readonly ShougunuPhase1ActionPatternSnapshot[] Patterns =
        {
            new ShougunuPhase1ActionPatternSnapshot(
                BasicAttack,
                "battle.effect_request.direct_player_damage",
                ShougunuPhase1DueKind.BasicDebt,
                0L, 400L, 300L, 700L, 600L,
                ShougunuPhase1InterruptPolicy.ReactiveCueFirst,
                ShougunuPhase1CueKind.BasicAttack),
            new ShougunuPhase1ActionPatternSnapshot(
                Skill1ShellRepair,
                "enemy.effect_request.repair_current_shell",
                ShougunuPhase1DueKind.HpThreshold,
                0L, 800L, 400L, 1200L, 800L,
                ShougunuPhase1InterruptPolicy.ReactiveCueFirst,
                ShougunuPhase1CueKind.Skill1ShellRepair),
            new ShougunuPhase1ActionPatternSnapshot(
                Skill2RopeHeavyStrike,
                "battle.effect_request.rope_heavy_strike",
                ShougunuPhase1DueKind.HpThreshold,
                0L, 1200L, 500L, 1700L, 900L,
                ShougunuPhase1InterruptPolicy.ReactiveCueFirst,
                ShougunuPhase1CueKind.Skill2RopeHeavyStrike),
            new ShougunuPhase1ActionPatternSnapshot(
                Skill3GroundSealBurst,
                "battle.effect_request.ground_seal_area_burst",
                ShougunuPhase1DueKind.HpThreshold,
                0L, 1800L, 700L, 2500L, 1200L,
                ShougunuPhase1InterruptPolicy.ReactiveCueFirst,
                ShougunuPhase1CueKind.Skill3GroundSealBurst)
        };

        private static readonly ThresholdDefinition[] Thresholds =
        {
            new ThresholdDefinition("S1-A", Skill1ShellRepair, 9000, 0),
            new ThresholdDefinition("S2-A", Skill2RopeHeavyStrike, 7500, 1),
            new ThresholdDefinition("S3-A", Skill3GroundSealBurst, 6000, 2),
            new ThresholdDefinition("S1-B", Skill1ShellRepair, 4500, 3),
            new ThresholdDefinition("S2-B", Skill2RopeHeavyStrike, 3000, 4),
            new ThresholdDefinition("S3-B", Skill3GroundSealBurst, 1500, 5)
        };

        public static IReadOnlyList<ShougunuPhase1ActionPatternSnapshot> GetPatterns()
        {
            return ShougunuPhase1Collection.Copy(Patterns);
        }

        public static string CanonicalSignature
        {
            get { return ComposeCanonicalSignature(false); }
        }

        public static string ComposeCanonicalSignature(bool reverseInputOrder)
        {
            IEnumerable<ShougunuPhase1ActionPatternSnapshot> patternInput =
                reverseInputOrder ? Patterns.Reverse() : Patterns;
            IEnumerable<ThresholdDefinition> thresholdInput =
                reverseInputOrder ? Thresholds.Reverse() : Thresholds;
            StringBuilder builder = new StringBuilder();
            foreach (ShougunuPhase1ActionPatternSnapshot pattern
                in patternInput.OrderBy(item => item.ActionPatternId, StringComparer.Ordinal))
            {
                builder.Append(pattern.ActionPatternId).Append('|')
                    .Append(pattern.EffectRequestId).Append('|')
                    .Append(((int)pattern.DueKind).ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(pattern.PreCastTicks.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(pattern.TelegraphTicks.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(pattern.CastTicks.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(pattern.ResolveOffsetTicks.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(pattern.RecoverTicks.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(((int)pattern.InterruptPolicy).ToString(CultureInfo.InvariantCulture))
                    .Append('|')
                    .Append(((int)pattern.CueKind).ToString(CultureInfo.InvariantCulture))
                    .Append('\n');
            }
            foreach (ThresholdDefinition threshold in thresholdInput.OrderBy(item => item.Order))
            {
                builder.Append(threshold.ThresholdId).Append('|')
                    .Append(threshold.ActionPatternId).Append('|')
                    .Append(threshold.BasisPoints.ToString(CultureInfo.InvariantCulture))
                    .Append('|').Append(threshold.Order.ToString(CultureInfo.InvariantCulture))
                    .Append('\n');
            }
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(builder.ToString()));
                StringBuilder hash = new StringBuilder(71).Append("sha256:");
                foreach (byte value in bytes)
                {
                    hash.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }
                return hash.ToString();
            }
        }

        public static ShougunuPhase1ActionPatternSnapshot GetPattern(string actionPatternId)
        {
            return Patterns.SingleOrDefault(
                item => string.Equals(item.ActionPatternId, actionPatternId, StringComparison.Ordinal));
        }

        internal static IReadOnlyList<ThresholdDefinition> GetThresholdsDescending()
        {
            return ShougunuPhase1Collection.Copy(Thresholds.OrderBy(item => item.Order));
        }

        internal sealed class ThresholdDefinition
        {
            public ThresholdDefinition(
                string thresholdId,
                string actionPatternId,
                int basisPoints,
                int order)
            {
                ThresholdId = thresholdId ?? string.Empty;
                ActionPatternId = actionPatternId ?? string.Empty;
                BasisPoints = basisPoints;
                Order = order;
            }

            public string ThresholdId { get; }
            public string ActionPatternId { get; }
            public int BasisPoints { get; }
            public int Order { get; }
        }
    }
}
