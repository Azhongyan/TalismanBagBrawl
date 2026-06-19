# StageConfigPanel01 Task 0-10 QA Report

## 1. Scope

- Package: `V0.2-StageConfigPanel01`
- Current delivery: Task 0-10 implementation and QA support
- Unity project: `F:\Porject\TalismanBagBrawl`
- Unity version: `2022.3.50f1c1`
- Main configuration panel: `Tools > Talisman Bag > Stage Config Panel 01`
- QA debug panel: `Tools > Talisman Bag > V0.2 Balance Debug Panel > Stage QA / 整包验收`

This package does not include Tune01, 3-10, new gameplay systems, a production GM backend, or a production operations backend.

## 2. Automated Verification

| Check | Result | Evidence |
|---|---|---|
| Unity compilation | Passed, no `error CS` | `stageconfigpanel01_task9_10_compile.log` |
| DataCatalog | Error=0, Warning=15, Info=0 | `stageconfigpanel01_task9_10_validate.log` |
| Stage count | 20 | `stageconfigpanel01_task9_10_smoke.log` |
| Chapter 1 stages | 1-1 through 1-10 present | Smoke passed |
| Chapter 2 stages | 2-1 through 2-10 present | Smoke passed |
| EnemyGroup references | 20 groups resolve an enemy | Smoke passed |
| stopBeforeBoss | 2-9 is the only stop stage | Smoke passed |
| 2-10 autoAdvance | false | Smoke passed |
| Fixed tutorial rewards | 1-2, 1-4, 1-5, 1-6, 1-9 present | Smoke passed |
| 1-10 Boss reward | `chapter_1_10_clear` present | Smoke passed |
| 2-10 Boss reward | `boss_2_10_clear` present | Smoke passed |
| Bronze seal item | Active `bronze_seal_basic` present | Smoke passed |
| QA API surface | 7 required service/flow methods present | Smoke passed |

## 3. QA Debug Panel

All controls are explicitly labelled `[QA / Debug Only]`.

### Edit Mode controls

- Refresh Config
- Validate DataCatalog
- Open StageConfigPanel01

### Play Mode controls

- Reset CoreLoop Save
- Start Main Trial 1-1
- Jump To 1-10 Boss
- Jump To 2-10 Boss Ready
- Simulate Reward by `rewardTableId`
- Print MainTrial State
- Print Resource / Inventory Summary

### Service routing

- Save reset uses `SaveService.ResetSave()`.
- 1-10 and 2-10 QA preparation is owned by `MainTrialFlowService`.
- Route execution and UI state are owned by `V02RunFlowController.StartNewRun()`.
- Reward simulation uses `RewardService.GrantConfig()`.
- Resource and item writes continue through `ResourceService` and `ItemInventoryService`.
- Persistence continues through `SaveService`.

The QA panel does not directly change resource fields, inventory lists, or raw SaveData fields.

## 4. DataCatalog Warning Result

- Error: 0
- Warning: 15
- Info: 0

All 15 warnings are explainable retained legacy data:

- 14 warning rows are seven duplicate item IDs, reported once for each asset in the pair.
- One warning is the retained `heart_demon_boss` / deprecated placeholder display-name collision.
- None of the warnings affect the active 1-10 or 2-10 main path.

See `Docs/V0.2/StageConfigPanel01_KnownWarnings.md`.

## 5. Configuration Panel Checklist

### ItemCatalog

- [ ] Rows display `displayName [itemId]`.
- [ ] Debug, Deprecated, and Legacy data can be distinguished.
- [ ] `bronze_seal_basic` is visible as an active item.
- [ ] Labels remain visible when the window is narrow.
- [ ] Lists and complex fields use Foldout.

### EnemyConfig / EnemyGroup

- [ ] Twenty EnemyGroup assets are visible.
- [ ] The 1-10 verified enemy snapshot is visible.
- [ ] Every `enemyGroupId` resolves.
- [ ] Enemy skills and complex lists use Foldout.

### StageConfig

- [ ] 1-1 through 1-10 are visible.
- [ ] 2-1 through 2-10 are visible.
- [ ] `nextStageId`, order, `autoAdvance`, and `stopBeforeBoss` can be inspected.
- [ ] 2-9 is the only `stopBeforeBoss`.
- [ ] 2-10 has `autoAdvance=false`.

