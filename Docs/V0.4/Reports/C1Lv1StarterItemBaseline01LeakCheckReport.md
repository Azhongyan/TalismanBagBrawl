# C1 Lv1 Starter Item Baseline 01 Leak Check Report

- Runtime source: `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1Lv1StarterItemBaseline.cs`
- Result: `PASS`
- Permitted existing dependencies: `ItemInstanceRarity`; `ItemInnerDataCatalog.FindById` for ordinary identity validation only.
- Workbench fallback: `ABSENT`
- Random, clock, and mutable global input: `ABSENT`
- Report-only design evidence is not present in runtime source.

## Forbidden Runtime Tokens

- `PASS` `Reward`: absent
- `PASS` `Drop`: absent
- `PASS` `Battle`: absent
- `PASS` `Enemy`: absent
- `PASS` `Stage`: absent
- `PASS` `Board`: absent
- `PASS` `Inventory`: absent
- `PASS` `Save`: absent
- `PASS` `UI`: absent
- `PASS` `Scene`: absent
- `PASS` `Build`: absent
- `PASS` `Roll`: absent
- `PASS` `RNG`: absent
- `PASS` `probability`: absent
- `PASS` `Core`: absent
- `PASS` `Affix`: absent
- `PASS` `UnityEngine`: absent
- `PASS` `UnityEditor`: absent
- `PASS` `MonoBehaviour`: absent
- `PASS` `ScriptableObject`: absent
- `PASS` `System.Random`: absent
- `PASS` `Guid.NewGuid`: absent
- `PASS` `DateTime.Now`: absent
- `PASS` `DateTime.UtcNow`: absent
- `PASS` `Environment.TickCount`: absent
- `PASS` `ItemBalanceWorkbench`: absent
- `PASS` `BALANCE_CANDIDATE`: absent
- `PASS` `designEvidence`: absent

## Protected Existing Sources

- `Assets/_Game/Configs/ItemBalanceWorkbench/**`: aggregate checked against frozen hash.
- `Assets/_Game/Scripts/TalismanBag/Items/Generation/ItemInstanceRarity.cs`: read-only dependency.
- `Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs`: read-only dependency.
