# V0.4-BoneAspectMechanicVocabularyExtension01 Assignment

## 1. Package

```text
Package: V0.4-BoneAspectMechanicVocabularyExtension01
Guard marker: ENEMY_GUARD_ASSIGNMENT_BONEASPECTMECHANICVOCABULARYEXTENSION01
Risk: YELLOW / VOCABULARY_AUTHORITY_EXTENSION
Scope: ADDITIVE_VOCABULARY_CONTRACT_DATA_ONLY
Scene handtest: NOT_REQUIRED
Next package: NOT_RELEASED
```

This Assignment is authorized by:

```text
DIRECT_USER_AUTHORIZATION_ENEMY_D1_D2_IMPLEMENTATION_SEQUENCE
```

The package adds exactly seven neutral Enemy vocabulary concepts accepted from the
BoneAspect Gap Survey. It does not implement mechanics, bind carriers, start a
Boss state machine, or connect BattleSandbox.

## 2. Accepted Preconditions

```text
V0.4-EnemyMechanicVocabulary01
GUARD_ACCEPTED / QA_PASS
Schema: EnemyMechanicVocabulary.v1
Base canonical:
sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833

V0.4-BoneAspectContentDesignLock01-GuardFix01
GUARD_ACCEPTED / CONTENT_DESIGN_LOCK_PASS

V0.4-BoneAspectCarrierPresentationCatalog01
GUARD_ACCEPTED / QA_PASS

V0.4-BoneAspectMechanicGapSurvey01-GuardFix01
GUARD_ACCEPTED / QA_PASS
Canonical:
sha256:f954a947fe835979bf85c9a65e8a37f0892c3d635805737e0076a7df0c3a0a3e

V0.4-BoneAspectBossArtMechanicLineageSurvey01
GUARD_ACCEPTED / STATIC_QA_PASS
Canonical:
sha256:4d6ff9619133ed654db8f26941c2eec6a3016a5f1f9abb208823262039ec24e0
```

BA-D1 and BA-D2 decision overlays remain read-only accepted evidence. BA-D3 and
BA-D4 remain `USER_DECISION_REQUIRED / NOT_SELECTED`. None of the four decision
records is implemented or modified by this package.

## 3. Authority Model

The accepted E02 default vocabulary remains immutable.

```text
DefaultEnemyMechanicVocabularyCatalog
  + EnemyMechanicVocabularyExtension.v1 delta
  -> explicit read-only composition
  -> one validated EnemyMechanicVocabularySnapshot
```

Required rules:

1. Do not modify `DefaultEnemyMechanicVocabularyCatalog`.
2. Do not modify E02 model, validator, provider, verifier, reports, or legacy mappings.
3. The extension is an additive vocabulary-domain truth, not a second default catalog.
4. The extension declares its exact base schema and version.
5. Composition is explicit and side-effect-free. It must not replace or mutate the
   global E02 default.
6. No runtime, carrier, encounter, skill, phase, pressure, readiness, player hint,
   or formal-flow consumer may automatically consume the extension in this package.
7. A verifier may explicitly compose base plus extension for validation only.

Frozen extension identity:

```text
SchemaId = EnemyMechanicVocabularyExtension.v1
SchemaVersion = 1
ExtensionId = extension.bone_aspect.mechanic_vocabulary_01
BaseSchemaId = EnemyMechanicVocabulary.v1
BaseSchemaVersion = 1
devOnly = true
isEnabled = false
entersFormalFlow = false
runtimeImplemented = false
```

## 4. Exact Seven Entries

The following values are exact. The development task must not rename, merge,
broaden, split, or infer an eighth entry.

| Candidate evidence | Category | Accepted stable key | Developer label | Neutral description | Survey rows |
| --- | --- | --- | --- | --- | --- |
| `ba_gap_candidate.charge_attack` | `Mechanic` | `mechanic.charge_attack` | `冲撞攻击` | `敌方以突进或冲撞作为可识别攻击意图的中立机制概念。` | `GAP-003` |
| `ba_gap_candidate.contested_mark` | `Mechanic` | `mechanic.contested_mark` | `争夺标记` | `敌方围绕某目标建立可争夺关系标记的中立机制概念，不定义资源结算。` | `GAP-015` |
| `ba_gap_candidate.damage_reduction` | `Mechanic` | `mechanic.damage_reduction` | `减伤` | `敌方以非护盾方式降低承伤的中立机制概念。` | `GAP-016` |
| `ba_gap_candidate.possession_state` | `Mechanic` | `mechanic.possession_state` | `附身状态` | `敌方进入或施加附身/寄宿关系状态的中立机制概念。` | `GAP-001` |
| `ba_gap_candidate.status_stack` | `Mechanic` | `mechanic.status_stack` | `状态叠加` | `同类敌方状态可累积层数或强度的中立机制概念，不指定状态类型。` | `GAP-005` |
| `ba_gap_candidate.recognition_reveal_window` | `CounterWindowType` | `counter_window.recognition_reveal` | `识破后显露窗口` | `玩家完成识破后出现显露或失防窗口的中立反制类别，不预设净化。` | `GAP-010` |
| `ba_gap_candidate.weakpoint_exposure_window` | `CounterWindowType` | `counter_window.weakpoint_exposure` | `弱点暴露窗口` | `敌方弱点或核心进入可暴露窗口的中立反制类别，不预设触发源。` | `GAP-039` |

