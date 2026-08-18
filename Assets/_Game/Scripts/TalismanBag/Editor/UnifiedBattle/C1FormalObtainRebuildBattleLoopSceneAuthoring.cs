using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.Items.Balance;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.UnifiedBattle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class C1FormalObtainRebuildBattleLoopSceneAuthoring
    {
        private const string UnifiedPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";
        private const string UnifiedScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        private const string ApproximateBoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemBoard.prefab";
        private const string ApproximateTrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemTray.prefab";
        private const string ExactBoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab";
        private const string ExactTrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab";
        private const string ExactCardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab";
        private const string CanonicalItemCatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/C1FormalObtainRebuildBattleLoop.cs",
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "PlayerPrefs",
            "new GameObject(",
            "GameObject.Find",
            "FindObjectOfType",
            "FindObjectsOfType",
            "OnGUI(",
            "ItemDevSessionRosterAvailability",
            "SceneManager.GetActiveScene().name"
        };

        [MenuItem("TalismanBag/V0.4/Unified Battle/Apply C1 Formal Loop Once")]
        public static void ApplyFromMenu()
        {
            ApplySingleAuthoringPass();
        }

        [MenuItem("TalismanBag/V0.4/Unified Battle/Bind Canonical Catalog To Formal Shell")]
        public static void BindCanonicalCatalogFromMenu()
        {
            BindCanonicalCatalogToUnifiedPrefab();
        }

        public static void BindCanonicalCatalogFromCommandLine()
        {
            try
            {
                BindCanonicalCatalogToUnifiedPrefab();
                Debug.Log(
                    "[C1FormalLoopAuthoring] CANONICAL_CATALOG_BINDING_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1FormalLoopAuthoring] CANONICAL_CATALOG_BINDING_FAIL " +
                    exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ApplySingleAuthoringPass();
                Debug.Log("[C1FormalLoopAuthoring] AUTHORING_AND_TESTS_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1FormalLoopAuthoring] AUTHORING_OR_TESTS_FAIL " +
                    exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void DiagnoseFormalShellBindingsFromCommandLine()
        {
            try
            {
                ValidateFormalShellPrefabReadOnly();
                ValidateFormalShellSceneReadOnly();
                Debug.Log(
                    "[C1FormalLoopAuthoring] " +
                    "FORMAL_SHELL_AUTHORED_BINDING_DIAGNOSIS_PASS " +
                    "prefab=1 scene=1 saved=0");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1FormalLoopAuthoring] " +
                    "FORMAL_SHELL_AUTHORED_BINDING_DIAGNOSIS_FAIL " +
                    exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateFormalShellPrefabReadOnly()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                UnifiedPrefabPath);
            Require(root != null, "FORMAL_SHELL_PREFAB_LOAD_FAILED");
            try
            {
                UnifiedBattleFormalSceneHost[] hosts =
                    root.GetComponentsInChildren<
                        UnifiedBattleFormalSceneHost>(true);
                Require(hosts.Length == 1,
                    "FORMAL_SHELL_PREFAB_HOST_COUNT_INVALID count=" +
                    hosts.Length);
                Require(hosts[0].ValidateAuthoredBindings(
                        out string diagnostic),
                    "FORMAL_SHELL_PREFAB_BINDING_REJECTED " + diagnostic);
                Debug.Log(
                    "[C1FormalLoopAuthoring] " +
                    "FORMAL_SHELL_PREFAB_BINDING_PASS saved=0");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateFormalShellSceneReadOnly()
        {
            Scene scene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            UnifiedBattleFormalSceneHost[] hosts =
                SceneComponents<UnifiedBattleFormalSceneHost>(scene);
            Require(hosts.Length == 1,
                "FORMAL_SHELL_SCENE_HOST_COUNT_INVALID count=" + hosts.Length);
            Require(hosts[0].ValidateAuthoredBindings(
                    out string diagnostic),
                "FORMAL_SHELL_SCENE_BINDING_REJECTED " + diagnostic);
            Require(!scene.isDirty,
                "FORMAL_SHELL_SCENE_DIAGNOSIS_DIRTIED_SCENE");
            Debug.Log(
                "[C1FormalLoopAuthoring] " +
                "FORMAL_SHELL_SCENE_BINDING_PASS saved=0");
        }

        private static void ApplySingleAuthoringPass()
        {
            C1FormalObtainRebuildBattleLoopTests.RunAllOrThrow();

            Scene preflightScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            ExactCarrierLayoutBaseline layoutBaseline =
                ValidateScenePreflight(preflightScene);

            AuthorUnifiedPrefab();
            AssetDatabase.ImportAsset(
                UnifiedPrefabPath,
                ImportAssetOptions.ForceUpdate |
                ImportAssetOptions.ForceSynchronousImport);

            Scene authoringScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            AuthorUnifiedScene(authoringScene, layoutBaseline);
            EditorSceneManager.MarkSceneDirty(authoringScene);
            Require(EditorSceneManager.SaveScene(authoringScene),
                "UNIFIED_EXACT_ITEM_SCENE_SAVE_FAILED");
            AssetDatabase.ImportAsset(
                UnifiedScenePath,
                ImportAssetOptions.ForceUpdate |
                ImportAssetOptions.ForceSynchronousImport);

            Scene validationScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            ValidateFinalScene(validationScene, layoutBaseline);
            ValidateStaticRuntimeBoundaries();
            Debug.Log(
                "[C1FormalLoopAuthoring] DIRECT_ITEM_MODULE_STATIC_VALID " +
                "exactBoard=1 exactTray=1 exactPresenter=1 " +
                "dynamicItemProvider=1 " +
                "approximateVisible=0 missingScripts=0");
        }

        private static void AuthorUnifiedPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(UnifiedPrefabPath);
            Require(root != null, "UNIFIED_EXACT_ITEM_PREFAB_LOAD_FAILED");
            try
            {
                UnifiedBattlePageShell shell =
                    root.GetComponent<UnifiedBattlePageShell>();
                UnifiedBattlePageShellMarker marker =
                    root.GetComponent<UnifiedBattlePageShellMarker>();
                Require(shell != null && marker != null,
                    "UNIFIED_EXACT_ITEM_PREFAB_ROOT_INVALID");
                IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
                Transform boardSlot = RequiredSlot(
                    slots,
                    UnifiedBattlePageShellSlotNames.BoardArea);
                Transform traySlot = RequiredSlot(
                    slots,
                    UnifiedBattlePageShellSlotNames.ItemTrayArea);
                Transform actionSlot = RequiredSlot(
                    slots,
                    UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot);

                Require(root.GetComponentsInChildren<
                            C1ExactBattleSandboxItemBoardView>(true).Length == 0
                        && root.GetComponentsInChildren<
                            C1ExactBattleSandboxItemTrayView>(true).Length == 0
                        && root.GetComponentsInChildren<
                            C1ExactBattleSandboxItemArrangementPresenter>(true)
                            .Length == 0,
                    "UNIFIED_EXACT_ITEM_DUPLICATED_IN_SHELL_PREFAB");

                C1FormalItemBoardView[] approximateBoards =
                    root.GetComponentsInChildren<C1FormalItemBoardView>(true);
                C1FormalItemTrayView[] approximateTrays =
                    root.GetComponentsInChildren<C1FormalItemTrayView>(true);
                C1FormalItemArrangementPresenter[] approximatePresenters =
                    root.GetComponentsInChildren<
                        C1FormalItemArrangementPresenter>(true);
                Require(approximateBoards.Length == 1
                        && approximateTrays.Length == 1
                        && approximatePresenters.Length <= 1,
                    "UNIFIED_APPROXIMATE_ITEM_BASELINE_AMBIGUOUS");
                ValidateKnownItemPresenters(root, approximatePresenters.Length, 0);

                GameObject approximateBoardRoot = RequiredNestedPrefabRoot(
                    approximateBoards[0],
                    ApproximateBoardPrefabPath);
                GameObject approximateTrayRoot = RequiredNestedPrefabRoot(
                    approximateTrays[0],
                    ApproximateTrayPrefabPath);
                Require(approximateBoardRoot.transform.parent == boardSlot
                        && approximateTrayRoot.transform.parent == traySlot,
                    "UNIFIED_APPROXIMATE_ITEM_MOUNT_PARENT_INVALID");

                approximateBoardRoot.SetActive(false);
                approximateTrayRoot.SetActive(false);
                if (approximatePresenters.Length == 1)
                {
                    Require(approximatePresenters[0].transform == actionSlot,
                        "UNIFIED_APPROXIMATE_ITEM_PRESENTER_PARENT_INVALID");
                    Object.DestroyImmediate(approximatePresenters[0]);
                }

                DisablePlaceholderRendering(boardSlot);
                DisablePlaceholderRendering(traySlot);

                UnifiedBattleFormalSceneHost host =
                    actionSlot.GetComponent<UnifiedBattleFormalSceneHost>();
                Button button = actionSlot.GetComponent<Button>();
                Text summary = DirectChild(actionSlot, "SummaryText")
                    ?.GetComponent<Text>();
                Text actionLabel = DirectChild(actionSlot, "Title")
                    ?.GetComponent<Text>();
                Require(host != null && button != null && summary != null
                        && actionLabel != null,
                    "UNIFIED_FORMAL_ACTION_SURFACE_MISSING");
                host.AssignForEditor(
                    shell,
                    marker,
                    summary,
                    button,
                    actionLabel,
                    null,
                    null,
                    null);
                host.AssignCanonicalItemCatalogForEditor(
                    LoadCanonicalItemCatalog());
                EditorUtility.SetDirty(host);

                Require(!approximateBoardRoot.activeSelf
                        && !approximateTrayRoot.activeSelf,
                    "UNIFIED_APPROXIMATE_ITEM_MOUNT_STILL_ACTIVE");
                Require(root.GetComponentsInChildren<
                            C1FormalItemArrangementPresenter>(true).Length == 0,
                    "UNIFIED_APPROXIMATE_ITEM_PRESENTER_STILL_PRESENT");
                ValidateKnownItemPresenters(root, 0, 0);
                ValidateTransparentSlot(boardSlot);
                ValidateTransparentSlot(traySlot);
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "UNIFIED_EXACT_ITEM_PREFAB_MISSING_SCRIPT");

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    UnifiedPrefabPath);
                Require(saved != null,
                    "UNIFIED_EXACT_ITEM_PREFAB_SAVE_FAILED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BindCanonicalCatalogToUnifiedPrefab()
        {
            ItemBalanceWorkbenchCatalog canonicalCatalog =
                LoadCanonicalItemCatalog();
            GameObject root = PrefabUtility.LoadPrefabContents(UnifiedPrefabPath);
            Require(root != null,
                "UNIFIED_FORMAL_CANONICAL_CATALOG_PREFAB_LOAD_FAILED");
            try
            {
                UnifiedBattleFormalSceneHost[] hosts =
                    root.GetComponentsInChildren<UnifiedBattleFormalSceneHost>(true);
                Require(hosts.Length == 1,
                    "UNIFIED_FORMAL_CANONICAL_CATALOG_HOST_COUNT count=" +
                    hosts.Length);
                hosts[0].AssignCanonicalItemCatalogForEditor(canonicalCatalog);
                EditorUtility.SetDirty(hosts[0]);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    UnifiedPrefabPath);
                Require(saved != null,
                    "UNIFIED_FORMAL_CANONICAL_CATALOG_PREFAB_SAVE_FAILED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.ImportAsset(
                UnifiedPrefabPath,
                ImportAssetOptions.ForceUpdate |
                ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                UnifiedPrefabPath);
            Require(prefab != null,
                "UNIFIED_FORMAL_CANONICAL_CATALOG_PREFAB_RELOAD_FAILED");
            UnifiedBattleFormalSceneHost[] persistedHosts =
                prefab.GetComponentsInChildren<UnifiedBattleFormalSceneHost>(true);
            Require(persistedHosts.Length == 1,
                "UNIFIED_FORMAL_CANONICAL_CATALOG_PERSISTED_HOST_COUNT count=" +
                persistedHosts.Length);
            SerializedObject serializedHost = new SerializedObject(
                persistedHosts[0]);
            SerializedProperty catalogProperty = serializedHost.FindProperty(
                "canonicalItemCatalogSource");
            Require(catalogProperty != null
                    && catalogProperty.objectReferenceValue == canonicalCatalog,
                "UNIFIED_FORMAL_CANONICAL_CATALOG_BINDING_NOT_PERSISTED");
        }

        private static ItemBalanceWorkbenchCatalog LoadCanonicalItemCatalog()
        {
            ItemBalanceWorkbenchCatalog canonicalCatalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    CanonicalItemCatalogPath);
            Require(canonicalCatalog != null,
                "UNIFIED_FORMAL_CANONICAL_ITEM_CATALOG_SOURCE_MISSING");
            return canonicalCatalog;
        }

        private static ExactCarrierLayoutBaseline ValidateScenePreflight(
            Scene scene)
        {
            Require(scene.IsValid() && scene.isLoaded,
                "UNIFIED_EXACT_ITEM_PREFLIGHT_SCENE_NOT_LOADED");
            UnifiedBattlePageShell shell = SingleInScene<UnifiedBattlePageShell>(scene);
            SingleInScene<UnifiedBattleFormalSceneHost>(scene);
            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
            C1ExactBattleSandboxItemBoardView board =
                SingleInScene<C1ExactBattleSandboxItemBoardView>(scene);
            C1ExactBattleSandboxItemTrayView tray =
                SingleInScene<C1ExactBattleSandboxItemTrayView>(scene);

            Require(SceneComponents<C1ExactBattleSandboxItemArrangementPresenter>(scene)
                        .Length == 0,
                "UNIFIED_EXACT_ITEM_PREFLIGHT_PRESENTER_ALREADY_PRESENT");
            Require(SceneComponents<C1FormalItemBoardView>(scene).Length == 1
                    && SceneComponents<C1FormalItemTrayView>(scene).Length == 1
                    && SceneComponents<C1FormalItemArrangementPresenter>(scene)
                        .Length == 1,
                "UNIFIED_APPROXIMATE_ITEM_PREFLIGHT_BASELINE_INVALID");
            ValidateKnownItemPresenters(scene, 1, 0);
            ValidateExactCarrierMounts(shell, board, tray, slots);
            ValidateDynamicItemPresentationMount(board, tray);
            ValidateExpectedUserGeometry(board, tray);
            ValidateNoMissingScripts(scene);
            return new ExactCarrierLayoutBaseline(
                RectTransformSnapshot.Capture(
                    RequireRect(board.transform,
                        "UNIFIED_EXACT_BOARD_ROOT_RECT_MISSING")),
                RectTransformSnapshot.Capture(
                    RequireRect(tray.transform,
                        "UNIFIED_EXACT_TRAY_ROOT_RECT_MISSING")));
        }

        private static void AuthorUnifiedScene(
            Scene scene,
            ExactCarrierLayoutBaseline layoutBaseline)
        {
            Require(scene.IsValid() && scene.isLoaded,
                "UNIFIED_EXACT_ITEM_AUTHORING_SCENE_NOT_LOADED");
            UnifiedBattlePageShell shell = SingleInScene<UnifiedBattlePageShell>(scene);
            UnifiedBattlePageShellMarker marker =
                SingleInScene<UnifiedBattlePageShellMarker>(scene);
            UnifiedBattleFormalSceneHost host =
                SingleInScene<UnifiedBattleFormalSceneHost>(scene);
            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
            Transform actionSlot = RequiredSlot(
                slots,
                UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot);
            C1ExactBattleSandboxItemBoardView board =
                SingleInScene<C1ExactBattleSandboxItemBoardView>(scene);
            C1ExactBattleSandboxItemTrayView tray =
                SingleInScene<C1ExactBattleSandboxItemTrayView>(scene);

            Require(SceneComponents<C1FormalItemArrangementPresenter>(scene)
                        .Length == 0,
                "UNIFIED_APPROXIMATE_ITEM_PRESENTER_NOT_RETIRED");
            ValidateApproximateVisibleLayerRetired(scene);
            C1ExactBattleSandboxItemArrangementPresenter[] exactPresenters =
                SceneComponents<C1ExactBattleSandboxItemArrangementPresenter>(scene);
            Require(exactPresenters.Length <= 1,
                "UNIFIED_EXACT_ITEM_PRESENTER_DUPLICATE");
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                exactPresenters.Length == 1
                    ? exactPresenters[0]
                    : actionSlot.gameObject.AddComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>();
            Require(presenter.transform == actionSlot,
                "UNIFIED_EXACT_ITEM_PRESENTER_PARENT_INVALID");
            ValidateKnownItemPresenters(scene, 0, 1);

            presenter.AssignForEditor(board, tray);
            EditorUtility.SetDirty(presenter);
            Button button = actionSlot.GetComponent<Button>();
            Text summary = DirectChild(actionSlot, "SummaryText")
                ?.GetComponent<Text>();
            Text actionLabel = DirectChild(actionSlot, "Title")
                ?.GetComponent<Text>();
            Require(button != null && summary != null && actionLabel != null,
                "UNIFIED_FORMAL_ACTION_SURFACE_MISSING");
            ItemBalanceWorkbenchCatalog canonicalCatalog =
                LoadCanonicalItemCatalog();
            host.AssignForEditor(
                shell,
                marker,
                summary,
                button,
                actionLabel,
                presenter,
                board,
                tray);
            host.AssignCanonicalItemCatalogForEditor(canonicalCatalog);
            EditorUtility.SetDirty(host);
            PrefabUtility.RecordPrefabInstancePropertyModifications(host);

            ValidateExactCarrierMounts(shell, board, tray, slots);
            ValidateDynamicItemPresentationMount(board, tray);
            ValidateLayoutUnchanged(board, tray, layoutBaseline);
            ValidateTransparentSlot(RequiredSlot(
                slots,
                UnifiedBattlePageShellSlotNames.BoardArea));
            ValidateTransparentSlot(RequiredSlot(
                slots,
                UnifiedBattlePageShellSlotNames.ItemTrayArea));
            Require(presenter.ValidateAuthoredReferences(),
                "UNIFIED_EXACT_ITEM_PRESENTER_REFERENCE_INVALID");
            Require(host.ValidateAuthoredBindings(out string diagnostic), diagnostic);
            ValidateNoMissingScripts(scene);
        }

        private static void ValidateFinalScene(
            Scene scene,
            ExactCarrierLayoutBaseline layoutBaseline)
        {
            Require(scene.IsValid() && scene.isLoaded,
                "UNIFIED_EXACT_ITEM_FINAL_SCENE_NOT_LOADED");
            UnifiedBattlePageShell shell = SingleInScene<UnifiedBattlePageShell>(scene);
            UnifiedBattleFormalSceneHost host =
                SingleInScene<UnifiedBattleFormalSceneHost>(scene);
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                SingleInScene<C1ExactBattleSandboxItemArrangementPresenter>(scene);
            C1ExactBattleSandboxItemBoardView board =
                SingleInScene<C1ExactBattleSandboxItemBoardView>(scene);
            C1ExactBattleSandboxItemTrayView tray =
                SingleInScene<C1ExactBattleSandboxItemTrayView>(scene);
            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();

            Require(string.Equals(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        shell.gameObject),
                    UnifiedPrefabPath,
                    StringComparison.Ordinal),
                "UNIFIED_EXACT_ITEM_SCENE_SHELL_SOURCE_MISMATCH");
            Require(presenter.transform == RequiredSlot(
                    slots,
                    UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot),
                "UNIFIED_EXACT_ITEM_FINAL_PRESENTER_PARENT_INVALID");
            ValidateKnownItemPresenters(scene, 0, 1);
            ValidateApproximateVisibleLayerRetired(scene);
            ValidateExactCarrierMounts(shell, board, tray, slots);
            ValidateDynamicItemPresentationMount(board, tray);
            ValidateLayoutUnchanged(board, tray, layoutBaseline);
            ValidateTransparentSlot(RequiredSlot(
                slots,
                UnifiedBattlePageShellSlotNames.BoardArea));
            ValidateTransparentSlot(RequiredSlot(
                slots,
                UnifiedBattlePageShellSlotNames.ItemTrayArea));
            Require(presenter.ValidateAuthoredReferences(),
                "UNIFIED_EXACT_ITEM_FINAL_PRESENTER_INVALID");
            Require(host.ValidateAuthoredBindings(out string diagnostic), diagnostic);
            ValidateNoMissingScripts(scene);
        }

        private static void ValidateExactCarrierMounts(
            UnifiedBattlePageShell shell,
            C1ExactBattleSandboxItemBoardView board,
            C1ExactBattleSandboxItemTrayView tray,
            IReadOnlyDictionary<string, Transform> slots)
        {
            Require(shell != null && board != null && tray != null,
                "UNIFIED_EXACT_ITEM_CARRIER_MISSING");
            Require(board.transform.parent == RequiredSlot(
                        slots,
                        UnifiedBattlePageShellSlotNames.BoardArea)
                    && tray.transform.parent == RequiredSlot(
                        slots,
                        UnifiedBattlePageShellSlotNames.ItemTrayArea),
                "UNIFIED_EXACT_ITEM_MOUNT_PARENT_INVALID");
            Require(string.Equals(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        board.gameObject),
                    ExactBoardPrefabPath,
                    StringComparison.Ordinal),
                "UNIFIED_EXACT_ITEM_BOARD_SOURCE_MISMATCH");
            Require(string.Equals(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        tray.gameObject),
                    ExactTrayPrefabPath,
                    StringComparison.Ordinal),
                "UNIFIED_EXACT_ITEM_TRAY_SOURCE_MISMATCH");
            Require(board.ValidateAuthoredReferences()
                    && tray.ValidateAuthoredReferences(),
                "UNIFIED_EXACT_ITEM_VIEW_REFERENCE_INVALID");
        }

        private static void ValidateDynamicItemPresentationMount(
            C1ExactBattleSandboxItemBoardView board,
            C1ExactBattleSandboxItemTrayView tray)
        {
            C1ExactBattleSandboxItemCardView[] cards =
                tray.GetComponentsInChildren<
                    C1ExactBattleSandboxItemCardView>(true);
            Require(board != null
                    && tray != null
                    && board.PresentationCatalogSnapshot != null
                    && board.AuthoredArtworkCapacity > 0
                    && board.AuthoredArtworkCapacity <=
                    C1ExactBattleSandboxItemBoardView
                        .PhysicalPresentationCapacity
                    && tray.AuthoredCardCapacity > 0
                    && tray.AuthoredCardCapacity <=
                    C1ExactBattleSandboxItemTrayView
                        .PhysicalPresentationCapacity
                    && cards.Length == tray.AuthoredCardCapacity
                    && cards.Distinct().Count() == cards.Length,
                "UNIFIED_EXACT_ITEM_DYNAMIC_PROVIDER_MOUNT_INVALID");
            foreach (C1ExactBattleSandboxItemCardView card in cards)
            {
                Require(card != null && card.ValidateAuthoredReferences(),
                    "UNIFIED_EXACT_ITEM_CARD_REFERENCE_INVALID");
                Require(string.Equals(
                        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                            card.gameObject),
                        ExactCardPrefabPath,
                        StringComparison.Ordinal),
                    "UNIFIED_EXACT_ITEM_CARD_SOURCE_MISMATCH");
            }
        }

        private static void ValidateExpectedUserGeometry(
            C1ExactBattleSandboxItemBoardView board,
            C1ExactBattleSandboxItemTrayView tray)
        {
            RectTransform boardRect = RequireRect(
                board.transform,
                "UNIFIED_EXACT_BOARD_ROOT_RECT_MISSING");
            RectTransform trayRect = RequireRect(
                tray.transform,
                "UNIFIED_EXACT_TRAY_ROOT_RECT_MISSING");
            Require(Nearly(boardRect.sizeDelta, new Vector2(800f, 800f))
                    && Nearly(boardRect.anchoredPosition,
                        new Vector2(135.67f, -47f)),
                "UNIFIED_EXACT_BOARD_USER_GEOMETRY_MISMATCH");
            Require(Nearly(trayRect.sizeDelta, new Vector2(800f, 800f))
                    && Nearly(trayRect.anchoredPosition,
                        new Vector2(167.39f, 95f)),
                "UNIFIED_EXACT_TRAY_USER_GEOMETRY_MISMATCH");
        }

        private static void ValidateLayoutUnchanged(
            C1ExactBattleSandboxItemBoardView board,
            C1ExactBattleSandboxItemTrayView tray,
            ExactCarrierLayoutBaseline baseline)
        {
            Require(baseline != null,
                "UNIFIED_EXACT_ITEM_LAYOUT_BASELINE_MISSING");
            Require(baseline.Board.Matches(RequireRect(
                    board.transform,
                    "UNIFIED_EXACT_BOARD_ROOT_RECT_MISSING")),
                "UNIFIED_EXACT_BOARD_LAYOUT_CHANGED");
            Require(baseline.Tray.Matches(RequireRect(
                    tray.transform,
                    "UNIFIED_EXACT_TRAY_ROOT_RECT_MISSING")),
                "UNIFIED_EXACT_TRAY_LAYOUT_CHANGED");
        }

        private static void ValidateApproximateVisibleLayerRetired(Scene scene)
        {
            C1FormalItemBoardView[] boards =
                SceneComponents<C1FormalItemBoardView>(scene);
            C1FormalItemTrayView[] trays =
                SceneComponents<C1FormalItemTrayView>(scene);
            Require(boards.Length == 1 && trays.Length == 1,
                "UNIFIED_APPROXIMATE_ITEM_MOUNT_COUNT_CHANGED");
            Require(!RequiredNestedPrefabRoot(
                        boards[0],
                        ApproximateBoardPrefabPath).activeSelf
                    && !RequiredNestedPrefabRoot(
                        trays[0],
                        ApproximateTrayPrefabPath).activeSelf,
                "UNIFIED_APPROXIMATE_ITEM_VISIBLE_LAYER_NOT_RETIRED");
            Require(SceneComponents<C1FormalItemArrangementPresenter>(scene)
                        .Length == 0,
                "UNIFIED_APPROXIMATE_ITEM_PRESENTER_NOT_RETIRED");
        }

        private static void DisablePlaceholderRendering(Transform slot)
        {
            foreach (Graphic graphic in slot.GetComponents<Graphic>())
            {
                graphic.enabled = false;
                graphic.raycastTarget = false;
            }

            foreach (Shadow shadow in slot.GetComponents<Shadow>())
            {
                shadow.enabled = false;
            }

            foreach (string childName in new[] { "Title", "SummaryText" })
            {
                Transform child = DirectChild(slot, childName);
                if (child != null)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static void ValidateTransparentSlot(Transform slot)
        {
            Require(slot != null, "UNIFIED_EXACT_ITEM_SLOT_MISSING");
            Require(slot.GetComponents<Graphic>()
                        .All(graphic => graphic != null
                                        && !graphic.enabled
                                        && !graphic.raycastTarget),
                "UNIFIED_EXACT_ITEM_SLOT_GRAPHIC_VISIBLE " + slot.name);
            Require(slot.GetComponents<Shadow>()
                        .All(shadow => shadow != null && !shadow.enabled),
                "UNIFIED_EXACT_ITEM_SLOT_EFFECT_VISIBLE " + slot.name);
            foreach (string childName in new[] { "Title", "SummaryText" })
            {
                Transform child = DirectChild(slot, childName);
                Require(child == null || !child.gameObject.activeSelf,
                    "UNIFIED_EXACT_ITEM_PLACEHOLDER_VISIBLE " + slot.name
                    + "/" + childName);
            }
        }

        private static void ValidateKnownItemPresenters(
            GameObject root,
            int expectedApproximate,
            int expectedExact)
        {
            MonoBehaviour[] candidates = root
                .GetComponentsInChildren<MonoBehaviour>(true)
                .Where(IsItemPresenter)
                .ToArray();
            ValidateKnownItemPresenterSet(
                candidates,
                expectedApproximate,
                expectedExact);
        }

        private static void ValidateKnownItemPresenters(
            Scene scene,
            int expectedApproximate,
            int expectedExact)
        {
            MonoBehaviour[] candidates = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<
                    MonoBehaviour>(true))
                .Where(IsItemPresenter)
                .ToArray();
            ValidateKnownItemPresenterSet(
                candidates,
                expectedApproximate,
                expectedExact);
        }

        private static void ValidateKnownItemPresenterSet(
            IReadOnlyCollection<MonoBehaviour> candidates,
            int expectedApproximate,
            int expectedExact)
        {
            Require(candidates.All(component =>
                    component is C1FormalItemArrangementPresenter
                    || component is
                        C1ExactBattleSandboxItemArrangementPresenter),
                "UNIFIED_UNKNOWN_ITEM_PRESENTER_PRESENT");
            Require(candidates.OfType<C1FormalItemArrangementPresenter>()
                        .Count() == expectedApproximate
                    && candidates.OfType<
                        C1ExactBattleSandboxItemArrangementPresenter>()
                        .Count() == expectedExact,
                "UNIFIED_ITEM_PRESENTER_COUNT_INVALID approximate=" +
                candidates.OfType<C1FormalItemArrangementPresenter>().Count()
                + " exact=" + candidates.OfType<
                    C1ExactBattleSandboxItemArrangementPresenter>().Count());
        }

        private static bool IsItemPresenter(MonoBehaviour component)
        {
            if (component == null)
            {
                return false;
            }

            Type type = component.GetType();
            return string.Equals(
                       type.Namespace,
                       "TalismanBag.Items.CampaignBaseline",
                       StringComparison.Ordinal)
                   && type.Name.EndsWith(
                       "ItemArrangementPresenter",
                       StringComparison.Ordinal);
        }

        private static void ValidateStaticRuntimeBoundaries()
        {
            foreach (string path in RuntimeSourcePaths)
            {
                Require(File.Exists(path), "RUNTIME_SOURCE_MISSING " + path);
                string source = File.ReadAllText(path);
                foreach (string forbidden in ForbiddenRuntimeTokens)
                {
                    Require(source.IndexOf(forbidden, StringComparison.Ordinal) < 0,
                        "FORBIDDEN_RUNTIME_TOKEN " + forbidden + " path=" + path);
                }
            }

            string loopSource = File.ReadAllText(RuntimeSourcePaths[0]);
            Require(loopSource.IndexOf("PlayerPrefs", StringComparison.Ordinal) < 0
                    && loopSource.IndexOf("TalismanSceneNavigationOwner",
                        StringComparison.Ordinal) < 0
                    && loopSource.IndexOf("FormalBattleLaunchTransit.TryPublish",
                        StringComparison.Ordinal) < 0,
                "FORMAL_LOOP_SECOND_OWNER_OR_PERSISTENCE_REFERENCE");
            string hostSource = File.ReadAllText(RuntimeSourcePaths[1]);
            Require(hostSource.IndexOf("layoutKind", StringComparison.Ordinal) < 0
                    && hostSource.IndexOf("i002Lit", StringComparison.Ordinal) < 0
                    && hostSource.IndexOf(
                        "C1FormalItemArrangementPresenter",
                        StringComparison.Ordinal) < 0
                    && hostSource.IndexOf(
                        "C1ExactBattleSandboxItemArrangementPresenter",
                        StringComparison.Ordinal) >= 0,
                "MAINLINE_EXACT_ITEM_BINDING_BOUNDARY_INVALID");
        }

        private static GameObject RequiredNestedPrefabRoot(
            Component component,
            string expectedPath)
        {
            Require(component != null,
                "UNIFIED_ITEM_NESTED_PREFAB_COMPONENT_MISSING");
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(
                component.gameObject);
            Require(root != null, "UNIFIED_ITEM_NESTED_PREFAB_ROOT_MISSING");
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                component.gameObject);
            Require(string.Equals(path, expectedPath, StringComparison.Ordinal),
                "UNIFIED_ITEM_NESTED_PREFAB_SOURCE_MISMATCH expected=" +
                expectedPath + " actual=" + path);
            return root;
        }

        private static Transform RequiredSlot(
            IReadOnlyDictionary<string, Transform> slots,
            string name)
        {
            Transform value = null;
            if (slots != null)
            {
                slots.TryGetValue(name, out value);
            }

            Require(value != null, "UNIFIED_REQUIRED_SLOT_MISSING " + name);
            return value;
        }

        private static Transform DirectChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (string.Equals(child.name, name, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static RectTransform RequireRect(
            Transform transform,
            string diagnostic)
        {
            RectTransform rect = transform as RectTransform;
            Require(rect != null, diagnostic);
            return rect;
        }

        private static T SingleInScene<T>(Scene scene) where T : Component
        {
            T[] matches = SceneComponents<T>(scene);
            Require(matches.Length == 1,
                "UNIFIED_EXACT_ITEM_SCENE_COUNT " + typeof(T).Name +
                " count=" + matches.Length);
            return matches[0];
        }

        private static T[] SceneComponents<T>(Scene scene) where T : Component
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(value => value != null && value.gameObject.scene == scene)
                .ToArray();
        }

        private static void ValidateNoMissingScripts(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "UNIFIED_EXACT_ITEM_SCENE_MISSING_SCRIPT " + root.name);
            }
        }

        private static bool Nearly(Vector2 left, Vector2 right)
        {
            return Mathf.Abs(left.x - right.x) <= 0.001f
                   && Mathf.Abs(left.y - right.y) <= 0.001f;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private sealed class ExactCarrierLayoutBaseline
        {
            public ExactCarrierLayoutBaseline(
                RectTransformSnapshot board,
                RectTransformSnapshot tray)
            {
                Board = board;
                Tray = tray;
            }

            public RectTransformSnapshot Board { get; }
            public RectTransformSnapshot Tray { get; }
        }

        private sealed class RectTransformSnapshot
        {
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 pivot;
            private readonly Vector3 localScale;
            private readonly Quaternion localRotation;
            private readonly int siblingIndex;

            private RectTransformSnapshot(RectTransform rect)
            {
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                pivot = rect.pivot;
                localScale = rect.localScale;
                localRotation = rect.localRotation;
                siblingIndex = rect.GetSiblingIndex();
            }

            public static RectTransformSnapshot Capture(RectTransform rect)
            {
                return new RectTransformSnapshot(rect);
            }

            public bool Matches(RectTransform rect)
            {
                return rect != null
                       && Exact(anchorMin, rect.anchorMin)
                       && Exact(anchorMax, rect.anchorMax)
                       && Exact(anchoredPosition, rect.anchoredPosition)
                       && Exact(sizeDelta, rect.sizeDelta)
                       && Exact(pivot, rect.pivot)
                       && Exact(localScale, rect.localScale)
                       && Exact(localRotation, rect.localRotation)
                       && siblingIndex == rect.GetSiblingIndex();
            }

            private static bool Exact(Vector2 left, Vector2 right)
            {
                return left.x.Equals(right.x) && left.y.Equals(right.y);
            }

            private static bool Exact(Vector3 left, Vector3 right)
            {
                return left.x.Equals(right.x)
                       && left.y.Equals(right.y)
                       && left.z.Equals(right.z);
            }

            private static bool Exact(Quaternion left, Quaternion right)
            {
                return left.x.Equals(right.x)
                       && left.y.Equals(right.y)
                       && left.z.Equals(right.z)
                       && left.w.Equals(right.w);
            }
        }
    }
}
