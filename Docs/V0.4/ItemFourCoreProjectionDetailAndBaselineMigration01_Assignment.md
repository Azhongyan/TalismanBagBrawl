# V0.4-ItemFourCoreProjectionDetailAndBaselineMigration01 Assignment

Status: `READY_FOR_DEV / ITEM_GUARD_APPROVED / USER_HANDTEST_REQUIRED`

Package: `V0.4-ItemFourCoreProjectionDetailAndBaselineMigration01`

Assignment freeze date: `2026-07-23 Asia/Shanghai`

Primary Guard: `Item System Guard`

Joint Guard: `NOT_REQUIRED`

## 0. Receipt and stop point

Accepted assignment receipt:

```text
ITEM_GUARD_PASS_ITEMFOURCOREPROJECTIONDETAILANDBASELINEMIGRATION01
```

Required development stop state:

```text
DEV_COMPLETE
QA_STATIC_PASS
REAL_RUNTIME_PATH_PASS
USER_HANDTEST_WAITING
PREFAB_MIGRATION_NOT_STARTED
```

This package is not complete until the user passes the manual test in the real BattleSandbox
scene. Static fixture PASS cannot replace that gate.

## 1. Accepted prerequisites

```text
V0.4-ItemFourCoreAuthorityAndMigrationDecision01
  PASS / USER_OPTION_C_FROZEN

V0.4-ItemFourCoreCandidateDataCorrection01
  ITEM_GUARD_ACCEPTED / ITEM_ALGORITHM_GUARD_ACCEPTED / QA_PASS

V0.4-ItemFourCoreAwakeningRuntimeContractCorrection01
  ITEM_GUARD_ACCEPTED / QA_PASS
  ItemCoreEffectIdentityCatalogSnapshot.v2 = 120 rows
  ItemInstanceCoreEffectRuntimeStateSnapshot.v2 = four rows per ordinary instance
  USER_HANDTEST_NOT_APPLICABLE
```

P2 evidence:

```text
Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeContractCorrectionReport.md
SHA-256 270ba4478458090559b9ca8163dd3fade9b71394d3b0ef7c4cb667fb7759d04c

Docs/V0.4/Reports/ItemFourCoreAwakeningIdentityMatrix120.csv
SHA-256 136d25056ad342aa7c396a21ed2b31f4f9bb9252dd15b69d39840a5006b11d45
```

## 2. Frozen player semantics

Every ordinary base item `I001-I030` owns four fixed progressive core effects:

```text
Core1    / Lv10 / candidate_core_ixxx_01       / Ixxx_CORE_01
Core2    / Lv20 / candidate_core_ixxx_02       / Ixxx_CORE_02
Core3    / Lv30 / candidate_core_ixxx_03       / Ixxx_CORE_03
Ultimate / Lv40 / candidate_core_ixxx_ultimate / Ixxx_CORE_ULT
```

Rarity eligibility and visibility are:

```text
White  = 1: Core1
Green  = 2: Core1 + Core2
Blue   = 3: Core1 + Core2 + Core3
Purple = 3: Core1 + Core2 + Core3
Orange = 4: Core1 + Core2 + Core3 + Ultimate
```

Final runtime activation remains:

```text
rarity eligible AND cultivation unlocked AND isLit
```

Core identity is fixed. It is never selected by RNG and is never rerolled.

`Core4`, `candidate_core_*_04` and `Ixxx_CORE_04` remain superseded historical evidence. They must
not appear in current Item Detail data or presentation.

## 3. Package objective

Repair the real Item Detail field lineage:

```text
Generated Item Projection
-> IF01
-> ItemSystemSnapshot.v2
-> ItemInstanceQualifiedBuildStateSnapshot.v1
-> ItemInstanceCoreEffectRuntimeStateSnapshot.v2
-> ItemDetailProjectionComposer
-> ItemDetailQualifiedBuildTrackProjector
-> ItemDetailQualifiedCoreEffectProjector
-> ItemDetailViewModel
-> existing ItemDetailPanelView / four authored core rows
-> real BattleSandbox click path
```

The new projector is an immutable, read-only communication projection. It is not a second Item
state owner and must not recalculate eligibility, cultivation unlock, lighting, active state or
Build stages.

## 4. Required runtime call path

