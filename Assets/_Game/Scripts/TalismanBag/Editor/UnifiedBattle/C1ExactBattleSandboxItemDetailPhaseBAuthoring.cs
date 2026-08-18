using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.Presentation.ExactItemDetail;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class C1ExactBattleSandboxItemDetailPhaseBAuthoring
    {
        private const string MenuPath =
            "TalismanBag/V0.4/Unified Battle/Apply Phase B Exact Item Detail Once";
        private const string SourceScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string UnifiedScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        private const string ItemDetailPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab";
        private const string UnifiedShellPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";
        private const string RuntimePresenterPath =
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/ExactItemDetail/C1ExactBattleSandboxItemDetailPresenter.cs";
        private const string ItemDetailSlotName = "ItemDetailPopupSlot";
        private const string ItemDetailPanelName = "ItemDetailPanel";
        private const string SourcePopupParentName = "PopupLayer";
        private const string SourceOnlyPanelAdapterTypeName =
            "TalismanBag.ItemSandbox.ItemSandboxDetailPanelView";
        private const string SourceOnlySectionAdapterTypeName =
            "TalismanBag.ItemSandbox.ItemSandboxDetailSectionView";
        private const int ExpectedSourceOnlySectionAdapterCount = 18;
        private const string ExpectedItemDetailPrefabGuid =
            "5f5c011600b50b249831e979eeb58313";

        [MenuItem(MenuPath)]
        public static void ApplyFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "PHASE_B_AUTHORING_REQUIRES_NON_PLAY_EDITOR");
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                throw new InvalidOperationException(
                    "PHASE_B_AUTHORING_REQUIRES_IDLE_EDITOR");
            }

            Scene activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.isDirty)
            {
                throw new InvalidOperationException(
                    "PHASE_B_AUTHORING_REQUIRES_CLEAN_ACTIVE_SCENE");
            }

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
                    "[ExactItemDetailPhaseB] "
                    + "PHASE_B_EXACT_ITEM_DETAIL_AUTHORING_OR_VALIDATION_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ApplySinglePass()
        {
            ValidateRuntimeBoundary();
            Require(
                string.Equals(
                    AssetDatabase.AssetPathToGUID(ItemDetailPrefabPath),
                    ExpectedItemDetailPrefabGuid,
                    StringComparison.Ordinal),
                "EXACT_ITEM_DETAIL_PREFAB_GUID_CHANGED");

            Scene sourceScene = EditorSceneManager.OpenScene(
                SourceScenePath,
                OpenSceneMode.Single);
            ItemDetailPanelView sourcePanel = RequiredSourcePanel(sourceScene);
            string sourceVisualSignature =
                BuildPromotedSourceVisualSignature(sourcePanel);
            EnsureExactItemDetailPrefab(sourcePanel, sourceVisualSignature);
            AuthorUnifiedShell(sourceVisualSignature);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                UnifiedShellPrefabPath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);

            Scene unifiedScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            AuthorAndValidateUnifiedScene(unifiedScene, sourceVisualSignature);

            ValidateRuntimeBoundary();
            Require(
                string.Equals(
                    AssetDatabase.AssetPathToGUID(ItemDetailPrefabPath),
                    ExpectedItemDetailPrefabGuid,
                    StringComparison.Ordinal),
                "EXACT_ITEM_DETAIL_PREFAB_GUID_CHANGED_AFTER_AUTHORING");
            Debug.Log(
                "[ExactItemDetailPhaseB] "
                + "PHASE_B_EXACT_ITEM_DETAIL_AUTHORING_AND_VALIDATION_PASS "
                + "popup=1 slot=1 presenter=1 initialClosed=1 "
                + "sourceParity=1 formalSelectionBinding=1 missingScripts=0");
        }

        private static ItemDetailPanelView RequiredSourcePanel(Scene scene)
        {
            List<ItemDetailPanelView> matches = AllInScene<ItemDetailPanelView>(
                    scene)
                .Where(view => view != null
                               && string.Equals(
                                   view.gameObject.name,
                                   ItemDetailPanelName,
                                   StringComparison.Ordinal)
                               && view.transform.parent != null
                               && string.Equals(
                                   view.transform.parent.name,
                                   SourcePopupParentName,
                                   StringComparison.Ordinal))
                .ToList();
            Require(matches.Count == 1, "EXACT_ITEM_DETAIL_SOURCE_NOT_UNIQUE");

            ItemDetailPanelView panel = matches[0];
            Require(
                string.IsNullOrEmpty(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        panel.gameObject)),
                "EXACT_ITEM_DETAIL_SOURCE_MUST_BE_DIRECT_AUTHORED_HIERARCHY");
            ValidatePanelContract(panel.gameObject, true);
            return panel;
        }

        private static string BuildPromotedSourceVisualSignature(
            ItemDetailPanelView sourcePanel)
        {
            GameObject clone = null;
            try
            {
                clone = CreateSanitizedPromotionClone(sourcePanel);
                return BuildVisualSignature(clone);
            }
            finally
            {
                if (clone != null)
                {
                    Object.DestroyImmediate(clone);
                }
            }
        }

        private static GameObject CreateSanitizedPromotionClone(
            ItemDetailPanelView sourcePanel)
        {
            GameObject clone = Object.Instantiate(
                sourcePanel.gameObject,
                sourcePanel.transform.parent,
                false);
            try
            {
                clone.name = ItemDetailPanelName;
                clone.transform.SetParent(null, false);
                StripExactSourceOnlyAdapters(clone);
                ValidatePanelContract(clone, false);
                return clone;
            }
            catch
            {
                Object.DestroyImmediate(clone);
                throw;
            }
        }

        private static void EnsureExactItemDetailPrefab(
            ItemDetailPanelView sourcePanel,
            string sourceVisualSignature)
        {
            bool parity = false;
            GameObject current = null;
            try
            {
                current = PrefabUtility.LoadPrefabContents(ItemDetailPrefabPath);
                parity = current != null
                         && string.Equals(
                             BuildVisualSignature(current),
                             sourceVisualSignature,
                             StringComparison.Ordinal);
            }
            finally
            {
                if (current != null)
                {
                    PrefabUtility.UnloadPrefabContents(current);
                }
            }

            if (!parity)
            {
                GameObject clone = CreateSanitizedPromotionClone(sourcePanel);
                try
                {
                    Require(
                        string.Equals(
                            BuildVisualSignature(clone),
                            sourceVisualSignature,
                            StringComparison.Ordinal),
                        "EXACT_ITEM_DETAIL_SANITIZED_SOURCE_SIGNATURE_DRIFTED");
                    RequireNoBuildSandboxOwner(clone);
                    GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                        clone,
                        ItemDetailPrefabPath);
                    Require(saved != null, "EXACT_ITEM_DETAIL_PREFAB_SAVE_FAILED");
                }
                finally
                {
                    Object.DestroyImmediate(clone);
                }

                AssetDatabase.ImportAsset(
                    ItemDetailPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport
                    | ImportAssetOptions.ForceUpdate);
            }

            GameObject validationRoot = null;
            try
            {
                validationRoot = PrefabUtility.LoadPrefabContents(
                    ItemDetailPrefabPath);
                Require(
                    string.Equals(
                        BuildVisualSignature(validationRoot),
                        sourceVisualSignature,
                        StringComparison.Ordinal),
                    "EXACT_ITEM_DETAIL_PREFAB_SOURCE_PARITY_FAILED");
                ValidatePanelContract(validationRoot, false);
            }
            finally
            {
                if (validationRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(validationRoot);
                }
            }
        }

        private static void AuthorUnifiedShell(string sourceVisualSignature)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                UnifiedShellPrefabPath);
            try
            {
                UnifiedBattlePageShell shell = SingleInHierarchy<
                    UnifiedBattlePageShell>(root);
                UnifiedBattleFormalSceneHost host = SingleInHierarchy<
                    UnifiedBattleFormalSceneHost>(root);

                C1ExactBattleSandboxItemDetailPresenter presenter =
                    ResolveOrCreateSlot(root, shell, host);
                CanvasGroup canvasGroup = RequiredSingleComponent<CanvasGroup>(
                    presenter.gameObject);
                ConfigureClosedCanvasGroup(canvasGroup);

                ItemDetailPanelView panelView = ResolveOrCreatePanel(
                    presenter.transform);
                Require(
                    string.Equals(
                        ResolveNearestPrefabAssetPath(panelView.gameObject),
                        ItemDetailPrefabPath,
                        StringComparison.Ordinal),
                    "UNIFIED_ITEM_DETAIL_NESTED_SOURCE_LINK_INVALID");
                Require(
                    string.Equals(
                        BuildVisualSignatureFromPrefabAsset(ItemDetailPrefabPath),
                        sourceVisualSignature,
                        StringComparison.Ordinal),
                    "UNIFIED_ITEM_DETAIL_SOURCE_PARITY_DRIFTED");

                Button closeButton = RequiredObjectReference<Button>(
                    panelView,
                    "closeButton",
                    "EXACT_ITEM_DETAIL_CLOSE_CONTROL_MISSING");
                C1ExactBattleSandboxItemDetailCloseRelay relay =
                    panelView.GetComponent<
                        C1ExactBattleSandboxItemDetailCloseRelay>();
                if (relay == null)
                {
                    relay = panelView.gameObject.AddComponent<
                        C1ExactBattleSandboxItemDetailCloseRelay>();
                }

                Require(
                    panelView.GetComponents<
                        C1ExactBattleSandboxItemDetailCloseRelay>().Length == 1,
                    "UNIFIED_ITEM_DETAIL_CLOSE_RELAY_DUPLICATE");
                presenter.AssignForEditor(
                    panelView,
                    closeButton,
                    canvasGroup);
                relay.AssignForEditor(presenter, panelView);
                shell.AssignItemDetailPopupSlotForEditor(presenter.transform);
                host.AssignItemDetailForEditor(presenter);

                EditorUtility.SetDirty(relay);
                EditorUtility.SetDirty(presenter);
                EditorUtility.SetDirty(shell);
                EditorUtility.SetDirty(host);
                ValidateUnifiedComposition(root, sourceVisualSignature, false);

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    UnifiedShellPrefabPath);
                Require(saved != null, "UNIFIED_ITEM_DETAIL_SHELL_SAVE_FAILED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            GameObject validationRoot = PrefabUtility.LoadPrefabContents(
                UnifiedShellPrefabPath);
            try
            {
                ValidateUnifiedComposition(
                    validationRoot,
                    sourceVisualSignature,
                    false);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(validationRoot);
            }
        }

        private static C1ExactBattleSandboxItemDetailPresenter
            ResolveOrCreateSlot(
                GameObject root,
                UnifiedBattlePageShell shell,
                UnifiedBattleFormalSceneHost host)
        {
            C1ExactBattleSandboxItemDetailPresenter[] presenters =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailPresenter>(true);
            if (presenters.Length == 0)
            {
                Require(
                    shell.ItemDetailPopupSlot == null
                    && host.ItemDetailPresenter == null,
                    "UNIFIED_ITEM_DETAIL_PARTIAL_AUTHORED_STATE");
                GameObject slot = new GameObject(
                    ItemDetailSlotName,
                    typeof(RectTransform),
                    typeof(CanvasGroup),
                    typeof(C1ExactBattleSandboxItemDetailPresenter));
                slot.layer = shell.gameObject.layer;
                RectTransform rect = (RectTransform)slot.transform;
                rect.SetParent(shell.transform, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
                rect.SetAsLastSibling();
                return slot.GetComponent<
                    C1ExactBattleSandboxItemDetailPresenter>();
            }

            Require(
                presenters.Length == 1,
                "UNIFIED_ITEM_DETAIL_PRESENTER_DUPLICATE");
            C1ExactBattleSandboxItemDetailPresenter presenter = presenters[0];
            Require(
                presenter.transform.parent == shell.transform
                && string.Equals(
                    presenter.gameObject.name,
                    ItemDetailSlotName,
                    StringComparison.Ordinal),
                "UNIFIED_ITEM_DETAIL_SLOT_IDENTITY_INVALID");
            Require(
                shell.ItemDetailPopupSlot == presenter.transform
                && host.ItemDetailPresenter == presenter,
                "UNIFIED_ITEM_DETAIL_EXISTING_BINDING_PARTIAL_OR_INVALID");
            return presenter;
        }

        private static ItemDetailPanelView ResolveOrCreatePanel(
            Transform slot)
        {
            ItemDetailPanelView[] panels =
                slot.GetComponentsInChildren<ItemDetailPanelView>(true);
            if (panels.Length == 0)
            {
                GameObject panelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                    ItemDetailPrefabPath);
                Require(panelAsset != null, "EXACT_ITEM_DETAIL_PREFAB_MISSING");
                GameObject panelInstance = PrefabUtility.InstantiatePrefab(
                    panelAsset,
                    slot) as GameObject;
                Require(
                    panelInstance != null,
                    "UNIFIED_ITEM_DETAIL_NESTED_INSTANCE_CREATE_FAILED");
                panels = panelInstance.GetComponentsInChildren<
                    ItemDetailPanelView>(true);
            }

            Require(panels.Length == 1, "UNIFIED_ITEM_DETAIL_PANEL_DUPLICATE");
            Require(
                panels[0].transform.parent == slot,
                "UNIFIED_ITEM_DETAIL_PANEL_PARENT_INVALID");
            return panels[0];
        }

        private static void AuthorAndValidateUnifiedScene(
            Scene scene,
            string sourceVisualSignature)
        {
            UnifiedBattlePageShell shell = SingleInScene<
                UnifiedBattlePageShell>(scene);
            UnifiedBattleFormalSceneHost host = SingleInScene<
                UnifiedBattleFormalSceneHost>(scene);
            GameObject instanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(
                shell.gameObject);
            Require(
                instanceRoot != null
                && string.Equals(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        instanceRoot),
                    UnifiedShellPrefabPath,
                    StringComparison.Ordinal),
                "UNIFIED_ITEM_DETAIL_SCENE_SHELL_SOURCE_INVALID");

            string unrelatedBefore = BuildUnrelatedPropertyOverrideSignature(
                instanceRoot);
            bool changed = false;
            changed |= RevertExactNullReferenceOverride(
                shell,
                "itemDetailPopupSlot");
            changed |= RevertExactNullReferenceOverride(
                host,
                "itemDetailPresenter");
            string unrelatedAfter = BuildUnrelatedPropertyOverrideSignature(
                instanceRoot);
            Require(
                string.Equals(
                    unrelatedBefore,
                    unrelatedAfter,
                    StringComparison.Ordinal),
                "UNIFIED_ITEM_DETAIL_UNRELATED_SCENE_OVERRIDE_DRIFT");

            ValidateUnifiedComposition(
                instanceRoot,
                sourceVisualSignature,
                true);
            Require(
                !HasPropertyOverride(shell, "itemDetailPopupSlot")
                && !HasPropertyOverride(host, "itemDetailPresenter"),
                "UNIFIED_ITEM_DETAIL_SCENE_REFERENCE_OVERRIDE_REMAINS");
            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                Require(
                    EditorSceneManager.SaveScene(scene),
                    "UNIFIED_ITEM_DETAIL_SCENE_SAVE_FAILED");
                AssetDatabase.ImportAsset(
                    UnifiedScenePath,
                    ImportAssetOptions.ForceSynchronousImport
                    | ImportAssetOptions.ForceUpdate);
            }
        }

        private static void ValidateUnifiedComposition(
            GameObject root,
            string sourceVisualSignature,
            bool requireHostValidation)
        {
            UnifiedBattlePageShell shell = SingleInHierarchy<
                UnifiedBattlePageShell>(root);
            UnifiedBattleFormalSceneHost host = SingleInHierarchy<
                UnifiedBattleFormalSceneHost>(root);
            C1ExactBattleSandboxItemDetailPresenter presenter =
                SingleInHierarchy<C1ExactBattleSandboxItemDetailPresenter>(root);
            ItemDetailPanelView panel = SingleInHierarchy<ItemDetailPanelView>(
                presenter.gameObject);
            CanvasGroup canvasGroup = RequiredSingleComponent<CanvasGroup>(
                presenter.gameObject);
            C1ExactBattleSandboxItemDetailCloseRelay relay =
                RequiredSingleComponent<
                    C1ExactBattleSandboxItemDetailCloseRelay>(panel.gameObject);

            Require(
                shell.ItemDetailPopupSlot == presenter.transform
                && host.ItemDetailPresenter == presenter
                && presenter.transform.parent == shell.transform,
                "UNIFIED_ITEM_DETAIL_AUTHORED_REFERENCE_INVALID");
            Require(
                presenter.PanelView == panel
                && presenter.CloseButton != null
                && presenter.PopupCanvasGroup == canvasGroup
                && presenter.ValidateAuthoredReferences()
                && relay.ValidateAuthoredReferences(),
                "UNIFIED_ITEM_DETAIL_PRESENTER_BINDING_INVALID");
            Require(
                Mathf.Approximately(canvasGroup.alpha, 0f)
                && !canvasGroup.interactable
                && !canvasGroup.blocksRaycasts,
                "UNIFIED_ITEM_DETAIL_INITIAL_CLOSED_GATE_INVALID");
            Require(
                string.Equals(
                    ResolveNearestPrefabAssetPath(panel.gameObject),
                    ItemDetailPrefabPath,
                    StringComparison.Ordinal),
                "UNIFIED_ITEM_DETAIL_NESTED_SOURCE_LINK_INVALID");
            Require(
                string.Equals(
                    BuildVisualSignatureFromPrefabAsset(ItemDetailPrefabPath),
                    sourceVisualSignature,
                    StringComparison.Ordinal),
                "UNIFIED_ITEM_DETAIL_SOURCE_PARITY_DRIFTED");
            Require(
                root.GetComponentsInChildren<ItemDetailPanelView>(true).Length == 1
                && root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailPresenter>(true).Length == 1,
                "UNIFIED_ITEM_DETAIL_POPUP_NOT_SINGLETON");
            ValidateNoMissingScripts(root);
            RequireNoBuildSandboxOwner(root);
            if (requireHostValidation)
            {
                Require(
                    host.ValidateAuthoredBindings(out string diagnostic),
                    "UNIFIED_ITEM_DETAIL_HOST_BINDING_REJECTED " + diagnostic);
            }
        }

        private static void ValidatePanelContract(
            GameObject root,
            bool requireDirectSource)
        {
            Require(
                root != null
                && string.Equals(
                    root.name,
                    ItemDetailPanelName,
                    StringComparison.Ordinal),
                "EXACT_ITEM_DETAIL_PANEL_IDENTITY_INVALID");
            ItemDetailPanelView panel = SingleInHierarchy<ItemDetailPanelView>(
                root);
            Button closeButton = RequiredObjectReference<Button>(
                panel,
                "closeButton",
                "EXACT_ITEM_DETAIL_CLOSE_CONTROL_MISSING");
            Require(
                closeButton.transform.IsChildOf(panel.transform),
                "EXACT_ITEM_DETAIL_CLOSE_CONTROL_OUTSIDE_PANEL");
            Require(
                root.GetComponentsInChildren<ScrollRect>(true).Length >= 2,
                "EXACT_ITEM_DETAIL_SCROLL_RECTS_MISSING");
            Require(
                RequiredObjectReference<ScrollRect>(
                    panel,
                    "detailScrollRect",
                    "EXACT_ITEM_DETAIL_DETAIL_SCROLL_MISSING") != null
                && RequiredObjectReference<ScrollRect>(
                    panel,
                    "debugScrollRect",
                    "EXACT_ITEM_DETAIL_DEBUG_SCROLL_MISSING") != null,
                "EXACT_ITEM_DETAIL_SCROLL_BINDING_INVALID");
            if (requireDirectSource)
            {
                Require(
                    !PrefabUtility.IsPartOfPrefabInstance(root),
                    "EXACT_ITEM_DETAIL_SOURCE_WAS_NOT_DIRECT_AUTHORED");
            }

            ValidateNoMissingScripts(root);
            if (requireDirectSource)
            {
                RequireExactKnownSourceOnlyAdapters(root);
            }
            else
            {
                RequireNoBuildSandboxOwner(root);
            }
        }

        private static void StripExactSourceOnlyAdapters(GameObject root)
        {
            List<MonoBehaviour> adapters =
                RequireExactKnownSourceOnlyAdapters(root);
            foreach (MonoBehaviour adapter in adapters)
            {
                Object.DestroyImmediate(adapter);
            }

            RequireNoBuildSandboxOwner(root);
        }

        private static List<MonoBehaviour> RequireExactKnownSourceOnlyAdapters(
            GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<
                MonoBehaviour>(true);
            List<MonoBehaviour> forbidden = behaviours
                .Where(behaviour => behaviour != null
                                    && IsForbiddenOwnerType(
                                        behaviour.GetType().FullName))
                .ToList();
            RequireKnownSourceOnlyAdapterTypeSet(forbidden
                .Select(behaviour => behaviour.GetType().FullName)
                .ToArray());

            List<MonoBehaviour> panelAdapters = forbidden
                .Where(behaviour => string.Equals(
                    behaviour.GetType().FullName,
                    SourceOnlyPanelAdapterTypeName,
                    StringComparison.Ordinal))
                .ToList();
            Require(
                panelAdapters[0].gameObject == root,
                "EXACT_ITEM_DETAIL_SOURCE_ADAPTER_LOCATION_INVALID "
                + SourceOnlyPanelAdapterTypeName);

            foreach (MonoBehaviour sectionAdapter in forbidden.Where(
                         behaviour => string.Equals(
                             behaviour.GetType().FullName,
                             SourceOnlySectionAdapterTypeName,
                             StringComparison.Ordinal)))
            {
                Require(
                    sectionAdapter.GetComponent<ItemDetailSectionView>() != null,
                    "EXACT_ITEM_DETAIL_SOURCE_SECTION_ADAPTER_TARGET_MISSING "
                    + RelativePath(root.transform, sectionAdapter.transform));
            }

            return forbidden;
        }

        private static void RequireKnownSourceOnlyAdapterTypeSet(
            IReadOnlyList<string> forbiddenTypeNames)
        {
            IReadOnlyList<string> types = forbiddenTypeNames
                                          ?? Array.Empty<string>();
            foreach (string typeName in types)
            {
                string value = typeName ?? string.Empty;
                Require(
                    string.Equals(value,
                        SourceOnlyPanelAdapterTypeName,
                        StringComparison.Ordinal)
                    || string.Equals(value,
                        SourceOnlySectionAdapterTypeName,
                        StringComparison.Ordinal),
                    "UNIFIED_ITEM_DETAIL_FORBIDDEN_OWNER " + value);
            }

            int panelCount = types.Count(typeName => string.Equals(
                typeName,
                SourceOnlyPanelAdapterTypeName,
                StringComparison.Ordinal));
            Require(
                panelCount == 1,
                "EXACT_ITEM_DETAIL_SOURCE_PANEL_ADAPTER_COUNT_INVALID "
                + "expected=1 actual="
                + panelCount.ToString(CultureInfo.InvariantCulture));

            int sectionCount = types.Count(typeName => string.Equals(
                typeName,
                SourceOnlySectionAdapterTypeName,
                StringComparison.Ordinal));
            Require(
                sectionCount == ExpectedSourceOnlySectionAdapterCount,
                "EXACT_ITEM_DETAIL_SOURCE_SECTION_ADAPTER_COUNT_INVALID "
                + "expected="
                + ExpectedSourceOnlySectionAdapterCount.ToString(
                    CultureInfo.InvariantCulture)
                + " actual="
                + sectionCount.ToString(CultureInfo.InvariantCulture));
        }

        private static void ConfigureClosedCanvasGroup(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.ignoreParentGroups = false;
        }

        private static bool RevertExactNullReferenceOverride(
            Object target,
            string propertyPath)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyPath);
            Require(
                property != null
                && property.propertyType ==
                SerializedPropertyType.ObjectReference,
                "UNIFIED_ITEM_DETAIL_SCENE_PROPERTY_MISSING " + propertyPath);
            if (property.objectReferenceValue != null)
            {
                return false;
            }

            Object source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(
                target);
            Require(
                source != null,
                "UNIFIED_ITEM_DETAIL_SCENE_SOURCE_COMPONENT_MISSING "
                + propertyPath);
            SerializedProperty sourceProperty = new SerializedObject(source)
                .FindProperty(propertyPath);
            Require(
                sourceProperty != null
                && sourceProperty.objectReferenceValue != null,
                "UNIFIED_ITEM_DETAIL_BASE_REFERENCE_MISSING " + propertyPath);
            Require(
                property.prefabOverride,
                "UNIFIED_ITEM_DETAIL_REQUIRED_CARRIER_REMOVED_OR_SUPPRESSED "
                + propertyPath);

            PrefabUtility.RevertPropertyOverride(
                property,
                InteractionMode.AutomatedAction);
            serialized.Update();
            property = serialized.FindProperty(propertyPath);
            Require(
                property.objectReferenceValue != null
                && !property.prefabOverride,
                "UNIFIED_ITEM_DETAIL_NULL_OVERRIDE_REVERT_FAILED "
                + propertyPath);
            Debug.Log(
                "[ExactItemDetailPhaseB] reverted exact null Scene override "
                + target.GetType().Name + "." + propertyPath);
            return true;
        }

        private static bool HasPropertyOverride(
            Object target,
            string propertyPath)
        {
            SerializedProperty property = new SerializedObject(target)
                .FindProperty(propertyPath);
            return property != null && property.prefabOverride;
        }

        private static string BuildUnrelatedPropertyOverrideSignature(
            GameObject instanceRoot)
        {
            IEnumerable<PropertyModification> modifications =
                PrefabUtility.GetPropertyModifications(instanceRoot)
                ?? Array.Empty<PropertyModification>();
            return string.Join(
                "\n",
                modifications
                    .Where(modification => !IsOwnedReferenceModification(
                        modification))
                    .Select(modification =>
                        StableObjectIdentity(modification.target)
                        + "|" + (modification.propertyPath ?? string.Empty)
                        + "|" + (modification.value ?? string.Empty)
                        + "|" + StableObjectIdentity(
                            modification.objectReference))
                    .OrderBy(value => value, StringComparer.Ordinal));
        }

        private static bool IsOwnedReferenceModification(
            PropertyModification modification)
        {
            return modification != null
                   && ((modification.target is UnifiedBattlePageShell
                        && string.Equals(
                            modification.propertyPath,
                            "itemDetailPopupSlot",
                            StringComparison.Ordinal))
                       || (modification.target is UnifiedBattleFormalSceneHost
                           && string.Equals(
                               modification.propertyPath,
                               "itemDetailPresenter",
                               StringComparison.Ordinal)));
        }

        private static void ValidateRuntimeBoundary()
        {
            string source = File.ReadAllText(RuntimePresenterPath);
            string[] requiredTokens =
            {
                "FormalItemDetailChanged",
                "CurrentFormalItemDetail",
                "CreateViewModelClone()",
                "result.projection.artwork",
                "popupCanvasGroup.alpha = visible ? 1f : 0f",
                "popupCanvasGroup.interactable = visible",
                "popupCanvasGroup.blocksRaycasts = visible",
                "panelView.Bind(null)",
                "panelView.Close()"
            };
            foreach (string token in requiredTokens)
            {
                Require(
                    source.Contains(token),
                    "PHASE_B_RUNTIME_SEAM_MISSING " + token);
            }

            string[] forbiddenTokens =
            {
                "TalismanBag.BuildSandbox",
                "GameObject.Find",
                "FindObjectOfType",
                "FindObjectsOfType",
                "new GameObject(",
                "ItemSandbox",
                "SampleData",
                "Update()"
            };
            foreach (string token in forbiddenTokens)
            {
                Require(
                    !source.Contains(token),
                    "PHASE_B_RUNTIME_FORBIDDEN_TOKEN " + token);
            }
        }

        private static string BuildVisualSignatureFromPrefabAsset(string path)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                return BuildVisualSignature(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string BuildVisualSignature(GameObject root)
        {
            Require(root != null, "EXACT_ITEM_DETAIL_SIGNATURE_ROOT_MISSING");
            StringBuilder builder = new StringBuilder();
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform current in transforms)
            {
                string path = RelativePath(root.transform, current);
                builder.Append("GO|").Append(path)
                    .Append('|').Append(current.gameObject.activeSelf ? '1' : '0')
                    .Append('|').Append(current.gameObject.layer)
                    .Append('|').Append(
                        current == root.transform
                            ? 0
                            : current.GetSiblingIndex())
                    .AppendLine();

                Component[] components = current.GetComponents<Component>();
                for (int index = 0; index < components.Length; index++)
                {
                    Component component = components[index];
                    Require(
                        component != null,
                        "EXACT_ITEM_DETAIL_MISSING_SCRIPT " + path);
                    builder.Append("COMP|").Append(path)
                        .Append('|').Append(index)
                        .Append('|').Append(component.GetType().FullName)
                        .AppendLine();
                    AppendSerializedComponent(
                        builder,
                        root.transform,
                        path,
                        index,
                        component);
                }
            }

            return builder.ToString();
        }

        private static void AppendSerializedComponent(
            StringBuilder builder,
            Transform root,
            string componentPath,
            int componentIndex,
            Component component)
        {
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;
                if (ShouldSkipSerializedProperty(iterator.propertyPath))
                {
                    continue;
                }

                builder.Append("PROP|").Append(componentPath)
                    .Append('|').Append(componentIndex)
                    .Append('|').Append(iterator.propertyPath)
                    .Append('|').Append(SerializedValue(root, iterator))
                    .AppendLine();
            }
        }

        private static bool ShouldSkipSerializedProperty(string propertyPath)
        {
            return string.Equals(propertyPath, "m_GameObject", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_Father", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_Children", StringComparison.Ordinal)
                   || propertyPath.StartsWith("m_Children.Array", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_CorrespondingSourceObject", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_PrefabInstance", StringComparison.Ordinal)
                   || string.Equals(propertyPath, "m_PrefabAsset", StringComparison.Ordinal);
        }

        private static string SerializedValue(
            Transform root,
            SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.longValue.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "1" : "0";
                case SerializedPropertyType.Float:
                    return property.doubleValue.ToString(
                        "R",
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.String:
                    return property.stringValue ?? string.Empty;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString("R");
                case SerializedPropertyType.ObjectReference:
                    return StableReference(root, property.objectReferenceValue);
                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString("R");
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString("R");
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString("R");
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString("R");
                case SerializedPropertyType.ArraySize:
                    return property.intValue.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.Character:
                    return property.intValue.ToString(
                        CultureInfo.InvariantCulture);
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue == null
                        ? string.Empty
                        : property.animationCurveValue.length.ToString(
                            CultureInfo.InvariantCulture);
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString("R");
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString("R");
                case SerializedPropertyType.ExposedReference:
                    return StableReference(
                        root,
                        property.exposedReferenceValue);
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString();
                case SerializedPropertyType.ManagedReference:
                    return property.managedReferenceFullTypename ?? string.Empty;
                default:
                    return property.propertyType.ToString();
            }
        }

        private static string StableReference(Transform root, Object value)
        {
            if (value == null)
            {
                return "null";
            }

            Transform referencedTransform = value is GameObject gameObject
                ? gameObject.transform
                : value is Component component
                    ? component.transform
                    : null;
            if (referencedTransform != null
                && (referencedTransform == root
                    || referencedTransform.IsChildOf(root)))
            {
                string path = RelativePath(root, referencedTransform);
                if (value is GameObject)
                {
                    return "internal:" + path + ":GameObject";
                }

                Component[] sameType = referencedTransform.GetComponents(
                    value.GetType());
                int index = Array.IndexOf(sameType, value as Component);
                return "internal:" + path + ":"
                       + value.GetType().FullName + ":" + index;
            }

            return StableObjectIdentity(value);
        }

        private static string StableObjectIdentity(Object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    value,
                    out string guid,
                    out long localId))
            {
                return "asset:" + guid + ":"
                       + localId.ToString(CultureInfo.InvariantCulture);
            }

            return "object:" + value.GetType().FullName + ":" + value.name;
        }

        private static string RelativePath(Transform root, Transform target)
        {
            if (target == root)
            {
                return "$";
            }

            Stack<string> names = new Stack<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Push(
                    current.name + "["
                    + current.GetSiblingIndex().ToString(
                        CultureInfo.InvariantCulture)
                    + "]");
                current = current.parent;
            }

            Require(current == root, "EXACT_ITEM_DETAIL_REFERENCE_OUTSIDE_ROOT");
            return "$/" + string.Join("/", names);
        }

        private static T RequiredObjectReference<T>(
            Object owner,
            string propertyPath,
            string diagnostic)
            where T : Object
        {
            SerializedProperty property = new SerializedObject(owner)
                .FindProperty(propertyPath);
            T value = property == null
                ? null
                : property.objectReferenceValue as T;
            Require(value != null, diagnostic);
            return value;
        }

        private static T RequiredSingleComponent<T>(GameObject target)
            where T : Component
        {
            T[] matches = target.GetComponents<T>();
            Require(
                matches.Length == 1,
                "UNIFIED_ITEM_DETAIL_COMPONENT_COUNT_INVALID "
                + typeof(T).FullName + "=" + matches.Length);
            return matches[0];
        }

        private static T SingleInHierarchy<T>(GameObject root)
            where T : Component
        {
            T[] matches = root.GetComponentsInChildren<T>(true);
            Require(
                matches.Length == 1,
                "UNIFIED_ITEM_DETAIL_HIERARCHY_COUNT_INVALID "
                + typeof(T).FullName + "=" + matches.Length);
            return matches[0];
        }

        private static T SingleInScene<T>(Scene scene)
            where T : Component
        {
            List<T> matches = AllInScene<T>(scene);
            Require(
                matches.Count == 1,
                "UNIFIED_ITEM_DETAIL_SCENE_COUNT_INVALID "
                + typeof(T).FullName + "=" + matches.Count);
            return matches[0];
        }

        private static List<T> AllInScene<T>(Scene scene)
            where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToList();
        }

        private static string ResolveNearestPrefabAssetPath(GameObject target)
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                target);
            if (!string.IsNullOrEmpty(path))
            {
                return path.Replace('\\', '/');
            }

            Object source = PrefabUtility.GetCorrespondingObjectFromSource(target);
            return source == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(source).Replace('\\', '/');
        }

        private static void ValidateNoMissingScripts(GameObject root)
        {
            int missing = root.GetComponentsInChildren<Transform>(true)
                .Sum(transform =>
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transform.gameObject));
            Require(missing == 0, "UNIFIED_ITEM_DETAIL_MISSING_SCRIPT");
        }

        private static void RequireNoBuildSandboxOwner(GameObject root)
        {
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<
                MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().FullName ?? string.Empty;
                Require(
                    !IsForbiddenOwnerType(typeName),
                    "UNIFIED_ITEM_DETAIL_FORBIDDEN_OWNER " + typeName);
            }
        }

        private static bool IsForbiddenOwnerType(string typeName)
        {
            string value = typeName ?? string.Empty;
            return value.Contains("BuildSandbox")
                   || value.Contains("ItemSandbox")
                   || value.Contains("BuildGridInteractionPreview");
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }
    }
}
