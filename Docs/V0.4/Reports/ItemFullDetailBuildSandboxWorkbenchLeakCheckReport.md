# ItemFullDetailBuildSandboxWorkbench01 Leak Check Report

- Result: PASS
- Scene: isolated `Scene_TalismanBag_V04_ItemSandbox.unity`; not in BuildSettings.
- Formal Battle / BattleContract / Adapter / Bridge: not connected.
- V0.3 RunFlow / Reward / Inventory / SaveData / Boss / formal drop / formal cultivation: not connected.
- `ItemSystemSnapshot.v1` schema/canonical fields remain compatible; validator now enforces unique ordinary baseItemId/itemId per layout.
- Candidate Unity assets, Generation schemas, Prefabs, and BuildSettings: protected hashes unchanged when PASS.
- Runtime layout fields: no anchoredPosition, sizeDelta, offsets, preferred sizes, spacing, padding, or font size writes.
- Randomness: no UnityEngine.Random or string.GetHashCode seed path.
- I031: fixed core placement only; ordinary roll pool count remains zero.
- Data maturity remains BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED.
- Commit / tag / push: not performed.
