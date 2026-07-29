# Shougunu Phase1 Runtime And Action Contract Report

- Package: `V0.4-ShougunuPhase1RuntimeAndActionContract01`
- Status: `QA_PASS`
- Assignment SHA-256: `40f244475a2aabadffb8ba440c011ec8a7dae46c69043db7f93e928788c0c757`
- Schema: `ShougunuPhase1RuntimeAndActionContract.v1`
- Mode: `DEV_ACTIVE / devOnly=true / isEnabled=false / entersFormalFlow=false / runtimeBoundToBattle=false`
- Existing files modified: `0`
- Unity scene/user handtest: `NOT_REQUIRED`
- Report canonical: `sha256:982aed21f2fbafcfcb237f94fc62b0012e87d8eefbfedd3932514508b0eb8c33`

## Result

- Deterministic checks: `89/89`
- Nominal Battle applications: `50/50`
- Nominal shell breaks / threshold skills / Basic resolves: `7/6/12`
- Final nominal lifecycle: `Defeated` at `75000` ticks
- Negative fixtures: `12/12`
- Consecutive report hash determinism: `PASS`
- Missing-report regeneration: `PASS`

## Frozen Ownership Boundary

- Enemy owns immutable Phase1 facts, reducer state, threshold/debt queue, action-pattern intent and cues.
- Battle owns accepted damage applications and player-side effect execution. No player HP or player damage value is authored here.
- Fixture `nominalPulseMagnitude` repeats `29,42,53,46,55,76`; authoritative applied deltas remain state-valid and may be lower at a shell/HP boundary without reducer clamping.
- Skill1 repairs only the current damaged nonzero shell by `30`, capped at `200`; it never heals HP or revives a shell.
- Skill2 does not author bind, stun or movement. Skill3 does not author hazard, DoT or persistent ground.
- BA-D1 runtime actor/damage-transfer implications are absent. BA-D3 and BA-D4 remain unresolved and are not exposed.
- Phase2, Phase3, summon, split, Item, BuildSandbox, CrossSystem, Scene, Prefab, UI and formal-flow bindings remain out of scope.

## Guard Namespace Adjudication

- Historical defective Assignment entry (preserved as evidence): `TalismanBag.Editor.EnemySystem.ShougunuPhase1RuntimeAndActionContractVerifier.RunFromCommandLine`
- Effective compile-safe entry: `TalismanBag.EditorTools.EnemySystem.ShougunuPhase1RuntimeAndActionContractVerifier.RunFromCommandLine`
- Guard supersession reason: matches accepted Enemy verifier namespaces and prevents creation of sibling namespace `TalismanBag.Editor`, avoiding `UnityEditor.Editor` / CS0118 shadowing.
- `TalismanBagSceneBuilder` modification: `0`

## Protected Baselines

