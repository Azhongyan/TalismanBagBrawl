using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant;

namespace TalismanBag.EnemySystem.BoneAspect.CampaignBalance.C1Stage3To5Playtest
{
    public static class C1Stage3To5PlaytestEnemyEncounterContract
    {
        public const string SchemaId = "C1Stage3To5PlaytestEnemyEncounter.v1";
        public const string ProfileRevision =
            "C1_STAGE3_TO5_PLAYTEST_ENEMY_ENCOUNTER_R1";
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string CandidateDisposition = "PLAYTEST_V1_NOT_FINAL_BALANCE";
        public const string ActivationStatus =
            "PLAYTEST_V1_RELEASED_FOR_FORMAL_CONSUMPTION";
        public const string BalanceProfileId =
            "campaign.normal.lv1.balance.c1.stage3to5.playtest.v1";

        public const string Stage1_3 = "1-3";
        public const string Stage1_4 = "1-4";
        public const string Stage1_5 = "1-5";
        public const string EncounterVariant1_3 =
            "campaign.normal.lv1.encounter.c1.1-3.playtest.v1";
        public const string EncounterVariant1_4 =
            "campaign.normal.lv1.encounter.c1.1-4.playtest.v1";
        public const string EncounterVariant1_5 =
            "campaign.normal.lv1.encounter.c1.1-5.playtest.v1";

        public const string AttackCadenceScope =
            "ENCOUNTER_WAVE_SINGLE_APPLICATION";
        public const string DownstreamConditionBattleContractRequired =
            "BATTLE_CONTRACT_REQUIRED";
        public const decimal FirstResolveSeconds = 4.5m;
        public const decimal IntervalSeconds = 4.5m;
        public const decimal DamagePerApplication = 7m;
        public const int ApplicationsPerWave = 1;

        public const string BoneSwapSourceKind =
            "PLAYER_RESOLVED_SINGLE_TARGET_DIRECT_DAMAGE";
        public const int BoneSwapRatioBasisPoints = 2500;
        public const int BoneSwapCapDamage = 7;
        public const int BoneSwapTelegraphMilliseconds = 1500;
        public const int BoneSwapRecoverMilliseconds = 18000;
        public const string BoneSwapPendingPolicy = "LATEST_WINS_ONE_SLOT";
        public const string BoneSwapTuningStatus = "PLAYTEST_V1";

        public const int Encounter1_3TotalHp = 40;
        public const int Encounter1_4TotalHp = 180;
        public const int Encounter1_5TotalHp = 220;

        public const string DefenseSourceSchemaId =
            "BoneAspectC1EnemyRuntime.v2";
        public const string DefenseSourceRevision =
            "C1_STAGE3_TO5_PLAYTEST_ENEMY_DEFENSE_FACT_R1";
        public const string LayeredShieldMechanicId =
            "mechanic.layered_shield";
        public const string ShellBreakTargetId = "enemy.resource.shell";
        public const string ShellBrokenStateId = "enemy.shell_state.broken";
        public const string ShellBreakCounterWindowId =
            "counter_window.shell_break";
        public const string NoShellRegenerationPolicyId =
            "NO_REGENERATION_THIS_ENCOUNTER";
        public const string NotApplicablePolicyId = "NOT_APPLICABLE";
        public const int PorcelainHoundMaxShell = 100;
        public const int PorcelainHoundInitialShell = 100;
        public const int PorcelainHoundShellRegenerationCount = 0;
    }

    public sealed class C1Stage3To5PlaytestEnemyDefenseFact
    {
        public C1Stage3To5PlaytestEnemyDefenseFact(
            bool hasShell,
            string shellMechanicId,
            int maxShell,
            int initialShell,
            int shellRegenerationCount,
            string shellRegenerationPolicyId,
            string breakTargetId,
            string brokenStateId,
            string shellBreakCounterWindowId,
            string sourceSchemaId,
            string sourceProfileId,
            string sourceRevision)
        {
            HasShell = hasShell;
            ShellMechanicId = shellMechanicId ?? string.Empty;
            MaxShell = maxShell;
            InitialShell = initialShell;
            ShellRegenerationCount = shellRegenerationCount;
            ShellRegenerationPolicyId =
                shellRegenerationPolicyId ?? string.Empty;
            BreakTargetId = breakTargetId ?? string.Empty;
            BrokenStateId = brokenStateId ?? string.Empty;
            ShellBreakCounterWindowId =
                shellBreakCounterWindowId ?? string.Empty;
            SourceSchemaId = sourceSchemaId ?? string.Empty;
            SourceProfileId = sourceProfileId ?? string.Empty;
            SourceRevision = sourceRevision ?? string.Empty;
        }

