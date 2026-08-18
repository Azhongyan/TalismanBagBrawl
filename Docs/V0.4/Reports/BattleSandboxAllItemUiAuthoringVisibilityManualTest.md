# BattleSandbox All Item UI Authoring Visibility Manual Test

Status: `USER_AUTHORING_HANDTEST_WAITING`

1. Open `Scene_TalismanBag_V04_BattleSandboxPreview.unity` in Edit Mode.
2. Run `Talisman Bag/V0.4/BattleSandbox Authoring/Enable Full BattleSandbox Item UI Authoring Preview`.
3. Confirm the existing full panel, 15 player sections, both Build groups and four core rows are visible.
4. Tune existing UI geometry manually and save the Scene.
5. Enter Play and confirm the detail panel starts hidden.
6. Click I004 and I007: FaMen + QiLei; I009: FaMen only; I001: QiLei only; I006: no eligible contribution.
7. Exit Play and confirm the saved Edit Mode authoring layout remains.
8. Run Disable, confirm the panel hides without layout deletion, then re-enable and confirm no duplicate panel.

Do not mark unobserved rows as user PASS. See the matrix `userVisualConfirmation` column.
