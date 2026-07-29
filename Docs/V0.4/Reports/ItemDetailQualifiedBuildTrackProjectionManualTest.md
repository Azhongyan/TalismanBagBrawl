# ItemDetailQualifiedBuildTrackProjection01 Manual Test

Status: `WAITING_USER_HANDTEST`

Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`

1. Play 后点击道具栏 I001：应显示器类 `符` Build 资格，不再显示旧 fallback。
2. 将 I001 放到未点亮位置：显示未点亮，source / qualified 均不贡献。
3. 合法放置 I031 使 I001 点亮：显示 `qilei:fu = 1/4`。
4. 点击 I004：显示法门震雷与器类令两条资格；点亮上阵后两条轨道均反映该实例贡献。
5. 点击 I006：Inventory 与 Board 都明确无 Build 资格；点亮后仍为 Known Zero，不贡献。
6. 点击 I009：只显示离火法门资格；点亮上阵后显示 `famen:lihuo = 1/6`。
7. 将普通道具移位、收回道具栏并重新打开：详情立即跟随最新状态。
8. 点击 I031：只显示系统道具详情，不出现普通法门 / 器类资格。
9. 连续点击多件道具：不得残留上一件道具的轨道、进度或阶段。