        public bool HasShell { get; private set; }
        public string ShellMechanicId { get; private set; }
        public int MaxShell { get; private set; }
        public int InitialShell { get; private set; }
        public int ShellRegenerationCount { get; private set; }
        public string ShellRegenerationPolicyId { get; private set; }
        public string BreakTargetId { get; private set; }
        public string BrokenStateId { get; private set; }
        public string ShellBreakCounterWindowId { get; private set; }
        public string SourceSchemaId { get; private set; }
        public string SourceProfileId { get; private set; }
        public string SourceRevision { get; private set; }

        internal static C1Stage3To5PlaytestEnemyDefenseFact ForActor(
            string contentId,
            string runtimeProfileId,
            string operatorProfileId)
        {
            if (string.Equals(
                contentId,
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                StringComparison.Ordinal))
            {
                return new C1Stage3To5PlaytestEnemyDefenseFact(
                    true,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        LayeredShieldMechanicId,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        PorcelainHoundMaxShell,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        PorcelainHoundInitialShell,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        PorcelainHoundShellRegenerationCount,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        NoShellRegenerationPolicyId,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        ShellBreakTargetId,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        ShellBrokenStateId,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        ShellBreakCounterWindowId,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        DefenseSourceSchemaId,
                    C1EnemyRuntimeContract.PorcelainHoundProfileId,
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        DefenseSourceRevision);
            }

            string sourceProfileId = !string.IsNullOrEmpty(operatorProfileId)
                ? operatorProfileId
                : runtimeProfileId;
            string sourceSchemaId = !string.IsNullOrEmpty(operatorProfileId)
                ? BoneSwapRemnantRuntimeContract.SchemaId
                : C1Stage3To5PlaytestEnemyEncounterContract.
                    DefenseSourceSchemaId;
            return new C1Stage3To5PlaytestEnemyDefenseFact(
                false,
                string.Empty,
                0,
                0,
                0,
                C1Stage3To5PlaytestEnemyEncounterContract.
                    NotApplicablePolicyId,
                string.Empty,
                string.Empty,
                string.Empty,
                sourceSchemaId,
                sourceProfileId,
                C1Stage3To5PlaytestEnemyEncounterContract.DefenseSourceRevision);
        }

        internal C1Stage3To5PlaytestEnemyDefenseFact Copy()
        {
            return new C1Stage3To5PlaytestEnemyDefenseFact(
                HasShell,
                ShellMechanicId,
                MaxShell,
                InitialShell,
                ShellRegenerationCount,
                ShellRegenerationPolicyId,
                BreakTargetId,
                BrokenStateId,
                ShellBreakCounterWindowId,
                SourceSchemaId,
                SourceProfileId,
                SourceRevision);
        }
    }

    public sealed class C1Stage3To5PlaytestActorRow
    {
        public C1Stage3To5PlaytestActorRow(
            string actorBalanceId,
            string contentId,
            string runtimeProfileId,
            string operatorProfileId,
            int occurrenceOrdinal,
            int maxHp,
            IEnumerable<string> actionIds,
            IEnumerable<string> effectRequestKeys,
            IEnumerable<string> downstreamConditions)
            : this(
                actorBalanceId,
                contentId,
                runtimeProfileId,
                operatorProfileId,
                occurrenceOrdinal,
                maxHp,
                actionIds,
                effectRequestKeys,
                downstreamConditions,
                C1Stage3To5PlaytestEnemyDefenseFact.ForActor(
                    contentId,
                    runtimeProfileId,
                    operatorProfileId))
        {
        }

