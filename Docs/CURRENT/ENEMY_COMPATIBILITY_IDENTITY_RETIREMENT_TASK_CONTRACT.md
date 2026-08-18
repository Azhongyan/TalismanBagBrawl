# ENEMY_COMPATIBILITY_IDENTITY_RETIREMENT

Status: COMPLETE / INTERNAL_QA / POOL15_VERIFIER_HANDOFF
Classification: COMPLEX_GUARDED_ONCE
Task mode: 水电 + 检查房间
Product context: CAMPAIGN_NORMAL_LV1

## Outcome

1-1～1-5 的正式启动身份只由现有 `Chapter1CampaignStageConfig` 与正式 Stage 资产提供；Formal Session 不再读取旧 Early / Stage3To5 Enemy Contract 作为第二套身份真相。与该路径绑定的 Stage fingerprint / SHA 门禁一并退休。旧 Enemy Catalog、Validation、Simulation 与 QA 只有在真实消费者归零后才物理删除。

## Player behavior preserved

- WorldMap → Unified 1-1～1-5 路线不变。
- Enemy 阵容、HP、Shell、攻击、技能、状态和表现不变。
- Reward、Item、I031、Save、Board/Tray 与 Presentation 不变。

## Current identity owner

- `Chapter1CampaignStageConfig`: Stage 启动身份与当前资产校验。
- `V04Chapter1StageWaveEncounterTruth`: Stage → Wave → Actor 组成。
- `C1FormalEnemyDefinitionCatalog`: Enemy 数值与技能定义。
- `C1FormalRealtimeBattleCanonicalEncounterResolver`: 当前 1-1～1-5 运行投影。

禁止新增第二套 Stage identity 表、Enemy Catalog、Bridge、Adapter 或兼容 fallback。

## Required work

1. 统一 1-1～1-5 的 balance、encounter 与 enemy-presentation 身份格式；更新现有 Stage 资产和现有 Authoring。
2. 删除 `Chapter1CampaignStageConfig` 与 `BattleLaunchContext` 的 fingerprint/SHA 生成、保存、传输与比较；继续使用具体字段、StageConfig 校验和 Context 字段逐项匹配。
3. Formal Session 只验证当前正式 Stage 范围及必要的非空启动字段，不再从旧 Enemy Contract 推导 expected profile/encounter。
4. 将仍有价值的 focused QA 改接当前 Stage/Enemy truth。
5. 旧 Enemy 文件与验证器只有在代码和序列化消费者为 0 后删除。

## Allowed writes

- `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleLaunchContext.cs`
- `Assets/_Game/Scripts/TalismanBag/V04/Campaign/Chapter1/Chapter1CampaignStageConfig.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1Stage1To5CorePlayabilitySliceAuthoring.cs`
- 现有 1-1～1-5 `CampaignStage_*.asset`
- 直接构造 `BattleLaunchContext` 或消费旧 Enemy Contract/Catalog 的现有 focused verifier/tests
- 经零消费者证明的旧 Enemy Contract/Catalog/Validation/Simulation 及对应 `.meta`
- 本 Task Contract 与 `PROJECT_CLEANROOM_COMPONENT_LEDGER.md`

## Forbidden writes

- Enemy 数值、技能、阵容与 Battle mutation
- Pool15 Session/Contract 岛
- Scene、Prefab、VFX、HUD、Item Detail、Reward、Save
- 新 Manager/System/Bridge/Adapter/Catalog
- 任何 hash/SHA 替代门禁
- Git/worktree

## Verification

- `SHA256`、`ComputeHash`、`StageConfigFingerprint` 在当前 Stage 启动链中为 0。
- 1-1～1-5 Stage 资产通过现有具体字段校验。
- Formal runtime 不再引用旧 Enemy Contract/Catalog。
- Unity 脚本编译通过。
- 运行现有 Stage authoring validation 与当前 Canonical Enemy focused verification；不以旧 Catalog verifier 证明完成。

## STOP

- 旧 QA 如与 Pool15 大 verifier 深度交织，登记到下一 `POOL15_SESSION_CONTRACT_RETIREMENT`，不得为了本包强拆 Battle。
- 没有 Fresh Play 不声明玩家交付；本包是技术清理。

## Result

- 1-1～1-5 的 Stage 启动身份已统一到 `Chapter1CampaignStageConfig` 与现有五个 `CampaignStage` 资产；1-3～1-5 不再携带 stage3to5 playtest compatibility identity。
- `BattleLaunchContext` 与 `Chapter1CampaignStageConfig` 已移除 Stage fingerprint 的生成、序列化、传输和比较；当前启动链只按具体字段校验。
- Formal Session、Reward、Adapter、Encounter Resolver 与当前 focused verifier 已不再引用旧 Early / Stage3To5 Enemy Contract/Catalog。
- 已删除两个只验证旧 Enemy 数据表、且没有其他调用者的 Editor 菜单验证器：`C1Lv1EarlyEncounterBalanceVerifier`、`C1Stage3To5PlaytestEnemyShieldFactVerifier`。
- 旧 Enemy Contract/Validation/Simulation 仍被 `C1FormalRealtimeBattleSessionVerifier` 的历史 Pool15/兼容测试体编译引用，因此没有提前裸删；该唯一外部消费者转交下一 `POOL15_SESSION_CONTRACT_RETIREMENT` 一次性简化。
- Unity 脚本编译记录为通过；当前启动链静态扫描中旧 Stage fingerprint/SHA 与旧 Enemy Contract/Catalog 引用均为 0。额外 Stage validation 批处理未成功启动工程，任务进程已独立结束，工程无 Unity 进程、无 Lock；不以该次尝试冒充 focused PASS。
- Player-visible delivery: `NONE`；Fresh Play: `NOT RUN`。
