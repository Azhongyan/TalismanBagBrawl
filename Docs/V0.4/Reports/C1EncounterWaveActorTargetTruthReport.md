# C1 Encounter Wave Actor Target Truth Report

- Package: `V0.4-C1EncounterWaveActorTargetTruth01`
- Guard: `GUARD_PASS_C1_ENCOUNTER_WAVE_ACTOR_TARGET_TRUTH01 / READY_FOR_DEV`
- Result: `PASS_WITH_UNITY_BATCH_STARTUP_BLOCKED`
- Schema: `V04Chapter1EncounterWaveActorTargetTruth.v2`
- Authority: `StagePlan -> WavePlan -> ActorPlan` only.

## Authoritative composition

- `1-1`: W1 `碎骨附身者 ×2`
- `1-2`: W1 `碎骨附身者 ×2 + 骨瓷犬 ×1`
- `1-3`: W1 `碎骨附身者 ×1 + 骨瓷犬 ×2`
- `1-4`: W1 `骨瓷犬 ×1 + 换骨残相 ×1`
- `1-5`: W1 `碎骨附身者 ×1 + 骨瓷犬 ×1 + 换骨残相 ×1`
- `1-6`: W1 `碎骨附身者 ×2 + 换骨残相 ×1`
- `1-7`: W1 `骨瓷犬 ×2 + 换骨残相 ×1`
- `1-8`: W1 `碎骨附身者 ×1 + 换骨残相 ×2`
- `1-9`: W1 `碎骨附身者 ×2 + 骨瓷犬 ×1`; W2 `骨瓷犬 ×1 + 换骨残相 ×2`
- `1-10`: W1 `守骨奴 only`

Totals are `10 stages / 11 waves / 29 actors`. Every Actor has a stable `waveEntryId`, `enemyContentId`, `runtimeProfileId`, `visualProfileKey`, stage-local `occurrenceOrdinal`, `displaySlotId`, and `slotOrdinal 1..3`. The complete row set is in `C1EncounterWaveActorPlan.csv`.

## Battle and target truth

- The existing `V04Chapter1NormalEnemyBattleAdapter` remains the sole ordinary Battle owner. It now owns all Actor Runtime instances in the current Wave; no second C1 Runtime or application reducer was introduced.
- Every Actor has its own `enemyInstanceId`, C1 Runtime snapshot, HP, shell, action schedule, cue stream, request dedupe set, and reset generation.
- Battle owns the only `selectedTargetEnemyInstanceId`. Default and fallback selection is the lowest live targetable `slotOrdinal`.
- `TrySelectTarget` accepts only a live targetable Actor in the current Wave. An illegal ID is rejected without destroying an existing legal selection.
- Selected Actor death immediately reselects the next lowest live slot. Final Actor death clears selection.
- Direct Item damage uses the existing `C1EnemyBattleApplicationResult` reducer path and touches the selected Actor only. Broadcast, AoE, random, and lowest-HP modes were not implemented.
- Flow advances only after every Actor in the current Wave is defeated. Intermediate 1-9 W1 completion reaches only W2; only W2 completion can produce the 1-9 stage result and then 1-10 BossGate.

## #3 HOLD and Boss boundary

- `换骨残相` Runtime remains `HELD_BY_BA-D3`; `d1_3` is not admitted as Runtime.
- Any Wave containing #3 is preflighted as a whole before Actor creation and fails with `RUNTIME_PROFILE_MISSING_HELD_BY_BA-D3`.
- Held-Wave verification observed `activeActors=0`, no partial Wave, no substitute profile, no merged HP, and no pollution of completed facts.
- 1-10 remains behind BossGate plus accepted ManualChallenge. Before acceptance there is no Boss request/session; after acceptance only one correlated Boss request is admitted. Existing Shougunu Battle owner was not replaced.

## Old contract retirement

The withdrawn `V0.4-C1StageWaveEncounterTruth01` actor-as-sequential-wave contract is no longer authoritative. Its `entries` API is retained only as a read-only flattened view derived from the v2 Stage/Wave/Actor plans so older probes can compile; Flow progression uses `CurrentWavePlan` and `waveId`, never flattened Actor order.

## Verification

- Pure truth and flow/Battle verifier: `PASS`.
- Assertions covered exact 10/11/29 truth, unique IDs/slots/ordinals, two independent 1-1 Hosts, selected-only damage, explicit Slot02 targeting, illegal-ID preservation, death fallback, selection clear, independent 1-2 three-Actor state, #3 whole-Wave fail-close, 1-9 W1/W2 boundary, BossGate/ManualChallenge correlation, and two reset/restart generations.
- Scoped runtime C# compilation using the current dirty source set: `PASS` with one unrelated existing warning.
- Scoped Editor verifier compilation: `PASS`.
- Unity batch: `BLOCKED_AT_STARTUP`; Unity created no requested log and stayed idle for more than one minute, so it was stopped and is not claimed as a Unity verifier pass.
- Scene / Prefab / BuildSettings / SceneBinder / UI / Visual / VFX / V02 / V03 / Item algorithm diff: `0`.
