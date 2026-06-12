# Development Log

## 2026-06-10 - V0.2 Step 7 Balance Counter Validation

### Balance Data

- Added V0.2 counter relation and multiplier config:
  - StrongCounter 1.8
  - LightCounter 1.35
  - Neutral 1.0
  - Resisted 0.7
  - HardResisted 0.55
  - reward shield break 2.5
  - group clear 1.6
- Added enemy element/function resistance and vulnerability fields.
- Centralized V0.2 enemy HP, attack, skill timing, shield, summon, poison, steal, seal, and Boss phase numbers in the scene builder.

### Runtime Logging

- Added `BattleBalanceRecord` and `BattleBalanceLogger`.
- Battle end now emits `[Balance]` records with enemy data, build tags, powered/unpowered counts, damage, duration, remaining HP, counter relation, counter triggers, and inferred failure reason.
- In editor/local dev, balance records also append to `Temp/V02BalanceRecords.log`.

### Validation Tools

- Added `V02BuildTestRunner` with preset build comparisons:
  - FireBasicBuild
  - ThunderShieldBreakBuild
  - ChainGroupClearBuild
  - PurifyAntiPoisonBuild
  - SoulAntiStealBuild
  - SpreadAntiSealBuild
  - BossReadyBuild
  - BadUnpoweredBuild
- Added six `[BalanceTest]` debug popup buttons for shield, group, poison, steal, seal, and Boss enemies.

## 2026-06-10 - V0.2 Step 6 Seven-Round Formation Counter Run

### Run Config

- Added `V02RunConfig`, `V02RoundConfig`, and `V02RunState`.
- Generated `Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset`.
- Configured 7 rounds:
  - Round 1: basic formation power
  - Round 2: shield break
  - Round 3: group clear
  - Round 4: poison/burn cleanse
  - Round 5: anti energy-steal
  - Round 6: anti seal / spread layout
  - Round 7: formation breaker Boss

### Run Flow

- Reworked `V02RunFlowController` around `Init -> Prep -> Combat -> Reward -> RunWin/RunLose`.
- Non-final wins now open reward selection with the next enemy preview.
- Reward selection advances to the next round.
- Round 7 victory goes directly to run clear.
- Any V0.2 defeat now opens the V0.2 failure review instead of the old generic result panel.

### Boss

- Added `V02BossPhaseController`.
- Formation Breaker Boss now switches by HP:
  - 100%-70%: shield pressure
  - 70%-40%: summon/group pressure
  - 40%-0%: formation-eye seal pressure
- Boss phase changes update intent text and emit battle log feedback.

### Failure Review And Stats

- Added `V02FailureReason`, `V02FailureReasonResolver`, `V02RunStatsTracker`, and `V02RunResultPanel`.
- Failure review can identify:
  - unpowered formation
  - lack of shield break
  - lack of cleanse
  - formation energy disruption
  - core seal pressure
  - lack of group clear
  - mixed Boss pressure
  - low defense
- Run stats track shield break, cleanse, unseal, soul suppress, chain clear, formation protection, powered/unpowered talismans at battle start, formation disruption, and sealed items.

### Debug

- Extended the left-bottom `debug` popup with:
  - Start New V02 Run
  - Skip to Round 1-7
  - Force Win Current Round
  - Force Lose Current Round
  - Give Anti Shield / Group / Poison / Steal / Seal / Boss Ready builds
  - Print Failure Tracker
  - Print Run Stats
  - Reset V02 Run

### Verification

- Ran Unity batchmode to regenerate `Scene_TalismanBag_V02_FormationCounter`.
- First pass had expected transient import errors for newly added namespaces while Unity imported new scripts.
- Second pass completed with `Tundra build success`, no `error CS`, and regenerated the V0.2 scene.
- Confirmed `RunConfig_V02_15Min.asset` contains all 7 rounds and the Boss round flag.
- Confirmed the scene contains the V0.2 result panel and new debug buttons.

## 2026-06-10 - V0.2 Debug Popup And Formal UI Cleanup

### UI Structure

- Added `V02DebugPopupController` for a left-bottom `debug` entry and closeable secondary popup.
- Moved V0.2 test/debug controls into `V02DebugPopup` instead of placing them on the formal bottom operation area.
- Kept only player-facing primary actions in the formal bottom UI:
  - Refresh Power
  - Start Battle
  - Reset Formation
- Reduced the formal bottom panel height and enlarged the primary action buttons for mobile thumb use.

### Combat Stage Layout

- Added `V02PlayerAvatar` to the V0.2 auto-combat stage.
- Rebalanced the combat stage columns so player avatar, enemy preview, enemy intent, and player status no longer crowd the same space.
- Rebuilt `Scene_TalismanBag_V02_FormationCounter` with the new layout.

### Verification

- First Unity batchmode pass hit the expected transient import timing issue for the newly added controller.
- Second Unity batchmode pass completed successfully and regenerated the V0.2 scene.
- Checked the generated scene contains `DebugOpenButton`, inactive `V02DebugPopup`, `DebugButtonGrid`, `V02PrimaryActionButtons`, and `V02PlayerAvatar`.
- Confirmed the old formal bottom `DebugButtons` grid is no longer generated into the scene.

## 2026-06-09 - Phase 1: Talisman Bag Combat POC

### Added Scripts

- `Assets/_Game/Scripts/TalismanBag/Items/TalismanItemDefinition.cs`
  - ScriptableObject item definition for talisman bag items.
  - Defines `TalismanItemType` and `ElementType`.
- `Assets/_Game/Scripts/TalismanBag/Items/TalismanItemRuntime.cs`
  - Runtime state for placed items, cooldowns, trigger counts, and reserved seal state.
- `Assets/_Game/Scripts/TalismanBag/Grid/TalismanBagGrid.cs`
  - 5x5 grid placement, removal, lookup, adjacency checks, and placed item enumeration.
- `Assets/_Game/Scripts/TalismanBag/Combat/CombatStats.cs`
  - Player and ghost HP, shield, mana, damage, and reset helpers.
- `Assets/_Game/Scripts/TalismanBag/Combat/AutoCombatController.cs`
  - Auto combat loop, item triggers, mana spending, ghost attacks, win/loss checks, and debug actions.
- `Assets/_Game/Scripts/TalismanBag/UI/TalismanGridSlotView.cs`
  - Grid slot visuals, hover state, drop target behavior, combo highlight, and flash feedback.
- `Assets/_Game/Scripts/TalismanBag/UI/DraggableTalismanItemView.cs`
  - Mouse drag/drop for starter items and grid-to-grid movement.
- `Assets/_Game/Scripts/TalismanBag/UI/TalismanCombatUI.cs`
  - HP, shield, mana, enemy HP, state text, HP flash, shield feedback, and enemy shake.
- `Assets/_Game/Scripts/TalismanBag/UI/BattleLogUI.cs`
  - Last 8 combat log lines.
- `Assets/_Game/Scripts/TalismanBag/Editor/TalismanBagSceneBuilder.cs`
  - Editor builder for item assets, basic prefabs, and the phase 1 test scene.

### Added Scene

- `Assets/_Game/Scenes/Scene_TalismanBag_CombatPOC.unity`
  - Generated by `TalismanBagSceneBuilder`.
  - Contains a 2D UI layout with player placeholder, 5x5 talisman bag, ghost placeholder, top stats, starter item bar, debug buttons, and battle log.

### Added Item Definitions

- `spirit_stone_basic`
  - Generates 12 mana every 2 seconds.
- `fire_talisman_basic`
  - Costs 10 mana, deals 12 damage, cooldown is 2.5 seconds or 1.6 seconds next to a spirit stone.
- `shield_talisman_basic`
  - Costs 8 mana, grants 18 shield, shield cap is 50.
- `qi_pill_basic`
  - Costs 12 mana when player HP is below 50, heals 20 HP or 28 HP next to a shield talisman.

### Implemented Features

- 5x5 talisman bag grid.
- Mouse drag/drop from starter bar to grid.
- Grid-to-grid item movement.
- Auto combat start/reset.
- Spirit stone mana generation.
- Fire talisman attack and spirit stone adjacency cooldown boost.
- Shield talisman shield generation.
- Qi pill low-HP healing and shield talisman adjacency healing boost.
- Ghost auto attack every 2.5 seconds.
- Shield-first damage handling.
- Victory when ghost HP reaches 0.
- Defeat when player HP reaches 0.
- Realtime stat UI.
- Last 8 battle log entries.
- Combo highlights for:
  - Fire talisman next to spirit stone.
  - Qi pill next to shield talisman.
