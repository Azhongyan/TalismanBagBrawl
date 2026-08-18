# V0.4-C1BoneSwapRemnantRuntimeOperator01 Assignment

## 1. Identity

- Package: `V0.4-C1BoneSwapRemnantRuntimeOperator01`
- Classification: `COMPLEX_GUARDED_ONCE / PURE ENEMY RUNTIME OPERATOR`
- Primary Guard: `Enemy Guard`
- Development owner: one existing visible direct-local Enemy Runtime task
- User Review Gate: `YES / COMPLETE`
- Product context: `CAMPAIGN_NORMAL_LV1 prerequisite`
- Git / worktree: `FORBIDDEN / FORBIDDEN`
- Unity: `NOT_REQUIRED / MUST_NOT_START`
- Expected marker: `C1_BONE_SWAP_REMNANT_RUNTIME_OPERATOR01_PASS`

This Assignment resolves only the previously held BA-D3 Copy semantics. It does
not release campaign tuning, Encounter admission, formal Battle binding, or a
player-visible milestone.

## 2. Player Promise And Fidelity Boundary

```text
MilestoneId: C1_CAMPAIGN_LV1_1-1_TO_1-5_REBUILD_CONSEQUENCE
ProductContext: CAMPAIGN_NORMAL_LV1
PlayerPromise: Clear live battles, receive meaningful Items, rebuild, and see
  later 1-2..1-5 battles change visibly, including replay.
PlayerMustDo: fight, receive an Item, rearrange the Build, and enter later stages.
PlayerMustSee: live Enemy HP/actions and a visibly different result after rebuild.
MustChangeLive: Battle duration, damage/application cadence, survivability, or
  another accepted combat consequence.
PersistenceOrResetExpectation: each Battle session resets correctly; replay is
  a new accepted session without stale events.
ExplicitNonCompletionCases: a data contract, synthetic trace, held Runtime,
  precomputed result, copied Item implementation, or dev-only Preview is not the
  playable milestone.
RequiredRealPath: WorldMap -> formal realtime 1-1..1-5 -> direct Item reward ->
  optional rebuild -> next formal realtime Battle -> replay.
UserAcceptedAt: current direct-line continuation authority.
```

Package contribution:

| Capability | Owner | Carrier | This package status | Evidence class |
|---|---|---|---|---|
| Record one normalized player-resolved single-target direct-damage numeric pulse | Enemy live state, from a future Battle adapter | immutable input fact + operator state | DELIVER | contract/data + deterministic runtime |
| Telegraph then emit a bounded/capped return request | Enemy scheduler; Battle later executes | operator + neutral effect request | DELIVER | deterministic runtime |
| Author campaign ratio/cap/timing | Enemy balance Config/Profile | future released profile | MISSING DOWNSTREAM | none |
| Admit Bone Swap Remnant into 1-4/1-5 | Enemy Encounter | current Stage/Wave/Actor truth | OUT OF SCOPE / STILL HELD | none |
| Apply returned damage to player | Battle/Bridge | formal realtime session | OUT OF SCOPE | none |
| Show telegraph/hit/VFX | Presentation | Enemy Prefab/Presenter | OUT OF SCOPE | none |

```text
PlayerVisibleDelivery: NONE
MilestoneCompletionEvidence: NO
```

Success may close this technical package only. It may not claim
`USER_HANDTEST_READY`, playable 1-4/1-5, or milestone completion.

## 3. User-Resolved BA-D3 Semantics

The authoritative user decision is:

1. Record only the numeric magnitude of the player's last resolved
   single-target direct-damage pulse delivered to this Enemy operator.
2. Telegraph, then request a bounded/capped portion of that magnitude back.
3. Do not copy Item code, cooldown, state, target, chain, trigger graph,
   internal behavior, source Item identity, Build identity, or effect payload.
4. The return request always asks Battle for its own legal player target. It
   never reuses the original damage target.
5. Enemy owns the recorded pulse, pending action, telegraph schedule, dedupe,
   revision, reset generation, and neutral effect request.
6. Battle remains owner of authoritative clock, source-result normalization,
   legal player target, request acceptance, application ordering, damage ledger,
   and actual player HP change.

Historical BA-D3 `HELD_BY_BA-D3` evidence must remain unchanged. This package
adds a new resolved operator contract beside it; a later integration package
may replace the hold only after campaign tuning is released.

## 4. Tuning Boundary

The package defines explicit immutable profile fields:

```text
profileId
tuningDisposition
copyRatioBasisPoints
copyCapDamage
telegraphTicks
recoverTicks
pendingPolicy
canonicalSignature
```

Rules:

- `copyRatioBasisPoints` valid range is `1..10000`.
- `copyCapDamage` must be positive.
- `telegraphTicks` must be positive; `recoverTicks` must be nonnegative.
- Return amount is `min(floor(resolvedDamage * ratioBps / 10000), capDamage)`
  using overflow-safe integer arithmetic.
