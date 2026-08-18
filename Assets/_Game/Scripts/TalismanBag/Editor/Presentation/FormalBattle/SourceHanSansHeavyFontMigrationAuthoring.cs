using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class SourceHanSansHeavyFontMigrationAuthoring
    {
        private const string SourceFontPath =
            "Assets/_Game/Fonts/SourceHanSansSC-Heavy.otf";
        private const string FontAssetPath =
            "Assets/_Game/Fonts/SourceHanSansSC-Heavy SDF.asset";
        private const string Marker =
            "TALISMANBAG_DECOR_SOURCE_HAN_SANS_HEAVY_FONT_PASS";

        private static readonly string[] PrefabPaths =
        {
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
                + "FormalBattlePresentationRoot.prefab",
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
                + "FormalBattlePlayerPresentation.prefab",
            "Assets/_Game/Prefabs/TalismanBag/BattlePresentation/"
                + "FormalBattleDamageFloatPool.prefab"
        };

        private static readonly string[] ScenePaths =
        {
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity",
            "Assets/_Game/Scenes/"
                + "Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab.unity"
        };

        private const string PresentationProfilePath =
            "Assets/_Game/Resources/V04/FormalBattlePresentation/"
            + "FormalBattlePresentationProfile.asset";

        [MenuItem(
            "TalismanBag/V0.4/Presentation/Apply Source Han Sans Heavy To Current UI")]
        public static void ExecuteFromMenu()
        {
            Execute();
        }

        public static void ExecuteFromCommandLine()
        {
            try
            {
                Execute();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void Execute()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            TMP_FontAsset heavyFont = EnsureHeavyFontAsset();

            int migratedTextCount = 0;
            foreach (string prefabPath in PrefabPaths)
            {
                migratedTextCount += ApplyToPrefab(prefabPath, heavyFont);
            }

            foreach (string scenePath in ScenePaths)
            {
                migratedTextCount += ApplyToScene(scenePath, heavyFont);
            }

            ApplyToPresentationProfile(heavyFont);
            ConfigureTmpDefaultFont(heavyFont);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateResult(heavyFont);
            Debug.Log(Marker + " textCount=" + migratedTextCount);
        }

        private static TMP_FontAsset EnsureHeavyFontAsset()
        {
            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
            if (sourceFont == null)
            {
                throw new InvalidOperationException(
                    "Missing Source Han Sans Heavy font: " + SourceFontPath);
            }

            TMP_FontAsset fontAsset =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (fontAsset == null)
            {
                fontAsset = TMP_FontAsset.CreateFontAsset(
                    sourceFont,
                    90,
                    9,
                    GlyphRenderMode.SDFAA,
                    2048,
                    2048,
                    AtlasPopulationMode.Dynamic,
                    true);
                if (fontAsset == null)
                {
                    throw new InvalidOperationException(
                        "TMP could not create Source Han Sans Heavy font asset.");
                }

                fontAsset.name = "SourceHanSansSC-Heavy SDF";
                fontAsset.atlasTexture.name = "SourceHanSansSC-Heavy Atlas";
                fontAsset.material.name = "SourceHanSansSC-Heavy Material";
                AssetDatabase.CreateAsset(fontAsset, FontAssetPath);
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.isMultiAtlasTexturesEnabled = true;
            AddCurrentUiGlyphs(fontAsset);
            EditorUtility.SetDirty(fontAsset);
            return fontAsset;
        }

        private static void AddCurrentUiGlyphs(TMP_FontAsset fontAsset)
        {
            const string glyphs =
                "明箓符背包山下玩家敌人生生命值护盾念力状态伤害治疗"
                + "点亮冷却触发目标战斗道具稀有度准备胜利奖励继续返回"
                + "攻击防御净化控制层剩余当前选中壳守护行动回合"
                + "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
                + " /+-:%()[]，。！？：；";

            if (!fontAsset.TryAddCharacters(glyphs, out string missing)
                && !string.IsNullOrEmpty(missing))
            {
                throw new InvalidOperationException(
                    "Source Han Sans Heavy is missing current UI glyphs: "
                    + missing);
            }
        }

        private static int ApplyToPrefab(
            string prefabPath,
            TMP_FontAsset heavyFont)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                int count = ApplyToHierarchy(root, heavyFont);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                return count;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static int ApplyToScene(
            string scenePath,
            TMP_FontAsset heavyFont)
        {
            Scene scene = EditorSceneManager.OpenScene(
                scenePath,
                OpenSceneMode.Additive);
            try
            {
                int count = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    count += ApplyToHierarchy(root, heavyFont);
                }

                EditorSceneManager.SaveScene(scene);
                return count;
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static int ApplyToHierarchy(
            GameObject root,
            TMP_FontAsset heavyFont)
        {
            int count = 0;
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.font == heavyFont
                    && text.fontSharedMaterial == heavyFont.material)
                {
                    continue;
                }

                text.font = heavyFont;
                text.fontSharedMaterial = heavyFont.material;
                EditorUtility.SetDirty(text);
                count++;
            }

            return count;
        }

        private static void ApplyToPresentationProfile(TMP_FontAsset heavyFont)
        {
            UnityEngine.Object profile =
                AssetDatabase.LoadMainAssetAtPath(PresentationProfilePath);
            if (profile == null)
            {
                throw new InvalidOperationException(
                    "Missing formal presentation profile: "
                    + PresentationProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty damageFont =
                serializedProfile.FindProperty("damageFont");
            if (damageFont == null)
            {
                throw new InvalidOperationException(
                    "Formal presentation profile has no damageFont field.");
            }

            damageFont.objectReferenceValue = heavyFont;
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void ConfigureTmpDefaultFont(TMP_FontAsset heavyFont)
        {
            TMP_Settings settings = TMP_Settings.instance;
            if (settings == null)
            {
                throw new InvalidOperationException(
                    "TextMesh Pro settings are missing.");
            }

            SerializedObject serializedSettings = new SerializedObject(settings);
            SerializedProperty defaultFont =
                serializedSettings.FindProperty("m_defaultFontAsset");
            if (defaultFont == null)
            {
                throw new InvalidOperationException(
                    "TMP Settings has no default font field.");
            }

            defaultFont.objectReferenceValue = heavyFont;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        private static void ValidateResult(TMP_FontAsset heavyFont)
        {
            if (heavyFont == null
                || heavyFont.material == null
                || heavyFont.atlasTexture == null
                || !heavyFont.HasCharacters("明箓符玩家敌人念力护盾"))
            {
                throw new InvalidOperationException(
                    "Source Han Sans Heavy font validation failed.");
            }

            TMP_Settings settings = TMP_Settings.instance;
            SerializedObject serializedSettings = new SerializedObject(settings);
            SerializedProperty defaultFont =
                serializedSettings.FindProperty("m_defaultFontAsset");
            if (defaultFont == null
                || defaultFont.objectReferenceValue != heavyFont)
            {
                throw new InvalidOperationException(
                    "Source Han Sans Heavy is not the TMP default font.");
            }

            UnityEngine.Object profile =
                AssetDatabase.LoadMainAssetAtPath(PresentationProfilePath);
            SerializedObject serializedProfile = new SerializedObject(profile);
            SerializedProperty damageFont =
                serializedProfile.FindProperty("damageFont");
            if (damageFont == null
                || damageFont.objectReferenceValue != heavyFont)
            {
                throw new InvalidOperationException(
                    "Formal damage font was not migrated to Source Han Sans Heavy.");
            }
        }
    }
}