### RewardConfig

- [ ] Fixed tutorial rewards for 1-2, 1-4, 1-5, 1-6, and 1-9 are visible.
- [ ] 1-10 chapter Boss reward is visible.
- [ ] 2-10 Boss reward is visible.

### DropTable

- [ ] Chapter-two normal drop table is visible.
- [ ] 2-1 through 2-9 reference the configured drop path.
- [ ] 2-10 does not grant the normal-stage drop table.

### Boss

- [ ] 1-10 BossConfig is visible.
- [ ] 2-10 BossConfig is visible.
- [ ] 2-10 BossInfoPanel is shown before combat.

### Upgrade

- [ ] Five verified Lv.1 to Lv.2 rows remain unchanged.
- [ ] Upgrade costs and effects are inspectable.
- [ ] First required cultivation can complete and persist.

### DataCatalog

- [ ] Error=0.
- [ ] Warning=15.
- [ ] Info=0.
- [ ] Every warning is explainable using the known-warning document.

## 6. Main Path Regression

### 1-10 path

- [ ] Reset CoreLoop Save from the QA panel.
- [ ] Start Main Trial 1-1.
- [ ] Verify fixed tutorial rewards at 1-2, 1-4, 1-5, 1-6, and 1-9.
- [ ] Alternatively use Jump To 1-10 Boss for focused Boss settlement testing.
- [ ] Defeat 1-10 Boss.
- [ ] Confirm chapter reward is granted through RewardService.
- [ ] Confirm reward is not duplicated after restart.

### Cultivation

- [ ] Complete the required first cultivation.
- [ ] Confirm resource spending uses UpgradeService / ResourceService.
- [ ] Stop and Play again; confirm completion state restores.

### 2-1 through 2-10

- [ ] Confirm 2-1 through 2-9 auto-advance.
- [ ] Confirm normal-stage drops are logged and persisted.
- [ ] Confirm 2-9 stops before the Boss.
- [ ] Confirm 2-10 BossInfoPanel opens.
- [ ] Confirm Boss combat starts only after manual confirmation.
- [ ] Confirm 2-10 Boss reward is granted through RewardService.
- [ ] Stop and Play again; confirm completion state restores.

## 7. Focused QA Debug Tests

### Reward simulation

1. Enter Play Mode.
2. Open `V0.2 Balance Debug Panel > Stage QA`.
3. Enter a valid ID, for example `chapter_1_10_clear`.
4. Record resource/inventory summary before the action.
5. Click `Simulate Reward`.
6. Record the summary after the action.
7. Confirm the Console reports RewardService usage and the save persists after restart.

### 2-10 Boss ready

1. Enter Play Mode.
2. Click `Jump To 2-10 Boss Ready`.
3. Confirm `mainTrialPhase=Chapter2BossReady`.
4. Confirm startup route is `BossInfo`.
5. Confirm BossInfoPanel is visible and combat has not started.
6. Start combat manually, finish the Boss, claim reward, then restart Play Mode.

## 8. Known Risks

- The QA jump methods intentionally prepare deterministic main-trial state and save it. They are compiled only for Unity Editor or development builds.
- Reward simulation modifies the active QA save because it exercises the real RewardService chain.
- The 15 retained legacy warnings remain until a separate legacy migration/removal package is approved.
- Full Play Mode execution cannot be proven by Editor smoke alone; the checklist above remains mandatory.

## 9. Final QA Decision

总体结论：

- [ ] 通过，建议进入 RepoOps 归档
- [ ] 不通过，进入 Fix
- [ ] 有风险，需要 Codex 补充说明

是否允许进入 RepoOps：

- [ ] 允许
- [ ] 不允许
- [ ] 暂缓

是否允许进入 Tune01：

- [ ] 允许
- [ ] 不允许
- [ ] 暂缓

是否允许继续下一个功能包：

- [ ] 允许
- [ ] 不允许
- [ ] 暂缓

## 10. QA Notes

- Tester:
- Date:
- Unity revision:
- Commit:
- Result:
- Reproduction notes:

