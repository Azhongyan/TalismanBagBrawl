using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.Items.Capability
{
    public enum ItemInstancePlacementBindingStatus
    {
        Valid = 0,
        Unknown = 1,
        Invalid = 2
    }

    public static class ItemInstancePlacementBindingValidationCodes
    {
        public const string None = "NONE";
        public const string ProjectionSourceMissing = "PROJECTION_SOURCE_MISSING";
        public const string ProjectionSourceInvalid = "PROJECTION_SOURCE_INVALID";
        public const string PlacementSourceMissing = "PLACEMENT_SOURCE_MISSING";
        public const string PlacementSourceInvalid = "PLACEMENT_SOURCE_INVALID";
        public const string BindingRowMissing = "BINDING_ROW_MISSING";
        public const string ItemInstanceIdMissing = "ITEM_INSTANCE_ID_MISSING";
        public const string PlacementIdMissing = "PLACEMENT_ID_MISSING";
        public const string BaseItemIdMissing = "BASE_ITEM_ID_MISSING";
        public const string ProjectionItemInstanceIdDuplicate = "PROJECTION_ITEM_INSTANCE_ID_DUPLICATE";
        public const string PlacementIdDuplicate = "SOURCE_PLACEMENT_ID_DUPLICATE";
        public const string BindingItemInstanceIdDuplicate = "BINDING_ITEM_INSTANCE_ID_DUPLICATE";
        public const string BindingPlacementIdDuplicate = "BINDING_PLACEMENT_ID_DUPLICATE";
        public const string ProjectionOrphan = "PROJECTION_ORPHAN";
        public const string PlacementOrphan = "PLACEMENT_ORPHAN";
        public const string ProjectionBindingMissing = "PROJECTION_BINDING_MISSING";
        public const string PlacementBindingMissing = "PLACEMENT_BINDING_MISSING";
        public const string ProjectionBaseItemIdMismatch = "PROJECTION_BASE_ITEM_ID_MISMATCH";
        public const string PlacementBaseItemIdMismatch = "PLACEMENT_BASE_ITEM_ID_MISMATCH";
        public const string I031OrdinaryInstanceForbidden = "I031_ORDINARY_INSTANCE_FORBIDDEN";
    }

    public sealed class ItemInstancePlacementBindingInput
    {
        public ItemInstancePlacementBindingInput(
            string itemInstanceId,
            string placementId,
            string baseItemId)
        {
            this.itemInstanceId = BindingCanonical.Normalize(itemInstanceId);
            this.placementId = BindingCanonical.Normalize(placementId);
            this.baseItemId = BindingCanonical.Normalize(baseItemId);
        }

        public string itemInstanceId { get; }
        public string placementId { get; }
        public string baseItemId { get; }
    }

    public sealed class ItemInstancePlacementBindingValidationError
    {
        public ItemInstancePlacementBindingValidationError(
            string code,
            ItemInstancePlacementBindingStatus status,
            string itemInstanceId,
            string placementId,
            string baseItemId,
            string message)
        {
            this.code = BindingCanonical.Normalize(code);
            this.status = status;
            this.itemInstanceId = BindingCanonical.Normalize(itemInstanceId);
            this.placementId = BindingCanonical.Normalize(placementId);
            this.baseItemId = BindingCanonical.Normalize(baseItemId);
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public ItemInstancePlacementBindingStatus status { get; }
        public string itemInstanceId { get; }
        public string placementId { get; }
        public string baseItemId { get; }
        public string message { get; }

        public string ToDiagnosticString()
        {
            return status + ":" + code
                + ": itemInstanceId=" + Format(itemInstanceId)
                + " placementId=" + Format(placementId)
                + " baseItemId=" + Format(baseItemId)
                + " " + message;
        }

        private static string Format(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
        }
    }

    public sealed class ItemInstancePlacementBindingSnapshot
    {
        internal ItemInstancePlacementBindingSnapshot(
            string itemInstanceId,
            string placementId,
            string baseItemId)
        {
            this.itemInstanceId = BindingCanonical.Normalize(itemInstanceId);
            this.placementId = BindingCanonical.Normalize(placementId);
            this.baseItemId = BindingCanonical.Normalize(baseItemId);
        }

        public string itemInstanceId { get; }
        public string placementId { get; }
        public string baseItemId { get; }
    }

    public sealed class ItemInstancePlacementBindingContractSnapshot
    {
        public const string CurrentSchemaId = "ItemInstancePlacementBindingContractSnapshot.v1";

        private readonly ReadOnlyCollection<ItemInstancePlacementBindingSnapshot> bindings;
        private readonly ReadOnlyCollection<ItemInstancePlacementBindingValidationError> validationErrors;

        internal ItemInstancePlacementBindingContractSnapshot(
            ItemInstancePlacementBindingStatus status,
            IEnumerable<ItemInstancePlacementBindingSnapshot> bindings,
            IEnumerable<ItemInstancePlacementBindingValidationError> validationErrors)
        {
            schemaId = CurrentSchemaId;
            this.status = status;
            this.bindings = BindingReadOnly.Freeze((bindings ??
                    Array.Empty<ItemInstancePlacementBindingSnapshot>())
                .Where(value => value != null)
                .Select(value => new ItemInstancePlacementBindingSnapshot(
                    value.itemInstanceId,
                    value.placementId,
                    value.baseItemId))
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.placementId, StringComparer.Ordinal)
                .ThenBy(value => value.baseItemId, StringComparer.Ordinal));
            this.validationErrors = BindingReadOnly.Freeze((validationErrors ??
                    Array.Empty<ItemInstancePlacementBindingValidationError>())
                .Where(value => value != null)
                .OrderBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.placementId, StringComparer.Ordinal)
                .ThenBy(value => value.baseItemId, StringComparer.Ordinal)
                .ThenBy(value => value.message, StringComparer.Ordinal));
            canonicalSignature = BindingCanonical.Sha256(BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public ItemInstancePlacementBindingStatus status { get; }
        public bool isValid => status == ItemInstancePlacementBindingStatus.Valid
            && validationErrors.Count == 0;
        public IReadOnlyList<ItemInstancePlacementBindingSnapshot> Bindings => bindings;
        public IReadOnlyList<ItemInstancePlacementBindingValidationError> ValidationErrors => validationErrors;
        public string canonicalSignature { get; }

        public ItemInstancePlacementBindingSnapshot FindByItemInstanceId(string itemInstanceId)
        {
            string normalized = BindingCanonical.Normalize(itemInstanceId);
            return bindings.FirstOrDefault(value => string.Equals(
                value.itemInstanceId,
                normalized,
                StringComparison.Ordinal));
        }

        public ItemInstancePlacementBindingSnapshot FindByPlacementId(string placementId)
        {
            string normalized = BindingCanonical.Normalize(placementId);
            return bindings.FirstOrDefault(value => string.Equals(
                value.placementId,
                normalized,
                StringComparison.Ordinal));
        }

        public string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            BindingCanonical.AppendField(builder, "schemaId", schemaId);
            BindingCanonical.AppendField(builder, "status", status.ToString());
            BindingCanonical.AppendField(builder, "bindingCount",
                bindings.Count.ToString(CultureInfo.InvariantCulture));
            BindingCanonical.AppendField(builder, "validationErrorCount",
                validationErrors.Count.ToString(CultureInfo.InvariantCulture));

            foreach (ItemInstancePlacementBindingSnapshot binding in bindings)
            {
                BindingCanonical.AppendField(builder, "itemInstanceId", binding.itemInstanceId);
                BindingCanonical.AppendField(builder, "placementId", binding.placementId);
                BindingCanonical.AppendField(builder, "baseItemId", binding.baseItemId);
            }

            foreach (ItemInstancePlacementBindingValidationError error in validationErrors)
            {
                BindingCanonical.AppendField(builder, "validationCode", error.code);
                BindingCanonical.AppendField(builder, "validationStatus", error.status.ToString());
                BindingCanonical.AppendField(builder, "validationItemInstanceId", error.itemInstanceId);
                BindingCanonical.AppendField(builder, "validationPlacementId", error.placementId);
                BindingCanonical.AppendField(builder, "validationBaseItemId", error.baseItemId);
                BindingCanonical.AppendField(builder, "validationMessage", error.message);
            }

            return builder.ToString();
        }
    }

    public sealed class ItemInstancePlacementBindingValidationResult
    {
        internal ItemInstancePlacementBindingValidationResult(
            ItemInstancePlacementBindingContractSnapshot snapshot)
        {
            this.snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        }

        public ItemInstancePlacementBindingContractSnapshot snapshot { get; }
        public ItemInstancePlacementBindingStatus status => snapshot.status;
        public bool isValid => snapshot.isValid;
        public IReadOnlyList<ItemInstancePlacementBindingValidationError> ValidationErrors =>
            snapshot.ValidationErrors;
        public string primaryValidationCode => snapshot.ValidationErrors.Count == 0
            ? ItemInstancePlacementBindingValidationCodes.None
            : snapshot.ValidationErrors[0].code;
    }

    internal static class BindingReadOnly
    {
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    internal static class BindingCanonical
    {
        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static void AppendField(StringBuilder builder, string key, string value)
        {
            string safeKey = key ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeKey.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeKey).Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append('\n');
        }

        public static string Sha256(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                return "sha256:" + BitConverter.ToString(digest)
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }
    }
}
