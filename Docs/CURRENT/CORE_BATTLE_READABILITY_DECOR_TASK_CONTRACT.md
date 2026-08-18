# Core Battle Readability - Decor Task Contract

## 1. Outcome

在 `CAMPAIGN_NORMAL_LV1` 的现有 Unified 正式战斗页面中，让玩家无需阅读调试日志即可判断：

- 双方当前生命与最大生命；
- 玩家当前 Guard、敌人当前 Shell；
- 玩家与每个敌人的当前 Buff/Debuff、层数与剩余时间；
- 当前道具是什么、如何触发、作用于谁、实际属性是多少，以及它是否正在贡献战斗。

本包只负责正式表现和已有 Prefab 的布局/绑定，不改变 Battle、Item、Reward、Stage、Save 或数值语义。

## 2. Accepted Water Evidence

- `Canonical Item Detail Player Semantics Focused Verify`: `PASS / assertions=128`.
- `Canonical Effect Operator Focused Verify`: `COMPLETE / checks=43`.
- Formal authoritative source: existing `C1FormalRealtimeBattleSessionStateSnapshot`.
- Item semantic source: existing `C1FormalItemDetailProjectionAndSelection`.

## 3. Carrier Audit

- `FormalBattlePlayerPresentation.prefab`: `KEEP / EXTEND_IN_PLACE`. Existing avatar, HP frame, HP fill, HP label and hit feedback are retained. Guard and status visuals are missing.
- `FormalBattleEnemySlot.prefab`: `KEEP / EXTEND_IN_PLACE`. Existing enemy visual, identity, HP frame, HP fill, HP label, target/telegraph/hit feedback are retained. Shell and status visuals are missing.
- `FormalBattlePresentationRoot.prefab`: `KEEP / FORMAL_BINDING_ROOT`. It already consumes the live state each frame and is the only presentation coordinator.
- `C1ExactBattleSandboxItemDetailPopupStandalone.prefab`: `KEEP / SIMPLIFY_PLAYER_READING_ORDER_IN_PLACE`. It already owns header, artwork, scroll content, fifteen player sections and three debug sections; no replacement popup is allowed.
- BattleSandbox `PlayerBuffAnchor`, `PlayerDebuffAnchor`, `EnemyBuffAnchor`, `EnemyDebuffAnchor` and their authored icon hierarchy: `VISUAL_ONLY_REFERENCE`.
- V02 `StatusAnchorUI`, `StatusIconView`, `StatusEffectController` and `AutoCombatController`: `LEGACY_RUNTIME / DO_NOT_MIGRATE`.

## 4. Required Implementation

1. Add one presentation-only Formal status-strip carrier reusable by player and enemy views. It consumes immutable `C1FormalRealtimeBattleStatusSnapshot` rows only and owns no gameplay state.
2. Bind player HP + Guard + player-targeted statuses from the current Formal state.
3. Bind enemy HP + Shell + statuses by the existing exact actor identity/stable order.
4. Use presentation data for status display name, polarity, a real icon Sprite and color; do not route by Item ID and do not read V02 Runtime. Status keys are routing identities only. A missing icon must stop at the Decor asset boundary and be filled with a generated or authored image placeholder; Unicode/TMP glyphs and field text are forbidden as status-icon fallbacks.
5. Preserve the existing item-detail header, artwork, rarity and scroll carrier. Put `stats`, `trigger` and `basic` first and make empty/non-player debug sections invisible on the normal player route.
6. Preserve existing Unified pulse/ribbon/impact VFX, Card VFX, damage floats, targeting, enemy animation and input behavior.
7. For this specific status package, the Decor audit has identified the shared authored `FormalBattleStatusSlot` / `StatusSlot_00` image structure as the correct carrier. New status artwork must be assigned to its existing `StatusIcon` slot without changing the user's authored RectTransform, slot size, spacing, alignment or sibling order. This carrier choice is an audit result for this package, not a global hardcoded answer for future visual tasks.

