# V0.4-C1FormalItemSessionAndArrangementAuthority01

## REV02 Guard Revision

The first two authorized Prefab authoring runs proved a Unity serialization
carrier defect before any Prefab could be retained: the three attachable
`MonoBehaviour` classes were grouped in
`C1FormalItemArrangementViews.cs`, so Unity could not create a matching
`MonoScript` asset for any of them and serialized `m_Script: {fileID: 0}`.

REV02 changes only the new-file whitelist and final serialization allowance:

- remove the grouped `C1FormalItemArrangementViews.cs/.meta` paths;
- replace them with one same-named `.cs/.meta` pair for each attachable Card,
  Board, and Tray view;
- allow one final owned Unity Prefab authoring/validation run after the split
  sources compile in the complete current Unity source graph.

State ownership, public behavior, the three-Prefab design, protected baselines,
Scene delta zero, and all other stop conditions remain unchanged. The retained
grouped source/meta are package-owned new files and may be deleted only as part
of this exact replacement.

## 1. Outcome

Provide the Item-owned formal runtime prerequisite for the accepted
`CAMPAIGN_NORMAL_LV1` loop:

`1-1 with I031 + I001@white -> fixed first-clear I002@white -> real player arrangement -> immutable Item input for 1-2`.

The package must own the in-memory formal Item session, entitlement roster,
placement transactions, real `ItemSystemSnapshot.v2` geometry/lighting result,
and an immutable Battle-readable Item snapshot. It must also create replaceable
Board, Tray, and Card prefabs for later mounting into the existing Unified
Battle shell slots. It does not mount them into a Scene in this package.

## 2. Product Context

- Product context: `CAMPAIGN_NORMAL_LV1`
- Chapter: `bone_aspect_chapter_1`
- Initial formal Item state: owned `SPECIAL_I031` plus fixed
  `I001@white`; both placed in a valid direct-light arrangement.
- First-clear addition: exactly the released `I002@white` instance from
  `C1FixedFirstClearItemInstanceCarrier`.
- Teaching rule remains: an ordinary Item acts only when the authoritative
  Item snapshot says it is lit.
- Accepted Item values remain unchanged: I001 `82 / 2 sec`, I002
  `28 / 2 sec`. This package does not recalculate or copy those values.

## 3. Task Class

`COMPLEX_GUARDED_ONCE / ITEM FORMAL STATE + PRESENTATION CARRIERS`

Reason: this package introduces the formal Item session state owner and an
immutable cross-system output contract, while keeping Battle application,
Reward production, Mainline flow, Scene composition, and persistence outside
the package.

## 3A. User Review Gate

- Required: `YES`
- Result: `USER_ACCEPTED_SCOPE`
- Accepted outcome: the existing P0 route
  `1-1 -> guaranteed I002@white -> player rebuilds -> 1-2 visibly changes`.
- No unresolved product choice remains. API shape, file scope, prefab carrier,
  and QA method are technical decisions.

## 4. Owners And Technical Director Decision

- Primary Guard: fixed Item Guard
- Development Owner: visible Item task
  `019fc074-2a68-72d0-9609-c1294a731ab7`
- State owner: new Item-owned `C1FormalItemSessionAuthority`; it is the only
  owner of the memory-only C1 formal availability and arrangement state.
- Geometry/lighting calculator: existing read-only
  `DefaultItemSystemSnapshotProvider` and `ItemSystemSnapshot.v2`.
- Reward owner: existing Reward contracts and fixed reward definition remain
  read-only. Item only validates and consumes the released immutable result.
- Battle owner: Battle later translates the immutable Item battle input and
  applies timing/damage. Item never calls Battle or owns damage application.
- Presentation owner: three new Item prefabs plus Item presenter/views. Prefabs
  render and route intent only; they do not own roster, placement, or lighting.
