# V0.4-C1FormalItemInteractionAuthorization01 Task Contract

Status: `COMPLEX_GUARDED_ONCE / BOUNDARY_FROZEN`

## 1. Outcome

Provide the missing Item-owned formal interaction authorization seam for
`CAMPAIGN_NORMAL_LV1`:

- `BATTLE_LOCKED` rejects Item click, item drag, rotation, preview and placement;
- `PREPARE_ENABLED` permits the existing accepted BattleSandbox-derived Item
  interaction path without changing its placement rules;
- switching to locked, resetting, rebinding or unbinding synchronously cancels
  any active pointer gesture and clears drag, preview and rotate feedback;
- `C1FormalItemSessionAuthority` remains the sole placement truth. Mainline
  owns Battle pause/resume order and consumes this seam only after its own
  authoritative pause succeeds.

Dependency consumer: Mainline package
`V0.4-C1UnifiedExactBattleSandboxNavigationAndItemDetailPromotion01`.

## 2. Product Context And Fidelity

- Product context: `CAMPAIGN_NORMAL_LV1`.
- PlayerVisibleDelivery: `NONE` for this technical prerequisite.
- MilestoneCompletionEvidence: `NO`.
- Player Promise Card: clicking Prepare pauses the live Battle with zero hidden
  time advance, reveals Board/Tray and enables accepted drag/rotate/rearrange;
  live Battle locks all Item movement; closing Prepare clears interaction
  residue and resumes from the same paused state.
- This package owns only the Item input-authorization row. Battle pause/resume,
  Prepare visibility and the continuous real-path handtest remain downstream.
- Explicit non-completion: hiding Board/Tray while leaving cards interactive,
  changing time scale inside Item, disabling only one input route, or clearing
  visuals without canceling the underlying gesture does not satisfy the row.

Capability coverage:

| Capability | State owner | Carrier | This package |
|---|---|---|---|
| Immutable formal Item interaction mode | Item presentation contract | authorization request/result/snapshot | OWNED_HERE |
| Gate click/drag/rotate/preview/commit | Item presentation | exact Item Presenter and views | OWNED_HERE |
| Cancel stale gesture and feedback on lock/reset/unbind | Item presentation | Presenter plus existing Card/Board/Tray views | OWNED_HERE |
| Zero-time Battle pause/resume orchestration | Mainline/Battle | Unified Host and Battle session APIs | DOWNSTREAM |
| Prepare surface visibility and final real-path proof | Mainline | Unified exact carriers | DOWNSTREAM |

Milestone verdict at intake: `MILESTONE_FIDELITY_BLOCKED` until this dependency
is integrated and the user passes the full Prepare real path.

## 3. Task Class And User Review

- Class: `COMPLEX_GUARDED_ONCE / TECHNICAL_PREREQUISITE / PURE_CODE`.
- Reason: a new immutable cross-system authorization contract is introduced,
  while placement truth, Battle truth and visual carriers remain unchanged.
- User Review Gate: `NO`.
- Marker: `NOT_REQUIRED / USER_TECHNICAL_AUTHORIZATION_GRANTED`.
- Primary Guard: Item Guard.
- Development Owner: visible unarchived direct-local task
  `019fc279-2bb0-7e61-a2f2-d9549b564cfb`.
- User Decision Point: `NONE`.
- Producer/Balance gates: `NOT_APPLICABLE`; no value, probability, cadence,
  damage, Build or economy decision changes.

Technical Director decision:

- authoritative placement owner: `C1FormalItemSessionAuthority`;
- chosen carrier: one immutable Item authorization contract plus the existing
  exact Item Presenter/View input graph;
- reuse: existing Board/Tray/Card Prefabs and runtime Presenter;
- Scene/Prefab serialization impact: `NONE`;
- mobile/APK risk: no Editor/runtime fallback dependency and no per-frame work;
- stop if the contract requires Battle pause ownership, Scene/Prefab changes,
  a duplicate placement owner or a copied BuildSandbox runtime graph.