- Debug buttons:
  - `Reset Battle`
  - `Auto Place Starter Build`
  - `Damage Player 20`
  - `Add Mana 30`

### How To Verify

1. Open `Assets/_Game/Scenes/Scene_TalismanBag_CombatPOC.unity`.
2. Press Play.
3. Drag the four starter items into the 5x5 grid, or click `Auto Place Starter Build`.
4. Click `开始斗法`.
5. Confirm the battle log shows:
   - Spirit stone mana generation.
   - Fire talisman attack.
   - Fire talisman speed boost when adjacent to spirit stone.
   - Shield talisman shield gain.
   - Ghost attacks.
   - Qi pill healing when HP is below 50.
6. Move fire talisman away from spirit stone and compare attack frequency.
7. Move qi pill away from shield talisman and compare heal amount.

### Not Implemented In Phase 1

- Full five-element system.
- Shop flow.
- Roguelike route map.
- Multiple enemy types.
- Formal art assets.
- Complex character animation.
- Save/load.
- Networking.
- Full 10-item pool.
- Complex VFX.

### Next Phase Suggestions

- Add thunder talisman, seal, sword pill, peach wood token, exorcism bell, and water talisman.
- Add sword cultivator and evil cultivator enemy mechanics.
- Add simple 3-choice shop rewards.
- Add a run flow controller for multiple rounds.
- Expand combo detection into a dedicated resolver.

## 2026-06-09 - Phase 2: Enemy Counters, Shop Flow, and Combo System

### Added Scripts

- `Assets/_Game/Scripts/TalismanBag/Enemies/EnemyDefinition.cs`
  - ScriptableObject enemy configuration with enemy type, HP, attack values, weakness text, and danger text.
- `Assets/_Game/Scripts/TalismanBag/Enemies/EnemyRuntime.cs`
  - Runtime enemy state for HP, attack timers, charging, special timers, and seal timers.
- `Assets/_Game/Scripts/TalismanBag/Run/RunFlowController.cs`
  - Four-round run flow: ghost, elite ghost, sword cultivator, evil cultivator.
- `Assets/_Game/Scripts/TalismanBag/Shop/ShopController.cs`
  - Victory shop with 3 random options and one selected item added to the inventory bar.
- `Assets/_Game/Scripts/TalismanBag/Shop/ShopOptionView.cs`
  - Single shop option UI view.
- `Assets/_Game/Scripts/TalismanBag/Combo/ComboResolver.cs`
  - Detects active adjacent combos in the 5x5 grid.
- `Assets/_Game/Scripts/TalismanBag/UI/ComboStatusUI.cs`
  - Displays active combo names or a no-combo message.
- `Assets/_Game/Scripts/TalismanBag/UI/EnemyPreviewUI.cs`
  - Displays current round, enemy, weaknesses, and dangers.

### Updated Scripts

- `AutoCombatController`
  - Now receives current enemies from `RunFlowController`.
  - Handles thunder talisman, seal, sword pill, peach wood token, exorcism bell, and water talisman.
  - Handles sword cultivator charging and thunder interruption.
  - Handles evil cultivator mana drain and temporary item sealing.
  - Notifies `RunFlowController` on battle win/loss.
- `TalismanGridSlotView`
  - Added sealed overlay support.
- `DraggableTalismanItemView`
  - Added stable home-position capture for shop-added inventory items.
- `TalismanCombatUI`
  - Displays the active enemy instead of a fixed ghost stat line.
- `TalismanItemDefinition`
  - Added `PassiveTool` and `SupportTalisman` item types.
- `TalismanItemRuntime`
  - Added `sealRemaining` for evil cultivator seal duration.
- `TalismanBagSceneBuilder`
  - Now generates the full phase 2 scene, 10 item definitions, 4 enemy definitions, shop UI, combo UI, enemy preview UI, and debug buttons.

### Added Enemies

- `ghost_basic`
  - 80 HP, 8 damage, 2.5 second attack interval.
- `ghost_elite`
  - 110 HP, 10 damage, 2.2 second attack interval.
- `sword_cultivator_basic`
  - 120 HP, 14 damage, 3 second attack interval, 8 second charge cycle, 30 damage slash.
- `evil_cultivator_basic`
  - 140 HP, 12 damage, 3.5 second attack interval, 6 second mana drain, 10 second seal cycle.

### Added Items

- `thunder_talisman_basic`
  - Thunder damage, bonus damage to ghosts, adjacent seal crit, interrupts sword cultivator charge.
- `seal_basic`
  - Passive tool, strengthens adjacent thunder talisman and fire talisman.
- `sword_pill_basic`
  - Fast physical attack, activates fire sword flow next to fire talisman.
- `peach_wood_basic`
  - Passive ghost suppression, activates exorcism array next to exorcism bell.
- `exorcism_bell_basic`
  - Exorcism attack, strong into ghosts, weak into sword cultivator.
- `water_talisman_basic`
  - Healing support, shortens qi pill cooldown when adjacent.

### Added Combos

- `火灵连发`
  - Fire talisman next to spirit stone, fire cooldown becomes 1.6 seconds.
- `护丹`
  - Qi pill next to shield talisman, healing increases from 20 to 28.
- `雷印`
  - Thunder talisman next to seal, 30% crit chance.
- `火剑流`
  - Sword pill next to fire talisman, sword pill gains fire damage and is stronger into sword cultivator.
- `驱邪阵`
  - Peach wood token next to exorcism bell, damage to ghost-type enemies increases.
- `水丹回气`
  - Water talisman next to qi pill, qi pill cooldown is reduced by 20%.

### Shop Flow

1. Win a battle.
2. Shop appears with 3 random item options from the 10-item pool.
3. Pick one item.
4. The item is added to the bottom inventory bar.
5. Click `下一场` to continue.
6. Re-arrange the 5x5 grid before starting the next battle.

### How To Verify Phase 2

1. Open `Assets/_Game/Scenes/Scene_TalismanBag_CombatPOC.unity`.
2. Press Play.
3. Use `Auto Place`, then click `开始斗法`.
4. Win Round 1 and pick one shop item.
5. Continue through:
   - Round 1: ordinary ghost.
   - Round 2: elite ghost.
   - Round 3: sword cultivator.
   - Round 4: evil cultivator.
6. Use debug buttons for targeted checks:
   - `Skip To Sword` tests sword cultivator charge and thunder interruption.
   - `Skip To Evil` tests mana drain and seal.
   - `Add Random Item` tests shop/inventory insertion.
   - `Clear All Seals` clears evil cultivator seals.
7. Confirm the combo panel updates when adjacent combos are formed.
8. Confirm the enemy preview changes per round.

### Not Implemented In Phase 2

- Full five-element system.
- Full roguelike map.
- Sect/faction system.
- Save/load.
- Equipment quality.
- Item upgrades.
- Formal art.
- Complex skill animation.
- Formal sound effects.
- Full numeric balance.

### Next Phase Suggestions

- Improve visual readability with mana flow lines and stronger trigger animations.
- Add a post-battle summary panel with trigger counts, damage taken, mana wasted, and strongest combo.
- Add small balance presets for the three enemy archetypes.
- Add more explicit enemy behavior warnings during combat.

## 2026-06-09 - Phase 3: Readability, Mana Flow, and Combat Feedback

### Added Scripts

- `Assets/_Game/Scripts/TalismanBag/Feedback/BattleEventRouter.cs`
  - Lightweight battle event hub used by combat, logs, floating text, mana flow, enemy feedback, and statistics.
- `Assets/_Game/Scripts/TalismanBag/Feedback/FloatingCombatText.cs`
  - Displays short floating text for damage, healing, shield, mana, interruption, counter, seal, and unseal events.
- `Assets/_Game/Scripts/TalismanBag/VFX/ManaFlowView.cs`
  - Shows short blue UI flow lines from spirit stones to adjacent mana-consuming items.
- `Assets/_Game/Scripts/TalismanBag/Feedback/TalismanTriggerFeedback.cs`
  - Plays simple item color flashes and scale pulses when talismans trigger.
- `Assets/_Game/Scripts/TalismanBag/Feedback/ComboHighlightController.cs`
  - Handles active combo slot highlights and forced combo-highlight debug mode.
- `Assets/_Game/Scripts/TalismanBag/Feedback/EnemyFeedbackController.cs`
  - Handles enemy hit flash, shake, sword charge bar, and interruption feedback.