Every ordinary detail open must consume the latest:

```text
baseItemId
itemInstanceId
placementId (empty only for Inventory)
ItemSystemBattleSandboxBoardAuthority.CurrentSnapshot
ItemSystemBattleSandboxBoardAuthority.CurrentQualifiedBuildState
ItemSystemBattleSandboxBoardAuthority.CurrentCoreEffectRuntimeState
```

The BattleSandbox controller must pass all three current snapshots in one call. It must not cache
an earlier core snapshot and must not derive a core state from `baseItemId`.

The existing weaker `Show` overloads must continue to fail closed. A caller that omits the
qualified Build snapshot or the v2 core runtime snapshot must not render a partial or stale detail.

## 5. Qualified core-effect detail projection

Add one public read-only projection contract and projector in:

```text
Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs
```

It may contain the result, projection, row and canonical helper types required by this package.
Do not create a second runtime assembler or Manager.

Each projected ordinary row must retain:

```text
baseItemId
itemInstanceId
placementId
location
rarity
nodeKind
candidateDefinitionId
awakeningNodeId
displayName
description
requiredLevel
requiredRarity
eligibleFact
visibleFact
unlockedFact
litFact
activeFact
stateCompleteness
source runtime canonical signature
```

The projection must use explicit formal order:

```text
Core1, Core2, Core3, Ultimate
```

Do not use enum numeric order, collection position, localized text, PNG name or suffix guessing.
Identity joins are exact `itemInstanceId + baseItemId + placementId/location`.

## 6. Inventory, Board and Unknown presentation

### Inventory

- Static names, descriptions, eligibility, visibility and cultivation-unlocked facts remain
  available.
- Placement, lit and active remain `NotApplicable`.
- Player text may say `未入阵，点亮后生效`.
- It must not say the item has no core data.
- Visible authored-row count equals the rarity policy `1/2/3/3/4`.

### Board unlit

- Exact placement identity is required.
- Eligible and unlocked facts remain visible.
- Lit and active are Known False.
- An unlocked row uses `ItemDetailCoreEffectRowState.UnlockedInactive`.

### Board lit

- Active rows come directly from P2 `activeFact`.
- Known True rows use `ItemDetailCoreEffectRowState.Active`.
- Ineligible rows never become active.

### Move and return

- Moving rebuilds from the latest authority snapshot.
- Returning to Inventory clears placement/lit/active presentation.
- No previous item, placement or active row may remain.

### Unknown and Invalid

- Unknown uses a stable player-safe message such as `当前核心效果状态暂不可用`.
- Unknown must not become zero, false, locked or no qualification.
- Invalid identity, stale placement, duplicate row or base-item mismatch fails closed and hides the
  panel. It must not keep the previous `LastProjectedModel`.

### I031

- No ordinary itemInstanceId.
- No ordinary core qualification or runtime rows.
- Preserve the existing system-item detail and lighting semantics.
- Do not fabricate four empty ordinary rows.

## 7. Existing four-row UI binding

The projector must update the existing ViewModel only:

- `displayCoreEffects`
- the existing player section with state key `coreEffect` or `awakening`
- `coreEffectRowStates`
- `statusFlags` core counts and current/next level
- `awakeningPreview`
- `displayAwakeningStatusText`
- a new immutable `qualifiedCoreEffect` projection property on `ItemDetailViewModel`

The ViewModel clone must deep-clone or safely share immutable qualified projection data.

Visible player rows use current runtime `displayName + description`. The View or Prefab must not
generate content. The existing authored rows own their fonts, sizes, icons, overlays and layout.

Do not modify:

```text
ItemDetailPanelView
ItemDetailSectionView
ItemDetailProjectionComposer
ItemDetailQualifiedBuildTrackProjector
ItemDetailPanel.prefab
either Scene
any core PNG or Sprite
```

## 8. Build-track preservation

P3 must preserve the accepted `ItemDetailQualifiedBuildTrackProjector` path.

Required real samples:

```text
I001 = QiLeiOnly
I004 = Dual
I006 = None / Known Zero
I009 = FaMenOnly
I031 = NotApplicable
```

The final model must contain both:

```text
qualifiedBuildTrack from Package A/B
qualifiedCoreEffect from P2/P3
```

