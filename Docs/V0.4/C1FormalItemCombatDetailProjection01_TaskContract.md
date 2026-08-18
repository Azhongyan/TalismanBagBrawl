# V0.4-C1FormalItemCombatDetailProjection01 Task Contract

Status: `CONTAINED_ONE_GUARD / BOUNDARY_FROZEN`

## 1. Outcome

Expose the released `CAMPAIGN_NORMAL_LV1` Item combat facts in the existing
formal Item-detail projection so the mounted `ItemDetailPanelView` can explain
the real Battle result without calculating or owning damage.

Required released facts:

- I001: base direct damage `82`, trigger interval `2` seconds,
  `LIT_REQUIRED`;
- I002: base direct damage `28`, trigger interval `2` seconds,
  `LIT_REQUIRED`;
- placement and lighting eligibility must state whether the selected instance
  currently contributes to Battle;
- an unplaced or unlit Item must be described as not contributing. It must not
  be represented as zero damage or an unknown successful trigger.

Terminal cap:
`ITEM_COMBAT_DETAIL_PROJECTION_COMPLETE / PlayerVisibleDelivery=NONE /
MilestoneCompletionEvidence=NO`.

Mainline remains responsible for mounting the popup and the final normal-Editor
real-path handtest.

## 2. Player Promise And Fidelity

- MilestoneId: `P0_FORMAL_REALTIME_LOOP_ITEM_DAMAGE_READABILITY`.
- ProductContext: `CAMPAIGN_NORMAL_LV1`.
- PlayerPromise: selecting I001 or I002 in the formal arrangement explains the
  real direct-damage number, cadence and lighting condition that Battle uses.
- PlayerMustSee: real name/artwork, base direct damage, trigger interval,
  trigger requirement and current contribution state.
- MustChangeLive: moving or lighting the same authoritative instance updates
  the eligibility explanation from the current formal snapshot.
- ExplicitNonCompletion: identity/art-only detail; hard-coded presentation
  numbers; BuildSandbox/sample/debug rows; a popup that calculates damage; an
  unplaced/unlit Item described as contributing; a mounted popup without these
  rows.

Capability coverage:

| Capability | Owner | Carrier | Status here |
|---|---|---|---|
| Released I001/I002 damage/cadence/requirement | Item data | `C1Lv1StarterItemBaselineCatalog` | READ_ONLY_SOURCE |
| Instance placement and lighting eligibility | Item authority | `C1FormalItemSessionSnapshot` / ItemSystem snapshot | READ_ONLY_SOURCE |
| Immutable combat-detail facts and visible view-model rows | Item | formal detail projection | OWNED_HERE |
| Popup mount and real-path display | Mainline | Unified exact detail presenter/carrier | DOWNSTREAM |

This package is a technical readability prerequisite. It cannot claim popup
visibility, runtime integration, user acceptance or milestone completion.

## 3. Classification And Gates

- Class: `CONTAINED_ONE_GUARD / P0_READABILITY / PURE_CODE`.
- Primary Guard: Item Guard.
- Development Owner: visible, unarchived, direct-local task
  `019fc279-2bb0-7e61-a2f2-d9549b564cfb`.
- User Review Gate: `NO / EXISTING_USER_AUTHORIZATION`.
- User decision: `NONE`.
- Unity Technical Director: `NOT_REQUIRED`; no carrier, Scene, Prefab or
  runtime-graph choice changes.
- Milestone Fidelity Gate: applied; PlayerVisibleDelivery remains `NONE`.

Producer gate:

- classification: `READABILITY / P0`;
- player choice: understand which Item caused the visible damage and whether
  moving/lighting it changes contribution;
- minimum existence proof: I001/I002 exact facts plus placed/lit eligibility;
- stop condition: no balance, formula, affix, Build or richer-detail expansion.

Balance gate decision:

> Do not tune or simulate values; verify exact lineage and display the released
> CAMPAIGN_NORMAL_LV1 facts 82/28 and 2 seconds without changing them.

No sensitivity run is required because this package makes no tuning decision.
Unknown, unsupported and not-applicable values must remain explicit and must
not be coerced to zero.

## 4. Ownership

- `C1Lv1StarterItemBaselineCatalog.TryGetProjection(...)` is the sole source
  of direct damage, cooldown/interval and lighting requirement.
