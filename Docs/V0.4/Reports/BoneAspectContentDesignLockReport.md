# Bone Aspect Content Design Lock Report

## 1. Package identity

| Field | Value |
|---|---|
| Reviewed package | `V0.4-BoneAspectContentDesignLock01` |
| GuardFix package | `V0.4-BoneAspectContentDesignLock01-GuardFix01` |
| Guard marker | `ENEMY_GUARD_ASSIGNMENT_BONEASPECTCONTENTDESIGNLOCK01` |
| Guard return | `ENEMY_GUARD_RETURN_BONEASPECTCONTENTDESIGNLOCK01 / CONTENT_IDENTITY_MISMATCH` |
| Ownership decision | `SYSTEM_OWNERSHIP_SPLIT_GUARD_BONEASPECT_DECISION / FEASIBLE_WITH_CHANGES` |
| Assignment | `Docs/V0.4/BoneAspectContentDesignLock01_Assignment.md` |
| Assignment SHA-256 | `4351ce9b5c070ee27e11eabc0f8debdd5e9d38c6bf4aa225c21d09175a433b23` |
| Result | `GUARD_FIX_CONTENT_COMPLETE / QA_STATIC_PASS / PROTECTED_BASELINE_EXTERNAL_CONCURRENT_DRIFT_BLOCK / NOT_PASS` |
| Runtime status | `NOT_IMPLEMENTED` |
| Formal flow status | `NOT_BOUND` |

This package is a report/content-only lock. It introduces no runtime type, asset, scene, prefab, configuration, formal route, reward, drop, save state, or implementation hook.

Fixed isolation values for the entire package:

- `devOnly=true`
- `isEnabled=false`
- `entersFormalFlow=false`
- `formalRouteBound=false`
- `runtimeImplemented=false`

The delegated task message supplied `4351ce9b5c070ee27e11eabc0f8debd5e9d38c6bf4aa225c21d09175a433b23`, which is 63 characters and therefore cannot be a SHA-256 digest. The report records the independently calculated 64-character disk digest above. The malformed delegated marker remains visible as historical metadata and is not silently rewritten.

GuardFix01 corrects the returned content-identity mismatch only. It restores Assignment sections 4–9 exactly and does not broaden scope or start P1.

## 2. Source gate

The source attachment was read from:

`C:/Users/Ella/.codex/attachments/39b477b4-cf82-4285-b587-0bc55040f032/pasted-text.txt`

| Check | Expected | Actual | Result |
|---|---|---|---|
| Source attachment SHA-256 | `cd3d72ec7339b2f0118e765d929f3ca102a6bec4efeaf9ffe45c94abd23fc21e` | `cd3d72ec7339b2f0118e765d929f3ca102a6bec4efeaf9ffe45c94abd23fc21e` | `PASS` |

Only Enemy content identity, encounter identity, mechanism promises, visual language, external boundaries, and unresolved user decisions were extracted from the source. Item, reward, drop, operations, marketing, backpack, costume, save, RunFlow, and formal chapter material remains external.

## 3. Delivery inventory

| Deliverable | Locked records | Purpose |
|---|---:|---|
| `BoneAspectContentCatalog.csv` | 16 | 12 normal enemies and 4 Bosses |
| `BoneAspectEncounterNodeCatalog.csv` | 12 | Three encounter nodes per chapter |
| `BoneAspectMechanicCommitmentMatrix.csv` | 16 | One mechanism commitment row per enemy or Boss |
| `BoneAspectVisualLanguageSpec.csv` | 44 | Global, color, family, material, view, readability, safe-area, and prohibition rules |
| `BoneAspectExternalReferenceBoundary.csv` | 9 | Assignment-exact external-only references |
| `BoneAspectUserDecisionSheet.csv` | 4 | BA-D1 through BA-D4 |
| `BoneAspectContentDesignLockReport.md` | 1 | Main content lock report |
| `BoneAspectContentDesignLockLeakCheckReport.md` | 1 | Scope and leak proof |

Original task-start state: all eight deliverable paths were absent. GuardFix01 modifies only these same eight paths, adds zero new paths, and modifies zero non-whitelist existing project files.

## 4. Locked content identity

### 4.1 Chapter and visual-family coverage

