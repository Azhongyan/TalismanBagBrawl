using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.CampaignBalance.C1Stage3To5Playtest
{
    public static class C1Stage3To5PlaytestSimulationContract
    {
        public const string SchemaId =
            "C1Stage3To5PlaytestEnemyEncounterSimulation.v1";
        public const string Accepted = "ACCEPTED";
        public const string Supported = "SUPPORTED";
        public const string PercentileP50 = "P50";
        public const string PercentileP90 = "P90";
        public const string SameTickOrder = "PLAYER_THEN_ENEMY";
        public const int PlayerStartingHp = 100;
        public const int PlayerPulseIntervalMilliseconds = 2000;
        public const int BasisPoints = 10000;
        public const int SensitivityLowBasisPoints = 8000;
        public const int SensitivityBaselineBasisPoints = 10000;
        public const int SensitivityHighBasisPoints = 12000;

        public const string MissingProfile = "MISSING_PROFILE";
        public const string UnknownProfile = "UNKNOWN_OR_CONFLICTED_PROFILE";
        public const string MissingItemEvidence = "MISSING_ITEM_EVIDENCE";
        public const string UnsupportedItemEvidence =
            "UNSUPPORTED_ITEM_EVIDENCE_REJECTED";
        public const string UnknownItemUpperBound =
            "UNKNOWN_ITEM_UPPER_BOUND_REJECTED";
        public const string StaleItemCanonical =
            "STALE_ITEM_CANONICAL_REJECTED";
        public const string NonPositiveItemPulse =
            "NON_POSITIVE_ITEM_PULSE_REJECTED";
        public const string InvalidOptions = "INVALID_OPTIONS";
    }

    public sealed class C1Stage3To5PlaytestItemPulseEvidence
    {
        public C1Stage3To5PlaytestItemPulseEvidence(
            int p50DirectDamage,
            int p90DirectDamage,
            string supportStatus,
            bool hasUnknownUpperBound,
            string observedFactsCanonicalSignature,
            string expectedFactsCanonicalSignature,
            string observedEnvelopeCanonicalSignature,
            string expectedEnvelopeCanonicalSignature)
        {
            P50DirectDamage = p50DirectDamage;
            P90DirectDamage = p90DirectDamage;
            SupportStatus = supportStatus ?? string.Empty;
            HasUnknownUpperBound = hasUnknownUpperBound;
            ObservedFactsCanonicalSignature =
                observedFactsCanonicalSignature ?? string.Empty;
            ExpectedFactsCanonicalSignature =
                expectedFactsCanonicalSignature ?? string.Empty;
            ObservedEnvelopeCanonicalSignature =
                observedEnvelopeCanonicalSignature ?? string.Empty;
            ExpectedEnvelopeCanonicalSignature =
                expectedEnvelopeCanonicalSignature ?? string.Empty;
            CanonicalSignature = SimulationCanonical.Hash(string.Join("|", new[]
            {
                P50DirectDamage.ToString(CultureInfo.InvariantCulture),
                P90DirectDamage.ToString(CultureInfo.InvariantCulture),
                SupportStatus,
                HasUnknownUpperBound ? "true" : "false",
                ObservedFactsCanonicalSignature,
                ExpectedFactsCanonicalSignature,
                ObservedEnvelopeCanonicalSignature,
                ExpectedEnvelopeCanonicalSignature
            }));
        }

        public int P50DirectDamage { get; private set; }
        public int P90DirectDamage { get; private set; }
        public string SupportStatus { get; private set; }
        public bool HasUnknownUpperBound { get; private set; }
        public string ObservedFactsCanonicalSignature { get; private set; }
        public string ExpectedFactsCanonicalSignature { get; private set; }
        public string ObservedEnvelopeCanonicalSignature { get; private set; }
        public string ExpectedEnvelopeCanonicalSignature { get; private set; }
        public string CanonicalSignature { get; private set; }
    }

    public sealed class C1Stage3To5PlaytestSimulationOptions
    {
        public C1Stage3To5PlaytestSimulationOptions(
            string percentile,
            int itemPulseBasisPoints,
            int encounterHpBasisPoints,
            int basicDamageBasisPoints)
        {
            Percentile = percentile ?? string.Empty;
            ItemPulseBasisPoints = itemPulseBasisPoints;
            EncounterHpBasisPoints = encounterHpBasisPoints;
            BasicDamageBasisPoints = basicDamageBasisPoints;
            CanonicalSignature = SimulationCanonical.Hash(string.Join("|", new[]
            {
                Percentile,
                ItemPulseBasisPoints.ToString(CultureInfo.InvariantCulture),
                EncounterHpBasisPoints.ToString(CultureInfo.InvariantCulture),
                BasicDamageBasisPoints.ToString(CultureInfo.InvariantCulture)
            }));
        }

        public string Percentile { get; private set; }
        public int ItemPulseBasisPoints { get; private set; }
        public int EncounterHpBasisPoints { get; private set; }
        public int BasicDamageBasisPoints { get; private set; }
        public string CanonicalSignature { get; private set; }

        public static C1Stage3To5PlaytestSimulationOptions Baseline(string percentile)
        {
            return new C1Stage3To5PlaytestSimulationOptions(
                percentile,
                C1Stage3To5PlaytestSimulationContract.
                    SensitivityBaselineBasisPoints,
                C1Stage3To5PlaytestSimulationContract.
                    SensitivityBaselineBasisPoints,
                C1Stage3To5PlaytestSimulationContract.
                    SensitivityBaselineBasisPoints);
        }
    }

    public sealed class C1Stage3To5PlaytestSimulationEvent
    {
        public C1Stage3To5PlaytestSimulationEvent(
            int timeMilliseconds,
            int sequence,
            string kind,
            int signedPlayerHpDelta,
            int signedEncounterHpDelta,
            string detail)
        {
            TimeMilliseconds = timeMilliseconds;
            Sequence = sequence;
            Kind = kind ?? string.Empty;
            SignedPlayerHpDelta = signedPlayerHpDelta;
            SignedEncounterHpDelta = signedEncounterHpDelta;
            Detail = detail ?? string.Empty;
            CanonicalSignature = SimulationCanonical.Hash(string.Join("|", new[]
            {
                TimeMilliseconds.ToString(CultureInfo.InvariantCulture),
                Sequence.ToString(CultureInfo.InvariantCulture),
                Kind,
                SignedPlayerHpDelta.ToString(CultureInfo.InvariantCulture),
                SignedEncounterHpDelta.ToString(CultureInfo.InvariantCulture),
                Detail
            }));
        }

        public int TimeMilliseconds { get; private set; }
        public int Sequence { get; private set; }
        public string Kind { get; private set; }
        public int SignedPlayerHpDelta { get; private set; }
        public int SignedEncounterHpDelta { get; private set; }
        public string Detail { get; private set; }
        public string CanonicalSignature { get; private set; }
    }

    public sealed class C1Stage3To5PlaytestSimulationSnapshot
    {
        public C1Stage3To5PlaytestSimulationSnapshot(
            string stageId,
            string encounterVariantId,
            string percentile,
            int playerPulseDamage,
            int encounterTotalHp,
            int enemyBasicDamage,
            int timeToKillMilliseconds,
            int playerPulseCount,
            int enemyBasicCount,
            int boneSwapReturnCount,
            int finalPlayerHp,
            IEnumerable<C1Stage3To5PlaytestSimulationEvent> events)
        {
            StageId = stageId ?? string.Empty;
            EncounterVariantId = encounterVariantId ?? string.Empty;
            Percentile = percentile ?? string.Empty;
            PlayerPulseDamage = playerPulseDamage;
            EncounterTotalHp = encounterTotalHp;
            EnemyBasicDamage = enemyBasicDamage;
            TimeToKillMilliseconds = timeToKillMilliseconds;
            PlayerPulseCount = playerPulseCount;
            EnemyBasicCount = enemyBasicCount;
            BoneSwapReturnCount = boneSwapReturnCount;
            FinalPlayerHp = finalPlayerHp;
            Events = new ReadOnlyCollection<C1Stage3To5PlaytestSimulationEvent>(
                (events ?? Enumerable.Empty<C1Stage3To5PlaytestSimulationEvent>()).
                    ToList());
            CanonicalSignature = SimulationCanonical.Hash(string.Join("|", new[]
            {
                StageId,
                EncounterVariantId,
                Percentile,
                PlayerPulseDamage.ToString(CultureInfo.InvariantCulture),
                EncounterTotalHp.ToString(CultureInfo.InvariantCulture),
                EnemyBasicDamage.ToString(CultureInfo.InvariantCulture),
                TimeToKillMilliseconds.ToString(CultureInfo.InvariantCulture),
                PlayerPulseCount.ToString(CultureInfo.InvariantCulture),
                EnemyBasicCount.ToString(CultureInfo.InvariantCulture),
                BoneSwapReturnCount.ToString(CultureInfo.InvariantCulture),
                FinalPlayerHp.ToString(CultureInfo.InvariantCulture),
                string.Join(",", Events.Select(item => item.CanonicalSignature))
            }));
        }

        public string StageId { get; private set; }
        public string EncounterVariantId { get; private set; }
        public string Percentile { get; private set; }
        public int PlayerPulseDamage { get; private set; }
        public int EncounterTotalHp { get; private set; }
        public int EnemyBasicDamage { get; private set; }
        public int TimeToKillMilliseconds { get; private set; }
        public int PlayerPulseCount { get; private set; }
        public int EnemyBasicCount { get; private set; }
        public int BoneSwapReturnCount { get; private set; }
        public int FinalPlayerHp { get; private set; }
        public IReadOnlyList<C1Stage3To5PlaytestSimulationEvent> Events
        {
            get;
            private set;
        }
        public string CanonicalSignature { get; private set; }
    }

    public sealed class C1Stage3To5PlaytestSimulationResult
    {
        public C1Stage3To5PlaytestSimulationResult(
            string status,
            string diagnostic,
            C1Stage3To5PlaytestSimulationSnapshot snapshot)
        {
            Status = status ?? string.Empty;
            Diagnostic = diagnostic ?? string.Empty;
            Snapshot = snapshot;
            CanonicalSignature = SimulationCanonical.Hash(string.Join("|", new[]
            {
                Status,
                Diagnostic,
                Snapshot == null ? "NULL" : Snapshot.CanonicalSignature
            }));
        }

        public string Status { get; private set; }
        public string Diagnostic { get; private set; }
        public C1Stage3To5PlaytestSimulationSnapshot Snapshot { get; private set; }
        public string CanonicalSignature { get; private set; }
        public bool IsAccepted
        {
            get
            {
                return string.Equals(
                    Status,
                    C1Stage3To5PlaytestSimulationContract.Accepted,
                    StringComparison.Ordinal)
                    && Snapshot != null;
            }
        }
    }

    public static class C1Stage3To5PlaytestEnemyEncounterSimulation
    {
        public static C1Stage3To5PlaytestSimulationResult Simulate(
            C1Stage3To5PlaytestEncounterProfile profile,
            C1Stage3To5PlaytestItemPulseEvidence itemEvidence,
            C1Stage3To5PlaytestSimulationOptions options)
        {
            string diagnostic = Validate(profile, itemEvidence, options);
            if (!string.IsNullOrEmpty(diagnostic))
            {
                return Rejected(diagnostic);
            }

            int sourcePulse = string.Equals(
                options.Percentile,
                C1Stage3To5PlaytestSimulationContract.PercentileP50,
                StringComparison.Ordinal)
                    ? itemEvidence.P50DirectDamage
                    : itemEvidence.P90DirectDamage;
            int pulseDamage = Scale(sourcePulse, options.ItemPulseBasisPoints);
            int encounterHp = Scale(
                profile.TotalMaxHp,
                options.EncounterHpBasisPoints);
            int basicDamage = Scale(
                DecimalToInt(profile.AttackWave.DamagePerApplication),
                options.BasicDamageBasisPoints);
            if (pulseDamage <= 0)
            {
                return Rejected(
                    C1Stage3To5PlaytestSimulationContract.NonPositiveItemPulse);
            }

            int playerPulseCount = (encounterHp + pulseDamage - 1) / pulseDamage;
            int timeToKill = playerPulseCount *
                C1Stage3To5PlaytestSimulationContract.
                    PlayerPulseIntervalMilliseconds;
            int basicInterval = DecimalSecondsToMilliseconds(
                profile.AttackWave.IntervalSeconds);
            int firstBasic = DecimalSecondsToMilliseconds(
                profile.AttackWave.FirstResolveSeconds);
            int basicCount = timeToKill <= firstBasic
                ? 0
                : ((timeToKill - 1 - firstBasic) / basicInterval) + 1;
            bool hasBoneSwap = profile.Actors.Any(actor =>
                actor != null
                && string.Equals(
                    actor.ContentId,
                    C1EnemyRuntime.C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                    StringComparison.Ordinal));
            List<int> boneReturnTimes = hasBoneSwap
                ? BoneReturnTimes(timeToKill)
                : new List<int>();
            int boneReturnDamage = Math.Min(
                (pulseDamage *
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        BoneSwapRatioBasisPoints)
                    / C1Stage3To5PlaytestSimulationContract.BasisPoints,
                C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapCapDamage);
            int finalPlayerHp =
                C1Stage3To5PlaytestSimulationContract.PlayerStartingHp
                - (basicCount * basicDamage)
                - (boneReturnTimes.Count * boneReturnDamage);
            List<C1Stage3To5PlaytestSimulationEvent> events = BuildEvents(
                timeToKill,
                playerPulseCount,
                pulseDamage,
                firstBasic,
                basicInterval,
                basicCount,
                basicDamage,
                boneReturnTimes,
                boneReturnDamage,
                hasBoneSwap);
            var snapshot = new C1Stage3To5PlaytestSimulationSnapshot(
                profile.StageId,
                profile.EncounterVariantId,
                options.Percentile,
                pulseDamage,
                encounterHp,
                basicDamage,
                timeToKill,
                playerPulseCount,
                basicCount,
                boneReturnTimes.Count,
                finalPlayerHp,
                events);
            return new C1Stage3To5PlaytestSimulationResult(
                C1Stage3To5PlaytestSimulationContract.Accepted,
                string.Empty,
                snapshot);
        }

        private static string Validate(
            C1Stage3To5PlaytestEncounterProfile profile,
            C1Stage3To5PlaytestItemPulseEvidence evidence,
            C1Stage3To5PlaytestSimulationOptions options)
        {
            if (profile == null)
            {
                return C1Stage3To5PlaytestSimulationContract.MissingProfile;
            }

            C1Stage3To5PlaytestEncounterProfile canonical =
                C1Stage3To5PlaytestEnemyEncounterCatalog.FindByStageId(
                    profile.StageId);
            if (canonical == null
                || !string.Equals(
                    canonical.CanonicalSignature,
                    profile.CanonicalSignature,
                    StringComparison.Ordinal))
            {
                return C1Stage3To5PlaytestSimulationContract.UnknownProfile;
            }

            if (evidence == null)
            {
                return C1Stage3To5PlaytestSimulationContract.MissingItemEvidence;
            }

            if (!string.Equals(
                evidence.SupportStatus,
                C1Stage3To5PlaytestSimulationContract.Supported,
                StringComparison.Ordinal))
            {
                return C1Stage3To5PlaytestSimulationContract.
                    UnsupportedItemEvidence;
            }

            if (evidence.HasUnknownUpperBound)
            {
                return C1Stage3To5PlaytestSimulationContract.UnknownItemUpperBound;
            }

            if (string.IsNullOrEmpty(evidence.ObservedFactsCanonicalSignature)
                || string.IsNullOrEmpty(evidence.ExpectedFactsCanonicalSignature)
                || string.IsNullOrEmpty(evidence.ObservedEnvelopeCanonicalSignature)
                || string.IsNullOrEmpty(evidence.ExpectedEnvelopeCanonicalSignature)
                || !string.Equals(
                    evidence.ObservedFactsCanonicalSignature,
                    evidence.ExpectedFactsCanonicalSignature,
                    StringComparison.Ordinal)
                || !string.Equals(
                    evidence.ObservedEnvelopeCanonicalSignature,
                    evidence.ExpectedEnvelopeCanonicalSignature,
                    StringComparison.Ordinal))
            {
                return C1Stage3To5PlaytestSimulationContract.StaleItemCanonical;
            }

            if (evidence.P50DirectDamage <= 0 || evidence.P90DirectDamage <= 0)
            {
                return C1Stage3To5PlaytestSimulationContract.NonPositiveItemPulse;
            }

            if (options == null
                || (!string.Equals(
                        options.Percentile,
                        C1Stage3To5PlaytestSimulationContract.PercentileP50,
                        StringComparison.Ordinal)
                    && !string.Equals(
                        options.Percentile,
                        C1Stage3To5PlaytestSimulationContract.PercentileP90,
                        StringComparison.Ordinal))
                || !ValidSensitivity(options.ItemPulseBasisPoints)
                || !ValidSensitivity(options.EncounterHpBasisPoints)
                || !ValidSensitivity(options.BasicDamageBasisPoints))
            {
                return C1Stage3To5PlaytestSimulationContract.InvalidOptions;
            }

            return string.Empty;
        }

        private static List<int> BoneReturnTimes(int timeToKill)
        {
            var times = new List<int>();
            int returnTime =
                C1Stage3To5PlaytestSimulationContract.
                    PlayerPulseIntervalMilliseconds
                + C1Stage3To5PlaytestEnemyEncounterContract.
                    BoneSwapTelegraphMilliseconds;
            int cycle =
                C1Stage3To5PlaytestEnemyEncounterContract.
                    BoneSwapRecoverMilliseconds
                + C1Stage3To5PlaytestEnemyEncounterContract.
                    BoneSwapTelegraphMilliseconds;
            while (returnTime < timeToKill)
            {
                times.Add(returnTime);
                returnTime += cycle;
            }

            return times;
        }

        private static List<C1Stage3To5PlaytestSimulationEvent> BuildEvents(
            int timeToKill,
            int playerPulseCount,
            int pulseDamage,
            int firstBasic,
            int basicInterval,
            int basicCount,
            int basicDamage,
            IEnumerable<int> boneReturnTimes,
            int boneReturnDamage,
            bool hasBoneSwap)
        {
            var pending = new List<PendingEvent>();
            for (int index = 1; index <= playerPulseCount; index++)
            {
                int time = index *
                    C1Stage3To5PlaytestSimulationContract.
                        PlayerPulseIntervalMilliseconds;
                pending.Add(new PendingEvent(
                    time,
                    0,
                    "PLAYER_DIRECT_DAMAGE_PULSE",
                    0,
                    -pulseDamage,
                    "player_resolved_single_target_direct_damage"));
            }

            for (int index = 0; index < basicCount; index++)
            {
                pending.Add(new PendingEvent(
                    firstBasic + (index * basicInterval),
                    3,
                    "ENCOUNTER_BASIC_DAMAGE",
                    -basicDamage,
                    0,
                    "encounter_wave_single_application"));
            }

            if (hasBoneSwap)
            {
                int telegraphTime =
                    C1Stage3To5PlaytestSimulationContract.
                        PlayerPulseIntervalMilliseconds;
                int cycle =
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        BoneSwapRecoverMilliseconds
                    + C1Stage3To5PlaytestEnemyEncounterContract.
                        BoneSwapTelegraphMilliseconds;
                while (telegraphTime < timeToKill)
                {
                    pending.Add(new PendingEvent(
                        telegraphTime,
                        1,
                        "BONE_SWAP_TELEGRAPH",
                        0,
                        0,
                        C1Stage3To5PlaytestEnemyEncounterContract.
                            BoneSwapPendingPolicy));
                    telegraphTime += cycle;
                }
            }

            foreach (int time in boneReturnTimes)
            {
                pending.Add(new PendingEvent(
                    time,
                    2,
                    "BONE_SWAP_RETURN_REQUEST",
                    -boneReturnDamage,
                    0,
                    "neutral_bounded_capped_direct_damage_request"));
            }

            pending.Add(new PendingEvent(
                timeToKill,
                4,
                "ENCOUNTER_DEFEATED_PENDING_CANCELLED",
                0,
                0,
                "defeat_cancels_pending_work"));
            List<PendingEvent> ordered = pending.
                OrderBy(item => item.TimeMilliseconds).
                ThenBy(item => item.Priority).ToList();
            var events = new List<C1Stage3To5PlaytestSimulationEvent>();
            for (int index = 0; index < ordered.Count; index++)
            {
                PendingEvent item = ordered[index];
                events.Add(new C1Stage3To5PlaytestSimulationEvent(
                    item.TimeMilliseconds,
                    index + 1,
                    item.Kind,
                    item.SignedPlayerHpDelta,
                    item.SignedEncounterHpDelta,
                    item.Detail));
            }

            return events;
        }

        private static int Scale(int value, int basisPoints)
        {
            long scaled = ((long)value * basisPoints)
                + (C1Stage3To5PlaytestSimulationContract.BasisPoints / 2);
            return (int)(scaled /
                C1Stage3To5PlaytestSimulationContract.BasisPoints);
        }

        private static int DecimalSecondsToMilliseconds(decimal value)
        {
            return DecimalToInt(value * 1000m);
        }

        private static int DecimalToInt(decimal value)
        {
            return decimal.ToInt32(decimal.Round(
                value,
                0,
                MidpointRounding.AwayFromZero));
        }

        private static bool ValidSensitivity(int value)
        {
            return value == C1Stage3To5PlaytestSimulationContract.
                    SensitivityLowBasisPoints
                || value == C1Stage3To5PlaytestSimulationContract.
                    SensitivityBaselineBasisPoints
                || value == C1Stage3To5PlaytestSimulationContract.
                    SensitivityHighBasisPoints;
        }

        private static C1Stage3To5PlaytestSimulationResult Rejected(
            string diagnostic)
        {
            return new C1Stage3To5PlaytestSimulationResult(
                "REJECTED",
                diagnostic,
                null);
        }

        private sealed class PendingEvent
        {
            public PendingEvent(
                int timeMilliseconds,
                int priority,
                string kind,
                int signedPlayerHpDelta,
                int signedEncounterHpDelta,
                string detail)
            {
                TimeMilliseconds = timeMilliseconds;
                Priority = priority;
                Kind = kind;
                SignedPlayerHpDelta = signedPlayerHpDelta;
                SignedEncounterHpDelta = signedEncounterHpDelta;
                Detail = detail;
            }

            public int TimeMilliseconds;
            public int Priority;
            public string Kind;
            public int SignedPlayerHpDelta;
            public int SignedEncounterHpDelta;
            public string Detail;
        }
    }

    internal static class SimulationCanonical
    {
        public static string Hash(string value)
        {
            return C1Stage3To5PlaytestEnemyEncounterCanonical.Hash(value);
        }
    }
}