## 5. Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattlePresentationRoot.cs`
- `Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattlePlayerPresentationView.cs`
- `Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleEnemySlotView.cs`
- one presentation-only Formal status-strip/icon View under the same folder if needed
- `Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattlePresentationProfile.cs` and its existing formal Profile asset only for status visual rows
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` only for normal-route visual ordering/visibility
- `Assets/_Game/Resources/角色敌人UI/` only for missing HUD/status image assets required by this Decor package
- the three existing Prefabs named in section 3
- existing focused Presentation/Item Detail verifier or one temporary focused in-place authoring entry that is removed after successful serialization
- this contract and `PROJECT_CLEANROOM_COMPONENT_LEDGER.md`

## 6. Forbidden Writes

- all Battle, Item Authority, Canonical Item, Reward, Board/Tray, Stage, Enemy, Save and WorldMap logic/data
- all Scene files
- V02/BattleSandbox Runtime or status controllers
- a second HUD, Detail popup, status database, Battle adapter, provider or fallback
- Item-ID presentation branches
- full Prefab reconstruction when an in-place extension is sufficient

## 7. Unity Lease

- Current state: `EXTERNAL UNITY OWNER`.
- Read-only audit and contract freeze are complete.
- Code preparation may begin only as a coherent package; Prefab/Profile serialization and visual verification require `CLOSED_CLEAN` and one explicit owned lifecycle.
- Never start, stop or attach to the user-owned Unity process.

## 8. Verification And Stop

- Compile: zero errors.
- Focused binding proof: player Guard, enemy Shell and exact-target status rows update from one state snapshot; removed statuses disappear.
- Prefab proof: existing carriers remain single-owner and all newly required references are assigned.
- Status visual coverage: every status reachable on the current Formal route resolves an exact non-null Sprite in the shared StatusSlot; no glyph/text fallback is used.
- Normal-route visual proof: one encounter shows readable HP/Guard/Shell/statuses and a readable real reward Item Detail without debug leakage.
- Then STOP. Do not continue into gameplay, balance, reward, persistence or VFX changes.

## 9. Current State

`DECOR_AUDIT_COMPLETE / CONTRACT_FROZEN / EXTERNAL_UNITY_OWNER / PREFAB_WRITES_NOT_STARTED / NOT_READY`

## 10. I031 Nian Generation Decor Addendum

Accepted water boundary: the live Formal snapshot publishes the exact I031
source instance, Battle-time generation schedule and current/max Nian; the
ordered cue publishes the committed `ITEM_TRIGGER / PERIODIC / NIAN_GAIN /
Player` result and actual applied amount. Decor does not own or infer these
facts.

This addendum permits the smallest presentation-only extension needed to:

- show Battle-time-driven charge progress on the exact placed+lit I031 view;
- play the existing source carrier, ribbon, Player Nian receiver pulse and
  actual semantic feedback after the committed cue;
- author a real charge-ring Sprite and inspector-editable special-source/VFX
  appearance without changing user-authored HUD positions.

Additional allowed writes:

- `Assets/_Game/Scripts/TalismanBag/Presentation/Items/ItemRarityContourBloomVfx.cs`
- `Assets/_Game/Scripts/TalismanBag/Presentation/Items/ItemRarityContourBloomProfile.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs` presentation binding only
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs` presentation binding only
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs` exact read-only presentation-source resolver only
- `Assets/_Game/Scripts/TalismanBag/Editor/Presentation/FormalBattle/FormalBattleI031NianDecorAuthoring.cs`
- existing `ItemRarityContourBloomVfx.prefab`, rarity VFX Profile and Formal
  Presentation Profile
- `Assets/_Game/Resources/角色敌人UI/I031NianPeriodicChargeRing_V1.png`

Battle Session/contracts, Item Authority/placement rules, I031 generation
values, Scenes and all user-authored HUD RectTransforms remain forbidden.
Current state: `I031_DECOR_INTERNAL_QA / NORMAL_ROUTE_VISUAL_NOT_READY`.
