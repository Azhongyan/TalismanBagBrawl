using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Presentation.FormalBattle;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.Presentation.ExactItemDetail;
using TalismanBag.UnifiedBattle.Presentation.ExactNavigation;
using TalismanBag.UnifiedBattle.Presentation.ExactPrepare;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class C1UnifiedSingleFormalPathAuthoring
    {
        private const string MenuPath =
            "TalismanBag/V0.4/Unified Battle/Apply REV11 Single Formal Path Once";
        private const string SuccessMarker =
            "SINGLE_FORMAL_PATH_AUTHORING_AND_VALIDATION_PASS";
        private const string DynamicItemProviderCleanupMenuPath =
            "TalismanBag/V0.4/Unified Battle/Cleanup Dynamic Item Provider Shell Wiring Once";
        private const string DynamicItemProviderCleanupSuccessMarker =
            "C1_DYNAMIC_ITEM_PROVIDER_SHELL_CLEANUP_AND_VALIDATION_PASS";
        private const string RetiredExactSourceCardViewsProperty =
            "exactSourceCardViews";
        private const string FormalPresentationPrefabGuid =
            "c34842e78f9303a4c82a79f0396e4dd7";
        private const long FormalPresentationRootSourceFileId =
            1247776835702022391L;
        private const string ShellPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";
        private const string UnifiedScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        private const string BattleSandboxSourceScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string PrepareSurfacePrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxPrepareSurface.prefab";
        private const string NavigationPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxBottomNavigation.prefab";
        private const string GameBackgroundPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxGameBackground.prefab";
        private const string ExactBoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab";
        private const string ExactTrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab";
        private const string ExactCardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab";
        private const string ItemDetailPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab";
        private const string FormalPresentationPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/FormalBattlePresentationRoot.prefab";
        private const string DamageFloatPoolPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/FormalBattleDamageFloatPool.prefab";
        private const string CueFxAudioRootPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/FormalBattleCueFxAudioRoot.prefab";
        private const string CombatLogPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxCombatLogPanel.prefab";
        private const string ThisAuthoringPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1UnifiedSingleFormalPathAuthoring.cs";
        private const string FormalGuajiannName = "guajiann";
        private const string FormalGuajiannRelativePath = "guajiann";
        private const string FormalGuajiannCallbackMethod = "PlayFromStart";
        private const string ItemDetailPresentationFrameName =
            "ItemDetailPresentationFrame";

        private enum ItemDetailCarrierState
        {
            FINAL_CLEAN,
            EXACT_REPAIRABLE_PARTIAL,
            INVALID_CONFLICT
        }

        private enum ItemDetailPresentationMountState
        {
            FINAL_VISIBLE_MOUNT,
            EXACT_BLACK_SCREEN_REPAIRABLE,
            INVALID_CONFLICT
        }

        private const long Rev11cMissingRelayFileId = 7099084332157108925L;
        private static readonly string[] LegacyShellObjectNames =
        {
            "BossCastBarSlot",
            "StoryGuidePopupLayer",
            "ResultRewardPlaceholder",
            "V03FlowAdapterSlot",
            "V04SandboxAdapterSlot",
            "DevOnlyDiagnosticsSlot"
        };

        private static readonly string[] DeleteAssetPaths =
        {
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattlePageShellSampleData.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/UnifiedBattlePageShellSceneBuilder.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/UnifiedBattlePageShellVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/UnifiedBattlePageShellReportWriter.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1FormalObtainRebuildBattleLoopSceneAuthoring.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1ExactBattleSandboxNavigationPhaseAAuthoring.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1ExactBattleSandboxItemDetailPhaseBAuthoring.cs"
        };

        [MenuItem(MenuPath)]
        public static void ApplyFromMenu()
        {
            ApplySinglePass();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ApplySinglePass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1UnifiedSingleFormalPathAuthoring] FAILED "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(DynamicItemProviderCleanupMenuPath)]
        public static void ApplyDynamicItemProviderShellCleanupFromMenu()
        {
            ApplyDynamicItemProviderShellCleanupSinglePass();
        }

        public static void
            ExecuteDynamicItemProviderShellCleanupFromCommandLine()
        {
            try
            {
                ApplyDynamicItemProviderShellCleanupSinglePass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1UnifiedSingleFormalPathAuthoring] "
                    + "DYNAMIC_ITEM_PROVIDER_SHELL_CLEANUP_FAILED "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ApplyDynamicItemProviderShellCleanupSinglePass()
        {
            Require(File.Exists(ShellPrefabPath),
                "DYNAMIC_ITEM_PROVIDER_SHELL_MISSING");

            string intakeYaml = NormalizeNewlines(
                File.ReadAllText(ShellPrefabPath));
            bool hasRetiredWiring = intakeYaml.IndexOf(
                "propertyPath: " + RetiredExactSourceCardViewsProperty,
                StringComparison.Ordinal) >= 0;
            bool changed = false;
            GameObject root = PrefabUtility.LoadPrefabContents(
                ShellPrefabPath);
            try
            {
                ValidateDynamicItemProviderShellForCleanup(root);
                FormalBattlePresentationRoot presentation =
                    Single<FormalBattlePresentationRoot>(root);
                GameObject presentationInstanceRoot = PrefabUtility
                    .GetNearestPrefabInstanceRoot(presentation.gameObject);
                Require(presentationInstanceRoot != null
                        && presentationInstanceRoot == presentation.gameObject
                        && string.Equals(
                            NearestPrefabPath(presentation.gameObject),
                            FormalPresentationPrefabPath,
                            StringComparison.Ordinal),
                    "DYNAMIC_ITEM_PROVIDER_PRESENTATION_INSTANCE_INVALID");

                PropertyModification[] original = PrefabUtility
                    .GetPropertyModifications(presentationInstanceRoot)
                    ?? Array.Empty<PropertyModification>();
                FormalBattlePresentationRoot sourcePresentation =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        presentation);
                Require(sourcePresentation != null,
                    "DYNAMIC_ITEM_PROVIDER_SOURCE_PRESENTATION_MISSING");
                Require(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                            sourcePresentation,
                            out string sourceGuid,
                            out long sourceFileId)
                        && string.Equals(
                            sourceGuid,
                            FormalPresentationPrefabGuid,
                            StringComparison.Ordinal)
                        && sourceFileId ==
                        FormalPresentationRootSourceFileId,
                    "DYNAMIC_ITEM_PROVIDER_SOURCE_PRESENTATION_IDENTITY_INVALID");
                PropertyModification[] allRetired = original.Where(value =>
                        value != null
                        && IsRetiredExactSourceCardViewsProperty(
                            value.propertyPath))
                    .ToArray();
                PropertyModification[] retired = original.Where(value =>
                        value != null
                        && value.target == sourcePresentation
                        && IsRetiredExactSourceCardViewsProperty(
                            value.propertyPath))
                    .ToArray();

                if (hasRetiredWiring)
                {
                    Require(allRetired.Length == retired.Length,
                        "DYNAMIC_ITEM_PROVIDER_RETIRED_WIRING_TARGET_CONFLICT");
                    ValidateRetiredExactSourceCardViewsModifications(retired);
                    string[] beforeOther = original
                        .Where(value => !retired.Contains(value))
                        .Select(CleanupPropertyModificationSignature)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray();
                    PropertyModification[] filtered = original
                        .Where(value => !retired.Contains(value))
                        .ToArray();
                    Require(original.Length - filtered.Length == retired.Length,
                        "DYNAMIC_ITEM_PROVIDER_RETIRED_WIRING_FILTER_INVALID");

                    PrefabUtility.SetPropertyModifications(
                        presentationInstanceRoot,
                        filtered);
                    PropertyModification[] actual = PrefabUtility
                        .GetPropertyModifications(presentationInstanceRoot)
                        ?? Array.Empty<PropertyModification>();
                    Require(actual.All(value => value == null
                                || value.target != sourcePresentation
                                || !IsRetiredExactSourceCardViewsProperty(
                                    value.propertyPath)),
                        "DYNAMIC_ITEM_PROVIDER_RETIRED_WIRING_REMAINS");
                    string[] afterOther = actual
                        .Select(CleanupPropertyModificationSignature)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray();
                    Require(beforeOther.SequenceEqual(afterOther),
                        "DYNAMIC_ITEM_PROVIDER_UNRELATED_OVERRIDE_DRIFT");
                    ValidateDynamicItemProviderShellForCleanup(root);
                    GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                        root,
                        ShellPrefabPath);
                    Require(saved != null,
                        "DYNAMIC_ITEM_PROVIDER_SHELL_SAVE_FAILED");
                    changed = true;
                }
                else
                {
                    Require(retired.Length == 0,
                        "DYNAMIC_ITEM_PROVIDER_LOADED_RETIRED_WIRING_CONFLICT");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            GameObject validationRoot = PrefabUtility.LoadPrefabContents(
                ShellPrefabPath);
            try
            {
                ValidateDynamicItemProviderShellForCleanup(validationRoot);
                FormalBattlePresentationRoot presentation =
                    Single<FormalBattlePresentationRoot>(validationRoot);
                GameObject presentationInstanceRoot = PrefabUtility
                    .GetNearestPrefabInstanceRoot(presentation.gameObject);
                FormalBattlePresentationRoot sourcePresentation =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        presentation);
                PropertyModification[] modifications = PrefabUtility
                    .GetPropertyModifications(presentationInstanceRoot)
                    ?? Array.Empty<PropertyModification>();
                Require(modifications.All(value => value == null
                            || value.target != sourcePresentation
                            || !IsRetiredExactSourceCardViewsProperty(
                                value.propertyPath)),
                    "DYNAMIC_ITEM_PROVIDER_POSTSAVE_RETIRED_WIRING_REMAINS");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(validationRoot);
            }

            string finalYaml = NormalizeNewlines(
                File.ReadAllText(ShellPrefabPath));
            Require(finalYaml.IndexOf(
                        "propertyPath: "
                        + RetiredExactSourceCardViewsProperty,
                        StringComparison.Ordinal) < 0,
                "DYNAMIC_ITEM_PROVIDER_SERIALIZED_RETIRED_WIRING_REMAINS");
            Debug.Log(
                "[C1UnifiedSingleFormalPathAuthoring] "
                + DynamicItemProviderCleanupSuccessMarker
                + " changed=" + (changed ? "1" : "0")
                + " shell=" + ShellPrefabPath);
        }

        private static void ApplySinglePass()
        {
            ValidateSourceItemDetailPresentationFacts();
            ValidateDeleteInventoryHasNoLiveConsumer();

            bool wroteProductAsset = false;
            GameObject root = PrefabUtility.LoadPrefabContents(ShellPrefabPath);
            try
            {
                ItemDetailPresentationMountState mountState =
                    ClassifyItemDetailPresentationMountState(root);
                if (mountState
                    == ItemDetailPresentationMountState
                        .EXACT_BLACK_SCREEN_REPAIRABLE)
                {
                    RepairExactBlackScreenItemDetailMount(root);
                    RequireFinalCleanLoadedItemDetailState(root);
                    ValidateFinalComposition(root);
                    GameObject repaired = PrefabUtility.SaveAsPrefabAsset(
                        root, ShellPrefabPath);
                    Require(repaired != null,
                        "REV11E_BLACK_SCREEN_SHELL_SAVE_FAILED");
                    wroteProductAsset = true;
                }
                else if (mountState
                         == ItemDetailPresentationMountState
                             .FINAL_VISIBLE_MOUNT)
                {
                    ValidateFinalComposition(root);
                }
                else
                {
                    ItemDetailCarrierState state =
                        ClassifyItemDetailCarrierState(root);
                    switch (state)
                    {
                        case ItemDetailCarrierState.EXACT_REPAIRABLE_PARTIAL:
                            RepairFrozenPartialItemDetailRelay(root);
                            AuthorVisibleItemDetailMountFromLegacyState(root);
                            RequireFinalCleanLoadedItemDetailState(root);
                            ValidateFinalComposition(root);
                            GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                                root,
                                ShellPrefabPath);
                            Require(saved != null,
                                "REV11D_PARTIAL_SHELL_SAVE_FAILED");
                            wroteProductAsset = true;
                            break;
                        default:
                            throw new InvalidOperationException(
                                "REV11D_ITEM_DETAIL_STATE_INVALID_CONFLICT");
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            GameObject validationRoot = PrefabUtility.LoadPrefabContents(
                ShellPrefabPath);
            try
            {
                ItemDetailCarrierState savedState =
                    ClassifyItemDetailCarrierState(validationRoot);
                ItemDetailPresentationMountState savedMountState =
                    ClassifyItemDetailPresentationMountState(validationRoot);
                Require(savedState == ItemDetailCarrierState.FINAL_CLEAN,
                    "REV11D_POST_SAVE_ITEM_DETAIL_STATE_INVALID "
                    + savedState);
                Require(savedMountState
                        == ItemDetailPresentationMountState
                            .FINAL_VISIBLE_MOUNT,
                    "REV11E_POST_SAVE_ITEM_DETAIL_MOUNT_INVALID "
                    + savedMountState);
                ValidateFinalComposition(validationRoot);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(validationRoot);
            }

            ValidateSourceItemDetailPresentationFacts();
            if (wroteProductAsset)
            {
                AssetDatabase.SaveAssets();
            }
            int assertionCount = C1FormalObtainRebuildBattleLoopTests
                .RunRev11SingleFormalPathConsolidationOrThrow();
            Debug.Log(
                "[C1UnifiedSingleFormalPathAuthoring] " + SuccessMarker
                + " shell=" + ShellPrefabPath
                + " combatLog=" + CombatLogPrefabPath
                + " assertions=" + assertionCount.ToString(
                    CultureInfo.InvariantCulture));
        }

        private static void AuthorSingleFormalPath(GameObject root)
        {
            UnifiedBattlePageShell shell = Single<UnifiedBattlePageShell>(root);
            UnifiedBattlePageShellMarker marker =
                Single<UnifiedBattlePageShellMarker>(root);
            UnifiedBattleFormalSceneHost oldHost =
                Single<UnifiedBattleFormalSceneHost>(root);
            Require(shell.transform == root.transform
                    && marker.transform == root.transform,
                "REV11_SHELL_ROOT_COMPONENT_IDENTITY_INVALID");
            RequireNoNamedObject(root, "BoardArea");
            RequireNoNamedObject(root, "ItemTrayArea");

            Dictionary<string, Transform> legacy =
                LegacyShellObjectNames.ToDictionary(
                    name => name,
                    name => RequiredDirectChild(root.transform, name),
                    StringComparer.Ordinal);
            Require(oldHost.transform == legacy["V03FlowAdapterSlot"],
                "REV11_LEGACY_HOST_LOCATION_INVALID");

            List<TransformState> keptDirectChildren = root.transform
                .Cast<Transform>()
                .Where(value => !LegacyShellObjectNames.Contains(
                    value.name,
                    StringComparer.Ordinal))
                .Select(value => new TransformState(value))
                .ToList();

            C1ExactBattleSandboxNavigationPresenter navigation =
                Single<C1ExactBattleSandboxNavigationPresenter>(root);
            C1ExactBattleSandboxPrepareSurfacePresenter prepare =
                Single<C1ExactBattleSandboxPrepareSurfacePresenter>(root);
            OmitFormalGuajiann(root, prepare);
            C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                Single<C1ExactBattleSandboxItemArrangementPresenter>(root);
            C1ExactBattleSandboxItemBoardView boardView =
                Single<C1ExactBattleSandboxItemBoardView>(root);
            C1ExactBattleSandboxItemTrayView trayView =
                Single<C1ExactBattleSandboxItemTrayView>(root);
            FormalBattlePresentationRoot presentation =
                Single<FormalBattlePresentationRoot>(root);
            FormalBattleDamageFloatPool damageFloatPool =
                Single<FormalBattleDamageFloatPool>(root);
            FormalBattleCueFxAudioRoot cueFxAudioRoot =
                Single<FormalBattleCueFxAudioRoot>(root);
            Image background = root.GetComponentsInChildren<Image>(true)
                .Single(value => string.Equals(
                    NearestPrefabPath(value.gameObject),
                    GameBackgroundPrefabPath,
                    StringComparison.Ordinal));

            Transform enemyInfo = RequiredDirectChild(
                root.transform,
                UnifiedBattlePageShellSlotNames.EnemyInfoArea);
            Transform feedback = RequiredDirectChild(
                root.transform,
                UnifiedBattlePageShellSlotNames.BattleFeedbackLayer);
            PruneAnchorToAcceptedChildren(
                enemyInfo,
                new[] { presentation.transform });
            PruneAnchorToAcceptedChildren(
                feedback,
                new[] { damageFloatPool.transform, cueFxAudioRoot.transform });

            C1ExactBattleSandboxCombatLogView combatLog =
                InstantiateCombatLogCarrier(root.transform);
            C1ExactBattleSandboxItemDetailPresenter itemDetail =
                CreateItemDetailCarrier(root.transform);

            UnifiedBattleFormalSceneHost host =
                root.AddComponent<UnifiedBattleFormalSceneHost>();
            marker.ConfigureFormalForEditor();
            shell.AssignFormalCompositionForEditor(
                root.transform,
                enemyInfo,
                feedback,
                itemDetail.transform);
            presentation.AssignExternalCompositionForEditor(
                damageFloatPool,
                cueFxAudioRoot);
            PrefabUtility.RecordPrefabInstancePropertyModifications(
                presentation);
            host.AssignCoreForEditor(shell, marker, combatLog);
            host.AssignNavigationForEditor(navigation);
            host.AssignPrepareSurfaceForEditor(
                prepare,
                background,
                itemPresenter,
                boardView,
                trayView);
            host.AssignItemDetailForEditor(itemDetail);
            host.AssignPresentationForEditor(presentation);

            Object.DestroyImmediate(oldHost);
            foreach (Transform obsolete in legacy.Values)
            {
                Object.DestroyImmediate(obsolete.gameObject);
            }

            foreach (TransformState state in keptDirectChildren)
            {
                state.RequireUnchanged();
            }
            RequireRelativeOrderPreserved(
                root.transform,
                keptDirectChildren.Select(value => value.Transform).ToArray());

            combatLog.ResetPresentation();
            itemDetail.ResetPresentation();
            EditorUtility.SetDirty(shell);
            EditorUtility.SetDirty(marker);
            EditorUtility.SetDirty(presentation);
            EditorUtility.SetDirty(host);
        }

        private static C1ExactBattleSandboxCombatLogView
            InstantiateCombatLogCarrier(Transform parent)
        {
            Require(parent.GetComponentsInChildren<
                        C1ExactBattleSandboxCombatLogView>(true).Length == 0,
                "REV11_COMBAT_LOG_PARTIAL_OR_DUPLICATE");
            GameObject asset = RequirePrefab(CombatLogPrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(
                asset,
                parent) as GameObject;
            Require(instance != null, "REV11_COMBAT_LOG_MOUNT_FAILED");
            C1ExactBattleSandboxCombatLogView view =
                instance.GetComponent<C1ExactBattleSandboxCombatLogView>();
            Require(view != null, "REV11_COMBAT_LOG_VIEW_MISSING");
            return view;
        }

        private static C1ExactBattleSandboxItemDetailPresenter
            CreateItemDetailCarrier(Transform parent)
        {
            Require(parent.GetComponentsInChildren<
                        C1ExactBattleSandboxItemDetailPresenter>(true).Length
                    == 0,
                "REV11_ITEM_DETAIL_PARTIAL_OR_DUPLICATE");
            GameObject slot = new GameObject(
                UnifiedBattlePageShellSlotNames.ItemDetailPopupSlot,
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(C1ExactBattleSandboxItemDetailPresenter));
            slot.layer = parent.gameObject.layer;
            RectTransform rect = (RectTransform)slot.transform;
            rect.SetParent(parent, false);
            ConfigureFullStretch(rect);

            CanvasGroup group = slot.GetComponent<CanvasGroup>();
            ConfigureClosedGroup(group);

            RectTransform frame = CreateItemDetailPresentationFrame(rect);

            GameObject panelInstance = PrefabUtility.InstantiatePrefab(
                RequirePrefab(ItemDetailPrefabPath),
                frame) as GameObject;
            Require(panelInstance != null, "REV11_ITEM_DETAIL_MOUNT_FAILED");
            ItemDetailPanelView panel =
                panelInstance.GetComponent<ItemDetailPanelView>();
            Require(panel != null, "REV11_ITEM_DETAIL_PANEL_VIEW_MISSING");
            ConfigureSourceItemDetailPanelTransform(
                (RectTransform)panel.transform);
            Button closeButton = new SerializedObject(panel)
                .FindProperty("closeButton").objectReferenceValue as Button;
            Require(closeButton != null,
                "REV11_ITEM_DETAIL_CLOSE_BUTTON_MISSING");

            C1ExactBattleSandboxItemDetailPresenter presenter =
                slot.GetComponent<C1ExactBattleSandboxItemDetailPresenter>();
            presenter.AssignForEditor(
                panel,
                closeButton,
                group,
                frame);
            presenter.ResetPresentation();
            rect.SetAsLastSibling();
            return presenter;
        }

        private static void EnsureCombatLogPrefab()
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(
                CombatLogPrefabPath);
            if (existing != null)
            {
                ValidateCombatLogPrefabAsset(existing);
                return;
            }

            GameObject prepareAsset = RequirePrefab(PrepareSurfacePrefabPath);
            Text[] sourceMatches = prepareAsset
                .GetComponentsInChildren<Text>(true)
                .Where(value => string.Equals(
                    value.gameObject.name,
                    "CombatLogText",
                    StringComparison.Ordinal))
                .ToArray();
            Require(sourceMatches.Length == 1,
                "REV11_SOURCE_COMBAT_LOG_TEXT_COUNT_INVALID "
                + sourceMatches.Length.ToString(CultureInfo.InvariantCulture));
            Text sourceText = sourceMatches[0];
            TextVisualState sourceState = new TextVisualState(sourceText);

            GameObject root = new GameObject(
                "C1ExactBattleSandboxCombatLogPanel",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(C1ExactBattleSandboxCombatLogView));
            try
            {
                ConfigureFullStretch((RectTransform)root.transform);
                CanvasGroup group = root.GetComponent<CanvasGroup>();
                ConfigureClosedGroup(group);
                GameObject textObject = Object.Instantiate(
                    sourceText.gameObject);
                textObject.name = "CombatLogText";
                textObject.transform.SetParent(root.transform, false);
                Text authoredText = textObject.GetComponent<Text>();
                Require(authoredText != null,
                    "REV11_COMBAT_LOG_TEXT_CLONE_MISSING");
                authoredText.text = string.Empty;
                authoredText.raycastTarget = false;
                sourceState.RequireSameVisual(authoredText);

                C1ExactBattleSandboxCombatLogView view =
                    root.GetComponent<C1ExactBattleSandboxCombatLogView>();
                view.AssignForEditor(group, authoredText);
                view.ResetPresentation();
                Require(view.ValidateAuthoredReferences(),
                    "REV11_COMBAT_LOG_VIEW_AUTHORING_INVALID");
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    CombatLogPrefabPath);
                Require(saved != null,
                    "REV11_COMBAT_LOG_PREFAB_SAVE_FAILED");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            ValidateCombatLogPrefabAsset(RequirePrefab(CombatLogPrefabPath));
        }

        private static void ValidateCombatLogPrefabAsset(GameObject asset)
        {
            C1ExactBattleSandboxCombatLogView view =
                asset.GetComponent<C1ExactBattleSandboxCombatLogView>();
            Require(view != null && view.ValidateAuthoredReferences(),
                "REV11_COMBAT_LOG_PREFAB_INVALID");
            Require(view.CombatLogText != null
                    && string.Equals(
                        view.CombatLogText.gameObject.name,
                        "CombatLogText",
                        StringComparison.Ordinal)
                    && string.IsNullOrEmpty(view.CombatLogText.text)
                    && !view.IsOpen,
                "REV11_COMBAT_LOG_DEFAULT_STATE_INVALID");
            Require(asset.GetComponentsInChildren<MonoBehaviour>(true)
                        .All(value => value == view
                                      || value == view.CombatLogText),
                "REV11_COMBAT_LOG_UNEXPECTED_BEHAVIOUR");
        }

        private static void ValidateFinalComposition(GameObject root)
        {
            UnifiedBattlePageShell shell = Single<UnifiedBattlePageShell>(root);
            UnifiedBattlePageShellMarker marker =
                Single<UnifiedBattlePageShellMarker>(root);
            UnifiedBattleFormalSceneHost host =
                Single<UnifiedBattleFormalSceneHost>(root);
            Require(shell.transform == root.transform
                    && marker.transform == root.transform
                    && host.transform == root.transform,
                "REV11_FORMAL_ROOT_COMPONENT_IDENTITY_INVALID");
            RequireNoNamedObject(root, "BoardArea");
            RequireNoNamedObject(root, "ItemTrayArea");
            foreach (string legacyName in LegacyShellObjectNames)
            {
                RequireNoNamedObject(root, legacyName);
            }

            Require(shell.CollectMissingRequiredSlots().Count == 0,
                "REV11_REQUIRED_FORMAL_ANCHOR_MISSING");
            Require(shell.BattlePageRoot == root.transform,
                "REV11_BATTLE_PAGE_ROOT_INVALID");

            C1ExactBattleSandboxPrepareSurfacePresenter prepare =
                Single<C1ExactBattleSandboxPrepareSurfacePresenter>(root);
            C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                Single<C1ExactBattleSandboxItemArrangementPresenter>(root);
            C1ExactBattleSandboxItemBoardView board =
                Single<C1ExactBattleSandboxItemBoardView>(root);
            C1ExactBattleSandboxItemTrayView tray =
                Single<C1ExactBattleSandboxItemTrayView>(root);
            RequirePrefabPath(prepare.gameObject, PrepareSurfacePrefabPath);
            ValidateFormalGuajiannOmission(root, prepare);
            RequirePrefabPath(board.gameObject, ExactBoardPrefabPath);
            RequirePrefabPath(tray.gameObject, ExactTrayPrefabPath);
            Require(itemPresenter.transform == prepare.transform
                    && board.transform.IsChildOf(prepare.transform)
                    && tray.transform.IsChildOf(prepare.transform)
                    && itemPresenter.ValidateAuthoredReferences()
                    && board.PresentationCatalogSnapshot != null,
                "REV11_EXACT_ITEM_CHAIN_INVALID");

            C1ExactBattleSandboxNavigationPresenter navigation =
                Single<C1ExactBattleSandboxNavigationPresenter>(root);
            RequirePrefabPath(navigation.gameObject, NavigationPrefabPath);
            Require(navigation.ValidateAuthoredReferences(),
                "REV11_NAVIGATION_BINDING_INVALID");
            Image background = root.GetComponentsInChildren<Image>(true)
                .Single(value => string.Equals(
                    NearestPrefabPath(value.gameObject),
                    GameBackgroundPrefabPath,
                    StringComparison.Ordinal));
            Require(!background.raycastTarget,
                "REV11_GAME_BACKGROUND_RAYCAST_INVALID");

            FormalBattlePresentationRoot presentation =
                Single<FormalBattlePresentationRoot>(root);
            FormalBattleDamageFloatPool damageFloatPool =
                Single<FormalBattleDamageFloatPool>(root);
            FormalBattleCueFxAudioRoot cueFxAudioRoot =
                Single<FormalBattleCueFxAudioRoot>(root);
            RequirePrefabPath(
                presentation.gameObject,
                FormalPresentationPrefabPath);
            RequirePrefabPath(
                damageFloatPool.gameObject,
                DamageFloatPoolPrefabPath);
            RequirePrefabPath(
                cueFxAudioRoot.gameObject,
                CueFxAudioRootPrefabPath);
            bool sourceValid = presentation.ValidateSourceCarrierReferences(
                out string sourceDiagnostic);
            bool downstreamValid =
                presentation.ValidateDownstreamCompositionReferences(
                    out string downstreamDiagnostic);
            Require(presentation.DamageFloatPool == damageFloatPool
                    && presentation.CueFxAudioRoot == cueFxAudioRoot
                    && sourceValid
                    && downstreamValid,
                "REV11_FORMAL_PRESENTATION_BINDING_INVALID "
                + sourceDiagnostic + " " + downstreamDiagnostic);
            Require(presentation.transform.parent == shell.EnemyInfoArea
                    && damageFloatPool.transform.parent
                    == shell.BattleFeedbackLayer
                    && cueFxAudioRoot.transform.parent
                    == shell.BattleFeedbackLayer,
                "REV11_FORMAL_PRESENTATION_EXTERNAL_PARENT_INVALID");
            RequireDirectChildrenExactly(
                shell.EnemyInfoArea,
                presentation.transform);
            RequireDirectChildrenExactly(
                shell.BattleFeedbackLayer,
                damageFloatPool.transform,
                cueFxAudioRoot.transform);

            C1ExactBattleSandboxCombatLogView combatLog =
                Single<C1ExactBattleSandboxCombatLogView>(root);
            RequirePrefabPath(combatLog.gameObject, CombatLogPrefabPath);
            Require(combatLog.transform.parent == root.transform
                    && combatLog.ValidateAuthoredReferences()
                    && !combatLog.IsOpen,
                "REV11_COMBAT_LOG_MOUNT_INVALID");

            C1ExactBattleSandboxItemDetailPresenter itemDetail =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            ItemDetailPanelView panel = Single<ItemDetailPanelView>(root);
            Button panelCloseButton = new SerializedObject(panel)
                .FindProperty("closeButton").objectReferenceValue as Button;
            RequirePrefabPath(panel.gameObject, ItemDetailPrefabPath);
            Require(HasFinalVisibleLoadedItemDetailState(root)
                    && itemDetail.transform == shell.ItemDetailPopupSlot
                    && itemDetail.transform.parent == root.transform
                    && itemDetail.PanelView == panel
                    && itemDetail.CloseButton != null
                    && itemDetail.CloseButton == panelCloseButton
                    && itemDetail.CloseButton.transform.IsChildOf(
                        panel.transform)
                    && itemDetail.ValidateAuthoredReferences()
                    && itemDetail.PopupCanvasGroup.alpha == 0f
                    && !itemDetail.PopupCanvasGroup.interactable
                    && !itemDetail.PopupCanvasGroup.blocksRaycasts,
                "REV11E_ITEM_DETAIL_VISIBLE_MOUNT_INVALID");
            RequireDirectChildrenExactly(
                itemDetail.transform,
                itemDetail.PresentationFrame);
            RequireDirectChildrenExactly(
                itemDetail.PresentationFrame,
                panel.transform);
            Require(root.GetComponentsInChildren<C1FormalItemBoardView>(true)
                        .Length == 0
                    && root.GetComponentsInChildren<C1FormalItemTrayView>(true)
                        .Length == 0
                    && root.GetComponentsInChildren<C1FormalItemCardView>(true)
                        .Length == 0,
                "REV11_APPROXIMATE_ITEM_CARRIER_REACHABLE");
            RequireNoForbiddenRuntimeOwner(root);
            bool hostValid = host.ValidateAuthoredBindings(
                out string hostDiagnostic);
            Require(host.CombatLogView == combatLog
                    && host.ItemDetailPresenter == itemDetail
                    && hostValid,
                "REV11_HOST_BINDING_INVALID " + hostDiagnostic);
            Require(CountMissingScriptsInHierarchy(root) == 0,
                "REV11_SHELL_MISSING_SCRIPT");
        }

        private static bool HasFinalCompositionShape(GameObject root)
        {
            return root.GetComponentsInChildren<
                       C1ExactBattleSandboxCombatLogView>(true).Length == 1
                   && root.GetComponentsInChildren<
                       C1ExactBattleSandboxItemDetailPresenter>(true).Length == 1
                   && root.GetComponentsInChildren<
                       UnifiedBattleFormalSceneHost>(true).Length == 1
                   && root.GetComponent<UnifiedBattleFormalSceneHost>() != null
                   && LegacyShellObjectNames.All(name =>
                       CountNamedObjects(root, name) == 0)
                   && CountNamedObjects(root, "BoardArea") == 0
                   && CountNamedObjects(root, "ItemTrayArea") == 0;
        }

        private static ItemDetailCarrierState ClassifyItemDetailCarrierState(
            GameObject root)
        {
            string yaml = NormalizeNewlines(File.ReadAllText(ShellPrefabPath));
            if (HasFinalCleanLoadedItemDetailState(root)
                && HasFinalCleanItemDetailYaml(yaml))
            {
                return ItemDetailCarrierState.FINAL_CLEAN;
            }

            if (HasFrozenPartialItemDetailYamlProof(yaml)
                && HasExactRepairableLoadedItemDetailState(root))
            {
                return ItemDetailCarrierState.EXACT_REPAIRABLE_PARTIAL;
            }

            return ItemDetailCarrierState.INVALID_CONFLICT;
        }

        private static ItemDetailPresentationMountState
            ClassifyItemDetailPresentationMountState(
                GameObject root)
        {
            if (HasFinalVisibleLoadedItemDetailState(root))
            {
                return ItemDetailPresentationMountState.FINAL_VISIBLE_MOUNT;
            }

            if (HasExactBlackScreenRepairableItemDetailState(root))
            {
                return ItemDetailPresentationMountState
                    .EXACT_BLACK_SCREEN_REPAIRABLE;
            }

            return ItemDetailPresentationMountState.INVALID_CONFLICT;
        }

        private static bool HasFinalCleanLoadedItemDetailState(GameObject root)
        {
            return HasFinalVisibleLoadedItemDetailState(root);
        }

        private static bool HasFinalVisibleLoadedItemDetailState(
            GameObject root)
        {
            if (!HasFinalCompositionShape(root)
                || CountMissingScriptsInHierarchy(root) != 0)
            {
                return false;
            }

            UnifiedBattlePageShell[] shells = root.GetComponentsInChildren<
                UnifiedBattlePageShell>(true);
            C1ExactBattleSandboxItemDetailPresenter[] presenters =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailPresenter>(true);
            ItemDetailPanelView[] panels = root.GetComponentsInChildren<
                ItemDetailPanelView>(true);
            if (shells.Length != 1
                || presenters.Length != 1
                || panels.Length != 1)
            {
                return false;
            }

            UnifiedBattlePageShell shell = shells[0];
            C1ExactBattleSandboxItemDetailPresenter presenter = presenters[0];
            ItemDetailPanelView panel = panels[0];
            Button panelCloseButton = PanelCloseButton(panel);
            RectTransform frame = presenter.PresentationFrame;
            return string.Equals(
                       NearestPrefabPath(panel.gameObject),
                       ItemDetailPrefabPath,
                       StringComparison.Ordinal)
                   && shell.ItemDetailPopupSlot != null
                   && presenter.transform == shell.ItemDetailPopupSlot
                   && presenter.transform.parent == root.transform
                   && presenter.PanelView == panel
                   && frame != null
                   && string.Equals(
                       frame.name,
                       ItemDetailPresentationFrameName,
                       StringComparison.Ordinal)
                   && CountNamedObjects(
                       root,
                       ItemDetailPresentationFrameName) == 1
                   && frame.parent == presenter.transform
                   && panel.transform.parent == frame
                   && frame.childCount == 1
                   && frame.GetComponents<Graphic>().Length == 0
                   && IsFullStretch((RectTransform)presenter.transform)
                   && IsAuthoredReferenceFrameGeometryValid(frame)
                   && IsSourcePanelGeometryValid(
                       (RectTransform)panel.transform)
                   && presenter.transform.GetSiblingIndex()
                   == root.transform.childCount - 1
                   && presenter.CloseButton != null
                   && presenter.CloseButton == panelCloseButton
                   && presenter.CloseButton.transform.IsChildOf(panel.transform)
                   && presenter.ValidateAuthoredReferences();
        }

        private static bool HasExactBlackScreenRepairableItemDetailState(
            GameObject root)
        {
            if (!HasFinalCompositionShape(root)
                || CountMissingScriptsInHierarchy(root) != 0
                || CountNamedObjects(
                    root,
                    ItemDetailPresentationFrameName) != 0)
            {
                return false;
            }

            UnifiedBattlePageShell[] shells = root.GetComponentsInChildren<
                UnifiedBattlePageShell>(true);
            C1ExactBattleSandboxItemDetailPresenter[] presenters =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailPresenter>(true);
            ItemDetailPanelView[] panels = root.GetComponentsInChildren<
                ItemDetailPanelView>(true);
            if (shells.Length != 1
                || presenters.Length != 1
                || panels.Length != 1)
            {
                return false;
            }

            UnifiedBattlePageShell shell = shells[0];
            C1ExactBattleSandboxItemDetailPresenter presenter = presenters[0];
            ItemDetailPanelView panel = panels[0];
            RectTransform panelRect = (RectTransform)panel.transform;
            return string.Equals(
                       NearestPrefabPath(panel.gameObject),
                       ItemDetailPrefabPath,
                       StringComparison.Ordinal)
                   && shell.ItemDetailPopupSlot != null
                   && presenter.transform == shell.ItemDetailPopupSlot
                   && presenter.transform.parent == root.transform
                   && IsLegacyItemDetailBindingValid(presenter, panel)
                   && panel.transform.parent == presenter.transform
                   && IsFullStretch((RectTransform)presenter.transform)
                   && panelRect.anchorMin == Vector2.zero
                   && panelRect.anchorMax == Vector2.one
                   && panelRect.pivot == new Vector2(0.5f, 0.5f)
                   && panelRect.anchoredPosition == Vector2.zero
                   && panelRect.sizeDelta == new Vector2(720f, 1280f)
                   && panelRect.localScale == Vector3.one
                   && panelRect.localRotation == Quaternion.identity;
        }

        private static bool HasExactRepairableLoadedItemDetailState(
            GameObject root)
        {
            if (!HasFinalCompositionShape(root)
                || CountMissingScriptsInHierarchy(root) != 1)
            {
                return false;
            }

            C1ExactBattleSandboxItemDetailPresenter[] presenters =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailPresenter>(true);
            ItemDetailPanelView[] panels = root.GetComponentsInChildren<
                ItemDetailPanelView>(true);
            if (presenters.Length != 1 || panels.Length != 1)
            {
                return false;
            }

            C1ExactBattleSandboxItemDetailPresenter presenter = presenters[0];
            ItemDetailPanelView panel = panels[0];
            return string.Equals(
                       NearestPrefabPath(panel.gameObject),
                       ItemDetailPrefabPath,
                       StringComparison.Ordinal)
                   && GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                       panel.gameObject) == 1
                   && IsLegacyItemDetailBindingValid(presenter, panel)
                   && presenter.PresentationFrame == null
                   && panel.transform.parent == presenter.transform;
        }

        private static void RequireFinalCleanLoadedItemDetailState(
            GameObject root)
        {
            Require(HasFinalCleanLoadedItemDetailState(root),
                "REV11D_FINAL_CLEAN_LOADED_ITEM_DETAIL_INVALID");
        }

        private static bool HasFinalCleanItemDetailYaml(string yaml)
        {
            string missingScript = "  m_Script: {fileID: 0}";
            string addedObject = Rev11cAddedObjectLine();
            string missingRelayFileId = Rev11cMissingRelayFileId.ToString(
                CultureInfo.InvariantCulture);
            string forbiddenRelayToken =
                "C1ExactBattleSandboxItemDetail" + "CloseRelay";
            return CountOrdinal(yaml, missingScript) == 0
                   && CountOrdinal(yaml, addedObject) == 0
                   && CountOrdinal(yaml, Rev11cExpectedAddedOverride()) == 0
                   && CountOrdinal(yaml, missingRelayFileId) == 0
                   && CountOrdinal(yaml, forbiddenRelayToken) == 0;
        }

        private static bool HasFrozenPartialItemDetailYamlProof(string yaml)
        {
            string header = Rev11cMissingComponentHeader();
            int start = yaml.IndexOf(header, StringComparison.Ordinal);
            int end = start < 0
                ? -1
                : yaml.IndexOf(
                    "\n--- !u!",
                    start + header.Length,
                    StringComparison.Ordinal);
            return CountOrdinal(yaml, "  m_Script: {fileID: 0}") == 1
                   && CountOrdinal(yaml, Rev11cAddedObjectLine()) == 1
                   && CountOrdinal(yaml, Rev11cExpectedAddedOverride()) == 1
                   && start >= 0
                   && end > start
                   && string.Equals(
                       yaml.Substring(start, end - start),
                       Rev11cExpectedMissingComponentBlock(),
                       StringComparison.Ordinal);
        }

        private static void ValidateDeleteInventoryHasNoLiveConsumer()
        {
            HashSet<string> deleteSet = new HashSet<string>(
                DeleteAssetPaths.Select(NormalizePath),
                StringComparer.OrdinalIgnoreCase);
            foreach (string path in DeleteAssetPaths)
            {
                deleteSet.Add(NormalizePath(path + ".meta"));
            }
            deleteSet.Add(NormalizePath(ThisAuthoringPath));

            string[] searchableFiles = Directory
                .GetFiles("Assets", "*", SearchOption.AllDirectories)
                .Where(IsTextReachabilityFile)
                .ToArray();
            foreach (string assetPath in DeleteAssetPaths)
            {
                Require(File.Exists(assetPath),
                    "REV11_DELETE_INTAKE_MISSING " + assetPath);
                string metaPath = assetPath + ".meta";
                Require(File.Exists(metaPath),
                    "REV11_DELETE_META_MISSING " + metaPath);
                string guid = File.ReadLines(metaPath)
                    .First(value => value.StartsWith(
                        "guid: ",
                        StringComparison.Ordinal))
                    .Substring("guid: ".Length).Trim();
                foreach (string candidate in searchableFiles)
                {
                    string normalized = NormalizePath(candidate);
                    if (deleteSet.Contains(normalized))
                    {
                        continue;
                    }
                    if (File.ReadAllText(candidate).IndexOf(
                            guid,
                            StringComparison.Ordinal) >= 0)
                    {
                        throw new InvalidOperationException(
                            "REV11_DELETE_LIVE_GUID_CONSUMER " + assetPath
                            + " -> " + normalized);
                    }
                }

                if (!assetPath.EndsWith(".cs", StringComparison.Ordinal))
                {
                    continue;
                }
                string typeName = Path.GetFileNameWithoutExtension(assetPath);
                foreach (string candidate in searchableFiles.Where(value =>
                             value.EndsWith(".cs", StringComparison.Ordinal)))
                {
                    string normalized = NormalizePath(candidate);
                    if (deleteSet.Contains(normalized))
                    {
                        continue;
                    }
                    if (File.ReadAllText(candidate).IndexOf(
                            typeName,
                            StringComparison.Ordinal) >= 0)
                    {
                        throw new InvalidOperationException(
                            "REV11_DELETE_LIVE_TYPE_CONSUMER " + typeName
                            + " -> " + normalized);
                    }
                }
            }
        }

        private static void RepairFrozenPartialItemDetailRelay(
            GameObject root)
        {
            ValidateFrozenPartialItemDetailRelayYaml();
            Require(CountMissingScriptsInHierarchy(root) == 1,
                "REV11C_PARTIAL_SHELL_MISSING_SCRIPT_COUNT_INVALID");

            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            ItemDetailPanelView panel = Single<ItemDetailPanelView>(root);
            RequirePrefabPath(panel.gameObject, ItemDetailPrefabPath);
            Button panelCloseButton = new SerializedObject(panel)
                .FindProperty("closeButton").objectReferenceValue as Button;
            Require(presenter.PanelView == panel
                    && presenter.CloseButton != null
                    && presenter.CloseButton == panelCloseButton
                    && presenter.CloseButton.transform.IsChildOf(
                        panel.transform),
                "REV11C_PARTIAL_ITEM_DETAIL_BINDING_INVALID");
            Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        panel.gameObject) == 1,
                "REV11C_PARTIAL_ITEM_DETAIL_MISSING_SCRIPT_IDENTITY_INVALID");

            int removed =
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(
                    panel.gameObject);
            Require(removed == 1,
                "REV11C_PARTIAL_ITEM_DETAIL_MISSING_SCRIPT_REMOVE_FAILED"
                + " removed=" + removed);
            Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        panel.gameObject) == 0
                    && CountMissingScriptsInHierarchy(root) == 0,
                "REV11C_PARTIAL_ITEM_DETAIL_MISSING_SCRIPT_REMAINS");
            EditorUtility.SetDirty(panel.gameObject);
        }

        private static void RepairExactBlackScreenItemDetailMount(
            GameObject root)
        {
            Require(HasExactBlackScreenRepairableItemDetailState(root),
                "REV11E_EXACT_BLACK_SCREEN_STATE_CHANGED");
            AuthorVisibleItemDetailMountFromLegacyState(root);
            Require(HasFinalVisibleLoadedItemDetailState(root),
                "REV11E_VISIBLE_MOUNT_REPAIR_FAILED");
        }

        private static void AuthorVisibleItemDetailMountFromLegacyState(
            GameObject root)
        {
            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            ItemDetailPanelView panel = Single<ItemDetailPanelView>(root);
            Require(IsLegacyItemDetailBindingValid(presenter, panel)
                    && presenter.PresentationFrame == null
                    && panel.transform.parent == presenter.transform
                    && CountNamedObjects(
                        root,
                        ItemDetailPresentationFrameName) == 0,
                "REV11E_LEGACY_ITEM_DETAIL_MOUNT_INVALID");

            RectTransform slot = (RectTransform)presenter.transform;
            RectTransform frame = CreateItemDetailPresentationFrame(slot);
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.SetParent(frame, false);
            ConfigureSourceItemDetailPanelTransform(panelRect);
            PrefabUtility.RecordPrefabInstancePropertyModifications(panelRect);

            presenter.AssignForEditor(
                panel,
                presenter.CloseButton,
                presenter.PopupCanvasGroup,
                frame);
            ConfigureClosedGroup(presenter.PopupCanvasGroup);
            panel.Close();
            slot.SetAsLastSibling();
            EditorUtility.SetDirty(presenter);
            EditorUtility.SetDirty(frame.gameObject);
        }

        private static void ValidateFrozenPartialItemDetailRelayYaml()
        {
            string yaml = NormalizeNewlines(File.ReadAllText(ShellPrefabPath));
            string missingScript = "  m_Script: {fileID: 0}";
            string addedObject = Rev11cAddedObjectLine();
            Require(CountOrdinal(yaml, missingScript) == 1
                    && CountOrdinal(yaml, addedObject) == 1,
                "REV11C_PARTIAL_YAML_OVERRIDE_COUNT_INVALID");

            string expectedAddedOverride = Rev11cExpectedAddedOverride();
            Require(CountOrdinal(yaml, expectedAddedOverride) == 1,
                "REV11C_PARTIAL_YAML_ADDED_OVERRIDE_IDENTITY_INVALID");

            string header = Rev11cMissingComponentHeader();
            int start = yaml.IndexOf(header, StringComparison.Ordinal);
            int end = start < 0
                ? -1
                : yaml.IndexOf("\n--- !u!", start + header.Length,
                    StringComparison.Ordinal);
            Require(start >= 0 && end > start,
                "REV11C_PARTIAL_YAML_MISSING_COMPONENT_BLOCK_INVALID");
            string actualBlock = yaml.Substring(start, end - start);
            string expectedBlock = Rev11cExpectedMissingComponentBlock();
            Require(string.Equals(
                    actualBlock,
                    expectedBlock,
                    StringComparison.Ordinal),
                "REV11C_PARTIAL_YAML_MISSING_COMPONENT_PAYLOAD_INVALID");
        }

        private static string Rev11cAddedObjectLine()
        {
            return "      addedObject: {fileID: "
                   + Rev11cMissingRelayFileId.ToString(
                       CultureInfo.InvariantCulture)
                   + "}";
        }

        private static string Rev11cExpectedAddedOverride()
        {
            return "    m_AddedComponents:\n"
                   + "    - targetCorrespondingSourceObject: {fileID: 2701806788130382786, guid: 5f5c011600b50b249831e979eeb58313, type: 3}\n"
                   + "      insertIndex: -1\n"
                   + Rev11cAddedObjectLine();
        }

        private static string Rev11cMissingComponentHeader()
        {
            return "--- !u!114 &"
                   + Rev11cMissingRelayFileId.ToString(
                       CultureInfo.InvariantCulture)
                   + "\n";
        }

        private static string Rev11cExpectedMissingComponentBlock()
        {
            return Rev11cMissingComponentHeader()
                   + "MonoBehaviour:\n"
                   + "  m_ObjectHideFlags: 0\n"
                   + "  m_CorrespondingSourceObject: {fileID: 0}\n"
                   + "  m_PrefabInstance: {fileID: 0}\n"
                   + "  m_PrefabAsset: {fileID: 0}\n"
                   + "  m_GameObject: {fileID: 6032628012687518763}\n"
                   + "  m_Enabled: 1\n"
                   + "  m_EditorHideFlags: 0\n"
                   + "  m_Script: {fileID: 0}\n"
                   + "  m_Name: \n"
                   + "  m_EditorClassIdentifier: \n"
                   + "  presenter: {fileID: 1395548109123010221}\n"
                   + "  panelView: {fileID: 7889701992368210192}";
        }

        private static int CountMissingScriptsInHierarchy(GameObject root)
        {
            return root.GetComponentsInChildren<Transform>(true).Sum(value =>
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    value.gameObject));
        }

        private static int CountOrdinal(string value, string needle)
        {
            int count = 0;
            int index = 0;
            while ((index = value.IndexOf(
                       needle,
                       index,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += needle.Length;
            }
            return count;
        }

        private static string NormalizeNewlines(string value)
        {
            return value.Replace("\r\n", "\n").Replace('\r', '\n');
        }

        private static void ValidateSourceItemDetailPresentationFacts()
        {
            string yaml = NormalizeNewlines(
                File.ReadAllText(BattleSandboxSourceScenePath));
            const string panelGeometry =
                "  m_Name: ItemDetailPanel\n"
                + "  m_TagString: Untagged\n";
            const string panelTransform =
                "  m_LocalScale: {x: 0.7999976, y: 0.7999976, z: 0.7999976}\n"
                + "  m_ConstrainProportionsScale: 1\n"
                + "  m_Children:\n"
                + "  - {fileID: 544824689}\n"
                + "  - {fileID: 557588684}\n"
                + "  - {fileID: 205394044}\n"
                + "  - {fileID: 929304621}\n"
                + "  - {fileID: 1713979189}\n"
                + "  m_Father: {fileID: 1703867848}\n"
                + "  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n"
                + "  m_AnchorMin: {x: 0.5, y: 0.5}\n"
                + "  m_AnchorMax: {x: 0.5, y: 0.5}\n"
                + "  m_AnchoredPosition: {x: 0, y: -192}\n"
                + "  m_SizeDelta: {x: 1080, y: 1920}\n"
                + "  m_Pivot: {x: 0.5, y: 0.5}";
            const string popupLayerIdentity =
                "  m_Name: PopupLayer\n"
                + "  m_TagString: Untagged\n";
            Require(CountOrdinal(yaml, panelGeometry) == 1
                    && CountOrdinal(yaml, panelTransform) == 1
                    && CountOrdinal(yaml, popupLayerIdentity) == 1
                    && CountOrdinal(yaml, "  - {fileID: 857425571}") == 1,
                "REV11E_SOURCE_ITEM_DETAIL_PRESENTATION_FACT_INVALID");
        }

        private static void ConfigureFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        private static RectTransform CreateItemDetailPresentationFrame(
            RectTransform viewport)
        {
            Require(viewport != null
                    && viewport.GetComponentsInChildren<Transform>(true)
                        .All(value => !string.Equals(
                            value.name,
                            ItemDetailPresentationFrameName,
                            StringComparison.Ordinal)),
                "REV11E_ITEM_DETAIL_FRAME_PARTIAL_OR_DUPLICATE");
            GameObject frameObject = new GameObject(
                ItemDetailPresentationFrameName,
                typeof(RectTransform));
            frameObject.layer = viewport.gameObject.layer;
            RectTransform frame = (RectTransform)frameObject.transform;
            frame.SetParent(viewport, false);
            frame.anchorMin = new Vector2(0.5f, 0.5f);
            frame.anchorMax = new Vector2(0.5f, 0.5f);
            frame.pivot = new Vector2(0.5f, 0.5f);
            frame.anchoredPosition = Vector2.zero;
            frame.sizeDelta = new Vector2(
                C1ExactBattleSandboxItemDetailPresenter
                    .PresentationReferenceWidth,
                C1ExactBattleSandboxItemDetailPresenter
                    .PresentationReferenceHeight);
            frame.localRotation = Quaternion.identity;
            frame.localScale = Vector3.one;
            return frame;
        }

        private static void ConfigureSourceItemDetailPanelTransform(
            RectTransform panel)
        {
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.anchoredPosition = new Vector2(0f, -192f);
            panel.sizeDelta = new Vector2(1080f, 1920f);
            panel.localRotation = Quaternion.identity;
            panel.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }

        private static bool IsFullStretch(RectTransform rect)
        {
            return rect != null
                   && rect.anchorMin == Vector2.zero
                   && rect.anchorMax == Vector2.one
                   && rect.anchoredPosition == Vector2.zero
                   && rect.sizeDelta == Vector2.zero
                   && rect.pivot == new Vector2(0.5f, 0.5f)
                   && rect.localRotation == Quaternion.identity
                   && rect.localScale == Vector3.one;
        }

        private static bool IsAuthoredReferenceFrameGeometryValid(
            RectTransform frame)
        {
            return frame != null
                   && frame.anchorMin == new Vector2(0.5f, 0.5f)
                   && frame.anchorMax == new Vector2(0.5f, 0.5f)
                   && frame.pivot == new Vector2(0.5f, 0.5f)
                   && frame.anchoredPosition == Vector2.zero
                   && frame.sizeDelta == new Vector2(
                       C1ExactBattleSandboxItemDetailPresenter
                           .PresentationReferenceWidth,
                       C1ExactBattleSandboxItemDetailPresenter
                           .PresentationReferenceHeight)
                   && frame.localRotation == Quaternion.identity
                   && frame.localScale == Vector3.one;
        }

        private static bool IsSourcePanelGeometryValid(RectTransform panel)
        {
            return panel != null
                   && panel.anchorMin == new Vector2(0.5f, 0.5f)
                   && panel.anchorMax == new Vector2(0.5f, 0.5f)
                   && panel.pivot == new Vector2(0.5f, 0.5f)
                   && panel.anchoredPosition == new Vector2(0f, -192f)
                   && panel.sizeDelta == new Vector2(1080f, 1920f)
                   && panel.localRotation == Quaternion.identity
                   && panel.localScale == new Vector3(0.8f, 0.8f, 0.8f);
        }

        private static Button PanelCloseButton(ItemDetailPanelView panel)
        {
            SerializedProperty property = panel == null
                ? null
                : new SerializedObject(panel).FindProperty("closeButton");
            return property == null
                ? null
                : property.objectReferenceValue as Button;
        }

        private static bool IsLegacyItemDetailBindingValid(
            C1ExactBattleSandboxItemDetailPresenter presenter,
            ItemDetailPanelView panel)
        {
            Button close = PanelCloseButton(panel);
            return presenter != null
                   && panel != null
                   && presenter.PanelView == panel
                   && presenter.CloseButton != null
                   && presenter.CloseButton == close
                   && presenter.CloseButton.transform.IsChildOf(panel.transform)
                   && presenter.PopupCanvasGroup != null
                   && presenter.PopupCanvasGroup.transform
                   == presenter.transform;
        }

        private static void ConfigureClosedGroup(CanvasGroup group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            group.ignoreParentGroups = false;
        }

        private static void PruneAnchorToAcceptedChildren(
            Transform anchor,
            IReadOnlyCollection<Transform> accepted)
        {
            HashSet<Transform> acceptedSet = new HashSet<Transform>(accepted);
            for (int index = anchor.childCount - 1; index >= 0; index--)
            {
                Transform child = anchor.GetChild(index);
                if (!acceptedSet.Contains(child))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void RequireDirectChildrenExactly(
            Transform parent,
            params Transform[] expected)
        {
            Transform[] actual = parent.Cast<Transform>().ToArray();
            Require(actual.Length == expected.Length
                    && actual.All(value => expected.Contains(value)),
                "REV11_ANCHOR_CHILDREN_INVALID " + parent.name);
        }

        private static void RequireRelativeOrderPreserved(
            Transform root,
            IReadOnlyList<Transform> expected)
        {
            Transform[] actual = root.Cast<Transform>()
                .Where(value => expected.Contains(value))
                .ToArray();
            Require(actual.SequenceEqual(expected),
                "REV11_UNRELATED_SIBLING_ORDER_CHANGED");
        }

        private static void RequireNoForbiddenRuntimeOwner(GameObject root)
        {
            foreach (MonoBehaviour behaviour in root
                         .GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }
                string fullName = behaviour.GetType().FullName ?? string.Empty;
                Require(fullName.IndexOf(
                            "BuildSandbox",
                            StringComparison.Ordinal) < 0
                        && fullName.IndexOf(
                            "ItemSandbox",
                            StringComparison.Ordinal) < 0
                        && fullName.IndexOf(
                            "BuildGridInteractionPreview",
                            StringComparison.Ordinal) < 0
                        && fullName.IndexOf(
                            "UnifiedBattleBattleLikePreviewAreaBridge",
                            StringComparison.Ordinal) < 0,
                    "REV11_FORBIDDEN_RUNTIME_OWNER " + fullName);
            }
        }

        private static void OmitFormalGuajiann(
            GameObject root,
            C1ExactBattleSandboxPrepareSurfacePresenter prepare)
        {
            RequirePrefabPath(prepare.gameObject, PrepareSurfacePrefabPath);
            GameObject sourceGuajiann = RequireSourceGuajiannContract();
            Transform[] allNamedMatches = root
                .GetComponentsInChildren<Transform>(true)
                .Where(value => string.Equals(
                    value.name,
                    FormalGuajiannName,
                    StringComparison.Ordinal))
                .ToArray();
            Transform relativeMatch = prepare.transform.Find(
                FormalGuajiannRelativePath);
            Require(allNamedMatches.Length == 1
                    && relativeMatch != null
                    && allNamedMatches[0] == relativeMatch,
                "REV11B_FORMAL_GUAJIAN_INSTANCE_PATH_INVALID"
                + " namedCount=" + allNamedMatches.Length
                + " relativeFound=" + (relativeMatch != null));

            GameObject instanceGuajiann = relativeMatch.gameObject;
            GameObject correspondingSource =
                PrefabUtility.GetCorrespondingObjectFromSource(
                    instanceGuajiann);
            Require(correspondingSource == sourceGuajiann
                    && PrefabUtility.GetNearestPrefabInstanceRoot(
                        instanceGuajiann) == prepare.gameObject,
                "REV11B_FORMAL_GUAJIAN_SOURCE_IDENTITY_INVALID");
            RequireGuajiannHistoricalContract(instanceGuajiann, root);

            Object.DestroyImmediate(instanceGuajiann);
            Require(CountNamedObjects(root, FormalGuajiannName) == 0,
                "REV11B_FORMAL_GUAJIAN_REMOVE_FAILED");
            RequireGuajiannRemovedOverride(prepare, sourceGuajiann);
        }

        private static void ValidateFormalGuajiannOmission(
            GameObject root,
            C1ExactBattleSandboxPrepareSurfacePresenter prepare)
        {
            Require(CountNamedObjects(root, FormalGuajiannName) == 0
                    && prepare.transform.Find(FormalGuajiannRelativePath)
                    == null,
                "REV11B_FORMAL_GUAJIAN_REMAINS");
            GameObject sourceGuajiann = RequireSourceGuajiannContract();
            RequireGuajiannRemovedOverride(prepare, sourceGuajiann);
            RequireNoStaleGuajiannCallback(root);
        }

        private static GameObject RequireSourceGuajiannContract()
        {
            GameObject sourcePrepare = RequirePrefab(
                PrepareSurfacePrefabPath);
            Require(string.Equals(
                    sourcePrepare.name,
                    "C1ExactBattleSandboxPrepareSurface",
                    StringComparison.Ordinal),
                "REV11B_PREPARE_SOURCE_ROOT_IDENTITY_INVALID");
            Transform[] namedMatches = sourcePrepare
                .GetComponentsInChildren<Transform>(true)
                .Where(value => string.Equals(
                    value.name,
                    FormalGuajiannName,
                    StringComparison.Ordinal))
                .ToArray();
            Transform relativeMatch = sourcePrepare.transform.Find(
                FormalGuajiannRelativePath);
            Require(namedMatches.Length == 1
                    && relativeMatch != null
                    && namedMatches[0] == relativeMatch
                    && relativeMatch.parent == sourcePrepare.transform
                    && string.Equals(
                        AssetDatabase.GetAssetPath(relativeMatch.gameObject),
                        PrepareSurfacePrefabPath,
                        StringComparison.Ordinal),
                "REV11B_GUAJIAN_SOURCE_PATH_INVALID"
                + " namedCount=" + namedMatches.Length
                + " relativeFound=" + (relativeMatch != null));
            RequireGuajiannHistoricalContract(
                relativeMatch.gameObject,
                sourcePrepare);
            return relativeMatch.gameObject;
        }

        private static void RequireGuajiannHistoricalContract(
            GameObject guajiann,
            GameObject consumerRoot)
        {
            Require(!guajiann.activeSelf,
                "REV11B_GUAJIAN_ACTIVE_STATE_INVALID");
            BuildSandboxSpriteSequencePlayer[] sequencePlayers =
                guajiann.GetComponents<BuildSandboxSpriteSequencePlayer>();
            Image[] images = guajiann.GetComponents<Image>();
            Button[] buttons = guajiann.GetComponents<Button>();
            Require(sequencePlayers.Length == 1
                    && images.Length == 1
                    && buttons.Length == 1,
                "REV11B_GUAJIAN_COMPONENT_IDENTITY_INVALID"
                + " sequence=" + sequencePlayers.Length
                + " image=" + images.Length
                + " button=" + buttons.Length);

            BuildSandboxSpriteSequencePlayer sequence = sequencePlayers[0];
            SerializedObject serializedSequence = new SerializedObject(
                sequence);
            SerializedProperty targetImage = serializedSequence.FindProperty(
                "targetImage");
            Require(targetImage != null
                    && targetImage.objectReferenceValue == images[0]
                    && images[0].gameObject == guajiann,
                "REV11B_GUAJIAN_SEQUENCE_TARGET_INVALID");

            Button button = buttons[0];
            Require(button.onClick.GetPersistentEventCount() == 1
                    && button.onClick.GetPersistentTarget(0) == sequence
                    && string.Equals(
                        button.onClick.GetPersistentMethodName(0),
                        FormalGuajiannCallbackMethod,
                        StringComparison.Ordinal),
                "REV11B_GUAJIAN_PERSISTENT_CALLBACK_INVALID");

            List<SerializedObjectReference> consumers =
                CollectSerializedObjectReferences(consumerRoot, sequence);
            Require(consumers.Count == 1
                    && consumers[0].Owner == button
                    && consumers[0].PropertyPath.EndsWith(
                        ".m_Target",
                        StringComparison.Ordinal),
                "REV11B_GUAJIAN_UNEXPECTED_CONSUMER"
                + " count=" + consumers.Count
                + (consumers.Count == 0
                    ? string.Empty
                    : " first=" + consumers[0].Owner.GetType().FullName
                      + ":" + consumers[0].PropertyPath));
        }

        private static List<SerializedObjectReference>
            CollectSerializedObjectReferences(
                GameObject root,
                Object target)
        {
            List<SerializedObjectReference> matches =
                new List<SerializedObjectReference>();
            foreach (Component component in root
                         .GetComponentsInChildren<Component>(true))
            {
                if (component == null || component == target)
                {
                    continue;
                }
                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty property = serialized.GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType
                            == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue == target)
                    {
                        matches.Add(new SerializedObjectReference(
                            component,
                            property.propertyPath));
                    }
                }
            }
            return matches;
        }

        private static void RequireGuajiannRemovedOverride(
            C1ExactBattleSandboxPrepareSurfacePresenter prepare,
            GameObject sourceGuajiann)
        {
            RemovedGameObject[] matches =
                (PrefabUtility.GetRemovedGameObjects(prepare.gameObject)
                 ?? new List<RemovedGameObject>())
                .Where(value => value != null
                                && value.assetGameObject == sourceGuajiann)
                .ToArray();
            Require(matches.Length == 1
                    && matches[0].parentOfRemovedGameObjectInInstance
                    == prepare.gameObject,
                "REV11B_GUAJIAN_REMOVED_OVERRIDE_INVALID"
                + " matchCount=" + matches.Length);
        }

        private static void RequireNoStaleGuajiannCallback(GameObject root)
        {
            foreach (Button button in root
                         .GetComponentsInChildren<Button>(true))
            {
                int count = button.onClick.GetPersistentEventCount();
                for (int index = 0; index < count; index++)
                {
                    Object target = button.onClick.GetPersistentTarget(index);
                    string method = button.onClick.GetPersistentMethodName(index);
                    string targetType = target == null
                        ? string.Empty
                        : target.GetType().FullName ?? string.Empty;
                    Require(!string.Equals(
                                method,
                                FormalGuajiannCallbackMethod,
                                StringComparison.Ordinal)
                            && !string.Equals(
                                targetType,
                                typeof(BuildSandboxSpriteSequencePlayer)
                                    .FullName,
                                StringComparison.Ordinal),
                        "REV11B_STALE_GUAJIAN_CALLBACK"
                        + " button=" + button.name
                        + " method=" + method
                        + " target=" + targetType);
                }
            }
        }

        private static void ValidateDynamicItemProviderShellForCleanup(
            GameObject root)
        {
            Require(root != null,
                "DYNAMIC_ITEM_PROVIDER_SHELL_ROOT_MISSING");
            UnifiedBattlePageShell shell = Single<UnifiedBattlePageShell>(root);
            UnifiedBattleFormalSceneHost host =
                Single<UnifiedBattleFormalSceneHost>(root);
            C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                Single<C1ExactBattleSandboxItemArrangementPresenter>(root);
            C1ExactBattleSandboxItemBoardView board =
                Single<C1ExactBattleSandboxItemBoardView>(root);
            C1ExactBattleSandboxItemTrayView tray =
                Single<C1ExactBattleSandboxItemTrayView>(root);
            FormalBattlePresentationRoot presentation =
                Single<FormalBattlePresentationRoot>(root);
            SerializedObject serializedHost = new SerializedObject(host);
            Require(shell.transform == root.transform
                    && host.transform == root.transform
                    && itemPresenter.ValidateAuthoredReferences()
                    && board.ValidateAuthoredReferences()
                    && tray.ValidateAuthoredReferences()
                    && board.PresentationCatalogSnapshot != null
                    && !presentation.ItemSourceProviderBound
                    && presentation.ValidateSourceCarrierReferences(out _)
                    && presentation.ValidateDownstreamCompositionReferences(
                        out _)
                    && serializedHost.FindProperty("itemPresenter")
                           ?.objectReferenceValue == itemPresenter
                    && serializedHost.FindProperty("itemBoardView")
                           ?.objectReferenceValue == board
                    && serializedHost.FindProperty("itemTrayView")
                           ?.objectReferenceValue == tray
                    && serializedHost.FindProperty("formalPresentationRoot")
                           ?.objectReferenceValue == presentation,
                "DYNAMIC_ITEM_PROVIDER_LIFECYCLE_REFERENCE_INVALID");
            Require(host.ValidateAuthoredBindings(out string diagnostic),
                "DYNAMIC_ITEM_PROVIDER_HOST_BINDING_INVALID " + diagnostic);
            Require(CountMissingScriptsInHierarchy(root) == 0,
                "DYNAMIC_ITEM_PROVIDER_SHELL_MISSING_SCRIPT");
            RequireNoForbiddenRuntimeOwner(root);
        }

        private static void ValidateRetiredExactSourceCardViewsModifications(
            IReadOnlyList<PropertyModification> retired)
        {
            Require(retired != null && retired.Count >= 1,
                "DYNAMIC_ITEM_PROVIDER_RETIRED_WIRING_MISSING");
            PropertyModification[] sizeRows = retired.Where(value =>
                    string.Equals(
                        value.propertyPath,
                        RetiredExactSourceCardViewsProperty + ".Array.size",
                        StringComparison.Ordinal))
                .ToArray();
            int serializedCount = -1;
            Require(sizeRows.Length == 1
                    && int.TryParse(
                        sizeRows[0].value,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out serializedCount)
                    && serializedCount >= 0
                    && sizeRows[0].objectReference == null,
                "DYNAMIC_ITEM_PROVIDER_RETIRED_ARRAY_SIZE_INVALID");
            PropertyModification[] dataRows = retired.Where(value =>
                    !ReferenceEquals(value, sizeRows[0]))
                .ToArray();
            string[] expectedDataPaths = Enumerable.Range(0, serializedCount)
                .Select(index => RetiredExactSourceCardViewsProperty
                                 + ".Array.data["
                                 + index.ToString(
                                     CultureInfo.InvariantCulture)
                                 + "]")
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Require(dataRows.Length == serializedCount
                    && dataRows.Select(value => value.propertyPath)
                        .Distinct(StringComparer.Ordinal).Count()
                       == dataRows.Length
                    && dataRows.Select(value => value.propertyPath)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .SequenceEqual(expectedDataPaths)
                    && dataRows.All(value =>
                        value.propertyPath.StartsWith(
                            RetiredExactSourceCardViewsProperty
                            + ".Array.data[",
                            StringComparison.Ordinal)
                        && value.propertyPath.EndsWith(
                            "]",
                            StringComparison.Ordinal)
                        && string.IsNullOrEmpty(value.value)
                        && value.objectReference != null)
                    && dataRows.Select(value => value.objectReference)
                        .Distinct().Count() == dataRows.Length,
                "DYNAMIC_ITEM_PROVIDER_RETIRED_ARRAY_ROWS_INVALID");
        }

        private static bool IsRetiredExactSourceCardViewsProperty(
            string propertyPath)
        {
            return !string.IsNullOrEmpty(propertyPath)
                   && (string.Equals(
                           propertyPath,
                           RetiredExactSourceCardViewsProperty,
                           StringComparison.Ordinal)
                       || propertyPath.StartsWith(
                           RetiredExactSourceCardViewsProperty + ".",
                           StringComparison.Ordinal));
        }

        private static string CleanupPropertyModificationSignature(
            PropertyModification modification)
        {
            if (modification == null) return "<null>";
            return string.Join("|", new[]
            {
                modification.target == null
                    ? "0"
                    : modification.target.GetInstanceID().ToString(
                        CultureInfo.InvariantCulture),
                modification.propertyPath ?? string.Empty,
                modification.value ?? string.Empty,
                modification.objectReference == null
                    ? "0"
                    : modification.objectReference.GetInstanceID().ToString(
                        CultureInfo.InvariantCulture)
            });
        }

        private static T Single<T>(GameObject root) where T : Component
        {
            T[] values = root.GetComponentsInChildren<T>(true);
            Require(values.Length == 1,
                "REV11_COMPONENT_COUNT_INVALID " + typeof(T).FullName
                + " actual=" + values.Length);
            return values[0];
        }

        private static Transform RequiredDirectChild(
            Transform parent,
            string name)
        {
            Transform[] matches = parent.Cast<Transform>()
                .Where(value => string.Equals(
                    value.name,
                    name,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "REV11_DIRECT_CHILD_COUNT_INVALID " + name
                + " actual=" + matches.Length);
            return matches[0];
        }

        private static void RequireNoNamedObject(GameObject root, string name)
        {
            int count = CountNamedObjects(root, name);
            Require(count == 0,
                "REV11_OBSOLETE_OR_GHOST_OBJECT_REMAINS " + name
                + " actual=" + count);
        }

        private static int CountNamedObjects(GameObject root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true).Count(value =>
                string.Equals(value.name, name, StringComparison.Ordinal));
        }

        private static GameObject RequirePrefab(string path)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Require(asset != null, "REV11_PREFAB_MISSING " + path);
            return asset;
        }

        private static void RequirePrefabPath(GameObject value, string expected)
        {
            string actual = NearestPrefabPath(value);
            Require(string.Equals(actual, expected, StringComparison.Ordinal),
                "REV11_NESTED_PREFAB_SOURCE_INVALID expected=" + expected
                + " actual=" + actual);
        }

        private static string NearestPrefabPath(GameObject value)
        {
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(value)
                   ?? string.Empty;
        }

        private static bool IsTextReachabilityFile(string path)
        {
            string extension = Path.GetExtension(path);
            return string.Equals(extension, ".cs", StringComparison.Ordinal)
                   || string.Equals(extension, ".prefab", StringComparison.Ordinal)
                   || string.Equals(extension, ".unity", StringComparison.Ordinal)
                   || string.Equals(extension, ".asset", StringComparison.Ordinal)
                   || string.Equals(extension, ".meta", StringComparison.Ordinal)
                   || string.Equals(extension, ".controller", StringComparison.Ordinal);
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }

        private readonly struct TransformState
        {
            private readonly bool active;
            private readonly Vector3 localPosition;
            private readonly Quaternion localRotation;
            private readonly Vector3 localScale;
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 pivot;

            public TransformState(Transform transform)
            {
                Transform = transform;
                active = transform.gameObject.activeSelf;
                localPosition = transform.localPosition;
                localRotation = transform.localRotation;
                localScale = transform.localScale;
                if (transform is RectTransform rect)
                {
                    anchorMin = rect.anchorMin;
                    anchorMax = rect.anchorMax;
                    anchoredPosition = rect.anchoredPosition;
                    sizeDelta = rect.sizeDelta;
                    pivot = rect.pivot;
                }
                else
                {
                    anchorMin = Vector2.zero;
                    anchorMax = Vector2.zero;
                    anchoredPosition = Vector2.zero;
                    sizeDelta = Vector2.zero;
                    pivot = Vector2.zero;
                }
            }

            public Transform Transform { get; }

            public void RequireUnchanged()
            {
                Require(Transform != null
                        && Transform.gameObject.activeSelf == active
                        && Transform.localPosition == localPosition
                        && Transform.localRotation == localRotation
                        && Transform.localScale == localScale,
                    "REV11_UNRELATED_TRANSFORM_CHANGED");
                if (Transform is RectTransform rect)
                {
                    Require(rect.anchorMin == anchorMin
                            && rect.anchorMax == anchorMax
                            && rect.anchoredPosition == anchoredPosition
                            && rect.sizeDelta == sizeDelta
                            && rect.pivot == pivot,
                        "REV11_UNRELATED_RECT_TRANSFORM_CHANGED "
                        + Transform.name);
                }
            }
        }

        private readonly struct SerializedObjectReference
        {
            public SerializedObjectReference(
                Component owner,
                string propertyPath)
            {
                Owner = owner;
                PropertyPath = propertyPath;
            }

            public Component Owner { get; }
            public string PropertyPath { get; }
        }

        private readonly struct TextVisualState
        {
            private readonly Font font;
            private readonly int fontSize;
            private readonly FontStyle fontStyle;
            private readonly TextAnchor alignment;
            private readonly Color color;
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 pivot;
            private readonly Vector3 localScale;

            public TextVisualState(Text value)
            {
                font = value.font;
                fontSize = value.fontSize;
                fontStyle = value.fontStyle;
                alignment = value.alignment;
                color = value.color;
                RectTransform rect = value.rectTransform;
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                pivot = rect.pivot;
                localScale = rect.localScale;
            }

            public void RequireSameVisual(Text value)
            {
                RectTransform rect = value.rectTransform;
                Require(value.font == font
                        && value.fontSize == fontSize
                        && value.fontStyle == fontStyle
                        && value.alignment == alignment
                        && value.color == color
                        && rect.anchorMin == anchorMin
                        && rect.anchorMax == anchorMax
                        && rect.anchoredPosition == anchoredPosition
                        && rect.sizeDelta == sizeDelta
                        && rect.pivot == pivot
                        && rect.localScale == localScale,
                    "REV11_COMBAT_LOG_SOURCE_VISUAL_DRIFT");
            }
        }
    }
}