        public C1Stage3To5PlaytestActorRow(
            string actorBalanceId,
            string contentId,
            string runtimeProfileId,
            string operatorProfileId,
            int occurrenceOrdinal,
            int maxHp,
            IEnumerable<string> actionIds,
            IEnumerable<string> effectRequestKeys,
            IEnumerable<string> downstreamConditions,
            C1Stage3To5PlaytestEnemyDefenseFact defenseFact)
        {
            ActorBalanceId = actorBalanceId ?? string.Empty;
            ContentId = contentId ?? string.Empty;
            RuntimeProfileId = runtimeProfileId ?? string.Empty;
            OperatorProfileId = operatorProfileId ?? string.Empty;
            OccurrenceOrdinal = occurrenceOrdinal;
            MaxHp = maxHp;
            ActionIds = Freeze(actionIds);
            EffectRequestKeys = Freeze(effectRequestKeys);
            DownstreamConditions = Freeze(downstreamConditions);
            DefenseFact = defenseFact == null ? null : defenseFact.Copy();
            CanonicalSignature =
                C1Stage3To5PlaytestEnemyEncounterCanonical.Actor(this);
        }

        public string ActorBalanceId { get; private set; }
        public string ContentId { get; private set; }
        public string RuntimeProfileId { get; private set; }
        public string OperatorProfileId { get; private set; }
        public int OccurrenceOrdinal { get; private set; }
        public int MaxHp { get; private set; }
        public IReadOnlyList<string> ActionIds { get; private set; }
        public IReadOnlyList<string> EffectRequestKeys { get; private set; }
        public IReadOnlyList<string> DownstreamConditions { get; private set; }
        public C1Stage3To5PlaytestEnemyDefenseFact DefenseFact
        {
            get;
            private set;
        }
        public string CanonicalSignature { get; private set; }

        internal C1Stage3To5PlaytestActorRow Copy()
        {
            return new C1Stage3To5PlaytestActorRow(
                ActorBalanceId,
                ContentId,
                RuntimeProfileId,
                OperatorProfileId,
                OccurrenceOrdinal,
                MaxHp,
                ActionIds,
                EffectRequestKeys,
                DownstreamConditions,
                DefenseFact);
        }

        private static IReadOnlyList<string> Freeze(IEnumerable<string> values)
        {
            return new ReadOnlyCollection<string>(
                (values ?? Enumerable.Empty<string>()).
                    Select(value => value ?? string.Empty).ToList());
        }
    }

    public sealed class C1Stage3To5PlaytestAttackWaveRow
    {
        public C1Stage3To5PlaytestAttackWaveRow(
            string cadenceScope,
            string effectRequestKey,
            decimal firstResolveSeconds,
            decimal intervalSeconds,
            decimal damagePerApplication,
            int applicationsPerWave)
        {
            CadenceScope = cadenceScope ?? string.Empty;
            EffectRequestKey = effectRequestKey ?? string.Empty;
            FirstResolveSeconds = firstResolveSeconds;
            IntervalSeconds = intervalSeconds;
            DamagePerApplication = damagePerApplication;
            ApplicationsPerWave = applicationsPerWave;
            CanonicalSignature =
                C1Stage3To5PlaytestEnemyEncounterCanonical.AttackWave(this);
        }

        public string CadenceScope { get; private set; }
        public string EffectRequestKey { get; private set; }
        public decimal FirstResolveSeconds { get; private set; }
        public decimal IntervalSeconds { get; private set; }
        public decimal DamagePerApplication { get; private set; }
        public int ApplicationsPerWave { get; private set; }
        public string CanonicalSignature { get; private set; }

        internal C1Stage3To5PlaytestAttackWaveRow Copy()
        {
            return new C1Stage3To5PlaytestAttackWaveRow(
                CadenceScope,
                EffectRequestKey,
                FirstResolveSeconds,
                IntervalSeconds,
                DamagePerApplication,
                ApplicationsPerWave);
        }
    }

