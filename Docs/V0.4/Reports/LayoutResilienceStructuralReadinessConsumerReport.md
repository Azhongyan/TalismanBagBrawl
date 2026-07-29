# Layout Resilience Structural Readiness Consumer Report

- Package: `V0.4-LayoutResilienceStructuralReadinessConsumer01`
- Guard: `GUARD_PASS_LAYOUTRESILIENCESTRUCTURALREADINESSCONSUMER01`
- Enemy Guard: `ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALREADINESSCONSUMER01`
- Capability Algorithm Guard: `CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCESTRUCTURALREADINESSCONSUMER01`
- Assignment SHA-256: `9b79bc8a4c2b4a4edbc7a58bc8fa9a33ef5bac8fed6d74735b411bfd0786cfe1`
- Schema: `LayoutResilienceStructuralReadinessConsumer.v1`
- Status: `Complete / Unknown / Invalid`
- Synthetic fixtures: `20/20 PASS`
- Canonical Signature: `sha256:e38538f2de1b85ce00721236790f9b2beab63f19df076a34e0508aad4993c01a`
- New files: `14`
- existing files modified: `0`

## Authority path

- Source is only the public P3 `LayoutResilienceEvaluationInputAssemblerResult`.
- Every legal non-null P3 EvaluationInput uses the same object and `Evaluate = 1`.
- Every normal evaluator return, including null, passes the explicit `ValidateResult = 1` gate.
- Evaluator exception uses `Evaluate = 1`, `ValidateResult = 0`, no retry and no fallback.
- Public evaluator/validator injection points: `0`.
- Default evaluator: `DefaultLayoutResilienceStructuralPredicateEvaluator.Instance`.
- Default result validator: `DefaultLayoutResilienceStructuralPredicateValidator.Instance`.

## Diagnostic outcomes

- KnownTrue: `4`
- KnownFalse: `2`
- NotApplicable: `1`
- Unknown: `5`
- Invalid: `8`

KnownTrue, KnownFalse, Unknown and NotApplicable are independent structural-channel diagnostics only. No result is named Ready, Failed, score, gap, BP or total readiness.

## Boundaries

- P3 Assembler calls: `0`
- P1/P2/N01B source reads: `0`
- Real Item/Enemy/MapRule/Encounter/requirement/readiness mapping: `0`
- Readiness aggregation: `0`
- N01C evaluator implementations used in production: `1`
- Clause-to-BP conversion: `0`
- E10 migration: `0`
- Scene/Prefab/Board/Map/Battle/UI/BuildSettings modifications: `0`
- Leak Count: `0`

## C02 control

- C02 blocked rows: `32/32`
- Actual Unknown reduction: `0`
- No C02 row, input, result, or report is rewritten by this package.

## Protected baseline

| Scope | Frozen SHA-256 |
|---|---|
| N01C-P3 exact 14 | `c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c` |
| N01C-P2 exact 14 | `15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050` |
| N01C-P1 exact 14 | `7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48` |
| N01C exact 16 | `691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b` |
| N01B exact 14 | `acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316` |
| Item105 | `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` |
| IF01 exact 11 | `2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8` |
| Enemy89 | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` |
| C01 | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` |
| C02 | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` |
| C02A | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` |
| N01 exact 8 | `03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52` |
| C02A-D1 exact 7 | `c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0` |
| N01A exact 8 | `705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de` |
| Scenes14 | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` |
| Prefabs16 | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` |
| BuildSettings | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |
| HEAD | `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba` |

Protected N01C-P3 Canonical Signature remains `sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a`.

## Verification

- Offline verifier: `95/95 PASS`
- Unity batch compile/verifier: `95/95 PASS`
- `git diff --check`: `PASS`
- Forbidden scope: `0`
- Next package started: `0`
- Commit/tag/push: `0`