- Scene owner: existing Unified Battle shell and its authored `BoardArea` and
  `ItemTrayArea` slots. Package serialization impact on Scene is exactly zero.
- Reuse decision: use the existing Unified Battle scene/shell and existing
  Item system rules. Do not copy the BattleSandbox installer/controller graph,
  CoreLoopLab dev roster, or V02 Inventory.
- Mobile/APK: runtime types must be regular player-safe code, with bounded
  5x5 Board and at most three C1 Item cards, no Editor-only runtime dependency,
  no `Resources.FindObjectsOfTypeAll`, no per-frame scene search, no fallback
  hierarchy creation, and no unbounded allocation loop.
- User decision point: `NONE`.

## 5. Allowed Writes

Only the following new implementation files may be created or modified:

```text
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemArrangementPresenter.cs
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemArrangementPresenter.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemCardView.cs
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemCardView.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemBoardView.cs
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemBoardView.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemTrayView.cs
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemTrayView.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemSessionAndArrangementAuthorityTests.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemSessionAndArrangementAuthorityTests.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemArrangementPrefabAuthoring.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemArrangementPrefabAuthoring.cs.meta
Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemBoard.prefab
Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemBoard.prefab.meta
Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemTray.prefab
Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemTray.prefab.meta
Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemCard.prefab
Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemCard.prefab.meta
```

All twenty REV02 targets were absent at the original task start. The superseded
grouped View source/meta were created by this package and are the only paths
REV02 permits deleting. This Task Contract is Guard-owned and must not be edited
by the development task.

## 6. Forbidden Writes

Do not modify any existing file. In particular, do not modify:

- `ItemSystemSnapshot.cs`, Item lighting/shape/catalog/Build/Core rules, the
  released C1 Item baseline, or Package 1 carrier;
- Reward/Drop/Claim/Save/Inventory/Progression or the fixed reward definition;
- Battle contracts/adapters, Enemy, Stage/Mainline/RunFlow, Unified host/shell;
- any Scene, existing Prefab, BuildSettings, ProjectSettings, package manifest,
  artwork, PNG, importer, material, animation, VFX, ItemDetail, or Sandbox file;
- CoreLoopLab dev-session availability, BattleSandbox tray/board controllers,
  archived Gravity/Tabletop code, or any V02 inventory/runtime;
- AGENTS, LOCKED, Queue, Notion, Git state, or unrelated dirty files.

No Git command or operation is authorized.

## 7. Required Sources And Frozen Baselines

Read only the V2 startup files plus these relevant sources. Their task-start
SHA-256 values are protected:

