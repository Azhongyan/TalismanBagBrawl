# Enemy Build Mapping Report

Package: `V0.3/V0.4-BuildSandbox-EnemyBossValidationPool01`
Data label: `devOnly / disabled / not formal flow`

| enemyId | 中文定位 | enemyType | 验证目标 Build | 验证标签 | 推荐羁绊 | 对应模拟场景 | 模拟器可读取 | 进入正式流程 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `dev_enemy_basic` | 普通怪 | `basic` | `baseline_build` | `basic;baseline` | 惊雷;离火 | `pool_enemy_dev_enemy_basic` | `True` | `False` |
| `dev_enemy_shield_guard` | 护盾怪 | `shield_guard` | `jing_lei_break;hu_zhen_guard` | `shield;break_test` | 惊雷;护阵 | `pool_enemy_dev_enemy_shield_guard` | `True` | `False` |
| `dev_enemy_poison_cultist` | 毒怪 | `poison` | `jing_e_cleanse` | `poison;negative_status` | 净厄 | `pool_enemy_dev_enemy_poison_cultist` | `True` | `False` |
| `dev_enemy_burning_wisp` | 燃烧怪 | `burning` | `li_huo_damage` | `burning;dot_pressure` | 离火;净厄 | `pool_enemy_dev_enemy_burning_wisp` | `True` | `False` |
| `dev_enemy_spirit_thief` | 偷灵怪 | `spirit_thief` | `ju_neng_energy` | `energy_drain;resource_disrupt` | 聚能 | `pool_enemy_dev_enemy_spirit_thief` | `True` | `False` |
| `dev_enemy_seal_locker` | 封符怪 | `seal_lock` | `zhen_hun_control;jing_e_cleanse` | `seal_lock;control` | 镇魂;净厄 | `pool_enemy_dev_enemy_seal_locker` | `True` | `False` |
| `dev_enemy_swarm_pack` | 群怪 | `swarm` | `li_huo_aoe` | `swarm;multi_target` | 离火 | `pool_enemy_dev_enemy_swarm_pack` | `True` | `False` |
| `dev_enemy_burst_assassin` | 高爆发怪 | `burst` | `hu_zhen_survive` | `burst;low_hp_pressure` | 护阵 | `pool_enemy_dev_enemy_burst_assassin` | `True` | `False` |
| `dev_enemy_caster_chanter` | 施法怪 | `caster` | `zhen_hun_interrupt` | `caster;skill_cast` | 镇魂 | `pool_enemy_dev_enemy_caster_chanter` | `True` | `False` |
| `dev_enemy_thick_blood` | 厚血怪 | `high_hp` | `li_huo_damage;jing_lei_break` | `high_hp;long_fight` | 离火;惊雷 | `pool_enemy_dev_enemy_thick_blood` | `True` | `False` |
| `dev_enemy_formation_eye_jammer` | 阵眼干扰怪 | `formation_eye_disrupt` | `ju_neng_energy;hu_zhen_guard` | `formation_eye;placement_disrupt` | 聚能;护阵 | `pool_enemy_dev_enemy_formation_eye_jammer` | `True` | `False` |
