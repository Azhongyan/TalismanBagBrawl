# ItemFullDetailCompletePresentation01 Code Survey

| Player/Debug field | Real source | Projection rule |
|---|---|---|
| 名称/法门/器类/形状/触发/基础效果/摆放提示/文化描述 | ItemInnerDataCatalog | 玩家页中文；Debug保留 item/base identity |
| 品阶/实例身份/生成溯源 | ItemInstanceProjectionContract | 品阶名称给玩家；rarityKey、seed、version仅Debug |
| 基础属性 | Projection.Stats + ItemStatRangeSchema | 玩家显示中文Roll与候选区间；Debug保留rawUnits与formattedValue |
| 固定/随机词条 | Projection.Affixes + ItemAffixPoolAndRangeSchema | 玩家显示名称和值；Debug保留slot/affix/profile/raw/range |
| 点亮/阵脉/接亮 | ItemSystemSnapshot派生Lighting/Array快照 | 只读状态投影 |
| Build | buildQualification + BuildSynergy snapshot | 资格、计数、阶段真实；效果载荷明确未配置 |
| 核心 | eligible/visible IDs + Awakening snapshot | 玩家只显示visible名称/状态；全部ID仅Debug |
| 主Build/技能监控 | ItemSkillMonitor snapshot | 显示监控状态；技能载荷明确未配置且不执行 |
| I031 | ItemInnerDataCatalog + placement snapshots | 明确系统道具边界，不进入普通生成 |

格式化原重复于 InstanceDataAdapter、CandidateAdapter 与 Session；现统一由 ItemDetailPresentationFormatter 提供。
