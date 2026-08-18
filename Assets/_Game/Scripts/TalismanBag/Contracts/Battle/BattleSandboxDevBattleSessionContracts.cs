using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Resource;

namespace TalismanBag.Contracts.Battle
{
    public enum BattleSandboxDevBattleSessionDecision
    {
        Rejected = 0,
        Accepted = 1
    }

    public enum BattleSandboxDevBattleSessionOutcome
    {
        Running = 0,
        Victory = 1,
        Defeat = 2,
        Timeout = 3
    }

    public enum BattleSandboxDevBattleLedgerKind
    {
        Started = 0,
        NianPulse = 1,
        ItemApplicationAccepted = 2,
        ItemApplicationRejected = 3,
        EnemyBasicAttack = 4,
        EnemySkill = 5,
        Result = 6
    }

    public enum BattleSandboxDevBattleCueKind
    {
        BattleStarted = 0,
        Build2Activated = 1,
        NianGenerated = 2,
        ItemApplicationAccepted = 3,
        ItemApplicationRejected = 4,
        EnemyBasicAttack = 5,
        EnemySkill = 6,
        BattleVictory = 7,
        BattleDefeat = 8,
        BattleTimeout = 9
    }

    public sealed class BattleSandboxDevBattleItemRequestFact
    {
        public BattleSandboxDevBattleItemRequestFact(
            string requestId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            string sourcePlacementId,
            string sourceProjectionCanonicalSignature,
            string nianCostRequestFactId,
            int shapeCellCount,
            long baseDamageUnits,
            long resolvedDamageUnits)
        {
            RequestId = requestId ?? string.Empty;
            SourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            SourceBaseItemId = sourceBaseItemId ?? string.Empty;
            SourcePlacementId = sourcePlacementId ?? string.Empty;
            SourceProjectionCanonicalSignature =
                sourceProjectionCanonicalSignature ?? string.Empty;
            NianCostRequestFactId = nianCostRequestFactId ?? string.Empty;
            ShapeCellCount = shapeCellCount;
            BaseDamageUnits = baseDamageUnits;
            ResolvedDamageUnits = resolvedDamageUnits;
        }

        public string RequestId { get; }
        public string SourceItemInstanceId { get; }
        public string SourceBaseItemId { get; }
        public string SourcePlacementId { get; }
        public string SourceProjectionCanonicalSignature { get; }
        public string NianCostRequestFactId { get; }
        public int ShapeCellCount { get; }
        public long BaseDamageUnits { get; }
        public long ResolvedDamageUnits { get; }
    }

    public sealed class BattleSandboxDevBattleLiveSignatures
    {
        public BattleSandboxDevBattleLiveSignatures(
            string rosterAvailabilitySignature,
            string boardSnapshotSignature,
            string qualifiedBuildSignature,
            string coreRuntimeSignature,
            string i031SourceSignature)
        {
            RosterAvailabilitySignature =
                rosterAvailabilitySignature ?? string.Empty;
            BoardSnapshotSignature = boardSnapshotSignature ?? string.Empty;
            QualifiedBuildSignature = qualifiedBuildSignature ?? string.Empty;
            CoreRuntimeSignature = coreRuntimeSignature ?? string.Empty;
            I031SourceSignature = i031SourceSignature ?? string.Empty;
        }

        public string RosterAvailabilitySignature { get; }
        public string BoardSnapshotSignature { get; }
        public string QualifiedBuildSignature { get; }
        public string CoreRuntimeSignature { get; }
        public string I031SourceSignature { get; }

        public bool Matches(BattleSandboxDevBattleLiveSignatures other)
        {
            return other != null
                && string.Equals(RosterAvailabilitySignature,
                    other.RosterAvailabilitySignature,
                    StringComparison.Ordinal)
                && string.Equals(BoardSnapshotSignature,
                    other.BoardSnapshotSignature,
                    StringComparison.Ordinal)
                && string.Equals(QualifiedBuildSignature,
                    other.QualifiedBuildSignature,
                    StringComparison.Ordinal)
                && string.Equals(CoreRuntimeSignature,
                    other.CoreRuntimeSignature,
                    StringComparison.Ordinal)
                && string.Equals(I031SourceSignature,
                    other.I031SourceSignature,
                    StringComparison.Ordinal);
        }
    }

