# V0.4-ItemFourCoreAwakeningRuntimeContractCorrection01 Assignment

Status: `READY_FOR_DEV / ITEM_GUARD_APPROVED / USER_OPTION_C_FROZEN`

Package: `V0.4-ItemFourCoreAwakeningRuntimeContractCorrection01`

Assignment freeze date: `2026-07-23 Asia/Shanghai`

Primary Guard: `Item System Guard`

Item Algorithm Guard: `ITEM_ALGORITHM_GUARD_ACCEPT_ITEMFOURCORECANDIDATEDATACORRECTION01`

## 0. Receipt and stop point

Accepted assignment receipt:

```text
ITEM_GUARD_PASS_ITEMFOURCOREAWAKENINGRUNTIMECONTRACTCORRECTION01
```

Required completion state:

```text
DEV_COMPLETE
QA_PASS
AWAKENING_FOUR_DEFINITIONS_120_PASS
RUNTIME_IDENTITY_CATALOG_V2_120_PASS
RUNTIME_STATE_V2_FOUR_ROWS_PASS
REAL_RUNTIME_STATE_ASSEMBLY_PASS
USER_HANDTEST_NOT_APPLICABLE
P3_NOT_STARTED
PREFAB_MIGRATION_NOT_STARTED
```

This package corrects the Awakening and instance runtime-state authority. It does not project
core effects into Item Detail and does not modify presentation.

## 1. Accepted prerequisites

```text
V0.4-ItemFourCoreAuthorityAndMigrationDecision01
  PASS / USER_OPTION_C_FROZEN

V0.4-ItemFourCoreCandidateDataCorrection01
  ITEM_GUARD_ACCEPTED / ITEM_ALGORITHM_GUARD_ACCEPTED / QA_PASS
  Candidate rows 120/120
  rarity eligibility 1/2/3/3/4 across 150 versions
  USER_HANDTEST_NOT_APPLICABLE
```

Accepted independent P1 review:

```text
ITEM_ALGORITHM_GUARD_ACCEPT_ITEMFOURCORECANDIDATEDATACORRECTION01
```

Required P1 evidence:

```text
Docs/V0.4/Reports/ItemFourCoreCandidateDataCorrectionReport.md
SHA-256 c2a2c77f6f9485ab8053deac1e870009327562f51bbb9c3c101a108b45878d28

Docs/V0.4/Reports/ItemCandidateCoreEffects120.csv
SHA-256 96cb7ba61376c1a1ab5bdc1ca08a42785bea0f4f864e6f89129d0f9a88fc2284
```

The P1 profile aggregate is:

```text
71fc7080eceaf9adf2aa47508e4900d00fae91d8f8158fb1a2618b69241a8143
```

Use the aggregate algorithm frozen in the P1 Assignment. P2 must not modify any profile.

## 2. Frozen formal node authority

For each ordinary base item `I001-I030`, formal Awakening definitions and runtime identities are
exactly:

```text
candidate_core_ixxx_01       -> Ixxx_CORE_01  / Core1    / Lv10 / white
candidate_core_ixxx_02       -> Ixxx_CORE_02  / Core2    / Lv20 / green
candidate_core_ixxx_03       -> Ixxx_CORE_03  / Core3    / Lv30 / blue
candidate_core_ixxx_ultimate -> Ixxx_CORE_ULT / Ultimate / Lv40 / orange
```

Stable formal order is explicitly:

```text
Core1, Core2, Core3, Ultimate
```

Do not derive the order from enum numeric values, array position, display name, PNG name or ID
suffix parsing.

`I031` has zero ordinary Awakening definitions, zero identity rows and zero ordinary runtime rows.

## 3. Core4 compatibility ruling

The serialized enum values must remain:

```text
Core1 = 0
Core2 = 1
Core3 = 2
Ultimate = 3
Core4 = 4
```

Do not delete or renumber `Core4`. It remains a deprecated/reserved compatibility identity for
historical evidence.

Current formal producers and contracts must produce:

