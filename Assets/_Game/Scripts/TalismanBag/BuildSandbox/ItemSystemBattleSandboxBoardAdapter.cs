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
        public const string CoreLoopLabScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab.unity";
        public const string BattleSandboxHostContext = "DEV_SHOWCASE_LV40";
        public const string CoreLoopLabHostContext = "PLAYTEST_VERTICAL_SLICE";
        public const string WorkbenchCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        public const string CultivationSourceKey =
            "BattleSandboxDevHandtestAllLv40";
        private const string RuntimeObjectName =
            "ItemSystemBattleSandboxBoardAdapter_Runtime";

        private BuildGridInteractionPreviewController controller;
        private IItemSystemBattleSandboxBoardAuthority authority;
        private ItemBalanceWorkbenchCatalog installedWorkbenchCatalog;
        private ItemInnerDataCatalogProvider installedCatalogProvider;
        private ItemDetailPanelView installedAuthoredScenePanel;
        private string installedHostScenePath = string.Empty;
        private string installedHostContext = string.Empty;

        public IItemSystemBattleSandboxBoardAuthority Authority => authority;
        public string LastDiagnosticCode { get; private set; } = string.Empty;

        public static ItemSystemBattleSandboxBoardInstallDecision DecideInstallation(
            string scenePath,
            int controllerCount)
        {
            return DecideInstallation(
                scenePath,
                Application.isEditor,
                controllerCount);
        }

        public static ItemSystemBattleSandboxBoardInstallDecision DecideInstallation(
            string scenePath,
            bool isEditor,
            int controllerCount)
        {
            if (!TryResolveApprovedHost(
                    scenePath,
                    isEditor,
                    out _,
                    out _,
                    out string hostDiagnosticCode))
            {
                return new ItemSystemBattleSandboxBoardInstallDecision(
                    false, hostDiagnosticCode, string.Empty);
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

        private static bool TryResolveApprovedHost(
            string scenePath,
            bool isEditor,
            out string normalizedScenePath,
            out string hostContext,
            out string diagnosticCode)
        {
            normalizedScenePath = NormalizeSceneAssetPath(scenePath);
            hostContext = string.Empty;
            diagnosticCode = string.Empty;
            if (string.IsNullOrEmpty(normalizedScenePath))
            {
                diagnosticCode = "APPROVED_HOST_SCENE_PATH_INVALID";
                return false;
            }

            if (string.Equals(
                    normalizedScenePath,
                    TargetScenePath,
                    StringComparison.Ordinal))
            {
                hostContext = BattleSandboxHostContext;
            }
            else if (string.Equals(
                         normalizedScenePath,
                         CoreLoopLabScenePath,
                         StringComparison.Ordinal))
            {
                hostContext = CoreLoopLabHostContext;
            }
            else
            {
                diagnosticCode = "APPROVED_HOST_SCENE_REJECTED";
                return false;
            }

            if (!isEditor)
            {
                hostContext = string.Empty;
                diagnosticCode = "APPROVED_HOST_EDITOR_ONLY";
                return false;
            }

            diagnosticCode = "NONE";
            return true;
        }

        private static string NormalizeSceneAssetPath(string scenePath)
        {
            return string.IsNullOrWhiteSpace(scenePath)
                ? string.Empty
                : scenePath.Trim().Replace('\\', '/');
        }

        public bool Initialize(
            BuildGridInteractionPreviewController targetController,
            ItemBalanceWorkbenchCatalog workbenchCatalog,
            ItemInnerDataCatalogProvider catalogProvider,
            ItemDetailPanelView authoredScenePanel)
        {
            Scene adapterScene = gameObject.scene;
            if (!TryResolveApprovedHost(
                    adapterScene.path,
                    Application.isEditor,
                    out string approvedScenePath,
                    out string approvedHostContext,
                    out string hostDiagnosticCode))
            {
                return Fail(hostDiagnosticCode, "当前 Scene 不是批准的 Editor 开发宿主。");
            }
            if (!adapterScene.IsValid() || !adapterScene.isLoaded)
            {
                return Fail(
                    "APPROVED_HOST_SCENE_NOT_LOADED",
                    "批准的开发宿主 Scene 未加载。");
            }
            if (targetController == null || workbenchCatalog == null
                || catalogProvider == null || authoredScenePanel == null)
            {
                return Fail("INSTALL_DEPENDENCY_MISSING", "道具棋盘适配器依赖缺失。");
            }
            if (targetController.gameObject.scene != adapterScene
                || authoredScenePanel.gameObject.scene != adapterScene
                || catalogProvider.gameObject.scene != adapterScene)
            {
                return Fail(
                    "INSTALL_SCENE_BINDING_MISMATCH",
                    "控制器、详情面板、目录 Provider 与 adapter 必须属于同一批准 Scene。");
            }
            if (!IsExactRuntimeAdapterObject(
                    adapterScene,
                    this,
                    catalogProvider))
            {
                return Fail(
                    "ADAPTER_RUNTIME_OBJECT_INVALID",
                    "adapter 必须位于批准 Scene 内唯一的标准运行时对象上。");
            }
            if (!TryResolveApprovedSceneBindings(
                    adapterScene,
                    approvedHostContext,
                    out BuildGridInteractionPreviewController resolvedController,
                    out ItemDetailPanelView resolvedPanel,
                    out string bindingDiagnosticCode,
                    out string bindingMessage))
            {
                return Fail(bindingDiagnosticCode, bindingMessage);
            }
            if (!ReferenceEquals(resolvedController, targetController)
                || !ReferenceEquals(resolvedPanel, authoredScenePanel))
            {
                return Fail(
                    "INSTALL_SCENE_BINDING_MISMATCH",
                    "传入依赖不是批准 Scene 的唯一 authoring 绑定。");
            }

            ItemSystemBattleSandboxBoardAdapter[] sceneAdapters =
                FindSceneComponents<ItemSystemBattleSandboxBoardAdapter>(
                    adapterScene);
            if (sceneAdapters.Length != 1
                || !ReferenceEquals(sceneAdapters[0], this))
            {
                return Fail(
                    "ADAPTER_CARDINALITY_INVALID",
                    "批准 Scene 必须且只能存在一个 Item adapter。");
            }
            if (authority != null)
            {
                if (IsInitializedFor(
                        adapterScene,
                        approvedScenePath,
                        approvedHostContext,
                        targetController,
                        workbenchCatalog,
                        catalogProvider,
                        authoredScenePanel))
                {
                    LastDiagnosticCode = "ADAPTER_ALREADY_INITIALIZED_NOOP";
                    return true;
                }
                return Fail(
                    "ADAPTER_INITIALIZED_BINDING_MISMATCH",
                    "已初始化 adapter 的宿主或依赖不匹配。");
            }

            ItemSystemBattleSandboxItemDetailAdapter createdDetailAdapter = null;
            IItemSystemBattleSandboxBoardAuthority createdAuthority = null;
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

                createdAuthority = new ItemSystemBattleSandboxBoardAuthority(
                        projection,
                        TalismanBag.Items.DefaultItemSystemSnapshotProvider.Instance,
                        ItemInstancePlacementBindingValidator.Instance,
                        identityCatalog,
                        cultivationRoster,
                        DefaultRealLayoutResilienceEvaluationPipeline.Instance);
                if (!targetController.InstallItemSystemBattleSandboxBoardAuthority(
                        createdAuthority,
                        createdDetailAdapter,
                        out string controllerDiagnostic))
                {
                    targetController.UninstallItemSystemBattleSandboxBoardAuthority(
                        createdAuthority);
                    createdDetailAdapter.Uninstall();
                    return Fail(
                        string.IsNullOrWhiteSpace(controllerDiagnostic)
                            ? "CONTROLLER_AUTHORITY_INSTALL_REJECTED"
                            : controllerDiagnostic,
                        "棋盘控制器拒绝安装 ItemSystem 权威。");
                }

                controller = targetController;
                authority = createdAuthority;
                installedWorkbenchCatalog = workbenchCatalog;
                installedCatalogProvider = catalogProvider;
                installedAuthoredScenePanel = authoredScenePanel;
                installedHostScenePath = approvedScenePath;
                installedHostContext = approvedHostContext;
                LastDiagnosticCode = "NONE";
                return true;
            }
            catch (Exception exception)
            {
                if (createdAuthority != null)
                {
                    targetController.UninstallItemSystemBattleSandboxBoardAuthority(
                        createdAuthority);
                }
                createdDetailAdapter?.Uninstall();
                return Fail("INSTALL_EXCEPTION_" + exception.GetType().Name,
                    "道具棋盘适配器初始化失败。");
            }
        }

        private bool IsInitializedFor(
            Scene approvedScene,
            string approvedScenePath,
            string approvedHostContext,
            BuildGridInteractionPreviewController expectedController,
            ItemBalanceWorkbenchCatalog expectedWorkbenchCatalog,
            ItemInnerDataCatalogProvider expectedCatalogProvider,
            ItemDetailPanelView expectedAuthoredPanel)
        {
            return authority != null
                && ReferenceEquals(controller, expectedController)
                && ReferenceEquals(
                    installedWorkbenchCatalog,
                    expectedWorkbenchCatalog)
                && ReferenceEquals(
                    installedCatalogProvider,
                    expectedCatalogProvider)
                && ReferenceEquals(
                    installedAuthoredScenePanel,
                    expectedAuthoredPanel)
                && string.Equals(
                    installedHostScenePath,
                    approvedScenePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    installedHostContext,
                    approvedHostContext,
                    StringComparison.Ordinal)
                && IsExactRuntimeAdapterObject(
                    approvedScene,
                    this,
                    expectedCatalogProvider);
        }

        private static bool TryResolveApprovedSceneBindings(
            Scene scene,
            string expectedHostContext,
            out BuildGridInteractionPreviewController resolvedController,
            out ItemDetailPanelView resolvedPanel,
            out string diagnosticCode,
            out string diagnosticMessage)
        {
            resolvedController = null;
            resolvedPanel = null;
            if (!TryResolveApprovedHost(
                    scene.path,
                    Application.isEditor,
                    out _,
                    out string resolvedHostContext,
                    out diagnosticCode))
            {
                diagnosticMessage = "当前 Scene 不是批准的 Editor 开发宿主。";
                return false;
            }
            if (!string.Equals(
                    resolvedHostContext,
                    expectedHostContext,
                    StringComparison.Ordinal))
            {
                diagnosticCode = "APPROVED_HOST_CONTEXT_MISMATCH";
                diagnosticMessage = "批准 Scene 的产品上下文不匹配。";
                return false;
            }
            if (!scene.IsValid() || !scene.isLoaded)
            {
                diagnosticCode = "APPROVED_HOST_SCENE_NOT_LOADED";
                diagnosticMessage = "批准的开发宿主 Scene 未加载。";
                return false;
            }

            BuildGridInteractionPreviewController[] controllers =
                FindSceneComponents<BuildGridInteractionPreviewController>(scene);
            ItemSystemBattleSandboxBoardInstallDecision controllerDecision =
                DecideInstallation(scene.path, Application.isEditor, controllers.Length);
            if (!controllerDecision.ShouldInstall)
            {
                diagnosticCode = controllerDecision.DiagnosticCode;
                diagnosticMessage = controllerDecision.ChineseMessage;
                return false;
            }

            Transform[] sceneTransforms = FindSceneComponents<Transform>(scene);
            Transform[] popupLayers = sceneTransforms
                .Where(value => value != null && string.Equals(
                    value.name,
                    "PopupLayer",
                    StringComparison.Ordinal))
                .ToArray();
            if (popupLayers.Length != 1)
            {
                diagnosticCode = "DETAIL_POPUP_LAYER_CARDINALITY_INVALID";
                diagnosticMessage = "批准 Scene 必须且只能存在一个 PopupLayer。";
                return false;
            }

            Transform[] detailRoots = sceneTransforms
                .Where(value => value != null && string.Equals(
                    value.name,
                    "ItemDetailPanel",
                    StringComparison.Ordinal))
                .ToArray();
            if (detailRoots.Length != 1
                || detailRoots[0].parent != popupLayers[0])
            {
                diagnosticCode = "DETAIL_AUTHORED_ROOT_CARDINALITY_INVALID";
                diagnosticMessage =
                    "批准 Scene 的 PopupLayer 必须且只能直属一个 ItemDetailPanel。";
                return false;
            }

            ItemDetailPanelView[] rootViews =
                detailRoots[0].GetComponents<ItemDetailPanelView>();
            ItemDetailPanelView[] sceneViews =
                FindSceneComponents<ItemDetailPanelView>(scene);
            if (rootViews.Length != 1 || sceneViews.Length != 1
                || !ReferenceEquals(rootViews[0], sceneViews[0]))
            {
                diagnosticCode = "DETAIL_AUTHORED_VIEW_CARDINALITY_INVALID";
                diagnosticMessage =
                    "批准 Scene 必须且只能存在一个 authoring ItemDetailPanelView。";
                return false;
            }

            resolvedController = controllers[0];
            resolvedPanel = sceneViews[0];
            diagnosticCode = "NONE";
            diagnosticMessage = string.Empty;
            return true;
        }

        private static T[] FindSceneComponents<T>(Scene scene)
            where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return Array.Empty<T>();
            }
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .Where(value => value != null && value.gameObject.scene == scene)
                .ToArray();
        }

        private static bool IsExactRuntimeAdapterObject(
            Scene approvedScene,
            ItemSystemBattleSandboxBoardAdapter adapter,
            ItemInnerDataCatalogProvider expectedCatalogProvider)
        {
            if (adapter == null || expectedCatalogProvider == null
                || adapter.gameObject.scene != approvedScene
                || expectedCatalogProvider.gameObject != adapter.gameObject
                || adapter.transform.parent != null
                || !string.Equals(
                    adapter.gameObject.name,
                    RuntimeObjectName,
                    StringComparison.Ordinal))
            {
                return false;
            }

            ItemSystemBattleSandboxBoardAdapter[] adapters =
                adapter.gameObject
                    .GetComponents<ItemSystemBattleSandboxBoardAdapter>();
            ItemInnerDataCatalogProvider[] providers =
                adapter.gameObject.GetComponents<ItemInnerDataCatalogProvider>();
            return adapters.Length == 1
                && ReferenceEquals(adapters[0], adapter)
                && providers.Length == 1
                && ReferenceEquals(providers[0], expectedCatalogProvider);
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
            installedWorkbenchCatalog = null;
            installedCatalogProvider = null;
            installedAuthoredScenePanel = null;
            installedHostScenePath = string.Empty;
            installedHostContext = string.Empty;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterPlayBootstrap()
        {
            SceneManager.sceneLoaded -= OnPlaySceneLoaded;
            SceneManager.sceneLoaded += OnPlaySceneLoaded;
        }

        private static void OnPlaySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!TryResolveApprovedHost(
                    scene.path,
                    Application.isEditor,
                    out string approvedScenePath,
                    out string approvedHostContext,
                    out _))
            {
                return;
            }
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                LogBootstrapRejection(
                    "APPROVED_HOST_PLAY_MODE_REQUIRED",
                    "批准宿主只能在 Editor Play Mode 安装 Item runtime。",
                    approvedScenePath);
                return;
            }