| Chapter | Visual family | Normal enemies | Boss | Encounter nodes |
|---|---|---|---|---|
| `bone_aspect_chapter_1` | `bone_aspect_visual_c1_bone_porcelain` | 碎骨附身者；骨瓷犬；换骨残相 | 守骨奴 | 青石坊丁巷外街；骨铺外巷；骨铺内堂 |
| `bone_aspect_chapter_2` | `bone_aspect_visual_c2_contract_identity` | 假相浮骨；契纸吏；竞骨客 | 竞骨人 | 地下停车场入口；罗刹海市内街；骨相拍卖厅 |
| `bone_aspect_chapter_3` | `bone_aspect_visual_c3_array_stone_memory` | 残阵石傀；旧令道影；骨尘小像 | 阵眼守护者 | 荒废山道；倒塌山门；记忆大殿·阵眼层 |
| `bone_aspect_chapter_4` | `bone_aspect_visual_c4_counterfeit_composite` | 无面骨俑；骨契商侍；天骨赝相 | 万相骨兽 | 封锁丁巷·街坊防线；异化骨铺；万相骨腹 |

Locked totals:

- Normal enemies: `12`
- Bosses: `4`
- Encounter nodes: `12`
- Visual families: `4`

### 4.2 Stable identity rules

The content catalog locks:

- `contentId`
- `contentKind`
- `chapterId`
- Chinese display name
- stable `localizationKey`
- stable `presentationKey`
- `visualFamilyId`
- `primarySilhouetteKey`
- `coreRecognitionKey`
- Assignment-exact `coreRecognitionPoint`
- short mechanism-commitment summary

The IDs and keys are content contracts only. No runtime class, registry, ScriptableObject, prefab, scene object, addressable, localization asset, or configuration entry is created by this package.

The catalog preserves 16/16 exact Assignment values for `contentId`, `chapterId`, `displayName`, `presentationKey`, `visualFamilyId`, and the core recognition point. No synonym, shortened ID, alternate family, or replacement carrier remains.

## 5. Encounter node lock

Each encounter node fixes a stable `nodeId`, chapter, stage-range label, display name, visual family, shared-asset family, primary visual purpose, battle-actor safe-area requirement, and UI safe-area requirement.

All twelve nodes are content specifications only:

- `devOnly=true`
- `isEnabled=false`
- `entersFormalFlow=false`
- `formalRouteBound=false`

No chapter entry, state machine, scene, prefab, build setting, RunFlow route, reward gate, save field, or formal progression hook is implied.

## 6. Mechanism commitment lock

The mechanism matrix preserves Assignment section 8 verbatim for all 16 carriers. This includes 守骨奴 protecting the blind woman with no summon/split, 竞骨人 using exactly three identities plus one unresolved auxiliary mechanism, 阵眼守护者 stopping attack and yielding instead of ordinary death, and 万相骨兽 using transaction sealing, two unresolved ability forms, blank-bone-fragment release, and memory-backflow collapse.

`reuseCandidateKeys` contains only stable keys observed in the current Enemy mechanic vocabulary. These are survey candidates, not final vocabulary bindings. `newMechanicGapCandidates` and `crossSystemCandidate` are gap/request labels only and do not register keys, allocate ownership, authorize implementation, or enable runtime flow.

Every matrix row remains:

- `implementationStatus=NOT_IMPLEMENTED`
- `devOnly=true`
- `isEnabled=false`
- `entersFormalFlow=false`

## 7. User decisions held open

| Decision | Open question | Status |
|---|---|---|
| BA-D1 | 守骨奴的“替盲眼女人承担伤害”是否存在真实可受击保护目标 | `USER_DECISION_REQUIRED` |
| BA-D2 | 竞骨人辅助机制选择“虚假分身”或“竞价护盾” | `USER_DECISION_REQUIRED` |
| BA-D3 | 换骨残相与骨尘小像的 Copy 范围 | `USER_DECISION_REQUIRED` |
| BA-D4 | 万相骨兽封存对象与两种正式形态 | `USER_DECISION_REQUIRED` |

No option was selected. No downstream implementation, quantity, timing, interface, phase binding, or system ownership was inferred.

## 8. Visual language lock

### 8.1 Global positioning

The version identity is locked as:

`中国志怪现代化；青石坊现实生活底色；骨瓷、骨片、契纸、阵纹形成版本识别`

### 8.2 Five primary colors

- 骨瓷白
- 旧纸黄
- 旧木褐
- 暗朱砂红
- 青石灰

The semantic roles are locked; numeric color values are not invented in this package.

### 8.3 Six material/narrative languages

- 骨瓷
- 契纸
- 阵石
- 朱砂骨纹
- 身份叠影
- 空白骨片

### 8.4 Readability requirements

The visual spec covers:

