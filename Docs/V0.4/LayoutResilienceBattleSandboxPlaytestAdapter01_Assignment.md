# V0.4-LayoutResilienceBattleSandboxPlaytestAdapter01 Joint Guard Assignment

```text
Package: V0.4-LayoutResilienceBattleSandboxPlaytestAdapter01
Queue Id: N01C-P7
Status: WAITING_JOINT_GUARD_CONFIRMATION / NOT_READY_FOR_DEV
Target Scene: Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
Target runtime source: ItemSandboxV04BoardFullDetailAdapter.Session.Snapshot.placementSnapshot
Target algorithm: DefaultRealLayoutResilienceEvaluationPipeline.Instance
```

## 1. 包目标

把用户当前已手调、已接完 Item System 的 V0.4 棋盘只读接入 `RealLayoutResilienceEvaluationPipeline.v1`，让用户在现有 ItemSandbox 场景 Play 时看到当前布局面对四条 devOnly 结构压力的中文现象反馈。

本包只完成：

```text
当前 ItemSandbox 棋盘 ItemSystemSnapshot.v1
→ 当前已摆放普通实例的显式 IF01 binding
→ RealLayoutResilienceEvaluationPipeline.v1 一次评估
→ 现有 ItemSandboxFeedbackText 追加一行中文结构反馈
```

本包不是正式战斗接入，不生成总 Readiness，不修改 Enemy requirement，不修改棋盘或道具数据，也不接 V0.2/V0.3、UnifiedBattle、Save、Reward、Chapter。

## 2. 为什么目标是 V04 ItemSandbox