    public sealed class C1Stage3To5BoneSwapReleaseRow
    {
        public C1Stage3To5BoneSwapReleaseRow(
            string contentId,
            string operatorProfileId,
            string actionId,
            string effectRequestKey,
            string acceptedSourceKind,
            int ratioBasisPoints,
            int capDamage,
            int telegraphMilliseconds,
            int recoverMilliseconds,
            string pendingPolicy,
            string tuningStatus,
            bool isEnabled,
            bool entersFormalFlow,
            bool runtimeBoundToBattle)
        {
            ContentId = contentId ?? string.Empty;
            OperatorProfileId = operatorProfileId ?? string.Empty;
            ActionId = actionId ?? string.Empty;
            EffectRequestKey = effectRequestKey ?? string.Empty;
            AcceptedSourceKind = acceptedSourceKind ?? string.Empty;
            RatioBasisPoints = ratioBasisPoints;
            CapDamage = capDamage;
            TelegraphMilliseconds = telegraphMilliseconds;
            RecoverMilliseconds = recoverMilliseconds;
            PendingPolicy = pendingPolicy ?? string.Empty;
            TuningStatus = tuningStatus ?? string.Empty;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
            RuntimeBoundToBattle = runtimeBoundToBattle;
            CanonicalSignature =
                C1Stage3To5PlaytestEnemyEncounterCanonical.BoneSwap(this);
        }

        public string ContentId { get; private set; }
        public string OperatorProfileId { get; private set; }
        public string ActionId { get; private set; }
        public string EffectRequestKey { get; private set; }
        public string AcceptedSourceKind { get; private set; }
        public int RatioBasisPoints { get; private set; }
        public int CapDamage { get; private set; }
        public int TelegraphMilliseconds { get; private set; }
        public int RecoverMilliseconds { get; private set; }
        public string PendingPolicy { get; private set; }
        public string TuningStatus { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool EntersFormalFlow { get; private set; }
        public bool RuntimeBoundToBattle { get; private set; }
        public string CanonicalSignature { get; private set; }

        internal C1Stage3To5BoneSwapReleaseRow Copy()
        {
            return new C1Stage3To5BoneSwapReleaseRow(
                ContentId,
                OperatorProfileId,
                ActionId,
                EffectRequestKey,
                AcceptedSourceKind,
                RatioBasisPoints,
                CapDamage,
                TelegraphMilliseconds,
                RecoverMilliseconds,
                PendingPolicy,
                TuningStatus,
                IsEnabled,
                EntersFormalFlow,
                RuntimeBoundToBattle);
        }
    }

    public sealed class C1Stage3To5PlaytestEncounterProfile
    {
        public C1Stage3To5PlaytestEncounterProfile(
            string schemaId,
            string profileRevision,
            string productContext,
            string candidateDisposition,
            string activationStatus,
            string balanceProfileId,
            string stageId,
            string encounterVariantId,
            IEnumerable<C1Stage3To5PlaytestActorRow> actors,
            C1Stage3To5PlaytestAttackWaveRow attackWave,
            bool isEnabled,
            bool entersFormalFlow,
            bool runtimeBoundToBattle)
        {
            SchemaId = schemaId ?? string.Empty;
            ProfileRevision = profileRevision ?? string.Empty;
            ProductContext = productContext ?? string.Empty;
            CandidateDisposition = candidateDisposition ?? string.Empty;
            ActivationStatus = activationStatus ?? string.Empty;
            BalanceProfileId = balanceProfileId ?? string.Empty;
            StageId = stageId ?? string.Empty;
            EncounterVariantId = encounterVariantId ?? string.Empty;
            Actors = new ReadOnlyCollection<C1Stage3To5PlaytestActorRow>(
                (actors ?? Enumerable.Empty<C1Stage3To5PlaytestActorRow>()).
                    Select(actor => actor == null ? null : actor.Copy()).ToList());
            AttackWave = attackWave == null ? null : attackWave.Copy();
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
            RuntimeBoundToBattle = runtimeBoundToBattle;
            CanonicalSignature =
                C1Stage3To5PlaytestEnemyEncounterCanonical.Profile(this);
        }

        public string SchemaId { get; private set; }
        public string ProfileRevision { get; private set; }
        public string ProductContext { get; private set; }
        public string CandidateDisposition { get; private set; }
        public string ActivationStatus { get; private set; }
        public string BalanceProfileId { get; private set; }
        public string StageId { get; private set; }
        public string EncounterVariantId { get; private set; }
        public IReadOnlyList<C1Stage3To5PlaytestActorRow> Actors { get; private set; }
        public C1Stage3To5PlaytestAttackWaveRow AttackWave { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool EntersFormalFlow { get; private set; }
        public bool RuntimeBoundToBattle { get; private set; }
        public string CanonicalSignature { get; private set; }

        public int TotalMaxHp
        {
            get { return Actors.Where(actor => actor != null).Sum(actor => actor.MaxHp); }
        }

        internal C1Stage3To5PlaytestEncounterProfile Copy()
        {
            return new C1Stage3To5PlaytestEncounterProfile(
                SchemaId,
                ProfileRevision,
                ProductContext,
                CandidateDisposition,
                ActivationStatus,
                BalanceProfileId,
                StageId,
                EncounterVariantId,
                Actors,
                AttackWave,
                IsEnabled,
                EntersFormalFlow,
                RuntimeBoundToBattle);
        }
    }