    public sealed class BattleSandboxDevBattleSessionRequest
    {
        public const string CurrentSchemaId =
            "BattleSandboxDevBattleSessionRequest.v1";
        public const string RequiredProductContext =
            "PLAYTEST_VERTICAL_SLICE";
        public const string ApprovedHostContext = "CORE_LOOP_LAB";
        public const string ApprovedHostPackageId =
            "V0.4-CoreLoopABCProductDirectionLab01";
        public const string ProfileA = "A";
        public const string ProfileB = "B";
        public const string ProfileC = "C";
        public const string SingleBattleMechanics = "SINGLE_BATTLE";
        public const string TwoBattleMechanics = "TWO_BATTLE_REWARD";

        private readonly ReadOnlyCollection<string> availableBaseItemIds;
        private readonly ReadOnlyCollection<string> availableIdentityIds;
        private readonly ReadOnlyCollection<string> liHuoSourceBaseItemIds;
        private readonly ReadOnlyCollection<BattleSandboxDevBattleItemRequestFact>
            itemRequestFacts;

        public BattleSandboxDevBattleSessionRequest(
            string schemaId,
            bool devOnly,
            string productContext,
            string hostContext,
            string hostPackageId,
            bool requestsFormalFlow,
            bool requestsCampaignFlow,
            bool requestsPlayerOrApk,
            string hostSessionToken,
            int resetGeneration,
            string labProfileId,
            string mechanicsProfileId,
            int battleIndex,
            string selectedRewardBaseItemId,
            string selectedRewardIdentityId,
            BattleSandboxExplicitDevEncounterRequest encounter,
            BattleSandboxDevBattleLiveSignatures sourceSignatures,
            IReadOnlyList<string> availableBaseItemIds,
            IReadOnlyList<string> availableIdentityIds,
            int liHuoQualifiedItemCount,
            int liHuoActiveStagePieceCount,
            IReadOnlyList<string> liHuoSourceBaseItemIds,
            bool liHuoBuild2Active,
            I031NianSourceSnapshot i031Source,
            IReadOnlyList<BattleSandboxDevBattleItemRequestFact>
                itemRequestFacts)
        {
            SchemaId = schemaId ?? string.Empty;
            DevOnly = devOnly;
            ProductContext = productContext ?? string.Empty;
            HostContext = hostContext ?? string.Empty;
            HostPackageId = hostPackageId ?? string.Empty;
            RequestsFormalFlow = requestsFormalFlow;
            RequestsCampaignFlow = requestsCampaignFlow;
            RequestsPlayerOrApk = requestsPlayerOrApk;
            HostSessionToken = hostSessionToken ?? string.Empty;
            ResetGeneration = resetGeneration;
            LabProfileId = labProfileId ?? string.Empty;
            MechanicsProfileId = mechanicsProfileId ?? string.Empty;
            BattleIndex = battleIndex;
            SelectedRewardBaseItemId =
                selectedRewardBaseItemId ?? string.Empty;
            SelectedRewardIdentityId = selectedRewardIdentityId ?? string.Empty;
            Encounter = encounter;
            SourceSignatures = sourceSignatures;
            this.availableBaseItemIds = FreezeText(availableBaseItemIds);
            this.availableIdentityIds = FreezeText(availableIdentityIds);
            LiHuoQualifiedItemCount = liHuoQualifiedItemCount;
            LiHuoActiveStagePieceCount = liHuoActiveStagePieceCount;
            this.liHuoSourceBaseItemIds = FreezeText(liHuoSourceBaseItemIds);
            LiHuoBuild2Active = liHuoBuild2Active;
            I031Source = i031Source;
            this.itemRequestFacts = new ReadOnlyCollection<
                BattleSandboxDevBattleItemRequestFact>(
                (itemRequestFacts
                    ?? Array.Empty<BattleSandboxDevBattleItemRequestFact>())
                .Where(value => value != null)
                .OrderBy(value => value.SourceItemInstanceId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.SourcePlacementId,
                    StringComparer.Ordinal)
                .ToList());
            StartKey = BattleSandboxDevBattleCanonical.ComputeStartKey(this);
        }

