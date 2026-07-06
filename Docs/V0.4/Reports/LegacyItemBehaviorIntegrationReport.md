# Legacy Item Behavior Integration Report

Package: `V0.4-LegacyItemBehaviorIntegration01`
Guard Pass: `GUARD_PASS_LEGACY_ITEM_BEHAVIOR_INTEGRATION01`
Status: `PASS_STATIC / UNITY_BATCH_BLOCKED`
Leak Count: `0`

## Scope

- BuildSandbox devOnly integration only; no formal itemId, drop, reward, backpack, save, Boss, scene, prefab, RectTransform, or BuildSettings writes.
- Runtime behavior reads `BuildSandboxPlacedItemSnapshot.energyState` and reuses `FormationEnergyContract`.
- `None` samples are dormant, `WeakPulse` samples use 45% efficiency, `Powered` samples use full base behavior and allow affix/synergy bonuses, `Suppressed` samples are blocked.
- Unity batch report generation was attempted but blocked because the project is already open in another Unity instance.

## Code Survey

| Source | Finding | Integration Response |
| --- | --- | --- |
| `BuildSandboxPlacedItemSnapshot.cs` | Snapshot already contains item identity, family/base id, stat, and `EnergyState`. | Added BuildSandbox-only behavior mapping; corrected `preview_soul_seal` to the `soul_suppress_talisman` family/base. |
| `FormationEnergyContract.cs` | Only spirit/energy stone family can be a Powered provider; incense/core are diagnostics only. | Evaluator reads the contract result and does not redefine providers. |
| `BattleSandboxRuntimeLoop.cs` | Item trigger settlement previously was not fully state-gated. | Runtime now evaluates legacy behavior before spending mana or applying damage/shield/cleanse/control. |
| `BattleSandboxCombatKernelAdapter.cs` | Existing sandbox kernel already applies shield-first damage and clamps HP/shield. | Legacy damage feeds into existing sandbox-only kernel. |
| `BattleSandboxBuildCombatPreview.cs` | Existing roster/stat/tag/affix preview data supplies context. | Reports reuse those devOnly fields; no formal configs are touched. |
| `BattleSandboxItemEffectRuntimePreview*` | Existing player-facing effect text covers fire/thunder/cleanse/control/energy/guard/peach. | Runtime rows continue using this text, with EnergyState-specific trigger feedback. |
| `LegacyAndAdvancedItemRoster*` | Current roster has 23 BuildSandbox items. | All roster items are represented in the family map and behavior rows. |
| V0.2/V0.3 behavior sources | Old behavior covers fire damage/burn, thunder break, exorcism, cleanse, soul control, peach protection, and spirit-stone energy. | V0.4 sandbox approximates those base roles without editing formal sources. |

## Required Core Items

| CN | Stable Key | itemFamily | baseItemId | V0.4 Advanced Relation | Powered Sample | Player Feedback |
| --- | --- | --- | --- | --- | --- | --- |
| 火符 | `legacy_fire_talisman` | `fire_talisman` | `fire_talisman_basic` | 炽火符 = 火符 family 进阶 / 分支 | `enemyHpDamage=36` | 供能稳定，火符完整触发。 |
| 雷符 | `legacy_thunder_talisman` | `thunder_talisman` | `thunder_talisman_basic` | 雷引剑符 = 雷符 family 进阶 / 分支 | `enemyHpDamage=62;enemyShieldPressure=17` | 供能稳定，雷符完整触发。 |
| 驱邪铃 | `legacy_exorcism_bell` | `exorcism_bell` | `exorcism_bell_basic` | 镇邪铃 = 驱邪铃 family 进阶 / 分支 | `enemyHpDamage=36;cleanse=2;control=5` | 供能稳定，驱邪铃完整触发。 |
| 净化符 | `legacy_purify_talisman` | `purify_talisman` | `purify_talisman_basic` | 净化折符 = 净化符 family 进阶 / 分支 | `cleanse=8` | 供能稳定，净化符完整触发。 |
| 镇魂符 | `legacy_soul_suppress_talisman` | `soul_suppress_talisman` | `soul_suppress_talisman_basic` | 镇魂法印 = 镇魂符 family 进阶 / 分支 | `cleanse=2;control=11` | 供能稳定，镇魂符完整触发。 |
| 桃木牌 | `legacy_peach_wood` | `peach_wood` | `peach_wood_basic` | 桃木剑 = 桃木牌 family 进阶 / 分支 | `playerShieldGain=4;control=4` | 供能稳定，桃木牌完整触发。 |
| 聚灵石 | `legacy_spirit_stone` | `spirit_stone` | `spirit_stone_basic` | 聚灵石 / 聚能石家族为正式供能 provider | `manaGain=12` | 供能稳定，聚灵石完整触发。 |

## EnergyState Samples

| EnergyState | Trigger Difference | Core Sample Result |
| --- | --- | --- |
| `None` | 不触发或沉寂，不耗灵，不输出完整效果。 | all required core rows: `no_trigger` |
| `WeakPulse` | 低效基础触发，45% efficiency，不允许词条 / 羁绊增益。 | 火符 `enemyHpDamage=16`; 雷符 `enemyHpDamage=28;enemyShieldPressure=8`; 净化符 `cleanse=4` |
| `Powered` | 完整基础效果，允许词条 / 羁绊增益。 | values listed in Required Core Items |
| `Suppressed` | 被压制，不触发或不输出效果。 | all required core rows: `no_trigger` |

## Player/Developer Masking

- Player feedback text is Chinese-only and does not include stable keys, answer fields, drop/reward/save data, or formal progression tokens.
- Developer fields are report-side only: `developerFieldsHidden=True` in behavior rows.
- `preview_energy_incense` and `preview_stone_core` are auxiliary rows and are not formal Powered providers.

## Validation Notes

- Static leak counters: `0`.
- Local `git diff --check -- <touched files>` passed.
- Full `git diff --check` still reports pre-existing whitespace in `Scene_TalismanBag_V04_BattleSandboxPreview.unity`, which is forbidden scope for this package and was not touched.
- Unity batch was attempted with `F:\2022.3.50f1c1\Editor\Unity.exe`, but Unity refused because another instance already has this project open.
