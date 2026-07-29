# Bone Aspect Carrier Presentation Catalog 01 Report

- Package: `V0.4-BoneAspectCarrierPresentationCatalog01`
- Guard marker: `ENEMY_GUARD_ASSIGNMENT_BONEASPECTCARRIERPRESENTATIONCATALOG01`
- Result: `PASS`
- Execution: `same-source deterministic verifier`
- Verifier: `44/44`
- Schema: `BoneAspectCarrierPresentationCatalog.v1`
- Full Canonical: `sha256:56c245b02fc6dfeee0e47d37aeab969c34c61ec2e3ac13f1c66abf97b7567f70`
- Player-safe Canonical: `sha256:4e4882ba5ffa77dcc9c9d0d7372c874971808878ca2b8e6d8e0346331ff1cd24`
- E02 dependency Canonical: `sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833`

## Locked counts

| Contract | Result |
|---|---:|
| Carrier / Enemy / Boss | 16 / 12 / 4 |
| Chapter / VisualFamily | 4 / 4 |
| Art slots | 93 = 72 / 16 / 5 |
| E02 references | 34 = 24 / 1 / 9 |
| Distinct / unresolved E02 keys | 12 / 0 |
| CandidateReuseOnly / runtime implemented | 34 / 0 |
| Gap survey required | 16/16 |
| User decisions selected | 0/4 |
| MechanicProfile / SkillPattern / BossPhase / Runtime consumer | 0 / 0 / 0 / 0 |

## P0 authority metadata

- Accepted P0 Canonical: `sha256:3350c02ee92165f46ba87e92056d58b79bd6e2824f5b194cbe5a827eb97d7926`.
- P0 actual disk Assignment SHA-256: `4351ce9b5c070ee27e11eabc0f8debdd5e9d38c6bf4aa225c21d09175a433b23`.
- Historical delegated marker is 63 characters and remains recorded verbatim: `4351ce9b5c070ee27e11eabc0f8debd5e9d38c6bf4aa225c21d09175a433b23`.
- P1 Assignment SHA-256: `0ca7e1ab5f089c0522d156b8def74e327737a9590756134512b4415a588fbfd8`.
- The malformed historical P0 marker is evidence only; it is not substituted for the 64-character disk hash.

## Boundary

- Immutable, devOnly presentation metadata only; `isEnabled=false`, `entersFormalFlow=false`, `runtimeImplemented=false`.
- E02 rows are typed `CandidateReuseOnly` references, not MechanicProfile, Skill, CounterWindow, BossPhase, or Runtime bindings.
- BA-D1..D4 remain `USER_DECISION_REQUIRED / NOT_SELECTED`; P2 is not started.
- No Scene, Prefab, Config, Item, Battle, Board, RunFlow, SaveData, Reward, Drop, or Chapter integration.
- Player-safe projection is a safe view only and is not wired to player UI.
