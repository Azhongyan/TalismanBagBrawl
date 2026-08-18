# V0.4-BattleSandboxExactBoardTrayItemEditorPromotion01 Task Contract

Status: `CONTAINED_ONE_GUARD / REV03_FOOTPRINT_PRESENTATION_ADAPTER / DEV_ACTIVE`

REV03 footprint presentation correction authority:

- User real-path evidence overrides the earlier Prefab/parity PASS: authoritative
  I001 occupies one Board cell, but the returned Tray card still renders at the
  fixed `285x285` carrier size and appears to occupy about four Board cells.
  The exact Item module remains `USER_HANDTEST_FAIL` until the user retests the
  correction in the normal Editor.
- Read-only attribution confirms that Mainline mounts the exact Board/Tray
  Prefabs without a scale override. Board rendering already derives its bounds
  from authoritative occupied cells. The first missing layer is entirely the
  package-owned exact Card/Tray/Presenter presentation footprint adapter.
  Item shape/catalog truth, formal Item authority, ItemSystem, Board placement,
  Mainline Scene/host and Unified mounting remain unchanged.
- The same instance must use one screen-visible cell unit in both containers.
  The adapter derives the Board cell unit from the authored Board cell geometry
  and transforms it into the Tray/Card parent coordinate space. It must not
  hard-code the current `140`, `160`, `275` or `285` measurements as the runtime
  truth and must not infer shape from item ID, display text, Sprite dimensions
  or rarity.
- Tray footprint width and height derive generically from the authoritative
  local shape cells after the presentation rotation is applied. A one-cell
  item is one visual cell in Tray and Board. Line, corner, square, I002, I031
  and future shapes use the same mapping. Shape cell count remains capacity
  truth only and cannot substitute for normalized shape bounds.
- The exact Card Prefab owns authored visual/input/selection/lighting/drag
  footprint anchors. Runtime may size those authored anchors from the immutable
  footprint presentation model; it may not create fallback hierarchy, replace
  artwork, stretch Sprite pixels, or change accepted artwork aspect. The final
  artwork and all overlays/raycast surfaces must stay aligned to the same
  footprint bounds. `Image.preserveAspect` remains required.
- The Presenter is the only translation seam. It reads current formal Item
  snapshot/catalog facts, obtains the authored Board cell geometry from
  BoardView, and supplies one immutable footprint presentation model to
  TrayView/CardView. No presentation size becomes Item state and no second
  placement or shape owner is introduced.
- Board placement, formal rotation degrees, visual quarter turns, preview and
  commit candidates remain byte/semantic-equivalent to REV02. Tray sizing must
  not mutate Board rotation, occupied cells, anchors, collision, lighting or
  Build facts. Tray -> Board, Board -> Board, Board -> Tray, reject/cancel,
  rebind/reset and scrolling remain atomic and idempotent.
- REV03 overrides only the older sentence in Required Behavior 6 that prohibited
  runtime carrier rescaling. Runtime footprint sizing is now required, but only
  through the authored anchors and authoritative generic mapping above. It does
  not authorize restyling, approximate redraw, Scene geometry changes or
  Mainline mount scaling.

