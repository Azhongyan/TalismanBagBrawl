using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Build;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.V04.RewardDrop.Contracts;

namespace TalismanBag.V04.RewardDrop.Runtime.CampaignLoot
{
    public enum C1CampaignDirectLootSessionPhase
    {
        InitialStageSequence = 1,
        RepeatChallengeOpen = 2
    }

    public static class C1CampaignDirectLootSessionDiagnosticCodes
    {
        public const string None = "NONE";
        public const string SessionInvalid = "SESSION_INVALID";
        public const string SessionCanonicalMismatch = "SESSION_CANONICAL_MISMATCH";
        public const string CompletionProofRequired = "COMPLETION_PROOF_REQUIRED";
        public const string CompletionProofInvalid = "COMPLETION_PROOF_INVALID";
        public const string CompletionProofCanonicalMismatch =
            "COMPLETION_PROOF_CANONICAL_MISMATCH";
        public const string StageClearRequired = "STAGE_CLEAR_REQUIRED";
        public const string StageClearContractInvalid = "STAGE_CLEAR_CONTRACT_INVALID";
        public const string StageClearContextMismatch = "STAGE_CLEAR_CONTEXT_MISMATCH";
        public const string StageClearSessionMismatch = "STAGE_CLEAR_SESSION_MISMATCH";
        public const string StageClearChapterMismatch = "STAGE_CLEAR_CHAPTER_MISMATCH";
        public const string StageClearStageNotEligible = "STAGE_CLEAR_STAGE_NOT_ELIGIBLE";
        public const string StageClearBossRejected = "STAGE_CLEAR_BOSS_REJECTED";
        public const string StageClearIdConflict = "STAGE_CLEAR_ID_CONFLICT";
        public const string ClearSequenceConflict = "CLEAR_SEQUENCE_CONFLICT";
        public const string ClearSequenceUnexpected = "CLEAR_SEQUENCE_UNEXPECTED";
        public const string InitialStageOrderMismatch = "INITIAL_STAGE_ORDER_MISMATCH";
        public const string PolicyRejected = "POLICY_REJECTED";
        public const string SelectionRejected = "SELECTION_REJECTED";
        public const string ItemMaterializationRejected = "ITEM_MATERIALIZATION_REJECTED";
        public const string ItemMaterializationMismatch = "ITEM_MATERIALIZATION_MISMATCH";
        public const string RewardContractRejected = "REWARD_CONTRACT_REJECTED";
    }

    public sealed class C1CampaignCompletionOnlyProgressProof
    {
        private readonly ReadOnlyCollection<string> completedStageIds;

        public C1CampaignCompletionOnlyProgressProof(
            string authorityIdentity,
            string productContext,
            string chapterId,
            IEnumerable<string> completedStageIds)
        {
            if (completedStageIds == null)
            {
                throw new ArgumentNullException(nameof(completedStageIds));
            }

            this.authorityIdentity = authorityIdentity ?? string.Empty;
            this.productContext = productContext ?? string.Empty;
            this.chapterId = chapterId ?? string.Empty;
            this.completedStageIds = Array.AsReadOnly(completedStageIds.ToArray());
            canonicalPayload = BuildCanonicalPayload();
            canonicalSignature = canonicalPayload;
        }

        public const string SchemaId = "C1CampaignCompletionOnlyProgressProof.v1";
        public const string AuthorityIdentity =
            "C1_CAMPAIGN_COMPLETION_ONLY_PROGRESS_AUTHORITY_R1";
        public const string ProductContext = C1CampaignDirectLootPoolAndPolicy.ProductContext;
        public const string ChapterId = C1CampaignDirectLootPoolAndPolicy.ChapterId;

        public string schemaId => SchemaId;
        public string authorityIdentity { get; }
        public string productContext { get; }
        public string chapterId { get; }
        public IReadOnlyList<string> CompletedStageIds => completedStageIds;
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public static C1CampaignCompletionOnlyProgressProof Create(
            IEnumerable<string> completedStageIds)
        {
            return new C1CampaignCompletionOnlyProgressProof(
                AuthorityIdentity,
                ProductContext,
                ChapterId,
                completedStageIds);
        }

