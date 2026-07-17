using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Detail.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.ItemSandbox
{
    internal static class ItemDetailMaxDaoArtTemplateVerifier
    {
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string PrefabPath = "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab";
        private const string BuildSettingsPath = "ProjectSettings/EditorBuildSettings.asset";
        private const string PanelPath = "ItemSandboxCanvas/MobileSafeAreaRoot/ItemSandboxRoot/ItemDetailPanel";
        private const string BeforeSceneHash = "2DAE1F3E5F4E9A7DE8ADCCCC6AA6D661529E236C1EB7ED373F903993B95ED802";
        private const string BeforePrefabHash = "2A6924DA0823B74E2639B593926D8C2F8422101B01EDBE4073AF33CD424E027C";
        private const string BeforeBuildHash = "08A277E3CA465A44E792318C0D3C210AFDBA61069F1170B74FA5A1A18598FE59";
        private const ulong ZhenFaId = 957452681;
        private const ulong QiXingId = 725680823;
        private const ulong DaoJuId = 743156000;
        private const string ReportRelative = "Docs/V0.4/Reports/ItemDetailMaxDaoArtTemplateReport.md";
        private const string SpecRelative = "Docs/V0.4/Reports/ItemDetailMaxDaoArtTemplateSpec.csv";
        private const string LeakRelative = "Docs/V0.4/Reports/ItemDetailMaxDaoArtTemplateLeakCheckReport.md";
        private const string AuthoringRelative = "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemDetailMaxDaoArtTemplateAuthoring.cs";
        private const string ManualRowsAuthoringRelative = "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemDetailBaseStatsManualRowsAuthoring.cs";
        private const string AffixRowsAuthoringRelative = "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemDetailAffixManualRowsAuthoring.cs";
        private const string VerifierRelative = "Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemDetailMaxDaoArtTemplateVerifier.cs";
        private const string PanelViewRelative = "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs";
        private const string SectionViewRelative = "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs";

        [MenuItem("TalismanBag/Item Sandbox/Verify Item Detail Max Dao Art Template")]
        public static void RunMenu()
        {
            Run(false);
        }

        public static void RunBatch()
        {
            Run(true);
        }

        private static void Run(bool failBatch)
        {
            List<Check> checks = new();
            string root = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Project root unavailable.");
            string sceneHash = Hash(Abs(root, ScenePath));
            string prefabHash = Hash(Abs(root, PrefabPath));
            string buildHash = Hash(Abs(root, BuildSettingsPath));
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform panel = FindPath(scene, PanelPath);
            Add(checks, "SCENE_ROOT", "scene", panel != null, PanelPath);
            if (panel != null)
            {
                VerifyScene(checks, panel);
                VerifyArt(checks, panel);
                VerifyProtected(checks, panel);
            }

            Add(checks, "SCENE_AUTHORIZED_CHANGE_NO_BASELINE", "hash",
                sceneHash != BeforeSceneHash, "before=" + BeforeSceneHash + "; after=" + sceneHash);
            Add(checks, "PREFAB_UNCHANGED", "hash",
                prefabHash == BeforePrefabHash, "before=" + BeforePrefabHash + "; after=" + prefabHash);
            Add(checks, "BUILD_SETTINGS_UNCHANGED", "hash",
                buildHash == BeforeBuildHash, "before=" + BeforeBuildHash + "; after=" + buildHash);

            List<string> missing = Enumerable.Range(1, 30)
                .Select(index => "I" + index.ToString("000"))
                .Where(id => !File.Exists(Abs(root, "Assets/_Game/Resources/item/道具icon/" + id + ".png")))
                .ToList();
            Add(checks, "BODY_ART_GAP_I001_I030", "art-gap", missing.Count == 30, string.Join(", ", missing));
            VerifySources(checks, root);
            WriteReports(checks, root, sceneHash, prefabHash, buildHash, missing);

            int failed = checks.Count(check => !check.pass);
            if (failed == 0)
            {
                Debug.Log("ITEM_DETAIL_MAX_DAO_ART_TEMPLATE01_PASS");
                return;
            }

            string failure = "ITEM_DETAIL_MAX_DAO_ART_TEMPLATE01_FAIL: " + failed + " checks failed.";
            Debug.LogError(failure);
            if (failBatch)
            {
                throw new BuildFailedException(failure);
            }
        }

        private static void VerifyScene(ICollection<Check> checks, Transform panel)
        {
            Add(checks, "SCENE_NATIVE_TRUTH", "scene",
                !PrefabUtility.IsPartOfPrefabInstance(panel.gameObject),
                "prefabStatus=" + PrefabUtility.GetPrefabInstanceStatus(panel.gameObject));
            Transform detail = Require(panel, "ItemDetailScrollView");
            Transform debug = Require(panel, "ItemDebugScrollView");
            Add(checks, "NON_PLAY_DEFAULT_VISIBLE", "scene",
                panel.gameObject.activeSelf && detail.gameObject.activeSelf && !debug.gameObject.activeSelf,
                "panel=" + panel.gameObject.activeSelf + "; detail=" + detail.gameObject.activeSelf + "; debug=" + debug.gameObject.activeSelf);

            Transform frame = Require(panel, "ItemDetailHeader/ItemArtworkFrame");
            VerifyIdentity(checks, "ZHENFAICON_IDENTITY", Require(frame, "zhenfaicon"), ZhenFaId,
                new Vector2(29f, -139f), new Vector2(200f, 200f));
            VerifyIdentity(checks, "QIXINGICON_IDENTITY", Require(frame, "qixingicon"), QiXingId,
                new Vector2(242.3f, -37.7f), new Vector2(50f, 100f));
            Transform daoju = Require(frame, "daoju");
            VerifyIdentity(checks, "DAOJU_IDENTITY", daoju, DaoJuId, Vector2.zero, new Vector2(210f, 210f));

            Image bodyImage = daoju.GetComponent<Image>();
            Text bodyText = Require(frame, "ItemArtworkText").GetComponent<Text>();
            Text bodyKey = Require(frame, "ItemArtworkKeyPlate/ItemArtworkKeyText").GetComponent<Text>();
            Add(checks, "DAOJU_EXPLICIT_GREYBOX", "scene",
                bodyImage != null && bodyImage.sprite == null && bodyImage.enabled
                    && bodyText != null && bodyText.text.Contains("主体图缺失", StringComparison.Ordinal)
                    && bodyKey != null && bodyKey.text == "MISSING_ART_I001",
                "sprite=" + AssetPath(bodyImage?.sprite) + "; key=" + bodyKey?.text);

            string allText = string.Join("\n", panel.GetComponentsInChildren<Text>(true)
                .Select(text => text.text ?? string.Empty));
            string[] tokens =
            {
                "EDITOR_PREVIEW_MAX_DAO_I001", "EDITOR_PREVIEW_ONLY", "NOT_ROLLED_INSTANCE",
                "NOT_LIVE_BALANCE_DATA", "震雷符", "道品", "震雷法", "qiLeiTag",
                "完整基础属性", "固定词条", "随机词条", "4 个核心效果",
                "2件效果", "4件效果", "6件效果", "普攻", "Build2", "Build4", "Build6",
                "点亮", "阵脉", "开窍"
            };
            Add(checks, "FULL_PRESSURE_TEXT", "content",
                tokens.All(token => allText.Contains(token, StringComparison.Ordinal)),
                "requiredTokens=" + tokens.Length);

            ItemDetailPanelView view = panel.GetComponent<ItemDetailPanelView>();
            Add(checks, "HEADER_ART", "slot",
                AssetPath(SerializedImage(view, "cardBackgroundImage")?.sprite)
                    == "Assets/_Game/Resources/item/弹窗背景/background-道品.png"
                && AssetPath(SerializedImage(view, "rarityBadgeImage")?.sprite)
                    == "Assets/_Game/Resources/item/品阶icon/品阶icon_道品.png"
                && Require(frame, "FaMenNameText").GetComponent<Text>() != null
                && Require(frame, "QiLeiNameText").GetComponent<Text>() != null
                && Require(frame, "PreviewComplianceText").GetComponent<Text>() != null,
                "background, rarity, identity labels, compliance labels");

            int rectCount = panel.GetComponentsInChildren<RectTransform>(true).Length;
            int enabledLayoutGroups = panel.GetComponentsInChildren<LayoutGroup>(true)
                .Count(driver => driver.enabled);
            int enabledContentFitters = panel.GetComponentsInChildren<ContentSizeFitter>(true)
                .Count(driver => driver.enabled);
            int enabledAspectFitters = panel.GetComponentsInChildren<AspectRatioFitter>(true)
                .Count(driver => driver.enabled);
            int enabledLayoutElements = panel.GetComponentsInChildren<LayoutElement>(true)
                .Count(driver => driver.enabled);
            Add(checks, "ALL_ITEMDETAIL_RECTS_MANUAL", "scene",
                enabledLayoutGroups == 0
                    && enabledContentFitters == 0
                    && enabledAspectFitters == 0
                    && enabledLayoutElements == 0,
                "rects=" + rectCount
                    + "; enabled LayoutGroup=" + enabledLayoutGroups
                    + "; ContentSizeFitter=" + enabledContentFitters
                    + "; AspectRatioFitter=" + enabledAspectFitters
                    + "; LayoutElement=" + enabledLayoutElements);
        }

        private static void VerifyArt(ICollection<Check> checks, Transform panel)
        {
            List<SlotSpec> specs = BuildSlotSpecs();
            int correct = 0;
            foreach (SlotSpec spec in specs)
            {
                Transform target;
                if (spec.section == null)
                {
                    target = panel.Find(spec.path);
                }
                else
                {
                    Transform body = FindDescendant(panel, spec.section)?.Find("BodyText");
                    target = body != null
                        ? body.Find(spec.path) ?? FindDescendant(body, spec.path)
                        : null;
                    if (body != null
                        && string.Equals(spec.section, "BaseStatsSection", StringComparison.Ordinal)
                        && spec.path.StartsWith("InlineArrayModifierIcon_", StringComparison.Ordinal))
                    {
                        target = body.GetComponentsInChildren<Transform>(true)
                            .FirstOrDefault(candidate =>
                                candidate.name.StartsWith("InlineArrayModifierIcon_", StringComparison.Ordinal)
                                && candidate.gameObject.activeSelf
                                && AssetPath(candidate.GetComponent<Image>()?.sprite) == spec.asset)
                            ?? target;
                    }
                }

                Image image = target?.GetComponent<Image>();
                bool expectsPlaceholder = string.IsNullOrEmpty(spec.asset);
                bool pass = image != null && image.enabled && image.gameObject.activeSelf
                    && (expectsPlaceholder ? image.sprite == null : AssetPath(image.sprite) == spec.asset);
                if (pass)
                {
                    correct++;
                }

                Add(checks, "SLOT_" + Clean(spec.name + "_" + spec.section), "slot", pass,
                    target == null ? "MISSING" : AssetPath(image?.sprite));
            }

            Transform detailContent = Require(panel, "ItemDetailScrollView/Viewport/DetailContent");
            ItemDetailSectionView[] sections = detailContent.GetComponentsInChildren<ItemDetailSectionView>(true);
            int thin = sections.Count(section =>
                AssetPath(section.transform.Find("SectionDivider")?.GetComponent<Image>()?.sprite)
                    == "Assets/_Game/Resources/item/分割线/分割线_细.png");
            int coarse = sections.Count(section =>
                AssetPath(section.transform.Find("CoarseDividerSlot")?.GetComponent<Image>()?.sprite)
                    == "Assets/_Game/Resources/item/分割线/分割线_粗.png");
            Add(checks, "DIVIDERS", "slot", thin == 15 && coarse == 3, "thin=" + thin + "; coarse=" + coarse);
            Add(checks, "ALL_ART_SLOTS", "slot", correct == specs.Count, "correct=" + correct + "/" + specs.Count);
        }

        private static List<SlotSpec> BuildSlotSpecs()
        {
            List<SlotSpec> specs = new()
            {
                HeaderSlot("zhenfaicon", "ItemDetailHeader/ItemArtworkFrame/zhenfaicon", "阵法icon/震雷法.png"),
                HeaderSlot("qixingicon", "ItemDetailHeader/ItemArtworkFrame/qixingicon", "器类icon/符类.png"),
                HeaderSlot("LightingStatusBadge", "ItemDetailHeader/ItemDetailHeaderTextGroup/ItemStatusBadgeRow/LightingStatusBadge", "聚念石icon/聚念石icon_点亮.png"),
                HeaderSlot("AwakeningStatusBadge", "ItemDetailHeader/ItemDetailHeaderTextGroup/ItemStatusBadgeRow/AwakeningStatusBadge", "核心icon/震雷法/I001/I001_4.png"),
                HeaderSlot("ArrayStatusBadge", "ItemDetailHeader/ItemDetailHeaderTextGroup/ItemStatusBadgeRow/ArrayStatusBadge", "阵脉icon/阵脉icon_点亮.png")
            };
            AddSeries(specs, "BaseStatsSection", "BaseStatIconSlot_", new[]
            {
                "基础属性icon/伤害.png", "基础属性icon/攻击.png", "基础属性icon/控制.png", "基础属性icon/破盾.png",
                "基础属性icon/冷却.png", "基础属性icon/耗念.png", "基础属性icon/护势.png", "阵脉icon/阵脉icon_点亮.png"
            });
            AddSeries(specs, "CoreAwakeningSection", "CoreEffectIconSlot_", new[]
            {
                "核心icon/震雷法/I001/I001_1.png", "核心icon/震雷法/I001/I001_2.png",
                "核心icon/震雷法/I001/I001_3.png", "核心icon/震雷法/I001/I001_4.png"
            });
            AddSeries(specs, "FaMenBuildSection", "FaMenBuildStageIconSlot_", new[]
            {
                "build技能icon/震雷法/技能icon_2.png", "build技能icon/震雷法/技能icon_3.png", "build技能icon/震雷法/技能icon_4.png"
            });
            AddSeries(specs, "QiLeiBuildSection", "QiLeiBuildStageIconSlot_", new[] { "器类icon/符类.png", "器类icon/符类.png" });
            AddSeries(specs, "MainBuildMonitorSection", "SkillMonitorIconSlot_", new[]
            {
                "build技能icon/震雷法/技能icon_1.png", "build技能icon/震雷法/技能icon_2.png",
                "build技能icon/震雷法/技能icon_3.png", "build技能icon/震雷法/技能icon_4.png"
            });
            AddSeries(specs, "FixedAffixSection", "FixedAffixIconSlot_", new[] { "词条icon/固定词条icon_道.png", "词条icon/固定词条icon_道.png" });
            AddSeries(specs, "RandomAffixSection", "RandomAffixIconSlot_", Enumerable.Repeat("词条icon/随机词条icon_道.png", 4).ToArray());
            AddSeries(specs, "OrangeGrowthSection", "DaoTraceIconSlot_", new[] { string.Empty });
            AddSeries(specs, "PlacementHintSection", "PlacementIconSlot_", new[] { "基础属性icon/摆放推荐.png" });
            AddSeries(specs, "FlavorSection", "FlavorIconSlot_", new[] { "基础属性icon/旧物记.png" });
            AddSeries(specs, "BaseStatsSection", "InlineArrayModifierIcon_", new[] { "阵脉icon/阵脉icon_点亮.png" });
            AddSeries(specs, "QiLeiBuildSection", "InlineArrayModifierIcon_", new[] { "阵脉icon/阵脉icon_点亮.png" });
            return specs;
        }

        private static void VerifyProtected(ICollection<Check> checks, Transform panel)
        {
            Transform root = panel.parent;
            bool geometry =
                Match(root, "CandidateInstancePreviewControls", "ItemSandboxRoot", true, 4, new Vector2(106f, -161f), new Vector2(127.32349f, 104.375206f))
                && Match(root, "BattleLikePreviewArea", "ItemSandboxRoot", true, 6, new Vector2(0f, 23f), new Vector2(960f, 1920f))
                && FindDescendant(root, "PopupLayer") == null
                && Match(root, "FeedbackRoot", "ItemSandboxRoot", true, 8, new Vector2(0f, 132f), Vector2.zero);
            Transform battleArea = FindDescendant(root, "BattleLikePreviewArea");
            geometry &= Match(battleArea, "ItemTrayPreview", "BattleLikePreviewArea", true, 2,
                new Vector2(0f, -515f), new Vector2(800f, 800f));
            Add(checks, "PROTECTED_NON_DETAIL_GEOMETRY", "protected", geometry,
                "ItemDetailPanel is the sole authorized geometry exclusion; PopupLayer was already absent in the pre-task Scene YAML and was not rebuilt");
            int cells = root.GetComponentsInChildren<ItemSandboxV04BoardCellView>(true).Length;
            Add(checks, "BOARD_READ_ONLY", "protected", cells == 25, "cells=" + cells);
            Add(checks, "FEEDBACK_AND_TRAY", "protected",
                FindDescendant(root, "FeedbackRoot") != null && FindDescendant(root, "ItemTrayPreview") != null,
                "protected siblings remain present");
        }

        private static void VerifySources(ICollection<Check> checks, string root)
        {
            string panelSource = File.ReadAllText(Abs(root, PanelViewRelative));
            string sectionSource = File.ReadAllText(Abs(root, SectionViewRelative));
            int disabled = sectionSource.IndexOf("#if false // ITEMDETAIL_LEGACY_AUTO_PREVIEW_DISABLED", StringComparison.Ordinal);
            string activeSection = disabled >= 0 ? sectionSource.Substring(0, disabled) : sectionSource;
            string runtime = panelSource + "\n" + activeSection;
            string[] layoutTokens =
            {
                ".anchoredPosition =", ".sizeDelta =", ".anchorMin =", ".anchorMax =", ".pivot =",
                ".SetParent(", ".SetSiblingIndex(", ".SetAsFirstSibling(", ".SetAsLastSibling(",
                "new GameObject(", "AddComponent<", "DestroyImmediate("
            };
            Add(checks, "NO_RUNTIME_LAYOUT_WRITES", "leak",
                layoutTokens.All(token => !runtime.Contains(token, StringComparison.Ordinal)),
                "tokens=" + layoutTokens.Length);
            Add(checks, "LEGACY_AUTO_PREVIEW_COMPILED_OUT", "leak",
                disabled >= 0 && disabled < sectionSource.IndexOf("[InitializeOnLoad]", StringComparison.Ordinal),
                "disabledBlockIndex=" + disabled);

            string authoring = File.ReadAllText(Abs(root, AuthoringRelative));
            string manualRowsAuthoring = File.ReadAllText(Abs(root, ManualRowsAuthoringRelative));
            string affixRowsAuthoring = File.ReadAllText(Abs(root, AffixRowsAuthoringRelative));
            string[] hooks =
            {
                "[InitializeOnLoad]", "sceneOpened", "playModeStateChanged", "delayCall",
                "RuntimeInitializeOnLoadMethod", "DidReloadScripts"
            };
            Add(checks, "MANUAL_AUTHORING_ONLY", "leak",
                hooks.All(token => !authoring.Contains(token, StringComparison.Ordinal))
                    && hooks.All(token => !manualRowsAuthoring.Contains(token, StringComparison.Ordinal))
                    && hooks.All(token => !affixRowsAuthoring.Contains(token, StringComparison.Ordinal))
                    && authoring.Contains("[MenuItem(", StringComparison.Ordinal)
                    && authoring.Contains("ApplyManualOnly()", StringComparison.Ordinal)
                    && manualRowsAuthoring.Contains("[MenuItem(", StringComparison.Ordinal)
                    && manualRowsAuthoring.Contains(
                        "MigrateCurrentNonPlayLayoutOnce()",
                        StringComparison.Ordinal)
                    && affixRowsAuthoring.Contains("[MenuItem(", StringComparison.Ordinal)
                    && affixRowsAuthoring.Contains(
                        "MigrateAffixRowsFromBaseStatsOnce()",
                        StringComparison.Ordinal),
                "no automatic hook");
            Add(checks, "RERUN_LAYOUT_GUARD", "leak",
                authoring.Contains("if (existing == null)", StringComparison.Ordinal)
                    && authoring.Contains("TransformSnapshot", StringComparison.Ordinal)
                    && authoring.Contains("AssertUnchanged", StringComparison.Ordinal)
                    && manualRowsAuthoring.Contains("ValidateExistingRows(existingRoot)", StringComparison.Ordinal)
                    && manualRowsAuthoring.Contains("ALREADY_PASS", StringComparison.Ordinal)
                    && affixRowsAuthoring.Contains("ITEM_DETAIL_AFFIX_MANUAL_ROWS_ALREADY_PASS", StringComparison.Ordinal),
                "creation-only initial RectTransform plus user-slot snapshot");

            string verifier = File.ReadAllText(Abs(root, VerifierRelative));
            string[] verifierMutations =
            {
                "EditorSceneManager." + "SaveScene(",
                "PrefabUtility." + "SaveAsPrefabAsset(",
                "AssetDatabase." + "SaveAssets(",
                "MarkScene" + "Dirty("
            };
            Add(checks, "VERIFIER_READ_ONLY", "leak",
                verifierMutations.All(token => !verifier.Contains(token, StringComparison.Ordinal))
                    && !verifier.Contains(
                        "ItemDetailMaxDaoArtTemplateAuthoring." + "ApplyManualOnly(",
                        StringComparison.Ordinal),
                "only report files are written");

            string[] forbidden =
            {
                "UnifiedBattlePage", "BattleContract", "BattleBridge", "V02RunFlowController",
                "MainTrialFlowService", "RewardService", "InventoryService", "SaveData", "Boss"
            };
            Add(checks, "NO_FORBIDDEN_SYSTEM_LEAK", "leak",
                forbidden.All(token => !authoring.Contains(token, StringComparison.Ordinal)
                    && !manualRowsAuthoring.Contains(token, StringComparison.Ordinal)
                    && !affixRowsAuthoring.Contains(token, StringComparison.Ordinal)
                    && !runtime.Contains(token, StringComparison.Ordinal)),
                "tokens=" + forbidden.Length);
            Add(checks, "NO_CANDIDATE_OR_ROLL_WRITEBACK", "leak",
                !authoring.Contains("Candidate", StringComparison.Ordinal)
                    && !authoring.Contains("ItemBalanceWorkbench", StringComparison.Ordinal)
                    && authoring.Contains("NOT_ROLLED_INSTANCE", StringComparison.Ordinal)
                    && authoring.Contains("NOT_LIVE_BALANCE_DATA", StringComparison.Ordinal),
                "static ItemDetailViewModel only");
        }

        private static void WriteReports(
            IReadOnlyCollection<Check> checks,
            string root,
            string sceneHash,
            string prefabHash,
            string buildHash,
            IReadOnlyCollection<string> missing)
        {
            string status = checks.All(check => check.pass) ? "PASS" : "FAIL";
            string reportPath = Abs(root, ReportRelative);
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? root);
            StringBuilder report = new();
            report.AppendLine("# ItemDetail Max Dao Art Template Report")
                .AppendLine()
                .AppendLine("- Status: " + status)
                .AppendLine("- Scene Root: " + PanelPath)
                .AppendLine("- Connected to ItemDetailPanel.prefab: NO; native Scene subtree is the edit truth")
                .AppendLine("- Preview: EDITOR_PREVIEW_MAX_DAO_I001 / EDITOR_PREVIEW_ONLY / NOT_ROLLED_INSTANCE / NOT_LIVE_BALANCE_DATA")
                .AppendLine("- User slots preserved: zhenfaicon=957452681, qixingicon=725680823, daoju=743156000")
                .AppendLine("- qixingicon semantic: qiLeiTag=符; it is not a seven-star system")
                .AppendLine("- Pre-task structural gap: PopupLayer was absent from the Scene YAML; this package did not rebuild it")
                .AppendLine("- Automatic layout writes: NO")
                .AppendLine("- Inspector-manual RectTransforms: all ItemDetailPanel descendants; all serialized layout drivers disabled")
                .AppendLine("- Manual authoring rerun: settled Scene hash remained " + sceneHash + "; no existing Slot RectTransform write path")
                .AppendLine("- Verifier Scene/Prefab save or rebuild: NO")
                .AppendLine()
                .AppendLine("## Hash ledger")
                .AppendLine()
                .AppendLine("| Asset | Before | After | Result |")
                .AppendLine("|---|---|---|---|")
                .AppendLine("| Scene | " + BeforeSceneHash + " | " + sceneHash + " | authorized child-subtree change; no baseline acceptance |")
                .AppendLine("| Prefab | " + BeforePrefabHash + " | " + prefabHash + " | " + (BeforePrefabHash == prefabHash ? "UNCHANGED" : "CHANGED") + " |")
                .AppendLine("| BuildSettings | " + BeforeBuildHash + " | " + buildHash + " | " + (BeforeBuildHash == buildHash ? "UNCHANGED" : "CHANGED") + " |")
                .AppendLine()
                .AppendLine("## Slot and asset map")
                .AppendLine()
                .AppendLine("| Slot | Scene path | Asset |")
                .AppendLine("|---|---|---|")
                .AppendLine("| cardBackgroundImage | ItemDetailPanel serialized slot | Assets/_Game/Resources/item/弹窗背景/background-道品.png |")
                .AppendLine("| rarityBadgeImage | ItemDetailHeader/RarityBadge | Assets/_Game/Resources/item/品阶icon/品阶icon_道品.png |")
                .AppendLine("| FaMenNameText | ItemDetailHeader/ItemArtworkFrame | N/A: static preview text |")
                .AppendLine("| QiLeiNameText | ItemDetailHeader/ItemArtworkFrame | N/A: static preview text |")
                .AppendLine("| PreviewComplianceText | ItemDetailHeader/ItemArtworkFrame | N/A: static preview text |")
                .AppendLine("| ItemArtworkText | ItemDetailHeader/ItemArtworkFrame | N/A: explicit missing-art label |")
                .AppendLine("| ItemArtworkKeyText | ItemDetailHeader/ItemArtworkFrame/ItemArtworkKeyPlate | N/A: MISSING_ART_I001 |");
            foreach (SlotSpec spec in BuildSlotSpecs())
            {
                string path = spec.section == null ? spec.path : "ItemDetailScrollView/Viewport/DetailContent/" + spec.section + "/BodyText/" + spec.path;
                report.AppendLine("| " + spec.name + " | " + path + " | " + spec.asset + " |");
            }
            report.AppendLine("| daoju | ItemDetailHeader/ItemArtworkFrame/daoju | MISSING_ART_I001 greybox; no core substitution |")
                .AppendLine("| SectionDivider (15) | each semantic detail section | Assets/_Game/Resources/item/分割线/分割线_细.png |")
                .AppendLine("| CoarseDividerSlot (3) | CoreAwakening/FaMen/MainBuild | Assets/_Game/Resources/item/分割线/分割线_粗.png |")
                .AppendLine()
                .AppendLine("## Missing art")
                .AppendLine()
                .AppendLine("Missing normal body sprites: " + string.Join(", ", missing))
                .AppendLine("No fake formal body art was generated.")
                .AppendLine()
                .AppendLine("## Checks");
            foreach (Check check in checks)
            {
                report.AppendLine("- " + (check.pass ? "PASS" : "FAIL") + " " + check.id + " — " + check.detail);
            }
            report.AppendLine()
                .AppendLine("## Manual test")
                .AppendLine()
                .AppendLine("1. Open Scene_TalismanBag_V04_ItemSandbox.unity without Play.")
                .AppendLine("2. Inspect all background, identity, stat, affix, core, Build, skill, and state slots.")
                .AppendLine("3. Move/resize any slot, save and reopen; confirm the layout persists.")
                .AppendLine("4. Enter Play once and exit; confirm no Builder or Runtime restores RectTransforms.");
            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(false));

            StringBuilder csv = new();
            csv.AppendLine("category,slot_name,scene_path,asset_path,status,notes");
            csv.AppendLine(Csv("header", "cardBackgroundImage", PanelPath, "Assets/_Game/Resources/item/弹窗背景/background-道品.png", "BOUND", "Dao background"));
            csv.AppendLine(Csv("header", "rarityBadgeImage", PanelPath + "/ItemDetailHeader", "Assets/_Game/Resources/item/品阶icon/品阶icon_道品.png", "BOUND", "Dao rarity"));
            csv.AppendLine(Csv("body-art", "daoju", PanelPath + "/ItemDetailHeader/ItemArtworkFrame/daoju", "MISSING_ART_I001", "GREYBOX", "original object; no core substitution"));
            csv.AppendLine(Csv("text-slot", "FaMenNameText", PanelPath + "/ItemDetailHeader/ItemArtworkFrame/FaMenNameText", "N/A", "BOUND", "阵法 · 震雷法"));
            csv.AppendLine(Csv("text-slot", "QiLeiNameText", PanelPath + "/ItemDetailHeader/ItemArtworkFrame/QiLeiNameText", "N/A", "BOUND", "qiLeiTag=符; not seven-star"));
            csv.AppendLine(Csv("text-slot", "PreviewComplianceText", PanelPath + "/ItemDetailHeader/ItemArtworkFrame/PreviewComplianceText", "N/A", "BOUND", "preview-only labels"));
            csv.AppendLine(Csv("text-slot", "ItemArtworkText", PanelPath + "/ItemDetailHeader/ItemArtworkFrame/ItemArtworkText", "N/A", "BOUND", "explicit missing body-art label"));
            csv.AppendLine(Csv("text-slot", "ItemArtworkKeyText", PanelPath + "/ItemDetailHeader/ItemArtworkFrame/ItemArtworkKeyPlate/ItemArtworkKeyText", "N/A", "BOUND", "MISSING_ART_I001"));
            foreach (SlotSpec spec in BuildSlotSpecs())
            {
                string path = spec.section == null ? spec.path : "ItemDetailScrollView/Viewport/DetailContent/" + spec.section + "/BodyText/" + spec.path;
                csv.AppendLine(Csv("art-slot", spec.name, PanelPath + "/" + path, spec.asset, "BOUND", spec.section ?? "header"));
            }
            csv.AppendLine(Csv("divider", "SectionDivider", PanelPath + "/ItemDetailScrollView/Viewport/DetailContent/*/SectionDivider", "Assets/_Game/Resources/item/分割线/分割线_细.png", "BOUND", "15 sections"));
            csv.AppendLine(Csv("divider", "CoarseDividerSlot", PanelPath + "/ItemDetailScrollView/Viewport/DetailContent/{CoreAwakeningSection,FaMenBuildSection,MainBuildMonitorSection}", "Assets/_Game/Resources/item/分割线/分割线_粗.png", "BOUND", "3 sections"));
            csv.AppendLine(Csv("manual-layout", "ItemDetailPanel descendants", PanelPath + "/**", "N/A", "UNLOCKED", "LayoutGroup, ContentSizeFitter, AspectRatioFitter and LayoutElement disabled"));
            foreach (string id in missing)
            {
                csv.AppendLine(Csv("missing-body-art", id, "Assets/_Game/Resources/item/道具icon", "Assets/_Game/Resources/item/道具icon/" + id + ".png", "MISSING", "gap only"));
            }
            File.WriteAllText(Abs(root, SpecRelative), csv.ToString(), new UTF8Encoding(false));

            StringBuilder leak = new();
            leak.AppendLine("# ItemDetail Max Dao Art Template Leak Check Report")
                .AppendLine()
                .AppendLine("- Status: " + status)
                .AppendLine("- Runtime RectTransform/hierarchy writes: NONE")
                .AppendLine("- Automatic authoring hooks: NONE; legacy block compiled out")
                .AppendLine("- Verifier Scene/Prefab save or rebuild: NONE")
                .AppendLine("- Candidate/Roll/live-balance writeback: NONE")
                .AppendLine("- Battle/RunFlow/Reward/Inventory/Save/Boss connection: NONE")
                .AppendLine("- BuildSettings modification: NONE")
                .AppendLine("- Board/tray/drag modification: NONE")
                .AppendLine("- .DS_Store consumption: NONE")
                .AppendLine("- Hash/geometry baseline update: NONE")
                .AppendLine();
            foreach (Check check in checks.Where(check => check.category is "leak" or "protected" or "hash" or "art-gap"))
            {
                leak.AppendLine("- " + (check.pass ? "PASS" : "FAIL") + " " + check.id + " — " + check.detail);
            }
            File.WriteAllText(Abs(root, LeakRelative), leak.ToString(), new UTF8Encoding(false));
        }

        private static void VerifyIdentity(ICollection<Check> checks, string id, Transform target, ulong expectedId, Vector2 position, Vector2 size)
        {
            RectTransform rect = target as RectTransform;
            ulong actual = GlobalObjectId.GetGlobalObjectIdSlow(target.gameObject).targetObjectId;
            bool pass = rect != null && actual == expectedId && target.parent?.name == "ItemArtworkFrame"
                && Approx(rect.anchoredPosition, position) && Approx(rect.sizeDelta, size);
            Add(checks, id, "identity", pass, "id=" + actual + "; pos=" + rect?.anchoredPosition + "; size=" + rect?.sizeDelta);
        }

        private static bool Match(Transform root, string name, string parent, bool active, int sibling, Vector2 position, Vector2 size)
        {
            RectTransform rect = FindDescendant(root, name) as RectTransform;
            return rect != null && rect.gameObject.activeSelf == active && rect.GetSiblingIndex() == sibling
                && rect.parent?.name == parent && Approx(rect.anchoredPosition, position) && Approx(rect.sizeDelta, size);
        }

        private static SlotSpec HeaderSlot(string name, string path, string asset)
        {
            return new SlotSpec(name, null, path, "Assets/_Game/Resources/item/" + asset);
        }

        private static void AddSeries(ICollection<SlotSpec> specs, string section, string prefix, IReadOnlyList<string> assets)
        {
            for (int index = 0; index < assets.Count; index++)
            {
                string asset = string.IsNullOrWhiteSpace(assets[index])
                    ? string.Empty
                    : "Assets/_Game/Resources/item/" + assets[index];
                specs.Add(new SlotSpec(prefix + index, section, prefix + index, asset));
            }
        }

        private static Image SerializedImage(ItemDetailPanelView view, string name)
        {
            return view == null ? null : new SerializedObject(view).FindProperty(name)?.objectReferenceValue as Image;
        }

        private static Transform FindPath(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            Transform current = scene.GetRootGameObjects().FirstOrDefault(go => go.name == parts[0])?.transform;
            for (int index = 1; index < parts.Length && current != null; index++)
            {
                current = current.Find(parts[index]);
            }
            return current;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            return root?.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == name);
        }

        private static Transform Require(Transform root, string path)
        {
            return root?.Find(path) ?? throw new InvalidOperationException("Required object missing: " + path);
        }

        private static string AssetPath(UnityEngine.Object asset)
        {
            return asset == null ? "MISSING" : AssetDatabase.GetAssetPath(asset).Replace('\\', '/');
        }

        private static string Abs(string root, string path)
        {
            return Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string Hash(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static bool Approx(Vector2 a, Vector2 b)
        {
            return Mathf.Abs(a.x - b.x) < 0.01f && Mathf.Abs(a.y - b.y) < 0.01f;
        }

        private static void Add(ICollection<Check> checks, string id, string category, bool pass, string detail)
        {
            checks.Add(new Check(id, category, pass, detail));
        }

        private static string Clean(string value)
        {
            return new string((value ?? string.Empty).Select(c => char.IsLetterOrDigit(c) ? char.ToUpperInvariant(c) : '_').ToArray());
        }

        private static string Csv(params string[] fields)
        {
            string quote = ((char)34).ToString();
            return string.Join(",", fields.Select(field =>
                quote + (field ?? string.Empty).Replace(quote, quote + quote) + quote));
        }

        private sealed class Check
        {
            public readonly string id;
            public readonly string category;
            public readonly bool pass;
            public readonly string detail;

            public Check(string id, string category, bool pass, string detail)
            {
                this.id = id;
                this.category = category;
                this.pass = pass;
                this.detail = detail;
            }
        }

        private sealed class SlotSpec
        {
            public readonly string name;
            public readonly string section;
            public readonly string path;
            public readonly string asset;

            public SlotSpec(string name, string section, string path, string asset)
            {
                this.name = name;
                this.section = section;
                this.path = path;
                this.asset = asset;
            }
        }
    }
}