- `Assets/_Game/Scripts/TalismanBag/Combat/BattleStatsTracker.cs`
  - Tracks per-battle damage, damage taken, healing, shield gain, mana generated/spent/wasted, trigger count, and active combos.
- `Assets/_Game/Scripts/TalismanBag/UI/BattleResultPanel.cs`
  - Shows single-battle result statistics after victory or defeat.

### Updated Scripts

- `AutoCombatController`
  - Emits battle events instead of directly owning every presentation concern.
  - Emits typed events for mana, damage, defense, healing, combos, counters, seals, interruption, and win/loss.
  - Tracks mana overflow waste through `BattleStatsTracker`.
  - Added debug actions:
    - `Trigger Feedback`
    - `Toggle Combos`
    - `Test Floating`
    - `Force Charge`
    - `Force Seal`
- `BattleLogUI`
  - Subscribes to battle events and displays category prefixes:
    - `[灵气]`
    - `[伤害]`
    - `[防御]`
    - `[治疗]`
    - `[阵法]`
    - `[危险]`
    - `[克制]`
    - `[封印]`
    - `[胜负]`
- `ComboStatusUI`
  - Shows combo names and their mechanical effects.
  - Hovering the combo panel emphasizes active combo slots.
- `EnemyPreviewUI`
  - Uses clearer stage-intel formatting with weaknesses, dangers, and recommended formations.
- `TalismanBagSceneBuilder`
  - Generates phase 3 scene UI:
    - Feedback root.
    - Floating text root.
    - Mana flow UI.
    - Sword charge bar.
    - Battle result panel.
    - New debug buttons.

### Mana Flow

- When `spirit_stone_basic` generates mana, `AutoCombatController` checks adjacent items.
- If adjacent items consume mana, it emits a mana flow event with source and target positions.
- `ManaFlowView` displays a short blue UI line from the spirit stone slot to the adjacent item slot.

### Floating Text Types

- Mana gained/spent.
- Damage dealt/taken.
- Healing received.
- Shield gained.
- Enemy countered.
- Sword cultivator interrupted.
- Item sealed/unsealed.

### Enemy Feedback

- Ghost counters now emit `克制！` feedback through counter events.
- Sword cultivator charge shows a visible charge bar and `连斩蓄力中`.
- Thunder interruption clears the charge bar and emits `打断！`.
- Evil cultivator seal emits `封印！`, and sealed slots keep the phase 2 gray overlay.

### Result Panel

The result panel shows:

- Strongest active formation.
- Talisman trigger count.
- Total damage dealt.
- Damage taken.
- Healing.
- Shield gained.
- Mana generated.
- Mana spent.
- Mana overflow waste.

### Not Implemented In Phase 3

- Formal VFX or particles.
- Formal art assets.
- Complex animations.
- Full tooltip system per individual combo row.
- Formal audio feedback.
- Full numeric rebalance.

### Next Phase Suggestions

- Add stronger authored UI art for talisman frames and enemy silhouettes.
- Add sound placeholders for trigger categories.
- Add a more polished post-run summary after Round 4.
- Add optional graph-style battle timeline for damage, mana, and seals.

## 2026-06-09 - Phase 4: Mobile Portrait 15-Minute Run Prototype

### Added Scripts

- `Assets/_Game/Scripts/TalismanBag/Run/RunConfig.cs`
  - ScriptableObject run configuration for the full 7-round validation run.
- `Assets/_Game/Scripts/TalismanBag/Run/RoundConfig.cs`
  - Per-round enemy, reward, unlock, title, and hint data.
- `Assets/_Game/Scripts/TalismanBag/Economy/SpiritJadeWallet.cs`
  - Spirit jade wallet with reset, earn, spend, and top-bar UI refresh.
- `Assets/_Game/Scripts/TalismanBag/Shop/ShopItemPriceConfig.cs`
  - Shop price table for all talisman item definitions.
- `Assets/_Game/Scripts/TalismanBag/Shop/ShopControllerV2.cs`
  - Four-item mobile shop, multi-buy support, refresh cost, sold-out state, and next-round flow.
- `Assets/_Game/Scripts/TalismanBag/Run/RunFlowControllerV2.cs`
  - 7-round run flow for the 15-minute prototype, rewards, shop entry, bag upgrade unlock, win/lose states, and restart.
- `Assets/_Game/Scripts/TalismanBag/Progression/PlayerTalismanInventory.cs`
  - Lightweight player inventory model for starting item setup.
- `Assets/_Game/Scripts/TalismanBag/Progression/ItemLevelSystem.cs`
  - Entry point for auto-merging duplicate level-1 items into level 2.
- `Assets/_Game/Scripts/TalismanBag/Progression/BagExpansionController.cs`
  - Unlocks enhanced grid slots at `(1,1)` and `(3,3)` after Round 4, reducing active item cooldown by 15%.
- `Assets/_Game/Scripts/TalismanBag/Progression/BuildArchetypeResolver.cs`
  - Resolves post-run archetypes such as fire-sword, thunder-seal, exorcism, shield-pill, and fire-spirit builds.
- `Assets/_Game/Scripts/TalismanBag/UI/TutorialHintController.cs`
  - Displays short round hints for mobile portrait play.

### Updated Scripts

- `TalismanBagSceneBuilder`
  - Builds `Scene_TalismanBag_Run15Min.unity` as the new mobile portrait primary scene.
  - Sets portrait defaults, 1080x1920 reference resolution, Canvas scaling, 5x5 center-lower board, compressed 3-line log, bottom thumb controls, single-column shop, and portrait result panel.
  - Generates English/pinyin asset paths only.
- `AutoCombatController`
  - Uses `RunFlowControllerV2`, `ShopControllerV2`, and `BagExpansionController`.
  - Keeps run-level stats across rounds and resets them only when a new run starts.
  - Adds Lv2 item values, auto merge, enhanced-slot cooldown scaling, boss behavior, and mobile button wiring.
- `DraggableTalismanItemView`
  - Supports mouse/touch drag, grid-to-grid movement, grid-to-inventory return, failed-drop rebound, drag scale-up, and an upward drag offset to reduce finger occlusion.
- `TalismanGridSlotView`
  - Adds enhanced slot visuals.
- `BattleLogUI`
  - Compresses mobile logs to the latest 3 key entries.
- `BattleResultPanel`
  - Shows run-level settlement data, archetype, strongest formation, failure reason or rating, purchases, merges, interrupts, and seals.
- `EnemyPreviewUI`
  - Adds Boss weakness and danger tags.
- `TalismanTriggerFeedback`
  - Fixes spirit stone trigger feedback so it restores its base scale and no longer grows without limit.

### Added Run Content

- Round 1: `ghost_basic`
- Round 2: `ghost_elite`
- Round 3: `sword_cultivator_basic`
- Round 4: `evil_cultivator_basic`, unlocks enhanced slots after victory
- Round 5: `ghost_swarm`
- Round 6: `sword_cultivator_elite`
- Round 7: `heart_demon_boss`

### Boss Behavior

- Heart demon boss absorbs mana.
- Temporarily seals placed items.
- Charges a heavy heart-demon strike that can be interrupted by thunder talisman.
- Enters an enraged state below half HP and seals more often.

### Mobile Portrait Requirements Covered

- Primary layout is 9:16 portrait with 1080x1920 reference resolution.
- Top bar keeps HP, shield, mana, spirit jade, state, and round info visible.
- Enemy area uses short weakness and danger tags.
- Combat feedback area stays compact.
- 5x5 grid is the visual and interaction center.
- Bottom inventory supports horizontal scrolling and thumb-sized buttons.
- Shop uses a single-column four-card layout.
- Main buttons are touch-sized and placed in the lower operation area.
- Dragging uses an upward icon offset and larger drag scale for touch readability.

### How To Verify Phase 4

1. Generate/open `Assets/_Game/Scenes/Scene_TalismanBag_Run15Min.unity`.
2. Test Game View sizes:
   - `1080x1920`
   - `720x1280`
   - `1170x2532`
   - `1440x2960`
3. Confirm the 5x5 grid, inventory, shop, buttons, top bar, result panel, and drag/drop do not overlap or overflow.
4. Play through all 7 rounds and confirm spirit jade rewards, shop buying, refresh cost, sold-out cards, Lv2 merge, Round 4 enhanced slots, Boss absorb/seal/charge/enrage, and final settlement.

## Phase 5 Mobile Playtest Polish

### Vertical UI Polish

