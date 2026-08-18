# Core Battle Readability - Water Task Contract

## 1. Outcome

在 `CAMPAIGN_NORMAL_LV1` 的正式 Unified 战斗中，为后续 HUD 与 Item Detail 装修提供唯一、可直接消费的正式语义：

- 玩家与敌人的 HP、玩家 Guard、敌人 Shell、玩家/敌人当前状态均来自现有 `C1FormalRealtimeBattleSession`；
- Item Detail 从同一 `CanonicalItemDefinition + ItemGeneratedInstanceSnapshot` 生成玩家可理解的效果、触发、目标与实际数值文案；
- 不接回 BattleSandbox/V02 Runtime，不建立第二套 Battle、Status、Item Detail 或 Presentation 数据源。

本包只完成水电接口与语义，不修改 Scene/Prefab、布局、皮肤、动画或 VFX。

## 2. Task Shape

- Product Context: `CAMPAIGN_NORMAL_LV1`
- Task Mode: `水电`
- Task Class: `COMPLEX_GUARDED_ONCE`
- Primary / Development Owner: 当前 Codex 任务窗口
- Runtime State Owner: 现有 `C1FormalRealtimeBattleSession`
- Item Semantic Owner: 现有 `CanonicalItemDefinitionResolver`
- Presentation Owner: 现有 `FormalBattlePresentationRoot` 与 Exact Item Detail Prefab，本包不修改
- Expected New Owner: `0`
- Expected New Data Source: `0`
- Expected New Fallback: `0`

## 3. First Real Gaps

1. `C1FormalRealtimeBattleSessionStateSnapshot` 每帧已包含双方 HP、玩家 Guard 与敌人 Shell，但没有当前状态列表；现有公开 `StatusSnapshots` 只枚举敌人状态，玩家污染状态不在其中。
2. Formal Presentation 当前不消费 `ActorShellChanged`、`ActorShellBroken`、`StatusApplied`、`StatusRefreshed`、`StatusRemoved`；旧 V02 `StatusAnchorUI/StatusIconView` 直接绑定旧 Runtime，只能作为视觉参考。
3. Canonical Item 数据已经包含 effect、trigger、condition、target、operation、数值、持续时间与状态语义；当前 Detail 投影主要只为直接伤害生成玩家句子，非伤害效果仍依赖候选/字段文案。

## 4. Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/C1FormalRealtimeBattleSessionContracts.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemDetailProjectionAndSelection.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalItemDetailProjectionAndSelectionTests.cs`
- 本 Task Contract
- `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md`

## 5. Forbidden Writes

- 所有 Scene、Prefab、Profile、图片、动画与 VFX 资产
- `FormalBattlePresentationRoot`、Player/Enemy Presentation View 与 Exact Item Detail View/Presenter
- BattleSandbox/V02 Runtime、`AutoCombatController`、旧 Status Controller
- Item/Enemy 数值、Reward、Board/Tray、Authority、Stage、Save、WorldMap
- 新 Battle/Status/Detail System、Manager、Bridge、Adapter、Provider 或备用数据源
- Item-ID 分支或为单一道具写死文案

## 6. Required Behavior

1. 每个正式 Battle state snapshot 可直接读取玩家与敌人的当前状态；玩家状态使用 player actor identity，敌人状态使用原 actor identity。
2. 状态快照只投影现有 Session 状态，不拥有或修改状态；重置后为空，应用/刷新/移除后与真实状态同步。
3. Item Detail 的玩家文案只读取当前实例的 Canonical definition、rolled stats 与正式放置/点亮状态；不得回查旧 Pool15、Starter 或 Inner Catalog。
4. 伤害、破壳、Guard、Heal、Nian、Control、Status 与零收益效果保留各自真实语义，不伪装成伤害。
5. 本包结束时，装修 Owner 无需读取 BattleSandbox Runtime 即可绑定正式 HP/Guard/Shell/Status；Detail Prefab 无需第二 Provider 即可展示玩家语义。

## 7. Component Labels

