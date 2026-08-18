# V0.4 ItemFourCoreProjectionDetailDualTrackVisibilityGuardFix01 Assignment

Status: `OVERALL_GUARD_APPROVED / READY_FOR_DEV`

Guard receipt:

```text
OVERALL_GUARD_PASS_ITEMFOURCOREPROJECTIONDETAILDUALTRACKVISIBILITYGUARDFIX01
ITEM_GUARD_PASS_ITEMFOURCOREPROJECTIONDETAILDUALTRACKVISIBILITYGUARDFIX01
```

Target receipt:

```text
GUARD_PASS_ITEMFOURCOREPROJECTIONDETAILDUALTRACKVISIBILITYGUARDFIX01
```

## 1. Package identity

Package:

```text
V0.4-ItemFourCoreProjectionDetailDualTrackVisibilityGuardFix01
```

Repository:

```text
F:\Porject\TalismanBagBrawl
```

Primary owner: Item Guard.

Joint ownership: Overall / Shared Presentation Guard has approved this narrow two-phase
presentation evidence package. Enemy Guard and Capability Algorithm Guard are not required.

This package is a GuardFix for:

```text
V0.4-ItemFourCoreProjectionDetailAndBaselineMigration01
```

The parent P3 package remains:

```text
USER_HANDTEST_FAIL / FINAL_ACCEPTANCE_HOLD
```

Item Detail Base Prefab Migration, Prefab A/B, consumer migration and UnifiedBattlePage remain
blocked.

## 2. Frozen evidence and ownership

User handtest:

```text
I001 QiLeiOnly: QiLei only, PASS
I009 FaMenOnly: FaMen only, PASS
I004 Dual: FaMen visible, QiLei missing, FAIL
```

The following facts are already accepted and must not be recalculated or changed:

- I004 has `BuildQualification.Dual`.
- I004 qualified tracks are `famen:zhenlei` and `qilei:ling`.
- The final I004 ViewModel immediately before the sole final `Bind` contains exactly one
  non-empty `famenBuild` section and one non-empty `qileiBuild` section.
- In the clean Inventory/zero-count sample, `famenBuild` body length is 216 and `qileiBuild` body
  length is 173; both use `keepWhenEmpty=true`.
- Core projection deep-clones and preserves both Build sections.
- The scene-authored panel has distinct `FaMenBuildSection` and `QiLeiBuildSection` references.
- The same `Bind` activates both section GameObjects and their authored rows.
- No duplicate state key, role collision, second Bind, later clear or Package A qualification
  defect was found.

First missing layer:

```text
PRESENTATION / DETAILCONTENT_VERTICAL_VIEWPORT_DISCOVERABILITY_AFTER_BIND
```

Serialized evidence:

```text
ScrollView: 800 x 950
Viewport: 800 x 1050
DetailContent authored height: 0
DetailContent sizing: ContentSizeFitter VerticalFit=PreferredSize + VerticalLayoutGroup
FaMen authored Y: approximately -941
QiLei authored Y: approximately -1451
```

Runtime post-layout content height and bottom-scroll reachability have not yet been observed.

## 3. Objective

Prove whether the real I004 QiLei Build section is reachable through the existing vertical
ScrollRect after the final runtime layout pass.

This package has two mandatory phases:

```text
Phase A: no-edit runtime evidence
Phase B: conditional minimal shared View correction
```

Phase B may begin only when Phase A proves a real unreachable or stale-layout defect. A normal
below-the-fold section is not a data loss and must not trigger a View edit.

## 4. Phase A - mandatory no-edit runtime evidence

Before modifying either allowed View file:

1. Open the real BattleSandbox scene read-only.
2. Build the real I004 final runtime model through the exact accepted path:

```text
CurrentSnapshot
+ CurrentQualifiedBuildState
+ CurrentCoreEffectRuntimeState
-> ItemDetailProjectionComposer
-> ItemDetailQualifiedBuildTrackProjector
-> ItemDetailQualifiedCoreEffectProjector
-> one ItemDetailPanelView.Bind
```