```text
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1Lv1StarterItemBaseline.cs|A4C454FFCC0A3B5CE1BF2181A39B79CA73C809FD81FD5D3AA3257C7908A140C7
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FixedFirstClearItemInstanceCarrier.cs|EFE734E86E60B54ECEED7A6518EB84CA9407E61E2898BF681EE2E855A8B5E76D
Assets/_Game/Scripts/TalismanBag/Items/Generation/ItemRarityInstanceFoundation.cs|937F04F1392F3C502A3BF3757094B3F4DC4EEDFE5D12EFD4E6D814F844132356
Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs|794C3A6D9DBE9FB1FF96275C247BCEF4D9D248EDC1DF3EAEE6D396DD87969827
Assets/_Game/Scripts/TalismanBag/Items/I031InventoryPlacementContract.cs|6FF5A159A3CFCBB810D271098870D0487556954F1F915BF781184B24DB4574BD
Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs|0F837D048D30A34F6D7C23FF03B74FAC4BB498BD7523E7EFF51B1BF319C9BB8F
Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs|606ACDC6538EEDA86F7631840A6ACBB47284368F198EF413293BB844833776C9
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractPrimitives.cs|60385BC84807B88122006948AB402FBA39C5D3EC5721D6A387DA842F3C7E05E3
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardClaimAndDedupeIdentities.cs|318573D6FC2378B33C7E0BFE8A2357328EAD2971D2B9BDC5F9C3466906F2494C
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropCanonical.cs|AC5DC9EFD9308752BCA2DAF1B39B3592233631CC1B5F22BBD854BBA6E6B49944
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardResult.cs|13A6698BD8D3B15CE21222DF0EDFB66D69E7F3B2BF09F08608AF5CD0E8687CCE
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractValidator.cs|ED60A14BABEF252BE4D9E646EA9AD16712B770F119D67E7BDE2182170522A796
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs|3D3501203162FF8F5EF40467B3846E22403EC515B02B2207BB17B0D2FA9B9F4C
Assets/_Game/Scripts/TalismanBag/Contracts/Battle/C1Lv1FormalBattleApplicationContracts.cs|1682BC1D7C38D694C564524BBA080F99DF1FFE5FADFA5F76D3900194DA81935A
Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1Lv1FormalBattleApplicationAdapter.cs|D54125939F04DEE97DC4A60DF6257D90F8DF5D38F9112C856342A3F90A295649
Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.cs|25283BCA93BA382EAB133CEFAB394A66D7F6EB6BE87014469C38FACE1157BD01
Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs|511572CB57772015890768632F1D9B8E71D9BDC103537AED52F0EE444CD4EA4A
Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab|15280C4AD42266951E3FD7F72F68BEB9D5311E696ABDED32802671352F1CA801
Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity|FC5E64037CACB2EA00ACC7BA614D404E3AB26D5FFB3CC091DF0BF15B4458F1D9
ProjectSettings/EditorBuildSettings.asset|CF244226FFD286638DEC957BBE5A9ABCA1DE52E9D529D71D406B5D03DE805932
```

The accepted design source is
`Docs/V0.4/V0.4-C1Lv1StarterItemBaseline01_Assignment.md`, specifically the
released loop `I031 + I001@white -> I002@white -> reposition` and
`LIT_REQUIRED` rule. It is read-only.

## 8. Patch Budget

- Suggested implementation files: 20 new files including metas/prefabs.
- Existing file modifications: zero.
- Scene modifications: zero.
- Prefab authoring: exactly three new prefabs.
- If an existing file, fourth prefab, Scene edit, runtime-created UI fallback,
  or a second state owner is required, stop as
  `ARCHITECTURE_REASSESS_REQUIRED` before writing outside the whitelist.

## 9. Required Behavior

### 9.1 Formal session truth

1. Initialize only an explicit `CAMPAIGN_NORMAL_LV1` C1 session with a
   non-empty session token and positive reset generation.
2. Create one deterministic fixed starter `I001@white` ordinary instance via
   the existing `ItemRarityInstanceFoundation`. Use a stable technical identity
   separate from the Package 1 I002 identity; freeze its canonical signature in
   focused tests after first calculation.
3. Own exactly one `SPECIAL_I031` using the existing I031 contract. I031 remains
   a special lighting source and never becomes an ordinary reward/combat row.
4. The released initial arrangement is 5x5, with I031 at `(1,1)` and I001 at
   `(0,1)`, rotation 0. The existing Item snapshot must validate this and report
   I001 direct-lit. Do not manufacture `isLit`.
5. The initial available roster is I031 plus I001. I002 is unavailable until an
   accepted fixed first-clear entitlement is consumed.

### 9.2 Entitlement

1. Accept only a valid released `RewardResult` for C1 stage 1-1 containing the
   exact Item-owned `c1fi_1_1_i002_white_v1` and released instance canonical
   signature from Package 1.
2. Validate Reward contract/source/context/stage/claim facts through the
   existing read-only contracts. Do not reproduce Reward production or claim
   state.
3. Add I002 to the formal session roster exactly once. Repeating the same
   released reward is accepted as an unchanged no-op. A conflicting reward,
   instance identity, token, generation, context, stage, or signature fails
   closed and leaves the session byte-identical.
