# Formal Shell Authored Binding Exact Diagnosis And First Cutover Task Contract

## Outcome

恢复 1-1、1-2、1-3 共用的现有 `UnifiedBattlePageShell` 正式装配门。
先让现有 Host 报出第一个真实失败 Owner，再只修该共享载体；不重复
Canonical Catalog 接线，不进入 Battle Session、效果家族或单道具逻辑。

## Product Context

- `CAMPAIGN_NORMAL_LV1`
- 最新用户证据：从 WorldMap 进入 1-1、1-2、1-3 均立即返回
  `UNIFIED_FORMAL_ITEM_MOUNT_BINDING_INVALID`。
- 当前分类：`USER_HANDTEST_FAIL / PRESENTATION_OR_LIFECYCLE_FAIL /
  SHARED_FORMAL_CARRIER / BATTLE_SESSION_NOT_REACHED`。

## Task Class And Ownership

- Class: `CONTAINED_ONE_GUARD`
- Primary / Development Owner: 当前 Codex 主对话窗口
- Unity serialization owner: 当前窗口的一次干净、可记录 batch 生命周期
- User decision point: `NONE`；本包不改变玩法语义、数值、掉落或存档。

## Required Order

1. 把 `UnifiedBattleFormalSceneHost.ValidateAuthoredBindings()` 的聚合错误按现有
   Owner 分组为精确诊断，保持相同 fail-closed 行为。
2. 在现有 authoring owner 中增加只读 Prefab／Scene 诊断入口；不得保存、重建
   或运行旧 `ApplySingleAuthoringPass`。
3. 用一次干净 Unity batch 命名第一个失败项。
4. 只修该失败项所在的一个现有共享载体。
5. 编译并重新运行同一精确诊断；通过后停止，不顺手处理后续 Session。

## Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1FormalObtainRebuildBattleLoopSceneAuthoring.cs`
- 若精确诊断命中，只允许修改以下现有共享载体中的必要最小集合：
  - `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemPresentationCatalog.cs`
  - `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs`
  - `Assets/_Game/Resources/V04/ItemPresentation/C1FormalItemPresentationCatalog.asset`
  - `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab`
  - `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxPrepareSurface.prefab`
  - `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab`
  - `Assets/_Game/Prefabs/TalismanBag/BattlePresentation/FormalBattlePresentationRoot.prefab`
  - `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity`
- `Logs/formal_shell_authored_binding_exact_diagnosis.log`
- 本 Task Contract
- `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md` 的本包结果段

## Protected Boundaries

- 不改 Reward、Item Authority、Canonical Item 数值、Stage、Enemy、Save。
- 不改 `C1FormalRealtimeBattleSession`、Canonical Effect Operator、NoMutation。
- 不扩 Item Detail，不改正式 VFX 产品表现。
- 不新建 Scene、Prefab、Catalog、Authority、Bridge、Manager、fallback 或测试框架。
- 不按 1-1／1-2／1-3 或 itemId 分支。
- 不恢复 Pool15／Starter 作为玩法权威。
- 不运行旧全量 `ApplySingleAuthoringPass`。
- 不继续横向 Cleanroom 删除。

## Verification

1. Unity 生命周期开始前为 `CLOSED_CLEAN`，结束后无残留进程和锁。
2. 第一次诊断必须命名精确 Owner／Predicate，不接受聚合错误作为结论。
3. 修复后现有 Prefab 与正式 Scene 的 Host authored binding 均通过。
4. Runtime／Editor 编译通过。
5. 没有正常 WorldMap Fresh Play 时最高状态为 `INTERNAL_QA / NOT_READY`。

## Stop Conditions

- `FORMAL_SHELL_BINDING_INTERNAL_PASS / PLAYER_PATH_NOT_READY`
- `FIRST_FAILED_PREDICATE_OUTSIDE_ALLOWED_SCOPE`
- `PRODUCT_SEMANTIC_GAP`
- `EXTERNAL_UNITY_OWNER`

## Status

`FORMAL_SHELL_BINDING_INTERNAL_PASS / PLAYER_PATH_NOT_READY / STOP`

## Execution Result — 2026-08-11

