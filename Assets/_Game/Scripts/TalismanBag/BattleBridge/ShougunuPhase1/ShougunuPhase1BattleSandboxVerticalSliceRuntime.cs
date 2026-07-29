using System;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Combat;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Resource;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TalismanBag.BattleBridge.ShougunuPhase1
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(600)]
    public sealed class ShougunuPhase1BattleSandboxVerticalSliceRuntime :
        MonoBehaviour
    {
        public const string PackageId =
            "V0.4-RealItemDamageShougunuPhase1BattleSandboxVerticalSlice01";
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string TargetSceneName =
            "Scene_TalismanBag_V04_BattleSandboxPreview";

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private BuildGridInteractionPreviewController
            gridController;
        [SerializeField] private ShougunuPhase1BattleSandboxSceneBinder
            sceneBinder;

        private ShougunuPhase1BattleApplicationSession session;
        private BattleSandboxNianResourceEngine nianResourceEngine;
        private I031NianSourceSnapshot nianSourceSnapshot;
        private BattleSandboxNianPulseResult lastNianPulseResult;
        private BattleSandboxAcceptedItemPresentationEvent
            lastAcceptedItemPresentation;
        private IItemSystemBattleSandboxBoardAuthority itemAuthority;
        private ItemCombatEffectRequestSnapshot itemRequestSnapshot;
        private int resetGeneration;
        private int displayEventCursor;
        private float elapsedMilliseconds;
        private float sourceRecheckTimer;
        private float pendingStartTimer;
        private float authorityBootstrapTimer;
        private string acceptedItemSystemSignature = string.Empty;
        private string lastDiagnosticCode = "NOT_STARTED";
        private string lastDiagnosticDetail = string.Empty;
        private bool observedBattleMode;
        private bool authoredContinueBattleActive;
        private bool pendingStart;
        private bool startRejected;
        private bool completed;
        private bool nianResourceBlocked;
        private bool authorityBootstrapAttempted;
        private int acceptedItemPresentationCount;
        private int rejectedItemPresentationCount;
        private GameObject ownedItemAuthorityBridge;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool EntersFormalBattle => false;
        public bool WritesFormalFlow => false;
        public bool WritesPersistentState => false;
        public bool GrantsFormalReward => false;
        public bool AdvancesChapter => false;
        public bool HasActiveSession => session != null;
        public bool Completed => completed;
        public int ResetGeneration => resetGeneration;
        public int DisplayEventCursor => displayEventCursor;
        public string LastDiagnosticCode => lastDiagnosticCode;
        public string LastDiagnosticDetail => lastDiagnosticDetail;
        public BuildGridInteractionPreviewController GridController =>
            gridController;
        public ShougunuPhase1BattleSandboxSceneBinder SceneBinder =>
            sceneBinder;
        public ShougunuPhase1BattleApplicationContext CurrentContext =>
            session?.Current;
        public ItemCombatEffectRequestSnapshot CurrentItemRequestSnapshot =>
            itemRequestSnapshot;
        public I031NianSourceSnapshot CurrentNianSourceSnapshot =>
            nianSourceSnapshot;
        public BattleSandboxNianResourceSnapshot CurrentNianResourceSnapshot =>
            nianResourceEngine?.Current;
        public BattleSandboxNianPulseResult LastNianPulseResult =>
            lastNianPulseResult;
        public bool NianResourceBlocked => nianResourceBlocked;
        public BattleSandboxAcceptedItemPresentationEvent
            LastAcceptedItemPresentation =>
                lastAcceptedItemPresentation;
        public int AcceptedItemPresentationCount =>
            acceptedItemPresentationCount;
        public int RejectedItemPresentationCount =>
            rejectedItemPresentationCount;

        private void Awake()
        {
            sceneBinder?.BindRuntime(this);
        }

        private void OnEnable()
        {
            observedBattleMode = false;
            authoredContinueBattleActive = false;
            pendingStart = false;
            startRejected = false;
            completed = false;
            authorityBootstrapTimer = 0f;
            authorityBootstrapAttempted = false;
            sceneBinder?.BindRuntime(this);
        }

        private void Update()
        {
            if (!devOnly || !isEnabled)
            {
                return;
            }

            TryEnsureTerminalItemAuthorityAdapter();

            bool gridBattleModeActive = IsGridBattleModeActive();
            if (authoredContinueBattleActive && gridBattleModeActive)
            {
                // The authored button enters a short "continue" transition
                // during which the grid does not yet report battle mode.
                // Once the transition has settled, the grid becomes the sole
                // lifecycle owner again so opening preparation can end P3.
                authoredContinueBattleActive = false;
            }

            bool battleModeActive = authoredContinueBattleActive
                || gridBattleModeActive;
            if (battleModeActive && !observedBattleMode)
            {
                observedBattleMode = true;
                BeginNewGeneration();
            }
            else if (!battleModeActive && observedBattleMode)
            {
                observedBattleMode = false;
                EndBattleMode();
            }

            if (!battleModeActive)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RequestReset();
                return;
            }

            if (pendingStart && !startRejected)
            {
                pendingStartTimer += Time.unscaledDeltaTime;
                if (pendingStartTimer >= 0.05f)
                {
                    pendingStartTimer = 0f;
                    TryStartFromCurrentItemAuthority();
                }
            }

            if (session == null || completed || nianResourceBlocked)
            {
                return;
            }

            sourceRecheckTimer += Time.unscaledDeltaTime;
            if (sourceRecheckTimer >= 0.25f)
            {
                sourceRecheckTimer = 0f;
                if (!SourceAuthorityIsCurrent())
                {
                    InvalidateForSourceMutation();
                    return;
                }
            }

            elapsedMilliseconds = Mathf.Min(
                ShougunuPhase1BattleApplicationEngine.NominalDurationTicks,
                elapsedMilliseconds + Time.unscaledDeltaTime * 1000f);
            long targetTick = Math.Min(
                ShougunuPhase1BattleApplicationEngine.NominalDurationTicks,
                (long)Math.Floor(elapsedMilliseconds));
            ShougunuPhase1BattleApplicationContext context =
                AdvanceWithNianGate(targetTick);
            if (nianResourceBlocked)
            {
                return;
            }
            PublishNewDisplayEvents(context);
            sceneBinder?.ApplyContext(context);

            if (targetTick
                    >= ShougunuPhase1BattleApplicationEngine
                        .NominalDurationTicks
                && context.enemy != null
                && context.enemy.CurrentHp == 0)
            {
                completed = true;
                lastDiagnosticCode = "NOMINAL_COMPLETE";
                sceneBinder?.ShowCompletion(context);
            }
        }

        public void RequestReset()
        {
            if (!observedBattleMode
                || (!authoredContinueBattleActive
                    && !IsGridBattleModeActive()))
            {
                return;
            }

            BeginNewGeneration();
        }

        public void HandleAuthoredBattleStateButton()
        {
            if (!devOnly || !isEnabled)
            {
                return;
            }

            if (authoredContinueBattleActive || observedBattleMode)
            {
                authoredContinueBattleActive = false;
                if (!IsGridBattleModeActive())
                {
                    observedBattleMode = false;
                    EndBattleMode();
                }
                return;
            }

            authoredContinueBattleActive = true;
            observedBattleMode = true;
            BeginNewGeneration();
        }

        private void BeginNewGeneration()
        {
            resetGeneration++;
            session = null;
            nianResourceEngine = null;
            nianSourceSnapshot = null;
            lastNianPulseResult = null;
            lastAcceptedItemPresentation = null;
            itemAuthority = null;
            itemRequestSnapshot = null;
            displayEventCursor = 0;
            elapsedMilliseconds = 0f;
            sourceRecheckTimer = 0f;
            pendingStartTimer = 0f;
            acceptedItemSystemSignature = string.Empty;
            pendingStart = true;
            startRejected = false;
            completed = false;
            nianResourceBlocked = false;
            acceptedItemPresentationCount = 0;
            rejectedItemPresentationCount = 0;
            lastDiagnosticCode = "ITEM_AUTHORITY_PENDING";
            lastDiagnosticDetail = string.Empty;
            sceneBinder?.BeginGeneration(resetGeneration);
            TryStartFromCurrentItemAuthority();
        }

        private void TryStartFromCurrentItemAuthority()
        {
            ItemSystemBattleSandboxBoardAdapter[] adapters =
                Resources.FindObjectsOfTypeAll<
                        ItemSystemBattleSandboxBoardAdapter>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (adapters.Length != 1 || adapters[0].Authority == null)
            {
                lastDiagnosticCode = adapters.Length > 1
                    ? "ITEM_AUTHORITY_DUPLICATE"
                    : "ITEM_AUTHORITY_PENDING";
                return;
            }

            IItemSystemBattleSandboxBoardAuthority authority =
                adapters[0].Authority;
            if (authority.CurrentSnapshot?.isValid != true
                || authority.CurrentQualifiedBuildState?.isValid != true
                || authority.CurrentCoreEffectRuntimeState == null)
            {
                RejectStart(
                    "ITEM_AUTHORITY_SNAPSHOT_INVALID",
                    "当前棋盘合同无效；请确认道具已完整落格、无重叠后重试");
                return;
            }

            try
            {
                ItemInstanceProjectionSetSnapshot projectionSet = new(
                    authority.Rows
                        .Where(row => row != null && !row.IsSystemItem)
                        .Select(row => row.Projection),
                    Array.Empty<ItemInstanceProjectionValidationError>());
                ItemCombatEffectRequestSnapshot assembled =
                    ItemCombatEffectRequestAssembler.Instance.Assemble(
                        projectionSet,
                        authority.CurrentSnapshot,
                        authority.CurrentQualifiedBuildState,
                        authority.CurrentCoreEffectRuntimeState);
                string readinessIssue = BuildTerminalReadinessIssue(
                    assembled,
                    authority);
                if (!string.IsNullOrEmpty(readinessIssue))
                {
                    RejectStart(
                        "TERMINAL_ITEM_CHAIN_NOT_READY",
                        readinessIssue);
                    return;
                }
                ShougunuPhase1BattleApplicationSession created =
                    ShougunuPhase1BattleApplicationEngine.Create(
                        assembled,
                        resetGeneration);
                I031NianSourceSnapshot source =
                    I031NianSourceAssembler.Assemble(
                        authority.CurrentSnapshot,
                        resetGeneration);
                if (source.status != I031NianSourceStatus.Valid
                    || !source.isEligible)
                {
                    RejectStart(
                        "I031_NIAN_SOURCE_NOT_READY",
                        source.ValidationErrors.Count == 0
                            ? "I031 念力源合同无效"
                            : "I031 念力源错误："
                              + string.Join("、", source.ValidationErrors));
                    return;
                }
                BattleSandboxNianResourceEngine resourceEngine =
                    BattleSandboxNianResourceEngine.Create(source);

                itemAuthority = authority;
                itemRequestSnapshot = assembled;
                session = created;
                nianSourceSnapshot = source;
                nianResourceEngine = resourceEngine;
                lastNianPulseResult = null;
                nianResourceBlocked = false;
                acceptedItemSystemSignature =
                    authority.CurrentSnapshot.BuildDebugSignature();
                pendingStart = false;
                startRejected = false;
                completed = false;
                lastDiagnosticCode = "RUNNING";
                lastDiagnosticDetail = string.Empty;
                sceneBinder?.ApplyContext(session.Current);
                sceneBinder?.BeginNianSession(
                    nianResourceEngine.Current);
                sceneBinder?.ShowStartFacts(
                    assembled.requestCount,
                    assembled.telemetryCount,
                    authority.CurrentSnapshot.i031State?.isOwned == true,
                    authority.CurrentSnapshot.i031State?.isPlaced == true);
            }
            catch (Exception exception)
            {
                RejectStart(
                    "TERMINAL_ITEM_CHAIN_REJECTED_"
                    + exception.GetType().Name,
                    "Battle 合同拒绝：" + exception.Message);
            }
        }

        private void TryEnsureTerminalItemAuthorityAdapter()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying
                || authorityBootstrapAttempted
                || ownedItemAuthorityBridge != null
                || gameObject.scene.name != TargetSceneName)
            {
                return;
            }

            authorityBootstrapTimer += Time.unscaledDeltaTime;
            if (authorityBootstrapTimer < 0.1f)
            {
                return;
            }

            ItemSystemBattleSandboxBoardAdapter[] existing =
                Resources.FindObjectsOfTypeAll<
                        ItemSystemBattleSandboxBoardAdapter>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (existing.Length > 0)
            {
                return;
            }

            ItemDetailPanelView[] panels =
                FindObjectsOfType<ItemDetailPanelView>(true)
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (gridController == null || panels.Length != 1)
            {
                return;
            }

            authorityBootstrapAttempted = true;
            var bridge = new GameObject(
                "ShougunuP1VerticalSlice_ItemAuthorityBridge");
            bridge.hideFlags = HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild
                | HideFlags.HideInHierarchy;
            SceneManager.MoveGameObjectToScene(bridge, gameObject.scene);
            ItemInnerDataCatalogProvider provider =
                bridge.AddComponent<ItemInnerDataCatalogProvider>();
            provider.hideFlags = HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild;
            ItemSystemBattleSandboxBoardAdapter adapter =
                bridge.AddComponent<ItemSystemBattleSandboxBoardAdapter>();
            adapter.hideFlags = HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild;
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    ItemSystemBattleSandboxBoardAdapter
                        .WorkbenchCatalogPath);
            if (!adapter.Initialize(
                    gridController,
                    workbench,
                    provider,
                    panels[0]))
            {
                lastDiagnosticCode =
                    "TERMINAL_ITEM_AUTHORITY_BOOTSTRAP_FAILED_"
                    + adapter.LastDiagnosticCode;
                Destroy(bridge);
                return;
            }

            ownedItemAuthorityBridge = bridge;
            lastDiagnosticCode = "TERMINAL_ITEM_AUTHORITY_READY";
