using System;

namespace TalismanBag.V04.RewardDrop.Contracts
{
    public enum RewardAcquisitionMode
    {
        Unspecified = 0,
        FirstClearOnline = 1,
        RepeatChallengeOnline = 2,
        OfflinePatrol = 3
    }

    public enum RewardLaunchContextKind
    {
        Unspecified = 0,
        CampaignNormal = 1,
        DevShowcase = 2
    }

    public enum RewardPersistencePolicy
    {
        Unspecified = 0,
        ProductClaimable = 1,
        DiagnosticsOnly = 2
    }

    public enum RewardSourceFactKind
    {
        Unspecified = 0,
        StageClear = 1,
        OfflinePatrolSettlement = 2
    }

    public enum RewardSubjectKind
    {
        Unspecified = 0,
        OrdinaryGeneratedItem = 1,
        OrdinaryResource = 2,
        SystemRhythmSpecial = 3,
        StoryCritical = 4,
        CosmeticSkin = 5,
        BossExclusive = 6
    }

    public enum RewardGrantKind
    {
        Unspecified = 0,
        OrdinaryRoll = 1,
        FirstClearFixed = 2,
        BossFirstClearGuaranteed = 3,
        SystemGrant = 4,
        StoryGrant = 5,
        CosmeticGrant = 6
    }

    public sealed class RewardLaunchContextIdentity
    {
        public const string SchemaId = "RewardLaunchContextIdentity.v1";
        public const string CampaignNormalLv1Id = "CAMPAIGN_NORMAL_LV1";
        public const string DevShowcaseLv40Id = "DEV_SHOWCASE_LV40";
        public const int InitialContextVersion = 1;

        public static RewardLaunchContextIdentity CampaignNormalLv1 { get; } =
            new RewardLaunchContextIdentity(
                CampaignNormalLv1Id,
                RewardLaunchContextKind.CampaignNormal,
                RewardPersistencePolicy.ProductClaimable,
                InitialContextVersion);

        public static RewardLaunchContextIdentity DevShowcaseLv40 { get; } =
            new RewardLaunchContextIdentity(
                DevShowcaseLv40Id,
                RewardLaunchContextKind.DevShowcase,
                RewardPersistencePolicy.DiagnosticsOnly,
                InitialContextVersion);

        public string schemaId { get; }
        public string launchContextId { get; }
        public RewardLaunchContextKind launchContextKind { get; }
        public RewardPersistencePolicy persistencePolicy { get; }
        public int contextVersion { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public RewardLaunchContextIdentity(
            string launchContextId,
            RewardLaunchContextKind launchContextKind,
            RewardPersistencePolicy persistencePolicy,
            int contextVersion)
        {
            schemaId = SchemaId;
            this.launchContextId = RewardDropCanonical.NormalizeIdentifier(launchContextId);
            this.launchContextKind = launchContextKind;
            this.persistencePolicy = persistencePolicy;
            this.contextVersion = contextVersion;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("launchContextId", this.launchContextId);
                builder.AddEnum("launchContextKind", (int)this.launchContextKind);
                builder.AddEnum("persistencePolicy", (int)this.persistencePolicy);
                builder.AddInt("contextVersion", this.contextVersion);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }

        public bool SemanticallyEquals(RewardLaunchContextIdentity other)
        {
            return other != null
                && RewardDropCanonical.EqualsOrdinal(launchContextId, other.launchContextId)
                && launchContextKind == other.launchContextKind
                && persistencePolicy == other.persistencePolicy
                && contextVersion == other.contextVersion;
        }
    }

    public sealed class RewardSubjectIdentity
    {
        public const string SchemaId = "RewardSubjectIdentity.v1";

        public string schemaId { get; }
        public string subjectId { get; }
        public RewardSubjectKind subjectKind { get; }
        public string sourceCatalogId { get; }
        public int classificationVersion { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public RewardSubjectIdentity(
            string subjectId,
            RewardSubjectKind subjectKind,
            string sourceCatalogId,
            int classificationVersion)
        {
            schemaId = SchemaId;
            this.subjectId = RewardDropCanonical.NormalizeIdentifier(subjectId);
            this.subjectKind = subjectKind;
            this.sourceCatalogId = RewardDropCanonical.NormalizeIdentifier(sourceCatalogId);
            this.classificationVersion = classificationVersion;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("subjectId", this.subjectId);
                builder.AddEnum("subjectKind", (int)this.subjectKind);
                builder.AddString("sourceCatalogId", this.sourceCatalogId);
                builder.AddInt("classificationVersion", this.classificationVersion);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }

        public bool SemanticallyEquals(RewardSubjectIdentity other)
        {
            return other != null
                && RewardDropCanonical.EqualsOrdinal(subjectId, other.subjectId)
                && subjectKind == other.subjectKind
                && RewardDropCanonical.EqualsOrdinal(sourceCatalogId, other.sourceCatalogId)
                && classificationVersion == other.classificationVersion;
        }
    }

