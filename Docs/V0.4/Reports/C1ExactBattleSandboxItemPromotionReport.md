# C1 Exact BattleSandbox Item Promotion Report

Status: `TECHNICAL_PREREQUISITE_COMPLETE / PACKAGE_COMPLETE`

- PlayerVisibleDelivery: `PARTIAL`
- MilestoneCompletionEvidence: `NO`
- Product context: `CAMPAIGN_NORMAL_LV1`
- Source scene SHA-256: `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA`
- Parity: `98/98 PASS`
- Missing scripts: `0`
- BuildSandbox runtime dependencies: `0`

## Created carriers

- `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab`
- `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab`
- `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab`

The Board and Tray base subtrees retain source transforms, images, fonts, cell/slot spacing, category hierarchy, RectMask2D, and ScrollRect semantics. `ItemCard_04` supplies the reusable 285x285 root and 275x275 authored artwork slot. The three formal Tray cards are authored nested instances; the 23 Sandbox fixture cards and their runtime controller are not copied as state owners.

## Runtime-generated presentation audit

- `BoardItemArtworkLayer`: `PROMOTED_AUTHORED_REF`
- placed / preview / invalid artwork: `FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF`
- formation / core-power / lighting overlays: `FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF`
- drag ghost artwork / invalid artwork / underlay carrier: `PROMOTED_AUTHORED_REF`
- `MobileRotateZoneLayer`, right zone, guide: `PROMOTED_AUTHORED_REF`
- selected-info close behavior: `OUTSIDE_THIS_CARRIER_DOWNSTREAM`

## Exact source card role map

- ItemCard_01: 285-class baseline role; simple authored hierarchy.
- ItemCard_02: 285-class role with authored footprint subtree.
- ItemCard_03: intentional 140x430 vertical role.
- ItemCard_04: selected reusable 285-class carrier; explicit 275x275 artwork slot.
- ItemCard_05: intentional 285-class variant; explicit 271x271 artwork slot.

## Protected baseline

All 19 frozen SHA-256 baselines revalidated unchanged. Current approximate `C1FormalItemBoard`, `C1FormalItemTray`, and `C1FormalItemCard` remain unchanged and are not the final visible carriers.

## Evidence boundary

Compile/static and presentation-parity evidence are proven. Unified mounting, live formal Battle runtime, full BattleSandbox page composition, and WorldMap -> 1-1 -> I002 -> rebuild -> 1-2 -> WorldMap real-path evidence remain downstream. This package does not claim `USER_HANDTEST_READY`, `MILESTONE_INTEGRATED`, or milestone `COMPLETE`.
