# ItemInstanceProjectionContract01 Report

- Package: `V0.4-ItemInstanceProjectionContract01`
- Result: FAIL
- Marker: `ITEM_INSTANCE_PROJECTION_CONTRACT01_FAIL`
- Single schema: `ItemInstanceProjectionContractSnapshot.v1`
- Set schema: `ItemInstanceProjectionSetSnapshot.v1`
- Source schema: `ItemGeneratedInstanceSnapshot.v1`
- QA status preserved: `QA_FIXTURE_ONLY|NOT_BALANCE_APPROVED|NOT_FORMAL_GENERATION_DATA`
- Spec: 155/156 PASS
- Instances: 6
- Queries: 9
- Leak total: 0

## Single-instance fields

`schemaId / sourceSchemaId / sourceGenerationAlgorithmId / generationDataStatus / itemInstanceId / baseItemId / rarity / rarityKey / generationVersion / rootSeed / cultivationPotentialProfileId / stats / affixes / eligibleCoreEffectIds / visibleCoreEffectIds / buildQualification / sourceCanonicalSignature`

Nested stat fields: `statId / rawUnits`.
Nested affix fields: `slotId / slotKind / affixId / affixValueProfileId / rawUnits`.

## Set and queries

The set stores an Ordinal-sorted read-only projection list, requires unique itemInstanceId, preserves same baseItemId + rarity instances, and exposes:
- `QueryByItemInstanceId(itemInstanceId)`
- `QueryByBaseItemId(baseItemId)`
- `QueryByBaseItemIdAndRarity(baseItemId, rarity)`
- Same I001+green instances retained: 2

## Consumer boundary

- Overall Item System may read baseItemId for Catalog lookup, itemInstanceId for instance identity, rarity, Build qualification, core potential IDs, stats and affixes.
- Future detail projection may read the same raw facts, but this package does not modify ItemDetailViewModel or ItemDetailProjectionComposer and does not emit localized player value text.
- No placement, lighting, cultivation, combat, reward, inventory or persistence state is included.

## Canonical signature

- Both signatures use explicit length-prefixed fields, Ordinal stable ordering and InvariantCulture numeric formatting.
- Single digest: `AA4E81E407B22698`
- Set digest: `F7BE30B5F6A3348F`
- Same-input repeat: `1000/1000 PASS` when this report is PASS.
- Reversed set input is identical; different itemInstanceId is different; stat/affix/core/Build/source signature mutations change the signature.

## Read-only and identity results

- Top-level set, stats, affixes, eligible/visible core IDs, query results and validation errors reject IList mutation.
- Mutating the source after projection does not change an existing projection.
- I031 is rejected and cannot enter a successful set.
- No `itemId` alias or `placementId` field exists on new Runtime contract types.

## Compatibility

- ItemSystem schema remains `ItemSystemSnapshot.v2`; BuildDebugSignature remains present and was not modified by this package.
- ItemDetailProjectionComplete report remains PASS; no ItemDetailViewModel, Composer, view or prefab modification is made.

## Formal systems not connected

`Reward / RunFlow / Inventory / SaveData / Battle / Boss / formal drop UI / formal detail UI / Scene / Prefab / BuildSettings / cultivation unlock / lighting / Build counting or activation`

## QA isolation

- Fixtures are generated through ItemInstanceRollEngine using `QA_FIXTURE_ONLY|NOT_BALANCE_APPROVED|NOT_FORMAL_GENERATION_DATA`.
- Invalid sources are created only inside this Editor verifier through non-public reflection; Runtime exposes no success-snapshot forgery API.

## Historical regressions

- Foundation: FAIL
- StatRange: FAIL
- CorePotential: FAIL
- AffixSchema: FAIL
- RollEngine: FAIL
- DropSandbox: FAIL
- SimulationValidator: FAIL
- ItemInnerDataCatalog: PASS
- ItemSystemValidatorAndSnapshot: PASS
- BuildSynergyCore: PASS
- CoreAwakeningPreview: FAIL
- ItemSkillTriggerContract: PASS
- ItemDetailProjectionComplete: FAIL
- JuNian Lighting: PASS
- ArrayBonus: PASS
- Foundation: 159/159
- StatRange: 39/39
- CorePotential: 69/69
- AffixSchema: 88/88
- RollEngine: 72/72
- DropSandbox: 87/87 Spec, 6/6 Determinism
- Simulation: 65/65 Spec, 46/46 Distribution, 12/12 Determinism, invariant=0

## Errors

- detail-viewmodel-unmodified-contract: expected 'ItemDetailProjectionComplete PASS', actual 'report marker check', code 'ASSERTION_FAILED'.
- Historical regression report is not PASS: Foundation.
- Historical regression report is not PASS: StatRange.
- Historical regression report is not PASS: CorePotential.
- Historical regression report is not PASS: AffixSchema.
- Historical regression report is not PASS: RollEngine.
- Historical regression report is not PASS: DropSandbox.
- Historical regression report is not PASS: SimulationValidator.
- Historical regression report is not PASS: CoreAwakeningPreview.
- Historical regression report is not PASS: ItemDetailProjectionComplete.
