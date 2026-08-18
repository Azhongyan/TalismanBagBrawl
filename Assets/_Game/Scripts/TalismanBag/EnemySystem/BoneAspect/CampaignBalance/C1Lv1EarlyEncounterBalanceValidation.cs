using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

namespace TalismanBag.EnemySystem.BoneAspect.CampaignBalance
{
    public static class C1Lv1EarlyEncounterBalanceErrorCodes
    {
        public const string SourceNull = "SOURCE_NULL";
        public const string SourceEnumerationFailed = "SOURCE_ENUMERATION_FAILED";
        public const string ProfileNull = "PROFILE_NULL";
        public const string ProfileCountInvalid = "PROFILE_COUNT_INVALID";
        public const string RequiredIdentifierMissing = "REQUIRED_IDENTIFIER_MISSING";
        public const string SchemaMismatch = "SCHEMA_MISMATCH";
        public const string ProductContextMismatch = "PRODUCT_CONTEXT_MISMATCH";
        public const string ProfileRevisionMismatch = "PROFILE_REVISION_MISMATCH";
        public const string BalanceProfileIdMismatch = "BALANCE_PROFILE_ID_MISMATCH";
        public const string EncounterVariantMismatch = "ENCOUNTER_VARIANT_MISMATCH";
        public const string EncounterMissing = "ENCOUNTER_MISSING";
        public const string EncounterDuplicate = "ENCOUNTER_DUPLICATE";
        public const string StageIdMismatch = "STAGE_ID_MISMATCH";
        public const string ActorMissing = "ACTOR_MISSING";
        public const string ActorCountInvalid = "ACTOR_COUNT_INVALID";
        public const string ActorBalanceIdDuplicate = "ACTOR_BALANCE_ID_DUPLICATE";
        public const string ActorOccurrenceDuplicate = "ACTOR_OCCURRENCE_DUPLICATE";
        public const string ActorIdentityOrOrderMismatch = "ACTOR_IDENTITY_OR_ORDER_MISMATCH";
        public const string ActorHpInvalid = "ACTOR_HP_INVALID";
        public const string ActorHpMismatch = "ACTOR_HP_MISMATCH";
        public const string EncounterHpTotalMismatch = "ENCOUNTER_HP_TOTAL_MISMATCH";
        public const string ShellOrSkillEnabled = "SHELL_OR_SKILL_ENABLED";
        public const string AttackWaveMissing = "ATTACK_WAVE_MISSING";
        public const string AttackCadenceInvalid = "ATTACK_CADENCE_INVALID";
        public const string AttackWaveValueMismatch = "ATTACK_WAVE_VALUE_MISMATCH";
        public const string EffectRequestMismatch = "EFFECT_REQUEST_MISMATCH";
        public const string ActionSubsetMismatch = "ACTION_SUBSET_MISMATCH";
        public const string ActivationStatusMismatch = "ACTIVATION_STATUS_MISMATCH";
        public const string PrematureActivation = "PREMATURE_ACTIVATION";
        public const string CanonicalSignatureMissing = "CANONICAL_SIGNATURE_MISSING";
        public const string CanonicalSignatureMismatch = "CANONICAL_SIGNATURE_MISMATCH";
    }

    public sealed class C1Lv1EarlyEncounterBalanceValidationError
    {
        public C1Lv1EarlyEncounterBalanceValidationError(
            string errorCode,
            string path,
            string message)
        {
            this.errorCode = errorCode ?? string.Empty;
            this.path = path ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string errorCode { get; private set; }
        public string path { get; private set; }
        public string message { get; private set; }
    }

    public sealed class C1Lv1EarlyEncounterBalanceValidationResult
    {
        private readonly ReadOnlyCollection<C1Lv1EarlyEncounterBalanceValidationError> errorRows;

        internal C1Lv1EarlyEncounterBalanceValidationResult(
            IEnumerable<C1Lv1EarlyEncounterBalanceValidationError> errors,
            string canonicalSignature)
        {
            errorRows = Array.AsReadOnly((errors
                ?? Enumerable.Empty<C1Lv1EarlyEncounterBalanceValidationError>())
                .Select(delegate(C1Lv1EarlyEncounterBalanceValidationError error)
                {
                    return new C1Lv1EarlyEncounterBalanceValidationError(
                        error.errorCode,
                        error.path,
                        error.message);
                })
                .ToArray());
            this.canonicalSignature = canonicalSignature ?? string.Empty;
        }

        public bool isValid
        {
            get { return errorRows.Count == 0; }
        }