- `Scene_TalismanBag_Run15Min` is rebuilt as the phone portrait playtest scene.
- Canvas Scaler uses `Scale With Screen Size`, `1080 x 1920`, and `Match Width Or Height = 0.5`.
- `MobileSafeAreaRoot` is clamped to valid anchors and `MobilePortraitLayoutRuntimeFix` adapts panel width/height for tall devices such as Huawei P40 Pro.
- The board, info area, bottom inventory, shop, result panel, version text, and debug panel are all generated inside the safe-area UI root.

### Touch Drag Feel

- `DraggableTalismanItemView` keeps mouse/touch drag support, drag scale-up, and a finger-offset icon.
- `TalismanGridSlotView` now shows red invalid target feedback when dragging over occupied cells, flashes red on failed drops, and dims sealed item icons.
- Returning items to the bottom inventory and grid-to-grid movement still use the existing grid runtime model.

### Tutorial Hints

- Round hints cover Round 1, ghosts, sword cultivator interrupts, evil cultivator seals, and the Boss.
- `TutorialHintController` now listens for fire talisman adjacent to spirit stone and shows: `火灵连发已激活：火符触发速度提高。`

### Combat Speed

- Added `CombatSpeedController`.
- Bottom controls include a `1x / 2x` toggle.
- 2x multiplies combat simulation delta time only; it does not change `Time.timeScale`, so dragging, shop, and UI input remain normal.

### Mobile Shop

- Shop cards remain single-column vertical cards.
- Sold-out cards display `已售出`.
- Refresh button displays `刷新 2 灵玉`.
- Shop footer includes `刷新商店 / 自动合成 / 下一场`.
- Purchases and shop refreshes write `[Playtest]` logs to the Unity Console.

### Merge UI

- Auto merge button now displays `暂无可合成` or `可合成：道具 Lv2`.
- Successful merge logs `道具 Lv1 ×2 合成为 道具 Lv2`.
- Merged items are returned to the inventory and grid references are refreshed.

### Boss Readability

- Boss HP text shows `狂暴` and `蓄力` tags.
- Enemy feedback displays `心魔冲击蓄力中`, charge progress, and temporary `打断！` text.
- Sealed grid items are dimmed and keep the seal overlay visible.
- Force Boss Phase 2 is available from the hidden playtest debug panel.

### Result Panel

- Result body is reorganized for mobile reading:
  - Victory/defeat title.
  - Current round.
  - Archetype and strongest combo.
  - Core evaluation or failure reason.
  - Key data first: damage, damage taken, healing, shield, thunder interrupts, seals.
  - Detailed data below: mana generated/spent/wasted, purchases, merges, trigger count.

### Debug Panel

- Added visible version text: `TalismanBag v0.1.0-Run15-Mobile`.
- Tap the version text 5 times to toggle the hidden `Playtest Debug` panel.
- Debug tools include:
  - Skip To Round 1-7.
  - Add 20 Spirit Jade.
  - Give All Basic Items.
  - Give Lv2 Core Build.
  - Force Boss Phase 2.
  - Finish Run Win.
  - Finish Run Lose.
  - Reset Run.

### Android Build Settings

- Company Name: `Prototype`.
- Product Name: `TalismanBagPrototype`.
- Bundle Version: `0.1.0`.
- Default size remains `1080 x 1920`.
- Portrait autorotation is enabled and landscape autorotation is disabled.
- Upside-down portrait is disabled for a cleaner phone playtest build.
- `androidRenderOutsideSafeArea` is disabled and max aspect ratio is raised for tall phones.
- Build Settings include `Assets/_Game/Scenes/Scene_TalismanBag_Run15Min.unity`.

### Playtest Logging

- Added `PlaytestSessionLogger`.
- Console logs include run start, round start/win/loss, shop opened, item purchased, item merged, Boss started, run won/lost, and detected archetype.

### Current Gaps

- APK export itself was not produced in this pass; the project is prepared for Android portrait export.
- No formal analytics SDK is connected by design.
- The debug panel is functional but intentionally utilitarian rather than final product UI.

### Suggested Next Phase

- Run on a real Android device and tune touch comfort.
- Record one full 15-minute producer playtest and adjust first-run hints from observed confusion.
- Add a small boot scene only if build flow needs loading or device checks.

## Phase 4 Step 1 Compatibility Pass

This pass restores the naming and API surface requested by the older Phase 4 Step 1 brief without rolling back the later mobile playtest features.

- Added `Assets/_Game/Scripts/TalismanBag/UI/Mobile/` with lightweight mobile HUD, round info, battle log, and run control panel components.
- Added public compatibility fields to `RunFlowControllerV2`: `runConfig`, `currentRoundIndex`, and `currentState`.
- Added compatibility run state names such as `Init`, `Prep`, `Combat`, `RoundWin`, `RoundLose`, `Shop`, `Boss`, `RunWin`, and `RunLose`.
- Added `RoundConfig.isBossRound` and the `TalismanBag/Run Config` asset creation menu.
- Generated `Assets/_Game/ScriptableObjects/TalismanBag/RunConfigs/RunConfig_15Min_Test.asset` for the 7-round mobile test run.
- Updated `Scene_TalismanBag_Run15Min` to reference `RunConfig_15Min_Test.asset`.

Notes:

- The current prototype intentionally keeps the later Phase 5 shop, merge, boss, result, speed, version, and debug-panel features.
- The mobile Canvas remains portrait-first at `1080 x 1920` with `Match Width Or Height = 0.5`.
- Use the hidden playtest debug panel to jump through rounds 1-7 for quick verification.

## Phase 4 Step 2 Combat Pacing Pass

This pass configures the 7-round run so each fight has a distinct enemy pressure, short pre-battle purpose, weakness tags, danger tags, recommended build, and target duration.

- Expanded `EnemyDefinition` with `GhostSwarm`, recommended build text, charge tuning, mana drain tuning, seal tuning, and ghost-shadow attack tuning.
- Updated `RunConfig_15Min_Test.asset` with 7 rounds:
  - Round 1: `ghost_basic`, core fire + spirit-stone loop, 30-45 seconds.
  - Round 2: `ghost_elite`, thunder/peach/exorcism counter learning, 45-60 seconds.
  - Round 3: `sword_cultivator_basic`, charge slash and thunder interrupt, 60-75 seconds.
  - Round 4: `evil_cultivator_basic`, mana drain and seal pressure, 60-75 seconds.
  - Round 5: `ghost_swarm`, single enemy with ghost-shadow attack, 75-90 seconds.
  - Round 6: `sword_cultivator_elite`, faster charge slash pressure, 75-90 seconds.
  - Round 7: `heart_demon_boss_placeholder`, placeholder final pressure fight, 90-120 seconds.
- Reworked enemy behavior dispatch in `AutoCombatController` into basic attack, sword charge, evil cultivator special, ghost swarm special, and Boss placeholder methods.
- Boss placeholder currently reuses the evil cultivator drain/seal pattern and does not auto-enter the full heart-demon charge/enrage tree.
- Thunder and exorcism counters now treat `GhostSwarm` as a ghost-family enemy.
- Battle logs keep only 3 lines, but danger/counter/seal/result entries are protected from being immediately pushed out by lower-priority damage/mana logs.
- Enemy preview and mobile HUD now read weakness, danger, and recommended build text from enemy configs.
- Added hidden debug-panel support for `Recommended Build`, which grants and auto-places a round-specific test build for the current round.

Verification:

- Ran `TalismanBag.EditorTools.TalismanBagSceneBuilder.BuildAll` to regenerate the scene and ScriptableObjects.
- Ran Unity batchmode compile verification with no C# compilation errors found in `Logs/phase4_step2_final_compile.log`.

## Phase 4 Step 3 Spirit Jade Shop Pass

This pass turns post-fight rewards into a mobile shop choice loop: win a fight, gain Spirit Jade, preview the next enemy, buy or refresh items, then continue to the next prep phase.

- Added `ShopPoolConfig` and `ShopOptionRuntime`.
- Added `Assets/_Game/Scripts/TalismanBag/Inventory/PlayerTalismanInventory.cs` for run inventory bookkeeping.
- Added lightweight mobile shop compatibility scripts: `MobileShopPanel` and `MobileShopOptionView`.
- Created `ShopPool_BasicTalismans.asset` with 10 basic items, prices, and weights:
  - Fire Talisman 6, weight 12.
  - Spirit Stone 7, weight 10.
  - Shield Talisman 6, weight 10.
  - Qi Pill 6, weight 10.
  - Thunder Talisman 8, weight 8.
  - Seal 5, weight 8.
  - Sword Pill 7, weight 8.
  - Peach Wood 5, weight 8.
  - Exorcism Bell 5, weight 8.
  - Water Talisman 6, weight 8.