- `C1FormalItemSessionSnapshot` remains the sole source of selected instance,
  placement and ItemSystem lighting truth.
- `C1FormalItemDetailProjectionAndSelection` owns validation, immutable combat
  detail projection and construction of player-visible view-model rows.
- `ItemDetailPanelView` remains a presentation consumer. It does not calculate
  or infer combat values.
- Battle remains the owner of clock, triggering, target validation and damage
  application.
- Mainline remains the owner of popup mounting/open-close composition only.

## 5. Exact Allowed Writes

Modify only:

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemDetailProjectionAndSelection.cs`
2. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemDetailProjectionAndSelectionTests.cs`

The Task Contract is Guard-owned and read-only to development. No new runtime,
meta, report, Scene or Prefab file is required.

## 6. Required Public/Immutable Result

The existing `C1FormalItemDetailProjection` must expose an immutable combat
detail fact or equivalent with:

- schema/version and `CAMPAIGN_NORMAL_LV1` context;
- authoritative base Item and rarity-version identity;
- direct damage;
- cooldown/trigger interval in the released exact unit;
- lighting requirement;
- placement presence and nullable authoritative lit fact;
- explicit contribution state: contributing, unplaced, unlit, or not
  applicable;
- source projection canonical signature and deterministic combat-detail
  canonical signature.

For I001/I002, source lookup must use the exact context, base identity,
`baseItemId@white`, and released profile revision, then require its projection
signature to equal the roster row's
`baselineProjectionCanonicalSignature`. Mismatch rejects the open request
without replacing a previously accepted result.

I031 remains a special lighting source. It may expose an explicit
not-applicable combat-detail state, but it must not receive a fabricated zero
damage/cooldown row and must continue opening normally.

## 7. Visible Model Requirements

The defensive `CreateViewModelClone()` result for I001/I002 must carry the
released facts into the existing player-visible fields and sections consumed
by `ItemDetailPanelView`:

- name and artwork remain the existing authoritative catalog/artwork values;
- `displayPrimaryStats` and the visible `stats` section include base direct
  damage (`82` or `28`) and trigger interval (`2 seconds`);
- `displayTriggerText` and the visible `trigger` section say that placement and
  lighting are required and that a lit Item triggers at the released interval;
- `displayBasicEffects`, `battleEffectPreview` and the visible `basic` section
  explain the direct-damage effect and current eligibility;
- placed+lit: clearly contributing;
- placed+unlit: clearly not contributing because it is unlit;
- unplaced: clearly not contributing because it is not on the Board.

Existing unrelated authored sections are preserved. Updating the three owned
section keys (`stats`, `trigger`, `basic`) must be deterministic, must not
duplicate rows on repeated projection and must work whether the catalog model
already contains those sections or not.

## 8. Validation And Failure Behavior

1. Missing/failed baseline lookup, wrong base/rarity/context/revision,
   non-positive released damage/cooldown, unsupported lighting requirement or
   baseline signature mismatch rejects fail-closed.
2. Unplaced is not unlit: placement and lit absence remain explicit.
3. A placed Item must use the already-validated ItemSystem placement; no second
   placement or lighting lookup is introduced.
4. No display-name, artwork, rarity, Item ID branch may supply numbers.
5. No Battle session, cue or damage event is used as a replacement Item-data
   owner.
6. Repeated identical open remains `Accepted / Changed=false` with stable
   signatures and byte-equivalent visible combat rows.
7. Existing stale token/generation/signature, reset/unbind and defensive clone
   behavior remains unchanged.

## 9. Required QA

- actual current-source `Assembly-CSharp` compile using the Unity response-file
  assembly boundary;
- actual `Assembly-CSharp-Editor` compile against the newly compiled Runtime
  reference;
- focused deterministic tests proving:
  - I001 board-lit = 82 / 2 seconds / contributing;
  - I001 tray = 82 / 2 seconds / not contributing because unplaced;
  - I002 board-lit = 28 / 2 seconds / contributing;
  - I002 board-unlit and tray states are explicit non-contribution;
  - exact baseline signature lineage and mismatch rejection;
  - player-visible `stats`, `trigger`, `basic` sections contain the released
    facts with no duplicates;
  - I031 remains openable and no zero combat facts are fabricated;
  - defensive clone, duplicate open, stale/reset/unbind and existing rejection
    matrices remain passing.