当前权威 V0.4 道具棋盘位于：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
ItemSandboxV04BoardFullDetailAdapter
ItemFullDetailBuildSandboxWorkbenchSession
ItemSystemSnapshot.v1
```

旧 `Scene_TalismanBag_V04_BattleSandboxPreview` 仍输出 `BuildSandboxLayoutSnapshot`，不是 P1/P6 要求的权威 `ItemSystemSnapshot + IF01`。本包禁止为旧棋盘另写一套转换器，也禁止让两套棋盘同时成为状态源。

## 3. 必需联合回执

开发前必须同时获得：

```text
GUARD_PASS_ASSIGNMENT_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01
ITEM_GUARD_CONFIRM_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01
ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEBATTLESANDBOXPLAYTESTADAPTER01
```

缺任意一项均不得开发。当前 Assignment 只进入联合审核，不自动释放任务窗口。

## 4. 权威输入与身份绑定

唯一棋盘输入：

```text
ItemSandboxV04BoardFullDetailAdapter.Session
→ Session.Snapshot.placementSnapshot
→ ItemSystemSnapshot.v1
```

不得调用 `DefaultItemSystemSnapshotProvider` 重新生成第二份快照，不得读取 `BuildSandboxLayoutSnapshot`，不得从 UI 格子、GameObject 名、图标或文字反推布局。

IF01 只允许按当前 Session 的公开只读数据生成：

```text
Session.Placements
Session.Instances[].Projection
ItemInstancePlacementBindingValidator.Instance.Validate(...)
```

普通 placement 的规则：

```text
placement.itemInstanceId 精确 Ordinal 命中唯一 Session.Instance
instance.baseItemId == placement.baseItemId
binding.itemInstanceId == placement.itemInstanceId
binding.placementId == placement.placementId
binding.baseItemId == placement.baseItemId
```

只把当前已摆放普通实例加入 Projection Set。未摆放候选不得加入，避免制造 projection orphan。

I031 是系统聚念石 placement：

```text
允许保留在 ItemSystemSnapshot
不得加入普通 Projection Set
不得生成普通 itemInstanceId
不得生成 IF01 binding
```

缺实例、重复身份、错配、孤儿 placement 或 I031 普通实例伪造必须保留 IF01 的 Unknown/Invalid 语义，不得猜测或补零。

## 5. Pipeline 调用边界

每次权威 Session Snapshot 引用发生变化时：

```text
IF01 Validate: 恰好 1 次
P6 Evaluate: 恰好 1 次
```

相同 Snapshot 不得每帧重复评估。允许在首次进入 Play 时评估一次；后续仅在 `Session.Snapshot` 被新快照替换时重评。

运行时 Adapter 只能直接引用：

```text
ItemSandboxV04BoardFullDetailAdapter
ItemFullDetailBuildSandboxWorkbenchSession
ItemInstancePlacementBindingValidator
DefaultRealLayoutResilienceEvaluationPipeline
```

禁止直接调用或复制：

```text
P1 LayoutResilienceItemFactProjectionAdapter
P2 AuthoredLayoutPressureSourceAdapter
P3 LayoutResilienceEvaluationInputAssembler
P4 LayoutResilienceStructuralReadinessConsumer
N01C Evaluator / Validator
P5-A / P5-M 内部实现
```

P1-P4/N01C 的权威调用只能发生在 P6 内部。不得增加 fallback、retry、第二套算法或本地 shape/pressure 判断。

`EvaluationBatchId` 必须稳定、Ordinal、无时间信息，至少由固定包 key 与当前 Item Snapshot Debug Signature 组成。不得使用时间戳、随机数、Culture-sensitive 文本或 Unity instance id。

## 6. Playtest 输出合同

新增：

```text
LayoutResilienceBattleSandboxPlaytestSnapshot.v1
```

状态精确为：

```text
Complete = 1
Unknown  = 2
Invalid  = 3
```

Snapshot 至少包含：

```text
schemaId
schemaVersion
status
devOnly = true
isEnabled = false
itemSnapshotSignature
bindingCanonicalSignature
pipelineCanonicalSignature
feedbackRows
issues
canonicalSignature
```

FeedbackRow 至少包含：

```text
stableRouteKey
chineseDisplayName
pipelineRowStatus
predicateState
chineseHint
answerMasked = true
```

开发者报告保留 English stable key；场景内只显示中文。

映射口径：

```text
KnownTrue      → 阵势在当前压力下保持稳定
KnownFalse     → 阵势在当前压力下出现断点
Unknown        → 当前布局信息不足，暂无法判断
NotApplicable  → 当前机制不适用
Pipeline Invalid → 结构预览数据异常
```

场景内只显示一行聚合现象，例如：

```text
【阵势韧性】稳定 2 项／受压 2 项
【阵势韧性】当前布局信息不足，暂无法判断
【阵势韧性】结构预览数据异常
```

不得显示：

```text
压力格坐标
连接边或 clause
完整解法
Legacy BP
DropBias
Boss 六钥匙答案
内部 Canonical Signature
英文 routeId / requirementId / pressureInputId
```

详细 route 结果只进入 devOnly CSV/报告，不进入玩家可见文本。

## 7. UI 与运行时接入

只允许复用现有：

```text
ItemSandboxRoot/FeedbackRoot/ItemSandboxFeedbackText
```

运行时通过 `ItemSandboxV04BoardFullDetailAdapter` 所在根节点内精确查找唯一 `ItemSandboxFeedbackText`。找不到或重复时返回 Invalid issue，不得按第一个对象猜测。

表现规则：

```text
保留 ItemSandbox 原交互反馈文本
移除上一条由本包追加的“【阵势韧性】”后缀
追加最新一行“【阵势韧性】...”
```

禁止：

```text
新建 UI 框、Panel、Popup、Button、Image、Canvas
创建第二个 Text/TMP
修改任何现有 RectTransform、anchor、size、position、sibling order
覆盖字体、颜色、字号或 Raycast 设置
运行 Scene Binder / Builder
保存 Scene 或 Prefab
修改 ItemSandboxV04BoardFullDetailAdapter.cs
修改 ItemFullDetailBuildSandboxWorkbenchSession.cs
```

运行时 bootstrap 只允许在 Unity Editor Play 且活动场景路径精确等于目标 ItemSandbox 场景时，把非 UI Adapter Component 临时挂到现有 `ItemSandboxV04BoardFullDetailAdapter.gameObject`。不得创建新 GameObject；不得在 Player 正式构建自动启用；退出 Play 后不得留下序列化对象。

## 8. 不允许做的内容

```text
不修改 Scene / Prefab / ProjectSettings / Packages
不修改 Item、Enemy、P1-P6、IF01 合同或 Canonical 算法
不修改 V0.2/V0.3 或旧 V04 BattleSandboxPreview
不修改 BuildGridInteractionPreviewController
不连接正式 Battle、RunFlow、SaveData、Reward、Chapter、Boss、掉落或数值
不生成胜负、伤害、奖励或章节推进
不生成总 Readiness
不迁移新的 requirement
不解除 C02 32/32 blocked
不启动 N02、PA01、C02B、C02R1、C03
```

## 9. 精确文件白名单

只允许新增以下 11 个文件；已有文件修改必须为 `0`：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/LayoutResilienceBattleSandboxPlaytestAdapter.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/LayoutResilienceBattleSandboxPlaytestAdapter.cs.meta
Assets/_Game/Scripts/TalismanBag/BuildSandbox/LayoutResilienceBattleSandboxPlaytestFeedback.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/LayoutResilienceBattleSandboxPlaytestFeedback.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/LayoutResilienceBattleSandboxPlaytestAdapterVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/LayoutResilienceBattleSandboxPlaytestAdapterVerifier.cs.meta
Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestAdapterReport.md
Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestStateReport.csv
Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestFeedbackReport.csv
Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestChecklist.csv
Docs/V0.4/Reports/LayoutResilienceBattleSandboxPlaytestLeakCheckReport.md
```