        public string SchemaId { get; }
        public bool DevOnly { get; }
        public string ProductContext { get; }
        public string HostContext { get; }
        public string HostPackageId { get; }
        public bool RequestsFormalFlow { get; }
        public bool RequestsCampaignFlow { get; }
        public bool RequestsPlayerOrApk { get; }
        public string HostSessionToken { get; }
        public int ResetGeneration { get; }
        public string LabProfileId { get; }
        public string MechanicsProfileId { get; }
        public int BattleIndex { get; }
        public string SelectedRewardBaseItemId { get; }
        public string SelectedRewardIdentityId { get; }
        public BattleSandboxExplicitDevEncounterRequest Encounter { get; }
        public BattleSandboxDevBattleLiveSignatures SourceSignatures { get; }
        public IReadOnlyList<string> AvailableBaseItemIds => availableBaseItemIds;
        public IReadOnlyList<string> AvailableIdentityIds => availableIdentityIds;
        public int LiHuoQualifiedItemCount { get; }
        public int LiHuoActiveStagePieceCount { get; }
        public IReadOnlyList<string> LiHuoSourceBaseItemIds =>
            liHuoSourceBaseItemIds;
        public bool LiHuoBuild2Active { get; }
        public I031NianSourceSnapshot I031Source { get; }
        public IReadOnlyList<BattleSandboxDevBattleItemRequestFact>
            ItemRequestFacts => itemRequestFacts;
        public string StartKey { get; }

        private static ReadOnlyCollection<string> FreezeText(
            IEnumerable<string> values)
        {
            return new ReadOnlyCollection<string>((values
                    ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList());
        }
    }

    public sealed class BattleSandboxDevBattleLedgerEntry
    {
        public BattleSandboxDevBattleLedgerEntry(
            int sequence,
            long battleTick,
            BattleSandboxDevBattleLedgerKind kind,
            string eventId,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            int nianBefore,
            int nianAfter,
            int enemyHpBefore,
            int enemyHpAfter,
            int playerShieldBefore,
            int playerShieldAfter,
            int playerHpBefore,
            int playerHpAfter,
            string reasonCode)
        {
            Sequence = sequence;
            BattleTick = battleTick;
            Kind = kind;
            EventId = eventId ?? string.Empty;
            SourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            SourceBaseItemId = sourceBaseItemId ?? string.Empty;
            NianBefore = nianBefore;
            NianAfter = nianAfter;
            EnemyHpBefore = enemyHpBefore;
            EnemyHpAfter = enemyHpAfter;
            PlayerShieldBefore = playerShieldBefore;
            PlayerShieldAfter = playerShieldAfter;
            PlayerHpBefore = playerHpBefore;
            PlayerHpAfter = playerHpAfter;
            ReasonCode = reasonCode ?? string.Empty;
        }

        public int Sequence { get; }
        public long BattleTick { get; }
        public BattleSandboxDevBattleLedgerKind Kind { get; }
        public string EventId { get; }
        public string SourceItemInstanceId { get; }
        public string SourceBaseItemId { get; }
        public int NianBefore { get; }
        public int NianAfter { get; }
        public int EnemyHpBefore { get; }
        public int EnemyHpAfter { get; }
        public int PlayerShieldBefore { get; }
        public int PlayerShieldAfter { get; }
        public int PlayerHpBefore { get; }
        public int PlayerHpAfter { get; }
        public string ReasonCode { get; }
    }

    public sealed class BattleSandboxDevBattlePresentationCue
    {
        public BattleSandboxDevBattlePresentationCue(
            int sequence,
            long battleTick,
            BattleSandboxDevBattleCueKind kind,
            string sourceItemInstanceId,
            string sourceBaseItemId,
            int nianDelta,
            int enemyHpDelta,
            int playerShieldDelta,
            int playerHpDelta,
            string semanticKey)
        {
            Sequence = sequence;
            BattleTick = battleTick;
            Kind = kind;
            SourceItemInstanceId = sourceItemInstanceId ?? string.Empty;
            SourceBaseItemId = sourceBaseItemId ?? string.Empty;
            NianDelta = nianDelta;
            EnemyHpDelta = enemyHpDelta;
            PlayerShieldDelta = playerShieldDelta;
            PlayerHpDelta = playerHpDelta;
            SemanticKey = semanticKey ?? string.Empty;
        }

