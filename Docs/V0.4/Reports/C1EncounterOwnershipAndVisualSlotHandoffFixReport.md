# C1 Encounter Ownership And Visual Slot Handoff Fix Report

- Package: `V0.4-C1BattleSandboxEncounterOwnershipAndVisualSlotHandoffFix01`
- Classification: `COMPLEX_GUARDED_ONCE / CROSS_OWNER_FLOW+BATTLE+VISUAL`
- Current status: `FRESH_PLAY_BLOCKED_BY_ENEMY_GUARD_AUTHORITATIVE_TUNING`
- Runtime compile from Unity-generated response file: `PASS / 0 errors`
- Full Editor assembly compile against the revised runtime ref: `PASS / 0 errors`
- Fresh Play acceptance: `NOT_EXECUTED / NOT CLAIMED`
- Scene save performed: `false`

## Why the first revision failed structurally

The first revision proved ChapterFlow could reach `1-1 / BattleRunning`, but it did
not connect that truth to the authored battle presentation. Its journey layer drew
an independent IMGUI actor, the authored HP/info surface continued to be rewritten
by the Shougunu binder, ordinary reducer applications were not published to the
existing floating-text/LiHuo/item-feedback consumers, and 1-1..1-7 reused one
static sprite even while runtime cues changed. The only practical Start path was
also the dev IMGUI dock. That was a diagnostic preview, not encounter presentation
handoff.

## Revised ownership and presentation contract

- `V04Chapter1BattleSandboxRuntimeController` remains the only
  `EncounterAdmission` publisher.
- `1-1..1-9` admit only the existing
  `V04Chapter1NormalEnemyBattleAdapter`.
- `1-10 / BossGate` publishes `None`; generic grid/button state cannot start P3.
- Only an accepted Manual Challenge publishes `Boss` and admits exactly one
  Shougunu P3 session/Visual owner.
- The exact authored `V04BattlePrepareStateButton` now enters ChapterFlow and
  starts the admitted ordinary encounter. Its generic grid transition is still
  owned by `BuildGridInteractionPreviewController`.
- BossGate returns the generic grid to its existing authored prepare surface by
  invoking the singular authored prepare toggle; Flow does not mutate grid
  internals.

`V04Chapter1NormalEnemyBattleAdapter` is still the only ordinary HP/shell reducer
consumer. It now publishes one immutable, monotonic, read-only presentation event
per accepted real Item request. The event carries request/item/placement identity,
actual reducer application IDs, HP/shell before/after, applied amount, cue
identity, enemy snapshot and revision. It does not own or recalculate battle truth.

The existing Shougunu scene binder now leases its authored HP/shell/info fields to
the admitted ordinary snapshot and stops rewriting boss defaults while that lease
is active. The same existing TMP, Item trigger feedback, LiHuo feedback and living
outline consumers read the single active presentation source and gate every event
by source/generation/event identity. No Shougunu P3 event is fabricated for an
ordinary hit.

`C1JourneyPresentationController` owns the singular
`V02EnemyArea/Shougunu_1` Image only during ordinary admission. It consumes the
full ordered cue sequence and visibly queues Idle/Attack/Skill/Hit/Death. Since no
profile-specific five-state assets exist for the two current ordinary runtime
profiles, 1-1..1-7 use `d1_3` only as an authorized missing-profile visual
fallback; 1-8/1-9 use it under the held-slot authorization. Runtime identities
remain the manifest/catalog identities.

## Validation actually completed

- Read-only authored slot survey: exact Shougunu slot `1`, ordinary sibling source
  `1`.
- Revised runtime assembly: `PASS / 0 errors`.
- Entire revised Editor assembly, including the Fresh Play verifier:
  `PASS / 0 errors`.
- Independent IMGUI enemy actor: removed.
- Scene/RectTransform/hierarchy serialization changes: none.
- Save/Reward/Drop/BuildSettings/Git operations: none.

## Exact Fresh Play blocker

The current authoritative first-encounter Enemy/Battle tuning cannot satisfy the
required observable action coverage:

- Item application cadence: `500` ticks.
- Shattered Host durability: `MaxHp=160`, `ShellMax=0`.
- Official real Lv40 LiHuo accepted damage rows: approximately
  `29/42/53/46/55/76`.
- First Shattered Host action due: `2000` ticks.
- Shattered Host action catalog: one `BasicAttack` pattern and no executable
  action path that emits `ChargeAttack`, the runtime cue legally mapped to
  ordinary `Skill`.

The first encounter therefore reaches terminal damage at or before the first
action boundary, with no safe visible BasicAttack window and no possible Skill
cue. A presentation-only Skill, delayed visual damage, or duplicate HP owner
would violate package ownership. Final PASS is waiting on the requested Enemy
Guard revision of the authoritative Enemy/Battle configuration.

The Fresh Play verifier now runs this as an official-fixture-driven preflight. It
rejects the current configuration with
`AUTHORITATIVE_FIRST_ENCOUNTER_TUNING_NOT_READY` and records cadence, durability,
terminal tick, first action due tick, action count, pre-terminal cue coverage and
the real damage sequence. It does not change combat values or emit cues.

There is also an independent environment blocker: the Hub-launched Editor process
`PID 3068` still owns
`F:\Porject\TalismanBagBrawl`, but exposes no top-level window and has not updated
`Editor.log` since `2026-07-29 22:06:27`. A second no-window Unity run was attempted
with the package Fresh Play verifier and failed before project load with Unity's
exact single-instance error:

`It looks like another Unity instance is running with this project open.`

Evidence log:
`C:\Users\Ella\Documents\符箓\unity_c1_ownership_freshplay.log`

The active Editor was not terminated. Consequently no A-E/Fresh Play result is
claimed. After the Enemy Guard revision is consumed and the project lock is
released, the one-shot verifier is available at:

`Tools/TalismanBag/V0.4/B-Line Chapter 1/[QA Only] Run Encounter Ownership Fresh Play`