Applying core projection must not erase or regenerate Build track rows. Applying Build projection
must not erase core rows.

## 9. ItemSandbox four-core baseline migration

`ItemBalanceCandidateDetailSandboxAdapter` currently treats Ultimate as an orange-affix surrogate
and excludes it from the four core rows. Correct only this drift:

- Display current visible formal candidates in explicit `01/02/03/ULT` order.
- Ultimate is the fourth core row when visible.
- Stop using Ultimate as `displayOrangeAffix` or `道痕 / 终极核心`.
- Preserve the existing non-core orange/Dao presentation fact from the upstream instance/detail
  model. Do not invent a replacement affix.
- No active `_04` row.

`ItemFullDetailBuildSandboxWorkbenchSession` currently contains a five-node positional fallback
where the fourth candidate can be mislabeled as Core3 and Ultimate was expected at index five.
Correct only this drift:

- Fallback level list is exactly `10/20/30/40`.
- Candidate-to-node mapping is exact `01/02/03/ULT`; no position-only inference.
- Ultimate remains distinct and fourth.
- Existing ItemSandbox preview remains dev-only and does not become a new runtime authority.

Do not modify Candidate profiles, P1/P2 contracts or algorithms.

## 10. Exact existing-file whitelist

Only these existing files may be modified:

| Path | Task-start SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs` | `65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84` |
| `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs` | `da81315cdcd71310ea43729a4ae043b9a61cf33f0e889c706bf81d5bcf24871b` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs` | `ced80cfa99b330ca3f5d67fc22198641de817105608b678603c33343ab555e94` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs` | `1c1504948012b94dcbb1cc91310becd2df169bb736003737c5109f19cd72cc06` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `f7101085b539df95a072ac36cae8ee1d2ccd9952f6af9be0f0324e6b818ecc19` |

These are task-start disk hashes, not Git HEAD. Do not reset, checkout, clean or reconstruct them.

The BuildSandbox changes are limited to the dev-only detail communication call and validation.
If any additional BuildSandbox or CrossSystem file is required, stop and return to Overall Guard.

## 11. Allowed new implementation files

Add exactly:

```text
Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs
Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.cs.meta
```

Required batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.VerifyStaticBatch
```

## 12. Report whitelist

The dedicated verifier may create only:

```text
Docs/V0.4/Reports/ItemFourCoreProjectionDetailAndBaselineMigrationReport.md
Docs/V0.4/Reports/ItemFourCoreProjectionDetailAndBaselineMigrationSpec.csv
Docs/V0.4/Reports/ItemFourCoreProjectionDetailFieldLineage.csv
Docs/V0.4/Reports/ItemFourCoreProjectionDetailRealSampleMatrix.csv
Docs/V0.4/Reports/ItemFourCoreProjectionDetailBaselineMigrationLedger.csv
Docs/V0.4/Reports/ItemFourCoreProjectionDetailManualTest.md
Docs/V0.4/Reports/ItemFourCoreProjectionDetailAndBaselineMigrationLeakCheckReport.md
```

Do not create `.meta` files under `Docs`.

Historical five-core Assignments, verifiers, reports and matrices remain byte-identical evidence.
The baseline ledger must mark them as:

```text
SUPERSEDED_BY_ITEM_FOUR_CORE_AUTHORITY_OPTION_C
HISTORICAL_EVIDENCE_RETAINED
NOT_EXECUTED_BY_P3
```

Do not update their golden hashes to make them pass.

## 13. Protected source and presentation baselines

