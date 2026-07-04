# Item Identity Leak Check Report

Package: `V0.4-ItemIdentityUnification01`

Status: `PASS / REPORT_ONLY`

## Checks

| Check | Result | Evidence |
| --- | --- | --- |
| Formal script leak for target `preview_*` ids outside BuildSandbox/editor sandbox | PASS | `rg "preview_(fire_talisman\|thunder_sword\|x2_wood_talisman\|guard_wood\|cleanse_corner\|stone_core\|energy_incense\|old_bell\|soul_seal\|taomu_sword)" Assets ProjectSettings Packages --glob "!Assets/_Game/Scripts/TalismanBag/BuildSandbox/**" --glob "!Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**"` returned no matches. |
| Existing preview usage remains devOnly/report sandbox | PASS | Matches are under `Assets/_Game/Scripts/TalismanBag/BuildSandbox/**`, `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**`, and `Docs/V0.4/Reports/**`. |
| Formal V0.2/V0.3 ids preserved | PASS | `fire_talisman_basic`, `thunder_talisman_basic`, `chain_thunder_talisman_basic`, `exorcism_bell_basic`, and `peach_wood_basic` remain existing formal ids; this package did not edit their scripts or assets. |
| V0.4 candidate ids not promoted | PASS | Candidate ids are documented only in this report set; this package did not add them to rewards, drops, upgrade configs, scenes, save data, or formal item tables. |
| SaveData / PlayerPrefs / MainTrialProgressData touched | PASS | No code or data files in save/progress paths were edited by this package. |
| Reward / Drop / Boss / RunFlow touched | PASS | No reward, drop, Boss, RunFlow, PageState, FormationState, DamageText, or V02FormationGridFrame files were edited by this package. |
| Scene UI / layout touched | PASS | No `.unity`, prefab, ProjectSettings, or UI layout files were edited by this package. |
| `Scene_TalismanBag_V04_BattleSandboxPreview` reordered | PASS | Scene was not edited. |

## Preview Id Locations

The target temporary ids currently appear in devOnly sandbox sources and report outputs:

- `BuildGridInteractionPreviewController.cs`: source tray preview item list.
- `BuildSandboxItemInfoPanel.cs`: player-facing sandbox info profile text.
- `BattleSandboxBuildCombatPreview.cs`: default sandbox combat preview layout.
- `DevChapterBalanceRun.cs`: devOnly balance samples.
- Editor BuildSandbox validators/report writers for sandbox-only reports.
- `Docs/V0.4/Reports/**`: generated sandbox reports.

No target `preview_*` id was found in formal gameplay scripts outside the BuildSandbox/editor sandbox scan.

## Pure Test Placeholder Policy

The following source ids are classified as temporary placeholders and must not be promoted directly:

- `preview_fire_talisman`
- `preview_thunder_sword`
- `preview_x2_wood_talisman`
- `preview_guard_wood`
- `preview_cleanse_corner`
- `preview_stone_core`
- `preview_energy_incense`
- `preview_old_bell`
- `preview_soul_seal`
- `preview_taomu_sword`

Related runtime fixture ids such as `runtime_fire_talisman`, `runtime_guard_wood`, `runtime_cleanse_corner`, `runtime_square_fixture`, and `v04_runtime_fixture_*` are also pure test placeholders.

## Conclusion

`V0.4-ItemIdentityUnification01` is contained to report-only BuildSandbox identity documentation. It establishes the mapping table and leak policy without replacing formal item ids or connecting any V0.4 candidate to formal combat, reward, save, UI, or scene flow.
