#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxUiLayoutGuard
    {
        private static readonly string[] ScanRoots =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox"
        };

        private static readonly string[] LayoutWriteTokens =
        {
            "RectTransform",
            ".anchoredPosition",
            ".sizeDelta",
            ".anchorMin",
            ".anchorMax",
            ".pivot",
            ".localScale",
            "SetParent(",
            "SetSiblingIndex(",
            "SetAsLastSibling(",
            "GridLayoutGroup",
            "ContentSizeFitter",
            "LayoutElement",
            "ScrollRect",
            "CanvasScaler"
        };

        private static readonly string[] FormalUiCreationTokens =
        {
            "new GameObject(",
            "AddComponent<Canvas>",
            "typeof(Canvas)",
            "GraphicRaycaster",
            "UnityEngine.UI"
        };

        private static readonly string[] CurrentUiSceneTokens =
        {
            "Scene_TalismanBag_V03_MainHome",
            "Scene_TalismanBag_V02_FormationCounter",
            "Scene_TalismanBag_V03_TalismanUpgrade",
            "BottomNavBar_Root",
            "MainHomeRoot",
            "V02BottomOperationArea",
            "V03BattlePrepare"
        };

        private static readonly string[] IndependentPreviewSceneAuthoringFiles =
        {
            "BattleSandboxPreviewSceneBuilder.cs",
            "BattleSandboxPreviewSceneVerifier.cs",
            "BattleSandboxPreviewSceneReportWriter.cs",
            "BuildPlacementFeedbackView.cs",
            "BuildSandboxItemInfoPanel.cs",
            "BuildGridPreviewSlotView.cs",
            "BuildItemPreviewCardView.cs",
            "BuildItemTrayPreviewView.cs",
            "BuildGridInteractionPreviewController.cs",
            "BuildGridInteractionPreviewSceneBinder.cs",
            "BuildGridInteractionPreviewValidator.cs",
            "BuildGridInteractionPreviewReportWriter.cs",
            "BattlePrepareComponentAdapterRuntimePlaytest.cs",
            "ShapeAwareItemTrayFixtureView.cs"
        };

        private static readonly string[] FormalUiSourceReferenceOnlyFiles =
        {
            "BattlePrepareComponentAdapterRuntimePlaytest.cs"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("UI Layout Guard");
            List<string> files = EnumerateBuildSandboxFiles().ToList();
            if (files.Count == 0)
            {
                report.AddWarning(
                    "NO_BUILDSANDBOX_CODE",
                    "No BuildSandbox C# files were found to scan.");
                return report;
            }

            foreach (string file in files)
            {
                string assetPath = ToAssetPath(file);
                string rawText = File.ReadAllText(file);
                string strippedCode = StripCommentsAndStrings(rawText);
                string sceneScanText = assetPath.EndsWith(
                    "BuildSandboxUiLayoutGuard.cs",
                    StringComparison.OrdinalIgnoreCase)
                    ? strippedCode
                    : rawText;
                bool independentPreviewAuthoring = IsIndependentPreviewSceneAuthoringFile(assetPath);
                bool formalUiSourceReferenceOnly = IsFormalUiSourceReferenceOnlyFile(assetPath);

                if (independentPreviewAuthoring)
                {
                    report.AddInfo(
                        "INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED",
                        "Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene.",
                        assetPath);
                }
                else
                {
                    CheckTokens(report, strippedCode, LayoutWriteTokens, "LAYOUT_WRITE_TOKEN", assetPath);
                    CheckTokens(report, strippedCode, FormalUiCreationTokens, "FORMAL_UI_CREATION_TOKEN", assetPath);
                }

                if (formalUiSourceReferenceOnly)
                {
                    report.AddInfo(
                        "FORMAL_UI_SOURCE_REFERENCE_ONLY_ALLOWED",
                        "This devOnly runtime playtest references mature formal UI sources without writing formal scene assets.",
                        assetPath);
                }
                else
                {
                    CheckTokens(report, sceneScanText, CurrentUiSceneTokens, "CURRENT_UI_SCENE_TOKEN", assetPath);
                }
            }

            if (report.ErrorCount == 0)
            {
                report.AddInfo(
                    "UI_LAYOUT_GUARD_PASS",
                    "BuildSandbox code scan found no scene access, formal UI creation, or layout write tokens.");
            }

            return report;
        }

        private static bool IsIndependentPreviewSceneAuthoringFile(string assetPath)
        {
            return IndependentPreviewSceneAuthoringFiles.Any(file =>
                assetPath.EndsWith(file, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsFormalUiSourceReferenceOnlyFile(string assetPath)
        {
            return FormalUiSourceReferenceOnlyFiles.Any(file =>
                assetPath.EndsWith(file, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> EnumerateBuildSandboxFiles()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            foreach (string scanRoot in ScanRoots)
            {
                string fullRoot = Path.Combine(projectRoot, scanRoot.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(fullRoot))
                {
                    continue;
                }

                foreach (string file in Directory.GetFiles(fullRoot, "*.cs", SearchOption.AllDirectories))
                {
                    yield return file;
                }
            }
        }

        private static void CheckTokens(
            BuildSandboxValidationReport report,
            string text,
            IReadOnlyList<string> tokens,
            string code,
            string assetPath)
        {
            foreach (string token in tokens)
            {
                if (!ContainsGuardedToken(text, token))
                {
                    continue;
                }

                report.AddError(
                    code,
                    $"BuildSandbox code contains guarded token '{token}'. Keep this package read-only for UI layout.",
                    assetPath);
            }
        }

        private static bool ContainsGuardedToken(string text, string token)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            if (!IsIdentifierToken(token))
            {
                return text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            string pattern = $@"(?<![A-Za-z0-9_]){Regex.Escape(token)}(?![A-Za-z0-9_])";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static bool IsIdentifierToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token)
                && token.All(character => char.IsLetterOrDigit(character) || character == '_');
        }

        private static string StripCommentsAndStrings(string source)
        {
            StringBuilder builder = new(source.Length);
            bool inString = false;
            bool inChar = false;
            bool inLineComment = false;
            bool inBlockComment = false;
            bool verbatimString = false;

            for (int i = 0; i < source.Length; i++)
            {
                char current = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : '\0';

                if (inLineComment)
                {
                    if (current == '\n')
                    {
                        inLineComment = false;
                        builder.Append('\n');
                    }

                    continue;
                }

                if (inBlockComment)
                {
                    if (current == '*' && next == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }

                    continue;
                }

                if (inString)
                {
                    if (verbatimString && current == '"' && next == '"')
                    {
                        i++;
                        continue;
                    }

                    if (current == '"' && (verbatimString || !IsEscaped(source, i)))
                    {
                        inString = false;
                        verbatimString = false;
                    }

                    continue;
                }

                if (inChar)
                {
                    if (current == '\'' && !IsEscaped(source, i))
                    {
                        inChar = false;
                    }

                    continue;
                }

                if (current == '/' && next == '/')
                {
                    inLineComment = true;
                    i++;
                    continue;
                }

                if (current == '/' && next == '*')
                {
                    inBlockComment = true;
                    i++;
                    continue;
                }

                if (current == '@' && next == '"')
                {
                    inString = true;
                    verbatimString = true;
                    i++;
                    continue;
                }

                if (current == '"')
                {
                    inString = true;
                    continue;
                }

                if (current == '\'')
                {
                    inChar = true;
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        private static bool IsEscaped(string source, int index)
        {
            int slashCount = 0;
            for (int i = index - 1; i >= 0 && source[i] == '\\'; i--)
            {
                slashCount++;
            }

            return slashCount % 2 == 1;
        }

        private static string ToAssetPath(string fullPath)
        {
            string dataPath = Application.dataPath.Replace('\\', '/');
            string normalized = fullPath.Replace('\\', '/');
            if (normalized.StartsWith(dataPath, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + normalized.Substring(dataPath.Length);
            }

            return normalized;
        }
    }
}
#endif