Unity, Play, batch mode, Fresh Play and serialization are forbidden because the
seam is pure code/data projection. Mainline owns the final runtime popup test.

## 10. Protected Boundaries

Do not modify:

- `C1Lv1StarterItemBaseline.cs` or any accepted number/profile;
- `ItemDetailViewModel.cs`, `ItemDetailPanelView.cs`, detail section UI or
  detail Prefab;
- formal Item authority, Presenter, BoardView, TrayView, CardView, Board/Tray/
  Card Prefabs and REV04F1 behavior;
- Mainline/Unified host, Scene, navigation, popup carrier/serialization;
- Battle contracts/session/adapter/application, Enemy, Reward/Drop, Save,
  Inventory persistence, VFX, BuildSandbox, ProjectSettings or Packages;
- AGENTS/LOCKED/Queue/Notion/Git/worktree.

If the existing view model cannot render the three player sections without an
ItemDetailViewModel/UI/Prefab change, stop at
`ARCHITECTURE_REASSESS_REQUIRED` with the first exact missing field. Do not
broaden silently.

## 11. Task-Start Baselines

Writable files:

| Path | SHA-256 |
|---|---|
| `C1FormalItemDetailProjectionAndSelection.cs` | `A8232FACA886D931B035A564420B07F4D0C66808BB04CE92BF3E02BFAF26D432` |
| `C1FormalItemDetailProjectionAndSelectionTests.cs` | `6BC22A9A89401B31D16FB8F0B23680A18CD8934C1660F6DAA724F0D7BD5B7C16` |

Protected baselines:

| Carrier | SHA-256 |
|---|---|
| `C1Lv1StarterItemBaseline.cs` | `A4C454FFCC0A3B5CE1BF2181A39B79CA73C809FD81FD5D3AA3257C7908A140C7` |
| `ItemDetailViewModel.cs` | `8A73A20B91BC79F546456584CCAAE38C2BBE384DEF6A27AB56F0715C98EE0689` |
| `ItemDetailPanelView.cs` | `045D20E2CC0D2ED87D477AB48BE7A00F954E3FE976904498502157239E534A90` |
| `C1ExactBattleSandboxItemArrangementPresenter.cs` | `99AE7C8F54DF002F9F45814AA251B1140B399BA9474936BE11F24AF63B91D043` |
| `C1ExactBattleSandboxItemBoardView.cs` | `8B987D53F50733445D09673848283A1D24C4D1F64C18E7A9000407418052054E` |
| `C1ExactBattleSandboxItemTrayView.cs` | `548C19747503F5B458D8BC99FB7E64C06E33C943E8784B1458943D0FAF9CB764` |
| `C1ExactBattleSandboxItemCardView.cs` | `4607F7CB7446DCDA9585CF71FFB29F99E3C3A18A7C07E0309D80142A006C19C2` |
| `C1ExactBattleSandboxItemDetailPresenter.cs` | `6A2D78CDE8680F72DABB4574B84F215CC3405CE8CD15D76AD8B3A83E5D039490` |
| exact Tray Prefab | `6FA5F995433B4433B201613CA7FBA939779F6664E44E19CC02ACA060E4F4D552` |
| exact Board Prefab | `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` |
| exact Card Prefab | `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` |

Before first write, development must verify the two writable hashes and the
listed protected baselines. Any overlap/drift stops as
`VERIFIED_WRITE_CONFLICT`; unrelated disjoint Mainline Battle/Bridge changes
are external and must not be restored or rebased by this task.

## 12. Terminal Receipt

Return once to Item Guard with:

- exact changed files and final SHA-256;
- public immutable combat-detail API fields;
- separate Runtime/Editor compile evidence;
- focused assertion count and exact I001/I002 visible rows;
- protected baseline reconciliation;
- confirmation of no Unity, Scene/Prefab, Mainline/Battle/VFX, Git or worktree
  operation;
- dependency release note for Mainline Phase B.

Successful terminal only:
`ITEM_COMBAT_DETAIL_PROJECTION_COMPLETE / PlayerVisibleDelivery=NONE /
MilestoneCompletionEvidence=NO`.