- `BossArtSurvey`: `PASS`; disposition `UNCHANGED`; task-start `9/93659d9b894251e73ad85a0d854c8a7c62d0f795c54ca38a5d3bf2065b841dc6`; actual `9/93659d9b894251e73ad85a0d854c8a7c62d0f795c54ca38a5d3bf2065b841dc6`
- `BuildSandbox`: `PASS`; disposition `UNCHANGED`; task-start `186/eae1e45367a3f970858a4befe0920bee15c350caaf4d0cddb9de363b5238a228`; actual `186/eae1e45367a3f970858a4befe0920bee15c350caaf4d0cddb9de363b5238a228`
- `BuildSettings`: `PASS`; disposition `UNCHANGED`; task-start `1/8acc1a59a38b35b90a92e81fb101ce4377bca9c9300ddee0e235f7efa4fc8197`; actual `1/8acc1a59a38b35b90a92e81fb101ce4377bca9c9300ddee0e235f7efa4fc8197`
- `Configs`: `PASS`; disposition `UNCHANGED`; task-start `121/2383359ab713475d8d85748032965c3188f47c436e3644d40abfb54260807769`; actual `121/2383359ab713475d8d85748032965c3188f47c436e3644d40abfb54260807769`
- `CrossSystem`: `PASS`; disposition `UNCHANGED`; task-start `63/dfd3445869710d3b28e3ddacdca20a7850ea8c0125246599b14e1ed649e1a64d`; actual `63/dfd3445869710d3b28e3ddacdca20a7850ea8c0125246599b14e1ed649e1a64d`
- `EditorEnemySystem`: `PASS`; disposition `UNCHANGED`; task-start `28/8dd2e42698d3bbab13305f27d222c9787590a708473cdeaa8d2a3ffa0d23a817`; actual `28/8dd2e42698d3bbab13305f27d222c9787590a708473cdeaa8d2a3ffa0d23a817`
- `EnemySystem`: `PASS`; disposition `UNCHANGED`; task-start `117/04402d30a62c54e7fd3ed08349c5553a66487eca64b6fe9daa0418fb367b7591`; actual `117/04402d30a62c54e7fd3ed08349c5553a66487eca64b6fe9daa0418fb367b7591`
- `GapSurvey`: `PASS`; disposition `UNCHANGED`; task-start `8/0a79e5e9a40cb8e88d0d25fc0db97972e03452cf3168878df98ed26871d9fca0`; actual `8/0a79e5e9a40cb8e88d0d25fc0db97972e03452cf3168878df98ed26871d9fca0`
- `Governance`: `PASS`; disposition `UNCHANGED`; task-start `18/1988a87a8a0a6864807db6c52708d26a326430d94c8945fa63804018c0dc46e3`; actual `18/1988a87a8a0a6864807db6c52708d26a326430d94c8945fa63804018c0dc46e3`
- `Items`: `PASS`; disposition `EXTERNAL_CONCURRENT_DRIFT / ITEM_OWNED`; task-start `141/4b41e4dbede32a14a661f0f985cb1682051c30836b54d811a4c55546652b18e4`; actual `148/bcc4b21367ecc7e7ccac018581e1eacdaf567a9d5cc9a1662d755a2c4ef4f2d4`
- `P0`: `PASS`; disposition `UNCHANGED`; task-start `8/2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5`; actual `8/2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5`
- `P1`: `PASS`; disposition `UNCHANGED`; task-start `21/21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209`; actual `21/21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209`
- `Packages`: `PASS`; disposition `UNCHANGED`; task-start `2/11efaaf4e1df1215e977f884d2d5e2c2bdbbbd1e62337b0517f6a0502a929cb6`; actual `2/11efaaf4e1df1215e977f884d2d5e2c2bdbbbd1e62337b0517f6a0502a929cb6`
- `Prefabs`: `PASS`; disposition `UNCHANGED`; task-start `5/a0274a939eeed890c98522bde495a37b2f4b533fdcd753bdb2896cf6b5be485e`; actual `5/a0274a939eeed890c98522bde495a37b2f4b533fdcd753bdb2896cf6b5be485e`
- `ProjectSettings`: `PASS`; disposition `UNCHANGED`; task-start `21/1969814e5ec3983ba426b7d5f19f13de7f480277cb5118a92d06c66b58fcbac7`; actual `21/1969814e5ec3983ba426b7d5f19f13de7f480277cb5118a92d06c66b58fcbac7`
- `Scenes`: `PASS`; disposition `UNCHANGED`; task-start `14/4c9e101026dd4aeb35e8c729f080a527c5a621d8df721836802f37057dad681d`; actual `14/4c9e101026dd4aeb35e8c729f080a527c5a621d8df721836802f37057dad681d`
- `UnityAssets`: `PASS`; disposition `UNCHANGED`; task-start `7/081bb0469c44f88ededd6ca63d6b722d6027311d31c4bbee4732462364fe242b`; actual `7/081bb0469c44f88ededd6ca63d6b722d6027311d31c4bbee4732462364fe242b`
- `Vocabulary`: `PASS`; disposition `UNCHANGED`; task-start `17/3e1a95be4d747423e8829939fafd31bb4eafa292c87c1d743f8d9ebe667941cc`; actual `17/3e1a95be4d747423e8829939fafd31bb4eafa292c87c1d743f8d9ebe667941cc`

## Attributed External Concurrent Drift

