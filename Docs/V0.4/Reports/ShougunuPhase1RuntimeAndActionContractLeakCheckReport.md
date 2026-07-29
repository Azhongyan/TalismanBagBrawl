# Shougunu Phase1 Runtime And Action Contract Leak Check Report

- Package: `V0.4-ShougunuPhase1RuntimeAndActionContract01`
- Status: `LEAK_CHECK_PASS`
- Assignment SHA-256: `40f244475a2aabadffb8ba440c011ec8a7dae46c69043db7f93e928788c0c757`
- Report canonical: `sha256:982aed21f2fbafcfcb237f94fc62b0012e87d8eefbfedd3932514508b0eb8c33`
- Exact additions / existing modifications: `23 / 0`
- Namespace collision regression: `PASS`
- Forbidden runtime domain scan: `PASS`
- Protected baselines: `18/18`
- Report determinism / missing-report regeneration: `PASS / PASS`
- Unity batch/Builder/import/scene handtest: `NOT_REQUIRED_BY_STATIC_REPORT`; final compile/verifier result is reported by the handoff.
- Git operations: `0`

## Leak Assertions

- No package-owned source declares `namespace TalismanBag.Editor`.
- No Runtime consumer or formal route is introduced.
- No blind-woman Runtime actor, HP, targetability, damage receiver or cross-target transfer is authored.
- No Phase2/3, summon, split, Item, Battle implementation, BuildSandbox, CrossSystem, Scene, Prefab, UI or Visual write is present.
- Presentation-safe projection excludes future thresholds, pending queues, Basic counters, dedupe state, developer diagnostics and BA-D3/BA-D4 answers.

## Guard Namespace Adjudication

- Historical defective Assignment entry (preserved as evidence): `TalismanBag.Editor.EnemySystem.ShougunuPhase1RuntimeAndActionContractVerifier.RunFromCommandLine`
- Effective compile-safe entry: `TalismanBag.EditorTools.EnemySystem.ShougunuPhase1RuntimeAndActionContractVerifier.RunFromCommandLine`
- Guard supersession reason: matches accepted Enemy verifier namespaces and prevents creation of sibling namespace `TalismanBag.Editor`, avoiding `UnityEditor.Editor` / CS0118 shadowing.
- `TalismanBagSceneBuilder` modification: `0`

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
