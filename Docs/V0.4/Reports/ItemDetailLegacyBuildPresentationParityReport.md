# ItemDetailLegacyBuildPresentationParity01 Report

Status: OFFLINE_PASS / USER_HANDTEST_WAITING

- Package: `V0.4-ItemDetailLegacyBuildPresentationParity01`
- Close mode: `SIMPLE_DIRECT_CLOSE`
- Player headings: `法门构筑` / `器类构筑`
- QualifiedBuild typed/debug authority retained
- FirstMissingLayer: `Presentation / Build body compatibility + authored Build state lifecycle + outside-dismiss frame geometry/input consumption`
- Scroll input: existing direct wheel/touch behavior retained; no ScrollRect routing change
- User handtest: `WAITING`

## Scenarios

| Id | Result | Marker | Detail |
|---|---|---|---|
| S01 | PASS | `SHARED_PRESENTATION_SOURCE_PASS` | verified |
| S02 | PASS | `PROTECTED_DOMAIN_HASH_PASS` | verified |
| S03 | PASS | `REAL_AUTHORED_TEXT_PASS` | verified |
| S04 | PASS | `MACHINE_FACT_PLAYER_LEAK_PASS` | verified |
| S05 | SUPPLEMENTAL | `USER_HANDTEST_WAITING` | Nine-point EventSystem matrix deferred to the first user handtest; it does not block this direct close. |

## Real samples

- I001: Inventory -> BoardUnlit -> BoardLit -> Move -> Return PASS; qualification=QiLeiOnly; faMen=Hidden; qiLei=qilei:fu
- I004: Inventory -> BoardUnlit -> BoardLit -> Move -> Return PASS; qualification=Dual; faMen=famen:zhenlei; qiLei=qilei:ling
- I006: Inventory -> BoardUnlit -> BoardLit -> Move -> Return PASS; qualification=None; faMen=Hidden; qiLei=Hidden
- I009: Inventory -> BoardUnlit -> BoardLit -> Move -> Return PASS; qualification=FaMenOnly; faMen=famen:lihuo; qiLei=Hidden
- I031: ordinary Build sections hidden PASS
- Unknown/Invalid: fail-closed PASS
- I004 -> I001 click switch: no visible stale text PASS

Final gate: `SIMPLE_DIRECT_CLOSE / USER_HANDTEST_WAITING`.
