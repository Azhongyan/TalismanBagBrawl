# V0.4-C1FormalItemDetailProjectionAndSelectionSeam01 Task Contract

Status: `CONTAINED_ONE_GUARD / BOUNDARY_FROZEN`

## 1. Outcome

Provide the missing Item-owned formal detail seam for `CAMPAIGN_NORMAL_LV1`:

- selecting an authoritative formal Item instance produces one immutable,
  session-bound detail projection plus the already-authored authoritative
  artwork;
- selection/open and close are explicit immutable results that Mainline can
  consume to call `ItemDetailPanelView.Bind(...)` or `Close()`;
- stale session, reset, unbind, missing instance, missing catalog data, or
  missing artwork fail closed and cannot leave an old detail open;
- Item remains the sole owner of instance identity, placement and lighting
  facts. Mainline owns only popup mounting and visible open/close consumption.

Dependency consumer:
the downstream Mainline exact bottom-navigation plus Item-detail-popup
promotion/mount package.

## 2. Product Context And Fidelity

- Product context: `CAMPAIGN_NORMAL_LV1`.
- PlayerVisibleDelivery: `PARTIAL`.
- MilestoneCompletionEvidence: `NO`.
- This is a technical prerequisite. It does not mount or display the popup and
  cannot claim `USER_HANDTEST_READY` for the formal milestone.
- Original player promise remains: the formal bridge page uses the exact
  accepted BattleSandbox-derived Board/Tray/Card and opens the real Item detail
  for the currently selected formal instance during the continuous
  WorldMap -> 1-1 -> I002 -> rebuild -> 1-2 -> WorldMap path.
- Explicit non-completion: a catalog preview, generated-instance preview,
  copied debug row, BuildSandbox adapter, selected-ID-only event, or Mainline
  label inference is not a formal detail result.

Capability rows:

| Capability | Owner | Carrier | This package |
|---|---|---|---|
| Validate formal selected instance/session/reset lineage | Item | immutable request/result | OWNED_HERE |
| Project Item detail facts without inference | Item | immutable projection snapshot | OWNED_HERE |
| Resolve the exact already-authored I001/I002/I031 artwork | Item presentation | exact BoardView artwork resolver | OWNED_HERE |
| Publish selection/open/close result | Item | exact Item Presenter event/current result | OWNED_HERE |
| Mount exact detail Prefab and outside-click/close behavior | Mainline/presentation | Unified popup carrier | DOWNSTREAM_MISSING |
| Prove continuous formal real path | Mainline integration | Unified route | DOWNSTREAM_REQUIRED |

Milestone verdict at intake: `FAITHFUL_PARTIAL`. The prerequisite is allowed to
proceed, but no user-visible or milestone completion claim may be made here.

## 3. Task Class And User Review

- Class: `CONTAINED_ONE_GUARD / TECHNICAL_PREREQUISITE / PURE_CODE`.
- User Review Gate: `NO`.
- Marker: `NOT_REQUIRED / USER_TECHNICAL_AUTHORIZATION_GRANTED`.
- Primary Guard: Item Guard.
- Development Owner: visible unarchived direct-local task
  `019fc279-2bb0-7e61-a2f2-d9549b564cfb`.
- User Decision Point: `NONE`.
- Producer/Balance gates: `NOT_APPLICABLE`; no values, probability, cadence,
  Build threshold, damage or economy decision changes.
- Technical Director gate: `NOT_REQUIRED`; the existing owner/carrier decision
  remains valid and this package adds no Scene, Prefab or runtime-graph owner.

## 4. Ownership

- `C1FormalItemSessionAuthority.Current` is the sole source of session token,
  reset generation, roster, placement, ItemSystem lighting and canonical
  signatures.
- `IItemDetailViewModelProvider` remains the read-only authored catalog-detail
  source. The Item seam receives it explicitly; no scene lookup or hidden
  provider is permitted.
- `C1ExactBattleSandboxItemBoardView` may expose one narrow read-only artwork
  resolution interface using its existing serialized I001/I002/I031 sprites.
  It does not acquire selection or detail state ownership.
- `C1ExactBattleSandboxItemArrangementPresenter` owns the current formal
  selection/open result lifecycle and publishes immutable results only.
