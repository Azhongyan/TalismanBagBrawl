using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TalismanBag.Contracts.Battle
{
    public enum BattleSandboxExplicitDevEncounterKind
    {
        Normal = 0,
        Elite = 1
    }

    public enum BattleSandboxExplicitDevEnemyActionKind
    {
        BasicAttack = 0,
        Skill = 1
    }

    public enum BattleSandboxExplicitDevEncounterRejectReason
    {
        None = 0,
        RequestMissing = 1,
        SchemaMismatch = 2,
        DevOnlyRequired = 3,
        ProductContextRejected = 4,
        HostContextRejected = 5,
        HostPackageRejected = 6,
        FormalOrCampaignRejected = 7,
        PlayerOrApkRejected = 8,
        GenerationInvalid = 9,
        StartTokenInvalid = 10,
        EncounterProfileInvalid = 11,
        ProfileFingerprintInvalid = 12,
        EnemyIdentityInvalid = 13,
        PlayerSurvivabilityInvalid = 14,
        EnemyHpInvalid = 15,
        EnemyDamageInvalid = 16,
        EnemyTimingInvalid = 17,
        EnemyCadenceInvalid = 18,
        DuplicateMismatch = 19,
        StaleGenerationOrToken = 20,
        BattleModeInactive = 21,
        RuntimeDependencyMissing = 22,
        RuntimeStartRejected = 23
    }

    public sealed class BattleSandboxExplicitDevEnemyActionCue
    {
        public BattleSandboxExplicitDevEnemyActionCue(
            int sequence,
            BattleSandboxExplicitDevEnemyActionKind actionKind,
            int atMilliseconds)
        {
            Sequence = sequence;
            ActionKind = actionKind;
            AtMilliseconds = atMilliseconds;
        }

        public int Sequence { get; }
        public BattleSandboxExplicitDevEnemyActionKind ActionKind { get; }
        public int AtMilliseconds { get; }
    }

    public sealed class BattleSandboxExplicitDevEncounterRequest
    {
        public const string CurrentSchemaId =
            "BattleSandboxExplicitDevEncounterRequest.v1";
        public const string RequiredProductContext =
            "PLAYTEST_VERTICAL_SLICE";
        public const string ApprovedHostContext = "CORE_LOOP_LAB";
        public const string ApprovedHostPackageId =
            "V0.4-CoreLoopABCProductDirectionLab01";

        public BattleSandboxExplicitDevEncounterRequest(
            string schemaId,
            bool devOnly,
            string productContext,
            string hostContext,
            string hostPackageId,
            bool requestsFormalFlow,
            bool requestsCampaignFlow,
            bool requestsPlayerOrApk,
            string encounterProfileId,
            int generation,
            string startToken,
            string enemyIdentity,
            BattleSandboxExplicitDevEncounterKind encounterKind,
            int playerMaxHp,
            int playerInitialShield,
            int enemyMaxHp,
            int basicAttackDamage,
            int skillDamage,
            int targetDurationMilliseconds,
            int basicAttackIntervalMilliseconds,
            int skillCastDurationMilliseconds,
            IReadOnlyList<BattleSandboxExplicitDevEnemyActionCue>
                acceptedEnemyActionCadence,
            string profileFingerprint)
        {
            SchemaId = schemaId ?? string.Empty;
            DevOnly = devOnly;
            ProductContext = productContext ?? string.Empty;
            HostContext = hostContext ?? string.Empty;
            HostPackageId = hostPackageId ?? string.Empty;
            RequestsFormalFlow = requestsFormalFlow;
            RequestsCampaignFlow = requestsCampaignFlow;
            RequestsPlayerOrApk = requestsPlayerOrApk;
            EncounterProfileId = encounterProfileId ?? string.Empty;
            Generation = generation;
            StartToken = startToken ?? string.Empty;
            EnemyIdentity = enemyIdentity ?? string.Empty;
            EncounterKind = encounterKind;
            PlayerMaxHp = playerMaxHp;
            PlayerInitialShield = playerInitialShield;
            EnemyMaxHp = enemyMaxHp;
            BasicAttackDamage = basicAttackDamage;
            SkillDamage = skillDamage;
            TargetDurationMilliseconds = targetDurationMilliseconds;
            BasicAttackIntervalMilliseconds =
                basicAttackIntervalMilliseconds;
            SkillCastDurationMilliseconds = skillCastDurationMilliseconds;
            AcceptedEnemyActionCadence =
                (acceptedEnemyActionCadence
                    ?? Array.Empty<BattleSandboxExplicitDevEnemyActionCue>())
                .Where(value => value != null)
                .Select(value => new BattleSandboxExplicitDevEnemyActionCue(
                    value.Sequence,
                    value.ActionKind,
                    value.AtMilliseconds))
                .ToArray();
            ProfileFingerprint = profileFingerprint ?? string.Empty;
        }

        public string SchemaId { get; }
        public bool DevOnly { get; }
        public string ProductContext { get; }
        public string HostContext { get; }
        public string HostPackageId { get; }
        public bool RequestsFormalFlow { get; }
        public bool RequestsCampaignFlow { get; }
        public bool RequestsPlayerOrApk { get; }
        public string EncounterProfileId { get; }
        public int Generation { get; }
        public string StartToken { get; }
        public string EnemyIdentity { get; }
        public BattleSandboxExplicitDevEncounterKind EncounterKind { get; }
        public int PlayerMaxHp { get; }
        public int PlayerInitialShield { get; }
        public int EnemyMaxHp { get; }
        public int BasicAttackDamage { get; }
        public int SkillDamage { get; }
        public int TargetDurationMilliseconds { get; }
        public int BasicAttackIntervalMilliseconds { get; }
        public int SkillCastDurationMilliseconds { get; }
        public IReadOnlyList<BattleSandboxExplicitDevEnemyActionCue>
            AcceptedEnemyActionCadence { get; }
        public string ProfileFingerprint { get; }
    }

    public sealed class BattleSandboxExplicitDevEncounterStartSnapshot
    {
        public const string CurrentSchemaId =
            "BattleSandboxExplicitDevEncounterStartSnapshot.v1";

        public BattleSandboxExplicitDevEncounterStartSnapshot(
            string enemyIdentity,
            BattleSandboxExplicitDevEncounterKind encounterKind,
            int playerMaxHp,
            int playerInitialShield,
            int enemyStartHp,
            int enemyMaxHp,
            int basicAttackDamage,
            int skillDamage,
            int targetDurationMilliseconds,
            int basicAttackIntervalMilliseconds,
            int skillCastDurationMilliseconds,
            IReadOnlyList<BattleSandboxExplicitDevEnemyActionCue>
                acceptedEnemyActionCadence,
            long runtimeGeneration,
            long runtimeRevision,
            string initialLedgerFingerprint)
        {
            EnemyIdentity = enemyIdentity ?? string.Empty;
            EncounterKind = encounterKind;
            PlayerMaxHp = playerMaxHp;
            PlayerInitialShield = playerInitialShield;
            EnemyStartHp = enemyStartHp;
            EnemyMaxHp = enemyMaxHp;
            BasicAttackDamage = basicAttackDamage;
            SkillDamage = skillDamage;
            TargetDurationMilliseconds = targetDurationMilliseconds;
            BasicAttackIntervalMilliseconds =
                basicAttackIntervalMilliseconds;
            SkillCastDurationMilliseconds = skillCastDurationMilliseconds;
            AcceptedEnemyActionCadence =
                (acceptedEnemyActionCadence
                    ?? Array.Empty<BattleSandboxExplicitDevEnemyActionCue>())
                .ToArray();
            RuntimeGeneration = runtimeGeneration;
            RuntimeRevision = runtimeRevision;
            InitialLedgerFingerprint = initialLedgerFingerprint
                ?? string.Empty;
        }

        public string SchemaId => CurrentSchemaId;
        public string EnemyIdentity { get; }
        public BattleSandboxExplicitDevEncounterKind EncounterKind { get; }
        public int PlayerMaxHp { get; }
        public int PlayerInitialShield { get; }
        public int EnemyStartHp { get; }
        public int EnemyMaxHp { get; }
        public int BasicAttackDamage { get; }
        public int SkillDamage { get; }
        public int TargetDurationMilliseconds { get; }
        public int BasicAttackIntervalMilliseconds { get; }
        public int SkillCastDurationMilliseconds { get; }
        public IReadOnlyList<BattleSandboxExplicitDevEnemyActionCue>
            AcceptedEnemyActionCadence { get; }
        public long RuntimeGeneration { get; }
        public long RuntimeRevision { get; }
        public string InitialLedgerFingerprint { get; }
    }

    public sealed class BattleSandboxExplicitDevEncounterAcceptedStart
    {
        public const string CurrentSchemaId =
            "BattleSandboxExplicitDevEncounterAcceptedStart.v1";

        public BattleSandboxExplicitDevEncounterAcceptedStart(
            int acceptedGeneration,
            string startToken,
            string encounterProfileId,
            string profileFingerprint,
            BattleSandboxExplicitDevEncounterStartSnapshot startSnapshot)
        {
            AcceptedGeneration = acceptedGeneration;
            StartToken = startToken ?? string.Empty;
            EncounterProfileId = encounterProfileId ?? string.Empty;
            ProfileFingerprint = profileFingerprint ?? string.Empty;
            StartSnapshot = startSnapshot;
        }

        public string SchemaId => CurrentSchemaId;
        public int AcceptedGeneration { get; }
        public string StartToken { get; }
        public string EncounterProfileId { get; }
        public string ProfileFingerprint { get; }
        public BattleSandboxExplicitDevEncounterStartSnapshot StartSnapshot
        {
            get;
        }
    }

    public static class BattleSandboxExplicitDevEncounterContract
    {
        public static bool Validate(
            BattleSandboxExplicitDevEncounterRequest request,
            bool isEditor,
            out BattleSandboxExplicitDevEncounterRejectReason rejectReason,
            out string diagnosticCode)
        {
            rejectReason = BattleSandboxExplicitDevEncounterRejectReason.None;
            diagnosticCode = "NONE";
            if (request == null)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason.RequestMissing,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (!string.Equals(
                    request.SchemaId,
                    BattleSandboxExplicitDevEncounterRequest.CurrentSchemaId,
                    StringComparison.Ordinal))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason.SchemaMismatch,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (!request.DevOnly)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason.DevOnlyRequired,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (!string.Equals(
                    request.ProductContext,
                    BattleSandboxExplicitDevEncounterRequest
                        .RequiredProductContext,
                    StringComparison.Ordinal))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .ProductContextRejected,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (!string.Equals(
                    request.HostContext,
                    BattleSandboxExplicitDevEncounterRequest.ApprovedHostContext,
                    StringComparison.Ordinal))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .HostContextRejected,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (!string.Equals(
                    request.HostPackageId,
                    BattleSandboxExplicitDevEncounterRequest
                        .ApprovedHostPackageId,
                    StringComparison.Ordinal))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .HostPackageRejected,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (request.RequestsFormalFlow || request.RequestsCampaignFlow)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .FormalOrCampaignRejected,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (!isEditor || request.RequestsPlayerOrApk)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .PlayerOrApkRejected,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (request.Generation <= 0)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .GenerationInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (string.IsNullOrWhiteSpace(request.StartToken))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .StartTokenInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (string.IsNullOrWhiteSpace(request.EncounterProfileId)
                || !Enum.IsDefined(
                    typeof(BattleSandboxExplicitDevEncounterKind),
                    request.EncounterKind))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .EncounterProfileInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (string.IsNullOrWhiteSpace(request.EnemyIdentity))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .EnemyIdentityInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (request.PlayerMaxHp < 200
                || request.PlayerInitialShield < 0
                || request.PlayerInitialShield > request.PlayerMaxHp)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .PlayerSurvivabilityInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (request.EnemyMaxHp <= 0)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason.EnemyHpInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }
            if (request.BasicAttackDamage <= 0 || request.SkillDamage <= 0)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .EnemyDamageInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }

            int minimumDuration = request.EncounterKind ==
                BattleSandboxExplicitDevEncounterKind.Normal
                    ? 12000
                    : 20000;
            int maximumDuration = request.EncounterKind ==
                BattleSandboxExplicitDevEncounterKind.Normal
                    ? 18000
                    : 30000;
            if (request.TargetDurationMilliseconds < minimumDuration
                || request.TargetDurationMilliseconds > maximumDuration
                || request.BasicAttackIntervalMilliseconds < 500
                || request.BasicAttackIntervalMilliseconds
                    > request.TargetDurationMilliseconds
                || request.SkillCastDurationMilliseconds < 200
                || request.SkillCastDurationMilliseconds
                    > request.TargetDurationMilliseconds)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .EnemyTimingInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }

            BattleSandboxExplicitDevEnemyActionCue[] cadence =
                request.AcceptedEnemyActionCadence?.ToArray()
                ?? Array.Empty<BattleSandboxExplicitDevEnemyActionCue>();
            int basicCount = 0;
            int skillCount = 0;
            int lastMilliseconds = -1;
            for (int index = 0; index < cadence.Length; index++)
            {
                BattleSandboxExplicitDevEnemyActionCue cue = cadence[index];
                if (cue == null
                    || cue.Sequence != index + 1
                    || cue.AtMilliseconds <= lastMilliseconds
                    || cue.AtMilliseconds <= 0
                    || cue.AtMilliseconds
                        >= request.TargetDurationMilliseconds
                    || !Enum.IsDefined(
                        typeof(BattleSandboxExplicitDevEnemyActionKind),
                        cue.ActionKind))
                {
                    return Reject(
                        BattleSandboxExplicitDevEncounterRejectReason
                            .EnemyCadenceInvalid,
                        out rejectReason,
                        out diagnosticCode);
                }
                lastMilliseconds = cue.AtMilliseconds;
                if (cue.ActionKind ==
                    BattleSandboxExplicitDevEnemyActionKind.BasicAttack)
                {
                    basicCount++;
                }
                else
                {
                    skillCount++;
                }
            }

            bool cadenceValid = request.EncounterKind ==
                BattleSandboxExplicitDevEncounterKind.Normal
                    ? basicCount >= 2 && basicCount <= 3 && skillCount == 0
                    : basicCount >= 4 && basicCount <= 6
                      && skillCount >= 1;
            if (!cadenceValid)
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .EnemyCadenceInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }

            string expectedFingerprint = ComputeProfileFingerprint(request);
            if (string.IsNullOrWhiteSpace(request.ProfileFingerprint)
                || !string.Equals(
                    request.ProfileFingerprint,
                    expectedFingerprint,
                    StringComparison.Ordinal))
            {
                return Reject(
                    BattleSandboxExplicitDevEncounterRejectReason
                        .ProfileFingerprintInvalid,
                    out rejectReason,
                    out diagnosticCode);
            }
            return true;
        }

        public static string ComputeProfileFingerprint(
            BattleSandboxExplicitDevEncounterRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }

            StringBuilder value = new();
            Append(value, request.EncounterProfileId);
            Append(value, request.EnemyIdentity);
            Append(value, ((int)request.EncounterKind).ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.PlayerMaxHp.ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.PlayerInitialShield.ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.EnemyMaxHp.ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.BasicAttackDamage.ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.SkillDamage.ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.TargetDurationMilliseconds.ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.BasicAttackIntervalMilliseconds.ToString(
                CultureInfo.InvariantCulture));
            Append(value, request.SkillCastDurationMilliseconds.ToString(
                CultureInfo.InvariantCulture));
            foreach (BattleSandboxExplicitDevEnemyActionCue cue in
                     request.AcceptedEnemyActionCadence
                     ?? Array.Empty<BattleSandboxExplicitDevEnemyActionCue>())
            {
                Append(value, cue.Sequence.ToString(
                    CultureInfo.InvariantCulture));
                Append(value, ((int)cue.ActionKind).ToString(
                    CultureInfo.InvariantCulture));
                Append(value, cue.AtMilliseconds.ToString(
                    CultureInfo.InvariantCulture));
            }
            return ComputeStableFingerprint(value.ToString());
        }

        public static bool IsIdempotentlyEquivalent(
            BattleSandboxExplicitDevEncounterRequest left,
            BattleSandboxExplicitDevEncounterRequest right)
        {
            return left != null
                && right != null
                && left.Generation == right.Generation
                && string.Equals(
                    left.StartToken,
                    right.StartToken,
                    StringComparison.Ordinal)
                && string.Equals(
                    left.ProfileFingerprint,
                    right.ProfileFingerprint,
                    StringComparison.Ordinal);
        }

        public static string ComputeInitialLedgerFingerprint(
            BattleSandboxExplicitDevEncounterRequest request,
            long runtimeGeneration,
            long runtimeRevision)
        {
            if (request == null)
            {
                return string.Empty;
            }
            return ComputeStableFingerprint(string.Join("|", new[]
            {
                request.Generation.ToString(CultureInfo.InvariantCulture),
                request.StartToken,
                request.EncounterProfileId,
                request.ProfileFingerprint,
                request.PlayerMaxHp.ToString(CultureInfo.InvariantCulture),
                request.PlayerInitialShield.ToString(CultureInfo.InvariantCulture),
                request.EnemyMaxHp.ToString(CultureInfo.InvariantCulture),
                runtimeGeneration.ToString(CultureInfo.InvariantCulture),
                runtimeRevision.ToString(CultureInfo.InvariantCulture)
            }));
        }

        private static bool Reject(
            BattleSandboxExplicitDevEncounterRejectReason reason,
            out BattleSandboxExplicitDevEncounterRejectReason rejectReason,
            out string diagnosticCode)
        {
            rejectReason = reason;
            diagnosticCode = "EXPLICIT_DEV_ENCOUNTER_" + reason.ToString()
                .ToUpperInvariant();
            return false;
        }

        private static void Append(StringBuilder builder, string value)
        {
            string safe = value ?? string.Empty;
            builder.Append(safe.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(safe);
            builder.Append('|');
        }

        private static string ComputeStableFingerprint(string value)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            foreach (char character in value ?? string.Empty)
            {
                hash ^= character;
                hash *= prime;
            }
            return hash.ToString("X16", CultureInfo.InvariantCulture);
        }
    }
}