- Normal-enemy front/three-quarter, silhouette, primary-material, mechanism-marker, state-difference, and small-screen views.
- Boss front, battle three-quarter, phase-difference, and poster-reference views.
- Foreground, battle-midground, and narrative-background scene layers.
- Battle-actor and UI safe areas.
- 大轮廓、高对比、手机小屏可读；一怪一个主机制标识；同章节共享材质家族。
- Boss 具有单一远距离记忆点；机制状态使用分层图，不烧入基础立绘。
- Assignment 第 4 节八项禁止方向逐字保留。

No image, animation, material, model, prefab, scene, or marketing asset is authored.

## 9. External-reference boundary

The following remain `EXTERNAL_REFERENCE / OUT_OF_SCOPE`:

- 道具保证件
- Boss 奖励
- 随机掉落
- 骨相念痕
- 照灯记服装
- 骨相·初痕背包
- 签到
- Logo / KV / PV / Boss海报
- 正式章节 / Reward / Save / RunFlow

Their names may be used only to describe external dependency boundaries. This package contains no numbers, probabilities, dates, inventory behavior, reward logic, drop tables, operations flow, marketing production, save schema, or formal-route binding.

## 10. System ownership boundary

| System or role | This package may lock | This package must not do |
|---|---|---|
| Enemy content data | Identity, recognition, narrative role, observable mechanism promise | Runtime implementation, schema changes, E01-E10 edits, final vocabulary registration |
| Encounter presentation | Node names, visual purpose, safe-area requirements | Scene, prefab, BuildSettings, formal route |
| Item | External names and unresolved dependency questions only | Item definitions, values, inventory, drop, reward, save |
| CrossSystem | Candidate request labels only | Enable executor, bind interfaces, implement copy/seal |
| Battle | Observable phenomena and candidate request labels only | State machine, battle logic, runtime events |
| Product/Operations/Marketing | External-reference list only | Chapters, sign-in, KV/PV/Logo/poster production or release |

`Cross-System Executor / 跨系统执行体` remains disabled. Nothing in this report re-enables it.

## 11. CSV canonical signature

Canonical construction:

1. Compute SHA-256 over the raw bytes of each of the six CSV files.
2. Format `relativeReportPath|lowercaseFileSha256`.
3. Sort by relative report path using ordinal order.
4. Join with UTF-8 LF and no trailing LF.
5. Compute SHA-256 over the resulting UTF-8 bytes.
6. Prefix the lowercase digest with `sha256:`.

Canonical input:

```text
Docs/V0.4/Reports/BoneAspectContentCatalog.csv|56bd2430adcaefb69ec2b51297f2ba2c561c63f953571e528d40080f6b6e660c
Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv|7d84bb15b4a878834d5f6ba2396b39c51cd7ebcbcc9f49b2b402b98db7afa3bb
Docs/V0.4/Reports/BoneAspectExternalReferenceBoundary.csv|67815862e62949bf75245c076c53de7bf888c2072be7b2f18007413d843dce00
Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv|f41322bfb8bba99fc1af2c1c70a527feee62ddb9bc68bb1c767e272027381a64
Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv|7827dab79dfd42119a9c47f7cd8007174226459388f244ffcb108424b190ba06
Docs/V0.4/Reports/BoneAspectVisualLanguageSpec.csv|5be259775a24e5ef7b8a11b9963b6bee2bb756ca05941dd91c49f77d9e739533
```

Calculation A, Node `crypto.createHash`:

`sha256:3350c02ee92165f46ba87e92056d58b79bd6e2824f5b194cbe5a827eb97d7926`

Calculation B, WebCrypto `subtle.digest`:

`sha256:3350c02ee92165f46ba87e92056d58b79bd6e2824f5b194cbe5a827eb97d7926`

Result: `PASS / MATCH`

The prior `sha256:9f68e5592b207c1def5b001bdf8994bab82c119c34121b035fce5a6a0027cb7f` identifies the returned, incorrect dataset only and is not reused as current evidence.

## 12. Protected baseline

The protected baseline was captured from task-start disk state, including the current dirty workspace. It was not reconstructed from Git HEAD.

For each protected group, existing task-start files were frozen as sorted `relativePath|fileSha256` lines and hashed with UTF-8 LF/no trailing LF. The same frozen file list was re-hashed after delivery.