        public int Sequence { get; }
        public long BattleTick { get; }
        public BattleSandboxDevBattleCueKind Kind { get; }
        public string SourceItemInstanceId { get; }
        public string SourceBaseItemId { get; }
        public int NianDelta { get; }
        public int EnemyHpDelta { get; }
        public int PlayerShieldDelta { get; }
        public int PlayerHpDelta { get; }
        public string SemanticKey { get; }
    }

    public sealed class BattleSandboxDevBattleSessionSnapshot
    {
        private readonly ReadOnlyCollection<BattleSandboxDevBattleLedgerEntry>
            ledger;
        private readonly ReadOnlyCollection<BattleSandboxDevBattlePresentationCue>
            presentationCues;

        public BattleSandboxDevBattleSessionSnapshot(
            long runtimeGeneration,
            long runtimeRevision,
            string labProfileId,
            string mechanicsProfileId,
            int battleIndex,
            string selectedRewardBaseItemId,
            string encounterProfileId,
            string encounterProfileFingerprint,
            string enemyIdentity,
            long elapsedMilliseconds,
            int playerMaxHp,
            int playerStartShield,
            int playerCurrentHp,
            int playerCurrentShield,
            int enemyMaxHp,
            int enemyCurrentHp,
            int nianStart,
            int nianCurrent,
            int nianGenerated,
            int nianSpent,
            int nianRejected,
            int acceptedItemApplicationCount,
            int rejectedItemApplicationCount,
            int acceptedEnemyBasicAttackCount,
            int acceptedEnemySkillCount,
            int acceptedEnemyPlayerDamage,
            BattleSandboxDevBattleSessionOutcome outcome,
            IReadOnlyList<BattleSandboxDevBattleLedgerEntry> ledger,
            IReadOnlyList<BattleSandboxDevBattlePresentationCue>
                presentationCues)
        {
            RuntimeGeneration = runtimeGeneration;
            RuntimeRevision = runtimeRevision;
            LabProfileId = labProfileId ?? string.Empty;
            MechanicsProfileId = mechanicsProfileId ?? string.Empty;
            BattleIndex = battleIndex;
            SelectedRewardBaseItemId =
                selectedRewardBaseItemId ?? string.Empty;
            EncounterProfileId = encounterProfileId ?? string.Empty;
            EncounterProfileFingerprint =
                encounterProfileFingerprint ?? string.Empty;
            EnemyIdentity = enemyIdentity ?? string.Empty;
            ElapsedMilliseconds = elapsedMilliseconds;
            PlayerMaxHp = playerMaxHp;
            PlayerStartShield = playerStartShield;
            PlayerCurrentHp = playerCurrentHp;
            PlayerCurrentShield = playerCurrentShield;
            EnemyMaxHp = enemyMaxHp;
            EnemyCurrentHp = enemyCurrentHp;
            NianStart = nianStart;
            NianCurrent = nianCurrent;
            NianGenerated = nianGenerated;
            NianSpent = nianSpent;
            NianRejected = nianRejected;
            AcceptedItemApplicationCount = acceptedItemApplicationCount;
            RejectedItemApplicationCount = rejectedItemApplicationCount;
            AcceptedEnemyBasicAttackCount = acceptedEnemyBasicAttackCount;
            AcceptedEnemySkillCount = acceptedEnemySkillCount;
            AcceptedEnemyPlayerDamage = acceptedEnemyPlayerDamage;
            Outcome = outcome;
            this.ledger = new ReadOnlyCollection<
                BattleSandboxDevBattleLedgerEntry>((ledger
                    ?? Array.Empty<BattleSandboxDevBattleLedgerEntry>())
                .Where(value => value != null).ToList());
            this.presentationCues = new ReadOnlyCollection<
                BattleSandboxDevBattlePresentationCue>((presentationCues
                    ?? Array.Empty<BattleSandboxDevBattlePresentationCue>())
                .Where(value => value != null).ToList());
            NonPresentationLedgerFingerprint =
                BattleSandboxDevBattleCanonical.ComputeLedgerFingerprint(
                    this.ledger);
        }