- Reworked `ShopControllerV2` to generate 4 weighted options, prevent duplicates when the pool is large enough, sell out purchased options, spend Spirit Jade, and refresh for 2 Spirit Jade.
- Shop status now shows current Spirit Jade plus the next enemy's title, weakness, danger, and recommended build.
- Purchased items immediately instantiate into the bottom inventory bar and can be dragged during the next prep phase.
- `RunFlowControllerV2` now resets Spirit Jade and starter inventory on run restart, sends non-final wins to Shop, and sends Round 7 victory directly to RunWin.
- Added debug tools: `Open Shop`, `Reroll Shop`, and `Give Random Shop Item`.

Verification:

- Ran `TalismanBag.EditorTools.TalismanBagSceneBuilder.BuildAll` to regenerate the scene and `ShopPool_BasicTalismans.asset`.
- Ran Unity batchmode compile verification with no C# compilation errors found in `Logs/phase4_step3_compile_verify.log`.

## Phase 4 Step 4 Inventory, Lv2 Merge, and Power Slots

This pass adds mid-run build growth to the 15-minute mobile run: duplicate Lv1 items can become Lv2 core pieces, and Round 4 unlocks two power slots that change late-run placement decisions.

### Inventory Runtime Cleanup

- `TalismanItemRuntime` now owns a unique `runtimeId` generated from `Guid.NewGuid().ToString("N")`.
- Runtime items keep `definition`, `gridPosition`, cooldown state, trigger count, `isPlaced`, `isSealed`, and `level`.
- `PlayerTalismanInventory` now supports reset, add with level, remove, all/placed/unplaced queries, item-id lookup, and mergeable-item detection.
- Shop purchases create new runtime items through `ShopControllerV2.AddItemToInventory`.
- The bottom inventory item view and the grid item view are the same runtime-backed object: unplaced items live in the bottom rail, placed items move to the grid, and `isPlaced` is updated by `TalismanBagGrid`.

### Lv2 Merge Rules

- Added `ItemMergeSystem` and `MobileMergeButton` compatibility components.
- `AutoCombatController.AutoMergeDuplicateLevelOneItems()` performs the UI-safe merge path used by the mobile button and debug panel.
- Merge rule is limited to `same itemId + Lv1 + Lv1 -> one Lv2`.
- If either item is on the grid, both are returned from the grid first so their cells are cleared.
- The two old Lv1 runtimes are removed from inventory, both old views are destroyed, then a fresh Lv2 runtime is added back to the bottom inventory rail.
- Merge logs show messages like `Fire Talisman Lv1 x2 -> Fire Talisman Lv2` in the run log/session log.
- Lv3, rarity, random affixes, saves, and long-term progression are intentionally not added.

### Lv2 Combat Values

- Spirit Stone: Lv1 generates 12 mana; Lv2 generates 18 mana.
- Fire Talisman: Lv1 deals 12 for 10 mana; Lv2 deals 20 for 12 mana.
- Thunder Talisman: Lv1 deals 18 for 16 mana; Lv2 deals 28 for 18 mana.
- Shield Talisman: Lv1 grants 18 shield for 8 mana; Lv2 grants 28 shield for 10 mana.
- Qi Pill: Lv1 heals 20, or 28 with Shield Talisman; Lv2 heals 32, or 40 with Shield Talisman; mana cost is 12/14.
- Sword Pill: Lv1 deals 8 plus 5 with Fire Talisman; Lv2 deals 14 plus 8 with Fire Talisman.
- Exorcism Bell: Lv1 deals 12; Lv2 deals 20.
- Water Talisman: Lv1 heals 10; Lv2 heals 18.
- Seal: adjacent Thunder crit chance is 30% at Lv1 and 45% at Lv2; adjacent Fire bonus is +3 at Lv1 and +6 at Lv2.
- Peach Wood: Exorcism Array ghost-family damage bonus is 30% at Lv1 and 45% if any placed Peach Wood is Lv2.
- Combat logs use runtime item names, so Lv2 triggers show `Lv2` in mana, damage, shield, heal, and trigger messages.

### Power Slots

- Added `BagPowerSlotController` in `Assets/_Game/Scripts/TalismanBag/Grid/`.
- `BagExpansionController` can now unlock and lock the two enhanced positions at `(1,1)` and `(3,3)`.
- `RunFlowControllerV2.StartNewRun()` locks power slots on restart.
- `RunFlowControllerV2.OnBattleWin()` unlocks the two power slots after Round 4 victory and shows the awakening status message.
- `TalismanGridSlotView` now has an `enhancedBadge`; generated slots show a visible `阵眼` badge and enhanced border only after unlock.
- Active items placed on unlocked power slots multiply effective cooldown by `0.85`.
- Passive items such as Seal and Peach Wood can sit on those slots without errors, but do not receive the cooldown bonus.
- At battle start, active items benefiting from power slots emit one concise log line instead of spamming every trigger.

### Debug Testing

- Debug panel additions:
  - `Duplicate Fire x2`.
  - `Duplicate Thunder x2`.
  - `Auto Merge All`.
  - `Unlock Power Slots`.
  - `Lock Power Slots`.
  - Existing `Give Lv2 Core Build`.
- Suggested quick test:
  - Tap version text 5 times to open debug.
  - Use `Duplicate Fire x2`, then `Auto Merge All`.
  - Confirm the two Lv1 views disappear and one Lv2 view returns to the bottom rail.
  - Drag the Lv2 item onto the grid and start combat to confirm Lv2 logs/values.
  - Use `Unlock Power Slots`, place an active item on a `阵眼` slot, then start combat and confirm the power-slot cooldown log.

### Current Gaps

- Boss remains the existing placeholder pressure fight; this step intentionally does not implement the full heart-demon mechanic set.
- Hover/touch detailed tooltips for Lv2 stat explanations are not a full production tooltip system yet.
- Inventory state is clear through item location and runtime state, but there is no duplicate shadow entry in the bottom rail for already placed items.

### Suggested Next Phase

- Phase 4 Step 5: implement the full Boss heart-demon evil-cultivator mechanic set.
- Add a focused mobile playtest pass for the merge button, bottom rail readability, and power-slot discoverability.

Verification:

- Ran `TalismanBag.EditorTools.TalismanBagSceneBuilder.BuildAll` to regenerate `Scene_TalismanBag_Run15Min`.
- Confirmed generated scene contains `EnhancedBadge`, `DebugDuplicateFire`, `DebugAutoMergeAll`, and `DebugUnlockPowerSlots`.
- Ran Unity batchmode compile verification with no C# compilation errors found in `Logs/phase4_step4_compile_verify_after_merge.log`.

## Phase 4 Step 5 Heart Demon Boss Pass

This pass replaces the Round 7 placeholder pressure enemy with the full `heart_demon_boss` fight. The Boss now checks mana stability, output uptime, defense/healing, Thunder interruption, and whether the layout overdepends on one sealed core item.

### Boss Config

- Round 7 now uses `heart_demon_boss` instead of `heart_demon_boss_placeholder`.
- `heart_demon_boss.asset` values:
  - Enemy ID: `heart_demon_boss`.
  - Display name: `心魔邪修`.
  - Type: `Boss`.
  - HP: 260.
  - Basic attack: 14 damage every 3 seconds.
  - Weakness: `可打断 / 怕稳定阵法 / 怕雷印爆发`.
  - Danger: `吸灵 / 封印 / 心魔冲击 / 半血狂暴`.
  - Recommended build: `雷印 / 护丹 / 火剑流 / 多聚灵石`.
  - Target duration: 90-150 seconds.

### Boss Rules

- Basic attack deals 14 damage every 3 seconds, then every 2.5 seconds after enrage.
- Mana drain triggers every 6 seconds and removes up to 12 player mana.
- Seal triggers every 9 seconds before enrage, then every 6 seconds after enrage.
- Seal targets one placed, unsealed runtime item and lasts 4 seconds.
- Sealed items cannot trigger, and combat adjacency checks now ignore sealed items so sealed Seal/Peach/Spirit Stone support effects temporarily stop working.
- Heart Demon Impact starts every 14 seconds, charges for 3 seconds, then clears current shield and deals 36 damage.
- Thunder Talisman Lv1 or Lv2 interrupts Heart Demon Impact while the Boss is charging.
- Boss enrage triggers once at 50% HP or lower; it speeds up basic attacks and seal frequency but does not speed up mana drain or charge.