不得修改 `BuildSandboxGuardRunner.cs`；Verifier 使用独立 MenuItem / StaticBatch 入口。

## 10. 固定测试场景

Verifier 至少覆盖 10 个确定性案例：

```text
S01 非目标场景不安装
S02 目标场景缺 ItemSandbox Adapter → Invalid
S03 目标场景存在重复 ItemSandbox Adapter → Invalid
S04 完整空布局
S05 I031-only 完整布局
S06 formation_eye 稳定样例
S07 formation_eye 受压样例
S08 polluted_tile 稳定样例
S09 polluted_tile 受压样例
S10 普通 placement 缺 IF01 身份 → Unknown，不补零
S11 非法 ItemSystemSnapshot → Invalid，不调用第二套算法
```

案例允许多于 10，但必须包含以上 11 项。正常案例每个 Snapshot 只允许 IF01 Validate 1 次、P6 Evaluate 1 次。

必须验证：

```text
输入顺序反转不改变 Canonical
相同 Snapshot 不重复评估
Snapshot 变化触发一次重评
I031 不伪造普通实例
未摆放候选不进入 Projection Set
现有交互反馈被保留
场景文本无英文字母、答案坐标和内部 key
运行时源不直接引用 P1-P4/N01C
无 new GameObject / RectTransform 写入 / Scene save / Builder
```

## 11. QA 与手测

Offline 与 Unity batch 入口：

```text
TalismanBag.EditorTools.BuildSandbox.LayoutResilienceBattleSandboxPlaytestAdapterVerifier.VerifyOffline
TalismanBag.EditorTools.BuildSandbox.LayoutResilienceBattleSandboxPlaytestAdapterVerifier.VerifyStaticBatch
```

静态 QA 通过后状态只能是：

```text
DEV_COMPLETE / QA_STATIC_PASS / WAITING_USER_HANDTEST
```

用户手测：

1. 打开 `Scene_TalismanBag_V04_ItemSandbox.unity`。
2. Play 后确认原道具栏、棋盘、详情、拖拽和旋转手感不变。
3. 确认现有反馈末尾出现一行 `【阵势韧性】...`，没有新增 UI 框。
4. 改变棋盘摆法后，结构反馈只刷新一次并产生可观察变化。
5. 空布局、I031-only、稳定布局和受压布局均不报红错。
6. Console 无本包 Error/Warning。
7. 退出 Play 后 Scene 不变脏，保存前后 Scene Hash 不变。

用户明确回复手测通过后，才能回传最终 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS`。

## 12. Protected Baseline

任务开始与结束必须保持：

```text
Item105 aggregate:
b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4

P6 exact15 aggregate:
7e9b4cb329a25477b32054a7661aaa0d4ffcf5789c48f0832d462fa2c4b412fc

P6 Canonical:
sha256:cc888ce3c7748f8bb3b3eafbbc89f37daccb3812ec984470093630b3bc94f98e

IF01 core exact4 aggregate:
d2c4392acf4d1d8b4d9230cf19bc7a41591f20dcbe41cd38849b9c43a9427f07

ItemSandbox authoritative core exact4 aggregate:
283432017bf6f2d559f54c1a9d2ba7102d60cd0118761b8258b67e3397e61144

Target Scene SHA-256:
8a249cd0c943abbb6096a84c60148928c1396c8ae6e1d73f8c80c73f769c08d8

All Scenes aggregate:
da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b

Prefabs aggregate:
7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087

EditorBuildSettings.asset:
08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59

HEAD:
f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

以任务开始时磁盘状态为准保护既有 dirty/untracked 内容。不得清理、回滚、认领或纳入本包。

## 13. 回传格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-LayoutResilienceBattleSandboxPlaytestAdapter01
Guard receipts: 四项完整列出
新增文件 / 修改已有文件: 11 / 0
Target scene / source adapter
IF01 Validate / P6 Evaluate calls per changed Snapshot: 1 / 1
Scenario count / feedback row count
KnownTrue / KnownFalse / Unknown / NotApplicable distribution
Chinese-only feedback check
Interaction feedback preservation check
Snapshot no-repeat / changed-refresh check
Canonical Signature
Offline verifier
Unity compile / verifier
User handtest result
Scene hash before / after
Protected hashes
Leak Count
git diff --check
Forbidden scope touched
HEAD unchanged
commit / tag / push: none
Next package: NOT_STARTED
```

## 14. 后续门禁

本包通过只证明：当前 V0.4 ItemSandbox 棋盘可以只读调用 P6，并在现有反馈文本中显示结构诊断。

它不代表：

```text
已接正式 Battle
已完成 UnifiedBattle
已完成总 Readiness
已解除 C02/C03
```

下一步必须重新由用户选择是继续 BattleSandbox 战斗表现接入，还是回到 N02/Runtime Fact/Unified Battle 队列；不得自动启动。