        public long RuntimeGeneration { get; }
        public long RuntimeRevision { get; }
        public string LabProfileId { get; }
        public string MechanicsProfileId { get; }
        public int BattleIndex { get; }
        public string SelectedRewardBaseItemId { get; }
        public string EncounterProfileId { get; }
        public string EncounterProfileFingerprint { get; }
        public string EnemyIdentity { get; }
        public long ElapsedMilliseconds { get; }
        public int PlayerMaxHp { get; }
        public int PlayerStartShield { get; }
        public int PlayerCurrentHp { get; }
        public int PlayerCurrentShield { get; }
        public int EnemyMaxHp { get; }
        public int EnemyCurrentHp { get; }
        public string EnemyState => EnemyCurrentHp <= 0 ? "DEAD" : "ALIVE";
        public int NianStart { get; }
        public int NianCurrent { get; }
        public int NianGenerated { get; }
        public int NianSpent { get; }
        public int NianRejected { get; }
        public int AcceptedItemApplicationCount { get; }
        public int RejectedItemApplicationCount { get; }
        public int AcceptedEnemyBasicAttackCount { get; }
        public int AcceptedEnemySkillCount { get; }
        public int AcceptedEnemyPlayerDamage { get; }
        public BattleSandboxDevBattleSessionOutcome Outcome { get; }
        public bool IsTerminal => Outcome !=
            BattleSandboxDevBattleSessionOutcome.Running;
        public bool Victory => Outcome ==
            BattleSandboxDevBattleSessionOutcome.Victory;
        public bool Defeat => Outcome ==
            BattleSandboxDevBattleSessionOutcome.Defeat;
        public IReadOnlyList<BattleSandboxDevBattleLedgerEntry> Ledger => ledger;
        public IReadOnlyList<BattleSandboxDevBattlePresentationCue>
            PresentationCues => presentationCues;
        public string NonPresentationLedgerFingerprint { get; }
    }

    public sealed class BattleSandboxDevBattleSessionAcceptedStart
    {
        public const string CurrentSchemaId =
            "BattleSandboxDevBattleSessionAcceptedStart.v1";

        public BattleSandboxDevBattleSessionAcceptedStart(
            string startKey,
            int acceptedResetGeneration,
            string acceptedHostSessionToken,
            int acceptedBattleIndex,
            string acceptedEncounterProfileId,
            string acceptedEncounterProfileFingerprint,
            BattleSandboxDevBattleSessionSnapshot startSnapshot)
        {
            StartKey = startKey ?? string.Empty;
            AcceptedResetGeneration = acceptedResetGeneration;
            AcceptedHostSessionToken = acceptedHostSessionToken ?? string.Empty;
            AcceptedBattleIndex = acceptedBattleIndex;
            AcceptedEncounterProfileId =
                acceptedEncounterProfileId ?? string.Empty;
            AcceptedEncounterProfileFingerprint =
                acceptedEncounterProfileFingerprint ?? string.Empty;
            StartSnapshot = startSnapshot;
        }

        public string SchemaId => CurrentSchemaId;
        public string StartKey { get; }
        public int AcceptedResetGeneration { get; }
        public string AcceptedHostSessionToken { get; }
        public int AcceptedBattleIndex { get; }
        public string AcceptedEncounterProfileId { get; }
        public string AcceptedEncounterProfileFingerprint { get; }
        public BattleSandboxDevBattleSessionSnapshot StartSnapshot { get; }
    }

    public static class BattleSandboxDevBattleSessionContract
    {
        private static readonly string[] ControlledBaseIds =
        {
            "I031", "I007", "I008", "I009", "I010", "I011", "I012"
        };