        internal bool HasValidCompletionSemantics()
        {
            if (!string.Equals(authorityIdentity, AuthorityIdentity, StringComparison.Ordinal)
                || !string.Equals(productContext, ProductContext, StringComparison.Ordinal)
                || !string.Equals(chapterId, ChapterId, StringComparison.Ordinal)
                || completedStageIds.Count
                    > C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count)
            {
                return false;
            }

            for (int index = 0; index < completedStageIds.Count; index++)
            {
                if (!string.Equals(
                    completedStageIds[index],
                    C1CampaignDirectLootPoolAndPolicy.EligibleStageIds[index],
                    StringComparison.Ordinal))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsCanonicalValid()
        {
            return HasValidCompletionSemantics()
                && string.Equals(
                    canonicalPayload,
                    BuildCanonicalPayload(),
                    StringComparison.Ordinal)
                && string.Equals(
                    canonicalSignature,
                    canonicalPayload,
                    StringComparison.Ordinal);
        }

        private string BuildCanonicalPayload()
        {
            return C1CampaignDirectLootCanonical.Build(
                SchemaId,
                authorityIdentity,
                productContext,
                chapterId,
                completedStageIds.Count.ToString(CultureInfo.InvariantCulture),
                C1CampaignDirectLootCanonical.Build(completedStageIds.ToArray()));
        }
    }

    public sealed class C1CampaignDirectLootSessionSpecification
    {
        internal C1CampaignDirectLootSessionSpecification(
            string runSessionId,
            long rootSeed,
            C1CampaignCompletionOnlyProgressProof completionProof)
        {
            this.runSessionId = runSessionId ?? string.Empty;
            this.rootSeed = rootSeed;
            this.completionProof = completionProof
                ?? throw new ArgumentNullException(nameof(completionProof));
            canonicalPayload = BuildCanonicalPayload();
            canonicalSignature = canonicalPayload;
        }

        public const string SchemaId = "C1CampaignDirectLootSessionSpecification.v2";
        public string schemaId => SchemaId;
        public string productContext => C1CampaignDirectLootPoolAndPolicy.ProductContext;
        public string chapterId => C1CampaignDirectLootPoolAndPolicy.ChapterId;
        public string runSessionId { get; }
        public long rootSeed { get; }
        public C1CampaignCompletionOnlyProgressProof completionProof { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public bool IsCanonicalValid()
        {
            return !string.IsNullOrWhiteSpace(runSessionId)
                && completionProof != null
                && completionProof.IsCanonicalValid()
                && string.Equals(
                    canonicalPayload,
                    BuildCanonicalPayload(),
                    StringComparison.Ordinal)
                && string.Equals(
                    canonicalSignature,
                    canonicalPayload,
                    StringComparison.Ordinal);
        }

        private string BuildCanonicalPayload()
        {
            return C1CampaignDirectLootCanonical.Build(
                SchemaId,
                C1CampaignDirectLootPoolAndPolicy.ProductContext,
                C1CampaignDirectLootPoolAndPolicy.ChapterId,
                runSessionId,
                rootSeed.ToString(CultureInfo.InvariantCulture),
                completionProof.canonicalSignature);
        }
    }

    public sealed class C1CampaignDirectLootGrantAdmissionLineage
    {
        private readonly ReadOnlyCollection<string> preOpportunityEffectiveCompletedStageIds;

        internal C1CampaignDirectLootGrantAdmissionLineage(
            C1CampaignDirectLootSessionSpecification sessionSpecification,
            IEnumerable<string> preOpportunityCompletedStageIds,
            StageClearFact stageClearFact,
            RewardAcquisitionMode acquisitionMode)
        {
            this.sessionSpecification = sessionSpecification
                ?? throw new ArgumentNullException(nameof(sessionSpecification));
            if (preOpportunityCompletedStageIds == null)
            {
                throw new ArgumentNullException(nameof(preOpportunityCompletedStageIds));
            }
            if (stageClearFact == null)
            {
                throw new ArgumentNullException(nameof(stageClearFact));
            }

            preOpportunityEffectiveCompletedStageIds = Array.AsReadOnly(
                preOpportunityCompletedStageIds.ToArray());
            runSessionId = stageClearFact.runSessionId ?? string.Empty;
            stageClearId = stageClearFact.stageClearId ?? string.Empty;
            stageClearCanonicalSignature = stageClearFact.canonicalSignature ?? string.Empty;
            clearSequence = stageClearFact.clearSequence;
            acceptedStageId = stageClearFact.stageId ?? string.Empty;
            acceptedAcquisitionMode = acquisitionMode;
            canonicalPayload = BuildCanonicalPayload();
            canonicalSignature = canonicalPayload;
        }

        public const string SchemaId = "C1CampaignDirectLootGrantAdmissionLineage.v1";
        public string schemaId => SchemaId;
        public C1CampaignCompletionOnlyProgressProof completionProof =>
            sessionSpecification.completionProof;
        public C1CampaignDirectLootSessionSpecification sessionSpecification { get; }
        public IReadOnlyList<string> PreOpportunityEffectiveCompletedStageIds =>
            preOpportunityEffectiveCompletedStageIds;
        public string runSessionId { get; }
        public string stageClearId { get; }
        public string stageClearCanonicalSignature { get; }
        public int clearSequence { get; }
        public string acceptedStageId { get; }
        public RewardAcquisitionMode acceptedAcquisitionMode { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public bool IsCanonicalValid()
        {
            int stageIndex = IndexOfEligibleStage(acceptedStageId);
            RewardAcquisitionMode expectedMode;
            if (stageIndex >= 0 && stageIndex < preOpportunityEffectiveCompletedStageIds.Count)
            {
                expectedMode = RewardAcquisitionMode.RepeatChallengeOnline;
            }
            else if (stageIndex == preOpportunityEffectiveCompletedStageIds.Count
                && stageIndex < C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count)
            {
                expectedMode = RewardAcquisitionMode.FirstClearOnline;
            }
            else
            {
                return false;
            }

            return sessionSpecification != null
                && sessionSpecification.IsCanonicalValid()
                && completionProof != null
                && completionProof.IsCanonicalValid()
                && HasValidPreOpportunityPrefix()
                && !string.IsNullOrWhiteSpace(runSessionId)
                && string.Equals(
                    runSessionId,
                    sessionSpecification.runSessionId,
                    StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(stageClearId)
                && clearSequence > 0
                && acceptedAcquisitionMode == expectedMode
                && string.Equals(
                    canonicalPayload,
                    BuildCanonicalPayload(),
                    StringComparison.Ordinal)
                && string.Equals(
                    canonicalSignature,
                    canonicalPayload,
                    StringComparison.Ordinal);
        }

        internal bool Matches(
            C1CampaignDirectLootSessionSpecification expectedSpecification,
            IEnumerable<string> expectedPreOpportunityPrefix,
            StageClearFact expectedStageClearFact,
            RewardAcquisitionMode expectedAcquisitionMode)
        {
            return IsCanonicalValid()
                && expectedSpecification != null
                && expectedPreOpportunityPrefix != null
                && expectedStageClearFact != null
                && string.Equals(
                    sessionSpecification.canonicalPayload,
                    expectedSpecification.canonicalPayload,
                    StringComparison.Ordinal)
                && string.Equals(
                    sessionSpecification.canonicalSignature,
                    expectedSpecification.canonicalSignature,
                    StringComparison.Ordinal)
                && preOpportunityEffectiveCompletedStageIds.SequenceEqual(
                    expectedPreOpportunityPrefix,
                    StringComparer.Ordinal)
                && string.Equals(
                    runSessionId,
                    expectedStageClearFact.runSessionId,
                    StringComparison.Ordinal)
                && string.Equals(
                    stageClearId,
                    expectedStageClearFact.stageClearId,
                    StringComparison.Ordinal)
                && string.Equals(
                    stageClearCanonicalSignature,
                    expectedStageClearFact.canonicalSignature,
                    StringComparison.Ordinal)
                && clearSequence == expectedStageClearFact.clearSequence
                && string.Equals(
                    acceptedStageId,
                    expectedStageClearFact.stageId,
                    StringComparison.Ordinal)
                && acceptedAcquisitionMode == expectedAcquisitionMode;
        }

        private bool HasValidPreOpportunityPrefix()
        {
            if (completionProof == null
                || preOpportunityEffectiveCompletedStageIds.Count
                    < completionProof.CompletedStageIds.Count
                || preOpportunityEffectiveCompletedStageIds.Count
                    > C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count)
            {
                return false;
            }

            for (int index = 0;
                index < preOpportunityEffectiveCompletedStageIds.Count;
                index++)
            {
                if (!string.Equals(
                    preOpportunityEffectiveCompletedStageIds[index],
                    C1CampaignDirectLootPoolAndPolicy.EligibleStageIds[index],
                    StringComparison.Ordinal))
                {
                    return false;
                }
                if (index < completionProof.CompletedStageIds.Count
                    && !string.Equals(
                        preOpportunityEffectiveCompletedStageIds[index],
                        completionProof.CompletedStageIds[index],
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }
            return true;
        }

        private string BuildCanonicalPayload()
        {
            return C1CampaignDirectLootCanonical.Build(
                SchemaId,
                completionProof == null ? string.Empty : completionProof.canonicalSignature,
                sessionSpecification == null
                    ? string.Empty
                    : sessionSpecification.canonicalSignature,
                preOpportunityEffectiveCompletedStageIds.Count.ToString(
                    CultureInfo.InvariantCulture),
                C1CampaignDirectLootCanonical.Build(
                    preOpportunityEffectiveCompletedStageIds.ToArray()),
                runSessionId,
                stageClearId,
                stageClearCanonicalSignature,
                clearSequence.ToString(CultureInfo.InvariantCulture),
                acceptedStageId,
                ((int)acceptedAcquisitionMode).ToString(CultureInfo.InvariantCulture));
        }

        private static int IndexOfEligibleStage(string stageId)
        {
            for (int index = 0;
                index < C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count;
                index++)
            {
                if (string.Equals(
                    C1CampaignDirectLootPoolAndPolicy.EligibleStageIds[index],
                    stageId,
                    StringComparison.Ordinal))
                {
                    return index;
                }
            }
            return -1;
        }
    }

    public sealed class C1CampaignDirectLootGrantBundle
    {
        internal C1CampaignDirectLootGrantBundle(
            C1CampaignDirectLootGrantAdmissionLineage admissionLineage,
            long acceptedDropOrdinal,
            string authoritativeRewardGrantIdentity,
            string selectedBaseItemId,
            StageClearFact stageClearFact,
            DropRequest dropRequest,
            DropRollResult dropRollResult,
            RewardResult rewardResult,
            RewardClaimIdentity claimIdentity,
            RewardDedupeIdentity dedupeIdentity,
            ItemGeneratedInstanceSnapshot generatedItem,
            CanonicalItemDefinition itemDefinition)
        {
            this.admissionLineage = admissionLineage
                ?? throw new ArgumentNullException(nameof(admissionLineage));
            this.acceptedDropOrdinal = acceptedDropOrdinal;
            this.authoritativeRewardGrantIdentity = authoritativeRewardGrantIdentity ?? string.Empty;
            this.selectedBaseItemId = selectedBaseItemId ?? string.Empty;
            this.stageClearFact = stageClearFact;
            this.dropRequest = dropRequest;
            this.dropRollResult = dropRollResult;
            this.rewardResult = rewardResult;
            this.claimIdentity = claimIdentity;
            this.dedupeIdentity = dedupeIdentity;
            this.generatedItem = generatedItem;
            this.itemDefinition = itemDefinition;
            canonicalPayload = BuildCanonicalPayload();
            canonicalSignature = canonicalPayload;
        }

        public const string SchemaId = "C1CampaignDirectLootGrantBundle.v3";
        public string schemaId => SchemaId;
        public C1CampaignDirectLootGrantAdmissionLineage admissionLineage { get; }
        public long acceptedDropOrdinal { get; }
        public string authoritativeRewardGrantIdentity { get; }
        public string selectedBaseItemId { get; }
        public StageClearFact stageClearFact { get; }
        public DropRequest dropRequest { get; }
        public DropRollResult dropRollResult { get; }
        public RewardResult rewardResult { get; }
        public RewardClaimIdentity claimIdentity { get; }
        public RewardDedupeIdentity dedupeIdentity { get; }
        public ItemGeneratedInstanceSnapshot generatedItem { get; }
        public CanonicalItemDefinition itemDefinition { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public bool IsCanonicalValidV2()
        {
            return acceptedDropOrdinal > 0
                && admissionLineage != null
                && admissionLineage.IsCanonicalValid()
                && !string.IsNullOrWhiteSpace(authoritativeRewardGrantIdentity)
                && !string.IsNullOrWhiteSpace(selectedBaseItemId)
                && stageClearFact != null
                && dropRequest != null
                && dropRollResult != null
                && rewardResult != null
                && claimIdentity != null
                && dedupeIdentity != null
                && generatedItem != null
                && itemDefinition != null
                && string.Equals(selectedBaseItemId, generatedItem.baseItemId,
                    StringComparison.Ordinal)
                && string.Equals(selectedBaseItemId, itemDefinition.baseItemId,
                    StringComparison.Ordinal)
                && acceptedDropOrdinal == stageClearFact.clearSequence
                && admissionLineage.Matches(
                    admissionLineage.sessionSpecification,
                    admissionLineage.PreOpportunityEffectiveCompletedStageIds,
                    stageClearFact,
                    dropRequest.acquisitionMode)
                && dropRollResult.acquisitionMode == admissionLineage.acceptedAcquisitionMode
                && rewardResult.acquisitionMode == admissionLineage.acceptedAcquisitionMode
                && claimIdentity.acquisitionMode == admissionLineage.acceptedAcquisitionMode
                && dedupeIdentity.acquisitionMode == admissionLineage.acceptedAcquisitionMode
                && RewardContractsAreValid()
                && string.Equals(
                    canonicalPayload,
                    BuildCanonicalPayload(),
                    StringComparison.Ordinal)
                && string.Equals(
                    canonicalSignature,
                    canonicalPayload,
                    StringComparison.Ordinal);
        }

        internal bool IsCanonicalValid()
        {
            return IsCanonicalValidV2();
        }

        private bool RewardContractsAreValid()
        {
            RewardDropValidationResult stageValidation =
                RewardDropContractValidator.ValidateStageClearFact(stageClearFact);
            RewardDropValidationResult requestValidation =
                RewardDropContractValidator.ValidateDropRequest(dropRequest, stageClearFact);
            RewardDropValidationResult rollValidation =
                RewardDropContractValidator.ValidateDropRollResult(
                    dropRollResult,
                    dropRequest,
                    stageClearFact);
            RewardDropValidationResult rewardValidation =
                RewardDropContractValidator.ValidateRewardResult(
                    rewardResult,
                    dropRequest,
                    dropRollResult,
                    stageClearFact);
            RewardDropValidationResult claimValidation =
                RewardDropContractValidator.ValidateClaimIdentity(claimIdentity, rewardResult);
            RewardDropValidationResult dedupeValidation =
                RewardDropContractValidator.ValidateDedupeIdentity(dedupeIdentity, rewardResult);
            return stageValidation != null && stageValidation.isValid
                && requestValidation != null && requestValidation.isValid
                && rollValidation != null && rollValidation.isValid
                && rewardValidation != null && rewardValidation.isValid
                && claimValidation != null && claimValidation.isValid
                && dedupeValidation != null && dedupeValidation.isValid;
        }

        private string BuildCanonicalPayload()
        {
            return C1CampaignDirectLootCanonical.Build(
                SchemaId,
                admissionLineage == null ? string.Empty : admissionLineage.canonicalSignature,
                acceptedDropOrdinal.ToString(CultureInfo.InvariantCulture),
                authoritativeRewardGrantIdentity,
                selectedBaseItemId,
                stageClearFact == null ? string.Empty : stageClearFact.canonicalSignature,
                dropRequest == null ? string.Empty : dropRequest.canonicalSignature,
                dropRollResult == null ? string.Empty : dropRollResult.canonicalSignature,
                rewardResult == null ? string.Empty : rewardResult.canonicalSignature,
                claimIdentity == null ? string.Empty : claimIdentity.canonicalSignature,
                dedupeIdentity == null ? string.Empty : dedupeIdentity.canonicalSignature,
                generatedItem == null
                    ? string.Empty
                    : generatedItem.BuildCanonicalSignature(),
                itemDefinition == null
                    ? string.Empty
                    : itemDefinition.baseItemId);
        }
    }

    public sealed class C1CampaignDirectLootAcceptedOpportunity
    {
        internal C1CampaignDirectLootAcceptedOpportunity(C1CampaignDirectLootGrantBundle grant)
        {
            this.grant = grant ?? throw new ArgumentNullException(nameof(grant));
            stageClearId = grant.stageClearFact.stageClearId;
            clearSequence = grant.stageClearFact.clearSequence;
            stageClearCanonicalPayload = grant.stageClearFact.canonicalPayload;
            stageClearCanonicalSignature = grant.stageClearFact.canonicalSignature;
            canonicalPayload = C1CampaignDirectLootCanonical.Build(
                SchemaId,
                stageClearId,
                clearSequence.ToString(CultureInfo.InvariantCulture),
                stageClearCanonicalPayload,
                stageClearCanonicalSignature,
                grant.canonicalSignature);
            canonicalSignature = canonicalPayload;
        }

        public const string SchemaId = "C1CampaignDirectLootAcceptedOpportunity.v1";
        public string schemaId => SchemaId;
        public string stageClearId { get; }
        public int clearSequence { get; }
        public string stageClearCanonicalPayload { get; }
        public string stageClearCanonicalSignature { get; }
        public C1CampaignDirectLootGrantBundle grant { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }
    }

    public sealed class C1CampaignDirectLootSessionSnapshot
    {
        private readonly ReadOnlyCollection<string> recentBaseItemIds;
        private readonly ReadOnlyCollection<C1CampaignDirectLootAcceptedOpportunity> acceptedOpportunities;
        private readonly ReadOnlyCollection<string> effectiveCompletedStageIds;

        internal C1CampaignDirectLootSessionSnapshot(
            C1CampaignDirectLootSessionSpecification specification,
            int nextExpectedClearSequence,
            IEnumerable<string> recentItems,
            IEnumerable<C1CampaignDirectLootAcceptedOpportunity> opportunities)
        {
            this.specification = specification ?? throw new ArgumentNullException(nameof(specification));
            this.nextExpectedClearSequence = nextExpectedClearSequence;
            recentBaseItemIds = Array.AsReadOnly((recentItems ?? Enumerable.Empty<string>()).ToArray());
            acceptedOpportunities = Array.AsReadOnly(
                (opportunities ?? Enumerable.Empty<C1CampaignDirectLootAcceptedOpportunity>()).ToArray());
            string[] derivedCompletedStageIds;
            if (!TryBuildEffectiveCompletedStageIds(
                specification,
                acceptedOpportunities,
                out derivedCompletedStageIds))
            {
                derivedCompletedStageIds = Array.Empty<string>();
            }
            effectiveCompletedStageIds = Array.AsReadOnly(derivedCompletedStageIds);
            phase = effectiveCompletedStageIds.Count
                >= C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count
                    ? C1CampaignDirectLootSessionPhase.RepeatChallengeOpen
                    : C1CampaignDirectLootSessionPhase.InitialStageSequence;
            canonicalPayload = BuildCanonicalPayload();
            canonicalSignature = canonicalPayload;
        }

        public const string SchemaId = "C1CampaignDirectLootSessionSnapshot.v2";
        public string schemaId => SchemaId;
        public C1CampaignDirectLootSessionSpecification specification { get; }
        public C1CampaignCompletionOnlyProgressProof completionProof =>
            specification.completionProof;
        public string productContext => specification.productContext;
        public string chapterId => specification.chapterId;
        public string runSessionId => specification.runSessionId;
        public long rootSeed => specification.rootSeed;
        public int nextExpectedClearSequence { get; }
        public long acceptedDropCount => acceptedOpportunities.Count;
        public bool initialStageSequenceComplete => effectiveCompletedStageIds.Count
            >= C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count;
        public C1CampaignDirectLootSessionPhase phase { get; }
        public IReadOnlyList<string> RecentBaseItemIds => recentBaseItemIds;
        public IReadOnlyList<C1CampaignDirectLootAcceptedOpportunity> AcceptedOpportunities =>
            acceptedOpportunities;
        public IReadOnlyList<string> EffectiveCompletedStageIds => effectiveCompletedStageIds;
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public C1CampaignDirectLootAcceptedOpportunity FindByStageClearId(string stageClearId)
        {
            return acceptedOpportunities.FirstOrDefault(row =>
                row != null && string.Equals(row.stageClearId, stageClearId, StringComparison.Ordinal));
        }

        public C1CampaignDirectLootAcceptedOpportunity FindByClearSequence(int clearSequence)
        {
            return acceptedOpportunities.FirstOrDefault(row =>
                row != null && row.clearSequence == clearSequence);
        }

        internal bool IsCanonicalValid()
        {
            string[] derivedCompletedStageIds;
            C1CampaignDirectLootSessionPhase expectedPhase = effectiveCompletedStageIds.Count
                >= C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count
                    ? C1CampaignDirectLootSessionPhase.RepeatChallengeOpen
                    : C1CampaignDirectLootSessionPhase.InitialStageSequence;
            return specification.IsCanonicalValid()
                && TryBuildEffectiveCompletedStageIds(
                    specification,
                    acceptedOpportunities,
                    out derivedCompletedStageIds)
                && effectiveCompletedStageIds.SequenceEqual(
                    derivedCompletedStageIds,
                    StringComparer.Ordinal)
                && nextExpectedClearSequence == acceptedOpportunities.Count + 1
                && phase == expectedPhase
                && recentBaseItemIds.Count <= C1CampaignDirectLootPoolAndPolicy.RecentExclusionLength
                && recentBaseItemIds.Distinct(StringComparer.Ordinal).Count() == recentBaseItemIds.Count
                && acceptedOpportunities.Select((row, index) => new { row, index }).All(value =>
                    value.row != null
                    && value.row.clearSequence == value.index + 1
                    && value.row.grant != null
                    && value.row.grant.IsCanonicalValid())
                && string.Equals(
                    canonicalPayload,
                    BuildCanonicalPayload(),
                    StringComparison.Ordinal)
                && string.Equals(
                    canonicalSignature,
                    canonicalPayload,
                    StringComparison.Ordinal);
        }

        private string BuildCanonicalPayload()
        {
            return C1CampaignDirectLootCanonical.Build(
                SchemaId,
                specification.canonicalSignature,
                completionProof.canonicalSignature,
                nextExpectedClearSequence.ToString(CultureInfo.InvariantCulture),
                ((int)phase).ToString(CultureInfo.InvariantCulture),
                C1CampaignDirectLootCanonical.Build(effectiveCompletedStageIds.ToArray()),
                string.Join(",", recentBaseItemIds),
                string.Join(",", acceptedOpportunities.Select(row =>
                    row == null ? string.Empty : row.canonicalSignature)));
        }

        private static bool TryBuildEffectiveCompletedStageIds(
            C1CampaignDirectLootSessionSpecification specification,
            IEnumerable<C1CampaignDirectLootAcceptedOpportunity> opportunities,
            out string[] completedStageIds)
        {
            completedStageIds = Array.Empty<string>();
            if (specification == null || !specification.IsCanonicalValid())
            {
                return false;
            }

            C1CampaignCompletionOnlyProgressProof proof = specification.completionProof;
            List<string> completed = proof.CompletedStageIds.ToList();
            foreach (C1CampaignDirectLootAcceptedOpportunity opportunity in
                opportunities ?? Enumerable.Empty<C1CampaignDirectLootAcceptedOpportunity>())
            {
                if (opportunity == null
                    || opportunity.grant == null
                    || opportunity.grant.stageClearFact == null
                    || opportunity.grant.dropRequest == null
                    || !opportunity.grant.IsCanonicalValidV2())
                {
                    return false;
                }

                string stageId = opportunity.grant.stageClearFact.stageId;
                int stageIndex = IndexOfEligibleStage(stageId);
                bool isReplay = stageIndex >= 0 && stageIndex < completed.Count;
                if (stageIndex < 0 || stageIndex > completed.Count)
                {
                    return false;
                }
                if (!opportunity.grant.admissionLineage.Matches(
                    specification,
                    completed,
                    opportunity.grant.stageClearFact,
                    opportunity.grant.dropRequest.acquisitionMode))
                {
                    return false;
                }
                if (isReplay)
                {
                    if (opportunity.grant.dropRequest.acquisitionMode
                        != RewardAcquisitionMode.RepeatChallengeOnline)
                    {
                        return false;
                    }
                }
                else
                {
                    if (opportunity.grant.dropRequest.acquisitionMode
                        != RewardAcquisitionMode.FirstClearOnline)
                    {
                        return false;
                    }
                    completed.Add(stageId);
                }
            }

            completedStageIds = completed.ToArray();
            return true;
        }

        private static int IndexOfEligibleStage(string stageId)
        {
            for (int index = 0;
                index < C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count;
                index++)
            {
                if (string.Equals(
                    C1CampaignDirectLootPoolAndPolicy.EligibleStageIds[index],
                    stageId,
                    StringComparison.Ordinal))
                {
                    return index;
                }
            }
            return -1;
        }
    }

    public sealed class C1CampaignDirectLootSessionCreateResult
    {
        private C1CampaignDirectLootSessionCreateResult(
            C1CampaignDirectLootSessionSnapshot snapshot,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.snapshot = snapshot;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool accepted => snapshot != null
            && string.Equals(diagnosticCode, C1CampaignDirectLootSessionDiagnosticCodes.None, StringComparison.Ordinal);
        public C1CampaignDirectLootSessionSnapshot snapshot { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }

        internal static C1CampaignDirectLootSessionCreateResult Success(
            C1CampaignDirectLootSessionSnapshot snapshot)
        {
            return new C1CampaignDirectLootSessionCreateResult(
                snapshot,
                C1CampaignDirectLootSessionDiagnosticCodes.None,
                string.Empty);
        }

        internal static C1CampaignDirectLootSessionCreateResult Failure(string code, string message)
        {
            return new C1CampaignDirectLootSessionCreateResult(null, code, message);
        }
    }

    public sealed class C1CampaignDirectLootResolveResult
    {
        private C1CampaignDirectLootResolveResult(
            bool accepted,
            bool isNewGrant,
            bool isIdempotentRetry,
            C1CampaignDirectLootSessionSnapshot snapshot,
            C1CampaignDirectLootGrantBundle grant,
            string diagnosticCode,
            string diagnosticMessage)
        {
            this.accepted = accepted;
            this.isNewGrant = isNewGrant;
            this.isIdempotentRetry = isIdempotentRetry;
            this.snapshot = snapshot;
            this.grant = grant;
            this.diagnosticCode = diagnosticCode ?? string.Empty;
            this.diagnosticMessage = diagnosticMessage ?? string.Empty;
        }

        public bool accepted { get; }
        public bool isNewGrant { get; }
        public bool isIdempotentRetry { get; }
        public C1CampaignDirectLootSessionSnapshot snapshot { get; }
        public C1CampaignDirectLootGrantBundle grant { get; }
        public string diagnosticCode { get; }
        public string diagnosticMessage { get; }

        internal static C1CampaignDirectLootResolveResult NewGrant(
            C1CampaignDirectLootSessionSnapshot snapshot,
            C1CampaignDirectLootGrantBundle grant)
        {
            return new C1CampaignDirectLootResolveResult(
                true,
                true,
                false,
                snapshot,
                grant,
                C1CampaignDirectLootSessionDiagnosticCodes.None,
                string.Empty);
        }

        internal static C1CampaignDirectLootResolveResult Retry(
            C1CampaignDirectLootSessionSnapshot snapshot,
            C1CampaignDirectLootGrantBundle grant)
        {
            return new C1CampaignDirectLootResolveResult(
                true,
                false,
                true,
                snapshot,
                grant,
                C1CampaignDirectLootSessionDiagnosticCodes.None,
                string.Empty);
        }

        internal static C1CampaignDirectLootResolveResult Rejected(
            C1CampaignDirectLootSessionSnapshot unchangedSnapshot,
            string code,
            string message)
        {
            return new C1CampaignDirectLootResolveResult(
                false,
                false,
                false,
                unchangedSnapshot,
                null,
                code,
                message);
        }
    }

    public static class C1CampaignDirectLootSessionResolver
    {
        public static C1CampaignDirectLootSessionCreateResult CreateSession(
            string runSessionId,
            long rootSeed)
        {
            return CreateSession(
                runSessionId,
                rootSeed,
                C1CampaignCompletionOnlyProgressProof.Create(Array.Empty<string>()));
        }

        public static C1CampaignDirectLootSessionCreateResult CreateSession(
            string runSessionId,
            long rootSeed,
            C1CampaignCompletionOnlyProgressProof completionProof)
        {
            if (string.IsNullOrWhiteSpace(runSessionId))
            {
                return C1CampaignDirectLootSessionCreateResult.Failure(
                    C1CampaignDirectLootSessionDiagnosticCodes.SessionInvalid,
                    "runSessionId is required.");
            }
            if (completionProof == null)
            {
                return C1CampaignDirectLootSessionCreateResult.Failure(
                    C1CampaignDirectLootSessionDiagnosticCodes.CompletionProofRequired,
                    "A completion-only progress proof is required.");
            }
            if (!completionProof.HasValidCompletionSemantics())
            {
                return C1CampaignDirectLootSessionCreateResult.Failure(
                    C1CampaignDirectLootSessionDiagnosticCodes.CompletionProofInvalid,
                    "Completion proof authority, lineage, or contiguous prefix is invalid.");
            }
            if (!completionProof.IsCanonicalValid())
            {
                return C1CampaignDirectLootSessionCreateResult.Failure(
                    C1CampaignDirectLootSessionDiagnosticCodes.CompletionProofCanonicalMismatch,
                    "Completion proof canonical validation failed.");
            }

            C1CampaignDirectLootSessionSpecification specification =
                new C1CampaignDirectLootSessionSpecification(
                    runSessionId,
                    rootSeed,
                    completionProof);
            C1CampaignDirectLootSessionSnapshot snapshot = new C1CampaignDirectLootSessionSnapshot(
                specification,
                1,
                Array.Empty<string>(),
                Array.Empty<C1CampaignDirectLootAcceptedOpportunity>());
            return snapshot.IsCanonicalValid()
                ? C1CampaignDirectLootSessionCreateResult.Success(snapshot)
                : C1CampaignDirectLootSessionCreateResult.Failure(
                    C1CampaignDirectLootSessionDiagnosticCodes.SessionCanonicalMismatch,
                    "Initial session canonical validation failed.");
        }

        public static C1CampaignDirectLootResolveResult Resolve(
            C1CampaignDirectLootSessionSnapshot snapshot,
            StageClearFact stageClearFact,
            CanonicalItemDefinitionResolver itemResolver,
            IReadOnlyDictionary<string, int> currentRunCopies,
            ItemBuildSynergyResolutionResult currentBuild)
        {
            if (snapshot == null)
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    null,
                    C1CampaignDirectLootSessionDiagnosticCodes.SessionInvalid,
                    "Session snapshot is required.");
            }
            if (!snapshot.IsCanonicalValid())
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.SessionCanonicalMismatch,
                    "Session canonical validation failed.");
            }
            if (stageClearFact == null)
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.StageClearRequired,
                    "StageClearFact is required.");
            }

            C1CampaignDirectLootAcceptedOpportunity existingById =
                snapshot.FindByStageClearId(stageClearFact.stageClearId);
            if (existingById != null)
            {
                bool byteIdentical = string.Equals(
                        existingById.stageClearCanonicalPayload,
                        stageClearFact.canonicalPayload,
                        StringComparison.Ordinal)
                    && string.Equals(
                        existingById.stageClearCanonicalSignature,
                        stageClearFact.canonicalSignature,
                        StringComparison.Ordinal);
                return byteIdentical
                    ? C1CampaignDirectLootResolveResult.Retry(snapshot, existingById.grant)
                    : C1CampaignDirectLootResolveResult.Rejected(
                        snapshot,
                        C1CampaignDirectLootSessionDiagnosticCodes.StageClearIdConflict,
                        "stageClearId was already accepted with different bytes.");
            }

            C1CampaignDirectLootAcceptedOpportunity existingBySequence =
                snapshot.FindByClearSequence(stageClearFact.clearSequence);
            if (existingBySequence != null)
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.ClearSequenceConflict,
                    "clearSequence was already accepted by another StageClearFact.");
            }

            RewardDropValidationResult stageClearValidation =
                RewardDropContractValidator.ValidateStageClearFact(stageClearFact);
            if (stageClearValidation == null || !stageClearValidation.isValid)
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.StageClearContractInvalid,
                    "StageClearFact failed the accepted Reward contract validator.");
            }
            if (stageClearFact.rewardLaunchContextIdentity == null
                || !string.Equals(
                    stageClearFact.rewardLaunchContextIdentity.launchContextId,
                    C1CampaignDirectLootPoolAndPolicy.ProductContext,
                    StringComparison.Ordinal))
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.StageClearContextMismatch,
                    "Only CAMPAIGN_NORMAL_LV1 is eligible.");
            }
            if (!string.Equals(stageClearFact.runSessionId, snapshot.runSessionId, StringComparison.Ordinal))
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.StageClearSessionMismatch,
                    "StageClearFact runSessionId differs from the session.");
            }
            if (!string.Equals(
                stageClearFact.chapterId,
                C1CampaignDirectLootPoolAndPolicy.ChapterId,
                StringComparison.Ordinal))
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.StageClearChapterMismatch,
                    "Only bone_aspect_chapter_1 is eligible.");
            }
            if (stageClearFact.isBossStage)
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.StageClearBossRejected,
                    "Boss StageClear is outside this ordinary campaign policy.");
            }
            if (!C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Contains(
                stageClearFact.stageId,
                StringComparer.Ordinal))
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.StageClearStageNotEligible,
                    "Only stages 1-1 through 1-5 are eligible.");
            }
            if (stageClearFact.clearSequence != snapshot.nextExpectedClearSequence)
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.ClearSequenceUnexpected,
                    "StageClearFact clearSequence is not the next accepted sequence.");
            }
            bool stageAlreadyCompleted = snapshot.EffectiveCompletedStageIds.Contains(
                stageClearFact.stageId,
                StringComparer.Ordinal);
            if (!stageAlreadyCompleted
                && (snapshot.EffectiveCompletedStageIds.Count
                    >= C1CampaignDirectLootPoolAndPolicy.EligibleStageIds.Count
                    || !string.Equals(
                        stageClearFact.stageId,
                        C1CampaignDirectLootPoolAndPolicy.EligibleStageIds[
                            snapshot.EffectiveCompletedStageIds.Count],
                        StringComparison.Ordinal)))
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.InitialStageOrderMismatch,
                    "A first clear must be the first missing stage in the completion prefix.");
            }

            RewardAcquisitionMode acquisitionMode = stageAlreadyCompleted
                ? RewardAcquisitionMode.RepeatChallengeOnline
                : RewardAcquisitionMode.FirstClearOnline;
            long acceptedDropOrdinal = snapshot.acceptedDropCount + 1;
            string rewardKey = string.Join(".", snapshot.runSessionId,
                acceptedDropOrdinal.ToString(CultureInfo.InvariantCulture),
                stageClearFact.stageId);
            string requestId = "c1.canonical.drop." + rewardKey;
            string itemInstanceId = "c1.item." + rewardKey;
            CanonicalItemDropResult selection =
                C1CampaignDirectLootPoolAndPolicy.SelectCanonical(
                    itemResolver,
                    requestId,
                    itemInstanceId,
                    stageClearFact.stageId,
                    (int)acceptedDropOrdinal,
                    snapshot.rootSeed,
                    currentRunCopies,
                    currentBuild);
            if (selection == null || !selection.isSuccess
                || selection.Definition == null || selection.ItemRoll?.snapshot == null)
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.SelectionRejected,
                    selection == null
                        ? "Canonical Item selection returned no result."
                        : string.Join(",", selection.Errors));
            }

            C1CampaignDirectLootGrantBundle grant;
            string buildDiagnostic;
            if (!TryBuildAndValidateGrant(
                snapshot,
                stageClearFact,
                acquisitionMode,
                acceptedDropOrdinal,
                selection,
                requestId,
                out grant,
                out buildDiagnostic))
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    string.IsNullOrEmpty(buildDiagnostic)
                        ? C1CampaignDirectLootSessionDiagnosticCodes.RewardContractRejected
                        : buildDiagnostic,
                    "Grant construction or correlation validation failed.");
            }

            List<C1CampaignDirectLootAcceptedOpportunity> opportunities =
                snapshot.AcceptedOpportunities.ToList();
            opportunities.Add(new C1CampaignDirectLootAcceptedOpportunity(grant));
            List<string> recent = snapshot.RecentBaseItemIds.ToList();
            recent.Add(grant.selectedBaseItemId);
            if (recent.Count > C1CampaignDirectLootPoolAndPolicy.RecentExclusionLength)
            {
                recent.RemoveAt(0);
            }

            C1CampaignDirectLootSessionSnapshot advanced = new C1CampaignDirectLootSessionSnapshot(
                snapshot.specification,
                snapshot.nextExpectedClearSequence + 1,
                recent,
                opportunities);
            if (!advanced.IsCanonicalValid())
            {
                return C1CampaignDirectLootResolveResult.Rejected(
                    snapshot,
                    C1CampaignDirectLootSessionDiagnosticCodes.SessionCanonicalMismatch,
                    "Advanced session canonical validation failed.");
            }
            return C1CampaignDirectLootResolveResult.NewGrant(advanced, grant);
        }

        private static bool TryBuildAndValidateGrant(
            C1CampaignDirectLootSessionSnapshot snapshot,
            StageClearFact stageClearFact,
            RewardAcquisitionMode acquisitionMode,
            long acceptedDropOrdinal,
            CanonicalItemDropResult selection,
            string requestId,
            out C1CampaignDirectLootGrantBundle grant,
            out string diagnosticCode)
        {
            grant = null;
            diagnosticCode = string.Empty;
            C1CampaignDirectLootGrantAdmissionLineage admissionLineage =
                new C1CampaignDirectLootGrantAdmissionLineage(
                    snapshot.specification,
                    snapshot.EffectiveCompletedStageIds,
                    stageClearFact,
                    acquisitionMode);
            if (!admissionLineage.IsCanonicalValid())
            {
                diagnosticCode = C1CampaignDirectLootSessionDiagnosticCodes
                    .RewardContractRejected;
                return false;
            }
            ItemGeneratedInstanceSnapshot item = selection.ItemRoll.snapshot;
            CanonicalItemDefinition definition = selection.Definition;
            string lineage = string.Join(".", snapshot.runSessionId,
                acceptedDropOrdinal.ToString(CultureInfo.InvariantCulture),
                stageClearFact.stageId,
                item.itemInstanceId);
            DropRequest request = new DropRequest(
                requestId,
                RewardSourceFactKind.StageClear,
                stageClearFact.stageClearId,
                stageClearFact.stageClearId,
                stageClearFact.chapterId,
                stageClearFact.stageId,
                stageClearFact.rewardLaunchContextIdentity,
                acquisitionMode,
                snapshot.rootSeed,
                item.generationVersion,
                C1CampaignDirectLootPoolAndPolicy.CanonicalPoolId,
                C1CampaignDirectLootPoolAndPolicy.CanonicalPoolVersion,
                C1CampaignDirectLootPoolAndPolicy.CanonicalRarityProfileId,
                C1CampaignDirectLootPoolAndPolicy.CanonicalAttributeProfileId,
                C1CampaignDirectLootPoolAndPolicy.CanonicalRollPolicyVersion,
                new[]
                {
                    new DropRollSlot(
                        C1CampaignDirectLootPoolAndPolicy.RollSlotId,
                        RewardGrantKind.OrdinaryRoll)
                });

            string rewardGrantIdentity = "c1.reward." + lineage;
            string instanceSignature = item.BuildCanonicalSignature();

            RewardSubjectIdentity subject = new RewardSubjectIdentity(
                item.baseItemId + "@" + item.rarity.ToStableKey(),
                RewardSubjectKind.OrdinaryGeneratedItem,
                C1CampaignDirectLootPoolAndPolicy.CanonicalPoolId,
                C1CampaignDirectLootPoolAndPolicy.SourceRuleVersion);
            string rollEntryId = StableId("c1-roll-entry", lineage);
            DropRollEntry rollEntry = new DropRollEntry(
                rollEntryId,
                C1CampaignDirectLootPoolAndPolicy.RollSlotId,
                RewardGrantKind.OrdinaryRoll,
                subject,
                item.itemInstanceId,
                instanceSignature,
                C1CampaignDirectLootPoolAndPolicy.GrantQuantity);
            DropRollResult rollResult = new DropRollResult(
                StableId("c1-drop-roll-result", lineage),
                request.dropRequestId,
                stageClearFact.stageClearId,
                stageClearFact.stageClearId,
                stageClearFact.chapterId,
                stageClearFact.stageId,
                stageClearFact.rewardLaunchContextIdentity,
                acquisitionMode,
                snapshot.rootSeed,
                item.generationVersion,
                C1CampaignDirectLootPoolAndPolicy.CanonicalAlgorithmId,
                C1CampaignDirectLootPoolAndPolicy.CanonicalPoolId,
                C1CampaignDirectLootPoolAndPolicy.CanonicalPoolVersion,
                C1CampaignDirectLootPoolAndPolicy.CanonicalRarityProfileId,
                C1CampaignDirectLootPoolAndPolicy.CanonicalAttributeProfileId,
                C1CampaignDirectLootPoolAndPolicy.CanonicalRollPolicyVersion,
                new[] { rollEntry });

            RewardEntry rewardEntry = new RewardEntry(
                StableId("c1-reward-entry", lineage),
                RewardGrantKind.OrdinaryRoll,
                subject,
                rollEntry.rollEntryId,
                C1CampaignDirectLootPoolAndPolicy.SourceRuleVersion,
                item.itemInstanceId,
                instanceSignature,
                C1CampaignDirectLootPoolAndPolicy.GrantQuantity);
            RewardResult rewardResult = new RewardResult(
                StableId("c1-reward-result", lineage),
                stageClearFact.stageClearId,
                stageClearFact.stageClearId,
                request.dropRequestId,
                rollResult.dropRollResultId,
                stageClearFact.chapterId,
                stageClearFact.stageId,
                stageClearFact.rewardLaunchContextIdentity,
                acquisitionMode,
                new[] { rewardEntry });

            string claimScope = StableId(
                "c1-claim-scope",
                string.Join(".",
                    snapshot.runSessionId,
                    stageClearFact.chapterId,
                    stageClearFact.stageId,
                    stageClearFact.clearSequence.ToString(CultureInfo.InvariantCulture),
                    ((int)acquisitionMode).ToString(CultureInfo.InvariantCulture)));
            RewardClaimIdentity claim = new RewardClaimIdentity(
                StableId("c1-claim", lineage),
                rewardResult.rewardResultId,
                stageClearFact.stageClearId,
                stageClearFact.stageClearId,
                request.dropRequestId,
                acquisitionMode,
                claimScope,
                stageClearFact.rewardLaunchContextIdentity);
            RewardDedupeIdentity dedupe = new RewardDedupeIdentity(
                StableId("c1-dedupe", lineage),
                stageClearFact.stageClearId,
                stageClearFact.stageClearId,
                request.dropRequestId,
                rewardResult.rewardResultId,
                acquisitionMode,
                claimScope,
                stageClearFact.rewardLaunchContextIdentity);

            if (!AllContractsValid(stageClearFact, request, rollResult, rewardResult, claim, dedupe))
            {
                diagnosticCode = C1CampaignDirectLootSessionDiagnosticCodes.RewardContractRejected;
                return false;
            }

            grant = new C1CampaignDirectLootGrantBundle(
                admissionLineage,
                acceptedDropOrdinal,
                rewardGrantIdentity,
                item.baseItemId,
                stageClearFact,
                request,
                rollResult,
                rewardResult,
                claim,
                dedupe,
                item,
                definition);
            return grant.IsCanonicalValid();
        }

        private static bool AllContractsValid(
            StageClearFact stageClearFact,
            DropRequest request,
            DropRollResult rollResult,
            RewardResult rewardResult,
            RewardClaimIdentity claim,
            RewardDedupeIdentity dedupe)
        {
            RewardDropValidationResult stageValidation =
                RewardDropContractValidator.ValidateStageClearFact(stageClearFact);
            RewardDropValidationResult requestValidation =
                RewardDropContractValidator.ValidateDropRequest(request, stageClearFact);
            RewardDropValidationResult rollValidation =
                RewardDropContractValidator.ValidateDropRollResult(rollResult, request, stageClearFact);
            RewardDropValidationResult rewardValidation =
                RewardDropContractValidator.ValidateRewardResult(
                    rewardResult,
                    request,
                    rollResult,
                    stageClearFact);
            RewardDropValidationResult claimValidation =
                RewardDropContractValidator.ValidateClaimIdentity(claim, rewardResult);
            RewardDropValidationResult dedupeValidation =
                RewardDropContractValidator.ValidateDedupeIdentity(dedupe, rewardResult);
            return stageValidation != null && stageValidation.isValid
                && requestValidation != null && requestValidation.isValid
                && rollValidation != null && rollValidation.isValid
                && rewardValidation != null && rewardValidation.isValid
                && claimValidation != null && claimValidation.isValid
                && dedupeValidation != null && dedupeValidation.isValid;
        }

        private static string StableId(string prefix, string payload)
        {
            return prefix + "." + (payload ?? string.Empty);
        }
    }
}