## 4. Exact Allowed Writes

Modify only:

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs`
2. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs`
3. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs`
4. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs`

Add only:

5. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemInteractionAuthorization.cs`
6. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemInteractionAuthorization.cs.meta`
7. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemInteractionAuthorizationTests.cs`
8. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemInteractionAuthorizationTests.cs.meta`

This Guard-owned Task Contract is read-only to development. No report is
required.

## 5. Forbidden Writes

- all Scene, Prefab, meta/importer and ProjectSettings/BuildSettings files;
- `C1FormalItemSessionAndArrangementAuthority.cs`, ItemSystem, Item identity,
  shape, placement, rotation, lighting, Build, Core and catalog truth;
- formal Item detail projection and popup work;
- Unified/Mainline Host, navigation, exact popup code and assignments;
- Battle clock/session/pause, Enemy, Reward/Drop, Save, Progression and RunFlow;
- BuildSandbox, CoreLoopLab, V02 Inventory and dev-only runtime graphs;
- VFX assets/runtime, AGENTS, LOCKED, Queue, Notion and all Git/worktree work.

Do not make internal view members public solely for tests. Tests must consume
the intended public contract or bounded reflection across the real
Assembly-CSharp / Assembly-CSharp-Editor boundary.

## 6. Immutable Public Contract

The public Item seam must expose local-convention equivalents of:

- mode enum with exactly `BATTLE_LOCKED` and `PREPARE_ENABLED`;
- immutable request containing product context, non-empty session token,
  positive reset generation, expected session canonical signature and mode;
- immutable result/current snapshot containing accepted, changed, diagnostic,
  resolved mode, source lineage and deterministic canonical signature;
- one Presenter method to apply authorization and one read-only current result;
- one idempotent change event only if Mainline needs observation. Mainline must
  not mutate Card/Board/Tray components directly.

Validation is fail closed. Wrong context, token, generation or signature leaves
the Presenter locked and cannot preserve an earlier enabled authorization.

## 7. Required Behavior

1. A new bind, reset/rebind and unbind begin or end in `BATTLE_LOCKED`.
2. `PREPARE_ENABLED` is accepted only against the exact current formal session
   token, reset generation and canonical signature. Duplicate identical
   requests are accepted with `Changed=false`.
3. `BATTLE_LOCKED` immediately resets Presenter interaction state, clears Board
   preview/drag/rotate feedback, cancels Board pressed-item state, and cancels
   every bound Card gesture/drag visual without committing or moving anything.
4. Presenter is the hard authorization boundary. Selection, begin/update/end
   Card drag, begin/update/end Board drag, rotation-zone transitions,
   preview/candidate creation and command submission all reject while locked.
5. Board and Card pointer handlers also fail closed locally so a gesture cannot
   be staged before the Presenter sees it. Vertical Tray scrolling may remain
   available while locked, but it must never convert to an Item drag or click.
6. Switching mode during an active drag produces no command, no placement
   mutation and no stale callback when the old pointer later sends Drag/EndDrag.
7. Reset/rebind to a different generation or session invalidates the previous
   authorization before publishing the new snapshot. A stale enable request
   cannot unlock the new session.
8. Existing placement, footprint, rotation adapter, collision/bounds rejection,
   Tray scrolling, selection/detail semantics and exact artwork/layout remain
   byte/semantic-equivalent outside the authorization gate.
9. Item never calls Battle pause/resume, changes time scale or controls Prepare
   surface visibility. Mainline must order pause -> enable and lock -> resume.

## 8. Failure And Stop Conditions

- Any need to write Prefab/Scene or add serialized references:
  `ARCHITECTURE_REASSESS_REQUIRED`.
- Any need to move pause/time ownership into Item or placement truth out of
  `C1FormalItemSessionAuthority`: `OWNERSHIP_BOUNDARY_BLOCKED`.
- Concurrent drift in an allowed existing file before first write:
  `VERIFIED_WRITE_CONFLICT`; do not silently refresh.
