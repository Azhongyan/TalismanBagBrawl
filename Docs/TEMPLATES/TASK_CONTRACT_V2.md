# Task Contract V2 模板

> 只填写执行所需字段。简单任务可压缩成一屏；复杂任务也不复制整套项目历史。

## 1. Outcome

用户最终能观察或验证到什么：

## 2. Product Context

- `DEV_SHOWCASE_LV40`
- `PLAYTEST_VERTICAL_SLICE`
- `CAMPAIGN_NORMAL_LV1`
- 其他：

不得靠 Scene 名、GameObject 名或中文展示名推断。

## 2A. Task Mode

- `装修`（默认）：不改变 Truth Owner、核心数据源或正式状态合同。
- `水电`：显式改变 Catalog / Authority / Session / Reward / Save / Bridge 等架构边界。
- `检查房间`：只读治理，不修改产品文件。

本任务模式：

- Expected New Owner：`0 / N`
- Expected New Data Source：`0 / N`
- Expected New Fallback：`0 / N`

装修任务执行中若发现必须改变 Owner、Truth 或核心 Contract，必须停止并申请升级为水电，不得自行增加 Provider、Bridge、Fallback 或第二数据源。

## 3. Task Class

- `SIMPLE_DIRECT_CLOSE`
- `CONTAINED_ONE_GUARD`
- `COMPLEX_GUARDED_ONCE`

分类理由：

## 3A. User Review Gate

- 是否需要：`YES / NO`
- 判断理由：
- 固定类别 Guard：
- `USER_ACCEPTED_SCOPE` 标识或链接（不需要则写 `NOT_REQUIRED`）：

涉及视觉、动画、VFX、声音、手感、交互、技能语义、流程节奏或掉落策略时，默认必须先经过 `USER_REVIEW_GATE`。未取得用户明确同意，不得创建开发 Assignment 或派发实现。

## 4. Owners

- Primary Guard：
- Development Owner：
- State / Data Owner：
- Presentation Owner：
- User Decision Point（没有则写 `NONE`）：

每个包只有一个 Primary Guard。其他 Guard 只回答合同中列出的一个明确归属问题。

固定类别 Primary Guard 在取得所需的 `USER_ACCEPTED_SCOPE` 后，负责创建或复用开发任务、完成一次性派发并事件驱动收口。总收口 / 系统归属拆分窗口默认只路由到 Primary Guard，不越级创建实现任务。

## 5. Allowed Writes

精确目录 / 文件 / 新增文件白名单：

## 6. Forbidden Writes

明确列出与本任务相邻但不得修改的系统、Scene、Prefab、数据、存档和流程：

## 7. Required Locks / Sources

除 `AGENTS.md`、V2 Lock 与工程快照外，本任务真正需要读取的锁、代码、数据和素材：

## 8. Patch Budget

- 建议文件数：
- 允许新增：
- 允许修改：
- 超预算时必须停止还是先说明：

默认参考：

| 类型 | 文件数 |
|---|---:|
| 小 Bug | 1–4 |
| 小功能 | 3–8 |
| 垂直切片 | 6–15 |
| 架构 / 迁移 | 独立包 |

## 9. Required Behavior

用输入 → 行为 → 输出描述，不写实现散文：

1. 
2. 
3. 

## 10. Failure / Edge Cases

- 缺数据：
- 重复事件：
- 生命周期 / Reload：
- 非法状态：
- 失败时是否回滚：

## 11. Delivery Shape

- Config / Profile：
- Runtime：
- Editor：
- Scene / Prefab：
- Adapter / Bridge：
- Report（必要时）：

## 12. Verification

只选择与风险匹配的最小集合：

- [ ] 链接 / 格式 / diff
- [ ] 定向编译
- [ ] 标准单元测试
- [ ] 标准集成测试
- [ ] 一次 Scene / Prefab authoring
- [ ] 一次 Fresh Play
- [ ] 一次用户视觉 / 手感测试
- [ ] 固定 APK build
- [ ] 实机 smoke

明确 PASS 条件：

明确不需要做的 QA：

## 13. Process Ownership

若启动长任务：

- command：
- owned PID：
- log：
- timeout：
- cleanup：

若不需要 Unity，明确写 `UNITY_NOT_REQUIRED`。

## 14. Completion

以下全部满足即可结束：

- 写域符合；
- 目标行为满足；
- 最小验收通过；
- 无任务自有残留进程；
- 无无关修改；
- 没有未解决的用户决定。

同包 Light Fix 留在同一任务，不重新拆包。

## 15. ROOM CHECK（中型以上任务）

- Task Mode：`装修 / 水电`
- New Owner：`0 / N`
- New Data Source：`0 / N`
- New Fallback：`0 / N`
- Formal → Sandbox：`0 / N`
- Pending Retirement：`0 / N`
- Room Status：`CLEAN / WARNING / TEMPORARY_DEBT`

若不是 `CLEAN`，只记录第一个真实原因与 `DeleteWhen`。小型装修任务无需机械填写完整回执，只需确认没有改变 Owner、数据源或 Fallback。

---

## Bug 任务补充

### Reproduction

- 起始状态：
- 操作：
- 实际结果：
- 期望结果：
- Fresh Play / APK / Reload 是否相关：

### First Failure Layer

必须选择一个：

- `PRODUCT_LOGIC_FAIL`
- `FIXTURE_OR_DATA_NOT_READY`
- `PRESENTATION_OR_LIFECYCLE_FAIL`
- `QA_HARNESS_FAIL`
- `EXTERNAL_ENVIRONMENT_BLOCKED`

### Evidence

只记录直接支持归因的日志、堆栈、对象、输入和状态；不要附无关大报告。

### Fix Boundary

说明为什么修复仍在同包，或为什么必须升格为新的架构 / 迁移包。
