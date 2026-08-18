# Formal Battle Presentation Prefab Promotion — Carrier Parity

- Package: `V0.4-FormalBattlePresentationPrefabPromotion01`
- Product context: `CAMPAIGN_NORMAL_LV1`
- State: `COMPLEX_GUARDED_ONCE / PRESENTATION_INSPECTION_BLOCKED`
- Player-visible delivery: `PARTIAL`
- Milestone completion evidence: `NO`

## BattleSandbox → Unified carrier parity

| Player-visible row | Accepted BattleSandbox carrier / grammar | Unified promoted carrier | Exact reuse decision |
|---|---|---|---|
| Player body and HP | Authored `Player`, `V02PlayerAvatar`, `PlayerHPBar` | `FormalBattlePlayerPresentation.prefab` | Reuses the accepted avatar and HP-frame assets through serialized profile references; HP is driven only from immutable Battle player state/cues. |
| Enemy frame and HP | `C1AuthoredEnemyPresentationRoot`, `C1AuthoredEnemyPresentationSlotView`, `V02EnemyArea`, `EnemyHPBar` | `FormalBattlePresentationRoot.prefab` + three nested `FormalBattleEnemySlot.prefab` instances | Promotes the same dark vermilion/gold framing and stable-slot grammar. Actor identity requires exact `actorBalanceId/contentId/runtimeProfileId/stableActorOrder`; no label or hierarchy-name inference. |
| Enemy animation | `C1EnemyVisualProfileCatalog` | Serialized `FormalBattlePresentationProfile.asset` | Reuses the catalog's accepted frame ordinals, frame timing and calibration bounds for Shattered Host; reuses the existing porcelain-hound idle/attack/hit/death textures. |
| Current target | `C1AuthoredEnemySelectionOutlineEffect` | Same effect component on each formal enemy slot | Direct component reuse. Exactly one outline is enabled only by authoritative `TARGET_CHANGED`; reset, defeat and unbind clear it. |
| Calibration | `C1AuthoredEnemyCalibrationMeshEffect` | Same effect component on each formal enemy slot | Direct component reuse with serialized reference/union bounds; no sprite-name or display-name inference. |
| Item trigger source | Accepted short source-pulse grammar around the formal card | `FormalBattleCueFxAudioRoot.prefab` resolving six serialized protected `C1FormalItemCardView` carriers through immutable roster `baseItemId@rarityKey` | Keeps only restrained local wind-up/pulse. Card travel, card projectile and hard-block projectile are excluded. |
| Enemy Attack / Hit / Death | Accepted slot-local attack, short hit reaction and terminal death grammar | `FormalBattleEnemySlotView` | Scheduled enemy cue captures the Battle-identified eligible actor; accepted attack uses that exact carrier. Hit is short/local and Death plays once. |
| Damage float | `BattleSandboxAuthoredTmpPresentation` and accepted warm damage typography | `FormalBattleDamageFloatPool.prefab` | Reuses `MFLangSongJianYuan-Regular SDF`, a fixed authored pool of 10 TMP labels and red/vermillion/warm-gold gradient. Exact cue receiver and `floatPayload` only. |
| VFX / audio root | Accepted localized cue VFX/audio grammar | `FormalBattleCueFxAudioRoot.prefab` + three serialized mono PCM clips | Uses a serialized existing `ZhaoShaMirror_VFX_05` texture as localized, non-travelling RawImage effects. Source pulse, dry enemy attack and muted impact each have a deterministic replaceable WAV carrier; runtime missing-clip handling remains quiet and non-blocking. |
| Unified composition | Existing `EnemyInfoArea` and `BattleFeedbackLayer` slots | Nested prefab instances in `UnifiedBattlePageShell.prefab` | Existing placeholder children are retained but inactive; owned slot surfaces are transparent. Board/Tray/Card RectTransforms and behavior are unchanged. |

## Explicit exclusions

- No BattleSandbox runtime controller, `RuntimeInitializeOnLoad`, `sceneLoaded` bootstrap, fixture, Lv40 panel or dev/test control was copied.
- No Shougunu runtime graph or asset was imported.
- No runtime fallback hierarchy, `GameObject.Find`, display-text inference, roster-order guess or second target owner exists.
- No card/hard-square projectile or coarse fire impact was restored.
- No Battle, Item, Enemy, Flow, Reward, Save or terminal truth is computed or mutated by Presentation.
- Unified Scene serialization remains unchanged; composition is entirely in the allowed Unified prefab and narrow host binding.

## Cue seam and deterministic evidence

- `UnifiedBattleFormalSceneHost` consumes `CurrentRealtimeStart.initialCues` once per exact start-envelope canonical signature, then sends every `LastRealtimeTick.emittedCues` through the same monotonic/dedupe consumer.
- Accepted-application dedupe key includes application id, cue kind, source, exact target actor and stable order, preserving legitimate multi-target roles while suppressing replay.
- The authored validation exercised: stage-1 initial target, repeated initial envelope, repeated tick cues, stage-2 refreshed envelope/dedupe reset, refreshed authoritative target, and idempotent reset cleanup.
- Unity authoring marker: `FORMAL_BATTLE_PRESENTATION_PREFABS_AUTHORED_PASS`.
- Targeted compile: Tundra success; Runtime and Editor assemblies loaded without C# errors.
- Audio light-fix authoring marker: `FORMAL_BATTLE_PRESENTATION_AUDIO_LIGHT_FIX_AUTHORED_PASS`; all three profile slots resolve to serialized `AudioClip` assets.
- Audio carriers: mono 22.05 kHz, 16-bit PCM, non-looping source files; deterministic durations are 120 ms / 200 ms / approximately 150 ms and generated peaks remain below -6 dBFS.
- Protected hashes remained exact: Unified Scene `FC5E...F1D9`; released loop `DB52...623B`; loop tests `9C50...8D0E`; specified Sandbox/outline/profile/TMP/LiHuo baselines all match the Assignment.

## Remaining evidence boundary

The required single normal-Editor attempt opened the existing WorldMap scene successfully, but the launched Editor process exposed no interactive Windows main-window handle in this execution session. Fresh Play and the authored Challenge button therefore could not be operated without prohibited keyboard/mouse automation or launch-context injection. No substitute Unified-open, synthetic context, screenshot harness or compile evidence is being promoted. All live 1-1/1-2 observation rows, including in-context audibility, remain unproven as `PRESENTATION_INSPECTION_BLOCKED`; this report does not claim close-review readiness, package completion or milestone completion.
