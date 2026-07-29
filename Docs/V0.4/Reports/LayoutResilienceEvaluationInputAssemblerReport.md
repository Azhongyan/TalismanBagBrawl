# Layout Resilience Evaluation Input Assembler Report

Package: `V0.4-LayoutResilienceEvaluationInputAssembler01`

Schema: `LayoutResilienceEvaluationInputAssembler.v1` / version `1`

## Result

- Synthetic fixtures: `22/22`
- Status priority: `Invalid > Unknown > Complete`
- `NotApplicable` is accepted only from the explicitly selected N01B row and produces `NotRequired/NotRequired` with null payloads.
- Selected `NotInChannel` is rejected as `Invalid` with a null evaluation input.
- Runtime execution boundary: `N01C Validator-only`; no evaluator, predicate result, clause result, or readiness result is created.
- Canonical Signature: `sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a`
- Ordering: `StringComparer.Ordinal`; numeric formatting: `InvariantCulture`; bool/presence: lowercase.
- Output collections are defensive copies and read-only.
- Repeat assembly, source-order reversal, `tr-TR` culture, source mutation, and field-sensitivity checks: `PASS`.

## Source policy

The assembler consumes only the public immutable N01B applicability snapshot, P1 build projection result, and P2 authored pressure result. It validates their public shapes without calling their providers, adapters, or validators. It makes one exact Ordinal RequirementId join, builds one neutral N01C input, and calls only `DefaultLayoutResilienceStructuralPredicateValidator.Validate(candidate)`.

Missing N01B is `Unknown + null`. Missing or incomplete P1/P2 remains Unknown/Incomplete when the selected row is Applicable or Unknown. Any malformed or Invalid source dominates all Unknown facts. No absence is represented as false, zero, or inferred NotApplicable.

## Blocked boundary

- C02 blocked rows: `32/32`
- Actual Unknown reduction: `0`
- Real requirement/readiness mapping: `0`
- N01C Evaluator calls: `0`
- N01C Validator calls: `1`
- Predicate/clause/readiness results: `0`

## Repository result

- Added files: `14`
- Existing file modifications: `0`
- Existing modification count is package-attributable scope only.
- Pre-existing unrelated tracked modifications preserved: `2`
- Leak Count: `0`
- GUID conflicts: `0`
- Trailing whitespace: `0`
- Offline verifier: `275/275 PASS`
- Unity batch verifier: `275/275 PASS` / exit `0`
- git diff --check: `PASS`
- Forbidden scope: `0`

## Protected baselines

- N01B exact 14: `acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316`
- N01B Canonical Signature: `sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326`
- N01C exact 16: `691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b`
- N01C Canonical Signature: `sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335`
- N01C-P1 exact 14: `7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48`
- N01C-P1 Canonical Signature: `sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd`
- N01C-P2 exact 14: `15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050`
- N01C-P2 Canonical Signature: `sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc`
- Item105: `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4`
- IF01 exact 11: `2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8`
- IF01 Canonical Signature: `sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428`
- Enemy existing 89: `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae`
- C01: `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a`
- C02: `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4`
- C02A: `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb`
- N01 exact 8: `03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52`
- C02A-D1 exact 7: `c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0`
- N01A exact 8: `705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de`
- Scenes14: `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b`
- Prefabs16: `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087`
- BuildSettings: `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59`
- HEAD: `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba`
- Branch: `backup/v0.4-itemdetail-maxdao-art-template01-wip-20260717`