    public static class C1Stage3To5PlaytestEnemyEncounterCatalog
    {
        private static readonly IReadOnlyList<C1Stage3To5PlaytestEncounterProfile>
            Profiles = BuildProfiles();
        private static readonly C1Stage3To5BoneSwapReleaseRow BoneSwap =
            BuildBoneSwapRelease();
        private static readonly C1Stage3To5PlaytestEnemyEncounterValidationResult
            CatalogValidation =
                C1Stage3To5PlaytestEnemyEncounterValidation.Validate(
                    Profiles,
                    BoneSwap);
        private static readonly string CatalogCanonicalSignature =
            C1Stage3To5PlaytestEnemyEncounterCanonical.Catalog(
                Profiles,
                BoneSwap,
                CatalogValidation);

        public static IReadOnlyList<C1Stage3To5PlaytestEncounterProfile> All
        {
            get
            {
                return new ReadOnlyCollection<C1Stage3To5PlaytestEncounterProfile>(
                    Profiles.Select(profile => profile.Copy()).ToList());
            }
        }

        public static C1Stage3To5PlaytestEnemyEncounterValidationResult Validation
        {
            get { return CatalogValidation.Copy(); }
        }

        public static string CanonicalSignature
        {
            get { return CatalogCanonicalSignature; }
        }

        public static C1Stage3To5BoneSwapReleaseRow BoneSwapRelease
        {
            get { return BoneSwap.Copy(); }
        }

        public static C1Stage3To5PlaytestEncounterProfile FindByStageId(
            string stageId)
        {
            C1Stage3To5PlaytestEncounterProfile profile = Profiles.SingleOrDefault(
                candidate => string.Equals(
                    candidate.StageId,
                    stageId,
                    StringComparison.Ordinal));
            return profile == null ? null : profile.Copy();
        }

        public static C1Stage3To5PlaytestEncounterProfile FindByEncounterVariantId(
            string encounterVariantId)
        {
            C1Stage3To5PlaytestEncounterProfile profile = Profiles.SingleOrDefault(
                candidate => string.Equals(
                    candidate.EncounterVariantId,
                    encounterVariantId,
                    StringComparison.Ordinal));
            return profile == null ? null : profile.Copy();
        }

        internal static IReadOnlyList<C1Stage3To5PlaytestEncounterProfile>
            BuildProfiles()
        {
            C1Stage3To5PlaytestAttackWaveRow attack = BuildAttackWave();
            var profiles = new List<C1Stage3To5PlaytestEncounterProfile>
            {
                BuildProfile(
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3,
                    C1Stage3To5PlaytestEnemyEncounterContract.EncounterVariant1_3,
                    new[]
                    {
                        Host(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3, 1, 12),
                        Hound(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3, 1, 14),
                        Hound(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_3, 2, 14)
                    },
                    attack),
                BuildProfile(
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4,
                    C1Stage3To5PlaytestEnemyEncounterContract.EncounterVariant1_4,
                    new[]
                    {
                        Hound(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4, 1, 80),
                        Bone(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_4, 1, 100)
                    },
                    attack),
                BuildProfile(
                    C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5,
                    C1Stage3To5PlaytestEnemyEncounterContract.EncounterVariant1_5,
                    new[]
                    {
                        Host(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5, 1, 60),
                        Hound(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5, 1, 75),
                        Bone(C1Stage3To5PlaytestEnemyEncounterContract.Stage1_5, 1, 85)
                    },
                    attack)
            };
            return new ReadOnlyCollection<C1Stage3To5PlaytestEncounterProfile>(profiles);
        }

