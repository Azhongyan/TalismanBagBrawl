# Real Item Damage × Shougunu Phase1 BattleSandbox Vertical Slice

- package = `V0.4-RealItemDamageShougunuPhase1BattleSandboxVerticalSlice01`
- classification = `COMPLEX_GUARDED_ONCE`
- status = `AUTOMATED_QA_PASS / USER_HANDTEST_WAITING`
- P3 assignment SHA-256 = `b1189b2887c4dd13774e165659db8b83203d13c389ac51e71a10bece34a14981`
- dependent package = `V0.4-ShougunuPhase1VisualPrototypeFacade01`
- dependent assignment SHA-256 = `a016e91e6398781578b56a2da80abe11f56590f9fa74a6be2672728d89021270`
- dependent status = `DEPENDENT_VISUAL_MICRO_PACKAGE_CONSUMED`
- Visual source before = `6815B3D37DEEDC50E14480EEB90074F05F44902A21499CCCCD2C5C684FBB6CCD`
- Visual source after = `22EFC7258F69BE28194B6E228F156450CAA00FF23E4B3A6AD711FC1F6459B912`
- Visual meta unchanged = `true`
- Scene before = `FF2D422476B94B525C0C48C261543AC8F73D55A41BE1B70340B485CB73EE0796`
- Scene after = `3E3E42F3C0F6293CDFC5D13BB459A71ED8F0520267E6D6969CAFF2EAA62CE9CF`
- normal Editor play smoke = `PASS`
- Git = `NOT_USED`

## Automated terminal facts

- Item schema = `ItemCombatEffectRequestSnapshot.v1`
- real values = `29,42,53,46,55,76`
- I011 truth = `True/True/False`
- I031 ordinary request = `false`
- accepted Item applications = `50`
- threshold skills = `6`
- BasicAttack resolutions = `12`
- shell breaks = `7`
- player = `9735/9999`
- enemy = `Defeated@75000`
- reset generation = `2`
- stale event accepted = `False`

## Presentation boundary

- Battle/P2 remains the sole damage, player HP, ledger and display-event owner.
- Scene binder updates only existing Text/Image values.
- The existing hidden terminal Item adapter/authority is reused; the dev fallback only delegates its public Initialize method and never duplicates Item truth.
- authored RectTransforms, RGB styles and hierarchy are unchanged; floating children are runtime-only.
- Visual facade returns presentation playability only.
- ShellBreak is a distinct existing-layer rupture cue; Defeated remains terminal until Idle/Reset.
- Reflection is not used.
- Formal flow/save/reward/drop/AutoCombat = `NOT_TOUCHED`.
