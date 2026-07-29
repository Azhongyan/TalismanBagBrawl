# B-Line Chapter 1 Encounter Manifest Report

## Package

- Package: `V0.4-BLineChapter1EncounterManifest01`
- Assignment SHA-256: `ab4b36da0d83e3edd502af6b919876143c3ed0c9e48e03a4c738d5b83efe4a0a`
- Manifest schema: `V04Chapter1EncounterManifest.v1`
- Policy schema: `V04Chapter1DevBossCompletionPolicy.v1`
- Validation schema: `V04Chapter1EncounterBindingValidation.v1`
- Workflow: `COMPLEX_GUARDED_ONCE / V2`
- User hand test: `NOT_REQUIRED`
- Unity batch verifier: `PASS`

## Counts

- chapters: `1`
- activeStageBindings: `9`
- shatteredHostBindings: `4`
- porcelainHoundBindings: `5`
- heldByBAD3: `1`
- activeBoneSwapRemnantBindings: `0`
- bossBindings: `0`
- phase1TemporaryClearPolicies: `1`
- requiredPhase1Labels: `3`
- legacyContentRefs: `0`
- enemyRuntimeBindings: `0`
- battleStartRequests: `0`
- sceneBindings: `0`
- uiBindings: `0`
- rewardBindings: `0`
- saveBindings: `0`
- inventoryBindings: `0`
- dropBindings: `0`
- existingFileModifications: `0`

## Mapping

- `1-1` → `bline.content_binding_slot.c1.s1` → `bone_aspect_enemy_c1_01_shattered_host` — First-zone minimal encounter
- `1-2` → `bline.content_binding_slot.c1.s2` → `bone_aspect_enemy_c1_01_shattered_host` — Repeat deterministic Runtime coverage
- `1-3` → `bline.content_binding_slot.c1.s3` → `bone_aspect_enemy_c1_01_shattered_host` — First-zone close
- `1-4` → `bline.content_binding_slot.c1.s4` → `bone_aspect_enemy_c1_02_porcelain_hound` — Introduce shell pressure
- `1-5` → `bline.content_binding_slot.c1.s5` → `bone_aspect_enemy_c1_02_porcelain_hound` — Shell repeat
- `1-6` → `bline.content_binding_slot.c1.s6` → `bone_aspect_enemy_c1_02_porcelain_hound` — Shell repeat
- `1-7` → `bline.content_binding_slot.c1.s7` → `bone_aspect_enemy_c1_02_porcelain_hound` — Second-zone close
- `1-8` → `bline.content_binding_slot.c1.s8` → `bone_aspect_enemy_c1_01_shattered_host` — Temporary dev substitute for held future slot
- `1-9` → `bline.content_binding_slot.c1.s9` → `bone_aspect_enemy_c1_02_porcelain_hound` — Boss-precheck dev encounter

Mapping labels: `DEV_ONLY_RUNTIME_COVERAGE`, `NOT_CONTENT_FINAL`, `NOT_BALANCE_FINAL`, `NOT_FORMAL_STAGE_COMPOSITION`.

## Phase1 decision

- `PHASE1_DEV_VERTICAL_SLICE`
- `NOT_CONTENT_FINAL`
- `NOT_FORMAL_BOSS_COMPLETION`
- Resolver boundary: `REPLACE_COMPLETION_RESOLVER_ONLY`.
- ChapterFlow truth owner remains `V04ChapterFlowManifest.v1`.

## BA-D3 hold

- `bone_aspect_enemy_c1_03_bone_swap_remnant` / `换骨残相`
- status: `HELD_BY_BA-D3`; active Runtime binding: `false`; active stage binding: `false`; Copy scope authored: `false`.

## Protected aggregates

- LOCKED before/after: `7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f` / `7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f`
- GOVERNANCE before/after: `beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2` / `beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2`
- FORMAL_V03 before/after: `97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d` / `97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d`
- BLINE_CORE before/after: `04c0c1de0752c920bdde2076cced3318e0b074c21ea64b8424e066a45c1f4984` / `04c0c1de0752c920bdde2076cced3318e0b074c21ea64b8424e066a45c1f4984`
- DECISION_INPUTS before/after: `bfb2ad1e9df31390a3f959fdadd18f83769718da6b20ca26f70e0536beeb6f17` / `bfb2ad1e9df31390a3f959fdadd18f83769718da6b20ca26f70e0536beeb6f17`

## Closure

- Exact nine-row mapping: `PASS`
- Negative fixtures: `12/12 PASS`
- No second package started: `true`
- Git operations: `0`

```text
BLINE_CHAPTER1_ENCOUNTER_MANIFEST01_PASS stages=9 shatteredHost=4 porcelainHound=5 heldByBAD3=1 phase1TemporaryClearPolicy=1 legacyContentRefs=0 existingFileModifications=0
```
