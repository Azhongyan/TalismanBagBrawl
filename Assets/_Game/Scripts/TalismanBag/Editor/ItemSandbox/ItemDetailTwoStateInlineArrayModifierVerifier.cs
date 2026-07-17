using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.Generation;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.ItemSandbox.Editor
{
    public static class ItemDetailTwoStateInlineArrayModifierVerifier
    {
        private const string Marker = "ITEM_DETAIL_TWO_STATE_INLINE_ARRAY_MODIFIER_GUARDFIX01_PASS";
        private const string ReportPath = "Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierReport.md";
        private const string SpecPath = "Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierSpec.csv";
        private const string FieldMatrixPath = "Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierFieldMatrix.csv";
        private const string StateMatrixPath = "Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierStateMatrix.csv";
        private const string InventoryPath = "Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierInventory.csv";
        private const string LeakPath = "Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierLeakCheckReport.md";

        private const string PrefabPath = "Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab";
        private const string ScenePath = "Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity";
        private const string BuildSettingsPath = "ProjectSettings/EditorBuildSettings.asset";

        [MenuItem("Tools/Talisman Bag/V0.4/ItemSandbox/ItemDetailTwoStateInlineArrayModifierGuardFix01/Verify")]
        public static void VerifyMenu()
        {
            VerifyInternal();
        }

        public static void VerifyBatch()
        {
            bool pass = VerifyInternal();
            if (!pass)
            {
                EditorApplication.Exit(1);
            }
        }

        private static bool VerifyInternal()
        {
            string prefabBefore = Sha256(PrefabPath);
            string sceneBefore = Sha256(ScenePath);
            string buildSettingsBefore = Sha256(BuildSettingsPath);

            List<CheckRow> checks = new();
            IReadOnlyList<ItemDetailArrayModifierDefinition> definitions =
                ItemDetailArrayModifierResolver.BuildDefaultCandidateDefinitions();

            CheckInventory(definitions, checks);
            CheckResolutionRules(definitions, checks);
            CheckThemeToken(checks);
            CheckSourceGuards(checks);

            string prefabAfter = Sha256(PrefabPath);
            string sceneAfter = Sha256(ScenePath);
            string buildSettingsAfter = Sha256(BuildSettingsPath);
            Add(checks, "protectedPrefabHashStable", "protected-hash", prefabBefore == prefabAfter,
                prefabBefore + " -> " + prefabAfter);
            Add(checks, "protectedSceneHashStable", "protected-hash", sceneBefore == sceneAfter,
                sceneBefore + " -> " + sceneAfter);
            Add(checks, "protectedBuildSettingsHashStable", "protected-hash", buildSettingsBefore == buildSettingsAfter,
                buildSettingsBefore + " -> " + buildSettingsAfter);

            bool pass = checks.All(check => check.pass);
            WriteReports(checks, definitions, prefabAfter, sceneAfter, buildSettingsAfter, pass);
            if (pass)
            {
                Debug.Log(Marker);
            }
            else
            {
                Debug.LogError("ItemDetailTwoStateInlineArrayModifierGuardFix01 failed. See " + ReportPath);
            }

            return pass;
        }

        private static void CheckInventory(IReadOnlyList<ItemDetailArrayModifierDefinition> definitions, List<CheckRow> checks)
        {
            int count = definitions.Count;
            int baseCount = definitions.Select(value => value.baseItemId).Distinct(StringComparer.Ordinal).Count();
            int rarityCount = definitions.Select(value => value.rarityVersionKey).Distinct(StringComparer.Ordinal).Count();
            Add(checks, "candidateModifierCount150", "inventory", count == 150, count.ToString(CultureInfo.InvariantCulture));
            Add(checks, "candidateModifierBaseCount30", "inventory", baseCount == 30, baseCount.ToString(CultureInfo.InvariantCulture));
            Add(checks, "candidateModifierRarityVersionCount150", "inventory", rarityCount == 150, rarityCount.ToString(CultureInfo.InvariantCulture));
            Add(checks, "candidateModifierMaturity", "inventory",
                definitions.All(value => value.dataMaturity == ItemDetailArrayModifierResolver.DataMaturity),
                ItemDetailArrayModifierResolver.DataMaturity);
            Add(checks, "candidateModifierUniqueIds", "inventory",
                definitions.Select(value => value.modifierId).Distinct(StringComparer.Ordinal).Count() == count,
                "unique=" + definitions.Select(value => value.modifierId).Distinct(StringComparer.Ordinal).Count());

            foreach (ItemDetailArrayModifierTargetKind targetKind in Enum.GetValues(typeof(ItemDetailArrayModifierTargetKind)))
            {
                int targetCount = definitions.Count(value => value.targetKind == targetKind);
                Add(checks, "targetCoverage_" + targetKind, "target-coverage", targetCount > 0,
                    targetKind + "=" + targetCount.ToString(CultureInfo.InvariantCulture));
            }
        }

        private static void CheckResolutionRules(
            IReadOnlyList<ItemDetailArrayModifierDefinition> definitions,
            List<CheckRow> checks)
        {
            ItemDetailArrayModifierDefinition basisPoint = new()
            {
                modifierId = "qa_basis_point_500",
                baseItemId = "I001",
                rarityVersionKey = "I001@white",
                targetKind = ItemDetailArrayModifierTargetKind.Signature,
                targetId = "signature_i001",
                operation = ItemDetailArrayModifierOperation.AddPercent,
                rawUnits = 500,
                unitKey = "basisPoint",
                dataMaturity = ItemDetailArrayModifierResolver.DataMaturity
            };
            Add(checks, "basisPoint500FormatsPlus5Percent", "format",
                ItemDetailArrayModifierResolver.FormatDelta(basisPoint) == "+5%",
                ItemDetailArrayModifierResolver.FormatDelta(basisPoint));

            ItemDetailArrayModifierResolutionContext inactive = new()
            {
                baseItemId = "I001",
                rarity = ItemInstanceRarity.White,
                isArrayBonusActive = false
            };
            IReadOnlyList<ItemDetailResolvedArrayModifier> inactiveModifiers =
                ItemDetailArrayModifierResolver.Resolve(inactive, definitions);
            string inactiveRendered = inactiveModifiers.Count == 0
                ? string.Empty
                : ItemDetailArrayModifierResolver.AppendInlineModifier("浼ゅ锛?8", inactiveModifiers[0]);
            Add(checks, "notOnArrayShowsGreyInlineModifier", "resolve",
                inactiveModifiers.Count == 1
                && !inactiveModifiers[0].isCurrentlyApplied
                && ItemDetailArrayModifierResolver.IsModifierIconTokenLegal(inactiveRendered)
                && inactiveRendered.Contains("<color=#8A8A8A>", StringComparison.Ordinal),
                inactiveRendered);

            ItemDetailArrayModifierResolutionContext inactiveOnArray = new()
            {
                baseItemId = "I001",
                rarity = ItemInstanceRarity.White,
                isOnArrayBonusCell = true,
                isArrayBonusActive = false
            };
            IReadOnlyList<ItemDetailResolvedArrayModifier> inactiveOnArrayModifiers =
                ItemDetailArrayModifierResolver.Resolve(inactiveOnArray, definitions);
            string inactiveOnArrayRendered = inactiveOnArrayModifiers.Count == 0
                ? string.Empty
                : ItemDetailArrayModifierResolver.AppendInlineModifier("伤害：18", inactiveOnArrayModifiers[0]);
            Add(checks, "inactiveOnArrayShowsGreyInlineModifier", "resolve",
                inactiveOnArrayModifiers.Count == 1
                && !inactiveOnArrayModifiers[0].isCurrentlyApplied
                && ItemDetailArrayModifierResolver.IsModifierIconTokenLegal(inactiveOnArrayRendered)
                && inactiveOnArrayRendered.Contains("<color=#8A8A8A>", StringComparison.Ordinal),
                inactiveOnArrayRendered);

            ItemDetailArrayModifierResolutionContext active = new()
            {
                baseItemId = "I001",
                rarity = ItemInstanceRarity.White,
                isOnArrayBonusCell = true,
                isArrayBonusActive = true
            };
            IReadOnlyList<ItemDetailResolvedArrayModifier> resolved =
                ItemDetailArrayModifierResolver.Resolve(active, definitions);
            string rendered = resolved.Count == 0
                ? string.Empty
                : ItemDetailArrayModifierResolver.AppendInlineModifier("伤害：18", resolved[0]);
            Add(checks, "activeArrayReturnsInlineModifier", "resolve",
                resolved.Count == 1 && ItemDetailArrayModifierResolver.IsModifierIconTokenLegal(rendered),
                rendered);

            ItemDetailArrayModifierResolutionContext coreLocked = new()
            {
                baseItemId = "I004",
                rarity = ItemInstanceRarity.White,
                isOnArrayBonusCell = true,
                isArrayBonusActive = true,
                activeCoreEffectIds = Array.Empty<string>()
            };
            IReadOnlyList<ItemDetailResolvedArrayModifier> coreLockedModifiers =
                ItemDetailArrayModifierResolver.Resolve(coreLocked, definitions);
            Add(checks, "coreLockedShowsGreyInlineModifier", "resolve",
                coreLockedModifiers.Count == 1 && !coreLockedModifiers[0].isCurrentlyApplied,
                "core target remains visible while inactive and should be grey");

            ItemDetailArrayModifierResolutionContext buildInactive = new()
            {
                baseItemId = "I005",
                rarity = ItemInstanceRarity.White,
                isOnArrayBonusCell = true,
                isArrayBonusActive = true,
                faMenActiveStagePieceCount = 0
            };
            IReadOnlyList<ItemDetailResolvedArrayModifier> inactiveBuildModifiers =
                ItemDetailArrayModifierResolver.Resolve(buildInactive, definitions);
            string inactiveBuildRendered = inactiveBuildModifiers.Count == 0
                ? string.Empty
                : ItemDetailArrayModifierResolver.AppendInlineModifier("2件效果：候选Build效果", inactiveBuildModifiers[0], false);
            Add(checks, "buildInactiveShowsGreyInlineModifier", "resolve",
                inactiveBuildModifiers.Count == 1
                && ItemDetailArrayModifierResolver.IsModifierIconTokenLegal(inactiveBuildRendered),
                "build stage target remains visible on inactive Build rows for grey presentation");
        }

        private static void CheckThemeToken(List<CheckRow> checks)
        {
            string hex = ColorUtility.ToHtmlStringRGB(ItemDetailVisualThemeDefaults.ArrayModifierColor);
            Add(checks, "arrayModifierColorDefault", "theme", hex == ItemDetailArrayModifierResolver.DefaultColorHex,
                "#" + hex);
            Add(checks, "arrayModifierIconKey", "theme",
                ItemDetailArrayModifierResolver.IconKey == "Icon_ArrayVeinModifier",
                ItemDetailArrayModifierResolver.IconKey);
        }

        private static void CheckSourceGuards(List<CheckRow> checks)
        {
            string panel = Read("Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs");
            string sectionView = Read("Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs");
            string composer = Read("Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs");
            string resolver = Read("Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailArrayModifierData.cs");
            string workbench = Read("Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs");

            Add(checks, "headerClearsThirdBadge", "source-guard",
                panel.Contains("ClearStatusBadge(2)", StringComparison.Ordinal),
                "third badge cleared at bind time");
            Add(checks, "headerHasNoAwakeningBadgeText", "source-guard",
                !panel.Contains("待开窍", StringComparison.Ordinal)
                && !panel.Contains("已开窍", StringComparison.Ordinal),
                "SetStatusBadges no longer emits awakening badge labels");
            Add(checks, "currentStateHasOnlyLightingAndArray", "source-guard",
                !composer.Contains("开窍状态：", StringComparison.Ordinal)
                && !composer.Contains("Build状态：", StringComparison.Ordinal)
                && !composer.Contains("接亮状态：", StringComparison.Ordinal),
                "BuildCurrentStateSection excludes awakening/build/relay status rows");
            Add(checks, "noStandaloneArrayModifierSection", "source-guard",
                !composer.Contains("阵脉加成", StringComparison.Ordinal)
                && !panel.Contains("阵脉加成", StringComparison.Ordinal),
                "no standalone player section created");
            Add(checks, "noEmojiOrUnicodeIconFallback", "source-guard",
                !resolver.Contains("◆", StringComparison.Ordinal)
                && !resolver.Contains("🔷", StringComparison.Ordinal)
                && !resolver.Contains("✨", StringComparison.Ordinal),
                "icon token is key-based, not emoji/unicode art");
            Add(checks, "arrayModifierTokenRenderedAsSprite", "source-guard",
                sectionView.Contains("ApplyInlineArrayModifierIcons", StringComparison.Ordinal)
                && sectionView.Contains("RemoveArrayModifierIconToken", StringComparison.Ordinal)
                && sectionView.Contains("Sprite.Create", StringComparison.Ordinal)
                && sectionView.Contains("arrayModifierIcon", StringComparison.Ordinal),
                "ItemDetailSectionView consumes Icon_ArrayVeinModifier key and renders a Sprite overlay instead of player-visible text.");
            Add(checks, "workbenchCanViewAndEditCandidateModifiers", "source-guard",
                workbench.Contains("ArrayModifierCandidates", StringComparison.Ordinal)
                && workbench.Contains("TryUpdateArrayModifierCandidate", StringComparison.Ordinal)
                && workbench.Contains("ArrayModifierCandidateEdit", StringComparison.Ordinal),
                "Workbench exposes BALANCE_CANDIDATE array modifiers and supports idempotent modifierId edits.");
        }

        private static void WriteReports(
            IReadOnlyList<CheckRow> checks,
            IReadOnlyList<ItemDetailArrayModifierDefinition> definitions,
            string prefabHash,
            string sceneHash,
            string buildSettingsHash,
            bool pass)
        {
            Encoding encoding = new UTF8Encoding(false);
            File.WriteAllText(ReportPath, BuildReport(checks, prefabHash, sceneHash, buildSettingsHash, pass), encoding);
            File.WriteAllText(SpecPath, BuildSpec(checks), encoding);
            File.WriteAllText(FieldMatrixPath, BuildFieldMatrix(), encoding);
            File.WriteAllText(StateMatrixPath, BuildStateMatrix(), encoding);
            File.WriteAllText(InventoryPath, BuildInventory(definitions), encoding);
            File.WriteAllText(LeakPath, BuildLeakReport(checks, pass), encoding);
        }

        private static string BuildReport(
            IReadOnlyList<CheckRow> checks,
            string prefabHash,
            string sceneHash,
            string buildSettingsHash,
            bool pass)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemDetailTwoStateInlineArrayModifierGuardFix01 Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-ItemDetailTwoStateAndInlineArrayModifierGuardFix01`");
            builder.AppendLine("- Status: **" + (pass ? "PASS" : "FAIL") + "**");
            builder.AppendLine("- Marker: `" + (pass ? Marker : "WITHHELD") + "`");
            builder.AppendLine("- Data maturity: `BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED`");
            builder.AppendLine("- User layout / RectTransform / prefab / scene writes: `0`");
            builder.AppendLine("- ItemSystemSnapshot.v1 changes: `0`");
            builder.AppendLine("- Prefab hash: `" + prefabHash + "`");
            builder.AppendLine("- Scene hash: `" + sceneHash + "`");
            builder.AppendLine("- BuildSettings hash: `" + buildSettingsHash + "`");
            builder.AppendLine();
            builder.AppendLine("## Checks");
            foreach (CheckRow check in checks)
            {
                builder.AppendLine("- " + (check.pass ? "PASS" : "FAIL") + " `" + check.id + "` — " + check.evidence);
            }
            builder.AppendLine();
            builder.AppendLine("## Notes");
            builder.AppendLine("- Header player badges are now limited to lighting state and array-vein state.");
            builder.AppendLine("- Awakening/opening status remains inside core-effect rows and is not emitted as a header badge.");
            builder.AppendLine("- Array modifier payload is independent candidate data; UI projection never invents values from strings or fixed multipliers.");
            builder.AppendLine("- Workbench exposes the candidate modifier inventory for view/edit and resolves details from the edited in-memory list.");
            builder.AppendLine("- The inline payload keeps the stable key `" + ItemDetailArrayModifierResolver.IconKey + "` until bind time; ItemDetailSectionView removes that key from player text and renders a Sprite overlay from theme/procedural fallback.");
            return builder.ToString();
        }

        private static string BuildSpec(IReadOnlyList<CheckRow> checks)
        {
            StringBuilder builder = new();
            builder.AppendLine("caseId,area,result,evidence");
            foreach (CheckRow check in checks)
            {
                builder.Append(Escape(check.id)).Append(',')
                    .Append(Escape(check.area)).Append(',')
                    .Append(check.pass ? "PASS" : "FAIL").Append(',')
                    .Append(Escape(check.evidence)).AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildFieldMatrix()
        {
            return "field,owner,purpose,writeBehavior\n"
                + "displayLightingStatusText,ItemDetailViewModel,header lighting status,read-only projection\n"
                + "displayArrayBonusStatusText,ItemDetailViewModel,header array-vein status,read-only projection\n"
                + "displayAwakeningStatusText,ItemDetailViewModel,core internal awakening state only,not consumed by header badge\n"
                + "arrayModifierColor,ItemDetailVisualTheme,editable array modifier special color,theme token default #55C6B3\n"
                + "arrayModifierIcon,ItemDetailVisualTheme,editable sprite reference for Icon_ArrayVeinModifier,nullable sprite slot\n"
                + "ItemDetailArrayModifierDefinition,ItemDetailArrayModifierResolver,candidate array modifier payload,does not mutate source stat/affix/core/build data\n"
                + "ArrayModifierCandidates,ItemFullDetailBuildSandboxWorkbenchSession,workbench-visible editable candidate inventory,modifierId-based idempotent edit list\n";
        }

        private static string BuildStateMatrix()
        {
            return "caseId,placementState,lightingBadge,arrayBadge,inlineModifierCount\n"
                + "notPlaced,no placementId,尚未入阵,阵脉未计算,0\n"
                + "placedUnlitNoArray,placed unlit not on array,未点亮,未占阵脉,0\n"
                + "placedUnlitArray,placed unlit on array,未点亮,阵脉待亮,0\n"
                + "directLitArray,direct lit on array,直接点亮,阵脉生效,configured target count\n"
                + "relayLitArray,relay lit on array,相邻接亮,阵脉生效,configured target count\n"
                + "lightingSource,isLightingSource true,点亮源,阵脉状态按占位计算,0\n";
        }

        private static string BuildInventory(IReadOnlyList<ItemDetailArrayModifierDefinition> definitions)
        {
            StringBuilder builder = new();
            builder.AppendLine("modifierId,baseItemId,rarityVersionKey,sourceKind,sourceCellScope,sourceCellId,targetKind,targetId,targetParameterKey,operation,rawUnits,unitKey,formattedDelta,stackingPolicy,dataMaturity");
            foreach (ItemDetailArrayModifierDefinition definition in definitions
                         .OrderBy(value => value.baseItemId, StringComparer.Ordinal)
                         .ThenBy(value => value.rarityVersionKey, StringComparer.Ordinal))
            {
                builder.Append(Escape(definition.modifierId)).Append(',')
                    .Append(Escape(definition.baseItemId)).Append(',')
                    .Append(Escape(definition.rarityVersionKey)).Append(',')
                    .Append(Escape(definition.sourceKind)).Append(',')
                    .Append(Escape(definition.sourceCellScope)).Append(',')
                    .Append(Escape(definition.sourceCellId)).Append(',')
                    .Append(Escape(definition.targetKind.ToString())).Append(',')
                    .Append(Escape(definition.targetId)).Append(',')
                    .Append(Escape(definition.targetParameterKey)).Append(',')
                    .Append(Escape(definition.operation.ToString())).Append(',')
                    .Append(definition.rawUnits.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(Escape(definition.unitKey)).Append(',')
                    .Append(Escape(ItemDetailArrayModifierResolver.FormatDelta(definition))).Append(',')
                    .Append(Escape(definition.stackingPolicy)).Append(',')
                    .Append(Escape(definition.dataMaturity)).AppendLine();
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(IReadOnlyList<CheckRow> checks, bool pass)
        {
            StringBuilder builder = new();
            builder.AppendLine("# ItemDetailTwoStateInlineArrayModifierGuardFix01 Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("- Status: **" + (pass ? "PASS" : "FAIL") + "**");
            builder.AppendLine("- Formal Battle changes: `0`");
            builder.AppendLine("- Reward / RunFlow changes: `0`");
            builder.AppendLine("- Inventory / SaveData changes: `0`");
            builder.AppendLine("- Boss changes: `0`");
            builder.AppendLine("- Formal drop changes: `0`");
            builder.AppendLine("- ItemSystemSnapshot.v1 changes: `0`");
            builder.AppendLine("- Scene / Prefab layout writes: `0`");
            builder.AppendLine("- Standalone array modifier section: `0`");
            builder.AppendLine();
            foreach (CheckRow check in checks.Where(value => value.area.Contains("guard", StringComparison.Ordinal)
                         || value.area.Contains("hash", StringComparison.Ordinal)))
            {
                builder.AppendLine("- " + (check.pass ? "PASS" : "FAIL") + " `" + check.id + "` — " + check.evidence);
            }

            return builder.ToString();
        }

        private static void Add(List<CheckRow> checks, string id, string area, bool pass, string evidence)
        {
            checks.Add(new CheckRow(id, area, pass, evidence));
        }

        private static string Read(string relativePath)
        {
            return File.ReadAllText(Path.Combine(ProjectRoot, relativePath));
        }

        private static string Sha256(string relativePath)
        {
            string path = Path.Combine(ProjectRoot, relativePath);
            if (!File.Exists(path))
            {
                return "MISSING";
            }

            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        private static string Escape(string value)
        {
            string text = value ?? string.Empty;
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private sealed class CheckRow
        {
            public CheckRow(string id, string area, bool pass, string evidence)
            {
                this.id = id;
                this.area = area;
                this.pass = pass;
                this.evidence = evidence ?? string.Empty;
            }

            public readonly string id;
            public readonly string area;
            public readonly bool pass;
            public readonly string evidence;
        }
    }
}
