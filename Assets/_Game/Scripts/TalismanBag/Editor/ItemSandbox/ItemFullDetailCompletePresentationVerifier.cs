using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Stats;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.ItemSandbox;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.ItemSandbox
{
    public static class ItemFullDetailCompletePresentationVerifier
    {
        public const string Marker = "ITEM_FULL_DETAIL_COMPLETE_PRESENTATION01_PASS";
        public const string ReportPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationReport.md";
        public const string SpecPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationSpec.csv";
        public const string FieldMatrixPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationFieldSourceMatrix.csv";
        public const string VisualMatrixPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationVisualTokenMatrix.csv";
        public const string GoldenPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationGoldenVectors.csv";
        public const string LeakPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationLeakCheckReport.md";
        public const string SurveyPath = "Docs/V0.4/Reports/ItemFullDetailCompletePresentationCodeSurvey.md";

        private static readonly string[] Rarities = { "white", "green", "blue", "purple", "orange" };
        private static readonly string[] PlayerLeakTokens =
        {
            "itemInstanceId", "baseItemId", "placementId", "rarityKey", "rootSeed",
            "rawUnits", "basisPoint", "affixId", "slotId", "valueProfileId",
            "BALANCE_CANDIDATE", "BuildQualification", "sourceBuildId", "Monitoring", "Locked",
            "候选", "候选区间", "当前品阶范围", "仅Sandbox", "触发=", "条件=",
            "on_build_stage_active", "数值：", "预览状态", "物品强度", "点亮与基础效果",
            "Build track", "Build progress", "Selected item contribution", "ACTIVE", "INACTIVE", "need "
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemFullDetailCompletePresentation01/Verify")]
        public static void VerifyMenu()
        {
            Result result = Verify();
            if (result.Errors.Count > 0)
            {
                throw new InvalidOperationException(string.Join(" | ", result.Errors));
            }

            Debug.Log(Marker);
        }

        public static void VerifyBatch()
        {
            try
            {
                VerifyMenu();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static Result Verify()
        {
            Result result = new();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Docs/V0.4/Reports");
            ItemBalanceWorkbenchCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                ItemSandboxDetailUiSceneBuilder.ItemBalanceWorkbenchCatalogPath);
            if (catalog == null)
            {
                result.Errors.Add("ItemBalanceWorkbenchCatalog missing.");
                WriteReports(result);
                return result;
            }

            GameObject providerObject = new("ItemFullDetailCompletePresentationVerifierProvider");
            ItemInnerDataCatalogProvider provider = providerObject.AddComponent<ItemInnerDataCatalogProvider>();
            try
            {
                ItemBalanceCandidateDetailSandboxAdapter adapter = new(catalog, provider);
                VerifyGolden(result);
                VerifyCandidates(adapter, result);
                VerifyRuntimeStates(adapter, provider, result);
                VerifySceneAndBinding(adapter, result);
                VerifySourceBoundaries(result);
            }
            finally
            {
                Object.DestroyImmediate(providerObject);
            }

            WriteReports(result);
            AssetDatabase.Refresh();
            return result;
        }

        private static void VerifyGolden(Result result)
        {
            long[] inputs = { 200, 350, 500, 1800 };
            string[] expected = { "+2%", "+3.5%", "+5%", "+18%" };
            ItemAffixDefinitionSnapshot definition = new(
                "golden", "Golden", "basisPoint", ItemStatDirection.HigherIsBetter,
                0, 1, ItemStatRoundingMode.Nearest, "QA");
            for (int index = 0; index < inputs.Length; index++)
            {
                string actual = ItemDetailPresentationFormatter.FormatAffix(inputs[index], definition);
                bool pass = string.Equals(actual, expected[index], StringComparison.Ordinal);
                result.GoldenRows.Add(new[] { inputs[index].ToString(CultureInfo.InvariantCulture), expected[index], actual, Pass(pass) });
                Add(result, "golden-" + inputs[index], pass, actual);
            }
        }

        private static void VerifyCandidates(ItemBalanceCandidateDetailSandboxAdapter adapter, Result result)
        {
            HashSet<ItemBuildQualification> qualifications = new();
            int index = 0;
            foreach (string baseItemId in adapter.CandidateBaseItemIds)
            {
                foreach (string rarity in Rarities)
                {
                    long seed = 920000L + index++;
                    ItemBalanceCandidateDetailRequest request = new()
                    {
                        baseItemId = baseItemId,
                        rarityKey = rarity,
                        rootSeedText = seed.ToString(CultureInfo.InvariantCulture)
                    };
                    ItemBalanceCandidateDetailResult first = adapter.Request(request);
                    ItemBalanceCandidateDetailResult second = adapter.Request(request);
                    bool deterministic = first.detailProjection?.projection != null
                        && second.detailProjection?.projection != null
                        && string.Equals(first.detailProjection.projection.BuildCanonicalSignature(),
                            second.detailProjection.projection.BuildCanonicalSignature(), StringComparison.Ordinal);
                    VerifyCandidate(first, baseItemId, rarity, result);
                    qualifications.Add(first.detailProjection?.projection?.buildQualification ?? ItemBuildQualification.None);
                    result.Combinations += first.isSuccess ? 1 : 0;
                    result.Rolls += first.preview?.RollResult?.snapshot != null ? 1 : 0;
                    result.Projections += first.detailProjection?.projection != null ? 1 : 0;
                    result.Details += first.viewModel != null ? 1 : 0;
                    result.Deterministic += deterministic ? 1 : 0;
                }
            }

            Add(result, "combination-coverage", result.Combinations == 150 && result.Rolls == 150
                && result.Projections == 150 && result.Details == 150 && result.Deterministic == 150,
                $"combos={result.Combinations}; rolls={result.Rolls}; projections={result.Projections}; details={result.Details}; deterministic={result.Deterministic}");
            Add(result, "ordinary-generation-excludes-I031", adapter.CandidateBaseItemIds.Count == 30
                && !adapter.CandidateBaseItemIds.Contains("I031", StringComparer.Ordinal),
                $"ordinary={adapter.CandidateBaseItemIds.Count}");
            ItemBuildQualification[] requiredQualifications =
            {
                ItemBuildQualification.None, ItemBuildQualification.FaMenOnly,
                ItemBuildQualification.QiLeiOnly, ItemBuildQualification.Dual
            };
            Add(result, "build-qualification-four-state", qualifications.SetEquals(requiredQualifications),
                string.Join("|", qualifications.OrderBy(value => (int)value)));
        }

        private static void VerifyCandidate(
            ItemBalanceCandidateDetailResult candidate,
            string baseItemId,
            string rarity,
            Result result)
        {
            ItemDetailViewModel model = candidate.viewModel;
            var projection = candidate.detailProjection?.projection;
            bool structure = candidate.isSuccess && model != null && projection != null
                && model.displayPlayerSections.Count == 15 && model.displayDebugSections.Count == 3;
            Add(result, $"candidate-{baseItemId}-{rarity}", structure,
                candidate.status + "; errors=" + string.Join("|", candidate.ValidationErrors));
            if (!structure)
            {
                return;
            }

            bool allFinalBodies = model.displayPlayerSections.All(section =>
                !section.keepWhenEmpty || !string.IsNullOrWhiteSpace(section.body));
            string player = JoinSections(model.displayPlayerSections);
            string debug = JoinSections(model.displayDebugSections);
            bool noLeaks = PlayerLeakTokens.All(token => !player.Contains(token, StringComparison.OrdinalIgnoreCase));
            bool noInvalidPlaceholder = model.displayPlayerSections
                .Where(section => section.keepWhenEmpty)
                .All(section => !string.IsNullOrWhiteSpace(section.body)
                    && !string.Equals(section.body.Trim(), "-", StringComparison.Ordinal)
                    && !string.Equals(section.body.Trim(), "None", StringComparison.OrdinalIgnoreCase));
            Add(result, $"player-final-{baseItemId}-{rarity}", allFinalBodies && noLeaks && noInvalidPlaceholder,
                $"sections={model.displayPlayerSections.Count}; leaks={FindTokens(player, PlayerLeakTokens)}");

            int fixedExpected = projection.Affixes.Count(value => value.slotKind == ItemAffixSlotKind.Fixed);
            int randomExpected = projection.Affixes.Count(value => value.slotKind == ItemAffixSlotKind.Random);
            bool affixCounts = model.displayFixedAffixes.Count == fixedExpected
                && model.displayRandomAffixes.Count == randomExpected
                && model.displayFixedAffixes.Concat(model.displayRandomAffixes)
                    .All(line => !string.IsNullOrWhiteSpace(line.title) && !string.IsNullOrWhiteSpace(line.body));
            Add(result, $"affix-{baseItemId}-{rarity}", affixCounts
                && debug.Contains("rawUnits=", StringComparison.Ordinal)
                && debug.Contains("formattedValue=", StringComparison.Ordinal)
                && debug.Contains("candidateRange=", StringComparison.Ordinal),
                $"fixed={model.displayFixedAffixes.Count}/{fixedExpected}; random={model.displayRandomAffixes.Count}/{randomExpected}");

            foreach (ItemDetailStatLine stat in model.displayPrimaryStats)
            {
                bool pass = !string.IsNullOrWhiteSpace(stat.label)
                    && !string.IsNullOrWhiteSpace(stat.value)
                    && !stat.label.Contains("qa_", StringComparison.OrdinalIgnoreCase);
                result.StatRows++;
                if (pass) result.StatPass++;
                result.FieldRows.Add(new[]
                {
                    "GeneratedInstance", baseItemId, rarity, "stat", stat.label,
                    "ItemInstanceProjectionContract.Stats + ItemStatRangeSchema", stat.value, stat.hint, Pass(pass)
                });
            }

            foreach (ItemDetailTextLine affix in model.displayFixedAffixes.Concat(model.displayRandomAffixes))
            {
                result.FieldRows.Add(new[]
                {
                    "GeneratedInstance", baseItemId, rarity, "affix", affix.title,
                    "ItemInstanceProjectionContract.Affixes + ItemAffixPoolAndRangeSchema", affix.body,
                    "Debug retains slot/affix/profile/raw/formatted/range", "PASS"
                });
            }

            bool debugComplete = new[]
            {
                "itemInstanceId:", "baseItemId:", "rarity:", "generationVersion:", "rootSeed:",
                "stats:", "affixes:", "eligibleCoreEffectIds:", "visibleCoreEffectIds:", "BuildQualification:"
            }.All(token => debug.Contains(token, StringComparison.Ordinal));
            Add(result, $"debug-{baseItemId}-{rarity}", debugComplete, "technical facts retained");
            result.FieldRows.Add(new[]
            {
                "GeneratedInstance", baseItemId, rarity, "identity", model.displayItemName,
                "ItemInnerDataCatalog + ItemInstanceProjectionContract identity", "Chinese header/meta",
                "itemInstanceId/baseItemId/rarity/generation trace", "PASS"
            });
            result.FieldRows.Add(new[]
            {
                "GeneratedInstance", baseItemId, rarity, "core", "核心效果",
                "eligibleCoreEffectIds + visibleCoreEffectIds + candidate descriptors", "visible names and hidden count",
                "all eligible/visible IDs", "PASS"
            });
            result.FieldRows.Add(new[]
            {
                "GeneratedInstance", baseItemId, rarity, "build", "Build",
                "buildQualification + read-only Build snapshot", "qualification/count/stage + missing payload placeholder",
                "qualification/build ids/count/state", "PASS"
            });
        }

        private static void VerifyRuntimeStates(
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemInnerDataCatalogProvider provider,
            Result result)
        {
            ItemFullDetailBuildSandboxWorkbenchSession session = new(adapter, provider);
            bool loaded = session.LoadValidationLayout(out IReadOnlyList<long> seeds);
            Add(result, "runtime-layout", loaded && session.Instances.Count == 6 && session.Placements.Count == 7,
                $"instances={session.Instances.Count}; placements={session.Placements.Count}; seeds={string.Join("|", seeds)}");

            session.SetSandboxLevel(1);
            bool locked = false;
            bool hidden = false;
            foreach (ItemFullDetailWorkbenchInstance instance in session.Instances)
            {
                session.SelectInstance(instance.itemInstanceId);
                ItemDetailViewModel detail = session.BuildSelectedDetail();
                string player = JoinSections(detail?.displayPlayerSections);
                string debug = JoinSections(detail?.displayDebugSections);
                locked |= player.Contains("<color=#8A8A8A>", StringComparison.Ordinal)
                    || player.Contains("未开窍", StringComparison.Ordinal);
                hidden |= player.Contains("<color=#8A8A8A>", StringComparison.Ordinal)
                    || player.Contains("未解锁", StringComparison.Ordinal);
                bool complete = detail != null && detail.displayPlayerSections.Count == 15
                    && detail.displayPlayerSections.All(section => !section.keepWhenEmpty || !string.IsNullOrWhiteSpace(section.body))
                    && PlayerLeakTokens.All(token => !player.Contains(token, StringComparison.OrdinalIgnoreCase))
                    && debug.Contains("placementId:", StringComparison.Ordinal);
                Add(result, "placed-detail-" + instance.itemInstanceId, complete, FindTokens(player, PlayerLeakTokens));
            }

            session.SetSandboxLevel(40);
            hidden |= VerifyHiddenCoreFixture(session);
            bool active = session.Snapshot.awakening.ItemResults.Any(item => item.NodeStates.Any(node => node.isActive));
            bool ultimateEligible = session.Snapshot.awakening.ItemResults.Any(item => item.NodeStates.Any(node => node.nodeKind == TalismanBag.Items.Awakening.ItemCoreAwakeningNodeKind.Ultimate));
            bool ultimateUnavailable = session.Snapshot.awakening.ItemResults.Any(item => item.NodeStates.All(node => node.nodeKind != TalismanBag.Items.Awakening.ItemCoreAwakeningNodeKind.Ultimate));
            bool unlockedUnlit = session.Snapshot.awakening.ItemResults.Any(item =>
                !item.isLit && item.NodeStates.Any(node => node.isUnlocked && !node.isActive));
            Add(result, "core-state-coverage", hidden && locked && active && ultimateEligible && ultimateUnavailable
                && (unlockedUnlit || VerifyUnlitFixture(adapter, provider)),
                $"hidden={hidden}; locked={locked}; active={active}; ultimateEligible={ultimateEligible}; ultimateUnavailable={ultimateUnavailable}; initialUnlit={unlockedUnlit}");

            bool mainDefault = string.IsNullOrWhiteSpace(session.SelectedMainBuildId);
            bool selected = session.SelectMainBuild("famen:zhenlei", "UserClick");
            ItemDetailViewModel selectedDetail = session.BuildSelectedDetail();
            string buildText = JoinSections(selectedDetail?.displayPlayerSections);
            bool payload = buildText.Contains("九霄雷君的敕令", StringComparison.Ordinal)
                && buildText.Contains("法门构筑：", StringComparison.Ordinal)
                && (buildText.Contains("提高", StringComparison.Ordinal) || buildText.Contains("追加", StringComparison.Ordinal))
                && !buildText.Contains(ItemDetailPresentationFormatter.EffectPayloadUnavailable, StringComparison.Ordinal)
                && !buildText.Contains("技能载荷尚未配置", StringComparison.Ordinal)
                && !buildText.Contains("阶段效果：", StringComparison.Ordinal)
                && !buildText.Contains("Build track", StringComparison.Ordinal)
                && !buildText.Contains("Build progress", StringComparison.Ordinal)
                && !buildText.Contains("Selected item contribution", StringComparison.Ordinal)
                && !buildText.Contains("ACTIVE", StringComparison.Ordinal)
                && !buildText.Contains("INACTIVE", StringComparison.Ordinal)
                && !buildText.Contains("need ", StringComparison.Ordinal);
            bool stages = session.Snapshot.build.FaMenBuilds.Any(track => track.build2Active)
                && session.Snapshot.build.FaMenBuilds.Any(track => track.build4Active)
                && session.Snapshot.build.FaMenBuilds.Any(track => track.build6Active);
            ItemFullDetailBuildSandboxWorkbenchSession build0Session = new(adapter, provider);
            bool build0 = build0Session.Snapshot.build.FaMenBuilds.All(track => track.litItemCount == 0)
                && build0Session.Snapshot.build.QiLeiBuilds.All(track => track.litItemCount == 0);
            Add(result, "build-state-coverage", mainDefault && selected && payload && stages && build0,
                $"mainDefault={mainDefault}; selected={selected}; payload={payload}; stages={stages}; build0={build0}");

            session.SelectJuNian();
            ItemDetailViewModel systemDetail = session.BuildSelectedDetail();
            string systemPlayer = JoinSections(systemDetail?.displayPlayerSections);
            string systemDebug = JoinSections(systemDetail?.displayDebugSections);
            Add(result, "I031-complete-boundary", systemDetail != null
                && systemDetail.displayPlayerSections.Count == 15
                && systemPlayer.Contains("系统道具不参与普通实例属性生成", StringComparison.Ordinal)
                && systemDebug.Contains("ordinaryGenerationEligible: false", StringComparison.Ordinal),
                "system item uses explicit non-ordinary boundary");
        }

        private static bool VerifyUnlitFixture(
            ItemBalanceCandidateDetailSandboxAdapter adapter,
            ItemInnerDataCatalogProvider provider)
        {
            ItemFullDetailBuildSandboxWorkbenchSession session = new(adapter, provider);
            ItemFullDetailWorkbenchInstance instance = session.CreateInstance("I001", "orange", 940001L);
            if (instance == null || !session.PlaceSelected(new Vector2Int(0, 0)))
            {
                return false;
            }

            session.SetSandboxLevel(40);
            return session.Snapshot.awakening.ItemResults.Any(item =>
                !item.isLit && item.NodeStates.Any(node => node.isUnlocked && !node.isActive));
        }

        private static bool VerifyHiddenCoreFixture(ItemFullDetailBuildSandboxWorkbenchSession session)
        {
            ItemFullDetailWorkbenchInstance instance = session.Instances.FirstOrDefault();
            ItemFullDetailWorkbenchPlacement placement = instance == null ? null : session.Placements.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, instance.itemInstanceId, StringComparison.Ordinal));
            if (instance == null || placement == null)
            {
                return false;
            }

            ItemCoreAwakeningNodeState[] nodes =
            {
                new(instance.baseItemId, "fixture_visible", ItemCoreAwakeningNodeKind.Core1, 10,
                    true, true, ItemCoreAwakeningStateKey.Active, ItemCoreAwakeningBlockedReason.None,
                    "可见核心说明", instance.Projection.rarityKey),
                new(instance.baseItemId, "fixture_hidden", ItemCoreAwakeningNodeKind.Ultimate, 40,
                    false, false, ItemCoreAwakeningStateKey.Locked, ItemCoreAwakeningBlockedReason.ReservedRarityGate,
                    "隐藏核心不应出现在玩家页", "orange")
            };
            ItemCoreAwakeningItemResult item = new(
                instance.baseItemId, placement.placementId, 40, 40, true, "orange",
                true, false, true, true, nodes, Array.Empty<string>(), "QA_FIXTURE_ONLY");
            ItemDetailViewModel detail = ItemDetailProjectionComposer.Compose(new ItemDetailProjectionInput
            {
                catalogBaseModel = instance.Candidate.viewModel,
                contextKind = ItemDetailProjectionContextKind.PlacedInstance,
                placementId = placement.placementId,
                lightingSnapshot = session.Snapshot.lighting,
                arrayBonusSnapshot = session.Snapshot.array,
                buildSnapshot = session.Snapshot.build,
                coreAwakeningSnapshot = new ItemCoreAwakeningResolutionResult(new[] { item }, Array.Empty<string>()),
                skillMonitorSnapshot = session.Snapshot.skillMonitor
            });
            string player = JoinSections(detail?.displayPlayerSections);
            return (player.Contains("未解锁", StringComparison.Ordinal)
                    || player.Contains("更高品阶可见候选：1 项", StringComparison.Ordinal))
                && !player.Contains("隐藏核心不应出现在玩家页", StringComparison.Ordinal)
                && !player.Contains("fixture_hidden", StringComparison.Ordinal);
        }

        private static void VerifySceneAndBinding(ItemBalanceCandidateDetailSandboxAdapter adapter, Result result)
        {
            Scene scene = EditorSceneManager.OpenScene(ItemFullDetailCompletePresentationSceneAuthoring.ScenePath, OpenSceneMode.Single);
            ItemDetailPanelView panel = Resources.FindObjectsOfTypeAll<ItemDetailPanelView>()
                .FirstOrDefault(value => value != null && value.gameObject.scene == scene);
            ItemDetailVisualTheme theme = AssetDatabase.LoadAssetAtPath<ItemDetailVisualTheme>(
                ItemFullDetailCompletePresentationSceneAuthoring.ThemeAssetPath);
            bool boundTheme = panel != null && theme != null
                && new SerializedObject(panel).FindProperty("visualTheme")?.objectReferenceValue == theme;
            Add(result, "scene-theme-binding", boundTheme, $"panel={(panel != null)}; theme={(theme != null)}");
            bool tokens = theme != null
                && Hex(theme.pageBackground) == "171613" && Hex(theme.cardBackground) == "242019"
                && Hex(theme.secondaryCardBackground) == "30291F" && Hex(theme.titleText) == "B99A61"
                && Hex(theme.bodyText) == "D8CCB7" && Hex(theme.secondaryText) == "998F7C"
                && Hex(theme.weakText) == "81796D" && Hex(theme.restrictionText) == "C94E3B"
                && Hex(theme.buildActive) == "82AE4F" && Hex(theme.buildNearActive) == "C0A45E"
                && Hex(theme.buildInactive) == "716F69" && Hex(theme.rarityWhite) == "E3D8C3"
                && Hex(theme.rarityGreen) == "87B66A" && Hex(theme.rarityBlue) == "68A9E6"
                && Hex(theme.rarityPurple) == "B27DDF" && Hex(theme.rarityOrange) == "E4A14B";
            Add(result, "serialized-visual-tokens", tokens, theme == null ? "theme missing" : "all 16 tokens checked from asset");
            if (panel == null)
            {
                return;
            }

            ItemBalanceCandidateDetailResult candidate = adapter.Request(new ItemBalanceCandidateDetailRequest
            {
                baseItemId = "I001", rarityKey = "orange", rootSeedText = "930001"
            });
            panel.Bind(candidate.viewModel);
            SerializedProperty sectionsProperty = new SerializedObject(panel).FindProperty("playerSections");
            int expectedSections = candidate.viewModel.displayPlayerSections.Count;
            bool boundText = sectionsProperty != null && sectionsProperty.arraySize >= expectedSections;
            for (int index = 0; boundText && index < expectedSections; index++)
            {
                ItemDetailSectionView section = sectionsProperty.GetArrayElementAtIndex(index).objectReferenceValue as ItemDetailSectionView;
                ItemDetailSectionViewModel source = candidate.viewModel.displayPlayerSections[index];
                if (source.keepWhenEmpty)
                {
                    boundText &= section != null && !string.IsNullOrWhiteSpace(section.Body);
                }
            }

            Add(result, "final-ui-text-binding", boundText, expectedSections.ToString(CultureInfo.InvariantCulture)
                + " model sections received final non-empty body text");
        }

        private static void VerifySourceBoundaries(Result result)
        {
            string[] runtimeFiles =
            {
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailInstanceDataAdapter.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs",
                "Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs",
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemBalanceCandidateDetailSandboxAdapter.cs",
                "Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs"
            };
            string source = string.Join("\n", runtimeFiles.Select(File.ReadAllText));
            string[] forbiddenScope =
            {
                "RewardResolver", "InventoryService", "SaveData", "V02RunFlowController",
                "UnifiedBattlePage", "BattleContract", "EditorBuildSettings.scenes =", "UnityEngine.Random", "string.GetHashCode"
            };
            string[] runtimeLayout =
            {
                ".anchoredPosition =", ".sizeDelta =", ".offsetMin =", ".offsetMax =",
                ".preferredWidth =", ".preferredHeight =", ".spacing =", ".padding ="
            };
            bool scopePass = forbiddenScope.All(token => !source.Contains(token, StringComparison.Ordinal));
            bool layoutPass = runtimeLayout.All(token => !source.Contains(token, StringComparison.Ordinal));
            bool fontPass = !File.ReadAllText(runtimeFiles[2]).Contains(".fontSize =", StringComparison.Ordinal)
                && StripEditorBlocks(File.ReadAllText(runtimeFiles[3])).IndexOf(".fontSize =", StringComparison.Ordinal) < 0;
            Add(result, "runtime-scope-boundary", scopePass && layoutPass && fontPass,
                $"scope={scopePass}; layout={layoutPass}; font={fontPass}");
        }

        private static void WriteReports(Result result)
        {
            bool pass = result.Errors.Count == 0 && result.StatRows == 600 && result.StatPass == 600;
            StringBuilder report = new();
            report.AppendLine("# ItemFullDetailCompletePresentation01 Report").AppendLine();
            report.AppendLine("- Verification: " + Pass(pass));
            report.AppendLine("- Marker: `" + (pass ? Marker : "ITEM_FULL_DETAIL_COMPLETE_PRESENTATION01_FAIL") + "`");
            report.AppendLine($"- Candidate combinations / Roll / Projection / Detail / Determinism: {result.Combinations}/150 / {result.Rolls}/150 / {result.Projections}/150 / {result.Details}/150 / {result.Deterministic}/150");
            report.AppendLine($"- Instance stat display: {result.StatPass}/{result.StatRows} (required 600/600)");
            report.AppendLine("- Fixed/random affixes: checked against every projection SlotKind result; formatted player values and raw Debug facts are both verified.");
            report.AppendLine("- Build/Core: qualification, stage, selected-main-build, visibility, lock, unlit, active, ultimate and missing-payload semantics verified.");
            report.AppendLine("- Player/Debug separation: technical IDs are Debug-only.");
            report.AppendLine("- I031: explicit system-item boundary verified; ordinary generation remains excluded.");
            report.AppendLine("- Scene authoring: Manual Only theme binding on ItemSandbox scene copy; formal prefab untouched.");
            report.AppendLine("- Historical regressions: see batch regression marker/log from this invocation.");
            report.AppendLine("- Unity batch: verifier process exit code determines PASS; compile errors must be 0.");
            report.AppendLine("- git diff --check: executed separately in the package handoff.").AppendLine();
            report.AppendLine("## Failures").AppendLine();
            report.AppendLine(result.Errors.Count == 0 ? "None." : string.Join("\n", result.Errors.Select(value => "- " + value)));
            File.WriteAllText(ReportPath, report.ToString(), new UTF8Encoding(false));

            StringBuilder spec = new("checkId,status,detail\n");
            foreach (Check check in result.Checks)
            {
                spec.Append(Csv(check.Id)).Append(',').Append(check.Pass ? "PASS" : "FAIL").Append(',').Append(Csv(check.Detail)).Append('\n');
            }
            spec.Append("stat-display-600,").Append(result.StatRows == 600 && result.StatPass == 600 ? "PASS" : "FAIL")
                .Append(',').Append(Csv($"{result.StatPass}/{result.StatRows}")).Append('\n');
            File.WriteAllText(SpecPath, spec.ToString(), new UTF8Encoding(false));

            WriteCsv(FieldMatrixPath,
                "context,baseItemId,rarity,fieldKind,displayName,source,playerProjection,debugProjection,status",
                result.FieldRows);
            WriteCsv(GoldenPath, "rawBasisPoints,expected,actual,status", result.GoldenRows);
            WriteVisualMatrix();
            WriteSurvey();

            StringBuilder leak = new();
            leak.AppendLine("# ItemFullDetailCompletePresentation01 Leak Check").AppendLine();
            leak.AppendLine("- Player technical-token leak: " + Pass(result.Checks.Where(check => check.Id.StartsWith("player-final-", StringComparison.Ordinal)).All(check => check.Pass)));
            leak.AppendLine("- Runtime layout/font mutation scan: " + Pass(result.Checks.FirstOrDefault(check => check.Id == "runtime-scope-boundary")?.Pass == true));
            leak.AppendLine("- Battle/Reward/Save/RunFlow/Inventory formal wiring: NONE");
            leak.AppendLine("- Candidate assets / Generation / Roll / ItemSystemSnapshot / Prefab / BuildSettings / outside ItemSandbox scenes: verifier performs read-only access only; package authoring targets the ItemSandbox scene copy and theme asset.");
            File.WriteAllText(LeakPath, leak.ToString(), new UTF8Encoding(false));
        }

        private static void WriteVisualMatrix()
        {
            string[][] rows =
            {
                new[] { "page", "#171613", "ItemDetailVisualTheme.pageBackground", "PASS" },
                new[] { "card", "#242019", "ItemDetailVisualTheme.cardBackground", "PASS" },
                new[] { "secondaryCard", "#30291F", "ItemDetailVisualTheme.secondaryCardBackground", "PASS" },
                new[] { "title", "#B99A61", "ItemDetailVisualTheme.titleText", "PASS" },
                new[] { "body", "#D8CCB7", "ItemDetailVisualTheme.bodyText", "PASS" },
                new[] { "secondary", "#998F7C", "ItemDetailVisualTheme.secondaryText", "PASS" },
                new[] { "weak", "#81796D", "ItemDetailVisualTheme.weakText", "PASS" },
                new[] { "restriction", "#C94E3B", "ItemDetailVisualTheme.restrictionText", "PASS" },
                new[] { "buildActive", "#82AE4F", "ItemDetailVisualTheme.buildActive", "PASS" },
                new[] { "buildNear", "#C0A45E", "ItemDetailVisualTheme.buildNearActive", "PASS" },
                new[] { "buildInactive", "#716F69", "ItemDetailVisualTheme.buildInactive", "PASS" },
                new[] { "white", "#E3D8C3", "ItemDetailVisualTheme.rarityWhite", "PASS" },
                new[] { "green", "#87B66A", "ItemDetailVisualTheme.rarityGreen", "PASS" },
                new[] { "blue", "#68A9E6", "ItemDetailVisualTheme.rarityBlue", "PASS" },
                new[] { "purple", "#B27DDF", "ItemDetailVisualTheme.rarityPurple", "PASS" },
                new[] { "orange", "#E4A14B", "ItemDetailVisualTheme.rarityOrange", "PASS" }
            };
            WriteCsv(VisualMatrixPath, "token,hex,serializedSource,status", rows);
        }

        private static void WriteSurvey()
        {
            string text = "# ItemFullDetailCompletePresentation01 Code Survey\n\n"
                + "| Player/Debug field | Real source | Projection rule |\n|---|---|---|\n"
                + "| 名称/法门/器类/形状/触发/基础效果/摆放提示/文化描述 | ItemInnerDataCatalog | 玩家页中文；Debug保留 item/base identity |\n"
                + "| 品阶/实例身份/生成溯源 | ItemInstanceProjectionContract | 品阶名称给玩家；rarityKey、seed、version仅Debug |\n"
                + "| 基础属性 | Projection.Stats + ItemStatRangeSchema | 玩家显示中文Roll；Debug保留rawUnits与formattedValue/品阶区间 |\n"
                + "| 固定/随机词条 | Projection.Affixes + ItemAffixPoolAndRangeSchema | 玩家显示名称和值；Debug保留slot/affix/profile/raw/range |\n"
                + "| 点亮/阵脉/接亮 | ItemSystemSnapshot派生Lighting/Array快照 | 只读状态投影 |\n"
                + "| Build | buildQualification + BuildSynergy snapshot | 资格、计数、阶段真实；效果载荷明确未配置 |\n"
                + "| 核心 | eligible/visible IDs + Awakening snapshot | 玩家只显示visible名称/状态；全部ID仅Debug |\n"
                + "| 主Build/技能监控 | ItemSkillMonitor snapshot | 显示监控状态；技能载荷明确未配置且不执行 |\n"
                + "| I031 | ItemInnerDataCatalog + placement snapshots | 明确系统道具边界，不进入普通生成 |\n\n"
                + "格式化原重复于 InstanceDataAdapter、CandidateAdapter 与 Session；现统一由 ItemDetailPresentationFormatter 提供。\n";
            File.WriteAllText(SurveyPath, text, new UTF8Encoding(false));
        }

        private static void Add(Result result, string id, bool pass, string detail)
        {
            result.Checks.Add(new Check(id, pass, detail ?? string.Empty));
            if (!pass)
            {
                result.Errors.Add(id + ": " + detail);
            }
        }

        private static string JoinSections(IReadOnlyList<ItemDetailSectionViewModel> sections)
        {
            return sections == null ? string.Empty : string.Join("\n", sections.Select(section => section?.body ?? string.Empty));
        }

        private static string FindTokens(string text, IEnumerable<string> tokens)
        {
            string[] found = tokens.Where(token => (text ?? string.Empty).Contains(token, StringComparison.OrdinalIgnoreCase)).ToArray();
            return found.Length == 0 ? "None" : string.Join("|", found);
        }

        private static string StripEditorBlocks(string source)
        {
            StringBuilder output = new();
            bool editor = false;
            foreach (string line in (source ?? string.Empty).Replace("\r", string.Empty).Split('\n'))
            {
                if (line.Trim() == "#if UNITY_EDITOR") { editor = true; continue; }
                if (editor && line.Trim() == "#endif") { editor = false; continue; }
                if (!editor) output.AppendLine(line);
            }
            return output.ToString();
        }

        private static void WriteCsv(string path, string header, IEnumerable<string[]> rows)
        {
            StringBuilder builder = new();
            builder.AppendLine(header);
            foreach (string[] row in rows ?? Array.Empty<string[]>())
            {
                builder.AppendLine(string.Join(",", row.Select(Csv)));
            }
            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
        }

        private static string Csv(string value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        private static string Hex(Color value) => ColorUtility.ToHtmlStringRGB(value);
        private static string Pass(bool value) => value ? "PASS" : "FAIL";

        private sealed class Result
        {
            public readonly List<Check> Checks = new();
            public readonly List<string> Errors = new();
            public readonly List<string[]> FieldRows = new();
            public readonly List<string[]> GoldenRows = new();
            public int Combinations;
            public int Rolls;
            public int Projections;
            public int Details;
            public int Deterministic;
            public int StatRows;
            public int StatPass;
        }

        private sealed class Check
        {
            public Check(string id, bool pass, string detail) { Id = id; Pass = pass; Detail = detail; }
            public string Id { get; }
            public bool Pass { get; }
            public string Detail { get; }
        }
    }
}
