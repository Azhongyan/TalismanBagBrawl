using System;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.BuildSandbox
{
    public sealed class ItemSystemBattleSandboxBoardInstallDecision
    {
        internal ItemSystemBattleSandboxBoardInstallDecision(
            bool shouldInstall,
            string diagnosticCode,
            string chineseMessage)
        {
            ShouldInstall = shouldInstall;
            DiagnosticCode = diagnosticCode ?? string.Empty;
            ChineseMessage = chineseMessage ?? string.Empty;
        }

        public bool ShouldInstall { get; }
        public string DiagnosticCode { get; }
        public string ChineseMessage { get; }
    }

    [DisallowMultipleComponent]
    public sealed class ItemSystemBattleSandboxBoardAdapter : MonoBehaviour
    {
        public const string PackageKey =
            "V0.4-ItemSystemBattleSandboxBoardAdapter01";
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        public const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        public const string CultivationSourceKey =
            "BattleSandboxDevHandtestAllLv40";
        private const string RuntimeObjectName =
            "ItemSystemBattleSandboxBoardAdapter_Runtime";

        private BuildGridInteractionPreviewController controller;
        private IItemSystemBattleSandboxBoardAuthority authority;

        public IItemSystemBattleSandboxBoardAuthority Authority => authority;
        public string LastDiagnosticCode { get; private set; } = string.Empty;

        public static ItemSystemBattleSandboxBoardInstallDecision DecideInstallation(
            string scenePath,
            int controllerCount)
        {
            if (!string.Equals(scenePath, TargetScenePath, StringComparison.Ordinal))
            {
                return new ItemSystemBattleSandboxBoardInstallDecision(
                    false, "TARGET_SCENE_REJECTED", string.Empty);
            }
            if (controllerCount == 0)
            {
                return new ItemSystemBattleSandboxBoardInstallDecision(
                    false,
                    "BATTLE_SANDBOX_CONTROLLER_MISSING",
                    "目标战斗沙盒缺少唯一棋盘控制器。");
            }
            if (controllerCount != 1)
            {
                return new ItemSystemBattleSandboxBoardInstallDecision(
                    false,
                    "BATTLE_SANDBOX_CONTROLLER_DUPLICATE",
                    "目标战斗沙盒存在重复棋盘控制器。");
            }
            return new ItemSystemBattleSandboxBoardInstallDecision(
                true, "NONE", string.Empty);
        }

        public bool Initialize(
            BuildGridInteractionPreviewController targetController,
            ItemBalanceWorkbenchCatalog workbenchCatalog,
            ItemInnerDataCatalogProvider catalogProvider,
            ItemDetailPanelView authoredScenePanel)
        {
            if (targetController == null || workbenchCatalog == null
                || catalogProvider == null || authoredScenePanel == null
                || !string.Equals(authoredScenePanel.gameObject.scene.path,
                    TargetScenePath, StringComparison.Ordinal))
            {
                return Fail("INSTALL_DEPENDENCY_MISSING", "道具棋盘适配器依赖缺失。");
            }
            if (authority != null)
            {
                return Fail("ADAPTER_ALREADY_INITIALIZED", "道具棋盘适配器重复初始化。");
            }

            ItemSystemBattleSandboxItemDetailAdapter createdDetailAdapter = null;
            try
            {
                ItemSystemBattleSandboxViewProjectionResult projection =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbenchCatalog, catalogProvider);
                if (!projection.IsValid)
                {
                    string code = projection.Diagnostics.FirstOrDefault()
                        ?? "VIEW_PROJECTION_INVALID";
                    return Fail(code, "道具名册、实例或共用图标加载失败。");
                }

                ItemCoreEffectIdentityCatalogSnapshot identityCatalog =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbenchCatalog,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                if (!identityCatalog.isValid
                    || identityCatalog.Rows.Count != 120
                    || !string.Equals(
                        identityCatalog.schemaId,
                        ItemCoreEffectIdentityCatalogSnapshot.CurrentSchemaId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        identityCatalog.authorityRevision,
                        ItemCoreEffectIdentityCatalogSnapshot
                            .CurrentAuthorityRevision,
                        StringComparison.Ordinal))
                {
                    string code = identityCatalog.ValidationErrors
                        .FirstOrDefault()?.code
                        ?? "CORE_EFFECT_IDENTITY_CATALOG_INVALID";
                    return Fail(code, "核心效果身份目录未通过校验。");
                }
                ItemCoreEffectCultivationRosterSnapshot cultivationRoster =
                    new(
                        ItemCoreEffectRosterCompleteness.Complete,
                        projection.OrdinaryProjectionSet.Projections
                            .Select(value => new ItemCoreEffectCultivationRow(
                                value.itemInstanceId,
                                value.baseItemId,
                                40,
                                CultivationSourceKey,
                                ItemCoreEffectFactCompleteness.Complete)));
                if (!cultivationRoster.isValid
                    || cultivationRoster.Rows.Count != 30)
                {
                    string code = cultivationRoster.ValidationErrors
                        .FirstOrDefault()?.code
                        ?? "CORE_EFFECT_CULTIVATION_ROSTER_INVALID";
                    return Fail(code, "核心效果手测等级名册未通过校验。");
                }

                int requiredTrayCells = projection.Rows
                    .Where(row => row != null)
                    .Sum(row => row.ShapeCells.Count);
                int availableTrayCells = BuildItemTrayPreviewView
                    .ItemSystemAuthorityLogicalSlotCount;
                if (requiredTrayCells > availableTrayCells)
                {
                    return Fail(
                        "ITEM_SYSTEM_TRAY_CAPACITY_INVALID",
                        "现有道具栏容量不足，无法安全安装完整 I001-I031 名册。");
                }

                createdDetailAdapter = new ItemSystemBattleSandboxItemDetailAdapter();
                if (!createdDetailAdapter.Initialize(authoredScenePanel, projection.Rows))
                {
                    return Fail(createdDetailAdapter.LastDiagnosticCode,
                        "道具详情面板依赖未通过校验。");
                }

                IItemSystemBattleSandboxBoardAuthority created =
                    new ItemSystemBattleSandboxBoardAuthority(
                        projection,
                        TalismanBag.Items.DefaultItemSystemSnapshotProvider.Instance,
                        ItemInstancePlacementBindingValidator.Instance,
                        identityCatalog,
                        cultivationRoster,
                        DefaultRealLayoutResilienceEvaluationPipeline.Instance);
                if (!targetController.InstallItemSystemBattleSandboxBoardAuthority(
                        created, createdDetailAdapter, out string controllerDiagnostic))
                {
                    createdDetailAdapter.Uninstall();
                    return Fail(
                        string.IsNullOrWhiteSpace(controllerDiagnostic)
                            ? "CONTROLLER_AUTHORITY_INSTALL_REJECTED"
                            : controllerDiagnostic,
                        "棋盘控制器拒绝安装 ItemSystem 权威。");
                }

                controller = targetController;
                authority = created;
                LastDiagnosticCode = "NONE";
                return true;
            }
            catch (Exception exception)
            {
                createdDetailAdapter?.Uninstall();
                return Fail("INSTALL_EXCEPTION_" + exception.GetType().Name,
                    "道具棋盘适配器初始化失败。");
            }
        }

        private bool Fail(string code, string chineseMessage)
        {
            LastDiagnosticCode = code ?? "INSTALL_INVALID";
            Debug.LogError("[ItemSystemBattleSandboxBoardAdapter]["
                + LastDiagnosticCode + "] " + (chineseMessage ?? string.Empty), this);
            return false;
        }

        private void OnDestroy()
        {
            if (controller != null && authority != null)
            {
                controller.UninstallItemSystemBattleSandboxBoardAuthority(authority);
            }
            controller = null;
            authority = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterPlayBootstrap()
        {
            SceneManager.sceneLoaded -= OnPlaySceneLoaded;
            SceneManager.sceneLoaded += OnPlaySceneLoaded;
        }

        private static void OnPlaySceneLoaded(Scene scene, LoadSceneMode mode)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return;
            }
#endif
            if (!string.Equals(
                    scene.path,
                    TargetScenePath,
                    StringComparison.Ordinal))
            {
                return;
            }

            BuildGridInteractionPreviewController[] controllers =
                UnityEngine.Object
                    .FindObjectsOfType<BuildGridInteractionPreviewController>(true)
                    .Where(value => value != null && value.gameObject.scene == scene)
                    .ToArray();
            ItemSystemBattleSandboxBoardInstallDecision decision =
                DecideInstallation(scene.path, controllers.Length);
            if (!decision.ShouldInstall)
            {
                Debug.LogError("[ItemSystemBattleSandboxBoardAdapter]["
                    + decision.DiagnosticCode + "] " + decision.ChineseMessage);
                return;
            }

            Transform[] popupLayers = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(value => value != null && string.Equals(
                    value.name, "PopupLayer", StringComparison.Ordinal))
                .ToArray();
            if (popupLayers.Length != 1)
            {
                Debug.LogError("[ItemSystemBattleSandboxBoardAdapter]"
                    + "[DETAIL_POPUP_LAYER_CARDINALITY_INVALID] "
                    + "目标战斗沙盒缺少唯一 PopupLayer。");
                return;
            }

            Transform[] detailRoots = popupLayers[0].Cast<Transform>()
                .Where(value => value != null && string.Equals(
                    value.name, "ItemDetailPanel", StringComparison.Ordinal))
                .ToArray();
            if (detailRoots.Length != 1)
            {
                Debug.LogError("[ItemSystemBattleSandboxBoardAdapter]"
                    + "[DETAIL_AUTHORED_ROOT_CARDINALITY_INVALID] "
                    + "目标战斗沙盒缺少唯一现成道具详情面板。");
                return;
            }

            ItemDetailPanelView[] rootViews =
                detailRoots[0].GetComponents<ItemDetailPanelView>();
            ItemDetailPanelView[] sceneViews = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<ItemDetailPanelView>(true))
                .Where(value => value != null)
                .ToArray();
            if (rootViews.Length != 1 || sceneViews.Length != 1
                || !ReferenceEquals(rootViews[0], sceneViews[0]))
            {
                Debug.LogError("[ItemSystemBattleSandboxBoardAdapter]"
                    + "[DETAIL_AUTHORED_VIEW_CARDINALITY_INVALID] "
                    + "目标战斗沙盒的道具详情视图不唯一。");
                return;
            }

            ItemBalanceWorkbenchCatalog workbench =
                ResolveRuntimeWorkbenchCatalog();
            if (workbench == null)
            {
                Debug.LogError("[ItemSystemBattleSandboxBoardAdapter]"
                    + "[WORKBENCH_CATALOG_RUNTIME_MISSING] "
                    + "BattleSandbox Player 缺少 ItemBalanceWorkbenchCatalog；"
                    + "请使用 V0.4 BattleSandbox Preview APK 构建菜单。");
                return;
            }

            GameObject runtimeObject = new(RuntimeObjectName);
            runtimeObject.hideFlags = HideFlags.DontSaveInEditor
                | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
            SceneManager.MoveGameObjectToScene(runtimeObject, scene);
            ItemInnerDataCatalogProvider provider =
                runtimeObject.AddComponent<ItemInnerDataCatalogProvider>();
            provider.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            ItemSystemBattleSandboxBoardAdapter adapter =
                runtimeObject.AddComponent<ItemSystemBattleSandboxBoardAdapter>();
            adapter.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            if (!adapter.Initialize(controllers[0], workbench, provider,
                    rootViews[0]))
            {
                UnityEngine.Object.Destroy(runtimeObject);
            }
        }

        private static ItemBalanceWorkbenchCatalog
            ResolveRuntimeWorkbenchCatalog()
        {
#if UNITY_EDITOR
            ItemBalanceWorkbenchCatalog editorCatalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchCatalogPath);
            if (editorCatalog != null)
            {
                return editorCatalog;
            }
#endif
            return Resources
                .FindObjectsOfTypeAll<ItemBalanceWorkbenchCatalog>()
                .Where(value => value != null
                    && string.Equals(
                        value.name,
                        nameof(ItemBalanceWorkbenchCatalog),
                        StringComparison.Ordinal))
                .Distinct()
                .SingleOrDefault();
        }
    }
}