- Disposition: `EXTERNAL_CONCURRENT_DRIFT / ITEM_OWNED / OWNER_LEASE_ACTIVE`
- Owner package: `V0.4-ItemCombatEffectRequestContract01`
- Owner Assignment SHA-256: `6dd4cad1fc930d0b969e9ff92455b1cbbdb52a40f8b4809add69819b7e9ec35e`
- Ownership receipt proves whitelist ownership only, not Item QA or acceptance.
- Enemy task-start Items aggregate remains `141/4b41e4dbede32a14a661f0f985cb1682051c30836b54d811a4c55546652b18e4`; attributed current aggregate is `148/bcc4b21367ecc7e7ccac018581e1eacdaf567a9d5cc9a1662d755a2c4ef4f2d4`.
- Initial ownership-receipt aggregate was `148/c743d73541b94c2194e1e4cb585db69aaf0428407397158be038b1610363ecaf`; later content updates on the same seven frozen Item paths remain under the same Owner Lease.
- These paths remain read-only and are not rebaselined, restored, formatted, staged or claimed by this package.
- `Assets/_Game/Scripts/TalismanBag/Items/Combat.meta`: `ABSENT -> 432cb8568fb85ea8f496b8ff26c0dc2d19129700a8c79be184a14f8103d2b051`; disposition `ITEM_OWNED`; initial receipt `432cb8568fb85ea8f496b8ff26c0dc2d19129700a8c79be184a14f8103d2b051`
- `Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs`: `ABSENT -> 5f6028b9e4801094a540df9889d58a80208ee43cd9d0fcb6f6f117ee373b0313`; disposition `ITEM_OWNED`; initial receipt `5f6028b9e4801094a540df9889d58a80208ee43cd9d0fcb6f6f117ee373b0313`
- `Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs.meta`: `ABSENT -> fc0141e38ff9123c5d3eeccd1ff923972fc2ee0c08237bd8b5a0a49df61b6b94`; disposition `ITEM_OWNED`; initial receipt `fc0141e38ff9123c5d3eeccd1ff923972fc2ee0c08237bd8b5a0a49df61b6b94`
- `Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs`: `ABSENT -> f5d1d46c2d8aa3654072f9c431adf59a5f6f9d815a24965e2e27519c1914aa95`; disposition `ITEM_OWNED`; initial receipt `f5d1d46c2d8aa3654072f9c431adf59a5f6f9d815a24965e2e27519c1914aa95`
- `Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs.meta`: `ABSENT -> f623ab680be2c3780909ec9a74907079b2a3e2ba2a4dbad1094056cd4f8d53d5`; disposition `ITEM_OWNED`; initial receipt `f623ab680be2c3780909ec9a74907079b2a3e2ba2a4dbad1094056cd4f8d53d5`
- `Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs`: `ABSENT -> c2428f82893d52542a150fb36aa24cc3833575fc863624e7740b376d470180cc`; disposition `ITEM_OWNED / INCREMENTAL_HASH_UPDATE`; initial receipt `0a2d91eead9d397c0f3c271f9318528e5e7ca98de1c2ebf5acb04ecab3c9d121`
- `Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs.meta`: `ABSENT -> 3e140241baabe73cacc3890b4117adb0093402dd8eb90d3de95fafe931500314`; disposition `ITEM_OWNED`; initial receipt `3e140241baabe73cacc3890b4117adb0093402dd8eb90d3de95fafe931500314`

## Package Manifest

- Additions: `23`
- Existing modifications: `0`
- `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/ShougunuPhase1RuntimeAndActionContractVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/ShougunuPhase1RuntimeAndActionContractVerifier.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeValidation.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeValidation.cs.meta`
- `Docs/V0.4/Reports/ShougunuPhase1NegativeFixtureRows.csv`
- `Docs/V0.4/Reports/ShougunuPhase1NominalTrace.csv`
- `Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractFieldMatrix.csv`
- `Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractLeakCheckReport.md`
- `Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md`
- `Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractSpec.csv`
- `Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionPattern.csv`
- `Docs/V0.4/Reports/ShougunuPhase1ThresholdAndQueueFixtureRows.csv`
