# Item Instance Placement Binding Contract Leak Check Report

- Package: `V0.4-ItemInstancePlacementBindingContract01`
- Result: `PASS`
- Leak Count: `0`
- Runtime scope: independent read-only Item identity binding contract only.
- Scene / Prefab / UI / RectTransform / BuildSettings writes: none.
- Enemy / CrossSystem / Battle / Board / RunFlow / SaveData / Reward / Chapter connections: none.
- Item→BuildCapability mapping or BP conversion: none.

- PASS: no forbidden runtime token findings.

## Protected Before / After

- PASS `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs`: 794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827 → 794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827
- PASS `Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs`: f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 → f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967
- PASS `Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs`: 3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db → 3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db
- PASS `ProjectSettings/EditorBuildSettings.asset`: 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 → 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
- PASS `Assets/_Game/Scripts/TalismanBag/EnemySystem`: 6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb → 6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb
- PASS `Assets/_Game/Scripts/TalismanBag/CrossSystem`: af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48 → af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48
- PASS `Assets/_Game/Scenes`: 7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579 → 7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579
- PASS `Assets/_Game/Prefabs`: 264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8 → 264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8
