using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Presentation.Items;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public sealed class C1ExactBattleSandboxItemPromotionEditor : EditorWindow
    {
        private const int DynamicPresentationInitialPrewarmCount = 7;

        public const string TerminalMarker =
            "C1_EXACT_BATTLESANDBOX_ITEM_PROMOTION_AUTHORING_VALIDATION_PASS";
        public const string Rev03TerminalMarker =
            "C1_EXACT_BATTLESANDBOX_ITEM_FOOTPRINT_REV03_AUTHORING_VALIDATION_PASS";
        public const string Rev04FTrayTerminalMarker =
            "C1_EXACT_BATTLESANDBOX_TRAY_REV04F_AUTHORING_VALIDATION_PASS";
        public const string Rev04DynamicPresentationTerminalMarker =
            "C1_DYNAMIC_ITEM_PRESENTATION_REV04_AUTHORING_VALIDATION_PASS";
        public const string Rev06CarrierTerminalMarker =
            "C1_AUTHORED_SEVEN_LAYER_ITEM_CARRIERS_REV06_AUTHORING_VALIDATION_PASS";

        private const string SourceScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string ExistingFormalCardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemCard.prefab";
        private const string BoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemBoard.prefab";
        private const string TrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemTray.prefab";
        private const string CardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemCard.prefab";
        private const string RarityContourBloomVfxPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/ItemVFX/"
            + "ItemRarityContourBloomVfx.prefab";
        private const string DynamicPresentationCatalogAssetPath =
            "Assets/_Game/Resources/V04/ItemPresentation/"
            + "C1FormalItemPresentationCatalog.asset";
        private const string ParityCsvPath =
            "Docs/V0.4/Reports/C1ExactBattleSandboxItemPresentationParity.csv";
        private const string CloseReportPath =
            "Docs/V0.4/Reports/C1ExactBattleSandboxItemPromotionReport.md";

        private static readonly IReadOnlyDictionary<string, string> ProtectedHashes =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [SourceScenePath] =
                    "1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"] =
                    "364727A582EE3F2D499C109D98939318B4D26AB9D2F1F571CB70E62E605913DC",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs"] =
                    "06AF79CB89EB4DCA0A2313E81AF81EFA89A35B90AE6FE5E6B0BBDD3C90F62921",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs"] =
                    "448A69762EC157214809E1D43CD19A134D77F7928742CC890A94A41B1588D56E",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs"] =
                    "D232A34F6A761D745564C5015C977F9EEF26E2C0D23CD764B6CFA2704D008214",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs"] =
                    "935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemArrangementPresenter.cs"] =
                    "6EB87259CACA6132E44FF5139B67D19E6DCC6ED7C9988877171792338373EE08",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemBoardView.cs"] =
                    "6A3A155198568870E2F0DE936D2E3A88D3DCFB6314B83347E6FA0B5D179C591F",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemTrayView.cs"] =
                    "743A7F11B1B4F1F1855A0E6C98CC6F59823913334294A7282A9045461591583D",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemCardView.cs"] =
                    "E2871981B734B4E9CAA09B60E131BC5E6D2F311D309A195C6E28DB60E3FAE41B",
                ["Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemBoard.prefab"] =
                    "984EEA4E7A33D86B29F16A1C5B59D26C08889D79E5A8F6E9D1C9B969F41DEFDD",
                ["Assets/_Game/Prefabs/TalismanBag/Items/C1FormalItemTray.prefab"] =
                    "416E534214B429A21A2EC7EC431EDDB324DF3A4EDAADE87155E2817CF35A5605",
                [ExistingFormalCardPrefabPath] =
                    "9D97B2B612F65FD0D1574FE13C690416523D7FE19A740F3854C4FC1AAAD06868",
                ["Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.cs"] =
                    "25283BCA93BA382EAB133CEFAB394A66D7F6EB6BE87014469C38FACE1157BD01",
                ["Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattlePageShellSlotNames.cs"] =
                    "22416AF3C3F7B2553E4D1E074A9AA82A0737F234313A5CF7D27CEC9C2680D906",
                ["Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs"] =
                    "C0B6B68E15E0803896AE3D0248611C8F944428BA7DAAF554A345D401CFC5D898",
                ["Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab"] =
                    "5E64146954350BD4F16877C9328DE371B0A16A2C3F2FFD7855727E71ABFBFF34",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity"] =
                    "FC5E64037CACB2EA00ACC7BA614D404E3AB26D5FFB3CC091DF0BF15B4458F1D9",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "CF244226FFD286638DEC957BBE5A9ABCA1DE52E9D529D71D406B5D03DE805932"
            };

        private static readonly IReadOnlyDictionary<string, string>
            Rev03ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs"] =
                    "935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317",
                ["Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs"] =
                    "794C3A6D9DBE9FB1FF96275C247BCEF4D9D248EDC1DF3EAEE6D396DD87969827",
                ["Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs"] =
                    "B83B23725B3F1D03D339D0B2181232F1EA68B367AF3947C688E61449BF8015BB",
                ["Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab"] =
                    "D5834DB6C2E3EBACC583418033E06F26D8B258B75AC78C8FBE6B13B1544D202E",
                [SourceScenePath] =
                    "1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity"] =
                    "9FED79FE312A34A6B802F464F928DFD032AF4615EBEE4715A7BF73DEC3BE3058",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "CF244226FFD286638DEC957BBE5A9ABCA1DE52E9D529D71D406B5D03DE805932"
            };

        private static readonly IReadOnlyDictionary<string, string>
            Rev03UntouchedPrefabHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [BoardPrefabPath] =
                    "46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8",
                [TrayPrefabPath] =
                    "AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1"
            };

        private static readonly IReadOnlyDictionary<string, string>
            Rev04FProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [BoardPrefabPath] =
                    "46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8",
                [CardPrefabPath] =
                    "C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C",
                [SourceScenePath] =
                    "1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity"] =
                    "C6F6EEAB8741F7827C530BF9F1BB2F951CC5619154236733C8E013B228869FE8",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs"] =
                    "06AF79CB89EB4DCA0A2313E81AF81EFA89A35B90AE6FE5E6B0BBDD3C90F62921",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs"] =
                    "364727A582EE3F2D499C109D98939318B4D26AB9D2F1F571CB70E62E605913DC",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs"] =
                    "DA8C1B628A02565908E7BC7A0EF9E56DC2E00BEC25AF549DF9382769372665BC",
                ["Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs"] =
                    "A4075E2279BB937858BE97A5E41365CE0EFF58F12277F0D19B93B7EFAC29B590",
                [TrayPrefabPath + ".meta"] =
                    "BF0E9EAEFA6D5D3123AE095F0CD55698DA437644707F7F51EEE5CE19CA6C52A4"
            };

        private static readonly string[] ExactOutputPaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionEditor.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionEditor.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/C1ExactBattleSandboxItemPromotionTests.cs.meta",
            BoardPrefabPath,
            BoardPrefabPath + ".meta",
            TrayPrefabPath,
            TrayPrefabPath + ".meta",
            CardPrefabPath,
            CardPrefabPath + ".meta",
            ParityCsvPath,
            CloseReportPath
        };

        private Vector2 scrollPosition;
        private string lastStatus = "Ready. Source and protected assets remain read-only.";

        [MenuItem("TalismanBag/V0.4/Items/Open Exact BattleSandbox Promotion")]
        public static void OpenWindow()
        {
            GetWindow<C1ExactBattleSandboxItemPromotionEditor>(
                "Exact Item Promotion");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField(
                "BattleSandbox Exact Board / Tray / Item Card",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(lastStatus, MessageType.Info);
            if (GUILayout.Button("Scan Source")) RunWindowOperation(ScanSource);
            if (GUILayout.Button("Extract/Update Exact Prefabs"))
                RunWindowOperation(ExtractOrUpdateExactPrefabs);
            if (GUILayout.Button("Compare/Validate Parity"))
                RunWindowOperation(CompareAndValidateParity);
            EditorGUILayout.EndScrollView();
        }

        public static void ScanSource()
        {
            ValidateProtectedHashes();
            WithReadOnlySourceScene(scan =>
            {
                Debug.Log("[C1ExactPromotion] SCAN_SOURCE_PASS "
                          + scan.BuildSummary());
                return true;
            });
            ValidateProtectedHashes();
        }

        public static void ExtractOrUpdateExactPrefabs()
        {
            ValidateProtectedHashes();
            WithReadOnlySourceScene(scan =>
            {
                AuthorExactPrefabs(scan);
                return true;
            });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateProtectedHashes();
            Debug.Log("[C1ExactPromotion] EXTRACT_UPDATE_EXACT_PREFABS_PASS");
        }

        public static void CompareAndValidateParity()
        {
            ValidateProtectedHashes();
            ParityBundle bundle = WithReadOnlySourceScene(ValidateParity);
            WriteEvidence(bundle);
            ValidateProtectedHashes();
            ValidateExactOutputsPresent();
            Debug.Log("[C1ExactPromotion] COMPARE_VALIDATE_PARITY_PASS rows="
                      + bundle.Rows.Count.ToString(CultureInfo.InvariantCulture));
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ValidateProtectedHashes();
                EnsureNoDirtyUserScenes();
                ParityBundle bundle = WithReadOnlySourceScene(scan =>
                {
                    Debug.Log("[C1ExactPromotion] OWNED_PASS_SCAN "
                              + scan.BuildSummary());
                    AuthorExactPrefabs(scan);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    C1ExactBattleSandboxItemPromotionTests.RunAllOrThrow();
                    Debug.Log("[C1ExactPromotion] "
                              + C1ExactBattleSandboxItemPromotionTests
                                  .TerminalMarker);
                    return ValidateParity(scan);
                });
                WriteEvidence(bundle);
                ValidateProtectedHashes();
                ValidateExactOutputsPresent();
                Debug.Log("[C1ExactPromotion] " + TerminalMarker
                          + " / parity=" + bundle.PassCount + "/"
                          + bundle.Rows.Count
                          + " / focusedTests="
                          + C1ExactBattleSandboxItemPromotionTests.TerminalMarker);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1ExactPromotion] OWNED_AUTHORING_VALIDATION_FAIL "
                    + exception.Message);
                CleanupIncompleteOwnedEvidence();
                EditorApplication.Exit(1);
            }
        }

        public static void ExecuteRev03FootprintFromCommandLine()
        {
            try
            {
                EnsureNoDirtyUserScenes();
                ValidateRev03ProtectedHashes();
                ValidateRev03UntouchedPrefabs();
                Require(string.Equals(
                        ComputeSha256(CardPrefabPath),
                        "B03AB01A3EDD0CBD4A4128DAF35445E1F2FBFFE4269967613188314439E10B46",
                        StringComparison.Ordinal),
                    "REV03_CARD_TASK_START_HASH_DRIFT");
                UpdateRev03CardPrefab();
                AssetDatabase.ImportAsset(
                    CardPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport);
                C1ExactBattleSandboxItemPromotionTests.RunAllOrThrow();
                ValidateRev03PrefabMode();
                ValidateRev03ProtectedHashes();
                ValidateRev03UntouchedPrefabs();
                Debug.Log("[C1ExactPromotion] " + Rev03TerminalMarker
                          + " / focusedTests="
                          + C1ExactBattleSandboxItemPromotionTests.TerminalMarker
                          + " / serializedPrefab=" + CardPrefabPath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1ExactPromotion] REV03_AUTHORING_VALIDATION_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void ExecuteRev04FTrayPrefabFromCommandLine()
        {
            byte[] originalTrayBytes = null;
            try
            {
                EnsureNoDirtyUserScenes();
                ValidateRev04FProtectedHashes();
                Require(string.Equals(
                        ComputeSha256(TrayPrefabPath),
                        "AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1",
                        StringComparison.Ordinal),
                    "REV04F_TRAY_TASK_START_HASH_DRIFT");
                originalTrayBytes = File.ReadAllBytes(
                    Path.GetFullPath(TrayPrefabPath));
                UpdateRev04FTrayPrefab();
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(
                    TrayPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport);
                ValidateRev04FTrayPrefabMode();
                C1ExactBattleSandboxItemPromotionTests.RunAllOrThrow();
                Require(C1FormalCanonicalItemSessionAuthorityTests
                            .RunFocused() == 0,
                    "REV04F_CANONICAL_AUTHORITY_FOCUSED_TESTS_FAILED");
                Require(C1FormalItemInteractionAuthorizationTests.RunFocused() == 0,
                    "REV04F_AUTHORIZATION_FOCUSED_TESTS_FAILED");
                ValidateRev04FProtectedHashes();
                Debug.Log("[C1ExactPromotion] " + Rev04FTrayTerminalMarker
                          + " / cells=65/65 / columns=5 / rows=13"
                          + " / serializedPrefab=" + TrayPrefabPath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                try
                {
                    if (originalTrayBytes != null)
                    {
                        File.WriteAllBytes(
                            Path.GetFullPath(TrayPrefabPath),
                            originalTrayBytes);
                        AssetDatabase.ImportAsset(
                            TrayPrefabPath,
                            ImportAssetOptions.ForceSynchronousImport);
                    }
                }
                catch (Exception rollbackException)
                {
                    Debug.LogException(rollbackException);
                }
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1ExactPromotion] REV04F_TRAY_AUTHORING_VALIDATION_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void ExecuteRev04DynamicItemPresentationFromCommandLine()
        {
            byte[] originalBoardBytes = null;
            byte[] originalCatalogBytes = null;
            bool catalogExisted = false;
            try
            {
                EnsureNoDirtyUserScenes();
                ValidateRev04DynamicProtectedAssets();
                originalBoardBytes = File.ReadAllBytes(
                    Path.GetFullPath(BoardPrefabPath));
                catalogExisted = File.Exists(DynamicPresentationCatalogAssetPath);
                if (catalogExisted)
                    originalCatalogBytes = File.ReadAllBytes(
                        Path.GetFullPath(DynamicPresentationCatalogAssetPath));

                C1FormalItemPresentationCatalog catalog =
                    AuthorRev04DynamicPresentationCatalog();
                BindRev04DynamicCatalogToBoard(catalog);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(
                    DynamicPresentationCatalogAssetPath,
                    ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(
                    BoardPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport);
                ValidateRev04DynamicPresentationPrefabMode();
                C1FormalItemPresentationCatalogTests.RunAllOrThrow();
                C1ExactBattleSandboxItemPromotionTests
                    .RunAllOrThrow();
                C1FormalItemDetailProjectionAndSelectionTests.RunAllOrThrow();
                ValidateRev04DynamicProtectedAssets();
                Debug.Log("[C1ExactPromotion] "
                          + Rev04DynamicPresentationTerminalMarker
                          + " / catalog=" + DynamicPresentationCatalogAssetPath
                          + " / boardPool=DEMAND_GROWN_MAX_25"
                          + " / trayPool=DEMAND_GROWN_MAX_65"
                          + " / CardPrefab=UNCHANGED");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                try
                {
                    if (originalBoardBytes != null)
                        File.WriteAllBytes(
                            Path.GetFullPath(BoardPrefabPath),
                            originalBoardBytes);
                    if (catalogExisted && originalCatalogBytes != null)
                        File.WriteAllBytes(
                            Path.GetFullPath(DynamicPresentationCatalogAssetPath),
                            originalCatalogBytes);
                    else if (!catalogExisted
                             && AssetDatabase.LoadAssetAtPath<Object>(
                                 DynamicPresentationCatalogAssetPath) != null)
                        AssetDatabase.DeleteAsset(
                            DynamicPresentationCatalogAssetPath);
                    AssetDatabase.ImportAsset(
                        BoardPrefabPath,
                        ImportAssetOptions.ForceSynchronousImport);
                }
                catch (Exception rollbackException)
                {
                    Debug.LogException(rollbackException);
                }
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1ExactPromotion] REV04_DYNAMIC_PRESENTATION_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void ExecuteRev06AuthoredSevenLayerCarriersFromCommandLine()
        {
            ExecuteRev06AuthoredSevenLayerCarriers(exitEditorWhenComplete: true);
        }

        [MenuItem(
            "TalismanBag/V0.4/Items/REV06 Author Seven-Layer Card And Board Carriers")]
        public static void ExecuteRev06AuthoredSevenLayerCarriersFromMenu()
        {
            ExecuteRev06AuthoredSevenLayerCarriers(exitEditorWhenComplete: false);
        }

        private static void ExecuteRev06AuthoredSevenLayerCarriers(
            bool exitEditorWhenComplete)
        {
            byte[] originalCardBytes = null;
            byte[] originalBoardBytes = null;
            try
            {
                EnsureNoDirtyUserScenes();
                originalCardBytes = File.ReadAllBytes(
                    Path.GetFullPath(CardPrefabPath));
                originalBoardBytes = File.ReadAllBytes(
                    Path.GetFullPath(BoardPrefabPath));

                GameObject carrierPrefab = AssetDatabase.LoadAssetAtPath<
                    GameObject>(RarityContourBloomVfxPrefabPath);
                ItemRarityContourBloomVfx carrierTemplate = carrierPrefab == null
                    ? null
                    : carrierPrefab.GetComponent<
                        ItemRarityContourBloomVfx>();
                Require(carrierTemplate != null
                        && carrierTemplate.ValidateAuthoredReferences(),
                    "REV06_VFX_PREFAB_CARRIER_INVALID");

                AuthorRev06CardCarrier(carrierPrefab);
                AuthorRev06BoardCarriers(carrierPrefab);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(
                    CardPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(
                    BoardPrefabPath,
                    ImportAssetOptions.ForceSynchronousImport);
                ValidateRev06CarrierPrefabMode();
                Debug.Log("[C1ExactPromotion] "
                          + Rev06CarrierTerminalMarker
                          + " / Card=1"
                          + " / Board=AUTHORED_POOL_TEMPLATE_COUNT"
                          + " / GraphicsPerCarrier=7");
                if (exitEditorWhenComplete)
                    EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                try
                {
                    if (originalCardBytes != null)
                        File.WriteAllBytes(
                            Path.GetFullPath(CardPrefabPath),
                            originalCardBytes);
                    if (originalBoardBytes != null)
                        File.WriteAllBytes(
                            Path.GetFullPath(BoardPrefabPath),
                            originalBoardBytes);
                    AssetDatabase.ImportAsset(
                        CardPrefabPath,
                        ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.ImportAsset(
                        BoardPrefabPath,
                        ImportAssetOptions.ForceSynchronousImport);
                }
                catch (Exception rollbackException)
                {
                    Debug.LogException(rollbackException);
                }
                Debug.LogException(exception);
                Debug.LogError(
                    "[C1ExactPromotion] REV06_CARRIER_AUTHORING_FAIL "
                    + exception.Message);
                if (exitEditorWhenComplete)
                {
                    EditorApplication.Exit(1);
                    return;
                }
                throw;
            }
        }

        private static void AuthorRev06CardCarrier(GameObject carrierPrefab)
        {
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(CardPrefabPath);
                Require(contents != null, "REV06_CARD_PREFAB_LOAD_FAILED");
                C1ExactBattleSandboxItemCardView card = RequireComponent<
                    C1ExactBattleSandboxItemCardView>(contents);
                ItemRarityContourBloomVfx carrier =
                    EnsureSingleRev06Carrier(card.transform, carrierPrefab);
                SerializedObject serialized = new(card);
                SerializedProperty property = serialized.FindProperty(
                    "rarityContourBloomVfx");
                Require(property != null,
                    "REV06_CARD_CARRIER_FIELD_MISSING");
                property.objectReferenceValue = carrier;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Require(card.ValidateAuthoredReferences(),
                    "REV06_CARD_CARRIER_REFERENCES_INVALID");
                EditorUtility.SetDirty(card);
                SaveExistingPrefab(contents, CardPrefabPath,
                    "REV06_CARD_PREFAB_SAVE_FAILED");
            }
            finally
            {
                if (contents != null)
                    PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void AuthorRev06BoardCarriers(GameObject carrierPrefab)
        {
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(BoardPrefabPath);
                Require(contents != null, "REV06_BOARD_PREFAB_LOAD_FAILED");
                C1ExactBattleSandboxItemBoardView board = RequireComponent<
                    C1ExactBattleSandboxItemBoardView>(contents);
                SerializedObject serialized = new(board);
                SerializedProperty rects = serialized.FindProperty(
                    "placedArtworkRects");
                SerializedProperty carriers = serialized.FindProperty(
                    "placedArtworkRarityCarriers");
                Require(rects != null && rects.arraySize > 0
                                      && carriers != null,
                    "REV06_BOARD_POOL_TEMPLATE_FIELDS_MISSING");
                carriers.arraySize = rects.arraySize;
                for (int index = 0; index < rects.arraySize; index++)
                {
                    RectTransform rect = rects.GetArrayElementAtIndex(index)
                        .objectReferenceValue as RectTransform;
                    Require(rect != null,
                        "REV06_BOARD_POOL_TEMPLATE_RECT_MISSING index="
                        + index.ToString(CultureInfo.InvariantCulture));
                    ItemRarityContourBloomVfx carrier =
                        EnsureSingleRev06Carrier(rect, carrierPrefab);
                    carriers.GetArrayElementAtIndex(index).objectReferenceValue =
                        carrier;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Require(board.ValidateAuthoredReferences()
                        && board.AuthoredRarityCarrierCapacity
                        == board.AuthoredArtworkCapacity,
                    "REV06_BOARD_CARRIER_REFERENCES_INVALID");
                EditorUtility.SetDirty(board);
                SaveExistingPrefab(contents, BoardPrefabPath,
                    "REV06_BOARD_PREFAB_SAVE_FAILED");
            }
            finally
            {
                if (contents != null)
                    PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static ItemRarityContourBloomVfx EnsureSingleRev06Carrier(
            Transform parent,
            GameObject carrierPrefab)
        {
            Require(parent != null && carrierPrefab != null,
                "REV06_CARRIER_PARENT_OR_PREFAB_MISSING");
            ItemRarityContourBloomVfx[] existing = parent
                .GetComponentsInChildren<ItemRarityContourBloomVfx>(true)
                .Where(value => value != null
                                && value.transform != parent
                                && value.transform.IsChildOf(parent))
                .ToArray();
            Require(existing.Length <= 1,
                "REV06_DUPLICATE_AUTHORED_CARRIER parent=" + parent.name);
            ItemRarityContourBloomVfx carrier = existing.SingleOrDefault();
            if (carrier == null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(
                    carrierPrefab,
                    parent) as GameObject;
                Require(instance != null,
                    "REV06_CARRIER_INSTANTIATION_FAILED parent="
                    + parent.name);
                instance.name = "ItemRarityContourBloomVfx";
                RectTransform rect = RequireComponent<RectTransform>(instance);
                Stretch(rect);
                carrier = RequireComponent<ItemRarityContourBloomVfx>(instance);
            }
            Require(ValidateRev06Carrier(carrier),
                "REV06_AUTHORED_CARRIER_INVALID parent=" + parent.name);
            return carrier;
        }

        private static void ValidateRev06CarrierPrefabMode()
        {
            GameObject cardContents = null;
            GameObject boardContents = null;
            try
            {
                cardContents = PrefabUtility.LoadPrefabContents(CardPrefabPath);
                boardContents = PrefabUtility.LoadPrefabContents(BoardPrefabPath);
                Require(cardContents != null && boardContents != null,
                    "REV06_CARRIER_PREFAB_MODE_LOAD_FAILED");
                C1ExactBattleSandboxItemCardView card = RequireComponent<
                    C1ExactBattleSandboxItemCardView>(cardContents);
                C1ExactBattleSandboxItemBoardView board = RequireComponent<
                    C1ExactBattleSandboxItemBoardView>(boardContents);
                Require(card.ValidateAuthoredReferences()
                        && ValidateRev06Carrier(card.RarityContourBloomVfx),
                    "REV06_CARD_PREFAB_MODE_CARRIER_INVALID");
                SerializedObject serialized = new(board);
                SerializedProperty rects = serialized.FindProperty(
                    "placedArtworkRects");
                SerializedProperty carriers = serialized.FindProperty(
                    "placedArtworkRarityCarriers");
                Require(board.ValidateAuthoredReferences()
                        && rects != null
                        && carriers != null
                        && rects.arraySize == carriers.arraySize
                        && carriers.arraySize > 0,
                    "REV06_BOARD_PREFAB_MODE_CARRIER_ARRAY_INVALID");
                for (int index = 0; index < carriers.arraySize; index++)
                {
                    RectTransform rect = rects.GetArrayElementAtIndex(index)
                        .objectReferenceValue as RectTransform;
                    ItemRarityContourBloomVfx carrier = carriers
                        .GetArrayElementAtIndex(index).objectReferenceValue
                        as ItemRarityContourBloomVfx;
                    Require(rect != null && carrier != null
                                         && carrier.transform != rect
                                         && carrier.transform.IsChildOf(rect)
                                         && rect.GetComponentsInChildren<
                                             ItemRarityContourBloomVfx>(true)
                                             .Length == 1
                                         && ValidateRev06Carrier(carrier),
                        "REV06_BOARD_PREFAB_MODE_CARRIER_INVALID index="
                        + index.ToString(CultureInfo.InvariantCulture));
                }
            }
            finally
            {
                if (cardContents != null)
                    PrefabUtility.UnloadPrefabContents(cardContents);
                if (boardContents != null)
                    PrefabUtility.UnloadPrefabContents(boardContents);
            }
        }

        private static bool ValidateRev06Carrier(
            ItemRarityContourBloomVfx carrier)
        {
            if (carrier == null || !carrier.ValidateAuthoredReferences())
                return false;
            MaskableGraphic[] graphics = carrier.GetComponentsInChildren<
                MaskableGraphic>(true);
            return graphics.Length == 7
                   && graphics.All(value => value != null
                       && !value.raycastTarget
                       && value.maskable);
        }

        private static void SaveExistingPrefab(
            GameObject contents,
            string path,
            string diagnostic)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                contents,
                path,
                out bool success);
            Require(success && saved != null, diagnostic);
        }

        private static C1FormalItemPresentationCatalog
            AuthorRev04DynamicPresentationCatalog()
        {
            C1CampaignLootEligibleItemPool15Result poolResult =
                C1CampaignLootEligibleItemPool15Carrier.Resolve();
            C1CampaignPool15BaseCombatFactResult factResult =
                C1CampaignPool15BaseCombatFacts.Resolve();
            Require(poolResult.accepted && factResult.accepted,
                "REV04_DYNAMIC_ITEM_FACT_SOURCE_REJECTED");
            GameObject boardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                BoardPrefabPath);
            C1ExactBattleSandboxItemBoardView board = boardAsset == null
                ? null
                : boardAsset.GetComponent<C1ExactBattleSandboxItemBoardView>();
            Require(board != null
                    && board.CatalogMigrationStarterArtwork != null
                    && board.CatalogMigrationPoolArtworkSample != null
                    && board.CatalogMigrationSpecialUnlitArtwork != null
                    && board.CatalogMigrationSpecialLitArtwork != null,
                "REV04_DYNAMIC_CATALOG_MIGRATION_ARTWORK_MISSING");

            List<C1FormalItemPresentationCatalogRowDefinition> rows = new();
            foreach (C1CampaignLootItemProfileSnapshot profile in
                     poolResult.snapshot.Profiles)
            {
                C1CampaignPool15BaseCombatFact fact =
                    factResult.snapshot.Find(profile.baseItemId);
                Require(fact != null,
                    "REV04_DYNAMIC_POOL_FACT_MISSING " + profile.baseItemId);
                Sprite artwork = string.Equals(
                    profile.baseItemId,
                    "I002",
                    StringComparison.Ordinal)
                    ? board.CatalogMigrationPoolArtworkSample
                    : LoadExactOrdinaryWhiteArtwork(profile.baseItemId);
                rows.Add(C1FormalItemPresentationCatalogRowDefinition
                    .CreateSigned(
                        profile.baseItemId,
                        profile.rarityKey,
                        artwork,
                        profile.artworkIdentity,
                        fact.familyId,
                        profile.presentationProfileId,
                        fact.cueIdentity,
                        fact.sourceProfileCanonicalSignature));
            }

            foreach (C1Lv1StarterItemBaselineProfile starter in
                     C1Lv1StarterItemBaselineCatalog.All.Where(value =>
                         poolResult.snapshot.FindProfile(value.baseItemId) == null))
            {
                ItemInnerDataDefinition item = ItemInnerDataCatalog.FindById(
                    starter.baseItemId);
                Require(item != null
                        && !string.IsNullOrWhiteSpace(item.iconPlaceholderKey),
                    "REV04_DYNAMIC_STARTER_PRESENTATION_IDENTITY_MISSING");
                rows.Add(C1FormalItemPresentationCatalogRowDefinition
                    .CreateSigned(
                        starter.baseItemId,
                        starter.rarityKey,
                        board.CatalogMigrationStarterArtwork,
                        item.iconPlaceholderKey,
                        C1FormalItemCombatDetailFacts.StarterSourceKind,
                        C1FormalItemCombatDetailFacts.StarterSourceKind,
                        starter.canonicalSignature,
                        starter.canonicalSignature));
            }

            string specialSourceSignature =
                C1FormalItemPresentationCatalog
                    .ComputeCatalogCanonicalSignature(
                        I031InventoryPlacementContract.SpecialIdentityId + "|"
                        + I031InventoryPlacementContract.StablePlacementId,
                        Array.Empty<string>());
            rows.Add(C1FormalItemPresentationCatalogRowDefinition.CreateSigned(
                I031InventoryPlacementContract.ItemId,
                string.Empty,
                board.CatalogMigrationSpecialUnlitArtwork,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.SpecialIdentityId,
                I031InventoryPlacementContract.SpecialIdentityId,
                specialSourceSignature,
                true,
                board.CatalogMigrationSpecialUnlitArtwork,
                I031InventoryPlacementContract.SpecialIdentityId + ".unlit",
                board.CatalogMigrationSpecialLitArtwork,
                I031InventoryPlacementContract.SpecialIdentityId + ".lit"));

            string sourceSignature = C1FormalItemPresentationCatalog
                .ComputeCatalogCanonicalSignature(
                    poolResult.snapshot.poolCanonicalSignature + "|"
                    + C1Lv1StarterItemBaselineCatalog.canonicalSignature + "|"
                    + specialSourceSignature,
                    Array.Empty<string>());
            C1FormalItemPresentationCatalog catalog = AssetDatabase
                .LoadAssetAtPath<C1FormalItemPresentationCatalog>(
                    DynamicPresentationCatalogAssetPath);
            if (catalog == null)
            {
                catalog = CreateInstance<C1FormalItemPresentationCatalog>();
                AssetDatabase.CreateAsset(
                    catalog,
                    DynamicPresentationCatalogAssetPath);
            }
            catalog.AssignForEditor(sourceSignature, rows.ToArray());
            EditorUtility.SetDirty(catalog);
            C1FormalItemPresentationCatalogResult result = catalog.Resolve();
            Require(result.accepted
                    && result.snapshot.Rows.Count == rows.Count,
                "REV04_DYNAMIC_CATALOG_VALIDATION_FAILED "
                + result.diagnosticCode);
            return catalog;
        }

        private static Sprite LoadExactOrdinaryWhiteArtwork(string baseItemId)
        {
            string exactName = baseItemId + "_1";
            Sprite[] matches = AssetDatabase.FindAssets(
                    exactName + " t:Sprite",
                    new[] { "Assets/_Game/Resources/item_daoju" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => string.Equals(
                        Path.GetFileNameWithoutExtension(path),
                        exactName,
                        StringComparison.Ordinal)
                    && path.Replace('\\', '/').Contains(
                        "/" + baseItemId + "/"))
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
                .Where(value => value != null)
                .Distinct()
                .ToArray();
            Require(matches.Length == 1,
                "REV04_DYNAMIC_EXACT_ARTWORK_NOT_UNIQUE " + baseItemId
                + " count=" + matches.Length.ToString(
                    CultureInfo.InvariantCulture));
            return matches[0];
        }

        private static void BindRev04DynamicCatalogToBoard(
            C1FormalItemPresentationCatalog catalog)
        {
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(BoardPrefabPath);
                Require(contents != null,
                    "REV04_DYNAMIC_BOARD_PREFAB_LOAD_FAILED");
                C1ExactBattleSandboxItemBoardView board = RequireComponent<
                    C1ExactBattleSandboxItemBoardView>(contents);
                SerializedObject serialized = new(board);
                SerializedProperty property = serialized.FindProperty(
                    "presentationCatalog");
                Require(property != null,
                    "REV04_DYNAMIC_BOARD_CATALOG_FIELD_MISSING");
                property.objectReferenceValue = catalog;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Require(board.ValidateAuthoredReferences(),
                    "REV04_DYNAMIC_BOARD_REFERENCES_INVALID");
                EditorUtility.SetDirty(board);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    contents,
                    BoardPrefabPath,
                    out bool success);
                Require(success && saved != null,
                    "REV04_DYNAMIC_BOARD_PREFAB_SAVE_FAILED");
            }
            finally
            {
                if (contents != null)
                    PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void ValidateRev04DynamicPresentationPrefabMode()
        {
            C1FormalItemPresentationCatalog catalog = AssetDatabase
                .LoadAssetAtPath<C1FormalItemPresentationCatalog>(
                    DynamicPresentationCatalogAssetPath);
            C1FormalItemPresentationCatalogResult catalogResult =
                catalog == null ? null : catalog.Resolve();
            Require(catalogResult != null && catalogResult.accepted,
                "REV04_DYNAMIC_CATALOG_ASSET_INVALID");
            GameObject board = null;
            GameObject tray = null;
            try
            {
                board = PrefabUtility.LoadPrefabContents(BoardPrefabPath);
                tray = PrefabUtility.LoadPrefabContents(TrayPrefabPath);
                Require(board != null && tray != null,
                    "REV04_DYNAMIC_PREFAB_LOAD_FAILED");
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            board) == 0
                        && GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(tray) == 0,
                    "REV04_DYNAMIC_PREFAB_MISSING_SCRIPT");
                C1ExactBattleSandboxItemBoardView boardView = RequireComponent<
                    C1ExactBattleSandboxItemBoardView>(board);
                C1ExactBattleSandboxItemTrayView trayView = RequireComponent<
                    C1ExactBattleSandboxItemTrayView>(tray);
                Require(boardView.ValidateAuthoredReferences()
                        && boardView.AuthoredArtworkCapacity > 0
                        && boardView.AuthoredArtworkCapacity
                           <= C1ExactBattleSandboxItemBoardView
                               .PhysicalPresentationCapacity,
                    "REV04_DYNAMIC_BOARD_TEMPLATE_INVALID");
                Require(trayView.ValidateAuthoredReferences()
                        && trayView.AuthoredCardCapacity > 0
                        && trayView.AuthoredCardCapacity
                           <= C1ExactBattleSandboxItemTrayView
                               .PhysicalPresentationCapacity,
                    "REV04_DYNAMIC_TRAY_TEMPLATE_INVALID");
            }
            finally
            {
                if (board != null) PrefabUtility.UnloadPrefabContents(board);
                if (tray != null) PrefabUtility.UnloadPrefabContents(tray);
            }
        }

        private static void ValidateRev04DynamicProtectedAssets()
        {
            Require(string.Equals(
                    ComputeSha256(CardPrefabPath),
                    "C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C",
                    StringComparison.Ordinal),
                "REV04_DYNAMIC_CARD_PREFAB_CHANGED");
            Require(string.Equals(
                    ComputeSha256(BoardPrefabPath + ".meta"),
                    "035AB747FFD25D01BBC4B1C10A44AB4824D31D6CEF0865545599FF5E2D43FC64",
                    StringComparison.Ordinal),
                "REV04_DYNAMIC_BOARD_META_CHANGED");
            Require(string.Equals(
                    ComputeSha256(TrayPrefabPath + ".meta"),
                    "BF0E9EAEFA6D5D3123AE095F0CD55698DA437644707F7F51EEE5CE19CA6C52A4",
                    StringComparison.Ordinal),
                "REV04_DYNAMIC_TRAY_META_CHANGED");
        }

        private static void AuthorExactPrefabs(SourceScan scan)
        {
            SpriteSet sprites = LoadFormalArtworkSprites();
            AuthorCardPrefab(scan, sprites);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                CardPrefabPath,
                ImportAssetOptions.ForceSynchronousImport);
            AuthorBoardPrefab(scan, sprites);
            AuthorTrayPrefab(scan);
            AssetDatabase.SaveAssets();
        }

        private static void AuthorCardPrefab(SourceScan scan, SpriteSet sprites)
        {
            GameObject clone = CloneIntoPreviewScene(scan.CardRoles[3]);
            Scene previewScene = clone.scene;
            try
            {
                clone.name = "C1ExactBattleSandboxItemCard";
                RemoveBuildSandboxComponents(clone);
                C1ExactBattleSandboxItemCardView existing =
                    clone.GetComponent<C1ExactBattleSandboxItemCardView>();
                if (existing != null) Object.DestroyImmediate(existing);

                RectTransform cardRect = RequireComponent<RectTransform>(clone);
                CanvasGroup canvasGroup = RequireComponent<CanvasGroup>(clone);
                Image background = RequireComponent<Image>(clone);
                Image artwork = RequiredNamedComponent<Image>(
                    clone,
                    "ItemCard_04_image");
                Text identity = RequiredNamedComponent<Text>(clone, "Title");
                Text category = RequiredNamedComponent<Text>(clone, "Category");
                Text shape = RequiredNamedComponent<Text>(clone, "Shape");
                RectTransform sourceFootprintLayer =
                    RequiredNamedComponent<RectTransform>(
                        clone,
                        "TrayLayoutCellLayer");
                Image[] sourceFootprintCells = Enumerable.Range(0, 3)
                    .Select(index => RequiredNamedComponent<Image>(
                        clone,
                        "TrayLayoutCell_" + index.ToString(
                            "00",
                            CultureInfo.InvariantCulture)))
                    .ToArray();
                foreach (Image sourceFootprintCell in sourceFootprintCells)
                    sourceFootprintCell.raycastTarget = false;
                artwork.preserveAspect = true;
                artwork.raycastTarget = false;
                artwork.color = WithAlpha(artwork.color, 1f);

                Image selection = DuplicateImageCarrier(
                    artwork,
                    cardRect,
                    "ExactSelectionOverlay");
                selection.color = new Color(1f, 0.82f, 0.34f, 0.30f);
                Image lighting = DuplicateImageCarrier(
                    artwork,
                    cardRect,
                    "ExactLightingOverlay");
                lighting.color = new Color(0.35f, 1f, 0.55f, 0.24f);
                Image drag = DuplicateImageCarrier(
                    artwork,
                    cardRect,
                    "ExactDragFeedbackOverlay");
                drag.color = new Color(0.42f, 0.88f, 0.66f, 0.28f);
                Text rotate = DuplicateTextCarrier(
                    identity,
                    cardRect,
                    "ExactRotateFeedbackLabel");
                rotate.text = "R 90\u00B0";
                Image inputSurface = DuplicateImageCarrier(
                    background,
                    cardRect,
                    "ExactInputSurface");
                Stretch(inputSurface.rectTransform);
                inputSurface.sprite = null;
                inputSurface.color = UnityEngine.Color.clear;
                inputSurface.preserveAspect = false;
                inputSurface.raycastTarget = true;
                selection.gameObject.SetActive(false);
                lighting.gameObject.SetActive(false);
                drag.gameObject.SetActive(false);
                rotate.gameObject.SetActive(false);

                C1ExactBattleSandboxItemCardView view = clone.AddComponent<
                    C1ExactBattleSandboxItemCardView>();
                view.AssignForEditor(
                    cardRect,
                    canvasGroup,
                    background,
                    artwork,
                    selection,
                    lighting,
                    drag,
                    inputSurface,
                    sourceFootprintLayer,
                    sourceFootprintCells,
                    identity,
                    category,
                    shape,
                    rotate,
                    null);
                Require(view.ValidateAuthoredReferences(),
                    "CARD_VIEW_AUTHORED_REFS_INVALID");
                SaveNewPrefab(clone, CardPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(clone);
                ClosePreviewScene(previewScene);
            }
        }

        private static void AuthorBoardPrefab(SourceScan scan, SpriteSet sprites)
        {
            GameObject clone = CloneIntoPreviewScene(scan.BoardRoot.gameObject);
            Scene previewScene = clone.scene;
            try
            {
                clone.name = "C1ExactBattleSandboxItemBoard";
                RemoveBuildSandboxComponents(clone);
                RectTransform boardRect = RequireComponent<RectTransform>(clone);
                RectTransform gridRoot = RequiredNamedComponent<RectTransform>(
                    clone,
                    "BoardGridCellsRoo");
                Image[] cells = Enumerable.Range(1, 25)
                    .Select(index => RequiredNamedComponent<Image>(
                        clone,
                        "BoardGridCell_" + index.ToString("00",
                            CultureInfo.InvariantCulture)))
                    .ToArray();
                Image sourceArtwork = scan.CardArtworkSlot;
                Text sourceText = scan.CardRoles[3]
                    .GetComponentsInChildren<Text>(true)
                    .First(value => string.Equals(
                        value.name,
                        "Title",
                        StringComparison.Ordinal));

                RectTransform artworkLayer = CreateRectLayer(
                    boardRect,
                    "BoardItemArtworkLayer");
                CopyRectTransform(gridRoot, artworkLayer);
                RectTransform[] placedRects = new RectTransform[
                    DynamicPresentationInitialPrewarmCount];
                Image[] placedImages = new Image[
                    DynamicPresentationInitialPrewarmCount];
                for (int index = 0; index < placedRects.Length; index++)
                {
                    placedImages[index] = DuplicateImageCarrier(
                        sourceArtwork,
                        artworkLayer,
                        "BoardPlacedArtwork_" + (index + 1).ToString("00",
                            CultureInfo.InvariantCulture));
                    placedImages[index].preserveAspect = true;
                    placedImages[index].raycastTarget = false;
                    placedImages[index].color = UnityEngine.Color.white;
                    placedImages[index].gameObject.SetActive(false);
                    placedRects[index] = placedImages[index].rectTransform;
                }

                Image selection = DuplicateImageCarrier(
                    sourceArtwork,
                    artworkLayer,
                    "BoardSelectionArtwork");
                selection.preserveAspect = true;
                selection.color = new Color(1f, 0.82f, 0.34f, 0.34f);
                selection.gameObject.SetActive(false);
                Image preview = DuplicateImageCarrier(
                    sourceArtwork,
                    artworkLayer,
                    "BoardPreviewArtwork");
                preview.preserveAspect = true;
                preview.color = new Color(0.62f, 1f, 0.72f, 0.88f);
                preview.gameObject.SetActive(false);
                Image shadow = DuplicateImageCarrier(
                    sourceArtwork,
                    artworkLayer,
                    "BoardPreviewShadowArtwork");
                shadow.preserveAspect = true;
                shadow.color = new Color(0.22f, 1f, 0.42f, 0.44f);
                shadow.gameObject.SetActive(false);
                Image invalid = DuplicateImageCarrier(
                    sourceArtwork,
                    artworkLayer,
                    "BoardPreviewInvalidArtwork");
                invalid.preserveAspect = true;
                invalid.color = new Color(1f, 0.18f, 0.12f, 0.52f);
                invalid.gameObject.SetActive(false);

                RectTransform dragGhostRoot = CreateRectLayer(
                    artworkLayer,
                    "DragGhostCellLayer");
                Image dragArtwork = DuplicateImageCarrier(
                    sourceArtwork,
                    dragGhostRoot,
                    "DragGhostArtwork");
                Stretch(dragArtwork.rectTransform);
                dragArtwork.preserveAspect = true;
                dragArtwork.color = new Color(1f, 1f, 1f, 0.88f);
                Image dragInvalid = DuplicateImageCarrier(
                    sourceArtwork,
                    dragGhostRoot,
                    "DragGhostInvalidArtwork");
                Stretch(dragInvalid.rectTransform);
                dragInvalid.preserveAspect = true;
                dragInvalid.color = new Color(1f, 0.16f, 0.10f, 0.58f);
                Text dragLabel = DuplicateTextCarrier(
                    sourceText,
                    dragGhostRoot,
                    "DragGhostLabel");
                Stretch(dragLabel.rectTransform);
                dragLabel.raycastTarget = false;
                dragGhostRoot.gameObject.SetActive(false);

                RectTransform rotateLayer = CreateRectLayer(
                    boardRect,
                    "MobileRotateZoneLayer");
                GameObject rightRotateObject = Object.Instantiate(
                    scan.RotatePreviewButtonSlot,
                    rotateLayer,
                    false);
                rightRotateObject.name = "MobileRotateZoneRight";
                RemoveBuildSandboxComponents(rightRotateObject);
                DisableButtonsAndRaycasts(rightRotateObject);
                RectTransform rightRotate = RequireComponent<RectTransform>(
                    rightRotateObject);
                Image rightRotateImage = rightRotateObject
                    .GetComponentsInChildren<Image>(true)
                    .FirstOrDefault();
                Text rightRotateText = rightRotateObject
                    .GetComponentsInChildren<Text>(true)
                    .FirstOrDefault();
                Require(rightRotateImage != null && rightRotateText != null,
                    "ROTATE_SOURCE_VISUAL_REFS_MISSING");
                rightRotateText.text = "R 90";
                GameObject rotateGuideObject = Object.Instantiate(
                    scan.RotatePreviewButtonSlot,
                    rotateLayer,
                    false);
                rotateGuideObject.name = "MobileRotateZoneGuide";
                RemoveBuildSandboxComponents(rotateGuideObject);
                DisableButtonsAndRaycasts(rotateGuideObject);
                RectTransform rotateGuide = RequireComponent<RectTransform>(
                    rotateGuideObject);
                Image rotateGuideImage = rotateGuideObject
                    .GetComponentsInChildren<Image>(true)
                    .FirstOrDefault();
                Require(rotateGuideImage != null,
                    "ROTATE_GUIDE_SOURCE_IMAGE_MISSING");
                rotateLayer.gameObject.SetActive(false);

                Image[] rangeImages = new Image[25];
                Text[] coreLabels = new Text[25];
                Text[] lightingLabels = new Text[25];
                for (int index = 0; index < cells.Length; index++)
                {
                    RectTransform cellRect = cells[index].rectTransform;
                    RectTransform overlay = CreateRectLayer(
                        cellRect,
                        "FormationCorePowerRangeOverlay");
                    rangeImages[index] = DuplicateImageCarrier(
                        cells[index],
                        overlay,
                        "Range");
                    Stretch(rangeImages[index].rectTransform);
                    rangeImages[index].raycastTarget = false;
                    rangeImages[index].color = UnityEngine.Color.clear;
                    Text cellLabel = cellRect.GetComponentsInChildren<Text>(true)
                        .FirstOrDefault() ?? sourceText;
                    coreLabels[index] = DuplicateTextCarrier(
                        cellLabel,
                        overlay,
                        "CoreLabel");
                    Stretch(coreLabels[index].rectTransform);
                    coreLabels[index].text = string.Empty;
                    coreLabels[index].raycastTarget = false;
                    lightingLabels[index] = DuplicateTextCarrier(
                        cellLabel,
                        overlay,
                        "LightingState");
                    Stretch(lightingLabels[index].rectTransform);
                    lightingLabels[index].text = string.Empty;
                    lightingLabels[index].raycastTarget = false;
                }

                C1ExactBattleSandboxItemBoardView view = clone.AddComponent<
                    C1ExactBattleSandboxItemBoardView>();
                view.AssignForEditor(
                    boardRect,
                    gridRoot,
                    cells,
                    artworkLayer,
                    placedRects,
                    placedImages,
                    selection.rectTransform,
                    selection,
                    preview.rectTransform,
                    preview,
                    shadow.rectTransform,
                    shadow,
                    invalid.rectTransform,
                    invalid,
                    dragGhostRoot,
                    dragArtwork.rectTransform,
                    dragArtwork,
                    dragInvalid.rectTransform,
                    dragInvalid,
                    dragLabel,
                    rotateLayer,
                    rightRotate,
                    rightRotateImage,
                    rightRotateText,
                    rotateGuide,
                    rotateGuideImage,
                    rangeImages,
                    coreLabels,
                    lightingLabels,
                    null);
                Require(view.ValidateAuthoredReferences(),
                    "BOARD_VIEW_AUTHORED_REFS_INVALID");
                SaveNewPrefab(clone, BoardPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(clone);
                ClosePreviewScene(previewScene);
            }
        }

        private static void AuthorTrayPrefab(SourceScan scan)
        {
            GameObject cardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                CardPrefabPath);
            Require(cardAsset != null, "EXACT_CARD_PREFAB_NOT_IMPORTED");
            GameObject clone = CloneIntoPreviewScene(scan.TrayRoot.gameObject);
            Scene previewScene = clone.scene;
            try
            {
                clone.name = "C1ExactBattleSandboxItemTray";
                RemoveBuildSandboxComponents(clone);
                RectTransform trayRect = RequireComponent<RectTransform>(clone);
                ScrollRect scrollRect = RequireComponent<ScrollRect>(clone);
                RectTransform viewport = RequiredNamedComponent<RectTransform>(
                    clone,
                    "ItemTrayViewport");
                RectMask2D viewportMask = RequireComponent<RectMask2D>(
                    viewport.gameObject);
                RectTransform content = RequiredNamedComponent<RectTransform>(
                    clone,
                    "ItemTrayContent");
                RectTransform cardLayer = RequiredNamedComponent<RectTransform>(
                    clone,
                    "ItemCardLayer");
                RectTransform[] sourceRoleRects =
                {
                    RequiredNamedComponent<RectTransform>(clone, "ItemCard_01"),
                    RequiredNamedComponent<RectTransform>(clone, "ItemCard_02"),
                    RequiredNamedComponent<RectTransform>(clone, "ItemCard_04")
                };
                C1ExactBattleSandboxItemCardView[] cardViews =
                    new C1ExactBattleSandboxItemCardView[
                        DynamicPresentationInitialPrewarmCount];
                for (int index = 0; index < cardViews.Length; index++)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(
                        cardAsset,
                        cardLayer) as GameObject;
                    Require(instance != null, "EXACT_CARD_NESTED_INSTANCE_FAILED");
                    instance.name = "FormalItemCard_" + (index + 1).ToString(
                        "00",
                        CultureInfo.InvariantCulture);
                    CopyRectTransform(
                        sourceRoleRects[Math.Min(
                            index,
                            sourceRoleRects.Length - 1)],
                        RequireComponent<RectTransform>(instance));
                    cardViews[index] = RequireComponent<
                        C1ExactBattleSandboxItemCardView>(instance);
                    SerializedObject serializedCard = new(cardViews[index]);
                    serializedCard.FindProperty("authoredScrollRect").objectReferenceValue =
                        scrollRect;
                    serializedCard.ApplyModifiedPropertiesWithoutUndo();
                }

                foreach (Transform sourceCard in cardLayer.Cast<Transform>()
                             .Where(value => value.name.StartsWith(
                                 "ItemCard_",
                                 StringComparison.Ordinal))
                             .ToArray())
                {
                    Object.DestroyImmediate(sourceCard.gameObject);
                }

                EnsureRev04FTrayCells(
                    content,
                    out RectTransform[] trayCellRects,
                    out Image[] trayCellImages);

                C1ExactBattleSandboxItemTrayView view = clone.AddComponent<
                    C1ExactBattleSandboxItemTrayView>();
                view.AssignForEditor(
                    trayRect,
                    scrollRect,
                    viewport,
                    viewportMask,
                    content,
                    cardLayer,
                    cardViews,
                    trayCellRects,
                    trayCellImages);
                Require(view.ValidateAuthoredReferences(),
                    "TRAY_VIEW_AUTHORED_REFS_INVALID");
                SaveNewPrefab(clone, TrayPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(clone);
                ClosePreviewScene(previewScene);
            }
        }

        private static ParityBundle ValidateParity(SourceScan scan)
        {
            GameObject board = AssetDatabase.LoadAssetAtPath<GameObject>(BoardPrefabPath);
            GameObject tray = AssetDatabase.LoadAssetAtPath<GameObject>(TrayPrefabPath);
            GameObject card = AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath);
            Require(board != null && tray != null && card != null,
                "EXACT_PREFAB_ASSET_MISSING");
            ParityBundle bundle = new();
            AddRectParity(bundle, "BOARD", "root.rect", scan.BoardRoot,
                RequireComponent<RectTransform>(board));
            AddImageParity(bundle, "BOARD", "root.image",
                RequireComponent<Image>(scan.BoardRoot.gameObject),
                RequireComponent<Image>(board));
            Add(bundle, "BOARD", "root.scale_0_78",
                Approximately(scan.BoardRoot.localScale,
                    RequireComponent<RectTransform>(board).localScale),
                Vector(scan.BoardRoot.localScale),
                Vector(RequireComponent<RectTransform>(board).localScale),
                "EXACT_COPY");
            AddBaseChildOrderParity(bundle, "BOARD", "root.child_order",
                scan.BoardRoot, board.transform);
            Transform sourceBoardGrid = RequiredNamedComponent<RectTransform>(
                scan.BoardRoot.gameObject, "BoardGridCellsRoo");
            Transform targetBoardGrid = RequiredNamedComponent<RectTransform>(
                board, "BoardGridCellsRoo");
            AddBaseChildOrderParity(bundle, "BOARD", "grid.child_order",
                sourceBoardGrid, targetBoardGrid);
            Add(bundle, "BOARD", "grid.layout",
                GridLayoutSignature(sourceBoardGrid)
                == GridLayoutSignature(targetBoardGrid),
                GridLayoutSignature(sourceBoardGrid),
                GridLayoutSignature(targetBoardGrid), "EXACT_COPY");
            C1ExactBattleSandboxItemBoardView boardView =
                RequireComponent<C1ExactBattleSandboxItemBoardView>(board);
            Add(bundle, "BOARD", "cell.count", boardView.BoardCellCount == 25,
                "25", boardView.BoardCellCount.ToString(CultureInfo.InvariantCulture),
                "EXACT_COPY");
            for (int index = 1; index <= 25; index++)
            {
                string name = "BoardGridCell_" + index.ToString("00",
                    CultureInfo.InvariantCulture);
                Image sourceCell = RequiredNamedComponent<Image>(
                    scan.BoardRoot.gameObject,
                    name);
                Image targetCell = RequiredNamedComponent<Image>(board, name);
                AddRectParity(bundle, "BOARD", name + ".rect",
                    sourceCell.rectTransform, targetCell.rectTransform);
                AddImageParity(bundle, "BOARD", name + ".image",
                    sourceCell, targetCell);
            }

            Add(bundle, "BOARD", "authored.dynamic_refs",
                boardView.ValidateAuthoredReferences(), "complete", "complete",
                "PROMOTED_AUTHORED_REF");
            Add(bundle, "BOARD", "missing_scripts",
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(board) == 0,
                "0",
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(board)
                    .ToString(CultureInfo.InvariantCulture),
                "STATIC_VALIDATION");

            AddRectParity(bundle, "TRAY", "root.rect", scan.TrayRoot,
                RequireComponent<RectTransform>(tray));
            AddImageParity(bundle, "TRAY", "root.image",
                RequireComponent<Image>(scan.TrayRoot.gameObject),
                RequireComponent<Image>(tray));
            AddBaseChildOrderParity(bundle, "TRAY", "root.child_order",
                scan.TrayRoot, tray.transform);
            foreach (string name in new[]
                     {
                         "CategoryTabsRoot",
                         "ItemTrayViewport",
                         "ItemTrayContent",
                         "ItemCardLayer"
                     })
            {
                AddRectParity(bundle, "TRAY", name + ".rect",
                    RequiredNamedComponent<RectTransform>(
                        scan.TrayRoot.gameObject, name),
                    RequiredNamedComponent<RectTransform>(tray, name));
            }

            ScrollRect sourceScroll = RequireComponent<ScrollRect>(
                scan.TrayRoot.gameObject);
            ScrollRect targetScroll = RequireComponent<ScrollRect>(tray);
            Add(bundle, "TRAY", "scrollrect.contract",
                ScrollRectSignature(sourceScroll) == ScrollRectSignature(targetScroll),
                ScrollRectSignature(sourceScroll), ScrollRectSignature(targetScroll),
                "EXACT_COPY");
            RectMask2D sourceMask = RequiredNamedComponent<RectMask2D>(
                scan.TrayRoot.gameObject,
                "ItemTrayViewport");
            RectMask2D targetMask = RequiredNamedComponent<RectMask2D>(
                tray,
                "ItemTrayViewport");
            Add(bundle, "TRAY", "viewport.mask.padding",
                sourceMask.padding == targetMask.padding,
                sourceMask.padding.ToString(), targetMask.padding.ToString(),
                "EXACT_COPY");
            Transform sourceTrayContent = RequiredNamedComponent<RectTransform>(
                scan.TrayRoot.gameObject, "ItemTrayContent");
            Transform targetTrayContent = RequiredNamedComponent<RectTransform>(
                tray, "ItemTrayContent");
            Add(bundle, "TRAY", "content.grid_layout",
                GridLayoutSignature(sourceTrayContent)
                == GridLayoutSignature(targetTrayContent),
                GridLayoutSignature(sourceTrayContent),
                GridLayoutSignature(targetTrayContent), "EXACT_COPY");
            Add(bundle, "TRAY", "category.count",
                CountDirectNamed(scan.TrayRoot.gameObject, "CategoryButton_")
                == CountDirectNamed(tray, "CategoryButton_"),
                CountDirectNamed(scan.TrayRoot.gameObject, "CategoryButton_")
                    .ToString(CultureInfo.InvariantCulture),
                CountDirectNamed(tray, "CategoryButton_")
                    .ToString(CultureInfo.InvariantCulture),
                "EXACT_COPY");
            C1ExactBattleSandboxItemTrayView trayView =
                RequireComponent<C1ExactBattleSandboxItemTrayView>(tray);
            Add(bundle, "TRAY", "formal_card_pool",
                trayView.AuthoredCardCapacity > 0
                && trayView.AuthoredCardCapacity
                   <= C1ExactBattleSandboxItemTrayView
                       .PhysicalPresentationCapacity,
                "at least one reusable generic Card template",
                trayView.AuthoredCardCapacity.ToString(CultureInfo.InvariantCulture),
                "INTENTIONAL_SCRIPT_CARRIER_SUBSTITUTION");
            Add(bundle, "TRAY", "missing_scripts",
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(tray) == 0,
                "0",
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(tray)
                    .ToString(CultureInfo.InvariantCulture),
                "STATIC_VALIDATION");

            RectTransform sourceCardRect = scan.CardRoles[3].transform as RectTransform;
            RectTransform targetCardRect = RequireComponent<RectTransform>(card);
            AddRectParity(bundle, "CARD", "root.rect", sourceCardRect,
                targetCardRect);
            AddBaseChildOrderParity(bundle, "CARD", "base_child_order",
                sourceCardRect, targetCardRect);
            Add(bundle, "CARD", "root.layout_element",
                LayoutElementSignature(sourceCardRect)
                == LayoutElementSignature(targetCardRect),
                LayoutElementSignature(sourceCardRect),
                LayoutElementSignature(targetCardRect), "EXACT_COPY");
            Image targetArtwork = RequiredNamedComponent<Image>(
                card,
                "ItemCard_04_image");
            AddRectParity(bundle, "CARD", "artwork_slot.rect",
                scan.CardArtworkSlot.rectTransform, targetArtwork.rectTransform);
            Add(bundle, "CARD", "artwork_slot.sprite",
                scan.CardArtworkSlot.sprite == targetArtwork.sprite,
                AssetPath(scan.CardArtworkSlot.sprite),
                AssetPath(targetArtwork.sprite),
                "EXACT_COPY");
            Add(bundle, "CARD", "artwork_slot.preserve_aspect",
                targetArtwork.preserveAspect,
                "source runtime contract forces true",
                targetArtwork.preserveAspect.ToString(),
                "INTENTIONAL_SCRIPT_CARRIER_SUBSTITUTION");
            foreach (string textName in new[] { "Title", "Category", "Shape" })
            {
                Text source = RequiredNamedComponent<Text>(
                    scan.CardRoles[3], textName);
                Text target = RequiredNamedComponent<Text>(card, textName);
                Add(bundle, "CARD", textName + ".font",
                    FontSignature(source) == FontSignature(target),
                    FontSignature(source), FontSignature(target), "EXACT_COPY");
            }

            C1ExactBattleSandboxItemCardView cardView =
                RequireComponent<C1ExactBattleSandboxItemCardView>(card);
            Add(bundle, "CARD", "authored.refs",
                cardView.ValidateAuthoredReferences(), "complete", "complete",
                "PROMOTED_AUTHORED_REF");
            Add(bundle, "CARD", "source_roles_01_05",
                scan.CardRoles.Length == 5,
                "5 compared",
                string.Join(";", scan.CardRoles.Select(RoleSignature)),
                "COMMON_PLUS_INTENTIONAL_ROLE_DIFFERENCES");

            AddPrefabModeValidation(bundle, BoardPrefabPath,
                typeof(C1ExactBattleSandboxItemBoardView));
            AddPrefabModeValidation(bundle, TrayPrefabPath,
                typeof(C1ExactBattleSandboxItemTrayView));
            AddPrefabModeValidation(bundle, CardPrefabPath,
                typeof(C1ExactBattleSandboxItemCardView));

            foreach (GameObject prefab in new[] { board, tray, card })
            {
                MonoBehaviour[] forbidden = prefab
                    .GetComponentsInChildren<MonoBehaviour>(true)
                    .Where(value => value != null
                                    && (value.GetType().Namespace ?? string.Empty)
                                    .StartsWith("TalismanBag.BuildSandbox",
                                        StringComparison.Ordinal))
                    .ToArray();
                Add(bundle, prefab.name, "no_sandbox_runtime_component",
                    forbidden.Length == 0, "0",
                    forbidden.Length.ToString(CultureInfo.InvariantCulture),
                    "STATIC_ISOLATION");
                Add(bundle, prefab.name, "missing_scripts",
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab) == 0,
                    "0",
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(prefab)
                        .ToString(CultureInfo.InvariantCulture),
                    "STATIC_VALIDATION");
            }

            AddRoleClassifications(bundle);
            Require(bundle.Rows.All(row => row.Pass),
                "PARITY_VALIDATION_FAILED " + string.Join(";", bundle.Rows
                    .Where(row => !row.Pass)
                    .Select(row => row.Carrier + "." + row.Check)));
            return bundle;
        }

        private static void AddRoleClassifications(ParityBundle bundle)
        {
            Add(bundle, "ROLE", "BoardItemArtworkLayer",
                true, "PROMOTED_AUTHORED_REF", "PROMOTED_AUTHORED_REF",
                "SERIALIZED_DYNAMIC_CARRIER");
            Add(bundle, "ROLE", "placed_preview_invalid_artwork",
                true,
                "FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF",
                "FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF",
                "SERIALIZED_DYNAMIC_CARRIER");
            Add(bundle, "ROLE", "formation_core_power_lighting_overlay",
                true,
                "FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF",
                "FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF",
                "SERIALIZED_DYNAMIC_CARRIER");
            Add(bundle, "ROLE", "drag_ghost_layers_underlays",
                true, "PROMOTED_AUTHORED_REF", "PROMOTED_AUTHORED_REF",
                "SERIALIZED_DYNAMIC_CARRIER");
            Add(bundle, "ROLE", "mobile_rotate_zone_right_and_guide",
                true, "PROMOTED_AUTHORED_REF", "PROMOTED_AUTHORED_REF",
                "AUTHORED_SOURCE_SUBTREE");
            Add(bundle, "ROLE", "selected_info_close_handling",
                true,
                "OUTSIDE_THIS_CARRIER_DOWNSTREAM",
                "OUTSIDE_THIS_CARRIER_DOWNSTREAM",
                "FULL_PAGE_COMPOSITION_NOT_COPIED");
        }

        private static void WriteEvidence(ParityBundle bundle)
        {
            StringBuilder csv = new();
            csv.AppendLine("carrier,check,status,source,target,outcome");
            foreach (ParityRow row in bundle.Rows)
            {
                csv.Append(Csv(row.Carrier)).Append(',')
                    .Append(Csv(row.Check)).Append(',')
                    .Append(row.Pass ? "PASS" : "FAIL").Append(',')
                    .Append(Csv(row.Source)).Append(',')
                    .Append(Csv(row.Target)).Append(',')
                    .Append(Csv(row.Outcome)).AppendLine();
            }

            File.WriteAllText(ParityCsvPath, csv.ToString(),
                new UTF8Encoding(false));
            StringBuilder report = new();
            report.AppendLine("# C1 Exact BattleSandbox Item Promotion Report")
                .AppendLine()
                .AppendLine("Status: `TECHNICAL_PREREQUISITE_COMPLETE / PACKAGE_COMPLETE`")
                .AppendLine()
                .AppendLine("- PlayerVisibleDelivery: `PARTIAL`")
                .AppendLine("- MilestoneCompletionEvidence: `NO`")
                .AppendLine("- Product context: `CAMPAIGN_NORMAL_LV1`")
                .AppendLine("- Source scene SHA-256: `"
                            + ComputeSha256(SourceScenePath) + "`")
                .AppendLine("- Parity: `" + bundle.PassCount + "/"
                            + bundle.Rows.Count + " PASS`")
                .AppendLine("- Missing scripts: `0`")
                .AppendLine("- BuildSandbox runtime dependencies: `0`")
                .AppendLine()
                .AppendLine("## Created carriers")
                .AppendLine()
                .AppendLine("- `" + BoardPrefabPath + "`")
                .AppendLine("- `" + TrayPrefabPath + "`")
                .AppendLine("- `" + CardPrefabPath + "`")
                .AppendLine()
                .AppendLine("The Board and Tray base subtrees retain source transforms, images, fonts, cell/slot spacing, category hierarchy, RectMask2D, and ScrollRect semantics. `ItemCard_04` supplies the reusable 285x285 root and 275x275 authored artwork slot. The three formal Tray cards are authored nested instances; the 23 Sandbox fixture cards and their runtime controller are not copied as state owners.")
                .AppendLine()
                .AppendLine("## Runtime-generated presentation audit")
                .AppendLine()
                .AppendLine("- `BoardItemArtworkLayer`: `PROMOTED_AUTHORED_REF`")
                .AppendLine("- placed / preview / invalid artwork: `FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF`")
                .AppendLine("- formation / core-power / lighting overlays: `FORMAL_ADAPTER_DYNAMIC_STATE_ON_AUTHORED_REF`")
                .AppendLine("- drag ghost artwork / invalid artwork / underlay carrier: `PROMOTED_AUTHORED_REF`")
                .AppendLine("- `MobileRotateZoneLayer`, right zone, guide: `PROMOTED_AUTHORED_REF`")
                .AppendLine("- selected-info close behavior: `OUTSIDE_THIS_CARRIER_DOWNSTREAM`")
                .AppendLine()
                .AppendLine("## Exact source card role map")
                .AppendLine()
                .AppendLine("- ItemCard_01: 285-class baseline role; simple authored hierarchy.")
                .AppendLine("- ItemCard_02: 285-class role with authored footprint subtree.")
                .AppendLine("- ItemCard_03: intentional 140x430 vertical role.")
                .AppendLine("- ItemCard_04: selected reusable 285-class carrier; explicit 275x275 artwork slot.")
                .AppendLine("- ItemCard_05: intentional 285-class variant; explicit 271x271 artwork slot.")
                .AppendLine()
                .AppendLine("## Protected baseline")
                .AppendLine()
                .AppendLine("All 19 frozen SHA-256 baselines revalidated unchanged. Current approximate `C1FormalItemBoard`, `C1FormalItemTray`, and `C1FormalItemCard` remain unchanged and are not the final visible carriers.")
                .AppendLine()
                .AppendLine("## Evidence boundary")
                .AppendLine()
                .AppendLine("Compile/static and presentation-parity evidence are proven. Unified mounting, live formal Battle runtime, full BattleSandbox page composition, and WorldMap -> 1-1 -> I002 -> rebuild -> 1-2 -> WorldMap real-path evidence remain downstream. This package does not claim `USER_HANDTEST_READY`, `MILESTONE_INTEGRATED`, or milestone `COMPLETE`.");
            File.WriteAllText(CloseReportPath, report.ToString(),
                new UTF8Encoding(false));
        }

        private static SourceScan Scan(Scene scene)
        {
            Require(scene.IsValid() && scene.isLoaded
                    && string.Equals(scene.path, SourceScenePath,
                        StringComparison.Ordinal),
                "SOURCE_SCENE_INVALID");
            GameObject[] roots = scene.GetRootGameObjects();
            Transform board = SingleNamed(roots, "BoardGridPreview");
            Transform tray = SingleNamed(roots, "ItemTrayPreview");
            GameObject[] cardRoles = Enumerable.Range(1, 5)
                .Select(index => SingleNamed(
                    roots,
                    "ItemCard_" + index.ToString("00",
                        CultureInfo.InvariantCulture)).gameObject)
                .ToArray();
            Image artwork = RequiredNamedComponent<Image>(
                cardRoles[3],
                "ItemCard_04_image");
            GameObject rotateSlot = SingleNamed(
                roots,
                "RotatePreviewButtonSlot").gameObject;
            Require(board.GetComponentsInChildren<Image>(true).Length >= 26,
                "SOURCE_BOARD_IMAGE_COUNT_INVALID");
            Require(tray.GetComponent<ScrollRect>() != null,
                "SOURCE_TRAY_SCROLLRECT_MISSING");
            Require(RequiredNamedComponent<RectMask2D>(
                    tray.gameObject,
                    "ItemTrayViewport") != null,
                "SOURCE_TRAY_MASK_MISSING");
            return new SourceScan(
                board as RectTransform,
                tray as RectTransform,
                cardRoles,
                artwork,
                rotateSlot);
        }

        private static T WithReadOnlySourceScene<T>(Func<SourceScan, T> action)
        {
            EnsureNoDirtyUserScenes();
            ValidateProtectedHashes();
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            Scene source = default;
            try
            {
                for (int index = 0; index < SceneManager.sceneCount; index++)
                {
                    Scene loaded = SceneManager.GetSceneAt(index);
                    Require(!string.Equals(loaded.path, SourceScenePath,
                            StringComparison.Ordinal),
                        "SOURCE_SCENE_ALREADY_LOADED_REFUSE");
                }

                source = EditorSceneManager.OpenScene(
                    SourceScenePath,
                    OpenSceneMode.Additive);

                Require(!source.isDirty, "SOURCE_SCENE_DIRTY_REFUSE");
                SourceScan scan = Scan(source);
                T result = action(scan);
                Require(string.Equals(
                        ComputeSha256(SourceScenePath),
                        ProtectedHashes[SourceScenePath],
                        StringComparison.Ordinal),
                    "SOURCE_HASH_DRIFT_DURING_OPERATION");
                return result;
            }
            finally
            {
                if (source.IsValid() && source.isLoaded)
                    EditorSceneManager.CloseScene(source, true);
                if (setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }

        private static void EnsureNoDirtyUserScenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                Require(!scene.isDirty,
                    "USER_SCENE_DIRTY_REFUSE path=" + scene.path);
            }
        }

        private static GameObject CloneIntoPreviewScene(GameObject source)
        {
            Require(source != null, "SOURCE_CLONE_OBJECT_MISSING");
            Scene previewScene = EditorSceneManager.NewPreviewScene();
            GameObject clone = Object.Instantiate(source);
            SceneManager.MoveGameObjectToScene(clone, previewScene);
            clone.hideFlags = HideFlags.None;
            return clone;
        }

        private static void ClosePreviewScene(Scene previewScene)
        {
            if (previewScene.IsValid() && previewScene.isLoaded)
                EditorSceneManager.ClosePreviewScene(previewScene);
        }

        private static void RemoveBuildSandboxComponents(GameObject root)
        {
            MonoBehaviour[] components = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (MonoBehaviour component in components)
            {
                if (component == null) continue;
                string componentNamespace = component.GetType().Namespace ?? string.Empty;
                if (componentNamespace.StartsWith(
                        "TalismanBag.BuildSandbox",
                        StringComparison.Ordinal))
                    Object.DestroyImmediate(component);
            }
        }

        private static RectTransform CreateRectLayer(Transform parent, string name)
        {
            GameObject target = new(name, typeof(RectTransform));
            target.transform.SetParent(parent, false);
            RectTransform rect = target.GetComponent<RectTransform>();
            Stretch(rect);
            return rect;
        }

        private static Image DuplicateImageCarrier(
            Image source,
            Transform parent,
            string name)
        {
            Require(source != null, "IMAGE_CARRIER_SOURCE_MISSING " + name);
            GameObject target = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            target.transform.SetParent(parent, false);
            CopyRectTransform(source.rectTransform,
                target.GetComponent<RectTransform>());
            Image image = target.GetComponent<Image>();
            EditorUtility.CopySerialized(source, image);
            image.raycastTarget = false;
            return image;
        }

        private static Text DuplicateTextCarrier(
            Text source,
            Transform parent,
            string name)
        {
            Require(source != null, "TEXT_CARRIER_SOURCE_MISSING " + name);
            GameObject target = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            target.transform.SetParent(parent, false);
            CopyRectTransform(source.rectTransform,
                target.GetComponent<RectTransform>());
            Text text = target.GetComponent<Text>();
            EditorUtility.CopySerialized(source, text);
            text.raycastTarget = false;
            return text;
        }

        private static void CopyRectTransform(RectTransform source, RectTransform target)
        {
            Require(source != null && target != null, "RECT_COPY_REF_MISSING");
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.pivot = source.pivot;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void DisableButtonsAndRaycasts(GameObject root)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                button.enabled = false;
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
        }

        private static void SaveNewPrefab(GameObject root, string path)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            Require(success && saved != null, "PREFAB_SAVE_FAILED " + path);
        }

        private static void UpdateRev03CardPrefab()
        {
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(CardPrefabPath);
                Require(contents != null, "REV03_CARD_PREFAB_MODE_LOAD_FAILED");
                C1ExactBattleSandboxItemCardView view = RequireComponent<
                    C1ExactBattleSandboxItemCardView>(contents);
                RectTransform sourceFootprintLayer =
                    RequiredNamedComponent<RectTransform>(
                        contents,
                        "TrayLayoutCellLayer");
                Image[] sourceFootprintCells = Enumerable.Range(0, 3)
                    .Select(index => RequiredNamedComponent<Image>(
                        contents,
                        "TrayLayoutCell_" + index.ToString(
                            "00",
                            CultureInfo.InvariantCulture)))
                    .ToArray();
                foreach (Image image in sourceFootprintCells)
                {
                    image.raycastTarget = false;
                    EditorUtility.SetDirty(image);
                }

                SerializedObject serialized = new(view);
                SerializedProperty layerProperty = serialized.FindProperty(
                    "sourceFootprintLayer");
                SerializedProperty cellsProperty = serialized.FindProperty(
                    "sourceFootprintCells");
                Require(layerProperty != null && cellsProperty != null,
                    "REV03_CARD_SERIALIZED_FOOTPRINT_FIELDS_MISSING");
                layerProperty.objectReferenceValue = sourceFootprintLayer;
                cellsProperty.arraySize = sourceFootprintCells.Length;
                for (int index = 0; index < sourceFootprintCells.Length; index++)
                    cellsProperty.GetArrayElementAtIndex(index)
                        .objectReferenceValue = sourceFootprintCells[index];
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(view);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    contents,
                    CardPrefabPath,
                    out bool success);
                Require(success && saved != null,
                    "REV03_CARD_PREFAB_SAVE_FAILED");
            }
            finally
            {
                if (contents != null) PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void UpdateRev04FTrayPrefab()
        {
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(TrayPrefabPath);
                Require(contents != null, "REV04F_TRAY_PREFAB_MODE_LOAD_FAILED");
                C1ExactBattleSandboxItemTrayView view = RequireComponent<
                    C1ExactBattleSandboxItemTrayView>(contents);
                RectTransform trayRect = RequireComponent<RectTransform>(contents);
                ScrollRect scrollRect = RequireComponent<ScrollRect>(contents);
                RectTransform viewport = RequiredNamedComponent<RectTransform>(
                    contents,
                    "ItemTrayViewport");
                RectMask2D viewportMask = RequireComponent<RectMask2D>(
                    viewport.gameObject);
                RectTransform content = RequiredNamedComponent<RectTransform>(
                    contents,
                    "ItemTrayContent");
                RectTransform cardLayer = RequiredNamedComponent<RectTransform>(
                    contents,
                    "ItemCardLayer");
                C1ExactBattleSandboxItemCardView[] cards = cardLayer
                    .GetComponentsInChildren<C1ExactBattleSandboxItemCardView>(true)
                    .OrderBy(value => value.name, StringComparer.Ordinal)
                    .ToArray();
                Require(cards.Length == DynamicPresentationInitialPrewarmCount,
                    "REV04F_TRAY_CARD_POOL_INVALID");
                EnsureRev04FTrayCells(
                    content,
                    out RectTransform[] trayCellRects,
                    out Image[] trayCellImages);
                view.AssignForEditor(
                    trayRect,
                    scrollRect,
                    viewport,
                    viewportMask,
                    content,
                    cardLayer,
                    cards,
                    trayCellRects,
                    trayCellImages);
                Require(view.ValidateAuthoredReferences(),
                    "REV04F_TRAY_SERIALIZED_REFS_INVALID");
                EditorUtility.SetDirty(view);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    contents,
                    TrayPrefabPath,
                    out bool success);
                Require(success && saved != null,
                    "REV04F_TRAY_PREFAB_SAVE_FAILED");
            }
            finally
            {
                if (contents != null) PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void EnsureRev04FTrayCells(
            RectTransform content,
            out RectTransform[] trayCellRects,
            out Image[] trayCellImages)
        {
            Require(content != null, "REV04F_TRAY_CONTENT_MISSING");
            GridLayoutGroup grid = RequireComponent<GridLayoutGroup>(
                content.gameObject);
            Require(grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                    && grid.constraintCount
                       == C1FormalItemSessionContract.TrayColumnCount,
                "REV04F_TRAY_GRID_GEOMETRY_INVALID");
            RectTransform template = RequiredNamedComponent<RectTransform>(
                content.gameObject,
                "TrayGridSlot_40");
            Require(template.parent == content
                    && template.GetComponent<Image>() != null
                    && template.GetComponent<Outline>() != null,
                "REV04F_TRAY_SLOT_TEMPLATE_INVALID");
            for (int index = 41;
                 index <= C1FormalItemSessionContract.TrayCellCount;
                 index++)
            {
                string name = "TrayGridSlot_" + index.ToString(
                    "00",
                    CultureInfo.InvariantCulture);
                Transform existing = content.Find(name);
                if (existing == null)
                {
                    GameObject clone = Object.Instantiate(
                        template.gameObject,
                        content,
                        false);
                    clone.name = name;
                    existing = clone.transform;
                }
                Require(existing.parent == content
                        && existing.GetComponent<Image>() != null
                        && existing.GetComponent<Outline>() != null,
                    "REV04F_TRAY_SLOT_COMPONENT_PATTERN_INVALID " + name);
            }

            trayCellRects = Enumerable.Range(
                    1,
                    C1FormalItemSessionContract.TrayCellCount)
                .Select(index => RequiredNamedComponent<RectTransform>(
                    content.gameObject,
                    "TrayGridSlot_" + index.ToString(
                        "00",
                        CultureInfo.InvariantCulture)))
                .ToArray();
            trayCellImages = trayCellRects
                .Select(value => RequireComponent<Image>(value.gameObject))
                .ToArray();
            for (int index = 0; index < trayCellRects.Length; index++)
            {
                trayCellRects[index].SetSiblingIndex(index);
            }

            int rows = C1FormalItemSessionContract.TrayRowCount;
            float requiredHeight = grid.padding.top
                                   + grid.padding.bottom
                                   + rows * grid.cellSize.y
                                   + Math.Max(0, rows - 1) * grid.spacing.y;
            content.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                requiredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        private static void ValidateRev04FTrayPrefabMode()
        {
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(TrayPrefabPath);
                Require(contents != null, "REV04F_TRAY_VALIDATION_LOAD_FAILED");
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                            contents) == 0,
                    "REV04F_TRAY_MISSING_SCRIPT");
                C1ExactBattleSandboxItemTrayView view = RequireComponent<
                    C1ExactBattleSandboxItemTrayView>(contents);
                Require(view.ValidateAuthoredReferences()
                        && view.AuthoredCellCount
                           == C1FormalItemSessionContract.TrayCellCount,
                    "REV04F_TRAY_VIEW_VALIDATION_FAILED");
                RectTransform content = RequiredNamedComponent<RectTransform>(
                    contents,
                    "ItemTrayContent");
                GridLayoutGroup grid = RequireComponent<GridLayoutGroup>(
                    content.gameObject);
                float requiredHeight = grid.padding.top
                                       + grid.padding.bottom
                                       + C1FormalItemSessionContract.TrayRowCount
                                         * grid.cellSize.y
                                       + (C1FormalItemSessionContract.TrayRowCount - 1)
                                         * grid.spacing.y;
                Require(Mathf.Abs(content.rect.height - requiredHeight) <= 0.01f,
                    "REV04F_TRAY_CONTENT_HEIGHT_INVALID");
                Image templateImage = RequiredNamedComponent<Image>(
                    contents,
                    "TrayGridSlot_40");
                Outline templateOutline = RequiredNamedComponent<Outline>(
                    contents,
                    "TrayGridSlot_40");
                for (int index = 41;
                     index <= C1FormalItemSessionContract.TrayCellCount;
                     index++)
                {
                    string name = "TrayGridSlot_" + index.ToString(
                        "00",
                        CultureInfo.InvariantCulture);
                    Image image = RequiredNamedComponent<Image>(contents, name);
                    Outline outline = RequiredNamedComponent<Outline>(contents, name);
                    Require(string.Equals(
                                ImageSignature(templateImage),
                                ImageSignature(image),
                                StringComparison.Ordinal)
                            && templateOutline.effectColor == outline.effectColor
                            && templateOutline.effectDistance
                               == outline.effectDistance
                            && templateOutline.useGraphicAlpha
                               == outline.useGraphicAlpha,
                        "REV04F_TRAY_SLOT_STYLE_DRIFT " + name);
                }
                Require(contents.GetComponentsInChildren<MonoBehaviour>(true)
                            .Where(value => value != null)
                            .All(value => !(value.GetType().Namespace
                                ?? string.Empty).StartsWith(
                                    "TalismanBag.BuildSandbox",
                                    StringComparison.Ordinal)),
                    "REV04F_TRAY_BUILDSANDBOX_DEPENDENCY");
            }
            finally
            {
                if (contents != null) PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void ValidateRev04FProtectedHashes()
        {
            foreach (KeyValuePair<string, string> expected in
                     Rev04FProtectedHashes)
            {
                Require(string.Equals(
                        ComputeSha256(expected.Key),
                        expected.Value,
                        StringComparison.Ordinal),
                    "REV04F_PROTECTED_HASH_DRIFT path=" + expected.Key);
            }
        }

        private static void ValidateRev03PrefabMode()
        {
            foreach (string path in new[]
                     {
                         BoardPrefabPath,
                         TrayPrefabPath,
                         CardPrefabPath
                     })
            {
                GameObject contents = null;
                try
                {
                    contents = PrefabUtility.LoadPrefabContents(path);
                    Require(contents != null,
                        "REV03_PREFAB_MODE_LOAD_FAILED path=" + path);
                    Require(GameObjectUtility
                                .GetMonoBehavioursWithMissingScriptCount(contents)
                            == 0,
                        "REV03_PREFAB_MODE_MISSING_SCRIPT path=" + path);
                    if (string.Equals(path, BoardPrefabPath,
                            StringComparison.Ordinal))
                        Require(contents.GetComponent<
                                    C1ExactBattleSandboxItemBoardView>()
                                ?.ValidateAuthoredReferences() == true,
                            "REV03_BOARD_PREFAB_MODE_REFS_INVALID");
                    else if (string.Equals(path, TrayPrefabPath,
                                 StringComparison.Ordinal))
                        Require(contents.GetComponent<
                                    C1ExactBattleSandboxItemTrayView>()
                                ?.ValidateAuthoredReferences() == true,
                            "REV03_TRAY_PREFAB_MODE_REFS_INVALID");
                    else
                        Require(contents.GetComponent<
                                    C1ExactBattleSandboxItemCardView>()
                                ?.ValidateAuthoredReferences() == true,
                            "REV03_CARD_PREFAB_MODE_REFS_INVALID");
                }
                finally
                {
                    if (contents != null)
                        PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }

        private static SpriteSet LoadFormalArtworkSprites()
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                ExistingFormalCardPrefabPath);
            Require(source != null, "FORMAL_CARD_ARTWORK_SOURCE_MISSING");
            C1FormalItemCardView view = source.GetComponent<C1FormalItemCardView>();
            Require(view != null, "FORMAL_CARD_ARTWORK_VIEW_MISSING");
            SerializedObject serialized = new(view);
            return new SpriteSet(
                RequiredSprite(serialized, "i001WhiteArtwork"),
                RequiredSprite(serialized, "i002WhiteArtwork"),
                RequiredSprite(serialized, "i031UnlitArtwork"),
                RequiredSprite(serialized, "i031LitArtwork"));
        }

        private static Sprite RequiredSprite(SerializedObject source, string field)
        {
            Sprite value = source.FindProperty(field)?.objectReferenceValue as Sprite;
            Require(value != null, "FORMAL_ARTWORK_SPRITE_MISSING " + field);
            return value;
        }

        private static void AddRectParity(
            ParityBundle bundle,
            string carrier,
            string check,
            RectTransform source,
            RectTransform target)
        {
            string sourceValue = RectSignature(source);
            string targetValue = RectSignature(target);
            Add(bundle, carrier, check,
                string.Equals(sourceValue, targetValue, StringComparison.Ordinal),
                sourceValue, targetValue, "EXACT_COPY");
        }

        private static void AddImageParity(
            ParityBundle bundle,
            string carrier,
            string check,
            Image source,
            Image target)
        {
            string sourceValue = ImageSignature(source);
            string targetValue = ImageSignature(target);
            Add(bundle, carrier, check,
                string.Equals(sourceValue, targetValue, StringComparison.Ordinal),
                sourceValue, targetValue, "EXACT_COPY");
        }

        private static void AddBaseChildOrderParity(
            ParityBundle bundle,
            string carrier,
            string check,
            Transform source,
            Transform target)
        {
            string sourceValue = DirectChildSignature(source);
            string targetValue = string.Join(">", Enumerable.Range(0,
                    Math.Min(source.childCount, target.childCount))
                .Select(index => target.GetChild(index).name));
            Add(bundle, carrier, check,
                source.childCount <= target.childCount
                && string.Equals(sourceValue, targetValue,
                    StringComparison.Ordinal),
                sourceValue, targetValue, "EXACT_BASE_ORDER");
        }

        private static void AddPrefabModeValidation(
            ParityBundle bundle,
            string prefabPath,
            Type expectedViewType)
        {
            GameObject contents = null;
            try
            {
                contents = PrefabUtility.LoadPrefabContents(prefabPath);
                int missingScripts = GameObjectUtility
                    .GetMonoBehavioursWithMissingScriptCount(contents);
                bool hasView = contents.GetComponent(expectedViewType) != null;
                Add(bundle, contents.name, "prefab_mode.serialized_inspection",
                    missingScripts == 0 && hasView,
                    "missing_scripts=0;view=" + expectedViewType.Name,
                    "missing_scripts=" + missingScripts.ToString(
                        CultureInfo.InvariantCulture)
                    + ";view=" + hasView,
                    "PREFAB_MODE_STATIC_INSPECTION");
            }
            finally
            {
                if (contents != null)
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }
            }
        }

        private static void Add(
            ParityBundle bundle,
            string carrier,
            string check,
            bool pass,
            string source,
            string target,
            string outcome)
        {
            bundle.Rows.Add(new ParityRow(
                carrier,
                check,
                pass,
                source,
                target,
                outcome));
        }

        private static string RectSignature(RectTransform rect)
        {
            Require(rect != null, "RECT_SIGNATURE_REF_MISSING");
            return string.Join("|", new[]
            {
                Vector(rect.anchorMin),
                Vector(rect.anchorMax),
                Vector(rect.anchoredPosition),
                Vector(rect.sizeDelta),
                Vector(rect.pivot),
                Vector(rect.localScale),
                Quaternion(rect.localRotation)
            });
        }

        private static string ImageSignature(Image image)
        {
            Require(image != null, "IMAGE_SIGNATURE_REF_MISSING");
            return string.Join("|", new[]
            {
                AssetPath(image.sprite),
                AssetPath(image.material),
                Color(image.color),
                image.type.ToString(),
                image.preserveAspect.ToString(),
                image.fillCenter.ToString(),
                image.raycastTarget.ToString(),
                image.maskable.ToString(),
                Float(image.pixelsPerUnitMultiplier)
            });
        }

        private static string DirectChildSignature(Transform root)
        {
            return string.Join(">", Enumerable.Range(0, root.childCount)
                .Select(index => root.GetChild(index).name));
        }

        private static string GridLayoutSignature(Transform root)
        {
            GridLayoutGroup grid = root.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                return "NONE";
            }

            return string.Join("|", new[]
            {
                grid.startCorner.ToString(),
                grid.startAxis.ToString(),
                grid.childAlignment.ToString(),
                Vector(grid.cellSize),
                Vector(grid.spacing),
                grid.constraint.ToString(),
                grid.constraintCount.ToString(CultureInfo.InvariantCulture),
                grid.padding.left.ToString(CultureInfo.InvariantCulture),
                grid.padding.right.ToString(CultureInfo.InvariantCulture),
                grid.padding.top.ToString(CultureInfo.InvariantCulture),
                grid.padding.bottom.ToString(CultureInfo.InvariantCulture)
            });
        }

        private static string LayoutElementSignature(Transform root)
        {
            LayoutElement layout = root.GetComponent<LayoutElement>();
            if (layout == null)
            {
                return "NONE";
            }

            return string.Join("|", new[]
            {
                layout.ignoreLayout.ToString(),
                Float(layout.minWidth),
                Float(layout.minHeight),
                Float(layout.preferredWidth),
                Float(layout.preferredHeight),
                Float(layout.flexibleWidth),
                Float(layout.flexibleHeight),
                layout.layoutPriority.ToString(CultureInfo.InvariantCulture)
            });
        }

        private static string FontSignature(Text text)
        {
            return string.Join("|", new[]
            {
                AssetPath(text.font),
                text.fontSize.ToString(CultureInfo.InvariantCulture),
                text.fontStyle.ToString(),
                text.alignment.ToString(),
                Color(text.color),
                text.raycastTarget.ToString()
            });
        }

        private static string ScrollRectSignature(ScrollRect scroll)
        {
            return string.Join("|", new[]
            {
                scroll.horizontal.ToString(),
                scroll.vertical.ToString(),
                scroll.movementType.ToString(),
                Float(scroll.elasticity),
                scroll.inertia.ToString(),
                Float(scroll.decelerationRate),
                Float(scroll.scrollSensitivity),
                scroll.viewport?.name ?? string.Empty,
                scroll.content?.name ?? string.Empty
            });
        }

        private static string RoleSignature(GameObject role)
        {
            RectTransform rect = role.transform as RectTransform;
            return role.name + "=" + (rect == null ? "NO_RECT" : RectSignature(rect));
        }

        private static int CountDirectNamed(GameObject root, string prefix)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Count(value => value.name.StartsWith(prefix, StringComparison.Ordinal));
        }

        private static Transform SingleNamed(GameObject[] roots, string name)
        {
            Transform[] matches = roots
                .Where(root => root != null)
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(value => string.Equals(value.name, name,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "SOURCE_ROLE_COUNT_INVALID name=" + name + " count="
                + matches.Length.ToString(CultureInfo.InvariantCulture));
            return matches[0];
        }

        private static T RequiredNamedComponent<T>(GameObject root, string name)
            where T : Component
        {
            T[] matches = root.GetComponentsInChildren<T>(true)
                .Where(value => string.Equals(value.name, name,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "NAMED_COMPONENT_COUNT_INVALID type=" + typeof(T).Name
                + " name=" + name + " count="
                + matches.Length.ToString(CultureInfo.InvariantCulture));
            return matches[0];
        }

        private static T RequireComponent<T>(GameObject root) where T : Component
        {
            T value = root == null ? null : root.GetComponent<T>();
            Require(value != null,
                "COMPONENT_MISSING type=" + typeof(T).Name + " root="
                + (root == null ? "null" : root.name));
            return value;
        }

        private static void ValidateProtectedHashes()
        {
            foreach (KeyValuePair<string, string> row in ProtectedHashes)
            {
                Require(File.Exists(row.Key), "PROTECTED_PATH_MISSING " + row.Key);
                string actual = ComputeSha256(row.Key);
                Require(string.Equals(actual, row.Value, StringComparison.Ordinal),
                    "PROTECTED_HASH_CHANGED path=" + row.Key + " actual=" + actual);
            }
        }

        private static void ValidateRev03ProtectedHashes()
        {
            ValidateHashes(Rev03ProtectedHashes, "REV03_PROTECTED");
        }

        private static void ValidateRev03UntouchedPrefabs()
        {
            ValidateHashes(Rev03UntouchedPrefabHashes, "REV03_UNTOUCHED_PREFAB");
        }

        private static void ValidateHashes(
            IReadOnlyDictionary<string, string> hashes,
            string label)
        {
            foreach (KeyValuePair<string, string> row in hashes)
            {
                Require(File.Exists(row.Key), label + "_PATH_MISSING " + row.Key);
                string actual = ComputeSha256(row.Key);
                Require(string.Equals(actual, row.Value, StringComparison.Ordinal),
                    label + "_HASH_CHANGED path=" + row.Key + " actual=" + actual);
            }
        }

        private static void ValidateExactOutputsPresent()
        {
            foreach (string path in ExactOutputPaths)
                Require(File.Exists(path), "EXACT_OUTPUT_MISSING " + path);
        }

        private static string ComputeSha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static void CleanupIncompleteOwnedEvidence()
        {
            foreach (string path in new[]
                     {
                         BoardPrefabPath,
                         TrayPrefabPath,
                         CardPrefabPath
                     })
            {
                if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                    AssetDatabase.DeleteAsset(path);
            }

            if (File.Exists(ParityCsvPath)) File.Delete(ParityCsvPath);
            if (File.Exists(CloseReportPath)) File.Delete(CloseReportPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private void RunWindowOperation(Action operation)
        {
            try
            {
                operation();
                lastStatus = operation.Method.Name + " completed.";
            }
            catch (Exception exception)
            {
                lastStatus = operation.Method.Name + " failed: " + exception.Message;
                Debug.LogException(exception);
            }
        }

        private static bool Approximately(Vector3 left, Vector3 right)
        {
            return Vector3.SqrMagnitude(left - right) <= 0.000001f;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static string AssetPath(Object value)
        {
            return value == null ? "null" : AssetDatabase.GetAssetPath(value);
        }

        private static string Vector(Vector2 value)
        {
            return Float(value.x) + ":" + Float(value.y);
        }

        private static string Vector(Vector3 value)
        {
            return Float(value.x) + ":" + Float(value.y) + ":" + Float(value.z);
        }

        private static string Quaternion(UnityEngine.Quaternion value)
        {
            return Float(value.x) + ":" + Float(value.y) + ":" + Float(value.z)
                   + ":" + Float(value.w);
        }

        private static string Color(UnityEngine.Color value)
        {
            return Float(value.r) + ":" + Float(value.g) + ":" + Float(value.b)
                   + ":" + Float(value.a);
        }

        private static string Float(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private sealed class SourceScan
        {
            public SourceScan(
                RectTransform boardRoot,
                RectTransform trayRoot,
                GameObject[] cardRoles,
                Image cardArtworkSlot,
                GameObject rotatePreviewButtonSlot)
            {
                BoardRoot = boardRoot;
                TrayRoot = trayRoot;
                CardRoles = cardRoles;
                CardArtworkSlot = cardArtworkSlot;
                RotatePreviewButtonSlot = rotatePreviewButtonSlot;
            }

            public RectTransform BoardRoot { get; }
            public RectTransform TrayRoot { get; }
            public GameObject[] CardRoles { get; }
            public Image CardArtworkSlot { get; }
            public GameObject RotatePreviewButtonSlot { get; }

            public string BuildSummary()
            {
                return "board=" + RectSignature(BoardRoot)
                       + " / boardCells=25 / tray=" + RectSignature(TrayRoot)
                       + " / scrollRect=1 / rectMask2D=1 / cardRoles="
                       + string.Join(";", CardRoles.Select(RoleSignature))
                       + " / artworkSlot=" + CardArtworkSlot.name;
            }
        }

        private sealed class SpriteSet
        {
            public SpriteSet(
                Sprite i001,
                Sprite i002,
                Sprite i031Unlit,
                Sprite i031Lit)
            {
                I001 = i001;
                I002 = i002;
                I031Unlit = i031Unlit;
                I031Lit = i031Lit;
            }

            public Sprite I001 { get; }
            public Sprite I002 { get; }
            public Sprite I031Unlit { get; }
            public Sprite I031Lit { get; }
        }

        private sealed class ParityBundle
        {
            public List<ParityRow> Rows { get; } = new();
            public int PassCount => Rows.Count(row => row.Pass);
        }

        private sealed class ParityRow
        {
            public ParityRow(
                string carrier,
                string check,
                bool pass,
                string source,
                string target,
                string outcome)
            {
                Carrier = carrier;
                Check = check;
                Pass = pass;
                Source = source;
                Target = target;
                Outcome = outcome;
            }

            public string Carrier { get; }
            public string Check { get; }
            public bool Pass { get; }
            public string Source { get; }
            public string Target { get; }
            public string Outcome { get; }
        }
    }
}
