# C1 Stage Wave Encounter Truth Report

> `SUPERSEDED / NOT AUTHORITATIVE`: this report belongs to the user-withdrawn `V0.4-C1StageWaveEncounterTruth01` sequential actor-as-wave contract. The only current authority is `V0.4-C1EncounterWaveActorTargetTruth01`, documented in `C1EncounterWaveActorTargetTruthReport.md`.

- Package: `V0.4-C1StageWaveEncounterTruth01`
- Schema: `V04Chapter1StageWaveEncounterTruth.v1`
- Scope: Chapter 1 ordered Wave/Encounter truth and existing Flow/Admission ownership only.
- Truth validation: `PASS` (`stages=10`, `entries=29`, stable IDs unique).
- Flow validation: `PASS` (`stageRequests=9`, `traceStages=10`, `bossGates=1`, `manualBossChallenges=1`).
- `occurrenceOrdinal`: one-based within each `stageId + enemyContentId`.

## Frozen plans

- `1-1`: Shattered Host ×2
- `1-2`: Shattered Host ×2 → Porcelain Hound ×1
- `1-3`: Shattered Host ×1 → Porcelain Hound ×2
- `1-4`: Porcelain Hound ×1 → Bone Swap Remnant ×1
- `1-5`: Shattered Host ×1 → Porcelain Hound ×1 → Bone Swap Remnant ×1
- `1-6`: Shattered Host ×2 → Bone Swap Remnant ×1
- `1-7`: Porcelain Hound ×2 → Bone Swap Remnant ×1
- `1-8`: Shattered Host ×1 → Bone Swap Remnant ×2
- `1-9`: Shattered Host ×2 → Porcelain Hound ×2 → Bone Swap Remnant ×2
- `1-10`: Shougunu only

## State ownership checks

- The existing `V04Chapter1ContinuousFlowSession` owns current wave, completed wave IDs, and monotonic wave reset generation.
- A stage keeps one reducer Battle request. Later waves receive a projection with the same `requestId`, `roundId`, stage, and seed, but their exact runtime profile.
- Intermediate wave defeat leaves the reducer in `BattleRunning`, does not add a completed stage, and advances only the wave pointer.
- Final wave defeat is the only state that permits a stage result and Settlement.
- Shattered Host and Porcelain Hound fresh generations start with full HP; Porcelain Hound also starts with full shell.
- Two reset/restart cycles clear current wave, completed wave IDs, and active request; new reset generations remain greater than stale generations.
- After 1-9, Flow reaches 1-10 `BossGate` with no Boss request. Shougunu starts only after the existing ManualChallenge path.

## BA-D3 HOLD

- Bone Swap Remnant content: `bone_aspect_enemy_c1_03_bone_swap_remnant`
- Runtime hold identity: `HELD_BY_BA-D3`
- Runtime resolution diagnostic: `RUNTIME_PROFILE_MISSING_HELD_BY_BA-D3`
- Visual identity contract: `enemy.bone_aspect.c1.bone_swap_remnant`
- Existing runtime profile rows: `0`
- At the held entry, Admission is `NONE`; the stage request and prior completed wave facts remain intact.
- No Shattered Host/Porcelain Hound runtime, merged HP, or `d1_3` fallback is used.

## Verification execution

- Unity-generated Roslyn response compile:
  - `Assembly-CSharp`: `PASS` (one unrelated existing `CS0414` warning).
  - `Assembly-CSharp-Editor`: `PASS`.
- Standalone invocation of the compiled truth and Flow validators: `PASS`.
- Full Unity `-executeMethod` batch: `BLOCKED`. The first attempt reached compilation and exposed two package-local return-type errors, which were fixed. Subsequent clean retries remained idle in the Unity startup/licensing chain before creating an AssetDatabase lock or log, so no full batch PASS is claimed.

No Presentation/UI/Scene/Prefab/BuildSettings/VFX/Save/Reward/Drop/Git history mutation is part of this package.