Every entry must use:

```text
playerVisible = false
developerOnly = false
runtimeImplemented = false
```

Clarifications:

- `mechanic.damage_reduction` is not `mechanic.layered_shield`.
- `counter_window.recognition_reveal` is not
  `counter_window.cleanse_reveal`; it does not author a cleanse trigger.
- `counter_window.weakpoint_exposure` does not author shell-break, clear,
  energy-counter, timing, duration, source, or success rules.
- `mechanic.contested_mark` does not own resource values or settlement.
- `mechanic.possession_state` does not define targetability, actor ownership,
  damage transfer, or AI.
- `mechanic.status_stack` does not select the concrete status type.
- `mechanic.charge_attack` does not define damage, distance, hitbox, target,
  movement execution, or animation timing.

## 5. Required Runtime-Neutral Types

Implement the smallest immutable extension data surface under the whitelist.
The exact public type names are:

```text
EnemyMechanicVocabularyExtensionSchema
EnemyMechanicVocabularyExtensionEntrySnapshot
EnemyMechanicVocabularyExtensionSnapshot
BoneAspectMechanicVocabularyExtensionCatalog
EnemyMechanicVocabularyExtensionComposer
EnemyMechanicVocabularyExtensionValidator
EnemyMechanicVocabularyExtensionValidationIssue
EnemyMechanicVocabularyExtensionValidationException
```

Required extension-entry fields:

```text
CandidateId
Category
StableKey
DeveloperLabelZh
Description
SupportingSurveyRowIds
PlayerVisible
DeveloperOnly
RuntimeImplemented
```

Required extension-snapshot fields:

```text
SchemaId
SchemaVersion
ExtensionId
BaseSchemaId
BaseSchemaVersion
DevOnly
IsEnabled
EntersFormalFlow
RuntimeImplemented
Entries
CanonicalSignature
```

Collections must be defensively copied, exposed read-only, and ordered with
`StringComparer.Ordinal`. Integer and string canonical serialization must be
culture invariant.

The composer may:

1. accept an explicit E02 base `EnemyMechanicVocabularySnapshotInput`;
2. validate the extension and base compatibility;
3. return a new composed `EnemyMechanicVocabularySnapshotInput`;
4. let the existing E02 provider/validator validate that new input.

The composer must not:

- modify either input;
- add or modify legacy mappings;
- write a static global default;
- read P0/P1 carrier content;
- bind any entry to a mechanic profile, skill, phase, encounter, pressure, or
  readiness row;
- expose a player projection;
- run Battle or Enemy runtime behavior.

## 6. Expected Composition

The verifier must prove:

```text
Base entries = 63
Extension entries = 7
Composed entries = 70

Base category counts:
Mechanic = 12
BuildCapability = 16
PressureChannel = 9
CounterWindowType = 6
PlayerHintCategory = 10
DeveloperDiagnosticCategory = 10

Composed category counts:
Mechanic = 17
BuildCapability = 16
PressureChannel = 9
CounterWindowType = 8
PlayerHintCategory = 10
DeveloperDiagnosticCategory = 10

Base legacy mappings = 124
Composed legacy mappings = 124
Added legacy mappings = 0
```

The base canonical must remain:

```text
sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833
```

The report must record separate values for:

```text
BaseCanonicalSignature
ExtensionCanonicalSignature
ComposedCanonicalSignature
```

No package may claim the composed signature as the new global E02 default.

## 7. Exact Verification Scenarios

The verifier must contain and report all of these check IDs:

```text
extension-schema-id
extension-schema-version
extension-id
extension-base-schema
extension-safe-flags
extension-entry-count
extension-exact-stable-keys
extension-exact-categories
extension-exact-labels
extension-exact-descriptions
extension-exact-candidate-lineage
extension-exact-survey-lineage
extension-player-visible-zero
extension-developer-only-zero
extension-runtime-implemented-zero
base-entry-count
base-category-counts
base-legacy-count
base-canonical-unchanged
composed-entry-count
composed-category-counts
composed-legacy-count-unchanged
composition-base-input-immutability
composition-extension-input-immutability
extension-defensive-copy
extension-read-only-collections
extension-canonical-determinism
composition-canonical-determinism
composition-reverse-input-determinism
composition-culture-determinism
extension-canonical-field-sensitivity
composition-canonical-field-sensitivity
reject-duplicate-extension-key
reject-base-key-collision
reject-prefix-category-mismatch
reject-unapproved-eighth-entry
reject-missing-required-entry
reject-unknown-survey-row
reject-candidate-lineage-mismatch
reject-null-entry
reject-schema-mismatch
reject-unsafe-flags
reject-player-visible-entry
no-runtime-consumer
no-formal-flow-consumer
package-path-whitelist
report-repeat-determinism
missing-report-regeneration
guid-uniqueness
text-integrity
protected-e02-baseline
protected-upstream-baseline
protected-broad-baseline
```

