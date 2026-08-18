# CANONICAL LV1 POWER BAND AND EARLY STAGE CURVE AUDIT

Date: 2026-08-12  
Mode: READ_ONLY_FIRST / ANALYSIS_ONLY  
Product context: CAMPAIGN_NORMAL_LV1  
Result: AUDIT_COMPLETE / STRATEGY_DECISION_REQUIRED / NO_PRODUCT_CHANGE / STOP

## Decision supported

Decide whether the current formal 1-1～1-5 enemy durability curve is compatible with the current Canonical Item database, initial acquisition policy, I031 Nian budget and real Battle cadence, and define a bounded Enemy-only PLAYTEST_V1 correction range before implementation.

## Player evidence

- Normal route rewards and placement work.
- The player entered 1-2 with the initial 离火符 plus a rewarded 离火符 and could not clear because enemy durability was too high.
- This is the current player-visible first blocker. The earlier reward feeder and state-sink internal proof does not override it.

## Authoritative executed inputs

- `CanonicalInitialItemAcquisitionPolicy` selects the lowest-power single-cell white damage Item. In the current unique database this is I007 离火符, not historical I001.
- I007 white: damage 8～12, Nian cost 4～5, cooldown 5～6 canonical units. The formal adapter executes cooldown units at 400 ms each, so cadence is 2.0～2.4 seconds.
- I007 applies Afterglow on action commit. Tick potency is 10% of source resolved damage with a minimum of 1, up to three contributions on the current target.
- I031 starts at 27/27 Nian and generates 1 Nian every 500 ms. Multiple lit Items share this resource; adding an Item does not multiply sustained throughput linearly.
- The direct reward policy uses the single I001～I030 Canonical database. Stage-1 rarity weight is white 70, green 24, blue 5, purple 0.9, orange 0.1. I031 is not an ordinary drop.
- Player max HP is 100. The 1-2 enemy wave deals 8 every 4.5 seconds; without Guard/Heal/Control the thirteenth hit defeats the player at 58.5 seconds.
- Formal enemy durability currently resolves as:

| Stage | Raw HP | Shell | Effective durability before mechanics | Enemy wave |
|---|---:|---:|---:|---|
| 1-1 | 48 | 0 | 48 | 8 / 4.5 s |
| 1-2 | 1920 | 0 | 1920 | 8 / 4.5 s |
| 1-3 | 1700 | 200 | 1900 | 7 / 4.5 s |
| 1-4 | 1650 | 100 | 1750 + Bone Swap | 7 / 4.5 s |
| 1-5 | 1800 | 100 | 1900 + Bone Swap | 7 / 4.5 s |

## Historical source of the mismatch

The existing 1-2～1-5 durability values were authored against an older Campaign assumption in which the reference output was I001 at 82 direct damage every 2 seconds, with later Stage 3～5 reports using p50 82 and p90 110 every 2 seconds. That assumption is no longer the formal Item feeder.

Current two-white-I007 output before the unprotected 58.5-second defeat is approximately 382～575 total damage, median about 473. The old reference output was about 41 direct DPS; the current median handtest Build is about 8.1 total DPS in the bounded analysis. Retaining 1920 HP therefore requires output that the current legal early Build does not own.

Historical Pool15/Starter reports remain historical evidence only. They must not be restored as runtime authority and must not be used to raise current Item stats back to 82/2 s.

## Analysis-only discrete traces

The trace follows current event ordering: Nian generation, Item action, status tick/expiry, then enemy wave. It enumerates legal white I007 damage/cost/cooldown ranges and does not include unlocked Core effects or invented affix gains.

### 1-1 current value

- One white I007 versus 48 total HP: all 20 stat combinations win.
- TTK range: 8.0～14.4 seconds.
- Player HP on victory: 76～92.
- Conclusion: 1-1 is a valid short onboarding encounter and is not the current balance blocker.

### 1-2 current player Build