REV03 exact existing-file write whitelist:

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs`
2. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs`
3. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs`
4. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs`
5. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionEditor.cs`
6. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs`
7. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab`
8. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab`
9. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab`

The three Prefabs are allowed only for the minimum serialized footprint anchors
or references proven necessary by the correction. Preserve their GUID/meta
files exactly. Do not rewrite a Prefab that does not need a serialized delta.
All other original package paths, reports and metas are read-only for REV03.

REV03 task-start hashes:

| Path | SHA-256 |
|---|---|
| `C1ExactBattleSandboxItemArrangementPresenter.cs` | `61DED546476469A4691B806525FDBE76AA6B393BA7F40F36B2B7DFB160B1D3CB` |
| `C1ExactBattleSandboxItemBoardView.cs` | `E95024633A5D2F58577F50AAC2544445ABA642B866DB006AEB51A27CBFBAE209` |
| `C1ExactBattleSandboxItemTrayView.cs` | `FEB1933517259DD07145645FCA00B1498664ED56B3924C22B83B5242458B34A8` |
| `C1ExactBattleSandboxItemCardView.cs` | `4A22274F9521D6C2576E9042E185EBF6A0ADF209B950BDC29D3B20121B3EC4DE` |
| `C1ExactBattleSandboxItemPromotionEditor.cs` | `572B5FAE3F4399B804C181DB95AE7CE7946DF9E4E9DE5E03682A003E35BA9E90` |
| `C1ExactBattleSandboxItemPromotionTests.cs` | `297DE1F0B9BEFA24D3219DEF4B1DAEC72A31A6BA05F2561CFAE150D1F4E4A2F9` |
| `C1ExactBattleSandboxItemBoard.prefab` | `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` |
| `C1ExactBattleSandboxItemTray.prefab` | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |
| `C1ExactBattleSandboxItemCard.prefab` | `B03AB01A3EDD0CBD4A4128DAF35445E1F2FBFFE4269967613188314439E10B46` |

REV03 protected overlap hashes:

| Read-only path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs` | `935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317` |
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `794C3A6D9DBE9FB1FF96275C247BCEF4D9D248EDC1DF3EAEE6D396DD87969827` |
| `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs` | `B83B23725B3F1D03D339D0B2181232F1EA68B367AF3947C688E61449BF8015BB` |
| `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab` | `D5834DB6C2E3EBACC583418033E06F26D8B258B75AC78C8FBE6B13B1544D202E` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` | `9FED79FE312A34A6B802F464F928DFD032AF4615EBEE4715A7BF73DEC3BE3058` |
| `ProjectSettings/EditorBuildSettings.asset` | `CF244226FFD286638DEC957BBE5A9ABCA1DE52E9D529D71D406B5D03DE805932` |

REV03 minimum QA and close gate:

1. Current-source scoped Runtime and Editor compile.
2. Focused deterministic checks for Board-cell-to-Tray-space conversion under
   the current authored transforms; generic normalized bounds for one-cell,
   line, both corner orientations and square shapes; I001/I002/I031 mapping;
   four rotation round trips; preview/commit unchanged; reject/cancel/reset
   restoration; artwork aspect and input/overlay footprint alignment.
3. At most one owned Unity Prefab authoring/validation pass, only if serialized
   Prefab changes are required. No Play, Fresh Play loop or Scene save. If code
   alone is sufficient, do not start Unity.
4. User performs the only runtime acceptance in a normal Editor. The shortest
   handtest is I001 Tray -> Board -> Tray plus I002/I031 and representative
   line/corner/square shapes, four rotations, scroll, invalid drop and cancel.
5. Success may claim only
   `DIRECT_ITEM_MODULE_USER_HANDTEST_READY / FAITHFUL_PARTIAL`,
   `PlayerVisibleDelivery=PARTIAL`, `MilestoneCompletionEvidence=NO`.
   It may not release VFX P2 or claim the full formal milestone before user PASS.

REV03 stop conditions:

- Any required change to Item shape/catalog, formal Item authority, ItemSystem,
  Board placement/collision/rotation truth, Mainline Scene/host, Unified Prefab,
  VFX P1 assets, Battle, Enemy, Reward or Save returns the first exact seam.
- Any new runtime hierarchy, item-ID sizing branch, Sprite/text-based shape
  inference, or second footprint truth is forbidden.
- If one bounded Prefab authoring pass fails objectively, stop with exact
  blocker and cleanup evidence. Do not loop Unity or manufacture readiness.

REV01 recovery authority:

- The first owned authoring attempt, PID `10992`, is classified as
  `AUTHORING_TOOL_BATCH_EMPTY_SCENE_SETUP_FAILURE`.
- It produced no accepted output: package cleanup removed whitelist paths
  13-20, the source Scene and all protected files remained exact, and owned
  processes/locks exited cleanly.
- The package-local compatibility defect is corrected in whitelist path 9 by
  guarding `RestoreSceneManagerSetup` behind `setup.Length > 0`.
- Exactly one replacement authoring/validation pass is authorized in the same
  visible task. This replaces the consumed failed pass; it does not add a
  general retry budget.
- All original product semantics, 20-path whitelist, protected baselines,
  evidence classes, stop conditions, no-Scene-save rule, no-Git rule and
  maximum completion status remain unchanged.
- If the replacement pass fails objectively, do not run Unity again. Return
  the exact first blocker with cleanup and protected-hash evidence.

REV02 recovery authority:

- The REV01 replacement authoring pass, PID `27684`, is preserved as an
  objective focused-test failure, not accepted package evidence. It compiled
  and scanned the exact source successfully, then failed before accepted
  Prefab/parity output because the new presentation carrier submitted visual
  quarter-turn value `1` to the formal authority, whose public ItemSystem
  contract requires rotation degrees `0/90/180/270`.
- Cleanup removed whitelist paths 13-20; paths 1-12, all 19 protected
  baselines, the source Scene, existing Prefabs and owned process/lock state
  remained exact. No REV01 output is accepted.
- This is one missing adapter inside the package-owned presentation boundary.
  Formal authority and ItemSystem remain unchanged. Presentation interaction
  uses quarter turns `0/1/2/3`; formal candidates and snapshots use degrees
  `0/90/180/270`.
- The code correction is limited to whitelist paths 1, 3, 7 and 11. Implement
  one centralized reversible conversion: visual quarter turns -> formal
  degrees before command/candidate submission, and formal degrees -> visual
  quarter turns during snapshot/card/board hydration. Unknown formal angles
  fail closed. Preview geometry, artwork rotation, rotate feedback and local
  interaction stay in visual quarter-turn units; committed snapshot assertions
  use formal degrees.
- Focused tests must cover exact round trips for all four rotations, reject
  invalid formal angles, prove preview/commit candidate consistency, and expect
  the first right rotation to commit as formal `90`, not `1`. No Item shape,
  anchor, coordinate, placement, rotation-direction or authority semantics may
  change.
- After current-source Runtime/Editor compile and focused static checks pass,
  exactly one final owned Unity authoring/validation pass is authorized in the
  same visible task. It may only produce paths 13-20 under the original rules.
  This is the final package pass; any objective repeat failure returns the
  first exact blocker with no further Unity retry or product-code workaround.

## 1. Outcome

Promote the already accepted BattleSandbox Board, Item Tray, and reusable Item Card presentation into exact authored Prefab carriers. Provide one Editor-only `Scan Source -> Extract/Update Exact Prefabs -> Compare/Validate Parity` surface and one narrow formal presentation adapter that consumes `C1FormalItemSessionAuthority` without copying BattleSandbox runtime truth or controller ownership.

This package is a technical prerequisite only.

- `PlayerVisibleDelivery: PARTIAL`
- `MilestoneCompletionEvidence: NO`
- strongest allowed success status: `TECHNICAL_PREREQUISITE_COMPLETE / PACKAGE_COMPLETE`
- forbidden status: `USER_HANDTEST_READY`, `MILESTONE_INTEGRATED`, or milestone `COMPLETE`

## 2. Immutable Player Promise Card

- `MilestoneId`: `P0_FORMAL_REALTIME_LOOP_EXACT_BATTLESANDBOX_PRESENTATION`
- `ProductContext`: `CAMPAIGN_NORMAL_LV1`
- `PlayerPromise`: In the formal bridge battle page, the Board, Item Tray, and Items that the player sees and operates are exactly the accepted BattleSandbox presentation while reading formal Item state.
- `PlayerMustDo`: click, long-press drag, rotate; Tray -> Board, Board -> Board, Board -> Tray; scroll the Tray; rearrange using first-clear I002.
- `PlayerMustSee`: exact BattleSandbox dimensions, scale, anchors, positions, spacing, hierarchy, Image/Sprite, fonts, Mask/ScrollRect, item-art proportions, selection, drag, and rotation feedback.
- `MustChangeLive`: when formal Item roster, placement, or lighting changes, exact carriers update data and state only; accepted layout and art do not change.
- `PersistenceOrResetExpectation`: scene re-entry or session reset rebuilds from the formal Item snapshot; Editor tooling is never a Player/APK runtime dependency.
- `ExplicitNonCompletionCases`: approximate or code-redrawn UI; current C1FormalItem Board/Tray/Card remaining as final visible layer; EditorWindow-only preview with no formal consumer; copied images without exact geometry/hierarchy/interaction; copied BuildSandbox runtime injection, Lv40 fixture, dev panel, or second state owner.
- `RequiredRealPath downstream`: WorldMap -> 1-1 exact BattleSandbox-derived visible battle -> I002 -> exact Tray/Board rearrangement -> 1-2 -> WorldMap.
- `UserAcceptedAt`: `2026-08-02`, including “大小、image都要一模一样” and the Editor-promotion request.

The promise is immutable for this package. Shared nouns do not permit weaker semantics.

## 3. Task Class And User Review Gate

- `TaskClass`: `COMPLEX_GUARDED_ONCE`
- Reason: player-visible Sandbox-to-formal Prefab promotion, Unity serialization, interaction carrier extraction, and a formal authority adapter cross a presentation architecture boundary.
- `UserReviewGate`: `YES / ACCEPTED`
- Accepted scope marker: `ITEM_GUARD_ROUTING_REQUEST_BATTLESANDBOX_EXACT_BOARD_TRAY_ITEM_EDITOR_PROMOTION`
- No unresolved product choice exists.
- Producer/Balance gate: `NOT_APPLICABLE`; this package changes no gameplay number, probability, Build threshold, cadence, or balance authority.

## 4. Technical Director Decision

- Player-visible outcome: exact accepted BattleSandbox Board/Tray/Card in the formal page, backed by formal Item truth.
- Product context: `CAMPAIGN_NORMAL_LV1`.
- Authoritative state owner: `C1FormalItemSessionAuthority`; `ItemSystemSnapshot` remains placement/lighting evidence inside that authority.
- Chosen Unity carriers: three new reusable authored Prefabs plus four same-named runtime presentation MonoBehaviours and one Editor-only extraction/parity surface.
- Existing Scene versus new Scene: no new Scene. The BattleSandbox Scene is read-only source; Unified remains the downstream shared formal Scene.
- Reuse: preserve source serialized hierarchy/assets/geometry and existing formal authority APIs. Do not reuse the current approximate C1FormalItem visible prefabs as final UI.
- New work: exact Board/Tray/Card carriers, serialized interaction/presentation references, and a formal presentation adapter.
- Scene impact: zero bytes written to either BattleSandbox or Unified Scene.
- Prefab impact: create only the three exact Item prefabs in this contract. Do not modify Unified or current C1FormalItem prefabs.
- Runtime dependency: no `UnityEditor`, EditorWindow, source Scene, BuildSandbox controller, fixture, or dev host dependency in Player/APK runtime.
- Mobile/APK: preserve existing UGUI Mask/ScrollRect/raycast semantics; no per-frame hierarchy search, unbounded allocations, runtime material cloning, or final-UI creation.
- Stop: unresolved scene-external references or parity that requires Sandbox truth/fixture returns a concrete carrier/reference map; do not redraw or copy the controller.

## 5. Owners

- Primary Guard: Item Guard.
- Development Owner: one visible, unarchived, direct-local Item implementation task.
- State/Data Owner: `C1FormalItemSessionAuthority` and its immutable snapshots.
- Presentation Owner: the exact Prefabs and presentation adapter created by this package.
- Source presentation truth: `Scene_TalismanBag_V04_BattleSandboxPreview.unity`, read-only.
- Downstream composition owner: Mainline/Shared Presentation, not this package.
- User decision point: `NONE`.

## 6. Capability Coverage

| Capability | Required evidence | State owner | Carrier | Package status | Package proof |
|---|---|---|---|---|---|
| Exact Board visual | Serialized parity for root, 25 cells, images, geometry, order, overlays | Formal Item authority for state; Prefab for presentation | `C1ExactBattleSandboxItemBoard.prefab` + BoardView | OWNED_HERE | parity map + Prefab Mode inspection |
| Exact Tray visual and scrolling | Serialized parity for root, viewport/content/card layer, ScrollRect/Mask/category hierarchy | Formal Item authority for roster; Prefab for presentation | `C1ExactBattleSandboxItemTray.prefab` + TrayView | OWNED_HERE | parity map + focused scroll contract check |
| Exact reusable Item Card | Exact 285-class visual scale, source art slot, hierarchy, typography, rarity/art proportion | Formal Item roster/placement/lighting | `C1ExactBattleSandboxItemCard.prefab` + CardView | OWNED_HERE | source-card role map + Prefab Mode inspection |
| Exact drag/selection/rotation presentation refs | Authored refs for board artwork, preview/invalid artwork, drag ghost and rotate feedback; no runtime fallback hierarchy | Formal Item command/result | Board/Tray/Card Prefabs + Presenter | OWNED_HERE | serialized reference validation |
| Formal Item binding | Snapshot-driven bind and command submission; no second owner | `C1FormalItemSessionAuthority` | `C1ExactBattleSandboxItemArrangementPresenter` | OWNED_HERE | focused deterministic interaction tests |
| Editor extraction/update/parity | Scan source, extract/update, compare, fail closed | Editor tooling only | `C1ExactBattleSandboxItemPromotionEditor` | OWNED_HERE | compile + parity output |
| Mount exact carriers into Unified without rescale/reflow | Player sees exact carriers in formal page | Mainline/Shared Presentation | Unified `BoardArea` and `ItemTrayArea` | DOWNSTREAM_MISSING | not proven here |
| Restore full exact BattleSandbox page composition | Player sees accepted full battle composition | Mainline/Shared Presentation | Unified shell + formal battle presentation Prefabs | DOWNSTREAM_MISSING | not proven here |
| Live formal Battle Session | Player sees realtime HP, targeting, damage and enemy response | Battle/Enemy | live Battle Session and formal presentation | DOWNSTREAM_REQUIRED | not proven here |
| Full 1-1/I002/rebuild/1-2 real path | user completes original route | Mainline integration | formal route | DOWNSTREAM_REQUIRED | user real-path evidence required later |

Any downstream row remaining missing prevents `USER_HANDTEST_READY` for the milestone.

## 7. Concrete Carrier Inventory

Present and reusable:

- read-only source `Scene_TalismanBag_V04_BattleSandboxPreview.unity`;
- source `BoardGridPreview`, `BoardGridCellsRoo`, 25 authored cells;
- source `ItemTrayPreview`, `ItemTrayViewport`, `ItemTrayContent`, `ItemCardLayer`, authored ScrollRect/Mask/category hierarchy;
- source `ItemCard_01..05`, with source root geometry and source artwork-slot evidence;
- `C1FormalItemSessionAuthority` and immutable roster/placement/lighting snapshots;
- Unified `BoardArea` and `ItemTrayArea` downstream mount slots;
- current approximate `C1FormalItemBoard/Tray/Card` prefabs as historical/backend evidence only.

Created by this package:

- exact Board, Tray, and reusable Card Prefabs;
- exact formal Item presentation Presenter/BoardView/TrayView/CardView;
- Editor-only extraction/update/parity surface;
- one concise source-to-prefab parity CSV and one close report.

Still missing but required by the player promise:

- downstream Unified mounting without rescale/reflow;
- exact full BattleSandbox-derived battle-page composition;
- live formal Battle Session presentation and target/damage/enemy feedback;
- final WorldMap -> 1-1 -> I002 -> rebuild -> 1-2 -> WorldMap user real-path proof.

## 8. Exact Allowed Writes

All paths below must be new at task start. No other path may be written.

Runtime source and metas:

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs`
2. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs.meta`
3. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs`
4. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs.meta`
5. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs`
6. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs.meta`
7. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs`
8. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs.meta`

Editor source and metas:

9. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionEditor.cs`
10. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionEditor.cs.meta`
11. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs`
12. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs.meta`

New Prefabs and metas:

13. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab`
14. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab.meta`
15. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab`
16. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab.meta`
17. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab`
18. `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab.meta`

Minimal evidence:

19. `Docs/V0.4/Reports/C1ExactBattleSandboxItemPresentationParity.csv`
20. `Docs/V0.4/Reports/C1ExactBattleSandboxItemPromotionReport.md`

Patch budget: exactly these 20 final paths. If another write is necessary, stop before writing and return the first exact seam.

## 9. Forbidden Writes And Ownership

- No modification or save of any Scene.
- No modification of existing Prefabs, including current C1FormalItem and Unified prefabs.
- No modification of `BuildGridInteractionPreviewController`, `BuildItemTrayPreviewView`, `BuildItemPreviewCardView`, `BuildGridPreviewSlotView`, ItemSystem, Item shape/catalog/artwork/importer, formal Item authority, current Presenter/Views, Battle, Enemy, Reward/Drop, Save, progression, RunFlow, ProjectSettings, BuildSettings, Packages, AGENTS, LOCKED, Queue, or Notion.
- No `BuildGridInteractionPreviewController` component or copied controller source inside final Prefabs/runtime.
- No BuildSandbox runtime injection, Lv40 fixture, category/data fixture, dev panel, runtime fallback hierarchy, `GameObject.Find`, display-text inference, or second Item state owner.
- No 31 independent Item Prefabs.
- No new Scene or custom stage editor.
- No Git command or operation; no worktree.

## 10. Required Behavior