- Mainline may subscribe/unsubscribe and call the public open/close seam. It
  must not construct `ItemDetailViewModel`, infer artwork, or validate Item
  lineage itself.
- `ItemDetailPanelView` remains the downstream authored presentation consumer
  and is read-only in this package.

## 5. Exact Allowed Writes

Modify only:

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs`
2. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs`

Add only:

3. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemDetailProjectionAndSelection.cs`
4. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemDetailProjectionAndSelection.cs.meta`
5. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemDetailProjectionAndSelectionTests.cs`
6. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemDetailProjectionAndSelectionTests.cs.meta`

The Task Contract is Guard-owned and read-only to development. No report file
is required.

## 6. Forbidden Writes

- all Scene and Prefab files, including exact Board/Tray/Card and Unified;
- `C1FormalItemSessionAndArrangementAuthority.cs`, ItemSystem, Item shape,
  placement, rotation, lighting, Build, Core, catalog and rarity truth;
- `C1ExactBattleSandboxItemCardView.cs` and TrayView;
- `ItemDetailViewModel.cs`, `ItemDetailProjectionComposer.cs`,
  `ItemDetailInstanceDataAdapter.cs`, `ItemDetailPanelView.cs` and all detail UI;
- BuildSandbox Item-detail adapter/controller, BattleSandbox fixtures and dev
  hosts;
- Mainline, Unified host, navigation, Battle, Enemy, Reward/Drop, Save,
  Inventory persistence, Progression and RunFlow;
- VFX P1/P2 assets and runtime;
- ProjectSettings, BuildSettings, Packages, AGENTS, LOCKED, Queue, Notion and
  all Git/worktree operations.

Do not make runtime members public solely for Editor tests. Tests must consume
the intended public contract or use bounded reflection only for a genuinely
internal test seam.

## 7. Required Sources

Read only:

- `AGENTS.md`, Engineering Process V2 lock and current engineering state;
- this Task Contract;
- the six whitelist paths;
- `C1FormalItemSessionAndArrangementAuthority.cs`;
- `C1Lv1StarterItemBaseline.cs` and
  `C1FixedFirstClearItemInstanceCarrier.cs`;
- `ItemDetailViewModel.cs`, `ItemDetailProjectionComposer.cs`,
  `ItemDetailInstanceDataAdapter.cs` and `ItemDetailPanelView.cs`;
- exact Board/Tray/Card Prefabs only to confirm existing artwork carrier facts;
- current Mainline Item mount/host only for consumer boundary, never as a
  writable owner.

Do not use BuildSandbox or V02 detail/inventory paths as a formal source.

## 8. Patch Budget

- existing runtime files modified: exactly 2 maximum;
- new runtime file/meta: exactly 2;
- new Editor test file/meta: exactly 2;
- Scene/Prefab/report files: 0.

Any need to modify formal authority, detail UI, Prefab/Scene serialization,
Item data semantics, Mainline, BuildSandbox or a seventh product path stops at
`ARCHITECTURE_REASSESS_REQUIRED`.

## 9. Immutable Public Contract

Names may follow local conventions, but the following facts and behavior are
mandatory.

Request:

- intent: `Open` or `Close`;
- product context `CAMPAIGN_NORMAL_LV1`;
- non-empty session token;
- positive reset generation;
- expected formal session canonical signature;
- selected authoritative itemInstanceId for `Open`; empty for `Close`.

Projection snapshot:

- schema/version and product context;
- session token, reset generation and source session signature;
- itemInstanceId, baseItemId, rarity key;
- identity, baseline-projection, catalog and instance canonical signatures;
- placement presence, placementId, anchor and formal rotation when placed;
- lighting presence and exact ItemSystem lighting facts when placed; absence is
  explicit for an unplaced tray item and is never coerced to false/zero;
- immutable detail model data exposed only through a defensive
  `CreateViewModelClone()` or equivalent;
- resolved authored `Sprite` plus a stable machine artwork identity derived
  from authoritative base identity and resolved lit state;
- deterministic projection canonical signature.

Result:

