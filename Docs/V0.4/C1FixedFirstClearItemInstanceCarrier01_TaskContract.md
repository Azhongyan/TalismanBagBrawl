# V0.4-C1FixedFirstClearItemInstanceCarrier01 Task Contract

Status: `CONTAINED_ONE_GUARD / BOUNDARY_FROZEN`

## 1. Outcome

Provide the missing Item-owned immutable instance carrier for the released
Chapter 1 first-clear reward:

- the exact released `CAMPAIGN_NORMAL_LV1 / 1-1 / I002@white` reward source
  resolves to one stable ordinary Item instance identity;
- the result exposes a non-empty `itemInstanceId` and
  `instanceCanonicalSignature` that can be copied into the already-released
  `RewardEntry` / `RewardResult` contract;
- repeated resolution is byte-for-byte deterministic and does not roll,
  fabricate or persist Item state;
- Reward remains the owner of reward definition, claim and dedupe facts; Item
  remains the owner of Item instance identity and canonical Item facts.

Dependency chain:

1. this package must reach terminal automated PASS;
2. only then may Item Guard freeze and dispatch
   `V0.4-C1FormalItemSessionAndArrangementAuthority01`;
3. the later Mainline integration consumes both released Item seams.

## 2. Product Context

- `CAMPAIGN_NORMAL_LV1`
- Chapter: `bone_aspect_chapter_1`
- Stage: `1-1`
- Acquisition source: released fixed first-clear definition only
- Subject: ordinary Item `I002@white`, quantity 1

No Scene, UI, Save, Inventory, Drop roll, claim, progression or battle runtime
is part of this component package.

## 3. Task Class And User Review

- Class: `CONTAINED_ONE_GUARD / TECHNICAL_PREREQUISITE`
- Primary Guard: Item Guard
- Development Owner: one new visible, unarchived local Item development task
- User Review Gate: `NO`
- Marker: `NOT_REQUIRED / EXISTING_DOWNSTREAM_P0_AUTHORIZATION`
- User Decision Point: `NONE`
- Producer/balance gate: `NOT_APPLICABLE`; no gameplay value, probability,
  cadence or balance number changes
- Unity Technical Director gate: `NOT_APPLICABLE`; pure immutable Item
  contract/data with no unresolved Unity carrier or serialization choice

## 4. Ownership

- Item owns the carrier request validation, ordinary instance identity and
  instance canonical signature.
- `C1Lv1StarterItemBaselineCatalog` remains the read-only Item fact source for
  `I002@white` projection identity and released values.
- `ItemRarityInstanceFoundation` remains the read-only constructor/validator
  for an ordinary Item identity.
- Reward owns source definition, claim scope, dedupe and `RewardResult`.
  Reward may copy the accepted carrier output but must not construct Item
  identity or signature itself.
- The later formal Item session package will own entitlement, roster,
  arrangement and lighting truth. This package owns none of those states.

## 5. Exact Allowed Writes

Add only:

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FixedFirstClearItemInstanceCarrier.cs`
2. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FixedFirstClearItemInstanceCarrier.cs.meta`
3. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FixedFirstClearItemInstanceCarrierTests.cs`
4. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FixedFirstClearItemInstanceCarrierTests.cs.meta`

The Task Contract is Guard-owned and must not be edited by the development
task. No report file is required.

## 6. Forbidden Writes

- all existing Item runtime files, including baseline, generation, projection,
  catalog, shape, rarity, Roll, Build, Core, combat, ItemSystem and tray/board;
- all Reward/Drop contracts, data, claim/dedupe and producers;
- formal Inventory, Save, progression, RunFlow and campaign state;
- Battle/Enemy/Visual code and accepted I001/I002 values;
- CoreLoop Lab dev roster or any `PLAYTEST_VERTICAL_SLICE` seam;
- V02/V03 Inventory or Save code;
- Scene, Prefab, Resources, PNG/meta/importer, ProjectSettings and
  BuildSettings;
- AGENTS, LOCKED, Queue, Notion and Git operations.

## 7. Required Read-Only Sources And Baselines