1. The Editor surface exposes exactly three clear operations: `Scan Source`, `Extract/Update Exact Prefabs`, and `Compare/Validate Parity`.
2. It opens/reads only the exact source Scene path, refuses to run when user scenes are dirty, never saves the source Scene, and leaves the user's open Scene setup recoverable.
3. Authored source subtrees are duplicated from the source objects, not rebuilt from numeric constants. RectTransform, sibling order, active state, Image/Sprite/font/material/color, Mask, ScrollRect, GridLayoutGroup/LayoutElement and raycast fields must remain semantically identical unless the parity report records an intentional script-carrier substitution.
4. The Board Prefab preserves the exact `BoardGridPreview` root transform (`800x800`, scale `0.78` at the source carrier), its 25-cell hierarchy and exact source images. Runtime-generated Board artwork/preview, formation/lighting overlay, drag-ghost and rotation-feedback responsibilities must become authored serialized refs or a narrow serialized presentation carrier; they may no longer be created as final UI at runtime.
5. The Tray Prefab preserves exact `ItemTrayPreview` root geometry, `ItemTrayViewport/Content`, `ItemCardLayer`, ScrollRect/Mask and category hierarchy. The formal adapter may update content state but may not overwrite authored geometry or style.
6. The reusable Card Prefab derives outer geometry/component style from the common source ItemCard contract and its authoritative artwork slot from the accepted authored source slot. The tool must compare `ItemCard_01..05`, record common versus intentional role differences, and create one reusable exact carrier. Runtime data may change Sprite/text/state/rotation only; it may not rescale or restyle the accepted carrier.
7. Every attachable public MonoBehaviour class name matches its source filename. Final Prefabs contain no missing scripts.
8. The Presenter consumes only `C1FormalItemSessionAuthority` snapshots and submits formal commands through its public API. It may translate snapshot facts into presentation state, but cannot infer or store roster, placement, lighting, Build, or rotation truth.
9. Click, long-press drag, rotation, Tray -> Board, Board -> Board, Board -> Tray, rejected/canceled restore, and Tray scrolling bind to authored refs. Preview and commit consume one formal candidate; no visual-only placement truth.
10. Reset/rebind is idempotent, clears stale visuals/listeners/drag state, and reconstructs presentation from the current formal snapshot without creating duplicate cards or hierarchy.
11. Editor APIs are absent from runtime assemblies and Player/APK behavior. Unknown Item artwork or missing required authored refs fail closed with stable diagnostics; no approximate fallback UI is created.
12. Current approximate C1FormalItem Prefabs remain unchanged and are explicitly marked in the report as not final visible carriers.

## 11. Runtime-Generated Presentation Audit

The source controller currently creates or repairs these presentation roles and they must be classified in the parity report:

- `BoardItemArtworkLayer` and placed/preview/invalid artwork children;
- formation/core-power overlays;
- `MobileRotateZoneLayer`, right rotate zone, and guide;
- drag-ghost cell/artwork/invalid-artwork layers and underlays;
- selected-info close handling when it intersects Board/Tray/Card interaction.

For each role record one outcome: `PROMOTED_AUTHORED_REF`, `FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF`, `OUTSIDE_THIS_CARRIER_DOWNSTREAM`, or `BLOCKED_SCENE_EXTERNAL_REF`. A transient Play object is never captured as source truth.

## 12. Failure And Stop Conditions

- Scene-external reference cannot be mapped without changing the source Scene or another existing Prefab: stop with exact object/component/fileID/reference map.
- Parity requires Sandbox Item truth, fixture, controller lifecycle, or copied runtime graph: stop and propose only the narrow missing adapter seam.
- Source Scene hash changes before authoring or during the package: stop; do not refresh baseline silently.
- Existing target path is unexpectedly present: stop before overwrite and return ownership evidence.
- Unity serialization would write any non-whitelisted asset: abort the authoring pass and remove only package-owned incomplete new outputs.
- Missing script, missing required ref, approximate/redrawn hierarchy, root rescale/reflow, or runtime fallback creation: package FAIL.
- Mainline mounting, live Battle runtime, or final route is needed to prove this package: record downstream missing; do not broaden.

## 13. Required Sources And Frozen Baselines

Task-start SHA-256 baselines:

| Read-only path | SHA-256 |
|---|---|
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `1d7a367b803f9a7477e261a90e05d58fbab2f9faa1f3464a86806b8a9faf9fca` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `364727a582ee3f2d499c109d98939318b4d26ab9d2f1f571cb70e62e605913dc` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `06af79cb89eb4dca0a2313e81af81efa89a35b90ae6fe5e6b0bbdd3c90f62921` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | `448a69762ec157214809e1d43cd19a134d77f7928742cc890a94a41b1588d56e` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | `d232a34f6a761d745564c5015c977f9eef26e2c0d23cd764b6cfa2704d008214` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs` | `935a699e038866f3ad5b870a8982a1100fbbdb666c76c23457638d0206160317` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemArrangementPresenter.cs` | `6eb87259caca6132e44ff5139b67d19e6dcc6ed7c9988877171792338373ee08` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemBoardView.cs` | `6a3a155198568870e2f0de936d2e3a88d3dcfb6314b83347e6fa0b5d179c591f` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemTrayView.cs` | `743a7f11b1b4f1f1855a0e6c98cc6f59823913334294a7282a9045461591583d` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemCardView.cs` | `e2871981b734b4e9caa09b60e131bc5e6d2f311d309a195c6e28db60e3fae41b` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemBoard.prefab` | `984eea4e7a33d86b29f16a1c5b59d26c08889d79e5a8f6e9d1c9b969f41defdd` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemTray.prefab` | `416e534214b429a21a2ec7ec431eddb324df3a4edaade87155e2817cf35a5605` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemCard.prefab` | `9d97b2b612f65fd0d1574fe13c690416523d7fe19a740f3854c4fc1aaad06868` |
| `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.cs` | `25283bca93ba382eab133cefab394a66d7f6eb6be87014469c38face1157bd01` |
| `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattlePageShellSlotNames.cs` | `22416af3c3f7b2553e4d1e074a9aa82a0737f234313a5cf7d27cec9c2680d906` |
| `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs` | `c0b6b68e15e0803896ae3d0248611c8f944428ba7daaf554a345d401cfc5d898` |
| `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab` | `5e64146954350bd4f16877c9328de371b0a16a2c3f2ffd7855727e71abfbff34` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` | `fc5e64037cacb2ea00acc7ba614d404e3ab26d5ffb3cc091df0bf15b4458f1d9` |
| `ProjectSettings/EditorBuildSettings.asset` | `cf244226ffd286638dec957bbe5a9abca1de52e9d529d71d406b5d03de805932` |

All 20 allowed output paths were absent during Guard freeze.

## 14. Verification And Evidence Classes

Required minimal QA:

1. Current-source scoped Runtime and Editor compile.
2. Focused deterministic tests for snapshot bind, command submission, preview/commit candidate consistency, rejected/canceled restoration, reset/rebind idempotence, listener/card uniqueness, and no BuildSandbox runtime dependency.
3. REV02 permits exactly one final owned Unity authoring/validation pass after
   the frozen rotation-unit adapter correction is write-quiescent and current-
   source Runtime/Editor compile plus focused static checks pass. It may open
   the source Scene read-only and create/update only the three new Prefabs plus
   parity outputs. It may not Play or save a Scene. PID 10992 and PID 27684
   remain failed evidence; this final pass grants no additional retry budget.
4. Prefab validation: missing scripts `0`; missing required refs `0`; one reusable Card carrier; Board 25 cells; exact Tray ScrollRect/Mask/content/card layer; all runtime-generated presentation roles classified.
5. Prefab Mode visual comparison of Board, Tray, representative Card and dynamic overlay refs against the source Scene. This is `PRESENTATION_INSPECTION`, not user real-path proof.
6. Recheck every frozen baseline and exact whitelist after authoring; clean all owned Unity/process/lock state.

Explicitly not required:

- no Fresh Play or repeated batch harness;
- no APK build;
- no full formal route test;
- no user handtest for this component package;
- no broad CSV/leak/report suite beyond the two allowed evidence files.

Evidence classification on success:

- contract/data: formal authority API ownership and parity map;
- compile/static: source compatibility, no forbidden runtime dependency, complete refs;
- presentation inspection: exact source-to-Prefab comparison;
- runtime integration: `NOT_PROVEN`;
- user real path: `NOT_PROVEN`.

## 15. Process Ownership

- Direct local project only: `F:\Porject\TalismanBagBrawl`.
- No worktree and no Git.
- The development task records exact Unity command/PID/log/timeout for its one authoring pass.
- It owns and cleans only its own Unity/ShaderCompiler/Bee processes and lock artifacts.
- If another Editor owns serialization, code/offline work may proceed, but the authoring pass waits or returns one exact external-occupancy blocker without asking the user to cycle Unity repeatedly.
- Package cannot close with owned process or lock residue.

## 16. Completion

Close only when all owned capability rows are implemented, exact parity and Prefab validation pass, all baselines remain exact, and only the 20 allowed paths exist as package writes.

Successful close text must include:

- `TECHNICAL_PREREQUISITE_COMPLETE / PACKAGE_COMPLETE`
- `PlayerVisibleDelivery=PARTIAL`
- `MilestoneCompletionEvidence=NO`
- exact created carriers and source-to-Prefab parity result;
- runtime integration and final real-path proof still downstream;
- no `USER_HANDTEST_READY` claim.

User acceptance cannot be inherited if the player-visible semantics change.

## 17. REV04 P0 Rotation Placement Trust Correction (Active)

This section supersedes the REV02 rotation-direction expectation and freezes the
only active development scope for the next dispatch. Earlier package evidence
remains historical; it cannot override the user's current real-path failure.

- `Status`: `CONTAINED_ONE_GUARD / SAME-PACKAGE P0 PRESENTATION ADAPTER CORRECTION / DEV_ACTIVE`
- `PlayerVisibleDelivery`: `PARTIAL`
- `MilestoneCompletionEvidence`: `NO`
- strongest allowed terminal before user acceptance:
  `CORRECTED_ITEM_INTERACTION_USER_HANDTEST_READY / FAITHFUL_PARTIAL`
- `UserReviewGate`: `YES / ACCEPTED`; the user explicitly authorized dispatch.
- Producer/Balance gate: `NOT_APPLICABLE`; no gameplay value, cadence,
  probability, Build threshold or balance fact changes.

### 17.1 Immutable Player Promise Delta

In Unified Prepare, the visible artwork, pointer/grabbed occupied cell, preview
footprint, bounds/collision decision and committed authoritative
`OccupiedCells` must describe exactly the same shape at presentation rotations
0/90/180/270. The accepted BattleSandbox right-rotate hot-zone interaction must
be present and reliable without stealing click/detail, Tray scroll or item drag.
`BATTLE_LOCKED` remains non-interactive; only `PREPARE_ENABLED` permits Item
interaction.

This is a foundation blocker. Compile, static parity and Prefab references are
not user-path proof. Final acceptance remains one normal-Editor user handtest.

### 17.2 Read-Only First-Missing-Layer Attribution

The first missing layer is entirely the exact formal Item presentation adapter,
not Item shape/catalog, `ItemSystemSnapshot`, formal Item authority, Mainline,
Battle or the mounted Unified Scene.

1. Unity UGUI renders a right rotation with negative local Z degrees.
2. Current formal artwork uses `-90 * visualQuarterTurns`, but
   `RotateVisualCell` and the visual-to-formal adapter currently use the
   opposite mathematical sign.
3. ItemSystem authority rotates raw local cells around the authored raw anchor:
   formal 90 maps `(x,y)` to `(-y,x)` and formal 270 maps `(x,y)` to `(y,-x)`.
4. Current formal Board candidate normalizes rotated cells for display but sends
   the normalized display anchor directly as the raw ItemSystem anchor.
5. Current Board pointer path stores only the item identity. It does not retain
   which occupied cell the player actually grabbed. Rotation can therefore move
   the grabbed cell under the pointer and odd quarter turns can disagree with
   commit.
6. `MobileRotateZoneLayer`, `MobileRotateZoneRight` and
   `MobileRotateZoneGuide` plus all required BoardView serialized references
   already exist in the exact Board Prefab. Their Images are non-raycasting.
   The missing behavior is code-side seek/entry/cooldown/confirm handling, not a
   missing hierarchy or serialized reference.

Frozen conversion table for the authored corner raw cells
`(0,0)|(1,0)|(0,1)` in Board coordinates where Y increases upward:

| Visual state | UGUI artwork angle | Formal authority degrees | Visible normalized corner |
|---|---:|---:|---|
| 0 | 0 | 0 | missing top-right |
| 1 right turn | -90 | 270 | missing bottom-right |
| 2 right turns | -180 | 180 | missing bottom-left |
| 3 right turns | -270 | 90 | missing top-left |

The adapter must be one reversible source of truth:
`visual 0/1/2/3 <-> formal 0/270/180/90`. Unknown formal values fail closed.

### 17.3 Candidate And Grabbed-Cell Contract

- Capture one stable raw authored shape-cell identity at pointer-down. On Board,
  resolve it from the actual hit occupied cell and current authority
  anchor/rotation. In Tray, resolve it from the actual pointer position inside
  the authoritative footprint. A pointer in a transparent/unoccupied footprint
  cell cannot silently fall back to top-left.
- Preserve that raw cell identity for the whole drag. A benign snapshot rebind
  in the same session may not erase it. Lock, cancel, unbind, reset generation,
  session identity change and drag completion must clear it synchronously.
- For every preview or right rotation, keep the transformed grabbed cell under
  the current pointer cell. Derive one immutable candidate containing visual
  quarter turns, formal degrees, normalized display anchor, raw authority
  anchor, ordered occupied cells and local legality.
- Conversion formula: rotate all raw cells with the mapped formal rotation,
  compute the raw rotated minimum, derive normalized offsets for presentation,
  then compute `authorityAnchor = normalizedAnchor - rotatedMinimum`. Preview,
  collision/bounds, command submission and post-commit assertion consume that
  same candidate. No independent recomputation is allowed.
- Accepted snapshot hydration converts formal degrees back to presentation
  quarter turns. Cancel/reject restores the exact prior authority placement and
  presentation state. Tray -> Board, Board -> Board and Board -> Tray remain the
  existing formal authority transactions.

### 17.4 Rotate-Zone Interaction Contract

Use only the existing authored rotate layer/right-zone/guide references. Do not
create runtime UI or add a second input surface.

- Zone is active only during an accepted Item drag with a Board preview and only
  in `PREPARE_ENABLED`; locking cancels it immediately.
- Port the accepted BattleSandbox right-zone behavior generically: visual size
  72 px, activation area 136x180 px, inside inset 6 px, exit padding 18 px,
  seek window 0.28 s, minimum seek age 0.02 s, minimum rightward delta 20 px,
  maximum upward drift 44 px, minimum rightward velocity 300 px/s, trigger
  cooldown 1 s, confirm visual 0.18 s and Board hold padding 36 px.
- The runtime instance may position the existing authored zone and guide around
  the active drag ghost. It may not save or rewrite authored Prefab geometry.
- Merely hovering in the zone cannot rotate repeatedly. Entry/seek, exit,
  cooldown and confirm state are deterministic and use unscaled interaction
  time. Zone/guide remain `raycastTarget=false` and therefore cannot steal
  Board drag, Tray scroll or detail click.
- Right rotation preserves the grabbed raw cell under the pointer and rebuilds
  the same immutable candidate once. No item-ID, display-name or corner-specific
  branch is allowed.

### 17.5 Exact REV04 Write Whitelist And Baselines

Only these existing files may be modified:

| Path | REV04 intake SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` | `646E0CDBAAB4C76F1A6B5165A766BE81E9D289DEE62AFC9194F53140F8C2C629` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs` | `531A0B4C6BB275E0F7E273CC84C3738099751B04F4F71BE4F9327B3E4E8ABA26` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` | `371F3BD8AEBA022B6DE01134CB6804A411146237ECE5D4B281885CADFBE0A988` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs` | `EB877B53A6D0471CFC9D7E22600BE8CE436C646E9EAA55D983F9F88CC2B33373` |

`C1ExactBattleSandboxItemTrayView.cs`, the promotion Editor, all three exact
Item Prefabs and all metas are read-only. Read-only inspection proved the
required rotate objects, references and non-raycast state already exist, so
REV04 authorizes no Prefab serialization and no Unity Editor run. If a missing
serialized reference is later proven, stop and return the exact reference/fileID
blocker; do not broaden or launch Unity under this contract.

Protected exact baselines:

| Read-only path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs` | `935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317` |
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `794C3A6D9DBE9FB1FF96275C247BCEF4D9D248EDC1DF3EAEE6D396DD87969827` |
| `Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs` | `606ACDC6538EEDA86F7631840A6ACBB47284368F198EF413293BB844833776C9` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemInteractionAuthorization.cs` | `22F94A3519186CAADBCF5FD313F17EC5D2B8769A60B0EEC4128297EB14260FE7` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `364727A582EE3F2D499C109D98939318B4D26AB9D2F1F571CB70E62E605913DC` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab` | `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab` | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab` | `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` |

The active Mainline REV10 owner has a disjoint seven-file Battle/Bridge/Mainline
whitelist and explicitly forbids Item Presenter/View, Prefab and Unity writes.
Its changing files are external active-owner state and are not REV04 hash gates.

### 17.6 Minimum QA And User Handtest

Offline evidence only before delivery:

1. Actual current-source `Assembly-CSharp` and `Assembly-CSharp-Editor` scoped
   compile with their real assembly boundary.
2. Focused deterministic matrix using real machine shape cells/core facts:
   I010 and I016 corner3, one line2 and one 2x2; four rotations; every occupied
   grabbed cell; interior and legal edge; bounds, collision, cancel/reject;
   repeated Board -> Board; Tray -> Board and Board -> Tray.
3. Assert the frozen corner table, reversible adapter, raw/normalized anchor
   conversion, pointer-cell preservation, ordered preview/commit/accepted cells,
   same-session rebind preservation and lock/reset cleanup.
4. Deterministic rotate-zone state-machine checks for seek threshold, entry,
   exit, cooldown, one-trigger-per-entry, confirmation lifetime and
   `BATTLE_LOCKED` no-op. Assert existing zone/guide Images are non-raycasting.
5. No Unity batch, Fresh Play, Scene save, Prefab write, Git or worktree.

User handtest after terminal delivery:

- In Unified Prepare, test I010 and I016 at 0/90/180/270 by grabbing every
  visible occupied cell and dropping at an interior and legal edge cell.
- Repeat with line2 and 2x2; verify artwork, preview and accepted occupied cells
  coincide and invalid/collision/cancel restores exactly.
- Exercise right-zone seek/entry repeatedly; verify one controlled clockwise
  turn per valid entry, no accidental detail/drag/scroll theft and no repeated
  hover rotation.
- Verify Tray -> Board, Board -> Board and Board -> Tray. Close Prepare and
  verify all Item movement/rotation is locked with no stale preview or gesture.

### 17.7 Stop Conditions

- Any need to change ItemSystem, shape/catalog/core, formal Item authority,
  interaction authorization semantics, Mainline/Battle/Enemy/Reward/Save/VFX,
  Scene, Prefab, ProjectSettings or BuildSettings returns the first exact seam.
- Any item-specific branch, duplicate candidate/placement truth, runtime-created
  final UI, fallback geometry, silent top-left grabbed-cell fallback or separate
  preview/commit calculation is forbidden.
- If the four-file correction cannot centralize direction, raw-anchor,
  grabbed-cell and rotate-zone behavior, return
  `ARCHITECTURE_REASSESS_REQUIRED` without widening the patch.

## 18. REV04A Obsolete Cross-Package Test Compatibility Close

Guard close review found one objective test-only blocker after the REV04 runtime
and focused matrix passed: the existing formal interaction-authorization test
still asserts the superseded direct mapping `visual 1 -> formal 90`. Section 17
and the accepted right-turn convention require `visual 1 -> formal 270`.

This is not a new product decision and does not reopen runtime implementation.
Authorize one same-package test-only correction in the same visible task:

- additional exact write path:
  `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemInteractionAuthorizationTests.cs`
- intake SHA-256:
  `2A6F2C4BD050E7566E429909026AF396421D274330165CDC3DDBD38F18424EE3`
- change only the obsolete four-rotation expected mapping and its diagnostic
  wording so it consumes the section-17 reversible adapter contract;
- do not weaken or remove any interaction lock, stale callback, drag cleanup,
  detail selection, authority mutation or lifecycle assertion;
- no runtime source, Prefab, Scene, meta, report, Mainline, Battle or authority
  write is authorized by REV04A.

Minimum close evidence:

1. actual separate Runtime and Editor assembly compile still passes;
2. REV04 deterministic suite still passes;
3. the complete interaction-authorization focused suite passes, including the
   corrected four-rotation matrix and all original lock/cleanup assertions;
4. all section-17 protected baselines remain exact and the four REV04 runtime
   files remain at their reported terminal hashes;
5. no Unity, Git or worktree operation.

Only after this test-only close may the package return
`CORRECTED_ITEM_INTERACTION_USER_HANDTEST_READY / FAITHFUL_PARTIAL`.

## 19. REV04B Same-Session Board-Tray-Board Reentry Correction (Active)

This section supersedes the REV04A handtest-ready status after the user's real
Unified path proved that an accepted Board -> Tray move prevents the next
same-instance Tray -> Board drag in the still-open Prepare session.

- `Status`: `CONTAINED_ONE_GUARD / SAME-PACKAGE P0 INPUT-INTENT CORRECTION / DEV_ACTIVE`
- `PlayerVisibleDelivery`: `PARTIAL`
- `MilestoneCompletionEvidence`: `NO`
- strongest allowed terminal before user acceptance:
  `CORRECTED_ITEM_INTERACTION_USER_HANDTEST_READY / FAITHFUL_PARTIAL`
- `UserReviewGate`: `YES / ACCEPTED`; the user report authorizes correction.
- Producer/Balance gate: `NOT_APPLICABLE`; no gameplay values or cadence change.

### 19.1 Immutable Player Promise Delta

While one paused Unified Prepare session remains open, an authoritative Item
instance returned from Board to Tray must immediately accept a new Tray drag
back to Board. No Prepare close/reopen, Scene re-entry, extra click, fake card,
or second placement owner may be required.

Click remains the explicit Item-detail intent. A drag/long-press remains the
explicit movement intent. Starting or completing a movement must not create a
detail popup that consumes the next movement gesture.

### 19.2 Read-Only First-Missing-Layer Attribution

The first missing layer is the exact formal Item Presenter's input-intent
lifecycle, not authority membership, Tray publication, Card authored input, or
Mainline Battle orchestration.

1. `SubmitReturnToTray` submits one authoritative `ReturnToTray`, clears the
   active interaction and republishes `authority.Current`.
2. `C1ExactBattleSandboxItemTrayView.Bind` selects every unplaced roster row,
   binds the returned stable instance to one authored Card, applies its
   authoritative footprint, and enables that Card's `CanvasGroup` input.
3. The Presenter nevertheless calls `OpenSelectedFormalItemDetail` from the
   private `BeginInteraction` shared by Board and Tray drags. Therefore the
   successful Board drag opens detail even though the pointer intent was move,
   not click.
4. The formal detail carrier becomes visible, and its existing outside-pointer
   close relay consumes the next pointer-down outside popup content. That next
   pointer-down is the attempted Tray drag, so Card pointer identity,
   long-press routing and candidate creation never begin on the first attempt.
5. Existing authority-only REV04 tests submit `ReturnToTray` followed by
   `PlaceFromTray` directly and therefore bypass this live pointer/detail seam.

The correction is code-only inside the Item Presenter: click selection remains
the sole path that opens detail; accepted Board/Card drag begin must not open
detail. Selection identity may still follow the dragged item, but movement may
not emit detail-open intent. Existing explicit close, reset, lock and unbind
detail lifecycle remains unchanged.

### 19.3 Exact REV04B Write Whitelist And Baselines

Only these existing files may be modified:

| Path | REV04B intake SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` | `D8181FFE88FBEA689DE12CEF13C9B853B540804A595EB8984A63B6F9F965C2FC` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs` | `10A072B6CC9E8686F3E09DDDDB7F851750B6D39F9E53D1AD7039F6D6F16AD322` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemInteractionAuthorizationTests.cs` | `2A23F51E36583D5E1EFBBF76D3408DD4E5F3F69E36C1B089C756637694F4F692` |

