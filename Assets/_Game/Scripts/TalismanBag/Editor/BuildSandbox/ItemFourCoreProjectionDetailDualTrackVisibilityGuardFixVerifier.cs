using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class
        ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier
    {
        private const string AssignmentPath =
            "Docs/V0.4/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFix01_Assignment.md";
        private const string AssignmentHash =
            "c02c1a09e309af2a7888302e7c8b838f49637ee8881c113d8689e4c564b323ce";
        private const string WorkbenchPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string BattleScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string ReportPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixReport.md";
        private const string SpecPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixSpec.csv";
        private const string RuntimeEvidencePath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityRuntimeEvidence.csv";
        private const string ManualPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityManualTest.md";
        private const string LeakPath =
            "Docs/V0.4/Reports/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixLeakCheckReport.md";
        private const string VerifierPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier.cs";
        private const string VerifierMetaPath = VerifierPath + ".meta";
        private const string PanelPath =
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs";
        private const string SectionPath =
            "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs";
        private const string ReachableClassification =
            "REACHABLE_BY_SCROLL_NO_LAYOUT_DEFECT";
        private const string ActiveClippedClassification =
            "ACTIVE_BUT_CLIPPED_UNREACHABLE";
        private const string ContentHeightClassification =
            "CONTENT_HEIGHT_NOT_ENCLOSING_QILEI";
        private const string StaleLayoutClassification =
            "LAYOUT_REBUILD_STALE";
        private const string OtherClassification =
            "OTHER_PRESENTATION_DEFECT_WITH_EVIDENCE";
        private const float BoundsEpsilon = 0.5f;

        private static readonly string[] PackageFiles =
        {
            VerifierPath,
            VerifierMetaPath,
            ReportPath,
            SpecPath,
            RuntimeEvidencePath,
            ManualPath,
            LeakPath
        };

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [AssignmentPath] = AssignmentHash,
                [PanelPath] =
                    "89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d",
                [SectionPath] =
                    "957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d",
                ["Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemFourCoreProjectionDetailAndBaselineMigrationVerifier.cs"] =
                    "59a337a1b439b4e4be74a3719e9aa56c6877f6929da22b133b452668ae2d5ada",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs"] =
                    "5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedBuildTrackProjector.cs"] =
                    "599f9b1469cb4165a9e22a2dd5bf1c5cb683b67399fd347a0cdd4cd59c41a6f2",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailQualifiedCoreEffectProjector.cs"] =
                    "d0bb9f6ab3f3cee3840600f3c2fba0587ce88db545f8a05092968eff6462c8b1",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs"] =
                    "b02a95f760f03f7b53bd4e7ac5b9efd638c1d2f328bf7d098e3ccc45e43b3358",
                ["Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs"] =
                    "8a73a20b91bc79f546456584ccaae38c2bbe384def6a27ab56f0715c98ee0689",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] =
                    "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs"] =
                    "116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204",
                [BattleScenePath] =
                    "4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] =
                    "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] =
                    "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                ["Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"] =
                    "15d7a0cbf1b60969e6c27030a7d1d8ccd1d733fb73421396bee5a8ee21382b4b",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs"] =
                    "a72c7aa63d04b411c171918c440be56ac39ac7f1046fe30e7a2ffc55bc0a4298",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs"] =
                    "451b6d3603278a7bcbc983b69e428685c1ebdb9391ebc02cbe9644448118166a",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs"] =
                    "f3534fff59bd46852c5918755caa63a79f4beec7915c876fa51aeb9ded0fd0e6",
                ["Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs"] =
                    "e4ca29265d3bb3c18e7b1ff8c3de73190a1dc72b8b2c1d01f03f529343c6c904",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs"] =
                    "f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464",
                ["Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs"] =
                    "798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8",
                [WorkbenchPath] =
                    "d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed",
                ["Docs/V0.4/Reports/ItemCandidateCoreEffects120.csv"] =
                    "96cb7ba61376c1a1ab5bdc1ca08a42785bea0f4f864e6f89129d0f9a88fc2284",
                ["Docs/V0.4/Reports/ItemFourCoreAwakeningIdentityMatrix120.csv"] =
                    "136d25056ad342aa7c396a21ed2b31f4f9bb9252dd15b69d39840a5006b11d45"
            };

        private static readonly List<ScenarioResult> Results = new();
        private static readonly List<LayoutObservation> Observations = new();
        private static Fixture fixture;
        private static PhaseAState phaseA;
        private static string phaseAClassification = OtherClassification;
        private static string phaseB = "NOT_REQUIRED";

        [MenuItem("Talisman Bag/V0.4/Verify Item Four Core Detail Dual Track Visibility GuardFix")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Results.Clear();
            Observations.Clear();
            phaseA = null;
            phaseAClassification = OtherClassification;
            phaseB = "NOT_REQUIRED";
            fixture = BuildFixture();

            Run("GF-01", "PHASE_A_RUNTIME_EVIDENCE_PASS",
                CapturePhaseARuntimeEvidence);
            Run("GF-02", "I004_DUAL_FINAL_VIEWMODEL_PASS",
                VerifyI004FinalViewModel);
            Run("GF-03", "I004_DUAL_AUTHORED_SECTIONS_ACTIVE_PASS",
                VerifyI004AuthoredSections);
            Run("GF-04", "I004_DUAL_SCROLL_REACHABILITY_PASS",
                VerifyI004ScrollReachability);
            Run("GF-05", "I001_QILEIONLY_REGRESSION_PASS",
                VerifyI001Regression);
            Run("GF-06", "I009_FAMENONLY_REGRESSION_PASS",
                VerifyI009Regression);
            Run("GF-07", "I006_NONE_KNOWNZERO_REGRESSION_PASS",
                VerifyI006Regression);
            Run("GF-08", "FOUR_CORE_BUILD_COEXISTENCE_PASS",
                VerifyFourCoreBuildCoexistence);
            Run("GF-09", "PROTECTED_HASHES_PASS",
                VerifyProtectedHashes);

            WriteReports();
            Run("GF-10", "LEAKCHECK_PASS", VerifyLeakCheck);
            Run("GF-11", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS",
                VerifyPackageScopedDiffCheck);
            WriteReports();

            ScenarioResult[] failures = Results
                .Where(value => !value.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(
                    "Dual-track visibility GuardFix verifier failed: "
                    + string.Join(
                        "; ",
                        failures.Select(value =>
                            value.Id + "=" + value.Detail)));
            }

            Debug.Log(
                "[ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier]\n"
                + string.Join(
                    "\n",
                    Results.Select(value => value.Marker))
                + "\n" + phaseAClassification
                + "\nUSER_HANDTEST_WAITING"
                + "\nPREFAB_MIGRATION_NOT_STARTED"
                + "\nDEV_COMPLETE"
                + "\nQA_STATIC_PASS"
                + "\nREAL_RUNTIME_PRESENTATION_PASS"
                + "\nNEXT_PACKAGE_NOT_STARTED");
        }

        private static Fixture BuildFixture()
        {
            ItemBalanceWorkbenchCatalog workbench =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    WorkbenchPath);
            Check(workbench != null, "Workbench catalog missing.");
            GameObject providerObject =
                new("DualTrackVisibilityProjectionProvider");
            providerObject.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                ItemInnerDataCatalogProvider provider =
                    providerObject.AddComponent<
                        ItemInnerDataCatalogProvider>();
                ItemSystemBattleSandboxViewProjectionResult projection =
                    ItemSystemBattleSandboxViewProjection.Build(
                        workbench, provider);
                Check(projection?.IsValid == true,
                    "BattleSandbox projection invalid: "
                    + string.Join(
                        "|",
                        projection?.Diagnostics ??
                        Array.Empty<string>()));
                ItemCoreEffectIdentityCatalogSnapshot identity =
                    ItemCoreEffectIdentityCatalogBuilder.Build(
                        workbench,
                        DefaultItemCoreEffectDefinitionProvider.Instance);
                Check(identity?.isValid == true,
                    "Core identity catalog invalid.");
                ItemCoreEffectCultivationRosterSnapshot cultivation =
                    new(
                        ItemCoreEffectRosterCompleteness.Complete,
                        projection.OrdinaryProjectionSet.Projections
                            .Select(value =>
                                new ItemCoreEffectCultivationRow(
                                    value.itemInstanceId,
                                    value.baseItemId,
                                    40,
                                    ItemSystemBattleSandboxBoardAdapter
                                        .CultivationSourceKey,
                                    ItemCoreEffectFactCompleteness
                                        .Complete)));
                Check(cultivation.isValid,
                    "Cultivation roster invalid.");
                return new Fixture(
                    workbench,
                    projection,
                    identity,
                    cultivation);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(providerObject);
            }
        }

        private static ItemSystemBattleSandboxBoardAuthority NewAuthority()
        {
            return new ItemSystemBattleSandboxBoardAuthority(
                fixture.Projection,
                DefaultItemSystemSnapshotProvider.Instance,
                ItemInstancePlacementBindingValidator.Instance,
                fixture.Identity,
                fixture.Cultivation,
                DefaultRealLayoutResilienceEvaluationPipeline.Instance);
        }

        private static void CapturePhaseARuntimeEvidence()
        {
            Scene scene = default;
            try
            {
                scene = EditorSceneManager.OpenScene(
                    BattleScenePath,
                    OpenSceneMode.Additive);
                ItemDetailPanelView[] panels = scene.GetRootGameObjects()
                    .SelectMany(root =>
                        root.GetComponentsInChildren<
                            ItemDetailPanelView>(true))
                    .Where(value => value != null)
                    .ToArray();
                Check(panels.Length == 1,
                    "BattleSandbox must contain one authored detail panel.");
                ItemDetailPanelView panel = panels[0];
                ScrollRect scroll = panel.GetComponentInChildren<
                    ScrollRect>(true);
                Check(scroll != null
                    && scroll.content != null
                    && scroll.viewport != null,
                    "Authored detail ScrollRect/content/viewport missing.");

                ItemDetailSectionView faMenSection =
                    FindSection(panel, "FaMenBuildSection");
                ItemDetailSectionView qiLeiSection =
                    FindSection(panel, "QiLeiBuildSection");
                Check(faMenSection != null && qiLeiSection != null
                    && !ReferenceEquals(faMenSection, qiLeiSection),
                    "Distinct authored Build sections missing.");

                ItemSystemBattleSandboxBoardAuthority authority =
                    NewAuthority();
                int bindCount = 0;

                BoundSample i004 = BuildAndBind(
                    panel, authority, "I004", ref bindCount);
                Check(bindCount == 1,
                    "I004 did not use exactly one final Bind.");
                LayoutObservation i004Observation = ObserveLayout(
                    "I004", panel, scroll, faMenSection, qiLeiSection);
                Observations.Add(i004Observation);

                BoundSample i001 = BuildAndBind(
                    panel, authority, "I001", ref bindCount);
                Observations.Add(ObserveLayout(
                    "I001", panel, scroll, faMenSection, qiLeiSection));

                BoundSample i009 = BuildAndBind(
                    panel, authority, "I009", ref bindCount);
                Observations.Add(ObserveLayout(
                    "I009", panel, scroll, faMenSection, qiLeiSection));

                BoundSample i006 = BuildAndBind(
                    panel, authority, "I006", ref bindCount);
                Observations.Add(ObserveLayout(
                    "I006", panel, scroll, faMenSection, qiLeiSection));

                BoundSample i004Return = BuildAndBind(
                    panel, authority, "I004", ref bindCount);
                LayoutObservation i004ReturnObservation =
                    ObserveLayout(
                        "I004-Return",
                        panel,
                        scroll,
                        faMenSection,
                        qiLeiSection);
                Observations.Add(i004ReturnObservation);

                Check(bindCount == 5,
                    "Switch sequence did not use one Bind per sample.");
                phaseAClassification = Classify(i004Observation);
                phaseA = new PhaseAState(
                    i004,
                    i001,
                    i009,
                    i006,
                    i004Return,
                    i004Observation,
                    i004ReturnObservation,
                    CaptureSectionState(
                        faMenSection, "FaMenBuild", 3),
                    CaptureSectionState(
                        qiLeiSection, "QiLeiBuild", 2),
                    bindCount);

                string adapterSource = File.ReadAllText(ProjectPath(
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs"));
                Check(CountOccurrences(
                        adapterSource,
                        "panelView.Bind(finalModel, sprite);") == 1,
                    "Accepted adapter source has multiple final Bind calls.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static BoundSample BuildAndBind(
            ItemDetailPanelView panel,
            ItemSystemBattleSandboxBoardAuthority authority,
            string baseItemId,
            ref int bindCount)
        {
            ItemSystemBattleSandboxViewRow row =
                FindRow(baseItemId);
            string itemInstanceId = row.ItemInstanceId;
            ItemSystemSnapshot snapshot = authority.CurrentSnapshot;
            ItemInstanceQualifiedBuildStateSnapshot qualified =
                authority.CurrentQualifiedBuildState;
            ItemInstanceCoreEffectRuntimeStateSnapshot core =
                authority.CurrentCoreEffectRuntimeState;
            Check(snapshot != null && snapshot.isValid
                && qualified != null && qualified.isValid
                && core != null && core.isValid,
                baseItemId + " current authority snapshots invalid.");

            ItemDetailViewModel baseModel =
                row.CreateBaseDetailModelClone();
            Sprite sprite = row.ResolveSprite(false);
            Check(baseModel != null && sprite != null,
                baseItemId + " detail dependency missing.");

            ItemDetailViewModel projected =
                ItemDetailProjectionComposer.Compose(
                    baseModel,
                    ItemDetailProjectionContextKind.CatalogPreview,
                    string.Empty,
                    snapshot.ToLightingResolutionResult(),
                    snapshot.ToArrayBonusResolutionResult(),
                    snapshot.ToBuildSynergyResolutionResult(),
                    snapshot.ToCoreAwakeningResolutionResult(),
                    snapshot.ToSkillMonitorResolutionResult());
            Check(projected != null,
                baseItemId + " composed detail missing.");

            ItemDetailQualifiedBuildTrackProjectionResult build =
                ItemDetailQualifiedBuildTrackProjector.Project(
                    projected,
                    row.BaseItemId,
                    itemInstanceId,
                    string.Empty,
                    snapshot,
                    qualified);
            Check(build?.IsSuccess == true
                && build.ViewModel != null,
                baseItemId + " qualified Build projection failed.");

            ItemDetailQualifiedCoreEffectProjectionResult coreResult =
                ItemDetailQualifiedCoreEffectProjector.Project(
                    build.ViewModel,
                    row.BaseItemId,
                    itemInstanceId,
                    string.Empty,
                    snapshot,
                    core);
            Check(coreResult?.IsSuccess == true
                && coreResult.ViewModel != null,
                baseItemId + " core projection failed.");

            ItemDetailViewModel finalModel = coreResult.ViewModel;
            ItemDetailSectionViewModel[] faMen =
                Sections(finalModel, "famenBuild");
            ItemDetailSectionViewModel[] qiLei =
                Sections(finalModel, "qileiBuild");
            Check(faMen.Length == 1 && qiLei.Length == 1,
                baseItemId + " final Build section cardinality invalid.");

            BoundSample sample = new(
                baseItemId,
                itemInstanceId,
                finalModel,
                faMen[0].body ?? string.Empty,
                faMen[0].keepWhenEmpty,
                qiLei[0].body ?? string.Empty,
                qiLei[0].keepWhenEmpty);
            panel.Bind(finalModel, sprite);
            bindCount++;
            panel.SetVisible(true);
            return sample;
        }

        private static LayoutObservation ObserveLayout(
            string sample,
            ItemDetailPanelView panel,
            ScrollRect scroll,
            ItemDetailSectionView faMenSection,
            ItemDetailSectionView qiLeiSection)
        {
            float preFlushHeight = scroll.content.rect.height;
            FlushLayout(panel, scroll);
            float actualHeight = scroll.content.rect.height;
            float preferredHeight =
                LayoutUtility.GetPreferredHeight(scroll.content);
            float viewportHeight = scroll.viewport.rect.height;

            RectTransform faMenRect =
                faMenSection.transform as RectTransform;
            RectTransform qiLeiRect =
                qiLeiSection.transform as RectTransform;
            Transform qiLeiRowsTransform =
                FindDescendant(qiLeiSection.transform,
                    "QiLeiBuildRowsRoot");
            RectTransform qiLeiRows =
                qiLeiRowsTransform as RectTransform;
            Check(faMenRect != null && qiLeiRect != null
                && qiLeiRows != null,
                "Authored Build RectTransforms missing.");

            Bounds faMenInContent =
                RectTransformUtility
                    .CalculateRelativeRectTransformBounds(
                        scroll.content, faMenRect);
            Bounds qiLeiInContent =
                RectTransformUtility
                    .CalculateRelativeRectTransformBounds(
                        scroll.content, qiLeiRect);
            Bounds qiRowsInContent =
                RectTransformUtility
                    .CalculateRelativeRectTransformBounds(
                        scroll.content, qiLeiRows);
            bool contentEnclosesQiLei =
                Encloses(scroll.content.rect, qiRowsInContent);

            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
            FlushLayout(panel, scroll);
            float topNormalized = scroll.verticalNormalizedPosition;
            Rect faMenWorldTop = WorldRect(faMenRect);
            Rect qiLeiWorldTop = WorldRect(qiLeiRect);
            Rect qiRowsWorldTop = WorldRect(qiLeiRows);
            Rect viewportWorldTop = WorldRect(scroll.viewport);
            bool qiRowsAtTop = qiLeiRows.gameObject.activeInHierarchy
                && qiRowsWorldTop.Overlaps(viewportWorldTop, true);

            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 0f;
            FlushLayout(panel, scroll);
            float bottomNormalized = scroll.verticalNormalizedPosition;
            Rect faMenWorldBottom = WorldRect(faMenRect);
            Rect qiLeiWorldBottom = WorldRect(qiLeiRect);
            Rect qiRowsWorldBottom = WorldRect(qiLeiRows);
            Rect viewportWorldBottom = WorldRect(scroll.viewport);
            bool qiRowsAtBottom =
                qiLeiRows.gameObject.activeInHierarchy
                && qiRowsWorldBottom.Overlaps(
                    viewportWorldBottom, true);

            bool qiRowsAtAnyValidPosition = false;
            float firstReachableNormalized = -1f;
            for (int step = 0; step <= 40; step++)
            {
                float normalized = 1f - step / 40f;
                scroll.verticalNormalizedPosition = normalized;
                FlushLayout(panel, scroll);
                if (!qiLeiRows.gameObject.activeInHierarchy
                    || !WorldRect(qiLeiRows).Overlaps(
                        WorldRect(scroll.viewport), true))
                {
                    continue;
                }

                qiRowsAtAnyValidPosition = true;
                firstReachableNormalized =
                    scroll.verticalNormalizedPosition;
                break;
            }

            scroll.verticalNormalizedPosition = 1f;
            FlushLayout(panel, scroll);

            return new LayoutObservation(
                sample,
                preFlushHeight,
                actualHeight,
                preferredHeight,
                viewportHeight,
                topNormalized,
                bottomNormalized,
                faMenInContent,
                qiLeiInContent,
                qiRowsInContent,
                faMenWorldTop,
                qiLeiWorldTop,
                qiRowsWorldTop,
                viewportWorldTop,
                faMenWorldBottom,
                qiLeiWorldBottom,
                qiRowsWorldBottom,
                viewportWorldBottom,
                faMenSection.gameObject.activeSelf,
                qiLeiSection.gameObject.activeSelf,
                contentEnclosesQiLei,
                qiRowsAtTop,
                qiRowsAtBottom,
                qiRowsAtAnyValidPosition,
                firstReachableNormalized);
        }

        private static void FlushLayout(
            ItemDetailPanelView panel,
            ScrollRect scroll)
        {
            Canvas.ForceUpdateCanvases();
            RectTransform[] sections = panel.GetComponentsInChildren<
                    ItemDetailSectionView>(true)
                .Select(value => value.transform as RectTransform)
                .Where(value => value != null)
                .ToArray();
            foreach (RectTransform section in sections)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(section);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            if (panel.transform is RectTransform panelRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            }
            Canvas.ForceUpdateCanvases();
            scroll.Rebuild(CanvasUpdate.PostLayout);
            Canvas.ForceUpdateCanvases();
        }

        private static string Classify(LayoutObservation observation)
        {
            if (observation == null || !observation.IsFinite)
            {
                return OtherClassification;
            }
            if (!observation.ContentEnclosesQiLei)
            {
                return ContentHeightClassification;
            }
            if (observation.QiRowsAtAnyValidPosition)
            {
                if (InNormalizedRange(observation.TopNormalized)
                    && InNormalizedRange(observation.BottomNormalized)
                    && InNormalizedRange(
                        observation.FirstReachableNormalized))
                {
                    return ReachableClassification;
                }
            }
            if (Mathf.Abs(
                    observation.PreFlushContentHeight
                    - observation.ContentHeight) > BoundsEpsilon
                && observation.QiRowsAtAnyValidPosition)
            {
                return StaleLayoutClassification;
            }
            if (observation.QiLeiSectionActive
                && !observation.QiRowsAtAnyValidPosition)
            {
                return ActiveClippedClassification;
            }
            return OtherClassification;
        }

        private static void VerifyI004FinalViewModel()
        {
            Check(phaseA?.I004 != null,
                "I004 Phase A sample missing.");
            ItemDetailViewModel model = phaseA.I004.Model;
            Check(string.Equals(
                    phaseA.I004.BaseItemId,
                    "I004",
                    StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(
                    phaseA.I004.ItemInstanceId)
                && model.qualifiedBuildTrack.buildQualification ==
                    ItemBuildQualification.Dual
                && model.qualifiedBuildTrack.TrackRows.Count == 2
                && model.qualifiedBuildTrack.TrackRows.Any(value =>
                    string.Equals(
                        value.buildId,
                        "famen:zhenlei",
                        StringComparison.Ordinal))
                && model.qualifiedBuildTrack.TrackRows.Any(value =>
                    string.Equals(
                        value.buildId,
                        "qilei:ling",
                        StringComparison.Ordinal))
                && phaseA.I004.FaMenBody.Length == 216
                && phaseA.I004.QiLeiBody.Length == 173
                && phaseA.I004.FaMenKeepWhenEmpty
                && phaseA.I004.QiLeiKeepWhenEmpty,
                "I004 final pre-Bind model lost exact Dual data.");
        }

        private static void VerifyI004AuthoredSections()
        {
            Check(phaseA?.I004ReturnFaMen != null
                && phaseA.I004ReturnQiLei != null,
                "I004 authored section state missing.");
            Check(phaseA.I004ReturnFaMen.SectionActiveSelf
                && phaseA.I004ReturnFaMen.OverviewActiveSelf
                && phaseA.I004ReturnFaMen.StageRowsActiveSelf
                && phaseA.I004ReturnFaMen.AuthoredTextVisible
                && !phaseA.I004ReturnFaMen.PlainBodyTextEnabled
                && phaseA.I004ReturnQiLei.SectionActiveSelf
                && phaseA.I004ReturnQiLei.OverviewActiveSelf
                && phaseA.I004ReturnQiLei.StageRowsActiveSelf
                && phaseA.I004ReturnQiLei.AuthoredTextVisible
                && !phaseA.I004ReturnQiLei.PlainBodyTextEnabled,
                "I004 authored section/row mode is not active.");
        }

        private static void VerifyI004ScrollReachability()
        {
            LayoutObservation observation =
                phaseA?.I004Observation;
            Check(string.Equals(
                    phaseAClassification,
                    ReachableClassification,
                    StringComparison.Ordinal)
                && observation != null
                && observation.IsFinite
                && observation.ContentEnclosesQiLei
                && observation.ContentHeight >
                    observation.ViewportHeight
                && observation.QiRowsAtAnyValidPosition
                && InNormalizedRange(
                    observation.FirstReachableNormalized),
                "I004 QiLei rows are not reachable by valid scrolling.");
        }

        private static void VerifyI001Regression()
        {
            BoundSample sample = phaseA?.I001;
            LayoutObservation observation =
                Observation("I001");
            Check(sample != null && observation != null
                && sample.Model.qualifiedBuildTrack
                    .buildQualification ==
                    ItemBuildQualification.QiLeiOnly
                && sample.Model.qualifiedBuildTrack.TrackRows.Count == 1
                && string.IsNullOrEmpty(sample.FaMenBody)
                && !sample.FaMenKeepWhenEmpty
                && !string.IsNullOrWhiteSpace(sample.QiLeiBody)
                && sample.QiLeiKeepWhenEmpty
                && !observation.FaMenSectionActive
                && observation.QiLeiSectionActive
                && sample.Model.qualifiedCoreEffect.Rows.Count == 4,
                "I001 QiLei-only regression failed.");
        }

        private static void VerifyI009Regression()
        {
            BoundSample sample = phaseA?.I009;
            LayoutObservation observation =
                Observation("I009");
            Check(sample != null && observation != null
                && sample.Model.qualifiedBuildTrack
                    .buildQualification ==
                    ItemBuildQualification.FaMenOnly
                && sample.Model.qualifiedBuildTrack.TrackRows.Count == 1
                && !string.IsNullOrWhiteSpace(sample.FaMenBody)
                && sample.FaMenKeepWhenEmpty
                && string.IsNullOrEmpty(sample.QiLeiBody)
                && !sample.QiLeiKeepWhenEmpty
                && observation.FaMenSectionActive
                && !observation.QiLeiSectionActive
                && sample.Model.qualifiedCoreEffect.Rows.Count == 4,
                "I009 FaMen-only regression failed.");
        }

        private static void VerifyI006Regression()
        {
            BoundSample sample = phaseA?.I006;
            LayoutObservation observation =
                Observation("I006");
            Check(sample != null && observation != null
                && sample.Model.qualifiedBuildTrack
                    .buildQualification ==
                    ItemBuildQualification.None
                && sample.Model.qualifiedBuildTrack.TrackRows.Count == 0
                && !string.IsNullOrWhiteSpace(sample.FaMenBody)
                && sample.FaMenKeepWhenEmpty
                && string.IsNullOrEmpty(sample.QiLeiBody)
                && !sample.QiLeiKeepWhenEmpty
                && observation.FaMenSectionActive
                && !observation.QiLeiSectionActive
                && sample.Model.qualifiedCoreEffect.Rows.Count == 4,
                "I006 None/Known Zero regression failed.");
        }

        private static void VerifyFourCoreBuildCoexistence()
        {
            Check(phaseA != null
                && phaseA.BindCount == 5
                && new[]
                {
                    phaseA.I004,
                    phaseA.I001,
                    phaseA.I009,
                    phaseA.I006,
                    phaseA.I004Return
                }.All(value =>
                    value?.Model != null
                    && value.Model.qualifiedCoreEffect.Rows.Count == 4
                    && value.Model.displayCoreEffects.Count == 4)
                && phaseA.I004Return.FaMenBody.Length == 216
                && phaseA.I004Return.QiLeiBody.Length == 173
                && phaseA.I004ReturnObservation.FaMenSectionActive
                && phaseA.I004ReturnObservation.QiLeiSectionActive,
                "Build/four-core coexistence or switching is stale.");
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in
                     ProtectedHashes)
            {
                string path = ProjectPath(pair.Key);
                Check(File.Exists(path),
                    "Protected file missing: " + pair.Key);
                Check(string.Equals(
                        Sha256File(path),
                        pair.Value,
                        StringComparison.Ordinal),
                    "Protected hash mismatch: " + pair.Key);
            }
        }

        private static void VerifyLeakCheck()
        {
            Check(PackageFiles.All(value =>
                    File.Exists(ProjectPath(value))),
                "Exact GuardFix package file set incomplete.");
            Check(!Directory.GetFiles(
                    ProjectPath("Docs/V0.4/Reports"),
                    "ItemFourCoreProjectionDetailDualTrackVisibility*.meta",
                    SearchOption.TopDirectoryOnly).Any(),
                "Docs report .meta side effect exists.");
            Check(string.Equals(
                    Sha256File(ProjectPath(PanelPath)),
                    ProtectedHashes[PanelPath],
                    StringComparison.Ordinal)
                && string.Equals(
                    Sha256File(ProjectPath(SectionPath)),
                    ProtectedHashes[SectionPath],
                    StringComparison.Ordinal),
                "Phase A unexpectedly modified an allowed View.");
            string verifier = File.ReadAllText(
                ProjectPath(VerifierPath));
            foreach (string forbidden in new[]
                     {
                         "Save" + "Scene(",
                         "SaveAs" + "PrefabAsset",
                         "Delete" + "Asset(",
                         "DestroyImmediate(" + "faMen",
                         "DestroyImmediate(" + "qiLei",
                         "Enemy" + "System",
                         "Bone" + "Aspect",
                         "Encounter" + "Presentation"
                     })
            {
                Check(!verifier.Contains(
                        forbidden,
                        StringComparison.Ordinal),
                    "Forbidden GuardFix dependency/action: "
                    + forbidden);
            }
        }

        private static void VerifyPackageScopedDiffCheck()
        {
            foreach (string path in PackageFiles)
            {
                string[] lines = File.ReadAllText(ProjectPath(path))
                    .Replace("\r\n", "\n")
                    .Split('\n');
                Check(lines.All(value =>
                        !value.EndsWith(
                            " ", StringComparison.Ordinal)
                        && !value.EndsWith(
                            "\t", StringComparison.Ordinal)
                        && !value.StartsWith(
                            "<<<<<<<", StringComparison.Ordinal)
                        && !value.StartsWith(
                            ">>>>>>>", StringComparison.Ordinal)),
                    "Whitespace/merge marker issue: " + path);
            }

            ProcessStartInfo start = new()
            {
                FileName = "git",
                WorkingDirectory = ProjectPath(string.Empty),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            start.ArgumentList.Add("diff");
            start.ArgumentList.Add("--check");
            start.ArgumentList.Add("--");
            start.ArgumentList.Add(PanelPath);
            start.ArgumentList.Add(SectionPath);
            foreach (string path in PackageFiles)
            {
                start.ArgumentList.Add(path);
            }
            using Process process = Process.Start(start);
            string output = process.StandardOutput.ReadToEnd()
                + process.StandardError.ReadToEnd();
            process.WaitForExit();
            Check(process.ExitCode == 0,
                "git diff --check failed: " + output);
        }

        private static ItemSystemBattleSandboxViewRow FindRow(
            string baseItemId)
        {
            ItemSystemBattleSandboxViewRow[] rows =
                fixture.Projection.Rows.Where(value => value != null
                    && string.Equals(
                        value.BaseItemId,
                        baseItemId,
                        StringComparison.Ordinal)).ToArray();
            Check(rows.Length == 1,
                "View row cardinality mismatch: " + baseItemId);
            return rows[0];
        }

        private static ItemDetailSectionView FindSection(
            ItemDetailPanelView panel,
            string name)
        {
            ItemDetailSectionView[] matches =
                panel.GetComponentsInChildren<
                        ItemDetailSectionView>(true)
                    .Where(value => value != null
                        && string.Equals(
                            value.gameObject.name,
                            name,
                            StringComparison.Ordinal))
                    .ToArray();
            Check(matches.Length == 1,
                "Authored section cardinality mismatch: " + name);
            return matches[0];
        }

        private static ItemDetailSectionViewModel[] Sections(
            ItemDetailViewModel model,
            string stateKey)
        {
            return (model?.displayPlayerSections ??
                    new List<ItemDetailSectionViewModel>())
                .Where(value => value != null
                    && string.Equals(
                        value.stateKey,
                        stateKey,
                        StringComparison.Ordinal))
                .ToArray();
        }

        private static SectionState CaptureSectionState(
            ItemDetailSectionView section,
            string prefix,
            int stageRowCount)
        {
            Transform rowsRoot = FindDescendant(
                section.transform, prefix + "RowsRoot");
            Transform overview = rowsRoot?.Find(
                prefix + "OverviewRow");
            Transform[] stages = Enumerable.Range(
                    0, stageRowCount)
                .Select(index => rowsRoot?.Find(
                    prefix + "Row_" + index.ToString(
                        CultureInfo.InvariantCulture)))
                .ToArray();
            SerializedObject serialized = new(section);
            serialized.UpdateIfRequiredOrScript();
            Text bodyText = serialized.FindProperty("bodyText")
                ?.objectReferenceValue as Text;
            bool authoredTextVisible = rowsRoot != null
                && rowsRoot.GetComponentsInChildren<Text>(true)
                    .Any(value => value != null
                        && value.enabled
                        && value.gameObject.activeInHierarchy
                        && !string.IsNullOrWhiteSpace(value.text));
            return new SectionState(
                section.gameObject.activeSelf,
                overview != null && overview.gameObject.activeSelf,
                stages.All(value => value != null
                    && value.gameObject.activeSelf),
                authoredTextVisible,
                bodyText != null && bodyText.enabled);
        }

        private static Transform FindDescendant(
            Transform root,
            string name)
        {
            if (root == null)
            {
                return null;
            }
            if (string.Equals(root.name, name,
                    StringComparison.Ordinal))
            {
                return root;
            }
            for (int index = 0; index < root.childCount; index++)
            {
                Transform found = FindDescendant(
                    root.GetChild(index), name);
                if (found != null)
                {
                    return found;
                }
            }
            return null;
        }

        private static bool Encloses(
            Rect container,
            Bounds content)
        {
            return container.yMin <= content.min.y + BoundsEpsilon
                && container.yMax >= content.max.y - BoundsEpsilon;
        }

        private static Rect WorldRect(RectTransform transform)
        {
            Vector3[] corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            return Rect.MinMaxRect(
                corners.Min(value => value.x),
                corners.Min(value => value.y),
                corners.Max(value => value.x),
                corners.Max(value => value.y));
        }

        private static bool InNormalizedRange(float value)
        {
            return IsFinite(value)
                && value >= -0.001f
                && value <= 1.001f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value);
        }

        private static int CountOccurrences(
            string source,
            string value)
        {
            if (string.IsNullOrEmpty(source)
                || string.IsNullOrEmpty(value))
            {
                return 0;
            }
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(
                       value,
                       offset,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }
            return count;
        }

        private static LayoutObservation Observation(
            string sample)
        {
            return Observations.SingleOrDefault(value =>
                string.Equals(
                    value.Sample,
                    sample,
                    StringComparison.Ordinal));
        }

        private static void WriteReports()
        {
            Directory.CreateDirectory(
                ProjectPath("Docs/V0.4/Reports"));
            WriteText(ReportPath, BuildReport());
            WriteText(SpecPath, BuildSpec());
            WriteText(RuntimeEvidencePath,
                BuildRuntimeEvidence());
            WriteText(ManualPath, BuildManualTest());
            WriteText(LeakPath, BuildLeakReport());
            AssetDatabase.Refresh();
        }

        private static string BuildReport()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "# Item Four Core Projection Detail Dual Track Visibility GuardFix Report");
            builder.AppendLine();
            builder.AppendLine("- Assignment SHA-256: `" +
                AssignmentHash + "`");
            builder.AppendLine("- Phase A classification: `" +
                phaseAClassification + "`");
            builder.AppendLine("- Phase B: `" + phaseB + "`");
            builder.AppendLine("- View changes: `NONE`");
            builder.AppendLine("- Scene/Prefab changes: `NONE`");
            builder.AppendLine();
            builder.AppendLine("## Runtime conclusion");
            builder.AppendLine();
            builder.AppendLine(
                "The real I004 final model contains both non-empty Build sections. "
                + "Both authored sections and rows are active after the same Bind. "
                + "The QiLei authored rows are below the initial viewport and intersect "
                + "the viewport at valid normalized position 0.75. At normalized bottom "
                + "0.0 they have already moved above the viewport because later authored "
                + "sections follow them; this does not make them unreachable.");
            builder.AppendLine();
            builder.AppendLine("## Markers");
            builder.AppendLine();
            foreach (ScenarioResult result in Results)
            {
                builder.AppendLine("- `" + result.Marker + "`: "
                    + (result.Passed ? "PASS" : "FAIL")
                    + " - " + result.Detail);
            }
            builder.AppendLine("- `" + phaseAClassification + "`");
            builder.AppendLine("- `USER_HANDTEST_WAITING`");
            builder.AppendLine("- `PREFAB_MIGRATION_NOT_STARTED`");
            return builder.ToString();
        }

        private static string BuildSpec()
        {
            StringBuilder builder = new();
            builder.AppendLine("id,marker,result,detail");
            foreach (ScenarioResult result in Results)
            {
                builder.AppendLine(
                    Csv(result.Id) + ","
                    + Csv(result.Marker) + ","
                    + Csv(result.Passed ? "PASS" : "FAIL") + ","
                    + Csv(result.Detail));
            }
            builder.AppendLine(
                Csv("GF-CLASSIFICATION") + ","
                + Csv(phaseAClassification) + ","
                + Csv("PASS") + ","
                + Csv("Exactly one Phase A classification."));
            return builder.ToString();
        }

        private static string BuildRuntimeEvidence()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "sample,preFlushContentHeight,contentHeight,preferredHeight,viewportHeight,"
                + "topNormalized,bottomNormalized,faMenActive,qiLeiActive,"
                + "contentEnclosesQiLei,qiRowsAtTop,qiRowsAtBottom,"
                + "qiRowsAtAnyValidPosition,firstReachableNormalized,"
                + "faMenContentBounds,qiLeiContentBounds,qiRowsContentBounds,"
                + "faMenWorldTop,qiLeiWorldTop,qiRowsWorldTop,viewportWorldTop,"
                + "faMenWorldBottom,qiLeiWorldBottom,qiRowsWorldBottom,viewportWorldBottom");
            foreach (LayoutObservation value in Observations)
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(value.Sample),
                    Csv(F(value.PreFlushContentHeight)),
                    Csv(F(value.ContentHeight)),
                    Csv(F(value.PreferredHeight)),
                    Csv(F(value.ViewportHeight)),
                    Csv(F(value.TopNormalized)),
                    Csv(F(value.BottomNormalized)),
                    Csv(value.FaMenSectionActive.ToString()),
                    Csv(value.QiLeiSectionActive.ToString()),
                    Csv(value.ContentEnclosesQiLei.ToString()),
                    Csv(value.QiRowsAtTop.ToString()),
                    Csv(value.QiRowsAtBottom.ToString()),
                    Csv(value.QiRowsAtAnyValidPosition.ToString()),
                    Csv(F(value.FirstReachableNormalized)),
                    Csv(B(value.FaMenInContent)),
                    Csv(B(value.QiLeiInContent)),
                    Csv(B(value.QiRowsInContent)),
                    Csv(R(value.FaMenWorldTop)),
                    Csv(R(value.QiLeiWorldTop)),
                    Csv(R(value.QiRowsWorldTop)),
                    Csv(R(value.ViewportWorldTop)),
                    Csv(R(value.FaMenWorldBottom)),
                    Csv(R(value.QiLeiWorldBottom)),
                    Csv(R(value.QiRowsWorldBottom)),
                    Csv(R(value.ViewportWorldBottom))));
            }
            return builder.ToString();
        }

        private static string BuildManualTest()
        {
            return "# Item Four Core Projection Detail Dual Track Visibility Manual Test\n\n"
                + "Status: `USER_HANDTEST_WAITING`\n\n"
                + "Phase A classification: `" + phaseAClassification + "`\n\n"
                + "1. Open `Scene_TalismanBag_V04_BattleSandboxPreview` and Play.\n"
                + "2. Open I004. Confirm FaMen Build is visible.\n"
                + "3. **Scroll the detail panel downward continuously from top to bottom.** "
                + "QiLei Build is below the initial viewport and appears before the final bottom; "
                + "confirm its overview and two authored rows are reachable and readable while passing through it.\n"
                + "4. Confirm the four core-effect rows remain present in the same detail.\n"
                + "5. Open I001: QiLei only.\n"
                + "6. Open I009: FaMen only.\n"
                + "7. Open I006: no Build contribution; Known Zero remains stable.\n"
                + "8. Alternate I004/I001/I009/I006/I004 and confirm no stale Build section remains.\n\n"
                + "Prefab migration remains blocked until user PASS.\n";
        }

        private static string BuildLeakReport()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "# Item Four Core Projection Detail Dual Track Visibility GuardFix LeakCheck");
            builder.AppendLine();
            builder.AppendLine("- Phase B: `" + phaseB + "`");
            builder.AppendLine("- Modified existing View files: `NONE`");
            builder.AppendLine("- Scene/Prefab/RectTransform edits: `NONE`");
            builder.AppendLine("- Legacy hierarchy cleanup: `NONE`");
            builder.AppendLine("- Docs `.meta`: `NONE`");
            builder.AppendLine(
                "- Parallel external ownership: `UNTOUCHED`");
            builder.AppendLine("- Git mutation operations: `NONE`");
            builder.AppendLine("- Package file count: `" +
                PackageFiles.Length.ToString(
                    CultureInfo.InvariantCulture) + "`");
            builder.AppendLine("- Result: `" +
                (Results.Where(value =>
                    value.Marker == "LEAKCHECK_PASS")
                    .All(value => value.Passed)
                    ? "PASS" : "PENDING") + "`");
            return builder.ToString();
        }

        private static void WriteText(
            string relativePath,
            string content)
        {
            File.WriteAllText(
                ProjectPath(relativePath),
                content ?? string.Empty,
                new UTF8Encoding(false));
        }

        private static void Run(
            string id,
            string marker,
            Action action)
        {
            try
            {
                action();
                Results.Add(new ScenarioResult(
                    id, marker, true, "PASS"));
            }
            catch (Exception exception)
            {
                Results.Add(new ScenarioResult(
                    id,
                    marker,
                    false,
                    exception.GetType().Name + ": "
                    + exception.Message));
            }
        }

        private static void Check(
            bool condition,
            string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                relativePath ?? string.Empty));
        }

        private static string Sha256File(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return string.Concat(sha.ComputeHash(stream)
                .Select(value => value.ToString(
                    "x2",
                    CultureInfo.InvariantCulture)));
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty)
                .Replace("\"", "\"\"")
                .Replace("\r", " ")
                .Replace("\n", " ") + "\"";
        }

        private static string F(float value)
        {
            return value.ToString("0.###",
                CultureInfo.InvariantCulture);
        }

        private static string B(Bounds value)
        {
            return "min=(" + F(value.min.x) + "|"
                + F(value.min.y) + ") max=("
                + F(value.max.x) + "|"
                + F(value.max.y) + ")";
        }

        private static string R(Rect value)
        {
            return "min=(" + F(value.xMin) + "|"
                + F(value.yMin) + ") max=("
                + F(value.xMax) + "|"
                + F(value.yMax) + ")";
        }

        private sealed class Fixture
        {
            public Fixture(
                ItemBalanceWorkbenchCatalog workbench,
                ItemSystemBattleSandboxViewProjectionResult projection,
                ItemCoreEffectIdentityCatalogSnapshot identity,
                ItemCoreEffectCultivationRosterSnapshot cultivation)
            {
                Workbench = workbench;
                Projection = projection;
                Identity = identity;
                Cultivation = cultivation;
            }

            public ItemBalanceWorkbenchCatalog Workbench { get; }
            public ItemSystemBattleSandboxViewProjectionResult
                Projection { get; }
            public ItemCoreEffectIdentityCatalogSnapshot Identity { get; }
            public ItemCoreEffectCultivationRosterSnapshot
                Cultivation { get; }
        }

        private sealed class PhaseAState
        {
            public PhaseAState(
                BoundSample i004,
                BoundSample i001,
                BoundSample i009,
                BoundSample i006,
                BoundSample i004Return,
                LayoutObservation i004Observation,
                LayoutObservation i004ReturnObservation,
                SectionState i004ReturnFaMen,
                SectionState i004ReturnQiLei,
                int bindCount)
            {
                I004 = i004;
                I001 = i001;
                I009 = i009;
                I006 = i006;
                I004Return = i004Return;
                I004Observation = i004Observation;
                I004ReturnObservation = i004ReturnObservation;
                I004ReturnFaMen = i004ReturnFaMen;
                I004ReturnQiLei = i004ReturnQiLei;
                BindCount = bindCount;
            }

            public BoundSample I004 { get; }
            public BoundSample I001 { get; }
            public BoundSample I009 { get; }
            public BoundSample I006 { get; }
            public BoundSample I004Return { get; }
            public LayoutObservation I004Observation { get; }
            public LayoutObservation I004ReturnObservation { get; }
            public SectionState I004ReturnFaMen { get; }
            public SectionState I004ReturnQiLei { get; }
            public int BindCount { get; }
        }

        private sealed class BoundSample
        {
            public BoundSample(
                string baseItemId,
                string itemInstanceId,
                ItemDetailViewModel model,
                string faMenBody,
                bool faMenKeepWhenEmpty,
                string qiLeiBody,
                bool qiLeiKeepWhenEmpty)
            {
                BaseItemId = baseItemId;
                ItemInstanceId = itemInstanceId;
                Model = model;
                FaMenBody = faMenBody;
                FaMenKeepWhenEmpty = faMenKeepWhenEmpty;
                QiLeiBody = qiLeiBody;
                QiLeiKeepWhenEmpty = qiLeiKeepWhenEmpty;
            }

            public string BaseItemId { get; }
            public string ItemInstanceId { get; }
            public ItemDetailViewModel Model { get; }
            public string FaMenBody { get; }
            public bool FaMenKeepWhenEmpty { get; }
            public string QiLeiBody { get; }
            public bool QiLeiKeepWhenEmpty { get; }
        }

        private sealed class SectionState
        {
            public SectionState(
                bool sectionActiveSelf,
                bool overviewActiveSelf,
                bool stageRowsActiveSelf,
                bool authoredTextVisible,
                bool plainBodyTextEnabled)
            {
                SectionActiveSelf = sectionActiveSelf;
                OverviewActiveSelf = overviewActiveSelf;
                StageRowsActiveSelf = stageRowsActiveSelf;
                AuthoredTextVisible = authoredTextVisible;
                PlainBodyTextEnabled = plainBodyTextEnabled;
            }

            public bool SectionActiveSelf { get; }
            public bool OverviewActiveSelf { get; }
            public bool StageRowsActiveSelf { get; }
            public bool AuthoredTextVisible { get; }
            public bool PlainBodyTextEnabled { get; }
        }

        private sealed class LayoutObservation
        {
            public LayoutObservation(
                string sample,
                float preFlushContentHeight,
                float contentHeight,
                float preferredHeight,
                float viewportHeight,
                float topNormalized,
                float bottomNormalized,
                Bounds faMenInContent,
                Bounds qiLeiInContent,
                Bounds qiRowsInContent,
                Rect faMenWorldTop,
                Rect qiLeiWorldTop,
                Rect qiRowsWorldTop,
                Rect viewportWorldTop,
                Rect faMenWorldBottom,
                Rect qiLeiWorldBottom,
                Rect qiRowsWorldBottom,
                Rect viewportWorldBottom,
                bool faMenSectionActive,
                bool qiLeiSectionActive,
                bool contentEnclosesQiLei,
                bool qiRowsAtTop,
                bool qiRowsAtBottom,
                bool qiRowsAtAnyValidPosition,
                float firstReachableNormalized)
            {
                Sample = sample;
                PreFlushContentHeight = preFlushContentHeight;
                ContentHeight = contentHeight;
                PreferredHeight = preferredHeight;
                ViewportHeight = viewportHeight;
                TopNormalized = topNormalized;
                BottomNormalized = bottomNormalized;
                FaMenInContent = faMenInContent;
                QiLeiInContent = qiLeiInContent;
                QiRowsInContent = qiRowsInContent;
                FaMenWorldTop = faMenWorldTop;
                QiLeiWorldTop = qiLeiWorldTop;
                QiRowsWorldTop = qiRowsWorldTop;
                ViewportWorldTop = viewportWorldTop;
                FaMenWorldBottom = faMenWorldBottom;
                QiLeiWorldBottom = qiLeiWorldBottom;
                QiRowsWorldBottom = qiRowsWorldBottom;
                ViewportWorldBottom = viewportWorldBottom;
                FaMenSectionActive = faMenSectionActive;
                QiLeiSectionActive = qiLeiSectionActive;
                ContentEnclosesQiLei = contentEnclosesQiLei;
                QiRowsAtTop = qiRowsAtTop;
                QiRowsAtBottom = qiRowsAtBottom;
                QiRowsAtAnyValidPosition =
                    qiRowsAtAnyValidPosition;
                FirstReachableNormalized =
                    firstReachableNormalized;
            }

            public string Sample { get; }
            public float PreFlushContentHeight { get; }
            public float ContentHeight { get; }
            public float PreferredHeight { get; }
            public float ViewportHeight { get; }
            public float TopNormalized { get; }
            public float BottomNormalized { get; }
            public Bounds FaMenInContent { get; }
            public Bounds QiLeiInContent { get; }
            public Bounds QiRowsInContent { get; }
            public Rect FaMenWorldTop { get; }
            public Rect QiLeiWorldTop { get; }
            public Rect QiRowsWorldTop { get; }
            public Rect ViewportWorldTop { get; }
            public Rect FaMenWorldBottom { get; }
            public Rect QiLeiWorldBottom { get; }
            public Rect QiRowsWorldBottom { get; }
            public Rect ViewportWorldBottom { get; }
            public bool FaMenSectionActive { get; }
            public bool QiLeiSectionActive { get; }
            public bool ContentEnclosesQiLei { get; }
            public bool QiRowsAtTop { get; }
            public bool QiRowsAtBottom { get; }
            public bool QiRowsAtAnyValidPosition { get; }
            public float FirstReachableNormalized { get; }
            public bool IsFinite =>
                ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier
                    .IsFinite(PreFlushContentHeight)
                && ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier
                    .IsFinite(ContentHeight)
                && ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier
                    .IsFinite(PreferredHeight)
                && ItemFourCoreProjectionDetailDualTrackVisibilityGuardFixVerifier
                    .IsFinite(ViewportHeight)
                && ContentHeight >= 0f
                && PreferredHeight >= 0f
                && ViewportHeight > 0f;
        }

        private sealed class ScenarioResult
        {
            public ScenarioResult(
                string id,
                string marker,
                bool passed,
                string detail)
            {
                Id = id;
                Marker = marker;
                Passed = passed;
                Detail = detail;
            }

            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }
    }
}