| Protected group | Files | Start hash | End hash | Result |
|---|---:|---|---|---|
| EnemySystem | 96 | `9951f1196fdb36f474e568591f10d2e65a263e9ebed6e6ade422069580c11c03` | `9951f1196fdb36f474e568591f10d2e65a263e9ebed6e6ade422069580c11c03` | `PASS` |
| Editor/EnemySystem | 24 | `6b2c4f391ad28e6a4a16a98cb621e53785c6ed7c1af81d411474f4e9552c23df` | `6b2c4f391ad28e6a4a16a98cb621e53785c6ed7c1af81d411474f4e9552c23df` | `PASS` |
| Items | 139 | `64ce69779c08397b60bccd56f25c2b1209200495d6af2d310dfa5fa43bb614de` | `36a1c1dd05e0a9d8466d8de0a09a94f625ae44e75fb710ecb85b4dbe5401b5e3` | `EXTERNAL_CONCURRENT_DRIFT / NOT_PACKAGE_OWNED` |
| BuildSandbox | 180 | `c35cf3a8f02153c7e5276485d2ee8e8388e96698a1e0be4e2e6d34607f29e516` | `5a8a38430129f28b6630223a35a0b8274ec2a5c4f0000ac4aff3ca23aa90622f` | `EXTERNAL_CONCURRENT_DRIFT / NOT_PACKAGE_OWNED` |
| Scenes | 14 | `9b670cd3ffa1b8ad8ada19b1b987a5d5cfc9de60c6a80312bd05eaba7ddf06b3` | `9b670cd3ffa1b8ad8ada19b1b987a5d5cfc9de60c6a80312bd05eaba7ddf06b3` | `PASS` |
| Prefabs | 5 | `b8297c8dd948aad294fd2e735c8c4c8da6328b4705703a702275d652396411b9` | `b8297c8dd948aad294fd2e735c8c4c8da6328b4705703a702275d652396411b9` | `PASS` |
| Configs | 121 | `965b28fca0d32d6b35bfd275bebefb10981319d99356c8706f096ba52c2bee62` | `965b28fca0d32d6b35bfd275bebefb10981319d99356c8706f096ba52c2bee62` | `PASS` |
| EditorBuildSettings | 1 | `5b637882eeae2966cfde540cdc4080c7555d3950ef55d7bc230901dbf001f756` | `5b637882eeae2966cfde540cdc4080c7555d3950ef55d7bc230901dbf001f756` | `PASS` |
| Docs/LOCKED | 15 | `f51c194fc56d736cea5d4e9dc7e7ff9dceee8acd53db26da01d848df1fd8ae12` | `f51c194fc56d736cea5d4e9dc7e7ff9dceee8acd53db26da01d848df1fd8ae12` | `PASS` |
| AGENTS.md | 1 | `08850cc1087406af273dc548e8a973313309a41fc038be565d10b2c3701d99f1` | `08850cc1087406af273dc548e8a973313309a41fc038be565d10b2c3701d99f1` | `PASS` |

Eight of ten protected groups remain byte-identical. During this Guard fix, the separately active Item task changed the following three frozen files:

- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs`

These paths are outside this package's eight-file whitelist and were not written by this package. The baseline is not redefined and the parallel task is not rolled back or interrupted; therefore the strict protected-baseline gate is reported as blocked rather than passed.

## 13. Verification posture

| Check | Result |
|---|---|
| Six CSV files parsed through the bundled spreadsheet artifact runtime | `PASS` |
| Required content, encounter, mechanism, visual, boundary, and decision record counts | `PASS` |
| Package-scope UTF-8, LF, no BOM, no trailing whitespace, one final LF | `PASS` |
| Package-scope whitelist and extension scan | `PASS` |
| Protected baseline | `8/10 PASS / 2 EXTERNAL_CONCURRENT_DRIFT / BLOCKED` |
| Canonical signature calculated twice | `PASS / MATCH` |
| Global `git diff --check` | `PREEXISTING_UNRELATED_FAILURES_ONLY` |
| Unity compile | `NOT_REQUIRED / NOT_RUN` |
| Unity batch verifier or builder | `NOT_REQUIRED / NOT_RUN` |
| Scene handtest | `NOT_REQUIRED / NOT_RUN` |

The global `git diff --check` already failed before this package wrote any file because parallel pre-existing scene changes contain trailing whitespace and two pre-existing reports contain a blank line at EOF. Package-scope checks over the eight deliverables pass with zero new whitespace defects.

No Unity process was started, stopped, or inspected for control. No Editor, Item P2 task, or other process was interrupted.

## 14. Completion boundary

This package stops at content design lock. It does not start P1 and does not authorize runtime implementation.

Final status:

`GUARD_FIX_CONTENT_COMPLETE / QA_STATIC_PASS / PROTECTED_BASELINE_EXTERNAL_CONCURRENT_DRIFT_BLOCK / NOT_PASS / P1_NOT_STARTED`