### Boss UI And Feedback

- Enemy charge bar is reused for Boss charge and now shows progress text such as `心魔冲击蓄力中 42%`.
- Boss charge start flashes the Boss visual once.
- Interrupt still clears the charge bar and shows `打断！`.
- Boss enrage emits an `EnemyEnraged` event, flashes the Boss visual, and spawns `狂暴！` floating text.
- Existing combat enemy text shows `狂暴` and `蓄力` tags.
- Mobile HUD, enemy preview, and round info fallback Boss tags now mention interruption, stable formation, mana drain, seal, Heart Demon Impact, and half-health enrage.
- Battle log still shows only 3 lines, with Boss danger/counter/seal/result events protected above ordinary damage/mana lines.

### Boss Stats And Failure Reason Prep

- `BattleStatsTracker` now records:
  - `bossChargeStartedCount`.
  - `bossChargeHitCount`.
  - `bossChargeInterruptedCount`.
  - `bossSealCount`.
  - `bossManaDrainCount`.
  - `bossEnrageTriggeredCount`.
- Result details now include these Boss counters.
- Failure reason text now has Boss-specific preconditions for repeated Heart Demon Impact hits, mana drain pressure, and seal-driven loop collapse.
- Full run settlement refinement is intentionally left for Phase 4 Step 6.

### Debug Testing

- Added debug controls:
  - `Start Boss Fight`.
  - `Force Boss Charge`.
  - `Force Boss Enrage`.
  - `Force Boss Seal`.
  - `Force Boss Mana Drain`.
  - `Give Anti Boss Build`.
- Anti Boss Build grants:
  - Spirit Stone Lv2.
  - Spirit Stone Lv1.
  - Thunder Talisman Lv2.
  - Seal Lv1.
  - Shield Talisman Lv2.
  - Qi Pill Lv2.
  - Fire Talisman Lv2.
  - Sword Pill Lv1.
- Quick test path:
  - Open hidden debug via version text.
  - Use `Start Boss Fight`.
  - Use `Give Anti Boss Build`.
  - Start combat.
  - Use `Force Boss Charge` and confirm Thunder can interrupt.
  - Use `Force Boss Seal`, `Force Boss Mana Drain`, and `Force Boss Enrage` to verify each pressure mechanic.

### Current Gaps

- No formal Boss art, audio, or narrative sequence is added.
- Full final settlement grouping, build-grade copy, and pace tuning are deferred.
- The old placeholder asset still exists on disk for compatibility, but the run no longer references it.

### Suggested Next Phase

- Phase 4 Step 6: full run settlement, archetype recognition polish, failure-reason copy, and 7-round pacing tuning.

Verification:

- Ran `TalismanBag.EditorTools.TalismanBagSceneBuilder.BuildAll` to regenerate `Scene_TalismanBag_Run15Min`, `heart_demon_boss.asset`, and `RunConfig_15Min_Test.asset`.
- Confirmed `RunConfig_15Min_Test.asset` Round 7 references the `heart_demon_boss.asset` GUID and `isBossRound = true`.
- Confirmed generated scene contains `DebugStartBossFight`, `DebugAntiBossBuild`, `DebugBossCharge`, `DebugBossEnrage`, `DebugBossSeal`, and `DebugBossManaDrain`.
- Ran Unity batchmode compile verification with no C# compilation errors found in `Logs/phase4_step5_boss_compile_verify.log`.

## V0.2 Step 0 + Step 1 Formation Power Counter

This pass creates an isolated V0.2 verification scene for formation-eye and Spirit Stone power routing. It intentionally does not continue shop economy, Lv2 merging, Boss expansion, APK output, or new enemy sets.

### Scene Isolation

- Added `Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity`.
- Kept the existing scenes:
  - `Assets/_Game/Scenes/Scene_TalismanBag_CombatPOC.unity`.
  - `Assets/_Game/Scenes/Scene_TalismanBag_Run15Min.unity`.
- Build Settings still point at `Scene_TalismanBag_Run15Min.unity`; V0.2 is opened manually for this verification step.
- Added/confirmed V0.2 folders:
  - `Assets/_Game/Scripts/TalismanBag/V02/`
  - `Assets/_Game/Scripts/TalismanBag/V02/Grid/`
  - `Assets/_Game/Scripts/TalismanBag/V02/Formation/`
  - `Assets/_Game/Scripts/TalismanBag/V02/UI/`
  - `Assets/_Game/ScriptableObjects/TalismanBag/V02/`
  - `Assets/_Game/Prefabs/TalismanBag/V02/`

### Formation Rules

- V0.2 grid is 5 columns x 4 rows, 20 slots total.
- Formation eye position is `(2, 1)`.
- Formation eye cannot accept normal item placement.
- Orthogonal cells around the eye are `Powered`.
- Diagonal cells around the eye are `WeakPowered`.
- Weak-powered items can trigger, but cooldown is multiplied by `1.35`.
- Spirit Stone is always `Powered`.
- Spirit Stone powers its 3x3 area as `Powered`.
- Priority is Spirit Stone powered area, then eye powered cells, then eye weak-powered cells, then unpowered.

### Runtime And Combat

- Added `FormationPowerState`.
- Added `FormationPowerResolver`.
- Added `powerState` to `TalismanItemRuntime`.
- `AutoCombatController` now optionally reads `FormationPowerResolver`.
- Old scenes keep old behavior because they do not bind a V0.2 resolver.
- In V0.2:
  - `Unpowered` items do not tick cooldown and cannot trigger.
  - `WeakPowered` items trigger with slower cooldown.
  - `Powered` items trigger normally.
  - Adjacency support checks ignore unpowered adjacent items when a resolver is active.

### UI And Debug

- `TalismanGridSlotView` supports formation-eye, powered, weak-powered, unpowered, occupied, hover, invalid, sealed, combo, and enhanced visual states.
- Unpowered placed items are dimmed.
- Powered and weak-powered cells show clear border/badge feedback.
- Added `FormationPowerUI` for:
  - powered count,
  - weak-powered count,
  - unpowered count,
  - unpowered warning.
- Added V0.2 debug buttons:
  - `Auto Place Powered`
  - `Auto Place Unpowered`
  - `Refresh Power`
  - `Start Test Battle`
  - `Reset Formation`
- Test enemy is `v02_mountain_imp.asset`, display name `山间小妖`, HP 80, attack 8, interval 2.5 seconds.

### Verification

- First Unity run generated V0.2 `.meta` files but failed because Unity compiled shared scripts before importing the new V0.2 scripts.
- Re-ran `TalismanBag.EditorTools.TalismanBagSceneBuilder.BuildV02FormationCounter`; scene generation succeeded in `Logs/v02_step0_step1_scene_build_retry.log`.
- Confirmed the V0.2 scene contains 20 slot objects.
- Confirmed serialized grid dimensions are `width: 5`, `height: 4`.
- Confirmed serialized formation eye is `{x: 2, y: 1}` and weak cooldown multiplier is `1.35`.
- Ran Unity batchmode compile verification with no C# compilation errors found in `Logs/v02_step0_step1_compile_verify.log`.

### Suggested Next Step

- V0.2 Step 2: add a talisman tag system so formation counters can reason about fire, thunder, shield, healing, exorcism, and tool tags instead of only item IDs.

## V0.2 Step 2 Talisman Tag System

This pass adds the isolated V0.2 tag layer requested for formation-counter verification. It does not add new enemies, shops, bosses, Lv2 progression, APK output, or a broad combat refactor.

### Tags And Definitions

- Added `Assets/_Game/Scripts/TalismanBag/V02/Tags/TalismanTags.cs`.
- Added `Assets/_Game/Scripts/TalismanBag/V02/Tags/TalismanTagUtility.cs`.
- Added V0.2 enums:
  - `ElementTag`.
  - `FunctionTag`.
  - `CounterTag`.
  - `EffectType`.
- Extended `TalismanItemDefinition` with V0.2 fields:
  - `elementTag`.
  - `functionTags`.
  - `counterTags`.
  - `effectType`.
  - `energyRequired`.
  - `requiresFormationPower`.
  - `shortRoleDescription`.
  - `counterDescription`.
- `TalismanTagUtility` supports:
  - element checks,
  - function tag checks,
  - counter tag checks,
  - direct counter matching,
  - compact debug summaries.

### V0.2 Item Assets

