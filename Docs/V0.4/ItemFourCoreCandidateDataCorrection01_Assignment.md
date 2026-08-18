# V0.4-ItemFourCoreCandidateDataCorrection01 Assignment

Status: `READY_FOR_DEV / ITEM_GUARD_APPROVED / USER_OPTION_C_FROZEN`

Package: `V0.4-ItemFourCoreCandidateDataCorrection01`

Assignment freeze date: `2026-07-23 Asia/Shanghai`

Primary Guard: `Item System Guard`

Joint boundary review: `Item Algorithm Guard`

## 0. Receipt and required stop point

Accepted assignment receipt:

```text
ITEM_GUARD_PASS_ITEMFOURCORECANDIDATEDATACORRECTION01
```

Required development completion state:

```text
DEV_COMPLETE
QA_PASS
CANDIDATE_FOUR_CORE_120_PASS
RARITY_ELIGIBILITY_OPTION_C_150_PASS
ROLL_PROJECTION_150_PASS
USER_HANDTEST_NOT_APPLICABLE
P2_NOT_STARTED
P3_NOT_STARTED
```

This Assignment authorizes Candidate data correction only. It does not authorize Awakening or
runtime-state correction, detail projection, UI, Prefab migration, Battle integration, SaveData,
rewards or any later package.

## 1. Accepted authority

Required P0 evidence:

```text
Docs/V0.4/Reports/ItemFourCoreAuthorityAndMigrationDecisionReport.md
SHA-256 bf70b346c705bd50ed0a1b7890304df69b12f2f9b5df04a44a49e4769fa790a5

Docs/V0.4/Reports/ItemFourCoreAuthorityAndMigrationDecisionMatrix.csv
SHA-256 b1664e0b193909b8890cd72a943db9b42fe011670006b959efc319bf2727f23c
```

Accepted P0 receipts:

```text
ITEM_GUARD_PASS_ITEMFOURCOREAUTHORITYANDMIGRATIONDECISION01
ITEM_ALGORITHM_GUARD_PASS_ITEMFOURCOREAUTHORITYANDMIGRATIONDECISION01
```

## 2. Frozen Candidate semantics

Every ordinary item `I001-I030` has exactly four fixed progressive Candidate core definitions:

```text
candidate_core_ixxx_01       / Core1    / Lv10 / required white
candidate_core_ixxx_02       / Core2    / Lv20 / required green
candidate_core_ixxx_03       / Core3    / Lv30 / required blue
candidate_core_ixxx_ultimate / Ultimate / Lv40 / required orange
```

Core identities are never randomly selected, weighted or rerolled. Stable progression order is
explicitly `01, 02, 03, ULT`; it must not be derived from enum numeric order.

Option C eligibility and visibility are exact:

```text
white  = [01]
green  = [01, 02]
blue   = [01, 02, 03]
purple = [01, 02, 03]
orange = [01, 02, 03, ULT]
```

`eligibleCoreEffectIds` and `visibleCoreEffectIds` are equal in the current Candidate policy.
Purple deliberately adds no core identity. Orange adds the fourth and final identity, Ultimate.

The corrected Candidate revision is:

```text
ITEM_FOUR_CORE_CANDIDATE_DATA_CORRECTION01_R1
```

It must be written consistently to:

- `ItemCompleteCandidateContentSeed.Revision`
- `ItemBalanceWorkbenchCatalog.asset.balanceDataRevision`
- all 30 `ItemBalanceProfile_Ixxx.asset.balanceDataRevision` fields

Do not change any schema ID, generation version, RNG algorithm ID or projection algorithm ID.

## 3. Superseded row and payload preservation

For each `I001-I030`:

```text
01       -> KEEP_CANONICAL
02       -> KEEP_CANONICAL
03       -> KEEP_CANONICAL
04       -> SUPERSEDED_EXPANSION_DRIFT
ultimate -> KEEP_CANONICAL
```

