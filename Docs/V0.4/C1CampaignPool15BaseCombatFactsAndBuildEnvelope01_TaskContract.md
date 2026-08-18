# V0.4-C1CampaignPool15BaseCombatFactsAndBuildEnvelope01 Task Contract

## 1. Outcome

Freeze immutable `CAMPAIGN_NORMAL_LV1` white base combat facts for the accepted 15-item campaign loot pool and produce deterministic Item-only build output envelopes for the starter plus zero through four rewards. This package is a technical prerequisite only; it does not make the campaign loot loop player-visible.

## 2. Product Context

- `CAMPAIGN_NORMAL_LV1`
- Milestone: `V0.4-C1CampaignLootPool15PlayableProof01`
- Player promise preserved: random loot plus legal rearrangement must later create explainable live differences in damage, survival, targeting, control timing, and battle outcome.
- Explicit non-completion: reports alone, `DEV_SHOWCASE_LV40` values, fake direct-damage substitutes, turn-to-seconds conversion, or a player-visible completion claim.

## 3. Task Class

- `COMPLEX_GUARDED_ONCE`
- Reason: 15 numeric rows, five effect families, deterministic simulation, and two cross-Guard discrete-progress dependencies must remain semantically aligned.

## 3A. User Review Gate

- Required: `YES`
- Status: `USER_ACCEPTED_SCOPE`
- Primary Guard: Item Guard
- Accepted decision: exact 15 pool, five families, candidate semantics, CAMPAIGN targets, recent-two reward assumption, and autonomous technical execution are already approved.

## 4. Owners

- Primary Guard: Item Guard
- Development Owner: one visible direct-local Item development task
- State / Data Owner: Item for immutable base facts and build envelopes
- Battle dependency owner: Battle/Bridge for discrete action progress, target/time/state execution, ledger, and cues
- Enemy dependency owner: Enemy Guard for 1-3..1-5 HP/actions after this package
- User Decision Point: `NONE`

## 5. Allowed Writes