```text
candidate_core_*_04 active rows = 0
Ixxx_CORE_04 formal definitions = 0
Core4 identity-catalog rows = 0
Core4 runtime-state rows = 0
```

If an input attempts to insert Core4 into the corrected formal catalog, validation must reject it
as `SUPERSEDED_EXPANSION_DRIFT`; it must not ignore, relabel or merge it into Ultimate.

## 4. Explicit contract version upgrade

The five-row contracts were accepted under:

```text
ItemCoreEffectIdentityCatalogSnapshot.v1
ItemInstanceCoreEffectRuntimeStateSnapshot.v1
```

Their Assignments, reports, matrices and hashes remain historical evidence. P2 must not silently
rewrite the version string or pretend v1 was four-core.

The current constructors must now emit:

```text
ItemCoreEffectIdentityCatalogSnapshot.v2
ItemInstanceCoreEffectRuntimeStateSnapshot.v2
authorityRevision = ITEM_FOUR_CORE_AWAKENING_RUNTIME_CONTRACT_CORRECTION01_R1
```

The existing public type names may remain to avoid a duplicate runtime owner. The contract source
must retain explicit legacy constants naming the two v1 schema IDs. Current v2 canonical payloads
must include both `schemaId` and `authorityRevision`.

`ItemCoreEffectCultivationRosterSnapshot.v1` is structurally unaffected and remains v1.
`ItemSystemSnapshot.v2` is variable-length and remains v2; do not change its schema.

## 5. Activation semantics

The authoritative final runtime `activeFact` is:

```text
rarity eligible
AND cultivation unlocked
AND item is lit on Board
```

Rules:

- Inventory retains four static identity rows, names, descriptions, eligibility and visibility.
- Inventory placement, lit and active facts are `NotApplicable`.
- Board unlit has a Known False active fact for every complete row.
- Board lit has Known True only when eligibility and cultivation unlock are also true.
- An ineligible row remains Known False even if its level threshold is reached and the item is lit.
- Missing identity, cultivation, binding or projection facts remain Unknown/Invalid according to
  the existing completeness rules. They must not become false or zero.
- `visibleFact` remains distinct from `eligibleFact`.
- `node.isActive` from the lower Awakening resolver is not sufficient by itself. The v2 runtime
  assembler must apply the complete three-fact formula and validate contradictions.

Required rarity examples at Lv40 and lit:

```text
White  -> Core1 active only
Green  -> Core1/Core2 active
Blue   -> Core1/Core2/Core3 active
Purple -> Core1/Core2/Core3 active; Ultimate ineligible and inactive
Orange -> Core1/Core2/Core3/Ultimate active
```

BoardSandbox's real dev roster remains Orange Lv40. It must yield four active rows when lit and
four unlocked but inactive rows when unlit.

## 6. Exact existing-file whitelist

Only these existing files may be modified:

| Path | Task-start SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs` | `294e73d516189f541d0e1e92e0efad583a634980d4b4b10ad0d2117075cf224b` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs` | `b59996875b6f52010de1f0df8266bf97746083c1966fc7655fb6fb0a9276fbf5` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs` | `99eaee275cd90fd857dc9a5ec50420269e03229b60c39e94b8448d950d9c666f` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs` | `03568f24593dbda6cfb8986191823d649dd1298b3e2a3a26d0fba942cb37b400` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs` | `504b03cc9e5140a4e53a9543a9c1a22e7c481a4f60d5cafc9dcb7357e6f422c6` |

These paths include accepted dirty/untracked work from earlier packages. Their task-start disk
hashes, not Git HEAD, are authoritative. Do not checkout, reset, recreate or clean them.

`ItemSystemBattleSandboxBoardAdapter.cs` may change only its identity-catalog schema/cardinality
acceptance from v1/150 to v2/120 and related diagnostic text. It must not change roster generation,
art, tray, Board behavior, detail behavior or installation flow.

## 7. Allowed new implementation files

Add exactly:

```text
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemFourCoreAwakeningRuntimeContractCorrectionVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemFourCoreAwakeningRuntimeContractCorrectionVerifier.cs.meta
```

Required batch entry:

```text
TalismanBag.EditorTools.ItemSandbox.ItemFourCoreAwakeningRuntimeContractCorrectionVerifier.VerifyStaticBatch
```

Do not add a second runtime-state assembler, second Board authority or migration Manager. Correct
the existing stateless assembler and existing single Board authority path.

## 8. Historical verifier and evidence isolation

The following historical verifiers must remain byte-identical and must not be executed by P2:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs` | `55963d1d5e2ac661ca7b2627778c92fd5f7e993d50bfa9bfcf28d918332050f0` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs` | `c6513768d1be22c15594d790d494d77331b561a947974d7d38d74fa7aadfa479` |

They describe accepted five-core v1 history and are expected to be incompatible with v2. Do not
weaken them, update their golden hashes or overwrite their reports.

Protected historical artifacts:

| Path | SHA-256 |
|---|---|
| `Docs/V0.4/ItemCoreAwakeningCore4NodeExpansion01_Assignment.md` | `8f9af4b60b601f64a59fe8741c95792d8222857b66403843b70340ed46e7a03f` |
| `Docs/V0.4/ItemCoreEffectIdentityAndRuntimeStateContract01_Assignment.md` | `34d4816ea5e43d64583767ddda6ae448295a8ec5fdeb7a5d7ba299b34c0db756` |
| `Docs/V0.4/Reports/CoreAwakeningPreviewReport.md` | `678298b7f8f50fac224307834aeed320a5b2b41fba970f183601178788d65b9a` |
| `Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv` | `e7081371aee2a7fe0600cb062726d9ce035e4378af1df52ec9ae5ad6af54afce` |
| `Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md` | `ceb7d5b6c44913fe832ce82a5c86bd2c8fa68a6f8baff7459b23f555531cd911` |
| `Docs/V0.4/Reports/ItemCoreEffectIdentityMatrix.csv` | `c5867237b758b71f90368e3c10c54a68b22d9b7f6ea549ae692df38826a13f32` |
| `Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractReport.md` | `05a1c68c18019f9bea245a4cf8810ec7aa03e07a273e4055386d5dc94144d05d` |
| `Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractSpec.csv` | `3cad9f7e8b02824dab7bd2f300c9e9544713407d5e4a872247d8f3ede80605b2` |
| `Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractLeakCheckReport.md` | `7ba5e1f5b4e2b55ef3eeebb8d4a3a60a6b41e043f1093566ef191b81009f0080` |

New reports must label the v1 five-core evidence:

```text
SUPERSEDED_BY_ITEM_FOUR_CORE_AUTHORITY_OPTION_C
HISTORICAL_EVIDENCE_RETAINED
```

## 9. P1 data protection

The following P1 outputs and sources must remain byte-identical:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs` | `15d7a0cbf1b60969e6c27030a7d1d8ccd1d733fb73421396bee5a8ee21382b4b` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemBalanceCandidateSeedBuilder.cs` | `46615405ed25f2939259be0d449727118574dc2cf4f0dc2e6dbd891da4d1cc4f` |
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemBalanceWorkbenchValidation.cs` | `f4cfa16238ab5454cbc41ac0892cffe4faae5fe8e02cedc3335d175fc5ea5cdc` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemCompleteCandidateContentWorkbenchVerifier.cs` | `7432f989a14b36fc4e35abd7f04a64684af9a7761e2d586941dd4c4cf134de7c` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionMigrator.cs` | `910bb52db53c408036366a6f75e66e8e53eadad7f2b302e752abcc336c28f03c` |
| `Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemFourCoreCandidateDataCorrectionVerifier.cs` | `005cf5fbd861f06f94fa1a19ba6b7070bf0261957978fec7bdfcf1491e6acda6` |
| `Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset` | `d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed` |
| `Docs/V0.4/Reports/ItemCandidateCoreEffects120.csv` | `96cb7ba61376c1a1ab5bdc1ca08a42785bea0f4f864e6f89129d0f9a88fc2284` |
| `Docs/V0.4/Reports/ItemCandidateCoreEffects150.csv` | `5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188` |

