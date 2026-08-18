# Canonical Effect Operator Task Contract

## CANONICAL_APPLICATION_FEEDBACK_AND_FLOAT_RELIABILITY - 2026-08-14

### Frozen behavior

- Battle state remains authoritative. Presentation feedback reports the real requested and applied result; it never creates or substitutes a mutation.
- A valid zero-benefit application is readable. Full-HP Heal emits `HP +0`; Cleanse with no removable status emits `CLEANSE 0`. Positive Guard, Heal, Cleanse, Nian refund, control delay and generated Break use the same generic feedback path without Item-ID dispatch.
- Existing direct and status damage float cues remain unchanged. Canonical effect damage now emits its missing same-instance `DamageFloatPayload` after the real HP/Shell commit.
- The existing ten authored float slots remain the sole carrier. A bounded pending queue waits for a free slot and staggers repeated output on the same receiver; active text is never overwritten merely because all slots are busy.

### Allowed writes for this package

- `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/C1FormalRealtimeBattleSessionContracts.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattlePresentationRoot.cs`
- `Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleDamageFloatPool.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier.cs`
- This Task Contract and `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md`.

### Protected boundaries and stop rule

- Do not change Item/Enemy values, Reward, Item Authority, Board/Tray, Stage, Save, Item Detail, Scene, Prefab, Profile, VFX grammar or product semantics.
- Do not create a second cue ledger, float manager, presentation bridge, test framework or per-Item branch.
- Stop after Runtime compilation, the existing focused verifier compilation, one external focused verifier run and one bounded player readability check. Compile or cue evidence alone is not player completion.

### Implementation checkpoint

- Added one generic `ApplicationFeedbackPayload` cue kind. It carries truthful requested/applied amounts through the existing cue fields and identifies feedback family generically (`HEAL`, `GUARD`, `CLEANSE`, `NIAN`, `CONTROL`, `SHELL`).
- Added the missing canonical-effect damage float after authoritative HP/Shell mutation.
- `FormalBattlePresentationRoot` consumes the generic feedback cue through the existing float pool.
- `FormalBattleDamageFloatPool` no longer replaces an active slot when all ten are occupied. It uses one bounded 32-entry wait queue, 80 ms same-receiver staggering and ten reusable receiver offsets.
- Existing focused proofs now require `HP +0`, `CLEANSE 0`, exact source identity and readable multi-target damage payloads.
- Offline Runtime compile: `PASS / 0 errors` with two pre-existing unused-field warnings. Focused Editor verifier source compile against the new Runtime: `PASS / 0 errors`. Unity, menu execution and Fresh Play were not started or fabricated.
- Package state: `IMPLEMENTED / OFFLINE_COMPILE_PASS / FOCUSED_VERIFY_RUN_PENDING / PLAYER_READABILITY_RECHECK_PENDING / NOT_READY`.

## CANONICAL_CLEANSE_CORE_LOOP_CORRECTION — 2026-08-13

### Frozen semantics

- `cleanse` is a player-side Battle State mutation. It only removes a legal `Cleanseable` negative status from the existing Formal Session player state.
- A cleanse application succeeds only when at least one real status stack is removed. When a scheduled placed+lit action has a valid trigger opportunity, lifecycle, cooldown and Nian but no legal status exists, the Item still activates, spends Nian once, advances cooldown and emits its exact-source `ItemTriggerAccepted`; the application result is zero and emits no `StatusRemoved` or success-only follow-up.
- The existing Shattered Host `polluted_pulse` request is the first formal provider of a cleanseable player status. It adds a bounded pollution stack through the existing enemy wave/session path; no second Enemy or Status system is introduced.
- `on_cleanse` and `cleanse_success` are emitted only after that removal. `same_target_chain` begins from the second consecutive successful cleanse of the player; `water_charge_3` resolves on every third successful cleanse. `cleanse_overflow` remains deferred.
- Reward admission is capability-based: a definition whose base action requires a cleanseable player status is eligible only when the next formal encounter declares that capability. No Item ID or stage ID dispatch is allowed.
- Generated fixed/random affixes continue to come from the single Canonical Item database. This package only activates generated affix payloads owned by the Cleanse family; it does not broadly release unrelated candidate affix semantics.

### Expanded allowed writes for this one package

- `Assets/_Game/Scripts/TalismanBag/Items/Canonical/CanonicalItemCatalog.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Canonical/CanonicalItemDropPolicy.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Runtime/CampaignLoot/C1CampaignDirectLootPoolAndPolicy.cs`
- `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/C1FormalRealtimeBattleSessionContracts.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier.cs`
- This Task Contract and `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md`.

### Protected boundaries

