using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.UnifiedBattle.Presentation.ExactItemDetail;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class
        C1UnifiedExactBattleSandboxItemDetailPopupMountTests
    {
        public const string BeforeTerminalMarker =
            "C1_UNIFIED_EXACT_ITEM_DETAIL_POPUP_MOUNT_PREAUTHOR_TESTS_PASS";
        public const string AfterTerminalMarker =
            "C1_UNIFIED_EXACT_ITEM_DETAIL_POPUP_MOUNT_POSTAUTHOR_TESTS_PASS";

        private static readonly MethodInfo RenderMethod =
            typeof(C1ExactBattleSandboxItemDetailPresenter).GetMethod(
                "Render",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static int assertionCount;

        public static void ExecuteFromCommandLine()
        {
            try
            {
                RunBeforeAuthoringOrThrow();
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState state =
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .InspectCurrentShell(out _);
                if (state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.FinalClean
                    || state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState
                        .FinalCleanKnownScrollbarDriverBaseline)
                {
                    RunAfterAuthoringOrThrow();
                }

                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[ExactPopupMountTests] FOCUSED_TESTS_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void RunBeforeAuthoringOrThrow()
        {
            assertionCount = 0;
            VerifyStaticBoundaries();
            VerifyAuthoredRectParityNumericContract();
            VerifyDriverControlledRectParityContract();
            VerifyDrivenOverrideMetadataValidationContract();
            VerifyRootNameMetadataOverrideContract();
            VerifyAuthoredStructureRejectsAddedComponents();
            VerifyCarrierDescendantReferenceContract();
            VerifyFormalProjectionOpenCloseAndIdempotence();
            VerifyFontFailureClosesAndReleasesInput();
            VerifyInvalidLayoutClosesAndReleasesInput();
            VerifyCurrentShellIsSupportedState();
            VerifyEmbeddedRootKnownHandleBaselineAndIdempotence();
            VerifyHashBoundRepairableRejectsStructuralDrift();
            VerifyInvalidConflictClassificationIsFailClosed();
            Debug.Log(
                "[ExactPopupMountTests] " + BeforeTerminalMarker
                + " assertions="
                + assertionCount.ToString(CultureInfo.InvariantCulture));
        }

        public static int RunRectParityNumericContractOfflineOrThrow()
        {
            assertionCount = 0;
            VerifyAuthoredRectParityNumericContract();
            VerifyDriverControlledRectParityContract();
            return assertionCount;
        }

        public static void RunAfterAuthoringOrThrow()
        {
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ShellMountState state =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .InspectCurrentShell(out string diagnostic);
            Require(
                state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.FinalClean
                || state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState
                    .FinalCleanKnownScrollbarDriverBaseline,
                "POSTAUTHOR_FINAL_STATE_INVALID " + diagnostic);

            GameObject root = PrefabUtility.LoadPrefabContents(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            try
            {
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateFinalLoadedShell(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            VerifySerializedFinalCarrier();
            Debug.Log(
                "[ExactPopupMountTests] " + AfterTerminalMarker
                + " assertions="
                + assertionCount.ToString(CultureInfo.InvariantCulture));
        }

        private static void VerifyStaticBoundaries()
        {
            string presenter = ReadRequiredSource(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .PresenterSourcePath);
            foreach (string required in new[]
                     {
                         "FormalItemDetailChanged",
                         "CurrentFormalItemDetail",
                         "CreateViewModelClone()",
                         "result.projection.artwork",
                         "carrierRoot",
                         "panelView.transform.IsChildOf(carrierRoot)",
                         "popupCanvasGroup.alpha = visible ? 1f : 0f",
                         "popupCanvasGroup.interactable = visible",
                         "popupCanvasGroup.blocksRaycasts = visible",
                         "HasValidEmbeddedCarrierMount()",
                         "RuntimeBindingIdentityMarker",
                         "scenePath=",
                         "rootCanvas=",
                         "renderMode=",
                         "panelView.SetExternalCloseRequest(RequestClose)",
                         "modalBackdropButton.onClick.AddListener(",
                         "ValidateFormalProjection(",
                         "ValidateVisibleFormalBinding(",
                         "modalBackdropImage.enabled = visible",
                         "panelView.Bind(null)",
                         "panelView.Close()"
                     })
            {
                Require(
                    presenter.Contains(required),
                    "PRESENTER_REQUIRED_BOUNDARY_MISSING " + required);
            }

            string panelView = ReadRequiredSource(
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/"
                + "ItemDetailPanelView.cs");
            foreach (string required in new[]
                     {
                         "SetExternalCloseRequest(",
                         "closeButton.onClick.AddListener(RequestClose)",
                         "ownerPanel.RequestClose()",
                         "IsFormalRuntimeDataCarrier(",
                         "ValidateVisibleFormalBinding("
                     })
            {
                Require(
                    panelView.Contains(required),
                    "PANEL_FORMAL_LIFECYCLE_BOUNDARY_MISSING " + required);
            }

            foreach (string forbidden in new[]
                     {
                         "TalismanBag.BuildSandbox",
                         "TalismanBag.ItemSandbox",
                         "GameObject.Find",
                         "FindObjectOfType",
                         "FindObjectsOfType",
                         "Resources.Load",
                         "new GameObject(",
                         "SampleData"
                     })
            {
                Require(
                    !presenter.Contains(forbidden),
                    "PRESENTER_FORBIDDEN_BOUNDARY " + forbidden);
            }

            string authoring = ReadRequiredSource(
                "Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/"
                + "C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring.cs");
            string authoringWithoutWhitespace = string.Concat(
                authoring.Where(character => !char.IsWhiteSpace(character)));
            foreach (string forbidden in new[]
                     {
                         "EditorSceneManager",
                         "OpenScene(",
                         "SaveScene(",
                         "ApplyPrefabInstance",
                         "UnpackPrefabInstance",
                         "GameObject.Find",
                         "TalismanBag.BuildSandbox",
                         "TalismanBag.ItemSandbox"
                     })
            {
                Require(
                    !authoring.Contains(forbidden),
                    "AUTHORING_FORBIDDEN_BOUNDARY " + forbidden);
            }

            Require(
                CountToken(authoring, "SaveAsPrefabAsset(") == 1
                && authoring.Contains(
                    "SaveAsPrefabAsset(\n                        root,\n"
                    + "                        ShellPrefabPath)"),
                "AUTHORING_SAVE_TARGET_NOT_EXACT_SHELL_ONLY");

            foreach (string requiredParityContract in new[]
                     {
                         "RectValueTolerance = 0.0001f;",
                         "RectRotationToleranceDegrees = 0.001f;",
                         "ValidateAuthoredHierarchyAndGeometry(",
                         "ValidateAuthoredRectGeometry(",
                         "Quaternion.Angle(",
                         "delta <= RectValueTolerance",
                         "angleDelta <= RectRotationToleranceDegrees",
                         "EXACT_POPUP_MOUNT_AUTHORED_PARITY_FAIL",
                         "path=",
                         "field=",
                         "source=",
                         "mounted=",
                         "delta=",
                         "mountedRect.drivenByObject != null",
                         "PrefabUtility.GetCorrespondingObjectFromSource(",
                         "PrefabUtility.GetPropertyModifications(",
                         "AssetDatabase.TryGetGUIDAndLocalFileIdentifier(",
                         "FindFirstSerializedRectOverrideInYaml(",
                         "sourceLocalFileId.ToString(",
                         "string exactTarget = \"- target: {fileID: \"",
                         "ValidateRootNameMetadataOverrideContract(",
                         "ValidateExactRootCanvasComponentContract(",
                         "RootCanvasRectContextPropertyPaths.Contains(",
                         "seenRootCanvasRectContextPaths.Add(propertyPath)",
                         "float.TryParse(",
                         "\"m_LocalEulerAnglesHint.x\"",
                         "\"m_LocalEulerAnglesHint.y\"",
                         "\"m_LocalEulerAnglesHint.z\"",
                         "modification.target == exactSourceRoot",
                         "modification.target == exactSourceRootRect",
                         "modification.propertyPath,\n"
                         + "                        \"m_Name\"",
                         "modification.objectReference == null",
                         "modification.value,\n"
                         + "                            exactSourceRoot.name",
                         "mountedRoot.name,\n"
                         + "                            exactSourceRoot.name",
                         "rootNameOverrideCount == 1",
                         "EXACT_POPUP_MOUNT_DRIVEN_RECT_SERIALIZED_OVERRIDE",
                         "EXACT_POPUP_MOUNT_DRIVEN_RECT_SOURCE_"
                         + "CORRESPONDENCE_INVALID",
                         "EXACT_POPUP_MOUNT_DRIVEN_RECT_COMPONENT_SOURCE_INVALID",
                         "ShellMountState.FinalClean",
                         "CountMissingScripts(root) == 0",
                         "EXACT_POPUP_MOUNT_SERIALIZED_ADDED_COMPONENT_RESIDUE",
                         "ValidateNoDrivenOverrideResidueInYaml(",
                         "EXACT_POPUP_MOUNT_SERIALIZED_DRIVEN_OVERRIDE_RESIDUE",
                         "ExpectedZeroRootMountRepairableShellHash",
                         "ShellMountState.ExactZeroRootMountRepairable",
                         "ApplyEmbeddedCarrierRootMountAdapter(root)",
                         "ValidateExactZeroRootMountRepairableSerializedShell()",
                         "ValidateExactZeroRootMountRowsInYaml(yaml)",
                         "IsExpectedZeroRootMountRepairableHash(",
                         "ValidateExactCarrierPreflight(\n"
                         + "                        root,\n"
                         + "                        presenter,\n"
                         + "                        carrierRoot)",
                         "ValidateEmbeddedRootRowsInYaml(yaml)",
                         "IsEmbeddedCarrierRootGeometryValid(carrier)",
                         "ExactStandaloneRootRectSourceFileId",
                         "ExpectedEmbeddedRootKnownHandleResidueShellHash",
                         "FinalCleanKnownScrollbarDriverBaseline",
                         "ValidateEmbeddedRootKnownHandleResidueSerializedShell()",
                         "ValidateEmbeddedRootKnownHandleResidueRowsInYaml(",
                         "ValidateEmbeddedRootKnownHandleResidueLoadedShell(",
                         "ShellMountState.ExactVisibleLifecycleRepairable",
                         "ApplyVisibleDataArtworkModalLifecycle(root)",
                         "ModalBackdropName",
                         "ValidateModalBackdropContract(",
                         "FindFirstLoadedRectOverride("
                      })
            {
                Require(
                    authoring.Contains(requiredParityContract),
                    "AUTHORING_PARITY_CONTRACT_MISSING "
                    + requiredParityContract);
            }

            Require(
                !authoring.Contains("BuildAuthoredHierarchySignature")
                && !authoring.Contains("VerticalScrollbar")
                && !authoring.Contains("0.4915253")
                && !authoring.Contains(
                    "AddComponent<\n"
                    + "                    C1ExactBattleSandboxItemDetailCloseRelay")
                && !authoring.Contains("relay.AssignForEditor")
                && !authoring.Contains(
                    "relays[0].ValidateAuthoredReferences")
                 && !authoring.Contains("ownsAuthoredPanel")
                 && !authoring.Contains("mountedModifications")
                 && !authoring.Contains("RevertObjectOverride(")
                && !authoring.Contains(
                    "mountedProperty.floatValue.Equals(\n"
                    + "                        sourceProperty.floatValue)")
                && CountToken(authoring, "isContextDrivenRootCanvas") == 2,
                "AUTHORING_PARITY_SCOPE_OR_DERIVED_VALUE_INVALID");

            int backdropValidatorStart = authoringWithoutWhitespace.IndexOf(
                "privatestaticvoidValidateModalBackdropContract(",
                StringComparison.Ordinal);
            int backdropValidatorEnd = backdropValidatorStart < 0
                ? -1
                : authoringWithoutWhitespace.IndexOf(
                    "internalstaticboolIsEmbeddedCarrierRootGeometryValid(",
                    backdropValidatorStart,
                    StringComparison.Ordinal);
            int backdropWriterStart = authoringWithoutWhitespace.IndexOf(
                "internalstaticvoidApplyVisibleDataArtworkModalLifecycle(",
                StringComparison.Ordinal);
            int backdropWriterEnd = backdropWriterStart < 0
                ? -1
                : authoringWithoutWhitespace.IndexOf(
                    "privatestaticvoid"
                    + "ValidateLegacyPresenterReferencesWithoutBackdrop(",
                    backdropWriterStart,
                    StringComparison.Ordinal);
            int carrierWriterStart = authoringWithoutWhitespace.IndexOf(
                "privatestaticvoidWriteAndRecordEmbeddedCarrierRoot(",
                StringComparison.Ordinal);
            int carrierWriterEnd = carrierWriterStart < 0
                ? -1
                : authoringWithoutWhitespace.IndexOf(
                    "privatestaticvoidValidateExactCarrierPreflight(",
                    carrierWriterStart,
                    StringComparison.Ordinal);
            Require(
                backdropValidatorStart >= 0
                && backdropValidatorEnd > backdropValidatorStart
                && backdropWriterStart >= 0
                && backdropWriterEnd > backdropWriterStart
                && carrierWriterStart >= 0
                && carrierWriterEnd > carrierWriterStart,
                "BACKDROP_GEOMETRY_METHOD_SCOPE_MISSING");
            string backdropValidatorScope =
                authoringWithoutWhitespace.Substring(
                    backdropValidatorStart,
                    backdropValidatorEnd - backdropValidatorStart);
            string backdropWriterScope = authoringWithoutWhitespace.Substring(
                backdropWriterStart,
                backdropWriterEnd - backdropWriterStart);
            string carrierWriterScope = authoringWithoutWhitespace.Substring(
                carrierWriterStart,
                carrierWriterEnd - carrierWriterStart);
            int carrierPreflightStart = authoringWithoutWhitespace.IndexOf(
                "privatestaticvoidValidateExactCarrierPreflight(",
                StringComparison.Ordinal);
            int carrierPreflightEnd = carrierPreflightStart < 0
                ? -1
                : authoringWithoutWhitespace.IndexOf(
                    "privatestaticvoidValidateModalBackdropContract(",
                    carrierPreflightStart,
                    StringComparison.Ordinal);
            Require(
                carrierPreflightStart >= 0
                && carrierPreflightEnd > carrierPreflightStart,
                "KNOWN_HANDLE_PREFLIGHT_SCOPE_MISSING");
            string carrierPreflightScope = authoringWithoutWhitespace.Substring(
                carrierPreflightStart,
                carrierPreflightEnd - carrierPreflightStart);
            string outsideExactGeometryWriters = authoringWithoutWhitespace
                .Replace(backdropWriterScope, string.Empty)
                .Replace(carrierWriterScope, string.Empty);
            Require(
                backdropValidatorScope.Contains(
                    "Approximately(rect.localPosition,Vector3.zero)")
                && !backdropValidatorScope.Contains("rect.localPosition=")
                && backdropWriterScope.Contains(
                    "newGameObject(ModalBackdropName,")
                && backdropWriterScope.Contains(
                    "backdrop.SetParent(presenter.transform,false)")
                && backdropWriterScope.Contains(
                    "backdrop.localPosition=Vector3.zero;")
                && !backdropWriterScope.Contains("carrier.localPosition=")
                && carrierWriterScope.Contains(
                    "carrier.localPosition=Vector3.zero;")
                && carrierWriterScope.Contains(
                    "RecordPrefabInstancePropertyModifications(carrier)")
                && !outsideExactGeometryWriters.Contains(".localPosition=")
                && CountToken(
                    authoringWithoutWhitespace,
                    ".localPosition=") == 2,
                "BACKDROP_GEOMETRY_SCOPE_OR_MUTATION_INVALID");
            Require(
                backdropWriterScope.Contains(
                    "ResolveExactDrivenOverrideHandle(carrier,"
                    + "outRectTransformsourceHandle,out_)")
                && backdropWriterScope.Contains(
                    "RequireExactSourceComponentIdentity(sourceHandle,"
                    + "DrivenOverrideHandleSourceFileId,")
                && backdropWriterScope.Contains(
                    "ValidateExactCarrierPreflight(root,presenter,carrier,"
                    + "sourceHandle)")
                && carrierPreflightScope.Contains(
                    "RectTransformallowedDrivenOverrideSourceRect=null")
                && carrierPreflightScope.Contains(
                    "ValidateAuthoredHierarchyAndGeometry(source.transform,"
                    + "carrier,allowedDrivenOverrideSourceRect)")
                && carrierPreflightScope.Contains(
                    "ValidateNoDisallowedPropertyOverrides(carrier,source,"
                    + "allowedDrivenOverrideSourceRect)")
                && CountToken(
                    authoringWithoutWhitespace,
                    "ValidateExactCarrierPreflight(root,presenter,carrier,"
                    + "sourceHandle)") == 1,
                "KNOWN_SCROLLBAR_PREFLIGHT_BOUNDARY_INCONSISTENT");
            Require(
                CountToken(
                    authoring,
                    "FinalCleanKnownScrollbarDriverBaseline") >= 4
                && authoringWithoutWhitespace.Contains(
                    "returnShellMountState."
                    + "FinalCleanKnownScrollbarDriverBaseline;"),
                "KNOWN_SCROLLBAR_FINAL_STATE_CONTRACT_INVALID");
            int knownHandleBranchStart = authoring.IndexOf(
                "if (IsExpectedEmbeddedRootKnownHandleResidueHash(",
                StringComparison.Ordinal);
            int knownHandleBranchEnd = knownHandleBranchStart < 0
                ? -1
                : authoring.IndexOf(
                    "catch (InvalidOperationException exception)",
                    knownHandleBranchStart,
                    StringComparison.Ordinal);
            Require(
                knownHandleBranchStart >= 0
                && knownHandleBranchEnd > knownHandleBranchStart,
                "KNOWN_HANDLE_FINAL_CLEAN_BRANCH_SCOPE_MISSING");
            string knownHandleBranchWithoutWhitespace = string.Concat(
                authoring.Substring(
                        knownHandleBranchStart,
                        knownHandleBranchEnd - knownHandleBranchStart)
                    .Where(character => !char.IsWhiteSpace(character)));
            Require(
                knownHandleBranchWithoutWhitespace.Contains(
                    "ValidateEmbeddedRootKnownHandleResidueSerializedShell();")
                && knownHandleBranchWithoutWhitespace.Contains(
                    "ValidateEmbeddedRootKnownHandleResidueLoadedShell(root);")
                && knownHandleBranchWithoutWhitespace.Contains(
                    "returnShellMountState."
                    + "FinalCleanKnownScrollbarDriverBaseline;")
                && !knownHandleBranchWithoutWhitespace.Contains(
                    "RepairEmbeddedRootKnownHandleResidue(")
                && !knownHandleBranchWithoutWhitespace.Contains(
                    "SetPropertyModifications(")
                && !knownHandleBranchWithoutWhitespace.Contains(
                    "RevertPropertyOverride("),
                "KNOWN_SCROLLBAR_FINAL_CLASSIFIER_SEMANTICS_INVALID");
            int applyStart = authoringWithoutWhitespace.IndexOf(
                "privatestaticvoidApplySinglePass()",
                StringComparison.Ordinal);
            int applyEnd = applyStart < 0
                ? -1
                : authoringWithoutWhitespace.IndexOf(
                    "internalstaticvoidApplyEmbeddedCarrierRootMountAdapter(",
                    applyStart,
                    StringComparison.Ordinal);
            Require(
                applyStart >= 0 && applyEnd > applyStart,
                "KNOWN_SCROLLBAR_APPLY_SCOPE_MISSING");
            string applyScope = authoringWithoutWhitespace.Substring(
                applyStart,
                applyEnd - applyStart);
            Require(
                applyScope.Contains(
                    "FinalCleanKnownScrollbarDriverBaseline")
                && applyScope.Contains("ValidateFinalLoadedShell(root);")
                && !applyScope.Contains(
                    "RepairEmbeddedRootKnownHandleResidue(root);")
                && !applyScope.Contains("SetPropertyModifications(")
                && !applyScope.Contains("RevertPropertyOverride(")
                && !applyScope.Contains(
                    "RecordPrefabInstancePropertyModifications("),
                "KNOWN_SCROLLBAR_FINAL_APPLY_WAS_NOT_VALIDATION_ONLY");
            Require(
                !presenter.Contains("void Update(")
                && CountToken(
                    presenter,
                    "UNIFIED_EXACT_ITEM_DETAIL_RUNTIME_BINDING_IDENTITY") == 1,
                "PRESENTER_RUNTIME_IDENTITY_LOG_SCOPE_INVALID");
            Require(
                CountToken(
                    authoring,
                    "IsExactZeroRootMountGeometry(") == 1,
                "REPAIR_PATH_STILL_DEPENDS_ON_LOADED_ZERO_GEOMETRY");
        }

        private static void VerifyAuthoredRectParityNumericContract()
        {
            const string path = "$/NumericParity[0]";
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry source = CreateRectGeometry(
                    Vector2.zero,
                    Quaternion.identity);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry threshold = CreateRectGeometry(
                    new Vector2(0.0001f, -0.0001f),
                    Quaternion.identity);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateAuthoredRectGeometry(path, source, threshold);
            Require(true, "RECT_VALUE_EXACT_THRESHOLD_REJECTED");

            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry oppositeQuaternionSign =
                    CreateRectGeometry(
                        Vector2.zero,
                        new Quaternion(0f, 0f, 0f, -1f));
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateAuthoredRectGeometry(
                    path,
                    source,
                    oppositeQuaternionSign);
            Require(true, "RECT_QUATERNION_SIGN_EQUIVALENCE_REJECTED");

            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry overThreshold = CreateRectGeometry(
                    new Vector2(0.0002f, 0.0003f),
                    Quaternion.identity);
            string firstFieldFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateAuthoredRectGeometry(
                        path,
                        source,
                        overThreshold));
            Require(
                string.Equals(
                    firstFieldFailure,
                    "EXACT_POPUP_MOUNT_AUTHORED_PARITY_FAIL"
                    + " path=$/NumericParity[0]"
                    + " field=anchorMin.x"
                    + " source=0"
                    + " mounted=0.0002"
                    + " delta=0.0002"
                    + " tolerance=0.0001",
                    StringComparison.Ordinal),
                "RECT_FIRST_FIELD_DIAGNOSTIC_NOT_EXACT "
                + firstFieldFailure);

            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry overRotation = CreateRectGeometry(
                    Vector2.zero,
                    Quaternion.AngleAxis(1f, Vector3.forward));
            string rotationFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateAuthoredRectGeometry(
                        path,
                        source,
                        overRotation));
            Require(
                rotationFailure.Contains(" field=localRotation ")
                && rotationFailure.Contains(" delta=")
                && rotationFailure.EndsWith(
                    " toleranceDegrees=0.001",
                    StringComparison.Ordinal),
                "RECT_ROTATION_THRESHOLD_OR_DIAGNOSTIC_INVALID "
                + rotationFailure);
        }

        private static void VerifyDriverControlledRectParityContract()
        {
            const string path = "$/DrivenRect[0]";
            const long exactHandleFileId = 1131740001722038517L;
            const string otherGuid =
                "11111111111111111111111111111111";
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry source = CreateRectGeometry(
                    Vector2.zero,
                    Quaternion.identity);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry transientDrift = CreateRectGeometry(
                    new Vector2(0.75f, 0.5f),
                    Quaternion.AngleAxis(45f, Vector3.forward));

            string transientOnlyYaml =
                "PrefabInstance:\n"
                + "  m_Modification:\n"
                + "    m_Modifications:\n"
                + "    - target: {fileID: 8610391659744506857, guid: "
                + otherGuid + ", type: 3}\n"
                + "      propertyPath: m_AnchorMax.x\n"
                + "      value: 0.5\n"
                + "      objectReference: {fileID: 0}\n"
                + "    m_RemovedComponents: []";
            string transientSerializedOverride =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .FindFirstSerializedRectOverrideInYaml(
                        transientOnlyYaml,
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExactStandaloneGuid,
                        exactHandleFileId);
            Require(
                transientSerializedOverride == null,
                "DRIVEN_TRANSIENT_STATE_FALSE_MATCHED_SHELL_YAML "
                + transientSerializedOverride);

            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateRectParityWithDriverState(
                    path,
                    source,
                    transientDrift,
                    true,
                    true,
                    transientSerializedOverride,
                    null);
            Require(true, "DRIVEN_TRANSIENT_RECT_DRIFT_REJECTED");

            string exactOverrideYaml =
                "PrefabInstance:\n"
                + "  m_Modification:\n"
                + "    m_Modifications:\n"
                + "    - target: {fileID: " + exactHandleFileId
                + ", guid: "
                + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ExactStandaloneGuid
                + ", type: 3}\n"
                + "      propertyPath: m_AnchorMax.x\n"
                + "      value: 0.5\n"
                + "      objectReference: {fileID: 0}\n"
                + "    m_RemovedComponents: []";
            string exactSerializedOverride =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .FindFirstSerializedRectOverrideInYaml(
                        exactOverrideYaml,
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExactStandaloneGuid,
                        exactHandleFileId);
            Require(
                string.Equals(
                    exactSerializedOverride,
                    "m_AnchorMax.x",
                    StringComparison.Ordinal),
                "EXACT_DRIVEN_RECT_YAML_OVERRIDE_NOT_FOUND "
                + exactSerializedOverride);

            string otherIdentityYaml = exactOverrideYaml.Replace(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ExactStandaloneGuid,
                otherGuid);
            Require(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .FindFirstSerializedRectOverrideInYaml(
                        otherIdentityYaml,
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExactStandaloneGuid,
                        exactHandleFileId) == null,
                "OTHER_GUID_DRIVEN_RECT_OVERRIDE_FALSE_MATCHED");
            otherIdentityYaml = exactOverrideYaml.Replace(
                exactHandleFileId.ToString(CultureInfo.InvariantCulture),
                "8610391659744506857");
            Require(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .FindFirstSerializedRectOverrideInYaml(
                        otherIdentityYaml,
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExactStandaloneGuid,
                        exactHandleFileId) == null,
                "OTHER_FILE_ID_DRIVEN_RECT_OVERRIDE_FALSE_MATCHED");

            string overrideFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateRectParityWithDriverState(
                        path,
                        source,
                        transientDrift,
                        true,
                        true,
                        exactSerializedOverride,
                        null));
            Require(
                string.Equals(
                    overrideFailure,
                    "EXACT_POPUP_MOUNT_DRIVEN_RECT_SERIALIZED_OVERRIDE"
                    + " path=$/DrivenRect[0]"
                    + " property=m_AnchorMax.x",
                    StringComparison.Ordinal),
                "DRIVEN_SERIALIZED_OVERRIDE_DIAGNOSTIC_INVALID "
                + overrideFailure);

            string sourceFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateRectParityWithDriverState(
                        path,
                        source,
                        transientDrift,
                        true,
                        false,
                        null,
                        null));
            Require(
                string.Equals(
                    sourceFailure,
                    "EXACT_POPUP_MOUNT_DRIVEN_RECT_SOURCE_"
                    + "CORRESPONDENCE_INVALID path=$/DrivenRect[0]",
                    StringComparison.Ordinal),
                "DRIVEN_SOURCE_CORRESPONDENCE_DIAGNOSTIC_INVALID "
                + sourceFailure);

            string componentFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateRectParityWithDriverState(
                        path,
                        source,
                        transientDrift,
                        true,
                        true,
                        null,
                        "component[2]=UnityEngine.UI.Image"));
            Require(
                string.Equals(
                    componentFailure,
                    "EXACT_POPUP_MOUNT_DRIVEN_RECT_COMPONENT_SOURCE_INVALID"
                    + " path=$/DrivenRect[0]"
                    + " component=component[2]=UnityEngine.UI.Image",
                    StringComparison.Ordinal),
                "DRIVEN_COMPONENT_SOURCE_DIAGNOSTIC_INVALID "
                + componentFailure);

            string nonDrivenFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateRectParityWithDriverState(
                        path,
                        source,
                        transientDrift,
                        false,
                        false,
                        "ignored-for-non-driven",
                        "ignored-for-non-driven"));
            Require(
                nonDrivenFailure.Contains(" field=anchorMin.x ")
                && nonDrivenFailure.EndsWith(
                    " tolerance=0.0001",
                    StringComparison.Ordinal),
                "NON_DRIVEN_RECT_DRIFT_WAS_NOT_REJECTED "
                + nonDrivenFailure);
        }

        private static void VerifyDrivenOverrideMetadataValidationContract()
        {
            const string propertyPath = "m_AnchorMax.x";
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateDrivenOverridePropertyPreflight(
                    propertyPath,
                    true,
                    SerializedPropertyType.Float,
                    0f,
                    true,
                    SerializedPropertyType.Float,
                    true);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateDrivenOverridePropertyAfterRevert(
                    propertyPath,
                    true,
                    SerializedPropertyType.Float,
                    false,
                    1f);
            Require(
                true,
                "DRIVEN_LIVE_GETTER_DIFFERENCE_REJECTED_AFTER_METADATA_REVERT");

            string missingFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverridePropertyPreflight(
                        propertyPath,
                        false,
                        SerializedPropertyType.Float,
                        0f,
                        true,
                        SerializedPropertyType.Float,
                        true));
            Require(
                missingFailure.Contains("PROPERTY_MISSING"),
                "DRIVEN_OVERRIDE_MISSING_PROPERTY_NOT_REJECTED "
                + missingFailure);
            string typeFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverridePropertyPreflight(
                        propertyPath,
                        true,
                        SerializedPropertyType.Integer,
                        0f,
                        true,
                        SerializedPropertyType.Float,
                        true));
            Require(
                typeFailure.Contains("PROPERTY_TYPE_INVALID"),
                "DRIVEN_OVERRIDE_PROPERTY_TYPE_NOT_REJECTED " + typeFailure);
            string notOverrideFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverridePropertyPreflight(
                        propertyPath,
                        true,
                        SerializedPropertyType.Float,
                        0f,
                        true,
                        SerializedPropertyType.Float,
                        false));
            Require(
                notOverrideFailure.Contains("NOT_PRESENT"),
                "DRIVEN_OVERRIDE_METADATA_ABSENCE_NOT_REJECTED "
                + notOverrideFailure);
            string baselineFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverridePropertyPreflight(
                        propertyPath,
                        true,
                        SerializedPropertyType.Float,
                        0.25f,
                        true,
                        SerializedPropertyType.Float,
                        true));
            Require(
                baselineFailure.Contains("SOURCE_BASELINE_INVALID")
                && baselineFailure.EndsWith(
                    " value=0.25",
                    StringComparison.Ordinal),
                "DRIVEN_OVERRIDE_SOURCE_BASELINE_DRIFT_NOT_REJECTED "
                + baselineFailure);

            RequireSemanticInvalidOperationRejected(
                () => ValidateSerializedHandleRowsForAcceptedState(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.FinalClean,
                    ExactDrivenOverrideRow(propertyPath)),
                "ORDINARY_FINAL_DRIVEN_OVERRIDE_YAML_RESIDUE");
        }

        private static void VerifyAuthoredStructureRejectsAddedComponents()
        {
            GameObject sourceRoot = null;
            GameObject mountedRoot = null;
            try
            {
                sourceRoot = new GameObject(
                    "SourceRoot",
                    typeof(RectTransform));
                mountedRoot = new GameObject(
                    "MountedRoot",
                    typeof(RectTransform));
                GameObject sourcePanel = new GameObject(
                    "ItemDetailPanel",
                    typeof(RectTransform),
                    typeof(ItemDetailPanelView));
                GameObject mountedPanel = new GameObject(
                    "ItemDetailPanel",
                    typeof(RectTransform),
                    typeof(ItemDetailPanelView));
                sourcePanel.transform.SetParent(sourceRoot.transform, false);
                mountedPanel.transform.SetParent(mountedRoot.transform, false);

                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateAuthoredHierarchyAndGeometry(
                        sourceRoot.transform,
                        mountedRoot.transform);
                Require(true, "EXACT_AUTHORED_STRUCTURE_REJECTED");

                CanvasGroup addedComponent =
                    mountedPanel.AddComponent<CanvasGroup>();
                string componentFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateAuthoredHierarchyAndGeometry(
                            sourceRoot.transform,
                            mountedRoot.transform));
                Require(
                    componentFailure.Contains(" field=componentCount "),
                    "ADDED_COMPONENT_CONFLICT_NOT_REJECTED "
                    + componentFailure);
                Object.DestroyImmediate(addedComponent);

                GameObject addedObject = new GameObject(
                    "UnexpectedAddedObject",
                    typeof(RectTransform));
                addedObject.transform.SetParent(mountedRoot.transform, false);
                string objectFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateAuthoredHierarchyAndGeometry(
                            sourceRoot.transform,
                            mountedRoot.transform));
                Require(
                    objectFailure.Contains(" field=transformCount "),
                    "ADDED_OBJECT_CONFLICT_NOT_REJECTED " + objectFailure);
                Object.DestroyImmediate(addedObject);
            }
            finally
            {
                if (mountedRoot != null)
                {
                    Object.DestroyImmediate(mountedRoot);
                }

                if (sourceRoot != null)
                {
                    Object.DestroyImmediate(sourceRoot);
                }
            }
        }

        private static void VerifyRootNameMetadataOverrideContract()
        {
            GameObject sourceRoot = null;
            GameObject mountedRoot = null;
            try
            {
                sourceRoot = new GameObject(
                    "C1ExactBattleSandboxItemDetailPopupStandalone",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                mountedRoot = new GameObject(
                    sourceRoot.name,
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                RectTransform sourceRootRect =
                    sourceRoot.transform as RectTransform;
                GameObject sourceDescendant = new GameObject(
                    "Descendant",
                    typeof(RectTransform));
                sourceDescendant.transform.SetParent(
                    sourceRoot.transform,
                    false);

                PropertyModification accepted = new PropertyModification
                {
                    target = sourceRoot,
                    propertyPath = "m_Name",
                    value = sourceRoot.name,
                    objectReference = null
                };
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateRootNameMetadataOverrideContract(
                        sourceRoot,
                        sourceRootRect,
                        mountedRoot,
                        new[] { accepted });
                Require(true, "IDENTICAL_ROOT_NAME_METADATA_REJECTED");

                PropertyModification changedValue = new PropertyModification
                {
                    target = sourceRoot,
                    propertyPath = "m_Name",
                    value = "ChangedRootName",
                    objectReference = null
                };
                string changedValueFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { changedValue }));
                Require(
                    string.Equals(
                        changedValueFailure,
                        "EXACT_POPUP_MOUNT_ROOT_NAME_OVERRIDE_VALUE_INVALID"
                        + " source=" + sourceRoot.name
                        + " mounted=" + mountedRoot.name
                        + " value=ChangedRootName",
                        StringComparison.Ordinal),
                    "CHANGED_ROOT_NAME_VALUE_NOT_REJECTED "
                    + changedValueFailure);

                PropertyModification descendantName =
                    new PropertyModification
                    {
                        target = sourceDescendant,
                        propertyPath = "m_Name",
                        value = sourceDescendant.name,
                        objectReference = null
                    };
                string descendantFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { descendantName }));
                Require(
                    descendantFailure.StartsWith(
                        "EXACT_POPUP_MOUNT_INTERNAL_PROPERTY_OVERRIDE ",
                        StringComparison.Ordinal)
                    && descendantFailure.EndsWith(
                        ".m_Name",
                        StringComparison.Ordinal),
                    "DESCENDANT_NAME_OVERRIDE_NOT_REJECTED "
                    + descendantFailure);

                string duplicateFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { accepted, accepted }));
                Require(
                    string.Equals(
                        duplicateFailure,
                        "EXACT_POPUP_MOUNT_ROOT_NAME_OVERRIDE_COUNT_INVALID 2",
                        StringComparison.Ordinal),
                    "DUPLICATE_ROOT_NAME_OVERRIDE_NOT_REJECTED "
                    + duplicateFailure);

                string[] allowedRootNumericFields =
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
                foreach (string propertyPath in allowedRootNumericFields)
                {
                    PropertyModification rootNumeric =
                        new PropertyModification
                        {
                            target = sourceRootRect,
                            propertyPath = propertyPath,
                            value = "0.125",
                            objectReference = null
                        };
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { accepted, rootNumeric });
                    Require(
                        true,
                        "ALLOWED_ROOT_CANVAS_RECT_CONTEXT_FIELD_REJECTED "
                        + propertyPath);
                }

                PropertyModification descendantRect =
                    new PropertyModification
                    {
                        target = sourceDescendant.transform,
                        propertyPath = "m_LocalEulerAnglesHint.x",
                        value = "0.5",
                        objectReference = null
                    };
                string descendantRectFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { accepted, descendantRect }));
                Require(
                    descendantRectFailure.StartsWith(
                        "EXACT_POPUP_MOUNT_INTERNAL_PROPERTY_OVERRIDE ",
                        StringComparison.Ordinal)
                    && descendantRectFailure.EndsWith(
                        ".m_LocalEulerAnglesHint.x",
                        StringComparison.Ordinal),
                    "DESCENDANT_RECT_CONTEXT_OVERRIDE_NOT_REJECTED "
                    + descendantRectFailure);

                PropertyModification unknownRootRect =
                    new PropertyModification
                    {
                        target = sourceRootRect,
                        propertyPath = "m_LocalEulerAnglesHint.w",
                        value = "0.5",
                        objectReference = null
                    };
                string unknownRootRectFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { accepted, unknownRootRect }));
                Require(
                    string.Equals(
                        unknownRootRectFailure,
                        "EXACT_POPUP_MOUNT_ROOT_CANVAS_RECT_OVERRIDE_"
                        + "PROPERTY_INVALID m_LocalEulerAnglesHint.w",
                        StringComparison.Ordinal),
                    "UNKNOWN_ROOT_RECT_PROPERTY_NOT_REJECTED "
                    + unknownRootRectFailure);

                PropertyModification unrelatedTarget =
                    new PropertyModification
                    {
                        target = sourceRoot.GetComponent<Canvas>(),
                        propertyPath = "m_Enabled",
                        value = "0",
                        objectReference = null
                    };
                string unrelatedTargetFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { accepted, unrelatedTarget }));
                Require(
                    unrelatedTargetFailure.StartsWith(
                        "EXACT_POPUP_MOUNT_INTERNAL_PROPERTY_OVERRIDE ",
                        StringComparison.Ordinal)
                    && unrelatedTargetFailure.EndsWith(
                        ".m_Enabled",
                        StringComparison.Ordinal),
                    "UNRELATED_TARGET_OVERRIDE_NOT_REJECTED "
                    + unrelatedTargetFailure);

                PropertyModification duplicateRootRect =
                    new PropertyModification
                    {
                        target = sourceRootRect,
                        propertyPath = "m_LocalEulerAnglesHint.z",
                        value = "0.5",
                        objectReference = null
                    };
                string duplicateRootRectFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[]
                            {
                                accepted,
                                duplicateRootRect,
                                duplicateRootRect
                            }));
                Require(
                    string.Equals(
                        duplicateRootRectFailure,
                        "EXACT_POPUP_MOUNT_ROOT_CANVAS_RECT_OVERRIDE_"
                        + "DUPLICATE m_LocalEulerAnglesHint.z",
                        StringComparison.Ordinal),
                    "DUPLICATE_ROOT_RECT_PROPERTY_NOT_REJECTED "
                    + duplicateRootRectFailure);

                PropertyModification nonFiniteRootEuler =
                    new PropertyModification
                    {
                        target = sourceRootRect,
                        propertyPath = "m_LocalEulerAnglesHint.y",
                        value = "NaN",
                        objectReference = null
                    };
                string nonFiniteRootEulerFailure = CaptureParityFailure(() =>
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateRootNameMetadataOverrideContract(
                            sourceRoot,
                            sourceRootRect,
                            mountedRoot,
                            new[] { accepted, nonFiniteRootEuler }));
                Require(
                    string.Equals(
                        nonFiniteRootEulerFailure,
                        "EXACT_POPUP_MOUNT_ROOT_CANVAS_RECT_OVERRIDE_"
                        + "VALUE_INVALID property=m_LocalEulerAnglesHint.y"
                        + " value=NaN",
                        StringComparison.Ordinal),
                    "NON_FINITE_ROOT_EULER_HINT_NOT_REJECTED "
                    + nonFiniteRootEulerFailure);
            }
            finally
            {
                if (mountedRoot != null)
                {
                    Object.DestroyImmediate(mountedRoot);
                }

                if (sourceRoot != null)
                {
                    Object.DestroyImmediate(sourceRoot);
                }
            }
        }

        private static C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
            .AuthoredRectGeometry CreateRectGeometry(
                Vector2 anchorMin,
                Quaternion localRotation)
        {
            return new C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .AuthoredRectGeometry(
                    anchorMin,
                    Vector2.one,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(12f, -24f),
                    new Vector2(320f, 480f),
                    Vector3.one,
                    localRotation);
        }

        private static string CaptureParityFailure(Action action)
        {
            try
            {
                action();
            }
            catch (InvalidOperationException exception)
            {
                return exception.Message;
            }

            throw new InvalidOperationException(
                "RECT_PARITY_EXPECTED_REJECTION_MISSING");
        }

        private static void VerifyCarrierDescendantReferenceContract()
        {
            using PresenterHarness harness = new PresenterHarness();
            Require(
                harness.Presenter.ValidateAuthoredReferences(),
                "DESCENDANT_CARRIER_REFERENCE_REJECTED");
            harness.Presenter.AssignForEditor(
                harness.Panel,
                harness.CloseButton,
                harness.CanvasGroup,
                harness.Frame,
                harness.Panel.transform as RectTransform);
            Require(
                !harness.Presenter.ValidateAuthoredReferences(),
                "PARTIAL_PANEL_AS_CARRIER_WAS_ACCEPTED");
            harness.RestoreExactReferences();
            Require(
                harness.Presenter.ValidateAuthoredReferences(),
                "RESTORED_DESCENDANT_CARRIER_REFERENCE_REJECTED");
            RectTransform carrier = harness.Carrier.transform as RectTransform;
            Require(carrier != null, "CARRIER_ROOT_RECT_MISSING");
            carrier.localScale = Vector3.zero;
            Require(
                !harness.Presenter.ValidateAuthoredReferences()
                && !harness.Presenter.RefreshPresentationLayout(),
                "ZERO_SCALE_EMBEDDED_CARRIER_WAS_ACCEPTED");
            harness.RestoreExactReferences();
            Require(
                harness.Presenter.ValidateAuthoredReferences(),
                "RESTORED_EMBEDDED_CARRIER_GEOMETRY_REJECTED");
        }

        private static void VerifyFormalProjectionOpenCloseAndIdempotence()
        {
            using FormalArrangementHarness formal =
                new FormalArrangementHarness(
                "popup-mount-open-close");
            using PresenterHarness harness = new PresenterHarness();
            C1FormalItemDetailSelectionResult open =
                formal.CurrentFormalItemDetail;
            harness.BackdropButton.onClick.Invoke();
            Require(
                ReferenceEquals(formal.CurrentFormalItemDetail, open)
                && formal.CurrentFormalItemDetail.isOpen
                && string.Equals(
                    formal.Presenter.SelectedItemInstanceId,
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId,
                    StringComparison.Ordinal)
                && Mathf.Approximately(harness.CanvasGroup.alpha, 0f)
                && !harness.CanvasGroup.interactable
                && !harness.CanvasGroup.blocksRaycasts
                && !harness.BackdropImage.enabled
                && !harness.BackdropButton.interactable,
                "BACKDROP_BUTTON_PRETENDED_TO_BE_BOUND_BEFORE_BIND");

            Require(
                harness.Presenter.Bind(
                    formal.Presenter,
                    out string bindDiagnostic),
                "FORMAL_PRESENTER_BIND_FAILED " + bindDiagnostic);
            Require(
                open.accepted
                && open.isOpen
                && harness.Panel.gameObject.activeSelf
                && Mathf.Approximately(harness.CanvasGroup.alpha, 1f)
                && harness.CanvasGroup.interactable
                && harness.CanvasGroup.blocksRaycasts
                && harness.BackdropImage.enabled
                && harness.BackdropImage.raycastTarget
                && harness.BackdropButton.interactable
                && harness.Panel.ValidateVisibleFormalBinding(
                    open.projection.itemInstanceId,
                    open.projection.baseItemId,
                    open.projection.artwork,
                    true),
                "FORMAL_PROJECTION_DID_NOT_OPEN_EXACT_PANEL");

            int fontResolutionCount = harness.Panel.LegacyTextFontResolutionCount;
            Require(
                harness.Presenter.Bind(
                    formal.Presenter,
                    out string rebindDiagnostic)
                && string.Equals(
                    rebindDiagnostic,
                    C1ExactBattleSandboxItemDetailPresenter.BoundDiagnostic,
                    StringComparison.Ordinal)
                && harness.Presenter.IsBound
                && formal.CurrentFormalItemDetail.isOpen
                && harness.Panel.gameObject.activeSelf
                &&
                harness.Panel.LegacyTextFontResolutionCount
                == fontResolutionCount
                && harness.CanvasGroup.blocksRaycasts
                && harness.BackdropImage.enabled,
                "FORMAL_REBIND_WAS_NOT_IDEMPOTENT");

            int formalChangeCount = 0;
            formal.Presenter.FormalItemDetailChanged +=
                _ => formalChangeCount++;
            int beforeClose = formalChangeCount;
            harness.CloseButton.onClick.Invoke();
            RequireClosed(harness, "FORMAL_CLOSE_BUTTON");
            formal.RequireClosedAndInputRestored(
                "FORMAL_CLOSE_BUTTON");
            Require(
                formalChangeCount == beforeClose + 1,
                "FORMAL_CLOSE_BUTTON_DUPLICATE_LISTENER");

            formal.Reopen();
            RequireOpen(harness, formal, "FORMAL_BACKDROP_REOPEN");
            beforeClose = formalChangeCount;
            harness.BackdropButton.onClick.Invoke();
            RequireClosed(harness, "FORMAL_BACKDROP_CLOSE");
            formal.RequireClosedAndInputRestored(
                "FORMAL_BACKDROP_CLOSE");
            Require(
                formalChangeCount == beforeClose + 1,
                "FORMAL_BACKDROP_CLOSE_DUPLICATE_LISTENER");

            formal.Reopen();
            RequireOpen(harness, formal, "FORMAL_OUTSIDE_REOPEN");
            beforeClose = formalChangeCount;
            harness.Panel.RequestClose();
            RequireClosed(harness, "FORMAL_OUTSIDE_CLOSE_FORWARD");
            formal.RequireClosedAndInputRestored(
                "FORMAL_OUTSIDE_CLOSE_FORWARD");
            Require(
                formalChangeCount == beforeClose + 1,
                "FORMAL_OUTSIDE_CLOSE_DUPLICATE_LISTENER");

            formal.Reopen();
            RequireOpen(harness, formal, "FORMAL_CLEAN_REOPEN");
            harness.CloseButton.onClick.Invoke();
            formal.RequireClosedAndInputRestored("FORMAL_CLEAN_REOPEN_CLOSE");
            RequireClosed(harness, "FORMAL_CLEAN_REOPEN_CLOSE");
            harness.Presenter.ResetPresentation();
            RequireClosed(harness, "RESET");
            harness.Presenter.Unbind();
            RequireClosed(harness, "UNBIND");
            Require(
                !harness.Presenter.IsBound,
                "FORMAL_UNBIND_DID_NOT_CLEAR_BINDING");
            harness.Presenter.Unbind();
            RequireClosed(harness, "REPEATED_UNBIND");
            MethodInfo onDisable = typeof(
                    C1ExactBattleSandboxItemDetailPresenter)
                .GetMethod(
                    "OnDisable",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            Require(
                onDisable != null,
                "FORMAL_PRESENTER_ON_DISABLE_LIFECYCLE_MISSING");
            onDisable.Invoke(harness.Presenter, Array.Empty<object>());
            RequireClosed(harness, "POST_UNBIND_DISABLE");

            formal.Reopen();
            harness.BackdropButton.onClick.Invoke();
            Require(
                formal.CurrentFormalItemDetail.isOpen,
                "POST_UNBIND_BACKDROP_LISTENER_REMAINS");
            RequireClosed(harness, "POST_UNBIND_BACKDROP");
            C1FormalItemDetailSelectionResult cleanup =
                formal.Presenter.CloseFormalItemDetail(
                    C1FormalItemDetailSelectionRequest.Close(
                        formal.Authority.Current));
            Require(
                cleanup != null && cleanup.accepted && !cleanup.isOpen,
                "POST_UNBIND_FORMAL_CLEANUP_FAILED");
        }

        private static void RequireOpen(
            PresenterHarness harness,
            FormalArrangementHarness formal,
            string scope)
        {
            C1FormalItemDetailSelectionResult result =
                formal.CurrentFormalItemDetail;
            Require(
                result != null
                && result.accepted
                && result.isOpen
                && result.projection != null
                && harness.Panel.gameObject.activeSelf
                && Mathf.Approximately(harness.CanvasGroup.alpha, 1f)
                && harness.CanvasGroup.interactable
                && harness.CanvasGroup.blocksRaycasts
                && harness.BackdropImage.enabled
                && harness.BackdropButton.interactable
                && harness.Panel.ValidateVisibleFormalBinding(
                    result.projection.itemInstanceId,
                    result.projection.baseItemId,
                    result.projection.artwork,
                    true),
                scope + "_DID_NOT_OPEN_ONE_CLEAN_POPUP");
        }

        private static void VerifyFontFailureClosesAndReleasesInput()
        {
            using ProjectionFixture fixture = new ProjectionFixture(
                "popup-mount-font-failure");
            using PresenterHarness harness = new PresenterHarness();
            SetPrivateField(
                harness.Panel,
                "legacyTextFontResolutionAttempted",
                true);
            SetPrivateField(
                harness.Panel,
                "legacyTextFontFailureLogged",
                true);
            SetPrivateField(harness.Panel, "cachedLegacyTextFont", null);
            InvokeRender(harness.Presenter, fixture.OpenResult, true);
            RequireClosed(harness, "FONT_FAILURE");
        }

        private static void VerifyInvalidLayoutClosesAndReleasesInput()
        {
            using PresenterHarness harness = new PresenterHarness();
            RectTransform viewport = harness.Presenter.transform as RectTransform;
            viewport.sizeDelta = Vector2.zero;
            harness.CanvasGroup.alpha = 1f;
            harness.CanvasGroup.interactable = true;
            harness.CanvasGroup.blocksRaycasts = true;
            Require(
                !harness.Presenter.RefreshPresentationLayout(),
                "ZERO_VIEWPORT_LAYOUT_WAS_ACCEPTED");
            RequireClosed(harness, "INVALID_LAYOUT");
            Require(
                !C1ExactBattleSandboxItemDetailPresenter
                    .TryCalculateUniformReferenceFit(Vector2.zero, out _)
                && !C1ExactBattleSandboxItemDetailPresenter
                    .TryCalculateUniformReferenceFit(
                        new Vector2(float.NaN, 1920f),
                        out _),
                "INVALID_LAYOUT_STATIC_GATE_WAS_ACCEPTED");
        }

        private static void VerifyHashAndSerializedZeroRootIntakeGates()
        {
            string expected =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ExpectedZeroRootMountRepairableShellHash;
            Require(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .IsExpectedZeroRootMountRepairableHash(expected)
                && !C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .IsExpectedZeroRootMountRepairableHash(
                        new string('0', 64)),
                "ZERO_ROOT_HASH_GATE_NOT_EXACT");

            string yaml = ReadRequiredSource(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellPrefabPath)
                .Replace("\r\n", "\n");
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateExactZeroRootMountRowsInYaml(yaml);
            string pivotRow =
                "    - target: {fileID: "
                + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ExactStandaloneRootRectSourceFileId
                    .ToString(CultureInfo.InvariantCulture)
                + ", guid: "
                + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ExactStandaloneGuid
                + ", type: 3}\n"
                + "      propertyPath: m_Pivot.x\n"
                + "      value: 0\n"
                + "      objectReference: {fileID: 0}";
            Require(
                CountToken(yaml, pivotRow) == 1,
                "ZERO_ROOT_PIVOT_ROW_FIXTURE_INVALID");
            string rowDrift = yaml.Replace(
                pivotRow,
                pivotRow.Replace(
                    "      value: 0\n",
                    "      value: 0.25\n"));
            string rowFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateExactZeroRootMountRowsInYaml(rowDrift));
            Require(
                rowFailure.Contains(
                    "EXACT_POPUP_MOUNT_ZERO_ROOT_ROW_VALUE_INVALID "
                    + "m_Pivot.x"),
                "ZERO_ROOT_ROW_DRIFT_WAS_NOT_REJECTED " + rowFailure);
        }

        private static void VerifyCurrentShellIsSupportedState()
        {
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ShellMountState state =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .InspectCurrentShell(out string diagnostic);
            Require(
                state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState
                    .FinalCleanKnownScrollbarDriverBaseline
                || state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.ExactVisibleLifecycleRepairable
                || state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.FinalClean,
                "CURRENT_SHELL_CLASSIFICATION_INVALID " + diagnostic);
            if (state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState
                    .ExactVisibleLifecycleRepairable)
            {
                Require(
                    string.Equals(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ComputeSha256(
                                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                    .ShellPrefabPath),
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExpectedEmbeddedRootKnownHandleResidueShellHash,
                        StringComparison.OrdinalIgnoreCase),
                    "KNOWN_HANDLE_INTAKE_HASH_MISMATCH");
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateEmbeddedRootKnownHandleResidueSerializedShell();
                Require(
                    true,
                    "KNOWN_HANDLE_SERIALIZED_GATE_REJECTED");
            }
            else if (state ==
                     C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                         .ShellMountState
                         .FinalCleanKnownScrollbarDriverBaseline)
            {
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateEmbeddedRootKnownHandleResidueRowsInYaml(
                        ReadRequiredSource(
                            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                .ShellPrefabPath));
            }
        }

        private static void
            VerifyEmbeddedRootKnownHandleBaselineAndIdempotence()
        {
            string expectedHash =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ExpectedEmbeddedRootKnownHandleResidueShellHash;
            Require(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .IsExpectedEmbeddedRootKnownHandleResidueHash(
                        expectedHash)
                && !C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .IsExpectedEmbeddedRootKnownHandleResidueHash(
                        new string('F', 64)),
                "KNOWN_HANDLE_HASH_GATE_NOT_EXACT");

            string yaml = ReadRequiredSource(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellPrefabPath)
                .Replace("\r\n", "\n");
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateEmbeddedRootKnownHandleResidueRowsInYaml(yaml);
            ValidateSerializedHandleRowsForAcceptedState(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState
                    .FinalCleanKnownScrollbarDriverBaseline,
                yaml);
            string ordinaryFinalYaml = yaml;
            foreach (string propertyPath in new[]
                     {
                         "m_AnchorMax.x",
                         "m_AnchorMax.y",
                         "m_AnchorMin.y"
                     })
            {
                ordinaryFinalYaml = ordinaryFinalYaml.Replace(
                    BuildKnownHandleRow(propertyPath),
                    string.Empty);
            }
            ValidateSerializedHandleRowsForAcceptedState(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.FinalClean,
                ordinaryFinalYaml);
            string ordinaryFinalInput = new string(yaml.ToCharArray());
            string ordinaryFinalShellHashBefore =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ComputeSha256(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ShellPrefabPath);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ShellMountState ordinaryFinalLoadedStateBefore =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .InspectCurrentShell(out _);
            RequireSemanticInvalidOperationRejected(
                () => ValidateSerializedHandleRowsForAcceptedState(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.FinalClean,
                    yaml),
                "ORDINARY_FINAL_KNOWN_HANDLE_ROWS");
            Require(
                string.Equals(yaml, ordinaryFinalInput, StringComparison.Ordinal)
                && string.Equals(
                    ordinaryFinalShellHashBefore,
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ComputeSha256(
                            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                .ShellPrefabPath),
                    StringComparison.OrdinalIgnoreCase)
                && ordinaryFinalLoadedStateBefore
                == C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .InspectCurrentShell(out _),
                "ORDINARY_FINAL_REJECTION_MUTATED_INPUT_OR_SHELL");
            string handleRow = BuildKnownHandleRow("m_AnchorMax.x");
            Require(
                CountToken(yaml, handleRow) == 1,
                "KNOWN_HANDLE_ROW_FIXTURE_INVALID");
            RequireKnownHandleYamlRejected(
                yaml.Replace(handleRow, handleRow + "\n" + handleRow),
                "EXTRA_ROW");
            RequireKnownHandleYamlRejected(
                yaml.Replace(handleRow, string.Empty),
                "MISSING_ROW");
            RequireKnownHandleYamlRejected(
                yaml.Replace(
                    handleRow,
                    handleRow.Replace(
                        "      value: 0\n",
                        "      value: 0.25\n")),
                "VALUE_DRIFT");
            RequireKnownHandleYamlRejected(
                yaml.Replace(
                    handleRow,
                    handleRow.Replace(
                        "propertyPath: m_AnchorMax.x",
                        "propertyPath: m_AnchorMin.x")),
                "PROPERTY_DRIFT");
            RequireKnownHandleYamlRejected(
                yaml.Replace(
                    handleRow,
                    handleRow.Replace(
                        "objectReference: {fileID: 0}",
                        "objectReference: {fileID: 1}")),
                "REFERENCE_DRIFT");
            RequireKnownHandleYamlRejected(
                yaml.Replace(
                    handleRow,
                    handleRow.Replace(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .DrivenOverrideHandleSourceFileId.ToString(
                                CultureInfo.InvariantCulture),
                        "1131740001722038518")),
                "TARGET_DRIFT");
            RequireKnownHandleYamlRejected(
                yaml.Replace(
                    handleRow,
                    handleRow.Replace(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExactStandaloneGuid,
                        "11111111111111111111111111111111")),
                "GUID_DRIFT");
            string otherTargetRow = handleRow.Replace(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .DrivenOverrideHandleSourceFileId.ToString(
                        CultureInfo.InvariantCulture),
                "1131740001722038518");
            RequireKnownHandleYamlRejected(
                yaml.Replace(handleRow, handleRow + "\n" + otherTargetRow),
                "OTHER_DRIVEN_TARGET");
            string fourthPropertyRow = handleRow.Replace(
                "propertyPath: m_AnchorMax.x",
                "propertyPath: m_AnchorMin.x");
            RequireKnownHandleYamlRejected(
                yaml.Replace(handleRow, handleRow + "\n" + fourthPropertyRow),
                "FOURTH_PROPERTY");

            string shellHashBefore =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ComputeSha256(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ShellPrefabPath);
            GameObject root = PrefabUtility.LoadPrefabContents(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            try
            {
                C1ExactBattleSandboxItemDetailPresenter presenter = root
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemDetailPresenter>(true)
                    .Single();
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState loadedState =
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(root, out string diagnostic);
                if (loadedState ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.ExactVisibleLifecycleRepairable)
                {
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ApplyVisibleDataArtworkModalLifecycle(root);
                    loadedState =
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ClassifyLoadedShell(
                                root,
                                out diagnostic);
                }
                Require(
                    loadedState
                    == C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState
                        .FinalCleanKnownScrollbarDriverBaseline
                    && C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .CountLoadedExactDrivenOverrides(
                            presenter.CarrierRoot) == 3,
                    "KNOWN_HANDLE_FINAL_STATE_INVALID " + diagnostic);
                Require(
                    presenter.ModalBackdropImage != null
                    && presenter.ModalBackdropButton != null
                    && !presenter.ModalBackdropImage.enabled
                    && !presenter.ModalBackdropButton.interactable,
                    "VISIBLE_LIFECYCLE_MODAL_CONTRACT_INVALID");
                Require(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .IsEmbeddedCarrierRootGeometryValid(
                            presenter.CarrierRoot),
                    "KNOWN_HANDLE_FINAL_EMBEDDED_ROOT_INVALID");
                RectTransform mountedHandle = FindMountedRectBySourceFileId(
                    presenter.CarrierRoot,
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .DrivenOverrideHandleSourceFileId);
                SerializedObject mountedHandleSerialized =
                    new SerializedObject(mountedHandle);
                mountedHandleSerialized.UpdateIfRequiredOrScript();
                foreach (string propertyPath in new[]
                         {
                             "m_AnchorMax.x",
                             "m_AnchorMax.y",
                             "m_AnchorMin.y"
                         })
                {
                    SerializedProperty property = mountedHandleSerialized
                        .FindProperty(propertyPath);
                    Require(
                        property != null
                        && property.propertyType
                        == SerializedPropertyType.Float
                        && property.prefabOverride,
                        "KNOWN_HANDLE_FINAL_PROPERTY_INVALID "
                        + propertyPath);
                }
                RectTransform sourceHandle =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        mountedHandle) as RectTransform;
                string sourceGuid = string.Empty;
                long sourceLocalFileId = 0;
                bool sourceIdentityResolved = sourceHandle != null
                    && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        sourceHandle,
                        out sourceGuid,
                        out sourceLocalFileId);
                Scrollbar[] owners = presenter.CarrierRoot
                    .GetComponentsInChildren<Scrollbar>(true)
                    .Where(scrollbar => scrollbar.handleRect == mountedHandle)
                    .ToArray();
                Scrollbar sourceScrollbar = owners.Length == 1
                    ? PrefabUtility.GetCorrespondingObjectFromSource(
                        owners[0]) as Scrollbar
                    : null;
                Require(
                    sourceIdentityResolved
                    && string.Equals(
                        sourceGuid,
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExactStandaloneGuid,
                        StringComparison.Ordinal)
                    && sourceLocalFileId
                    == C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .DrivenOverrideHandleSourceFileId
                    && owners.Length == 1
                    && ReferenceEquals(
                        mountedHandle.drivenByObject,
                        owners[0])
                    && sourceScrollbar != null
                    && sourceScrollbar.handleRect == sourceHandle,
                    "KNOWN_HANDLE_FINAL_DRIVER_SOURCE_CORRESPONDENCE_INVALID");
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateFinalLoadedShell(root);
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateFinalLoadedShell(root);
                Require(
                    string.Equals(
                        shellHashBefore,
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ComputeSha256(
                                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                    .ShellPrefabPath),
                        StringComparison.OrdinalIgnoreCase),
                    "KNOWN_HANDLE_VALIDATION_CHANGED_SHELL_BYTES");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string BuildKnownHandleRow(string propertyPath)
        {
            return "    - target: {fileID: "
                   + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                       .DrivenOverrideHandleSourceFileId.ToString(
                           CultureInfo.InvariantCulture)
                   + ", guid: "
                   + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                       .ExactStandaloneGuid
                   + ", type: 3}\n"
                   + "      propertyPath: " + propertyPath + "\n"
                   + "      value: 0\n"
                   + "      objectReference: {fileID: 0}";
        }

        private static void RequireKnownHandleYamlRejected(
            string yaml,
            string fixture)
        {
            string inputBefore = new string(yaml.ToCharArray());
            string shellHashBefore =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ComputeSha256(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ShellPrefabPath);
            RequireSemanticInvalidOperationRejected(
                () =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateEmbeddedRootKnownHandleResidueRowsInYaml(yaml),
                "KNOWN_HANDLE_" + fixture);
            Require(
                string.Equals(yaml, inputBefore, StringComparison.Ordinal)
                && string.Equals(
                    shellHashBefore,
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ComputeSha256(
                            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                .ShellPrefabPath),
                    StringComparison.OrdinalIgnoreCase),
                "KNOWN_HANDLE_" + fixture + "_REJECTION_MUTATED_INPUT");
        }

        private static void RequireSemanticInvalidOperationRejected(
            Action action,
            string fixture)
        {
            bool rejected = false;
            try
            {
                action();
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }

            Require(rejected, fixture + "_WAS_NOT_REJECTED");
        }

        private static void VerifyExactZeroRootMountRepairAndIdempotence()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            try
            {
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState state =
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(root, out string diagnostic);
                if (state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.FinalClean
                    || state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState
                        .FinalCleanKnownScrollbarDriverBaseline)
                {
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ValidateFinalLoadedShell(root);
                    Require(true, "FINAL_EMBEDDED_ROOT_RERUN_REJECTED");
                    return;
                }

                Require(
                    state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.ExactZeroRootMountRepairable,
                    "EXPECTED_ZERO_ROOT_REPAIR_STATE_MISSING " + diagnostic);
                C1ExactBattleSandboxItemDetailPresenter presenter = root
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemDetailPresenter>(true)
                    .Single();
                RectTransform carrier = presenter.CarrierRoot;
                Require(
                    !C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .IsEmbeddedCarrierRootGeometryValid(carrier)
                    && !presenter.ValidateAuthoredReferences(),
                    "HASH_BOUND_INTAKE_WAS_NOT_FAIL_CLOSED_BEFORE_REPAIR");

                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ExactStandalonePrefabPath);
                Require(source != null, "ZERO_ROOT_SOURCE_MISSING");
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateAuthoredHierarchyAndGeometry(
                        source.transform,
                        carrier);

                carrier.anchorMax = new Vector2(0.25f, 0.75f);
                carrier.pivot = new Vector2(0.75f, 0.25f);
                carrier.localScale = new Vector3(3f, 2f, 1f);
                Require(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(
                            root,
                            out string drivenDiagnostic)
                    == C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.ExactZeroRootMountRepairable,
                    "CONTEXT_DRIVEN_ROOT_VALUES_CHANGED_SERIALIZED_IDENTITY "
                    + drivenDiagnostic);

                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ApplyEmbeddedCarrierRootMountAdapter(root);
                Require(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(root, out string finalDiagnostic)
                    == C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.FinalClean
                    && C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .IsEmbeddedCarrierRootGeometryValid(carrier)
                    && presenter.ValidateAuthoredReferences(),
                    "ZERO_ROOT_REPAIR_DID_NOT_CONVERGE " + finalDiagnostic);
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateAuthoredHierarchyAndGeometry(
                        source.transform,
                        carrier);
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateFinalLoadedShell(root);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void VerifyHashBoundRepairableRejectsStructuralDrift()
        {
            VerifyHashBoundMutationRejects(
                presenter =>
                {
                    presenter.CarrierRoot.gameObject.name += "_Drift";
                },
                "ROOT_SOURCE_METADATA_DRIFT");
            VerifyHashBoundMutationRejects(
                presenter =>
                {
                    presenter.CarrierRoot.gameObject.AddComponent<CanvasGroup>();
                },
                "ROOT_COMPONENT_DRIFT");
            VerifyHashBoundMutationRejects(
                presenter =>
                {
                    Transform mobile = presenter.CarrierRoot.Find(
                        "MobileSafeAreaRoot");
                    Require(
                        mobile != null,
                        "DESCENDANT_DRIFT_FIXTURE_MISSING");
                    mobile.gameObject.SetActive(
                        !mobile.gameObject.activeSelf);
                },
                "DESCENDANT_GEOMETRY_DRIFT");
        }

        private static void VerifyHashBoundMutationRejects(
            Action<C1ExactBattleSandboxItemDetailPresenter> mutate,
            string scope)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            try
            {
                C1ExactBattleSandboxItemDetailPresenter presenter = root
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemDetailPresenter>(true)
                    .Single();
                mutate(presenter);
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState state =
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(root, out string diagnostic);
                Require(
                    state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.InvalidConflict,
                    scope + "_WAS_NOT_REJECTED " + diagnostic);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void
            VerifyDrivenOverrideRepairableClassificationAndSurgicalRevert()
        {
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ShellMountState diskState =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .InspectCurrentShell(out string diagnostic);
            string yaml = ReadRequiredSource(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath).Replace("\r\n", "\n");
            if (diskState ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState
                    .FinalCleanKnownScrollbarDriverBaseline)
            {
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateEmbeddedRootKnownHandleResidueRowsInYaml(yaml);
                return;
            }

            if (diskState ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.FinalClean)
            {
                Require(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .CountSerializedModificationTargetRowsInYaml(
                            yaml,
                            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                .ExactStandaloneGuid,
                            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                .DrivenOverrideHandleSourceFileId) == 0,
                    "FINAL_CLEAN_DRIVEN_OVERRIDE_RERUN_NOT_IDEMPOTENT");
                return;
            }

            Require(
                diskState ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.ExactDrivenOverridesRepairable,
                "DRIVEN_OVERRIDE_REPAIR_STATE_NOT_CLASSIFIED " + diagnostic);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateDrivenOverrideRepairRowsInYaml(yaml);
            Require(true, "EXACT_THREE_DRIVEN_OVERRIDE_ROWS_REJECTED");

            string anchorMaxX = ExactDrivenOverrideRow("m_AnchorMax.x");
            string missingFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverrideRepairRowsInYaml(
                        yaml.Replace(anchorMaxX, string.Empty)));
            Require(
                missingFailure.Contains("ROW_COUNT_INVALID"),
                "MISSING_DRIVEN_OVERRIDE_ROW_NOT_REJECTED " + missingFailure);
            string extraFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverrideRepairRowsInYaml(
                        yaml.Replace(
                            anchorMaxX,
                            anchorMaxX + "\n" + anchorMaxX)));
            Require(
                extraFailure.Contains("ROW_COUNT_INVALID"),
                "EXTRA_DRIVEN_OVERRIDE_ROW_NOT_REJECTED " + extraFailure);
            string pathFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverrideRepairRowsInYaml(
                        yaml.Replace(
                            anchorMaxX,
                            anchorMaxX.Replace(
                                "m_AnchorMax.x",
                                "m_AnchoredPosition.x"))));
            Require(
                pathFailure.Contains("ROW_INVALID m_AnchorMax.x"),
                "DRIVEN_OVERRIDE_PATH_CONFLICT_NOT_REJECTED " + pathFailure);
            string valueFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverrideRepairRowsInYaml(
                        yaml.Replace(
                            anchorMaxX,
                            anchorMaxX.Replace("value: 0", "value: 0.5"))));
            Require(
                valueFailure.Contains("ROW_INVALID m_AnchorMax.x"),
                "DRIVEN_OVERRIDE_VALUE_CONFLICT_NOT_REJECTED " + valueFailure);
            string guidFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverrideRepairRowsInYaml(
                        yaml.Replace(
                            anchorMaxX,
                            anchorMaxX.Replace(
                                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                    .ExactStandaloneGuid,
                                "11111111111111111111111111111111"))));
            Require(
                guidFailure.Contains("ROW_COUNT_INVALID"),
                "DRIVEN_OVERRIDE_GUID_CONFLICT_NOT_REJECTED " + guidFailure);
            string fileIdFailure = CaptureParityFailure(() =>
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateDrivenOverrideRepairRowsInYaml(
                        yaml.Replace(
                            anchorMaxX,
                            anchorMaxX.Replace(
                                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                                    .DrivenOverrideHandleSourceFileId.ToString(
                                        CultureInfo.InvariantCulture),
                                "8610391659744506857"))));
            Require(
                fileIdFailure.Contains("ROW_COUNT_INVALID"),
                "DRIVEN_OVERRIDE_FILE_ID_CONFLICT_NOT_REJECTED "
                + fileIdFailure);

            GameObject root = PrefabUtility.LoadPrefabContents(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            try
            {
                C1ExactBattleSandboxItemDetailPresenter presenter = root
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemDetailPresenter>(true)
                    .Single();
                RectTransform handle = FindMountedRectBySourceFileId(
                    presenter.CarrierRoot,
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .DrivenOverrideHandleSourceFileId);
                SerializedObject serialized = new SerializedObject(handle);
                foreach (string propertyPath in new[]
                         {
                             "m_AnchorMax.x",
                             "m_AnchorMax.y",
                             "m_AnchorMin.y"
                         })
                {
                    SerializedProperty property =
                        serialized.FindProperty(propertyPath);
                    Require(
                        property != null && property.prefabOverride,
                        "EXPECTED_DRIVEN_OVERRIDE_NOT_PRESENT " + propertyPath);
                }

                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .RepairExactDrivenOverrides(root);
                serialized.UpdateIfRequiredOrScript();
                foreach (string propertyPath in new[]
                         {
                             "m_AnchorMax.x",
                             "m_AnchorMax.y",
                             "m_AnchorMin.y"
                         })
                {
                    SerializedProperty property =
                        serialized.FindProperty(propertyPath);
                    Require(
                        property != null && !property.prefabOverride,
                        "SURGICAL_DRIVEN_OVERRIDE_REVERT_FAILED "
                        + propertyPath);
                }

                Require(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .CountMissingScripts(root) == 0,
                    "SURGICAL_REVERT_CHANGED_MISSING_SCRIPT_STATE");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string ExactDrivenOverrideRow(string propertyPath)
        {
            return "    - target: {fileID: "
                   + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                       .DrivenOverrideHandleSourceFileId.ToString(
                           CultureInfo.InvariantCulture)
                   + ", guid: "
                   + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                       .ExactStandaloneGuid
                   + ", type: 3}\n"
                   + "      propertyPath: " + propertyPath + "\n"
                   + "      value: 0\n"
                   + "      objectReference: {fileID: 0}";
        }

        private static RectTransform FindMountedRectBySourceFileId(
            RectTransform carrier,
            long expectedFileId)
        {
            List<RectTransform> matches = new List<RectTransform>();
            foreach (RectTransform mounted in carrier
                         .GetComponentsInChildren<RectTransform>(true))
            {
                RectTransform source =
                    PrefabUtility.GetCorrespondingObjectFromSource(
                        mounted) as RectTransform;
                if (source != null
                    && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        source,
                        out string guid,
                        out long fileId)
                    && string.Equals(
                        guid,
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .ExactStandaloneGuid,
                        StringComparison.Ordinal)
                    && fileId == expectedFileId)
                {
                    matches.Add(mounted);
                }
            }

            Require(
                matches.Count == 1,
                "TEST_MOUNTED_SOURCE_RECT_COUNT_INVALID " + matches.Count);
            return matches[0];
        }

        private static void VerifyRepairablePartialAndExactPanelOnlyRemoval()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            try
            {
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState state =
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(root, out string diagnostic);
                if (state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.FinalClean
                    || state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState
                        .FinalCleanKnownScrollbarDriverBaseline)
                {
                    Require(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .CountMissingScripts(root) == 0,
                        "FINAL_CLEAN_RERUN_HAS_MISSING_SCRIPT");
                    return;
                }

                if (state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.ExactDrivenOverridesRepairable)
                {
                    Require(
                        C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                            .CountMissingScripts(root) == 0,
                        "DRIVEN_OVERRIDE_REPAIR_STATE_HAS_MISSING_SCRIPT");
                    return;
                }

                Require(
                    state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.ExactRepairablePartial,
                    "EXPECTED_REPAIRABLE_PARTIAL_NOT_CLASSIFIED "
                    + diagnostic);
                C1ExactBattleSandboxItemDetailPresenter presenter = root
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemDetailPresenter>(true)
                    .Single();
                ItemDetailPanelView panel = presenter.PanelView;
                Require(
                    panel != null,
                    "REPAIRABLE_PARTIAL_PANEL_REFERENCE_MISSING");
                SerializedProperty closeProperty = new SerializedObject(panel)
                    .FindProperty("closeButton");
                Button authoredClose = closeProperty == null
                    ? null
                    : closeProperty.objectReferenceValue as Button;
                Require(
                    authoredClose != null
                    && presenter.CloseButton == authoredClose
                    && presenter.ValidateAuthoredReferences()
                    && panel.GetComponents<
                        C1ExactBattleSandboxItemDetailCloseRelay>().Length == 0,
                    "REPAIRABLE_PARTIAL_DIRECT_CLOSE_CONTRACT_INVALID");
                Require(
                    GameObjectUtility
                        .GetMonoBehavioursWithMissingScriptCount(
                            panel.gameObject) == 1
                    && C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .CountMissingScripts(root) == 1,
                    "REPAIRABLE_PARTIAL_MISSING_SCRIPT_SCOPE_INVALID");

                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .RepairExactPartialShell(root);
                Require(
                    GameObjectUtility
                        .GetMonoBehavioursWithMissingScriptCount(
                            panel.gameObject) == 0
                    && C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .CountMissingScripts(root) == 0,
                    "EXACT_PANEL_ONLY_REPAIR_DID_NOT_REMOVE_ONE_PLACEHOLDER");
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState repairedState =
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(
                            root,
                            out string repairedDiagnostic);
                Require(
                    repairedState ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.FinalClean,
                    "IN_MEMORY_REPAIR_NOT_FINAL_CLEAN "
                    + repairedDiagnostic);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void VerifyInvalidConflictClassificationIsFailClosed()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            try
            {
                C1ExactBattleSandboxItemDetailPresenter presenter = root
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemDetailPresenter>(true)
                    .Single();
                GameObject conflict = new GameObject(
                    "UnexpectedDuplicateCarrier",
                    typeof(RectTransform));
                conflict.transform.SetParent(presenter.PresentationFrame, false);
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState state =
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ClassifyLoadedShell(root, out string diagnostic);
                Require(
                    state ==
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ShellMountState.InvalidConflict
                    && diagnostic.Contains("FOUNDATION_REFERENCE_INVALID"),
                    "DUPLICATE_CARRIER_CONFLICT_WAS_NOT_FAIL_CLOSED "
                    + diagnostic);
                Object.DestroyImmediate(conflict);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void VerifySerializedFinalCarrier()
        {
            string yaml = ReadRequiredSource(
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellPrefabPath);
            string exactSourceLine =
                "m_SourcePrefab: {fileID: 100100000, guid: "
                + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ExactStandaloneGuid;
            Require(
                CountToken(yaml, exactSourceLine) == 1,
                "SERIALIZED_EXACT_CARRIER_SOURCE_COUNT_INVALID");
            Require(
                CountToken(
                    yaml,
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .LegacyPanelGuid) == 0,
                "SERIALIZED_LEGACY_PANEL_SOURCE_REMAINS");
            Require(
                CountToken(yaml, "carrierRoot: {fileID:") == 1
                && CountToken(yaml, "carrierRoot: {fileID: 0}") == 0,
                "SERIALIZED_CARRIER_REFERENCE_INVALID");
            Require(
                CountToken(
                    yaml,
                    "m_Name: "
                    + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ModalBackdropName) == 1
                && CountToken(yaml, "modalBackdropImage: {fileID: 0}") == 0
                && CountToken(yaml, "modalBackdropImage: {fileID:") == 1
                && CountToken(yaml, "modalBackdropButton: {fileID: 0}") == 0
                && CountToken(yaml, "modalBackdropButton: {fileID:") == 1,
                "SERIALIZED_MODAL_BACKDROP_CONTRACT_INVALID");
            Require(
                CountToken(
                    yaml,
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .RepairablePartialAddedObjectFileId) == 0
                && CountToken(yaml, "m_Script: {fileID: 0}") == 0
                && CountToken(
                    yaml,
                    "targetCorrespondingSourceObject: {fileID: "
                    + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .RepairablePartialPanelSourceFileId
                    + ", guid: "
                    + C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ExactStandaloneGuid) == 0,
                "SERIALIZED_REPAIRABLE_PARTIAL_RESIDUE_REMAINS");
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ShellMountState state =
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .InspectCurrentShell(out string diagnostic);
            Require(
                state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.FinalClean
                || state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState
                    .FinalCleanKnownScrollbarDriverBaseline,
                "SERIALIZED_FINAL_STATE_INVALID " + diagnostic);
            ValidateSerializedHandleRowsForAcceptedState(state, yaml);
        }

        private static void ValidateSerializedHandleRowsForAcceptedState(
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ShellMountState state,
            string yaml)
        {
            if (state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState
                    .FinalCleanKnownScrollbarDriverBaseline)
            {
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ValidateEmbeddedRootKnownHandleResidueRowsInYaml(yaml);
                return;
            }

            Require(
                state ==
                C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                    .ShellMountState.FinalClean,
                "SERIALIZED_HANDLE_ROWS_UNSUPPORTED_FINAL_STATE " + state);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateNoDrivenOverrideResidueInYaml(yaml);
            C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                .ValidateEmbeddedRootRowsInYaml(yaml);
        }

        private static void RequireClosed(
            PresenterHarness harness,
            string scope)
        {
            Require(
                !harness.Panel.gameObject.activeSelf
                && Mathf.Approximately(harness.CanvasGroup.alpha, 0f)
                && !harness.CanvasGroup.interactable
                && !harness.CanvasGroup.blocksRaycasts
                && !harness.BackdropImage.enabled
                && !harness.BackdropButton.interactable,
                scope + "_DID_NOT_RELEASE_POPUP_INPUT");
        }

        private static void InvokeRender(
            C1ExactBattleSandboxItemDetailPresenter presenter,
            C1FormalItemDetailSelectionResult result,
            bool force)
        {
            Require(RenderMethod != null, "PRESENTER_RENDER_METHOD_MISSING");
            RenderMethod.Invoke(presenter, new object[] { result, force });
        }

        private static void SetPrivateField(
            object target,
            string name,
            object value)
        {
            FieldInfo field = target.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(field != null, "PRIVATE_TEST_FIELD_MISSING " + name);
            field.SetValue(target, value);
        }

        private static string ReadRequiredSource(string path)
        {
            Require(File.Exists(path), "TEST_SOURCE_MISSING " + path);
            return File.ReadAllText(path);
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

        private static void Require(bool condition, string diagnostic)
        {
            assertionCount++;
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }

        private sealed class FormalArrangementHarness : IDisposable
        {
            private const string BoardPrefabPath =
                "Assets/_Game/Prefabs/TalismanBag/Items/"
                + "C1ExactBattleSandboxItemBoard.prefab";
            private const string TrayPrefabPath =
                "Assets/_Game/Prefabs/TalismanBag/Items/"
                + "C1ExactBattleSandboxItemTray.prefab";

            private readonly GameObject root;

            public FormalArrangementHarness(string sessionSuffix)
            {
                root = new GameObject(
                    "FormalArrangementHarness",
                    typeof(RectTransform),
                    typeof(C1ExactBattleSandboxItemArrangementPresenter));
                root.hideFlags = HideFlags.HideAndDontSave;
                GameObject boardAsset = AssetDatabase.LoadAssetAtPath<
                    GameObject>(BoardPrefabPath);
                GameObject trayAsset = AssetDatabase.LoadAssetAtPath<
                    GameObject>(TrayPrefabPath);
                Require(
                    boardAsset != null && trayAsset != null,
                    "FORMAL_ARRANGEMENT_EXACT_PREFAB_MISSING");
                GameObject boardObject = Object.Instantiate(boardAsset);
                GameObject trayObject = Object.Instantiate(trayAsset);
                boardObject.name = boardAsset.name;
                trayObject.name = trayAsset.name;
                boardObject.transform.SetParent(root.transform, false);
                trayObject.transform.SetParent(root.transform, false);

                C1ExactBattleSandboxItemBoardView board = boardObject
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemBoardView>(true)
                    .Single();
                C1ExactBattleSandboxItemTrayView tray = trayObject
                    .GetComponentsInChildren<
                        C1ExactBattleSandboxItemTrayView>(true)
                    .Single();
                Presenter = root.GetComponent<
                    C1ExactBattleSandboxItemArrangementPresenter>();
                Presenter.AssignForEditor(board, tray);
                Require(
                    Presenter.ValidateAuthoredReferences(),
                    "FORMAL_ARRANGEMENT_AUTHORED_REFERENCE_INVALID");

                C1FormalItemSessionCreationResult creation =
                    C1FormalItemSessionAuthority.Create(
                        "exact-popup-bind-lifecycle." + sessionSuffix,
                        1L);
                Require(
                    creation != null
                    && creation.isSuccess
                    && creation.authority != null,
                    "FORMAL_ARRANGEMENT_AUTHORITY_CREATE_FAILED "
                    + creation?.diagnosticCode);
                Authority = creation.authority;
                Require(
                    Presenter.Bind(
                        Authority,
                        out string diagnostic),
                    "FORMAL_ARRANGEMENT_BIND_FAILED " + diagnostic);
                C1FormalItemInteractionAuthorizationResult authorization =
                    Presenter.ApplyInteractionAuthorization(
                        C1FormalItemInteractionAuthorizationRequest
                            .FromSnapshot(
                                Authority.Current,
                                C1FormalItemInteractionMode
                                    .PREPARE_ENABLED));
                Require(
                    authorization != null
                    && authorization.accepted
                    && authorization.interactionEnabled,
                    "FORMAL_ARRANGEMENT_INTERACTION_ENABLE_FAILED "
                    + authorization?.diagnostic);
                Presenter.SelectBoardItem(
                    CanonicalInitialItemAcquisitionPolicy.ItemInstanceId);
                Require(
                    CurrentFormalItemDetail != null
                    && CurrentFormalItemDetail.accepted
                    && CurrentFormalItemDetail.isOpen
                    && CurrentFormalItemDetail.projection != null,
                    "FORMAL_ARRANGEMENT_REAL_I001_OPEN_FAILED "
                    + CurrentFormalItemDetail?.diagnostic);
            }

            public C1ExactBattleSandboxItemArrangementPresenter Presenter
            {
                get;
            }

            public C1FormalItemSessionAuthority Authority { get; }

            public C1FormalItemDetailSelectionResult CurrentFormalItemDetail =>
                Presenter.CurrentFormalItemDetail;

            public void Reopen()
            {
                C1FormalItemDetailSelectionResult result =
                    Presenter.OpenSelectedFormalItemDetail(
                        C1FormalItemDetailSelectionRequest.Open(
                            Authority.Current,
                            CanonicalInitialItemAcquisitionPolicy.ItemInstanceId));
                Require(
                    result != null
                    && result.accepted
                    && result.isOpen
                    && result.projection != null,
                    "FORMAL_ARRANGEMENT_REOPEN_FAILED "
                    + result?.diagnostic);
            }

            public void RequireClosedAndInputRestored(string scope)
            {
                C1FormalItemDetailSelectionResult current =
                    CurrentFormalItemDetail;
                Require(
                    current != null
                    && current.accepted
                    && !current.isOpen
                    && Presenter.CurrentInteractionAuthorization != null
                    && Presenter.CurrentInteractionAuthorization
                        .interactionEnabled,
                    scope + "_FORMAL_STATE_OR_ITEM_INPUT_NOT_RESTORED");
            }

            public void Dispose()
            {
                if (Presenter != null)
                {
                    Presenter.Unbind();
                }

                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private sealed class PresenterHarness : IDisposable
        {
            private readonly GameObject root;

            public PresenterHarness()
            {
                root = new GameObject(
                    "ExactItemDetailPresenterHarness",
                    typeof(RectTransform),
                    typeof(CanvasGroup),
                    typeof(C1ExactBattleSandboxItemDetailPresenter));
                RectTransform rootRect = (RectTransform)root.transform;
                rootRect.anchorMin = new Vector2(0.5f, 0.5f);
                rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.sizeDelta = new Vector2(1080f, 1920f);
                CanvasGroup = root.GetComponent<CanvasGroup>();
                CanvasGroup.alpha = 0f;
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
                CanvasGroup.ignoreParentGroups = false;
                Presenter = root.GetComponent<
                    C1ExactBattleSandboxItemDetailPresenter>();

                GameObject backdropObject = new GameObject(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ModalBackdropName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(Button));
                RectTransform backdropRect =
                    (RectTransform)backdropObject.transform;
                backdropRect.SetParent(root.transform, false);
                backdropRect.anchorMin = Vector2.zero;
                backdropRect.anchorMax = Vector2.one;
                backdropRect.anchoredPosition = Vector2.zero;
                backdropRect.sizeDelta = Vector2.zero;
                backdropRect.pivot = new Vector2(0.5f, 0.5f);
                BackdropImage = backdropObject.GetComponent<Image>();
                BackdropImage.color = new Color(
                    0f,
                    0f,
                    0f,
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ModalBackdropAlpha);
                BackdropImage.raycastTarget = true;
                BackdropImage.enabled = false;
                BackdropButton = backdropObject.GetComponent<Button>();
                BackdropButton.targetGraphic = BackdropImage;
                BackdropButton.transition = Selectable.Transition.None;
                UnityEngine.UI.Navigation navigation =
                    BackdropButton.navigation;
                navigation.mode = UnityEngine.UI.Navigation.Mode.None;
                BackdropButton.navigation = navigation;
                BackdropButton.interactable = false;

                GameObject frameObject = new GameObject(
                    "ItemDetailPresentationFrame",
                    typeof(RectTransform));
                Frame = (RectTransform)frameObject.transform;
                Frame.SetParent(root.transform, false);
                Frame.anchorMin = new Vector2(0.5f, 0.5f);
                Frame.anchorMax = new Vector2(0.5f, 0.5f);
                Frame.sizeDelta = new Vector2(1080f, 1920f);

                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(
                    C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring
                        .ExactStandalonePrefabPath);
                Require(asset != null, "HARNESS_EXACT_SOURCE_MISSING");
                Carrier = Object.Instantiate(asset);
                Carrier.name = asset.name;
                Carrier.transform.SetParent(Frame, false);
                ApplyEmbeddedCarrierRootGeometry(
                    Carrier.transform as RectTransform);
                Panel = Carrier.GetComponentsInChildren<
                        ItemDetailPanelView>(true)
                    .Single();
                CloseButton = RequiredObjectReference<Button>(
                    Panel,
                    "closeButton");
                RestoreExactReferences();
            }

            public C1ExactBattleSandboxItemDetailPresenter Presenter { get; }
            public CanvasGroup CanvasGroup { get; }
            public Image BackdropImage { get; }
            public Button BackdropButton { get; }
            public RectTransform Frame { get; }
            public GameObject Carrier { get; }
            public ItemDetailPanelView Panel { get; }
            public Button CloseButton { get; }

            public void RestoreExactReferences()
            {
                ApplyEmbeddedCarrierRootGeometry(
                    Carrier.transform as RectTransform);
                Presenter.AssignForEditor(
                    Panel,
                    CloseButton,
                    CanvasGroup,
                    Frame,
                    Carrier.transform as RectTransform,
                    BackdropImage,
                    BackdropButton);
            }

            private static void ApplyEmbeddedCarrierRootGeometry(
                RectTransform carrier)
            {
                Require(carrier != null, "HARNESS_CARRIER_RECT_MISSING");
                carrier.anchorMin = Vector2.zero;
                carrier.anchorMax = Vector2.one;
                carrier.anchoredPosition = Vector2.zero;
                carrier.sizeDelta = Vector2.zero;
                carrier.pivot = new Vector2(0.5f, 0.5f);
                carrier.localPosition = Vector3.zero;
                carrier.localRotation = Quaternion.identity;
                carrier.localEulerAngles = Vector3.zero;
                carrier.localScale = Vector3.one;
            }

            public void Dispose()
            {
                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }
            }

            private static T RequiredObjectReference<T>(
                Object owner,
                string propertyPath)
                where T : Object
            {
                SerializedProperty property = new SerializedObject(owner)
                    .FindProperty(propertyPath);
                T value = property == null
                    ? null
                    : property.objectReferenceValue as T;
                Require(
                    value != null,
                    "HARNESS_REFERENCE_MISSING " + propertyPath);
                return value;
            }
        }

        private sealed class ProjectionFixture : IDisposable
        {
            private readonly Texture2D texture;
            private readonly Sprite sprite;

            public ProjectionFixture(string sessionSuffix)
            {
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 2f, 2f),
                    new Vector2(0.5f, 0.5f));
                C1FormalItemSessionCreationResult creation =
                    C1FormalItemSessionAuthority.Create(
                        "exact-popup-tests." + sessionSuffix,
                        1L);
                Require(
                    creation != null
                    && creation.isSuccess
                    && creation.authority != null,
                    "FORMAL_SESSION_CREATE_FAILED "
                    + creation?.diagnosticCode);
                C1FormalItemSessionSnapshot snapshot =
                    creation.authority.Current;
                OpenResult = C1FormalItemDetailProjectionAndSelection.Evaluate(
                    C1FormalItemDetailSelectionRequest.Open(
                        snapshot,
                        CanonicalInitialItemAcquisitionPolicy.ItemInstanceId),
                    snapshot,
                    new FakeArtworkResolver(sprite),
                    C1FormalItemDetailSelectionResult.InitialClosed());
                Require(
                    OpenResult != null
                    && OpenResult.accepted
                    && OpenResult.isOpen
                    && OpenResult.projection != null,
                    "FORMAL_OPEN_RESULT_INVALID "
                    + OpenResult?.diagnostic);
                CloseResult = C1FormalItemDetailProjectionAndSelection.Evaluate(
                    C1FormalItemDetailSelectionRequest.Close(snapshot),
                    snapshot,
                    new FakeArtworkResolver(sprite),
                    OpenResult);
                Require(
                    CloseResult != null
                    && CloseResult.accepted
                    && !CloseResult.isOpen,
                    "FORMAL_CLOSE_RESULT_INVALID "
                    + CloseResult?.diagnostic);
            }

            public C1FormalItemDetailSelectionResult OpenResult { get; }
            public C1FormalItemDetailSelectionResult CloseResult { get; }

            public void Dispose()
            {
                if (sprite != null)
                {
                    Object.DestroyImmediate(sprite);
                }

                if (texture != null)
                {
                    Object.DestroyImmediate(texture);
                }
            }
        }

        private sealed class FakeProvider : IItemDetailViewModelProvider
        {
            private readonly ItemDetailViewModel model = CreateModel();

            public IReadOnlyList<ItemDetailListEntry> GetItemList()
            {
                return new[]
                {
                    new ItemDetailListEntry(
                        model.itemId,
                        model.displayItemName)
                };
            }

            public ItemDetailViewModel GetDetailViewModel(string itemId)
            {
                return string.Equals(
                    itemId,
                    model.itemId,
                    StringComparison.Ordinal)
                    ? model
                    : null;
            }

            private static ItemDetailViewModel CreateModel()
            {
                ItemDetailViewModel value = new ItemDetailViewModel
                {
                    itemId = "I001",
                    baseItemId = "I001",
                    displayItemName = "formal.I001",
                    displayShapeName = "shape_single_1",
                    displayRarityName = "白",
                    rarityKey = "white",
                    statusFlags = new ItemDetailStatusFlags(),
                    lightingPreview = new ItemLightingPreview
                    {
                        previewText = "formal lighting"
                    },
                    buildPreview = new ItemBuildPreview
                    {
                        faMenPreviewText = "formal fa-men",
                        qiLeiPreviewText = "formal qi-lei"
                    }
                };
                value.displayPrimaryStats.Add(new ItemDetailStatLine(
                    "damage",
                    "82",
                    "formal"));
                value.displayPlayerSections.Add(
                    new ItemDetailSectionViewModel(
                        "formal",
                        "formal combat facts",
                        "basic",
                        true));
                return value;
            }
        }

        private sealed class FakeArtworkResolver :
            IC1FormalItemArtworkResolver
        {
            private readonly Sprite sprite;

            public FakeArtworkResolver(Sprite configuredSprite)
            {
                sprite = configuredSprite;
            }

            public bool TryResolveFormalItemArtwork(
                string authoritativeBaseItemId,
                C1FormalItemArtworkLightingState lightingState,
                out Sprite artwork,
                out string artworkIdentity)
            {
                artwork = sprite;
                artworkIdentity = C1FormalItemArtworkIdentity.Create(
                    authoritativeBaseItemId,
                    lightingState);
                return artwork != null;
            }
        }
    }
}