        private static C1Stage3To5PlaytestEncounterProfile BuildProfile(
            string stageId,
            string encounterVariantId,
            IEnumerable<C1Stage3To5PlaytestActorRow> actors,
            C1Stage3To5PlaytestAttackWaveRow attack)
        {
            return new C1Stage3To5PlaytestEncounterProfile(
                C1Stage3To5PlaytestEnemyEncounterContract.SchemaId,
                C1Stage3To5PlaytestEnemyEncounterContract.ProfileRevision,
                C1Stage3To5PlaytestEnemyEncounterContract.ProductContext,
                C1Stage3To5PlaytestEnemyEncounterContract.CandidateDisposition,
                C1Stage3To5PlaytestEnemyEncounterContract.ActivationStatus,
                C1Stage3To5PlaytestEnemyEncounterContract.BalanceProfileId,
                stageId,
                encounterVariantId,
                actors,
                attack,
                true,
                true,
                false);
        }

        private static C1Stage3To5PlaytestActorRow Host(
            string stageId,
            int ordinal,
            int maxHp)
        {
            return Actor(
                stageId,
                "shattered_host",
                C1EnemyRuntimeContract.ShatteredHostContentId,
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                string.Empty,
                ordinal,
                maxHp,
                new[]
                {
                    C1EnemyRuntimeContract.ShatteredHostActionId,
                    C1EnemyRuntimeContract.ShatteredHostSkillActionId
                },
                new[]
                {
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    C1EnemyRuntimeContract.PollutedPulseSkillRequest
                });
        }

        private static C1Stage3To5PlaytestActorRow Hound(
            string stageId,
            int ordinal,
            int maxHp)
        {
            return Actor(
                stageId,
                "porcelain_hound",
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                string.Empty,
                ordinal,
                maxHp,
                new[] { C1EnemyRuntimeContract.PorcelainHoundActionId },
                new[] { C1EnemyRuntimeContract.ChargeAttackRequest });
        }

        private static C1Stage3To5PlaytestActorRow Bone(
            string stageId,
            int ordinal,
            int maxHp)
        {
            return Actor(
                stageId,
                "bone_swap_remnant",
                C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                ordinal,
                maxHp,
                new[] { BoneSwapRemnantRuntimeContract.ActionId },
                new[] { BoneSwapRemnantRuntimeContract.EffectRequestKey });
        }

        private static C1Stage3To5PlaytestActorRow Actor(
            string stageId,
            string actorToken,
            string contentId,
            string runtimeProfileId,
            string operatorProfileId,
            int ordinal,
            int maxHp,
            IEnumerable<string> actionIds,
            IEnumerable<string> requestKeys)
        {
            string id = string.Format(
                CultureInfo.InvariantCulture,
                "campaign.normal.lv1.balance.actor.c1.{0}.{1}.o{2:D2}",
                stageId,
                actorToken,
                ordinal);
            return new C1Stage3To5PlaytestActorRow(
                id,
                contentId,
                runtimeProfileId,
                operatorProfileId,
                ordinal,
                maxHp,
                actionIds,
                requestKeys,
                new[]
                {
                    C1Stage3To5PlaytestEnemyEncounterContract.
                        DownstreamConditionBattleContractRequired
                });
        }

        private static C1Stage3To5PlaytestAttackWaveRow BuildAttackWave()
        {
            return new C1Stage3To5PlaytestAttackWaveRow(
                C1Stage3To5PlaytestEnemyEncounterContract.AttackCadenceScope,
                C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                C1Stage3To5PlaytestEnemyEncounterContract.FirstResolveSeconds,
                C1Stage3To5PlaytestEnemyEncounterContract.IntervalSeconds,
                C1Stage3To5PlaytestEnemyEncounterContract.DamagePerApplication,
                C1Stage3To5PlaytestEnemyEncounterContract.ApplicationsPerWave);
        }

        internal static C1Stage3To5BoneSwapReleaseRow BuildBoneSwapRelease()
        {
            return new C1Stage3To5BoneSwapReleaseRow(
                C1EnemyRuntimeContract.BoneSwapRemnantContentId,
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                BoneSwapRemnantRuntimeContract.ActionId,
                BoneSwapRemnantRuntimeContract.EffectRequestKey,
                C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapSourceKind,
                C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapRatioBasisPoints,
                C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapCapDamage,
                C1Stage3To5PlaytestEnemyEncounterContract.
                    BoneSwapTelegraphMilliseconds,
                C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapRecoverMilliseconds,
                C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapPendingPolicy,
                C1Stage3To5PlaytestEnemyEncounterContract.BoneSwapTuningStatus,
                true,
                true,
                false);
        }
    }

