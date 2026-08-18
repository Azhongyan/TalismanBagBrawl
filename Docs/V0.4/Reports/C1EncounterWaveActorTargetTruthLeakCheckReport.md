# C1 Encounter Wave Actor Target Truth Leak Check

- Package: `V0.4-C1EncounterWaveActorTargetTruth01`
- Result: `PASS`
- Old sequential actor-as-wave authority: `0`; the legacy `entries` surface is a derived compatibility view only.
- Held Wave partial Actor creation: `0`.
- Direct damage broadcast targets: `0`.
- Stale Actor ID / HP / shell / selection / request / cue / action facts after two resets: `0`.
- Scene / Prefab / BuildSettings / SceneBinder / UI / Visual / VFX / V02 / V03 / Item algorithm mutations: `0`.
- Scoped runtime compilation: `PASS`; one pre-existing unrelated warning in `ItemDetailAuthoredRowsVerticalLayoutGroup`.
- Full Unity batch launch: `BLOCKED_AT_STARTUP`; the process created no log and stayed idle until stopped. This is not reported as a verifier pass.
