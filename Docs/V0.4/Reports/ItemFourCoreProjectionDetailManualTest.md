# P3 BattleSandbox manual test

Status: `USER_HANDTEST_WAITING`

Open `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`, enter Play Mode, then:

1. Click Inventory I001: four Orange core rows are unlocked/inactive and QiLei Build remains visible.
2. Place I001 unlit: the same four rows remain unlocked/inactive.
3. Place or move I031 so I001 is lit: all four rows become active and Build progress refreshes.
4. Move I001 out of lighting: rows return to unlocked/inactive.
5. Return I001 to Inventory: placement/lit/active residue is absent.
6. Click I004: Dual Build tracks and four core rows coexist.
7. Click I006: Build is None/Known Zero and four core rows remain.
8. Click I009: FaMen-only Build and four core rows coexist.
9. Click I031: system detail has no ordinary Build/core rows.
10. Rapidly alternate I001/I004/I006/I009/I031 and confirm no stale row/text/icon state.

Do not start Prefab migration from this checklist.