Exactly these six new paths:

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/C1CampaignPool15BaseCombatFacts.cs`
2. `Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/C1CampaignPool15BaseCombatFacts.cs.meta`
3. `Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/C1CampaignPool15BuildEnvelopeSimulator.cs`
4. `Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/C1CampaignPool15BuildEnvelopeSimulator.cs.meta`
5. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignLoot/C1CampaignPool15BaseCombatFactsAndBuildEnvelopeTests.cs`
6. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignLoot/C1CampaignPool15BaseCombatFactsAndBuildEnvelopeTests.cs.meta`

No existing source file may be modified.

## 6. Forbidden Writes

- Existing Pool15 carrier/materializer and starter baseline
- Battle/Bridge, Enemy, Reward/Drop, Mainline, Save, formal Inventory, ItemDetail
- Item catalog, shapes, placement/rotation/lighting authority
- Scene, Prefab, material, shader, sprite/importer, ProjectSettings, Packages
- `DEV_SHOWCASE_LV40` data or adapters
- Git, worktree, Notion, reports outside the six paths

## 7. Required Locks / Sources

- `AGENTS.md`
- `Docs/LOCKED/ENGINEERING_PROCESS_V2_LOCK.md`
- `Docs/CURRENT/PROJECT_ENGINEERING_STATE_V2.md`
- This Task Contract
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/C1CampaignLootEligibleItemPool15Carrier.cs`
  - protected SHA-256 `E65C64970F43E3D67ADDF5698D4A1F574CD72ED5CCBC2316EE14916B46B253E3`
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1Lv1StarterItemBaseline.cs`
  - protected SHA-256 `A4C454FFCC0A3B5CE1BF2181A39B79CA73C809FD81FD5D3AA3257C7908A140C7`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/CampaignLoot/C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.cs`
  - protected SHA-256 at intake `843803883018BC8F750BCF696F27BF1B1BD84C588460B67119A00DF359B7308E`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/CampaignLoot/C1CampaignLootPool15BattleEffectFamilyFoundation.cs`
  - protected SHA-256 at intake `6BA09AD9FC493A48C5AFA2C6DDDA9015F7EDD614815106251A9B9D6F7A91605A`
- Battle/Bridge terminal release for `ADVANCE_ITEM_TRIGGER_PROGRESS_STEP` and `DELAY_ENEMY_CAST_PROGRESS_STEP`; read-only exact API/signature/unit/cap evidence must be captured before package completion.

## 8. Patch Budget

- Files: exactly 6 new paths
- Existing files modified: 0
- If a current owner API must be changed, stop and return the exact owner seam to Item Guard; do not expand the whitelist.

## 9. Required Behavior

1. Resolve the accepted Pool15 signature `572c3421a4874ef203634e64896f59950c5746aa023026fc2503a643b99a47a0` and all exact identities in stable order:
   `I002,I003,I004,I007,I010,I011,I013,I014,I015,I022,I024,I025,I026,I028,I030`.
2. Freeze one immutable white base combat-fact row per identity with effect family, native trigger/cadence/first-offset semantics, target/activation legality, numeric magnitude, native unit, cap, cue identity, maturity, and canonical signature.
3. Preserve released I001 `82 direct damage / 2 seconds` and I002 `28 direct damage / 2 seconds`; I002 refund remains one native nian point. Do not copy `DEV_SHOWCASE_LV40` values.
4. Preserve the accepted candidate semantic values only when the CAMPAIGN model supports the native operator:
   I003 break `+650bp`; I004 extra target `+1`; I007 burn `+1 stack`; I010 nian cost `-1 point`; I011 burn conversion `+1 stack`; I013 guard `+4 flat`; I014 guard `+400bp`; I015 guard `+8 flat`; I022 cooldown progress `-1 discrete item-trigger step`; I024 heal extra trigger `+12 flat`; I025 enemy cast progress `-1 discrete enemy-cast step`; I026 damage `+800bp`; I028 targeting override `+1`; I030 extra target `+1`.
5. No global turn-to-seconds conversion exists. I022 and I025 must reference the released Battle discrete-progress API and remain unsupported/fail-closed until that exact dependency is terminal.
6. Produce deterministic build envelopes for I001 plus 0/1/2/3/4 rewards under equal-pool/recent-two sampling assumptions. Each envelope reports p10/p50/p90 damage, guard/heal, control progress/uptime, spread contribution, per-source event counts, maturity, and canonical signature.
7. Compare poor/middle/intended legal layouts for the same inventory. Preserve arrangement-dependent activation and report exact deltas; do not manufacture a 20% delta where no supported legal arrangement produces one.
8. Run discrete-event traces and at least `-20% / nominal / +20%` sensitivity for supported tunable magnitudes/cadences. Unknown, unsupported, conflicted, or not-executed values remain explicit and never become zero.
9. Export immutable downstream inventories for Enemy and Battle identifying supported facts, candidate facts, unsupported operators, envelope signatures, and exact source lineage.

## 10. Failure / Edge Cases

- Pool/catalog/signature mismatch: reject atomically with no partial snapshot.
- Missing/duplicate identity or family row: reject atomically.
- Unknown unit, nonfinite value, invalid cap, or illegal target/trigger: reject atomically.
- Unsupported I022/I025 Battle operator: return exact unsupported dependency and do not substitute seconds/direct damage/zero.
- Culture/order/repeated execution: byte-identical output for the same inputs.
- No static mutable roster, stateful RNG, Reward history, claim, entitlement mutation, or Battle state mutation.

## 11. Delivery Shape

- Config/Profile: immutable code facts and downstream inventories
- Runtime: pure resolver/snapshot/signature types and deterministic simulator
- Editor: focused tests only
- Scene/Prefab: none
- Adapter/Bridge: none; Battle API consumed read-only
- Report: snapshots are returned by public immutable APIs, not separate report files

## 12. Verification

- [x] No Unity serialization or Fresh Play required
- [ ] Actual separate current-source Assembly-CSharp compile using Unity response-file boundary
- [ ] Actual separate Assembly-CSharp-Editor compile against the newly built Runtime reference
- [ ] Focused deterministic tests for all 15 rows, exact order/signatures, units/caps, rejection matrix, culture/order/repeat stability
- [ ] p10/p50/p90 envelopes for 1-5 active Item counts and poor/middle/intended layouts
- [ ] discrete traces and `-20% / nominal / +20%` sensitivity
- [ ] no `DEV_SHOWCASE_LV40`, turn-to-seconds conversion, fake zero, RNG/history/claim, or Unity/runtime hierarchy dependency
- [ ] exact read-only reconciliation against Battle progress-operator terminal release

PASS requires every row and envelope to remain source-attributable and deterministic. Compile/static/simulator evidence is technical-only.

Fresh Play: `NOT_APPLICABLE`; no mounted player-visible carrier is delivered by this package.

## 13. Process Ownership

- `UNITY_NOT_REQUIRED`
- No user/unknown Unity process may be started, stopped, polled, or manipulated.
- No Git/worktree operation.
- Development task owns only its scoped compiler/test helper processes and must clean temporary outputs.

## 14. Completion

- Exact six-path write domain only; all protected sources unchanged except externally owned Battle files whose terminal release is reconciled read-only.
- Required facts, envelopes, tests, compile, sensitivity, and rejection checks pass.
- Battle progress operators are terminal and exact API evidence is recorded.
- No task-owned process/artifact residue and no unresolved product decision.
- Terminal cap: `TECHNICAL_PREREQUISITE_COMPLETE / PlayerVisibleDelivery=NONE / MilestoneCompletionEvidence=NO`.
- On Item Guard acceptance, release one immutable inventory directly to Enemy Guard; do not claim player-visible completion.