| Path | Task-start SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1Lv1StarterItemBaseline.cs` | `A4C454FFCC0A3B5CE1BF2181A39B79CA73C809FD81FD5D3AA3257C7908A140C7` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/ItemRarityInstanceFoundation.cs` | `937F04F1392F3C502A3BF3757094B3F4DC4EEDFE5D12EFD4E6D814F844132356` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/ItemInstanceRarity.cs` | `145E2F9325C9F4064D56F563ED589E651029AE54551D9FB20AEA7B9234388AD9` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemGeneratedInstanceSnapshot.cs` | `8ED5DA129D0819978C98E0EF53DEAC273CC9DBC9D10F5DC9E28CBEB71FBA18EF` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs` | `19E989FFB535D8CD268C94D216300CC3D98D63B9FF13E50551A0AE2796ADA63E` |
| `Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs` | `606ACDC6538EEDA86F7631840A6ACBB47284368F198EF413293BB844833776C9` |
| `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs` | `3D3501203162FF8F5EF40467B3846E22403EC515B02B2207BB17B0D2FA9B9F4C` |
| `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardResult.cs` | `13A6698BD8D3B15CE21222DF0EDFB66D69E7F3B2BF09F08608AF5CD0E8687CCE` |
| `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractPrimitives.cs` | `60385BC84807B88122006948AB402FBA39C5D3EC5721D6A387DA842F3C7E05E3` |
| `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractValidator.cs` | `ED60A14BABEF252BE4D9E646EA9AD16712B770F119D67E7BDE2182170522A796` |

All four allowed product/test paths were absent at task start. Any existing
path or any drift in the read-only sources before first write is a verified
write conflict; stop without refreshing the baseline.

## 8. Patch Budget

- new runtime files: exactly 2 including `.meta`;
- new Editor test files: exactly 2 including `.meta`;
- modified existing files: 0;
- reports: 0;
- Scene/Prefab files: 0.

Any need to modify an existing Item or Reward file stops at
`ARCHITECTURE_REASSESS_REQUIRED`.

## 9. Immutable Carrier Contract

The implementation may follow local naming conventions, but it must expose one
immutable request/result/snapshot boundary under the Item-owned
`TalismanBag.Items.CampaignBaseline` namespace.

Accepted request facts:

- product context `CAMPAIGN_NORMAL_LV1`;
- chapter `bone_aspect_chapter_1`;
- stage `1-1`;
- reward source definition `c1.first_clear.1-1.i002.white.v1`;
- source rule version `1`;
- released claim scope and dedupe policy identities from the read-only
  first-clear definition;
- subject base Item `I002`;
- rarity version key `I002@white`;
- quantity `1`;
- released Item projection canonical signature and Item catalog canonical
  signature.

Accepted output facts:

- schema/version and deterministic carrier algorithm ID;
- all normalized source identities above;
- exact stable `itemInstanceId`: `c1fi_1_1_i002_white_v1`;
- ordinary identity schema, base Item `I002`, rarity `white`, generation
  version `1`, fixed technical root seed `1102001`, and no fabricated
  cultivation profile;
- identity canonical payload/signature from the existing ordinary identity
  foundation;
- released `I002@white` projection source canonical signature;
- immutable carrier canonical payload;
- `instanceCanonicalSignature` as lowercase SHA-256 of that complete canonical
  carrier payload.

The technical root seed is an identity-domain constant only. It must not enter
the QA-only Roll engine and must not create stats, affixes, Build qualification
or Core facts.

## 10. Required Behavior

1. Resolve the exact released first-clear source through read-only current
   definition and Item baseline facts before constructing the carrier.
2. Create/validate the ordinary identity through
   `ItemRarityInstanceFoundation`; do not directly instantiate its internal
   snapshot and do not modify the foundation.
3. Repeated accepted requests in any call order return the exact same
   `itemInstanceId`, canonical payload and `instanceCanonicalSignature`.
4. The canonical payload is length-delimited or equivalently unambiguous,
   invariant-culture, order-stable and includes every identity/source
   signature needed to detect stale or mismatched input.
5. Wrong context/chapter/stage/source/rule/claim/dedupe/base Item/rarity/
   quantity/profile revision/projection signature/catalog signature rejects
   fail-closed with no partial snapshot.
6. The result is immutable and exposes a stable diagnostic code. Unknown,
   unsupported or mismatched facts are not converted to empty success or zero.
7. `ItemInstanceRollEngine` and `ItemInstanceProjectionQaFixture` are never
   called. They explicitly remain QA-only and not formal generation data.
8. Affixes, Build qualification, Core potential, cultivation progression,
   placement, lighting and combat effects are not authored by this carrier.
9. A `RewardEntry` built in the focused test from the accepted output validates
   as an ordinary generated Item entry without Reward calculating or replacing
   either Item instance field.
10. No static mutable session, claim or entitlement state is retained.

## 11. Failure And Edge Cases

- null request or missing identifier: reject with stable diagnostic;
- stale released source signature: reject, no fallback;
- I001, I003-I031 or a non-white I002 request: reject;
- duplicate calls: deterministic identical success, not a new instance;
- culture change: output remains byte-identical;
- unsupported formal generation detail: remain absent/Unknown, never zero;
- any need for RNG, Save, claim or roster ownership: stop.

## 12. Verification

Minimum evidence:

- scoped current-source Runtime and Editor offline compile;
- focused deterministic tests using actual carrier code;
- exact accepted identity and canonical signature stability across repeated
  calls, request reconstruction and at least two cultures;
- rejection matrix for every frozen request/source identity;
- existing foundation validation is used and succeeds for the exact carrier;
- focused construction and validation of one `RewardEntry` using the carrier
  output;
- source/static proof that QA Roll/fixture, mutable state and forbidden
  namespaces are not used;
- four-file whitelist, whitespace and protected baseline check.

Explicitly not required:

- Unity batch, Fresh Play or user handtest;
- Scene/Prefab authoring;
- CSV/report generation;
- integration with Reward producer or formal arrangement.

## 13. Process Ownership

`UNITY_NOT_REQUIRED`

Do not start, wait for, poll, close or clean Unity. Do not run Git commands.
Preserve all unrelated dirty/untracked project state.

## 14. Completion And Stop

Self-close at:

`C1_FIXED_FIRST_CLEAR_ITEM_INSTANCE_CARRIER_PASS / PACKAGE_COMPLETE`

Terminal sync to Item Guard must include exact changed files, public API names,
the fixed `itemInstanceId`, final `instanceCanonicalSignature`, compile/test
result, protected baseline result, and Unity/Git-none confirmation.

Stop at `ARCHITECTURE_REASSESS_REQUIRED` if implementation requires:

- modifying any existing Item or Reward source;
- inventing formal stats/affixes/Build/Core through the QA Roll path;
- Save/claim/progression/roster/arrangement state;
- any Scene, Prefab, UI or cross-system runtime adapter;
- more than the exact four product/test paths.

Package 2 remains `NOT_STARTED / NOT_DISPATCHED` until this package reaches the
terminal PASS marker above.