The migration removes `candidate_core_ixxx_04` from active Candidate data. It must not merge the
old `_04` `ExtraTrigger` fields, values or text into Ultimate.

The surviving `01/02/03/ultimate` definitions must retain their task-start semantics field by
field:

```text
coreEffectId
displayName
description
isUltimate
nodeKind
unlockLevel
requiredRarity
effectType
effectPayload, including all nested fields
stateDescription
dataMaturity
designNote
```

Important implementation trap: the current five-row source calculates Ultimate from old stage
index `4`. Shortening a loop and calculating Ultimate at index `3` would silently alter its
payload, secondary value, cooldown or identity. Build the four definitions explicitly or
otherwise prove that the task-start Ultimate semantic hash is unchanged for all 30 items.

## 4. Exact existing-file whitelist and task-start hashes

Only the following existing source files may be modified:

| Path | Task-start SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs` | `c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemBalanceCandidateSeedBuilder.cs` | `26e296805ceba69b6116d91aca1456cb76ad04f2ec23c7ec5aaa49f2c74ff522` |
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchValidation.cs` | `b4a5d2107e52c5db8e8f581300501c53a8498b27a1e3aafb7a238442bcc3a181` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemCompleteCandidateContentWorkbenchVerifier.cs` | `75545133907efe7e2e499b9d116653982d9e2999b52409e1c3b3fb96df41ed6e` |

Only the following existing assets may be modified:

```text
Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset
Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/ItemBalanceProfile_I001.asset
...
Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/ItemBalanceProfile_I030.asset
```

Catalog task-start SHA-256:

```text
5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45
```

Profile aggregate task-start SHA-256:

```text
d56c6c141e8ca9d4cdc6f0ed8119fdd297e470c78e03677b66b719f61d152951
```

Aggregate algorithm:

1. Select exactly `ItemBalanceProfile_I001.asset` through `ItemBalanceProfile_I030.asset`.
2. Sort by filename using `StringComparer.Ordinal`.
3. For each file emit `filename|lowercaseFileSha256`.
4. Join rows with LF and no trailing LF.
5. SHA-256 the UTF-8 bytes.

Per-profile task-start hashes:

```text
I001 ecbb1e9523557f9d72350430e7a91c32f2e097bf34aad4c537958b1c93493f43
I002 76620f8dfac02e3e151bd633e918b54b941417f9be4b36c9918d4281c5fb3964
I003 f75ad7151f671d2ca0d600d5d44401bf9ab9dc9a3175bae6faa3a95374ce77b5
I004 4c231c5dad50afece07ede4a54464c47366a13d21d1cb71f00ea93b630fe278c
I005 2542d1674b90c06d09b350b0d60e8c4915b8c6deecb3ed29d1662021e5d20ad3
I006 410d7cd4ecb11e5b57fd5d2efe3a9fd7cffe379a13e7b6f85b730c4460cffe51
I007 ef4148abd55d5a0dadde878a26295f5f1717c302cfc8f5aff60c5c0423616797
I008 7cc1f6346f910d581fe68706796de27e58338d09de3e7f5f74a45ac76f6085bc
I009 d5152479b4d399e201a2a389d13782d55525f0cee561ad4891728b1624ba4f8d
I010 0fda8beebd26389cc8dbce1f8f1bf70fdac8609c366157448bb8d2e60ed82396
I011 94206fb2b16b3aa9d86475aa5125c13ab704867c6691582a57771c7b0260162c
I012 f11cde87251c22fffd23be2023c881151a0a47f70bbc2fcdf443df04f895fb67
I013 2dca1df6fbfa8a5367d982434843dbd58dc9de1fd9f356a7941eaa6b6ca9fa52
I014 074fdcfec54ce13f9eefcef768d81ba156e2da4aec605db5f5320ed385c74210
I015 b97b996c179fcd9951ea93a53b7388a8446a25c65f3f28f5057abc489ea812d7
I016 5f88655a9945cc7a40dda1891d32562e9d1b14e3ae5dfd66790fae53c7b6b90b
I017 aad9438c1e5eee0f112761a2b7d17bcb00041a29fd9197950429d821570e2a90
I018 a40f43fc0c1543621af816283fe116089e412b31e8e043e9d8d8d186239dd3f4
I019 b5ff2c4b4790904e218f6407910715f256c00e56c1e9471340555ac8367e3c47
I020 6dad4c085e16565cf0b9061e2f079d0dde9a4535afbaff8a5b61549debdea9ce
I021 42d4d9d0fc99a6156686781100c452cedf851c7f5c4f1320e95979e2cb6ecdd8
I022 96e67ac3ec588ef50717bb588eef4991d80b88991e484bfe5bd05dc7e03182e7
I023 ce0f83d3a2f5c3d969acca034ab70c27d003b5440687a91c3a48d597e3c5bd45
I024 4fbefd48bac478c7bff0e90c16ac484b1edd9c9a93d2d1c1eb6140ad168002a0
I025 218fbea81e9a9dd2d367c7c4dba6bc336f17e437a7cd62b5a0a03f1b48f41e94
I026 1bd5e370ba6a21d9b3d7885f83f213a26c50b40de74dbfe106f3a053f965c0c3
I027 469ff50e86f210cce55d463d39e5500a49551577246ee4921f9481a835ff190d
I028 3542ea33504f834283a5965d2fc1a15b4f076167072f85f7fd85c84189b2e539
I029 f2727e712650f901b1c16670a8602888f61fd9d2b646362500aef75088cc0c27
I030 7b987de1af7f1d9408024abd3e88b4e2e976649e39ee5d68202459e023295f31
```

Several whitelisted assets and `ItemCompleteCandidateContent.cs` are already dirty from accepted
earlier work. These disk hashes, not Git HEAD, are the package baseline. Do not checkout, reset,
rebuild or replace them from Git. Preserve all unrelated task-start content.

## 5. Field-level asset whitelist

For each profile, only these serialized fields may change:

```text
balanceDataRevision
coreCandidates
rarityVersions[*].eligibleCoreEffectIds
rarityVersions[*].visibleCoreEffectIds
```

The migrator must preserve every other field byte-equivalently or semantic-canonically, including:

```text
baseItemId
displayName
faMenTag
qiLeiTag
qiLeiPosition
primaryStatId
secondaryStatId
fixedAffixId
randomPoolId
randomAffixes
signatureAffix
candidateDisplay
```

It must also preserve these fields for every rarity version:

```text
rarity
versionKey
cultivationPotentialProfileId
dataMaturity
statRanges
candidateItemPower
candidateItemPowerOverridden
candidateDisplaySummary
```

This specifically protects accepted shape corrections and user-authored Candidate display
content. The package must fail if any non-core field changes.

For the catalog asset, only `balanceDataRevision` may change. The catalog identity, all 30 profile
references, stat/affix/build collections and their ordering must remain semantically identical.

## 6. Allowed new implementation files

The package may add exactly:

```text
Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionMigrator.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionMigrator.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionVerifier.cs.meta
```

Use the existing namespace:

```text
TalismanBag.EditorTools.ItemBalance
```

The migrator must provide a dedicated batch entry:

```text
TalismanBag.EditorTools.ItemBalance.ItemFourCoreCandidateDataCorrectionMigrator.MigrateStaticBatch
```

The verifier must provide:

```text
TalismanBag.EditorTools.ItemBalance.ItemFourCoreCandidateDataCorrectionVerifier.VerifyStaticBatch
```

The dedicated migrator must preflight all 30 profiles and the catalog before applying changes.
It must capture the complete task-start semantic state, build the complete migration plan, reject
all invalid plans before `SaveAssets`, and restore captured state if applying any row fails.
It must not call `[Setup] Rebuild All Candidate Assets` or rebuild whole profiles.

`ItemBalanceCandidateSeedBuilder.cs` may expose or support the dedicated core-only migration, but
its destructive full-rebuild path must not be invoked by this package.

## 7. Existing verifier migration

`ItemCompleteCandidateContentWorkbenchVerifier.cs` may be updated only to remove the stale
five-core assumptions and to recognize:

```text
core definitions = 120
ultimate definitions = 30
eligibility counts = 1/2/3/3/4
```

It must not overwrite the historical file:

```text
Docs/V0.4/Reports/ItemCandidateCoreEffects150.csv
```

If it emits a core matrix, it must target `ItemCandidateCoreEffects120.csv`. P1 must not run any
legacy verifier that writes non-whitelisted historical reports. The dedicated P1 verifier is the
authoritative QA entry for this package.

## 8. New report whitelist

Only these new report artifacts may be written:

```text
Docs/V0.4/Reports/ItemFourCoreCandidateDataCorrectionReport.md
Docs/V0.4/Reports/ItemFourCoreCandidateDataCorrectionSpec.csv
Docs/V0.4/Reports/ItemFourCoreCandidateMigrationLedger.csv
Docs/V0.4/Reports/ItemFourCoreRarityEligibilityMatrix150.csv
Docs/V0.4/Reports/ItemFourCoreCandidateCanonicalDelta.csv
Docs/V0.4/Reports/ItemFourCoreCandidateNonCoreFieldGuard.csv
Docs/V0.4/Reports/ItemFourCoreCandidateLeakCheckReport.md
Docs/V0.4/Reports/ItemCandidateCoreEffects120.csv
```

Do not add `.meta` files under `Docs`.

The historical five-core matrix is protected:

```text
Docs/V0.4/Reports/ItemCandidateCoreEffects150.csv
SHA-256 5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188
```

It must remain byte-identical and be referenced by the migration report as:

```text
SUPERSEDED_EXPANSION_DRIFT_EVIDENCE
```

## 9. Migration ledger requirements

`ItemFourCoreCandidateMigrationLedger.csv` must contain exactly 150 data rows: five task-start
rows for each `I001-I030`.

Required dispositions:

```text
01       KEEP_CANONICAL
02       KEEP_CANONICAL
03       KEEP_CANONICAL
04       SUPERSEDED_EXPANSION_DRIFT
ultimate KEEP_CANONICAL
```

Each row must include at minimum:

```text
baseItemId
oldCandidateDefinitionId
oldNodeKind
oldUnlockLevel
oldRequiredRarity
oldSemanticHash
disposition
newCandidateDefinitionId
newNodeKind
newUnlockLevel
newRequiredRarity
newSemanticHash
notes
```

The 120 retained rows require equal old/new semantic hashes. The 30 `_04` rows have no new
canonical target and must never be silently mapped to Ultimate.

## 10. Required Candidate verifier assertions

The dedicated verifier must prove all of the following:

1. Profiles `30/30`; rarity versions `150/150`.
2. Candidate core rows `120/120`; four per ordinary base item.
3. Unique candidate IDs `120/120`.
4. Exact identity kinds `01/02/03/ULT`, `120/120`.
5. Active `_04` occurrences `0`.
6. Ultimate rows `30/30`; Ultimate is eligible and visible only at Orange.
7. Rarity matrix `150/150` with counts `1/2/3/3/4`.
8. Each rarity has exactly 30 versions.
9. `eligible == visible`; visible is a subset of eligible.
10. No null, empty or duplicate core identity.
11. No core RNG domain, weight, roll, reroll or random-selection API was added.
12. Retained `01/02/03/ULT` semantic hashes are unchanged `120/120`.
13. Non-core profile fields are unchanged `30/30`.
14. Catalog fields except revision are unchanged.
15. Candidate CoreBuild schema compilation is valid.
16. All 150 Candidate rolls and projections succeed.
17. Repeating the same complete input yields the same result and canonical signature.
18. I031 ordinary Candidate generation and core rows remain `0`.
19. P2 and P3 protected hashes are unchanged.
20. Exact output whitelist and no unapproved file modifications.

The verifier must report failures honestly. It may not update a baseline merely because current
output differs.

## 11. Canonical delta contract

Allowed changes:

- The 30 profile file hashes and catalog asset hash, limited to the field whitelist.
- Candidate CoreBuild schema content signatures affected by the corrected core sets.
- Purple 30 generated/projection core collections:
  `01/02/03/04 -> 01/02/03`.
- Orange 30 generated/projection core collections:
  `01/02/03/04/ULT -> 01/02/03/ULT`.
- Projection-set signatures containing corrected Purple or Orange instances.
- New P1 Candidate reports and ledgers.

Must remain unchanged:

- White, Green and Blue 90 instances retain their eligible/visible core collections. Under the
  same complete inputs, their generated and projection canonical signatures must remain equal to
  task-start values.
- All 150 instances retain stats, fixed/random affixes, BuildQualification, `itemInstanceId`,
  `baseItemId`, rarity, root seed, generation version and cultivation profile ID.
- RNG algorithm, FNV/SplitMix golden values, domain consumption order, Build probability and drop
  probability.
- Foundation `30 x 5 = 150`, Foundation canonical and I031 exclusion.
- Projection contract field/API/signature algorithm and independent QA fixture digests.
- Awakening identity/runtime and ItemSystem/Board canonicals are not migrated in P1. Their
  temporary incompatibility is `EXPECTED_PENDING_P2`; do not weaken their verifiers or accept new
  P2 baselines in this package.
- All UI, Scene, Prefab, font, Sprite and PNG hashes.

`ItemFourCoreCandidateCanonicalDelta.csv` must identify each allowed delta by real sample and
must separately prove protected fields unchanged. Unknown or unexpected deltas fail.

## 12. Protected source and asset hashes

These files must remain byte-identical:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceProfile.cs` | `fd2b3700b0a500fd7f1450c5e1acff5a90617f657251bd49136cf712005fb91d` |
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchCatalog.cs` | `8ac332b9905f912e8c6aaa9facb116fb49f4ab0523f0bad90a350682301d3428` |
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchCompiler.cs` | `9921fb3b607f19ebaff46af95df43b7dc1840fecb7f2b617bee287dea6a0378c` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Potential/ItemCorePotentialAndBuildEligibilitySchema.cs` | `0a3cc80b1dc047dfe6ba6caec83e01a0be69aed78f7697e7666b401ecfac2b99` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/DeterministicItemRandom.cs` | `021959b426899ff937dae82dfc563b15d52a83a7591d19374c0ddc67ecccf075` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs` | `19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemGeneratedInstanceSnapshot.cs` | `8ed5da129d0819978c98e0ef53deac273cc9dbc9d10f5dc9e28cbeb71fba18ef` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` | `f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionProvider.cs` | `c7f8d5f66c254c1c98fc7b4181d4d5d94276eac65f79cdbe5c7748b06009debd` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs` | `294e73d516189f541d0e1e92e0efad583a634980d4b4b10ad0d2117075cf224b` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs` | `b59996875b6f52010de1f0df8266bf97746083c1966fc7655fb6fb0a9276fbf5` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs` | `99eaee275cd90fd857dc9a5ec50420269e03229b60c39e94b8448d950d9c666f` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs` | `03568f24593dbda6cfb8986191823d649dd1298b3e2a3a26d0fba942cb37b400` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs` | `504b03cc9e5140a4e53a9543a9c1a22e7c481a4f60d5cafc9dcb7357e6f422c6` |
| `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs` | `65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84` |
| `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs` | `da81315cdcd71310ea43729a4ae043b9a61cf33f0e889c706bf81d5bcf24871b` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` | `89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity` | `8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |

Also protected:

- all other Scenes and Prefabs;
- all PNGs, core icons, fonts and visual assets;
- all Awakening/runtime/Board verifiers, reports and golden baselines;
- AGENTS.md and `Docs/LOCKED/*`;
- Enemy, CrossSystem, Capability, Battle, RunFlow, SaveData, Reward, Chapter and Boss;
- stat, affix, drop and Build-effect data and probability rules.

## 13. Required validation flow

Before migration:

1. Re-read project instructions and this Assignment.
2. Verify Assignment SHA supplied by the Guard handoff.
3. Verify all allowed-existing and protected task-start hashes.
4. Verify the 30-profile aggregate and per-file hashes.
5. Record the package-scoped dirty baseline. Never use Git HEAD to replace dirty files.
6. Capture pre-migration semantic hashes for all 150 old core rows, all non-core profile fields,
   all 150 generated instances and all 150 projections.

Execute only the dedicated migration:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
  -batchmode -quit
  -projectPath F:\Porject\TalismanBagBrawl
  -executeMethod TalismanBag.EditorTools.ItemBalance.ItemFourCoreCandidateDataCorrectionMigrator.MigrateStaticBatch
```

Then execute only the dedicated verifier:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
  -batchmode -quit
  -projectPath F:\Porject\TalismanBagBrawl
  -executeMethod TalismanBag.EditorTools.ItemBalance.ItemFourCoreCandidateDataCorrectionVerifier.VerifyStaticBatch
```

Required results:

```text
Unity compile PASS
Unity batch return code 0
CANDIDATE_FOUR_CORE_120_PASS
RARITY_ELIGIBILITY_OPTION_C_150_PASS
SURVIVING_CORE_PAYLOAD_120_UNCHANGED_PASS
NON_CORE_PROFILE_FIELDS_30_UNCHANGED_PASS
ROLL_PROJECTION_150_PASS
WHITE_GREEN_BLUE_CANONICAL_UNCHANGED_PASS
PURPLE_ORANGE_ALLOWED_DELTA_PASS
I031_EXCLUSION_PASS
P2_P3_PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_NOT_APPLICABLE
P2_NOT_STARTED
P3_NOT_STARTED
```

Run `git diff --check` only for package paths and report any unrelated global warning separately.
Do not clean, reset or reformat unrelated files.

## 14. Stop and return conditions

Stop without migration or restore the captured pre-migration state if:

- any task-start allowed/protected hash differs before work begins;
- any of the 30 profiles or 150 rarity versions is missing or duplicated;
- any surviving Ultimate semantic hash would change;
- any non-core profile field would change;
- any `_04` row would be merged into another payload;
- any operation requires schema, RNG, Roll, Projection, Awakening/runtime, UI or Scene changes;
- any historical report must be overwritten;
- the exact file whitelist cannot be maintained;
- Unity compilation or the dedicated verifier fails.

Do not weaken a verifier, update a protected baseline, write Known Zero for missing data or report
the package as PASS after a stop condition.

## 15. Completion sync

On successful development and QA, send directly to Item Guard and RepoOps:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-ItemFourCoreCandidateDataCorrection01

AssignmentSHA256:
<exact Guard-supplied SHA>

Result:
DEV_COMPLETE / QA_PASS

Markers:
CANDIDATE_FOUR_CORE_120_PASS
RARITY_ELIGIBILITY_OPTION_C_150_PASS
ROLL_PROJECTION_150_PASS
USER_HANDTEST_NOT_APPLICABLE
P2_NOT_STARTED
P3_NOT_STARTED

ModifiedFiles:
<exact whitelist files only>

Reports:
<all eight report paths>

Unity:
<compile result, batch command, exit code, log path>

ProtectedHashes:
<before/after evidence>

GitOperations:
NONE
```

No `git add`, commit, tag, push, reset or rollback is authorized. RepoOps must later select exact
package files and must not add whole directories.

