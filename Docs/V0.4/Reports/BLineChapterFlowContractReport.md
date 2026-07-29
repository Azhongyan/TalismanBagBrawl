# B-Line Chapter Flow Contract Report

## Package

- Package: `V0.4-BLineChapterFlowContract01`
- Manifest schema: `V04ChapterFlowManifest.v1`
- State schema: `V04ChapterFlowStateSnapshot.v1`
- Reducer schema: `V04ChapterFlowReducer.v1`
- Projection schema: `V04ChapterFlowBattleContractProjection.v1`
- Validation schema: `V04ChapterFlowValidation.v1`
- Assignment SHA-256: `db04bd4afe30c3eb5e40d2b62f5595dade6d100ea405b463debbac17d6c334f1`

## File inventory

- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowPrimitives.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowReducer.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowValidation.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/V04ChapterFlowContractVerifier.cs`
- `Docs/V0.4/Reports/BLineChapterFlowContractReport.md`
- `Docs/V0.4/Reports/BLineChapterFlowContractSpec.csv`
- `Docs/V0.4/Reports/BLineChapterFlowManifest40.csv`
- `Docs/V0.4/Reports/BLineChapterFlowTransitionMatrix.csv`
- `Docs/V0.4/Reports/BLineChapterFlowLeakCheckReport.md`

## Required counts

- chapters: `4`
- stages: `40`
- encounterNodes: `12`
- bossGates: `4`
- manualBossChallenges: `4`
- automaticBossStarts: `0`
- normalContentBindings: `0`
- sceneBindings: `0`
- uiBindings: `0`
- rewardBindings: `0`
- saveBindings: `0`
- inventoryBindings: `0`
- enemyRuntimeBindings: `0`
- battleExecutorBindings: `0`
- formalBindings: `0`
- legacyContentRefs: `0`
- duplicateStageIds: `0`
- missingHookIds: `0`
- forbiddenDependencyHits: `0`
- protectedHashDrift: `0`

## QA

- Unity compile: `PASS`
- Unity batch verifier: `PASS`
- Static manifest/reducer/leak QA: `PASS`
- User hand test: `NOT_REQUIRED_FOR_THIS_PURE_CONTRACT_PACKAGE`

## Protected aggregates

| Group | Before | After | Result |
|---|---|---|---|
|READ_INPUTS|`d7f13a98a439ab7d1a201bc02bf159241a6656691be7e2e880190080ab342683`|`d7f13a98a439ab7d1a201bc02bf159241a6656691be7e2e880190080ab342683`|`PASS`|
|LOCKED|`7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f`|`7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f`|`PASS`|
|GOVERNANCE|`beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2`|`beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2`|`PASS`|
|FORMAL_V03|`97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d`|`97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d`|`PASS`|

## Boundary

- existing-file modifications: `0`
- out-of-whitelist new files: `0`
- Git operations: `0`
- second package started: `0`
- devOnly: `true`
- isEnabled: `false`
- formalFlow: `false`

## Final marker

`BLINE_CHAPTER_FLOW_CONTRACT01_PASS chapters=4 stages=40 bossGates=4 manualBossChallenges=4 formalBindings=0 saveWrites=0 rewardGrants=0 legacyContentRefs=0`

No second package was started.