- Added dedicated V0.2 item assets under `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/`.
- Configured 10 V0.2 talismans:
  - Fire Talisman.
  - Thunder Talisman.
  - Sword Pill.
  - Chain Thunder Talisman.
  - Shield Talisman.
  - Purify Talisman.
  - Soul Suppress Talisman.
  - Spirit Stone.
  - Qi Pill.
  - Seal.
- Spirit Stone uses `requiresFormationPower = false` so it can remain a power source even when outside an already powered cell.
- Other V0.2 items default to `requiresFormationPower = true` and continue to obey the Step 1 formation-power gate.

### UI And Debug

- Added `V02TalismanTooltipUI`.
- `DraggableTalismanItemView` now emits a click event so V0.2 tooltip UI can show selected item tags.
- The V0.2 tooltip panel shows:
  - element tag,
  - function tags,
  - counter tags,
  - effect type,
  - power requirement,
  - role and counter descriptions.
- Added V0.2 debug buttons:
  - `Print Tags`.
  - `Give All V02`.
  - `Test Tag Query`.
  - `Test Counter`.
- The generated V0.2 scene now exposes all 10 V0.2 items in the bottom inventory for quick validation.

### Combat Integration

- `AutoCombatController` now reads `effectType` through a lightweight tag dispatch entry.
- Existing `itemId` combat logic is intentionally preserved for old scenes and current combat behavior.
- Added placeholder trigger coverage for Chain Thunder, Purify, and Soul Suppress so the new V0.2 assets can participate in debug combat without a full combat rewrite.
- Formation gating now respects `requiresFormationPower`:
  - unpowered normal items do not tick or trigger,
  - Spirit Stone can remain active as a power source,
  - old scenes keep old behavior when no V0.2 resolver is bound.

### Verification

- Re-ran `TalismanBag.EditorTools.TalismanBagSceneBuilder.BuildV02FormationCounter`.
- The first Step 2 generation run hit the same transient Unity import-order issue as Step 1, then the retry succeeded in `Logs/v02_step2_scene_build_retry.log`.
- Ran Unity batchmode compile verification with no C# compilation errors found in `Logs/v02_step2_compile_verify.log`.
- Confirmed `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/` contains 10 V0.2 `.asset` files.
- Confirmed the generated V0.2 scene contains the tooltip panel and 10 item views.
- Build Settings still point at `Scene_TalismanBag_Run15Min.unity`; V0.2 remains a manually opened verification scene.

### Suggested Next Step

- V0.2 Step 3: add enemy profession, archetype, intent, and weakness tags so item counters can be tested against enemy-side tags instead of only debug strings.

### Follow-up Localization Fix

- Kept V0.2 enum, file, and asset identifiers in English for Unity stability.
- Localized player-facing tag display to Chinese:
  - element tags,
  - function tags,
  - counter tags,
  - effect type names,
  - tooltip labels.
- Localized V0.2 item role and counter descriptions in the dedicated V0.2 item assets.
- Localized visible V0.2 debug scene button labels for tag validation.
- Unity batchmode regeneration was blocked because another Unity instance already had the project open, so the serialized V0.2 scene and item assets were patched directly and verified by text search.

## V0.2 Step 3 Enemy Archetype And Skill Intent System

This pass adds the V0.2 enemy-side question system: enemy identity, archetype, weakness tags, skill definitions, skill runtime state, intent UI, and debug switching. It does not add the full 7-round run flow, shop, Lv2 merging, full Boss phase logic, post-battle reward choices, or the Step 4 counter-feedback layer.

### Enemy Definition Extensions

- Extended `EnemyDefinition` with V0.2 fields:
  - `enemyClass`.
  - `enemyArchetype`.
  - `intentText`.
  - `recommendedCounterText`.
  - `weaknessTags`.
  - `resistTags`.
  - `skills`.
- Existing enemy fields remain in place for old scenes and the 15-minute run.
- Extended `EnemyRuntime` with:
  - `skillRuntimes`.
  - `currentShield`.
  - `poisonStacks`.
  - `burnStacks`.
  - `currentIntentText`.
  - `isCastingSkill`.
  - `currentCastingSkill`.
- Extended `CombatStats` with player-side `poisonStacks` and `burnStacks`.

### Enemy Skill System

- Added `EnemySkillDefinition`.
- Added `EnemySkillType`.
- Added `EnemySkillRuntime`.
- Added `EnemySkillController`.
- `EnemySkillController` owns:
  - skill cooldown ticking,
  - cast start,
  - intent display during cast time,
  - cast completion,
  - simplified skill effect dispatch.
- `AutoCombatController` now optionally binds the V0.2 skill controller. Old scenes without this binding keep their previous enemy behavior.

### Implemented Skill Effects

- `GainShield` and `BossPhaseShield` add real enemy shield.
- Enemy shield now absorbs player talisman damage before HP is reduced.
- `SummonMinions` and `BossPhaseSummon` are simplified as group-pressure damage to the player.
- `ApplyPoison` adds poison and burn stacks.
- Poison and burn stacks deal periodic simplified damage to the player.
- `StealEnergy` temporarily disables talismans around the first Spirit Stone, or around the formation eye if no Spirit Stone is placed.
- Temporarily disabled talismans do not trigger and are dimmed with the existing sealed visual treatment.
- `SealRowOrColumn` and `BossPhaseSealEye` seal a random row or column for a duration.

### V0.2 UI

- Added `V02EnemyIntentUI`.
- Added `V02EnemyPreviewPanel`.
- Added `V02PlayerStatusUI`.
- Enemy preview shows:
  - enemy name,
  - class,
  - archetype,
  - skills,
  - weakness tags,
  - recommended counter text,
  - default intent.
- Enemy intent UI shows the current casting intent and remaining cast time.
- Player status UI shows poison and burn stacks.
- `TalismanCombatUI` now shows enemy shield beside enemy HP.

### V0.2 Enemy Assets

- Added 7 V0.2 enemy assets under `Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/`:
  - `mountain_imp_basic`.
  - `turtle_guardian_shield`.
  - `imp_swarm`.
  - `red_poison_beast`.
  - `energy_thief_ghost`.
  - `seal_talisman_taoist`.
  - `formation_breaker_boss`.
- Added 8 V0.2 skill assets under `Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/Skills/`:
  - turtle shield gain,
  - imp swarm summon,
  - red poison,
  - energy steal,
  - seal row/column,
  - Boss shield,
  - Boss summon,
  - Boss seal.

### Scene Builder And Debug

- Updated `BuildV02FormationCounter` to generate V0.2 enemy and skill definitions.
- V0.2 scene generation now attaches:
  - `EnemySkillController`,
  - `V02EnemyIntentUI`,
  - `V02EnemyPreviewPanel`,
  - `V02PlayerStatusUI`.
- Extended `V02FormationDebugController` with:
  - spawn buttons for all 7 V0.2 enemies,
  - `Force Enemy Skill`,
  - `Clear Player Status`,
  - `Clear Seals`.
- Existing formation-power and tag debug buttons remain.

### Verification

- Confirmed the new V0.2 enemy scripts and UI scripts exist with `.meta` files.
- Confirmed 7 V0.2 enemy assets exist.
- Confirmed 8 V0.2 skill assets exist.
- Confirmed enemy assets contain class, archetype, weakness tags, and skill references.
- Checked the active Unity editor log tail and found no fresh C# compiler errors in the most recent output.
- Unity batchmode scene regeneration and compile verification were not run because the project is currently open in another Unity editor instance, with active AssetImportWorker processes. The scene builder is ready; after closing the open editor or running the menu item inside Unity, regenerate `Scene_TalismanBag_V02_FormationCounter`.

### Suggested Next Step

- V0.2 Step 4: connect item counter tags to enemy weakness tags and add explicit combat feedback for shield break, cleanse, exorcism, anti-steal, anti-seal, and group-clear counters.

## V0.2 Step 4 Counter Tags And Combat Feedback

This pass connects the V0.2 item tags to combat-side counter resolution and visible feedback. It keeps the existing 15-minute run, shop, reward, Boss expansion, Lv2 systems, and old scenes intact.

### Added Scripts

- `Assets/_Game/Scripts/TalismanBag/V02/Counters/CounterMatchResolver.cs`
  - Resolves shield break, cleanse, unseal, anti-steal-energy, group/summon clear, and charge guard counter checks.