        public IReadOnlyList<C1Lv1EarlyEncounterBalanceValidationError> errors
        {
            get
            {
                return Array.AsReadOnly(errorRows
                    .Select(delegate(C1Lv1EarlyEncounterBalanceValidationError error)
                    {
                        return new C1Lv1EarlyEncounterBalanceValidationError(
                            error.errorCode,
                            error.path,
                            error.message);
                    })
                    .ToArray());
            }
        }

        public string canonicalSignature { get; private set; }

        public bool HasError(string errorCode)
        {
            return errorRows.Any(delegate(C1Lv1EarlyEncounterBalanceValidationError error)
            {
                return string.Equals(error.errorCode, errorCode, StringComparison.Ordinal);
            });
        }

        internal C1Lv1EarlyEncounterBalanceValidationResult Copy()
        {
            return new C1Lv1EarlyEncounterBalanceValidationResult(
                errorRows,
                canonicalSignature);
        }
    }

    public static class C1Lv1EarlyEncounterBalanceValidation
    {
        public static C1Lv1EarlyEncounterBalanceValidationResult Validate(
            IEnumerable<C1Lv1EarlyEncounterBalanceProfile> source)
        {
            List<C1Lv1EarlyEncounterBalanceValidationError> errors =
                new List<C1Lv1EarlyEncounterBalanceValidationError>();
            if (source == null)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.SourceNull,
                    "profiles",
                    "Encounter profile source is required."));
                return Result(errors, string.Empty);
            }