- A computed amount `<= 0` emits no request and reports fail-closed diagnostic.
- There is no default campaign ratio, cap, telegraph, or recovery value.
- No `CampaignReleased` profile instance may be authored in this package.
- Verifier profiles must be marked `SYNTHETIC_FIXTURE_ONLY` and may not be
  published through any Runtime catalog or Stage binding.
- The future campaign profile remains blocked until the accepted Item/Build
  output envelope exists and the producer/balance gates are run.

Synthetic QA values are test vectors, not balance decisions. The verifier must
use at least two different ratio/cap combinations to prove field sensitivity
and the absence of a hidden default.

## 5. Runtime Contract

Namespace:

```text
TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant
```

Stable identities:

```text
schema = BoneSwapRemnantRuntimeOperator.v1
contentId = C1EnemyRuntimeContract.BoneSwapRemnantContentId
operatorProfileId = bone_aspect.runtime.c1.bone_swap_remnant.operator.v1
actionId = c1.bone_swap_remnant.return_last_direct_pulse
effectRequestKey = battle.effect_request.direct_player_damage
targetRequestKind = PLAYER_PRIMARY_TARGET
devOnly = true
isEnabled = false
entersFormalFlow = false
runtimeBoundToBattle = false
```

Required immutable types include:

```text
BoneSwapRemnantCopyProfile
BoneSwapResolvedDirectDamageFact
BoneSwapRemnantRuntimeSnapshot
BoneSwapRemnantCapturedPulseSnapshot
BoneSwapRemnantActionSnapshot
BoneSwapRemnantCueSnapshot
BoneSwapRemnantEffectRequestSnapshot
BoneSwapRemnantTransitionResult
BoneSwapRemnantValidationResult
```

The normalized damage fact may contain only:

```text
schemaId
sourceEventId
applicationSequence
battleTick
resetGeneration
recipientEnemyInstanceId
acceptedByBattleLedger
sourceKind = PLAYER_RESOLVED_SINGLE_TARGET_DIRECT_DAMAGE
resolvedDamage
```

It must not contain Item ID, Item instance, cooldown, Build, placement, tags,
effect implementation, original target, chain, proc, RNG, crit, or payload.

The neutral return request must contain only stable correlation, Enemy source,
Battle resolve tick, amount, target request kind, reset/revision, source pulse
event ID, and canonical signature. It is a request, never an applied result.

## 6. State Machine And Ordering

States:

```text
Idle
Telegraphing
Recovering
Defeated
Invalid
```

Deterministic rules:

1. Create/reset requires an explicit valid profile and stable Enemy instance ID.
2. A fact is accepted only when ledger-accepted, current generation, positive,
   monotonic by application sequence/tick, unique by event ID, and routed to
   this Enemy instance.
3. Idle + accepted fact captures that pulse and starts one Telegraph action.
4. Telegraphing/Recovering + newer accepted fact stores exactly one pending
   latest pulse. `LATEST_WINS_ONE_SLOT` is the only allowed pending policy.
5. The active action's captured pulse is immutable. A newer pulse never changes
   an already telegraphed return amount.
6. At the authoritative resolve tick, emit exactly one neutral return request
   and one resolved cue. Animation completion never advances gameplay.
7. At recovery end, start a new Telegraph only when a pending pulse exists;
   otherwise return to Idle.
8. Defeat cancels active/pending work and emits no later return request.
9. Reset increments generation, clears pulse/action/request/cue/dedupe state,
   and rejects stale facts from prior generations.
10. Revision, cue sequence, request sequence, and accepted application sequence
    are monotonic. Duplicate or stale input changes nothing.

Required cue kinds are presentation-neutral contract facts only:

```text
CopyTelegraph
CopyReturnRequested
Reset
Defeated
```

No Prefab, sprite, Animator, VFX, HP, shell, pathing, target selection, or
player damage is implemented here.

## 7. Exact Write Whitelist

