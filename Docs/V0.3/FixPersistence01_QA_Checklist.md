# V0.3-NavigationFlow01-FixPersistence01 QA Checklist

## Scope

- From `Scene_TalismanBag_V03_MainHome`, enter Trial through the real homepage button.
- Verify V02 run progress, runtime talisman inventory, and board placement restore together after Stop/Play.
- Do not clear PlayerPrefs or manufacture save state for pass conditions.

## Required manual path

1. Start Play in `Scene_TalismanBag_V03_MainHome`.
2. Click Trial and enter `Scene_TalismanBag_V02_FormationCounter`.
3. Progress naturally to around `1-5`.
4. During the run, record:
   - current round,
   - runtime talisman list and levels,
   - board placement coordinates / visible layout,
   - at least one acquired talisman or reward modifier.
5. Stop Play.
6. Start Play again from `Scene_TalismanBag_V03_MainHome`.
7. Click Trial again.
8. Confirm all three restore together:
   - current round is unchanged,
   - runtime talisman inventory is unchanged,
   - board placement is unchanged.

## Additional coverage

- Verify a talisman/reward gained during the run persists after Stop/Play.
- Verify a consumed or removed runtime item does not reappear after Stop/Play.
- Verify V02 Golden Path still works:
   - board visible,
   - damage numbers visible,
   - enemy HP drops,
   - talismans trigger,
   - formation power works,
   - reward flow continues,
   - `2-9` to `2-10` boss gate behavior remains unchanged.
- Verify old saves without `runtimeLoadoutSnapshot` load safely and create a snapshot going forward without requiring a manual clear.

## Compile / log gate

- Unity C# compile errors: `0`.
- Search Editor log for:
   - `error CS`
   - `Compilation failed`
   - `All compiler errors`