- `FormalBattlePlayerPresentation` / `FormalBattleEnemySlot`: `KEEP / FORMAL_VISUAL_CARRIER`
- `C1FormalRealtimeBattleSessionStateSnapshot`: `KEEP / FORMAL_OBSERVABILITY_SOURCE`
- `C1ExactBattleSandboxItemDetailPopupStandalone`: `KEEP / FORMAL_DETAIL_VISUAL_CARRIER`
- `C1FormalItemDetailProjectionAndSelection`: `KEEP / CANONICAL_PLAYER_SEMANTIC_SOURCE`
- V02 HUD authored hierarchy: `VISUAL_ONLY_REFERENCE`
- V02 `StatusAnchorUI` / `StatusIconView` / status controller: `LEGACY_RUNTIME / DO_NOT_MIGRATE`
- BattleSandbox HUD authoring tools: `QUARANTINE / REFERENCE_ONLY`

## 8. Verification And Stop

- Existing Battle focused verifier proves player and enemy statuses appear in the same authoritative state snapshot and disappear after removal/reset.
- Existing Item Detail focused tests cover representative damage, Guard, Heal/Cleanse/Control or Status definitions and require non-empty player-facing effect/trigger/target text without Item-ID dispatch.
- Runtime and Editor offline compilation: zero errors.
- Unity, Scene authoring and Fresh Play are not part of this water package.
- After these checks, STOP. The next package is decoration of the existing Formal HUD and Exact Detail Prefab, followed by a separate Formal binding/Fresh Play cycle.

## 9. Room Check

- Formal -> Sandbox Runtime: `0`
- New Owner: `0`
- New Data Source: `0`
- New Fallback: `0`
- Pending Retirement: old V02 runtime remains quarantined; no deletion in this package
- Room Status: `CLEAN` when the two existing formal sources expose all required presentation semantics

## 10. Implementation Checkpoint - 2026-08-14

- `C1FormalRealtimeBattleSessionStateSnapshot` now carries the current player and enemy status snapshots from the existing live Session owner. No second status store or Presentation-owned gameplay state was introduced.
- The public Session status projection now includes both player and enemy status truth; focused coverage checks enemy status visibility and player pollution removal through the same authoritative state snapshot.
- `C1FormalItemDetailProjectionAndSelection` now generates player-facing trigger, condition, target, effect and stat language from Canonical fields and rolled values. Runtime Item-ID text dispatch remains `0`.
- Direct-damage presentation keeps the shared Canonical explanation and only appends the current placed/lit contribution result; it no longer replaces the common semantic source.
- A read-only focused menu was added: `Tools/TalismanBag/QA/Canonical Item Detail Player Semantics Focused Verify`.
- Runtime offline compile: `PASS / 0 errors / 2 pre-existing unused-field warnings`.
- Focused Editor verifier source compile: `PASS / 0 errors / 0 warnings`.
- First Unity menu run exposed `PRESENTER_EVENT_CHANGED_ONLY_GATE_MISSING` as a verifier-source-shape false positive: the live Presenter already gates null, rejected and unchanged results before state assignment/event notification, but the verifier required all three checks on one exact source line. The verifier now proves the real ordered gates without changing Runtime behavior; full Editor offline compile remains `PASS / 0 errors`.
- The next menu run exposed `VISIBLE_DETAIL_PROJECTION_REJECTED I001` as an invalid artwork fixture: `FormatterServices` created a CLR-non-null Sprite with no Unity native object, so the formal Presenter correctly treated it as missing. The fixture now uses a real transient in-memory Sprite; the Runtime artwork gate was not weakened. Full Editor offline compile remains `PASS / 0 errors`.
- User-owned Unity evidence: `Canonical Item Detail Player Semantics Focused Verify = PASS / assertions=128`.
- User-owned Unity evidence: `Canonical Effect Operator Focused Verify = COMPLETE / checks=43`.
- Fresh Play: `NOT RUN`.
- State: `WATER_COMPLETE / ITEM_DETAIL_FOCUSED_PASS_128 / BATTLE_STATUS_FOCUSED_PASS_43 / DECOR_READY / NOT_READY`.
- WATER STOP reached. Further work belongs to a separately frozen DECOR package and must not alter Battle or Item semantic ownership.
- STOP condition remains active: no Scene, Prefab, visual layout or BattleSandbox runtime changes belong to this package.
