# I031 Nian Generation Battle Resource Vertical Slice Report

- Package: `V0.4-I031NianGenerationBattleResourceVerticalSlice01`
- Phase: `LIGHT_FIX_CODE_COMPLETE / USER_HANDTEST_WAITING`
- Scope: `devOnly Item/Battle resource authority + Shougunu P3 runtime/UI projection; no Scene/formal-flow writes`
- Verification: `25/25 PASS`
- Runtime QA: `full Unity runtime/editor compile PASS; offline I031 25/25 PASS; current-revision normal Editor Play smoke PENDING`
- Source signature: `bfc708b1fef71eee7d3271bfcd57b050d80d03ae2ceb83158cecbdb54edcf62f`
- 50-pulse snapshot: `7f39d00a4f202e3186357b6f4a3fc89a64e496245e40911b24e4a428a25ab367`

| ID | Check | Result | Evidence |
|---|---|---:|---|
| SOURCE-01 | ItemSystemSnapshot.v2 authority | PASS | bfc708b1fef71eee7d3271bfcd57b050d80d03ae2ceb83158cecbdb54edcf62f |
| SOURCE-02 | Lv40 Orange machine cost facts | PASS | 2,3,5,3,3,6 |
| SOURCE-03 | Immutable source collection | PASS | ReadOnlyCollection mutation rejected |
| SOURCE-04 | Inventory state cannot generate | PASS | I031_NOT_ON_BOARD\|I031_PLACEMENT_COUNT_NOT_ONE |
| BATTLE-01 | Initial authoritative resource snapshot | PASS | 962d1c355ad374a0c441bd2df09deb572c27f23772e863cc9508450619711f6d |
| BATTLE-02 | 50 pulse frozen fixture | PASS | initial=20; generated=200; spent=181; final=39 |
| BATTLE-03 | Accepted applications preserve 50 damage gates | PASS | acceptedApplications=50; durationTicks=75000 |
| BATTLE-04 | Pulse order generation then spend | PASS | ledgerRows=100 |
| BATTLE-05 | Insufficient balance rejects damage authorization | PASS | INSUFFICIENT_NIAN |
| BATTLE-06 | Signature validation precedes generation | PASS | SOURCE_SIGNATURE_MISMATCH |
| BATTLE-07 | Duplicate pulse is idempotently rejected | PASS | DUPLICATE_PULSE_EVENT |
| BATTLE-08 | Cap is enforced and recorded | PASS | final=96 |
| RESET-01 | Reset restores 20 and clears old ledger/events | PASS | 7023e6bdcee262438107e8d092512d2d2fbd871e4306c131ec676e82000f6b09 |
| RESET-02 | Old generation cannot leak after reset | PASS | GENERATION_MISMATCH |
| DETERMINISM-01 | Same source and pulses are deterministic | PASS | 7f39d00a4f202e3186357b6f4a3fc89a64e496245e40911b24e4a428a25ab367 |
| BOUNDARY-01 | No forbidden runtime coupling in owned sources | PASS | leakCount=0 |
| INTEGRATION-01 | P3 pulse is gated by shared Nian identity | PASS | shared battle.request.g identity; Nian before session pulse |
| INTEGRATION-02 | Existing Nian UI is read-only runtime projection | PASS | ManaText/Fill + generated/spent authored TMP clones; authored state restored |
| INTEGRATION-03 | Legacy loops remain suppressed without Scene writes | PASS | legacy loops forced disabled; Scene serialization untouched |
| INTEGRATION-04 | Real Projection damage is not a frozen launch gate | PASS | I008 real Projection 40 at adjacency 2/3 starts without fabricating 42 |
| INTEGRATION-05 | Authored TMP source and settlement cues share one event | PASS | exact MFLangSongJianYuan-Regular SDF; authored asset is static ASCII-only (96 glyphs/no fallback), runtime exact-OTF dynamic fallback; source=resolvedPreMitigationDamageUnits; enemy=shell/hp applied |
| INTEGRATION-06 | Accepted-only board activation VFX bypasses legacy truth | PASS | accepted Nian + matching ItemApplication ledger only; direct old Play path restores source pulse/flash/resource-sequence/procedural VFX, then explicit FIRE_PROJECTILE/SWORD_QI_SLASH/HEAVY_SEAL_DROP carrier + impact; card artwork never transfers; legacy Text suppressed |
| PRESENTATION-01 | I007-I012 have six stable source-number gradients | PASS | I007=FFF4A5FF-FF4123FF;I008=FFFBB5FF-FF8718FF;I009=FFFFF6FF-E81F2CFF;I010=FFE884FF-15CDD8FF;I011=FFD06FFF-B02FD8FF;I012=FFFDC4FF-FF2C18FF |
| PRESENTATION-02 | HP/SH/BREAK and NP deltas have distinct result semantics | PASS | presentation.nian.generated=FFF489FF-1BDEB4FF;presentation.nian.spent=FFB63DFF-B02889FF;presentation.settlement.hp=FFEEE1FF-99181FFF;presentation.settlement.shell=FFFAAEFF-DA7E18FF;presentation.settlement.break=FFF7E0FF-FFC631FF;taibaiReservedOnly=True |
| PRESENTATION-03 | Accepted VFX uses three explicit skill carriers, never card artwork | PASS | I007=carrier.fire_projectile;I008=carrier.sword_qi_slash;I009=carrier.heavy_seal_drop;I010=carrier.fire_projectile;I011=carrier.sword_qi_slash;I012=carrier.heavy_seal_drop;taibaiFamilyReserved=carrier.family.taibai.reserved |