        public static bool Validate(
            BattleSandboxDevBattleSessionRequest request,
            bool isEditor,
            out string diagnosticCode)
        {
            diagnosticCode = "NONE";
            if (request == null)
            {
                diagnosticCode = "DEV_SESSION_REQUEST_MISSING";
                return false;
            }
            if (!string.Equals(request.SchemaId,
                    BattleSandboxDevBattleSessionRequest.CurrentSchemaId,
                    StringComparison.Ordinal)
                || !request.DevOnly)
            {
                diagnosticCode = "DEV_SESSION_SCHEMA_OR_DEV_ONLY_REJECTED";
                return false;
            }
            if (!isEditor || request.RequestsPlayerOrApk)
            {
                diagnosticCode = "DEV_SESSION_PLAYER_OR_APK_REJECTED";
                return false;
            }
            if (request.RequestsFormalFlow || request.RequestsCampaignFlow)
            {
                diagnosticCode = "DEV_SESSION_FORMAL_OR_CAMPAIGN_REJECTED";
                return false;
            }
            if (!string.Equals(request.ProductContext,
                    BattleSandboxDevBattleSessionRequest.RequiredProductContext,
                    StringComparison.Ordinal)
                || !string.Equals(request.HostContext,
                    BattleSandboxDevBattleSessionRequest.ApprovedHostContext,
                    StringComparison.Ordinal)
                || !string.Equals(request.HostPackageId,
                    BattleSandboxDevBattleSessionRequest.ApprovedHostPackageId,
                    StringComparison.Ordinal))
            {
                diagnosticCode = "DEV_SESSION_HOST_OR_CONTEXT_REJECTED";
                return false;
            }
            if (string.IsNullOrWhiteSpace(request.HostSessionToken)
                || request.ResetGeneration <= 0
                || request.BattleIndex < 1 || request.BattleIndex > 2)
            {
                diagnosticCode = "DEV_SESSION_GENERATION_TOKEN_OR_INDEX_INVALID";
                return false;
            }
            if (!ValidateProfile(request))
            {
                diagnosticCode = "DEV_SESSION_PROFILE_FLOW_INVALID";
                return false;
            }
            if (!BattleSandboxExplicitDevEncounterContract.Validate(
                    request.Encounter,
                    isEditor,
                    out _,
                    out string encounterDiagnostic))
            {
                diagnosticCode = "DEV_SESSION_ENCOUNTER_REJECTED_"
                    + encounterDiagnostic;
                return false;
            }
            if (request.SourceSignatures == null
                || string.IsNullOrWhiteSpace(
                    request.SourceSignatures.RosterAvailabilitySignature)
                || string.IsNullOrWhiteSpace(
                    request.SourceSignatures.BoardSnapshotSignature)
                || string.IsNullOrWhiteSpace(
                    request.SourceSignatures.QualifiedBuildSignature)
                || string.IsNullOrWhiteSpace(
                    request.SourceSignatures.CoreRuntimeSignature)
                || string.IsNullOrWhiteSpace(
                    request.SourceSignatures.I031SourceSignature))
            {
                diagnosticCode = "DEV_SESSION_SOURCE_SIGNATURE_MISSING";
                return false;
            }
            if (request.I031Source?.status != I031NianSourceStatus.Valid
                || !request.I031Source.isEligible
                || request.I031Source.generationPerPulse != 4
                || request.I031Source.sourceGeneration
                    != request.ResetGeneration
                || !string.Equals(request.I031Source.canonicalSignature,
                    request.SourceSignatures.I031SourceSignature,
                    StringComparison.Ordinal))
            {
                diagnosticCode = "DEV_SESSION_I031_SOURCE_INVALID";
                return false;
            }
            if (request.AvailableBaseItemIds.Any(id =>
                    !ControlledBaseIds.Contains(id, StringComparer.Ordinal))
                || request.ItemRequestFacts.Count == 0)
            {
                diagnosticCode = "DEV_SESSION_CONTROLLED_ITEM_FACTS_INVALID";
                return false;
            }
            if (!ValidateBuild(request))
            {
                diagnosticCode = "DEV_SESSION_QUALIFIED_BUILD_INVALID";
                return false;
            }

            HashSet<string> requestIds = new(StringComparer.Ordinal);
            foreach (BattleSandboxDevBattleItemRequestFact fact
                     in request.ItemRequestFacts)
            {
                I031NianCostRequestFact cost =
                    request.I031Source.FindCostFact(fact.SourceBaseItemId);
                if (string.IsNullOrWhiteSpace(fact.RequestId)
                    || !requestIds.Add(fact.RequestId)
                    || string.IsNullOrWhiteSpace(fact.SourceItemInstanceId)
                    || string.IsNullOrWhiteSpace(fact.SourcePlacementId)
                    || string.IsNullOrWhiteSpace(
                        fact.SourceProjectionCanonicalSignature)
                    || cost == null
                    || !string.Equals(fact.NianCostRequestFactId,
                        cost.requestFactId,
                        StringComparison.Ordinal)
                    || fact.ShapeCellCount <= 0
                    || fact.BaseDamageUnits <= 0
                    || fact.ResolvedDamageUnits <= 0
                    || !request.AvailableBaseItemIds.Contains(
                        fact.SourceBaseItemId,
                        StringComparer.Ordinal))
                {
                    diagnosticCode = "DEV_SESSION_ITEM_REQUEST_FACT_INVALID";
                    return false;
                }
                if ((string.Equals(fact.SourceBaseItemId, "I007",
                         StringComparison.Ordinal)
                        || string.Equals(fact.SourceBaseItemId, "I009",
                            StringComparison.Ordinal))
                    && fact.ShapeCellCount != 1)
                {
                    diagnosticCode = "DEV_SESSION_ONE_CELL_FACT_MISMATCH";
                    return false;
                }
                if (string.Equals(fact.SourceBaseItemId, "I012",
                        StringComparison.Ordinal)
                    && fact.ShapeCellCount != 2)
                {
                    diagnosticCode = "DEV_SESSION_I012_CELL_FACT_MISMATCH";
                    return false;
                }
                if (!ValidateDamageLineage(
                        fact,
                        request.BattleIndex,
                        out diagnosticCode))
                {
                    return false;
                }
            }

            if (!ValidateFrozenCosts(request.I031Source))
            {
                diagnosticCode = "DEV_SESSION_NIAN_COST_FACTS_MISMATCH";
                return false;
            }

            return true;
        }

