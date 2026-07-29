using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.EnemySystem.BoneAspect.CarrierPresentation
{
    public static class BoneAspectCarrierPresentationCatalogSchema
    {
        public const string SchemaId = "BoneAspectCarrierPresentationCatalog.v1";
        public const int SchemaVersion = 1;
        public const bool DevOnly = true;
        public const bool IsEnabled = false;
        public const bool EntersFormalFlow = false;
        public const bool RuntimeImplemented = false;
    }

    public enum BoneAspectCarrierKind
    {
        Enemy = 0,
        Boss = 1
    }

    public enum BoneAspectArtDeliveryStatus
    {
        RequiredByLockedDesign = 0,
        BlockedByUserDecision = 1
    }

    public enum BoneAspectMechanicReferenceBindingStatus
    {
        CandidateReuseOnly = 0
    }

    public sealed class BoneAspectCarrierPresentationValidationIssue
    {
        public BoneAspectCarrierPresentationValidationIssue(string code, string path, string message)
        {
            Code = code ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public string Path { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Code + " @ " + Path + ": " + Message;
        }
    }

    public sealed class BoneAspectCarrierPresentationValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<BoneAspectCarrierPresentationValidationIssue> issues;

        public BoneAspectCarrierPresentationValidationException(
            IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly((issues
                ?? Array.Empty<BoneAspectCarrierPresentationValidationIssue>()).ToArray());
        }

        public IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> Issues => issues;

        private static string BuildMessage(
            IReadOnlyList<BoneAspectCarrierPresentationValidationIssue> issues)
        {
            return "Bone Aspect Carrier Presentation Catalog validation failed: "
                + string.Join(" | ", (issues
                    ?? Array.Empty<BoneAspectCarrierPresentationValidationIssue>())
                    .Select(value => value == null ? "<null>" : value.ToString()));
        }
    }
}
