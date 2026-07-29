using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.CarrierPresentation
{
    public sealed class BoneAspectCarrierPresentationSnapshot
    {
        private readonly ReadOnlyCollection<string> decisionIds;

        public BoneAspectCarrierPresentationSnapshot(
            string contentId,
            BoneAspectCarrierKind carrierKind,
            string chapterId,
            string displayName,
            string nameLocalizationKey,
            string descriptionLocalizationKey,
            string presentationKey,
            string visualFamilyId,
            string primarySilhouetteKey,
            string coreRecognitionKey,
            string coreRecognitionPoint,
            string mechanicCommitmentSummary,
            string mechanicCommitmentId,
            bool gapSurveyRequired,
            IReadOnlyList<string> decisionIds,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false,
            bool runtimeImplemented = false)
        {
            ContentId = contentId ?? string.Empty;
            CarrierKind = carrierKind;
            ChapterId = chapterId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            NameLocalizationKey = nameLocalizationKey ?? string.Empty;
            DescriptionLocalizationKey = descriptionLocalizationKey ?? string.Empty;
            PresentationKey = presentationKey ?? string.Empty;
            VisualFamilyId = visualFamilyId ?? string.Empty;
            PrimarySilhouetteKey = primarySilhouetteKey ?? string.Empty;
            CoreRecognitionKey = coreRecognitionKey ?? string.Empty;
            CoreRecognitionPoint = coreRecognitionPoint ?? string.Empty;
            MechanicCommitmentSummary = mechanicCommitmentSummary ?? string.Empty;
            MechanicCommitmentId = mechanicCommitmentId ?? string.Empty;
            GapSurveyRequired = gapSurveyRequired;
            this.decisionIds = Array.AsReadOnly((decisionIds ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .ToArray());
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
            RuntimeImplemented = runtimeImplemented;
        }

        public string ContentId { get; }
        public BoneAspectCarrierKind CarrierKind { get; }
        public string ChapterId { get; }
        public string DisplayName { get; }
        public string NameLocalizationKey { get; }
        public string DescriptionLocalizationKey { get; }
        public string PresentationKey { get; }
        public string VisualFamilyId { get; }
        public string PrimarySilhouetteKey { get; }
        public string CoreRecognitionKey { get; }
        public string CoreRecognitionPoint { get; }
        public string MechanicCommitmentSummary { get; }
        public string MechanicCommitmentId { get; }
        public bool GapSurveyRequired { get; }
        public IReadOnlyList<string> DecisionIds => decisionIds;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
        public bool RuntimeImplemented { get; }

        internal BoneAspectCarrierPresentationSnapshot Clone()
        {
            return new BoneAspectCarrierPresentationSnapshot(
                ContentId,
                CarrierKind,
                ChapterId,
                DisplayName,
                NameLocalizationKey,
                DescriptionLocalizationKey,
                PresentationKey,
                VisualFamilyId,
                PrimarySilhouetteKey,
                CoreRecognitionKey,
                CoreRecognitionPoint,
                MechanicCommitmentSummary,
                MechanicCommitmentId,
                GapSurveyRequired,
                decisionIds,
                DevOnly,
                IsEnabled,
                EntersFormalFlow,
                RuntimeImplemented);
        }
    }

    public sealed class BoneAspectArtDeliverySlotSnapshot
    {
        public BoneAspectArtDeliverySlotSnapshot(
            string carrierId,
            string slotKey,
            BoneAspectArtDeliveryStatus deliveryStatus,
            string blockedByDecisionId,
            string runtimeBindingKey,
            bool runtimeImplemented = false)
        {
            CarrierId = carrierId ?? string.Empty;
            SlotKey = slotKey ?? string.Empty;
            DeliveryStatus = deliveryStatus;
            BlockedByDecisionId = blockedByDecisionId ?? string.Empty;
            RuntimeBindingKey = runtimeBindingKey ?? string.Empty;
            RuntimeImplemented = runtimeImplemented;
        }

        public string CarrierId { get; }
        public string SlotKey { get; }
        public BoneAspectArtDeliveryStatus DeliveryStatus { get; }
        public string BlockedByDecisionId { get; }
        public string RuntimeBindingKey { get; }
        public bool RuntimeImplemented { get; }

        internal BoneAspectArtDeliverySlotSnapshot Clone()
        {
            return new BoneAspectArtDeliverySlotSnapshot(
                CarrierId,
                SlotKey,
                DeliveryStatus,
                BlockedByDecisionId,
                RuntimeBindingKey,
                RuntimeImplemented);
        }
    }

    public sealed class BoneAspectMechanicReferenceSnapshot
    {
        public BoneAspectMechanicReferenceSnapshot(
            string carrierId,
            EnemyVocabularyCategory vocabularyCategory,
            string stableKey,
            BoneAspectMechanicReferenceBindingStatus bindingStatus,
            bool runtimeImplemented,
            string sourceCommitmentId)
        {
            CarrierId = carrierId ?? string.Empty;
            VocabularyCategory = vocabularyCategory;
            StableKey = stableKey ?? string.Empty;
            BindingStatus = bindingStatus;
            RuntimeImplemented = runtimeImplemented;
            SourceCommitmentId = sourceCommitmentId ?? string.Empty;
        }

        public string CarrierId { get; }
        public EnemyVocabularyCategory VocabularyCategory { get; }
        public string StableKey { get; }
        public BoneAspectMechanicReferenceBindingStatus BindingStatus { get; }
        public bool RuntimeImplemented { get; }
        public string SourceCommitmentId { get; }

        internal BoneAspectMechanicReferenceSnapshot Clone()
        {
            return new BoneAspectMechanicReferenceSnapshot(
                CarrierId,
                VocabularyCategory,
                StableKey,
                BindingStatus,
                RuntimeImplemented,
                SourceCommitmentId);
        }
    }

    public sealed class BoneAspectUserDecisionReferenceSnapshot
    {
        private readonly ReadOnlyCollection<string> carrierIds;

        public BoneAspectUserDecisionReferenceSnapshot(
            string decisionId,
            string title,
            IReadOnlyList<string> carrierIds,
            string decisionStatus,
            string selectedOption,
            string defaultOption,
            bool runtimeImplemented)
        {
            DecisionId = decisionId ?? string.Empty;
            Title = title ?? string.Empty;
            this.carrierIds = Array.AsReadOnly((carrierIds ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .ToArray());
            DecisionStatus = decisionStatus ?? string.Empty;
            SelectedOption = selectedOption ?? string.Empty;
            DefaultOption = defaultOption ?? string.Empty;
            RuntimeImplemented = runtimeImplemented;
        }

        public string DecisionId { get; }
        public string Title { get; }
        public IReadOnlyList<string> CarrierIds => carrierIds;
        public string DecisionStatus { get; }
        public string SelectedOption { get; }
        public string DefaultOption { get; }
        public bool RuntimeImplemented { get; }

        internal BoneAspectUserDecisionReferenceSnapshot Clone()
        {
            return new BoneAspectUserDecisionReferenceSnapshot(
                DecisionId,
                Title,
                carrierIds,
                DecisionStatus,
                SelectedOption,
                DefaultOption,
                RuntimeImplemented);
        }
    }

    public sealed class BoneAspectCarrierPresentationCatalogInput
    {
        private readonly ReadOnlyCollection<BoneAspectCarrierPresentationSnapshot> carriers;
        private readonly ReadOnlyCollection<BoneAspectArtDeliverySlotSnapshot> artDeliverySlots;
        private readonly ReadOnlyCollection<BoneAspectMechanicReferenceSnapshot> mechanicReferences;
        private readonly ReadOnlyCollection<BoneAspectUserDecisionReferenceSnapshot> decisionReferences;

        public BoneAspectCarrierPresentationCatalogInput(
            IReadOnlyList<BoneAspectCarrierPresentationSnapshot> carriers,
            IReadOnlyList<BoneAspectArtDeliverySlotSnapshot> artDeliverySlots,
            IReadOnlyList<BoneAspectMechanicReferenceSnapshot> mechanicReferences,
            IReadOnlyList<BoneAspectUserDecisionReferenceSnapshot> decisionReferences,
            string schemaId = BoneAspectCarrierPresentationCatalogSchema.SchemaId,
            int schemaVersion = BoneAspectCarrierPresentationCatalogSchema.SchemaVersion,
            bool devOnly = true,
            bool isEnabled = false,
            bool entersFormalFlow = false,
            bool runtimeImplemented = false)
        {
            HadNullCarriersCollection = carriers == null;
            HadNullArtDeliverySlotsCollection = artDeliverySlots == null;
            HadNullMechanicReferencesCollection = mechanicReferences == null;
            HadNullDecisionReferencesCollection = decisionReferences == null;
            SchemaId = schemaId ?? string.Empty;
            SchemaVersion = schemaVersion;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
            RuntimeImplemented = runtimeImplemented;
            this.carriers = Array.AsReadOnly((carriers
                ?? Array.Empty<BoneAspectCarrierPresentationSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
            this.artDeliverySlots = Array.AsReadOnly((artDeliverySlots
                ?? Array.Empty<BoneAspectArtDeliverySlotSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
            this.mechanicReferences = Array.AsReadOnly((mechanicReferences
                ?? Array.Empty<BoneAspectMechanicReferenceSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
            this.decisionReferences = Array.AsReadOnly((decisionReferences
                ?? Array.Empty<BoneAspectUserDecisionReferenceSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
        public bool RuntimeImplemented { get; }
        public IReadOnlyList<BoneAspectCarrierPresentationSnapshot> Carriers => carriers;
        public IReadOnlyList<BoneAspectArtDeliverySlotSnapshot> ArtDeliverySlots => artDeliverySlots;
        public IReadOnlyList<BoneAspectMechanicReferenceSnapshot> MechanicReferences => mechanicReferences;
        public IReadOnlyList<BoneAspectUserDecisionReferenceSnapshot> DecisionReferences => decisionReferences;
        internal bool HadNullCarriersCollection { get; }
        internal bool HadNullArtDeliverySlotsCollection { get; }
        internal bool HadNullMechanicReferencesCollection { get; }
        internal bool HadNullDecisionReferencesCollection { get; }
    }

    public sealed class BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot
    {
        private readonly ReadOnlyCollection<string> requiredArtSlotKeys;

        internal BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot(
            BoneAspectCarrierPresentationSnapshot carrier,
            IEnumerable<string> requiredArtSlotKeys)
        {
            ContentId = carrier.ContentId;
            CarrierKind = carrier.CarrierKind;
            DisplayName = carrier.DisplayName;
            NameLocalizationKey = carrier.NameLocalizationKey;
            DescriptionLocalizationKey = carrier.DescriptionLocalizationKey;
            PresentationKey = carrier.PresentationKey;
            VisualFamilyId = carrier.VisualFamilyId;
            PrimarySilhouetteKey = carrier.PrimarySilhouetteKey;
            CoreRecognitionKey = carrier.CoreRecognitionKey;
            this.requiredArtSlotKeys = Array.AsReadOnly((requiredArtSlotKeys ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            DevOnly = carrier.DevOnly;
            IsEnabled = carrier.IsEnabled;
            EntersFormalFlow = carrier.EntersFormalFlow;
        }

        public string ContentId { get; }
        public BoneAspectCarrierKind CarrierKind { get; }
        public string DisplayName { get; }
        public string NameLocalizationKey { get; }
        public string DescriptionLocalizationKey { get; }
        public string PresentationKey { get; }
        public string VisualFamilyId { get; }
        public string PrimarySilhouetteKey { get; }
        public string CoreRecognitionKey { get; }
        public IReadOnlyList<string> RequiredArtSlotKeys => requiredArtSlotKeys;
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
    }

    public sealed class BoneAspectCarrierPresentationPlayerSafeSnapshot
    {
        private readonly ReadOnlyCollection<BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot> carriers;

        internal BoneAspectCarrierPresentationPlayerSafeSnapshot(
            string schemaId,
            int schemaVersion,
            IEnumerable<BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot> carriers)
        {
            SchemaId = schemaId ?? string.Empty;
            SchemaVersion = schemaVersion;
            this.carriers = Array.AsReadOnly((carriers
                ?? Array.Empty<BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot>())
                .OrderBy(value => value.ContentId, StringComparer.Ordinal)
                .ToArray());
            PlayerSafeCanonicalSignature = BoneAspectCarrierPresentationCanonical.PlayerSafe(this);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot> Carriers => carriers;
        public string PlayerSafeCanonicalSignature { get; }
    }

    public sealed class BoneAspectCarrierPresentationCatalogSnapshot
    {
        private readonly ReadOnlyCollection<BoneAspectCarrierPresentationSnapshot> carriers;
        private readonly ReadOnlyCollection<BoneAspectArtDeliverySlotSnapshot> artDeliverySlots;
        private readonly ReadOnlyCollection<BoneAspectMechanicReferenceSnapshot> mechanicReferences;
        private readonly ReadOnlyCollection<BoneAspectUserDecisionReferenceSnapshot> decisionReferences;

        internal BoneAspectCarrierPresentationCatalogSnapshot(
            BoneAspectCarrierPresentationCatalogInput input,
            string enemyVocabularyCanonicalSignature)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            DevOnly = input.DevOnly;
            IsEnabled = input.IsEnabled;
            EntersFormalFlow = input.EntersFormalFlow;
            RuntimeImplemented = input.RuntimeImplemented;
            carriers = Array.AsReadOnly(input.Carriers.Select(value => value.Clone()).ToArray());
            artDeliverySlots = Array.AsReadOnly(input.ArtDeliverySlots.Select(value => value.Clone()).ToArray());
            mechanicReferences = Array.AsReadOnly(input.MechanicReferences.Select(value => value.Clone()).ToArray());
            decisionReferences = Array.AsReadOnly(input.DecisionReferences.Select(value => value.Clone()).ToArray());
            EnemyVocabularyCanonicalSignature = enemyVocabularyCanonicalSignature ?? string.Empty;
            PlayerSafe = CreatePlayerSafe();
            CanonicalSignature = BoneAspectCarrierPresentationCanonical.Full(this);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
        public bool RuntimeImplemented { get; }
        public IReadOnlyList<BoneAspectCarrierPresentationSnapshot> Carriers => carriers;
        public IReadOnlyList<BoneAspectArtDeliverySlotSnapshot> ArtDeliverySlots => artDeliverySlots;
        public IReadOnlyList<BoneAspectMechanicReferenceSnapshot> MechanicReferences => mechanicReferences;
        public IReadOnlyList<BoneAspectUserDecisionReferenceSnapshot> DecisionReferences => decisionReferences;
        public string EnemyVocabularyCanonicalSignature { get; }
        public BoneAspectCarrierPresentationPlayerSafeSnapshot PlayerSafe { get; }
        public string CanonicalSignature { get; }

        private BoneAspectCarrierPresentationPlayerSafeSnapshot CreatePlayerSafe()
        {
            BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot[] rows = carriers
                .Select(carrier => new BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot(
                    carrier,
                    artDeliverySlots
                        .Where(slot => string.Equals(slot.CarrierId, carrier.ContentId, StringComparison.Ordinal)
                            && slot.DeliveryStatus == BoneAspectArtDeliveryStatus.RequiredByLockedDesign)
                        .Select(slot => slot.SlotKey)))
                .ToArray();
            return new BoneAspectCarrierPresentationPlayerSafeSnapshot(SchemaId, SchemaVersion, rows);
        }
    }

    internal static class BoneAspectCarrierPresentationCanonical
    {
        public static string Full(BoneAspectCarrierPresentationCatalogSnapshot value)
        {
            StringBuilder builder = new StringBuilder(65536);
            Field(builder, "schemaId", value.SchemaId);
            Field(builder, "schemaVersion", value.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "devOnly", Bool(value.DevOnly));
            Field(builder, "isEnabled", Bool(value.IsEnabled));
            Field(builder, "entersFormalFlow", Bool(value.EntersFormalFlow));
            Field(builder, "runtimeImplemented", Bool(value.RuntimeImplemented));
            Collection(builder, "carriers", value.Carriers, Carrier);
            Collection(builder, "artDeliverySlots", value.ArtDeliverySlots, ArtSlot);
            Collection(builder, "mechanicReferences", value.MechanicReferences, MechanicReference);
            Collection(builder, "decisionReferences", value.DecisionReferences, DecisionReference);
            Field(builder, "enemyVocabularyCanonicalSignature", value.EnemyVocabularyCanonicalSignature);
            Field(builder, "playerSafeCanonicalSignature", value.PlayerSafe.PlayerSafeCanonicalSignature);
            return Hash(builder.ToString());
        }

        public static string PlayerSafe(BoneAspectCarrierPresentationPlayerSafeSnapshot value)
        {
            StringBuilder builder = new StringBuilder(32768);
            Field(builder, "schemaId", value.SchemaId);
            Field(builder, "schemaVersion", value.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            Collection(builder, "carriers", value.Carriers, PlayerSafeCarrier);
            return Hash(builder.ToString());
        }

        private static string Carrier(BoneAspectCarrierPresentationSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "contentId", value.ContentId);
            Field(builder, "carrierKind", ((int)value.CarrierKind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "chapterId", value.ChapterId);
            Field(builder, "displayName", value.DisplayName);
            Field(builder, "nameLocalizationKey", value.NameLocalizationKey);
            Field(builder, "descriptionLocalizationKey", value.DescriptionLocalizationKey);
            Field(builder, "presentationKey", value.PresentationKey);
            Field(builder, "visualFamilyId", value.VisualFamilyId);
            Field(builder, "primarySilhouetteKey", value.PrimarySilhouetteKey);
            Field(builder, "coreRecognitionKey", value.CoreRecognitionKey);
            Field(builder, "coreRecognitionPoint", value.CoreRecognitionPoint);
            Field(builder, "mechanicCommitmentSummary", value.MechanicCommitmentSummary);
            Field(builder, "mechanicCommitmentId", value.MechanicCommitmentId);
            Field(builder, "gapSurveyRequired", Bool(value.GapSurveyRequired));
            Collection(builder, "decisionIds", value.DecisionIds, item => item);
            Field(builder, "devOnly", Bool(value.DevOnly));
            Field(builder, "isEnabled", Bool(value.IsEnabled));
            Field(builder, "entersFormalFlow", Bool(value.EntersFormalFlow));
            Field(builder, "runtimeImplemented", Bool(value.RuntimeImplemented));
            return builder.ToString();
        }

        private static string ArtSlot(BoneAspectArtDeliverySlotSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "carrierId", value.CarrierId);
            Field(builder, "slotKey", value.SlotKey);
            Field(builder, "deliveryStatus", ((int)value.DeliveryStatus).ToString(CultureInfo.InvariantCulture));
            Field(builder, "blockedByDecisionId", value.BlockedByDecisionId);
            Field(builder, "runtimeBindingKey", value.RuntimeBindingKey);
            Field(builder, "runtimeImplemented", Bool(value.RuntimeImplemented));
            return builder.ToString();
        }

        private static string MechanicReference(BoneAspectMechanicReferenceSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "carrierId", value.CarrierId);
            Field(builder, "vocabularyCategory", ((int)value.VocabularyCategory).ToString(CultureInfo.InvariantCulture));
            Field(builder, "stableKey", value.StableKey);
            Field(builder, "bindingStatus", ((int)value.BindingStatus).ToString(CultureInfo.InvariantCulture));
            Field(builder, "runtimeImplemented", Bool(value.RuntimeImplemented));
            Field(builder, "sourceCommitmentId", value.SourceCommitmentId);
            return builder.ToString();
        }

        private static string DecisionReference(BoneAspectUserDecisionReferenceSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "decisionId", value.DecisionId);
            Field(builder, "title", value.Title);
            Collection(builder, "carrierIds", value.CarrierIds, item => item);
            Field(builder, "decisionStatus", value.DecisionStatus);
            Field(builder, "selectedOption", value.SelectedOption);
            Field(builder, "defaultOption", value.DefaultOption);
            Field(builder, "runtimeImplemented", Bool(value.RuntimeImplemented));
            return builder.ToString();
        }

        private static string PlayerSafeCarrier(
            BoneAspectCarrierPresentationPlayerSafeCarrierSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "contentId", value.ContentId);
            Field(builder, "carrierKind", ((int)value.CarrierKind).ToString(CultureInfo.InvariantCulture));
            Field(builder, "displayName", value.DisplayName);
            Field(builder, "nameLocalizationKey", value.NameLocalizationKey);
            Field(builder, "descriptionLocalizationKey", value.DescriptionLocalizationKey);
            Field(builder, "presentationKey", value.PresentationKey);
            Field(builder, "visualFamilyId", value.VisualFamilyId);
            Field(builder, "primarySilhouetteKey", value.PrimarySilhouetteKey);
            Field(builder, "coreRecognitionKey", value.CoreRecognitionKey);
            Collection(builder, "requiredArtSlotKeys", value.RequiredArtSlotKeys, item => item);
            Field(builder, "devOnly", Bool(value.DevOnly));
            Field(builder, "isEnabled", Bool(value.IsEnabled));
            Field(builder, "entersFormalFlow", Bool(value.EntersFormalFlow));
            return builder.ToString();
        }

        private static void Collection<T>(
            StringBuilder builder,
            string name,
            IEnumerable<T> values,
            Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .Select(value => value == null ? "<null>" : canonical(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(builder, name + ".count", rows.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(
                    builder,
                    name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                    rows[index]);
            }
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeName)
                .Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':')
                .Append(safeValue)
                .Append(';');
        }

        private static string Bool(bool value)
        {
            return value ? "1" : "0";
        }

        private static string Hash(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte item in bytes)
                {
                    builder.Append(item.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
