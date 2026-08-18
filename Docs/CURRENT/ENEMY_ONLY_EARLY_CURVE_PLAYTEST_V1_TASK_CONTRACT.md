# ENEMY ONLY EARLY CURVE PLAYTEST V1

Date: 2026-08-12  
Product context: CAMPAIGN_NORMAL_LV1  
Task class: CONTAINED_ONE_GUARD  
Primary owner: Enemy balance/data  
Development task: current visible task  
Status: INTERNAL_QA / WAITING_PLAYER_HANDTEST

## Outcome

Replace the obsolete early Enemy durability budget with one PLAYTEST_V1 curve that lets the current Canonical Build establish combat, growth, counterplay and a five-stage finish without changing Item, Reward, Nian or Battle semantics.

## Player evidence

- The normal route, reward claim, placement and next-stage transition work.
- The current legal early Build cannot defeat the 1-2 encounter at 1920 HP.
- The old 1-2 through 1-5 values were authored for the retired I001 82-damage / 2-second reference, not the current Canonical I007 and shared I031 Nian economy.

## Experience budget

| Stage | Intended role | Target TTK | Raw HP | Existing shell | Effective durability before BoneSwap |
|---|---|---:|---:|---:|---:|
| 1-1 | establish battle | 8-15 s | 48 | 0 | 48 |
| 1-2 | establish growth | 18-25 s | 180 | 0 | 180 |
| 1-3 | establish counterplay | 20-30 s | 40 | 200 | 240 |
| 1-4 | combination pressure | 25-35 s | 180 | 100 | 280 |
| 1-5 | chapter finish | 30-40 s | 220 | 100 | 320 |

The shell-heavy 1-3 budget intentionally makes Porcelain Hounds fragile after their shell is broken. Break is an accelerator, not a mandatory prerequisite: a legal non-Break reward path still has a finite clear route.

## Allowed writes

- Existing 1-2 Enemy balance rows and validation.
- Existing 1-3 through 1-5 PLAYTEST_V1 Enemy rows and validation.
- Existing exact Enemy verifiers.
- This Task Contract and the existing Component Ledger.

## Protected boundaries

- No Item database, initial acquisition policy, Reward, I031 Nian, Battle operator, Stage flow, Scene, Prefab, Presentation, VFX or Save changes.
- Keep rosters, attack cadence, Shell facts and BoneSwap semantics unchanged.
- Do not restore historical I001 or Pool15 authority and do not hardcode reward identity.
- Do not create a second simulator, Battle framework, Enemy profile or data source.

## Verification

1. Compile the existing Runtime and Editor projects.
2. Run the existing early Enemy and stage 3-5 exact validation entrypoints when a clean task-owned Unity lease is available.
3. Confirm the formal catalogs expose effective durability 48 / 180 / 240 / 280 / 320 with unchanged rosters and mechanics.
4. After internal QA, stop for one normal player route check from WorldMap through 1-5.

## Internal result — 2026-08-12

- Runtime offline full compile: PASS / 0 errors.
- Editor offline full compile: PASS / 0 errors.
- Existing 1-1/1-2 Enemy focused verifier: PASS.
- Existing 1-3 through 1-5 Enemy/Shield/BoneSwap focused verifier: PASS / 195 assertions.
- Effective durability exposed by the authoritative rows is 48 / 180 / 240 / 280 / 320.
- The early Enemy verifier no longer imports the retired I001/I002 starter projection, fixed-Build scenario or historical TTK sensitivity fixture. It now verifies only the Enemy catalog, exact rows, negative cases, deterministic catalog behavior and defensive copies.
- A Unity process already owns the project, so task-owned Unity batch and Fresh Play were not started. Player-path evidence remains NOT RUN.

## Stop rule

- Stop at the first real compile, catalog or Battle semantic conflict; do not widen into Item, Reward, Nian, Battle or Presentation.
- Internal validation may produce `INTERNAL_QA`, not `USER_HANDTEST_READY`.
