# V0.4 BattleSandboxAllItemUiAuthoringVisibility01 Assignment

Status: `OVERALL_SHARED_PRESENTATION_APPROVED / ITEM_GUARD_READY_FOR_DEV`

Guard receipts:

```text
OVERALL_SHARED_PRESENTATION_CONFIRM_BATTLESANDBOXALLITEMUIAUTHORINGVISIBILITY01
ITEM_GUARD_PASS_BATTLESANDBOXALLITEMUIAUTHORINGVISIBILITY01
```

Target receipt:

```text
GUARD_PASS_BATTLESANDBOXALLITEMUIAUTHORINGVISIBILITY01
```

## 1. Package identity

Package:

```text
V0.4-BattleSandboxAllItemUiAuthoringVisibility01
```

Repository:

```text
F:\Porject\TalismanBagBrawl
```

Primary owner: Item Guard.

Joint boundary: Overall / Shared Presentation Guard has approved this narrow Editor/dev-only
authoring-preview package. Enemy Guard and Capability Algorithm Guard are not required.

This package does not start Item Detail Base Prefab Migration, Prefab A/B, consumer migration,
UnifiedBattlePage or legacy hierarchy cleanup.

## 2. Part A - frozen BuildQualification diagnosis

The visible FaMen/QiLei variation is the intended result of the current BattleSandbox fixed-seed
ordinary-instance generation. It is not evidence that the Item Detail panel randomly drops a
section.

Current BattleSandbox ordinary roster:

```text
rarity = Orange
packageSeed = 404310000
rootSeed = packageSeed + ordinal
generationVersion = 1
profileId = candidate_build_orange
domain = build / qualification / candidate_build_orange
weights in profile order:
  None = 5
  FaMenOnly = 20
  QiLeiOnly = 20
  Dual = 55
```

The exact generation path is:

```text
ItemSystemBattleSandboxViewProjection
-> ItemBalanceCandidateDetailSandboxAdapter
-> ItemInstanceRollEngine.GenerateBuildQualification
-> ItemInstanceProjectionContractSnapshot.buildQualification
-> ItemInstanceQualifiedBuildStateSnapshot.v1
-> ItemDetailQualifiedBuildTrackProjector
-> ItemDetailPanel
```

Qualification semantics:

```text
None        -> no eligible Build track
FaMenOnly   -> eligibleFaMenBuildId only
QiLeiOnly   -> eligibleQiLeiBuildId only
Dual        -> both eligible track IDs
```

The package must not alter this matrix, the seeds, the weights or the Roll algorithm.

### 2.1 Exact I001-I030 matrix

`rollTarget` is the deterministic bounded target in `[0,99]` and is included only as audit
evidence. It is not a new gameplay field.

