# V0.4-C1CampaignLootFormalItemEntitlementAcceptance01 Task Contract

## 1. Outcome

`CAMPAIGN_NORMAL_LV1` 中，一个已经由 Reward/Drop 权威解析并确认的 Pool15 普通白色道具奖励，可以被正式 Item Session 原子接收并加入名册/道具栏一次。相同 claim/dedupe/entitlement 重试是幂等 no-op；新的合法重战掉落仍是新的接收机会。

本包不声明玩家已经在 WorldMap 正常流程中看到掉落、完成重摆或看到后续战斗变化。

## 2. Product Context

- `CAMPAIGN_NORMAL_LV1`

## 3. Task Class

- `COMPLEX_GUARDED_ONCE`

分类理由：单一 Item 状态 Owner，但新增 Reward -> Item immutable contract，并扩展正式 Session 的 entitlement 历史与原子名册变更。

## 3A. User Review Gate

- 是否需要：`NO`
- 判断理由：玩家结果、Pool15、直接随机奖励和幂等语义已由用户明确接受；本包只补缺失的 Item 技术接收层。
- 固定类别 Guard：Item Guard
- `USER_ACCEPTED_SCOPE`：当前 Reward/Drop Guard 直接依赖请求及既有 Pool15 玩家承诺。

## 4. Owners

- Primary Guard：Item Guard
- Development Owner：一个新的 visible / unarchived / direct-local Item 开发任务
- State / Data Owner：`C1FormalItemSessionAuthority`
- Upstream dependency owner：Reward/Drop Guard，仅提供 immutable grant bundle
- Presentation Owner：既有 Item Presenter / Card / Tray / Board；本包只读
- User Decision Point：`NONE`

## 5. Allowed Writes

仅允许以下 5 个产品路径：

1. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs`
2. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalCampaignLootEntitlementAcceptance.cs`（新增）
3. `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalCampaignLootEntitlementAcceptance.cs.meta`（新增）
4. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalCampaignLootEntitlementAcceptanceTests.cs`（新增）
5. `Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1FormalCampaignLootEntitlementAcceptanceTests.cs.meta`（新增）

Task Contract 本身由 Item Guard 冻结，不属于开发写域。

## 6. Forbidden Writes

- Reward pool、RNG、recent-two、claim/dedupe 生成规则和 RewardResult 生产代码
- Item 数值、形状、目录、放置/旋转/拖拽算法及现有 Item 详情/VFX
- Battle、Enemy、Mainline、WorldMap、Stage、Scene、Prefab、UI、VFX、Save、APK
- `C1FormalObtainRebuildBattleLoop.cs`
- `C1CampaignDirectLootSessionResolver.cs`
- `C1CampaignLootEligibleItemPool15Carrier.cs`
- `C1CampaignLootItemInstanceAndEntitlementCarrier.cs`
- `C1FixedFirstClearItemInstanceCarrier.cs`
- ProjectSettings、BuildSettings、Packages、Git/worktree

## 7. Required Locks / Sources

- `AGENTS.md`
- `Docs/LOCKED/ENGINEERING_PROCESS_V2_LOCK.md`
- `Docs/CURRENT/PROJECT_ENGINEERING_STATE_V2.md`
- 本 Task Contract
- `C1FormalItemSessionAndArrangementAuthority.cs`
- `C1FormalItemSessionAndArrangementAuthorityTests.cs`
- `C1CampaignDirectLootSessionResolver.cs`
- `C1CampaignLootEligibleItemPool15Carrier.cs`
- `C1CampaignLootItemInstanceAndEntitlementCarrier.cs`
- Reward immutable contracts and validator directly used by the grant bundle

## 8. Patch Budget

- 建议文件数：5
- 允许新增：4
- 允许修改：1
- 超预算：停止并返回第一条缺失 carrier；不得扩大到 Mainline、Reward 或 Save。

## 9. Required Behavior

1. 新增 immutable `C1FormalCampaignLootEntitlementCommand`，输入精确 Session token/generation/prior signature/command identity 与一个 released `C1CampaignDirectLootGrantBundle`；不得靠 Scene 名、item 名或 sample 数据推断。
2. `C1FormalItemSessionAuthority` 新增一个清晰命名的 Pool15 接收 API。Reward grant、StageClear、DropRequest、DropRollResult、RewardResult、claim、dedupe、Item materialization、Pool15 profile/catalog/signature 必须全链一致。
3. 仅接受普通白色 `I002/I003/I004/I007/I010/I011/I013/I014/I015/I022/I024/I025/I026/I028/I030`。`I001`、`I031`、未知/特殊/故事/皮肤/Boss 身份及非白色 fail closed。
4. 接收成功后保留 upstream `itemInstanceId`、instance canonical、entitlement identity/canonical、claim/dedupe/StageClear/Reward correlation，并将同一实例原子加入正式 roster；Tray placement 继续由既有正式 Item 规则确定。
5. accepted entitlement history 是 Item Session 的 immutable 状态。相同 entitlement/claim/dedupe/grant 的任意重试 accepted + changed=false；同一身份的冲突 payload fail closed；新的合法 replay clear/grant 可生成并接收一个新实例。
6. 任一 malformed/stale/conflicting context、token、generation、prior session signature、grant、claim、dedupe、RewardResult、materialization、pool/profile/catalog signature 或 Tray/roster build 失败，必须保持 roster、placement、tray、entitlement history 与 canonical signature 全部不变。
7. 现有 fixed I002 first-clear `ConsumeEntitlement` 行为、canonical、幂等与测试保持通过；不得被 Pool15 路径替换。
8. Session reset/new authority 清除 Pool15 session-memory entitlement history；本包不实现 durable Save、迁移、容量/溢出产品政策。

## 10. Failure / Edge Cases

- null/缺字段/未知 schema：拒绝，无部分状态。
- commandId 相同 payload 相同：幂等 no-op；commandId 相同 payload 不同：冲突拒绝。
- entitlement、itemInstance、claim、dedupe 任一已存在但 lineage 不同：冲突拒绝。
- 相同合法 grant 使用新 commandId：仍为幂等 no-op。
- 新 replay StageClear/grant：保持独立 identity，可接收。
- 不能进入现有 5x13 Tray：整体拒绝，不做 overflow fallback。

## 11. Delivery Shape

- Config / Profile：只读消费现有 Pool15 carrier
- Runtime：Item-owned immutable command、accepted receipt/history、authority acceptance API
- Editor：聚焦确定性与回归测试
- Scene / Prefab：`NONE`
- Adapter / Bridge：Reward immutable grant -> Item authority 的窄接收层；不拥有 Reward 或 Mainline 状态
- Report：`NONE`

## 12. Verification

- [x] 开发前 Technical Director gate：System + immutable adapter；无 Scene/Prefab/Unity
- [x] 开发前 Milestone Fidelity gate：本包仅覆盖“正式 Item 接收一次”一行
- [ ] 当前源码 `Assembly-CSharp` 独立编译
- [ ] 当前源码 `Assembly-CSharp-Editor` 对新 Runtime ref 独立编译
- [ ] 聚焦确定性测试
- [ ] 现有 formal Item authority 回归测试
- [ ] 5-path whitelist、meta/GUID、编码/空白和受保护哈希检查

聚焦测试至少覆盖：15 个允许身份、fixed I002 回归、重复 command、相同 grant 新 command、冲突 entitlement/item/claim/dedupe、stale token/generation/prior signature、malformed Reward chain、I001/I031/unknown/non-white、连续 5 次接收、合法 replay 第 6 次、原子失败、reset 清除以及 defensive immutable snapshots。

明确不需要：Unity batch、Fresh Play、Scene/Prefab authoring、玩家手测、APK。原因：本包未挂载到正常游戏流程且无序列化资产。

## 13. Process Ownership

- `UNITY_NOT_REQUIRED`
- 不启动、停止、等待或轮询 Unity/Tuanjie/ShaderCompiler/Bee。
- 编译/测试 scratch 必须位于任务自有临时目录，并在终态删除。
- `NO_GIT / NO_WORKTREE`

## 14. Milestone Fidelity

Player Promise：1-1..1-5 合法胜利直接随机获得一个 Item，进入正式名册一次，重摆后改变后续实时战斗，第五次掉落后可立即重战且重战胜利仍有奖励机会。

本包覆盖：

| 能力 | Owner | Carrier | 本包状态 | 证据上限 |
|---|---|---|---|---|
| Reward 产生合法直接掉落 bundle | Reward/Drop | `C1CampaignDirectLootGrantBundle` | 上游已提供/只读 | contract/data |
| 正式 Item 接收一次并保留 lineage | Item | 本包 command + authority history | 本包实现 | compile/deterministic |
| 掉落身份可见并进入 Tray | Mainline + Item presentation | 既有 Card/Tray/Presenter mount | 下游 | 未证明 |
| 重摆改变后续实时战斗 | Battle/Mainline | live Battle integration | 下游 | 未证明 |
| WorldMap 1-1..1-5 + replay | Mainline/Reward | formal route | 下游 | 未证明 |
| normal-path Fresh Play / user acceptance | Mainline | mounted real path | 下游 | 未执行 |

Milestone verdict：`FAITHFUL_PARTIAL`。PlayerVisibleDelivery=`NONE`，MilestoneCompletionEvidence=`NO`。

## 15. Protected Intake Baselines

- Authority：`2A1CA12EF83E1BCC1CAA7FA2CD03C3F16AD50E3E5959AEBAE834298D9A80718E`（唯一允许修改的既有文件）
- Existing authority tests：`BC6CF38F4C1321F0943E2A6F20094863D039C917433681AD508C5C4BA2BF6082`
- Fixed I002 carrier：`EFE734E86E60B54ECEED7A6518EB84CA9407E61E2898BF681EE2E855A8B5E76D`
- Pool15 carrier：`E65C64970F43E3D67ADDF5698D4A1F574CD72ED5CCBC2316EE14916B46B253E3`
- Item materialization carrier：`7AB56A35127A72D3706766C982F5AC2F0E280C6018E8AE377B6B839BE56CC158`
- Direct loot resolver：`16B14CFF4D6A84C964B37A2E502F3883AA27D9685728E37E29A345FE8491FD7F`
- RewardResult：`13A6698BD8D3B15CE21222DF0EDFB66D69E7F3B2BF09F08608AF5CD0E8687CCE`
- Reward validator：`ED60A14BABEF252BE4D9E646EA9AD16712B770F119D67E7BDE2182170522A796`
- Mainline loop：`C6A35BB5DC754CDEB692111EC4179DAB448793ED9D8A587C4FCE4BD4983C5315`
- Card/Board/Tray Prefabs：`C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` / `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` / `6FA5F995433B4433B201613CA7FBA939779F6664E44E19CC02ACA060E4F4D552`

## 16. Completion

- exactly 5 whitelisted paths only;
- required acceptance/idempotency/atomicity behavior and fixed I002 regression pass;
- Runtime/Editor current-source compile pass;
- protected baselines exact except the one allowed Authority file;
- no task-owned process/scratch residue;
- no Unity/Git/worktree;
- terminal cap：`TECHNICAL_PREREQUISITE_COMPLETE / PlayerVisibleDelivery=NONE / MilestoneCompletionEvidence=NO`。

同包产品修正留在同一 visible task。若必须修改 Reward producer、Mainline、Save、Scene/Prefab 或 Item placement/shape 算法，返回 `ARCHITECTURE_REASSESS_REQUIRED`，不得扩大写域。
