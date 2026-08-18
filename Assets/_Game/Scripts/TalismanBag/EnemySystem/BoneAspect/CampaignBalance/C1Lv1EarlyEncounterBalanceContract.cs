using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

namespace TalismanBag.EnemySystem.BoneAspect.CampaignBalance
{
    public static class C1Lv1EarlyEncounterBalanceContract
    {
        public const string SchemaId = "C1Lv1EarlyEncounterBalance.v1";
        public const string ProfileRevision = "C1LV1_EARLY_ENCOUNTER_BALANCE_R1";
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string BalanceProfileId = "campaign.normal.lv1.balance.identity.c1";
        public const string EncounterVariant1_1 = "campaign.normal.lv1.encounter.c1.1-1";
        public const string EncounterVariant1_2 = "campaign.normal.lv1.encounter.c1.1-2";
        public const string AttackCadenceScope = "ENCOUNTER_WAVE_SINGLE_APPLICATION";
        public const string ActionSubset = "BASIC_ATTACK_ONLY";
        public const string ActivationStatus = "APPROVED_NOT_BOUND";

        public const string Stage1_1 = "1-1";
        public const string Stage1_2 = "1-2";
        public const decimal FirstResolveSeconds = 4.5m;
        public const decimal IntervalSeconds = 4.5m;
        public const decimal DamagePerApplication = 8m;
        public const int ApplicationsPerWave = 1;
        public const int Encounter1_1TotalHp = 48;
        public const int Encounter1_2TotalHp = 180;
    }

    public sealed class C1Lv1EncounterActorBalance
    {
        public C1Lv1EncounterActorBalance(
            string actorBalanceId,
            string contentId,
            string runtimeProfileId,
            int occurrenceOrdinal,
            int maxHp,
            bool shellEnabled,
            bool skillEnabled)
        {
            this.actorBalanceId = actorBalanceId ?? string.Empty;
            this.contentId = contentId ?? string.Empty;
            this.runtimeProfileId = runtimeProfileId ?? string.Empty;
            this.occurrenceOrdinal = occurrenceOrdinal;
            this.maxHp = maxHp;
            this.shellEnabled = shellEnabled;
            this.skillEnabled = skillEnabled;
            canonicalSignature = C1Lv1EarlyEncounterBalanceCanonical.ActorSignature(this);
        }

        public string actorBalanceId { get; private set; }
        public string contentId { get; private set; }
        public string runtimeProfileId { get; private set; }
        public int occurrenceOrdinal { get; private set; }
        public int maxHp { get; private set; }
        public bool shellEnabled { get; private set; }
        public bool skillEnabled { get; private set; }
        public string canonicalSignature { get; private set; }

        internal C1Lv1EncounterActorBalance Copy()
        {
            return new C1Lv1EncounterActorBalance(
                actorBalanceId,
                contentId,
                runtimeProfileId,
                occurrenceOrdinal,
                maxHp,
                shellEnabled,
                skillEnabled);
        }
    }

    public sealed class C1Lv1EncounterAttackWaveBalance
    {
        public C1Lv1EncounterAttackWaveBalance(
            string cadenceScope,
            string effectRequestKey,
            decimal firstResolveSeconds,
            decimal intervalSeconds,
            decimal damagePerApplication,
            int applicationsPerWave)
        {
            this.cadenceScope = cadenceScope ?? string.Empty;
            this.effectRequestKey = effectRequestKey ?? string.Empty;
            this.firstResolveSeconds = firstResolveSeconds;
            this.intervalSeconds = intervalSeconds;
            this.damagePerApplication = damagePerApplication;
            this.applicationsPerWave = applicationsPerWave;
            canonicalSignature = C1Lv1EarlyEncounterBalanceCanonical.AttackWaveSignature(this);
        }

        public string cadenceScope { get; private set; }
        public string effectRequestKey { get; private set; }
        public decimal firstResolveSeconds { get; private set; }
        public decimal intervalSeconds { get; private set; }
        public decimal damagePerApplication { get; private set; }
        public int applicationsPerWave { get; private set; }
        public string canonicalSignature { get; private set; }

        internal C1Lv1EncounterAttackWaveBalance Copy()
        {
            return new C1Lv1EncounterAttackWaveBalance(
                cadenceScope,
                effectRequestKey,
                firstResolveSeconds,
                intervalSeconds,
                damagePerApplication,
                applicationsPerWave);
        }
    }