        private static bool ValidateProfile(
            BattleSandboxDevBattleSessionRequest request)
        {
            bool profileA = string.Equals(request.LabProfileId,
                BattleSandboxDevBattleSessionRequest.ProfileA,
                StringComparison.Ordinal);
            bool profileB = string.Equals(request.LabProfileId,
                BattleSandboxDevBattleSessionRequest.ProfileB,
                StringComparison.Ordinal);
            bool profileC = string.Equals(request.LabProfileId,
                BattleSandboxDevBattleSessionRequest.ProfileC,
                StringComparison.Ordinal);
            if ((!profileA && !profileB && !profileC)
                || (profileA && request.BattleIndex != 1)
                || (profileA && !string.Equals(request.MechanicsProfileId,
                    BattleSandboxDevBattleSessionRequest.SingleBattleMechanics,
                    StringComparison.Ordinal))
                || ((profileB || profileC)
                    && !string.Equals(request.MechanicsProfileId,
                        BattleSandboxDevBattleSessionRequest.TwoBattleMechanics,
                        StringComparison.Ordinal)))
            {
                return false;
            }

            bool secondBattle = request.BattleIndex == 2;
            if (!secondBattle)
            {
                return string.IsNullOrWhiteSpace(
                    request.SelectedRewardBaseItemId)
                    && string.IsNullOrWhiteSpace(
                        request.SelectedRewardIdentityId);
            }

            return (profileB || profileC)
                && new[] { "I007", "I009", "I012" }.Contains(
                    request.SelectedRewardBaseItemId,
                    StringComparer.Ordinal)
                && !string.IsNullOrWhiteSpace(
                    request.SelectedRewardIdentityId);
        }

