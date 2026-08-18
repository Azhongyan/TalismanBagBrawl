# V0.4 World Map Qingshifang Stage Loop Vertical Slice 01

- Catalog schema: `V04WorldMapCatalog.v1`
- Verification scope: `FULL_WITH_SCENE`
- Result: `PASS`
- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_WorldMap.unity`

## Assertions

| Assertion | Expected | Actual | Result |
| --- | --- | --- | --- |
| `world.regions` | 1 | 1 | PASS |
| `region.chapters` | 4 | 4 | PASS |
| `chapter1.stages` | 10 | 10 | PASS |
| `stage.visualStates` | Locked/Available/Cleared/Boss | Locked/Available/Cleared/Boss | PASS |
| `chapter.stableIds` | bone_aspect_chapter_1\|bone_aspect_chapter_2\|bone_aspect_chapter_3\|bone_aspect_chapter_4 | bone_aspect_chapter_1\|bone_aspect_chapter_2\|bone_aspect_chapter_3\|bone_aspect_chapter_4 | PASS |
| `stage.identity.1-1` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-1` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-2` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-2` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-3` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-3` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-4` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-4` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-5` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-5` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-6` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-6` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-7` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-7` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-8` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-8` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-9` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-9` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.identity.1-10` | read-only ChapterFlow identity match | matched | PASS |
| `stage.dropPreview.1-10` | 3 placeholder rows, owner unbound | 3 | PASS |
| `stage.1-10.boss` | boss identity only | bone_aspect_boss_c1_bone_guard | PASS |
| `progress.default` | unbound/non-formal/zero cleared | WORLD_MAP_PROGRESS_OWNER_UNBOUND | PASS |
| `stage.state.1-1.Available` | Available | Available | PASS |
| `stage.state.1-2.Locked` | Locked | Locked | PASS |
| `stage.state.1-10.Boss` | Boss | Boss | PASS |
| `stage.state.1-1.Cleared` | Cleared | Cleared | PASS |
| `bridge.battle.default` | devOnly=true formalFlow=false | devOnly=True formalFlow=False | PASS |
| `bridge.patrol.default` | requiresClearedStage/devOnly/non-formal | requiresClearedStage=True | PASS |
| `bridge.offline.default` | no per-frame battle simulation | simulatePerFrameBattle=False | PASS |
| `runtimeLock.forbiddenHierarchyWrites` | 0 | 0 | PASS |
| `scene.controller` | 1 | 1 | PASS |
| `scene.authoredBindings` | valid | valid | PASS |
| `scene.unique.WorldMapCanvas` | 1 | 1 | PASS |
| `scene.unique.WorldRegionView` | 1 | 1 | PASS |
| `scene.unique.QingshifangRegionView` | 1 | 1 | PASS |
| `scene.unique.ChapterStageMapView` | 1 | 1 | PASS |
| `scene.unique.StageDetailDrawer` | 1 | 1 | PASS |
| `scene.count.ChapterEntry_` | 4 | 4 | PASS |
| `scene.count.StageNode_` | 10 | 10 | PASS |
| `scene.count.DropPreviewRow_` | 3 | 3 | PASS |
| `scene.verifierReadOnly` | scene.isDirty=false | scene.isDirty=False | PASS |

## Errors / waiting conditions

- None.

## Ownership boundary

- WorldMap owns display/navigation shell only; ChapterFlow IDs are read-only inputs.
- No formal Save, Reward, Drop, Battle executor, Patrol settlement, or offline reward owner is connected.
- Runtime binds authored objects and updates state/text only; it does not create final UI or write geometry.

WORLD_MAP_QINGSHIFANG_VERTICAL_SLICE01_PASS