    public sealed class C1Lv1EarlyEncounterBalanceProfile
    {
        private readonly ReadOnlyCollection<C1Lv1EncounterActorBalance> actorRows;
        private readonly C1Lv1EncounterAttackWaveBalance attackWaveRow;

        public C1Lv1EarlyEncounterBalanceProfile(
            string schemaId,
            string productContext,
            string profileRevision,
            string balanceProfileId,
            string encounterVariantId,
            string stageId,
            IEnumerable<C1Lv1EncounterActorBalance> actors,
            C1Lv1EncounterAttackWaveBalance attackWave,
            string actionSubset,
            string activationStatus,
            bool isEnabled,
            bool entersFormalFlow,
            bool runtimeBoundToBattle)
        {
            this.schemaId = schemaId ?? string.Empty;
            this.productContext = productContext ?? string.Empty;
            this.profileRevision = profileRevision ?? string.Empty;
            this.balanceProfileId = balanceProfileId ?? string.Empty;
            this.encounterVariantId = encounterVariantId ?? string.Empty;
            this.stageId = stageId ?? string.Empty;
            actorRows = CopyActors(actors);
            attackWaveRow = attackWave == null ? null : attackWave.Copy();
            this.actionSubset = actionSubset ?? string.Empty;
            this.activationStatus = activationStatus ?? string.Empty;
            this.isEnabled = isEnabled;
            this.entersFormalFlow = entersFormalFlow;
            this.runtimeBoundToBattle = runtimeBoundToBattle;
            canonicalSignature = C1Lv1EarlyEncounterBalanceCanonical.ProfileSignature(this);
        }

        public string schemaId { get; private set; }
        public string productContext { get; private set; }
        public string profileRevision { get; private set; }
        public string balanceProfileId { get; private set; }
        public string encounterVariantId { get; private set; }
        public string stageId { get; private set; }

        public IReadOnlyList<C1Lv1EncounterActorBalance> actors
        {
            get { return CopyActors(actorRows); }
        }

        public C1Lv1EncounterAttackWaveBalance attackWave
        {
            get { return attackWaveRow == null ? null : attackWaveRow.Copy(); }
        }

        public string actionSubset { get; private set; }
        public string activationStatus { get; private set; }
        public bool isEnabled { get; private set; }
        public bool entersFormalFlow { get; private set; }
        public bool runtimeBoundToBattle { get; private set; }
        public string canonicalSignature { get; private set; }

        internal C1Lv1EarlyEncounterBalanceProfile Copy()
        {
            return new C1Lv1EarlyEncounterBalanceProfile(
                schemaId,
                productContext,
                profileRevision,
                balanceProfileId,
                encounterVariantId,
                stageId,
                actorRows,
                attackWaveRow,
                actionSubset,
                activationStatus,
                isEnabled,
                entersFormalFlow,
                runtimeBoundToBattle);
        }

        private static ReadOnlyCollection<C1Lv1EncounterActorBalance> CopyActors(
            IEnumerable<C1Lv1EncounterActorBalance> source)
        {
            C1Lv1EncounterActorBalance[] rows;
            try
            {
                rows = (source ?? Enumerable.Empty<C1Lv1EncounterActorBalance>())
                    .Select(delegate(C1Lv1EncounterActorBalance actor)
                    {
                        return actor == null ? null : actor.Copy();
                    })
                    .ToArray();
            }
            catch
            {
                rows = new C1Lv1EncounterActorBalance[0];
            }

            return Array.AsReadOnly(rows);
        }
    }

    public static class C1Lv1EarlyEncounterBalanceCatalog
    {
        private static readonly ReadOnlyCollection<C1Lv1EarlyEncounterBalanceProfile> Profiles =
            Array.AsReadOnly(new[]
            {
                BuildEncounter1_1(),
                BuildEncounter1_2()
            });

        private static readonly C1Lv1EarlyEncounterBalanceValidationResult CatalogValidation =
            C1Lv1EarlyEncounterBalanceValidation.Validate(Profiles);

        public static IReadOnlyList<C1Lv1EarlyEncounterBalanceProfile> All
        {
            get
            {
                return Array.AsReadOnly(Profiles
                    .Select(delegate(C1Lv1EarlyEncounterBalanceProfile profile)
                    {
                        return profile.Copy();
                    })
                    .ToArray());
            }
        }

        public static C1Lv1EarlyEncounterBalanceValidationResult Validation
        {
            get { return CatalogValidation.Copy(); }
        }

        public static string canonicalSignature
        {
            get { return CatalogValidation.canonicalSignature; }
        }