- Mainline/Unified source drift is external and read-only; record it without
  changing this package baseline unless it changes the public consumer
  contract, in which case stop with the exact incompatibility.
- A compile/test design that ignores the real Editor assembly boundary must be
  corrected before dispatch completion; no public gameplay API is added only
  to satisfy tests.

## 9. Task-Start Baselines

Allowed existing files:

| Path | SHA-256 |
|---|---|
| `C1ExactBattleSandboxItemArrangementPresenter.cs` | `C76888A9B9EE17667B14D9BF908A1EA7E78A81C140F4271C39DAB15EA048E07C` |
| `C1ExactBattleSandboxItemBoardView.cs` | `620AA1F9A9BB2BE45E0406262900304AD603983DEE720F10157DD87129E14BD4` |
| `C1ExactBattleSandboxItemTrayView.cs` | `1C09886A117CD5806E8915B8565780B3A19E04AFED371ACDCC3B9C817A30425D` |
| `C1ExactBattleSandboxItemCardView.cs` | `7EEA77EFE28959E51C3986B046C4A739EAA7E45847FB103FE6D4770666CCE3A0` |

New package files 5-8 must be absent at task start.

Protected baselines:

| Path | SHA-256 |
|---|---|
| `C1FormalItemSessionAndArrangementAuthority.cs` | `935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317` |
| `C1FormalItemDetailProjectionAndSelection.cs` | `A8232FACA886D931B035A564420B07F4D0C66808BB04CE92BF3E02BFAF26D432` |
| exact Card Prefab | `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` |
| exact Board Prefab | `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` |
| exact Tray Prefab | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |
| `UnifiedBattleFormalSceneHost.cs` read-only intake | `FCFAA8DD95C2B7CB463EE78B726404D6957EEE96D5AED588AF0A56C73537A937` |

## 10. Verification

Minimum evidence:

- current-source Runtime compile using the actual Assembly-CSharp boundary;
- current-source Editor compile using the actual Assembly-CSharp-Editor
  boundary;
- focused deterministic tests for default locked state, valid enable, duplicate
  no-op, wrong context/token/generation/signature rejection, stale enable after
  reset, and deterministic result signatures;
- focused input matrix covering Card click/long-press/item drag, Board click and
  drag, rotation-zone transition, preview and commit in both modes;
- active-drag -> lock, reset, rebind and unbind cancellation: zero commands,
  zero placement changes, no feedback/gesture residue and stale EndDrag no-op;
- regression checks for Tray vertical scroll while locked, existing placement
  candidate identity, four rotations, cancellation and detail selection when
  enabled;
- static proof for no time/pause calls, no runtime hierarchy creation, no
  BuildSandbox, no Prefab/Scene writes and no forbidden path change;
- exact whitelist, protected hashes, whitespace and temporary-output cleanup.

Explicitly not required:

- Unity batch, Fresh Play, Scene/Prefab authoring/import, reports or component
  user handtest.

`UNITY_NOT_REQUIRED`. Do not start, wait for, poll, close or clean Unity.

## 11. Completion And Dependency Release

Success terminal:

`C1_FORMAL_ITEM_INTERACTION_AUTHORIZATION_PASS / PACKAGE_COMPLETE`

- `TECHNICAL_PREREQUISITE_COMPLETE`;
- `PlayerVisibleDelivery=NONE`;
- `MilestoneCompletionEvidence=NO`.

Terminal sync must list exact changed files, public APIs, compile/tests,
protected baselines and no Unity/Git. Item Guard performs one close review and
releases the public seam to Mainline Flow Guard
`019f45d7-a33b-7630-a049-976481bb7b6d` for same-package pause/visibility
orchestration in visible task `019fc0d0-be0b-7280-bebf-5600250eec24`.

No user handtest belongs to this component package. Mainline owns the final
normal-Editor real-path test and cannot claim readiness until pause/enable,
lock/resume, visibility and continuous Battle state are integrated.