| Item | Display name | Root seed | Roll target | Qualification | eligibleFaMenBuildId | eligibleQiLeiBuildId | Expected visible Build sections |
|---|---|---:|---:|---|---|---|---|
| I001 | 震雷符 | 404310001 | 41 | QiLeiOnly |  | qilei:fu | QiLei |
| I002 | 五雷急符 | 404310002 | 80 | Dual | famen:zhenlei | qilei:fu | FaMen + QiLei |
| I003 | 震雷破壳印 | 404310003 | 44 | QiLeiOnly |  | qilei:yin | QiLei |
| I004 | 五雷急令 | 404310004 | 77 | Dual | famen:zhenlei | qilei:ling | FaMen + QiLei |
| I005 | 照壳雷镜 | 404310005 | 28 | QiLeiOnly |  | qilei:jing | QiLei |
| I006 | 天鼓槌 | 404310006 | 3 | None |  |  | neither |
| I007 | 离火符 | 404310007 | 77 | Dual | famen:lihuo | qilei:fu | FaMen + QiLei |
| I008 | 焚邪长符 | 404310008 | 27 | QiLeiOnly |  | qilei:fu | QiLei |
| I009 | 离火焚邪印 | 404310009 | 19 | FaMenOnly | famen:lihuo |  | FaMen |
| I010 | 风火令旗 | 404310010 | 78 | Dual | famen:lihuo | qilei:ling | FaMen + QiLei |
| I011 | 照焚铜镜 | 404310011 | 4 | None |  |  | neither |
| I012 | 长明灯 | 404310012 | 64 | Dual | famen:lihuo | qilei:fa | FaMen + QiLei |
| I013 | 护身符 | 404310013 | 96 | Dual | famen:zhongyue | qilei:fu | FaMen + QiLei |
| I014 | 镇宅长符 | 404310014 | 55 | Dual | famen:zhongyue | qilei:fu | FaMen + QiLei |
| I015 | 中岳镇守章 | 404310015 | 32 | QiLeiOnly |  | qilei:yin | QiLei |
| I016 | 护坛令旗 | 404310016 | 5 | FaMenOnly | famen:zhongyue |  | FaMen |
| I017 | 照邪八卦镜 | 404310017 | 35 | QiLeiOnly |  | qilei:jing | QiLei |
| I018 | 镇山铜鼎 | 404310018 | 23 | FaMenOnly | famen:zhongyue |  | FaMen |
| I019 | 净水符 | 404310019 | 13 | FaMenOnly | famen:xuanshui |  | FaMen |
| I020 | 涤秽长符 | 404310020 | 36 | QiLeiOnly |  | qilei:fu | QiLei |
| I021 | 玄水涤秽印 | 404310021 | 23 | FaMenOnly | famen:xuanshui |  | FaMen |
| I022 | 玄水流令 | 404310022 | 30 | QiLeiOnly |  | qilei:ling | QiLei |
| I023 | 照秽水镜 | 404310023 | 82 | Dual | famen:xuanshui | qilei:jing | FaMen + QiLei |
| I024 | 净水盂 | 404310024 | 63 | Dual | famen:xuanshui | qilei:fa | FaMen + QiLei |
| I025 | 封煞符 | 404310025 | 90 | Dual | famen:taibai | qilei:fu | FaMen + QiLei |
| I026 | 太白斩符 | 404310026 | 47 | Dual | famen:taibai | qilei:fu | FaMen + QiLei |
| I027 | 太白斩煞印 | 404310027 | 92 | Dual | famen:taibai | qilei:yin | FaMen + QiLei |
| I028 | 断煞令 | 404310028 | 2 | None |  |  | neither |
| I029 | 照煞镜 | 404310029 | 40 | QiLeiOnly |  | qilei:jing | QiLei |
| I030 | 桃木剑 | 404310030 | 15 | FaMenOnly | famen:taibai |  | FaMen |

Authority/display evidence:

```text
I001 QiLeiOnly: existing exact runtime verifier + prior user observation
I004 Dual: existing exact runtime verifier + user observation
I006 None: existing exact runtime verifier
I007 Dual: user observation matches generated authority
I009 FaMenOnly: existing exact runtime verifier + user observation
I002-I003, I005, I008, I010-I030:
  same non-item-specific projection path; exact deterministic authority is listed above;
  per-item visual confirmation is part of this package's verifier matrix and user workflow.
```

Do not label an unobserved row as user-handtest PASS. The dedicated report must keep separate:

```text
AUTHORITY_GENERATION_PASS
RUNTIME_PROJECTION_PATH_PASS
USER_VISUAL_CONFIRMATION
```

## 3. Objective

Provide an explicit, reversible Editor-only authoring mode for:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

When enabled in Edit Mode, the user must be able to see and manually tune:

- the existing BattleSandbox presentation roots;
- the full existing `ItemDetailPanel` frame;
- header, rarity, name, level, power, status and tab controls;
- every existing player-facing section title/body;
- every existing authored stat/affix/art row;
- all four core-effect rows;
- FaMen overview plus Build 2/4/6 rows;
- QiLei overview plus Build 2/4 rows;
- placement recommendation and old-item/flavor rows;
- existing icons, text, status badges, dividers and artwork slots;
- existing debug tab content where it is already part of the panel.

The preview content is representative Editor/dev-only text and imagery. It is not Item data,
not a generated instance, not a Snapshot, and not a second state owner.

## 4. Required authoring workflow

Add explicit menu commands under a clear BattleSandbox authoring path:

```text
Enable Full BattleSandbox Item UI Authoring Preview
Disable Full BattleSandbox Item UI Authoring Preview
Validate Full BattleSandbox Item UI Authoring Preview
```

Required behavior:

1. The tool operates only when the exact target BattleSandbox scene is the active scene.
2. Enable uses existing Scene objects only.
3. Enable does not instantiate, clone, delete, rename, reparent or reorder UI.
4. Enable makes the existing presentation roots and ItemDetail presentation children needed for
   visual authoring visible.
5. Enable binds one representative full authoring `ItemDetailViewModel` through the existing
   `ItemDetailPanelView.Bind` API.
6. The representative model must display both Build sections and four core rows simultaneously,
   regardless of the current real instance qualification. It must carry an explicit
   `EDITOR_DEV_ONLY_AUTHORING_PREVIEW` marker in an existing debug/player-safe location.
7. Enable may change serialized active state, existing Text content, existing Image Sprite/color
   state and existing tab/section presentation state.
8. Enable must not write any RectTransform, sibling index, hierarchy, LayoutGroup,
   ContentSizeFitter, ScrollRect geometry or user-authored size/position value.
9. Disable must hide the detail panel and remove/replace the preview marker without modifying
   geometry. It must not delete the user's hand-tuned layout.
10. The tool must be explicit. Do not add `InitializeOnLoad`, scene-open auto-apply or Play-exit
    auto-apply behavior for BattleSandbox.
11. The user may save the scene while preview is enabled. On Play, the existing runtime adapter
    must still call `SetVisible(false)` during initialization and keep the panel hidden until an
    item is clicked.
12. After a real item click, runtime content must overwrite preview text and show only the real
    qualification/state for that instance.
13. Exiting Play returns to the saved Edit Mode authoring preview without creating a runtime copy.

Manual user flow:

```text
Open target scene
-> Enable Full BattleSandbox Item UI Authoring Preview
-> tune existing UI in Edit Mode
-> save scene
-> enter Play
-> confirm detail starts hidden
-> click real items and confirm authoritative runtime content
-> exit Play
-> continue Edit Mode tuning
-> Disable Preview only when a clean initial-state view is needed
```

## 5. Exact existing-file whitelist

Only this existing file may be changed:

| Path | Task-start SHA-256 | Permission |
|---|---|---|
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `993b53b7f6aaecd5816ff8ad7bcc369ae1cb0c2471e8432c4cc483e8f476fc4e` | Authoring visibility/text/image state only; no hierarchy or geometry rewrite |

This is the task-start disk baseline, not Git HEAD. The scene is already user-tuned and dirty.
Do not overwrite it from Git, a Prefab or a Builder.

The developer must record:

```text
taskStartSceneSha256
postInstallSceneSha256
postDisableInMemoryState
```

The final scene hash may change only through the approved authoring state and user-owned manual
tuning. Do not invent or silently accept a new geometry baseline.

## 6. Allowed new implementation files

Add exactly:

```text
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxAllItemUiAuthoringVisibilityTool.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxAllItemUiAuthoringVisibilityTool.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxAllItemUiAuthoringVisibilityVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxAllItemUiAuthoringVisibilityVerifier.cs.meta
```

The Editor tool may contain a dedicated presentation fixture. Do not add a runtime component,
ScriptableObject, Manager, Catalog or Scene state owner.