4. Entitlement is memory-only in this P0 package. New session initialization or
   reset restores the initial I031+I001 roster. No persistence is implied.

### 9.3 Arrangement transactions

1. Expose immutable commands/results for `PlaceFromTray`, `MoveOnBoard`,
   `ReturnToTray`, and session reset. Every command carries session token,
   reset generation, command identity, stable instance identity, expected prior
   session signature, and placement candidate when applicable.
2. Use `DefaultItemSystemSnapshotProvider` for board bounds, eye exclusion,
   shape rotation, overlap, I031 source truth, and lighting. Do not duplicate or
   reinterpret those rules in the new authority.
3. Ordinary Item placement identity is its stable itemInstanceId. I031 uses
   `P_SYSTEM_I031` and `SPECIAL_I031` exactly.
4. Successful commands replace one immutable session snapshot atomically.
   Duplicate command IDs are deterministic no-ops only when their full request
   signature matches. Stale/conflicting commands reject with no partial state.
5. I002 placement before entitlement, unknown identities, duplicate instances,
   eye coverage, overlap, out-of-bounds cells, unsupported rotation, and source
   mismatch all fail closed.
6. Cancel is represented by not committing a command; presentation must not
   mutate authority while previewing.

### 9.4 Battle-readable Item output

1. Publish an immutable `C1FormalItemBattleInputSnapshot` derived only from the
   accepted session and current valid `ItemSystemSnapshot.v2`.
2. Include stable session/arrangement/ItemSystem signatures and one row per
   available ordinary I001/I002 instance with instance identity, base/rarity
   identity, baseline projection signature, placed state, and authoritative
   `isLit` fact.
3. Exclude I031 from ordinary combat rows while retaining the source placement
   in the arrangement/ItemSystem evidence.
4. Do not publish damage timing, target, applied damage, Enemy facts, Battle
   layout identifiers, or a fabricated boolean fixture. Battle/Mainline later
   translates this snapshot into its released request.
5. Unknown/unsupported facts remain absent or explicit diagnostics, never zero.

### 9.5 Replaceable presentation carriers

1. Create separate authored `C1FormalItemBoard`, `C1FormalItemTray`, and
   `C1FormalItemCard` prefabs. The Board and Tray prefabs must be independently
   mountable later into the existing Unified `BoardArea` and `ItemTrayArea`.
2. Prefabs contain only presentation components, serialized references, and
   intent routing. They never own authoritative Item collections or placement.
3. The Board displays the fixed 5x5 cells and accepted Item placements. The Tray
   displays available but unplaced Items. Cards use the final Item artwork,
   preserve artwork aspect ratio, and expose lit/unlit state without deriving it.
4. Drag preview may be presentation-local. Drop intent calls the presenter,
   which submits exactly one immutable authority command and republishes only an
   accepted result. Reject/cancel restores the prior view without a fake state.
5. No runtime hierarchy fallback, scene name lookup, BuildSandbox dependency,
   hidden dev controls, or direct Battle invocation is allowed.

## 10. Failure And Lifecycle Rules

- Missing catalog/baseline/carrier/Reward facts: explicit failure, no session
  or partial entitlement.
- Duplicate or stale request: deterministic unchanged result or explicit reject
  as specified above.
- Invalid Item snapshot: authority remains on the last accepted immutable
  snapshot; no view publication of invalid candidates.
- Prefab reference missing: presentation fails closed with one stable
  diagnostic and does not create replacement objects.
- Session reset/reload: memory-only state is cleared and rebuilt from the fixed
  I031+I001 baseline. No PlayerPrefs, static mutable singleton, or Scene object
  may preserve entitlement.
- View bind/unbind: listeners are idempotent; repeated bind does not duplicate
  commands, cards, or callbacks.

## 11. Delivery Shape

- Runtime: new immutable contracts, state authority, presenter, and prefab view
  components under Item ownership.