## LIGHT_FIX diagnosis

- Live first rejection: `TERMINAL_ITEM_CHAIN_NOT_READY`; `I008 damage=40`, adjacency `2/3`, while the stale P3 launch gate required the frozen `42` fixture.
- The authored Continue Battle button, current binder, and Item request assembly were already reached; rejection occurred before I031 Nian source/session creation.
- P3 now retains row/cardinality/positive-damage and exact I031 source checks, while consuming each current authoritative Projection magnitude without fabricating `I008=42`.

## Presentation LIGHT_FIX

- Exact old VFX owner: `BattleSandboxRuntimeLoopRuntime.ApplyCurrentRow -> BattleSandboxItemTriggerFeedbackController.Play`. Disabling the legacy runtime stopped `Play`, and `ResetLoop/OnDisable` cleared its transient effects.
- Exact old source-activation path is now reused for accepted events: `PulseTarget -> SpawnFlashOnTarget/AtAnchor -> TryPlayResourceSequenceVfx -> SpawnSkillVfx`; its legacy `SpawnFloatingText` is intentionally suppressed because the correlated authored-TMP number is the sole numeric presentation.
- Card artwork never leaves its board cell. After source activation, a presentation-only explicit table maps I007/I010 to `FIRE_PROJECTILE`, I008/I011 to `SWORD_QI_SLASH`, and I009/I012 to `HEAVY_SEAL_DROP`; each independent carrier owns travel/drop and impact visuals, is deduped by the accepted event ID, and is reliably destroyed/reset. TaiBai only reserves a separate carrier-family key.
- The legacy damage/mana loops remain disabled. The facade only translates the accepted event's `itemInstanceId/baseItemId/placementId/occupiedCells/eventId` into the old presentation request; duplicate/stale/rejected/zero events never enter it, and every transient is cleared.
- Both authored templates use the exact `MFLangSongJianYuan-Regular SDF` FontAsset and shared material. The asset is static (`AtlasPopulationMode=Static`), contains exactly 96 ASCII/control glyphs, and has no fallback; that was the first missing layer for every Chinese clone.
- The exact runtime rejection layer was TMP 3.0.9 `TryAddCharacters`: after the prewarm had already inserted a glyph, a repeated request had no new glyph work and returned `false` with the whole request echoed as `missingCharacters`. The helper treated that as real font absence and rejected otherwise valid Chinese. It now checks `HasCharacter` before and after insertion, so prewarmed glyphs are accepted while genuinely absent glyphs still reject explicitly.
- The source literals and compiled assembly contain proper Chinese; earlier shell output was the UTF-8 Editor log decoded through the local ANSI code page. The touched Chinese sources remain UTF-8 BOM as an explicit encoding guard.
- Runtime keeps the authored FontAsset/material on each clone, attaches a DontSave dynamic fallback built from the asset's exact linked `MFLangSongJianYuan-Regular.otf` GUID, prewarms required Chinese glyphs, forces mesh generation, and restores the original empty fallback table on reset/disable. Neither template nor FontAsset/Scene bytes are saved.
- I007-I012 source numbers use six stable presentation-only vertex gradients (朱红金、橙金、赤红白、青焰金、紫红金、亮金赤) keyed by `baseItemId`. Enemy settlement independently uses result semantics: HP deep cinnabar to warm white (`-N HP`), Shell amber to gold (`-N SH`), and break gold to warm white (`BREAK`).
- Accepted resource ledger deltas project as `+4 NP` at I031 in cyan-green/gold and exact `-2/-3/-5/-3/-3/-6 NP` at the actual trigger in purple-red/orange-gold. They never reuse HP red and never own resource truth.
- One pulse event produces at most one source number, one VFX carrier chain, one enemy settlement, one generated NP cue, one spent NP cue, and one break cue. Source number is the accepted request row's `resolvedPreMitigationDamageUnits`; enemy text is the matching damage ledger's `shellDamageApplied` or `hpDamageApplied`.
- Floating timing is `1.65s`: pop/hold, slow rise, then fade, with three-lane staggering for consecutive events.

## Frozen fixture

- `resourceKey=nian`, `max=100`, `initial/reset=20`, `generation=4`, `pulse=1500 ticks`.
- I007-I012 costs: `2,3,5,3,3,6` from immutable Item-owned request facts.
- 50 pulses: `generated=200`, `spent=181`, `accepted=50`, `final=39`, `duration=75000 ticks`.
- Frozen I008=42 remains an exact-fixture assertion only; runtime launch consumes the current authoritative Projection (including I008=40 at 2/3 adjacency).
- P3 uses the same resetGeneration, tick, requestId and `battle.request.g...` pulse identity; damage advances only after an accepted Nian application.
- Scene serialization remains unchanged; existing ManaText/Fill, authored anchors, and authored TMP templates are resolved at runtime.