#endif
            if (!TryResolveApprovedSceneBindings(
                    scene,
                    approvedHostContext,
                    out BuildGridInteractionPreviewController controller,
                    out ItemDetailPanelView authoredPanel,
                    out string bindingDiagnosticCode,
                    out string bindingMessage))
            {
                LogBootstrapRejection(
                    bindingDiagnosticCode,
                    bindingMessage,
                    approvedScenePath);
                return;
            }

            ItemSystemBattleSandboxBoardAdapter[] existingAdapters =
                FindSceneComponents<ItemSystemBattleSandboxBoardAdapter>(scene);
            if (existingAdapters.Length > 1)
            {
                LogBootstrapRejection(
                    "ADAPTER_CARDINALITY_INVALID",
                    "批准 Scene 存在多个 Item adapter。",
                    approvedScenePath);
                return;
            }
            if (existingAdapters.Length == 1)
            {
                ItemSystemBattleSandboxBoardAdapter existingAdapter =
                    existingAdapters[0];
                if (existingAdapter.IsInitializedFor(
                        scene,
                        approvedScenePath,
                        approvedHostContext,
                        controller,
                        existingAdapter.installedWorkbenchCatalog,
                        existingAdapter.installedCatalogProvider,
                        authoredPanel))
                {
                    existingAdapter.LastDiagnosticCode =
                        "ADAPTER_ALREADY_INITIALIZED_NOOP";
                    return;
                }
                LogBootstrapRejection(
                    "ADAPTER_EXISTING_STATE_REJECTED",
                    "现有 Item adapter 未初始化或宿主绑定不匹配。",
                    approvedScenePath);
                return;
            }

            ItemBalanceWorkbenchCatalog workbench =
                ResolveRuntimeWorkbenchCatalog();
            if (workbench == null)
            {
                LogBootstrapRejection(
                    "WORKBENCH_CATALOG_RUNTIME_MISSING",
                    "Editor 开发宿主缺少 ItemBalanceWorkbenchCatalog。",
                    approvedScenePath);
                return;
            }

            GameObject runtimeObject = null;
            try
            {
                runtimeObject = new GameObject(RuntimeObjectName);
                runtimeObject.hideFlags = HideFlags.DontSaveInEditor
                    | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;
                SceneManager.MoveGameObjectToScene(runtimeObject, scene);
                ItemInnerDataCatalogProvider provider =
                    runtimeObject.AddComponent<ItemInnerDataCatalogProvider>();
                provider.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                ItemSystemBattleSandboxBoardAdapter adapter =
                    runtimeObject.AddComponent<ItemSystemBattleSandboxBoardAdapter>();
                adapter.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                if (!adapter.Initialize(
                        controller,
                        workbench,
                        provider,
                        authoredPanel))
                {
                    DestroyCreatedRuntimeObject(runtimeObject);
                }
            }
            catch (Exception exception)
            {
                DestroyCreatedRuntimeObject(runtimeObject);
                LogBootstrapRejection(
                    "BOOTSTRAP_EXCEPTION_" + exception.GetType().Name,
                    "Item installer 创建运行时宿主失败。",
                    approvedScenePath);
            }
        }

        private static void DestroyCreatedRuntimeObject(GameObject runtimeObject)
        {
            if (runtimeObject != null)
            {
                UnityEngine.Object.DestroyImmediate(runtimeObject);
            }
        }

        private static void LogBootstrapRejection(
            string diagnosticCode,
            string diagnosticMessage,
            string scenePath)
        {
            Debug.LogError("[ItemSystemBattleSandboxBoardAdapter]["
                + (string.IsNullOrWhiteSpace(diagnosticCode)
                    ? "INSTALL_INVALID"
                    : diagnosticCode)
                + "] " + (diagnosticMessage ?? string.Empty)
                + " scene=" + NormalizeSceneAssetPath(scenePath));
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