- Initial white I007 plus rewarded white I007 versus 1920 HP: 0 / 400 combinations win.
- Damage before defeat: 382～575; median about 473.
- Even an orange rewarded I007 cannot reach 1920 before the same unprotected defeat in this bounded trace.
- Required average output is 32.8 DPS; the actual white duplicate-I007 band is about 6.5～9.8 DPS.

### 1-2 HP sensitivity for the reported duplicate-I007 Build

| Total HP | Result | Median TTK | Median player HP |
|---:|---|---:|---:|
| 256 (-20% around 320) | 400 / 400 win | 33.0 s | 44 |
| 280 | 400 / 400 win | 36.0 s | 44 |
| 320 | 400 / 400 win | 42.0 s | 28 |
| 384 (+20%) | 358 / 400 win | 50.2 s among wins | 12 among wins |

One initial I007 without reward damage also clears 320 HP in every legal white stat combination, but its TTK reaches 42.0～57.8 seconds. Therefore 320 is a supported upper edge for a stage that should remain clearable after a weak or non-damage first reward; 384 is already outside that robust band.

### 1-3 reported cumulative Build reference

Using two white I007 plus a white I003 震雷破壳印, including I003's real 16～23 Break and two 100-shell Hounds:

| 1-3 raw HP | Effective durability | Win rate | Median TTK | Median player HP |
|---:|---:|---:|---:|---:|
| 160 | 360 | 100% | 44.0 s | 37 |
| 200 | 400 | 99.8% | 50.0 s | 23 |
| 240 | 440 | 95.6% | 57.6 s | 16 |
| 280 | 480 | 78.6% | 61.0 s among wins | 9 among wins |
| 320 | 520 | 49.4% | 62.0 s among wins | 9 among wins |

Current 1-3 effective durability is 1900, so it is not a continuation of the current Canonical early power band.

## Bounded recommendation

Keep 1-1 at 48. Do not buff the Item database to rescue obsolete enemy values. Correct the existing Enemy rows in one later, separately authorized Enemy-owned package.

Recommended PLAYTEST_V1 durability bands:

| Stage | Recommended raw HP | Existing shell/mechanic retained | Effective intent |
|---|---:|---|---|
| 1-1 | 48 unchanged | none | 8～15 s onboarding |
| 1-2 | 280～320 | none | first reward visibly helps; weak path still clearable |
| 1-3 | 200～240 | two existing 100 shells | 400～440 effective; Break gains visible value |
| 1-4 | 340～400 | one existing 100 shell + Bone Swap | 440～500 effective plus mechanic pressure |
| 1-5 | 420～500 | one existing 100 shell + Bone Swap | 520～600 effective plus final mechanic pressure |

The 1-2 and 1-3 bands are directly supported by enumerated traces. The 1-4 and 1-5 bands are bounded candidates, not final values; before writing them, the same package must run the current Battle Session against legal cumulative four- and five-Item reference layouts and choose inside these bands. No new simulator or second Battle framework is authorized.

## Minimal later implementation, if approved

1. Modify only the existing 1-2 early Enemy profile rows and existing 1-3～1-5 PLAYTEST_V1 Enemy rows, plus their existing exact-value validators.
2. Keep rosters, Stage assets, attack cadence, I031, Item stats, Reward policy, Battle operators, Shell, Bone Swap and presentation unchanged.
3. Use one current Battle focused verifier to prove weak/middle/intended cumulative layouts; do not revive historical I001/Pool15 fixtures.
4. Run one normal player route only after internal curve traces pass.

## Prohibited responses

- Do not restore or write a second Pool15/Starter database.
- Do not write I007 or any reward identity into progression logic.
- Do not buff all Items to match 1920 HP.
- Do not alter Nian, Reward, Battle, Scene, Prefab, Presentation or Save in the Enemy curve package.
- Do not call historical compile or fixture evidence a player-ready result.

## Stop rule

This audit stops without product edits. Implementation requires explicit authorization of one Enemy-only PLAYTEST_V1 curve package. If current Battle traces cannot produce legal four- and five-Item wins inside the proposed bands, stop and name the first actual Item/Nian/Battle semantic conflict instead of widening the package.
