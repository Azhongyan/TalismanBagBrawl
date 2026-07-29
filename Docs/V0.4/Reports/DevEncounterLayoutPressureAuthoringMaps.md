# Dev Encounter 5×5 压力格图

坐标：x 从左到右 0..4；y 从上到下 0..4。图例：· 可用，污 不可用，基 基准阵眼，效 有效阵眼，同 基准与有效阵眼重合。

## 阵眼压力 / 4-10 炉心灰落

- PressureInputId: `pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1`
- Context: `dev_seed_4_10_furnace_core / dev_encounter_4_10_furnace_core / dev_map_furnace_ash_fall`
- 有效阵眼：`2:1`
- 保留连接：`35` 条
- 压力说明：阵眼移至 (2,1)；y=1 与 y=2 之间的 5 条正交边未保留。

```text
    x=0 x=1 x=2 x=3 x=4
y=0  ·   ·   ·   ·   ·
y=1  ·   ·   效   ·   ·
y=2  ·   ·   基   ·   ·
y=3  ·   ·   ·   ·   ·
y=4  ·   ·   ·   ·   ·
```

## 阵眼压力 / 4-10 雷火交叉·青石裂隙

- PressureInputId: `pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1`
- Context: `dev_seed_4_10_thunder_fire_cross / dev_encounter_4_10_thunder_fire_cross / dev_map_bluestone_crack`
- 有效阵眼：`3:2`
- 保留连接：`35` 条
- 压力说明：阵眼移至 (3,2)；x=2 与 x=3 之间的 5 条正交边未保留。

```text
    x=0 x=1 x=2 x=3 x=4
y=0  ·   ·   ·   ·   ·
y=1  ·   ·   ·   ·   ·
y=2  ·   ·   基   效   ·
y=3  ·   ·   ·   ·   ·
y=4  ·   ·   ·   ·   ·
```

## 污染格压力 / 3-10 净化角·青石潮湿

- PressureInputId: `pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1`
- Context: `dev_seed_3_10_cleanse_corner / dev_encounter_3_10_cleanse_corner / dev_map_bluestone_damp`
- 有效阵眼：`2:2`
- 保留连接：`32` 条
- 压力说明：污染格为 (1,1)、(3,3)。

```text
    x=0 x=1 x=2 x=3 x=4
y=0  ·   ·   ·   ·   ·
y=1  ·   污   ·   ·   ·
y=2  ·   ·   同   ·   ·
y=3  ·   ·   ·   污   ·
y=4  ·   ·   ·   ·   ·
```

## 污染格压力 / 4-10 炉心灰落

- PressureInputId: `pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1`
- Context: `dev_seed_4_10_furnace_core / dev_encounter_4_10_furnace_core / dev_map_furnace_ash_fall`
- 有效阵眼：`2:2`
- 保留连接：`24` 条
- 压力说明：污染格为 (1,1)、(3,1)、(1,3)、(3,3)。

```text
    x=0 x=1 x=2 x=3 x=4
y=0  ·   ·   ·   ·   ·
y=1  ·   污   ·   污   ·
y=2  ·   ·   同   ·   ·
y=3  ·   污   ·   污   ·
y=4  ·   ·   ·   ·   ·
```
