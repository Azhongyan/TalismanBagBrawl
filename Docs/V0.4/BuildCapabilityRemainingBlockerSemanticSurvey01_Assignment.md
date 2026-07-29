# V0.4-BuildCapabilityRemainingBlockerSemanticSurvey01 Guard Assignment

状态：`GUARD_PASS_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01 / READY_FOR_REPORT_ONLY_SURVEY`
Enemy Guard 确认：`ENEMY_GUARD_CONFIRM_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01`
风险：`YELLOW / CROSS_SYSTEM_SEMANTIC_ANALYSIS_ONLY`
维护方：总体 / 跨系统 Guard + Capability Algorithm Guard + Enemy Guard
日期：2026-07-19

## 0. 包定位

本包是 N01A，只调查以下四项决定性 Unknown 的策划语义、通道归属、稳定来源资格、缺失语义与 C02 条件影响：

```text
capability.placement_shape
capability.debuff_counter
capability.interrupt_timing
capability.spirit_lock
```

本包不修改事实源、合同、Schema、算法、E10/E08 requirement、C02 或 readiness 行为。Survey 完成后四项仍保持 `Unknown`，所有推荐状态仍为 `USER_DECISION_REQUIRED`。

## 1. 四项唯一主分类

| Capability | 唯一主分类 | 当前阻断 Offline | 推荐阻断 Offline | 推荐通道 |
| --- | --- | ---: | ---: | --- |
| `capability.placement_shape` | `STRUCTURAL_PREDICATE` | true | false | `LayoutResilience / EncounterStructuralPredicate` |
| `capability.debuff_counter` | `NO_CURRENT_STABLE_SOURCE` | true | false，等待用户定义 | `StatusMitigation / Resistance` |
| `capability.interrupt_timing` | `RUNTIME_EVENT_SIGNAL` | true | false | `RuntimeCounterWindow / EventSignal` |
| `capability.spirit_lock` | `NO_CURRENT_STABLE_SOURCE` | true | false，等待用户定义 | `AntiDrainDefense` structural/runtime channel |

这些是报告必须采用的唯一主分类和推荐通道，不代表已经实施迁移或解除阻断。

## 2. Enemy planning meaning

```text
placement_shape
  在污染格、阵眼移动、阵眼干扰、断线等摆放压力下，保持可用布局、阵眼/核心访问和供能路径。
  不是占格数量、通用紧凑度或单件道具尺寸。

debuff_counter
  负面状态施加前或施加过程中的压制、抵抗、免疫或反制。
  与已经施加后的 cleanse_power 明确分离，不得双计。

interrupt_timing
  在敌方有效读条窗口内、施法完成前成功执行打断。
  拥有控制手段只证明战前潜力，不能证明实际 timing/outcome。

spirit_lock
  在受保护源或供能路径处阻止灵力窃取或供能切断。
  与受干扰后维持或恢复供能的 energy_stability 分离。
```

## 3. 稳定来源结论

```text
Item Snapshot：只提供稳定几何、placement、点亮与 counted-in-Build 事实。
IF01：只提供 itemInstanceId / placementId / baseItemId 身份绑定。
IF02：只提供 Item 原生单位与范围权威。
IF03：提供 trigger、cleanse、chain、nian Runtime Facts；不提供本包四项所需事实。
```

不得把以下内容误当作来源：

```text
shapeId / occupiedCells → placement_shape 结论
cleanse_power → debuff_counter
control_power / 持有控制道具 → interrupt_timing 成功
energy_stability / 道具名称 / 标签 / 中文文案 → spirit_lock
affix_debuff_target_power / target_has_debuff → debuff_counter
```

`BuildCapabilityReadContract` 是 BP-only 合同，不得承载 structural predicate 或 runtime signal。

## 4. 缺失、适用性与状态语义

```text
placement_shape
  无摆放压力 Encounter = NotApplicable
  适用但压力变换或输入不完整 = side-channel Unknown
  完整输入 = bool / predicate 结果

debuff_counter
  适用但缺少抵抗事实 = Unknown
  不得从 cleanse 推导 Zero

interrupt_timing
  战前 Offline BP 通道 = NotInChannel
  完整 Runtime horizon = KnownTrue / KnownFalse
  Runtime horizon 不完整 = Unknown
  无读条压力 = NotApplicable

spirit_lock
  适用但缺 anti-drain 事实 = Unknown
  无相关机制 = NotApplicable
```

`Unknown`、`NotApplicable` 与 `NotInChannel` 均不得解释为 `KnownZero`。

## 5. C02 影响分析

Survey 实际结果固定为：

```text
Unknown-reference reduction = 0
Projected blocked-row reduction = 0
Remaining decisive Unknown references = 64
Blocked rows = 32/32
```

N01 条件基线下允许报告以下重分类影响：

| 条件 | 引用数 | 条件解阻行 |
| --- | ---: | ---: |
| placement | 32 | 0 |
| debuff | 8 | 0 |
| interrupt | 8 | 0 |
| spirit | 16 | 0 |
| placement + debuff | 40 | 8 |
| placement + interrupt | 40 | 8 |
| placement + spirit | 48 | 16 |
| 四项全部 | 64 | 32 |

所有组合必须按每行集合减法计算，禁止直接相加或双计。任何不含 placement 的组合均解阻 `0` 行。四项全部条件解阻 32 行只在用户批准、Schema/合同完成、N01 规则获批且必要 Runtime Producer 完成后成立，不是本 Survey 的实际结果，也不是 Readiness、难度或胜负结论。

## 6. 用户决策点

报告必须生成 D1–D6 决策表，全部保持 `USER_DECISION_REQUIRED`：

