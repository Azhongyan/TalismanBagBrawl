# C1 Encounter Ownership Leak Check Report

- Package: `V0.4-C1BattleSandboxEncounterOwnershipAndVisualSlotHandoffFix01`
- Static/source audit: `PASS`
- Runtime/full Editor assembly compile: `PASS / PASS`
- Fresh Play: `BLOCKED_BY_ENEMY_GUARD_AUTHORITATIVE_TUNING`
- Scene save: `none`
- Duplicate authored Shougunu slot: `false` (`count=1`)
- Duplicate ordinary source Image: `false` (`count=1`)
- Independent IMGUI enemy actor: `false`
- Second ordinary HP/shell/AI/Battle owner: `none`
- Ordinary Item application authority: `V04Chapter1NormalEnemyBattleAdapter`
- Ordinary presentation seam: `read-only; monotonic source/generation/event IDs`
- Existing authored HP/shell/info lease: `implemented`
- Existing floating TMP/LiHuo/item/outline consumer reuse: `implemented`
- Stale event gates: `source + resetGeneration + battleTick + eventId`
- P3 generic grid auto-start: `blocked by EncounterAdmission`
- Boss Visual admission before accepted Manual Challenge: `false`
- Boss Visual admission after accepted Manual Challenge: `true by contract`
- Authored slot teardown/restore: `implemented and idempotent`
- Runtime root flags: `DontSave`
- Scene active/transform/layout/hierarchy writes: `none`
- Save/Reward/Drop/BuildSettings/Git writes: `none`
- 1-1..1-9 runtime identity replacement: `false`
- d1_3 use: `visual fallback only`
- Presentation-authored Skill: `none`
- Visual damage delay: `none`
- First encounter tuning mutation by this package: `none`

## Fresh Play acceptance

No row is promoted from compile/static inference:

- A - 1-1 ordinary HP/shell/feedback and P3 false: `NOT_RUN`
- B - 1-1..1-9 exact identities/five states/no replay: `NOT_RUN`
- C - generic authored Start cannot start P3: `NOT_RUN`
- D - BossGate idle then accepted Manual Challenge handoff: `NOT_RUN`
- E - reset/second cycle/exit Play restore/scene clean: `NOT_RUN`

Verified authoritative blocker: `ItemApplicationCadenceTicks=500`, Shattered
Host `MaxHp=160/ShellMax=0`, real accepted damage approximately
`29/42/53/46/55/76`, first action due `2000`, and exactly one BasicAttack action
with no executable Skill/ChargeAttack cue path. Current tuning cannot demonstrate
the required visible Attack and Skill before terminal damage. The verifier rejects
this state with `AUTHORITATIVE_FIRST_ENCOUNTER_TUNING_NOT_READY`; this package does
not repair Enemy/Battle-owned tuning in Presentation.

Independent environment blocker: Hub Editor `PID 3068` retains the project lock.
The independent Fresh Play attempt was rejected before project load with Unity's
`another Unity instance is running with this project open` fatal error. The
existing Editor was deliberately not killed.