        public static C1Lv1EarlyEncounterBalanceProfile Find(string encounterVariantId)
        {
            C1Lv1EarlyEncounterBalanceProfile match = Profiles.SingleOrDefault(
                delegate(C1Lv1EarlyEncounterBalanceProfile profile)
                {
                    return string.Equals(
                        profile.encounterVariantId,
                        encounterVariantId,
                        StringComparison.Ordinal);
                });
            return match == null ? null : match.Copy();
        }

        private static C1Lv1EarlyEncounterBalanceProfile BuildEncounter1_1()
        {
            return BuildProfile(
                C1Lv1EarlyEncounterBalanceContract.EncounterVariant1_1,
                C1Lv1EarlyEncounterBalanceContract.Stage1_1,
                new[]
                {
                    Host(C1Lv1EarlyEncounterBalanceContract.Stage1_1, 1, 24),
                    Host(C1Lv1EarlyEncounterBalanceContract.Stage1_1, 2, 24)
                });
        }

        private static C1Lv1EarlyEncounterBalanceProfile BuildEncounter1_2()
        {
            return BuildProfile(
                C1Lv1EarlyEncounterBalanceContract.EncounterVariant1_2,
                C1Lv1EarlyEncounterBalanceContract.Stage1_2,
                new[]
                {
                    Host(C1Lv1EarlyEncounterBalanceContract.Stage1_2, 1, 56),
                    Host(C1Lv1EarlyEncounterBalanceContract.Stage1_2, 2, 56),
                    Hound(C1Lv1EarlyEncounterBalanceContract.Stage1_2, 1, 68)
                });
        }

        private static C1Lv1EarlyEncounterBalanceProfile BuildProfile(
            string encounterVariantId,
            string stageId,
            IEnumerable<C1Lv1EncounterActorBalance> actors)
        {
            return new C1Lv1EarlyEncounterBalanceProfile(
                C1Lv1EarlyEncounterBalanceContract.SchemaId,
                C1Lv1EarlyEncounterBalanceContract.ProductContext,
                C1Lv1EarlyEncounterBalanceContract.ProfileRevision,
                C1Lv1EarlyEncounterBalanceContract.BalanceProfileId,
                encounterVariantId,
                stageId,
                actors,
                new C1Lv1EncounterAttackWaveBalance(
                    C1Lv1EarlyEncounterBalanceContract.AttackCadenceScope,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    C1Lv1EarlyEncounterBalanceContract.FirstResolveSeconds,
                    C1Lv1EarlyEncounterBalanceContract.IntervalSeconds,
                    C1Lv1EarlyEncounterBalanceContract.DamagePerApplication,
                    C1Lv1EarlyEncounterBalanceContract.ApplicationsPerWave),
                C1Lv1EarlyEncounterBalanceContract.ActionSubset,
                C1Lv1EarlyEncounterBalanceContract.ActivationStatus,
                false,
                false,
                false);
        }

        private static C1Lv1EncounterActorBalance Host(
            string stageId,
            int occurrenceOrdinal,
            int maxHp)
        {
            return Actor(
                stageId,
                "shattered_host",
                C1EnemyRuntimeContract.ShatteredHostContentId,
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                occurrenceOrdinal,
                maxHp);
        }

        private static C1Lv1EncounterActorBalance Hound(
            string stageId,
            int occurrenceOrdinal,
            int maxHp)
        {
            return Actor(
                stageId,
                "porcelain_hound",
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                occurrenceOrdinal,
                maxHp);
        }

        private static C1Lv1EncounterActorBalance Actor(
            string stageId,
            string actorToken,
            string contentId,
            string runtimeProfileId,
            int occurrenceOrdinal,
            int maxHp)
        {
            string actorBalanceId = string.Format(
                CultureInfo.InvariantCulture,
                "campaign.normal.lv1.balance.actor.c1.{0}.{1}.o{2:D2}",
                stageId,
                actorToken,
                occurrenceOrdinal);
            return new C1Lv1EncounterActorBalance(
                actorBalanceId,
                contentId,
                runtimeProfileId,
                occurrenceOrdinal,
                maxHp,
                false,
                false);
        }
    }

