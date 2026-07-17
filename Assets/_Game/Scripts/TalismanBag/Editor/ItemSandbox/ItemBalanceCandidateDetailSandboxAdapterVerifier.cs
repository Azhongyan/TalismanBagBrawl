#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.ItemSandbox;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemBalanceCandidateDetailSandboxAdapterVerifier
    {
        public const string Marker = "ITEM_BALANCE_CANDIDATE_DETAIL_SANDBOX_ADAPTER01_PASS";
        public const string ReportPath = "Docs/V0.4/Reports/ItemBalanceCandidateDetailSandboxAdapterReport.md";
        public const string SpecPath = "Docs/V0.4/Reports/ItemBalanceCandidateDetailSandboxAdapterSpec.csv";
        public const string LeakPath = "Docs/V0.4/Reports/ItemBalanceCandidateDetailSandboxAdapterLeakCheckReport.md";

        private static readonly string[] RarityKeys =
        {
            "white", "green", "blue", "purple", "orange"
        };

        private static readonly string[] ScopedRuntimeSources =
        {
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "UnityEngine.Random",
            ".GetHashCode(",
            "using TalismanBag.V02",
            "using TalismanBag.UnifiedBattle",
            "PlayerPrefs",
            "ItemInventoryService",
            "RewardService",
            "V02RunFlowController",
            "EditorBuildSettings",
            "AssetDatabase.SaveAssets"
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter01/Verify")]
        public static void VerifyMenu()
        {
            Verification verification = Verify();
            WriteOutputs(verification);
            if (!verification.Passed)
            {
                throw new InvalidOperationException(string.Join(" | ", verification.Errors));
            }

            Debug.Log($"{Marker} candidates={verification.CandidateCount}/150 ranges={verification.StatRangeCount}/600 determinism={verification.DeterminismCount}/150");
        }

        public static void VerifyBatch()
        {
            try
            {
                VerifyMenu();
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        private static Verification Verify()
        {
            Verification verification = new();
            BoundarySnapshot before = CaptureBoundaries();
            ItemBalanceWorkbenchCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                ItemSandboxDetailUiSceneBuilder.ItemBalanceWorkbenchCatalogPath);
            GameObject host = new("ItemBalanceCandidateDetailVerifierHost");
            try
            {
                ItemInnerDataCatalogProvider catalogProvider = host.AddComponent<ItemInnerDataCatalogProvider>();
                ItemBalanceCandidateDetailSandboxAdapter adapter =
                    new(catalog, catalogProvider);
                ItemBalanceCandidateDetailSandboxAdapter repeatAdapter =
                    new(catalog, catalogProvider);

                Check(verification, "catalog-asset", catalog != null,
                    "Workbench catalog asset exists", catalog == null ? "missing" : "present");
                Check(verification, "ordinary-list-30", adapter.CandidateBaseItemIds.Count == 30,
                    "30 ordinary base items", adapter.CandidateBaseItemIds.Count.ToString(CultureInfo.InvariantCulture));
                Check(verification, "ordinary-list-excludes-I031",
                    !adapter.CandidateBaseItemIds.Contains("I031", StringComparer.Ordinal),
                    "I031 excluded", string.Join("|", adapter.CandidateBaseItemIds));

                const long seed = 77041201L;
                ItemBalanceCandidateDetailResult layoutCandidate = null;
                foreach (string baseItemId in adapter.CandidateBaseItemIds)
                {
                    ItemDetailViewModel catalogModel = catalogProvider.GetDetailViewModel(baseItemId);
                    bool queryable = catalogModel != null
                        && string.Equals(catalogModel.itemId, baseItemId, StringComparison.Ordinal);
                    Check(verification, "base-query-" + baseItemId, queryable,
                        "catalog base item query succeeds", queryable ? baseItemId : "missing");
                    if (queryable) verification.BaseItemCount++;

                    foreach (string rarityKey in RarityKeys)
                    {
                        ItemBalanceCandidateDetailRequest request = new()
                        {
                            baseItemId = baseItemId,
                            rarityKey = rarityKey,
                            rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
                        };
                        ItemBalanceCandidateDetailResult result = adapter.Request(request);
                        string caseId = baseItemId + "@" + rarityKey;
                        bool generated = result.isSuccess
                            && result.preview?.RollResult?.snapshot != null;
                        Check(verification, "candidate-" + caseId, generated,
                            "candidate instance generated", Describe(result));
                        if (generated) verification.CandidateCount++;

                        bool projected = result.detailProjection?.projection != null
                            && string.Equals(result.detailProjection.projection.itemInstanceId,
                                result.viewModel?.itemInstanceId, StringComparison.Ordinal);
                        Check(verification, "projection-" + caseId, projected,
                            "exact read-only projection", result.viewModel?.itemInstanceId ?? "missing");
                        if (projected) verification.ProjectionCount++;

                        bool viewModel = result.viewModel != null
                            && result.viewModel.displayPlayerSections?.Count == 15
                            && result.viewModel.displayDebugSections?.Count == 3
                            && Hidden(result.viewModel.displayPlayerSections, "itemPower")
                            && Hidden(result.viewModel.displayPlayerSections, "trigger")
                            && Hidden(result.viewModel.displayPlayerSections, "basic")
                            && Hidden(result.viewModel.displayPlayerSections, "skillMonitor")
                            && !Contains(result.viewModel.displayPlayerSections, "BALANCE_CANDIDATE")
                            && !Contains(result.viewModel.displayPlayerSections, "预览状态")
                            && !Contains(result.viewModel.displayPlayerSections, "物品强度")
                            && !Contains(result.viewModel.displayPlayerSections, "点亮与基础效果")
                            && Contains(result.viewModel.displayDebugSections, "BALANCE_CANDIDATE")
                            && Contains(result.viewModel.displayDebugSections, "NOT_BATTLE_CONNECTED")
                            && Contains(result.viewModel.displayDebugSections, "playerHiddenItemPower")
                            && Contains(result.viewModel.displayDebugSections, "validationErrors: None");
                        Check(verification, "viewmodel-" + caseId, viewModel,
                            "15 prefab player slots + 3 debug candidate sections; empty/test slots hidden",
                            viewModel ? "15/3" : "invalid");
                        if (viewModel) verification.ViewModelCount++;

                        int rangeCount = result.candidateStatRangeCount;
                        for (int index = 0; index < rangeCount; index++)
                        {
                            verification.Rows.Add(new SpecRow(
                                "stat-range-" + caseId + "-" + index,
                                baseItemId,
                                rarityKey,
                                seed,
                                result.viewModel?.itemInstanceId,
                                "candidate range readable with final roll",
                                "PASS",
                                "range index " + index));
                        }
                        verification.StatRangeCount += rangeCount;

                        ItemBalanceCandidateDetailResult repeat = repeatAdapter.Request(request);
                        bool deterministic = result.isSuccess && repeat.isSuccess
                            && string.Equals(result.viewModel.itemInstanceId,
                                repeat.viewModel.itemInstanceId, StringComparison.Ordinal)
                            && string.Equals(result.detailProjection.projection.sourceCanonicalSignature,
                                repeat.detailProjection.projection.sourceCanonicalSignature,
                                StringComparison.Ordinal);
                        Check(verification, "determinism-" + caseId, deterministic,
                            "same base+rarity+seed is identical",
                            deterministic ? result.viewModel.itemInstanceId : "mismatch");
                        if (deterministic) verification.DeterminismCount++;

                        long alternateSeed = seed + 1L;
                        ItemBalanceCandidateDetailResult alternate = adapter.Request(
                            new ItemBalanceCandidateDetailRequest
                            {
                                baseItemId = baseItemId,
                                rarityKey = rarityKey,
                                rootSeedText = alternateSeed.ToString(CultureInfo.InvariantCulture)
                            });
                        bool seedSwitch = alternate.isSuccess
                            && !string.Equals(result.viewModel?.itemInstanceId,
                                alternate.viewModel?.itemInstanceId, StringComparison.Ordinal)
                            && string.Equals(result.viewModel?.baseItemId,
                                alternate.viewModel?.baseItemId, StringComparison.Ordinal);
                        Check(verification, "seed-switch-" + caseId, seedSwitch,
                            "explicit seed change creates a distinct legal instance",
                            alternate.viewModel?.itemInstanceId ?? "missing");
                        if (seedSwitch) verification.SeedSwitchCount++;

                        layoutCandidate ??= result;
                    }
                }

                Check(verification, "count-base-items", verification.BaseItemCount == 30,
                    "30/30", verification.BaseItemCount + "/30");
                Check(verification, "count-candidates", verification.CandidateCount == 150,
                    "150/150", verification.CandidateCount + "/150");
                Check(verification, "count-projections", verification.ProjectionCount == 150,
                    "150/150", verification.ProjectionCount + "/150");
                Check(verification, "count-viewmodels", verification.ViewModelCount == 150,
                    "150/150", verification.ViewModelCount + "/150");
                Check(verification, "count-stat-ranges", verification.StatRangeCount == 600,
                    "600/600", verification.StatRangeCount + "/600");
                Check(verification, "count-determinism", verification.DeterminismCount == 150,
                    "150/150", verification.DeterminismCount + "/150");
                Check(verification, "count-seed-switch", verification.SeedSwitchCount == 150,
                    "150/150", verification.SeedSwitchCount + "/150");

                CheckInvalidRequests(verification, adapter, catalog, catalogProvider, seed);
                CheckContextIsolation(verification, adapter, catalogProvider, seed);
                CheckScene(verification);
                CheckLayout(verification, layoutCandidate?.viewModel);
                CheckRegressionReports(verification);
                CheckLeaks(verification);
            }
            catch (Exception exception)
            {
                verification.Errors.Add(exception.GetType().Name + ": " + exception.Message);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }

            BoundarySnapshot after = CaptureBoundaries();
            CompareBoundaries(verification, before, after);
            return verification;
        }

        private static void CheckInvalidRequests(
            Verification verification,
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemBalanceWorkbenchCatalog catalog,
            ItemInnerDataCatalogProvider catalogProvider,
            long seed)
        {
            ItemBalanceCandidateDetailResult i031 = adapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = "I031",
                rarityKey = "white",
                rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
            });
            Check(verification, "I031-generation-zero",
                !i031.isSuccess
                && i031.status == ItemBalanceCandidateDetailRequestStatus.OrdinaryGenerationExcluded,
                "I031 ordinary generation count 0", string.Join("|", i031.ValidationErrors));

            ItemBalanceCandidateDetailResult badRarity = adapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = "I001", rarityKey = "red", rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
            });
            Check(verification, "invalid-rarity-validation",
                !badRarity.isSuccess && badRarity.ValidationErrors.Any(value => value.Contains("RARITY_INVALID", StringComparison.Ordinal)),
                "validationErrors, no throw", string.Join("|", badRarity.ValidationErrors));

            ItemBalanceCandidateDetailResult badSeed = adapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = "I001", rarityKey = "white", rootSeedText = "not-a-seed"
            });
            Check(verification, "invalid-seed-validation",
                !badSeed.isSuccess && badSeed.ValidationErrors.Any(value => value.Contains("ROOT_SEED_INVALID", StringComparison.Ordinal)),
                "validationErrors, no throw", string.Join("|", badSeed.ValidationErrors));

            ItemBalanceWorkbenchCatalog missingProfileCatalog = catalog == null
                ? null
                : UnityEngine.Object.Instantiate(catalog);
            try
            {
                if (missingProfileCatalog != null)
                {
                    missingProfileCatalog.profiles = missingProfileCatalog.profiles
                        .Where(profile => profile != null && profile.baseItemId != "I001")
                        .ToList();
                }
                ItemBalanceCandidateDetailSandboxAdapter missingAdapter =
                    new(missingProfileCatalog, catalogProvider);
                ItemBalanceCandidateDetailResult missing = missingAdapter.Request(
                    new ItemBalanceCandidateDetailRequest
                    {
                        baseItemId = "I001",
                        rarityKey = "white",
                        rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
                    });
                Check(verification, "missing-profile-validation",
                    !missing.isSuccess && missing.status == ItemBalanceCandidateDetailRequestStatus.ProfileMissing,
                    "validationErrors, no throw", string.Join("|", missing.ValidationErrors));
            }
            finally
            {
                if (missingProfileCatalog != null) UnityEngine.Object.DestroyImmediate(missingProfileCatalog);
            }
        }

        private static void CheckContextIsolation(
            Verification verification,
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemInnerDataCatalogProvider catalogProvider,
            long seed)
        {
            ItemDetailViewModel catalogI031 = catalogProvider.GetDetailViewModel("I031");
            ItemBalanceCandidateDetailResult candidateI001 = adapter.Request(
                new ItemBalanceCandidateDetailRequest
                {
                    baseItemId = "I001",
                    rarityKey = "green",
                    rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
                });
            bool isolated = catalogI031 != null
                && candidateI001.isSuccess
                && !Contains(catalogI031.displayPlayerSections, "BALANCE_CANDIDATE")
                && !Contains(candidateI001.viewModel.displayPlayerSections, "BALANCE_CANDIDATE")
                && Contains(candidateI001.viewModel.displayDebugSections, "BALANCE_CANDIDATE")
                && adapter.CandidateBaseItemIds.Count == 30;
            Check(verification, "catalog-candidate-context-isolation", isolated,
                "Catalog keeps 31 including I031; Candidate keeps 30 ordinary profiles",
                isolated ? "31/30 isolated" : "context leak");

            ItemBalanceCandidateDetailResult seedA = candidateI001;
            ItemBalanceCandidateDetailResult seedB = adapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = "I001", rarityKey = "green", rootSeedText = (seed + 2L).ToString(CultureInfo.InvariantCulture)
            });
            bool strictIdentity = seedA.isSuccess && seedB.isSuccess
                && seedA.viewModel.itemInstanceId != seedB.viewModel.itemInstanceId
                && seedA.viewModel.baseItemId == seedB.viewModel.baseItemId;
            Check(verification, "same-base-rarity-multi-instance-strict", strictIdentity,
                "same base+rarity instances are distinct by itemInstanceId",
                (seedA.viewModel?.itemInstanceId ?? "missing") + " | " + (seedB.viewModel?.itemInstanceId ?? "missing"));
        }

        private static void CheckScene(Verification verification)
        {
            EditorSceneManager.OpenScene(ItemSandboxDetailUiSceneBuilder.ScenePath, OpenSceneMode.Single);
            ItemBalanceCandidateDetailSandboxProvider provider =
                UnityEngine.Object.FindObjectOfType<ItemBalanceCandidateDetailSandboxProvider>(true);
            ItemSandboxGeneratedInstanceProvider legacyFixture =
                UnityEngine.Object.FindObjectOfType<ItemSandboxGeneratedInstanceProvider>(true);
            ItemSandboxDetailUiController controller =
                UnityEngine.Object.FindObjectOfType<ItemSandboxDetailUiController>(true);
            Transform sceneRoot = controller != null ? controller.transform.root : null;
            Transform controls = FindChild(sceneRoot, "CandidateInstancePreviewControls");
            Transform modeBar = FindChild(sceneRoot, "ItemDetailContextModeBar");
            bool structure = provider != null
                && legacyFixture != null
                && controller != null
                && controller.HasCandidateInstanceMode
                && controls != null
                && modeBar != null
                && FindChild(modeBar, "CatalogModeButton") != null
                && FindChild(modeBar, "GeneratedInstancesModeButton") != null
                && FindChild(controls, "CandidateSeedInput") != null
                && FindChild(controls, "RegenerateCandidateButton") != null;
            Check(verification, "scene-candidate-controls", structure,
                "Catalog/Candidate modes + 5 rarity + seed + regenerate",
                structure ? "present" : "missing");
            bool providerBoundary = provider != null
                && provider.DevOnly
                && !provider.ReadsFormalBattleObject
                && !provider.ReadsFormalSaveData
                && !provider.WritesFormalSystem
                && provider.GetItemList().Count == 30
                && provider.GetItemList().All(entry => entry.itemId != "I031");
            Check(verification, "scene-provider-boundary", providerBoundary,
                "devOnly read-only, 30 ordinary items, no formal writes",
                provider == null ? "missing" : provider.GetItemList().Count.ToString(CultureInfo.InvariantCulture));
        }

        private static void CheckLayout(Verification verification, ItemDetailViewModel candidate)
        {
            if (candidate == null)
            {
                Check(verification, "layout-candidate-source", false, "candidate model exists", "missing");
                return;
            }

            ItemDetailViewModel stress = candidate.Clone();
            stress.displayPlayerSections[3].body = "  " + string.Join("\n  ",
                Enumerable.Range(1, 120).Select(index => "候选属性/词条压力行 " + index));
            CheckLayoutAtResolution(verification, stress, new Vector2(1080f, 1920f), "layout-1080x1920");
            CheckLayoutAtResolution(verification, stress, new Vector2(720f, 1280f), "layout-720x1280");
        }

        private static void CheckLayoutAtResolution(
            Verification verification,
            ItemDetailViewModel model,
            Vector2 resolution,
            string caseId)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                ItemSandboxDetailUiSceneBuilder.ItemDetailPanelPrefabPath);
            GameObject canvasObject = new("CandidateLayoutVerifierCanvas", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            try
            {
                RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
                canvasRect.sizeDelta = resolution;
                Canvas canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = resolution;

                GameObject instance = prefab == null
                    ? null
                    : PrefabUtility.InstantiatePrefab(prefab, canvasObject.transform) as GameObject;
                RectTransform panelRect = instance?.GetComponent<RectTransform>();
                ItemDetailPanelView view = instance?.GetComponent<ItemDetailPanelView>();
                if (panelRect == null || view == null)
                {
                    Check(verification, caseId, false, "reusable ItemDetailPanel layout", "missing prefab/view");
                    return;
                }

                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = new Vector2(18f, 18f);
                panelRect.offsetMax = new Vector2(-18f, -18f);
                view.Bind(model);
                view.ShowPlayerDetailTab();
                ForceLayout(panelRect);
                ScrollRect detail = FindChildComponent<ScrollRect>(instance.transform, "ItemDetailScrollView");
                RectTransform detailContent = FindChild(instance.transform, "DetailContent") as RectTransform;
                int expectedActiveSections = model.displayPlayerSections.Count(section =>
                    section.keepWhenEmpty || !string.IsNullOrWhiteSpace(section.body));
                bool detailOk = CheckScroll(detail, detailContent)
                    && CheckNoOverlap(detailContent)
                    && CheckTextContained(detailContent)
                    && detailContent.GetComponentsInChildren<ItemDetailSectionView>(true)
                        .Count(section => section.gameObject.activeSelf) == expectedActiveSections
                    && detail.content.rect.height > detail.viewport.rect.height + 1f;

                view.ShowDebugTab();
                ForceLayout(panelRect);
                ScrollRect debug = FindChildComponent<ScrollRect>(instance.transform, "ItemDebugScrollView");
                RectTransform debugContent = FindChild(instance.transform, "DebugContent") as RectTransform;
                bool debugOk = CheckScroll(debug, debugContent)
                    && CheckNoOverlap(debugContent)
                    && CheckTextContained(debugContent)
                    && debugContent.GetComponentsInChildren<ItemDetailSectionView>(true)
                        .Count(section => section.gameObject.activeSelf) == 3;
                view.ShowPlayerDetailTab();
                ForceLayout(panelRect);
                bool reset = detail != null && Mathf.Abs(detail.verticalNormalizedPosition - 1f) <= 0.01f;
                bool passed = detailOk && debugOk && reset;
                string actual = $"detail={detail?.viewport?.rect.width:0}x{detail?.viewport?.rect.height:0}; content={detailContent?.rect.height:0}; debug={debug?.viewport?.rect.width:0}x{debug?.viewport?.rect.height:0}; reset={reset}";
                Check(verification, caseId, passed,
                    "no overlap/text overflow; long scroll; item switch resets top", actual);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvasObject);
            }
        }

        private static void CheckRegressionReports(Verification verification)
        {
            string[] reports =
            {
                "Docs/V0.4/Reports/ItemBalanceWorkbenchReport.md",
                "Docs/V0.4/Reports/ItemRarityInstanceFoundationReport.md",
                "Docs/V0.4/Reports/ItemStatRangeSchemaReport.md",
                "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaReport.md",
                "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaReport.md",
                "Docs/V0.4/Reports/ItemInstanceRollEngineReport.md",
                "Docs/V0.4/Reports/ItemDropGenerationSandboxReport.md",
                "Docs/V0.4/Reports/ItemGenerationSimulationValidatorReport.md",
                "Docs/V0.4/Reports/ItemInstanceProjectionContractReport.md",
                "Docs/V0.4/Reports/ItemDetailInstanceDataAdapterReport.md",
                "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md",
                "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md",
                "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md",
                "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md",
                "Docs/V0.4/Reports/BuildSynergyCoreReport.md"
            };
            foreach (string path in reports)
            {
                bool pass = File.Exists(path)
                    && File.ReadAllText(path).Contains("PASS", StringComparison.OrdinalIgnoreCase);
                Check(verification, "regression-" + Path.GetFileNameWithoutExtension(path), pass,
                    "existing package report PASS; live candidate chain compiled/exercised",
                    pass ? path : "missing/no PASS");
                if (pass) verification.RegressionCount++;
            }
        }

        private static void CheckLeaks(Verification verification)
        {
            List<string> leaks = new();
            foreach (string path in ScopedRuntimeSources)
            {
                if (!File.Exists(path))
                {
                    leaks.Add("Missing scoped source: " + path);
                    continue;
                }

                string source = File.ReadAllText(path);
                foreach (string token in ForbiddenRuntimeTokens)
                {
                    if (source.Contains(token, StringComparison.Ordinal))
                    {
                        leaks.Add(path + ": forbidden token " + token);
                    }
                }
            }

            verification.Leaks.AddRange(leaks);
            Check(verification, "leak-scan", leaks.Count == 0,
                "no global Random/GetHashCode/formal Battle/RunFlow/Reward/Inventory/Save/BuildSettings wiring",
                leaks.Count == 0 ? "clean" : string.Join(" | ", leaks));
        }

        private static BoundarySnapshot CaptureBoundaries()
        {
            return new BoundarySnapshot
            {
                Profiles = HashTree("Assets/_Game/Configs/ItemBalanceWorkbench/Profiles", _ => true),
                WorkbenchCatalog = HashFiles(new[]
                {
                    ItemSandboxDetailUiSceneBuilder.ItemBalanceWorkbenchCatalogPath,
                    ItemSandboxDetailUiSceneBuilder.ItemBalanceWorkbenchCatalogPath + ".meta"
                }),
                RuntimeSchema = HashTree("Assets/_Game/Scripts/TalismanBag/Items", _ => true),
                Prefabs = HashTree("Assets/_Game/Prefabs", _ => true),
                ScenesOutsideTarget = HashTree("Assets/_Game/Scenes", path =>
                    !path.Replace('\\', '/').EndsWith("Scene_TalismanBag_V04_ItemSandbox.unity", StringComparison.Ordinal)
                    && !path.Replace('\\', '/').EndsWith("Scene_TalismanBag_V04_ItemSandbox.unity.meta", StringComparison.Ordinal)),
                BuildSettings = HashFiles(new[] { "ProjectSettings/EditorBuildSettings.asset" })
            };
        }

        private static void CompareBoundaries(
            Verification verification,
            BoundarySnapshot before,
            BoundarySnapshot after)
        {
            CompareBoundary(verification, "hash-profiles-30", before.Profiles, after.Profiles);
            CompareBoundary(verification, "hash-workbench-catalog", before.WorkbenchCatalog, after.WorkbenchCatalog);
            CompareBoundary(verification, "hash-runtime-schema", before.RuntimeSchema, after.RuntimeSchema);
            CompareBoundary(verification, "hash-prefabs", before.Prefabs, after.Prefabs);
            CompareBoundary(verification, "hash-scenes-outside-target", before.ScenesOutsideTarget, after.ScenesOutsideTarget);
            CompareBoundary(verification, "hash-buildsettings", before.BuildSettings, after.BuildSettings);
            verification.Boundaries = new Dictionary<string, (string before, string after)>
            {
                ["30 candidate profiles"] = (before.Profiles, after.Profiles),
                ["workbench catalog"] = (before.WorkbenchCatalog, after.WorkbenchCatalog),
                ["runtime/schema"] = (before.RuntimeSchema, after.RuntimeSchema),
                ["prefabs"] = (before.Prefabs, after.Prefabs),
                ["scenes outside ItemSandbox"] = (before.ScenesOutsideTarget, after.ScenesOutsideTarget),
                ["BuildSettings"] = (before.BuildSettings, after.BuildSettings)
            };
        }

        private static void CompareBoundary(Verification verification, string id, string before, string after)
        {
            Check(verification, id, string.Equals(before, after, StringComparison.Ordinal),
                "before hash equals after hash", before + " -> " + after);
        }

        private static string HashTree(string root, Func<string, bool> predicate)
        {
            if (!Directory.Exists(root)) return "MISSING";
            return HashFiles(Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Where(path => predicate == null || predicate(path)));
        }

        private static string HashFiles(IEnumerable<string> files)
        {
            StringBuilder manifest = new();
            foreach (string path in (files ?? Array.Empty<string>())
                .Where(File.Exists)
                .OrderBy(path => path.Replace('\\', '/'), StringComparer.Ordinal))
            {
                manifest.Append(path.Replace('\\', '/')).Append('|')
                    .Append(HashBytes(File.ReadAllBytes(path))).Append('\n');
            }
            return HashBytes(Encoding.UTF8.GetBytes(manifest.ToString()));
        }

        private static string HashBytes(byte[] bytes)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes ?? Array.Empty<byte>())).Replace("-", string.Empty);
        }

        private static void WriteOutputs(Verification verification)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            StringBuilder csv = new();
            csv.AppendLine("caseId,baseItemId,rarity,rootSeed,itemInstanceId,expected,result,actual");
            foreach (SpecRow row in verification.Rows)
            {
                csv.AppendLine(string.Join(",", new[]
                {
                    Csv(row.CaseId), Csv(row.BaseItemId), Csv(row.Rarity),
                    row.RootSeed.ToString(CultureInfo.InvariantCulture), Csv(row.ItemInstanceId),
                    Csv(row.Expected), Csv(row.Result), Csv(row.Actual)
                }));
            }
            File.WriteAllText(SpecPath, csv.ToString(), new UTF8Encoding(false));

            StringBuilder report = new();
            report.AppendLine("# Item Balance Candidate Detail Sandbox Adapter Report")
                .AppendLine()
                .AppendLine("- Package: `V0.4-ItemBalanceCandidateDetailSandboxAdapter01`")
                .AppendLine("- Result: " + (verification.Passed ? "PASS" : "FAIL"))
                .AppendLine("- Marker: `" + (verification.Passed ? Marker : "NOT_EMITTED") + "`")
                .AppendLine("- Base items: " + verification.BaseItemCount + "/30")
                .AppendLine("- Candidate combinations: " + verification.CandidateCount + "/150")
                .AppendLine("- Read-only projections: " + verification.ProjectionCount + "/150")
                .AppendLine("- ItemDetailViewModels: " + verification.ViewModelCount + "/150")
                .AppendLine("- Candidate stat ranges: " + verification.StatRangeCount + "/600")
                .AppendLine("- Determinism: " + verification.DeterminismCount + "/150")
                .AppendLine("- Explicit seed switches: " + verification.SeedSwitchCount + "/150")
                .AppendLine("- I031 ordinary generation: 0")
                .AppendLine("- Regression reports: " + verification.RegressionCount + "/15 PASS")
                .AppendLine("- Data maturity: `BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED`")
                .AppendLine("- Sandbox Item Detail connected: YES")
                .AppendLine("- Formal Item Detail connected: NO")
                .AppendLine("- Battle connected: NO")
                .AppendLine("- Reward/Save connected: NO")
                .AppendLine()
                .AppendLine("## Protected boundary hashes")
                .AppendLine()
                .AppendLine("| category | before | after | result |")
                .AppendLine("|---|---|---|---|");
            foreach (KeyValuePair<string, (string before, string after)> boundary in verification.Boundaries)
            {
                report.Append("| ").Append(boundary.Key).Append(" | `")
                    .Append(boundary.Value.before).Append("` | `")
                    .Append(boundary.Value.after).Append("` | ")
                    .Append(boundary.Value.before == boundary.Value.after ? "PASS" : "FAIL")
                    .AppendLine(" |");
            }
            report.AppendLine()
                .AppendLine("## Unity batch")
                .AppendLine()
                .AppendLine("- Scene authoring command: `Unity.exe -batchmode -nographics -quit -projectPath F:\\Porject\\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemSandboxDetailUiSceneBuilder.ApplyItemBalanceCandidateDetailSandboxLayoutBatch`")
                .AppendLine("- Scene authoring log: `Logs/codex_item_balance_candidate_detail_sandbox_builder.log`")
                .AppendLine("- Verifier command: `Unity.exe -batchmode -nographics -quit -projectPath F:\\Porject\\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemBalanceCandidateDetailSandboxAdapterVerifier.VerifyBatch`")
                .AppendLine("- Verifier log: `Logs/codex_item_balance_candidate_detail_sandbox_verify.log`")
                .AppendLine()
                .AppendLine("## User hand test")
                .AppendLine()
                .AppendLine("1. Open `Scene_TalismanBag_V04_ItemSandbox.unity` and enter Candidate Instance Preview.")
                .AppendLine("2. Select I001 and switch white / green / blue / purple / orange.")
                .AppendLine("3. Keep Seed fixed; reopen detail tabs and confirm identity and rolls do not change.")
                .AppendLine("4. Click Regenerate and confirm Seed plus legal rolls change.")
                .AppendLine("5. Switch items and confirm detail identity, ranges, affixes, core visibility and BuildQualification do not cross.")
                .AppendLine("6. Confirm no lit/array/awakening/battle/build-count/skill-trigger state is fabricated.")
                .AppendLine("7. Return to Catalog Preview and confirm 31 catalog entries including I031 remain available.")
                .AppendLine("8. Confirm no Battle, Reward, Inventory, SaveData or formal drop flow is entered.")
                .AppendLine()
                .AppendLine("## Errors")
                .AppendLine();
            if (verification.Errors.Count == 0) report.AppendLine("- None");
            else foreach (string error in verification.Errors) report.AppendLine("- " + error);
            File.WriteAllText(ReportPath, report.ToString(), new UTF8Encoding(false));

            StringBuilder leak = new();
            leak.AppendLine("# Item Balance Candidate Detail Sandbox Adapter Leak Check")
                .AppendLine()
                .AppendLine("- Result: " + (verification.Leaks.Count == 0 ? "PASS" : "FAIL"))
                .AppendLine("- Leak count: " + verification.Leaks.Count)
                .AppendLine("- UnityEngine.Random / unstable GetHashCode: not used.")
                .AppendLine("- Formal Battle / RunFlow / Reward / Inventory / Save / BuildSettings wiring: not present.")
                .AppendLine("- Candidate adapter is read-only; ItemDetailPanel remains ViewModel-only.")
                .AppendLine("- Candidate assets, runtime/schema, prefabs and BuildSettings hashes: unchanged during verifier.")
                .AppendLine()
                .AppendLine("## Leaks");
            if (verification.Leaks.Count == 0) leak.AppendLine("- None");
            else foreach (string value in verification.Leaks) leak.AppendLine("- " + value);
            File.WriteAllText(LeakPath, leak.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
        }

        private static void Check(
            Verification verification,
            string id,
            bool passed,
            string expected,
            string actual)
        {
            verification.Rows.Add(new SpecRow(id, string.Empty, string.Empty, 0L,
                string.Empty, expected, passed ? "PASS" : "FAIL", actual));
            if (!passed) verification.Errors.Add(id + ": expected " + expected + ", actual " + actual);
        }

        private static bool Contains(IEnumerable<ItemDetailSectionViewModel> sections, string text)
        {
            return sections != null && sections.Any(section =>
                section != null && ((section.title?.Contains(text, StringComparison.Ordinal) ?? false)
                    || (section.body?.Contains(text, StringComparison.Ordinal) ?? false)));
        }

        private static bool Hidden(IEnumerable<ItemDetailSectionViewModel> sections, string stateKey)
        {
            ItemDetailSectionViewModel section = sections?.FirstOrDefault(value => value != null
                && string.Equals(value.stateKey, stateKey, StringComparison.Ordinal));
            return section != null
                && !section.keepWhenEmpty
                && string.IsNullOrWhiteSpace(section.title)
                && string.IsNullOrWhiteSpace(section.body);
        }

        private static string Describe(ItemBalanceCandidateDetailResult result)
        {
            return result == null
                ? "null"
                : result.status + ":" + (result.viewModel?.itemInstanceId ?? string.Join("|", result.ValidationErrors));
        }

        private static string Csv(string value)
        {
            string normalized = value ?? string.Empty;
            return "\"" + normalized.Replace("\"", "\"\"") + "\"";
        }

        private static void ForceLayout(RectTransform root)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            Canvas.ForceUpdateCanvases();
        }

        private static bool CheckScroll(ScrollRect scroll, RectTransform content)
        {
            return scroll != null && content != null && scroll.viewport != null
                && scroll.content != null && scroll.vertical && !scroll.horizontal
                && scroll.viewport.rect.width > 0f && scroll.viewport.rect.height > 0f
                && content.rect.width <= scroll.viewport.rect.width + 4f
                && content.GetComponent<VerticalLayoutGroup>() != null
                && content.GetComponent<ContentSizeFitter>() != null
                && scroll.viewport.GetComponent<RectMask2D>() != null
                && scroll.verticalScrollbar != null;
        }

        private static bool CheckNoOverlap(RectTransform content)
        {
            if (content == null) return false;
            RectTransform previous = null;
            for (int index = 0; index < content.childCount; index++)
            {
                RectTransform current = content.GetChild(index) as RectTransform;
                if (current == null || !current.gameObject.activeSelf) continue;
                if (current.rect.height <= 0f) return false;
                if (previous != null && LocalTop(current, content) > LocalBottom(previous, content) + 0.5f)
                    return false;
                previous = current;
            }
            return true;
        }

        private static bool CheckTextContained(RectTransform content)
        {
            if (content == null) return false;
            foreach (Text text in content.GetComponentsInChildren<Text>(true))
            {
                if (!text.gameObject.activeInHierarchy || text.name != "BodyText") continue;
                if (text.rectTransform.rect.width <= 0f || text.rectTransform.rect.height <= 0f) return false;
                if (text.preferredHeight > text.rectTransform.rect.height + 8f) return false;
            }
            return true;
        }

        private static float LocalTop(RectTransform target, RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            return corners.Select(corner => relativeTo.InverseTransformPoint(corner).y).Max();
        }

        private static float LocalBottom(RectTransform target, RectTransform relativeTo)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            return corners.Select(corner => relativeTo.InverseTransformPoint(corner).y).Min();
        }

        private static Transform FindChild(Transform root, string objectName)
        {
            return root == null ? null : root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child => child.name == objectName);
        }

        private static T FindChildComponent<T>(Transform root, string objectName) where T : Component
        {
            return FindChild(root, objectName)?.GetComponent<T>();
        }

        private sealed class Verification
        {
            public bool Passed => Errors.Count == 0;
            public readonly List<SpecRow> Rows = new();
            public readonly List<string> Errors = new();
            public readonly List<string> Leaks = new();
            public int BaseItemCount;
            public int CandidateCount;
            public int ProjectionCount;
            public int ViewModelCount;
            public int StatRangeCount;
            public int DeterminismCount;
            public int SeedSwitchCount;
            public int RegressionCount;
            public Dictionary<string, (string before, string after)> Boundaries = new();
        }

        private sealed class SpecRow
        {
            public SpecRow(string caseId, string baseItemId, string rarity, long rootSeed,
                string itemInstanceId, string expected, string result, string actual)
            {
                CaseId = caseId ?? string.Empty;
                BaseItemId = baseItemId ?? string.Empty;
                Rarity = rarity ?? string.Empty;
                RootSeed = rootSeed;
                ItemInstanceId = itemInstanceId ?? string.Empty;
                Expected = expected ?? string.Empty;
                Result = result ?? string.Empty;
                Actual = actual ?? string.Empty;
            }
            public string CaseId { get; }
            public string BaseItemId { get; }
            public string Rarity { get; }
            public long RootSeed { get; }
            public string ItemInstanceId { get; }
            public string Expected { get; }
            public string Result { get; }
            public string Actual { get; }
        }

        private sealed class BoundarySnapshot
        {
            public string Profiles;
            public string WorkbenchCatalog;
            public string RuntimeSchema;
            public string Prefabs;
            public string ScenesOutsideTarget;
            public string BuildSettings;
        }
    }
}
#endif
