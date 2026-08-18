# ItemSystem BattleSandbox Board Leak Check

- Status: `PASS`
- Target scene SHA-256 remains `4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6`.
- Runtime installer is exact-path, Editor Play only, and DontSave.
- Runtime slots 41-65 are removed and the pre-authority Content height is restored on uninstall.
- The one authored PopupLayer/ItemDetailPanel is hidden first, reused, and hidden without destruction on uninstall.
- ItemDetailPanel prefab load/instantiate, runtime detail-root creation, ItemSandbox presenter/session calls and authored-panel destruction are all zero.
- DaoPin model coverage is 30/30; authored panel binding: `PASS / 30 of 30`.
- Legacy Text S20G-S20N: `PASS`; only null fonts are recovered and existing visual parameters remain untouched.
- Revision09-Rev03 S20O-S20V: `PASS`; SimSun-first cached font and actual authored Image visibility/identity are verified.
- Final-visible authored icons: `FaMen 90 / QiLei 60 / Core 30 / Placement 30 / Flavor 30`.
- ItemDetailSectionView clears stale Body/Rows and restores hidden/plain/authored/plain/hidden deterministically.
- Authority lighting renders only ItemSystemSnapshot.v2 LitRangeCells and placement facts.
- No Scene/Prefab/authored-slot/viewport/GridLayoutGroup/Item truth-source write is used.
- Authority-active commit returns before legacy board cache commit.
- Package gate status: `DEV_COMPLETE / QA_STATIC_PASS / WAITING_USER_HANDTEST`.
