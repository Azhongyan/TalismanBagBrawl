# WorktreeLocks

本目录存放多工具文件锁说明。

推荐命名：

```text
<包体名>_LOCK_FROM_CODEX.md
<包体名>_LOCK_FROM_CLAUDE.md
<包体名>_LOCK_FROM_ZCODE.md
```

文件锁用于避免多个工具同时修改 Unity 高冲突资源。

锁文件必须写明：

- 工具名
- 包体名
- worktree / branch
- 预计修改文件或目录
- 明确禁止其他工具同时修改的文件或目录
- 锁开始时间
- 锁释放条件

Scene、Prefab、ProjectSettings、Save、RunFlow、Formation、Reward、Boss、数值表等文件必须谨慎锁定。