#endif
        }

        private static string BuildTerminalReadinessIssue(
            ItemCombatEffectRequestSnapshot snapshot,
            IItemSystemBattleSandboxBoardAuthority authority)
        {
            if (snapshot == null)
            {
                return "未生成 ItemCombatEffectRequestSnapshot.v1";
            }

            string[] validationCodes = snapshot.ValidationErrors
                .Where(value => value != null)
                .Select(value => value.code)
                .Distinct(StringComparer.Ordinal)
                .Take(3)
                .ToArray();
            if (snapshot.status != ItemCombatEffectRequestSnapshotStatus.Valid)
            {
                return validationCodes.Length == 0
                    ? "道具请求合同为 Invalid"
                    : "道具请求合同错误：" + string.Join("、", validationCodes);
            }

            string[] issues = ShougunuPhase1BattleApplicationEngine.ItemIds
                .Select(itemId =>
                {
                    ItemCombatEffectRequestRow[] rows = snapshot.Requests
                        .Where(value => value != null
                            && string.Equals(
                                value.sourceBaseItemId,
                                itemId,
                                StringComparison.Ordinal))
                        .ToArray();
                    if (rows.Length == 0)
                    {
                        return itemId + " 未点亮/未计入 Build";
                    }
                    if (rows.Length != 1)
                    {
                        return itemId + " 请求数=" + rows.Length;
                    }
                    if (rows[0].resolvedPreMitigationDamageUnits <= 0)
                    {
                        return itemId + " 真实 Projection 伤害="
                            + rows[0].resolvedPreMitigationDamageUnits
                            + "（必须为正值）";
                    }
                    return null;
                })
                .Where(value => !string.IsNullOrEmpty(value))
                .ToArray();
            if (issues.Length > 0)
            {
                return string.Join("；", issues);
            }

            if (authority?.CurrentSnapshot?.i031State?.isOwned != true)
            {
                return "I031 未拥有";
            }
            if (authority.CurrentSnapshot.i031State.isPlaced != true)
            {
                return "I031 未放入棋盘，I007-I012 无法形成终态供能链";
            }
            return string.Empty;
        }

        private void RejectStart(
            string diagnosticCode,
            string diagnosticDetail)
        {
            session = null;
            nianResourceEngine = null;
            nianSourceSnapshot = null;
            lastNianPulseResult = null;
            lastAcceptedItemPresentation = null;
            itemRequestSnapshot = null;
            pendingStart = false;
            startRejected = true;
            completed = false;
            nianResourceBlocked = false;
            acceptedItemPresentationCount = 0;
            rejectedItemPresentationCount = 0;
            lastDiagnosticCode = diagnosticCode ?? "START_REJECTED";
            lastDiagnosticDetail =
                string.IsNullOrWhiteSpace(diagnosticDetail)
                    ? "道具链未就绪"
                    : diagnosticDetail.Trim();
            sceneBinder?.ShowConfigurationError(
                lastDiagnosticCode,
                lastDiagnosticDetail);
        }

        private bool SourceAuthorityIsCurrent()
        {
            return itemAuthority != null
                && itemAuthority.CurrentSnapshot?.isValid == true
                && string.Equals(
                    acceptedItemSystemSignature,
                    itemAuthority.CurrentSnapshot.BuildDebugSignature(),
                    StringComparison.Ordinal);
        }

        private void InvalidateForSourceMutation()
        {
            resetGeneration++;
            session = null;
            nianResourceEngine = null;
            nianSourceSnapshot = null;
            lastNianPulseResult = null;
            lastAcceptedItemPresentation = null;
            itemRequestSnapshot = null;
            displayEventCursor = 0;
            elapsedMilliseconds = 0f;
            completed = false;
            nianResourceBlocked = false;
            acceptedItemPresentationCount = 0;
            rejectedItemPresentationCount = 0;
            pendingStart = false;
            startRejected = true;
            lastDiagnosticCode = "ITEM_SOURCE_CHANGED_STALE_REJECTED";
            lastDiagnosticDetail =
                "阵容状态已变化，旧请求与表现已拒绝";
            sceneBinder?.BeginGeneration(resetGeneration);
            sceneBinder?.ShowSourceChanged();
        }

        private void PublishNewDisplayEvents(
            ShougunuPhase1BattleApplicationContext context)
        {
            if (context == null || context.DisplayEvents == null)
            {
                return;
            }

            while (displayEventCursor < context.DisplayEvents.Count)
            {
                ShougunuPhase1BattleDisplayEvent displayEvent =
                    context.DisplayEvents[displayEventCursor++];
                sceneBinder?.RenderDisplayEvent(
                    displayEvent,
                    resetGeneration);
            }
        }

        private ShougunuPhase1BattleApplicationContext AdvanceWithNianGate(
            long targetTick)
        {
            if (session == null || nianResourceEngine == null
                || itemAuthority?.CurrentSnapshot == null)
            {
                BlockNianResource(
                    "NIAN_RUNTIME_NOT_READY",
                    null);
                return session?.Current;
            }

            while (!nianResourceBlocked)
            {
                int pulseSequence =
                    nianResourceEngine.Current.pulseCount + 1;
                long pulseTick = checked(
                    pulseSequence
                    * BattleSandboxNianResourceEngine.PulseIntervalTicks);
                if (pulseTick > targetTick)
                {
                    break;
                }

                if (session.Current.battleTick < pulseTick - 1L)
                {
                    session.AdvanceTo(pulseTick - 1L);
                }

                I031NianSourceSnapshot currentSource =
                    I031NianSourceAssembler.Assemble(
                        itemAuthority.CurrentSnapshot,
                        resetGeneration);
                int itemOrdinal =
                    nianResourceEngine.Current.nextCostOrdinal;
                ItemCombatEffectRequestRow damageRow =
                    session.TerminalRows[itemOrdinal];
                I031NianCostRequestFact expectedCost =
                    currentSource.FindCostFact(
                        damageRow.sourceBaseItemId);
                string pulseEventId = BuildSharedPulseEventId(
                    resetGeneration,
                    pulseSequence,
                    damageRow.requestId);
                int generatedBefore =
                    nianResourceEngine.Current.totalGenerated;
                BattleSandboxNianPulseResult resourceResult =
                    nianResourceEngine.ApplyPulse(
                        currentSource,
                        new BattleSandboxNianPulseRequest(
                            pulseEventId,
                            resetGeneration,
                            currentSource.canonicalSignature,
                            expectedCost?.requestFactId ?? string.Empty,
                            pulseTick));
                lastNianPulseResult = resourceResult;
                int generatedAmount =
                    resourceResult.snapshot.totalGenerated
                    - generatedBefore;
                sceneBinder?.ApplyNianPulse(
                    resourceResult.snapshot,
                    resourceResult.resourceApplication,
                    generatedAmount);
                if (!resourceResult.accepted)
                {
                    BlockNianResource(
                        resourceResult.reasonCode,
                        resourceResult);
                    return session.Current;
                }

                int ledgerCountBeforeDamage =
                    session.Current.ledger?.Events.Count ?? 0;
                int displayCountBeforeDamage =
                    session.Current.DisplayEvents?.Count ?? 0;
                ShougunuPhase1BattleApplicationContext afterDamage =
                    session.AdvanceTo(pulseTick);
                if (afterDamage.resetGeneration
                        != resourceResult.snapshot.resetGeneration
                    || afterDamage.enemy.AcceptedDamageApplicationCount
                        != resourceResult.snapshot
                            .acceptedApplicationCount)
                {
                    BlockNianResource(
                        "NIAN_DAMAGE_LEDGER_IDENTITY_MISMATCH",
                        resourceResult);
                    return afterDamage;
                }

                PresentAcceptedItemApplication(
                    resourceResult,
                    damageRow,
                    itemAuthority.CurrentSnapshot,
                    afterDamage,
                    ledgerCountBeforeDamage,
                    displayCountBeforeDamage);
                nianSourceSnapshot = currentSource;
            }

            return session.AdvanceTo(targetTick);
        }

        private void PresentAcceptedItemApplication(
            BattleSandboxNianPulseResult resourceResult,
            ItemCombatEffectRequestRow damageRow,
            ItemSystemSnapshot itemSnapshot,
            ShougunuPhase1BattleApplicationContext afterDamage,
            int ledgerCountBeforeDamage,
            int displayCountBeforeDamage)
        {
            BattleSandboxNianResourceApplication nianApplication =
                resourceResult?.resourceApplication;
            if (nianApplication?.accepted != true
                || damageRow == null
                || itemSnapshot == null
                || afterDamage?.ledger?.Events == null
                || afterDamage.DisplayEvents == null
                || damageRow.resolvedPreMitigationDamageUnits <= 0L
                || damageRow.resolvedPreMitigationDamageUnits
                    > int.MaxValue)
            {
                rejectedItemPresentationCount++;
                return;
            }

            ShougunuPhase1BattleLedgerEvent damageLedger =
                afterDamage.ledger.Events
                    .Skip(Mathf.Max(0, ledgerCountBeforeDamage))
                    .FirstOrDefault(value =>
                        value != null
                        && value.resetGeneration == resetGeneration
                        && value.battleTick == nianApplication.battleTick
                        && value.eventKind
                            == ShougunuPhase1BattleLedgerEventKind
                                .ItemApplication
                        && value.decision
                            == ShougunuPhase1BattleApplicationDecision
                                .Accepted
                        && value.amount > 0
                        && string.Equals(
                            value.sourceId,
                            damageRow.requestId,
                            StringComparison.Ordinal));
            if (damageLedger == null)
            {
                rejectedItemPresentationCount++;
                return;
            }

            ShougunuPhase1BattleDisplayEvent[] settlementEvents =
                afterDamage.DisplayEvents
                    .Skip(Mathf.Max(0, displayCountBeforeDamage))
                    .Where(value => value != null
                        && string.Equals(
                            value.ledgerEventId,
                            damageLedger.ledgerEventId,
                            StringComparison.Ordinal)
                        && (value.channel
                                == ShougunuPhase1BattleDisplayChannel
                                    .ItemToEnemyShellDamage
                            || value.channel
                                == ShougunuPhase1BattleDisplayChannel
                                    .ItemToEnemyHpDamage))
                    .ToArray();
            int shellDamageApplied = settlementEvents
                .Where(value => value.channel
                    == ShougunuPhase1BattleDisplayChannel
                        .ItemToEnemyShellDamage)
                .Sum(value => value.amount);
            int hpDamageApplied = settlementEvents
                .Where(value => value.channel
                    == ShougunuPhase1BattleDisplayChannel
                        .ItemToEnemyHpDamage)
                .Sum(value => value.amount);
            int totalDamageApplied =
                checked(shellDamageApplied + hpDamageApplied);
            BattleSandboxNianLedgerEntry[] generationEntries =
                resourceResult.snapshot.Ledger
                    .Where(value => value != null
                        && value.resetGeneration == resetGeneration
                        && value.battleTick == nianApplication.battleTick
                        && value.kind
                            == BattleSandboxNianLedgerKind.Generation
                        && string.Equals(
                            value.pulseEventId,
                            nianApplication.pulseEventId,
                            StringComparison.Ordinal))
                    .ToArray();
            BattleSandboxNianLedgerEntry[] spendEntries =
                resourceResult.snapshot.Ledger
                    .Where(value => value != null
                        && value.resetGeneration == resetGeneration
                        && value.battleTick == nianApplication.battleTick
                        && value.kind
                            == BattleSandboxNianLedgerKind.SpendAccepted
                        && string.Equals(
                            value.pulseEventId,
                            nianApplication.pulseEventId,
                            StringComparison.Ordinal))
                    .ToArray();
            ItemSystemPlacementSnapshot placement =
                itemSnapshot.FindPlacement(
                    damageRow.sourcePlacementId);
            ItemSystemPlacementSnapshot i031Placement =
                itemSnapshot.FindPlacement(
                    I031InventoryPlacementContract.StablePlacementId);
            if (placement == null
                || !string.Equals(
                    placement.itemId,
                    damageRow.sourceBaseItemId,
                    StringComparison.Ordinal)
                || placement.OccupiedCells == null
                || placement.OccupiedCells.Count == 0
                || i031Placement == null
                || !string.Equals(
                    i031Placement.itemId,
                    I031InventoryPlacementContract.ItemId,
                    StringComparison.Ordinal)
                || i031Placement.OccupiedCells == null
                || i031Placement.OccupiedCells.Count == 0
                || generationEntries.Length != 1
                || generationEntries[0].amount < 0
                || spendEntries.Length != 1
                || spendEntries[0].amount
                    != nianApplication.nianCost
                || totalDamageApplied <= 0
                || damageLedger.amount != totalDamageApplied)
            {
                rejectedItemPresentationCount++;
                return;
            }

            BattleSandboxAcceptedItemPresentationEvent presentation =
                new(
                    nianApplication.pulseEventId,
                    nianApplication.applicationId,
                    damageLedger.ledgerEventId,
                    resetGeneration,
                    nianApplication.battleTick,
                    damageRow.sourceBaseItemId,
                    damageRow.sourceItemInstanceId,
                    damageRow.sourcePlacementId,
                    placement.OccupiedCells.Select(value =>
                        new ItemShapeCell(value.x, value.y)),
                    i031Placement.OccupiedCells.Select(value =>
                        new ItemShapeCell(value.x, value.y)),
                    generationEntries[0].amount,
                    spendEntries[0].amount,
                    checked((int)
                        damageRow.resolvedPreMitigationDamageUnits),
                    shellDamageApplied,
                    hpDamageApplied);
            if (!presentation.IsAcceptedCorrelation
                || sceneBinder?.PresentAcceptedItemApplication(
                    presentation) != true)
            {
                rejectedItemPresentationCount++;
                return;
            }

            lastAcceptedItemPresentation = presentation;
            acceptedItemPresentationCount++;
        }

        public static string BuildSharedPulseEventId(
            int generation,
            int pulseSequence,
            string damageRequestId)
        {
            return "battle.request.g"
                + generation.ToString(CultureInfo.InvariantCulture)
                + "."
                + pulseSequence.ToString(
                    "D3",
                    CultureInfo.InvariantCulture)
                + "."
                + (damageRequestId ?? string.Empty);
        }

        private void BlockNianResource(
            string reasonCode,
            BattleSandboxNianPulseResult result)
        {
            nianResourceBlocked = true;
            completed = false;
            lastDiagnosticCode = string.IsNullOrWhiteSpace(reasonCode)
                ? "NIAN_RESOURCE_REJECTED"
                : reasonCode;
            BattleSandboxNianResourceApplication application =
                result?.resourceApplication;
            bool insufficient =
                application?.reasonCode == "INSUFFICIENT_NIAN";
            lastDiagnosticDetail = application == null
                ? "念力资源链未就绪；按 R 重置"
                : insufficient
                    ? application.itemId + " 需要 "
                      + application.nianCost.ToString(
                          CultureInfo.InvariantCulture)
                      + " 念力，当前 "
                      + (result.snapshot?.currentNian ?? 0).ToString(
                          CultureInfo.InvariantCulture)
                      + "；本次伤害已拒绝，按 R 重置"
                    : "I031 念力源校验拒绝（"
                      + application.reasonCode
                      + "）；未产念、未扣念、未造成伤害，按 R 重置";
            sceneBinder?.ShowNianResourceRejected(
                lastDiagnosticCode,
                lastDiagnosticDetail);
        }

        private void EndBattleMode()
        {
            session = null;
            nianResourceEngine = null;
            nianSourceSnapshot = null;
            lastNianPulseResult = null;
            lastAcceptedItemPresentation = null;
            itemAuthority = null;
            itemRequestSnapshot = null;
            displayEventCursor = 0;
            elapsedMilliseconds = 0f;
            sourceRecheckTimer = 0f;
            pendingStartTimer = 0f;
            acceptedItemSystemSignature = string.Empty;
            pendingStart = false;
            startRejected = false;
            completed = false;
            nianResourceBlocked = false;
            acceptedItemPresentationCount = 0;
            rejectedItemPresentationCount = 0;
            lastDiagnosticCode = "BATTLE_MODE_INACTIVE";
            lastDiagnosticDetail = string.Empty;
            authoredContinueBattleActive = false;
            sceneBinder?.EndBattleMode();
        }

        private bool IsGridBattleModeActive()
        {
            return gridController != null
                && gridController.IsSandboxBattleModeActive;
        }

        private void OnDisable()
        {
            observedBattleMode = false;
            authoredContinueBattleActive = false;
            EndBattleMode();
#if UNITY_EDITOR
            if (ownedItemAuthorityBridge != null)
            {
                Destroy(ownedItemAuthorityBridge);
                ownedItemAuthorityBridge = null;
            }
#endif
        }
    }
}
