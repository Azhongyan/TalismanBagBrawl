# V0.3-NavigationFlow01 QA Checklist

## 包体

- 包体编号：`V0.3-NavigationFlow01`
- 真实入口：打开 `Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity` 后点击 Play
- Trial 目标场景：`Scene_TalismanBag_V02_FormationCounter`
- Trial 加载模式：`LoadSceneMode.Single`

## 自动验证结果

- 最终 Unity 静态编译：通过
- `error CS`：`0`
- 首页首帧关键文本可见：通过
- 首页默认 active：通过
- `RefinePageRoot` 默认 inactive：通过
- `ExplorePageRoot` 默认 inactive：通过
- `MorePageRoot` 默认 inactive：通过
- `SecondaryBottomNavRoot` 默认 inactive：通过
- 首页 `炼符` 热点进入 Refine 页壳：通过
- 底栏切换 Explore：通过
- 底栏切换 More：通过
- 底栏 Home 返回首页：通过
- 底栏 Trial 单向进入 V02 FormationCounter：通过
- V02 目标场景重复 Canvas：无
- V02 目标场景重复 EventSystem：无
- Missing Script：无

验证日志：

- `Logs/v03_navigationflow01_final_compile.log`
- `Logs/v03_navigationflow01_playmode.log`
- `Logs/v03_navigationflow01_firstframe.log`

## 测试独立性

- 未主动 `SetActive(true)` 制造目标可见状态
- 未使用 reflection
- 未临时实例化正式场景对象
- 未直接切换 `PageState` 或 `FormationState`
- 未调用 QA / Reset / Jump 入口
- 未修改存档制造通过
- 导航烟测只点击真实首页热点与真实底栏 Button

## Build Settings 最终顺序

1. `Assets/_Game/Scenes/Scene_TalismanBag_Run15Min.unity`
2. `Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity`
3. `Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity`

- `Run15Min`：保留
- 新增场景：仅 V03 MainHome 与 V02 FormationCounter
- 删除 / 重排 / 覆盖原数组：无

## QA Reviewer 必查

1. Project 中存在独立 V03 MainHome 场景。
2. Hierarchy 中存在且仅存在一个：
   - `MainHomeRoot`
   - `RefinePageRoot`
   - `ExplorePageRoot`
   - `MorePageRoot`
   - `SecondaryBottomNavRoot`
3. Play 首帧直接看到“照灯小铺”。
4. 首帧三个二级页 Root 与底栏均隐藏。
5. 点击首页“炼符”进入 Refine 本地页壳。
6. 底栏可切换 Explore 与 More。
7. Explore 明确显示 `Coming Soon`。
8. More 仅显示本地占位入口集合。
9. 底栏不出现“背包”或 `BattleBackpack`。
10. 点击 Home 返回首页，二级页和底栏隐藏。
11. 点击首页 Trial 与底栏 Trial，均应进入 V02 FormationCounter。
12. 进入 V02 后，主线落点仍由现有启动链决定。

## 用户手测

1. 打开 V03 MainHome 场景并 Play。
2. 确认首帧直接看到“照灯小铺”。
3. 点击首页“炼符”，确认进入 Refine 本地页壳。
4. 依次点击 Explore、More、Home。
5. 确认 Explore 为 Coming Soon，More 为占位入口集合。
6. 确认 Home 返回首页并隐藏二级页与底栏。
7. 点击首页“试炼”，确认进入 V02 FormationCounter。
8. 重新进入 V03 首页，从 Refine 页点击底栏 Trial，确认同样进入 V02。
9. 完整回归 V0.2 黄金路径：
   - 棋盘显示
   - 伤害数字
   - 敌人掉血
   - 道具触发
   - 阵盘供能
   - 整备入口
   - 2-9 后 Boss 不自动开战
   - 2-10 Boss 手动触发
   - 奖励正常
   - 主流程继续
10. 关闭并重新打开 Unity，再次执行第 1 步，确认默认状态仍成立。

## 尚待人工结论

- 首页 Trial 热点的人工点击体验
- Unity 完整重启后的默认状态
- V0.2 完整黄金路径
- 用户最终“通过 / 不通过”结论
