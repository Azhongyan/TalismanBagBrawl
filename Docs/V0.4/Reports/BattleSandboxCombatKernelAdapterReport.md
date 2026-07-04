# BattleSandbox Combat Kernel Adapter Report

Package: `V0.4-BattleSandboxCombatKernelAdapter01`
Generated: `2026-07-04`
Status: `PASS`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- Builds a devOnly V04 BuildSandbox combat-rule adapter snapshot.
- Reuses V0.2/V0.3 mouthfeel for spirit power, cooldown, damage, shield, healing, enemy HP, enemy skill cast bars, and status ticks.
- Emits adapter rows and sample values only; it does not run formal combat.
- Does not connect formal RunFlow, SaveData, Reward, Chapter, feature flags, formal damage settlement, or V04 UI layout changes.
- Does not modify V0.2/V0.3 runtime files.

## Required Counters

- rule row count: `9`
- feature flag default true count: `0`
- feature flags all disabled: `True`
- formal flow leak count: `0`
- UI layout leak count: `0`
- rule scope leak count: `0`
- devOnly/isEnabled isolation pass: `True`

## Sample Snapshot

| Rule | Expected Sample |
| --- | --- |
| `spiritPower` | cross=FullPowered, diagonal=WeakPowered, spiritNineGrid=FullPowered |
| `cooldown` | fullFire=1.6, weakFire=2.16, trainedWeakFire=1.944, min=0.1 |
| `damage` | blocked=8, hpDamage=12, hpAfter=108, shieldAfter=0 |
| `playerShield` | shieldAfter=50, wasted=13, enemyShieldAfter=12 |
| `healing` | hpAfter=100, overheal=10 |
| `enemyHp` | spawnHp=120, interval=2, enrageHp=60 |
| `enemySkillCastBar` | remaining=0.75, bar=0.6, cooldownAfterComplete=4 |
| `statusTick` | playerDot=5, enemyBurnDot=3 |

## Adapter Rows

| Rule | Source | Sample Input | Sample Output |
| --- | --- | --- | --- |
| `spiritPower` 灵力供能 | V02/Formation/FormationPowerResolver + V02/Balance/V02FormationBalanceConfig | eyeCross=3:2, eyeDiagonal=3:3, spiritCell=2:3 | cross=FullPowered, diagonal=WeakPowered, spiritNineGrid=FullPowered |
| `cooldown` 冷却口径 | V02/CoreLoop/Battle/BattleLoadoutSnapshotBuilder + Combat/AutoCombatController.GetEffectiveCooldown | base=2, computed=1.8, weakMultiplier=1.35 | fullFire=1.6, weakFire=2.16, trainedWeakFire=1.944 |
| `damage` 伤害与护盾抵扣 | Combat/AutoCombatController.DealDamage + Combat/CombatStats.TakeDamage | enemyHp=120, shield=8, damage=20 | blocked=8, hpDamage=12, hpAfter=108, shieldAfter=0 |
| `playerShield` 玩家护盾 | Combat/CombatStats.AddShield + Combat/AutoCombatController.V02AddEnemyShield | playerShield=45, gain=18, cap=50 | playerShieldAfter=50, wasted=13, enemyShieldAfter=12 |
| `healing` 治疗口径 | Combat/CombatStats.Heal + Combat/AutoCombatController.TryTriggerItem | hp=90, heal=20, maxHp=100 | hpAfter=100, overheal=10 |
| `enemyHp` 敌人血量与普攻 | Enemies/EnemyRuntime + Combat/AutoCombatController.UpdateEnemyBasicAttack | maxHp=120, attack=8, interval=2 | spawnHp=120, spawnShield=0, enrageHp=60 |
| `enemySkillCastBar` 敌人施法条 | V02/EnemySkills/EnemySkillRuntime + V02/EnemySkills/EnemySkillController | initialDelay=0.5, castTime=1.25, tick=0.5 | remaining=0.75, bar=0.6, cooldownAfterComplete=4 |
| `statusTick` 状态跳字 | V02/Status/StatusEffectController + Combat/AutoCombatController status ticks | playerPoison=2, playerBurn=3, enemyBurn=3 | playerDot=5, enemyBurnDot=3 |
| `adapterIsolation` 沙盒隔离 | Docs/LOCKED + Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP | devOnly=true, isEnabled=false, featureFlagsDefaultFalse=true | formalRunFlow=0, saveWrites=0, rewardWrites=0, chapterAdvances=0, uiRewrites=0, stableRuntimeWrites=0 |

## Validation Summary

| Check | Status | Errors | Warnings |
| --- | --- | ---: | ---: |
| BattleSandbox Combat Kernel Adapter 01 | `PASS` | 0 | 0 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