- Do not change Item IDs, Item numerical assets, enemy numerical assets, StageConfig, Scene, Prefab, Item Detail, Presentation/VFX, Save, Board/Tray, Item Authority, or I031.
- Do not add a second Item catalog, Reward pool, Battle Session, Manager, Bridge, Adapter, or per-Item branch.
- Do not implement `cleanse_overflow`, broadly release every generated candidate affix, or reinterpret non-damage Items as damage.

### Gates and stop rule

1. With a real Shattered Host pollution stack, a placed+lit Cleanse Item removes real player status state, spends Nian once, then releases the same-instance `on_cleanse` effects.
2. Without a legal player status, the same scheduled action still activates, spends Nian once and emits the exact-source activation cue, while the Cleanse application remains zero and emits no success-only Cleanse cue or chain.
3. Before a next encounter with no cleanseable threat, Cleanse-dependent definitions are absent from candidates; before a declared threat, they are admitted by capability rather than Item/stage identity.
4. The existing focused verifier proves the two runtime paths and Reward admission. Runtime/Editor offline compilation must have zero errors. Then STOP; player status remains `NOT_READY` until a normal-route handtest.

### Implementation checkpoint — 2026-08-13

- `CanonicalItemDefinitionResolver` remains the only Item semantic database. It now resolves each rolled fixed/random affix back to that same Catalog definition and derives the `requiresCleanseablePlayerStatus` capability requirement from Canonical stats/trigger/condition/target semantics, not an Item-ID list.
- `C1CampaignDirectLootPoolAndPolicy` now supplies next-encounter capabilities to the existing drop algorithm. Shattered Host capability is admitted only when one actor row declares the aligned `shattered_host_skill + polluted_pulse` pair; no reward table, Item branch, or second pool was created.
- `C1FormalRealtimeBattleSession` now owns one bounded player `pollution` status created by the existing 1-3/1-5 Shattered Host enemy-wave path. A generated `cleanse` value removes real stacks; no stack produces a zero application result after the valid action has committed its Nian spend and `ItemTriggerAccepted`.
- A successful cleanse releases only the acting `ItemInstance`'s `on_cleanse` effects. Extra cleanse, refund, heal/guard conversion, consecutive-chain state, and three-charge healing all read the rolled value/effect from the same Canonical definition and generated instance. Cross-instance charging was explicitly closed.
- The existing focused verifier was extended in place with `CLEANSE_REAL_PLAYER_STATUS`, `CLEANSE_NO_STATUS_ZERO_BENEFIT_ACTIVATION`, `HEAL_FULL_HP_ZERO_BENEFIT_ACTIVATION`, and `CLEANSE_REWARD_CAPABILITY_ADMISSION`. Generic zero-benefit proofs select definitions by Canonical semantics rather than an Item-ID branch.
- Final offline compilation: Runtime `0 errors` (two pre-existing unused-field warnings); Editor `0 errors` (one pre-existing obsolete Sprite API warning). Unity, focused verifier execution, and Fresh Play were not started or fabricated.
- First focused execution reached the real Shattered Host pollution path but checked at 5000 ms while its authored Item cooldown scheduled the first Cleanse action at 8000 ms. The observed remaining pollution was therefore a verifier lifecycle/timing gap, not evidence that an executed Cleanse failed.
- The existing verifier now uses contract-valid timings: pollution at 4500 ms then Cleanse at 4800 ms; the no-status case executes at 4000 ms before pollution. Editor offline compilation remains `0 errors` after the correction.
- Package state: `ACTIVATION_APPLICATION_SEMANTIC_IMPLEMENTED / OFFLINE_COMPILE_PASS / FOCUSED_VERIFY_RERUN_PENDING / FRESH_PLAY_NOT_RUN / INTERNAL_QA / NOT_READY`.

### Activation / Application semantic closure — 2026-08-13

- Final chain: `Canonical Trigger Opportunity -> Activation Preflight -> Activation Commit -> Application Result (may be 0) -> Success-only Follow-up -> Presentation`.
- `C1FormalRealtimeBattleSession.ProcessItemAction` no longer rejects a valid scheduled action merely because its predicted immediate mutation is zero. Existing placed+lit admission, living-target, Nian, cooldown and retry boundaries remain unchanged.
- A full-HP Heal and a no-status Cleanse now consume their authored Nian cost once, advance through the existing action lifecycle and emit exact-source `ItemTriggerAccepted`; they do not fabricate HP/status mutation, Hit, damage float, `StatusRemoved` or `on_cleanse` success.
- `ApplyCanonicalBaseCleanse` remains the success boundary: only an actual removed stack advances same-instance Cleanse chain/charge state and releases `on_cleanse`.
- The obsolete prediction helper `CanCanonicalActionMutate` was removed rather than retained as a second semantic gate.
- The existing focused verifier now distinguishes real Cleanse success, no-status zero-benefit activation, full-HP zero-benefit Heal, and Tray/unlit rejection. No second test framework or Item-ID runtime branch was added.
- Offline compilation after the change: Runtime `PASS / 0 errors` with two pre-existing unused-field warnings; Editor `PASS / 0 errors` with one pre-existing obsolete Sprite API warning. Unity and the focused verifier were not started by this task.

