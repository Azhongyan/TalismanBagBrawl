using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using TalismanBag.BattleBridge.Formal;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime.BoneSwapRemnant;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Presentation.FormalBattle;
using TalismanBag.UnifiedBattle;
using TalismanBag.V04.Campaign.Chapter1;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class FormalBattlePresentationPrefabAuthoring
    {
        public const string TerminalMarker =
            "FORMAL_BATTLE_PRESENTATION_PREFABS_AUTHORED_PASS";
        public const string AudioLightFixTerminalMarker =
            "FORMAL_BATTLE_PRESENTATION_AUDIO_LIGHT_FIX_AUTHORED_PASS";
        public const string CausalVisualTerminalMarker =
            "FORMAL_BATTLE_CAUSAL_VISUALS_AUTHORED_PASS";
        public const string SourceCarrierCorrectionTerminalMarker =
            "FORMAL_BATTLE_PRESENTATION_SOURCE_CARRIER_REV01_AUTHORED_PASS";
        public const string DynamicGrammarProfileTerminalMarker =
            "FORMAL_BATTLE_DYNAMIC_CAUSAL_GRAMMAR_PROFILE_AUTHORED_PASS";
        public const string BattleSandboxHudVisualParityTerminalMarker =
            "FORMAL_BATTLE_SANDBOX_HUD_VISUAL_PARITY_AUTHORED_PASS";
        public const string EnemyHudActorStageSeparationTerminalMarker =
            "FORMAL_BATTLE_ENEMY_HUD_ACTOR_STAGE_SEPARATION_AUTHORED_PASS";
        public const string SelectedEnemyShellIconTerminalMarker =
            "FORMAL_BATTLE_SELECTED_ENEMY_SHELL_ICON_AUTHORED_PASS";
        public const string BattleHudStatusVisualTerminalMarker =
            "FORMAL_BATTLE_HUD_STATUS_VISUALS_AUTHORED_PASS";
        public const string PinnedPositiveStateAreaTerminalMarker =
            "FORMAL_BATTLE_PINNED_POSITIVE_STATE_AREA_AUTHORED_PASS";
        public const string SelectedEnemyHudPrefabTerminalMarker =
            "FORMAL_BATTLE_SELECTED_ENEMY_HUD_PREFAB_EXTRACTED_PASS";
        public const string StatusIconPaletteTerminalMarker =
            "FORMAL_BATTLE_STATUS_ICON_PALETTE_AUTHORED_PASS";
        public const string StatusSlotPrefabTerminalMarker =
            "FORMAL_BATTLE_STATUS_SLOT_PREFABIZED_PASS";
        public const string StatusStripCompactAlignmentTerminalMarker =
            "FORMAL_BATTLE_STATUS_STRIP_COMPACT_ALIGNMENT_PASS";
        public const string ItemDetailPlayerReadabilityTerminalMarker =
            "FORMAL_BATTLE_ITEM_DETAIL_PLAYER_READABILITY_AUTHORED_PASS";
        private const string UnifiedPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab";
        private const string PresentationPrefabFolder =
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation";
        private const string ProfileFolder =
            "Assets/_Game/Resources/V04/FormalBattlePresentation";
        private const string AudioFolder = ProfileFolder + "/Audio";
        private const string PlayerPrefabPath = PresentationPrefabFolder
            + "/FormalBattlePlayerPresentation.prefab";
        private const string EnemyPrefabPath = PresentationPrefabFolder
            + "/FormalBattleEnemySlot.prefab";
        private const string DamageFloatPrefabPath = PresentationPrefabFolder
            + "/FormalBattleDamageFloatPool.prefab";
        private const string CueFxPrefabPath = PresentationPrefabFolder
            + "/FormalBattleCueFxAudioRoot.prefab";
        private const string PresentationRootPrefabPath =
            PresentationPrefabFolder + "/FormalBattlePresentationRoot.prefab";
        private const string SelectedEnemyHudPrefabPath =
            PresentationPrefabFolder + "/FormalBattleSelectedEnemyHud.prefab";
        private const string StatusSlotPrefabPath =
            PresentationPrefabFolder + "/FormalBattleStatusSlot.prefab";
        private const string ExactSandboxBoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemBoard.prefab";
        private const string ProfilePath = ProfileFolder
            + "/FormalBattlePresentationProfile.asset";
        private const string SourcePulseAudioPath = AudioFolder
            + "/Formal_SourcePulse_V1.wav";
        private const string EnemyAttackAudioPath = AudioFolder
            + "/Formal_EnemyAttack_V1.wav";
        private const string ImpactAudioPath = AudioFolder
            + "/Formal_Impact_V1.wav";
        private const string UnifiedScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity";
        private const string ExactSandboxCardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemCard.prefab";
        private const string ExactSandboxTrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemTray.prefab";
        private const string ExactSandboxCardViewPath =
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1ExactBattleSandboxItemCardView.cs";
        private const string ItemDetailPopupPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemDetailPopupStandalone.prefab";
        private const string ItemDetailLightingLitIconPath =
            "Assets/_Game/Resources/item/\u805a\u5ff5\u77f3icon/"
            + "\u805a\u5ff5\u77f3icon_\u70b9\u4eae.png";
        private const string ItemDetailLightingUnlitIconPath =
            "Assets/_Game/Resources/item/\u805a\u5ff5\u77f3icon/"
            + "\u805a\u5ff5\u77f3icon_\u672a\u70b9\u4eae.png";
        private const string ItemDetailArrayLitIconPath =
            "Assets/_Game/Resources/item/\u9635\u8109icon/"
            + "\u9635\u8109icon_\u70b9\u4eae.png";
        private const string ItemDetailArrayUnlitIconPath =
            "Assets/_Game/Resources/item/\u9635\u8109icon/"
            + "\u9635\u8109icon_\u672a\u70b9\u4eae.png";
        private const string EnemyPortraitFramePath =
            "Assets/_Game/Resources/角色敌人UI/敌人头像框.png";
        private const string HpFillSpritePath =
            "Assets/_Game/Resources/角色敌人UI/Hpbar.png";
        private const string GuardFillSpritePath =
            "Assets/_Game/Resources/角色敌人UI/Manabar.png";

        private const string EnemyPortraitFrameGuid =
            "7a55f0baed59e4d4c9c3d6c859b32fe9";
        private const string HpFillGuid =
            "706a98284ee73d247a86eef84986fef6";
        private const string ManaFillGuid =
            "f21a77a3ab18b3d48a69aabc0f3b1e9b";
        private const string EnemyShellIconPath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/"
            + "EnemyShellIcon_V1.png";
        private const string StatusSlotFramePath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/"
            + "StatusSlotFrame_V1.png";
        private const string StatusAfterglowIconPath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/"
            + "StatusAfterglow_V1.png";
        private const string StatusPollutionIconPath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/"
            + "StatusPollution_V1.png";
        private const string StatusPollutionWarmRedIconPath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/"
            + "StatusPollutionWarmRed_V1.png";
        private const string SelectedEnemyPreviewPortraitPath =
            "Assets/_Game/Resources/\u89d2\u8272\u654c\u4ebaUI/diren_1.png";
        private const string PlayerAvatarGuid =
            "ec9a0d655598fb9489a2e07732aae6f7";
        private const string HpFrameGuid =
            "b93205d89b9e3464c8a9916672246dc9";
        private const string DamageFontGuid =
            "c14729bd87e5e46458b20b395b18160b";
        private const string RestrainedVfxGuid =
            "a5ceadc894804794684b8cf054782df2";

        private static readonly string[] ProfileCoverageProtectedPaths =
        {
            ProfilePath + ".meta",
            UnifiedPrefabPath,
            UnifiedScenePath,
            PlayerPrefabPath,
            EnemyPrefabPath,
            DamageFloatPrefabPath,
            CueFxPrefabPath,
            PresentationRootPrefabPath,
            ExactSandboxBoardPrefabPath,
            ExactSandboxCardPrefabPath,
            ExactSandboxTrayPrefabPath,
            ExactSandboxCardViewPath
        };

        private sealed class ProfilePreservationSnapshot
        {
            public string HostSignature = string.Empty;
            public string HoundSignature = string.Empty;
            public string[] CausalVisualSignatures = Array.Empty<string>();
            public int WindupBits;
            public int SourceHoldBits;
            public int RibbonBits;
        }

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattlePresentationProfile.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattlePlayerPresentationView.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleEnemySlotView.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleSelectedEnemyHudView.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleStatusSlotView.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleStatusStripView.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleDamageFloatPool.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleCueFxAudioRoot.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleCausalSourceVisualView.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattleCausalRibbonGraphic.cs",
            "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/FormalBattlePresentationRoot.cs",
            "Assets/_Game/Scripts/TalismanBag/UnifiedBattle/UnifiedBattleFormalSceneHost.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "GameObject.Find(",
            "FindObjectOfType",
            "Resources.Load",
            "RuntimeInitializeOnLoadMethod",
            "sceneLoaded +=",
            "new GameObject(",
            "AssetDatabase.",
            "EditorSceneManager"
        };

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Author Presentation Prefabs",
            false,
            2460)]
        public static void AuthorFromMenu()
        {
            ApplySingleAuthoringPass();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                ApplySingleAuthoringPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattlePresentationAuthoring] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Author Audio Light Fix",
            false,
            2461)]
        public static void AuthorAudioLightFixFromMenu()
        {
            ApplyAudioLightFix();
        }

        public static void ExecuteAudioLightFixFromCommandLine()
        {
            try
            {
                ApplyAudioLightFix();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattlePresentationAudioLightFix] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Author Causal Visuals",
            false,
            2462)]
        public static void AuthorCausalVisualsFromMenu()
        {
            ApplyCausalVisualAuthoringPass();
        }

        public static void ExecuteCausalVisualsFromCommandLine()
        {
            try
            {
                ApplyCausalVisualAuthoringPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleCausalVisualAuthoring] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Author Source Carrier REV01",
            false,
            2463)]
        public static void AuthorSourceCarrierCorrectionFromMenu()
        {
            ApplySourceCarrierCorrectionPass();
        }

        public static void ExecuteSourceCarrierCorrectionFromCommandLine()
        {
            try
            {
                ApplySourceCarrierCorrectionPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleSourceCarrierREV01] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Author Dynamic Causal Grammar Profile",
            false,
            2464)]
        public static void AuthorDynamicCausalGrammarProfileFromMenu()
        {
            ApplyDynamicCausalGrammarProfilePass();
        }

        public static void ExecuteDynamicCausalGrammarProfileFromCommandLine()
        {
            try
            {
                ApplyDynamicCausalGrammarProfilePass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleDynamicGrammarProfile] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        public static void AuthorBattleSandboxHudVisualParityFromMenu()
        {
            ApplyBattleSandboxHudVisualParityPass();
        }

        public static void ExecuteBattleSandboxHudVisualParityFromCommandLine()
        {
            try
            {
                ApplyBattleSandboxHudVisualParityPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleSandboxHudVisualParity] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Correct Selected Enemy HUD And Actor Stages",
            false,
            2466)]
        public static void CorrectSelectedEnemyHudAndActorStagesFromMenu()
        {
            ApplyEnemyHudActorStageSeparationPass();
        }

        public static void ExecuteEnemyHudActorStageSeparationFromCommandLine()
        {
            try
            {
                ApplyEnemyHudActorStageSeparationPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleEnemyHudActorStageSeparation] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Replace Selected Enemy Shell Bar With Icon",
            false,
            2467)]
        public static void ReplaceSelectedEnemyShellBarWithIconFromMenu()
        {
            ApplySelectedEnemyShellIconPass();
        }

        public static void ExecuteSelectedEnemyShellIconFromCommandLine()
        {
            try
            {
                ApplySelectedEnemyShellIconPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleSelectedEnemyShellIcon] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Apply HUD Status Visuals",
            false,
            2468)]
        public static void ApplyBattleHudStatusVisualsFromMenu()
        {
            ApplyBattleHudStatusVisualPass();
        }

        public static void ExecuteBattleHudStatusVisualsFromCommandLine()
        {
            try
            {
                ApplyBattleHudStatusVisualPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleHudStatusVisuals] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Apply Pinned Guard And Shell States",
            false,
            2469)]
        public static void ApplyPinnedPositiveStateAreasFromMenu()
        {
            ApplyPinnedPositiveStateAreaPass();
        }

        public static void ExecutePinnedPositiveStateAreasFromCommandLine()
        {
            try
            {
                ApplyPinnedPositiveStateAreaPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattlePinnedPositiveState] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Extract Selected Enemy HUD Prefab",
            false,
            2470)]
        public static void ExtractSelectedEnemyHudPrefabFromMenu()
        {
            ApplySelectedEnemyHudPrefabExtractionPass();
        }

        public static void ExecuteSelectedEnemyHudPrefabExtractionFromCommandLine()
        {
            try
            {
                ApplySelectedEnemyHudPrefabExtractionPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleSelectedEnemyHudPrefab] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Apply Status Icon Palette",
            false,
            2471)]
        public static void ApplyStatusIconPaletteFromMenu()
        {
            ApplyStatusIconPalettePass();
        }

        public static void ExecuteStatusIconPaletteFromCommandLine()
        {
            try
            {
                ApplyStatusIconPalettePass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleStatusIconPalette] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Prefabize Status Slots",
            false,
            2472)]
        public static void PrefabizeStatusSlotsFromMenu()
        {
            ApplyStatusSlotPrefabizationPass();
        }

        public static void ExecuteStatusSlotPrefabizationFromCommandLine()
        {
            try
            {
                ApplyStatusSlotPrefabizationPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleStatusSlotPrefab] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Apply Compact Status Alignment",
            false,
            2473)]
        public static void ApplyCompactStatusAlignmentFromMenu()
        {
            ApplyStatusStripCompactAlignmentPass();
        }

        public static void ExecuteStatusStripCompactAlignmentFromCommandLine()
        {
            try
            {
                ApplyStatusStripCompactAlignmentPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleStatusStripCompactAlignment] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        [MenuItem(
            "TalismanBag/V0.4/Formal Battle/Apply Item Detail Player Readability",
            false,
            2474)]
        public static void ApplyItemDetailPlayerReadabilityFromMenu()
        {
            ApplyItemDetailPlayerReadabilityPass();
        }

        public static void ExecuteItemDetailPlayerReadabilityFromCommandLine()
        {
            try
            {
                ApplyItemDetailPlayerReadabilityPass();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError(
                    "[FormalBattleItemDetailReadability] FAIL "
                    + exception.Message);
                EditorApplication.Exit(1);
            }
        }

        private static void ApplyItemDetailPlayerReadabilityPass()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                ItemDetailPopupPrefabPath);
            try
            {
                ItemDetailPanelView panel = root.GetComponentInChildren<
                    ItemDetailPanelView>(true);
                Require(panel != null,
                    "ITEM_DETAIL_PLAYER_PANEL_MISSING");
                ConfigureItemDetailPlayerReadability(root, panel);
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    ItemDetailPopupPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateItemDetailPlayerReadability();
            Debug.Log(ItemDetailPlayerReadabilityTerminalMarker
                      + " battleSandboxOrder=1 statusSprites=4"
                      + " factWrites=0 outerLayoutWrites=0");
        }

        private static void ApplyBattleHudStatusVisualPass()
        {
            EnsureUiSpriteImport(StatusSlotFramePath);
            EnsureUiSpriteImport(StatusAfterglowIconPath);
            EnsureUiSpriteImport(StatusPollutionWarmRedIconPath);

            Sprite frame = RequireAssetAtPath<Sprite>(StatusSlotFramePath);
            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            profile.AssignStatusVisualsForEditor(
                CreateDefaultStatusVisualRows());
            EditorUtility.SetDirty(profile);

            RestyleExistingStatusStripPrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                frame);
            RestyleExistingStatusStripPrefab(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip",
                frame);

            Require(profile.ValidateAuthoredReferences(),
                "FORMAL_HUD_STATUS_PROFILE_INVALID");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(BattleHudStatusVisualTerminalMarker
                      + " playerStrip=1 selectedEnemyStrip=1"
                      + " icons=2 outerRectTransformWrites=0"
                      + " sceneWrites=0 waterWrites=0");
        }

        private static void ApplyPinnedPositiveStateAreaPass()
        {
            EnsureUiSpriteImport(StatusSlotFramePath);
            EnsureUiSpriteImport(EnemyShellIconPath);
            Sprite frame = RequireAssetAtPath<Sprite>(StatusSlotFramePath);
            Sprite shield = RequireAssetAtPath<Sprite>(EnemyShellIconPath);

            ConfigurePinnedPositiveStatePrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                "PlayerGuardReadout",
                "PlayerGuardIcon",
                "PlayerGuardLabel",
                frame,
                shield,
                false);
            ConfigurePinnedPositiveStatePrefab(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip",
                "SelectedEnemyShellReadout",
                "SelectedEnemyShellIcon",
                "SelectedEnemyShellLabel",
                frame,
                shield,
                true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidatePinnedPositiveStateAreas();
            Debug.Log(PinnedPositiveStateAreaTerminalMarker
                      + " playerGuard=1 enemyShell=1"
                      + " dynamicBuffSlots=8 waterWrites=0 sceneWrites=0");
        }

        private static void ApplySelectedEnemyHudPrefabExtractionPass()
        {
            Require(AssetDatabase.LoadAssetAtPath<GameObject>(
                        SelectedEnemyHudPrefabPath) == null,
                "FORMAL_SELECTED_ENEMY_HUD_PREFAB_ALREADY_EXISTS_MANUAL_LAYOUT_OWNED");
            EnsureUiSpriteImport(SelectedEnemyPreviewPortraitPath);
            EnsureUiSpriteImport(StatusAfterglowIconPath);
            EnsureUiSpriteImport(StatusPollutionWarmRedIconPath);

            GameObject presentation = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                FormalBattlePresentationRoot presenter = presentation
                    .GetComponent<FormalBattlePresentationRoot>();
                FormalBattleSelectedEnemyHudView embeddedView = presentation
                    .GetComponentsInChildren<
                        FormalBattleSelectedEnemyHudView>(true)
                    .Single();
                Require(presenter != null
                        && embeddedView.ValidateAuthoredReferences(),
                    "FORMAL_SELECTED_ENEMY_HUD_EMBEDDED_SOURCE_INVALID");

                GameObject embedded = embeddedView.gameObject;
                int siblingIndex = embedded.transform.GetSiblingIndex();
                GameObject extracted = UnityEngine.Object.Instantiate(
                    embedded);
                extracted.name = "SelectedEnemyHud";
                extracted.transform.SetParent(null, false);
                try
                {
                    ConfigureSelectedEnemyHudEditPreview(extracted);
                    FormalBattleSelectedEnemyHudView extractedView = extracted
                        .GetComponent<FormalBattleSelectedEnemyHudView>();
                    Require(extractedView != null
                            && extractedView.ValidateAuthoredReferences(),
                        "FORMAL_SELECTED_ENEMY_HUD_EXTRACTED_SOURCE_INVALID");
                    GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                        extracted,
                        SelectedEnemyHudPrefabPath);
                    Require(saved != null,
                        "FORMAL_SELECTED_ENEMY_HUD_PREFAB_SAVE_FAILED");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(extracted);
                }

                GameObject prefab = RequirePrefab(
                    SelectedEnemyHudPrefabPath);
                GameObject nested = (GameObject)PrefabUtility
                    .InstantiatePrefab(prefab, presentation.transform);
                nested.name = "SelectedEnemyHud";
                CopyRectTransform(
                    embedded.transform as RectTransform,
                    nested.transform as RectTransform);
                nested.transform.SetSiblingIndex(siblingIndex);
                FormalBattleSelectedEnemyHudView nestedView = nested
                    .GetComponent<FormalBattleSelectedEnemyHudView>();
                Require(nestedView != null
                        && nestedView.ValidateAuthoredReferences(),
                    "FORMAL_SELECTED_ENEMY_HUD_NESTED_REFERENCE_INVALID");

                UnityEngine.Object.DestroyImmediate(embedded);
                SerializedObject presenterObject = new SerializedObject(
                    presenter);
                SerializedProperty selectedEnemyHudProperty = presenterObject
                    .FindProperty("selectedEnemyHud");
                Require(selectedEnemyHudProperty != null,
                    "FORMAL_SELECTED_ENEMY_HUD_PRESENTER_SLOT_MISSING");
                selectedEnemyHudProperty.objectReferenceValue = nestedView;
                presenterObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(presenter);
                Require(presenter.ValidateSourceCarrierReferences(out _),
                    "FORMAL_SELECTED_ENEMY_HUD_ROOT_BINDING_INVALID");
                PrefabUtility.SaveAsPrefabAsset(
                    presentation,
                    PresentationRootPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(presentation);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateSelectedEnemyHudPrefabExtraction();
            Debug.Log(SelectedEnemyHudPrefabTerminalMarker
                      + " standalonePrefab=1 rootNestedInstance=1"
                      + " editPreview=portrait+hp+shell+8statuses"
                      + " waterWrites=0 sceneWrites=0");
        }

        private static void ApplyStatusIconPalettePass()
        {
            EnsureUiSpriteImport(StatusAfterglowIconPath);
            EnsureUiSpriteImport(StatusPollutionWarmRedIconPath);
            EnsureUiSpriteImport(EnemyShellIconPath);

            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            profile.AssignStatusVisualsForEditor(
                CreateDefaultStatusVisualRows());
            EditorUtility.SetDirty(profile);

            ConfigureStatusIconPalettePrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                false);
            ConfigureStatusIconPalettePrefab(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip",
                true);

            Require(profile.ValidateAuthoredReferences(),
                "FORMAL_STATUS_ICON_PALETTE_PROFILE_INVALID");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateStatusIconPalette();
            Debug.Log(StatusIconPaletteTerminalMarker
                      + " formalStatuses=2 icons=2"
                      + " positivePalette=cold-blue"
                      + " negativePalette=warm-red"
                      + " statusTint=off manualAnchorsPreserved=1"
                      + " selectedEnemyInternalLayoutFromSlot00=1"
                      + " waterWrites=0 sceneWrites=0");
        }

        private static void ApplyStatusSlotPrefabizationPass()
        {
            EnsureUiSpriteImport(EnemyShellIconPath);
            EnsureUiSpriteImport(StatusAfterglowIconPath);
            EnsureUiSpriteImport(StatusPollutionWarmRedIconPath);

            AuthorStatusSlotPrefabFromSelectedEnemySlotZero();
            ConvertStatusStripToNestedPrefabSlots(
                PlayerPrefabPath,
                "PlayerStatusStrip");
            ConvertStatusStripToNestedPrefabSlots(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateStatusSlotPrefabization();
            Debug.Log(StatusSlotPrefabTerminalMarker
                      + " sharedSlotPrefab=1 playerInstances=8"
                      + " selectedEnemyInstances=8"
                      + " slot00VisualInheritance=icon+stack+duration"
                      + " rootPositionOverridesPreserved=1"
                      + " waterWrites=0 sceneWrites=0");
        }

        private static void AuthorStatusSlotPrefabFromSelectedEnemySlotZero()
        {
            GameObject hud = PrefabUtility.LoadPrefabContents(
                SelectedEnemyHudPrefabPath);
            try
            {
                FormalBattleStatusStripView strip = hud
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "SelectedEnemyStatusStrip",
                        StringComparison.Ordinal));
                Transform sourceSlot = FindDirectChild(
                    strip.transform,
                    "StatusSlot_00");
                GameObject template = UnityEngine.Object.Instantiate(
                    sourceSlot.gameObject);
                try
                {
                    template.name = "FormalBattleStatusSlot";
                    template.transform.SetParent(null, false);
                    RectTransform rect = template.transform as RectTransform;
                    Require(rect != null,
                        "FORMAL_STATUS_SLOT_TEMPLATE_RECT_MISSING");
                    rect.anchoredPosition = Vector2.zero;
                    template.SetActive(true);
                    FormalBattleStatusSlotView view =
                        ConfigureStatusSlotViewReferences(template);
                    Require(view.ValidateAuthoredReferences(),
                        "FORMAL_STATUS_SLOT_TEMPLATE_INVALID");
                    GameObject saved = PrefabUtility.SaveAsPrefabAsset(
                        template,
                        StatusSlotPrefabPath);
                    Require(saved != null,
                        "FORMAL_STATUS_SLOT_PREFAB_SAVE_FAILED");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(template);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(hud);
            }
        }

        private static void ConvertStatusStripToNestedPrefabSlots(
            string prefabPath,
            string stripName)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                Require(rootRect != null,
                    "FORMAL_STATUS_SLOT_OWNER_ROOT_RECT_MISSING "
                    + prefabPath);
                Vector2 rootPosition = rootRect.anchoredPosition;
                Vector2 rootSize = rootRect.sizeDelta;

                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                GameObject slotPrefab = RequirePrefab(StatusSlotPrefabPath);
                FormalBattleStatusSlotView[] slots =
                    new FormalBattleStatusSlotView[8];
                for (int index = 0; index < slots.Length; index++)
                {
                    string slotName = "StatusSlot_" + index.ToString("D2");
                    Transform previous = FindDirectChild(
                        strip.transform,
                        slotName);
                    RectTransform previousRect = previous as RectTransform;
                    Require(previousRect != null,
                        "FORMAL_STATUS_SLOT_PREVIOUS_RECT_MISSING "
                        + slotName);
                    int siblingIndex = previous.GetSiblingIndex();

                    GameObject nested = (GameObject)PrefabUtility
                        .InstantiatePrefab(slotPrefab, strip.transform);
                    nested.name = slotName;
                    CopyRectTransform(
                        previousRect,
                        nested.transform as RectTransform);
                    nested.transform.SetSiblingIndex(siblingIndex);
                    FormalBattleStatusSlotView slot = nested
                        .GetComponent<FormalBattleStatusSlotView>();
                    Require(slot != null
                            && slot.ValidateAuthoredReferences(),
                        "FORMAL_STATUS_SLOT_NESTED_INSTANCE_INVALID "
                        + slotName);
                    ConfigureStatusSlotPreview(slot, index);
                    slots[index] = slot;
                    UnityEngine.Object.DestroyImmediate(previous.gameObject);
                }

                strip.AssignForEditor(slots);
                strip.AssignAlignmentForEditor(
                    string.Equals(
                        stripName,
                        "SelectedEnemyStatusStrip",
                        StringComparison.Ordinal)
                        ? FormalBattleStatusStripAlignment.Right
                        : FormalBattleStatusStripAlignment.Left);
                EditorUtility.SetDirty(strip);
                Require(strip.ValidateAuthoredReferences()
                        && rootRect.anchoredPosition == rootPosition
                        && rootRect.sizeDelta == rootSize,
                    "FORMAL_STATUS_SLOT_OWNER_LAYOUT_CHANGED "
                    + prefabPath);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static FormalBattleStatusSlotView
            ConfigureStatusSlotViewReferences(GameObject slotRoot)
        {
            Require(slotRoot != null,
                "FORMAL_STATUS_SLOT_REFERENCE_ROOT_MISSING");
            Transform tint = FindDirectChild(slotRoot.transform, "StatusTint");
            Transform icon = FindDirectChild(slotRoot.transform, "StatusIcon");
            Transform label = FindDirectChild(slotRoot.transform, "StatusLabel");
            Transform stack = FindDirectChild(slotRoot.transform, "StatusStack");
            Transform duration = FindDirectChild(
                slotRoot.transform,
                "StatusDuration");
            FormalBattleStatusSlotView view =
                slotRoot.GetComponent<FormalBattleStatusSlotView>();
            if (view == null)
            {
                view = slotRoot.AddComponent<FormalBattleStatusSlotView>();
            }
            view.AssignForEditor(
                tint.GetComponent<Image>(),
                icon.GetComponent<Image>(),
                label.GetComponent<TMP_Text>(),
                stack.GetComponent<TMP_Text>(),
                duration.GetComponent<TMP_Text>());
            EditorUtility.SetDirty(view);
            return view;
        }

        private static void ConfigureStatusSlotPreview(
            FormalBattleStatusSlotView slot,
            int slotIndex)
        {
            Color color = ResolveStatusPreviewColor(slotIndex);
            bool positive = slotIndex < 4;
            Sprite icon = positive
                ? RequireAssetAtPath<Sprite>(EnemyShellIconPath)
                : (slotIndex - 4) % 2 == 0
                    ? RequireAssetAtPath<Sprite>(StatusAfterglowIconPath)
                    : RequireAssetAtPath<Sprite>(
                        StatusPollutionWarmRedIconPath);
            slot.ShowPreviewForEditor(
                icon,
                positive ? color : Color.white,
                color,
                "\u00d72",
                "8s");
            EditorUtility.SetDirty(slot);
        }

        private static void ValidateStatusSlotPrefabization()
        {
            GameObject slotPrefab = PrefabUtility.LoadPrefabContents(
                StatusSlotPrefabPath);
            try
            {
                FormalBattleStatusSlotView slot = slotPrefab
                    .GetComponent<FormalBattleStatusSlotView>();
                Require(slot != null && slot.ValidateAuthoredReferences(),
                    "FORMAL_STATUS_SLOT_PREFAB_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(slotPrefab);
            }

            ValidateNestedStatusSlots(
                PlayerPrefabPath,
                "PlayerStatusStrip");
            ValidateNestedStatusSlots(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip");
        }

        private static void ApplyStatusStripCompactAlignmentPass()
        {
            ConfigureReadableStatusSlotPrefabGeometry();
            ConfigureStatusStripAlignmentPrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                FormalBattleStatusStripAlignment.Left);
            ConfigureStatusStripAlignmentPrefab(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip",
                FormalBattleStatusStripAlignment.Right);
            ConfigureUnifiedStatusStripGeometryPrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                "PlayerGuardReadout",
                "PlayerGuardIcon",
                "PlayerGuardLabel");
            ConfigureUnifiedStatusStripGeometryPrefab(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip",
                "SelectedEnemyShellReadout",
                "SelectedEnemyShellIcon",
                "SelectedEnemyShellLabel");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateStatusStripAlignmentPrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                FormalBattleStatusStripAlignment.Left);
            ValidateStatusStripAlignmentPrefab(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip",
                FormalBattleStatusStripAlignment.Right);
            ValidatePinnedStatusCompactParticipation(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                "PlayerGuardReadout");
            ValidatePinnedStatusCompactParticipation(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip",
                "SelectedEnemyShellReadout");
            Debug.Log(StatusStripCompactAlignmentTerminalMarker
                      + " player=left enemy=right compact=1"
                      + " polarityGaps=0 playerGuardCompacts=1"
                      + " enemyShellCompacts=1"
                      + " sharedSize=110 sharedStep=110 rowGap=172"
                      + " transformScale=1"
                      + " rootLayoutWrites=0"
                      + " waterWrites=0 sceneWrites=0");
        }

        private static void ConfigureReadableStatusSlotPrefabGeometry()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                StatusSlotPrefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                Image icon = FindNamedComponent<Image>(root, "StatusIcon");
                TMP_Text stack = FindNamedComponent<TMP_Text>(
                    root,
                    "StatusStack");
                TMP_Text duration = FindNamedComponent<TMP_Text>(
                    root,
                    "StatusDuration");
                Require(rootRect != null
                        && icon != null
                        && stack != null
                        && duration != null,
                    "FORMAL_STATUS_SLOT_READABLE_GEOMETRY_REFERENCE_INVALID");

                rootRect.sizeDelta = new Vector2(110f, 110f);
                rootRect.localScale = Vector3.one;
                Pin(
                    icon.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(80f, 80f));
                icon.rectTransform.localScale = Vector3.one;
                Pin(
                    stack.rectTransform,
                    Vector2.one,
                    new Vector2(-55f, 15.0002f),
                    new Vector2(60f, 30f));
                stack.rectTransform.localScale = Vector3.one;
                stack.fontSize = 40f;
                Pin(
                    duration.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -74f),
                    new Vector2(110f, 36f));
                duration.rectTransform.localScale = Vector3.one;
                duration.fontSize = 40f;
                PrefabUtility.SaveAsPrefabAsset(root, StatusSlotPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureUnifiedStatusStripGeometryPrefab(
            string prefabPath,
            string stripName,
            string pinnedRootName,
            string pinnedIconName,
            string pinnedLabelName)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                RectTransform stripRect = strip.transform as RectTransform;
                RectTransform pinnedRoot = FindNamedTransform(
                        root,
                        pinnedRootName)
                    as RectTransform;
                Image pinnedIcon = FindNamedComponent<Image>(
                    root,
                    pinnedIconName);
                TMP_Text pinnedLabel = FindNamedComponent<TMP_Text>(
                    root,
                    pinnedLabelName);
                FormalBattleStatusSlotView[] slots =
                    strip.GetSlotsForEditor();
                Require(rootRect != null
                        && stripRect != null
                        && pinnedRoot != null
                        && pinnedIcon != null
                        && pinnedLabel != null
                        && slots.Length == 8,
                    "FORMAL_UNIFIED_STATUS_GEOMETRY_REFERENCE_INVALID "
                    + prefabPath);

                Vector2 rootPosition = rootRect.anchoredPosition;
                Vector2 rootSize = rootRect.sizeDelta;
                Vector2 stripPosition = stripRect.anchoredPosition;
                Vector2 stripSize = stripRect.sizeDelta;
                int rowCapacity = slots.Length / 2;
                int firstSlotIndex = strip
                    .ResolveCompactSlotIndexForEditor(0);
                float firstX = slots[firstSlotIndex]
                    .RectTransform.anchoredPosition.x;
                float direction = strip.Alignment
                                  == FormalBattleStatusStripAlignment.Left
                    ? 1f
                    : -1f;
                const float slotSize = 110f;
                const float horizontalStep = 110f;
                const float rowY = 86f;

                stripRect.localScale = Vector3.one;
                for (int compactIndex = 0;
                     compactIndex < slots.Length;
                     compactIndex++)
                {
                    int slotIndex = strip
                        .ResolveCompactSlotIndexForEditor(compactIndex);
                    int rowIndex = compactIndex / rowCapacity;
                    int indexWithinRow = compactIndex % rowCapacity;
                    RectTransform slotRect = slots[slotIndex].RectTransform;
                    Pin(
                        slotRect,
                        new Vector2(0.5f, 0.5f),
                        new Vector2(
                            firstX + direction
                            * horizontalStep
                            * indexWithinRow,
                            rowIndex == 0 ? rowY : -rowY),
                        new Vector2(slotSize, slotSize));
                    slotRect.localScale = Vector3.one;
                }

                Pin(
                    pinnedRoot,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(
                        firstX - direction * horizontalStep,
                        rowY),
                    new Vector2(slotSize, slotSize));
                pinnedRoot.localScale = Vector3.one;
                Image pinnedFrame = pinnedRoot.GetComponent<Image>();
                Require(pinnedFrame != null,
                    "FORMAL_PINNED_STATUS_FRAME_MISSING "
                    + pinnedRootName);
                pinnedFrame.sprite = RequireAssetAtPath<Sprite>(
                    StatusSlotFramePath);
                pinnedFrame.preserveAspect = true;
                pinnedFrame.color = Color.white;
                pinnedFrame.raycastTarget = false;

                Pin(
                    pinnedIcon.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    Vector2.zero,
                    new Vector2(80f, 80f));
                pinnedIcon.rectTransform.localScale = Vector3.one;
                pinnedIcon.preserveAspect = true;
                pinnedIcon.raycastTarget = false;

                Pin(
                    pinnedLabel.rectTransform,
                    Vector2.one,
                    new Vector2(-55f, 15.0002f),
                    new Vector2(60f, 30f));
                pinnedLabel.rectTransform.localScale = Vector3.one;
                pinnedLabel.fontSize = 40f;
                pinnedLabel.fontStyle = FontStyles.Bold;
                pinnedLabel.alignment = TextAlignmentOptions.BottomRight;
                pinnedLabel.color = new Color(0.55f, 0.82f, 1f, 1f);
                pinnedLabel.raycastTarget = false;
                pinnedLabel.text = "100";
                pinnedRoot.gameObject.SetActive(true);

                Require(rootRect.anchoredPosition == rootPosition
                        && rootRect.sizeDelta == rootSize
                        && stripRect.anchoredPosition == stripPosition
                        && stripRect.sizeDelta == stripSize,
                    "FORMAL_UNIFIED_STATUS_OUTER_LAYOUT_CHANGED "
                    + prefabPath);
                EditorUtility.SetDirty(strip);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureStatusStripAlignmentPrefab(
            string prefabPath,
            string stripName,
            FormalBattleStatusStripAlignment alignment)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                Require(rootRect != null,
                    "FORMAL_STATUS_COMPACT_OWNER_ROOT_MISSING "
                    + prefabPath);
                Vector2 rootPosition = rootRect.anchoredPosition;
                Vector2 rootSize = rootRect.sizeDelta;
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                FormalBattleStatusSlotView[] slots =
                    strip.GetSlotsForEditor();
                Vector2[] slotPositions = slots
                    .Select(value => value.RectTransform.anchoredPosition)
                    .ToArray();
                Vector2[] slotSizes = slots
                    .Select(value => value.RectTransform.sizeDelta)
                    .ToArray();

                strip.AssignAlignmentForEditor(alignment);
                EditorUtility.SetDirty(strip);
                Require(rootRect.anchoredPosition == rootPosition
                        && rootRect.sizeDelta == rootSize
                        && slots.Select(value =>
                                value.RectTransform.anchoredPosition)
                            .SequenceEqual(slotPositions)
                        && slots.Select(value =>
                                value.RectTransform.sizeDelta)
                            .SequenceEqual(slotSizes),
                    "FORMAL_STATUS_COMPACT_LAYOUT_CHANGED " + prefabPath);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateStatusStripAlignmentPrefab(
            string prefabPath,
            string stripName,
            FormalBattleStatusStripAlignment expectedAlignment)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                int[] expectedOrder = expectedAlignment
                                      == FormalBattleStatusStripAlignment.Left
                    ? new[] { 0, 1, 2, 3, 4, 5, 6, 7 }
                    : new[] { 3, 2, 1, 0, 7, 6, 5, 4 };
                int[] actualOrder = Enumerable.Range(0, 8)
                    .Select(strip.ResolveCompactSlotIndexForEditor)
                    .ToArray();
                Require(strip.ValidateAuthoredReferences()
                        && strip.Alignment == expectedAlignment
                        && actualOrder.SequenceEqual(expectedOrder),
                    "FORMAL_STATUS_COMPACT_ALIGNMENT_INVALID "
                    + prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidatePinnedStatusCompactParticipation(
            string prefabPath,
            string stripName,
            string pinnedRootName)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                prefabPath);
            try
            {
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                RectTransform pinnedRoot = FindNamedTransform(
                        root,
                        pinnedRootName)
                    as RectTransform;
                FormalBattleStatusSlotView[] slots =
                    strip.GetSlotsForEditor();
                Require(pinnedRoot != null && slots.Length == 8,
                    "FORMAL_PINNED_STATUS_COMPACT_REFERENCE_INVALID "
                    + pinnedRootName);
                Vector2 pinnedPosition = pinnedRoot.anchoredPosition;
                Vector2[] slotPositions = slots
                    .Select(value => value.RectTransform.anchoredPosition)
                    .ToArray();
                int[] compactOrder = Enumerable.Range(0, slots.Length)
                    .Select(strip.ResolveCompactSlotIndexForEditor)
                    .ToArray();
                float direction = strip.Alignment
                                  == FormalBattleStatusStripAlignment.Left
                    ? 1f
                    : -1f;
                Vector2 expectedPinnedPosition = new Vector2(
                    slotPositions[compactOrder[0]].x - direction * 110f,
                    slotPositions[compactOrder[0]].y);
                Require(strip.transform.localScale == Vector3.one
                        && pinnedRoot.sizeDelta == new Vector2(110f, 110f)
                        && pinnedPosition == expectedPinnedPosition
                        && slots.All(value =>
                            value.RectTransform.sizeDelta
                            == new Vector2(110f, 110f)),
                    "FORMAL_PINNED_STATUS_SHARED_GEOMETRY_INVALID "
                    + pinnedRootName);

                Require(strip.ArrangeWithLeadingPinned(pinnedRoot, false)
                        && slots[compactOrder[0]]
                            .RectTransform.anchoredPosition
                        == pinnedPosition
                        && slots[compactOrder[1]]
                            .RectTransform.anchoredPosition
                        == slotPositions[compactOrder[0]]
                        && slots[compactOrder[7]]
                            .RectTransform.anchoredPosition
                        == slotPositions[compactOrder[6]],
                    "FORMAL_PINNED_STATUS_HIDDEN_GAP_REMAINS "
                    + pinnedRootName);
                Require(strip.ArrangeWithLeadingPinned(pinnedRoot, true)
                        && pinnedRoot.anchoredPosition == pinnedPosition
                        && slots.Select(value =>
                                value.RectTransform.anchoredPosition)
                            .SequenceEqual(slotPositions),
                    "FORMAL_PINNED_STATUS_VISIBLE_FLOW_INVALID "
                    + pinnedRootName);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateNestedStatusSlots(
            string prefabPath,
            string stripName)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                FormalBattleStatusSlotView[] slots =
                    strip.GetSlotsForEditor();
                Require(slots.Length == 8
                        && strip.ValidateAuthoredReferences(),
                    "FORMAL_STATUS_SLOT_STRIP_INVALID " + prefabPath);
                for (int index = 0; index < slots.Length; index++)
                {
                    string sourcePath = PrefabUtility
                        .GetPrefabAssetPathOfNearestInstanceRoot(
                            slots[index].gameObject);
                    Require(string.Equals(
                                sourcePath,
                                StatusSlotPrefabPath,
                                StringComparison.Ordinal)
                            && string.Equals(
                                slots[index].gameObject.name,
                                "StatusSlot_" + index.ToString("D2"),
                                StringComparison.Ordinal),
                        "FORMAL_STATUS_SLOT_NOT_SHARED_NESTED_INSTANCE "
                        + prefabPath + " " + index);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureStatusIconPalettePrefab(
            string prefabPath,
            string stripName,
            bool arrangeFromSelectedEnemyManualReference)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                Require(rootRect != null,
                    "FORMAL_STATUS_ICON_PALETTE_ROOT_RECT_MISSING "
                    + prefabPath);
                Vector2 rootPosition = rootRect.anchoredPosition;
                Vector2 rootSize = rootRect.sizeDelta;

                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                RectTransform slotZero = FindDirectChild(
                        strip.transform,
                        "StatusSlot_00")
                    as RectTransform;
                Require(slotZero != null,
                    "FORMAL_STATUS_ICON_PALETTE_SLOT_ZERO_MISSING");
                Vector2 slotZeroPosition = slotZero.anchoredPosition;
                Vector2 slotZeroSize = slotZero.sizeDelta;

                RectTransform selectedShell = null;
                Vector2 selectedShellPosition = Vector2.zero;
                Vector2 selectedShellSize = Vector2.zero;
                if (arrangeFromSelectedEnemyManualReference)
                {
                    selectedShell = FindNamedTransform(
                            root,
                            "SelectedEnemyShellReadout")
                        as RectTransform;
                    Require(selectedShell != null,
                        "FORMAL_SELECTED_ENEMY_MANUAL_SHELL_REFERENCE_MISSING");
                    selectedShellPosition = selectedShell.anchoredPosition;
                    selectedShellSize = selectedShell.sizeDelta;
                    ArrangeSelectedEnemyStatusSlotsFromManualReference(
                        strip,
                        selectedShell,
                        slotZero);
                }

                Sprite positivePreviewIcon = RequireAssetAtPath<Sprite>(
                    EnemyShellIconPath);
                Sprite afterglowIcon = RequireAssetAtPath<Sprite>(
                    StatusAfterglowIconPath);
                Sprite pollutionIcon = RequireAssetAtPath<Sprite>(
                    StatusPollutionWarmRedIconPath);
                for (int index = 0; index < strip.AuthoredSlotCount; index++)
                {
                    Transform slot = FindDirectChild(
                        strip.transform,
                        "StatusSlot_" + index.ToString("D2"));
                    Require(slot != null,
                        "FORMAL_STATUS_ICON_PALETTE_SLOT_MISSING " + index);
                    slot.gameObject.SetActive(true);

                    Transform tint = FindDirectChild(slot, "StatusTint");
                    Require(tint != null,
                        "FORMAL_STATUS_TINT_MISSING " + index);
                    Image tintImage = tint.GetComponent<Image>();
                    Require(tintImage != null,
                        "FORMAL_STATUS_TINT_IMAGE_MISSING " + index);
                    tint.gameObject.SetActive(false);
                    tintImage.enabled = false;

                    Image icon = FindDirectChild(slot, "StatusIcon")
                        .GetComponent<Image>();
                    TMP_Text label = FindDirectChild(slot, "StatusLabel")
                        .GetComponent<TMP_Text>();
                    TMP_Text stack = FindDirectChild(slot, "StatusStack")
                        .GetComponent<TMP_Text>();
                    TMP_Text duration = FindDirectChild(
                            slot,
                            "StatusDuration")
                        .GetComponent<TMP_Text>();
                    icon.sprite = index < strip.AuthoredSlotCount / 2
                        ? positivePreviewIcon
                        : (index - strip.AuthoredSlotCount / 2) % 2 == 0
                            ? afterglowIcon
                            : pollutionIcon;
                    Color previewColor = ResolveStatusPreviewColor(index);
                    icon.color = index < strip.AuthoredSlotCount / 2
                        ? previewColor
                        : Color.white;
                    icon.enabled = true;
                    label.text = string.Empty;
                    stack.text = "\u00d72";
                    stack.color = previewColor;
                    duration.text = "8s";
                    duration.color = previewColor;
                }

                Require(rootRect.anchoredPosition == rootPosition
                        && rootRect.sizeDelta == rootSize
                        && slotZero.anchoredPosition == slotZeroPosition
                        && slotZero.sizeDelta == slotZeroSize,
                    "FORMAL_STATUS_MANUAL_ROOT_OR_SLOT_ZERO_MOVED");
                if (arrangeFromSelectedEnemyManualReference)
                {
                    Require(selectedShell.anchoredPosition
                            == selectedShellPosition
                            && selectedShell.sizeDelta == selectedShellSize,
                        "FORMAL_SELECTED_ENEMY_MANUAL_SHELL_MOVED");
                }

                EditorUtility.SetDirty(strip);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ArrangeSelectedEnemyStatusSlotsFromManualReference(
            FormalBattleStatusStripView strip,
            RectTransform selectedShell,
            RectTransform slotZero)
        {
            float horizontalStep = slotZero.anchoredPosition.x
                                   - selectedShell.anchoredPosition.x;
            Require(Mathf.Abs(horizontalStep) >= 1f,
                "FORMAL_SELECTED_ENEMY_MANUAL_STATUS_STEP_INVALID");

            RectTransform positiveMarker = FindDirectChild(
                    strip.transform,
                    "PositiveStatusMarker")
                as RectTransform;
            RectTransform negativeMarker = FindDirectChild(
                    strip.transform,
                    "NegativeStatusMarker")
                as RectTransform;
            Require(positiveMarker != null && negativeMarker != null,
                "FORMAL_SELECTED_ENEMY_STATUS_ROW_MARKERS_MISSING");
            float negativeRowY = slotZero.anchoredPosition.y
                                 + negativeMarker.anchoredPosition.y
                                 - positiveMarker.anchoredPosition.y;

            for (int index = 1; index < 4; index++)
            {
                RectTransform slot = FindDirectChild(
                        strip.transform,
                        "StatusSlot_" + index.ToString("D2"))
                    as RectTransform;
                CopyRectTransform(slotZero, slot);
                slot.anchoredPosition = new Vector2(
                    slotZero.anchoredPosition.x + horizontalStep * index,
                    slotZero.anchoredPosition.y);
            }
            for (int index = 4; index < 8; index++)
            {
                RectTransform slot = FindDirectChild(
                        strip.transform,
                        "StatusSlot_" + index.ToString("D2"))
                    as RectTransform;
                CopyRectTransform(slotZero, slot);
                slot.anchoredPosition = new Vector2(
                    slotZero.anchoredPosition.x
                    + horizontalStep * (index - 4),
                    negativeRowY);
            }
        }

        private static Color ResolveStatusPreviewColor(int slotIndex)
        {
            Color[] positivePalette =
            {
                new Color(0.55f, 0.82f, 1f, 1f),
                new Color(0.40f, 0.70f, 1f, 1f),
                new Color(0.34f, 0.58f, 0.94f, 1f),
                new Color(0.42f, 0.48f, 0.88f, 1f)
            };
            Color[] negativePalette =
            {
                new Color(1f, 0.66f, 0.34f, 1f),
                new Color(1f, 0.58f, 0.48f, 1f),
                new Color(0.96f, 0.40f, 0.30f, 1f),
                new Color(0.86f, 0.28f, 0.26f, 1f)
            };
            return slotIndex < 4
                ? positivePalette[Mathf.Clamp(slotIndex, 0, 3)]
                : negativePalette[Mathf.Clamp(slotIndex - 4, 0, 3)];
        }

        private static void ValidateStatusIconPalette()
        {
            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            FormalBattleStatusVisualStyle[] styles = profile
                .GetStatusStylesForEditor();
            Require(styles.Length == 2
                    && styles.All(value => value != null
                        && value.Validate()
                        && value.Icon != null
                        && value.Polarity
                        == FormalBattleStatusPolarity.Negative),
                "FORMAL_STATUS_ICON_PALETTE_ROWS_INVALID");

            ValidateStatusIconPalettePrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip");
            ValidateStatusIconPalettePrefab(
                SelectedEnemyHudPrefabPath,
                "SelectedEnemyStatusStrip");
        }

        private static void ValidateStatusIconPalettePrefab(
            string prefabPath,
            string stripName)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                for (int index = 0; index < strip.AuthoredSlotCount; index++)
                {
                    Transform slot = FindDirectChild(
                        strip.transform,
                        "StatusSlot_" + index.ToString("D2"));
                    Transform tint = FindDirectChild(slot, "StatusTint");
                    Image icon = FindDirectChild(slot, "StatusIcon")
                        .GetComponent<Image>();
                    TMP_Text label = FindDirectChild(slot, "StatusLabel")
                        .GetComponent<TMP_Text>();
                    Require(slot.gameObject.activeSelf
                            && tint != null
                            && !tint.gameObject.activeSelf
                            && !tint.GetComponent<Image>().enabled
                            && icon.enabled
                            && icon.sprite != null
                            && string.IsNullOrEmpty(label.text),
                        "FORMAL_STATUS_ICON_ONLY_PREVIEW_INVALID "
                        + prefabPath + " " + index);
                }
                if (string.Equals(
                    prefabPath,
                    SelectedEnemyHudPrefabPath,
                    StringComparison.Ordinal))
                {
                    RectTransform templateSlot = FindDirectChild(
                            strip.transform,
                            "StatusSlot_00")
                        as RectTransform;
                    for (int index = 1;
                         index < strip.AuthoredSlotCount;
                         index++)
                    {
                        RectTransform targetSlot = FindDirectChild(
                                strip.transform,
                                "StatusSlot_" + index.ToString("D2"))
                            as RectTransform;
                        ValidateStatusSlotVisualLayoutMatches(
                            templateSlot,
                            targetSlot,
                            index);
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateStatusSlotVisualLayoutMatches(
            RectTransform templateSlot,
            RectTransform targetSlot,
            int slotIndex)
        {
            string[] visualChildren =
            {
                "StatusTint",
                "StatusIcon",
                "StatusLabel",
                "StatusStack",
                "StatusDuration"
            };
            foreach (string childName in visualChildren)
            {
                RectTransform sourceChild = FindDirectChild(
                        templateSlot,
                        childName)
                    as RectTransform;
                RectTransform targetChild = FindDirectChild(
                        targetSlot,
                        childName)
                    as RectTransform;
                Require(sourceChild != null
                        && targetChild != null
                        && targetChild.anchorMin == sourceChild.anchorMin
                        && targetChild.anchorMax == sourceChild.anchorMax
                        && targetChild.pivot == sourceChild.pivot
                        && targetChild.anchoredPosition
                        == sourceChild.anchoredPosition
                        && targetChild.sizeDelta == sourceChild.sizeDelta
                        && targetChild.localScale == sourceChild.localScale
                        && targetChild.localRotation
                        == sourceChild.localRotation,
                    "FORMAL_STATUS_INTERNAL_RECT_LAYOUT_MISMATCH "
                    + slotIndex + " " + childName);

                TMP_Text sourceText = sourceChild.GetComponent<TMP_Text>();
                TMP_Text targetText = targetChild.GetComponent<TMP_Text>();
                if (sourceText != null && targetText != null)
                {
                    Require(Mathf.Approximately(
                                targetText.fontSize,
                                sourceText.fontSize)
                            && targetText.alignment
                            == sourceText.alignment,
                        "FORMAL_STATUS_INTERNAL_TEXT_LAYOUT_MISMATCH "
                        + slotIndex + " " + childName);
                }
            }
        }

        private static void ConfigureSelectedEnemyHudEditPreview(
            GameObject root)
        {
            Require(root != null,
                "FORMAL_SELECTED_ENEMY_HUD_PREVIEW_ROOT_MISSING");
            root.SetActive(true);

            Image portrait = FindNamedComponent<Image>(
                root,
                "SelectedEnemyPortrait");
            portrait.sprite = RequireAssetAtPath<Sprite>(
                SelectedEnemyPreviewPortraitPath);
            portrait.color = Color.white;
            portrait.enabled = true;
            portrait.preserveAspect = true;

            TMP_Text identity = FindNamedComponent<TMP_Text>(
                root,
                "SelectedEnemyIdentity");
            identity.text = "\u9009\u4e2d\u654c\u4eba";

            Image hpFill = FindNamedComponent<Image>(
                root,
                "SelectedEnemyHpFill");
            hpFill.enabled = true;
            hpFill.fillAmount = 0.82f;
            TMP_Text hpLabel = FindNamedComponent<TMP_Text>(
                root,
                "SelectedEnemyHpLabel");
            hpLabel.text = "820 / 1000";

            Transform shellRoot = FindNamedTransform(
                root,
                "SelectedEnemyShellReadout");
            shellRoot.gameObject.SetActive(true);
            Image shellIcon = FindNamedComponent<Image>(
                root,
                "SelectedEnemyShellIcon");
            shellIcon.enabled = true;
            TMP_Text shellLabel = FindNamedComponent<TMP_Text>(
                root,
                "SelectedEnemyShellLabel");
            shellLabel.text = "100";

            FormalBattleStatusStripView strip = root
                .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                .Single();
            strip.gameObject.SetActive(true);
            Transform positiveMarker = FindDirectChild(
                strip.transform,
                "PositiveStatusMarker");
            Transform negativeMarker = FindDirectChild(
                strip.transform,
                "NegativeStatusMarker");
            Require(positiveMarker != null && negativeMarker != null,
                "FORMAL_SELECTED_ENEMY_HUD_PREVIEW_MARKERS_MISSING");
            positiveMarker.gameObject.SetActive(true);
            negativeMarker.gameObject.SetActive(true);

            Sprite positiveIcon = RequireAssetAtPath<Sprite>(
                EnemyShellIconPath);
            Sprite negativeIcon = RequireAssetAtPath<Sprite>(
                StatusPollutionWarmRedIconPath);
            for (int index = 0; index < strip.AuthoredSlotCount; index++)
            {
                Transform slot = FindDirectChild(
                    strip.transform,
                    "StatusSlot_" + index.ToString("D2"));
                Require(slot != null,
                    "FORMAL_SELECTED_ENEMY_HUD_PREVIEW_SLOT_MISSING "
                    + index);
                slot.gameObject.SetActive(true);
                foreach (Transform child in slot.Cast<Transform>())
                {
                    child.gameObject.SetActive(true);
                }

                Image icon = slot.Cast<Transform>()
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "StatusIcon",
                        StringComparison.Ordinal))
                    .GetComponent<Image>();
                TMP_Text label = slot.Cast<Transform>()
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "StatusLabel",
                        StringComparison.Ordinal))
                    .GetComponent<TMP_Text>();
                TMP_Text stack = slot.Cast<Transform>()
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "StatusStack",
                        StringComparison.Ordinal))
                    .GetComponent<TMP_Text>();
                TMP_Text duration = slot.Cast<Transform>()
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "StatusDuration",
                        StringComparison.Ordinal))
                    .GetComponent<TMP_Text>();

                icon.sprite = index < strip.AuthoredSlotCount / 2
                    ? positiveIcon
                    : (index - strip.AuthoredSlotCount / 2) % 2 == 0
                        ? RequireAssetAtPath<Sprite>(
                            StatusAfterglowIconPath)
                        : negativeIcon;
                icon.color = Color.white;
                icon.enabled = true;
                label.text = string.Empty;
                stack.text = "\u00d72";
                duration.text = "8s";
            }

            EditorUtility.SetDirty(root);
        }

        private static void ValidateSelectedEnemyHudPrefabExtraction()
        {
            GameObject source = PrefabUtility.LoadPrefabContents(
                SelectedEnemyHudPrefabPath);
            try
            {
                FormalBattleSelectedEnemyHudView view = source
                    .GetComponent<FormalBattleSelectedEnemyHudView>();
                FormalBattleStatusStripView strip = source
                    .GetComponentInChildren<FormalBattleStatusStripView>(true);
                Require(view != null
                        && view.ValidateAuthoredReferences()
                        && source.activeSelf
                        && source.GetComponent<
                            FormalBattlePlayerPresentationView>() == null
                        && strip != null
                        && strip.AuthoredSlotCount == 8,
                    "FORMAL_SELECTED_ENEMY_HUD_PREFAB_STRUCTURE_INVALID");
                Require(FindNamedComponent<Image>(
                            source,
                            "SelectedEnemyPortrait").enabled
                        && FindNamedComponent<Image>(
                            source,
                            "SelectedEnemyPortrait").sprite != null
                        && !string.IsNullOrWhiteSpace(
                            FindNamedComponent<TMP_Text>(
                                source,
                                "SelectedEnemyIdentity").text)
                        && !string.IsNullOrWhiteSpace(
                            FindNamedComponent<TMP_Text>(
                                source,
                                "SelectedEnemyHpLabel").text)
                        && FindNamedTransform(
                            source,
                            "SelectedEnemyShellReadout").gameObject.activeSelf,
                    "FORMAL_SELECTED_ENEMY_HUD_EDIT_PREVIEW_INVALID");
                for (int index = 0; index < strip.AuthoredSlotCount; index++)
                {
                    Transform slot = FindDirectChild(
                        strip.transform,
                        "StatusSlot_" + index.ToString("D2"));
                    Require(slot != null && slot.gameObject.activeSelf,
                        "FORMAL_SELECTED_ENEMY_HUD_PREVIEW_SLOT_HIDDEN "
                        + index);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(source);
            }

            GameObject presentation = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                FormalBattlePresentationRoot presenter = presentation
                    .GetComponent<FormalBattlePresentationRoot>();
                FormalBattleSelectedEnemyHudView nestedView = presentation
                    .GetComponentsInChildren<
                        FormalBattleSelectedEnemyHudView>(true)
                    .Single();
                GameObject sourceObject = PrefabUtility
                    .GetCorrespondingObjectFromSource(nestedView.gameObject);
                Require(presenter != null
                        && presenter.ValidateSourceCarrierReferences(out _)
                        && nestedView.gameObject.activeSelf
                        && sourceObject != null
                        && string.Equals(
                            AssetDatabase.GetAssetPath(sourceObject),
                            SelectedEnemyHudPrefabPath,
                            StringComparison.Ordinal),
                    "FORMAL_SELECTED_ENEMY_HUD_ROOT_NESTING_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(presentation);
            }
        }

        private static void ApplyEnemyHudActorStageSeparationPass()
        {
            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            Require(profile.ValidateAuthoredReferences(),
                "FORMAL_PRESENTATION_PROFILE_REFERENCE_INVALID");

            EnsureUiSpriteImport(EnemyShellIconPath);
            AddPlayerNianBarFromCurrentHpLayout();
            ConvertEnemyPrefabToActorStage();
            RecomposeSelectedEnemyHud(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateEnemyHudActorStageSeparation();
            Debug.Log(EnemyHudActorStageSeparationTerminalMarker
                      + " playerNian=1 selectedEnemyHud=1 actorStages=3"
                      + " sceneWrites=0 shellWrites=0 waterWrites=0");
        }

        private static void ApplySelectedEnemyShellIconPass()
        {
            EnsureUiSpriteImport(EnemyShellIconPath);
            GameObject root = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                FormalBattleSelectedEnemyHudView selectedHud = root
                    .GetComponentsInChildren<
                        FormalBattleSelectedEnemyHudView>(true)
                    .Single();
                Transform shellTransform = FindNamedTransform(
                    root,
                    "SelectedEnemyShellReadout");
                TMP_Text shellLabel = FindNamedComponent<TMP_Text>(
                    root,
                    "SelectedEnemyShellLabel");
                Image shellIcon = ConfigureSelectedEnemyShellIcon(
                    shellTransform,
                    shellLabel);
                selectedHud.AssignShellIconForEditor(
                    shellTransform.gameObject,
                    shellIcon,
                    shellLabel);
                EditorUtility.SetDirty(selectedHud);
                Require(selectedHud.ValidateAuthoredReferences(),
                    "FORMAL_SELECTED_ENEMY_SHELL_ICON_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    PresentationRootPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(SelectedEnemyShellIconTerminalMarker
                      + " icon=1 bar=0 sceneWrites=0 waterWrites=0");
        }

        private static void ApplyBattleSandboxHudVisualParityPass()
        {
            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            Require(profile.ValidateAuthoredReferences(),
                "FORMAL_PRESENTATION_PROFILE_REFERENCE_INVALID");

            ExtendPlayerReadabilityPrefab(profile);
            ExtendEnemyReadabilityPrefab(profile);
            RestylePresentationRootPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateCoreBattleReadabilityDecorAssets();
            Debug.Log(BattleSandboxHudVisualParityTerminalMarker
                      + " playerHud=1 enemyHud=1 statusRows=2"
                      + " presentationRoot=1 unifiedWrites=0 sceneWrites=0"
                      + " waterWrites=0");
        }

        private static void ApplyDynamicCausalGrammarProfilePass()
        {
            FormalBattlePresentationProfile existing =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            ProfilePreservationSnapshot preservation =
                CaptureProfilePreservation(existing);
            IReadOnlyDictionary<string, byte[]> protectedBytes =
                CaptureProfileCoverageProtectedBytes();

            try
            {
                FormalBattlePresentationProfile profile =
                    AuthorProfile(preservation);
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssetIfDirty(profile);
                ValidateProfileCoverageProtectedBytes(protectedBytes);
                AssetDatabase.ImportAsset(
                    ProfilePath,
                    ImportAssetOptions.ForceSynchronousImport
                    | ImportAssetOptions.ForceUpdate);
                ValidateProfileCoverageProtectedBytes(protectedBytes);

                FormalBattlePresentationProfile imported =
                    RequireAssetAtPath<FormalBattlePresentationProfile>(
                        ProfilePath);
                ValidateDynamicGrammarProfileAsset(imported, preservation);
                ValidateRuntimeBoundaries();
                ValidateDynamicGrammarProfileBoundaries();
                ValidateProfileCoverageProtectedBytes(protectedBytes);
                Debug.Log(DynamicGrammarProfileTerminalMarker
                          + " enemyRows=3 grammarRows="
                          + imported.GetCausalStylesForEditor().Length
                          + " profileWrites=1 prefabWrites=0 sceneWrites=0"
                          + " mainlineWrites=0 itemWrites=0 enemyWrites=0"
                          + " battleWrites=0");
            }
            finally
            {
                ValidateProfileCoverageProtectedBytes(protectedBytes);
            }
        }

        private static void ApplySourceCarrierCorrectionPass()
        {
            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            Require(profile.ValidateAuthoredReferences(),
                "FORMAL_PRESENTATION_PROFILE_REFERENCE_INVALID");
            AuthorPresentationRootPrefab(profile);
            AssetDatabase.ImportAsset(
                PresentationRootPrefabPath,
                ImportAssetOptions.ForceSynchronousImport
                | ImportAssetOptions.ForceUpdate);
            ValidateSourceCarrierAuthoredAsset();
            ValidateSourceCarrierApiContract();
            Debug.Log(SourceCarrierCorrectionTerminalMarker
                      + " player=1 stageLabel=1 enemySlots=3"
                      + " causalSourceProxies=4 externalBindings=0"
                      + " unifiedWrites=0 sceneWrites=0 itemWrites=0");
        }

        private static void ApplyCausalVisualAuthoringPass()
        {
            EnsureFolder(PresentationPrefabFolder);
            EnsureFolder(ProfileFolder);
            FormalBattlePresentationProfile profile = AuthorProfile();
            AuthorCueFxPrefab(profile);
            AuthorPresentationRootPrefab(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateAuthoredAssets();
            ValidateDeterministicCueConsumption();
            ValidateRuntimeBoundaries();
            ValidateCausalVisualBoundaries();
            Debug.Log(CausalVisualTerminalMarker
                      + " sourceProxyPool=4 ribbonPool=6"
                      + " damageFloatWrites=0 unifiedWrites=0 sceneWrites=0");
        }

        private static void ApplyAudioLightFix()
        {
            EnsureFolder(ProfileFolder);
            EnsureFolder(AudioFolder);
            AuthorDeterministicAudioAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            FormalBattlePresentationProfile profile = AuthorProfile();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateAuthoredAssets();
            ValidateRuntimeBoundaries();
            Debug.Log(AudioLightFixTerminalMarker
                      + " source=1 enemyAttack=1 impact=1"
                      + " unifiedSceneWrites=0 prefabWrites=0");
        }

        private static void ApplySingleAuthoringPass()
        {
            EnsureFolder(PresentationPrefabFolder);
            EnsureFolder(ProfileFolder);
            EnsureFolder(AudioFolder);
            AuthorDeterministicAudioAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            FormalBattlePresentationProfile profile = AuthorProfile();
            AuthorPlayerPrefab(profile);
            AuthorEnemyPrefab(profile);
            ExtendPlayerReadabilityPrefab(profile);
            ExtendEnemyReadabilityPrefab(profile);
            AuthorDamageFloatPrefab(profile);
            AuthorCueFxPrefab(profile);
            AuthorPresentationRootPrefab(profile);
            AuthorUnifiedComposition(profile);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            ValidateAuthoredAssets();
            ValidateDeterministicCueConsumption();
            ValidateRuntimeBoundaries();
            Debug.Log(TerminalMarker
                      + " root=1 player=1 enemySlot=1 floats=10 cueFx=1"
                      + " audioClips=3 unifiedSceneWrites=0");
        }

        private static FormalBattlePresentationProfile AuthorProfile(
            ProfilePreservationSnapshot requiredPreservation = null)
        {
            FormalBattlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    FormalBattlePresentationProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<
                    FormalBattlePresentationProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            ProfilePreservationSnapshot preservation =
                requiredPreservation ?? CaptureProfilePreservation(profile);

            Sprite playerAvatar = RequireGuidAsset<Sprite>(PlayerAvatarGuid);
            Sprite hpFrame = RequireGuidAsset<Sprite>(HpFrameGuid);
            TMP_FontAsset font = RequireGuidAsset<TMP_FontAsset>(DamageFontGuid);
            Texture2D restrainedVfx =
                RequireGuidAsset<Texture2D>(RestrainedVfxGuid);
            AudioClip sourcePulse = RequireAssetAtPath<AudioClip>(
                SourcePulseAudioPath);
            AudioClip enemyAttack = RequireAssetAtPath<AudioClip>(
                EnemyAttackAudioPath);
            AudioClip impact = RequireAssetAtPath<AudioClip>(ImpactAudioPath);

            RectInt hostReference = new RectInt(0, 0, 960, 960);
            RectInt hostUnion = new RectInt(14, 0, 945, 949);
            RectInt hostLarge = new RectInt(0, 0, 1440, 1440);
            RectInt houndBounds = new RectInt(0, 0, 512, 765);

            FormalBattleEnemyVisualProfile host =
                new FormalBattleEnemyVisualProfile();
            host.AssignForEditor(
                C1EnemyRuntimeContract.ShatteredHostContentId,
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                "碎契宿主",
                SequenceClip(
                    "shattered-host.idle",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Idle/frames",
                    125,
                    true,
                    hostReference,
                    hostUnion,
                    1, 4, 7, 10, 13, 16, 19, 22),
                SequenceClip(
                    "shattered-host.attack",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Attack/frames",
                    95,
                    false,
                    hostReference,
                    hostUnion,
                    1, 4, 7, 10, 13, 17, 21, 25),
                SequenceClip(
                    "shattered-host.hit",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Hit/frames",
                    60,
                    false,
                    hostReference,
                    hostLarge,
                    1, 7, 13, 19),
                SequenceClip(
                    "shattered-host.death",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Death/frames",
                    100,
                    false,
                    hostReference,
                    hostUnion,
                    1, 3, 5, 8, 11, 14, 17, 20, 22, 24));

            FormalBattleEnemyVisualProfile hound =
                new FormalBattleEnemyVisualProfile();
            hound.AssignForEditor(
                C1EnemyRuntimeContract.PorcelainHoundContentId,
                C1EnemyRuntimeContract.PorcelainHoundProfileId,
                "瓷骨猎犬",
                SingleTextureClip(
                    "porcelain-hound.idle",
                    "Assets/_Game/Resources/Enemy/guciquan_idle.png",
                    true,
                    houndBounds),
                SingleTextureClip(
                    "porcelain-hound.attack",
                    "Assets/_Game/Resources/Enemy/guciquan_attack.png",
                    false,
                    houndBounds),
                SingleTextureClip(
                    "porcelain-hound.hit",
                    "Assets/_Game/Resources/Enemy/guciquan_hit.png",
                    false,
                    houndBounds),
                SingleTextureClip(
                    "porcelain-hound.death",
                    "Assets/_Game/Resources/Enemy/guciquan_death.png",
                    false,
                    houndBounds));

            FormalBattleEnemyVisualProfile boneSwapRemnant =
                new FormalBattleEnemyVisualProfile();
            boneSwapRemnant.AssignForEditor(
                BoneSwapRemnantRuntimeContract.ContentId,
                BoneSwapRemnantRuntimeContract.OperatorProfileId,
                "\u6362\u9AA8\u6B8B\u76F8",
                SequenceClip(
                    "bone-swap-remnant.idle",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Idle/frames",
                    125,
                    true,
                    hostReference,
                    hostUnion,
                    1, 4, 7, 10, 13, 16, 19, 22),
                SequenceClip(
                    "bone-swap-remnant.attack-or-skill",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Skill/frames",
                    100,
                    false,
                    hostReference,
                    hostLarge,
                    1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 22, 25),
                SequenceClip(
                    "bone-swap-remnant.hit",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Hit/frames",
                    60,
                    false,
                    hostReference,
                    hostLarge,
                    1, 7, 13, 19),
                SequenceClip(
                    "bone-swap-remnant.death",
                    "Assets/_Game/Resources/anim/Enemy/d1/d1_3/d1_3_Death/frames",
                    100,
                    false,
                    hostReference,
                    hostUnion,
                    1, 3, 5, 8, 11, 14, 17, 20, 22, 24));

            profile.AssignForEditor(
                playerAvatar,
                hpFrame,
                font,
                restrainedVfx,
                sourcePulse,
                enemyAttack,
                impact,
                new[] { host, hound, boneSwapRemnant });
            profile.AssignCausalVisualsForEditor(
                BuildDynamicCausalGrammarStyles(
                    profile,
                    ResolveAuthoredItemPresentationCatalog()),
                0.42f,
                0.72f,
                0.28f);
            profile.AssignStatusVisualsForEditor(
                CreateDefaultStatusVisualRows());
            EditorUtility.SetDirty(profile);
            Require(profile.ValidateAuthoredReferences(),
                "FORMAL_PRESENTATION_PROFILE_REFERENCE_INVALID");
            Require(profile.SourcePulseClip != null
                    && profile.EnemyAttackClip != null
                    && profile.ImpactClip != null,
                "FORMAL_PRESENTATION_AUDIO_PROFILE_SLOT_MISSING");
            ValidateProfilePreservation(profile, preservation);
            return profile;
        }

        private static FormalBattleStatusVisualStyle[]
            CreateDefaultStatusVisualRows()
        {
            Sprite afterglowIcon = RequireAssetAtPath<Sprite>(
                StatusAfterglowIconPath);
            Sprite pollutionIcon = RequireAssetAtPath<Sprite>(
                StatusPollutionWarmRedIconPath);
            FormalBattleStatusVisualStyle afterglow =
                new FormalBattleStatusVisualStyle();
            afterglow.AssignForEditor(
                "afterglow",
                "PERIODIC_DAMAGE",
                "\u4F59\u70EC",
                string.Empty,
                afterglowIcon,
                FormalBattleStatusPolarity.Negative,
                new Color(0.48f, 0.11f, 0.045f, 0.96f),
                new Color(1f, 0.66f, 0.34f, 1f));

            FormalBattleStatusVisualStyle pollution =
                new FormalBattleStatusVisualStyle();
            pollution.AssignForEditor(
                "pollution",
                "NEGATIVE_POLLUTION",
                "\u6C61\u67D3",
                string.Empty,
                pollutionIcon,
                FormalBattleStatusPolarity.Negative,
                new Color(0.42f, 0.055f, 0.075f, 0.96f),
                new Color(1f, 0.58f, 0.48f, 1f));

            return new[] { afterglow, pollution };
        }

        private static void RestyleExistingStatusStripPrefab(
            string prefabPath,
            string stripName,
            Sprite frameSprite)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                FormalBattleStatusStripView strip =
                    root.GetComponentsInChildren<
                            FormalBattleStatusStripView>(true)
                        .Single(value => string.Equals(
                            value.gameObject.name,
                            stripName,
                            StringComparison.Ordinal));
                int count = strip.AuthoredSlotCount;
                GameObject[] roots = new GameObject[count];
                Image[] backgrounds = new Image[count];
                Image[] icons = new Image[count];
                TMP_Text[] labels = new TMP_Text[count];
                TMP_Text[] stacks = new TMP_Text[count];
                TMP_Text[] durations = new TMP_Text[count];

                for (int index = 0; index < count; index++)
                {
                    string slotName = "StatusSlot_"
                        + index.ToString("D2");
                    Transform slot = strip.transform.Cast<Transform>()
                        .Single(value => string.Equals(
                            value.gameObject.name,
                            slotName,
                            StringComparison.Ordinal));
                    roots[index] = slot.gameObject;

                    Image frame = slot.GetComponent<Image>();
                    Require(frame != null,
                        "FORMAL_STATUS_SLOT_FRAME_MISSING " + slotName);
                    frame.sprite = frameSprite;
                    frame.type = Image.Type.Simple;
                    frame.preserveAspect = true;
                    frame.color = Color.white;
                    frame.raycastTarget = false;

                    Transform tintTransform = slot.Cast<Transform>()
                        .SingleOrDefault(value => string.Equals(
                            value.gameObject.name,
                            "StatusTint",
                            StringComparison.Ordinal));
                    Image tint;
                    if (tintTransform == null)
                    {
                        tint = CreateImage(
                            "StatusTint",
                            slot,
                            new Color(0.18f, 0.08f, 0.06f, 0.94f));
                    }
                    else
                    {
                        tint = tintTransform.GetComponent<Image>();
                        Require(tint != null,
                            "FORMAL_STATUS_TINT_IMAGE_MISSING "
                            + slotName);
                    }
                    tint.sprite = null;
                    tint.type = Image.Type.Simple;
                    tint.raycastTarget = false;
                    Pin(
                        tint.rectTransform,
                        new Vector2(0.5f, 0.5f),
                        Vector2.zero,
                        new Vector2(30f, 30f));
                    tint.transform.SetAsFirstSibling();
                    backgrounds[index] = tint;

                    icons[index] = slot.Cast<Transform>()
                        .Single(value => string.Equals(
                            value.gameObject.name,
                            "StatusIcon",
                            StringComparison.Ordinal))
                        .GetComponent<Image>();
                    labels[index] = slot.Cast<Transform>()
                        .Single(value => string.Equals(
                            value.gameObject.name,
                            "StatusLabel",
                            StringComparison.Ordinal))
                        .GetComponent<TMP_Text>();
                    stacks[index] = slot.Cast<Transform>()
                        .Single(value => string.Equals(
                            value.gameObject.name,
                            "StatusStack",
                            StringComparison.Ordinal))
                        .GetComponent<TMP_Text>();
                    durations[index] = slot.Cast<Transform>()
                        .Single(value => string.Equals(
                            value.gameObject.name,
                            "StatusDuration",
                            StringComparison.Ordinal))
                        .GetComponent<TMP_Text>();
                }

                strip.AssignForEditor(
                    roots,
                    backgrounds,
                    icons,
                    labels,
                    stacks,
                    durations);
                strip.AssignAlignmentForEditor(
                    stripName.IndexOf(
                        "Enemy",
                        StringComparison.Ordinal) >= 0
                        ? FormalBattleStatusStripAlignment.Right
                        : FormalBattleStatusStripAlignment.Left);
                EditorUtility.SetDirty(strip);
                Require(strip.ValidateAuthoredReferences(),
                    "FORMAL_STATUS_STRIP_RESTYLE_INVALID " + stripName);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static FormalBattleCausalItemStyle[]
            BuildDynamicCausalGrammarStyles(
                FormalBattlePresentationProfile profile,
                C1FormalItemPresentationCatalogSnapshot catalog)
        {
            Require(profile != null,
                "FORMAL_CAUSAL_GRAMMAR_PROFILE_MISSING");
            Require(catalog != null
                    && catalog.Rows != null
                    && catalog.Rows.Count > 0,
                "FORMAL_CAUSAL_GRAMMAR_CATALOG_MISSING");

            FormalBattleCausalItemStyle[] current = profile
                .GetCausalStylesForEditor();
            if (current.Length > 0
                && current.All(value => value != null && value.Validate()))
            {
                return current.Select(CloneCausalGrammar).ToArray();
            }

            FormalBattleCausalItemStyle[] legacy = current
                .Where(value => value != null
                    && value.GrammarKey.Length > 0
                    && value.ValidateVisualGrammarForEditor())
                .ToArray();
            Require(legacy.Length > 0
                    && legacy.Select(value => value.GrammarKey)
                        .Distinct(StringComparer.Ordinal).Count()
                    == legacy.Length,
                "FORMAL_CAUSAL_LEGACY_GRAMMAR_SOURCE_INVALID");

            C1FormalItemPresentationRowSnapshot[] ordinaryRows = catalog.Rows
                .Where(value => value != null
                    && !value.isSpecialLightingSource)
                .OrderBy(value => value.effectFamilyKey,
                    StringComparer.Ordinal)
                .ThenBy(value => value.rarityVersionIdentity,
                    StringComparer.Ordinal)
                .ToArray();
            Dictionary<string, FormalBattleCausalItemStyle> legacyByBaseId =
                legacy.ToDictionary(
                    value => value.GrammarKey,
                    value => value,
                    StringComparer.Ordinal);
            Require(ordinaryRows.Count(value => legacyByBaseId.ContainsKey(
                        value.baseItemId)) == legacy.Length,
                "FORMAL_CAUSAL_LEGACY_CATALOG_LINEAGE_MISMATCH");

            List<FormalBattleCausalItemStyle> grammars = new();
            foreach (IGrouping<string, C1FormalItemPresentationRowSnapshot>
                     family in ordinaryRows.GroupBy(
                         value => value.effectFamilyKey,
                         StringComparer.Ordinal))
            {
                C1FormalItemPresentationRowSnapshot[] familyRows = family
                    .Where(value => legacyByBaseId.ContainsKey(
                        value.baseItemId))
                    .ToArray();
                Require(!string.IsNullOrWhiteSpace(family.Key)
                        && familyRows.Length > 0,
                    "FORMAL_CAUSAL_GRAMMAR_FAMILY_SOURCE_MISSING "
                    + family.Key);
                FormalBattleCausalItemStyle fallbackSource =
                    legacyByBaseId[familyRows[0].baseItemId];
                string fallbackSignature = CausalVisualSignature(
                    fallbackSource);
                grammars.Add(CloneCausalGrammar(
                    fallbackSource,
                    family.Key,
                    string.Empty,
                    family.Key,
                    string.Empty,
                    string.Empty,
                    string.Empty));

                foreach (C1FormalItemPresentationRowSnapshot row
                         in familyRows.Skip(1))
                {
                    FormalBattleCausalItemStyle source =
                        legacyByBaseId[row.baseItemId];
                    if (string.Equals(
                            CausalVisualSignature(source),
                            fallbackSignature,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }
                    grammars.Add(CloneCausalGrammar(
                        source,
                        string.Join("|", new[]
                        {
                            family.Key,
                            row.presentationStyleKey,
                            row.cueIdentity
                        }),
                        row.presentationStyleKey,
                        family.Key,
                        row.cueIdentity,
                        string.Empty,
                        string.Empty));
                }
            }

            FormalBattleCausalItemStyle[] result = grammars.ToArray();
            Require(result.Length > 0
                    && result.All(value => value != null && value.Validate())
                    && result.Select(value => value.GrammarKey)
                        .Distinct(StringComparer.Ordinal).Count()
                    == result.Length,
                "FORMAL_CAUSAL_DYNAMIC_GRAMMAR_INVALID");
            return result;
        }

        private static C1FormalItemPresentationCatalogSnapshot
            ResolveAuthoredItemPresentationCatalog()
        {
            string[] catalogGuids = AssetDatabase.FindAssets(
                "t:C1FormalItemPresentationCatalog");
            Require(catalogGuids.Length == 1,
                "FORMAL_CAUSAL_AUTHORED_CATALOG_COUNT_INVALID "
                + catalogGuids.Length);
            string path = AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
            C1FormalItemPresentationCatalog catalog =
                RequireAssetAtPath<C1FormalItemPresentationCatalog>(path);
            C1FormalItemPresentationCatalogResult result = catalog.Resolve();
            Require(result != null
                    && result.accepted
                    && result.snapshot != null,
                "FORMAL_CAUSAL_AUTHORED_CATALOG_REJECTED "
                + (result?.diagnosticCode ?? "NULL"));
            return result.snapshot;
        }

        private static ProfilePreservationSnapshot CaptureProfilePreservation(
            FormalBattlePresentationProfile profile)
        {
            Require(profile != null,
                "FORMAL_PROFILE_PRESERVATION_SOURCE_MISSING");
            Require(profile.TryGetEnemyProfile(
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    out FormalBattleEnemyVisualProfile host),
                "FORMAL_PROFILE_PRESERVATION_HOST_MISSING");
            Require(profile.TryGetEnemyProfile(
                    C1EnemyRuntimeContract.PorcelainHoundContentId,
                    C1EnemyRuntimeContract.PorcelainHoundProfileId,
                    out FormalBattleEnemyVisualProfile hound),
                "FORMAL_PROFILE_PRESERVATION_HOUND_MISSING");
            FormalBattleCausalItemStyle[] causalStyles = profile
                .GetCausalStylesForEditor();
            Require(causalStyles.Length > 0
                    && causalStyles.All(value => value != null
                        && value.ValidateVisualGrammarForEditor()),
                "FORMAL_PROFILE_PRESERVATION_CAUSAL_GRAMMAR_MISSING");
            return new ProfilePreservationSnapshot
            {
                HostSignature = EnemyProfileSignature(host),
                HoundSignature = EnemyProfileSignature(hound),
                CausalVisualSignatures = causalStyles
                    .Select(CausalVisualSignature)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                WindupBits = FloatBits(profile.CausalWindupDuration),
                SourceHoldBits = FloatBits(profile.CausalSourceHoldDuration),
                RibbonBits = FloatBits(profile.CausalRibbonDuration)
            };
        }

        private static void ValidateProfilePreservation(
            FormalBattlePresentationProfile profile,
            ProfilePreservationSnapshot expected)
        {
            Require(profile != null && expected != null,
                "FORMAL_PROFILE_PRESERVATION_EVIDENCE_MISSING");
            Require(profile.TryGetEnemyProfile(
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    out FormalBattleEnemyVisualProfile host)
                    && string.Equals(
                        EnemyProfileSignature(host),
                        expected.HostSignature,
                        StringComparison.Ordinal),
                "FORMAL_PROFILE_EXISTING_HOST_CHANGED");
            Require(profile.TryGetEnemyProfile(
                    C1EnemyRuntimeContract.PorcelainHoundContentId,
                    C1EnemyRuntimeContract.PorcelainHoundProfileId,
                    out FormalBattleEnemyVisualProfile hound)
                    && string.Equals(
                        EnemyProfileSignature(hound),
                        expected.HoundSignature,
                        StringComparison.Ordinal),
                "FORMAL_PROFILE_EXISTING_HOUND_CHANGED");
            string[] actualCausalVisualSignatures = profile
                .GetCausalStylesForEditor()
                .Where(value => value != null
                    && value.ValidateVisualGrammarForEditor())
                .Select(CausalVisualSignature)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Require(actualCausalVisualSignatures.SequenceEqual(
                    expected.CausalVisualSignatures
                    ?? Array.Empty<string>(),
                    StringComparer.Ordinal),
                "FORMAL_PROFILE_EXISTING_CAUSAL_VISUAL_GRAMMAR_CHANGED");
            Require(FloatBits(profile.CausalWindupDuration)
                    == expected.WindupBits
                    && FloatBits(profile.CausalSourceHoldDuration)
                    == expected.SourceHoldBits
                    && FloatBits(profile.CausalRibbonDuration)
                    == expected.RibbonBits,
                "FORMAL_PROFILE_GLOBAL_CAUSAL_TIMING_CHANGED");
        }

        private static FormalBattleCausalItemStyle CloneCausalGrammar(
            FormalBattleCausalItemStyle source)
        {
            return CloneCausalGrammar(
                source,
                source.GrammarKey,
                source.PresentationStyleKey,
                source.EffectFamilyKey,
                source.CueIdentity,
                source.CueKind,
                source.EffectVariantId);
        }

        private static FormalBattleCausalItemStyle CloneCausalGrammar(
            FormalBattleCausalItemStyle source,
            string grammarKey,
            string presentationStyleKey,
            string effectFamilyKey,
            string cueIdentity,
            string cueKind,
            string effectVariantId)
        {
            Require(source != null
                    && source.ValidateVisualGrammarForEditor(),
                "FORMAL_CAUSAL_GRAMMAR_CLONE_SOURCE_INVALID");
            FormalBattleCausalItemStyle clone =
                new FormalBattleCausalItemStyle();
            clone.AssignForEditor(
                grammarKey,
                presentationStyleKey,
                effectFamilyKey,
                cueIdentity,
                cueKind,
                effectVariantId,
                source.SourcePulseColor,
                source.ProxyHaloColor,
                source.RibbonCoreColor,
                source.RibbonGlowColor,
                source.RibbonSegmentCount,
                source.RibbonWaveAmplitude,
                source.RibbonWaveCycles,
                source.RibbonCoreWidth,
                source.RibbonGlowWidth,
                source.SourcePulseCadence);
            Require(clone.Validate()
                    && string.Equals(
                        CausalVisualSignature(clone),
                        CausalVisualSignature(source),
                        StringComparison.Ordinal),
                "FORMAL_CAUSAL_GRAMMAR_CLONE_CHANGED " + grammarKey);
            return clone;
        }

        private static string EnemyProfileSignature(
            FormalBattleEnemyVisualProfile profile)
        {
            Require(profile != null && profile.Validate(),
                "FORMAL_ENEMY_PROFILE_SIGNATURE_SOURCE_INVALID");
            return string.Join(
                "|",
                profile.ContentId,
                profile.RuntimeProfileId,
                profile.DisplayName,
                AnimationClipSignature(profile.Idle),
                AnimationClipSignature(profile.Attack),
                AnimationClipSignature(profile.Hit),
                AnimationClipSignature(profile.Death));
        }

        private static string AnimationClipSignature(
            FormalBattleAnimationClip clip)
        {
            Require(clip != null && clip.Frames.Length > 0,
                "FORMAL_ANIMATION_CLIP_SIGNATURE_SOURCE_INVALID");
            RectInt reference = clip.ReferencePixelBounds;
            RectInt union = clip.UnionPixelBounds;
            StringBuilder builder = new StringBuilder();
            builder.Append(clip.ClipId).Append('|')
                .Append(clip.FrameDurationMilliseconds).Append('|')
                .Append(clip.Loops ? 1 : 0).Append('|')
                .Append(reference.x).Append(',').Append(reference.y)
                .Append(',').Append(reference.width).Append(',')
                .Append(reference.height).Append('|')
                .Append(union.x).Append(',').Append(union.y)
                .Append(',').Append(union.width).Append(',')
                .Append(union.height);
            foreach (Texture2D frame in clip.Frames)
            {
                builder.Append('|').Append(AssetDatabase.GetAssetPath(frame));
            }
            return builder.ToString();
        }

        private static string CausalVisualSignature(
            FormalBattleCausalItemStyle style)
        {
            Require(style != null
                    && style.ValidateVisualGrammarForEditor(),
                "FORMAL_CAUSAL_GRAMMAR_SIGNATURE_SOURCE_INVALID");
            return string.Join(
                "|",
                ColorSignature(style.SourcePulseColor),
                ColorSignature(style.ProxyHaloColor),
                ColorSignature(style.RibbonCoreColor),
                ColorSignature(style.RibbonGlowColor),
                style.RibbonSegmentCount.ToString(),
                FloatBits(style.RibbonWaveAmplitude).ToString(),
                FloatBits(style.RibbonWaveCycles).ToString(),
                FloatBits(style.RibbonCoreWidth).ToString(),
                FloatBits(style.RibbonGlowWidth).ToString(),
                FloatBits(style.SourcePulseCadence).ToString());
        }

        private static string ColorSignature(Color value)
        {
            return FloatBits(value.r) + "," + FloatBits(value.g) + ","
                   + FloatBits(value.b) + "," + FloatBits(value.a);
        }

        private static int FloatBits(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        private static void AuthorDeterministicAudioAssets()
        {
            WriteMonoPcm16Wav(
                SourcePulseAudioPath,
                BuildDeterministicWaveform(2646, 0));
            WriteMonoPcm16Wav(
                EnemyAttackAudioPath,
                BuildDeterministicWaveform(4410, 1));
            WriteMonoPcm16Wav(
                ImpactAudioPath,
                BuildDeterministicWaveform(3308, 2));
            ValidateMonoPcm16Wav(SourcePulseAudioPath, 2646);
            ValidateMonoPcm16Wav(EnemyAttackAudioPath, 4410);
            ValidateMonoPcm16Wav(ImpactAudioPath, 3308);
        }

        private static float[] BuildDeterministicWaveform(
            int sampleCount,
            int role)
        {
            const int sampleRate = 22050;
            float[] values = new float[sampleCount];
            uint noiseState = unchecked(
                0x9E3779B9u + (uint)(role * 0x45D9F3B));
            double filteredNoise = 0d;
            double peak = 0d;
            for (int index = 0; index < sampleCount; index++)
            {
                double time = index / (double)sampleRate;
                double progress = sampleCount <= 1
                    ? 1d
                    : index / (double)(sampleCount - 1);
                noiseState = unchecked(
                    noiseState * 1664525u + 1013904223u);
                double noise = ((noiseState >> 8) & 0xFFFF) / 32767.5d
                               - 1d;
                double sample;
                if (role == 0)
                {
                    filteredNoise = filteredNoise * 0.68d + noise * 0.32d;
                    double envelope = Math.Min(1d, progress / 0.055d)
                                      * Math.Pow(1d - progress, 1.65d)
                                      * Math.Exp(-0.9d * progress);
                    double pitch = 980d + 520d * progress;
                    sample = envelope * (
                        0.58d * Math.Sin(2d * Math.PI * pitch * time)
                        + 0.42d * filteredNoise);
                }
                else if (role == 1)
                {
                    filteredNoise = filteredNoise * 0.91d + noise * 0.09d;
                    double envelope = Math.Pow(
                        Math.Sin(Math.PI * progress),
                        0.72d);
                    double pitch = 175d - 78d * progress;
                    sample = envelope * (
                        0.62d * filteredNoise
                        + 0.38d * Math.Sin(
                            2d * Math.PI * pitch * time));
                }
                else
                {
                    filteredNoise = filteredNoise * 0.48d + noise * 0.52d;
                    double body = Math.Exp(-5.4d * progress);
                    double transient = Math.Exp(-24d * progress);
                    double pitch = 126d - 32d * progress;
                    sample = body * (
                        0.72d * Math.Sin(2d * Math.PI * pitch * time)
                        + 0.18d * Math.Sin(
                            2d * Math.PI * pitch * 2.03d * time))
                        + transient * 0.28d * filteredNoise;
                }
                values[index] = (float)sample;
                peak = Math.Max(peak, Math.Abs(sample));
            }

            Require(peak > 0.0001d,
                "FORMAL_PRESENTATION_AUDIO_WAVEFORM_SILENT role=" + role);
            float targetPeak = role == 1 ? 0.44f : 0.46f;
            float gain = targetPeak / (float)peak;
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = Mathf.Clamp(
                    values[index] * gain,
                    -targetPeak,
                    targetPeak);
            }
            return values;
        }

        private static void WriteMonoPcm16Wav(
            string path,
            IReadOnlyList<float> samples)
        {
            const int sampleRate = 22050;
            const short channelCount = 1;
            const short bitsPerSample = 16;
            int dataBytes = samples.Count * sizeof(short);
            using MemoryStream memory = new MemoryStream(44 + dataBytes);
            using (BinaryWriter writer = new BinaryWriter(memory))
            {
                writer.Write(new[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
                writer.Write(36 + dataBytes);
                writer.Write(new[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });
                writer.Write(new[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write(channelCount);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channelCount * bitsPerSample / 8);
                writer.Write((short)(channelCount * bitsPerSample / 8));
                writer.Write(bitsPerSample);
                writer.Write(new[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
                writer.Write(dataBytes);
                foreach (float sample in samples)
                {
                    float clamped = Mathf.Clamp(sample, -0.5f, 0.5f);
                    writer.Write((short)Math.Round(
                        clamped * short.MaxValue,
                        MidpointRounding.AwayFromZero));
                }
            }
            File.WriteAllBytes(path, memory.ToArray());
        }

        private static void ValidateMonoPcm16Wav(
            string path,
            int expectedSampleCount)
        {
            Require(File.Exists(path),
                "FORMAL_PRESENTATION_AUDIO_FILE_MISSING " + path);
            byte[] bytes = File.ReadAllBytes(path);
            Require(bytes.Length == 44 + expectedSampleCount * 2
                    && bytes[0] == (byte)'R'
                    && bytes[1] == (byte)'I'
                    && bytes[2] == (byte)'F'
                    && bytes[3] == (byte)'F'
                    && bytes[8] == (byte)'W'
                    && bytes[9] == (byte)'A'
                    && bytes[10] == (byte)'V'
                    && bytes[11] == (byte)'E'
                    && BitConverter.ToInt16(bytes, 20) == 1
                    && BitConverter.ToInt16(bytes, 22) == 1
                    && BitConverter.ToInt32(bytes, 24) == 22050
                    && BitConverter.ToInt16(bytes, 34) == 16
                    && BitConverter.ToInt32(bytes, 40)
                    == expectedSampleCount * 2,
                "FORMAL_PRESENTATION_AUDIO_PCM_CONTRACT_INVALID " + path);
            int peak = 0;
            for (int offset = 44; offset < bytes.Length; offset += 2)
            {
                peak = Math.Max(
                    peak,
                    Math.Abs((int)BitConverter.ToInt16(bytes, offset)));
            }
            Require(peak >= 1024 && peak <= 16384,
                "FORMAL_PRESENTATION_AUDIO_PEAK_INVALID " + path
                + " peak=" + peak);
        }

        private static FormalBattleAnimationClip SequenceClip(
            string clipId,
            string folder,
            int frameDurationMilliseconds,
            bool loops,
            RectInt referenceBounds,
            RectInt unionBounds,
            params int[] ordinals)
        {
            Texture2D[] frames = (ordinals ?? Array.Empty<int>())
                .Select(value => AssetDatabase.LoadAssetAtPath<Texture2D>(
                    folder + "/frame_" + value.ToString("D3") + ".png"))
                .ToArray();
            Require(frames.Length > 0 && frames.All(value => value != null),
                "FORMAL_PRESENTATION_FRAME_SEQUENCE_MISSING " + clipId);
            FormalBattleAnimationClip clip =
                new FormalBattleAnimationClip();
            clip.AssignForEditor(
                clipId,
                frames,
                frameDurationMilliseconds,
                loops,
                referenceBounds,
                unionBounds);
            return clip;
        }

        private static FormalBattleAnimationClip SingleTextureClip(
            string clipId,
            string path,
            bool loops,
            RectInt bounds)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Require(texture != null,
                "FORMAL_PRESENTATION_SINGLE_TEXTURE_MISSING " + path);
            FormalBattleAnimationClip clip =
                new FormalBattleAnimationClip();
            clip.AssignForEditor(
                clipId,
                new[] { texture },
                350,
                loops,
                bounds,
                bounds);
            return clip;
        }

        private static void AddPlayerNianBarFromCurrentHpLayout()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PlayerPrefabPath);
            try
            {
                FormalBattlePlayerPresentationView view =
                    root.GetComponent<FormalBattlePlayerPresentationView>();
                Require(view != null, "FORMAL_PLAYER_VIEW_MISSING");

                DestroyDirectChildIfPresent(root.transform,
                    "PlayerNianBack");
                DestroyDirectChildIfPresent(root.transform,
                    "PlayerNianLabel");

                Image hpBack = FindNamedComponent<Image>(
                    root,
                    "PlayerHpBack");
                TMP_Text hpLabel = FindNamedComponent<TMP_Text>(
                    root,
                    "PlayerHpLabel");
                GameObject nianRoot = UnityEngine.Object.Instantiate(
                    hpBack.gameObject,
                    root.transform);
                nianRoot.name = "PlayerNianBack";
                RectTransform nianRect = nianRoot.transform as RectTransform;
                float verticalStep = Mathf.Abs(
                    hpBack.rectTransform.rect.height
                    * hpBack.rectTransform.localScale.y) + 6f;
                nianRect.anchoredPosition = hpBack.rectTransform.anchoredPosition
                    + Vector2.down * verticalStep;

                Image nianFill = nianRoot
                    .GetComponentsInChildren<Image>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "PlayerHpFill",
                        StringComparison.Ordinal));
                nianFill.gameObject.name = "PlayerNianFill";
                nianFill.sprite = RequireGuidAsset<Sprite>(ManaFillGuid);
                nianFill.color = Color.white;
                nianFill.type = Image.Type.Filled;
                nianFill.fillMethod = Image.FillMethod.Horizontal;
                nianFill.fillOrigin = 0;
                nianFill.fillAmount = 1f;
                nianFill.raycastTarget = false;

                TMP_Text nianLabel = UnityEngine.Object.Instantiate(
                    hpLabel,
                    root.transform);
                nianLabel.gameObject.name = "PlayerNianLabel";
                nianLabel.rectTransform.anchoredPosition =
                    hpLabel.rectTransform.anchoredPosition
                    + Vector2.down * verticalStep;
                nianLabel.text = string.Empty;
                nianLabel.raycastTarget = false;

                view.AssignNianForEditor(
                    nianRoot,
                    nianFill,
                    nianLabel);
                nianRoot.SetActive(false);
                EditorUtility.SetDirty(view);
                Require(view.ValidateAuthoredReferences(),
                    "FORMAL_PLAYER_NIAN_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConvertEnemyPrefabToActorStage()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                EnemyPrefabPath);
            try
            {
                FormalBattleEnemySlotView view =
                    root.GetComponent<FormalBattleEnemySlotView>();
                Require(view != null, "FORMAL_ENEMY_ACTOR_STAGE_VIEW_MISSING");

                foreach (string childName in new[]
                         {
                             "EnemyPortraitFrame",
                             "EnemyIdentity",
                             "EnemyHpBack",
                             "EnemyHpLabel",
                             "EnemyShellReadout",
                             "EnemyStatusStrip"
                         })
                {
                    DestroyNamedDescendantIfPresent(root, childName);
                }

                Image visual = FindNamedComponent<Image>(
                    root,
                    "AuthoritativeEnemyVisual");
                RawImage telegraph = FindNamedComponent<RawImage>(
                    root,
                    "RestrainedAttackTelegraph");
                visual.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                visual.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                visual.rectTransform.anchoredPosition = Vector2.zero;
                telegraph.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                telegraph.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                telegraph.rectTransform.anchoredPosition = Vector2.zero;

                EditorUtility.SetDirty(view);
                Require(view.ValidateAuthoredReferences(),
                    "FORMAL_ENEMY_ACTOR_STAGE_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RecomposeSelectedEnemyHud(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                DestroyDirectChildIfPresent(root.transform,
                    "SelectedEnemyHud");

                FormalBattlePresentationRoot presenter =
                    root.GetComponent<FormalBattlePresentationRoot>();
                FormalBattlePlayerPresentationView player = root
                    .GetComponentsInChildren<
                        FormalBattlePlayerPresentationView>(true)
                    .Single();
                FormalBattleEnemySlotView[] slots = root
                    .GetComponentsInChildren<FormalBattleEnemySlotView>(true)
                    .OrderBy(value => value.AuthoredStableOrder)
                    .ToArray();
                FormalBattleCausalSourceVisualView causalSourceView = root
                    .GetComponentInChildren<
                        FormalBattleCausalSourceVisualView>(true);
                TMP_Text stageLabel = root
                    .GetComponentsInChildren<TMP_Text>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "AuthoritativeStageLabel",
                        StringComparison.Ordinal));
                Require(presenter != null
                        && slots.Length == 3
                        && causalSourceView != null,
                    "FORMAL_PRESENTATION_ROOT_BASE_STRUCTURE_INVALID");

                GameObject selected = (GameObject)PrefabUtility
                    .InstantiatePrefab(RequirePrefab(PlayerPrefabPath),
                        root.transform);
                if (PrefabUtility.IsPartOfPrefabInstance(selected))
                {
                    PrefabUtility.UnpackPrefabInstance(
                        selected,
                        PrefabUnpackMode.Completely,
                        InteractionMode.AutomatedAction);
                }
                selected.name = "SelectedEnemyHud";
                CopyRectTransform(
                    player.transform as RectTransform,
                    selected.transform as RectTransform);
                MirrorRectTransformHorizontally(
                    selected.transform as RectTransform);

                FormalBattlePlayerPresentationView copiedPlayerView =
                    selected.GetComponent<FormalBattlePlayerPresentationView>();
                if (copiedPlayerView != null)
                {
                    UnityEngine.Object.DestroyImmediate(copiedPlayerView);
                }
                foreach (string childName in new[]
                         {
                             "PlayerNianBack",
                             "PlayerNianLabel",
                             "PlayerAvatarGlyph",
                             "VermilionAccent"
                         })
                {
                    DestroyDirectChildIfPresent(
                        selected.transform,
                        childName);
                }
                Transform copiedHitOverlay = selected
                    .GetComponentsInChildren<Transform>(true)
                    .SingleOrDefault(value => string.Equals(
                        value.gameObject.name,
                        "PlayerHitOverlay",
                        StringComparison.Ordinal));
                if (copiedHitOverlay != null)
                {
                    UnityEngine.Object.DestroyImmediate(
                        copiedHitOverlay.gameObject);
                }

                foreach (Transform directChild in selected.transform
                             .Cast<Transform>())
                {
                    MirrorRectTransformHorizontally(
                        directChild as RectTransform);
                }

                Image portrait = FindNamedComponent<Image>(
                    selected,
                    "AcceptedPlayerAvatar");
                portrait.gameObject.name = "SelectedEnemyPortrait";
                portrait.sprite = null;
                portrait.color = Color.white;
                portrait.preserveAspect = true;
                Image portraitFrame = CreateImage(
                    "SelectedEnemyPortraitFrame",
                    portrait.transform,
                    Color.white);
                portraitFrame.sprite = RequireGuidAsset<Sprite>(
                    EnemyPortraitFrameGuid);
                portraitFrame.preserveAspect = true;
                Stretch(portraitFrame.rectTransform,
                    Vector2.zero,
                    Vector2.zero);

                TMP_Text identity = FindNamedComponent<TMP_Text>(
                    selected,
                    "PlayerIdentity");
                identity.gameObject.name = "SelectedEnemyIdentity";
                identity.text = string.Empty;

                Image hpBack = FindNamedComponent<Image>(
                    selected,
                    "PlayerHpBack");
                hpBack.gameObject.name = "SelectedEnemyHpBack";
                Image hpFill = FindNamedComponent<Image>(
                    selected,
                    "PlayerHpFill");
                hpFill.gameObject.name = "SelectedEnemyHpFill";
                hpFill.sprite = RequireGuidAsset<Sprite>(HpFillGuid);
                hpFill.fillOrigin = 1;
                TMP_Text hpLabel = FindNamedComponent<TMP_Text>(
                    selected,
                    "PlayerHpLabel");
                hpLabel.gameObject.name = "SelectedEnemyHpLabel";
                hpLabel.text = string.Empty;

                Transform shellTransform = FindNamedTransform(
                    selected,
                    "PlayerGuardReadout");
                shellTransform.gameObject.name =
                    "SelectedEnemyShellReadout";
                TMP_Text shellLabel = FindNamedComponent<TMP_Text>(
                    selected,
                    "PlayerGuardLabel");
                shellLabel.gameObject.name = "SelectedEnemyShellLabel";
                shellLabel.text = string.Empty;
                Image shellIcon = ConfigureSelectedEnemyShellIcon(
                    shellTransform,
                    shellLabel);
                shellTransform.gameObject.SetActive(false);

                FormalBattleStatusStripView statusStrip = selected
                    .GetComponentsInChildren<
                        FormalBattleStatusStripView>(true)
                    .Single();
                statusStrip.gameObject.name = "SelectedEnemyStatusStrip";

                FormalBattleSelectedEnemyHudView selectedHud =
                    selected.AddComponent<FormalBattleSelectedEnemyHudView>();
                selectedHud.AssignForEditor(
                    portrait,
                    identity,
                    hpFill,
                    hpLabel,
                    shellTransform.gameObject,
                    shellIcon,
                    shellLabel,
                    statusStrip);
                Require(selectedHud.ValidateAuthoredReferences(),
                    "FORMAL_SELECTED_ENEMY_HUD_REFERENCE_INVALID");
                selected.SetActive(false);

                presenter.AssignSourceCarrierForEditor(
                    profile,
                    player,
                    slots,
                    selectedHud,
                    causalSourceView,
                    stageLabel);
                EditorUtility.SetDirty(presenter);
                Require(presenter.ValidateSourceCarrierReferences(out _),
                    "FORMAL_PRESENTATION_SEPARATED_SOURCE_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    PresentationRootPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateEnemyHudActorStageSeparation()
        {
            GameObject player = PrefabUtility.LoadPrefabContents(
                PlayerPrefabPath);
            try
            {
                FormalBattlePlayerPresentationView view =
                    player.GetComponent<FormalBattlePlayerPresentationView>();
                Require(view != null
                        && view.ValidateAuthoredReferences()
                        && player.GetComponentsInChildren<Transform>(true)
                            .Count(value => string.Equals(
                                value.gameObject.name,
                                "PlayerNianBack",
                                StringComparison.Ordinal)) == 1,
                    "FORMAL_PLAYER_NIAN_AUTHORED_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(player);
            }

            GameObject enemy = PrefabUtility.LoadPrefabContents(
                EnemyPrefabPath);
            try
            {
                FormalBattleEnemySlotView view =
                    enemy.GetComponent<FormalBattleEnemySlotView>();
                string[] retiredHudNames =
                {
                    "EnemyPortraitFrame",
                    "EnemyIdentity",
                    "EnemyHpBack",
                    "EnemyHpLabel",
                    "EnemyShellReadout",
                    "EnemyStatusStrip"
                };
                Require(view != null
                        && view.ValidateAuthoredReferences()
                        && !enemy.GetComponentsInChildren<Transform>(true)
                            .Any(value => retiredHudNames.Contains(
                                value.gameObject.name)),
                    "FORMAL_ENEMY_ACTOR_STAGE_AUTHORED_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(enemy);
            }

            GameObject root = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                FormalBattlePresentationRoot presenter =
                    root.GetComponent<FormalBattlePresentationRoot>();
                FormalBattleSelectedEnemyHudView selectedHud = root
                    .GetComponentsInChildren<
                        FormalBattleSelectedEnemyHudView>(true)
                    .SingleOrDefault();
                FormalBattleEnemySlotView[] actorStages = root
                    .GetComponentsInChildren<FormalBattleEnemySlotView>(true);
                Require(presenter != null
                        && presenter.ValidateSourceCarrierReferences(out _)
                        && selectedHud != null
                        && selectedHud.ValidateAuthoredReferences()
                        && actorStages.Length == 3
                        && actorStages.All(value =>
                            !selectedHud.transform.IsChildOf(value.transform)),
                    "FORMAL_ENEMY_HUD_ACTOR_STAGE_ROOT_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ExtendPlayerReadabilityPrefab(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PlayerPrefabPath);
            try
            {
                FormalBattlePlayerPresentationView view =
                    root.GetComponent<FormalBattlePlayerPresentationView>();
                Require(view != null,
                    "FORMAL_PLAYER_READABILITY_VIEW_MISSING");

                RectTransform rootRect = root.transform as RectTransform;
                rootRect.sizeDelta = new Vector2(420f, 560f);
                Image rootImage = root.GetComponent<Image>();
                Require(rootImage != null,
                    "FORMAL_PLAYER_ROOT_IMAGE_MISSING");
                rootImage.color = Color.clear;
                rootImage.raycastTarget = false;
                Outline rootOutline = root.GetComponent<Outline>();
                if (rootOutline != null) rootOutline.enabled = false;
                DestroyDirectChildIfPresent(root.transform,
                    "VermilionAccent");

                Image avatar = FindNamedComponent<Image>(
                    root,
                    "AcceptedPlayerAvatar");
                avatar.sprite = profile.PlayerAvatarSprite;
                avatar.color = Color.white;
                avatar.preserveAspect = true;
                Pin(
                    avatar.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 32f),
                    new Vector2(360f, 492f));

                DestroyDirectChildIfPresent(root.transform,
                    "PlayerAvatarGlyph");
                TMP_Text avatarGlyph = CreateText(
                    "PlayerAvatarGlyph",
                    root.transform,
                    profile.DamageFont,
                    "修",
                    76f,
                    new Color(0.24f, 0.16f, 0.08f, 0.92f));
                avatarGlyph.fontStyle = FontStyles.Bold;
                Pin(
                    avatarGlyph.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 54f),
                    new Vector2(210f, 210f));

                TMP_Text identity = FindNamedComponent<TMP_Text>(
                    root,
                    "PlayerIdentity");
                identity.text = "修士";
                identity.fontSize = 26f;
                identity.color = new Color(0.26f, 0.16f, 0.07f, 1f);
                identity.fontStyle = FontStyles.Bold;
                Pin(
                    identity.rectTransform,
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -36f),
                    new Vector2(260f, 42f));

                Image hpBack = FindNamedComponent<Image>(
                    root,
                    "PlayerHpBack");
                hpBack.sprite = profile.HpFrameSprite;
                hpBack.color = Color.white;
                hpBack.type = Image.Type.Sliced;
                Pin(
                    hpBack.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -130f),
                    new Vector2(350f, 70f));
                Image hpFill = FindNamedComponent<Image>(
                    root,
                    "PlayerHpFill");
                hpFill.sprite = RequireAssetAtPath<Sprite>(HpFillSpritePath);
                hpFill.color = Color.white;
                hpFill.type = Image.Type.Filled;
                hpFill.fillMethod = Image.FillMethod.Horizontal;
                hpFill.fillOrigin = 0;
                Stretch(
                    hpFill.rectTransform,
                    new Vector2(34f, 20f),
                    new Vector2(-34f, -20f));

                TMP_Text hpLabel = FindNamedComponent<TMP_Text>(
                    root,
                    "PlayerHpLabel");
                hpLabel.fontSize = 24f;
                hpLabel.fontStyle = FontStyles.Bold;
                hpLabel.color = new Color(1f, 0.93f, 0.78f, 1f);
                Pin(
                    hpLabel.rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -130f),
                    new Vector2(300f, 42f));

                DestroyDirectChildIfPresent(root.transform,
                    "PlayerGuardReadout");
                DestroyDirectChildIfPresent(root.transform,
                    "PlayerStatusStrip");

                GameObject guardRoot = NewUiObject(
                    "PlayerGuardReadout",
                    root.transform);
                Image guardFrame = AddImage(guardRoot, Color.white);
                guardFrame.sprite = profile.HpFrameSprite;
                guardFrame.type = Image.Type.Sliced;
                Pin(
                    guardRoot.transform as RectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, -184f),
                    new Vector2(350f, 42f));
                TMP_Text guardLabel = CreateText(
                    "PlayerGuardLabel",
                    guardRoot.transform,
                    profile.DamageFont,
                    "\u62a4\u76fe 0",
                    18f,
                    new Color(0.72f, 0.91f, 1f, 1f));
                guardLabel.fontStyle = FontStyles.Bold;
                Stretch(
                    guardLabel.rectTransform,
                    new Vector2(28f, 4f),
                    new Vector2(-28f, -4f));

                FormalBattleStatusStripView statusStrip =
                    CreateStatusStrip(
                        "PlayerStatusStrip",
                        root.transform,
                        profile,
                        new Vector2(0f, -248f));
                (statusStrip.transform as RectTransform).sizeDelta =
                    new Vector2(350f, 94f);
                view.AssignReadabilityForEditor(
                    guardRoot,
                    guardLabel,
                    statusStrip);
                EditorUtility.SetDirty(view);
                Require(view.ValidateAuthoredReferences(),
                    "FORMAL_PLAYER_READABILITY_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ExtendEnemyReadabilityPrefab(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                EnemyPrefabPath);
            try
            {
                FormalBattleEnemySlotView view =
                    root.GetComponent<FormalBattleEnemySlotView>();
                Require(view != null,
                    "FORMAL_ENEMY_READABILITY_VIEW_MISSING");

                RectTransform rootRect = root.transform as RectTransform;
                rootRect.sizeDelta = new Vector2(440f, 210f);
                Image rootImage = root.GetComponent<Image>();
                Require(rootImage != null,
                    "FORMAL_ENEMY_ROOT_IMAGE_MISSING");
                rootImage.color = Color.clear;
                rootImage.raycastTarget = false;
                Outline rootOutline = root.GetComponent<Outline>();
                if (rootOutline != null) rootOutline.enabled = false;

                Image visual = FindNamedComponent<Image>(
                    root,
                    "AuthoritativeEnemyVisual");
                Pin(
                    visual.rectTransform,
                    new Vector2(1f, 0.5f),
                    new Vector2(-86f, 0f),
                    new Vector2(164f, 202f));
                RawImage telegraph = FindNamedComponent<RawImage>(
                    root,
                    "RestrainedAttackTelegraph");
                Pin(
                    telegraph.rectTransform,
                    new Vector2(1f, 0.5f),
                    new Vector2(-86f, 0f),
                    new Vector2(174f, 210f));

                DestroyDirectChildIfPresent(root.transform,
                    "EnemyPortraitFrame");
                Image portraitFrame = CreateImage(
                    "EnemyPortraitFrame",
                    root.transform,
                    Color.white);
                portraitFrame.sprite = RequireAssetAtPath<Sprite>(
                    EnemyPortraitFramePath);
                portraitFrame.preserveAspect = true;
                Pin(
                    portraitFrame.rectTransform,
                    new Vector2(1f, 0.5f),
                    new Vector2(-86f, 0f),
                    new Vector2(176f, 210f));

                TMP_Text identity = FindNamedComponent<TMP_Text>(
                    root,
                    "EnemyIdentity");
                identity.fontSize = 22f;
                identity.fontStyle = FontStyles.Bold;
                identity.color = new Color(1f, 0.86f, 0.58f, 1f);
                Pin(
                    identity.rectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(138f, -22f),
                    new Vector2(250f, 34f));

                Image hpBack = FindNamedComponent<Image>(
                    root,
                    "EnemyHpBack");
                hpBack.sprite = profile.HpFrameSprite;
                hpBack.color = Color.white;
                hpBack.type = Image.Type.Sliced;
                Pin(
                    hpBack.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(138f, 30f),
                    new Vector2(270f, 54f));
                Image hpFill = FindNamedComponent<Image>(
                    root,
                    "EnemyHpFill");
                hpFill.sprite = RequireAssetAtPath<Sprite>(HpFillSpritePath);
                hpFill.color = Color.white;
                hpFill.type = Image.Type.Filled;
                hpFill.fillMethod = Image.FillMethod.Horizontal;
                hpFill.fillOrigin = 1;
                Stretch(
                    hpFill.rectTransform,
                    new Vector2(28f, 15f),
                    new Vector2(-28f, -15f));

                TMP_Text hpLabel = FindNamedComponent<TMP_Text>(
                    root,
                    "EnemyHpLabel");
                hpLabel.fontSize = 18f;
                hpLabel.fontStyle = FontStyles.Bold;
                hpLabel.color = new Color(1f, 0.93f, 0.78f, 1f);
                Pin(
                    hpLabel.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(138f, 30f),
                    new Vector2(230f, 34f));

                DestroyDirectChildIfPresent(root.transform,
                    "EnemyShellReadout");
                DestroyDirectChildIfPresent(root.transform,
                    "EnemyStatusStrip");

                GameObject shellRoot = NewUiObject(
                    "EnemyShellReadout",
                    root.transform);
                Image shellFrame = AddImage(shellRoot, Color.white);
                shellFrame.sprite = profile.HpFrameSprite;
                shellFrame.type = Image.Type.Sliced;
                Pin(
                    shellRoot.transform as RectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(138f, -13f),
                    new Vector2(270f, 36f));
                Image shellFill = CreateImage(
                    "EnemyShellFill",
                    shellRoot.transform,
                    Color.white);
                shellFill.sprite = RequireAssetAtPath<Sprite>(
                    GuardFillSpritePath);
                Stretch(
                    shellFill.rectTransform,
                    new Vector2(25f, 10f),
                    new Vector2(-25f, -10f));
                shellFill.type = Image.Type.Filled;
                shellFill.fillMethod = Image.FillMethod.Horizontal;
                shellFill.fillOrigin = 1;
                shellFill.fillAmount = 1f;
                TMP_Text shellLabel = CreateText(
                    "EnemyShellLabel",
                    shellRoot.transform,
                    profile.DamageFont,
                    string.Empty,
                    14f,
                    Color.white);
                shellLabel.fontStyle = FontStyles.Bold;
                Stretch(
                    shellLabel.rectTransform,
                    new Vector2(4f, 0f),
                    new Vector2(-4f, 0f));
                shellRoot.SetActive(false);

                FormalBattleStatusStripView statusStrip =
                    CreateStatusStrip(
                        "EnemyStatusStrip",
                        root.transform,
                        profile,
                        new Vector2(138f, -72f));
                view.AssignReadabilityForEditor(
                    shellRoot,
                    shellFill,
                    shellLabel,
                    statusStrip);
                EditorUtility.SetDirty(view);
                Require(view.ValidateAuthoredReferences(),
                    "FORMAL_ENEMY_READABILITY_REFERENCE_INVALID");
                PrefabUtility.SaveAsPrefabAsset(root, EnemyPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static FormalBattleStatusStripView CreateStatusStrip(
            string objectName,
            Transform parent,
            FormalBattlePresentationProfile profile,
            Vector2 anchoredPosition)
        {
            const int slotCount = 8;
            GameObject stripRoot = NewUiObject(objectName, parent);
            Pin(
                stripRoot.transform as RectTransform,
                new Vector2(0.5f, 0.5f),
                anchoredPosition,
                new Vector2(300f, 94f));

            TMP_Text positiveMarker = CreateText(
                "PositiveStatusMarker",
                stripRoot.transform,
                profile.DamageFont,
                "益",
                15f,
                new Color(0.62f, 0.92f, 0.68f, 0.94f));
            Pin(
                positiveMarker.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(-132f, 23f),
                new Vector2(24f, 34f));
            TMP_Text negativeMarker = CreateText(
                "NegativeStatusMarker",
                stripRoot.transform,
                profile.DamageFont,
                "损",
                15f,
                new Color(1f, 0.56f, 0.46f, 0.94f));
            Pin(
                negativeMarker.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(-132f, -23f),
                new Vector2(24f, 34f));

            GameObject[] roots = new GameObject[slotCount];
            Image[] backgrounds = new Image[slotCount];
            Image[] icons = new Image[slotCount];
            TMP_Text[] labels = new TMP_Text[slotCount];
            TMP_Text[] stacks = new TMP_Text[slotCount];
            TMP_Text[] durations = new TMP_Text[slotCount];
            for (int index = 0; index < slotCount; index++)
            {
                GameObject slot = NewUiObject(
                    "StatusSlot_" + index.ToString("D2"),
                    stripRoot.transform);
                roots[index] = slot;
                backgrounds[index] = AddImage(
                    slot,
                    new Color(0.18f, 0.08f, 0.06f, 0.94f));
                int row = index / 4;
                int column = index % 4;
                Pin(
                    slot.transform as RectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-75f + column * 50f,
                        row == 0 ? 23f : -23f),
                    new Vector2(42f, 42f));

                icons[index] = CreateImage(
                    "StatusIcon",
                    slot.transform,
                    Color.white);
                Pin(
                    icons[index].rectTransform,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 2f),
                    new Vector2(26f, 26f));
                icons[index].preserveAspect = true;
                icons[index].enabled = false;

                labels[index] = CreateText(
                    "StatusLabel",
                    slot.transform,
                    profile.DamageFont,
                    string.Empty,
                    22f,
                    Color.white);
                Stretch(
                    labels[index].rectTransform,
                    new Vector2(4f, 4f),
                    new Vector2(-4f, -4f));
                labels[index].fontStyle = FontStyles.Bold;
                stacks[index] = CreateText(
                    "StatusStack",
                    slot.transform,
                    profile.DamageFont,
                    string.Empty,
                    9f,
                    Color.white);
                Pin(
                    stacks[index].rectTransform,
                    new Vector2(1f, 1f),
                    new Vector2(-8f, -7f),
                    new Vector2(18f, 12f));
                durations[index] = CreateText(
                    "StatusDuration",
                    slot.transform,
                    profile.DamageFont,
                    string.Empty,
                    8f,
                    Color.white);
                Pin(
                    durations[index].rectTransform,
                    new Vector2(1f, 0f),
                    new Vector2(-10f, 6f),
                    new Vector2(22f, 10f));
                slot.SetActive(false);
            }

            FormalBattleStatusStripView strip =
                stripRoot.AddComponent<FormalBattleStatusStripView>();
            strip.AssignForEditor(
                roots,
                backgrounds,
                icons,
                labels,
                stacks,
                durations);
            strip.AssignAlignmentForEditor(
                objectName.IndexOf(
                    "Enemy",
                    StringComparison.Ordinal) >= 0
                    ? FormalBattleStatusStripAlignment.Right
                    : FormalBattleStatusStripAlignment.Left);
            Require(strip.ValidateAuthoredReferences(),
                "FORMAL_STATUS_STRIP_AUTHORING_INVALID " + objectName);
            return strip;
        }

        private static void ReorderItemDetailPlayerSections()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                ItemDetailPopupPrefabPath);
            try
            {
                ItemDetailPanelView panel = root.GetComponentInChildren<
                    ItemDetailPanelView>(true);
                Require(panel != null,
                    "ITEM_DETAIL_PLAYER_PANEL_MISSING");
                ConfigureItemDetailPlayerReadability(root, panel);
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    ItemDetailPopupPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureItemDetailPlayerReadability(
            GameObject root,
            ItemDetailPanelView panel)
        {
            Transform stats = FindNamedTransform(
                root,
                "BaseStatsSection");
            Transform trigger = FindNamedTransform(
                root,
                "TriggerConditionSection");
            Transform basic = FindNamedTransform(
                root,
                "BasicEffectSection");
            Transform flavor = FindNamedTransform(
                root,
                "FlavorSection");
            Require(string.Equals(
                        stats.parent.gameObject.name,
                        "ItemDetailFixedBaseStatsRoot",
                        StringComparison.Ordinal)
                    && ReferenceEquals(trigger.parent, basic.parent)
                    && ReferenceEquals(trigger.parent, flavor.parent)
                    && string.Equals(
                        trigger.parent.gameObject.name,
                        "DetailContent",
                        StringComparison.Ordinal),
                "ITEM_DETAIL_PLAYER_SECTION_CARRIER_MISMATCH");

            // Restore the actual BattleSandbox Play hierarchy before making any
            // later readability decisions. Visual parity and semantic editing
            // remain separate passes.
            basic.SetSiblingIndex(0);
            trigger.SetAsLastSibling();
            trigger.SetSiblingIndex(flavor.GetSiblingIndex() + 1);

            ConfigureBattleSandboxParitySection(
                stats,
                new RectOffset(0, 0, 0, 0));
            ConfigureBattleSandboxParitySection(
                basic,
                new RectOffset(0, 0, 0, 0));
            ConfigureBattleSandboxParitySection(
                trigger,
                new RectOffset(22, 18, 12, 14));

            panel.ConfigureStatusBadgeSpritesEditor(
                RequireAssetAtPath<Sprite>(ItemDetailLightingLitIconPath),
                RequireAssetAtPath<Sprite>(ItemDetailLightingUnlitIconPath),
                RequireAssetAtPath<Sprite>(ItemDetailArrayLitIconPath),
                RequireAssetAtPath<Sprite>(ItemDetailArrayUnlitIconPath));
            EditorUtility.SetDirty(FindNamedTransform(
                root,
                "LightingStatusBadge").GetComponent<Image>());
            EditorUtility.SetDirty(FindNamedTransform(
                root,
                "ArrayStatusBadge").GetComponent<Image>());

            SerializedObject serializedPanel =
                new SerializedObject(panel);
            SerializedProperty showDebug = serializedPanel.FindProperty(
                "showDebugTabOnPlayerRoute");
            Require(showDebug != null,
                "ITEM_DETAIL_DEBUG_VISIBILITY_PROPERTY_MISSING");
            showDebug.boolValue = false;
            serializedPanel.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(panel);
        }

        private static void ConfigureBattleSandboxParitySection(
            Transform section,
            RectOffset padding)
        {
            ItemDetailSectionView view = section.GetComponent<
                ItemDetailSectionView>();
            Require(view != null,
                "ITEM_DETAIL_PLAYER_SECTION_VIEW_MISSING " + section.name);
            view.ConfigurePlayerReadabilityEditor(false, string.Empty);

            VerticalLayoutGroup layout = section.GetComponent<
                VerticalLayoutGroup>();
            Require(layout != null,
                "ITEM_DETAIL_PLAYER_SECTION_LAYOUT_MISSING " + section.name);
            layout.padding = padding;
            layout.spacing = 0f;

            EditorUtility.SetDirty(view);
            EditorUtility.SetDirty(layout);
            EditorUtility.SetDirty(section.gameObject);
        }

        private static void ValidateItemDetailPlayerReadability()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                ItemDetailPopupPrefabPath);
            try
            {
                Transform stats = FindNamedTransform(
                    root,
                    "BaseStatsSection");
                Transform trigger = FindNamedTransform(
                    root,
                    "TriggerConditionSection");
                Transform basic = FindNamedTransform(
                    root,
                    "BasicEffectSection");
                Transform flavor = FindNamedTransform(
                    root,
                    "FlavorSection");
                Require(basic.GetSiblingIndex() == 0
                        && trigger.GetSiblingIndex()
                        == flavor.GetSiblingIndex() + 1,
                    "ITEM_DETAIL_PLAYER_READING_ORDER_INVALID");
                ValidateBattleSandboxParitySection(
                    stats,
                    new RectOffset(0, 0, 0, 0));
                ValidateBattleSandboxParitySection(
                    basic,
                    new RectOffset(0, 0, 0, 0));
                ValidateBattleSandboxParitySection(
                    trigger,
                    new RectOffset(22, 18, 12, 14));
                ValidateItemDetailStatusSprites(
                    root.GetComponentInChildren<ItemDetailPanelView>(true));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateBattleSandboxParitySection(
            Transform section,
            RectOffset expectedPadding)
        {
            ItemDetailSectionView view = section.GetComponent<
                ItemDetailSectionView>();
            Require(view != null,
                "ITEM_DETAIL_PLAYER_SECTION_VIEW_MISSING " + section.name);
            SerializedObject serializedView = new SerializedObject(view);
            SerializedProperty showChrome = serializedView.FindProperty(
                "showSectionChrome");
            SerializedProperty titleOverride = serializedView.FindProperty(
                "playerFacingTitleOverride");
            Require(showChrome != null
                    && !showChrome.boolValue
                    && titleOverride != null
                    && string.IsNullOrEmpty(titleOverride.stringValue),
                "ITEM_DETAIL_BATTLESANDBOX_SECTION_STYLE_INVALID "
                + section.name);

            VerticalLayoutGroup layout = section.GetComponent<
                VerticalLayoutGroup>();
            Require(layout != null
                    && layout.padding.left == expectedPadding.left
                    && layout.padding.right == expectedPadding.right
                    && layout.padding.top == expectedPadding.top
                    && layout.padding.bottom == expectedPadding.bottom
                    && Mathf.Approximately(layout.spacing, 0f),
                "ITEM_DETAIL_BATTLESANDBOX_SECTION_LAYOUT_INVALID "
                + section.name);
        }

        private static void ValidateItemDetailStatusSprites(
            ItemDetailPanelView panel)
        {
            Require(panel != null,
                "ITEM_DETAIL_PLAYER_PANEL_MISSING");
            SerializedObject serializedPanel = new SerializedObject(panel);
            foreach (string propertyName in new[]
                     {
                         "lightingStatusLitSprite",
                         "lightingStatusUnlitSprite",
                         "arrayStatusLitSprite",
                         "arrayStatusUnlitSprite"
                     })
            {
                SerializedProperty sprite = serializedPanel.FindProperty(
                    propertyName);
                Require(sprite != null
                        && sprite.objectReferenceValue != null,
                    "ITEM_DETAIL_STATUS_SPRITE_MISSING " + propertyName);
            }
        }

        private static void AuthorPlayerPrefab(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = NewUiObject("FormalBattlePlayerPresentation");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(330f, 160f);
                Image background = AddImage(
                    root,
                    new Color(0.12f, 0.035f, 0.045f, 0.96f));
                background.raycastTarget = false;
                AddGoldOutline(root, 2f);

                Image accent = CreateImage(
                    "VermilionAccent",
                    root.transform,
                    new Color(0.78f, 0.16f, 0.075f, 0.96f));
                Pin(accent.rectTransform,
                    new Vector2(0.5f, 1f),
                    new Vector2(0f, -5f),
                    new Vector2(312f, 5f));

                Image avatar = CreateImage(
                    "AcceptedPlayerAvatar",
                    root.transform,
                    Color.white);
                avatar.sprite = profile.PlayerAvatarSprite;
                avatar.preserveAspect = true;
                Pin(avatar.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(68f, 0f),
                    new Vector2(126f, 140f));

                TMP_Text title = CreateText(
                    "PlayerIdentity",
                    root.transform,
                    profile.DamageFont,
                    "明箓行者",
                    24f,
                    new Color(1f, 0.86f, 0.55f, 1f));
                Pin(title.rectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(226f, -38f),
                    new Vector2(180f, 40f));

                Image hpBack = CreateImage(
                    "PlayerHpBack",
                    root.transform,
                    new Color(0.055f, 0.012f, 0.018f, 0.98f));
                Pin(hpBack.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(227f, -12f),
                    new Vector2(178f, 26f));
                Image hpFill = CreateImage(
                    "PlayerHpFill",
                    hpBack.transform,
                    profile.HpFillColor);
                Stretch(hpFill.rectTransform,
                    new Vector2(3f, 3f),
                    new Vector2(-3f, -3f));
                hpFill.type = Image.Type.Filled;
                hpFill.fillMethod = Image.FillMethod.Horizontal;
                hpFill.fillOrigin = 0;
                hpFill.fillAmount = 1f;

                Image hpFrame = CreateImage(
                    "AcceptedHpFrame",
                    hpBack.transform,
                    Color.white);
                Stretch(hpFrame.rectTransform, Vector2.zero, Vector2.zero);
                hpFrame.sprite = profile.HpFrameSprite;
                hpFrame.preserveAspect = false;

                TMP_Text hpLabel = CreateText(
                    "PlayerHpLabel",
                    root.transform,
                    profile.DamageFont,
                    "-- / --",
                    20f,
                    Color.white);
                Pin(hpLabel.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(227f, -12f),
                    new Vector2(174f, 24f));

                GameObject guardRoot = NewUiObject(
                    "PlayerGuardReadout",
                    root.transform);
                AddImage(
                    guardRoot,
                    new Color(0.08f, 0.17f, 0.22f, 0.96f));
                Pin(
                    guardRoot.transform as RectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(227f, -39f),
                    new Vector2(176f, 20f));
                TMP_Text guardLabel = CreateText(
                    "PlayerGuardLabel",
                    guardRoot.transform,
                    profile.DamageFont,
                    "\u62a4\u76fe 0",
                    13f,
                    new Color(0.72f, 0.91f, 1f, 1f));
                Stretch(
                    guardLabel.rectTransform,
                    new Vector2(5f, 0f),
                    new Vector2(-5f, 0f));

                FormalBattleStatusStripView statusStrip =
                    CreateStatusStrip(
                        "PlayerStatusStrip",
                        root.transform,
                        profile,
                        new Vector2(227f, -65f));

                Image hitOverlay = CreateImage(
                    "PlayerHitOverlay",
                    avatar.transform,
                    new Color(1f, 0.25f, 0.08f, 0f));
                Stretch(hitOverlay.rectTransform, Vector2.zero, Vector2.zero);
                hitOverlay.sprite = profile.PlayerAvatarSprite;
                hitOverlay.preserveAspect = true;

                FormalBattlePlayerPresentationView view =
                    root.AddComponent<FormalBattlePlayerPresentationView>();
                view.AssignForEditor(
                    avatar,
                    avatar.rectTransform,
                    hpFill,
                    hpLabel,
                    hitOverlay,
                    avatar.rectTransform);
                view.AssignReadabilityForEditor(
                    guardRoot,
                    guardLabel,
                    statusStrip);
                Require(view.ValidateAuthoredReferences(),
                    "FORMAL_PLAYER_PREFAB_REFERENCE_INVALID");
                SaveOwnedPrefab(root, PlayerPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AuthorEnemyPrefab(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = NewUiObject("FormalBattleEnemySlot");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(330f, 172f);
                Image background = AddImage(
                    root,
                    new Color(0.10f, 0.025f, 0.035f, 0.94f));
                background.raycastTarget = false;
                AddGoldOutline(root, 1.5f);

                Image visual = CreateImage(
                    "AuthoritativeEnemyVisual",
                    root.transform,
                    Color.white);
                Pin(visual.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(78f, 4f),
                    new Vector2(150f, 154f));
                visual.preserveAspect = true;
                visual.enabled = false;
                C1AuthoredEnemySelectionOutlineEffect outline =
                    visual.gameObject.AddComponent<
                        C1AuthoredEnemySelectionOutlineEffect>();
                outline.enabled = false;
                C1AuthoredEnemyCalibrationMeshEffect calibration =
                    visual.gameObject.AddComponent<
                        C1AuthoredEnemyCalibrationMeshEffect>();

                RawImage telegraph = CreateRawImage(
                    "RestrainedAttackTelegraph",
                    root.transform,
                    profile.RestrainedVfxTexture,
                    new Color(1f, 0.58f, 0.12f, 1f));
                Pin(telegraph.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(78f, 4f),
                    new Vector2(162f, 162f));
                CanvasGroup telegraphGroup =
                    telegraph.gameObject.AddComponent<CanvasGroup>();
                telegraphGroup.alpha = 0f;
                telegraphGroup.blocksRaycasts = false;
                telegraphGroup.interactable = false;

                TMP_Text identity = CreateText(
                    "EnemyIdentity",
                    root.transform,
                    profile.DamageFont,
                    "敌方",
                    23f,
                    new Color(1f, 0.84f, 0.52f, 1f));
                Pin(identity.rectTransform,
                    new Vector2(0f, 1f),
                    new Vector2(238f, -38f),
                    new Vector2(170f, 40f));

                Image hpBack = CreateImage(
                    "EnemyHpBack",
                    root.transform,
                    new Color(0.045f, 0.008f, 0.012f, 0.98f));
                Pin(hpBack.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(238f, -5f),
                    new Vector2(168f, 24f));
                Image hpFill = CreateImage(
                    "EnemyHpFill",
                    hpBack.transform,
                    profile.HpFillColor);
                Stretch(hpFill.rectTransform,
                    new Vector2(3f, 3f),
                    new Vector2(-3f, -3f));
                hpFill.type = Image.Type.Filled;
                hpFill.fillMethod = Image.FillMethod.Horizontal;
                hpFill.fillOrigin = 0;

                Image hpFrame = CreateImage(
                    "AcceptedHpFrame",
                    hpBack.transform,
                    Color.white);
                Stretch(hpFrame.rectTransform, Vector2.zero, Vector2.zero);
                hpFrame.sprite = profile.HpFrameSprite;

                TMP_Text hpLabel = CreateText(
                    "EnemyHpLabel",
                    root.transform,
                    profile.DamageFont,
                    "-- / --",
                    18f,
                    Color.white);
                Pin(hpLabel.rectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(238f, -5f),
                    new Vector2(164f, 22f));

                GameObject shellRoot = NewUiObject(
                    "EnemyShellReadout",
                    root.transform);
                AddImage(
                    shellRoot,
                    new Color(0.045f, 0.13f, 0.18f, 0.98f));
                Pin(
                    shellRoot.transform as RectTransform,
                    new Vector2(0f, 0.5f),
                    new Vector2(238f, -34f),
                    new Vector2(168f, 18f));
                Image shellFill = CreateImage(
                    "EnemyShellFill",
                    shellRoot.transform,
                    new Color(0.32f, 0.76f, 0.88f, 0.92f));
                Stretch(
                    shellFill.rectTransform,
                    new Vector2(2f, 2f),
                    new Vector2(-2f, -2f));
                shellFill.type = Image.Type.Filled;
                shellFill.fillMethod = Image.FillMethod.Horizontal;
                shellFill.fillOrigin = 0;
                shellFill.fillAmount = 1f;
                TMP_Text shellLabel = CreateText(
                    "EnemyShellLabel",
                    shellRoot.transform,
                    profile.DamageFont,
                    string.Empty,
                    11f,
                    Color.white);
                Stretch(
                    shellLabel.rectTransform,
                    new Vector2(4f, 0f),
                    new Vector2(-4f, 0f));
                shellRoot.SetActive(false);

                FormalBattleStatusStripView statusStrip =
                    CreateStatusStrip(
                        "EnemyStatusStrip",
                        root.transform,
                        profile,
                        new Vector2(238f, -63f));

                Image hitOverlay = CreateImage(
                    "EnemyHitOverlay",
                    visual.transform,
                    new Color(1f, 0.25f, 0.08f, 0f));
                Stretch(hitOverlay.rectTransform, Vector2.zero, Vector2.zero);
                hitOverlay.preserveAspect = true;

                FormalBattleEnemySlotView view =
                    root.AddComponent<FormalBattleEnemySlotView>();
                view.AssignForEditor(
                    0,
                    visual,
                    visual.rectTransform,
                    outline,
                    calibration,
                    hpFill,
                    hpLabel,
                    identity,
                    telegraphGroup,
                    hitOverlay,
                    visual.rectTransform);
                view.AssignReadabilityForEditor(
                    shellRoot,
                    shellFill,
                    shellLabel,
                    statusStrip);
                Require(view.ValidateAuthoredReferences(),
                    "FORMAL_ENEMY_PREFAB_REFERENCE_INVALID");
                SaveOwnedPrefab(root, EnemyPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AuthorDamageFloatPrefab(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = NewUiObject("FormalBattleDamageFloatPool");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(1920f, 1080f);
                FormalBattleDamageFloatPool pool =
                    root.AddComponent<FormalBattleDamageFloatPool>();
                RectTransform[] rects = new RectTransform[10];
                CanvasGroup[] groups = new CanvasGroup[10];
                TMP_Text[] labels = new TMP_Text[10];
                for (int index = 0; index < rects.Length; index++)
                {
                    TMP_Text label = CreateText(
                        "DamageFloat_" + index.ToString("D2"),
                        root.transform,
                        profile.DamageFont,
                        string.Empty,
                        42f,
                        Color.white);
                    label.fontStyle = FontStyles.Bold;
                    label.enableWordWrapping = false;
                    label.overflowMode = TextOverflowModes.Overflow;
                    label.rectTransform.sizeDelta = new Vector2(190f, 72f);
                    CanvasGroup group =
                        label.gameObject.AddComponent<CanvasGroup>();
                    group.alpha = 0f;
                    group.blocksRaycasts = false;
                    group.interactable = false;
                    rects[index] = label.rectTransform;
                    groups[index] = group;
                    labels[index] = label;
                    label.gameObject.SetActive(false);
                }
                pool.AssignForEditor(rootRect, profile, rects, groups, labels);
                Require(pool.ValidateAuthoredReferences(),
                    "FORMAL_DAMAGE_FLOAT_POOL_REFERENCE_INVALID");
                SaveOwnedPrefab(root, DamageFloatPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AuthorCueFxPrefab(
            FormalBattlePresentationProfile profile)
        {
            GameObject root = NewUiObject("FormalBattleCueFxAudioRoot");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(1920f, 1080f);
                GameObject ribbonLayer = NewUiObject(
                    "CausalRibbonLayer",
                    root.transform);
                Stretch(
                    ribbonLayer.transform as RectTransform,
                    Vector2.zero,
                    Vector2.zero);
                FormalBattleCausalRibbonGraphic[] ribbons =
                    new FormalBattleCausalRibbonGraphic[6];
                for (int index = 0; index < ribbons.Length; index++)
                {
                    GameObject ribbonObject = NewUiObject(
                        "CausalRibbon_" + index.ToString("D2"),
                        ribbonLayer.transform);
                    Stretch(
                        ribbonObject.transform as RectTransform,
                        Vector2.zero,
                        Vector2.zero);
                    ribbons[index] = ribbonObject.AddComponent<
                        FormalBattleCausalRibbonGraphic>();
                    ribbons[index].AssignForEditor();
                }
                RawImage sourcePulse = CreateRawImage(
                    "ExactItemSourcePulse",
                    root.transform,
                    profile.RestrainedVfxTexture,
                    new Color(1f, 0.64f, 0.18f, 0f));
                sourcePulse.rectTransform.sizeDelta = new Vector2(160f, 160f);
                RawImage impact = CreateRawImage(
                    "RestrainedReceiverImpact",
                    root.transform,
                    profile.RestrainedVfxTexture,
                    new Color(1f, 0.22f, 0.06f, 0f));
                impact.rectTransform.sizeDelta = new Vector2(150f, 150f);

                AudioSource sourceAudio = root.AddComponent<AudioSource>();
                AudioSource enemyAudio = root.AddComponent<AudioSource>();
                AudioSource impactAudio = root.AddComponent<AudioSource>();
                ConfigureAudio(sourceAudio);
                ConfigureAudio(enemyAudio);
                ConfigureAudio(impactAudio);

                FormalBattleCueFxAudioRoot cueRoot =
                    root.AddComponent<FormalBattleCueFxAudioRoot>();
                cueRoot.AssignForEditor(
                    rootRect,
                    sourcePulse.rectTransform,
                    sourcePulse,
                    impact.rectTransform,
                    impact,
                    ribbons,
                    sourceAudio,
                    enemyAudio,
                    impactAudio,
                    profile);
                Require(cueRoot.ValidateAuthoredReferences(),
                    "FORMAL_CUE_FX_ROOT_REFERENCE_INVALID");
                SaveOwnedPrefab(root, CueFxPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void RestylePresentationRootPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                RectTransform rootRect = root.transform as RectTransform;
                rootRect.sizeDelta = new Vector2(1080f, 720f);
                rootRect.pivot = new Vector2(0.5f, 1f);

                Transform player = FindNamedTransform(
                    root,
                    "FormalBattlePlayerPresentation");
                Pin(
                    player as RectTransform,
                    new Vector2(0.5f, 1f),
                    new Vector2(-360f, -330f),
                    new Vector2(420f, 560f));

                FormalBattleEnemySlotView[] slots = root
                    .GetComponentsInChildren<FormalBattleEnemySlotView>(true)
                    .OrderBy(value => value.AuthoredStableOrder)
                    .ToArray();
                Require(slots.Length == 3,
                    "FORMAL_HUD_PARITY_ENEMY_SLOT_COUNT_INVALID");
                for (int index = 0; index < slots.Length; index++)
                {
                    Pin(
                        slots[index].transform as RectTransform,
                        new Vector2(0.5f, 1f),
                        new Vector2(150f, -105f - index * 215f),
                        new Vector2(440f, 210f));
                }

                TMP_Text stageLabel = FindNamedComponent<TMP_Text>(
                    root,
                    "AuthoritativeStageLabel");
                stageLabel.fontSize = 24f;
                stageLabel.fontStyle = FontStyles.Bold;
                stageLabel.color = new Color(1f, 0.82f, 0.46f, 1f);
                Pin(
                    stageLabel.rectTransform,
                    new Vector2(0.5f, 1f),
                    new Vector2(-55f, -22f),
                    new Vector2(760f, 38f));

                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    PresentationRootPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AuthorPresentationRootPrefab(
            FormalBattlePresentationProfile profile)
        {
            GameObject playerAsset = RequirePrefab(PlayerPrefabPath);
            GameObject enemyAsset = RequirePrefab(EnemyPrefabPath);
            GameObject root = NewUiObject("FormalBattlePresentationRoot");
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                rootRect.sizeDelta = new Vector2(1080f, 720f);
                rootRect.pivot = new Vector2(0.5f, 1f);
                FormalBattlePresentationRoot presenter =
                    root.AddComponent<FormalBattlePresentationRoot>();

                GameObject playerObject = InstantiateNested(
                    playerAsset,
                    root.transform,
                    "FormalBattlePlayerPresentation");
                Pin(playerObject.transform as RectTransform,
                    new Vector2(0.5f, 1f),
                    new Vector2(-360f, -330f),
                    new Vector2(420f, 560f));
                FormalBattlePlayerPresentationView player =
                    playerObject.GetComponent<
                        FormalBattlePlayerPresentationView>();

                FormalBattleEnemySlotView[] slots =
                    new FormalBattleEnemySlotView[3];
                for (int index = 0; index < slots.Length; index++)
                {
                    GameObject enemyObject = InstantiateNested(
                        enemyAsset,
                        root.transform,
                        "EnemySlot_" + index);
                    Pin(enemyObject.transform as RectTransform,
                        new Vector2(0.5f, 1f),
                        new Vector2(150f, -105f - index * 215f),
                        new Vector2(440f, 210f));
                    slots[index] = enemyObject.GetComponent<
                        FormalBattleEnemySlotView>();
                    slots[index].AssignStableOrderForEditor(index);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(
                        slots[index]);
                }

                TMP_Text stageLabel = CreateText(
                    "AuthoritativeStageLabel",
                    root.transform,
                    profile.DamageFont,
                    string.Empty,
                    22f,
                    new Color(1f, 0.76f, 0.34f, 1f));
                Pin(stageLabel.rectTransform,
                    new Vector2(0.5f, 1f),
                    new Vector2(-55f, -22f),
                    new Vector2(760f, 38f));

                GameObject proxyLayer = NewUiObject(
                    "CausalSourceProxyLayer",
                    root.transform);
                Stretch(
                    proxyLayer.transform as RectTransform,
                    Vector2.zero,
                    Vector2.zero);
                RectTransform[] proxyRects = new RectTransform[4];
                CanvasGroup[] proxyGroups = new CanvasGroup[4];
                RawImage[] proxyHalos = new RawImage[4];
                Image[] proxyArtworks = new Image[4];
                for (int index = 0; index < proxyRects.Length; index++)
                {
                    GameObject proxy = NewUiObject(
                        "CausalSourceProxy_" + index.ToString("D2"),
                        proxyLayer.transform);
                    proxyRects[index] = proxy.transform as RectTransform;
                    Pin(
                        proxyRects[index],
                        new Vector2(0.5f, 0f),
                        new Vector2(-116f, 176f + index * 102f),
                        new Vector2(86f, 86f));
                    proxyGroups[index] = proxy.AddComponent<CanvasGroup>();
                    proxyGroups[index].alpha = 0f;
                    proxyGroups[index].blocksRaycasts = false;
                    proxyGroups[index].interactable = false;
                    proxyHalos[index] = CreateRawImage(
                        "SourceProxyHalo",
                        proxy.transform,
                        profile.RestrainedVfxTexture,
                        Color.clear);
                    Stretch(
                        proxyHalos[index].rectTransform,
                        new Vector2(-13f, -13f),
                        new Vector2(13f, 13f));
                    proxyArtworks[index] = CreateImage(
                        "SourceProxyArtwork",
                        proxy.transform,
                        Color.white);
                    Stretch(
                        proxyArtworks[index].rectTransform,
                        new Vector2(8f, 8f),
                        new Vector2(-8f, -8f));
                    proxyArtworks[index].preserveAspect = true;
                    proxyArtworks[index].sprite = null;
                }
                FormalBattleCausalSourceVisualView causalSourceView =
                    root.AddComponent<FormalBattleCausalSourceVisualView>();
                causalSourceView.AssignForEditor(
                    profile,
                    proxyRects,
                    proxyGroups,
                    proxyHalos,
                    proxyArtworks);
                Require(causalSourceView.ValidateAuthoredReferences(),
                    "FORMAL_CAUSAL_SOURCE_VIEW_REFERENCE_INVALID");

                presenter.AssignSourceCarrierForEditor(
                    profile,
                    player,
                    slots,
                    causalSourceView,
                    stageLabel);
                Require(presenter.ValidateSourceCarrierReferences(
                        out string sourceDiagnostic),
                    sourceDiagnostic);
                SaveOwnedPrefab(root, PresentationRootPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AuthorUnifiedComposition(
            FormalBattlePresentationProfile profile)
        {
            GameObject presentationAsset =
                RequirePrefab(PresentationRootPrefabPath);
            GameObject floatAsset = RequirePrefab(DamageFloatPrefabPath);
            GameObject cueAsset = RequirePrefab(CueFxPrefabPath);
            GameObject root = PrefabUtility.LoadPrefabContents(
                UnifiedPrefabPath);
            try
            {
                UnifiedBattlePageShell shell =
                    root.GetComponent<UnifiedBattlePageShell>();
                UnifiedBattleFormalSceneHost host =
                    root.GetComponentInChildren<
                        UnifiedBattleFormalSceneHost>(true);
                Require(shell != null && host != null,
                    "UNIFIED_FORMAL_PRESENTATION_HOST_MISSING");
                IReadOnlyDictionary<string, Transform> slots =
                    shell.BuildSlotMap();
                Transform enemyInfo = RequiredSlot(
                    slots,
                    UnifiedBattlePageShellSlotNames.EnemyInfoArea);
                Transform feedback = RequiredSlot(
                    slots,
                    UnifiedBattlePageShellSlotNames.BattleFeedbackLayer);

                RemoveOwnedPresentationChildren(enemyInfo, feedback);
                RetirePlaceholderSurface(enemyInfo);
                RetirePlaceholderSurface(feedback);

                GameObject presentationObject = InstantiateNested(
                    presentationAsset,
                    enemyInfo,
                    "FormalBattlePresentationRoot");
                RectTransform presentationRect =
                    presentationObject.transform as RectTransform;
                presentationRect.anchorMin = Vector2.zero;
                presentationRect.anchorMax = Vector2.one;
                presentationRect.pivot = new Vector2(0.5f, 1f);
                presentationRect.offsetMin = new Vector2(0f, -650f);
                presentationRect.offsetMax = Vector2.zero;

                GameObject floatObject = InstantiateNested(
                    floatAsset,
                    feedback,
                    "FormalBattleDamageFloatPool");
                ConfigureFeedbackExtent(
                    floatObject.transform as RectTransform);
                GameObject cueObject = InstantiateNested(
                    cueAsset,
                    feedback,
                    "FormalBattleCueFxAudioRoot");
                ConfigureFeedbackExtent(
                    cueObject.transform as RectTransform);

                FormalBattlePresentationRoot presenter =
                    presentationObject.GetComponent<
                        FormalBattlePresentationRoot>();
                FormalBattlePlayerPresentationView player =
                    presentationObject.GetComponentInChildren<
                        FormalBattlePlayerPresentationView>(true);
                FormalBattleEnemySlotView[] enemyViews =
                    presentationObject.GetComponentsInChildren<
                        FormalBattleEnemySlotView>(true)
                    .OrderBy(value => value.AuthoredStableOrder)
                    .ToArray();
                TMP_Text stageLabel = presentationObject
                    .GetComponentsInChildren<TMP_Text>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        "AuthoritativeStageLabel",
                        StringComparison.Ordinal));
                FormalBattleDamageFloatPool floatPool =
                    floatObject.GetComponent<FormalBattleDamageFloatPool>();
                FormalBattleCueFxAudioRoot cueRoot =
                    cueObject.GetComponent<FormalBattleCueFxAudioRoot>();
                FormalBattleCausalSourceVisualView causalSourceView =
                    presentationObject.GetComponentInChildren<
                        FormalBattleCausalSourceVisualView>(true);

                presenter.AssignForEditor(
                    profile,
                    player,
                    enemyViews,
                    floatPool,
                    cueRoot,
                    causalSourceView,
                    stageLabel);
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    presenter);
                host.AssignPresentationForEditor(presenter);
                Require(host.ValidateAuthoredBindings(out string diagnostic),
                    diagnostic);
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "UNIFIED_FORMAL_PRESENTATION_MISSING_SCRIPT");
                PrefabUtility.SaveAsPrefabAsset(root, UnifiedPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateSourceCarrierAuthoredAsset()
        {
            GameObject authoredRoot = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                FormalBattlePresentationRoot presenter =
                    authoredRoot.GetComponent<FormalBattlePresentationRoot>();
                Require(presenter != null,
                    "FORMAL_PRESENTATION_SOURCE_ROOT_COMPONENT_MISSING");
                Require(presenter.ValidateSourceCarrierReferences(
                        out string diagnostic),
                    diagnostic);
                Require(!presenter.ValidateDownstreamCompositionReferences(
                            out string externalDiagnostic)
                        && string.Equals(
                            externalDiagnostic,
                            FormalBattlePresentationRoot
                                .ExternalDamageFloatPoolMissingDiagnostic,
                            StringComparison.Ordinal),
                    "FORMAL_PRESENTATION_SOURCE_EXTERNAL_SPLIT_INVALID "
                    + externalDiagnostic);

                FormalBattlePlayerPresentationView[] players =
                    authoredRoot.GetComponentsInChildren<
                        FormalBattlePlayerPresentationView>(true);
                Require(players.Length == 1
                        && string.Equals(
                            players[0].gameObject.name,
                            "FormalBattlePlayerPresentation",
                            StringComparison.Ordinal)
                        && string.Equals(
                            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(
                                players[0].gameObject),
                            PlayerPrefabPath,
                            StringComparison.Ordinal),
                    "FORMAL_PRESENTATION_SOURCE_PLAYER_CARRIER_INVALID");

                TMP_Text[] stageLabels = authoredRoot
                    .GetComponentsInChildren<TMP_Text>(true)
                    .Where(value => string.Equals(
                        value.gameObject.name,
                        "AuthoritativeStageLabel",
                        StringComparison.Ordinal))
                    .ToArray();
                Require(stageLabels.Length == 1
                        && !stageLabels[0].raycastTarget,
                    "FORMAL_PRESENTATION_SOURCE_STAGE_LABEL_INVALID");

                FormalBattleEnemySlotView[] enemyViews = authoredRoot
                    .GetComponentsInChildren<FormalBattleEnemySlotView>(true);
                Require(enemyViews.Length == 3
                        && enemyViews.Select(value => value.AuthoredStableOrder)
                            .OrderBy(value => value)
                            .SequenceEqual(new[] { 0, 1, 2 }),
                    "FORMAL_PRESENTATION_SOURCE_ENEMY_SLOTS_INVALID");

                FormalBattleCausalSourceVisualView[] sourceViews =
                    authoredRoot.GetComponentsInChildren<
                        FormalBattleCausalSourceVisualView>(true);
                RectTransform[] sourceProxies = authoredRoot
                    .GetComponentsInChildren<RectTransform>(true)
                    .Where(value => value.name.StartsWith(
                        "CausalSourceProxy_",
                        StringComparison.Ordinal))
                    .ToArray();
                Require(sourceViews.Length == 1
                        && sourceViews[0].ValidateAuthoredReferences()
                        && sourceProxies.Length == 4
                        && sourceProxies.Select(value => value.name)
                            .OrderBy(value => value, StringComparer.Ordinal)
                            .SequenceEqual(new[]
                            {
                                "CausalSourceProxy_00",
                                "CausalSourceProxy_01",
                                "CausalSourceProxy_02",
                                "CausalSourceProxy_03"
                            }),
                    "FORMAL_PRESENTATION_SOURCE_CAUSAL_PROXIES_INVALID");

                Require(authoredRoot.GetComponentsInChildren<
                            FormalBattleDamageFloatPool>(true).Length == 0
                        && authoredRoot.GetComponentsInChildren<
                            FormalBattleCueFxAudioRoot>(true).Length == 0
                        && authoredRoot.GetComponentsInChildren<
                            C1ExactBattleSandboxItemCardView>(true).Length == 0,
                    "FORMAL_PRESENTATION_SOURCE_EXTERNAL_CARRIER_EMBEDDED");
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(
                                authoredRoot) == 0,
                    "FORMAL_PRESENTATION_SOURCE_MISSING_SCRIPT");
                Require(!AssetDatabase.GetDependencies(
                            PresentationRootPrefabPath,
                            true).Any(value => value.EndsWith(
                            ".unity",
                            StringComparison.OrdinalIgnoreCase)),
                    "FORMAL_PRESENTATION_SOURCE_SCENE_DEPENDENCY");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(authoredRoot);
            }
        }

        private static void ValidateSourceCarrierApiContract()
        {
            Type rootType = typeof(FormalBattlePresentationRoot);
            System.Reflection.FieldInfo providerField = rootType.GetField(
                "itemSourceProvider",
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);
            System.Reflection.FieldInfo catalogField = rootType.GetField(
                "itemPresentationCatalog",
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);
            Require(providerField != null
                    && providerField.FieldType
                    == typeof(C1ExactBattleSandboxItemArrangementPresenter)
                    && catalogField != null
                    && catalogField.FieldType
                    == typeof(C1FormalItemPresentationCatalogSnapshot),
                "FORMAL_PRESENTATION_DYNAMIC_PROVIDER_FIELDS_INVALID");
            Require(rootType.GetMethod(
                        "BindItemSourceProvider",
                        new[]
                        {
                            typeof(C1ExactBattleSandboxItemArrangementPresenter),
                            typeof(C1FormalItemPresentationCatalogSnapshot),
                            typeof(string).MakeByRefType()
                        }) != null,
                "FORMAL_PRESENTATION_DYNAMIC_PROVIDER_BIND_API_MISSING");
            Require(rootType.GetMethod(
                        "UnbindItemSourceProvider",
                        Type.EmptyTypes) != null,
                "FORMAL_PRESENTATION_DYNAMIC_PROVIDER_UNBIND_API_MISSING");

            string rootSource = File.ReadAllText(
                "Assets/_Game/Scripts/TalismanBag/Presentation/"
                + "FormalBattle/FormalBattlePresentationRoot.cs");
            Require(rootSource.IndexOf(
                        "TryCreateCurrentSourceAnchorRequest(",
                        StringComparison.Ordinal) >= 0
                    && rootSource.IndexOf(
                        ".TryResolveCurrentPlacedLitSourceAnchor(",
                        StringComparison.Ordinal) >= 0
                    && rootSource.IndexOf(
                        "binding.Matches(request)",
                        StringComparison.Ordinal) >= 0
                    && rootSource.IndexOf(
                        "cue.sourceItemInstanceId",
                        StringComparison.Ordinal) >= 0
                    && rootSource.IndexOf(
                        "cue.sourceBaseItemId",
                        StringComparison.Ordinal) >= 0
                    && rootSource.IndexOf(
                        "cue.effectFamilyId",
                        StringComparison.Ordinal) >= 0
                    && rootSource.IndexOf(
                        "cue.effectVariantId",
                        StringComparison.Ordinal) >= 0
                    && rootSource.IndexOf(
                        "exactSource" + "CardViews",
                        StringComparison.Ordinal) < 0
                    && rootSource.IndexOf(
                        "C1ExactBattleSandboxItemCardView",
                        StringComparison.Ordinal) < 0
                    && rootSource.IndexOf(
                        "AuthoredPresentation" + "Capacity",
                        StringComparison.Ordinal) < 0
                    && rootSource.IndexOf(
                        "MaxPool15Active" + "SourceCount",
                        StringComparison.Ordinal) < 0
                    && rootSource.IndexOf(
                        "new GameObject(",
                        StringComparison.Ordinal) < 0
                    && rootSource.IndexOf(
                        "GameObject.Find(",
                        StringComparison.Ordinal) < 0,
                "FORMAL_PRESENTATION_DYNAMIC_PROVIDER_STATIC_CONTRACT_INVALID");

            string authoringSource = File.ReadAllText(
                "Assets/_Game/Scripts/TalismanBag/Editor/Presentation/"
                + "FormalBattle/FormalBattlePresentationPrefabAuthoring.cs");
            int passStart = authoringSource.IndexOf(
                "private static void ApplySourceCarrierCorrectionPass()",
                StringComparison.Ordinal);
            int passEnd = authoringSource.IndexOf(
                "private static void ApplyCausalVisualAuthoringPass()",
                passStart,
                StringComparison.Ordinal);
            Require(passStart >= 0 && passEnd > passStart,
                "FORMAL_PRESENTATION_SOURCE_ONLY_PASS_MISSING");
            string sourceOnlyPass = authoringSource.Substring(
                passStart,
                passEnd - passStart);
            Require(sourceOnlyPass.IndexOf(
                        "UnifiedPrefabPath",
                        StringComparison.Ordinal) < 0
                    && sourceOnlyPass.IndexOf(
                        "AuthorUnifiedComposition",
                        StringComparison.Ordinal) < 0
                    && sourceOnlyPass.IndexOf(
                        "ValidateAuthoredAssets",
                        StringComparison.Ordinal) < 0,
                "FORMAL_PRESENTATION_SOURCE_ONLY_PASS_BROADENED");

            Require(authoringSource.IndexOf(
                        "Refresh Shared Item " + "Capacity Mount",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf(
                        "ExecuteSharedItem" + "CapacityMountFromCommandLine",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf(
                        "SharedItem" + "CapacityMountTerminalMarker",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf(
                        "ResolveShared" + "CapacityExactCards",
                        StringComparison.Ordinal) < 0
                    && authoringSource.IndexOf(
                        "AuthoredPresentation" + "Capacity",
                        StringComparison.Ordinal) < 0,
                "FORMAL_PRESENTATION_RETIRED_CAPACITY_ENTRY_REMAINS");
        }

        private static void ValidateCoreBattleReadabilityDecorAssets()
        {
            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            Require(profile.ValidateAuthoredReferences()
                    && profile.GetStatusStylesForEditor().Length == 2,
                "CORE_BATTLE_READABILITY_PROFILE_INVALID");

            GameObject player = PrefabUtility.LoadPrefabContents(
                PlayerPrefabPath);
            try
            {
                FormalBattlePlayerPresentationView view =
                    player.GetComponent<FormalBattlePlayerPresentationView>();
                Require(view != null
                        && view.ValidateAuthoredReferences()
                        && player.GetComponentsInChildren<
                            FormalBattleStatusStripView>(true).Length == 1,
                    "CORE_BATTLE_READABILITY_PLAYER_PREFAB_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(player);
            }

            GameObject enemy = PrefabUtility.LoadPrefabContents(
                EnemyPrefabPath);
            try
            {
                FormalBattleEnemySlotView view =
                    enemy.GetComponent<FormalBattleEnemySlotView>();
                Require(view != null
                        && view.ValidateAuthoredReferences()
                        && enemy.GetComponentsInChildren<
                            FormalBattleStatusStripView>(true).Length == 1,
                    "CORE_BATTLE_READABILITY_ENEMY_PREFAB_INVALID");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(enemy);
            }

            GameObject presentationRoot = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                FormalBattlePresentationRoot root =
                    presentationRoot.GetComponent<
                        FormalBattlePresentationRoot>();
                Require(root != null,
                    "CORE_BATTLE_READABILITY_ROOT_MISSING");
                Require(root.ValidateSourceCarrierReferences(
                        out string diagnostic),
                    diagnostic);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(presentationRoot);
            }

            GameObject detail = PrefabUtility.LoadPrefabContents(
                ItemDetailPopupPrefabPath);
            try
            {
                ItemDetailPanelView panel = detail.GetComponentInChildren<
                    ItemDetailPanelView>(true);
                Require(panel != null,
                    "CORE_BATTLE_READABILITY_DETAIL_PANEL_MISSING");
                Transform stats = FindNamedTransform(
                    detail,
                    "BaseStatsSection");
                Transform trigger = FindNamedTransform(
                    detail,
                    "TriggerConditionSection");
                Transform basic = FindNamedTransform(
                    detail,
                    "BasicEffectSection");
                Transform flavor = FindNamedTransform(
                    detail,
                    "FlavorSection");
                SerializedObject serializedPanel =
                    new SerializedObject(panel);
                SerializedProperty showDebug = serializedPanel.FindProperty(
                    "showDebugTabOnPlayerRoute");
                Require(string.Equals(
                            stats.parent.gameObject.name,
                            "ItemDetailFixedBaseStatsRoot",
                            StringComparison.Ordinal)
                        && ReferenceEquals(trigger.parent, basic.parent)
                        && string.Equals(
                            trigger.parent.gameObject.name,
                            "DetailContent",
                            StringComparison.Ordinal)
                        && basic.GetSiblingIndex() == 0
                        && trigger.GetSiblingIndex()
                        == flavor.GetSiblingIndex() + 1
                        && showDebug != null
                        && !showDebug.boolValue,
                    "CORE_BATTLE_READABILITY_DETAIL_PREFAB_INVALID");
                ValidateItemDetailStatusSprites(panel);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(detail);
            }

            GameObject unified = PrefabUtility.LoadPrefabContents(
                UnifiedPrefabPath);
            try
            {
                UnifiedBattleFormalSceneHost host =
                    unified.GetComponentInChildren<
                        UnifiedBattleFormalSceneHost>(true);
                Require(host != null,
                    "CORE_BATTLE_READABILITY_HOST_MISSING");
                Require(host.ValidateAuthoredBindings(
                        out string diagnostic),
                    diagnostic);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(unified);
            }
        }

        private static void ValidateAuthoredAssets()
        {
            FormalBattlePresentationProfile profile =
                AssetDatabase.LoadAssetAtPath<
                    FormalBattlePresentationProfile>(ProfilePath);
            Require(profile != null && profile.ValidateAuthoredReferences(),
                "FORMAL_PRESENTATION_PROFILE_POST_IMPORT_INVALID");
            Require(profile.SourcePulseClip != null
                    && profile.EnemyAttackClip != null
                    && profile.ImpactClip != null,
                "FORMAL_PRESENTATION_PROFILE_AUDIO_SLOT_NULL");
            Require(string.Equals(
                        AssetDatabase.GetAssetPath(profile.SourcePulseClip),
                        SourcePulseAudioPath,
                        StringComparison.Ordinal)
                    && string.Equals(
                        AssetDatabase.GetAssetPath(profile.EnemyAttackClip),
                        EnemyAttackAudioPath,
                        StringComparison.Ordinal)
                    && string.Equals(
                        AssetDatabase.GetAssetPath(profile.ImpactClip),
                        ImpactAudioPath,
                        StringComparison.Ordinal),
                "FORMAL_PRESENTATION_PROFILE_AUDIO_PATH_INVALID");
            ValidateImportedAudioClip(
                profile.SourcePulseClip,
                SourcePulseAudioPath,
                0.08f,
                0.14f);
            ValidateImportedAudioClip(
                profile.EnemyAttackClip,
                EnemyAttackAudioPath,
                0.14f,
                0.24f);
            ValidateImportedAudioClip(
                profile.ImpactClip,
                ImpactAudioPath,
                0.10f,
                0.20f);
            ValidateRootComponent<FormalBattlePlayerPresentationView>(
                PlayerPrefabPath,
                value => value.ValidateAuthoredReferences());
            ValidateRootComponent<FormalBattleEnemySlotView>(
                EnemyPrefabPath,
                value => value.ValidateAuthoredReferences());
            ValidateRootComponent<FormalBattleDamageFloatPool>(
                DamageFloatPrefabPath,
                value => value.ValidateAuthoredReferences());
            ValidateRootComponent<FormalBattleCueFxAudioRoot>(
                CueFxPrefabPath,
                value => value.ValidateAuthoredReferences());

            GameObject authoredRoot = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                Require(authoredRoot.GetComponent<
                            FormalBattlePresentationRoot>() != null,
                    "FORMAL_PRESENTATION_ROOT_COMPONENT_MISSING");
                Require(authoredRoot.GetComponentsInChildren<
                            FormalBattlePlayerPresentationView>(true).Length == 1,
                    "FORMAL_PRESENTATION_ROOT_PLAYER_COUNT_INVALID");
                Require(authoredRoot.GetComponentsInChildren<
                            FormalBattleEnemySlotView>(true)
                            .Select(value => value.AuthoredStableOrder)
                            .OrderBy(value => value)
                            .SequenceEqual(new[] { 0, 1, 2 }),
                    "FORMAL_PRESENTATION_ROOT_STABLE_ORDER_INVALID");
                Require(authoredRoot.GetComponentsInChildren<
                            FormalBattleCausalSourceVisualView>(true).Length == 1
                        && authoredRoot.GetComponentInChildren<
                                FormalBattleCausalSourceVisualView>(true)
                            .ValidateAuthoredReferences(),
                    "FORMAL_CAUSAL_SOURCE_ROOT_INVALID");
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(
                                authoredRoot) == 0,
                    "FORMAL_PRESENTATION_ROOT_MISSING_SCRIPT");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(authoredRoot);
            }

            GameObject unified = PrefabUtility.LoadPrefabContents(
                UnifiedPrefabPath);
            try
            {
                UnifiedBattleFormalSceneHost host =
                    unified.GetComponentInChildren<
                        UnifiedBattleFormalSceneHost>(true);
                Require(host != null,
                    "UNIFIED_FORMAL_HOST_MISSING_POST_IMPORT");
                Require(host.ValidateAuthoredBindings(
                        out string diagnostic),
                    diagnostic);
                Require(unified.GetComponentsInChildren<
                            FormalBattlePresentationRoot>(true).Length == 1,
                    "UNIFIED_FORMAL_PRESENTATION_ROOT_COUNT_INVALID");
                Require(unified.GetComponentsInChildren<
                            FormalBattleDamageFloatPool>(true).Length == 1,
                    "UNIFIED_FORMAL_FLOAT_POOL_COUNT_INVALID");
                Require(unified.GetComponentsInChildren<
                            FormalBattleCueFxAudioRoot>(true).Length == 1,
                    "UNIFIED_FORMAL_CUE_ROOT_COUNT_INVALID");
                Require(unified.GetComponentsInChildren<
                            FormalBattleCausalSourceVisualView>(true).Length == 1,
                    "UNIFIED_FORMAL_CAUSAL_SOURCE_COUNT_INVALID");
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(unified) == 0,
                    "UNIFIED_FORMAL_PRESENTATION_POST_IMPORT_MISSING_SCRIPT");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(unified);
            }
        }

        private static void ValidateDeterministicCueConsumption()
        {
            GameObject unifiedAsset = RequirePrefab(UnifiedPrefabPath);
            GameObject instance = PrefabUtility.InstantiatePrefab(
                unifiedAsset) as GameObject;
            Require(instance != null,
                "FORMAL_PRESENTATION_DETERMINISTIC_INSTANCE_FAILED");
            C1FormalObtainRebuildBattleLoop loop = null;
            try
            {
                FormalBattlePresentationRoot presenter =
                    instance.GetComponentInChildren<
                        FormalBattlePresentationRoot>(true);
                Require(presenter != null
                        && presenter.ValidateAuthoredReferences(),
                    "FORMAL_PRESENTATION_DETERMINISTIC_ROOT_INVALID");
                loop = CreateFormalLoop("presentation-consumer", 41L);
                C1FormalRealtimeBattleSessionStartSnapshot stageOneStart =
                    loop.CurrentRealtimeStart;
                Require(presenter.BeginRealtime(
                        stageOneStart,
                        loop.ItemAuthority.Current,
                        Chapter1CampaignStageConfig.StageOneId,
                        out string diagnostic),
                    diagnostic);
                C1FormalRealtimeBattleCue initialTarget = stageOneStart
                    .initialCues.Single(value => string.Equals(
                        value.cueKind,
                        C1FormalRealtimeBattleCueKinds.TargetChanged,
                        StringComparison.Ordinal));
                Require(presenter.ConsumedInitialEnvelopeCount == 1
                        && string.Equals(
                            presenter.CurrentTargetActorId,
                            initialTarget.targetActorId,
                            StringComparison.Ordinal)
                        && presenter.CurrentTargetStableOrder
                        == initialTarget.targetStableOrder,
                    "FORMAL_PRESENTATION_INITIAL_TARGET_NOT_CUE_OWNED");
                int initialCueCount = presenter.ConsumedCueCount;
                Require(presenter.BeginRealtime(
                        stageOneStart,
                        loop.ItemAuthority.Current,
                        Chapter1CampaignStageConfig.StageOneId,
                        out diagnostic)
                        && presenter.ConsumedInitialEnvelopeCount == 1
                        && presenter.ConsumedCueCount == initialCueCount,
                    "FORMAL_PRESENTATION_INITIAL_ENVELOPE_REPLAYED");

                Require(loop.TryAdvanceBy(9000L, out diagnostic), diagnostic);
                Require(presenter.ConsumeRealtime(
                        loop.RealtimeState,
                        loop.LastRealtimeTick.emittedCues,
                        loop.ItemAuthority.Current,
                        out diagnostic),
                    diagnostic);
                Require(presenter.CausalSourceVisualView != null
                        && presenter.CausalSourceVisualView
                            .AcceptedVisualCount > 0,
                    "FORMAL_CAUSAL_SOURCE_CUE_NOT_CONSUMED");
                Require(presenter.CueFxAudioRoot != null
                        && presenter.CueFxAudioRoot
                            .CausalRibbonPlayCount > 0,
                    "FORMAL_CAUSAL_RIBBON_CUE_NOT_CONSUMED");
                int tickCueCount = presenter.ConsumedCueCount;
                long tickSequence = presenter.LastConsumedSequence;
                Require(presenter.ConsumeRealtime(
                        loop.RealtimeState,
                        loop.LastRealtimeTick.emittedCues,
                        loop.ItemAuthority.Current,
                        out diagnostic)
                        && presenter.ConsumedCueCount == tickCueCount
                        && presenter.LastConsumedSequence == tickSequence,
                    "FORMAL_PRESENTATION_TICK_REPLAY_NOT_DEDUPED");

                Require(loop.TryAdvanceBy(60000L, out diagnostic), diagnostic);
                Require(loop.TryClaimFirstClearReward(out diagnostic),
                    diagnostic);
                C1FormalItemSessionOperationResult placement =
                    loop.ItemAuthority.Submit(
                        new C1FormalItemArrangementCommand(
                            C1FormalItemArrangementCommandKind.PlaceFromTray,
                            loop.ItemAuthority.Current.sessionToken,
                            loop.ItemAuthority.Current.resetGeneration,
                            "formal-presentation-stage-two-place",
                            loop.FirstClearReceipt.rewardResult.rewardEntries
                                .Single().itemInstanceId,
                            loop.ItemAuthority.Current.canonicalSignature,
                            new C1FormalItemPlacementCandidate(
                                new Vector2Int(0, 2),
                                0)));
                Require(placement != null && placement.accepted,
                    placement?.diagnosticCode
                    ?? "FORMAL_PRESENTATION_STAGE_TWO_PLACEMENT_FAILED");
                Require(loop.TryStartStageTwo(out diagnostic), diagnostic);
                C1FormalRealtimeBattleSessionStartSnapshot stageTwoStart =
                    loop.CurrentRealtimeStart;
                Require(!string.Equals(
                        stageOneStart.canonicalSignature,
                        stageTwoStart.canonicalSignature,
                        StringComparison.Ordinal)
                        && presenter.BeginRealtime(
                            stageTwoStart,
                            loop.ItemAuthority.Current,
                            Chapter1CampaignStageConfig.StageTwoId,
                            out diagnostic)
                        && presenter.ConsumedInitialEnvelopeCount == 1
                        && string.Equals(
                            presenter.ActiveStartSignature,
                            stageTwoStart.canonicalSignature,
                            StringComparison.Ordinal),
                    diagnostic);
                C1FormalRealtimeBattleCue stageTwoTarget = stageTwoStart
                    .initialCues.Single(value => string.Equals(
                        value.cueKind,
                        C1FormalRealtimeBattleCueKinds.TargetChanged,
                        StringComparison.Ordinal));
                Require(string.Equals(
                        presenter.CurrentTargetActorId,
                        stageTwoTarget.targetActorId,
                        StringComparison.Ordinal)
                        && presenter.CurrentTargetStableOrder
                        == stageTwoTarget.targetStableOrder,
                    "FORMAL_PRESENTATION_STAGE_TWO_TARGET_NOT_REFRESHED");
                presenter.ResetPresentation();
                Require(string.IsNullOrEmpty(presenter.ActiveStartSignature)
                        && string.IsNullOrEmpty(
                            presenter.CurrentTargetActorId)
                        && presenter.CurrentTargetStableOrder == -1
                        && presenter.ConsumedCueCount == 0,
                    "FORMAL_PRESENTATION_RESET_NOT_IDEMPOTENT");
            }
            finally
            {
                loop?.Reset();
                if (instance != null)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }

        private static C1FormalObtainRebuildBattleLoop CreateFormalLoop(
            string suffix,
            long generation)
        {
            Require(Chapter1CampaignStageCatalog.TryGet(
                    Chapter1CampaignStageConfig.StageOneId,
                    out Chapter1CampaignStageConfig config,
                    out string diagnostic),
                diagnostic);
            BattleLaunchContext context = config.CreateLaunchContext(
                "formal-presentation-authoring-" + suffix,
                "formal-presentation-token-" + suffix,
                generation);
            ItemBalanceWorkbenchCatalog canonicalSource =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                    "Assets/_Game/Configs/ItemBalanceWorkbench/" +
                    "ItemBalanceWorkbenchCatalog.asset");
            Require(canonicalSource != null,
                "FORMAL_PRESENTATION_CANONICAL_ITEM_SOURCE_MISSING");
            Require(CanonicalItemDefinitionResolver.TryCreate(
                    canonicalSource,
                    out CanonicalItemDefinitionResolver canonicalResolver,
                    out IReadOnlyList<string> canonicalErrors),
                "FORMAL_PRESENTATION_CANONICAL_ITEM_SOURCE_INVALID " +
                string.Join(",", canonicalErrors));
            Require(C1FormalObtainRebuildBattleLoop.TryCreate(
                    context,
                    config,
                    canonicalResolver,
                    out C1FormalObtainRebuildBattleLoop loop,
                    out diagnostic),
                diagnostic);
            return loop;
        }

        private static void ValidateDynamicGrammarProfileAsset(
            FormalBattlePresentationProfile profile,
            ProfilePreservationSnapshot preservation)
        {
            Require(profile != null && profile.ValidateAuthoredReferences(),
                "FORMAL_PROFILE_COVERAGE_REFERENCE_INVALID");
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty enemyRows = serialized.FindProperty(
                "enemyProfiles");
            SerializedProperty causalRows = serialized.FindProperty(
                "causalItemStyles");
            Require(enemyRows != null && enemyRows.isArray
                    && enemyRows.arraySize == 3,
                "FORMAL_PROFILE_COVERAGE_ENEMY_SERIALIZED_COUNT_INVALID");
            Require(causalRows != null && causalRows.isArray
                    && causalRows.arraySize > 0,
                "FORMAL_PROFILE_DYNAMIC_GRAMMAR_SERIALIZED_COUNT_INVALID");

            Require(profile.TryGetEnemyProfile(
                    BoneSwapRemnantRuntimeContract.ContentId,
                    BoneSwapRemnantRuntimeContract.OperatorProfileId,
                    out FormalBattleEnemyVisualProfile bone)
                    && string.Equals(
                        bone.DisplayName,
                        "\u6362\u9AA8\u6B8B\u76F8",
                        StringComparison.Ordinal),
                "FORMAL_PROFILE_BONE_SWAP_IDENTITY_INVALID");
            ValidateClipSource(
                bone.Idle,
                "bone-swap-remnant.idle",
                "/d1_3_Idle/frames/");
            ValidateClipSource(
                bone.Attack,
                "bone-swap-remnant.attack-or-skill",
                "/d1_3_Skill/frames/");
            ValidateClipSource(
                bone.Hit,
                "bone-swap-remnant.hit",
                "/d1_3_Hit/frames/");
            ValidateClipSource(
                bone.Death,
                "bone-swap-remnant.death",
                "/d1_3_Death/frames/");

            FormalBattleCausalItemStyle[] grammars = profile
                .GetCausalStylesForEditor();
            Require(grammars.Length == causalRows.arraySize
                    && grammars.All(value => value != null
                        && value.Validate())
                    && grammars.Select(value => value.GrammarKey)
                        .Distinct(StringComparer.Ordinal).Count()
                    == grammars.Length
                    && grammars.Select(value => string.Join("|", new[]
                    {
                        value.PresentationStyleKey,
                        value.EffectFamilyKey,
                        value.CueIdentity,
                        value.CueKind,
                        value.EffectVariantId
                    })).Distinct(StringComparer.Ordinal).Count()
                    == grammars.Length
                    && grammars.All(value => value.SourceArtwork == null),
                "FORMAL_PROFILE_DYNAMIC_GRAMMAR_ROWS_INVALID");
            ValidateProfilePreservation(profile, preservation);
        }

        private static void ValidateClipSource(
            FormalBattleAnimationClip clip,
            string expectedClipId,
            string expectedPathSegment)
        {
            Require(clip != null
                    && string.Equals(
                        clip.ClipId,
                        expectedClipId,
                        StringComparison.Ordinal)
                    && clip.Frames.Length > 0
                    && clip.Frames.All(value => value != null
                        && AssetDatabase.GetAssetPath(value).IndexOf(
                            expectedPathSegment,
                            StringComparison.Ordinal) >= 0),
                "FORMAL_PROFILE_BONE_SWAP_CLIP_SOURCE_INVALID "
                + expectedClipId);
        }

        private static IReadOnlyDictionary<string, byte[]>
            CaptureProfileCoverageProtectedBytes()
        {
            Dictionary<string, byte[]> snapshot =
                new Dictionary<string, byte[]>(StringComparer.Ordinal);
            foreach (string path in ProfileCoverageProtectedPaths)
            {
                Require(File.Exists(path),
                    "FORMAL_PROFILE_PROTECTED_FILE_MISSING " + path);
                snapshot.Add(path, File.ReadAllBytes(path));
            }
            return snapshot;
        }

        private static void ValidateProfileCoverageProtectedBytes(
            IReadOnlyDictionary<string, byte[]> expected)
        {
            Require(expected != null
                    && expected.Count == ProfileCoverageProtectedPaths.Length,
                "FORMAL_PROFILE_PROTECTED_SNAPSHOT_INVALID");
            foreach (string path in ProfileCoverageProtectedPaths)
            {
                Require(expected.TryGetValue(path, out byte[] expectedBytes)
                        && File.Exists(path),
                    "FORMAL_PROFILE_PROTECTED_FILE_MISSING " + path);
                byte[] currentBytes = File.ReadAllBytes(path);
                Require(BytesEqual(currentBytes, expectedBytes),
                    "FORMAL_PROFILE_PROTECTED_FILE_CHANGED_DURING_AUTHORING "
                    + path);
            }
        }

        private static bool BytesEqual(byte[] left, byte[] right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateDynamicGrammarProfileBoundaries()
        {
            const string authoringPath =
                "Assets/_Game/Scripts/TalismanBag/Editor/Presentation/"
                + "FormalBattle/FormalBattlePresentationPrefabAuthoring.cs";
            const string runtimeProfilePath =
                "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/"
                + "FormalBattlePresentationProfile.cs";
            string source = File.ReadAllText(authoringPath);
            string startToken =
                "private static void ApplyDynamicCausalGrammarProfilePass()";
            string endToken =
                "private static void ApplySourceCarrierCorrectionPass()";
            int start = source.IndexOf(startToken, StringComparison.Ordinal);
            int end = source.IndexOf(endToken, start, StringComparison.Ordinal);
            Require(start >= 0 && end > start,
                "FORMAL_PROFILE_SOURCE_ONLY_PASS_NOT_FOUND");
            string sourceOnlyPass = source.Substring(start, end - start);
            foreach (string forbidden in new[]
                     {
                         "PrefabUtility.",
                         "EditorSceneManager.",
                         "AssetDatabase.SaveAssets(",
                         "AssetDatabase.Refresh(",
                         "AuthorPresentationRootPrefab(",
                         "AuthorUnifiedComposition("
                     })
            {
                Require(sourceOnlyPass.IndexOf(
                            forbidden,
                            StringComparison.Ordinal) < 0,
                    "FORMAL_PROFILE_SOURCE_ONLY_PASS_FORBIDDEN_TOKEN "
                    + forbidden);
            }

            string runtime = File.ReadAllText(runtimeProfilePath);
            foreach (string forbidden in new[]
                     {
                         "new GameObject(",
                         "GameObject.Find(",
                         "Resources.Load",
                         "ExpectedCausal" + "ItemIds",
                         "AuthoritativeBase" + "ItemId"
                     })
            {
                Require(runtime.IndexOf(
                            forbidden,
                            StringComparison.Ordinal) < 0,
                    "FORMAL_PROFILE_RUNTIME_BOUNDARY_FORBIDDEN_TOKEN "
                    + forbidden);
            }
        }

        private static void ValidateRuntimeBoundaries()
        {
            foreach (string path in RuntimeSourcePaths)
            {
                Require(File.Exists(path),
                    "FORMAL_PRESENTATION_RUNTIME_SOURCE_MISSING " + path);
                string source = File.ReadAllText(path);
                foreach (string forbidden in ForbiddenRuntimeTokens)
                {
                    Require(source.IndexOf(
                                forbidden,
                                StringComparison.Ordinal) < 0,
                        "FORMAL_PRESENTATION_FORBIDDEN_RUNTIME_TOKEN "
                        + forbidden + " path=" + path);
                }
            }

            string hostSource = File.ReadAllText(RuntimeSourcePaths.Last());
            Require(hostSource.IndexOf(
                        "CurrentRealtimeStart",
                        StringComparison.Ordinal) >= 0
                    && hostSource.IndexOf(
                        "LastRealtimeTick.emittedCues",
                        StringComparison.Ordinal) >= 0,
                "FORMAL_PRESENTATION_HOST_REALTIME_SEAM_MISSING");
        }

        private static void ValidateCausalVisualBoundaries()
        {
            string sourceViewPath =
                "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/"
                + "FormalBattleCausalSourceVisualView.cs";
            string ribbonPath =
                "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/"
                + "FormalBattleCausalRibbonGraphic.cs";
            string rootPath =
                "Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/"
                + "FormalBattlePresentationRoot.cs";
            foreach (string path in new[]
                     {
                         sourceViewPath,
                         ribbonPath,
                         rootPath
                     })
            {
                Require(File.Exists(path),
                    "FORMAL_CAUSAL_RUNTIME_SOURCE_MISSING " + path);
            }

            string sourceView = File.ReadAllText(sourceViewPath);
            Require(sourceView.IndexOf(
                        "TMP_Text",
                        StringComparison.Ordinal) < 0
                    && sourceView.IndexOf(
                        "TextMeshPro",
                        StringComparison.Ordinal) < 0,
                "FORMAL_CAUSAL_SOURCE_TEXT_CARRIER_FORBIDDEN");
            string rootSource = File.ReadAllText(rootPath);
            Require(rootSource.IndexOf(
                        "requestedDamage.ToString",
                        StringComparison.Ordinal) < 0,
                "FORMAL_CAUSAL_EXPLANATION_TEXT_FORBIDDEN");
        }

        private static void ValidateRootComponent<T>(
            string path,
            Func<T, bool> validate)
            where T : Component
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                T component = root.GetComponent<T>();
                Require(component != null && validate(component),
                    "FORMAL_PRESENTATION_PREFAB_REFERENCE_INVALID " + path);
                Require(GameObjectUtility
                            .GetMonoBehavioursWithMissingScriptCount(root) == 0,
                    "FORMAL_PRESENTATION_PREFAB_MISSING_SCRIPT " + path);
                Require(!AssetDatabase.GetDependencies(path, true).Any(
                        value => value.EndsWith(
                            ".unity",
                            StringComparison.OrdinalIgnoreCase)),
                    "FORMAL_PRESENTATION_PREFAB_SCENE_DEPENDENCY " + path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RemoveOwnedPresentationChildren(
            Transform enemyInfo,
            Transform feedback)
        {
            foreach (FormalBattlePresentationRoot value in enemyInfo
                         .GetComponentsInChildren<
                             FormalBattlePresentationRoot>(true))
            {
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            }
            foreach (FormalBattleDamageFloatPool value in feedback
                         .GetComponentsInChildren<
                             FormalBattleDamageFloatPool>(true))
            {
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            }
            foreach (FormalBattleCueFxAudioRoot value in feedback
                         .GetComponentsInChildren<
                             FormalBattleCueFxAudioRoot>(true))
            {
                UnityEngine.Object.DestroyImmediate(value.gameObject);
            }
        }

        private static void RetirePlaceholderSurface(Transform slot)
        {
            foreach (Transform child in slot.Cast<Transform>().ToArray())
            {
                child.gameObject.SetActive(false);
            }
            Image image = slot.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = 0f;
                image.color = color;
                image.raycastTarget = false;
            }
            foreach (Outline outline in slot.GetComponents<Outline>())
            {
                outline.enabled = false;
            }
        }

        private static void ConfigureFeedbackExtent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(-1150f, -450f);
            rect.offsetMax = new Vector2(350f, 500f);
        }

        private static Transform RequiredSlot(
            IReadOnlyDictionary<string, Transform> slots,
            string slotName)
        {
            Require(slots != null,
                "FORMAL_PRESENTATION_SLOT_MAP_MISSING");
            Require(slots.TryGetValue(slotName, out Transform value)
                    && value != null,
                "FORMAL_PRESENTATION_REQUIRED_SLOT_MISSING " + slotName);
            return value;
        }

        private static GameObject InstantiateNested(
            GameObject asset,
            Transform parent,
            string instanceName)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(
                asset,
                parent) as GameObject;
            Require(instance != null,
                "FORMAL_PRESENTATION_NESTED_PREFAB_FAILED " + instanceName);
            instance.name = instanceName;
            return instance;
        }

        private static void SaveOwnedPrefab(GameObject root, string path)
        {
            Require(root != null, "FORMAL_PRESENTATION_PREFAB_ROOT_NULL");
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            Require(saved != null,
                "FORMAL_PRESENTATION_PREFAB_SAVE_FAILED " + path);
        }

        private static GameObject RequirePrefab(string path)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Require(asset != null,
                "FORMAL_PRESENTATION_PREFAB_MISSING " + path);
            return asset;
        }

        private static T RequireGuidAsset<T>(string guid)
            where T : UnityEngine.Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null,
                "FORMAL_PRESENTATION_GUID_ASSET_MISSING " + guid);
            return asset;
        }

        private static T RequireAssetAtPath<T>(string path)
            where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Require(asset != null,
                "FORMAL_PRESENTATION_ASSET_MISSING " + path);
            return asset;
        }

        private static void EnsureUiSpriteImport(string path)
        {
            AssetDatabase.ImportAsset(
                path,
                ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path)
                as TextureImporter;
            Require(importer != null,
                "FORMAL_PRESENTATION_UI_SPRITE_IMPORTER_MISSING " + path);

            if (importer.textureType == TextureImporterType.Sprite
                && importer.spriteImportMode == SpriteImportMode.Single
                && importer.alphaIsTransparency
                && !importer.mipmapEnabled)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static void ConfigurePinnedPositiveStatePrefab(
            string prefabPath,
            string stripName,
            string pinnedRootName,
            string iconName,
            string labelName,
            Sprite frameSprite,
            Sprite shieldSprite,
            bool selectedEnemy)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                Transform pinnedRoot = FindNamedTransform(
                    root,
                    pinnedRootName);
                TMP_Text label = FindNamedComponent<TMP_Text>(
                    root,
                    labelName);

                pinnedRoot.SetParent(strip.transform, false);
                Image icon = ConfigurePinnedPositiveStateReadout(
                    pinnedRoot,
                    iconName,
                    label,
                    frameSprite,
                    shieldSprite);
                ArrangePositiveAndNegativeStateRows(strip, pinnedRoot);

                if (selectedEnemy)
                {
                    FormalBattleSelectedEnemyHudView selectedHud = root
                        .GetComponentsInChildren<
                            FormalBattleSelectedEnemyHudView>(true)
                        .Single();
                    selectedHud.AssignShellIconForEditor(
                        pinnedRoot.gameObject,
                        icon,
                        label);
                    EditorUtility.SetDirty(selectedHud);
                    Require(selectedHud.ValidateAuthoredReferences(),
                        "FORMAL_SELECTED_ENEMY_PINNED_STATE_REFERENCE_INVALID");
                }
                else
                {
                    FormalBattlePlayerPresentationView playerView = root
                        .GetComponent<FormalBattlePlayerPresentationView>();
                    Require(playerView != null
                            && playerView.ValidateAuthoredReferences(),
                        "FORMAL_PLAYER_PINNED_STATE_REFERENCE_INVALID");
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Image ConfigurePinnedPositiveStateReadout(
            Transform pinnedRoot,
            string iconName,
            TMP_Text label,
            Sprite frameSprite,
            Sprite shieldSprite)
        {
            Require(pinnedRoot != null
                    && label != null
                    && frameSprite != null
                    && shieldSprite != null,
                "FORMAL_PINNED_POSITIVE_STATE_INPUT_INVALID");

            Image frame = pinnedRoot.GetComponent<Image>();
            if (frame == null)
            {
                frame = pinnedRoot.gameObject.AddComponent<Image>();
            }
            frame.sprite = frameSprite;
            frame.type = Image.Type.Simple;
            frame.preserveAspect = true;
            frame.color = Color.white;
            frame.raycastTarget = false;

            Transform existingIcon = pinnedRoot.Cast<Transform>()
                .SingleOrDefault(value => string.Equals(
                    value.gameObject.name,
                    iconName,
                    StringComparison.Ordinal));
            Image icon;
            if (existingIcon == null)
            {
                icon = CreateImage(
                    iconName,
                    pinnedRoot,
                    Color.white);
            }
            else
            {
                icon = existingIcon.GetComponent<Image>();
                Require(icon != null,
                    "FORMAL_PINNED_POSITIVE_STATE_ICON_MISSING "
                    + iconName);
            }
            icon.sprite = shieldSprite;
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.color = Color.white;
            icon.raycastTarget = false;
            Pin(
                icon.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, 2f),
                new Vector2(26f, 26f));

            label.text = string.Empty;
            label.fontSize = 14f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.BottomRight;
            label.color = new Color(0.82f, 0.94f, 1f, 1f);
            label.raycastTarget = false;
            Pin(
                label.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(7f, -11f),
                new Vector2(34f, 18f));

            pinnedRoot.gameObject.SetActive(false);
            return icon;
        }

        private static void ArrangePositiveAndNegativeStateRows(
            FormalBattleStatusStripView strip,
            Transform pinnedRoot)
        {
            RectTransform stripRect = strip.transform as RectTransform;
            Require(stripRect != null && pinnedRoot != null,
                "FORMAL_STATUS_AREA_RECT_INVALID");

            float width = Mathf.Max(350f, Mathf.Abs(stripRect.sizeDelta.x));
            float height = Mathf.Max(94f, Mathf.Abs(stripRect.sizeDelta.y));
            float rowY = Mathf.Clamp(height * 0.26f, 23f, 43f);
            float markerX = -width * 0.5f + 22f;
            float pinnedX = markerX + 42f;
            float firstDynamicX = pinnedX + 55f;
            const float slotSpacing = 55f;

            RectTransform positiveMarker = FindDirectChild(
                    strip.transform,
                    "PositiveStatusMarker")
                as RectTransform;
            RectTransform negativeMarker = FindDirectChild(
                    strip.transform,
                    "NegativeStatusMarker")
                as RectTransform;
            Require(positiveMarker != null && negativeMarker != null,
                "FORMAL_STATUS_AREA_MARKERS_MISSING");
            Pin(
                positiveMarker,
                new Vector2(0.5f, 0.5f),
                new Vector2(markerX, rowY),
                new Vector2(24f, 34f));
            Pin(
                negativeMarker,
                new Vector2(0.5f, 0.5f),
                new Vector2(markerX, -rowY),
                new Vector2(24f, 34f));
            Pin(
                pinnedRoot as RectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(pinnedX, rowY),
                new Vector2(42f, 42f));

            for (int index = 0; index < 8; index++)
            {
                RectTransform slot = FindDirectChild(
                        strip.transform,
                        "StatusSlot_" + index.ToString("D2"))
                    as RectTransform;
                Require(slot != null,
                    "FORMAL_STATUS_AREA_SLOT_MISSING " + index);
                int rowIndex = index < 4 ? index : index - 4;
                Pin(
                    slot,
                    new Vector2(0.5f, 0.5f),
                    new Vector2(
                        firstDynamicX + rowIndex * slotSpacing,
                        index < 4 ? rowY : -rowY),
                    new Vector2(42f, 42f));
            }
        }

        private static void ValidatePinnedPositiveStateAreas()
        {
            ValidatePinnedPositiveStatePrefab(
                PlayerPrefabPath,
                "PlayerStatusStrip",
                "PlayerGuardReadout",
                "PlayerGuardIcon",
                false);
            ValidatePinnedPositiveStatePrefab(
                PresentationRootPrefabPath,
                "SelectedEnemyStatusStrip",
                "SelectedEnemyShellReadout",
                "SelectedEnemyShellIcon",
                true);

            FormalBattlePresentationProfile profile =
                RequireAssetAtPath<FormalBattlePresentationProfile>(
                    ProfilePath);
            GameObject player = PrefabUtility.LoadPrefabContents(
                PlayerPrefabPath);
            try
            {
                FormalBattlePlayerPresentationView playerView =
                    player.GetComponent<FormalBattlePlayerPresentationView>();
                GameObject guardRoot = FindNamedTransform(
                    player,
                    "PlayerGuardReadout").gameObject;
                TMP_Text guardLabel = FindNamedComponent<TMP_Text>(
                    player,
                    "PlayerGuardLabel");
                Require(playerView.ApplyObservability(
                            new C1FormalRealtimeBattlePlayerSnapshot(
                                100,
                                100,
                                12L),
                            Array.Empty<
                                C1FormalRealtimeBattleStatusSnapshot>(),
                            profile,
                            0L,
                            out _)
                        && guardRoot.activeSelf
                        && string.Equals(
                            guardLabel.text,
                            "12",
                            StringComparison.Ordinal),
                    "FORMAL_PLAYER_GUARD_PINNED_STATE_NOT_VISIBLE");
                Require(playerView.ApplyObservability(
                            new C1FormalRealtimeBattlePlayerSnapshot(
                                100,
                                100,
                                0L),
                            Array.Empty<
                                C1FormalRealtimeBattleStatusSnapshot>(),
                            profile,
                            0L,
                            out _)
                        && !guardRoot.activeSelf
                        && string.IsNullOrEmpty(guardLabel.text),
                    "FORMAL_PLAYER_ZERO_GUARD_NOT_HIDDEN");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(player);
            }

            GameObject presentation = PrefabUtility.LoadPrefabContents(
                PresentationRootPrefabPath);
            try
            {
                FormalBattleSelectedEnemyHudView selectedHud = presentation
                    .GetComponentsInChildren<
                        FormalBattleSelectedEnemyHudView>(true)
                    .Single();
                GameObject shellRoot = FindNamedTransform(
                    presentation,
                    "SelectedEnemyShellReadout").gameObject;
                TMP_Text shellLabel = FindNamedComponent<TMP_Text>(
                    presentation,
                    "SelectedEnemyShellLabel");
                System.Reflection.MethodInfo applyShell = typeof(
                        FormalBattleSelectedEnemyHudView)
                    .GetMethod(
                        "ApplyShell",
                        System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.NonPublic);
                Require(applyShell != null,
                    "FORMAL_SELECTED_ENEMY_APPLY_SHELL_MISSING");
                applyShell.Invoke(
                    selectedHud,
                    new object[]
                    {
                        CreatePinnedStateShellSnapshot(42)
                    });
                Require(shellRoot.activeSelf
                        && string.Equals(
                            shellLabel.text,
                            "42",
                            StringComparison.Ordinal),
                    "FORMAL_SELECTED_ENEMY_SHELL_PINNED_STATE_NOT_VISIBLE");
                applyShell.Invoke(
                    selectedHud,
                    new object[]
                    {
                        CreatePinnedStateShellSnapshot(0)
                    });
                Require(!shellRoot.activeSelf
                        && string.IsNullOrEmpty(shellLabel.text),
                    "FORMAL_SELECTED_ENEMY_ZERO_SHELL_NOT_HIDDEN");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(presentation);
            }
        }

        private static C1FormalRealtimeBattleActorSnapshot
            CreatePinnedStateShellSnapshot(int currentShell)
        {
            return new C1FormalRealtimeBattleActorSnapshot(
                "decor.shell.actor",
                "decor.shell.content",
                "decor.shell.runtime",
                string.Empty,
                0,
                1,
                100,
                100,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                "decor.shell.snapshot",
                true,
                100,
                currentShell,
                currentShell <= 0,
                "enemy.resource.shell",
                "enemy.shell_state.broken",
                "counter_window.shell_break");
        }

        private static void ValidatePinnedPositiveStatePrefab(
            string prefabPath,
            string stripName,
            string pinnedRootName,
            string iconName,
            bool selectedEnemy)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                FormalBattleStatusStripView strip = root
                    .GetComponentsInChildren<FormalBattleStatusStripView>(true)
                    .Single(value => string.Equals(
                        value.gameObject.name,
                        stripName,
                        StringComparison.Ordinal));
                Transform pinnedRoot = FindNamedTransform(
                    root,
                    pinnedRootName);
                Image icon = FindNamedComponent<Image>(root, iconName);
                Require(pinnedRoot.parent == strip.transform
                        && pinnedRoot.GetComponent<Image>() != null
                        && icon.sprite != null
                        && !icon.raycastTarget
                        && !pinnedRoot.gameObject.activeSelf,
                    "FORMAL_PINNED_STATE_STRUCTURE_INVALID "
                    + pinnedRootName);

                for (int index = 0; index < 8; index++)
                {
                    RectTransform slot = FindDirectChild(
                            strip.transform,
                            "StatusSlot_" + index.ToString("D2"))
                        as RectTransform;
                    Require(slot != null
                            && (index < 4
                                ? slot.anchoredPosition.y > 0f
                                : slot.anchoredPosition.y < 0f),
                        "FORMAL_STATUS_AREA_ROW_INVALID " + index);
                }

                Require(selectedEnemy
                        ? root.GetComponentsInChildren<
                                FormalBattleSelectedEnemyHudView>(true)
                            .Single()
                            .ValidateAuthoredReferences()
                        : root.GetComponent<
                                FormalBattlePlayerPresentationView>()
                            .ValidateAuthoredReferences(),
                    "FORMAL_PINNED_STATE_VIEW_REFERENCE_INVALID "
                    + pinnedRootName);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Image ConfigureSelectedEnemyShellIcon(
            Transform shellTransform,
            TMP_Text shellLabel)
        {
            Require(shellTransform != null && shellLabel != null,
                "FORMAL_SELECTED_ENEMY_SHELL_ICON_INPUT_INVALID");

            Image oldFrame = shellTransform.GetComponent<Image>();
            if (oldFrame != null)
            {
                UnityEngine.Object.DestroyImmediate(oldFrame);
            }
            DestroyDirectChildIfPresent(
                shellTransform,
                "SelectedEnemyShellFill");

            Transform existingIcon = shellTransform.Cast<Transform>()
                .SingleOrDefault(value => string.Equals(
                    value.gameObject.name,
                    "SelectedEnemyShellIcon",
                    StringComparison.Ordinal));
            Image shellIcon;
            if (existingIcon == null)
            {
                shellIcon = CreateImage(
                    "SelectedEnemyShellIcon",
                    shellTransform,
                    Color.white);
            }
            else
            {
                shellIcon = existingIcon.GetComponent<Image>();
                Require(shellIcon != null,
                    "FORMAL_SELECTED_ENEMY_SHELL_ICON_IMAGE_MISSING");
            }

            shellIcon.sprite = RequireAssetAtPath<Sprite>(EnemyShellIconPath);
            shellIcon.type = Image.Type.Simple;
            shellIcon.preserveAspect = true;
            shellIcon.raycastTarget = false;
            shellIcon.color = Color.white;
            Pin(
                shellIcon.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(-48f, 0f),
                new Vector2(44f, 44f));

            shellLabel.fontSize = 22f;
            shellLabel.fontStyle = FontStyles.Bold;
            shellLabel.color = new Color(0.78f, 0.9f, 0.96f, 1f);
            Pin(
                shellLabel.rectTransform,
                new Vector2(0.5f, 0.5f),
                new Vector2(30f, 0f),
                new Vector2(108f, 42f));
            shellTransform.gameObject.SetActive(false);
            return shellIcon;
        }

        private static void ValidateImportedAudioClip(
            AudioClip clip,
            string path,
            float minimumSeconds,
            float maximumSeconds)
        {
            Require(clip != null
                    && clip.channels == 1
                    && clip.frequency == 22050
                    && clip.length >= minimumSeconds
                    && clip.length <= maximumSeconds
                    && !clip.ambisonic,
                "FORMAL_PRESENTATION_IMPORTED_AUDIO_INVALID " + path);
        }

        private static T FindNamedComponent<T>(
            GameObject root,
            string objectName)
            where T : Component
        {
            T[] matches = root.GetComponentsInChildren<T>(true)
                .Where(value => string.Equals(
                    value.gameObject.name,
                    objectName,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "FORMAL_PRESENTATION_NAMED_COMPONENT_INVALID "
                + objectName);
            return matches[0];
        }

        private static Transform FindNamedTransform(
            GameObject root,
            string objectName)
        {
            Transform[] matches = root.GetComponentsInChildren<Transform>(true)
                .Where(value => string.Equals(
                    value.gameObject.name,
                    objectName,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "FORMAL_PRESENTATION_NAMED_TRANSFORM_INVALID "
                + objectName);
            return matches[0];
        }

        private static Transform FindDirectChild(
            Transform parent,
            string childName)
        {
            Require(parent != null,
                "FORMAL_PRESENTATION_DIRECT_CHILD_PARENT_INVALID "
                + childName);
            Transform[] matches = parent.Cast<Transform>()
                .Where(value => string.Equals(
                    value.gameObject.name,
                    childName,
                    StringComparison.Ordinal))
                .ToArray();
            Require(matches.Length == 1,
                "FORMAL_PRESENTATION_DIRECT_CHILD_INVALID "
                + childName);
            return matches[0];
        }

        private static void DestroyDirectChildIfPresent(
            Transform parent,
            string childName)
        {
            Transform child = parent.Cast<Transform>().SingleOrDefault(
                value => string.Equals(
                    value.gameObject.name,
                    childName,
                    StringComparison.Ordinal));
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void DestroyNamedDescendantIfPresent(
            GameObject root,
            string objectName)
        {
            Transform match = root.GetComponentsInChildren<Transform>(true)
                .SingleOrDefault(value => value != root.transform
                    && string.Equals(
                        value.gameObject.name,
                        objectName,
                        StringComparison.Ordinal));
            if (match != null)
            {
                UnityEngine.Object.DestroyImmediate(match.gameObject);
            }
        }

        private static GameObject NewUiObject(
            string name,
            Transform parent = null)
        {
            GameObject value = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                value.transform.SetParent(parent, false);
            }
            return value;
        }

        private static Image AddImage(GameObject target, Color color)
        {
            Image image = target.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Image CreateImage(
            string name,
            Transform parent,
            Color color)
        {
            GameObject value = NewUiObject(name, parent);
            return AddImage(value, color);
        }

        private static RawImage CreateRawImage(
            string name,
            Transform parent,
            Texture texture,
            Color color)
        {
            GameObject value = NewUiObject(name, parent);
            RawImage image = value.AddComponent<RawImage>();
            image.texture = texture;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            TMP_FontAsset font,
            string text,
            float fontSize,
            Color color)
        {
            GameObject value = NewUiObject(name, parent);
            TextMeshProUGUI label = value.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text ?? string.Empty;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.enableWordWrapping = false;
            return label;
        }

        private static void AddGoldOutline(GameObject target, float distance)
        {
            Outline outline = target.AddComponent<Outline>();
            outline.effectColor = new Color(0.86f, 0.58f, 0.19f, 0.88f);
            outline.effectDistance = Vector2.one * distance;
            outline.useGraphicAlpha = true;
        }

        private static void ConfigureAudio(AudioSource source)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = 0.72f;
        }

        private static void Pin(
            RectTransform rect,
            Vector2 anchor,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void CopyRectTransform(
            RectTransform source,
            RectTransform target)
        {
            Require(source != null && target != null,
                "FORMAL_PRESENTATION_RECT_COPY_INVALID");
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.localScale = source.localScale;
            target.localRotation = source.localRotation;
        }

        private static void MirrorRectTransformHorizontally(
            RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }
            Vector2 originalMin = rect.anchorMin;
            Vector2 originalMax = rect.anchorMax;
            rect.anchorMin = new Vector2(1f - originalMax.x, originalMin.y);
            rect.anchorMax = new Vector2(1f - originalMin.x, originalMax.y);
            rect.pivot = new Vector2(1f - rect.pivot.x, rect.pivot.y);
            rect.anchoredPosition = new Vector2(
                -rect.anchoredPosition.x,
                rect.anchoredPosition.y);
        }

        private static void Stretch(
            RectTransform rect,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void EnsureFolder(string assetPath)
        {
            string normalized = assetPath.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(normalized))
            {
                return;
            }
            string parent = Path.GetDirectoryName(normalized)
                ?.Replace('\\', '/');
            string leaf = Path.GetFileName(normalized);
            Require(!string.IsNullOrEmpty(parent)
                    && !string.IsNullOrEmpty(leaf),
                "FORMAL_PRESENTATION_FOLDER_PATH_INVALID " + assetPath);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void Require(bool condition, string diagnostic)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    diagnostic ?? "FORMAL_PRESENTATION_ASSERTION_FAILED");
            }
        }
    }
}
