using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemBalance
{
    public sealed class ItemBalanceWorkbenchWindow : EditorWindow
    {
        private static readonly string[] Tabs =
        {
            "Overview", "Stat Dictionary", "Item Ranges", "Candidate Item Power",
            "Fixed Signature Affixes", "Random Affix Dictionary", "Item Random Pools",
            "Affix Effect Payloads", "Core Effects", "FaMen Build Effects",
            "QiLei Build Effects", "Drop Progression", "Detail Preview", "Validation"
        };

        private ItemBalanceWorkbenchCatalog catalog;
        private ItemBalanceProfile selected;
        private int tab;
        private Vector2 pageScroll;
        private Vector2 itemScroll;
        private string search = string.Empty;
        private string faMenFilter = "All";
        private string qiLeiFilter = "All";
        private string statFilter = "All";
        private string rarityFilter = "All";
        private long rootSeed = 150001;
        private ItemInstanceRarity previewRarity;
        private ItemBalancePreviewResult preview;
        private ItemBalanceValidationReport validation;
        private ItemBalanceCsvImportPreview importPreview;

        [MenuItem("Tools/Talisman Bag/V0.4/Data/[Manual Only] Item Balance Workbench")]
        public static void Open()
        {
            ItemBalanceWorkbenchWindow window = GetWindow<ItemBalanceWorkbenchWindow>();
            window.titleContent = new GUIContent("Item Balance Workbench");
            window.minSize = new Vector2(1100, 680);
            window.Show();
        }

        private void OnEnable()
        {
            LoadCatalog();
        }

        private void OnGUI()
        {
            if (catalog == null)
            {
                EditorGUILayout.HelpBox("Candidate catalog is missing. Create the 30 seeded profiles first.", MessageType.Warning);
                if (GUILayout.Button("Create Missing Candidate Assets", GUILayout.Height(34)))
                {
                    catalog = ItemBalanceCandidateSeedBuilder.BuildAssets(false);
                    SelectFirst();
                }
                return;
            }

            DrawHeader();
            DrawToolbar();
            tab = GUILayout.Toolbar(tab, Tabs, GUILayout.Height(28));
            EditorGUILayout.Space(4);
            switch (tab)
            {
                case 0: DrawOverview(); break;
                case 1: DrawCatalogProperty("statDefinitions", "Stat Definitions"); DrawCatalogProperty("higherBandTemplate", "HigherIsBetter Band Template"); DrawCatalogProperty("lowerBandTemplate", "LowerIsBetter Band Template"); break;
                case 2: DrawItemRanges(); break;
                case 3: DrawCandidateItemPower(); break;
                case 4: DrawFixedSignatureAffix(); break;
                case 5: DrawCatalogProperty("candidateRandomAffixes", "Shared Random Affix Dictionary (>=36)"); break;
                case 6: DrawItemRandomPools(); break;
                case 7: DrawCatalogProperty("candidateRandomAffixes", "Editable Affix Effect Payloads"); break;
                case 8: DrawCore(); break;
                case 9: DrawCatalogProperty("faMenBuildEffects", "FaMen Build2 / Build4 / Build6 Candidate Effects"); break;
                case 10: DrawCatalogProperty("qiLeiBuildEffects", "QiLei Build2 / Build4 Candidate Effects"); break;
                case 11: DrawCatalogProperty("dropSegments", "Candidate Drop Segments"); break;
                case 12: DrawDetailPreview(); break;
                case 13: DrawPreviewValidation(); break;
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("V0.4 Item Balance Workbench", EditorStyles.boldLabel, GUILayout.Width(230));
                EditorGUILayout.LabelField("BALANCE_CANDIDATE", GUILayout.Width(150));
                EditorGUILayout.LabelField("EDITABLE", GUILayout.Width(80));
                EditorGUILayout.LabelField("NOT_LIVE_LOCKED", GUILayout.Width(130));
                EditorGUILayout.LabelField("NOT_BATTLE_CONNECTED", GUILayout.Width(170));
                GUILayout.FlexibleSpace();
                bool dirty = EditorUtility.IsDirty(catalog) || catalog.profiles.Any(value => value != null && EditorUtility.IsDirty(value));
                EditorGUILayout.LabelField(dirty ? "● Unsaved changes" : "Saved", dirty ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.Width(130));
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Save Selected", EditorStyles.toolbarButton)) SaveSelected();
                if (GUILayout.Button("Save All", EditorStyles.toolbarButton)) AssetDatabase.SaveAssets();
                if (GUILayout.Button("Reload", EditorStyles.toolbarButton)) LoadCatalog();
                if (GUILayout.Button("Undo", EditorStyles.toolbarButton)) { Undo.PerformUndo(); Repaint(); }
                if (GUILayout.Button("Redo", EditorStyles.toolbarButton)) { Undo.PerformRedo(); Repaint(); }
                GUILayout.Space(10);
                if (GUILayout.Button("Copy Item", EditorStyles.toolbarButton)) CopyItem();
                if (GUILayout.Button("Copy Rarity Band", EditorStyles.toolbarButton)) CopyRarityBand();
                if (GUILayout.Button("Copy Stat Range", EditorStyles.toolbarButton)) CopyStatRange();
                GUILayout.Space(10);
                if (GUILayout.Button("Generate Suggested Bands", EditorStyles.toolbarButton)) RegenerateSelected("Generate suggested bands for the selected item?");
                if (GUILayout.Button("Reset Selected From Candidate Seed", EditorStyles.toolbarButton)) ResetSelected();
                GUILayout.Space(10);
                if (GUILayout.Button("CSV Export", EditorStyles.toolbarButton)) ExportCsv();
                if (GUILayout.Button("CSV Import Preview", EditorStyles.toolbarButton)) PreviewCsvImport();
                if (GUILayout.Button("Validation", EditorStyles.toolbarButton)) { validation = ItemBalanceWorkbenchValidation.Validate(catalog); tab = 13; }
            }
        }

        private void DrawOverview()
        {
            validation ??= ItemBalanceWorkbenchValidation.Validate(catalog, false);
            int profiles = catalog.profiles.Count(value => value != null);
            int versions = catalog.profiles.Where(value => value != null).Sum(value => value.rarityVersions.Count);
            int ranges = catalog.profiles.Where(value => value != null).Sum(value => value.rarityVersions.Sum(v => v.statRanges.Count));
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Prototype Count", profiles + " / 30");
                EditorGUILayout.LabelField("Rarity Count", "5");
                EditorGUILayout.LabelField("Version Count", versions + " / 150");
                EditorGUILayout.LabelField("Stat Range Count", ranges + " / 600");
                EditorGUILayout.LabelField("Missing / Invalid", validation.Errors.Count.ToString(CultureInfo.InvariantCulture));
                EditorGUILayout.LabelField("Data Maturity", catalog.dataMaturity);
            }
            DrawValidationMessages(validation);
        }

        private void DrawItemRanges()
        {
            DrawFilters();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(230)))
                {
                    itemScroll = EditorGUILayout.BeginScrollView(itemScroll, EditorStyles.helpBox);
                    foreach (ItemBalanceProfile profile in FilteredProfiles())
                    {
                        bool active = selected == profile;
                        if (GUILayout.Toggle(active, profile.baseItemId + "  " + profile.displayName, "Button") && !active)
                            selected = profile;
                    }
                    EditorGUILayout.EndScrollView();
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    if (selected == null) { EditorGUILayout.HelpBox("No item matches the current filters.", MessageType.Info); return; }
                    EditorGUILayout.LabelField($"{selected.baseItemId} {selected.displayName}  |  {selected.faMenTag} / {selected.qiLeiTag}  |  primary={selected.primaryStatId}, secondary={selected.secondaryStatId}", EditorStyles.boldLabel);
                    pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
                    DrawRangeTable(selected);
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void DrawFilters()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                search = EditorGUILayout.TextField("Search", search, GUILayout.Width(250));
                faMenFilter = Popup("FaMen", faMenFilter, catalog.profiles.Where(value => value != null).Select(value => value.faMenTag));
                qiLeiFilter = Popup("QiLei", qiLeiFilter, catalog.profiles.Where(value => value != null).Select(value => value.qiLeiTag));
                statFilter = Popup("Stat", statFilter, catalog.statDefinitions.Where(value => value != null).Select(value => value.statId));
                rarityFilter = Popup("Rarity", rarityFilter, ItemInstanceRarityCatalog.All.Select(value => value.stableKey));
            }
        }

        private void DrawRangeTable(ItemBalanceProfile profile)
        {
            string[] statIds = { profile.primaryStatId, profile.secondaryStatId, "nianCost", "cooldown" };
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Stat / Direction / Total", EditorStyles.boldLabel, GUILayout.Width(210));
                foreach (ItemInstanceRarityDefinition rarity in VisibleRarities())
                    GUILayout.Label(rarity.stableKey, EditorStyles.boldLabel, GUILayout.Width(160));
            }
            foreach (string statId in statIds)
            {
                if (statFilter != "All" && statFilter != statId) continue;
                ItemBalanceStatDefinition definition = catalog.FindStat(statId);
                ItemBalanceRange[] all = ItemInstanceRarityCatalog.All.Select(value => profile.FindVersion(value.rarity)?.FindRange(statId)).Where(value => value != null).ToArray();
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    string role = statId == profile.primaryStatId ? "PRIMARY" : statId == profile.secondaryStatId ? "SECONDARY" : "BASE";
                    GUILayout.Label($"{statId} [{role}]\n{definition?.direction}\n{all.Min(value => value.minUnits)}..{all.Max(value => value.maxUnits)}", GUILayout.Width(210));
                    foreach (ItemInstanceRarityDefinition rarity in VisibleRarities())
                    {
                        ItemBalanceRange range = profile.FindVersion(rarity.rarity)?.FindRange(statId);
                        if (range == null) { GUILayout.Label("missing", GUILayout.Width(160)); continue; }
                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(160)))
                        {
                            EditorGUI.BeginChangeCheck();
                            long min = EditorGUILayout.LongField("Min", range.minUnits);
                            long max = EditorGUILayout.LongField("Max", range.maxUnits);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(profile, "Edit Item Balance Range");
                                range.minUnits = min; range.maxUnits = max;
                                EditorUtility.SetDirty(profile);
                            }
                        }
                    }
                }
            }
        }

        private void DrawAffixes()
        {
            DrawSelectedProfilePicker();
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            DrawCatalogProperty("affixDefinitions", "Editable Candidate Affix Dictionary");
            if (selected != null)
            {
                SerializedObject serialized = new(selected);
                serialized.Update();
                EditorGUILayout.PropertyField(serialized.FindProperty("fixedAffixId"));
                EditorGUILayout.PropertyField(serialized.FindProperty("randomPoolId"));
                EditorGUILayout.PropertyField(serialized.FindProperty("randomAffixes"), true);
                serialized.ApplyModifiedProperties();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawCandidateItemPower()
        {
            DrawSelectedProfilePicker();
            DrawCatalogProperty("candidateItemPowerCoefficients", "Editable Candidate Item Power Coefficients");
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            if (selected != null)
            {
                SerializedObject serialized = new(selected);
                serialized.Update();
                SerializedProperty versions = serialized.FindProperty("rarityVersions");
                for (int index = 0; index < versions.arraySize; index++)
                {
                    SerializedProperty version = versions.GetArrayElementAtIndex(index);
                    EditorGUILayout.PropertyField(version.FindPropertyRelative("rarity"));
                    EditorGUILayout.PropertyField(version.FindPropertyRelative("candidateItemPower"));
                    EditorGUILayout.PropertyField(version.FindPropertyRelative("candidateItemPowerOverridden"));
                    EditorGUILayout.PropertyField(version.FindPropertyRelative("candidateDisplaySummary"));
                    EditorGUILayout.Space(4);
                }
                serialized.ApplyModifiedProperties();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawFixedSignatureAffix()
        {
            DrawSelectedProfilePicker();
            if (selected == null) return;
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            SerializedObject serialized = new(selected);
            serialized.Update();
            EditorGUILayout.PropertyField(serialized.FindProperty("signatureAffix"), true);
            EditorGUILayout.PropertyField(serialized.FindProperty("candidateDisplay"), true);
            serialized.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        private void DrawItemRandomPools()
        {
            DrawSelectedProfilePicker();
            if (selected == null) return;
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            SerializedObject serialized = new(selected);
            serialized.Update();
            EditorGUILayout.PropertyField(serialized.FindProperty("randomPoolId"));
            EditorGUILayout.PropertyField(serialized.FindProperty("randomAffixes"), true);
            serialized.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        private void DrawDetailPreview()
        {
            DrawSelectedProfilePicker();
            if (selected == null) return;
            ItemBalanceRarityVersion version = selected.FindVersion(previewRarity)
                ?? selected.rarityVersions.FirstOrDefault();
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            EditorGUILayout.HelpBox("即时预览读取当前编辑值；仅Sandbox候选，不写实例、棋盘或存档。", MessageType.Info);
            EditorGUILayout.LabelField(selected.displayName, EditorStyles.largeLabel);
            EditorGUILayout.LabelField((version == null ? "凡品" : version.rarity.ToDisplayName()) + " · "
                + selected.candidateDisplay.faMenDisplayName + " / " + selected.candidateDisplay.qiLeiDisplayName
                + " · " + selected.candidateDisplay.shapeDescription);
            EditorGUILayout.LabelField("物品强度（候选）", (version?.candidateItemPower ?? 0).ToString(CultureInfo.InvariantCulture));
            PreviewBlock("触发说明", selected.candidateDisplay.triggerDescription);
            PreviewBlock("基础效果", selected.candidateDisplay.basicEffectDescription);
            PreviewBlock("固定词条", selected.signatureAffix.displayName + "：" + selected.signatureAffix.description);
            PreviewBlock("随机词条池", string.Join("\n", selected.randomAffixes.Select(value =>
                value.affixId + " · weight=" + value.weight + " · mutex=" + value.mutexGroupId)));
            PreviewBlock("核心效果", string.Join("\n", selected.coreCandidates
                .Where(value => value != null && (version?.visibleCoreEffectIds.Contains(value.coreEffectId) ?? false))
                .Select(value => value.displayName + "：" + value.description)));
            PreviewBlock("法门 Build", string.Join("\n", catalog.FindBuildStages(selected.faMenTag, true)
                .Select(value => value.displayName + "：" + value.description)));
            PreviewBlock("器类 Build", string.Join("\n", catalog.FindBuildStages(selected.qiLeiTag, false)
                .Select(value => value.displayName + "：" + value.description)));
            PreviewBlock("推荐摆放", selected.candidateDisplay.placementRecommendation);
            PreviewBlock("旧物记", selected.candidateDisplay.flavorText);
            EditorGUILayout.EndScrollView();
        }

        private static void PreviewBlock(string title, string body)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.TextArea(string.IsNullOrWhiteSpace(body) ? "候选数据校验失败" : body,
                GUILayout.MinHeight(42));
        }

        private void DrawCore()
        {
            DrawSelectedProfilePicker();
            if (selected == null) return;
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            SerializedObject serialized = new(selected);
            serialized.Update();
            EditorGUILayout.PropertyField(serialized.FindProperty("coreCandidates"), true);
            EditorGUILayout.PropertyField(serialized.FindProperty("rarityVersions"), new GUIContent("Per-Rarity Eligible / Visible"), true);
            serialized.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        private void DrawPreviewValidation()
        {
            DrawSelectedProfilePicker();
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                previewRarity = (ItemInstanceRarity)EditorGUILayout.EnumPopup("Rarity", previewRarity);
                rootSeed = EditorGUILayout.LongField("rootSeed", rootSeed);
                if (GUILayout.Button("Generate Instance Preview", GUILayout.Width(190))) GeneratePreview();
                if (GUILayout.Button("Projection Contract Preview", GUILayout.Width(190))) GeneratePreview();
            }
            if (preview != null)
            {
                EditorGUILayout.LabelField("Preview Result: " + (preview.isSuccess ? "PASS" : "FAIL"), EditorStyles.boldLabel);
                if (preview.RollResult?.snapshot != null)
                {
                    EditorGUILayout.LabelField("Stats", string.Join(", ", preview.RollResult.snapshot.GeneratedStats.Select(value => value.statId + "=" + value.rawUnits)));
                    EditorGUILayout.LabelField("Affixes", string.Join(", ", preview.RollResult.snapshot.GeneratedAffixes.Select(value => value.affixId + "=" + value.rawUnits)));
                    EditorGUILayout.LabelField("Core eligible/visible", preview.RollResult.snapshot.GeneratedCorePotential.EligibleCoreEffectIds.Count + "/" + preview.RollResult.snapshot.GeneratedCorePotential.VisibleCoreEffectIds.Count);
                    EditorGUILayout.LabelField("BuildQualification", preview.RollResult.snapshot.buildQualification.ToString());
                    EditorGUILayout.TextArea(preview.ProjectionResult?.snapshot?.BuildCanonicalSignature() ?? string.Empty, GUILayout.MinHeight(90));
                    EditorGUILayout.LabelField("DEV_PREVIEW_ONLY Dimensions", EditorStyles.boldLabel);
                    foreach (ItemBalancePreviewDimension dimension in preview.Dimensions)
                        EditorGUILayout.LabelField(dimension.dimensionId, dimension.value.ToString("0.##", CultureInfo.InvariantCulture) + "  [" + string.Join(", ", dimension.Contributors) + "]");
                }
                foreach (string error in preview.Errors) EditorGUILayout.HelpBox(error, MessageType.Error);
            }
            validation ??= ItemBalanceWorkbenchValidation.Validate(catalog);
            DrawValidationMessages(validation);
            DrawImportPreview();
            DrawCatalogProperty("previewCoefficients", "DEV_PREVIEW_ONLY Coefficients");
        }

        private void DrawImportPreview()
        {
            if (importPreview == null) return;
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("CSV Import Diff", EditorStyles.boldLabel);
            foreach (string error in importPreview.errors.Take(30)) EditorGUILayout.HelpBox(error, MessageType.Error);
            foreach (ItemBalanceCsvChange change in importPreview.changes.Take(80)) EditorGUILayout.LabelField(change.Summary);
            using (new EditorGUI.DisabledScope(!importPreview.canApply))
            {
                if (GUILayout.Button("Apply CSV Diff With Undo", GUILayout.Height(28)))
                {
                    ItemBalanceWorkbenchCsvUtility.Apply(importPreview);
                    importPreview = null;
                    validation = ItemBalanceWorkbenchValidation.Validate(catalog);
                }
            }
        }

        private void DrawValidationMessages(ItemBalanceValidationReport report)
        {
            if (report == null) return;
            EditorGUILayout.HelpBox(report.isValid ? "Validation PASS" : "Validation FAIL: " + report.Errors.Count,
                report.isValid ? MessageType.Info : MessageType.Error);
            foreach (string error in report.Errors.Take(40)) EditorGUILayout.HelpBox(error, MessageType.Error);
            foreach (string warning in report.Warnings.Take(20)) EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }

        private void DrawCatalogProperty(string propertyName, string label)
        {
            pageScroll = EditorGUILayout.BeginScrollView(pageScroll);
            SerializedObject serialized = new(catalog);
            serialized.Update();
            EditorGUILayout.PropertyField(serialized.FindProperty(propertyName), new GUIContent(label), true);
            serialized.ApplyModifiedProperties();
            EditorGUILayout.EndScrollView();
        }

        private void DrawSelectedProfilePicker()
        {
            ItemBalanceProfile[] profiles = catalog.profiles.Where(value => value != null).OrderBy(value => value.baseItemId).ToArray();
            int index = Math.Max(0, Array.IndexOf(profiles, selected));
            index = EditorGUILayout.Popup("Selected Profile", index, profiles.Select(value => value.baseItemId + " " + value.displayName).ToArray());
            if (profiles.Length > 0) selected = profiles[Math.Min(index, profiles.Length - 1)];
        }

        private IEnumerable<ItemBalanceProfile> FilteredProfiles()
        {
            return catalog.profiles.Where(value => value != null)
                .Where(value => string.IsNullOrWhiteSpace(search) || value.baseItemId.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 || value.displayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(value => faMenFilter == "All" || value.faMenTag == faMenFilter)
                .Where(value => qiLeiFilter == "All" || value.qiLeiTag == qiLeiFilter)
                .Where(value => statFilter == "All" || value.primaryStatId == statFilter || value.secondaryStatId == statFilter || statFilter == "nianCost" || statFilter == "cooldown")
                .OrderBy(value => value.baseItemId);
        }

        private IEnumerable<ItemInstanceRarityDefinition> VisibleRarities()
        {
            return ItemInstanceRarityCatalog.All.Where(value => rarityFilter == "All" || value.stableKey == rarityFilter);
        }

        private static string Popup(string label, string current, IEnumerable<string> source)
        {
            string[] options = new[] { "All" }.Concat(source.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().OrderBy(value => value)).ToArray();
            int index = Math.Max(0, Array.IndexOf(options, current));
            return options[EditorGUILayout.Popup(label, index, options, GUILayout.Width(210))];
        }

        private void GeneratePreview()
        {
            if (selected == null) return;
            ItemBalanceCompiledData compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
            preview = ItemBalanceWorkbenchCompiler.Preview(catalog, compiled, selected.baseItemId, previewRarity, rootSeed);
        }

        private void RegenerateSelected(string message)
        {
            if (selected == null || !EditorUtility.DisplayDialog("Confirm overwrite", message, "Overwrite Selected", "Cancel")) return;
            ItemBalanceCandidateSeedBuilder.GenerateSuggestedBands(catalog, selected);
            validation = null;
        }

        private void ResetSelected()
        {
            if (selected == null || !EditorUtility.DisplayDialog("Reset selected candidate seed?", "All hand-edited values in the selected profile will be overwritten.", "Reset Selected", "Cancel")) return;
            ItemBalanceCandidateSeedBuilder.ResetSelectedProfile(catalog, selected);
            validation = null;
        }

        private void CopyItem() { if (selected != null) EditorGUIUtility.systemCopyBuffer = EditorJsonUtility.ToJson(selected, true); }
        private void CopyRarityBand()
        {
            ItemBalanceRarityVersion version = selected?.FindVersion(previewRarity);
            if (version != null) EditorGUIUtility.systemCopyBuffer = string.Join("\n", version.statRanges.Select(value => value.statId + "," + value.minUnits + "," + value.maxUnits));
        }
        private void CopyStatRange()
        {
            ItemBalanceRange range = selected?.FindVersion(previewRarity)?.FindRange(selected.primaryStatId);
            if (range != null) EditorGUIUtility.systemCopyBuffer = selected.baseItemId + "," + previewRarity.ToStableKey() + "," + range.statId + "," + range.minUnits + "," + range.maxUnits;
        }

        private void ExportCsv()
        {
            ItemCompleteCandidateContentWorkbenchVerifier.VerifyAndWrite(out string summary);
            ShowNotification(new GUIContent(summary.Split('\n')[0]));
        }

        private void PreviewCsvImport()
        {
            string path = EditorUtility.OpenFilePanel("Preview Item Balance CSV Import", Path.GetFullPath("Docs/V0.4/Reports"), "csv");
            if (string.IsNullOrWhiteSpace(path)) return;
            importPreview = ItemBalanceWorkbenchCsvUtility.PreviewImport(catalog, path);
            tab = 13;
        }

        private void SaveSelected()
        {
            if (selected != null) EditorUtility.SetDirty(selected);
            AssetDatabase.SaveAssets();
        }

        private void LoadCatalog()
        {
            AssetDatabase.Refresh();
            catalog = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(ItemBalanceCandidateSeedBuilder.CatalogPath);
            SelectFirst();
            validation = null; preview = null; importPreview = null;
        }

        private void SelectFirst()
        {
            selected = catalog?.profiles.Where(value => value != null).OrderBy(value => value.baseItemId).FirstOrDefault();
        }
    }
}
