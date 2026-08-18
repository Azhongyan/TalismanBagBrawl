# V0.4-ItemFourCoreAuthorityAndMigrationDecision01 Report

Status: `PASS / USER_OPTION_C_FROZEN / REPORT_ONLY`

Package: `V0.4-ItemFourCoreAuthorityAndMigrationDecision01`

Freeze date: `2026-07-23 Asia/Shanghai`

Primary Guard: `Item System Guard`

Joint review: `Item Algorithm Guard`

## Receipts

```text
ITEM_GUARD_PASS_ITEMFOURCOREAUTHORITYANDMIGRATIONDECISION01
ITEM_ALGORITHM_GUARD_PASS_ITEMFOURCOREAUTHORITYANDMIGRATIONDECISION01
```

## Authoritative four-core rule

For every ordinary base item `I001-I030`:

- There are exactly four fixed progressive core effects.
- The effects are stable identities owned by the base item. They are never randomly selected,
  rerolled, replaced or weighted.
- Canonical identities are `01`, `02`, `03`, and `ULT`.
- Cultivation unlock thresholds are Lv10, Lv20, Lv30 and Lv40.
- Runtime activation requires all three facts: rarity eligible, cultivation unlocked and item lit.
- The authored Item Detail capacity remains four rows and four PNG icons. There is no fifth slot.
- `I031` remains a special system item and has no ordinary core-effect identity row.

## Frozen rarity eligibility: Option C

| Rarity | Eligible fixed effects | Count |
|---|---|---:|
| White / Fan | Core1 | 1 |
| Green / Liang | Core1, Core2 | 2 |
| Blue / Ling | Core1, Core2, Core3 | 3 |
| Purple / Xuan | Core1, Core2, Core3 | 3 |
| Orange / Dao | Core1, Core2, Core3, Ultimate | 4 |

Purple deliberately adds no new core identity. It may still improve stats, affixes and other
rarity-owned facts, but those systems are outside this decision package.

## Identity and migration decision

```text
candidate_core_ixxx_01       -> Ixxx_CORE_01  -> KEEP_CANONICAL
candidate_core_ixxx_02       -> Ixxx_CORE_02  -> KEEP_CANONICAL
candidate_core_ixxx_03       -> Ixxx_CORE_03  -> KEEP_CANONICAL
candidate_core_ixxx_04       -> Ixxx_CORE_04  -> SUPERSEDED_EXPANSION_DRIFT
candidate_core_ixxx_ultimate -> Ixxx_CORE_ULT -> KEEP_CANONICAL
```

The old `_04` candidate and Awakening identities are retained as historical evidence but are
not formal effects. They must not be merged into Ultimate and their payload must not be used to
recalculate Ultimate. Existing Ultimate identity, text, payload and Lv40/Orange semantics remain.

## Version and historical-evidence ruling

The following accepted packages contain a later five-core expansion that conflicts with the
user's frozen four-core design:

```text
V0.4-ItemCoreAwakeningCore4NodeExpansion01
V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01
```

Their Assignments, reports, matrices, hashes and QA evidence remain preserved. Their five-core
semantic authority is marked:

```text
CONFLICTING_WITH_FROZEN_FOUR_CORE_DESIGN
SUPERSEDED_IN_CORE_COUNT_AND_IDENTITY_SET
HISTORICAL_EVIDENCE_RETAINED
```

No historical artifact may be rewritten to imply that the five-core expansion never occurred.
Corrected Candidate data, Awakening/runtime contracts and detail projection must use new
versioned correction packages and migration ledgers.

## Package split

```text
P0 V0.4-ItemFourCoreAuthorityAndMigrationDecision01
   COMPLETE / REPORT_ONLY

P1 V0.4-ItemFourCoreCandidateDataCorrection01
   READY_FOR_ASSIGNMENT

P2 V0.4-ItemFourCoreAwakeningRuntimeContractCorrection01
   NOT_STARTED / BLOCKED_UNTIL_P1_QA_ACCEPTANCE

P3 V0.4-ItemFourCoreProjectionDetailAndBaselineMigration01
   NOT_STARTED / BLOCKED_UNTIL_P2_QA_ACCEPTANCE
```

Item Detail qualified core projection and Prefab migration remain on hold. P1 completion alone
does not make the runtime or detail chain four-core complete.

## Protected design boundaries

- Ordinary rarity instances remain `30 x 5 = 150`.
- Corrected core identities become `30 x 4 = 120`.
- Core identity uses no RNG stream and has no reroll.
- Stat, affix, drop and BuildQualification rules and probabilities are unchanged.
- Item UI, Scene, Prefab, RectTransform, fonts, icons and PNGs are unchanged.
- No Battle, RunFlow, SaveData, Reward, Chapter, Boss, Enemy or Capability integration is added.
- No commit, tag, push, reset or rollback is authorized.

## Review result

The Option C rule is internally consistent with the existing rule that Ultimate is Orange-only.
P1 may correct Candidate data without changing the core-potential schema source. P2 must later
replace the five-row Awakening/runtime authority with an explicitly versioned four-row contract.

ModifiedFiles during review: `NONE`

Unity / Builder / migration run during review: `NONE`