    public sealed class DropRollSlot
    {
        public const string SchemaId = "DropRollSlot.v1";

        public string schemaId { get; }
        public string rollSlotId { get; }
        public RewardGrantKind grantKind { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public DropRollSlot(string rollSlotId, RewardGrantKind grantKind)
        {
            schemaId = SchemaId;
            this.rollSlotId = RewardDropCanonical.NormalizeIdentifier(rollSlotId);
            this.grantKind = grantKind;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("rollSlotId", this.rollSlotId);
                builder.AddEnum("grantKind", (int)this.grantKind);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }

    public sealed class DropRollEntry
    {
        public const string SchemaId = "DropRollEntry.v1";

        public string schemaId { get; }
        public string rollEntryId { get; }
        public string rollSlotId { get; }
        public RewardGrantKind grantKind { get; }
        public RewardSubjectIdentity rewardSubjectIdentity { get; }
        public string itemInstanceId { get; }
        public string instanceCanonicalSignature { get; }
        public int quantity { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public DropRollEntry(
            string rollEntryId,
            string rollSlotId,
            RewardGrantKind grantKind,
            RewardSubjectIdentity rewardSubjectIdentity,
            string itemInstanceId,
            string instanceCanonicalSignature,
            int quantity)
        {
            schemaId = SchemaId;
            this.rollEntryId = RewardDropCanonical.NormalizeIdentifier(rollEntryId);
            this.rollSlotId = RewardDropCanonical.NormalizeIdentifier(rollSlotId);
            this.grantKind = grantKind;
            this.rewardSubjectIdentity = rewardSubjectIdentity;
            this.itemInstanceId = RewardDropCanonical.NormalizeIdentifier(itemInstanceId);
            this.instanceCanonicalSignature =
                RewardDropCanonical.NormalizeIdentifier(instanceCanonicalSignature);
            this.quantity = quantity;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("rollEntryId", this.rollEntryId);
                builder.AddString("rollSlotId", this.rollSlotId);
                builder.AddEnum("grantKind", (int)this.grantKind);
                RewardDropCanonical.AddSubject(
                    builder,
                    "rewardSubjectIdentity",
                    this.rewardSubjectIdentity);
                builder.AddString("itemInstanceId", this.itemInstanceId);
                builder.AddString(
                    "instanceCanonicalSignature",
                    this.instanceCanonicalSignature);
                builder.AddInt("quantity", this.quantity);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }

    public sealed class RewardEntry
    {
        public const string SchemaId = "RewardEntry.v1";

        public string schemaId { get; }
        public string rewardEntryId { get; }
        public RewardGrantKind grantKind { get; }
        public RewardSubjectIdentity rewardSubjectIdentity { get; }
        public string sourceDefinitionId { get; }
        public int sourceRuleVersion { get; }
        public string itemInstanceId { get; }
        public string instanceCanonicalSignature { get; }
        public int quantity { get; }
        public string canonicalPayload { get; }
        public string canonicalSignature { get; }

        public RewardEntry(
            string rewardEntryId,
            RewardGrantKind grantKind,
            RewardSubjectIdentity rewardSubjectIdentity,
            string sourceDefinitionId,
            int sourceRuleVersion,
            string itemInstanceId,
            string instanceCanonicalSignature,
            int quantity)
        {
            schemaId = SchemaId;
            this.rewardEntryId = RewardDropCanonical.NormalizeIdentifier(rewardEntryId);
            this.grantKind = grantKind;
            this.rewardSubjectIdentity = rewardSubjectIdentity;
            this.sourceDefinitionId =
                RewardDropCanonical.NormalizeIdentifier(sourceDefinitionId);
            this.sourceRuleVersion = sourceRuleVersion;
            this.itemInstanceId = RewardDropCanonical.NormalizeIdentifier(itemInstanceId);
            this.instanceCanonicalSignature =
                RewardDropCanonical.NormalizeIdentifier(instanceCanonicalSignature);
            this.quantity = quantity;
            canonicalPayload = RewardDropCanonical.CreatePayload(builder =>
            {
                builder.AddString("schemaId", schemaId);
                builder.AddString("rewardEntryId", this.rewardEntryId);
                builder.AddEnum("grantKind", (int)this.grantKind);
                RewardDropCanonical.AddSubject(
                    builder,
                    "rewardSubjectIdentity",
                    this.rewardSubjectIdentity);
                builder.AddString("sourceDefinitionId", this.sourceDefinitionId);
                builder.AddInt("sourceRuleVersion", this.sourceRuleVersion);
                builder.AddString("itemInstanceId", this.itemInstanceId);
                builder.AddString(
                    "instanceCanonicalSignature",
                    this.instanceCanonicalSignature);
                builder.AddInt("quantity", this.quantity);
            });
            canonicalSignature = RewardDropCanonical.ComputeSignature(canonicalPayload);
        }
    }
}
