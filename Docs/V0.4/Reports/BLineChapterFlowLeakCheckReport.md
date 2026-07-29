# B-Line Chapter Flow Leak Check Report

## Result

- Result: `PASS`
- Forbidden dependency hits: `0`
- Legacy content references: `0`
- Existing-file modifications: `0`
- Out-of-whitelist new files: `0`
- Missing whitelist files: `0`
- Git operations: `0`
- Second package started: `0`

## Forbidden runtime terms

- `UnityEngine.SceneManagement`: `0`
- `SceneManager`: `0`
- `MonoBehaviour`: `0`
- `ScriptableObject`: `0`
- `Resources.Load`: `0`
- `GameObject`: `0`
- `Transform`: `0`
- `PlayerPrefs`: `0`
- `SaveData`: `0`
- `SaveService`: `0`
- `MainTrialFlowService`: `0`
- `V02RunFlowController`: `0`
- `V02RunConfig`: `0`
- `AutoCombatController`: `0`
- `RewardService`: `0`
- `RewardConfig`: `0`
- `RewardDropTable`: `0`
- `Inventory`: `0`
- `ItemDropGeneration`: `0`
- `ItemInstanceRollEngine`: `0`
- `EnemyRuntime`: `0`
- `EnemySkillController`: `0`
- `BuildSettings`: `0`
- `AssetDatabase`: `0`
- `UnityEditor`: `0`
- `System.Random`: `0`
- `UnityEngine.Random`: `0`
- `Guid.NewGuid`: `0`
- `DateTime.Now`: `0`
- `DateTime.UtcNow`: `0`
- `Environment.TickCount`: `0`

## Whitelist delta

### Existing file modifications

- `NONE`

### Out-of-whitelist new files

- `NONE`

### Missing whitelist files

- `NONE`


## Protected aggregates

| Group | Before | After | Result |
|---|---|---|---|
|READ_INPUTS|`d7f13a98a439ab7d1a201bc02bf159241a6656691be7e2e880190080ab342683`|`d7f13a98a439ab7d1a201bc02bf159241a6656691be7e2e880190080ab342683`|`PASS`|
|LOCKED|`7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f`|`7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f`|`PASS`|
|GOVERNANCE|`beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2`|`beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2`|`PASS`|
|FORMAL_V03|`97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d`|`97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d`|`PASS`|

## Fixed isolation

- `devOnly=true`
- `isEnabled=false`
- `formalFlow=false`
- `sceneBindings=0`
- `uiBindings=0`
- `rewardBindings=0`
- `saveBindings=0`
- `inventoryBindings=0`
- `enemyRuntimeBindings=0`
- `battleExecutorBindings=0`

`BLINE_CHAPTER_FLOW_CONTRACT01_PASS chapters=4 stages=40 bossGates=4 manualBossChallenges=4 formalBindings=0 saveWrites=0 rewardGrants=0 legacyContentRefs=0`