Create exactly these 18 files; existing-file modifications are zero:

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantCopyProfile.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantCopyProfile.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeOperator.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeOperator.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneSwapRemnantRuntimeOperatorVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneSwapRemnantRuntimeOperatorVerifier.cs.meta
Docs/V0.4/Reports/BoneSwapRemnantRuntimeOperatorReport.md
Docs/V0.4/Reports/BoneSwapRemnantRuntimeOperatorSpec.csv
Docs/V0.4/Reports/BoneSwapRemnantRuntimeOperatorTrace.csv
Docs/V0.4/Reports/BoneSwapRemnantRuntimeOperatorNegativeFixtures.csv
Docs/V0.4/Reports/BoneSwapRemnantRuntimeOperatorLeakCheckReport.md
```

No report `.meta` is allowed. The Assignment is Guard-owned and must not be
modified by development.

## 8. Protected Sources

These exact task-start hashes must remain unchanged:

```text
8b102bc08f30d79016dada658a8a0698c456f872dcbb15a0d5eba99c549e0f14  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs
b0958f053634fc40161f4b55e653e4c53789d854a0208bd9e4d0f7bf78e24279  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs
f60aea2213edff0d7397ca5849452ccedf6f284a64d20259bdbfd2b6aa22347a  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs
be816e5557b0384eed4fb76d9d4212701051883545a3909f215b9628f8e56f80  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs
291002c3736113d581b50f1d6c5e763eb581fb1adf6752dc7034916dd7f30f5b  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs
109cfa7a7f21490d3215f05e8984df87647005c5e465aa8a6deaee820192ae4c  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs
f66a8bb0b472877b59d9b1c2d085b7b501dca62838de09857d75d5b5fcc9abbc  Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs
22133b148e59e78f4f19e3fab0b9dd74da09317025e673495ba40a5c998b4af5  Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1StageWaveEncounterTruth.cs
a6f037fdf369da144dfea841a964bcc1e88a64b8aee5a9c5bcf4212d50a28174  Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1StageWaveEncounterValidation.cs
```

If any protected path drifts before development writes, stop with exact path
and hash. Do not rebaseline, restore, stage, or claim concurrent work.

## 9. Forbidden Scope

- No modification of existing C1 Runtime, Catalog, hold row, Stage/Wave/Actor
  truth, Encounter binding, Battle adapter, formal StageConfig, Mainline, Item,
  Build, Reward, Drop, Save, Inventory, Scene, Prefab, UI, animation, VFX,
  BuildSettings, ProjectSettings, Packages, Queue, AGENTS, LOCKED, or Git.
- Do not admit Bone Swap Remnant into any Wave or change `HELD_BY_BA-D3`.
- Do not author HP, shell, Basic attack, campaign ratio/cap, campaign timings,
  target TTK, player HP, or formal action cadence.
- Do not reference `TalismanBag.Items`, inspect Item requests, or reconstruct an
  Item effect from IDs, tags, text, rarity, cooldown, Build, or placement.
- Do not apply player damage, select a player target, advance Battle clock, or
  write an application ledger.
- No low-HP split, summon, swarm, copied proc, copied chain, copied cooldown,
  copied status, copied target, RNG, crit, DoT, resource, or area behavior.
- Do not start the campaign tuning, StageConfig, Mainline, Battle integration,
  Presentation, or any later package.

## 10. Deterministic QA

Required scenarios:

1. idle accepted pulse -> Telegraph -> one request -> Recover -> Idle;
2. two profile ratio/cap combinations prove formula sensitivity;
3. cap hit and uncapped result;
4. newer pulse during Telegraph becomes one pending latest slot;
5. three newer pulses collapse to the latest pending pulse;
6. pending pulse starts only after current recovery;
7. duplicate event ID rejected with no state change;
8. stale generation/sequence/tick rejected;
9. rejected ledger fact, wrong recipient, wrong source kind, zero/negative
   damage, missing IDs, undefined enums, malformed profile, overflow boundary;
10. computed zero emits no request;
11. defeat before resolve cancels action/request;
12. reset clears state, increments generation, and rejects old facts;
13. reversed input/culture/repeat generation produces stable signatures;
14. defensive copies and caller mutation isolation;
15. protected hashes 9/9 unchanged and exact package whitelist 18/18;
16. runtime references to Item/Stage/Battle executor/Scene/Prefab/Save/Reward
    are zero;
17. campaign released profile rows = 0; formal bindings = 0;
18. current BA-D3 hold sources remain byte-identical.

Minimum QA:

- focused offline C# compile, zero errors/warnings;
- same-source deterministic verifier and report regeneration equality;
- package text/whitelist/protected-hash checks;
- no Unity batch, Editor, Scene handtest, or user handtest;
- no package-owned process or temporary file at terminal.

## 11. Completion And Return

Return `COMPLETE / AUTOMATED_QA_PASS` only when all 18 additions are exact,
existing modifications are zero, all scenarios pass, protected hashes remain
unchanged, campaign released tuning/bindings remain zero, and the marker is
present in both main and leak reports.

Return directly to Enemy Guard with:

- exact files and hashes;
- compile/verifier counts;
- synthetic traces and formula sensitivity;
- negative/reset/dedupe results;
- tuning status `WAITING_CAMPAIGN_ITEM_ENVELOPE`;
- `PlayerVisibleDelivery=NONE` and `MilestoneCompletionEvidence=NO`;
- process cleanup and `GitOperations=NONE`;
- `NEXT_PACKAGE=NOT_STARTED`.

The same visible task owns any in-scope correction. The development task must
not dispatch downstream work.