3. Assert before Bind:
   - exact `baseItemId=I004`;
   - exact real `itemInstanceId`;
   - Build qualification is Dual;
   - TrackRows are exactly `famen:zhenlei` and `qilei:ling`;
   - exactly one `famenBuild` body and one `qileiBuild` body;
   - both bodies are non-empty.
4. Assert after the same Bind:
   - both section GameObjects are active;
   - FaMen overview and three authored stage rows are active;
   - QiLei overview and two authored stage rows are active;
   - authored-row mode is active and disabled plain BodyText is not misclassified as missing text.
5. Perform normal in-memory Unity canvas/layout updates. Do not save the Scene or Prefab.
6. Measure and report:
   - DetailContent rect height;
   - DetailContent preferred height;
   - viewport rect height;
   - ScrollRect vertical normalized range;
   - FaMen and QiLei section bounds in content and world coordinates;
   - bounds at scroll top;
   - bounds after setting the ScrollRect to its real bottom and rebuilding layout;
   - whether the QiLei authored rows intersect the viewport at any valid scroll position.
7. Run the same post-Bind evidence for:
   - I001 QiLeiOnly;
   - I009 FaMenOnly;
   - I006 None / Known Zero.

Phase A must choose exactly one classification:

```text
REACHABLE_BY_SCROLL_NO_LAYOUT_DEFECT
ACTIVE_BUT_CLIPPED_UNREACHABLE
CONTENT_HEIGHT_NOT_ENCLOSING_QILEI
LAYOUT_REBUILD_STALE
OTHER_PRESENTATION_DEFECT_WITH_EVIDENCE
```

If the classification is `REACHABLE_BY_SCROLL_NO_LAYOUT_DEFECT`, do not modify either View file.
Deliver evidence and a corrected user handtest instruction that explicitly tells the user to
scroll the detail panel downward. Do not add a scroll indicator or new UI in this package.

## 5. Phase B - conditional minimal correction

Phase B is allowed only for:

```text
ACTIVE_BUT_CLIPPED_UNREACHABLE
CONTENT_HEIGHT_NOT_ENCLOSING_QILEI
LAYOUT_REBUILD_STALE
OTHER_PRESENTATION_DEFECT_WITH_EVIDENCE
```

The correction must:

- be generic shared Item Detail presentation behavior;
- restore accurate post-Bind content measurement and ScrollRect reachability;
- preserve authored positions, sizes, anchors, hierarchy and sibling order;
- preserve both authored Build row renderers;
- use normal Unity layout APIs;
- avoid item IDs, track IDs, hardcoded section Y values or special handling for I004;
- avoid a second Presenter, state owner, Build calculation or detail model;
- avoid Scene/Prefab saving.

Preferred correction area:

```text
ItemDetailPanelView.cs
ItemDetailSectionView.cs
```

If Phase A proves the authored Scene or Prefab geometry itself is invalid and cannot be repaired
through generic shared View layout behavior, stop and return:

```text
GUARD_RETURN_ITEMFOURCOREPROJECTIONDETAILDUALTRACKVISIBILITYGUARDFIX01_AUTHORED_GEOMETRY_REQUIRED
```

Do not edit the Scene or Prefab under this Assignment.

## 6. Exact existing-file whitelist

The following files may be modified only as stated:

| Path | Task-start SHA-256 | Permission |
|---|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` | `89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d` | Phase B only, after evidence |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs` | `957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d` | Phase B only, after evidence |

If either start hash differs before development, stop and return to Item Guard. These are current
disk baselines, not Git HEAD.

Phase A must leave both hashes unchanged.

## 7. Allowed new files

Add exactly:

```text
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier.cs.meta
```