- `UnifiedBattleFormalSceneHost.ValidateAuthoredBindings()` 已按真实责任层拆分诊断；失败关闭行为保持不变，不再把背景、层级、Combat Log、Item Catalog、Presenter、Prepare Surface、Navigation 与 Formal Presentation Root 全部汇总成同一个 Item Mount 错误。
- 现有 authoring owner 已增加只读 Prefab / Scene 诊断入口；该入口不保存、不重建，也不调用旧 `ApplySingleAuthoringPass`。
- 现有 Unity 编译响应文件离线全量编译结果：Runtime `0` 错误、Editor `0` 错误；只有 2 条既有未使用字段警告。
- 干净租约下启动的任务 Unity batch 在日志初始化前停住；不加载工程的版本探针表现相同。两次任务进程均已回收，当前无 Unity 进程、无 `Temp/UnityLockfile`、无工程资产写入。
- 静态核对确认：正式 Scene 没有覆盖 Host 的关键引用；Shell Prefab 已绑定唯一 Canonical Catalog；旧 Formal Item Presentation Catalog 及其图片仍存在，但它继续依赖旧 Pool15 / Starter 责任岛。它是当前最可疑的共享前置门，但在精确运行诊断返回前不执行猜测式切除。
- 下一门只要求用户 Unity 完成脚本导入后，从 WorldMap 进入任一关一次并返回新的精确 `UNIFIED_FORMAL_*` 错误；这不是玩家验收，也不要求重复测试 1-1、1-2、1-3。

## Same-package Correction — Presentation Catalog Cutover

- 用户运行先返回 `UNIFIED_FORMAL_ITEM_PRESENTATION_CATALOG_INVALID`，随后精确到 `C1_ITEM_PRESENTATION_PROFILE_SIGNATURE_MISMATCH`；首个真实断点因此确认是旧 Formal Item Presentation Catalog 对旧 Pool15／Starter Profile 的签名依赖，而不是 Board 容量、关卡数据或 Battle Session。
- `C1FormalItemPresentationCatalog` 已收口为纯展示资源载体：只验证唯一展示身份、图片、图片身份、效果族、展示样式、Cue 与 I031 明暗图完整性。玩法身份、属性和战斗事实继续由唯一 Canonical Item Authority 提供。
- 已移除该 Catalog 对旧 Pool15、旧 Starter、Inner Catalog 的运行查询，以及 Profile／Row／Catalog 签名门禁。旧序列化字段即使仍残留在未保存资产文本中，也不再参与运行判断；本包没有更新、计算或比较旧签名。
- Catalog 与 Row 对下游保留的是可读运行身份，不新建第二 Catalog、Adapter、Bridge 或 fallback；Scene、Prefab、资源 YAML、Reward、Battle、Stage 与 Save 均未修改。
- 展示 Catalog focused tests 已改为验证纯展示结构、精确查找、未知／稀有度失败关闭、I031 明暗图、不可变快照和未来道具数据驱动，不再依赖旧 Pool15 fixture。
- 修正后的离线全量编译：Runtime `0` 错误、Editor `0` 错误；只有 2 条既有未使用字段警告。Unity／Fresh Play 未运行，等待用户 Editor 完成脚本重载后进入任意一关一次，确认该共享门是否通过或返回下一条精确错误。

## User Editor Runtime Result

- 用户完成脚本重载后进入正式关卡，没有新的红色错误；此前的 `UNIFIED_FORMAL_ITEM_PRESENTATION_CATALOG_INVALID` 与 `C1_ITEM_PRESENTATION_PROFILE_SIGNATURE_MISMATCH` 均未再出现。
- `UNIFIED_EXACT_ITEM_DETAIL_RUNTIME_BINDING_IDENTITY` 由 `UnityEngine.Debug.Log` 输出，是只读绑定身份信息，不是 Error 或 Warning；它证明现有 Item Detail Presenter 找到了 Scene、Carrier、Panel、CloseButton 与 Overlay Canvas。
- Formal Shell 共享启动门本包已通过。Item Detail 功能本身仍按既定范围冻结；黄色 Warning 尚未按唯一消息归因，不纳入本包，也不影响本包停止条件。
