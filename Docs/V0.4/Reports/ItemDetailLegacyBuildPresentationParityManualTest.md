# ItemDetailLegacyBuildPresentationParity01 Manual Test

Status: `WAITING_USER_HANDTEST`

Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`

1. 旧 Build 内容：检查 I001 / I004 / I006 / I009 的法门构筑、器类构筑名称、成员、权威进度与阶段行；再走 Inventory -> Board 未点亮 -> Board 点亮 -> Move -> Return，确认实时刷新且无旧文字残留。
2. 弹窗边界与不穿透：弹窗内部任意位置不关闭，左/右/上/下外部点击关闭；外部关闭的同一次 PointerDown 不命中或拖动下层道具栏，显式关闭按钮及关闭后的下一次托盘交互正常。
