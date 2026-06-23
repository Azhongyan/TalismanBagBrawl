# V0.3 MainHomeScene01 Retry QA Checklist

版本：`V0.3-MainHomeScene01-Retry`

## 自动校验

- [ ] Unity 编译 `error CS = 0`
- [ ] `V03MainHomeScene01RetrySmoke.VerifyBatch` 输出 `SMOKE_SUCCESS`
- [ ] DataCatalog `Error = 0`
- [ ] 首页标题为“照灯小铺”
- [ ] 默认热点共 15 个，且只来自白名单
- [ ] Trial 与 Refine 使用 `MainHomeGreyboxPanel.Show` 注入的既有委托
- [ ] ComingSoon 与 Locked 点击不会抛异常
- [ ] visualKey / iconKey 缺失时仅 Warning，并显示灰盒
- [ ] 首页源码不调用 StartCombat、ResetSave、UpgradeService 或 RewardService
- [ ] 首页无 BattleBackpack，PVP 占位不命名为“论道”

## 首页黄金路径

1. 启动 `Scene_TalismanBag_V02_FormationCounter`。
2. 到达既有首页停靠点后，确认标题为“照灯小铺”。
3. 确认资源摘要、主线进度、当前目标、店内热点与右侧快捷入口可见。
4. 点击柜台，确认显示当前目标提示。
5. 点击 Trial，确认只走既有主线委托入口；不重置存档、不清背包、不清阵盘。
6. 返回首页，点击 Refine，确认进入既有培养入口；若当前不可培养，应显示锁定提示。
7. 点击后屋、梦签、罗盘台、旧炉子、切磋占位及右侧快捷入口，确认只显示 ComingSoon。
8. 验证首页没有完整 BottomNavBar，也没有 BattleBackpack 标签。

## V0.2 战斗黄金路径

1. 从 Trial 进入当前主线，确认战斗棋盘显示。
2. 确认伤害数字显示、敌人血量下降、道具正常触发、阵盘供能可见有效。
3. 验证 1-10 与 2-10 主线闭环仍可推进。
4. 验证 2-9 后停在 BossInfoPanel。
5. 验证 2-10 Boss 仍需手动触发。
6. 验证奖励、掉落、培养消耗与存档结构没有变化。

## 立即回滚条件

- 棋盘或伤害数字消失。
- Trial 导致 ResetBattle、ResetSave、清背包或清阵盘。
- 2-9 不再停在 BossInfoPanel，或 2-10 自动开战。
- 道具、供能、奖励或主线闭环失效。
- SaveData、奖励表、数值表或受保护核心文件出现改动。
