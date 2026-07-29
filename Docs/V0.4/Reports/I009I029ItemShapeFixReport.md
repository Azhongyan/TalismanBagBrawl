# I009 / I029 Item Shape Fix Report

- Package: `V0.4-I009AndI029ItemShapeFix01`
- Guard: `GUARD_PASS_I009_I029_ITEM_SHAPE_FIX01`
- Result: PASS
- Expected: `shape_single_1 / cells=(0,0) / coreCellLocal=(0,0)`
- Scope: catalog truth, four rotations, Candidate profile, five rarities, two Roll/Projection seeds per rarity, detail display, shared artwork-slot routing.
- UI layout mutation: NO
- Formal Battle / Reward / RunFlow / Inventory / Save / BuildSettings mutation: NO

## Verification Notes
- I009 shape invariant passed: shape_single_1, only (0,0) occupied, coreCellLocal (0,0).
- I009 placement invariant passed: rotations 0/90/180/270 each occupy only (0,0).
- I029 shape invariant passed: shape_single_1, only (0,0) occupied, coreCellLocal (0,0).
- I029 placement invariant passed: rotations 0/90/180/270 each occupy only (0,0).
- I009 derived contract passed: Candidate shapeDescription, five rarities, two seeds per rarity, Roll, Projection, and detail display all inherit shape_single_1.
- I029 derived contract passed: Candidate shapeDescription, five rarities, two seeds per rarity, Roll, Projection, and detail display all inherit shape_single_1.
- Item detail artwork routing passed: I009/I029 displayShapeName resolves through the shared single_1 rule to DaojuSingleCellImage; DaojuMultiCellImage remains the multi-cell route, with no item-id special case.

## Errors
- None

I009_I029_ITEM_SHAPE_FIX01_PASS