These paths must remain byte-identical:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs` | `5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs` | `599f9b1469cb4165a9e22a2dd5bf1c5cb683b67399fd347a0cdd4cd59c41a6f2` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` | `89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs` | `957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs` | `6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs` | `116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity` | `8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |

Also protected: fonts, Sprites, PNGs, RectTransforms, IF01, P6, formal Battle, EnemySystem,
BoneAspect, EncounterPresentation, Enemy queues/reports, CrossSystem shared contracts, RunFlow,
SaveData, Reward, Chapter, Boss, AGENTS.md and `Docs/LOCKED/*`.

## 14. P1/P2 authority protection

P3 must not modify:

```text
ItemCompleteCandidateContent.cs
30 ItemBalanceProfile assets
ItemBalanceWorkbenchCatalog.asset
ItemCoreAwakeningRules.cs
ItemCoreEffectIdentityAndRuntimeStateContract.cs
ItemInstanceCoreEffectRuntimeStateValidation.cs
ItemInstanceCoreEffectRuntimeStateAssembler.cs
ItemInstanceQualifiedBuildStateContract.cs
ItemInstanceQualifiedBuildStateAssembler.cs
ItemSystemSnapshot.cs
Roll/Projection/RNG/Probability sources
```

Required current hashes include:

```text
ItemCompleteCandidateContent.cs
15d7a0cbf1b60969e6c27030a7d1d8ccd1d733fb73421396bee5a8ee21382b4b

ItemCoreAwakeningRules.cs
a72c7aa63d04b411c171918c440be56ac39ac7f1046fe30e7a2ffc55bc0a4298

ItemCoreEffectIdentityAndRuntimeStateContract.cs
451b6d3603278a7bcbc983b69e428685c1ebdb9391ebc02cbe9644448118166a

ItemInstanceCoreEffectRuntimeStateValidation.cs
f3534fff59bd46852c5918755caa63a79f4beec7915c876fa51aeb9ded0fd0e6

ItemInstanceCoreEffectRuntimeStateAssembler.cs
e4ca29265d3bb3c18e7b1ff8c3de73190a1dc72b8b2c1d01f03f529343c6c904

ItemInstanceQualifiedBuildStateContract.cs
f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464

ItemInstanceQualifiedBuildStateAssembler.cs
798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8

ItemBalanceWorkbenchCatalog.asset
d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed

P1 profile aggregate
71fc7080eceaf9adf2aa47508e4900d00fae91d8f8158fb1a2618b69241a8143

ItemCandidateCoreEffects120.csv
96cb7ba61376c1a1ab5bdc1ca08a42785bea0f4f864e6f89129d0f9a88fc2284

ItemFourCoreAwakeningIdentityMatrix120.csv
136d25056ad342aa7c396a21ed2b31f4f9bb9252dd15b69d39840a5006b11d45
```

## 15. Dedicated verifier requirements

The new verifier must prove:

1. P1 Candidate authority remains 120 `01/02/03/ULT` rows and Option C remains
   `1/2/3/3/4`.
2. P2 Identity and runtime schemas are current v2 and Core4 formal rows remain zero.
3. Component projection covers White/Green/Blue/Purple/Orange visible row counts
   `1/2/3/3/4`.
4. Inventory preserves static names/descriptions/eligibility/unlock while lit/active are
   NotApplicable.
5. Board unlit and Board lit consume P2 facts without recalculation.
6. Move and return consume the latest canonical snapshot and clear stale state.
7. Unknown differs from Known False and does not expose fake zero/locked rows.
8. Stale itemInstanceId, placementId or baseItemId fails closed.
9. I031 remains system-item NotApplicable with zero ordinary core rows.
10. Candidate detail shows Ultimate as the fourth core row and no `_04`.
11. Ultimate is not reused as orange affix or `道痕 / 终极核心`.
12. ItemSandbox workbench maps exact `01/02/03/ULT`, levels `10/20/30/40`, with no positional
    five-node fallback.
13. Existing qualified Build projector output for I001/I004/I006/I009/I031 is preserved.
14. The final ViewModel simultaneously carries correct Build and core projections.
15. The actual BattleSandbox controller calls the detail adapter with the latest ItemSystem,
    QualifiedBuild and CoreEffectRuntime snapshots.
16. The existing scene-authored `ItemDetailPanelView` receives `Bind` with non-empty core names,
    descriptions and exact row states. The verifier may open the scene read-only but must not save
    it.
17. Switching I001 -> I004 -> I006 -> I009 -> I031 does not retain earlier rows.
18. All external collections and projection rows are immutable.
19. Canonical signatures are Ordinal/InvariantCulture and stable under reversed input.
20. P1/P2 authority, presentation, scene, prefab and BuildSettings hashes remain unchanged.
21. Exact whitelist, LeakCheck and package-scoped `git diff --check` pass.

Required markers:

```text
COMPONENT_FIXTURE_PASS
FOUR_CORE_VISIBLE_COUNT_12334_PASS
INVENTORY_BOARD_CORE_STATE_PASS
REAL_RUNTIME_PATH_PASS
BUILD_TRACK_COEXISTENCE_PASS
ITEMSANDBOX_FOUR_CORE_BASELINE_PASS
ULTIMATE_FOURTH_ROW_PASS
UNKNOWN_INVALID_STALE_REJECTION_PASS
I031_EXCLUSION_PASS
SCENE_AUTHORED_PANEL_BIND_PASS
P1_P2_PROTECTED_HASHES_PASS
PRESENTATION_PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_WAITING
PREFAB_MIGRATION_NOT_STARTED
```

## 16. Unity and parallel-development isolation

Before Unity batch:

1. Check `Temp/UnityLockfile`.
2. Check running Unity processes.
3. If the user Editor or another package batch is active, wait. Do not close, kill or steal it.
4. Run only the dedicated P3 verifier after the lock is free.

Command:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
  -batchmode -quit
  -projectPath F:\Porject\TalismanBagBrawl
  -executeMethod TalismanBag.EditorTools.BuildSandbox.ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.VerifyStaticBatch
```

Do not run a Builder or any historical five-core/v1 verifier. Do not run regressions that save a
Scene or rebuild the ItemDetailPanel prefab.

EnemySystem/BoneAspect/EncounterPresentation changes belong to the concurrent Enemy package.
Treat them as external read-only changes. Never revert, format, stage or claim them.

If Unity imports a new non-whitelist `.meta`, attribute it to this process before removal. If
ownership is uncertain, stop and report.

## 17. User manual test

Target scene:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Required handtest:

1. Play, click Inventory I001: the core section shows four Orange core names/descriptions; all are
   unlocked but not active because the item is not in the formation. QiLei Build qualification
   remains visible.
2. Place I001 on Board but unlit: the same four rows remain; they are unlocked/inactive.
3. Place/move I031 so I001 becomes lit: the four rows become active and Build progress refreshes.
4. Move I001 out of lighting: rows return to unlocked/inactive without disappearing.
5. Return I001 to Inventory: no placement/active residue remains.
6. Click I004: both FaMen and QiLei Build tracks remain, plus four core rows.
7. Click I006: Build is explicit None/Known Zero, but its four core rows still display.
8. Click I009: FaMen-only Build and four core rows display.
9. Click I031: system detail appears with no ordinary Build or core rows.
10. Rapidly alternate I001/I004/I006/I009/I031: no prior text, icon state or row state remains.

User PASS must be returned to Item Guard. Until then the package remains:

```text
DEV_COMPLETE / QA_STATIC_PASS / REAL_RUNTIME_PATH_PASS / USER_HANDTEST_WAITING
```

## 18. Stop conditions

Stop without claiming PASS if:

- any allowed/protected start hash differs before editing;
- P1 is not 120 formal candidates or P2 is not v2/120;
- the solution needs a Scene, Prefab, View, Composer, Build projector, BoardAuthority or
  ItemSystem schema change;
- the solution needs a new runtime state owner or recalculates P2 facts;
- any `_04` row enters current detail data;
- Ultimate payload/name/description changes;
- Build rows regress or are regenerated by the core projector;
- a historical report/verifier would be overwritten;
- another Unity process owns the project lock;
- concurrent Enemy files are modified by this package;
- Unity compile/verifier fails;
- exact whitelist cannot be maintained.

No protected baseline may be updated to make a failure pass.

## 19. Completion sync

On static success, send directly to Item Guard and RepoOps:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-ItemFourCoreProjectionDetailAndBaselineMigration01

AssignmentSHA256:
<exact Guard-supplied SHA>

Result:
DEV_COMPLETE / QA_STATIC_PASS / REAL_RUNTIME_PATH_PASS / USER_HANDTEST_WAITING

Markers:
<all required markers>

ModifiedFiles:
<exact whitelist>

Reports:
<all seven report paths>

Unity:
<compile result, batch command, exit code and log>

ProtectedHashes:
<before/after evidence>

GitOperations:
NONE
```

Do not start ItemDetail Base Prefab Migration, Prefab A/B, Scene consumer migration or Unified
BattlePage. No `git add`, commit, tag, push, reset or rollback is authorized.
