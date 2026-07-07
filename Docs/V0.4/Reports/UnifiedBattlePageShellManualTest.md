# UnifiedBattlePageShell01 Manual Test

- Generated: 2026-07-07
- Validation source: CODEX_STATIC_NO_UNITY_CLI
- Package: V0.4-UnifiedBattlePageShell01

## Unity Editor QA Menus
- Build scene/prefab: `Tools/Talisman Bag/V0.4/UnifiedBattle/UnifiedBattlePageShell01/[Writes Scene][Manual Only] Build Shell Scene And Prefab`
- Run validation reports: `Tools/Talisman Bag/V0.4/UnifiedBattle/UnifiedBattlePageShell01/[QA Only] Run Validation Reports`

## Manual Checks
1. Run the build menu and confirm it writes only:
   - `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity`
   - `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab`
2. Open the devOnly shell scene.
3. Confirm `BattlePageRoot` exists.
4. Confirm these required children exist under `BattlePageRoot`: `BoardArea`, `ItemTrayArea`, `EnemyInfoArea`, `BossCastBarSlot`, `BattleFeedbackLayer`, `StoryGuidePopupLayer`, `ResultRewardPlaceholder`, `V03FlowAdapterSlot`, `V04SandboxAdapterSlot`, `DevOnlyDiagnosticsSlot`.
5. Confirm `UnifiedBattlePageShellMarker` has devOnly=true, isEnabled=false, formalFlow=false, connectedToFormalRoute=false.
6. Confirm sample BattleContract data is displayed without starting formal battle.
7. Confirm `ResultRewardPlaceholder` displays placeholder copy only and does not grant reward or write save.
8. Confirm `DevOnlyDiagnosticsSlot` is visually separated from player-visible areas.
9. Run the validation report menu and confirm leak count remains 0.
10. Confirm the shell scene is not added to BuildSettings.
11. Confirm V02/V03 formal scenes, `Scene_TalismanBag_V04_BattleSandboxPreview`, RunFlow, SaveData, RewardService, BossInfoPanel, and chapter progression remain unchanged.

## Expected Static Result
- Required slot constants: 11/11
- Runtime forbidden reference leaks: 0
- Player-visible answer token leaks: 0
- Reward/save leaks: 0
- Formal route connections: 0