            C1Lv1EarlyEncounterBalanceProfile[] profiles;
            try
            {
                profiles = source.ToArray();
            }
            catch (Exception exception)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.SourceEnumerationFailed,
                    "profiles",
                    "Encounter profile source enumeration failed: "
                        + exception.GetType().Name));
                return Result(errors, string.Empty);
            }

            if (profiles.Length != 2)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.ProfileCountInvalid,
                    "profiles",
                    "Exactly two encounter profiles are required."));
            }

            for (int index = 0; index < profiles.Length; index++)
            {
                ValidateProfile(profiles[index], index, errors);
            }

            C1Lv1EarlyEncounterBalanceProfile[] nonNull = profiles
                .Where(delegate(C1Lv1EarlyEncounterBalanceProfile profile)
                {
                    return profile != null;
                })
                .ToArray();

            foreach (IGrouping<string, C1Lv1EarlyEncounterBalanceProfile> duplicate
                in nonNull.GroupBy(
                    delegate(C1Lv1EarlyEncounterBalanceProfile profile)
                    {
                        return profile.encounterVariantId;
                    },
                    StringComparer.Ordinal)
                    .Where(delegate(IGrouping<string, C1Lv1EarlyEncounterBalanceProfile> group)
                    {
                        return group.Count() > 1;
                    }))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.EncounterDuplicate,
                    "profiles",
                    "Duplicate encounterVariantId: " + duplicate.Key));
            }

            RequireEncounter(
                nonNull,
                C1Lv1EarlyEncounterBalanceContract.EncounterVariant1_1,
                errors);
            RequireEncounter(
                nonNull,
                C1Lv1EarlyEncounterBalanceContract.EncounterVariant1_2,
                errors);

            string signature = C1Lv1EarlyEncounterBalanceCanonical.CatalogSignature(nonNull);
            if (string.IsNullOrWhiteSpace(signature))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.CanonicalSignatureMissing,
                    "profiles.canonicalSignature",
                    "Catalog canonical signature is required."));
            }
            return Result(errors, signature);
        }

        private static void ValidateProfile(
            C1Lv1EarlyEncounterBalanceProfile profile,
            int profileIndex,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            string path = "profiles[" + profileIndex + "]";
            if (profile == null)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.ProfileNull,
                    path,
                    "Encounter profile is required."));
                return;
            }

            RequireIdentifier(profile.schemaId, path + ".schemaId", errors);
            RequireIdentifier(profile.productContext, path + ".productContext", errors);
            RequireIdentifier(profile.profileRevision, path + ".profileRevision", errors);
            RequireIdentifier(profile.balanceProfileId, path + ".balanceProfileId", errors);
            RequireIdentifier(profile.encounterVariantId, path + ".encounterVariantId", errors);
            RequireIdentifier(profile.stageId, path + ".stageId", errors);
            RequireIdentifier(profile.actionSubset, path + ".actionSubset", errors);
            RequireIdentifier(profile.activationStatus, path + ".activationStatus", errors);

            Match(
                profile.schemaId,
                C1Lv1EarlyEncounterBalanceContract.SchemaId,
                C1Lv1EarlyEncounterBalanceErrorCodes.SchemaMismatch,
                path + ".schemaId",
                errors);
            Match(
                profile.productContext,
                C1Lv1EarlyEncounterBalanceContract.ProductContext,
                C1Lv1EarlyEncounterBalanceErrorCodes.ProductContextMismatch,
                path + ".productContext",
                errors);
            Match(
                profile.profileRevision,
                C1Lv1EarlyEncounterBalanceContract.ProfileRevision,
                C1Lv1EarlyEncounterBalanceErrorCodes.ProfileRevisionMismatch,
                path + ".profileRevision",
                errors);
            Match(
                profile.balanceProfileId,
                C1Lv1EarlyEncounterBalanceContract.BalanceProfileId,
                C1Lv1EarlyEncounterBalanceErrorCodes.BalanceProfileIdMismatch,
                path + ".balanceProfileId",
                errors);

            bool is1_1 = string.Equals(
                profile.encounterVariantId,
                C1Lv1EarlyEncounterBalanceContract.EncounterVariant1_1,
                StringComparison.Ordinal);
            bool is1_2 = string.Equals(
                profile.encounterVariantId,
                C1Lv1EarlyEncounterBalanceContract.EncounterVariant1_2,
                StringComparison.Ordinal);
            if (!is1_1 && !is1_2)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.EncounterVariantMismatch,
                    path + ".encounterVariantId",
                    "Only the approved 1-1 and 1-2 variants are supported."));
            }

            string expectedStage = is1_1
                ? C1Lv1EarlyEncounterBalanceContract.Stage1_1
                : is1_2
                    ? C1Lv1EarlyEncounterBalanceContract.Stage1_2
                    : string.Empty;
            if (!string.Equals(profile.stageId, expectedStage, StringComparison.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.StageIdMismatch,
                    path + ".stageId",
                    "stageId does not match the encounter identity."));
            }

            Match(
                profile.actionSubset,
                C1Lv1EarlyEncounterBalanceContract.ActionSubset,
                C1Lv1EarlyEncounterBalanceErrorCodes.ActionSubsetMismatch,
                path + ".actionSubset",
                errors);
            Match(
                profile.activationStatus,
                C1Lv1EarlyEncounterBalanceContract.ActivationStatus,
                C1Lv1EarlyEncounterBalanceErrorCodes.ActivationStatusMismatch,
                path + ".activationStatus",
                errors);
            if (profile.isEnabled || profile.entersFormalFlow || profile.runtimeBoundToBattle)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.PrematureActivation,
                    path + ".activation",
                    "The approved profile must remain disabled, unbound, and outside formal flow."));
            }

            ValidateActors(profile, path, is1_1, is1_2, errors);
            ValidateAttackWave(profile.attackWave, path + ".attackWave", errors);

            if (string.IsNullOrWhiteSpace(profile.canonicalSignature))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.CanonicalSignatureMissing,
                    path + ".canonicalSignature",
                    "Profile canonical signature is required."));
            }
            else if (!string.Equals(
                profile.canonicalSignature,
                C1Lv1EarlyEncounterBalanceCanonical.ProfileSignature(profile),
                StringComparison.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.CanonicalSignatureMismatch,
                    path + ".canonicalSignature",
                    "Profile canonical signature does not match its fields."));
            }
        }

        private static void ValidateActors(
            C1Lv1EarlyEncounterBalanceProfile profile,
            string path,
            bool is1_1,
            bool is1_2,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            IReadOnlyList<C1Lv1EncounterActorBalance> actors = profile.actors;
            int expectedCount = is1_1 ? 2 : is1_2 ? 3 : -1;
            if (expectedCount >= 0 && actors.Count != expectedCount)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.ActorCountInvalid,
                    path + ".actors",
                    "Actor count does not match the approved encounter."));
            }

            for (int index = 0; index < actors.Count; index++)
            {
                C1Lv1EncounterActorBalance actor = actors[index];
                string actorPath = path + ".actors[" + index + "]";
                if (actor == null)
                {
                    errors.Add(Error(
                        C1Lv1EarlyEncounterBalanceErrorCodes.ActorMissing,
                        actorPath,
                        "Actor row is required."));
                    continue;
                }

                RequireIdentifier(actor.actorBalanceId, actorPath + ".actorBalanceId", errors);
                RequireIdentifier(actor.contentId, actorPath + ".contentId", errors);
                RequireIdentifier(actor.runtimeProfileId, actorPath + ".runtimeProfileId", errors);
                if (actor.maxHp <= 0)
                {
                    errors.Add(Error(
                        C1Lv1EarlyEncounterBalanceErrorCodes.ActorHpInvalid,
                        actorPath + ".maxHp",
                        "Actor maxHp must be positive."));
                }
                if (actor.shellEnabled || actor.skillEnabled)
                {
                    errors.Add(Error(
                        C1Lv1EarlyEncounterBalanceErrorCodes.ShellOrSkillEnabled,
                        actorPath,
                        "Shell and skill must remain disabled in the P0 profile."));
                }
                if (string.IsNullOrWhiteSpace(actor.canonicalSignature))
                {
                    errors.Add(Error(
                        C1Lv1EarlyEncounterBalanceErrorCodes.CanonicalSignatureMissing,
                        actorPath + ".canonicalSignature",
                        "Actor canonical signature is required."));
                }
                else if (!string.Equals(
                    actor.canonicalSignature,
                    C1Lv1EarlyEncounterBalanceCanonical.ActorSignature(actor),
                    StringComparison.Ordinal))
                {
                    errors.Add(Error(
                        C1Lv1EarlyEncounterBalanceErrorCodes.CanonicalSignatureMismatch,
                        actorPath + ".canonicalSignature",
                        "Actor canonical signature does not match its fields."));
                }
            }

            C1Lv1EncounterActorBalance[] nonNull = actors
                .Where(delegate(C1Lv1EncounterActorBalance actor) { return actor != null; })
                .ToArray();
            if (nonNull.GroupBy(
                delegate(C1Lv1EncounterActorBalance actor) { return actor.actorBalanceId; },
                StringComparer.Ordinal).Any(delegate(IGrouping<string, C1Lv1EncounterActorBalance> group)
                {
                    return group.Count() > 1;
                }))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.ActorBalanceIdDuplicate,
                    path + ".actors",
                    "actorBalanceId values must be unique."));
            }
            if (nonNull.GroupBy(
                delegate(C1Lv1EncounterActorBalance actor)
                {
                    return actor.contentId + "\u001f" + actor.occurrenceOrdinal;
                },
                StringComparer.Ordinal).Any(delegate(IGrouping<string, C1Lv1EncounterActorBalance> group)
                {
                    return group.Count() > 1;
                }))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.ActorOccurrenceDuplicate,
                    path + ".actors",
                    "Occurrence ordinals must be unique within each contentId."));
            }

            if (is1_1)
            {
                ValidateExpectedActor(
                    actors,
                    0,
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    1,
                    24,
                    path,
                    errors);
                ValidateExpectedActor(
                    actors,
                    1,
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    2,
                    24,
                    path,
                    errors);
                ValidateTotalHp(
                    nonNull,
                    C1Lv1EarlyEncounterBalanceContract.Encounter1_1TotalHp,
                    path,
                    errors);
            }
            else if (is1_2)
            {
                ValidateExpectedActor(
                    actors,
                    0,
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    1,
                    56,
                    path,
                    errors);
                ValidateExpectedActor(
                    actors,
                    1,
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    2,
                    56,
                    path,
                    errors);
                ValidateExpectedActor(
                    actors,
                    2,
                    C1EnemyRuntimeContract.PorcelainHoundContentId,
                    C1EnemyRuntimeContract.PorcelainHoundProfileId,
                    1,
                    68,
                    path,
                    errors);
                ValidateTotalHp(
                    nonNull,
                    C1Lv1EarlyEncounterBalanceContract.Encounter1_2TotalHp,
                    path,
                    errors);
            }
        }

        private static void ValidateExpectedActor(
            IReadOnlyList<C1Lv1EncounterActorBalance> actors,
            int index,
            string contentId,
            string runtimeProfileId,
            int occurrenceOrdinal,
            int maxHp,
            string path,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            if (index >= actors.Count || actors[index] == null)
            {
                return;
            }

            C1Lv1EncounterActorBalance actor = actors[index];
            if (!string.Equals(actor.contentId, contentId, StringComparison.Ordinal)
                || !string.Equals(actor.runtimeProfileId, runtimeProfileId, StringComparison.Ordinal)
                || actor.occurrenceOrdinal != occurrenceOrdinal)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.ActorIdentityOrOrderMismatch,
                    path + ".actors[" + index + "]",
                    "Actor identity, occurrence ordinal, or order is not approved."));
            }
            if (actor.maxHp != maxHp)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.ActorHpMismatch,
                    path + ".actors[" + index + "].maxHp",
                    "Actor maxHp does not match the approved profile."));
            }
        }

        private static void ValidateTotalHp(
            IEnumerable<C1Lv1EncounterActorBalance> actors,
            int expected,
            string path,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            int actual = actors.Sum(delegate(C1Lv1EncounterActorBalance actor)
            {
                return actor.maxHp;
            });
            if (actual != expected)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.EncounterHpTotalMismatch,
                    path + ".actors.maxHp",
                    "Actor HP sum does not match the approved encounter total."));
            }
        }

        private static void ValidateAttackWave(
            C1Lv1EncounterAttackWaveBalance wave,
            string path,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            if (wave == null)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.AttackWaveMissing,
                    path,
                    "Attack-wave row is required."));
                return;
            }

            RequireIdentifier(wave.cadenceScope, path + ".cadenceScope", errors);
            RequireIdentifier(wave.effectRequestKey, path + ".effectRequestKey", errors);
            if (wave.firstResolveSeconds <= 0m
                || wave.intervalSeconds <= 0m
                || wave.damagePerApplication <= 0m
                || wave.applicationsPerWave <= 0)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.AttackCadenceInvalid,
                    path,
                    "Attack-wave cadence, damage, and applications must be positive."));
            }
            Match(
                wave.cadenceScope,
                C1Lv1EarlyEncounterBalanceContract.AttackCadenceScope,
                C1Lv1EarlyEncounterBalanceErrorCodes.AttackWaveValueMismatch,
                path + ".cadenceScope",
                errors);
            Match(
                wave.effectRequestKey,
                C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                C1Lv1EarlyEncounterBalanceErrorCodes.EffectRequestMismatch,
                path + ".effectRequestKey",
                errors);
            if (wave.firstResolveSeconds != C1Lv1EarlyEncounterBalanceContract.FirstResolveSeconds
                || wave.intervalSeconds != C1Lv1EarlyEncounterBalanceContract.IntervalSeconds
                || wave.damagePerApplication != C1Lv1EarlyEncounterBalanceContract.DamagePerApplication
                || wave.applicationsPerWave != C1Lv1EarlyEncounterBalanceContract.ApplicationsPerWave)
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.AttackWaveValueMismatch,
                    path,
                    "Attack-wave values do not match the approved baseline."));
            }
            if (string.IsNullOrWhiteSpace(wave.canonicalSignature))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.CanonicalSignatureMissing,
                    path + ".canonicalSignature",
                    "Attack-wave canonical signature is required."));
            }
            else if (!string.Equals(
                wave.canonicalSignature,
                C1Lv1EarlyEncounterBalanceCanonical.AttackWaveSignature(wave),
                StringComparison.Ordinal))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.CanonicalSignatureMismatch,
                    path + ".canonicalSignature",
                    "Attack-wave canonical signature does not match its fields."));
            }
        }

        private static void RequireEncounter(
            IEnumerable<C1Lv1EarlyEncounterBalanceProfile> profiles,
            string encounterVariantId,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            if (!profiles.Any(delegate(C1Lv1EarlyEncounterBalanceProfile profile)
            {
                return string.Equals(
                    profile.encounterVariantId,
                    encounterVariantId,
                    StringComparison.Ordinal);
            }))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.EncounterMissing,
                    "profiles",
                    "Missing required encounter: " + encounterVariantId));
            }
        }

        private static void RequireIdentifier(
            string value,
            string path,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add(Error(
                    C1Lv1EarlyEncounterBalanceErrorCodes.RequiredIdentifierMissing,
                    path,
                    "Required identifier is missing."));
            }
        }

        private static void Match(
            string actual,
            string expected,
            string errorCode,
            string path,
            List<C1Lv1EarlyEncounterBalanceValidationError> errors)
        {
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
            {
                errors.Add(Error(
                    errorCode,
                    path,
                    "Value does not match the approved contract."));
            }
        }

        private static C1Lv1EarlyEncounterBalanceValidationError Error(
            string errorCode,
            string path,
            string message)
        {
            return new C1Lv1EarlyEncounterBalanceValidationError(errorCode, path, message);
        }

        private static C1Lv1EarlyEncounterBalanceValidationResult Result(
            IEnumerable<C1Lv1EarlyEncounterBalanceValidationError> errors,
            string canonicalSignature)
        {
            return new C1Lv1EarlyEncounterBalanceValidationResult(errors, canonicalSignature);
        }
    }
}
