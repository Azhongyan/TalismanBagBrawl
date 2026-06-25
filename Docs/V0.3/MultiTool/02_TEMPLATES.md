# MultiTool Templates

本文件提供多工具协作时可复制使用的模板。

这些模板不授权开发；只有 Codex 官方工单、Guard、用户审批才授权开发。

## 1. Handoff Summary Template

```text
# <包体名> HANDOFF SUMMARY FROM <工具名>

来源工具：
包体名：
官方任务档案：
执行角色：开发 / QA / 其他
当前状态：HANDOFF_TO_CODEX / BLOCKED / RETURNED

## 1. 一句话摘要

## 2. 做了什么

## 3. 修改文件

- 修改：
- 新增：
- 删除：

## 4. 验证结果

- 编译：
- 自动测试：
- 手测建议：

## 5. 是否触碰禁止范围

结论：否 / 是
说明：

## 6. 已知风险

## 7. git 证据

- diff 摘要：
- commit hash（如有，且必须用户授权）：

## 8. 下一步应交给谁

Codex / QA / Tech Architect / Task Writer / 用户
```

## 2. Dev Receipt Template

```text
# <包体名> DEV RECEIPT FROM <工具名>

来源工具：
任务档案：
执行 worktree / branch：
开始时间：
结束时间：

## 1. 执行范围

## 2. 修改清单

- 修改：
- 新增：
- 删除：

## 3. 实现说明

## 4. 未做内容

## 5. 编译与测试

## 6. 测试独立性自检

- 是否反射制造通过：
- 是否临时实例化制造通过：
- 是否直接切状态制造通过：
- 是否改存档 / 配置制造通过：

## 7. 边界自检

- 是否修改 AGENTS.md：
- 是否修改 Docs/LOCKED/*：
- 是否越过任务档案允许范围：
- 是否触碰 V0.2 保护链路：

## 8. 回滚方式

## 9. 提交给 Codex 的建议
```

## 3. QA Report Template

```text
# <包体名> QA REPORT FROM <工具名>

来源工具：
任务档案：
QA 类型：工具侧 QA / Codex 最终 QA / 用户手测辅助

## 1. 结论

通过 / 不通过 / 阻塞

## 2. 验收依据

## 3. 执行路径

## 4. 结果证据

## 5. 失败现象（若有）

## 6. 是否违反测试独立性

## 7. 用户手测建议

## 8. 是否建议回 Codex Tech Architect
```

## 4. Worktree Lock Template

```text
# <包体名> WORKTREE LOCK FROM <工具名>

工具：
包体：
worktree：
branch：
锁开始时间：
预计释放条件：

## 1. 允许写入

## 2. 禁止其他工具同时写入

## 3. 高风险文件说明

## 4. 释放记录
```