- `Assets/_Game/Scripts/TalismanBag/V02/Feedback/CounterFeedbackController.cs`
  - Emits typed counter feedback through the existing battle event router.
  - Supports `破盾！`, `净化！`, `解封！`, `镇魂反制！`, `阵眼保护成功！`, `连锁清场！`, and `护身抵挡！`.
- `Assets/_Game/Scripts/TalismanBag/V02/Feedback/V02FailureTracker.cs`
  - Records shield-not-broken count, poison/burn damage, steal-energy hits/counters, seal hits/cleanses, unpowered trigger blocks, and no-damage duration placeholder data.

### Combat Integration

- `AutoCombatController` now binds optional V0.2 counter and feedback systems.
- Chain thunder, purify, and soul suppress talismans now have dedicated trigger behavior instead of placeholder triggers.
- Shield counter rules:
  - Shield-break talismans multiply shield damage by `2.0`.
  - Burst-style counters multiply shield damage by `1.3`.
  - Shield counter feedback emits before the regular damage log.
- Purify talisman clears poison/burn stacks and cleanses active seals.
- Soul suppress talisman deals bonus damage against ghost/steal-energy weaknesses.
- Steal-energy enemy skills now ask `V02TryCounterStealEnergy` before applying disruption.
  - A powered, unsealed soul-suppress counter cancels the steal.
  - Emits both `镇魂反制！` and `阵眼保护成功！`.
- Chain thunder receives a `1.6x` multiplier against group/summon enemies and emits `连锁清场！`.
- Player shield blocking charge pressure emits the lightweight `护身抵挡！` feedback.
- Unpowered V0.2 talismans now record one blocked-trigger event per item per battle instead of silently failing forever.

### UI And Debug

- `FloatingCombatText` now reads typed `CounterFeedbackType` values so counter float text is specific instead of generic.
- `BattleLogUI` already keeps Counter/Danger/Seal/Result logs at the highest priority, so new counter messages stay visible within the recent 3-line mobile log.
- `V02FormationDebugController` now has targeted debug actions:
  - Test Shield Break.
  - Test Cleanse Poison.
  - Test Cleanse Seal.
  - Test Soul Suppress.
  - Test Chain Clear.
  - Test Counter Log Priority.
- `TalismanBagSceneBuilder` now attaches:
  - `CounterMatchResolver`,
  - `CounterFeedbackController`,
  - `V02FailureTracker`.
- The V0.2 scene builder now creates and wires six extra debug buttons for the Step 4 counter tests.

### Verification Notes

- Confirmed all new scripts have `.meta` files with stable GUIDs.
- Confirmed the new Step 4 code paths are referenced from `AutoCombatController`, `EnemySkillController`, `V02FormationDebugController`, and `TalismanBagSceneBuilder`.
- Ran Unity batchmode with `TalismanBag.EditorTools.TalismanBagSceneBuilder.BuildV02FormationCounter`.
- The first import pass reported transient missing-namespace errors while Unity had not yet imported the new V0.2 folders; the follow-up Tundra pass compiled successfully and regenerated the V0.2 formation counter scene.
- Ran a second Unity batchmode pass after import stabilization; it completed with `Tundra build success`, no `error CS` entries, and regenerated the V0.2 formation counter scene again.

## V0.2 Step 5 Post-Battle Reward Draft System

This pass adds the V0.2 post-battle three-choice reward draft. Rewards are designed to change the next formation answer and counter options, not only add flat stats.

### Added Reward Scripts

- `Assets/_Game/Scripts/TalismanBag/V02/Rewards/V02RewardTypes.cs`
  - Defines `V02RewardType`, `V02FormationModifierType`, and `V02BuildModifierType`.
- `Assets/_Game/Scripts/TalismanBag/V02/Rewards/V02RewardDefinition.cs`
  - ScriptableObject reward definition with display text, type, optional new talisman, formation modifier, build modifier, helpful counter tags, related function tags, and base weight.
- `Assets/_Game/Scripts/TalismanBag/V02/Rewards/V02RewardPoolConfig.cs`
  - ScriptableObject reward pool containing the V0.2 reward list.
- `Assets/_Game/Scripts/TalismanBag/V02/Rewards/V02RewardController.cs`
  - Generates three reward options, adjusts weights by the next enemy's weakness/skill tags, applies rewards, and drives the reward panel.
- `Assets/_Game/Scripts/TalismanBag/V02/Rewards/V02RunModifierState.cs`
  - Stores current-run formation and build modifiers.
- `Assets/_Game/Scripts/TalismanBag/V02/Rewards/V02RewardInventoryAdapter.cs`
  - Adds new talisman rewards to the V0.2 bottom draggable inventory bar.
- `Assets/_Game/Scripts/TalismanBag/V02/UI/V02RewardPanel.cs`
  - Mobile portrait three-card reward selection panel.
- `Assets/_Game/Scripts/TalismanBag/V02/Run/V02RunFlowController.cs`
  - Lightweight V0.2 local flow: battle win -> reward selection -> reward chosen -> next enemy prep.

### Reward Pool

- Added `Assets/_Game/ScriptableObjects/TalismanBag/V02/Rewards/RewardPool_V02_Basic.asset`.
- Added 14 base reward assets:
  - `reward_add_thunder_talisman`
  - `reward_add_sword_pill`
  - `reward_add_chain_thunder`
  - `reward_add_purify_talisman`
  - `reward_add_soul_suppress`
  - `reward_add_spirit_stone`
  - `reward_upgrade_eye_core_nine_grid`
  - `reward_spirit_link_between_stones`
  - `reward_outer_ring_defense_boost`
  - `reward_fire_burn_plus_one`
  - `reward_thunder_shieldbreak_boost`
  - `reward_sword_crit_boost`
  - `reward_shield_amount_boost`
  - `reward_cleanse_cooldown_reduction`

### Reward Generation Rules

- Generates up to 3 choices.
- Slot 1 prefers a new talisman.
- Slot 2 prefers a formation modifier or build modifier.
- Slot 3 is weighted from the whole pool.
- Duplicate `rewardId` is avoided.
- Already-applied formation/build modifiers are not offered again.
- Rewards whose `helpfulAgainstTags` match the next enemy's `weaknessTags` or skill `skillTags` receive extra weight.

### Formation Modifier Integration

- `FormationPowerResolver` now reads `V02RunModifierState`.
- `UpgradeEyeCoreToNineGrid`
  - The formation eye powers its full 3x3 area.
- `SpiritLinkBetweenStones`
  - Same-row or same-column Spirit Stones power the cells between them.
- `OuterRingDefenseBoost`
  - Outer-ring Shield Talismans gain +30% shield.
  - Outer-ring Purify Talismans gain an additional 15% cooldown reduction.
- `FormationPowerUI` now shows active formation reward hints.

### Build Modifier Integration

- `FireBurnPlusOne`
  - Fire Talisman hits add enemy burn stacks.
  - Enemy burn stacks now tick once per second for damage.
- `ThunderShieldBreakBoost`
  - Thunder shield-break multiplier rises from 2.0x to 2.5x.
- `SwordCritBoost`
  - Sword Pill gains 25% chance to deal 1.8x damage.
- `ShieldAmountBoost`
  - Shield Talisman shield amount increases by 30%.
- `CleanseCooldownReduction`
  - Purify Talisman cooldown is reduced by 25%.

### V0.2 Scene And Debug

- `Scene_TalismanBag_V02_FormationCounter` now includes:
  - `V02RewardController`
  - `V02RewardPanel`
  - `V02RunModifierState`
  - `V02RewardInventoryAdapter`
  - `V02RunFlowController`
- Battle victory in the V0.2 scene opens the reward draft before moving to the next prep state.
- Added reward debug buttons:
  - Open Reward Panel
  - Set next enemy: shield / swarm / poison / thief / seal / Boss
  - Give Eye Core
  - Give Spirit Link
  - Give Outer Ring
  - Print Run Modifiers
  - Reset Run Modifiers

### Verification

- Ran Unity batchmode to regenerate `Scene_TalismanBag_V02_FormationCounter`.
- The first pass had transient missing-namespace errors while Unity imported new V0.2 folders; it then completed with `Tundra build success`.
- Ran a second Unity batchmode pass after import stabilization; it completed with `Tundra build success`, no `error CS` entries, and regenerated the V0.2 formation counter scene.
- Confirmed 15 reward assets exist in the V0.2 reward folder: 14 rewards plus `RewardPool_V02_Basic.asset`.

### Suggested Next Step

- V0.2 Step 6: connect the full 15-minute flow, Boss pass, and failure review around the new reward draft decisions.