Read-only protected seams for this revision:

| Path | REV04B baseline SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs` | `8B987D53F50733445D09673848283A1D24C4D1F64C18E7A9000407418052054E` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs` | `35026ED05B541A185D9FF002DBF6EB300C76F4C2E6F27E7C62DB8A835DD5D740` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` | `5E3E66A11A5D22D4A3B9EC61202B8AE90E11960C2A66FBF490BAEFA157CFC224` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemDetailProjectionAndSelectionTests.cs` | `6BC22A9A89401B31D16FB8F0B23680A18CD8934C1660F6DAA724F0D7BD5B7C16` |
| `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/ExactItemDetail/C1ExactBattleSandboxItemDetailPresenter.cs` | `D985851F663FBCB530C7BF02E1F602B909D8418DD511862B6673A38E897E3453` |

All section-17 authority, shape/catalog, authorization contract, BuildSandbox,
Scene and exact Prefab baselines remain protected. Mainline REV10/host files are
external active-owner state and are read-only, not hash gates for this patch.

No Scene, Prefab, meta, Item authority, formal detail carrier, Mainline,
Battle, Enemy, Reward, Save, VFX, ProjectSettings or BuildSettings write is
authorized. No Unity, Git or worktree operation is authorized.

### 19.4 Required Correction And Evidence

1. Remove detail-open intent from accepted Board/Card drag begin. Preserve
   explicit Card/Board click detail-open behavior and selected-instance truth.
2. Add a focused same-session sequence using the actual Presenter/View/Card
   seams: `PREPARE_ENABLED` -> Board drag -> accepted `ReturnToTray` -> snapshot
   publication -> `FindBoundCard(sameInstance)` -> valid pressed raw cell ->
   Card drag -> one shared candidate -> accepted `PlaceFromTray`.
3. Assert the returned Card is active, uniquely bound, input-enabled, carries
   the same stable instance and authoritative footprint, and does not require
   a second click or a new authorization request.
4. Assert drag begin leaves detail closed, while explicit Card and Board click
   still open the same authoritative detail projection.
5. Preserve Board -> Board, ordinary initial Tray -> Board, four rotations,
   rotate-zone behavior, bounds/collision/cancel restoration,
   `BATTLE_LOCKED` stale-event no-op, detail explicit close, lifecycle reset and
   unbind cleanup.
6. Compile actual current-source `Assembly-CSharp` and
   `Assembly-CSharp-Editor` separately and run both the revised promotion suite
   and complete interaction-authorization suite. No combined assembly proof.

The only runtime acceptance is one normal-Editor user retest in Unified:
without closing Prepare, move the same item Board -> Tray -> Board, then repeat
once after an invalid/canceled drop; verify no duplicate Card/listener, stale
pointer/source-container state, popup-consumed first gesture, or placement
drift. Close Prepare and verify locked input plus clean residue.

### 19.5 Stop Conditions

- If the returned Card is not actually bound/input-enabled after publication,
  stop with the first exact Card/Tray serialized or lifecycle seam; do not add a
  second roster or fake visual move.
- If the failure requires modifying the formal detail carrier, Mainline host,
  Scene or Prefab, return that exact owner boundary before writing it.
- Any authority/shape/catalog, placement algorithm, Battle pause, VFX or formal
  persistence change is forbidden.
- One implementation and at most one objective same-package code/test light fix
  are allowed before `ARCHITECTURE_REASSESS_REQUIRED`.

## 20. REV04C One-Shot Real Pointer Diagnostic (Active)

REV04B handtest readiness is withdrawn. The user's repeated real Unified path
failure overrides the synthetic Presenter/View/Card regression evidence. Static
inspection proves that the initial and returned Tray items share the same
serialized Prepare -> Tray -> Card carriers, but it does not identify the first
runtime raycast or lifecycle difference. This revision gathers that missing
evidence only; it is not another behavioral fix.

- `Status`: `CONTAINED_ONE_GUARD / SAME-PACKAGE DIAGNOSTIC_ONLY / DEV_ACTIVE`
- `PlayerVisibleDelivery`: `NONE`
- `MilestoneCompletionEvidence`: `NO`
- strongest terminal status:
  `REAL_POINTER_DIAGNOSTIC_USER_CAPTURE_READY / BUG_NOT_CORRECTED`
- `UserReviewGate`: `NOT_REQUIRED`; the accepted blocker and user reproduction
  authorize technical evidence collection.
- Unity Technical Director carrier: Editor-only instrumentation inside the
  existing Item Presenter/Tray/Card runtime carrier, compiled out or inert in
  Player/APK. No Scene, Prefab, fallback UI, second state owner or serialization
  carrier is allowed.

### 20.1 Frozen Diagnostic Outcome

During one normal-Editor Unified Play session, record a bounded paired snapshot:

1. the first known-good pointer on an initially present Tray Card;
2. the first pointer attempt on the same stable Item instance after an accepted
   Board -> Tray transaction in the still-open `PREPARE_ENABLED` session.

Emit exactly one concise paired capture, or one terminal reason explaining why
the pair could not be completed. Do not emit per-frame, per-raycast or repeated
pointer spam. The capture must be easy to remove after attribution and must be
inactive outside the exact Editor diagnostic context.

The paired capture must include:

- `EventSystem.RaycastAll` ordered hits, raycaster module, depth, sorting layer,
  sorting order and hit GameObject hierarchy;
- Prepare InputGate `CanvasGroup` alpha/interactable/blocksRaycasts;
- Tray ScrollRect, Viewport, RectMask2D and Content containment/cull facts;
- Card slot/index, stable identity, `activeSelf`, `activeInHierarchy`, sibling
  order and bound source-container state;
- Card CanvasGroup state;
- `ExactInputSurface` active/raycastTarget/CanvasRenderer cull state;
- Card bound identity, current footprint dimensions/cells and raw grabbed-cell
  resolution result for the pointer;
- Presenter interaction mode, product context, session token,
  resetGeneration/session identity and active interaction identity;
- whether Card `OnPointerDown` and `OnBeginDrag` were actually reached.

The diagnostic may observe the accepted `ReturnToTray` result only to tag the
same returned stable identity. It may not change authority, selection, drag,
scroll, event routing, hierarchy, raycast state or command submission.

### 20.2 Exact Write Whitelist And Intake Baselines

Only these existing files may be modified:

| Path | REV04C intake SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` | `BE763FF4006306695D3D78DC3AE815C4BE516FA52C6E2C4AC50F73D7DB390577` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs` | `35026ED05B541A185D9FF002DBF6EB300C76F4C2E6F27E7C62DB8A835DD5D740` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` | `5E3E66A11A5D22D4A3B9EC61202B8AE90E11960C2A66FBF490BAEFA157CFC224` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs` | `0519BABC6B5DEEFB5DA61DD68276768B4E833251CCF72A42D08E547CB85DEA35` |

Read-only real-composition baselines:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxPrepareSurface.prefab` | `95207E6F0123938B9F532AE911575867795EC0A6040AB8776F4E9F487ED1FC32` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab` | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab` | `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` |
| `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab` | `44A897ACECF2F871A7BCD45A3390FFF5ED72EB894FB0A815E751BC6BA4663590` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` | `C6F6EEAB8741F7827C530BF9F1BB2F951CC5619154236733C8E013B228869FE8` |

All section-17/19 authority, shape/catalog, authorization, BattleSandbox Scene,
exact Item Prefab and BuildSandbox protections remain active. Current Mainline
REV10 code remains an external read-only owner and is not a writable dependency.

### 20.3 Forbidden Scope

- No behavior correction, guessed lifecycle patch or changed input threshold.
- No Scene, Prefab, meta, Mainline, Battle, Enemy, Reward, Save, VFX,
  ProjectSettings, BuildSettings, Item authority, shape or catalog write.
- No runtime hierarchy creation, debug panel, fallback UI, event interception,
  synthetic pointer injection or second Item/selection/placement owner.
- No Unity launch, batch, Fresh Play, automated Play or serialization pass.
- No Git or worktree operation.
- The missing Item Detail Prefab mount is a separate Mainline Phase B carrier.
  It may be reported as absent but may not be implemented or treated as the
  current pointer interceptor in this revision.

### 20.4 Verification And Return Contract

Before delivery:

1. compile current-source `Assembly-CSharp` and `Assembly-CSharp-Editor`
   separately using their real assembly boundary;
2. statically prove Editor-only/inert Player behavior, exact host/context gate,
   one-shot pair cardinality, no per-frame logging, no input/event mutation and
   complete required evidence fields;
3. preserve all read-only baselines and leave no temporary outputs;
4. do not claim product correction or runtime PASS.

Return exactly one terminal event to Item Guard:

- `REAL_POINTER_DIAGNOSTIC_USER_CAPTURE_READY / BUG_NOT_CORRECTED`, with changed
  files, compile/static evidence, unique log marker and the shortest user steps;
- `DIAGNOSTIC_CARRIER_BLOCKED`, with the first exact missing observation seam;
- `VERIFIED_WRITE_CONFLICT`, without modifying or restoring the external path.

The user performs one normal-Editor reproduction only:

1. enter Unified realtime battle and open Prepare;
2. press/briefly drag one initially present Tray Card once;
3. move one Board Item into Tray;
4. attempt to pick up that returned same-instance Card once;
5. paste the single paired diagnostic line back to Item Guard.

After capture, Item Guard owns one read-only attribution and may then freeze the
real correction in this same visible task. Until then the package remains
`USER_HANDTEST_FAIL / MILESTONE_FIDELITY_BLOCKED`.

## 21. REV04D Automatic Returned-Card Diagnostic Recovery (Active)

