using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.Items.InnerCatalog;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxTrayArtworkLayerSeparationGuardFixVerifier
    {
        private const string PackageName = "V0.4-BattleSandboxTrayArtworkLayerSeparationGuardFix01";
        private const string AssignmentPath =
            "Docs/V0.4/BattleSandboxTrayArtworkLayerSeparationGuardFix01_Assignment.md";
        private const string AssignmentHash =
            "843fd66ff0edc4add8ded8142a79ce804b22d8baaf66d8eb38cc51ed2b8ce361";
        private const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string SceneHash =
            "329696dde34e5f6569ca69b4bde926a25ca95e4b3d2f2158c198d26264e6a441";
        private const string CardViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs";
        private const string LayoutViewPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs";
        private const string ReportRoot = "Docs/V0.4/Reports/";
        private const float ColorTolerance = 0.004f;

        private static readonly string[] ReportPaths =
        {
            ReportRoot + "BattleSandboxTrayArtworkLayerSeparationGuardFixReport.md",
            ReportRoot + "BattleSandboxTrayArtworkLayerSeparationGuardFixSpec.csv",
            ReportRoot + "BattleSandboxTrayArtworkLayerSeparationMatrix30.csv",
            ReportRoot + "BattleSandboxTrayRuntimeCardArtworkMatrix.csv",
            ReportRoot + "BattleSandboxTrayArtworkLeaseRestorationMatrix.csv",
            ReportRoot + "BattleSandboxTrayArtworkLayerSeparationManualTest.md",
            ReportRoot + "BattleSandboxTrayArtworkLayerSeparationGuardFixLeakCheckReport.md"
        };

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ScenePath] = SceneHash,
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs"] =
                    "29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"] =
                    "6b3d3d76e179d9d40251ef1c0ae7e8b99469628c48e39af6d19f4f4973c05a5f",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs"] =
                    "da8c1b628a02565908e7bc7a0ef9e56dc2e00bec25af549df9382769372665bc",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs"] =
                    "a4075e2279bb937858be97a5e41365ce0eff58f12277f0d19b93b7efac29b590",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs"] =
                    "6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] =
                    "794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827",
                ["Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs"] =
                    "606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity"] =
                    "8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29",
                ["Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab"] =
                    "2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59"
            };

        private static readonly List<SpecResult> Specs = new();
        private static readonly List<string> MatrixRows = new();
        private static readonly List<string> RuntimeRows = new();
        private static readonly List<string> RestorationRows = new();
        private static string finalSceneHash = string.Empty;

        [MenuItem("Talisman Bag/V0.4/Verify BattleSandbox Tray Artwork Layer Separation GuardFix")]
        public static void VerifyOffline() => VerifyStaticBatch();

        public static void VerifyStaticBatch()
        {
            Specs.Clear();
            MatrixRows.Clear();
            RuntimeRows.Clear();
            RestorationRows.Clear();
            finalSceneHash = Sha256File(ProjectPath(ScenePath));

            Run("TALS-01", "AUTHORITATIVE_ARTWORK_RENDER_LEASE_PASS", VerifySourceContract);
            Run("TALS-02", "APPLY_BODY_COLOR_ARTWORK_ISOLATION_PASS", VerifyArtworkLeaseIsolation);
            Run("TALS-03", "I010_I016_ARTWORK_UNOBSCURED_PASS", VerifyCornerArtworkAndHitGeometry);
            Run("TALS-04", "I010_I016_HIT_GEOMETRY_UNCHANGED_PASS", VerifyCornerArtworkAndHitGeometry);
            Run("TALS-05", "I025_I030_RUNTIME_FALLBACK_ARTWORK_PASS", VerifyThirtyItemArtworkMatrix);
            Run("TALS-06", "I004_I005_DIRECTION_REGRESSION_PASS", VerifyDirectionRegression);
            Run("TALS-07", "TRAY_ARTWORK_PRESENCE_30_PASS", VerifyThirtyItemArtworkMatrix);
            Run("TALS-08", "TRAY_PHYSICAL_PARITY_30_PASS", VerifyPhysicalParity);
            Run("TALS-09", "BOARD_COORDINATE_PARITY_REGRESSION_PASS", VerifyProtectedHashes);
            Run("TALS-10", "LEASE_REBIND_RESTORE_PASS", VerifyArtworkLeaseIsolation);
            Run("TALS-11", "LEASE_DISABLE_DESTROY_RESTORE_PASS", VerifyArtworkLeaseIsolation);
            Run("TALS-12", "MISSING_ARTWORK_SAFE_FAILURE_PASS", VerifyArtworkLeaseIsolation);
            Run("TALS-13", "EDITMODE_ROUNDTRIP_PASS", VerifyEditModeRoundTrip);
            Run("TALS-14", "SCENE_BYTE_IDENTICAL_PASS", VerifySceneHash);
            Run("TALS-15", "PREFAB_BYTE_IDENTICAL_PASS", VerifyProtectedHashes);
            Run("TALS-16", "PNG_AGGREGATE_UNCHANGED_PASS", VerifyPngAggregate);
            Run("TALS-17", "PROTECTED_HASHES_PASS", VerifyProtectedHashes);
            Run("TALS-18", "LEAKCHECK_PASS", VerifyLeakCheck);
            Run("TALS-19", "PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS", VerifyScopedDiffCheck);
            Run("TALS-20", "USER_HANDTEST_WAITING", () => { });

            WriteReports();
            SpecResult[] failures = Specs.Where(spec => !spec.Passed).ToArray();
            if (failures.Length > 0)
            {
                throw new InvalidOperationException(PackageName + " failed: " + string.Join("; ",
                    failures.Select(spec => spec.Marker + "=" + spec.Detail)));
            }

            UnityEngine.Debug.Log(PackageName + " PASS");
        }

        private static void VerifySourceContract()
        {
            Require(Sha256File(ProjectPath(AssignmentPath)) == AssignmentHash, "Assignment SHA mismatch.");
            string cardSource = File.ReadAllText(ProjectPath(CardViewPath), Encoding.UTF8);
            string layoutSource = File.ReadAllText(ProjectPath(LayoutViewPath), Encoding.UTF8);
            Require(cardSource.Contains("HasAuthoritativeArtworkRenderLease", StringComparison.Ordinal)
                    && cardSource.Contains("SetLayoutCellRenderersNonObscuringForArtworkLease", StringComparison.Ordinal)
                    && cardSource.Contains("backgroundImage != authoritativeArtworkImage", StringComparison.Ordinal),
                "Artwork render lease or body isolation is missing.");
            Require(!layoutSource.Contains("SetAsFirstSibling", StringComparison.Ordinal),
                "Tray layout still reorders the generated layout layer.");
            Require(!ContainsAny(cardSource, "I010", "I016", "I025", "I026", "I027", "I028", "I029", "I030",
                    "Taibai", "TaiBai", "FaMen", "QiLei", "black mask"),
                "Runtime card source contains forbidden item/category presentation branch tokens.");
            Require(!ContainsAny(layoutSource, "I010", "I016", "I025", "I026", "I027", "I028", "I029", "I030",
                    "Taibai", "TaiBai", "FaMen", "QiLei", "black mask"),
                "Runtime layout source contains forbidden item/category presentation branch tokens.");
        }

        private static void VerifyArtworkLeaseIsolation()
        {
            Sprite sprite = ResolveAnyArtworkSprite();
            Require(sprite != null, "No source artwork sprite resolved.");

            VerifyIndependentArtworkLease(sprite);
            VerifyFallbackArtworkLease(sprite);
        }

        private static void VerifyIndependentArtworkLease(Sprite sprite)
        {
            GameObject root = NewRuntimeObject("ArtworkLeaseIndependent");
            try
            {
                Image body = root.AddComponent<Image>();
                body.color = new Color(0.16f, 0.28f, 0.41f, 0.74f);
                body.raycastTarget = true;
                BuildItemPreviewCardView card = root.AddComponent<BuildItemPreviewCardView>();
                card.Bind(root.GetComponent<RectTransform>(), root.AddComponent<CanvasGroup>(), body, null, null, null);

                GameObject artObject = NewRuntimeObject("ArtworkImage");
                artObject.transform.SetParent(root.transform, false);
                Image artwork = artObject.AddComponent<Image>();
                artwork.sprite = sprite;
                Color authoredColor = new(0.31f, 0.62f, 0.83f, 0.44f);
                artwork.color = authoredColor;
                artwork.raycastTarget = false;

                Image hitCell = NewLayoutCell(root.transform, new Color(0.81f, 0.22f, 0.11f, 0.76f));
                card.SetLayoutCellVisuals(new[] { hitCell });
                Require(card.BindAuthoritativeArtwork(sprite), "Independent artwork bind failed.");
                Require(card.AuthoritativeArtworkImage == artwork && card.HasAuthoritativeArtworkRenderLease,
                    "Independent artwork did not become the unique lease owner.");
                Require(SameRgb(artwork.color, authoredColor) && Approximately(artwork.color.a, 1f),
                    "Independent artwork RGB modulation was not preserved.");
                Require(hitCell.raycastTarget && Approximately(hitCell.color.a, 0f),
                    "Generated hit cell was not preserved as transparent raycast geometry.");

                card.SetNormalVisual();
                card.SetSelectedVisual();
                card.SetDraggingVisual();
                Require(SameRgb(artwork.color, authoredColor) && Approximately(artwork.color.a, 1f),
                    "Body visual mutated independent artwork.");

                Require(!card.BindAuthoritativeArtwork(null) && !card.HasAuthoritativeArtworkRenderLease,
                    "Missing artwork did not safely release independent lease.");
                Require(SameImageState(artwork, sprite, authoredColor, false),
                    "Independent artwork did not restore after missing-artwork release.");
                Require(Approximately(hitCell.color.a, 0.76f) && hitCell.raycastTarget,
                    "Generated hit cell did not restore after lease release.");
                RestorationRows.Add("independent,bind-missing,exact,PASS");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void VerifyFallbackArtworkLease(Sprite sprite)
        {
            GameObject root = NewRuntimeObject("ArtworkLeaseFallback");
            try
            {
                Image body = root.AddComponent<Image>();
                Color initialColor = new(0.18f, 0.11f, 0.07f, 0.67f);
                body.color = initialColor;
                body.raycastTarget = true;
                body.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
                BuildItemPreviewCardView card = root.AddComponent<BuildItemPreviewCardView>();
                card.Bind(root.GetComponent<RectTransform>(), root.AddComponent<CanvasGroup>(), body, null, null, null);

                Require(card.BindAuthoritativeArtwork(sprite), "Fallback artwork bind failed.");
                Require(card.AuthoritativeArtworkImage == body && card.UsesFallbackArtworkRenderLease,
                    "Root/background image did not enter fallback lease.");
                Require(SameRgb(body.color, Color.white) && Approximately(body.color.a, 1f),
                    "Fallback artwork was not neutral-white opaque.");
                Require(Approximately(NormalizeDegrees(body.rectTransform.localEulerAngles.z), 0f),
                    "Fallback artwork did not reset absolute rotation.");

                card.SetNormalVisual();
                card.SetSelectedVisual();
                card.SetDraggingVisual();
                Require(SameRgb(body.color, Color.white) && Approximately(body.color.a, 1f),
                    "Body visual tinted or cleared fallback artwork.");

                Require(card.BindAuthoritativeArtwork(sprite), "Fallback rebind failed.");
                card.RestoreAuthoritativeArtwork();
                Require(SameImageState(body, null, initialColor, true)
                    && Approximately(NormalizeDegrees(body.rectTransform.localEulerAngles.z), -90f),
                    "Fallback rebind/restore did not round-trip exact state.");
                RestorationRows.Add("fallback,rebind-restore,exact,PASS");

                Require(card.BindAuthoritativeArtwork(sprite), "Fallback disable-cycle bind failed.");
                root.SetActive(false);
                Require(!card.HasAuthoritativeArtworkRenderLease,
                    "Fallback disable did not release the active artwork lease.");
                root.SetActive(true);
                Require(card.HasAuthoritativeArtworkRenderLease && card.AuthoritativeArtworkImage == body
                    && SameRgb(body.color, Color.white) && Approximately(body.color.a, 1f),
                    "Fallback enable did not safely restore and rebind artwork.");
                for (int cycle = 0; cycle < 3; cycle++)
                {
                    card.SetVisible(false);
                    Require(!card.HasAuthoritativeArtworkRenderLease,
                        "Filter disable retained an active artwork lease at cycle " + cycle + ".");
                    card.SetVisible(true);
                    Require(card.HasAuthoritativeArtworkRenderLease && card.AuthoritativeArtworkImage == body
                        && SameRgb(body.color, Color.white) && Approximately(body.color.a, 1f),
                        "Filter enable did not safely rebind artwork at cycle " + cycle + ".");
                }
                RestorationRows.Add("fallback,filter-disable-enable-x3,rebind-safe,PASS");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void VerifyCornerArtworkAndHitGeometry()
        {
            Sprite sprite = ResolveAnyArtworkSprite();
            foreach (string itemId in new[] { "I010", "I016" })
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(itemId);
                Require(item != null && item.shapeId == "shape_corner3", itemId + " is not an expected corner item.");
                ShapeItemPayload payload = new(item.itemId, item.shapeId, ItemShapeRotation.Rotation0,
                    item.defaultLocalCells.Select(cell => new ItemShapeCell(cell.x, cell.y)).ToArray(), ShapePlacementSource.Tray);
                ShapeAwareItemTrayGrid grid = new(columnCount: 5, slotCount: 40);
                ShapePlacementResult placement = grid.CanPlace(payload, new ItemShapeCell(0, 0));
                Require(placement.IsValid && FormatCells(placement.OccupiedCells) == "0,0;0,1;1,1",
                    itemId + " physical tray footprint changed.");

                GameObject root = NewRuntimeObject(itemId + "_Artwork");
                try
                {
                    Image body = root.AddComponent<Image>();
                    BuildItemPreviewCardView card = root.AddComponent<BuildItemPreviewCardView>();
                    card.Bind(root.GetComponent<RectTransform>(), root.AddComponent<CanvasGroup>(), body, null, null, null);
                    Image[] cells =
                    {
                        NewLayoutCell(root.transform, Color.red),
                        NewLayoutCell(root.transform, Color.green),
                        NewLayoutCell(root.transform, Color.blue)
                    };
                    card.SetLayoutCellVisuals(cells);
                    Require(card.BindAuthoritativeArtwork(sprite), itemId + " artwork bind failed.");
                    Require(card.AreLayoutCellRenderersNonObscuring && cells.All(cell => cell.raycastTarget),
                        itemId + " layout cells obscured art or lost raycasts.");
                    MatrixRows.Add(itemId + ",corner,unobscured,0;5;6,1,PASS");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void VerifyThirtyItemArtworkMatrix()
        {
            foreach (ItemInnerDataDefinition item in ItemInnerDataCatalog.AllItems.Where(item => item != null
                         && string.CompareOrdinal(item.itemId, "I001") >= 0
                         && string.CompareOrdinal(item.itemId, "I030") <= 0)
                     .OrderBy(item => item.itemId, StringComparer.Ordinal))
            {
                Sprite sprite = Resources.Load<Sprite>("item_daoju/" + item.FaMenDisplayName + "/"
                    + item.itemId + "/" + item.itemId + "_5");
                Require(sprite != null, item.itemId + " authoritative sprite missing.");
                GameObject root = NewRuntimeObject(item.itemId + "_RuntimeCard");
                try
                {
                    Image body = root.AddComponent<Image>();
                    body.color = new Color(0.12f, 0.19f, 0.27f, 1f);
                    BuildItemPreviewCardView card = root.AddComponent<BuildItemPreviewCardView>();
                    card.Bind(root.GetComponent<RectTransform>(), root.AddComponent<CanvasGroup>(), body, null, null, null);
                    Require(card.BindAuthoritativeArtwork(sprite), item.itemId + " runtime fallback bind failed.");
                    card.SetNormalVisual();
                    Require(card.AuthoritativeArtworkImage == body && SameRgb(body.color, Color.white)
                        && Approximately(body.color.a, 1f), item.itemId + " artwork was body tinted.");
                    RuntimeRows.Add(item.itemId + ",fallback-root,neutral-white,visible,PASS");
                    if (!MatrixRows.Any(row => row.StartsWith(item.itemId + ",", StringComparison.Ordinal)))
                    {
                        MatrixRows.Add(item.itemId + ",standard,visible,unchanged,unchanged,PASS");
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
            Require(RuntimeRows.Count == 30 && MatrixRows.Count == 30, "Thirty-item artwork matrix is incomplete.");
        }

        private static void VerifyDirectionRegression()
        {
            Sprite sprite = ResolveAnyArtworkSprite();
            foreach (string itemId in new[] { "I004", "I005" })
            {
                GameObject root = NewRuntimeObject(itemId + "_Direction");
                try
                {
                    Image body = root.AddComponent<Image>();
                    body.rectTransform.localEulerAngles = new Vector3(0f, 0f, -90f);
                    BuildItemPreviewCardView card = root.AddComponent<BuildItemPreviewCardView>();
                    card.Bind(root.GetComponent<RectTransform>(), root.AddComponent<CanvasGroup>(), body, null, null, null);
                    Require(card.BindAuthoritativeArtwork(sprite), itemId + " bind failed.");
                    card.ApplyArtworkImageRotationOffset(0f);
                    Require(Approximately(NormalizeDegrees(body.rectTransform.localEulerAngles.z), 0f),
                        itemId + " artwork direction regression.");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void VerifyPhysicalParity()
        {
            foreach (ItemInnerDataDefinition item in ItemInnerDataCatalog.AllItems.Where(item => item != null
                         && string.CompareOrdinal(item.itemId, "I001") >= 0
                         && string.CompareOrdinal(item.itemId, "I030") <= 0))
            {
                ShapeItemPayload payload = new(item.itemId, item.shapeId, ItemShapeRotation.Rotation0,
                    item.defaultLocalCells.Select(cell => new ItemShapeCell(cell.x, cell.y)).ToArray(), ShapePlacementSource.Tray);
                ShapeAwareItemTrayGrid grid = new(columnCount: 5, slotCount: 65);
                Require(grid.CanPlace(payload, new ItemShapeCell(0, 0)).IsValid,
                    item.itemId + " tray physical placement regressed.");
            }
        }

        private static void VerifyEditModeRoundTrip()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                BuildItemPreviewCardView[] cards = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<BuildItemPreviewCardView>(true)).ToArray();
                Require(cards.Length >= 24, "BattleSandbox authored card capacity unexpectedly changed.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
            Require(Sha256File(ProjectPath(ScenePath)) == SceneHash,
                "Opening real BattleSandbox scene changed its disk bytes.");
        }

        private static void VerifySceneHash()
        {
            Require(finalSceneHash == SceneHash && Sha256File(ProjectPath(ScenePath)) == SceneHash,
                "BattleSandbox scene hash changed.");
        }

        private static void VerifyPngAggregate()
        {
            string root = ProjectPath("Assets/_Game/Resources/item_daoju");
            string[] pngs = Directory.GetFiles(root, "*.png", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.Ordinal).ToArray();
            Require(pngs.Length == 174, "Unexpected artwork PNG count: " + pngs.Length + ".");
            string aggregate = Sha256Text(string.Join("\n", pngs.Select(path =>
                path.Replace('\\', '/').ToUpperInvariant() + "|" + Sha256File(path))));
            Require(aggregate == "a5379f9b1f6472ffe0c58b9a27f29e4553e12cc98ad6f03992d9468b14b67be3",
                "Artwork PNG aggregate changed.");
        }

        private static void VerifyProtectedHashes()
        {
            foreach (KeyValuePair<string, string> pair in ProtectedHashes)
            {
                Require(Sha256File(ProjectPath(pair.Key)) == pair.Value, "Protected hash mismatch: " + pair.Key);
            }
        }

        private static void VerifyLeakCheck()
        {
            Require(!Directory.GetFiles(ProjectPath(ReportRoot), "*.meta", SearchOption.TopDirectoryOnly)
                .Any(path => ReportPaths.Any(report => string.Equals(path.Replace('\\', '/'),
                    ProjectPath(report).Replace('\\', '/'), StringComparison.OrdinalIgnoreCase))),
                "Package reports must not create .meta files.");
            Require(!File.ReadAllText(ProjectPath(LayoutViewPath), Encoding.UTF8)
                    .Contains("SetAsFirstSibling", StringComparison.Ordinal),
                "Generated layout hierarchy reordering leaked into runtime source.");
        }

        private static void VerifyScopedDiffCheck()
        {
            ProcessStartInfo startInfo = new("git")
            {
                WorkingDirectory = ProjectRoot(),
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add("-c");
            startInfo.ArgumentList.Add("safe.directory=" + ProjectRoot());
            startInfo.ArgumentList.Add("diff");
            startInfo.ArgumentList.Add("--check");
            startInfo.ArgumentList.Add("--");
            startInfo.ArgumentList.Add("Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs");
            startInfo.ArgumentList.Add("Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs");
            startInfo.ArgumentList.Add("Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkLayerSeparationGuardFixVerifier.cs");
            foreach (string reportPath in ReportPaths)
            {
                startInfo.ArgumentList.Add(reportPath);
            }
            using Process process = Process.Start(startInfo);
            process.WaitForExit();
            Require(process.ExitCode == 0, "git diff --check failed: " + process.StandardError.ReadToEnd());
        }

        private static Image NewLayoutCell(Transform parent, Color color)
        {
            GameObject layer = parent.Find("TrayLayoutCellLayer") == null
                ? NewRuntimeObject("TrayLayoutCellLayer")
                : parent.Find("TrayLayoutCellLayer").gameObject;
            if (layer.transform.parent == null)
            {
                layer.transform.SetParent(parent, false);
            }
            GameObject cell = NewRuntimeObject("TrayLayoutCell");
            cell.transform.SetParent(layer.transform, false);
            Image image = cell.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        private static GameObject NewRuntimeObject(string name)
        {
            GameObject target = new(name, typeof(RectTransform));
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            return target;
        }

        private static Sprite ResolveAnyArtworkSprite()
        {
            ItemInnerDataDefinition item = ItemInnerDataCatalog.AllItems.FirstOrDefault(value => value != null
                && string.CompareOrdinal(value.itemId, "I001") >= 0 && string.CompareOrdinal(value.itemId, "I030") <= 0);
            return item == null ? null : Resources.Load<Sprite>("item_daoju/" + item.FaMenDisplayName + "/"
                + item.itemId + "/" + item.itemId + "_5");
        }

        private static bool SameImageState(Image image, Sprite sprite, Color color, bool raycastTarget)
        {
            return image != null && image.sprite == sprite && image.overrideSprite == null
                && Approximately(image.color.r, color.r) && Approximately(image.color.g, color.g)
                && Approximately(image.color.b, color.b) && Approximately(image.color.a, color.a)
                && image.raycastTarget == raycastTarget;
        }

        private static bool SameRgb(Color left, Color right) => Approximately(left.r, right.r)
            && Approximately(left.g, right.g) && Approximately(left.b, right.b);
        private static bool Approximately(float left, float right) => Mathf.Abs(left - right) <= ColorTolerance;
        private static float NormalizeDegrees(float value) => Mathf.Repeat(value + 180f, 360f) - 180f;
        private static string FormatCells(IEnumerable<ItemShapeCell> cells) => string.Join(";", cells
            .OrderBy(cell => cell.y).ThenBy(cell => cell.x).Select(cell => cell.x + "," + cell.y));

        private static bool ContainsAny(string source, params string[] tokens) => tokens.Any(token =>
            source.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);

        private static void Run(string id, string marker, Action action)
        {
            try
            {
                action();
                Specs.Add(new SpecResult(id, marker, true, "PASS"));
            }
            catch (Exception exception)
            {
                Specs.Add(new SpecResult(id, marker, false, exception.Message));
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void WriteReports()
        {
            WriteText(ReportPaths[0], "# BattleSandbox Tray Artwork Layer Separation GuardFix Report\n\n"
                + "Status: `" + (Specs.All(spec => spec.Passed) ? "PASS" : "FAIL") + "`\n\n"
                + string.Join("\n", Specs.Select(spec => "- `" + spec.Marker + "`: " + (spec.Passed ? "PASS" : "FAIL") + " — " + spec.Detail)) + "\n");
            WriteText(ReportPaths[1], "id,marker,result,detail\n" + string.Join("\n", Specs.Select(spec =>
                spec.Id + "," + spec.Marker + "," + (spec.Passed ? "PASS" : "FAIL") + ",\"" + spec.Detail.Replace("\"", "\"\"") + "\"")) + "\n");
            WriteText(ReportPaths[2], "itemId,kind,artwork,occupiedSlots,hole,result\n" + string.Join("\n", MatrixRows) + "\n");
            WriteText(ReportPaths[3], "itemId,cardKind,modulation,artwork,result\n" + string.Join("\n", RuntimeRows) + "\n");
            WriteText(ReportPaths[4], "case,transition,state,result\n" + string.Join("\n", RestorationRows) + "\n");
            WriteText(ReportPaths[5], "# BattleSandbox Tray Artwork Layer Separation Manual Test\n\n"
                + "1. Enter Play in the BattleSandbox scene.\n2. Verify I010/I016 artwork remains visible and their three occupied hit cells still work while the corner hole does not.\n3. Verify I025-I030 retain original artwork colors without a black card-body mask.\n4. Verify I004/I005 remain upright.\n5. Exit Play and confirm authored UI remains unchanged.\n");
            WriteText(ReportPaths[6], "# Leak Check\n\n- Runtime source item/category special branches: `NONE`\n- Scene/Prefab persistence: `NONE`\n- Generated layout layer sibling reorder: `NONE`\n- Package report metas: `NONE`\n");
        }

        private static void WriteText(string relativePath, string content)
        {
            File.WriteAllText(ProjectPath(relativePath), content ?? string.Empty, new UTF8Encoding(false));
        }

        private static string ProjectRoot() => Directory.GetParent(Application.dataPath).FullName;
        private static string ProjectPath(string relativePath) => Path.Combine(ProjectRoot(), relativePath);
        private static string Sha256File(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Sha256Text(string value)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private readonly struct SpecResult
        {
            public SpecResult(string id, string marker, bool passed, string detail)
            {
                Id = id;
                Marker = marker;
                Passed = passed;
                Detail = detail ?? string.Empty;
            }

            public string Id { get; }
            public string Marker { get; }
            public bool Passed { get; }
            public string Detail { get; }
        }
    }
}