- accepted, changed and stable diagnostic;
- explicit open/closed state;
- immutable projection when open, absent projection when closed;
- deterministic result/current-state signature.

Presenter seam:

- current immutable detail result;
- one subscribe/unsubscribe-safe public event for accepted changes;
- explicit open-selected and close methods;
- existing Card/Board selection routes publish an `Open` result for the newly
  selected authoritative instance;
- unbind, reset/rebind to another token/generation, or selected-instance
  removal publishes one closed result before stale data can be consumed.

## 10. Required Behavior

1. `Open` first validates exact product context, token, generation and expected
   session signature against the current authority snapshot. It then resolves
   exactly one roster row by itemInstanceId; no baseItemId fallback.
2. The detail base model comes only from the injected
   `IItemDetailViewModelProvider` for the authoritative baseItemId. Missing or
   mismatched model fails closed without partial projection.
3. Formal instance identity/rarity/signatures overwrite any catalog-preview
   identity fields. Catalog text/stats remain authored facts; unsupported
   runtime sections remain the existing explicit unavailable/unknown state and
   are never inferred as zero.
4. Placement and lighting facts are copied from the same immutable formal
   session/ItemSystem snapshot. Tray items have explicit placement/lighting
   absence. Board items must have matching placementId/base identity and a
   resolvable ItemSystem placement; mismatch fails closed.
5. Artwork resolves from the exact authored formal Item carrier. Missing
   artwork, mismatched base identity or unsupported lit state rejects `Open`;
   no Resources lookup, name inference, placeholder, icon substitution or
   BuildSandbox fallback is allowed.
6. Repeating the identical valid `Open` is accepted with `Changed=false` and a
   stable signature. Opening another instance emits exactly one changed result.
7. `Close` is idempotent. The first close clears the projection and emits one
   changed result; repeated close is accepted unchanged.
8. Stale token/generation/session signature, unknown/removed instance, null
   provider, invalid model, missing artwork and invalid formal placement all
   reject without replacing a previously accepted current result. Reset/unbind
   is the exception: lifecycle invalidation must actively close stale detail.
9. Existing selection, drag, rotation, placement, footprint, scrolling and
   snapshot publication behavior remains unchanged. Detail projection is
   read-only and cannot submit an Item arrangement command.
10. Event binding/unbinding is idempotent; disable/destroy/unbind leaves no
    subscriber invocation owned by the Presenter and no stale projection.

## 11. Failure And Stop Conditions

- Existing formal catalog provider cannot produce I001/I002/I031 detail models
  without modifying Item detail truth: stop with the first exact missing
  provider seam; do not fabricate rows.
- Existing authored exact carrier cannot resolve an authoritative artwork
  without Prefab/Scene changes: stop with the exact missing serialized carrier;
  do not use `Resources.Load`, display names or debug icons.
- A valid Board placement lacks matching ItemSystem lighting/placement facts:
  reject that request and report the exact signature/identity mismatch.
- Any concurrent drift in either allowed existing source before first write:
  stop as `VERIFIED_WRITE_CONFLICT`; do not refresh the baseline silently.
- Need for Mainline/UI/Prefab/Scene write or duplicate selection/detail truth:
  stop at `ARCHITECTURE_REASSESS_REQUIRED`.

## 12. Task-Start Baselines

Allowed existing files:

| Path | SHA-256 |
|---|---|
| `C1ExactBattleSandboxItemArrangementPresenter.cs` | `3F58FCB2A3FEA730EF6E15807018EF7FC8580E253BBFAF59D4D2255A12774ABE` |
| `C1ExactBattleSandboxItemBoardView.cs` | `725CF21E90260D1454EE46C0534452D932635EF4D5F6027683E78624627A6ECC` |

New package files must be absent at task start:

- `C1FormalItemDetailProjectionAndSelection.cs` and `.meta`;
- `C1FormalItemDetailProjectionAndSelectionTests.cs` and `.meta`.

Protected baselines:

| Path | SHA-256 |
|---|---|
| `C1FormalItemSessionAndArrangementAuthority.cs` | `935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317` |
| `C1Lv1StarterItemBaseline.cs` | `A4C454FFCC0A3B5CE1BF2181A39B79CA73C809FD81FD5D3AA3257C7908A140C7` |
| `C1FixedFirstClearItemInstanceCarrier.cs` | `EFE734E86E60B54ECEED7A6518EB84CA9407E61E2898BF681EE2E855A8B5E76D` |
| `C1ExactBattleSandboxItemTrayView.cs` | `1C09886A117CD5806E8915B8565780B3A19E04AFED371ACDCC3B9C817A30425D` |
| `C1ExactBattleSandboxItemCardView.cs` | `7EEA77EFE28959E51C3986B046C4A739EAA7E45847FB103FE6D4770666CCE3A0` |
| `ItemDetailViewModel.cs` | `8A73A20B91BC79F546456584CCAAE38C2BBE384DEF6A27AB56F0715C98EE0689` |
| `ItemDetailProjectionComposer.cs` | `5B4017A3D5FD7E2C73C73F88F6D31A7E24824E9E748FF929D74C72333055A782` |
| `ItemDetailInstanceDataAdapter.cs` | `0EAAE3F90DD585F77D8F6D3F5426916FF6FF5C7CC1727F299974825228538806` |
| `ItemDetailPanelView.cs` | `045D20E2CC0D2ED87D477AB48BE7A00F954E3FE976904498502157239E534A90` |
| exact Card Prefab | `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` |
| exact Board Prefab | `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` |
| exact Tray Prefab | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |
| `UnifiedBattleFormalSceneHost.cs` | `B83B23725B3F1D03D339D0B2181232F1EA68B367AF3947C688E61449BF8015BB` |
| Unified shell Prefab | `D5834DB6C2E3EBACC583418033E06F26D8B258B75AC78C8FBE6B13B1544D202E` |
| Unified Scene | `9FED79FE312A34A6B802F464F928DFD032AF4615EBEE4715A7BF73DEC3BE3058` |
| `EditorBuildSettings.asset` | `CF244226FFD286638DEC957BBE5A9ABCA1DE52E9D529D71D406B5D03DE805932` |

## 13. Verification

Minimum automated evidence:

- current-source Runtime compile using the actual Assembly-CSharp boundary;
- current-source Editor compile using the actual Assembly-CSharp-Editor
  boundary;
- focused deterministic tests for valid Tray and Board I001/I002/I031 open,
  exact identity/placement/lighting/artwork facts, model defensive cloning and
  canonical signatures;
- rejection matrix for wrong product context, token, generation, source
  signature, unknown/removed item, mismatched placement, null catalog model and
  missing artwork;
- idempotent duplicate open/close, changed-instance event cardinality,
  reset/rebind/unbind stale-close and subscriber cleanup;
- static boundary proof for no BuildSandbox, Resources lookup, display-name
  inference, arrangement mutation, runtime hierarchy creation or forbidden
  write;
- exact whitelist, protected hashes, whitespace and temporary-output cleanup.

Explicitly not required:

- Unity batch, Fresh Play, Scene/Prefab authoring or import;
- user handtest for this component package;
- CSV, matrix or package report.

The consuming Mainline package owns popup mount, outside-click/close behavior
and the final normal-Editor real-path handtest.

## 14. Process Ownership

`UNITY_NOT_REQUIRED`

Do not start, wait for, poll, close or clean any Unity process. No Git operation.

## 15. Completion And Dependency Release

The component package self-closes when all frozen behavior and focused QA pass:

`C1_FORMAL_ITEM_DETAIL_PROJECTION_AND_SELECTION_SEAM_PASS / PACKAGE_COMPLETE`

Success classification:

- `TECHNICAL_PREREQUISITE_COMPLETE`;
- `PlayerVisibleDelivery=PARTIAL`;
- `MilestoneCompletionEvidence=NO`.

Terminal sync must list exact changed files, final public API names,
compile/tests, protected baselines, known limitations and no Unity/Git. Item
Guard then performs one close review and sends one dependency release to
Mainline Flow Guard `019f45d7-a33b-7630-a049-976481bb7b6d`.

Do not message Mainline implementation or the user directly. Do not claim popup
visibility, navigation-bar completion, `USER_HANDTEST_READY`,
`MILESTONE_INTEGRATED` or milestone completion.
