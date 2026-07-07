# Battle Snapshot Adapter Report

Package: `V0.4-BattleSnapshotAdapter01`
Generated: `2026-07-07`
Status: `PASS`
Validation mode: `SOURCE_STATIC`
Unity menu execution: `NOT_RUN_UNITY_CLI_NOT_FOUND`

## Scope

- Added neutral `TalismanBag.Contracts.Battle` DTOs and read-only adapters.
- Did not create or modify UnifiedBattlePage, scenes, prefabs, UI layout, BuildSettings, RunFlow, SaveData, rewards, Boss, or chapter progression.
- V0.4 sandbox result mapping is data-only and defaults to `devOnly=true`, `shouldWriteSave=false`, `shouldGrantReward=false`.

## Contract Types

| Type | Path | Purpose |
| --- | --- | --- |
| `BattleStartRequest` | `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleStartRequest.cs` | Read-only battle request data. |
| `BattleLayoutSnapshot` | `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleLayoutSnapshot.cs` | Neutral board/layout state. |
| `BattleItemSnapshot` | `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleItemSnapshot.cs` | Neutral item placement and stat state. |
| `BattleEnemySnapshot` | `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleEnemySnapshot.cs` | Neutral enemy/boss readable state. |
| `BuildEvaluationSnapshot` | `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BuildEvaluationSnapshot.cs` | Player build summary plus dev diagnostics. |
| `BattleResultSnapshot` | `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleResultSnapshot.cs` | Result data, not a commit command. |
| `BattleContractDiagnostics` | `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleContractDiagnostics.cs` | Report/dev-only diagnostics. |

## Adapter Types

| Adapter | Source | Output | Write Behavior |
| --- | --- | --- | --- |
| `V03BattleStartRequestExporter` | `MainTrialStartupRoute` / `V02RoundConfig` | `BattleStartRequest` | read-only |
| `V03BattleLayoutSnapshotExporter` | `BattleLoadoutSnapshot` | `BattleLayoutSnapshot` | read-only |
| `V04BattleLayoutNormalizer` | `BuildSandboxLayoutSnapshot` | `BattleLayoutSnapshot` | read-only |
| `BuildEvaluationSnapshotExporter` | `BuildEvaluationResult` / modifier/event/context | `BuildEvaluationSnapshot` | read-only |
| `SandboxBattleResultMapper` | `BattleSandboxRuntimeLoopPreview` | `BattleResultSnapshot` | read-only |
| `BattleContractFieldValidator` | in-memory samples | validation snapshot | read-only |

## Validation Findings

| Check | Result | Notes |
| --- | --- | --- |
| Neutral contract layer exists | `PASS` | All contract DTOs are under `Assets/_Game/Scripts/TalismanBag/Contracts/Battle` and namespace `TalismanBag.Contracts.Battle`. |
| V0.3 single-cell layout maps to `Single1` | `PASS` | `V03BattleLayoutSnapshotExporter.MapItem` sets `shapeId=Single1`, `rotationIndex=0`, `anchorCell=gridPosition`, `occupiedCells=[gridPosition]`, `legacySingleCell=true`. |
| V0.3 formal grid is not modified | `PASS` | Exporter reads `BattleLoadoutSnapshot`; no formal grid/controller calls. |
| V0.4 multi-cell layout normalizes | `PASS` | `V04BattleLayoutNormalizer` maps `shapeId`, `rotationIndex`, `anchorCell`, `occupiedCells`, energy, affixes, rarity, family/base item, and stat fields. |
| BuildEvaluation player/dev split exists | `PASS` | Player summaries stay in `activeSynergies`, `readinessSummary`, `modifierBundleSummary`, `eventBundleSummary`, `playerVisibleHints`, `recommendedAction`; answer data stays in dev fields. |
| Sandbox result defaults | `PASS` | Mapper writes `devOnly=true`, `shouldWriteSave=false`, `shouldGrantReward=false`, `chapterProgressDelta=none`, empty reward claim token. |
| Formal write leak check | `PASS` | Static scan found no SaveData, PlayerPrefs, MainTrialProgressData, RewardService, DropTable, RunFlow, BossInfoPanel, scene, prefab, or BuildSettings writes in the contract/adapter layer. |

## Known Validation Limit

`where Unity` did not find a Unity executable in the current shell, so the editor menu report writer was not executed from CLI. Static source checks, `git diff --check`, and redline string scans were completed.
