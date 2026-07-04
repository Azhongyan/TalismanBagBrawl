# Item Identity Unification Report

Package: `V0.4-ItemIdentityUnification01`

Status: `REPORT_ONLY / DEVONLY_SAFE`

## Summary

This package establishes a devOnly identity map for the current V0.4 BuildSandbox `preview_*` item ids.

No formal item id was replaced. No V0.2/V0.3 formal asset, SaveData, Reward, RunFlow, scene UI, or `Scene_TalismanBag_V04_BattleSandboxPreview` layout was changed.

## Boundary

Allowed scope:

- BuildSandbox devOnly identity documentation.
- Report outputs under `Docs/V0.4/Reports/**`.
- Mapping temporary `preview_*` ids to long-term candidate or existing formal item ids.

Explicitly not done:

- Did not replace runtime `preview_*` usages.
- Did not edit formal V0.2/V0.3 item definitions.
- Did not edit SaveData, PlayerPrefs, MainTrialProgressData, rewards, drop tables, RunFlow, Boss, combat values, or scenes.
- Did not promote any V0.4 candidate into formal gameplay.

## Identity Classes

| Class | Meaning | Formal Entry Policy |
| --- | --- | --- |
| `v02_v03_legacy_continuation` | Existing stable item identity already used by V0.2/V0.3 formal scripts. | May be referenced as the long-term identity, but this package does not change the existing item. |
| `v04_formal_candidate` | Long-term candidate item id for a future V0.4+ contentization package. | Must remain devOnly/candidate until a separate promote package approves data, rewards, save, UI, and balance. |
| `pure_preview_placeholder` | Temporary source id used by BuildSandbox scenes, reports, runtime fixtures, or shape previews. | Must not enter formal rewards, save data, formal UI, or official item tables. |

## Mapping

| Preview Id | Display Name | Source Class | Target Item Id | Target Class | Related Legacy Id | Policy |
| --- | --- | --- | --- | --- | --- | --- |
| `preview_fire_talisman` | 炽火符 | `pure_preview_placeholder` | `fire_talisman_basic` | `v02_v03_legacy_continuation` | `fire_talisman_basic` | Use existing formal identity only as reference; do not replace or rewrite. |
| `preview_thunder_sword` | 雷引剑符 | `pure_preview_placeholder` | `thunder_sword_talisman` | `v04_formal_candidate` | `thunder_talisman_basic`, `chain_thunder_talisman_basic` | Candidate in thunder talisman series. |
| `preview_x2_wood_talisman` | 护阵木牌 | `pure_preview_placeholder` | `guardian_wood_talisman` | `v04_formal_candidate` |  | Remove shape wording from identity. |
| `preview_guard_wood` | 守护木牌 | `pure_preview_placeholder` | `guard_wood_artifact` | `v04_formal_candidate` |  | Keep separate from talisman candidate until formal design decides. |
| `preview_cleanse_corner` | 净化折符 | `pure_preview_placeholder` | `purify_talisman_corner` | `v04_formal_candidate` |  | Shape remains placement data. |
| `preview_stone_core` | 炉芯石 | `pure_preview_placeholder` | `furnace_core_stone` | `v04_formal_candidate` |  | Material/core candidate only. |
| `preview_energy_incense` | 聚能香 | `pure_preview_placeholder` | `awakening_incense` | `v04_formal_candidate` |  | Candidate consumable; no formal consumable flow enabled. |
| `preview_old_bell` | 镇邪铃 | `pure_preview_placeholder` | `exorcism_bell_basic` | `v02_v03_legacy_continuation` | `exorcism_bell_basic` | Existing bell identity remains authoritative; `old_exorcism_bell` is an alias candidate only. |
| `preview_soul_seal` | 镇魂法印 | `pure_preview_placeholder` | `soul_suppress_seal` | `v04_formal_candidate` |  | Heavy control/defense candidate only. |
| `preview_taomu_sword` | 桃木剑 | `pure_preview_placeholder` | `peach_wood_sword` | `v04_formal_candidate` | `peach_wood_basic` | Related to existing peach wood line, but not the same formal item. |

## Existing Formal Identity Findings

Confirmed existing formal V0.2/V0.3 identities in scripts:

- `fire_talisman_basic`
- `thunder_talisman_basic`
- `chain_thunder_talisman_basic`
- `exorcism_bell_basic`
- `peach_wood_basic`

Candidate ids not currently found as formal item ids in `Assets/_Game/Scripts`:

- `thunder_sword_talisman`
- `guardian_wood_talisman`
- `guard_wood_artifact`
- `purify_talisman_corner`
- `furnace_core_stone`
- `awakening_incense`
- `old_exorcism_bell`
- `soul_suppress_seal`
- `peach_wood_sword`

## Promotion Rules

Future formal promotion must be a separate package. It must not simply replace `preview_*` strings in-place.

Required future gates:

1. Formal item config design and naming approval.
2. Explicit devOnly to formal promotion plan.
3. Save/reward/drop/upgrade impact review.
4. V0.2/V0.3 golden path regression.
5. Separate leak check proving no temporary preview id is exposed to formal player flow.

## Output Files

- `Docs/V0.4/Reports/ItemIdentityMapping.csv`
- `Docs/V0.4/Reports/ItemIdentityUnificationReport.md`
- `Docs/V0.4/Reports/ItemIdentityLeakCheckReport.md`

