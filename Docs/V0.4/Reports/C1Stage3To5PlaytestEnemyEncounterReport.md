# C1 Stage 3 To 5 Playtest Enemy Encounter Report

- Package: `V0.4-C1Stage3To5PlaytestEnemyEncounterConfig01`
- Result: `PACKAGE_COMPLETE / AUTOMATED_QA_PASS`
- Product context: `CAMPAIGN_NORMAL_LV1`
- Candidate: `PLAYTEST_V1_NOT_FINAL_BALANCE`
- Activation: `PLAYTEST_V1_RELEASED_FOR_FORMAL_CONSUMPTION`
- Balance profile: `campaign.normal.lv1.balance.c1.stage3to5.playtest.v1`
- Catalog canonical: `79d5e5d0b3cb18f0e5a854dbba1327b1ad7948702ff3c581b0e67eebd948058c`
- Item facts canonical (read-only): `dde664f848057436b3dd56225ca715dcaa5a0f132d86869af4366a4e07a36a3c`
- Item envelope snapshot canonical (read-only): `b73fc79a7694832bce734973c6f57e0b6c891549dbbecca3a8fd88c7a0a06eb1`
- Item intended-five envelope canonical (read-only): `2b25831a91b7eec5582b9ccb5d5fc2be01e028f069c424629e63ec467a738128`
- Item trace canonical (read-only): `d1b5c9707204cedc1f9419faab606f53ae421394a0829eefd8e1e208404a54eb`
- Item trace event count: `6`
- Item p50/p90 direct pulse: `82 / 110` every `2000ms`
- PlayerVisibleDelivery: `NONE`
- MilestoneCompletionEvidence: `NO`
- FreshPlay: `NOT_APPLICABLE`
- Unity: `NOT_STARTED`
- GitOperations: `NONE`
- NEXT_PACKAGE: `NOT_STARTED`

## Released sidecar

| Stage | Variant | Roster HP | Basic cadence | Flags |
|---|---|---:|---|---|
| 1-3 | `campaign.normal.lv1.encounter.c1.1-3.playtest.v1` | 1700 | 7 at 4.5s / 4.5s | enabled, formal, Battle-unbound |
| 1-4 | `campaign.normal.lv1.encounter.c1.1-4.playtest.v1` | 1650 | 7 at 4.5s / 4.5s | enabled, formal, Battle-unbound |
| 1-5 | `campaign.normal.lv1.encounter.c1.1-5.playtest.v1` | 1800 | 7 at 4.5s / 4.5s | enabled, formal, Battle-unbound |

Bone Swap release mapping is `25% / cap 7 / telegraph 1500ms / recovery 18000ms / LATEST_WINS_ONE_SLOT`. It records only accepted player-resolved single-target direct-damage numeric pulses and does not copy Item identity, code, cooldown, state, target, chain, proc, or internal behavior.

## Baseline trace summaries

| Stage / input | TTK | Enemy basics | Bone returns | Player HP |
|---|---:|---:|---:|---:|
| 1-3 / p50 direct 82 every 2s | 42s | 9 | 0 | 37 |
| 1-3 / p90 direct 110 every 2s | 32s | 7 | 0 | 51 |
| 1-4 / p50 direct 82 every 2s | 42s | 9 | 2 | 23 |
| 1-4 / p90 direct 110 every 2s | 30s | 6 | 2 | 44 |
| 1-5 / p50 direct 82 every 2s | 44s | 9 | 3 | 16 |
| 1-5 / p90 direct 110 every 2s | 34s | 7 | 2 | 37 |

## Deterministic verification

- Profiles / actors: `3 / 8`.
- Sensitivity rows: `27` (`-20% / baseline / +20%`, one variable at a time).
- Same-tick rule: `PLAYER_THEN_ENEMY`.
- Baseline victory: `PASS`.
- Monotonic TTK and incoming-damage directions: `PASS`.
- Unsupported/Unknown Item fact to zero conversion: `REJECTED`.
- Fabricated Item event: `NONE`.
- Animation completion gameplay dependency: `NONE`.
- Repeat generation: `BYTE_IDENTICAL`.
- Tuning status: `PLAYTEST_V1_NOT_FINAL_BALANCE`.

## Artifact content hashes

- Spec content: `20d6aaa12ea1d5d00b39a3bdf89e190d6b268197fba093916d36a836ab0a4af8`
- Trace content: `25d0638c11091b54589fe6eca39142fa9dc7417541e508636d712bcdbed3f858`
- Sensitivity content: `e952f30a8bfc755a3facaa2ee11d84c12c8ebe5092da01e92640658eb8cb33bf`

