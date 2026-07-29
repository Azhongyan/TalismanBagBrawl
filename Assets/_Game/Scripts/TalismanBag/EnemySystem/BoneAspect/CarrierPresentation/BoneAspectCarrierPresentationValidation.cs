using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.CarrierPresentation
{
    public sealed class BoneAspectCarrierPresentationCatalogValidator
    {
        public static readonly BoneAspectCarrierPresentationCatalogValidator Instance =
            new BoneAspectCarrierPresentationCatalogValidator();

        public IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> Validate(
            BoneAspectCarrierPresentationCatalogInput input,
            EnemyMechanicVocabularySnapshot enemyVocabulary)
        {
            List<BoneAspectCarrierPresentationValidationIssue> issues =
                new List<BoneAspectCarrierPresentationValidationIssue>();
            if (input == null)
            {
                Add(issues, "INPUT_NULL", "$", "Catalog input is required.");
                return Sort(issues);
            }

            if (enemyVocabulary == null)
            {
                Add(issues, "E02_DEPENDENCY_NULL", "enemyVocabulary",
                    "The current E02 snapshot is required.");
            }

            ValidateRoot(input, issues);
            ValidateCollectionNulls(input, issues);

            BoneAspectCarrierPresentationSnapshot[] carriers =
                input.Carriers.Where(value => value != null).ToArray();
            BoneAspectArtDeliverySlotSnapshot[] artSlots =
                input.ArtDeliverySlots.Where(value => value != null).ToArray();
            BoneAspectMechanicReferenceSnapshot[] mechanicReferences =
                input.MechanicReferences.Where(value => value != null).ToArray();
            BoneAspectUserDecisionReferenceSnapshot[] decisions =
                input.DecisionReferences.Where(value => value != null).ToArray();

            AddNullRows(input, issues);
            ValidateCarriers(carriers, issues);
            ValidateArtSlots(artSlots, issues);
            ValidateMechanicReferences(mechanicReferences, enemyVocabulary, issues);
            ValidateDecisions(decisions, issues);

            if (issues.Count == 0)
            {
                ValidateCanonical(input, enemyVocabulary, issues);
            }

            return Sort(issues);
        }

        private static void ValidateRoot(
            BoneAspectCarrierPresentationCatalogInput input,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            if (!string.Equals(
                input.SchemaId,
                BoneAspectCarrierPresentationCatalogSchema.SchemaId,
                StringComparison.Ordinal))
            {
                Add(issues, "SCHEMA_ID_MISMATCH", "schemaId",
                    "Schema ID must match exactly.");
            }

            if (input.SchemaVersion != BoneAspectCarrierPresentationCatalogSchema.SchemaVersion)
            {
                Add(issues, "SCHEMA_VERSION_MISMATCH", "schemaVersion",
                    "Schema version must match exactly.");
            }

            if (!input.DevOnly || input.IsEnabled || input.EntersFormalFlow
                || input.RuntimeImplemented)
            {
                Add(issues, "CATALOG_ISOLATION_INVALID", "catalog",
                    "Catalog must remain devOnly=true, disabled, outside formal flow, and not implemented.");
            }
        }

        private static void ValidateCollectionNulls(
            BoneAspectCarrierPresentationCatalogInput input,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            if (input.HadNullCarriersCollection)
            {
                Add(issues, "CARRIERS_COLLECTION_NULL", "carriers",
                    "Carrier collection cannot be null.");
            }

            if (input.HadNullArtDeliverySlotsCollection)
            {
                Add(issues, "ART_SLOTS_COLLECTION_NULL", "artDeliverySlots",
                    "Art slot collection cannot be null.");
            }

            if (input.HadNullMechanicReferencesCollection)
            {
                Add(issues, "MECHANIC_REFERENCES_COLLECTION_NULL", "mechanicReferences",
                    "Mechanic reference collection cannot be null.");
            }

            if (input.HadNullDecisionReferencesCollection)
            {
                Add(issues, "DECISIONS_COLLECTION_NULL", "decisionReferences",
                    "Decision reference collection cannot be null.");
            }
        }

        private static void AddNullRows(
            BoneAspectCarrierPresentationCatalogInput input,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            for (int index = 0; index < input.Carriers.Count; index++)
            {
                if (input.Carriers[index] == null)
                {
                    Add(issues, "CARRIER_ROW_NULL", "carriers[" + index + "]",
                        "Carrier row cannot be null.");
                }
            }

            for (int index = 0; index < input.ArtDeliverySlots.Count; index++)
            {
                if (input.ArtDeliverySlots[index] == null)
                {
                    Add(issues, "ART_SLOT_ROW_NULL", "artDeliverySlots[" + index + "]",
                        "Art slot row cannot be null.");
                }
            }

            for (int index = 0; index < input.MechanicReferences.Count; index++)
            {
                if (input.MechanicReferences[index] == null)
                {
                    Add(issues, "MECHANIC_REFERENCE_ROW_NULL",
                        "mechanicReferences[" + index + "]",
                        "Mechanic reference row cannot be null.");
                }
            }

            for (int index = 0; index < input.DecisionReferences.Count; index++)
            {
                if (input.DecisionReferences[index] == null)
                {
                    Add(issues, "DECISION_ROW_NULL", "decisionReferences[" + index + "]",
                        "Decision row cannot be null.");
                }
            }
        }

        private static void ValidateCarriers(
            IReadOnlyList<BoneAspectCarrierPresentationSnapshot> carriers,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            if (carriers.Count != 16)
            {
                Add(issues, "CARRIER_COUNT_INVALID", "carriers",
                    "Exactly 16 Carrier rows are required.");
            }

            if (carriers.Count(value => value.CarrierKind == BoneAspectCarrierKind.Enemy) != 12
                || carriers.Count(value => value.CarrierKind == BoneAspectCarrierKind.Boss) != 4)
            {
                Add(issues, "CARRIER_KIND_COUNTS_INVALID", "carriers",
                    "Carrier kinds must be exactly 12 Enemy and 4 Boss.");
            }

            CheckUnique(carriers.Select(value => value.ContentId), "CONTENT_ID", issues);
            CheckUnique(carriers.Select(value => value.PresentationKey), "PRESENTATION_KEY", issues);
            CheckUnique(carriers.Select(value => value.NameLocalizationKey), "LOCALIZATION_KEY", issues);
            CheckUnique(carriers.Select(value => value.MechanicCommitmentId),
                "MECHANIC_COMMITMENT_ID", issues);

            IReadOnlyDictionary<string, BoneAspectCarrierPresentationSnapshot> expected =
                BoneAspectCarrierPresentationCatalog.CreateLockedCarrierBlueprints()
                    .ToDictionary(value => value.ContentId, StringComparer.Ordinal);
            foreach (BoneAspectCarrierPresentationSnapshot carrier in carriers)
            {
                string path = "carriers[" + carrier.ContentId + "]";
                if (!Enum.IsDefined(typeof(BoneAspectCarrierKind), carrier.CarrierKind))
                {
                    Add(issues, "CARRIER_KIND_UNDEFINED", path + ".carrierKind",
                        "Carrier kind enum value must be defined.");
                }

                if (IsBlank(carrier.ContentId)
                    || IsBlank(carrier.PresentationKey)
                    || IsBlank(carrier.NameLocalizationKey)
                    || IsBlank(carrier.MechanicCommitmentId))
                {
                    Add(issues, "CARRIER_IDENTITY_BLANK", path,
                        "Stable carrier identity fields cannot be blank.");
                }

                if (!carrier.DevOnly || carrier.IsEnabled || carrier.EntersFormalFlow
                    || carrier.RuntimeImplemented)
                {
                    Add(issues, "CARRIER_ISOLATION_INVALID", path,
                        "Carrier isolation flags are fixed and cannot be enabled.");
                }

                if (!carrier.GapSurveyRequired)
                {
                    Add(issues, "GAP_SURVEY_REQUIRED_FALSE", path + ".gapSurveyRequired",
                        "Every Carrier requires the later gap survey.");
                }

                if (!string.Equals(
                    carrier.DescriptionLocalizationKey,
                    carrier.PresentationKey + ".description",
                    StringComparison.Ordinal))
                {
                    Add(issues, "DESCRIPTION_KEY_INVALID",
                        path + ".descriptionLocalizationKey",
                        "Description key must equal presentationKey + .description.");
                }

                if (!string.Equals(
                    carrier.MechanicCommitmentId,
                    carrier.ContentId + ".mechanic_commitment",
                    StringComparison.Ordinal))
                {
                    Add(issues, "COMMITMENT_ID_INVALID", path + ".mechanicCommitmentId",
                        "Commitment ID must derive from contentId exactly.");
                }

                if (!expected.TryGetValue(carrier.ContentId, out BoneAspectCarrierPresentationSnapshot locked))
                {
                    Add(issues, "P0_CARRIER_UNKNOWN", path,
                        "Carrier is not part of the accepted P0 identity set.");
                    continue;
                }

                if (!CarrierEquals(carrier, locked))
                {
                    Add(issues, "P0_CARRIER_MISMATCH", path,
                        "Carrier fields must equal the accepted P0 identity row.");
                }
            }

            foreach (string contentId in expected.Keys)
            {
                if (!carriers.Any(value => string.Equals(
                    value.ContentId,
                    contentId,
                    StringComparison.Ordinal)))
                {
                    Add(issues, "P0_CARRIER_MISSING", "carriers[" + contentId + "]",
                        "Accepted P0 Carrier row is missing.");
                }
            }
        }

        private static void ValidateArtSlots(
            IReadOnlyList<BoneAspectArtDeliverySlotSnapshot> artSlots,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            if (artSlots.Count != 93)
            {
                Add(issues, "ART_SLOT_COUNT_INVALID", "artDeliverySlots",
                    "Exactly 93 art delivery slots are required.");
            }

            int required = artSlots.Count(value =>
                value.DeliveryStatus == BoneAspectArtDeliveryStatus.RequiredByLockedDesign);
            int blocked = artSlots.Count(value =>
                value.DeliveryStatus == BoneAspectArtDeliveryStatus.BlockedByUserDecision);
            if (required != 88 || blocked != 5)
            {
                Add(issues, "ART_SLOT_STATUS_COUNTS_INVALID", "artDeliverySlots",
                    "Art slots must be 88 required base rows and 5 decision-blocked rows.");
            }

            IReadOnlyList<BoneAspectCarrierPresentationSnapshot> expectedCarriers =
                BoneAspectCarrierPresentationCatalog.CreateLockedCarrierBlueprints();
            Dictionary<string, BoneAspectArtDeliverySlotSnapshot> expected =
                BoneAspectCarrierPresentationCatalog.CreateArtDeliverySlots(expectedCarriers)
                    .ToDictionary(ArtIdentity, StringComparer.Ordinal);
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (BoneAspectArtDeliverySlotSnapshot slot in artSlots)
            {
                string identity = ArtIdentity(slot);
                string path = "artDeliverySlots[" + identity + "]";
                if (!seen.Add(identity))
                {
                    Add(issues, "ART_SLOT_DUPLICATE", path,
                        "Carrier/slot identity must be unique.");
                }

                if (!Enum.IsDefined(typeof(BoneAspectArtDeliveryStatus), slot.DeliveryStatus))
                {
                    Add(issues, "ART_SLOT_STATUS_UNDEFINED", path + ".deliveryStatus",
                        "Art delivery status enum value must be defined.");
                }

                if (slot.RuntimeImplemented || !string.IsNullOrEmpty(slot.RuntimeBindingKey))
                {
                    Add(issues, "ART_SLOT_RUNTIME_BINDING_FORBIDDEN", path,
                        "Art slots cannot contain runtime implementation or binding keys.");
                }

                if (!expected.TryGetValue(identity, out BoneAspectArtDeliverySlotSnapshot locked)
                    || !ArtEquals(slot, locked))
                {
                    Add(issues, "ART_SLOT_MAPPING_INVALID", path,
                        "Art slot must match the locked base or decision-blocked mapping.");
                }
            }

            foreach (string identity in expected.Keys)
            {
                if (!seen.Contains(identity))
                {
                    Add(issues, "ART_SLOT_MISSING", "artDeliverySlots[" + identity + "]",
                        "Locked art delivery slot is missing.");
                }
            }

            int enemyBase = artSlots.Count(value =>
                value.DeliveryStatus == BoneAspectArtDeliveryStatus.RequiredByLockedDesign
                && value.CarrierId.StartsWith("bone_aspect_enemy_", StringComparison.Ordinal));
            int bossBase = artSlots.Count(value =>
                value.DeliveryStatus == BoneAspectArtDeliveryStatus.RequiredByLockedDesign
                && value.CarrierId.StartsWith("bone_aspect_boss_", StringComparison.Ordinal));
            if (enemyBase != 72 || bossBase != 16)
            {
                Add(issues, "ART_SLOT_BASE_COUNTS_INVALID", "artDeliverySlots",
                    "Base art slots must be exactly 72 Enemy and 16 Boss rows.");
            }
        }

        private static void ValidateMechanicReferences(
            IReadOnlyList<BoneAspectMechanicReferenceSnapshot> references,
            EnemyMechanicVocabularySnapshot enemyVocabulary,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            if (references.Count != 34)
            {
                Add(issues, "MECHANIC_REFERENCE_COUNT_INVALID", "mechanicReferences",
                    "Exactly 34 E02 CandidateReuseOnly references are required.");
            }

            if (references.Count(value =>
                    value.VocabularyCategory == EnemyVocabularyCategory.Mechanic) != 24
                || references.Count(value =>
                    value.VocabularyCategory == EnemyVocabularyCategory.PressureChannel) != 1
                || references.Count(value =>
                    value.VocabularyCategory == EnemyVocabularyCategory.CounterWindowType) != 9)
            {
                Add(issues, "MECHANIC_REFERENCE_CATEGORY_COUNTS_INVALID",
                    "mechanicReferences",
                    "Typed reference counts must be 24 Mechanic, 1 PressureChannel, and 9 CounterWindowType.");
            }

            if (references.Select(value => value.StableKey).Distinct(StringComparer.Ordinal).Count() != 12)
            {
                Add(issues, "MECHANIC_REFERENCE_DISTINCT_KEY_COUNT_INVALID",
                    "mechanicReferences", "Exactly 12 distinct E02 keys are required.");
            }

            Dictionary<string, BoneAspectMechanicReferenceSnapshot> expected =
                BoneAspectCarrierPresentationCatalog.CreateMechanicReferences(
                    BoneAspectCarrierPresentationCatalog.CreateLockedCarrierBlueprints())
                    .ToDictionary(MechanicIdentity, StringComparer.Ordinal);
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            Dictionary<string, EnemyVocabularyCategory> firstCategory =
                new Dictionary<string, EnemyVocabularyCategory>(StringComparer.Ordinal);

            foreach (BoneAspectMechanicReferenceSnapshot reference in references)
            {
                string identity = MechanicIdentity(reference);
                string path = "mechanicReferences[" + identity + "]";
                if (!seen.Add(identity))
                {
                    Add(issues, "MECHANIC_REFERENCE_DUPLICATE", path,
                        "Carrier/category/key identity must be unique.");
                }

                if (!Enum.IsDefined(
                    typeof(EnemyVocabularyCategory),
                    reference.VocabularyCategory))
                {
                    Add(issues, "E02_CATEGORY_UNDEFINED", path + ".vocabularyCategory",
                        "E02 category enum value must be defined.");
                }

                if (!Enum.IsDefined(
                    typeof(BoneAspectMechanicReferenceBindingStatus),
                    reference.BindingStatus))
                {
                    Add(issues, "BINDING_STATUS_UNDEFINED", path + ".bindingStatus",
                        "Binding status enum value must be defined.");
                }

                if (reference.BindingStatus
                    != BoneAspectMechanicReferenceBindingStatus.CandidateReuseOnly)
                {
                    Add(issues, "BINDING_STATUS_INVALID", path + ".bindingStatus",
                        "Every reference must remain CandidateReuseOnly.");
                }

                if (reference.RuntimeImplemented)
                {
                    Add(issues, "CANDIDATE_RUNTIME_IMPLEMENTED", path + ".runtimeImplemented",
                        "Candidate references cannot be marked implemented.");
                }

                if (firstCategory.TryGetValue(
                    reference.StableKey,
                    out EnemyVocabularyCategory earlierCategory)
                    && earlierCategory != reference.VocabularyCategory)
                {
                    Add(issues, "E02_CATEGORY_COLLISION", path,
                        "One stable key cannot be projected through multiple typed categories.");
                }
                else
                {
                    firstCategory[reference.StableKey] = reference.VocabularyCategory;
                }

                if (enemyVocabulary != null
                    && !enemyVocabulary.TryGetEntry(
                        reference.VocabularyCategory,
                        reference.StableKey,
                        out _))
                {
                    Add(issues, "E02_REFERENCE_UNRESOLVED", path,
                        "Typed key must resolve in the current E02 default snapshot.");
                }

                if (!expected.TryGetValue(
                    identity,
                    out BoneAspectMechanicReferenceSnapshot locked)
                    || !MechanicEquals(reference, locked))
                {
                    Add(issues, "MECHANIC_REFERENCE_MAPPING_INVALID", path,
                        "Reference must equal the P0 projected E02 candidate row.");
                }
            }

            foreach (string identity in expected.Keys)
            {
                if (!seen.Contains(identity))
                {
                    Add(issues, "MECHANIC_REFERENCE_MISSING",
                        "mechanicReferences[" + identity + "]",
                        "Locked E02 candidate reference is missing.");
                }
            }
        }

        private static void ValidateDecisions(
            IReadOnlyList<BoneAspectUserDecisionReferenceSnapshot> decisions,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            if (decisions.Count != 4)
            {
                Add(issues, "DECISION_COUNT_INVALID", "decisionReferences",
                    "Exactly BA-D1 through BA-D4 must remain referenced.");
            }

            Dictionary<string, BoneAspectUserDecisionReferenceSnapshot> expected =
                BoneAspectCarrierPresentationCatalog.CreateDecisionReferences()
                    .ToDictionary(value => value.DecisionId, StringComparer.Ordinal);
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (BoneAspectUserDecisionReferenceSnapshot decision in decisions)
            {
                string path = "decisionReferences[" + decision.DecisionId + "]";
                if (!seen.Add(decision.DecisionId))
                {
                    Add(issues, "DECISION_DUPLICATE", path,
                        "Decision identity must be unique.");
                }

                if (!string.Equals(
                        decision.DecisionStatus,
                        "USER_DECISION_REQUIRED",
                        StringComparison.Ordinal)
                    || !string.Equals(
                        decision.SelectedOption,
                        "NOT_SELECTED",
                        StringComparison.Ordinal)
                    || !string.IsNullOrEmpty(decision.DefaultOption)
                    || decision.RuntimeImplemented)
                {
                    Add(issues, "DECISION_RESOLUTION_FORBIDDEN", path,
                        "User decision must remain required, not selected, without a default or implementation.");
                }

                if (!expected.TryGetValue(
                    decision.DecisionId,
                    out BoneAspectUserDecisionReferenceSnapshot locked)
                    || !DecisionEquals(decision, locked))
                {
                    Add(issues, "DECISION_MAPPING_INVALID", path,
                        "Decision semantics and Carrier mapping must match accepted P0.");
                }
            }

            foreach (string decisionId in expected.Keys)
            {
                if (!seen.Contains(decisionId))
                {
                    Add(issues, "DECISION_MISSING",
                        "decisionReferences[" + decisionId + "]",
                        "Locked user decision reference is missing.");
                }
            }
        }

        private static void ValidateCanonical(
            BoneAspectCarrierPresentationCatalogInput input,
            EnemyMechanicVocabularySnapshot enemyVocabulary,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            BoneAspectCarrierPresentationCatalogSnapshot first =
                new BoneAspectCarrierPresentationCatalogSnapshot(
                    input,
                    enemyVocabulary.BuildCanonicalSignature());
            BoneAspectCarrierPresentationCatalogSnapshot repeat =
                new BoneAspectCarrierPresentationCatalogSnapshot(
                    input,
                    enemyVocabulary.BuildCanonicalSignature());
            BoneAspectCarrierPresentationCatalogSnapshot reversed =
                new BoneAspectCarrierPresentationCatalogSnapshot(
                    Reverse(input),
                    enemyVocabulary.BuildCanonicalSignature());

            if (!ValidSignature(first.CanonicalSignature)
                || !ValidSignature(first.PlayerSafe.PlayerSafeCanonicalSignature))
            {
                Add(issues, "CANONICAL_FORMAT_INVALID", "canonical",
                    "Full and player-safe signatures must use sha256 plus 64 lowercase hex.");
            }

            if (!string.Equals(
                    first.CanonicalSignature,
                    repeat.CanonicalSignature,
                    StringComparison.Ordinal)
                || !string.Equals(
                    first.CanonicalSignature,
                    reversed.CanonicalSignature,
                    StringComparison.Ordinal)
                || !string.Equals(
                    first.PlayerSafe.PlayerSafeCanonicalSignature,
                    reversed.PlayerSafe.PlayerSafeCanonicalSignature,
                    StringComparison.Ordinal))
            {
                Add(issues, "CANONICAL_NONDETERMINISTIC", "canonical",
                    "Signatures must be stable across repeat and reversed collections.");
            }

            BoneAspectCarrierPresentationCatalogInput fullOnlyMutation =
                MutateFirstCarrier(input, false);
            BoneAspectCarrierPresentationCatalogSnapshot fullOnly =
                new BoneAspectCarrierPresentationCatalogSnapshot(
                    fullOnlyMutation,
                    enemyVocabulary.BuildCanonicalSignature());
            if (string.Equals(
                    first.CanonicalSignature,
                    fullOnly.CanonicalSignature,
                    StringComparison.Ordinal)
                || !string.Equals(
                    first.PlayerSafe.PlayerSafeCanonicalSignature,
                    fullOnly.PlayerSafe.PlayerSafeCanonicalSignature,
                    StringComparison.Ordinal))
            {
                Add(issues, "FULL_CANONICAL_SENSITIVITY_INVALID", "canonical",
                    "Full-only mutation must change Full and preserve Player-safe signature.");
            }

            BoneAspectCarrierPresentationCatalogInput playerSafeMutation =
                MutateFirstCarrier(input, true);
            BoneAspectCarrierPresentationCatalogSnapshot playerSafe =
                new BoneAspectCarrierPresentationCatalogSnapshot(
                    playerSafeMutation,
                    enemyVocabulary.BuildCanonicalSignature());
            if (string.Equals(
                    first.CanonicalSignature,
                    playerSafe.CanonicalSignature,
                    StringComparison.Ordinal)
                || string.Equals(
                    first.PlayerSafe.PlayerSafeCanonicalSignature,
                    playerSafe.PlayerSafe.PlayerSafeCanonicalSignature,
                    StringComparison.Ordinal))
            {
                Add(issues, "PLAYER_SAFE_CANONICAL_SENSITIVITY_INVALID", "canonical",
                    "Player-safe mutation must change both signatures.");
            }
        }

        private static BoneAspectCarrierPresentationCatalogInput Reverse(
            BoneAspectCarrierPresentationCatalogInput input)
        {
            return new BoneAspectCarrierPresentationCatalogInput(
                input.Carriers.Reverse().ToArray(),
                input.ArtDeliverySlots.Reverse().ToArray(),
                input.MechanicReferences.Reverse().ToArray(),
                input.DecisionReferences.Reverse().ToArray(),
                input.SchemaId,
                input.SchemaVersion,
                input.DevOnly,
                input.IsEnabled,
                input.EntersFormalFlow,
                input.RuntimeImplemented);
        }

        private static BoneAspectCarrierPresentationCatalogInput MutateFirstCarrier(
            BoneAspectCarrierPresentationCatalogInput input,
            bool playerSafeField)
        {
            BoneAspectCarrierPresentationSnapshot[] rows =
                input.Carriers.Select(value => value.Clone()).ToArray();
            BoneAspectCarrierPresentationSnapshot source = rows[0];
            rows[0] = new BoneAspectCarrierPresentationSnapshot(
                source.ContentId,
                source.CarrierKind,
                source.ChapterId,
                playerSafeField ? source.DisplayName + "·mutation" : source.DisplayName,
                source.NameLocalizationKey,
                source.DescriptionLocalizationKey,
                source.PresentationKey,
                source.VisualFamilyId,
                source.PrimarySilhouetteKey,
                source.CoreRecognitionKey,
                source.CoreRecognitionPoint,
                playerSafeField
                    ? source.MechanicCommitmentSummary
                    : source.MechanicCommitmentSummary + "·mutation",
                source.MechanicCommitmentId,
                source.GapSurveyRequired,
                source.DecisionIds,
                source.DevOnly,
                source.IsEnabled,
                source.EntersFormalFlow,
                source.RuntimeImplemented);
            return new BoneAspectCarrierPresentationCatalogInput(
                rows,
                input.ArtDeliverySlots,
                input.MechanicReferences,
                input.DecisionReferences,
                input.SchemaId,
                input.SchemaVersion,
                input.DevOnly,
                input.IsEnabled,
                input.EntersFormalFlow,
                input.RuntimeImplemented);
        }

        private static bool CarrierEquals(
            BoneAspectCarrierPresentationSnapshot left,
            BoneAspectCarrierPresentationSnapshot right)
        {
            return left.CarrierKind == right.CarrierKind
                && Equal(left.ContentId, right.ContentId)
                && Equal(left.ChapterId, right.ChapterId)
                && Equal(left.DisplayName, right.DisplayName)
                && Equal(left.NameLocalizationKey, right.NameLocalizationKey)
                && Equal(left.DescriptionLocalizationKey, right.DescriptionLocalizationKey)
                && Equal(left.PresentationKey, right.PresentationKey)
                && Equal(left.VisualFamilyId, right.VisualFamilyId)
                && Equal(left.PrimarySilhouetteKey, right.PrimarySilhouetteKey)
                && Equal(left.CoreRecognitionKey, right.CoreRecognitionKey)
                && Equal(left.CoreRecognitionPoint, right.CoreRecognitionPoint)
                && Equal(left.MechanicCommitmentSummary, right.MechanicCommitmentSummary)
                && Equal(left.MechanicCommitmentId, right.MechanicCommitmentId)
                && left.GapSurveyRequired == right.GapSurveyRequired
                && left.DecisionIds.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(
                        right.DecisionIds.OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal)
                && left.DevOnly == right.DevOnly
                && left.IsEnabled == right.IsEnabled
                && left.EntersFormalFlow == right.EntersFormalFlow
                && left.RuntimeImplemented == right.RuntimeImplemented;
        }

        private static bool ArtEquals(
            BoneAspectArtDeliverySlotSnapshot left,
            BoneAspectArtDeliverySlotSnapshot right)
        {
            return Equal(left.CarrierId, right.CarrierId)
                && Equal(left.SlotKey, right.SlotKey)
                && left.DeliveryStatus == right.DeliveryStatus
                && Equal(left.BlockedByDecisionId, right.BlockedByDecisionId)
                && Equal(left.RuntimeBindingKey, right.RuntimeBindingKey)
                && left.RuntimeImplemented == right.RuntimeImplemented;
        }

        private static bool MechanicEquals(
            BoneAspectMechanicReferenceSnapshot left,
            BoneAspectMechanicReferenceSnapshot right)
        {
            return Equal(left.CarrierId, right.CarrierId)
                && left.VocabularyCategory == right.VocabularyCategory
                && Equal(left.StableKey, right.StableKey)
                && left.BindingStatus == right.BindingStatus
                && left.RuntimeImplemented == right.RuntimeImplemented
                && Equal(left.SourceCommitmentId, right.SourceCommitmentId);
        }

        private static bool DecisionEquals(
            BoneAspectUserDecisionReferenceSnapshot left,
            BoneAspectUserDecisionReferenceSnapshot right)
        {
            return Equal(left.DecisionId, right.DecisionId)
                && Equal(left.Title, right.Title)
                && left.CarrierIds.OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(
                        right.CarrierIds.OrderBy(value => value, StringComparer.Ordinal),
                        StringComparer.Ordinal)
                && Equal(left.DecisionStatus, right.DecisionStatus)
                && Equal(left.SelectedOption, right.SelectedOption)
                && Equal(left.DefaultOption, right.DefaultOption)
                && left.RuntimeImplemented == right.RuntimeImplemented;
        }

        private static string ArtIdentity(BoneAspectArtDeliverySlotSnapshot value)
        {
            return value.CarrierId + "\u001f" + value.SlotKey;
        }

        private static string MechanicIdentity(BoneAspectMechanicReferenceSnapshot value)
        {
            return value.CarrierId + "\u001f" + ((int)value.VocabularyCategory)
                + "\u001f" + value.StableKey;
        }

        private static void CheckUnique(
            IEnumerable<string> values,
            string identityName,
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string value in values)
            {
                if (IsBlank(value))
                {
                    Add(issues, identityName + "_BLANK", identityName,
                        "Stable identity cannot be blank.");
                }
                else if (!seen.Add(value))
                {
                    Add(issues, identityName + "_DUPLICATE", identityName + "[" + value + "]",
                        "Stable identity must be unique.");
                }
            }
        }

        private static bool ValidSignature(string value)
        {
            return value != null
                && value.Length == 71
                && value.StartsWith("sha256:", StringComparison.Ordinal)
                && value.Skip(7).All(character =>
                    (character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f'));
        }

        private static bool Equal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static bool IsBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        private static void Add(
            ICollection<BoneAspectCarrierPresentationValidationIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new BoneAspectCarrierPresentationValidationIssue(code, path, message));
        }

        private static IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> Sort(
            IEnumerable<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            return new ReadOnlyCollection<BoneAspectCarrierPresentationValidationIssue>(
                issues.OrderBy(value => value.Code, StringComparer.Ordinal)
                    .ThenBy(value => value.Path, StringComparer.Ordinal)
                    .ThenBy(value => value.Message, StringComparer.Ordinal)
                    .ToArray());
        }
    }
}