Required batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier.VerifyStaticBatch
```

## 8. Report whitelist

The dedicated verifier may create only:

```text
Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixReport.md
Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixSpec.csv
Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityRuntimeEvidence.csv
Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityManualTest.md
Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixLeakCheckReport.md
```

Do not create `.meta` files under `Docs`.

The parent P3 verifier and its seven reports are protected historical package evidence. The new
verifier may consume their public runtime helpers only if they already exist and are read-only;
it must not modify or regenerate parent P3 reports.

## 9. Protected baselines

The following files must remain byte-identical:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.cs` | `59a337a1b439b4e4be74a3719e9aa56c6877f6929da22b133b452668ae2d5ada` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs` | `599f9b1469cb4165a9e22a2dd5bf1c5cb683b67399fd347a0cdd4cd59c41a6f2` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs` | `d0bb9f6ab3f3cee3840600f3c2fba0587ce88db545f8a05092968eff6462c8b1` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs` | `b02a95f760f03f7b53bd4e7ac5b9efd638c1d2f328bf7d098e3ccc45e43b3358` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs` | `8a73a20b91bc79f546456584ccaae38c2bbe384def6a27ab56f0715c98ee0689` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity` | `8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |

Also protected:

- Package A qualification contracts, assembler, algorithms and canonical signatures;
- P1/P2 four-core authority and all Candidate/Profile/Catalog data;
- ItemSystemSnapshot, IF01, P6, BoardAuthority and BoardAdapter;
- ItemDetailProjectionComposer;
- fonts, icons, Sprites, PNGs and all RectTransform values;
- all other Scenes and Prefabs;
- BuildSettings and ProjectSettings;
- formal Battle, RunFlow, SaveData, Reward, Chapter and Boss;
- EnemySystem, BoneAspect, EncounterPresentation, Enemy queues and reports;
- CrossSystem and Capability contracts;
- AGENTS.md and `Docs/LOCKED/*`.

## 10. Required verifier assertions

The verifier must prove:

1. The exact final I004 runtime model has Dual qualification and two expected track IDs.
2. The final pre-Bind ViewModel has one non-empty FaMen body and one non-empty QiLei body.
3. One and only one final panel Bind is used by the tested runtime path.
4. Both authored section GameObjects are active after Bind.
5. FaMen overview + three stage rows and QiLei overview + two stage rows are active.
6. Plain BodyText disabled by authored-row mode is not reported as missing presentation.
7. Layout and canvas updates are completed before measuring reachability.
8. DetailContent preferred/actual height and section bounds are finite and internally consistent.
9. At the real scroll bottom, QiLei authored rows intersect the viewport.
10. No content or authored row is reachable only by using an invalid normalized position.
11. I001 remains QiLei-only.
12. I009 remains FaMen-only.
13. I006 remains None / Known Zero without a false Build contribution.
14. Core-effect four-row data remains present beside Build data.
15. No stale section survives I004 -> I001 -> I009 -> I006 -> I004 switching.
16. Scene, Prefab, RectTransform, Build projector, P1/P2/P3 authority and BuildSettings hashes
    remain unchanged.
17. Phase A/Phase B classification and changed-file decision are written to the report.
18. LeakCheck and exact whitelist checks pass.
19. Package-scoped `git diff --check` passes.

Required markers:

```text
PHASE_A_RUNTIME_EVIDENCE_PASS
I004_DUAL_FINAL_VIEWMODEL_PASS
I004_DUAL_AUTHORED_SECTIONS_ACTIVE_PASS
I004_DUAL_SCROLL_REACHABILITY_PASS
I001_QILEIONLY_REGRESSION_PASS
I009_FAMENONLY_REGRESSION_PASS
I006_NONE_KNOWNZERO_REGRESSION_PASS
FOUR_CORE_BUILD_COEXISTENCE_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_WAITING
PREFAB_MIGRATION_NOT_STARTED
```

The report must also emit exactly one classification marker:

```text
REACHABLE_BY_SCROLL_NO_LAYOUT_DEFECT
ACTIVE_BUT_CLIPPED_UNREACHABLE
CONTENT_HEIGHT_NOT_ENCLOSING_QILEI
LAYOUT_REBUILD_STALE
OTHER_PRESENTATION_DEFECT_WITH_EVIDENCE
```

If Phase B is required, the final verifier must prove the original failure classification before
the fix and prove `I004_DUAL_SCROLL_REACHABILITY_PASS` after the fix.

## 11. Unity and concurrency isolation

Before every Unity run:

1. Check `Temp/UnityLockfile`.
2. Check running Unity, Tuanjie and ShaderCompiler processes.
3. If the user Editor or another package batch is active, wait. Do not close, kill or steal it.
4. Run only this package's dedicated verifier.

Command:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
  -batchmode -quit
  -projectPath F:\Porject\TalismanBagBrawl
  -executeMethod TalismanBag.EditorTools.BuildSandbox.ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier.VerifyStaticBatch
```

Do not run a Builder, parent P3 verifier, historical regression or any process that saves a Scene
or Prefab.

EnemySystem/BoneAspect/EncounterPresentation work is concurrent external work. Treat it as
read-only and do not revert, format, stage or claim it.

If Unity creates a non-whitelist `.meta`, remove it only when this process's ownership is proven.
If ownership is uncertain, stop and report.

## 12. User handtest

After a successful evidenced delivery, stop at:

```text
DEV_COMPLETE / QA_STATIC_PASS / REAL_RUNTIME_PRESENTATION_PASS / USER_HANDTEST_WAITING
```

Target scene:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Required handtest:

1. Play and open I004.
2. Confirm both FaMen and QiLei Build text and authored rows exist.
3. Scroll the detail panel from top to bottom and confirm QiLei is reachable and readable.
4. Confirm four core-effect rows remain visible in the same detail.
5. Open I001: QiLei only.
6. Open I009: FaMen only.
7. Open I006: no Build contribution; Known Zero text is stable.
8. Alternate I004/I001/I009/I006 and confirm no stale Build section remains.

If Phase A classified `REACHABLE_BY_SCROLL_NO_LAYOUT_DEFECT`, the handtest report must explicitly
state that QiLei is below the initial viewport and requires downward scrolling.

User PASS is required before parent P3 acceptance or Prefab migration can resume.

## 13. Stop conditions

Stop without claiming PASS if:

- an allowed or protected start hash differs;
- Phase A evidence is incomplete;
- the exact I004 runtime model does not contain both Build bodies;
- a fix would modify Build qualification, the Build projector, P1/P2/P3 core authority or data;
- a fix needs Scene, Prefab, authored RectTransform, font, icon or art changes;
- a fix requires item/track-specific code or hardcoded Y positions;
- another Unity process owns the project lock;
- any external Enemy file is modified;
- the exact whitelist cannot be maintained;
- Unity compile/verifier fails.

No protected baseline may be updated to make a failure pass.

## 14. Completion sync

On QA success, send directly to Item Guard, Overall Guard and RepoOps:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-ItemFourCoreProjectionDetailDualTrackVisibilityGuardFix01

AssignmentSHA256:
<exact Guard-supplied SHA>

PhaseAClassification:
<exact one classification>

PhaseB:
NOT_REQUIRED / APPLIED_WITH_EVIDENCE

Result:
DEV_COMPLETE / QA_STATIC_PASS / REAL_RUNTIME_PRESENTATION_PASS / USER_HANDTEST_WAITING

ModifiedFiles:
<exact files>

Reports:
<five reports>

Unity:
<compile, command, exit code, log>

ProtectedHashes:
<before/after>

GitOperations:
NONE
```

Do not start Prefab migration, Prefab A/B, consumer migration, UnifiedBattlePage or any other
package. No `git add`, commit, tag, push, reset or rollback is authorized.
