# Layout Resilience Requirement Migration Survey Leak Check Report

- Package: V0.4-LayoutResilienceRequirementMigrationSurvey01
- Schema: LayoutResilienceRequirementMigrationSurvey.v1
- Canonical Signature: sha256:4a77a47181d41ecd18f589f86be911ce08fc466725807a16c42ab1d3e5068126
- Leak Count: 0

## Scope result

- New allowlisted files: 9.
- Existing files modified by this package: 0.
- Candidate rows: 4.
- Requirement-group membership rows: 9.
- Pressure fact gap rows: 32.
- Context rows: 10.
- User decision rows: 12.
- Synthetic fixtures: 20/20 PASS.
- Assertions: 112/112 PASS.

## Zero-impact checks

- P3 assembler calls: 0.
- P4 readiness-consumer calls: 0.
- N01C evaluator calls: 0.
- N01C validator calls: 0.
- Real migration/readiness mappings: 0.
- Applicability activation: 0.
- C02 reduction: 0; 32/32 blocked remains unchanged.
- Actual Unknown reduction: 0.
- Legacy BP calculation, conversion, comparison, or threshold use: 0.
- Enemy, Item, Battle, Board, Map, Scene, Prefab, UI, BuildSettings, SaveData, Reward, Chapter, and runtime producer modifications: 0.
- Later-package starts: 0.

## Protected baselines

- E10 exact 21: 3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be.
- E10 Full / PlayerSafe: sha256:377fb8d27d35dd127b915944d8a00c1b17afd6a580254dc21bc8d44e51a815a3 / sha256:aa1c4ccde3fccaadc09da4bcb0e72bf53ee9c110ed84d366e20ebd875bf4775d.
- E10 Catalog / E06 snapshot / E10 pressure / E10 composition / E03 inventory: 9f08a734b82af27f9b6777946f431fd7247466017b95abccef6da4116424eaa0 / 56cc65badaf89be6882629ae847fcf03c2d0bac3d800bdb562de66e275e4d1c1 / 9e543c04022bf7c4a3e646511cf67bb20d10f620f5bfb0c551c848baad9ee50c / 7feaad7c4bef64e2ea3dcb3bd51dfbd78690a80337d576cd5f018fab2960472e / f993828a64b112a8ac2c640d592f50ee139bb374821550520932f7e6637b04f6.
- P4 / P3 / P2 / P1 / N01C / N01B: 8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530 / c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c / 15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050 / 7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48 / 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b / acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316.
- Item105 / IF01 / Enemy89: b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4 / 2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8 / 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae.
- C01 / C02 / C02A / N01 / N01A: fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a / 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 / a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb / 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52 / 705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de.
- Scenes14 / Prefabs16 / BuildSettings: da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b / 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 / 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59.
- Protected hashes before/after: identical.

## Repository audit

- Offline verifier: 112/112 PASS; fixtures 20/20 PASS.
- Unity batch verifier: 112/112 PASS; fixtures 20/20 PASS.
- GUID conflicts: 0.
- Trailing whitespace: 0.
- git diff --check: PASS.
- Forbidden scope touched by this package: 0.
- Pre-existing tracked dirty preserved unchanged by this package: Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset; Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs.
- HEAD: f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba unchanged.
- Commit / tag / push: none.
- Next package: NOT_STARTED.

All four candidates remain MIGRATION_CANDIDATE / APPLICABILITY_UNRESOLVED / USER_AUTHORING_REQUIRED. The legacy BP values 4631, 5340, 5383, and 2884 remain quarantined diagnostic literals only.
