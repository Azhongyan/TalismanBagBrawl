# V0.4-C1Stage3To5PlaytestEnemyEncounterConfig01 Shield Fact Revision01

## Classification

- Workflow: `CONTAINED_ONE_GUARD`
- Relationship: same-package light correction
- Primary Guard: Enemy Guard
- User Review Gate: `NOT_REQUIRED`
- PlayerVisibleDelivery: `NONE`
- MilestoneCompletionEvidence: `NO`

## Player Outcome Supported Downstream

When Mainline and Battle mount this dependency, I003 can act on the real shell of
Porcelain Hounds in stages 1-3 through 1-5. This package only publishes the Enemy
fact. It does not implement live shell state, I003 application, UI, or Fresh Play.

## Technical Director Decision

- Authoritative static owner: Enemy PLAYTEST_V1 encounter configuration.
- Live mutable shell owner: Battle Session, downstream and out of scope.
- Carrier: the existing `C1Stage3To5PlaytestActorRow` plus one immutable defense
  fact value object.
- Reused source evidence: `BoneAspectC1EnemyRuntime.v2` Porcelain Hound profile.
- Scene / Prefab / material / import impact: none.
- Unity lease: not required; do not start Unity.
- QA: focused Runtime/Editor C# compile and one direct deterministic fact check.

## Required Behavior

Add an immutable actor defense fact with these public fields or exact equivalents:

- `HasShell`
- `ShellMechanicId`
- `MaxShell`
- `InitialShell`
- `ShellRegenerationCount`
- `ShellRegenerationPolicyId`
- `BreakTargetId`
- `BrokenStateId`
- `ShellBreakCounterWindowId`
- `SourceSchemaId`
- `SourceProfileId`
- `SourceRevision`

Frozen PLAYTEST_V1 values for every Porcelain Hound actor:

- `HasShell=true`
- `ShellMechanicId=mechanic.layered_shield`
- `MaxShell=100`
- `InitialShell=100`
- `ShellRegenerationCount=0`
- `ShellRegenerationPolicyId=NO_REGENERATION_THIS_ENCOUNTER`
- `BreakTargetId=enemy.resource.shell`
- `BrokenStateId=enemy.shell_state.broken`
- `ShellBreakCounterWindowId=counter_window.shell_break`
- `SourceSchemaId=BoneAspectC1EnemyRuntime.v2`
- `SourceProfileId=bone_aspect.runtime.c1.porcelain_hound.v1`
- `SourceRevision=C1_STAGE3_TO5_PLAYTEST_ENEMY_DEFENSE_FACT_R1`

Every Shattered Host and Bone Swap Remnant actor must expose an explicit
unshielded fact: `HasShell=false`, all shell numbers zero, regeneration policy
`NOT_APPLICABLE`, and no break/counter-window identity.

Do not change roster, HP, occurrence order, actions, cadence, Bone Swap tuning,
balance profile ID, encounter variant IDs, product context, activation status,
or the existing 1-1/1-2 sources.

Battle must not import or execute the dev-only `C1EnemyRuntimeCatalog`. The formal
sidecar copies the approved immutable values and records their source lineage.

## Exact Write Scope

Existing files allowed to change:

1. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterContract.cs`
2. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterValidation.cs`

New files allowed:

3. `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1Stage3To5PlaytestEnemyShieldFactVerifier.cs`
4. `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1Stage3To5PlaytestEnemyShieldFactVerifier.cs.meta`

No other product or report file may change. Do not add or update hashes, SHA-256
checks, canonical gates, broad matrices, or generated reports. Existing legacy
signature code is not acceptance evidence and does not need to be reworked.

## Minimum QA

1. Focused compile of the changed Runtime source, its direct dependencies, and the
   new Editor verifier: zero errors and zero warnings.
2. Stage 1-3 returns exactly two shielded Porcelain Hounds at 100/100 and one
   unshielded Shattered Host.
3. Stages 1-4 and 1-5 each return one shielded Porcelain Hound at 100/100.
4. Every Shattered Host and Bone Swap Remnant remains explicitly unshielded.
5. Existing roster, HP, action, cadence, Bone Swap and encounter identities remain
   unchanged by direct value comparison.
6. Unity, Scene, Prefab, Item, Battle, Mainline, Reward, Save and Git remain
   untouched.

## Terminal Return

Return to Enemy Guard only:

- `TECHNICAL_PREREQUISITE_COMPLETE`, or one exact blocker;
- changed files;
- compile and focused fact-check result;
- public Enemy API and field list;
- explicit `PlayerVisibleDelivery=NONE`, `FreshPlay=NOT_APPLICABLE`;
- no hash evidence.

Do not start downstream work.
