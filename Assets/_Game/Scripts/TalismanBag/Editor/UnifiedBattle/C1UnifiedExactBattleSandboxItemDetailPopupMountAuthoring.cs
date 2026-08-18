using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.Presentation.ExactItemDetail;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class
        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
    {
        public const string TerminalMarker =
            "C1_UNIFIED_EXACT_ITEM_DETAIL_POPUP_MOUNT_AUTHORING_PASS";

        private const string MenuPath =
            "TalismanBag/V0.4/Unified Battle/"
            + "Mount Exact BattleSandbox Item Detail Popup Once";
        public const string ContractPath =
            "Docs/V0.4/"
            + "V0.4-C1UnifiedExactBattleSandboxItemDetailPopupMount01_"
            + "TaskContract.md";
        public const string ShellPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/"
            + "UnifiedBattlePageShell.prefab";
        public const string ExactStandalonePrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemDetailPopupStandalone.prefab";
        private const string ExactStandaloneMetaPath =
            ExactStandalonePrefabPath + ".meta";
        private const string ExactStandaloneParityReportPath =
            "Docs/V0.4/Reports/"
            + "C1ExactBattleSandboxItemDetailPopupStandalonePrefabParity.md";
        public const string LegacyPanelPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab";
        public const string UnifiedScenePath =
            "Assets/_Game/Scenes/"
            + "Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        public const string PresenterSourcePath =
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/"
            + "ExactItemDetail/"
            + "C1ExactBattleSandboxItemDetailPresenter.cs";

        public const string ExactStandaloneGuid =
            "4a168f3df70995a42a4f90167b42697a";
        public const string LegacyPanelGuid =
            "5f5c011600b50b249831e979eeb58313";
        public const string ExpectedIntakeShellHash =
            "69E34855B9146DA50F941CF19F389FEEB005ED35861D1011C30293D6BC147090";
        public const string ExpectedRepairablePartialShellHash =
            "67C156895A9C021248F96C1ABCB1132861A2BCAF26D82C6E0553A8DF8F7443EF";
        public const string ExpectedDrivenOverrideRepairableShellHash =
            "4D20AC105861EA53FEDFB469C866710C7F5C82A6521F5E2B57D43AD034BCDD8D";
        public const string ExpectedZeroRootMountRepairableShellHash =
            "8B903EBA8C5D3BF3AFD96F1587874B4421F4E3D26C05E05BDF065BC04ACCC543";
        public const string ExpectedEmbeddedRootKnownHandleResidueShellHash =
            "28AA10D8FEBC9A6185E59C6D40286EF7875F2FE76F162E8CA4F15CA5C56C20F9";
        internal const long ExactStandaloneRootRectSourceFileId =
            2277835597935611812L;
        internal const string RepairablePartialPanelSourceFileId =
            "1040749123798447714";
        internal const string RepairablePartialAddedObjectFileId =
            "186771055874766441";
        internal const long DrivenOverrideHandleSourceFileId =
            1131740001722038517L;
        private static readonly string[] DrivenOverrideRepairPropertyPaths =
        {
            "m_AnchorMax.x",
            "m_AnchorMax.y",
            "m_AnchorMin.y"
        };
        internal const float RectValueTolerance = 0.0001f;
        internal const float RectRotationToleranceDegrees = 0.001f;
        private const int OtherModificationDiffEntryLimit = 8;
        private const int KnownHandleResidueModificationCount = 27;
        private const int FinalEmbeddedModificationCount = 24;
        internal const string ModalBackdropName =
            "ItemDetailModalBackdrop";
        internal const float ModalBackdropAlpha = 0.62f;

        private static readonly HashSet<string>
            RootCanvasRectContextPropertyPaths = new HashSet<string>(
                StringComparer.Ordinal)
            {
                "m_AnchorMin.x",
                "m_AnchorMin.y",
                "m_AnchorMax.x",
                "m_AnchorMax.y",
                "m_AnchoredPosition.x",
                "m_AnchoredPosition.y",
                "m_SizeDelta.x",
                "m_SizeDelta.y",
                "m_Pivot.x",
                "m_Pivot.y",
                "m_LocalPosition.x",
                "m_LocalPosition.y",
                "m_LocalPosition.z",
                "m_LocalRotation.x",
                "m_LocalRotation.y",
                "m_LocalRotation.z",
                "m_LocalRotation.w",
                "m_LocalEulerAnglesHint.x",
                "m_LocalEulerAnglesHint.y",
                "m_LocalEulerAnglesHint.z",
                "m_LocalScale.x",
                "m_LocalScale.y",
                "m_LocalScale.z"
            };

        private static readonly IReadOnlyDictionary<string, float>
            EmbeddedRootPropertyValues = new Dictionary<string, float>(
                StringComparer.Ordinal)
            {
                ["m_AnchorMin.x"] = 0f,
                ["m_AnchorMin.y"] = 0f,
                ["m_AnchorMax.x"] = 1f,
                ["m_AnchorMax.y"] = 1f,
                ["m_AnchoredPosition.x"] = 0f,
                ["m_AnchoredPosition.y"] = 0f,
                ["m_SizeDelta.x"] = 0f,
                ["m_SizeDelta.y"] = 0f,
                ["m_Pivot.x"] = 0.5f,
                ["m_Pivot.y"] = 0.5f,
                ["m_LocalPosition.x"] = 0f,
                ["m_LocalPosition.y"] = 0f,
                ["m_LocalPosition.z"] = 0f,
                ["m_LocalRotation.x"] = 0f,
                ["m_LocalRotation.y"] = 0f,
                ["m_LocalRotation.z"] = 0f,
                ["m_LocalRotation.w"] = 1f,
                ["m_LocalEulerAnglesHint.x"] = 0f,
                ["m_LocalEulerAnglesHint.y"] = 0f,
                ["m_LocalEulerAnglesHint.z"] = 0f,
                ["m_LocalScale.x"] = 1f,
                ["m_LocalScale.y"] = 1f,
                ["m_LocalScale.z"] = 1f
            };

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [ContractPath] =
                    "F37EBECAF7D799DAA0F11F578CAFC90258A6DC7CB65A214718E4574DE5B1F409",
                [ExactStandalonePrefabPath] =
                    "F3164CE168A2167065EA3F695995E092C8BA137676B5AFB8F26A756B05E222A1",
                [ExactStandaloneMetaPath] =
                    "16EFADA28C497BEA9B68BC5FAA4384AE57EA8AC03FA19364537C16C7ACB7345B",
                [ExactStandaloneParityReportPath] =
                    "F8103ECE700326C9428F5780DB9D7ECE6451FD7A5B7FEC0DB1345E1130592132",
                [LegacyPanelPrefabPath] =
                    "2A6924DA0823B74E2639B593926D8C2F8422101B01EDBE4073AF33CD424E027C",
                [UnifiedScenePath] =
                    "D432B6E492EDF0CB0EEA8B62E66E27A6EE3F7773C8BBCC8AD886A86A1A0BDD63"
            };

        public enum ShellMountState
        {
            ExactIntake = 0,
            ExactRepairablePartial = 1,
            ExactDrivenOverridesRepairable = 2,
            FinalClean = 3,
            InvalidConflict = 4,
            ExactZeroRootMountRepairable = 5,
            EmbeddedRootKnownHandleResidueRepairable = 6,
            FinalCleanKnownScrollbarDriverBaseline = 7,
            ExactVisibleLifecycleRepairable = 8
        }

        internal readonly struct AuthoredRectGeometry
        {
            public AuthoredRectGeometry(
                Vector2 anchorMin,
                Vector2 anchorMax,
                Vector2 pivot,
                Vector2 anchoredPosition,
                Vector2 sizeDelta,
                Vector3 localScale,
                Quaternion localRotation)
            {
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                Pivot = pivot;
                AnchoredPosition = anchoredPosition;
                SizeDelta = sizeDelta;
                LocalScale = localScale;
                LocalRotation = localRotation;
            }

            public Vector2 AnchorMin { get; }
            public Vector2 AnchorMax { get; }
            public Vector2 Pivot { get; }
            public Vector2 AnchoredPosition { get; }
            public Vector2 SizeDelta { get; }
            public Vector3 LocalScale { get; }
            public Quaternion LocalRotation { get; }
        }

        [MenuItem(MenuPath)]
        public static void ApplyFromMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    "EXACT_POPUP_MOUNT_REQUIRES_NON_PLAY_EDITOR");
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                throw new InvalidOperationException(
                    "EXACT_POPUP_MOUNT_REQUIRES_IDLE_EDITOR");
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
                    "[ExactPopupMount] EXACT_POPUP_MOUNT_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static ShellMountState InspectCurrentShell(
            out string diagnostic)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                ShellPrefabPath);
            try
            {
                return ClassifyLoadedShell(root, out diagnostic);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static ShellMountState ClassifyLoadedShell(
            GameObject root,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (root == null)
            {
                diagnostic = "EXACT_POPUP_MOUNT_SHELL_ROOT_MISSING";
                return ShellMountState.InvalidConflict;
            }

            UnifiedBattlePageShell[] shells = root.GetComponentsInChildren<
                UnifiedBattlePageShell>(true);
            UnifiedBattleFormalSceneHost[] hosts =
                root.GetComponentsInChildren<UnifiedBattleFormalSceneHost>(true);
            C1ExactBattleSandboxItemDetailPresenter[] presenters =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailPresenter>(true);
            if (shells.Length != 1 || hosts.Length != 1
                || presenters.Length != 1)
            {
                diagnostic = "EXACT_POPUP_MOUNT_FOUNDATION_COUNT_INVALID "
                             + "shell=" + shells.Length
                             + " host=" + hosts.Length
                             + " presenter=" + presenters.Length;
                return ShellMountState.InvalidConflict;
            }

            UnifiedBattlePageShell shell = shells[0];
            UnifiedBattleFormalSceneHost host = hosts[0];
            C1ExactBattleSandboxItemDetailPresenter presenter = presenters[0];
            RectTransform frame = presenter.PresentationFrame;
            if (shell.ItemDetailPopupSlot != presenter.transform
                || host.ItemDetailPresenter != presenter
                || presenter.transform.parent != shell.transform
                || frame == null
                || frame.parent != presenter.transform
                || frame.childCount != 1)
            {
                diagnostic =
                    "EXACT_POPUP_MOUNT_FOUNDATION_REFERENCE_INVALID";
                return ShellMountState.InvalidConflict;
            }

            GameObject directCarrier = frame.GetChild(0).gameObject;
            GameObject nearestInstance =
                PrefabUtility.GetNearestPrefabInstanceRoot(directCarrier);
            string carrierPath = ResolveNearestPrefabAssetPath(directCarrier);
            ItemDetailPanelView[] panels = frame.GetComponentsInChildren<
                ItemDetailPanelView>(true);
            CanvasGroup canvasGroup = presenter.GetComponent<CanvasGroup>();
            if (nearestInstance != directCarrier
                || panels.Length != 1
                || canvasGroup == null
                || presenter.PanelView != panels[0]
                || presenter.CloseButton == null
                || presenter.PopupCanvasGroup != canvasGroup
                || !IsClosedCanvasGroup(canvasGroup))
            {
                diagnostic =
                    "EXACT_POPUP_MOUNT_CARRIER_OR_BINDING_PARTIAL";
                return ShellMountState.InvalidConflict;
            }

            int legacyRoots = CountNestedPrefabRoots(root, LegacyPanelPrefabPath);
            int exactRoots = CountNestedPrefabRoots(
                root,
                ExactStandalonePrefabPath);
            ItemDetailPanelView panel = panels[0];
            Button authoredClose = RequiredObjectReference<Button>(
                panel,
                "closeButton",
                "EXACT_POPUP_MOUNT_CLOSE_REFERENCE_MISSING");
            C1ExactBattleSandboxItemDetailCloseRelay[] relays =
                panel.GetComponents<C1ExactBattleSandboxItemDetailCloseRelay>();
            int panelMissingScripts =
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    panel.gameObject);
            int totalMissingScripts = CountMissingScripts(root);

            RectTransform carrierRoot = directCarrier.transform as RectTransform;
            bool exactMountedBinding = string.Equals(
                    carrierPath,
                    ExactStandalonePrefabPath,
                    StringComparison.Ordinal)
                && legacyRoots == 0
                && exactRoots == 1
                && carrierRoot != null
                && presenter.CarrierRoot == carrierRoot
                && panel.transform != carrierRoot
                && panel.transform.IsChildOf(carrierRoot)
                && presenter.CloseButton == authoredClose
                && relays.Length == 0;
            if (exactMountedBinding
                && panelMissingScripts == 0
                && totalMissingScripts == 0)
            {
                if (IsExpectedEmbeddedRootKnownHandleResidueHash(
                        ComputeSha256(ShellPrefabPath)))
                {
                    try
                    {
                        ValidateEmbeddedRootKnownHandleResidueSerializedShell();
                        bool missingFormalModal =
                            presenter.ModalBackdropImage == null
                            && presenter.ModalBackdropButton == null
                            && root.GetComponentsInChildren<Transform>(true)
                                .Count(value => value != null
                                                && string.Equals(
                                                    value.name,
                                                    ModalBackdropName,
                                                    StringComparison.Ordinal))
                            == 0;
                        if (missingFormalModal)
                        {
                            ValidateLegacyPresenterReferencesWithoutBackdrop(
                                presenter,
                                carrierRoot);
                            ValidateEmbeddedRootKnownHandleResidueLoadedShell(
                                root,
                                false);
                            diagnostic =
                                "EXACT_POPUP_MOUNT_VISIBLE_LIFECYCLE_"
                                + "REPAIRABLE";
                            return ShellMountState
                                .ExactVisibleLifecycleRepairable;
                        }

                        ValidateEmbeddedRootKnownHandleResidueLoadedShell(root);
                        diagnostic =
                            "EXACT_POPUP_MOUNT_FINAL_CLEAN_KNOWN_SCROLLBAR_"
                            + "DRIVER_BASELINE";
                        return ShellMountState
                            .FinalCleanKnownScrollbarDriverBaseline;
                    }
                    catch (InvalidOperationException exception)
                    {
                        diagnostic =
                            "EXACT_POPUP_MOUNT_KNOWN_HANDLE_INTAKE_INVALID "
                            + exception.Message;
                        return ShellMountState.InvalidConflict;
                    }
                }

                try
                {
                    string yaml = NormalizeYaml(
                        File.ReadAllText(ShellPrefabPath));
                    ValidateEmbeddedRootKnownHandleResidueRowsInYaml(yaml);
                    ValidateEmbeddedRootKnownHandleResidueLoadedShell(root);
                    diagnostic =
                        "EXACT_POPUP_MOUNT_FINAL_CLEAN_KNOWN_SCROLLBAR_"
                        + "DRIVER_BASELINE";
                    return ShellMountState
                        .FinalCleanKnownScrollbarDriverBaseline;
                }
                catch (InvalidOperationException)
                {
                    // A zero-handle final or the exact historical zero-root
                    // intake is classified by the strict branches below.
                }

                if (presenter.ValidateAuthoredReferences()
                    && IsEmbeddedCarrierRootGeometryValid(carrierRoot))
                {
                    diagnostic = "EXACT_POPUP_MOUNT_FINAL_CLEAN";
                    return ShellMountState.FinalClean;
                }

                try
                {
                    ValidateExactZeroRootMountRepairableSerializedShell();
                    ValidateExactCarrierPreflight(
                        root,
                        presenter,
                        carrierRoot);
                    diagnostic =
                        "EXACT_POPUP_MOUNT_ZERO_ROOT_REPAIRABLE_HASH_BOUND_"
                        + "SERIALIZED_INTAKE";
                    return ShellMountState.ExactZeroRootMountRepairable;
                }
                catch (InvalidOperationException exception)
                {
                    diagnostic =
                        "EXACT_POPUP_MOUNT_ZERO_ROOT_INTAKE_INVALID "
                        + exception.Message;
                    return ShellMountState.InvalidConflict;
                }
            }

            diagnostic = "EXACT_POPUP_MOUNT_INVALID_CONFLICT "
                         + "carrier=" + carrierPath
                         + " legacyRoots=" + legacyRoots
                         + " exactRoots=" + exactRoots
                         + " relay=" + relays.Length
                         + " panelMissing=" + panelMissingScripts
                         + " totalMissing=" + totalMissingScripts
                         + " carrierRef="
                         + (presenter.CarrierRoot == null ? "null" : "set");
            return ShellMountState.InvalidConflict;
        }

        public static void ValidateFinalLoadedShell(GameObject root)
        {
            ShellMountState state = ClassifyLoadedShell(
                root,
                out string diagnostic);
            bool knownScrollbarDriverBaseline = state == ShellMountState
                .FinalCleanKnownScrollbarDriverBaseline;
            Require(
                state == ShellMountState.FinalClean
                || knownScrollbarDriverBaseline,
                "EXACT_POPUP_MOUNT_FINAL_CLASSIFICATION_FAILED " + diagnostic);

            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            ItemDetailPanelView panel = presenter.PanelView;
            Require(
                string.Equals(
                    ResolveNearestPrefabAssetPath(carrier.gameObject),
                    ExactStandalonePrefabPath,
                    StringComparison.Ordinal),
                "EXACT_POPUP_MOUNT_SOURCE_LINK_INVALID");

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                ExactStandalonePrefabPath);
            Require(source != null, "EXACT_POPUP_MOUNT_SOURCE_ASSET_MISSING");
            if (knownScrollbarDriverBaseline)
            {
                ValidateEmbeddedRootKnownHandleResidueRowsInYaml(
                    NormalizeYaml(File.ReadAllText(ShellPrefabPath)));
                ValidateEmbeddedRootKnownHandleResidueLoadedShell(root);
                return;
            }

            ValidateAuthoredHierarchyAndGeometry(
                source.transform,
                carrier);
            ValidateNoDisallowedPropertyOverrides(carrier, source);
            Require(
                IsEmbeddedCarrierRootGeometryValid(carrier),
                "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_GEOMETRY_INVALID");
            ValidateEmbeddedRootPropertyOverrideContract(carrier, source);
            ValidatePresentation(carrier, panel, presenter.CloseButton);
            ValidateModalBackdropContract(root, presenter, true);
            ValidateNoMissingScripts(root);
            ValidateNoForbiddenOwners(root);
            Require(
                root.GetComponentsInChildren<EventSystem>(true).Length == 0,
                "EXACT_POPUP_MOUNT_DUPLICATE_EVENT_SYSTEM_OWNER");
        }

        private static void ApplySinglePass()
        {
            ValidateProtectedInputs();
            C1UnifiedExactBattleSandboxItemDetailPopupMountTests
                .RunBeforeAuthoringOrThrow();

            string shellHashBefore = ComputeSha256(ShellPrefabPath);
            bool changed = false;
            GameObject root = PrefabUtility.LoadPrefabContents(
                ShellPrefabPath);
            try
            {
                ShellMountState state = ClassifyLoadedShell(
                    root,
                    out string diagnostic);
                Require(
                    state != ShellMountState.InvalidConflict,
                    diagnostic);
                if (state == ShellMountState.ExactZeroRootMountRepairable)
                {
                    Require(
                        string.Equals(
                            shellHashBefore,
                            ExpectedZeroRootMountRepairableShellHash,
                            StringComparison.OrdinalIgnoreCase),
                        "EXACT_POPUP_MOUNT_ZERO_ROOT_SHELL_HASH_DRIFT "
                        + shellHashBefore);
                    ApplyEmbeddedCarrierRootMountAdapter(root);
                    changed = true;
                }
                else if (state
                         == ShellMountState.ExactVisibleLifecycleRepairable)
                {
                    Require(
                        string.Equals(
                            shellHashBefore,
                            ExpectedEmbeddedRootKnownHandleResidueShellHash,
                            StringComparison.OrdinalIgnoreCase),
                        "EXACT_POPUP_MOUNT_VISIBLE_LIFECYCLE_SHELL_HASH_"
                        + "DRIFT " + shellHashBefore);
                    ApplyVisibleDataArtworkModalLifecycle(root);
                    changed = true;
                }
                else
                {
                    bool knownScrollbarDriverBaseline = state
                        == ShellMountState
                            .FinalCleanKnownScrollbarDriverBaseline;
                    Require(
                        state == ShellMountState.FinalClean
                        || knownScrollbarDriverBaseline,
                        "EXACT_POPUP_MOUNT_UNSUPPORTED_STATE " + state);
                    if (knownScrollbarDriverBaseline)
                    {
                        Require(
                            string.Equals(
                                shellHashBefore,
                                ExpectedEmbeddedRootKnownHandleResidueShellHash,
                                StringComparison.OrdinalIgnoreCase),
                            "EXACT_POPUP_MOUNT_KNOWN_SCROLLBAR_FINAL_HASH_"
                            + "DRIFT " + shellHashBefore);
                    }
                    ValidateFinalLoadedShell(root);
                }

                if (changed)
                {
                    ValidateFinalLoadedShell(root);

                    GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                        root,
                        ShellPrefabPath);
                    Require(
                        saved != null,
                        "EXACT_POPUP_MOUNT_SHELL_SAVE_FAILED");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            if (changed)
            {
                AssetDatabase.ImportAsset(
                    ShellPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport
                    | ImportAssetOptions.ForceUpdate);
            }

            GameObject validationRoot = PrefabUtility.LoadPrefabContents(
                ShellPrefabPath);
            try
            {
                ValidateFinalLoadedShell(validationRoot);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(validationRoot);
            }

            ValidateSerializedShell();
            ValidateProtectedInputs();
            C1UnifiedExactBattleSandboxItemDetailPopupMountTests
                .RunAfterAuthoringOrThrow();
            Debug.Log(
                "[ExactPopupMount] " + TerminalMarker
                + " changed=" + (changed ? "1" : "0")
                + " shellBefore=" + shellHashBefore
                + " shellAfter=" + ComputeSha256(ShellPrefabPath)
                + " exactCarrier=1 oldCarrier=0 panel=1 presenter=1"
                + " modalBackdrop=1 formalCloseOwner=1"
                + " relay=0 closedRaycast=1 missingScripts=0"
                + " forbiddenOwners=0 addedResidue=0"
                + " embeddedRootMount=1");
        }

        internal static void ApplyEmbeddedCarrierRootMountAdapter(
            GameObject root)
        {
            ShellMountState state = ClassifyLoadedShell(
                root,
                out string diagnostic);
            Require(
                state == ShellMountState.ExactZeroRootMountRepairable,
                "EXACT_POPUP_MOUNT_ZERO_ROOT_REPAIR_STATE_INVALID "
                + diagnostic);

            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            ValidateExactZeroRootMountRepairableSerializedShell();
            ValidateExactCarrierPreflight(root, presenter, carrier);

            WriteAndRecordEmbeddedCarrierRoot(carrier);

            Require(
                IsEmbeddedCarrierRootGeometryValid(carrier)
                && presenter.ValidateAuthoredReferences(),
                "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_APPLY_FAILED");
            ValidateExactCarrierPreflight(root, presenter, carrier);
        }

        internal static void ApplyVisibleDataArtworkModalLifecycle(
            GameObject root)
        {
            ShellMountState state = ClassifyLoadedShell(
                root,
                out string diagnostic);
            Require(
                state == ShellMountState.ExactVisibleLifecycleRepairable,
                "EXACT_POPUP_MOUNT_VISIBLE_LIFECYCLE_REPAIR_STATE_INVALID "
                + diagnostic);

            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            ValidateEmbeddedRootKnownHandleResidueSerializedShell();
            ValidateLegacyPresenterReferencesWithoutBackdrop(
                presenter,
                carrier);
            ResolveExactDrivenOverrideHandle(
                carrier,
                out RectTransform sourceHandle,
                out _);
            RequireExactSourceComponentIdentity(
                sourceHandle,
                DrivenOverrideHandleSourceFileId,
                "EXACT_POPUP_MOUNT_VISIBLE_LIFECYCLE_HANDLE_SOURCE_INVALID");
            ValidateExactCarrierPreflight(
                root,
                presenter,
                carrier,
                sourceHandle);

            GameObject backdropObject = new GameObject(
                ModalBackdropName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            RectTransform backdrop =
                backdropObject.GetComponent<RectTransform>();
            backdrop.SetParent(presenter.transform, false);
            backdrop.anchorMin = Vector2.zero;
            backdrop.anchorMax = Vector2.one;
            backdrop.anchoredPosition = Vector2.zero;
            backdrop.sizeDelta = Vector2.zero;
            backdrop.pivot = new Vector2(0.5f, 0.5f);
            backdrop.localPosition = Vector3.zero;
            backdrop.localRotation = Quaternion.identity;
            backdrop.localScale = Vector3.one;
            backdrop.SetSiblingIndex(
                presenter.PresentationFrame.GetSiblingIndex());

            Image image = backdropObject.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, ModalBackdropAlpha);
            image.raycastTarget = true;
            image.enabled = false;
            Button button = backdropObject.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            UnityEngine.UI.Navigation navigation = button.navigation;
            navigation.mode = UnityEngine.UI.Navigation.Mode.None;
            button.navigation = navigation;
            button.interactable = false;
            button.onClick.RemoveAllListeners();

            presenter.AssignForEditor(
                presenter.PanelView,
                presenter.CloseButton,
                presenter.PopupCanvasGroup,
                presenter.PresentationFrame,
                carrier,
                image,
                button);
            EditorUtility.SetDirty(backdropObject);
            EditorUtility.SetDirty(presenter);

            ValidateModalBackdropContract(root, presenter, true);
            Require(
                ClassifyLoadedShell(root, out string finalDiagnostic)
                == ShellMountState.FinalCleanKnownScrollbarDriverBaseline,
                "EXACT_POPUP_MOUNT_VISIBLE_LIFECYCLE_IN_MEMORY_FINAL_FAILED "
                + finalDiagnostic);
            ValidateFinalLoadedShell(root);
        }

        private static void ValidateLegacyPresenterReferencesWithoutBackdrop(
            C1ExactBattleSandboxItemDetailPresenter presenter,
            RectTransform carrier)
        {
            Require(
                presenter != null
                && presenter.PanelView != null
                && presenter.CloseButton != null
                && presenter.PopupCanvasGroup != null
                && presenter.PresentationFrame != null
                && carrier != null
                && presenter.ModalBackdropImage == null
                && presenter.ModalBackdropButton == null
                && presenter.PopupCanvasGroup.transform
                == presenter.transform
                && presenter.PresentationFrame.parent
                == presenter.transform
                && presenter.PresentationFrame.childCount == 1
                && presenter.PresentationFrame.GetChild(0) == carrier
                && carrier.parent == presenter.PresentationFrame
                && IsEmbeddedCarrierRootGeometryValid(carrier)
                && presenter.PanelView.transform.IsChildOf(carrier)
                && presenter.CloseButton.transform.IsChildOf(
                    presenter.PanelView.transform),
                "EXACT_POPUP_MOUNT_VISIBLE_LIFECYCLE_INTAKE_BINDING_"
                + "INVALID");
        }

        private static void WriteAndRecordEmbeddedCarrierRoot(
            RectTransform carrier)
        {
            Require(
                carrier != null,
                "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_WRITE_TARGET_MISSING");
            carrier.anchorMin = Vector2.zero;
            carrier.anchorMax = Vector2.one;
            carrier.anchoredPosition = Vector2.zero;
            carrier.sizeDelta = Vector2.zero;
            carrier.pivot = new Vector2(0.5f, 0.5f);
            carrier.localPosition = Vector3.zero;
            carrier.localRotation = Quaternion.identity;
            carrier.localEulerAngles = Vector3.zero;
            carrier.localScale = Vector3.one;
            PrefabUtility.RecordPrefabInstancePropertyModifications(carrier);
            EditorUtility.SetDirty(carrier);
        }

        private static void ValidateExactCarrierPreflight(
            GameObject shellRoot,
            C1ExactBattleSandboxItemDetailPresenter presenter,
            RectTransform carrier,
            RectTransform allowedDrivenOverrideSourceRect = null)
        {
            Require(
                shellRoot != null
                && presenter != null
                && carrier != null
                && presenter.PresentationFrame != null
                && carrier.parent == presenter.PresentationFrame
                && presenter.PresentationFrame.childCount == 1
                && presenter.PresentationFrame.GetChild(0) == carrier,
                "EXACT_POPUP_MOUNT_CARRIER_HIERARCHY_INVALID");
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                ExactStandalonePrefabPath);
            Require(source != null, "EXACT_POPUP_MOUNT_SOURCE_ASSET_MISSING");
            ValidateAuthoredHierarchyAndGeometry(
                source.transform,
                carrier,
                allowedDrivenOverrideSourceRect);
            ValidateNoDisallowedPropertyOverrides(
                carrier,
                source,
                allowedDrivenOverrideSourceRect);
            ValidatePresentation(
                carrier,
                presenter.PanelView,
                presenter.CloseButton);
            ValidateNoMissingScripts(shellRoot);
            ValidateNoForbiddenOwners(shellRoot);
            Require(
                shellRoot.GetComponentsInChildren<EventSystem>(true).Length
                == 0,
                "EXACT_POPUP_MOUNT_DUPLICATE_EVENT_SYSTEM_OWNER");
        }

        private static void ValidateModalBackdropContract(
            GameObject shellRoot,
            C1ExactBattleSandboxItemDetailPresenter presenter,
            bool requireClosed)
        {
            Transform[] named = shellRoot.GetComponentsInChildren<Transform>(
                    true)
                .Where(value => value != null
                                && string.Equals(
                                    value.name,
                                    ModalBackdropName,
                                    StringComparison.Ordinal))
                .ToArray();
            Require(
                named.Length == 1,
                "EXACT_POPUP_MOUNT_MODAL_BACKDROP_COUNT_INVALID "
                + named.Length.ToString(CultureInfo.InvariantCulture));
            RectTransform rect = named[0] as RectTransform;
            Image image = named[0].GetComponent<Image>();
            Button button = named[0].GetComponent<Button>();
            Component[] components = named[0].GetComponents<Component>();
            Require(
                rect != null
                && image != null
                && button != null
                && components.Length == 4
                && components[0] is RectTransform
                && components[1] is CanvasRenderer
                && components[2] is Image
                && components[3] is Button
                && named[0].parent == presenter.transform
                && presenter.ModalBackdropImage == image
                && presenter.ModalBackdropButton == button
                && button.targetGraphic == image
                && button.transition == Selectable.Transition.None
                && button.navigation.mode
                == UnityEngine.UI.Navigation.Mode.None
                && button.onClick.GetPersistentEventCount() == 0
                && image.raycastTarget
                && Mathf.Abs(image.color.r) <= RectValueTolerance
                && Mathf.Abs(image.color.g) <= RectValueTolerance
                && Mathf.Abs(image.color.b) <= RectValueTolerance
                && Mathf.Abs(image.color.a - ModalBackdropAlpha)
                <= RectValueTolerance
                && Approximately(rect.anchorMin, Vector2.zero)
                && Approximately(rect.anchorMax, Vector2.one)
                && Approximately(rect.anchoredPosition, Vector2.zero)
                && Approximately(rect.sizeDelta, Vector2.zero)
                && Approximately(rect.pivot, new Vector2(0.5f, 0.5f))
                && Approximately(rect.localPosition, Vector3.zero)
                && Approximately(rect.localScale, Vector3.one)
                && Quaternion.Angle(rect.localRotation, Quaternion.identity)
                <= RectRotationToleranceDegrees
                && named[0].GetSiblingIndex()
                < presenter.PresentationFrame.GetSiblingIndex()
                && (!requireClosed
                    || (!image.enabled
                        && !button.interactable
                        && IsClosedCanvasGroup(
                            presenter.PopupCanvasGroup))),
                "EXACT_POPUP_MOUNT_MODAL_BACKDROP_CONTRACT_INVALID");
        }

        internal static bool IsEmbeddedCarrierRootGeometryValid(
            RectTransform carrier)
        {
            return carrier != null
                   && Approximately(carrier.anchorMin, Vector2.zero)
                   && Approximately(carrier.anchorMax, Vector2.one)
                   && Approximately(carrier.anchoredPosition, Vector2.zero)
                   && Approximately(carrier.sizeDelta, Vector2.zero)
                   && Approximately(
                       carrier.pivot,
                       new Vector2(0.5f, 0.5f))
                   && Approximately(carrier.localPosition, Vector3.zero)
                   && Approximately(carrier.localScale, Vector3.one)
                   && Quaternion.Angle(
                       carrier.localRotation,
                       Quaternion.identity) <= RectRotationToleranceDegrees
                   && Approximately(carrier.localEulerAngles, Vector3.zero);
        }

        internal static bool IsExactZeroRootMountGeometry(
            RectTransform carrier)
        {
            return carrier != null
                   && Approximately(carrier.anchorMin, Vector2.zero)
                   && Approximately(carrier.anchorMax, Vector2.zero)
                   && Approximately(carrier.anchoredPosition, Vector2.zero)
                   && Approximately(carrier.sizeDelta, Vector2.zero)
                   && Approximately(carrier.pivot, Vector2.zero)
                   && Approximately(carrier.localPosition, Vector3.zero)
                   && Approximately(carrier.localScale, Vector3.zero)
                   && Quaternion.Angle(
                       carrier.localRotation,
                       Quaternion.identity) <= RectRotationToleranceDegrees
                   && Approximately(carrier.localEulerAngles, Vector3.zero);
        }

        private static void ReplaceLegacyCarrier(GameObject root)
        {
            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform frame = presenter.PresentationFrame;
            Require(
                frame != null && frame.childCount == 1,
                "EXACT_POPUP_MOUNT_INTAKE_FRAME_INVALID");
            GameObject legacyRoot = frame.GetChild(0).gameObject;
            Require(
                string.Equals(
                    ResolveNearestPrefabAssetPath(legacyRoot),
                    LegacyPanelPrefabPath,
                    StringComparison.Ordinal)
                && PrefabUtility.GetNearestPrefabInstanceRoot(legacyRoot)
                == legacyRoot,
                "EXACT_POPUP_MOUNT_LEGACY_ROOT_IDENTITY_INVALID");
            Object.DestroyImmediate(legacyRoot);
            Require(
                frame.childCount == 0,
                "EXACT_POPUP_MOUNT_LEGACY_ROOT_RETIRE_FAILED");

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                ExactStandalonePrefabPath);
            Require(source != null, "EXACT_POPUP_MOUNT_SOURCE_ASSET_MISSING");
            GameObject carrier = PrefabUtility.InstantiatePrefab(
                    source,
                    frame) as GameObject;
            Require(
                carrier != null,
                "EXACT_POPUP_MOUNT_NESTED_INSTANCE_CREATE_FAILED");
            Require(
                PrefabUtility.GetNearestPrefabInstanceRoot(carrier) == carrier
                && string.Equals(
                    ResolveNearestPrefabAssetPath(carrier),
                    ExactStandalonePrefabPath,
                    StringComparison.Ordinal),
                "EXACT_POPUP_MOUNT_NESTED_SOURCE_LINK_FAILED");

            RectTransform carrierRect = carrier.transform as RectTransform;
            Require(
                carrierRect != null,
                "EXACT_POPUP_MOUNT_CARRIER_RECT_MISSING");
            ItemDetailPanelView panel = Single<ItemDetailPanelView>(carrier);
            Button closeButton = RequiredObjectReference<Button>(
                panel,
                "closeButton",
                "EXACT_POPUP_MOUNT_CLOSE_REFERENCE_MISSING");
            Require(
                panel.GetComponents<
                    C1ExactBattleSandboxItemDetailCloseRelay>().Length == 0,
                "EXACT_POPUP_MOUNT_SOURCE_ALREADY_OWNS_CLOSE_RELAY");

            CanvasGroup canvasGroup =
                RequiredSingleComponent<CanvasGroup>(presenter.gameObject);
            ConfigureClosedCanvasGroup(canvasGroup);
            presenter.AssignForEditor(
                panel,
                closeButton,
                canvasGroup,
                frame,
                carrierRect);
            EditorUtility.SetDirty(presenter);
        }

        internal static void RepairExactPartialShell(GameObject root)
        {
            ValidateRepairablePartialSerializedShell();
            ShellMountState state = ClassifyLoadedShell(
                root,
                out string diagnostic);
            Require(
                state == ShellMountState.ExactRepairablePartial,
                "EXACT_POPUP_MOUNT_REPAIR_STATE_INVALID " + diagnostic);

            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            ItemDetailPanelView panel = presenter.PanelView;
            Require(
                carrier != null
                && panel != null
                && panel.gameObject.name == "ItemDetailPanel"
                && panel.transform.IsChildOf(carrier)
                && string.Equals(
                    ResolveNearestPrefabAssetPath(panel.gameObject),
                    ExactStandalonePrefabPath,
                    StringComparison.Ordinal)
                && PrefabUtility.GetNearestPrefabInstanceRoot(
                    panel.gameObject) == carrier.gameObject,
                "EXACT_POPUP_MOUNT_REPAIR_PANEL_SOURCE_INVALID");

            int panelMissingBefore =
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    panel.gameObject);
            int totalMissingBefore = CountMissingScripts(root);
            Require(
                panelMissingBefore == 1 && totalMissingBefore == 1,
                "EXACT_POPUP_MOUNT_REPAIR_MISSING_SCRIPT_SCOPE_INVALID"
                + " panel=" + panelMissingBefore
                + " total=" + totalMissingBefore);

            int removed =
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(
                    panel.gameObject);
            Require(
                removed == 1,
                "EXACT_POPUP_MOUNT_REPAIR_REMOVED_COUNT_INVALID "
                + removed.ToString(CultureInfo.InvariantCulture));
            EditorUtility.SetDirty(panel.gameObject);
            Require(
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                    panel.gameObject) == 0
                && CountMissingScripts(root) == 0,
                "EXACT_POPUP_MOUNT_REPAIR_MISSING_SCRIPT_REMAINS");
            Require(
                panel.GetComponents<
                    C1ExactBattleSandboxItemDetailCloseRelay>().Length == 0,
                "EXACT_POPUP_MOUNT_REPAIR_RELAY_REMAINS");
            ValidateFinalLoadedShell(root);
        }

        internal static void RepairExactDrivenOverrides(GameObject root)
        {
            ValidateDrivenOverrideRepairableSerializedShell();
            ValidateDrivenOverrideRepairableLoadedShell(root);
            RevertExactDrivenOverrideProperties(root);
        }

        internal static void RepairEmbeddedRootKnownHandleResidue(
            GameObject root)
        {
            ValidateEmbeddedRootKnownHandleResidueSerializedShell();
            ShellMountState state = ClassifyLoadedShell(
                root,
                out string diagnostic);
            Require(
                state == ShellMountState
                    .EmbeddedRootKnownHandleResidueRepairable,
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_REPAIR_STATE_INVALID "
                + diagnostic);
            ValidateEmbeddedRootKnownHandleResidueLoadedShell(root);
            RewriteExactKnownHandlePropertyModifications(root);
            Require(
                ClassifyLoadedShell(root, out string finalDiagnostic)
                == ShellMountState.FinalClean,
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_IN_MEMORY_FINAL_FAILED "
                + finalDiagnostic);
            ValidateFinalLoadedShell(root);
        }

        private static void RewriteExactKnownHandlePropertyModifications(
            GameObject root)
        {
            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            Require(
                IsEmbeddedCarrierRootGeometryValid(carrier),
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_ROOT_INVALID");
            ResolveExactDrivenOverrideHandle(
                carrier,
                out RectTransform sourceHandle,
                out _);
            RectTransform sourceRoot =
                PrefabUtility.GetCorrespondingObjectFromSource(carrier);
            RequireExactSourceComponentIdentity(
                sourceRoot,
                ExactStandaloneRootRectSourceFileId,
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_ROOT_SOURCE_INVALID");

            PropertyModification[] originalModifications =
                PrefabUtility.GetPropertyModifications(carrier.gameObject)
                ?? Array.Empty<PropertyModification>();
            Require(
                originalModifications.Length
                == KnownHandleResidueModificationCount,
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_INTAKE_COUNT_INVALID "
                + originalModifications.Length.ToString(
                    CultureInfo.InvariantCulture));
            ValidateEmbeddedRootModificationSnapshot(
                originalModifications,
                sourceRoot);
            ValidateExactHandleModificationSnapshot(
                originalModifications,
                sourceHandle,
                true);

            List<PropertyModification> filteredModifications =
                new List<PropertyModification>(
                    FinalEmbeddedModificationCount);
            int removed = 0;
            foreach (PropertyModification modification in originalModifications)
            {
                if (IsExactKnownHandleModification(
                        modification,
                        sourceHandle))
                {
                    removed++;
                    continue;
                }

                filteredModifications.Add(modification);
            }

            Require(
                removed == DrivenOverrideRepairPropertyPaths.Length
                && filteredModifications.Count
                == originalModifications.Length - removed
                && filteredModifications.Count
                == FinalEmbeddedModificationCount,
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_REMOVED_COUNT_INVALID"
                + " removed=" + removed.ToString(CultureInfo.InvariantCulture)
                + " filtered="
                + filteredModifications.Count.ToString(
                    CultureInfo.InvariantCulture));
            string[] beforeOther = filteredModifications
                .Select(PropertyModificationSignature)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

            PrefabUtility.SetPropertyModifications(
                carrier.gameObject,
                filteredModifications.ToArray());

            PropertyModification[] actualModifications =
                PrefabUtility.GetPropertyModifications(carrier.gameObject)
                ?? Array.Empty<PropertyModification>();
            Require(
                actualModifications.Length == FinalEmbeddedModificationCount,
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_OUTPUT_COUNT_INVALID "
                + actualModifications.Length.ToString(
                    CultureInfo.InvariantCulture));
            ValidateEmbeddedRootModificationSnapshot(
                actualModifications,
                sourceRoot);
            ValidateExactHandleModificationSnapshot(
                actualModifications,
                sourceHandle,
                false);
            string[] afterOther = actualModifications
                .Select(PropertyModificationSignature)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Require(
                beforeOther.SequenceEqual(afterOther),
                "EXACT_NESTED_PREFAB_OVERRIDE_BLOCKER"
                + " beforeCount="
                + beforeOther.Length.ToString(CultureInfo.InvariantCulture)
                + " afterCount="
                + afterOther.Length.ToString(CultureInfo.InvariantCulture)
                + " beforeOnly="
                + FormatBoundedSignatures(
                    StableMultisetDifference(beforeOther, afterOther))
                + " afterOnly="
                + FormatBoundedSignatures(
                    StableMultisetDifference(afterOther, beforeOther)));
            Require(
                IsEmbeddedCarrierRootGeometryValid(carrier),
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_ROOT_COLLAPSED");
            Require(
                CountLoadedExactDrivenOverrides(carrier) == 0,
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_HANDLE_REMAINS");
        }

        private static void ValidateEmbeddedRootModificationSnapshot(
            IReadOnlyList<PropertyModification> modifications,
            RectTransform sourceRoot)
        {
            PropertyModification[] rootModifications = modifications
                .Where(modification =>
                    modification != null
                    && modification.target == sourceRoot)
                .ToArray();
            Require(
                rootModifications.Length == EmbeddedRootPropertyValues.Count,
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_ROOT_ROW_COUNT_INVALID "
                + rootModifications.Length.ToString(
                    CultureInfo.InvariantCulture));
            foreach (KeyValuePair<string, float> expected in
                     EmbeddedRootPropertyValues)
            {
                PropertyModification[] matches = rootModifications
                    .Where(modification => string.Equals(
                        modification.propertyPath,
                        expected.Key,
                        StringComparison.Ordinal))
                    .ToArray();
                Require(
                    matches.Length == 1
                    && matches[0].objectReference == null
                    && float.TryParse(
                        matches[0].value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float actualValue)
                    && actualValue.Equals(expected.Value),
                    "EXACT_POPUP_MOUNT_FILTERED_REWRITE_ROOT_ROW_INVALID "
                    + expected.Key);
            }
        }

        private static void ValidateExactHandleModificationSnapshot(
            IReadOnlyList<PropertyModification> modifications,
            RectTransform sourceHandle,
            bool requireRows)
        {
            PropertyModification[] handleModifications = modifications
                .Where(modification =>
                    modification != null
                    && modification.target == sourceHandle)
                .ToArray();
            int expectedCount = requireRows
                ? DrivenOverrideRepairPropertyPaths.Length
                : 0;
            Require(
                handleModifications.Length == expectedCount,
                "EXACT_POPUP_MOUNT_FILTERED_REWRITE_HANDLE_ROW_COUNT_INVALID "
                + handleModifications.Length.ToString(
                    CultureInfo.InvariantCulture));
            if (!requireRows)
            {
                return;
            }

            foreach (string propertyPath in DrivenOverrideRepairPropertyPaths)
            {
                PropertyModification[] matches = handleModifications
                    .Where(modification => string.Equals(
                        modification.propertyPath,
                        propertyPath,
                        StringComparison.Ordinal))
                    .ToArray();
                Require(
                    matches.Length == 1
                    && string.Equals(
                        matches[0].value,
                        "0",
                        StringComparison.Ordinal)
                    && matches[0].objectReference == null,
                    "EXACT_POPUP_MOUNT_FILTERED_REWRITE_HANDLE_ROW_INVALID "
                    + propertyPath);
            }
        }

        private static bool IsExactKnownHandleModification(
            PropertyModification modification,
            RectTransform sourceHandle)
        {
            return modification != null
                   && modification.target == sourceHandle
                   && DrivenOverrideRepairPropertyPaths.Contains(
                       modification.propertyPath,
                       StringComparer.Ordinal);
        }

        private static void RequireExactSourceComponentIdentity(
            Object sourceComponent,
            long expectedLocalFileId,
            string diagnostic)
        {
            Require(
                sourceComponent != null
                && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    sourceComponent,
                    out string guid,
                    out long localFileId)
                && string.Equals(
                    guid,
                    ExactStandaloneGuid,
                    StringComparison.Ordinal)
                && localFileId == expectedLocalFileId,
                diagnostic);
        }

        private static void RevertExactDrivenOverrideProperties(
            GameObject root)
        {
            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            Require(
                IsEmbeddedCarrierRootGeometryValid(carrier),
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_EMBEDDED_ROOT_INVALID");
            RectTransform mountedHandle = ResolveExactDrivenOverrideHandle(
                carrier,
                out RectTransform sourceHandle,
                out _);
            SerializedObject mountedSerialized =
                new SerializedObject(mountedHandle);
            SerializedObject sourceSerialized = new SerializedObject(
                sourceHandle);
            string[] otherModificationsBefore =
                CaptureOtherPropertyModificationSignatures(
                    carrier,
                    mountedHandle,
                    sourceHandle);

            int reverted = 0;
            foreach (string propertyPath in DrivenOverrideRepairPropertyPaths)
            {
                mountedSerialized.UpdateIfRequiredOrScript();
                sourceSerialized.UpdateIfRequiredOrScript();
                SerializedProperty mountedProperty =
                    mountedSerialized.FindProperty(propertyPath);
                SerializedProperty sourceProperty =
                    sourceSerialized.FindProperty(propertyPath);
                ValidateDrivenOverridePropertyPreflight(
                    propertyPath,
                    sourceProperty != null,
                    sourceProperty == null
                        ? SerializedPropertyType.Generic
                        : sourceProperty.propertyType,
                    sourceProperty == null
                        ? float.NaN
                        : sourceProperty.floatValue,
                    mountedProperty != null,
                    mountedProperty == null
                        ? SerializedPropertyType.Generic
                        : mountedProperty.propertyType,
                    mountedProperty != null
                    && mountedProperty.prefabOverride);
                PrefabUtility.RevertPropertyOverride(
                    mountedProperty,
                    InteractionMode.AutomatedAction);
                mountedSerialized.UpdateIfRequiredOrScript();
                mountedProperty = mountedSerialized.FindProperty(propertyPath);
                ValidateDrivenOverridePropertyAfterRevert(
                    propertyPath,
                    mountedProperty != null,
                    mountedProperty == null
                        ? SerializedPropertyType.Generic
                        : mountedProperty.propertyType,
                    mountedProperty != null
                    && mountedProperty.prefabOverride,
                    mountedProperty == null
                        ? float.NaN
                        : mountedProperty.floatValue);
                reverted++;
            }

            Require(
                reverted == DrivenOverrideRepairPropertyPaths.Length,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REVERT_COUNT_INVALID "
                + reverted.ToString(CultureInfo.InvariantCulture));
            WriteAndRecordEmbeddedCarrierRoot(carrier);
            Require(
                IsEmbeddedCarrierRootGeometryValid(carrier),
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_ROOT_RESTORE_FAILED");
            Require(
                CountLoadedExactDrivenOverrides(carrier) == 0,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REMAINS_AFTER_ROOT_"
                + "RESTORE");
            string[] otherModificationsAfter =
                CaptureOtherPropertyModificationSignatures(
                    carrier,
                    mountedHandle,
                    sourceHandle);
            string[] beforeOnly = StableMultisetDifference(
                otherModificationsBefore,
                otherModificationsAfter);
            string[] afterOnly = StableMultisetDifference(
                otherModificationsAfter,
                otherModificationsBefore);
            Require(
                otherModificationsBefore.SequenceEqual(
                    otherModificationsAfter),
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_OTHER_MODIFICATION_DRIFT"
                + " beforeCount="
                + otherModificationsBefore.Length.ToString(
                    CultureInfo.InvariantCulture)
                + " afterCount="
                + otherModificationsAfter.Length.ToString(
                    CultureInfo.InvariantCulture)
                + " beforeOnly=" + FormatBoundedSignatures(beforeOnly)
                + " afterOnly=" + FormatBoundedSignatures(afterOnly));
        }

        internal static void ValidateDrivenOverridePropertyPreflight(
            string propertyPath,
            bool sourceExists,
            SerializedPropertyType sourceType,
            float sourceValue,
            bool mountedExists,
            SerializedPropertyType mountedType,
            bool mountedPrefabOverride)
        {
            Require(
                sourceExists && mountedExists,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_PROPERTY_MISSING "
                + propertyPath);
            Require(
                sourceType == SerializedPropertyType.Float
                && mountedType == SerializedPropertyType.Float,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_PROPERTY_TYPE_INVALID "
                + propertyPath);
            Require(
                sourceValue.Equals(0f),
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_SOURCE_BASELINE_INVALID "
                + propertyPath + " value=" + RawFloat(sourceValue));
            Require(
                mountedPrefabOverride,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_NOT_PRESENT "
                + propertyPath);
        }

        internal static void ValidateDrivenOverridePropertyAfterRevert(
            string propertyPath,
            bool mountedExists,
            SerializedPropertyType mountedType,
            bool mountedPrefabOverride,
            float drivenLiveValue)
        {
            Require(
                mountedExists,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_POST_PROPERTY_MISSING "
                + propertyPath);
            Require(
                mountedType == SerializedPropertyType.Float,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_POST_PROPERTY_TYPE_INVALID "
                + propertyPath);
            Require(
                !mountedPrefabOverride,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REVERT_FAILED "
                + propertyPath);
        }

        internal static int CountLoadedExactDrivenOverrides(
            RectTransform carrier)
        {
            RectTransform mountedHandle = ResolveExactDrivenOverrideHandle(
                carrier,
                out RectTransform sourceHandle,
                out _);
            SerializedObject mountedSerialized =
                new SerializedObject(mountedHandle);
            SerializedObject sourceSerialized =
                new SerializedObject(sourceHandle);
            mountedSerialized.UpdateIfRequiredOrScript();
            sourceSerialized.UpdateIfRequiredOrScript();
            int count = 0;
            foreach (string propertyPath in DrivenOverrideRepairPropertyPaths)
            {
                SerializedProperty mountedProperty =
                    mountedSerialized.FindProperty(propertyPath);
                SerializedProperty sourceProperty =
                    sourceSerialized.FindProperty(propertyPath);
                Require(
                    mountedProperty != null
                    && sourceProperty != null
                    && mountedProperty.propertyType
                    == SerializedPropertyType.Float
                    && sourceProperty.propertyType
                    == SerializedPropertyType.Float
                    && sourceProperty.floatValue.Equals(0f),
                    "EXACT_POPUP_MOUNT_KNOWN_HANDLE_LOADED_PROPERTY_INVALID "
                    + propertyPath);
                if (mountedProperty.prefabOverride)
                {
                    count++;
                }
            }

            return count;
        }

        private static void
            ValidateEmbeddedRootKnownHandleResidueLoadedShell(
                GameObject root,
                bool requireFormalModal = true)
        {
            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            Require(
                carrier != null
                && (requireFormalModal
                    ? presenter.ValidateAuthoredReferences()
                    : presenter.ModalBackdropImage == null
                      && presenter.ModalBackdropButton == null)
                && IsEmbeddedCarrierRootGeometryValid(carrier)
                && CountMissingScripts(root) == 0
                && root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailCloseRelay>(true).Length == 0,
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_LOADED_STATE_INVALID");
            Require(
                CountLoadedExactDrivenOverrides(carrier)
                == DrivenOverrideRepairPropertyPaths.Length,
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_LOADED_OVERRIDE_SET_INVALID");
            RectTransform mountedHandle = ResolveExactDrivenOverrideHandle(
                carrier,
                out RectTransform sourceHandle,
                out _);
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                ExactStandalonePrefabPath);
            Require(
                source != null
                && sourceHandle.transform.IsChildOf(source.transform),
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_SOURCE_ASSET_INVALID");
            ValidateNoOtherSerializedDrivenRectOverrides(
                carrier,
                mountedHandle,
                sourceHandle);
            ValidateAuthoredHierarchyAndGeometry(
                source.transform,
                carrier,
                sourceHandle);
            ValidateNoDisallowedPropertyOverrides(
                carrier,
                source,
                sourceHandle);
            ValidateEmbeddedRootPropertyOverrideContract(carrier, source);
            ValidatePresentation(
                carrier,
                presenter.PanelView,
                presenter.CloseButton);
            if (requireFormalModal)
            {
                ValidateModalBackdropContract(root, presenter, true);
            }
            ValidateNoMissingScripts(root);
            ValidateNoForbiddenOwners(root);
            Require(
                root.GetComponentsInChildren<EventSystem>(true).Length == 0,
                "EXACT_POPUP_MOUNT_DUPLICATE_EVENT_SYSTEM_OWNER");
        }

        private static void ValidateDrivenOverrideRepairableLoadedShell(
            GameObject root)
        {
            C1ExactBattleSandboxItemDetailPresenter presenter =
                Single<C1ExactBattleSandboxItemDetailPresenter>(root);
            RectTransform carrier = presenter.CarrierRoot;
            Require(
                carrier != null
                && CountMissingScripts(root) == 0
                && root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemDetailCloseRelay>(true).Length == 0,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REPAIR_LOADED_STATE_INVALID");
            RectTransform mountedHandle = ResolveExactDrivenOverrideHandle(
                carrier,
                out RectTransform sourceHandle,
                out _);
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                ExactStandalonePrefabPath);
            Require(
                source != null
                && sourceHandle.transform.IsChildOf(source.transform),
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_SOURCE_ASSET_INVALID");
            ValidateNoOtherSerializedDrivenRectOverrides(
                carrier,
                mountedHandle,
                sourceHandle);
            ValidateAuthoredHierarchyAndGeometry(
                source.transform,
                carrier,
                sourceHandle);
            ValidateNoDisallowedPropertyOverrides(
                carrier,
                source,
                sourceHandle);
            ValidatePresentation(
                carrier,
                presenter.PanelView,
                presenter.CloseButton);
            ValidateNoMissingScripts(root);
            ValidateNoForbiddenOwners(root);
            Require(
                root.GetComponentsInChildren<EventSystem>(true).Length == 0,
                "EXACT_POPUP_MOUNT_DUPLICATE_EVENT_SYSTEM_OWNER");
        }

        private static RectTransform ResolveExactDrivenOverrideHandle(
            RectTransform carrier,
            out RectTransform sourceHandle,
            out Scrollbar authoredScrollbar)
        {
            Require(
                carrier != null,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_CARRIER_MISSING");
            List<RectTransform> matches = new List<RectTransform>();
            foreach (RectTransform mountedRect in carrier
                         .GetComponentsInChildren<RectTransform>(true))
            {
                RectTransform sourceRect =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        mountedRect) as RectTransform;
                if (sourceRect != null
                    && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        sourceRect,
                        out string sourceGuid,
                        out long sourceLocalFileId)
                    && string.Equals(
                        sourceGuid,
                        ExactStandaloneGuid,
                        StringComparison.Ordinal)
                    && sourceLocalFileId
                    == DrivenOverrideHandleSourceFileId)
                {
                    matches.Add(mountedRect);
                }
            }

            Require(
                matches.Count == 1,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_HANDLE_COUNT_INVALID "
                + matches.Count.ToString(CultureInfo.InvariantCulture));
            RectTransform handle = matches[0];
            sourceHandle = PrefabUtility.GetCorrespondingObjectFromSource(
                handle) as RectTransform;
            Scrollbar[] owners = carrier.GetComponentsInChildren<Scrollbar>(true)
                .Where(scrollbar => scrollbar.handleRect == handle)
                .ToArray();
            Require(
                sourceHandle != null
                && owners.Length == 1
                && ReferenceEquals(handle.drivenByObject, owners[0]),
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_SCROLLBAR_OWNER_INVALID");
            authoredScrollbar = owners[0];
            Scrollbar sourceScrollbar =
                PrefabUtility.GetCorrespondingObjectFromSource(
                    authoredScrollbar) as Scrollbar;
            Require(
                sourceScrollbar != null
                && sourceScrollbar.handleRect == sourceHandle,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_SCROLLBAR_SOURCE_INVALID");
            return handle;
        }

        private static void ValidateNoOtherSerializedDrivenRectOverrides(
            RectTransform carrier,
            RectTransform mountedHandle,
            RectTransform sourceHandle)
        {
            string yaml = File.ReadAllText(ShellPrefabPath);
            foreach (RectTransform mountedRect in carrier
                         .GetComponentsInChildren<RectTransform>(true))
            {
                if (mountedRect == carrier
                    || mountedRect.drivenByObject == null)
                {
                    continue;
                }

                RectTransform sourceRect =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        mountedRect) as RectTransform;
                string sourceGuid = string.Empty;
                long sourceLocalFileId = 0;
                bool identityResolved = sourceRect != null
                    && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        sourceRect,
                        out sourceGuid,
                        out sourceLocalFileId);
                Require(
                    identityResolved
                    && string.Equals(
                        sourceGuid,
                        ExactStandaloneGuid,
                        StringComparison.Ordinal)
                    && sourceLocalFileId != 0,
                    "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_SOURCE_IDENTITY_INVALID");
                int serializedRows =
                    CountSerializedModificationTargetRowsInYaml(
                        yaml,
                        sourceGuid,
                        sourceLocalFileId);
                int expectedRows = mountedRect == mountedHandle
                                   && sourceRect == sourceHandle
                    ? DrivenOverrideRepairPropertyPaths.Length
                    : 0;
                Require(
                    serializedRows == expectedRows,
                    "EXACT_POPUP_MOUNT_OTHER_DRIVEN_RECT_OVERRIDE"
                    + " source=" + sourceGuid + ":"
                    + sourceLocalFileId.ToString(
                        CultureInfo.InvariantCulture)
                    + " rows=" + serializedRows.ToString(
                        CultureInfo.InvariantCulture));
            }
        }

        private static string[] CaptureOtherPropertyModificationSignatures(
            RectTransform carrier,
            RectTransform mountedHandle,
            RectTransform sourceHandle)
        {
            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(carrier.gameObject)
                ?? Array.Empty<PropertyModification>();
            return modifications
                .Where(modification =>
                    !IsExactDrivenOverrideRepairModification(
                        modification,
                        mountedHandle,
                        sourceHandle))
                .Select(PropertyModificationSignature)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
        }

        internal static string[] CaptureOtherPropertyModificationSignatures(
            RectTransform carrier)
        {
            RectTransform mountedHandle = ResolveExactDrivenOverrideHandle(
                carrier,
                out RectTransform sourceHandle,
                out _);
            return CaptureOtherPropertyModificationSignatures(
                carrier,
                mountedHandle,
                sourceHandle);
        }

        private static string[] StableMultisetDifference(
            IReadOnlyList<string> source,
            IReadOnlyList<string> subtract)
        {
            Dictionary<string, int> remaining = new Dictionary<string, int>(
                StringComparer.Ordinal);
            foreach (string signature in subtract)
            {
                remaining.TryGetValue(signature, out int count);
                remaining[signature] = count + 1;
            }

            List<string> difference = new List<string>();
            foreach (string signature in source.OrderBy(
                         value => value,
                         StringComparer.Ordinal))
            {
                if (remaining.TryGetValue(signature, out int count)
                    && count > 0)
                {
                    remaining[signature] = count - 1;
                    continue;
                }

                difference.Add(signature);
            }

            return difference.ToArray();
        }

        private static string FormatBoundedSignatures(
            IReadOnlyList<string> signatures)
        {
            int displayed = Math.Min(
                signatures.Count,
                OtherModificationDiffEntryLimit);
            string result = "[" + string.Join(
                " || ",
                signatures.Take(displayed)) + "]";
            int omitted = signatures.Count - displayed;
            return omitted <= 0
                ? result
                : result + "(+"
                  + omitted.ToString(CultureInfo.InvariantCulture)
                  + " more)";
        }

        private static bool IsExactDrivenOverrideRepairModification(
            PropertyModification modification,
            RectTransform mountedHandle,
            RectTransform sourceHandle)
        {
            return modification != null
                   && (modification.target == mountedHandle
                       || modification.target == sourceHandle)
                   && DrivenOverrideRepairPropertyPaths.Contains(
                       modification.propertyPath,
                       StringComparer.Ordinal);
        }

        private static string PropertyModificationSignature(
            PropertyModification modification)
        {
            return modification == null
                ? "<null>"
                : StableObjectIdentity(modification.target)
                  + "." + (modification.propertyPath ?? "<null>")
                  + "=" + (modification.value ?? "<null>")
                  + "@" + StableObjectIdentity(
                      modification.objectReference);
        }

        private static void ValidatePresentation(
            RectTransform carrier,
            ItemDetailPanelView panel,
            Button closeButton)
        {
            Require(
                carrier.GetComponents<Canvas>().Length == 1
                && carrier.GetComponents<CanvasScaler>().Length == 1
                && carrier.GetComponents<GraphicRaycaster>().Length == 1,
                "EXACT_POPUP_MOUNT_CANVAS_CARRIER_INVALID");
            Transform mobile = RequiredDirectChild(
                carrier,
                "MobileSafeAreaRoot");
            Transform safe = RequiredDirectChild(mobile, "SafeAreaRoot");
            Transform popup = RequiredDirectChild(safe, "PopupLayer");
            Transform authoredPanel = RequiredDirectChild(
                popup,
                "ItemDetailPanel");
            Require(
                authoredPanel == panel.transform,
                "EXACT_POPUP_MOUNT_PANEL_CHAIN_INVALID");

            RectTransform panelRect = panel.transform as RectTransform;
            Require(
                panelRect != null
                && IsFinitePositive(panelRect.sizeDelta.x)
                && IsFinitePositive(panelRect.sizeDelta.y)
                && IsFinitePositive(Mathf.Abs(panelRect.localScale.x))
                && IsFinitePositive(Mathf.Abs(panelRect.localScale.y)),
                "EXACT_POPUP_MOUNT_PANEL_AUTHORED_GEOMETRY_INVALID");
            ScrollRect[] scrollRects = carrier.GetComponentsInChildren<
                ScrollRect>(true);
            Require(
                scrollRects.Length == 2,
                "EXACT_POPUP_MOUNT_SCROLL_RECT_COUNT_INVALID "
                + scrollRects.Length);
            foreach (ScrollRect scrollRect in scrollRects)
            {
                Require(
                    scrollRect != null
                    && scrollRect.enabled
                    && scrollRect.viewport != null
                    && scrollRect.content != null
                    && scrollRect.vertical
                    && (scrollRect.viewport.GetComponent<Mask>() != null
                        || scrollRect.viewport.GetComponent<RectMask2D>() != null),
                    "EXACT_POPUP_MOUNT_SCROLL_OR_MASK_INVALID");
            }

            Require(
                closeButton != null
                && closeButton.interactable
                && closeButton.gameObject.activeSelf
                && closeButton.transform.IsChildOf(panel.transform)
                && closeButton.targetGraphic != null
                && closeButton.targetGraphic.raycastTarget,
                "EXACT_POPUP_MOUNT_CLOSE_CONTROL_INVALID");
            Image cardBackground = RequiredObjectReference<Image>(
                panel,
                "cardBackgroundImage",
                "EXACT_POPUP_MOUNT_CARD_BACKGROUND_MISSING");
            Text itemName = RequiredObjectReference<Text>(
                panel,
                "itemNameText",
                "EXACT_POPUP_MOUNT_READABLE_ITEM_NAME_MISSING");
            Require(
                cardBackground.gameObject.activeSelf
                && itemName.gameObject.activeSelf
                && itemName.transform.IsChildOf(cardBackground.transform),
                "EXACT_POPUP_MOUNT_BACKING_OR_READABLE_CONTENT_ORDER_INVALID");
            Require(
                carrier.GetComponentsInChildren<CanvasGroup>(true)
                    .All(group => group != null && !group.ignoreParentGroups),
                "EXACT_POPUP_MOUNT_CARRIER_IGNORES_CLOSED_GATE");
        }

        private static void ValidateNoDisallowedPropertyOverrides(
            RectTransform carrier,
            GameObject exactSourceRoot,
            RectTransform allowedDrivenOverrideSourceRect = null)
        {
            GameObject correspondingSourceRoot =
                PrefabUtility.GetCorrespondingObjectFromSource(
                    carrier.gameObject);
            RectTransform exactSourceRootRect =
                exactSourceRoot.transform as RectTransform;
            RectTransform correspondingSourceRootRect =
                PrefabUtility.GetCorrespondingObjectFromSource(carrier);
            Require(
                correspondingSourceRoot == exactSourceRoot
                && exactSourceRootRect != null
                && correspondingSourceRootRect == exactSourceRootRect
                && string.Equals(
                    AssetDatabase.GetAssetPath(correspondingSourceRoot),
                    ExactStandalonePrefabPath,
                    StringComparison.Ordinal),
                "EXACT_POPUP_MOUNT_ROOT_SOURCE_CORRESPONDENCE_INVALID");
            ValidateExactRootCanvasComponentContract(
                exactSourceRoot,
                carrier.gameObject);
            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(carrier.gameObject)
                ?? Array.Empty<PropertyModification>();
            HashSet<Object> transientDrivenTargets =
                new HashSet<Object>();
            foreach (RectTransform mountedRect in carrier
                         .GetComponentsInChildren<RectTransform>(true))
            {
                if (mountedRect.drivenByObject == null)
                {
                    continue;
                }

                RectTransform sourceRect =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        mountedRect) as RectTransform;
                if (sourceRect != null
                    && (sourceRect == allowedDrivenOverrideSourceRect
                        || FindFirstLoadedRectOverride(
                            mountedRect,
                            sourceRect) == null))
                {
                    transientDrivenTargets.Add(sourceRect);
                    transientDrivenTargets.Add(mountedRect);
                }
            }

            modifications = modifications
                .Where(modification => modification == null
                    || !transientDrivenTargets.Contains(modification.target))
                .ToArray();
            ValidateRootNameMetadataOverrideContract(
                exactSourceRoot,
                exactSourceRootRect,
                carrier.gameObject,
                modifications);
        }

        private static void ValidateExactRootCanvasComponentContract(
            GameObject exactSourceRoot,
            GameObject mountedRoot)
        {
            Type[] expectedTypes =
            {
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            };
            Component[] sourceComponents =
                exactSourceRoot.GetComponents<Component>();
            Component[] mountedComponents =
                mountedRoot.GetComponents<Component>();
            RectTransform sourceRect = exactSourceRoot.transform
                as RectTransform;
            Require(
                sourceRect != null
                && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    sourceRect,
                    out string sourceGuid,
                    out long sourceLocalFileId)
                && string.Equals(
                    sourceGuid,
                    ExactStandaloneGuid,
                    StringComparison.Ordinal)
                && sourceLocalFileId == ExactStandaloneRootRectSourceFileId,
                "EXACT_POPUP_MOUNT_ROOT_RECT_SOURCE_IDENTITY_INVALID");
            Require(
                sourceComponents.Length == expectedTypes.Length
                && mountedComponents.Length == expectedTypes.Length,
                "EXACT_POPUP_MOUNT_ROOT_CANVAS_COMPONENT_COUNT_INVALID");
            for (int index = 0; index < expectedTypes.Length; index++)
            {
                Require(
                    sourceComponents[index] != null
                    && mountedComponents[index] != null
                    && sourceComponents[index].GetType()
                        == expectedTypes[index]
                    && mountedComponents[index].GetType()
                        == expectedTypes[index]
                    && PrefabUtility.GetCorrespondingObjectFromSource(
                        mountedComponents[index]) == sourceComponents[index],
                    "EXACT_POPUP_MOUNT_ROOT_CANVAS_COMPONENT_ORDER_OR_SOURCE_"
                    + "INVALID index="
                    + index.ToString(CultureInfo.InvariantCulture));
            }

            Canvas sourceCanvas = sourceComponents[1] as Canvas;
            Canvas mountedCanvas = mountedComponents[1] as Canvas;
            Require(
                sourceCanvas != null
                && mountedCanvas != null
                && sourceCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                && mountedCanvas.renderMode == sourceCanvas.renderMode,
                "EXACT_POPUP_MOUNT_ROOT_CANVAS_RENDER_MODE_INVALID");
        }

        private static void ValidateEmbeddedRootPropertyOverrideContract(
            RectTransform mountedRoot,
            GameObject exactSourceRoot)
        {
            RectTransform sourceRoot = exactSourceRoot == null
                ? null
                : exactSourceRoot.transform as RectTransform;
            Require(
                mountedRoot != null && sourceRoot != null,
                "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_OVERRIDE_ROOT_MISSING");
            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(
                    mountedRoot.gameObject)
                ?? Array.Empty<PropertyModification>();
            Dictionary<string, float> actual = new Dictionary<string, float>(
                StringComparer.Ordinal);
            foreach (PropertyModification modification in modifications)
            {
                if (modification == null
                    || modification.target != sourceRoot)
                {
                    continue;
                }

                Require(
                    EmbeddedRootPropertyValues.ContainsKey(
                        modification.propertyPath ?? string.Empty)
                    && modification.objectReference == null
                    && float.TryParse(
                        modification.value,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float value)
                    && IsFinite(value)
                    && actual.TryAdd(modification.propertyPath, value),
                    "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_OVERRIDE_INVALID "
                    + (modification.propertyPath ?? "<null>"));
            }

            Require(
                actual.Count == EmbeddedRootPropertyValues.Count,
                "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_OVERRIDE_COUNT_INVALID "
                + actual.Count.ToString(CultureInfo.InvariantCulture));
            foreach (KeyValuePair<string, float> expected in
                     EmbeddedRootPropertyValues)
            {
                Require(
                    actual.TryGetValue(expected.Key, out float value)
                    && Mathf.Abs(value - expected.Value)
                    <= RectValueTolerance,
                    "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_OVERRIDE_VALUE_INVALID "
                    + expected.Key);
            }
        }

        internal static void ValidateRootNameMetadataOverrideContract(
            GameObject exactSourceRoot,
            RectTransform exactSourceRootRect,
            GameObject mountedRoot,
            IReadOnlyList<PropertyModification> modifications)
        {
            Require(
                exactSourceRoot != null
                && exactSourceRootRect != null
                && exactSourceRootRect.gameObject == exactSourceRoot
                && mountedRoot != null,
                "EXACT_POPUP_MOUNT_ROOT_NAME_OVERRIDE_ROOT_MISSING");
            int rootNameOverrideCount = 0;
            HashSet<string> seenRootCanvasRectContextPaths =
                new HashSet<string>(StringComparer.Ordinal);
            IReadOnlyList<PropertyModification> values = modifications
                ?? Array.Empty<PropertyModification>();
            foreach (PropertyModification modification in values)
            {
                if (modification == null)
                {
                    continue;
                }

                bool isExactRootNameMetadata =
                    modification.target == exactSourceRoot
                    && string.Equals(
                        modification.propertyPath,
                        "m_Name",
                        StringComparison.Ordinal);
                if (isExactRootNameMetadata)
                {
                    rootNameOverrideCount++;
                    Require(
                        rootNameOverrideCount == 1,
                        "EXACT_POPUP_MOUNT_ROOT_NAME_OVERRIDE_COUNT_INVALID "
                        + rootNameOverrideCount.ToString(
                            CultureInfo.InvariantCulture));
                    Require(
                        modification.objectReference == null,
                        "EXACT_POPUP_MOUNT_ROOT_NAME_OVERRIDE_REFERENCE_INVALID");
                    Require(
                        string.Equals(
                            modification.value,
                            exactSourceRoot.name,
                            StringComparison.Ordinal)
                        && string.Equals(
                            mountedRoot.name,
                            exactSourceRoot.name,
                            StringComparison.Ordinal),
                        "EXACT_POPUP_MOUNT_ROOT_NAME_OVERRIDE_VALUE_INVALID"
                        + " source=" + exactSourceRoot.name
                        + " mounted=" + mountedRoot.name
                        + " value=" + (modification.value ?? "null"));
                    continue;
                }

                if (modification.target == exactSourceRootRect)
                {
                    string propertyPath =
                        modification.propertyPath ?? string.Empty;
                    Require(
                        RootCanvasRectContextPropertyPaths.Contains(
                            propertyPath),
                        "EXACT_POPUP_MOUNT_ROOT_CANVAS_RECT_OVERRIDE_PROPERTY_"
                        + "INVALID " + propertyPath);
                    Require(
                        seenRootCanvasRectContextPaths.Add(propertyPath),
                        "EXACT_POPUP_MOUNT_ROOT_CANVAS_RECT_OVERRIDE_DUPLICATE "
                        + propertyPath);
                    Require(
                        modification.objectReference == null
                        && float.TryParse(
                            modification.value,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out float numericValue)
                        && !float.IsNaN(numericValue)
                        && !float.IsInfinity(numericValue),
                        "EXACT_POPUP_MOUNT_ROOT_CANVAS_RECT_OVERRIDE_VALUE_"
                        + "INVALID property=" + propertyPath
                        + " value=" + (modification.value ?? "null"));
                    continue;
                }

                throw new InvalidOperationException(
                    "EXACT_POPUP_MOUNT_INTERNAL_PROPERTY_OVERRIDE "
                    + StableObjectIdentity(modification.target)
                    + "." + (modification.propertyPath ?? string.Empty));
            }

            Require(
                rootNameOverrideCount == 1,
                "EXACT_POPUP_MOUNT_ROOT_NAME_OVERRIDE_COUNT_INVALID "
                + rootNameOverrideCount.ToString(
                    CultureInfo.InvariantCulture));
        }

        internal static void ValidateAuthoredHierarchyAndGeometry(
            Transform sourceRoot,
            Transform mountedRoot,
            RectTransform allowedDrivenOverrideSourceRect = null)
        {
            Transform[] sourceTransforms =
                sourceRoot.GetComponentsInChildren<Transform>(true);
            Transform[] mountedTransforms =
                mountedRoot.GetComponentsInChildren<Transform>(true);
            if (sourceTransforms.Length != mountedTransforms.Length)
            {
                ThrowAuthoredParityMismatch(
                    "$",
                    "transformCount",
                    sourceTransforms.Length.ToString(
                        CultureInfo.InvariantCulture),
                    mountedTransforms.Length.ToString(
                        CultureInfo.InvariantCulture),
                    Math.Abs(sourceTransforms.Length - mountedTransforms.Length)
                        .ToString(CultureInfo.InvariantCulture));
            }

            for (int index = 0; index < sourceTransforms.Length; index++)
            {
                Transform source = sourceTransforms[index];
                Transform mounted = mountedTransforms[index];
                string sourcePath = RelativePath(sourceRoot, source);
                string mountedPath = RelativePath(mountedRoot, mounted);
                if (!string.Equals(
                        sourcePath,
                        mountedPath,
                        StringComparison.Ordinal))
                {
                    ThrowAuthoredParityMismatch(
                        sourcePath,
                        "path",
                        sourcePath,
                        mountedPath,
                        "n/a");
                }

                string path = sourcePath;
                ValidateExactAuthoredParity(
                    path,
                    "activeSelf",
                    source.gameObject.activeSelf ? "1" : "0",
                    mounted.gameObject.activeSelf ? "1" : "0");
                ValidateExactAuthoredParity(
                    path,
                    "layer",
                    source.gameObject.layer.ToString(
                        CultureInfo.InvariantCulture),
                    mounted.gameObject.layer.ToString(
                        CultureInfo.InvariantCulture));

                Component[] sourceComponents = ComparableComponents(source);
                Component[] mountedComponents = ComparableComponents(mounted);
                if (sourceComponents.Length != mountedComponents.Length)
                {
                    ThrowAuthoredParityMismatch(
                        path,
                        "componentCount",
                        sourceComponents.Length.ToString(
                            CultureInfo.InvariantCulture),
                        mountedComponents.Length.ToString(
                            CultureInfo.InvariantCulture),
                        Math.Abs(
                            sourceComponents.Length - mountedComponents.Length)
                            .ToString(CultureInfo.InvariantCulture));
                }

                for (int componentIndex = 0;
                     componentIndex < sourceComponents.Length;
                     componentIndex++)
                {
                    Component sourceComponent =
                        sourceComponents[componentIndex];
                    Component mountedComponent =
                        mountedComponents[componentIndex];
                    string sourceType = sourceComponent == null
                        ? "<missing-script>"
                        : sourceComponent.GetType().FullName;
                    string mountedType = mountedComponent == null
                        ? "<missing-script>"
                        : mountedComponent.GetType().FullName;
                    if (sourceComponent == null || mountedComponent == null)
                    {
                        ThrowAuthoredParityMismatch(
                            path,
                            "componentType[" + componentIndex.ToString(
                                CultureInfo.InvariantCulture) + "]",
                            sourceType,
                            mountedType,
                            "missing-script");
                    }

                    ValidateExactAuthoredParity(
                        path,
                        "componentType[" + componentIndex.ToString(
                            CultureInfo.InvariantCulture) + "]",
                        sourceType,
                        mountedType);
                }

                RectTransform sourceRect = source as RectTransform;
                RectTransform mountedRect = mounted as RectTransform;
                if ((sourceRect == null) != (mountedRect == null))
                {
                    ThrowAuthoredParityMismatch(
                        path,
                        "rectTransform",
                        sourceRect == null ? "absent" : "present",
                        mountedRect == null ? "absent" : "present",
                        "n/a");
                }

                bool isContextDrivenRootCanvas =
                    source == sourceRoot
                    && mounted == mountedRoot
                    && source.GetComponent<Canvas>() != null
                    && mounted.GetComponent<Canvas>() != null;
                if (sourceRect != null
                    && mountedRect != null
                    && !isContextDrivenRootCanvas)
                {
                    bool hasDrivenOwner =
                        mountedRect.drivenByObject != null;
                    RectTransform correspondingSourceRect = hasDrivenOwner
                        ? PrefabUtility.GetCorrespondingObjectFromSource(
                            mountedRect) as RectTransform
                        : null;
                    string serializedOverrideProperty = hasDrivenOwner
                        ? FindFirstLoadedRectOverride(
                            mountedRect,
                            sourceRect)
                        : null;
                    if (sourceRect == allowedDrivenOverrideSourceRect)
                    {
                        serializedOverrideProperty = null;
                    }

                    ValidateRectParityWithDriverState(
                        path,
                        CaptureAuthoredRectGeometry(sourceRect),
                        CaptureAuthoredRectGeometry(mountedRect),
                        hasDrivenOwner,
                        !hasDrivenOwner
                        || correspondingSourceRect == sourceRect,
                        serializedOverrideProperty,
                        hasDrivenOwner
                            ? FindFirstComponentSourceConflict(
                                sourceComponents,
                                mountedComponents)
                            : null);
                }
            }
        }

        private static string FindFirstLoadedRectOverride(
            RectTransform mountedRect,
            RectTransform sourceRect)
        {
            Require(
                mountedRect != null && sourceRect != null,
                "EXACT_POPUP_MOUNT_DRIVEN_RECT_SOURCE_IDENTITY_INVALID");
            GameObject instanceRoot =
                PrefabUtility.GetNearestPrefabInstanceRoot(
                    mountedRect.gameObject);
            Require(
                instanceRoot != null,
                "EXACT_POPUP_MOUNT_DRIVEN_RECT_SOURCE_IDENTITY_INVALID");
            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(instanceRoot)
                ?? Array.Empty<PropertyModification>();
            return modifications
                .Where(modification => modification != null
                    && (modification.target == sourceRect
                        || modification.target == mountedRect)
                    && !string.IsNullOrWhiteSpace(
                        modification.propertyPath))
                .Select(modification => modification.propertyPath)
                .OrderBy(value => value, StringComparer.Ordinal)
                .FirstOrDefault();
        }

        internal static string FindFirstSerializedRectOverrideInYaml(
            string yaml,
            string sourceGuid,
            long sourceLocalFileId)
        {
            Require(
                !string.IsNullOrWhiteSpace(sourceGuid)
                && sourceLocalFileId != 0,
                "EXACT_POPUP_MOUNT_SERIALIZED_RECT_SOURCE_IDENTITY_INVALID");
            string exactTarget = "- target: {fileID: "
                                 + sourceLocalFileId.ToString(
                                     CultureInfo.InvariantCulture)
                                 + ", guid: " + sourceGuid + ", type: 3}";
            string[] lines = NormalizeYaml(yaml).Split('\n');
            bool insideModifications = false;
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index].Trim();
                if (string.Equals(
                        line,
                        "m_Modifications:",
                        StringComparison.Ordinal))
                {
                    insideModifications = true;
                    continue;
                }

                if (!insideModifications)
                {
                    continue;
                }

                if (line.StartsWith(
                        "m_RemovedComponents:",
                        StringComparison.Ordinal))
                {
                    insideModifications = false;
                    continue;
                }

                if (!string.Equals(line, exactTarget, StringComparison.Ordinal))
                {
                    continue;
                }

                for (int propertyIndex = index + 1;
                     propertyIndex < lines.Length;
                     propertyIndex++)
                {
                    string propertyLine = lines[propertyIndex].Trim();
                    if (propertyLine.StartsWith(
                            "propertyPath:",
                            StringComparison.Ordinal))
                    {
                        string propertyPath = propertyLine.Substring(
                                "propertyPath:".Length)
                            .Trim();
                        return string.IsNullOrEmpty(propertyPath)
                            ? "<unknown>"
                            : propertyPath;
                    }

                    if (propertyLine.StartsWith(
                            "- target:",
                            StringComparison.Ordinal)
                        || propertyLine.StartsWith(
                            "m_RemovedComponents:",
                            StringComparison.Ordinal))
                    {
                        return "<unknown>";
                    }
                }

                return "<unknown>";
            }

            return null;
        }

        private static string FindFirstComponentSourceConflict(
            IReadOnlyList<Component> sourceComponents,
            IReadOnlyList<Component> mountedComponents)
        {
            for (int index = 0; index < sourceComponents.Count; index++)
            {
                Component correspondingSource =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        mountedComponents[index]);
                if (correspondingSource != sourceComponents[index])
                {
                    return "component["
                           + index.ToString(CultureInfo.InvariantCulture)
                           + "]="
                           + mountedComponents[index].GetType().FullName;
                }
            }

            return null;
        }

        private static Component[] ComparableComponents(Transform current)
        {
            return current.GetComponents<Component>();
        }

        private static AuthoredRectGeometry CaptureAuthoredRectGeometry(
            RectTransform rect)
        {
            return new AuthoredRectGeometry(
                rect.anchorMin,
                rect.anchorMax,
                rect.pivot,
                rect.anchoredPosition,
                rect.sizeDelta,
                rect.localScale,
                rect.localRotation);
        }

        internal static void ValidateAuthoredRectGeometry(
            string path,
            AuthoredRectGeometry source,
            AuthoredRectGeometry mounted)
        {
            ValidateRectFloat(
                path,
                "anchorMin.x",
                source.AnchorMin.x,
                mounted.AnchorMin.x);
            ValidateRectFloat(
                path,
                "anchorMin.y",
                source.AnchorMin.y,
                mounted.AnchorMin.y);
            ValidateRectFloat(
                path,
                "anchorMax.x",
                source.AnchorMax.x,
                mounted.AnchorMax.x);
            ValidateRectFloat(
                path,
                "anchorMax.y",
                source.AnchorMax.y,
                mounted.AnchorMax.y);
            ValidateRectFloat(
                path,
                "pivot.x",
                source.Pivot.x,
                mounted.Pivot.x);
            ValidateRectFloat(
                path,
                "pivot.y",
                source.Pivot.y,
                mounted.Pivot.y);
            ValidateRectFloat(
                path,
                "anchoredPosition.x",
                source.AnchoredPosition.x,
                mounted.AnchoredPosition.x);
            ValidateRectFloat(
                path,
                "anchoredPosition.y",
                source.AnchoredPosition.y,
                mounted.AnchoredPosition.y);
            ValidateRectFloat(
                path,
                "sizeDelta.x",
                source.SizeDelta.x,
                mounted.SizeDelta.x);
            ValidateRectFloat(
                path,
                "sizeDelta.y",
                source.SizeDelta.y,
                mounted.SizeDelta.y);
            ValidateRectFloat(
                path,
                "localScale.x",
                source.LocalScale.x,
                mounted.LocalScale.x);
            ValidateRectFloat(
                path,
                "localScale.y",
                source.LocalScale.y,
                mounted.LocalScale.y);
            ValidateRectFloat(
                path,
                "localScale.z",
                source.LocalScale.z,
                mounted.LocalScale.z);

            float angleDelta = Quaternion.Angle(
                source.LocalRotation,
                mounted.LocalRotation);
            if (!(angleDelta <= RectRotationToleranceDegrees))
            {
                ThrowAuthoredParityMismatch(
                    path,
                    "localRotation",
                    RawQuaternion(source.LocalRotation),
                    RawQuaternion(mounted.LocalRotation),
                    RawFloat(angleDelta),
                    " toleranceDegrees="
                    + RawFloat(RectRotationToleranceDegrees));
            }
        }

        internal static void ValidateRectParityWithDriverState(
            string path,
            AuthoredRectGeometry source,
            AuthoredRectGeometry mounted,
            bool hasDrivenOwner,
            bool hasExactSourceCorrespondence,
            string serializedOverrideProperty,
            string componentSourceConflict)
        {
            if (!hasDrivenOwner)
            {
                ValidateAuthoredRectGeometry(path, source, mounted);
                return;
            }

            if (!hasExactSourceCorrespondence)
            {
                throw new InvalidOperationException(
                    "EXACT_POPUP_MOUNT_DRIVEN_RECT_SOURCE_CORRESPONDENCE_INVALID"
                    + " path=" + path);
            }

            if (!string.IsNullOrEmpty(componentSourceConflict))
            {
                throw new InvalidOperationException(
                    "EXACT_POPUP_MOUNT_DRIVEN_RECT_COMPONENT_SOURCE_INVALID"
                    + " path=" + path
                    + " component=" + componentSourceConflict);
            }

            if (!string.IsNullOrEmpty(serializedOverrideProperty))
            {
                throw new InvalidOperationException(
                    "EXACT_POPUP_MOUNT_DRIVEN_RECT_SERIALIZED_OVERRIDE"
                    + " path=" + path
                    + " property=" + serializedOverrideProperty);
            }
        }

        private static void ValidateRectFloat(
            string path,
            string field,
            float source,
            float mounted)
        {
            float delta = Mathf.Abs(source - mounted);
            if (!(delta <= RectValueTolerance))
            {
                ThrowAuthoredParityMismatch(
                    path,
                    field,
                    RawFloat(source),
                    RawFloat(mounted),
                    RawFloat(delta),
                    " tolerance=" + RawFloat(RectValueTolerance));
            }
        }

        private static void ValidateExactAuthoredParity(
            string path,
            string field,
            string source,
            string mounted)
        {
            if (!string.Equals(source, mounted, StringComparison.Ordinal))
            {
                ThrowAuthoredParityMismatch(
                    path,
                    field,
                    source,
                    mounted,
                    "n/a");
            }
        }

        private static void ThrowAuthoredParityMismatch(
            string path,
            string field,
            string source,
            string mounted,
            string delta,
            string suffix = "")
        {
            throw new InvalidOperationException(
                "EXACT_POPUP_MOUNT_AUTHORED_PARITY_FAIL"
                + " path=" + path
                + " field=" + field
                + " source=" + source
                + " mounted=" + mounted
                + " delta=" + delta
                + suffix);
        }

        internal static void ValidateExactZeroRootMountRepairableSerializedShell()
        {
            Require(
                IsExpectedZeroRootMountRepairableHash(
                    ComputeSha256(ShellPrefabPath)),
                "EXACT_POPUP_MOUNT_ZERO_ROOT_HASH_INVALID");
            string yaml = NormalizeYaml(File.ReadAllText(ShellPrefabPath));
            ValidateExactZeroRootMountRowsInYaml(yaml);
        }

        internal static bool IsExpectedEmbeddedRootKnownHandleResidueHash(
            string actualHash)
        {
            return string.Equals(
                actualHash,
                ExpectedEmbeddedRootKnownHandleResidueShellHash,
                StringComparison.OrdinalIgnoreCase);
        }

        internal static void
            ValidateEmbeddedRootKnownHandleResidueSerializedShell()
        {
            Require(
                IsExpectedEmbeddedRootKnownHandleResidueHash(
                    ComputeSha256(ShellPrefabPath)),
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_HASH_INVALID");
            string yaml = NormalizeYaml(File.ReadAllText(ShellPrefabPath));
            ValidateEmbeddedRootKnownHandleResidueRowsInYaml(yaml);
        }

        internal static void ValidateEmbeddedRootKnownHandleResidueRowsInYaml(
            string yaml)
        {
            string normalized = NormalizeYaml(yaml);
            ValidateEmbeddedRootRowsInYaml(normalized);
            ValidateDrivenOverrideRepairRowsInYaml(normalized);
            Require(
                CountSerializedModificationRowsForGuid(
                    normalized,
                    ExactStandaloneGuid)
                == EmbeddedRootPropertyValues.Count
                   + DrivenOverrideRepairPropertyPaths.Length
                   + 1,
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_OTHER_TARGET_ROW_INVALID");
            Require(
                CountToken(
                    normalized,
                    "m_SourcePrefab: {fileID: 100100000, guid: "
                    + ExactStandaloneGuid) == 1
                && CountToken(normalized, LegacyPanelGuid) == 0
                && CountToken(normalized, "  m_Script: {fileID: 0}") == 0
                && CountToken(normalized, "carrierRoot: {fileID: 0}") == 0
                && CountToken(normalized, "carrierRoot: {fileID:") == 1,
                "EXACT_POPUP_MOUNT_KNOWN_HANDLE_SERIALIZED_STATE_INVALID");
        }

        internal static int CountSerializedModificationRowsForGuid(
            string yaml,
            string sourceGuid)
        {
            Require(
                !string.IsNullOrWhiteSpace(sourceGuid),
                "EXACT_POPUP_MOUNT_SERIALIZED_GUID_MISSING");
            string[] lines = NormalizeYaml(yaml).Split('\n');
            bool insideModifications = false;
            int count = 0;
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.Equals(
                        line,
                        "m_Modifications:",
                        StringComparison.Ordinal))
                {
                    insideModifications = true;
                    continue;
                }

                if (!insideModifications)
                {
                    continue;
                }

                if (line.StartsWith(
                        "m_RemovedComponents:",
                        StringComparison.Ordinal))
                {
                    insideModifications = false;
                    continue;
                }

                if (line.StartsWith(
                        "- target: {fileID:",
                        StringComparison.Ordinal)
                    && line.Contains(
                        ", guid: " + sourceGuid + ", type: 3}") )
                {
                    count++;
                }
            }

            return count;
        }

        internal static bool IsExpectedZeroRootMountRepairableHash(
            string actualHash)
        {
            return string.Equals(
                actualHash,
                ExpectedZeroRootMountRepairableShellHash,
                StringComparison.OrdinalIgnoreCase);
        }

        internal static void ValidateExactZeroRootMountRowsInYaml(
            string yaml)
        {
            Dictionary<string, float> values =
                ReadSerializedRootRectModificationValues(yaml);
            Require(
                values.Count == EmbeddedRootPropertyValues.Count - 3
                && !values.ContainsKey("m_LocalScale.x")
                && !values.ContainsKey("m_LocalScale.y")
                && !values.ContainsKey("m_LocalScale.z"),
                "EXACT_POPUP_MOUNT_ZERO_ROOT_ROW_COUNT_INVALID "
                + values.Count.ToString(CultureInfo.InvariantCulture));
            foreach (KeyValuePair<string, float> expected in
                     EmbeddedRootPropertyValues)
            {
                if (expected.Key.StartsWith(
                        "m_LocalScale.",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                float expectedValue = expected.Key.StartsWith(
                                          "m_AnchorMax.",
                                          StringComparison.Ordinal)
                                      || expected.Key.StartsWith(
                                          "m_Pivot.",
                                          StringComparison.Ordinal)
                    ? 0f
                    : expected.Value;
                Require(
                    values.TryGetValue(expected.Key, out float actual)
                    && Mathf.Abs(actual - expectedValue)
                    <= RectValueTolerance,
                    "EXACT_POPUP_MOUNT_ZERO_ROOT_ROW_VALUE_INVALID "
                    + expected.Key);
            }
        }

        internal static void ValidateEmbeddedRootRowsInYaml(string yaml)
        {
            Dictionary<string, float> values =
                ReadSerializedRootRectModificationValues(yaml);
            Require(
                values.Count == EmbeddedRootPropertyValues.Count,
                "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_ROW_COUNT_INVALID "
                + values.Count.ToString(CultureInfo.InvariantCulture));
            foreach (KeyValuePair<string, float> expected in
                     EmbeddedRootPropertyValues)
            {
                Require(
                    values.TryGetValue(expected.Key, out float actual)
                    && Mathf.Abs(actual - expected.Value)
                    <= RectValueTolerance,
                    "EXACT_POPUP_MOUNT_EMBEDDED_ROOT_ROW_VALUE_INVALID "
                    + expected.Key);
            }
        }

        private static Dictionary<string, float>
            ReadSerializedRootRectModificationValues(string yaml)
        {
            string[] lines = NormalizeYaml(yaml).Split('\n');
            string exactTarget = "- target: {fileID: "
                                 + ExactStandaloneRootRectSourceFileId
                                     .ToString(CultureInfo.InvariantCulture)
                                 + ", guid: " + ExactStandaloneGuid
                                 + ", type: 3}";
            Dictionary<string, float> values = new Dictionary<string, float>(
                StringComparer.Ordinal);
            for (int index = 0; index < lines.Length; index++)
            {
                if (!string.Equals(
                        lines[index].Trim(),
                        exactTarget,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                Require(
                    index + 3 < lines.Length,
                    "EXACT_POPUP_MOUNT_ROOT_ROW_TRUNCATED");
                string propertyLine = lines[index + 1].Trim();
                string valueLine = lines[index + 2].Trim();
                string referenceLine = lines[index + 3].Trim();
                const string propertyPrefix = "propertyPath: ";
                const string valuePrefix = "value: ";
                Require(
                    propertyLine.StartsWith(
                        propertyPrefix,
                        StringComparison.Ordinal)
                    && valueLine.StartsWith(
                        valuePrefix,
                        StringComparison.Ordinal)
                    && string.Equals(
                        referenceLine,
                        "objectReference: {fileID: 0}",
                        StringComparison.Ordinal),
                    "EXACT_POPUP_MOUNT_ROOT_ROW_FORMAT_INVALID");
                string propertyPath = propertyLine.Substring(
                    propertyPrefix.Length);
                bool parsed = float.TryParse(
                    valueLine.Substring(valuePrefix.Length),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float value);
                Require(
                    RootCanvasRectContextPropertyPaths.Contains(propertyPath)
                    && parsed
                    && IsFinite(value)
                    && !values.ContainsKey(propertyPath),
                    "EXACT_POPUP_MOUNT_ROOT_ROW_PAYLOAD_INVALID "
                    + propertyPath);
                values.Add(propertyPath, value);
            }

            return values;
        }

        private static string RawQuaternion(Quaternion value)
        {
            return "("
                   + RawFloat(value.x) + ","
                   + RawFloat(value.y) + ","
                   + RawFloat(value.z) + ","
                   + RawFloat(value.w) + ")";
        }

        private static string RawFloat(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        internal static void ValidateRepairablePartialSerializedShell()
        {
            Require(
                string.Equals(
                    ComputeSha256(ShellPrefabPath),
                    ExpectedRepairablePartialShellHash,
                    StringComparison.OrdinalIgnoreCase),
                "EXACT_POPUP_MOUNT_REPAIRABLE_PARTIAL_HASH_INVALID");
            string yaml = NormalizeYaml(File.ReadAllText(ShellPrefabPath));
            string exactAddedComponentRow =
                "    m_AddedComponents:\n"
                + "    - targetCorrespondingSourceObject: {fileID: "
                + RepairablePartialPanelSourceFileId
                + ", guid: " + ExactStandaloneGuid + ", type: 3}\n"
                + "      insertIndex: -1\n"
                + "      addedObject: {fileID: "
                + RepairablePartialAddedObjectFileId + "}";
            string missingPayload =
                "  m_Script: {fileID: 0}\n"
                + "  m_Name: \n"
                + "  m_EditorClassIdentifier: \n"
                + "  presenter: {fileID: 1395548109123010221}\n"
                + "  panelView: {fileID: 8853304749544962635}";
            Require(
                CountToken(
                    yaml,
                    "m_SourcePrefab: {fileID: 100100000, guid: "
                    + ExactStandaloneGuid) == 1
                && CountToken(yaml, LegacyPanelGuid) == 0,
                "EXACT_POPUP_MOUNT_REPAIRABLE_PARTIAL_SOURCE_INVALID");
            Require(
                CountToken(yaml, exactAddedComponentRow) == 1
                && CountToken(
                    yaml,
                    "targetCorrespondingSourceObject:") == 1
                && CountToken(yaml, "      addedObject: {fileID:") == 1
                && CountToken(
                    yaml,
                    "      addedObject: {fileID: "
                    + RepairablePartialAddedObjectFileId + "}") == 1,
                "EXACT_POPUP_MOUNT_REPAIRABLE_PARTIAL_ADDED_ROW_INVALID");
            Require(
                CountToken(
                    yaml,
                    "--- !u!114 &"
                    + RepairablePartialAddedObjectFileId) == 1
                && CountToken(yaml, "  m_Script: {fileID: 0}") == 1
                && CountToken(yaml, missingPayload) == 1,
                "EXACT_POPUP_MOUNT_REPAIRABLE_PARTIAL_PAYLOAD_INVALID");
            Require(
                CountToken(yaml, "carrierRoot: {fileID: 0}") == 0
                && CountToken(yaml, "carrierRoot: {fileID:") == 1
                && CountToken(
                    yaml,
                    "  m_Alpha: 0\n"
                    + "  m_Interactable: 0\n"
                    + "  m_BlocksRaycasts: 0\n"
                    + "  m_IgnoreParentGroups: 0") == 1,
                "EXACT_POPUP_MOUNT_REPAIRABLE_PARTIAL_BINDING_OR_CLOSED_"
                + "STATE_INVALID");
        }

        internal static void ValidateDrivenOverrideRepairableSerializedShell()
        {
            Require(
                string.Equals(
                    ComputeSha256(ShellPrefabPath),
                    ExpectedDrivenOverrideRepairableShellHash,
                    StringComparison.OrdinalIgnoreCase),
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REPAIR_HASH_INVALID");
            string yaml = NormalizeYaml(File.ReadAllText(ShellPrefabPath));
            ValidateDrivenOverrideRepairRowsInYaml(yaml);
            Require(
                CountToken(yaml, "m_SourcePrefab: {fileID: 100100000, guid: "
                                 + ExactStandaloneGuid) == 1
                && CountToken(yaml, LegacyPanelGuid) == 0
                && CountToken(yaml, "  m_Script: {fileID: 0}") == 0
                && CountToken(yaml, RepairablePartialAddedObjectFileId) == 0
                && CountToken(
                    yaml,
                    "targetCorrespondingSourceObject: {fileID: "
                    + RepairablePartialPanelSourceFileId
                    + ", guid: " + ExactStandaloneGuid) == 0
                && CountToken(yaml, "carrierRoot: {fileID: 0}") == 0
                && CountToken(yaml, "carrierRoot: {fileID:") == 1,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REPAIR_SERIALIZED_STATE_"
                + "INVALID");
        }

        internal static void ValidateDrivenOverrideRepairRowsInYaml(
            string yaml)
        {
            string normalized = NormalizeYaml(yaml);
            Require(
                CountSerializedModificationTargetRowsInYaml(
                    normalized,
                    ExactStandaloneGuid,
                    DrivenOverrideHandleSourceFileId)
                == DrivenOverrideRepairPropertyPaths.Length,
                "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REPAIR_ROW_COUNT_INVALID");
            foreach (string propertyPath in DrivenOverrideRepairPropertyPaths)
            {
                string exactRow = "    - target: {fileID: "
                                  + DrivenOverrideHandleSourceFileId.ToString(
                                      CultureInfo.InvariantCulture)
                                  + ", guid: " + ExactStandaloneGuid
                                  + ", type: 3}\n"
                                  + "      propertyPath: " + propertyPath
                                  + "\n"
                                  + "      value: 0\n"
                                  + "      objectReference: {fileID: 0}";
                Require(
                    CountToken(normalized, exactRow) == 1,
                    "EXACT_POPUP_MOUNT_DRIVEN_OVERRIDE_REPAIR_ROW_INVALID "
                    + propertyPath);
            }
        }

        internal static int CountSerializedModificationTargetRowsInYaml(
            string yaml,
            string sourceGuid,
            long sourceLocalFileId)
        {
            Require(
                !string.IsNullOrWhiteSpace(sourceGuid)
                && sourceLocalFileId != 0,
                "EXACT_POPUP_MOUNT_SERIALIZED_RECT_SOURCE_IDENTITY_INVALID");
            string exactTarget = "- target: {fileID: "
                                 + sourceLocalFileId.ToString(
                                     CultureInfo.InvariantCulture)
                                 + ", guid: " + sourceGuid + ", type: 3}";
            return NormalizeYaml(yaml).Split('\n')
                .Count(line => string.Equals(
                    line.Trim(),
                    exactTarget,
                    StringComparison.Ordinal));
        }

        internal static void ValidateNoDrivenOverrideResidueInYaml(
            string yaml)
        {
            Require(
                CountSerializedModificationTargetRowsInYaml(
                    yaml,
                    ExactStandaloneGuid,
                    DrivenOverrideHandleSourceFileId) == 0,
                "EXACT_POPUP_MOUNT_SERIALIZED_DRIVEN_OVERRIDE_RESIDUE");
        }

        private static void ValidateSerializedShell()
        {
            string yaml = NormalizeYaml(File.ReadAllText(ShellPrefabPath));
            Require(
                CountToken(yaml, "m_SourcePrefab: {fileID: 100100000, guid: "
                                 + ExactStandaloneGuid) == 1,
                "EXACT_POPUP_MOUNT_SERIALIZED_EXACT_SOURCE_COUNT_INVALID");
            Require(
                CountToken(yaml, LegacyPanelGuid) == 0,
                "EXACT_POPUP_MOUNT_SERIALIZED_LEGACY_SOURCE_REMAINS");
            Require(
                CountToken(yaml, "carrierRoot: {fileID: 0}") == 0
                && CountToken(yaml, "carrierRoot: {fileID:") == 1,
                "EXACT_POPUP_MOUNT_SERIALIZED_CARRIER_REFERENCE_INVALID");
            Require(
                CountToken(
                    yaml,
                    RepairablePartialAddedObjectFileId) == 0
                && CountToken(
                    yaml,
                    "targetCorrespondingSourceObject: {fileID: "
                    + RepairablePartialPanelSourceFileId
                    + ", guid: " + ExactStandaloneGuid) == 0
                && CountToken(yaml, "  m_Script: {fileID: 0}") == 0,
                "EXACT_POPUP_MOUNT_SERIALIZED_ADDED_COMPONENT_RESIDUE");
            int knownHandleRows = CountSerializedModificationTargetRowsInYaml(
                yaml,
                ExactStandaloneGuid,
                DrivenOverrideHandleSourceFileId);
            if (knownHandleRows
                == DrivenOverrideRepairPropertyPaths.Length)
            {
                ValidateEmbeddedRootKnownHandleResidueRowsInYaml(yaml);
            }
            else
            {
                Require(
                    knownHandleRows == 0,
                    "EXACT_POPUP_MOUNT_SERIALIZED_HANDLE_ROW_COUNT_INVALID "
                    + knownHandleRows.ToString(CultureInfo.InvariantCulture));
                ValidateNoDrivenOverrideResidueInYaml(yaml);
                ValidateEmbeddedRootRowsInYaml(yaml);
            }
        }

        private static string NormalizeYaml(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", "\n");
        }

        private static void ValidateProtectedInputs()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                Require(
                    File.Exists(pair.Key),
                    "EXACT_POPUP_MOUNT_PROTECTED_FILE_MISSING " + pair.Key);
                Require(
                    string.Equals(
                        ComputeSha256(pair.Key),
                        pair.Value,
                        StringComparison.OrdinalIgnoreCase),
                    "EXACT_POPUP_MOUNT_PROTECTED_HASH_CHANGED " + pair.Key);
            }

            Require(
                string.Equals(
                    AssetDatabase.AssetPathToGUID(ExactStandalonePrefabPath),
                    ExactStandaloneGuid,
                    StringComparison.Ordinal),
                "EXACT_POPUP_MOUNT_STANDALONE_GUID_CHANGED");
            Require(
                string.Equals(
                    AssetDatabase.AssetPathToGUID(LegacyPanelPrefabPath),
                    LegacyPanelGuid,
                    StringComparison.Ordinal),
                "EXACT_POPUP_MOUNT_LEGACY_GUID_CHANGED");
        }

        private static int CountNestedPrefabRoots(
            GameObject root,
            string assetPath)
        {
            HashSet<GameObject> roots = new HashSet<GameObject>();
            foreach (Transform transform in root.GetComponentsInChildren<
                         Transform>(true))
            {
                GameObject instanceRoot =
                    PrefabUtility.GetNearestPrefabInstanceRoot(
                        transform.gameObject);
                if (instanceRoot != null
                    && string.Equals(
                        ResolveNearestPrefabAssetPath(instanceRoot),
                        assetPath,
                        StringComparison.Ordinal))
                {
                    roots.Add(instanceRoot);
                }
            }

            return roots.Count;
        }

        private static string ResolveNearestPrefabAssetPath(GameObject target)
        {
            return (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target)
                    ?? string.Empty).Replace('\\', '/');
        }

        private static bool IsClosedCanvasGroup(CanvasGroup canvasGroup)
        {
            return canvasGroup != null
                   && Mathf.Approximately(canvasGroup.alpha, 0f)
                   && !canvasGroup.interactable
                   && !canvasGroup.blocksRaycasts
                   && !canvasGroup.ignoreParentGroups;
        }

        private static void ConfigureClosedCanvasGroup(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.ignoreParentGroups = false;
        }

        private static Transform RequiredDirectChild(
            Transform parent,
            string name)
        {
            List<Transform> matches = new List<Transform>();
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (string.Equals(child.name, name, StringComparison.Ordinal))
                {
                    matches.Add(child);
                }
            }

            Require(
                matches.Count == 1,
                "EXACT_POPUP_MOUNT_DIRECT_CHILD_INVALID " + name
                + "=" + matches.Count);
            return matches[0];
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
                "EXACT_POPUP_MOUNT_COMPONENT_COUNT_INVALID "
                + typeof(T).FullName + "=" + matches.Length);
            return matches[0];
        }

        private static T Single<T>(GameObject root)
            where T : Component
        {
            T[] matches = root.GetComponentsInChildren<T>(true);
            Require(
                matches.Length == 1,
                "EXACT_POPUP_MOUNT_HIERARCHY_COUNT_INVALID "
                + typeof(T).FullName + "=" + matches.Length);
            return matches[0];
        }

        private static void ValidateNoMissingScripts(GameObject root)
        {
            int missing = CountMissingScripts(root);
            Require(
                missing == 0,
                "EXACT_POPUP_MOUNT_MISSING_SCRIPT_COUNT " + missing);
        }

        internal static int CountMissingScripts(GameObject root)
        {
            Require(
                root != null,
                "EXACT_POPUP_MOUNT_MISSING_SCRIPT_ROOT_NULL");
            return root.GetComponentsInChildren<Transform>(true)
                .Sum(transform =>
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transform.gameObject));
        }

        private static void ValidateNoForbiddenOwners(GameObject root)
        {
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<
                         MonoBehaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                string typeName = behaviour.GetType().FullName ?? string.Empty;
                Require(
                    !typeName.Contains("BuildSandbox")
                    && !typeName.Contains("ItemSandbox")
                    && !typeName.Contains("BuildGridInteractionPreview"),
                    "EXACT_POPUP_MOUNT_FORBIDDEN_OWNER " + typeName);
            }
        }

        private static string RelativePath(
            Transform root,
            Transform target)
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

            Require(
                current == root,
                "EXACT_POPUP_MOUNT_REFERENCE_OUTSIDE_CARRIER");
            return "$/" + string.Join("/", names);
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
                return guid + ":"
                       + localId.ToString(CultureInfo.InvariantCulture);
            }

            return value.GetType().FullName + ":" + value.name;
        }

        private static int CountToken(string value, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = value.IndexOf(
                       token,
                       index,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }

        private static bool IsFinitePositive(float value)
        {
            return value > 0f
                   && !float.IsNaN(value)
                   && !float.IsInfinity(value);
        }

        private static bool Approximately(Vector2 left, Vector2 right)
        {
            return IsFinite(left.x)
                   && IsFinite(left.y)
                   && Mathf.Abs(left.x - right.x) <= RectValueTolerance
                   && Mathf.Abs(left.y - right.y) <= RectValueTolerance;
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return IsFinite(left.x)
                   && IsFinite(left.y)
                   && IsFinite(left.z)
                   && Mathf.Abs(left.x - right.x) <= RectValueTolerance
                   && Mathf.Abs(left.y - right.y) <= RectValueTolerance
                   && Mathf.Abs(left.z - right.z) <= RectValueTolerance;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static string ComputeSha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(File.ReadAllBytes(path));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
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
