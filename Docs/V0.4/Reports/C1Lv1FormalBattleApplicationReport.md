# C1 Lv1 Formal Battle Application Report

- Package: `V0.4-C1Lv1FormalBattleApplication01`
- Classification: `COMPLEX_GUARDED_ONCE`
- Product context: `CAMPAIGN_NORMAL_LV1`
- Status: `PACKAGE_COMPLETE / AUTOMATED_QA_PASS`
- Assignment SHA before: `4c19fbbfe35b9db99b758d3e88e223c3014df37fc7c4665f72b4fbd8e375e921`
- Assignment SHA after: `4c19fbbfe35b9db99b758d3e88e223c3014df37fc7c4665f72b4fbd8e375e921`
- GitOperations: `NONE`

## Frozen traces

| Scenario | Result |
|---|---|
| 1-1 / I001 lit | VICTORY / 16s / 3 enemy applications / player HP 76 / total Enemy HP 0 |
| 1-2 / I001 lit + I002 unlit | VICTORY / 48s / 10 enemy applications / player HP 20 / total Enemy HP 0 |
| 1-2 / I001 lit + I002 lit | VICTORY / 36s / 7 enemy applications / player HP 44 / total Enemy HP 0 |

- SpeedGain: `0.25`
- EnemyApplicationsAvoided: `3`
- MiddleTier: `NOT_AUTHORED / NOT_DISTINCT`
- RewardGrantCalls: `0`
- SaveWrites: `0`
- SceneWrites: `0`
- DevShowcaseDataUses: `0`
- ShougunuReferences: `0`

## Verification summary

- Checks: `77`
- Failures: `0`
- Protected source drift: `NO`
- BattleResultSnapshot shouldWriteSave: `false`
- BattleResultSnapshot shouldGrantReward: `false`

`C1_LV1_FORMAL_BATTLE_APPLICATION01_PASS`