The user's real REV04C capture ended with
`INITIAL_CAPTURE_MISSING_AT_ACCEPTED_RETURN`. That line proves the authoritative
Board -> Tray return was accepted, but it does not attribute the returned Card
input failure. REV04C made a valid raw occupied-cell resolution an unstated
prerequisite for the initial sample and then terminated at the accepted return
before arming the returned-card observation. This section corrects only that
diagnostic lifecycle; it does not authorize another guessed gameplay fix.

- `Status`: `CONTAINED_ONE_GUARD / SAME-PACKAGE DIAGNOSTIC_RECOVERY / DEV_ACTIVE`
- `PlayerVisibleDelivery`: `NONE`
- `MilestoneCompletionEvidence`: `NO`
- strongest terminal status:
  `REV04D_RETURNED_POINTER_CAPTURE_READY / BUG_NOT_CORRECTED`
- `UserReviewGate`: `NOT_REQUIRED`; the user's capture authorizes this bounded
  evidence correction.
- Milestone verdict remains `BLOCKED`: the original same-session
  Board -> Tray -> Board promise is unchanged and still lacks real-path proof.
- Unity Technical Director carrier remains the existing Editor-only Item
  Presenter/Tray/Card diagnostic seam. No serialized carrier or Unity authoring
  pass is required or authorized.

### 21.1 Confirmed Diagnostic First Missing Layer

`C1ExactBattleSandboxItemCardView.OnPointerDown` always notifies the Editor
observer after attempting raw-cell resolution. The current Presenter observer,
however, returns without recording anything unless
`TryGetPressedRawGrabbedCell` succeeds. Consequently, these materially different
states collapse into the same missing sample:

1. the Card pointer callback was never reached;
2. the callback was reached but the pointer did not resolve to an occupied raw
   footprint cell;
3. the user did not perform an initial Tray pointer before returning the Board
   item.

`EditorObserveAcceptedReturn` then emits a terminal reason immediately when the
initial sample is absent. It never records the accepted returned identity and
never arms the existing returned-pointer observation. The diagnostic therefore
cannot inspect the exact defect it was created to distinguish. This is a
diagnostic-carrier defect, not evidence that the product bug is in Card input,
Tray composition, Presenter state or authority.

### 21.2 Required Recovery

1. An initial Tray sample is optional. If one is captured, automatically record
   the first real Tray Card pointer/raycast attempt without requiring raw-cell
   success. Store `cardCallbackReached` and `rawCellResolved` as separate facts;
   never use one as proof of the other.
2. An accepted `ReturnToTray` must always record the returned stable identity
   and arm returned-card capture after validating the unchanged Editor host,
   product context, `PREPARE_ENABLED` session token and reset generation. A
   missing initial sample must not emit or latch a terminal result.
3. The first user pointer-down inside the returned Card rect must collect the
   complete ordered `EventSystem.RaycastAll` chain and the section-20 Card,
   Tray, InputGate, mask, session and source-container facts even when the Card
   callback never fires or raw-cell resolution fails.
4. Observe `OnPointerDown` and `OnBeginDrag` independently. Finalize one output
   after the attempt has either reached begin-drag or ended/boundedly settled,
   so the line truthfully reports both callback states. Do not intercept,
   replay or synthesize the pointer event.
5. Emit exactly one line with the existing marker family. It may contain an
   optional initial sample plus the returned sample, or
   `initial={missing;reason=OPTIONAL_NOT_OBSERVED}` plus the complete returned
   sample. Emit a terminal reason only if the accepted return cannot bind/arm a
   unique returned Card or the returned attempt itself cannot be observed.
6. Preserve the capture across the accepted authority publication/rebind and
   clear it only on one emitted result, host/context/session/reset change,
   lock/unbind, or diagnostic disable. No per-frame logging or gameplay state
   mutation is allowed.

### 21.3 Exact Write Whitelist And Intake Baselines

Only these existing files may be modified:

| Path | REV04D intake SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` | `D281D449089F332D6135D1BB12DB6B467DABF3E4C885BEB693B9001C6BB5ABC1` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs` | `313AA3BF7328044C7F399471F68991EFAC63FCC07978AAB6878153DAE7CA0BD5` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` | `CDDDA7DA992E36BC3E8EE85AEFC0E99D67242592E84CFB4A9ADDB33F48B1B9CB` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs` | `A447FAFFE91983D8C5DFE3880FAD00844F069C8DD979831FB5DF1110E993243E` |

All section-20 real-composition baselines and all section-17/19 authority,
shape/catalog, authorization, BattleSandbox Scene, exact Item Prefab and
BuildSandbox protections remain active. Mainline code remains an external
read-only owner.

No Scene, Prefab, meta, Mainline, Battle, Enemy, Reward, Save, VFX, Item
authority, shape/catalog, ProjectSettings or BuildSettings write is authorized.
No Unity launch, batch, Fresh Play, serialization, Git or worktree operation is
authorized.

### 21.4 Minimum Evidence And Return Contract

Before delivery:

1. compile current-source `Assembly-CSharp` and `Assembly-CSharp-Editor`
   separately using their real assembly boundary;
2. focused deterministic/static proof must cover: initial callback reached with
   raw-cell failure; initial sample absent at accepted return; accepted return
   still arms one unique returned identity; returned-only full raycast capture;
   independent pointer-down/begin-drag flags; one-line cardinality; cleanup on
   context/session/reset/lock/unbind; and zero diagnostic gameplay mutation;
3. preserve all protected baselines and remove temporary outputs;
4. do not claim the original product bug is fixed or handtest-ready.

Return exactly one terminal event to Item Guard:

- `REV04D_RETURNED_POINTER_CAPTURE_READY / BUG_NOT_CORRECTED`, with changed
  files, compile/static evidence, final unique marker and the shortest user
  action;
- `DIAGNOSTIC_CARRIER_BLOCKED`, with the first exact missing observation seam;
- `VERIFIED_WRITE_CONFLICT`, without modifying or restoring the external path.

The next user capture must require only:

1. enter Unified realtime battle and open Prepare;
2. move one Board Item into Tray;
3. attempt to press/drag that returned Item once;
4. paste the single diagnostic line.

No initially present Tray Card gesture is required. After this capture, Item
Guard performs one read-only attribution before authorizing any product fix.

## 22. REV04E Real Tray Scroll-Range Arbitration Correction (Active)

The user's REV04D real-path capture is sufficient to attribute the first
product failure. The returned Card is uniquely bound, active, authorized,
raycast-first, raw-cell-resolved, and receives both `OnPointerDown` and
`OnBeginDrag`. The failure is after callback entry and before Presenter item
interaction creation.

- `Status`: `CONTAINED_ONE_GUARD / SAME-PACKAGE P0 INPUT-ROUTING CORRECTION / DEV_ACTIVE`
- `PlayerVisibleDelivery`: `PARTIAL`
- `MilestoneCompletionEvidence`: `NO`
- strongest terminal status:
  `CORRECTED_ITEM_INTERACTION_USER_HANDTEST_READY / FAITHFUL_PARTIAL`
- `UserReviewGate`: `YES / ACCEPTED`; the user's real-path failure authorizes
  this correction.
- Producer/Balance gate: `NOT_APPLICABLE`; no gameplay values, probability or
  cadence change.
- Unity Technical Director carrier remains the existing formal Item Card/View
  input adapter. No Scene, Prefab or new runtime carrier is required.

### 22.1 Sufficient Real Diagnostic Attribution

The first missing layer is
`C1ExactBattleSandboxItemCardView` scroll-versus-item drag arbitration.

1. The exact REV04D capture proves the returned I001 Card is the first
   `GraphicRaycaster` hit, input-enabled, inside the active Viewport/Content,
   raw cell `0:0` resolves, and both pointer-down and begin-drag callbacks run.
2. The exact serialized Tray Prefab has an authored vertical `ScrollRect`, but
   both `ItemTrayViewport` and `ItemTrayContent` are `700 x 700`. There is no
   real vertical scroll range for the current formal roster.
3. `CardView.CanScroll()` currently returns true from component configuration
   alone (`active + vertical + content`) and never checks whether Content
   actually exceeds Viewport.
4. `ResolvePendingRoute` therefore classifies an ordinary upward/vertical
   Board-directed gesture as `DragRoute.Scrolling`. Since the zero-range
   ScrollRect cannot move, the gesture is swallowed and `BeginItemDrag` /
   `Presenter.BeginCardDrag` never establishes `activeItemInstanceId`.
5. `cardWithinViewport=0` is not the blocking condition. It is emitted only by
   the Editor diagnostic and is not consumed by Card routing, Presenter
   candidate creation, authority validation or commit.

This explains the real line's otherwise contradictory facts:
`OnBeginDrag=1`, `rawCellResolved=1`, `PREPARE_ENABLED`, but
`activeInteraction` remains empty and the Card never lifts.

### 22.2 Required Correction

1. Replace configuration-only scrollability with one deterministic
   `HasScrollableVerticalRange` decision derived from the authored ScrollRect's
   actual Viewport and Content geometry. Use Viewport-relative Content bounds
   (including active transform scale) and a small fixed layout epsilon; do not
   infer from roster count or item identity.
2. When no positive vertical scroll range exists, every valid occupied-cell
   drag must route directly to Item drag regardless of initial drag direction.
   Do not forward initialize/begin/drag/end events to a zero-range ScrollRect.
3. When Content genuinely exceeds Viewport, preserve the accepted
   BattleSandbox gesture contract: vertical swipe scrolls; long-press,
   horizontal item intent, or leaving the Viewport transitions to Item drag;
   scroll momentum is stopped on that transition.
4. The same generic rule applies to initially present and returned Cards. No
   returned-item, I001, slot, roster-count or scene-specific branch is allowed.
5. Preserve the exact stable instance, raw grabbed cell, rotation, footprint,
   preview/commit candidate, bounds/collision result and authority transaction
   across Board -> Tray -> Board in one unchanged `PREPARE_ENABLED` session.
6. Remove the temporary REV04C/REV04D one-shot diagnostic carrier and marker
   after adding focused regression evidence. No persistent Editor logging,
   polling or diagnostic state may remain in the product path.

### 22.3 Exact Write Whitelist And Intake Baselines

Only these existing files may be modified:

| Path | REV04E intake SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` | `5933D448E05637008084D6082DD4CD5973C70727A9CAC36060D540B9EEE17BD1` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs` | `E16D0D01031ADD5F17C309E7D15CE164FF613AF87E5F20D7F86897AA336FE05B` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` | `D679A2A853A803074A0C53C06B9DAC7103A8303D3476C8EC139BDF815A8669AE` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs` | `1C08DBC7E424E6D8D7D902FBE9C0F3FC5D2543F9DE39A3CD7F4DAE7692D009CF` |

Read-only carrier baselines:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxPrepareSurface.prefab` | `95207E6F0123938B9F532AE911575867795EC0A6040AB8776F4E9F487ED1FC32` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab` | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab` | `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` |
| `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab` | `44A897ACECF2F871A7BCD45A3390FFF5ED72EB894FB0A815E751BC6BA4663590` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` | `C6F6EEAB8741F7827C530BF9F1BB2F951CC5619154236733C8E013B228869FE8` |

