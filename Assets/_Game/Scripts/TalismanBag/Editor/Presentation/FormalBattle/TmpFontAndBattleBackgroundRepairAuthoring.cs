using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace TalismanBag.Editor.Presentation.FormalBattle
{
    public static class TmpFontAndBattleBackgroundRepairAuthoring
    {
        public const string TerminalMarker =
            "TALISMANBAG_DECOR_FONT_AND_BACKGROUND_REPAIR_PASS";

        private const string NotoFontSourcePath =
            "Assets/_Game/Fonts/NotoSerifSC-VF.ttf";
        private const string NotoFontAssetPath =
            "Assets/_Game/Fonts/NotoSerifSC-VF SDF.asset";
        private const string MainChineseFontAssetPath =
            "Assets/TextMesh Pro/Fonts/MFLangSongJianYuan-Regular SDF.asset";
        private const string BackgroundPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/"
            + "C1ExactBattleSandboxGameBackground.prefab";
        private const string GradientSpritePath =
            "Assets/_Game/Resources/V03/Battle/black.png";

        [MenuItem(
            "TalismanBag/V0.4/Presentation/Repair Chinese Fonts And Battle Background")]
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

        public static void ExecuteFontRepairFromCommandLine()
        {
            try
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                TMP_FontAsset notoFallback = ExecuteFontRepair();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ValidateFontResult(notoFallback);
                Debug.Log("TALISMANBAG_DECOR_TRANSPARENT_FONT_REPAIR_PASS");
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

            TMP_FontAsset notoFallback = ExecuteFontRepair();
            RestoreGradientMasks();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateResult(notoFallback);
            Debug.Log(TerminalMarker);
        }

        private static TMP_FontAsset ExecuteFontRepair()
        {
            TMP_FontAsset notoFallback = EnsureNotoFallbackFontAsset();
            RestorePrimaryFontToTransparentFallbackMode();
            RepairDynamicFontAtlasReadability();
            ConfigureGlobalFallback(notoFallback);
            PrewarmPlayerFacingGlyphs(notoFallback);
            return notoFallback;
        }

        private static TMP_FontAsset EnsureNotoFallbackFontAsset()
        {
            TMP_FontAsset existing =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NotoFontAssetPath);
            if (existing != null)
            {
                existing.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                existing.isMultiAtlasTexturesEnabled = true;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Font source = AssetDatabase.LoadAssetAtPath<Font>(NotoFontSourcePath);
            if (source == null)
            {
                throw new InvalidOperationException(
                    "Missing authored Chinese fallback source font: "
                    + NotoFontSourcePath);
            }

            TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(
                source,
                90,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);
            if (created == null)
            {
                throw new InvalidOperationException(
                    "TMP could not create the Noto Serif SC fallback asset.");
            }

            created.name = "NotoSerifSC-VF SDF";
            created.isMultiAtlasTexturesEnabled = true;
            AssetDatabase.CreateAsset(created, NotoFontAssetPath);
            AssetDatabase.AddObjectToAsset(created.atlasTexture, created);
            AssetDatabase.AddObjectToAsset(created.material, created);
            EditorUtility.SetDirty(created);
            return created;
        }

        private static void RepairDynamicFontAtlasReadability()
        {
            string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/", StringComparison.Ordinal))
                {
                    continue;
                }

                TMP_FontAsset fontAsset =
                    AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (fontAsset == null
                    || fontAsset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
                {
                    continue;
                }

                fontAsset.isMultiAtlasTexturesEnabled = true;
                EnsureCurrentAtlasTexture(fontAsset);
                Texture2D[] atlasTextures = fontAsset.atlasTextures;
                if (atlasTextures != null)
                {
                    foreach (Texture2D atlasTexture in atlasTextures)
                    {
                        if (atlasTexture != null && !atlasTexture.isReadable)
                        {
                            SerializedObject serializedTexture =
                                new SerializedObject(atlasTexture);
                            SerializedProperty readable =
                                serializedTexture.FindProperty("m_IsReadable");
                            if (readable == null)
                            {
                                throw new InvalidOperationException(
                                    "Cannot repair TMP atlas readability for "
                                    + fontAsset.name);
                            }

                            readable.boolValue = true;
                            serializedTexture.ApplyModifiedPropertiesWithoutUndo();
                            EditorUtility.SetDirty(atlasTexture);
                        }
                    }
                }

                EditorUtility.SetDirty(fontAsset);
            }
        }

        private static void RestorePrimaryFontToTransparentFallbackMode()
        {
            TMP_FontAsset primary =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    MainChineseFontAssetPath);
            if (primary == null)
            {
                throw new InvalidOperationException(
                    "Missing primary Chinese TMP font asset: "
                    + MainChineseFontAssetPath);
            }

            primary.ReadFontAssetDefinition();
            HashSet<uint> removedGlyphIndices = new HashSet<uint>();
            List<Glyph> glyphTable = primary.glyphTable;
            for (int index = glyphTable.Count - 1; index >= 0; index--)
            {
                Glyph glyph = glyphTable[index];
                if (glyph != null && glyph.atlasIndex > 0)
                {
                    removedGlyphIndices.Add(glyph.index);
                    glyphTable.RemoveAt(index);
                }
            }

            List<TMP_Character> characterTable = primary.characterTable;
            for (int index = characterTable.Count - 1; index >= 0; index--)
            {
                TMP_Character character = characterTable[index];
                bool usesRemovedAtlas = character != null
                    && ((character.glyph != null && character.glyph.atlasIndex > 0)
                        || removedGlyphIndices.Contains(character.glyphIndex));
                if (usesRemovedAtlas)
                {
                    characterTable.RemoveAt(index);
                }
            }

            Texture2D[] atlasTextures = primary.atlasTextures;
            if (atlasTextures == null
                || atlasTextures.Length == 0
                || atlasTextures[0] == null)
            {
                throw new InvalidOperationException(
                    "Primary Chinese TMP font has no valid original Atlas 0.");
            }

            primary.atlasTextures = new[] { atlasTextures[0] };
            primary.atlasPopulationMode = AtlasPopulationMode.Static;
            primary.isMultiAtlasTexturesEnabled = false;

            SerializedObject serializedFont = new SerializedObject(primary);
            SerializedProperty atlasIndex =
                serializedFont.FindProperty("m_AtlasTextureIndex");
            SerializedProperty usedRects =
                serializedFont.FindProperty("m_UsedGlyphRects");
            SerializedProperty freeRects =
                serializedFont.FindProperty("m_FreeGlyphRects");
            if (atlasIndex == null || usedRects == null || freeRects == null)
            {
                throw new InvalidOperationException(
                    "Cannot normalize the primary Chinese TMP atlas state.");
            }

            atlasIndex.intValue = 0;
            usedRects.ClearArray();
            freeRects.ClearArray();
            serializedFont.ApplyModifiedPropertiesWithoutUndo();
            primary.ReadFontAssetDefinition();
            EditorUtility.SetDirty(primary);
        }

        private static void EnsureCurrentAtlasTexture(TMP_FontAsset fontAsset)
        {
            SerializedObject serializedFont = new SerializedObject(fontAsset);
            SerializedProperty atlasIndexProperty =
                serializedFont.FindProperty("m_AtlasTextureIndex");
            if (atlasIndexProperty == null)
            {
                throw new InvalidOperationException(
                    "Cannot inspect TMP atlas index for " + fontAsset.name);
            }

            int atlasIndex = Mathf.Max(0, atlasIndexProperty.intValue);
            Texture2D[] atlasTextures = fontAsset.atlasTextures
                ?? Array.Empty<Texture2D>();
            if (atlasTextures.Length <= atlasIndex)
            {
                Array.Resize(ref atlasTextures, atlasIndex + 1);
            }

            if (atlasTextures[atlasIndex] != null)
            {
                if (fontAsset.atlasTexture == null)
                {
                    throw new InvalidOperationException(
                        "TMP base atlas is missing for " + fontAsset.name);
                }
                return;
            }

            Texture2D atlasTexture = new Texture2D(
                fontAsset.atlasWidth,
                fontAsset.atlasHeight,
                TextureFormat.Alpha8,
                false);
            atlasTexture.name = fontAsset.name + " Atlas " + atlasIndex;
            atlasTexture.LoadRawTextureData(
                new byte[fontAsset.atlasWidth * fontAsset.atlasHeight]);
            atlasTexture.Apply(false, false);
            AssetDatabase.AddObjectToAsset(atlasTexture, fontAsset);
            atlasTextures[atlasIndex] = atlasTexture;
            fontAsset.atlasTextures = atlasTextures;
            if (fontAsset.atlasTexture == null)
            {
                throw new InvalidOperationException(
                    "TMP base atlas is missing for " + fontAsset.name);
            }
            EditorUtility.SetDirty(atlasTexture);
            EditorUtility.SetDirty(fontAsset);
        }

        private static void ConfigureGlobalFallback(TMP_FontAsset notoFallback)
        {
            TMP_Settings settings = TMP_Settings.instance;
            if (settings == null)
            {
                throw new InvalidOperationException(
                    "TextMesh Pro settings are not installed in this project.");
            }

            List<TMP_FontAsset> fallbackFonts = TMP_Settings.fallbackFontAssets;
            fallbackFonts.RemoveAll(font => font == null || font == notoFallback);
            fallbackFonts.Add(notoFallback);
            EditorUtility.SetDirty(settings);
        }

        private static void PrewarmPlayerFacingGlyphs(TMP_FontAsset notoFallback)
        {
            AddCharactersOrThrow(
                notoFallback,
                "明箓符背包山下玩家敌人生生命护盾念力状态修伤害治疗点亮冷却触发目标战斗道具稀有度");
            EditorUtility.SetDirty(notoFallback);
        }

        private static void AddCharactersOrThrow(
            TMP_FontAsset fontAsset,
            string characters)
        {
            List<char> required = new List<char>();
            HashSet<char> seen = new HashSet<char>();
            foreach (char character in characters)
            {
                if (seen.Add(character) && !fontAsset.HasCharacter(character))
                {
                    required.Add(character);
                }
            }

            if (required.Count == 0)
            {
                return;
            }

            string requiredText = new string(required.ToArray());
            if (!fontAsset.TryAddCharacters(requiredText, out string missing)
                && !string.IsNullOrEmpty(missing))
            {
                throw new InvalidOperationException(
                    fontAsset.name + " cannot author glyphs: " + missing);
            }
        }

        private static void RestoreGradientMasks()
        {
            Sprite gradientSprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(GradientSpritePath);
            if (gradientSprite == null)
            {
                throw new InvalidOperationException(
                    "Missing original BattleSandbox gradient sprite: "
                    + GradientSpritePath);
            }

            GameObject root = PrefabUtility.LoadPrefabContents(BackgroundPrefabPath);
            try
            {
                RectTransform rootRect = root.GetComponent<RectTransform>();
                if (rootRect == null)
                {
                    throw new InvalidOperationException(
                        "Battle background Prefab root must be a RectTransform.");
                }

                ConfigureGradientMask(
                    rootRect,
                    "TopGradientMask",
                    gradientSprite,
                    true);
                ConfigureGradientMask(
                    rootRect,
                    "BottomGradientMask",
                    gradientSprite,
                    false);

                PrefabUtility.SaveAsPrefabAsset(root, BackgroundPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigureGradientMask(
            RectTransform parent,
            string objectName,
            Sprite gradientSprite,
            bool isTop)
        {
            Transform existing = parent.Find(objectName);
            GameObject gameObject;
            if (existing == null)
            {
                gameObject = new GameObject(
                    objectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                gameObject.transform.SetParent(parent, false);
            }
            else
            {
                gameObject = existing.gameObject;
            }

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            Image image = gameObject.GetComponent<Image>();
            if (rect == null || image == null)
            {
                throw new InvalidOperationException(
                    objectName + " must contain RectTransform and Image.");
            }

            rect.anchorMin = isTop ? new Vector2(0f, 1f) : new Vector2(0f, 0f);
            rect.anchorMax = isTop ? new Vector2(1f, 1f) : new Vector2(1f, 0f);
            rect.pivot = isTop ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 400f);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.Euler(isTop ? 180f : 0f, 0f, 0f);

            image.sprite = gradientSprite;
            image.color = Color.black;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
            image.maskable = true;
            gameObject.SetActive(true);
            rect.SetAsLastSibling();
            EditorUtility.SetDirty(gameObject);
        }

        private static void ValidateResult(TMP_FontAsset notoFallback)
        {
            ValidateFontResult(notoFallback);

            GameObject background =
                AssetDatabase.LoadAssetAtPath<GameObject>(BackgroundPrefabPath);
            ValidateGradientMask(background, "TopGradientMask", true);
            ValidateGradientMask(background, "BottomGradientMask", false);
        }

        private static void ValidateFontResult(TMP_FontAsset notoFallback)
        {
            if (!TMP_Settings.fallbackFontAssets.Contains(notoFallback))
            {
                throw new InvalidOperationException(
                    "Noto Serif SC was not saved as a global TMP fallback.");
            }

            if (!notoFallback.HasCharacter('明'))
            {
                throw new InvalidOperationException(
                    "Global Chinese fallback still cannot render 明.");
            }

            if (!notoFallback.HasCharacter('箓'))
            {
                throw new InvalidOperationException(
                    "Global Chinese fallback still cannot render 箓.");
            }

            ValidatePrimaryFontUsesOnlyTransparentAtlasZero();
        }

        private static void ValidatePrimaryFontUsesOnlyTransparentAtlasZero()
        {
            TMP_FontAsset primary =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    MainChineseFontAssetPath);
            if (primary == null
                || primary.atlasPopulationMode != AtlasPopulationMode.Static
                || primary.atlasTextures == null
                || primary.atlasTextures.Length != 1
                || primary.atlasTextures[0] == null)
            {
                throw new InvalidOperationException(
                    "Primary Chinese TMP font still references a generated atlas.");
            }

            foreach (Glyph glyph in primary.glyphTable)
            {
                if (glyph != null && glyph.atlasIndex != 0)
                {
                    throw new InvalidOperationException(
                        "Primary Chinese TMP font still contains a colored-quad glyph.");
                }
            }
        }

        private static void ValidateGradientMask(
            GameObject background,
            string objectName,
            bool isTop)
        {
            if (background == null)
            {
                throw new InvalidOperationException(
                    "Battle background Prefab cannot be loaded after authoring.");
            }

            Transform child = background.transform.Find(objectName);
            Image image = child == null ? null : child.GetComponent<Image>();
            if (child == null
                || image == null
                || image.sprite == null
                || image.raycastTarget
                || Quaternion.Angle(
                    child.localRotation,
                    Quaternion.Euler(isTop ? 180f : 0f, 0f, 0f)) > 0.1f)
            {
                throw new InvalidOperationException(
                    objectName + " authored validation failed.");
            }
        }
    }
}
