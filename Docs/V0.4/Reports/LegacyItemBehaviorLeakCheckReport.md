# Legacy Item Behavior Leak Check Report

Package: `V0.4-LegacyItemBehaviorIntegration01`
Guard Pass: `GUARD_PASS_LEGACY_ITEM_BEHAVIOR_INTEGRATION01`
Status: `PASS_STATIC / UNITY_BATCH_BLOCKED`
Leak Count: `0`

## Leak Counters

| Check | Count | Expected |
| --- | ---: | ---: |
| `noneOutputLeaks` | 0 | 0 |
| `weakPulseFullPowerLeaks` | 0 | 0 |
| `poweredMissingCoreOutputs` | 0 | 0 |
| `suppressedOutputLeaks` | 0 | 0 |
| `energyProviderDamageLeaks` | 0 | 0 |
| `forbiddenProviderLeaks` | 0 | 0 |
| `playerSideAnswerLeaks` | 0 | 0 |
| `developerFieldVisibleLeaks` | 0 | 0 |
| `formalFlowWrites` | 0 | 0 |
| `formalSaveWrites` | 0 | 0 |
| `formalRewardWrites` | 0 | 0 |
| `formalDropWrites` | 0 | 0 |
| `formalBackpackWrites` | 0 | 0 |
| `scenePrefabRectTransformWrites` | 0 | 0 |
| `buildSettingsWrites` | 0 | 0 |
| `featureFlagDefaultTrue` | 0 | 0 |
| `totalLeaks` | 0 | 0 |

## Energy Contract Checks

- `spirit_stone_basic` / spirit-stone family is the only provider family represented in behavior samples.
- `preview_energy_incense` and `preview_stone_core` remain auxiliary/non-provider rows even in Powered samples.
- `spirit_stone_basic` has `enemyHpDamage=0` in every sample state.
- `None` and `Suppressed` rows have `triggers=False`, `manaCost=0`, and no output values.
- `WeakPulse` rows use `efficiencyPercent=45` and `allowAffixSynergyBonus=False`.

## Formal Scope

This package did not edit or write:

- V0.2 / V0.3 formal item configs
- formal itemId
- formal drops / rewards / backpack / save data
- `V02RunFlowController`
- `PageState` / `FormationState`
- Boss / reward / formal numeric mainline
- Scene / Prefab / RectTransform
- BuildSettings

## Verification Note

- `git diff --check -- <package touched files>` passed.
- Full `git diff --check` still reports pre-existing trailing whitespace in `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`; this package did not touch that forbidden scene file.
- Unity batch was attempted with `F:\2022.3.50f1c1\Editor\Unity.exe`, but the project was already open in another Unity instance, so Console red/yellow acceptance could not be verified in batch from this shell.