- Prefab: three new replaceable prefabs only.
- Editor: one focused deterministic test entry and one explicit prefab authoring
  entry.
- Scene: none.
- Reports: none. Terminal sync is the minimal delivery record.
- Integration: later Mainline package mounts the prefabs into Unified slots and
  translates the Item battle snapshot. That integration is not part of this
  package.

## 12. Verification

Required minimum:

1. Current-source scoped Runtime compile: zero errors.
2. Current-source scoped Editor/test compile: zero errors.
3. Focused deterministic tests without Play mode:
   - fixed I001 identity/signature and exact initial I031+I001 arrangement;
   - real Item snapshot valid and I001 direct-lit;
   - released I002 reward accepted once and duplicate no-op;
   - wrong/stale/conflicting Reward/session/command facts reject unchanged;
   - legal place/move/return for I001/I002/I031;
   - overlap, eye, bounds, rotation, unavailable I002 and duplicate identity
     rejection;
   - weak I002-unlit and target I002-lit arrangements produce different exact
     immutable Battle input signatures while baseline values remain read-only;
   - reset removes only temporary I002 entitlement and restores exact initial
     signature;
   - determinism under repeated calls and invariant culture.
4. REV02 allows exactly one final owned Unity Editor authoring/validation run
   after the three attachable view classes have same-named source files and the
   complete current Runtime/Editor source graph compiles. It may only create the
   three new prefab assets and validate serialized references. It must not enter
   Play mode, open/save a Scene, run Builders, or modify any existing asset.
   Record command, PID, log, timeout, and owned-process cleanup. No further
   Unity authoring retry is permitted after this REV02 run.
5. Prefab static validation: no missing scripts/references, no Scene dependency,
   Board/Tray independently mountable, Card reference valid, no runtime-created
   hierarchy fallback, no BuildSandbox/V02/Editor runtime reference.
6. All protected baselines and the exact twenty-file REV02 whitelist pass at
   close; the two superseded grouped View paths are absent.

Not required:

- Fresh Play or Unity batch Play harness;
- user handtest for this unmounted prerequisite;
- formal navigation, Battle, Reward production, Save, APK, or Scene QA.

The later C1 final integration package owns the single end-to-end user test.

## 13. Process Ownership

- Code and deterministic tests should run offline without Unity.
- The one final REV02 prefab authoring run is permitted only when code is
  write-quiescent, all three same-named `MonoScript` assets are resolvable, and
  the project serialization gate is clean.
- Record the exact owned Unity PID/command/log, use a bounded timeout, and clean
  only owned processes. The package cannot close with an owned Unity,
  ShaderCompiler, Bee, lockfile, or EditorInstance residue.
- Do not wait on or manipulate a user/external Unity process. If serialization
  is occupied, keep code complete and return the exact blocker once.

## 14. Completion And Stop Conditions

Package completes as
`C1_FORMAL_ITEM_SESSION_AND_ARRANGEMENT_AUTHORITY_PASS / PACKAGE_COMPLETE`
when:

- all state/ownership/output behavior above passes focused QA;
- exactly the twenty new REV02 implementation paths exist, the two superseded
  grouped View paths are absent, and no existing file was modified;
- all three new prefabs validate and all protected hashes remain exact;
- no task-owned process or lock remains;
- no unresolved user decision exists.

Stop as `ARCHITECTURE_REASSESS_REQUIRED` if implementation requires:

- modifying existing ItemSystem/lighting/shape/catalog truth;
- approving a formal Scene as a dev installer host;
- copying BattleSandbox/CoreLoopLab/V02 runtime ownership;
- Scene modification, runtime UI fallback, hidden I031 source, fake lit boolean,
  formal Save/claim/progression, or a second roster/placement owner;
- changing accepted Item values or Battle/Reward/Mainline contracts;
- any write outside the exact whitelist.

No follow-on package may be started by this development task.
