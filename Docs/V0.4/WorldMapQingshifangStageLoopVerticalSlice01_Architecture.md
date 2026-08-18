# V0.4 World Map / Qingshifang Stage Loop Vertical Slice 01

## Package boundary

This package owns the visible World Map navigation shell and presentation catalog only:

```text
World Map -> Version Region -> Chapter -> Stage Node -> Stage Detail Drawer
```

The dedicated authored scene is:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_WorldMap.unity
```

It is one scene with page roots, not one scene per chapter or stage.

## Read-only evidence and reuse

### MainHome Explore entry

`V03NavigationFlowController.ShowExplore()` currently opens the V0.3 `explorePageRoot`.
The MainHome fallback Explore button logs that the entry is reserved for a later package.
This package does not modify MainHome. A later isolated integration package may replace only
that entry action with a load of the dedicated WorldMap scene after Build Settings ownership is
approved.

### Chapter and stage identity

`V04ChapterFlowManifest` already owns:

- `bone_aspect_chapter_1` through `bone_aspect_chapter_4`;
- deterministic stage IDs `1-1` through `4-10`;
- the twelve Encounter Node IDs;
- the four Boss profile IDs.

WorldMap reads these identities and owns only hierarchy/presentation metadata. It does not
duplicate the ChapterFlow reducer, completion rules, Battle request execution, or result
acceptance.

### Existing progress, save, drop, and reward truth

- `MainTrialProgressData` and `MainTrialFlowService` are formal V0.2/V0.3 Save owners for the
  existing 1-10/2-10 loop. They do not represent the new four-chapter WorldMap contract.
- `RewardDropTable` rolls configured drops; `RewardService` grants them; `IdleDropConfig`
  references the current CoreLoop normal-stage drop table.
- These formal systems remain read-only. The WorldMap drawer uses stable placeholder rows marked
  `DROP_TABLE_OWNER_UNBOUND`, `RANK_OWNER_UNBOUND`, and
  `FIRST_CLEAR_REWARD_OWNER_UNBOUND`.

## First vertical-slice write scope

Allowed new files:

```text
Assets/_Game/Scripts/TalismanBag/V04/WorldMap/*
Assets/_Game/Scripts/TalismanBag/Editor/V04/WorldMap/*
Assets/_Game/Scenes/Scene_TalismanBag_V04_WorldMap.unity
Docs/V0.4/WorldMapQingshifangStageLoopVerticalSlice01_Architecture.md
Docs/V0.4/Reports/WorldMapQingshifangStageLoopVerticalSlice01Report.md
```

Forbidden existing owners include MainHome layout, BattleSandbox scene, C1 presentation/runtime,
Enemy Runtime, Item, LiHuo, Shougunu, formal Save/Reward/Drop rules, Build Settings, AGENTS,
LOCKED, queues, and Git.

## One-owner contracts

| Domain | One owner | WorldMap package behavior |
| --- | --- | --- |
| Map display and page navigation | `V04WorldMapSceneController` | Owns World/Region/Chapter/Drawer display state only |
| Stage identity/order | `V04ChapterFlowManifest` | Read-only stable ID source |
| Progress/unlock | Future WorldMap Progress adapter | Unbound snapshot; zero formal cleared stages invented |
| Drop table | Existing/future Drop owner | Placeholder slot IDs only; no roll, rate, item, or grant |
| Battle entry/result | Neutral request/result bridge + Battle owner | Contract draft only; Challenge disabled |
| Patrol selection | Future Patrol owner | Contract draft requires a cleared stage; button disabled |
| Offline settlement | Future Offline Settlement owner | Contract draft consumes a stage drop-table ID; explicitly no per-frame battle simulation |

## Later isolated integration packages

1. MainHome Explore -> dedicated WorldMap scene.
2. Stage 1-1 Challenge -> immutable neutral Battle request.
3. Battle result -> WorldMap return context, authoritative clear/unlock adapter.
4. Cleared stage -> replay and patrol selection.
5. Offline settlement -> elapsed-time settlement against the selected stage drop table.

No later package may make WorldMap a second Battle, Save, Reward, Drop, Patrol, or Offline
Settlement owner.

## Genuine decisions still deferred

The frozen UX needs no further decision for this package. Later owner packages must decide:

- which formal progress store owns the four-chapter route;
- which DropTable IDs and preview visibility rules bind to each stage;
- the Battle executor scene/adapter and authoritative result acceptance;
- patrol caps, offline duration rules, and settlement limits.

Those decisions are intentionally not guessed here.