## Outcome

`CAMPAIGN_NORMAL_LV1` 中，I001–I030 的 placed+lit Canonical Item 继续使用同一 `ItemGeneratedInstanceSnapshot` 与 `CanonicalItemDefinition`。每次真实 Item 行动先确认 trigger opportunity、生命周期、冷却与念力，再提交 Activation；rolled 基础结果与 `combatEffect` 的 trigger / condition / operation / target / unit 语法进入现有 `C1FormalRealtimeBattleSession` 的 HP、shell、Nian、行动、治疗、目标或状态槽。Activation 始终使用同一 `sourceItemInstanceId` 发 `ItemTriggerAccepted`；只有真实状态改变后才发 application/success-only cue。

## Player Promise Card

- MilestoneId: `CAMPAIGN_NORMAL_LV1_CANONICAL_EFFECT_OPERATOR`
- ProductContext: `CAMPAIGN_NORMAL_LV1`
- PlayerPromise: 玩家领取并摆放任意普通奖励道具后，进入下一关时该实例保留图片、执行真实伤害或定义效果，并影响实时战斗结果。
- PlayerMustDo: WorldMap 正常路线领取奖励、放入 5x5 Board、进入下一关。
- PlayerMustSee: 新道具参与伤害/状态变化，既有 Unified 与 Card VFX 由同源 cue 驱动。
- MustChangeLive: Live Session 的 HP、shell、Nian、行动、治疗、目标或状态之一。
- PersistenceOrResetExpectation: 仅保存通关关卡；Item 与战斗状态仍按本切片规则重置。
- ExplicitNonCompletionCases: 只写 effectState、只发 cue/VFX、只修 I004/I007、只通过编译或 fixture、从旧 Pool15 重建第二套事实。
- RequiredRealPath: `WorldMap -> 1-1/重玩关卡 -> 奖励 -> Tray -> 5x5 Board placed+lit -> 下一关 Unified live battle`。
- UserAcceptedAt: `2026-08-10`，沿用已批准的 Canonical Item 单一数据库与 30 件同规则范围。

## Task Class / Owners

- Class: `COMPLEX_GUARDED_ONCE`
- Primary Guard / Development Owner: 当前 Codex 任务窗口（Battle/Bridge）
- Static Data Owner: `CanonicalItemDefinitionResolver`
- Runtime State Owner: 现有 `C1FormalRealtimeBattleSession`
- Presentation Owner: 现有 Unified Presenter / VFX；本包不修改表现资产
- User Decision Point: `NONE`
- PlayerVisibleDelivery: `PARTIAL`
- MilestoneCompletionEvidence: `NO`

## Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleCanonicalEffectOperator.cs`（仅无状态语法求值，不拥有战斗状态）
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier.cs`
- 与上述新增源码对应的 `.meta`
- 当前合同、`Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md`
- 外部临时离线编译响应清单（不进入 Unity 工程）

## Forbidden Writes

- Scene、Prefab、Profile、Canonical Item 数值、Reward、Authority、Board/Tray、Presentation、VFX、Enemy、StageConfig、Save、ProjectSettings
- 新 Battle Session、Bridge、Adapter、第二 Item 数据源或旧 Pool15 事实重建
- per-Item ID switch、静态结果替代、只 cue 不 mutation
- Git/worktree、哈希/SHA、新的机械报告链、Unity 启停或序列化

## Required Behavior

1. Operator 只按 `operation + targetStatId + unit` 解释效果，不读取 I001–I030 ID。
2. Session 负责 trigger/condition 上下文与真实 commit；Operator 保持无状态，不能成为第二 Battle Owner。
3. placed+lit Item 的 rolled `damage / break / guard / heal / nianCost / cooldown` 继续生效；unlit/Tray 不执行。
4. signature effect 只在对应真实事件与条件成立时提交到现有状态槽；Activation cue 与 application result 分离，mutation 成功后才发同源 application/success-only cue。
5. 旧 Pool15 分支不得作为 Canonical 完成证据，也不得重新成为正式 feeder。

## Verification

- 定向 Runtime / Editor 离线编译均为 0 错误。
- focused verifier 覆盖唯一 Catalog 中全部 22 个 `operation + targetStatId` 语法组合，确认无 Item-ID dispatch，并至少覆盖 HP、shell、Nian、行动、guard/heal、target、status 的 live mutation family。
- 不启动 Unity；正常路径 Fresh Play 由外部 Unity owner 后续执行。

## Completion

- 写域内没有 Item-ID 特例或第二状态 Owner。
- Operator 支持唯一 Catalog 当前 22 个效果语法组合，未知组合明确返回 unsupported，不伪造 cue。
- Session 在合法 Action Activation 后以同一 `sourceItemInstanceId` 发 `ItemTriggerAccepted`；真实 mutation 后再发 application/success-only cue。
- 编译与 focused verification 通过；无任务自有进程；玩家状态在 Fresh Play 前保持 `NOT_READY`。

## Implementation Checkpoint — 2026-08-10

- 已建立一个无 Item-ID 分支的 Canonical Effect Operator；唯一 Catalog 的 I001–I030 共 30 行、22 个 `operation + targetStatId + unit` 组合均能解析。
- 已接入现有 Live Session 的 HP、敌方 shell、Nian、玩家 guard/heal、敌方行动时间、Item 行动时间、目标选择与现有 burn status 槽；mutation 成功后才发送同一 `sourceItemInstanceId` cue。
- I004 的 `on_direct_lit -> next_zhenlei_trigger -> extra target` 已接到真实第二敌人 HP；镜像伤害转护势会先记录本次伤害，再在下一次受击时消费记录值；冷却推进只在 `on_effect_end` 时修改相邻 Item 的待执行行动，不再错误地每次行动常驻生效。
- focused verifier 只证明 30 行/22 类语法映射与无 Item-ID dispatch；它不再被描述为玩家路径或 Live mutation 完成证明。
- 第一个未闭合的权威边界是 `on_cleanse`：当前正式 Live Session 没有玩家负面状态、净化成功 transition 或其 source/target 事件，因此 `cleanse_success / same_target_chain / water_charge_3` 不能在正常路径真实发生；也不能把敌方 burn 移除伪装为玩家净化。
- 与净化相关的 heal-to-guard、Nian refund、额外 cleanse 与蓄水治疗，以及需要复制负面状态剩余时间的持续治疗，保持 `PRODUCT_SEMANTIC_GAP`；本包未猜测 Enemy debuff、玩家状态或 HoT 语义。
- 定向离线编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误；未启动 Unity，未执行 Fresh Play。
- 当前终态：`CANONICAL_OPERATOR_GRAMMAR_READY / PROVEN_LIVE_SINKS_PARTIAL / CLEANSE_OWNER_API_MISSING / ARCHITECTURE_REASSESS_REQUIRED / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Canonical Effect Runtime Proof — 2026-08-10

- 本轮只扩展既有 `C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier`，直接加载真实 `ItemBalanceWorkbenchCatalog.asset` 并启动现有 `C1FormalRealtimeBattleSession`；未新建 Battle/Test Framework，未修改 Product Semantic、Cleanse、Authority、Presentation、旧 Sandbox 或 Pool15。
- `I004_EXTRA_TARGET`：`PASS`。一次真实 Item 行动改变至少两个敌方 Actor；实际 HP 与实际 Shell 均出现下降，且成功 mutation 后 Nian 减少 1。
- `I007_AFTERGLOW`：`PASS`。真实 `BattleStatusState` 建立；后续 tick 使目标实际 HP/Shell 总量下降。
- `GUARD`：`PASS`。direct-lit 启动效果使玩家实际 Guard 大于 0。
- `HEAL`：`PASS`。敌方先造成实际 HP 缺口，随后 Canonical Heal 使玩家实际 HP 上升；未实现或验证 Cleanse。
- `ENEMY_ACTION_DELAY`：`PASS`。至少一个既有待执行敌方 Action 的实际 due time 后移。
- `TRAY_UNLIT`：`PASS`。未点亮 Item 被现有 Session 输入边界拒绝，未产生 Nian transaction 或成功 Item cue。
- `NO_MUTATION`：`CODE_GAP`。敌方 HP/Shell、玩家 HP/Guard、Action/Status 均无有效 mutation 时，Session 仍错误消费 Nian；本证明包按约定未修复该缺口。
- `SEMANTIC_GAP`：代表性七项中为 `NONE`；既有 Cleanse 语义缺口不属于本轮代表项，也未被改变。
- 最终证据：Editor 定向离线编译 `0` 错误；任务自有 Unity batch PID `13364` 正常退出且无残留进程。batch 返回 `1` 是验证器对唯一 `NO_MUTATION=CODE_GAP` 的预期失败关闭，不是编译失败。
- 本轮终态：`RUNTIME_PROOF_COMPLETE / 6_PASS / 1_CODE_GAP_NO_MUTATION_NIAN_SPEND / 0_NEW_SEMANTIC_GAP / NO_PRODUCT_FIX / STOP / NOT_READY`。