    internal static class C1Stage3To5PlaytestEnemyEncounterCanonical
    {
        public static string Actor(C1Stage3To5PlaytestActorRow actor)
        {
            return Hash(string.Join("|", new[]
            {
                actor.ActorBalanceId,
                actor.ContentId,
                actor.RuntimeProfileId,
                actor.OperatorProfileId,
                actor.OccurrenceOrdinal.ToString(CultureInfo.InvariantCulture),
                actor.MaxHp.ToString(CultureInfo.InvariantCulture),
                string.Join(",", actor.ActionIds),
                string.Join(",", actor.EffectRequestKeys),
                string.Join(",", actor.DownstreamConditions)
            }));
        }

        public static string AttackWave(C1Stage3To5PlaytestAttackWaveRow row)
        {
            return Hash(string.Join("|", new[]
            {
                row.CadenceScope,
                row.EffectRequestKey,
                Decimal(row.FirstResolveSeconds),
                Decimal(row.IntervalSeconds),
                Decimal(row.DamagePerApplication),
                row.ApplicationsPerWave.ToString(CultureInfo.InvariantCulture)
            }));
        }

        public static string BoneSwap(C1Stage3To5BoneSwapReleaseRow row)
        {
            return Hash(string.Join("|", new[]
            {
                row.ContentId,
                row.OperatorProfileId,
                row.ActionId,
                row.EffectRequestKey,
                row.AcceptedSourceKind,
                row.RatioBasisPoints.ToString(CultureInfo.InvariantCulture),
                row.CapDamage.ToString(CultureInfo.InvariantCulture),
                row.TelegraphMilliseconds.ToString(CultureInfo.InvariantCulture),
                row.RecoverMilliseconds.ToString(CultureInfo.InvariantCulture),
                row.PendingPolicy,
                row.TuningStatus,
                Bool(row.IsEnabled),
                Bool(row.EntersFormalFlow),
                Bool(row.RuntimeBoundToBattle)
            }));
        }

        public static string Profile(C1Stage3To5PlaytestEncounterProfile profile)
        {
            return Hash(string.Join("|", new[]
            {
                profile.SchemaId,
                profile.ProfileRevision,
                profile.ProductContext,
                profile.CandidateDisposition,
                profile.ActivationStatus,
                profile.BalanceProfileId,
                profile.StageId,
                profile.EncounterVariantId,
                string.Join(",", profile.Actors.Select(actor =>
                    actor == null ? "NULL" : actor.CanonicalSignature)),
                profile.AttackWave == null
                    ? "NULL"
                    : profile.AttackWave.CanonicalSignature,
                Bool(profile.IsEnabled),
                Bool(profile.EntersFormalFlow),
                Bool(profile.RuntimeBoundToBattle)
            }));
        }

        public static string Catalog(
            IEnumerable<C1Stage3To5PlaytestEncounterProfile> profiles,
            C1Stage3To5BoneSwapReleaseRow boneSwap,
            C1Stage3To5PlaytestEnemyEncounterValidationResult validation)
        {
            return Hash(string.Join("|", new[]
            {
                C1Stage3To5PlaytestEnemyEncounterContract.SchemaId,
                C1Stage3To5PlaytestEnemyEncounterContract.ProfileRevision,
                string.Join(",", profiles.Select(profile =>
                    profile == null ? "NULL" : profile.CanonicalSignature)),
                boneSwap == null ? "NULL" : boneSwap.CanonicalSignature,
                validation == null ? "NULL" : validation.CanonicalSignature
            }));
        }

        public static string Validation(
            bool isValid,
            IEnumerable<string> errors)
        {
            return Hash(string.Join("|", new[]
            {
                Bool(isValid),
                string.Join(",", errors ?? Enumerable.Empty<string>())
            }));
        }

        public static string Hash(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value ?? string.Empty));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (byte item in bytes)
                {
                    builder.Append(item.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static string Decimal(decimal value)
        {
            return value.ToString("0.############################", CultureInfo.InvariantCulture);
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }
    }
}
