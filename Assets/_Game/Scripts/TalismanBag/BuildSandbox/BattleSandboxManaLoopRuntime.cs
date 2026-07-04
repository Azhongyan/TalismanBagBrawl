using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleSandboxManaLoopPreview
    {
        public const string PackageName = "V0.4-BattleSandboxManaLoopRuntime01";
        public const string DeveloperDataPanelFieldKey = "manaLoopRuntimePreview";

        public bool devOnly = true;
        public bool isEnabled;
        public bool readsBuildSandboxItemStat = true;
        public bool readsFormalV02V03Combat;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool writesFormalBossProgress;
        public bool writesFormalChapterProgress;
        public bool touchesFormalSceneUiLayout;
        public int rectTransformLayoutWriteCount;
        public int maxMana;
        public int initialMana;
        public int finalMana;
        public int generatedManaTotal;
        public int spentManaTotal;
        public int sourceItemStatProfileCount;
        public List<BattleSandboxManaLoopPreviewRow> rows = new();

        public int ManaGainRowCount => CountRows("gain");
        public int ManaSpendRowCount => CountRows("spend");
        public int ManaWaitRowCount => CountRows("wait");
        public int PlayerSideAnswerLeakCount => CountRows(row => row != null && row.playerSideAnswerLeak);
        public int UiLayoutWriteCount => rectTransformLayoutWriteCount + CountRows(row => row != null && row.uiLayoutWrite);

        public int FormalLeakCount =>
            (readsFormalV02V03Combat ? 1 : 0)
            + (writesFormalSaveData ? 1 : 0)
            + (writesFormalReward ? 1 : 0)
            + (writesFormalBossProgress ? 1 : 0)
            + (writesFormalChapterProgress ? 1 : 0)
            + CountRows(row => row != null && row.formalFlowLeak);

        public int FeatureFlagDefaultTrueCount
        {
            get
            {
                int count = 0;
                foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
                {
                    if (flag.DefaultValue)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public bool DevOnlyIsolationPass =>
            devOnly
            && !isEnabled
            && readsBuildSandboxItemStat
            && FormalLeakCount == 0
            && PlayerSideAnswerLeakCount == 0
            && FeatureFlagDefaultTrueCount == 0
            && UiLayoutWriteCount == 0
            && !touchesFormalSceneUiLayout;

        private int CountRows(string rowKind)
        {
            return CountRows(row => row != null && string.Equals(row.rowKind, rowKind, StringComparison.Ordinal));
        }

        private int CountRows(Func<BattleSandboxManaLoopPreviewRow, bool> predicate)
        {
            int count = 0;
            foreach (BattleSandboxManaLoopPreviewRow row in rows ?? new List<BattleSandboxManaLoopPreviewRow>())
            {
                if (predicate(row))
                {
                    count++;
                }
            }

            return count;
        }
    }

    [Serializable]
    public sealed class BattleSandboxManaLoopPreviewRow
    {
        public string rowId = string.Empty;
        public string rowKind = string.Empty;
        public string itemId = string.Empty;
        public string statProfileId = string.Empty;
        public int manaDelta;
        public int currentManaAfter;
        public int maxMana;
        public string playerHudTextChinese = string.Empty;
        public string floatingTextChinese = string.Empty;
        public string sourceDataPath = "BuildSandboxLayoutSnapshot.placedItems[].itemStat";
        public string developerDataPanelFieldKey = BattleSandboxManaLoopPreview.DeveloperDataPanelFieldKey;
        public bool readsBuildSandboxItemStat = true;
        public bool readsFormalCombat;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool writesFormalBossProgress;
        public bool writesFormalChapterProgress;
        public bool uiLayoutWrite;

        public bool formalFlowLeak =>
            readsFormalCombat
            || writesFormalSaveData
            || writesFormalReward
            || writesFormalBossProgress
            || writesFormalChapterProgress;

        public bool playerSideAnswerLeak;
    }

    public static class BattleSandboxManaLoopPreviewBuilder
    {
        public const string PackageName = BattleSandboxManaLoopPreview.PackageName;
        private const int PreviewStepCount = 8;

        public static BattleSandboxManaLoopPreview BuildDefaultPreview()
        {
            return Build(BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot());
        }

        public static BattleSandboxManaLoopPreview Build(BuildSandboxLayoutSnapshot snapshot)
        {
            BuildSandboxLayoutSnapshot safeSnapshot = snapshot ?? new BuildSandboxLayoutSnapshot();
            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(safeSnapshot);
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems =
                safeSnapshot.placedItems ?? new List<BuildSandboxPlacedItemSnapshot>();

            BattleSandboxManaLoopPreview preview = new()
            {
                maxMana = CalculateMaxMana(placedItems),
                initialMana = CalculateInitialMana(placedItems),
                sourceItemStatProfileCount = CountItemStatProfiles(placedItems)
            };

            int currentMana = preview.initialMana;
            int spendIndex = 0;
            for (int step = 0; step < PreviewStepCount; step++)
            {
                int gain = CalculateManaGain(placedItems);
                currentMana = Mathf.Clamp(currentMana + gain, 0, preview.maxMana);
                preview.generatedManaTotal += gain;
                preview.rows.Add(CreateGainRow(step, gain, currentMana, preview.maxMana));

                BuildSandboxPlacedItemSnapshot spendItem = FindNextSpendItem(
                    placedItems,
                    ref spendIndex,
                    out int manaCost);
                if (spendItem == null || manaCost <= 0)
                {
                    continue;
                }

                if (currentMana >= manaCost)
                {
                    currentMana -= manaCost;
                    preview.spentManaTotal += manaCost;
                    preview.rows.Add(CreateSpendRow(step, spendItem, manaCost, currentMana, preview.maxMana));
                }
                else
                {
                    preview.rows.Add(CreateWaitRow(step, spendItem, manaCost, currentMana, preview.maxMana));
                }
            }

            preview.finalMana = currentMana;
            return preview;
        }

        public static int CalculateMaxMana(IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            int spirit = 0;
            int count = 0;
            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
                spirit += Mathf.Max(0, stat.spirit);
                count++;
            }

            return Mathf.Max(40, 36 + spirit * 4 + count * 2);
        }

        public static int CalculateInitialMana(IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            int spirit = 0;
            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
                spirit += Mathf.Max(0, stat.spirit);
            }

            return Mathf.Clamp(spirit, 0, CalculateMaxMana(placedItems));
        }

        public static int CalculateManaGain(IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            int gain = 0;
            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
                gain += Mathf.Max(0, stat.manaGainPerTick);
                if (item?.isPowered == true)
                {
                    gain += Mathf.Max(0, stat.spirit / 3);
                }
            }

            return gain;
        }

        public static int CalculateManaCost(BuildSandboxPlacedItemSnapshot item)
        {
            BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
            return Mathf.Max(0, stat.manaCostPerCast);
        }

        public static int CountItemStatProfiles(IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems)
        {
            int count = 0;
            foreach (BuildSandboxPlacedItemSnapshot item in placedItems ?? Array.Empty<BuildSandboxPlacedItemSnapshot>())
            {
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
                if (!string.IsNullOrWhiteSpace(stat.statProfileId))
                {
                    count++;
                }
            }

            return count;
        }

        public static BuildSandboxPlacedItemSnapshot FindNextSpendItem(
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> placedItems,
            ref int spendIndex,
            out int manaCost)
        {
            manaCost = 0;
            if (placedItems == null || placedItems.Count == 0)
            {
                return null;
            }

            for (int scanned = 0; scanned < placedItems.Count; scanned++)
            {
                int index = Mathf.Abs(spendIndex + scanned) % placedItems.Count;
                BuildSandboxPlacedItemSnapshot item = placedItems[index];
                int cost = CalculateManaCost(item);
                if (cost <= 0)
                {
                    continue;
                }

                spendIndex = index + 1;
                manaCost = cost;
                return item;
            }

            return null;
        }

        private static BattleSandboxManaLoopPreviewRow CreateGainRow(
            int step,
            int gain,
            int currentMana,
            int maxMana)
        {
            return new BattleSandboxManaLoopPreviewRow
            {
                rowId = $"manaLoop.gain.{step:00}",
                rowKind = "gain",
                manaDelta = gain,
                currentManaAfter = currentMana,
                maxMana = maxMana,
                playerHudTextChinese = FormatHud(currentMana, maxMana),
                floatingTextChinese = $"\u7075\u6c14 +{gain}",
                sourceDataPath = "BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick"
            };
        }

        private static BattleSandboxManaLoopPreviewRow CreateSpendRow(
            int step,
            BuildSandboxPlacedItemSnapshot item,
            int manaCost,
            int currentMana,
            int maxMana)
        {
            BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
            return new BattleSandboxManaLoopPreviewRow
            {
                rowId = $"manaLoop.spend.{step:00}",
                rowKind = "spend",
                itemId = item?.itemId ?? string.Empty,
                statProfileId = stat.statProfileId,
                manaDelta = -manaCost,
                currentManaAfter = currentMana,
                maxMana = maxMana,
                playerHudTextChinese = FormatHud(currentMana, maxMana),
                floatingTextChinese = $"\u7075\u6c14 -{manaCost}",
                sourceDataPath = "BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast"
            };
        }

        private static BattleSandboxManaLoopPreviewRow CreateWaitRow(
            int step,
            BuildSandboxPlacedItemSnapshot item,
            int manaCost,
            int currentMana,
            int maxMana)
        {
            BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
            return new BattleSandboxManaLoopPreviewRow
            {
                rowId = $"manaLoop.wait.{step:00}",
                rowKind = "wait",
                itemId = item?.itemId ?? string.Empty,
                statProfileId = stat.statProfileId,
                manaDelta = 0,
                currentManaAfter = currentMana,
                maxMana = maxMana,
                playerHudTextChinese = FormatHud(currentMana, maxMana),
                floatingTextChinese = $"\u7075\u529b\u4e0d\u8db3 {currentMana}/{manaCost}",
                sourceDataPath = "BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast"
            };
        }

        public static string FormatHud(int currentMana, int maxMana)
        {
            return $"\u7075\u529b {currentMana}/{Mathf.Max(1, maxMana)}";
        }
    }

    public sealed class BattleSandboxManaLoopRuntime : MonoBehaviour
    {
        public const string PackageName = BattleSandboxManaLoopPreview.PackageName;

        private const float ManaGainTickSeconds = 1f;
        private const float FallbackManaSpendTickSeconds = 1.45f;
        private const string ManaTextName = "ManaText";
        private const string PlayerManaBarName = "PlayerManaBar";
        private const string ManaBarFillName = "Fill";
        private const string FloatingRootName = "EnemyCombatFeedbackFloatingRoot";
        private const string ManaGeneratedAnchorName = "ManaGeneratedAnchor";
        private const string ManaSpentAnchorName = "ManaSpentAnchor";

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool readsBuildSandboxItemStat = true;
        [SerializeField] private bool readsFormalV02V03Combat;
        [SerializeField] private bool writesFormalSaveData;
        [SerializeField] private bool writesFormalReward;
        [SerializeField] private bool writesFormalBossProgress;
        [SerializeField] private bool writesFormalChapterProgress;
        [SerializeField] private bool touchesFormalSceneUiLayout;

        [SerializeField] private Text manaText;
        [SerializeField] private Image manaFillImage;
        [SerializeField] private RectTransform floatingRoot;
        [SerializeField] private RectTransform manaGeneratedAnchor;
        [SerializeField] private RectTransform manaSpentAnchor;

        private readonly List<BuildSandboxPlacedItemSnapshot> activeItems = new();
        private BuildGridInteractionPreviewController gridController;
        private float gainTimer;
        private float spendTimer;
        private int spendIndex;
        private int currentMana;
        private int maxMana = 40;
        private bool wasBattleActive;
        private int generatedManaTotal;
        private int spentManaTotal;
        private int loopTickCount;
        private string lastStatus = "\u6c99\u76d2\u5f85\u673a";

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool ReadsBuildSandboxItemStat => readsBuildSandboxItemStat;
        public bool ReadsFormalV02V03Combat => readsFormalV02V03Combat;
        public bool WritesFormalSaveData => writesFormalSaveData;
        public bool WritesFormalReward => writesFormalReward;
        public bool WritesFormalBossProgress => writesFormalBossProgress;
        public bool WritesFormalChapterProgress => writesFormalChapterProgress;
        public bool TouchesFormalSceneUiLayout => touchesFormalSceneUiLayout;
        public int CurrentMana => currentMana;
        public int MaxMana => maxMana;
        public int GeneratedManaTotal => generatedManaTotal;
        public int SpentManaTotal => spentManaTotal;
        public int LoopTickCount => loopTickCount;
        public int ActiveItemCount => activeItems.Count;

        public void Bind(BuildGridInteractionPreviewController controller)
        {
            gridController = controller;
            ResolveUiReferences();
            RefreshSnapshot(resetMana: false);
            UpdateHud(active: gridController != null && gridController.IsSandboxBattleModeActive);
        }

        private void Awake()
        {
            ResolveUiReferences();
        }

        private void OnEnable()
        {
            ResolveUiReferences();
            UpdateHud(active: false);
        }

        private void Update()
        {
            bool battleActive = gridController != null && gridController.IsSandboxBattleModeActive;
            if (!battleActive)
            {
                wasBattleActive = false;
                UpdateHud(active: false);
                return;
            }

            if (!wasBattleActive)
            {
                StartLoop();
            }

            wasBattleActive = true;
            TickLoop(Time.deltaTime);
        }

        public void ResetLoop()
        {
            generatedManaTotal = 0;
            spentManaTotal = 0;
            loopTickCount = 0;
            spendIndex = 0;
            gainTimer = 0f;
            spendTimer = 0f;
            RefreshSnapshot(resetMana: true);
            lastStatus = "\u6c99\u76d2\u5f85\u673a";
            UpdateHud(active: false);
        }

        private void StartLoop()
        {
            ResolveUiReferences();
            RefreshSnapshot(resetMana: true);
            generatedManaTotal = 0;
            spentManaTotal = 0;
            loopTickCount = 0;
            spendIndex = 0;
            gainTimer = 0f;
            spendTimer = 0f;
            lastStatus = "\u7075\u529b\u56de\u8def\u542f\u52a8";
            UpdateHud(active: true);
        }

        private void TickLoop(float deltaTime)
        {
            if (activeItems.Count == 0)
            {
                RefreshSnapshot(resetMana: false);
            }

            gainTimer += deltaTime;
            spendTimer += deltaTime;

            if (gainTimer >= ManaGainTickSeconds)
            {
                gainTimer -= ManaGainTickSeconds;
                GenerateMana();
            }

            float spendTickSeconds = ResolveSpendTickSeconds();
            if (spendTimer >= spendTickSeconds)
            {
                spendTimer -= spendTickSeconds;
                SpendMana();
            }

            UpdateHud(active: true);
        }

        private void RefreshSnapshot(bool resetMana)
        {
            activeItems.Clear();
            BuildSandboxLayoutSnapshot snapshot = gridController == null
                ? BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot()
                : gridController.BuildCurrentLayoutSnapshot();

            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            foreach (BuildSandboxPlacedItemSnapshot item in snapshot?.placedItems ?? new List<BuildSandboxPlacedItemSnapshot>())
            {
                if (item?.itemStat == null)
                {
                    continue;
                }

                item.itemStat = BuildSandboxItemStatCatalog.ResolveFrom(item.itemStat, item.itemId);
                activeItems.Add(item);
            }

            maxMana = BattleSandboxManaLoopPreviewBuilder.CalculateMaxMana(activeItems);
            if (resetMana)
            {
                currentMana = BattleSandboxManaLoopPreviewBuilder.CalculateInitialMana(activeItems);
            }
            else
            {
                currentMana = Mathf.Clamp(currentMana, 0, maxMana);
            }
        }

        private void GenerateMana()
        {
            int gain = BattleSandboxManaLoopPreviewBuilder.CalculateManaGain(activeItems);
            if (gain <= 0)
            {
                lastStatus = "\u65e0\u7075\u529b\u6765\u6e90";
                return;
            }

            int before = currentMana;
            currentMana = Mathf.Clamp(currentMana + gain, 0, maxMana);
            int actualGain = currentMana - before;
            generatedManaTotal += Mathf.Max(0, actualGain);
            loopTickCount++;
            lastStatus = actualGain > 0 ? $"\u4ea7\u7075 +{actualGain}" : "\u7075\u529b\u5df2\u6ee1";
            if (actualGain > 0)
            {
                SpawnFloatingText(manaGeneratedAnchor, $"\u7075\u6c14 +{actualGain}", new Color(0.72f, 0.98f, 1f, 1f));
            }
        }

        private void SpendMana()
        {
            BuildSandboxPlacedItemSnapshot spendItem =
                BattleSandboxManaLoopPreviewBuilder.FindNextSpendItem(activeItems, ref spendIndex, out int manaCost);
            if (spendItem == null || manaCost <= 0)
            {
                lastStatus = "\u6682\u65e0\u8017\u7075\u9879";
                return;
            }

            if (currentMana < manaCost)
            {
                lastStatus = $"\u7075\u529b\u4e0d\u8db3 {currentMana}/{manaCost}";
                SpawnFloatingText(manaSpentAnchor, "\u7075\u529b\u4e0d\u8db3", new Color(1f, 0.74f, 0.42f, 1f));
                return;
            }

            currentMana -= manaCost;
            spentManaTotal += manaCost;
            loopTickCount++;
            lastStatus = $"\u8017\u7075 -{manaCost}";
            SpawnFloatingText(manaSpentAnchor, $"\u7075\u6c14 -{manaCost}", new Color(0.88f, 0.56f, 1f, 1f));
        }

        private float ResolveSpendTickSeconds()
        {
            float bestInterval = float.MaxValue;
            foreach (BuildSandboxPlacedItemSnapshot item in activeItems)
            {
                BuildSandboxItemStat stat = BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
                if (stat.manaCostPerCast <= 0)
                {
                    continue;
                }

                if (float.IsNaN(stat.castIntervalSeconds) || float.IsInfinity(stat.castIntervalSeconds))
                {
                    continue;
                }

                bestInterval = Mathf.Min(bestInterval, Mathf.Max(0.5f, stat.castIntervalSeconds));
            }

            return bestInterval == float.MaxValue ? FallbackManaSpendTickSeconds : bestInterval;
        }

        private void UpdateHud(bool active)
        {
            if (manaText != null)
            {
                string state = active ? lastStatus : "\u6c99\u76d2\u5f85\u673a";
                manaText.text = $"{BattleSandboxManaLoopPreviewBuilder.FormatHud(currentMana, maxMana)}  {state}";
            }

            if (manaFillImage != null)
            {
                manaFillImage.fillAmount = maxMana <= 0 ? 0f : Mathf.Clamp01(currentMana / (float)maxMana);
            }
        }

        private void SpawnFloatingText(RectTransform anchor, string text, Color color)
        {
            if (floatingRoot == null || anchor == null)
            {
                return;
            }

            GameObject textObject = new("ManaLoopFloatingText_Runtime", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(CanvasGroup));
            textObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            textObject.transform.SetParent(floatingRoot, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(160f, 32f);
            rect.anchoredPosition = ResolveFloatingPosition(anchor);

            Text label = textObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.color = color;
            label.text = text;

            CanvasGroup group = textObject.GetComponent<CanvasGroup>();
            StartCoroutine(AnimateFloatingText(textObject, rect, group));
        }

        private Vector2 ResolveFloatingPosition(RectTransform anchor)
        {
            if (anchor == null)
            {
                return Vector2.zero;
            }

            return anchor.parent == floatingRoot
                ? anchor.anchoredPosition
                : floatingRoot.InverseTransformPoint(anchor.position);
        }

        private IEnumerator AnimateFloatingText(GameObject textObject, RectTransform rect, CanvasGroup group)
        {
            const float duration = 0.9f;
            Vector2 start = rect.anchoredPosition;
            Vector2 end = start + new Vector2(0f, 42f);
            float elapsed = 0f;
            while (elapsed < duration && textObject != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rect.anchoredPosition = Vector2.Lerp(start, end, t);
                group.alpha = 1f - t;
                yield return null;
            }

            if (textObject != null)
            {
                Destroy(textObject);
            }
        }

        private void ResolveUiReferences()
        {
            if (manaText == null)
            {
                manaText = FindComponentByName<Text>(ManaTextName);
            }

            if (manaFillImage == null)
            {
                RectTransform manaBar = FindRectTransform(PlayerManaBarName);
                manaFillImage = FindChildComponentByName<Image>(manaBar, ManaBarFillName);
            }

            if (floatingRoot == null)
            {
                floatingRoot = FindRectTransform(FloatingRootName);
            }

            if (manaGeneratedAnchor == null)
            {
                manaGeneratedAnchor = FindRectTransform(ManaGeneratedAnchorName);
            }

            if (manaSpentAnchor == null)
            {
                manaSpentAnchor = FindRectTransform(ManaSpentAnchorName);
            }
        }

        private static T FindComponentByName<T>(string objectName)
            where T : Component
        {
            RectTransform rect = FindRectTransform(objectName);
            return rect == null ? null : rect.GetComponent<T>();
        }

        private static T FindChildComponentByName<T>(Transform parent, string childName)
            where T : Component
        {
            if (parent == null)
            {
                return null;
            }

            foreach (T component in parent.GetComponentsInChildren<T>(true))
            {
                if (component != null && string.Equals(component.name, childName, StringComparison.Ordinal))
                {
                    return component;
                }
            }

            return null;
        }

        private static RectTransform FindRectTransform(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            foreach (Transform transform in FindObjectsOfType<Transform>(true))
            {
                if (string.Equals(transform.name, objectName, StringComparison.Ordinal))
                {
                    return transform as RectTransform;
                }
            }

            return null;
        }
    }
}