```text
D1 placement predicate 集，以及是否模拟已配置地图压力。
D2 debuff counter 采用抵抗率、持续时间/层数减免、免疫还是 Runtime counter。
D3 是否批准 interrupt timing 移出决定性 Offline BP，并定义 Runtime horizon/outcome。
D4 spirit lock 采用预防性结构保护还是实际 Runtime prevention。
D5 是否批准跨通道 readiness/applicability；side-channel Unknown 是否抑制 Offline summary。
D6 是否批准 Enemy requirement schema 增加 requirementChannel 与 applicability 语义，并建立新合同。
```

本包不得代替用户作出任何选择，也不得把推荐值写成已批准值。

## 7. 文件白名单

只允许新增以下 8 个文件：

```text
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs.meta
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticMatrix.csv
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSourceEvidence.csv
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerC02Impact.csv
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerUserDecisionSheet.csv
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerLeakCheckReport.md
```

任务窗口修改已有文件数必须为 `0`。任务窗口不得新增 Assignment，不得修改 Package Queue。

## 8. 禁止文件与行为

禁止修改：

```text
Item / Enemy 事实源与 Canonical Signature
C01 / C02 / C02A / C02A-D1 / N01
IF01 / IF02 / IF03
BuildCapabilityReadContract
E08 / E10 requirement、readiness 或 Enemy schema
Scene / Prefab / Config / UI / RectTransform / BuildSettings
Battle / Board / RunFlow / SaveData / Reward / Chapter / Boss
AGENTS.md / Docs/LOCKED/* / Package Queue / 其他 Assignment
```

同时禁止：

```text
实现 N02 / C02B / C02R1 / C03
新建任何 Runtime Producer
把 shape 数量换算为 placement_shape BP
把 cleanse 当 debuff_counter
把控制潜力当成功打断
用名称、标签或文案猜 spirit_lock
将 Unknown / NotApplicable / NotInChannel 写成 KnownZero
删除、放宽或迁移 E10 requirement
输出正式 Readiness、难度、胜负、奖励或掉落结论
运行 Scene/Prefab Builder
commit / tag / push
自动启动下一包
```

## 9. 报告与 Verifier 要求

主报告与 CSV 必须覆盖：

```text
四项唯一主分类与 Enemy planning meaning
当前稳定来源、缺失事实与不合格来源
推荐 requirement channel 与 Offline 阻断状态
Unknown / NotApplicable / NotInChannel / KnownTrue / KnownFalse 的使用边界
C02 32 行逐行 requirement 集合、剩余 Unknown 与条件解阻结果
D1-D6 的选项、影响、前置合同、拥有系统与默认 KEEP_UNKNOWN
Protected hashes、Leak、白名单与现有文件 0 修改证明
```

Editor-only Verifier 只允许读取并验证证据、报告结构、确定性与保护边界；不得保存 Scene/Prefab，不得调用 Builder，不得写任何正式配置或行为。

## 10. Protected baselines

以下基线必须按任务开始时同一固定口径前后完全一致，不得刷新：

```text
Item105: b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
Enemy89: 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
C01: fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a
C02: 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4
C02A: a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb
IF01 Canonical: sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428
IF02 Canonical: sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673
IF03 Canonical: sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a
Affix Schema: sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f
150 Roll: sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8
150 Projection: sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2
Scenes14: da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16: 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings: 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
N01 exact 8 files: 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52
C02A-D1 exact 7 files: c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0
HEAD: f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

N01/C02A-D1 聚合算法固定为：按 `relativePath` 排序，每行 `relativePath|lowercaseFileSha256`，UTF-8 编码并保留 LF。撤销旧 N01 `f922...` 口径，`03d266...` 是唯一有效 N01 基线。

## 11. 验收条件

```text
新增文件 = 8
修改已有文件 = 0
四项唯一主分类 = 4/4
四项 Survey 输出状态 = Unknown / no behavior change
D1-D6 = USER_DECISION_REQUIRED
Actual Unknown-reference reduction = 0
Actual blocked-row reduction = 0
Remaining decisive Unknown = 64
Blocked rows = 32/32
Offline verifier = PASS
Unity batch compile/verifier = PASS；若被用户 Editor 锁阻断，必须如实标记，不得冒充 PASS
Protected hashes = unchanged
Leak Count = 0
git diff --check = PASS
Forbidden scope touched = 0
HEAD unchanged
```

本包不需要 Scene 或 Play 手测。

## 12. 后续顺序与阻塞

```text
1. 完成本 Survey。
2. 用户确认 D1-D6。
3. 条件启动 Enemy requirement channel/applicability schema 合同。
4. 条件启动 placement structural/layout-resilience 合同。
5. 条件启动 interrupt Runtime Event Fact 合同；Battle Producer 必须另包、另行 Guard。
6. debuff/spirit 按用户语义决定静态事实、predicate 或 Runtime 合同。
7. N02 只处理用户批准的连续 BP 能力。
8. C02B → C02R1 → 条件 C03。
```

N02、PA01、C02B、C02R1 与 C03 均继续阻塞，本包通过不自动解锁任何后续包。

## 13. 回传格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-BuildCapabilityRemainingBlockerSemanticSurvey01
Guard receipt: GUARD_PASS_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01
Enemy Guard confirmation: ENEMY_GUARD_CONFIRM_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01
新增文件 / 修改已有文件
Four unique classifications
Source eligibility summary
Missing/applicability semantics
C02 actual and conditional impact
D1-D6 decision sheet status
Protected hashes
Offline verifier
Unity verifier
Leak Count
git diff --check
Forbidden scope touched
HEAD status
Commit / tag / push status
Next package status: NOT_STARTED
```