All section-17/19 authority, shape/catalog, authorization, BuildSandbox,
BattleSandbox Scene and exact Item Prefab protections remain active. Mainline
code remains an external read-only owner.

No Scene, Prefab, meta, Mainline, Battle, Enemy, Reward, Save, VFX, Item
authority, shape/catalog, ProjectSettings or BuildSettings write is authorized.
No Unity launch, batch, Fresh Play, serialization, Git or worktree operation is
authorized.

### 22.4 Minimum Evidence And User Retest

Before delivery:

1. compile current-source `Assembly-CSharp` and `Assembly-CSharp-Editor`
   separately using their real assembly boundary;
2. focused deterministic tests must prove equal/smaller Content does not route
   to ScrollRect, actual overflow does, transform-aware bounds remain stable,
   the accepted vertical/horizontal/long-press/Viewport-exit matrix is
   preserved, and zero-range Board -> Tray -> Board creates one shared
   candidate and accepted commit;
3. preserve Board -> Board, ordinary Tray -> Board, four rotations, rotate
   hot-zone, invalid/collision/cancel restore, detail click, `BATTLE_LOCKED`
   cleanup and ScrollRect behavior when overflow is real;
4. statically prove all REV04C/REV04D marker, capture and one-shot logging code
   is removed, all protected baselines remain exact, and temporary outputs are
   cleaned;
5. no automated Unity run or runtime PASS claim.

The user performs one normal-Editor retest:

1. in one unchanged Unified Prepare session, move one Board Item to Tray and
   immediately drag that same returned Card back to Board;
2. repeat once after an invalid/canceled drop;
3. verify an actual overflowing Tray still scrolls vertically while item
   long-press/horizontal/Viewport-exit dragging remains available;
4. close Prepare and verify Item movement is locked with no stale preview,
   duplicate Card/listener, detail-popup theft or placement drift.

### 22.5 Stop Conditions

- If the actual ScrollRect has a positive runtime overflow range despite the
  exact equal serialized geometry, stop with the first runtime geometry owner;
  do not hardcode current dimensions or roster count.
- If the correction requires a Prefab/Scene/Mainline or authority change,
  return that exact dependency before writing it.
- Any item-specific branch, duplicate placement/input owner, changed gesture
  thresholds, changed shape/rotation truth or runtime fallback UI is forbidden.
- One bounded implementation only. A repeated real-path failure returns
  `ARCHITECTURE_REASSESS_REQUIRED`; do not add another diagnostic loop.

## 23. REV04F Formal Tray Layout Truth And Exact Sandbox Grid Parity (Active)

The user's normal-Editor evidence accepts the repaired Board -> Tray and
Tray -> Board directions, but rejects two still-missing player-promise rows:
items cannot be moved within the Tray, and the visible formal Tray is not the
accepted BattleSandbox grid/slot system. REV04E is preserved as the scroll
arbitration prerequisite; it is not evidence for these missing rows.

- `Status`: `COMPLEX_GUARDED_ONCE / SAME-PACKAGE P0 TRAY TRUTH AND
  PRESENTATION CORRECTION / DEV_ACTIVE`
- `PlayerVisibleDelivery`: `PARTIAL`
- `MilestoneCompletionEvidence`: `NO`
- strongest terminal status:
  `CORRECTED_TRAY_PARITY_USER_HANDTEST_READY / FAITHFUL_PARTIAL`
- `UserReviewGate`: `YES / ACCEPTED`; the user explicitly requires the
  accepted BattleSandbox Tray appearance, deterministic positions and
  Tray -> Tray movement.
- Producer/Balance gate: `NOT_APPLICABLE`; no values, probability, cadence,
  capacity or reward rule changes.
- Milestone verdict remains `BLOCKED` until one real Prepare session proves
  Tray -> Tray plus both cross-container directions.

### 23.1 Technical Director Decision And First Missing Layer

Plain-language decision: the reusable Tray body already contains much of the
accepted artwork, but the formal game has neither a referee-owned Tray position
nor a command for moving an Item between its cells. A fixed three-card display
cannot satisfy the accepted grid interaction.

Concrete carrier decision:

| Object | Carrier owns | Carrier must not own | Runtime feeder |
|---|---|---|---|
| Exact Tray Prefab | accepted frame, 5-column/13-row authored cell refs, viewport, mask, scrolling, card layer and local feedback refs | roster membership, Item identity, legal occupancy, persistence | exact Item Presenter from formal Item Authority snapshot |
| Shared Item Card | artwork, authored footprint/input surface, local selection/drag/rotation feedback | Tray position, Board position, shape truth | exact Item Presenter |
| Formal Item Authority | immutable Tray position/rotation/occupied-cell truth, legality, deterministic reset/return and command idempotence | authored RectTransform geometry or input routing | formal Item commands |
| Presenter/TrayView | pointer-to-authored-cell translation, immutable candidate preview, snapshot publication | a second remembered layout, fixed display order or inferred Item shape | Authority snapshot and results |

Read-only comparison confirms the first missing layer is the formal Item state
contract, followed by its serialized presentation carrier:

1. `C1FormalItemArrangementCommandKind` exposes only `PlaceFromTray`,
   `MoveOnBoard` and `ReturnToTray`. `C1FormalItemSessionSnapshot` exposes no
   Tray anchor, rotation, occupied cells, ordering or immutable Tray layout.
2. `C1ExactBattleSandboxItemTrayView.Bind` selects unplaced rows, sorts only by
   `itemInstanceId`, and binds them to a fixed three-card array by index. It
   never consumes a Tray placement and never positions a Card on an authored
   slot.
3. `C1ExactBattleSandboxItemArrangementPresenter.EndInteraction` can commit a
   Board candidate or return a Board Item to Tray. It has no Tray target
   candidate, preview or `MoveWithinTray` submission path.
4. The exact formal Tray Prefab preserves the source root/viewport/content and
   the 40 Scene-authored `TrayGridSlot_01..40`, but the accepted current
   BattleSandbox runtime operates a deterministic 5 x 13 / 65-cell shape-aware
   Tray and extends cells 41..65. The formal carrier has not promoted those
   final cells as authored serialized refs.
5. The accepted Sandbox keeps Tray placement in its dev controller/grid. The
   formal route may reuse its deterministic behavior and read-only shape facts,
   but may not depend on `BuildSandbox`, copy its runtime graph, or make the
   Presenter a second state owner.

Chosen Unity carrier: existing Unified Scene and existing exact Tray/Card
Prefabs; one additive formal Authority contract plus the existing
Presenter/Views. No new Scene, runtime-created final UI, BuildSandbox dependency
or second Item owner is permitted. The Editor tool may author/update and
validate the Tray Prefab only; Player/APK has no Editor dependency.

### 23.2 Required Authority And Transaction Semantics

1. Add one immutable formal Tray-layout snapshot owned by
   `C1FormalItemSessionAndArrangementAuthority`. It must identify each stable
   `itemInstanceId`, its deterministic Tray anchor, formal rotation, ordered
   occupied cells, active Tray membership versus remembered return position,
   and canonical signature. It must be part of the session canonical truth and
   provide exact instance lookup.
2. Add exactly one `MoveWithinTray` arrangement command/intent. Existing enum
   numeric values and existing command semantics remain stable. The command is
   accepted only for an unplaced current-session Item and one exact legal Tray
   candidate; stale token/generation/signature, duplicate identity, bounds or
   collision failure rejects without partial mutation.
3. The formal Tray is exactly 5 columns x 13 rows / 65 logical cells. Placement
   uses authoritative catalog shape cells and the existing reversible
   presentation-quarter-turn <-> formal-degree convention. Capacity/roster,
   Board shape and Build/lighting truth are unchanged.
4. Reset/new session creates one deterministic no-overlap Tray layout for all
   unplaced roster rows. Entitlement of I002 inserts it once at the first
   deterministic legal position. Unknown or geometrically impossible input
   fails closed; no Item is hidden or hard-packed into overlap.
5. `PlaceFromTray` atomically deactivates that instance's Tray occupancy while
   retaining its remembered legal return position. `ReturnToTray` atomically
   removes the Board placement and restores the remembered candidate when
   legal, otherwise uses the first deterministic legal candidate. If no legal
   candidate exists, the entire operation rejects and the Board state remains
   exact.
6. `MoveWithinTray` changes only the moved instance. Unrelated Tray anchors,
   Board placements, session identity, roster identity, lighting and Battle
   input remain unchanged. Duplicate command replay is idempotent.
7. No fixed visual ordering, Card sibling index, hidden UI list or Presenter
   dictionary may become Tray truth. The Authority snapshot is the only source
   for publication, reset and re-entry.

### 23.3 Required Exact Tray Carrier And Interaction

1. Update only `C1ExactBattleSandboxItemTray.prefab` through the existing
   Editor-only promotion surface. Preserve the accepted root, background,
   800 x 800 carrier, 700 x 700 Viewport, RectMask2D, ScrollRect, Content,
   ItemCardLayer, slot Image/Outline styling, fonts, order and category
   hierarchy. Do not redraw, rescale or approximate the Tray.
2. Promote cells 41..65 as authored Prefab children by cloning the exact final
   source slot component/style pattern in deterministic row-major order. Names
   and serialized refs must be stable. Set Content height from the accepted
   5-column GridLayoutGroup geometry so 13 complete rows scroll and settle
   without runtime slot creation.
3. `TrayView` must serialize and validate all 65 cell RectTransform/Image refs.
   It binds each unplaced Card to the Authority Tray snapshot, positions the
   Card from the exact authored occupied-cell centers, and keeps artwork,
   footprint and input surface aligned. It may display preview/invalid state on
   the existing cell Images but must restore authored colors after completion.
4. Pointer-to-Tray-cell resolution must use the authored slot refs and the
   exact raw grabbed occupied cell. Preview, bounds/collision result and commit
   consume one immutable Tray interaction candidate. Transparent footprint
   gaps remain non-grabbable; no top-left fallback or identity branch.
5. Releasing a Tray Card over a legal Tray target submits exactly one
   `MoveWithinTray`; illegal/canceled drops restore the exact prior snapshot.
   Releasing over Board preserves the accepted Tray -> Board route. Board ->
   Tray preserves the accepted return transaction and immediately publishes
   the returned Authority Tray placement.
6. Preserve REV04E arbitration: zero-range content routes valid gestures to
   Item drag; real overflow retains vertical scroll and the accepted
   long-press/horizontal/Viewport-exit transition with momentum stop.
7. Preserve click/detail, four rotations, rotate hot zone, Board -> Board,
   selection, masks/raycasts, `BATTLE_LOCKED` cleanup and session reset. No
   category rule, Arrange command, VFX binding, Mainline host behavior or
   gameplay value is added here.

### 23.4 Exact Write Whitelist And Intake Baselines

Only these existing paths may be modified:

| Path | REV04F intake SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs` | `935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` | `BE763FF4006306695D3D78DC3AE815C4BE516FA52C6E2C4AC50F73D7DB390577` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs` | `F20EC9F72F15F6303C05CE45125F0DF03E6164DFC04C57A380FC685764A36074` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` | `4607F7CB7446DCDA9585CF71FFB29F99E3C3A18A7C07E0309D80142A006C19C2` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionEditor.cs` | `7549C6949BB652CE7862EAE68CEF1915A9AA718FC3C8290B7269B5656E8E330D` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs` | `25D773F4A42D59FC15C018C6CCE527719475D4C99AAF96A7629420D6BFDE5E96` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemSessionAndArrangementAuthorityTests.cs` | `4FB1B0E478E258528ABF0546B5380D1D33DF3BCE5D8169B2298DBA04BF607FAB` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemInteractionAuthorizationTests.cs` | `09395D1049E37DFF8EBD3D7C41FA863111C4E40A8DFD9AA0D268148249FB47A5` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab` | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |

The existing Tray Prefab `.meta` is read-only and must keep SHA-256
`BF0E9EAEFA6D5D3123AE095F0CD55698DA437644707F7F51EEE5CE19CA6C52A4`.

Read-only protected baselines:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab` | `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab` | `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` | `C6F6EEAB8741F7827C530BF9F1BB2F951CC5619154236733C8E013B228869FE8` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `06AF79CB89EB4DCA0A2313E81AF81EFA89A35B90AE6FE5E6B0BBDD3C90F62921` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `364727A582EE3F2D499C109D98939318B4D26AB9D2F1F571CB70E62E605913DC` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs` | `DA8C1B628A02565908E7BC7A0EF9E56DC2E00BEC25AF549DF9382769372665BC` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs` | `A4075E2279BB937858BE97A5E41365CE0EFF58F12277F0D19B93B7EFAC29B590` |

No other Item, BoardView, Card/Board Prefab, PrepareSurface, Unified/Mainline,
Scene, Battle, Enemy, Reward, Save, VFX, catalog/shape source,
ProjectSettings, BuildSettings, Package, report or meta write is authorized.
No Git or worktree operation is authorized.

### 23.5 Verification And One Serialization Pass

1. Compile current-source `Assembly-CSharp` and `Assembly-CSharp-Editor`
   separately against their real response-file boundary.
2. Focused Authority tests must prove deterministic 5 x 13 initial layout,
   authoritative shape/rotation occupancy, valid `MoveWithinTray`, collision
   and bounds rejection, exact unchanged signature on reject, duplicate
   idempotence, unrelated-anchor stability, entitlement insertion, remembered
   return, occupied-anchor fallback, full rejection, reset equality and stale
   token/generation/signature rejection.
3. Focused Presenter/View tests must prove every grabbed occupied cell for
   I010/I016 corner3 plus line2/2x2 across four rotations; one shared Tray
   preview/commit candidate; Tray -> Tray, Tray -> Board, Board -> Tray and
   invalid/cancel rollback; exact Card position against authored cell centers;
   65 unique serialized slot refs; real overflow scroll; REV04E zero-range
   arbitration; detail/rotate/lock cleanup regressions.
4. One bounded Unity Prefab authoring/validation pass is allowed only after an
   atomic clean serialization-owner check. It may update only the exact Tray
   Prefab, must not save a Scene, and must prove: 65/65 cells, 5 columns,
   13 rows, source slot style parity, content height/ScrollRect/Mask validity,
   all required refs, no missing script, no runtime fallback hierarchy and no
   BuildSandbox dependency.
5. No automated Play/Fresh Play or runtime PASS claim. The user owns one
   normal-Editor real-path handtest.

### 23.6 User Handtest And Close Path

The shortest mandatory real-path test is one unchanged Unified Prepare
session:

1. move an initially unplaced Item from one legal Tray location to another;
2. repeat with one invalid/canceled Tray target and confirm exact restoration;
3. move that same instance Tray -> Board -> Tray -> a second legal Tray cell;
4. verify its identity, shape, rotation and deterministic position remain
   exact, unrelated Tray Items do not jump, and the visible 5 x 13 grid,
   background, dimensions, mask and scrolling match the accepted Sandbox;
5. close Prepare and confirm interaction locks with no stale preview, duplicate
   Card/listener or placement drift.

On automated evidence PASS, return exactly:

- `CORRECTED_TRAY_PARITY_USER_HANDTEST_READY / FAITHFUL_PARTIAL`, with changed
  files, Authority contract, Prefab validation, compile/tests, protections and
  the shortest handtest;
- `ARCHITECTURE_REASSESS_REQUIRED`, if formal Tray truth cannot be added without
  a second owner or broad ItemSystem migration;
- `VERIFIED_WRITE_CONFLICT` or `EXTERNAL_SERIALIZATION_OWNER`, without
  modifying/restoring the external path.

Only user real-path PASS may close this Item capability row. It does not prove
the full C1 milestone and does not open VFX P2 while REV04F is active.

### 23.7 Stop Conditions

- Any attempt to keep Tray order only in Presenter/View/Card siblings, infer
  legality from visible gaps, or add a second runtime roster/layout owner must
  stop.
- Any dependence on `BuildSandbox` runtime classes, copied controller graph,
  runtime creation of final slots/cards, item-specific branch, changed
  capacity, shape/catalog, Build/lighting, Board authority or formal Save must
  stop.
- If the exact Prefab cannot carry 65 authored cells without Scene/Mainline or
  Card/Board Prefab writes, return the exact carrier blocker before broadening.
- One bounded implementation plus at most one objective same-package light fix.
  A repeated user real-path failure returns `ARCHITECTURE_REASSESS_REQUIRED`;
  do not add another diagnostic cycle.

## 24. REV04F1 Superseded Scroll Assertion Compatibility Recovery (Active)

The single section-23 authoring pass reached the newly authored 5 x 13 / 65
cell Tray geometry, then failed only because an older REV04E live-Prefab
assertion still required that exact Tray to have no vertical scroll range. The
authoring tool restored the original Tray Prefab byte-for-byte and cleaned its
owned process/locks. Authority, Presenter and Tray-layout implementation did
not fail this pass.

- `Status`: `CONTAINED_ONE_GUARD / SAME-PACKAGE OBJECTIVE TEST-CARRIER LIGHT
  FIX / DEV_ACTIVE`
- `PlayerVisibleDelivery`: `PARTIAL`
- `MilestoneCompletionEvidence`: `NO`
- strongest terminal status remains
  `CORRECTED_TRAY_PARITY_USER_HANDTEST_READY / FAITHFUL_PARTIAL`
- `UserReviewGate`: `NOT_REQUIRED`; this changes no product semantics and only
  reconciles a superseded test expectation with the accepted section-23
  carrier.
- Unity Technical Director carrier is unchanged: the existing exact Tray
  Prefab and one Editor-only authoring/validation method.

### 24.1 Exact Failure And Required Test Correction

`C1ExactBattleSandboxItemPromotionTests.RunAllOrThrow` currently evaluates the
returned Card against the live exact Tray and unconditionally requires:

`REV04E_EXACT_EQUAL_TRAY_MUST_HAVE_NO_SCROLL_RANGE`.

That expectation was valid only while the formal Tray Content and Viewport
were both 700 x 700. Section 23 intentionally authors 13 rows into the same
700 x 700 Viewport, so the final exact Tray must have positive vertical
overflow. Keeping the old assertion would reject the required product state.

Required correction:

1. Keep the REV04E synthetic equal-height, smaller-content and epsilon matrix
   unchanged: those controlled geometries must still report no scroll range.
2. Replace only the obsolete live exact-Prefab assertion with explicit
   section-23 post-authoring evidence: the 65-cell/13-row Tray has positive
   vertical scroll range, while the returned Card remains uniquely bound,
   active, input-enabled and able to enter Item drag through the preserved
   overflow gesture contract.
3. Do not relax `HasScrollableVerticalRange`, alter thresholds, manufacture
   overflow in tests, skip the full promotion suite, or make the assertion pass
   from slot count alone. The live geometry and serialized 65-cell refs must
   both prove the expected state.
4. No Authority, Presenter, TrayView, CardView, authoring algorithm or
   interaction behavior change is authorized by this recovery.

### 24.2 Exact Recovery Whitelist And Baselines

Only these paths may change in REV04F1:

| Path | REV04F1 intake SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs` | `1770C640DEE323F6063BEBBEB5F622E78E53A74B9AF60378BBF523101D388C55` |
| `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab` | `AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1` |

The Prefab may change only through the already-implemented section-23 Editor
authoring method during the single replacement validation pass. Its `.meta`
remains read-only at
`BF0E9EAEFA6D5D3123AE095F0CD55698DA437644707F7F51EEE5CE19CA6C52A4`.

Preserve the current section-23 implementation exactly:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs` | `2A1CA12EF83E1BCC1CAA7FA2CD03C3F16AD50E3E5959AEBAE834298D9A80718E` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` | `99AE7C8F54DF002F9F45814AA251B1140B399BA9474936BE11F24AF63B91D043` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs` | `548C19747503F5B458D8BC99FB7E64C06E33C943E8784B1458943D0FAF9CB764` |
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` | `4607F7CB7446DCDA9585CF71FFB29F99E3C3A18A7C07E0309D80142A006C19C2` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionEditor.cs` | `8B4EEFBF9A4EBF86B4F6D57108F0DB284D2B823FDDA945FAC8FFCDA0F0A674A9` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemSessionAndArrangementAuthorityTests.cs` | `BC6CF38F4C1321F0943E2A6F20094863D039C917433681AD508C5C4BA2BF6082` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemInteractionAuthorizationTests.cs` | `09395D1049E37DFF8EBD3D7C41FA863111C4E40A8DFD9AA0D268148249FB47A5` |

All section-23 protected Board/Card Prefabs, Scenes, BuildSandbox sources,
Mainline, Battle, Enemy, Reward, Save, VFX, catalog/shape, settings and meta
baselines remain exact and read-only. No Git or worktree operation is
authorized.

### 24.3 Replacement Authoring And Close Gate

1. Apply the bounded test correction and compile current-source
   `Assembly-CSharp` and `Assembly-CSharp-Editor` separately.
2. Re-run the focused no-Unity Authority and InteractionAuthorization suites;
   their current 299 and 424 assertion evidence may be retained only if the
   preserved hashes above remain exact.
3. After an atomic clean serialization-owner check, authorize exactly one
   replacement invocation of the existing
   `ExecuteRev04FTrayPrefabFromCommandLine` method. This replacement is for the
   objective test-carrier failure and does not create another product retry.
4. The pass must author and retain only the exact Tray Prefab, then complete
   the full focused promotion suite and Prefab validation: 65 unique cells,
   5 columns, 13 rows, positive live overflow, synthetic no-overflow matrix,
   exact source slot style, Content/Viewport/Mask/ScrollRect refs, Card
   position/candidate parity, no missing script, no runtime final hierarchy,
   no BuildSandbox dependency and no Scene save.
5. Clean every owned Unity/ShaderCompiler/bee child and lock. No Play or Fresh
   Play is authorized.

On PASS, return
`CORRECTED_TRAY_PARITY_USER_HANDTEST_READY / FAITHFUL_PARTIAL` with the final
test and Prefab hashes and the section-23 user handtest. Do not claim the full
milestone.

If the replacement pass fails for any objective reason, do not run Unity
again. Restore/retain compile health, preserve the automatic Prefab rollback
behavior where applicable, and return the first exact blocker for Guard
reassessment.
