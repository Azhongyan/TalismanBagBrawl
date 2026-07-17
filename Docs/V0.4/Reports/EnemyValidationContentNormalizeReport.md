# Enemy Validation Content Normalize 01 Report

- Result: `PASS`
- Execution: `Unity batch`
- Verifier: `33/33`
- Canonical signature: `sha256:bbc3c329874019d01d05912ee3b727025432ffc57678c2e9a3ad0f3e9eaf3d46`
- Player-safe signature: `sha256:101c7f1887fb868a05a72be6b2e1f5c4aa08ae2d2a93e978b493460ea13733cd`
- Carriers: `11 enemy / 7 boss / 18 total`
- Mechanic profiles: `10 enemy / 6 boss / 16 total`
- MapRules: `10`
- Carrier bindings: `17`
- MapRule bindings: `30`
- Intentional exceptions: `2`

## Checks

- PASS — `count.enemy`: 11
- PASS — `count.boss`: 7
- PASS — `count.carrier.total`: 18
- PASS — `count.profile.enemy`: 10
- PASS — `count.profile.boss`: 6
- PASS — `count.profile.total`: 16
- PASS — `count.map`: 10
- PASS — `count.carrier.binding`: 17
- PASS — `count.map.binding`: 30
- PASS — `count.exception.carrier`: 1
- PASS — `count.exception.profile`: 1
- PASS — `id.ordinal.unique`: 44
- PASS — `domain.e01.projection`: 11/7/16/10
- PASS — `lookup.ordinal`: True/False
- PASS — `binding.reuse`: 2
- PASS — `binding.map.shape`: 10
- PASS — `orphan.unexplained`: 0
- PASS — `mapping.status`: True
- PASS — `canonical.order.independent`: sha256:bbc3c329874019d01d05912ee3b727025432ffc57678c2e9a3ad0f3e9eaf3d46
- PASS — `player.canonical.order.independent`: sha256:101c7f1887fb868a05a72be6b2e1f5c4aa08ae2d2a93e978b493460ea13733cd
- PASS — `canonical.enemy.developer-content.sensitive`: changed
- PASS — `player.enemy.developer-content.isolated`: unchanged
- PASS — `canonical.boss.developer-content.sensitive`: changed
- PASS — `player.boss.developer-content.isolated`: unchanged
- PASS — `canonical.player-safe-content.sensitive`: changed
- PASS — `player.player-safe-content.sensitive`: changed
- PASS — `player.answer.leak`: none
- PASS — `input.output.readonly`: True
- PASS — `runtime.buildsandbox.reference`: 0
- PASS — `editor.legacy.reader`: 1
- PASS — `protected.e01e02`: 10/10 unchanged
- PASS — `protected.legacy`: 3/3 unchanged
- PASS — `package.scene.prefab.config.battle.board.item`: 0/0/0/0/0/0
