using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.UnifiedBattle;
using TalismanBag.UnifiedBattle.Presentation.ExactNavigation;
using TalismanBag.UnifiedBattle.Presentation.ExactPrepare;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class C1ExactBattleSandboxNavigationPhaseAAuthoring
    {
        private const string AssignmentPath =
            "Docs/V0.4/V0.4-C1UnifiedExactBattleSandboxNavigationAndItemDetailPromotion01_Assignment.md";
        private const string SourceScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string UnifiedScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        private const string UnifiedShellPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";
        private const string NavigationPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxBottomNavigation.prefab";
        private const string PrepareSurfacePrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxPrepareSurface.prefab";
        private const string GameBackgroundPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/C1ExactBattleSandboxGameBackground.prefab";
        private const string ExactBoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab";
        private const string ExactTrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab";
        private const string ExactCardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab";
        private const string RequiredInertSlotShellInputSha256 =
            "0D6CB76DB22F6E6F575DF9827D82423282B6FF28D1B299BA70B239224DA1389A";
        private const string BuildGridControllerPath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs";
        private const string SourceRootName =
            "V04BattlePrepareBottomActions";

        private static readonly string[] ExactChildNames =
        {
            "BattlePrepareBottomActions",
            "V04BattlePrepareBackButton",
            "V04BattlePrepareStateButton",
            "V04BattlePrepareToggleButton",
            "V04BattlePreparejiasu",
            "V04BattlePreparezhandourizhi"
        };

        private static readonly string[] RequiredHostReferencePaths =
        {
            "shell",
            "marker",
            "v03FlowAdapterSummaryText",
            "navigationPresenter",
            "prepareSurfacePresenter",
            "gameBackgroundImage",
            "itemPresenter",
            "itemBoardView",
            "itemTrayView",
            "formalPresentationRoot"
        };

        private static readonly IReadOnlyDictionary<string, string>
            ProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [AssignmentPath] =
                    "6C52B514187D19588986E6D625918646627056EA6F01DF0CB02C739994EF4DCD",
                [SourceScenePath] =
                    "1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA",
                [BuildGridControllerPath] =
                    "364727A582EE3F2D499C109D98939318B4D26AB9D2F1F571CB70E62E605913DC",
                ["Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab"] =
                    "46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8",
                ["Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab"] =
                    "AE444A9BB96E84DB5409A444DADF13787575E377EE93956FA0B31E608F1751E1",
                ["Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab"] =
                    "C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs"] =
                    "646E0CDBAAB4C76F1A6B5165A766BE81E9D289DEE62AFC9194F53140F8C2C629",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs"] =
                    "A176C16F21545BC00B0D91D3B6F8812C565E1F4CFB9489943F5D701C27C19E65",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemTrayView.cs"] =
                    "35026ED05B541A185D9FF002DBF6EB300C76F4C2E6F27E7C62DB8A835DD5D740",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs"] =
                    "371F3BD8AEBA022B6DE01134CB6804A411146237ECE5D4B281885CADFBE0A988",
                ["Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs"] =
                    "935A699E038866F3AD5B870A8982A1100FBBDB666C76C23457638D0206160317",
                ["Assets/_Game/Scenes/Scene_TalismanBag_V04_WorldMap.unity"] =
                    "9F946361BA208E129A349BB9F382F40A18D33DC49D6EA4DC68D0E8605B108B3E",
                ["ProjectSettings/EditorBuildSettings.asset"] =
                    "CF244226FFD286638DEC957BBE5A9ABCA1DE52E9D529D71D406B5D03DE805932"
            };

        private static readonly IReadOnlyDictionary<string, string>
            PreservedCarrierHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [NavigationPrefabPath] =
                    "E8F694911F95A0EA68A164E487B7501DEFAAA09EB2F74453C7678BAABDF4CD80",
                [PrepareSurfacePrefabPath] =
                    "95207E6F0123938B9F532AE911575867795EC0A6040AB8776F4E9F487ED1FC32",
                [GameBackgroundPrefabPath] =
                    "B79F90501227A4FA5CB35BA1F3F85DED7B8C83397428FBF56F39481669CB5906",
                [UnifiedShellPrefabPath] =
                    "44A897ACECF2F871A7BCD45A3390FFF5ED72EB894FB0A815E751BC6BA4663590"
            };

        private static readonly IReadOnlyDictionary<string, string>
            RequiredInertSlotProtectedHashes = new Dictionary<string, string>(
                StringComparer.Ordinal)
            {
                [UnifiedScenePath] =
                    "D432B6E492EDF0CB0EEA8B62E66E27A6EE3F7773C8BBCC8AD886A86A1A0BDD63",
                [PrepareSurfacePrefabPath] =
                    "95207E6F0123938B9F532AE911575867795EC0A6040AB8776F4E9F487ED1FC32",
                [ExactBoardPrefabPath] =
                    "46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8",
                [ExactTrayPrefabPath] =
                    "6FA5F995433B4433B201613CA7FBA939779F6664E44E19CC02ACA060E4F4D552",
                [ExactCardPrefabPath] =
                    "C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C"
            };

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs",
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/ExactNavigation/C1ExactBattleSandboxNavigationBarView.cs",
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/ExactNavigation/C1ExactBattleSandboxNavigationPresenter.cs",
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/ExactPrepare/C1ExactBattleSandboxPrepareSurfaceView.cs",
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/ExactPrepare/C1ExactBattleSandboxPrepareSurfacePresenter.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "new GameObject(",
            "GameObject.Find",
            "FindObjectOfType",
            "FindObjectsOfType",
            "SceneManager.GetActiveScene().name",
            "BuildGridInteractionPreviewController",
            "AutoCombatController",
            "Time.timeScale",
            "PlayerPrefs",
            "OnGUI(",
            "ItemDetailPanel",
            "BuildSandbox"
        };

        [MenuItem(
            "TalismanBag/V0.4/Unified Battle/Apply Exact Navigation Phase A Once")]
        public static void ApplyFromMenu()
        {
            ApplySinglePass();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ApplySinglePass();
                Debug.Log(
                    "[ExactNavigationPhaseA] PHASE_A_AUTHORING_AND_VALIDATION_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[ExactNavigationPhaseA] PHASE_A_AUTHORING_OR_VALIDATION_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Unified Battle/Apply REV05 Narrow Mount Once")]
        public static void ApplyNarrowMountFromMenu()
        {
            ApplyNarrowMountSinglePass();
        }

        [MenuItem(
            "TalismanBag/V0.4/Unified Battle/Restore Required Inert Shell Slots Once")]
        public static void RestoreRequiredInertShellSlotsFromMenu()
        {
            RestoreRequiredInertShellSlotsSinglePass();
            Debug.Log(
                "[ExactNavigationPhaseA] "
                + "SHELL_REQUIRED_INERT_SLOTS_AUTHORING_AND_VALIDATION_PASS");
        }

        public static void ExecuteNarrowMountFromCommandLine()
        {
            try
            {
                ApplyNarrowMountSinglePass();
                Debug.Log(
                    "[ExactNavigationPhaseA] REV05_NARROW_MOUNT_AND_VALIDATION_PASS");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[ExactNavigationPhaseA] REV05_NARROW_MOUNT_OR_VALIDATION_FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ApplyNarrowMountSinglePass()
        {
            ValidateProtectedHashes();
            ValidatePreservedCarrierHashes();
            ValidateRuntimeSourceBoundaries();

            Scene sourceScene = EditorSceneManager.OpenScene(
                SourceScenePath,
                OpenSceneMode.Single);
            GameObject sourceRoot = RequiredNamedObject(
                sourceScene,
                SourceRootName);
            GameObject prepareSource = RequiredNamedObject(
                sourceScene,
                "BattleLikePreviewArea");
            GameObject backgroundSource = RequiredNamedObject(
                sourceScene,
                "gameBackground");
            ValidateSourceComposition(sourceRoot);
            ValidatePrepareSourceComposition(prepareSource);
            ValidateBackgroundSource(backgroundSource);
            ValidatePrefabAgainstSource(sourceRoot);
            ValidatePreparePrefabAgainstSource(prepareSource);
            ValidateBackgroundPrefabAgainstSource(backgroundSource);
            ValidatePreservedCarrierHashes();
            ValidateProtectedHashes();
            ValidateAcceptedUnifiedShellPrefab();

            Scene unifiedScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            AuthorUnifiedScene(unifiedScene);
            EditorSceneManager.MarkSceneDirty(unifiedScene);
            Require(
                EditorSceneManager.SaveScene(unifiedScene),
                "UNIFIED_EXACT_NAVIGATION_SCENE_SAVE_FAILED");
            AssetDatabase.ImportAsset(
                UnifiedScenePath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);

            Scene validationScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            ValidateFinalUnifiedScene(validationScene);
            ValidateRuntimeSourceBoundaries();
            ValidatePreservedCarrierHashes();
            ValidateProtectedHashes();
            Debug.Log(
                "[ExactNavigationPhaseA] REV05_NARROW_MOUNT_STATIC_VALID "
                + "exactNavigation=1 prepareSurface=1 background=1 "
                + "hostReferences=valid shellReferences=valid "
                + "missingScripts=0 exactItemLinks=2");
        }

        private static void RestoreRequiredInertShellSlotsSinglePass()
        {
            Require(!Application.isPlaying
                    && !EditorApplication.isPlayingOrWillChangePlaymode,
                "SHELL_REQUIRED_INERT_SLOTS_REQUIRES_NON_PLAY_EDITOR");
            ValidateRequiredInertSlotProtectedHashes();

            GameObject root = PrefabUtility.LoadPrefabContents(
                UnifiedShellPrefabPath);
            Require(root != null, "UNIFIED_SHELL_PREFAB_LOAD_FAILED");
            try
            {
                PreflightRequiredInertShellSlots(
                    root,
                    out UnifiedBattlePageShell shell,
                    out UnifiedBattleFormalSceneHost host,
                    out Transform battlePageRoot);
                RequireFileHash(
                    UnifiedShellPrefabPath,
                    RequiredInertSlotShellInputSha256,
                    "SHELL_REQUIRED_INERT_SLOT_INPUT_HASH_CHANGED");

                ShellPreservationSnapshot preservation =
                    ShellPreservationSnapshot.Capture(root, shell);
                Transform boardArea = CreateRequiredInertShellSlot(
                    battlePageRoot,
                    UnifiedBattlePageShellSlotNames.BoardArea);
                Transform itemTrayArea = CreateRequiredInertShellSlot(
                    battlePageRoot,
                    UnifiedBattlePageShellSlotNames.ItemTrayArea);
                AssignRequiredInertShellSlotReferences(
                    shell,
                    boardArea,
                    itemTrayArea);

                preservation.Validate(shell);
                ValidateRequiredInertShellSlotsLoaded(root, shell, host);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    UnifiedShellPrefabPath);
                Require(saved != null,
                    "SHELL_REQUIRED_INERT_SLOTS_PREFAB_SAVE_FAILED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.ImportAsset(
                UnifiedShellPrefabPath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);
            GameObject validationRoot = PrefabUtility.LoadPrefabContents(
                UnifiedShellPrefabPath);
            Require(validationRoot != null,
                "UNIFIED_SHELL_PREFAB_POST_SAVE_LOAD_FAILED");
            try
            {
                UnifiedBattlePageShell shell = validationRoot
                    .GetComponentsInChildren<UnifiedBattlePageShell>(true)
                    .SingleOrDefault();
                UnifiedBattleFormalSceneHost host = validationRoot
                    .GetComponentsInChildren<
                        UnifiedBattleFormalSceneHost>(true)
                    .SingleOrDefault();
                Require(shell != null && host != null,
                    "SHELL_REQUIRED_INERT_SLOTS_POST_SAVE_CORE_MISSING");
                ValidateRequiredInertShellSlotsLoaded(
                    validationRoot,
                    shell,
                    host);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(validationRoot);
            }

            ValidateRequiredInertSlotProtectedHashes();
        }

        private static void PreflightRequiredInertShellSlots(
            GameObject root,
            out UnifiedBattlePageShell shell,
            out UnifiedBattleFormalSceneHost host,
            out Transform battlePageRoot)
        {
            UnifiedBattlePageShell[] shells = root.GetComponentsInChildren<
                UnifiedBattlePageShell>(true);
            UnifiedBattleFormalSceneHost[] hosts = root.GetComponentsInChildren<
                UnifiedBattleFormalSceneHost>(true);
            Require(shells.Length == 1 && shells[0].gameObject == root,
                "SHELL_REQUIRED_INERT_SLOT_SHELL_COUNT_OR_ROOT_INVALID count="
                + shells.Length);
            Require(hosts.Length == 1,
                "SHELL_REQUIRED_INERT_SLOT_HOST_COUNT_INVALID count="
                + hosts.Length);
            shell = shells[0];
            host = hosts[0];

            SerializedObject serializedShell = new SerializedObject(shell);
            serializedShell.Update();
            SerializedProperty battleRootProperty = serializedShell.FindProperty(
                "battlePageRoot");
            SerializedProperty boardProperty = serializedShell.FindProperty(
                "boardArea");
            SerializedProperty trayProperty = serializedShell.FindProperty(
                "itemTrayArea");
            Require(battleRootProperty != null
                    && boardProperty != null
                    && trayProperty != null,
                "SHELL_REQUIRED_INERT_SLOT_SERIALIZED_PROPERTY_MISSING");
            battlePageRoot = battleRootProperty.objectReferenceValue as Transform;
            Require(battlePageRoot == root.transform,
                "SHELL_REQUIRED_INERT_SLOT_BATTLE_PAGE_ROOT_INVALID");

            bool boardAssigned = boardProperty.objectReferenceValue != null;
            bool trayAssigned = trayProperty.objectReferenceValue != null;
            Require(boardAssigned == trayAssigned,
                "SHELL_REQUIRED_INERT_SLOT_PARTIAL_REFERENCE_STATE board="
                + (boardAssigned ? "1" : "0") + " tray="
                + (trayAssigned ? "1" : "0"));
            Require(!boardAssigned,
                "SHELL_REQUIRED_INERT_SLOT_REFERENCES_ALREADY_PRESENT");

            int boardNameCount = CountNamedTransforms(
                root,
                UnifiedBattlePageShellSlotNames.BoardArea);
            int trayNameCount = CountNamedTransforms(
                root,
                UnifiedBattlePageShellSlotNames.ItemTrayArea);
            Require(boardNameCount == 0 && trayNameCount == 0,
                "SHELL_REQUIRED_INERT_SLOT_NAME_CONFLICT BoardArea="
                + boardNameCount + " ItemTrayArea=" + trayNameCount);

            List<string> missing = shell.CollectMissingRequiredSlots();
            Require(missing.Count == 2
                    && missing.Contains(
                        UnifiedBattlePageShellSlotNames.BoardArea)
                    && missing.Contains(
                        UnifiedBattlePageShellSlotNames.ItemTrayArea),
                "SHELL_REQUIRED_INERT_SLOT_UNEXPECTED_MISSING_SET "
                + string.Join(",", missing));
            ValidateRequiredInertSlotRealItemChain(root, shell, host);
            Require(GameObjectUtility
                        .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                "SHELL_REQUIRED_INERT_SLOT_PREFLIGHT_MISSING_SCRIPT");
        }

        private static Transform CreateRequiredInertShellSlot(
            Transform battlePageRoot,
            string slotName)
        {
            GameObject carrier = new GameObject(
                slotName,
                typeof(RectTransform));
            RectTransform rect = carrier.GetComponent<RectTransform>();
            rect.SetParent(battlePageRoot, false);
            carrier.SetActive(false);
            return rect;
        }

        private static void AssignRequiredInertShellSlotReferences(
            UnifiedBattlePageShell shell,
            Transform boardArea,
            Transform itemTrayArea)
        {
            SerializedObject serializedShell = new SerializedObject(shell);
            serializedShell.Update();
            SerializedProperty boardProperty = serializedShell.FindProperty(
                "boardArea");
            SerializedProperty trayProperty = serializedShell.FindProperty(
                "itemTrayArea");
            Require(boardProperty != null && trayProperty != null,
                "SHELL_REQUIRED_INERT_SLOT_ASSIGN_PROPERTY_MISSING");
            boardProperty.objectReferenceValue = boardArea;
            trayProperty.objectReferenceValue = itemTrayArea;
            Require(serializedShell.ApplyModifiedPropertiesWithoutUndo(),
                "SHELL_REQUIRED_INERT_SLOT_REFERENCE_ASSIGN_FAILED");
            EditorUtility.SetDirty(shell);
        }

        private static void ValidateRequiredInertShellSlotsLoaded(
            GameObject root,
            UnifiedBattlePageShell shell,
            UnifiedBattleFormalSceneHost host)
        {
            Require(root.GetComponentsInChildren<UnifiedBattlePageShell>(true)
                        .Length == 1
                    && root.GetComponentsInChildren<
                        UnifiedBattleFormalSceneHost>(true).Length == 1
                    && shell.gameObject == root,
                "SHELL_REQUIRED_INERT_SLOT_POST_CORE_COUNT_INVALID");
            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
            bool hasBattlePageRoot = slots.TryGetValue(
                UnifiedBattlePageShellSlotNames.BattlePageRoot,
                out Transform battlePageRoot);
            bool hasBoardArea = slots.TryGetValue(
                UnifiedBattlePageShellSlotNames.BoardArea,
                out Transform boardArea);
            bool hasItemTrayArea = slots.TryGetValue(
                UnifiedBattlePageShellSlotNames.ItemTrayArea,
                out Transform itemTrayArea);
            Require(hasBattlePageRoot
                    && battlePageRoot == root.transform
                    && hasBoardArea
                    && hasItemTrayArea
                    && boardArea != null
                    && itemTrayArea != null,
                "SHELL_REQUIRED_INERT_SLOT_POST_REFERENCE_INVALID");

            Transform[] boardNames = NamedTransforms(
                root,
                UnifiedBattlePageShellSlotNames.BoardArea);
            Transform[] trayNames = NamedTransforms(
                root,
                UnifiedBattlePageShellSlotNames.ItemTrayArea);
            Require(boardNames.Length == 1
                    && trayNames.Length == 1
                    && boardNames[0] == boardArea
                    && trayNames[0] == itemTrayArea,
                "SHELL_REQUIRED_INERT_SLOT_POST_NAME_OR_REFERENCE_INVALID "
                + "BoardArea=" + boardNames.Length + " ItemTrayArea="
                + trayNames.Length);
            ValidateRequiredInertShellSlot(
                boardArea,
                battlePageRoot,
                UnifiedBattlePageShellSlotNames.BoardArea);
            ValidateRequiredInertShellSlot(
                itemTrayArea,
                battlePageRoot,
                UnifiedBattlePageShellSlotNames.ItemTrayArea);
            Require(shell.CollectMissingRequiredSlots().Count == 0,
                "SHELL_REQUIRED_INERT_SLOT_REQUIRED_SET_STILL_MISSING");
            ValidateRequiredInertSlotRealItemChain(root, shell, host);
            Require(host.ValidateAuthoredBindings(out string diagnostic),
                diagnostic);
            Require(GameObjectUtility
                        .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                "SHELL_REQUIRED_INERT_SLOT_POST_SAVE_MISSING_SCRIPT");
        }

        private static void ValidateRequiredInertShellSlot(
            Transform slot,
            Transform battlePageRoot,
            string slotName)
        {
            Component[] components = slot.GetComponents<Component>();
            Require(slot is RectTransform
                    && slot.parent == battlePageRoot
                    && !slot.gameObject.activeSelf
                    && slot.childCount == 0
                    && components.Length == 1
                    && components[0] == slot
                    && slot.GetComponentsInChildren<Graphic>(true)
                        .All(value => !value.raycastTarget)
                    && slot.GetComponentsInChildren<MonoBehaviour>(true)
                        .Length == 0
                    && slot.GetComponentsInChildren<
                        C1ExactBattleSandboxItemArrangementPresenter>(true)
                        .Length == 0
                    && slot.GetComponentsInChildren<
                        C1ExactBattleSandboxItemBoardView>(true).Length == 0
                    && slot.GetComponentsInChildren<
                        C1ExactBattleSandboxItemTrayView>(true).Length == 0
                    && slot.GetComponentsInChildren<
                        C1ExactBattleSandboxItemCardView>(true).Length == 0,
                "SHELL_REQUIRED_INERT_SLOT_NOT_EMPTY_OR_INERT " + slotName);
        }

        private static void ValidateRequiredInertSlotRealItemChain(
            GameObject root,
            UnifiedBattlePageShell shell,
            UnifiedBattleFormalSceneHost host)
        {
            C1ExactBattleSandboxPrepareSurfacePresenter[] preparePresenters =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxPrepareSurfacePresenter>(true);
            C1ExactBattleSandboxItemArrangementPresenter[] itemPresenters =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemArrangementPresenter>(true);
            C1ExactBattleSandboxItemBoardView[] boardViews =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemBoardView>(true);
            C1ExactBattleSandboxItemTrayView[] trayViews =
                root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemTrayView>(true);
            Require(preparePresenters.Length == 1
                    && itemPresenters.Length == 1
                    && boardViews.Length == 1
                    && trayViews.Length == 1,
                "SHELL_REQUIRED_INERT_SLOT_REAL_ITEM_CHAIN_COUNT_INVALID "
                + "prepare=" + preparePresenters.Length
                + " presenter=" + itemPresenters.Length
                + " board=" + boardViews.Length
                + " tray=" + trayViews.Length);

            C1ExactBattleSandboxPrepareSurfacePresenter prepare =
                preparePresenters[0];
            C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                itemPresenters[0];
            C1ExactBattleSandboxItemBoardView boardView = boardViews[0];
            C1ExactBattleSandboxItemTrayView trayView = trayViews[0];
            Require(prepare.transform.parent == shell.transform
                    && prepare.GetComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>()
                    == itemPresenter
                    && prepare.View != null
                    && prepare.View.BoardView == boardView
                    && prepare.View.TrayView == trayView
                    && itemPresenter.ValidateAuthoredReferences()
                    && prepare.ValidateAuthoredReferences(),
                "SHELL_REQUIRED_INERT_SLOT_REAL_ITEM_CHAIN_BINDING_INVALID");
            RequireExactNestedPrefabPath(
                prepare.gameObject,
                PrepareSurfacePrefabPath,
                "PrepareSurface");
            RequireExactNestedPrefabPath(
                boardView.gameObject,
                ExactBoardPrefabPath,
                "Board");
            RequireExactNestedPrefabPath(
                trayView.gameObject,
                ExactTrayPrefabPath,
                "ItemTray");

            C1ExactBattleSandboxItemCardView[] cards = root
                .GetComponentsInChildren<
                    C1ExactBattleSandboxItemCardView>(true);
            Require(cards.Length > 0
                    && cards.All(value => value.transform.IsChildOf(
                            prepare.transform)
                        && string.Equals(
                            PrefabUtility
                                .GetPrefabAssetPathOfNearestInstanceRoot(
                                    value.gameObject),
                            ExactCardPrefabPath,
                            StringComparison.Ordinal)),
                "SHELL_REQUIRED_INERT_SLOT_EXACT_CARD_CHAIN_INVALID count="
                + cards.Length);

            SerializedObject serializedHost = new SerializedObject(host);
            RequireHostReferenceValue(
                serializedHost,
                "shell",
                shell,
                false);
            RequireHostReferenceValue(
                serializedHost,
                "prepareSurfacePresenter",
                prepare,
                false);
            RequireHostReferenceValue(
                serializedHost,
                "itemPresenter",
                itemPresenter,
                false);
            RequireHostReferenceValue(
                serializedHost,
                "itemBoardView",
                boardView,
                false);
            RequireHostReferenceValue(
                serializedHost,
                "itemTrayView",
                trayView,
                false);
        }

        private static void RequireExactNestedPrefabPath(
            GameObject value,
            string expectedPath,
            string label)
        {
            string actualPath = PrefabUtility
                .GetPrefabAssetPathOfNearestInstanceRoot(value);
            Require(string.Equals(
                    actualPath,
                    expectedPath,
                    StringComparison.Ordinal),
                "SHELL_REQUIRED_INERT_SLOT_EXACT_SOURCE_INVALID " + label
                + " actual=" + actualPath);
        }

        private static Transform[] NamedTransforms(
            GameObject root,
            string name)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Where(value => string.Equals(
                    value.name,
                    name,
                    StringComparison.Ordinal))
                .ToArray();
        }

        private static int CountNamedTransforms(GameObject root, string name)
        {
            return NamedTransforms(root, name).Length;
        }

        private static void ValidateRequiredInertSlotProtectedHashes()
        {
            foreach (KeyValuePair<string, string> row in
                     RequiredInertSlotProtectedHashes)
            {
                RequireFileHash(
                    row.Key,
                    row.Value,
                    "SHELL_REQUIRED_INERT_SLOT_PROTECTED_HASH_CHANGED");
            }
        }

        private static void RequireFileHash(
            string path,
            string expected,
            string diagnostic)
        {
            Require(File.Exists(path), diagnostic + " missing=" + path);
            string actual = ComputeSha256(path);
            Require(string.Equals(actual, expected, StringComparison.Ordinal),
                diagnostic + " path=" + path + " actual=" + actual);
        }

        private static void ApplySinglePass()
        {
            ValidateProtectedHashes();
            ValidateRuntimeSourceBoundaries();

            Scene sourceScene = EditorSceneManager.OpenScene(
                SourceScenePath,
                OpenSceneMode.Single);
            GameObject sourceRoot = RequiredNamedObject(
                sourceScene,
                SourceRootName);
            GameObject prepareSource = RequiredNamedObject(
                sourceScene,
                "BattleLikePreviewArea");
            GameObject backgroundSource = RequiredNamedObject(
                sourceScene,
                "gameBackground");
            ValidateSourceComposition(sourceRoot);
            ValidatePrepareSourceComposition(prepareSource);
            ValidateBackgroundSource(backgroundSource);
            AuthorNavigationPrefab(sourceRoot);
            AuthorPrepareSurfacePrefab(prepareSource);
            AuthorGameBackgroundPrefab(backgroundSource);
            AssetDatabase.ImportAsset(
                NavigationPrefabPath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(
                PrepareSurfacePrefabPath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(
                GameBackgroundPrefabPath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);
            ValidatePrefabAgainstSource(sourceRoot);
            ValidatePreparePrefabAgainstSource(prepareSource);
            ValidateBackgroundPrefabAgainstSource(backgroundSource);

            AuthorUnifiedShellPrefab();
            AssetDatabase.ImportAsset(
                UnifiedShellPrefabPath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);

            Scene unifiedScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            AuthorUnifiedScene(unifiedScene);
            EditorSceneManager.MarkSceneDirty(unifiedScene);
            Require(
                EditorSceneManager.SaveScene(unifiedScene),
                "UNIFIED_EXACT_NAVIGATION_SCENE_SAVE_FAILED");
            AssetDatabase.ImportAsset(
                UnifiedScenePath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);

            Scene validationScene = EditorSceneManager.OpenScene(
                UnifiedScenePath,
                OpenSceneMode.Single);
            ValidateFinalUnifiedScene(validationScene);
            ValidateRuntimeSourceBoundaries();
            ValidateProtectedHashes();
            Debug.Log(
                "[ExactNavigationPhaseA] PHASE_A_STATIC_VALID "
                + "exactNavigation=1 prepareSurface=1 background=1 "
                + "visibleControls=5 listeners=host-bound fallbackBars=0 "
                + "missingScripts=0 exactItemLinks=2 inputChain=valid");
        }

        private static void AuthorNavigationPrefab(GameObject sourceRoot)
        {
            GameObject clone = Object.Instantiate(sourceRoot);
            try
            {
                clone.name = SourceRootName;
                clone.transform.SetParent(null, false);
                Button back = RequiredButton(
                    clone,
                    "V04BattlePrepareBackButton");
                Button primary = RequiredButton(
                    clone,
                    "V04BattlePrepareStateButton");
                Button arrangement = RequiredButton(
                    clone,
                    "V04BattlePrepareToggleButton");
                Button speed = RequiredButton(
                    clone,
                    "V04BattlePreparejiasu");
                Button battleLog = RequiredButton(
                    clone,
                    "V04BattlePreparezhandourizhi");
                Text battleLogLabel = RequiredLabel(battleLog);
                battleLogLabel.text = "战斗日志";

                foreach (Button button in new[]
                         {
                             back,
                             primary,
                             arrangement,
                             speed,
                             battleLog
                         })
                {
                    button.enabled = true;
                }

                C1ExactBattleSandboxNavigationBarView view =
                    clone.AddComponent<
                        C1ExactBattleSandboxNavigationBarView>();
                view.AssignForEditor(
                    back,
                    RequiredLabel(back),
                    primary,
                    RequiredLabel(primary),
                    arrangement,
                    RequiredLabel(arrangement),
                    speed,
                    RequiredLabel(speed),
                    battleLog,
                    battleLogLabel);
                C1ExactBattleSandboxNavigationPresenter presenter =
                    clone.AddComponent<
                        C1ExactBattleSandboxNavigationPresenter>();
                presenter.AssignForEditor(view);

                Require(view.ValidateAuthoredReferences()
                        && presenter.ValidateAuthoredReferences(),
                    "EXACT_NAVIGATION_RUNTIME_REFERENCES_INVALID");
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    clone,
                    NavigationPrefabPath);
                Require(saved != null,
                    "EXACT_NAVIGATION_PREFAB_SAVE_FAILED");
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        private static void AuthorPrepareSurfacePrefab(GameObject sourceRoot)
        {
            GameObject clone = Object.Instantiate(sourceRoot);
            try
            {
                clone.name = "C1ExactBattleSandboxPrepareSurface";
                clone.transform.SetParent(null, false);
                DestroyDirectChild(clone.transform, "BoardGridPreview");
                DestroyDirectChild(clone.transform, "ItemTrayPreview");
                DestroyDirectChild(clone.transform, "PlacementFeedback_Runtime");

                GameObject boardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                    ExactBoardPrefabPath);
                GameObject trayAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                    ExactTrayPrefabPath);
                Require(boardAsset != null && trayAsset != null,
                    "EXACT_PREPARE_ITEM_PREFAB_ASSET_MISSING");
                GameObject board = PrefabUtility.InstantiatePrefab(
                    boardAsset,
                    clone.transform) as GameObject;
                GameObject tray = PrefabUtility.InstantiatePrefab(
                    trayAsset,
                    clone.transform) as GameObject;
                Require(board != null && tray != null,
                    "EXACT_PREPARE_ITEM_PREFAB_INSTANTIATE_FAILED");
                board.transform.SetSiblingIndex(1);
                tray.transform.SetSiblingIndex(2);

                C1ExactBattleSandboxItemBoardView boardView =
                    board.GetComponent<C1ExactBattleSandboxItemBoardView>();
                C1ExactBattleSandboxItemTrayView trayView =
                    tray.GetComponent<C1ExactBattleSandboxItemTrayView>();
                Require(boardView != null && trayView != null,
                    "EXACT_PREPARE_ITEM_VIEW_MISSING");

                CanvasGroup inputGate = clone.GetComponent<CanvasGroup>();
                if (inputGate == null)
                {
                    inputGate = clone.AddComponent<CanvasGroup>();
                }

                C1ExactBattleSandboxPrepareSurfaceView view =
                    clone.GetComponent<
                        C1ExactBattleSandboxPrepareSurfaceView>();
                if (view == null)
                {
                    view = clone.AddComponent<
                        C1ExactBattleSandboxPrepareSurfaceView>();
                }

                view.AssignForEditor(
                    (RectTransform)clone.transform,
                    inputGate,
                    boardView,
                    trayView);
                C1ExactBattleSandboxPrepareSurfacePresenter presenter =
                    clone.GetComponent<
                        C1ExactBattleSandboxPrepareSurfacePresenter>();
                if (presenter == null)
                {
                    presenter = clone.AddComponent<
                        C1ExactBattleSandboxPrepareSurfacePresenter>();
                }

                presenter.AssignForEditor(view);
                C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                    clone.GetComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>();
                if (itemPresenter == null)
                {
                    itemPresenter = clone.AddComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>();
                }

                itemPresenter.AssignForEditor(boardView, trayView);
                presenter.ResetForLifecycle(1);
                Require(view.ValidateAuthoredReferences()
                        && presenter.ValidateAuthoredReferences()
                        && itemPresenter.ValidateAuthoredReferences(),
                    "EXACT_PREPARE_RUNTIME_REFERENCES_INVALID");
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    clone,
                    PrepareSurfacePrefabPath);
                Require(saved != null,
                    "EXACT_PREPARE_PREFAB_SAVE_FAILED");
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        private static void AuthorGameBackgroundPrefab(GameObject sourceRoot)
        {
            GameObject clone = Object.Instantiate(sourceRoot);
            try
            {
                clone.name = "C1ExactBattleSandboxGameBackground";
                clone.transform.SetParent(null, false);
                Image image = clone.GetComponent<Image>();
                Require(image != null,
                    "EXACT_GAME_BACKGROUND_IMAGE_MISSING");
                image.raycastTarget = false;
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    clone,
                    GameBackgroundPrefabPath);
                Require(saved != null,
                    "EXACT_GAME_BACKGROUND_PREFAB_SAVE_FAILED");
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        private static void AuthorUnifiedShellPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                UnifiedShellPrefabPath);
            Require(root != null, "UNIFIED_SHELL_PREFAB_LOAD_FAILED");
            try
            {
                UnifiedBattlePageShell shell =
                    root.GetComponent<UnifiedBattlePageShell>();
                UnifiedBattleFormalSceneHost host =
                    root.GetComponentsInChildren<
                            UnifiedBattleFormalSceneHost>(true)
                        .SingleOrDefault();
                Require(shell != null && host != null,
                    "UNIFIED_SHELL_FORMAL_HOST_MISSING");

                C1ExactBattleSandboxNavigationPresenter[] existing =
                    root.GetComponentsInChildren<
                        C1ExactBattleSandboxNavigationPresenter>(true);
                Require(existing.Length <= 1,
                    "UNIFIED_EXACT_NAVIGATION_DUPLICATE_IN_SHELL");
                C1ExactBattleSandboxNavigationPresenter presenter;
                if (existing.Length == 1)
                {
                    presenter = existing[0];
                }
                else
                {
                    GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(
                        NavigationPrefabPath);
                    Require(asset != null,
                        "EXACT_NAVIGATION_PREFAB_ASSET_MISSING");
                    GameObject instance = PrefabUtility.InstantiatePrefab(
                        asset,
                        root.transform) as GameObject;
                    Require(instance != null,
                        "UNIFIED_EXACT_NAVIGATION_INSTANTIATE_FAILED");
                    instance.transform.SetAsLastSibling();
                    presenter = instance.GetComponent<
                        C1ExactBattleSandboxNavigationPresenter>();
                }

                Require(presenter != null
                        && presenter.transform.parent == root.transform
                        && string.Equals(
                            PrefabUtility
                                .GetPrefabAssetPathOfNearestInstanceRoot(
                                    presenter.gameObject),
                            NavigationPrefabPath,
                            StringComparison.Ordinal),
                    "UNIFIED_EXACT_NAVIGATION_NESTED_SOURCE_INVALID");

                C1ExactBattleSandboxPrepareSurfacePresenter[] prepareExisting =
                    root.GetComponentsInChildren<
                        C1ExactBattleSandboxPrepareSurfacePresenter>(true);
                Require(prepareExisting.Length <= 1,
                    "UNIFIED_EXACT_PREPARE_DUPLICATE_IN_SHELL");
                C1ExactBattleSandboxPrepareSurfacePresenter preparePresenter;
                if (prepareExisting.Length == 1)
                {
                    preparePresenter = prepareExisting[0];
                }
                else
                {
                    GameObject prepareAsset =
                        AssetDatabase.LoadAssetAtPath<GameObject>(
                            PrepareSurfacePrefabPath);
                    Require(prepareAsset != null,
                        "EXACT_PREPARE_PREFAB_ASSET_MISSING");
                    GameObject instance = PrefabUtility.InstantiatePrefab(
                        prepareAsset,
                        root.transform) as GameObject;
                    Require(instance != null,
                        "UNIFIED_EXACT_PREPARE_INSTANTIATE_FAILED");
                    preparePresenter = instance.GetComponent<
                        C1ExactBattleSandboxPrepareSurfacePresenter>();
                }

                Require(preparePresenter != null
                        && preparePresenter.transform.parent == root.transform
                        && string.Equals(
                            PrefabUtility
                                .GetPrefabAssetPathOfNearestInstanceRoot(
                                    preparePresenter.gameObject),
                            PrepareSurfacePrefabPath,
                            StringComparison.Ordinal),
                    "UNIFIED_EXACT_PREPARE_NESTED_SOURCE_INVALID");

                Image[] backgroundCandidates = root
                    .GetComponentsInChildren<Image>(true)
                    .Where(value => string.Equals(
                        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                            value.gameObject),
                        GameBackgroundPrefabPath,
                        StringComparison.Ordinal))
                    .ToArray();
                Require(backgroundCandidates.Length <= 1,
                    "UNIFIED_EXACT_BACKGROUND_DUPLICATE_IN_SHELL");
                Image background;
                if (backgroundCandidates.Length == 1)
                {
                    background = backgroundCandidates[0];
                }
                else
                {
                    GameObject backgroundAsset =
                        AssetDatabase.LoadAssetAtPath<GameObject>(
                            GameBackgroundPrefabPath);
                    Require(backgroundAsset != null,
                        "EXACT_GAME_BACKGROUND_PREFAB_ASSET_MISSING");
                    GameObject instance = PrefabUtility.InstantiatePrefab(
                        backgroundAsset,
                        root.transform) as GameObject;
                    Require(instance != null,
                        "UNIFIED_EXACT_BACKGROUND_INSTANTIATE_FAILED");
                    background = instance.GetComponent<Image>();
                }

                Require(background != null && !background.raycastTarget,
                    "UNIFIED_EXACT_BACKGROUND_RAYCAST_INVALID");
                background.transform.SetSiblingIndex(0);
                preparePresenter.transform.SetAsLastSibling();
                presenter.transform.SetAsLastSibling();

                C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                    preparePresenter.GetComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>();
                C1ExactBattleSandboxItemBoardView boardView =
                    preparePresenter.View.BoardView;
                C1ExactBattleSandboxItemTrayView trayView =
                    preparePresenter.View.TrayView;
                Require(itemPresenter != null
                        && itemPresenter.ValidateAuthoredReferences(),
                    "UNIFIED_EXACT_PREPARE_ITEM_PRESENTER_INVALID");

                Transform boardSlot = RequiredSlot(
                    shell,
                    UnifiedBattlePageShellSlotNames.BoardArea);
                Transform traySlot = RequiredSlot(
                    shell,
                    UnifiedBattlePageShellSlotNames.ItemTrayArea);
                boardSlot.gameObject.SetActive(false);
                traySlot.gameObject.SetActive(false);
                DisableRaycastsInSubtree(boardSlot);
                DisableRaycastsInSubtree(traySlot);

                Transform actionSlot = RequiredSlot(
                    shell,
                    UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot);
                Button retiredButton = actionSlot.GetComponent<Button>();
                Require(retiredButton != null,
                    "UNIFIED_LEGACY_ACTION_BUTTON_MISSING");
                retiredButton.enabled = false;
                retiredButton.interactable = false;
                Graphic retiredGraphic = actionSlot.GetComponent<Graphic>();
                if (retiredGraphic != null)
                {
                    retiredGraphic.raycastTarget = false;
                }

                Transform retiredTitle = DirectChild(actionSlot, "Title");
                if (retiredTitle != null)
                {
                    retiredTitle.gameObject.SetActive(false);
                }

                host.AssignNavigationForEditor(presenter);
                host.AssignPrepareSurfaceForEditor(
                    preparePresenter,
                    background,
                    itemPresenter,
                    boardView,
                    trayView);
                EditorUtility.SetDirty(host);
                Require(host.ValidateNavigationAuthoredBinding(),
                    "UNIFIED_SHELL_NAVIGATION_HOST_BINDING_INVALID");
                Require(host.ValidateAuthoredBindings(out string diagnostic),
                    diagnostic);
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                    root,
                    UnifiedShellPrefabPath);
                Require(saved != null,
                    "UNIFIED_SHELL_PREFAB_SAVE_FAILED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AuthorUnifiedScene(Scene scene)
        {
            UnifiedBattlePageShell shell =
                SingleInScene<UnifiedBattlePageShell>(scene);
            UnifiedBattleFormalSceneHost host =
                SingleInScene<UnifiedBattleFormalSceneHost>(scene);
            C1ExactBattleSandboxPrepareSurfacePresenter preparePresenter =
                SingleInScene<
                    C1ExactBattleSandboxPrepareSurfacePresenter>(scene);
            C1ExactBattleSandboxItemArrangementPresenter preparedItemPresenter =
                preparePresenter.GetComponent<
                    C1ExactBattleSandboxItemArrangementPresenter>();
            Require(preparedItemPresenter != null,
                "UNIFIED_EXACT_PREPARE_ITEM_PRESENTER_MISSING");

            RemoveRetiredSceneAddedItemPresenter(
                scene,
                shell,
                preparedItemPresenter);
            RemoveRetiredExactItemInstance(
                scene,
                shell,
                preparePresenter.transform,
                typeof(C1ExactBattleSandboxItemBoardView),
                ExactBoardPrefabPath,
                UnifiedBattlePageShellSlotNames.BoardArea);
            RemoveRetiredExactItemInstance(
                scene,
                shell,
                preparePresenter.transform,
                typeof(C1ExactBattleSandboxItemTrayView),
                ExactTrayPrefabPath,
                UnifiedBattlePageShellSlotNames.ItemTrayArea);

            C1ExactBattleSandboxItemBoardView boardView =
                preparePresenter.View.BoardView;
            C1ExactBattleSandboxItemTrayView trayView =
                preparePresenter.View.TrayView;
            RevertRequiredHostSceneReferenceOverride(host, "itemPresenter");
            RevertRequiredHostSceneReferenceOverride(host, "itemBoardView");
            RevertRequiredHostSceneReferenceOverride(host, "itemTrayView");
            ValidateHostItemReferenceInheritance(
                host,
                preparedItemPresenter,
                boardView,
                trayView);
            GameObject shellInstanceRoot =
                PrefabUtility.GetOutermostPrefabInstanceRoot(
                    shell.gameObject);
            Require(shellInstanceRoot == shell.gameObject
                    && string.Equals(
                        PrefabUtility
                            .GetPrefabAssetPathOfNearestInstanceRoot(
                                shellInstanceRoot),
                        UnifiedShellPrefabPath,
                        StringComparison.Ordinal),
                "UNIFIED_REQUIRED_HOST_SHELL_INSTANCE_ROOT_INVALID");
            SceneOverrideSnapshot overrideSnapshot =
                SceneOverrideSnapshot.Capture(shellInstanceRoot);
            HostReferenceRepairResult repairResult =
                RepairRequiredHostReferences(host, shellInstanceRoot);
            overrideSnapshot.ValidateAfter(
                shellInstanceRoot,
                repairResult.RevertedPropertyModificationSignatures,
                repairResult.RevertedRemovedGameObjectSignature);
            ValidateRequiredHostReferences(host);
            Require(host.ValidateAuthoredBindings(out string diagnostic),
                diagnostic);
        }

        private static void ValidateAcceptedUnifiedShellPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                UnifiedShellPrefabPath);
            Require(root != null, "UNIFIED_SHELL_PREFAB_LOAD_FAILED");
            try
            {
                UnifiedBattlePageShell shell =
                    root.GetComponent<UnifiedBattlePageShell>();
                UnifiedBattleFormalSceneHost host =
                    root.GetComponentsInChildren<
                            UnifiedBattleFormalSceneHost>(true)
                        .SingleOrDefault();
                C1ExactBattleSandboxPrepareSurfacePresenter preparePresenter =
                    root.GetComponentsInChildren<
                            C1ExactBattleSandboxPrepareSurfacePresenter>(true)
                        .SingleOrDefault();
                Require(shell != null && host != null
                        && preparePresenter != null,
                    "UNIFIED_ACCEPTED_SHELL_CORE_REFERENCE_MISSING");
                C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                    preparePresenter.GetComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>();
                C1ExactBattleSandboxItemBoardView boardView =
                    preparePresenter.View.BoardView;
                C1ExactBattleSandboxItemTrayView trayView =
                    preparePresenter.View.TrayView;
                Require(itemPresenter != null
                        && root.GetComponentsInChildren<
                            C1ExactBattleSandboxItemArrangementPresenter>(true)
                            .Length == 1
                        && root.GetComponentsInChildren<
                            C1ExactBattleSandboxItemBoardView>(true).Length == 1
                        && root.GetComponentsInChildren<
                            C1ExactBattleSandboxItemTrayView>(true).Length == 1,
                    "UNIFIED_ACCEPTED_SHELL_ITEM_COUNT_INVALID");
                SerializedObject serializedHost = new SerializedObject(host);
                RequireHostReferenceValue(
                    serializedHost,
                    "itemPresenter",
                    itemPresenter,
                    false);
                RequireHostReferenceValue(
                    serializedHost,
                    "itemBoardView",
                    boardView,
                    false);
                RequireHostReferenceValue(
                    serializedHost,
                    "itemTrayView",
                    trayView,
                    false);
                ValidateRequiredHostReferences(host);
                Require(host.ValidateAuthoredBindings(out string diagnostic),
                    diagnostic);
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "UNIFIED_ACCEPTED_SHELL_MISSING_SCRIPT");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static HostReferenceRepairResult RepairRequiredHostReferences(
            UnifiedBattleFormalSceneHost host,
            GameObject shellInstanceRoot)
        {
            UnifiedBattleFormalSceneHost assetHost =
                PrefabUtility.GetCorrespondingObjectFromSource(host);
            Require(assetHost != null
                    && string.Equals(
                        AssetDatabase.GetAssetPath(assetHost),
                        UnifiedShellPrefabPath,
                        StringComparison.Ordinal),
                "UNIFIED_REQUIRED_HOST_ACCEPTED_SOURCE_MISSING");

            HostReferenceRepairResult result =
                new HostReferenceRepairResult();
            List<MissingRequiredHostReference> removedCarrierMissing =
                new List<MissingRequiredHostReference>();
            foreach (string propertyPath in RequiredHostReferencePaths)
            {
                SerializedObject serializedHost = new SerializedObject(host);
                SerializedObject serializedAssetHost =
                    new SerializedObject(assetHost);
                serializedHost.Update();
                serializedAssetHost.Update();
                SerializedProperty property =
                    serializedHost.FindProperty(propertyPath);
                SerializedProperty assetProperty =
                    serializedAssetHost.FindProperty(propertyPath);
                Require(property != null && assetProperty != null,
                    "UNIFIED_REQUIRED_HOST_PROPERTY_MISSING path="
                    + propertyPath);

                Object acceptedReference = assetProperty.objectReferenceValue;
                Require(acceptedReference != null,
                    "UNIFIED_REQUIRED_HOST_ACCEPTED_REFERENCE_NULL path="
                    + propertyPath);
                if (property.objectReferenceValue != null)
                {
                    continue;
                }

                GameObject acceptedCarrier =
                    RequiredReferenceCarrierGameObject(
                        acceptedReference,
                        propertyPath);
                if (!property.prefabOverride)
                {
                    removedCarrierMissing.Add(
                        new MissingRequiredHostReference(
                            propertyPath,
                            acceptedReference,
                            acceptedCarrier));
                    continue;
                }

                string modificationSignature =
                    RequiredPropertyModificationSignature(
                        shellInstanceRoot,
                        assetHost,
                        propertyPath);
                PrefabUtility.RevertPropertyOverride(
                    property,
                    InteractionMode.AutomatedAction);

                serializedHost = new SerializedObject(host);
                serializedHost.Update();
                property = serializedHost.FindProperty(propertyPath);
                Require(property != null
                        && property.objectReferenceValue != null
                        && !property.prefabOverride,
                    "UNIFIED_REQUIRED_HOST_NULL_PROPERTY_REVERT_FAILED path="
                    + propertyPath
                    + " overrideKind=NULL_PROPERTY_OVERRIDE"
                    + " assetComponent="
                    + DescribeObject(acceptedReference)
                    + " assetGameObject="
                    + DescribeObject(acceptedCarrier));
                result.RevertedPropertyModificationSignatures.Add(
                    modificationSignature);
                Debug.Log(
                    "[ExactNavigationPhaseA] "
                    + "REV08_REQUIRED_HOST_REFERENCE_REPAIRED path="
                    + propertyPath
                    + " overrideKind=NULL_PROPERTY_OVERRIDE"
                    + " matchedAssetComponent="
                    + DescribeObject(acceptedReference)
                    + " matchedAssetGameObject="
                    + DescribeObject(acceptedCarrier)
                    + " inheritedReference="
                    + DescribeObject(property.objectReferenceValue)
                    + " prefabOverride=false");
            }

            Require(removedCarrierMissing.Count <= 1,
                "UNIFIED_REQUIRED_HOST_REMOVED_CARRIER_MISSING_COUNT_INVALID "
                + "count=" + removedCarrierMissing.Count
                + " paths=" + string.Join(",",
                    removedCarrierMissing.Select(value => value.PropertyPath)));
            if (removedCarrierMissing.Count == 1)
            {
                RestoreUniqueRequiredHostRemovedCarrier(
                    host,
                    shellInstanceRoot,
                    removedCarrierMissing[0],
                    result);
            }

            ValidateRequiredHostReferences(host);
            return result;
        }

        private static void RestoreUniqueRequiredHostRemovedCarrier(
            UnifiedBattleFormalSceneHost host,
            GameObject shellInstanceRoot,
            MissingRequiredHostReference missing,
            HostReferenceRepairResult result)
        {
            RemovedGameObject[] removed =
                (PrefabUtility.GetRemovedGameObjects(shellInstanceRoot)
                    ?? new List<RemovedGameObject>()).ToArray();
            RemovedGameObject[] belongingToShell = removed
                .Where(value => value != null
                    && value.assetGameObject != null
                    && value.parentOfRemovedGameObjectInInstance != null
                    && PrefabUtility.GetOutermostPrefabInstanceRoot(
                        value.parentOfRemovedGameObjectInInstance)
                    == shellInstanceRoot)
                .ToArray();
            RemovedGameObject[] matches = belongingToShell
                .Where(value => value.assetGameObject
                    == missing.AcceptedCarrierGameObject)
                .ToArray();
            Require(removed.Length == 1
                    && belongingToShell.Length == 1
                    && matches.Length == 1,
                "UNIFIED_REQUIRED_HOST_REMOVED_CARRIER_MAPPING_INVALID path="
                + missing.PropertyPath
                + " overrideKind=REMOVED_GAMEOBJECT_OVERRIDE"
                + " removedCount=" + removed.Length
                + " shellRemovedCount=" + belongingToShell.Length
                + " matchCount=" + matches.Length
                + " assetComponent="
                + DescribeObject(missing.AcceptedReference)
                + " assetGameObject="
                + DescribeObject(missing.AcceptedCarrierGameObject));

            RemovedGameObject matched = matches[0];
            string removedSignature =
                SceneOverrideSnapshot.DescribeRemovedGameObject(matched);
            matched.Revert(InteractionMode.AutomatedAction);

            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.Update();
            SerializedProperty property =
                serializedHost.FindProperty(missing.PropertyPath);
            Require(property != null
                    && property.objectReferenceValue != null
                    && !property.prefabOverride,
                "UNIFIED_REQUIRED_HOST_REMOVED_CARRIER_REVERT_FAILED path="
                + missing.PropertyPath
                + " overrideKind=REMOVED_GAMEOBJECT_OVERRIDE"
                + " assetComponent="
                + DescribeObject(missing.AcceptedReference)
                + " assetGameObject="
                + DescribeObject(missing.AcceptedCarrierGameObject));
            result.RevertedRemovedGameObjectSignature = removedSignature;
            Debug.Log(
                "[ExactNavigationPhaseA] "
                + "REV08_REQUIRED_HOST_REFERENCE_REPAIRED path="
                + missing.PropertyPath
                + " overrideKind=REMOVED_GAMEOBJECT_OVERRIDE"
                + " matchedAssetComponent="
                + DescribeObject(missing.AcceptedReference)
                + " matchedAssetGameObject="
                + DescribeObject(missing.AcceptedCarrierGameObject)
                + " inheritedReference="
                + DescribeObject(property.objectReferenceValue)
                + " prefabOverride=false");
        }

        private static void ValidateRequiredHostReferences(
            UnifiedBattleFormalSceneHost host)
        {
            SerializedObject serializedHost = new SerializedObject(host);
            serializedHost.Update();
            foreach (string propertyPath in RequiredHostReferencePaths)
            {
                SerializedProperty property =
                    serializedHost.FindProperty(propertyPath);
                Require(property != null
                        && property.objectReferenceValue != null,
                    "UNIFIED_REQUIRED_HOST_REFERENCE_STILL_MISSING path="
                    + propertyPath
                    + " prefabOverride="
                    + (property != null && property.prefabOverride));
            }
        }

        private static GameObject RequiredReferenceCarrierGameObject(
            Object reference,
            string propertyPath)
        {
            if (reference is Component component)
            {
                return component.gameObject;
            }

            if (reference is GameObject gameObject)
            {
                return gameObject;
            }

            throw new InvalidOperationException(
                "UNIFIED_REQUIRED_HOST_REFERENCE_CARRIER_UNSUPPORTED path="
                + propertyPath + " type="
                + (reference == null
                    ? "<null>"
                    : reference.GetType().FullName));
        }

        private static string RequiredPropertyModificationSignature(
            GameObject shellInstanceRoot,
            UnifiedBattleFormalSceneHost assetHost,
            string propertyPath)
        {
            PropertyModification[] matches =
                (PrefabUtility.GetPropertyModifications(shellInstanceRoot)
                    ?? Array.Empty<PropertyModification>())
                .Where(value => value != null
                    && value.target == assetHost
                    && string.Equals(
                        value.propertyPath,
                        propertyPath,
                        StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "UNIFIED_REQUIRED_HOST_NULL_PROPERTY_MODIFICATION_INVALID "
                + "path=" + propertyPath
                + " overrideKind=NULL_PROPERTY_OVERRIDE"
                + " count=" + matches.Length);
            return SceneOverrideSnapshot.DescribePropertyModification(
                matches[0]);
        }

        private static string DescribeObject(Object value)
        {
            if (value == null)
            {
                return "<null>";
            }

            return value.GetType().Name + ":" + value.name + "@"
                   + SceneOverrideSnapshot.DescribeObjectIdentity(value);
        }

        private static void RevertRequiredHostSceneReferenceOverride(
            UnifiedBattleFormalSceneHost host,
            string propertyPath)
        {
            SerializedObject serializedHost = new SerializedObject(host);
            SerializedProperty property =
                serializedHost.FindProperty(propertyPath);
            Require(property != null
                    && property.prefabOverride,
                "UNIFIED_HOST_ITEM_REFERENCE_OVERRIDE_MISSING path="
                + propertyPath);
            PrefabUtility.RevertPropertyOverride(
                property,
                InteractionMode.AutomatedAction);

            serializedHost = new SerializedObject(host);
            property = serializedHost.FindProperty(propertyPath);
            Require(property != null
                    && !property.prefabOverride,
                "UNIFIED_HOST_ITEM_REFERENCE_OVERRIDE_REVERT_FAILED path="
                + propertyPath);
        }

        private static void ValidateHostItemReferenceInheritance(
            UnifiedBattleFormalSceneHost host,
            C1ExactBattleSandboxItemArrangementPresenter itemPresenter,
            C1ExactBattleSandboxItemBoardView boardView,
            C1ExactBattleSandboxItemTrayView trayView)
        {
            SerializedObject serializedHost = new SerializedObject(host);
            RequireHostReferenceValue(
                serializedHost,
                "itemPresenter",
                itemPresenter,
                true);
            RequireHostReferenceValue(
                serializedHost,
                "itemBoardView",
                boardView,
                true);
            RequireHostReferenceValue(
                serializedHost,
                "itemTrayView",
                trayView,
                true);
        }

        private static void RequireHostReferenceValue(
            SerializedObject serializedHost,
            string propertyPath,
            Object expected,
            bool requireInherited)
        {
            serializedHost.Update();
            SerializedProperty property =
                serializedHost.FindProperty(propertyPath);
            Require(property != null
                    && property.objectReferenceValue == expected
                    && (!requireInherited
                        || !property.prefabOverride),
                "UNIFIED_HOST_ITEM_REFERENCE_INHERITANCE_INVALID path="
                + propertyPath);
        }

        private static void ValidateSourceComposition(GameObject sourceRoot)
        {
            Require(sourceRoot != null && sourceRoot.transform.parent != null
                    && string.Equals(
                        sourceRoot.transform.parent.name,
                        "SafeAreaRoot",
                        StringComparison.Ordinal),
                "EXACT_NAVIGATION_SOURCE_PARENT_INVALID");
            Require(sourceRoot.transform.childCount == ExactChildNames.Length,
                "EXACT_NAVIGATION_SOURCE_CHILD_COUNT_INVALID");
            for (int index = 0; index < ExactChildNames.Length; index++)
            {
                Require(string.Equals(
                        sourceRoot.transform.GetChild(index).name,
                        ExactChildNames[index],
                        StringComparison.Ordinal),
                    "EXACT_NAVIGATION_SOURCE_SIBLING_ORDER_INVALID index="
                    + index);
            }

            RectTransform background = DirectChild(
                    sourceRoot.transform,
                    "BattlePrepareBottomActions") as RectTransform;
            Require(background != null
                    && background.sizeDelta == new Vector2(1080f, 144f),
                "EXACT_NAVIGATION_SOURCE_1080X144_CARRIER_MISSING");
            foreach (string name in ExactChildNames.Skip(1))
            {
                Button button = RequiredButton(sourceRoot, name);
                Require(button.onClick.GetPersistentEventCount() == 0,
                    "EXACT_NAVIGATION_SOURCE_BUTTON_HAS_SANDBOX_LISTENER "
                    + name);
                RequiredLabel(button);
            }
        }

        private static void ValidatePrepareSourceComposition(GameObject sourceRoot)
        {
            RectTransform rect = sourceRoot.transform as RectTransform;
            Require(rect != null
                    && rect.anchorMin == Vector2.zero
                    && rect.anchorMax == Vector2.one
                    && rect.anchoredPosition ==
                    C1ExactBattleSandboxPrepareSurfaceView
                        .SourceOpenAnchoredPosition
                    && rect.sizeDelta == new Vector2(80f, -191f)
                    && rect.localScale == new Vector3(1.02f, 1.02f, 1.02f),
                "EXACT_PREPARE_SOURCE_ROOT_GEOMETRY_INVALID");
            string[] expected =
            {
                "beibao_background",
                "BoardGridPreview",
                "ItemTrayPreview",
                "zhandouxinxi",
                "guajiann",
                "PlacementFeedback_Runtime"
            };
            Require(sourceRoot.transform.childCount == expected.Length,
                "EXACT_PREPARE_SOURCE_CHILD_COUNT_INVALID");
            for (int index = 0; index < expected.Length; index++)
            {
                Require(string.Equals(
                        sourceRoot.transform.GetChild(index).name,
                        expected[index],
                        StringComparison.Ordinal),
                    "EXACT_PREPARE_SOURCE_SIBLING_INVALID index=" + index);
            }
        }

        private static void ValidateBackgroundSource(GameObject sourceRoot)
        {
            RectTransform rect = sourceRoot.transform as RectTransform;
            Image image = sourceRoot.GetComponent<Image>();
            Require(rect != null && image != null
                    && rect.anchorMin == new Vector2(0.5f, 1f)
                    && rect.anchorMax == new Vector2(0.5f, 1f)
                    && rect.anchoredPosition == new Vector2(-3.5537f, -131.05f)
                    && rect.sizeDelta == new Vector2(1300f, 900f)
                    && rect.pivot == new Vector2(0.5f, 1f)
                    && rect.localScale == Vector3.one
                    && image.sprite != null,
                "EXACT_GAME_BACKGROUND_SOURCE_INVALID");
        }

        private static void ValidatePreparePrefabAgainstSource(
            GameObject sourceRoot)
        {
            ValidateSourceBoardInputBaseline();
            GameObject prefab = PrefabUtility.LoadPrefabContents(
                PrepareSurfacePrefabPath);
            Require(prefab != null, "EXACT_PREPARE_PREFAB_LOAD_FAILED");
            try
            {
                RectTransform sourceRect = (RectTransform)sourceRoot.transform;
                RectTransform prefabRect = (RectTransform)prefab.transform;
                Require(prefabRect.anchorMin == sourceRect.anchorMin
                        && prefabRect.anchorMax == sourceRect.anchorMax
                        && prefabRect.sizeDelta == sourceRect.sizeDelta
                        && prefabRect.pivot == sourceRect.pivot
                        && prefabRect.localScale == sourceRect.localScale
                        && prefabRect.anchoredPosition ==
                        C1ExactBattleSandboxPrepareSurfaceView
                            .SourceClosedAnchoredPosition,
                    "EXACT_PREPARE_PREFAB_ROOT_GEOMETRY_INVALID");

                string[] expected =
                {
                    "beibao_background",
                    "C1ExactBattleSandboxItemBoard",
                    "C1ExactBattleSandboxItemTray",
                    "zhandouxinxi",
                    "guajiann"
                };
                Require(prefab.transform.childCount == expected.Length,
                    "EXACT_PREPARE_PREFAB_CHILD_COUNT_INVALID");
                for (int index = 0; index < expected.Length; index++)
                {
                    Require(string.Equals(
                            prefab.transform.GetChild(index).name,
                            expected[index],
                            StringComparison.Ordinal),
                        "EXACT_PREPARE_PREFAB_SIBLING_INVALID index=" + index);
                }

                foreach (string decorative in new[]
                         {
                             "beibao_background",
                             "zhandouxinxi",
                             "guajiann"
                         })
                {
                    ValidateVisualHierarchy(
                        DirectChild(sourceRoot.transform, decorative),
                        DirectChild(prefab.transform, decorative),
                        false);
                }

                C1ExactBattleSandboxPrepareSurfacePresenter presenter =
                    prefab.GetComponent<
                        C1ExactBattleSandboxPrepareSurfacePresenter>();
                C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                    prefab.GetComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>();
                Require(presenter != null
                        && presenter.ValidateAuthoredReferences()
                        && itemPresenter != null
                        && itemPresenter.ValidateAuthoredReferences()
                        && !presenter.View.InputGate.blocksRaycasts
                        && !presenter.View.InputGate.interactable,
                    "EXACT_PREPARE_PREFAB_RUNTIME_GATE_INVALID");

                C1ExactBattleSandboxItemBoardView board =
                    presenter.View.BoardView;
                C1ExactBattleSandboxItemTrayView tray = presenter.View.TrayView;
                Require(string.Equals(
                            PrefabUtility
                                .GetPrefabAssetPathOfNearestInstanceRoot(
                                    board.gameObject),
                            ExactBoardPrefabPath,
                            StringComparison.Ordinal)
                        && string.Equals(
                            PrefabUtility
                                .GetPrefabAssetPathOfNearestInstanceRoot(
                                    tray.gameObject),
                            ExactTrayPrefabPath,
                            StringComparison.Ordinal),
                    "EXACT_PREPARE_ITEM_NESTED_SOURCE_INVALID");
                ValidateNestedBoardRaycastOverride(board);
                ValidateRootRectMatchesSource(
                    RequiredNamedChild(sourceRoot, "BoardGridPreview"),
                    board.gameObject,
                    "BOARD");
                ValidateRootRectMatchesSource(
                    RequiredNamedChild(sourceRoot, "ItemTrayPreview"),
                    tray.gameObject,
                    "TRAY");
                Require(prefab.GetComponentsInChildren<
                            C1ExactBattleSandboxItemCardView>(true).Length > 0
                        && prefab.GetComponentsInChildren<MonoBehaviour>(true)
                            .All(value => value == null
                                || (!string.Equals(
                                        value.GetType().Name,
                                        "BuildGridInteractionPreviewController",
                                        StringComparison.Ordinal)
                                    && !string.Equals(
                                        value.GetType().Name,
                                        "BuildGridInteractionPreviewRuntime",
                                        StringComparison.Ordinal))),
                    "EXACT_PREPARE_INPUT_OR_RUNTIME_BOUNDARY_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void ValidateBackgroundPrefabAgainstSource(
            GameObject sourceRoot)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(
                GameBackgroundPrefabPath);
            Require(prefab != null,
                "EXACT_GAME_BACKGROUND_PREFAB_LOAD_FAILED");
            try
            {
                RectTransform left = (RectTransform)sourceRoot.transform;
                RectTransform right = (RectTransform)prefab.transform;
                Image sourceImage = sourceRoot.GetComponent<Image>();
                Image prefabImage = prefab.GetComponent<Image>();
                Require(right.anchorMin == left.anchorMin
                        && right.anchorMax == left.anchorMax
                        && right.anchoredPosition == left.anchoredPosition
                        && right.sizeDelta == left.sizeDelta
                        && right.pivot == left.pivot
                        && right.localScale == left.localScale
                        && prefabImage != null
                        && prefabImage.sprite == sourceImage.sprite
                        && prefabImage.color == sourceImage.color
                        && prefabImage.type == sourceImage.type
                        && prefabImage.preserveAspect ==
                        sourceImage.preserveAspect
                        && !prefabImage.raycastTarget,
                    "EXACT_GAME_BACKGROUND_PREFAB_PARITY_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void ValidatePrefabAgainstSource(GameObject sourceRoot)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(
                NavigationPrefabPath);
            Require(prefab != null,
                "EXACT_NAVIGATION_PREFAB_LOAD_FAILED");
            try
            {
                ValidateVisualHierarchy(
                    sourceRoot.transform,
                    prefab.transform,
                    true);
                C1ExactBattleSandboxNavigationBarView view =
                    prefab.GetComponent<
                        C1ExactBattleSandboxNavigationBarView>();
                C1ExactBattleSandboxNavigationPresenter presenter =
                    prefab.GetComponent<
                        C1ExactBattleSandboxNavigationPresenter>();
                Require(view != null && presenter != null
                        && view.ValidateAuthoredReferences()
                        && presenter.ValidateAuthoredReferences()
                        && string.Equals(
                            view.BattleLogLabelText,
                            "战斗日志",
                            StringComparison.Ordinal)
                        && !string.Equals(
                            view.SpeedLabelText,
                            view.BattleLogLabelText,
                            StringComparison.Ordinal),
                    "EXACT_NAVIGATION_PREFAB_RUNTIME_REFERENCES_INVALID");
                Require(view.BackButton.enabled
                        && view.PrimaryButton.enabled
                        && view.ArrangementButton.enabled
                        && view.SpeedButton.enabled
                        && view.BattleLogButton.enabled,
                    "EXACT_NAVIGATION_PREFAB_CONTROL_DISABLED");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void ValidateVisualHierarchy(
            Transform source,
            Transform promoted,
            bool assetRoot)
        {
            Require(source != null && promoted != null
                    && (string.Equals(source.name, promoted.name,
                            StringComparison.Ordinal)
                        || (assetRoot && string.Equals(
                            promoted.name,
                            Path.GetFileNameWithoutExtension(
                                NavigationPrefabPath),
                            StringComparison.Ordinal)))
                    && source.gameObject.activeSelf
                        == promoted.gameObject.activeSelf
                    && source.gameObject.layer == promoted.gameObject.layer
                    && source.childCount == promoted.childCount,
                "EXACT_NAVIGATION_VISUAL_HIERARCHY_MISMATCH "
                + (source == null ? "null" : source.name));

            RectTransform sourceRect = source as RectTransform;
            RectTransform promotedRect = promoted as RectTransform;
            Require(sourceRect != null && promotedRect != null
                    && sourceRect.anchorMin == promotedRect.anchorMin
                    && sourceRect.anchorMax == promotedRect.anchorMax
                    && sourceRect.anchoredPosition
                        == promotedRect.anchoredPosition
                    && sourceRect.sizeDelta == promotedRect.sizeDelta
                    && sourceRect.pivot == promotedRect.pivot
                    && sourceRect.localScale == promotedRect.localScale
                    && sourceRect.localRotation == promotedRect.localRotation,
                "EXACT_NAVIGATION_RECT_MISMATCH " + source.name);

            CompareImage(source, promoted);
            CompareText(source, promoted);
            CompareButton(source, promoted);
            CompareShadow(source, promoted);
            CanvasRenderer sourceRenderer = source.GetComponent<CanvasRenderer>();
            CanvasRenderer promotedRenderer =
                promoted.GetComponent<CanvasRenderer>();
            Require((sourceRenderer == null) == (promotedRenderer == null)
                    && (sourceRenderer == null
                        || sourceRenderer.cullTransparentMesh
                        == promotedRenderer.cullTransparentMesh),
                "EXACT_NAVIGATION_CANVAS_RENDERER_MISMATCH " + source.name);

            for (int index = 0; index < source.childCount; index++)
            {
                ValidateVisualHierarchy(
                    source.GetChild(index),
                    promoted.GetChild(index),
                    false);
            }
        }

        private static void CompareImage(Transform source, Transform promoted)
        {
            Image left = source.GetComponent<Image>();
            Image right = promoted.GetComponent<Image>();
            Require((left == null) == (right == null),
                "EXACT_NAVIGATION_IMAGE_COUNT_MISMATCH " + source.name);
            if (left == null)
            {
                return;
            }

            Require(left.enabled == right.enabled
                    && left.sprite == right.sprite
                    && left.color == right.color
                    && left.raycastTarget == right.raycastTarget
                    && left.type == right.type
                    && left.preserveAspect == right.preserveAspect
                    && left.fillMethod == right.fillMethod
                    && left.fillAmount.Equals(right.fillAmount)
                    && left.fillClockwise == right.fillClockwise
                    && left.fillOrigin == right.fillOrigin,
                "EXACT_NAVIGATION_IMAGE_STYLE_MISMATCH " + source.name);
        }

        private static void CompareText(Transform source, Transform promoted)
        {
            Text left = source.GetComponent<Text>();
            Text right = promoted.GetComponent<Text>();
            Require((left == null) == (right == null),
                "EXACT_NAVIGATION_TEXT_COUNT_MISMATCH " + source.name);
            if (left == null)
            {
                return;
            }

            bool battleLogLabelException = source.parent != null
                                           && promoted.parent != null
                                           && string.Equals(
                                               source.parent.name,
                                               "V04BattlePreparezhandourizhi",
                                               StringComparison.Ordinal)
                                           && string.Equals(
                                               promoted.parent.name,
                                               source.parent.name,
                                               StringComparison.Ordinal)
                                           && string.Equals(
                                               left.text,
                                               "加速",
                                               StringComparison.Ordinal)
                                           && string.Equals(
                                               right.text,
                                               "战斗日志",
                                               StringComparison.Ordinal);
            Require(left.enabled == right.enabled
                    && (battleLogLabelException
                        || string.Equals(left.text, right.text,
                            StringComparison.Ordinal))
                    && left.font == right.font
                    && left.fontSize == right.fontSize
                    && left.fontStyle == right.fontStyle
                    && left.alignment == right.alignment
                    && left.color == right.color
                    && left.raycastTarget == right.raycastTarget
                    && left.horizontalOverflow == right.horizontalOverflow
                    && left.verticalOverflow == right.verticalOverflow
                    && left.lineSpacing.Equals(right.lineSpacing),
                "EXACT_NAVIGATION_TEXT_STYLE_MISMATCH " + source.name);
        }

        private static void CompareButton(Transform source, Transform promoted)
        {
            Button left = source.GetComponent<Button>();
            Button right = promoted.GetComponent<Button>();
            Require((left == null) == (right == null),
                "EXACT_NAVIGATION_BUTTON_COUNT_MISMATCH " + source.name);
            if (left == null)
            {
                return;
            }

            bool formalFunctionalEnableException =
                !left.enabled
                && right.enabled
                && (string.Equals(
                        source.name,
                        "V04BattlePreparejiasu",
                        StringComparison.Ordinal)
                    || string.Equals(
                        source.name,
                        "V04BattlePreparezhandourizhi",
                        StringComparison.Ordinal));
            Require((left.enabled == right.enabled
                        || formalFunctionalEnableException)
                    && left.interactable == right.interactable
                    && left.transition == right.transition
                    && left.colors.Equals(right.colors)
                    && left.spriteState.Equals(right.spriteState)
                    && left.navigation.mode == right.navigation.mode
                    && left.navigation.wrapAround
                        == right.navigation.wrapAround
                    && (left.targetGraphic == null)
                        == (right.targetGraphic == null)
                    && left.onClick.GetPersistentEventCount()
                        == right.onClick.GetPersistentEventCount(),
                "EXACT_NAVIGATION_BUTTON_STYLE_MISMATCH " + source.name);
        }

        private static void CompareShadow(Transform source, Transform promoted)
        {
            Shadow left = source.GetComponent<Shadow>();
            Shadow right = promoted.GetComponent<Shadow>();
            Require((left == null) == (right == null),
                "EXACT_NAVIGATION_SHADOW_COUNT_MISMATCH " + source.name);
            if (left == null)
            {
                return;
            }

            Require(left.enabled == right.enabled
                    && left.effectColor == right.effectColor
                    && left.effectDistance == right.effectDistance
                    && left.useGraphicAlpha == right.useGraphicAlpha,
                "EXACT_NAVIGATION_SHADOW_STYLE_MISMATCH " + source.name);
        }

        private static void ValidateFinalUnifiedScene(Scene scene)
        {
            UnifiedBattlePageShell shell =
                SingleInScene<UnifiedBattlePageShell>(scene);
            UnifiedBattleFormalSceneHost host =
                SingleInScene<UnifiedBattleFormalSceneHost>(scene);
            C1ExactBattleSandboxNavigationPresenter presenter =
                SingleInScene<
                    C1ExactBattleSandboxNavigationPresenter>(scene);
            C1ExactBattleSandboxPrepareSurfacePresenter preparePresenter =
                SingleInScene<
                    C1ExactBattleSandboxPrepareSurfacePresenter>(scene);
            C1ExactBattleSandboxItemArrangementPresenter itemPresenter =
                SingleInScene<
                    C1ExactBattleSandboxItemArrangementPresenter>(scene);
            C1ExactBattleSandboxItemBoardView boardView =
                SingleInScene<C1ExactBattleSandboxItemBoardView>(scene);
            C1ExactBattleSandboxItemTrayView trayView =
                SingleInScene<C1ExactBattleSandboxItemTrayView>(scene);
            Image background = SingleExactPrefabImageInScene(
                scene,
                GameBackgroundPrefabPath);
            ValidateHostItemReferenceInheritance(
                host,
                itemPresenter,
                boardView,
                trayView);
            ValidateRequiredHostReferences(host);
            Require(presenter.transform.parent == shell.transform
                    && string.Equals(
                        PrefabUtility
                            .GetPrefabAssetPathOfNearestInstanceRoot(
                                presenter.gameObject),
                        NavigationPrefabPath,
                        StringComparison.Ordinal),
                "UNIFIED_EXACT_NAVIGATION_SCENE_SOURCE_INVALID");
            Require(host.NavigationPresenter == presenter,
                "UNIFIED_EXACT_NAVIGATION_HOST_REFERENCE_MISMATCH");
            Require(preparePresenter.transform.parent == shell.transform
                    && string.Equals(
                        PrefabUtility
                            .GetPrefabAssetPathOfNearestInstanceRoot(
                                preparePresenter.gameObject),
                        PrepareSurfacePrefabPath,
                        StringComparison.Ordinal)
                    && background.transform.parent == shell.transform
                    && host.PrepareSurfacePresenter == preparePresenter
                    && host.GameBackgroundImage == background
                    && itemPresenter.transform == preparePresenter.transform
                    && boardView.transform.IsChildOf(
                        preparePresenter.transform)
                    && trayView.transform.IsChildOf(
                        preparePresenter.transform)
                    && string.Equals(
                        PrefabUtility
                            .GetPrefabAssetPathOfNearestInstanceRoot(
                                boardView.gameObject),
                        ExactBoardPrefabPath,
                        StringComparison.Ordinal)
                    && string.Equals(
                        PrefabUtility
                            .GetPrefabAssetPathOfNearestInstanceRoot(
                                trayView.gameObject),
                        ExactTrayPrefabPath,
                        StringComparison.Ordinal)
                    && boardView.GetComponent<Graphic>() != null
                    && boardView.GetComponent<Graphic>().raycastTarget
                    && !preparePresenter.View.InputGate.blocksRaycasts
                    && !preparePresenter.View.InputGate.interactable,
                "UNIFIED_EXACT_PREPARE_OR_BACKGROUND_SCENE_INVALID");
            Require(host.ValidateAuthoredBindings(out string diagnostic),
                diagnostic);

            Transform boardSlot = RequiredSlot(
                shell,
                UnifiedBattlePageShellSlotNames.BoardArea);
            Transform traySlot = RequiredSlot(
                shell,
                UnifiedBattlePageShellSlotNames.ItemTrayArea);
            Require(!boardSlot.gameObject.activeSelf
                    && !traySlot.gameObject.activeSelf
                    && boardSlot.GetComponentsInChildren<Graphic>(true)
                        .All(value => !value.raycastTarget)
                    && traySlot.GetComponentsInChildren<Graphic>(true)
                        .All(value => !value.raycastTarget),
                "UNIFIED_APPROXIMATE_ITEM_SURFACE_NOT_RETIRED");

            Transform actionSlot = RequiredSlot(
                shell,
                UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot);
            Button retired = actionSlot.GetComponent<Button>();
            Transform retiredTitle = DirectChild(actionSlot, "Title");
            Require(retired != null && !retired.enabled
                    && !retired.interactable
                    && (retiredTitle == null
                        || !retiredTitle.gameObject.activeSelf),
                "UNIFIED_LEGACY_ACTION_SURFACE_VISIBLE");

            MonoBehaviour[] all = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<
                    MonoBehaviour>(true))
                .ToArray();
            Require(all.Count(value => value != null
                        && string.Equals(
                            value.GetType().Name,
                            "C1ExactBattleSandboxNavigationPresenter",
                            StringComparison.Ordinal)) == 1,
                "UNIFIED_EXACT_NAVIGATION_PRESENTER_COUNT_INVALID");
            Require(all.All(value => value == null
                    || (!string.Equals(value.GetType().Name,
                            "BuildGridInteractionPreviewController",
                            StringComparison.Ordinal)
                        && !string.Equals(value.GetType().Name,
                            "BuildGridInteractionPreviewRuntime",
                            StringComparison.Ordinal)
                        && !string.Equals(value.GetType().Name,
                            "AutoCombatController",
                            StringComparison.Ordinal))),
                "UNIFIED_SANDBOX_RUNTIME_OWNER_IMPORTED");
            Require(all.Count(value => value is
                        C1ExactBattleSandboxPrepareSurfacePresenter) == 1
                    && all.Count(value => value is
                        C1ExactBattleSandboxItemArrangementPresenter) == 1
                    && scene.GetRootGameObjects()
                        .SelectMany(root => root.GetComponentsInChildren<Image>(
                            true))
                        .Count(value => string.Equals(
                            PrefabUtility
                                .GetPrefabAssetPathOfNearestInstanceRoot(
                                    value.gameObject),
                            GameBackgroundPrefabPath,
                            StringComparison.Ordinal)) == 1,
                "UNIFIED_EXACT_CARRIER_COUNT_INVALID");
            ValidateNoMissingScripts(scene);
        }

        private static void ValidateRuntimeSourceBoundaries()
        {
            foreach (string path in RuntimeSourcePaths)
            {
                Require(File.Exists(path),
                    "EXACT_NAVIGATION_RUNTIME_SOURCE_MISSING " + path);
                string source = File.ReadAllText(path);
                foreach (string forbidden in ForbiddenRuntimeTokens)
                {
                    Require(source.IndexOf(forbidden,
                                StringComparison.Ordinal) < 0,
                        "EXACT_NAVIGATION_FORBIDDEN_RUNTIME_TOKEN "
                        + forbidden + " path=" + path);
                }
            }

            string host = File.ReadAllText(RuntimeSourcePaths[0]);
            Require(host.IndexOf("TalismanSceneNavigationOwner.TryNavigate",
                        StringComparison.Ordinal) >= 0
                    && host.IndexOf("Time.unscaledDeltaTime * 1000d * playbackRate",
                        StringComparison.Ordinal) >= 0
                    && host.IndexOf("CurrentRealtimeStart",
                        StringComparison.Ordinal) >= 0
                    && host.IndexOf("initialCues",
                        StringComparison.Ordinal) >= 0
                    && host.IndexOf("emittedCues",
                        StringComparison.Ordinal) >= 0,
                "EXACT_NAVIGATION_FORMAL_SEAM_MISSING");
        }

        private static void ValidateProtectedHashes()
        {
            foreach (KeyValuePair<string, string> row in ProtectedHashes)
            {
                Require(File.Exists(row.Key),
                    "PROTECTED_PATH_MISSING " + row.Key);
                string actual = ComputeSha256(row.Key);
                Require(string.Equals(actual, row.Value,
                        StringComparison.Ordinal),
                    "PROTECTED_HASH_CHANGED path=" + row.Key
                    + " actual=" + actual);
            }
        }

        private static void ValidatePreservedCarrierHashes()
        {
            foreach (KeyValuePair<string, string> row in
                     PreservedCarrierHashes)
            {
                Require(File.Exists(row.Key),
                    "PRESERVED_CARRIER_MISSING " + row.Key);
                string actual = ComputeSha256(row.Key);
                Require(string.Equals(actual, row.Value,
                        StringComparison.Ordinal),
                    "PRESERVED_CARRIER_HASH_CHANGED path=" + row.Key
                    + " actual=" + actual);
            }
        }

        private static void ValidateSourceBoardInputBaseline()
        {
            GameObject source = PrefabUtility.LoadPrefabContents(
                ExactBoardPrefabPath);
            Require(source != null, "EXACT_BOARD_SOURCE_PREFAB_LOAD_FAILED");
            try
            {
                Graphic sourceGraphic = source.GetComponent<Graphic>();
                Require(sourceGraphic != null
                        && !sourceGraphic.raycastTarget,
                    "EXACT_BOARD_SOURCE_RAYCAST_BASELINE_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        private static void ValidateNestedBoardRaycastOverride(
            C1ExactBattleSandboxItemBoardView board)
        {
            Graphic boardGraphic = board.GetComponent<Graphic>();
            Require(boardGraphic != null && boardGraphic.raycastTarget,
                "EXACT_PREPARE_BOARD_RAYCAST_OVERRIDE_MISSING");
            Object sourceGraphic =
                PrefabUtility.GetCorrespondingObjectFromSource(boardGraphic);
            Require(sourceGraphic != null,
                "EXACT_PREPARE_BOARD_GRAPHIC_SOURCE_MISSING");
            PropertyModification[] raycastOverrides =
                (PrefabUtility.GetPropertyModifications(board.gameObject)
                    ?? Array.Empty<PropertyModification>())
                .Where(value => value != null
                    && string.Equals(
                        value.propertyPath,
                        "m_RaycastTarget",
                        StringComparison.Ordinal))
                .ToArray();
            Require(raycastOverrides.Length == 1
                    && raycastOverrides[0].target == sourceGraphic
                    && string.Equals(
                        raycastOverrides[0].value,
                        "1",
                        StringComparison.Ordinal),
                "EXACT_PREPARE_BOARD_RAYCAST_OVERRIDE_NOT_LOCAL");
        }

        private static string ComputeSha256(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha256 = SHA256.Create();
            return string.Concat(sha256.ComputeHash(stream)
                .Select(value => value.ToString("X2")));
        }

        private static void DestroyDirectChild(Transform parent, string name)
        {
            Transform child = DirectChild(parent, name);
            Require(child != null,
                "EXACT_PREPARE_SOURCE_CHILD_MISSING " + name);
            Object.DestroyImmediate(child.gameObject);
        }

        private static void DisableRaycastsInSubtree(Transform root)
        {
            foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(
                         true))
            {
                graphic.raycastTarget = false;
            }
        }

        private static void RemoveRetiredSceneAddedItemPresenter(
            Scene scene,
            UnifiedBattlePageShell shell,
            C1ExactBattleSandboxItemArrangementPresenter acceptedPresenter)
        {
            C1ExactBattleSandboxItemArrangementPresenter[] retired = scene
                .GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<
                    C1ExactBattleSandboxItemArrangementPresenter>(true))
                .Where(value => value != acceptedPresenter)
                .ToArray();
            Require(retired.Length <= 1,
                "UNIFIED_RETIRED_ITEM_PRESENTER_COUNT_INVALID count="
                + retired.Length);
            if (retired.Length == 0)
            {
                return;
            }

            Transform legacyParent = RequiredSlot(
                shell,
                UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot);
            C1ExactBattleSandboxItemArrangementPresenter value = retired[0];
            Require(value.transform == legacyParent
                    && PrefabUtility.IsAddedComponentOverride(value),
                "UNIFIED_RETIRED_ITEM_PRESENTER_NOT_STRICT_ADDED_OVERRIDE");
            PrefabUtility.RevertAddedComponent(
                value,
                InteractionMode.AutomatedAction);
        }

        private static void RemoveRetiredExactItemInstance(
            Scene scene,
            UnifiedBattlePageShell shell,
            Transform acceptedAncestor,
            Type viewType,
            string expectedPrefabPath,
            string expectedParentSlot)
        {
            Component[] retired = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren(
                    viewType,
                    true).Cast<Component>())
                .Where(value => !value.transform.IsChildOf(acceptedAncestor))
                .ToArray();
            Require(retired.Length <= 1,
                "UNIFIED_RETIRED_EXACT_ITEM_INSTANCE_COUNT_INVALID type="
                + viewType.Name + " count=" + retired.Length);
            Transform expectedParent = RequiredSlot(shell, expectedParentSlot);
            foreach (Component value in retired)
            {
                GameObject instanceRoot =
                    PrefabUtility.GetNearestPrefabInstanceRoot(value.gameObject);
                Require(instanceRoot != null,
                    "UNIFIED_RETIRED_EXACT_ITEM_INSTANCE_ROOT_MISSING "
                    + value.name);
                string path = PrefabUtility
                    .GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
                Require(string.Equals(
                            path,
                            expectedPrefabPath,
                            StringComparison.Ordinal)
                        && instanceRoot.transform.parent == expectedParent
                        && PrefabUtility.IsAddedGameObjectOverride(
                            instanceRoot),
                    "UNIFIED_RETIRED_ITEM_NOT_STRICT_ADDED_OVERRIDE type="
                    + viewType.Name + " path=" + path);
                PrefabUtility.RevertAddedGameObject(
                    instanceRoot,
                    InteractionMode.AutomatedAction);
            }
        }

        private static Image SingleExactPrefabImageInScene(
            Scene scene,
            string prefabPath)
        {
            Image[] values = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Image>(true))
                .Where(value => string.Equals(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                        value.gameObject),
                    prefabPath,
                    StringComparison.Ordinal))
                .ToArray();
            Require(values.Length == 1,
                "UNIFIED_EXACT_PREFAB_IMAGE_COUNT path=" + prefabPath
                + " count=" + values.Length);
            return values[0];
        }

        private static GameObject RequiredNamedChild(
            GameObject root,
            string name)
        {
            Transform[] values = root.GetComponentsInChildren<Transform>(true)
                .Where(value => string.Equals(
                    value.name,
                    name,
                    StringComparison.Ordinal))
                .ToArray();
            Require(values.Length == 1,
                "NAMED_CHILD_COUNT_INVALID name=" + name
                + " count=" + values.Length);
            return values[0].gameObject;
        }

        private static void ValidateRootRectMatchesSource(
            GameObject source,
            GameObject promoted,
            string label)
        {
            RectTransform left = source.transform as RectTransform;
            RectTransform right = promoted.transform as RectTransform;
            Require(left != null && right != null
                    && left.anchorMin == right.anchorMin
                    && left.anchorMax == right.anchorMax
                    && left.anchoredPosition == right.anchoredPosition
                    && left.sizeDelta == right.sizeDelta
                    && left.pivot == right.pivot
                    && left.localScale == right.localScale
                    && left.localRotation == right.localRotation,
                "EXACT_PREPARE_" + label + "_ROOT_GEOMETRY_INVALID");
        }

        private static GameObject RequiredNamedObject(Scene scene, string name)
        {
            GameObject[] matches = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(value => string.Equals(value.name, name,
                    StringComparison.Ordinal))
                .Select(value => value.gameObject)
                .ToArray();
            Require(matches.Length == 1,
                "NAMED_SCENE_OBJECT_COUNT_INVALID name=" + name
                + " count=" + matches.Length);
            return matches[0];
        }

        private static Button RequiredButton(GameObject root, string name)
        {
            Transform[] matches = root.GetComponentsInChildren<Transform>(true)
                .Where(value => string.Equals(value.name, name,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "EXACT_NAVIGATION_BUTTON_OBJECT_COUNT name=" + name
                + " count=" + matches.Length);
            Button button = matches[0].GetComponent<Button>();
            Require(button != null,
                "EXACT_NAVIGATION_BUTTON_COMPONENT_MISSING " + name);
            return button;
        }

        private static Text RequiredLabel(Button button)
        {
            Transform label = DirectChild(button.transform, "Label");
            Text text = label == null ? null : label.GetComponent<Text>();
            Require(text != null,
                "EXACT_NAVIGATION_LABEL_MISSING " + button.name);
            return text;
        }

        private static Transform RequiredSlot(
            UnifiedBattlePageShell shell,
            string name)
        {
            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
            slots.TryGetValue(name, out Transform value);
            Require(value != null,
                "UNIFIED_REQUIRED_SLOT_MISSING " + name);
            return value;
        }

        private static Transform DirectChild(Transform parent, string name)
        {
            if (parent == null)
            {
                return null;
            }

            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (string.Equals(child.name, name, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static T SingleInScene<T>(Scene scene) where T : Component
        {
            T[] values = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
            Require(values.Length == 1,
                "UNIFIED_SCENE_COMPONENT_COUNT type=" + typeof(T).Name
                + " count=" + values.Length);
            return values[0];
        }

        private static void ValidateNoMissingScripts(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "UNIFIED_SCENE_MISSING_SCRIPT " + root.name);
            }
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(diagnostic);
            }
        }

        private sealed class ShellPreservationSnapshot
        {
            private readonly ShellTransformState[] transforms;
            private readonly IReadOnlyDictionary<Component, string>
                componentJson;
            private readonly string shellSerializedSignature;

            private ShellPreservationSnapshot(
                ShellTransformState[] capturedTransforms,
                IReadOnlyDictionary<Component, string> capturedComponentJson,
                string capturedShellSerializedSignature)
            {
                transforms = capturedTransforms;
                componentJson = capturedComponentJson;
                shellSerializedSignature = capturedShellSerializedSignature;
            }

            public static ShellPreservationSnapshot Capture(
                GameObject root,
                UnifiedBattlePageShell shell)
            {
                ShellTransformState[] capturedTransforms = root
                    .GetComponentsInChildren<Transform>(true)
                    .Select(value => new ShellTransformState(value))
                    .ToArray();
                Component[] components = capturedTransforms
                    .SelectMany(value => value.Components)
                    .Where(value => value != null
                        && !(value is Transform)
                        && value != shell)
                    .ToArray();
                Dictionary<Component, string> capturedJson = components
                    .ToDictionary(
                        value => value,
                        value => EditorJsonUtility.ToJson(value, false));
                return new ShellPreservationSnapshot(
                    capturedTransforms,
                    capturedJson,
                    CaptureShellSerializedSignature(shell));
            }

            public void Validate(UnifiedBattlePageShell shell)
            {
                foreach (ShellTransformState state in transforms)
                {
                    state.Validate();
                }

                foreach (KeyValuePair<Component, string> row in componentJson)
                {
                    Require(row.Key != null
                            && string.Equals(
                                EditorJsonUtility.ToJson(row.Key, false),
                                row.Value,
                                StringComparison.Ordinal),
                        "SHELL_REQUIRED_INERT_SLOT_UNRELATED_COMPONENT_DRIFT type="
                        + (row.Key == null
                            ? "<destroyed>"
                            : row.Key.GetType().FullName));
                }

                Require(string.Equals(
                        CaptureShellSerializedSignature(shell),
                        shellSerializedSignature,
                        StringComparison.Ordinal),
                    "SHELL_REQUIRED_INERT_SLOT_UNRELATED_SHELL_FIELD_DRIFT");
            }

            private static string CaptureShellSerializedSignature(
                UnifiedBattlePageShell shell)
            {
                SerializedObject serializedShell = new SerializedObject(shell);
                serializedShell.Update();
                SerializedProperty iterator = serializedShell.GetIterator();
                List<string> rows = new List<string>();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (string.Equals(
                            iterator.propertyPath,
                            "boardArea",
                            StringComparison.Ordinal)
                        || string.Equals(
                            iterator.propertyPath,
                            "itemTrayArea",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    rows.Add(iterator.propertyPath + "|"
                             + iterator.propertyType + "|"
                             + SerializedPropertyToken(iterator));
                }

                return string.Join("\n", rows);
            }

            private static string SerializedPropertyToken(
                SerializedProperty property)
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        return property.longValue.ToString();
                    case SerializedPropertyType.Boolean:
                        return property.boolValue ? "1" : "0";
                    case SerializedPropertyType.Float:
                        return property.doubleValue.ToString("R");
                    case SerializedPropertyType.String:
                        return property.stringValue ?? string.Empty;
                    case SerializedPropertyType.ObjectReference:
                        return SceneOverrideSnapshot.DescribeObjectIdentity(
                            property.objectReferenceValue);
                    case SerializedPropertyType.Enum:
                        return property.enumValueIndex.ToString();
                    default:
                        return property.type;
                }
            }
        }

        private sealed class ShellTransformState
        {
            private readonly Transform transform;
            private readonly Transform parent;
            private readonly int siblingIndex;
            private readonly string name;
            private readonly bool activeSelf;
            private readonly int layer;
            private readonly string tag;
            private readonly HideFlags hideFlags;
            private readonly Vector3 localPosition;
            private readonly Quaternion localRotation;
            private readonly Vector3 localScale;
            private readonly bool isRectTransform;
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 pivot;
            private readonly Vector2 offsetMin;
            private readonly Vector2 offsetMax;

            public ShellTransformState(Transform captured)
            {
                transform = captured;
                parent = captured.parent;
                siblingIndex = captured.GetSiblingIndex();
                name = captured.name;
                activeSelf = captured.gameObject.activeSelf;
                layer = captured.gameObject.layer;
                tag = captured.gameObject.tag;
                hideFlags = captured.gameObject.hideFlags;
                localPosition = captured.localPosition;
                localRotation = captured.localRotation;
                localScale = captured.localScale;
                Components = captured.gameObject.GetComponents<Component>();

                RectTransform rect = captured as RectTransform;
                isRectTransform = rect != null;
                if (rect != null)
                {
                    anchorMin = rect.anchorMin;
                    anchorMax = rect.anchorMax;
                    anchoredPosition = rect.anchoredPosition;
                    sizeDelta = rect.sizeDelta;
                    pivot = rect.pivot;
                    offsetMin = rect.offsetMin;
                    offsetMax = rect.offsetMax;
                }
            }

            public Component[] Components { get; }

            public void Validate()
            {
                RectTransform rect = transform as RectTransform;
                Require(transform != null
                        && transform.parent == parent
                        && transform.GetSiblingIndex() == siblingIndex
                        && string.Equals(
                            transform.name,
                            name,
                            StringComparison.Ordinal)
                        && transform.gameObject.activeSelf == activeSelf
                        && transform.gameObject.layer == layer
                        && string.Equals(
                            transform.gameObject.tag,
                            tag,
                            StringComparison.Ordinal)
                        && transform.gameObject.hideFlags == hideFlags
                        && transform.localPosition == localPosition
                        && transform.localRotation == localRotation
                        && transform.localScale == localScale
                        && (rect != null) == isRectTransform
                        && Components.SequenceEqual(
                            transform.gameObject.GetComponents<Component>())
                        && (!isRectTransform
                            || (rect.anchorMin == anchorMin
                                && rect.anchorMax == anchorMax
                                && rect.anchoredPosition == anchoredPosition
                                && rect.sizeDelta == sizeDelta
                                && rect.pivot == pivot
                                && rect.offsetMin == offsetMin
                                && rect.offsetMax == offsetMax)),
                    "SHELL_REQUIRED_INERT_SLOT_UNRELATED_OBJECT_DRIFT name="
                    + name);
            }
        }

        private sealed class MissingRequiredHostReference
        {
            public MissingRequiredHostReference(
                string propertyPath,
                Object acceptedReference,
                GameObject acceptedCarrierGameObject)
            {
                PropertyPath = propertyPath;
                AcceptedReference = acceptedReference;
                AcceptedCarrierGameObject = acceptedCarrierGameObject;
            }

            public string PropertyPath { get; }
            public Object AcceptedReference { get; }
            public GameObject AcceptedCarrierGameObject { get; }
        }

        private sealed class HostReferenceRepairResult
        {
            public List<string> RevertedPropertyModificationSignatures
            {
                get;
            } = new List<string>();

            public string RevertedRemovedGameObjectSignature { get; set; }
        }

        private sealed class SceneOverrideSnapshot
        {
            private readonly string[] propertyModifications;
            private readonly string[] removedGameObjects;

            private SceneOverrideSnapshot(
                string[] capturedPropertyModifications,
                string[] capturedRemovedGameObjects)
            {
                propertyModifications = capturedPropertyModifications;
                removedGameObjects = capturedRemovedGameObjects;
            }

            public static SceneOverrideSnapshot Capture(
                GameObject shellInstanceRoot)
            {
                string[] capturedProperties =
                    (PrefabUtility.GetPropertyModifications(shellInstanceRoot)
                        ?? Array.Empty<PropertyModification>())
                    .Where(value => value != null)
                    .Select(DescribePropertyModification)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                string[] capturedRemoved =
                    (PrefabUtility.GetRemovedGameObjects(shellInstanceRoot)
                        ?? new List<RemovedGameObject>())
                    .Where(value => value != null)
                    .Select(DescribeRemovedGameObject)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                return new SceneOverrideSnapshot(
                    capturedProperties,
                    capturedRemoved);
            }

            public void ValidateAfter(
                GameObject shellInstanceRoot,
                IReadOnlyCollection<string>
                    revertedPropertyModificationSignatures,
                string revertedRemovedGameObjectSignature)
            {
                HashSet<string> allowedPropertyRemovals =
                    new HashSet<string>(
                        revertedPropertyModificationSignatures,
                        StringComparer.Ordinal);
                string[] expectedProperties = propertyModifications
                    .Where(value => !allowedPropertyRemovals.Contains(value))
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                string[] actualProperties =
                    (PrefabUtility.GetPropertyModifications(shellInstanceRoot)
                        ?? Array.Empty<PropertyModification>())
                    .Where(value => value != null)
                    .Select(DescribePropertyModification)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                Require(expectedProperties.SequenceEqual(actualProperties),
                    "UNIFIED_REQUIRED_HOST_UNRELATED_PROPERTY_OVERRIDE_DRIFT"
                    + " before=" + expectedProperties.Length
                    + " after=" + actualProperties.Length);

                string[] expectedRemoved = removedGameObjects
                    .Where(value => !string.Equals(
                        value,
                        revertedRemovedGameObjectSignature,
                        StringComparison.Ordinal))
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                string[] actualRemoved =
                    (PrefabUtility.GetRemovedGameObjects(shellInstanceRoot)
                        ?? new List<RemovedGameObject>())
                    .Where(value => value != null)
                    .Select(DescribeRemovedGameObject)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
                Require(expectedRemoved.SequenceEqual(actualRemoved),
                    "UNIFIED_REQUIRED_HOST_UNRELATED_REMOVED_OVERRIDE_DRIFT"
                    + " before=" + expectedRemoved.Length
                    + " after=" + actualRemoved.Length);
            }

            public static string DescribePropertyModification(
                PropertyModification modification)
            {
                return DescribeObjectIdentity(modification.target)
                       + "|" + modification.propertyPath
                       + "|" + modification.value
                       + "|" + DescribeObjectIdentity(
                           modification.objectReference);
            }

            public static string DescribeRemovedGameObject(
                RemovedGameObject removed)
            {
                return DescribeObjectIdentity(removed.assetGameObject)
                       + "|parent=" + DescribeObjectIdentity(
                           removed.parentOfRemovedGameObjectInInstance);
            }

            public static string DescribeObjectIdentity(Object value)
            {
                if (value == null)
                {
                    return "<null>";
                }

                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                        value,
                        out string guid,
                        out long localId))
                {
                    return guid + ":" + localId;
                }

                return "instance:" + value.GetInstanceID()
                       + ":" + value.GetType().FullName
                       + ":" + value.name;
            }
        }

        private sealed class ExactItemLayoutBaseline
        {
            private readonly RectSnapshot board;
            private readonly RectSnapshot tray;

            private ExactItemLayoutBaseline(
                RectSnapshot boardSnapshot,
                RectSnapshot traySnapshot)
            {
                board = boardSnapshot;
                tray = traySnapshot;
            }

            public static ExactItemLayoutBaseline Capture(Scene scene)
            {
                return new ExactItemLayoutBaseline(
                    RectSnapshot.Capture(SingleInScene<
                        C1ExactBattleSandboxItemBoardView>(scene).transform),
                    RectSnapshot.Capture(SingleInScene<
                        C1ExactBattleSandboxItemTrayView>(scene).transform));
            }

            public void Validate(Scene scene)
            {
                board.Validate(SingleInScene<
                    C1ExactBattleSandboxItemBoardView>(scene).transform);
                tray.Validate(SingleInScene<
                    C1ExactBattleSandboxItemTrayView>(scene).transform);
            }
        }

        private sealed class RectSnapshot
        {
            private readonly Vector2 anchorMin;
            private readonly Vector2 anchorMax;
            private readonly Vector2 anchoredPosition;
            private readonly Vector2 sizeDelta;
            private readonly Vector2 pivot;
            private readonly Vector3 scale;
            private readonly Quaternion rotation;
            private readonly int siblingIndex;

            private RectSnapshot(RectTransform rect)
            {
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                anchoredPosition = rect.anchoredPosition;
                sizeDelta = rect.sizeDelta;
                pivot = rect.pivot;
                scale = rect.localScale;
                rotation = rect.localRotation;
                siblingIndex = rect.GetSiblingIndex();
            }

            public static RectSnapshot Capture(Transform transform)
            {
                RectTransform rect = transform as RectTransform;
                Require(rect != null,
                    "UNIFIED_EXACT_ITEM_RECT_MISSING " + transform.name);
                return new RectSnapshot(rect);
            }

            public void Validate(Transform transform)
            {
                RectTransform rect = transform as RectTransform;
                Require(rect != null
                        && anchorMin == rect.anchorMin
                        && anchorMax == rect.anchorMax
                        && anchoredPosition == rect.anchoredPosition
                        && sizeDelta == rect.sizeDelta
                        && pivot == rect.pivot
                        && scale == rect.localScale
                        && rotation == rect.localRotation
                        && siblingIndex == rect.GetSiblingIndex(),
                    "UNIFIED_EXACT_ITEM_LAYOUT_CHANGED " + transform.name);
            }
        }
    }
}