    internal static class C1Lv1EarlyEncounterBalanceCanonical
    {
        internal static string ActorSignature(C1Lv1EncounterActorBalance actor)
        {
            if (actor == null)
            {
                return string.Empty;
            }

            return Sha256(string.Join("|", new[]
            {
                "actorBalanceId=" + actor.actorBalanceId,
                "contentId=" + actor.contentId,
                "runtimeProfileId=" + actor.runtimeProfileId,
                "occurrenceOrdinal=" + actor.occurrenceOrdinal.ToString(CultureInfo.InvariantCulture),
                "maxHp=" + actor.maxHp.ToString(CultureInfo.InvariantCulture),
                "shellEnabled=" + Bool(actor.shellEnabled),
                "skillEnabled=" + Bool(actor.skillEnabled)
            }));
        }

        internal static string AttackWaveSignature(C1Lv1EncounterAttackWaveBalance wave)
        {
            if (wave == null)
            {
                return string.Empty;
            }

            return Sha256(string.Join("|", new[]
            {
                "cadenceScope=" + wave.cadenceScope,
                "effectRequestKey=" + wave.effectRequestKey,
                "firstResolveSeconds=" + Decimal(wave.firstResolveSeconds),
                "intervalSeconds=" + Decimal(wave.intervalSeconds),
                "damagePerApplication=" + Decimal(wave.damagePerApplication),
                "applicationsPerWave=" + wave.applicationsPerWave.ToString(CultureInfo.InvariantCulture)
            }));
        }

        internal static string ProfileSignature(C1Lv1EarlyEncounterBalanceProfile profile)
        {
            if (profile == null)
            {
                return string.Empty;
            }

            StringBuilder canonical = new StringBuilder();
            canonical.Append(string.Join("|", new[]
            {
                "schemaId=" + profile.schemaId,
                "productContext=" + profile.productContext,
                "profileRevision=" + profile.profileRevision,
                "balanceProfileId=" + profile.balanceProfileId,
                "encounterVariantId=" + profile.encounterVariantId,
                "stageId=" + profile.stageId,
                "actionSubset=" + profile.actionSubset,
                "activationStatus=" + profile.activationStatus,
                "isEnabled=" + Bool(profile.isEnabled),
                "entersFormalFlow=" + Bool(profile.entersFormalFlow),
                "runtimeBoundToBattle=" + Bool(profile.runtimeBoundToBattle)
            }));
            IReadOnlyList<C1Lv1EncounterActorBalance> actors = profile.actors;
            canonical.Append("\nattackWave=");
            canonical.Append(AttackWaveSignature(profile.attackWave));
            canonical.Append("\nactorCount=");
            canonical.Append(actors.Count.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < actors.Count; index++)
            {
                canonical.Append("\nactor[");
                canonical.Append(index.ToString(CultureInfo.InvariantCulture));
                canonical.Append("]=");
                canonical.Append(ActorSignature(actors[index]));
            }
            return Sha256(canonical.ToString());
        }

        internal static string CatalogSignature(
            IEnumerable<C1Lv1EarlyEncounterBalanceProfile> profiles)
        {
            C1Lv1EarlyEncounterBalanceProfile[] rows;
            try
            {
                rows = (profiles ?? Enumerable.Empty<C1Lv1EarlyEncounterBalanceProfile>())
                    .Where(delegate(C1Lv1EarlyEncounterBalanceProfile profile)
                    {
                        return profile != null;
                    })
                    .OrderBy(
                        delegate(C1Lv1EarlyEncounterBalanceProfile profile)
                        {
                            return profile.encounterVariantId;
                        },
                        StringComparer.Ordinal)
                    .ToArray();
            }
            catch
            {
                return string.Empty;
            }

            StringBuilder canonical = new StringBuilder();
            canonical.Append("schemaId=");
            canonical.Append(C1Lv1EarlyEncounterBalanceContract.SchemaId);
            canonical.Append("|profileRevision=");
            canonical.Append(C1Lv1EarlyEncounterBalanceContract.ProfileRevision);
            canonical.Append("|profileCount=");
            canonical.Append(rows.Length.ToString(CultureInfo.InvariantCulture));
            foreach (C1Lv1EarlyEncounterBalanceProfile profile in rows)
            {
                canonical.Append('\n');
                canonical.Append(ProfileSignature(profile));
            }
            return Sha256(canonical.ToString());
        }

        private static string Sha256(string value)
        {
            byte[] hash;
            using (SHA256 algorithm = SHA256.Create())
            {
                hash = algorithm.ComputeHash(new UTF8Encoding(false).GetBytes(value ?? string.Empty));
            }

            StringBuilder result = new StringBuilder(hash.Length * 2);
            foreach (byte part in hash)
            {
                result.Append(part.ToString("x2", CultureInfo.InvariantCulture));
            }
            return result.ToString();
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