Required batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.BattleSandboxAllItemUiAuthoringVisibilityVerifier.VerifyStaticBatch
```

The verifier must exercise enable/disable in memory on a temporary opened scene context and must
not save over user geometry. The final serialized preview install must be performed only through
the explicit target-scene menu command.

## 7. Report whitelist

The package may create only:

```text
Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilityReport.md
Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilitySpec.csv
Docs/V0.4/Reports/BattleSandboxBuildQualificationMatrix30.csv
Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringNodeInventory.csv
Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilityManualTest.md
Docs/V0.4/Reports/BattleSandboxAllItemUiAuthoringVisibilityLeakCheckReport.md
```

Do not create `.meta` files under `Docs`.

`BattleSandboxBuildQualificationMatrix30.csv` must reproduce Section 2 exactly and include:

```text
baseItemId
displayName
rootSeed
rollTarget
buildQualification
eligibleFaMenBuildId
eligibleQiLeiBuildId
expectedFaMenVisible
expectedQiLeiVisible
authorityResult
runtimeProjectionResult
userVisualConfirmation
```

## 8. Protected baselines

The following files must remain byte-identical:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` | `89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs` | `957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs` | `b02a95f760f03f7b53bd4e7ac5b9efd638c1d2f328bf7d098e3ccc45e43b3358` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs` | `ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `4d87522e3d172a4804a576471f9de31b108a33ad9bdf239b6be243a820b0e7d3` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/DeterministicItemRandom.cs` | `021959b426899ff937dae82dfc563b15d52a83a7591d19374c0ddc67ecccf075` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs` | `19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e` |
| `Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset` | `d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |

Also protected:

- Item Detail ViewModel, Composer and both qualified projectors;
- Package A Build qualification state and canonical signatures;
- four-core P1/P2/P3 authority, runtime state and reports;
- ItemSystemSnapshot.v2, IF01, BoardAuthority, BoardAdapter and P6;
- all Item data, Candidate profiles, rarity, probability, Roll and RNG;
- all fonts, Sprites, PNGs and Prefabs;
- ItemSandbox Scene and every other Scene;
- EnemySystem, BoneAspect, EncounterPresentation, Enemy queues/reports;
- formal Battle, RunFlow, SaveData, Reward, Chapter and Boss;
- AGENTS.md and `Docs/LOCKED/*`.

## 9. Required authoring node inventory

The verifier/report must inventory every existing GameObject changed by Enable or Disable:

```text
scenePath
hierarchyPath
componentRole
beforeActiveSelf
previewActiveSelf
disableActiveSelf
textOrSpriteTouched
rectTransformTouched
created
deleted
renamed
reparented
```

Required result:

```text
rectTransformTouched = false for every row
created = false
deleted = false
renamed = false
reparented = false
```

Do not treat an empty GameObject as redundant. It may be a layout anchor, mask, safe-area root,
animation pivot, raycast layer, Scene Slot or serialized reference target.

## 10. Required verifier assertions

The dedicated verifier must prove:

1. The target scene and exactly one authored `ItemDetailPanelView` are found.
2. No runtime Item Detail Prefab is instantiated.
3. Enable operates only on the target scene.
4. All existing ItemDetail header/status/tab controls intended for authoring are visible.
5. Every existing player section is visible with non-empty representative content.
6. The core section exposes exactly four authored rows, not five.
7. FaMen and QiLei Build authored groups are simultaneously active.
8. FaMen has overview + 2/4/6 rows; QiLei has overview + 2/4 rows.
9. Existing art/icon/text/status slots are visible without a new duplicate hierarchy.
10. The preview carries `EDITOR_DEV_ONLY_AUTHORING_PREVIEW`.
11. No RectTransform, hierarchy, sibling order or layout geometry is changed by Enable/Disable.
12. Disable hides the detail panel without deleting user layout.
13. Play initialization still hides the authored panel before the first item click.
14. Real I001, I004, I006, I007 and I009 runtime detail retains its actual qualification.
15. The exact 30-row BuildQualification matrix matches Section 2.
16. I031 remains a special system item with no ordinary BuildQualification.
17. The formal Roll algorithm, weights, seeds, Profiles and Item data remain unchanged.
18. Scene changes are limited to the approved authoring visibility/content state.
19. Prefab, ItemSandbox Scene, Views, runtime adapters, BuildSettings and all protected hashes
    remain unchanged.
20. LeakCheck and exact whitelist checks pass.
21. Package-scoped `git diff --check` passes.

Required markers:

```text
BUILDQUALIFICATION_MATRIX_30_PASS
BUILDQUALIFICATION_VARIATION_IS_AUTHORITY_PASS
AUTHORING_PREVIEW_EXPLICIT_ENABLE_DISABLE_PASS
FULL_BATTLESANDBOX_UI_EDITMODE_VISIBLE_PASS
ITEMDETAIL_ALL_EXISTING_FIELDS_VISIBLE_PASS
FOUR_CORE_ROWS_VISIBLE_PASS
FAMEN_QILEI_SIMULTANEOUS_AUTHORING_PASS
NO_RUNTIME_DATA_OVERRIDE_PASS
RUNTIME_INITIAL_PANEL_HIDDEN_PASS
NO_HIERARCHY_OR_GEOMETRY_REWRITE_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_AUTHORING_HANDTEST_WAITING
PREFAB_MIGRATION_NOT_STARTED
LEGACY_CLEANUP_NOT_STARTED
```

## 11. Unity and concurrency isolation

Before every Unity run:

1. Check `Temp/UnityLockfile`.
2. Check Unity, Tuanjie and ShaderCompiler processes.
3. If the user Editor or another package batch is active, wait.
4. Do not close, kill or steal another process.
5. Run only this package's dedicated verifier.

Command:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
  -batchmode -quit
  -projectPath F:\Porject\TalismanBagBrawl
  -executeMethod TalismanBag.EditorTools.BuildSandbox.BattleSandboxAllItemUiAuthoringVisibilityVerifier.VerifyStaticBatch
```

Do not run any Builder or historical verifier that writes a Scene, Prefab, report baseline or
protected asset.

EnemySystem/BoneAspect/EncounterPresentation work is concurrent external work. Treat it as
read-only and do not revert, format, stage or claim it.

If Unity creates a non-whitelist `.meta`, remove it only when this process's ownership is proven.
If ownership is uncertain, stop and report.

## 12. User handtest

After development and static QA, stop at:

```text
DEV_COMPLETE / QA_STATIC_PASS / USER_AUTHORING_HANDTEST_WAITING
```

User handtest:

1. Open `Scene_TalismanBag_V04_BattleSandboxPreview.unity` in Edit Mode.
2. Run `Enable Full BattleSandbox Item UI Authoring Preview`.
3. Confirm the full existing BattleSandbox UI and ItemDetail frame are visible.
4. Confirm header/status/tabs, all existing sections, both Build groups and four core rows are
   visible at once.
5. Adjust several existing UI positions/sizes manually and save the scene.
6. Enter Play. Confirm ItemDetail starts hidden.
7. Click I004 and I007: both Build groups use real runtime data.
8. Click I009: FaMen only.
9. Click I001: QiLei only.
10. Click I006: neither Build contribution.
11. Exit Play. Confirm the saved Edit Mode authoring layout remains.
12. Run Disable Preview and confirm the panel hides without losing geometry.
13. Re-enable Preview and confirm all authoring fields return without a duplicate panel.

User handtest PASS is required before accepting the scene's final authoring state. Prefab
migration remains blocked.

## 13. Stop conditions

Stop without claiming PASS if:

- any task-start hash differs before development;
- the scene cannot be changed without touching hierarchy or RectTransform geometry;
- the tool needs a runtime component, Manager, Catalog or second state owner;
- runtime Play reads the authoring fixture;
- a real item is forced to Dual or any Build qualification changes;
- the runtime initial panel becomes visible before click;
- a new ItemDetail panel or UI hierarchy must be instantiated;
- Scene/Prefab/View/runtime adapter/Item data/Enemy/formal-flow changes are required;
- another Unity process owns the project lock;
- exact whitelist cannot be maintained;
- compile or verifier fails.

No protected baseline may be updated to make a failure pass.

## 14. Completion sync

On QA success, send directly to Item Guard, Overall Guard and RepoOps:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BattleSandboxAllItemUiAuthoringVisibility01

AssignmentSHA256:
<exact Guard-supplied SHA>

Result:
DEV_COMPLETE / QA_STATIC_PASS / USER_AUTHORING_HANDTEST_WAITING

BuildQualificationAudit:
30/30 exact

ModifiedFiles:
<exact files>

Reports:
<six reports>

Scene:
taskStartSha / finalSha / geometry-hierarchy diff

Unity:
<compile, command, exit code, log>

ProtectedHashes:
<before/after>

GitOperations:
NONE
```

Do not start Prefab migration, consumer migration, UnifiedBattlePage, legacy hierarchy inventory
or retirement, or any other Item package. No `git add`, commit, tag, push, reset or rollback is
authorized.