All 30 profiles must retain aggregate:

```text
71fc7080eceaf9adf2aa47508e4900d00fae91d8f8158fb1a2618b69241a8143
```

Do not run the P1 verifier after changing P2 files because its historical protected-hash gate
intentionally targets the pre-P2 state. P2 must verify Candidate 120/120 directly and verify all
P1 source/asset hashes instead.

## 10. Additional protected runtime and presentation files

These files must remain byte-identical:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs` | `6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20` |
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827` |
| `Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs` | `f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` | `f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs` | `19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` | `89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity` | `8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |

Also protected: IF01, Qualified Build assembler, P6, all Item Detail adapters/ViewModels, all other
Scenes/Prefabs/UI/RectTransforms/fonts/Sprites/PNGs, Enemy, Capability, formal Battle, RunFlow,
SaveData, Reward, Chapter, Boss, AGENTS.md and `Docs/LOCKED/*`.

## 11. Report whitelist

The dedicated verifier may create only:

```text
Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeContractCorrectionReport.md
Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeContractCorrectionSpec.csv
Docs/V0.4/Reports/ItemFourCoreAwakeningIdentityMatrix120.csv
Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeStateMatrix.csv
Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeCanonicalDelta.csv
Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeMigrationLedger.csv
Docs/V0.4/Reports/ItemFourCoreAwakeningRuntimeContractCorrectionLeakCheckReport.md
```

Do not create `.meta` files under `Docs`. Do not overwrite any historical report.

The migration ledger must contain 150 legacy identity rows:

```text
120 retained 01/02/03/ULT rows -> v2 current identity
30 Core4 rows -> SUPERSEDED_EXPANSION_DRIFT with no target
```

Retained Candidate payload identities and display text must match P1. The runtime package may
change Awakening ID set and schema/canonical signatures; it may not change Candidate payload.

## 12. Canonical delta contract

Allowed changes:

- Awakening definition-set canonical content: 150/five-node to 120/four-node.
- Identity-catalog canonical: v1/150 to v2/120.
- Runtime-state canonical: v1/five rows per ordinary instance to v2/four rows.
- `ItemSystemSnapshot.v2` content/debug signature for snapshots whose Awakening node arrays lose
  Core4. The schema and signature algorithm must remain unchanged.
- Board current core-effect runtime signature and source identity signature.
- New P2 reports.

Must remain unchanged:

- Candidate profile, Candidate core identity, Roll, Projection and RNG algorithms and P1 hashes.
- `itemInstanceId`, `baseItemId`, rarity, root seed, stats, affixes and BuildQualification.
- Cultivation roster schema/source/levels.
- IF01, Qualified Build, Board layout, lighting, Build and P6 facts.
- Atomic Board commit and no-op semantics.
- Item Detail model, presentation, Scene and Prefab.

The new canonical-delta report must distinguish permitted v1-to-v2 changes from protected-source
changes. An unexplained delta is a failure.

## 13. Verifier cases

The new verifier must prove:

1. P1 Candidate authority is still 120 rows with exact `01/02/03/ULT`.
2. Default Awakening definitions are 120/120, four per `I001-I030`.
3. I031 definitions are zero.
4. Core4 enum value remains `4`, but formal Core4 definitions/identity/runtime rows are zero.
5. Identity mappings are exact `candidate ID + baseItemId + machine nodeKind`, not positional.
6. Identity catalog is `v2`, authority revision matches, rows are 120/120 and deterministic.
7. Runtime state is `v2`, authority revision matches and each complete ordinary item has four rows.
8. Option C eligibility is preserved at runtime for White/Green/Blue/Purple/Orange.
9. Lv1/10/20/30/40 cultivation thresholds are unchanged.
10. Complete Board active facts use `eligible AND unlocked AND lit`.
11. Inventory active/lit/placement facts are NotApplicable while static content remains.
12. Board unlit, Board lit, move and return-to-inventory rebuild correctly.
13. Missing cultivation remains Unknown; it is not defaulted to Lv1 in v2 output.
14. Duplicate, orphan, stale or mismatched identities are Invalid.
15. A supplied Core4 definition or identity row is rejected as superseded drift.
16. I031 has zero ordinary identity/cultivation/runtime rows.
17. External collections are immutable.
18. Ordinal/InvariantCulture ordering and reversed-input canonical determinism pass.
19. BoardAuthority still atomically commits ItemSystem/IF01/QualifiedBuild/CoreRuntime/P6.
20. Invalid assembly preserves all five previous references.
21. No-op preserves references and counters.
22. Real BattleSandbox dev roster contains 30 Orange Lv40 ordinary instances and uses v2/120.
23. P1, presentation and historical-evidence hashes are unchanged.
24. Exact output whitelist, LeakCheck and package-scoped `git diff --check` pass.

Required real samples include `I001`, `I004`, `I009`, `I030` and `I031`.

## 14. Validation flow

Before editing:

1. Re-read project rules and this Assignment.
2. Verify the exact Assignment SHA supplied by Guard.
3. Verify all allowed/protected task-start hashes and the P1 profile aggregate.
4. Record package and unrelated dirty/untracked baselines. Do not clean them.
5. Verify historical report hashes before any Unity run.

Run only the new P2 verifier:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
  -batchmode -quit
  -projectPath F:\Porject\TalismanBagBrawl
  -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemFourCoreAwakeningRuntimeContractCorrectionVerifier.VerifyStaticBatch
```

Required markers:

```text
AWAKENING_FOUR_DEFINITIONS_120_PASS
RUNTIME_IDENTITY_CATALOG_V2_120_PASS
RUNTIME_STATE_V2_FOUR_ROWS_PASS
OPTION_C_ACTIVE_FORMULA_PASS
CORE4_RESERVED_ZERO_FORMAL_ROWS_PASS
V1_EVIDENCE_RETAINED_PASS
INVENTORY_BOARD_TRANSITIONS_PASS
I031_EXCLUSION_PASS
ATOMIC_COMMIT_PASS
NO_OP_STABILITY_PASS
P1_PROTECTED_HASHES_PASS
PRESENTATION_PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
REAL_RUNTIME_STATE_ASSEMBLY_PASS
USER_HANDTEST_NOT_APPLICABLE
P3_NOT_STARTED
PREFAB_MIGRATION_NOT_STARTED
```

Do not run:

- the historical CoreAwakening verifier;
- the historical v1 identity/runtime verifier;
- the P1 verifier with its intentionally pre-P2 protected-hash baseline;
- any Scene Builder or UI verifier that writes assets.

## 15. Stop conditions

Stop and report without claiming PASS if:

- an allowed/protected task-start hash differs before editing;
- P1 Candidate rows are not exactly 120 or profile aggregate differs;
- the fix requires changing Candidate, Roll, Projection, ItemSystemSnapshot schema or BoardAuthority;
- any Core4 row remains in formal definitions, identity catalog or runtime output;
- Ultimate identity, text or Candidate payload changes;
- any v1 historical artifact would be overwritten;
- the final activation formula cannot be proven without inventing missing facts;
- any Scene, Prefab, UI or Item Detail file changes;
- Unity compile or the dedicated verifier fails;
- exact file/report whitelist cannot be maintained.

No protected baseline may be updated to make a failure pass.

## 16. Completion sync

On success, send directly to Item Guard and RepoOps:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-ItemFourCoreAwakeningRuntimeContractCorrection01

AssignmentSHA256:
<exact Guard-supplied SHA>

Result:
DEV_COMPLETE / QA_PASS

Markers:
<all required markers>

ModifiedFiles:
<exact whitelist>

Reports:
<all seven report paths>

Unity:
<compile result, batch command, exit code and log>

ProtectedHashes:
<before/after evidence>

GitOperations:
NONE
```

No `git add`, commit, tag, push, reset or rollback is authorized. P3 and Prefab migration remain
not started until P2 Guard acceptance.