        private static bool ValidateBuild(
            BattleSandboxDevBattleSessionRequest request)
        {
            string[] expected = request.BattleIndex == 1
                ? new[] { "I010" }
                : new[] { "I010", request.SelectedRewardBaseItemId };
            return request.LiHuoSourceBaseItemIds.SequenceEqual(
                    expected.OrderBy(value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal)
                && request.LiHuoQualifiedItemCount == expected.Length
                && (request.BattleIndex == 1
                    ? !request.LiHuoBuild2Active
                        && request.LiHuoActiveStagePieceCount < 2
                    : request.LiHuoBuild2Active
                        && request.LiHuoActiveStagePieceCount >= 2);
        }

        private static bool ValidateFrozenCosts(I031NianSourceSnapshot source)
        {
            return Cost(source, "I007") == 2
                && Cost(source, "I008") == 3
                && Cost(source, "I009") == 5
                && Cost(source, "I010") == 3
                && Cost(source, "I011") == 3
                && Cost(source, "I012") == 6;
        }

        private static bool ValidateDamageLineage(
            BattleSandboxDevBattleItemRequestFact fact,
            int battleIndex,
            out string diagnosticCode)
        {
            diagnosticCode = "NONE";
            long expectedBase;
            long expectedResolved;
            switch (fact.SourceBaseItemId)
            {
                case "I007":
                    expectedBase = 24L;
                    expectedResolved = 29L;
                    break;
                case "I009":
                    expectedBase = 44L;
                    expectedResolved = 53L;
                    break;
                case "I012":
                    expectedBase = 62L;
                    expectedResolved = 76L;
                    break;
                case "I010":
                    expectedBase = 37L;
                    expectedResolved = battleIndex == 1 ? 43L : 46L;
                    break;
                default:
                    return true;
            }

            if (fact.BaseDamageUnits != expectedBase)
            {
                diagnosticCode = "DEV_SESSION_" + fact.SourceBaseItemId
                    + "_BASE_DAMAGE_MISMATCH";
                return false;
            }
            if (fact.ResolvedDamageUnits != expectedResolved)
            {
                diagnosticCode = "DEV_SESSION_" + fact.SourceBaseItemId
                    + "_RESOLVED_DAMAGE_MISMATCH";
                return false;
            }

            return true;
        }

        private static int Cost(I031NianSourceSnapshot source, string id)
        {
            return source?.FindCostFact(id)?.nianCost ?? -1;
        }
    }

    internal static class BattleSandboxDevBattleCanonical
    {
        public static string ComputeStartKey(
            BattleSandboxDevBattleSessionRequest request)
        {
            if (request == null)
            {
                return string.Empty;
            }
            StringBuilder text = new();
            Append(text, request.HostSessionToken);
            Append(text, request.ResetGeneration);
            Append(text, request.LabProfileId);
            Append(text, request.MechanicsProfileId);
            Append(text, request.BattleIndex);
            Append(text, request.SelectedRewardBaseItemId);
            Append(text, request.SelectedRewardIdentityId);
            Append(text, request.Encounter?.ProfileFingerprint);
            Append(text, request.SourceSignatures?.RosterAvailabilitySignature);
            Append(text, request.SourceSignatures?.BoardSnapshotSignature);
            Append(text, request.SourceSignatures?.QualifiedBuildSignature);
            Append(text, request.SourceSignatures?.CoreRuntimeSignature);
            Append(text, request.SourceSignatures?.I031SourceSignature);
            foreach (BattleSandboxDevBattleItemRequestFact fact
                     in request.ItemRequestFacts)
            {
                Append(text, fact.RequestId);
                Append(text, fact.SourceItemInstanceId);
                Append(text, fact.SourceBaseItemId);
                Append(text, fact.SourcePlacementId);
                Append(text, fact.SourceProjectionCanonicalSignature);
                Append(text, fact.NianCostRequestFactId);
                Append(text, fact.ShapeCellCount);
                Append(text, fact.BaseDamageUnits);
                Append(text, fact.ResolvedDamageUnits);
            }
            return Sha256(text.ToString());
        }

        public static string ComputeLedgerFingerprint(
            IEnumerable<BattleSandboxDevBattleLedgerEntry> entries)
        {
            StringBuilder text = new();
            foreach (BattleSandboxDevBattleLedgerEntry entry in entries
                         ?? Array.Empty<BattleSandboxDevBattleLedgerEntry>())
            {
                Append(text, entry.Sequence);
                Append(text, entry.BattleTick);
                Append(text, (int)entry.Kind);
                Append(text, entry.EventId);
                Append(text, entry.SourceItemInstanceId);
                Append(text, entry.SourceBaseItemId);
                Append(text, entry.NianBefore);
                Append(text, entry.NianAfter);
                Append(text, entry.EnemyHpBefore);
                Append(text, entry.EnemyHpAfter);
                Append(text, entry.PlayerShieldBefore);
                Append(text, entry.PlayerShieldAfter);
                Append(text, entry.PlayerHpBefore);
                Append(text, entry.PlayerHpAfter);
                Append(text, entry.ReasonCode);
            }
            return Sha256(text.ToString());
        }

        private static void Append(StringBuilder target, object value)
        {
            string text = Convert.ToString(
                value,
                CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(text).Append('|');
        }

        private static string Sha256(string value)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value ?? string.Empty)))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }
    }
}
