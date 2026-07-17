# ItemSandboxDetailUi01 Report

- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`
- Package: `TASK_START_ITEMSANDBOX_DETAILUI01`
- Guard receipt target: `GUARD_PASS_ITEMSANDBOX_DETAILUI01`
- Verification: PASS

## Implemented
- Created an independent V0.4 Item Sandbox scene with an old-paper single-column item detail card.
- Added a greybox item name list; each button label is only the Chinese display name.
- Clicking an item refreshes the detail card through `IItemDetailViewModelProvider` and `ItemDetailViewModel`.
- Added dev stub data covering lit, unlit, unawakened, awakened, inactive array bonus, and active array bonus states.
- Applied the approved 1080x1920 typography conversion and five-rarity palette: Fan #E3D8C3, Liang #87B66A, Ling #68A9E6, Xuan #B27DDF, Dao #E4A14B; shared #D8CCB7 / #998F7C / #B99A61 / #C94E3B / #748D62 / #82AE4F / #716F69.
- Reserved future preview provider interfaces without wiring formal lighting, build, awakening, acquire, upgrade, or battle-effect systems.

## Field Order
1. 道具名
2. 品阶换行；法门 / 器类 · 中文形状
3. 物品强度（品阶色动态数字）
4. 主属性
5. 触发说明
6. 点亮 / 接亮 / 开窍状态
7. 固定词条
8. 随机词条
9. 分隔线
10. 橙色词条 / 道痕
11. 基础效果
12. 核心效果
13. 法门 / Build
14. 器类 / Build
15. 分隔线
16. 摆放提示
17. 鉴定笺

## Notes
- BuildSettings check passed: item sandbox scene is not registered.
- Hierarchy naming check passed: all GameObject names are English stable names.
- CatalogProvider guard flags passed: no formal battle object, save data, or formal system access.
- CatalogProvider is active and exposes 31 catalog items.
- Catalog coverage passed: 31 items, required fields, no eye stone, unique 聚念石 lighting source.
- Source scope check completed for Item Sandbox Detail UI files.

## Errors
- None