Expected result:

```text
53 / 53 PASS
```

Negative fixtures must use synthetic IDs and must not be added to the catalog.

## 8. Exact Output Whitelist

Exactly 17 new files are allowed:

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs.meta
Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionReport.md
Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSpec.csv
Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionInventory.csv
Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSourceEvidence.csv
Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionComposition.csv
Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionLeakCheckReport.md
```

Existing files modified:

```text
0
```

Any required change to an existing source, report, scene, prefab, config, queue,
or project setting is a Guard Return, not an implementation choice.

## 9. Protected Baseline

Use the disk state at task start, not Git HEAD. The following Guard-time
aggregates use:

```text
Ordinal relative-path ordering
relativePath + NUL + lowercase file SHA-256
LF between rows
SHA-256 of the joined UTF-8 bytes
```

Accepted protected groups:

```text
E02 base package:
14 files
sha256:b1ee6ea5a5e407b0f809de35d03e3325331824ed296272dcb886fae668d78166

BoneAspect P0:
8 files
sha256:2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5

BoneAspect P1:
21 files
sha256:21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209

BoneAspect Gap Survey:
8 files
sha256:0a79e5e9a40cb8e88d0d25fc0db97972e03452cf3168878df98ed26871d9fca0

BoneAspect Boss Art Lineage:
9 files
sha256:93659d9b894251e73ad85a0d854c8a7c62d0f795c54ca38a5d3bf2065b841dc6
```

At task start, additionally freeze every existing file under:

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/**
Assets/_Game/Scripts/TalismanBag/Items/**
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/CrossSystem/**
Assets/_Game/Scenes/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/_Game/Configs/**
ProjectSettings/**
Packages/**
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
```

The 11 new source/meta files in the whitelist are allowed additions. Every
task-start existing file in these groups must remain byte-identical.

Concurrent external drift must be attributed to its owning task and must never
be restored, reformatted, staged, or claimed by this package.

## 10. Hard Prohibitions

Do not:

- modify the accepted E02 default catalog or any E01-E10 file;
- modify P0, P1, Gap Survey, Boss Art Lineage, or decision overlays;
- add BA-D3/BA-D4 vocabulary or select either decision;
- implement BA-D1 or BA-D2 runtime behavior;
- add mechanic profiles, skill patterns, counter-window instances, Boss phases,
  pressure profiles, requirements, readiness, encounters, or seeds;
- create player hints, player-visible answers, thresholds, damage, cooldown,
  duration, target, AI, hitbox, movement, or animation facts;
- read or modify Item, Board, Battle, BuildSandbox, CrossSystem, Scene, Prefab,
  UI, BuildSettings, SaveData, Reward, Drop, Chapter, RunFlow, or formal
  1-10/2-10 flow;
- create a runtime MonoBehaviour, ScriptableObject, `.asset`, Scene object,
  Prefab, Resources key, Addressable key, animator, material, VFX, or UI;
- run a Builder or save a Scene/Prefab;
- update Guard rules or Package Queues;
- commit, tag, push, stage, reset, clean, rollback, or terminate another task's
  Unity process.

## 11. Reports

Reports must be regenerated from the same verifier source and include:

```text
exact seven-entry inventory
candidate and survey lineage
base / extension / composed counts
base / extension / composed canonical signatures
negative-fixture results
protected baseline results
runtime/formal/player leak count
package manifest
execution mode
```

Report generation must be deterministic. Two consecutive generations must
produce identical hashes for all six reports. Deleting any one report and
running the verifier must rebuild byte-identical content.

## 12. Verification Entry Points

Required Unity entry:

```text
TalismanBag.Editor.EnemySystem.BoneAspectMechanicVocabularyExtensionVerifier.RunBatch
```

Required menu item:

```text
TalismanBag/Enemy System/Run Bone Aspect Mechanic Vocabulary Extension Verifier
```

Required validation:

```text
same-source offline compile + verifier
Unity 2022.3 batch compile
Unity batch verifier
53/53 PASS in both verifier modes
six-report hash equality across modes
GUID conflicts = 0
new-file trailing whitespace = 0
package-scope text check = PASS
```

If the project is open in Unity or another Unity/batch process owns the project,
wait. Do not close the user's Editor, kill another process, remove a lock, or run
a competing batch.

No Scene handtest is required.

## 13. Stop Gate

After QA:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS
```

Send the status directly to Enemy Guard and RepoOps. Then stop.

The following remain not started:

```text
V0.4-BoneGuardRepresentativeMechanicRuntimeContract01
V0.4-BoneGuardBattleSandboxVerticalSlice01
all BA-D2 packages
all BA-D3 / BA-D4 packages
formal flow migration
```

Do not draft, route, or start the next package.
