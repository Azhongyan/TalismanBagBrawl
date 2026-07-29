# Normal Editor Handtest

1. Open `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` and press Play.
2. Before placement, confirm the existing player/enemy panels already show `玩家 HP 9999/9999`, `守骨奴 HP 980/980`, and `护壳 200/200`.
3. Open battle preparation and place: I031@(0,0), I009@(1,0), I012@(2,0), I010@(1,1) rotated 90°, I008@(0,2), I011@(1,3), I007@(0,4).
4. Press the bottom-navigation middle `继续战斗` button once. If the chain is illegal, the existing top state text lists the exact missing/unlit/wrong-damage item.
5. Observe about 75 seconds: live I007→I012 damage, seven shell breaks, six skills, twelve Basics, authored HP/guard/floating text/player-hit feedback and Shougunu visual cues.
6. Press `R` to reset; player must return to 9999/9999 and no old text/cue may remain.
7. Let the restarted loop finish and confirm final player 9735/9999 and Shougunu Defeated.
