# C1 Shattered Host / Porcelain Hound Runtime Report

- Package: `V0.4-C1ShatteredHostPorcelainHoundRuntime01`
- Guard marker: `ENEMY_GUARD_ASSIGNMENT_C1SHATTEREDHOSTPORCELAINHOUNDRUNTIME01`
- Assignment: `Docs/V0.4/C1ShatteredHostPorcelainHoundRuntime01_Assignment.md`
- Assignment SHA-256: `2329fdb2c8c5ea07484831dec68f60ed1568078aadec0222edfb0b4440cf3751`
- Schema: `BoneAspectC1EnemyRuntime.v1` / version `1`
- Flags: `devOnly=true`, `isEnabled=false`, `entersFormalFlow=false`, `runtimeBoundToBattle=false`
- Offline result: `PASS`
- Unity result: `PASS`
- Fixture assertions: `70`
- Negative fixtures: `14`

## Profiles and actions

- `bone_aspect.runtime.c1.shattered_host.v1` → `c1.shattered_host.basic_attack`
- `bone_aspect.runtime.c1.porcelain_hound.v1` → `c1.porcelain_hound.charge_attack`
- BA-D3: `bone_aspect_enemy_c1_03_bone_swap_remnant`, `HELD_BY_BA-D3`, runtime/action/copy/binding=`0/0/0/0`

## Dev-only fixture values

- Tick rate `1000`; all values are `devOnlyFixture=true`, `formalBalance=false`, `liveTuning=false`.
- Shattered Host: HP `160`, first/repeat `2000/4000`, telegraph/cast/resolve/recover `500/300/800/700`, post-defeat lifetime `SHORT`.
- Porcelain Hound: HP/shell `180/100`, regeneration `0`, first/repeat `2500/6000`, telegraph/cast/resolve/recover `800/400/1200/900`, requested window `3000`.

## Exact 23-path manifest

- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs.meta`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeReport.md`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeSpec.csv`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFieldMatrix.csv`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeEnemyProfiles.csv`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeActionPattern.csv`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFixtureTrace.csv`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeNegativeFixtureRows.csv`
- `Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeLeakCheckReport.md`

## Protected before/after aggregates

- `LOCKED`: `7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f` → unchanged
- `GOVERNANCE_AND_QUEUES`: `beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2` → unchanged
- `FORMAL_V03_1_10_2_10`: `97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d` → unchanged
- `BLINE_CHAPTER_FLOW_PACKAGE`: `5ceb86f913df40bc20640c797f65bd3ac9c5df5b5d5dc49ba999d27bc4d170f1` → unchanged
- `BONE_ASPECT_RUNTIME_PREPACKAGE`: `db18eacdb402409bcdfa81a2b7fb995a11d252cee88a1396e7a0a0f1f4f87aae` → unchanged
- `EDITOR_ENEMY_PREPACKAGE`: `d06b197da8bf62117f90fe391afe298ad068f1ed23081b1fa25f144f233c2639` → unchanged
- `BONE_ASPECT_AND_SHOUGUNU_REPORTS_PREPACKAGE`: `c18aad2f88d9e59da6fe66e55e7f6bee7a26cbdb2b019f807e313a38e7a82906` → unchanged
- `ITEMS_READ_ONLY`: `14a6f36f86f2a4cc2dee00eaa7b97eb65c6f55e64b2ee7351a04f84ed05b7598` → unchanged
- `BUILDSANDBOX_READ_ONLY`: `cbe13503ad5c16c487e9d5eec0a5b4b441d30d7b5d6e0d5c13da7d8a38051811` → unchanged
- `CROSSSYSTEM_READ_ONLY`: `f89f6d531b0bc97f83658918887574fca4775bed7dbe4f4ac2bd1124da154d40` → unchanged
- `SCENES_READ_ONLY`: `a2bc880126c7ca69143277486bd495c3d7aa144a29d3446a5a248626348920d1` → unchanged
- `PREFABS_READ_ONLY`: `a3c0fdc0ae331987e22ad409c5142ace4c4a03fe5f36dfbc6f7683bc9d82d208` → unchanged
- `PROJECTSETTINGS_READ_ONLY`: `9e3c370e01deaeb794933fb4d19103d613c5a270b8d9d211c42b4ce8971a76f5` → unchanged
- `PACKAGES_READ_ONLY`: `52ad627caad2e8ca13a4cd16644a9178394c7cfe07b296f5610b00a2bcb546d2` → unchanged
- BuildSandbox drift owner: `V0.4-LiHuoCombatFeedbackOrchestrationVerticalSlice01`; overlap=`false`.
- ProjectSettings lifecycle attribution: `EXTERNAL_UNITY_EDITOR_SERIALIZATION_LIFECYCLE_CLOSED`; frozen aggregate retained.

## Acceptance

C1_SHATTERED_HOST_PORCELAIN_HOUND_RUNTIME01_PASS profiles=2 actions=2 heldByBad3=1 battleBindings=0 formalBindings=0

No next package was started.
