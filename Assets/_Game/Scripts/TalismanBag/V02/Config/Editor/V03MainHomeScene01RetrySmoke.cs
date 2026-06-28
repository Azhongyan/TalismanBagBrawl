#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class V03MainHomeScene01RetrySmoke
    {
        private const string MainHomeSourcePath =
            "Assets/_Game/Scripts/TalismanBag/V02/UI/MainHomeGreyboxPanel.cs";
        private const string HotspotConfigSourcePath =
            "Assets/_Game/Scripts/TalismanBag/V02/UI/HomeHotspotConfig.cs";
        [MenuItem("Tools/Talisman Bag/V0.3/Run Main Home Retry Smoke")]
        public static void VerifyBatch()
        {
            VerifyHotspotConfiguration();
            VerifyHomePanelContract();
            VerifyRuntimeGreybox();
            VerifySourceBoundaries();
            Task09_10QaSmoke.VerifyBatch();

            Debug.Log(
                "[V0.3-MainHomeScene01-Retry] SMOKE_SUCCESS " +
                "home=照灯小铺, hotspots=12, bottomNavOwnsTrialAndCultivate=true, " +
                "comingSoon=ok, fallback=ok, " +
                "sceneVerification=delegatedToFix02, reflectionActivation=false, " +
                "v02GoldenPathConfig=ok, DataCatalogErrors=0");
        }

        private static void VerifyHotspotConfiguration()
        {
            List<HomeHotspotConfig> configs = HomeHotspotConfig.CreateDefaultSet();
            Require(configs.Count == 12, "Home hotspot default set must contain 12 entries.");
            Require(configs.All(config => config != null), "Home hotspot config must not contain null rows.");
            Require(configs.All(config => HomeHotspotConfig.IsAllowed(config.hotspotId)),
                "Home hotspot config contains an id outside the whitelist.");
            Require(configs.Select(config => config.hotspotId).Distinct().Count() == configs.Count,
                "Home hotspot ids must be unique.");

            Require(configs.Any(config =>
                    config.hotspotId == HomeHotspotId.Ledger &&
                    config.state == HomeHotspotState.Available),
                "Ledger hotspot must be available.");
            Require(configs.Any(config =>
                    config.hotspotId == HomeHotspotId.CodexBook &&
                    config.targetType == HomeHotspotTargetType.Collection),
                "CodexBook hotspot must use the collection target.");
            Require(configs.Any(config =>
                    config.hotspotId == HomeHotspotId.ClueBook &&
                    config.state == HomeHotspotState.Locked),
                "ClueBook hotspot must be locked.");
            Require(configs.Any(config =>
                    config.hotspotId == HomeHotspotId.BackRoom &&
                    config.state == HomeHotspotState.ComingSoon),
                "BackRoom must remain ComingSoon.");
            Require(configs.Any(config =>
                    config.hotspotId == HomeHotspotId.Xiaoman &&
                    config.targetType == HomeHotspotTargetType.CharacterPrompt),
                "Xiaoman hotspot must use the character prompt target.");
            Require(configs.Any(config =>
                    config.hotspotId == HomeHotspotId.StreetEntrance &&
                    config.state == HomeHotspotState.ComingSoon),
                "StreetEntrance must remain ComingSoon.");
            Require(!configs.Any(config =>
                    config.hotspotId is HomeHotspotId.Trial or HomeHotspotId.Refine or HomeHotspotId.Explore),
                "Trial, Refine, and Explore must be owned by the bottom navigation, not home hotspots.");

            string[] requiredFields =
            {
                "hotspotId",
                "displayName",
                "state",
                "targetType",
                "visualKey",
                "iconKey",
                "lockedReason",
                "comingSoonText"
            };
            foreach (string fieldName in requiredFields)
            {
                Require(typeof(HomeHotspotConfig).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public) != null,
                    $"HomeHotspotConfig is missing field '{fieldName}'.");
            }
        }

        private static void VerifyHomePanelContract()
        {
            Require(MainHomeGreyboxPanel.HomeTitle == "照灯小铺", "Home title must be 照灯小铺.");

            MethodInfo showMethod = typeof(MainHomeGreyboxPanel).GetMethod(
                "Show",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new[]
                {
                    typeof(string),
                    typeof(string),
                    typeof(string),
                    typeof(Action),
                    typeof(Action),
                    typeof(Action)
                },
                null);
            Require(showMethod != null, "MainHomeGreyboxPanel must preserve the existing Show delegate contract.");
            Require(typeof(MainHomeGreyboxPanel).GetMethod("ShowComplete", BindingFlags.Instance | BindingFlags.Public) != null,
                "MainHomeGreyboxPanel must preserve ShowComplete.");
            Require(typeof(MainHomeGreyboxPanel).GetMethod("ApplyV03MainHomeUiueLayout", BindingFlags.Instance | BindingFlags.Public) != null,
                "MainHomeGreyboxPanel must expose the V03 UIUE layout hook.");
        }

        private static void VerifyRuntimeGreybox()
        {
            SaveService preexistingSaveService = SaveService.Instance;
            GameObject canvasObject = null;
            try
            {
                canvasObject = new GameObject("V03MainHomeSmokeCanvas", typeof(RectTransform), typeof(Canvas));
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                MainHomeGreyboxPanel panel = MainHomeGreyboxPanel.CreateRuntime(canvas.transform);
                Require(panel != null, "Runtime home panel could not be created.");
                panel.ApplyV03MainHomeUiueLayout();

                panel.Show(
                    "Legacy title must not win",
                    "当前资源\n- 灵石 10\n- 符纸 5\n- 朱砂 2\n- 初阶符胚 1\n- 修为 3",
                    "Smoke status",
                    () => { },
                    () => { },
                    null);

                Text[] texts = panel.GetComponentsInChildren<Text>(true);
                Require(texts.Any(text => text.text == "照灯小铺"), "Runtime home panel does not display 照灯小铺.");
                Require(texts.Any(text => text.text == "照灯账本"), "Runtime home panel does not display 照灯账本.");
                Require(texts.Any(text => text.text == "道藏典册"), "Runtime home panel does not display 道藏典册.");
                Require(texts.Any(text => text.text == "旧物线索簿"), "Runtime home panel does not display 旧物线索簿.");
                Require(texts.Any(text => text.text == "青石坊街口"), "Runtime home panel does not display 青石坊街口.");
                Require(!texts.Any(text => text.text.Contains("BattleBackpack")), "Home panel exposes a forbidden battle-backpack label.");
                Require(!texts.Any(text => text.text == "论道"), "Home panel exposes a forbidden PVP label.");

                HomeHotspotView[] views = panel.GetComponentsInChildren<HomeHotspotView>(true);
                Require(views.Any(view => view.Config?.hotspotId == HomeHotspotId.Ledger), "Runtime Ledger hotspot is missing.");
                Require(views.Any(view => view.Config?.hotspotId == HomeHotspotId.Xiaoman), "Runtime Xiaoman hotspot is missing.");
                Require(!views.Any(view => view.Config?.hotspotId == HomeHotspotId.Trial),
                    "Runtime Trial must not be a home hotspot.");
                Require(!views.Any(view => view.Config?.hotspotId == HomeHotspotId.Refine),
                    "Runtime Refine must not be a home hotspot.");

                Click(views, HomeHotspotId.Ledger);
                Click(views, HomeHotspotId.Xiaoman);
                Click(views, HomeHotspotId.BackRoom);

                HomeHotspotView fallbackView = HomeHotspotView.CreateRuntime(
                    canvas.transform,
                    Vector2.zero,
                    new Vector2(180f, 100f));
                fallbackView.Bind(
                    new HomeHotspotConfig(
                        HomeHotspotId.Notice,
                        "Fallback Smoke",
                        HomeHotspotState.Available,
                        HomeHotspotTargetType.SystemShortcut,
                        "V03/DefinitelyMissingVisual",
                        "V03/DefinitelyMissingIcon"),
                    null);
                Require(fallbackView.gameObject.activeSelf, "Missing visual keys disabled the fallback hotspot.");

                HomeHotspotView hiddenView = HomeHotspotView.CreateRuntime(
                    canvas.transform,
                    Vector2.zero,
                    new Vector2(180f, 100f));
                hiddenView.Bind(
                    new HomeHotspotConfig(
                        HomeHotspotId.Settings,
                        "Hidden Smoke",
                        HomeHotspotState.Hidden,
                        HomeHotspotTargetType.SystemShortcut),
                    null);
                Require(!hiddenView.gameObject.activeSelf, "Hidden hotspot must not be displayed.");
            }
            finally
            {
                if (canvasObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(canvasObject);
                }

                if (preexistingSaveService == null && SaveService.Instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(SaveService.Instance.gameObject);
                }
            }
        }

        private static void VerifySourceBoundaries()
        {
            string homeSource = File.ReadAllText(MainHomeSourcePath);
            string hotspotSource = File.ReadAllText(HotspotConfigSourcePath);

            string[] forbiddenCalls =
            {
                "StartCombat(",
                "ResetMainTrialFromBeginning(",
                "ResetSave(",
                "UpgradeService",
                "RewardService",
                "FormationState",
                "V02FormationStateSnapshot"
            };
            foreach (string forbiddenCall in forbiddenCalls)
            {
                Require(!homeSource.Contains(forbiddenCall, StringComparison.Ordinal),
                    $"Main home source contains forbidden core call '{forbiddenCall}'.");
            }

            Require(!homeSource.Contains("BattleBackpack", StringComparison.Ordinal),
                "Main home source contains a forbidden battle-backpack entry.");
            Require(!hotspotSource.Contains("BattleBackpack", StringComparison.Ordinal),
                "Hotspot config contains a forbidden battle-backpack entry.");
            Require(!hotspotSource.Contains("\"论道\"", StringComparison.Ordinal),
                "Hotspot config contains a forbidden PVP name.");
        }

        private static void Click(IReadOnlyList<HomeHotspotView> views, HomeHotspotId hotspotId)
        {
            HomeHotspotView view = views.FirstOrDefault(candidate => candidate.Config?.hotspotId == hotspotId);
            Require(view != null, $"Runtime hotspot {hotspotId} is missing.");
            Button button = view.GetComponent<Button>();
            Require(button != null, $"Runtime hotspot {hotspotId} has no Button.");
            button.onClick.Invoke();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
#endif
